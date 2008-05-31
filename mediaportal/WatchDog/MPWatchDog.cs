#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using DaggerLib.DSGraphEdit;
using MediaPortal.Configuration;
using System.Threading;

namespace MPTestTool
{
  public partial class MPWatchDog : MPForm
  {
    #region Variables
    string _tempDir = "";
    string _zipFile = "";
    bool _autoMode = false;
    bool _watchdog = false;
    bool _restartMP=false;
    int _cancelDelay=10;
    Process _processMP = null;
    int _lastMPLogLevel = 2;
    int _graphsCreated = 0;
    List<string> _knownPids = new List<string>();
    #endregion

    #region Helper functions
    private void ShowUsage()
    {
      string usageText = "\n" +
        "Usage: MPWatchDog.exe [-auto] [-watchdog] [-zipFile <path+filename>] [-restartMP <delay in seconds>] \n" +
        "\n" +
        "auto     : Perform all actions automatically and start MediaPortal in between\n" +
        "watchdog : Used internally by MediaPortal to monitor MP\n" +
        "zipFile  : full path and filename to the zip where all logfiles will be included\n" +
        "restartMP: automatically collects all logs, saves them as zip to desktop, restarts MP and closes\n" +
        "           the delay is the time in where you can cancel the operation\n" +
        "\n";
      MessageBox.Show(usageText,"MediaPortal test tool usage",MessageBoxButtons.OK,MessageBoxIcon.Information);
    }
    private void setStatus(string status)
    {
      this.statusBar.Text = string.Format("Status: {0}", status);
    }
    #endregion

    public MPWatchDog()
    {
      Thread.CurrentThread.Name = "MPWatchDog";
      InitializeComponent();
      _tempDir=Path.GetTempPath();
      if (!_tempDir.EndsWith("\\"))
        _tempDir += "\\";
      _tempDir += "MPTemp";
      _zipFile=Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)+"\\MediaPortalLogs_"+DateTime.Now.ToString("dd_MM_yy")+".zip";
      tbZipFile.Text = _zipFile;
      if (!ParseCommandLine())
        Application.Exit();
      if (_autoMode)
      {
        if (!CheckRequirements())
          Application.Exit();
        preTestButton.Enabled = false;
        LaunchMPButton.Enabled = false;
        postTestButton.Enabled = false;
        setStatus("Running in auto/debug mode...");
        tmrUnAttended.Enabled = true;
      }
      if (_watchdog)
      {
        WindowState = FormWindowState.Minimized;
        ShowInTaskbar = false;
        tmrWatchdog.Enabled=true;
      }
    }

    #region Checks
    private bool ParseCommandLine()
    {
      string[] args = Environment.GetCommandLineArgs();
      for (int i = 1; i < args.Length; )
      {
        switch (args[i].ToLower())
        {
          case "-zipfile":
            _zipFile = args[++i];
            break;
          case "-auto":
            _autoMode = true;
            break;
          case "-watchdog":
            _watchdog = true;
            break;
          case "-restartmp":
            _restartMP = true;
            if (!Int32.TryParse(args[++i],out _cancelDelay))
            {
              ShowUsage();
              return false;
            }
            break;
          default:
            ShowUsage();
            return false;
        }
        i++;
      }
      return true;
    }
    public bool CheckRequirements()
    {
      Directory.CreateDirectory(_tempDir);
      if (!Directory.Exists(Path.GetDirectoryName(_zipFile)))
      {
        try
        {
          Directory.CreateDirectory(Path.GetDirectoryName(_zipFile));
        }
        catch (Exception)
        {
          Utils.ErrorDlg("You supplied an invalid path for the zip file.");
          return false;
        }
      }
      return true;
    }
    #endregion

    #region Form Events
    private void btnZipFile_Click(object sender, EventArgs e)
    {
      SaveFileDialog saveDialog = new SaveFileDialog();
      //Default settings
      saveDialog.AddExtension = true;
      saveDialog.OverwritePrompt = true;
      saveDialog.DefaultExt = ".zip";
      saveDialog.Title = "Choose ZIP file to create";

      saveDialog.FileName = tbZipFile.Text;
      DialogResult dr = saveDialog.ShowDialog();
      if (dr == DialogResult.OK)
      {
        tbZipFile.Text = saveDialog.FileName;
        _zipFile = tbZipFile.Text;
      }
    }
    private void menuItem2_Click(object sender, EventArgs e)
    {
      Close();
    }
    private void preTestButton_Click(object sender, EventArgs e)
    {
      PerformPreTestActions();
    }
    private void LaunchMPButton_Click(object sender, EventArgs e)
    {
      LaunchMediaPortalAction();
    }
    private void postTestButton_Click(object sender, EventArgs e)
    {
      PerformPostTestActions();
    }
    private void menuItem7_Click(object sender, EventArgs e)
    {
      AboutForm dlg = new AboutForm();
      dlg.ShowDialog();
    }
    #endregion

    #region Perform actions
    void PerformPreTestActions()
    {
      setStatus("Busy performing pre-test actions...");
      PreTestActions pta = new PreTestActions();
      pta.Show();

      // give windows 1 sec to render the form
      Utils.SleepNonBlocking(1000);

      if (pta.PerformActions())
      {
        setStatus("Done performing pre-test actions.");
      }
      else
      {
        setStatus("Pre-test actions were aborted.");
      }
      if (_autoMode)
      {
        pta.Close();
        pta = null;
      }
    }
    void LaunchMediaPortalAction()
    {
      _knownPids.Clear();
      if (!Directory.Exists(_tempDir))
        Directory.CreateDirectory(_tempDir);
      setStatus("Launching MediaPortal...");
      // Set the loglevel to "debug"
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Application.StartupPath+"\\MediaPortal.xml",false))
      {
        _lastMPLogLevel = xmlreader.GetValueAsInt("general", "loglevel", 1);
        xmlreader.SetValue("general", "loglevel", 3);
      }
      _processMP = new Process();
      _processMP.StartInfo.WorkingDirectory = Application.StartupPath;
      _processMP.StartInfo.FileName = "mediaportal.exe";
      _processMP.Start();
      setStatus("MediaPortal started. Waiting for exit...");
      Update();
      tmrMPWatcher.Enabled = true;
    }
    void PerformPostTestActions()
    {
      setStatus("Busy performing post-test actions...");
      PostTestActions pta = new PostTestActions(_tempDir,_zipFile);
      pta.Show();

      // give windows 1 sec to render the form
      Utils.SleepNonBlocking(1000);
      
      if (pta.PerformActions())
      {
        setStatus("Done performing post-test actions.");
      }
      else
      {
        setStatus("Post-test actions were aborted.");
      }
      if (_autoMode)
      {
        pta.Close();
        pta = null;
      }
    }
    #endregion

    #region Timer callbacks
    private void tmrUnAttended_Tick(object sender, EventArgs e)
    {
      tmrUnAttended.Enabled = false;
      PerformPreTestActions();
      LaunchMediaPortalAction();
    }
    private void tmrMPWatcher_Tick(object sender, EventArgs e)
    {
      tmrMPWatcher.Enabled = false;
      if (_processMP.HasExited)
      {
        // Reset the loglevel to "debug"
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Application.StartupPath + "\\MediaPortal.xml", false))
        {
          xmlreader.SetValue("general", "loglevel", _lastMPLogLevel);
        }
        setStatus("idle");
        PerformPostTestActions();
        preTestButton.Enabled = true;
        LaunchMPButton.Enabled = true;
        postTestButton.Enabled = true;
        return;
      }
      List<DSGrapheditROTEntry> rotEntries = DaggerDSUtils.GetFilterGraphsFromROT();
      foreach (DSGrapheditROTEntry rot in rotEntries)
      {
        if (!_knownPids.Contains(rot.ToString()))
        {
          _knownPids.Add(rot.ToString());
          MakeGraphSnapshot(rot);
        }
      }
      tmrMPWatcher.Enabled = true;
    }
    #endregion

    private void MakeGraphSnapshot(DSGrapheditROTEntry rotEntry)
    {
      _graphsCreated++;
      DSGraphEditPanel panel=null;
      try
      {
        panel = new DSGraphEditPanel(rotEntry.ConnectToROTEntry());
      }
      catch (Exception)
      {
        return;
      }
      if (panel == null)
        return;
      panel.Width = 3000;
      panel.ShowPinNames = true;
      panel.ShowTimeSlider = false;
      panel.dsDaggerUIGraph1.AutoArrangeWidthOffset = 150;
      panel.dsDaggerUIGraph1.ArrangeNodes(DaggerLib.UI.AutoArrangeStyle.All);
      Bitmap b = new Bitmap(panel.Width, panel.Height); 
      panel.DrawToBitmap(b, panel.Bounds);
      string imgFile = _tempDir + "\\graph_" + rotEntry.ToString() + ".jpg";
      try
      {
        b.Save(imgFile, System.Drawing.Imaging.ImageFormat.Jpeg);
      }
      catch (Exception ex)
      {
        Utils.ErrorDlg("Exception raised while trying to save graph snapshot. file=[" + imgFile + "] message=[" + ex.Message + "]");
      }
      b.Dispose(); 
      panel.Dispose();
    }

    private void tmrWatchdog_Tick(object sender, EventArgs e)
    {
      tmrWatchdog.Enabled = false;
      Process[] procs=Process.GetProcesses();
      bool running=false;
      foreach (Process p in procs)
      {
        if (p.ProcessName == "MediaPortal")
        {
          running = true;
          break;
        }
      }
      if (running)
        tmrWatchdog.Enabled = true;
      else
      {
        if (!File.Exists(Config.GetFile(Config.Dir.Config, "mediaportal.running")))
        {
          Close();
          return;
        }
        this.ShowInTaskbar = true;
        this.WindowState = FormWindowState.Normal;
        CheckRequirements();
        preTestButton.Enabled = false;
        LaunchMPButton.Enabled = false;
        if (!_restartMP)
          Utils.ErrorDlg("MediaPortal crashed unexpectedly.");
        else
        {
          CrashRestartDlg dlg = new CrashRestartDlg(_cancelDelay);
          if (dlg.ShowDialog() == DialogResult.OK)
          {
            PerformPostTestActions();
            string mpExe = Config.GetFolder(Config.Dir.Base) + "\\MediaPortal.exe";
            Process mp = new Process();
            mp.StartInfo.FileName = mpExe;
            mp.Start();
            Close();
          }
        }
      }
    }
  }
}