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
using System.Collections;
using System.Collections.Generic;

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.MusicVideos.Database;

namespace MediaPortal.GUI.MusicVideos
{
  public class GUIMusicVideos : GUIWindow, ISetupForm, IShowPlugin
  {
    #region SkinControlAttributes
    [SkinControlAttribute(2)]     protected GUIButtonControl btnTop = null;
    [SkinControlAttribute(7)]     protected GUIButtonControl btnNew = null;
    [SkinControlAttribute(8)]     protected GUIButtonControl btnPlayAll = null;
    [SkinControlAttribute(3)]     protected GUIButtonControl btnSearch = null;
    [SkinControlAttribute(6)]     protected GUIButtonControl btnFavorites = null;
    [SkinControlAttribute(9)]     protected GUIButtonControl btnBack = null;
    [SkinControlAttribute(25)]    protected GUIButtonControl btnPlayList = null;
    [SkinControlAttribute(34)]    protected GUIButtonControl btnNextPage = null;
    [SkinControlAttribute(35)]    protected GUIButtonControl btnPreviousPage = null;
    [SkinControlAttribute(36)]    protected GUIImage imgCountry = null;
    [SkinControlAttribute(37)]    protected GUIButtonControl btnGenre = null;
    [SkinControlAttribute(38)]    protected GUIButtonControl btnCountry = null;

    [SkinControlAttribute(50)]    protected GUIListControl listSongs = null;

    #endregion

    #region Enumerations
    enum State
    {
      HOME = -1,
      TOP = 0,
      SEARCH = 1,
      FAVORITE = 2,
      NEW = 3,
      GENRE = 4
    };

    #endregion

    #region variables
    private int WINDOW_ID = 4734;
    private YahooSettings moSettings;
    private YahooTopVideos moTopVideos;
    private YahooNewVideos moNewVideos;
    private YahooSearch moYahooSearch;
    private YahooFavorites moFavoriteManager;
    private YahooGenres moGenre;
    private int CURRENT_STATE = (int)State.HOME;
    private int miSelectedIndex = 0;
    private YahooVideo moCurrentPlayingVideo;
    private string msSelectedGenre;
    #endregion

    public GUIMusicVideos()
    {
    }
    
    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string PluginName()
    {
      return "My Music Videos ";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public bool HasSetup()
    {
      return true;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = GUILocalizeStrings.Get(30000);// "My MusicVideos";
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "hover_musicvideo.png";
      return true;
    }

    public string Author()
    {
      return "Gregmac45";
    }

    public string Description()
    {
      return "This plugin shows online music videos from Yahoo";
    }

    public bool ShowDefaultHome()
    {
      return false;
    }

    public void ShowPlugin() // show the setup dialog
    {
      System.Windows.Forms.Form setup = new SetupForm();
      setup.ShowDialog();
    }

    public int GetWindowId()
    {
      return GetID;
    }

    #endregion
    
    #region GUIWindow Overrides

    public override int GetID
    {
      get { return WINDOW_ID; }
      set { base.GetID = value; }
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\mymusicvideos.xml");
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      //labelState.Label = "";
      base.OnPageDestroy(new_windowId);
    }

    public override void OnAction(Action action)
    {
      if (action.wID != Action.ActionType.ACTION_MOUSE_MOVE)
      {
        Log.Info("action wID = {0}", action.wID);
      }
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU && CURRENT_STATE != (int)State.HOME)
      {
        CURRENT_STATE = (int)State.HOME;
        updateButtonStates();
        this.LooseFocus();
        btnTop.Focus = true;
        return;
      }

      base.OnAction(action);
    }
    public override bool OnMessage(GUIMessage message)
    {
      if (GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED != message.Message
          && GUIMessage.MessageType.GUI_MSG_SETFOCUS != message.Message)
      {
        Log.Info("Message = {0}", message.Message);
      }

      if (GUIMessage.MessageType.GUI_MSG_SETFOCUS == message.Message)
      {
        if (message.TargetControlId == listSongs.GetID && listSongs.Count > 0)
        {
          ////labelSelected.Label = "Press Menu or F9 for more options.";
          // todo : show current title here...
          //GUIPropertyManager.SetProperty("#title", listSongs.SelectedItem);
        }
        else
        {
          //labelSelected.Label = "";
        }
      }

      return base.OnMessage(message);
    }
    protected override void OnPreviousWindow()
    {
      if (g_Player.Playing)
      {
        Log.Info("in OnPreviousWindow and g_player is playing");

        if (moCurrentPlayingVideo != null) 
          GUIPropertyManager.SetProperty("#Play.Current.Title", moCurrentPlayingVideo.artistName + "-" + moCurrentPlayingVideo.songName);
      }
      base.OnPreviousWindow();
    }
    protected override void OnPageLoad()
    {
      if (moSettings == null)
      {
        moSettings = YahooSettings.getInstance();
      }
      Log.Info("Image filename = '{0}'", imgCountry.FileName);
      if (String.IsNullOrEmpty(imgCountry.FileName))
      {
        Log.Info("Updating country image");
        YahooUtil loUtil = YahooUtil.getInstance();
        string lsCountryId = loUtil.getYahooSite(moSettings.msDefaultCountryName).countryId;
        Log.Info("country image -country id = {0}", lsCountryId);
        string countryFlag = GUIGraphicsContext.Skin + @"\media\" + lsCountryId + ".png";
        if (System.IO.File.Exists(countryFlag))
          imgCountry.SetFileName(countryFlag);
      }

      if (CURRENT_STATE == (int)State.HOME)
      {
        updateButtonStates();
        this.LooseFocus();
        btnTop.Focus = true;
      }
      else
      {
        if (CURRENT_STATE == (int)State.TOP)
        { 
          refreshStage2Screen();
        }
        else if (CURRENT_STATE == (int)State.NEW)
        {
          refreshStage2Screen();
        }
        else if (CURRENT_STATE == (int)State.FAVORITE)
        { 
          refreshStage2Screen();
        }
        else if (CURRENT_STATE == (int)State.SEARCH)
        {
          refreshStage2Screen();
        }
        else if (CURRENT_STATE == (int)State.GENRE)
        {
         refreshStage2Screen();
        }
        this.LooseFocus();
        listSongs.Focus = true;

      }
      if (g_Player.Playing)
      {
        if (moCurrentPlayingVideo != null)
        {
          GUIPropertyManager.SetProperty("#Play.Current.Title", moCurrentPlayingVideo.artistName + " - " + moCurrentPlayingVideo.songName);
        }
      }

    }
    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      Log.Info("GUIMusicVideo: Clicked");
      if (actionType == Action.ActionType.ACTION_QUEUE_ITEM)
      {
        Log.Info("Caught on Queue action for list item {0}", listSongs.SelectedListItemIndex);
        OnQueueItem();
        return;
      }
      if (control == listSongs)
      {
        miSelectedIndex = listSongs.SelectedListItemIndex;
        playVideo(getSelectedVideo());
      }
      else if (control == btnTop)
      {
        onClickTopVideos();
      }
      else if (control == btnNew)
      {
        onClickNewVideos();
      }
      else if (control == btnSearch)
      {
        SearchVideos(true, string.Empty);
      }
      else if (control == btnFavorites)
      {
        onClickFavorites();
      }
      else if (control == btnGenre)
      {
        onClickGenre();
      }
      else if (control == btnBack)
      {
        CURRENT_STATE = (int)State.HOME;
        updateButtonStates();
        this.LooseFocus();
        btnTop.Focus = true;
      }

      else if (control == btnPlayAll)
      {
        OnQueueAllItems(getStateVideoList());
      }
      else if (control == btnPlayList)
      {
        onClickPlaylist();
      }
     
      else if (control == btnNextPage)
      {
        OnClickNextPage();
      }
      else if (control == btnPreviousPage)
      {
        OnClickPreviousPage();
      }
      else if (control == btnCountry)
      {
        onClickChangeCountry();
      }
    }
    public override void Process()
    {
      base.Process();
    }
    protected override void OnShowContextMenu()
    {
      YahooVideo loVideo = getSelectedVideo();
      if (loVideo == null)
      {
        return;
      }
      GUIListItem loSelectVideo = listSongs.SelectedListItem;
      int liSelectedIndex = listSongs.SelectedListItemIndex;
      if (liSelectedIndex > -1)
      {
        GUIDialogMenu dlgSel = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        dlgSel.Reset();
        if (dlgSel != null)
        {
          dlgSel.SetHeading(GUILocalizeStrings.Get(498)); // Menu

          dlgSel.Add(GUILocalizeStrings.Get(208)); // Play
          dlgSel.Add(GUILocalizeStrings.Get(926)); // Add to playList
          if ((int)State.FAVORITE == CURRENT_STATE)
            dlgSel.Add(GUILocalizeStrings.Get(933)); // Remove from favorites
          else
            dlgSel.Add(GUILocalizeStrings.Get(930)); // Add to favorites
          dlgSel.Add(GUILocalizeStrings.Get(30007)); // Search other videos by this artist
          
          dlgSel.DoModal(GetID);
          int liSelectedIdx = dlgSel.SelectedId;
          Log.Info("you selected action :{0}", liSelectedIdx);
          switch (liSelectedIdx)
          {
            case 1:
              playVideo(loVideo);
              break;
            case 2:
              OnQueueItem();
              break;
            case 3:
              if (CURRENT_STATE == (int)State.FAVORITE)
              {
                moFavoriteManager.removeFavorite(loVideo);
                DisplayVideoList(moFavoriteManager.getFavoriteVideos());
              }
              else
              {
                //prompt user for favorite list to add to
                string lsSelectedFav = promptForFavoriteList();
                Log.Info("adding to favorites.");
                if (moFavoriteManager == null)
                {
                  moFavoriteManager = new YahooFavorites();
                }
                moFavoriteManager.setSelectedFavorite(lsSelectedFav);
                moFavoriteManager.addFavorite(loVideo);
              }
              break;
            case 4:
              SearchVideos(false, loVideo.artistName);
              break;
          }
        }
      }
    }
    #endregion

    #region userdefined methods
    private void onClickFavorites()
    {
      if (moFavoriteManager == null)
      {
        moFavoriteManager = new YahooFavorites();
      }

      string lsSelectedFav = promptForFavoriteList();
      if (String.IsNullOrEmpty(lsSelectedFav))
      {
        return;
      }

      CURRENT_STATE = (int)State.FAVORITE;


      if (lsSelectedFav != null || lsSelectedFav.Length > 0)
      {
        moFavoriteManager.setSelectedFavorite(lsSelectedFav);
      }
      DisplayVideoList(moFavoriteManager.getFavoriteVideos());
      //labelState.Label = (GUILocalizeStrings.Get(932) + " - " + moFavoriteManager.getSelectedFavorite());
      updateButtonStates();
      if (listSongs.Count == 0)
      {
        this.LooseFocus();
        btnBack.Focus = true;
      }
    }

    private void onClickPlaylist()
    {
      if (GetID == GUIWindowManager.ActiveWindow)
      {
        GUIWindowManager.ActivateWindow(4735);
      }
    }

    private string promptForGenre()
    {
      string lsSelectedGenre = "";
      moGenre = new YahooGenres();
      ArrayList loGenreNames = moGenre.moSortedGenreList;

      GUIDialogMenu dlgSel = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      dlgSel.Reset();
      if (dlgSel != null)
      {
        foreach (string lsGenreNm in loGenreNames)        
          dlgSel.Add(lsGenreNm);
        
        dlgSel.SetHeading(GUILocalizeStrings.Get(496)); // Menu
        dlgSel.DoModal(GetID);
        if (dlgSel.SelectedLabel == -1)
          return "";
        
        Log.Info("you selected genre :{0}", dlgSel.SelectedLabelText);
        lsSelectedGenre = dlgSel.SelectedLabelText;
      }
      return lsSelectedGenre;
    }

    private string promptForFavoriteList()
    {

      string lsSelectedFav = "";
      if (moFavoriteManager == null)      
        moFavoriteManager = new YahooFavorites();
      
      ArrayList loFavNames = moFavoriteManager.getFavoriteNames();
      if (loFavNames.Count > 1)
      {
        GUIDialogMenu dlgSel = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        dlgSel.Reset();
        if (dlgSel != null)
        {
          foreach (string lsFavNm in loFavNames)          
            dlgSel.Add(lsFavNm);
          
          dlgSel.SetHeading(GUILocalizeStrings.Get(497)); // Menu
          dlgSel.DoModal(GetID);
          if (dlgSel.SelectedLabel == -1)          
            return "";
          
          Log.Info("you selected favorite :{0}", dlgSel.SelectedLabelText);
          lsSelectedFav = dlgSel.SelectedLabelText;
        }
      }
      else      
        lsSelectedFav = (string)loFavNames[0];
      
      return lsSelectedFav;
    }

    private void onClickChangeCountry()
    {
      GUIDialogMenu dlgSel = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      dlgSel.Reset();
      if (dlgSel != null)
      {
        String[] loCountryArray = new String[moSettings.moYahooSiteTable.Keys.Count];
        moSettings.moYahooSiteTable.Keys.CopyTo(loCountryArray, 0);
        Array.Sort(loCountryArray);

        foreach (string country in loCountryArray)
          dlgSel.Add(country);
        
        dlgSel.SetHeading(GUILocalizeStrings.Get(496)); // Menu
        dlgSel.DoModal(GetID);
        if (dlgSel.SelectedLabel == -1)        
          return;
        
        Log.Info("you selected country :{0}", dlgSel.SelectedLabelText);
        moSettings.msDefaultCountryName = dlgSel.SelectedLabelText;
        moYahooSearch = new YahooSearch(moSettings.msDefaultCountryName);
        moTopVideos = new YahooTopVideos(moSettings.msDefaultCountryName);
        RefreshPage();
      }
    }

    private void onClickNewVideos()
    {
      miSelectedIndex = 0;

      CURRENT_STATE = (int)State.NEW;
      Log.Info("button new clicked");
      if (moNewVideos == null)
      {
        moNewVideos = new YahooNewVideos();
      }
      moNewVideos.loadNewVideos(moSettings.msDefaultCountryName);
      Log.Info("The new video page has next video ={0}", moNewVideos.hasNext());
      if (moNewVideos.hasNext())
        btnNextPage.Disabled = false;
      else
        btnNextPage.Disabled = true;

      btnPreviousPage.Disabled = true;
      refreshStage2Screen();
    }

    private void onClickGenre()
    {
      miSelectedIndex = 0;
      if (moGenre == null)
        moGenre = new YahooGenres();

      msSelectedGenre = promptForGenre();
      if (String.IsNullOrEmpty(msSelectedGenre))
      {
        return;
      }
      CURRENT_STATE = (int)State.GENRE;

      Log.Info("button GENRE clicked");

      moGenre.loadFirstGenreVideos(msSelectedGenre);

      if (moGenre.hasNext())
        btnNextPage.Disabled = false;
      else
        btnNextPage.Disabled = true;

      btnPreviousPage.Disabled = true;
      refreshStage2Screen();
    }

    private void OnQueueItem()
    {

      PlayListPlayer loPlaylistPlayer = PlayListPlayer.SingletonPlayer;
      loPlaylistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;
      PlayList loPlaylist = loPlaylistPlayer.GetPlaylist(loPlaylistPlayer.CurrentPlaylistType);
      MVPlayListItem loItem;
      YahooVideo loVideo = getSelectedVideo();


      loItem = new MVPlayListItem();
      loItem.YahooVideo = loVideo;
      loPlaylist.Add(loItem);


      if (listSongs.SelectedListItemIndex + 1 < listSongs.Count)
      {
        listSongs.SelectedListItemIndex = listSongs.SelectedListItemIndex + 1;
      }
      //}
    }

    private void OnQueueAllItems(List<YahooVideo> foVideoList)
    {
      Log.Info("in Onqueue All");
      PlayListPlayer loPlaylistPlayer = PlayListPlayer.SingletonPlayer;
      loPlaylistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC_VIDEO;
      PlayList loPlaylist = loPlaylistPlayer.GetPlaylist(loPlaylistPlayer.CurrentPlaylistType);
      MVPlayListItem loItem;
      foreach (YahooVideo loVideo in foVideoList)
      {
        loItem = new MVPlayListItem();
        loItem.YahooVideo = loVideo;
        loItem.Description = loVideo.artistName + "-" + loVideo.songName;
        loPlaylist.Add(loItem);
      }
      Log.Info("current playlist type:{0}", loPlaylistPlayer.CurrentPlaylistType);
      Log.Info("playlist count:{0}", loPlaylistPlayer.GetPlaylist(loPlaylistPlayer.CurrentPlaylistType));

      onClickPlaylist();
      loPlaylistPlayer.PlayNext();
    }


    private void SearchVideos(bool fbClicked, String fsSearchTxt)
    {

      CURRENT_STATE = (int)State.SEARCH;
      if (moYahooSearch == null)
      {
        moYahooSearch = new YahooSearch(moSettings.msDefaultCountryName);
      }

      //clear the list
      listSongs.Clear();
      if (fbClicked)
      {
        VirtualKeyboard keyBoard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
        if (null == keyBoard)
          return;
        keyBoard.Reset();
        keyBoard.DoModal(GUIWindowManager.ActiveWindow);

        if (!keyBoard.IsConfirmed)
          return;
        moYahooSearch.searchVideos(keyBoard.Text);
      }
      else
      {
        moYahooSearch.searchVideos(fsSearchTxt);
      }
      DisplayVideoList(moYahooSearch.moLastSearchResult);

      btnNextPage.Disabled = !moYahooSearch.hasNext();
      btnPreviousPage.Disabled = true;
      updateButtonStates();
    }

    private void onClickTopVideos()
    {
      CURRENT_STATE = (int)State.TOP;
      miSelectedIndex = 0;
      if (moTopVideos == null)
      {
        moTopVideos = new YahooTopVideos(moSettings.msDefaultCountryName);
      }
      moTopVideos.loadFirstPage();
      if (moTopVideos.hasMorePages())
      {
        btnNextPage.Disabled = false;
      }
      else
      {
        btnNextPage.Disabled = true;
      }
      btnPreviousPage.Disabled = true;
      refreshStage2Screen();
    }

    private void OnClickNextPage()
    {
      miSelectedIndex = 0;
      bool lbNext = false;
      bool lbPrevious = false;
      switch (CURRENT_STATE)
      {
        case (int)State.NEW:
          moNewVideos.loadNextVideos(moSettings.msDefaultCountryName);
          lbNext = moNewVideos.hasNext();
          lbPrevious = moNewVideos.hasPrevious();
          DisplayVideoList(moNewVideos.moNewVideoList);
         break;
        case (int)State.TOP:
          moTopVideos.loadNextPage();
          lbNext = moTopVideos.hasMorePages();
          lbPrevious = moTopVideos.hasPreviousPage();
          DisplayVideoList(moTopVideos.getLastLoadedList());
         break;
        case (int)State.SEARCH:
          moYahooSearch.loadNextVideos();
          lbNext = moYahooSearch.hasNext();
          lbPrevious = moYahooSearch.hasPrevious();
          DisplayVideoList(moYahooSearch.moLastSearchResult);
          break;
        case (int)State.GENRE:
          moGenre.loadNextVideos();
          lbNext = moGenre.hasNext();
          lbPrevious = moGenre.hasPrevious();
          DisplayVideoList(moGenre.moGenreVideoList);
         break;
      }
      Log.Info("The video page has next video ={0}", lbNext);
      Log.Info("The video page has previous video ={0}", lbPrevious);

      btnNextPage.Disabled = !lbNext;
      btnPreviousPage.Disabled = !lbPrevious;
      updateButtonStates();
    }

    private void OnClickPreviousPage()
    {
      miSelectedIndex = 0;
      bool lbNext = false;
      bool lbPrevious = false;
      switch (CURRENT_STATE)
      {
        case (int)State.NEW:
          moNewVideos.loadPreviousVideos(moSettings.msDefaultCountryName);
          lbNext = moNewVideos.hasNext();
          lbPrevious = moNewVideos.hasPrevious();
          DisplayVideoList(moNewVideos.moNewVideoList);
          break;
        case (int)State.TOP:
          moTopVideos.loadPreviousPage();
          lbNext = moTopVideos.hasMorePages();
          lbPrevious = moTopVideos.hasPreviousPage();
          DisplayVideoList(moTopVideos.getLastLoadedList());
         break;
        case (int)State.SEARCH:
          moYahooSearch.loadPreviousVideos();
          lbNext = moYahooSearch.hasNext();
          lbPrevious = moYahooSearch.hasPrevious();
          DisplayVideoList(moYahooSearch.moLastSearchResult);
          break;
        case (int)State.GENRE:
          moGenre.loadPreviousVideos();
          lbNext = moGenre.hasNext();
          lbPrevious = moGenre.hasPrevious();
          DisplayVideoList(moGenre.moGenreVideoList);
          break;
      }
      Log.Info("The video page has next video ={0}", lbNext);
      Log.Info("The video page has previous video ={0}", lbPrevious);

      btnNextPage.Disabled = !lbNext;
      btnPreviousPage.Disabled = !lbPrevious;
      updateButtonStates();
    }

    private List<YahooVideo> getStateVideoList()
    {
      List<YahooVideo> loCurrentDisplayVideoList = null;
      switch (CURRENT_STATE)
      {
        case (int)State.TOP:
          loCurrentDisplayVideoList = moTopVideos.getLastLoadedList();
          break;
        case (int)State.NEW:
          loCurrentDisplayVideoList = moNewVideos.moNewVideoList;
          break;
        case (int)State.SEARCH:
          loCurrentDisplayVideoList = moYahooSearch.moLastSearchResult;
          break;
        case (int)State.FAVORITE:
          loCurrentDisplayVideoList = moFavoriteManager.getFavoriteVideos();
          break;
        case (int)State.GENRE:
          loCurrentDisplayVideoList = moGenre.moGenreVideoList;
          break;
        default: break;
      }
      return loCurrentDisplayVideoList;
    }

    private YahooVideo getSelectedVideo()
    {
      YahooVideo loVideo = null;

      List<YahooVideo> loCurrentDisplayVideoList = getStateVideoList();

      if (loCurrentDisplayVideoList != null && loCurrentDisplayVideoList.Count > 0)
      {
        loVideo = loCurrentDisplayVideoList[listSongs.SelectedListItemIndex];
      }
      return loVideo;
    }

    private string getUserTypedText()
    {
      string KB_Search_Str = "";
      VirtualKeyboard keyBoard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      keyBoard.Text = "";
      keyBoard.Reset();
      keyBoard.DoModal(GUIWindowManager.ActiveWindow); // show it...            
      System.GC.Collect(); // collect some garbage
      if (keyBoard.Text == "" || keyBoard.Text == null)
      {
        return "";
      }
      KB_Search_Str = keyBoard.Text;
      return KB_Search_Str;
    }

    private void refreshScreenVideoList()
    {
      Log.Info("Refreshing video list on screen");
      List<YahooVideo> loCurrentDisplayVideoList = getStateVideoList();
      DisplayVideoList(loCurrentDisplayVideoList);
      listSongs.SelectedListItemIndex = miSelectedIndex;
    }

    private void DisplayVideoList(List<YahooVideo> foVideoList)
    {
      if (foVideoList == null && foVideoList.Count < 1) { return; }
      listSongs.Clear();
      GUIListItem item = null;
      int liVideoListSize = foVideoList.Count;
      foreach (YahooVideo loYahooVideo in foVideoList)
      {
        item = new GUIListItem();
        item.DVDLabel = loYahooVideo.songId;
        if (loYahooVideo.artistName == null || loYahooVideo.artistName.Equals(""))
        {
          item.Label = loYahooVideo.songName;
        }
        else
        {
          item.Label = loYahooVideo.artistName + " - " + loYahooVideo.songName;
        }
        item.IsFolder = false;
        //item.MusicTag = true;
        listSongs.Add(item);
      }
      this.LooseFocus();
      listSongs.Focus = true;
      if (listSongs.Count > 0)
      {
        ////labelSelected.Label = "Press Menu or F9 for more options.";
      }
      else
      {
        //labelSelected.Label = "";
      }
    }

    void playVideo(YahooVideo video)
    {
      //Log.Info("in playVideo()");
      string lsVideoLink = null;
      YahooSite loSite;
      YahooUtil loUtil = YahooUtil.getInstance();
      loSite = loUtil.getYahooSiteById(video.countryId);
      lsVideoLink = loUtil.getVideoMMSUrl(video, moSettings.msDefaultBitRate);
      lsVideoLink = lsVideoLink.Substring(0, lsVideoLink.Length - 2) + "&txe=.wmv";
      if (moSettings.mbUseVMR9)
          g_Player.PlayVideoStream(lsVideoLink, video.artistName + " - " + video.songName);
      else
        g_Player.PlayAudioStream(lsVideoLink, true);

      if (g_Player.Playing)
      {
        Log.Info("GUIMusicVideo: Playing: {0} with Bitrate: {1}", video.songName, moSettings.msDefaultBitRate);
        g_Player.ShowFullScreenWindow();
        moCurrentPlayingVideo = video;
      }
      else
      {
        Log.Info("GUIMusicVideo: Unable to play {0}", lsVideoLink);
        GUIDialogNotify dlg = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        dlg.SetHeading("ERROR");
        dlg.SetText("Unable to play the selected music video. Please try again later.");
        dlg.DoModal(GUIWindowManager.ActiveWindow);
      }
    }

    public void refreshStage2Screen()
    {
      updateButtonStates();
      refreshScreenVideoList();
    }

    void RefreshPage()
    {
      this.Restore();
      this.Init();
      this.Render(0);
      this.OnPageLoad();
    }

    private void updateButtonStates()
    {
      if ((int)State.HOME == CURRENT_STATE)
      {
        btnTop.Visible = true;
        btnNew.Visible = true;
        btnGenre.Visible = true;
        btnSearch.Visible = true;
        btnFavorites.Visible = true;
        btnCountry.Visible = true;

        btnBack.Visible = false;
        btnPlayAll.Visible = false;
        btnNextPage.Visible = false;
        btnPreviousPage.Visible = false;

        btnTop.NavigateUp = btnPlayList.GetID;
        btnPlayList.NavigateUp = btnCountry.GetID;
        btnPlayList.NavigateDown = btnTop.GetID;
        listSongs.NavigateLeft = btnTop.GetID;
        listSongs.Clear();
        if (g_Player.Playing)
        {
          // Temp hack
          btnPlayList.NavigateDown = 99;
          btnTop.NavigateUp = 99;
        }
        GUIPropertyManager.SetProperty("#itemcount", "");
      }
      else
      {
        btnNextPage.Visible = true;
        btnPreviousPage.Visible = true;
        btnBack.Visible = true;
        btnPlayAll.Visible = true;

        btnTop.Visible = false;
        btnSearch.Visible = false;
        btnFavorites.Visible = false;
        btnGenre.Visible = false;
        btnNew.Visible = false;
        btnCountry.Visible = false;


        btnPlayList.NavigateUp = btnPlayAll.GetID;
        btnPlayList.NavigateDown = btnBack.GetID;

        listSongs.NavigateLeft = btnBack.GetID;
        miSelectedIndex = 0;

        String lsItemCount = string.Empty;

        switch (CURRENT_STATE)
        {
          case (int)State.FAVORITE:
            lsItemCount = GUILocalizeStrings.Get(932) + " - " + moFavoriteManager.getSelectedFavorite();
            break;
          case (int)State.GENRE:
            lsItemCount = String.Format("{0} {1} - {2} {3} ", GUILocalizeStrings.Get(174), msSelectedGenre, GUILocalizeStrings.Get(30009), moGenre.getCurrentPageNumber());
            break;
          case (int)State.NEW:
            lsItemCount = GUILocalizeStrings.Get(30002) + " - " + GUILocalizeStrings.Get(30009) + " " + moNewVideos.getCurrentPageNumber();
            break;
          case (int)State.SEARCH:
            lsItemCount = String.Format(GUILocalizeStrings.Get(30010), moYahooSearch.getLastSearchText(), moYahooSearch.getCurrentPageNumber());
            break;
          case (int)State.TOP:
            lsItemCount = GUILocalizeStrings.Get(30001) + " " + GUILocalizeStrings.Get(30008) + " " + moTopVideos.getFirstVideoRank() + "-" + moTopVideos.getLastVideoRank();
            break;

        }
        //GUIPropertyManager.SetProperty("#header.label", lsHeaderLbl);
        //GUIPropertyManager.SetProperty("#selecteditem", lsSelectedItem);
        GUIPropertyManager.SetProperty("#itemcount", lsItemCount);
      }
    }
    #endregion
  }
}
