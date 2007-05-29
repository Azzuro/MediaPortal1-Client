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
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Xml.Serialization;
using Core.Util;

using MediaPortal.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Services;
using MediaPortal.Player;
using MediaPortal.Threading;
using MediaPortal.Configuration;
using MediaPortal.Picture.Database;


namespace MediaPortal.GUI.Pictures
{
  #region ThumbCacher class
  public class MissingThumbCacher
  {
    string _filepath = String.Empty;
    bool _createLarge = true;
    //bool _hideFileExtensions = true;
    Work work;

    public MissingThumbCacher(string Filepath, bool CreateLargeThumbs)
    {
      _filepath = Filepath;
      _createLarge = CreateLargeThumbs;
      //_hideFileExtensions = HideExtensions;

      work = new Work(new DoWorkHandler(this.PerformRequest));
      work.ThreadPriority = ThreadPriority.Lowest;
      GlobalServiceProvider.Get<IThreadPool>().Add(work, QueuePriority.Low);
    }

    /// <summary>
    /// searches for folder.jpg in the mp3 directory and creates cached thumbs in MP's thumbs\folder dir
    /// </summary>
    void PerformRequest()
    {
      VirtualDirectory virtualDirectory = new VirtualDirectory();
      string path = _filepath;
      bool autocreateLargeThumbs = _createLarge;

      if (!virtualDirectory.IsRemote(path))
      {
        List<GUIListItem> itemlist = virtualDirectory.GetDirectoryUnProtectedExt(path, false);

        foreach (GUIListItem item in itemlist)
        {
          //if (currentFolder != path)
          //  return;
          //if (GUIWindowManager.ActiveWindow != GetID)
          //  return;
          if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
            return;
          if (!item.IsFolder)
          {
            if (!item.IsRemote && MediaPortal.Util.Utils.IsPicture(item.Path))
            {
              using (PictureDatabase dbs = new PictureDatabase())
              {
                string thumbnailImage = String.Format(@"{0}\{1}.jpg", Thumbs.Pictures, MediaPortal.Util.Utils.EncryptLine(item.Path));
                if (!System.IO.File.Exists(thumbnailImage))
                {
                  int iRotate = dbs.GetRotation(item.Path);
                  Util.Picture.CreateThumbnail(item.Path, thumbnailImage, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, iRotate);
                  System.Threading.Thread.Sleep(25);
                  Log.Info("GUIPictures: On-Demand-Creation of missing thumb successful for {0}", item.Path);
                }

                if (autocreateLargeThumbs)
                {
                  thumbnailImage = String.Format(@"{0}\{1}L.jpg", Thumbs.Pictures, MediaPortal.Util.Utils.EncryptLine(item.Path));
                  if (!System.IO.File.Exists(thumbnailImage))
                  {
                    int iRotate = dbs.GetRotation(item.Path);
                    Util.Picture.CreateThumbnail(item.Path, thumbnailImage, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, iRotate);
                    System.Threading.Thread.Sleep(25);
                    Log.Info("GUIPictures: On-Demand-Creation of missing large thumb successful for {0}", item.Path);
                  }
                }
              }
            }
          }
          //else
          //{
          //  int pin;
          //  if ((item.Label != "..") && (!virtualDirectory.IsProtectedShare(item.Path, out pin)))
          //  {
          //    string thumbnailImage = item.Path + @"\folder.jpg";
          //    if (!item.IsRemote && !System.IO.File.Exists(thumbnailImage))
          //    {
          //      CreateFolderThumb(item.Path);
          //      System.Threading.Thread.Sleep(25);
          //    }
          //  }
          //}
        } //foreach (GUIListItem item in itemlist)
      }
    }
  }
  #endregion

  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUIPictures : GUIWindow, IComparer<GUIListItem>, ISetupForm, IShowPlugin
  {
    #region MapSettings class
    [Serializable]
    public class MapSettings
    {
      protected int _SortBy;
      protected int _ViewAs;
      protected bool _SortAscending;

      public MapSettings()
      {
        // Set default view
        _SortBy = (int)SortMethod.Name;
        _ViewAs = (int)View.Icons;
        _SortAscending = true;
      }

      [XmlElement("SortBy")]
      public int SortBy
      {
        get { return _SortBy; }
        set { _SortBy = value; }
      }

      [XmlElement("ViewAs")]
      public int ViewAs
      {
        get { return _ViewAs; }
        set { _ViewAs = value; }
      }

      [XmlElement("SortAscending")]
      public bool SortAscending
      {
        get { return _SortAscending; }
        set { _SortAscending = value; }
      }
    }
    #endregion

    #region Base variables
    enum SortMethod
    {
      Name = 0,
      Date = 1,
      Size = 2
    }

    enum View
    {
      List = 0,
      Icons = 1,
      BigIcons = 2,
      Albums = 3,
      Filmstrip = 4,
    }

    enum Display
    {
      Files = 0,
      Date = 1
    }
    
    [SkinControlAttribute(2)]    protected GUIButtonControl btnViewAs = null;
    [SkinControlAttribute(3)]    protected GUISortButtonControl btnSortBy = null;
    [SkinControlAttribute(4)]    protected GUIButtonControl btnSwitchView = null;
    [SkinControlAttribute(6)]    protected GUIButtonControl btnSlideShow = null;
    [SkinControlAttribute(7)]    protected GUIButtonControl btnSlideShowRecursive = null;
    [SkinControlAttribute(50)]   protected GUIFacadeControl facadeView = null;

    const int MAX_PICS_PER_DATE = 1000;

    int selectedItemIndex = -1;
    GUIListItem selectedListItem = null;
    DirectoryHistory folderHistory = new DirectoryHistory();
    string currentFolder = String.Empty;
    string m_strDirectoryStart = String.Empty;
    string destinationFolder = String.Empty;
    VirtualDirectory virtualDirectory = new VirtualDirectory();
    MapSettings mapSettings = new MapSettings();
    bool isFileMenuEnabled = false;
    string fileMenuPinCode = String.Empty;
    bool _autocreateLargeThumbs = true;
    //bool _hideExtensions = true;
    Display disp = Display.Files;
    

    #endregion

    #region ctor/dtor
    public GUIPictures()
    {
      GetID = (int)GUIWindow.Window.WINDOW_PICTURES;

      virtualDirectory.AddDrives();
      virtualDirectory.SetExtensions(MediaPortal.Util.Utils.PictureExtensions);
    }
    ~GUIPictures()
    {
      SaveSettings();
    }

    #endregion

    #region Serialisation
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        isFileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);
        fileMenuPinCode = MediaPortal.Util.Utils.DecryptPin(xmlreader.GetValueAsString("filemenu", "pincode", String.Empty));
        string strDefault = xmlreader.GetValueAsString("pictures", "default", String.Empty);
        virtualDirectory.Clear();
        for (int i = 0; i < 20; i++)
        {
          string shareName = String.Format("sharename{0}", i);
          string sharePath = String.Format("sharepath{0}", i);
          string strPincode = String.Format("pincode{0}", i);

          string shareType = String.Format("sharetype{0}", i);
          string shareServer = String.Format("shareserver{0}", i);
          string shareLogin = String.Format("sharelogin{0}", i);
          string sharePwd = String.Format("sharepassword{0}", i);
          string sharePort = String.Format("shareport{0}", i);
          string remoteFolder = String.Format("shareremotepath{0}", i);
          string shareViewPath = String.Format("shareview{0}", i);

          Share share = new Share();
          share.Name = xmlreader.GetValueAsString("pictures", shareName, String.Empty);
          share.Path = xmlreader.GetValueAsString("pictures", sharePath, String.Empty);
          string pinCode = MediaPortal.Util.Utils.DecryptPin(xmlreader.GetValueAsString("pictures", strPincode, string.Empty));
          if (pinCode != string.Empty)
            share.Pincode = Convert.ToInt32(pinCode);
          else
            share.Pincode = -1;

          share.IsFtpShare = xmlreader.GetValueAsBool("pictures", shareType, false);
          share.FtpServer = xmlreader.GetValueAsString("pictures", shareServer, String.Empty);
          share.FtpLoginName = xmlreader.GetValueAsString("pictures", shareLogin, String.Empty);
          share.FtpPassword = xmlreader.GetValueAsString("pictures", sharePwd, String.Empty);
          share.FtpPort = xmlreader.GetValueAsInt("pictures", sharePort, 21);
          share.FtpFolder = xmlreader.GetValueAsString("pictures", remoteFolder, "/");
          share.DefaultView = (Share.Views)xmlreader.GetValueAsInt("pictures", shareViewPath, (int)Share.Views.List);

          if (share.Name.Length > 0)
          {
            if (strDefault == share.Name)
            {
              share.Default = true;
              if (currentFolder.Length == 0)
              {
                if (share.IsFtpShare)
                {
                  //remote:hostname?port?login?password?folder
                  currentFolder = virtualDirectory.GetShareRemoteURL(share);
                  m_strDirectoryStart = currentFolder;
                }
                else
                {
                  currentFolder = share.Path;
                  m_strDirectoryStart = share.Path;
                }
              }
            }
            virtualDirectory.Add(share);
          }
          else
            break;
        }
        if (xmlreader.GetValueAsBool("pictures", "rememberlastfolder", false))
        {
          string lastFolder = xmlreader.GetValueAsString("pictures", "lastfolder", currentFolder);
          if (lastFolder != "root")
            currentFolder = lastFolder;
        }
        _autocreateLargeThumbs = !xmlreader.GetValueAsBool("thumbnails", "picturenolargethumbondemand", false);
        //_hideExtensions = xmlreader.GetValueAsBool("general", "hideextensions", true);
      }
    }

    void SaveSettings()
    {
    }
    #endregion

    #region overrides
    public override bool Init()
    {
      currentFolder = String.Empty;
      destinationFolder = String.Empty;

      bool result = Load(GUIGraphicsContext.Skin + @"\mypics.xml");
      LoadSettings();
      return result;
    }


    public override void OnAction(Action action)
    {

      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (facadeView.Focus)
        {
          GUIListItem item = facadeView[0];
          if (item != null)
          {
            if (item.IsFolder && item.Label == "..")
            {
              if (currentFolder != m_strDirectoryStart)
              {
                LoadDirectory(item.Path);
                return;
              }
            }
          }
        }
      }
      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = GetItem(0);
        if (item != null)
        {
          if (item.IsFolder && item.Label == "..")
          {
            LoadDirectory(item.Path);
          }
        }
        return;
      }
      if (action.wID == Action.ActionType.ACTION_DELETE_ITEM)
      {
        // delete current picture
        GUIListItem item = GetSelectedItem();
        if (item != null)
        {
          if (item.IsFolder == false)
          {
            OnDeleteItem(item);
          }
        }
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      if (!KeepVirtualDirectory(PreviousWindowId))
      {
        virtualDirectory.Reset();
      }
      base.OnPageLoad();
      GUITextureManager.CleanupThumbs();
      LoadSettings();
      LoadFolderSettings(currentFolder);
      ShowThumbPanel();
      LoadDirectory(currentFolder);
      if (selectedItemIndex >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, selectedItemIndex);
      }

      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
    }
    protected override void OnPageDestroy(int newWindowId)
    {
      selectedItemIndex = GetSelectedItemNo();
      SaveSettings();
      SaveFolderSettings(currentFolder);
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnViewAs)
      {
        switch ((View)mapSettings.ViewAs)
        {
          case View.List:
            mapSettings.ViewAs = (int)View.Icons;
            break;
          case View.Icons:
            mapSettings.ViewAs = (int)View.BigIcons;
            break;
          case View.BigIcons:
            mapSettings.ViewAs = (int)View.Filmstrip;
            break;
          case View.Albums:
            mapSettings.ViewAs = (int)View.Filmstrip;
            break;

          case View.Filmstrip:
            mapSettings.ViewAs = (int)View.List;
            break;
        }
        ShowThumbPanel();
        GUIControl.FocusControl(GetID, control.GetID);
      }
      if (control == btnSortBy) // sort by
      {
        OnShowSortMenu();
      }

      if (control == facadeView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, facadeView.GetID, 0, 0, null);
        OnMessage(msg);
        int itemIndex = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SHOW_INFO)
        {
          if (virtualDirectory.IsRemote(currentFolder))
            return;
          OnInfo(itemIndex);
        }
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(itemIndex);
        }
        if (actionType == Action.ActionType.ACTION_QUEUE_ITEM)
        {
          if (virtualDirectory.IsRemote(currentFolder))
            return;
          OnQueueItem(itemIndex);
        }
      }
      else if (control == btnSlideShow) // Slide Show
      {
        OnSlideShow();
      }
      else if (control == btnSlideShowRecursive) // Recursive Slide Show
      {
        OnSlideShowRecursive();
      }
      else if (control == btnSwitchView) // Switch View
      {
        OnSwitchView();
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_START_SLIDESHOW:
          {
            string strUrl = message.Label;
            LoadDirectory(strUrl);
            OnSlideShow();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_AUTOPLAY_VOLUME:
          currentFolder = message.Label;
          OnSlideShowRecursive();
          break;

        case GUIMessage.MessageType.GUI_MSG_SHOW_DIRECTORY:
          currentFolder = message.Label;
          LoadDirectory(currentFolder);
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING:
          GUIFacadeControl pControl = (GUIFacadeControl)GetControl(facadeView.GetID);
          pControl.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED:
          GUIFacadeControl pControl2 = (GUIFacadeControl)GetControl(facadeView.GetID);
          pControl2.OnMessage(message);
          break;

        case GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED:
        case GUIMessage.MessageType.GUI_MSG_VOLUME_REMOVED:
          if (currentFolder == String.Empty || currentFolder.Substring(0, 2) == message.Label)
          {
            currentFolder = String.Empty;
            LoadDirectory(currentFolder);
          }
          break;

      }
      return base.OnMessage(message);
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem item = GetSelectedItem();
      selectedListItem = item;
      int itemNo = GetSelectedItemNo();
      selectedItemIndex = itemNo;

      if (item == null)
        return;
      if (item.IsFolder && item.Label == "..")
        return;

      GUIControl cntl = GetControl(facadeView.GetID);
      if (cntl == null)
        return; // Control not found

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(498); // menu
      if (!item.IsFolder)
      {
        dlg.AddLocalizedString(735); //rotate
        dlg.AddLocalizedString(923); //show
        dlg.AddLocalizedString(108); //start slideshow
        dlg.AddLocalizedString(940); //properties
      }
      else
      {
        dlg.AddLocalizedString(200046); //Generate Thumbnails
        dlg.AddLocalizedString(200047); //Recursive Generate Thumbnails
        dlg.AddLocalizedString(200048); //Regenerate Thumbnails
      }
      dlg.AddLocalizedString(457); //Switch View
      int iPincodeCorrect;
      if (!virtualDirectory.IsProtectedShare(item.Path, out iPincodeCorrect) && !item.IsRemote && isFileMenuEnabled)
        dlg.AddLocalizedString(500); // FileMenu      

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;
      switch (dlg.SelectedId)
      {
        case 735: // rotate
          OnRotatePicture();
          break;

        case 923: // show
          OnClick(itemNo);
          break;

        case 108: // start slideshow
          OnSlideShow(itemNo);
          break;

        case 940: // properties
          OnInfo(itemNo);
          break;

        case 500: // File menu
          {
            // get pincode
            if (fileMenuPinCode != String.Empty)
            {
              string strUserCode = String.Empty;
              if (GetUserInputString(ref strUserCode) && strUserCode == fileMenuPinCode)
              {
                OnShowFileMenu();
              }
            }
            else
              OnShowFileMenu();
          }
          break;
        case 200046: // Generate Thumbnails
          {
            if (item.IsFolder)
              OnCreateAllThumbs(item.Path, false, false);
          }
          break;
        case 200047: // Revursive Generate Thumbnails
          {
            if (item.IsFolder)
              OnCreateAllThumbs(item.Path, false, true);
          }
          break;
        case 200048: // Regenerate Thumbnails
          {
            if (item.IsFolder)
              OnCreateAllThumbs(item.Path, true, true);
          }
          break;
        case 457: // Test change view
          OnSwitchView();
          break;

      }
    }

    #endregion

    #region listview management

    bool ViewByIcon
    {
      get
      {
        if (mapSettings.ViewAs != (int)View.List)
          return true;
        return false;
      }
    }

    bool ViewByLargeIcon
    {
      get
      {
        if (mapSettings.ViewAs == (int)View.BigIcons)
          return true;
        return false;
      }
    }

    GUIListItem GetSelectedItem()
    {
      return facadeView.SelectedListItem;
    }

    GUIListItem GetItem(int itemIndex)
    {
      if (itemIndex >= facadeView.Count || itemIndex < 0)
        return null;
      return facadeView[itemIndex];
    }

    int GetSelectedItemNo()
    {
      return facadeView.SelectedListItemIndex;
    }

    int GetItemCount()
    {
      return facadeView.Count;
    }

    void UpdateButtonStates()
    {
      GUIControl.HideControl(GetID, facadeView.GetID);
      int iControl = facadeView.GetID;
      GUIControl.ShowControl(GetID, iControl);
      GUIControl.FocusControl(GetID, iControl);

      string textLine = String.Empty;
      View view = (View)mapSettings.ViewAs;
      SortMethod method = (SortMethod)mapSettings.SortBy;
      bool sortAsc = mapSettings.SortAscending;
      switch (view)
      {
        case View.List:
          textLine = GUILocalizeStrings.Get(101);
          break;
        case View.Icons:
          textLine = GUILocalizeStrings.Get(100);
          break;
        case View.BigIcons:
          textLine = GUILocalizeStrings.Get(417);
          break;
        case View.Albums:
          textLine = GUILocalizeStrings.Get(417);
          break;
        case View.Filmstrip:
          textLine = GUILocalizeStrings.Get(733);
          break;
      }
      GUIControl.SetControlLabel(GetID, btnViewAs.GetID, textLine);

      switch (method)
      {
        case SortMethod.Name:
          textLine = GUILocalizeStrings.Get(103);
          break;
        case SortMethod.Date:
          textLine = GUILocalizeStrings.Get(104);
          break;
        case SortMethod.Size:
          textLine = GUILocalizeStrings.Get(105);
          break;
      }
      GUIControl.SetControlLabel(GetID, btnSortBy.GetID, textLine);
      btnSortBy.IsAscending = sortAsc;
    }

    void ShowThumbPanel()
    {
      int itemIndex = GetSelectedItemNo();
      if (mapSettings.ViewAs == (int)View.BigIcons)
      {
        facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
      }
      else if (mapSettings.ViewAs == (int)View.Albums)
      {
        facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
      }
      else if (mapSettings.ViewAs == (int)View.Icons)
      {
        facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
      }
      else if (mapSettings.ViewAs == (int)View.List)
      {
        facadeView.View = GUIFacadeControl.ViewMode.List;
      }
      else if (mapSettings.ViewAs == (int)View.Filmstrip)
      {
        facadeView.View = GUIFacadeControl.ViewMode.Filmstrip;
      }
      if (itemIndex > -1)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, itemIndex);
      }
      UpdateButtonStates();
    }

    #endregion

    #region folder settings
    void LoadFolderSettings(string folderName)
    {
      if (folderName == String.Empty)
        folderName = "root";
      object o;
      FolderSettings.GetFolderSetting(folderName, "Pictures", typeof(GUIPictures.MapSettings), out o);
      if (o != null)
      {
        mapSettings = o as MapSettings;
        if (mapSettings == null)
          mapSettings = new MapSettings();
      }
      else
      {
        Share share = virtualDirectory.GetShare(folderName);
        if (share != null)
        {
          if (mapSettings == null)
            mapSettings = new MapSettings();
          mapSettings.ViewAs = (int)share.DefaultView;
        }
      }
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        if (xmlreader.GetValueAsBool("pictures", "rememberlastfolder", false))
          xmlreader.SetValue("pictures", "lastfolder", folderName);
    }

    void SaveFolderSettings(string folder)
    {
      if (folder == String.Empty)
        folder = "root";
      FolderSettings.AddFolderSetting(folder, "Pictures", typeof(GUIPictures.MapSettings), mapSettings);
    }
    #endregion

    #region Sort Members
    void OnSort()
    {
      facadeView.Sort(this);
      UpdateButtonStates();
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2)
        return 0;
      if (item1 == null)
        return -1;
      if (item2 == null)
        return -1;
      if (item1.IsFolder && item1.Label == "..")
        return -1;
      if (item2.IsFolder && item2.Label == "..")
        return -1;
      if (item1.IsFolder && !item2.IsFolder)
        return -1;
      else if (!item1.IsFolder && item2.IsFolder)
        return 1;

      string sizeItem1 = String.Empty;
      string sizeItem2 = String.Empty;
      if (item1.FileInfo != null && !item1.IsFolder)
        sizeItem1 = MediaPortal.Util.Utils.GetSize(item1.FileInfo.Length);
      if (item2.FileInfo != null && !item1.IsFolder)
        sizeItem2 = MediaPortal.Util.Utils.GetSize(item2.FileInfo.Length);

      SortMethod method = (SortMethod)mapSettings.SortBy;
      bool sortAsc = mapSettings.SortAscending;

      switch (method)
      {
        case SortMethod.Name:
          item1.Label2 = sizeItem1;
          item2.Label2 = sizeItem2;

          if (sortAsc)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            return String.Compare(item2.Label, item1.Label, true);
          }


        case SortMethod.Date:
          if (item1.FileInfo == null)
            return -1;
          if (item2.FileInfo == null)
            return -1;

          item1.Label2 = item1.FileInfo.ModificationTime.ToShortDateString() + " " + item1.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          item2.Label2 = item2.FileInfo.ModificationTime.ToShortDateString() + " " + item2.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          if (sortAsc)
          {
            return DateTime.Compare(item1.FileInfo.ModificationTime, item2.FileInfo.ModificationTime);
          }
          else
          {
            return DateTime.Compare(item2.FileInfo.ModificationTime, item1.FileInfo.ModificationTime);
          }

        case SortMethod.Size:
          if (item1.FileInfo == null)
            return -1;
          if (item2.FileInfo == null)
            return -1;
          item1.Label2 = sizeItem1;
          item2.Label2 = sizeItem2;
          if (sortAsc)
          {
            return (int)(item1.FileInfo.Length - item2.FileInfo.Length);
          }
          else
          {
            return (int)(item2.FileInfo.Length - item1.FileInfo.Length);
          }
      }
      return 0;
    }
    #endregion

    #region onXXX methods
    void OnRetrieveCoverArt(GUIListItem item)
    {
      if (item.IsRemote)
        return;
      MediaPortal.Util.Utils.SetDefaultIcons(item);
      if (!item.IsFolder)
      {
        MediaPortal.Util.Utils.SetThumbnails(ref item);
        string thumbnailImage = GetThumbnail(item.Path);
        item.ThumbnailImage = thumbnailImage;
      }
      else
      {
        if (item.Label != "..")
        {
          int pin;
          if (!virtualDirectory.IsProtectedShare(item.Path, out pin))
          {
            MediaPortal.Util.Utils.SetThumbnails(ref item);
          }
        }
      }
    }

    void OnDeleteItem(GUIListItem item)
    {
      if (item.IsRemote)
        return;

      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo)
        return;
      string strFileName = System.IO.Path.GetFileName(item.Path);
      if (!item.IsFolder)
        dlgYesNo.SetHeading(664);
      else
        dlgYesNo.SetHeading(503);
      dlgYesNo.SetLine(1, strFileName);
      dlgYesNo.SetLine(2, String.Empty);
      dlgYesNo.SetLine(3, String.Empty);
      dlgYesNo.DoModal(GetID);

      if (!dlgYesNo.IsConfirmed)
        return;
      DoDeleteItem(item);

      selectedItemIndex = GetSelectedItemNo();
      if (selectedItemIndex > 0)
        selectedItemIndex--;
      LoadDirectory(currentFolder);
      if (selectedItemIndex >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, selectedItemIndex);
      }
    }

    void DoDeleteItem(GUIListItem item)
    {
      if (item.IsFolder)
      {
        if (item.Label != "..")
        {
          List<GUIListItem> items = new List<GUIListItem>();
          items = virtualDirectory.GetDirectoryUnProtectedExt(item.Path, false);
          foreach (GUIListItem subItem in items)
          {
            DoDeleteItem(subItem);
          }
          MediaPortal.Util.Utils.DirectoryDelete(item.Path);
        }
      }
      else if (!item.IsRemote)
      {
        MediaPortal.Util.Utils.FileDelete(item.Path);
      }
    }

    void OnInfo(int itemNumber)
    {
      GUIListItem item = GetItem(itemNumber);
      if (item == null)
        return;
      if (item.IsFolder || item.IsRemote)
        return;
      GUIDialogExif exifDialog = (GUIDialogExif)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_EXIF);
      exifDialog.FileName = item.Path;
      exifDialog.DoModal(GetID);
    }

    void OnRotatePicture()
    {
      GUIListItem item = GetSelectedItem();
      if (item == null)
        return;
      if (item.IsFolder)
        return;
      if (item.IsRemote)
        return;
      int rotate = 0;
      using (PictureDatabase dbs = new PictureDatabase())
      {
        rotate = dbs.GetRotation(item.Path);
        rotate++;
        if (rotate >= 4)
        {
          rotate = 0;
        }
        dbs.SetRotation(item.Path, rotate);
      }
      string thumbnailImage = GetThumbnail(item.Path);
      Util.Picture.CreateThumbnail(item.Path, thumbnailImage, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, rotate);

      thumbnailImage = GetLargeThumbnail(item.Path);
      Util.Picture.CreateThumbnail(item.Path, thumbnailImage, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, rotate);
      System.Threading.Thread.Sleep(50);
      GUIControl.RefreshControl(GetID, facadeView.GetID);
    }

    void OnClick(int itemIndex)
    {
      GUIListItem item = GetSelectedItem();
      if (item == null)
        return;
      if (item.IsFolder)
      {
        selectedItemIndex = -1;
        LoadDirectory(item.Path);
      }
      else
      {
        if (virtualDirectory.IsRemote(item.Path))
        {
          if (!virtualDirectory.IsRemoteFileDownloaded(item.Path, item.FileInfo.Length))
          {
            if (!virtualDirectory.ShouldWeDownloadFile(item.Path))
              return;
            if (!virtualDirectory.DownloadRemoteFile(item.Path, item.FileInfo.Length))
            {
              //show message that we are unable to download the file
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, 0, 0, 0, 0, 0, 0);
              msg.Param1 = 916;
              msg.Param2 = 920;
              msg.Param3 = 0;
              msg.Param4 = 0;
              GUIWindowManager.SendMessage(msg);

              return;
            }
          }
          return;
        }

        selectedItemIndex = GetSelectedItemNo();
        OnShowPicture(item.Path);
      }
    }

    void OnQueueItem(int itemIndex)
    {
    }

    void OnShowPicture(string strFile)
    {
      GUISlideShow SlideShow = (GUISlideShow)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
      if (SlideShow == null)
        return;


      SlideShow.Reset();
      for (int i = 0; i < GetItemCount(); ++i)
      {
        GUIListItem item = GetItem(i);
        if (!item.IsFolder)
        {
          if (item.IsRemote)
            continue;
          SlideShow.Add(item.Path);
        }
      }
      if (SlideShow.Count > 0)
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
        SlideShow.Select(strFile);
      }
    }

    void AddDir(GUISlideShow SlideShow, string strDir)
    {
      List<GUIListItem> itemlist = virtualDirectory.GetDirectoryExt(strDir);
      Filter(ref itemlist);
      foreach (GUIListItem item in itemlist)
      {
        if (item.IsFolder)
        {
          if (item.Label != "..")
            AddDir(SlideShow, item.Path);
        }
        else if (!item.IsRemote)
        {
          SlideShow.Add(item.Path);
        }
      }
    }

    void OnSlideShowRecursive()
    {
      GUISlideShow SlideShow = (GUISlideShow)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
      if (SlideShow == null)
        return;

      SlideShow.Reset();
      if (disp == Display.Files)
      {
        AddDir(SlideShow, currentFolder);
      }
      else
      {
        using (PictureDatabase dbs = new PictureDatabase())
        {
          List<string> pics = new List<string>();
          int totalCount = dbs.ListPicsByDate(currentFolder.Replace("\\", "-"), ref pics);
          foreach (string pic in pics)
            SlideShow.Add(pic);
        }
      }
      if (SlideShow.Count > 0)
      {
        SlideShow.StartSlideShow(currentFolder);
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
      }
    }

    void OnSlideShow()
    {
      OnSlideShow(0);
    }

    void OnSlideShow(int iStartItem)
    {
      GUISlideShow SlideShow = (GUISlideShow)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
      if (SlideShow == null)
        return;

      SlideShow.Reset();

      if ((iStartItem < 0) || (iStartItem > GetItemCount()))
        iStartItem = 0;
      int i = iStartItem;
      do
      {
        GUIListItem item = GetItem(i);
        if (!item.IsFolder && !item.IsRemote)
        {
          SlideShow.Add(item.Path);
        }

        i++;
        if (i >= GetItemCount())
        {
          i = 0;
        }
      }
      while (i != iStartItem);

      if (SlideShow.Count > 0)
      {
        SlideShow.StartSlideShow(currentFolder);
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SLIDESHOW);
      }
    }

    void CreateAllThumbs(string strDir, GUIDialogProgress dlgProgress, PictureDatabase dbs, bool Regenerate, bool Recursive)
    {
      int Count = 0;
      if (disp == Display.Files)
      {
        List<GUIListItem> itemlist = virtualDirectory.GetDirectoryExt(strDir);
        Filter(ref itemlist);
        foreach (GUIListItem item in itemlist)
        {
          if (dlgProgress != null)
          {
            dlgProgress.SetLine(1, strDir);
            dlgProgress.SetLine(2, String.Format(GUILocalizeStrings.Get(8033) + ": {0}/{1}", ++Count, itemlist.Count));
            dlgProgress.SetPercentage((100 * Count) / itemlist.Count);
            dlgProgress.Progress();
            if (dlgProgress.IsCanceled)
              return;
          }
          if (item.IsFolder)
          {
            if (item.Label != "..")
            {
              if (Recursive)
                CreateAllThumbs(item.Path, dlgProgress, dbs, Regenerate, Recursive);
              if (Regenerate || !System.IO.File.Exists(item.Path + @"\folder.jpg"))
                CreateFolderThumb(item.Path);
            }
          }
          else
          {
            string thumbnailImage = String.Format(@"{0}\{1}.jpg", Thumbs.Pictures, MediaPortal.Util.Utils.EncryptLine(item.Path));
            if ((Regenerate || !System.IO.File.Exists(thumbnailImage)) && MediaPortal.Util.Utils.IsPicture(item.Path))
            {
              int iRotate = dbs.GetRotation(item.Path);

              // create thumbs for default listcontrol views
              thumbnailImage = GetThumbnail(item.Path);
              Util.Picture.CreateThumbnail(item.Path, thumbnailImage, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, iRotate);

              // create large thumbs for filmstrip panel
              thumbnailImage = GetLargeThumbnail(item.Path);
              Util.Picture.CreateThumbnail(item.Path, thumbnailImage, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, iRotate);
            }
          }
        }
      }
      else if (disp == Display.Date)
      {
        List<string> pics = new List<string>();
        int totalCount = dbs.ListPicsByDate(strDir.Replace("\\", "-"), ref pics);
        foreach (string pic in pics)
        {
          if (dlgProgress != null)
          {
            dlgProgress.SetLine(1, strDir);
            dlgProgress.SetLine(2, String.Format(GUILocalizeStrings.Get(8033) + ": {0}/{1}", ++Count, totalCount));
            dlgProgress.SetPercentage((100 * Count) / totalCount);
            dlgProgress.Progress();
            if (dlgProgress.IsCanceled)
              return;
          }
          string thumbnailImage = String.Format(@"{0}\{1}.jpg", Thumbs.Pictures, MediaPortal.Util.Utils.EncryptLine(pic));
          if ((Regenerate || !System.IO.File.Exists(thumbnailImage)) && MediaPortal.Util.Utils.IsPicture(pic))
          {
            int iRotate = dbs.GetRotation(pic);

            // create thumbs for default listcontrol views
            thumbnailImage = GetThumbnail(pic);
            Util.Picture.CreateThumbnail(pic, thumbnailImage, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, iRotate);

            // create large thumbs for filmstrip panel
            thumbnailImage = GetLargeThumbnail(pic);
            Util.Picture.CreateThumbnail(pic, thumbnailImage, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, iRotate);
          }
        }
      }
    }

    void OnCreateAllThumbs(string strDir, bool Regenerate, bool Recursive)
    {
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (dlgProgress != null)
      {
        dlgProgress.SetHeading(110);
        dlgProgress.ShowProgressBar(false);
        dlgProgress.SetLine(1, String.Empty);
        dlgProgress.SetLine(2, String.Empty);
        dlgProgress.StartModal(GetID);
        dlgProgress.Progress();
      }
      using (PictureDatabase dbs = new PictureDatabase())
      {
        CreateAllThumbs(strDir, dlgProgress, dbs, Regenerate, Recursive);
      }
      if (dlgProgress != null)
        dlgProgress.Close();
      GUITextureManager.CleanupThumbs();
      GUIWaitCursor.Hide();
      LoadDirectory(currentFolder);
    }

    void OnCreateThumbs()
    {/*
      CreateFolderThumbs();
      //GUIWaitCursor.Show();
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (dlgProgress != null)
      {
        dlgProgress.SetHeading(110);
        dlgProgress.ShowProgressBar(true);
        dlgProgress.SetLine(1, String.Empty);
        dlgProgress.SetLine(2, String.Empty);
        dlgProgress.StartModal(GetID);
        dlgProgress.ShowProgressBar(true);
        dlgProgress.Progress();
      }

      using (PictureDatabase dbs = new PictureDatabase())
      {
        for (int i = 0; i < GetItemCount(); ++i)
        {
          int percent = ((i + 1) * 100) / GetItemCount();
          GUIListItem item = GetItem(i);
          if (item.IsRemote)
            continue;
          if (dlgProgress != null)
          {
            dlgProgress.SetPercentage(percent);
            string progressLine = String.Format(GUILocalizeStrings.Get(8033) + ": {0}/{1}", i + 1, GetItemCount());
            dlgProgress.SetLine(1, String.Empty);
            dlgProgress.SetLine(2, progressLine);
          }
          if (item.IsRemote)
          {
            if (dlgProgress != null)
            {
              dlgProgress.Progress();
              if (dlgProgress.IsCanceled)
                break;
            }
            continue;
          }

          if (!item.IsFolder)
          {
            if (MediaPortal.Util.Utils.IsPicture(item.Path))
            {
              if (dlgProgress != null)
              {
                string strFile = String.Format(GUILocalizeStrings.Get(863) + ": {0}", item.Label);
                dlgProgress.SetLine(1, strFile);
                dlgProgress.Progress();
                if (dlgProgress.IsCanceled)
                  break;
              }

              // create thumbs for default listcontrol views
              string thumbnailImage = GetThumbnail(item.Path);
              //if (!System.IO.File.Exists(thumbnailImage))
              //{
                int iRotate = dbs.GetRotation(item.Path);
                Util.Picture.CreateThumbnail(item.Path, thumbnailImage, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, iRotate);
              //}

              // create large thumbs for filmstrip panel
              thumbnailImage = GetLargeThumbnail(item.Path);
              //if (!System.IO.File.Exists(thumbnailImage))
              //{
                iRotate = dbs.GetRotation(item.Path);
                Util.Picture.CreateThumbnail(item.Path, thumbnailImage, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, iRotate);
              //}
            }
          }
        }
      }
      
      if (dlgProgress != null)
        dlgProgress.Close();
      GUITextureManager.CleanupThumbs();
      GUIWaitCursor.Hide();
      LoadDirectory(currentFolder);
		  */
    }

    private void OnShowSortMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(495);

      dlg.AddLocalizedString(103); // name
      dlg.AddLocalizedString(105); // size
      dlg.AddLocalizedString(104); // date

      dlg.DoModal(GetID);

      if (dlg.SelectedLabel == -1)
        return;

      switch (dlg.SelectedId)
      {
        case 103:
          mapSettings.SortBy = (int)SortMethod.Name;
          break;
        case 104:
          mapSettings.SortBy = (int)SortMethod.Date;
          break;
        case 105:
          mapSettings.SortBy = (int)SortMethod.Size;
          break;
        default:
          mapSettings.SortBy = (int)SortMethod.Name;
          break;
      }

      OnSort();
      GUIControl.FocusControl(GetID, btnSortBy.GetID);
    }

    void OnShowFileMenu()
    {
      GUIListItem item = selectedListItem;
      if (item == null)
        return;
      if (item.IsFolder && item.Label == "..")
        return;

      // init
      GUIDialogFile dlgFile = (GUIDialogFile)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_FILE);
      if (dlgFile == null)
        return;

      // File operation settings
      dlgFile.SetSourceItem(item);
      dlgFile.SetSourceDir(currentFolder);
      dlgFile.SetDestinationDir(destinationFolder);
      dlgFile.SetDirectoryStructure(virtualDirectory);
      dlgFile.DoModal(GetID);
      destinationFolder = dlgFile.GetDestinationDir();

      //final		
      if (dlgFile.Reload())
      {
        LoadDirectory(currentFolder);
        if (selectedItemIndex >= 0)
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, selectedItemIndex);
        }
      }

      dlgFile.DeInit();
      dlgFile = null;
    }

    void OnSwitchView()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(457);

      dlg.AddLocalizedString(134); // Shares
      dlg.AddLocalizedString(636); // date

      dlg.DoModal(GetID);

      if (dlg.SelectedLabel == -1)
        return;

      switch (dlg.SelectedId)
      {
        case 134:
          if (disp != Display.Files)
          {
            disp = Display.Files;
            LoadDirectory(m_strDirectoryStart);
          }
          break;
        case 636:
          if (disp != Display.Date)
          {
            disp = Display.Date;
            LoadDirectory("");
          }
          break;
      }


      GUIControl.FocusControl(GetID, btnSwitchView.GetID);
    }


    #endregion

    #region various
    /// <summary>
    /// Returns true if the specified window should maintain virtual directory
    /// </summary>
    /// <param name="windowId">id of window</param>
    /// <returns>
    /// true: if the specified window should maintain virtual directory
    /// false: if the specified window should not maintain virtual directory</returns>
    static public bool KeepVirtualDirectory(int windowId)
    {
      if (windowId == (int)GUIWindow.Window.WINDOW_PICTURES)
        return true;
      if (windowId == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
        return true;
      return false;
    }

    void LoadDirectory(string strNewDirectory)
    {
      List<GUIListItem> itemlist;
      string objectCount = String.Empty;

      GUIWaitCursor.Show();
      GUIListItem SelectedItem = GetSelectedItem();
      if (SelectedItem != null)
      {
        if (SelectedItem.IsFolder && SelectedItem.Label != "..")
        {
          folderHistory.Set(SelectedItem.Label, currentFolder);
        }
      }
      if (strNewDirectory != currentFolder && mapSettings != null)
      {
        SaveFolderSettings(currentFolder);
      }

      if (strNewDirectory != currentFolder || mapSettings == null)
      {
        LoadFolderSettings(strNewDirectory);
      }
      currentFolder = strNewDirectory;
      GUIControl.ClearControl(GetID, facadeView.GetID);

      if (disp == Display.Files)
      {
        //CreateMissingThumbnails();
        MissingThumbCacher ThumbWorker = new MissingThumbCacher(currentFolder, _autocreateLargeThumbs);

        itemlist = virtualDirectory.GetDirectoryExt(currentFolder);
        Filter(ref itemlist);


        int itemIndex = 0;
        foreach (GUIListItem item in itemlist)
        {
          item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
          item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
          facadeView.Add(item);

        }
        OnSort();
        /*
        for (int i = 0; i < GetItemCount(); ++i)
        {
          GUIListItem item = GetItem(i);
          if (item.Label == strSelectedItem)
          {
            GUIControl.SelectItemControl(GetID, facadeView.GetID, itemIndex);
            break;
          }
          itemIndex++;
        }	*/
      }
      else
      {
        LoadDateView(strNewDirectory);
      }

      int totalItemCount = facadeView.Count;
      string strSelectedItem = folderHistory.Get(currentFolder);
      for (int i = 0; i < totalItemCount; i++)
      {
        if (facadeView[i].Label == strSelectedItem)
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, i);
          break;
        }
      }
      if (totalItemCount > 0)
      {
        GUIListItem rootItem = (GUIListItem)facadeView[0];
        if (rootItem.Label == "..")
          totalItemCount--;
      }
      objectCount = String.Format("{0} {1}", totalItemCount, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount", objectCount);

      ShowThumbPanel();
      GUIWaitCursor.Hide();
    }

    void LoadDateView(string strNewDirectory)
    {
      if (strNewDirectory == "")
      {
        // Years
        using (PictureDatabase dbs = new PictureDatabase())
        {
          List<string> Years = new List<string>();
          int Count = dbs.ListYears(ref Years);
          foreach (string year in Years)
          {
            GUIListItem item = new GUIListItem(year);
            item.Label = year;
            Log.Info("Load Year: " + year);
            item.Path = year;
            item.IsFolder = true;
            Util.Utils.SetDefaultIcons(item);
            item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
            item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
            facadeView.Add(item);
          }
        }
      }
      else if (strNewDirectory.Length == 4)
      {
        // Months
        string year = strNewDirectory.Substring(0, 4);
        GUIListItem item = new GUIListItem("..");
        item.Path = "";
        item.IsFolder = true;
        Util.Utils.SetDefaultIcons(item);
        item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        facadeView.Add(item);
        using (PictureDatabase dbs = new PictureDatabase())
        {
          List<string> Months = new List<string>();
          int Count = dbs.ListMonths(year, ref Months);
          foreach (string month in Months)
          {
            item = new GUIListItem(month);
            item.Path = year + "\\" + month;
            item.IsFolder = true;
            Util.Utils.SetDefaultIcons(item);
            item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
            item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
            facadeView.Add(item);
          }
          List<string> pics = new List<string>();
          int PicCount = dbs.CountPicsByDate(year);
          if (PicCount <= MAX_PICS_PER_DATE)
          {
            Count += dbs.ListPicsByDate(year, ref pics);
            foreach (string pic in pics)
            {
              item = new GUIListItem(Path.GetFileNameWithoutExtension(pic));
              item.Path = pic;
              item.IsFolder = false;
              Util.Utils.SetDefaultIcons(item);
              item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
              item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
              item.FileInfo = new FileInformation(pic, false);
              facadeView.Add(item);
            }
          }
        }
      }
      else if (strNewDirectory.Length == 7)
      {
        // Days
        string year = strNewDirectory.Substring(0, 4);
        string month = strNewDirectory.Substring(5, 2);
        GUIListItem item = new GUIListItem("..");
        item.Path = year;
        item.IsFolder = true;
        Util.Utils.SetDefaultIcons(item);
        item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        facadeView.Add(item);
        using (PictureDatabase dbs = new PictureDatabase())
        {
          List<string> Days = new List<string>();
          int Count = dbs.ListDays(year, month, ref Days);
          foreach (string day in Days)
          {
            item = new GUIListItem(day);
            item.Path = year + "\\" + month + "\\" + day;
            item.IsFolder = true;
            Util.Utils.SetDefaultIcons(item);
            item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
            item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
            facadeView.Add(item);
          }
          List<string> pics = new List<string>();
          int PicCount = dbs.CountPicsByDate(year + "-" + month);
          if (PicCount <= MAX_PICS_PER_DATE)
          {
            Count += dbs.ListPicsByDate(year + "-" + month, ref pics);
            foreach (string pic in pics)
            {
              item = new GUIListItem(Path.GetFileNameWithoutExtension(pic));
              item.Path = pic;
              item.IsFolder = false;
              Util.Utils.SetDefaultIcons(item);
              item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
              item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
              item.FileInfo = new FileInformation(pic, false);
              facadeView.Add(item);
            }
          }
        }
      }
      else if (strNewDirectory.Length == 9)
      {
        // Pics from one day
        string year = strNewDirectory.Substring(0, 4);
        string month = strNewDirectory.Substring(5, 2);
        string day = strNewDirectory.Substring(7, 2);
        GUIListItem item = new GUIListItem("..");
        item.Path = year + "\\" + month;
        item.IsFolder = true;
        Util.Utils.SetDefaultIcons(item);
        item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
        item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        facadeView.Add(item);
        using (PictureDatabase dbs = new PictureDatabase())
        {
          List<string> pics = new List<string>();
          int Count = dbs.ListPicsByDate(year + "-" + month + "-" + day, ref pics);
          foreach (string pic in pics)
          {
            item = new GUIListItem(Path.GetFileNameWithoutExtension(pic));
            item.Path = pic;
            item.IsFolder = false;
            Util.Utils.SetDefaultIcons(item);
            item.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
            item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
            item.FileInfo = new FileInformation(pic, false);
            facadeView.Add(item);
          }
        }
      }
      if (facadeView.Count == 0 && strNewDirectory != "")
      {
        // Wrong path for date view, go back to top level
        currentFolder = "";
        LoadDateView(currentFolder);
      }
    }

    void CreateFolderThumbs()
    {
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (dlgProgress != null)
      {
        dlgProgress.SetHeading(110);
        dlgProgress.SetLine(1, String.Empty);
        dlgProgress.SetLine(2, String.Empty);
        dlgProgress.StartModal(GetID);
        dlgProgress.ShowProgressBar(true);
        dlgProgress.Progress();
      }
      for (int i = 0; i < GetItemCount(); ++i)
      {
        int percent = ((i + 1) * 100) / GetItemCount();
        if (dlgProgress != null)
        {
          dlgProgress.SetPercentage(percent);
          string progressLine = String.Format(GUILocalizeStrings.Get(8033) + ": {0}/{1}", i + 1, GetItemCount());
          dlgProgress.SetLine(1, String.Empty);
          dlgProgress.SetLine(2, progressLine);
        }
        GUIListItem item = GetItem(i);
        if (item.IsFolder && item.Label != "..")
        {
          if (dlgProgress != null)
          {
            string fileName = String.Format(GUILocalizeStrings.Get(110) + ": {0}", item.Label);
            dlgProgress.SetLine(1, fileName);
            dlgProgress.Progress();
            if (dlgProgress.IsCanceled)
              break;
          }

          CreateFolderThumb(item.Path);
        }//if (item.IsFolder)
        else if (dlgProgress != null)
        {
          dlgProgress.Progress();
          if (dlgProgress.IsCanceled)
            break;
        }
      }//for (int i=0; i < GetItemCount(); ++i)
      if (dlgProgress != null)
        dlgProgress.Close();
    }

    void CreateFolderThumb(string path)
    {
      // find first 4 jpegs in this subfolder
      List<GUIListItem> itemlist = virtualDirectory.GetDirectoryUnProtectedExt(path, true);
      Filter(ref itemlist);
      List<string> pictureList = new List<string>();
      foreach (GUIListItem subitem in itemlist)
      {
        if (!subitem.IsFolder)
        {
          if (!subitem.IsRemote && MediaPortal.Util.Utils.IsPicture(subitem.Path))
          {
            pictureList.Add(subitem.Path);
            if (pictureList.Count >= 4)
              break;
          }
        }
      }
      if (pictureList.Count > 0)
      {
        using (Image imgFolder = Image.FromFile(GUIGraphicsContext.Skin + @"\media\previewbackground.png"))
        {
          int width = imgFolder.Width;
          int height = imgFolder.Height;

          int thumbnailWidth = (width - 30) / 2;
          int thumbnailHeight = (height - 30) / 2;

          using (Bitmap bmp = new Bitmap(width, height))
          {
            using (Graphics g = Graphics.FromImage(bmp))
            {
              g.CompositingQuality = Thumbs.Compositing;
              g.InterpolationMode = Thumbs.Interpolation;
              g.SmoothingMode = Thumbs.Smoothing;

              g.DrawImage(imgFolder, 0, 0, width, height);
              int x, y, w, h;
              x = 0;
              y = 0;
              w = thumbnailWidth;
              h = thumbnailHeight;
              //Load first of 4 images for the folder thumb.
              //Avoid crashes caused by damaged image files:
              try
              {
                AddPicture(g, (string)pictureList[0], x + 10, y + 10, w, h);
              }
              catch (Exception)
              {
                Log.Info("Damaged picture file found: {0}. Try to repair or delete this file please!", (string)pictureList[0]);
              }

              //If exists load second of 4 images for the folder thumb.
              if (pictureList.Count > 1)
              {
                try
                {
                  AddPicture(g, (string)pictureList[1], x + thumbnailWidth + 20, y + 10, w, h);
                }
                catch (Exception)
                {
                  Log.Info("Damaged picture file found: {0}. Try to repair or delete this file please!", (string)pictureList[1]);
                }
              }

              //If exists load third of 4 images for the folder thumb.
              if (pictureList.Count > 2)
              {
                try
                {
                  AddPicture(g, (string)pictureList[2], x + 10, y + thumbnailHeight + 20, w, h);
                }
                catch (Exception)
                {
                  Log.Info("Damaged picture file found: {0}. Try to repair or delete this file please!", (string)pictureList[2]);
                }
              }

              //If exists load fourth of 4 images for the folder thumb.
              if (pictureList.Count > 3)
              {
                try
                {
                  AddPicture(g, (string)pictureList[3], x + thumbnailWidth + 20, y + thumbnailHeight + 20, w, h);
                }
                catch (Exception)
                {
                  Log.Info("Damaged picture file found: {0}. Try to repair or delete this file please!", (string)pictureList[3]);
                }
              }
            }//using (Graphics g = Graphics.FromImage(bmp) )
            try
            {
              string thumbnailImageName = path + @"\folder.jpg";
              if (System.IO.File.Exists(thumbnailImageName))
                MediaPortal.Util.Utils.FileDelete(thumbnailImageName);
              bmp.Save(thumbnailImageName, System.Drawing.Imaging.ImageFormat.Jpeg);

              File.SetAttributes(thumbnailImageName, FileAttributes.Hidden);
            }
            catch (Exception)
            {
            }
          }//using (Bitmap bmp = new Bitmap(210,210))
        }
      }//if (pictureList.Count>0)
    }

    void AddPicture(Graphics g, string strFileName, int x, int y, int w, int h)
    {
      // Add a thumbnail of the specified picture file to the image referenced by g, draw it at the
      // given location and size.
      Image img = null;
      using (PictureDatabase dbs = new PictureDatabase())
      {
        int iRotate = dbs.GetRotation(strFileName);
        img = Image.FromFile(strFileName);
        if (img != null)
        {
          if (iRotate > 0)
          {
            RotateFlipType flipType;
            switch (iRotate)
            {
              case 1:
                flipType = RotateFlipType.Rotate90FlipNone;
                img.RotateFlip(flipType);
                break;
              case 2:
                flipType = RotateFlipType.Rotate180FlipNone;
                img.RotateFlip(flipType);
                break;
              case 3:
                flipType = RotateFlipType.Rotate270FlipNone;
                img.RotateFlip(flipType);
                break;
              default:
                flipType = RotateFlipType.RotateNoneFlipNone;
                break;
            }
          }
          g.DrawImage(img, x, y, w, h);

        }
      }
    }

    void Filter(ref List<GUIListItem> itemlist)
    {
      bool isFound;
      do
      {
        isFound = false;
        for (int i = 0; i < itemlist.Count; ++i)
        {
          GUIListItem item = itemlist[i];
          if (!item.IsFolder)
          {
            if (item.Path.IndexOf("folder.jpg") > 0)
            {
              isFound = true;
              itemlist.RemoveAt(i);
              break;
            }
          }
        }
      } while (isFound);
    }

    static public string GetThumbnail(string fileName)
    {
      if (fileName == String.Empty)
        return String.Empty;
      return String.Format(@"{0}\{1}.jpg", Thumbs.Pictures, MediaPortal.Util.Utils.EncryptLine(fileName));
    }

    static public string GetLargeThumbnail(string fileName)
    {
      if (fileName == String.Empty)
        return String.Empty;
      return String.Format(@"{0}\{1}L.jpg", Thumbs.Pictures, MediaPortal.Util.Utils.EncryptLine(fileName));
    }


    void CreateMissingThumbnails()
    {
      Thread WorkerThread = new Thread(new ThreadStart(MissingThumbWorker));
      WorkerThread.SetApartmentState(ApartmentState.STA);
      WorkerThread.IsBackground = true;
      WorkerThread.Priority = ThreadPriority.BelowNormal;
      WorkerThread.Start();
    }

    void MissingThumbWorker()
    {
      //if (!virtualDirectory.IsRemote(currentFolder))
      //{
      //  string path = currentFolder;
      //  List<GUIListItem> itemlist = virtualDirectory.GetDirectoryUnProtectedExt(path, true);

      //  foreach (GUIListItem item in itemlist)
      //  {
      //    if (currentFolder != path)
      //      return;
      //    if (GUIWindowManager.ActiveWindow != GetID)
      //      return;
      //    if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
      //      return;
      //    if (!item.IsFolder)
      //    {
      //      if (!item.IsRemote && MediaPortal.Util.Utils.IsPicture(item.Path))
      //      {
      //        using (PictureDatabase dbs = new PictureDatabase())
      //        {
      //          string thumbnailImage = GetThumbnail(item.Path);
      //          if (!System.IO.File.Exists(thumbnailImage))
      //          {
      //            int iRotate = dbs.GetRotation(item.Path);
      //            Util.Picture.CreateThumbnail(item.Path, thumbnailImage, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, iRotate);
      //            System.Threading.Thread.Sleep(25);
      //          }

      //          if (_autocreateLargeThumbs)
      //          {
      //            thumbnailImage = GetLargeThumbnail(item.Path);
      //            if (!System.IO.File.Exists(thumbnailImage))
      //            {
      //              int iRotate = dbs.GetRotation(item.Path);
      //              Util.Picture.CreateThumbnail(item.Path, thumbnailImage, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, iRotate);
      //              System.Threading.Thread.Sleep(25);
      //            }
      //          }
      //        }
      //      }
      //    }
      //    else
      //    {
      //      int pin;
      //      if ((item.Label != "..") && (!virtualDirectory.IsProtectedShare(item.Path, out pin)))
      //      {
      //        string thumbnailImage = item.Path + @"\folder.jpg";
      //        if (!item.IsRemote && !System.IO.File.Exists(thumbnailImage))
      //        {
      //          CreateFolderThumb(item.Path);
      //          //  System.Threading.Thread.Sleep(100);
      //        }
      //      }
      //    }
      //  } //foreach (GUIListItem item in itemlist)
      //}
    } //void MissingThumbWorker()

    bool GetUserInputString(ref string sString)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
        return false;
      keyboard.IsSearchKeyboard = true;
      keyboard.Reset();
      keyboard.Text = sString;
      keyboard.DoModal(GetID); // show it...
      if (keyboard.IsConfirmed)
        sString = keyboard.Text;
      return keyboard.IsConfirmed;
    }

    #endregion

    #region callback events
    public bool ThumbnailCallback()
    {
      return false;
    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
      if (filmstrip == null)
        return;
      string thumbnailImage = GetLargeThumbnail(item.Path);
      filmstrip.InfoImageFileName = thumbnailImage;
      //UpdateButtonStates();  -> fixing mantis bug 902
    }

    private void SortChanged(object sender, SortEventArgs e)
    {
      mapSettings.SortAscending = e.Order != System.Windows.Forms.SortOrder.Descending;

      OnSort();

      GUIControl.FocusControl(GetID, ((GUIControl)sender).GetID);
    }

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public bool HasSetup()
    {
      return false;
    }
    public string PluginName()
    {
      return "My Pictures";
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return GetID;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      // TODO:  Add GUIPictures.GetHome implementation
      strButtonText = GUILocalizeStrings.Get(1);
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = String.Empty;
      return true;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string Description()
    {
      return "Watch your photos and slideshows with MediaPortal";
    }

    public void ShowPlugin()
    {
      // TODO:  Add GUIPictures.ShowPlugin implementation
    }

    #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return true;
    }

    #endregion

  }
}