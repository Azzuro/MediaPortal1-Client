#region Copyright (C) 2005-2012 Team MediaPortal

// Copyright (C) 2005-2012 Team MediaPortal
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

#region usings

using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using MediaPortal;
using MediaPortal.Common.Utils;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.InputDevices;
using MediaPortal.IR;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.RedEyeIR;
using MediaPortal.Ripper;
using MediaPortal.SerialIR;
using MediaPortal.Util;
using MediaPortal.Services;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.Win32;
using Action = MediaPortal.GUI.Library.Action;

#endregion

namespace MediaPortal
{
  [Flags]
  // ReSharper disable InconsistentNaming
  public enum EXECUTION_STATE : uint
  {
    ES_SYSTEM_REQUIRED  = 0x00000001,
    ES_DISPLAY_REQUIRED = 0x00000002,
    ES_CONTINUOUS       = 0x80000000
  }
  // ReSharper restore InconsistentNaming
}

/// <summary>
/// 
/// </summary>
public class MediaPortalApp : D3DApp, IRender
{
  #region vars

#if AUTOUPDATE
  private ApplicationUpdateManager _updater = null;
  private Thread _updaterThread = null;
  private const int UPDATERTHREAD_JOIN_TIMEOUT = 3 * 1000;
#endif
  private Point _lastCursorPosition;
  private bool _playingState;
  private bool _showStats;
  private bool _showStatsPrevious;
  private readonly Rectangle[] _region = new Rectangle[1];
  private int _xpos = 50;
  private int _frameCount;
  private SerialUIR _serialuirdevice;
  private USBUIRT _usbuirtdevice;
  private WinLirc _winlircdevice; //sd00//
  private RedEye _redeyedevice; //PB00//
  private readonly bool _useScreenSaver = true;
  private readonly bool _useIdleblankScreen;
  private static bool _isWinScreenSaverInUse;
  private readonly int _timeScreenSaver = 300;
  private bool _restoreTopMost;
  private bool _startWithBasicHome;
  private bool _useOnlyOneHome;
  private bool _suspended;
  private bool _suspending;
  private bool _resuming;
  private bool _ignoreContextMenuAction;
  private DateTime _lastContextMenuAction = DateTime.MaxValue;
  protected string DateFormat = string.Empty;
  protected bool UseLongDateFormat = false;
  private readonly bool _showLastActiveModule;
  private readonly int _suspendGracePeriodSec = 5;
  // ReSharper disable NotAccessedField.Local
  private int _dateLayout;
  private int _lastActiveModule = -1;
  private bool _lastActiveModuleFullscreen;
  // ReSharper restore NotAccessedField.Local
  private static bool _mpCrashed;
  private static int _startupDelay;
  private static bool _waitForTvServer;
  private static DateTime _lastOnresume = DateTime.Now;
  private static string _alternateConfig = string.Empty;
  private static string _safePluginsList;
#if AUTOUPDATE
  string m_strNewVersion = "";
  bool m_bNewVersionAvailable = false;
  bool m_bCancelVersion = false;
#endif
  private MouseEventArgs _lastMouseClickEvent;
  private bool _mouseClickFired;

  // ReSharper disable InconsistentNaming
  private const int WM_SYSCOMMAND           = 0x0112; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646360(v=vs.85).aspx
  private const int SC_MINIMIZE             = 0xF020; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646360(v=vs.85).aspx
  private const int SC_SCREENSAVE           = 0xF140; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646360(v=vs.85).aspx
  private const int SC_MONITORPOWER         = 0xF170; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646360(v=vs.85).aspx
  private const int WM_ENDSESSION           = 0x0016; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa376889(v=vs.85).aspx
  private const int WM_DEVICECHANGE         = 0x0219; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa363480(v=vs.85).aspx
  private const int WM_QUERYENDSESSION      = 0x0011; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa376890(v=vs.85).aspx
  private const int WM_ACTIVATE             = 0x0006; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646274(v=vs.85).aspx
  private const int WA_INACTIVE             = 0;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646274(v=vs.85).aspx
  private const int WA_ACTIVE               = 1;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646274(v=vs.85).aspx
  private const int WA_CLICKACTIVE          = 2;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646274(v=vs.85).aspx
  private const int WM_SIZING               = 0x0214; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_LEFT               = 1;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_RIGHT              = 2;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_TOP                = 3;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_TOPLEFT            = 4;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_TOPRIGHT           = 5;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_BOTTOM             = 6;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_BOTTOMLEFT         = 7;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_BOTTOMRIGHT        = 8;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WM_GETMINMAXINFO        = 0x0024; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632626(v=vs.85).aspx
  private const int WM_MOVING               = 0x0216; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632632(v=vs.85).aspx
  private const int WM_CREATE               = 0x0001; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632619(v=vs.85).aspx
  private const int WM_POWERBROADCAST       = 0x0218; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa373247(v=vs.85).aspx
  private const int PBT_APMSUSPEND          = 0x0004; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa372721(v=vs.85).aspx
  private const int PBT_APMRESUMECRITICAL   = 0x0006; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa372719(v=vs.85).aspx
  private const int PBT_APMRESUMESUSPEND    = 0x0007; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa372720(v=vs.85).aspx
  private const int PBT_APMRESUMEAUTOMATIC  = 0x0012; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa372718(v=vs.85).aspx
  private const int SPI_SETSCREENSAVEACTIVE = 0x0011; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724947(v=vs.85).aspx
  private const int SPI_GETSCREENSAVEACTIVE = 0x0010; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724947(v=vs.85).aspx
  private const int SPIF_SENDCHANGE         = 0x0002; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724947(v=vs.85).aspx
  private const int D3DERR_DEVICEHUNG       = -2005530508; // http://msdn.microsoft.com/en-us/library/windows/desktop/bb172554(v=vs.85).aspx
  private const int D3DERR_DEVICEREMOVED    = -2005530512; // http://msdn.microsoft.com/en-us/library/windows/desktop/bb172554(v=vs.85).aspx
  private const int D3DERR_INVALIDCALL      = -2005530516; // http://msdn.microsoft.com/en-us/library/windows/desktop/bb172554(v=vs.85).aspx
  // ReSharper restore InconsistentNaming

  private const string MPMutex = "{E0151CBA-7F81-41df-9849-F5298A779EB3}";
#pragma warning disable 169
  private const string ConfigMutex = "{0BFD648F-A59F-482A-961B-337D70968611}";
#pragma warning restore 169
  private bool _supportsFiltering;
  private bool _supportsAlphaBlend;
  private int _anisotropy;
  private DateTime _updateTimer = DateTime.MinValue;
  private static SplashScreen _splashScreen;
#if !DEBUG
  private static bool _avoidVersionChecking;
#endif
  private string _outdatedSkinName;
  private static bool _isRendering;
  private int _errorCounter;
  private RECT _lastRect; 
  private readonly int _clientSizeConfigX;
  private readonly int _clientSizeConfigY;
  // ReSharper disable ConvertToConstant.Local
  private readonly bool _allowMinOOB = true;
  private readonly bool _allowMaxOOB = true;
  // ReSharper restore ConvertToConstant.Local

  #endregion

  #region structs

  [StructLayout(LayoutKind.Sequential)]
  // ReSharper disable InconsistentNaming
  private struct RECT
  {
    public int left;
    public int top;
    public int right;
    public int bottom;
  }
  // ReSharper restore InconsistentNaming

  // ReSharper disable InconsistentNaming
  // ReSharper disable NotAccessedField.Local
#pragma warning disable 169, 649
  private struct POINTAPI
  {
    public int x;
    public int y;
  }

  private struct MINMAXINFO
  {
    public POINTAPI ptReserved;
    public POINTAPI ptMaxSize;
    public POINTAPI ptMaxPosition;
    public POINTAPI ptMinTrackSize;
    public POINTAPI ptMaxTrackSize;
  }
#pragma warning restore 169, 649
  // ReSharper restore NotAccessedField.Local
  // ReSharper restore InconsistentNaming

  #endregion

  // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724947(v=vs.85).aspx
  [DllImport("user32")]
  private static extern bool SystemParametersInfo(int uAction, int uParam, ref bool lpvParam, int fuWinIni);

  // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724947(v=vs.85).aspx
  [DllImport("user32")]
  private static extern bool SystemParametersInfo(int uAction, int uParam, int lpvParam, int fuWinIni);

  // http://msdn.microsoft.com/en-us/library/windows/desktop/aa373208(v=vs.85).aspx
  [DllImport("Kernel32.dll")]
  private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE state);

  // http://msdn.microsoft.com/en-us/library/windows/desktop/aa372708(v=vs.85).aspx
  [DllImport("Kernel32.dll")]
  private static extern bool IsSystemResumeAutomatic();

  private static RestartOptions _restartOptions = RestartOptions.Reboot;
  private static bool _useRestartOptions;

  [DllImport("shlwapi.dll")]
  private static extern bool PathIsNetworkPath(string path);

  #region main()

  // NProf doesn't work if the [STAThread] attribute is set
  // but is needed when you want to play music or video
  [STAThread]
  public static void Main(string[] args)
  {
    Thread.CurrentThread.Name = "MPMain";
    if (args.Length > 0)
    {
      foreach (string arg in args)
      {
        if (arg == "/fullscreen")
        {
          FullscreenOverride = true;
        }
        if (arg == "/windowed")
        {
          WindowedOverride = true;
        }
        if (arg.StartsWith("/fullscreen="))
        {
          string argValue = arg.Remove(0, 12); // remove /?= from the argument  
          FullscreenOverride |= argValue != "no";
          WindowedOverride |= argValue.Equals("no");
        }
        if (arg == "/crashtest")
        {
          _mpCrashed = true;
        }
        if (arg.StartsWith("/screen="))
        {
          GUIGraphicsContext._useScreenSelector = true;
          string screenarg = arg.Remove(0, 8); // remove /?= from the argument          
          if (!int.TryParse(screenarg, out ScreenNumberOverride))
          {
            ScreenNumberOverride = -1;
          }
        }
        if (arg.StartsWith("/skin="))
        {
          string skinOverrideArg = arg.Remove(0, 6); // remove /?= from the argument
          SkinOverride = skinOverrideArg;
        }
        if (arg.StartsWith("/config="))
        {
          _alternateConfig = arg.Remove(0, 8); // remove /?= from the argument
          if (!Path.IsPathRooted(_alternateConfig))
          {
            _alternateConfig = Config.GetFile(Config.Dir.Config, _alternateConfig);
          }
        }
        if (arg.StartsWith("/safelist="))
        {
          _safePluginsList = arg.Remove(0, 10); // remove /?= from the argument
        }

#if !DEBUG
        _avoidVersionChecking = false;
        if (arg.ToLowerInvariant() == "/avoidversioncheck")
        {
          _avoidVersionChecking = true;
          Log.Warn("Version check is disabled by command line switch \"/avoidVersionCheck\"");
        }
#endif
      }
    }

    if (string.IsNullOrEmpty(_alternateConfig))
    {
      Log.BackupLogFiles();
    }
    else
    {
      if (File.Exists(_alternateConfig))
      {
        try
        {
          MPSettings.ConfigPathName = _alternateConfig;
          Log.BackupLogFiles();
          Log.Info("Using alternate configuration file: {0}", MPSettings.ConfigPathName);
        }
        catch (Exception ex)
        {
          Log.BackupLogFiles();
          Log.Error("Failed to change to alternate configuration file:");
          Log.Error(ex);
        }
      }
      else
      {
        Log.BackupLogFiles();
        Log.Info("Alternative configuration file was specified but the file was not found: '{0}'", _alternateConfig);
        Log.Info("Using default configuration file instead.");
      }
    }

    if (!Config.DirsFileUpdateDetected)
    {
      // check if Mediaportal has been configured
      var fi = new FileInfo(MPSettings.ConfigPathName);
      if (!File.Exists(MPSettings.ConfigPathName) || (fi.Length < 10000))
      {
        // no, then start configuration.exe in wizard form
        Log.Info("MediaPortal.xml not found. Launching configuration tool and exiting...");
        try
        {
          Process.Start(Config.GetFile(Config.Dir.Base, "configuration.exe"), @"/wizard");
        }
        catch {} // no exception logging needed, since MP is now closed
        return;
      }

      bool autoHideTaskbar;
#if !DEBUG
      bool watchdogEnabled;
      bool restartOnError;
      int restartDelay;
#endif
      using (Settings xmlreader = new MPSettings())
      {
        string threadPriority = xmlreader.GetValueAsString("general", "ThreadPriority", "Normal");
        switch (threadPriority)
        {
          case "AboveNormal":
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
            break;
          case "High":
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            break;
          case "BelowNormal":
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
            break;
        }
        autoHideTaskbar = xmlreader.GetValueAsBool("general", "hidetaskbar", false);
        _startupDelay = xmlreader.GetValueAsBool("general", "delay startup", false) 
                          ? xmlreader.GetValueAsInt("general", "delay", 0)
                          : 0;
        _waitForTvServer = xmlreader.GetValueAsBool("general", "wait for tvserver", false);
#if !DEBUG
        watchdogEnabled = xmlreader.GetValueAsBool("general", "watchdogEnabled", true);
        restartOnError = xmlreader.GetValueAsBool("general", "restartOnError", false);
        restartDelay = xmlreader.GetValueAsInt("general", "restart delay", 10);        
#endif
        GUIGraphicsContext._useScreenSelector |= xmlreader.GetValueAsBool("screenselector", "usescreenselector", false);
      }
#if !DEBUG
      AddExceptionHandler();
      if (watchdogEnabled)
      {
        using (var sw = new StreamWriter(Config.GetFile(Config.Dir.Config, "mediaportal.running"), false))
        {
          sw.WriteLine("running");
          sw.Close();
        }
        Log.Info("Main: Starting MPWatchDog");
        string cmdargs = "-watchdog";
        if (restartOnError)
        {
          cmdargs += " -restartMP " + restartDelay.ToString(CultureInfo.InvariantCulture);
        }
        var mpWatchDog = new Process
                           {
                             StartInfo =
                               {
                                 ErrorDialog = true,
                                 UseShellExecute = true,
                                 WorkingDirectory = Application.StartupPath,
                                 FileName = "WatchDog.exe",
                                 Arguments = cmdargs
                               }
                           };
        mpWatchDog.Start();
      }
#endif
      // Log MediaPortal version build and operating system level
      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);

      Log.Info("Main: MediaPortal v" + versionInfo.FileVersion + " is starting up on " + OSInfo.OSInfo.GetOSDisplayVersion());
#if DEBUG
      Log.Info("Debug Build: " + Application.ProductVersion);
#else
      Log.Info("Build: " + Application.ProductVersion);
#endif

      // Check for unsupported operating systems
      OSPrerequisites.OSPrerequisites.OsCheck(false);

      // Log last install of WindowsUpdate patches
      string lastSuccessTime = "NEVER !!!";
      UIntPtr res;

      int options = Convert.ToInt32(Reg.RegistryRights.ReadKey);
      if (OSInfo.OSInfo.Xp64OrLater())
      {
        options = options | Convert.ToInt32(Reg.RegWow64Options.KEY_WOW64_64KEY);
      }
      var rKey = new UIntPtr(Convert.ToUInt32(Reg.RegistryRoot.HKLM));
      int lastError;
      int retval = Reg.RegOpenKeyEx(rKey, 
                                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update\\Results\\Install",
                                    0, options, out res);
      if (retval == 0)
      {
        uint tKey;
        uint lKey = 100;
        var sKey = new System.Text.StringBuilder((int)lKey);
        retval = Reg.RegQueryValueEx(res, "LastSuccessTime", 0, out tKey, sKey, ref lKey);
        if (retval == 0)
        {
          lastSuccessTime = sKey.ToString();
        }
        else
        {
          lastError = Marshal.GetLastWin32Error();
          Log.Debug("RegQueryValueEx retval=<{0}>, lastError=<{1}>", retval, lastError);
        }
      }
      else
      {
        lastError = Marshal.GetLastWin32Error();
        Log.Debug("RegOpenKeyEx retval=<{0}>, lastError=<{1}>", retval, lastError);
      }
      Log.Info("Main: Last install from WindowsUpdate is dated {0}", lastSuccessTime);

      Log.Debug("Disabling process window ghosting");
      NativeMethods.DisableProcessWindowsGhosting();

      // Start MediaPortal
      Log.Info("Main: Using Directories:");
      foreach (Config.Dir option in Enum.GetValues(typeof (Config.Dir)))
      {
        Log.Info("{0} - {1}", option, Config.GetFolder(option));
      }
      var mpFi = new FileInfo(Assembly.GetExecutingAssembly().Location);
      Log.Info("Main: Assembly creation time: {0} (UTC)", mpFi.LastWriteTimeUtc.ToUniversalTime());
      using (var processLock = new ProcessLock(MPMutex))
      {
        if (processLock.AlreadyExists)
        {
          Log.Warn("Main: MediaPortal is already running");
          Win32API.ActivatePreviousInstance();
        }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        // Set current directory
        string applicationPath = Application.ExecutablePath;
        applicationPath = Path.GetFullPath(applicationPath);
        applicationPath = Path.GetDirectoryName(applicationPath);
        if (!String.IsNullOrEmpty(applicationPath))
        {
          Directory.SetCurrentDirectory(applicationPath);
          Log.Info("Main: Set current directory to: {0}", applicationPath);
        }
        else
        {
          Log.Error("Main: Cannot set current directory to {0}", applicationPath);
        }


        // Localization strings for new splash screen and for MediaPortal itself
        LoadLanguageString();

        // Initialize the skin and theme prior to beginning the splash screen thread.  This provides for the splash screen to be used in a theme.
        string skin;
        try
        {
          using (Settings xmlreader = new MPSettings())
          {
            skin = string.IsNullOrEmpty(SkinOverride) ? xmlreader.GetValueAsString("skin", "name", "Default") : SkinOverride;
          }
        }
        catch (Exception)
        {
           skin = "Default";
        }
        Config.SkinName = skin;
        GUIGraphicsContext.Skin = skin;
        SkinSettings.Load();

        // Send a message that the skin has changed.
        var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SKIN_CHANGED, 0, 0, 0, 0, 0, null);
        GUIGraphicsContext.SendMessage(msg);

        Log.Info("Main: Skin is {0} using theme {1}", skin, GUIThemeManager.CurrentTheme);

//#if !DEBUG
        string version = ConfigurationManager.AppSettings["version"];
        _splashScreen = new SplashScreen {Version = version};
        _splashScreen.Run();
//#endif
        Application.DoEvents();
        if (_waitForTvServer)
        {
          Log.Debug("Main: Wait for TV service requested. Checking if installed...");
          ServiceController ctrl;
          try
          {
            ctrl = new ServiceController("TVService");
          }
          catch (Exception)
          {
            ctrl = null;
            Log.Debug("Main: TV service not installed - proceeding...");
          }
          if (ctrl != null)
          {
            Log.Debug("Main: TV service found. Checking status...");
            if (_splashScreen != null)
            {
              _splashScreen.SetInformation(GUILocalizeStrings.Get(60)); // Waiting for startup of TV service...
            }
            if (ctrl.Status == ServiceControllerStatus.StartPending || ctrl.Status == ServiceControllerStatus.Stopped)
            {
              if (ctrl.Status == ServiceControllerStatus.StartPending)
              {
                Log.Info("Main: TV service start is pending. Waiting...");
              }
              if (ctrl.Status == ServiceControllerStatus.Stopped)
              {
                Log.Info("Main: TV service is stopped, so we try start it...");
                try
                {
                  ctrl.Start();
                }
                catch (Exception)
                {
                  Log.Info("TvService seems to be already starting up.");
                }
              }
              try
              {
                ctrl.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 45));
              }
              catch (Exception) {}
              if (ctrl.Status == ServiceControllerStatus.Running)
              {
                Log.Info("Main: The TV service has started successfully.");
              }
              else
              {
                Log.Info("Main: Startup of the TV service failed - current status: {0}", ctrl.Status.ToString());
              }
            }
            Log.Info("Main: TV service is in status {0} - proceeding...", ctrl.Status.ToString());
            ctrl.Close();
          }
        }
        Application.DoEvents();
        if (_startupDelay > 0)
        {
          Log.Info("Main: Waiting {0} second(s) before startup", _startupDelay);
          for (int i = _startupDelay; i > 0; i--)
          {
            if (_splashScreen != null)
            {
              _splashScreen.SetInformation(String.Format(GUILocalizeStrings.Get(61), i.ToString(CultureInfo.InvariantCulture)));
            }
            Application.DoEvents();
          }
        }
        Log.Debug("Main: Checking prerequisites");
        try
        {
          // CHECK if DirectX 9.0c if installed
          Log.Debug("Main: Verifying DirectX 9");
          if (!DirectXCheck.IsInstalled())
          {
            string strLine = "Please install a newer DirectX 9.0c redist!\r\n";
            strLine = strLine + "MediaPortal cannot run without DirectX 9.0c redist (August 2008)\r\n";
            strLine = strLine + "http://install.team-mediaportal.com/DirectX";
#if !DEBUG
            if (_splashScreen != null)
            {
              _splashScreen.Stop();
              _splashScreen = null;
            }
#endif
            MessageBox.Show(strLine, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
          }
          Application.DoEvents();

          // CHECK if Windows MediaPlayer 11 is installed
          const string wmpMainVer = "11";
          Log.Debug("Main: Verifying Windows Media Player");

          Version aParamVersion;
          if (FilterChecker.CheckFileVersion(Environment.SystemDirectory + "\\wmp.dll", wmpMainVer + ".0.0000.0000", out aParamVersion))
          {
            Log.Info("Main: Windows Media Player version {0} installed", aParamVersion);
          }
          else
          {
#if !DEBUG
            if (_splashScreen != null)
            {
              _splashScreen.Stop();
              _splashScreen = null;
            }
#endif
            string strLine = "Please install Windows Media Player " + wmpMainVer + "\r\n";
            strLine = strLine + "MediaPortal cannot run without Windows Media Player " + wmpMainVer;
            MessageBox.Show(strLine, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
          }

#if !DEBUG
          // Check TvPlugin version
          string mpExe = Assembly.GetExecutingAssembly().Location;
          string tvPlugin = Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll";
          if (File.Exists(tvPlugin) && !_avoidVersionChecking)
          {
            string tvPluginVersion = FileVersionInfo.GetVersionInfo(tvPlugin).ProductVersion;
            string mpVersion = FileVersionInfo.GetVersionInfo(mpExe).ProductVersion;
            if (mpVersion != tvPluginVersion)
            {
              string strLine = "TvPlugin and MediaPortal don't have the same version.\r\n";
              strLine += "Please update the older component to the same version as the newer one.\r\n";
              strLine += "MediaPortal Version: " + mpVersion + "\r\n";
              strLine += "TvPlugin    Version: " + tvPluginVersion;
              if (_splashScreen != null)
              {
                _splashScreen.Stop();
                _splashScreen = null;
              }
              MessageBox.Show(strLine, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
              Log.Info(strLine);
              return;
            }
          }
#endif
        }
        catch (Exception) {}
#if !DEBUG
        try
        {
#endif
        Application.DoEvents();
        if (_splashScreen != null)
        {
          _splashScreen.SetInformation(GUILocalizeStrings.Get(62)); // Initializing DirectX...
        }
        var app = new MediaPortalApp();
        Log.Debug("Main: Initializing DirectX");
        if (app.CreateGraphicsSample())
        {
          // Initialize Input Devices
          if (_splashScreen != null)
          {
            _splashScreen.SetInformation(GUILocalizeStrings.Get(63)); // Initializing input devices...
          }
          InputDevices.Init();
          try
          {
            Log.Info("Main: Running");
            GUIGraphicsContext.BlankScreen = false;
            Application.Run(app);
            app.Focus();
            Debug.WriteLine("after Application.Run");
          }
          catch (Exception ex)
          {
            Log.Error(ex);
            Log.Error("MediaPortal stopped due to an exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
            _mpCrashed = true;
          }
          app.OnExit();
        }
#if !DEBUG
        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Log.Error("MediaPortal stopped due to an exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
          _mpCrashed = true;
        }
#endif
#if !DEBUG
        if (_splashScreen != null)
        {
          _splashScreen.Stop();
          _splashScreen = null;
        }
#endif
        Settings.SaveCache();

        if (autoHideTaskbar)
        {
          // only re-show the task bar if MP is the one that has hidden it.
          HideTaskBar(false);
        }
        if (_useRestartOptions)
        {
          Log.Info("Main: Exiting Windows - {0}", _restartOptions);
          if (File.Exists(Config.GetFile(Config.Dir.Config, "mediaportal.running")))
          {
            File.Delete(Config.GetFile(Config.Dir.Config, "mediaportal.running"));
          }
          WindowsController.ExitWindows(_restartOptions, false);
        }
        else
        {
          if (!_mpCrashed && File.Exists(Config.GetFile(Config.Dir.Config, "mediaportal.running")))
          {
            File.Delete(Config.GetFile(Config.Dir.Config, "mediaportal.running"));
          }
        }
      }
    }
    else
    {
      string msg = "The file MediaPortalDirs.xml has been changed by a recent update in the MediaPortal application directory.\n\n";
      msg += "You have to open the file ";
      msg += Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Team MediaPortal\MediaPortalDirs.xml";
      msg += " with an editor, update it with all changes and SAVE it at least once to start up MediaPortal successfully after this update.\n\n";
      msg += "If you are not using windows user profiles for MediaPortal's configuration management, ";
      msg += "just delete the whole directory mentioned above and reconfigure MediaPortal.";
      string msg2 = "\n\n\n";
      msg2 += "Do you want to open your local file now?";
      Log.Error(msg);
#if !DEBUG
      if (_splashScreen != null)
      {
        _splashScreen.Stop();
        _splashScreen = null;
      }
#endif
      DialogResult result = MessageBox.Show(msg + msg2, "MediaPortal - Update Conflict", MessageBoxButtons.YesNo, MessageBoxIcon.Stop);
      try
      {
        if (result == DialogResult.Yes)
        {
          Process.Start("notepad.exe",
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                        @"\Team MediaPortal\MediaPortalDirs.xml");
        }
      }
      catch (Exception)
      {
        MessageBox.Show(
          "Error opening file " + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
          @"\Team MediaPortal\MediaPortalDirs.xml using notepad.exe", "Error", MessageBoxButtons.OK,
          MessageBoxIcon.Error);
      }
    }
    Environment.Exit(0);
  }

#if !DEBUG
  private static UnhandledExceptionLogger _logger;

  /// <remark>This method is only used in release builds.</remark>
  private static void AddExceptionHandler()
  {
    _logger = new UnhandledExceptionLogger();
    AppDomain current = AppDomain.CurrentDomain;
    current.UnhandledException += _logger.LogCrash;
  }
#endif

  #endregion

  #region remote callbacks

  /// <summary>
  /// 
  /// </summary>
  /// <param name="command"></param>
  private static void OnRemoteCommand(object command)
  {
    GUIGraphicsContext.OnAction(new Action((Action.ActionType)command, 0, 0));
  }

  #endregion

  #region ctor

  /// <summary>
  /// 
  /// </summary>
  public MediaPortalApp()
  {
    int screenNumber = 0;
    // check to load plugins
    using (Settings xmlreader = new MPSettings())
    {
      _suspendGracePeriodSec = xmlreader.GetValueAsInt("general", "suspendgraceperiod", 5);
      _useScreenSaver = xmlreader.GetValueAsBool("general", "IdleTimer", true);
      _timeScreenSaver = xmlreader.GetValueAsInt("general", "IdleTimeValue", 300);
      _useIdleblankScreen = xmlreader.GetValueAsBool("general", "IdleBlanking", false);
      _clientSizeConfigX = xmlreader.GetValueAsInt("general", "sizex", 720);
      _clientSizeConfigY = xmlreader.GetValueAsInt("general", "sizey", 576);
      _showLastActiveModule = xmlreader.GetValueAsBool("general", "showlastactivemodule", false);
      _lastActiveModule = xmlreader.GetValueAsInt("general", "lastactivemodule", -1);
      _lastActiveModuleFullscreen = xmlreader.GetValueAsBool("general", "lastactivemodulefullscreen", false);
      screenNumber = xmlreader.GetValueAsInt("screenselector", "screennumber", screenNumber);
    }
    if (GUIGraphicsContext._useScreenSelector)
    {
      if (ScreenNumberOverride >= 0)
      {
        screenNumber = ScreenNumberOverride;
      }
      if (screenNumber < 0 || screenNumber >= Screen.AllScreens.Length)
      {
        screenNumber = 0;
      }
      Log.Info("currentScreenNr:" + screenNumber);
      GUIGraphicsContext.currentScreen = Screen.AllScreens[screenNumber];
    }
    // check if MediaPortal is already running...
    Log.Info("Main: Checking for running MediaPortal instance");
    Log.Info(@"Main: Deleting old log\capture.log");
    Utils.FileDelete(Config.GetFile(Config.Dir.Log, "capture.log"));
    ClientSize = new Size(_clientSizeConfigX, _clientSizeConfigY);
    Text = "MediaPortal";
    GUIGraphicsContext.form = this;
    GUIGraphicsContext.graphics = null;
    GUIGraphicsContext.RenderGUI = this;
    try
    {
      using (Settings xmlreader = new MPSettings())
      {
        AutoHideMouse = xmlreader.GetValueAsBool("general", "autohidemouse", true);
        GUIGraphicsContext.MouseSupport = xmlreader.GetValueAsBool("gui", "mousesupport", false);
        GUIGraphicsContext.AllowRememberLastFocusedItem = xmlreader.GetValueAsBool("gui", "allowRememberLastFocusedItem", false);
        GUIGraphicsContext.DBLClickAsRightClick = xmlreader.GetValueAsBool("general", "dblclickasrightclick", false);
        MinimizeOnStartup = xmlreader.GetValueAsBool("general", "minimizeonstartup", false);
        MinimizeOnGuiExit = xmlreader.GetValueAsBool("general", "minimizeonexit", false);
      }
    }
    catch (Exception)
    {
    }

    SetStyle(ControlStyles.Opaque, true);
    SetStyle(ControlStyles.UserPaint, true);
    SetStyle(ControlStyles.AllPaintingInWmPaint, true);
    SetStyle(ControlStyles.DoubleBuffer, false);
    Activated += MediaPortalAppActivated;
    Deactivate += MediaPortalAppDeactivate;
    Log.Info("Main: Checking skin version");
    CheckSkinVersion();
    using (Settings xmlreader = new MPSettings())
    {
      var startFullscreen = !WindowedOverride && (FullscreenOverride || xmlreader.GetValueAsBool("general", "startfullscreen", false));
      Windowed = !startFullscreen;
    }
    DoStartupJobs();
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  private static void MediaPortalAppDeactivate(object sender, EventArgs e)
  {
    GUIGraphicsContext.HasFocus = false;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  private static void MediaPortalAppActivated(object sender, EventArgs e)
  {
    GUIGraphicsContext.HasFocus = true;
  }

  #endregion

  #region RenderStats() method
  
  /// <summary>
  /// 
  /// </summary>
  private void RenderStats()
  {
    try
    {
      UpdateStats();

      if (GUIGraphicsContext.IsEvr && g_Player.HasVideo)
      {
        if (_showStats != _showStatsPrevious)
        {
          // notify EVR presenter only when the setting changes
          if (VMR9Util.g_vmr9 == null)
          {
            return;
          }
          VMR9Util.g_vmr9.EnableEVRStatsDrawing(_showStats);
        }
        // EVR presenter will draw the stats internally
        _showStatsPrevious = _showStats;
        return;
      }
      _showStatsPrevious = false;

      if (_showStats)
      {
        GetStats();
        GUIFont font = GUIFontManager.GetFont(0);
        if (font != null)
        {
          GUIGraphicsContext.SetScalingResolution(0, 0, false);
          // '\n' doesn't work with the DirectX9 Ex device, so the string is split into two
          font.DrawText(80, 40, 0xffffffff, FrameStatsLine1, GUIControl.Alignment.ALIGN_LEFT, -1);
          font.DrawText(80, 55, 0xffffffff, FrameStatsLine2, GUIControl.Alignment.ALIGN_LEFT, -1);
          _region[0].X = _xpos;
          _region[0].Y = 0;
          _region[0].Width = 4;
          _region[0].Height = GUIGraphicsContext.Height;
          GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.FromArgb(255, 255, 255, 255), 1.0f, 0, _region);
          float fStep = (GUIGraphicsContext.Width - 100);
          fStep /= (2f * 16f);
          fStep /= GUIGraphicsContext.CurrentFPS;
          _frameCount++;
          if (_frameCount >= (int)fStep)
          {
            _frameCount = 0;
            _xpos += 12;
            if (_xpos > GUIGraphicsContext.Width - 50)
            {
              _xpos = 50;
            }
          }
        }
      }
    }
    catch 
    {
      // Intentionally left blank - if stats rendering fails it is not a critical issue
    }
  }

  #endregion

  #region PreProcessMessage() and WndProc()

  /// <summary>
  /// Find the Greatest Common Divisor
  /// </summary>
  /// <param name="a">Number a</param>
  /// <param name="b">Number b</param>
  /// <returns>The greatest common Divisor</returns>
  private static long GCD(long a, long b)
  {
    while (b != 0)
    {
      var tmp = b;
      b = a % b;
      a = tmp;
    }
    return a;
  }
  

  /// <summary>
  /// Message Pump
  /// </summary>
  /// <param name="msg"></param>
  protected override void WndProc(ref Message msg)
  {
    try
    {
      RECT rc;
      Rectangle bounds;

      switch (msg.Msg)
      {
        // power management
        case WM_POWERBROADCAST:
          Log.Info("Main: WM_POWERBROADCAST: {0}", msg.WParam.ToInt32());
          switch (msg.WParam.ToInt32())
          {
            case PBT_APMSUSPEND:
              Log.Info("Main: Suspending operation.");
              OnSuspend();
              break;

            // When resuming from hibernation, the OS always assume that a user is present. This is by design of Windows.
            case PBT_APMRESUMEAUTOMATIC:
              bool useS3Hack;
              using (Settings xmlreader = new MPSettings())
              {
                useS3Hack = xmlreader.GetValueAsBool("debug", "useS3Hack", false);
              }
            
              if (useS3Hack)
              {
                Log.Info("Main: Resuming operation (useS3Hack enabled)");
                OnResume();
              }
              else
              {
                Log.Info("Main: Automatic Resume - doing nothing");
              }
              break;

            case PBT_APMRESUMECRITICAL:
            case PBT_APMRESUMESUSPEND:
              Log.Info("Main: Resuming operation.");
              OnResume();
              break;
          }
          msg.Result = (IntPtr)1;
          break;

        // window was created but not yet shown
        case WM_CREATE:
          Log.Debug("Main: WM_CREATE");
          _lastRect.top    = 0;
          _lastRect.left   = 0;
          _lastRect.bottom = Height;
          _lastRect.right  = Width;
          msg.Result = (IntPtr)0;
          break;

        // set maximum and minimum form size in windowed mode
        case WM_GETMINMAXINFO:
          Log.Debug("Main: WM_GETMINMAXINFO");
          if (FormBorderStyle == FormBorderStyle.Sizable)
          {
            var mmi = (MINMAXINFO)Marshal.PtrToStructure(msg.LParam, typeof(MINMAXINFO));
            var ratio = Math.Min((double)GUIGraphicsContext.currentScreen.WorkingArea.Width / Width, 
                                 (double)GUIGraphicsContext.currentScreen.WorkingArea.Height / Height);
            mmi.ptMaxSize.x = (int)(Width * ratio);
            mmi.ptMaxSize.y = (int)(Height * ratio);
            mmi.ptMaxPosition.x = GUIGraphicsContext.currentScreen.WorkingArea.Left;
            mmi.ptMaxPosition.y = GUIGraphicsContext.currentScreen.WorkingArea.Top;
            mmi.ptMinTrackSize.x = GUIGraphicsContext.SkinSize.Width / 4;
            mmi.ptMinTrackSize.y = GUIGraphicsContext.SkinSize.Height / 4;
            mmi.ptMaxTrackSize.x = GUIGraphicsContext.currentScreen.WorkingArea.Right - GUIGraphicsContext.currentScreen.WorkingArea.Left;
            mmi.ptMaxTrackSize.y = GUIGraphicsContext.currentScreen.WorkingArea.Bottom - GUIGraphicsContext.currentScreen.WorkingArea.Top;
            Marshal.StructureToPtr(mmi, msg.LParam, true);
            msg.Result = (IntPtr)0;
          }
          break;

        // move window
        case WM_MOVING:
          Log.Debug("Main: WM_MOVING");
          if ((!_allowMinOOB && WindowState == FormWindowState.Minimized) || (!_allowMaxOOB && WindowState == FormWindowState.Maximized))
          {
            rc = (RECT)Marshal.PtrToStructure(msg.LParam, typeof(RECT));
            bounds = GUIGraphicsContext.currentScreen.WorkingArea;
            // out of bounds check
            if (rc.top < bounds.Top || rc.bottom > bounds.Bottom || rc.right > bounds.Right || rc.left < bounds.Left)
            {
              rc = _lastRect;
            }
            Marshal.StructureToPtr(rc, msg.LParam, false);
            _lastRect = rc;
          }
          msg.Result = (IntPtr)1;
          break;
        
        // aspect ratio save window resizing (except when being in MiniTV Mode)
        case WM_SIZING:
          Log.Debug("Main WM_SIZING");

          rc = (RECT) Marshal.PtrToStructure(msg.LParam, typeof(RECT));
          var borderWidth  = Width  - ClientSize.Width;
          var borderHeight = Height - ClientSize.Height;
          var width        = rc.right  - rc.left - borderWidth;
          var height       = rc.bottom - rc.top  - borderHeight;
          var gcd          = GCD(GUIGraphicsContext.SkinSize.Width, GUIGraphicsContext.SkinSize.Height);
          var ratioX       = (double) GUIGraphicsContext.SkinSize.Width  / gcd;
          var ratioY       = (double) GUIGraphicsContext.SkinSize.Height / gcd;
          bounds           = GUIGraphicsContext.currentScreen.WorkingArea;

          switch (msg.WParam.ToInt32())
          {
            // adjust height by overriding bottom
            case WMSZ_LEFT:
            case WMSZ_RIGHT:
            case WMSZ_BOTTOMRIGHT:
              rc.bottom = rc.top + borderHeight + (int)(ratioY * width / ratioX);
              break;
            // adjust width by overriding right
            case WMSZ_TOP:
            case WMSZ_BOTTOM:
              rc.right = rc.left + borderWidth + (int)(ratioX * height / ratioY);
              break;
            // adjust width by overriding left
            case WMSZ_TOPLEFT:
            case WMSZ_BOTTOMLEFT:
              rc.left = rc.right - borderWidth - (int)(ratioX * height / ratioY);
              break;
            // adjust height by overriding top
            case WMSZ_TOPRIGHT:
              rc.top = rc.bottom - borderHeight - (int)(ratioY * width / ratioX);
              break;
          }

          // out of bounds check
          if (rc.top < bounds.Top || rc.bottom > bounds.Bottom || rc.right > bounds.Right || rc.left < bounds.Left)
          {
            rc = _lastRect;
          }

          // minimum size check, form cannot be smaller than a quarter of the initial skin size plus window borders
          if (rc.right - rc.left < GUIGraphicsContext.SkinSize.Width / 4 + borderWidth)
          {
            rc = _lastRect;
          }

          // only redraw if rectangle size changed
          if (((rc.right - rc.left) != (_lastRect.right - _lastRect.left)) || ((rc.bottom - rc.top) != (_lastRect.bottom - _lastRect.top)))
          {
            Log.Info("Main: Aspect ratio safe resizing from {0}x{1} to {2}x{3} (Skin resized to {4}x{5})", 
                      _lastRect.right - _lastRect.left, _lastRect.bottom - _lastRect.top, 
                      rc.right - rc.left, rc.bottom - rc.top,
                      rc.right - rc.left - borderWidth, rc.bottom - rc.top - borderHeight);
            OnPaintEvent();
          }
          Marshal.StructureToPtr(rc, msg.LParam, false);
          _lastRect = rc;
          msg.Result = (IntPtr)1;
          break;

        case WM_QUERYENDSESSION:
          Log.Info("Main: Windows is requesting shutdown mode");
          base.WndProc(ref msg);
          Log.Info("Main: shutdown mode granted");
          ShuttingDown = true;
          msg.Result = (IntPtr)1; // tell Windows we are ready to shutdown   
          break;

        case WM_ENDSESSION:
          base.WndProc(ref msg);
          Log.Info("Main: Shutdown mode executed");
          msg.Result = IntPtr.Zero; // tell Windows it's ok to shutdown        
          Application.ExitThread();
          Application.Exit();
          msg.Result = (IntPtr)0;
          break;

        case WM_DEVICECHANGE:
          if (RemovableDriveHelper.HandleDeviceChangedMessage(msg))
          {
            return;
          }
          msg.Result = (IntPtr)1;
          break;

        case WM_ACTIVATE:
          switch (msg.WParam.ToInt32())
          {
            case WA_INACTIVE:
              Log.Info("Main: Deactivation Request Received");
              MinimizeToTray(false);
              break;
            case WA_ACTIVE:
            case WA_CLICKACTIVE:
              Log.Info("Main: Activation Request Received");
              RestoreFromTray();
              break;
          }
          msg.Result = (IntPtr)0;
          break;

        // handle system commands
        case WM_SYSCOMMAND:
          switch (msg.WParam.ToInt32())
          {
            // user clocked on minimize button
            case SC_MINIMIZE:
              Log.Debug("Main: SC_MINIMIZE");
              MinimizeToTray(true);
              break;
            // windows wants to start the screen saver
            case SC_MONITORPOWER:
            case SC_SCREENSAVE:
              if ((GUIGraphicsContext.IsFullScreenVideo && !g_Player.Paused) || GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
              {
                // disable it when we're watching TV/movies/...
                msg.Result = new IntPtr(0);
                return;
              }
              break;
            }
          break;
      }

      // don't continue if activating or deactivating app
      //if (msg.Msg == WM_ACTIVATE)
      //{
      //  g_Player.WndProc(ref msg);
      //  base.WndProc(ref msg);
      //  return;
      //}

      if (PluginManager.WndProc(ref msg))
      {
        return;
      }

      Action action;
      char key;
      Keys keyCode;
      if (InputDevices.WndProc(ref msg, out action, out key, out keyCode))
      {
        if (msg.Result.ToInt32() != 1)
        {
          msg.Result = new IntPtr(0);
        }
        if (action != null && action.wID != Action.ActionType.ACTION_INVALID)
        {
          Log.Info("Main: Incoming action: {0}", action.wID);
          if (ActionTranslator.GetActionDetail(GUIWindowManager.ActiveWindowEx, action))
          {
            if (action.SoundFileName.Length > 0 && !g_Player.Playing)
            {
              Utils.PlaySound(action.SoundFileName, false, true);
            }
          }
          GUIGraphicsContext.ResetLastActivity();
          GUIGraphicsContext.OnAction(action);
        }
        if (keyCode != Keys.A)
        {
          Log.Info("Main: Incoming Keycode: {0}", keyCode.ToString());
          var ke = new KeyEventArgs(keyCode);
          OnKeyDown(ke);
          return;
        }
        if (key != 0)
        {
          Log.Info("Main: Incoming Key: {0}", key);
          var e = new KeyPressEventArgs(key);
          OnKeyPress(e);
          return;
        }
        return;
      }

      g_Player.WndProc(ref msg);
      base.WndProc(ref msg);
    }
    catch (Exception ex)
    {
      Log.Error(ex);
    }
  }

  
  /// <summary>
  /// 
  /// </summary>
  private static void ReOpenDBs()
  {
    string dbPath = FolderSettings.DatabaseName;
    bool isRemotePath = (string.IsNullOrEmpty(dbPath) || PathIsNetworkPath(dbPath));
    if (isRemotePath)
    {
      Log.Info("Main: reopen FolderDatabase3 sqllite database.");
      FolderSettings.ReOpen();
    }
    dbPath = MediaPortal.Picture.Database.PictureDatabase.DatabaseName;
    isRemotePath = (string.IsNullOrEmpty(dbPath) || PathIsNetworkPath(dbPath));
    if (isRemotePath)
    {
      Log.Info("Main: reopen PictureDatabase sqllite database.");
      MediaPortal.Picture.Database.PictureDatabase.ReOpen();
    }
    dbPath = MediaPortal.Video.Database.VideoDatabase.DatabaseName;
    isRemotePath = (string.IsNullOrEmpty(dbPath) || PathIsNetworkPath(dbPath));
    if (isRemotePath)
    {
      Log.Info("Main: reopen VideoDatabaseV5.db3 sqllite database.");
      MediaPortal.Video.Database.VideoDatabase.ReOpen();
    }
    else
    {
      Log.Info("Main: VideoDatabaseV5.db3 sqllite database disk cache activated.");
      MediaPortal.Video.Database.VideoDatabase.RevertFlushTransactionsToDisk();
    }

    dbPath = MediaPortal.Music.Database.MusicDatabase.Instance.DatabaseName;
    isRemotePath = (string.IsNullOrEmpty(dbPath) || PathIsNetworkPath(dbPath));
    if (isRemotePath)
    {
      Log.Info("Main: reopen MusicDatabase.db3 sqllite database.");
      MediaPortal.Music.Database.MusicDatabase.ReOpen();
    }
  }

  // we only dispose the DB connections if the DB path is remote.      
  // since local DBs have no problems.
  private static void DisposeDBs()
  {
    string dbPath = FolderSettings.DatabaseName;
    bool isRemotePath = (!string.IsNullOrEmpty(dbPath) && PathIsNetworkPath(dbPath));
    if (isRemotePath)
    {
      Log.Info("Main: disposing FolderDatabase3 sqllite database.");
      FolderSettings.Dispose();
    }

    dbPath = MediaPortal.Picture.Database.PictureDatabase.DatabaseName;
    isRemotePath = (!string.IsNullOrEmpty(dbPath) && PathIsNetworkPath(dbPath));
    if (isRemotePath)
    {
      Log.Info("Main: disposing PictureDatabase sqllite database.");
      MediaPortal.Picture.Database.PictureDatabase.Dispose();
    }

    dbPath = MediaPortal.Video.Database.VideoDatabase.DatabaseName;
    isRemotePath = (!string.IsNullOrEmpty(dbPath) && PathIsNetworkPath(dbPath));
    if (isRemotePath)
    {
      Log.Info("Main: disposing VideoDatabaseV5.db3 sqllite database.");
      MediaPortal.Video.Database.VideoDatabase.Dispose();
    }
    else
    {
      Log.Info("Main: VideoDatabaseV5.db3 sqllite database cache flushed to disk.");
      MediaPortal.Video.Database.VideoDatabase.FlushTransactionsToDisk();
    }

    dbPath = MediaPortal.Music.Database.MusicDatabase.Instance.DatabaseName;
    isRemotePath = (!string.IsNullOrEmpty(dbPath) && PathIsNetworkPath(dbPath));
    if (isRemotePath)
    {
      Log.Info("Main: disposing MusicDatabase db3 sqllite database.");
      MediaPortal.Music.Database.MusicDatabase.Dispose();
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  private static bool Currentmodulefullscreen()
  {
    bool currentmodulefullscreen = (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN ||
                                    GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_MUSIC ||
                                    GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO ||
                                    GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
    return currentmodulefullscreen;
  }


  /// <summary>
  /// 
  /// </summary>
  private void OnSuspend()
  {
    if (_suspending)
    {
      Log.Debug("Suspending is already in progress");
      return;
    }
    _suspending = true;

    if (!_suspended)
    {
      _ignoreContextMenuAction = true;
      _suspended = true;
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.SUSPENDING; // this will close all open dialogs      

      Log.Info("Main: Stopping playback");
      if (GUIGraphicsContext.IsPlaying)
      {
        Currentmodulefullscreen();
        g_Player.Stop();
        // wait for player to stop before proceeding                
        while (GUIGraphicsContext.IsPlaying)
        {
          Thread.Sleep(100);
        }
      }
      SaveLastActiveModule();

      // stop playback
      _suspended = true;

      Log.Info("Main: Stopping Input Devices");
      InputDevices.Stop();
      
      Log.Info("Main: Stopping AutoPlay");
      AutoPlay.StopListening();
      
      // we only dispose the DB connection if the DB path is remote.      
      DisposeDBs();
      VolumeHandler.Dispose();
      Log.Info("Main: OnSuspend - Done");
    }
    _suspending = false;
  }


  /// <summary>
  /// 
  /// </summary>
  private void OnResume()
  {
    if (_resuming)
    {
      Log.Info("Main: Resuming is already in progress");
      return;
    }
    _resuming = true;
    
    // do nothing if system was not woken up by a user
    if (IsSystemResumeAutomatic())
    {
      return;
    }

    using (Settings xmlreader = new MPSettings())
    {
      int waitOnResume = xmlreader.GetValueAsBool("general", "delay resume", false)
                           ? xmlreader.GetValueAsInt("general", "delay", 0)
                           : 0;
      if (waitOnResume > 0)
      {
        Log.Info("MP waiting on resume {0} secs", waitOnResume);
        Thread.Sleep(waitOnResume * 1000);
      }
    }

    GUIGraphicsContext.ResetLastActivity(); // avoid screen saver after standby
    _ignoreContextMenuAction = true;

    if (!_suspended)
    {
      Log.Error("Main: OnResume - OnResume called but MP is not in suspended state.");
    }
    else
    {
      ReOpenDBs();

      // Systems without DirectX9Ex have lost graphics device in suspend/hibernate cycle
      if (!GUIGraphicsContext.IsDirectX9ExUsed())
      {
        Log.Info("Main: OnResume - set GUIGraphicsContext.State.LOST");
        GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
      }

      Log.Info("Main: OnResume - show last active module?");
      if (!ShowLastActiveModule())
      {
        if (_startWithBasicHome && File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\basichome.xml")))
        {
          Log.Info("Main: OnResume - Switch to basic home screen");
          GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_SECOND_HOME);
        }
        else
        {
          Log.Info("Main: OnResume - Switch to home screen");
          GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_HOME);
        }
      }

      Log.Info("Main: OnResume - Recovering device");
      RecoverDevice();

      Log.Info("Main: OnResume - Resetting executing state");
      SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS); 
 
      Log.Info("Main: OnResume - Init Input Devices");
      InputDevices.Init();
      
      _suspended = false;

      if (GUIGraphicsContext.IsDirectX9ExUsed())
      {
        Log.Info("Main: OnResume - set GUIGraphicsContext.State.RUNNING");
        GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;
      }

      Log.Debug("Main: OnResume - autoplay start listening");
      AutoPlay.StartListening();

      Log.Debug("Main: OnResume - initializing volume handler");
#pragma warning disable 168
      VolumeHandler vh = VolumeHandler.Instance;
#pragma warning restore 168

      _ignoreContextMenuAction = false;
      _lastOnresume = DateTime.Now;
      Log.Info("Main: OnResume - Done");
    }
    _resuming = false;
  }

  #endregion

  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void OnShown(EventArgs e)
  {
    if (MinimizeOnStartup && FirstTimeWindowDisplayed)
    {
      Log.Info("D3D: Minimizing to tray on startup");
      MinimizeToTray(true);
    }
    base.OnShown(e);
  }

  #region process

  /// <summary>
  /// Process() gets called when a dialog is presented.
  /// It contains the message loop 
  /// </summary>
  public void MPProcess()
  {
    if (!_suspended)
    {
      try
      {
        g_Player.Process();
        HandleMessage();
        FrameMove();
        FullRender();
        if (GUIGraphicsContext.Vmr9Active)
        {
          Thread.Sleep(50);
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }
  }

  #endregion

  #region RenderFrame()

  /// <summary>
  /// 
  /// </summary>
  /// <param name="timePassed"></param>
  public void RenderFrame(float timePassed)
  {
    if (!_suspended)
    {
      try
      {
        CreateStateBlock();
        GUILayerManager.Render(timePassed);
        RenderStats();
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        Log.Error("RenderFrame exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
    }
  }

  #endregion

  #region Onstartup() / OnExit()

  /// <summary>
  /// OnStartup() gets called just before the application starts
  /// </summary>
  protected override void OnStartup()
  {
    // set window form styles
    // these styles enable double buffering, which results in no flickering
    Log.Info("Main: Starting up");
    MouseTimeOutTimer = DateTime.Now;
    UpdateSplashScreenMessage(GUILocalizeStrings.Get(64)); // Starting plugins...
    PluginManager.Load();
    PluginManager.Start();
    using (Settings xmlreader = new MPSettings())
    {
      DateFormat = xmlreader.GetValueAsString("home", "dateformat", "<Day> <Month> <DD>");
    }
    // Asynchronously pre-initialize the music engine if we're using the BassMusicPlayer
    if (BassMusicPlayer.IsDefaultMusicPlayer)
    {
      BassMusicPlayer.CreatePlayerAsync();
    }
    try
    {
      GUIPropertyManager.SetProperty("#date", GetDate());
      GUIPropertyManager.SetProperty("#time", GetTime());
      GUIPropertyManager.SetProperty("#Day", GetDay()); // 01
      GUIPropertyManager.SetProperty("#SDOW", GetShortDayOfWeek()); // Sun
      GUIPropertyManager.SetProperty("#DOW", GetDayOfWeek()); // Sunday
      GUIPropertyManager.SetProperty("#Month", GetMonth()); // 01
      GUIPropertyManager.SetProperty("#SMOY", GetShortMonthOfYear()); // Jan
      GUIPropertyManager.SetProperty("#MOY", GetMonthOfYear()); // January
      GUIPropertyManager.SetProperty("#SY", GetShortYear()); // 80
      GUIPropertyManager.SetProperty("#Year", GetYear()); // 1980

      if (_splashScreen != null)
      {
        _splashScreen.Stop();
        Activate();
        while (!_splashScreen.isStopped())
        {
          Thread.Sleep(100);
        }
        _splashScreen = null;
      }

      // disable screen saver when MP running and internal selected
      if (_useScreenSaver)
      {
        SystemParametersInfo(SPI_GETSCREENSAVEACTIVE, 0, ref _isWinScreenSaverInUse, 0);
        if (_isWinScreenSaverInUse)
        {
          SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, 0, 0, SPIF_SENDCHANGE);
        }
      }

      GlobalServiceProvider.Add<IVideoThumbBlacklist>(new MediaPortal.Video.Database.VideoThumbBlacklistDBImpl());
      Utils.CheckThumbExtractorVersion();
    }
    catch (Exception ex)
    {
      Log.Error("MediaPortalApp: Error setting date and time properties - {0}", ex.Message);
    }
    if (_outdatedSkinName != null || PluginManager.IncompatiblePluginAssemblies.Count > 0 || PluginManager.IncompatiblePlugins.Count > 0)
    {
      GUIWindowManager.SendThreadCallback(ShowStartupWarningDialogs, 0, 0, null);
    }
    Log.Debug("Main: Auto play start listening");
    AutoPlay.StartListening();
    Log.Info("Main: Initializing volume handler");
#pragma warning disable 168
    VolumeHandler vh = VolumeHandler.Instance;
#pragma warning restore 168
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="param1"></param>
  /// <param name="param2"></param>
  /// <param name="data"></param>
  /// <returns></returns>
  private int ShowStartupWarningDialogs(int param1, int param2, object data)
  {
    // If skin is outdated it may not have a skin file for this dialog but user may choose to use it anyway
    // So show incompatible plugins dialog first (possibly using default skin)
    if (PluginManager.IncompatiblePluginAssemblies.Count > 0 || PluginManager.IncompatiblePlugins.Count > 0)
    {
      var dlg = (GUIDialogIncompatiblePlugins)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_INCOMPATIBLE_PLUGINS);
      dlg.DoModal(GUIWindowManager.ActiveWindow);
    }

    if (_outdatedSkinName != null)
    {
      var dlg = (GUIDialogOldSkin)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OLD_SKIN);
      dlg.UserSkin = _outdatedSkinName;
      dlg.DoModal(GUIWindowManager.ActiveWindow);
    }

    return 0;
  }


  /// <summary>
  /// Load string_xx.xml based on config
  /// </summary>
  private static void LoadLanguageString()
  {
    string mylang;
    try
    {
      using (Settings xmlreader = new MPSettings())
      {
        mylang = xmlreader.GetValueAsString("gui", "language", "English");
      }
    }
    catch
    {
      Log.Warn("Load language file failed, fall back to \"English\"");
      mylang = "English";
    }
    Log.Info("Loading selected language: " + mylang);
    try
    {
      GUILocalizeStrings.Load(mylang);
    }
    catch (Exception ex)
    {
      MessageBox.Show(
        String.Format("Failed to load your language! Aborting startup...\n\n{0}\nstack:{1}", ex.Message, ex.StackTrace),
        "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
      Application.Exit();
    }
  }


  /// <summary>
  /// saves last active module.
  /// </summary>
  private void SaveLastActiveModule()
  {
    // persist the currently selected module to XML for later use.
    Log.Debug("Main: SaveLastActiveModule - enabled {0}", _showLastActiveModule);
    bool currentmodulefullscreen = Currentmodulefullscreen();
    string currentmodulefullscreenstate = GUIPropertyManager.GetProperty("#currentmodulefullscreenstate");
    string currentmoduleid = GUIPropertyManager.GetProperty("#currentmoduleid");
    if (_showLastActiveModule && !Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
    {
      using (Settings xmlreader = new MPSettings())
      {
        if (currentmodulefullscreen)
        {
          currentmoduleid = Convert.ToString(GUIWindowManager.GetPreviousActiveWindow());
        }


        if (!currentmodulefullscreen && currentmodulefullscreenstate == "True")
        {
          currentmodulefullscreen = true;
        }
        if (currentmoduleid.Length == 0)
        {
          currentmoduleid = "0";
        }

        string section;
        switch (GUIWindowManager.ActiveWindow)
        {
          case (int)GUIWindow.Window.WINDOW_PICTURES:
            {
              section = "pictures";
              break;
            }
          case (int)GUIWindow.Window.WINDOW_MUSIC:
            {
              section = "music";
              break;
            }
          case (int)GUIWindow.Window.WINDOW_VIDEOS:
            {
              section = "movies";
              break;
            }
          default:
            {
              section = "";
              break;
            }
        }

        bool rememberLastFolder = xmlreader.GetValueAsBool(section, "rememberlastfolder", false);
        string lastFolder = xmlreader.GetValueAsString(section, "lastfolder", "");

        var virtualDir = new VirtualDirectory();
        virtualDir.LoadSettings(section);
        int pincode;
        bool lastFolderPinProtected = virtualDir.IsProtectedShare(lastFolder, out pincode);
        if (rememberLastFolder && lastFolderPinProtected)
        {
          lastFolder = "root";
          xmlreader.SetValue(section, "lastfolder", lastFolder);
          Log.Debug("Main: reverting to root folder, pin protected folder was open, SaveLastFolder {0}", lastFolder);
        }

        xmlreader.SetValue("general", "lastactivemodule", currentmoduleid);
        xmlreader.SetValueAsBool("general", "lastactivemodulefullscreen", currentmodulefullscreen);
        Log.Debug("Main: SaveLastActiveModule - module {0}", currentmoduleid);
        Log.Debug("Main: SaveLastActiveModule - fullscreen {0}", currentmodulefullscreen);
      }
    }
  }


  /// <summary>
  /// OnExit() Gets called just b4 application stops
  /// </summary>
  protected override void OnExit()
  {
    SaveLastActiveModule();

    Log.Info("Main: Exiting");

    if (_usbuirtdevice != null)
    {
      _usbuirtdevice.Close();
    }
    if (_serialuirdevice != null)
    {
      _serialuirdevice.Close();
    }
    if (_redeyedevice != null)
    {
      _redeyedevice.Close();
    }
#if AUTOUPDATE
    StopUpdater();
#endif
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
    // stop any file playback
    g_Player.Stop();
    // tell window manager that application is closing
    // this gives the windows the chance to do some cleanup
    InputDevices.Stop();
    AutoPlay.StopListening();
    PluginManager.Stop();
    GUIWaitCursor.Dispose();
    GUIFontManager.ReleaseUnmanagedResources();
    GUIFontManager.Dispose();
    GUITextureManager.Dispose();
    GUIWindowManager.Clear();
    GUILocalizeStrings.Dispose();
    TexturePacker.Cleanup();
    VolumeHandler.Dispose();
    if (_isWinScreenSaverInUse)
    {
      SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, 1, 0, SPIF_SENDCHANGE);
    }
  }


  /// <summary>
  /// The device has been created.  Resources that are not lost on
  /// Reset() can be created here -- resources in Pool.Managed,
  /// Pool.Scratch, or Pool.SystemMemory.  Image surfaces created via
  /// CreateImageSurface are never lost and can be created here.  Vertex
  /// shaders and pixel shaders can also be created here as they are not
  /// lost on Reset().
  /// </summary>
  protected override void InitializeDeviceObjects()
  {
    GUIWindowManager.Clear();
    GUIWaitCursor.Dispose();
    GUITextureManager.Dispose();

    // Loading keymap.xml
    Log.Info("Startup: Load keymap.xml");
    UpdateSplashScreenMessage(GUILocalizeStrings.Get(65));
    ActionTranslator.Load();
    GUIGraphicsContext.ActiveForm = Handle;

    // Caching Graphics
    Log.Info("Startup: Caching Graphics");
    UpdateSplashScreenMessage(GUILocalizeStrings.Get(67));
    try
    {
      GUITextureManager.Init();
    }
    catch (Exception exs)
    {
      MessageBox.Show(String.Format("Failed to load your skin! Aborting startup...\n\n{0}", exs.Message), "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
      Close();
    }
    Utils.FileExistsInCache(Config.GetSubFolder(Config.Dir.Skin, "") + "dummy.png");
    Utils.FileExistsInCache(Config.GetSubFolder(Config.Dir.Thumbs, "") + "dummy.png");
    Utils.FileExistsInCache(Thumbs.Videos + "\\dummy.png");
    Utils.FileExistsInCache(Thumbs.MusicFolder + "\\dummy.png");

    // Loading Skin
    UpdateSplashScreenMessage(String.Format(GUILocalizeStrings.Get(69), GUIGraphicsContext.SkinName + " - " + GUIThemeManager.CurrentTheme));
    GUIControlFactory.LoadReferences(GUIGraphicsContext.GetThemedSkinFile(@"\references.xml"));
    if (Windowed)
    {
      Log.Debug("Startup: Resizing form to Skin Dimensions");
      var borderWidth = Width - ClientSize.Width;
      var borderHeight = Height - ClientSize.Height;
      Size = new Size(GUIGraphicsContext.SkinSize.Width + borderWidth, GUIGraphicsContext.SkinSize.Height + borderHeight + SystemInformation.MenuHeight);
      ClientSize = new Size(GUIGraphicsContext.SkinSize.Width, GUIGraphicsContext.SkinSize.Height);
    }
    else
    {
      Log.Debug("Startup: Resizing form to Screen Dimensions");
      Size = new Size(GUIGraphicsContext.currentScreen.Bounds.Right, GUIGraphicsContext.currentScreen.Bounds.Bottom);
      ClientSize = Size;
    }
    UpdatePresentParams(Windowed, true);

    // Loading Fonts
    Log.Info("Startup: Loading Fonts");
    UpdateSplashScreenMessage(GUILocalizeStrings.Get(68));
    GUIFontManager.LoadFonts(GUIGraphicsContext.GetThemedSkinFile(@"\fonts.xml"));
    GUIFontManager.InitializeDeviceObjects();

    // Loading window plugins
    Log.Info("Startup: Loading Plugins");
    UpdateSplashScreenMessage(GUILocalizeStrings.Get(70));
    if (!string.IsNullOrEmpty(_safePluginsList))
    {
      PluginManager.LoadWhiteList(_safePluginsList);
    }
    PluginManager.LoadWindowPlugins();
    PluginManager.CheckExternalPlayersCompatibility();

    // Initialize window manager
    UpdateSplashScreenMessage(GUILocalizeStrings.Get(71));
    Log.Info("Startup: Initialize Window Manager...");
    GUIGraphicsContext.Load();
    GUIWindowManager.Initialize();

    using (Settings xmlreader = new MPSettings())
    {
      UseLongDateFormat = xmlreader.GetValueAsBool("home", "LongTimeFormat", false);
      _startWithBasicHome = xmlreader.GetValueAsBool("gui", "startbasichome", false);
      _useOnlyOneHome = xmlreader.GetValueAsBool("gui", "useonlyonehome", false);
    }

    Log.Info("Startup: Starting Window Manager");
    GUIWindowManager.PreInit();
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;

    Log.Info("Startup: Activating Window Manager");
    if ((_startWithBasicHome) && (File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\basichome.xml"))))
    {
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SECOND_HOME);
    }
    else
    {
      GUIWindowManager.ActivateWindow(GUIWindowManager.ActiveWindow);
    }
   
    // setting D3D9 helper variables
    if (GUIGraphicsContext.DX9Device != null)
    {
      _anisotropy = GUIGraphicsContext.DX9Device.DeviceCaps.MaxAnisotropy;
      _supportsFiltering = Manager.CheckDeviceFormat(GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal,
                                                     GUIGraphicsContext.DX9Device.DeviceCaps.DeviceType,
                                                     GUIGraphicsContext.DX9Device.DisplayMode.Format,
                                                     Usage.RenderTarget | Usage.QueryFilter, ResourceType.Textures,
                                                     Format.A8R8G8B8);
      _supportsAlphaBlend = Manager.CheckDeviceFormat(GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal,
                                                      GUIGraphicsContext.DX9Device.DeviceCaps.DeviceType,
                                                      GUIGraphicsContext.DX9Device.DisplayMode.Format,
                                                      Usage.RenderTarget | Usage.QueryPostPixelShaderBlending,
                                                      ResourceType.Surface,
                                                      Format.A8R8G8B8);
      Log.Info("Main: DX9 size: {0}x{1}", GUIGraphicsContext.SkinSize.Width, GUIGraphicsContext.SkinSize.Height);
      Log.Info("Main: Video memory left: {0} MB", (uint)GUIGraphicsContext.DX9Device.AvailableTextureMemory / 1048576);
    }

    // ReSharper disable ObjectCreationAsStatement
    new GUILayerRenderer();
    // ReSharper restore ObjectCreationAsStatement
    WorkingSet.Minimize();
  }


  /// <summary>
  /// Updates the splash screen to display the given string. 
  /// This method checks whether the splash screen exists.
  /// </summary>
  /// <param name="aSplashLine"></param>
  private static void UpdateSplashScreenMessage(string aSplashLine)
  {
    try
    {
      if (_splashScreen != null)
      {
        _splashScreen.SetInformation(aSplashLine);
      }
    }
    catch (Exception ex)
    {
      Log.Error("Main: Could not update splashscreen - {0}", ex.Message);
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  protected override void OnDeviceLost(object sender, EventArgs e)
  {
    Log.Warn("Main: ***** OnDeviceLost *****");
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
    base.OnDeviceLost(sender, e);
  }


  /// <summary>
  /// The device exists, but may have just been Reset().  Resources in
  /// Pool.Managed and any other device state that persists during
  /// rendering should be set here.  Render states, matrices, textures,
  /// etc., that don't change during rendering can be set once here to
  /// avoid redundant state setting during Render() or FrameMove().
  /// </summary>
  protected override void OnDeviceReset(Object sender, EventArgs e)
  {
    // Only perform the device reset if we're not shutting down MediaPortal.
    if (GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.STOPPING)
    {
      Log.Info("Main: Resetting DX9 device");

      int activeWin = GUIWindowManager.ActiveWindow;
      if (activeWin == 0 && !GUIWindowManager.HasPreviousWindow())
      {
        if (_startWithBasicHome && File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\basichome.xml")))
        {
          activeWin = (int)GUIWindow.Window.WINDOW_SECOND_HOME;
        }
      }

      if (GUIGraphicsContext.DX9ExRealDeviceLost)
      {
        activeWin = (int)GUIWindow.Window.WINDOW_HOME;
      }
        // Device lost must be prioritized over this one!
      else if (Currentmodulefullscreen())
      {
        activeWin = GUIWindowManager.GetPreviousActiveWindow();
        GUIWindowManager.ShowPreviousWindow();
      }

      GUIWindowManager.UnRoute();
      // avoid that there is an active Window when GUIWindowManager.ActivateWindow(activeWin); is called
      Log.Info("Main: UnRoute - done");

      GUITextureManager.Dispose();
      GUIFontManager.Dispose();

      GUIGraphicsContext.DX9Device.EvictManagedResources();
      GUIWaitCursor.Dispose();
      GUIGraphicsContext.Load();
      GUIFontManager.LoadFonts(GUIGraphicsContext.GetThemedSkinFile(@"\fonts.xml"));
      GUIFontManager.InitializeDeviceObjects();

      if (GUIGraphicsContext.DX9Device != null)
      {
        GUIWindowManager.PreInit();
        GUIWindowManager.ActivateWindow(activeWin);
        GUIWindowManager.OnDeviceRestored();
      }
      // Must set the FVF after reset
      GUIFontManager.SetDevice();

      GUIGraphicsContext.DX9ExRealDeviceLost = false;
      Log.Info("Main: Resetting DX9 device done");
    }
  }

  #endregion


  #region Render()

  /// <summary>
  /// 
  /// </summary>
  /// <param name="timePassed"></param>
  protected override void Render(float timePassed)
  {
    if (!_suspended && !_isRendering && GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.LOST && GUIGraphicsContext.DX9Device != null)
    {
      if (GUIGraphicsContext.InVmr9Render)
      {
        Log.Error("Main: MediaPortal.Render() called while VMR9 render - {0} / {1}", GUIGraphicsContext.Vmr9Active, GUIGraphicsContext.Vmr9FPS);
        return;
      }
      if (GUIGraphicsContext.Vmr9Active)
      {
        Log.Error("Main: MediaPortal.Render() called while VMR9 active");
        return;
      }

      // render frame
      try
      {
        _isRendering = true;
        Frames++;
        GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
        GUIGraphicsContext.DX9Device.BeginScene();
        CreateStateBlock();
        GUIGraphicsContext.SetScalingResolution(0, 0, false);
        GUILayerManager.Render(timePassed);
        RenderStats();
        GUIFontManager.Present();
        GUIGraphicsContext.DX9Device.EndScene();

        // show the frame on the primary surface.
        try
        {
          GUIGraphicsContext.DX9Device.Present(); // SLOW
        }
        catch (DeviceLostException ex)
        {
          Log.Error("Main: Device lost - {0}", ex.ToString());
          if (!RefreshRateChanger.RefreshRateChangePending)
          {
            g_Player.Stop();
          }
          GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
        }
      }
      catch (DirectXException dex)
      {
        switch (dex.ErrorCode)
        {
          case D3DERR_INVALIDCALL:
            _errorCounter++;
            int currentScreenNr = GUIGraphicsContext.currentScreenNumber;
            if ((currentScreenNr > -1) && (Manager.Adapters.Count > currentScreenNr))
            {
              double currentRR = Manager.Adapters[currentScreenNr].CurrentDisplayMode.RefreshRate;
              if (currentRR > 0 && _errorCounter > (5 * currentRR))
              {
                _errorCounter = 0; //reset counter
                Log.Info("Main: D3DERR_INVALIDCALL - {0}", dex.ToString());
                GUIGraphicsContext.DX9ExRealDeviceLost = true;
                GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
              }
            }
            break;
  
          case D3DERR_DEVICEHUNG:
          case D3DERR_DEVICEREMOVED:
            Log.Info("Main: GPU_HUNG - {0}", dex.ToString());
            GUIGraphicsContext.DX9ExRealDeviceLost = true;
            if (!RefreshRateChanger.RefreshRateChangePending)
            {
              g_Player.Stop();
            }
            GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
            break;

          default:
            Log.Error(dex);
            break;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      finally
      {
        _isRendering = false;
      }
    }
  }

  #endregion

  #region OnProcess()

  /// <summary>
  /// 
  /// </summary>
  protected override void OnProcess()
  {
    // Set the date & time
    if (DateTime.Now.Second != _updateTimer.Second)
    {
      _updateTimer = DateTime.Now;
      GUIPropertyManager.SetProperty("#date", GetDate());
      GUIPropertyManager.SetProperty("#time", GetTime());
    }
#if AUTOUPDATE
    CheckForNewUpdate();
#endif
    g_Player.Process();
    // update playing status
    if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST)
    {
      RecoverDevice();
    }

    if (g_Player.Playing)
    {
      _playingState = true;
      if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO)
      {
        GUIGraphicsContext.IsFullScreenVideo = true;
      }
      GUIGraphicsContext.IsPlaying = true;
      GUIGraphicsContext.IsPlayingVideo = (g_Player.IsVideo || g_Player.IsTV);
      if (g_Player.Paused)
      {
        GUIPropertyManager.SetProperty("#playlogo", "logo_pause.png");
      }
      else if (g_Player.Speed > 1)
      {
        GUIPropertyManager.SetProperty("#playlogo", "logo_fastforward.png");
      }
      else if (g_Player.Speed < 1)
      {
        GUIPropertyManager.SetProperty("#playlogo", "logo_rewind.png");
      }
      else if (g_Player.Playing)
      {
        GUIPropertyManager.SetProperty("#playlogo", "logo_play.png");
      }
      if (g_Player.IsTV && !g_Player.IsTVRecording)
      {
        GUIPropertyManager.SetProperty("#currentplaytime", GUIPropertyManager.GetProperty("#TV.Record.current"));
        GUIPropertyManager.SetProperty("#shortcurrentplaytime", GUIPropertyManager.GetProperty("#TV.Record.current"));
      }
      else
      {
        GUIPropertyManager.SetProperty("#currentplaytime", Utils.SecondsToHMSString((int)g_Player.CurrentPosition));
        GUIPropertyManager.SetProperty("#currentremaining", Utils.SecondsToHMSString((int)(g_Player.Duration - g_Player.CurrentPosition)));
        GUIPropertyManager.SetProperty("#shortcurrentremaining", Utils.SecondsToShortHMSString((int)(g_Player.Duration - g_Player.CurrentPosition)));
        GUIPropertyManager.SetProperty("#shortcurrentplaytime", Utils.SecondsToShortHMSString((int)g_Player.CurrentPosition));
      }
      if (g_Player.Duration > 0)
      {
        GUIPropertyManager.SetProperty("#duration", Utils.SecondsToHMSString((int)g_Player.Duration));
        GUIPropertyManager.SetProperty("#shortduration", Utils.SecondsToShortHMSString((int)g_Player.Duration));
        var fPercentage = (float)(100.0d * g_Player.CurrentPosition / g_Player.Duration);
        GUIPropertyManager.SetProperty("#percentage", fPercentage.ToString(CultureInfo.InvariantCulture));
      }
      else
      {
        GUIPropertyManager.SetProperty("#duration", string.Empty);
        GUIPropertyManager.SetProperty("#shortduration", string.Empty);
        GUIPropertyManager.SetProperty("#percentage", "0.0");
      }
      GUIPropertyManager.SetProperty("#playspeed", g_Player.Speed.ToString(CultureInfo.InvariantCulture));
    }
    else
    {
      GUIGraphicsContext.IsPlaying = false;
      if (_playingState)
      {
        GUIPropertyManager.RemovePlayerProperties();
        _playingState = false;
      }
    }
  }

  #endregion

  #region FrameMove()
  
  /// <summary>
  /// 
  /// </summary>
  protected override void FrameMove()
  {
    // we are suspended/hibernated
    if (_suspended)
    {
      return;
    }
#if !DEBUG
    try
#endif
    {
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
      {
        Log.Info("Main: Stopping FrameMove");
        Close();
        return;
      }

      try
      {
        GUIWindowManager.DispatchThreadMessages();
        GUIWindowManager.ProcessWindows();
      }
      catch (FileNotFoundException ex)
      {
        Log.Error(ex);
        MessageBox.Show("File not found:" + ex.FileName, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
        Close();
      }
      if (_useScreenSaver)
      {
        if ((GUIGraphicsContext.IsFullScreenVideo && g_Player.Paused == false) ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
        {
          GUIGraphicsContext.ResetLastActivity();
        }
        if (!GUIGraphicsContext.BlankScreen && !Windowed)
        {
          TimeSpan ts = DateTime.Now - GUIGraphicsContext.LastActivity;
          if (ts.TotalSeconds >= _timeScreenSaver)
          {
            if (_useIdleblankScreen)
            {
              if (!GUIGraphicsContext.BlankScreen)
              {
                Log.Debug("Main: Idle timer is blanking the screen after {0} seconds of inactivity", ts.TotalSeconds.ToString("n0"));
              }
              GUIGraphicsContext.BlankScreen = true;
            }
            else
            {
              // Slower rendering will have an impact on scrolling labels or list items
              // As long as we're e.g. listening to music on "Playing Now" screen
              // we might not want to slow things down here.
              // This feature is mainly intended to save energy on idle 24/7 rigs.
              if (GUIWindowManager.ActiveWindow != (int) GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW)
              {
                if (!GUIGraphicsContext.SaveRenderCycles)
                {
                  Log.Debug("Main: Idle timer is entering power save mode after {0} seconds of inactivity", ts.TotalSeconds.ToString("n0"));
                }
                GUIGraphicsContext.SaveRenderCycles = true;
              }
            }
          }
        }
      }
    }
#if !DEBUG
    catch (Exception ex)
    {
      Log.Error(ex);
    }
#endif
  }

  #endregion

  #region Handle messages, keypresses, mouse moves etc

  /// <summary>
  /// 
  /// </summary>
  /// <param name="action"></param>
  private void OnAction(Action action)
  {
    try
    {
      // hack/fix for lastactivemodulefullscreen
      // when recovering from hibernation/standby after closing with remote control somehow a F9 (keycode 120) onkeydown event is thrown from outside
      // we are currently filtering it away.
      // sometimes more than one F9 keydown event fired.
      // if these events are not filtered away the F9 context menu is shown on the restored/shown module.
      if ((action.wID == Action.ActionType.ACTION_CONTEXT_MENU || _suspended) && (_showLastActiveModule))
      {
        //Log.Info("ACTION_CONTEXT_MENU, ignored = {0}, suspended = {1}", ignoreContextMenuAction, _suspended);      
        if (_ignoreContextMenuAction)
        {
          _ignoreContextMenuAction = false;
          _lastContextMenuAction = DateTime.Now;
          return;
        }
        
        if (_lastContextMenuAction != DateTime.MaxValue)
        {
          TimeSpan ts = _lastContextMenuAction - DateTime.Now;
          if (ts.TotalMilliseconds > -100)
          {
            _ignoreContextMenuAction = false;
            _lastContextMenuAction = DateTime.Now;
            return;
          }
        }
        _lastContextMenuAction = DateTime.Now;
      }

      GUIWindow window;
      if (action.IsUserAction())
      {
        GUIGraphicsContext.ResetLastActivity();
      }
      switch (action.wID)
      {
        // record current tv program
        case Action.ActionType.ACTION_RECORD:
          if ((GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindowEx) &&
               GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_TVGUIDE) &&
              (GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_DIALOG_TVGUIDE))
          {
            GUIWindow tvHome = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
            if (tvHome != null && tvHome.GetID != GUIWindowManager.ActiveWindow)
            {
              tvHome.OnAction(action);
              return;
            }
          }
          break;

        // TV: zap to previous channel
        case Action.ActionType.ACTION_PREV_CHANNEL:
          if (!GUIWindowManager.IsRouted)
          {
            window = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
            window.OnAction(action);
            return;
          }
          break;

        // TV: zap to next channel
        case Action.ActionType.ACTION_NEXT_CHANNEL:
          if (!GUIWindowManager.IsRouted)
          {
            window = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
            window.OnAction(action);
            return;
          }
          break;

        // TV: zap to last channel viewed
        case Action.ActionType.ACTION_LAST_VIEWED_CHANNEL: // mPod
          if (!GUIWindowManager.IsRouted)
          {
            window = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
            window.OnAction(action);
            return;
          }
          break;

        // toggle between directx windowed and exclusive mode
        case Action.ActionType.ACTION_TOGGLE_WINDOWED_FULLSCREEN:
          ToggleFullscreen();
          return;

        // mute or unmute audio
        case Action.ActionType.ACTION_VOLUME_MUTE:
          VolumeHandler.Instance.IsMuted = !VolumeHandler.Instance.IsMuted;
          break;

        // decrease volume 
        case Action.ActionType.ACTION_VOLUME_DOWN:
          VolumeHandler.Instance.Volume = VolumeHandler.Instance.Previous;
          break;

        // increase volume 
        case Action.ActionType.ACTION_VOLUME_UP:
          VolumeHandler.Instance.Volume = VolumeHandler.Instance.Next;
          break;

        // toggle live tv in background
        case Action.ActionType.ACTION_BACKGROUND_TOGGLE:
          // show livetv or video as background instead of the static GUI background
          // toggle livetv/video in background on/off
          if (GUIGraphicsContext.ShowBackground)
          {
            Log.Info("Main: Using live TV as background");
            // if on, but we're not playing any video or watching tv
            if (GUIGraphicsContext.Vmr9Active)
            {
              GUIGraphicsContext.ShowBackground = false;
            }
            else
            {
              // show warning message
              var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, 0, 0, 0, 0, 0, 0) {Param1 = 727, Param2 = 728, Param3 = 729};
              GUIWindowManager.SendMessage(msg);
              return;
            }
          }
          else
          {
            Log.Info("Main: Using GUI as background");
            GUIGraphicsContext.ShowBackground = true;
            //GUIGraphicsContext.Overlay = true;
          }
          return;

        // switch between several home windows
        case Action.ActionType.ACTION_SWITCH_HOME:
          GUIWindow.Window newHome = _startWithBasicHome
                                       ? GUIWindow.Window.WINDOW_SECOND_HOME
                                       : GUIWindow.Window.WINDOW_HOME;
          // do we prefer to use only one home screen?
          if (_useOnlyOneHome)
          {
            // skip if we are already in there
            if (GUIWindowManager.ActiveWindow == (int)newHome)
            {
              return;
            }
          }
          // we like both 
          else
          {
            // if already in one home switch to the other
            switch (GUIWindowManager.ActiveWindow)
            {
              case (int)GUIWindow.Window.WINDOW_HOME:
                newHome = GUIWindow.Window.WINDOW_SECOND_HOME;
                break;
              case (int)GUIWindow.Window.WINDOW_SECOND_HOME:
                newHome = GUIWindow.Window.WINDOW_HOME;
                break;
            }
          }
          var homeMsg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, (int)newHome, 0, null);
          GUIWindowManager.SendThreadMessage(homeMsg);
          return;

        case Action.ActionType.ACTION_MPRESTORE:
          Log.Info("Main: Restore MP by action");
          RestoreFromTray();
          if ((g_Player.IsVideo || g_Player.IsTV || g_Player.IsDVD) && Volume > 0)
          {
            g_Player.Volume = Volume;
            g_Player.ContinueGraph();
            if (g_Player.Paused && !GUIGraphicsContext.IsVMR9Exclusive)
            {
              g_Player.Pause();
            }
          }
          return;

        // reboot pc
        case Action.ActionType.ACTION_POWER_OFF:
        case Action.ActionType.ACTION_SUSPEND:
        case Action.ActionType.ACTION_HIBERNATE:
        case Action.ActionType.ACTION_REBOOT:
          // reboot
          Log.Info("Main: Reboot requested");
          bool okToChangePowermode = (action.fAmount1 == 1);

          if (!okToChangePowermode)
          {
            okToChangePowermode = PromptUserBeforeChangingPowermode(action);
          }

          if (okToChangePowermode)
          {
            switch (action.wID)
            {
              case Action.ActionType.ACTION_REBOOT:
                _restartOptions = RestartOptions.Reboot;
                _useRestartOptions = true;
                GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
                break;

              case Action.ActionType.ACTION_POWER_OFF:
                _restartOptions = RestartOptions.PowerOff;
                _useRestartOptions = true;
                GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
                ShuttingDown = true;
                break;

              case Action.ActionType.ACTION_SUSPEND:
                if (IsSuspendOrHibernationAllowed())
                {
                  _restartOptions = RestartOptions.Suspend;
                  Utils.SuspendSystem(false);
                }
                else
                {
                  Log.Info("Main: SUSPEND ignored since suspend graceperiod of {0} sec. is violated.", _suspendGracePeriodSec); 
                }
                break;

              case Action.ActionType.ACTION_HIBERNATE:
                if (IsSuspendOrHibernationAllowed())
                {
                  _restartOptions = RestartOptions.Hibernate;
                  Utils.HibernateSystem(false);
                }
                else
                {
                  Log.Info("Main: HIBERNATE ignored since hibernate graceperiod of {0} sec. is violated.", _suspendGracePeriodSec);
                }
                break;
            }
          }
          return;

        // eject cd
        case Action.ActionType.ACTION_EJECTCD:
          Utils.EjectCDROM();
          return;

        // shutdown pc
        case Action.ActionType.ACTION_SHUTDOWN:
          Log.Info("Main: Shutdown dialog");
          var dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
          if (dlg != null)
          {
            dlg.Reset();
            dlg.SetHeading(GUILocalizeStrings.Get(498)); //Menu
            dlg.AddLocalizedString(1057); //Exit MediaPortal
            dlg.AddLocalizedString(1058); //Restart MediaPortal
            dlg.AddLocalizedString(1032); //Suspend
            dlg.AddLocalizedString(1049); //Hibernate
            dlg.AddLocalizedString(1031); //Reboot
            dlg.AddLocalizedString(1030); //PowerOff
            dlg.DoModal(GUIWindowManager.ActiveWindow);

            if (dlg.SelectedId >= 0)
            {
              switch (dlg.SelectedId)
              {
                case 1057:
                  ExitMP();
                  return;

                case 1058:
                  GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
                  Utils.RestartMePo();
                  break;

                case 1030:
                  _restartOptions = RestartOptions.PowerOff;
                  _useRestartOptions = true;
                  GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
                  ShuttingDown = true;
                  break;

                case 1031:
                  _restartOptions = RestartOptions.Reboot;
                  _useRestartOptions = true;
                  GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
                  ShuttingDown = true;
                  break;

                case 1032:
                  _restartOptions = RestartOptions.Suspend;
                  Utils.SuspendSystem(false);
                  break;

                case 1049:
                  _restartOptions = RestartOptions.Hibernate;
                  Utils.HibernateSystem(false);
                  break;
              }
            }
            else
            {
              GUIWindow win = GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_HOME);
              if (win != null)
              {
                win.OnAction(new Action(Action.ActionType.ACTION_MOVE_LEFT, 0, 0));
              }
              return;
            }
          }
          break;

        // exit Mediaportal
        case Action.ActionType.ACTION_EXIT:
          ExitMP();
          return;

        //stop radio
        case Action.ActionType.ACTION_STOP:
          break;

        // Take Screen shot
        case Action.ActionType.ACTION_TAKE_SCREENSHOT:
          try
          {
            string directory = string.Format("{0}\\MediaPortal Screenshots\\{1:0000}-{2:00}-{3:00}",
                                             Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                                             DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            if (!Directory.Exists(directory))
            {
              Log.Info("Main: Taking screenshot - Creating directory: {0}", directory);
              Directory.CreateDirectory(directory);
            }

            string fileName = string.Format("{0}\\{1:00}-{2:00}-{3:00}", directory, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            Log.Info("Main: Taking screenshot - Target: {0}.png", fileName);
            Surface backbuffer = GUIGraphicsContext.DX9Device.GetBackBuffer(0, 0, BackBufferType.Mono);
            SurfaceLoader.Save(fileName + ".png", ImageFileFormat.Png, backbuffer);
            backbuffer.Dispose();
            Log.Info("Main: Taking screenshot done");
          }
          catch (Exception ex)
          {
            Log.Info("Main: Error taking screenshot: {0}", ex.Message);
          }
          break;

        case Action.ActionType.ACTION_SHOW_GUI:
          // can we handle the switch to fullscreen?
          if (!GUIGraphicsContext.IsFullScreenVideo && g_Player.ShowFullScreenWindow())
          {
            return;
          }
          break;
      }
      if (g_Player.Playing)
      {
        switch (action.wID)
        {
          // show DVD menu
          case Action.ActionType.ACTION_DVD_MENU:
            if (g_Player.IsDVD)
            {
              g_Player.OnAction(action);
              return;
            }
            break;

          // DVD: goto previous chapter
          // play previous item from playlist;
          case Action.ActionType.ACTION_PREV_ITEM:
          case Action.ActionType.ACTION_PREV_CHAPTER:
            if (g_Player.IsDVD || g_Player.HasChapters)
            {
              action = new Action(Action.ActionType.ACTION_PREV_CHAPTER, 0, 0);
              g_Player.OnAction(action);
              return;
            }

            if (!ActionTranslator.HasKeyMapped(GUIWindowManager.ActiveWindowEx, action.m_key))
            {
              PlaylistPlayer.PlayPrevious();
            }
            break;

          // play next item from playlist;
          // DVD: goto next chapter
          case Action.ActionType.ACTION_NEXT_CHAPTER:
          case Action.ActionType.ACTION_NEXT_ITEM:
            if (g_Player.IsDVD || g_Player.HasChapters)
            {
              action = new Action(Action.ActionType.ACTION_NEXT_CHAPTER, 0, 0);
              g_Player.OnAction(action);
              return;
            }

            if (!ActionTranslator.HasKeyMapped(GUIWindowManager.ActiveWindowEx, action.m_key))
            {
              PlaylistPlayer.PlayNext();
            }
            break;

          // stop playback
          case Action.ActionType.ACTION_STOP:
            // When MyPictures Plugin shows the pictures we want to stop the slide show only, not the player
            if ((GUIWindow.Window)(Enum.Parse(typeof (GUIWindow.Window), GUIWindowManager.ActiveWindow.ToString(CultureInfo.InvariantCulture))) == GUIWindow.Window.WINDOW_SLIDESHOW)
            {
              break;
            }

            if (!g_Player.IsTV || !GUIGraphicsContext.IsFullScreenVideo)
            {
              Log.Info("Main: Stopping media");
              g_Player.Stop();
              return;
            }
            break;

          // Jump to Music Now Playing
          case Action.ActionType.ACTION_JUMP_MUSIC_NOW_PLAYING:
            if (g_Player.IsMusic && GUIWindowManager.ActiveWindow != (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW)
            {
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW);
            }
            break;

          // play music
          // resume playback
          case Action.ActionType.ACTION_PLAY:
          case Action.ActionType.ACTION_MUSIC_PLAY:
            // Don't start playing from the beginning if we press play to return to normal speed
            if (g_Player.IsMusic && g_Player.Speed != 1 &&
                (GUIWindowManager.ActiveWindow != (int) GUIWindow.Window.WINDOW_MUSIC_FILES &&
                 GUIWindowManager.ActiveWindow != (int) GUIWindow.Window.WINDOW_MUSIC_GENRE))
            {
              g_Player.Speed = 1;
              return;
            }

            g_Player.StepNow();
            g_Player.Speed = 1;

            if (g_Player.Paused)
            {
              g_Player.Pause();
            }
            break;

          // pause (or resume playback)
          case Action.ActionType.ACTION_PAUSE:
            g_Player.Pause();
            break;

          // fast forward...
          case Action.ActionType.ACTION_FORWARD:
          case Action.ActionType.ACTION_MUSIC_FORWARD:
            if (g_Player.Paused)
            {
              g_Player.Pause();
            }
            g_Player.Speed = Utils.GetNextForwardSpeed(g_Player.Speed);
            break;
 
          // fast rewind...
          case Action.ActionType.ACTION_REWIND:
          case Action.ActionType.ACTION_MUSIC_REWIND:
            if (g_Player.Paused)
            {
              g_Player.Pause();
            }
            g_Player.Speed = Utils.GetNextRewindSpeed(g_Player.Speed);
            break;
         }
      }
      GUIWindowManager.OnAction(action);
    }
    catch (FileNotFoundException ex)
    {
      Log.Error(ex);
      MessageBox.Show("File not found:" + ex.FileName, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
      Close();
    }
    catch (Exception ex)
    {
      Log.Error(ex);
      Log.Error("Exception: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
#if !DEBUG
      throw new Exception("exception occurred", ex);
#endif
    }
  }


  /// <summary>
  /// 
  /// </summary>
  private void ExitMP()
  {
    Log.Info("Main: Exit requested");
    // is the minimize on gui option set?  If so, minimize to tray...
    if (MinimizeOnGuiExit && !ShuttingDown)
    {
      if (WindowState != FormWindowState.Minimized)
      {
        Log.Info("Main: Minimizing to tray on GUI exit and restoring taskbar");
      }
      WindowState = FormWindowState.Minimized;
      Hide();
      if (AutoHideTaskbar)
      {
        // only re-show the task bar if MP is the one that has hidden it.
        HideTaskBar(false);
      }
      if (g_Player.IsVideo || g_Player.IsTV || g_Player.IsDVD)
      {
        if (g_Player.Volume > 0)
        {
          Volume = g_Player.Volume;
          g_Player.Volume = 0;
        }
        if (g_Player.Paused == false && !GUIGraphicsContext.IsVMR9Exclusive)
        {
          g_Player.Pause();
        }
      }
      return;
    }
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="action"></param>
  /// <returns></returns>
  private static bool PromptUserBeforeChangingPowermode(Action action)
  {
    var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, 0);
    switch (action.wID)
    {
      case Action.ActionType.ACTION_REBOOT:
        msg.Param1 = 630;
        break;

      case Action.ActionType.ACTION_POWER_OFF:
        msg.Param1 = 1600;
        break;

      case Action.ActionType.ACTION_SUSPEND:
        msg.Param1 = 1601;
        break;

      case Action.ActionType.ACTION_HIBERNATE:
        msg.Param1 = 1602;
        break;
    }
    msg.Param2 = 0;
    msg.Param3 = 0;
    GUIWindowManager.SendMessage(msg);

    return (msg.Param1 == 1);
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  private bool IsSuspendOrHibernationAllowed()
  {
    TimeSpan ts = DateTime.Now - _lastOnresume;
    return (ts.TotalSeconds > _suspendGracePeriodSec);
  }

  #region keypress handlers
  
  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void KeyPressEvent(KeyPressEventArgs e)
  {
    GUIGraphicsContext.BlankScreen = false;
    var key = new Key(e.KeyChar, 0);
    var action = new Action();
    if (GUIWindowManager.IsRouted || GUIWindowManager.ActiveWindowEx == (int)GUIWindow.Window.WINDOW_TV_SEARCH)
    // is a dialog open or maybe the tv schedule search (GUISMSInputControl)?
    {
      GUIGraphicsContext.ResetLastActivity();
      if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action) &&
          (GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD) &&
          (GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_TV_SEARCH))
      {
        if (action.SoundFileName.Length > 0 && !g_Player.Playing)
        {
          Utils.PlaySound(action.SoundFileName, false, true);
        }
        GUIGraphicsContext.OnAction(action);
      }
      else
      {
        action = new Action(key, Action.ActionType.ACTION_KEY_PRESSED, 0, 0);
        GUIGraphicsContext.OnAction(action);
      }
      return;
    }
    if (key.KeyChar == '!')
    {
      _showStats = !_showStats;
    }
    if (key.KeyChar == '|' && g_Player.Playing == false)
    {
      g_Player.Play("rtsp://localhost/stream0");
      g_Player.ShowFullScreenWindow();
      return;
    }
    if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
    {
      if (action.ShouldDisableScreenSaver)
      {
        GUIGraphicsContext.ResetLastActivity();
      }
      if (action.SoundFileName.Length > 0 && !g_Player.Playing)
      {
        Utils.PlaySound(action.SoundFileName, false, true);
      }
      GUIGraphicsContext.OnAction(action);
    }
    else
    {
      GUIGraphicsContext.ResetLastActivity();
    }
    action = new Action(key, Action.ActionType.ACTION_KEY_PRESSED, 0, 0);
    GUIGraphicsContext.OnAction(action);
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void KeyDownEvent(KeyEventArgs e)
  {
    if (!_suspended)
    {
      GUIGraphicsContext.ResetLastActivity();
      var key = new Key(0, (int) e.KeyCode);
      var action = new Action();
      if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
      {
        if (action.SoundFileName.Length > 0 && !g_Player.Playing)
        {
          Utils.PlaySound(action.SoundFileName, false, true);
        }
        GUIGraphicsContext.OnAction(action);
      }
    }
  }

  #endregion

  #region mouse event handlers
  
  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void OnMouseWheel(MouseEventArgs e)
  {
    if (e.Delta > 0)
    {
      var action = new Action(Action.ActionType.ACTION_MOVE_UP, e.X, e.Y) {MouseButton = e.Button};
      GUIGraphicsContext.ResetLastActivity(); 
      GUIGraphicsContext.OnAction(action);
    }
    else if (e.Delta < 0)
    {
      var action = new Action(Action.ActionType.ACTION_MOVE_DOWN, e.X, e.Y) {MouseButton = e.Button};
      GUIGraphicsContext.ResetLastActivity();
      GUIGraphicsContext.OnAction(action);
    }
    base.OnMouseWheel(e);
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void MouseMoveEvent(MouseEventArgs e)
  {
    // Disable first mouse action when mouse was hidden
    base.MouseMoveEvent(e);

    if (ShowCursor)
    {
      if (_lastCursorPosition != Cursor.Position)
      {
        bool cursorMovedFarEnough = (Math.Abs(_lastCursorPosition.X - Cursor.Position.X) > 10) || (Math.Abs(_lastCursorPosition.Y - Cursor.Position.Y) > 10);
        if (cursorMovedFarEnough)
        {
          GUIGraphicsContext.ResetLastActivity();
          if (GUIGraphicsContext.DBLClickAsRightClick && _mouseClickFired)
          {
            CheckSingleClick(e);
          }
        }
        _lastCursorPosition = Cursor.Position;

        if (GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow) != null)
        {
          var action = new Action(Action.ActionType.ACTION_MOUSE_MOVE, e.X, e.Y) {MouseButton = e.Button};
          GUIGraphicsContext.OnAction(action);
        }
      }
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void MouseDoubleClickEvent(MouseEventArgs e)
  {
    if (GUIGraphicsContext.DBLClickAsRightClick)
    {
      return;
    }

    GUIGraphicsContext.ResetLastActivity();

    if (!ShowCursor)
    {
      base.MouseClickEvent(e);
      return;
    }
    _lastCursorPosition = Cursor.Position;

    var actionMove = new Action(Action.ActionType.ACTION_MOUSE_MOVE, e.X, e.Y);
    GUIGraphicsContext.OnAction(actionMove);

    var action = new Action(Action.ActionType.ACTION_MOUSE_DOUBLECLICK, e.X, e.Y) {MouseButton = e.Button, SoundFileName = "click.wav"};
    if (action.SoundFileName.Length > 0 && !g_Player.Playing)
    {
      Utils.PlaySound(action.SoundFileName, false, true);
    }
    GUIGraphicsContext.OnAction(action);
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void MouseClickEvent(MouseEventArgs e)
  {
    // Disable first mouse action when mouse was hidden
    GUIGraphicsContext.ResetLastActivity();

    if (!ShowCursor)
    {
      base.MouseClickEvent(e);
    }
    else
    {
      Action action;
      bool mouseButtonRightClick = false;
      _lastCursorPosition = Cursor.Position;

      var actionMove = new Action(Action.ActionType.ACTION_MOUSE_MOVE, e.X, e.Y);
      GUIGraphicsContext.OnAction(actionMove);

      if (e.Button == MouseButtons.Left)
      {
        if (GUIGraphicsContext.DBLClickAsRightClick)
        {
          _mouseClickFired = false;
          if (e.Clicks < 2)
          {
            _lastMouseClickEvent = e;
            _mouseClickFired = true;
            return;
          }
          // Double click used as right click
          _lastMouseClickEvent = null;

          mouseButtonRightClick = true;
        }
        else
        {
          action = new Action(Action.ActionType.ACTION_MOUSE_CLICK, e.X, e.Y) {MouseButton = e.Button, SoundFileName = "click.wav"};
          if (action.SoundFileName.Length > 0 && !g_Player.Playing)
          {
            Utils.PlaySound(action.SoundFileName, false, true);
          }
          GUIGraphicsContext.OnAction(action);
          return;
        }
      }

      // right mouse button=back
      if ((e.Button == MouseButtons.Right) || mouseButtonRightClick)
      {
        GUIWindow window = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
        if ((window.GetFocusControlId() != -1) || GUIGraphicsContext.IsFullScreenVideo ||
            (GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_SLIDESHOW))
        {
          // Get context menu
          action = new Action(Action.ActionType.ACTION_CONTEXT_MENU, e.X, e.Y) {MouseButton = e.Button, SoundFileName = "click.wav"};
          if (action.SoundFileName.Length > 0 && !g_Player.Playing)
          {
            Utils.PlaySound(action.SoundFileName, false, true);
          }
          GUIGraphicsContext.OnAction(action);
        }
        else
        {
          var key = new Key(0, (int) Keys.Escape);
          action = new Action();
          if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
          {
            if (action.SoundFileName.Length > 0 && !g_Player.Playing)
            {
              Utils.PlaySound(action.SoundFileName, false, true);
            }
            GUIGraphicsContext.OnAction(action);
            return;
          }
        }
      }

      // middle mouse button=Y
      if (e.Button == MouseButtons.Middle)
      {
        var key = new Key('y', 0);
        action = new Action();
        if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
        {
          if (action.SoundFileName.Length > 0 && !g_Player.Playing)
          {
            Utils.PlaySound(action.SoundFileName, false, true);
          }
          GUIGraphicsContext.OnAction(action);
        }
      }
    }
  }


  /// <summary>
  /// 
  /// </summary>
  private void CheckSingleClick(MouseEventArgs e)
  {
    // Check for touchscreen users and TVGuide items
    if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVGUIDE)
    {
      GUIWindow window = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
      if ((window.GetFocusControlId() == 1) && (GUIWindowManager.RoutedWindow == -1))
      {
        // Don't send single click (only the mouse move event is send)
        _mouseClickFired = false;
        return;
      }
    }

    if (_mouseClickFired)
    {
      _mouseClickFired = false;
      var action = new Action(Action.ActionType.ACTION_MOUSE_CLICK, e.X, e.Y) {MouseButton = _lastMouseClickEvent.Button, SoundFileName = "click.wav"};
      if (action.SoundFileName.Length > 0 && !g_Player.Playing)
      {
        Utils.PlaySound(action.SoundFileName, false, true);
      }
      GUIGraphicsContext.OnAction(action);
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  protected override void NotifyIconRestore(Object sender, EventArgs e)
  {
    if (Volume > 0 && (g_Player.IsVideo || g_Player.IsTV))
    {
      g_Player.Volume = Volume;
      if (g_Player.Paused)
      {
        g_Player.Pause();
      }
    }
    RestoreFromTray();
  }

  #endregion

#if AUTOUPDATE
  private void MediaPortal_Closed(object sender, EventArgs e)
  {
    StopUpdater();
  }
		
  private void CurrentDomain_ProcessExit(object sender, EventArgs e)
  {
    StopUpdater();
  }

  private delegate void MarshalEventDelegate(object sender, UpdaterActionEventArgs e);
 
  private void OnUpdaterDownloadStartedHandler(object sender, UpdaterActionEventArgs e) 
  {		
    Log.Info("Main: Update - Download started for: {0}",e.ApplicationName);
  }

  private void OnUpdaterDownloadStarted(object sender, UpdaterActionEventArgs e)
  { 
    this.Invoke(
      new MarshalEventDelegate(this.OnUpdaterDownloadStartedHandler), 
      new object[] { sender, e });
  }

  private void CheckForNewUpdate()
  {
    if (!m_bNewVersionAvailable) return;
    if (GUIWindowManager.IsRouted) return;
    g_Player.Stop();
    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO,0,0,0,0,0,0);
    msg.Param1=709;
    msg.Param2=710;
    msg.Param3=0;
    GUIWindowManager.SendMessage(msg);
    if (msg.Param1==0) 
    {
      Log.Info("Main: Update - User canceled download");
      m_bCancelVersion = true;
      m_bNewVersionAvailable = false;
      return;
    }
    m_bCancelVersion = false;
    m_bNewVersionAvailable = false;
  }

  private void OnUpdaterUpdateAvailable(object sender, UpdaterActionEventArgs e)
  {
    Log.Info("Main: Update - New version available: {0}", e.ApplicationName);
    m_strNewVersion = e.ServerInformation.AvailableVersion;
    m_bNewVersionAvailable = true;
    while (m_bNewVersionAvailable) System.Threading.Thread.Sleep(100);
    if (m_bCancelVersion)
    {
      _updater.StopUpdater(e.ApplicationName);
    }
  }

  private void OnUpdaterDownloadCompletedHandler(object sender, UpdaterActionEventArgs e)
  {
    Log.Info("Main: Update - Download completed");
    StartNewVersion();
  }

  private void OnUpdaterDownloadCompleted(object sender, UpdaterActionEventArgs e)
  {
    //  using the synchronous "Invoke".  This marshals from the eventing thread--which comes from the Updater and should not
    //  be allowed to enter and "touch" the UI's window thread
    //  so we use Invoke which allows us to block the Updater thread at will while only allowing window thread to update UI
    this.Invoke(
      new MarshalEventDelegate(this.OnUpdaterDownloadCompletedHandler), 
      new object[] { sender, e });
  }

  private void StartNewVersion()
  {
    Log.Info("Main: Update - Starting appstart.exe");
    XmlDocument doc = new XmlDocument();
    //  load config file to get base dir
    doc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
    //  get the base dir
    string baseDir = System.IO.Directory.GetCurrentDirectory(); //doc.SelectSingleNode("configuration/appUpdater/UpdaterConfiguration/application/client/baseDir").InnerText;
    string newDir = Path.Combine(baseDir, "AppStart.exe");
		ClientApplicationInfo clientInfoNow = ClientApplicationInfo.Deserialize("MediaPortal.exe.config");
    ClientApplicationInfo clientInfo = ClientApplicationInfo.Deserialize("AppStart.exe.config");
    clientInfo.AppFolderName = System.IO.Directory.GetCurrentDirectory();
    ClientApplicationInfo.Save("AppStart.exe.config",clientInfo.AppFolderName, clientInfoNow.InstalledVersion);
    ProcessStartInfo process = new ProcessStartInfo(newDir);
    process.WorkingDirectory = baseDir;
    process.Arguments = clientInfoNow.InstalledVersion;
    //  launch new version (actually, launch AppStart.exe which HAS pointer to new version )
    System.Diagnostics.Process.Start(process);
    //  tell updater to stop
    Log.Info("Main: Update - Stopping MP");
    CurrentDomain_ProcessExit(null, null);
    //  leave this app
    Environment.Exit(0);
  }

  private void btnStop_Click(object sender, System.EventArgs e)
  {
    StopUpdater();
  }

  private void StopUpdater()
  {
    if (_updater==null) return;
    //  tell updater to stop
    _updater.StopUpdater();
    if (null != _updaterThread)
    {
      //  join the updater thread with a suitable timeout
      bool isThreadJoined = _updaterThread.Join(UPDATERTHREAD_JOIN_TIMEOUT);
      //  check if we joined, if we didn't interrupt the thread
      if (!isThreadJoined)
      {
        _updaterThread.Interrupt();	
      }
      _updaterThread = null;
    }
  }
#endif


  /// <summary>
  /// 
  /// </summary>
  /// <param name="message"></param>
  private void OnMessage(GUIMessage message)
  {
    if (!_suspended)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_RESTART_REMOTE_CONTROLS:
          Log.Info("Main: Restart remote controls");
          InputDevices.Stop();
          InputDevices.Init();
          break;

        case GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW:
          GUIWindowManager.ActivateWindow(message.Param1);
          GUIGraphicsContext.IsFullScreenVideo = GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_TVFULLSCREEN ||
                                                 GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT ||
                                                 GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO ||
                                                 GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_FULLSCREEN_MUSIC;
          break;

        case GUIMessage.MessageType.GUI_MSG_CD_INSERTED:
          AutoPlay.ExamineCD(message.Label);
          break;

        case GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED:
          AutoPlay.ExamineVolume(message.Label);
          break;

        case GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL:
          double retNum;
          bool bIsInteger = Double.TryParse(message.Label, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out retNum);
          try
          {
            if (bIsInteger)
            {
              _usbuirtdevice.ChangeTunerChannel(message.Label);
            }
          }
          catch (Exception)
          {
          }
          try
          {
            _winlircdevice.ChangeTunerChannel(message.Label);
          }
          catch (Exception)
          {
          }
          try
          {
            if (bIsInteger)
            {
              _redeyedevice.ChangeTunerChannel(message.Label);
            }
          }
          catch (Exception)
          {
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED:
          if (GUIGraphicsContext.IsDirectX9ExUsed() && UseEnhancedVideoRenderer)
          {
            return;
          }
          bool fullscreen = (message.Param1 != 0);
          Log.Debug("Main: Received DX exclusive mode switch message. Fullscreen && Windowed == {0}", fullscreen && Windowed);
          if (!Windowed || GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
          {
            return;
          }
          if (fullscreen)
          {
            // switch to fullscreen mode
            Log.Debug("Main: Goto fullscreen: {0}", GUIGraphicsContext.DX9Device.PresentationParameters.Windowed);
            UpdatePresentParams(false, false);
          }
          else
          {
            // switch to windowed mode
            Log.Debug("Main: Goto windowed mode: {0}", GUIGraphicsContext.DX9Device.PresentationParameters.Windowed);
            UpdatePresentParams(true, false);
          }

          // Must set the FVF after reset
          GUIFontManager.SetDevice();
          break;

        case GUIMessage.MessageType.GUI_MSG_GETFOCUS:
          Log.Debug("Main: Setting focus");
          if (WindowState != FormWindowState.Minimized)
          {
            Activate();
          }
          else
          {
            if (Volume > 0 && (g_Player.IsVideo || g_Player.IsTV))
            {
              g_Player.Volume = Volume;
              if (g_Player.Paused)
              {
                g_Player.Pause();
              }
            }
            RestoreFromTray();
          }
          //Force.SetForegroundWindow(this.Handle, true);
          break;

        case GUIMessage.MessageType.GUI_MSG_CODEC_MISSING:
          var dlgOk = (GUIDialogOK) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_OK);
          dlgOk.SetHeading(string.Empty);
          dlgOk.SetLine(1, message.Label);
          dlgOk.SetLine(2, string.Empty);
          dlgOk.SetLine(3, message.Label2);
          dlgOk.SetLine(4, message.Label3);
          dlgOk.DoModal(GUIWindowManager.ActiveWindow);
          break;

        case GUIMessage.MessageType.GUI_MSG_REFRESHRATE_CHANGED:
          var dlgNotify = (GUIDialogNotify) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
          if (dlgNotify != null)
          {
            dlgNotify.Reset();
            dlgNotify.ClearAll();
            dlgNotify.SetHeading(message.Label);
            dlgNotify.SetText(message.Label2);
            dlgNotify.TimeOut = message.Param1;
            dlgNotify.DoModal(GUIWindowManager.ActiveWindow);
          }

          break;
      }
    }
  }

  #endregion

  #region External process start / stop handling


  /// <summary>
  /// 
  /// </summary>
  /// <param name="proc"></param>
  /// <param name="waitForExit"></param>
  public void OnStartExternal(Process proc, bool waitForExit)
  {
    if (TopMost && waitForExit)
    {
      TopMost = false;
      _restoreTopMost = true;
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="proc"></param>
  /// <param name="waitForExit"></param>
  public void OnStopExternal(Process proc, bool waitForExit)
  {
    if (_restoreTopMost)
    {
      TopMost = true;
      _restoreTopMost = false;
    }
  }

  #endregion

  #region helper funcs
  
  /// <summary>
  /// 
  /// </summary>
  private void CreateStateBlock()
  {
    GUIGraphicsContext.DX9Device.RenderState.CullMode          = Cull.None;
    GUIGraphicsContext.DX9Device.RenderState.Lighting          = false;
    GUIGraphicsContext.DX9Device.RenderState.ZBufferEnable     = true;
    GUIGraphicsContext.DX9Device.RenderState.FogEnable         = false;
    GUIGraphicsContext.DX9Device.RenderState.FillMode          = FillMode.Solid;
    GUIGraphicsContext.DX9Device.RenderState.SourceBlend       = Blend.SourceAlpha;
    GUIGraphicsContext.DX9Device.RenderState.DestinationBlend  = Blend.InvSourceAlpha;
    GUIGraphicsContext.DX9Device.TextureState[0].ColorOperation = TextureOperation.Modulate;
    GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
    GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
    GUIGraphicsContext.DX9Device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
    GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
    GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument2 = TextureArgument.Diffuse;
    if (_supportsFiltering)
    {
      GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter     = TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter     = TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter     = TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[0].MaxAnisotropy = _anisotropy;
      GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter     = TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter     = TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter     = TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[1].MaxAnisotropy = _anisotropy;
    }
    else
    {
      GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter = TextureFilter.Point;
      GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter = TextureFilter.Point;
      GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter = TextureFilter.Point;
      GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter = TextureFilter.Point;
      GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter = TextureFilter.Point;
      GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter = TextureFilter.Point;
    }
    if (_supportsAlphaBlend)
    {
      GUIGraphicsContext.DX9Device.RenderState.AlphaTestEnable = true;
      GUIGraphicsContext.DX9Device.RenderState.ReferenceAlpha  = 0x01;
      GUIGraphicsContext.DX9Device.RenderState.AlphaFunction   = Compare.GreaterEqual;
    }
  }

  /// <summary>
  /// Get the current date from the system and localize it based on the user preferences.
  /// </summary>
  /// <returns>A string containing the localized version of the date.</returns>
  protected string GetDate()
  {
    string dateString = DateFormat;
    if (!string.IsNullOrEmpty(dateString))
    {
      DateTime cur = DateTime.Now;
      string day;
      switch (cur.DayOfWeek)
      {
        case DayOfWeek.Monday:
          day = GUILocalizeStrings.Get(11);
          break;
        case DayOfWeek.Tuesday:
          day = GUILocalizeStrings.Get(12);
          break;
        case DayOfWeek.Wednesday:
          day = GUILocalizeStrings.Get(13);
          break;
        case DayOfWeek.Thursday:
          day = GUILocalizeStrings.Get(14);
          break;
        case DayOfWeek.Friday:
          day = GUILocalizeStrings.Get(15);
          break;
        case DayOfWeek.Saturday:
          day = GUILocalizeStrings.Get(16);
          break;
        default:
          day = GUILocalizeStrings.Get(17);
          break;
      }
      string month;
      switch (cur.Month)
      {
        case 1:
          month = GUILocalizeStrings.Get(21);
          break;
        case 2:
          month = GUILocalizeStrings.Get(22);
          break;
        case 3:
          month = GUILocalizeStrings.Get(23);
          break;
        case 4:
          month = GUILocalizeStrings.Get(24);
          break;
        case 5:
          month = GUILocalizeStrings.Get(25);
          break;
        case 6:
          month = GUILocalizeStrings.Get(26);
          break;
        case 7:
          month = GUILocalizeStrings.Get(27);
          break;
        case 8:
          month = GUILocalizeStrings.Get(28);
          break;
        case 9:
          month = GUILocalizeStrings.Get(29);
          break;
        case 10:
          month = GUILocalizeStrings.Get(30);
          break;
        case 11:
          month = GUILocalizeStrings.Get(31);
          break;
        default:
          month = GUILocalizeStrings.Get(32);
          break;
      }
      dateString = Utils.ReplaceTag(dateString, "<Day>", day, "unknown");
      dateString = Utils.ReplaceTag(dateString, "<DD>", cur.Day.ToString(CultureInfo.InvariantCulture), "unknown");
      dateString = Utils.ReplaceTag(dateString, "<Month>", month, "unknown");
      dateString = Utils.ReplaceTag(dateString, "<MM>", cur.Month.ToString(CultureInfo.InvariantCulture), "unknown");
      dateString = Utils.ReplaceTag(dateString, "<Year>", cur.Year.ToString(CultureInfo.InvariantCulture), "unknown");
      dateString = Utils.ReplaceTag(dateString, "<YY>", (cur.Year - 2000).ToString("00"), "unknown");
      GUIPropertyManager.SetProperty("#date", dateString);
      return dateString;
    }
    return string.Empty;
  }

  /// <summary>
  /// Get the current time from the system. Set the format in the Home plugin's config
  /// </summary>
  /// <returns>A string containing the current time.</returns>
  protected string GetTime()
  {
    return DateTime.Now.ToString(UseLongDateFormat 
      ? Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongTimePattern 
      : Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortTimePattern);
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  protected string GetDay()
  {
    DateTime cur = DateTime.Now;
    return String.Format("{0}", cur.Day);
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  protected string GetShortDayOfWeek()
  {
    DateTime cur = DateTime.Now;
    string ddd;
    switch (cur.DayOfWeek)
    {
      case DayOfWeek.Monday:
        ddd = GUILocalizeStrings.Get(657);
        break;
      case DayOfWeek.Tuesday:
        ddd = GUILocalizeStrings.Get(658);
        break;
      case DayOfWeek.Wednesday:
        ddd = GUILocalizeStrings.Get(659);
        break;
      case DayOfWeek.Thursday:
        ddd = GUILocalizeStrings.Get(660);
        break;
      case DayOfWeek.Friday:
        ddd = GUILocalizeStrings.Get(661);
        break;
      case DayOfWeek.Saturday:
        ddd = GUILocalizeStrings.Get(662);
        break;
      default:
        ddd = GUILocalizeStrings.Get(663);
        break;
    }
    return ddd;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  protected string GetDayOfWeek()
  {
    DateTime cur = DateTime.Now;
    string dddd;
    switch (cur.DayOfWeek)
    {
      case DayOfWeek.Monday:
        dddd = GUILocalizeStrings.Get(11);
        break;
      case DayOfWeek.Tuesday:
        dddd = GUILocalizeStrings.Get(12);
        break;
      case DayOfWeek.Wednesday:
        dddd = GUILocalizeStrings.Get(13);
        break;
      case DayOfWeek.Thursday:
        dddd = GUILocalizeStrings.Get(14);
        break;
      case DayOfWeek.Friday:
        dddd = GUILocalizeStrings.Get(15);
        break;
      case DayOfWeek.Saturday:
        dddd = GUILocalizeStrings.Get(16);
        break;
      default:
        dddd = GUILocalizeStrings.Get(17);
        break;
    }
    return dddd;
  }

  protected string GetMonth()
  {
    DateTime cur = DateTime.Now;
    return cur.ToString("MM");
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  protected string GetShortMonthOfYear()
  {
    string smoy = GetMonthOfYear();
    smoy = smoy.Substring(0, 3);
    return smoy;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  protected string GetMonthOfYear()
  {
    DateTime cur = DateTime.Now;
    string mmmm;
    switch (cur.Month)
    {
      case 1:
        mmmm = GUILocalizeStrings.Get(21);
        break;
      case 2:
        mmmm = GUILocalizeStrings.Get(22);
        break;
      case 3:
        mmmm = GUILocalizeStrings.Get(23);
        break;
      case 4:
        mmmm = GUILocalizeStrings.Get(24);
        break;
      case 5:
        mmmm = GUILocalizeStrings.Get(25);
        break;
      case 6:
        mmmm = GUILocalizeStrings.Get(26);
        break;
      case 7:
        mmmm = GUILocalizeStrings.Get(27);
        break;
      case 8:
        mmmm = GUILocalizeStrings.Get(28);
        break;
      case 9:
        mmmm = GUILocalizeStrings.Get(29);
        break;
      case 10:
        mmmm = GUILocalizeStrings.Get(30);
        break;
      case 11:
        mmmm = GUILocalizeStrings.Get(31);
        break;
      default:
        mmmm = GUILocalizeStrings.Get(32);
        break;
    }
    return mmmm;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  protected string GetShortYear()
  {
    DateTime cur = DateTime.Now;
    return cur.ToString("yy");
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  protected string GetYear()
  {
    DateTime cur = DateTime.Now;
    return cur.ToString("yyyy");
  }


  /// <summary>
  /// 
  /// </summary>
  protected void CheckSkinVersion()
  {
    bool ignoreErrors;
    using (Settings xmlreader = new MPSettings())
    {
      ignoreErrors = xmlreader.GetValueAsBool("general", "dontshowskinversion", false);
    }

    if (!ignoreErrors)
    {
      Version versionSkin = null;
      string filename = GUIGraphicsContext.GetThemedSkinFile(@"\references.xml");
      if (File.Exists(filename))
      {
        var doc = new XmlDocument();
        doc.Load(filename);
        XmlNode node = doc.SelectSingleNode("/controls/skin/version");
        if (node != null)
        {
          versionSkin = new Version(node.InnerText);
        }
      }
      if (CompatibilityManager.SkinVersion != versionSkin)
      {
        _outdatedSkinName = GUIGraphicsContext.SkinName;
        float screenHeight = GUIGraphicsContext.currentScreen.Bounds.Height;
        float screenWidth = GUIGraphicsContext.currentScreen.Bounds.Width;
        float screenRatio = (screenWidth/screenHeight);
        GUIGraphicsContext.Skin = screenRatio > 1.5 ? "DefaultWide" : "Default";
        Config.SkinName = GUIGraphicsContext.SkinName;
        SkinSettings.Load();

        // Send a message that the skin has changed.
        var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SKIN_CHANGED, 0, 0, 0, 0, 0, null);
        GUIGraphicsContext.SendMessage(msg);

        Log.Info("Main: User skin is not compatible, using skin {0} with theme {1}", GUIGraphicsContext.SkinName, GUIThemeManager.CurrentTheme);
      }
    }
  }


  #region registry helper function

  public static void SetDWORDRegKey(RegistryKey hklm, string key, string value, Int32 iValue)
  {
    try
    {
      using (RegistryKey subkey = hklm.CreateSubKey(key))
      {
        if (subkey != null)
        {
          subkey.SetValue(value, iValue);
        }
      }
    }
    catch (SecurityException)
    {
      Log.Error(@"User does not have sufficient rights to modify registry key HKLM\{0}", key);
    }
    catch (UnauthorizedAccessException)
    {
      Log.Error(@"User does not have sufficient rights to modify registry key HKLM\{0}", key);
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="hklm"></param>
  /// <param name="key"></param>
  /// <param name="name"></param>
  /// <param name="value"></param>
  public static void SetREGSZRegKey(RegistryKey hklm, string key, string name, string value)
  {
    try
    {
      using (RegistryKey subkey = hklm.CreateSubKey(key))
      {
        if (subkey != null)
        {
          subkey.SetValue(name, value);
        }
      }
    }
    catch (SecurityException)
    {
      Log.Error(@"User does not have sufficient rights to modify registry key HKLM\{0}", key);
    }
    catch (UnauthorizedAccessException)
    {
      Log.Error(@"User does not have sufficient rights to modify registry key HKLM\{0}", key);
    }
  }

  #endregion

  #endregion

  /// <summary>
  /// 
  /// </summary>
  private void DoStartupJobs()
  {
    FilterChecker.CheckInstalledVersions();
    
    Version aParamVersion;
    //
    // 6.5.2600.3243 = KB941568,   6.5.2600.3024 = KB927544
    //
    if (
      !FilterChecker.CheckFileVersion(Environment.SystemDirectory + "\\quartz.dll", "6.5.2600.3024", out aParamVersion))
    {
      string errorMsg = string.Format("Your version {0} of quartz.dll has too many bugs! \nPlease check our Wiki's requirements page.", aParamVersion);
      Log.Info("Util: quartz.dll error - {0}", errorMsg);
      if (MessageBox.Show(errorMsg, "Core directshow component (quartz.dll) is outdated!", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
      {
        Process.Start(@"http://wiki.team-mediaportal.com/GeneralRequirements");
      }
    }

    GUIWindowManager.OnNewAction += OnAction;
    GUIWindowManager.Receivers += OnMessage;
    GUIWindowManager.Callbacks += MPProcess;
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STARTING;
    Utils.OnStartExternal += OnStartExternal;
    Utils.OnStopExternal += OnStopExternal;
    // register the playlistplayer for thread messages (like playback stopped,ended)
    Log.Info("Main: Init playlist player");
    g_Player.Factory = new PlayerFactory();
    PlaylistPlayer.Init();
    // Only load the USBUIRT device if it has been enabled in the configuration
    using (Settings xmlreader = new MPSettings())
    {
      bool inputEnabled = xmlreader.GetValueAsBool("USBUIRT", "internal", false);
      bool outputEnabled = xmlreader.GetValueAsBool("USBUIRT", "external", false);
      if (inputEnabled || outputEnabled)
      {
        Log.Info("Main: Creating the USBUIRT device");
        _usbuirtdevice = USBUIRT.Create(OnRemoteCommand);
        Log.Info("Main: Creating the USBUIRT device done");
      }
      // Load Winlirc if enabled.
      bool winlircInputEnabled = xmlreader.GetValueAsString("WINLIRC", "enabled", "false") == "true";
      if (winlircInputEnabled)
      {
        Log.Info("Main: Creating the WINLIRC device");
        _winlircdevice = new WinLirc();
        Log.Info("Main: Creating the WINLIRC device done");
      }
      // Load RedEye if enabled.
      bool redeyeInputEnabled = xmlreader.GetValueAsString("RedEye", "internal", "false") == "true";
      if (redeyeInputEnabled)
      {
        Log.Info("Main: Creating the REDEYE device");
        _redeyedevice = RedEye.Create(OnRemoteCommand);
        Log.Info("Main: Creating the RedEye device done");
      }
      inputEnabled = xmlreader.GetValueAsString("SerialUIR", "internal", "false") == "true";
      if (inputEnabled)
      {
        Log.Info("Main: Creating the SerialUIR device");
        _serialuirdevice = SerialUIR.Create(OnRemoteCommand);
        Log.Info("Main: Creating the SerialUIR device done");
      }
    }
    // registers the player for video window size notifications
    Log.Info("Main: Init players");
    g_Player.Init();
    GUIGraphicsContext.ActiveForm = Handle;
    // hook ProcessExit for a chance to clean up when closed peremptorily
 #if AUTOUPDATE
    AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
    //  hook form close to stop updater too
    this.Closed += new EventHandler(MediaPortal_Closed);
#endif
    var doc = new XmlDocument();
    try
    {
      doc.Load("mediaportal.exe.config");
      XmlNode node = doc.SelectSingleNode("/configuration/appStart/ClientApplicationInfo/appFolderName");
      if (node != null)
      {
        node.InnerText = Directory.GetCurrentDirectory();
      }
      node = doc.SelectSingleNode("/configuration/appUpdater/UpdaterConfiguration/application/client/baseDir");
      if (node != null)
      {
        node.InnerText = Directory.GetCurrentDirectory();
      }
      node = doc.SelectSingleNode("/configuration/appUpdater/UpdaterConfiguration/application/client/tempDir");
      if (node != null)
      {
        node.InnerText = Directory.GetCurrentDirectory();
      }
      doc.Save("MediaPortal.exe.config");
    }
    catch (Exception) {}
    Thumbs.CreateFolders();
    try
    {
#if DEBUG
#else
#if AUTOUPDATE
      UpdaterConfiguration config = UpdaterConfiguration.Instance;
      config.Logging.LogPath = Config.Get(Config.Dir.Log) + "updatelog.log";
      config.Applications[0].Client.BaseDir = Config.Get(Config.Dir.Base)
      config.Applications[0].Client.TempDir =  Config.Get(Config.Dir.Base) + "temp";
      config.Applications[0].Client.XmlFile =  Config.Get(Config.Dir.Base) + "MediaPortal.exe.config";
      config.Applications[0].Server.ServerManifestFileDestination =  Config.Get(Config.Dir.Base) + @"xml\ServerManifest.xml";
      try
      {
        System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"temp");
				System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"xml");
				System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"log");
			}
			catch(Exception){}
			Utils.DeleteFiles(config.Applications[0].Client.BaseDir + "log", "*.log");
			ClientApplicationInfo clientInfo = ClientApplicationInfo.Deserialize("MediaPortal.exe.config");
			clientInfo.AppFolderName = System.IO.Directory.GetCurrentDirectory();
			ClientApplicationInfo.Save("MediaPortal.exe.config",clientInfo.AppFolderName, clientInfo.InstalledVersion);
			m_strCurrentVersion = clientInfo.InstalledVersion;
			Text += (" - [v" + m_strCurrentVersion + "]");
			//  make an Updater for use in-process with us
			_updater = new ApplicationUpdateManager();
			//  hook Updater events
			_updater.DownloadStarted += new UpdaterActionEventHandler(OnUpdaterDownloadStarted);
			_updater.UpdateAvailable += new UpdaterActionEventHandler(OnUpdaterUpdateAvailable);
			_updater.DownloadCompleted += new UpdaterActionEventHandler(OnUpdaterDownloadCompleted);
			//  start the updater on a separate thread so that our UI remains responsive
			_updaterThread = new Thread(new ThreadStart(_updater.StartUpdater));
			_updaterThread.Start();
#endif
#endif
    }
    catch (Exception) {}
    using (Settings xmlreader = new MPSettings())
    {
      _dateLayout = xmlreader.GetValueAsInt("home", "datelayout", 0);
    }
    GUIGraphicsContext.ResetLastActivity();
  }
}