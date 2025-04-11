using System;
using System.Runtime.Serialization;

namespace WinfordEthIO
{
	// Customized Exception class - includes the ETH32 API error code as a member
	// Make it serializable (implement ISerializable) so the exception can be 
	// passed through Remoting
	[Serializable]
	public class Eth32Exception : System.Exception 
	{
		private EthError ecode;
		public Eth32Exception(EthError errorcode)
		{
			ecode=errorcode;
		}

		public Eth32Exception(string message, EthError errorcode) : base(message)
		{
			ecode=errorcode;
		}

		public Eth32Exception(string message, EthError errorcode, Exception inner) : base(message, inner)
		{
			ecode=errorcode;
		}

		public Eth32Exception(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			ecode=(EthError)(info.GetInt32("ecode"));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("ecode", (int)ecode);
		}

		public EthError ErrorCode
		{
			get
			{
				return(ecode);
			}
		}
	}

	// Error codes
	public enum EthError
	{
        None=0,
        General=-1,            // Unknown or other error
        Closing=-2,            // Function aborted since the device is being closed
        Network=-10,           // unable to open, read, or write to the network socket
        Thread=-11,            // General error ocurred in the threads library used by this API.
        NotSupported=-12,      // returned if an API function is not supported by a device
        Pipe=-13,              // Internal API error
        Rthread=-14,           // Internal API error
        Ethread=-15,           // Internal API error
        Malloc=-16,            // Problem involving allocating memory
        Windows=-17,           // Internal API error - specific to Windows platform
        Winsock=-18,           // Internal API error - specific to Windows sockets
        NetworkIntr=-19,       //  Network read/write operation was interrupted.
        WrongMode=-20,         // Something is not configured correctly in order to allow this functionality.
        BcastOpt=-21,          // Error setting SO_BROADCAST option on socket
        ReuseOpt=-22,           // Error setting SO_REUSEADDR option on socket
        ConfigNoAck=-23,        // Really a warning - no acknowledgement after configuring IP settings of device
        ConfigReject=-24,       // The device refused to set its IP configuration settings
        LoadLib=-25,            // Error loading an external DLL library
        Plugin=-26,             // General error with plugin being used (for device discovery, sniffing, etc)
        BufSize=-27,            // A buffer provided is either invalid size or too small
        InvalidHandle=-101,    // Invalid device handle was passed in
        InvalidPort=-104,      // Port specified is invalid for the device model
        InvalidBit=-109,       // Value passed identifying bit is out of range
        InvalidChannel=-111,   // Invalid channel number specified
        InvalidPointer=-112,   // Invalid pointer given as a parameter to an API function
        InvalidOther=-113,     // Some parameter passed to an API function was invalid or out of range
        InvalidValue=-114,     // Value out of possible range for that I/O port
        InvalidIp=-115,        // Invalid IP address was provided
        InvalidNetmask=-116,   // Invalid netmask was provided
        InvalidIndex=-117,     // Invalid index value
        Timeout=-201,          // Timeout ocurred. Most likely communication has been lost w/ device

        AlreadyConnected=-1000,// Can't call Connect when an object is already connected
        NotConnected=-1001     // Tried to perform operation before object was connected
	};

}