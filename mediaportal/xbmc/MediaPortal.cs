using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.Win32;
using System.Globalization;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

using Microsoft.ApplicationBlocks.ApplicationUpdater;

using MediaPortal.GUI.Library;
using MediaPortal;
using MediaPortal.Player;
using MediaPortal.Util;
using MediaPortal.Playlists;
using MediaPortal.TV.Recording;
using MediaPortal.SerialIR;
using MediaPortal.IR;
using MediaPortal.WINLIRC;//sd00//
using MediaPortal.Ripper;
using MediaPortal.Core.Transcoding;
/// <summary>
/// - fixed issues in tvguide
/// - fixed possible hangups when 2 threads where accessing onmessage() or onaction()
/// -
/// performance issues:
/// guilib
///   thumbpanel     : better to use an array of GUILabelControls
///   autoplay&dialogs subproject is gone.
///   batch DX9      : sprite class?
/// 
/// performance enhancements made:
///   - re-packaged all subprojects into 10 assemblies
///   - added font caching to guilabelcontrol
///   - Tuned the following controls:button, selectbutton, spin, label 
///   - WMP9 now gets cleaned-up when playing music has ended
///   - don't clear DX9 device anymore
///   - use of DX9 compressed textures for fonts
///   - use of DX9 saved renderstate state blocks
///   - debug build now shows cpu performance after pressing the character ! 
/// </summary>
public class MediaPortalApp : D3DApp, IRender
{ 
    private ApplicationUpdateManager _updater = null;
    private Thread _updaterThread = null;
    private const int UPDATERTHREAD_JOIN_TIMEOUT = 3 * 1000;
    int m_iLastMousePositionX = 0;
    int m_iLastMousePositionY = 0;
    private System.Threading.Mutex m_Mutex;
    private string m_UniqueIdentifier;
    bool m_bPlayingState = false;
    bool m_bShowStats = false;
    Rectangle[]                     region = new Rectangle[1];
    int m_ixpos = 50;
    int m_iFrameCount = 0;
	private SerialUIR serialuirdevice = null;
    private USBUIRT usbuirtdevice;
	  private WinLirc winlircdevice;//sd00//
    string m_strNewVersion = "";
    string m_strCurrentVersion = "";
    bool m_bNewVersionAvailable = false;
    bool m_bCancelVersion = false;
    MCE2005Remote MCE2005Remote = new MCE2005Remote();
		MouseEventArgs eLastMouseClickEvent = null;
		private System.Timers.Timer tMouseClickTimer = null;
		private bool bMouseClickFired = false;

    const int WM_KEYDOWN = 0x0100;
    const int WM_SYSCOMMAND = 0x0112;
    const int SC_SCREENSAVE = 0xF140;
    const int SW_RESTORE = 9;
		bool supportsFiltering=false;
		int g_nAnisotropy;
		DateTime      m_updateTimer=DateTime.MinValue;
		int						m_iDateLayout;

    static SplashScreen splashScreen;

    [DllImport("user32.dll")] private static extern
        bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern
        bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);


    //NProf doesnt work if the [STAThread] attribute is set
    //but is needed when you want to play music or video
		[STAThread]
    public static void Main()
    {

			/*
			TranscodeInfo info = new TranscodeInfo();
			info.file=@"C:\media\movies\Chart Show TV (90)_Manual_200412121542p4224.dvr-ms";
			info.Title="Circular floor";
			info.Description="test movie";
			info.Rating="6";
			info.Author="i dont know";

			TranscodeFactory factory = new TranscodeFactory();
			ITranscode transcoder=factory.GetTranscoder(info,VideoFormat.Mpeg2,Quality.High);
			transcoder.Transcode(info,VideoFormat.Mpeg2,Quality.High);
			*/

			Log.Write("Mediaportal is starting up");

			//Set current directory
			string applicationPath=Application.ExecutablePath;
			applicationPath=System.IO.Path.GetFullPath(applicationPath);
			applicationPath=System.IO.Path.GetDirectoryName(applicationPath);
			System.IO.Directory.SetCurrentDirectory(applicationPath);

			Log.Write("  Set current directory to :{0}", applicationPath);
      // Display splash screen
      //

      //check if mediaportal has been configured
      if ( !System.IO.File.Exists("mediaportal.xml") )
      {
        //no, then start configuration.exe in wizard form
        System.Diagnostics.Process.Start("configuration.exe", @"/wizard");
        return;
      }
      CodecsForm form = new CodecsForm();
      if (!form.AreCodecsInstalled())
      {
        form.ShowDialog();
      }


      ClientApplicationInfo clientInfo = ClientApplicationInfo.Deserialize("MediaPortal.exe.config");
#if DEBUG
#else
      splashScreen = new SplashScreen();
      splashScreen.SetVersion(clientInfo .InstalledVersion);
      splashScreen.Show();
      splashScreen.Update();
#endif

      Log.Write("  Set registry keys for intervideo/windvd/hauppauge codecs");
      // Set Intervideo registry keys 
      try
      {
        RegistryKey hklm = Registry.LocalMachine;

        // windvd6 mpeg2 codec settings
        SetDWORDRegKey(hklm,@"SOFTWARE\InterVideo\MediaPortal\AudioDec","DsContinuousRate",1);
        SetDWORDRegKey(hklm,@"SOFTWARE\InterVideo\MediaPortal\VideoDec","DsContinuousRate",1);
        SetDWORDRegKey(hklm,@"SOFTWARE\InterVideo\MediaPortal\VideoDec","Dxva",1);
        SetDWORDRegKey(hklm,@"SOFTWARE\InterVideo\MediaPortal\VideoDec","DxvaFetchSample",0);
        SetDWORDRegKey(hklm,@"SOFTWARE\InterVideo\MediaPortal\VideoDec","ResendOnFamine",0);
        SetDWORDRegKey(hklm,@"SOFTWARE\InterVideo\MediaPortal\VideoDec","VgaQuery",1);
        SetDWORDRegKey(hklm,@"SOFTWARE\InterVideo\MediaPortal\VideoDec","VMR",2);
        
        // hauppauge mpeg2 codec settings
        SetDWORDRegKey(hklm,@"SOFTWARE\IviSDK4Hauppauge\Common\VideoDec","Hwmc",1);
        SetDWORDRegKey(hklm,@"SOFTWARE\IviSDK4Hauppauge\Common\VideoDec","Dxva",1);

        hklm.Close();

				// windvd6 mpeg2 codec settings
				SetDWORDRegKey(hklm,@"SOFTWARE\InterVideo\MediaPortal\AudioDec","DsContinuousRate",1);
				SetDWORDRegKey(hklm,@"SOFTWARE\InterVideo\MediaPortal\VideoDec","DsContinuousRate",1);
				SetDWORDRegKey(hklm,@"SOFTWARE\InterVideo\MediaPortal\VideoDec","Dxva",1);
				SetDWORDRegKey(hklm,@"SOFTWARE\InterVideo\MediaPortal\VideoDec","DxvaFetchSample",0);
				SetDWORDRegKey(hklm,@"SOFTWARE\InterVideo\MediaPortal\VideoDec","ResendOnFamine",0);
				SetDWORDRegKey(hklm,@"SOFTWARE\InterVideo\MediaPortal\VideoDec","VgaQuery",1);
				SetDWORDRegKey(hklm,@"SOFTWARE\InterVideo\MediaPortal\VideoDec","VMR",2);
				hklm.Close();

      }
      catch(Exception)
      {
      }


			Log.Write("  verify that directx 9 is installed");
			try 
			{
				// CHECK if DirectX 9.0c if installed
				RegistryKey hklm = Registry.LocalMachine;
				RegistryKey subkey = hklm.OpenSubKey(@"Software\Microsoft\DirectX");
				if (subkey != null)
				{
					string strVersion = (string)subkey.GetValue("Version");
					if (strVersion != null)
					{
						if (strVersion.Length > 0)
						{
							string strTmp = "";
							for (int i = 0; i < strVersion.Length; ++i)
							{
								if (Char.IsDigit(strVersion[i])) strTmp += strVersion[i];
							}
							long lVersion = System.Convert.ToInt64(strTmp);
              if (lVersion < 409000904)
              {
                string strLine = "Please install DirectX 9.0c!\r\n";
                strLine = strLine + "Current version installed:" + strVersion + "\r\n\r\n";
                strLine = strLine + "Mediaportal cannot run without DirectX 9.0c\r\n";
                strLine = strLine + "http://www.microsoft.com/directx";
                System.Windows.Forms.MessageBox.Show(strLine, "MediaPortal", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
              }
						}
					}
					
					string strVersionMng = (string)subkey.GetValue("ManagedDirectXVersion");
					if (strVersionMng != null)
					{
						if (strVersionMng.Length > 0)
						{
							string strTmp = "";
							for (int i = 0; i < strVersionMng.Length; ++i)
							{
								if (Char.IsDigit(strVersionMng[i])) strTmp += strVersionMng[i];
							}
							long lVersion = System.Convert.ToInt64(strTmp);
//							if (lVersion < 409001126)
//							{
//                string strLine="Please install Managed DirectX 9.0c!\r\n";
//                strLine=strLine+ "Current version installed:"+strVersionMng+"\r\n\r\n";
//                strLine=strLine+ "Mediaportal cannot run without DirectX 9.0c\r\n";
//                strLine=strLine+ "http://www.microsoft.com/directx";
//                System.Windows.Forms.MessageBox.Show(strLine, "MediaPortal", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
//                return;
//							}
						}
					}
					subkey.Close();
					subkey = null;
				}

				// CHECK if Windows MediaPlayer 9 is installed
				Log.Write("  verify that windows mediaplayer 9 or 10 is installed");
				subkey = hklm.OpenSubKey(@"Software\Microsoft\MediaPlayer\9.0");
        if (subkey == null)
          subkey = hklm.OpenSubKey(@"Software\Microsoft\MediaPlayer\10.0");
        if (subkey != null)
        {
          subkey.Close();
          subkey = null;
        }
        else
        {
          string strLine = "Please install Windows Mediaplayer 9\r\n";
          strLine = strLine + "Mediaportal cannot run without Windows Mediaplayer 9";
          System.Windows.Forms.MessageBox.Show(strLine, "MediaPortal", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
          return;
        }
				hklm.Close();
				hklm = null;
			}
			catch(Exception)
			{
			}

			Log.Write("  Stop any known recording processes");
			Utils.KillExternalTVProcesses();
      try
      {
				if (splashScreen!=null) splashScreen.SetInformation("Initializing DirectX...");
        MediaPortalApp app = new MediaPortalApp();
				Log.Write("  initializing DirectX");
        if (app.CreateGraphicsSample())
        {
          //app.PreRun();
          Log.Write("running...");
          try
          {
            app.Run();

            //Application.Run(app);
          }
          catch (Exception ex)
          {
            Log.Write("MediaPortal stopped due 2 an exception {0} {1} {2}",ex.Message, ex.Source, ex.StackTrace);
          }
          app.OnExit();
          Log.Write("MediaPortal done");
          Win32API.EnableStartBar(true);
          Win32API.ShowStartBar(true);
        }
      }
      catch (Exception ex)
      {
        Log.Write("MediaPortal stopped due 2 an exception {0} {1} {2}",ex.Message, ex.Source, ex.StackTrace);
      }
#if DEBUG
#else
      if (splashScreen != null)
        splashScreen.Close();
#endif
    }

    public static void SetDWORDRegKey(RegistryKey hklm,string Key,string Value,Int32 iValue)
    {      
      RegistryKey subkey = hklm.CreateSubKey(Key);
      if (subkey != null)
      {
        subkey.SetValue(Value,iValue);
        subkey.Close();
      }
    }

	  private void OnRemoteCommand(object command)
	  {
		  GUIGraphicsContext.OnAction(new Action((Action.ActionType) command, 0, 0));
	  }

    public MediaPortalApp()
		{
		// check to load plugins
		bool tmpPluginsFlag=false;
		using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
		{
			tmpPluginsFlag=xmlreader.GetValueAsBool("DVBSS2","enablePlugins",false);
		}

		if(tmpPluginsFlag==true)
		{
			if(System.IO.File.Exists(Application.StartupPath+@"\SoftCSA.dll")==true)
				DVBGraphSS2.SetMenuHandle(mnuMain.Handle.ToInt32());
			else
			{
				using (AMS.Profile.Xml   xmlwriter=new AMS.Profile.Xml("MediaPortal.xml"))
				{
					xmlwriter.SetValueAsBool("DVBSS2","enablePlugins",false);
				}
				MessageBox.Show("Plugins are disabled, DLL 'SoftCSA.dll' must be in MediaPortal-Folder!");
			}
		}

      // check if MediaPortal is already running...

      Log.Write("  Check if mediaportal is already started");
      m_UniqueIdentifier = Application.ExecutablePath.Replace("\\","_");
      m_Mutex = new System.Threading.Mutex(false, m_UniqueIdentifier);
      if (!m_Mutex.WaitOne(1, true))
      {
        Log.Write("  Check if mediaportal is already running...");
        string strMsg = "Mediaportal is already running!";

        // Find the other instance of MP (use System.Diagnostics. because Process is also a method in this class)
        string processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
        Process[] processes = System.Diagnostics.Process.GetProcessesByName(processName);

        // The array returned from the previous call will contain 2 processes, the already running
        // process and this process. Figure out what process the running one is and focus that window

        int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
        Process otherProcess;

        if (processes[0].Id == pid)
        {
           otherProcess = processes[1];
        }
        else
        {
           otherProcess = processes[0];
        }

        // bring the window of the other process to the foreground
        IntPtr hWnd = otherProcess.MainWindowHandle;
        ShowWindowAsync(hWnd, SW_RESTORE );
        SetForegroundWindow(hWnd);

        // Exit this process
        throw new Exception(strMsg);
      }

      Log.Write(@"  delete old log\capture.log file...");
      Utils.FileDelete(@"log\capture.log");
      if (Screen.PrimaryScreen.Bounds.Width > 720)
      {
        this.MinimumSize = new Size(720 + 8, 576 + 27);
      }
      else
      {
        this.MinimumSize = new Size(720, 576);
      }
			this.Text = "Media Portal";
      

      GUIGraphicsContext.form = this;
      GUIGraphicsContext.graphics = null;
			GUIGraphicsContext.RenderGUI=this;

      GUIWindowManager.OnNewAction += new OnActionHandler(this.OnAction);
      try
      {
        using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
        {
          m_strSkin = xmlreader.GetValueAsString("skin","name","mce");
          m_strLanguage = xmlreader.GetValueAsString("skin","language","English");
          m_bAutoHideMouse = xmlreader.GetValueAsBool("general","autohidemouse",false);
          GUIGraphicsContext.MouseSupport = xmlreader.GetValueAsBool("general","mousesupport",true);
					GUIGraphicsContext.DBLClickAsRightClick = xmlreader.GetValueAsBool("general","dblclickasrightclick",false);
        }
      }
      catch (Exception)
      {
        m_strSkin = "mce";
        m_strLanguage = "english";
      }
     
			Log.Write("  Check skin version");
			CheckSkinVersion();

      GUIWindowManager.Receivers += new SendMessageHandler(OnMessage);
      GUIWindowManager.Callbacks += new GUIWindowManager.OnCallBackHandler(this.Process);
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STARTING;

      // load keymapping from keymap.xml
      ActionTranslator.Load();

      // One time setup for proxy servers
      // also set credentials to allow use with firewalls that require them
      // this means we can use the .NET internet objects and not have
      // to worry about proxies elsewhere in the code
      System.Net.WebProxy proxy = System.Net.WebProxy.GetDefaultProxy();
      proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
      System.Net.GlobalProxySelection.Select = proxy;

      //register the playlistplayer for thread messages (like playback stopped,ended)
      Log.Write("  Init playlist player");
      PlayListPlayer.Init();

		//
		// Only load the USBUIRT device if it has been enabled in the configuration
		//
		using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
		{
			bool inputEnabled = xmlreader.GetValueAsString("USBUIRT", "internal", "false") == "true";
			bool outputEnabled = xmlreader.GetValueAsString("USBUIRT", "external", "false") == "true";

			if (inputEnabled == true || outputEnabled == true)
			{
				Log.Write("  Creating the USBUIRT device");
				this.usbuirtdevice = USBUIRT.Create(new USBUIRT.OnRemoteCommand(OnRemoteCommand));
				Log.Write("  done creating the USBUIRT device");
			}
			//Load Winlirc if enabled.
			//sd00//
			bool winlircInputEnabled = xmlreader.GetValueAsString("WINLIRC", "enabled", "false") == "true";
			if (winlircInputEnabled == true)
			{
				Log.Write("  creating the WINLIRC device");
				this.winlircdevice = new WinLirc();
				Log.Write("  done creating the WINLIRC device");
			}
			//sd00//
			inputEnabled = xmlreader.GetValueAsString("SerialUIR", "internal", "false") == "true";

			if (inputEnabled == true)
			{
				Log.Write("  creating the SerialUIR device");
				this.serialuirdevice = SerialUIR.Create(new SerialUIR.OnRemoteCommand(OnRemoteCommand));
				Log.Write("  done creating the SerialUIR device");
			}
		}

      //registers the player for video window size notifications
      Log.Write("  Init players");
      g_Player.Init();


      //  hook ProcessExit for a chance to clean up when closed peremptorily
      AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

      //  hook form close to stop updater too
      this.Closed += new EventHandler(MediaPortal_Closed);

      XmlDocument doc = new XmlDocument();
      try
      {
        doc.Load("mediaportal.exe.config");
        XmlNode node = doc.SelectSingleNode("/configuration/appStart/ClientApplicationInfo/appFolderName");
        node.InnerText = System.IO.Directory.GetCurrentDirectory();

        node = doc.SelectSingleNode("/configuration/appUpdater/UpdaterConfiguration/application/client/baseDir");
        node.InnerText = System.IO.Directory.GetCurrentDirectory();
        
        node = doc.SelectSingleNode("/configuration/appUpdater/UpdaterConfiguration/application/client/tempDir");
        node.InnerText = System.IO.Directory.GetCurrentDirectory();
        doc.Save("Mediaportal.exe.config");
      }
      catch (Exception)
      {
      }
			
			try
			{
#if DEBUG
#else
        UpdaterConfiguration config = UpdaterConfiguration.Instance;
				config.Logging.LogPath = System.IO.Directory.GetCurrentDirectory() + @"\log\updatelog.log";
				config.Applications[0].Client.BaseDir = System.IO.Directory.GetCurrentDirectory();
				config.Applications[0].Client.TempDir = System.IO.Directory.GetCurrentDirectory() + @"\temp";
				config.Applications[0].Client.XmlFile = System.IO.Directory.GetCurrentDirectory() + @"\MediaPortal.exe.config";
				config.Applications[0].Server.ServerManifestFileDestination = System.IO.Directory.GetCurrentDirectory() + @"\xml\ServerManifest.xml";
				
				try
				{
					System.IO.Directory.CreateDirectory("thumbs");
					System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"\temp");
					System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"\xml");
					System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"\log");
				}
				catch(Exception){}
				Utils.DeleteFiles(config.Applications[0].Client.BaseDir + @"\log", "*.log");
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
			}
			catch(Exception )
			{
			}
			using(AMS.Profile.Xml xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				m_iDateLayout = xmlreader.GetValueAsInt("home","datelayout",0);
			}
    }

    void RenderStats()
    {
      UpdateStats();
      if (m_bShowStats)
      {
        GUIFont font = GUIFontManager.GetFont(0);
        if (font != null)
        {
          font.DrawText(80, 40, 0xffffffff, frameStats, GUIControl.Alignment.ALIGN_LEFT);
          region[0].X = m_ixpos;
          region[0].Y = 0;
          region[0].Width = 4;
          region[0].Height = GUIGraphicsContext.Height;
          GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.FromArgb(255, 255, 255, 255), 1.0f, 0, region);
         
          float fStep = (GUIGraphicsContext.Width - 100);
          fStep /= (2f * 16f);

          fStep /= framePerSecond;
          m_iFrameCount++;
          if (m_iFrameCount >= (int)fStep)
          {
            m_iFrameCount = 0;
            m_ixpos += 12;
            if (m_ixpos > GUIGraphicsContext.Width - 50) m_ixpos = 50;
             
          }
        }
      }
    }

    public override bool PreProcessMessage(ref Message msg)
    {
      //if (msg.Msg==WM_KEYDOWN) Debug.WriteLine("pre keydown");

      return base.PreProcessMessage(ref msg);
    }

    protected override void WndProc(ref Message msg)
    {
      Action action;
      char key;
      Keys keyCode;
      if ( MCE2005Remote.WndProc(ref msg, out action, out  key, out keyCode) ) 
      {
        
        msg.Result = new IntPtr(0);
        if (action!=null && action.wID!=Action.ActionType.ACTION_INVALID)
        {
          Log.Write("action:{0} ", action.wID);
          if (ActionTranslator.GetActionDetail(GUIWindowManager.ActiveWindow, action))
          {
            if (action.SoundFileName.Length > 0)
              Utils.PlaySound(action.SoundFileName, false, true);
          }
          GUIGraphicsContext.OnAction(action);
        }
        
        if (keyCode !=Keys.A)
        {
          Log.Write("keycode:{0} ", keyCode.ToString());
          System.Windows.Forms.KeyEventArgs ke= new KeyEventArgs(keyCode);
          keydown(ke);
          return;
        }
        if (((int)key) !=0)
        {
          Log.Write("key:{0} ", key);
          System.Windows.Forms.KeyPressEventArgs e= new KeyPressEventArgs(key);
          keypressed(e);
          return;
        }
        return;
      }
		// plugins menu clicked?
		if(msg.Msg==0x111)
		{
			bool tmpPluginsFlag=false;
			using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
			{
				tmpPluginsFlag=xmlreader.GetValueAsBool("DVBSS2","enablePlugins",false);
			}
			if(tmpPluginsFlag==true)
				DVBGraphSS2.MenuItemClick(msg.WParam.ToInt32());
		}

      if (msg.Msg == WM_SYSCOMMAND && msg.WParam.ToInt32() == SC_SCREENSAVE)
      {
        // windows wants to activate the screensaver
        if (GUIGraphicsContext.IsFullScreenVideo || GUIWindowManager.ActiveWindow==(int)GUIWindow.Window.WINDOW_SLIDESHOW) 
        {
          //disable it when we're watching tv/movies/...
          msg.Result = new IntPtr(0);
          return;
        }
      }
      //if (msg.Msg==WM_KEYDOWN) Debug.WriteLine("msg keydown");
      g_Player.WndProc(ref msg);
      base.WndProc(ref msg);
    }

    /// <summary>
    /// Process() gets called when a dialog is presented.
    /// It contains the message loop 
    /// </summary>
    public void Process()
    {
      Application.DoEvents();
      FrameMove();
      FullRender();

    }

    public void RenderFrame()
    {
      try
      {
				CreateStateBlock();
        GUIWindowManager.Render();
        RenderStats();
      }
      catch (Exception ex)
      {
        Log.Write("RenderFrame exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
    }

    /// <summary>
    /// OnStartup() gets called just before the application starts
    /// </summary>
    protected override void OnStartup() 
    {
      // set window form styles
      // these styles enable double buffering, which results in no flickering
      SetStyle(ControlStyles.Opaque, true);
      SetStyle(ControlStyles.UserPaint, true);
      SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      SetStyle(ControlStyles.DoubleBuffer, false);

      // set process priority
      m_MouseTimeOut = DateTime.Now;
      //System.Threading.Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;

      Recorder.Start();
      AutoPlay.StartListening();


      using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
        string strDefault=xmlreader.GetValueAsString("myradio","default","");
        GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAY_RADIO_STATION,(int)GUIWindow.Window.WINDOW_RADIO,GUIWindowManager.ActiveWindow,0,0,0,null);
        msg.SendToTargetWindow=true;
        msg.Label=strDefault;
        GUIWindowManager.SendThreadMessage(msg);
      }
      if (splashScreen!=null) splashScreen.SetInformation("Starting plugins...");
      PluginManager.Load();
      PluginManager.Start();
      
      //
      // Kill the splash screen
      //
      if (splashScreen != null)
      {
        splashScreen.Close();
        splashScreen.Dispose();
        splashScreen = null;
      }

			tMouseClickTimer = new System.Timers.Timer(SystemInformation.DoubleClickTime);
			tMouseClickTimer.Enabled = false;
			tMouseClickTimer.Elapsed += new System.Timers.ElapsedEventHandler(tMouseClickTimer_Elapsed);
			tMouseClickTimer.SynchronizingObject = this;
    } 

    /// <summary>
    /// OnExit() Gets called just b4 application stops
    /// </summary>
    protected override void OnExit() 
    {
			if(serialuirdevice != null)
				serialuirdevice.Close();
      StopUpdater();
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
      // stop any file playback
      g_Player.Stop();
      
      // tell window manager that application is closing
      // this gives the windows the change to do some cleanup
      Recorder.Stop();

      AutoPlay.StopListening();
      
      PluginManager.Stop();

			if(tMouseClickTimer != null)
			{
				tMouseClickTimer.Stop();
				tMouseClickTimer.Dispose();
				tMouseClickTimer = null;
			}

      GUIFontManager.Dispose();
      GUIWindowManager.Clear();
      GUILocalizeStrings.Dispose();
    }


  static int prevwindow=0;
  static bool reentrant=false;
  protected override void Render() 
  { 
    if (reentrant) return;
    try
    {
      reentrant=true;
      // if there's no DX9 device (during resizing for exmaple) then just return
      if (GUIGraphicsContext.DX9Device == null) return;

      // clear the surface
      //if (prevwindow!=GUIWindowManager.ActiveWindow)
      {
        GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
        prevwindow=GUIWindowManager.ActiveWindow;
      }
			CreateStateBlock();
      GUIGraphicsContext.DX9Device.BeginScene();

      // ask the window manager to render the current active window
      GUIWindowManager.Render();
      RenderStats();
         
      GUIFontManager.Present();
      GUIGraphicsContext.DX9Device.EndScene();
      try
      {
        // Show the frame on the primary surface.
        GUIGraphicsContext.DX9Device.Present();//SLOW
      }
      catch (DeviceLostException)
      {
        g_Player.Stop();
        deviceLost = true;
      }
    }
    finally
    {
      reentrant=false;
    }
  }

  protected override void FrameMove()
  {
		try
		{
			// Set the date & time
			if (DateTime.Now.Minute != m_updateTimer.Minute)
			{
				m_updateTimer=DateTime.Now;
				GUIPropertyManager.SetProperty("#date",GetDate()); 	 
				GUIPropertyManager.SetProperty("#time",GetTime()); 	 
			}

			CheckForNewUpdate();
			Recorder.Process();
			g_Player.Process();
			GUIWindowManager.DispatchThreadMessages();
			GUIWindowManager.ProcessWindows();

			// update playing status
			if (g_Player.Playing)
			{
				GUIGraphicsContext.IsPlaying = true;
				GUIGraphicsContext.IsPlayingVideo = (g_Player.IsVideo || g_Player.IsTV);

				if (g_Player.Paused) GUIPropertyManager.SetProperty("#playlogo","logo_pause.png");
				else if (g_Player.Speed > 1) GUIPropertyManager.SetProperty("#playlogo", "logo_fastforward.png");
				else if (g_Player.Speed < 1) GUIPropertyManager.SetProperty("#playlogo", "logo_rewind.png");
				else if (g_Player.Playing) GUIPropertyManager.SetProperty("#playlogo", "logo_play.png");

				GUIPropertyManager.SetProperty("#currentplaytime",  Utils.SecondsToHMSString((int)g_Player.CurrentPosition));
				GUIPropertyManager.SetProperty("#shortcurrentplaytime", Utils.SecondsToShortHMSString((int)g_Player.CurrentPosition));
				if (g_Player.Duration>0)
				{
					GUIPropertyManager.SetProperty("#duration", Utils.SecondsToHMSString((int)g_Player.Duration));
					GUIPropertyManager.SetProperty("#shortduration", Utils.SecondsToShortHMSString((int)g_Player.Duration));
	        
					double fPercentage = g_Player.CurrentPosition / g_Player.Duration;
					int iPercent = (int)(100 * fPercentage);
					GUIPropertyManager.SetProperty("#percentage", iPercent.ToString());
				}
				else
				{
					GUIPropertyManager.SetProperty("#duration", String.Empty);
					GUIPropertyManager.SetProperty("#shortduration", String.Empty);
					GUIPropertyManager.SetProperty("#percentage", "0");
				}
				GUIPropertyManager.SetProperty("#playspeed", g_Player.Speed.ToString());
			}
			else
			{
				if (!Recorder.View)
					GUIGraphicsContext.IsFullScreenVideo = false;
				GUIGraphicsContext.IsPlaying = false;
			}
			if (!g_Player.Playing && !Recorder.IsRecording)
			{
				if (m_bPlayingState)
				{
					GUIPropertyManager.RemovePlayerProperties();
				}
				m_bPlayingState = false;
			}
			else 
			{
				m_bPlayingState = true;
			}
	/*
			// disable TV preview when playing a movie
			if (g_Player.Playing && g_Player.HasVideo)
			{
				if (!g_Player.IsTV)
				{
					Recorder.View = false;
				}
			}*/
		}
		catch (System.IO.FileNotFoundException ex)
		{
			System.Windows.Forms.MessageBox.Show("File not found:"+ex.FileName, "MediaPortal", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
			Close();
		}
  }


  /// <summary>
  /// The device has been created.  Resources that are not lost on
  /// Reset() can be created here -- resources in Pool.Default,
  /// Pool.Scratch, or Pool.SystemMemory.  Image surfaces created via
  /// CreateImageSurface are never lost and can be created here.  Vertex
  /// shaders and pixel shaders can also be created here as they are not
  /// lost on Reset().
  /// </summary>
    protected override void InitializeDeviceObjects()
		{
      GUIWindowManager.Clear();
      GUITextureManager.Dispose();
      GUIFontManager.Dispose();

			if (splashScreen!=null) splashScreen.SetInformation("Loading keymap.xml...");
      ActionTranslator.Load();
      
      if (splashScreen!=null) splashScreen.SetInformation("Loading strings...");
      GUIGraphicsContext.Skin = @"skin\" + m_strSkin;
      GUIGraphicsContext.ActiveForm = this.Handle;
      GUILocalizeStrings.Load(@"language\" + m_strLanguage + @"\strings.xml");
      
      if (splashScreen!=null) splashScreen.SetInformation("Loading fonts...");
      GUIFontManager.LoadFonts(@"skin\" + m_strSkin + @"\fonts.xml");
      GUIFontManager.InitializeDeviceObjects();
      GUIFontManager.RestoreDeviceObjects();
      
      if (splashScreen!=null) splashScreen.SetInformation("Loading skin...");
      Log.Write("  Load skin {0}", m_strSkin);
      GUIWindowManager.Initialize();
      
      if (splashScreen!=null) splashScreen.SetInformation("Loading window plugins...");
      PluginManager.LoadWindowPlugins();
      
      if (splashScreen!=null) splashScreen.SetInformation("Initializing skin...");
      Log.Write("  WindowManager.Preinitialize");
      GUIWindowManager.PreInit();
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;

			Log.Write("  WindowManager.Load");
			GUIGraphicsContext.Load();

			Log.Write("  WindowManager.ActivateWindow");
			GUIWindowManager.ActivateWindow(GUIWindowManager.ActiveWindow);
			
			Log.Write("  skin initalized");
      if (GUIGraphicsContext.DX9Device != null)
      {
        Log.Write("  DX9 size: {0}x{1}", GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth, GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight);
        Log.Write("  video ram left:{0} KByte", GUIGraphicsContext.DX9Device.AvailableTextureMemory / 1024);
      }

      MCE2005Remote.Init(GUIGraphicsContext.ActiveForm);

      SetupCamera2D();

			g_nAnisotropy=GUIGraphicsContext.DX9Device.DeviceCaps.MaxAnisotropy;
			supportsFiltering=Manager.CheckDeviceFormat(
				GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal, 
				GUIGraphicsContext.DX9Device.DeviceCaps.DeviceType, 
				GUIGraphicsContext.DX9Device.DisplayMode.Format, 
				Usage.RenderTarget | Usage.QueryFilter, ResourceType.Textures, 
				Format.A8R8G8B8);
    }

    /// <summary>
    /// The device exists, but may have just been Reset().  Resources in
    /// Pool.Default and any other device state that persists during
    /// rendering should be set here.  Render states, matrices, textures,
    /// etc., that don't change during rendering can be set once here to
    /// avoid redundant state setting during Render() or FrameMove().
    /// </summary>
    protected override void OnDeviceReset(System.Object sender, System.EventArgs e) 
	{
		//
		// Only perform the device reset if we're not shutting down MediaPortal.
		//
		if (GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.STOPPING)
		{
			Log.Write("OnDeviceReset()");
			//g_Player.Stop();
			GUIGraphicsContext.Load();
			GUIFontManager.Dispose();
			GUIFontManager.LoadFonts(@"skin\" + m_strSkin + @"\fonts.xml");
			GUIFontManager.InitializeDeviceObjects();
			if (GUIGraphicsContext.DX9Device != null)
			{
				GUIWindowManager.Restore();
				GUIWindowManager.PreInit();
				GUIWindowManager.ActivateWindow(GUIWindowManager.ActiveWindow);
				GUIWindowManager.OnDeviceRestored();
				GUIGraphicsContext.Load();
			}
			Log.Write(" done");
			g_Player.PositionX++;
			g_Player.PositionX--;
		}
	}

    void OnAction(Action action)
    {
      int iCurrent, iMin, iMax, iStep;
		try
		{
			GUIWindow window;
			switch (action.wID)
			{
				case Action.ActionType.ACTION_RECORD:
					// record current program
					GUIWindow tvHome = GUIWindowManager.GetWindow( (int)GUIWindow.Window.WINDOW_TV);
					if (tvHome!=null)
					{
						if (tvHome.GetID!=GUIWindowManager.ActiveWindow)
						{
							tvHome.OnAction(action); 
							return;
						}
					}
					break;
				case Action.ActionType.ACTION_DVD_MENU:
					if (g_Player.IsDVD && g_Player.Playing)
					{
						g_Player.OnAction(action);
						return;
					}
					break;
				case Action.ActionType.ACTION_NEXT_CHAPTER:
					if (g_Player.IsDVD && g_Player.Playing)
					{
						g_Player.OnAction(action);
						return;
					}
					break;
				case Action.ActionType.ACTION_PREV_CHAPTER:
					if (g_Player.IsDVD && g_Player.Playing)
					{
						g_Player.OnAction(action);
						return;
					}
					break;
				case Action.ActionType.ACTION_PREV_CHANNEL:
					window=(GUIWindow )GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
					window.OnAction(action);
					return;
        
				case Action.ActionType.ACTION_NEXT_CHANNEL:
					window=(GUIWindow )GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
					window.OnAction(action);
					return;

				case Action.ActionType.ACTION_TOGGLE_WINDOWED_FULSLCREEN:
					ToggleFullWindowed(); 
					return;
					//break;

				case Action.ActionType.ACTION_VOLUME_DOWN : 
					iCurrent = AudioMixerHelper.GetMinMaxVolume(out iMin, out iMax);
					iStep = (iMax - iMin) / 10;
					iCurrent -= iStep;
					if (iCurrent < iMin) iCurrent = iMin;
					AudioMixerHelper.SetVolume(iCurrent);
					return;
          

				case Action.ActionType.ACTION_VOLUME_UP : 
					iCurrent = AudioMixerHelper.GetMinMaxVolume(out iMin, out iMax);
					iStep = (iMax - iMin) / 10;
					iCurrent += iStep;
					if (iCurrent > iMax) iCurrent = iMax;
					AudioMixerHelper.SetVolume(iCurrent);
					break;
            
				case Action.ActionType.ACTION_BACKGROUND_TOGGLE : 
					//show livetv or video as background instead of the static GUI background
					// toggle livetv/video in background on/pff
					if (GUIGraphicsContext.ShowBackground)
					{
						Log.Write("Use live TV as background");
						// if on, but we're not playing any video or watching tv
						if (GUIGraphicsContext.Vmr9Active)
						{
							GUIGraphicsContext.ShowBackground = false;
							GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Stretch;
						}
						else
						{
							bool ok=false;
							for (int i=0; i < Recorder.Count;++i)
							{
								if (Recorder.DoesCardSupportTimeshifting(i))
								{
									if (Recorder.IsCardRecording(i)) 
									{
										ok=true;
										break;
									}
									else
									{
										string channel=Recorder.GetTVChannelName(i);
										Recorder.StartViewing(i,channel,true,true);
										if (GUIGraphicsContext.Vmr9Active)
										{
											ok=true;
											break;
										}
										else
										{
											Recorder.StartViewing(i,channel,false,false);
										}
									}
								}
							}
							if (!ok)
							{
								GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING,0,0,0,0,0,0);
								msg.Param1=727;
								msg.Param2=728;
								msg.Param3=729;
								GUIWindowManager.SendMessage(msg);
								return;
							}
							GUIGraphicsContext.ShowBackground = false;
							GUIGraphicsContext.ARType=MediaPortal.GUI.Library.Geometry.Type.Stretch;
						}
					}
					else
					{
						Log.Write("Use GUI as background");
						GUIGraphicsContext.ShowBackground = true;
						for (int i=0; i < Recorder.Count;++i)
						{
							if (Recorder.IsCardViewing(i) && !Recorder.IsCardRecording(i))
							{
								string channel=Recorder.GetTVChannelName(i);
								Recorder.StartViewing(i,String.Empty,false,false);
							}
						}
					}
					return;

				case Action.ActionType.ACTION_EXIT : 
					GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
					return;

				case Action.ActionType.ACTION_REBOOT : 
				{
					//reboot
					GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO,0,0,0,0,0,0);
					msg.Param1=630;
					msg.Param2=0;
					msg.Param3=0;

					if (msg.Param1==1)
					{
						WindowsController.ExitWindows(RestartOptions.Reboot, true);
						GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
					}
				}
					return;

				case Action.ActionType.ACTION_EJECTCD : 
					Utils.EjectCDROM();
					return;

				case Action.ActionType.ACTION_SHUTDOWN : 
				{
					GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO,0,0,0,0,0,0);
					msg.Param1=631;
					msg.Param2=0;
					msg.Param3=0;
					GUIWindowManager.SendMessage(msg);

					if (msg.Param1==1)
					{
						WindowsController.ExitWindows(RestartOptions.PowerOff, true);
						GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
					}
					break;
				}
						
			}

			if (g_Player.Playing)
			{
				switch (action.wID)
				{
					case Action.ActionType.ACTION_SHOW_GUI : 
						if (!GUIGraphicsContext.IsFullScreenVideo)
						{
							GUIWindow win = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
							if (win.FullScreenVideoAllowed)
							{
								if (!g_Player.IsTV)
								{
									if (g_Player.HasVideo)
									{
										GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
										GUIGraphicsContext.IsFullScreenVideo = true;
										return;
									}
								}
								else
								{
									GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
									GUIGraphicsContext.IsFullScreenVideo = true;
									return;
								}
							}
						}
						break;
	          
					case Action.ActionType.ACTION_PREV_ITEM :
						if (ActionTranslator.HasKeyMapped(GUIWindowManager.ActiveWindow,action.m_key))
						{
							return;
						}
						PlayListPlayer.PlayPrevious();
						break;

					case Action.ActionType.ACTION_NEXT_ITEM :
						if (ActionTranslator.HasKeyMapped(GUIWindowManager.ActiveWindow,action.m_key))
						{
							return;
						}
						PlayListPlayer.PlayNext(true);
						break;

					case Action.ActionType.ACTION_STOP : 
						if (!GUIGraphicsContext.IsFullScreenVideo)
						{
							g_Player.Stop();
							return;
						}
						break;

					case Action.ActionType.ACTION_MUSIC_PLAY : 
						if (!GUIGraphicsContext.IsFullScreenVideo)
						{
							g_Player.StepNow();
							g_Player.Speed=1;
							if (g_Player.Paused) g_Player.Pause();

							return;
						}
						break;
	          
					case Action.ActionType.ACTION_PAUSE : 
						if (!GUIGraphicsContext.IsFullScreenVideo)
						{
							g_Player.Pause();
							return;
						}
						break;

					case Action.ActionType.ACTION_PLAY : 
						if (!GUIGraphicsContext.IsFullScreenVideo)
						{
							if (g_Player.Speed != 1)
							{
								g_Player.Speed = 1;
							}
							if (g_Player.Paused) g_Player.Pause();
							return;
						}
						break;

	          
					case Action.ActionType.ACTION_MUSIC_FORWARD : 
						if (!GUIGraphicsContext.IsFullScreenVideo)
						{
							g_Player.Speed = Utils.GetNextForwardSpeed(g_Player.Speed);
							return;
						}
						break;
					case Action.ActionType.ACTION_MUSIC_REWIND : 
						if (!GUIGraphicsContext.IsFullScreenVideo)
						{
							g_Player.Speed = Utils.GetNextRewindSpeed(g_Player.Speed);
							return;
						}
						break;
				}
			}
			GUIWindowManager.OnAction(action);
		}
		catch (System.IO.FileNotFoundException ex)
		{
			System.Windows.Forms.MessageBox.Show("File not found:"+ex.FileName, "MediaPortal", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
			Close();
		}
		catch (Exception ex)
		{
			Log.Write("  exception: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
			throw new Exception("exception occured",ex);
		}
    }

		protected override void keypressed(System.Windows.Forms.KeyPressEventArgs e)
		{
			char keyc = e.KeyChar;

      Log.Write("key:{0} 0x{1:X}", (int)keyc,(int)keyc);
			Key key = new Key(e.KeyChar, 0);
      if (key.KeyChar == '!') m_bShowStats = !m_bShowStats;
			if (key.KeyChar == '?')
      {

        GC.Collect();GC.Collect();GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();GC.Collect();GC.Collect();
      }
			if (key.KeyChar=='f')
			{
				GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,1,0,null);
				GUIWindowManager.SendMessage(msg);
			}
			if (key.KeyChar=='w')
			{
				GUIMessage msg =new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED,0,0,0,0,0,null);
				GUIWindowManager.SendMessage(msg);
			}
			Action action = new Action();
      if (GUIWindowManager.IsRouted && 
        (GUIWindowManager.RoutedWindow == (int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD ||
         GUIWindowManager.RoutedWindow == (int)GUIWindow.Window.WINDOW_VIRTUAL_SEARCH_KEYBOARD) ||
				 GUIWindowManager.RoutedWindow == (int)GUIWindow.Window.WINDOW_TVMSNOSD ||
				 GUIWindowManager.RoutedWindow == (int)GUIWindow.Window.WINDOW_MSNOSD)
      {
        action = new Action(key, Action.ActionType.ACTION_KEY_PRESSED, 0, 0);
        GUIGraphicsContext.OnAction(action);
        return;
      }

			if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow, key, ref action))
      {
        if (action.SoundFileName.Length > 0)
          Utils.PlaySound(action.SoundFileName, false, true);
				GUIGraphicsContext.OnAction(action);
			}
      action = new Action(key, Action.ActionType.ACTION_KEY_PRESSED, 0, 0);
      GUIGraphicsContext.OnAction(action);
		}

		protected override void keydown(System.Windows.Forms.KeyEventArgs e)
		{
				Key key = new Key(0, (int)e.KeyCode);
				Action action = new Action();
				if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow, key, ref action))
				{
          if (action.SoundFileName.Length > 0)
            Utils.PlaySound(action.SoundFileName, false, true);
					GUIGraphicsContext.OnAction(action);
				}
      
	  }



  protected override void OnMouseWheel(MouseEventArgs e)
  {
    // Calculate Mouse position
    Point ptClientUL = new Point();
    Point ptScreenUL = new Point();

    ptScreenUL.X = Cursor.Position.X;
    ptScreenUL.Y = Cursor.Position.Y;
    ptClientUL = this.PointToClient(ptScreenUL);

    int iCursorX = ptClientUL.X;
    int iCursorY = ptClientUL.Y;

    float fX = ((float)GUIGraphicsContext.Width) / ((float)this.ClientSize.Width);
    float fY = ((float)GUIGraphicsContext.Height) / ((float)this.ClientSize.Height);
    float x = (fX * ((float)iCursorX)) - GUIGraphicsContext.OffsetX;
    float y = (fY * ((float)iCursorY)) - GUIGraphicsContext.OffsetY;

    if (e.Delta>0)
    {
        Action action = new Action(Action.ActionType.ACTION_MOVE_UP, x, y);
      action.MouseButton = e.Button;
      GUIGraphicsContext.OnAction(action);
    }
    else if (e.Delta<0)
    {
        Action action = new Action(Action.ActionType.ACTION_MOVE_DOWN, x, y);
      action.MouseButton = e.Button;
      GUIGraphicsContext.OnAction(action);
    }
    base.OnMouseWheel (e);
  }

	protected override void mousemove(System.Windows.Forms.MouseEventArgs e)
  {
    base.mousemove(e);
    if (!m_bShowCursor) return;

    // Calculate Mouse position
    Point ptClientUL = new Point();
    Point ptScreenUL = new Point();

    ptScreenUL.X = Cursor.Position.X;
    ptScreenUL.Y = Cursor.Position.Y;
    ptClientUL = this.PointToClient(ptScreenUL);

    int iCursorX = ptClientUL.X;
    int iCursorY = ptClientUL.Y;

    if (m_iLastMousePositionX != iCursorX || m_iLastMousePositionY != iCursorY)
    {
			// check any still waiting single click events
			if (GUIGraphicsContext.DBLClickAsRightClick)
			{
				if ((Math.Abs(m_iLastMousePositionX - iCursorX) > 10) || (Math.Abs(m_iLastMousePositionY - iCursorY) > 10))
				  CheckSingleClick();
			}

      // Save last position
      m_iLastMousePositionX = iCursorX;
      m_iLastMousePositionY = iCursorY;

      //this.Text=String.Format("show {0},{1} {2},{3}",e.X,e.Y,m_iLastMousePositionX,m_iLastMousePositionY);

      float fX = ((float)GUIGraphicsContext.Width) / ((float)this.ClientSize.Width);
      float fY = ((float)GUIGraphicsContext.Height) / ((float)this.ClientSize.Height);
      float x = (fX * ((float)iCursorX)) - GUIGraphicsContext.OffsetX;
      float y = (fY * ((float)iCursorY)) - GUIGraphicsContext.OffsetY;
      
      GUIWindow window = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
      if (window != null)
      {
        Action action = new Action(Action.ActionType.ACTION_MOUSE_MOVE, x, y);
        action.MouseButton = e.Button;
        GUIGraphicsContext.OnAction(action);        
      }
    }
	}

	protected override void mouseclick(MouseEventArgs e)
	{
    base.mouseclick(e);
    if (!m_bShowCursor) return;
    Action actionMove;
		Action action;
		bool MouseButtonRightClick = false;

    // Calculate Mouse position
    Point ptClientUL = new Point();
    Point ptScreenUL = new Point();

    ptScreenUL.X = Cursor.Position.X;
    ptScreenUL.Y = Cursor.Position.Y;
    ptClientUL = this.PointToClient(ptScreenUL);

    int iCursorX = ptClientUL.X;
    int iCursorY = ptClientUL.Y;

    // first move mouse
    float fX = ((float)GUIGraphicsContext.Width) / ((float)this.ClientSize.Width);
    float fY = ((float)GUIGraphicsContext.Height) / ((float)this.ClientSize.Height);
    float x = (fX * ((float)iCursorX)) - GUIGraphicsContext.OffsetX;
    float y = (fY * ((float)iCursorY)) - GUIGraphicsContext.OffsetY; ;
    actionMove = new Action(Action.ActionType.ACTION_MOUSE_MOVE, x, y);
    GUIGraphicsContext.OnAction(actionMove);

		if (e.Button == MouseButtons.Left)
		{		
			if (GUIGraphicsContext.DBLClickAsRightClick)
			{
				if(tMouseClickTimer != null)
				{
					bMouseClickFired = false;

					if(e.Clicks < 2)
					{
						eLastMouseClickEvent = e;
						bMouseClickFired = true;
						tMouseClickTimer.Start();
						return;
					}
					else
					{
						// Double click used as right click
						eLastMouseClickEvent = null;
						tMouseClickTimer.Stop();
						MouseButtonRightClick = true;
					}
				}
			}
			else
			{
				action = new Action(Action.ActionType.ACTION_MOUSE_CLICK, x, y);
				action.MouseButton = e.Button;
				action.SoundFileName = "click.wav";
				if (action.SoundFileName.Length > 0)
					Utils.PlaySound(action.SoundFileName, false, true);

				GUIGraphicsContext.OnAction(action);
				return;
			}
		}   

    // right mouse button=back
    if ((e.Button == MouseButtons.Right) || (MouseButtonRightClick))
    {
			GUIWindow window = (GUIWindow) GUIWindowManager.GetWindow( GUIWindowManager.ActiveWindow );
			if ((window.GetFocusControlId() != -1) ||	GUIGraphicsContext.IsFullScreenVideo ||
				  (GUIWindowManager.ActiveWindow==(int)GUIWindow.Window.WINDOW_SLIDESHOW))
			{
				// Get context menu
				action = new Action(Action.ActionType.ACTION_CONTEXT_MENU, x, y);
				action.MouseButton = e.Button;
				action.SoundFileName = "click.wav";
				if (action.SoundFileName.Length > 0)
					Utils.PlaySound(action.SoundFileName, false, true);

				GUIGraphicsContext.OnAction(action);
			}
			else
			{
				Key key = new Key(0, (int)Keys.Escape);
				action = new Action();
				if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow, key, ref action))
				{
					if (action.SoundFileName.Length > 0)
						Utils.PlaySound(action.SoundFileName, false, true);
					GUIGraphicsContext.OnAction(action);
					return;
				}      
			}
    }

    //middle mouse button=Y
    if (e.Button == MouseButtons.Middle)
    {
      Key key = new Key('y',0);
      action = new Action();
      if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow, key, ref action))
      {
        if (action.SoundFileName.Length > 0)
          Utils.PlaySound(action.SoundFileName, false, true);
        GUIGraphicsContext.OnAction(action);
				return;
      }
    }	

    // Save last position
    m_iLastMousePositionX = iCursorX;
    m_iLastMousePositionY = iCursorY;
	}
	
	private void tMouseClickTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
	{
		CheckSingleClick();
	}

	void CheckSingleClick()
	{
		Action action;

		if(tMouseClickTimer != null)
		{
			tMouseClickTimer.Stop();
			if(bMouseClickFired)
			{
        float fX = ((float)GUIGraphicsContext.Width) / ((float)this.ClientSize.Width);
        float fY = ((float)GUIGraphicsContext.Height) / ((float)this.ClientSize.Height);
        float x = (fX * ((float)m_iLastMousePositionX)) - GUIGraphicsContext.OffsetX;
        float y = (fY * ((float)m_iLastMousePositionY)) - GUIGraphicsContext.OffsetY;

				bMouseClickFired = false;
				action = new Action(Action.ActionType.ACTION_MOUSE_CLICK, x, y);
				action.MouseButton = eLastMouseClickEvent.Button;
				action.SoundFileName = "click.wav";
				if (action.SoundFileName.Length > 0)
					Utils.PlaySound(action.SoundFileName, false, true);

				GUIGraphicsContext.OnAction(action);
			}
		}
	}

  private void MediaPortal_Closed(object sender, EventArgs e)
  {
    StopUpdater();
  }

		
  private void CurrentDomain_ProcessExit(object sender, EventArgs e)
  {
    StopUpdater();
  }
  private delegate void MarshalEventDelegate(object sender, UpdaterActionEventArgs e);

	//---------------------------------------------------
  private void OnUpdaterDownloadStartedHandler(object sender, UpdaterActionEventArgs e) 
  {		
    Log.Write("update:Download started for:{0}",e.ApplicationName);
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
      Log.Write("update:User canceled download");
      m_bCancelVersion = true;
      m_bNewVersionAvailable = false;
      return;
    }
    m_bCancelVersion = false;
    m_bNewVersionAvailable = false;
  }

  private void OnUpdaterUpdateAvailable(object sender, UpdaterActionEventArgs e)
  {
    Log.Write("update:new version available:{0}", e.ApplicationName);
    m_strNewVersion = e.ServerInformation.AvailableVersion;
    m_bNewVersionAvailable = true;
    while (m_bNewVersionAvailable) System.Threading.Thread.Sleep(100);
    if (m_bCancelVersion)
    {
      _updater.StopUpdater(e.ApplicationName);
    }
  }

  //---------------------------------------------------
  private void OnUpdaterDownloadCompletedHandler(object sender, UpdaterActionEventArgs e)
  {
    Log.Write("update:Download Completed.");
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


  //---------------------------------------------------
  private void StartNewVersion()
  {
    Log.Write("update:start appstart.exe");
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
    Log.Write("update:stop mp...");
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

  private void OnMessage(GUIMessage message)
  {
    switch (message.Message)
    {
      case GUIMessage.MessageType.GUI_MSG_CD_INSERTED:
        AutoPlay.ExamineCD(message.Label);
      break;

      case GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL : 
				bool bIsInteger;
				double retNum;	
				bIsInteger = Double.TryParse(message.Label, System.Globalization.NumberStyles.Integer,System.Globalization.NumberFormatInfo.InvariantInfo, out retNum );
        try
        {
          if(bIsInteger) //sd00//
          usbuirtdevice.ChangeTunerChannel(message.Label);
        }
        catch (Exception) {}
				try
				{
					winlircdevice.ChangeTunerChannel(message.Label);
				}
        catch (Exception) {}
      break;



      case GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED : 
        bool fullscreen=false;
        if (message.Param1!=0) fullscreen=true;
        message.Param1=0;//not full screen
        if (isMaximized == false) 
        {
          return;
        }
        if (GUIGraphicsContext.CurrentState==GUIGraphicsContext.State.STOPPING) return;


        if (fullscreen)
        {
          //switch to fullscreen mode
					Log.Write("goto fullscreen:{0}",GUIGraphicsContext.DX9Device.PresentationParameters.Windowed);
          if (!GUIGraphicsContext.DX9Device.PresentationParameters.Windowed) 
          {
            message.Param1=1;
            return;
          }
          SwitchFullScreenOrWindowed(false,true);
          GUIFontManager.RestoreDeviceObjects();
          if (!GUIGraphicsContext.DX9Device.PresentationParameters.Windowed) 
          {
            message.Param1=1;
            return;
          }
        }
        else
        {
          //switch to windowed mode
					Log.Write("goto windowed:{0}",GUIGraphicsContext.DX9Device.PresentationParameters.Windowed);
          if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed) return;
          SwitchFullScreenOrWindowed(true,true);
          GUIFontManager.RestoreDeviceObjects();
        }

      break;
    }
  }

	void CreateStateBlock()
	{
		GUIGraphicsContext.DX9Device.RenderState.ZBufferEnable = false;
		GUIGraphicsContext.DX9Device.RenderState.AlphaBlendEnable = true;
		GUIGraphicsContext.DX9Device.RenderState.SourceBlend = Blend.SourceAlpha;
		GUIGraphicsContext.DX9Device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
		//GUIGraphicsContext.DX9Device.RenderState.AlphaTestEnable = true;
		GUIGraphicsContext.DX9Device.RenderState.FillMode = FillMode.Solid;
		GUIGraphicsContext.DX9Device.RenderState.CullMode = Cull.CounterClockwise;
		GUIGraphicsContext.DX9Device.RenderState.StencilEnable = false;
		//GUIGraphicsContext.DX9Device.RenderState.Clipping = true;
		GUIGraphicsContext.DX9Device.ClipPlanes.DisableAll();
		GUIGraphicsContext.DX9Device.RenderState.VertexBlend = VertexBlend.Disable;
		GUIGraphicsContext.DX9Device.RenderState.IndexedVertexBlendEnable = false;
		GUIGraphicsContext.DX9Device.RenderState.FogEnable = false;
		//GUIGraphicsContext.DX9Device.RenderState.ColorWriteEnable = ColorWriteEnable.RedGreenBlueAlpha;
		GUIGraphicsContext.DX9Device.TextureState[0].ColorOperation = TextureOperation.Modulate;
		GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
		GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
		GUIGraphicsContext.DX9Device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
		GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
		GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument2 = TextureArgument.Diffuse;
		GUIGraphicsContext.DX9Device.TextureState[0].TextureCoordinateIndex = 0;
		GUIGraphicsContext.DX9Device.TextureState[0].TextureTransform = TextureTransform.Disable; // REVIEW
		GUIGraphicsContext.DX9Device.TextureState[1].ColorOperation = TextureOperation.Disable;
		GUIGraphicsContext.DX9Device.TextureState[1].AlphaOperation = TextureOperation.Disable;
		//GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter = TextureFilter.None;
		//GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter = TextureFilter.None;
		//GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter = TextureFilter.None;

		if (supportsFiltering)
		{
			GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter=TextureFilter.Linear;
			GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter=TextureFilter.Linear;
			GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter=TextureFilter.Linear;
			GUIGraphicsContext.DX9Device.SamplerState[0].MaxAnisotropy=g_nAnisotropy;
    	      
			GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter=TextureFilter.Linear;
			GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter=TextureFilter.Linear;
			GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter=TextureFilter.Linear;
			GUIGraphicsContext.DX9Device.SamplerState[1].MaxAnisotropy=g_nAnisotropy;
		}
		else
		{
			GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter=TextureFilter.Point;
			GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter=TextureFilter.Point;
			GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter=TextureFilter.Point;
    	      
			GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter=TextureFilter.Point;
			GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter=TextureFilter.Point;
			GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter=TextureFilter.Point;
		}

	}

	/// <summary>
	/// Get the current date from the system and localize it based on the user preferences.
	/// </summary>
	/// <returns>A string containing the localized version of the date.</returns>
	protected string GetDate()
	{
		DateTime cur=DateTime.Now;
		string day;
		switch (cur.DayOfWeek)
		{
			case DayOfWeek.Monday :	day = GUILocalizeStrings.Get(11);	break;
			case DayOfWeek.Tuesday :	day = GUILocalizeStrings.Get(12);	break;
			case DayOfWeek.Wednesday :	day = GUILocalizeStrings.Get(13);	break;
			case DayOfWeek.Thursday :	day = GUILocalizeStrings.Get(14);	break;
			case DayOfWeek.Friday :	day = GUILocalizeStrings.Get(15);	break;
			case DayOfWeek.Saturday :	day = GUILocalizeStrings.Get(16);	break;
			default:	day = GUILocalizeStrings.Get(17);	break;
		}

		string month;
		switch (cur.Month)
		{
			case 1 :	month= GUILocalizeStrings.Get(21);	break;
			case 2 :	month= GUILocalizeStrings.Get(22);	break;
			case 3 :	month= GUILocalizeStrings.Get(23);	break;
			case 4 :	month= GUILocalizeStrings.Get(24);	break;
			case 5 :	month= GUILocalizeStrings.Get(25);	break;
			case 6 :	month= GUILocalizeStrings.Get(26);	break;
			case 7 :	month= GUILocalizeStrings.Get(27);	break;
			case 8 :	month= GUILocalizeStrings.Get(28);	break;
			case 9 :	month= GUILocalizeStrings.Get(29);	break;
			case 10:	month= GUILocalizeStrings.Get(30);	break;
			case 11:	month= GUILocalizeStrings.Get(31);	break;
			default:	month= GUILocalizeStrings.Get(32);	break;
		}

		string strDate=String.Format("{0} {1} {2}",day, cur.Day, month);
		if (m_iDateLayout==1)
		{
			strDate=String.Format("{0} {1} {2}",day, month, cur.Day);
		}
		return strDate;
	}
		
	/// <summary>
	/// Get the current time from the system.
	/// </summary>
	/// <returns>A string containing the current time.</returns>
	// TODO: Localize the time settings based on the user preferences
	protected string GetTime()
	{
		DateTime cur=DateTime.Now;
		string strTime=cur.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat);
		return strTime;
	}


	protected void CheckSkinVersion()
	{
		OldSkinForm form = new OldSkinForm();
		if (form.CheckSkinVersion(m_strSkin)) return;
		form.ShowDialog(this);
	}
 
}
