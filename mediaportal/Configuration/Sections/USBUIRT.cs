/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using MediaPortal.IR;
using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration.Sections
{
	public class USBUIRT : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private MediaPortal.UserInterface.Controls.MPCheckBox inputCheckBox;
		private MediaPortal.UserInterface.Controls.MPCheckBox outputCheckBox;
		private MediaPortal.UserInterface.Controls.MPCheckBox digitCheckBox;
		private MediaPortal.UserInterface.Controls.MPCheckBox enterCheckBox;
		private System.Windows.Forms.Button internalCommandsButton;
		private System.Windows.Forms.Button tunerCommandsButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblUSBUIRTVersion;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel SettingsPnl;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown commandRepeatNumUpDn;
		private System.Windows.Forms.NumericUpDown interCommandDelayNumUpDn;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox testSendIrTxtBox;
		private System.Windows.Forms.Button testSendIrBtn;
        private GroupBox groupBox3;
		private System.ComponentModel.IContainer components = null;

        #region Properties

        public override bool CanActivate
        {
            get { return MediaPortal.IR.USBUIRT.Instance != null; }
        }

        #endregion

        public USBUIRT() : this("USBUIRT")
		{
		}

		public USBUIRT(string name) : base(name)
		{
			InitializeComponent();
			
			MediaPortal.IR.USBUIRT.Create(new MediaPortal.IR.USBUIRT.OnRemoteCommand(OnRemoteCommand));

			if(MediaPortal.IR.USBUIRT.Instance == null)
				return;

			lblUSBUIRTVersion.Text=MediaPortal.IR.USBUIRT.Instance.GetVersions();
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

				MediaPortal.IR.USBUIRT.Instance.Dispose();
			}
			base.Dispose( disposing );
		}

		public override void LoadSettings()
		{
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				try
				{
					Log.Write("USBUIRT: Setting configuration control values");

					inputCheckBox.Checked	= xmlreader.GetValueAsBool("USBUIRT", "internal", false) ;
					outputCheckBox.Checked	= xmlreader.GetValueAsBool("USBUIRT", "external", false) ;
					digitCheckBox.Checked	= xmlreader.GetValueAsBool("USBUIRT", "is3digit", false) ;
					enterCheckBox.Checked	= xmlreader.GetValueAsBool("USBUIRT", "needsenter", false) ; 
	
					int repeatCount = xmlreader.GetValueAsInt("USBUIRT", "repeatcount", 2) ;
					commandRepeatNumUpDn.Value = repeatCount;

					int interCommandDelay = xmlreader.GetValueAsInt("USBUIRT", "commanddelay", 100) ;
					interCommandDelayNumUpDn.Value = interCommandDelay;

					EnableTestIrControls();

					if(MediaPortal.IR.USBUIRT.Instance == null)
						return;

					MediaPortal.IR.USBUIRT.Instance.CommandRepeatCount = repeatCount;
					MediaPortal.IR.USBUIRT.Instance.InterCommandDelay = interCommandDelay;
				}

				catch(Exception ex)
				{
					Log.Write("USBUIRT: Setting control values failed: " + ex.Message);
				}
			}
		}

		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("USBUIRT", "internal", inputCheckBox.Checked );
				xmlwriter.SetValueAsBool("USBUIRT", "external", outputCheckBox.Checked );
				xmlwriter.SetValueAsBool("USBUIRT", "is3digit", digitCheckBox.Checked );
				xmlwriter.SetValueAsBool("USBUIRT", "needsenter", enterCheckBox.Checked );	
		
				xmlwriter.SetValue("USBUIRT", "repeatcount", commandRepeatNumUpDn.Value);	
				xmlwriter.SetValue("USBUIRT", "commanddelay", interCommandDelayNumUpDn.Value);	
			}			
		}


		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(USBUIRT));
            this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.SettingsPnl = new System.Windows.Forms.Panel();
            this.testSendIrBtn = new System.Windows.Forms.Button();
            this.testSendIrTxtBox = new System.Windows.Forms.TextBox();
            this.interCommandDelayNumUpDn = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.commandRepeatNumUpDn = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.tunerCommandsButton = new System.Windows.Forms.Button();
            this.internalCommandsButton = new System.Windows.Forms.Button();
            this.enterCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.digitCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.outputCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.inputCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.lblUSBUIRTVersion = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SettingsPnl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.interCommandDelayNumUpDn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.commandRepeatNumUpDn)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.SettingsPnl);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(472, 219);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // SettingsPnl
            // 
            this.SettingsPnl.Controls.Add(this.testSendIrBtn);
            this.SettingsPnl.Controls.Add(this.testSendIrTxtBox);
            this.SettingsPnl.Controls.Add(this.interCommandDelayNumUpDn);
            this.SettingsPnl.Controls.Add(this.label6);
            this.SettingsPnl.Controls.Add(this.commandRepeatNumUpDn);
            this.SettingsPnl.Controls.Add(this.label5);
            this.SettingsPnl.Controls.Add(this.tunerCommandsButton);
            this.SettingsPnl.Controls.Add(this.internalCommandsButton);
            this.SettingsPnl.Controls.Add(this.enterCheckBox);
            this.SettingsPnl.Controls.Add(this.digitCheckBox);
            this.SettingsPnl.Controls.Add(this.outputCheckBox);
            this.SettingsPnl.Controls.Add(this.inputCheckBox);
            this.SettingsPnl.Location = new System.Drawing.Point(8, 8);
            this.SettingsPnl.Name = "SettingsPnl";
            this.SettingsPnl.Size = new System.Drawing.Size(456, 198);
            this.SettingsPnl.TabIndex = 9;
            // 
            // testSendIrBtn
            // 
            this.testSendIrBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.testSendIrBtn.Location = new System.Drawing.Point(288, 133);
            this.testSendIrBtn.Name = "testSendIrBtn";
            this.testSendIrBtn.Size = new System.Drawing.Size(160, 23);
            this.testSendIrBtn.TabIndex = 14;
            this.testSendIrBtn.Text = "Test settop box control";
            this.testSendIrBtn.Click += new System.EventHandler(this.testSendIrBtn_Click);
            // 
            // testSendIrTxtBox
            // 
            this.testSendIrTxtBox.AcceptsReturn = true;
            this.testSendIrTxtBox.Location = new System.Drawing.Point(288, 156);
            this.testSendIrTxtBox.Name = "testSendIrTxtBox";
            this.testSendIrTxtBox.Size = new System.Drawing.Size(160, 20);
            this.testSendIrTxtBox.TabIndex = 13;
            this.testSendIrTxtBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.testSendIrTxtBox_KeyPress);
            // 
            // interCommandDelayNumUpDn
            // 
            this.interCommandDelayNumUpDn.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.interCommandDelayNumUpDn.Location = new System.Drawing.Point(153, 106);
            this.interCommandDelayNumUpDn.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.interCommandDelayNumUpDn.Name = "interCommandDelayNumUpDn";
            this.interCommandDelayNumUpDn.Size = new System.Drawing.Size(48, 20);
            this.interCommandDelayNumUpDn.TabIndex = 12;
            this.interCommandDelayNumUpDn.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(24, 106);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(121, 20);
            this.label6.TabIndex = 11;
            this.label6.Text = "Inter-command delay ms";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // commandRepeatNumUpDn
            // 
            this.commandRepeatNumUpDn.Location = new System.Drawing.Point(153, 82);
            this.commandRepeatNumUpDn.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.commandRepeatNumUpDn.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.commandRepeatNumUpDn.Name = "commandRepeatNumUpDn";
            this.commandRepeatNumUpDn.Size = new System.Drawing.Size(48, 20);
            this.commandRepeatNumUpDn.TabIndex = 10;
            this.commandRepeatNumUpDn.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(24, 82);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(117, 20);
            this.label5.TabIndex = 9;
            this.label5.Text = "Command repeat count";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tunerCommandsButton
            // 
            this.tunerCommandsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tunerCommandsButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.tunerCommandsButton.Location = new System.Drawing.Point(288, 53);
            this.tunerCommandsButton.Name = "tunerCommandsButton";
            this.tunerCommandsButton.Size = new System.Drawing.Size(160, 23);
            this.tunerCommandsButton.TabIndex = 8;
            this.tunerCommandsButton.Text = "Learn settop box commands";
            this.tunerCommandsButton.Click += new System.EventHandler(this.tunerCommandsButton_Click);
            // 
            // internalCommandsButton
            // 
            this.internalCommandsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.internalCommandsButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.internalCommandsButton.Location = new System.Drawing.Point(288, 16);
            this.internalCommandsButton.Name = "internalCommandsButton";
            this.internalCommandsButton.Size = new System.Drawing.Size(160, 23);
            this.internalCommandsButton.TabIndex = 7;
            this.internalCommandsButton.Text = "Learn Media Portal commands";
            this.internalCommandsButton.Click += new System.EventHandler(this.internalCommandsButton_Click);
            // 
            // enterCheckBox
            // 
            this.enterCheckBox.Enabled = false;
            this.enterCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.enterCheckBox.Location = new System.Drawing.Point(24, 156);
            this.enterCheckBox.Name = "enterCheckBox";
            this.enterCheckBox.Size = new System.Drawing.Size(192, 16);
            this.enterCheckBox.TabIndex = 3;
            this.enterCheckBox.Text = "Send \'Enter\' for changing channels";
            this.enterCheckBox.CheckedChanged += new System.EventHandler(this.enterCheckBox_CheckedChanged);
            // 
            // digitCheckBox
            // 
            this.digitCheckBox.Enabled = false;
            this.digitCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.digitCheckBox.Location = new System.Drawing.Point(24, 140);
            this.digitCheckBox.Name = "digitCheckBox";
            this.digitCheckBox.Size = new System.Drawing.Size(176, 16);
            this.digitCheckBox.TabIndex = 2;
            this.digitCheckBox.Text = "Use 3 digits for channel selection";
            this.digitCheckBox.CheckedChanged += new System.EventHandler(this.digitCheckBox_CheckedChanged);
            // 
            // outputCheckBox
            // 
            this.outputCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.outputCheckBox.Location = new System.Drawing.Point(8, 56);
            this.outputCheckBox.Name = "outputCheckBox";
            this.outputCheckBox.Size = new System.Drawing.Size(208, 16);
            this.outputCheckBox.TabIndex = 1;
            this.outputCheckBox.Text = "Let Media Portal control your settopbox";
            this.outputCheckBox.CheckedChanged += new System.EventHandler(this.outputCheckBox_CheckedChanged);
            // 
            // inputCheckBox
            // 
            this.inputCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.inputCheckBox.Location = new System.Drawing.Point(8, 16);
            this.inputCheckBox.Name = "inputCheckBox";
            this.inputCheckBox.Size = new System.Drawing.Size(208, 16);
            this.inputCheckBox.TabIndex = 0;
            this.inputCheckBox.Text = "Use your remote to control Media Portal";
            this.inputCheckBox.CheckedChanged += new System.EventHandler(this.inputCheckBox_CheckedChanged);
            // 
            // lblUSBUIRTVersion
            // 
            this.lblUSBUIRTVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUSBUIRTVersion.Location = new System.Drawing.Point(200, 29);
            this.lblUSBUIRTVersion.Name = "lblUSBUIRTVersion";
            this.lblUSBUIRTVersion.Size = new System.Drawing.Size(256, 32);
            this.lblUSBUIRTVersion.TabIndex = 6;
            this.lblUSBUIRTVersion.Text = "Version";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 16);
            this.label1.TabIndex = 5;
            this.label1.Text = "USBUIRT version detected:";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 23);
            this.label4.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.linkLabel1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox2.Location = new System.Drawing.Point(0, 300);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(472, 104);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "General Information";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 16);
            this.label3.TabIndex = 1;
            this.label3.Text = "More information:";
            // 
            // linkLabel1
            // 
            this.linkLabel1.Location = new System.Drawing.Point(112, 72);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(120, 16);
            this.linkLabel1.TabIndex = 2;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://www.usbuirt.com";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(16, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(448, 40);
            this.label2.TabIndex = 0;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblUSBUIRTVersion);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox3.Location = new System.Drawing.Point(0, 223);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(472, 73);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Status";
            // 
            // USBUIRT
            // 
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "USBUIRT";
            this.Size = new System.Drawing.Size(472, 408);
            this.Load += new System.EventHandler(this.USBUIRT_Load);
            this.groupBox1.ResumeLayout(false);
            this.SettingsPnl.ResumeLayout(false);
            this.SettingsPnl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.interCommandDelayNumUpDn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.commandRepeatNumUpDn)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void ShowDriverOfflineMsg()
		{
			MessageBox.Show(this, "USBUIRT Driver is not loaded.  Please ensure the USBUIRT is\t\r\nproperly connected and drivers are installed.", 
				"USBUIRT", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}

		private void internalCommandsButton_Click(object sender, System.EventArgs e)
		{
			if(!GetUsbUirtDriverStatusOK())
			{
				ShowDriverOfflineMsg();
				return;
			}

			MediaPortal.IR.USBUIRTLearnForm learnForm = new MediaPortal.IR.USBUIRTLearnForm(MediaPortal.IR.USBUIRTLearnForm.LearnType.MediaPortalCommands);
			learnForm.ShowDialog(this);
		}

		private void tunerCommandsButton_Click(object sender, System.EventArgs e)
		{
			if(!GetUsbUirtDriverStatusOK())
			{
				ShowDriverOfflineMsg();
				return;
			}

			MediaPortal.IR.USBUIRTLearnForm learnForm = new MediaPortal.IR.USBUIRTLearnForm(MediaPortal.IR.USBUIRTLearnForm.LearnType.SetTopBoxCommands);
			learnForm.ShowDialog(this);
		}

		private void outputCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			bool setTopControl = outputCheckBox.Checked;
			digitCheckBox.Enabled = setTopControl;
			enterCheckBox.Enabled = setTopControl;
			tunerCommandsButton.Enabled = setTopControl;
			commandRepeatNumUpDn.Enabled = setTopControl;
			interCommandDelayNumUpDn.Enabled = setTopControl;
			testSendIrBtn.Enabled = setTopControl;
			testSendIrTxtBox.Enabled = setTopControl;

			MediaPortal.IR.USBUIRT.Instance.TransmitEnabled = outputCheckBox.Checked;
		}

		private void inputCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			internalCommandsButton.Enabled = inputCheckBox.Checked;
			MediaPortal.IR.USBUIRT.Instance.ReceiveEnabled = inputCheckBox.Checked;
		}

		private void OnRemoteCommand(object command)
		{
		}

		private bool GetUsbUirtDriverStatusOK()
		{
			bool driverStatusOK = MediaPortal.IR.USBUIRT.Instance.IsUsbUirtLoaded;

			// Check if the driver is loaded...
			if(driverStatusOK)
			{
				// The driver is loaded but is the USBUIRT connected?
				driverStatusOK = MediaPortal.IR.USBUIRT.Instance.IsUsbUirtConnected;
			}

			else
			{
				driverStatusOK = MediaPortal.IR.USBUIRT.Instance.Reconnect();
			}

            lblUSBUIRTVersion.Text = MediaPortal.IR.USBUIRT.Instance.GetVersions();
            return driverStatusOK;
		}

		private void USBUIRT_Load(object sender, System.EventArgs e)
		{
			outputCheckBox_CheckedChanged(null, null);
			inputCheckBox_CheckedChanged(null, null);

			SettingsPnl.BringToFront();
		}

		private void testSendIrTxtBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if((e.KeyChar >= '0' && e.KeyChar <= '9')  || e.KeyChar == 8) // keys 0-9 and the BackSpace key
			{
				// do nothing...
			}

			else if(e.KeyChar == 13) // Enter key
			{
				testSendIrBtn_Click(null, null);
				e.Handled = true;
			}

			else
			{
				e.Handled = true;
			}
		}

		private void testSendIrBtn_Click(object sender, System.EventArgs e)
		{
            if (!GetUsbUirtDriverStatusOK())
            {
                ShowDriverOfflineMsg();
                return;
            }

			if(testSendIrTxtBox.Text.Length == 0)
			{
				System.Windows.Forms.MessageBox.Show(this, "No channel number entered.  Nothing to send.\t", "USBUIRT", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				testSendIrTxtBox.Focus();
			}

			else if(MediaPortal.IR.USBUIRT.Instance.TunerCodesLoaded)
			{
				MediaPortal.IR.USBUIRT.Instance.CommandRepeatCount = (int)commandRepeatNumUpDn.Value;
				MediaPortal.IR.USBUIRT.Instance.InterCommandDelay = (int)interCommandDelayNumUpDn.Value;
				MediaPortal.IR.USBUIRT.Instance.Is3Digit = digitCheckBox.Checked;
				MediaPortal.IR.USBUIRT.Instance.NeedsEnter = enterCheckBox.Checked;
				
				if(MediaPortal.IR.USBUIRT.Instance.Is3Digit && testSendIrTxtBox.Text.Length > 3)
				{
					string msg = string.Format("Channel number is longer than 3 digits.  Either disable '{0}'\t\r\nor enter a channel number less than 3 digits long.", digitCheckBox.Text);
					System.Windows.Forms.MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}

				else
					MediaPortal.IR.USBUIRT.Instance.ChangeTunerChannel(testSendIrTxtBox.Text, true);
			}

			else
			{
				string msg = "No set top box codes loaded--nothing to send.\t\r\n Please ensure you have completed IR training.";
				MessageBox.Show(this, msg, "USBUIRT", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void EnableTestIrControls()
		{
			commandRepeatNumUpDn.Enabled = MediaPortal.IR.USBUIRT.Instance.TunerCodesLoaded;
			interCommandDelayNumUpDn.Enabled = MediaPortal.IR.USBUIRT.Instance.TunerCodesLoaded;
		}

		private void digitCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			MediaPortal.IR.USBUIRT.Instance.Is3Digit = digitCheckBox.Checked;
		}

		private void enterCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			MediaPortal.IR.USBUIRT.Instance.NeedsEnter = enterCheckBox.Checked;
		}

		private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(linkLabel1.Text);
		}

		private void SkipCodeBtn_Click(object sender, System.EventArgs e)
		{
			MediaPortal.IR.USBUIRT.Instance.SkipLearnForCurrentCode = true;
		}
	}
}