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
using System.Threading;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Implementations.DVB;
using TvLibrary.Log;
using TvControl;
using TvDatabase;

namespace TvService
{
  public class TimeShifter
  {
    private readonly ITvCardHandler _cardHandler;
    private readonly bool _linkageScannerEnabled;
    private readonly bool _timeshiftingEpgGrabberEnabled;
    private readonly int _waitForTimeshifting = 15;
    private bool _tuneInProgress = false;

    private ManualResetEvent _eventAudio = new ManualResetEvent(false); // gets signaled when audio PID is seen
    private ManualResetEvent _eventVideo = new ManualResetEvent(false); // gets signaled when video PID is seen
    private ITvSubChannel _subchannel; // the active sub channel to record        

    private readonly ChannelLinkageGrabber _linkageGrabber;

    private DateTime _timeAudioEvent;
    private DateTime _timeVideoEvent;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeShifter"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public TimeShifter(ITvCardHandler cardHandler)
    {
      _eventAudio.Reset();
      _eventVideo.Reset();

      _cardHandler = cardHandler;
      TvBusinessLayer layer = new TvBusinessLayer();
      _linkageScannerEnabled = (layer.GetSetting("linkageScannerEnabled", "no").Value == "yes");

      _linkageGrabber = new ChannelLinkageGrabber(cardHandler.Card);
      _timeshiftingEpgGrabberEnabled = (layer.GetSetting("timeshiftingEpgGrabberEnabled", "no").Value == "yes");

      _waitForTimeshifting = Int32.Parse(layer.GetSetting("timeshiftWaitForTimeshifting", "15").Value);

      _timeAudioEvent = DateTime.MinValue;
      _timeVideoEvent = DateTime.MinValue;
    }




    /// <summary>
    /// Gets the name of the time shift file.
    /// </summary>
    /// <value>The name of the time shift file.</value>
    public string FileName(ref IUser user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return "";

        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return "";
          if (_cardHandler.IsLocal == false)
          {
            return RemoteControl.Instance.TimeShiftFileName(ref user);
          }
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return "";
        }
        ITvCardContext context = _cardHandler.Card.Context as ITvCardContext;
        if (context == null)
          return null;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
        if (subchannel == null)
          return null;
        return subchannel.TimeShiftFileName + ".tsbuffer";
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return "";
      }
    }

    /// <summary>
    /// Returns the position in the current timeshift file and the id of the current timeshift file
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="position">The position in the current timeshift buffer file</param>
    /// <param name="bufferId">The id of the current timeshift buffer file</param>
    public bool GetCurrentFilePosition(ref IUser user, ref Int64 position, ref long bufferId)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return false;

        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return false;
          if (_cardHandler.IsLocal == false)
          {
            return RemoteControl.Instance.TimeShiftGetCurrentFilePosition(ref user, ref position,
                                                                          ref bufferId);
          }
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return false;
        }
        ITvCardContext context = _cardHandler.Card.Context as ITvCardContext;
        if (context == null)
          return false;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
        if (subchannel == null)
          return false;
        subchannel.TimeShiftGetCurrentFilePosition(ref position, ref bufferId);
        return (position != -1);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this card is recording.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this card is recording; otherwise, <c>false</c>.
    /// </value>
    public bool IsAnySubChannelTimeshifting
    {
      get
      {
        IUser[] users = _cardHandler.Users.GetUsers();
        if (users == null)
          return false;
        if (users.Length == 0)
          return false;
        for (int i = 0; i < users.Length; ++i)
        {
          IUser user = users[i];
          if (IsTimeShifting(ref user))
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Returns if the card is timeshifting or not
    /// </summary>
    /// <returns>true when card is timeshifting otherwise false</returns>
    public bool IsTimeShifting(ref IUser user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return false;

        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return false;
          if (_cardHandler.IsLocal == false)
          {
            return RemoteControl.Instance.IsTimeShifting(ref user);
          }
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return false;
        }
        ITvCardContext context = _cardHandler.Card.Context as ITvCardContext;
        if (context == null)
          return false;
        bool userExists;
        context.GetUser(ref user, out userExists);
        if (!userExists)
          return false;
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
        if (subchannel == null)
          return false;
        return subchannel.IsTimeShifting;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return false;
      }
    }


    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime TimeShiftStarted(IUser user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return DateTime.MinValue;

        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return DateTime.MinValue;
          if (_cardHandler.IsLocal == false)
          {
            return RemoteControl.Instance.TimeShiftStarted(user);
          }
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return DateTime.MinValue;
        }

        ITvCardContext context = _cardHandler.Card.Context as ITvCardContext;
        if (context == null)
          return DateTime.MinValue;
        context.GetUser(ref user);
        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
        if (subchannel == null)
          return DateTime.MinValue;
        return subchannel.StartOfTimeShift;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return DateTime.MinValue;
      }
    }

    private void AudioVideoEventHandler(PidType pidType)
    {
      if (_tuneInProgress)
      {
        Log.Info("audioVideoEventHandler - tune in progress");
        return;
      }

      // we are only interested in video and audio PIDs
      if (pidType == PidType.Audio)
      {
        TimeSpan ts = DateTime.Now - _timeAudioEvent;
        if (ts.TotalMilliseconds > 1000)
        {
          // Avoid repetitive events that are kept for next channel change, so trig only once.
          Log.Info("audioVideoEventHandler {0}", pidType);
          _eventAudio.Set();
        }
        else
        {
          Log.Info("audio last seen at {0}", _timeAudioEvent);
        }
        _timeAudioEvent = DateTime.Now;
      }

      if (pidType == PidType.Video)
      {
        TimeSpan ts = DateTime.Now - _timeVideoEvent;
        if (ts.TotalMilliseconds > 1000)
        {
          // Avoid repetitive events that are kept for next channel change, so trig only once.
          Log.Info("audioVideoEventHandler {0}", pidType);
          _eventVideo.Set();
        }
        else
        {
          Log.Info("video last seen at {0}", _timeVideoEvent);
        }
        _timeVideoEvent = DateTime.Now;
      }
    }

    /// <summary>
    /// Start timeshifting.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="fileName">Name of the timeshiftfile.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult Start(ref IUser user, ref string fileName)
    {
      try
      {
        // Is the card enabled ?
        if (_cardHandler.DataBaseCard.Enabled == false)
        {
          return TvResult.CardIsDisabled;
        }

        lock (this)
        {
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
              return TvResult.CardIsDisabled;


            // Let's verify if hard disk drive has enough free space before we start time shifting. The function automatically handles both local and UNC paths
            if (!IsTimeShifting(ref user))
            {
              ulong FreeDiskSpace = Utils.GetFreeDiskSpace(fileName);

              TvBusinessLayer layer = new TvBusinessLayer();
              UInt32 MaximumFileSize = UInt32.Parse(layer.GetSetting("timeshiftMaxFileSize", "256").Value);// in MB
              ulong DiskSpaceNeeded = Convert.ToUInt64(MaximumFileSize);
              DiskSpaceNeeded *= 1000000 * 2; // Convert to bytes; 2 times of timeshiftMaxFileSize
              if (FreeDiskSpace < DiskSpaceNeeded)
              // TimeShifter need at least this free disk space otherwise, it will not start.
              {
                return TvResult.NoFreeDiskSpace;
              }
            }

            Log.Write("card: StartTimeShifting {0} {1} ", _cardHandler.DataBaseCard.IdCard, fileName);

            if (_cardHandler.IsLocal == false)
            {
              return RemoteControl.Instance.StartTimeShifting(ref user, ref fileName);
            }
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}",
                      _cardHandler.DataBaseCard.ReferencedServer().HostName);
            return TvResult.UnknownError;
          }

          ITvCardContext context = _cardHandler.Card.Context as ITvCardContext;
          if (context == null)
            return TvResult.UnknownChannel;
          context.GetUser(ref user);
          ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);

          if (subchannel == null)
            return TvResult.UnknownChannel;

          _subchannel = subchannel;

          Log.Write("card: CAM enabled : {0}", _cardHandler.HasCA);

          if (subchannel is TvDvbChannel)
          {
            if (!((TvDvbChannel)subchannel).PMTreceived)
            {
              Log.Info("start subch:{0} No PMT received. Timeshifting failed", subchannel.SubChannelId);
              Stop(ref user);
              return TvResult.UnableToStartGraph;
            }
          }

          if (subchannel is BaseSubChannel)
          {
            ((BaseSubChannel)subchannel).AudioVideoEvent += AudioVideoEventHandler;
          }

          bool isScrambled;
          if (subchannel.IsTimeShifting)
          {
            if (!WaitForTimeShiftFile(ref user, out isScrambled))
            {
              Stop(ref user);
              if (isScrambled)
              {
                return TvResult.ChannelIsScrambled;
              }
              return TvResult.NoVideoAudioDetected;
            }

            context.OnZap(user);
            if (_linkageScannerEnabled)
              _cardHandler.Card.StartLinkageScanner(_linkageGrabber);
            if (_timeshiftingEpgGrabberEnabled)
            {
              Channel channel = Channel.Retrieve(user.IdChannel);
              if (channel.GrabEpg)
                _cardHandler.Card.GrabEpg();
              else
                Log.Info("TimeshiftingEPG: channel {0} is not configured for grabbing epg",
                         channel.DisplayName);
            }
            return TvResult.Succeeded;
          }

          bool result = subchannel.StartTimeShifting(fileName);
          if (result == false)
          {
            Stop(ref user);
            return TvResult.UnableToStartGraph;
          }

          fileName += ".tsbuffer";
          if (!WaitForTimeShiftFile(ref user, out isScrambled))
          {
            Stop(ref user);
            if (isScrambled)
            {
              return TvResult.ChannelIsScrambled;
            }
            return TvResult.NoVideoAudioDetected;
          }
          context.OnZap(user);
          if (_linkageScannerEnabled)
            _cardHandler.Card.StartLinkageScanner(_linkageGrabber);
          if (_timeshiftingEpgGrabberEnabled)
          {
            Channel channel = Channel.Retrieve(user.IdChannel);
            if (channel.GrabEpg)
              _cardHandler.Card.GrabEpg();
            else
              Log.Info("TimeshiftingEPG: channel {0} is not configured for grabbing epg",
                       channel.DisplayName);
          }
          return TvResult.Succeeded;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }

      Stop(ref user);
      return TvResult.UnknownError;
    }

    /// <summary>
    /// Stops the time shifting.
    /// </summary>
    /// <returns></returns>
    public bool Stop(ref IUser user)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return true;

        ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);
        if (subchannel is BaseSubChannel)
        {
          ((BaseSubChannel)subchannel).AudioVideoEvent -= AudioVideoEventHandler;
        }

        Log.Write("card {2}: StopTimeShifting user:{0} sub:{1}", user.Name, user.SubChannel,
                  _cardHandler.Card.Name);

        lock (this)
        {
          try
          {
            RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
            if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
              return true;

            Log.Write("card: StopTimeShifting user:{0} sub:{1}", user.Name, user.SubChannel);

            if (_cardHandler.IsLocal == false)
            {
              return RemoteControl.Instance.StopTimeShifting(ref user);
            }
          }
          catch (Exception)
          {
            Log.Error("card: unable to connect to slave controller at:{0}",
                      _cardHandler.DataBaseCard.ReferencedServer().HostName);
            return false;
          }

          ITvCardContext context = _cardHandler.Card.Context as ITvCardContext;
          if (context == null)
            return true;
          if (_linkageScannerEnabled)
            _cardHandler.Card.ResetLinkageScanner();

          if (_cardHandler.IsIdle)
          {
            _cardHandler.PauseCard(user);
          }
          else
          {
            Log.Debug("card not IDLE - removing user: {0}", user.Name);
            _cardHandler.Users.RemoveUser(user);
          }

          context.Remove(user);
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return false;
    }

    /// <summary>
    /// Start timeshifting on the card
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>TvResult indicating whether method succeeded</returns>
    public TvResult CardTimeShift(ref IUser user, ref string fileName)
    {
      try
      {
        if (_cardHandler.DataBaseCard.Enabled == false)
          return TvResult.CardIsDisabled;

        try
        {
          RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
          if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
            return TvResult.CardIsDisabled;
        }
        catch (Exception)
        {
          Log.Error("card: unable to connect to slave controller at:{0}",
                    _cardHandler.DataBaseCard.ReferencedServer().HostName);
          return TvResult.UnknownError;
        }

        Log.WriteFile("card: CardTimeShift {0} {1}", _cardHandler.DataBaseCard.IdCard, fileName);
        if (IsTimeShifting(ref user))
          return TvResult.Succeeded;
        return Start(ref user, ref fileName);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        return TvResult.UnknownError;
      }
    }

    /// <summary>
    /// Waits for time shift file to be at leat 300kb.
    /// </summary>
    /// <param name="user">User</param>
    /// <param name="scrambled">Indicates if the cahnnel is scambled</param>
    /// <returns>true when timeshift files is at least of 300kb, else timeshift file is less then 300kb</returns>
    public bool WaitForTimeShiftFile(ref IUser user, out bool scrambled)
    {
      scrambled = false;
      if (_cardHandler.DataBaseCard.Enabled == false)
        return false;
      try
      {
        RemoteControl.HostName = _cardHandler.DataBaseCard.ReferencedServer().HostName;
        if (!RemoteControl.Instance.CardPresent(_cardHandler.DataBaseCard.IdCard))
          return false;
      }
      catch (Exception)
      {
        Log.Error("card: unable to connect to slave controller at:{0}",
                  _cardHandler.DataBaseCard.ReferencedServer().HostName);
        return false;
      }

      // no need to wait for unscrambled signal, since the wait audio video pids are internally raised in tswriter when the packets are unscrambled.
      // instead we should query tswriter in the event that no audio/video events was received, wether or not the current state of the stream is scrambled or not.
      /*
if (!WaitForUnScrambledSignal(ref user))
{
  scrambled = true;
  return false;
}
*/

      //lets check if stream is initially scrambled, if it is and the card has no CA, then we are unable to decrypt stream.
      if (_cardHandler.IsScrambled(ref user))
      {
        if (!_cardHandler.HasCA)
        {
          Log.Write("card: WaitForTimeShiftFile - return scrambled, since card has no CAM.");
          scrambled = true;
          return false;
        }
      }

      int waitForEvent = _waitForTimeshifting * 1000; // in ms           

      DateTime timeStart = DateTime.Now;

      if (_cardHandler.Card.SubChannels.Length <= 0)
        return false;

      IChannel channel = _subchannel.CurrentChannel;
      bool isRadio = channel.IsRadio;

      if (isRadio)
      {
        Log.Write("card: WaitForTimeShiftFile - waiting _eventAudio");
        // wait for audio PID to be seen
        if (_eventAudio.WaitOne(waitForEvent, true))
        {
          // start of the video & audio is seen
          TimeSpan ts = DateTime.Now - timeStart;
          Log.Write("card: WaitForTimeShiftFile - audio is seen after {0} seconds", ts.TotalSeconds);
          return true;
        }
        else
        {
          TimeSpan ts = DateTime.Now - timeStart;
          Log.Write("card: WaitForTimeShiftFile - no audio was found after {0} seconds", ts.TotalSeconds);
          if (_cardHandler.IsScrambled(ref user))
          {
            Log.Write("card: WaitForTimeShiftFile - audio stream is scrambled");
            scrambled = true;
          }
        }
      }
      else
      {
        Log.Write("card: WaitForTimeShiftFile - waiting _eventAudio & _eventVideo");
        // block until video & audio PIDs are seen or the timeout is reached
        if (_eventAudio.WaitOne(waitForEvent, true))
        {
          if (_eventVideo.WaitOne(waitForEvent, true))
          {
            // start of the video & audio is seen
            TimeSpan ts = DateTime.Now - timeStart;
            Log.Write("card: WaitForTimeShiftFile - video and audio are seen after {0} seconds",
                      ts.TotalSeconds);
            return true;
          }
          else
          {
            TimeSpan ts = DateTime.Now - timeStart;
            Log.Write(
                "card: WaitForTimeShiftFile - audio was found, but video was not found after {0} seconds",
                ts.TotalSeconds);
            if (_cardHandler.IsScrambled(ref user))
            {
              Log.Write("card: WaitForTimeShiftFile - audio stream is scrambled");
              scrambled = true;
            }
          }
        }
        else
        {
          TimeSpan ts = DateTime.Now - timeStart;
          Log.Write("card: WaitForTimeShiftFile - no audio was found after {0} seconds", ts.TotalSeconds);
          if (_cardHandler.IsScrambled(ref user))
          {
            Log.Write("card: WaitForTimeShiftFile - audio and video stream is scrambled");
            scrambled = true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Fetches the stream quality information
    /// </summary>   
    /// <param name="user">user</param>    
    /// <param name="totalTSpackets">Amount of packets processed</param>    
    /// <param name="discontinuityCounter">Number of stream discontinuities</param>
    /// <returns></returns>
    public void GetStreamQualityCounters(IUser user, out int totalTSpackets, out int discontinuityCounter)
    {
      totalTSpackets = 0;
      discontinuityCounter = 0;

      ITvCardContext context = _cardHandler.Card.Context as ITvCardContext;
      bool userExists;
      context.GetUser(ref user, out userExists);
      ITvSubChannel subchannel = _cardHandler.Card.GetSubChannel(user.SubChannel);

      TvDvbChannel dvbSubchannel = subchannel as TvDvbChannel;

      if (dvbSubchannel != null)
      {
        dvbSubchannel.GetStreamQualityCounters(out totalTSpackets, out discontinuityCounter);
      }
    }    

    public void OnBeforeTune()
    {
      Log.Debug("TimeShifter.OnBeforeTune: resetting audio/video events");
      _tuneInProgress = true;
      _eventAudio.Reset();
      _eventVideo.Reset();
    }

    public void OnAfterTune()
    {
      Log.Debug("TimeShifter.OnAfterTune: resetting audio/video time");
      _timeAudioEvent = DateTime.MinValue;
      _timeVideoEvent = DateTime.MinValue;

      _tuneInProgress = false;
    }
  }
}