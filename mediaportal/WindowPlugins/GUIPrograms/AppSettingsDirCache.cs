using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using ProgramsDatabase;
using Programs.Utils;

namespace WindowPlugins.GUIPrograms
{
	public class AppSettingsDirCache : WindowPlugins.GUIPrograms.AppSettings
	{
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label lblImgDirectories;
		private System.Windows.Forms.TextBox txtExtensions;
		private System.Windows.Forms.TextBox txtImageDirs;
		private System.Windows.Forms.TextBox txtFiles;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnImageDirs;
		private System.Windows.Forms.CheckBox chkbUseShellExecute;
		private System.Windows.Forms.CheckBox chkbUseQuotes;
		private System.Windows.Forms.TextBox txtStartupDir;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox cbWindowStyle;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox txtArguments;
		private System.Windows.Forms.Label lblArg;
		private System.Windows.Forms.Label lblImageFile;
		private System.Windows.Forms.TextBox txtImageFile;
		private System.Windows.Forms.CheckBox chkbEnabled;
		private System.Windows.Forms.TextBox txtFilename;
		private System.Windows.Forms.TextBox txtTitle;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label lblFilename;
		private System.Windows.Forms.Button buttonFileDirectory;
		private System.Windows.Forms.Button buttonStartup;
		private System.Windows.Forms.Button buttonImageFile;
		private System.Windows.Forms.Button buttonLaunchingApp;
		private System.Windows.Forms.CheckBox chkbEnableGUIRefresh;
		private System.Windows.Forms.Label LblPinCode;
		private System.Windows.Forms.TextBox txtPinCode;
		private System.Windows.Forms.Button buttonGetExtensions;
		private System.ComponentModel.IContainer components = null;


		public AppSettingsDirCache()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
			this.txtPinCode.PasswordChar = (char)0x25CF;
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
			this.label3 = new System.Windows.Forms.Label();
			this.lblImgDirectories = new System.Windows.Forms.Label();
			this.buttonFileDirectory = new System.Windows.Forms.Button();
			this.txtExtensions = new System.Windows.Forms.TextBox();
			this.txtImageDirs = new System.Windows.Forms.TextBox();
			this.txtFiles = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.btnImageDirs = new System.Windows.Forms.Button();
			this.chkbUseShellExecute = new System.Windows.Forms.CheckBox();
			this.chkbUseQuotes = new System.Windows.Forms.CheckBox();
			this.buttonStartup = new System.Windows.Forms.Button();
			this.txtStartupDir = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.cbWindowStyle = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.txtArguments = new System.Windows.Forms.TextBox();
			this.lblArg = new System.Windows.Forms.Label();
			this.lblImageFile = new System.Windows.Forms.Label();
			this.buttonImageFile = new System.Windows.Forms.Button();
			this.txtImageFile = new System.Windows.Forms.TextBox();
			this.chkbEnabled = new System.Windows.Forms.CheckBox();
			this.txtFilename = new System.Windows.Forms.TextBox();
			this.txtTitle = new System.Windows.Forms.TextBox();
			this.lblTitle = new System.Windows.Forms.Label();
			this.lblFilename = new System.Windows.Forms.Label();
			this.buttonLaunchingApp = new System.Windows.Forms.Button();
			this.chkbEnableGUIRefresh = new System.Windows.Forms.CheckBox();
			this.LblPinCode = new System.Windows.Forms.Label();
			this.txtPinCode = new System.Windows.Forms.TextBox();
			this.buttonGetExtensions = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(0, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(184, 32);
			this.label3.TabIndex = 0;
			this.label3.Text = "Directory-Cache";
			// 
			// lblImgDirectories
			// 
			this.lblImgDirectories.Location = new System.Drawing.Point(0, 312);
			this.lblImgDirectories.Name = "lblImgDirectories";
			this.lblImgDirectories.TabIndex = 25;
			this.lblImgDirectories.Text = "Image directories:";
			// 
			// buttonFileDirectory
			// 
			this.buttonFileDirectory.Location = new System.Drawing.Point(376, 264);
			this.buttonFileDirectory.Name = "buttonFileDirectory";
			this.buttonFileDirectory.Size = new System.Drawing.Size(20, 20);
			this.buttonFileDirectory.TabIndex = 22;
			this.buttonFileDirectory.Text = "...";
			this.buttonFileDirectory.Click += new System.EventHandler(this.buttonFileDirectory_Click);
			// 
			// txtExtensions
			// 
			this.txtExtensions.Location = new System.Drawing.Point(120, 288);
			this.txtExtensions.Name = "txtExtensions";
			this.txtExtensions.Size = new System.Drawing.Size(250, 20);
			this.txtExtensions.TabIndex = 24;
			this.txtExtensions.Text = "";
			// 
			// txtImageDirs
			// 
			this.txtImageDirs.Location = new System.Drawing.Point(120, 312);
			this.txtImageDirs.Multiline = true;
			this.txtImageDirs.Name = "txtImageDirs";
			this.txtImageDirs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtImageDirs.Size = new System.Drawing.Size(250, 64);
			this.txtImageDirs.TabIndex = 26;
			this.txtImageDirs.Text = "txtImageDirs";
			// 
			// txtFiles
			// 
			this.txtFiles.Location = new System.Drawing.Point(120, 264);
			this.txtFiles.Name = "txtFiles";
			this.txtFiles.Size = new System.Drawing.Size(250, 20);
			this.txtFiles.TabIndex = 21;
			this.txtFiles.Text = "";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(0, 288);
			this.label4.Name = "label4";
			this.label4.TabIndex = 23;
			this.label4.Text = "File-Extensions:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(0, 264);
			this.label2.Name = "label2";
			this.label2.TabIndex = 20;
			this.label2.Text = "File Directory:";
			// 
			// btnImageDirs
			// 
			this.btnImageDirs.Location = new System.Drawing.Point(376, 312);
			this.btnImageDirs.Name = "btnImageDirs";
			this.btnImageDirs.Size = new System.Drawing.Size(20, 20);
			this.btnImageDirs.TabIndex = 27;
			this.btnImageDirs.Text = "...";
			this.btnImageDirs.Click += new System.EventHandler(this.btnImageDirs_Click);
			// 
			// chkbUseShellExecute
			// 
			this.chkbUseShellExecute.Location = new System.Drawing.Point(120, 88);
			this.chkbUseShellExecute.Name = "chkbUseShellExecute";
			this.chkbUseShellExecute.Size = new System.Drawing.Size(176, 24);
			this.chkbUseShellExecute.TabIndex = 7;
			this.chkbUseShellExecute.Text = "Startup using ShellExecute";
			// 
			// chkbUseQuotes
			// 
			this.chkbUseQuotes.Checked = true;
			this.chkbUseQuotes.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkbUseQuotes.Location = new System.Drawing.Point(120, 232);
			this.chkbUseQuotes.Name = "chkbUseQuotes";
			this.chkbUseQuotes.Size = new System.Drawing.Size(184, 24);
			this.chkbUseQuotes.TabIndex = 19;
			this.chkbUseQuotes.Text = "Quotes around Filenames";
			// 
			// buttonStartup
			// 
			this.buttonStartup.Location = new System.Drawing.Point(376, 208);
			this.buttonStartup.Name = "buttonStartup";
			this.buttonStartup.Size = new System.Drawing.Size(20, 20);
			this.buttonStartup.TabIndex = 18;
			this.buttonStartup.Text = "...";
			this.buttonStartup.Click += new System.EventHandler(this.buttonStartup_Click);
			// 
			// txtStartupDir
			// 
			this.txtStartupDir.Location = new System.Drawing.Point(120, 208);
			this.txtStartupDir.Name = "txtStartupDir";
			this.txtStartupDir.Size = new System.Drawing.Size(250, 20);
			this.txtStartupDir.TabIndex = 17;
			this.txtStartupDir.Text = "";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(0, 208);
			this.label5.Name = "label5";
			this.label5.TabIndex = 16;
			this.label5.Text = "Startup Directory:";
			// 
			// cbWindowStyle
			// 
			this.cbWindowStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbWindowStyle.Items.AddRange(new object[] {
															   "Normal",
															   "Minimized",
															   "Maximized",
															   "Hidden"});
			this.cbWindowStyle.Location = new System.Drawing.Point(120, 184);
			this.cbWindowStyle.Name = "cbWindowStyle";
			this.cbWindowStyle.Size = new System.Drawing.Size(250, 21);
			this.cbWindowStyle.TabIndex = 15;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(0, 184);
			this.label6.Name = "label6";
			this.label6.TabIndex = 14;
			this.label6.Text = "Window-Style:";
			// 
			// txtArguments
			// 
			this.txtArguments.Location = new System.Drawing.Point(120, 160);
			this.txtArguments.Name = "txtArguments";
			this.txtArguments.Size = new System.Drawing.Size(250, 20);
			this.txtArguments.TabIndex = 13;
			this.txtArguments.Text = "";
			// 
			// lblArg
			// 
			this.lblArg.Location = new System.Drawing.Point(0, 160);
			this.lblArg.Name = "lblArg";
			this.lblArg.TabIndex = 12;
			this.lblArg.Text = "Arguments:";
			// 
			// lblImageFile
			// 
			this.lblImageFile.Location = new System.Drawing.Point(0, 136);
			this.lblImageFile.Name = "lblImageFile";
			this.lblImageFile.Size = new System.Drawing.Size(80, 20);
			this.lblImageFile.TabIndex = 9;
			this.lblImageFile.Text = "Imagefile:";
			// 
			// buttonImageFile
			// 
			this.buttonImageFile.Location = new System.Drawing.Point(376, 136);
			this.buttonImageFile.Name = "buttonImageFile";
			this.buttonImageFile.Size = new System.Drawing.Size(20, 20);
			this.buttonImageFile.TabIndex = 11;
			this.buttonImageFile.Text = "...";
			this.buttonImageFile.Click += new System.EventHandler(this.buttonImageFile_Click);
			// 
			// txtImageFile
			// 
			this.txtImageFile.Location = new System.Drawing.Point(120, 136);
			this.txtImageFile.Name = "txtImageFile";
			this.txtImageFile.Size = new System.Drawing.Size(250, 20);
			this.txtImageFile.TabIndex = 10;
			this.txtImageFile.Text = "";
			// 
			// chkbEnabled
			// 
			this.chkbEnabled.Checked = true;
			this.chkbEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkbEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.chkbEnabled.Location = new System.Drawing.Point(320, 8);
			this.chkbEnabled.Name = "chkbEnabled";
			this.chkbEnabled.Size = new System.Drawing.Size(72, 24);
			this.chkbEnabled.TabIndex = 29;
			this.chkbEnabled.Text = "Enabled";
			this.chkbEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtFilename
			// 
			this.txtFilename.Location = new System.Drawing.Point(120, 64);
			this.txtFilename.Name = "txtFilename";
			this.txtFilename.Size = new System.Drawing.Size(250, 20);
			this.txtFilename.TabIndex = 4;
			this.txtFilename.Text = "";
			// 
			// txtTitle
			// 
			this.txtTitle.Location = new System.Drawing.Point(120, 40);
			this.txtTitle.Name = "txtTitle";
			this.txtTitle.Size = new System.Drawing.Size(250, 20);
			this.txtTitle.TabIndex = 2;
			this.txtTitle.Text = "";
			// 
			// lblTitle
			// 
			this.lblTitle.Location = new System.Drawing.Point(0, 40);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.TabIndex = 1;
			this.lblTitle.Text = "Title:";
			// 
			// lblFilename
			// 
			this.lblFilename.Location = new System.Drawing.Point(0, 64);
			this.lblFilename.Name = "lblFilename";
			this.lblFilename.Size = new System.Drawing.Size(120, 20);
			this.lblFilename.TabIndex = 3;
			this.lblFilename.Text = "Launching Application:";
			// 
			// buttonLaunchingApp
			// 
			this.buttonLaunchingApp.Location = new System.Drawing.Point(376, 64);
			this.buttonLaunchingApp.Name = "buttonLaunchingApp";
			this.buttonLaunchingApp.Size = new System.Drawing.Size(20, 20);
			this.buttonLaunchingApp.TabIndex = 5;
			this.buttonLaunchingApp.Text = "...";
			this.buttonLaunchingApp.Click += new System.EventHandler(this.buttonLaunchingApp_Click);
			// 
			// chkbEnableGUIRefresh
			// 
			this.chkbEnableGUIRefresh.Checked = true;
			this.chkbEnableGUIRefresh.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkbEnableGUIRefresh.Location = new System.Drawing.Point(120, 376);
			this.chkbEnableGUIRefresh.Name = "chkbEnableGUIRefresh";
			this.chkbEnableGUIRefresh.Size = new System.Drawing.Size(208, 24);
			this.chkbEnableGUIRefresh.TabIndex = 28;
			this.chkbEnableGUIRefresh.Text = "Allow refresh in MediaPortal";
			// 
			// LblPinCode
			// 
			this.LblPinCode.Location = new System.Drawing.Point(0, 112);
			this.LblPinCode.Name = "LblPinCode";
			this.LblPinCode.Size = new System.Drawing.Size(96, 16);
			this.LblPinCode.TabIndex = 6;
			this.LblPinCode.Text = "Pin-Code";
			// 
			// txtPinCode
			// 
			this.txtPinCode.Location = new System.Drawing.Point(120, 112);
			this.txtPinCode.MaxLength = 4;
			this.txtPinCode.Name = "txtPinCode";
			this.txtPinCode.Size = new System.Drawing.Size(64, 20);
			this.txtPinCode.TabIndex = 8;
			this.txtPinCode.Text = "";
			this.txtPinCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPinCode_KeyPress);
			// 
			// buttonGetExtensions
			// 
			this.buttonGetExtensions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.buttonGetExtensions.Location = new System.Drawing.Point(376, 288);
			this.buttonGetExtensions.Name = "buttonGetExtensions";
			this.buttonGetExtensions.Size = new System.Drawing.Size(20, 20);
			this.buttonGetExtensions.TabIndex = 30;
			this.buttonGetExtensions.Text = "?";
			this.toolTip.SetToolTip(this.buttonGetExtensions, "Read all available file extensions from the file directory");
			this.buttonGetExtensions.Click += new System.EventHandler(this.buttonGetExtensions_Click);
			// 
			// AppSettingsDirCache
			// 
			this.Controls.Add(this.buttonGetExtensions);
			this.Controls.Add(this.LblPinCode);
			this.Controls.Add(this.txtPinCode);
			this.Controls.Add(this.chkbEnableGUIRefresh);
			this.Controls.Add(this.lblImgDirectories);
			this.Controls.Add(this.buttonFileDirectory);
			this.Controls.Add(this.txtExtensions);
			this.Controls.Add(this.txtImageDirs);
			this.Controls.Add(this.txtFiles);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnImageDirs);
			this.Controls.Add(this.chkbUseShellExecute);
			this.Controls.Add(this.chkbUseQuotes);
			this.Controls.Add(this.buttonStartup);
			this.Controls.Add(this.txtStartupDir);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.cbWindowStyle);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.txtArguments);
			this.Controls.Add(this.lblArg);
			this.Controls.Add(this.lblImageFile);
			this.Controls.Add(this.buttonImageFile);
			this.Controls.Add(this.txtImageFile);
			this.Controls.Add(this.chkbEnabled);
			this.Controls.Add(this.txtFilename);
			this.Controls.Add(this.txtTitle);
			this.Controls.Add(this.lblTitle);
			this.Controls.Add(this.lblFilename);
			this.Controls.Add(this.buttonLaunchingApp);
			this.Controls.Add(this.label3);
			this.Name = "AppSettingsDirCache";
			this.Size = new System.Drawing.Size(408, 408);
			this.Load += new System.EventHandler(this.AppSettingsDirCache_Load);
			this.ResumeLayout(false);

		}
		#endregion

		public override bool AppObj2Form(AppItem curApp)
		{
			this.chkbEnabled.Checked = curApp.Enabled;
			this.txtTitle.Text = curApp.Title;
			this.txtFilename.Text = curApp.Filename;
			this.txtArguments.Text = curApp.Arguments;
			SetWindowStyle(curApp.WindowStyle);
			this.txtStartupDir.Text = curApp.Startupdir;
			this.chkbUseShellExecute.Checked = (curApp.UseShellExecute);
			this.chkbUseQuotes.Checked = (curApp.UseQuotes);
			this.txtImageFile.Text = curApp.Imagefile;
			this.txtFiles.Text = curApp.FileDirectory;
			this.txtImageDirs.Text = curApp.ImageDirectory;
			this.txtExtensions.Text = curApp.ValidExtensions;
			this.chkbEnableGUIRefresh.Checked = curApp.EnableGUIRefresh;
			if (curApp.Pincode > 0)
			{
				this.txtPinCode.Text = String.Format("{0}", curApp.Pincode);
			}
			else
			{
				this.txtPinCode.Text = "";
			}
			return true;
		}


		public override void Form2AppObj(AppItem curApp)
		{
			curApp.Enabled = this.chkbEnabled.Checked;
			curApp.Title = this.txtTitle.Text;
			curApp.Filename = this.txtFilename.Text;
			curApp.Arguments = this.txtArguments.Text;
			curApp.WindowStyle = GetSelectedWindowStyle();
			curApp.Startupdir = this.txtStartupDir.Text;
			curApp.UseShellExecute = (this.chkbUseShellExecute.Checked);
			curApp.UseQuotes = (this.chkbUseQuotes.Checked);
			curApp.SourceType = myProgSourceType.DIRCACHE;
			curApp.Imagefile = this.txtImageFile.Text;
			curApp.FileDirectory = this.txtFiles.Text;
			curApp.ImageDirectory = this.txtImageDirs.Text;
			curApp.ValidExtensions = this.txtExtensions.Text;
			curApp.EnableGUIRefresh = this.chkbEnableGUIRefresh.Checked;
			curApp.Pincode = ProgramUtils.StrToIntDef(this.txtPinCode.Text, -1);
		}

		public override bool EntriesOK(AppItem curApp)
		{
			m_Checker.Clear();
			m_Checker.DoCheck(txtTitle.Text != "", "No title entered!");
			if (txtFilename.Text == "") 
			{
				m_Checker.DoCheck(chkbUseShellExecute.Checked, "No launching filename entered!");
			}
			m_Checker.DoCheck(txtFiles.Text != "", "No filedirectory entered!");
			m_Checker.DoCheck(txtExtensions.Text != "", "No file extensions entered!");

			if (!m_Checker.IsOk)
			{
				string strHeader = "The following entries are invalid: \r\n\r\n";
				string strFooter = "\r\n\r\n(Click DELETE to remove this item)";
				System.Windows.Forms.MessageBox.Show(strHeader + m_Checker.Problems + strFooter, "Invalid Entries");
			}
			else
			{
			}
			return m_Checker.IsOk;
		}

		private void SetWindowStyle(ProcessWindowStyle val) 
		{
			switch (val)
			{
				case ProcessWindowStyle.Normal:
					cbWindowStyle.SelectedIndex = 0;
					break;
				case ProcessWindowStyle.Minimized:
					cbWindowStyle.SelectedIndex = 1;
					break;
				case ProcessWindowStyle.Maximized:
					cbWindowStyle.SelectedIndex = 2;
					break;
				case ProcessWindowStyle.Hidden:
					cbWindowStyle.SelectedIndex = 3;
					break;
			}

		}
		
		private ProcessWindowStyle GetSelectedWindowStyle()
		{
			if (this.cbWindowStyle.SelectedIndex == 1)
			{
				return ProcessWindowStyle.Minimized;
			}
			else if (this.cbWindowStyle.SelectedIndex == 2)
			{
				return ProcessWindowStyle.Maximized;
			} 
			else if (this.cbWindowStyle.SelectedIndex == 3)
			{
				return ProcessWindowStyle.Hidden;
			}
			else
				return ProcessWindowStyle.Normal;

		}

		private void AppSettingsDirCache_Load(object sender, System.EventArgs e)
		{
			// set tooltip-stuff..... 
			toolTip.SetToolTip(txtTitle, "This text will appear in the listitem of MediaPortal\r\n(mandatory)");
			toolTip.SetToolTip(txtFiles, "First directory to display in MediaPortal.\r\n(mandatory)");
			toolTip.SetToolTip(chkbUseShellExecute, "Enable this if you want to run a program that is associated with a specific file-" +
				"extension.\r\nYou can omit the \"Launching Application\" in this case.");
			toolTip.SetToolTip(chkbUseQuotes, "Quotes are usually needed to handle filenames with spaces correctly. \r\nAvoid double" +
				" quotes though!");
			toolTip.SetToolTip(txtStartupDir, "Optional path that is passed as the launch-directory \r\n\r\n(advanced hint: Use %FILEDIR" +
				"% if you want to use the directory where the launched file is stored)");
			toolTip.SetToolTip(cbWindowStyle, "Appearance of the launched program. \r\nTry HIDDEN or MINIMIZED for a seamless integr" +
				"ation in MediaPortal");
			toolTip.SetToolTip(txtArguments, "Optional arguments that are needed to launch the program \r\n\r\n(advanced hint: Use %FIL" +
				"E% if the filename needs to be placed in some specific place between several arg" +
				"uments)");
			toolTip.SetToolTip(txtImageFile, "Optional filename for an image to display in MediaPortal");
			toolTip.SetToolTip(chkbEnabled, "Only enabled items will appear in MediaPortal");
			toolTip.SetToolTip(txtFilename, "Program you wish to execute, include the full path (mandatory if ShellExecute is " +
				"OFF)");
			toolTip.SetToolTip(txtExtensions, "Only files with matching extensions will be displayed. \r\nSeparate several extension" +
				"s by a coma (.zip,.txt)\r\n(mandatory)");
			toolTip.SetToolTip(txtImageDirs, "Optional directory where MediaPortal searches for matching images. \r\n MediaPort" +
				"al will cycle through all the directories and display a mini-slideshow of all ma" +
				"tching images.");
			toolTip.SetToolTip(chkbEnableGUIRefresh, "Check this if users can run the import through the REFRESH button in MediaPortal.");
		}

		private void buttonLaunchingApp_Click(object sender, System.EventArgs e)
		{
			dialogFile.FileName = txtFilename.Text;
			dialogFile.RestoreDirectory = true;
			if( dialogFile.ShowDialog(null) == DialogResult.OK )
			{
				txtFilename.Text = dialogFile.FileName;
			}
		}

		private void buttonImageFile_Click(object sender, System.EventArgs e)
		{
			dialogFile.FileName = txtImageFile.Text;
			dialogFile.RestoreDirectory = true;
			if( dialogFile.ShowDialog(null) == DialogResult.OK )
			{
				txtImageFile.Text = dialogFile.FileName;
			}
		}

		private void buttonStartup_Click(object sender, System.EventArgs e)
		{
			dialogFolder.SelectedPath = txtStartupDir.Text;
			if( dialogFolder.ShowDialog( null ) == DialogResult.OK )
			{
				txtStartupDir.Text = dialogFolder.SelectedPath;
			}
		}

		private void buttonFileDirectory_Click(object sender, System.EventArgs e)
		{
			dialogFolder.SelectedPath = txtFiles.Text;
			if( dialogFolder.ShowDialog( null ) == DialogResult.OK )
			{
				txtFiles.Text = dialogFolder.SelectedPath;
			}
		}

		private void btnImageDirs_Click(object sender, System.EventArgs e)
		{
			if (txtImageDirs.Text != "")
			{
				dialogFolder.SelectedPath = txtImageDirs.Lines[0];
			}
			if( dialogFolder.ShowDialog( null ) == DialogResult.OK )
			{
				string strSep = "";
				if (txtImageDirs.Text != "")
				{
					strSep = "\r\n";
				}
				txtImageDirs.Text = txtImageDirs.Text + strSep + dialogFolder.SelectedPath;
			}
		}

		private void txtPinCode_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			//
			// Allow only numbers, and backspace.
			//
			if(char.IsNumber(e.KeyChar) == false && e.KeyChar != 8)
			{
				e.Handled = true;
			}
		
		}

		private void buttonGetExtensions_Click(object sender, System.EventArgs e)
		{
			txtExtensions.Text = ProgramUtils.GetAvailableExtensions(txtFiles.Text);
		}


		public override void LoadFromAppItem(AppItem tempApp)
		{
			this.txtTitle.Text = tempApp.Title;
			if (txtFilename.Text == "")
			{
				this.txtFilename.Text = tempApp.Filename;
			}
			this.txtArguments.Text = tempApp.Arguments;
			SetWindowStyle(tempApp.WindowStyle);
			this.chkbUseShellExecute.Checked = (tempApp.UseShellExecute);
			this.chkbUseQuotes.Checked = (tempApp.UseQuotes);
		}

	}
}

