using System;
using System.Globalization;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class DVD : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.ComboBox defaultSubtitleLanguageComboBox;
		private System.Windows.Forms.ComboBox defaultAudioLanguageComboBox;
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
		private MediaPortal.UserInterface.Controls.MPCheckBox pixelRatioCheckBox;
		private System.Windows.Forms.ComboBox displayModeComboBox;
		private System.Windows.Forms.ComboBox aspectRatioComboBox;
		private System.Windows.Forms.Label aspectRatioLabel;
		private MediaPortal.UserInterface.Controls.MPCheckBox showSubtitlesCheckBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label defaultAudioLanguagelabel;
		private System.Windows.Forms.Label displayModeLabel;
		private System.ComponentModel.IContainer components = null;

		public DVD() : this("DVD")
		{
		}

		public DVD(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			//
			// Populate combo box with languages
			//
			PopulateLanguages(defaultSubtitleLanguageComboBox, "English");
			PopulateLanguages(defaultAudioLanguageComboBox, "English");
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

		/// <summary>
		/// 
		/// </summary>
		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				defaultAudioLanguageComboBox.SelectedItem = xmlreader.GetValueAsString("dvdplayer", "audiolanguage", "English");
				defaultSubtitleLanguageComboBox.SelectedItem = xmlreader.GetValueAsString("dvdplayer", "subtitlelanguage", "English");

				showSubtitlesCheckBox.Checked = xmlreader.GetValueAsBool("dvdplayer", "showsubtitles", true);
				pixelRatioCheckBox.Checked = xmlreader.GetValueAsBool("dvdplayer", "pixelratiocorrection", false);

				aspectRatioComboBox.Text = xmlreader.GetValueAsString("dvdplayer", "armode", "Stretch");
				displayModeComboBox.Text = xmlreader.GetValueAsString("dvdplayer", "displaymode", "Default");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValue("dvdplayer", "audiolanguage", defaultAudioLanguageComboBox.Text);
				xmlwriter.SetValue("dvdplayer", "subtitlelanguage", defaultSubtitleLanguageComboBox.Text);

				xmlwriter.SetValueAsBool("dvdplayer", "showsubtitles", showSubtitlesCheckBox.Checked);
				xmlwriter.SetValueAsBool("dvdplayer", "pixelratiocorrection", pixelRatioCheckBox.Checked);

				xmlwriter.SetValue("dvdplayer", "armode", aspectRatioComboBox.Text);
				xmlwriter.SetValue("dvdplayer", "displaymode", displayModeComboBox.Text);
			}			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="comboBox"></param>
		/// <param name="defaultLanguage"></param>
		void PopulateLanguages(ComboBox comboBox, string defaultLanguage)
		{
			comboBox.Items.Clear();
			foreach(CultureInfo cultureInformation in CultureInfo.GetCultures(CultureTypes.NeutralCultures)) 
			{
				comboBox.Items.Add(cultureInformation.EnglishName);
				
				if(String.Compare(cultureInformation.EnglishName, defaultLanguage, true) == 0) 
				{
					comboBox.Text = defaultLanguage;
				}
			}
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.defaultAudioLanguagelabel = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.defaultAudioLanguageComboBox = new System.Windows.Forms.ComboBox();
      this.defaultSubtitleLanguageComboBox = new System.Windows.Forms.ComboBox();
      this.showSubtitlesCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.displayModeLabel = new System.Windows.Forms.Label();
      this.displayModeComboBox = new System.Windows.Forms.ComboBox();
      this.aspectRatioComboBox = new System.Windows.Forms.ComboBox();
      this.aspectRatioLabel = new System.Windows.Forms.Label();
      this.pixelRatioCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.groupBox1.SuspendLayout();
      this.mpGroupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.defaultAudioLanguagelabel);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.defaultAudioLanguageComboBox);
      this.groupBox1.Controls.Add(this.defaultSubtitleLanguageComboBox);
      this.groupBox1.Controls.Add(this.showSubtitlesCheckBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(440, 144);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Languages";
      // 
      // defaultAudioLanguagelabel
      // 
      this.defaultAudioLanguagelabel.Location = new System.Drawing.Point(32, 105);
      this.defaultAudioLanguagelabel.Name = "defaultAudioLanguagelabel";
      this.defaultAudioLanguagelabel.TabIndex = 15;
      this.defaultAudioLanguagelabel.Text = "Audio";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(32, 80);
      this.label1.Name = "label1";
      this.label1.TabIndex = 14;
      this.label1.Text = "Subtitles";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 56);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(150, 23);
      this.label2.TabIndex = 13;
      this.label2.Text = "Default language for:";
      // 
      // defaultAudioLanguageComboBox
      // 
      this.defaultAudioLanguageComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultAudioLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultAudioLanguageComboBox.Location = new System.Drawing.Point(168, 102);
      this.defaultAudioLanguageComboBox.Name = "defaultAudioLanguageComboBox";
      this.defaultAudioLanguageComboBox.Size = new System.Drawing.Size(256, 21);
      this.defaultAudioLanguageComboBox.Sorted = true;
      this.defaultAudioLanguageComboBox.TabIndex = 11;
      // 
      // defaultSubtitleLanguageComboBox
      // 
      this.defaultSubtitleLanguageComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.defaultSubtitleLanguageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.defaultSubtitleLanguageComboBox.Location = new System.Drawing.Point(168, 77);
      this.defaultSubtitleLanguageComboBox.Name = "defaultSubtitleLanguageComboBox";
      this.defaultSubtitleLanguageComboBox.Size = new System.Drawing.Size(256, 21);
      this.defaultSubtitleLanguageComboBox.Sorted = true;
      this.defaultSubtitleLanguageComboBox.TabIndex = 9;
      // 
      // showSubtitlesCheckBox
      // 
      this.showSubtitlesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.showSubtitlesCheckBox.Location = new System.Drawing.Point(16, 24);
      this.showSubtitlesCheckBox.Name = "showSubtitlesCheckBox";
      this.showSubtitlesCheckBox.Size = new System.Drawing.Size(264, 24);
      this.showSubtitlesCheckBox.TabIndex = 7;
      this.showSubtitlesCheckBox.Text = "Show subtitles";
      // 
      // mpGroupBox1
      // 
      this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.mpGroupBox1.Controls.Add(this.displayModeLabel);
      this.mpGroupBox1.Controls.Add(this.displayModeComboBox);
      this.mpGroupBox1.Controls.Add(this.aspectRatioComboBox);
      this.mpGroupBox1.Controls.Add(this.aspectRatioLabel);
      this.mpGroupBox1.Controls.Add(this.pixelRatioCheckBox);
      this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.mpGroupBox1.Location = new System.Drawing.Point(8, 160);
      this.mpGroupBox1.Name = "mpGroupBox1";
      this.mpGroupBox1.Size = new System.Drawing.Size(440, 120);
      this.mpGroupBox1.TabIndex = 1;
      this.mpGroupBox1.TabStop = false;
      this.mpGroupBox1.Text = "Aspect Ratio";
      // 
      // displayModeLabel
      // 
      this.displayModeLabel.Location = new System.Drawing.Point(16, 81);
      this.displayModeLabel.Name = "displayModeLabel";
      this.displayModeLabel.Size = new System.Drawing.Size(150, 23);
      this.displayModeLabel.TabIndex = 16;
      this.displayModeLabel.Text = "Display mode";
      // 
      // displayModeComboBox
      // 
      this.displayModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.displayModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.displayModeComboBox.Items.AddRange(new object[] {
                                                             "Default",
                                                             "16:9",
                                                             "4:3 Pan Scan",
                                                             "4:3 Letterbox"});
      this.displayModeComboBox.Location = new System.Drawing.Point(168, 78);
      this.displayModeComboBox.Name = "displayModeComboBox";
      this.displayModeComboBox.Size = new System.Drawing.Size(256, 21);
      this.displayModeComboBox.TabIndex = 15;
      // 
      // aspectRatioComboBox
      // 
      this.aspectRatioComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.aspectRatioComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.aspectRatioComboBox.Items.AddRange(new object[] {
                                                             "Crop",
                                                             "Letterbox",
                                                             "Stretch",
                                                             "Follow stream"});
      this.aspectRatioComboBox.Location = new System.Drawing.Point(168, 53);
      this.aspectRatioComboBox.Name = "aspectRatioComboBox";
      this.aspectRatioComboBox.Size = new System.Drawing.Size(256, 21);
      this.aspectRatioComboBox.TabIndex = 13;
      // 
      // aspectRatioLabel
      // 
      this.aspectRatioLabel.Location = new System.Drawing.Point(16, 56);
      this.aspectRatioLabel.Name = "aspectRatioLabel";
      this.aspectRatioLabel.Size = new System.Drawing.Size(150, 23);
      this.aspectRatioLabel.TabIndex = 12;
      this.aspectRatioLabel.Text = "Aspect ratio correction mode";
      // 
      // pixelRatioCheckBox
      // 
      this.pixelRatioCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.pixelRatioCheckBox.Location = new System.Drawing.Point(16, 24);
      this.pixelRatioCheckBox.Name = "pixelRatioCheckBox";
      this.pixelRatioCheckBox.Size = new System.Drawing.Size(264, 24);
      this.pixelRatioCheckBox.TabIndex = 8;
      this.pixelRatioCheckBox.Text = "Use pixel ratio correction";
      // 
      // DVD
      // 
      this.Controls.Add(this.mpGroupBox1);
      this.Controls.Add(this.groupBox1);
      this.Name = "DVD";
      this.Size = new System.Drawing.Size(456, 448);
      this.groupBox1.ResumeLayout(false);
      this.mpGroupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion
	}
}

