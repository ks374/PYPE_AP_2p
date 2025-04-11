using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;


namespace WinfordEthIO
{
	#region Enums
	public enum Eth32EventType
	{
		Digital=0,
		Analog=1,
		CounterRollover=2,
		CounterThreshold=3,
		Heartbeat=4
	}
	
	public enum Eth32AnalogChannel
	{
		SE0=0x00, // Single Ended channels 0-7
		SE1=0x01,
		SE2=0x02,
		SE3=0x03,
		SE4=0x04,
		SE5=0x05,
		SE6=0x06,
		SE7=0x07,

		DI00X10 = 0x08,       // Difference of channel 0 and 0 (for calibration), with 10X gain
		DI10X10 = 0x09,       // Difference of channel 1 and 0, with 10X gain
		DI00X200 = 0x0A,
		DI10X200 = 0x0B,
		DI22X10 = 0x0C,
		DI32X10 = 0x0D,
		DI22X200 = 0x0E,
		DI32X200 = 0x0F,
		DI01X1 = 0x10,
		DI11X1 = 0x11,
		DI21X1 = 0x12,
		DI31X1 = 0x13,
		DI41X1 = 0x14,
		DI51X1 = 0x15,
		DI61X1 = 0x16,
		DI71X1 = 0x17,
		DI02X1 = 0x18,
		DI12X1 = 0x19,
		DI22X1 = 0x1A,
		DI32X1 = 0x1B,
		DI42X1 = 0x1C,
		DI52X1 = 0x1D,
		R122V = 0x1E,            // 1.22V
		R0V = 0x1F              // 0V
	}

	public enum Eth32CounterState
	{
		Disabled=0,
		Falling=1,
		Rising=2
	}

	public enum Eth32AnalogState
	{
		Disabled=0,
		Enabled=1
	}

	public enum Eth32AnalogReference
	{
		External=0,
		Internal=1,
		Internal256=3
	}

	public enum Eth32PulseEdge
	{
		Falling=0,
		Rising=1
	}

	public enum Eth32AnalogEvtDef
	{
		Low=0,
		High=1
	}

	public enum Eth32PwmClock
	{
		Disabled=0,
		Enabled=1
	}

	public enum Eth32PwmChannel
	{
		Disabled=0,
		Normal=1,
		Inverted=2
	}

	[Flags]
	public enum Eth32ConnectionFlag
	{
		None=0,
		Response=1,
		DigitalEvent=2,
		AnalogEvent=4,
		CounterEvent=8
	}

	public enum Eth32QueueMode
	{
		DiscardNew=0,
		DiscardOld=1
	}

	#endregion

	public class Eth32 : IDisposable 
	{
		#region Private / Internal members and constants
		private IntPtr handle;
        private int firmware_major; // We cache the firmware version at Connect for compatibility checks
        private int firmware_minor;
        private bool serial_have; // We cache the serial number the first time either piece is requested.
        private int serial_batch;
        private int serial_unit;
		internal bool closing; // Indicates whether we're trying to Disconnect.  This is currently
		                       // used in the event_thread_func to avoid a potential deadlock during shutdown
		private ReaderWriterLock rwlock; // Used to protect against potential conflicts in case 
		                                 // a Connect/Disconnect call comes in at the same time 
		                                 // another thread is trying to do a normal operation, 
		                                 // or other multithreading scenarios.  It still allows 
		                                 // multiple threads to use the object at the same time.
		private Mutex event_queue_config_lock; // Since configuring the event queue requires a read 
		                                       // and writing back, we need to lock the process.
		private Thread event_thread_ref;
		private event EventHandler hardware_event; // member to allow firing events

		private const int EthErrorNetClass=-1000; // Error codes <= this constant originate from within this assembly, not the API.
		
		// Codes to indicate event handler mechanism to the API
		private const int HANDLER_NONE=0;
		private const int HANDLER_CALLBACK=1;
		private const int HANDLER_MESSAGE=2;
		// Event queue configuration constants
		private const int QUEUE_DISCARD_NEW=0;
		private const int QUEUE_DISCARD_OLD=1;
		#endregion

		#region Public Constants
		public const int DefaultPort=7152;
		public const int DirInput=0;
		public const int DirOutput=255;
		#endregion

		#region Indexer member variables
		public readonly LedIndexer Led;
		public readonly AnalogAssignmentIndexer AnalogAssignment;
		public readonly CounterStateIndexer CounterState;
		public readonly CounterValueIndexer CounterValue;
		public readonly CounterRolloverIndexer CounterRollover;
		public readonly CounterThresholdIndexer CounterThreshold;
		public readonly PwmChannelIndexer PwmChannel;
		public readonly PwmDutyPeriodIndexer PwmDutyPeriod;
		#endregion


		#region Private / Internal functions used by the class or by other support classes in this assembly

		private void handle_error(int ecode)
		{
			// Raise an error (throw an exception)
			Eth32Exception ex = new Eth32Exception(ErrorString((EthError)ecode), (EthError)ecode);
			ex.Source="eth32";

			throw ex;
		}

		private void precheck()
		{
			EthError ecode;

			if(Connected==false)
				ecode=EthError.NotConnected;
			else
				ecode=EthError.None;

			if(ecode!=EthError.None)
				handle_error((int)ecode);
		}

		internal IntPtr get_handle()
		{
			return(handle);
		}
		internal Delegate[] get_delegates()
		{
			if(hardware_event==null)
				return(new Delegate[]{});
			return(hardware_event.GetInvocationList());
		}

        internal void get_serial()
        {
            int result;
            int temp_batch;
            int temp_unit;
            rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
            try
            {
                precheck();

                result = import.eth32_get_serialnum(handle, out temp_batch, out temp_unit);
                if (result != 0)
                {
                    handle_error(result);
                }
                serial_batch = temp_batch;
                serial_unit = temp_unit;
                serial_have = true;
            }
            finally
            {
                rwlock.ReleaseReaderLock();
            }

        }

		internal bool get_reader_lock(int timeout)
		{
			// Attempt to get the reader lock.  Returns false if it failed
			// to do so (for example, timed out)
			
			try
			{
				rwlock.AcquireReaderLock(timeout);
			}
			catch
			{
				return(false);
			}
			return(true);
		}

		internal void release_reader_lock()
		{
			rwlock.ReleaseReaderLock();
		}

		#endregion

		public Eth32()
		{
			handle=IntPtr.Zero;
            firmware_major = 0;
            firmware_minor = 0;
			closing=false;

			rwlock = new ReaderWriterLock();
			event_queue_config_lock = new Mutex();


			// Make new objects of any indexer helper classes
			Led=new LedIndexer(this);
			AnalogAssignment=new AnalogAssignmentIndexer(this);
			CounterState=new CounterStateIndexer(this);
			CounterValue=new CounterValueIndexer(this);
			CounterRollover=new CounterRolloverIndexer(this);
			CounterThreshold=new CounterThresholdIndexer(this);
			PwmChannel=new PwmChannelIndexer(this);
			PwmDutyPeriod=new PwmDutyPeriodIndexer(this);
		}

		protected virtual void Cleanup()
		{
			// Common cleanup routine called by destructor or Dispose


			// Perform any cleanup necessary when this object is being destroyed
			if(Connected)
			{
				Disconnect();
			}
		}

		public void Dispose()
		{
			Cleanup();
		}

		~Eth32()
		{
			try
			{
				Cleanup();
			}
			catch(ObjectDisposedException ex)
			{
				// When the application is shutting down, there is a distinct possibility that
				// the DllImport functions will be garbage collected before instances of the 
				// Eth32 class.  In this case, when the objects try to close things out by 
				// calling DLL functions, it causes an exception.  Since the application is
				// closing down anyways, we'll catch the exception here and just stay quiet.
				string junk;
				junk=ex.Source; // simply eliminate the compiler warning about not using ex
			}
		}

		// Event accessor to allow client to add and remove delegates from receiving events
		public event EventHandler HardwareEvent
		{
			add
			{
				hardware_event+=value;
			}
			remove
			{
				hardware_event-=value;
			}
		}


		public void Connect(string address, int port, int timeout)
		{
			IntPtr h;
			int result;

			rwlock.AcquireWriterLock(System.Threading.Timeout.Infinite);
			try
			{

				if(Connected)
				{
					handle_error((int)EthError.AlreadyConnected);
					return;
				}

				h=import.eth32_open(address, (ushort)port, (uint)timeout, out result);
			
				if(h==IntPtr.Zero)
				{
					handle_error(result);
					return;
				}
			
				// We are now connected, so set the handle member
				handle=h;

				// At this point, everything is up and running besides our event handling.
				// The property code for EventQueueLimit handles creating and destroying our own
				// thread for helping with events, so call that to set up the event handling.
				try
				{
					// Pass in false for whether to lock the main lock, since we already have 
					// it locked as a WRITER at this point.
					set_event_queue_limit(1000, false);

                    // cache the firmware version of the connected device
                    // for any functionality that depends on a certain version.
                    firmware_major = FirmwareMajor;
                    firmware_minor = FirmwareMinor;
				}
				catch
				{
					// If there is any problem, re-throw the error, but remain connected
					throw;
				}
			}
			finally
			{
				rwlock.ReleaseWriterLock();
			}
		}
		public void Connect(string address, int port)
		{
			Connect(address, port, 0);
		}
		public void Connect(string address)
		{
			Connect(address, DefaultPort, 0);
		}

		public int Timeout
		{
			get
			{
				int result;
				uint val;

				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result=import.eth32_get_timeout(handle, out val);
                
					if(result!=0)
						handle_error(result);

					return((int)val);
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
			set
			{
				int result;

				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result=import.eth32_set_timeout(handle, (uint)value);

					if(result!=0)
						handle_error(result);
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
		}

		public bool Connected
		{
			get
			{
				// Do not implement the rwlock here.
				return(handle!=IntPtr.Zero);
			}
		}

		public void VerifyConnection()
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_verify_connection(handle);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}

		}
		public void OutputByte(int port, int val)
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_output_byte(handle, port, val);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}

		}
		public void OutputBit(int port, int bit, int val)
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_output_bit(handle, port, bit, val);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}

		}
        public void OutputBit(int port, int bit, bool val)
        {
            OutputBit(port, bit, (val) ? 1 : 0);
        }

		public void PulseBit(int port, int bit, Eth32PulseEdge edge, int count)
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_pulse_bit(handle, port, bit, (int)edge, count);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}

		}
		public int InputByte(int port)
		{
			int result;
			int val;
			
			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_input_byte(handle, port, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return(val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}

		}
        public byte InputByteByte(int port)
        {
            return ((byte)InputByte(port));
        }
		public int InputSuccessive(int port, int maxcount, out int status)
		{
			int result;
			int val;
			
			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_input_successive(handle, port, maxcount, out val, out status);
				if(result!=0)
				{
					handle_error(result);
				}
				return(val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}
        public byte InputSuccessiveByte(int port, int maxcount, out int status)
        {
            return ((byte)InputSuccessive(port, maxcount, out status));
        }
		public int InputBit(int port, int bit)
		{
			int result;
			int val;
			
			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_input_bit(handle, port, bit, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return(val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}

		}
        public bool InputBitBool(int port, int bit)
        {
            return (InputBit(port, bit) != 0);
        }
		public int Readback(int port)
		{
			int result;
			int val;
			
			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_readback(handle, port, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return(val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}

		}
        public byte ReadbackByte(int port)
        {
            return ((byte)Readback(port));
        }
		public void SetDirection(int port, int direction)
		{
			int result;
			
			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_set_direction(handle, port, direction);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}

		}
		public int GetDirection(int port)
		{
			int result;
			int val;
			
			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_get_direction(handle, port, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return(val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}

		}
        public byte GetDirectionByte(int port)
        {
            return((byte)GetDirection(port));
        }
		public void SetDirectionBit(int port, int bit, int direction)
		{
			int result;
			
			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_set_direction_bit(handle, port, bit, direction);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}

		}
        public void SetDirectionBit(int port, int bit, bool direction)
        {
            SetDirectionBit(port, bit, (direction) ? 1 : 0);
        }
		public int GetDirectionBit(int port, int bit)
		{
			int result;
			int val;
			
			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_get_direction_bit(handle, port, bit, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return(val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}

		}
        public bool GetDirectionBitBool(int port, int bit)
        {
            return(GetDirectionBit(port, bit)!=0);
        }
		public Eth32AnalogState AnalogState
		{
			get
			{
				int result;
				int val;
			
				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result = import.eth32_get_analog_state(handle, out val);
					if(result!=0)
					{
						handle_error(result);
					}
					return((Eth32AnalogState)val);
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
			set
			{
				int result;
			
				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result = import.eth32_set_analog_state(handle, (int)value);
					if(result!=0)
					{
						handle_error(result);
					}
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
		}
		public Eth32AnalogReference AnalogReference
		{
			get
			{
				int result;
				int val;
			
				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result = import.eth32_get_analog_reference(handle, out val);
					if(result!=0)
					{
						handle_error(result);
					}
					return((Eth32AnalogReference)val);
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
			set
			{
				int result;
			
				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result = import.eth32_set_analog_reference(handle, (int)value);
					if(result!=0)
					{
						handle_error(result);
					}
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
		}
		public int InputAnalog(int channel)
		{
			int result;
			int val;
			
			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_input_analog(handle, channel, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return(val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}
        public ushort InputAnalogUShort(int channel)
        {
            return((ushort)InputAnalog(channel));
        }
		public void SetAnalogEventDef(int bank, int channel, int lomark, int himark, Eth32AnalogEvtDef defaultval)
		{
			int result;
			
			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_set_analog_eventdef(handle, bank, channel, lomark, himark, (int)defaultval);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}
		public void GetAnalogEventDef(int bank, int channel, out int lomark, out int himark)
		{
			int result;
			
			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_get_analog_eventdef(handle, bank, channel, out lomark, out himark);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}
		public void ResetDevice()
		{
			int result;
			
			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_reset(handle);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}
        
        public int SerialBatch
        {
            get
            {
                if (serial_have == false)
                    get_serial();
                return (serial_batch);
            }
        }
        public int SerialUnit
        {
            get
            {
                if (serial_have == false)
                    get_serial();
                return (serial_unit);
            }
        }
        public string SerialNum
		{
			get
			{
				int result;
				StringBuilder sb=new StringBuilder(100);
                
				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result=import.eth32_get_serialnum_string(handle, sb, sb.Capacity);
					if(result!=0)
					{
						handle_error(result);
					}

					return(sb.ToString());
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
		}
		public int ProductID
		{
			get
			{
				int result;
				int val;

				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result = import.eth32_get_product_id(handle, out val);
					if(result!=0)
					{
						handle_error(result);
					}
					return(val);
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
		}
		public int FirmwareMajor
		{
			get
			{
				int result;
				int major;
				int minor;

				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result = import.eth32_get_firmware_release(handle, out major, out minor);
					if(result!=0)
					{
						handle_error(result);
					}
					return(major);
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
		}
		public int FirmwareMinor
		{
			get
			{
				int result;
				int major;
				int minor;

				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result = import.eth32_get_firmware_release(handle, out major, out minor);
					if(result!=0)
					{
						handle_error(result);
					}
					return(minor);
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}

			}
		}
		public Eth32ConnectionFlag ConnectionFlags(int reset)
		{
			int result;
			int val;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_connection_flags(handle, reset, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return((Eth32ConnectionFlag)val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}
		public int EventQueueLimit
		{
			get
			{
				return(get_event_queue_limit());
			}
			set
			{
				set_event_queue_limit(value, true);
			}
		}
		public Eth32QueueMode EventQueueMode
		{
			get
			{
				int result;
				int maxsize;
				int fullqueue;
				int cursize;

				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result = import.eth32_get_event_queue_status(handle, out maxsize, out fullqueue, out cursize);
					if(result!=0)
					{
						handle_error(result);
					}
					return((Eth32QueueMode)fullqueue);
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
			set
			{
				int result;
				int maxsize;
				int fullqueue;
				int cursize;

				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					event_queue_config_lock.WaitOne();
					try
					{
						result = import.eth32_get_event_queue_status(handle, out maxsize, out fullqueue, out cursize);
						if(result!=0)
						{
							handle_error(result);
						}

						result=import.eth32_set_event_queue_config(handle, maxsize, (int)value);
						if(result!=0)
						{
							handle_error(result);
						}
					}
					finally
					{
						event_queue_config_lock.ReleaseMutex();
					}
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
		}
		public int EventQueueCurrentSize
		{
			get
			{
				int result;
				int maxsize;
				int fullqueue;
				int cursize;

				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result = import.eth32_get_event_queue_status(handle, out maxsize, out fullqueue, out cursize);
					if(result!=0)
					{
						handle_error(result);
					}
					return(cursize);
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
		}
		public void EmptyEventQueue()
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_empty_event_queue(handle);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}
		public void EnableEvent(Eth32EventType eventtype, int port, int bit, int id)
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_enable_event(handle, (int)eventtype, port, bit, id);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}
		public void DisableEvent(Eth32EventType eventtype, int port, int bit)
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_disable_event(handle, (int)eventtype, port, bit);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}
		public Eth32PwmClock PwmClockState
		{
			get
			{
				int result;
				int val;

				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result=import.eth32_get_pwm_clock_state(handle, out val);
					if(result!=0)
					{
						handle_error(result);
					}
					return((Eth32PwmClock)val);
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
			set
			{
				int result;

				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result=import.eth32_set_pwm_clock_state(handle, (int)value);
					if(result!=0)
					{
						handle_error(result);
					}
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
		}
		public int PwmBasePeriod
		{
			get
			{
				int result;
				int val;

				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result=import.eth32_get_pwm_base_period(handle, out val);
					if(result!=0)
					{
						handle_error(result);
					}
					return(val);
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
			set
			{
				int result;

				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
				try
				{
					precheck();

					result=import.eth32_set_pwm_base_period(handle, value);
					if(result!=0)
					{
						handle_error(result);
					}
				}
				finally
				{
					rwlock.ReleaseReaderLock();
				}
			}
		}
		public void SetPwmParameters(int channel, Eth32PwmChannel state, double freq, double duty)
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_set_pwm_parameters(handle, channel, (int)state, (float)freq, (float)duty);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}
		public void GetPwmParameters(int channel, out Eth32PwmChannel state, out double freq, out double duty)
		{
			int result;
			int temp_state;
			float temp_freq;
			float temp_duty;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_get_pwm_parameters(handle, channel, out temp_state, out temp_freq, out temp_duty);
				if(result!=0)
				{
					handle_error(result);
				}
				state=(Eth32PwmChannel)temp_state;
				freq=(double)temp_freq;
				duty=(double)temp_duty;
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}
        public byte[] GetEeprom(int address, int length)
        {
            int result;

            rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
            try
            {
                precheck();

                if (firmware_major < 3)
                    handle_error((int)(EthError.NotSupported));

                // Specific check to ETH32 -- but keeps us from inadvertently trying 
                // to allocate a huge amount of memory:
                if (length > 256)
                    handle_error((int)(EthError.InvalidOther));

                byte[] results = new byte[length];
                result = import.eth32_get_eeprom(handle, address, length, results);
                if (result != 0)
                {
                    handle_error(result);
                }
                return(results);
            }
            finally
            {
                rwlock.ReleaseReaderLock();
            }

        }
        public void SetEeprom(int address, int length, byte[] buffer)
        {
            int result;

            rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
            try
            {
                precheck();

                if (firmware_major < 3)
                    handle_error((int)(EthError.NotSupported));
                if (buffer.Length < length)
                    handle_error((int)(EthError.BufSize));

                result = import.eth32_set_eeprom(handle, address, length, buffer);
                if (result != 0)
                {
                    handle_error(result);
                }
            }
            finally
            {
                rwlock.ReleaseReaderLock();
            }

        }

        public void Disconnect()
		{
			int result;

			rwlock.AcquireWriterLock(System.Threading.Timeout.Infinite);

			try
			{
				closing=true;
				if(Connected==false)
					return;

				precheck();

				// Set the Event Queue max size to 0, which shuts down our thread
				// Pass false for whether to lock the main lock since we already have it
				// locked as a writer.  This avoids a deadlock.
				set_event_queue_limit(0, false);

				result=import.eth32_close(handle);
				if(result!=0)
				{
					handle_error(result);
				}

				handle=IntPtr.Zero;
			}
			finally
			{
				closing=false;
				rwlock.ReleaseWriterLock();
			}
		}

		public static string ErrorString(EthError errorcode)
		{
            IntPtr ptr;
			string result;

			if((int)errorcode>EthErrorNetClass)
			{
				ptr=import.eth32_error_string((int)errorcode);
                result = Marshal.PtrToStringAnsi(ptr);
			}
			else
			{
				switch(errorcode)
				{
					case EthError.AlreadyConnected:
						result="EthError.AlreadyConnected: A connection is already open in this class instance.  Please disconnect first before reconnecting to another device.";
						break;
					case EthError.NotConnected:
						result="EthError.NotConnected: This class instance is not yet connected to a device.";
						break;
					default:
						result="Unrecognized error code";
						break;
				}
			}
			return(result);
		}




		internal void set_led(int led, bool val)
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_set_led(handle, led, val ? 1 : 0);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}

		}
		internal bool get_led(int led)
		{
			int result;
			int val;
			
			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_get_led(handle, led, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return(val!=0);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}

		}

		internal void set_pwm_channel(int channel, Eth32PwmChannel state)
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_set_pwm_channel(handle, channel, (int)state);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}

		internal Eth32PwmChannel get_pwm_channel(int channel)
		{
			int result;
			int val;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_get_pwm_channel(handle, channel, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return((Eth32PwmChannel)val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}

		internal void set_pwm_duty_period(int channel, int period)
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_set_pwm_duty_period(handle, channel, period);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}

		internal int get_pwm_duty_period(int channel)
		{
			int result;
			int val;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_get_pwm_duty_period(handle, channel, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return(val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}

		internal void set_counter_state(int counter, Eth32CounterState state)
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_set_counter_state(handle, counter, (int)state);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}

		internal Eth32CounterState get_counter_state(int counter)
		{
			int result;
			int val;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_get_counter_state(handle, counter, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return((Eth32CounterState)val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}

		internal void set_counter_value(int counter, int val)
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_set_counter_value(handle, counter, val);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}

		internal int get_counter_value(int counter)
		{
			int result;
			int val;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_get_counter_value(handle, counter, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return(val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}

		internal void set_counter_rollover(int counter, int val)
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_set_counter_rollover(handle, counter, val);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}

		internal int get_counter_rollover(int counter)
		{
			int result;
			int val;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_get_counter_rollover(handle, counter, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return(val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}

		internal void set_counter_threshold(int counter, int val)
		{
			int result;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_set_counter_threshold(handle, counter, val);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}

		internal int get_counter_threshold(int counter)
		{
			int result;
			int val;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_get_counter_threshold(handle, counter, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return(val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}

		internal int get_event_queue_limit()
		{
			int result;
			int val;
			int junk;
			int junk2;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result=import.eth32_get_event_queue_status(handle, out val, out junk, out junk2);
				if(result!=0)
				{
					handle_error(result);
				}

				return(val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}

		}

		internal void set_event_queue_limit(int limit, bool mainlock)
		{
			// The mainlock parameter specifies whether to acquire a reader lock on the 
			// main rwlock.  This should normally be passed in as true.  The only reason
			// we allow you to pass in false is because this function is used from the 
			// Disconnect function and if we didn't have a way to skip locking, this would
			// deadlock.
			ThreadStart threadStart;
			int oldlimit;
			int result;
			int fullqueue;
			int junk;

			if(mainlock)
			{
				rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			}

			try
			{

				precheck();

				event_queue_config_lock.WaitOne();
				try
				{
					result=import.eth32_get_event_queue_status(handle, out oldlimit, out fullqueue, out junk);
					if(result!=0)
					{
						handle_error(result);
					}

					result=import.eth32_set_event_queue_config(handle, limit, fullqueue);
					if(result!=0)
					{
						handle_error(result);
					}

					// If the new limit is nonzero, we need the thread running, and if it is zero, we need it
					// stopped.  We determine whether the thread is running or not by whether the 
					// event_thread private member is null or non-null
					if(limit>0 && event_thread_ref==null)
					{
						// Start our own thread that retrieves events.
						try
						{
							event_thread ethread = new event_thread(this);

							threadStart = new ThreadStart(ethread.event_thread_func);
							event_thread_ref = new Thread(threadStart);
							// Set the thread type to background thread so that it won't prevent
							// the application from terminating if only it or other background 
							// threads are left
							event_thread_ref.IsBackground=true;
							event_thread_ref.Start();
						}
						catch
						{
							handle_error((int)EthError.Ethread);
						}
					}
					else if(limit==0 && event_thread_ref!=null)
					{
						// Stop our thread.  Actually, by setting the queue limit to zero
						// above, we've already stopped the thread.  We just need to wait on 
						// it here now to be sure it has gotten that far and actually exited.

						event_thread_ref.Join();
						event_thread_ref = null;
					}
				}
				finally
				{
					event_queue_config_lock.ReleaseMutex();
				}

			}
			finally
			{
				if(mainlock)
				{
					rwlock.ReleaseReaderLock();
				}
			}

		}

		internal void set_analog_assignment(int channel, Eth32AnalogChannel source)
		{
			int result;
			
			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_set_analog_assignment(handle, channel, (int)source);
				if(result!=0)
				{
					handle_error(result);
				}
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}

		internal Eth32AnalogChannel get_analog_assignment(int channel)
		{
			int result;
			int val;

			rwlock.AcquireReaderLock(System.Threading.Timeout.Infinite);
			try
			{
				precheck();

				result = import.eth32_get_analog_assignment(handle, channel, out val);
				if(result!=0)
				{
					handle_error(result);
				}
				return((Eth32AnalogChannel)val);
			}
			finally
			{
				rwlock.ReleaseReaderLock();
			}
		}

	
	
	}
}
