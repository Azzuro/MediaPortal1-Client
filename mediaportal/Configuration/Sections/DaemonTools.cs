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
    private System.Windows.Forms.CheckBox checkBoxAskBeforePlaying;
		private System.ComponentModel.IContainer components = null;

		public DeamonTools() : this("Daemon Tools")
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
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
        checkBoxDaemonTools.Checked= xmlreader.GetValueAsBool("daemon", "enabled", false);
        textBoxDaemonTools.Text= xmlreader.GetValueAsString("daemon", "path", "");
        comboBoxDrive.SelectedItem=xmlreader.GetValueAsString("daemon", "drive", "E:");
        comboDriveNo.SelectedItem=xmlreader.GetValueAsInt("daemon", "driveNo", 0).ToString();
        checkBoxAskBeforePlaying.Checked=xmlreader.GetValueAsBool("daemon", "askbeforeplaying", false);
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
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
		    
        xmlwriter.SetValueAsBool("daemon", "enabled", checkBoxDaemonTools.Checked);
        xmlwriter.SetValue("daemon", "path", textBoxDaemonTools.Text);
        xmlwriter.SetValue("daemon", "drive", (string)comboBoxDrive.SelectedItem);
        xmlwriter.SetValue("daemon", "driveNo", Int32.Parse((string)comboDriveNo.SelectedItem));
        xmlwriter.SetValueAsBool("daemon", "askbeforeplaying", checkBoxAskBeforePlaying.Checked);
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
      this.buttonSelectFolder = new System.Windows.Forms.Button();
      this.comboDriveNo = new System.Windows.Forms.ComboBox();
      this.comboBoxDrive = new System.Windows.Forms.ComboBox();
      this.textBoxDaemonTools = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.checkBoxDaemonTools = new System.Windows.Forms.CheckBox();
      this.checkBoxAskBeforePlaying = new System.Windows.Forms.CheckBox();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.buttonSelectFolder);
      this.groupBox2.Controls.Add(this.comboDriveNo);
      this.groupBox2.Controls.Add(this.comboBoxDrive);
      this.groupBox2.Controls.Add(this.textBoxDaemonTools);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Controls.Add(this.checkBoxDaemonTools);
      this.groupBox2.Controls.Add(this.checkBoxAskBeforePlaying);
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(0, 0);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(472, 168);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Settings";
      // 
      // buttonSelectFolder
      // 
      this.buttonSelectFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonSelectFolder.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonSelectFolder.Location = new System.Drawing.Point(384, 52);
      this.buttonSelectFolder.Name = "buttonSelectFolder";
      this.buttonSelectFolder.Size = new System.Drawing.Size(72, 22);
      this.buttonSelectFolder.TabIndex = 3;
      this.buttonSelectFolder.Text = "Browse";
      this.buttonSelectFolder.Click += new System.EventHandler(this.buttonSelectFolder_Click);
      // 
      // comboDriveNo
      // 
      this.comboDriveNo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.comboDriveNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboDriveNo.Items.AddRange(new object[] {
                                                      "0",
                                                      "1",
                                                      "2",
                                                      "3"});
      this.comboDriveNo.Location = new System.Drawing.Point(168, 100);
      this.comboDriveNo.Name = "comboDriveNo";
      this.comboDriveNo.Size = new System.Drawing.Size(288, 21);
      this.comboDriveNo.TabIndex = 7;
      // 
      // comboBoxDrive
      // 
      this.comboBoxDrive.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxDrive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDrive.Items.AddRange(new object[] {
                                                       "D:",
                                                       "E:",
                                                       "F:",
                                                       "G:",
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
      this.comboBoxDrive.Location = new System.Drawing.Point(168, 76);
      this.comboBoxDrive.Name = "comboBoxDrive";
      this.comboBoxDrive.Size = new System.Drawing.Size(288, 21);
      this.comboBoxDrive.TabIndex = 5;
      // 
      // textBoxDaemonTools
      // 
      this.textBoxDaemonTools.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxDaemonTools.Location = new System.Drawing.Point(168, 53);
      this.textBoxDaemonTools.Name = "textBoxDaemonTools";
      this.textBoxDaemonTools.Size = new System.Drawing.Size(208, 20);
      this.textBoxDaemonTools.TabIndex = 2;
      this.textBoxDaemonTools.Text = "";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 104);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(64, 16);
      this.label4.TabIndex = 6;
      this.label4.Text = "Drive No:";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 56);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(80, 16);
      this.label3.TabIndex = 1;
      this.label3.Text = "Daemon tools:";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 80);
      this.label1.Name = "label1";
      this.label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.label1.Size = new System.Drawing.Size(80, 16);
      this.label1.TabIndex = 4;
      this.label1.Text = "Virtual drive:";
      // 
      // checkBoxDaemonTools
      // 
      this.checkBoxDaemonTools.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxDaemonTools.Location = new System.Drawing.Point(16, 24);
      this.checkBoxDaemonTools.Name = "checkBoxDaemonTools";
      this.checkBoxDaemonTools.Size = new System.Drawing.Size(296, 16);
      this.checkBoxDaemonTools.TabIndex = 0;
      this.checkBoxDaemonTools.Text = "Automount .iso/.bin files using Daemon tools";
      this.checkBoxDaemonTools.CheckedChanged += new System.EventHandler(this.checkBoxDaemonTools_CheckedChanged);
      // 
      // checkBoxAskBeforePlaying
      // 
      this.checkBoxAskBeforePlaying.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxAskBeforePlaying.Location = new System.Drawing.Point(16, 136);
      this.checkBoxAskBeforePlaying.Name = "checkBoxAskBeforePlaying";
      this.checkBoxAskBeforePlaying.Size = new System.Drawing.Size(176, 16);
      this.checkBoxAskBeforePlaying.TabIndex = 8;
      this.checkBoxAskBeforePlaying.Text = "Ask before playing .iso/.bin files";
      // 
      // DeamonTools
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.groupBox2);
      this.Name = "DaemonTools";
      this.Size = new System.Drawing.Size(472, 408);
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
        checkBoxAskBeforePlaying.Enabled=true;
      }
      else
      {
        textBoxDaemonTools.Enabled=false;
        comboBoxDrive.Enabled=false;
        buttonSelectFolder.Enabled=false;
        comboDriveNo.Enabled=false;
        checkBoxAskBeforePlaying.Enabled=false;
      }

    }
	}
}

