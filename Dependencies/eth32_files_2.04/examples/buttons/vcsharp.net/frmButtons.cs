using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using WinfordEthIO;

namespace Buttons
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class frmButtons : System.Windows.Forms.Form
	{
		internal System.Windows.Forms.Label Label4;
		internal System.Windows.Forms.Label Label3;
		internal System.Windows.Forms.Label Label2;
		internal System.Windows.Forms.Label button1;
		internal System.Windows.Forms.Label button0;
		internal System.Windows.Forms.Button cmdConnect;
		internal System.Windows.Forms.TextBox txtAddress;
		internal System.Windows.Forms.Label Label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		Eth32 dev = new Eth32();
		private const int ID_BUTTON0 = 100;
		private const int ID_BUTTON1 = 101;

		public frmButtons()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Label4 = new System.Windows.Forms.Label();
			this.Label3 = new System.Windows.Forms.Label();
			this.Label2 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Label();
			this.button0 = new System.Windows.Forms.Label();
			this.cmdConnect = new System.Windows.Forms.Button();
			this.txtAddress = new System.Windows.Forms.TextBox();
			this.Label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// Label4
			// 
			this.Label4.Location = new System.Drawing.Point(10, 55);
			this.Label4.Name = "Label4";
			this.Label4.Size = new System.Drawing.Size(128, 56);
			this.Label4.TabIndex = 15;
			this.Label4.Text = "Once connected, press external pushbuttons to update screen status and on-board L" +
				"EDs.";
			// 
			// Label3
			// 
			this.Label3.Location = new System.Drawing.Point(178, 95);
			this.Label3.Name = "Label3";
			this.Label3.Size = new System.Drawing.Size(104, 16);
			this.Label3.TabIndex = 14;
			this.Label3.Text = "Button 1 Status";
			// 
			// Label2
			// 
			this.Label2.Location = new System.Drawing.Point(178, 63);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(104, 16);
			this.Label2.TabIndex = 13;
			this.Label2.Text = "Button 0 Status";
			// 
			// button1
			// 
			this.button1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.button1.Location = new System.Drawing.Point(146, 87);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(24, 24);
			this.button1.TabIndex = 12;
			// 
			// button0
			// 
			this.button0.BackColor = System.Drawing.SystemColors.Control;
			this.button0.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.button0.Location = new System.Drawing.Point(146, 55);
			this.button0.Name = "button0";
			this.button0.Size = new System.Drawing.Size(24, 24);
			this.button0.TabIndex = 11;
			// 
			// cmdConnect
			// 
			this.cmdConnect.Location = new System.Drawing.Point(210, 15);
			this.cmdConnect.Name = "cmdConnect";
			this.cmdConnect.Size = new System.Drawing.Size(72, 24);
			this.cmdConnect.TabIndex = 10;
			this.cmdConnect.Text = "Connect";
			this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
			// 
			// txtAddress
			// 
			this.txtAddress.Location = new System.Drawing.Point(106, 15);
			this.txtAddress.Name = "txtAddress";
			this.txtAddress.Size = new System.Drawing.Size(96, 20);
			this.txtAddress.TabIndex = 9;
			this.txtAddress.Text = "192.168.1.100";
			// 
			// Label1
			// 
			this.Label1.Location = new System.Drawing.Point(10, 15);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(88, 16);
			this.Label1.TabIndex = 8;
			this.Label1.Text = "ETH32 Address:";
			// 
			// frmButtons
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 126);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.Label4,
																		  this.Label3,
																		  this.Label2,
																		  this.button1,
																		  this.button0,
																		  this.cmdConnect,
																		  this.txtAddress,
																		  this.Label1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(300, 160);
			this.MinimumSize = new System.Drawing.Size(300, 160);
			this.Name = "frmButtons";
			this.Text = "ETH32 Buttons Example";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.frmButtons_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new frmButtons());
		}


		private void Eth32EventHandler(Object s, EventArgs e)
		{
			// If this function was called by Eth32 event processing, then
			// s (sender) is actually the Eth32 object that received the event
			// and e is actually an Eth32EventArgs object. But to be safe,
			// we’ll make sure by using this code:
			Eth32 sender;
			Eth32EventArgs args;
			sender = s as Eth32;
			if(sender == null)
			{
				// s wasn’t really an Eth32 object - quit
				return;
			}

            // Since we're going to be working with Forms controls, test to see if 
            // we need to use Invoke (because we're running on a different thread)
            // and if so, call ourselves (this function) using Invoke.  At that point,
            // this same test will indicate that we don't need to use Invoke and it will
            // actually do what we wanted.
            if (this.InvokeRequired)
            {
                EventHandler d = new EventHandler(Eth32EventHandler);
                this.Invoke(d, new object[] { s, e });
                return; // Go no further if we are invoking ourselves
            }

			args = e as Eth32EventArgs;
			if(args == null)
			{
				// The arguments weren’t really Eth32EventArgs - quit
				return;
			}
			// You may now easily access the Eth32 object by using sender and
			// the event information by using args.event_info

			switch(args.event_info.id)
			{
				case ID_BUTTON0:
					if(args.event_info.val != 0)
					{
						// Button has just been released
						button0.BackColor = System.Drawing.SystemColors.Control; // Set status square color to same as form 
						dev.Led[0] = false; // Turn off LED on ETH32
					}
					else
					{
						// Button has just been pressed
						button0.BackColor = System.Drawing.Color.Red; // Set status square color to Red
						dev.Led[0] = true; // Turn on LED on ETH32
					}

					break;
				case ID_BUTTON1:
					if(args.event_info.val != 0)
					{
						// Button has just been released
						button1.BackColor = System.Drawing.SystemColors.Control; // Set status square color to same as form 
						dev.Led[1] = false; // Turn off LED on ETH32
					}
					else
					{
						// Button has just been pressed
						button1.BackColor = System.Drawing.Color.Red; // Set status square color to Red
						dev.Led[1] = true; // Turn on LED on ETH32
					}

					break;
			}
		}

		private void cmdConnect_Click(object sender, System.EventArgs e)
		{
			// If we're already connected, disconnect and create a new object
			if(dev.Connected)
			{
				dev.Disconnect();
				dev = new Eth32();
			}

			try
			{
				// Connect, use a 5 second timeout
				dev.Connect(txtAddress.Text, Eth32.DefaultPort, 5000);

				// Enable pullup resistors on pushbutton lines
				dev.OutputBit(0, 0, 1);  // Port 0, bit 0
				dev.OutputBit(0, 1, 1);  // Port 0, bit 1

				// Enable events on both pushbutton lines
				dev.EnableEvent(Eth32EventType.Digital, 0, 0, ID_BUTTON0);
				dev.EnableEvent(Eth32EventType.Digital, 0, 1, ID_BUTTON1);

				dev.HardwareEvent += new EventHandler(Eth32EventHandler);

				MessageBox.Show("Successfully connected to ETH32 device.");
			}
			catch (Eth32Exception etherr)
			{
				MessageBox.Show("Error connecting or configuring the ETH32: " + Eth32.ErrorString (etherr.ErrorCode));
			}
		}

		private void frmButtons_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
            // When this form closes, close the connection to the ETH32 as well
			if(dev != null)
			{
				// dev is at least instantiated
				if(dev.Connected)
				{
					dev.Disconnect();
				}
			}
		}

	}
}
