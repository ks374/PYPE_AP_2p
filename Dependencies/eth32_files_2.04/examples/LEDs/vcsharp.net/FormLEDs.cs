using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WinfordEthIO;

namespace LEDs
{
    public partial class FormLEDs : Form
    {
        Eth32 dev = new Eth32();

        public FormLEDs()
        {
            InitializeComponent();
        }

        private void cmdDetect_Click(object sender, EventArgs e)
        {
            Eth32Config ethdetect = new Eth32Config();
            int i;

            // Start with an empty Combo Box
            cmbETH32.Items.Clear();

            // Detect any ETH32 devices on the local network
            // Make the mouse pointer an hour-glass during detection (which takes a couple seconds)
            // so the user knows it's in progress.
            Cursor.Current = Cursors.WaitCursor;
            ethdetect.Query();
            Cursor.Current = Cursors.Default;

            if (ethdetect.NumResults > 0) // If we found at least one device
            {
                // loop through them and add the Active IP of each to the combo box list
                for (i = 0; i < ethdetect.NumResults; i++)
                {
                    // Retrieve the active IP of this particular device and add it to the combo box
                    cmbETH32.Items.Add(ethdetect.Result[i].active_ip.ToString());

                }
                // Make the combo box drop down so the user can see the available options
                cmbETH32.DroppedDown = true;
            }
            else
            {
                // Otherwise, we didn't find any devices.  Let the user know
                MessageBox.Show("No devices were found.  Detection will only find ETH32 devices on the local network segment.  For devices that are outside the local segment, please enter the IP address or host name into the combo, and click Connect.");
            }

        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            if (cmbETH32.Text.Length == 0)
            {
                MessageBox.Show("Please select (or manually type) an IP address or host name of the ETH32 to connect to.");
                return;
            }

            // If we're already connected, disconnect and create a new object
            if (dev.Connected)
            {
                dev.Disconnect();
                dev = new Eth32();
            }

            try
            {
                // Connect, use a 5 second timeout
                dev.Connect(cmbETH32.Text, Eth32.DefaultPort, 5000);

                // After connecting, call the procedure for the Refresh button
                // so that the initial state of the check boxes matches the
                // state of the ETH32 LED's
                cmdRefresh_Click(null, null);

                MessageBox.Show("Successfully connected to ETH32 device.");
            }
            catch (Eth32Exception etherr)
            {
                MessageBox.Show("Error connecting to the ETH32: " + Eth32.ErrorString(etherr.ErrorCode));
            }

        }

        private void FormLEDs_FormClosing(object sender, FormClosingEventArgs e)
        {
            // When this form closes, close the connection to the ETH32 as well
            if (dev != null)
            {
                // dev is at least instantiated
                if (dev.Connected)
                {
                    dev.Disconnect();
                }
            }
        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            // Retrieve the LED state from the ETH32 again (in case another connection changed them)
            if (dev == null || dev.Connected == false)
            {
                MessageBox.Show("Please connect first.");
                return;
            }

            try
            {
                led0.Checked = dev.Led[0];
                led1.Checked = dev.Led[1];
            }
            catch (Eth32Exception etherr)
            {
                MessageBox.Show("Error communicating with the ETH32: " + Eth32.ErrorString(etherr.ErrorCode));
            }

        }

        private void led0_CheckedChanged(object sender, EventArgs e)
        {
            if (dev == null || dev.Connected == false)
            {
                MessageBox.Show("Please connect first.");
                return;
            }

            try
            {
                dev.Led[0] = led0.Checked;
            }
            catch (Eth32Exception etherr)
            {
                MessageBox.Show("Error communicating with the ETH32: " + Eth32.ErrorString(etherr.ErrorCode));
            }

        }

        private void led1_CheckedChanged(object sender, EventArgs e)
        {
            if (dev == null || dev.Connected == false)
            {
                MessageBox.Show("Please connect first.");
                return;
            }

            try
            {
                dev.Led[1] = led1.Checked;
            }
            catch (Eth32Exception etherr)
            {
                MessageBox.Show("Error communicating with the ETH32: " + Eth32.ErrorString(etherr.ErrorCode));
            }

        }
    }
}
