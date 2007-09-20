#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;
using MediaPortal.Dialogs;
using MediaPortal.GUI.View;
using MediaPortal.MusicImport;
using MediaPortal.Configuration;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for GUIMusicBaseWindow.
  /// </summary>
  public class GUIMusicBaseWindow : GUIWindow
  {

    #region enums

    protected enum Level
    {
      Root,
      Sub
    }

    public enum View
    {
      List = 0,
      Icons = 1,
      LargeIcons = 2,
      Albums = 3,
      FilmStrip = 4,
      PlayList = 5
    }

    protected enum PlayNowJumpToType    //SV Added by SteveV 2006-09-07
    {
      None = 0,
      NowPlayingAlways,
      NowPlayingMultipleItems,
      CurrentPlaylistAlways,
      CurrentPlaylistMultipleItems,
      FullscreenIfVizEnabledAlways,
      FullscreenIfVizEnabledMultipleItems,
    }

    #endregion

    #region Base variables

    protected View currentView = View.List;
    protected View currentViewRoot = View.List;
    protected MusicSort.SortMethod currentSortMethod = MusicSort.SortMethod.Name;
    protected MusicSort.SortMethod currentSortMethodRoot = MusicSort.SortMethod.Name;
    protected bool m_bSortAscending;
    protected bool m_bSortAscendingRoot;
    protected string m_strPlayListPath = string.Empty;
    private bool m_bUseID3 = false;
    private bool _autoShuffleOnLoad = false;

    protected MusicViewHandler handler;
    protected MusicDatabase m_database;

    const string defaultTrackTag = "[%track%. ][%artist% - ][%title%]";
    const string albumTrackTag = "[%track%. ][%artist% - ][%title%]";
    string[] _sortModes = { "Name", "Date", "Size", "Track", "Duration", "Title", "Artist", "Album", "Filename", "Rating" };
    string[] _defaultSortTags1 = { defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultTrackTag, defaultTrackTag, albumTrackTag, defaultTrackTag, defaultTrackTag };
    string[] _defaultSortTags2 = { "%duration%", "%year%", "%filesize%", "%duration%", "%duration%", "%duration%", "%duration%", "%duration%", "%filesize%", "%rating%" };

    string[] _sortTags1 = new string[20];
    string[] _sortTags2 = new string[20];
    protected PlayListPlayer playlistPlayer;

    //SV
    //protected int PlayNowJumpToWindowID = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW;
    protected PlayNowJumpToType PlayNowJumpTo = PlayNowJumpToType.None;
    protected bool UsingInternalMusicPlayer = false;

    protected bool PlayAllOnSingleItemPlayNow = false;
    protected string _currentPlaying = string.Empty;

    #endregion

    #region SkinControls

    [SkinControlAttribute(50)]    protected GUIFacadeControl facadeView = null;
    [SkinControlAttribute(2)]     protected GUIButtonControl btnViewAs = null;
    [SkinControlAttribute(3)]     protected GUISortButtonControl btnSortBy = null;
    [SkinControlAttribute(6)]     protected GUIButtonControl btnViews = null;

    [SkinControlAttribute(8)]     protected GUIButtonControl btnSearch = null;
    [SkinControlAttribute(12)]    protected GUIButtonControl btnPlayCd = null;
    [SkinControlAttribute(10)]    protected GUIButtonControl btnSavedPlaylists = null;

    #endregion

    #region Constructor / Destructor

    public GUIMusicBaseWindow()
    {
      playlistPlayer = PlayListPlayer.SingletonPlayer;

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        //string playNowJumpTo = xmlreader.GetValueAsString("musicmisc", "playnowjumpto", "nowplaying");

        //switch (playNowJumpTo)
        //{
        //    case "nowplaying":
        //        PlayNowJumpToWindowID = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW;
        //        break;

        //    case "playlist":
        //        PlayNowJumpToWindowID = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST;
        //        break;

        //    case "none":
        //        PlayNowJumpToWindowID = -1;
        //        break;
        //}

        string playNowJumpTo = xmlreader.GetValueAsString("musicmisc", "playnowjumpto", "none");

        switch (playNowJumpTo)
        {
          case "none":
            PlayNowJumpTo = PlayNowJumpToType.None;
            break;

          case "nowPlayingAlways":
            PlayNowJumpTo = PlayNowJumpToType.NowPlayingAlways;
            break;

          case "nowPlayingMultipleItems":
            PlayNowJumpTo = PlayNowJumpToType.NowPlayingMultipleItems;
            break;

          case "currentPlaylistAlways":
            PlayNowJumpTo = PlayNowJumpToType.CurrentPlaylistAlways;
            break;

          case "currentPlaylistMultipleItems":
            PlayNowJumpTo = PlayNowJumpToType.CurrentPlaylistMultipleItems;
            break;

          case "fullscreenAlways":
            PlayNowJumpTo = PlayNowJumpToType.FullscreenIfVizEnabledAlways;
            break;

          case "fullscreenMultipleItems":
            PlayNowJumpTo = PlayNowJumpToType.FullscreenIfVizEnabledMultipleItems;
            break;

          default:
            PlayNowJumpTo = PlayNowJumpToType.None;
            break;
        }

        _autoShuffleOnLoad = xmlreader.GetValueAsBool("musicfiles", "autoshuffle", false);
      }

      UsingInternalMusicPlayer = BassMusicPlayer.IsDefaultMusicPlayer;
    }

    #endregion 

    #region Serialisation

    protected virtual void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        currentView = (View)xmlreader.GetValueAsInt(SerializeName, "view", (int)View.List);
        currentViewRoot = (View)xmlreader.GetValueAsInt(SerializeName, "viewroot", (int)View.List);

        currentSortMethod = (MusicSort.SortMethod)xmlreader.GetValueAsInt(SerializeName, "sortmethod", (int)MusicSort.SortMethod.Name);
        currentSortMethodRoot = (MusicSort.SortMethod)xmlreader.GetValueAsInt(SerializeName, "sortmethodroot", (int)MusicSort.SortMethod.Name);
        m_bSortAscending = xmlreader.GetValueAsBool(SerializeName, "sortasc", true);
        m_bSortAscendingRoot = xmlreader.GetValueAsBool(SerializeName, "sortascroot", true);
        m_bUseID3 = xmlreader.GetValueAsBool("musicfiles", "showid3", true);

        for (int i = 0; i < _sortModes.Length; ++i)
        {
          _sortTags1[i] = xmlreader.GetValueAsString("mymusic", _sortModes[i] + "1", _defaultSortTags1[i]);
          _sortTags2[i] = xmlreader.GetValueAsString("mymusic", _sortModes[i] + "2", _defaultSortTags2[i]);
        }

        string playListFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        playListFolder += @"\My Playlists";

        m_strPlayListPath = xmlreader.GetValueAsString("music", "playlists", playListFolder);
        m_strPlayListPath = MediaPortal.Util.Utils.RemoveTrailingSlash(m_strPlayListPath);
      }
      SwitchView();
    }

    protected virtual void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue(SerializeName, "view", (int)currentView);
        xmlwriter.SetValue(SerializeName, "viewroot", (int)currentViewRoot);
        xmlwriter.SetValue(SerializeName, "sortmethod", (int)currentSortMethod);
        xmlwriter.SetValue(SerializeName, "sortmethodroot", (int)currentSortMethodRoot);
        xmlwriter.SetValueAsBool(SerializeName, "sortasc", m_bSortAscending);
        xmlwriter.SetValueAsBool(SerializeName, "sortascroot", m_bSortAscendingRoot);
      }
    }

    #endregion

    protected bool UseID3
    {
      get { return m_bUseID3; }
      set { m_bUseID3 = value; }
    }

    protected virtual bool AllowView(View view)
    {
      if (view == View.PlayList)
        return false;
      return true;
    }

    protected virtual bool AllowSortMethod(MusicSort.SortMethod method)
    {
      return true;
    }

    protected virtual View CurrentView
    {
      get { return currentView; }
      set { currentView = value; }
    }

    protected virtual MusicSort.SortMethod CurrentSortMethod
    {
      get { return currentSortMethod; }
      set { currentSortMethod = value; }
    }

    protected virtual bool CurrentSortAsc
    {
      get { return m_bSortAscending; }
      set { m_bSortAscending = value; }
    }

    protected virtual string SerializeName
    {
      get
      {
        return "musicbase";
      }
    }

    protected bool ViewByIcon
    {
      get
      {
        if (CurrentView != View.List)
          return true;
        return false;
      }
    }

    protected bool ViewByLargeIcon
    {
      get
      {
        if (CurrentView == View.LargeIcons)
          return true;
        return false;
      }
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_SHOW_PLAYLIST)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
        return;
      }
      else if (action.wID == Action.ActionType.ACTION_IMPORT_TRACK)
      {
        MusicImport.MusicImport ripper = new MusicImport.MusicImport();
        ripper.EncodeTrack(facadeView, GetID);
        return;
      }
      else if (action.wID == Action.ActionType.ACTION_IMPORT_DISC)
      {
        MusicImport.MusicImport ripper = new MusicImport.MusicImport();
        ripper.EncodeDisc(facadeView, GetID);
        return;
      }
      else if (action.wID == Action.ActionType.ACTION_CANCEL_IMPORT)
      {
        MusicImport.MusicImport ripper = new MusicImport.MusicImport();
        ripper.Cancel();
        return;
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      if (message.Message == GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS)
      {
        _currentPlaying = message.Label;
        facadeView.OnMessage(message);
      }
      return base.OnMessage(message);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnViewAs)
      {
        bool shouldContinue = false;
        do
        {
          shouldContinue = false;
          switch (CurrentView)
          {
            case View.List:
              CurrentView = View.PlayList;
              if (!AllowView(CurrentView) || facadeView.PlayListView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.Playlist;
              break;

            case View.PlayList:
              CurrentView = View.Icons;
              if (!AllowView(CurrentView) || facadeView.ThumbnailView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
              break;

            case View.Icons:
              CurrentView = View.LargeIcons;
              if (!AllowView(CurrentView) || facadeView.ThumbnailView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
              break;

            case View.LargeIcons:
              CurrentView = View.Albums;
              if (!AllowView(CurrentView) || facadeView.AlbumListView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.AlbumView;
              break;

            case View.Albums:
              CurrentView = View.FilmStrip;
              if (!AllowView(CurrentView) || facadeView.FilmstripView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.Filmstrip;
              break;

            case View.FilmStrip:
              CurrentView = View.List;
              if (!AllowView(CurrentView) || facadeView.ListView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.List;
              break;
          }
        } while (shouldContinue);

        SelectCurrentItem();
        GUIControl.FocusControl(GetID, controlId);
        return;
      }//if (control == btnViewAs)

      if (control == btnSortBy)
      {
        OnShowSort();
      }

      if (control == btnViews)
      {
        OnShowViews();
      }

      if (control == btnSavedPlaylists)
      {
        OnShowSavedPlaylists(m_strPlayListPath);
      }

      if (control == facadeView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        int iItem = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SHOW_INFO)
        {
          OnInfo(iItem);
        }
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(iItem);
        }
        if (actionType == Action.ActionType.ACTION_QUEUE_ITEM)
        {
          OnQueueItem(iItem);
        }
      }
    }

    protected void SelectCurrentItem()
    {
      int iItem = facadeView.SelectedListItemIndex;
      if (iItem > -1)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
      }
      UpdateButtonStates();
    }

    protected virtual void UpdateButtonStates()
    {
      GUIPropertyManager.SetProperty("#view", handler.LocalizedCurrentView);
      if (GetID == (int)GUIWindow.Window.WINDOW_MUSIC_GENRE)
      {
        GUIPropertyManager.SetProperty("#currentmodule", String.Format("{0}/{1}", GUILocalizeStrings.Get(100005), handler.LocalizedCurrentView));
      }
      else
      {
        GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100000 + GetID));
      }

      facadeView.IsVisible = false;
      facadeView.IsVisible = true;
      GUIControl.FocusControl(GetID, facadeView.GetID);

      string strLine = String.Empty;
      View view = CurrentView;
      switch (view)
      {
        case View.List:
          strLine = GUILocalizeStrings.Get(101);
          break;
        case View.Icons:
          strLine = GUILocalizeStrings.Get(100);
          break;
        case View.LargeIcons:
          strLine = GUILocalizeStrings.Get(417);
          break;
        case View.Albums:
          strLine = GUILocalizeStrings.Get(529);
          break;
        case View.FilmStrip:
          strLine = GUILocalizeStrings.Get(733);
          break;
        case View.PlayList:
          strLine = GUILocalizeStrings.Get(101);
          break;
      }
      btnViewAs.Label = strLine;

      switch (CurrentSortMethod)
      {
        case MusicSort.SortMethod.Name:
          strLine = GUILocalizeStrings.Get(103);
          break;
        case MusicSort.SortMethod.Date:
          strLine = GUILocalizeStrings.Get(104);
          break;
        case MusicSort.SortMethod.Size:
          strLine = GUILocalizeStrings.Get(105);
          break;
        case MusicSort.SortMethod.Track:
          strLine = GUILocalizeStrings.Get(266);
          break;
        case MusicSort.SortMethod.Duration:
          strLine = GUILocalizeStrings.Get(267);
          break;
        case MusicSort.SortMethod.Title:
          strLine = GUILocalizeStrings.Get(268);
          break;
        case MusicSort.SortMethod.Artist:
          strLine = GUILocalizeStrings.Get(269);
          break;
        case MusicSort.SortMethod.Album:
          strLine = GUILocalizeStrings.Get(270);
          break;
        case MusicSort.SortMethod.Filename:
          strLine = GUILocalizeStrings.Get(363);
          break;
        case MusicSort.SortMethod.Rating:
          strLine = GUILocalizeStrings.Get(367);
          break;
      }

      if (btnSortBy != null)
      {
        btnSortBy.Label = strLine;
        btnSortBy.IsAscending = CurrentSortAsc;
      }
    }

    protected virtual void OnClick(int item)
    {
    }

    protected virtual void OnQueueItem(int item)
    {
    }

    protected void OnSetRating(int itemNumber)
    {
      GUIListItem item = facadeView[itemNumber];
      if (item == null)
        return;
      MusicTag tag = item.MusicTag as MusicTag;
      GUIDialogSetRating dialog = (GUIDialogSetRating)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_RATING);
      if (tag != null)
      {
        dialog.Rating = tag.Rating;
        dialog.SetTitle(String.Format("{0}-{1}", tag.Artist, tag.Title));
      }
      dialog.FileName = item.Path;
      dialog.DoModal(GetID);
      if (tag != null)
      {
        tag.Rating = dialog.Rating;
      }
      m_database.SetRating(item.Path, dialog.Rating);
      if (dialog.Result == GUIDialogSetRating.ResultCode.Previous)
      {
        while (itemNumber > 0)
        {
          itemNumber--;
          item = facadeView[itemNumber];
          if (!item.IsFolder && !item.IsRemote)
          {
            OnSetRating(itemNumber);
            return;
          }
        }
      }

      if (dialog.Result == GUIDialogSetRating.ResultCode.Next)
      {
        while (itemNumber + 1 < facadeView.Count)
        {
          itemNumber++;
          item = facadeView[itemNumber];
          if (!item.IsFolder && !item.IsRemote)
          {
            OnSetRating(itemNumber);
            return;
          }
        }
      }
    }

    protected override void OnPageLoad()
    {
      // watch if we're still playing a last.fm radio stream
      if (g_Player.Playing)
        if (Util.Utils.IsLastFMStream(g_Player.CurrentFile))
          g_Player.Stop();

      if (m_database == null)      
        m_database = new MusicDatabase();
      
      if (handler == null)      
        handler = new MusicViewHandler();
      
      LoadSettings();

      if (btnSortBy != null)
        btnSortBy.SortChanged += new SortEventHandler(SortChanged);

      base.OnPageLoad();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();

      // Save view
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("music", "startWindow", MusicState.StartWindow.ToString());
        xmlwriter.SetValue("music", "startview", MusicState.View);
      }
      base.OnPageDestroy(newWindowId);
    }

    protected void LoadPlayList(string strPlayList)
    {
      IPlayListIO loader = PlayListFactory.CreateIO(strPlayList);
      if (loader == null)
        return;
      PlayList playlist = new PlayList();

      if (!loader.Load(playlist, strPlayList))
      {
        TellUserSomethingWentWrong();
        return;
      }

      if (_autoShuffleOnLoad)
      {
        Random r = new Random((int)DateTime.Now.Ticks);
        int shuffleCount = r.Next() % 50;
        for (int i = 0; i < shuffleCount; ++i)
        {
          playlist.Shuffle();
        }
      }

      playlistPlayer.CurrentPlaylistName = System.IO.Path.GetFileNameWithoutExtension(strPlayList);
      if (playlist.Count == 1)
      {
        Log.Info("GUIMusic:Play: play single playlist item - {0}", playlist[0].FileName);
        g_Player.Play(playlist[0].FileName);
        return;
      }

      // clear current playlist
      playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Clear();

      Song song = new Song();
      // add each item of the playlist to the playlistplayer
      for (int i = 0; i < playlist.Count; ++i)
      {
        PlayListItem playListItem = playlist[i];
        m_database.GetSongByFileName(playListItem.FileName, ref song);
        MusicTag tag = new MusicTag();
        tag.Album = song.Album;
        tag.Artist = song.Artist;
        tag.Genre = song.Genre;
        tag.Duration = song.Duration;
        tag.Title = song.Title;
        tag.Track = song.Track;
        tag.Rating = song.Rating;
        playListItem.MusicTag = tag;
        playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Add(playListItem);
      }

      // if we got a playlist
      if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Count > 0)
      {
        // then get 1st song
        playlist = playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
        PlayListItem item = playlist[0];

        // and start playing it
        playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
        playlistPlayer.Reset();
        playlistPlayer.Play(0);

        // and activate the playlist window if its not activated yet
        if (GetID == GUIWindowManager.ActiveWindow)
        {
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
        }
      }
    }

    private void TellUserSomethingWentWrong()
    {
      GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (dlgOK != null)
      {
        dlgOK.SetHeading(6);
        dlgOK.SetLine(1, 477);
        dlgOK.SetLine(2, String.Empty);
        dlgOK.DoModal(GetID);
      }
    }

    protected virtual void OnSort()
    {
      SetLabels();
      facadeView.Sort(new MusicSort(CurrentSortMethod, CurrentSortAsc));
      UpdateButtonStates();
    }

    protected virtual void SetLabels()
    {
      MusicSort.SortMethod method = CurrentSortMethod;

      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem item = facadeView[i];
        MusicTag tag = (MusicTag)item.MusicTag;
        if (tag != null)
        {
          string trackNr = String.Format("{0:##00}", tag.Track);
          string fileSize = MediaPortal.Util.Utils.GetSize(item.Size);
          string year = tag.Year.ToString();
          string filename = MediaPortal.Util.Utils.GetFilename(item.Path);
          // For an index view, don't translate the duration
          string duration = "";
          // When in Shares View, the View = null
          if (handler.View != null)
          {
            FilterDefinition filter = (FilterDefinition)handler.View.Filters[handler.CurrentLevel];
            if (filter.SqlOperator == "group")
            {
              duration = Convert.ToString(tag.Duration);
            }
            else
            {
              duration = MediaPortal.Util.Utils.SecondsToHMSString(tag.Duration);
            }
          }
          else
          {
            duration = MediaPortal.Util.Utils.SecondsToHMSString(tag.Duration);
          }
          string rating = tag.Rating.ToString();
          if (tag.Track <= 0)
            trackNr = "";
          if (tag.Year < 1900)
            year = "";

          string date = "";
          if (item.FileInfo != null)
            date = item.FileInfo.ModificationTime.ToShortDateString() + " " + item.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          ;

          string line1 = _sortTags1[(int)method];
          string line2 = _sortTags2[(int)method];
          line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%track%", trackNr);
          line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%track%", trackNr);
          line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%filesize%", fileSize);
          line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%filesize%", fileSize);
          if (handler.View != null)
          {
            FilterDefinition tempfilter = (FilterDefinition)handler.View.Filters[handler.CurrentLevel];
            if (tempfilter.Where == "albumartist")
            {
              line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%artist%", tag.AlbumArtist);
              line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%artist%", tag.AlbumArtist);
            }
            else
            {
              line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%artist%", tag.Artist);
              line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%artist%", tag.Artist);
            }
          }
          else
          {
            line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%artist%", tag.Artist);
            line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%artist%", tag.Artist);
          }
          line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%album%", tag.Album);
          line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%album%", tag.Album);
          line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%title%", tag.Title);
          line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%title%", tag.Title);
          line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%year%", year);
          line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%year%", year);
          line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%filename%", filename);
          line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%filename%", filename);
          line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%rating%", rating);
          line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%rating%", rating);
          line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%duration%", duration);
          line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%duration%", duration);
          line1 = MediaPortal.Util.Utils.ReplaceTag(line1, "%date%", date);
          line2 = MediaPortal.Util.Utils.ReplaceTag(line2, "%date%", date);
          item.Label = line1;
          item.Label2 = line2;
        }
        /*
        if (tag.Title.Length > 0)
        {
          if (tag.Artist.Length > 0)
          {
            if (tag.Track > 0)
              item.Label = String.Format("{0:00}. {1} - {2}", tag.Track, tag.Artist, tag.Title);
            else
              item.Label = String.Format("{0} - {1}", tag.Artist, tag.Title);
          }
          else
          {
            if (tag.Track > 0)
              item.Label = String.Format("{0:00}. {1} ", tag.Track, tag.Title);
            else
              item.Label = String.Format("{0}", tag.Title);
          }
          if (method == MusicSort.SortMethod.Album)
          {
            if (tag.Album.Length > 0 && tag.Title.Length > 0)
            {
              item.Label = String.Format("{0} - {1}", tag.Album, tag.Title);
            }
          }
          if (method == MusicSort.SortMethod.Rating)
          {
            item.Label2 = String.Format("{0}", tag.Rating);
          }
        }
      }


      if (method == MusicSort.SortMethod.Size || method == MusicSort.SortMethod.Filename)
      {
        if (item.IsFolder) item.Label2 = String.Empty;
        else
        {
          if (item.Size > 0)
          {
            item.Label2 = MediaPortal.Util.Utils.GetSize(item.Size);
          }
          if (method == MusicSort.SortMethod.Filename)
          {
            item.Label = MediaPortal.Util.Utils.GetFilename(item.Path);
          }
        }
      }
      else if (method == MusicSort.SortMethod.Date)
      {
        if (item.FileInfo != null)
        {
          item.Label2 = item.FileInfo.ModificationTime.ToShortDateString() + " " + item.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
        }
      }
      else if (method != MusicSort.SortMethod.Rating)
      {
        if (tag != null)
        {
          int nDuration = tag.Duration;
          if (nDuration > 0)
          {
            item.Label2 = MediaPortal.Util.Utils.SecondsToHMSString(nDuration);
          }
        }
      }*/
      }
    }

    protected void SwitchView()
    {
      switch (CurrentView)
      {
        case View.List:
          facadeView.View = GUIFacadeControl.ViewMode.List;
          break;
        case View.Icons:
          facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
          break;
        case View.LargeIcons:
          facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
          break;
        case View.Albums:
          facadeView.View = GUIFacadeControl.ViewMode.AlbumView;
          break;
        case View.FilmStrip:
          facadeView.View = GUIFacadeControl.ViewMode.Filmstrip;
          break;
        case View.PlayList:
          facadeView.View = GUIFacadeControl.ViewMode.Playlist;
          break;
      }

      UpdateButtonStates(); // Ensure "View: xxxx" button label is updated to suit
    }

    protected bool GetKeyboard(ref string strLine)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
        return false;
      keyboard.Reset();
      keyboard.Text = playlistPlayer.CurrentPlaylistName;
      keyboard.DoModal(GetID);
      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
        playlistPlayer.CurrentPlaylistName = keyboard.Text;
        return true;
      }
      return false;
    }

    protected virtual void OnRetrieveCoverArt(GUIListItem item)
    {
      MediaPortal.Util.Utils.SetDefaultIcons(item);
      if (item.Label == "..")
        return;
      MusicTag tag = (MusicTag)item.MusicTag;
      string strThumb = GUIMusicFiles.GetCoverArt(item.IsFolder, item.Path, tag);
      if (strThumb != String.Empty)
      {
        item.ThumbnailImage = strThumb;
        item.IconImageBig = strThumb;
        item.IconImage = strThumb;

        // let us test if there is a larger cover art image
        string strLarge = MediaPortal.Util.Utils.ConvertToLargeCoverArt(strThumb);
        if (System.IO.File.Exists(strLarge))
        {
          item.ThumbnailImage = strLarge;
        }
      }
    }

    protected void OnShowSort()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(495); // Sort options

      dlg.AddLocalizedString(103); // name
      dlg.AddLocalizedString(269); // artist
      dlg.AddLocalizedString(270); // album
      dlg.AddLocalizedString(266); // track
      dlg.AddLocalizedString(268); // title
      dlg.AddLocalizedString(363); // filename
      dlg.AddLocalizedString(367); // rating
      dlg.AddLocalizedString(267); // duration
      dlg.AddLocalizedString(105); // size
      dlg.AddLocalizedString(104); // date

      // !!! this does not work yet, because we need to change
      //       the order of MusicSort.SortMethod items OR
      //       the order which the methods are added to the dialog above
      // set the focus to currently used sort method
      //dlg.SelectedLabel = (int)CurrentSortMethod;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1) return;

      switch (dlg.SelectedId)
      {
        case 103:
          CurrentSortMethod = MusicSort.SortMethod.Name;
          break;
        case 269:
          CurrentSortMethod = MusicSort.SortMethod.Artist;
          break;
        case 270:
          CurrentSortMethod = MusicSort.SortMethod.Album;
          break;
        case 266:
          CurrentSortMethod = MusicSort.SortMethod.Track;
          break;
        case 268:
          CurrentSortMethod = MusicSort.SortMethod.Title;
          break;
        case 363:
          CurrentSortMethod = MusicSort.SortMethod.Filename;
          break;
        case 367:
          CurrentSortMethod = MusicSort.SortMethod.Rating;
          break;
        case 267:
          CurrentSortMethod = MusicSort.SortMethod.Duration;
          break;
        case 105:
          CurrentSortMethod = MusicSort.SortMethod.Size;
          break;
        case 104:
          CurrentSortMethod = MusicSort.SortMethod.Date;
          break;
        default:
          CurrentSortMethod = MusicSort.SortMethod.Name;
          break;
      }
      
      OnSort();
      GUIControl.FocusControl(GetID, btnSortBy.GetID);
    }

    protected void OnShowViews()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(499); // Views menu

      dlg.AddLocalizedString(134); // Shares
      foreach (ViewDefinition view in handler.Views)
      {
        dlg.Add(view.LocalizedName);
      }
      dlg.AddLocalizedString(4540); // Now playing

      // set the focus to currently used view
      if (this.GetID == (int)GUIWindow.Window.WINDOW_MUSIC_FILES)
        dlg.SelectedLabel = 0;
      else if (this.GetID == (int)GUIWindow.Window.WINDOW_MUSIC_GENRE)
        dlg.SelectedLabel = handler.CurrentViewIndex + 1;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1) return;

      switch (dlg.SelectedId)
      {
        case 134: // Shares
          {
            int nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_FILES;
            MusicState.StartWindow = nNewWindow;
            if (nNewWindow != GetID)
            {
              GUIWindowManager.ReplaceWindow(nNewWindow);
            }
          }
          break;

        case 4540: // Now playing
          {
            int nPlayingNowWindow = (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW;

            GUIMusicPlayingNow guiPlayingNow = (GUIMusicPlayingNow)GUIWindowManager.GetWindow(nPlayingNowWindow);

            if (guiPlayingNow != null)
            {
              guiPlayingNow.MusicWindow = this;
              GUIWindowManager.ActivateWindow(nPlayingNowWindow);
            }
          }
          break;

        default: // a db view
          {
            ViewDefinition selectedView = (ViewDefinition)handler.Views[dlg.SelectedLabel - 1];
            handler.CurrentView = selectedView.Name;
            MusicState.View = selectedView.Name;
            int nNewWindow = (int)GUIWindow.Window.WINDOW_MUSIC_GENRE;
            if (GetID != nNewWindow)
            {
              MusicState.StartWindow = nNewWindow;
              if (nNewWindow != GetID)
              {
                GUIWindowManager.ReplaceWindow(nNewWindow);
              }
            }
            else
            {
              LoadDirectory(String.Empty);
              if (facadeView.Count <= 0)
              {
                GUIControl.FocusControl(GetID, btnViewAs.GetID);
              }
            }
          }
          break;
      }
    }

    protected void OnShowSavedPlaylists(string _directory)
    {
      VirtualDirectory _virtualDirectory = new VirtualDirectory();
      _virtualDirectory.AddExtension(".m3u");
      _virtualDirectory.AddExtension(".pls");
      _virtualDirectory.AddExtension(".b4s");
      _virtualDirectory.AddExtension(".wpl");

      List<GUIListItem> itemlist = _virtualDirectory.GetDirectoryExt(_directory);
      if (_directory == m_strPlayListPath)
        itemlist.RemoveAt(0);

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(983); // Saved Playlists

      foreach (GUIListItem item in itemlist)
      {
        MediaPortal.Util.Utils.SetDefaultIcons(item);
        dlg.Add(item);
      }

      dlg.DoModal(GetID);

      if (dlg.SelectedLabel == -1)
        return;

      GUIListItem selectItem = itemlist[dlg.SelectedLabel];
      if (selectItem.IsFolder)
      {
        OnShowSavedPlaylists(selectItem.Path);
        return;
      }

      GUIWaitCursor.Show();
      LoadPlayList(selectItem.Path);
      GUIWaitCursor.Hide();
    }

    protected virtual void LoadDirectory(string path)
    {
    }

    static public string GetArtistCoverArtName(string artist)
    {
      return MediaPortal.Util.Utils.GetCoverArtName(Thumbs.MusicArtists, artist);
    }

    void OnInfoFile(GUIListItem item)
    {
    }

    void OnInfoFolder(GUIListItem item)
    {

    }

    protected virtual void OnFindCoverArt(int iItem)
    {
    }

    protected virtual void OnInfo(int iItem)
    {
      GUIListItem pItem = facadeView[iItem];
      if (pItem == null) return;
      Song song = pItem.AlbumInfoTag as Song;
      if (song == null)
      {
        if (!pItem.IsFolder)
        {
          if (pItem.Path != String.Empty)
            OnInfoFile(pItem);
        }
        else
        {
          if (pItem.Path != String.Empty)
            OnInfoFolder(pItem);
        }
        facadeView.RefreshCoverArt();
        return;
      }
      else if (song.songId >= 0)
      {
        ShowAlbumInfo(false, song.Artist, song.Album, song.FileName, pItem.MusicTag as MusicTag, song.albumId);
      }
      else if (song.albumId >= 0)
      {
        ShowAlbumInfo(false, song.Artist, song.Album, song.FileName, pItem.MusicTag as MusicTag, song.albumId);
      }
      else if (song.artistId >= 0)
      {
        ShowArtistInfo(song.Artist, song.Album, song.artistId, song.albumId);
      }
      else if (song.albumartistId >= 0)
      {
        song.artistId = m_database.GetArtistId(song.AlbumArtist);
        if (song.artistId != -1)
          ShowArtistInfo(song.AlbumArtist, song.Album, song.artistId, song.albumId);
      }
      facadeView.RefreshCoverArt();

    }

    protected virtual void ShowArtistInfo(string artistName, string albumName, int artistId, int albumId)
    {
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

      // check cache
      bool bSaveDb = true;
      ArtistInfo artistinfo = new ArtistInfo();
      if (m_database.GetArtistInfo(artistName, ref artistinfo))
      {
        List<Song> songs = new List<Song>();
        MusicArtistInfo artist = new MusicArtistInfo();
        artist.Set(artistinfo);

        // ok, show Artist info
        GUIMusicArtistInfo pDlgArtistInfo = (GUIMusicArtistInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_ARTIST_INFO);
        if (null != pDlgArtistInfo)
        {
          pDlgArtistInfo.Artist = artist;
          pDlgArtistInfo.DoModal(GetID);

          if (pDlgArtistInfo.NeedsRefresh)
          {
            m_database.DeleteArtistInfo(artist.Artist);
            ShowArtistInfo(artistName, albumName, artistId, albumId);
            return;
          }
        }
        return;
      }


      if (null != pDlgOK && !Util.Win32API.IsConnectedToInternet())
      {
        pDlgOK.SetHeading(703);
        pDlgOK.SetLine(1, 703);
        pDlgOK.SetLine(2, String.Empty);
        pDlgOK.DoModal(GetID);
        return;
      }
      else if (!Util.Win32API.IsConnectedToInternet())
      {
        return;
      }

      // show dialog box indicating we're searching the artist
      if (dlgProgress != null)
      {
        dlgProgress.Reset();
        dlgProgress.SetHeading(320);
        dlgProgress.SetLine(1, artistName);
        dlgProgress.SetLine(2, String.Empty);
        dlgProgress.SetPercentage(0);
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
        dlgProgress.ShowProgressBar(true);
      }
      bool bDisplayErr = false;

      // find artist info
      AllmusicSiteScraper scraper = new AllmusicSiteScraper();
      if (scraper.FindInfo(AllmusicSiteScraper.SearchBy.Artists, artistName))
      {
        if (dlgProgress != null)
          dlgProgress.Close();
        // did we found at least 1 album?
        if (scraper.IsMultiple())
        {
          //yes
          // if we found more then 1 album, let user choose one
          int iSelectedAlbum = 0;
          string[] artistsFound = scraper.GetItemsFound();
          //show dialog with all albums found
          string szText = GUILocalizeStrings.Get(181);
          GUIDialogSelect pDlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
          if (null != pDlg)
          {
            pDlg.Reset();
            pDlg.SetHeading(szText);
            for (int i = 0; i < artistsFound.Length; ++i)
            {
              pDlg.Add(artistsFound[i]);
            }
            pDlg.DoModal(GetID);

            // and wait till user selects one
            iSelectedAlbum = pDlg.SelectedLabel;
            if (iSelectedAlbum < 0)
              return;
          }

          // ok, now show dialog we're downloading the artist info
          if (null != dlgProgress)
          {
            dlgProgress.Reset();
            dlgProgress.SetHeading(320);
            dlgProgress.SetLine(1, artistName);
            dlgProgress.SetLine(2, String.Empty);
            dlgProgress.SetPercentage(40);
            dlgProgress.StartModal(GetID);
            dlgProgress.ShowProgressBar(true);
            dlgProgress.Progress();
          }

          // download the artist info
          if (scraper.FindInfoByIndex(iSelectedAlbum))
          {
            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(60);
              dlgProgress.Progress();
            }
            MusicArtistInfo artistInfo = new MusicArtistInfo();
            if (artistInfo.Parse(scraper.GetHtmlContent()))
            {
              if (null != dlgProgress)
              {
                dlgProgress.SetPercentage(80);
                dlgProgress.Progress();
              }
              // if the artist selected from allmusic.com does not match
              // the one from the file, override the one from the allmusic
              // with the one from the file so the info is correct in the
              // database...
              if (!artistInfo.Artist.Equals(artistName))
                artistInfo.Artist = artistName;

              if (bSaveDb)
              {
                m_database.AddArtistInfo(artistInfo.Get());
              }
              if (null != dlgProgress)
              {
                dlgProgress.SetPercentage(100);
                dlgProgress.Progress();
                dlgProgress.Close();
                dlgProgress = null;
              }

              // ok, show Artist info
              GUIMusicArtistInfo pDlgArtistInfo = (GUIMusicArtistInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_ARTIST_INFO);
              if (null != pDlgArtistInfo)
              {
                pDlgArtistInfo.Artist = artistInfo;
                pDlgArtistInfo.DoModal(GetID);

                if (pDlgArtistInfo.NeedsRefresh)
                {
                  m_database.DeleteArtistInfo(artistInfo.Artist);
                  ShowArtistInfo(artistName, albumName, artistId, albumId);
                  return;
                }
              }
            }
          }

          if (null != dlgProgress)
            dlgProgress.Close();
        }
        else // single
        {
          if (null != dlgProgress)
          {
            dlgProgress.SetPercentage(40);
            dlgProgress.Progress();
          }
          MusicArtistInfo artistInfo = new MusicArtistInfo();
          if (artistInfo.Parse(scraper.GetHtmlContent()))
          {

            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(60);
              dlgProgress.Progress();
            }
            // if the artist selected from allmusic.com does not match
            // the one from the file, override the one from the allmusic
            // with the one from the file so the info is correct in the
            // database...
            if (!artistInfo.Artist.Equals(artistName))
              artistInfo.Artist = artistName;

            if (bSaveDb)
            {
              // save to database
              m_database.AddArtistInfo(artistInfo.Get());
            }

            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(100);
              dlgProgress.Progress();
              dlgProgress.Close();
              dlgProgress = null;
            }
            // ok, show Artist info
            GUIMusicArtistInfo pDlgArtistInfo = (GUIMusicArtistInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_ARTIST_INFO);
            if (null != pDlgArtistInfo)
            {
              pDlgArtistInfo.Artist = artistInfo;
              pDlgArtistInfo.DoModal(GetID);

              if (pDlgArtistInfo.NeedsRefresh)
              {
                m_database.DeleteArtistInfo(artistInfo.Artist);
                ShowArtistInfo(artistName, albumName, artistId, albumId);
                return;
              }
            }
          }
        }
      }
      else
      {
        // unable 2 connect to www.allmusic.com
        bDisplayErr = true;
      }
      // if an error occured, then notice the user
      if (bDisplayErr)
      {
        if (null != dlgProgress)
          dlgProgress.Close();
        if (null != pDlgOK)
        {
          pDlgOK.SetHeading(702);
          pDlgOK.SetLine(1, 702);
          pDlgOK.SetLine(2, String.Empty);
          pDlgOK.DoModal(GetID);
        }
      }
    }

    public void FindCoverArt(bool isFolder, string artistName, string albumName, string strPath, MusicTag tag, int albumId)
    {
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);

      if (null != pDlgOK && !Util.Win32API.IsConnectedToInternet())
      {
        pDlgOK.SetHeading(703);
        pDlgOK.SetLine(1, 703);
        pDlgOK.SetLine(2, String.Empty);
        pDlgOK.DoModal(GetID);

        //throw new Exception("no internet");
        return;
      }

      else if (!Util.Win32API.IsConnectedToInternet())
      {
        //throw new Exception("no internet");
        return;
      }

      bool bDisplayErr = false;
      GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      AlbumInfo albuminfo = new AlbumInfo();
      MusicAlbumInfo album = new MusicAlbumInfo();

      GUICoverArtGrabberResults guiCoverGrabberResults = (GUICoverArtGrabberResults)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_COVERART_GRABBER_RESULTS);

      if (null != guiCoverGrabberResults)
      {
        guiCoverGrabberResults.SearchMode = GUICoverArtGrabberResults.SearchDepthMode.Album;
        GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

        if (dlgProgress != null)
        {
          dlgProgress.Reset();
          dlgProgress.SetHeading(185);
          dlgProgress.SetLine(1, albumName);
          dlgProgress.SetLine(2, artistName);
          dlgProgress.SetLine(3, string.Empty);
          dlgProgress.StartModal(GetID);
        }

        guiCoverGrabberResults.GetAlbumCovers(artistName, albumName, strPath, GetID, true);
        guiCoverGrabberResults.DoModal(GetID);
        albuminfo = guiCoverGrabberResults.SelectedAlbum;

        if (GUICoverArtGrabberResults.CancelledByUser)
        {
          string line1Text = GUILocalizeStrings.Get(4507);

          if (line1Text.Length == 0)
            line1Text = "Cover art grabber aborted by user";

          string caption = GUILocalizeStrings.Get(4511);

          if (caption.Length == 0)
            caption = "Cover Art Grabber Done";

          if (null != dlgOk)
          {
            dlgOk.SetHeading(caption);
            dlgOk.SetLine(1, line1Text);
            dlgOk.SetLine(2, String.Empty);
            dlgOk.DoModal(GetID);
          }
        }

        else if (albuminfo != null)
        {
          // the GUICoverArtGrabberResults::SelectedAlbum AlbumInfo object contains 
          // the Artist and Album name returned by the Amazon Webservice which may not
          // match our original artist and album.  We want to use the original artist
          // and album name...

          albuminfo.Artist = artistName;
          albuminfo.Album = albumName;
          SaveCoverArtImage(albuminfo, strPath, true, true);
          facadeView.RefreshCoverArt();
        }

        else
          bDisplayErr = true;
      }

      if (bDisplayErr)
      {
        if (null != dlgOk)
        {
          dlgOk.SetHeading(187);
          dlgOk.SetLine(1, 187);
          dlgOk.SetLine(2, String.Empty);
          dlgOk.DoModal(GetID);
        }
      }
    }

    protected void ShowAlbumInfo(bool isFolder, string artistName, string albumName, string strPath, MusicTag tag, int albumId)
    {
      ShowAlbumInfo(GetID, isFolder, artistName, albumName, strPath, tag, albumId);
    }

    public void ShowAlbumInfo(int parentWindowID, bool isFolder, string artistName, string albumName, string strPath, MusicTag tag, int albumId)
    {
      // check cache
      GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      AlbumInfo albuminfo = new AlbumInfo();
      if (m_database.GetAlbumInfo(albumId, ref albuminfo))
      {
        List<Song> songs = new List<Song>();
        MusicAlbumInfo album = new MusicAlbumInfo();
        album.Set(albuminfo);

        GUIMusicInfo pDlgAlbumInfo = (GUIMusicInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_INFO);
        if (null != pDlgAlbumInfo)
        {
          pDlgAlbumInfo.Album = album;
          pDlgAlbumInfo.Tag = tag;

          //pDlgAlbumInfo.DoModal(GetID);
          pDlgAlbumInfo.DoModal(parentWindowID);
          if (pDlgAlbumInfo.NeedsRefresh)
          {
            m_database.DeleteAlbumInfo(albumName);
            ShowAlbumInfo(isFolder, artistName, albumName, strPath, tag, albumId);
          }
          return;
        }
      }

      // show dialog box indicating we're searching the album
      if (dlgProgress != null)
      {
        dlgProgress.Reset();
        dlgProgress.SetHeading(185);
        dlgProgress.SetLine(1, albumName);
        dlgProgress.SetLine(2, artistName);
        dlgProgress.SetLine(3, tag.Year.ToString());
        dlgProgress.SetPercentage(0);
        //dlgProgress.StartModal(GetID);
        dlgProgress.StartModal(parentWindowID);
        dlgProgress.ShowProgressBar(true);
        dlgProgress.Progress();
      }
      bool bDisplayErr = false;

      // find album info
      MusicInfoScraper scraper = new MusicInfoScraper();
      if (scraper.FindAlbuminfo(albumName, artistName, tag.Year))
      {
        if (dlgProgress != null)
        {
          dlgProgress.SetPercentage(30);
          dlgProgress.Progress();
          dlgProgress.Close();
        }
        // did we found at least 1 album?
        int iAlbumCount = scraper.Count;
        if (iAlbumCount >= 1)
        {
          //yes
          // if we found more then 1 album, let user choose one
          int iSelectedAlbum = 0;
          if (iAlbumCount > 1)
          {
            //show dialog with all albums found
            string szText = GUILocalizeStrings.Get(181);
            GUIDialogSelect pDlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
            if (null != pDlg)
            {
              pDlg.Reset();
              pDlg.SetHeading(szText);
              for (int i = 0; i < iAlbumCount; ++i)
              {
                MusicAlbumInfo info = scraper[i];
                pDlg.Add(info.Title2);
              }
              //pDlg.DoModal(GetID);
              pDlg.DoModal(parentWindowID);

              // and wait till user selects one
              iSelectedAlbum = pDlg.SelectedLabel;
              if (iSelectedAlbum < 0)
                return;
            }
          }

          // ok, now show dialog we're downloading the album info
          MusicAlbumInfo album = scraper[iSelectedAlbum];
          if (null != dlgProgress)
          {
            dlgProgress.Reset();
            dlgProgress.SetHeading(185);
            dlgProgress.SetLine(1, album.Title2);
            dlgProgress.SetLine(2, album.Artist);
            //dlgProgress.StartModal(GetID);
            dlgProgress.StartModal(parentWindowID);
            dlgProgress.ShowProgressBar(true);
            dlgProgress.SetPercentage(40);
            dlgProgress.Progress();
          }

          // download the album info
          bool bLoaded = album.Loaded;
          if (!bLoaded)
            bLoaded = album.Load();
          if (null != dlgProgress)
          {
            dlgProgress.SetPercentage(70);
            dlgProgress.Progress();
          }
          if (bLoaded)
          {
            // set album title from musicinfotag, not the one we got from allmusic.com
            album.Title = albumName;
            // set path, needed to store album in database
            album.AlbumPath = strPath;
            albuminfo = new AlbumInfo();
            albuminfo.Album = album.Title;
            albuminfo.Artist = album.Artist;
            albuminfo.Genre = album.Genre;
            albuminfo.Tones = album.Tones;
            albuminfo.Styles = album.Styles;
            albuminfo.Review = album.Review;
            albuminfo.Image = album.ImageURL;
            albuminfo.Rating = album.Rating;
            albuminfo.Tracks = album.Tracks;
            try
            {
              albuminfo.Year = Int32.Parse(album.DateOfRelease);
            }
            catch (Exception)
            {
            }
            //albuminfo.Path   = album.AlbumPath;
            // save to database
            m_database.AddAlbumInfo(albuminfo);
            if (null != dlgProgress)
            {
              dlgProgress.SetPercentage(100);
              dlgProgress.Progress();
              dlgProgress.Close();
            }

            // ok, show album info
            GUIMusicInfo pDlgAlbumInfo = (GUIMusicInfo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_INFO);
            if (null != pDlgAlbumInfo)
            {
              pDlgAlbumInfo.Album = album;
              pDlgAlbumInfo.Tag = tag;

              //pDlgAlbumInfo.DoModal(GetID);
              pDlgAlbumInfo.DoModal(parentWindowID);
              if (pDlgAlbumInfo.NeedsRefresh)
              {
                m_database.DeleteAlbumInfo(album.Title);
                ShowAlbumInfo(isFolder, artistName, albumName, strPath, tag, albumId);
                return;
              }
              if (isFolder)
              {
                string thumb = GetAlbumThumbName(album.Artist, album.Title);
                if (System.IO.File.Exists(thumb))
                {
                  try
                  {
                    string folderjpg = String.Format(@"{0}\folder.jpg", MediaPortal.Util.Utils.RemoveTrailingSlash(strPath));
                    MediaPortal.Util.Utils.FileDelete(folderjpg);
                    System.IO.File.Copy(thumb, folderjpg);
                  }
                  catch (Exception)
                  {
                  }
                }
              }
            }
          }
          else
          {
            // failed 2 download album info
            bDisplayErr = true;
          }
        }
        else
        {
          // no albums found
          bDisplayErr = true;
        }
      }
      else
      {
        // unable 2 connect to www.allmusic.com
        bDisplayErr = true;
      }
      // if an error occured, then notice the user
      if (bDisplayErr)
      {
        if (null != dlgProgress)
          dlgProgress.Close();
        if (null != dlgOk)
        {
          dlgOk.SetHeading(187);
          dlgOk.SetLine(1, 187);
          dlgOk.SetLine(2, String.Empty);
          //dlgOk.DoModal(GetID);
          dlgOk.DoModal(parentWindowID);
        }
      }
    }

    protected bool SaveCoverArtImage(AlbumInfo albumInfo, string albumFolderPath, bool bSaveToAlbumFolder, bool bSaveToThumbsFolder)
    {
      bool result = false;
      bool isCdOrDVD = MediaPortal.Util.Utils.IsDVD(albumFolderPath);

      try
      {
        System.Drawing.Image coverImg = MediaPortal.Music.Amazon.AmazonWebservice.GetImageFromURL(albumInfo.Image);
        string thumbPath = GetAlbumThumbName(albumInfo.Artist, albumInfo.Album);

        if (thumbPath.Length == 0 || coverImg == null)
          return false;

        //if (bSaveToThumbsFolder)
        if (bSaveToAlbumFolder && !isCdOrDVD)
        {
          string folderjpg = String.Format(@"{0}\folder.jpg", MediaPortal.Util.Utils.RemoveTrailingSlash(albumFolderPath));

          if (System.IO.File.Exists(folderjpg))
            System.IO.File.Delete(folderjpg);

          coverImg.Save(folderjpg);
          result = true;
        }

        if (bSaveToThumbsFolder || isCdOrDVD)
        {
          if (System.IO.File.Exists(thumbPath))
            System.IO.File.Delete(thumbPath);

          coverImg.Save(thumbPath);
          result = true;
        }
      }

      catch
      {
        result = false;
      }

      return result;
    }

    public static bool CoverArtExists(string ArtistName, string AlbumName, string albumPath, bool checkAlbumFolder)
    {
      if (ArtistName.Length == 0 || AlbumName.Length == 0)
        return false;

      bool bHasThumbnailImage = System.IO.File.Exists(GUIMusicBaseWindow.GetAlbumThumbName(ArtistName, AlbumName));

      if (!checkAlbumFolder)
        return bHasThumbnailImage;

      string path = System.IO.Path.GetDirectoryName(albumPath);
      bool bHasAlbumFolderImage = System.IO.File.Exists(System.IO.Path.Combine(path, "folder.jpg"));

      return bHasThumbnailImage && bHasAlbumFolderImage;
    }

    static public string GetAlbumThumbName(string ArtistName, string AlbumName)
    {
      if (ArtistName == String.Empty)
        return String.Empty;
      if (AlbumName == String.Empty)
        return String.Empty;
      Util.Utils.RemoveStackEndings(ref AlbumName);

      string name = String.Format("{0}-{1}", ArtistName, AlbumName);
      return MediaPortal.Util.Utils.GetCoverArtName(Thumbs.MusicAlbum, name);
    }

    protected virtual void AddSongToFavorites(GUIListItem item)
    {
      Song song = item.AlbumInfoTag as Song;
      if (song == null)
        return;
      if (song.songId < 0)
        return;
      song.Favorite = true;
      m_database.SetFavorite(song);
    }

    void SortChanged(object sender, SortEventArgs args)
    {
      this.CurrentSortAsc = args.Order != System.Windows.Forms.SortOrder.Descending;

      OnSort();
      UpdateButtonStates();
    }

    protected virtual bool IsSortableView(ViewDefinition view, int viewLevel)
    {
      if (view == null || viewLevel < 0 || viewLevel >= view.Filters.Count)
        return false;

      string sWhere = ((FilterDefinition)view.Filters[viewLevel]).Where;

      if (sWhere.Length == 0)
        return true;

      switch (sWhere.ToLower())
      {
        case "timesplayed":
          return false;

        default:
          return true;
      }
    }
    //SV Added by SteveV 2006-09-07
    protected bool DoPlayNowJumpTo(int playlistItemCount)
    {
      switch (PlayNowJumpTo)
      {
        case PlayNowJumpToType.NowPlayingAlways:
          {
            if (playlistItemCount < 1)
              return false;

            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW);
            return true;
          }

        case PlayNowJumpToType.NowPlayingMultipleItems:
          {
            if (playlistItemCount <= 1)
              return false;

            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW);
            return true;
          }

        case PlayNowJumpToType.CurrentPlaylistAlways:
          {
            if (playlistItemCount < 1)
              return false;

            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
            return true;
          }

        case PlayNowJumpToType.CurrentPlaylistMultipleItems:
          {
            if (playlistItemCount <= 1)
              return false;

            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MUSIC_PLAYLIST);
            return true;
          }

        case PlayNowJumpToType.FullscreenIfVizEnabledAlways:
          {
            if (playlistItemCount < 1)
              return false;

            if (!UsingInternalMusicPlayer || !g_Player.IsMusic || !g_Player.Playing)
              return false;

            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_MUSIC);
            GUIGraphicsContext.IsFullScreenVideo = true;
            return true;
          }

        case PlayNowJumpToType.FullscreenIfVizEnabledMultipleItems:
          {
            if (playlistItemCount <= 1)
              return false;

            if (!UsingInternalMusicPlayer || !g_Player.IsMusic || !g_Player.Playing)
              return false;

            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_MUSIC);
            GUIGraphicsContext.IsFullScreenVideo = true;
            return true;
          }

        default:
          return false;
      }
    }
  }
}