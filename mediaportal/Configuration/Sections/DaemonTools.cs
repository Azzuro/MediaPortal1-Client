using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using MediaPortal.Util;

namespace MediaPortal.Configuration.Sections
{
	public class DeamonTools : MediaPortal.Configuration.SectionSettings
	{
		private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private System.Windows.Forms.CheckBox checkBoxDaemonTools;
    private System.Windows.Forms.TextBox textBoxDaemonTools;
    private System.Windows.Forms.Button buttonSelectFolder;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox comboBoxDrive;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ComboBox comboDriveNo;
		private System.ComponentModel.IContainer components = null;

		public DeamonTools() : this("Deamon Tools")
		{
		}

		public DeamonTools(string name) : base(name)
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
		/// <summary>
		/// 
		/// </summary>
		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
        checkBoxDaemonTools.Checked= xmlreader.GetValueAsBool("daemon", "enabled", false);
        textBoxDaemonTools.Text= xmlreader.GetValueAsString("daemon", "path", "");
        comboBoxDrive.SelectedItem=xmlreader.GetValueAsString("daemon", "drive", "E:");
        comboDriveNo.SelectedItem=xmlreader.GetValueAsInt("daemon", "driveNo", 0).ToString();
			}
      checkBoxDaemonTools_CheckedChanged(null,null);

      if (textBoxDaemonTools.Text.Length==0)
      {
        try
        {
          RegistryKey hklm = Registry.LocalMachine;
          RegistryKey subkey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Components\5DF1C3B1C87EFD376582F3A8B81F52D4");
          if (subkey != null)
          {
            textBoxDaemonTools.Text=(string)subkey.GetValue("27A3DED38A1678B4895AFEB08C30A80A");
          }
        }
        catch(Exception){}
      }
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
		    
        xmlwriter.SetValueAsBool("daemon", "enabled", checkBoxDaemonTools.Checked);
        xmlwriter.SetValue("daemon", "path", textBoxDaemonTools.Text);
        xmlwriter.SetValue("daemon", "drive", (string)comboBoxDrive.SelectedItem);
        xmlwriter.SetValue("daemon", "driveNo", Int32.Parse((string)comboDriveNo.SelectedItem));
      }
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
			this.comboDriveNo = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.comboBoxDrive = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonSelectFolder = new System.Windows.Forms.Button();
			this.textBoxDaemonTools = new System.Windows.Forms.TextBox();
			this.checkBoxDaemonTools = new System.Windows.Forms.CheckBox();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.comboDriveNo);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.comboBoxDrive);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.buttonSelectFolder);
			this.groupBox2.Controls.Add(this.textBoxDaemonTools);
			this.groupBox2.Controls.Add(this.checkBoxDaemonTools);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(8, 8);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(440, 152);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Daemon tools";
			// 
			// comboDriveNo
			// 
			this.comboDriveNo.Items.AddRange(new object[] {
																											"0",
																											"1",
																											"2",
																											"3"});
			this.comboDriveNo.Location = new System.Drawing.Point(128, 104);
			this.comboDriveNo.Name = "comboDriveNo";
			this.comboDriveNo.Size = new System.Drawing.Size(121, 21);
			this.comboDriveNo.TabIndex = 4;
			this.comboDriveNo.Text = "0";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(32, 104);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(64, 16);
			this.label4.TabIndex = 6;
			this.label4.Text = "Drive No:";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(32, 56);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 16);
			this.label3.TabIndex = 5;
			this.label3.Text = "Daemon tools:";
			// 
			// comboBoxDrive
			// 
			this.comboBoxDrive.Items.AddRange(new object[] {
																											 "D:",
																											 "E:",
																											 "F:",
																											 "G",
																											 "H:",
																											 "I:",
																											 "J:",
																											 "K:",
																											 "L:",
																											 "M:",
																											 "N:",
																											 "O:",
																											 "P:",
																											 "Q:",
																											 "R:",
																											 "S:",
																											 "T:",
																											 "U:",
																											 "V:",
																											 "W:",
																											 "X:",
																											 "Y:",
																											 "Z:"});
			this.comboBoxDrive.Location = new System.Drawing.Point(128, 80);
			this.comboBoxDrive.Name = "comboBoxDrive";
			this.comboBoxDrive.Size = new System.Drawing.Size(121, 21);
			this.comboBoxDrive.TabIndex = 3;
			this.comboBoxDrive.Text = "L:";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(32, 80);
			this.label1.Name = "label1";
			this.label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.label1.Size = new System.Drawing.Size(80, 16);
			this.label1.TabIndex = 3;
			this.label1.Text = "Virtual drive:";
			// 
			// buttonSelectFolder
			// 
			this.buttonSelectFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSelectFolder.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.buttonSelectFolder.Location = new System.Drawing.Point(368, 48);
			this.buttonSelectFolder.Name = "buttonSelectFolder";
			this.buttonSelectFolder.Size = new System.Drawing.Size(56, 20);
			this.buttonSelectFolder.TabIndex = 2;
			this.buttonSelectFolder.Text = "Browse";
			this.buttonSelectFolder.Click += new System.EventHandler(this.buttonSelectFolder_Click);
			// 
			// textBoxDaemonTools
			// 
			this.textBoxDaemonTools.Location = new System.Drawing.Point(128, 48);
			this.textBoxDaemonTools.Name = "textBoxDaemonTools";
			this.textBoxDaemonTools.Size = new System.Drawing.Size(258, 20);
			this.textBoxDaemonTools.TabIndex = 1;
			this.textBoxDaemonTools.Text = "";
			// 
			// checkBoxDaemonTools
			// 
			this.checkBoxDaemonTools.Location = new System.Drawing.Point(16, 24);
			this.checkBoxDaemonTools.Name = "checkBoxDaemonTools";
			this.checkBoxDaemonTools.Size = new System.Drawing.Size(296, 16);
			this.checkBoxDaemonTools.TabIndex = 0;
			this.checkBoxDaemonTools.Text = "Automount .iso/.bin files using Daemon tools";
			this.checkBoxDaemonTools.CheckedChanged += new System.EventHandler(this.checkBoxDaemonTools_CheckedChanged);
			// 
			// General
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.groupBox2);
			this.Name = "General";
			this.Size = new System.Drawing.Size(456, 448);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

    private void buttonSelectFolder_Click(object sender, System.EventArgs e)
    {
      using(OpenFileDialog openFileDialog = new OpenFileDialog())
      {
        openFileDialog.FileName = textBoxDaemonTools.Text;
        openFileDialog.CheckFileExists = true;
        openFileDialog.RestoreDirectory=true;
        openFileDialog.Filter= "exe files (*.exe)|*.exe";
        openFileDialog.FilterIndex = 0;
        openFileDialog.Title = "Select Daemon Tools";

        DialogResult dialogResult = openFileDialog.ShowDialog();

        if(dialogResult == DialogResult.OK)
        {
          textBoxDaemonTools.Text = openFileDialog.FileName;
        }
      }
    }

    private void checkBoxDaemonTools_CheckedChanged(object sender, System.EventArgs e)
    {
      if (checkBoxDaemonTools.Checked)
      {
        textBoxDaemonTools.Enabled=true;
        comboBoxDrive.Enabled=true;
        buttonSelectFolder.Enabled=true;
        comboDriveNo.Enabled=true;
      }
      else
      {
        textBoxDaemonTools.Enabled=false;
        comboBoxDrive.Enabled=false;
        buttonSelectFolder.Enabled=false;
        comboDriveNo.Enabled=false;
      }

    }
	}
}

