using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration.Sections
{
  public class Remote : MediaPortal.Configuration.SectionSettings
  {
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxMCE;
    private System.Windows.Forms.PictureBox pictureBoxUSA;
    private System.Windows.Forms.PictureBox pictureBoxEU;
    private System.Windows.Forms.RadioButton radioButtonUSA;
    private System.Windows.Forms.RadioButton radioButtonEurope;
    private System.Windows.Forms.GroupBox groupBoxSettings;
    private System.Windows.Forms.Label labelPowerButton;
    private System.Windows.Forms.ComboBox comboBoxPowerButton;
    private System.Windows.Forms.Label labelDelay;
    public System.Windows.Forms.TrackBar trackBarDelay;
    private System.Windows.Forms.CheckBox checkBoxAllowExternal;
    private System.Windows.Forms.CheckBox checkBoxKeepControl;
    private System.Windows.Forms.CheckBox checkBoxVerboseLog;
    private System.Windows.Forms.Button buttonDefault;
    private System.Windows.Forms.CheckBox checkBoxHCW;
    private System.Windows.Forms.Label infoDriverStatus;
    private System.Windows.Forms.GroupBox groupBoxInformation;
    private System.Windows.Forms.TabControl tabControlRemotes;
    private System.Windows.Forms.TabPage tabPageMCE;
    private System.Windows.Forms.TabPage tabPageHCW;
    private System.Windows.Forms.Label label2sec;
    private System.Windows.Forms.Label label0sec;
    private System.ComponentModel.IContainer components = null;

    public struct RAWINPUTDEVICE 
    {
      public ushort usUsagePage;
      public ushort usUsage;
      public uint dwFlags;
      public IntPtr hwndTarget;
    }

    [DllImport("User32.dll", EntryPoint="RegisterRawInputDevices", SetLastError=true)]
    public extern static bool RegisterRawInputDevices(
      [In] RAWINPUTDEVICE[] pRawInputDevices,
      [In] uint uiNumDevices,
      [In] uint cbSize);


    public Remote() : this("Remote")
    {
    }

    public Remote(string name) : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    static public bool IsMceRemoteInstalled(IntPtr hwnd)
    {
      RAWINPUTDEVICE[] rid1 = new RAWINPUTDEVICE[1];

      rid1[0].usUsagePage = 0xFFBC;
      rid1[0].usUsage = 0x88;
      rid1[0].dwFlags = 0;
      rid1[0].hwndTarget = hwnd;
      bool Success = RegisterRawInputDevices(rid1, (uint)rid1.Length, (uint)Marshal.SizeOf(rid1[0]));
      if (Success) 
      {
        return true;
      }

      rid1[0].usUsagePage = 0x0C;
      rid1[0].usUsage = 0x01;
      rid1[0].dwFlags = 0;
      rid1[0].hwndTarget = hwnd;
      Success = RegisterRawInputDevices(rid1, (uint)rid1.Length, (uint)Marshal.SizeOf(rid1[0]));
      if (Success) 
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      string errNotInstalled       = "The Hauppauge IR components have not been found.\n\nYou should download and install the latest Hauppauge IR drivers.";
      string errOutOfDate          = "The driver components are not up to date.\n\nYou should update your Hauppauge IR drivers to the current version.";
      string errMissingExe         = "IR application not found. You might want to use it to control external applications.\n\nReinstall the Hauppauge IR drivers to fix this problem.";

      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        checkBoxMCE.Checked               = xmlreader.GetValueAsBool("remote", "mce2005", false);
        radioButtonUSA.Checked            = xmlreader.GetValueAsBool("remote", "USAModel", false);
        checkBoxHCW.Checked               = xmlreader.GetValueAsBool("remote", "HCW", false);
        checkBoxAllowExternal.Checked     = xmlreader.GetValueAsBool("remote", "HCWAllowExternal", false);
        checkBoxKeepControl.Checked       = xmlreader.GetValueAsBool("remote", "HCWKeepControl", false);
        checkBoxVerboseLog.Checked        = xmlreader.GetValueAsBool("remote", "HCWVerboseLog", false);
        trackBarDelay.Value               = xmlreader.GetValueAsInt("remote", "HCWDelay", 0);
//        comboBoxPowerButton.SelectedIndex = xmlreader.GetValueAsInt("remote", "HCWPower", 0);
      }

      radioButtonEurope.Checked = !radioButtonUSA.Checked;

      if (checkBoxMCE.Checked)
      {
        radioButtonEurope.Enabled = true;
        radioButtonUSA.Enabled = true;
      }
      else
      {
        radioButtonEurope.Enabled = false;
        radioButtonUSA.Enabled = false;
      }

      if (radioButtonUSA.Checked)
      {
        pictureBoxUSA.Visible = true;
        pictureBoxEU.Visible = false;
      }
      else
      {
        pictureBoxEU.Visible = true;
        pictureBoxUSA.Visible = false;
      }
      
      if (checkBoxAllowExternal.Checked)
        checkBoxKeepControl.Enabled = true;
      else
        checkBoxKeepControl.Enabled = false;
      
      if (checkBoxHCW.Checked)
        groupBoxSettings.Enabled = true;
      else
        groupBoxSettings.Enabled = false;

      string exePath = HCWRemote.GetHCWPath();
      string dllPath = HCWRemote.GetDllPath();
      
      if (File.Exists(exePath + "Ir.exe"))
      {
        FileVersionInfo exeVersionInfo = FileVersionInfo.GetVersionInfo(exePath + "Ir.exe");
        if (exeVersionInfo.FileVersion.CompareTo("2.45.22350") < 0)
          infoDriverStatus.Text = errOutOfDate;
      }
      else
        infoDriverStatus.Text = errMissingExe;

      if (File.Exists(dllPath + "irremote.DLL"))
      {
        FileVersionInfo dllVersionInfo = FileVersionInfo.GetVersionInfo(dllPath + "irremote.DLL");
        if (dllVersionInfo.FileVersion.CompareTo("2.45.22350") < 0)
          infoDriverStatus.Text = errOutOfDate;
      }
      else
      {
        infoDriverStatus.Text = errNotInstalled;
        checkBoxHCW.Enabled = false;
        groupBoxSettings.Enabled = false;
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        xmlwriter.SetValueAsBool("remote", "mce2005", checkBoxMCE.Checked);
        xmlwriter.SetValueAsBool("remote", "USAModel", radioButtonUSA.Checked);
        xmlwriter.SetValueAsBool("remote", "HCW", checkBoxHCW.Checked);
        xmlwriter.SetValueAsBool("remote", "HCWAllowExternal", checkBoxAllowExternal.Checked);
        xmlwriter.SetValueAsBool("remote", "HCWKeepControl", checkBoxKeepControl.Checked);
        xmlwriter.SetValueAsBool("remote", "HCWVerboseLog", checkBoxVerboseLog.Checked);
        xmlwriter.SetValue("remote", "HCWDelay", trackBarDelay.Value);
//        xmlwriter.SetValue("remote", "HCWPower", comboBoxPowerButton.SelectedIndex);
      }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
      if (disposing)
      {
        if (components != null) 
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Remote));
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.pictureBoxEU = new System.Windows.Forms.PictureBox();
      this.pictureBoxUSA = new System.Windows.Forms.PictureBox();
      this.checkBoxMCE = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.tabControlRemotes = new System.Windows.Forms.TabControl();
      this.tabPageMCE = new System.Windows.Forms.TabPage();
      this.radioButtonEurope = new System.Windows.Forms.RadioButton();
      this.radioButtonUSA = new System.Windows.Forms.RadioButton();
      this.tabPageHCW = new System.Windows.Forms.TabPage();
      this.groupBoxInformation = new System.Windows.Forms.GroupBox();
      this.infoDriverStatus = new System.Windows.Forms.Label();
      this.groupBoxSettings = new System.Windows.Forms.GroupBox();
      this.labelPowerButton = new System.Windows.Forms.Label();
      this.comboBoxPowerButton = new System.Windows.Forms.ComboBox();
      this.label2sec = new System.Windows.Forms.Label();
      this.label0sec = new System.Windows.Forms.Label();
      this.labelDelay = new System.Windows.Forms.Label();
      this.trackBarDelay = new System.Windows.Forms.TrackBar();
      this.checkBoxAllowExternal = new System.Windows.Forms.CheckBox();
      this.checkBoxKeepControl = new System.Windows.Forms.CheckBox();
      this.checkBoxVerboseLog = new System.Windows.Forms.CheckBox();
      this.buttonDefault = new System.Windows.Forms.Button();
      this.checkBoxHCW = new System.Windows.Forms.CheckBox();
      this.tabControlRemotes.SuspendLayout();
      this.tabPageMCE.SuspendLayout();
      this.tabPageHCW.SuspendLayout();
      this.groupBoxInformation.SuspendLayout();
      this.groupBoxSettings.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarDelay)).BeginInit();
      this.SuspendLayout();
      // 
      // pictureBoxEU
      // 
      this.pictureBoxEU.BackColor = System.Drawing.Color.Transparent;
      this.pictureBoxEU.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxEU.Image")));
      this.pictureBoxEU.Location = new System.Drawing.Point(216, 16);
      this.pictureBoxEU.Name = "pictureBoxEU";
      this.pictureBoxEU.Size = new System.Drawing.Size(145, 352);
      this.pictureBoxEU.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxEU.TabIndex = 4;
      this.pictureBoxEU.TabStop = false;
      // 
      // pictureBoxUSA
      // 
      this.pictureBoxUSA.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxUSA.Image")));
      this.pictureBoxUSA.Location = new System.Drawing.Point(216, 16);
      this.pictureBoxUSA.Name = "pictureBoxUSA";
      this.pictureBoxUSA.Size = new System.Drawing.Size(136, 352);
      this.pictureBoxUSA.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.pictureBoxUSA.TabIndex = 3;
      this.pictureBoxUSA.TabStop = false;
      // 
      // checkBoxMCE
      // 
      this.checkBoxMCE.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxMCE.Location = new System.Drawing.Point(32, 24);
      this.checkBoxMCE.Name = "checkBoxMCE";
      this.checkBoxMCE.Size = new System.Drawing.Size(152, 24);
      this.checkBoxMCE.TabIndex = 0;
      this.checkBoxMCE.Text = "Use Microsoft MCE remote";
      this.checkBoxMCE.CheckedChanged += new System.EventHandler(this.checkBoxMCE_CheckedChanged);
      // 
      // tabControlRemotes
      // 
      this.tabControlRemotes.Controls.Add(this.tabPageMCE);
      this.tabControlRemotes.Controls.Add(this.tabPageHCW);
      this.tabControlRemotes.Location = new System.Drawing.Point(8, 16);
      this.tabControlRemotes.Name = "tabControlRemotes";
      this.tabControlRemotes.SelectedIndex = 0;
      this.tabControlRemotes.Size = new System.Drawing.Size(472, 408);
      this.tabControlRemotes.TabIndex = 5;
      // 
      // tabPageMCE
      // 
      this.tabPageMCE.Controls.Add(this.radioButtonEurope);
      this.tabPageMCE.Controls.Add(this.radioButtonUSA);
      this.tabPageMCE.Controls.Add(this.checkBoxMCE);
      this.tabPageMCE.Controls.Add(this.pictureBoxEU);
      this.tabPageMCE.Controls.Add(this.pictureBoxUSA);
      this.tabPageMCE.Location = new System.Drawing.Point(4, 22);
      this.tabPageMCE.Name = "tabPageMCE";
      this.tabPageMCE.Size = new System.Drawing.Size(464, 382);
      this.tabPageMCE.TabIndex = 0;
      this.tabPageMCE.Text = "Microsoft MCE Remote";
      // 
      // radioButtonEurope
      // 
      this.radioButtonEurope.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonEurope.Location = new System.Drawing.Point(40, 64);
      this.radioButtonEurope.Name = "radioButtonEurope";
      this.radioButtonEurope.Size = new System.Drawing.Size(104, 16);
      this.radioButtonEurope.TabIndex = 6;
      this.radioButtonEurope.Text = "European version";
      this.radioButtonEurope.CheckedChanged += new System.EventHandler(this.radioButtonEurope_CheckedChanged);
      // 
      // radioButtonUSA
      // 
      this.radioButtonUSA.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonUSA.Location = new System.Drawing.Point(40, 48);
      this.radioButtonUSA.Name = "radioButtonUSA";
      this.radioButtonUSA.Size = new System.Drawing.Size(104, 16);
      this.radioButtonUSA.TabIndex = 5;
      this.radioButtonUSA.Text = "USA version";
      this.radioButtonUSA.CheckedChanged += new System.EventHandler(this.radioButtonUSA_CheckedChanged);
      // 
      // tabPageHCW
      // 
      this.tabPageHCW.Controls.Add(this.groupBoxInformation);
      this.tabPageHCW.Controls.Add(this.groupBoxSettings);
      this.tabPageHCW.Controls.Add(this.checkBoxHCW);
      this.tabPageHCW.Location = new System.Drawing.Point(4, 22);
      this.tabPageHCW.Name = "tabPageHCW";
      this.tabPageHCW.Size = new System.Drawing.Size(464, 382);
      this.tabPageHCW.TabIndex = 1;
      this.tabPageHCW.Text = "Hauppauge Remote";
      // 
      // groupBoxInformation
      // 
      this.groupBoxInformation.Controls.Add(this.infoDriverStatus);
      this.groupBoxInformation.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBoxInformation.Location = new System.Drawing.Point(16, 272);
      this.groupBoxInformation.Name = "groupBoxInformation";
      this.groupBoxInformation.Size = new System.Drawing.Size(432, 88);
      this.groupBoxInformation.TabIndex = 12;
      this.groupBoxInformation.TabStop = false;
      this.groupBoxInformation.Text = "Information";
      // 
      // infoDriverStatus
      // 
      this.infoDriverStatus.ForeColor = System.Drawing.SystemColors.ControlText;
      this.infoDriverStatus.Location = new System.Drawing.Point(12, 16);
      this.infoDriverStatus.Name = "infoDriverStatus";
      this.infoDriverStatus.Size = new System.Drawing.Size(414, 64);
      this.infoDriverStatus.TabIndex = 11;
      this.infoDriverStatus.Text = "No problems found.";
      this.infoDriverStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBoxSettings
      // 
      this.groupBoxSettings.Controls.Add(this.labelPowerButton);
      this.groupBoxSettings.Controls.Add(this.comboBoxPowerButton);
      this.groupBoxSettings.Controls.Add(this.label2sec);
      this.groupBoxSettings.Controls.Add(this.label0sec);
      this.groupBoxSettings.Controls.Add(this.labelDelay);
      this.groupBoxSettings.Controls.Add(this.trackBarDelay);
      this.groupBoxSettings.Controls.Add(this.checkBoxAllowExternal);
      this.groupBoxSettings.Controls.Add(this.checkBoxKeepControl);
      this.groupBoxSettings.Controls.Add(this.checkBoxVerboseLog);
      this.groupBoxSettings.Controls.Add(this.buttonDefault);
      this.groupBoxSettings.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBoxSettings.Location = new System.Drawing.Point(16, 64);
      this.groupBoxSettings.Name = "groupBoxSettings";
      this.groupBoxSettings.Size = new System.Drawing.Size(432, 184);
      this.groupBoxSettings.TabIndex = 7;
      this.groupBoxSettings.TabStop = false;
      this.groupBoxSettings.Text = "Settings";
      // 
      // labelPowerButton
      // 
      this.labelPowerButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.labelPowerButton.Location = new System.Drawing.Point(12, 108);
      this.labelPowerButton.Name = "labelPowerButton";
      this.labelPowerButton.Size = new System.Drawing.Size(108, 23);
      this.labelPowerButton.TabIndex = 14;
      this.labelPowerButton.Text = "Power button action:";
      // 
      // comboBoxPowerButton
      // 
      this.comboBoxPowerButton.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxPowerButton.Enabled = false;
      this.comboBoxPowerButton.Items.AddRange(new object[] {
                                                             "do nothing",
                                                             "Exit Media Portal",
                                                             "Shutdown Windows",
                                                             "Standby",
                                                             "Hibernate"});
      this.comboBoxPowerButton.Location = new System.Drawing.Point(120, 104);
      this.comboBoxPowerButton.Name = "comboBoxPowerButton";
      this.comboBoxPowerButton.Size = new System.Drawing.Size(136, 21);
      this.comboBoxPowerButton.TabIndex = 4;
      // 
      // label2sec
      // 
      this.label2sec.BackColor = System.Drawing.SystemColors.Control;
      this.label2sec.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label2sec.Location = new System.Drawing.Point(224, 160);
      this.label2sec.Name = "label2sec";
      this.label2sec.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.label2sec.Size = new System.Drawing.Size(40, 16);
      this.label2sec.TabIndex = 12;
      this.label2sec.Text = "2 sec.";
      // 
      // label0sec
      // 
      this.label0sec.BackColor = System.Drawing.SystemColors.Control;
      this.label0sec.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label0sec.Location = new System.Drawing.Point(112, 160);
      this.label0sec.Name = "label0sec";
      this.label0sec.Size = new System.Drawing.Size(40, 16);
      this.label0sec.TabIndex = 11;
      this.label0sec.Text = "0 sec.";
      // 
      // labelDelay
      // 
      this.labelDelay.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.labelDelay.Location = new System.Drawing.Point(12, 144);
      this.labelDelay.Name = "labelDelay";
      this.labelDelay.Size = new System.Drawing.Size(96, 23);
      this.labelDelay.TabIndex = 10;
      this.labelDelay.Text = "Repeat-delay:";
      // 
      // trackBarDelay
      // 
      this.trackBarDelay.LargeChange = 100;
      this.trackBarDelay.Location = new System.Drawing.Point(112, 136);
      this.trackBarDelay.Maximum = 2000;
      this.trackBarDelay.Name = "trackBarDelay";
      this.trackBarDelay.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.trackBarDelay.Size = new System.Drawing.Size(152, 45);
      this.trackBarDelay.SmallChange = 100;
      this.trackBarDelay.TabIndex = 3;
      this.trackBarDelay.TickFrequency = 1000;
      this.trackBarDelay.TickStyle = System.Windows.Forms.TickStyle.None;
      // 
      // checkBoxAllowExternal
      // 
      this.checkBoxAllowExternal.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxAllowExternal.Location = new System.Drawing.Point(16, 24);
      this.checkBoxAllowExternal.Name = "checkBoxAllowExternal";
      this.checkBoxAllowExternal.Size = new System.Drawing.Size(240, 24);
      this.checkBoxAllowExternal.TabIndex = 0;
      this.checkBoxAllowExternal.Text = "External processes may use the remote control";
      this.checkBoxAllowExternal.CheckedChanged += new System.EventHandler(this.checkBoxAllowExternal_CheckedChanged);
      // 
      // checkBoxKeepControl
      // 
      this.checkBoxKeepControl.Enabled = false;
      this.checkBoxKeepControl.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxKeepControl.Location = new System.Drawing.Point(32, 48);
      this.checkBoxKeepControl.Name = "checkBoxKeepControl";
      this.checkBoxKeepControl.Size = new System.Drawing.Size(192, 24);
      this.checkBoxKeepControl.TabIndex = 1;
      this.checkBoxKeepControl.Text = "Keep control when MP looses focus";
      // 
      // checkBoxVerboseLog
      // 
      this.checkBoxVerboseLog.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxVerboseLog.Location = new System.Drawing.Point(16, 72);
      this.checkBoxVerboseLog.Name = "checkBoxVerboseLog";
      this.checkBoxVerboseLog.Size = new System.Drawing.Size(108, 24);
      this.checkBoxVerboseLog.TabIndex = 2;
      this.checkBoxVerboseLog.Text = "Extended Logging";
      // 
      // buttonDefault
      // 
      this.buttonDefault.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonDefault.Location = new System.Drawing.Point(308, 148);
      this.buttonDefault.Name = "buttonDefault";
      this.buttonDefault.Size = new System.Drawing.Size(112, 24);
      this.buttonDefault.TabIndex = 9;
      this.buttonDefault.Text = "Reset to &default";
      this.buttonDefault.Click += new System.EventHandler(this.buttonDefault_Click);
      // 
      // checkBoxHCW
      // 
      this.checkBoxHCW.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxHCW.Location = new System.Drawing.Point(32, 24);
      this.checkBoxHCW.Name = "checkBoxHCW";
      this.checkBoxHCW.Size = new System.Drawing.Size(144, 24);
      this.checkBoxHCW.TabIndex = 10;
      this.checkBoxHCW.Text = "Use Hauppauge remote";
      this.checkBoxHCW.CheckedChanged += new System.EventHandler(this.checkBoxHCW_CheckedChanged);
      // 
      // Remote
      // 
      this.Controls.Add(this.tabControlRemotes);
      this.Name = "Remote";
      this.Size = new System.Drawing.Size(488, 432);
      this.tabControlRemotes.ResumeLayout(false);
      this.tabPageMCE.ResumeLayout(false);
      this.tabPageHCW.ResumeLayout(false);
      this.groupBoxInformation.ResumeLayout(false);
      this.groupBoxSettings.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.trackBarDelay)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    #region Form control commands
    //
    // USA version
    //
    private void radioButtonUSA_CheckedChanged(object sender, System.EventArgs e)
    {
      pictureBoxUSA.Visible = radioButtonUSA.Checked;
      pictureBoxEU.Visible = !radioButtonUSA.Checked;
      radioButtonEurope.Checked = !radioButtonUSA.Checked;
    }

    //
    // European version
    //
    private void radioButtonEurope_CheckedChanged(object sender, System.EventArgs e)
    {
      pictureBoxUSA.Visible = !radioButtonEurope.Checked;
      pictureBoxEU.Visible = radioButtonEurope.Checked;
      radioButtonUSA.Checked = !radioButtonEurope.Checked;
    }

    //
    // Use Microsoft MCE remote
    //
    private void checkBoxMCE_CheckedChanged(object sender, System.EventArgs e)
    {
      radioButtonEurope.Enabled = checkBoxMCE.Checked;
      radioButtonUSA.Enabled = checkBoxMCE.Checked;
    }

    //
    // External processes may use the remote control
    //
    private void checkBoxAllowExternal_CheckedChanged(object sender, System.EventArgs e)
    {
      checkBoxKeepControl.Enabled = checkBoxAllowExternal.Checked;
    }

    //
    // Use Hauppauge remote
    //
    private void checkBoxHCW_CheckedChanged(object sender, System.EventArgs e)
    {
      groupBoxSettings.Enabled = checkBoxHCW.Checked;
    }

    //
    // Reset to default
    //    
    private void buttonDefault_Click(object sender, System.EventArgs e)
    {
      checkBoxAllowExternal.Checked = false;
      checkBoxKeepControl.Checked = false;
      checkBoxVerboseLog.Checked = false;
      trackBarDelay.Value = 0;
//      comboBoxPowerButton.SelectedIndex = 1;    
    }
    #endregion
  }
}
