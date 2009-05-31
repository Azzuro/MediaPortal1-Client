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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Services;
using MediaPortal.Threading;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Util;
using MediaPortal.Video.Database;

namespace MediaPortal.GUI.TV
{

  #region ThumbCacher

  public class RecordingThumbCacher
  {
    private Work work;

    public RecordingThumbCacher()
    {
      work = new Work(new DoWorkHandler(this.PerformRequest));
      work.ThreadPriority = ThreadPriority.Lowest;
      GlobalServiceProvider.Get<IThreadPool>().Add(work, QueuePriority.Low);
    }

    private void PerformRequest()
    {
      List<TVRecorded> recordings = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref recordings);
      foreach (TVRecorded rec in recordings)
      {
        string thumbNail = Util.Utils.GetCoverArtName(Thumbs.TVRecorded,
                                                      Util.Utils.SplitFilename(Path.ChangeExtension(rec.FileName,
                                                                                                    Util.Utils.
                                                                                                      GetThumbExtension())));
        if (!File.Exists(thumbNail))
        {
          Log.Debug("GUIRecordedTV: No thumbnail found at {0} for recording {1} - grabbing from file now", thumbNail,
                    rec.FileName);
          if (!DvrMsImageGrabber.GrabFrame(rec.FileName, thumbNail))
          {
            Log.Info("GUIRecordedTV: No thumbnail created for {0}", Util.Utils.SplitFilename(rec.FileName));
          }
        }
      }
    }
  }

  #endregion

  /// <summary>
  /// shows recorded content and program information (wheren provided)
  /// from in-built TV engine database or that from TVServer
  /// </summary>
  public class GUIRecordedTV : GUIWindow, IComparer<GUIListItem>
  {
    public GUIRecordedTV()
    {
      GetID = (int) Window.WINDOW_RECORDEDTV;
    }

    #region enums

    private enum SortMethod
    {
      Channel = 0,
      Date = 1,
      Name = 2,
      Genre = 3,
      Played = 4,
      Duration = 5
    }

    private enum ViewAs
    {
      List,
      Album,
      BigIcon,
    }

    #endregion

    #region variables

    private ViewAs currentViewMethod = ViewAs.Album;
    private SortMethod currentSortMethod = SortMethod.Date;
    private bool m_bSortAscending = true;
    private bool _deleteWatchedShows = false;
    private bool _createRecordedThumbs = true;
    private int m_iSelectedItem = 0;
    private string currentShow = string.Empty;
    private RecordingThumbCacher thumbworker = null;
    [SkinControl(2)] protected GUIButtonControl btnViewAs = null;
    [SkinControl(3)] protected GUISortButtonControl btnSortBy = null;
    [SkinControl(5)] protected GUIButtonControl btnView = null;
    [SkinControl(6)] protected GUIButtonControl btnCleanup = null;
    [SkinControl(50)] protected GUIFacadeControl facadeView = null;

    #endregion

    #region Serialisation

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string strTmp = string.Empty;
        strTmp = (string) xmlreader.GetValue("tvrecorded", "sort");
        if (strTmp != null)
        {
          if (strTmp == "channel")
          {
            currentSortMethod = SortMethod.Channel;
          }
          else if (strTmp == "date")
          {
            currentSortMethod = SortMethod.Date;
          }
          else if (strTmp == "name")
          {
            currentSortMethod = SortMethod.Name;
          }
          else if (strTmp == "type")
          {
            currentSortMethod = SortMethod.Genre;
          }
          else if (strTmp == "played")
          {
            currentSortMethod = SortMethod.Played;
          }
          else if (strTmp == "duration")
          {
            currentSortMethod = SortMethod.Duration;
          }
        }
        strTmp = (string) xmlreader.GetValue("tvrecorded", "view");
        if (strTmp != null)
        {
          if (strTmp == "album")
          {
            currentViewMethod = ViewAs.Album;
            facadeView.View = GUIFacadeControl.ViewMode.AlbumView;
          }
          else
          {
            currentViewMethod = ViewAs.List;
            facadeView.View = GUIFacadeControl.ViewMode.List;
          }
        }

        m_bSortAscending = xmlreader.GetValueAsBool("tvrecorded", "sortascending", true);
        _deleteWatchedShows = xmlreader.GetValueAsBool("capture", "deletewatchedshows", false);
        _createRecordedThumbs = xmlreader.GetValueAsBool("thumbnails", "tvrecordedondemand", true);
      }
      thumbworker = null;
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            xmlwriter.SetValue("tvrecorded", "sort", "channel");
            break;
          case SortMethod.Date:
            xmlwriter.SetValue("tvrecorded", "sort", "date");
            break;
          case SortMethod.Name:
            xmlwriter.SetValue("tvrecorded", "sort", "name");
            break;
          case SortMethod.Genre:
            xmlwriter.SetValue("tvrecorded", "sort", "type");
            break;
          case SortMethod.Played:
            xmlwriter.SetValue("tvrecorded", "sort", "played");
            break;
          case SortMethod.Duration:
            xmlwriter.SetValue("tvrecorded", "sort", "duration");
            break;
        }
        switch (currentViewMethod)
        {
          case ViewAs.Album:
            xmlwriter.SetValue("tvrecorded", "view", "album");
            break;
          case ViewAs.List:
            xmlwriter.SetValue("tvrecorded", "view", "list");
            break;
            //          case ViewAs.BigIcon:
            //            xmlwriter.SetValue("tvrecorded", "view", "bigicon");
            //            break;
        }
        xmlwriter.SetValueAsBool("tvrecorded", "sortascending", m_bSortAscending);
      }
    }

    #endregion

    #region overrides

    public override bool Init()
    {
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(OnPlayRecordingBackStopped);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(OnPlayRecordingBackEnded);
      g_Player.PlayBackStarted += new g_Player.StartedHandler(OnPlayRecordingBackStarted);
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvrecordedtv_TVE2.xml");
      //LoadSettings();
      return bResult;
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        if (facadeView.Focus)
        {
          GUIListItem item = GetItem(0);
          if (item != null)
          {
            if (item.IsFolder && item.Label == "..")
            {
              currentShow = string.Empty;
              LoadDirectory();
              return;
            }
          }
        }
      }
      switch (action.wID)
      {
        case Action.ActionType.ACTION_DELETE_ITEM:
          {
            int item = GetSelectedItemNo();
            if (item >= 0)
            {
              OnDeleteRecording(item);
            }
            UpdateProperties();
          }
          break;
      }
      base.OnAction(action);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      m_iSelectedItem = GetSelectedItemNo();
      SaveSettings();
      if (!GUIGraphicsContext.IsTvWindow(newWindowId))
      {
        if (Recorder.IsViewing() && !(Recorder.IsTimeShifting() || Recorder.IsRecording()))
        {
          if (GUIGraphicsContext.ShowBackground)
          {
            // stop timeshifting & viewing... 
            Recorder.StopViewing();
          }
        }
      }
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      LoadSettings();
      LoadDirectory();
      if (Recorder.IsViewing())
      {
        GUIControl cntl = GetControl(300);
        if (cntl != null)
        {
          cntl.Visible = true;
        }
        cntl = GetControl(99);
        if (cntl != null)
        {
          cntl.Visible = true;
        }
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int) Window.WINDOW_TV, GetID, 0, 0, 0,
                                        null);
        msg.SendToTargetWindow = true;
        GUIWindowManager.SendThreadMessage(msg);
      }
      else
      {
        GUIControl cntl = GetControl(300);
        if (cntl != null)
        {
          cntl.Visible = false;
        }
        cntl = GetControl(99);
        if (cntl != null)
        {
          cntl.Visible = false;
        }
      }
      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0)
      {
        m_iSelectedItem--;
      }
      GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iSelectedItem);
      btnSortBy.SortChanged += new SortEventHandler(SortChanged);
      if (thumbworker == null)
      {
        if (_createRecordedThumbs)
        {
          thumbworker = new RecordingThumbCacher();
        }
      }
      else
      {
        Log.Debug("GUIRecordedTV: thumbworker already running - didn't start another one");
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnView)
      {
        ShowViews();
        return;
      }
      if (control == btnViewAs)
      {
        switch (currentViewMethod)
        {
          case ViewAs.Album:
            currentViewMethod = ViewAs.List;
            break;
          case ViewAs.List:
            currentViewMethod = ViewAs.Album;
            break;
        }
        LoadDirectory();
      }
      if (control == btnSortBy) // sort by
      {
        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            currentSortMethod = SortMethod.Date;
            break;
          case SortMethod.Date:
            currentSortMethod = SortMethod.Name;
            break;
          case SortMethod.Name:
            currentSortMethod = SortMethod.Genre;
            break;
          case SortMethod.Genre:
            currentSortMethod = SortMethod.Played;
            break;
          case SortMethod.Played:
            currentSortMethod = SortMethod.Duration;
            break;
          case SortMethod.Duration:
            currentSortMethod = SortMethod.Channel;
            break;
        }
        OnSort();
      }
      if (control == btnCleanup)
      {
        OnDeleteWatchedRecordings();
      }
      if (control == facadeView)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, control.GetID, 0, 0,
                                        null);
        OnMessage(msg);
        int iItem = (int) msg.Param1;
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnPlayRecording(iItem);
        }
        if (actionType == Action.ActionType.ACTION_SHOW_INFO)
        {
          OnShowContextMenu();
        }
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
          UpdateProperties();
          break;
      }
      return base.OnMessage(message);
    }

    protected override void OnShowContextMenu()
    {
      int iItem = GetSelectedItemNo();
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null)
      {
        return;
      }
      if (pItem.IsFolder)
      {
        return;
      }
      TVRecorded rec = (TVRecorded) pItem.TVTag;
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(rec.Title);
      dlg.AddLocalizedString(655); //Play recorded tv
      dlg.AddLocalizedString(656); //Delete recorded tv
      if (pItem.IsPlayed)
      {
        dlg.AddLocalizedString(830); //Reset watched status
      }
      dlg.AddLocalizedString(1048); //Settings
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }
      switch (dlg.SelectedId)
      {
        case 656: // delete
          {
            OnDeleteRecording(iItem);
          }
          break;

        case 655: // play
          {
            if (OnPlayRecording(iItem))
            {
              return;
            }
          }
          break;

        case 1048: // Settings
          {
            GUITvRecordedInfo.CurrentProgram = rec;
            GUIWindowManager.ActivateWindow((int) Window.WINDOW_TV_RECORDED_INFO, true);
          }
          break;

        case 830: // Reset watched status
          {
            m_iSelectedItem = GetSelectedItemNo();
            ResetWatchedStatus(rec.FileName);
            LoadDirectory();
            GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iSelectedItem);
          }
          break;
      }
    }

    #endregion

    #region recording methods

    private void ShowViews()
    {
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }
      dlg.Reset();
      dlg.SetHeading(652); // my recorded tv
      dlg.AddLocalizedString(914);
      dlg.AddLocalizedString(135);
      dlg.AddLocalizedString(915);
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
      {
        return;
      }
      int nNewWindow = GetID;
      switch (dlg.SelectedId)
      {
        case 914: //	all
          nNewWindow = (int) Window.WINDOW_RECORDEDTV;
          break;
        case 135: //	genres
          nNewWindow = (int) Window.WINDOW_RECORDEDTVGENRE;
          break;
        case 915: //	channel
          nNewWindow = (int) Window.WINDOW_RECORDEDTVCHANNEL;
          break;
      }
      if (nNewWindow != GetID)
      {
        GUIWindowManager.ReplaceWindow(nNewWindow);
      }
    }

    private void LoadDirectory()
    {
      GUIWaitCursor.Show();
      try
      {
        String strDefaultUnseenIcon = GUIGraphicsContext.Skin + @"\Media\defaultVideoBig.png";
        String strDefaultSeenIcon = GUIGraphicsContext.Skin + @"\Media\defaultVideoSeenBig.png";
        GUIControl.ClearControl(GetID, facadeView.GetID);
        List<TVRecorded> recordings = new List<TVRecorded>();
        List<GUIListItem> itemlist = new List<GUIListItem>();
        TVDatabase.GetRecordedTV(ref recordings);
        if (currentShow == string.Empty)
        {
          foreach (TVRecorded rec in recordings)
          {
            bool add = true;
            foreach (GUIListItem item in itemlist)
            {
              TVRecorded rec2 = item.TVTag as TVRecorded;
              if (rec.Title.Equals(rec2.Title))
              {
                item.IsFolder = true;
                Util.Utils.SetDefaultIcons(item);
                string strLogo = Util.Utils.GetCoverArt(Thumbs.TVShows, rec.Title);
                if (File.Exists(strLogo))
                {
                  item.ThumbnailImage = strLogo;
                  item.IconImageBig = strLogo;
                  item.IconImage = strLogo;
                }
                add = false;
                break;
              }
            }
            if (add)
            {
              GUIListItem item = new GUIListItem();
              item.Label = rec.Title;
              item.TVTag = rec;
              string strLogo = Util.Utils.GetCoverArt(Thumbs.TVRecorded,
                                                      Util.Utils.SplitFilename(Path.ChangeExtension(rec.FileName,
                                                                                                    Util.Utils.
                                                                                                      GetThumbExtension())));
              if (!File.Exists(strLogo))
              {
                strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, rec.Channel);
                if (!File.Exists(strLogo))
                {
                  strLogo = rec.Played > 0 ? strDefaultSeenIcon : strDefaultUnseenIcon;
                }
              }
              else
              {
                string strLogoL = Util.Utils.ConvertToLargeCoverArt(strLogo);
                if (File.Exists(strLogoL))
                {
                  item.IconImageBig = strLogoL;
                }
                else
                {
                  item.IconImageBig = strLogo;
                }
              }
              item.ThumbnailImage = strLogo;
              item.IconImage = strLogo;
              itemlist.Add(item);
            }
          }
        }
        else
        {
          GUIListItem item = new GUIListItem();
          item.Label = "..";
          item.IsFolder = true;
          Util.Utils.SetDefaultIcons(item);
          itemlist.Add(item);
          foreach (TVRecorded rec in recordings)
          {
            if (rec.Title.Equals(currentShow))
            {
              item = new GUIListItem();
              item.Label = rec.Title;
              item.TVTag = rec;
              string strLogo = Util.Utils.GetCoverArt(Thumbs.TVRecorded,
                                                      Util.Utils.SplitFilename(Path.ChangeExtension(rec.FileName,
                                                                                                    Util.Utils.
                                                                                                      GetThumbExtension())));

              if (!File.Exists(strLogo))
              {
                strLogo = Util.Utils.GetCoverArt(Thumbs.TVChannel, rec.Channel);
                if (!File.Exists(strLogo))
                {
                  strLogo = rec.Played > 0 ? strDefaultSeenIcon : strDefaultUnseenIcon;
                }
              }
              else
              {
                string strLogoL = Util.Utils.ConvertToLargeCoverArt(strLogo);
                if (File.Exists(strLogoL))
                {
                  item.IconImageBig = strLogoL;
                }
                else
                {
                  item.IconImageBig = strLogo;
                }
              }
              item.ThumbnailImage = strLogo;
              item.IconImage = strLogo;
              itemlist.Add(item);
            }
          }
        }
        foreach (GUIListItem item in itemlist)
        {
          facadeView.Add(item);
        }

        //set object count label
        GUIPropertyManager.SetProperty("#itemcount", Util.Utils.GetObjectCountLabel(itemlist.Count));

        OnSort();
        UpdateProperties();
        GUIWaitCursor.Hide();
      }
      catch (Exception ex)
      {
        GUIWaitCursor.Hide();
        Log.Error("GUIRecordedTV: An error occured while loading the recordings {0}", ex.Message);
      }
    }

    private void UpdateButtonStates()
    {
      string strLine = string.Empty;
      switch (currentSortMethod)
      {
        case SortMethod.Channel:
          strLine = GUILocalizeStrings.Get(620); //Sort by: Channel
          break;
        case SortMethod.Date:
          strLine = GUILocalizeStrings.Get(621); //Sort by: Date
          break;
        case SortMethod.Name:
          strLine = GUILocalizeStrings.Get(268); //Sort by: Title
          break;
        case SortMethod.Genre:
          strLine = GUILocalizeStrings.Get(678); //Sort by: Genre
          break;
        case SortMethod.Played:
          strLine = GUILocalizeStrings.Get(671); //Sort by: Watched
          break;
        case SortMethod.Duration:
          strLine = GUILocalizeStrings.Get(1017); //Sort by: Duration
          break;
      }
      GUIControl.SetControlLabel(GetID, btnSortBy.GetID, strLine);
      switch (currentViewMethod)
      {
        case ViewAs.Album:
          strLine = GUILocalizeStrings.Get(100);
          break;
        case ViewAs.List:
          strLine = GUILocalizeStrings.Get(101);
          break;
          //        case ViewAs.BigIcon:
          //          strLine = GUILocalizeStrings.Get(417);
          //          break;
      }
      GUIControl.SetControlLabel(GetID, btnViewAs.GetID, strLine);
      btnSortBy.IsAscending = m_bSortAscending;
      if (currentViewMethod == ViewAs.List)
      {
        facadeView.View = GUIFacadeControl.ViewMode.List;
      }
      else
      {
        facadeView.View = GUIFacadeControl.ViewMode.AlbumView;
      }
    }

    private void SetLabels()
    {
      SortMethod method = currentSortMethod;
      bool bAscending = m_bSortAscending;

      for (int i = 0; i < facadeView.Count; ++i)
      {
        GUIListItem item1 = facadeView[i];
        if (item1.Label == "..")
        {
          continue;
        }
        TVRecorded rec = (TVRecorded) item1.TVTag;
        item1.Label = rec.Title;
        TimeSpan ts = rec.EndTime - rec.StartTime;
        string strTime = String.Format("{0} {1} ({2})",
                                       Util.Utils.GetShortDayString(rec.StartTime),
                                       rec.StartTime.ToShortTimeString(),
                                       Util.Utils.SecondsToHMString((int) ts.TotalSeconds));
        item1.Label2 = strTime;
        if (currentViewMethod == ViewAs.Album)
        {
          if (rec.Genre != "unknown")
          {
            item1.Label3 = rec.Genre;
          }
          else
          {
            item1.Label3 = string.Empty;
          }
        }
        else
        {
          if (currentSortMethod == SortMethod.Channel)
          {
            item1.Label2 = rec.Channel;
          }
        }
        if (rec.Played > 0)
        {
          if (!item1.IsFolder)
          {
            item1.IsPlayed = true;
          }
        }
      }
    }

    private bool OnPlayRecording(int iItem)
    {
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null)
      {
        return false;
      }
      if (pItem.IsFolder)
      {
        if (pItem.Label.Equals(".."))
        {
          currentShow = string.Empty;
        }
        else
        {
          currentShow = pItem.Label;
        }
        LoadDirectory();
        return false;
      }
      TVRecorded rec = (TVRecorded) pItem.TVTag;
      if (File.Exists(rec.FileName))
      {
        Log.Info("GUIRecordedTV: play - {0}", rec.FileName);
        g_Player.Stop();
        Recorder.StopViewing();
        rec.Played++;
        TVDatabase.PlayedRecordedTV(rec);
        IMDBMovie movieDetails = new IMDBMovie();
        VideoDatabase.GetMovieInfo(rec.FileName, ref movieDetails);
        int idFile = VideoDatabase.GetFileId(rec.FileName);
        int stoptime = 0;
        if (idFile >= 0)
        {
          Log.Info("play got file id:{0} for {1}", idFile, rec.FileName);
          stoptime = VideoDatabase.GetMovieStopTime(idFile);
          if (stoptime > 0)
          {
            string title = Path.GetFileName(rec.FileName);
            if (movieDetails.Title != string.Empty)
            {
              title = movieDetails.Title;
            }

            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
            if (null == dlgYesNo)
            {
              return false;
            }
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(900)); //resume movie?
            dlgYesNo.SetLine(1, rec.Channel);
            dlgYesNo.SetLine(2, title);
            dlgYesNo.SetLine(3, GUILocalizeStrings.Get(936) + Util.Utils.SecondsToHMSString(stoptime));
            dlgYesNo.SetDefaultToYes(true);
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

            if (!dlgYesNo.IsConfirmed)
            {
              stoptime = 0;
            }
          }
        }

        Log.Info("GUIRecordedTV Play: {0}", rec.FileName);
        if (g_Player.Play(rec.FileName))
        {
          if (Util.Utils.IsVideo(rec.FileName))
          {
            g_Player.ShowFullScreenWindow();
          }
          if (stoptime > 0)
          {
            g_Player.SeekAbsolute(stoptime);
          }
          return true;
        }
      }
      return false;
    }

    private void OnDeleteRecording(int iItem)
    {
      m_iSelectedItem = GetSelectedItemNo();
      GUIListItem pItem = GetItem(iItem);
      if (pItem == null)
      {
        return;
      }
      if (pItem.IsFolder)
      {
        return;
      }
      TVRecorded rec = (TVRecorded) pItem.TVTag;
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo)
      {
        return;
      }
      if (rec.Played > 0)
      {
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));
      }
      else
      {
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(820));
      }
      dlgYesNo.SetLine(1, rec.Channel);
      dlgYesNo.SetLine(2, rec.Title);
      dlgYesNo.SetLine(3, string.Empty);
      dlgYesNo.SetDefaultToYes(false);
      dlgYesNo.DoModal(GetID);
      if (!dlgYesNo.IsConfirmed)
      {
        return;
      }
      Recorder.DeleteRecording(rec);
      LoadDirectory();
      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0)
      {
        m_iSelectedItem--;
      }
      GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iSelectedItem);
    }

    private void OnDeleteWatchedRecordings()
    {
      m_iSelectedItem = GetSelectedItemNo();
      GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      if (null == dlgYesNo)
      {
        return;
      }
      dlgYesNo.SetHeading(GUILocalizeStrings.Get(676)); //delete watched recordings?
      dlgYesNo.SetLine(1, string.Empty);
      dlgYesNo.SetLine(2, string.Empty);
      dlgYesNo.SetLine(3, string.Empty);
      dlgYesNo.SetDefaultToYes(true);
      dlgYesNo.DoModal(GetID);

      if (!dlgYesNo.IsConfirmed)
      {
        return;
      }
      List<TVRecorded> itemlist = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref itemlist);
      foreach (TVRecorded rec in itemlist)
      {
        if (rec.Played > 0)
        {
          Recorder.DeleteRecording(rec);
        }
        else if (!File.Exists(rec.FileName))
        {
          Recorder.DeleteRecording(rec);
        }
      }

      LoadDirectory();
      while (m_iSelectedItem >= GetItemCount() && m_iSelectedItem > 0)
      {
        m_iSelectedItem--;
      }
      GUIControl.SelectItemControl(GetID, facadeView.GetID, m_iSelectedItem);
    }

    private void UpdateProperties()
    {
      TVRecorded rec;
      GUIListItem pItem = GetItem(GetSelectedItemNo());
      if (pItem == null)
      {
        rec = new TVRecorded();
        rec.SetProperties();
        return;
      }
      rec = pItem.TVTag as TVRecorded;
      if (rec == null)
      {
        rec = new TVRecorded();
        rec.SetProperties();
        return;
      }
      rec.SetProperties();
    }

    #endregion

    #region album/list view management

    private GUIListItem GetSelectedItem()
    {
      //int iControl;
      //iControl = listAlbums.GetID;
      //if (currentViewMethod == ViewAs.List)
      //  iControl = listViews.GetID;
      //GUIListItem item = GUIControl.GetSelectedListItem(GetID, iControl);
      return facadeView.SelectedListItem;
    }

    private GUIListItem GetItem(int iItem)
    {
      if (iItem < 0 || iItem >= facadeView.Count)
      {
        return null;
      }
      return facadeView[iItem];
    }

    private int GetSelectedItemNo()
    {
      //int iControl;
      //iControl = listAlbums.GetID;
      //if (currentViewMethod == ViewAs.List)
      //  iControl = listViews.GetID;
      //GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
      //OnMessage(msg);
      //int iItem = (int)msg.Param1;
      //return iItem;
      return facadeView.SelectedListItemIndex;
    }

    private int GetItemCount()
    {
      return facadeView.Count;
    }

    #endregion

    #region Sort Members

    private void OnSort()
    {
      SetLabels();
      facadeView.Sort(this);
      UpdateButtonStates();
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2)
      {
        return 0;
      }
      if (item1 == null)
      {
        return -1;
      }
      if (item2 == null)
      {
        return -1;
      }
      if (item1.IsFolder && item1.Label == "..")
      {
        return -1;
      }
      if (item2.IsFolder && item2.Label == "..")
      {
        return -1;
      }
      if (item1.IsFolder && !item2.IsFolder)
      {
        return -1;
      }
      else if (!item1.IsFolder && item2.IsFolder)
      {
        return 1;
      }

      int iComp = 0;
      TimeSpan ts;
      TVRecorded rec1 = (TVRecorded) item1.TVTag;
      TVRecorded rec2 = (TVRecorded) item2.TVTag;
      switch (currentSortMethod)
      {
        case SortMethod.Played:
          item1.Label2 = String.Format("{0} {1}", rec1.Played, GUILocalizeStrings.Get(677)); //times
          item2.Label2 = String.Format("{0} {1}", rec2.Played, GUILocalizeStrings.Get(677)); //times
          if (rec1.Played == rec2.Played)
          {
            goto case SortMethod.Name;
          }
          else
          {
            if (m_bSortAscending)
            {
              return rec1.Played - rec2.Played;
            }
            else
            {
              return rec2.Played - rec1.Played;
            }
          }

        case SortMethod.Name:
          if (m_bSortAscending)
          {
            iComp = String.Compare(rec1.Title, rec2.Title, true);
            if (iComp == 0)
            {
              goto case SortMethod.Channel;
            }
            else
            {
              return iComp;
            }
          }
          else
          {
            iComp = String.Compare(rec2.Title, rec1.Title, true);
            if (iComp == 0)
            {
              goto case SortMethod.Channel;
            }
            else
            {
              return iComp;
            }
          }


        case SortMethod.Channel:
          if (m_bSortAscending)
          {
            iComp = String.Compare(rec1.Channel, rec2.Channel, true);
            if (iComp == 0)
            {
              goto case SortMethod.Date;
            }
            else
            {
              return iComp;
            }
          }
          else
          {
            iComp = String.Compare(rec2.Channel, rec1.Channel, true);
            if (iComp == 0)
            {
              goto case SortMethod.Date;
            }
            else
            {
              return iComp;
            }
          }

        case SortMethod.Duration:
          {
            long duration1 = rec1.End - rec1.Start;
            long duration2 = rec2.End - rec2.Start;
            if (m_bSortAscending)
            {
              if (duration1 == duration2)
              {
                goto case SortMethod.Date;
              }
              if (duration1 > duration2)
              {
                return 1;
              }
              return -1;
            }
            else
            {
              if (duration1 == duration2)
              {
                goto case SortMethod.Date;
              }
              if (duration1 < duration2)
              {
                return 1;
              }
              return -1;
            }
          }

        case SortMethod.Date:
          if (m_bSortAscending)
          {
            if (rec1.StartTime == rec2.StartTime)
            {
              return 0;
            }
            if (rec1.StartTime < rec2.StartTime)
            {
              return 1;
            }
            return -1;
          }
          else
          {
            if (rec1.StartTime == rec2.StartTime)
            {
              return 0;
            }
            if (rec1.StartTime > rec2.StartTime)
            {
              return 1;
            }
            return -1;
          }

        case SortMethod.Genre:
          item1.Label2 = rec1.Genre;
          item2.Label2 = rec2.Genre;
          if (rec1.Genre != rec2.Genre)
          {
            if (m_bSortAscending)
            {
              return String.Compare(rec1.Genre, rec2.Genre, true);
            }
            else
            {
              return String.Compare(rec2.Genre, rec1.Genre, true);
            }
          }
          if (rec1.StartTime != rec2.StartTime)
          {
            if (m_bSortAscending)
            {
              ts = rec1.StartTime - rec2.StartTime;
              return (int) (ts.Minutes);
            }
            else
            {
              ts = rec2.StartTime - rec1.StartTime;
              return (int) (ts.Minutes);
            }
          }
          if (rec1.Channel != rec2.Channel)
          {
            if (m_bSortAscending)
            {
              return String.Compare(rec1.Channel, rec2.Channel);
            }
            else
            {
              return String.Compare(rec2.Channel, rec1.Channel);
            }
          }
          if (rec1.Title != rec2.Title)
          {
            if (m_bSortAscending)
            {
              return String.Compare(rec1.Title, rec2.Title);
            }
            else
            {
              return String.Compare(rec2.Title, rec1.Title);
            }
          }
          return 0;
      }
      return 0;
    }

    #endregion

    #region playback events

    private void OnPlayRecordingBackStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      if (type != g_Player.MediaType.Recording)
      {
        return;
      }
      int fileid = VideoDatabase.GetFileId(filename);
      if (fileid < 0)
      {
        return;
      }
      if (stoptime > 0)
      {
        VideoDatabase.SetMovieStopTime(fileid, stoptime);
      }
      else
      {
        VideoDatabase.DeleteMovieStopTime(fileid);
      }
      if (GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow))
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int) Window.WINDOW_TV, GetID, 0, 0, 0,
                                        null);
        msg.SendToTargetWindow = true;
        GUIWindowManager.SendThreadMessage(msg);
      }
    }

    private void OnPlayRecordingBackEnded(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Recording)
      {
        return;
      }
      int fileid = VideoDatabase.GetFileId(filename);
      int movieid = VideoDatabase.GetMovieId(filename);
      if (fileid < 0)
      {
        return;
      }
      if (VideoDatabase.HasMovieInfo(filename))
      {
        VideoDatabase.DeleteMovieStopTime(fileid);
      }
      g_Player.Stop();
      List<TVRecorded> itemlist = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref itemlist);
      foreach (TVRecorded rec in itemlist)
      {
        if (_deleteWatchedShows || rec.KeepRecordingMethod == TVRecorded.KeepMethod.UntilWatched)
        {
          if (String.Compare(rec.FileName, filename, true) == 0)
          {
            Recorder.DeleteRecording(rec);
            return;
          }
        }
      }
      IMDBMovie details = new IMDBMovie();
      VideoDatabase.GetMovieInfoById(movieid, ref details);
      details.Watched++;
      VideoDatabase.SetWatched(details);
    }

    private void OnPlayRecordingBackStarted(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Recording)
      {
        return;
      }
      VideoDatabase.AddMovieFile(filename);
    }

    private void ResetWatchedStatus(string filename)
    {
      if (VideoDatabase.HasMovieInfo(filename))
      {
        IMDBMovie movieDetails = new IMDBMovie();
        int idMovie = VideoDatabase.GetMovieInfo(filename, ref movieDetails);
        movieDetails.Watched = 0;
        VideoDatabase.SetWatched(movieDetails);
      }
      int fileId = VideoDatabase.GetFileId(filename);
      VideoDatabase.DeleteMovieStopTime(fileId);

      TVRecorded rec = new TVRecorded();
      TVDatabase.GetRecordedTVByFilename(filename, ref rec);
      rec.Played = 0;
      TVDatabase.PlayedRecordedTV(rec);
    }

    #endregion

    private void SortChanged(object sender, SortEventArgs e)
    {
      m_bSortAscending = e.Order != SortOrder.Descending;
      OnSort();
    }
  }
}