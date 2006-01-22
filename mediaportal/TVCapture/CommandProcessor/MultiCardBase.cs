/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#region usings
using System;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Management;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using MediaPortal.Radio.Database;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using MediaPortal.TV.Teletext;
using MediaPortal.TV.DiskSpace;
#endregion

namespace MediaPortal.TV.Recording
{
  public class MultiCardBase
  {
    public delegate void OnTvViewHandler(int card, TVCaptureDevice device);
    public delegate void OnTvChannelChangeHandler(string tvChannelName);

    //event which gets called when the tv channel changes (due to zapping for example)
    public event OnTvChannelChangeHandler OnTvChannelChanged = null;

    //event which happens when TV viewing is started
    public event OnTvViewHandler OnTvViewingStarted = null;

    //event which happens when TV viewing is stopped
    public event OnTvViewHandler OnTvViewingStopped = null;



    protected int _currentCardIndex;
    string _currentTvChannel;
    protected DateTime _killTimeshiftingTimer;

    static List<TVChannel> _tvChannelsList = new List<TVChannel>();

    public MultiCardBase()
    {
      _killTimeshiftingTimer = DateTime.Now;
      TVDatabase.GetChannels(ref _tvChannelsList);
    }

    public string TVChannelName
    {
      get { return _currentTvChannel; }
      set {
        if (_currentTvChannel != value)
        {
          _currentTvChannel = value;
          if (OnTvChannelChanged!=null)
            OnTvChannelChanged(_currentTvChannel);
        }
        else
        {
          _currentTvChannel = value;
        }
      }
    }
    
    public int CurrentCardIndex
    {
      get { return _currentCardIndex; }
      set { _currentCardIndex=value; }
    }
    public void ResetTimeshiftTimer()
    {
      _killTimeshiftingTimer = DateTime.Now;
    }


    public void TuneExternalChannel(string channelName, bool isViewing)
    {
      foreach (TVChannel chan in _tvChannelsList)
      {
        if (String.Compare(chan.Name,channelName,true)==0)
        {
          if (chan.External)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL, 0, 0, 0, 0, 0, null);
            msg.Label = chan.ExternalTunerChannel;
            GUIWindowManager.SendThreadMessage(msg);
          }
          break;
        }
      }
    }

    public void OnTvStopped(int card,TVCaptureDevice device)
    {
      if (OnTvViewingStopped!=null)
        OnTvViewingStopped(card, device);
    }
    
    public void OnTvStart(int card,TVCaptureDevice device)
    {
      if (OnTvViewingStarted != null)
        OnTvViewingStarted(card, device);
    }
  }
}
