using System;
using System.IO;
using System.Xml.Serialization;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using Microsoft.DirectX.Direct3D;
using Programs.Utils;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// The GUIPrograms plugin is used to list a collection of arbitrary files
  /// and use them as arguments when launching external applications.
  /// </summary>
  /// 
  public class GUIPrograms: GUIWindow
  {

    #region Serialisation

    [Serializable]
    public class MapSettings
    {
      protected int _SortBy;
      protected int _ViewAs;
      protected bool _SortAscending;
      protected int _LastAppID;
      protected int _LastFileID;
      protected bool _OverviewVisible;

      public MapSettings()
      {
        _SortBy = 0; //name
        _ViewAs = 0; //list
        _SortAscending = true;
        _OverviewVisible = true;
        _LastAppID =  - 1;
        _LastFileID =  - 1;
      }


      [XmlElement("SortBy")]
      public int SortBy
      {
        get
        {
          return _SortBy;
        }
        set
        {
          _SortBy = value;
        }
      }

      [XmlElement("ViewAs")]
      public int ViewAs
      {
        get
        {
          return _ViewAs;
        }
        set
        {
          _ViewAs = value;
        }
      }

      [XmlElement("SortAscending")]
      public bool SortAscending
      {
        get
        {
          return _SortAscending;
        }
        set
        {
          _SortAscending = value;
        }
      }

      [XmlElement("OverviewVisible")]
      public bool OverviewVisible
      {
        get
        {
          return _OverviewVisible;
        }
        set
        {
          _OverviewVisible = value;
        }
      }

      [XmlElement("LastAppID")]
      public int LastAppID
      {
        get
        {
          return _LastAppID;
        }
        set
        {
          _LastAppID = value;
        }
      }

      [XmlElement("LastFileID")]
      public int LastFileID
      {
        get
        {
          return _LastFileID;
        }
        set
        {
          _LastFileID = value;
        }
      }

      public string ViewAsText
      {
        get
        {
          return GetViewAsText();
        }
      }

      public void SwitchToNextView()
      {
        switch ((View)ViewAs)
        {
          case View.VIEW_AS_LIST:
            ViewAs = (int)View.VIEW_AS_ICONS;
            break;
          case View.VIEW_AS_ICONS:
            ViewAs = (int)View.VIEW_AS_LARGEICONS;
            break;
          case View.VIEW_AS_LARGEICONS:
            ViewAs = (int)View.VIEW_AS_FILMSTRIP;
            break;
          case View.VIEW_AS_FILMSTRIP:
            ViewAs = (int)View.VIEW_AS_LIST;
            break;
        }
      }

      string GetViewAsText()
      {
        string result = "";
        switch ((View)ViewAs)
        {
          case View.VIEW_AS_LIST:
            result = GUILocalizeStrings.Get(101);
            break;
          case View.VIEW_AS_ICONS:
            result = GUILocalizeStrings.Get(100);
            break;
          case View.VIEW_AS_LARGEICONS:
            result = GUILocalizeStrings.Get(417);
            break;
          case View.VIEW_AS_FILMSTRIP:
            result = GUILocalizeStrings.Get(733);
            break;
        }
        return result;
      }
    }



    void SaveSettings()
    {
      using(Xml xmlwriter = new Xml("MediaPortal.xml"))
      {
        switch ((View)mapSettings.ViewAs)
        {
          case View.VIEW_AS_LIST:
            xmlwriter.SetValue("myprograms", "viewby", "list");
            break;
          case View.VIEW_AS_ICONS:
            xmlwriter.SetValue("myprograms", "viewby", "icons");
            break;
          case View.VIEW_AS_LARGEICONS:
            xmlwriter.SetValue("myprograms", "viewby", "largeicons");
            break;
          case View.VIEW_AS_FILMSTRIP:
            xmlwriter.SetValue("myprograms", "viewby", "filmstrip");
            break;
        }
        xmlwriter.SetValue("myprograms", "lastAppID", mapSettings.LastAppID.ToString());
        xmlwriter.SetValue("myprograms", "sortby", mapSettings.SortBy);
        // avoid bool conversion...... don't wanna know why it doesn't work! :-(
        if (mapSettings.SortAscending)
        {
          xmlwriter.SetValue("myprograms", "sortasc", "yes");
        }
        else
        {
          xmlwriter.SetValue("myprograms", "sortasc", "no");
        }

        if (mapSettings.OverviewVisible)
        {
          xmlwriter.SetValue("myprograms", "overviewvisible", "yes");
        }
        else
        {
          xmlwriter.SetValue("myprograms", "overviewvisible", "no");
        }

        //Log.Write("dw myPrograms: saving xmlsettings lastappid {0}", _MapSettings.LastAppID);
      }
    }

    void LoadSettings()
    {
      using(Xml xmlreader = new Xml("MediaPortal.xml"))
      {
        string curText = "";
        curText = (string)xmlreader.GetValue("myprograms", "viewby");
        if (curText != null)
        {
          if (curText == "list")
            mapSettings.ViewAs = (int)View.VIEW_AS_LIST;
          else if (curText == "icons")
            mapSettings.ViewAs = (int)View.VIEW_AS_ICONS;
          else if (curText == "largeicons")
            mapSettings.ViewAs = (int)View.VIEW_AS_LARGEICONS;
          else if (curText == "filmstrip")
            mapSettings.ViewAs = (int)View.VIEW_AS_FILMSTRIP;
        }

        mapSettings.LastAppID = xmlreader.GetValueAsInt("myprograms", "lastAppID",  - 1);
        mapSettings.SortBy = xmlreader.GetValueAsInt("myprograms", "sortby", 0);
        curText = (string)xmlreader.GetValue("myprograms", "sortasc");
        if (curText != null)
        {
          mapSettings.SortAscending = (curText.ToLower() == "yes");
        }
        else
        {
          mapSettings.SortAscending = true;
        }

        curText = (string)xmlreader.GetValue("myprograms", "overviewvisible");
        if (curText != null)
        {
          mapSettings.OverviewVisible = (curText.ToLower() == "yes");
        }
        else
        {
          mapSettings.OverviewVisible = true;
        }

      }
    }


    void LoadLastAppIDFromSettings()
    {
      using(Xml xmlreader = new Xml("MediaPortal.xml"))
      {
        mapSettings.LastAppID = xmlreader.GetValueAsInt("myprograms", "lastAppID",  - 1);
        mapSettings.SortBy = xmlreader.GetValueAsInt("myprograms", "sortby", 0);
        mapSettings.SortAscending = xmlreader.GetValueAsBool("myprograms", "sortasc", true);
        mapSettings.OverviewVisible = xmlreader.GetValueAsBool("myprograms", "overviewvisible", true);
      }
    }

    void LoadFolderSettings(string directoryName)
    {
      if (directoryName == "")
        directoryName = "root";
      object o;
      FolderSettings.GetFolderSetting(directoryName, "Programs", typeof(MapSettings), out o);
      if (o != null)
        mapSettings = o as MapSettings;
      if (mapSettings == null)
        mapSettings = new MapSettings();
    }

    void SaveFolderSettings(string directoryName)
    {
      if (directoryName == "")
        directoryName = "root";
      FolderSettings.AddFolderSetting(directoryName, "Programs", typeof(MapSettings), mapSettings);
    }

    #endregion 

    #region SkinControls

    enum View
    {
      VIEW_AS_LIST = 0, VIEW_AS_ICONS = 1, VIEW_AS_LARGEICONS = 2, VIEW_AS_FILMSTRIP = 3, 
    } 

    // Labels
    [SkinControlAttribute(9)]
    protected GUILabelControl lblMyPrograms = null;
    [SkinControlAttribute(10)]
    protected GUILabelControl lblCurApp = null;

    // Buttons
    [SkinControlAttribute(2)]
    protected GUIButtonControl btnViewAs = null;
    [SkinControlAttribute(3)]
    protected GUIButtonControl btnRefresh = null;
    [SkinControlAttribute(4)]
    protected GUIButtonControl btnSortBy = null;
    [SkinControlAttribute(5)]
    protected GUIToggleButtonControl btnSortAsc = null;

    //Images                     
    [SkinControlAttribute(6)]
    protected GUIImage screenShotImage = null;


    // FacadeView
    const int cFacadeID = 7;
    [SkinControlAttribute(cFacadeID)]
    protected GUIFacadeControl facadeView = null;

    #endregion 

    #region Constructor / Destructor

    public GUIPrograms()
    {
      GetID = (int)Window.WINDOW_FILES;
      apps = ProgramDatabase.AppList;
      LoadSettings();
    }

    ~GUIPrograms()
    {
      SaveSettings();
      FolderSettings.DeleteFolderSetting("root", "Programs");
    }

    #endregion 

    #region Init / DeInit

    void DeInitMyPrograms()
    {
      SaveSettings();
      if (curTexture != null)
      {
        curTexture.Dispose();
        curTexture = null;
      }
      // make sure the selected index wasn't reseted already
      // and save the index only if it's non-zero
      // otherwise: DXDevice.Reset clears selection 
      int itemIndex = GetSelectedItemNo();
      if (itemIndex > 0)
      {
        selectedItemIndex = GetSelectedItemNo();
      }
    }

    void InitMyPrograms()
    {
      LoadFolderSettings("");
      LoadLastAppIDFromSettings(); // hacky load back the last app id, otherwise this can get lost from dx resets....
      lastApp = apps.GetAppByID(mapSettings.LastAppID);
      if (lastApp != null)
      {
        lastFilepath = lastApp.DefaultFilepath();
        lastApp.CurrentSortIndex = mapSettings.SortBy;
        lastApp.CurrentSortIsAscending = mapSettings.SortAscending;
      }
      else
      {
        lastFilepath = "";
      }
      UpdateListControl();
      ShowThumbPanel();
      curTexture = null;
//      slideTime = (int)(DateTime.Now.Ticks / 10000); // reset timer!
    }

    #endregion 

    #region Base & Content Variables

    static Applist apps = ProgramDatabase.AppList;
    MapSettings mapSettings = new MapSettings();
    AppItem lastApp = null;
    string lastFilepath = "";
    int selectedItemIndex =  - 1;
    int slideSpeed = 3; // speed in seconds between two slides
    int slideTime = 0;
    Texture curTexture = null;
    int textureWidth = 0;
    int textureHeight = 0;

    #endregion 

    #region Properties / Helper Routines

    GUIListItem GetSelectedItem()
    {
      return facadeView.SelectedListItem;
    }

    int GetSelectedItemNo()
    {
      return facadeView.SelectedListItemIndex;
    }

    int GetCurrentFatherID()
    {
      if (lastApp != null)
      {
        return lastApp.AppID;
      }
      else
      {
        return  - 1; // root
      }
    }

    #endregion 

    #region Overrides

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\myprograms.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      InitMyPrograms();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      DeInitMyPrograms();
      base.OnPageDestroy(newWindowId);
    }

    public override void Render(float timePassed)
    {
      base.Render(timePassed);
      RenderFilmStrip();
      RenderScreenShot();
    }

    void OnInfo()
    {
      // <F3> keypress
      if (null != lastApp)
      {
        selectedItemIndex = GetSelectedItemNo();
        GUIListItem item = GetSelectedItem();
        if (!item.Label.Equals(ProgramUtils.cBackLabel) && (!item.IsFolder))
        {
          // show file info but only if the selected item is not the back button
          bool ovVisible = mapSettings.OverviewVisible;
          lastApp.OnInfo(item, ref ovVisible); // quickndirty..
          mapSettings.OverviewVisible = ovVisible;
          UpdateListControl();
        }
      }
    }


    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnViewAs)
      {
        mapSettings.SwitchToNextView();
        ShowThumbPanel();
        GUIControl.FocusControl(GetID, control.GetID);
      }
      else if (control == btnSortBy)
      {
        // get next sort method...
        if (lastApp != null)
        {
          lastApp.OnSort(facadeView, true);
          mapSettings.SortBy = lastApp.CurrentSortIndex;
          UpdateButtons();
        }
        GUIControl.FocusControl(GetID, control.GetID);
      }
      else if (control == btnSortAsc)
      {
        // toggle asc / desc for current sort method...
        if (lastApp != null)
        {
          lastApp.OnSortToggle(facadeView);
          mapSettings.SortAscending = lastApp.CurrentSortIsAscending;
          UpdateButtons();
        }
        GUIControl.FocusControl(GetID, control.GetID);
      }
      else if (control == btnRefresh)
      {
        if (lastApp != null)
        {
          lastApp.Refresh(true);
          lastFilepath = lastApp.DefaultFilepath();
          UpdateListControl();
        }
      }
      else if (control == facadeView)
      {
        // application or file-item was clicked....
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick();
        }
      }
    }


    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
        case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
        {
          int iControl=message.SenderControlId;
          if (iControl==cFacadeID)
          {
            if (lastApp != null)
            {
              lastApp.ResetThumbs();
            }
          }
        }
          break;
      }
      return base.OnMessage(message);
    }



    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        // <U> keypress
        BackItemClicked();
        UpdateButtons();
        return ;
      }

      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        // <ESC> keypress in some myProgram Menu => jump to main menu
        SaveFolderSettings("");
        GUIWindowManager.ShowPreviousWindow();
        return ;
      }

      if (action.wID == Action.ActionType.ACTION_SHOW_INFO)
      {
        OnInfo();
        return ;
      }
      base.OnAction(action);
    }

    #endregion 

    #region Display

    void UpdateButtons()
    {
      btnRefresh.IsVisible = RefreshButtonVisible();

      // display apptitle if available.....
      if (lastApp != null)
      {
        lblMyPrograms.IsVisible = false;
        lblCurApp.Label = lastApp.Title;
        lblCurApp.IsVisible = true;
        btnSortBy.IsVisible = true;
        btnSortAsc.IsVisible = true;
      }
      else
      {
        lblCurApp.IsVisible = false;
        lblMyPrograms.IsVisible = true;
        btnSortBy.IsVisible = false;
        btnSortAsc.IsVisible = false;
      }

      btnViewAs.Label = mapSettings.ViewAsText;

      if (lastApp != null)
      {
        btnSortBy.Label = lastApp.CurrentSortTitle();
        btnSortAsc.Selected = lastApp.CurrentSortIsAscending;
      }
    }

    void ShowThumbPanel()
    {
      int itemIndex = GetSelectedItemNo();
      if (mapSettings.ViewAs == (int)View.VIEW_AS_LARGEICONS)
      {
        facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
      }
      else if (mapSettings.ViewAs == (int)View.VIEW_AS_ICONS)
      {
        facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
      }
      else if (mapSettings.ViewAs == (int)View.VIEW_AS_LIST)
      {
        facadeView.View = GUIFacadeControl.ViewMode.List;
      }
      else if (mapSettings.ViewAs == (int)View.VIEW_AS_FILMSTRIP)
      {
        facadeView.View = GUIFacadeControl.ViewMode.Filmstrip;
      }
      if (itemIndex >  - 1)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, itemIndex);
      }
      UpdateButtons();
    }


    void RenderScreenShot()
    {
      if (mapSettings == null)
        return ;
      if (mapSettings.ViewAs == (int)View.VIEW_AS_LIST)
      {
        // does the thumb needs replacing??
        int timeElapsed = ((int)(DateTime.Now.Ticks / 10000)) - slideTime;
        if (timeElapsed >= (slideSpeed * 1000))
        {
          RefreshScreenShot(); 
          // only refresh the picture, don't refresh the other data otherwise scrolling of labels is interrupted!
        }

        if ((screenShotImage != null) && (curTexture != null))
        {
          float x = (float)screenShotImage.XPosition;
          float y = (float)screenShotImage.YPosition;
          int curWidth;
          int curHeight;
          GUIGraphicsContext.Correct(ref x, ref y);

          int maxWidth = screenShotImage.Width;
          int maxHeight = screenShotImage.Height;
          GUIGraphicsContext.GetOutputRect(textureWidth, textureHeight, maxWidth, maxHeight, out curWidth, out curHeight);
          GUIFontManager.Present();
          int deltaX = ((screenShotImage.Width - curWidth) / 2);
          if (deltaX < 0)
          {
            deltaX = 0;
          }
          int deltaY = ((screenShotImage.Height - curHeight) / 2);
          if (deltaY < 0)
          {
            deltaY = 0;
          }
          x = x + deltaX;
          y = y + deltaY;
          Picture.RenderImage(ref curTexture, (int)x, (int)y, curWidth, curHeight, textureWidth, textureHeight, 0, 0, true);
        }
      }
    }

    void RenderFilmStrip()
    {
      // in filmstrip mode, start a slideshow if more than one
      // pic is available for the selected item
      if (facadeView == null)
        return ;
      if (facadeView.FilmstripView == null)
        return ;
      if (facadeView.FilmstripView.InfoImageFileName == "")
        return ;
      if (mapSettings == null)
        return ;
      if (mapSettings.ViewAs == (int)View.VIEW_AS_FILMSTRIP)
      {
        // does the thumb needs replacing??
        int timeElapsed = ((int)(DateTime.Now.Ticks / 10000)) - slideTime;
        if (timeElapsed >= (slideSpeed * 1000))
        {
          RefreshFilmstripThumb(facadeView.FilmstripView); 
            // only refresh the picture, don't refresh the other data otherwise scrolling of labels is interrupted!
        }
      }
    }


    void RefreshScreenShot()
    {
      GUIListItem item = GetSelectedItem();
      if (curTexture != null)
      {
        curTexture.Dispose();
        curTexture = null;
      }
      // some preconditions...
      if (lastApp == null)
        return ;
      if (item.MusicTag == null)
        return ;
      if (!(item.MusicTag is FileItem))
        return ;
      FileItem curFile = item.MusicTag as FileItem;
      // ok... let's get a filename
      string thumbFilename = lastApp.GetCurThumb(curFile);
      if (System.IO.File.Exists(thumbFilename))
      {
        curTexture = Picture.Load(thumbFilename, 0, 512, 512, true, false, out textureWidth, out textureHeight);
      }
      lastApp.NextThumb(); // try to find a next thumbnail
      slideTime = (int)(DateTime.Now.Ticks / 10000); // reset timer!
    }

    void RefreshFilmstripThumb(GUIFilmstripControl pControl)
    {
      GUIListItem item = GetSelectedItem();
      // some preconditions...
      if (lastApp == null)
        return ;
      if (item.MusicTag == null)
        return ;
      if (!(item.MusicTag is FileItem))
        return ;
      FileItem curFile = item.MusicTag as FileItem;
      // ok... let's get a filename
      string thumbFilename = lastApp.GetCurThumb(curFile);
      if (File.Exists(thumbFilename))
      {
        pControl.InfoImageFileName = thumbFilename;
      }
      lastApp.NextThumb(); // try to find a next thumbnail
      slideTime = (int)(DateTime.Now.Ticks / 10000); // reset timer!
    }

    bool RefreshButtonVisible()
    {
      if (lastApp == null)
      {
        return false;
      }
      else
      {
        return (lastApp.RefreshButtonVisible() && lastApp.GUIRefreshPossible && lastApp.EnableGUIRefresh);
      }
    }

    bool thereAreAppsToDisplay()
    {
      if (lastApp == null)
      {
        return true; // root has apps
      }
      else
      {
        return lastApp.SubItemsAllowed(); // grouper items for example
      }
    }

    bool thereAreFilesOrLinksToDisplay()
    {
      return (lastApp != null); // all apps can have files except the root
    }

    bool isBackButtonNecessary()
    {
      return (lastApp != null); // always show back button except for root
    }

    void UpdateListControl()
    {
      int TotalItems = 0;
      GUIControl.ClearControl(GetID, facadeView.GetID);
      if (isBackButtonNecessary())
      {
        ProgramUtils.AddBackButton(facadeView);
      }

      if (thereAreAppsToDisplay())
      {
        TotalItems = TotalItems + DisplayApps();
      }

      if (thereAreFilesOrLinksToDisplay())
      {
        TotalItems = TotalItems + DisplayFiles();
      }

      if (lastApp != null)
      {
        lastApp.OnSort(facadeView, false);
      }


      string itemCountText = String.Format("{0} {1}", TotalItems, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount", itemCountText);

      if (selectedItemIndex >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, selectedItemIndex);
      }
    }


    int DisplayFiles()
    {
      int totalFiles = 0;
      if (lastApp == null)
      {
        return totalFiles;
      }
      totalFiles = lastApp.DisplayFiles(this.lastFilepath, facadeView);
      return (totalFiles);
    }

    int DisplayApps()
    {
      int totalApps = 0;
      foreach (AppItem app in apps.appsOfFatherID(GetCurrentFatherID()))
      {
        if (app.Enabled)
        {
          totalApps = totalApps + 1;
          GUIListItem item = new GUIListItem(app.Title);
          if (app.Imagefile != "")
          {
            item.ThumbnailImage = app.Imagefile;
            item.IconImageBig = app.Imagefile;
            item.IconImage = app.Imagefile;
          }
          else
          {
            item.ThumbnailImage = GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png";
            item.IconImageBig = GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png";
            item.IconImage = GUIGraphicsContext.Skin + @"\media\DefaultFolderNF.png";
          }
          item.MusicTag = app;
          item.IsFolder = true; // pseudo-folder....
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(OnItemSelected);
          facadeView.Add(item);
        }
      }
      return (totalApps);
    }

    #endregion 

    #region EventHandlers

    void FileItemClicked(GUIListItem item)
    {
      // file item was clicked => launch it!
      if (lastApp != null)
      {
        mapSettings.LastAppID = lastApp.AppID;
        lastFilepath = lastApp.DefaultFilepath();
        //        Log.Write("dw myPrograms: FileItemClicked: lastAppID changes to {0} {1}", _MapSettings.LastAppID, lastApp.Title);

        lastApp.LaunchFile(item);
      }
    }

    void FolderItemClicked(GUIListItem item)
    {
      if (item.MusicTag != null)
      {
        if (item.MusicTag is AppItem)
        {
          bool bPinOk = true;
          AppItem candidate = (AppItem)item.MusicTag;
          if (candidate.Pincode > 0)
          {
            bPinOk = candidate.CheckPincode();
          }
          if (bPinOk)
          {
            lastApp = candidate;
            mapSettings.LastAppID = lastApp.AppID;
            lastFilepath = lastApp.DefaultFilepath();
            //            Log.Write("dw myPrograms: FolderItemClicked: lastAppID changes to {0} {1}", _MapSettings.LastAppID, lastApp.Title);

          }
        }
        else if (item.MusicTag is FileItem)
        {
          // example: subfolder in directory-cache mode
          // => set filepath which will be a search criteria for sql / browse
          if (lastFilepath == "")
          {
            // first subfolder
            lastFilepath = lastApp.FileDirectory + "\\" + item.Label;
          }
          else
          {
            // subsequent subfolder
            lastFilepath = lastFilepath + "\\" + item.Label;
          }
        }
        UpdateListControl();
      }
      else
      {
        // tag is null
        // example: subfolder in directory-browse mode
        lastFilepath = item.Path;
        UpdateListControl();
      }
    }

    void BackItemClicked()
    {
      if (lastApp != null)
      {
        // debug: Log.Write("lastFilepath {0} / lastApp.FileDirectory {1} / lastApp.Title {2}", this.lastFilepath, lastApp.FileDirectory, lastApp.Title);
        if ((lastFilepath != null) && (lastFilepath != "") && (lastFilepath != lastApp.FileDirectory))
        {
          // back item in flielist clicked
          string newFilepath = Path.GetDirectoryName(lastFilepath);
          lastFilepath = newFilepath;
        }
        else
        {
          // back item in application list clicked
          // go to father item
          lastApp = apps.GetAppByID(lastApp.FatherID);
          if (lastApp != null)
          {
            mapSettings.LastAppID = lastApp.AppID;
            lastFilepath = lastApp.DefaultFilepath();
            //            Log.Write("dw myPrograms: BackItemClicked 1: lastAppID changes to {0} {1}", _MapSettings.LastAppID, lastApp.Title);
          }
          else
          {
            // back to home screen.....
            mapSettings.LastAppID =  - 1;
            lastFilepath = "";
            //            Log.Write("dw myPrograms: BackItemClicked 2: lastAppID changes to {0}", _MapSettings.LastAppID);
          }
        }
        UpdateListControl();
      }
      else
      {
        // from root.... go back to main menu
        GUIWindowManager.ShowPreviousWindow();
      }


    }

    void OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
      if (filmstrip == null)
        return ;
      string thumbName = "";
      if ((item.ThumbnailImage != GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png") && (item.ThumbnailImage != ""))
      {
        // only show big thumb if there is really one....
        thumbName = item.ThumbnailImage;
      }
      filmstrip.InfoImageFileName = thumbName;
    }


    void OnClick()
    {
      GUIListItem item = GetSelectedItem();
      if (!item.IsFolder)
      {
        selectedItemIndex = GetSelectedItemNo();
        // non-folder item clicked => always a fileitem!
        FileItemClicked(item);
      }
      else
      {
        // folder-item clicked.... 
        selectedItemIndex =  - 1;
        if (item.Label.Equals(ProgramUtils.cBackLabel))
        {
          BackItemClicked();
          UpdateButtons();
        }
        else
        {
          // application-item or subfolder
          FolderItemClicked(item);
          UpdateButtons();
        }
      }
    }

    #endregion 
  }
}
