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

#region usings
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;

using TvDatabase;
using TvControl;
using TvLibrary.Interfaces;

using Gentle.Common;
using Gentle.Framework;
#endregion

namespace TvPlugin
{
  /// <summary>
  /// 
  /// </summary>
  public class TvFullScreen : GUIWindow, IRenderLayer
  {
    #region FullScreenState class
    class FullScreenState
    {
      public int SeekStep = 1;
      public int Speed = 1;
      public bool OsdVisible = false;
      public bool Paused = false;
      public bool MsnVisible = false;
      public bool ContextMenuVisible = false;
      public bool ShowStatusLine = false;
      public bool ShowTime = false;
      public bool ZapOsdVisible = false;
      public bool MsgBoxVisible = false;
      public bool ShowGroup = false;
      public bool ShowInput = false;
      public bool _notifyDialogVisible = false;
      public bool _bottomDialogMenuVisible = false;
      public bool wasVMRBitmapVisible = false;
      public bool volumeVisible = false;
      public bool _dialogYesNoVisible = false;
    }
    #endregion

    #region variables
    bool _stepSeekVisible = false;
    bool _statusVisible = false;
    bool _groupVisible = false;
    bool _byIndex = false;
    DateTime _statusTimeOutTimer = DateTime.Now;
    ///@
    TvZapOsd _zapWindow = null;
    TvOsd _osdWindow = null;
    ///GUITVMSNOSD _msnWindow = null;
    DateTime _osdTimeoutTimer;
    DateTime _zapTimeOutTimer;
    DateTime _groupTimeOutTimer;
    DateTime _vmr7UpdateTimer = DateTime.Now;
    //		string			m_sZapChannel;
    //		long				m_iZapDelay;
    bool _isOsdVisible = false;
    bool _zapOsdVisible = false;
    bool _msnWindowVisible = false;
    bool _channelInputVisible = false;

    long _timeOsdOnscreen;
    long _zapTimeOutValue;
    DateTime _updateTimer = DateTime.Now;
    bool _lastPause = false;
    int _lastSpeed = 1;
    DateTime _keyPressedTimer = DateTime.Now;
    string _channelName = "";
    bool _isDialogVisible = false;
    bool _isMsnChatPopup = false;
    GUIDialogMenu dlg;
    GUIDialogNotify _dialogNotify = null;
    GUIDialogMenuBottomRight _dialogBottomMenu = null;
    GUIDialogYesNo _dlgYesNo = null;
    // Message box
    bool _dialogYesNoVisible = false;
    bool _notifyDialogVisible = false;
    bool _bottomDialogMenuVisible = false;
    bool _messageBoxVisible = false;
    DateTime _msgTimer = DateTime.Now;
    int _msgBoxTimeout = 0;
    int _notifyTVTimeout = 15;
    bool _playNotifyBeep = true;
    bool _needToClearScreen = false;
    bool _useVMR9Zap = false;
    ///@
    ///VMR9OSD _vmr9OSD = null;
    FullScreenState _screenState = new FullScreenState();
    bool _isVolumeVisible = false;
    DateTime _volumeTimer = DateTime.MinValue;
    bool _isStartingTSForRecording = false;

    [SkinControlAttribute(500)]
    protected GUIImage imgVolumeMuteIcon;
    [SkinControlAttribute(501)]
    protected GUIVolumeBar imgVolumeBar;

    string lastChannelWithNoSignal = string.Empty;
    VideoRendererStatistics.State videoState = VideoRendererStatistics.State.VideoPresent;
    #endregion

    #region enums
    enum Control
    {
      BLUE_BAR = 0
    ,
      MSG_BOX = 2
    ,
      MSG_BOX_LABEL1 = 3
    ,
      MSG_BOX_LABEL2 = 4
    ,
      MSG_BOX_LABEL3 = 5
    ,
      MSG_BOX_LABEL4 = 6
    ,
      LABEL_ROW1 = 10
    ,
      LABEL_ROW2 = 11
    ,
      LABEL_ROW3 = 12
    ,
      IMG_PAUSE = 16
    ,
      IMG_2X = 17
    ,
      IMG_4X = 18
    ,
      IMG_8X = 19
    ,
      IMG_16X = 20
    ,
      IMG_32X = 21

    ,
      IMG_MIN2X = 23
    ,
      IMG_MIN4X = 24
    ,
      IMG_MIN8X = 25
    ,
      IMG_MIN16X = 26
    ,
      IMG_MIN32X = 27
    ,
      LABEL_CURRENT_TIME = 22
    ,
      OSD_VIDEOPROGRESS = 100
    , REC_LOGO = 39
    };
    #endregion

    public TvFullScreen()
    {
      Log.Write("TvFullScreen:ctor");
      GetID = (int)GUIWindow.Window.WINDOW_TVFULLSCREEN;
    }

    public override void OnAdded()
    {
      Log.Write("TvFullScreen:OnAdded");
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_TVFULLSCREEN, this);
      Restore();
      PreInit();
      ResetAllControls();
    }
    public override bool SupportsDelayedLoad
    {
      get
      {
        return false;
      }
    }

    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Gets called by the runtime when a  window will be destroyed
    /// Every window window should override this method and cleanup any resources
    /// </summary>
    /// <returns></returns>
    public override void DeInit()
    {
      OnPageDestroy(-1);
    }


    public override bool Init()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        _useVMR9Zap = xmlreader.GetValueAsBool("general", "useVMR9ZapOSD", false);
        _notifyTVTimeout = xmlreader.GetValueAsInt("movieplayer", "notifyTVTimeout", 10);
        _playNotifyBeep = xmlreader.GetValueAsBool("movieplayer", "notifybeep", true);
      }
      Load(GUIGraphicsContext.Skin + @"\mytvFullScreen.xml");
      GetID = (int)GUIWindow.Window.WINDOW_TVFULLSCREEN;

      Log.Write("TvFullScreen:Init");
      return true;
    }

    #region serialisation
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        _isMsnChatPopup = (xmlreader.GetValueAsInt("MSNmessenger", "popupwindow", 0) == 1);
        _timeOsdOnscreen = 1000 * xmlreader.GetValueAsInt("movieplayer", "osdtimeout", 5);
        //				m_iZapDelay = 1000*xmlreader.GetValueAsInt("movieplayer","zapdelay",2);
        _zapTimeOutValue = 1000 * xmlreader.GetValueAsInt("movieplayer", "zaptimeout", 5);
        _byIndex = xmlreader.GetValueAsBool("mytv", "byindex", true);
        string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "normal");
        if (strValue.Equals("zoom")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom;
        if (strValue.Equals("stretch")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Stretch;
        if (strValue.Equals("normal")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Normal;
        if (strValue.Equals("original")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Original;
        if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
        if (strValue.Equals("panscan")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.PanScan43;
      }

    }

    //		public string ZapChannel
    //		{
    //			set
    //			{
    //				m_sZapChannel = value;
    //			}
    //			get
    //			{
    //				return m_sZapChannel;
    //			}
    //		}
    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        TVHome.Navigator.SaveSettings(xmlwriter);
        switch (GUIGraphicsContext.ARType)
        {
          case MediaPortal.GUI.Library.Geometry.Type.Zoom:
            xmlwriter.SetValue("mytv", "defaultar", "zoom");
            break;

          case MediaPortal.GUI.Library.Geometry.Type.Stretch:
            xmlwriter.SetValue("mytv", "defaultar", "stretch");
            break;

          case MediaPortal.GUI.Library.Geometry.Type.Normal:
            xmlwriter.SetValue("mytv", "defaultar", "normal");
            break;

          case MediaPortal.GUI.Library.Geometry.Type.Original:
            xmlwriter.SetValue("mytv", "defaultar", "original");
            break;
          case MediaPortal.GUI.Library.Geometry.Type.LetterBox43:
            xmlwriter.SetValue("mytv", "defaultar", "letterbox");
            break;

          case MediaPortal.GUI.Library.Geometry.Type.PanScan43:
            xmlwriter.SetValue("mytv", "defaultar", "panscan");
            break;
        }
      }
    }
    #endregion

    public override void OnAction(Action action)
    {

      _needToClearScreen = true;

      if (action.wID == Action.ActionType.ACTION_SHOW_VOLUME)
      {
        _volumeTimer = DateTime.Now;
        _isVolumeVisible = true;
        RenderVolume(_isVolumeVisible);
        //				if(_vmr9OSD!=null)
        //					_vmr9OSD.RenderVolumeOSD();
      }
      //ACTION_SHOW_CURRENT_TV_INFO
      if (action.wID == Action.ActionType.ACTION_SHOW_CURRENT_TV_INFO)
      {
        //if(_vmr9OSD!=null)
        //	_vmr9OSD.RenderCurrentShowInfo();
      }

      if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK && action.MouseButton == MouseButtons.Right)
      {
        // switch back to the menu
        _isOsdVisible = false;
        _msnWindowVisible = false;
        GUIWindowManager.IsOsdVisible = false;
        GUIGraphicsContext.IsFullScreenVideo = false;
        GUIWindowManager.ShowPreviousWindow();
        return;
      }
      if (_isOsdVisible)
      {
        if (((action.wID == Action.ActionType.ACTION_SHOW_OSD) || (action.wID == Action.ActionType.ACTION_SHOW_GUI) || (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)) && !_osdWindow.SubMenuVisible) // hide the OSD
        {
          lock (this)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0, GetID, 0, null);
            _osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
            _isOsdVisible = false;
            GUIWindowManager.IsOsdVisible = false;
            return;
          }
        }
        else
        {
          _osdTimeoutTimer = DateTime.Now;
          if (action.wID == Action.ActionType.ACTION_MOUSE_MOVE || action.wID == Action.ActionType.ACTION_MOUSE_CLICK)
          {
            int x = (int)action.fAmount1;
            int y = (int)action.fAmount2;
            if (!GUIGraphicsContext.MouseSupport)
            {
              _osdWindow.OnAction(action);	// route keys to OSD window

              return;
            }
            else
            {
              if (_osdWindow.InWindow(x, y))
              {
                _osdWindow.OnAction(action);	// route keys to OSD window

                if (_zapOsdVisible)
                {
                  GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _zapWindow.GetID, 0, 0, GetID, 0, null);
                  _zapWindow.OnMessage(msg);
                  _zapOsdVisible = false;
                }

                return;
              }
              else
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0, GetID, 0, null);
                _osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
                _isOsdVisible = false;
                GUIWindowManager.IsOsdVisible = false;
                return;
              }
            }
          }
          _osdWindow.OnAction(action);
          return;
        }
      }
      //@
      /*
       * else if (_msnWindowVisible)
      {
        if (((action.wID == Action.ActionType.ACTION_SHOW_OSD) || (action.wID == Action.ActionType.ACTION_SHOW_GUI))) // hide the OSD
        {
          lock (this)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _msnWindow.GetID, 0, 0, GetID, 0, null);
            _msnWindow.OnMessage(msg);	// Send a de-init msg to the OSD
            _msnWindowVisible = false;
            GUIWindowManager.IsOsdVisible = false;
          }
          return;
        }
        if (action.wID == Action.ActionType.ACTION_KEY_PRESSED)
        {
          _msnWindow.OnAction(action);

          return;
        }
      }*/

      else if (action.wID == Action.ActionType.ACTION_MOUSE_MOVE && GUIGraphicsContext.MouseSupport)
      {
        int y = (int)action.fAmount2;
        if (y > GUIGraphicsContext.Height - 100)
        {
          _osdTimeoutTimer = DateTime.Now;
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, _osdWindow.GetID, 0, 0, GetID, 0, null);
          _osdWindow.OnMessage(msg);	// Send an init msg to the OSD
          _isOsdVisible = true;
          GUIWindowManager.VisibleOsd = GUIWindow.Window.WINDOW_TVOSD;
        }
      }
      else if (_zapOsdVisible)
      {
        if ((action.wID == Action.ActionType.ACTION_SHOW_GUI) || (action.wID == Action.ActionType.ACTION_SHOW_OSD) || (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU))
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _zapWindow.GetID, 0, 0, GetID, 0, null);
          _zapWindow.OnMessage(msg);
          _zapOsdVisible = false;

        }
      }
      //Log.WriteFile(Log.LogType.Error, "action:{0}",action.wID);
      switch (action.wID)
      {
        case Action.ActionType.ACTION_MOUSE_DOUBLECLICK:
        case Action.ActionType.ACTION_SELECT_ITEM:
          {
            if (_zapOsdVisible)
            {
              TVHome.Navigator.ZapNow();
            }
            else
            {
              TvMiniGuide miniGuide = (TvMiniGuide)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MINI_GUIDE);
              _isDialogVisible = true;
              miniGuide.DoModal(GetID);
              _isDialogVisible = false;

              // LastChannel has been moved to "0"
              //if (!GUIWindowManager.IsRouted)
              //{
              //  GUITVHome.OnLastViewedChannel();
              //}
            }
          }
          break;

        case Action.ActionType.ACTION_SHOW_INFO:
        case Action.ActionType.ACTION_SHOW_CURRENT_TV_INFO:
          {
            if (action.fAmount1 != 0)
            {
              _zapTimeOutTimer = DateTime.MaxValue;
              _zapTimeOutTimer = DateTime.Now;
            }
            else
            {
              _zapTimeOutTimer = DateTime.Now;
            }

            if (!_zapOsdVisible)
            {
              if (!_useVMR9Zap)
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, _zapWindow.GetID, 0, 0, GetID, 0, null);
                _zapWindow.OnMessage(msg);
                Log.Write("ZAP OSD:ON");
                _zapTimeOutTimer = DateTime.Now;
                _zapOsdVisible = true;
              }
            }
            else
            {
              _zapWindow.UpdateChannelInfo();
              _zapTimeOutTimer = DateTime.Now;
            }
          }
          break;
        case Action.ActionType.ACTION_SHOW_MSN_OSD:
          if (_isMsnChatPopup)
          {
            Log.Write("MSN CHAT:ON");

            _msnWindowVisible = true;
            GUIWindowManager.VisibleOsd = GUIWindow.Window.WINDOW_TVMSNOSD;
            ///@
            ///_msnWindow.DoModal(GetID, null);
            _msnWindowVisible = false;
            GUIWindowManager.IsOsdVisible = false;
          }
          break;

        case Action.ActionType.ACTION_ASPECT_RATIO:
          {
            _statusVisible = true;
            _statusTimeOutTimer = DateTime.Now;
            string status = "";
            switch (GUIGraphicsContext.ARType)
            {
              case MediaPortal.GUI.Library.Geometry.Type.Zoom:
                GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Stretch;
                status = GUILocalizeStrings.Get(942); // "Stretch";
                break;

              case MediaPortal.GUI.Library.Geometry.Type.Stretch:
                GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Normal;
                status = GUILocalizeStrings.Get(943); //"Normal";
                break;

              case MediaPortal.GUI.Library.Geometry.Type.Normal:
                GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Original;
                status = GUILocalizeStrings.Get(944); //"Original";
                break;

              case MediaPortal.GUI.Library.Geometry.Type.Original:
                GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
                status = GUILocalizeStrings.Get(945); //"Letterbox 4:3";
                break;

              case MediaPortal.GUI.Library.Geometry.Type.LetterBox43:
                GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.PanScan43;
                status = GUILocalizeStrings.Get(946); //"Pan and Scan 4:3";
                break;

              case MediaPortal.GUI.Library.Geometry.Type.PanScan43:
                GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom;
                status = GUILocalizeStrings.Get(947); //"Zoom";
                break;
            }

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.LABEL_ROW1, 0, 0, null);
            msg.Label = status;
            OnMessage(msg);

            SaveSettings();
          }
          break;

        case Action.ActionType.ACTION_PAGE_UP:
          OnPageUp();
          break;

        case Action.ActionType.ACTION_PAGE_DOWN:
          OnPageDown();
          break;

        case Action.ActionType.ACTION_KEY_PRESSED:
          {
            if ((action.m_key != null) && (!_msnWindowVisible))
              OnKeyCode((char)action.m_key.KeyChar);

            _messageBoxVisible = false;
          }
          break;

        case Action.ActionType.ACTION_REWIND:
          {
            if (g_Player.IsTimeShifting)
            {
              g_Player.Speed = Utils.GetNextRewindSpeed(g_Player.Speed);
              if (g_Player.Paused) g_Player.Pause();

              ScreenStateChanged();
              UpdateGUI();
            }
          }
          break;

        case Action.ActionType.ACTION_FORWARD:
          {
            if (g_Player.IsTimeShifting)
            {
              g_Player.Speed = Utils.GetNextForwardSpeed(g_Player.Speed);
              if (g_Player.Paused) g_Player.Pause();

              ScreenStateChanged();
              UpdateGUI();

            }
          }
          break;

        case Action.ActionType.ACTION_PREVIOUS_MENU:
        case Action.ActionType.ACTION_SHOW_GUI:
          Log.Write("fullscreentv:show gui");
          //if(_vmr9OSD!=null)
          //	_vmr9OSD.HideBitmap();
          GUIWindowManager.ShowPreviousWindow();
          return;

        case Action.ActionType.ACTION_SHOW_OSD:	// Show the OSD
          {
            Log.Write("OSD:ON");
            _osdTimeoutTimer = DateTime.Now;

            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, _osdWindow.GetID, 0, 0, GetID, 0, null);
            _osdWindow.OnMessage(msg);	// Send an init msg to the OSD
            _isOsdVisible = true;
            GUIWindowManager.VisibleOsd = GUIWindow.Window.WINDOW_TVOSD;


          }
          break;

        case Action.ActionType.ACTION_MOVE_LEFT:
        case Action.ActionType.ACTION_STEP_BACK:
          {
            if (g_Player.IsTimeShifting)
            {
              _stepSeekVisible = true;
              _statusTimeOutTimer = DateTime.Now;
              g_Player.SeekStep(false);
              string strStatus = g_Player.GetStepDescription();
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.LABEL_ROW1, 0, 0, null);
              msg.Label = strStatus;
              OnMessage(msg);
            }
          }
          break;

        case Action.ActionType.ACTION_MOVE_RIGHT:
        case Action.ActionType.ACTION_STEP_FORWARD:
          {
            if (g_Player.IsTimeShifting)
            {
              _stepSeekVisible = true;
              _statusTimeOutTimer = DateTime.Now;
              g_Player.SeekStep(true);
              string strStatus = g_Player.GetStepDescription();
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.LABEL_ROW1, 0, 0, null);
              msg.Label = strStatus;
              OnMessage(msg);
            }
          }
          break;

        case Action.ActionType.ACTION_MOVE_DOWN:
        case Action.ActionType.ACTION_BIG_STEP_BACK:
          {
            if (g_Player.IsTimeShifting)
            {
              _statusVisible = true;
              _statusTimeOutTimer = DateTime.Now;
              g_Player.SeekRelativePercentage(-10);
            }
          }
          break;

        case Action.ActionType.ACTION_MOVE_UP:
        case Action.ActionType.ACTION_BIG_STEP_FORWARD:
          {
            if (g_Player.IsTimeShifting)
            {
              _statusVisible = true;
              _statusTimeOutTimer = DateTime.Now;
              g_Player.SeekRelativePercentage(10);
            }
          }
          break;

        case Action.ActionType.ACTION_PAUSE:
          {
            if (g_Player.IsTimeShifting)
            {
              g_Player.Pause();
            }

            ScreenStateChanged();
            UpdateGUI();
            if (g_Player.Paused)
            {
              if ((GUIGraphicsContext.Vmr9Active && VMR9Util.g_vmr9 != null))
              {
                VMR9Util.g_vmr9.SetRepaint();
                VMR9Util.g_vmr9.Repaint();// repaint vmr9
              }
            }
          }
          break;

        case Action.ActionType.ACTION_PLAY:
        case Action.ActionType.ACTION_MUSIC_PLAY:
          if (g_Player.IsTimeShifting)
          {
            g_Player.StepNow();
            g_Player.Speed = 1;
            if (g_Player.Paused) g_Player.Pause();
          }
          break;

        case Action.ActionType.ACTION_CONTEXT_MENU:
          ShowContextMenu();
          break;
      }

      base.OnAction(action);
    }
    public override void SetObject(object obj)
    {
      base.SetObject(obj);
      ///@
      /// if (obj.GetType() == typeof(VMR9OSD))
      ///{
      ///  _vmr9OSD = (VMR9OSD)obj;

      ///}
    }

    public override bool OnMessage(GUIMessage message)
    {
      _needToClearScreen = true;

      #region case GUI_MSG_RECORD
      if (message.Message == GUIMessage.MessageType.GUI_MSG_RECORD)
      {
        string channel = TVHome.Card.ChannelName;
        TvBusinessLayer layer = new TvBusinessLayer();

        Program prog = TVHome.Navigator.GetChannel(channel).CurrentProgram;
        VirtualCard card;
        if (RemoteControl.Instance.IsRecording(channel, out card))
        {
          _dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
          _dlgYesNo.SetHeading(1449); // stop recording
          _dlgYesNo.SetLine(1, 1450); // are you sure to stop recording?
          if (prog != null)
            _dlgYesNo.SetLine(2, prog.Title);
          _dialogYesNoVisible = true;
          _dlgYesNo.DoModal(GetID);
          _dialogYesNoVisible = false;

          if (!_dlgYesNo.IsConfirmed) return true;

          int id = TVHome.Card.RecordingScheduleId;
          if (id > 0)
            TVHome.TvServer.StopRecordingSchedule(id);
          GUIDialogNotify dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
          if (dlgNotify == null) return true;
          string logo = Utils.GetCoverArt(Thumbs.TVChannel, channel);
          dlgNotify.Reset();
          dlgNotify.ClearAll();
          dlgNotify.SetImage(logo);
          dlgNotify.SetHeading(GUILocalizeStrings.Get(1447));//recording stopped
          if (prog != null)
          {
            dlgNotify.SetText(String.Format("{0} {1}-{2}",
              prog.Title,
              prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
              prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat)));
          }
          else
          {
            dlgNotify.SetText(GUILocalizeStrings.Get(736));//no tvguide data available
          }
          dlgNotify.TimeOut = 5;

          _notifyDialogVisible = true;
          dlgNotify.DoModal(GUIWindowManager.ActiveWindow);
          _notifyDialogVisible = false;
          return true;
        }
        else
        {
          if (prog != null)
          {
            _dialogBottomMenu = (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
            if (_dialogBottomMenu != null)
            {
              _dialogBottomMenu.Reset();
              _dialogBottomMenu.SetHeading(605);//my tv
              _dialogBottomMenu.AddLocalizedString(875); //current program
              _dialogBottomMenu.AddLocalizedString(876); //till manual stop
              _bottomDialogMenuVisible = true;

              _dialogBottomMenu.DoModal(GetID);

              _bottomDialogMenuVisible = false;
              Schedule rec;
              switch (_dialogBottomMenu.SelectedId)
              {
                case 875:
                  //record current program
                  _isStartingTSForRecording = !g_Player.IsTimeShifting;
                  Program program = TVHome.Navigator.Channel.CurrentProgram;
                  rec = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);
                  rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                  rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);

                  rec.Persist();
                  RemoteControl.Instance.OnNewSchedule();
                  break;

                case 876:
                  //manual record
                  _isStartingTSForRecording = !g_Player.IsTimeShifting;

                  rec = new Schedule(TVHome.Navigator.Channel.IdChannel, GUILocalizeStrings.Get(413) + " (" + TVHome.Navigator.Channel.Name + ")", DateTime.Now, DateTime.Now.AddDays(1));
                  rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
                  rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
                  rec.Persist();
                  RemoteControl.Instance.OnNewSchedule();
                  break;
                default:
                  return true;

              }
            }
          }
          else
          {
            _isStartingTSForRecording = !g_Player.IsTimeShifting;
            Schedule rec = new Schedule(TVHome.Navigator.Channel.IdChannel, (int)ScheduleRecordingType.Once, GUILocalizeStrings.Get(413) + " (" + TVHome.Navigator.Channel.Name + ")", DateTime.Now, DateTime.Now.AddDays(1), 1, 1, "", 1, 1, Schedule.MinSchedule, 5, 5, Schedule.MinSchedule);
            rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
            rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);

            rec.Persist();
            RemoteControl.Instance.OnNewSchedule();
          }

          // check if recorder has to start timeshifting for this recording
          if (_isStartingTSForRecording)
          {
            TVHome.ViewChannel(channel);
            _isStartingTSForRecording = false;
          }

          GUIDialogNotify dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
          if (dlgNotify == null) return true;
          string logo = Utils.GetCoverArt(Thumbs.TVChannel, channel);
          dlgNotify.Reset();
          dlgNotify.ClearAll();
          dlgNotify.SetImage(logo);
          dlgNotify.SetHeading(GUILocalizeStrings.Get(1446));//recording started
          if (prog != null)
          {
            dlgNotify.SetText(String.Format("{0} {1}-{2}",
              prog.Title,
              prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
              prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat)));
          }
          else
          {
            dlgNotify.SetText(GUILocalizeStrings.Get(736));//no tvguide data available
          }
          dlgNotify.TimeOut = 5;
          _notifyDialogVisible = true;
          dlgNotify.DoModal(GUIWindowManager.ActiveWindow);
          _notifyDialogVisible = false;
        }
        return true;
      }
      #endregion

      #region case GUI_MSG_NOTIFY_TV_PROGRAM
      if (message.Message == GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM)
      {
        _dialogNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        ///@
        /*
        Program notify = message.Object as Program;
        if (notify == null) return true;
        _dialogNotify.SetHeading(1016);
        _dialogNotify.SetText(String.Format("{0}\n{1}", notify.Title, notify.Description));
        string logo = Utils.GetCoverArt(Thumbs.TVChannel, notify.Channel);
        _dialogNotify.SetImage(logo);
        _dialogNotify.TimeOut = _notifyTVTimeout;
        _notifyDialogVisible = true;
        if (_playNotifyBeep)
          Utils.PlaySound("notify.wav", false, true);
        _dialogNotify.DoModal(GetID);
        _notifyDialogVisible = false;
         * */
      }
      #endregion

      #region case GUI_MSG_RECORDER_ABOUT_TO_START_RECORDING
      if (message.Message == GUIMessage.MessageType.GUI_MSG_RECORDER_ABOUT_TO_START_RECORDING)
      {
        /*
                TVRecording rec = message.Object as TVRecording;
                if (rec == null) return true;
                if (rec.Channel == Recorder.TVChannelName) return true;
                if (!Recorder.NeedChannelSwitchForRecording(rec)) return true;

                _messageBoxVisible = false;
                _msnWindowVisible = false;
                GUIWindowManager.IsOsdVisible = false;
                if (_zapOsdVisible)
                {
                  GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _zapWindow.GetID, 0, 0, GetID, 0, null);
                  _zapWindow.OnMessage(msg);
                  _zapOsdVisible = false;
                  GUIWindowManager.IsOsdVisible = false;
                }
                if (_isOsdVisible)
                {
                  GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0, GetID, 0, null);
                  _osdWindow.OnMessage(msg);
                  _isOsdVisible = false;
                  GUIWindowManager.IsOsdVisible = false;
                }
                if (_msnWindowVisible)
                {
                  GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _msnWindow.GetID, 0, 0, GetID, 0, null);
                  _msnWindow.OnMessage(msg);	// Send a de-init msg to the OSD
                  _msnWindowVisible = false;
                  GUIWindowManager.IsOsdVisible = false;
                }
                if (_isDialogVisible && dlg != null)
                {
                  GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, dlg.GetID, 0, 0, GetID, 0, null);
                  dlg.OnMessage(msg);	// Send a de-init msg to the OSD
                }

                _bottomDialogMenuVisible = true;
                _dialogBottomMenu = (GUIDialogMenuBottomRight)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT);
                _dialogBottomMenu.TimeOut = 10;
                _dialogBottomMenu.SetHeading(1004);//About to start recording
                _dialogBottomMenu.SetHeadingRow2(String.Format("{0} {1}", GUILocalizeStrings.Get(1005), rec.Channel));
                _dialogBottomMenu.SetHeadingRow3(rec.Title);
                _dialogBottomMenu.AddLocalizedString(1006); //Allow recording to begin
                _dialogBottomMenu.AddLocalizedString(1007); //Cancel recording and maintain watching tv
                _dialogBottomMenu.DoModal(GetID);
                if (_dialogBottomMenu.SelectedId == 1007) //cancel recording
                {
                  if (rec.RecType == TVRecording.RecordingType.Once)
                  {
                    rec.Canceled = Utils.datetolong(DateTime.Now);
                  }
                  else
                  {
                    Program prog = message.Object2 as Program;
                    if (prog != null)
                      rec.CanceledSeries.Add(prog.Start);
                    else
                      rec.CanceledSeries.Add(Utils.datetolong(DateTime.Now));
                  }
                  TVDatabase.UpdateRecording(rec, TVDatabase.RecordingChange.Canceled);
                }
         */
        _bottomDialogMenuVisible = false;
      }
      #endregion

      #region case GUI_MSG_NOTIFY
      if (message.Message == GUIMessage.MessageType.GUI_MSG_NOTIFY)
      {
        GUIDialogNotify dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        if (dlgNotify == null) return true;
        string channel = GUIPropertyManager.GetProperty("#TV.View.channel");
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, channel);
        dlgNotify.Reset();
        dlgNotify.ClearAll();
        dlgNotify.SetImage(strLogo);
        dlgNotify.SetHeading(channel);
        dlgNotify.SetText(message.Label);
        dlgNotify.TimeOut = message.Param1;
        _notifyDialogVisible = true;
        dlgNotify.DoModal(GUIWindowManager.ActiveWindow);
        _notifyDialogVisible = false;
        Log.Write("Notify Message:" + channel + ", " + message.Label);
        return true;
      }
      #endregion

      #region case GUI_MSG_WINDOW_DEINIT
      if (_isOsdVisible)
      {
        if ((message.Message != GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT))
        {
          _osdTimeoutTimer = DateTime.Now;
          // route messages to OSD window
          if (_osdWindow.OnMessage(message))
          {
            return true;
          }
        }
        else if (message.Param1 == GetID)
        {
          _osdTimeoutTimer = DateTime.Now;
          _osdWindow.OnMessage(message);
        }
      }
      #endregion

      switch (message.Message)
      {
        #region case GUI_MSG_HIDE_MESSAGE
        case GUIMessage.MessageType.GUI_MSG_HIDE_MESSAGE:
          {
            _messageBoxVisible = false;
          }
          break;
        #endregion

        #region case GUI_MSG_SHOW_MESSAGE
        case GUIMessage.MessageType.GUI_MSG_SHOW_MESSAGE:
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.MSG_BOX_LABEL1, 0, 0, null);
            msg.Label = message.Label;
            OnMessage(msg);

            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.MSG_BOX_LABEL2, 0, 0, null);
            msg.Label = message.Label2;
            OnMessage(msg);

            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.MSG_BOX_LABEL3, 0, 0, null);
            msg.Label = message.Label3;
            OnMessage(msg);

            msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.MSG_BOX_LABEL4, 0, 0, null);
            msg.Label = message.Label4;
            OnMessage(msg);

            _messageBoxVisible = true;
            // Set specified timeout
            _msgBoxTimeout = message.Param1;
            _msgTimer = DateTime.Now;
          }
          break;
        #endregion

        #region case GUI_MSG_MSN_CLOSECONVERSATION

        case GUIMessage.MessageType.GUI_MSG_MSN_CLOSECONVERSATION:
          if (_msnWindowVisible)
          {
            ///@
            /// GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _msnWindow.GetID, 0, 0, GetID, 0, null);
            ///_msnWindow.OnMessage(msg);	// Send a de-init msg to the OSD
          }
          _msnWindowVisible = false;
          GUIWindowManager.IsOsdVisible = false;
          break;

        #endregion

        #region case GUI_MSG_MSN_STATUS_MESSAGE

        case GUIMessage.MessageType.GUI_MSG_MSN_STATUS_MESSAGE:

        #endregion

        #region case GUI_MSG_MSN_MESSAGE
        case GUIMessage.MessageType.GUI_MSG_MSN_MESSAGE:
          if (_isOsdVisible && _isMsnChatPopup)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0, GetID, 0, null);
            _osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
            _isOsdVisible = false;
            GUIWindowManager.IsOsdVisible = false;

          }

          if (!_msnWindowVisible && _isMsnChatPopup)
          {
            Log.Write("MSN CHAT:ON");
            _msnWindowVisible = true;
            GUIWindowManager.VisibleOsd = GUIWindow.Window.WINDOW_TVMSNOSD;
            ///@
            ///_msnWindow.DoModal(GetID, message);
            _msnWindowVisible = false;
            GUIWindowManager.IsOsdVisible = false;

          }
          break;
        #endregion

        #region case GUI_MSG_WINDOW_DEINIT

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            Log.Write("TvFullScreen:deinit->OSD:Off");
            if (_isOsdVisible)
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0, GetID, 0, null);
              _osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
            }
            _isOsdVisible = false;
            GUIWindowManager.IsOsdVisible = false;

            if (_msnWindowVisible)
            {
              ///@
              /// GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _msnWindow.GetID, 0, 0, GetID, 0, null);
              ///_msnWindow.OnMessage(msg);	// Send a de-init msg to the OSD
            }

            _isOsdVisible = false;
            GUIWindowManager.IsOsdVisible = false;
            _channelInputVisible = false;
            _keyPressedTimer = DateTime.Now;
            _channelName = "";
            _updateTimer = DateTime.Now;

            _stepSeekVisible = false;
            _statusVisible = false;
            _groupVisible = false;
            _notifyDialogVisible = false;
            _dialogYesNoVisible = false;
            _bottomDialogMenuVisible = false;
            _statusTimeOutTimer = DateTime.Now;

            _screenState.ContextMenuVisible = false;
            _screenState.MsgBoxVisible = false;
            _screenState.MsnVisible = false;
            _screenState.OsdVisible = false;
            _screenState.Paused = false;
            _screenState.ShowGroup = false;
            _screenState.ShowInput = false;
            _screenState.ShowStatusLine = false;
            _screenState.ShowTime = false;
            _screenState.ZapOsdVisible = false;
            _needToClearScreen = false;


            base.OnMessage(message);
            GUIGraphicsContext.IsFullScreenVideo = false;
            if (!GUIGraphicsContext.IsTvWindow(message.Param1))
            {
              if (!g_Player.Playing)
              {
                if (GUIGraphicsContext.ShowBackground)
                {
                  // stop timeshifting & viewing... 

                  ///@
                  ///Recorder.StopViewing();
                }
              }
            }
            if (VMR7Util.g_vmr7 != null)
            {
              VMR7Util.g_vmr7.SaveBitmap(null, false, false, 0.8f);
            }
            /*
            if (VMR9Util.g_vmr9!=null)
            {	
              VMR9Util.g_vmr9.SaveBitmap(null,false,false,0.8f);
            }*/
            GUILayerManager.UnRegisterLayer(this);
            return true;

          }

        #endregion

        #region case GUI_MSG_WINDOW_INIT
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            LoadSettings();
            GUIGraphicsContext.IsFullScreenVideo = true;
            ///@
            GUIGraphicsContext.VideoWindow = new Rectangle(GUIGraphicsContext.OverScanLeft, GUIGraphicsContext.OverScanTop, GUIGraphicsContext.OverScanWidth, GUIGraphicsContext.OverScanHeight);
            _osdWindow = (TvOsd)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVOSD);
            _zapWindow = (TvZapOsd)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVZAPOSD);
            ///_msnWindow = (GUITVMSNOSD)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TVMSNOSD);

            _lastPause = g_Player.Paused;
            _lastSpeed = g_Player.Speed;
            ///Log.Write("start fullscreen channel:{0}", Recorder.TVChannelName);
            Log.Write("TvFullScreen:init->OSD:Off");
            _isOsdVisible = false;
            GUIWindowManager.IsOsdVisible = false;
            _channelInputVisible = false;
            _keyPressedTimer = DateTime.Now;
            _channelName = "";
            //					m_sZapChannel="";

            _isOsdVisible = false;
            GUIWindowManager.IsOsdVisible = false;
            _updateTimer = DateTime.Now;
            //					_zapTimeOutTimer=DateTime.Now;

            _stepSeekVisible = false;
            _statusVisible = false;
            _groupVisible = false;
            _notifyDialogVisible = false;
            _dialogYesNoVisible = false;
            _bottomDialogMenuVisible = false;
            _statusTimeOutTimer = DateTime.Now;
            RenderVolume(false);
            ScreenStateChanged();
            UpdateGUI();

            ///@
            /// GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
            ///try
            ///{
            ///  GUIGraphicsContext.DX9Device.Present();
            ///}
            ///catch (Exception)
            ///{
            ///}
            GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Osd);


            return true;
          }
        #endregion

        #region case GUI_MSG_SETFOCUS
        case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
          goto case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS;
        #endregion

        #region case GUI_MSG_LOSTFOCUS
        case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS:
          if (_isOsdVisible) return true;
          if (_msnWindowVisible) return true;
          if (message.SenderControlId != (int)GUIWindow.Window.WINDOW_TVFULLSCREEN) return true;
          break;
        #endregion

      }

      if (_msnWindowVisible)
      {
        ///@
        ///_msnWindow.OnMessage(message);	// route messages to MSNChat window
      }
      return base.OnMessage(message);
    }

    void ShowContextMenu()
    {

      if (dlg == null)
        dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(924); // menu

      if (GUIGraphicsContext.DBLClickAsRightClick)
        dlg.AddLocalizedString(10104); // TV MiniEPG

      //dlg.AddLocalizedString(915); // TV Channels
      dlg.AddLocalizedString(4); // TV Guide

      /*if (TVHome.Navigator.Groups.Count > 1)
        dlg.AddLocalizedString(971); // Group*/
      if (TVHome.Card.HasTeletext)
        dlg.AddLocalizedString(1441); // Fullscreen teletext
      dlg.AddLocalizedString(941); // Change aspect ratio
      if (PluginManager.IsPluginNameEnabled("MSN Messenger"))
      {
        dlg.AddLocalizedString(12902); // MSN Messenger
        dlg.AddLocalizedString(902); // MSN Online contacts
      }

      IAudioStream[] streams = TVHome.Card.AvailableAudioStreams;
      if (streams != null && streams.Length > 0)
      {
        dlg.AddLocalizedString(492); // Audio language menu
      }
      dlg.AddLocalizedString(970); // Previous window

      _isDialogVisible = true;

      dlg.DoModal(GetID);
      _isDialogVisible = false;

      Log.Write("selected id:{0}", dlg.SelectedId);
      if (dlg.SelectedId == -1) return;
      switch (dlg.SelectedId)
      {
        case 4: //TVGuide
          {
            TVGuideDialog dlgTvGuide = (TVGuideDialog)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_TVGUIDE);

            _isDialogVisible = true;
            dlgTvGuide.DoModal(GetID);
            _isDialogVisible = false;
            break;
          }
        case 10104: // MiniEPG
          {

            Log.Write("get miniguide");
            TvMiniGuide miniGuide = (TvMiniGuide)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MINI_GUIDE);
            _isDialogVisible = true;
            Log.Write("show miniguide");
            miniGuide.DoModal(GetID);
            Log.Write("done miniguide");
            _isDialogVisible = false;
            break;
          }

        //TV Channels not needed anymore after MiniEPG
        /*case 915: //TVChannels
          {
            dlg.Reset();
            dlg.SetHeading(GUILocalizeStrings.Get(915));//TV Channels
            int selected = 0;
            int i = 0;
            foreach (TVChannel channel in TVHome.Navigator.CurrentGroup.TvChannels)
            {
              GUIListItem pItem = new GUIListItem(channel.Name);
              string logo = Utils.GetCoverArt(Thumbs.TVChannel, channel.Name);
              if (System.IO.File.Exists(logo))
              {
                pItem.IconImage = logo;
              }
              dlg.Add(pItem);
              if (channel.Name == TVHome.Navigator.CurrentTVChannel.Name)
              {
                selected = i;
              }
              i++;
            }
            dlg.SelectedLabel = selected;
            _isDialogVisible = true;

            dlg.DoModal(GetID);
            _isDialogVisible = false;


            if (dlg.SelectedLabel == -1) return;
            ChangeChannelNr(dlg.SelectedLabel + 1);
          }
          break;*/

        //Groups not used anymore
        /*case 971: //group
          {
            dlg.Reset();
            dlg.SetHeading(GUILocalizeStrings.Get(971));//Group
            foreach (TVGroup group in TVHome.Navigator.Groups)
            {
              dlg.Add(group.GroupName);
            }

            _isDialogVisible = true;

            dlg.DoModal(GetID);
            _isDialogVisible = false;


            if (dlg.SelectedLabel == -1) return;
            int selectedItem = dlg.SelectedLabel;
            if (selectedItem >= 0 && selectedItem < TVHome.Navigator.Groups.Count)
            {
              TVGroup group = (TVGroup)TVHome.Navigator.Groups[selectedItem];
              TVHome.Navigator.SetCurrentGroup(group.GroupName);
            }
          }
          break;*/

        case 941: // Change aspect ratio
          ShowAspectRatioMenu();
          break;

        case 492: // Show audio language menu
          ShowAudioLanguageMenu();
          break;

        case 12902: // MSN Messenger
          Log.Write("MSN CHAT:ON");
          _msnWindowVisible = true;
          GUIWindowManager.VisibleOsd = GUIWindow.Window.WINDOW_TVMSNOSD;
          //@_msnWindow.DoModal(GetID, null);
          _msnWindowVisible = false;
          GUIWindowManager.IsOsdVisible = false;
          break;

        case 902: // Online contacts
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MSN);
          break;

        case 1441: // Fullscreen teletext
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
          break;

        case 970:
          // switch back to previous window
          _isOsdVisible = false;
          _msnWindowVisible = false;
          GUIWindowManager.IsOsdVisible = false;
          GUIGraphicsContext.IsFullScreenVideo = false;
          GUIWindowManager.ShowPreviousWindow();
          break;
      }
    }

    void ShowAspectRatioMenu()
    {
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(941); // Change aspect ratio

      dlg.AddLocalizedString(942); // Stretch
      dlg.AddLocalizedString(943); // Normal
      dlg.AddLocalizedString(944); // Original
      dlg.AddLocalizedString(945); // Letterbox
      dlg.AddLocalizedString(946); // Pan and scan
      dlg.AddLocalizedString(947); // Zoom

      _isDialogVisible = true;

      dlg.DoModal(GetID);
      _isDialogVisible = false;

      if (dlg.SelectedId == -1) return;
      _statusTimeOutTimer = DateTime.Now;
      string strStatus = "";
      switch (dlg.SelectedId)
      {
        case 942: // Stretch
          GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Stretch;
          strStatus = "Stretch";
          SaveSettings();
          break;

        case 943: // Normal
          GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Normal;
          strStatus = "Normal";
          SaveSettings();
          break;

        case 944: // Original
          GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Original;
          strStatus = "Original";
          SaveSettings();
          break;

        case 945: // Letterbox
          GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
          strStatus = "Letterbox 4:3";
          SaveSettings();
          break;

        case 946: // Pan and scan
          GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.PanScan43;
          strStatus = "PanScan 4:3";
          SaveSettings();
          break;

        case 947: // Zoom
          GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom;
          strStatus = "Zoom";
          SaveSettings();
          break;
      }
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.LABEL_ROW1, 0, 0, null);
      msg.Label = strStatus;
      OnMessage(msg);

    }

    void ShowAudioLanguageMenu()
    {

      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(492); // set audio language menu

      dlg.ShowQuickNumbers = true;

      IAudioStream[] streams = TVHome.Card.AvailableAudioStreams;
      IAudioStream streamCurrent = TVHome.Card.AudioStream;
      int selected = 0;
      for (int i = 0; i < streams.Length; i++)
      {
        if (streamCurrent == streams[i])
        {
          selected = i;
        }
        GUIListItem item = new GUIListItem();
        item.Label = String.Format("{0}:{1}", streams[i].StreamType, streams[i].Language);
        dlg.Add(item);
      }
      dlg.SelectedLabel = selected;

      _isDialogVisible = true;

      dlg.DoModal(GetID);
      _isDialogVisible = false;

      if (dlg.SelectedLabel < 0) return;

      // Set new language			
      if ((dlg.SelectedLabel >= 0) && (dlg.SelectedLabel < streams.Length))
      {
        TVHome.Card.AudioStream = streams[dlg.SelectedLabel];
        if (g_Player.Paused == false)
        {
          g_Player.Pause();
          g_Player.Pause();
        }
      }
    }


    public override void Process()
    {
      //	_isTvOn=true;

      TVHome.UpdateProgressPercentageBar();
      CheckTimeOuts();
      if (ScreenStateChanged())
      {
        UpdateGUI();
      }

      if (!VideoRendererStatistics.IsVideoFound)
      {
        if ((lastChannelWithNoSignal != TVHome.Navigator.CurrentChannel) || (videoState != VideoRendererStatistics.VideoState))
        {
          if (!_zapOsdVisible)
          {
            GUIMessage message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY, GetID, GetID, 0, 5, 0, null);
            switch (VideoRendererStatistics.VideoState)
            {
              case VideoRendererStatistics.State.NoSignal:
                message.Label = GUILocalizeStrings.Get(1034);
                break;
              case VideoRendererStatistics.State.Scrambled:
                message.Label = GUILocalizeStrings.Get(1035);
                break;
              case VideoRendererStatistics.State.Signal:
                message.Label = GUILocalizeStrings.Get(1036);
                break;
              default:
                message.Label = GUILocalizeStrings.Get(1036);
                break;
            }
            lastChannelWithNoSignal = TVHome.Navigator.CurrentChannel;
            videoState = VideoRendererStatistics.VideoState;
            OnMessage(message);
          }
        }
      }
      else
      {
        lastChannelWithNoSignal = string.Empty;
        videoState = VideoRendererStatistics.State.VideoPresent;
      }


      GUIGraphicsContext.IsFullScreenVideo = true;
    }

    public bool ScreenStateChanged()
    {
      bool updateGUI = false;
      if (g_Player.Speed != _screenState.Speed)
      {
        _screenState.Speed = g_Player.Speed;
        updateGUI = true;
      }
      if (g_Player.Paused != _screenState.Paused)
      {
        _screenState.Paused = g_Player.Paused;
        updateGUI = true;
      }
      if (_isOsdVisible != _screenState.OsdVisible)
      {
        _screenState.OsdVisible = _isOsdVisible;
        updateGUI = true;
      }
      if (_zapOsdVisible != _screenState.ZapOsdVisible)
      {
        _screenState.ZapOsdVisible = _zapOsdVisible;
        updateGUI = true;
      }
      if (_msnWindowVisible != _screenState.MsnVisible)
      {
        _screenState.MsnVisible = _msnWindowVisible;
        updateGUI = true;
      }
      if (_isDialogVisible != _screenState.ContextMenuVisible)
      {
        _screenState.ContextMenuVisible = _isDialogVisible;
        updateGUI = true;
      }

      bool bStart, bEnd;
      int step = g_Player.GetSeekStep(out bStart, out bEnd);
      if (step != _screenState.SeekStep)
      {
        if (step != 0) _stepSeekVisible = true;
        else _stepSeekVisible = false;
        _screenState.SeekStep = step;
        updateGUI = true;
      }
      if (_statusVisible != _screenState.ShowStatusLine)
      {
        _screenState.ShowStatusLine = _statusVisible;
        updateGUI = true;
      }
      if (_bottomDialogMenuVisible != _screenState._bottomDialogMenuVisible)
      {
        _screenState._bottomDialogMenuVisible = _bottomDialogMenuVisible;
        updateGUI = true;
      }
      if (_notifyDialogVisible != _screenState._notifyDialogVisible)
      {
        _screenState._notifyDialogVisible = _notifyDialogVisible;
        updateGUI = true;
      }
      if (_messageBoxVisible != _screenState.MsgBoxVisible)
      {
        _screenState.MsgBoxVisible = _messageBoxVisible;
        updateGUI = true;
      }
      if (_groupVisible != _screenState.ShowGroup)
      {
        _screenState.ShowGroup = _groupVisible;
        updateGUI = true;
      }
      if (_channelInputVisible != _screenState.ShowInput)
      {
        _screenState.ShowInput = _channelInputVisible;
        updateGUI = true;
      }
      if (_isVolumeVisible != _screenState.volumeVisible)
      {
        _screenState.volumeVisible = _isVolumeVisible;
        updateGUI = true;
        _volumeTimer = DateTime.Now;
      }
      if (_dialogYesNoVisible != _screenState._dialogYesNoVisible)
      {
        _screenState._dialogYesNoVisible = _dialogYesNoVisible;
        updateGUI = true;
      }

      if (updateGUI)
      {
        _needToClearScreen = true;
      }
      return updateGUI;
    }

    void UpdateGUI()
    {
      if ((_statusVisible || _stepSeekVisible || (!_isOsdVisible && g_Player.Speed != 1) || (!_isOsdVisible && g_Player.Paused)))
      {
        if (!_isOsdVisible)
        {
          for (int i = (int)Control.OSD_VIDEOPROGRESS; i < (int)Control.OSD_VIDEOPROGRESS + 50; ++i)
            ShowControl(GetID, i);

          // Set recorder status
          VirtualCard card;
          if (RemoteControl.Instance.IsRecording(TVHome.Navigator.CurrentChannel, out card))
          {
            ShowControl(GetID, (int)Control.REC_LOGO);
          }
        }
        else
        {
          for (int i = (int)Control.OSD_VIDEOPROGRESS; i < (int)Control.OSD_VIDEOPROGRESS + 50; ++i)
            HideControl(GetID, i);
          HideControl(GetID, (int)Control.REC_LOGO);
        }
      }
      else
      {
        for (int i = (int)Control.OSD_VIDEOPROGRESS; i < (int)Control.OSD_VIDEOPROGRESS + 50; ++i)
          HideControl(GetID, i);
        HideControl(GetID, (int)Control.REC_LOGO);
      }


      if (g_Player.Paused)
      {
        ShowControl(GetID, (int)Control.IMG_PAUSE);
      }
      else
      {
        HideControl(GetID, (int)Control.IMG_PAUSE);
      }

      int speed = g_Player.Speed;
      HideControl(GetID, (int)Control.IMG_2X);
      HideControl(GetID, (int)Control.IMG_4X);
      HideControl(GetID, (int)Control.IMG_8X);
      HideControl(GetID, (int)Control.IMG_16X);
      HideControl(GetID, (int)Control.IMG_32X);
      HideControl(GetID, (int)Control.IMG_MIN2X);
      HideControl(GetID, (int)Control.IMG_MIN4X);
      HideControl(GetID, (int)Control.IMG_MIN8X);
      HideControl(GetID, (int)Control.IMG_MIN16X);
      HideControl(GetID, (int)Control.IMG_MIN32X);

      if (speed != 1)
      {
        if (speed == 2)
        {
          ShowControl(GetID, (int)Control.IMG_2X);
        }
        else if (speed == 4)
        {
          ShowControl(GetID, (int)Control.IMG_4X);
        }
        else if (speed == 8)
        {
          ShowControl(GetID, (int)Control.IMG_8X);
        }
        else if (speed == 16)
        {
          ShowControl(GetID, (int)Control.IMG_16X);
        }
        else if (speed == 32)
        {
          ShowControl(GetID, (int)Control.IMG_32X);
        }

        if (speed == -2)
        {
          ShowControl(GetID, (int)Control.IMG_MIN2X);
        }
        else if (speed == -4)
        {
          ShowControl(GetID, (int)Control.IMG_MIN4X);
        }
        else if (speed == -8)
        {
          ShowControl(GetID, (int)Control.IMG_MIN8X);
        }
        else if (speed == -16)
        {
          ShowControl(GetID, (int)Control.IMG_MIN16X);
        }
        else if (speed == -32)
        {
          ShowControl(GetID, (int)Control.IMG_MIN32X);
        }
      }
      HideControl(GetID, (int)Control.LABEL_ROW1);
      HideControl(GetID, (int)Control.LABEL_ROW2);
      HideControl(GetID, (int)Control.LABEL_ROW3);
      HideControl(GetID, (int)Control.BLUE_BAR);
      if (_screenState.SeekStep != 0)
      {
        ShowControl(GetID, (int)Control.BLUE_BAR);
        ShowControl(GetID, (int)Control.LABEL_ROW1);
      }
      if (_statusVisible)
      {
        ShowControl(GetID, (int)Control.BLUE_BAR);
        ShowControl(GetID, (int)Control.LABEL_ROW1);
      }
      if (_groupVisible || _channelInputVisible)
      {
        ShowControl(GetID, (int)Control.BLUE_BAR);
        ShowControl(GetID, (int)Control.LABEL_ROW1);
      }
      HideControl(GetID, (int)Control.MSG_BOX);
      HideControl(GetID, (int)Control.MSG_BOX_LABEL1);
      HideControl(GetID, (int)Control.MSG_BOX_LABEL2);
      HideControl(GetID, (int)Control.MSG_BOX_LABEL3);
      HideControl(GetID, (int)Control.MSG_BOX_LABEL4);

      if (_messageBoxVisible)
      {
        ShowControl(GetID, (int)Control.MSG_BOX);
        ShowControl(GetID, (int)Control.MSG_BOX_LABEL1);
        ShowControl(GetID, (int)Control.MSG_BOX_LABEL2);
        ShowControl(GetID, (int)Control.MSG_BOX_LABEL3);
        ShowControl(GetID, (int)Control.MSG_BOX_LABEL4);
      }

      RenderVolume(_isVolumeVisible);

    }


    void CheckTimeOuts()
    {

      if (_isVolumeVisible)
      {
        TimeSpan ts = DateTime.Now - _volumeTimer;
        if (ts.TotalSeconds >= 3) RenderVolume(false);
      }
      if (_groupVisible)
      {
        TimeSpan ts = (DateTime.Now - _groupTimeOutTimer);
        if (ts.TotalMilliseconds >= _zapTimeOutValue)
        {
          _groupVisible = false;
        }
      }

      if (_statusVisible || _stepSeekVisible)
      {
        TimeSpan ts = (DateTime.Now - _statusTimeOutTimer);
        if (ts.TotalMilliseconds >= 2000)
        {
          _stepSeekVisible = false;
          _statusVisible = false;
        }
      }

      if (_useVMR9Zap == true)
      {
        TimeSpan ts = DateTime.Now - _zapTimeOutTimer;
        if (ts.TotalMilliseconds > _zapTimeOutValue)
        {
          //if(_vmr9OSD!=null)
          //	_vmr9OSD.HideBitmap();
        }
      }
      //if(_vmr9OSD!=null)
      //	_vmr9OSD.CheckTimeOuts();



      // OSD Timeout?
      if (_isOsdVisible && _timeOsdOnscreen > 0)
      {
        TimeSpan ts = DateTime.Now - _osdTimeoutTimer;
        if (ts.TotalMilliseconds > _timeOsdOnscreen)
        {
          //yes, then remove osd offscreen
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0, GetID, 0, null);
          _osdWindow.OnMessage(msg);	// Send a de-init msg to the OSD
          _isOsdVisible = false;
          GUIWindowManager.IsOsdVisible = false;
          msg = null;
        }
      }


      OnKeyTimeout();


      if (_messageBoxVisible && _msgBoxTimeout > 0)
      {
        TimeSpan ts = DateTime.Now - _msgTimer;
        if (ts.TotalSeconds > _msgBoxTimeout)
        {
          _messageBoxVisible = false;
        }
      }


      // Let the navigator zap channel if needed
      TVHome.Navigator.CheckChannelChange();
      //Log.Write("osd visible:{0} timeoutvalue:{1}", _zapOsdVisible ,_zapTimeOutValue);
      if (_zapOsdVisible && _zapTimeOutValue > 0)
      {
        TimeSpan ts = DateTime.Now - _zapTimeOutTimer;
        //Log.Write("timeout :{0}", ts.TotalMilliseconds);
        if (ts.TotalMilliseconds > _zapTimeOutValue)
        {
          //yes, then remove osd offscreen
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _zapWindow.GetID, 0, 0, GetID, 0, null);
          _zapWindow.OnMessage(msg);	// Send a de-init msg to the OSD
          Log.Write("ZAP OSD:Off timeout");
          _zapOsdVisible = false;
          msg = null;
        }
      }
    }

    public override void Render(float timePassed)
    {
      if (GUIWindowManager.IsSwitchingToNewWindow) return;
      if (VMR7Util.g_vmr7 != null)
      {
        if (!GUIWindowManager.IsRouted)
        {
          if (_screenState.ContextMenuVisible ||
            _screenState.MsgBoxVisible ||
            _screenState.MsnVisible ||
            _screenState.OsdVisible ||
            _screenState.Paused ||
            _screenState.ShowGroup ||
            _screenState.ShowInput ||
            _screenState.ShowStatusLine ||
            _screenState.ShowTime ||
            _screenState.ZapOsdVisible ||
            g_Player.Speed != 1 ||
            _needToClearScreen)
          {
            TimeSpan ts = DateTime.Now - _vmr7UpdateTimer;
            if ((ts.TotalMilliseconds >= 5000) || _needToClearScreen)
            {
              _needToClearScreen = false;
              using (Bitmap bmp = new Bitmap(GUIGraphicsContext.Width, GUIGraphicsContext.Height))
              {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                  GUIGraphicsContext.graphics = g;
                  base.Render(timePassed);
                  RenderForm(timePassed);
                  GUIGraphicsContext.graphics = null;
                  _screenState.wasVMRBitmapVisible = true;
                  VMR7Util.g_vmr7.SaveBitmap(bmp, true, true, 0.8f);
                }
              }
              _vmr7UpdateTimer = DateTime.Now;
            }
          }
          else
          {
            if (_screenState.wasVMRBitmapVisible)
            {
              _screenState.wasVMRBitmapVisible = false;
              VMR7Util.g_vmr7.SaveBitmap(null, false, false, 0.8f);
            }
          }
        }
      }

      if (GUIGraphicsContext.Vmr9Active)
      {
        base.Render(timePassed);
      }
      if (_isOsdVisible)
        _osdWindow.Render(timePassed);
      else if (_zapOsdVisible)
        _zapWindow.Render(timePassed);

      if (g_Player.Playing) return;
      if (_isStartingTSForRecording) return;

      //close window
      GUIMessage msg2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _osdWindow.GetID, 0, 0, GetID, 0, null);
      _osdWindow.OnMessage(msg2);	// Send a de-init msg to the OSD
      msg2 = null;
      Log.Write("timeout->OSD:Off");
      _isOsdVisible = false;
      GUIWindowManager.IsOsdVisible = false;

      //close window
      ///@
      /// msg2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, _msnWindow.GetID, 0, 0, GetID, 0, null);
      ///_msnWindow.OnMessage(msg2);	// Send a de-init msg to the OSD
      /// msg2 = null;
      _msnWindowVisible = false;
      GUIWindowManager.IsOsdVisible = false;
      Log.Write("Tvfullscreen:not viewing anymore");
      GUIWindowManager.ShowPreviousWindow();
    }

    public void UpdateOSD()
    {
      if (GUIWindowManager.ActiveWindow != GetID) return;
      Log.Write("UpdateOSD()");
      if (_isOsdVisible)
      {
        _osdWindow.UpdateChannelInfo();
        _osdTimeoutTimer = DateTime.Now;
      }
      else
      {
        Action myaction = new Action();
        //Show ZAP window indefinetely until channel has been tuned
        myaction.fAmount1 = -1;
        myaction.wID = Action.ActionType.ACTION_SHOW_INFO;
        this.OnAction(myaction);
        myaction = null;
      }
    }


    public void RenderForm(float timePassed)
    {
      if (_needToClearScreen)
      {
        _needToClearScreen = false;
        GUIGraphicsContext.graphics.Clear(Color.Black);
      }
      base.Render(timePassed);
      if (GUIGraphicsContext.graphics != null)
      {

        if (_isDialogVisible)
          dlg.Render(timePassed);

        ///if (_msnWindowVisible)
        ///_msnWindow.Render(timePassed);
      }
      // do we need 2 render the OSD?

      if (_isOsdVisible)
        _osdWindow.Render(timePassed);
      else if (_zapOsdVisible)
        _zapWindow.Render(timePassed);
    }

    void HideControl(int idSender, int idControl)
    {
      GUIControl cntl = base.GetControl(idControl);
      if (cntl != null)
      {
        cntl.IsVisible = false;
      }
      cntl = null;
    }
    void ShowControl(int idSender, int idControl)
    {
      GUIControl cntl = base.GetControl(idControl);
      if (cntl != null)
      {
        cntl.IsVisible = true;
      }
      cntl = null;
    }

    void OnKeyTimeout()
    {
      if (_channelName.Length == 0) return;
      TimeSpan ts = DateTime.Now - _keyPressedTimer;
      if (ts.TotalMilliseconds >= 1000)
      {
        // change channel
        int iChannel = Int32.Parse(_channelName);
        ChangeChannelNr(iChannel);
        _channelInputVisible = false;

        _channelName = String.Empty;
      }
    }
    public void OnKeyCode(char chKey)
    {
      if (_isDialogVisible) return;
      if (GUIWindowManager.IsRouted) return;
      if (chKey == 'o')
      {
        Action showInfo = new Action(Action.ActionType.ACTION_SHOW_CURRENT_TV_INFO, 0, 0);
        OnAction(showInfo);
        return;
      }
      if (chKey == '0' && !_channelInputVisible)
      {
        TVHome.OnLastViewedChannel();
        if (!_zapOsdVisible)
        {
          if (!_useVMR9Zap)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, _zapWindow.GetID, 0, 0, GetID, 0, null);
            _zapWindow.OnMessage(msg);
            Log.Write("ZAP OSD:ON");
            _zapTimeOutTimer = DateTime.Now;
            _zapOsdVisible = true;
          }
        }
        else
        {
          _zapWindow.UpdateChannelInfo();
          _zapTimeOutTimer = DateTime.Now;
        }
        return;
      }
      if (chKey >= '0' && chKey <= '9') //Make sure it's only for the remote
      {
        _channelInputVisible = true;
        _keyPressedTimer = DateTime.Now;
        _channelName += chKey;
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.LABEL_ROW1, 0, 0, null);

        string displayedChannelName = string.Empty;
        GroupMap map = (GroupMap)TVHome.Navigator.CurrentGroup.ReferringGroupMap()[Int32.Parse(_channelName) - 1];
        if (_byIndex)
          displayedChannelName = map.ReferencedChannel().Name;
        else
          for (int ChannelCnt = 0; ChannelCnt < TVHome.Navigator.CurrentGroup.ReferringGroupMap().Count; ChannelCnt++)
          {
            map = (GroupMap)TVHome.Navigator.CurrentGroup.ReferringGroupMap()[ChannelCnt];
            if (map.ReferencedChannel().SortOrder == Int32.Parse(_channelName))
            {
              displayedChannelName = map.ReferencedChannel().Name;
              break;
            }
          }
        if (displayedChannelName != string.Empty)
          msg.Label = String.Format("{0} {1} ({2})", GUILocalizeStrings.Get(602), _channelName, displayedChannelName);  // Channel
        else
          msg.Label = String.Format("{0} {1}", GUILocalizeStrings.Get(602), _channelName);  // Channel

        GUIControl cntTarget = base.GetControl((int)Control.LABEL_ROW1);
        if (cntTarget != null)
        {
          cntTarget.OnMessage(msg);
        }
        cntTarget = null;

        if (_channelName.Length == 3)
        {
          // Change channel immediately
          int iChannel = Int32.Parse(_channelName);
          ChangeChannelNr(iChannel);
          _channelInputVisible = false;
          _channelName = "";
        }
      }
    }

    /*
      List<TVChannel> channels = CurrentGroup.TvChannels;
      channelNr--;
      if (channelNr >= 0 && channelNr < channels.Count)
      {
        TVChannel chan = (TVChannel)channels[channelNr];
        ZapToChannel(chan.Name, useZapDelay);
      }
     */


    /*
          List<TVChannel> channels = CurrentGroup.TvChannels;
      if (channelNr >= 0)
      {
        bool found = false;
        int ChannelCnt = 0;
        TVChannel chan;
        while (found == false && ChannelCnt < channels.Count)
        {
          chan = (TVChannel)channels[ChannelCnt];
          if (chan.Number == channelNr)
          {
            ZapToChannel(chan.Name, useZapDelay);
            found = true;
          }
          else
            ChannelCnt++;
        }
      }
*/
    private void OnPageDown()
    {
      // Switch to the next channel group and tune to the first channel in the group
      TVHome.Navigator.ZapToPreviousGroup(true);
      _groupVisible = true;
      _groupTimeOutTimer = DateTime.Now;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.LABEL_ROW1, 0, 0, null);
      msg.Label = String.Format("{0}:{1}", GUILocalizeStrings.Get(971), TVHome.Navigator.ZapGroupName);
      OnMessage(msg);
    }

    private void OnPageUp()
    {
      // Switch to the next channel group and tune to the first channel in the group
      TVHome.Navigator.ZapToNextGroup(true);
      _groupVisible = true;
      _groupTimeOutTimer = DateTime.Now;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, (int)Control.LABEL_ROW1, 0, 0, null);
      msg.Label = String.Format("{0}:{1}", GUILocalizeStrings.Get(971), TVHome.Navigator.ZapGroupName);
      OnMessage(msg);
    }

    void ChangeChannelNr(int channelNr)
    {

      Log.Write("ChangeChannelNr()");
      if (_byIndex == true)
      {
        TVHome.Navigator.ZapToChannel(channelNr, false);
      }
      else
      {
        TVHome.Navigator.ZapToChannelNumber(channelNr, false);
      }

      UpdateOSD();
      _zapTimeOutTimer = DateTime.Now;

    }

    public void ZapPreviousChannel()
    {
      Log.Write("ZapPreviousChannel()");
      TVHome.Navigator.ZapToPreviousChannel(true);
      _zapTimeOutTimer = DateTime.Now;
      UpdateOSD();
      ///@
      ///if (_useVMR9Zap == true && _vmr9OSD != null)
      {
        //_vmr9OSD.RenderChannelList(TVHome.Navigator.CurrentGroup,TVHome.Navigator.ZapChannel);
      }
    }

    public void ZapNextChannel()
    {
      Log.Write("ZapNextChannel()");
      TVHome.Navigator.ZapToNextChannel(true);
      _zapTimeOutTimer = DateTime.Now;
      UpdateOSD();
      ///@
      ///if (_useVMR9Zap == true && _vmr9OSD != null)
      {
        //_vmr9OSD.RenderChannelList(TVHome.Navigator.CurrentGroup,TVHome.Navigator.ZapChannel);
      }

    }


    public override int GetFocusControlId()
    {
      if (_isOsdVisible)
      {
        return _osdWindow.GetFocusControlId();
      }
      if (_msnWindowVisible)
      {
        ///@
        ///return _msnWindow.GetFocusControlId();
      }

      return base.GetFocusControlId();
    }

    public override GUIControl GetControl(int iControlId)
    {
      if (_isOsdVisible)
      {
        return _osdWindow.GetControl(iControlId);
      }
      if (_msnWindowVisible)
      {
        ///@
        ///return _msnWindow.GetControl(iControlId);
      }

      return base.GetControl(iControlId);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      ///@
      /*
      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        if (Recorder.IsViewing() && !(Recorder.IsTimeShifting() || Recorder.IsRecording()))
        {
          if (GUIGraphicsContext.ShowBackground)
          {
            // stop timeshifting & viewing... 

            Recorder.StopViewing();
          }
        }
      }*/
      SaveSettings();
      base.OnPageDestroy(newWindowId);
    }
    void RenderVolume(bool show)
    {
      if (imgVolumeBar == null) return;

      if (!show)
      {
        _isVolumeVisible = false;
        imgVolumeBar.Visible = false;
        imgVolumeMuteIcon.Visible = false;
        return;
      }
      else
      {
        imgVolumeBar.Visible = true;
        if (VolumeHandler.Instance.IsMuted)
        {
          imgVolumeMuteIcon.Visible = true;
          imgVolumeBar.Image1 = 1;
          imgVolumeBar.Current = 0;
        }
        else
        {
          imgVolumeBar.Current = VolumeHandler.Instance.Step;
          imgVolumeBar.Maximum = VolumeHandler.Instance.StepMax;
          imgVolumeMuteIcon.Visible = false;
          imgVolumeBar.Image1 = 2;
          imgVolumeBar.Image2 = 1;
        }

      }
    }

    #region IRenderLayer
    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }
    #endregion
  }
}
