using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.MPInstaller;

namespace WindowPlugins.GUI.Extensions
{
  public class GUIExtensions : GUIWindow, ISetupForm, IShowPlugin, IComparer<GUIListItem>
  {
    #region enums
    enum SortMethod
    {
      Name = 0,
      Type = 1,
      Date = 2,
    }

    enum View : int
    {
      List = 0,
      Icons = 1,
      LargeIcons = 2,
    }

    enum Views
    {
      Local = 0,
      Online = 1,
    }
    
    #endregion

    #region Base variabeles
    View currentView = View.List;
    Views currentListing = Views.Local;
    SortMethod currentSortMethod = SortMethod.Date;
    bool sortAscending = true;
    VirtualDirectory virtualDirectory = new VirtualDirectory();
    DirectoryHistory directoryHistory = new DirectoryHistory();
    string currentFolder = string.Empty;
    int selectedItemIndex = -1;
    WebClient client = new WebClient();
    static GUIDialogProgress dlgProgress;

    public MPInstallHelper lst = new MPInstallHelper();
    public MPInstallHelper lst_online = new MPInstallHelper();
    public QueueEnumerator queue = new QueueEnumerator();

    #endregion

    #region SkinControls

    [SkinControlAttribute(50)]
    protected GUIFacadeControl facadeView = null;
    [SkinControlAttribute(2)]
    protected GUIButtonControl btnViewAs = null;
    [SkinControlAttribute(3)]
    protected GUISortButtonControl btnSortBy = null;
    [SkinControlAttribute(6)]
    protected GUIButtonControl btnViews = null;

    #endregion

    public GUIExtensions()
    {
      GetID = (int)GUIWindow.Window.WINDOW_EXTENSIONS;
    }

    public override bool Init()
    {
      currentFolder = string.Empty;
      client.CachePolicy = new System.Net.Cache.RequestCachePolicy();
      client.UseDefaultCredentials = true;
    
      client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
      client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(client_DownloadFileCompleted);
      bool bResult = Load(GUIGraphicsContext.Skin + @"\myextensions.xml");

      return bResult;
    }

    void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      if (dlgProgress != null)
      {
        dlgProgress.SetPercentage(100);
        dlgProgress.Progress();
        dlgProgress.ShowProgressBar(true);
        dlgProgress.Close();
        dlgProgress = null;
      }
    }

    void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      if (dlgProgress != null)
      {
        dlgProgress.SetLine(2, string.Format("{0} / {1} ({2}%)", e.TotalBytesToReceive, e.BytesReceived,e.ProgressPercentage));
        dlgProgress.ShowProgressBar(true);
        dlgProgress.SetPercentage(e.ProgressPercentage);
        dlgProgress.Progress();
      }
    }

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string PluginName()
    {
      return "Extensions";
    }

    public bool HasSetup()
    {
      return false;
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
      strButtonText = GUILocalizeStrings.Get(14001);
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = @"hover_my radio.png";
      return true;
    }

    public string Author()
    {
      return "Dukus";
    }

    public string Description()
    {
      return "Browse (Un)Install Extensions";
    }

    public void ShowPlugin()
    {
      
    }

   #endregion

    #region IShowPlugin Members

    public bool ShowDefaultHome()
    {
      return true;
    }

    #endregion

    #region Serialisation
    void LoadSettings()
    {
      lst.LoadFromFile();
      lst_online.LoadFromFile(MpiFileList.ONLINE_LISTING);
      queue = queue.Load(MpiFileList.QUEUE_LISTING);

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        //currentRadioFolder = xmlreader.GetValueAsString("radio", "folder", string.Empty);

        string tmpLine = string.Empty;
        tmpLine = (string)xmlreader.GetValue("myextensions", "viewby");
        if (tmpLine != null)
        {
          if (tmpLine == "list") currentView = View.List;
          else if (tmpLine == "icons") currentView = View.Icons;
          else if (tmpLine == "largeicons") currentView = View.LargeIcons;
        }

        tmpLine = (string)xmlreader.GetValue("myextensions", "sort");
        if (tmpLine != null)
        {
          if (tmpLine == "name") currentSortMethod = SortMethod.Name;
          else if (tmpLine == "type") currentSortMethod = SortMethod.Type;
          else if (tmpLine == "date") currentSortMethod = SortMethod.Date;
        }
        tmpLine = (string)xmlreader.GetValue("myextensions", "listing");
        if (tmpLine != null)
        {
          if (tmpLine == "local") currentListing = Views.Local;
          else if (tmpLine == "online") currentListing = Views.Online;
        }
        sortAscending = xmlreader.GetValueAsBool("myextensions", "sortascending", true);
      }
    }

    void SaveSettings()
    {
      queue.Save(MpiFileList.QUEUE_LISTING);
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        switch (currentView)
        {
          case View.List:
            xmlwriter.SetValue("myextensions", "viewby", "list");
            break;
          case View.Icons:
            xmlwriter.SetValue("myextensions", "viewby", "icons");
            break;
          case View.LargeIcons:
            xmlwriter.SetValue("myextensions", "viewby", "largeicons");
            break;
        }

        switch (currentSortMethod)
        {
          case SortMethod.Name:
            xmlwriter.SetValue("myextensions", "sort", "name");
            break;
          case SortMethod.Type:
            xmlwriter.SetValue("myextensions", "sort", "type");
            break;
          case SortMethod.Date:
            xmlwriter.SetValue("myextensions", "sort", "date");
            break;
        }

        switch (currentListing)
        {
          case Views.Local:
            xmlwriter.SetValue("myextensions", "listing", "local");
            break;
          case Views.Online:
            xmlwriter.SetValue("myextensions", "listing", "online");
            break;
        }

        xmlwriter.SetValueAsBool("myextensions", "sortascending", sortAscending);
      }
    }
    #endregion

    #region BaseWindow Members
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
              LoadDirectory(item.Path);
              return;
            }
          }
        }
      }

      if (action.wID == Action.ActionType.ACTION_PARENT_DIR)
      {
        GUIListItem item = facadeView[0];
        if (item != null)
        {
          if (item.IsFolder && item.Label == "..")
          {
            LoadDirectory(item.Path);
          }
        }
        return;
      }

      base.OnAction(action);
    }

    protected override void OnPageLoad()
    {
      LoadSettings();
      switch (currentSortMethod)
      {
        case SortMethod.Name:
          btnSortBy.SelectedItem = 0;
          break;
        case SortMethod.Type:
          btnSortBy.SelectedItem = 1;
          break;
        case SortMethod.Date:
          btnSortBy.SelectedItem = 2;
          break;
      }


      virtualDirectory = new VirtualDirectory();
      SelectCurrentItem();
      UpdateButtonStates();
      LoadDirectory(currentFolder);

      btnSortBy.SortChanged += new SortEventHandler(SortChanged);


      base.OnPageLoad();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      selectedItemIndex = facadeView.SelectedListItemIndex;
      SaveSettings();
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      if (control == btnViewAs)
      {
        bool shouldContinue = false;
        do
        {
          shouldContinue = false;
          switch (currentView)
          {
            case View.List:
              currentView = View.Icons;
              if (facadeView.ThumbnailView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
              break;

            case View.Icons:
              currentView = View.LargeIcons;
              if (facadeView.ThumbnailView == null)
                shouldContinue = true;
              else
                facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
              break;

            case View.LargeIcons:
              currentView = View.List;
              if (facadeView.ListView == null)
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

      if (control == facadeView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, controlId, 0, 0, null);
        OnMessage(msg);
        int itemIndex = (int)msg.Param1;
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick(itemIndex);
        }
      }
    }

    void OnClick(int itemIndex)
    {
      GUIListItem item = facadeView.SelectedListItem;
      if (item == null) return;
      if (item.IsFolder)
      {
        selectedItemIndex = -1;
        LoadDirectory(item.Path);
      }
      else
      {
        //Play(item);
        MPpackageStruct pk = item.MusicTag as MPpackageStruct;
        if (pk != null)
        {
          GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
          if (dlg == null) return;
          dlg.Reset();
          //dlg.SetHeading(495); // Sort options
          if (queue.ContainName(pk._intalerStruct.Name))
          {
            dlg.AddLocalizedString(14008);
          }
          else
          {
            if (lst.Find(pk._intalerStruct.Name) == null)
            {
              dlg.AddLocalizedString(14005); // install
            }
            else
            {
              dlg.AddLocalizedString(14006); // uninstall
              dlg.AddLocalizedString(14007); // reinstall
            }
          }
          
          // show dialog and wait for result
          dlg.DoModal(GetID);
          if (dlg.SelectedId == -1) return;
          QueueItem qitem = new QueueItem();
          qitem.Name = pk._intalerStruct.Name;
          qitem.DownloadUrl = MPinstallerStruct.DEFAULT_UPDATE_SITE + "/mp.php?option=down&user=&passwd=&filename=" + Path.GetFileName(pk.FileName);
          qitem.LocalFile = Config.GetFolder(Config.Dir.Installer) + @"\" + pk._intalerStruct.Name+" "+ pk._intalerStruct.Version + ".mpi";
          
          switch (dlg.SelectedId)
          {
            case 14005:
              qitem.Action = QueueAction.Install;
              break;
            case 14006:
              qitem.Action = QueueAction.Uninstall;
              break;
            case 14007:
              qitem.Action = QueueAction.Install;
              break;
            case 14008:
              queue.Remove(pk._intalerStruct.Name);              
              break;
          }
          
          if(qitem.Action==QueueAction.Install)
          {
            if (!File.Exists(qitem.LocalFile))
            {
              dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

              if (dlgProgress != null)
              {
                dlgProgress.Reset();
                dlgProgress.SetHeading(14010);
                dlgProgress.SetLine(1, pk._intalerStruct.Name+" - "+pk._intalerStruct.Version);
                dlgProgress.SetLine(2, "");
                dlgProgress.SetPercentage(0);
                dlgProgress.Progress();
                dlgProgress.DisableCancel(true);
                dlgProgress.ShowProgressBar(true);
                client.DownloadFileAsync(new Uri(qitem.DownloadUrl), qitem.LocalFile);
                dlgProgress.DoModal(GetID);
              }
            }

            MPpackageStruct package = new MPpackageStruct();
            package.LoadFromFile(qitem.LocalFile);
            if (package.isValid)
            {
              if (package._intalerStruct.SetupGroups.Count > 1)
              {
                if (package._intalerStruct.ProiectProperties.SingleGroupSelect)
                {

                  GUIDialogMenu dlgselect = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
                  if (dlgselect == null) return;
                  dlgselect.Reset();
                  dlgselect.SetHeading(14011); // Sort options
                  foreach (GroupString gs in package._intalerStruct.SetupGroups)
                  {
                    gs.Checked = false;
                    dlgselect.Add(gs.Name);
                  }
                  dlgselect.DoModal(GetID);
                  if (dlgselect.SelectedLabel != -1)
                    package._intalerStruct.SetupGroups[dlgselect.SelectedLabel].Checked = true;
                  qitem.SetupGroups = package._intalerStruct.SetupGroups;
                }
                else
                {
                  foreach (GroupString gs in package._intalerStruct.SetupGroups)
                  {
                    GUIDialogYesNo dlgselect = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                    dlgselect.SetHeading(14011); // Sort options
                    dlgselect.SetLine(1, gs.Name);
                    dlgselect.DoModal(GetID);
                    if (dlgselect.IsConfirmed)
                      gs.Checked = true;
                  }
                  qitem.SetupGroups = package._intalerStruct.SetupGroups;
                }
              }
            }
          }

          if (dlg.SelectedId != 14008)
          {
            queue.Items.Add(qitem);
            GUIDialogNotify dlg1 = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
            if (dlg1 == null) return;
            dlg1.Reset();
            dlg1.SetHeading(14001);
            dlg1.SetText(GUILocalizeStrings.Get(14009));
            dlg1.Reset();
            dlg1.TimeOut = 2;
            dlg1.DoModal(GetID);
          }
        }
        GUIPropertyManager.SetProperty("#selecteditem", item.Label);
      }
    }

    void OnShowSort()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(495); // Sort options

      dlg.AddLocalizedString(103); // name
      dlg.AddLocalizedString(668); // Type
      dlg.AddLocalizedString(104); // date

      dlg.SelectedLabel = (int)currentSortMethod;

      // show dialog and wait for result
      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1) return;

      switch (dlg.SelectedId)
      {
        case 103:
          currentSortMethod = SortMethod.Name;
          break;
        case 668:
          currentSortMethod = SortMethod.Type;
          break;
        case 104:
          currentSortMethod = SortMethod.Date;
          break;
        default:
          currentSortMethod = SortMethod.Name;
          break;
      }

      OnSort();
      if (btnSortBy != null)
        GUIControl.FocusControl(GetID, btnSortBy.GetID);
    }

    void OnShowViews()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(14002); // Sort options

      dlg.AddLocalizedString(14003); // local
      dlg.AddLocalizedString(14004); // online

      dlg.SelectedLabel = (int)currentListing;

      // show dialog and wait for result
      dlg.DoModal(GetID);    
      if (dlg.SelectedId == -1) return;

      switch (dlg.SelectedId)
      {
        case 14003:
          currentListing = Views.Local;
          break;
        case 14004:
          currentListing = Views.Online;
          break;
      }

      LoadDirectory(currentFolder);
    }

    #endregion

    #region Sort Members
    void OnSort()
    {
      SetLabels();
      facadeView.Sort(this);
      UpdateButtonStates();
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2) return 0;
      if (item1 == null) return -1;
      if (item2 == null) return -1;
      if (item1.IsFolder && item1.Label == "..") return -1;
      if (item2.IsFolder && item2.Label == "..") return -1;
      if (item1.IsFolder && !item2.IsFolder) return -1;
      else if (!item1.IsFolder && item2.IsFolder) return 1;


      SortMethod method = currentSortMethod;
      bool bAscending = sortAscending;
      MPpackageStruct pk1 = item1.MusicTag as MPpackageStruct;
      MPpackageStruct pk2 = item2.MusicTag as MPpackageStruct;
      switch (method)
      {
        case SortMethod.Name:
          if (bAscending)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            return String.Compare(item2.Label, item1.Label, true);
          }

        case SortMethod.Type:
          if (bAscending)
          {
            return String.Compare(pk1._intalerStruct.Group, pk2._intalerStruct.Group, true);
          }
          else
          {
            return String.Compare(pk2._intalerStruct.Group, pk1._intalerStruct.Group, true);
          }
        case SortMethod.Date:
          if (bAscending)
          {
            return DateTime.Compare(pk1._intalerStruct.ProiectProperties.CreationDate, pk2._intalerStruct.ProiectProperties.CreationDate);
          }
          else
          {
            return DateTime.Compare(pk2._intalerStruct.ProiectProperties.CreationDate, pk1._intalerStruct.ProiectProperties.CreationDate);
          }
      }
      return 0;
    }
    #endregion

    #region helper func's

    void LoadDirectory(string strNewDirectory)
    {
      SwitchView();
      OnSort();
      GUIWaitCursor.Show();
      GUIListItem SelectedItem = facadeView.SelectedListItem;

      currentFolder = strNewDirectory;
      GUIControl.ClearControl(GetID, facadeView.GetID);
      
      //------------
      switch (currentListing)
      {
        case Views.Local:
          {
            lst.LoadFromFile();
            lst.NormalizeNames();
            Log.Debug("MyExtensions: loading extensions list from local file : {0}", lst.FileName);
            GUIListItem item = new GUIListItem();
            foreach (MPpackageStruct pk in lst.Items)
            {
              item = new GUIListItem();
              item.MusicTag = pk;
              item.IsFolder = false;
              item.Label = pk._intalerStruct.Name;
              item.Label2 = pk._intalerStruct.Group;
              facadeView.Add(item);
            }
          }
          break;
        case Views.Online:
          {
            if (string.IsNullOrEmpty(strNewDirectory))
            {
              GUIListItem item = new GUIListItem();
              item.Label = "All";
              item.Path = item.Label;
              item.IsFolder = true;
              item.MusicTag = null;
              item.ThumbnailImage = string.Empty;
              MediaPortal.Util.Utils.SetDefaultIcons(item);
              facadeView.Add(item);

              foreach (string s in MPinstallerStruct.CategoryListing)
              {
                item = new GUIListItem();
                item.Label = s;
                item.Path = s;
                item.IsFolder = true;
                item.MusicTag = null;
                item.ThumbnailImage = string.Empty;
                MediaPortal.Util.Utils.SetDefaultIcons(item);
                facadeView.Add(item);
              }
            }
            else
            {
              GUIListItem item = new GUIListItem();
              item.Label = "..";
              item.Path = string.Empty;
              item.IsFolder = true;
              item.MusicTag = null;
              item.ThumbnailImage = string.Empty;
              MediaPortal.Util.Utils.SetDefaultIcons(item);
              facadeView.Add(item);
              foreach (MPpackageStruct pk in lst_online.Items)
              {
                if ((pk._intalerStruct.Group == strNewDirectory || strNewDirectory == "All"))
                {
                  item = new GUIListItem();
                  item.MusicTag = pk;
                  item.IsFolder = false;
                  item.Label = pk._intalerStruct.Name;
                  item.Label2 = pk._intalerStruct.Group;
                  facadeView.Add(item);
                }
              }
            }
          }
          break;
      }

      //------------
      //set object count label
      //GUIPropertyManager.SetProperty("#itemcount", MediaPortal.Util.Utils.GetObjectCountLabel(totalItems));
      SetLabels();
      SelectCurrentItem();

      //set selected item
      if (selectedItemIndex >= 0)
        GUIControl.SelectItemControl(GetID, facadeView.GetID, selectedItemIndex);

      GUIWaitCursor.Hide();
    }

    void SelectCurrentItem()
    {
      int iItem = facadeView.SelectedListItemIndex;
      if (iItem > -1)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, iItem);
      }
      UpdateButtonStates();
    }
   
    void UpdateButtonStates()
    {
      facadeView.IsVisible = false;
      facadeView.IsVisible = true;
      GUIControl.FocusControl(GetID, facadeView.GetID);

      string strLine = string.Empty;
      View view = currentView;
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
      }
      if (btnViewAs != null)
      {
        btnViewAs.Label = strLine;
      }

      switch (currentSortMethod)
      {
        case SortMethod.Name:
          strLine = GUILocalizeStrings.Get(103);
          break;
        case SortMethod.Type:
          strLine = GUILocalizeStrings.Get(668);
          break;
        case SortMethod.Date:
          strLine = GUILocalizeStrings.Get(104);
          break;
      }
      if (btnSortBy != null)
      {
        btnSortBy.Label = strLine;
        btnSortBy.IsAscending = sortAscending;
      }
    }

    void SwitchView()
    {
      switch (currentView)
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
      }
      UpdateButtonStates(); 
    }

    void SortChanged(object sender, SortEventArgs e)
    {
      sortAscending = e.Order != System.Windows.Forms.SortOrder.Descending;

      OnSort();
      UpdateButtonStates();

      GUIControl.FocusControl(GetID, ((GUIControl)sender).GetID);
    }

    void SetLabels()
    {
      SortMethod method = currentSortMethod;
      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem item = facadeView[i];
        if (item.MusicTag != null)
        {
          MPpackageStruct pak = (MPpackageStruct)item.MusicTag;
          switch (method)
          {
            case SortMethod.Name:
              item.Label2 = pak._intalerStruct.Group;
              break;
            case SortMethod.Type:
              item.Label2 = pak._intalerStruct.Group;
              break;
            case SortMethod.Date:
              item.Label2 = pak._intalerStruct.ProiectProperties.CreationDate.ToShortDateString();
              break;
            default:
              break;
          }
          if (method == SortMethod.Name)
          {
          }
        }
      }
    }
    #endregion

  }
}
