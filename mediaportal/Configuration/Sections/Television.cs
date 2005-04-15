using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using DShowNET;
using DShowNET.Device;

namespace MediaPortal.Configuration.Sections
{
	public class Television : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.ComboBox rendererComboBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.ComboBox audioCodecComboBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox videoCodecComboBox;
		private System.Windows.Forms.Label label5;
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
		private System.Windows.Forms.ComboBox countryComboBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox inputComboBox;
		private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox defaultZoomModeComboBox;
    private System.Windows.Forms.Label label6;
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.GroupBox groupBox4;
		private MediaPortal.UserInterface.Controls.MPCheckBox alwaysTimeShiftCheckBox;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox textBoxTimeShiftBuffer;
		private System.Windows.Forms.Label lblminutes;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.ComboBox cbDeinterlace;

    string[] aspectRatio = { "normal", "original", "stretch", "zoom", "letterbox", "panscan" };

		public Television() : this("Television")
		{
		}		

		public Television(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			//
			// Populate the video renderer combobox
			//
			rendererComboBox.Items.AddRange(VideoRenderers.List);

			//
			// Populate the country combobox
			//
			countryComboBox.Items.AddRange(TunerCountries.Countries);

			//
			// Populate video and audio codecs
			//
			ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.MPEG2);
			ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.MPEG2_Audio);

			videoCodecComboBox.Items.AddRange(availableVideoFilters.ToArray());
			audioCodecComboBox.Items.AddRange(availableAudioFilters.ToArray());
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
			this.defaultZoomModeComboBox = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.rendererComboBox = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.audioCodecComboBox = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.videoCodecComboBox = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.countryComboBox = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.inputComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.lblminutes = new System.Windows.Forms.Label();
			this.textBoxTimeShiftBuffer = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.alwaysTimeShiftCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.label8 = new System.Windows.Forms.Label();
			this.cbDeinterlace = new System.Windows.Forms.ComboBox();
			this.groupBox1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.cbDeinterlace);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.defaultZoomModeComboBox);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.rendererComboBox);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 104);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "General Settings";
			// 
			// defaultZoomModeComboBox
			// 
			this.defaultZoomModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.defaultZoomModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.defaultZoomModeComboBox.Items.AddRange(new object[] {
																																 "Normal",
																																 "Original Source Format",
																																 "Stretch",
																																 "Zoom",
																																 "4:3 Letterbox",
																																 "4:3 Pan and scan"});
			this.defaultZoomModeComboBox.Location = new System.Drawing.Point(168, 40);
			this.defaultZoomModeComboBox.Name = "defaultZoomModeComboBox";
			this.defaultZoomModeComboBox.Size = new System.Drawing.Size(256, 21);
			this.defaultZoomModeComboBox.TabIndex = 2;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 48);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(150, 23);
			this.label6.TabIndex = 29;
			this.label6.Text = "Default zoom mode";
			// 
			// rendererComboBox
			// 
			this.rendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.rendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.rendererComboBox.Location = new System.Drawing.Point(168, 16);
			this.rendererComboBox.Name = "rendererComboBox";
			this.rendererComboBox.Size = new System.Drawing.Size(256, 21);
			this.rendererComboBox.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(150, 23);
			this.label2.TabIndex = 9;
			this.label2.Text = "Video renderer";
			// 
			// radioButton1
			// 
			this.radioButton1.Location = new System.Drawing.Point(0, 0);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.TabIndex = 0;
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox3.Controls.Add(this.audioCodecComboBox);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.videoCodecComboBox);
			this.groupBox3.Controls.Add(this.label5);
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox3.Location = new System.Drawing.Point(8, 120);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(440, 96);
			this.groupBox3.TabIndex = 1;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "MPEG2 Codec Settings";
			// 
			// audioCodecComboBox
			// 
			this.audioCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.audioCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.audioCodecComboBox.Location = new System.Drawing.Point(168, 51);
			this.audioCodecComboBox.Name = "audioCodecComboBox";
			this.audioCodecComboBox.Size = new System.Drawing.Size(256, 21);
			this.audioCodecComboBox.TabIndex = 1;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 55);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(144, 23);
			this.label3.TabIndex = 8;
			this.label3.Text = "Audio codec";
			// 
			// videoCodecComboBox
			// 
			this.videoCodecComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.videoCodecComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.videoCodecComboBox.Location = new System.Drawing.Point(168, 26);
			this.videoCodecComboBox.Name = "videoCodecComboBox";
			this.videoCodecComboBox.Size = new System.Drawing.Size(256, 21);
			this.videoCodecComboBox.TabIndex = 0;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 30);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(144, 23);
			this.label5.TabIndex = 6;
			this.label5.Text = "Video codec";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.countryComboBox);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.inputComboBox);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(8, 224);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(440, 96);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "TV Tuner Settings";
			// 
			// countryComboBox
			// 
			this.countryComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.countryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.countryComboBox.Location = new System.Drawing.Point(168, 51);
			this.countryComboBox.MaxDropDownItems = 16;
			this.countryComboBox.Name = "countryComboBox";
			this.countryComboBox.Size = new System.Drawing.Size(256, 21);
			this.countryComboBox.Sorted = true;
			this.countryComboBox.TabIndex = 1;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 55);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(150, 23);
			this.label4.TabIndex = 11;
			this.label4.Text = "Country";
			// 
			// inputComboBox
			// 
			this.inputComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.inputComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.inputComboBox.Items.AddRange(new object[] {
																											 "Antenna",
																											 "Cable"});
			this.inputComboBox.Location = new System.Drawing.Point(168, 26);
			this.inputComboBox.Name = "inputComboBox";
			this.inputComboBox.Size = new System.Drawing.Size(256, 21);
			this.inputComboBox.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 30);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(150, 23);
			this.label1.TabIndex = 7;
			this.label1.Text = "Input source";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.lblminutes);
			this.groupBox4.Controls.Add(this.textBoxTimeShiftBuffer);
			this.groupBox4.Controls.Add(this.label7);
			this.groupBox4.Controls.Add(this.alwaysTimeShiftCheckBox);
			this.groupBox4.Location = new System.Drawing.Point(8, 328);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(440, 88);
			this.groupBox4.TabIndex = 3;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Timeshifting settings";
			// 
			// lblminutes
			// 
			this.lblminutes.Location = new System.Drawing.Point(176, 48);
			this.lblminutes.Name = "lblminutes";
			this.lblminutes.Size = new System.Drawing.Size(100, 16);
			this.lblminutes.TabIndex = 3;
			this.lblminutes.Text = "Minutes";
			// 
			// textBoxTimeShiftBuffer
			// 
			this.textBoxTimeShiftBuffer.Location = new System.Drawing.Point(120, 48);
			this.textBoxTimeShiftBuffer.Name = "textBoxTimeShiftBuffer";
			this.textBoxTimeShiftBuffer.Size = new System.Drawing.Size(40, 20);
			this.textBoxTimeShiftBuffer.TabIndex = 2;
			this.textBoxTimeShiftBuffer.Text = "30";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(24, 48);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(88, 16);
			this.label7.TabIndex = 1;
			this.label7.Text = "Timeshift buffer:";
			// 
			// alwaysTimeShiftCheckBox
			// 
			this.alwaysTimeShiftCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.alwaysTimeShiftCheckBox.Location = new System.Drawing.Point(16, 16);
			this.alwaysTimeShiftCheckBox.Name = "alwaysTimeShiftCheckBox";
			this.alwaysTimeShiftCheckBox.Size = new System.Drawing.Size(280, 24);
			this.alwaysTimeShiftCheckBox.TabIndex = 0;
			this.alwaysTimeShiftCheckBox.Text = "Always use timeshifting";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(16, 72);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(100, 16);
			this.label8.TabIndex = 30;
			this.label8.Text = "Deinterlace mode:";
			// 
			// cbDeinterlace
			// 
			this.cbDeinterlace.Items.AddRange(new object[] {
																											 "None",
																											 "Bob",
																											 "Weave",
																											 "Best"});
			this.cbDeinterlace.Location = new System.Drawing.Point(168, 72);
			this.cbDeinterlace.Name = "cbDeinterlace";
			this.cbDeinterlace.Size = new System.Drawing.Size(160, 21);
			this.cbDeinterlace.TabIndex = 31;
			// 
			// Television
			// 
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox1);
			this.Name = "Television";
			this.Size = new System.Drawing.Size(456, 448);
			this.groupBox1.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		public override void LoadSettings()
		{
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				alwaysTimeShiftCheckBox.Checked = xmlreader.GetValueAsBool("mytv", "alwaystimeshift", false);
				inputComboBox.SelectedItem = xmlreader.GetValueAsString("capture", "tuner", "Antenna");
				textBoxTimeShiftBuffer.Text= xmlreader.GetValueAsInt("capture", "timeshiftbuffer", 30).ToString();
				int DeInterlaceMode= xmlreader.GetValueAsInt("mytv", "deinterlace", 0);
				if (DeInterlaceMode<0 || DeInterlaceMode>3)
					DeInterlaceMode=3;
				cbDeinterlace.SelectedIndex=DeInterlaceMode;

				//
				// Set video renderer
				//
				int videoRenderer = xmlreader.GetValueAsInt("mytv", "vmr9", 0);

				if(videoRenderer >= 0 && videoRenderer <= VideoRenderers.List.Length)
					rendererComboBox.SelectedItem = VideoRenderers.List[videoRenderer];


				//
				// We can't set the SelectedItem here as the items in the combobox are of TunerCountry type.
				//
				countryComboBox.Text = xmlreader.GetValueAsString("capture", "countryname", "Netherlands");

				//
				// Set codecs
				//
        string audioCodec=xmlreader.GetValueAsString("mytv", "audiocodec", "");
        string videoCodec=xmlreader.GetValueAsString("mytv", "videocodec", "");
        if (audioCodec==String.Empty)
        {
          ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.MPEG2_Audio);
          if (availableAudioFilters.Count>0)
          {
						bool Mpeg2DecFilterFound=true;
						bool DScalerFilterFound=true;
						audioCodec=(string)availableAudioFilters[0];
            foreach (string filter in availableAudioFilters)
						{
							if (filter.Equals("MPEG/AC3/DTS/LPCM Audio Decoder"))
							{
								Mpeg2DecFilterFound=true;
							}
							if (filter.Equals("DScaler Audio Decoder"))
							{
								DScalerFilterFound=true;
							}
            }
            if (Mpeg2DecFilterFound) audioCodec="MPEG/AC3/DTS/LPCM Audio Decoder";
            else if (DScalerFilterFound) audioCodec="DScaler Audio Decoder";
          }
        }
        if (videoCodec==String.Empty)
        {
          ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.MPEG2);
          bool Mpeg2DecFilterFound=true;
					bool DScalerFilterFound=true;
          if (availableVideoFilters.Count>0)
          {
            videoCodec=(string)availableVideoFilters[0];
            foreach (string filter in availableVideoFilters)
            {
              if (filter.Equals("Mpeg2Dec Filter"))
              {
                Mpeg2DecFilterFound=true;
              }
              if (filter.Equals("DScaler Mpeg2 Video Decoder"))
              {
                DScalerFilterFound=true;
              }
            }
            if (Mpeg2DecFilterFound) videoCodec="Mpeg2Dec Filter";
            else if (DScalerFilterFound) videoCodec="DScaler Mpeg2 Video Decoder";
          }
        }
				audioCodecComboBox.SelectedItem = audioCodec;
				videoCodecComboBox.SelectedItem = videoCodec;

        //
        // Set default aspect ratio
        //
        string defaultAspectRatio = xmlreader.GetValueAsString("mytv","defaultar", "normal");

        for(int index = 0; index < aspectRatio.Length; index++)
        {
          if(aspectRatio[index].Equals(defaultAspectRatio))
          {
            defaultZoomModeComboBox.SelectedIndex = index;
            break;
          }
        }
      }			
		}


		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				if (cbDeinterlace.SelectedIndex>=0)
					xmlwriter.SetValue("mytv", "deinterlace", cbDeinterlace.SelectedIndex.ToString());

				xmlwriter.SetValueAsBool("mytv", "alwaystimeshift", alwaysTimeShiftCheckBox.Checked);
				xmlwriter.SetValue("capture", "tuner", inputComboBox.Text);
				try
				{
					int buffer=Int32.Parse(textBoxTimeShiftBuffer.Text);
					xmlwriter.SetValue("capture", "timeshiftbuffer", buffer.ToString());
				}
				catch(Exception){}
        xmlwriter.SetValue("mytv","defaultar", aspectRatio[defaultZoomModeComboBox.SelectedIndex]);

				for(int index = 0; index < VideoRenderers.List.Length; index++)
				{
					if(VideoRenderers.List[index].Equals(rendererComboBox.Text))
					{
						xmlwriter.SetValue("mytv", "vmr9", index);
					}
				}

        if(countryComboBox.Text.Length > 0)
        {
          TunerCountry tunerCountry = countryComboBox.SelectedItem as TunerCountry;
          xmlwriter.SetValue("capture", "countryname", countryComboBox.Text);
          xmlwriter.SetValue("capture", "country", tunerCountry.Id.ToString());
        }

				//
				// Set codecs
				//
				xmlwriter.SetValue("mytv", "audiocodec", audioCodecComboBox.Text);
				xmlwriter.SetValue("mytv", "videocodec", videoCodecComboBox.Text);

			}
		}

		public override object GetSetting(string name)
		{
			switch(name)
			{
				case "television.country" :
				{
					int countryId = 0;

					if(countryComboBox.SelectedItem != null)
					{
						TunerCountry tunerCountry = countryComboBox.SelectedItem as TunerCountry;
						countryId = tunerCountry.Id;
					}

					return countryId;
				}

        case "television.countryname" :
          return countryComboBox.Text;
      }

			return null;
		}

    public override void OnSectionActivated()
    {
     
    }
	}
}

