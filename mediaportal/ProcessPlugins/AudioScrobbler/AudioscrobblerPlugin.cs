#region Copyright (C) 2006 Team MediaPortal

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
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Player;

namespace MediaPortal.Audioscrobbler
{
  public class AudioscrobblerPlugin : ISetupForm, IPlugin
  {
    private int _timerTickSecs = 15;
    private int skipThreshold = 6;
    // maybe increase after introduction of x-fading
    private const int STARTED_LATE = 15;

    // songs longer or shorter than this won't be submitted
    private const int MIN_DURATION = 30;
    private const int MAX_DURATION = 2700;

    private const int INFINITE_TIME = Int32.MaxValue;

    private Song currentSong;
    // whether the current song has been submitted    
    private bool queued;
    // when to submbit the current song
    private int alertTime;
    // check for skipping
    private int lastPosition = 0;

    public bool _doSubmit = true;

    private System.Timers.Timer SongCheckTimer;


    #region Properties
    /* The number of seconds at which the current song will be queued */
    public int AlertTime
    {
      get
      {
        return alertTime;
      }
    }

    /* Whether the current song has been added to the queue */
    public bool Queued
    {
      get
      {
        return queued;
      }
    }

    //public static AudioscrobblerBase Audioscrobbler
    //{
    //  get {
    //    return scrobbler;
    //  }
    //}

    private void OnManualDisconnect(object sender, EventArgs args)
    {
      AudioscrobblerBase.Disconnect();
    }

    private void OnManualConnect(object sender, EventArgs args)
    {
      if(!AudioscrobblerBase.Connected)
        AudioscrobblerBase.Connect();
    }

    private void OnEnabledChangedEvent(bool isEnabled)
    {
      if (isEnabled)
        OnManualConnect(null, null);
      else
      {
        AudioscrobblerBase.Disconnect();
      }
    }
    #endregion

    #region MediaPortal events
    // Make sure we get all of the ACTION_PLAY events (OnAction only receives the ACTION_PLAY event when 
    // the player is not playing)...     
    void OnNewAction(Action action)
    {
      if ((action.wID == Action.ActionType.ACTION_PLAY || action.wID == Action.ActionType.ACTION_MUSIC_PLAY || action.wID == Action.ActionType.ACTION_PAUSE))
      {
        // finalize stuff before new song starts?
        OnStateChangedEvent(true);
      }
      if (action.wID == Action.ActionType.ACTION_STOP)
        OnStateChangedEvent(false);
      if (action.wID == Action.ActionType.ACTION_NEXT_ITEM || action.wID == Action.ActionType.ACTION_PREV_ITEM)
      {
        OnStateChangedEvent(true);
      }
    }


    /// <summary>
    /// Gets called if you skip to other tracks - mainly to set the alert time
    /// </summary>
    /// <param name="currentSong">accepts the current playing Song reference</param>
    public void OnSongChangedEvent(Song currentSong)
    {
      queued = false;
      alertTime = INFINITE_TIME;

      if (!_doSubmit || currentSong == null)
        return;

      // Only submit if we have reasonable info about the song
      if (currentSong.Artist == "" || currentSong.Title == "")
      {
        Log.Info("Audioscrobbler plugin: {0}", "no tags found ignoring song");
        return;
      }

      // Don't queue if the song didn't start at 0
      if (Convert.ToInt32(g_Player.Player.CurrentPosition) <= (STARTED_LATE + _timerTickSecs))
      {
        alertTime = GetAlertTime();
        currentSong.AudioScrobblerStatus = SongStatus.Loaded;
        return;
      }
      else
        Log.Info("Audioscrobbler plugin: {0}", "song started late - ignoring");
    }

    /// <summary>
    /// Gets called everytime the playback status of MyMusic changes.
    /// </summary>
    /// <param name="playing">on true it does a Song lookup for new Tracks if necessary</param>
    public void OnStateChangedEvent(bool playing)
    {
      if (playing)
      {
        if (currentSong == null)
          currentSong = new Song();
        // Track has changed
        if (g_Player.CurrentFile != currentSong.FileName)
        {
          bool songFound = false;
          if (g_Player.IsCDA)
          {
            if (g_Player.CurrentFile.IndexOf("Track") > 0 && g_Player.CurrentFile.IndexOf(".cda") > 0)
            {
              currentSong.Artist = GUIPropertyManager.GetProperty("#Play.Current.Artist");
              currentSong.Title = GUIPropertyManager.GetProperty("#Play.Current.Title");
              currentSong.Album = GUIPropertyManager.GetProperty("#Play.Current.Album");              
              //currentSong.Track = Int32.Parse(GUIPropertyManager.GetProperty("#Play.Current.Track"), System.Globalization.NumberStyles.Integer, new System.Globalization.CultureInfo("en-US"));              
              currentSong.Duration = Convert.ToInt32(g_Player.Duration);
              currentSong.Genre = GUIPropertyManager.GetProperty("#Play.Current.Genre");              
              currentSong.FileName = g_Player.CurrentFile;

              songFound = currentSong.Artist != "" ? true : false;
              //return;
            }
          }
          else
          {
            // local DB file
            MusicDatabase dbs = new MusicDatabase();
            string strFile = g_Player.Player.CurrentFile;
            songFound = dbs.GetSongByFileName(strFile, ref currentSong);
          }

          if (songFound)
          {
            // playback couuuuld be stopped in theory
            if (g_Player.Playing)
            {
              currentSong.AudioScrobblerStatus = SongStatus.Init;
              currentSong.DateTimePlayed = DateTime.UtcNow - TimeSpan.FromSeconds(g_Player.CurrentPosition);
              // avoid false skip detection            
              lastPosition = Convert.ToInt32(g_Player.Player.CurrentPosition);
              OnSongChangedEvent(currentSong);
            }
          }
          // DB lookup of song failed
          else
            if (g_Player.IsMusic)
            {
              Log.Info("Audioscrobbler plugin: database does not contain track - ignoring track: {0} by {1} from {2}", currentSong.Title, currentSong.Artist, currentSong.Album);
              Log.Debug("g_player: filename of current song - {0}", g_Player.CurrentFile);
            }

        }
        else // Track was paused / unpaused
        {
          // avoid false skip detection
          if (g_Player.Playing && g_Player.CurrentPosition > 0)
          {
            lastPosition = Convert.ToInt32(g_Player.CurrentPosition);
            if (currentSong.AudioScrobblerStatus == SongStatus.Init)
              Log.Info("Audioscrobbler plugin: {0}", "track paused - avoid skip protection");
          }
        }
      }
    }

    public void OnTickEvent(object trash_, ElapsedEventArgs args_)
    {
      if (!_doSubmit)
        return;
      int position = 0;
      if (g_Player.Playing)
      {
        // attempt to detect skipping
        position = Convert.ToInt32(g_Player.Player.CurrentPosition);
        if (currentSong != null)
        {
          // manually skipped song with "SELECT" action
          if (g_Player.CurrentFile != currentSong.FileName)
          {
            OnStateChangedEvent(true);
            return;
          }

          if (!queued)
          {
            if (alertTime < INFINITE_TIME && position > (lastPosition + skipThreshold + _timerTickSecs))
            {
              alertTime = INFINITE_TIME;
              Log.Info("Audioscrobbler plugin: song was forwarded - ignoring {0}", currentSong.ToShortString());
            }
            // then actually queue the song if we're that far along
            if (position >= alertTime && alertTime > 14)
            {
              Log.Info("Audioscrobbler plugin: queuing song: {0}", currentSong.ToShortString());
              AudioscrobblerBase.pushQueue(currentSong);
              queued = true;
              currentSong.AudioScrobblerStatus = SongStatus.Cached;
            }

            lastPosition = position;
          }
        }
        else // Playing but no Song? Action missed! (Or manually started via "SELECT" action)
          OnStateChangedEvent(true);
      }
    }
    #endregion

    #region Utilities
    private void startStopSongCheckTimer(bool startNow)
    {
      if (SongCheckTimer == null)
        SongCheckTimer = new System.Timers.Timer();
      if (startNow)
      {
        Log.Info("Audioscrobbler plugin: {0}", "starting check timer");
        SongCheckTimer.Interval = _timerTickSecs * 1000;
        SongCheckTimer.Elapsed += new ElapsedEventHandler(OnTickEvent);
        SongCheckTimer.Start();
      }
      else
        SongCheckTimer.Stop();
    }

    // Logic about when we should submit a song to Audioscrobbler - http://www.audioscrobbler.net/wiki/Protocol1.1   
    private int GetAlertTime()
    {
      if (currentSong.Duration > MAX_DURATION)
      {
        Log.Info("Audioscrobbler plugin: ignoring long song {0}", currentSong.ToShortString());
        return INFINITE_TIME;
      }
      else if (currentSong.Duration < MIN_DURATION)
      {
        Log.Info("Audioscrobbler plugin: ignoring short song {0}", currentSong.ToShortString());
        return INFINITE_TIME;
      }
      // If the duration is less then 480 secs, alert when the song
      // is half over, otherwise after 240 seconds.
      if (currentSong.Duration < 480)
        return currentSong.Duration / 2;
      else
        return 240;
    }

    #endregion

    #region IPlugin Members
    public void Start()
    {
      string currentUser = "";

      currentSong = null;
      queued = false;      
      alertTime = INFINITE_TIME;

      GUIWindowManager.OnNewAction += new OnActionHandler(OnNewAction);

      startStopSongCheckTimer(true);

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        currentUser = xmlreader.GetValueAsString("audioscrobbler", "user", "");
      }

      MusicDatabase mdb = new MusicDatabase();
      _doSubmit = (mdb.AddScrobbleUserSettings(Convert.ToString(mdb.AddScrobbleUser(currentUser)), "iSubmitOn", -1) == 1) ? true : false;

      Log.Info("Audioscrobbler plugin: submitting songs: {0}", Convert.ToString(_doSubmit));

      if (_doSubmit)
      {
        OnManualConnect(null, null);
      }
    }

    public void Stop()
    {
      OnManualDisconnect(null, null);
      startStopSongCheckTimer(false);
    }

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {    
      return "The Audioscrobbler plugin populates your profile on http://www.last.fm \nand automatically fills your playlist with songs you'll like.";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      return -1;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = null;
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;
      return false;
    }

    public string Author()
    {
      return "rtv";
    }

    public string PluginName()
    {
      return "Audioscrobbler";
    }

    public bool HasSetup()
    {
      return true;
    }

    public void ShowPlugin()
    {
      Form assetup = new AudioScrobbler.AudioscrobblerSettings();
      assetup.ShowDialog();
    }

    #endregion
  }
}
