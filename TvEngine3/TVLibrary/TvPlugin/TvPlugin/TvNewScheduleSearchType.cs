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
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;


using TvDatabase;
using TvControl;

using Gentle.Common;
using Gentle.Framework;

namespace TvPlugin
{
  public class TvNewScheduleSearchType : GUIWindow
  {
    [SkinControlAttribute(2)]    protected GUIButtonControl btnQuickRecord = null;
    [SkinControlAttribute(3)]    protected GUIButtonControl btnAdvancedRecord = null;
    [SkinControlAttribute(6)]    protected GUIButtonControl btnTvGuide = null;
    [SkinControlAttribute(7)]    protected GUIButtonControl btnSearchTitle = null;
    [SkinControlAttribute(8)]    protected GUIButtonControl btnSearchKeyword = null;
    [SkinControlAttribute(9)]    protected GUIButtonControl btnSearchGenre = null;

    public TvNewScheduleSearchType()
    {
      Log.Info("newsearch ctor");
      GetID = (int)GUIWindow.Window.WINDOW_TV_SEARCHTYPE;
    }
    ~TvNewScheduleSearchType()
    {
    }

    public override bool IsTv
    {
      get
      {
        return true;
      }
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvschedulerserverSearchType.xml");

      return bResult;
    }

    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            GUIWindowManager.ShowPreviousWindow();
            return;
          }
      }
      base.OnAction(action);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnSearchTitle)
      {
        TvNewScheduleSearch.SearchFor = TvNewScheduleSearch.SearchType.Title;
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV_SEARCH);
        return;
      }
      if (control == btnSearchGenre)
      {
        TvNewScheduleSearch.SearchFor = TvNewScheduleSearch.SearchType.Genres;
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV_SEARCH);
        return;
      }
      if (control == btnSearchKeyword)
      {
        TvNewScheduleSearch.SearchFor = TvNewScheduleSearch.SearchType.KeyWord;
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TV_SEARCH);
        return;
      }
      if (control == btnTvGuide)
      {
        TvNewScheduleSearch.SearchFor = TvNewScheduleSearch.SearchType.KeyWord;
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVGUIDE);
        return;
      }
      if (control == btnQuickRecord)
      {
        OnQuickRecord();
        return;
      }
      if (control == btnAdvancedRecord)
      {
        OnAdvancedRecord();
        return;
      }
      base.OnClicked(controlId, control, actionType);
    }

    void OnQuickRecord()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(891));  //Select TV Channel
      IList channels = TVHome.Navigator.CurrentGroup.ReferringGroupMap();
      foreach (GroupMap chan in channels)
      {
        GUIListItem item = new GUIListItem(chan.ReferencedChannel().Name);
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, chan.ReferencedChannel().Name);
        if (!System.IO.File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
        item.ThumbnailImage = strLogo;
        item.IconImageBig = strLogo;
        item.IconImage = strLogo;
        dlg.Add(item);
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0) return;

      Channel selectedChannel = ((GroupMap)channels[dlg.SelectedLabel]).ReferencedChannel() as Channel;
      dlg.Reset();
      dlg.SetHeading(616);//select recording type
      for (int i = 611; i <= 615; ++i)
      {
        dlg.Add(GUILocalizeStrings.Get(i));
      }
      dlg.Add(GUILocalizeStrings.Get(672));// 672=Record Mon-Fri
      dlg.Add(GUILocalizeStrings.Get(1051));// 1051=Record Sat-Sun

      Schedule rec = new Schedule(selectedChannel.IdChannel, "", Schedule.MinSchedule, Schedule.MinSchedule);

      TvBusinessLayer layer = new TvBusinessLayer();
      rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
      rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
      rec.ScheduleType = (int)ScheduleRecordingType.Once;

      DateTime dtNow = DateTime.Now;
      int day;
      day = 0;

      dlg.Reset();
      dlg.SetHeading(142);//select time
      dlg.ShowQuickNumbers = false;
      //time
      //int no = 0;
      int hour, minute, steps;
      steps = 15;
      dlg.Add("00:00");
      for (hour = 0; hour <= 23; hour++)
      {
        for (minute = 0; minute < 60; minute += steps)
        {
          if (hour == 0 && minute == 0) continue;
          string time = "";
          if (hour < 10) time = "0" + hour.ToString();
          else time = hour.ToString();
          time += ":";
          if (minute < 10) time = time + "0" + minute.ToString();
          else time += minute.ToString();

          //if (hour < 1) time = String.Format("{0} {1}", minute, GUILocalizeStrings.Get(3004));
          dlg.Add(time);
        }
      }
      // pre-select the current time
      dlg.SelectedLabel = (DateTime.Now.Hour * (60 / steps)) + (Convert.ToInt16(DateTime.Now.Minute / steps));
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;

      int mins = (dlg.SelectedLabel) * steps;
      hour = (mins) / 60;
      minute = ((mins) % 60);


      dlg.Reset();
      dlg.SetHeading(180);//select time
      dlg.ShowQuickNumbers = false;
      //duration
      for (float hours = 0.5f; hours <= 24f; hours += 0.5f)
      {
        dlg.Add(String.Format("{0} {1}", hours.ToString("f2"), GUILocalizeStrings.Get(3002)));
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;
      int duration = (dlg.SelectedLabel + 1) * 30;


      dtNow = DateTime.Now.AddDays(day);
      rec.StartTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, hour, minute, 0, 0);
      rec.EndTime = rec.StartTime.AddMinutes(duration);
      rec.ProgramName = GUILocalizeStrings.Get(413) + " (" + rec.ReferencedChannel().Name + ")";
      rec.Persist();
      TvServer server = new TvServer();
      server.OnNewSchedule();
      GUIWindowManager.ShowPreviousWindow();
    }

    void OnAdvancedRecord()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;

      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(891));  //Select TV Channel
      IList channels = TVHome.Navigator.CurrentGroup.ReferringGroupMap();
      foreach (GroupMap chan in channels)
      {
        GUIListItem item = new GUIListItem(chan.ReferencedChannel().Name);
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, chan.ReferencedChannel().Name);
        if (!System.IO.File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
        item.ThumbnailImage = strLogo;
        item.IconImageBig = strLogo;
        item.IconImage = strLogo;
        dlg.Add(item);
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0) return;

      Channel selectedChannel = ((GroupMap)channels[dlg.SelectedLabel]).ReferencedChannel() as Channel;
      dlg.Reset();
      dlg.SetHeading(616);//select recording type
      for (int i = 611; i <= 615; ++i)
      {
        dlg.Add(GUILocalizeStrings.Get(i));
      }
      dlg.Add(GUILocalizeStrings.Get(672));// 672=Record Mon-Fri
      dlg.Add(GUILocalizeStrings.Get(1051));// 1051=Record Sat-Sun

      Schedule rec = new Schedule(selectedChannel.IdChannel, "", Schedule.MinSchedule, Schedule.MinSchedule);

      TvBusinessLayer layer = new TvBusinessLayer();
      rec.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
      rec.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);

      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;

      switch (dlg.SelectedLabel)
      {
        case 0://once
          rec.ScheduleType = (int)ScheduleRecordingType.Once;
          break;
        case 1://everytime, this channel
          rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnThisChannel;
          break;
        case 2://everytime, all channels
          rec.ScheduleType = (int)ScheduleRecordingType.EveryTimeOnEveryChannel;
          break;
        case 3://weekly
          rec.ScheduleType = (int)ScheduleRecordingType.Weekly;
          break;
        case 4://daily
          rec.ScheduleType = (int)ScheduleRecordingType.Daily;
          break;
        case 5://Mo-Fi
          rec.ScheduleType = (int)ScheduleRecordingType.WorkingDays;
          break;
        case 6://Sat-Sun
          rec.ScheduleType = (int)ScheduleRecordingType.Weekends;
          break;
      }


      DateTime dtNow = DateTime.Now;
      int day;
      dlg.Reset();
      dlg.SetHeading(636);//select day
      dlg.ShowQuickNumbers = false;

      for (day = 0; day < 30; day++)
      {
        if (day > 0)
          dtNow = DateTime.Now.AddDays(day);
        dlg.Add(dtNow.ToLongDateString());
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1)
        return;
      day = dlg.SelectedLabel;


      dlg.Reset();
      dlg.SetHeading(142);//select time
      dlg.ShowQuickNumbers = false;
      //time
      //int no = 0;
      int hour, minute, steps;
      steps = 5;
      dlg.Add("00:00");
      for (hour = 0; hour <= 23; hour++)
      {
        for (minute = 0; minute < 60; minute += steps)
        {
          if (hour == 0 && minute == 0) continue;
          string time = "";
          if (hour < 10) time = "0" + hour.ToString();
          else time = hour.ToString();
          time += ":";
          if (minute < 10) time = time + "0" + minute.ToString();
          else time += minute.ToString();

          //if (hour < 1) time = String.Format("{0} {1}", minute, GUILocalizeStrings.Get(3004));
          dlg.Add(time);
        }
      }
      // pre-select the current time
      dlg.SelectedLabel = (DateTime.Now.Hour * (60 / steps)) + (Convert.ToInt16(DateTime.Now.Minute / steps));
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;

      int mins = (dlg.SelectedLabel) * steps;
      hour = (mins) / 60;
      minute = ((mins) % 60);


      dlg.Reset();
      dlg.SetHeading(180);//select time
      dlg.ShowQuickNumbers = false;
      //duration
      for (float hours = 0.5f; hours <= 24f; hours += 0.5f)
      {
        dlg.Add(String.Format("{0} {1}", hours.ToString("f2"), GUILocalizeStrings.Get(3002)));
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;
      int duration = (dlg.SelectedLabel + 1) * 30;

      TvPriorities.OnSetQuality(rec);

      dtNow = DateTime.Now.AddDays(day);
      rec.StartTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, hour, minute, 0, 0);
      rec.EndTime = rec.StartTime.AddMinutes(duration);
      rec.ProgramName = GUILocalizeStrings.Get(413) + " (" + rec.ReferencedChannel().Name + ")";
      rec.Persist();
      TvServer server = new TvServer();
      server.OnNewSchedule();
      GUIWindowManager.ShowPreviousWindow();
    }
  }
}
