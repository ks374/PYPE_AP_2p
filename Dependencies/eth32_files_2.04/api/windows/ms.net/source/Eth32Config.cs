using System;
using System.Collections.Generic;
using System.Text;

namespace WinfordEthIO
{
    #region Enumerators
    public enum Eth32ConfigPluginType
    {
        None=0,
        System=1,
        Pcap=2
    }
    public enum Eth32ConfigInterfaceName
    {
        Standard = 0,
        Friendly = 1,
        Description = 2
    }
    public enum Eth32ConfigInterfaceType
    {
        Unknown = 0,
        Other = 1,
        Ethernet = 6,
        Tokenring = 9,
        Fddi = 15,
        Ppp = 23,
        Loopback = 24,
        Slip = 28
    }
    [Flags]
    public enum Eth32ConfigFilter
    {
        None = 0,
        Mac = 1,
        Serial = 2
    }

    #endregion

    public struct Eth32ConfigPluginInterface
    {
        public eth32cfg_ip Ip;
        public eth32cfg_ip Netmask;
        public Eth32ConfigInterfaceType InterfaceType;
        public string StandardName;
        public string FriendlyName;
        public string Description;
    }

    public class Eth32Config : IDisposable
    {
        #region Private / Internal members and constants
        private IntPtr handle;
        private int numresults;
        private eth32cfg_ip bcastaddr;
        #endregion

        #region Indexer member variables
        public readonly ConfigResultIndexer Result;
        #endregion

        public const byte ProductId = 105; // Product ID for ETH32 devices

        #region Private / Internal functions
        private static void handle_error(int ecode)
        {
            // Raise an error (throw an exception)
            Eth32Exception ex = new Eth32Exception(Eth32.ErrorString((EthError)ecode), (EthError)ecode);
            ex.Source = "Eth32Config";

            throw ex;
        }

        internal eth32cfg_data get_result(int index)
        {
            eth32cfg_data result;
            int ecode;

            if ((index < 0) || (index >= numresults))
                handle_error((int)EthError.InvalidIndex);

            ecode = import.eth32cfg_get_config(handle, index, out result);
            if (ecode != 0)
                handle_error(ecode);

            return (result);
        }
        #endregion



        public Eth32Config()
        {
            Result = new ConfigResultIndexer(this);

            handle = IntPtr.Zero;
            bcastaddr = new eth32cfg_ip(true);
            bcastaddr.bytes[0] = 255; // Default broadcast address
            bcastaddr.bytes[1] = 255;
            bcastaddr.bytes[2] = 255;
            bcastaddr.bytes[3] = 255;

        }

        protected virtual void Cleanup()
		{
			// Common cleanup routine called by destructor or Dispose
            Free();
		}

		public void Dispose()
		{
			Cleanup();
		}

		~Eth32Config()
		{
			try
			{
				Cleanup();
			}
			catch(ObjectDisposedException ex)
			{
				// When the application is shutting down, there is a distinct possibility that
				// the DllImport functions will be garbage collected before instances of the 
				// Eth32Config class.  In this case, when the objects try to close things out by 
				// calling DLL functions, it causes an exception.  Since the application is
				// closing down anyway, we'll catch the exception here and just stay quiet.
				string junk;
				junk=ex.Source; // simply eliminate the compiler warning about not using ex
			}
		}

        public void Free()
        {
            // If we have a handle open to a results list, free it
            if (handle != IntPtr.Zero)
            {
                import.eth32cfg_free(handle);
            }
            handle = IntPtr.Zero;
            numresults = 0;
        }

        public int NumResults
        {
            get
            {
                return(numresults);
            }
        }

        public eth32cfg_ip BroadcastAddress
        {
            get
            {
                return (bcastaddr);
            }
            set
            {
                bcastaddr = value;
            }
        }
        public string BroadcastAddressString
        {
            get
            {
                return (IpConvertToString(bcastaddr));
            }
            set
            {
                // This will throw an exception if they provide an invalid IP string
                bcastaddr = IpConvert(value);
            }
        }

        public static eth32cfg_ip IpConvert(string ipaddr)
        {
            eth32cfg_ip result;
            int ecode;

            ecode = import.eth32cfg_string_to_ip(ipaddr, out result);
            if (ecode != 0)
                handle_error(ecode);
            return (result);
        }
        public static eth32cfg_ip IpConvert(System.Net.IPAddress ipaddr)
        {
            eth32cfg_ip result = new eth32cfg_ip();

            if (ipaddr.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                handle_error((int)EthError.InvalidIp);

            result.bytes = ipaddr.GetAddressBytes();

            return(result);
        }

        public static eth32cfg_mac MacConvert(string macstring)
        {
            try
            {
                eth32cfg_mac result = new eth32cfg_mac(true);
                string[] pieces;
                int i;

                pieces = macstring.Split(":".ToCharArray());

                if (pieces.Length != 6)
                    handle_error((int)EthError.InvalidOther);

                for (i = 0; i < 6; i++)
                {
                    if (pieces[i].Length > 2)
                        handle_error((int)EthError.InvalidOther);
                    result.bytes[i] = byte.Parse(pieces[i], System.Globalization.NumberStyles.AllowHexSpecifier);
                }

                return (result);
            }
            catch
            {
                handle_error((int)EthError.InvalidOther);
            }
            // Should never get here:
            return (new eth32cfg_mac(false));
        }

        public static string MacConvertToString(eth32cfg_mac macbinary)
        {
            int i;
            string result = "";

            if (macbinary.bytes.Length >= 6)
            {
                for (i = 0; i < 6; i++)
                {
                    result += macbinary.bytes[i].ToString("X2");
                    if (i < 5)
                        result += ":";
                }
            }
            // Otherwise, just return the empty string

            return (result);
        }

        public static string IpConvertToString(eth32cfg_ip ipbinary)
        {
            StringBuilder sb=new StringBuilder(20);
            int ecode;

            ecode = import.eth32cfg_ip_to_string(ref ipbinary, sb);
            if (ecode != 0)
                handle_error(ecode);
            return (sb.ToString());
        }

        public static System.Net.IPAddress IpConvertToNetIPAddress(eth32cfg_ip ipbinary)
        {
            // This method converts a binary IP address structure to a 
            // System.Net.IPAddress class.
            System.Net.IPAddress result;

            if(ipbinary.bytes.Length != 4)
                handle_error((int)EthError.InvalidIp);

            result = new System.Net.IPAddress(ipbinary.bytes);
            // Note that the AddressFamily will be InterNetwork, per the MS docs,
            // when the given array of bytes has a length of 4.

            return (result);
        }

        public static string SerialNumString(byte product_id, ushort serialnum_batch, ushort serialnum_unit)
        {
            int ecode;
            StringBuilder sb = new StringBuilder(100);

			ecode=import.eth32cfg_serialnum_string(product_id, serialnum_batch, serialnum_unit, sb, sb.Capacity);
			if(ecode!=0)
			{
				handle_error(ecode);
			}

			return(sb.ToString());
        }


        public int Query()
        {
            int number;
            IntPtr temp;
            int ecode;

            temp = import.eth32cfg_query(ref bcastaddr, out number, out ecode);

            if(temp == IntPtr.Zero)
                handle_error(ecode);

            // If successful, go ahead and free the previous results before we overwrite the pointer (if applicable)
            if (handle != IntPtr.Zero)
                Free();

            handle = temp;
            numresults = number;
            return(numresults);
        }

        public int DiscoverIp(eth32cfg_mac mac)
        {
            return(DiscoverIp(Eth32ConfigFilter.Mac, mac, 0, 0,0));
        }

        public int DiscoverIp(byte product_id, ushort serialnum_batch, ushort serialnum_unit)
        {
            return(DiscoverIp(Eth32ConfigFilter.Serial, new eth32cfg_mac(), product_id, serialnum_batch, serialnum_unit));
        }

        public int DiscoverIp(eth32cfg_mac mac, byte product_id, ushort serialnum_batch, ushort serialnum_unit)
        {
            return (DiscoverIp(Eth32ConfigFilter.Mac | Eth32ConfigFilter.Serial, mac, product_id, serialnum_batch, serialnum_unit));
        }

        public int DiscoverIp(Eth32ConfigFilter filter, eth32cfg_mac mac, byte product_id, ushort serialnum_batch, ushort serialnum_unit)
        {
            int number;
            IntPtr temp;
            int ecode;

            // If we're filtering on Mac, then the provided array of mac bytes must be at least 6 long
            if (((filter & Eth32ConfigFilter.Mac) == Eth32ConfigFilter.Mac) && mac.bytes.Length < 6)
                handle_error((int)EthError.BufSize);

            temp = import.eth32cfg_discover_ip(ref bcastaddr, (uint)filter, ref mac, product_id, serialnum_batch, serialnum_unit, out number, out ecode);

            if (temp == IntPtr.Zero)
                handle_error(ecode);

            // If successful, go ahead and free the previous results before we overwrite the pointer (if applicable)
            if (handle != IntPtr.Zero)
                Free();

            handle = temp;
            numresults = number;
            return (numresults);
        }

        public void SetConfig(eth32cfg_data config_data)
        {
            int ecode;

            ecode = import.eth32cfg_set_config(ref bcastaddr, ref config_data);

            if(ecode!=0)
                handle_error(ecode);
        }

    }


    public class Eth32ConfigPlugin : IDisposable
    {
        #region Private / Internal members and constants
        private IntPtr handle;
        private int numresults;
        #endregion

        #region Indexer member variables
        public readonly ConfigPluginInterfaceIndexer NetworkInterface;
        #endregion

        #region Private / Internal functions
        private static void handle_error(int ecode)
        {
            // Raise an error (throw an exception)
            Eth32Exception ex = new Eth32Exception(Eth32.ErrorString((EthError)ecode), (EthError)ecode);
            ex.Source = "Eth32ConfigPlugin";

            throw ex;
        }

        internal string get_name(int index, Eth32ConfigInterfaceName name_type)
        {
            int ecode;
            int length;
            StringBuilder sb;

            length = 0;
            ecode = import.eth32cfg_plugin_interface_name(handle, index, (int)name_type, null, ref length);
            if (ecode == (int)EthError.BufSize)
            {
                sb = new StringBuilder(length);
                ecode = import.eth32cfg_plugin_interface_name(handle, index, (int)name_type, sb, ref length);
                if (ecode == 0)
                    return (sb.ToString());
                else if (ecode == (int)EthError.NotSupported)
                    return (null);
                else
                    handle_error(ecode);
            }
            else if (ecode == (int)EthError.NotSupported)
                return (null);
            else
                handle_error(ecode);

            // We should never get here, but just in case and to satisfy compiler warnings:
            return(null);
        }

        internal Eth32ConfigPluginInterface get_interface(int index)
        {
            Eth32ConfigPluginInterface iface=new Eth32ConfigPluginInterface();
            int ecode;
            eth32cfg_ip ip;
            eth32cfg_ip netmask;
            int iftype;

            if ((index < 0) || (index >= numresults))
                handle_error((int)EthError.InvalidIndex);

            ecode = import.eth32cfg_plugin_interface_address(handle, index, out ip, out netmask);
            if(ecode==0)
            {
                iface.Ip = ip;
                iface.Netmask = netmask;
            }
            else if (ecode == (int)EthError.NotSupported)
            {
                iface.Ip.bytes = null;
                iface.Netmask.bytes = null;
            }
            else
            {
                handle_error(ecode);
            }

            iface.StandardName = get_name(index, Eth32ConfigInterfaceName.Standard);
            iface.FriendlyName = get_name(index, Eth32ConfigInterfaceName.Friendly);
            iface.Description = get_name(index, Eth32ConfigInterfaceName.Description);

            ecode = import.eth32cfg_plugin_interface_type(handle, index, out iftype);
            if (ecode == 0)
            {
                iface.InterfaceType = (Eth32ConfigInterfaceType)iftype;

            }
            else if (ecode == ((int)EthError.NotSupported))
                iface.InterfaceType = 0;
            else
                handle_error(ecode);
           
            return(iface);
        }
        #endregion



        public Eth32ConfigPlugin()
        {
            NetworkInterface = new ConfigPluginInterfaceIndexer(this);

            handle = IntPtr.Zero;
        }

        protected virtual void Cleanup()
		{
			// Common cleanup routine called by destructor or Dispose
            Free();
		}

		public void Dispose()
		{
			Cleanup();
		}

		~Eth32ConfigPlugin()
		{
			try
			{
				Cleanup();
			}
			catch(ObjectDisposedException ex)
			{
				// When the application is shutting down, there is a distinct possibility that
				// the DllImport functions will be garbage collected before instances of this 
				// class.  In this case, when the objects try to close things out by 
				// calling DLL functions, it causes an exception.  Since the application is
				// closing down anyway, we'll catch the exception here and just stay quiet.
				string junk;
				junk=ex.Source; // simply eliminate the compiler warning about not using ex
			}
		}

        public static void Load(Eth32ConfigPluginType plugin_type)
        {
            int ecode;

            ecode = import.eth32cfg_plugin_load((int)plugin_type);
            if (ecode != 0)
                handle_error(ecode);
        }

        public int GetInterfaces()
        {
            int ecode;
            int number;
            IntPtr temphandle;

            temphandle = import.eth32cfg_plugin_interface_list(out number, out ecode);
            if (ecode != 0)
                handle_error(ecode);

            Free(); // just in case we have previous results to free

            handle = temphandle;
            numresults = number;

            return (numresults);
        }

        public void ChooseInterface(int index)
        {
            int ecode;

            if (index < 0 || index >= numresults)
                handle_error((int)EthError.InvalidIndex);

            ecode = import.eth32cfg_plugin_choose_interface(handle, index);
            if (ecode != 0)
                handle_error(ecode);
        }

        public void Free()
        {
            // If we have a handle open to a results list, free it
            if (handle != IntPtr.Zero)
            {
                import.eth32cfg_plugin_interface_list_free(handle);
            }
            handle = IntPtr.Zero;
            numresults = 0;
        }

        public int NumInterfaces
        {
            get
            {
                return(numresults);
            }
        }

    }
}
