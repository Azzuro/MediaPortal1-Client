using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

using System.Runtime.InteropServices;

using DShowNET;
using DShowNET.Device;

namespace MediaPortal.Configuration.Sections
{

	public class MPEG2DecAudioFilter : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton radioPCM16Bit;
		private System.Windows.Forms.RadioButton radioButtonPCM24Bit;
		private System.Windows.Forms.RadioButton radioButtonPCM32Bit;
		private System.Windows.Forms.RadioButton radioButtonIEEE;
		private System.Windows.Forms.CheckBox checkBoxNormalize;
		private System.Windows.Forms.TrackBar trackBarBoost;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.RadioButton radioButtonAC3Speakers;
		private System.Windows.Forms.RadioButton radioButtonAC3SPDIF;
		private System.Windows.Forms.CheckBox checkBoxAC3DynamicRange;
		private System.Windows.Forms.ComboBox comboBoxAC3SpeakerConfig;
		private System.Windows.Forms.CheckBox checkBoxAC3LFE;
		private System.Windows.Forms.ComboBox comboBoxDTSSpeakerConfig;
		private System.Windows.Forms.CheckBox checkBoxDTSDynamicRange;
		private System.Windows.Forms.RadioButton radioButtonDTSSPDIF;
		private System.Windows.Forms.RadioButton radioButtonDTSSpeakers;
		private System.Windows.Forms.CheckBox checkBoxDTSLFE;
		private System.Windows.Forms.CheckBox checkBoxAACDownmix;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Label label4;
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 
		/// </summary>
		public MPEG2DecAudioFilter() : this("MPEG/AC3/DTS/LPCM Audio Decoder")
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public MPEG2DecAudioFilter(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
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

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.checkBoxAACDownmix = new System.Windows.Forms.CheckBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.radioButtonAC3SPDIF = new System.Windows.Forms.RadioButton();
			this.radioButtonAC3Speakers = new System.Windows.Forms.RadioButton();
			this.checkBoxAC3LFE = new System.Windows.Forms.CheckBox();
			this.comboBoxAC3SpeakerConfig = new System.Windows.Forms.ComboBox();
			this.checkBoxAC3DynamicRange = new System.Windows.Forms.CheckBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.radioPCM16Bit = new System.Windows.Forms.RadioButton();
			this.radioButtonPCM24Bit = new System.Windows.Forms.RadioButton();
			this.radioButtonPCM32Bit = new System.Windows.Forms.RadioButton();
			this.radioButtonIEEE = new System.Windows.Forms.RadioButton();
			this.checkBoxNormalize = new System.Windows.Forms.CheckBox();
			this.trackBarBoost = new System.Windows.Forms.TrackBar();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.radioButtonDTSSPDIF = new System.Windows.Forms.RadioButton();
			this.radioButtonDTSSpeakers = new System.Windows.Forms.RadioButton();
			this.checkBoxDTSLFE = new System.Windows.Forms.CheckBox();
			this.comboBoxDTSSpeakerConfig = new System.Windows.Forms.ComboBox();
			this.checkBoxDTSDynamicRange = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBarBoost)).BeginInit();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.groupBox5);
			this.groupBox1.Controls.Add(this.groupBox4);
			this.groupBox1.Controls.Add(this.groupBox3);
			this.groupBox1.Controls.Add(this.groupBox2);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 424);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "MPEG/AC3/DTS/LPCM Audio Decoder";
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.checkBoxAACDownmix);
			this.groupBox5.Location = new System.Drawing.Point(24, 336);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(392, 72);
			this.groupBox5.TabIndex = 25;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "AAC Decoder Settings";
			// 
			// checkBoxAACDownmix
			// 
			this.checkBoxAACDownmix.Checked = true;
			this.checkBoxAACDownmix.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAACDownmix.Location = new System.Drawing.Point(32, 32);
			this.checkBoxAACDownmix.Name = "checkBoxAACDownmix";
			this.checkBoxAACDownmix.Size = new System.Drawing.Size(128, 16);
			this.checkBoxAACDownmix.TabIndex = 21;
			this.checkBoxAACDownmix.Text = "Downmix to stereo";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.radioButtonAC3SPDIF);
			this.groupBox4.Controls.Add(this.radioButtonAC3Speakers);
			this.groupBox4.Controls.Add(this.checkBoxAC3LFE);
			this.groupBox4.Controls.Add(this.comboBoxAC3SpeakerConfig);
			this.groupBox4.Controls.Add(this.checkBoxAC3DynamicRange);
			this.groupBox4.Location = new System.Drawing.Point(24, 128);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(392, 100);
			this.groupBox4.TabIndex = 24;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "AC3 Decoder Settings";
			// 
			// radioButtonAC3SPDIF
			// 
			this.radioButtonAC3SPDIF.Location = new System.Drawing.Point(16, 40);
			this.radioButtonAC3SPDIF.Name = "radioButtonAC3SPDIF";
			this.radioButtonAC3SPDIF.Size = new System.Drawing.Size(104, 16);
			this.radioButtonAC3SPDIF.TabIndex = 10;
			this.radioButtonAC3SPDIF.Text = "S/PDIF";
			this.radioButtonAC3SPDIF.CheckedChanged += new System.EventHandler(this.radioButtonAC3SPDIF_CheckedChanged);
			// 
			// radioButtonAC3Speakers
			// 
			this.radioButtonAC3Speakers.Checked = true;
			this.radioButtonAC3Speakers.Location = new System.Drawing.Point(16, 16);
			this.radioButtonAC3Speakers.Name = "radioButtonAC3Speakers";
			this.radioButtonAC3Speakers.Size = new System.Drawing.Size(136, 16);
			this.radioButtonAC3Speakers.TabIndex = 9;
			this.radioButtonAC3Speakers.TabStop = true;
			this.radioButtonAC3Speakers.Text = "Decode to speakers:";
			this.radioButtonAC3Speakers.CheckedChanged += new System.EventHandler(this.radioButtonAC3Speakers_CheckedChanged);
			// 
			// checkBoxAC3LFE
			// 
			this.checkBoxAC3LFE.Location = new System.Drawing.Point(288, 16);
			this.checkBoxAC3LFE.Name = "checkBoxAC3LFE";
			this.checkBoxAC3LFE.Size = new System.Drawing.Size(72, 16);
			this.checkBoxAC3LFE.TabIndex = 13;
			this.checkBoxAC3LFE.Text = "LFE";
			// 
			// comboBoxAC3SpeakerConfig
			// 
			this.comboBoxAC3SpeakerConfig.Items.AddRange(new object[] {
																																	"Mono",
																																	"Dual Mono",
																																	"Stereo",
																																	"Dolby Stereo",
																																	"3 Front",
																																	"2 Front + 1 Rear",
																																	"3 Front + 1 Rear",
																																	"2 Front + 2 Rear",
																																	"3 Front + 2 Rear",
																																	"Channel 1",
																																	"Channel 2",
																																	""});
			this.comboBoxAC3SpeakerConfig.Location = new System.Drawing.Point(160, 16);
			this.comboBoxAC3SpeakerConfig.Name = "comboBoxAC3SpeakerConfig";
			this.comboBoxAC3SpeakerConfig.Size = new System.Drawing.Size(121, 21);
			this.comboBoxAC3SpeakerConfig.TabIndex = 12;
			this.comboBoxAC3SpeakerConfig.Text = "Stereo";
			// 
			// checkBoxAC3DynamicRange
			// 
			this.checkBoxAC3DynamicRange.Location = new System.Drawing.Point(16, 64);
			this.checkBoxAC3DynamicRange.Name = "checkBoxAC3DynamicRange";
			this.checkBoxAC3DynamicRange.Size = new System.Drawing.Size(144, 16);
			this.checkBoxAC3DynamicRange.TabIndex = 11;
			this.checkBoxAC3DynamicRange.Text = "Dynamic Range Control";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.label4);
			this.groupBox3.Controls.Add(this.radioPCM16Bit);
			this.groupBox3.Controls.Add(this.radioButtonPCM24Bit);
			this.groupBox3.Controls.Add(this.radioButtonPCM32Bit);
			this.groupBox3.Controls.Add(this.radioButtonIEEE);
			this.groupBox3.Controls.Add(this.checkBoxNormalize);
			this.groupBox3.Controls.Add(this.trackBarBoost);
			this.groupBox3.Location = new System.Drawing.Point(24, 16);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(392, 100);
			this.groupBox3.TabIndex = 23;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "AC3/AAC/DTS/LPCM Format";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(104, 64);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(64, 23);
			this.label4.TabIndex = 7;
			this.label4.Text = "Boost:";
			// 
			// radioPCM16Bit
			// 
			this.radioPCM16Bit.Checked = true;
			this.radioPCM16Bit.Location = new System.Drawing.Point(8, 24);
			this.radioPCM16Bit.Name = "radioPCM16Bit";
			this.radioPCM16Bit.Size = new System.Drawing.Size(80, 16);
			this.radioPCM16Bit.TabIndex = 1;
			this.radioPCM16Bit.TabStop = true;
			this.radioPCM16Bit.Text = "PCM 16 bit";
			// 
			// radioButtonPCM24Bit
			// 
			this.radioButtonPCM24Bit.Location = new System.Drawing.Point(96, 24);
			this.radioButtonPCM24Bit.Name = "radioButtonPCM24Bit";
			this.radioButtonPCM24Bit.Size = new System.Drawing.Size(80, 16);
			this.radioButtonPCM24Bit.TabIndex = 2;
			this.radioButtonPCM24Bit.Text = "PCM 24 bit";
			// 
			// radioButtonPCM32Bit
			// 
			this.radioButtonPCM32Bit.Location = new System.Drawing.Point(184, 24);
			this.radioButtonPCM32Bit.Name = "radioButtonPCM32Bit";
			this.radioButtonPCM32Bit.Size = new System.Drawing.Size(80, 16);
			this.radioButtonPCM32Bit.TabIndex = 3;
			this.radioButtonPCM32Bit.Text = "PCM 32 bit";
			// 
			// radioButtonIEEE
			// 
			this.radioButtonIEEE.Location = new System.Drawing.Point(280, 24);
			this.radioButtonIEEE.Name = "radioButtonIEEE";
			this.radioButtonIEEE.Size = new System.Drawing.Size(80, 16);
			this.radioButtonIEEE.TabIndex = 4;
			this.radioButtonIEEE.Text = "IEEE float";
			// 
			// checkBoxNormalize
			// 
			this.checkBoxNormalize.Location = new System.Drawing.Point(8, 64);
			this.checkBoxNormalize.Name = "checkBoxNormalize";
			this.checkBoxNormalize.Size = new System.Drawing.Size(80, 16);
			this.checkBoxNormalize.TabIndex = 5;
			this.checkBoxNormalize.Text = "Normalize";
			// 
			// trackBarBoost
			// 
			this.trackBarBoost.Location = new System.Drawing.Point(176, 48);
			this.trackBarBoost.Maximum = 100;
			this.trackBarBoost.Name = "trackBarBoost";
			this.trackBarBoost.Size = new System.Drawing.Size(200, 45);
			this.trackBarBoost.TabIndex = 6;
			this.trackBarBoost.TickStyle = System.Windows.Forms.TickStyle.None;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.radioButtonDTSSPDIF);
			this.groupBox2.Controls.Add(this.radioButtonDTSSpeakers);
			this.groupBox2.Controls.Add(this.checkBoxDTSLFE);
			this.groupBox2.Controls.Add(this.comboBoxDTSSpeakerConfig);
			this.groupBox2.Controls.Add(this.checkBoxDTSDynamicRange);
			this.groupBox2.Location = new System.Drawing.Point(24, 232);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(392, 96);
			this.groupBox2.TabIndex = 22;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "DTS Decoder Settings";
			// 
			// radioButtonDTSSPDIF
			// 
			this.radioButtonDTSSPDIF.Location = new System.Drawing.Point(16, 40);
			this.radioButtonDTSSPDIF.Name = "radioButtonDTSSPDIF";
			this.radioButtonDTSSPDIF.Size = new System.Drawing.Size(104, 16);
			this.radioButtonDTSSPDIF.TabIndex = 16;
			this.radioButtonDTSSPDIF.Text = "S/PDIF";
			this.radioButtonDTSSPDIF.CheckedChanged += new System.EventHandler(this.radioButtonDTSSPDIF_CheckedChanged);
			// 
			// radioButtonDTSSpeakers
			// 
			this.radioButtonDTSSpeakers.Checked = true;
			this.radioButtonDTSSpeakers.Location = new System.Drawing.Point(16, 16);
			this.radioButtonDTSSpeakers.Name = "radioButtonDTSSpeakers";
			this.radioButtonDTSSpeakers.Size = new System.Drawing.Size(128, 16);
			this.radioButtonDTSSpeakers.TabIndex = 15;
			this.radioButtonDTSSpeakers.TabStop = true;
			this.radioButtonDTSSpeakers.Text = "Decode to speakers:";
			this.radioButtonDTSSpeakers.CheckedChanged += new System.EventHandler(this.radioButtonDTSSpeakers_CheckedChanged);
			// 
			// checkBoxDTSLFE
			// 
			this.checkBoxDTSLFE.Location = new System.Drawing.Point(280, 16);
			this.checkBoxDTSLFE.Name = "checkBoxDTSLFE";
			this.checkBoxDTSLFE.Size = new System.Drawing.Size(72, 16);
			this.checkBoxDTSLFE.TabIndex = 19;
			this.checkBoxDTSLFE.Text = "LFE";
			// 
			// comboBoxDTSSpeakerConfig
			// 
			this.comboBoxDTSSpeakerConfig.Items.AddRange(new object[] {
																																	"Mono",
																																	"Dual Mono",
																																	"Stereo",
																																	"3 Front",
																																	"2 Front + 1 Rear",
																																	"3 Front + 1 Rear",
																																	"2 Front + 2 Rear",
																																	"3 Front + 2 Rear"});
			this.comboBoxDTSSpeakerConfig.Location = new System.Drawing.Point(152, 16);
			this.comboBoxDTSSpeakerConfig.Name = "comboBoxDTSSpeakerConfig";
			this.comboBoxDTSSpeakerConfig.Size = new System.Drawing.Size(121, 21);
			this.comboBoxDTSSpeakerConfig.TabIndex = 18;
			this.comboBoxDTSSpeakerConfig.Text = "Stereo";
			// 
			// checkBoxDTSDynamicRange
			// 
			this.checkBoxDTSDynamicRange.Location = new System.Drawing.Point(16, 64);
			this.checkBoxDTSDynamicRange.Name = "checkBoxDTSDynamicRange";
			this.checkBoxDTSDynamicRange.Size = new System.Drawing.Size(144, 16);
			this.checkBoxDTSDynamicRange.TabIndex = 17;
			this.checkBoxDTSDynamicRange.Text = "Dynamic Range Control";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(104, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 16);
			this.label2.TabIndex = 7;
			this.label2.Text = "Boost:";
			// 
			// MPEG2DecAudioFilter
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "MPEG2DecAudioFilter";
			this.Size = new System.Drawing.Size(456, 448);
			this.groupBox1.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trackBarBoost)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		public override void LoadSettings()
		{
			RegistryKey hkcu = Registry.CurrentUser;
			RegistryKey subkey = hkcu.CreateSubKey(@"Software\Mediaportal\Mpeg Audio Filter");
			if (subkey!=null)
			{
				try
				{
					Int32 regValue=(Int32)subkey.GetValue("AAC Downmix");
					if (regValue==1) checkBoxAACDownmix.Checked=true;
					else checkBoxAACDownmix.Checked=false;

					regValue=(Int32)subkey.GetValue("AC3 Dynamic Range");
					if (regValue==1) checkBoxAC3DynamicRange.Checked=true;
					else checkBoxAC3DynamicRange.Checked=false;
					
					regValue=(Int32)subkey.GetValue("AC3 LFE");
					if (regValue==1) checkBoxAC3LFE.Checked=true;
					else checkBoxAC3LFE.Checked=false;
					
					regValue=(Int32)subkey.GetValue("DTS Dynamic Range");
					if (regValue==1) checkBoxDTSDynamicRange.Checked=true;
					else checkBoxDTSDynamicRange.Checked=false;
					
					regValue=(Int32)subkey.GetValue("DTS LFE");
					if (regValue==1) checkBoxDTSLFE.Checked=true;
					else checkBoxDTSLFE.Checked=false;
					
					regValue=(Int32)subkey.GetValue("Normalize");
					if (regValue==1) checkBoxNormalize.Checked=true;
					else checkBoxNormalize.Checked=false;

					regValue=(Int32)subkey.GetValue("AC3 Speaker Config");
					comboBoxAC3SpeakerConfig.SelectedIndex=regValue;

					regValue=(Int32)subkey.GetValue("DTS Speaker Config");
					comboBoxDTSSpeakerConfig.SelectedIndex=regValue;

					
					regValue=(Int32)subkey.GetValue("Boost");
					trackBarBoost.Value=regValue;

					regValue=(Int32)subkey.GetValue("Output Format");
					radioPCM16Bit.Checked=(regValue==0);
					radioButtonPCM24Bit.Checked=(regValue==1);
					radioButtonPCM32Bit.Checked=(regValue==2);
					radioButtonIEEE.Checked=(regValue==3);

					regValue=(Int32)subkey.GetValue("AC3Decoder");
					radioButtonAC3Speakers.Checked = (regValue==0);
					radioButtonAC3SPDIF.Checked= (regValue==1);

					
					regValue=(Int32)subkey.GetValue("DTSDecoder");
					radioButtonDTSSpeakers.Checked = (regValue==0);
					radioButtonDTSSPDIF.Checked= (regValue==1);
				}
				catch (Exception)
				{
				}
				finally
				{
					subkey.Close();
				}
			}
		}
		public override void SaveSettings()
		{
			RegistryKey hkcu = Registry.CurrentUser;
			RegistryKey subkey = hkcu.CreateSubKey(@"Software\Mediaportal\Mpeg Audio Filter");
			if (subkey!=null)
			{
				Int32 regValue;
				if (checkBoxAACDownmix.Checked) regValue=1;
				else regValue=0;
				subkey.SetValue("AAC Downmix",regValue);

				if (checkBoxAC3DynamicRange.Checked) regValue=1;
				else regValue=0;
				subkey.SetValue("AC3 Dynamic Range",regValue);

				if (checkBoxAC3LFE.Checked) regValue=1;
				else regValue=0;
				subkey.SetValue("AC3 LFE",regValue);

				if (checkBoxDTSDynamicRange.Checked) regValue=1;
				else regValue=0;
				subkey.SetValue("DTS Dynamic Range",regValue);

				if (checkBoxDTSLFE.Checked) regValue=1;
				else regValue=0;
				subkey.SetValue("DTS LFE",regValue);

				if (checkBoxNormalize.Checked) regValue=1;
				else regValue=0;
				subkey.SetValue("Normalize",regValue);
								
				subkey.SetValue("AC3 Speaker Config",comboBoxAC3SpeakerConfig.SelectedIndex);
				subkey.SetValue("DTS Speaker Config",comboBoxDTSSpeakerConfig.SelectedIndex);
				subkey.SetValue("Boost",trackBarBoost.Value);
				
				if (radioPCM16Bit.Checked) regValue=0;
				if (radioButtonPCM24Bit.Checked) regValue=1;
				if (radioButtonPCM32Bit.Checked) regValue=2;
				if (radioButtonIEEE.Checked) regValue=3;
				subkey.SetValue("Output Format",regValue);

				if (radioButtonAC3Speakers.Checked) regValue=0;
				if (radioButtonAC3SPDIF.Checked) regValue=1;
				subkey.SetValue("AC3Decoder",regValue);

				if (radioButtonDTSSpeakers.Checked) regValue=0;
				if (radioButtonDTSSPDIF.Checked) regValue=1;
				subkey.SetValue("DTSDecoder",regValue);

				subkey.Close();
			}
		}

		private void radioButtonAC3Speakers_CheckedChanged(object sender, System.EventArgs e)
		{
			if (radioButtonAC3Speakers.Checked) 
			{
				comboBoxAC3SpeakerConfig.Enabled=true;
				checkBoxAC3LFE.Enabled=true;
			}

		}

		private void radioButtonAC3SPDIF_CheckedChanged(object sender, System.EventArgs e)
		{
			if (radioButtonAC3SPDIF.Checked) 
			{
				comboBoxAC3SpeakerConfig.Enabled=false;
				checkBoxAC3LFE.Enabled=false;
			}
		}

		private void radioButtonDTSSpeakers_CheckedChanged(object sender, System.EventArgs e)
		{
			if (radioButtonDTSSpeakers.Checked) 
			{
				comboBoxDTSSpeakerConfig.Enabled=true;
				checkBoxDTSLFE.Enabled=true;
			}
		}

		private void radioButtonDTSSPDIF_CheckedChanged(object sender, System.EventArgs e)
		{
			if (radioButtonDTSSPDIF.Checked) 
			{
				comboBoxDTSSpeakerConfig.Enabled=false;
				checkBoxDTSLFE.Enabled=false;
			}
		}

	}
}

