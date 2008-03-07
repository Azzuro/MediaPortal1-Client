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
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using Microsoft.Win32;
using AMS.Profile;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using MediaPortal.Configuration;
using TvDatabase;
using TvControl;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.DVB;
using Gentle.Common;
using Gentle.Framework;
namespace TvPlugin
{
  /// <summary>v
  /// Summary description for Class1.
  /// </summary>
  public class TVHome : GUIWindow, ISetupForm, IShowPlugin, IPluginReceiver
  {
    #region constants
    private const int HEARTBEAT_INTERVAL = 5; //seconds
    private const int WM_POWERBROADCAST = 0x0218;
    private const int PBT_APMRESUMESUSPEND = 0x0007;
    private const int PBT_APMRESUMESTANDBY = 0x0008;
    #endregion

    #region variables
    enum Controls
    {
      IMG_REC_CHANNEL = 21,
      LABEL_REC_INFO = 22,
      IMG_REC_RECTANGLE = 23,

    };
    //heartbeat related stuff
    private Thread heartBeatTransmitterThread = null;

    static DateTime _updateProgressTimer = DateTime.MinValue;
    static ChannelNavigator m_navigator;
    static TVUtil _util;
    static VirtualCard _card = null;
    DateTime _updateTimer = DateTime.Now;
    static bool _autoTurnOnTv = false;
    //int _lagtolerance = 10; //Added by joboehl
    public static bool settingsLoaded = false;
    DateTime _dtlastTime = DateTime.Now;
    TvCropManager _cropManager = new TvCropManager();
    TvNotifyManager _notifyManager = new TvNotifyManager();
    static string[] _preferredLanguages;
    static bool _preferAC3 = false;
    static bool _preferAudioTypeOverLang = false;
    static bool _autoFullScreen = false;
    static bool _autoFullScreenOnly = false;
    static bool _resumed = false;
    static bool _showlastactivemodule = false;
    static bool _showlastactivemoduleFullscreen = false;
    static bool _playbackStopped = false;
    static bool _onPageLoadDone = false;
    static bool _userChannelChanged = false;
    static private bool _doingHandleServerNotConnected = false;
    static private bool _doingChannelChange = false;
    //Stopwatch benchClock = null;

    [SkinControlAttribute(2)]
    protected GUIButtonControl btnTvGuide = null;
    [SkinControlAttribute(3)]
    protected GUIButtonControl btnRecord = null;
    // [SkinControlAttribute(6)]     protected GUIButtonControl btnGroup = null;
    [SkinControlAttribute(7)]
    protected GUIButtonControl btnChannel = null;
    [SkinControlAttribute(8)]
    protected GUIToggleButtonControl btnTvOnOff = null;
    [SkinControlAttribute(13)]
    protected GUIButtonControl btnTeletext = null;
    //    [SkinControlAttribute(14)]    protected GUIButtonControl btnTuningDetails = null;
    [SkinControlAttribute(24)]
    protected GUIImage imgRecordingIcon = null;
    [SkinControlAttribute(99)]
    protected GUIVideoControl videoWindow = null;
    [SkinControlAttribute(9)]
    protected GUIButtonControl btnActiveStreams = null;
    static bool _connected = false;

    static protected TvServer _server;

    #endregion

    #region Events
    #endregion

    static TVHome()
    {
      try
      {
        m_navigator = new ChannelNavigator();
      }
      catch (Exception ex)
      {
        MediaPortal.GUI.Library.Log.Error(ex);
      }
      LoadSettings();
    }

    public TVHome()
    {
      //ServiceProvider services = GlobalServiceProvider.Instance;

      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
      MediaPortal.GUI.Library.Log.Info("TVHome V" + versionInfo.FileVersion + ":ctor");
      GetID = (int)GUIWindow.Window.WINDOW_TV;
      Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

      startHeartBeatThread();
    }

    void Application_ApplicationExit(object sender, EventArgs e)
    {
      try
      {
        if (TVHome.Card.IsTimeShifting)
        {
          if (!TVHome.Card.IsRecording)
          {
            TVHome.Card.User.Name = new User().Name;
            TVHome.Card.StopTimeShifting();
          }
        }
        stopHeartBeatThread();
      }
      catch (Exception)
      {
      }
    }

    private void HeartBeatTransmitter()
    {
      while (true)
      {
        if (TVHome.Connected && TVHome.Card.IsTimeShifting)
        {
          // send heartbeat to tv server each 5 sec.
          // this way we signal to the server that we are alive thus avoid being kicked.
          // Log.Debug("TVHome: sending HeartBeat signal to server.");

          // when debugging we want to disable heartbeats
#if !DEBUG
          try
          {
            RemoteControl.Instance.HeartBeat(TVHome.Card.User);
          }
          catch (Exception e)
          {
            Log.Error("TVHome: failed sending HeartBeat signal to server. ({0})", e.Message);
          }
#endif
        }
        else if (TVHome.Connected && !TVHome.Card.IsTimeShifting && !_playbackStopped && _onPageLoadDone && (!g_Player.IsMusic && !g_Player.IsDVD && !g_Player.IsRadio && !g_Player.IsVideo))
        {
          // check the possible reason why timeshifting has suddenly stopped
          // maybe the server kicked the client b/c a recording on another transponder was due.

          TvStoppedReason result = TVHome.Card.GetTimeshiftStoppedReason;
          if (result != TvStoppedReason.UnknownReason)
          {
            Log.Debug("TVHome: Timeshifting seems to have stopped - TvStoppedReason:{0}", result);
            string errMsg = "";

            switch (result)
            {
              case TvStoppedReason.HeartBeatTimeOut:
                errMsg = GUILocalizeStrings.Get(1515);
                break;
              case TvStoppedReason.KickedByAdmin:
                errMsg = GUILocalizeStrings.Get(1514);
                break;
              case TvStoppedReason.RecordingStarted:
                errMsg = GUILocalizeStrings.Get(1513);
                break;
              default:
                errMsg = GUILocalizeStrings.Get(1516);
                break;
            }

            GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);

            if (pDlgOK != null)
            {
              pDlgOK.SetHeading(GUILocalizeStrings.Get(605) + " - " + TVHome.Navigator.CurrentChannel);//my tv
              errMsg = errMsg.Replace("\\r", "\r");
              string[] lines = errMsg.Split('\r');
              //pDlgOK.SetLine(1, TVHome.Navigator.CurrentChannel);

              for (int i = 0; i < lines.Length; i++)
              {
                string line = lines[i];
                pDlgOK.SetLine(1 + i, line);
              }
              pDlgOK.DoModal(GUIWindowManager.ActiveWindowEx);
            }
            Action keyAction = new Action(Action.ActionType.ACTION_STOP, 0, 0);
            GUIGraphicsContext.OnAction(keyAction);
            _playbackStopped = true;
          }
        }
        Thread.Sleep(HEARTBEAT_INTERVAL * 1000); //sleep for 5 secs. before sending heartbeat again
      }
    }

    private void startHeartBeatThread()
    {
      // setup heartbeat transmitter thread.						
      // thread already running, then leave it.
      if (heartBeatTransmitterThread != null)
      {
        if (heartBeatTransmitterThread.IsAlive)
        {
          return;
        }
      }
      Log.Debug("TVHome: HeartBeat Transmitter started.");
      heartBeatTransmitterThread = new Thread(HeartBeatTransmitter);
      heartBeatTransmitterThread.Start();
    }

    private void stopHeartBeatThread()
    {
      if (heartBeatTransmitterThread != null)
      {
        if (heartBeatTransmitterThread.IsAlive)
        {
          Log.Debug("TVHome: HeartBeat Transmitter stopped.");
          heartBeatTransmitterThread.Abort();
        }
      }
    }

    static public bool DoingChannelChange()
    {
      return _doingChannelChange;
    }

    static public bool HandleServerNotConnected()
    {
      // _doingHandleServerNotConnected is used to avoid multiple calls to this method.
      // the result could be that the dialogue is not shown.

      if (_doingHandleServerNotConnected) return TVHome.Connected;
      _doingHandleServerNotConnected = true;
      bool remConnected = RemoteControl.IsConnected;

      // we just did a successful connect      
      if (remConnected && !TVHome.Connected)
      {
        GUIMessage initMsg = null;
        initMsg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, (int)GUIWindow.Window.WINDOW_TV_OVERLAY, 0, 0, 0, 0, null);
        GUIWindowManager.SendThreadMessage(initMsg);
      }

      TVHome.Connected = remConnected;

      if (!TVHome.Connected)
      {
        g_Player.Stop();
        TVHome.Card.User.Name = new User().Name;

        if (g_Player.FullScreen)
        {
          GUIMessage initMsgTV = null;
          initMsgTV = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, (int)GUIWindow.Window.WINDOW_TV, 0, 0, 0, 0, null);
          GUIWindowManager.SendThreadMessage(initMsgTV);

          _doingHandleServerNotConnected = false;
          return true;
        }

        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);

        if (pDlgOK != null)
        {
          pDlgOK.Reset();
          pDlgOK.SetHeading(605);//my tv
          pDlgOK.SetLine(1, TVHome.Navigator.CurrentChannel);
          pDlgOK.SetLine(2, GUILocalizeStrings.Get(1510)); //Connection to TV server lost

          //pDlgOK.DoModal(GUIWindowManager.ActiveWindowEx);

          ParameterizedThreadStart pThread = new ParameterizedThreadStart(ShowDlg);
          Thread showDlgThread = new Thread(pThread);

          // show the dialog asynch.
          // this fixes a hang situation that would happen when resuming TV with showlastactivemodule
          showDlgThread.Start(pDlgOK);
        }
        _doingHandleServerNotConnected = false;
        return true;
      }

      _doingHandleServerNotConnected = false;
      return false;
    }

    public override void OnAdded()
    {
      MediaPortal.GUI.Library.Log.Info("TVHome:OnAdded");

      // replace g_player's ShowFullScreenWindowTV
      g_Player.ShowFullScreenWindowTV = ShowFullScreenWindowTVHandler;

      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_TV, this);
      Restore();
      PreInit();
      ResetAllControls();
      Connected = RemoteControl.IsConnected;
    }
    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    static public bool UserChannelChanged
    {
      set
      {
        _userChannelChanged = value;
      }
    }

    static public TVUtil Util
    {
      get
      {
        if (_util == null) _util = new TVUtil();
        return _util;
      }
    }
    static public TvServer TvServer
    {
      get
      {
        if (_server == null)
        {
          _server = new TvServer();
        }
        return _server;
      }
    }
    static public bool Connected
    {
      get
      {
        return _connected;
      }
      set
      {
        _connected = value;
      }
    }
    static public VirtualCard Card
    {
      get
      {
        if (_card == null)
        {
          User user = new User();
          _card = TvServer.CardByIndex(user, 0);
        }
        return _card;
      }
      set
      {
        if (_card != null)
        {
          bool stop = true;
          if (value != null)
          {
            if (value.Id == _card.Id || value.Id == -1) stop = false;
          }
          if (stop)
          {
            _card.User.Name = new User().Name;
            _card.StopTimeShifting();
          }
          _card = value;
        }
      }
    }

    #region Serialisation
    static void LoadSettings()
    {
      if (settingsLoaded) return;
      settingsLoaded = true;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        m_navigator.LoadSettings(xmlreader);
        _autoTurnOnTv = xmlreader.GetValueAsBool("mytv", "autoturnontv", false);
        _showlastactivemodule = xmlreader.GetValueAsBool("general", "showlastactivemodule", false);
        _showlastactivemoduleFullscreen = xmlreader.GetValueAsBool("general", "lastactivemodulefullscreen", false);

        string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "normal");
        if (strValue.Equals("zoom")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom;
        if (strValue.Equals("stretch")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Stretch;
        if (strValue.Equals("normal")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Normal;
        if (strValue.Equals("original")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Original;
        if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
        if (strValue.Equals("panscan")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.PanScan43;

        string preferredLanguages = xmlreader.GetValueAsString("tvservice", "preferredaudiolanguages", "");
        _preferredLanguages = preferredLanguages.Split(';');

        _preferAC3 = xmlreader.GetValueAsBool("tvservice", "preferac3", false);
        _preferAudioTypeOverLang = xmlreader.GetValueAsBool("tvservice", "preferAudioTypeOverLang", true);
        _autoFullScreen = xmlreader.GetValueAsBool("mytv", "autofullscreen", false);
        _autoFullScreenOnly = xmlreader.GetValueAsBool("mytv", "autofullscreenonly", false);
      }
    }

    void SaveSettings()
    {
      if (m_navigator != null)
      {
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          m_navigator.SaveSettings(xmlwriter);
        }
      }
    }
    #endregion

    #region overrides
    /// <summary>
    /// Gets called by the runtime when a  window will be destroyed
    /// Every window window should override this method and cleanup any resources
    /// </summary>
    /// <returns></returns>
    public override void DeInit()
    {
      OnPageDestroy(-1);
    }

    public override bool SupportsDelayedLoad
    {
      get
      {
        return false;
      }
    }
    public override bool Init()
    {
      MediaPortal.GUI.Library.Log.Info("TVHome:Init");
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvhomeServer.xml");
      GetID = (int)GUIWindow.Window.WINDOW_TV;

      g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayBackStarted);
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayBackStopped);
      g_Player.AudioTracksReady += new g_Player.AudioTracksReadyHandler(OnAudioTracksReady);

      GUIWindowManager.Receivers += new SendMessageHandler(OnGlobalMessage);
      return bResult;
    }

    static public void OnGlobalMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_RECORDER_TUNE_RADIO:
          {
            TvBusinessLayer layer = new TvBusinessLayer();
            Channel channel = layer.GetChannelByName(message.Label);
            if (channel != null)
            {
              TVHome.ViewChannelAndCheck(channel);
            }
            break;
          }
        case GUIMessage.MessageType.GUI_MSG_STOP_SERVER_TIMESHIFTING:
          {
            User user = new User();
            if (user.Name == TVHome.Card.User.Name) { TVHome.Card.StopTimeShifting(); };
            break;
          }
      }
    }

    void OnAudioTracksReady()
    {
      Log.Debug("TVHome.OnAudioTracksReady()");
      int prefLangIdx = TVHome.GetPreferedAudioStreamIndex();
      g_Player.CurrentAudioStream = prefLangIdx;
    }

    void OnPlayBackStarted(g_Player.MediaType type, string filename)
    {
      // when we are watching TV and suddenly decides to watch a audio/video etc., we want to make sure that the TV is stopped on server.
      GUIWindow currentWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
      if (currentWindow.IsTv) return;
      if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_RADIO) return;

      TVHome.Card.StopTimeShifting();
    }

    void OnPlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type != g_Player.MediaType.TV && type != g_Player.MediaType.Radio) return;

      //GUIWindow currentWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
      //if (currentWindow.IsTv) return;
      if (TVHome.Card.IsTimeShifting == false) return;
      if (TVHome.Card.IsRecording == true) return;
      TVHome.Card.User.Name = new User().Name;
      TVHome.Card.StopTimeShifting();
      _playbackStopped = true;
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_RECORD:
          //record current program on current channel
          //are we watching tv?
          if (GUIWindowManager.ActiveWindow == (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          {
            MediaPortal.GUI.Library.Log.Info("send message to fullscreen tv");
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORD, GUIWindowManager.ActiveWindow, 0, 0, 0, 0, null);
            msg.SendToTargetWindow = true;
            msg.TargetWindowId = (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN;
            GUIGraphicsContext.SendMessage(msg);
            return;
          }

          MediaPortal.GUI.Library.Log.Info("TVHome:Record action");
          TvServer server = new TvServer();
          string channel = TVHome.Card.ChannelName;
          VirtualCard card;
          if (false == server.IsRecording(channel, out card))
          {
            TvBusinessLayer layer = new TvBusinessLayer();
            Program prog = null;
            if (Navigator.Channel != null)
              prog = Navigator.Channel.CurrentProgram;
            if (prog != null)
            {
              GUIDialogMenuBottomRight pDlgOK = (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
              if (pDlgOK != null)
              {
                pDlgOK.Reset();
                pDlgOK.SetHeading(605);//my tv
                pDlgOK.AddLocalizedString(875); //current program
                pDlgOK.AddLocalizedString(876); //till manual stop
                pDlgOK.DoModal(GUIWindowManager.ActiveWindow);
                switch (pDlgOK.SelectedId)
                {
                  case 875:
                    {
                      //record current program
                      Schedule newSchedule = new Schedule(Navigator.Channel.IdChannel, Navigator.Channel.CurrentProgram.Title,
                                                  Navigator.Channel.CurrentProgram.StartTime, Navigator.Channel.CurrentProgram.EndTime);
                      newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                      newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);

                      newSchedule.RecommendedCard = TVHome.Card.Id;

                      newSchedule.Persist();
                      server.OnNewSchedule();
                    }
                    break;

                  case 876:
                    {
                      Schedule newSchedule = new Schedule(Navigator.Channel.IdChannel, GUILocalizeStrings.Get(413) + " (" + Navigator.Channel.DisplayName + ")",
                                                  DateTime.Now, DateTime.Now.AddDays(1));
                      newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                      newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
                      newSchedule.RecommendedCard = TVHome.Card.Id;

                      newSchedule.Persist();
                      server.OnNewSchedule();
                    }
                    break;
                }
              }
            }//if (prog != null)
            else
            {
              Schedule newSchedule = new Schedule(Navigator.Channel.IdChannel, GUILocalizeStrings.Get(413) + " (" + Navigator.Channel.DisplayName + ")",
                                                  DateTime.Now, DateTime.Now.AddDays(1));
              newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
              newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
              newSchedule.RecommendedCard = TVHome.Card.Id;

              newSchedule.Persist();
              server.OnNewSchedule();

            }
          }//if (false == server.IsRecording(channel, out Card))
          else
          {
            server.StopRecordingSchedule(card.RecordingScheduleId);
          }
          break;

        case Action.ActionType.ACTION_PREV_CHANNEL:
          TVHome.OnPreviousChannel();
          break;
        case Action.ActionType.ACTION_PAGE_DOWN:
          TVHome.OnPreviousChannel();
          break;

        case Action.ActionType.ACTION_NEXT_CHANNEL:
          TVHome.OnNextChannel();
          break;
        case Action.ActionType.ACTION_PAGE_UP:
          TVHome.OnNextChannel();
          break;

        case Action.ActionType.ACTION_LAST_VIEWED_CHANNEL:  // mPod
          TVHome.OnLastViewedChannel();
          break;

        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            // goto home 
            // are we watching tv & doing timeshifting

            // No, then stop viewing... 
            //g_Player.Stop();
            GUIWindowManager.ShowPreviousWindow();
            return;
          }

        case Action.ActionType.ACTION_KEY_PRESSED:
          {
            if ((char)action.m_key.KeyChar == '0')
              OnLastViewedChannel();
          }
          break;
        case Action.ActionType.ACTION_SHOW_GUI:
          {
            // If we are in tvhome and TV is currently off and no fullscreen TV then turn ON TV now!
            if (!g_Player.IsTimeShifting && !g_Player.FullScreen)
            {
              OnClicked(8, btnTvOnOff, MediaPortal.GUI.Library.Action.ActionType.ACTION_MOUSE_CLICK); //8=togglebutton
            }
            break;
          }
      }
      base.OnAction(action);
    }

    private void OnResume()
    {
      Log.Debug("TVHome.OnResume()");
      _resumed = true;
    }

    public void Start()
    {
      Log.Debug("TVHome.Start()");
    }

    public void Stop()
    {
      Log.Debug("TVHome.Stop()");
    }


    public bool WndProc(ref System.Windows.Forms.Message msg)
    {
      if (msg.Msg == WM_POWERBROADCAST)
      {

        switch (msg.WParam.ToInt32())
        {
          case PBT_APMRESUMESUSPEND:
            Log.Info("TVHome.WndProc(): Windows has resumed from hibernate mode");
            OnResume();
            break;
          case PBT_APMRESUMESTANDBY:
            Log.Info("TVHome.WndProc(): Windows has resumed from standby mode");
            OnResume();
            break;
        }
        return true;
      }
      return false;
    }

    private static bool wasPrevWinTVplugin()
    {
      bool result = false;

      int act = GUIWindowManager.ActiveWindow;
      int prev = GUIWindowManager.GetWindow(act).PreviousWindowId;

      //plz any newly added ID's to this list.

      result = (
                prev == (int)GUIWindow.Window.WINDOW_TV_CROP_SETTINGS ||
                prev == (int)GUIWindow.Window.WINDOW_SETTINGS_SORT_CHANNELS ||
                prev == (int)GUIWindow.Window.WINDOW_SETTINGS_TV_EPG ||
                prev == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN ||
                prev == (int)GUIWindow.Window.WINDOW_TVGUIDE ||
                prev == (int)GUIWindow.Window.WINDOW_MINI_GUIDE ||
                prev == (int)GUIWindow.Window.WINDOW_TV_SEARCH ||
                prev == (int)GUIWindow.Window.WINDOW_TV_SEARCHTYPE ||
                prev == (int)GUIWindow.Window.WINDOW_TV_SCHEDULER_PRIORITIES ||
                prev == (int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO ||
                prev == (int)GUIWindow.Window.WINDOW_RECORDEDTV ||
                prev == (int)GUIWindow.Window.WINDOW_TV_RECORDED_INFO ||
                prev == (int)GUIWindow.Window.WINDOW_SETTINGS_RECORDINGS ||
                prev == (int)GUIWindow.Window.WINDOW_SCHEDULER ||
                prev == (int)GUIWindow.Window.WINDOW_SEARCHTV ||
                prev == (int)GUIWindow.Window.WINDOW_TV_TUNING_DETAILS
        );

      return result;
    }

    protected override void OnPageLoad()
    {
      // when suspending MP while watching fullscreen TV, the player is stopped ok, but it returns to tvhome, which starts timeshifting.
      // this could lead the tv server timeshifting even though client is asleep.
      // although we have to make sure that resuming again activates TV, this is done by checking previous window ID.      

      // disabled currently as pausing the graph stops GUI repainting
      //GUIWaitCursor.Show();
      if (GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow).PreviousWindowId != (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
      {
        _playbackStopped = false;
      }

      btnActiveStreams.Label = GUILocalizeStrings.Get(692);

      if (!RemoteControl.IsConnected)
      {
        if (!_onPageLoadDone)
        {
          RemoteControl.Clear();
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SETTINGS_TVENGINE);
        }
        else
        {
          bool res = HandleServerNotConnected();
          UpdateStateOfButtons();
          UpdateProgressPercentageBar();
          UpdateRecordingIndicator();
        }
        //GUIWaitCursor.Hide();
        return;
      }
      else
      {
        TVHome.Connected = true;
      }

      try
      {
        int cards = RemoteControl.Instance.Cards;
      }
      catch (Exception)
      {
        RemoteControl.Clear();
      }

      try
      {
        IList cards = TvDatabase.Card.ListAll();
      }
      catch (Exception)
      {
        // lets try one more time - seems like the gentle framework is not properly initialized when coming out of standby/hibernation.        
        if (TVHome.Connected && RemoteControl.IsConnected)
        {
          //lets wait 10 secs before giving up.
          DateTime now = DateTime.Now;
          TimeSpan ts = now - DateTime.Now;
          bool success = false;

          while (ts.TotalSeconds > -10 && !success)
          {
            try
            {
              IList cards = TvDatabase.Card.ListAll();
              success = true;
            }
            catch (Exception)
            {
              success = false;
            }
            ts = now - DateTime.Now;
          }

          if (!success)
          {
            RemoteControl.Clear();
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SETTINGS_TVENGINE);
            //GUIWaitCursor.Hide();
            return;
          }

        }
        else
        {
          RemoteControl.Clear();
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SETTINGS_TVENGINE);
          //GUIWaitCursor.Hide();
          return;
        }
      }

      //stop the old recorder.
      //DatabaseManager.Instance.DefaultQueryStrategy = QueryStrategy.DataSourceOnly;
      GUIMessage msgStopRecorder = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msgStopRecorder);

      if (!_onPageLoadDone && m_navigator != null)
      {
        m_navigator.ReLoad();
      }

      if (m_navigator == null)
      {
        m_navigator = new ChannelNavigator();			// Create the channel navigator (it will load groups and channels)
      }

      base.OnPageLoad();

      //set video window position
      if (videoWindow != null)
      {
        GUIGraphicsContext.VideoWindow = new Rectangle(videoWindow.XPosition, videoWindow.YPosition, videoWindow.Width, videoWindow.Height);
      }

      // start viewing tv... 
      GUIGraphicsContext.IsFullScreenVideo = false;
      Channel channel = Navigator.Channel;
      if (channel == null || channel.IsRadio)
      {
        if (Navigator.CurrentGroup != null && Navigator.Groups.Count > 0)
        {
          Navigator.SetCurrentGroup(Navigator.Groups[0].GroupName);
          GUIPropertyManager.SetProperty("#TV.Guide.Group", Navigator.Groups[0].GroupName);
        }
        if (Navigator.CurrentGroup != null)
        {
          if (Navigator.CurrentGroup.ReferringGroupMap().Count > 0)
          {
            GroupMap gm = (GroupMap)Navigator.CurrentGroup.ReferringGroupMap()[0];
            channel = gm.ReferencedChannel();
          }
        }
      }

      if (channel != null)
      {
        /*
        if (TVHome.Card.IsTimeShifting)
        {
          int id = TVHome.Card.IdChannel;
          if (id >= 0)
          {
            channel = Channel.Retrieve(id);
          }
        }
        */
        MediaPortal.GUI.Library.Log.Info("tv home init:{0}", channel.DisplayName);
        if (_autoTurnOnTv && !_playbackStopped && !wasPrevWinTVplugin())
        {
          if (!wasPrevWinTVplugin())
          {
            _userChannelChanged = false;
          }

          ViewChannelAndCheck(channel);
        }
        GUIPropertyManager.SetProperty("#TV.Guide.Group", Navigator.CurrentGroup.GroupName);
        MediaPortal.GUI.Library.Log.Info("tv home init:{0} done", channel.DisplayName);
      }

      // if using showlastactivemodule feature and last module is fullscreen while returning from powerstate, then do not set fullscreen here (since this is done by the resume last active module feature)
      // we depend on the onresume method, thats why tvplugin now impl. the IPluginReceiver interface.      
      bool showlastActModFS = (_showlastactivemodule && _showlastactivemoduleFullscreen && _resumed && _autoTurnOnTv);
      bool useDelay = false;

      if (_resumed && !showlastActModFS)
      {
        useDelay = true;
        showlastActModFS = false;
      }
      else if (_resumed)
      {
        showlastActModFS = true;
      }
      if (!showlastActModFS && (g_Player.IsTV || g_Player.IsTVRecording))
      {
        if (_autoFullScreen && !g_Player.FullScreen && (!wasPrevWinTVplugin()))
        {
          Log.Debug("TVHome.OnPageLoad(): setting autoFullScreen");
          //if we are resuming from standby with tvhome, we want this in fullscreen, but we need a delay for it to work.
          if (useDelay)
          {
            Thread tvDelayThread = new Thread(TvDelayThread);
            tvDelayThread.Start();
          }
          else //no delay needed here, since this is when the system is being used normally
          {
            // wait for timeshifting to complete
            /*
            int waits = 0;
            while (_playbackStopped && waits < 100)
            {
              //Log.Debug("TVHome.OnPageLoad(): waiting for timeshifting to start");
              Thread.Sleep(100);
              waits++;
            }
            if (!_playbackStopped)
            {
              //Log.Debug("TVHome.OnPageLoad(): timeshifting has started - waits: {0}", waits);
              g_Player.ShowFullScreenWindow();
            }
            */
            g_Player.ShowFullScreenWindow();
          }
        }
        else if (_autoFullScreenOnly && !g_Player.FullScreen && (PreviousWindowId == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN))
        {
          Log.Debug("TVHome.OnPageLoad(): autoFullScreenOnly set, returning to previous window");
          GUIWindowManager.ShowPreviousWindow();
        }
      }

      _onPageLoadDone = true;
      _resumed = false;
      //GUIWaitCursor.Hide();
    }

    private void TvDelayThread()
    {
      //we have to use a small delay before calling tvfullscreen.                                    
      Thread.Sleep(200);

      // wait for timeshifting to complete
      int waits = 0;
      while (_playbackStopped && waits < 100)
      {
        //Log.Debug("TVHome.OnPageLoad(): waiting for timeshifting to start");
        Thread.Sleep(100);
        waits++;
      }
      if (!_playbackStopped)
      {
        //Log.Debug("TVHome.OnPageLoad(): timeshifting has started - waits: {0}", waits);
        g_Player.ShowFullScreenWindow();
      }
    }


    protected override void OnPageDestroy(int newWindowId)
    {
      //if we're switching to another plugin
      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        //and we're not playing which means we dont timeshift tv
        //g_Player.Stop();
      }
      if (RemoteControl.IsConnected)
      {
        SaveSettings();
      }
      base.OnPageDestroy(newWindowId);
    }

    static public void OnSelectGroup()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(971); // group
      int selected = 0;

      for (int i = 0; i < Navigator.Groups.Count; ++i)
      {
        dlg.Add(Navigator.Groups[i].GroupName);
        if (Navigator.Groups[i].GroupName == Navigator.CurrentGroup.GroupName)
        {
          selected = i;
        }
      }
      dlg.SelectedLabel = selected;
      dlg.DoModal(GUIWindowManager.ActiveWindow);
      if (dlg.SelectedLabel < 0)
        return;

      Navigator.SetCurrentGroup(dlg.SelectedLabelText);
      GUIPropertyManager.SetProperty("#TV.Guide.Group", dlg.SelectedLabelText);

      //if (Navigator.CurrentGroup != null)
      //{
      //  if (Navigator.CurrentGroup.ReferringGroupMap().Count > 0)
      //  {
      //    GroupMap gm = (GroupMap)Navigator.CurrentGroup.ReferringGroupMap()[0];
      //  }
      //}
    }

    void OnSelectChannel()
    {
      Stopwatch benchClock = null;
      benchClock = Stopwatch.StartNew();
      TvMiniGuide miniGuide = (TvMiniGuide)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MINI_GUIDE);
      miniGuide.AutoZap = false;
      miniGuide.SelectedChannel = Navigator.Channel;
      miniGuide.DoModal(GetID);

      //Only change the channel if the channel selectd is actually different. 
      //Without this, a ChannelChange might occur even when MiniGuide is canceled. 
      if (!miniGuide.Canceled)
      {
        //_userChannelChanged = true;
        ViewChannelAndCheck(miniGuide.SelectedChannel);
      }

      benchClock.Stop();
      Log.Debug("TVHome.OnSelecChannel(): Total Time {0} ms", benchClock.ElapsedMilliseconds.ToString());
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      Stopwatch benchClock = null;
      benchClock = Stopwatch.StartNew();

      if (HandleServerNotConnected())
      {
        UpdateStateOfButtons();
        UpdateProgressPercentageBar();
        UpdateRecordingIndicator();
        return;
      }

      if (control == btnActiveStreams)
      {
        OnActiveStreams();
      }
      if (control == btnTvOnOff)
      {
        if (TVHome.Card.IsTimeShifting && g_Player.IsTV && g_Player.Playing)
        {
          //tv off
          Log.Info("TVHome:turn tv off");
          SaveSettings();
          g_Player.Stop();
          TVHome.Card.User.Name = new User().Name;
          TVHome.Card.StopTimeShifting();
          benchClock.Stop();
          Log.Warn("TVHome.OnClicked(): EndTvOff {0} ms", benchClock.ElapsedMilliseconds.ToString());

          return;
        }
        else
        {
          // tv on
          Log.Info("TVHome:turn tv on {0}", Navigator.CurrentChannel);

          //stop playing anything
          if (g_Player.Playing)
          {
            if (g_Player.IsTV && !g_Player.IsTVRecording)
            {
              //already playing tv...
            }
            else
            {
              Log.Warn("TVHome.OnClicked: Stop Called - {0} ms", benchClock.ElapsedMilliseconds.ToString());
              g_Player.Stop(true);
            }
          }
          SaveSettings();
        }

        // turn tv on/off        
        if (Navigator.Channel.IsTv)
        {
          ViewChannelAndCheck(Navigator.Channel);
        }
        else // current channel seems to be non-tv (radio ?), get latest known tv channel from xml config and use this instead
        {
          MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml");
          string currentchannelName = xmlreader.GetValueAsString("mytv", "channel", String.Empty);
          Channel currentChannel = Navigator.GetChannel(currentchannelName);
          ViewChannelAndCheck(currentChannel);
        }

        UpdateStateOfButtons();
        UpdateProgressPercentageBar();
        benchClock.Stop();
        Log.Warn("TVHome.OnClicked(): Total Time - {0} ms", benchClock.ElapsedMilliseconds.ToString());

      }
      /*
      if (control == btnGroup)
      {
        OnSelectGroup();
      }*/
      if (control == btnTeletext)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TELETEXT);
        return;
      }
      //if (control == btnTuningDetails)
      //{
      //  GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV_TUNING_DETAILS);
      //}

      if (control == btnRecord)
      {
        OnRecord();
      }
      if (control == btnChannel)
      {
        OnSelectChannel();
      }
      base.OnClicked(controlId, control, actionType);
    }


    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_RESUME_TV:
          {
            if (_autoTurnOnTv && !wasPrevWinTVplugin()) // we only want to resume TV if previous window is NOT a tvplugin based one. (ex. tvguide.)
            {
              //restart viewing...  
              MediaPortal.GUI.Library.Log.Info("tv home msg resume tv:{0}", Navigator.CurrentChannel);
              ViewChannel(Navigator.Channel);
            }
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_RECORDER_VIEW_CHANNEL:
          MediaPortal.GUI.Library.Log.Info("tv home msg view chan:{0}", message.Label);
          {
            TvBusinessLayer layer = new TvBusinessLayer();
            Channel ch = layer.GetChannelByName(message.Label);
            ViewChannel(ch);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_VIEWING:
          {
            MediaPortal.GUI.Library.Log.Info("tv home msg stop chan:{0}", message.Label);
            TvBusinessLayer layer = new TvBusinessLayer();
            Channel ch = layer.GetChannelByName(message.Label);
            ViewChannel(ch);
          }
          break;
      }
      return base.OnMessage(message);
    }

    public override void Process()
    {
      TimeSpan ts = DateTime.Now - _updateTimer;
      if (ts.TotalMilliseconds < 1000) return;

      // BAV, 02.03.08: a channel change should not be delayed by rendering.
      //                by moving thisthe 1 min delays in zapping should be fixed
      // Let the navigator zap channel if needed
      Navigator.CheckChannelChange();

      //TVHome.SendHeartBeat(); //not needed, now sent from tvoverlay.cs

      if (GUIGraphicsContext.InVmr9Render) return;
      
      UpdateRecordingIndicator();
      if (btnChannel.Disabled != false)
        btnChannel.Disabled = false;
      //if (btnGroup.Disabled != false)
      //btnGroup.Disabled = false;
      if (btnRecord.Disabled != false)
        btnRecord.Disabled = false;
      //btnTeletext.Visible = false;
      bool isTimeShifting = !TVHome.Connected || TVHome.Card.IsTimeShifting;
      if (btnTvOnOff.Selected != (g_Player.Playing && g_Player.IsTV && isTimeShifting))
      {
        btnTvOnOff.Selected = (g_Player.Playing && g_Player.IsTV && isTimeShifting);
      }
      if (g_Player.Playing == false)
      {
        UpdateProgressPercentageBar();
        //if (btnTuningDetails!=null)
        //  btnTuningDetails.Visible = false;
        if (btnTeletext.Visible)
          btnTeletext.Visible = false;
        return;
      }
      //else
      //  if (btnTuningDetails!=null)
      //    btnTuningDetails.Visible = true;
      if (btnChannel.Disabled != false)
        btnChannel.Disabled = false;
      //if (btnGroup.Disabled != false)
      // btnGroup.Disabled = false;
      if (btnRecord.Disabled != false)
        btnRecord.Disabled = false;

      bool hasTeletext = !TVHome.Connected || TVHome.Card.HasTeletext;
      if (btnTeletext.IsVisible != hasTeletext)
      {
        btnTeletext.IsVisible = hasTeletext;
      }
      // BAV, 02.03.08
      //Navigator.CheckChannelChange();

      // Update navigator with information from the Recorder
      // TODO: This should ideally be handled using events. Recorder should fire an event
      // when the current channel changes. This is a temporary workaround //Vic
      string currchan = Navigator.CurrentChannel;		// Remember current channel
      Navigator.UpdateCurrentChannel();
      bool channelchanged = currchan != Navigator.CurrentChannel;

      UpdateStateOfButtons();
      UpdateProgressPercentageBar();
      UpdateRecordingIndicator();
      GUIControl.HideControl(GetID, (int)Controls.LABEL_REC_INFO);
      GUIControl.HideControl(GetID, (int)Controls.IMG_REC_RECTANGLE);
      GUIControl.HideControl(GetID, (int)Controls.IMG_REC_CHANNEL);
      _updateTimer = DateTime.Now;
    }

    #endregion

    /// <summary>
    /// This function replaces g_player.ShowFullScreenWindowTV
    /// </summary>
    /// <returns></returns>
    private static bool ShowFullScreenWindowTVHandler()
    {
      if (g_Player.IsTV && Card.IsTimeShifting)
      {
        // watching TV
        if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          return true;
        Log.Info("TVHome: ShowFullScreenWindow switching to fullscreen tv");
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
        GUIGraphicsContext.IsFullScreenVideo = true;
        return true;
      }
      return g_Player.ShowFullScreenWindowTVDefault();
    }

    /// <summary>
    /// check if we have a single seat environment
    /// </summary>
    /// <returns></returns>
    public static bool IsSingleSeat()
    {
      Log.Debug("TvFullScreen: IsSingleSeat - RemoteControl.HostName = {0} / Environment.MachineName = {1}", RemoteControl.HostName, Environment.MachineName);
      return (RemoteControl.HostName.ToLowerInvariant() == Environment.MachineName.ToLowerInvariant());
    }

    public static void UpdateTimeShift()
    {
    }

    void OnActiveStreams()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(692); // Active Tv Streams
      int selected = 0;

      IList cards = TvDatabase.Card.ListAll();
      List<Channel> channels = new List<Channel>();
      int count = 0;
      TvServer server = new TvServer();
      List<User> _users = new List<User>();
      foreach (Card card in cards)
      {
        if (card.Enabled == false) continue;
        if (!RemoteControl.Instance.CardPresent(card.IdCard)) continue;
        User[] users = RemoteControl.Instance.GetUsersForCard(card.IdCard);
        if (users == null) return;
        for (int i = 0; i < users.Length; ++i)
        {
          User user = users[i];
          if (card.IdCard != user.CardId)
          {
            continue;
          }
          bool isRecording;
          bool isTimeShifting;
          VirtualCard tvcard = new VirtualCard(user, RemoteControl.HostName);
          isRecording = tvcard.IsRecording;
          isTimeShifting = tvcard.IsTimeShifting;
          if (isTimeShifting || (isRecording && !isTimeShifting))
          {
            int idChannel = tvcard.IdChannel;
            user = tvcard.User;
            Channel ch = Channel.Retrieve(idChannel);
            channels.Add(ch);
            GUIListItem item = new GUIListItem();
            item.Label = ch.DisplayName;
            item.Label2 = user.Name;
            string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, ch.DisplayName);
            if (!System.IO.File.Exists(strLogo))
            {
              strLogo = "defaultVideoBig.png";
            }
            item.IconImage = strLogo;
            if (isRecording)
              item.PinImage = Thumbs.TvRecordingIcon;
            else
              item.PinImage = "";
            dlg.Add(item);
            _users.Add(user);
            if (TVHome.Card != null && TVHome.Card.IdChannel == idChannel)
            {
              selected = count;
            }
            count++;
          }
        }
      }
      if (channels.Count == 0)
      {
        GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (pDlgOK != null)
        {
          pDlgOK.SetHeading(692);//my tv
          pDlgOK.SetLine(1, GUILocalizeStrings.Get(1511)); // No Active streams
          pDlgOK.SetLine(2, "");
          pDlgOK.DoModal(this.GetID);
        }
        return;
      }
      dlg.SelectedLabel = selected;
      dlg.DoModal(this.GetID);
      if (dlg.SelectedLabel < 0) return;

      VirtualCard vCard = new VirtualCard(_users[dlg.SelectedLabel], RemoteControl.HostName);
      Channel channel = Navigator.GetChannel(vCard.IdChannel);
      TVHome.ViewChannel(channel);
    }

    void OnRecord()
    {
      //record now.
      //Are we recording this channel already?
      TvBusinessLayer layer = new TvBusinessLayer();
      TvServer server = new TvServer();
      VirtualCard card;
      if (false == server.IsRecording(Navigator.Channel.Name, out card))
      {
        //no then start recording
        Program prog = Navigator.Channel.CurrentProgram;
        if (prog != null)
        {
          GUIDialogMenuBottomRight pDlgOK = (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
          if (pDlgOK != null)
          {
            pDlgOK.SetHeading(605);//my tv
            pDlgOK.AddLocalizedString(875); //current program
            pDlgOK.AddLocalizedString(876); //till manual stop
            pDlgOK.DoModal(this.GetID);
            switch (pDlgOK.SelectedId)
            {
              case 875:
                {
                  Schedule newSchedule = new Schedule(Navigator.Channel.IdChannel, Navigator.Channel.CurrentProgram.Title,
                            Navigator.Channel.CurrentProgram.StartTime, Navigator.Channel.CurrentProgram.EndTime);
                  newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                  newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
                  newSchedule.RecommendedCard = TVHome.Card.Id; //added by joboehl - Enables the server to use the current card as the prefered on for recording. 

                  newSchedule.Persist();
                  server.OnNewSchedule();
                }
                break;

              case 876:
                {
                  Schedule newSchedule = new Schedule(Navigator.Channel.IdChannel, GUILocalizeStrings.Get(413) + " (" + Navigator.Channel.DisplayName + ")",
                                              DateTime.Now, DateTime.Now.AddDays(1));
                  newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                  newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
                  newSchedule.RecommendedCard = TVHome.Card.Id; //added by joboehl - Enables the server to use the current card as the prefered on for recording. 

                  newSchedule.Persist();
                  server.OnNewSchedule();
                }
                break;
            }
          }
        }
        else
        {
          //manual record
          Schedule newSchedule = new Schedule(Navigator.Channel.IdChannel, GUILocalizeStrings.Get(413) + " (" + Navigator.Channel.DisplayName + ")",
                                      DateTime.Now, DateTime.Now.AddDays(1));
          newSchedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
          newSchedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
          newSchedule.RecommendedCard = TVHome.Card.Id;

          newSchedule.Persist();
          server.OnNewSchedule();
        }
      }
      else
      {
        server.StopRecordingSchedule(card.RecordingScheduleId);
      }
      UpdateStateOfButtons();
    }

    /// <summary>
    /// Update the state of the following buttons
    /// - tv on/off
    /// - timeshifting on/off
    /// - record now
    /// </summary>
    void UpdateStateOfButtons()
    {
      if (!TVHome.Connected)
      {
        btnTvOnOff.Selected = false;
        return;
      }
      bool isTimeShifting = TVHome.Card.IsTimeShifting;
      if (btnTvOnOff.Selected != (g_Player.Playing && g_Player.IsTV && isTimeShifting))
      {
        btnTvOnOff.Selected = (g_Player.Playing && g_Player.IsTV && isTimeShifting);
      }
      bool hasTeletext = TVHome.Card.HasTeletext;
      if (btnTeletext.IsVisible != hasTeletext)
      {
        btnTeletext.IsVisible = hasTeletext;
      }
      //are we recording a tv program?
      VirtualCard card;
      if (Navigator.Channel != null && TVHome.Card != null)
      {
        TvServer server = new TvServer();
        string label;
        if (server.IsRecording(Navigator.Channel.Name, out card))
        {
          //yes then disable the timeshifting on/off buttons
          //and change the Record Now button into Stop Record
          label = GUILocalizeStrings.Get(629);//stop record
        }
        else
        {
          //nop. then change the Record Now button
          //to Record Now
          label = GUILocalizeStrings.Get(601);// record
        }
        if (label != btnRecord.Label)
        {
          btnRecord.Label = label;
        }
      }
    }

    void UpdateRecordingIndicator()
    {
      // if we're recording tv, update gui with info
      if (TVHome.Connected && TVHome.Card.IsRecording)
      {
        //int card;
        int scheduleId = TVHome.Card.RecordingScheduleId;
        if (scheduleId > 0)
        {
          Schedule schedule = Schedule.Retrieve(scheduleId);
          if (schedule.ScheduleType == (int)ScheduleRecordingType.Once)
          {
            imgRecordingIcon.SetFileName(Thumbs.TvRecordingIcon);
          }
          else
          {
            imgRecordingIcon.SetFileName(Thumbs.TvRecordingSeriesIcon);
          }
        }
      }
      else
      {
        imgRecordingIcon.IsVisible = false;
      }
    }

    /// <summary>
    /// Update the the progressbar in the GUI which shows
    /// how much of the current tv program has elapsed
    /// </summary>
    static public void UpdateProgressPercentageBar()
    {
      TimeSpan ts = DateTime.Now - _updateProgressTimer;
      if (ts.TotalMilliseconds < 1000) return;
      _updateProgressTimer = DateTime.Now;

      if (!TVHome.Connected)
      {
        return;
      }
      /*if (!TVHome.Connected || (!g_Player.IsTVRecording && !g_Player.IsTV))
      {
        GUIPropertyManager.SetProperty("#TV.View.channel", "");
        GUIPropertyManager.SetProperty("#TV.View.start", String.Empty);
        GUIPropertyManager.SetProperty("#TV.View.stop", String.Empty);
        GUIPropertyManager.SetProperty("#TV.View.title", String.Empty);
        GUIPropertyManager.SetProperty("#TV.View.description", String.Empty);
        return;
      }*/

      if (g_Player.Playing && g_Player.IsTimeShifting)
      {
        if (TVHome.Card != null)
        {
          if (TVHome.Card.IsTimeShifting == false)
          {
            g_Player.Stop();
          }
        }
        if ((g_Player.currentDescription.Length == 0) && (GUIPropertyManager.GetProperty("#TV.View.description").Length != 0))
        {
          GUIPropertyManager.SetProperty("#TV.View.channel", "");
          GUIPropertyManager.SetProperty("#TV.View.start", String.Empty);
          GUIPropertyManager.SetProperty("#TV.View.stop", String.Empty);
          GUIPropertyManager.SetProperty("#TV.View.title", String.Empty);
          GUIPropertyManager.SetProperty("#TV.View.description", String.Empty);
        }
      }

      if (!g_Player.IsTVRecording)
      {
        if (Navigator.Channel == null) return;
        try
        {
          if (Navigator.CurrentChannel == null)
          {
            GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
            GUIPropertyManager.SetProperty("#TV.View.channel", "");

            GUIPropertyManager.SetProperty("#TV.View.start", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.stop", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.remaining", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.genre", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.title", String.Empty);
            GUIPropertyManager.SetProperty("#TV.View.description", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.start", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.stop", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.genre", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.title", String.Empty);
            GUIPropertyManager.SetProperty("#TV.Next.description", String.Empty);
            return;
          }
          GUIPropertyManager.SetProperty("#TV.View.channel", Navigator.CurrentChannel);
          GUIPropertyManager.SetProperty("#TV.View.title", Navigator.CurrentChannel);
          Program current = Navigator.Channel.CurrentProgram;

          if (current != null)
          {
            GUIPropertyManager.SetProperty("#TV.View.start", current.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.View.stop", current.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.View.remaining", Utils.SecondsToHMSString(current.EndTime - current.StartTime));
            GUIPropertyManager.SetProperty("#TV.View.genre", current.Genre);
            GUIPropertyManager.SetProperty("#TV.View.title", current.Title);
            GUIPropertyManager.SetProperty("#TV.View.description", current.Description);
          }
          else
          {
            GUIPropertyManager.SetProperty("#TV.View.title", GUILocalizeStrings.Get(736));// no epg for this channel
          }
          Program next = Navigator.Channel.NextProgram;
          if (next != null)
          {
            GUIPropertyManager.SetProperty("#TV.Next.start", next.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.Next.stop", next.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            GUIPropertyManager.SetProperty("#TV.Next.remaining", Utils.SecondsToHMSString(next.EndTime - next.StartTime));
            GUIPropertyManager.SetProperty("#TV.Next.genre", next.Genre);
            GUIPropertyManager.SetProperty("#TV.Next.title", next.Title);
            GUIPropertyManager.SetProperty("#TV.Next.description", next.Description);
          }
          else
          {
            GUIPropertyManager.SetProperty("#TV.Next.title", GUILocalizeStrings.Get(736));// no epg for this channel
          }

          string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, Navigator.CurrentChannel);
          if (!System.IO.File.Exists(strLogo))
          {
            strLogo = "defaultVideoBig.png";
          }
          GUIPropertyManager.SetProperty("#TV.View.thumb", strLogo);

          //get current tv program
          Program prog = Navigator.Channel.CurrentProgram;
          if (prog == null)
          {
            GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
            GUIPropertyManager.SetProperty("#TV.Record.percent1", "0");
            GUIPropertyManager.SetProperty("#TV.Record.percent2", "0");
            GUIPropertyManager.SetProperty("#TV.Record.percent3", "0");
            return;
          }
          ts = prog.EndTime - prog.StartTime;
          if (ts.TotalSeconds <= 0)
          {
            GUIPropertyManager.SetProperty("#TV.View.Percentage", "0");
            GUIPropertyManager.SetProperty("#TV.Record.percent1", "0");
            GUIPropertyManager.SetProperty("#TV.Record.percent2", "0");
            GUIPropertyManager.SetProperty("#TV.Record.percent3", "0");
            return;
          }

          // caclulate total duration of the current program
          ts = (prog.EndTime - prog.StartTime);
          double programDuration = ts.TotalSeconds;

          //calculate where the program is at this time
          ts = (DateTime.Now - prog.StartTime);
          double livePoint = ts.TotalSeconds;

          //calculate when timeshifting was started
          double timeShiftStartPoint = livePoint - g_Player.Duration;
          if (timeShiftStartPoint < 0) timeShiftStartPoint = 0;

          //calculate where we the current playing point is
          double playingPoint = g_Player.Duration - g_Player.CurrentPosition;
          playingPoint = (livePoint - playingPoint);

          double timeShiftStartPointPercent = ((double)timeShiftStartPoint) / ((double)programDuration);
          timeShiftStartPointPercent *= 100.0d;
          GUIPropertyManager.SetProperty("#TV.Record.percent1", ((int)timeShiftStartPointPercent).ToString());

          if (!g_Player.Paused)
          {
            double playingPointPercent = ((double)playingPoint) / ((double)programDuration);
            playingPointPercent *= 100.0d;
            GUIPropertyManager.SetProperty("#TV.Record.percent2", ((int)playingPointPercent).ToString());
          }

          double percentLivePoint = ((double)livePoint) / ((double)programDuration);
          percentLivePoint *= 100.0d;
          GUIPropertyManager.SetProperty("#TV.View.Percentage", ((int)percentLivePoint).ToString());
          GUIPropertyManager.SetProperty("#TV.Record.percent3", ((int)percentLivePoint).ToString());
        }

        catch (Exception ex)
        {
          MediaPortal.GUI.Library.Log.Info("UpdateProgressPercentageBar:{0}", ex.Source, ex.StackTrace);
        }

      }
      else //recording is playing
      {
        double currentPosition = (double)(g_Player.CurrentPosition);
        double duration = (double)(g_Player.Duration);

        string startTime = Utils.SecondsToHMSString((int)currentPosition);
        string endTime = Utils.SecondsToHMSString((int)duration);

        double percentLivePoint = ((double)currentPosition) / ((double)duration);
        percentLivePoint *= 100.0d;

        GUIPropertyManager.SetProperty("#TV.Record.percent1", ((int)percentLivePoint).ToString());
        GUIPropertyManager.SetProperty("#TV.Record.percent2", "0");
        GUIPropertyManager.SetProperty("#TV.Record.percent3", "0");
        GUIPropertyManager.SetProperty("#TV.View.channel", TvRecorded.ActiveRecording().ReferencedChannel().DisplayName + " (" + GUILocalizeStrings.Get(949) + ")");
        GUIPropertyManager.SetProperty("#TV.View.title", g_Player.currentTitle);
        GUIPropertyManager.SetProperty("#TV.View.description", g_Player.currentDescription);

        GUIPropertyManager.SetProperty("#TV.View.start", startTime);
        GUIPropertyManager.SetProperty("#TV.View.stop", endTime);
        //GUIPropertyManager.SetProperty("#TV.View.remaining", Utils.SecondsToHMSString(prog.EndTime - prog.StartTime));                
      }
    }

    /// <summary>
    /// When called this method will switch to the previous TV channel
    /// </summary>
    static public void OnPreviousChannel()
    {
      MediaPortal.GUI.Library.Log.Info("TVHome:OnPreviousChannel()");
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        // where in fullscreen so delayzap channel instead of immediatly tune..
        TvFullScreen TVWindow = (TvFullScreen)GUIWindowManager.GetWindow((int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
        if (TVWindow != null) TVWindow.ZapPreviousChannel();
        return;
      }

      // Zap to previous channel immediately
      Navigator.ZapToPreviousChannel(false);
    }

    static public int GetPreferedAudioStreamIndex() // also used from tvrecorded class
    {
      int idxFirstAc3 = -1;         // the index of the first avail. ac3 found
      int idxFirstmpeg = -1;        // the index of the first avail. mpg found
      int idxLangAc3 = -1;          // the index of ac3 found based on lang. pref
      int idxLangmpeg = -1;         // the index of mpg found based on lang. pref   
      int idx = -1;                 // the chosen audio index we return
      string langSel = "";          // find audio based on this language.
      string ac3BasedOnLang = "";   // for debugging, what lang. in prefs. where used to choose the ac3 audio track ?
      string mpegBasedOnLang = "";  // for debugging, what lang. in prefs. where used to choose the mpeg audio track ?

      IAudioStream[] streams;

      List<IAudioStream> streamsList = new List<IAudioStream>();
      for (int i = 0; i < g_Player.AudioStreams; i++)
      {
        DVBAudioStream stream = new DVBAudioStream();

        string streamType = g_Player.AudioType(i);

        switch (streamType)
        {
          case "AC3":
            stream.StreamType = AudioStreamType.AC3;
            break;
          case "Mpeg1":
            stream.StreamType = AudioStreamType.Mpeg1;
            break;
          case "Mpeg2":
            stream.StreamType = AudioStreamType.Mpeg2;
            break;
          case "AAC":
            stream.StreamType = AudioStreamType.AAC;
            break;
          case "LATMAAC":
            stream.StreamType = AudioStreamType.LATMAAC;
            break;
          default:
            stream.StreamType = AudioStreamType.Unknown;
            break;
        }

        stream.Language = g_Player.AudioLanguage(i);
        streamsList.Add(stream);
      }
      streams = (IAudioStream[])streamsList.ToArray();

      if (_preferredLanguages != null)
      {
        Log.Debug("TVHome.GetPreferedAudioStreamIndex(): preferred LANG(s):{0} preferAC3:{1} preferAudioTypeOverLang:{2}", String.Join(";", _preferredLanguages), _preferAC3, _preferAudioTypeOverLang);
      }
      else
      {
        Log.Debug("TVHome.GetPreferedAudioStreamIndex(): preferred LANG(s):{0} preferAC3:{1} _preferAudioTypeOverLang:{2}", "n/a", _preferAC3, _preferAudioTypeOverLang);
      }
      Log.Debug("Audio streams avail: {0}", streams.Length);

      if (streams.Length == 1)
      {
        Log.Info("Audio stream: switching to preferred AC3/MPEG audio stream 0 (only 1 track avail.)");
        return 0;
      }

      for (int i = 0; i < streams.Length; i++)
      {
        //tag the first found ac3 and mpeg indexes
        if (streams[i].StreamType == AudioStreamType.AC3)
        {
          if (idxFirstAc3 == -1) idxFirstAc3 = i;
        }
        else
        {
          if (idxFirstmpeg == -1) idxFirstmpeg = i;
        }

        //now find the ones based on LANG prefs.
        if (_preferredLanguages != null)
        {
          for (int j = 0; j < _preferredLanguages.Length; j++)
          {
            if (_preferredLanguages[j].Length == 0) continue;
            langSel = _preferredLanguages[j];
            if (langSel.Contains(streams[i].Language) && langSel.Length > 0)
            {
              if ((streams[i].StreamType == AudioStreamType.AC3)) //is the audio track an AC3 track ?
              {
                if (idxLangAc3 == -1)
                {
                  idxLangAc3 = i;
                  ac3BasedOnLang = langSel;
                }
              }
              else //audiotrack is mpeg
              {
                if (idxLangmpeg == -1)
                {
                  idxLangmpeg = i;
                  mpegBasedOnLang = langSel;
                }
              }
            }
            if (idxLangAc3 > -1 && idxLangmpeg > -1) break;
          } //for loop
        }
        if (idxFirstAc3 > -1 && idxFirstmpeg > -1 && idxLangAc3 > -1 && idxLangmpeg > -1) break;
      }

      if (_preferAC3)
      {
        if (_preferredLanguages != null)
        {
          //did we find an ac3 track that matches our LANG prefs ?
          if (idxLangAc3 > -1)
          {
            idx = idxLangAc3;
            Log.Info("Audio stream: switching to preferred AC3 audio stream {0}, based on LANG {1}", idx, ac3BasedOnLang);
          }
          //if not, did we even find an ac3 track ?
          else if (idxFirstAc3 > -1)
          {
            //we did find an AC3 track, but not based on LANG - should we choose this or the mpeg track which is based on LANG.
            if (_preferAudioTypeOverLang || (idxLangmpeg == -1 && _preferAudioTypeOverLang))
            {
              idx = idxFirstAc3;
              Log.Info("Audio stream: switching to preferred AC3 audio stream {0}, NOT based on LANG (none avail. matching {1})", idx, ac3BasedOnLang);
            }
            else
            {
              Log.Info("Audio stream: ignoring AC3 audio stream {0}", idxFirstAc3);
            }
          }
          //if not then proceed with mpeg lang. selection below.
        }
        else
        {
          //did we find an ac3 track ?
          if (idxFirstAc3 > -1)
          {
            idx = idxFirstAc3;
            Log.Info("Audio stream: switching to preferred AC3 audio stream {0}, NOT based on LANG", idx);
          }
          //if not then proceed with mpeg lang. selection below.
        }
      }

      if (idx == -1 && _preferAC3)
      {
        Log.Info("Audio stream: no preferred AC3 audio stream found, trying mpeg instead.");
      }

      if (idx == -1 || !_preferAC3) // we end up here if ac3 selection didnt happen (no ac3 avail.) or if preferac3 is disabled.
      {
        if (_preferredLanguages != null)
        {
          //did we find a mpeg track that matches our LANG prefs ?
          if (idxLangmpeg > -1)
          {
            idx = idxLangmpeg;
            Log.Info("Audio stream: switching to preferred MPEG audio stream {0}, based on LANG {1}", idx, mpegBasedOnLang);
          }
          //if not, did we even find a mpeg track ?
          else if (idxFirstmpeg > -1)
          {
            //we did find a AC3 track, but not based on LANG - should we choose this or the mpeg track which is based on LANG.
            if (_preferAudioTypeOverLang || (idxLangAc3 == -1 && _preferAudioTypeOverLang))
            {
              idx = idxFirstmpeg;
              Log.Info("Audio stream: switching to preferred MPEG audio stream {0}, NOT based on LANG (none avail. matching {1})", idx, mpegBasedOnLang);
            }
            else
            {
              if (idxLangAc3 > -1)
              {
                idx = idxLangAc3;
                Log.Info("Audio stream: ignoring MPEG audio stream {0}", idx);
              }
            }
          }
        }
        else
        {
          idx = idxFirstmpeg;
          Log.Info("Audio stream: switching to preferred MPEG audio stream {0}, NOT based on LANG", idx);
        }
      }

      if (idx == -1)
      {
        idx = 0;
        Log.Info("Audio stream: switching to preferred AC3/MPEG audio stream {0}", idx);
      }

      return idx;
    }

    static public bool ViewChannelAndCheck(Channel channel)
    {
      _doingChannelChange = false;
      //System.Diagnostics.Debugger.Launch();
      try
      {
        if (channel == null)
        {
          MediaPortal.GUI.Library.Log.Info("TVHome.ViewChannelAndCheck(): channel==null");
          return false;
        }
        MediaPortal.GUI.Library.Log.Info("TVHome.ViewChannelAndCheck(): View channel={0}", channel.DisplayName);

        // do we stop the player when changing channel ?
        // _userChannelChanged is true if user did interactively change the channel, like with mini ch. list. etc.
        if (!_userChannelChanged)
        {
          if (g_Player.IsTVRecording) return true;
          if (!_autoTurnOnTv) //respect the autoturnontv setting.
          {
            if (g_Player.IsVideo) return true;
            if (g_Player.IsDVD) return true;
            if (g_Player.IsMusic) return true;
            if (g_Player.IsRadio) return true;
          }
          else
          {
            if (g_Player.IsVideo || g_Player.IsDVD || g_Player.IsMusic)// || g_Player.IsRadio)
            {
              g_Player.Stop(true); // tell that we are zapping so exclusive mode is not going to be disabled
            }
          }
        }
        else if (g_Player.IsTVRecording && _userChannelChanged) //we are watching a recording, we have now issued a ch. change..stop the player.
        {
          _userChannelChanged = false;
          g_Player.Stop(true);
        }
        else if ((channel.IsTv && g_Player.IsRadio) || (channel.IsRadio && g_Player.IsTV))
        {
          g_Player.Stop(true);
        }

        if (Navigator.Channel != null)
        {
          if (channel.IdChannel != Navigator.Channel.IdChannel || (Navigator.LastViewedChannel == null))
          {
            Navigator.LastViewedChannel = Navigator.Channel;
          }
        }
        else
        {
          MediaPortal.GUI.Library.Log.Info("Navigator.Channel==null");
        }
        /* A part of the code to implement IP-TV 
        if (channel.IsWebstream())
        {
          IList details = channel.ReferringTuningDetail();
          TuningDetail detail = (TuningDetail)details[0];
          g_Player.PlayVideoStream(detail.Url, channel.DisplayName);
          return true;
        }
        else
        {
          if (Navigator.LastViewedChannel.IsWebstream())
            g_Player.Stop();
        }*/

        string errorMessage;
        TvResult succeeded;
        if (TVHome.Card != null)
        {
          if (g_Player.Playing && g_Player.IsTV && !g_Player.IsTVRecording) //modified by joboehl. Avoids other video being played instead of TV. 
            //if we're already watching this channel, then simply return
            if (TVHome.Card.IsTimeShifting == true && TVHome.Card.IdChannel == channel.IdChannel)
            {
              return true;
            }
        }

        _doingChannelChange = true;

        User user = new User();
        if (TVHome.Card != null)
        {
          user.CardId = TVHome.Card.Id;
        }

        //GUIWaitCursor.Show();
        bool wasPlaying = (g_Player.Playing && g_Player.IsTimeShifting && !g_Player.Stopped) && (g_Player.IsTV || g_Player.IsRadio);

        //Start timeshifting the new tv channel
        TvServer server = new TvServer();
        VirtualCard card;
        //bool _return = false;			

        if (wasPlaying)
        {
          // we need to stop player HERE if card has changed.        
          int newCardId = server.TimeShiftingWouldUseCard(ref user, channel.IdChannel);

          //Added by joboehl - If any major related to the timeshifting changed during the start, restart the player. 
          bool cardChanged = false;

          if (newCardId == -1)
          {
            cardChanged = false;
          }
          else
          {
            cardChanged = (TVHome.Card.Id != newCardId); // || TVHome.Card.RTSPUrl != newCard.RTSPUrl || TVHome.Card.TimeShiftFileName != newCard.TimeShiftFileName);
          }

          if (cardChanged)
          {
            if (wasPlaying)
            {
              MediaPortal.GUI.Library.Log.Debug("TVHome.ViewChannelAndCheck(): Stopping player. CardId:{0}/{1}, RTSP:{2}", TVHome.Card.Id, newCardId, TVHome.Card.RTSPUrl);
              MediaPortal.GUI.Library.Log.Debug("TVHome.ViewChannelAndCheck(): Stopping player. Timeshifting:{0}", TVHome.Card.TimeShiftFileName);
              g_Player.StopAndKeepTimeShifting(); // keep timeshifting on server, we only want to recreate the graph on the client
              MediaPortal.GUI.Library.Log.Debug("TVHome.ViewChannelAndCheck(): rebulding graph (card changed) - timeshifting continueing.");
            }
            succeeded = server.StartTimeShifting(ref user, channel.IdChannel, out card);
          }
          else
          {
            g_Player.PauseGraph();
            succeeded = server.StartTimeShifting(ref user, channel.IdChannel, out card);
            SeekToEnd(true);
            g_Player.ContinueGraph();
          }
        }
        else
        {
          succeeded = server.StartTimeShifting(ref user, channel.IdChannel, out card);
        }

        if (succeeded == TvResult.Succeeded)
        {
          //timeshifting succeeded					          
          TVHome.Card = card; //Moved by joboehl - Only touch the card if starttimeshifting succeeded. 

          MediaPortal.GUI.Library.Log.Info("succeeded:{0} {1}", succeeded, card);

          if (!g_Player.Playing)
          {
            StartPlay();
          }

          _playbackStopped = false;

          //GUIWaitCursor.Hide();
          _doingChannelChange = false;
          return true;
        }
        else
        {
          //timeshifting new channel failed. 

          if (g_Player.Duration == 0)
          {
            //Added by joboehl - Only stops if stream is empty. Otherwise, a message is displaying but everything stays as before.  
            g_Player.Stop();
          }
        }

        //GUIWaitCursor.Hide();

        //if (pDlgOK != null && pDlgYesNo != null)

        errorMessage = GUILocalizeStrings.Get(1500);
        switch (succeeded)
        {
          case TvResult.CardIsDisabled:
            errorMessage += "\r" + GUILocalizeStrings.Get(1501) + "\r";
            break;
          case TvResult.AllCardsBusy:
            errorMessage += "\r" + GUILocalizeStrings.Get(1502) + "\r";
            break;
          case TvResult.ChannelIsScrambled:
            errorMessage += "\r" + GUILocalizeStrings.Get(1503) + "\r";
            break;
          case TvResult.NoVideoAudioDetected:
            errorMessage += "\r" + GUILocalizeStrings.Get(1504) + "\r";
            break;
          case TvResult.UnableToStartGraph:
            errorMessage += "\r" + GUILocalizeStrings.Get(1505) + "\r";
            break;
          case TvResult.UnknownError:
            // this error can also happen if we have no connection to the server.
            if (!TVHome.Connected || !RemoteControl.IsConnected)
            {
              errorMessage += "\r" + GUILocalizeStrings.Get(1510) + "\r"; // Connection to TV server lost
            }
            else
            {
              errorMessage += "\r" + GUILocalizeStrings.Get(1506) + "\r";
            }
            break;
          case TvResult.UnknownChannel:
            errorMessage += "\r" + GUILocalizeStrings.Get(1507) + "\r";
            break;
          case TvResult.ChannelNotMappedToAnyCard:
            errorMessage += "\r" + GUILocalizeStrings.Get(1508) + "\r";
            break;
          case TvResult.NoTuningDetails:
            errorMessage += "\r" + GUILocalizeStrings.Get(1509) + "\r";
            break;
          default:
            // this error can also happen if we have no connection to the server.
            if (!TVHome.Connected || !RemoteControl.IsConnected)
            {
              errorMessage += "\r" + GUILocalizeStrings.Get(1510) + "\r"; // Connection to TV server lost
            }
            else
            {
              errorMessage += "\r" + GUILocalizeStrings.Get(1506) + "\r";
            }
            break;
        }
        if (wasPlaying) //show yes no dialogue
        {
          GUIDialogYesNo pDlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
          string[] lines = errorMessage.Split('\r');
          string caption = GUILocalizeStrings.Get(605) + " - " + GUILocalizeStrings.Get(1512);
          pDlgYesNo.SetHeading(caption);//my tv
          pDlgYesNo.SetLine(1, channel.DisplayName);
          pDlgYesNo.SetLine(2, lines[0]);
          if (lines.Length > 1)
            pDlgYesNo.SetLine(3, lines[1]);
          else
            pDlgYesNo.SetLine(3, "");
          if (lines.Length > 2)
            pDlgYesNo.SetLine(4, lines[2]);
          else
            pDlgYesNo.SetLine(4, "");

          //pDlgYesNo.SetLine(5, "Tune previous channel?"); //Tune previous channel?            

          /*if (GUIWindowManager.ActiveWindow == (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          {
            g_Player.FullScreen = false;
          }*/
          if (GUIWindowManager.ActiveWindow == (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          {
            pDlgYesNo.DoModal((int)GUIWindowManager.ActiveWindowEx);
            // If failed and wasPlaying TV, fallback to the last viewed channel. 

            if (pDlgYesNo.IsConfirmed)
            {
              ViewChannelAndCheck(Navigator.Channel);
              GUIWaitCursor.Hide();
            }
          }
          else
          {
            ParameterizedThreadStart pThread = new ParameterizedThreadStart(ShowDlg);
            Thread showDlgThread = new Thread(pThread);

            //GUIWaitCursor.Hide();						
            // show the dialog asynch.
            // this fixes a hang situation that would happen when resuming TV with showlastactivemodule
            showDlgThread.Start(pDlgYesNo);
          }
        }
        else //show ok
        {
          GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
          string[] lines = errorMessage.Split('\r');
          pDlgOK.SetHeading(605);//my tv
          pDlgOK.SetLine(1, channel.DisplayName);
          pDlgOK.SetLine(2, lines[0]);
          if (lines.Length > 1)
            pDlgOK.SetLine(3, lines[1]);
          else
            pDlgOK.SetLine(3, "");
          if (lines.Length > 2)
            pDlgOK.SetLine(4, lines[2]);
          else
            pDlgOK.SetLine(4, "");

          if (GUIWindowManager.ActiveWindow == (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
          {
            pDlgOK.DoModal(GUIWindowManager.ActiveWindowEx);
          }
          else
          {
            ParameterizedThreadStart pThread = new ParameterizedThreadStart(ShowDlg);
            Thread showDlgThread = new Thread(pThread);

            // show the dialog asynch.
            // this fixes a hang situation that would happen when resuming TV with showlastactivemodule
            showDlgThread.Start(pDlgOK);
          }
        }
        _doingChannelChange = false;
        return false;
      }
      catch (Exception ex)
      {
        Log.Debug("TvPlugin:ViewChannelandCheck Exception {0}", ex.ToString());
        //GUIWaitCursor.Hide();
        _doingChannelChange = false;
        return false;
      }
    }

    public static void ShowDlg(object Dialogue)
    {
      GUIDialogOK pDlgOK = null;
      GUIDialogYesNo pDlgYESNO = null;

      if (Dialogue is GUIDialogOK)
      {
        pDlgOK = (GUIDialogOK)Dialogue;
      }
      else if (Dialogue is GUIDialogYesNo)
      {
        pDlgYESNO = (GUIDialogYesNo)Dialogue;
      }
      else
      {
        return;
      }

      GUIWindow guiWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);

      int count = 0;

      while (count < 50)
      {
        if (guiWindow.WindowLoaded)
        {
          break;
        }
        else
        {
          System.Threading.Thread.Sleep(100);
        }
        count++;
      }

      if (pDlgOK != null) pDlgOK.DoModal(GUIWindowManager.ActiveWindowEx);
      if (pDlgYESNO != null)
      {
        pDlgYESNO.DoModal(GUIWindowManager.ActiveWindowEx);
        // If failed and wasPlaying TV, fallback to the last viewed channel. 						
        if (pDlgYESNO.IsConfirmed)
        {
          //ViewChannelAndCheck(Navigator.Channel); not working from thread
        }
      }
    }

    static public void ViewChannel(Channel channel)
    {
      ViewChannelAndCheck(channel);
      Navigator.UpdateCurrentChannel();
      TVHome.UpdateProgressPercentageBar();
      return;
    }

    /// <summary>
    /// When called this method will switch to the next TV channel
    /// </summary>
    static public void OnNextChannel()
    {
      MediaPortal.GUI.Library.Log.Info("TVHome:OnNextChannel()");
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        // where in fullscreen so delayzap channel instead of immediatly tune..
        TvFullScreen TVWindow = (TvFullScreen)GUIWindowManager.GetWindow((int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
        if (TVWindow != null)
        {
          TVWindow.ZapNextChannel();
        }
        return;
      }

      // Zap to next channel immediately
      Navigator.ZapToNextChannel(false);
    }

    /// <summary>
    /// When called this method will switch to the last viewed TV channel   // mPod
    /// </summary>
    static public void OnLastViewedChannel()
    {
      Navigator.ZapToLastViewedChannel();
    }

    /// <summary>
    /// Returns true if the specified window belongs to the my tv plugin
    /// </summary>
    /// <param name="windowId">id of window</param>
    /// <returns>
    /// true: belongs to the my tv plugin
    /// false: does not belong to the my tv plugin</returns>
    static public bool IsTVWindow(int windowId)
    {
      if (windowId == (int)GUIWindow.Window.WINDOW_TV) return true;

      return false;
    }

    /// <summary>
    /// Gets the channel navigator that can be used for channel zapping.
    /// </summary>
    static public ChannelNavigator Navigator
    {
      get { return m_navigator; }
    }

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string PluginName()
    {
      return "My TV";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return (int)GUIWindow.Window.WINDOW_TV;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      // TODO:  Add TVHome.GetHome implementation
      strButtonText = GUILocalizeStrings.Get(605);
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return true;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string Description()
    {
      return "Watch, record and timeshift analog and digital TV with MediaPortal. Tv Engine v3";
    }

    public bool HasSetup()
    {
      return true;
    }

    public void ShowPlugin()
    {
      TvSetupForm setup = new TvSetupForm();
      setup.ShowDialog();
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return false;
    }

    #endregion

    static void StartPlay()
    {
      Stopwatch benchClock = null;
      benchClock = Stopwatch.StartNew();
      if (TVHome.Card == null)
      {
        MediaPortal.GUI.Library.Log.Info("tvhome:startplay card=null");
        return;
      }
      if (TVHome.Card.IsScrambled)
      {
        MediaPortal.GUI.Library.Log.Info("tvhome:startplay scrambled");
        return;
      }
      MediaPortal.GUI.Library.Log.Info("tvhome:startplay");
      string timeshiftFileName = TVHome.Card.TimeShiftFileName;
      MediaPortal.GUI.Library.Log.Info("tvhome:file:{0}", timeshiftFileName);

      IChannel channel = TVHome.Card.Channel;
      if (channel == null)
      {
        MediaPortal.GUI.Library.Log.Info("tvhome:startplay channel=null");
        return;
      }
      g_Player.MediaType mediaType = g_Player.MediaType.TV;
      if (channel.IsRadio)
        mediaType = g_Player.MediaType.Radio;

      bool useRtsp = System.IO.File.Exists("usertsp.txt");
      benchClock.Stop();
      Log.Warn("tvhome:startplay.  Phase 1 - {0} ms - Done method initialization", benchClock.ElapsedMilliseconds.ToString());
      benchClock.Reset();
      benchClock.Start();
      if (System.IO.File.Exists(timeshiftFileName) && !useRtsp)
      {
        MediaPortal.GUI.Library.Log.Info("tvhome:startplay:{0}", timeshiftFileName);
        g_Player.Play(timeshiftFileName, mediaType);
        benchClock.Stop();
        Log.Warn("tvhome:startplay.  Phase 2 - {0} ms - Done starting g_Player.Play()", benchClock.ElapsedMilliseconds.ToString());
        benchClock.Reset();
        //benchClock.Start();
        //SeekToEnd(false);
        //Log.Warn("tvhome:startplay.  Phase 3 - {0} ms - Done seeking.", benchClock.ElapsedMilliseconds.ToString());
      }
      else
      {
        timeshiftFileName = TVHome.Card.RTSPUrl;
        MediaPortal.GUI.Library.Log.Info("tvhome:startplay:{0}", timeshiftFileName);
        g_Player.Play(timeshiftFileName, mediaType);
        benchClock.Stop();
        Log.Warn("tvhome:startplay.  Phase 2 - {0} ms - Done starting g_Player.Play()", benchClock.ElapsedMilliseconds.ToString());
        benchClock.Reset();
        //benchClock.Start();
        //SeekToEnd(true);
        //Log.Warn("tvhome:startplay.  Phase 3 - {0} ms - Done seeking.", benchClock.ElapsedMilliseconds.ToString());
        //SeekToEnd(true);
      }
      benchClock.Stop();
    }

    static void SeekToEnd(bool zapping)
    {
      Log.Info("tvhome:SeektoEnd({0})", zapping);
      double duration = g_Player.Duration;
      double position = g_Player.CurrentPosition;

      string timeshiftFileName = TVHome.Card.TimeShiftFileName;
      bool useRtsp = System.IO.File.Exists("usertsp.txt");
      if (System.IO.File.Exists(timeshiftFileName) && !useRtsp)
      {
        if (g_Player.IsRadio == false)
        {
          if (duration > 0 || position > 0)
            g_Player.SeekAbsolute(duration);
        }
      }
      else
      {
        //streaming....
        if (zapping)
        {
          // avoid seeking on radion in multiseat, b/c of a an unsolved bug in tsreader.ax
          if (!g_Player.IsRadio)
          {
            //System.Threading.Thread.Sleep(100);            
            Log.Info("tvhome:SeektoEnd({0}/{1})", position, duration);
            if (duration > 0 || position > 0)
              g_Player.SeekAbsolute(duration + 10);
          }
        }
      }
    }
  }

  #region ChannelNavigator class

  /// <summary>
  /// Handles the logic for channel zapping. This is used by the different GUI modules in the TV section.
  /// </summary>
  public class ChannelNavigator
  {
    #region config xml file
    const string ConfigFileXml = @"<?xml version=|1.0| encoding=|utf-8|?> 
<ideaBlade xmlns:xsi=|http://www.w3.org/2001/XMLSchema-instance| xmlns:xsd=|http://www.w3.org/2001/XMLSchema| useDeclarativeTransactions=|false| version=|1.03|> 
  <useDTC>false</useDTC>
  <copyLocal>false</copyLocal>
  <logging>
    <archiveLogs>false</archiveLogs>
    <logFile>DebugMediaPortal.GUI.Library.Log.xml</logFile>
    <usesSeparateAppDomain>false</usesSeparateAppDomain>
    <port>0</port>
  </logging>
  <rdbKey name=|default| databaseProduct=|Unknown|>
    <connection>[CONNECTION]</connection>
    <probeAssemblyName>TVDatabase</probeAssemblyName>
  </rdbKey>
  <remoting>
    <remotePersistenceEnabled>false</remotePersistenceEnabled>
    <remoteBaseURL>http://localhost</remoteBaseURL>
    <serverPort>9009</serverPort>
    <serviceName>PersistenceServer</serviceName>
    <serverDetectTimeoutMilliseconds>-1</serverDetectTimeoutMilliseconds>
    <proxyPort>0</proxyPort>
  </remoting>
  <appUpdater/>
</ideaBlade>
";

    #endregion

    #region constants
    #endregion

    #region Private members

    private List<Channel> _channelList = new List<Channel>();
    private List<ChannelGroup> m_groups = new List<ChannelGroup>(); // Contains all channel groups (including an "all channels" group)
    private int m_currentgroup = 0;
    private DateTime m_zaptime;
    private long m_zapdelay;
    private Channel m_zapchannel = null;
    private int m_zapgroup = -1;
    private Channel _lastViewedChannel = null; // saves the last viewed Channel  // mPod    
    private Channel m_currentChannel = null;
    private IList channels = new ArrayList();
    private bool reentrant = false;

    #endregion

    #region Constructors

    public ChannelNavigator()
    {
      // Load all groups
      //ServiceProvider services = GlobalServiceProvider.Instance;

      MediaPortal.GUI.Library.Log.Info("ChannelNavigator::ctor()");
      string ipadres = Dns.GetHostName();
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        ipadres = xmlreader.GetValueAsString("tvservice", "hostname", "");
        if (ipadres == "" || ipadres == "localhost")
        {
          ipadres = Dns.GetHostName();
          MediaPortal.GUI.Library.Log.Info("Remote control: hostname not specified on mediaportal.xml!");
          xmlreader.SetValue("tvservice", "hostname", ipadres);
          ipadres = "localhost";
          MediaPortal.Profile.Settings.SaveCache();
        }
      }
      RemoteControl.HostName = ipadres;
      MediaPortal.GUI.Library.Log.Info("Remote control:master server :{0}", RemoteControl.HostName);

      ReLoad();
    }

    public void ReLoad()
    {
      try
      {
        string connectionString, provider;
        RemoteControl.Instance.GetDatabaseConnectionString(out connectionString, out provider);

        try
        {
          XmlDocument doc = new XmlDocument();
          doc.Load("gentle.config");
          XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
          XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString");
          XmlNode nodeProvider = nodeKey.Attributes.GetNamedItem("name");
          node.InnerText = connectionString;
          nodeProvider.InnerText = provider;
          doc.Save("gentle.config");
        }
        catch (Exception ex)
        {
          Log.Error("Unable to create/modify gentle.config {0},{1}", ex.Message, ex.StackTrace);
        }

        MediaPortal.GUI.Library.Log.Info("ChannelNavigator::Reload()");
        Gentle.Framework.ProviderFactory.ResetGentle(true);
        Gentle.Framework.ProviderFactory.SetDefaultProviderConnectionString(connectionString);
        MediaPortal.GUI.Library.Log.Info("get channels from database");
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Channel));
        sb.AddConstraint(Operator.Equals, "isTv", 1);
        sb.AddOrderByField(true, "sortOrder");
        SqlStatement stmt = sb.GetStatement(true);
        channels = ObjectFactory.GetCollection(typeof(Channel), stmt.Execute());
        MediaPortal.GUI.Library.Log.Info("found:{0} tv channels", channels.Count);
        TvNotifyManager.OnNotifiesChanged();
        m_groups.Clear();

        TvBusinessLayer layer = new TvBusinessLayer();
        MediaPortal.GUI.Library.Log.Info("Checking if radio group for all radio channels exists");
        RadioChannelGroup allRadioChannelsGroup = layer.GetRadioChannelGroupByName(GUILocalizeStrings.Get(972));
        if (allRadioChannelsGroup == null)
        {
          MediaPortal.GUI.Library.Log.Info("All channels group for radio channels does not exist. Creating it...");
          allRadioChannelsGroup = new RadioChannelGroup(GUILocalizeStrings.Get(972), 9999);
          allRadioChannelsGroup.Persist();
        }
        IList radioChannels = layer.GetAllRadioChannels();
        if (radioChannels != null)
        {
          if (radioChannels.Count > allRadioChannelsGroup.ReferringRadioGroupMap().Count)
          {
            foreach (Channel radioChannel in radioChannels)
              layer.AddChannelToRadioGroup(radioChannel, allRadioChannelsGroup);
          }
        }
        MediaPortal.GUI.Library.Log.Info("Done.");

        MediaPortal.GUI.Library.Log.Info("get all groups from database");
        bool found = false;
        sb = new SqlBuilder(StatementType.Select, typeof(ChannelGroup));
        sb.AddOrderByField(true, "groupName");
        stmt = sb.GetStatement(true);
        IList groups = ObjectFactory.GetCollection(typeof(ChannelGroup), stmt.Execute());
        IList allgroupMaps = GroupMap.ListAll();
        found = false;
        foreach (ChannelGroup group in groups)
        {
          if (group.GroupName == GUILocalizeStrings.Get(972))
          {
            found = true;
            foreach (Channel channel in channels)
            {
              if (channel.IsTv == false) continue;
              bool groupContainsChannel = false;
              foreach (GroupMap map in allgroupMaps)
              {
                if (map.IdGroup != group.IdGroup) continue;
                if (map.IdChannel == channel.IdChannel)
                {
                  groupContainsChannel = true;
                  break;
                }
              }
              if (!groupContainsChannel)
              {
                layer.AddChannelToGroup(channel, GUILocalizeStrings.Get(972));

              }
            }
            break;
          }
        }

        if (!found)
        {
          MediaPortal.GUI.Library.Log.Info(" group:{0} not found. create it", GUILocalizeStrings.Get(972));
          foreach (Channel channel in channels)
          {
            layer.AddChannelToGroup(channel, GUILocalizeStrings.Get(972));
          }
          MediaPortal.GUI.Library.Log.Info(" group:{0} created", GUILocalizeStrings.Get(972));
        }

        groups = ChannelGroup.ListAll();
        foreach (ChannelGroup group in groups)
        {
          //group.GroupMaps.ApplySort(new GroupMap.Comparer(), false);
          m_groups.Add(group);
        }

        MediaPortal.GUI.Library.Log.Info("loaded {0} groups", m_groups.Count);
        //TVHome.Connected = true;
      }
      catch (Exception ex)
      {
        MediaPortal.GUI.Library.Log.Error(ex);
        //TVHome.Connected = false;
      }
    }
    #endregion

    #region Public properties

    /// <summary>
    /// Gets the channel that we currently watch.
    /// Returns empty string if there is no current channel.
    /// </summary>
    public string CurrentChannel
    {
      get
      {
        if (m_currentChannel == null) return null;
        return m_currentChannel.DisplayName;
      }
    }
    public Channel Channel
    {
      get { return m_currentChannel; }
    }

    /// <summary>
    /// Gets and sets the last viewed channel
    /// Returns empty string if no zap occurred before
    /// </summary>
    public Channel LastViewedChannel
    {
      get { return _lastViewedChannel; }
      set { _lastViewedChannel = value; }
    }

    /// <summary>
    /// Gets the currently active channel group.
    /// </summary>
    public ChannelGroup CurrentGroup
    {
      get
      {
        if (m_groups.Count == 0) return null;
        return (ChannelGroup)m_groups[m_currentgroup];
      }
    }

    /// <summary>
    /// Gets the list of channel groups.
    /// </summary>
    public List<ChannelGroup> Groups
    {
      get { return m_groups; }
    }

    /// <summary>
    /// Gets the channel that we will zap to. Contains the current channel if not zapping to anything.
    /// </summary>
    public Channel ZapChannel
    {
      get
      {
        if (m_zapchannel == null)
          return m_currentChannel;
        return m_zapchannel;
      }
    }

    /// <summary>
    /// Gets the configured zap delay (in milliseconds).
    /// </summary>
    public long ZapDelay
    {
      get { return m_zapdelay; }
    }

    /// <summary>
    /// Gets the group that we will zap to. Contains the current group name if not zapping to anything.
    /// </summary>
    public string ZapGroupName
    {
      get
      {
        if (m_zapgroup == -1)
          return CurrentGroup.GroupName;
        return ((ChannelGroup)m_groups[m_zapgroup]).GroupName;
      }
    }

    #endregion

    #region Public methods


    public void ZapNow()
    {
      m_zaptime = DateTime.Now.AddSeconds(-1);
      // MediaPortal.GUI.Library.Log.Info(MediaPortal.GUI.Library.Log.LogType.Error, "zapnow group:{0} current group:{0}", m_zapgroup, m_currentgroup);
      //if (m_zapchannel == null)
      //   MediaPortal.GUI.Library.Log.Info(MediaPortal.GUI.Library.Log.LogType.Error, "zapchannel==null");
      //else
      //   MediaPortal.GUI.Library.Log.Info(MediaPortal.GUI.Library.Log.LogType.Error, "zapchannel=={0}",m_zapchannel);
    }
    /// <summary>
    /// Checks if it is time to zap to a different channel. This is called during Process().
    /// </summary>
    public bool CheckChannelChange()
    {
      if (reentrant) return false;
      // BAV, 02.03.08: a channel change should not be delayed by rendering.
      //                by scipping this => 1 min delays in zapping should be avoided 
      //if (GUIGraphicsContext.InVmr9Render) return false;
      reentrant = true;
      UpdateCurrentChannel();

      // Zapping to another group or channel?
      if (m_zapgroup != -1 || m_zapchannel != null)
      {
        // Time to zap?
        if (DateTime.Now >= m_zaptime)
        {
          // Zapping to another group?
          if (m_zapgroup != -1 && m_zapgroup != m_currentgroup)
          {
            // Change current group and zap to the first channel of the group
            m_currentgroup = m_zapgroup;
            if (CurrentGroup!= null && CurrentGroup.ReferringGroupMap().Count > 0)
            {
              GroupMap gm = (GroupMap)CurrentGroup.ReferringGroupMap()[0];
              Channel chan = (Channel)gm.ReferencedChannel();
              m_zapchannel = chan;
            }
          }
          m_zapgroup = -1;

          //if (m_zapchannel != m_currentchannel)
          //  lastViewedChannel = m_currentchannel;
          // Zap to desired channel
          Channel zappingTo = m_zapchannel;

          //remember to apply the new group also.
          if (m_zapchannel.CurrentGroup != null)
          {
            m_currentgroup = GetGroupIndex(m_zapchannel.CurrentGroup.GroupName);
            MediaPortal.GUI.Library.Log.Info("Channel change:{0} on group {1}", zappingTo.DisplayName, m_zapchannel.CurrentGroup.GroupName);
          }
          else
          {
            MediaPortal.GUI.Library.Log.Info("Channel change:{0}", zappingTo.DisplayName);
          }

          m_zapchannel = null;

          TVHome.ViewChannel(zappingTo);
          reentrant = false;

          return true;
        }
      }

      reentrant = false;
      return false;
    }

    /// <summary>
    /// Changes the current channel group.
    /// </summary>
    /// <param name="groupname">The name of the group to change to.</param>
    public void SetCurrentGroup(string groupname)
    {
      m_currentgroup = GetGroupIndex(groupname);
    }

    /// <summary>
    /// Changes the current channel group.
    /// </summary>
    /// <param name="groupIndex">The id of the group to change to.</param>
    public void SetCurrentGroup(int groupIndex)
    {
      m_currentgroup = groupIndex;
    }



    /// <summary>
    /// Ensures that the navigator has the correct current channel (retrieved from the Recorder).
    /// </summary>
    public void UpdateCurrentChannel()
    {
      Channel newChannel = null;
      //if current card is watching tv then use that channel
      int id;


      if (!TVHome.HandleServerNotConnected())
      {

        if (TVHome.Card.IsTimeShifting || TVHome.Card.IsRecording)
        {
          id = TVHome.Card.IdChannel;
          if (id >= 0)
            newChannel = Channel.Retrieve(id);
        }
        else
        {
          // else if any card is recording
          // then get & use that channel
          TvServer server = new TvServer();
          if (server.IsAnyCardRecording())
          {
            for (int i = 0; i < server.Count; ++i)
            {
              User user = new User();
              VirtualCard card = server.CardByIndex(user, i);
              if (card.IsRecording)
              {
                id = card.IdChannel;
                if (id >= 0)
                {
                  newChannel = Channel.Retrieve(id);
                  break;
                }
              }
            }
          }
        }
        if (newChannel == null)
          newChannel = m_currentChannel;
        if (m_currentChannel != newChannel && newChannel != null)
        {
          m_currentChannel = newChannel;
          m_currentChannel.CurrentGroup = CurrentGroup;
        }
      }
    }

    /// <summary>
    /// Changes the current channel after a specified delay.
    /// </summary>
    /// <param name="channelName">The channel to switch to.</param>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToChannel(Channel channel, bool useZapDelay)
    {
      Log.Debug("ChannelNavigator.ZapToChannel {0} - zapdelay {1}", channel.DisplayName, useZapDelay);
      TVHome.UserChannelChanged = true;
      m_zapchannel = channel;

      if (useZapDelay)
        m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
      else
        m_zaptime = DateTime.Now;
    }

    private void GetChannels(bool refresh)
    {
      if (refresh)
        _channelList = new List<Channel>();
      if (_channelList == null)
        _channelList = new List<Channel>();
      if (_channelList.Count == 0)
      {
        try
        {
          if (TVHome.Navigator.CurrentGroup != null)
          {
            foreach (GroupMap chan in TVHome.Navigator.CurrentGroup.ReferringGroupMap())
            {
              Channel ch = chan.ReferencedChannel();
              if (ch.VisibleInGuide && ch.IsTv)
                _channelList.Add(ch);
            }
          }
        }
        catch
        {
        }

        if (_channelList.Count == 0)
        {
          Channel newChannel = new Channel(GUILocalizeStrings.Get(911), false, true, 0, DateTime.MinValue, false, DateTime.MinValue, 0, true, "", true, GUILocalizeStrings.Get(911));
          for (int i = 0; i < 10; ++i)
            _channelList.Add(newChannel);
        }
      }
    }

    /// <summary>
    /// Changes the current channel (based on channel number) after a specified delay.
    /// </summary>
    /// <param name="channelNr">The nr of the channel to change to.</param>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToChannelNumber(int channelNr, bool useZapDelay)
    {
      IList channels = CurrentGroup.ReferringGroupMap();
      if (channelNr >= 0)
      {
        bool found = false;
        int iCounter = 0;
        Channel chan;
        GetChannels(true);
        while (iCounter < channels.Count && found == false)
        {
          chan = (Channel)_channelList[iCounter];
          foreach (TuningDetail detail in chan.ReferringTuningDetail())
          {
            if (detail.ChannelNumber == channelNr)
            {
              Log.Debug("find channel: iCounter {0}, detail.ChannelNumber {1}, detail.name {2}, channels.Count {3}", iCounter, detail.ChannelNumber, detail.Name, channels.Count);
              found = true;
              ZapToChannel(iCounter + 1, useZapDelay);
            }
          }
          iCounter++;
        }
      }
    }

    /// <summary>
    /// Changes the current channel after a specified delay.
    /// </summary>
    /// <param name="channelNr">The nr of the channel to change to.</param>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToChannel(int channelNr, bool useZapDelay)
    {
      IList channels = CurrentGroup.ReferringGroupMap();
      channelNr--;
      if (channelNr >= 0 && channelNr < channels.Count)
      {
        GroupMap gm = (GroupMap)channels[channelNr];
        Channel chan = gm.ReferencedChannel();
        TVHome.UserChannelChanged = true;
        ZapToChannel(chan, useZapDelay);
      }
    }

    /// <summary>
    /// Changes to the next channel in the current group.
    /// </summary>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToNextChannel(bool useZapDelay)
    {
      Channel currentChan = null;
      int currindex;
      if (m_zapchannel == null)
      {
        currindex = GetChannelIndex(Channel);
        currentChan = Channel;
      }
      else
      {
        currindex = GetChannelIndex(m_zapchannel); // Zap from last zap channel 
        currentChan = Channel;
      }
      GroupMap gm;
      Channel chan;
      //check if channel is visible 
      //if not find next visible 
      do
      {
        // Step to next channel 
        currindex++;
        if (currindex >= CurrentGroup.ReferringGroupMap().Count)
        {
          currindex = 0;
        }
        gm = (GroupMap)CurrentGroup.ReferringGroupMap()[currindex];
        chan = (Channel)gm.ReferencedChannel();
      }
      while (!chan.VisibleInGuide);

      TVHome.UserChannelChanged = true;
      m_zapchannel = chan;
      MediaPortal.GUI.Library.Log.Info("Navigator:ZapNext {0}->{1}", currentChan.DisplayName, m_zapchannel.DisplayName);
      if (GUIWindowManager.ActiveWindow == (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
      {
        if (useZapDelay)
          m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
        else
          m_zaptime = DateTime.Now;
      }
      else
      {
        m_zaptime = DateTime.Now;
      }
    }

    /// <summary>
    /// Changes to the previous channel in the current group.
    /// </summary>
    /// <param name="useZapDelay">If true, the configured zap delay is used. Otherwise it zaps immediately.</param>
    public void ZapToPreviousChannel(bool useZapDelay)
    {
      Channel currentChan = null;
      int currindex;
      if (m_zapchannel == null)
      {
        currentChan = Channel;
        currindex = GetChannelIndex(Channel);
      }
      else
      {
        currentChan = m_zapchannel;
        currindex = GetChannelIndex(m_zapchannel); // Zap from last zap channel 
      }
      GroupMap gm;
      Channel chan;
      //check if channel is visible 
      //if not find next visible 
      do
      { // Step to prev channel 
        currindex--;
        if (currindex < 0)
        {
          currindex = CurrentGroup.ReferringGroupMap().Count - 1;
        }
        gm = (GroupMap)CurrentGroup.ReferringGroupMap()[currindex];
        chan = (Channel)gm.ReferencedChannel();
      }
      while (!chan.VisibleInGuide);

      TVHome.UserChannelChanged = true;
      m_zapchannel = chan;
      MediaPortal.GUI.Library.Log.Info("Navigator:ZapPrevious {0}->{1}",
      currentChan.DisplayName, m_zapchannel.DisplayName);
      if (GUIWindowManager.ActiveWindow == (int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN)
      {
        if (useZapDelay)
          m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
        else
          m_zaptime = DateTime.Now;
      }
      else
      {
        m_zaptime = DateTime.Now;
      }
    }

    /// <summary>
    /// Changes to the next channel group.
    /// </summary>
    public void ZapToNextGroup(bool useZapDelay)
    {
      if (m_zapgroup == -1)
        m_zapgroup = m_currentgroup + 1;
      else
        m_zapgroup = m_zapgroup + 1;			// Zap from last zap group

      if (m_zapgroup >= m_groups.Count)
        m_zapgroup = 0;

      if (useZapDelay)
        m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
      else
        m_zaptime = DateTime.Now;
    }

    /// <summary>
    /// Changes to the previous channel group.
    /// </summary>
    public void ZapToPreviousGroup(bool useZapDelay)
    {
      if (m_zapgroup == -1)
        m_zapgroup = m_currentgroup - 1;
      else
        m_zapgroup = m_zapgroup - 1;

      if (m_zapgroup < 0)
        m_zapgroup = m_groups.Count - 1;

      if (useZapDelay)
        m_zaptime = DateTime.Now.AddMilliseconds(m_zapdelay);
      else
        m_zaptime = DateTime.Now;
    }

    /// <summary>
    /// Zaps to the last viewed Channel (without ZapDelay).  // mPod
    /// </summary>
    public void ZapToLastViewedChannel()
    {

      if (_lastViewedChannel != null)
      {
        TVHome.UserChannelChanged = true;
        m_zapchannel = _lastViewedChannel;
        m_zaptime = DateTime.Now;
      }

    }
    #endregion

    #region Private methods


    /// <summary>
    /// Retrieves the index of the current channel.
    /// </summary>
    /// <returns></returns>
    private int GetChannelIndex(Channel ch)
    {
      IList groupMaps = CurrentGroup.ReferringGroupMap();
      for (int i = 0; i < groupMaps.Count; i++)
      {
        GroupMap gm = (GroupMap)groupMaps[i];
        Channel chan = (Channel)gm.ReferencedChannel();
        if (chan.IdChannel == ch.IdChannel)
          return i;
      }
      return 0; // Not found, return first channel index
    }

    /// <summary>
    /// Retrieves the index of the group with the specified name.
    /// </summary>
    /// <param name="groupname"></param>
    /// <returns></returns>
    private int GetGroupIndex(string groupname)
    {
      for (int i = 0; i < m_groups.Count; i++)
      {
        ChannelGroup group = (ChannelGroup)m_groups[i];
        if (group.GroupName == groupname)
          return i;
      }
      return -1;
    }

    public Channel GetChannel(int channelId)
    {
      foreach (Channel chan in channels)
      {
        if (chan.IdChannel == channelId && chan.VisibleInGuide) return chan;
      }
      return null;
    }

    public Channel GetChannel(string channelName)
    {
      foreach (Channel chan in channels)
      {
        if (chan.DisplayName == channelName && chan.VisibleInGuide) return chan;
      }
      return null;
    }

    #endregion

    #region Serialization

    public void LoadSettings(MediaPortal.Profile.Settings xmlreader)
    {
      MediaPortal.GUI.Library.Log.Info("ChannelNavigator::LoadSettings()");
      string currentchannelName = xmlreader.GetValueAsString("mytv", "channel", String.Empty);
      m_zapdelay = 1000 * xmlreader.GetValueAsInt("movieplayer", "zapdelay", 2);
      string groupname = xmlreader.GetValueAsString("mytv", "group", GUILocalizeStrings.Get(972));
      m_currentgroup = GetGroupIndex(groupname);
      if (m_currentgroup < 0 || m_currentgroup >= m_groups.Count)		// Group no longer exists?
        m_currentgroup = 0;

      m_currentChannel = GetChannel(currentchannelName);

      if (m_currentChannel == null)
      {
        if (m_currentgroup < m_groups.Count)
        {
          ChannelGroup group = (ChannelGroup)m_groups[m_currentgroup];
          if (group.ReferringGroupMap().Count > 0)
          {
            GroupMap gm = (GroupMap)group.ReferringGroupMap()[0];
            m_currentChannel = gm.ReferencedChannel();
          }
        }
      }

      //check if the channel does indeed belong to the group read from the XML setup file ?



      bool foundMatchingGroupName = false;

      if (m_currentChannel != null)
      {
        foreach (GroupMap groupMap in m_currentChannel.ReferringGroupMap())
        {
          if (groupMap.ReferencedChannelGroup().GroupName == groupname)
          {
            foundMatchingGroupName = true;
            break;
          }
        }
      }

      //if we still havent found the right group, then iterate through the selected group and find the channelname.      
      if (!foundMatchingGroupName && m_currentChannel != null && m_groups != null)
      {
        foreach (GroupMap groupMap in ((ChannelGroup)m_groups[m_currentgroup]).ReferringGroupMap())
        {
          if (groupMap.ReferencedChannel().DisplayName == currentchannelName)
          {
            foundMatchingGroupName = true;
            m_currentChannel = GetChannel(groupMap.ReferencedChannel().IdChannel);
            break;
          }
        }
      }


      // if the groupname does not match any of the groups assigned to the channel, then find the last group avail. (avoiding the all "channels group") for that channel and set is as the new currentgroup
      if (!foundMatchingGroupName && m_currentChannel != null && m_currentChannel.ReferringGroupMap().Count > 0)
      {
        GroupMap groupMap = (GroupMap)m_currentChannel.ReferringGroupMap()[m_currentChannel.ReferringGroupMap().Count - 1];
        m_currentgroup = GetGroupIndex(groupMap.ReferencedChannelGroup().GroupName);
        if (m_currentgroup < 0 || m_currentgroup >= m_groups.Count)		// Group no longer exists?
          m_currentgroup = 0;
      }

      if (m_currentChannel != null)
      {
        m_currentChannel.CurrentGroup = CurrentGroup;
      }

    }

    public void SaveSettings(MediaPortal.Profile.Settings xmlwriter)
    {
      string groupName = "";
      if (CurrentGroup != null)
      {
        groupName = CurrentGroup.GroupName.Trim();
        try
        {
          if (groupName != String.Empty)
          {
            if (m_currentgroup > -1)
            {
              groupName = ((ChannelGroup)m_groups[m_currentgroup]).GroupName;
            }
            else if (m_currentChannel != null)
            {
              groupName = m_currentChannel.CurrentGroup.GroupName;
            }

            if (groupName.Length > 0)
            {
              xmlwriter.SetValue("mytv", "group", groupName);
            }
          }
        }
        catch (Exception)
        {
        }
      }

      if (m_currentChannel != null)
      {
        try
        {
          if (m_currentChannel.IsTv)
          {
            bool foundMatchingGroupName = false;

            foreach (GroupMap groupMap in m_currentChannel.ReferringGroupMap())
            {
              if (groupMap.ReferencedChannelGroup().GroupName == groupName)
              {
                foundMatchingGroupName = true;
                break;
              }
            }
            if (foundMatchingGroupName)
            {
              xmlwriter.SetValue("mytv", "channel", m_currentChannel.DisplayName);
            }
            else //the channel did not belong to the group, then pick the first channel avail in the group and set this as the last channel.
            {
              if (m_currentgroup > -1)
              {
                ChannelGroup cg = (ChannelGroup)m_groups[m_currentgroup];
                if (cg.ReferringGroupMap().Count > 0)
                {
                  GroupMap gm = (GroupMap)cg.ReferringGroupMap()[0];
                  xmlwriter.SetValue("mytv", "channel", gm.ReferencedChannel().DisplayName);
                }
              }
            }
          }
        }
        catch (Exception)
        {
        }
      }
    }

    #endregion
  }

  #endregion
}
