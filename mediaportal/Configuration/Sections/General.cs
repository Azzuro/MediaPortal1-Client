using System;
using System.Globalization;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using MediaPortal.Util;

namespace MediaPortal.Configuration.Sections
{
	public class General : MediaPortal.Configuration.SectionSettings
	{
		const string LanguageDirectory = @"language\";
		private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox1;
		private System.Windows.Forms.ComboBox languageComboBox;
		private System.Windows.Forms.Label label2;
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
		private System.Windows.Forms.CheckedListBox settingsCheckedListBox;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.NumericUpDown numericUpDown1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private System.ComponentModel.IContainer components = null;

		public General() : this("General")
		{
		}

		public General(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			//
			// Populate comboboxes
			//
			LoadLanguages();
		}

		private void LoadLanguages()
		{
			// Get system language
			string strLongLanguage = CultureInfo.CurrentCulture.EnglishName;
			int iTrimIndex = strLongLanguage.IndexOf(" ", 0, strLongLanguage.Length);
			string strShortLanguage = strLongLanguage.Substring(0, iTrimIndex);

			bool bExactLanguageFound = false;
			if(Directory.Exists(LanguageDirectory))
			{
				string[] folders = Directory.GetDirectories(LanguageDirectory, "*.*");

				foreach(string folder in folders)
				{
					string fileName = folder.Substring(@"language\".Length);

					//
					// Exclude cvs folder
					//
					if(fileName.ToLower() != "cvs")
					{
						if(fileName.Length > 0)
						{
							fileName = fileName.Substring(0, 1).ToUpper() + fileName.Substring(1);
							languageComboBox.Items.Add(fileName);

							// Check language file to user region language
							if (fileName.ToLower() == strLongLanguage.ToLower())
							{
								languageComboBox.Text = fileName;
								bExactLanguageFound = true;
							}
							else if (!bExactLanguageFound && (fileName.ToLower() == strShortLanguage.ToLower()))
							{
								languageComboBox.Text = fileName;
							}							
						}
					}
				}
			}

			if (languageComboBox.Text == "")
			{
				languageComboBox.Text = "English";
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

		string[][] sectionEntries = new string[][] { 
												new string[] { "general", "startfullscreen", "false" },
												new string[] { "general", "autohidemouse", "false" },
												new string[] { "general", "mousesupport", "true" }, 
                        new string[] { "general", "hideextensions", "true" },
                        new string[] { "general", "animations", "true" },
												new string[] { "general", "autostart", "false" },
												new string[] { "general", "baloontips", "false" },
												new string[] { "general", "dblclickasrightclick", "false" },
												new string[] { "general", "hidetaskbar", "true" },
												new string[] { "general", "alwaysontop", "false" },
												new string[] { "general", "exclusivemode", "false" },
												new string[] { "general", "useVMR9ZapOSD", "false" },
                        new string[] { "general", "enableguisounds", "true" }
												};

		/// <summary>
		/// 
		/// </summary>
		public override void LoadSettings()
		{
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				//
				// Load general settings
				//
				for(int index = 0; index < sectionEntries.Length; index++)
				{
					string[] currentSection = sectionEntries[index];

					settingsCheckedListBox.SetItemChecked(index, xmlreader.GetValueAsBool(currentSection[0], currentSection[1], bool.Parse(currentSection[2])));
				}
	
				//
				// Set language
				//
				languageComboBox.Text = xmlreader.GetValueAsString("skin", "language", languageComboBox.Text);
				numericUpDown1.Value=xmlreader.GetValueAsInt("vmr9OSDSkin","alphaValue",10);

			}

		}

		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				//
				// Load general settings
				//
				for(int index = 0; index < sectionEntries.Length; index++)
				{
					string[] currentSection = sectionEntries[index];
					xmlwriter.SetValueAsBool(currentSection[0], currentSection[1], settingsCheckedListBox.GetItemChecked(index));
				}
	
				//
				// Set language
				string prevLanguage = xmlwriter.GetValueAsString("skin", "language", "English");
				string skin         = xmlwriter.GetValueAsString("skin", "name", "mce");
				if (prevLanguage!=languageComboBox.Text)
				{
					Utils.DeleteFiles(@"skin\"+skin+@"\fonts","*");
				}
				xmlwriter.SetValue("skin", "language", languageComboBox.Text);
        
				xmlwriter.SetValue("vmr9OSDSkin","alphaValue",numericUpDown1.Value);
      }

			try
			{
				RegistryKey hkcu=Registry.CurrentUser;
				RegistryKey subkey;
				if (settingsCheckedListBox.GetItemChecked(5))
				{
					string fileName=String.Format("\"{0}\"",System.IO.Path.GetFullPath("mediaportal.exe"));
					subkey=hkcu.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run",true);
					subkey.SetValue("Mediaportal", fileName);
					subkey.Close();
				}
				else
				{
					subkey=hkcu.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run",true);
					subkey.DeleteValue("Mediaportal",false);
					subkey.Close();
				}

				Int32 iValue=1;
				if (settingsCheckedListBox.GetItemChecked(6))
				{
					iValue=0;
				}
				subkey=hkcu.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer",true);
				subkey.SetValue("EnableBalloonTips", iValue);
				subkey.Close();
				hkcu.Close();
				
			}
			catch(Exception)
			{
			}
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.mpGroupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.languageComboBox = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.settingsCheckedListBox = new System.Windows.Forms.CheckedListBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.mpGroupBox1.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			this.SuspendLayout();
			// 
			// mpGroupBox1
			// 
			this.mpGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.mpGroupBox1.Controls.Add(this.languageComboBox);
			this.mpGroupBox1.Controls.Add(this.label2);
			this.mpGroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.mpGroupBox1.Location = new System.Drawing.Point(8, 8);
			this.mpGroupBox1.Name = "mpGroupBox1";
			this.mpGroupBox1.Size = new System.Drawing.Size(440, 56);
			this.mpGroupBox1.TabIndex = 1;
			this.mpGroupBox1.TabStop = false;
			this.mpGroupBox1.Text = "Language Settings";
			// 
			// languageComboBox
			// 
			this.languageComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.languageComboBox.Location = new System.Drawing.Point(168, 16);
			this.languageComboBox.Name = "languageComboBox";
			this.languageComboBox.Size = new System.Drawing.Size(256, 21);
			this.languageComboBox.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(150, 23);
			this.label2.TabIndex = 4;
			this.label2.Text = "Display language";
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.settingsCheckedListBox);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 72);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 264);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "General Settings";
			// 
			// settingsCheckedListBox
			// 
			this.settingsCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.settingsCheckedListBox.Items.AddRange(new object[] {
																		"Start Media Portal in fullscreen mode",
																		"Auto hide mouse cursor when inactive",
																		"Show special mouse controls (scrollbars, etc)",
																		"Dont show file extensions like .mp3, .avi, .mpg,...",
																		"Enable animations",
																		"Autostart Mediaportal when windows starts",
																		"Disable Windows XP balloon tips",
																		"Use mouse left double click as right click",
																		"Hide taskbar in fullscreen mode",
																		"MediaPortal always on top",
																		"use Exclusive DirectX Mode for fullscreen tv/video",
																		"use VMR9-ZapOSD (GUIZapOSD will not displayed then)",
																		"enable GUI sound effects"});
			this.settingsCheckedListBox.Location = new System.Drawing.Point(16, 24);
			this.settingsCheckedListBox.Name = "settingsCheckedListBox";
			this.settingsCheckedListBox.Size = new System.Drawing.Size(416, 214);
			this.settingsCheckedListBox.TabIndex = 0;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.numericUpDown1);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(8, 352);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(440, 64);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "VMR9 OSD Settings";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(184, 32);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(128, 16);
			this.label3.TabIndex = 2;
			this.label3.Text = "(10 = solid, 0 = invisible)";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "OSD Alpha level:";
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.Location = new System.Drawing.Point(116, 30);
			this.numericUpDown1.Maximum = new System.Decimal(new int[] {
																		   10,
																		   0,
																		   0,
																		   0});
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(56, 20);
			this.numericUpDown1.TabIndex = 0;
			this.numericUpDown1.Value = new System.Decimal(new int[] {
																		 10,
																		 0,
																		 0,
																		 0});
			// 
			// General
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.mpGroupBox1);
			this.Name = "General";
			this.Size = new System.Drawing.Size(456, 448);
			this.mpGroupBox1.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

    
	}
}

