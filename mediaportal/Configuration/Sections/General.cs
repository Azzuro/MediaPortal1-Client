#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#endregion

using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using System.Runtime.InteropServices;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class General : MediaPortal.Configuration.SectionSettings
  {
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern int SendMessageTimeout(
      IntPtr hwnd,
      IntPtr msg,
      IntPtr wParam,
      IntPtr lParam,
      IntPtr fuFlags,
      IntPtr uTimeout,
      out IntPtr lpdwResult);

    const int WM_SETTINGCHANGE = 0x1A;
    const int SMTO_ABORTIFHUNG = 0x2;
    const int HWND_BROADCAST = 0xFFFF;

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBoxGeneralSettings;
    private System.Windows.Forms.CheckedListBox settingsCheckedListBox;
    private MediaPortal.UserInterface.Controls.MPComboBox cbDebug;
    private Label lbDebug;
    private System.ComponentModel.IContainer components = null;

    public General()
      : this("General")
    {
    }

    public General(string name)
      : base(name)
    {

      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }


    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
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

    string loglevel = "3";  // Degub is default

    string[][] sectionEntries = new string[][] { 
      new string[] { "general", "startfullscreen", "false" },
      new string[] { "general", "minimizeonstartup", "false" },
      new string[] { "general", "minimizeonexit", "false" },
      new string[] { "general", "autohidemouse", "true" },
	    new string[] { "general", "mousesupport", "true" }, 
      new string[] { "general", "hideextensions", "true" },
      new string[] { "general", "animations", "true" },
	    new string[] { "general", "autostart", "false" },
	    new string[] { "general", "baloontips", "false" },
	    new string[] { "general", "dblclickasrightclick", "false" },
	    new string[] { "general", "hidetaskbar", "false" },
	    new string[] { "general", "alwaysontop", "false" },
	    new string[] { "general", "exclusivemode", "true" },
	    new string[] { "general", "enableguisounds", "true" },
	    new string[] { "general", "screensaver", "false" },
      new string[] { "general", "turnoffmonitor", "false" },
	    new string[] { "general", "startbasichome", "false" },
      new string[] { "general", "turnmonitoronafterresume", "false" },
      new string[] { "general", "enables3trick","true" },
      new string[] { "general", "autosize", "false" },
      new string[] { "general", "useuithread", "false" }
      //new string[] { "general", "allowfocus", "false" }
      };

    // PLEASE NOTE: when adding items, adjust the box so it doesn't get scrollbars
    //              AND be aware that "allowfocus" has to be last item in the list
    //              AND be careful cause depending on where you add a setting, the indexes might change
    //              (e.g. SaveSettings depends on the index!!!)


    /// <summary>
    /// 
    /// </summary>
    public override void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        //
        // Load general settings
        //
        for (int index = 0; index < sectionEntries.Length; index++)
        {
          string[] currentSection = sectionEntries[index];
          settingsCheckedListBox.SetItemChecked(index, xmlreader.GetValueAsBool(currentSection[0], currentSection[1], bool.Parse(currentSection[2])));
        }

        loglevel = xmlreader.GetValueAsString("general", "loglevel", "3");
        cbDebug.SelectedIndex = Convert.ToInt16(loglevel);

        //numericUpDown1.Value=xmlreader.GetValueAsInt("vmr9OSDSkin","alphaValue",10);

        //// Allow Focus
        //using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false))
        //  settingsCheckedListBox.SetItemChecked(settingsCheckedListBox.Items.Count - 1, ((int)subkey.GetValue("ForegroundLockTimeout", 2000000) == 0));
      }
    }

    public override void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        // Save Debug Level
        xmlwriter.SetValue("general", "loglevel", cbDebug.SelectedIndex);

        //
        // Load general settings
        //
        for (int index = 0; index < sectionEntries.Length; index++)  // Leave out last setting (focus)!
        {
          string[] currentSection = sectionEntries[index];
          xmlwriter.SetValueAsBool(currentSection[0], currentSection[1], settingsCheckedListBox.GetItemChecked(index));
        }
        //xmlwriter.SetValue("vmr9OSDSkin","alphaValue",numericUpDown1.Value);
      }

      try
      {
        if (settingsCheckedListBox.GetItemChecked(7))
        {
          string fileName = String.Format("\"{0}\"", System.IO.Path.GetFullPath("mediaportal.exe"));
          using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            subkey.SetValue("MediaPortal", fileName);
        }
        else
          using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            subkey.DeleteValue("MediaPortal", false);

        Int32 iValue = 1;
        if (settingsCheckedListBox.GetItemChecked(8))
          iValue = 0;

        using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", true))
          subkey.SetValue("EnableBalloonTips", iValue);

        if (settingsCheckedListBox.GetItemChecked(settingsCheckedListBox.Items.Count - 1))
          using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            subkey.SetValue("ForegroundLockTimeout", 0);

        //// Allow Focus
        //using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
        //{
        //  bool focusChecked = ((int)subkey.GetValue("ForegroundLockTimeout", 200000) == 0);

        //  if (focusChecked != settingsCheckedListBox.GetItemChecked(18))
        //    if (settingsCheckedListBox.GetItemChecked(settingsCheckedListBox.Items.Count - 1))
        //      subkey.SetValue("ForegroundLockTimeout", 0);
        //    else
        //      subkey.SetValue("ForegroundLockTimeout", 200000);
        //}

        IntPtr result = IntPtr.Zero;
        SendMessageTimeout((IntPtr)HWND_BROADCAST, (IntPtr)WM_SETTINGCHANGE, IntPtr.Zero, Marshal.StringToBSTR(String.Empty), (IntPtr)SMTO_ABORTIFHUNG, (IntPtr)3, out result);
      }
      catch (Exception ex)
      {
        Log.Info("Exception: {0}", ex.Message);
        Log.Info("Exception: {0}", ex);
        Log.Info("Exception: {0}", ex.StackTrace);
      }
    }

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBoxGeneralSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lbDebug = new System.Windows.Forms.Label();
      this.cbDebug = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.settingsCheckedListBox = new System.Windows.Forms.CheckedListBox();
      this.groupBoxGeneralSettings.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBoxGeneralSettings
      // 
      this.groupBoxGeneralSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxGeneralSettings.Controls.Add(this.lbDebug);
      this.groupBoxGeneralSettings.Controls.Add(this.cbDebug);
      this.groupBoxGeneralSettings.Controls.Add(this.settingsCheckedListBox);
      this.groupBoxGeneralSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxGeneralSettings.Location = new System.Drawing.Point(0, 3);
      this.groupBoxGeneralSettings.Name = "groupBoxGeneralSettings";
      this.groupBoxGeneralSettings.Size = new System.Drawing.Size(472, 397);
      this.groupBoxGeneralSettings.TabIndex = 1;
      this.groupBoxGeneralSettings.TabStop = false;
      this.groupBoxGeneralSettings.Text = "General Settings";
      // 
      // lbDebug
      // 
      this.lbDebug.AutoSize = true;
      this.lbDebug.Location = new System.Drawing.Point(21, 364);
      this.lbDebug.Name = "lbDebug";
      this.lbDebug.Size = new System.Drawing.Size(57, 13);
      this.lbDebug.TabIndex = 3;
      this.lbDebug.Text = "Log Level:";
      // 
      // cbDebug
      // 
      this.cbDebug.BorderColor = System.Drawing.Color.Empty;
      this.cbDebug.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDebug.FormattingEnabled = true;
      this.cbDebug.Items.AddRange(new object[] {
            "Error",
            "Warning",
            "Information",
            "Debug"});
      this.cbDebug.Location = new System.Drawing.Point(85, 360);
      this.cbDebug.Name = "cbDebug";
      this.cbDebug.Size = new System.Drawing.Size(121, 21);
      this.cbDebug.TabIndex = 2;
      // 
      // settingsCheckedListBox
      // 
      this.settingsCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.settingsCheckedListBox.CheckOnClick = true;
      this.settingsCheckedListBox.Items.AddRange(new object[] {
            "Start MediaPortal in fullscreen mode",
            "Minimize to tray on start up",
            "Minimize to tray on GUI exit",
            "Autohide mouse cursor in fullscreen mode when idle",
            "Show special mouse controls (scrollbars, etc)",
            "Dont show file extensions like .mp3, .avi, .mpg,...",
            "Enable animations",
            "Autostart MediaPortal when windows starts",
            "Disable Windows XP balloon tips",
            "Use mouse left double click as right click",
            "Hide taskbar in fullscreen mode",
            "MediaPortal always on top",
            "Use Exclusive DirectX mode - avoids tearing",
            "Enable GUI sound effects",
            "Blank screen in fullscreen mode when MediaPortal is idle",
            "Turn off monitor when blanking screen",
            "Start with basic home screen",
            "Turn monitor/tv on when resuming from standby",
            "Allow S3 standby although wake up devices are present",
            "Autosize window mode to skin",
            "Enable seperate thread to render GUI"});
      this.settingsCheckedListBox.Location = new System.Drawing.Point(16, 24);
      this.settingsCheckedListBox.Name = "settingsCheckedListBox";
      this.settingsCheckedListBox.Size = new System.Drawing.Size(440, 319);
      this.settingsCheckedListBox.TabIndex = 0;
      // 
      // General
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.groupBoxGeneralSettings);
      this.Name = "General";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBoxGeneralSettings.ResumeLayout(false);
      this.groupBoxGeneralSettings.PerformLayout();
      this.ResumeLayout(false);

    }
    #endregion
  }
}

