using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

namespace MediaPortal.Configuration.Sections
{
	public class Movies : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.Button fileNameButton;
		private System.Windows.Forms.TextBox folderNameTextBox;
		private System.Windows.Forms.Label folderNameLabel;
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label6;
		private MediaPortal.UserInterface.Controls.MPCheckBox repeatPlaylistCheckBox;
		private MediaPortal.UserInterface.Controls.MPCheckBox showSubtitlesCheckBox;
		private System.Windows.Forms.Button subtitlesButton;
		private System.Windows.Forms.TextBox subtitlesFontTextBox;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.FontDialog fontDialog;
		private System.ComponentModel.IContainer components = null;

		string fontName;
		string fontColor;
		bool fontIsBold;
		private System.Windows.Forms.TextBox dropShadowTextBox;
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox2;
		private System.Windows.Forms.TextBox displayTimoutTextBox;
		private System.Windows.Forms.Label label5;
		int fontSize;
		private System.Windows.Forms.ComboBox defaultZoomModeComboBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ListView lvDatabase;
		private System.Windows.Forms.ColumnHeader chDatabaseDB;
		private System.Windows.Forms.ColumnHeader chDatabaseLanguage;
		private System.Windows.Forms.ColumnHeader chDatabaseLimit;
		private System.Windows.Forms.Button bDatabaseUp;
		private System.Windows.Forms.Button bDatabaseDown;
		private System.Windows.Forms.ComboBox cbDatabaseLimit;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.TabPage tabPage4;

		string[] aspectRatio = { "normal", "original", "stretch", "zoom", "letterbox", "panscan" };

		public Movies() : this("Movies")
		{
		}

		public Movies(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
		}

		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				repeatPlaylistCheckBox.Checked = xmlreader.GetValueAsBool("movies", "repeat", true);
				folderNameTextBox.Text = xmlreader.GetValueAsString("movies", "playlists", "");

				showSubtitlesCheckBox.Checked = xmlreader.GetValueAsBool("subtitles", "enabled", true);

				dropShadowTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("subtitles", "shadow", 5));
				displayTimoutTextBox.Text = Convert.ToString(xmlreader.GetValueAsInt("movieplayer", "osdtimeout", 0));

				//
				// Get font settings
				//
				fontName	= xmlreader.GetValueAsString("subtitles", "fontface", "Arial");
				fontColor	= xmlreader.GetValueAsString("subtitles", "color", "ffffff");
				fontIsBold	= xmlreader.GetValueAsBool("subtitles", "bold", true);
				fontSize	= xmlreader.GetValueAsInt("subtitles", "fontsize", 18);

				subtitlesFontTextBox.Text  = String.Format("{0} {1}{2}", fontName, fontSize, fontIsBold ? ", Bold" : "");

				//
				// Try to parse the specified color into a valid color
				//
				if (fontColor != null && fontColor.Length > 0)
				{
					try
					{
						int rgbColor = Int32.Parse(fontColor, NumberStyles.HexNumber);
						subtitlesFontTextBox.BackColor = Color.Black;
						subtitlesFontTextBox.ForeColor = Color.FromArgb(rgbColor);
					}
					catch {}
				}

				//
				// Set default aspect ratio
				//
				string defaultAspectRatio = xmlreader.GetValueAsString("movieplayer","defaultar", "normal");

				for(int index = 0; index < aspectRatio.Length; index++)
				{
					if(aspectRatio[index].Equals(defaultAspectRatio))
					{
						defaultZoomModeComboBox.SelectedIndex = index;
						break;
					}
				}

				// Load settings for Database
				int iNumber = xmlreader.GetValueAsInt("moviedatabase","number",0);
				if (iNumber<=0)
				{
					// no given databases in XML - entering default values
					// create entry for IMDB
					this.lvDatabase.Items.Add("IMDB");
					this.lvDatabase.Items[0].SubItems.Add("english");
					this.lvDatabase.Items[0].SubItems.Add("25");
					// create entry for OFDB
					this.lvDatabase.Items.Add("OFDB");
					this.lvDatabase.Items[1].SubItems.Add("german");
					this.lvDatabase.Items[1].SubItems.Add("25");
          
          // create entry for frdb
          this.lvDatabase.Items.Add("FRDB");
          this.lvDatabase.Items[2].SubItems.Add("french");
          this.lvDatabase.Items[2].SubItems.Add("25");
				}
				else
				{
					int iCount = 0;
					string strLimit = "";
					string strDatabase = "";
					string strLanguage = "";
					// Load values
          bool frenchFound=false;
					for(int i = 0;i < iNumber;i++)
					{
						strLimit = xmlreader.GetValueAsString("moviedatabase","limit"+i.ToString(),"false");
						strDatabase = xmlreader.GetValueAsString("moviedatabase","database"+i.ToString(),"false");
						strLanguage = xmlreader.GetValueAsString("moviedatabase","language"+i.ToString(),"false");
						if ((strLimit!="false") && (strDatabase!="false") && (strLanguage!="false"))
						{
							// create entry for the database
							this.lvDatabase.Items.Add(strDatabase);
							this.lvDatabase.Items[iCount].SubItems.Add(strLanguage);
							this.lvDatabase.Items[iCount].SubItems.Add(strLimit);
							iCount++;
              if (strLanguage=="french")
                frenchFound=true;
						}
					}
          if (!frenchFound)
          {
            this.lvDatabase.Items.Add("FRDB");
            this.lvDatabase.Items[2].SubItems.Add("french");
            this.lvDatabase.Items[2].SubItems.Add("25");
          }
          
				}
				// set the first entry "activ"
				this.lvDatabase.Items[0].Selected = true;
				this.cbDatabaseLimit.Text = this.lvDatabase.Items[0].SubItems[2].Text;
			}		
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("movies", "repeat", repeatPlaylistCheckBox.Checked);
				xmlwriter.SetValue("movies", "playlists", folderNameTextBox.Text);

				xmlwriter.SetValueAsBool("subtitles", "enabled", showSubtitlesCheckBox.Checked);
				xmlwriter.SetValue("subtitles", "shadow", dropShadowTextBox.Text);
				
				xmlwriter.SetValue("movieplayer", "osdtimeout", displayTimoutTextBox.Text);

				xmlwriter.SetValue("subtitles", "fontface", fontName);
				xmlwriter.SetValue("subtitles", "color", fontColor);
				xmlwriter.SetValueAsBool("subtitles", "bold", fontIsBold);
				xmlwriter.SetValue("subtitles", "fontsize", fontSize);
      
				xmlwriter.SetValue("movieplayer","defaultar", aspectRatio[defaultZoomModeComboBox.SelectedIndex]);

				// Database
				xmlwriter.SetValue("moviedatabase", "number", this.lvDatabase.Items.Count);
				for (int i = 0; i < this.lvDatabase.Items.Count; i++)
				{
					xmlwriter.SetValue("moviedatabase", "database"+i.ToString(), this.lvDatabase.Items[i].SubItems[0].Text);
					xmlwriter.SetValue("moviedatabase", "limit"+i.ToString(), this.lvDatabase.Items[i].SubItems[2].Text);
					xmlwriter.SetValue("moviedatabase", "language"+i.ToString(), this.lvDatabase.Items[i].SubItems[1].Text);
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

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.defaultZoomModeComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.fileNameButton = new System.Windows.Forms.Button();
			this.folderNameTextBox = new System.Windows.Forms.TextBox();
			this.repeatPlaylistCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.folderNameLabel = new System.Windows.Forms.Label();
			this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.dropShadowTextBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.showSubtitlesCheckBox = new MediaPortal.UserInterface.Controls.MPCheckBox();
			this.subtitlesButton = new System.Windows.Forms.Button();
			this.subtitlesFontTextBox = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.fontDialog = new System.Windows.Forms.FontDialog();
			this.mpGroupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.displayTimoutTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.cbDatabaseLimit = new System.Windows.Forms.ComboBox();
			this.bDatabaseDown = new System.Windows.Forms.Button();
			this.bDatabaseUp = new System.Windows.Forms.Button();
			this.lvDatabase = new System.Windows.Forms.ListView();
			this.chDatabaseDB = new System.Windows.Forms.ColumnHeader();
			this.chDatabaseLanguage = new System.Windows.Forms.ColumnHeader();
			this.chDatabaseLimit = new System.Windows.Forms.ColumnHeader();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.groupBox1.SuspendLayout();
			this.mpGroupBox1.SuspendLayout();
			this.mpGroupBox2.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.tabPage4.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.defaultZoomModeComboBox);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.fileNameButton);
			this.groupBox1.Controls.Add(this.folderNameTextBox);
			this.groupBox1.Controls.Add(this.repeatPlaylistCheckBox);
			this.groupBox1.Controls.Add(this.folderNameLabel);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(360, 280);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "General settings";
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
			this.defaultZoomModeComboBox.Location = new System.Drawing.Point(24, 128);
			this.defaultZoomModeComboBox.Name = "defaultZoomModeComboBox";
			this.defaultZoomModeComboBox.Size = new System.Drawing.Size(256, 21);
			this.defaultZoomModeComboBox.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 104);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 16);
			this.label1.TabIndex = 33;
			this.label1.Text = "Default zoom mode";
			// 
			// fileNameButton
			// 
			this.fileNameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.fileNameButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.fileNameButton.Location = new System.Drawing.Point(288, 72);
			this.fileNameButton.Name = "fileNameButton";
			this.fileNameButton.Size = new System.Drawing.Size(56, 20);
			this.fileNameButton.TabIndex = 2;
			this.fileNameButton.Text = "Browse";
			this.fileNameButton.Click += new System.EventHandler(this.fileNameButton_Click);
			// 
			// folderNameTextBox
			// 
			this.folderNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.folderNameTextBox.Location = new System.Drawing.Point(24, 72);
			this.folderNameTextBox.Name = "folderNameTextBox";
			this.folderNameTextBox.Size = new System.Drawing.Size(256, 20);
			this.folderNameTextBox.TabIndex = 1;
			this.folderNameTextBox.Text = "";
			// 
			// repeatPlaylistCheckBox
			// 
			this.repeatPlaylistCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.repeatPlaylistCheckBox.Location = new System.Drawing.Point(16, 24);
			this.repeatPlaylistCheckBox.Name = "repeatPlaylistCheckBox";
			this.repeatPlaylistCheckBox.Size = new System.Drawing.Size(264, 24);
			this.repeatPlaylistCheckBox.TabIndex = 0;
			this.repeatPlaylistCheckBox.Text = "Repeat/loop video playlists";
			// 
			// folderNameLabel
			// 
			this.folderNameLabel.Location = new System.Drawing.Point(16, 56);
			this.folderNameLabel.Name = "folderNameLabel";
			this.folderNameLabel.Size = new System.Drawing.Size(80, 23);
			this.folderNameLabel.TabIndex = 0;
			this.folderNameLabel.Text = "Playlist folder";
			// 
			// mpGroupBox1
			// 
			this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.mpGroupBox1.Controls.Add(this.dropShadowTextBox);
			this.mpGroupBox1.Controls.Add(this.label4);
			this.mpGroupBox1.Controls.Add(this.showSubtitlesCheckBox);
			this.mpGroupBox1.Controls.Add(this.subtitlesButton);
			this.mpGroupBox1.Controls.Add(this.subtitlesFontTextBox);
			this.mpGroupBox1.Controls.Add(this.label6);
			this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.mpGroupBox1.Location = new System.Drawing.Point(16, 16);
			this.mpGroupBox1.Name = "mpGroupBox1";
			this.mpGroupBox1.Size = new System.Drawing.Size(352, 216);
			this.mpGroupBox1.TabIndex = 3;
			this.mpGroupBox1.TabStop = false;
			this.mpGroupBox1.Text = "Subtitles";
			// 
			// dropShadowTextBox
			// 
			this.dropShadowTextBox.Location = new System.Drawing.Point(32, 120);
			this.dropShadowTextBox.Name = "dropShadowTextBox";
			this.dropShadowTextBox.Size = new System.Drawing.Size(40, 20);
			this.dropShadowTextBox.TabIndex = 4;
			this.dropShadowTextBox.Text = "";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 96);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(150, 23);
			this.label4.TabIndex = 3;
			this.label4.Text = "Drop shadow (pixels)";
			// 
			// showSubtitlesCheckBox
			// 
			this.showSubtitlesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.showSubtitlesCheckBox.Location = new System.Drawing.Point(16, 24);
			this.showSubtitlesCheckBox.Name = "showSubtitlesCheckBox";
			this.showSubtitlesCheckBox.Size = new System.Drawing.Size(112, 24);
			this.showSubtitlesCheckBox.TabIndex = 0;
			this.showSubtitlesCheckBox.Text = "Show subtitles";
			// 
			// subtitlesButton
			// 
			this.subtitlesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.subtitlesButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.subtitlesButton.Location = new System.Drawing.Point(208, 72);
			this.subtitlesButton.Name = "subtitlesButton";
			this.subtitlesButton.Size = new System.Drawing.Size(56, 20);
			this.subtitlesButton.TabIndex = 2;
			this.subtitlesButton.Text = "Browse";
			this.subtitlesButton.Click += new System.EventHandler(this.subtitlesButton_Click);
			// 
			// subtitlesFontTextBox
			// 
			this.subtitlesFontTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.subtitlesFontTextBox.Location = new System.Drawing.Point(32, 72);
			this.subtitlesFontTextBox.Name = "subtitlesFontTextBox";
			this.subtitlesFontTextBox.ReadOnly = true;
			this.subtitlesFontTextBox.Size = new System.Drawing.Size(168, 20);
			this.subtitlesFontTextBox.TabIndex = 1;
			this.subtitlesFontTextBox.Text = "";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(16, 56);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(150, 23);
			this.label6.TabIndex = 16;
			this.label6.Text = "Subtitle display font";
			// 
			// mpGroupBox2
			// 
			this.mpGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.mpGroupBox2.Controls.Add(this.displayTimoutTextBox);
			this.mpGroupBox2.Controls.Add(this.label5);
			this.mpGroupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.mpGroupBox2.Location = new System.Drawing.Point(16, 16);
			this.mpGroupBox2.Name = "mpGroupBox2";
			this.mpGroupBox2.Size = new System.Drawing.Size(344, 136);
			this.mpGroupBox2.TabIndex = 4;
			this.mpGroupBox2.TabStop = false;
			this.mpGroupBox2.Text = "OnScreen Display (OSD)";
			// 
			// displayTimoutTextBox
			// 
			this.displayTimoutTextBox.Location = new System.Drawing.Point(160, 27);
			this.displayTimoutTextBox.Name = "displayTimoutTextBox";
			this.displayTimoutTextBox.Size = new System.Drawing.Size(40, 20);
			this.displayTimoutTextBox.TabIndex = 16;
			this.displayTimoutTextBox.Text = "";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 30);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(150, 23);
			this.label5.TabIndex = 0;
			this.label5.Text = "Display timeout (seconds)";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.cbDatabaseLimit);
			this.groupBox2.Controls.Add(this.bDatabaseDown);
			this.groupBox2.Controls.Add(this.bDatabaseUp);
			this.groupBox2.Controls.Add(this.lvDatabase);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(16, 16);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(352, 200);
			this.groupBox2.TabIndex = 5;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "IMDB Database search results";
			// 
			// cbDatabaseLimit
			// 
			this.cbDatabaseLimit.Items.AddRange(new object[] {
																												 "0",
																												 "5",
																												 "10",
																												 "15",
																												 "20",
																												 "25"});
			this.cbDatabaseLimit.Location = new System.Drawing.Point(88, 160);
			this.cbDatabaseLimit.Name = "cbDatabaseLimit";
			this.cbDatabaseLimit.Size = new System.Drawing.Size(48, 21);
			this.cbDatabaseLimit.TabIndex = 2;
			this.cbDatabaseLimit.Text = "0";
			this.cbDatabaseLimit.SelectedIndexChanged += new System.EventHandler(this.cbDatabaseLimit_SelectedIndexChanged);
			// 
			// bDatabaseDown
			// 
			this.bDatabaseDown.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.bDatabaseDown.Location = new System.Drawing.Point(8, 160);
			this.bDatabaseDown.Name = "bDatabaseDown";
			this.bDatabaseDown.Size = new System.Drawing.Size(48, 24);
			this.bDatabaseDown.TabIndex = 1;
			this.bDatabaseDown.Text = "down";
			this.bDatabaseDown.Click += new System.EventHandler(this.bDatabaseDown_Click);
			// 
			// bDatabaseUp
			// 
			this.bDatabaseUp.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.bDatabaseUp.Location = new System.Drawing.Point(160, 160);
			this.bDatabaseUp.Name = "bDatabaseUp";
			this.bDatabaseUp.Size = new System.Drawing.Size(48, 24);
			this.bDatabaseUp.TabIndex = 3;
			this.bDatabaseUp.Text = "up";
			this.bDatabaseUp.Click += new System.EventHandler(this.bDatabaseUp_Click);
			// 
			// lvDatabase
			// 
			this.lvDatabase.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																																								 this.chDatabaseDB,
																																								 this.chDatabaseLanguage,
																																								 this.chDatabaseLimit});
			this.lvDatabase.FullRowSelect = true;
			this.lvDatabase.GridLines = true;
			this.lvDatabase.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lvDatabase.HideSelection = false;
			this.lvDatabase.Location = new System.Drawing.Point(8, 24);
			this.lvDatabase.MultiSelect = false;
			this.lvDatabase.Name = "lvDatabase";
			this.lvDatabase.Size = new System.Drawing.Size(216, 120);
			this.lvDatabase.TabIndex = 0;
			this.lvDatabase.View = System.Windows.Forms.View.Details;
			this.lvDatabase.SelectedIndexChanged += new System.EventHandler(this.lvDatabase_SelectedIndexChanged);
			// 
			// chDatabaseDB
			// 
			this.chDatabaseDB.Text = "Database";
			this.chDatabaseDB.Width = 70;
			// 
			// chDatabaseLanguage
			// 
			this.chDatabaseLanguage.Text = "Language";
			this.chDatabaseLanguage.Width = 70;
			// 
			// chDatabaseLimit
			// 
			this.chDatabaseLimit.Text = "Limit";
			this.chDatabaseLimit.Width = 55;
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.tabPage4);
			this.tabControl1.Location = new System.Drawing.Point(8, 8);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(456, 448);
			this.tabControl1.TabIndex = 6;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.groupBox1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(448, 422);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "General";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.groupBox2);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(448, 422);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "IMDB";
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.mpGroupBox1);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(448, 422);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Subtitles";
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.Add(this.mpGroupBox2);
			this.tabPage4.Location = new System.Drawing.Point(4, 22);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(448, 422);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "OSD";
			// 
			// Movies
			// 
			this.Controls.Add(this.tabControl1);
			this.Name = "Movies";
			this.Size = new System.Drawing.Size(392, 360);
			this.groupBox1.ResumeLayout(false);
			this.mpGroupBox1.ResumeLayout(false);
			this.mpGroupBox2.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.tabPage4.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void fileNameButton_Click(object sender, System.EventArgs e)
		{
			using(folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.Description = "Select the folder where movie playlists will be stored";
				folderBrowserDialog.ShowNewFolderButton = true;
				folderBrowserDialog.SelectedPath = folderNameTextBox.Text;
				DialogResult dialogResult = folderBrowserDialog.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					folderNameTextBox.Text = folderBrowserDialog.SelectedPath;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void subtitlesButton_Click(object sender, System.EventArgs e)
		{
			using(fontDialog = new FontDialog())
			{
				fontDialog.AllowScriptChange = false;
				fontDialog.ShowColor = true;
				fontDialog.FontMustExist = true;
				fontDialog.ShowEffects = true;

				fontDialog.Font = new Font(fontName, (float)fontSize, fontIsBold ? FontStyle.Bold : FontStyle.Regular);

				if(fontColor != null && fontColor.Length > 0)
					fontDialog.Color = subtitlesFontTextBox.BackColor;

				DialogResult dialogResult = fontDialog.ShowDialog(this);

				if(dialogResult == DialogResult.OK)
				{
					fontName	= fontDialog.Font.Name;
					fontSize	= (int)fontDialog.Font.Size;
					fontIsBold	= fontDialog.Font.Style == FontStyle.Bold;
					fontColor	= String.Format("{0:x}", fontDialog.Color.ToArgb());

					subtitlesFontTextBox.Text  = String.Format("{0} {1}{2}", fontName, fontSize, fontIsBold ? ", Bold" : "");

					//
					// Try to parse the specified color into a valid color
					//
					if (fontColor != null && fontColor.Length > 0)
					{
						try
						{
							int rgbColor = Int32.Parse(fontColor, NumberStyles.HexNumber);
							subtitlesFontTextBox.BackColor = Color.Black;
							subtitlesFontTextBox.ForeColor = Color.FromArgb(rgbColor);
						}
						catch {}
					}

				}
			}
		}

		private void bDatabaseDown_Click(object sender, System.EventArgs e)
		{
			// Moves the selected entry down
			// get the entry
			ListView.SelectedIndexCollection indexes = lvDatabase.SelectedIndices;
			// guilty entry?
			if (indexes.Count==1)
			{
				int index = indexes[0];
				// not the last entry?
				if (index < lvDatabase.Items.Count-1)
				{
					// save current text
					string strSub0 = lvDatabase.Items[index+1].SubItems[0].Text;
					string strSub1 = lvDatabase.Items[index+1].SubItems[1].Text;
					string strSub2 = lvDatabase.Items[index+1].SubItems[2].Text;
					// copy text
					lvDatabase.Items[index+1].SubItems[0].Text = lvDatabase.Items[index].SubItems[0].Text;
					lvDatabase.Items[index+1].SubItems[1].Text = lvDatabase.Items[index].SubItems[1].Text;
					lvDatabase.Items[index+1].SubItems[2].Text = lvDatabase.Items[index].SubItems[2].Text;
					// restore backuped text
					lvDatabase.Items[index].SubItems[0].Text = strSub0;
					lvDatabase.Items[index].SubItems[1].Text = strSub1;
					lvDatabase.Items[index].SubItems[2].Text = strSub2;
					// move the selection down
					lvDatabase.Items[index].Selected = false;
					lvDatabase.Items[index+1].Selected = true;
				}
			}			
		}

		private void bDatabaseUp_Click(object sender, System.EventArgs e)
		{
			// Moves the selected entry up
			// get the entry
			ListView.SelectedIndexCollection indexes = lvDatabase.SelectedIndices;
			// guilty entry?
			if (indexes.Count==1)
			{
				int index = indexes[0];
				// not the first entry?
				if (index > 0)
				{
					// save current text
					string strSub0 = lvDatabase.Items[index-1].SubItems[0].Text;
					string strSub1 = lvDatabase.Items[index-1].SubItems[1].Text;
					string strSub2 = lvDatabase.Items[index-1].SubItems[2].Text;
					// copy text
					lvDatabase.Items[index-1].SubItems[0].Text = lvDatabase.Items[index].SubItems[0].Text;
					lvDatabase.Items[index-1].SubItems[1].Text = lvDatabase.Items[index].SubItems[1].Text;
					lvDatabase.Items[index-1].SubItems[2].Text = lvDatabase.Items[index].SubItems[2].Text;
					// restore backuped text
					lvDatabase.Items[index].SubItems[0].Text = strSub0;
					lvDatabase.Items[index].SubItems[1].Text = strSub1;
					lvDatabase.Items[index].SubItems[2].Text = strSub2;
					// move the selection up
					lvDatabase.Items[index].Selected = false;
					lvDatabase.Items[index-1].Selected = true;
				}
			}
		}

		private void cbDatabaseLimit_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// changes the limit of the selected DB entry with that from the dropbox
			ListView.SelectedIndexCollection indexes = lvDatabase.SelectedIndices;
			if (indexes.Count==1)
			{
				// guilty entry
				int index = indexes[0];
				// copy the text
				lvDatabase.Items[index].SubItems[2].Text = this.cbDatabaseLimit.Text;
			} 
		}

		private void lvDatabase_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// updates the dropdownbox for the limits with the selected item
			ListView.SelectedIndexCollection indexes = lvDatabase.SelectedIndices;
			if (indexes.Count==1)
			{
				// guilty entry
				int index = indexes[0];
				// copy the text
				this.cbDatabaseLimit.Text = this.lvDatabase.Items[index].SubItems[2].Text;
			}
		}
	}
}

