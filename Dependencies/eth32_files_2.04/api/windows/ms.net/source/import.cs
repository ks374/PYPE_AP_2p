using System;
using System.Text;
using System.Runtime.InteropServices;

namespace WinfordEthIO
{
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct eth32_event
	{
		public int id; // User-defined event ID
		public int type; // Event type, as defined by the EVENT_DIGITAL, EVENT_* definitions
		public int port; // Digital port, Analog event bank, or Counter number
		public int bit;  // Bit or Channel, or -1 for a port event
		public int prev_value; // Value of the bit / port / channel(above/below) before
		                       // the event fired
		public int val;      // New value of the bit / port / channel(full value)
		                       // or the number of times the counter has fired an event or rolled
		public int direction;  // -1 for falling, 1 for rising
	}



	[StructLayout(LayoutKind.Sequential, Pack=4)]
	internal struct eth32_handler
	{
		public int type; /* which method to use for event notification */
			  /* HANDLER_NONE     - none; disabled.
			   * HANDLER_CALLBACK - callback function
			   * HANDLER_MESSAGE  - Windows message notification: 
			   *     A windows message with the given message ID is sent 
			   *     to the specified window
			   *     whenever a new event fires.  This is intended to be 
			   *     used with the event queue such that when the windows
			   *     message is received, you attempt to dequeue any events
			   *     that are in the message queue.
			   */
		/* The following are only used with type HANDLER_CALLBACK */
		public int maxqueue; // Maximum number of events that can be queued waiting for 
		                    // the callback to finish
		public int fullqueue; // What to do if queue ever gets full.  Specify one of the
		                     // QUEUE_... constants
		public int eventfn; // Address of user-defined callback function.
		public int extra;  // Extra user-defined value to be passed to the callback
		                   // whenever it is called.

		/* The following are only used with type HANDLER_MESSAGE: */
		public int window; // Window handle to send messages to.
		public uint msgid; // Windows message to be sent to window.
		public uint wparam; // wparam to be included with any messages that are sent
		public int lparam; // lparam to be included with any messages that are sent
	}

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct eth32cfg_ip
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
        public byte[] bytes;

        public eth32cfg_ip(bool initarray)
        {
            // constructor  -- we have the bool parameter because a parameter-less
            // constructor is not allowed in a struct
            if(initarray)
                bytes = new byte[4] { 0, 0, 0, 0 };
            else
                bytes = null;
        }

        public override string ToString()
        {
            return(Eth32Config.IpConvertToString(this));
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct eth32cfg_mac
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] bytes;

        public eth32cfg_mac(bool initarray)
        {
            if (initarray)
                bytes = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            else
                bytes = null;
        }

        public override string ToString()
        {
            return (Eth32Config.MacConvertToString(this));
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct eth32cfg_data
    {
        public byte product_id;
	    public byte firmware_major;
	    public byte firmware_minor;
	    public byte config_enable;
        public eth32cfg_mac mac; 
	    public ushort serialnum_batch;
	    public ushort serialnum_unit; 
	    public eth32cfg_ip config_ip;
	    public eth32cfg_ip config_gateway;
	    public eth32cfg_ip config_netmask;
	    public eth32cfg_ip active_ip;
	    public eth32cfg_ip active_gateway;
	    public eth32cfg_ip active_netmask;
	    public byte dhcp;
    }

	internal class import
	{
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern IntPtr eth32_open([MarshalAs(UnmanagedType.LPStr)] string address, ushort port, uint timeout, out int result);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_timeout(IntPtr handle, uint timeout);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_timeout(IntPtr handle, out uint timeout);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_verify_connection(IntPtr handle);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_output_byte(IntPtr handle, int port, int value);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_output_bit(IntPtr handle, int port, int bit, int value);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_pulse_bit(IntPtr handle, int port, int bit, int edge, int count);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_led(IntPtr handle, int led, int value);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_input_byte(IntPtr handle, int port, out int value);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_input_successive(IntPtr handle, int port, int max, out int value, out int status);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_input_bit(IntPtr handle, int port, int bit, out int value);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_readback(IntPtr handle, int port, out int value);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_led(IntPtr handle, int led, out int value);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_direction(IntPtr handle, int port, int direction);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_direction(IntPtr handle, int port, out int direction);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_direction_bit(IntPtr handle, int port, int bit, int direction);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_direction_bit(IntPtr handle, int port, int bit, out int direction);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_analog_state(IntPtr handle, int state);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_analog_state(IntPtr handle, out int state);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_analog_reference(IntPtr handle, int reference);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_analog_reference(IntPtr handle, out int reference);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_input_analog(IntPtr handle, int channel, out int value);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_analog_eventdef(IntPtr handle, int bank, int channel, int lomark, int himark, int defaultval);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_analog_eventdef(IntPtr handle, int bank, int channel, out int lomark, out int himark);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_analog_assignment(IntPtr handle, int channel, int source);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_analog_assignment(IntPtr handle, int channel, out int source);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_reset(IntPtr handle);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_serialnum(IntPtr handle, out int batch, out int unit);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern int eth32_get_serialnum_string(IntPtr handle, [In, Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder serial, int bufsize);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_product_id(IntPtr handle, out int prodid);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_firmware_release(IntPtr handle, out int major, out int minor);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_connection_flags(IntPtr handle, int reset, out int flags);

		
		
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_event_queue_config(IntPtr handle, int maxsize, int fullqueue);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_event_queue_status(IntPtr handle, out int maxsize, out int fullqueue, out int cursize);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_dequeue_event(IntPtr handle, out eth32_event ev, int timeout);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_empty_event_queue(IntPtr handle);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_enable_event(IntPtr handle, int type, int port, int bit, int id);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_disable_event(IntPtr handle, int type, int port, int bit);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_event_handler(IntPtr handle, ref eth32_handler handler);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_event_handler(IntPtr handle, out eth32_handler handler);

		
		
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_counter_state(IntPtr handle, int counter, int state);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_counter_state(IntPtr handle, int counter, out int state);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_counter_value(IntPtr handle, int counter, int value);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_counter_value(IntPtr handle, int counter, out int value);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_counter_rollover(IntPtr handle, int counter, int rollover);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_counter_rollover(IntPtr handle, int counter, out int rollover);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_counter_threshold(IntPtr handle, int counter, int threshold);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_counter_threshold(IntPtr handle, int counter, out int threshold);

		
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_pwm_clock_state(IntPtr handle, int state);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_pwm_clock_state(IntPtr handle, out int state);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_pwm_base_period(IntPtr handle, int period);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_pwm_base_period(IntPtr handle, out int period);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_pwm_channel(IntPtr handle, int channel, int state);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_pwm_channel(IntPtr handle, int channel, out int state);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_pwm_duty_period(IntPtr handle, int channel, int period);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_pwm_duty_period(IntPtr handle, int channel, out int period);


		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_set_pwm_parameters(IntPtr handle, int channel, int state, float freq, float duty);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_get_pwm_parameters(IntPtr handle, int channel, out int state, out float freq, out float duty);

        [DllImport("eth32api.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int eth32_get_eeprom(IntPtr handle, int address, int length, [In, Out] byte[] buffer);

        [DllImport("eth32api.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int eth32_set_eeprom(IntPtr handle, int address, int length, [In] byte[] buffer);
		
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern IntPtr eth32_error_string(int errorcode);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern int eth32_close(IntPtr handle);


        // Configuration functions
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern int eth32cfg_ip_to_string(ref eth32cfg_ip ipbinary, [In, Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder ipstring); // ipstring buffer MUST be at least 16 bytes long
		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern int eth32cfg_string_to_ip([MarshalAs(UnmanagedType.LPStr)] string ipstring, out eth32cfg_ip ipbinary);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern IntPtr eth32cfg_query(ref eth32cfg_ip bcastaddr, out int number, out int result);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern IntPtr eth32cfg_discover_ip(ref eth32cfg_ip bcastaddr, uint flags, ref eth32cfg_mac mac, byte product_id, ushort serialnum_batch, ushort serialnum_unit, out int number, out int result);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern int eth32cfg_get_config(IntPtr handle, int index, out eth32cfg_data config_data);
		
        [DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern int eth32cfg_set_config(ref eth32cfg_ip bcastaddr, ref eth32cfg_data config_data);
		
        [DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern int eth32cfg_serialnum_string(byte product_id, ushort batch, ushort unit, [In, Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder serialstring, int bufsize);
		
        [DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern void eth32cfg_free(IntPtr handle);


		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern int eth32cfg_plugin_load(int option);

		[DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern IntPtr eth32cfg_plugin_interface_list(out int numd, out int result);
		
        [DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern int eth32cfg_plugin_interface_address(IntPtr handle, int index, out eth32cfg_ip ip, out eth32cfg_ip netmask);
		
        [DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern int eth32cfg_plugin_interface_type(IntPtr handle, int index, out int type);
		
        [DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern int eth32cfg_plugin_interface_name(IntPtr handle, int index, int nametype, [In, Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder name, ref int length);
		
        [DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern int eth32cfg_plugin_choose_interface(IntPtr handle, int index);
		
        [DllImport("eth32api.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
        public static extern void eth32cfg_plugin_interface_list_free(IntPtr handle);

	}
}
