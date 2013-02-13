﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Music;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.LastFM;

using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;

namespace MediaPortal.GUI.LastFMRadio
{

  [PluginIcons("WindowPlugins.GUILastFMRadio.BallonRadio.gif", "WindowPlugins.GUILastFMRadio.BallonRadioDisabled.gif")]
  public class GUILastFMRadio : GUIWindow, ISetupForm
  {

    private enum SkinControls
    {
      USER_RECOMMENDED_BUTTON = 2,
      USER_MIX_BUTTON = 11,
      USER_LIBRARY_BUTTON = 12,
      ARTIST_BUTTON = 13,
      TAG_BUTTON = 14,
      PLAYLIST_CONTROL = 50
    }

    [SkinControlAttribute((int)SkinControls.USER_RECOMMENDED_BUTTON)] protected GUIButtonControl BtnUserRecomended = null;
    [SkinControlAttribute((int)SkinControls.USER_MIX_BUTTON)] protected GUIButtonControl BtnUserMix = null;
    [SkinControlAttribute((int)SkinControls.USER_LIBRARY_BUTTON)] protected GUIButtonControl BtnUserLibrary = null;
    [SkinControlAttribute((int)SkinControls.ARTIST_BUTTON)] protected GUIButtonControl BtnArtist = null;
    [SkinControlAttribute((int)SkinControls.TAG_BUTTON)] protected GUIButtonControl BtnTag = null;
    [SkinControlAttribute((int)SkinControls.PLAYLIST_CONTROL)] protected GUIFacadeControl PlaylistControl = null;

    protected delegate void UpdateThumbsDelegate(GUIListItem item, string strThumbFile);
    protected static TempFileCollection tfc;
    private static PlayListPlayer _playlistPlayer;

    #region ISetupForm Members

    // Returns the name of the plugin which is shown in the plugin menu
    public string PluginName()
    {
      return "Last.fm Radio";
    }

    // Returns the description of the plugin is shown in the plugin menu
    public string Description()
    {
      return "Last.fm Radio";
    }

    // Returns the author of the plugin which is shown in the plugin menu
    public string Author()
    {
      return "Jameson_uk";
    }

    // show the setup dialog
    public void ShowPlugin()
    {
      //MessageBox.Show("Nothing to configure, this is just an example");
    }

    // Indicates whether plugin can be enabled/disabled
    public bool CanEnable()
    {
      return true;
    }

    // Get Windows-ID
    public int GetWindowId()
    {
      // WindowID of windowplugin belonging to this setup
      // enter your own unique code
      return 56789;
    }

    // Indicates if plugin is enabled by default;
    public bool DefaultEnabled()
    {
      return true;
    }

    // indicates if a plugin has it's own setup screen
    public bool HasSetup()
    {
      return false;
    }

    /// <summary>
    /// If the plugin should have it's own button on the main menu of Mediaportal then it
    /// should return true to this method, otherwise if it should not be on home
    /// it should return false
    /// </summary>
    /// <param name="strButtonText">text the button should have</param>
    /// <param name="strButtonImage">image for the button, or empty for default</param>
    /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
    /// <param name="strPictureImage">subpicture for the button or empty for none</param>
    /// <returns>true : plugin needs it's own button on home
    /// false : plugin does not need it's own button on home</returns>

    public bool GetHome(out string strButtonText, out string strButtonImage,
                        out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(34000);
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = "hover_LastFmRadio.png";;
      return true;
    }

    // With GetID it will be an window-plugin / otherwise a process-plugin
    // Enter the id number here again
    public override int GetID
    {
      get
      {
        return 56789;
      }
    }

    #endregion

    #region ctor

    public GUILastFMRadio()
    {
      tfc = new TempFileCollection();
    }

    #endregion

    #region overrides

    public override bool Init()
    {
      _playlistPlayer = PlayListPlayer.SingletonPlayer;
      g_Player.PlayBackEnded += OnPlayBackEnded;
      g_Player.PlayBackChanged += OnPlayBackChanged;
      g_Player.PlayBackStopped += OnPlayBackStopped;
      var a = new LastFMLibrary(); //TODO this is just making _SK get loaded.   No need to actual instansiate
      return Load(GUIGraphicsContext.Skin + @"\lastFmRadio.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      PlaylistControl.CurrentLayout = Layout.List;
      PlaylistControl.Clear();
      LoadPlaylist();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (controlId == (int)SkinControls.USER_RECOMMENDED_BUTTON)
      {
        TuneToStation("lastfm://user/" + LastFMLibrary.CurrentUser + "/recommended");
      }
      if (controlId == (int)SkinControls.USER_MIX_BUTTON)
      {
        TuneToStation("lastfm://user/" + LastFMLibrary.CurrentUser + "/mix");
      }
      if (controlId == (int)SkinControls.USER_LIBRARY_BUTTON)
      {
        TuneToStation("lastfm://user/" + LastFMLibrary.CurrentUser + "/library");
      }
      if (controlId == (int)SkinControls.ARTIST_BUTTON)
      {
        string strArtist = GetInputFromUser("Select Artist");
        if(!string.IsNullOrEmpty(strArtist))
        {
          TuneToStation("lastfm://artist/" + strArtist +"/similarartists"); 
        }
      }
      if (controlId == (int)SkinControls.TAG_BUTTON)
      {
        string strTag = GetInputFromUser("Select Tag");
        if (!string.IsNullOrEmpty(strTag))
        {
          TuneToStation("lastfm://globaltags/" + strTag);
        }
      }
      if (controlId == (int)SkinControls.PLAYLIST_CONTROL)
      {
        _playlistPlayer.Play(PlaylistControl.SelectedListItemIndex);
      }

      base.OnClicked(controlId, control, actionType);
    }

    #endregion

    #region last.fm code

    /// <summary>
    /// Tune to a new last.fm radio station and add tracks to playlist
    /// </summary>
    /// <param name="strStation"></param>
    private void TuneToStation(string strStation)
    {

      Log.Debug("Attempting to Tune to last.fm station: {0}", strStation);

      // Clear playlist and start playback
      var pl = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_LAST_FM);
      pl.Clear();
      _playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_LAST_FM;
      _playlistPlayer.Reset();

      LastFMLibrary.TuneRadio(strStation);
      AddMoreTracks();
      _playlistPlayer.Play(0);
    }

    /// <summary>
    /// Get more tracks for the current last.fm radio station and add to playlist
    /// </summary>
    private void AddMoreTracks()
    {
      List<LastFMStreamingTrack> tracks;
      if (!LastFMLibrary.GetRadioPlaylist(out tracks))
      {
        var dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        if (null != dlgNotify)
        {
          dlgNotify.SetHeading(GUILocalizeStrings.Get(107890));
          //TODO: need localised string and possibly better check for lack of content vs other failure
          dlgNotify.SetText("Error getting tracks.  Most likely too many skips.\nWait a while or try another station.");
          dlgNotify.DoModal(GetID);
        }
        return;
      }
     
      foreach (var lastFMTrack in tracks)
      {
        var tag = new MusicTag
        {
          AlbumArtist = lastFMTrack.ArtistName,
          Artist = lastFMTrack.ArtistName,
          Title = lastFMTrack.TrackTitle,
          FileName = lastFMTrack.TrackURL,
          Duration = lastFMTrack.Duration,
          Lyrics = lastFMTrack.ImageURL //TODO: this should not be lyrics
        };
        var pli = new PlayListItem
        {
          Type = PlayListItem.PlayListItemType.Audio,
          FileName = lastFMTrack.TrackURL,
          Description = lastFMTrack.ArtistName + " - " + lastFMTrack.TrackTitle,
          Duration = lastFMTrack.Duration,
          MusicTag = tag
        };

        Log.Info("Artist: {0} :Title: {1} :URL: {2}", lastFMTrack.ArtistName, lastFMTrack.TrackTitle, lastFMTrack.TrackURL);

        var pl = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_LAST_FM);
        pl.Add(pli);

      }
      LoadPlaylist();
    }

    #endregion

    #region g_player events

    private void OnPlayBackEnded(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Music)
      {
        return;
      }
      DoOnEnded();
    }

    /// <summary>
    /// Handle event fired by MP player.
    /// Playback has changed; this event signifies that the existing item has been changed
    /// this could be because that song has ended and playback has moved to next item in playlist
    /// or could be because user has skipped tracks
    /// </summary>
    /// <param name="type">MediaType of item that was playing</param>
    /// <param name="stoptime">Number of seconds item has played for when it stopped</param>
    /// <param name="filename">filename of item that was playing</param>
    private void OnPlayBackChanged(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type != g_Player.MediaType.Music)
      {
        return;
      }
      DoOnChanged();
    }

    /// <summary>
    /// Handle event fired by MP player.
    /// Playback has stopped; user has pressed stop mid way through a track
    /// </summary>
    /// <param name="type">MediaType of track that was stopped</param>
    /// <param name="stoptime">Number of seconds item has played for before it was stopped</param>
    /// <param name="filename">filename of item that was stopped</param>
    private void OnPlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type != g_Player.MediaType.Music)
      {
        return;
      }

      DoOnEnded();
    }

    /// <summary>
    /// When track changes (either by user switching tracks or previous track ended)
    /// Remove old tracks from playlist and ensure there is a stock of new tracks
    /// </summary>
    private void DoOnChanged()
    {
      if (_playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_LAST_FM)
      {
        return;
      }
      var pl = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_LAST_FM);
      var currentTrackIndex = _playlistPlayer.CurrentSong;

      for (int i = currentTrackIndex - 1; i >= 0; i--)
      {
        RemovePlayListItem(i);
      }

      if (pl.Count< 5)
      {
        AddMoreTracks();
      }
    }

    /// <summary>
    /// Handle situation where music ends (user pressed stop).   Clear playlist
    /// </summary>
    private void DoOnEnded()
    {
      if (_playlistPlayer.CurrentPlaylistType != PlayListType.PLAYLIST_LAST_FM)
      {
        return;
      }
      var pl = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_LAST_FM);
      pl.Clear();
      _playlistPlayer.Reset();
      LoadPlaylist();
      
    }
    
    #endregion

    #region GUI methods

    /// <summary>
    /// Check for thumbs.   Will check for
    ///  1 - Local thumb (from users music collection)
    ///  2 - Already downloaded thumb from last.fm
    ///  3 - Last.fm thumb and download accordingly
    /// Thumbs are stored in temp folder and deleted when MP is closed
    /// </summary>
    /// <param name="item">Item to lookup thumb for</param>
    protected virtual void OnRetrieveCoverArt(GUIListItem item)
    {
      Util.Utils.SetDefaultIcons(item);
      if (item.Label == "..")
      {
        return;
      }
      MusicTag tag = (MusicTag)item.MusicTag;
      string trackImgURL = tag.Lyrics;
      string tmpThumb = GetTemporaryThumbName(trackImgURL);

      string strThumb = GUIMusicBaseWindow.GetCoverArt(item.IsFolder, item.Path, tag);
      if (strThumb != string.Empty)
      {
        UpdateListItemImage(item, strThumb);
      }
      else if (File.Exists(tmpThumb))
      {
        Log.Debug("last.fm thumb already exists for: {0} - {1}", tag.Artist, tag.Title);
        item.ThumbnailImage = tmpThumb;
        item.IconImageBig = tmpThumb;
        item.IconImage = tmpThumb;
        UpdateListItemImage(item, tmpThumb);
      }
      else
      {
        Log.Debug("Downloading last.fm thumb for: {0} - {1}", tag.Artist, tag.Title);
        if (!string.IsNullOrEmpty(trackImgURL))
        {
          // download image from last.fm in background thread
          var worker = new BackgroundWorker();
          worker.DoWork += (obj, e) => ImageDownloadDoWork(trackImgURL, tmpThumb, item);
          worker.RunWorkerAsync();
        }
      }
    }

    /// <summary>
    /// Displays a virtual keyboard
    /// </summary>
    /// <param name="aDefaultText">a text which will be preselected</param>
    /// <returns>the text entered by the user</returns>
    private string GetInputFromUser(string aDefaultText)
    {
      string searchterm = aDefaultText;
      var keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      keyboard.Reset();
      keyboard.IsSearchKeyboard = false;
      keyboard.Text = searchterm;
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        searchterm = keyboard.Text;
      }
      return searchterm;
    }

    /// <summary>
    /// Load Facade Control
    /// </summary>
    private void LoadPlaylist()
    {
      PlaylistControl.Clear();
      var playlist = _playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_LAST_FM);
      foreach (var pli in playlist)
      {
        var pItem = new GUIListItem(pli.Description);

        var tag = (MusicTag) pli.MusicTag;
        var dirtyTag = false;
        if (tag != null)
        {
          pItem.MusicTag = tag;
          if (tag.Title == ("unknown") || tag.Title.IndexOf("unknown") > 0 || tag.Title == string.Empty)
          {
            dirtyTag = true;
          }
        }
        else
        {
          dirtyTag = true;
        }

        if (tag != null && !dirtyTag)
        {
          var duration = Util.Utils.SecondsToHMSString(tag.Duration);
          pItem.Label = string.Format("{0} - {1}", tag.Artist, tag.Title);
          pItem.Label2 = duration;
          pItem.MusicTag = pli.MusicTag;
        }

        pItem.Path = pli.FileName;
        pItem.IsFolder = false;

        if (pli.Played)
        {
          pItem.Shaded = true;
        }

        Util.Utils.SetDefaultIcons(pItem);

        pItem.OnRetrieveArt += OnRetrieveCoverArt;

        PlaylistControl.Add(pItem);
      }
      //select first item in list (this will update selected thumb skin property)
      GUIControl.SelectItemControl(GetID, PlaylistControl.GetID, 0);
    }

    #endregion

    #region playlist changes

    private void RemovePlayListItem(int iItem)
    {
      GUIListItem pItem = PlaylistControl[iItem];
      if (pItem == null)
      {
        return;
      }
      string strFileName = pItem.Path;

      _playlistPlayer.Remove(PlayListType.PLAYLIST_LAST_FM, strFileName);

      LoadPlaylist();

      GUIControl.SelectItemControl(GetID, PlaylistControl.GetID, iItem);
    }

    #endregion

    #region Thumb download

    /// <summary>
    /// Background thread to download thumbs from last.fm where there is no local thumb
    /// </summary>
    /// <param name="strTrackImgURL">The URL of the image for track</param>
    /// <param name="tmpThumbName">Full path to file to download image to</param>
    /// <param name="item">GUIListItem to which this thumb relates</param>
    private static void ImageDownloadDoWork(string strTrackImgURL, string tmpThumbName, GUIListItem item)
    {
      Util.Utils.DownLoadImage(strTrackImgURL, tmpThumbName);
      tfc.AddFile(tmpThumbName, false);
      GUIGraphicsContext.form.Invoke(new UpdateThumbsDelegate(UpdateListItemImage), item, tmpThumbName);

    }

    /// <summary>
    /// Update Playlist control items with thumbs
    /// </summary>
    /// <param name="item">Item to update</param>
    /// <param name="strThumbFile">Thumb file to use</param>
    private static void UpdateListItemImage(GUIListItem item, string strThumbFile)
    {
      if (item == null) return;

      var pli = _playlistPlayer.GetCurrentItem();
      if (item.Path == pli.FileName)
      {
        GUIPropertyManager.SetProperty("#Play.Current.Thumb", strThumbFile);
      }

      item.ThumbnailImage = strThumbFile;
      item.IconImageBig = strThumbFile;
      item.IconImage = strThumbFile;

      // let us test if there is a larger cover art image
      string strLarge = Util.Utils.ConvertToLargeCoverArt(strThumbFile);
      if (Util.Utils.FileExistsInCache(strLarge))
      {
        item.ThumbnailImage = strLarge;
      }
    }

    /// <summary>
    /// Generate a standard file name in %TEMP% that will be used for downloaded thumbs from last.fm
    /// </summary>
    /// <param name="strUrl">URL of image</param>
    /// <returns>full file path to temporary file</returns>
    private static string GetTemporaryThumbName(string strUrl)
    {
      if (string.IsNullOrEmpty(strUrl)) return string.Empty;
      try
      {
        var s = new Uri(strUrl);
        return Path.Combine(Path.GetTempPath(), string.Format("MP-Lastfm-{0}", Path.GetFileName(s.LocalPath)));
      }
      catch (Exception)
      {
        return string.Empty;
      }
    }
  
    #endregion

  }
}
