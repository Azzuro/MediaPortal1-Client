#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Hardware;
using MediaPortal.Hooks;
using MediaPortal.ServiceImplementations;
using MediaPortal.Util;
using Microsoft.Win32;

namespace MPTray
{
  public class ShellApplication
  {
    private NotifyIcon _SystemNotificationAreaIcon = null;

    #region Methods

    private void InstallKeyboardHook()
    {
      _keyboardHook = new KeyboardHook();
      _keyboardHook.KeyDown += new KeyEventHandler(OnKeyDown);
      _keyboardHook.KeyUp += new KeyEventHandler(OnKeyUp);
      _keyboardHook.IsEnabled = true;
    }

    private void SwitchFocus()
    {
      Process[] processes = Process.GetProcessesByName("mediaportal");

      if (processes.Length > 0)
      {
        Win32API.ShowWindow((uint)processes[0].MainWindowHandle, (int)Win32API.ShowWindowFlags.ShowNormal);
        if (Win32API.SetForegroundWindow(processes[0].MainWindowHandle, true))
        {
          Log.Info("MPTray: Successfully switched focus.");
        }
      }
      else
      {
        if (processes.Length <= 0)
        {
          Log.Debug("MPTray: MediaPortal is not running (yet).");
        }
      }

      // MPTrayMOD version
      //int attempts = 0;
      //while (attempts < 60) // considerably high since MP might be caching textures
      //{
      //  processes = Process.GetProcessesByName("mediaportal");
      //  if (processes.Length > 0 && NativeMethods.SetForegroundWindow(processes[0].MainWindowHandle, true))
      //  {
      //    Log.Info("MPTray: Successfully switched focus.");
      //    break;
      //  }
      //  else
      //  {
      //    if (processes.Length <= 0)
      //    {
      //      Log.Debug("MPTray: MediaPortal is not running (yet).");
      //    }
      //  }
      //  Thread.Sleep(1000);
      //  attempts++;
      //}
    }

    private void OnClick(object sender, RemoteEventArgs e)
    {
      if (e.Button != RemoteButton.Start)
        return;

      Process[] processes = Process.GetProcessesByName("mediaportal");

      if (processes.Length != 0)
      {
        if (processes.Length > 1)
        {
          try
          {
            Log.Warn("MPTray: More than one window named \"MediaPortal\" has been found!");
            foreach (Process procName in processes)
            {
              Log.Info("MPTray:   {0} (Started: {1}, ID: {2})", procName.ProcessName, procName.StartTime.ToShortTimeString(), procName.Id);
            }
          }
          catch (Exception) { }
        }
        Log.Info("MPTray: MediaPortal is already running - switching focus.");
        SwitchFocus();
      }
      else
      {
        try
        {
          Uri uri = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);

          Process process = new Process();

          process.StartInfo.FileName = "mediaportal.exe";
          process.StartInfo.WorkingDirectory = Path.GetDirectoryName(uri.LocalPath);
          process.StartInfo.UseShellExecute = true;
          process.Start();

          using (
            EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset, "MediaPortalHandleCreated"))
          {
            if (handle.SafeWaitHandle.IsInvalid)
            {
              return;
            }

            handle.Set();
            handle.Close();

            SwitchFocus();
          }
        }
        catch (Exception ex)
        {
          Log.Error("MPTray: Error starting MediaPortal {0}", ex.Message);
        }
      }
    }

    private void OnDeviceArrival(object sender, EventArgs e)
    {
      Log.Debug("MPTray: Device installed");
    }

    private void OnDeviceRemoval(object sender, EventArgs e)
    {
      Log.Debug("MPTray: Device removed");
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
        _windowsKeyPressed = true;

      if (_windowsKeyPressed && e.KeyCode == Keys.E)
      {
        Process[] processes = Process.GetProcessesByName("explorer");

        if (processes.Length == 0)
          Process.Start("explorer.exe");
      }

      if (_windowsKeyPressed && e.KeyCode == Keys.S)
      {
        OnClick(null, new RemoteEventArgs(RemoteButton.Start));
        e.Handled = true;
      }

      if (_windowsKeyPressed && e.KeyCode == Keys.K)
      {
        TerminateProcess("mediaportal");
        e.Handled = true;
      }

      if (_windowsKeyPressed && (e.KeyCode == Keys.T || e.KeyCode == Keys.K || e.KeyCode != Keys.M))
      {
        string className = "Shell_TrayWnd";
        string n= null;
        IntPtr handle = (IntPtr)Win32API.FindWindow(ref className, ref n);

        if (handle != IntPtr.Zero && Win32API.IsWindowVisible(handle) == false)
          Win32API.ShowWindow((uint)handle, (int)Win32API.ShowWindowFlags.ShowNA);

        if (handle != IntPtr.Zero && Win32API.IsWindowEnabled(handle) == false)
          Win32API.EnableWindow((uint)handle, 1);

        e.Handled = e.KeyCode != Keys.M;
      }

      if (_windowsKeyPressed && e.KeyCode == Keys.C)
      {
        Uri uri = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);

        Process.Start(Path.GetDirectoryName(uri.LocalPath) + @"\configuration.exe");

        e.Handled = true;
      }

      if (e.KeyCode == Keys.Escape && (e.Modifiers & Keys.Control | Keys.Shift) == (Keys.Control | Keys.Shift))
        Process.Start("taskmgr.exe");

      if (e.KeyCode == Keys.Delete && (e.Modifiers & Keys.Control | Keys.Alt) == (Keys.Control | Keys.Alt))
        Process.Start("taskmgr.exe");
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
        _windowsKeyPressed = false;
    }

    private void Register(bool register, RegistryKey hive)
    {
      try
      {
        if (register)
        {
          RegistryKey key = hive.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
          key.SetValue("MediaPortal Shell", System.Windows.Forms.Application.ExecutablePath);
        }
        else
        {
          RegistryKey key = hive.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
          key.DeleteValue("MediaPortal Shell");
        }
      }
      catch (Exception e)
      {
        Log.Error("MPTray: Failed to modify autostart entry {0}", e.ToString());
      }
    }

    private void Run()
    {
      try
      {        
        try
        {
          Thread.CurrentThread.Name = "MPTray";
        }
        catch (InvalidOperationException) { }

        Log.Debug("MPTray: Starting...");

        if (TerminateProcess("ehtray"))
        {
          Log.Info("MPTray: Terminating running instance(s) of ehtray.exe");
        }

        Remote.Click += new RemoteEventHandler(this.OnClick);
        Remote.DeviceArrival += new DeviceEventHandler(this.OnDeviceArrival);
        Remote.DeviceRemoval += new DeviceEventHandler(this.OnDeviceRemoval);

        // reduce the memory footprint of the app
        Process process = Process.GetCurrentProcess();

        process.MaxWorkingSet = (IntPtr)900000;
        process.MinWorkingSet = (IntPtr)300000;

        InitTrayIcon();
        Application.Run();
      }
      catch (Exception e)
      {
        Log.Error("MPTray: Error on startup {0}", e.ToString());
      }

      if (_keyboardHook != null)
      {
        Log.Info("MPTray: Disabling keyboard hook");
        _keyboardHook.IsEnabled = false;
      }

      Log.Debug("MPTray: Exiting");
    }

    private bool TerminateProcess(string processName)
    {
      bool terminatedProcess = false;

      try
      {
        foreach (Process process in Process.GetProcessesByName(processName))
        {
          if (process != Process.GetCurrentProcess())
          {
            process.Kill();
            process.Close();

            if (terminatedProcess == false)
              terminatedProcess = true;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("MPTray: Error while terminating process(es): {0}, {1}", processName, ex.ToString());
      }

      return terminatedProcess;
    }

    #endregion Methods

    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      ShellApplication handler = new ShellApplication();

      foreach (string arg in args)
      {
        switch (arg.ToLower())
        {
          case "/shell":
          case "-shell":
            handler.InstallKeyboardHook();
            break;
          case "/noicon":
          case "-noicon":
            break;
          case "/kill":
          case "-kill":
            handler.TerminateProcess("mptray");
            return;
          case "/register":
          case "-register":
          case "/register:user":
          case "-register:user":
            handler.Register(true, Registry.CurrentUser);
            return;
          case "/register:all":
          case "-register:all":
            handler.Register(true, Registry.LocalMachine);
            return;
          case "/unregister":
          case "-unregister":
          case "/unregister:user":
          case "-unregister:user":
            handler.Register(false, Registry.CurrentUser);
            return;
          case "/unregister:all":
          case "-unregister:all":
            handler.Register(false, Registry.LocalMachine);
            return;
          case "/unregister:both":
          case "-unregister:both":
            handler.Register(false, Registry.CurrentUser);
            handler.Register(false, Registry.LocalMachine);
            return;
          default:
            Log.Info("MPTray: Ignoring unknown command line parameter: '{0}'", arg);
            break;
        }
      }

      Process[] processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

      if (processes.Length != 1)
      {
        Log.Warn("MPTray: Another instance of MPTray is already running");
        return;
      }

      handler.Run();
    }

    private void InitTrayIcon()
    {
      if (_SystemNotificationAreaIcon == null)
      {
        try
        {
          ContextMenu contextMenuTray = new ContextMenu();
          MenuItem menuItem1 = new MenuItem();
          //MenuItem menuItem2 = new MenuItem();

          // Initialize contextMenuTray
          contextMenuTray.MenuItems.AddRange(new MenuItem[] { menuItem1 /*, menuItem2 */});

          // Initialize menuItem1
          menuItem1.Index = 0;
          menuItem1.Text = "Close";
          menuItem1.Click += new EventHandler(menuItem1_Click);

          _SystemNotificationAreaIcon = new NotifyIcon();
          _SystemNotificationAreaIcon.ContextMenu = contextMenuTray;

          //Stream s = GetType().Assembly.GetManifestResourceStream("MPTrayIcon");
          _SystemNotificationAreaIcon.Icon = MPTray.Properties.Resources.MPTrayIcon;
          _SystemNotificationAreaIcon.Text = "MediaPortal Tray Launcher";
          _SystemNotificationAreaIcon.Visible = true;
        }
        catch (Exception ex)
        {
          Log.Error("MPTray: Could not init tray icon - {0}", ex.ToString());
        }
      }
    }

    private void menuItem1_Click(object sender, EventArgs e)
    {
      _SystemNotificationAreaIcon.Visible = false;
      _SystemNotificationAreaIcon = null;
      Application.Exit();
    }

    #endregion Entry Point

    #region Fields

    private KeyboardHook _keyboardHook;
    private bool _windowsKeyPressed;

    #endregion Fields
  }
}