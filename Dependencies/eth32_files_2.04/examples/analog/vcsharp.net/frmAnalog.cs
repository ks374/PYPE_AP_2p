using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using WinfordEthIO;

namespace Analog
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class frmAnalog : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		internal System.Windows.Forms.Button cmdConnect;
		internal System.Windows.Forms.TextBox txtAddress;
		internal System.Windows.Forms.Label Label1;
		private System.Windows.Forms.TrackBar trackHiMark;
		private System.Windows.Forms.TrackBar trackLoMark;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label lblOutline;
		private System.Windows.Forms.Label lblGraph;
		private System.Windows.Forms.CheckBox chkPoll;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label lblEventHigh;
		private System.Windows.Forms.Label lblEventLow;
		private System.Timers.Timer timerPoll;

		Eth32 dev = new Eth32();
		int hicount=0;
		int locount=0;

		public frmAnalog()
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
			this.cmdConnect = new System.Windows.Forms.Button();
			this.txtAddress = new System.Windows.Forms.TextBox();
			this.Label1 = new System.Windows.Forms.Label();
			this.trackHiMark = new System.Windows.Forms.TrackBar();
			this.trackLoMark = new System.Windows.Forms.TrackBar();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.lblOutline = new System.Windows.Forms.Label();
			this.lblGraph = new System.Windows.Forms.Label();
			this.chkPoll = new System.Windows.Forms.CheckBox();
			this.lblEventHigh = new System.Windows.Forms.Label();
			this.lblEventLow = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.timerPoll = new System.Timers.Timer();
			((System.ComponentModel.ISupportInitialize)(this.trackHiMark)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackLoMark)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.timerPoll)).BeginInit();
			this.SuspendLayout();
			// 
			// cmdConnect
			// 
			this.cmdConnect.Location = new System.Drawing.Point(208, 16);
			this.cmdConnect.Name = "cmdConnect";
			this.cmdConnect.Size = new System.Drawing.Size(72, 24);
			this.cmdConnect.TabIndex = 13;
			this.cmdConnect.Text = "Connect";
			this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
			// 
			// txtAddress
			// 
			this.txtAddress.Location = new System.Drawing.Point(104, 16);
			this.txtAddress.Name = "txtAddress";
			this.txtAddress.Size = new System.Drawing.Size(96, 20);
			this.txtAddress.TabIndex = 12;
			this.txtAddress.Text = "192.168.1.100";
			// 
			// Label1
			// 
			this.Label1.Location = new System.Drawing.Point(8, 16);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(88, 16);
			this.Label1.TabIndex = 11;
			this.Label1.Text = "ETH32 Address:";
			// 
			// trackHiMark
			// 
			this.trackHiMark.AutoSize = false;
			this.trackHiMark.Location = new System.Drawing.Point(120, 64);
			this.trackHiMark.Maximum = 255;
			this.trackHiMark.Name = "trackHiMark";
			this.trackHiMark.Size = new System.Drawing.Size(272, 24);
			this.trackHiMark.TabIndex = 14;
			this.trackHiMark.TickStyle = System.Windows.Forms.TickStyle.None;
			this.trackHiMark.Value = 130;
			this.trackHiMark.ValueChanged += new System.EventHandler(this.trackHiMark_ValueChanged);
			// 
			// trackLoMark
			// 
			this.trackLoMark.AutoSize = false;
			this.trackLoMark.Location = new System.Drawing.Point(120, 96);
			this.trackLoMark.Maximum = 255;
			this.trackLoMark.Name = "trackLoMark";
			this.trackLoMark.Size = new System.Drawing.Size(272, 25);
			this.trackLoMark.TabIndex = 15;
			this.trackLoMark.TickStyle = System.Windows.Forms.TickStyle.None;
			this.trackLoMark.Value = 80;
			this.trackLoMark.ValueChanged += new System.EventHandler(this.trackLoMark_ValueChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(136, 16);
			this.label2.TabIndex = 16;
			this.label2.Text = "Analog Event Thresholds";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(48, 64);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 16);
			this.label3.TabIndex = 17;
			this.label3.Text = "Hi-Mark:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(48, 96);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(72, 16);
			this.label4.TabIndex = 18;
			this.label4.Text = "Lo-Mark:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 128);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(112, 16);
			this.label5.TabIndex = 20;
			this.label5.Text = "Analog Reading:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblOutline
			// 
			this.lblOutline.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblOutline.Location = new System.Drawing.Point(128, 128);
			this.lblOutline.Name = "lblOutline";
			this.lblOutline.Size = new System.Drawing.Size(256, 16);
			this.lblOutline.TabIndex = 21;
			// 
			// lblGraph
			// 
			this.lblGraph.BackColor = System.Drawing.Color.Red;
			this.lblGraph.Location = new System.Drawing.Point(128, 128);
			this.lblGraph.Name = "lblGraph";
			this.lblGraph.Size = new System.Drawing.Size(0, 16);
			this.lblGraph.TabIndex = 22;
			// 
			// chkPoll
			// 
			this.chkPoll.Checked = true;
			this.chkPoll.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkPoll.Location = new System.Drawing.Point(136, 152);
			this.chkPoll.Name = "chkPoll";
			this.chkPoll.Size = new System.Drawing.Size(168, 16);
			this.chkPoll.TabIndex = 23;
			this.chkPoll.Text = "Monitor (poll) analog reading";
			this.chkPoll.CheckedChanged += new System.EventHandler(this.chkPoll_CheckedChanged);
			// 
			// lblEventHigh
			// 
			this.lblEventHigh.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblEventHigh.Location = new System.Drawing.Point(216, 184);
			this.lblEventHigh.Name = "lblEventHigh";
			this.lblEventHigh.Size = new System.Drawing.Size(48, 24);
			this.lblEventHigh.TabIndex = 24;
			this.lblEventHigh.Text = "0";
			this.lblEventHigh.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblEventLow
			// 
			this.lblEventLow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblEventLow.Location = new System.Drawing.Point(216, 216);
			this.lblEventLow.Name = "lblEventLow";
			this.lblEventLow.Size = new System.Drawing.Size(48, 24);
			this.lblEventLow.TabIndex = 25;
			this.lblEventLow.Text = "0";
			this.lblEventLow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label8
			// 
			this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label8.Location = new System.Drawing.Point(32, 176);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(88, 16);
			this.label8.TabIndex = 26;
			this.label8.Text = "Analog Events:";
			this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(32, 200);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(112, 56);
			this.label9.TabIndex = 27;
			this.label9.Text = "(box highlights and number increases when event is received)";
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(152, 192);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(48, 16);
			this.label10.TabIndex = 28;
			this.label10.Text = "High:";
			this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(152, 224);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(48, 16);
			this.label11.TabIndex = 29;
			this.label11.Text = "Low:";
			this.label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// timerPoll
			// 
			this.timerPoll.Enabled = true;
			this.timerPoll.SynchronizingObject = this;
			this.timerPoll.Elapsed += new System.Timers.ElapsedEventHandler(this.timerPoll_Elapsed);
			// 
			// frmAnalog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(402, 266);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label11,
																		  this.label10,
																		  this.label9,
																		  this.label8,
																		  this.lblEventLow,
																		  this.lblEventHigh,
																		  this.chkPoll,
																		  this.lblGraph,
																		  this.lblOutline,
																		  this.label5,
																		  this.label4,
																		  this.label3,
																		  this.label2,
																		  this.trackLoMark,
																		  this.trackHiMark,
																		  this.cmdConnect,
																		  this.txtAddress,
																		  this.Label1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "frmAnalog";
			this.Text = "ETH32 Analog Example";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.frmAnalog_Closing);
			((System.ComponentModel.ISupportInitialize)(this.trackHiMark)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackLoMark)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.timerPoll)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new frmAnalog());
		}


		private void Eth32EventHandler(Object s, EventArgs e)
		{
			// If this function was called by Eth32 event processing, then
			// s (sender) is actually the Eth32 object that received the event
			// and e is actually an Eth32EventArgs object. But to be safe,
			// we’ll make sure by using this code:
			Eth32 sender;
			Eth32EventArgs args;

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

			sender = s as Eth32;
			if(sender == null)
			{
				// s wasn’t really an Eth32 object - quit
				return;
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
				case 100: /* We used an ID of 100 when enabling the analog event */
					if(args.event_info.direction>0)
					{
						// The signal exceeded the hi-mark
						hicount++; // Increment number of events we've had
						lblEventHigh.Text=Convert.ToString(hicount); // And update the label
						lblEventHigh.BackColor=System.Drawing.Color.Yellow;
						lblEventLow.BackColor=System.Drawing.SystemColors.Control;
					}
					else
					{
						// The signal went below the lo-mark
						locount++; // Increment number of events we've had
						lblEventLow.Text=Convert.ToString(locount); // And update the label
						lblEventLow.BackColor=System.Drawing.Color.Yellow;
						lblEventHigh.BackColor=System.Drawing.SystemColors.Control;
					}

					break;
			}
		}


		private void cmdConnect_Click(object sender, System.EventArgs e)
		{
			// If we're already connected, disconnect
			if(dev.Connected)
			{
				dev.Disconnect();
				dev = new Eth32();
			}

			try
			{
				// Connect, use a 5 second timeout
				dev.Connect(txtAddress.Text, Eth32.DefaultPort, 5000);


				// Use internal 5V voltage reference
				dev.AnalogReference=Eth32AnalogReference.Internal;

				// Enable Analog to Digital Convertor
				dev.AnalogState=Eth32AnalogState.Enabled;


				// Create the initial analog event definition based on the slide bars
				updateEventDefinition();

				// Enable analog event
				dev.EnableEvent(Eth32EventType.Analog, 0, 0, 100); // Bank 0, Logical Channel 0, ID 100


				// Register our event handler function
				dev.HardwareEvent += new EventHandler(Eth32EventHandler);

				MessageBox.Show("Successfully connected to ETH32 device.");
			}
			catch (Eth32Exception etherr)
			{
				MessageBox.Show("Error connecting or configuring the ETH32: " + Eth32.ErrorString (etherr.ErrorCode));
			}

		}

		private void updateEventDefinition()
		{
			// This function is called when either of the analog event definition
			// trackbars on the form are updated.

			try
			{
				// First make sure the Hi-mark is higher than the lo-mark.
				if(trackHiMark.Value <= trackLoMark.Value)
				{
					// HiMark isn't higher - we need to fix it.
					// See whether LoMark is maxed out
					if(trackLoMark.Value >= trackLoMark.Maximum)
					{
						// It's maxed out, so set HiMark to max and LoMark to one under
						trackHiMark.Value=trackHiMark.Maximum;
						trackLoMark.Value = trackHiMark.Value-1;
					}
					else
					{
						// It's not maxed out, so set HiMark to one over it.
						trackHiMark.Value=trackLoMark.Value+1;
					}
				}

				// Clear the backgrounds on both event counter labels to show that 
				// a new event definition has been set.
				lblEventHigh.BackColor=System.Drawing.SystemColors.Control;
				lblEventLow.BackColor=System.Drawing.SystemColors.Control;


				if(dev.Connected==false)
					return; // Go no further if we're not connected

				// Now update the event definition.
				// The trackbars range in value from 0 to 255, so we can pass their 
				// value directly into the method that defines the analog event.
				// (Remember that the analog events use the eight most significant 
				//  bits of the analog reading (0-255), not all 10 bits (0-1023)).
				dev.SetAnalogEventDef(0, 0, trackLoMark.Value, trackHiMark.Value, Eth32AnalogEvtDef.Low);
			}
			catch (Eth32Exception etherr)
			{
				MessageBox.Show("Error configuring the ETH32: " + Eth32.ErrorString (etherr.ErrorCode));
			}
		}


		private void trackHiMark_ValueChanged(object sender, System.EventArgs e)
		{
			// Call a helper function to handle the new event definition
			updateEventDefinition();
		}

		private void trackLoMark_ValueChanged(object sender, System.EventArgs e)
		{
			// Call a helper function to handle the new event definition
			updateEventDefinition();
		}

		private void chkPoll_CheckedChanged(object sender, System.EventArgs e)
		{
			// The checkbox has been checked or unchecked - update the timer 
			// accordingly
			if(chkPoll.Checked)
				timerPoll.Enabled = true;
			else
				timerPoll.Enabled = false;

		}

		private void timerPoll_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			// This event fires periodically when the timer is enabled

			try
			{
				if(dev != null && dev.Connected)
				{
					// We use a label with a red background as a very 
					// primitive way of creating a bar graph.
					// Alter the width of the label based on the analog reading.
					// Calculate the width based on a maximum width, which is the 
					// width of the label with the black outline.
					lblGraph.Width = (int)( ((double)dev.InputAnalog(0) / 1023.0) * lblOutline.Width);
				}
			}
			catch (Eth32Exception etherr)
			{
				MessageBox.Show("Error reading an analog value from the ETH32: " + Eth32.ErrorString (etherr.ErrorCode));
			}

		}

		private void frmAnalog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
