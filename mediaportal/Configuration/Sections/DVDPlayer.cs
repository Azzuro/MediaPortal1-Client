using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class DVDPlayer : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox fileNameTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button fileNameButton;
		private System.Windows.Forms.Button parametersButton;
		private System.Windows.Forms.TextBox parametersTextBox;
		private MediaPortal.UserInterface.Controls.MPCheckBox internalPlayerCheckBox;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox3;
    private System.Windows.Forms.ComboBox videoRendererComboBox;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ComboBox defaultZoomModeComboBox;
    private System.Windows.Forms.Label label6;
		private System.ComponentModel.IContainer components = null;

    string[] aspectRatio = { "normal", "original", "stretch", "zoom", "letterbox", "panscan" };

    public DVDPlayer() : this("DVD Player")
		{
		}

		public DVDPlayer(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

      //
      // Populate combobox
      // 
      videoRendererComboBox.Items.AddRange(VideoRenderersShort.List);
    }

		/// <summary>
		/// 
		/// </summary>
		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				fileNameTextBox.Text = xmlreader.GetValueAsString("dvdplayer", "path", @"");
				parametersTextBox.Text = xmlreader.GetValueAsString("dvdplayer","arguments", "");

        int videoRenderer = xmlreader.GetValueAsInt("dvdplayer", "vmr9", 0);

        if(videoRenderer >= 0 && videoRenderer <= VideoRenderersShort.List.Length)				
          videoRendererComboBox.SelectedItem = VideoRenderersShort.List[videoRenderer];        

				//
				// Fake a check changed to force a CheckChanged event
				//
				internalPlayerCheckBox.Checked = xmlreader.GetValueAsBool("dvdplayer", "internal", true);
				internalPlayerCheckBox.Checked = !internalPlayerCheckBox.Checked;

        //
        // Set default aspect ratio
        //
        string defaultAspectRatio = xmlreader.GetValueAsString("dvdplayer","defaultar", "original");

        for(int index = 0; index < aspectRatio.Length; index++)
        {
          if(aspectRatio[index].Equals(defaultAspectRatio))
          {
						if (index<defaultZoomModeComboBox.Items.Count)
							defaultZoomModeComboBox.SelectedIndex = index;
            break;
          }
        }
      }
		}

    /// <summary>
    /// 
    /// </summary>
    public override void SaveSettings()
    {
      using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValue("dvdplayer", "path", fileNameTextBox.Text);
        xmlwriter.SetValue("dvdplayer","arguments", parametersTextBox.Text);
        
        xmlwriter.SetValueAsBool("dvdplayer", "internal", !internalPlayerCheckBox.Checked);

        xmlwriter.SetValue("dvdplayer","defaultar", aspectRatio[defaultZoomModeComboBox.SelectedIndex]);

        for(int index = 0; index < VideoRenderersShort.List.Length; index++)
        {
          if(VideoRenderersShort.List[index].Equals(videoRendererComboBox.Text))
          {
						xmlwriter.SetValue("dvdplayer", "vmr9", index);
						break;
          }
        }
      }
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
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void internalPlayerCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			fileNameTextBox.Enabled = parametersTextBox.Enabled = fileNameButton.Enabled = parametersButton.Enabled = internalPlayerCheckBox.Checked;
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.internalPlayerCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.defaultZoomModeComboBox = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.parametersButton = new System.Windows.Forms.Button();
			this.parametersTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.fileNameButton = new System.Windows.Forms.Button();
			this.fileNameTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.button2 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.mpGroupBox3 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.videoRendererComboBox = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.mpGroupBox1.SuspendLayout();
			this.mpGroupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// internalPlayerCheckBox
			// 
			this.internalPlayerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.internalPlayerCheckBox.Location = new System.Drawing.Point(16, 24);
			this.internalPlayerCheckBox.Name = "internalPlayerCheckBox";
			this.internalPlayerCheckBox.Size = new System.Drawing.Size(200, 24);
			this.internalPlayerCheckBox.TabIndex = 0;
			this.internalPlayerCheckBox.Text = "Use external player";
			this.internalPlayerCheckBox.CheckedChanged += new System.EventHandler(this.internalPlayerCheckBox_CheckedChanged);
			// 
			// mpGroupBox1
			// 
			this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.mpGroupBox1.Controls.Add(this.defaultZoomModeComboBox);
			this.mpGroupBox1.Controls.Add(this.label6);
			this.mpGroupBox1.Controls.Add(this.internalPlayerCheckBox);
			this.mpGroupBox1.Controls.Add(this.parametersButton);
			this.mpGroupBox1.Controls.Add(this.parametersTextBox);
			this.mpGroupBox1.Controls.Add(this.label2);
			this.mpGroupBox1.Controls.Add(this.fileNameButton);
			this.mpGroupBox1.Controls.Add(this.fileNameTextBox);
			this.mpGroupBox1.Controls.Add(this.label1);
			this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.mpGroupBox1.Location = new System.Drawing.Point(8, 8);
			this.mpGroupBox1.Name = "mpGroupBox1";
			this.mpGroupBox1.Size = new System.Drawing.Size(440, 168);
			this.mpGroupBox1.TabIndex = 1;
			this.mpGroupBox1.TabStop = false;
			this.mpGroupBox1.Text = "General settings";
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
			this.defaultZoomModeComboBox.Location = new System.Drawing.Point(168, 123);
			this.defaultZoomModeComboBox.Name = "defaultZoomModeComboBox";
			this.defaultZoomModeComboBox.Size = new System.Drawing.Size(256, 21);
			this.defaultZoomModeComboBox.TabIndex = 5;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 127);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(150, 23);
			this.label6.TabIndex = 31;
			this.label6.Text = "Default zoom mode";
			// 
			// parametersButton
			// 
			this.parametersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.parametersButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.parametersButton.Location = new System.Drawing.Point(366, 78);
			this.parametersButton.Name = "parametersButton";
			this.parametersButton.Size = new System.Drawing.Size(58, 20);
			this.parametersButton.TabIndex = 4;
			this.parametersButton.Text = "List";
			this.parametersButton.Click += new System.EventHandler(this.parametersButton_Click);
			// 
			// parametersTextBox
			// 
			this.parametersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.parametersTextBox.Location = new System.Drawing.Point(96, 78);
			this.parametersTextBox.Name = "parametersTextBox";
			this.parametersTextBox.Size = new System.Drawing.Size(264, 20);
			this.parametersTextBox.TabIndex = 3;
			this.parametersTextBox.Text = "";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 81);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 23);
			this.label2.TabIndex = 3;
			this.label2.Text = "Parameters";
			// 
			// fileNameButton
			// 
			this.fileNameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.fileNameButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.fileNameButton.Location = new System.Drawing.Point(366, 53);
			this.fileNameButton.Name = "fileNameButton";
			this.fileNameButton.Size = new System.Drawing.Size(58, 20);
			this.fileNameButton.TabIndex = 2;
			this.fileNameButton.Text = "Browse";
			this.fileNameButton.Click += new System.EventHandler(this.fileNameButton_Click);
			// 
			// fileNameTextBox
			// 
			this.fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.fileNameTextBox.Location = new System.Drawing.Point(96, 53);
			this.fileNameTextBox.Name = "fileNameTextBox";
			this.fileNameTextBox.Size = new System.Drawing.Size(264, 20);
			this.fileNameTextBox.TabIndex = 1;
			this.fileNameTextBox.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 56);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(80, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "Filename";
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(0, 0);
			this.button2.Name = "button2";
			this.button2.TabIndex = 0;
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(0, 0);
			this.textBox1.Name = "textBox1";
			this.textBox1.TabIndex = 0;
			this.textBox1.Text = "";
			// 
			// mpGroupBox3
			// 
			this.mpGroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.mpGroupBox3.Controls.Add(this.videoRendererComboBox);
			this.mpGroupBox3.Controls.Add(this.label4);
			this.mpGroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.mpGroupBox3.Location = new System.Drawing.Point(8, 184);
			this.mpGroupBox3.Name = "mpGroupBox3";
			this.mpGroupBox3.Size = new System.Drawing.Size(440, 72);
			this.mpGroupBox3.TabIndex = 4;
			this.mpGroupBox3.TabStop = false;
			this.mpGroupBox3.Text = "Renderer Settings";
			// 
			// videoRendererComboBox
			// 
			this.videoRendererComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.videoRendererComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.videoRendererComboBox.Location = new System.Drawing.Point(168, 27);
			this.videoRendererComboBox.Name = "videoRendererComboBox";
			this.videoRendererComboBox.Size = new System.Drawing.Size(256, 21);
			this.videoRendererComboBox.TabIndex = 0;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 31);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(150, 23);
			this.label4.TabIndex = 27;
			this.label4.Text = "Video renderer";
			// 
			// DVDPlayer
			// 
			this.Controls.Add(this.mpGroupBox3);
			this.Controls.Add(this.mpGroupBox1);
			this.Name = "DVDPlayer";
			this.Size = new System.Drawing.Size(456, 440);
			this.mpGroupBox1.ResumeLayout(false);
			this.mpGroupBox3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void parametersButton_Click(object sender, System.EventArgs e)
		{
			ParameterForm parameters = new ParameterForm();

			parameters.AddParameter("%filename%", "This will be replaced by the selected media file");

			if(parameters.ShowDialog(parametersButton) == DialogResult.OK)
			{
				parametersTextBox.Text += parameters.SelectedParameter;
			}		
		}

		private void fileNameButton_Click(object sender, System.EventArgs e)
		{
			using(openFileDialog = new OpenFileDialog())
			{
				openFileDialog.FileName = fileNameTextBox.Text;
				openFileDialog.CheckFileExists = true;
				openFileDialog.RestoreDirectory=true;
				openFileDialog.Filter= "exe files (*.exe)|*.exe";
				openFileDialog.FilterIndex = 0;
				openFileDialog.Title = "Select DVD player";

				DialogResult dialogResult = openFileDialog.ShowDialog();

				if(dialogResult == DialogResult.OK)
				{
					fileNameTextBox.Text = openFileDialog.FileName;
				}
			}		
		}
	}
}

