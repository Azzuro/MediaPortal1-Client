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
using System.Drawing;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MediaPortal.Util;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUITrailers : GUIWindow
  {
    #region SkinControlAttributes
    [SkinControlAttribute(3)]     protected GUISelectButtonControl btnletter = null;
    [SkinControlAttribute(4)]     protected GUIListControl listview = null;
    [SkinControlAttribute(5)]     protected GUIToggleButtonControl btntoggleplot = null;
    [SkinControlAttribute(6)]     protected GUIToggleButtonControl btntogglecast = null;
    [SkinControlAttribute(24)]    protected GUIImage poster = null;
    [SkinControlAttribute(50)]    protected GUILabelControl label0 = null;
    [SkinControlAttribute(51)]    protected GUIFadeLabel label1 = null;
    [SkinControlAttribute(52)]    protected GUILabelControl label2 = null; //runtime
    [SkinControlAttribute(53)]    protected GUILabelControl label3 = null;
    [SkinControlAttribute(54)]    protected GUILabelControl label4 = null;
    [SkinControlAttribute(55)]    protected GUILabelControl label5 = null;
    [SkinControlAttribute(56)]    protected GUILabelControl label6 = null;
    [SkinControlAttribute(57)]    protected GUITextScrollUpControl castarea = null;
    [SkinControlAttribute(58)]    protected GUILabelControl label8 = null;
    [SkinControlAttribute(59)]    protected GUITextScrollUpControl plotarea = null;
    //[SkinControlAttribute(60)]			protected GUIImageList labelrating =null;
    #endregion
    #region Variables

    int Prev_SelectedItem = 0;	// remember listview selections
    int[] SelectedItem = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    public static string TempHTML;	// GetWeb string
    public static string MMSUrl;
    string currentletter = "";
    string backgroundposter = null;

    string[] LMovieUrl = new string[200]; // strings for letterbutton movies
    string[] LMovieName = new string[200];

    DateTime RefreshDaily = DateTime.Now.Date; //for the daily update, if HTPC is on 24h it will refresh strings every day.

    bool mainview = false;
    bool letterview = false;
    bool plotview = true;
    bool castview = false;


    public static string ptitle;			// Used by SetGUIProperties, GetGUIProperties
    public static string pgenre;			// Before MP start playing a movie fullscreen
    public static string pruntime;		// it wil save the #tags info in these string
    public static string preleasedate;    // and will load them back after playing the 
    public static string pplot;			// movie.
    public static string pcast;
    public static double prating;

    string[] MainListMenu = new String[15];  //Decause the number of menu item is dynamic.

    // Get from mediaportal.xml
    public static string bitrate = string.Empty;
    bool Show_GT = false;
    bool Show_TSR = false;
    public static bool useVMR9 = true;
    public static string TSRbitrate = string.Empty;
    public static string TSRnmbOfResults = string.Empty;

    // BackGroundworker
    string _downloadedText = string.Empty;

    #endregion
    #region Override functions

    public override int GetID
    {
      get
      {
        return 5900;
      }
      set
      {
        base.GetID = value;
      }
    }
    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\mytrailers.xml");
    }
    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)  // For when a button is pressed
    {
      if (control == btnletter)
        OnButtonTwo();
      if (control == listview)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        int itemIndex = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(itemIndex);
        }
      }
      if (control == btntoggleplot)
        ToggleButtonPlot();
      if (control == btntogglecast)
        ToggleButtonCast();
      base.OnClicked(controlId, control, actionType);
    }
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_STOP)
        SetGUIProperties();
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU && mainview != true)
      {
        if (TrailersUtility._workerCompleted == false)
        {
          TrailersUtility._workerCompleted = true;
          TrailersUtility.interupted = true;
          ShowLabelsFalse();
        }
        OnClick(0);
        return;
      }

      base.OnAction(action);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      if (GermanTrailers.G_viewInfoAndTrailer == true || YahooTrailers.tview == true || YahooTrailers.cview == true || YahooTrailers.mview == true)
      {
        Prev_SelectedItem = listview.SelectedListItemIndex;
      }
      //GetGUIProperties();
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      btnletter.RestoreSelection = false;
      btnletter.AddSubItem("#");
      for (char k = 'A' ; k <= 'Z' ; k++)
      {
        btnletter.AddSubItem(k.ToString());
      }
      LoadSettings();
      if (GermanTrailers.G_viewInfoAndTrailer == true || YahooTrailers.tview == true || YahooTrailers.cview == true || YahooTrailers.mview == true)
      {

        listview.SelectedListItemIndex = Prev_SelectedItem;
        listview.Focus = true;
      }
      else
      {
        ShowLabelsFalse();
        GUIPropertyManager.SetProperty("#title", "");
        if (backgroundposter == null)
          backgroundposter = poster.FileName;
        poster.SetFileName(GUIGraphicsContext.Skin + @"\media\" + backgroundposter);
        ShowMainListView();
        listview.Focus = true;
      }
    }
    protected override void OnPreviousWindow()
    {
      base.OnPreviousWindow();
    }

    //public override bool OnMessage(GUIMessage message)
    //{
    //  if(message.Message == GUIMessage.MessageType.GUI_MSG_WINDOWS_MEDIA_PLAYER_BUFFERING)

    //  return base.OnMessage(message);
    //}

    #endregion
    #region Button/Click functions
    private void OnButtonTwo()
    {
      if (RefreshDaily.Equals(DateTime.Now.Date) == false) // daily update
        Array.Clear(YahooTrailers.MovieName, 0, 2000);
      if (YahooTrailers.MovieName[0] == null)
        YahooTrailers.GetTrailers();
      mainview = false;
      letterview = true;
      YahooTrailers.allview = false;
      YahooTrailers.tcmview = false;
      YahooTrailers.RSSView = false;
      YahooTrailers.tview = false;
      YahooTrailers.cview = false;
      YahooTrailers.mview = false;
      ShowLetterListView(YahooTrailers.MovieName, YahooTrailers.MovieURL);
    }
    private void ToggleButtonPlot()
    {
      btntoggleplot.Selected = true;
      btntogglecast.Selected = false;
      plotarea.Visible = true;
      castarea.Visible = false;
      label8.Visible = true;
      label6.Visible = false;
    }
    private void ToggleButtonCast()
    {
      if (GermanTrailers.G_viewInfoAndTrailer == true)
        GUIPropertyManager.SetProperty("cast", GermanTrailers.G_Cast[GermanTrailers.GermanSelected]);
      else YahooTrailers.GetCastInfo(YahooTrailers.casturl);
      btntoggleplot.Selected = false;
      btntogglecast.Selected = true;
      plotarea.Visible = false;
      castarea.Visible = true;
      label8.Visible = false;
      label6.Visible = true;
    }
    private string GetSelectedListViewItemText()
    {
      string strLabel1 = "", strLabel2 = "", strThumb = "";
      listview.GetSelectedItem(ref strLabel1, ref strLabel2, ref strThumb);
      if (strLabel1 != "")
      {
        if (strLabel1[0] == '[')
          strLabel1 = strLabel1.Remove(0, 1);
        if (strLabel1[strLabel1.Length - 1] == ']')
          strLabel1 = strLabel1.Remove(strLabel1.Length - 1, 1);
      }
      return strLabel1;
    }
    private void OnClick(int itemindex) // // When something is pressed in the listview
    {
      // Trailer, Clips, Movie listview
      if (YahooTrailers.tcmview == true)
      {
        if (itemindex == 0)
        {
          if (YahooTrailers.allview == true)
          {
            ShowLabelsFalse();
            YahooTrailers.tcmview = false;
            ShowListView(YahooTrailers.MovieName, 5905);
            listview.SelectedListItemIndex = SelectedItem[1];
          }
          else if (YahooTrailers.RSSView == true)
          {
            ShowLabelsFalse();
            YahooTrailers.tcmview = false;
            ShowListView(YahooTrailers.RSSMovieName, YahooTrailers.RSSTitle);
            listview.SelectedListItemIndex = SelectedItem[1];
          }
          else if (letterview == true)
          {
            ShowLabelsFalse();
            YahooTrailers.tcmview = false;
            OnButtonTwo();
            listview.SelectedListItemIndex = SelectedItem[1];
          }
        }
        else if (itemindex == 1)
        {
          SelectedItem[2] = listview.SelectedListItemIndex; ;
          if (YahooTrailers.foundt == true)
          {
            ShowLabelsFalse();
            YahooTrailers.tcmview = false;
            YahooTrailers.tview = true;
            ShowListView(YahooTrailers.Trailers, ptitle, true);
          }
          else if (YahooTrailers.foundt == false & YahooTrailers.foundc == true)
          {
            ShowLabelsFalse();
            YahooTrailers.tcmview = false;
            YahooTrailers.cview = true;
            ShowListView(YahooTrailers.Clips, ptitle, true);
          }
        }
        else if (itemindex == 2)
        {
          SelectedItem[2] = listview.SelectedListItemIndex; ;
          if (YahooTrailers.foundt == false & YahooTrailers.foundc == true)
          {
            ShowLabelsFalse();
            YahooTrailers.tcmview = false;
            YahooTrailers.mview = true;
            ShowListView(YahooTrailers.More, ptitle, true);
          }
          else if (YahooTrailers.foundc == true)
          {
            ShowLabelsFalse();
            YahooTrailers.tcmview = false;
            YahooTrailers.cview = true;
            ShowListView(YahooTrailers.Clips, ptitle, true);
          }
        }
        else if (itemindex == 3)
        {
          ShowLabelsFalse();
          SelectedItem[2] = listview.SelectedListItemIndex;
          YahooTrailers.tcmview = false;
          YahooTrailers.mview = true;
          ShowListView(YahooTrailers.More, ptitle, true);
        }
      }
      // Trailerview
      else if (YahooTrailers.tview == true)
      {
        if (itemindex == 0)
        {
          YahooTrailers.tview = false;
          YahooTrailers.tcmview = true;
          ShowLabelsTrue();
          SetGUIProperties();
          ShowListView(YahooTrailers.TrailersClipsMore, true);
          listview.SelectedListItemIndex = SelectedItem[2];
        }
        else
        {
          Prev_SelectedItem = listview.SelectedListItemIndex;
          PlayTrailer(YahooTrailers.TrailersUrl[itemindex - 1], GetSelectedListViewItemText());
        }
      }
      // Clipsview
      else if (YahooTrailers.cview == true)
      {
        if (itemindex == 0)
        {
          YahooTrailers.cview = false;
          YahooTrailers.tcmview = true;
          ShowLabelsTrue();
          SetGUIProperties();
          ShowListView(YahooTrailers.TrailersClipsMore, true);
          listview.SelectedListItemIndex = SelectedItem[2];
        }
        else
        {
          Prev_SelectedItem = listview.SelectedListItemIndex;
          PlayTrailer(YahooTrailers.ClipsUrl[itemindex - 1], GetSelectedListViewItemText());
        }

      }
      // Moreview
      else if (YahooTrailers.mview == true)
      {
        if (itemindex == 0)
        {
          YahooTrailers.mview = false;
          YahooTrailers.tcmview = true;
          ShowLabelsTrue();
          SetGUIProperties();
          ShowListView(YahooTrailers.TrailersClipsMore, true);
          listview.SelectedListItemIndex = SelectedItem[2];
        }
        else
        {
          Prev_SelectedItem = listview.SelectedListItemIndex;
          PlayTrailer(YahooTrailers.MoreUrl[itemindex - 1], GetSelectedListViewItemText());
        }
      }
      // RSSView
      else if (YahooTrailers.RSSView == true)
      {
        if (itemindex == 0)
        {
          YahooTrailers.RSSView = false;
          mainview = true;
          ShowMainListView();
          listview.SelectedListItemIndex = SelectedItem[0];
        }
        else
        {
          SelectedItem[1] = listview.SelectedListItemIndex;
          YahooTrailers.GetMovieInfo(YahooTrailers.RSSMovieUrl[itemindex - 1], YahooTrailers.RSSMovieName[itemindex - 1]);
          if (TrailersUtility.interupted == true)
          {
            TrailersUtility.interupted = false;
            return;
          }
          ShowListView(YahooTrailers.TrailersClipsMore, false);
          YahooTrailers.GetMovieDetails(YahooTrailers.RSSMovieUrl[itemindex - 1], YahooTrailers.RSSMovieName[itemindex - 1]);
          if (TrailersUtility.interupted == true)
          {
            TrailersUtility.interupted = false;
            return;
          }
          ShowMovieInfo(YahooTrailers.RSSMovieName[itemindex - 1], YahooTrailers.PosterUrl);
          YahooTrailers.RSSTitle = GUIPropertyManager.GetProperty("#title");
          YahooTrailers.tcmview = true;
          GetGUIProperties();
        }
      }
      // Letterbutton view
      else if (letterview == true)
      {
        if (itemindex == 0)
        {
          letterview = false;
          mainview = true;
          ShowMainListView();
          listview.SelectedListItemIndex = SelectedItem[0];
        }
        else
        {
          SelectedItem[1] = listview.SelectedListItemIndex;
          YahooTrailers.tcmview = true;
          YahooTrailers.GetMovieInfo(LMovieUrl[itemindex - 1], LMovieName[itemindex - 1]);
          ShowListView(YahooTrailers.TrailersClipsMore, false);
          YahooTrailers.GetMovieDetails(LMovieUrl[itemindex - 1], LMovieName[itemindex - 1]);
          ShowMovieInfo(LMovieName[itemindex - 1], YahooTrailers.PosterUrl);
        }
      }
      // All movies view
      else if (YahooTrailers.allview == true)
      {
        if (itemindex == 0)
        {
          YahooTrailers.allview = false;
          mainview = true;
          ShowMainListView();
          listview.SelectedListItemIndex = SelectedItem[0];
        }
        else
        {
          SelectedItem[1] = listview.SelectedListItemIndex;
          YahooTrailers.tcmview = true;
          YahooTrailers.GetMovieInfo(YahooTrailers.MovieURL[itemindex - 1], YahooTrailers.MovieName[itemindex - 1]);
          ShowListView(YahooTrailers.TrailersClipsMore, false);
          YahooTrailers.GetMovieDetails(YahooTrailers.MovieURL[itemindex - 1], YahooTrailers.MovieName[itemindex - 1]);
          ShowMovieInfo(YahooTrailers.MovieName[itemindex - 1], YahooTrailers.PosterUrl);
        }
      }
      // Main selection view
      else if (mainview == true)
      {
        if (itemindex == 0) // just added movies
        {
          mainview = false;
          YahooTrailers.RSSView = true;
          YahooTrailers.GetYahooTrailersRSS(@"http://rss.ent.yahoo.com/movies/newtrailers.xml");
          ShowListView(YahooTrailers.RSSMovieName, 5903);
        }
        if (itemindex == 1) // mostwatched movies
        {
          mainview = false;
          YahooTrailers.RSSView = true;
          SelectedItem[0] = listview.SelectedListItemIndex;
          YahooTrailers.GetYahooTrailersRSS(@"http://rss.ent.yahoo.com/movies/top25trailers.xml");
          ShowListView(YahooTrailers.RSSMovieName, 5904);
        }
        if (itemindex == 2) // weekend box office
        {
          mainview = false;
          YahooTrailers.RSSView = true;
          SelectedItem[0] = listview.SelectedListItemIndex;
          YahooTrailers.GetYahooTrailersRSS(@"http://rss.ent.yahoo.com/movies/boxoffice.xml");
          ShowListView(YahooTrailers.RSSMovieName, 5919);
        }
        if (itemindex == 3) // opening this week
        {
          mainview = false;
          YahooTrailers.RSSView = true;
          SelectedItem[0] = listview.SelectedListItemIndex;
          YahooTrailers.GetYahooTrailersRSS(@"http://rss.ent.yahoo.com/movies/thisweek.xml");
          ShowListView(YahooTrailers.RSSMovieName, 5920);
        }
        if (itemindex == 4) // coming soon
        {
          mainview = false;
          YahooTrailers.RSSView = true;
          SelectedItem[0] = listview.SelectedListItemIndex;
          YahooTrailers.GetYahooTrailersRSS(@"http://rss.ent.yahoo.com/movies/upcoming.xml");
          ShowListView(YahooTrailers.RSSMovieName, 5921);
        }
        if (itemindex == 5) // top selling dvds
        {
          mainview = false;
          YahooTrailers.RSSView = true;
          SelectedItem[0] = listview.SelectedListItemIndex;
          YahooTrailers.GetYahooTrailersRSS(@"http://rss.movies.yahoo.com/dvd/topsellers.xml");
          ShowListView(YahooTrailers.RSSMovieName, 5922);
        }
        if (itemindex == 6) // new this week on DVD
        {
          mainview = false;
          YahooTrailers.RSSView = true;
          SelectedItem[0] = listview.SelectedListItemIndex;
          YahooTrailers.GetYahooTrailersRSS(@"http://rss.movies.yahoo.com/dvd/newthisweek.xml");
          ShowListView(YahooTrailers.RSSMovieName, 5923);
        }
        if (itemindex == 7) // new next week on DVD
        {
          mainview = false;
          YahooTrailers.RSSView = true;
          SelectedItem[0] = listview.SelectedListItemIndex;
          YahooTrailers.GetYahooTrailersRSS(@"http://rss.movies.yahoo.com/dvd/newnextweek.xml");
          ShowListView(YahooTrailers.RSSMovieName, 5924);
        }
        if (itemindex == 8) // pre-order dvds
        {
          mainview = false;
          YahooTrailers.RSSView = true;
          SelectedItem[0] = listview.SelectedListItemIndex;
          YahooTrailers.GetYahooTrailersRSS(@"http://rss.ent.yahoo.com/movies/dvd/preorders.xml");
          ShowListView(YahooTrailers.RSSMovieName, 5925);
        }
        if (itemindex == 9) // all movies
        {
          mainview = false;
          YahooTrailers.allview = true;
          SelectedItem[0] = listview.SelectedListItemIndex;
          if (RefreshDaily.Equals(DateTime.Now.Date) == false)
            Array.Clear(YahooTrailers.MovieName, 0, 2000);
          if (YahooTrailers.MovieName[0] == null)
          { YahooTrailers.GetTrailers(); }
          ShowListView(YahooTrailers.MovieName, 5905);
        }
        if (itemindex >= 9)
        {
          if (MainListMenu[itemindex] == "GERMAN")// german trailers
          {
            mainview = false;
            GermanTrailers.G_viewWoche = true;
            SelectedItem[0] = listview.SelectedListItemIndex;
            ShowListView(GermanTrailers.Woche, 5911, 5915);
          }
          if (MainListMenu[itemindex] == "TSRVOD") // TSR VOD
          {
            mainview = false;
            btnletter.Visible = false;
            TSRVodTrailers.menuview = true;
            SelectedItem[0] = listview.SelectedListItemIndex;
            if (TSRVodTrailers.MenuName[0] == null)
            { TSRVodTrailers.GetMenu(); }
            ShowListView(TSRVodTrailers.MenuName, 5918);
          }
        }
      }
      else if (TSRVodTrailers.menuview == true)
      {
        if (itemindex == 0)
        {
          TSRVodTrailers.menuview = false;
          mainview = true;
          btnletter.Visible = true;
          //poster.Visible = true;
          ShowMainListView();
          listview.SelectedListItemIndex = SelectedItem[0];
        }
        else
        {
          TSRVodTrailers.menuview = false;
          TSRVodTrailers.submenuview = true;
          TSRVodTrailers.GetSubMenu(TSRVodTrailers.MenuURL[itemindex - 1], TSRbitrate, TSRnmbOfResults);
          listview.SelectedListItemIndex = itemindex;
          ShowListView(TSRVodTrailers.SubMenuName, TSRVodTrailers.MenuName[itemindex - 1], false);
        }
      }
      else if (TSRVodTrailers.submenuview == true)
      {
        if (itemindex == 0)
        {
          TSRVodTrailers.submenuview = false;
          TSRVodTrailers.menuview = true;
          ShowListView(TSRVodTrailers.MenuName, 5918);
          listview.SelectedListItemIndex = SelectedItem[0];
        }
        else
        {
          //Play VOD
          g_Player.ShowFullScreenWindow();
          g_Player.FullScreen = true;
          g_Player.Play(TSRVodTrailers.SubMenuURL[itemindex - 1]);
        }
      }
      // German Trailerview Woche
      else if (GermanTrailers.G_viewWoche == true)
      {
        if (itemindex == 0)
        {
          GermanTrailers.G_viewWoche = false;
          mainview = true;
          ShowMainListView();
          listview.SelectedListItemIndex = SelectedItem[0];
        }
        if (itemindex == 1)
        {
          GermanTrailers.G_viewWoche = false;
          GermanTrailers.G_viewMovie = true;
          SelectedItem[1] = listview.SelectedListItemIndex;
          GermanTrailers.GetGermanTrailers("http://de.movies.yahoo.com/mvsl.html");
          ShowListView(GermanTrailers.GermanMovieName, 5911, 5915);
        }
        if (itemindex == 2)
        {
          GermanTrailers.G_viewWoche = false;
          GermanTrailers.G_viewMovie = true;
          SelectedItem[1] = listview.SelectedListItemIndex;
          GermanTrailers.GetGermanTrailers("http://de.movies.yahoo.com/neu_im_kino.html");
          ShowListView(GermanTrailers.GermanMovieName, 5911, 5915);
        }
        if (itemindex == 3)
        {
          GermanTrailers.G_viewWoche = false;
          GermanTrailers.G_viewMovie = true;
          SelectedItem[1] = listview.SelectedListItemIndex;
          GermanTrailers.GetGermanTrailers("http://de.movies.yahoo.com/mvsn.html");
          ShowListView(GermanTrailers.GermanMovieName, 5911, 5915);
        }

      }
      // German TrailerMovies view
      else if (GermanTrailers.G_viewMovie == true)
      {
        if (itemindex == 0)
        {
          GermanTrailers.G_viewWoche = true;
          GermanTrailers.G_viewMovie = false;
          ShowListView(GermanTrailers.Woche, 5911, 5915);
          listview.SelectedListItemIndex = SelectedItem[1];
        }
        else
        {
          SelectedItem[2] = listview.SelectedListItemIndex;
          GermanTrailers.G_viewMovie = false;
          GermanTrailers.G_viewInfoAndTrailer = true;
          GermanTrailers.GermanSelected = itemindex - 1;
          Prev_SelectedItem = itemindex + 1;
          GermanTrailers.SetProperties(GermanTrailers.GermanMovieName[itemindex - 1], itemindex - 1);
          ShowMovieInfo(GermanTrailers.GermanMovieName[itemindex - 1], GermanTrailers.G_PosterUrl[itemindex - 1]);
          ShowListViewAndInfo(GermanTrailers.GermanMovieName[itemindex - 1], GermanTrailers.GermanTrailerURL[itemindex - 1]);
          label2.Visible = false; //runtime info not available
        }
      }
      // German movie info and single trailer view
      else if (GermanTrailers.G_viewInfoAndTrailer == true)
      {
        if (itemindex == 0)
        {
          ShowLabelsFalse();
          GermanTrailers.G_viewMovie = true;
          GermanTrailers.G_viewInfoAndTrailer = false;
          ShowListView(GermanTrailers.GermanMovieName, false);
          listview.SelectedListItemIndex = SelectedItem[2];
        }
        if (itemindex == 1)
        {
          if (GermanTrailers.GermanTrailerURL[GermanTrailers.GermanSelected] != null)
            Prev_SelectedItem = listview.SelectedListItemIndex;
          GermanTrailers.PlayGermanTrailer(GermanTrailers.GermanTrailerURL[GermanTrailers.GermanSelected]);
          ShowListViewAndInfo(GermanTrailers.GermanMovieName[GermanTrailers.GermanSelected], GermanTrailers.GermanTrailerURL[GermanTrailers.GermanSelected]);
          label2.Visible = false; //runtime info not available
        }
      }
    }
    #endregion
    void ShowMainListView()
    {
      mainview = true;
      YahooTrailers.allview = false;
      YahooTrailers.RSSView = false;
      letterview = false;
      poster.SetFileName(GUIGraphicsContext.Skin + @"\media\" + backgroundposter);
      GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(5902)); //Show which movies?

      string[] MainListOptions = new string[15];
      MainListOptions[0] = GUILocalizeStrings.Get(5903);
      MainListMenu[0] = MainListOptions[0];
      MainListOptions[1] = GUILocalizeStrings.Get(5904);
      MainListMenu[1] = MainListOptions[1];
      MainListOptions[2] = GUILocalizeStrings.Get(5919);
      MainListMenu[2] = MainListOptions[2];
      MainListOptions[3] = GUILocalizeStrings.Get(5920);
      MainListMenu[3] = MainListOptions[3];
      MainListOptions[4] = GUILocalizeStrings.Get(5921);
      MainListMenu[4] = MainListOptions[4];
      MainListOptions[5] = GUILocalizeStrings.Get(5922);
      MainListMenu[5] = MainListOptions[5];
      MainListOptions[6] = GUILocalizeStrings.Get(5923);
      MainListMenu[6] = MainListOptions[6];
      MainListOptions[7] = GUILocalizeStrings.Get(5924);
      MainListMenu[7] = MainListOptions[7];
      MainListOptions[8] = GUILocalizeStrings.Get(5925);
      MainListMenu[8] = MainListOptions[8];
      MainListOptions[9] = GUILocalizeStrings.Get(5905);
      MainListMenu[9] = MainListOptions[9];

      //string language = "";
      //using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      //{
      //  language = xmlreader.GetValue("skin", "language");
      //}
      int iNextItem = 10;
      //if (language.Equals("German") == true || Show_GT == true)
      //{
      //  MainListOptions[iNextItem] = GUILocalizeStrings.Get(5917);
      //  MainListMenu[iNextItem] = "GERMAN";
      //  iNextItem++;
      //}
      if (Show_TSR == true)
      {
        MainListOptions[iNextItem] = GUILocalizeStrings.Get(5918);
        MainListMenu[iNextItem] = "TSRVOD";
        iNextItem++;
      }

      listview.Clear();
      int i = 0;
      while (MainListOptions[i] != null)
      {
        GUIListItem item = new GUIListItem();
        item.IsFolder = true;
        MediaPortal.Util.Utils.SetDefaultIcons(item);
        item.Label = MainListOptions[i];
        listview.Add(item);
        i++;
      }
    }

    public static void GetMMSURL(string url)
    {
      TempHTML = "";
      TrailersUtility TU = new TrailersUtility();
      TU.GetWebPage(url, out TempHTML);

      if (TempHTML == null || TempHTML == string.Empty)
        return;

      Match m = Regex.Match(TempHTML, @"<Ref\shref\s=\s.(?<mmsurl>mms.*).\s/>");
      MMSUrl = m.Groups["mmsurl"].Value;
      if (YahooTrailers.server.Equals(string.Empty) == false)
      {
        int B = MMSUrl.IndexOf("/") + 2;
        int E = MMSUrl.IndexOf("/", B);
        int T = E - B;
        MMSUrl.Replace(MMSUrl.Substring(B, T), YahooTrailers.server);
      }
      if (MMSUrl.Equals("") == true)
      {
        GUIDialogOK dlg = new GUIDialogOK();
        dlg.SetHeading(1025);
        dlg.SetLine(1, 5909);
        dlg.DoModal(GUIWindowManager.ActiveWindow);
      }
    }

    public static void PlayTrailer(string url, string videoName)
    {
      GetGUIProperties();
      MMSUrl = string.Empty;
      GetMMSURL(url);
      Log.Debug("Url: {0}, mmsurl: {1}, videoname: {2}", url, MMSUrl, videoName);
      bool playOk = false;
      if (useVMR9)
        playOk = g_Player.PlayVideoStream(MMSUrl, videoName);
      else
        playOk = g_Player.PlayAudioStream(MMSUrl, true);
      if (playOk)
      {
        g_Player.ShowFullScreenWindow();
        g_Player.FullScreen = true;
      }
      else
      {
        Log.Info("GUITrailers: Unable to play {0}", MMSUrl);
        GUIDialogNotify dlg = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        dlg.SetHeading("ERROR");
        dlg.SetText("Unable to play the selected video trailer. Please try again later.");
        dlg.DoModal(GUIWindowManager.ActiveWindow);
      }
    }

    void ShowLabelsFalse()
    {
      label0.Visible = false;
      label1.Visible = false;
      label2.Visible = false;
      label3.Visible = false;
      label4.Visible = false;
      label5.Visible = false;
      label6.Visible = false;
      castarea.Visible = false;
      label8.Visible = false;
      plotarea.Visible = false;
      btntoggleplot.Visible = false;
      btntogglecast.Visible = false;
      //labelrating.Visible=false;
      btnletter.NavigateDown = 4;
    }

    void ShowLabelsTrue()
    {
      label0.Visible = true;
      label1.Visible = true;
      label2.Visible = true;
      label3.Visible = true;
      label4.Visible = true;
      label5.Visible = true;
      label6.Visible = true;
      label8.Visible = true;
      btntoggleplot.Visible = true;
      btntogglecast.Visible = true;
      //labelrating.Visible=true;
      btnletter.NavigateDown = 5;
      if (castview == true)
        ToggleButtonCast();
      else
        ToggleButtonPlot();
    }

    public static void GetGUIProperties()
    {
      ptitle = GUIPropertyManager.GetProperty("#title");
      pgenre = GUIPropertyManager.GetProperty("#genre");
      pruntime = GUIPropertyManager.GetProperty("#runtime");
      preleasedate = GUIPropertyManager.GetProperty("#year");
      pplot = GUIPropertyManager.GetProperty("#plot");
      pcast = GUIPropertyManager.GetProperty("#cast");
      if (YahooTrailers.tcmview != true)
      {
        GUIPropertyManager.SetProperty("#title", string.Empty);
        GUIPropertyManager.SetProperty("#genre", string.Empty);
        GUIPropertyManager.SetProperty("#runtime", string.Empty);
        GUIPropertyManager.SetProperty("#year", string.Empty);
        GUIPropertyManager.SetProperty("#plot", string.Empty);
        GUIPropertyManager.SetProperty("#cast", string.Empty);
      }
      //float.Parse(prating) = GUIPropertyManager.GetProperty("#rating");
    }

    void SetGUIProperties()
    {
      GUIPropertyManager.SetProperty("#title", ptitle);
      GUIPropertyManager.SetProperty("#genre", pgenre);
      GUIPropertyManager.SetProperty("#runtime", pruntime);
      GUIPropertyManager.SetProperty("#year", preleasedate);
      GUIPropertyManager.SetProperty("#plot", pplot);
      GUIPropertyManager.SetProperty("#cast", pcast);
      GUIPropertyManager.SetProperty("#rating", prating.ToString());
    }

    void LoadSettings()
    {
      if (YahooTrailers.tview == true)
      {
        YahooTrailers.tview = true;
        ShowListView(YahooTrailers.Trailers, ptitle, true);
      }
      if (YahooTrailers.cview == true)
      {
        YahooTrailers.cview = true;
        ShowListView(YahooTrailers.Clips, ptitle, true);
      }
      if (YahooTrailers.mview == true)
      {
        YahooTrailers.mview = true;
        ShowListView(YahooTrailers.More, ptitle, true);
      }
      if (GermanTrailers.G_viewInfoAndTrailer == true)
      {
        ShowMovieInfo(GermanTrailers.GermanMovieName[GermanTrailers.GermanSelected], GermanTrailers.G_PosterUrl[GermanTrailers.GermanSelected]);
        GermanTrailers.G_viewMovie = false;
        GermanTrailers.G_viewInfoAndTrailer = true;
        Prev_SelectedItem = GermanTrailers.GermanSelected;
        GermanTrailers.SetProperties(GermanTrailers.GermanMovieName[GermanTrailers.GermanSelected], GermanTrailers.GermanSelected);
        label2.Visible = false; //runtime info not available
        if (plotview == true)
          ToggleButtonPlot();
        if (castview == true)
          ToggleButtonCast();
      }
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        bitrate = xmlreader.GetValue("mytrailers", "speed");
        Show_GT = xmlreader.GetValueAsBool("mytrailers", "Show german trailers", false);
        Show_TSR = xmlreader.GetValueAsBool("mytrailers", "Show tsr vod", false);
        TSRbitrate = xmlreader.GetValue("mytrailers", "TSR speed");
        TSRnmbOfResults = xmlreader.GetValue("mytrailers", "TSR nmbOfResults");
        useVMR9 = xmlreader.GetValueAsBool("general", "usevrm9forwebstreams", true);
        if (TSRnmbOfResults != "-1")
          TSRnmbOfResults = "&nmbOfResults=" + TSRnmbOfResults;
        YahooTrailers.server = xmlreader.GetValue("mytrailers", "YahooServer");
      }
    }

    public void ShowListView(string[] _TrailerName, bool show_poster)
    {
      if (show_poster == false)
        poster.SetFileName(GUIGraphicsContext.Skin + @"\media\" + backgroundposter);

      listview.Clear();
      GUIListItem item1 = new GUIListItem();
      item1.Label = "..";
      item1.IsFolder = true;
      MediaPortal.Util.Utils.SetDefaultIcons(item1);
      listview.Add(item1);

      int i = 0;
      while (_TrailerName[i] != null)
      {
        GUIListItem item = new GUIListItem();
        item.IsFolder = true;
        MediaPortal.Util.Utils.SetDefaultIcons(item);
        item.Label = _TrailerName[i];
        listview.Add(item);
        i++;
      }
    }

    public void ShowListView(string[] _TrailerName, int _titlenumber)
    {
      if (TrailersUtility.interupted == true)
      {
        TrailersUtility.interupted = false;
        return;
      }
      poster.SetFileName(GUIGraphicsContext.Skin + @"\media\" + backgroundposter);
      GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(_titlenumber));

      listview.Clear();
      GUIListItem item1 = new GUIListItem();
      item1.Label = "..";
      item1.IsFolder = true;
      MediaPortal.Util.Utils.SetDefaultIcons(item1);
      listview.Add(item1);

      int i = 0;
      while (_TrailerName[i] != null)
      {
        GUIListItem item = new GUIListItem();
        item.IsFolder = true;
        MediaPortal.Util.Utils.SetDefaultIcons(item);
        item.Label = _TrailerName[i];
        listview.Add(item);
        i++;
      }
    }

    public void ShowListView(string[] _TrailerName, string _titlename)
    {
      poster.SetFileName(GUIGraphicsContext.Skin + @"\media\" + backgroundposter);
      GUIPropertyManager.SetProperty("#title", _titlename);

      listview.Clear();
      GUIListItem item1 = new GUIListItem();
      item1.Label = "..";
      item1.IsFolder = true;
      MediaPortal.Util.Utils.SetDefaultIcons(item1);
      listview.Add(item1);

      int i = 0;
      while (_TrailerName[i] != null)
      {
        GUIListItem item = new GUIListItem();
        item.IsFolder = true;
        MediaPortal.Util.Utils.SetDefaultIcons(item);
        item.Label = _TrailerName[i];
        listview.Add(item);
        i++;
      }
    }

    public void ShowListView(string[] _TrailerName, string _titlename, bool show_poster)
    {
      if (show_poster == false)
        poster.SetFileName(GUIGraphicsContext.Skin + @"\media\" + backgroundposter);

      GUIPropertyManager.SetProperty("#title", _titlename);

      listview.Clear();
      GUIListItem item1 = new GUIListItem();
      item1.Label = "..";
      item1.IsFolder = true;
      MediaPortal.Util.Utils.SetDefaultIcons(item1);
      listview.Add(item1);

      int i = 0;
      while (_TrailerName[i] != null)
      {
        GUIListItem item = new GUIListItem();
        item.IsFolder = true;
        item.Label = _TrailerName[i];
        item.IconImage = "defaultVideo.png";
        listview.Add(item);
        i++;
      }
    }

    public void ShowListView(string[] _TrailerName, string[] _TralerUrl, int _titlenumber)
    {
      poster.SetFileName(GUIGraphicsContext.Skin + @"\media\" + backgroundposter);
      GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(_titlenumber));

      listview.Clear();
      if (_TralerUrl[0] == null)
      {
        GUIListItem item = new GUIListItem();
        item.Label = ".. - " + GUILocalizeStrings.Get(5916);
        item.IsFolder = true;
        item.IconImage = "defaultFolderBack.png";
        listview.Add(item);
        return;
      }
      else
      {
        GUIListItem item1 = new GUIListItem();
        item1.Label = "..";
        item1.IsFolder = true;
        MediaPortal.Util.Utils.SetDefaultIcons(item1);
        listview.Add(item1);

        int i = 0;
        while (_TrailerName[i] != null)
        {
          GUIListItem item = new GUIListItem();
          item.IsFolder = true;
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          item.Label = _TrailerName[i];
          listview.Add(item);
          i++;
        }
      }
    }

    // fe no movie review yet
    public void ShowListView(string[] _TrailerName, int _titlenumber, int _folderupnumber)
    {
      poster.SetFileName(GUIGraphicsContext.Skin + @"\media\" + backgroundposter);
      GUIPropertyManager.SetProperty("#title", GUILocalizeStrings.Get(_titlenumber));

      listview.Clear();
      if (_TrailerName[0] == null)
      {
        GUIListItem item = new GUIListItem();
        item.Label = ".. - " + GUILocalizeStrings.Get(_folderupnumber);
        item.IsFolder = true;
        item.IconImage = "defaultFolderBack.png";
        listview.Add(item);
        return;
      }
      else
      {
        GUIListItem item1 = new GUIListItem();
        item1.Label = "..";
        item1.IsFolder = true;
        MediaPortal.Util.Utils.SetDefaultIcons(item1);
        listview.Add(item1);

        int i = 0;
        while (_TrailerName[i] != null)
        {
          GUIListItem item = new GUIListItem();
          item.IsFolder = true;
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          item.Label = _TrailerName[i];
          listview.Add(item);
          i++;
        }
      }
    }

    public void ShowListViewAndInfo(string _TrailerName, string _TralerUrl)
    {
      listview.Clear();
      if (_TralerUrl == null)
      {
        GUIListItem item = new GUIListItem();
        item.Label = ".. - " + GUILocalizeStrings.Get(5916);
        item.IsFolder = true;
        item.IconImage = "defaultFolderBack.png";
        listview.Add(item);
        return;
      }
      else
      {
        GUIListItem item1 = new GUIListItem();
        item1.Label = "..";
        item1.IsFolder = true;
        MediaPortal.Util.Utils.SetDefaultIcons(item1);
        listview.Add(item1);

        GUIListItem item = new GUIListItem();
        item.Label = _TrailerName;
        item.IconImage = "defaultVideo.png";
        listview.Add(item);
      }

    }

    public void ShowPoster(string downloadurl, string moviename)
    {
      if (downloadurl == null | downloadurl == string.Empty)
        return;
      else
      {
        TrailersUtility TU = new TrailersUtility();
        TU.DownloadPoster(downloadurl, moviename);
        moviename = moviename.Replace(":", "-");
        if (moviename.Contains("opens"))
        {
          int i = moviename.IndexOf("opens");
          moviename = moviename.Remove(i);
        }

        // TODO - add utils function here
        string thumbFile = System.IO.Path.Combine(Thumbs.Trailers, moviename + ".jpg");
        if (System.IO.File.Exists(thumbFile))
          poster.SetFileName(thumbFile);
      }
    }

    public void ShowMovieInfo(string moviename, string posterurl)
    {
      ShowLabelsTrue();
      ShowPoster(posterurl, moviename);
    }

    public void ShowLetterListView(string[] movienames, string[] movieurls)
    {
      int i = 0;
      int j = 0;
      string letter;
      listview.Clear();
      ShowLabelsFalse();
      Array.Clear(LMovieName, 0, 200);
      Array.Clear(LMovieUrl, 0, 200);

      poster.SetFileName(GUIGraphicsContext.Skin + @"\media\" + backgroundposter);
      GUIPropertyManager.SetProperty("#title", "");
      letter = btnletter.SelectedLabel;

      GUIListItem item1 = new GUIListItem();
      item1.Label = "..";
      item1.IsFolder = true;
      MediaPortal.Util.Utils.SetDefaultIcons(item1);
      listview.Add(item1);

      while (movienames[i] != null)
      {
        if (movienames[i].StartsWith("The " + letter) == true)
        {
          LMovieName[j] = movienames[i];
          LMovieUrl[j] = movieurls[i];
          GUIListItem item = new GUIListItem();
          item.Label = LMovieName[j];
          item.IsFolder = true;
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          listview.Add(item);
          j++;
        }
        else if (movienames[i].StartsWith("The ") == true) { }
        else if (movienames[i].StartsWith("A " + letter) == true)
        {
          LMovieName[j] = movienames[i];
          LMovieUrl[j] = movieurls[i];
          GUIListItem item = new GUIListItem();
          item.Label = LMovieName[j];
          item.IsFolder = true;
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          listview.Add(item);
          j++;
        }
        else if (movienames[i].StartsWith("A ") == true) { }
        else if (movienames[i].StartsWith("An " + letter) == true)
        {
          LMovieName[j] = movienames[i];
          LMovieUrl[j] = movieurls[i];
          GUIListItem item = new GUIListItem();
          item.Label = LMovieName[j];
          item.IsFolder = true;
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          listview.Add(item);
          j++;
        }
        else if (movienames[i].StartsWith("An ") == true) { }
        else if (movienames[i].StartsWith(letter) == true)
        {
          LMovieName[j] = movienames[i];
          LMovieUrl[j] = movieurls[i];
          GUIListItem item = new GUIListItem();
          item.Label = LMovieName[j];
          listview.Add(item);
          item.IsFolder = true;
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          j++;
        }
        else if (letter.Equals("#") == true)
        {
          for (int n = 0 ; n <= 9 ; n++)
            if (movienames[i].StartsWith(n.ToString()) == true)
            {
              LMovieName[j] = movienames[i];
              LMovieUrl[j] = movieurls[i];
              GUIListItem item = new GUIListItem();
              item.Label = LMovieName[j];
              item.IsFolder = true;
              MediaPortal.Util.Utils.SetDefaultIcons(item);
              listview.Add(item);
              j++;
            }
        }

        i++;
      }
      if (currentletter == letter)
        listview.SelectedListItemIndex = Prev_SelectedItem;
      else
      {
        listview.SelectedListItemIndex = 0;
        currentletter = letter;
      }
    }

  }
}