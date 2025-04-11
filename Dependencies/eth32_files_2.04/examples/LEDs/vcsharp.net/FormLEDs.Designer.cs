namespace LEDs
{
    partial class FormLEDs
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.cmbETH32 = new System.Windows.Forms.ComboBox();
            this.cmdDetect = new System.Windows.Forms.Button();
            this.cmdConnect = new System.Windows.Forms.Button();
            this.cmdRefresh = new System.Windows.Forms.Button();
            this.led0 = new System.Windows.Forms.CheckBox();
            this.led1 = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "ETH32 Address:";
            // 
            // cmbETH32
            // 
            this.cmbETH32.FormattingEnabled = true;
            this.cmbETH32.Location = new System.Drawing.Point(101, 8);
            this.cmbETH32.Name = "cmbETH32";
            this.cmbETH32.Size = new System.Drawing.Size(121, 21);
            this.cmbETH32.TabIndex = 1;
            // 
            // cmdDetect
            // 
            this.cmdDetect.Location = new System.Drawing.Point(236, 10);
            this.cmdDetect.Name = "cmdDetect";
            this.cmdDetect.Size = new System.Drawing.Size(72, 25);
            this.cmdDetect.TabIndex = 2;
            this.cmdDetect.Text = "Detect";
            this.cmdDetect.UseVisualStyleBackColor = true;
            this.cmdDetect.Click += new System.EventHandler(this.cmdDetect_Click);
            // 
            // cmdConnect
            // 
            this.cmdConnect.Location = new System.Drawing.Point(236, 41);
            this.cmdConnect.Name = "cmdConnect";
            this.cmdConnect.Size = new System.Drawing.Size(72, 25);
            this.cmdConnect.TabIndex = 3;
            this.cmdConnect.Text = "Connect";
            this.cmdConnect.UseVisualStyleBackColor = true;
            this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
            // 
            // cmdRefresh
            // 
            this.cmdRefresh.Location = new System.Drawing.Point(236, 72);
            this.cmdRefresh.Name = "cmdRefresh";
            this.cmdRefresh.Size = new System.Drawing.Size(72, 25);
            this.cmdRefresh.TabIndex = 4;
            this.cmdRefresh.Text = "Refresh";
            this.cmdRefresh.UseVisualStyleBackColor = true;
            this.cmdRefresh.Click += new System.EventHandler(this.cmdRefresh_Click);
            // 
            // led0
            // 
            this.led0.AutoSize = true;
            this.led0.Location = new System.Drawing.Point(138, 54);
            this.led0.Name = "led0";
            this.led0.Size = new System.Drawing.Size(53, 17);
            this.led0.TabIndex = 5;
            this.led0.Text = "LED0";
            this.led0.UseVisualStyleBackColor = true;
            this.led0.CheckedChanged += new System.EventHandler(this.led0_CheckedChanged);
            // 
            // led1
            // 
            this.led1.AutoSize = true;
            this.led1.Location = new System.Drawing.Point(138, 80);
            this.led1.Name = "led1";
            this.led1.Size = new System.Drawing.Size(53, 17);
            this.led1.TabIndex = 6;
            this.led1.Text = "LED1";
            this.led1.UseVisualStyleBackColor = true;
            this.led1.CheckedChanged += new System.EventHandler(this.led1_CheckedChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 43);
            this.label2.TabIndex = 7;
            this.label2.Text = "Check / Uncheck boxes to turn LEDs on or off.";
            // 
            // FormLEDs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 111);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.led1);
            this.Controls.Add(this.led0);
            this.Controls.Add(this.cmdRefresh);
            this.Controls.Add(this.cmdConnect);
            this.Controls.Add(this.cmdDetect);
            this.Controls.Add(this.cmbETH32);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormLEDs";
            this.Text = "ETH32 LEDs Example";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormLEDs_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbETH32;
        private System.Windows.Forms.Button cmdDetect;
        private System.Windows.Forms.Button cmdConnect;
        private System.Windows.Forms.Button cmdRefresh;
        private System.Windows.Forms.CheckBox led0;
        private System.Windows.Forms.CheckBox led1;
        private System.Windows.Forms.Label label2;
    }
}

