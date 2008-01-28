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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;

using TvDatabase;
using TvControl;

using Gentle.Common;
using Gentle.Framework;

namespace TvPlugin
{
  /// <summary>
  /// GUIMiniGuide
  /// </summary>
  /// 
  public class TvMiniGuide : GUIWindow, IRenderLayer
  {
    // Member variables                                  
    [SkinControlAttribute(34)]    protected GUIButtonControl cmdExit = null;
    [SkinControlAttribute(35)]    protected GUIListControl lstChannels = null;
    [SkinControlAttribute(36)]    protected GUISpinControl spinGroup = null;

    bool _canceled = false;
    bool _running = false;    
    int _parentWindowID = 0;
    GUIWindow _parentWindow = null;
    List<Channel> _tvChannelList = null;
    List<ChannelGroup> _channelGroupList = null;
    Channel _selectedChannel;
    bool _zap = true;
    Stopwatch benchClock = null;
    private List<Channel> _channelList = new List<Channel>();

    bool _byIndex = false;
    bool _showChannelNumber = false;
    int _channelNumberMaxLength = 3;    

    #region Serialisation
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _byIndex = xmlreader.GetValueAsBool("mytv", "byindex", true);
        _showChannelNumber = xmlreader.GetValueAsBool("mytv", "showchannelnumber", false);
        _channelNumberMaxLength = xmlreader.GetValueAsInt("mytv", "channelnumbermaxlength", 3);
      }
    }
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public TvMiniGuide()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MINI_GUIDE;
    }

    public override void OnAdded()
    {
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_MINI_GUIDE, this);
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

    /// <summary>
    /// Gets a value indicating whether this instance is tv.
    /// </summary>
    /// <value><c>true</c> if this instance is tv; otherwise, <c>false</c>.</value>
    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the dialog was canceled. 
    /// </summary>
    /// <value><c>true</c> if dialog was canceled without a selection</value>
    public  bool Canceled
    {
        get
        {
            return _canceled;
        }
    }

    /// <summary>
    /// Gets or sets the selected channel.
    /// </summary>
    /// <value>The selected channel.</value>
    public Channel SelectedChannel
    {
      get
      {
        return _selectedChannel;
      }
      set
      {
        _selectedChannel = value;
      }
    }
	
    /// <summary>
    /// Gets or sets a value indicating whether [auto zap].
    /// </summary>
    /// <value><c>true</c> if [auto zap]; otherwise, <c>false</c>.</value>
    public bool AutoZap
    {
      get
      {
        return _zap;
      }
      set
      {
        _zap = value;
      }
    }

    /// <summary>
    /// Init method
    /// </summary>
    /// <returns></returns>
    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\TVMiniGuide.xml");

      GetID = (int)GUIWindow.Window.WINDOW_MINI_GUIDE;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MiniEPG);
      _canceled = true;
      LoadSettings();
      return bResult;
    }


    /// <summary>
    /// Renderer
    /// </summary>
    /// <param name="timePassed"></param>
    public override void Render(float timePassed)
    {
      base.Render(timePassed);		// render our controls to the screen
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
    /// On close
    /// </summary>
    void Close()
    {
      Log.Debug("miniguide: close()");
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
        OnMessage(msg);

        GUIWindowManager.UnRoute();        
        _running = false;
        _parentWindow = null;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
      GUILayerManager.UnRegisterLayer(this);

      Log.Debug("miniguide: closed");
    }

    /// <summary>
    /// On Message
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            if (message.SenderControlId == 35) // listbox
            {
              if ((int)Action.ActionType.ACTION_SELECT_ITEM == message.Param1)
              {
                // switching logic
                SelectedChannel = (Channel)lstChannels.SelectedListItem.MusicTag;
                
                Channel changeChannel = null;
                if (AutoZap)
                {
                  string selectedChan = (string)lstChannels.SelectedListItem.TVTag;
                  if ((TVHome.Navigator.CurrentChannel != selectedChan) || g_Player.IsTVRecording)
                  {                    
                    changeChannel = (Channel)_tvChannelList[lstChannels.SelectedListItemIndex];                    
                  }
                }
                _canceled = false;
                Close();
                
                //This one shows the zapOSD when changing channel from mini GUIDE, this is currently unwanted.
                /*
                TvFullScreen TVWindow = (TvFullScreen)GUIWindowManager.GetWindow((int)(int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
                if (TVWindow != null) TVWindow.UpdateOSD(changeChannel.Name);                
                */

                TVHome.UserChannelChanged = true;

                if (changeChannel != null)
                {                  
                  TVHome.ViewChannel(changeChannel);                  
                }
              }
            }
            else if (message.SenderControlId == 36) // spincontrol
            {
              // switch group              
              OnGroupChanged();
            }
            else if (message.SenderControlId == 34) // exit button
            {
              // exit
              Close();
              _canceled = true;
            }
            break;
          }
      }
      return base.OnMessage(message);
    }

    /// <summary>
    /// On action
    /// </summary>
    /// <param name="action"></param>
    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_CONTEXT_MENU:
          //_running = false;
          Close();
          return;
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          //_running = false;
          _canceled = true;
          Close();
          return;
        case Action.ActionType.ACTION_MOVE_LEFT:
          // switch group
          spinGroup.MoveUp();
          return;
        case Action.ActionType.ACTION_MOVE_RIGHT:
          // switch group
          spinGroup.MoveDown();
          return;
      }
      base.OnAction(action);
    }

    /// <summary>
    /// Page gets destroyed
    /// </summary>
    /// <param name="new_windowId"></param>
    protected override void OnPageDestroy(int new_windowId)
    {
      Log.Debug("miniguide: OnPageDestroy");
      base.OnPageDestroy(new_windowId);
      _running = false;
    }

    /// <summary>
    /// Page gets loaded
    /// </summary>
    protected override void OnPageLoad()
    {      
      benchClock = Stopwatch.StartNew();      
      Log.Debug("miniguide: onpageload");
      // following line should stay. Problems with OSD not
      // appearing are already fixed elsewhere
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.MiniEPG);
      AllocResources();
      ResetAllControls();							// make sure the controls are positioned relevant to the OSD Y offset
      benchClock.Stop();
      Log.Debug("miniguide: all controls are reset after {0}ms", benchClock.ElapsedMilliseconds.ToString());
      FillChannelList();
      FillGroupList();
      base.OnPageLoad();      
    }

    private void OnGroupChanged()
    {
      GUIWaitCursor.Show();
      TVHome.Navigator.SetCurrentGroup(spinGroup.Value);
      lstChannels.Clear();
      FillChannelList();
      GUIWaitCursor.Hide();
    }

    /// <summary>
    /// Fill up the list with groups
    /// </summary>
    public void FillGroupList()
    {
      benchClock.Reset();
      benchClock.Start();
      ChannelGroup current = null;
      _channelGroupList = TVHome.Navigator.Groups;
      // empty list of channels currently in the 
      // spin control
      spinGroup.Reset();
      // start to fill them up again
      for (int i = 0; i < _channelGroupList.Count; i++)
      {
        current = _channelGroupList[i];
        spinGroup.AddLabel(current.GroupName, i);
        // set selected
        if (current.GroupName.CompareTo(TVHome.Navigator.CurrentGroup.GroupName) == 0)
        {
          spinGroup.Value = i;          
        }
      }
      benchClock.Stop();
      Log.Debug("miniguide: FillGroupList finished after {0}ms", benchClock.ElapsedMilliseconds.ToString());
    }

    /// <summary>
    /// Fill the list with channels
    /// </summary>
    public void FillChannelList()
    {
      lstChannels.Visible = false;
      benchClock.Reset();
      benchClock.Start();
      ///_tvChannelList = (List<Channel>)TVHome.Navigator.CurrentGroup.ReferringTvGuideChannels();      
      TvBusinessLayer layer = new TvBusinessLayer();
      _tvChannelList = layer.GetTVGuideChannelsForGroup(TVHome.Navigator.CurrentGroup.IdGroup);
      benchClock.Stop();
      string BenchGroupChannels = benchClock.ElapsedMilliseconds.ToString();      
      benchClock.Reset();
      benchClock.Start();
      Dictionary<int, NowAndNext> listNowNext = layer.GetNowAndNext();
      benchClock.Stop();
      string BenchNowNext = benchClock.ElapsedMilliseconds.ToString();
      Channel CurrentChan = null;
      GUIListItem item = null;
      string ChannelLogo = "";
      List<int> RecChannels = null;
      List<int> TSChannels = null;
      int SelectedID = 0;
      int CurrentChanState = 0;
      bool CheckChannelState = true;
      bool DisplayStatusInfo = true;
      string PathIconNoTune = GUIGraphicsContext.Skin + @"\Media\remote_blue.png";
      string PathIconTimeshift = GUIGraphicsContext.Skin + @"\Media\remote_yellow.png";
      string PathIconRecord = GUIGraphicsContext.Skin + @"\Media\remote_red.png";      
      
      if (!CheckChannelState)
        Log.Debug("miniguide: not checking channel state");
      else
      {
        benchClock.Reset();
        benchClock.Start();
        TVHome.TvServer.GetAllRecordingChannels(out RecChannels, out TSChannels);
        benchClock.Stop();
        Log.Debug("miniguide: FillChannelList - currently ts: {0}, rec: {1} / GetChans: {2}ms, NowNextSQL: {3}ms, GetAllRecs: {4}ms", Convert.ToString(TSChannels.Count), Convert.ToString(RecChannels.Count), BenchGroupChannels, BenchNowNext, benchClock.ElapsedMilliseconds.ToString());
      }

      if (RecChannels.Count == 0)
      {
        // not using cards at all - assume tuneability (why else should the user have this channel added..)
        if (TSChannels.Count == 0)
          CheckChannelState = false;
        else
        {
          // note: it could be possible we're watching a stream another user is timeshifting...
          // TODO: add user check
          if (TSChannels.Count == 1 && g_Player.IsTV && g_Player.Playing)
          {
            CheckChannelState = false;
            Log.Debug("miniguide: assume we're the only current timeshifting user - switching to fast channel check mode");
          }
        }
      }

      benchClock.Reset();
      benchClock.Start();
      for (int i = 0; i < _tvChannelList.Count; i++)
      {
        CurrentChan = _tvChannelList[i];
        if (CheckChannelState)
          CurrentChanState = (int)TVHome.TvServer.GetChannelState(CurrentChan.IdChannel, TVHome.Card.User);
        else
          CurrentChanState = (int)ChannelState.tunable;

        if (CurrentChan.VisibleInGuide)
        {
          NowAndNext prog;
          if (listNowNext.ContainsKey(CurrentChan.IdChannel) != false)
            prog = listNowNext[CurrentChan.IdChannel];
          else
            prog = new NowAndNext(CurrentChan.IdChannel, DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1), DateTime.Now.AddHours(2), DateTime.Now.AddHours(3), GUILocalizeStrings.Get(736), GUILocalizeStrings.Get(736), -1, -1);

          StringBuilder sb = new StringBuilder();
          item = new GUIListItem("");
          // store here as it is not needed right now - please beat me later..
          item.TVTag = CurrentChan.DisplayName;
          item.MusicTag = CurrentChan;

          sb.Append(CurrentChan.DisplayName);
          ChannelLogo = MediaPortal.Util.Utils.GetCoverArt(Thumbs.TVChannel, CurrentChan.DisplayName);

          // if we are watching this channel mark it
          if (TVHome.Navigator.Channel.IdChannel == CurrentChan.IdChannel)
          {
            item.IsRemote = true;
            SelectedID = lstChannels.Count;
          }

          if (System.IO.File.Exists(ChannelLogo))
          {
            item.IconImageBig = ChannelLogo;
            item.IconImage = ChannelLogo;
          }
          else
          {
            item.IconImageBig = string.Empty;
            item.IconImage = string.Empty;
          }

          if (DisplayStatusInfo)
          {
            if (RecChannels.Contains(CurrentChan.IdChannel))
              CurrentChanState = (int)ChannelState.recording;
            else
              if (TSChannels.Contains(CurrentChan.IdChannel))
                CurrentChanState = (int)ChannelState.timeshifting;

            // Log.Debug("miniguide: state of {0} is {1}", CurrentChan.Name, Convert.ToString(CurrentChanState));
            switch (CurrentChanState)
            {
              case 0:
                //item.IconImageBig = PathIconNoTune;
                //item.IconImage = PathIconNoTune;
                sb.Append(" ");
                sb.Append(GUILocalizeStrings.Get(1056));
                item.IsPlayed = true;
                break;
              case 2:
                //item.IconImageBig = PathIconTimeshift;
                //item.IconImage = PathIconTimeshift;
                sb.Append(" ");
                sb.Append(GUILocalizeStrings.Get(1055));
                break;
              case 3:
                //item.IconImageBig = PathIconRecord;
                //item.IconImage = PathIconRecord;
                sb.Append(" ");
                sb.Append(GUILocalizeStrings.Get(1054));
                break;
              default:
                item.IsPlayed = false;
                break;
            }
          }

          item.Label2 = prog.TitleNow;
          //                    item.Label3 = prog.Title + " [" + prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat) + "-" + prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat) + "]";

          item.Label3 = GUILocalizeStrings.Get(789) + prog.TitleNow;
          
          sb.Append(" - ");
          if (_showChannelNumber == true)
          {
            foreach (TuningDetail detail in _tvChannelList[i].ReferringTuningDetail())
              sb.Append(detail.ChannelNumber + " - ");
          }
          sb.Append(CalculateProgress(prog.NowStartTime, prog.NowEndTime).ToString());
          sb.Append("%");
          item.Label2 = sb.ToString();
          item.Label = GUILocalizeStrings.Get(790) + prog.TitleNext;

          lstChannels.Add(item);
        }
      }
      benchClock.Stop();
      Log.Debug("miniguide: state check + filling completed after {0}ms", benchClock.ElapsedMilliseconds.ToString());
      lstChannels.SelectedListItemIndex = SelectedID;
      lstChannels.Visible = true;
    }

    /// <summary>
    /// Get current tv program
    /// </summary>
    /// <param name="prog"></param>
    /// <returns></returns>
    private double CalculateProgress(DateTime start, DateTime end)
    {
      TimeSpan length = end - start;
      TimeSpan passed = DateTime.Now - start;
      if (length.TotalMinutes > 0)
      {
        double fprogress = (passed.TotalMinutes / length.TotalMinutes) * 100;
        fprogress = Math.Floor(fprogress);
        if (fprogress > 100.0f)
          return 100.0f;
        return fprogress;
      }
      else
        return 0;
    }

    /// <summary>
    /// Do this modal
    /// </summary>
    /// <param name="dwParentId"></param>
    public void DoModal(int dwParentId)
    {
      Log.Debug("miniguide: domodal");
      _parentWindowID = dwParentId;
      _parentWindow = GUIWindowManager.GetWindow(_parentWindowID);
      if (null == _parentWindow)
      {
        Log.Debug("miniguide: parentwindow=0");
        _parentWindowID = 0;
        return;
      }

      GUIWindowManager.IsSwitchingToNewWindow = true;
      GUIWindowManager.RouteToWindow(GetID);

      // activate this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, -1, 0, null);
      OnMessage(msg);

      GUIWindowManager.IsSwitchingToNewWindow = false;
      _running = true;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
      while (_running && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
        if (!GUIGraphicsContext.Vmr9Active)
          System.Threading.Thread.Sleep(50);
      }

      Close();      
    }

    // Overlay IRenderLayer members
    #region IRenderLayer
    public bool ShouldRenderLayer()
    {
      //TVHome.SendHeartBeat(); //not needed, now sent from tvoverlay.cs
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      if (_running)
        Render(timePassed);
    }
    #endregion
  }
}
