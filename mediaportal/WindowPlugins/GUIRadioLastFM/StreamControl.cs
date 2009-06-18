#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

#region Usings

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Playlists;
using MediaPortal.Utils.Web;
using MediaPortal.Configuration;

#endregion

namespace MediaPortal.GUI.RADIOLASTFM
{

  #region enums

  /// <summary>
  /// Indicates the current playback status
  /// </summary>
  public enum StreamPlaybackState : int
  {
    offline = 0,
    initialized = 1,
    nocontent = 2,
    starting = 3,
    streaming = 4,
    paused = 5,
  }

  /// <summary>
  /// Possible user interactions to manipulate the radio stream
  /// </summary>
  public enum StreamControls : int
  {
    skiptrack = 0,
    lovetrack = 1,
    bantrack = 2,
  }

  /// <summary>
  /// Do we want to listen to "artist" radio or maybe some specific "tag"
  /// </summary>
  public enum StreamType : int
  {
    Artist = 0,
    Group = 1,
    Loved = 2,
    Library = 3,
    Recommended = 4,
    Tag = 5,
    Neighbourhood = 6,
    Playlist = 7,
    Radio = 8,
    Unknown = 9,
  }

  #endregion

  /// <summary>
  /// Handles last.fm radio protocol interaction
  /// </summary>
  internal class StreamControl
  {
    #region Event delegates

    public delegate void RadioSettingsLoaded();

    public event RadioSettingsLoaded RadioSettingsSuccess;

    public delegate void RadioSettingsFailed();

    public event RadioSettingsFailed RadioSettingsError;

    #endregion

    #region Variables

    private PlayListPlayer PlaylistPlayer = null;

    /// <summary>
    /// The "filename" used by the player to access the stream
    /// </summary>
    private string _currentRadioURL = String.Empty;

    /// <summary>
    /// The user associated Session ID - from the response to the Audioscrobbler handshake
    /// </summary>
    private string _currentSession = String.Empty;

    /// <summary>
    /// The last.fm user from you configured in the Audioscrobbler plugin
    /// </summary>
    private string _currentUser = String.Empty;

    /// <summary>
    /// The last.fm user which stream will be tuned to
    /// </summary>
    private string _currentStreamsUser = String.Empty;

    /// <summary>
    /// Did you pay for exclusive member options
    /// </summary>
    private bool _isSubscriber = false;

    /// <summary>
    /// Discovery mode tries to avoid stream tracks you've already listened to
    /// </summary>
    private bool _discoveryMode = false;

    /// <summary>
    /// Settings loaded
    /// </summary>
    private bool _isInit = false;

    /// <summary>
    /// Streaming or no content
    /// </summary>
    private StreamPlaybackState _currentState = StreamPlaybackState.offline;

    /// <summary>
    /// Type of desired music
    /// </summary>
    private StreamType _currentTuneType = StreamType.Recommended;

    /// <summary>
    /// Type of most recent playlist
    /// </summary>
    private StreamType _currentPlaylistType = StreamType.Unknown;

    /// <summary>
    /// The time of the last http access
    /// </summary>
    private DateTime _lastConnectAttempt = DateTime.MinValue;

    /// <summary>
    /// Sets the minimum timespan between each http access to avoid hammering
    /// </summary>
    private TimeSpan _minConnectWaitTime = new TimeSpan(0, 0, 1);

    private AsyncGetRequest httpcommand = null;

    #endregion

    #region Constructor

    public StreamControl()
    {
      AudioscrobblerBase.RadioHandshakeSuccess += new AudioscrobblerBase.RadioHandshakeCompleted(OnRadioLoginSuccess);
      AudioscrobblerBase.RadioHandshakeError += new AudioscrobblerBase.RadioHandshakeFailed(OnRadioLoginFailed);

      PlaylistPlayer = PlayListPlayer.SingletonPlayer;
    }

    #endregion

    #region Examples

    // 4. http.request.uri = Request URI: http://ws.audioscrobbler.com/ass/upgrade.php?platform=win&version=1.0.7&lang=en&user=
    // 5. http.request.uri = Request URI: http://ws.audioscrobbler.com/radio/np.php?session=
    // 6. http.request.uri = Request URI: http://ws.audioscrobbler.com/ass/artistmetadata.php?artist=Sportfreunde%20Stiller&lang=en
    // 7. http.request.uri = Request URI: http://ws.audioscrobbler.com/ass/metadata.php?artist=Sportfreunde%20Stiller&track=Alles%20Das&album=Macht%20doch%20was%20ihr%20wollt%20-%20Ich%20geh%2527%20jetzt%2521

    //price=
    //shopname=
    //clickthrulink=
    //streaming=true
    //discovery=0
    //station=Global Tag Radio: metal, viking metal, Melodic Death Metal
    //artist=Sonata Arctica
    //artist_url=http://www.last.fm/music/Sonata+Arctica
    //track=8th Commandment
    //track_url=http://www.last.fm/music/Sonata+Arctica/_/8th+Commandment
    //album=Ecliptica
    //album_url=http://www.last.fm/music/Sonata+Arctica/Ecliptica
    //albumcover_small=http://images.amazon.com/images/P/B00004T40X.01._SCMZZZZZZZ_.jpg
    //albumcover_medium=http://images.amazon.com/images/P/B00004T40X.01._SCMZZZZZZZ_.jpg
    //albumcover_large=http://images.amazon.com/images/P/B00004T40X.01._SCMZZZZZZZ_.jpg
    //trackduration=222
    //radiomode=1
    //recordtoprofile=1

    #endregion

    #region Serialisation

    public void LoadSettings(bool aForceRequired)
    {
      httpcommand = new AsyncGetRequest();
      httpcommand.workerFinished += new AsyncGetRequest.AsyncGetRequestCompleted(OnParseAsyncResponse);
      httpcommand.workerError += new AsyncGetRequest.AsyncGetRequestError(OnAsyncRequestError);

      _currentUser = AudioscrobblerBase.Username;
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _discoveryMode = xmlreader.GetValueAsBool("audioscrobbler", "discoveryenabled", false);
      }

      if (_currentUser.Length > 0)
      {
        AudioscrobblerBase.DoRadioHandshake(aForceRequired);
      }
      else
      {
        OnRadioLoginFailed();
      }
    }

    private void OnRadioLoginSuccess()
    {
      // for now..
      _currentStreamsUser = _currentUser;
      _currentSession = AudioscrobblerBase.RadioSession;

      if (_currentSession != String.Empty)
      {
        _isSubscriber = AudioscrobblerBase.IsSubscriber;
        //_currentRadioURL = "http://streamer1.last.fm/last.mp3?Session=" + _currentSession;
        _currentRadioURL = AudioscrobblerBase.RadioStreamLocation;
        _currentState = StreamPlaybackState.initialized;
        _isInit = true;
        RadioSettingsSuccess();
      }
      else
      {
        RadioSettingsError();
      }
    }

    private void OnRadioLoginFailed()
    {
      _currentState = StreamPlaybackState.offline;
      _currentSession = String.Empty;
      _isInit = false; // need to check that..
      RadioSettingsError();
    }

    #endregion

    #region Getters & Setters

    /// <summary>
    /// The active last.fm user for the plugin
    /// </summary>
    public string AccountUser
    {
      get { return _currentUser; }
    }

    /// <summary>
    /// The username which will be used for stream setups
    /// </summary>
    public string StreamsUser
    {
      get { return _currentStreamsUser; }

      set
      {
        if (value != _currentStreamsUser)
        {
          _currentStreamsUser = value;
          Log.Debug("StreamControl: Setting StreamsUser to {0}", _currentStreamsUser);
        }
      }
    }

    /// <summary>
    /// URL for playback with buffering audioplayers
    /// </summary>
    public string CurrentStream
    {
      get { return _currentRadioURL; }

      set
      {
        if (value != _currentRadioURL)
        {
          _currentRadioURL = value;
          Log.Debug("StreamControl: Setting RadioURL to {0}", _currentRadioURL);
        }
      }
    }

    /// <summary>
    /// Is the current tuned radio someone's favorite or recommendations, etc
    /// </summary>
    public StreamType CurrentTuneType
    {
      get { return _currentTuneType; }
      set { _currentTuneType = value; }
    }

    ///// <summary>
    ///// Is the most recent playlist someone's favorite or recommendations, etc
    ///// </summary>
    //public StreamType CurrentPlaylistType
    //{
    //  get { return _currentPlaylistType; }
    //  set { _currentPlaylistType = value; }
    //}

    public StreamPlaybackState CurrentStreamState
    {
      get { return _currentState; }

      set
      {
        if (value != _currentState)
        {
          _currentState = value;
        }
        Log.Debug("StreamControl: Setting CurrentStreamState to {0}", _currentState.ToString());
      }
    }

    public bool DiscoveryMode
    {
      get { return _discoveryMode; }

      set
      {
        _discoveryMode = value;
        // TODO: add proper multi-user setting...
        using (Profile.Settings xmlwriter = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          xmlwriter.SetValueAsBool("audioscrobbler", "discoveryenabled", _discoveryMode);
        }
      }
    }

    public int DiscoveryEnabledInt
    {
      get { return _discoveryMode ? 1 : 0; }
    }

    /// <summary>
    /// Property to check if the settings are loaded and a session is available
    /// </summary>
    public bool IsInit
    {
      get { return _isInit; }
    }

    /// <summary>
    /// Determines if the user has access to restricted streams
    /// </summary>
    public bool IsSubscriber
    {
      get { return _isSubscriber; }
    }

    #endregion

    #region Control functions

    //public void ToggleRecordToProfile(bool submitTracks_)
    //{
    //  //if (submitTracks_)
    //  //{
    //  //  if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/control.php?session=" + _currentSession + "&command=rtp"))
    //  //  {
    //  //    AudioscrobblerBase.IsSubmittingRadioSongs = true;
    //  //    Log.Info("StreamControl: Enabled submitting of radio tracks to profile");
    //  //  }
    //  //}
    //  //else
    //  //  if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/control.php?session=" + _currentSession + "&command=nortp"))
    //  //  {
    //  //    if (CurrentPlaybackType != PlaybackType.PlaylistPlayer)
    //  //      Log.Info("StreamControl: Disabled submitting of radio tracks to profile");
    //  //  }
    //}

    //public bool ToggleDiscoveryMode(bool enableDiscovery_)
    //{
    //  bool success = false;
    //  string actionCommand = enableDiscovery_ ? "on" : "off";

    //  if (SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession + @"&url=lastfm://settings/discovery/" + actionCommand))
    //  {
    //    success = true;
    //    _discoveryMode = enableDiscovery_;
    //    Log.Info("StreamControl: Toggled discovery mode {0}", actionCommand);
    //  }

    //  return success;
    //}

    #endregion

    #region Tuning functions

    public bool TuneIntoPersonalRadio(string username_)
    {
      string TuneUser = AudioscrobblerBase.getValidURLLastFMString(username_);

      if (
        SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession +
                           @"&url=lastfm://user/" + TuneUser + "/personal"))
      {
        _currentTuneType = StreamType.Library;
        Log.Info("StreamControl: Tune into personal station of: {0}", username_);
        // GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", GUILocalizeStrings.Get(34043) + username_);
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool TuneIntoNeighbourRadio(string username_)
    {
      string TuneUser = AudioscrobblerBase.getValidURLLastFMString(username_);

      if (
        SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession +
                           @"&url=lastfm://user/" + TuneUser + "/neighbours"))
      {
        _currentTuneType = StreamType.Neighbourhood;
        Log.Info("StreamControl: Tune into neighbour station of: {0}", username_);
        // GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", GUILocalizeStrings.Get(34048)); // My neighbour radio
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool TuneIntoLovedTracks(string username_)
    {
      string TuneUser = AudioscrobblerBase.getValidURLLastFMString(username_);

      if (
        SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession +
                           @"&url=lastfm://user/" + TuneUser + "/loved"))
      {
        _currentTuneType = StreamType.Loved;
        Log.Info("StreamControl: Tune into loved tracks of: {0}", username_);
        // GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", GUILocalizeStrings.Get(34044) + username_);
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool TuneIntoGroupRadio(string groupname_)
    {
      string TuneGroup = AudioscrobblerBase.getValidURLLastFMString(groupname_);

      if (
        SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession +
                           @"&url=lastfm://group/" + TuneGroup))
      {
        _currentTuneType = StreamType.Group;
        Log.Info("StreamControl: Tune into group radio for: {0}", groupname_);

        // GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", "Group radio of: " + groupname_);

        return true;
      }
      else
      {
        return false;
      }
    }

    public bool TuneIntoRecommendedRadio(string username_)
    {
      string TuneUser = AudioscrobblerBase.getValidURLLastFMString(username_);

      if (
        SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession +
                           @"&url=lastfm://user/" + TuneUser + "/recommended"))
      {
        _currentTuneType = StreamType.Recommended;
        Log.Info("StreamControl: Tune into recommended station for: {0}", username_);
        // GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", GUILocalizeStrings.Get(34040));
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool TuneIntoArtists(List<String> artists_)
    {
      string TuneArtists = String.Empty;
      foreach (string singleArtist in artists_)
      {
        TuneArtists += AudioscrobblerBase.getValidURLLastFMString(singleArtist) + ",";
      }
      // remove trailing comma
      TuneArtists = TuneArtists.Remove(TuneArtists.Length - 1);
      if (
        SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession +
                           @"&url=lastfm://artist/" + TuneArtists + "/similarartists&lang=de"))
      {
        _currentTuneType = StreamType.Artist;
        Log.Info("StreamControl: Tune into artists similar to: {0}", TuneArtists);
        // GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", "Artists similar to: " + TuneArtists);
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool TuneIntoTags(List<String> tags_)
    {
      string TuneTags = String.Empty;
      foreach (string singleTag in tags_)
      {
        TuneTags += AudioscrobblerBase.getValidURLLastFMString(singleTag) + ",";
      }
      // remove trailing comma
      TuneTags = TuneTags.Remove(TuneTags.Length - 1);

      if (
        SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession +
                           @"&url=lastfm://globaltags/" + TuneTags))
      {
        _currentTuneType = StreamType.Tag;
        Log.Info("StreamControl: Tune into tags: {0}", TuneTags);
        // GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", GUILocalizeStrings.Get(34041) + TuneTags);
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool TuneIntoWebPlaylist(string username_)
    {
      string TuneUser = AudioscrobblerBase.getValidURLLastFMString(username_);
      if (
        SendCommandRequest(@"http://ws.audioscrobbler.com/radio/adjust.php?session=" + _currentSession +
                           @"&url=lastfm://user/" + TuneUser + "/playlist"))
      {
        _currentTuneType = StreamType.Playlist;
        Log.Info("StreamControl: Tune into web playlist of: {0}", username_);
        // GUIPropertyManager.SetProperty("#Play.Current.Lastfm.CurrentStream", GUILocalizeStrings.Get(34049));
        return true;
      }
      else
      {
        return false;
      }
    }

    #endregion

    #region Network related

    private bool SendCommandRequest(string url_)
    {
      try
      {
        // Enforce a minimum wait time between connects.
        DateTime nextconnect = _lastConnectAttempt.Add(_minConnectWaitTime);
        if (DateTime.Now < nextconnect)
        {
          TimeSpan waittime = nextconnect - DateTime.Now;
          //Log.Debug("StreamControl: Avoiding too fast connects for {0} - sleeping until {1}", url_, nextconnect.ToString());
          Thread.Sleep(waittime);
        }
      }
      // While debugging you might get a waittime which is no longer a valid integer.
      catch (Exception)
      {
      }

      _lastConnectAttempt = DateTime.Now;

      httpcommand.SendAsyncGetRequest(url_);

      return true;
    }

    private void SendDelayedCommandRequest(string url_, int delayMSecs_)
    {
      httpcommand.SendDelayedAsyncGetRequest(url_, delayMSecs_);
    }

    private void OnAsyncRequestError(String urlCommand, Exception errorReason)
    {
      try
      {
        Log.Warn("StreamControl: Async request for {0} unsuccessful: {1}", urlCommand, errorReason.Message);
      }
      finally
      {
        httpcommand.workerError -= new AsyncGetRequest.AsyncGetRequestError(OnAsyncRequestError);
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void OnParseAsyncResponse(List<string> responseList, HttpStatusCode responseCode, String requestedURLCommand)
    {
      // parse the response
      try
      {
        string responseMessage = String.Empty;
        if (responseList.Count > 0)
        {
          List<string> responseStrings = new List<string>(responseList);

          if (responseCode == HttpStatusCode.OK)
          {
            responseMessage = responseStrings[0];
            {
              if (responseMessage.StartsWith("response=OK"))
              {
                ParseSuccessful(responseStrings, requestedURLCommand);
                return;
              }

              //if (responseMessage.StartsWith("price="))
              //{
              //  ParseNowPlaying(responseStrings);
              //  return;
              //}
            }
          }
          else
          {
            string logmessage = "StreamControl: ***** Unknown response! - " + responseMessage;
            foreach (String unkStr in responseStrings)
            {
              logmessage += "\n" + unkStr;
            }

            if (logmessage.Contains("Not enough content"))
            {
              _currentState = StreamPlaybackState.nocontent;
              Log.Warn("StreamControl: Not enough content left to play this station");
              return;
            }
            else
            {
              Log.Warn(logmessage);
            }
          }
        }
        else
        {
          Log.Debug("StreamControl: SendCommandRequest: Reader object already destroyed");
        }
      }
      catch (Exception e)
      {
        Log.Error("StreamControl: SendCommandRequest: Parsing response failed {0}", e.Message);
        return;
      }
      finally
      {
        httpcommand.workerFinished -= new AsyncGetRequest.AsyncGetRequestCompleted(OnParseAsyncResponse);
      }
    }

    # endregion

    #region Response parser

    private void ParseSuccessful(List<String> responseList_, String formerRequest_)
    {
      if (formerRequest_.Contains(@"&command=skip"))
      {
        Log.Info("StreamControl: Successfully send skip command");
        return;
      }

      if (formerRequest_.Contains(@"&command=love"))
      {
        Log.Info("StreamControl: Track added to loved tracks list");
        return;
      }

      if (formerRequest_.Contains(@"&command=ban"))
      {
        Log.Info("StreamControl: Track added to banned tracks list");
        return;
      }
    }

    //private void ParseNowPlaying(List<String> responseList_)
    //{
    //  List<String> NowPlayingInfo = new List<string>();
    //  String prevTitle = CurrentSongTag.Title;
    //  CurrentSongTag.Clear();

    //  try
    //  {
    //    foreach (String respStr in responseList_)
    //      NowPlayingInfo.Add(respStr);

    //    foreach (String token in NowPlayingInfo)
    //    {
    //      if (token.StartsWith("artist="))
    //      {
    //        if (token.Length > 7)
    //          CurrentSongTag.Artist = token.Substring(7);
    //      }
    //      else if (token.StartsWith("album="))
    //      {
    //        if (token.Length > 6)
    //          CurrentSongTag.Album = token.Substring(6);
    //      }
    //      else if (token.StartsWith("track="))
    //      {
    //        if (token.Length > 6)
    //          CurrentSongTag.Title = token.Substring(6);
    //      }
    //      else if (token.StartsWith("station="))
    //      {
    //        if (token.Length > 8)
    //          CurrentSongTag.Genre = token.Substring(8);
    //      }
    //      else if (token.StartsWith("albumcover_large="))
    //      {
    //        if (token.Length > 17)
    //          CurrentSongTag.Comment = token.Substring(17);
    //      }
    //      else if (token.StartsWith("trackduration="))
    //      {
    //        if (token.Length > 14)
    //        {
    //          int trackLength = Convert.ToInt32(token.Substring(14));
    //          CurrentSongTag.Duration = trackLength;
    //        }
    //      }
    //    }

    //    if (CurrentSongTag.Title != prevTitle)
    //    {
    //      //AudioscrobblerBase.CurrentSong.Clear();
    //      //AudioscrobblerBase.CurrentSong.Artist = CurrentSongTag.Artist;
    //      //AudioscrobblerBase.CurrentSong.Album = CurrentSongTag.Album;
    //      //AudioscrobblerBase.CurrentSong.Title = CurrentSongTag.Title;
    //      //AudioscrobblerBase.CurrentSong.Genre = CurrentSongTag.Genre;
    //      //AudioscrobblerBase.CurrentSong.Duration = CurrentSongTag.Duration;
    //      //AudioscrobblerBase.CurrentSong.WebImage = CurrentSongTag.Comment;
    //      //AudioscrobblerBase.CurrentSong.FileName = g_Player.Player.CurrentFile;

    //      // fire the event
    //      if (StreamSongChanged != null)
    //        StreamSongChanged(CurrentSongTag, DateTime.Now);

    //      //GUIPropertyManager.SetProperty("#Play.Current.Artist", CurrentSongTag.Artist);
    //      //GUIPropertyManager.SetProperty("#Play.Current.Album", CurrentSongTag.Album);
    //      //GUIPropertyManager.SetProperty("#Play.Current.Title", CurrentSongTag.Title);
    //      //GUIPropertyManager.SetProperty("#Play.Current.Genre", CurrentSongTag.Genre);
    //      //GUIPropertyManager.SetProperty("#Play.Current.Thumb", CurrentSongTag.Comment);
    //      //GUIPropertyManager.SetProperty("#trackduration", Util.Utils.SecondsToHMSString(CurrentSongTag.Duration));

    //      // UpdateNotifyBallon();
    //    }

    //  }
    //  catch (Exception ex)
    //  {
    //    Log.Error("StreamControl: Error parsing now playing info: {0}", ex.Message);
    //  }
    //}

    #endregion
  }
}