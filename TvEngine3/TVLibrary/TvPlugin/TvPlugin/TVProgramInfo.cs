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
using System.Globalization;
using System.Threading;

using Gentle.Common;
using Gentle.Framework;

using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

using TvDatabase;
using TvControl;


namespace TvPlugin
{
  /// <summary>
  /// Summary description for GUITVProgramInfo.
  /// </summary>
  public class TVProgramInfo : GUIWindow
  {
    [SkinControlAttribute(17)] protected GUILabelControl lblProgramGenre = null;
    [SkinControlAttribute(15)] protected GUITextScrollUpControl lblProgramDescription = null;
    [SkinControlAttribute(14)] protected GUILabelControl lblProgramTime = null;
    [SkinControlAttribute(13)] protected GUIFadeLabel lblProgramTitle = null;
    [SkinControlAttribute(16)] protected GUIFadeLabel lblProgramChannel = null;
    [SkinControlAttribute(2)]  protected GUIButtonControl btnRecord = null;
    [SkinControlAttribute(3)]  protected GUIButtonControl btnAdvancedRecord = null;
    [SkinControlAttribute(4)]  protected GUIButtonControl btnKeep = null;
    [SkinControlAttribute(5)]  protected GUIToggleButtonControl btnNotify = null;
    [SkinControlAttribute(10)] protected GUIListControl lstUpcomingEpsiodes = null;
    [SkinControlAttribute(6)]  protected GUIButtonControl btnQuality = null;
    [SkinControlAttribute(7)]  protected GUIButtonControl btnEpisodes = null;
    [SkinControlAttribute(8)]  protected GUIButtonControl btnPreRecord = null;
    [SkinControlAttribute(9)]  protected GUIButtonControl btnPostRecord = null;

    static Program currentProgram = null;

    List<int> RecordingIntervalValues = new List<int>();

    public TVProgramInfo()
    {
      GetID = (int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO;//748

      //Fill the list with all available pre & post intervals
      RecordingIntervalValues.Add(0);
      RecordingIntervalValues.Add(1);
      RecordingIntervalValues.Add(3);
      RecordingIntervalValues.Add(5);
      RecordingIntervalValues.Add(10);
      RecordingIntervalValues.Add(15);
      RecordingIntervalValues.Add(30);
      RecordingIntervalValues.Add(45);
      RecordingIntervalValues.Add(60);
      RecordingIntervalValues.Add(90);
    }

    public override void OnAdded()
    {
      Log.Debug("TVProgramInfo:OnAdded");
      GUIWindowManager.Replace((int)GUIWindow.Window.WINDOW_TV_PROGRAM_INFO, this);
      Restore();
      PreInit();
      ResetAllControls();
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
      bool bResult = Load(GUIGraphicsContext.Skin + @"\mytvprogram.xml");
      return bResult;
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      Update();
    }

    static public Program CurrentProgram
    {
      get { return currentProgram; }
      set { currentProgram = value; }
    }

    static public Schedule CurrentRecording
    {
      set
      {
        CurrentProgram = null;
        IList programs = new ArrayList();
        TvBusinessLayer layer = new TvBusinessLayer();
        programs = layer.GetPrograms(DateTime.Now, DateTime.Now.AddDays(10));
        foreach (Program prog in programs)
        {
          if (value.IsRecordingProgram(prog, false))
          {
            CurrentProgram = prog;
            return;
          }
        }
      }
    }

    void UpdateProgramDescription(Schedule rec, Program program)
    {
      if (program == null)
        return;

      string strTime = String.Format("{0} {1} - {2}",
        Utils.GetShortDayString(program.StartTime),
        program.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
        program.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      lblProgramGenre.Label = program.Genre;
      lblProgramTime.Label = strTime;
      lblProgramDescription.Label = program.Description;
      lblProgramTitle.Label = program.Title;
    }

    void Update()
    {
      GUIListItem lastSelectedItem = lstUpcomingEpsiodes.SelectedListItem;
      int itemToSelect = -1;
      lstUpcomingEpsiodes.Clear();
      if (currentProgram == null) return;

      //set program description
      string strTime = String.Format("{0} {1} - {2}",
        Utils.GetShortDayString(currentProgram.StartTime),
        currentProgram.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
        currentProgram.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));

      lblProgramGenre.Label = currentProgram.Genre;
      lblProgramTime.Label = strTime;
      lblProgramDescription.Label = currentProgram.Description;
      lblProgramTitle.Label = currentProgram.Title;

      //check if we are recording this program
      IList schedules = Schedule.ListAll();
      bool isRecording = false;
      bool isSeries = false;
      foreach (Schedule schedule in schedules)
      {
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (schedule.IsRecordingProgram(currentProgram, true))
        {
          if (!schedule.IsSerieIsCanceled(currentProgram.StartTime))
          {
            if ((ScheduleRecordingType)schedule.ScheduleType != ScheduleRecordingType.Once)
              isSeries = true;
            isRecording = true;
            break;
          }
        }
      }
      // Quality control is currently not implemented, so we don't want to confuse the user
      btnQuality.Disabled = true;
      if (isRecording)
      {
        btnRecord.Label = GUILocalizeStrings.Get(1039);//dont record
        btnAdvancedRecord.Disabled = true;
        btnKeep.Disabled = false;
        //btnQuality.Disabled = false;
        btnEpisodes.Disabled = !isSeries;
        btnPreRecord.Disabled = false;
        btnPostRecord.Disabled = false;
      }
      else
      {
        btnRecord.Label = GUILocalizeStrings.Get(264);//record
        btnAdvancedRecord.Disabled = false;
        btnKeep.Disabled = true;
        //btnQuality.Disabled = true;
        btnEpisodes.Disabled = true;
        btnPreRecord.Disabled = true;
        btnPostRecord.Disabled = true;
      }
      btnNotify.Selected = currentProgram.Notify;

      //find upcoming episodes
      lstUpcomingEpsiodes.Clear();
      TvBusinessLayer layer = new TvBusinessLayer();
      DateTime dtDay = DateTime.Now;
      IList episodes = layer.SearchMinimalPrograms(dtDay, dtDay.AddDays(14), currentProgram.Title, null);

      foreach (Program episode in episodes)
      {
        GUIListItem item = new GUIListItem();
        item.Label = episode.Title;
        item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        string logo = Utils.GetCoverArt(Thumbs.TVChannel, episode.ReferencedChannel().DisplayName);
        if (!System.IO.File.Exists(logo))
        {
          item.Label = String.Format("{0} {1}", episode.ReferencedChannel().DisplayName, episode.Title);
          logo = "defaultVideoBig.png";
        }
        Schedule recordingSchedule;
        if (IsRecordingProgram(episode, out recordingSchedule, false))
        {
          if (false == recordingSchedule.IsSerieIsCanceled(episode.StartTime))
          {
            if (recordingSchedule.ReferringConflicts().Count > 0)
            {
              item.PinImage = Thumbs.TvConflictRecordingIcon;
            }
            else
            {
              item.PinImage = Thumbs.TvRecordingIcon;
            }
          }
          item.TVTag = recordingSchedule;
        }
        item.MusicTag = episode;
        item.ThumbnailImage = logo;
        item.IconImageBig = logo;
        item.IconImage = logo;
        item.Label2 = String.Format("{0} {1} - {2}",
                                  Utils.GetShortDayString(episode.StartTime),
                                  episode.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                  episode.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat)); ;
        if (lastSelectedItem != null)
        {
          if ((item.Label == lastSelectedItem.Label) && (item.Label2 == lastSelectedItem.Label2))
            itemToSelect = lstUpcomingEpsiodes.Count;
        }
        lstUpcomingEpsiodes.Add(item);
      }
      if (itemToSelect != -1)
        lstUpcomingEpsiodes.SelectedListItemIndex = itemToSelect;
    }

    bool IsRecordingProgram(Program program, out Schedule recordingSchedule, bool filterCanceledRecordings)
    {
      recordingSchedule = null;
      IList schedules = Schedule.ListAll();
      foreach (Schedule schedule in schedules)
      {
        if (schedule.Canceled != Schedule.MinSchedule) continue;
        if (schedule.IsRecordingProgram(program, filterCanceledRecordings))
        {
          recordingSchedule = schedule;
          return true;
        }
      }
      return false;
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      if (control == btnPreRecord)
        OnPreRecordInterval();
      if (control == btnPostRecord)
        OnPostRecordInterval();
      if (control == btnEpisodes)
        OnSetEpisodes();
      // Quality control is currently not implemented, so we don't want to confuse the user
      //if (control == btnQuality)
      //  OnSetQuality();
      if (control == btnKeep)
        OnKeep();
      if (control == btnRecord)
        OnRecordProgram(currentProgram);
      if (control == btnAdvancedRecord)
        OnAdvancedRecord();
      if (control == btnNotify)
        OnNotify();
      if (control == lstUpcomingEpsiodes)
      {
        GUIListItem item = lstUpcomingEpsiodes.SelectedListItem;
        if ((item != null) && (item.MusicTag != null))
        {
					OnRecordProgram(item.MusicTag as Program);
        }
        else
          Log.Warn("TVProgrammInfo.OnClicked: item {0} was NULL!", lstUpcomingEpsiodes.SelectedItem.ToString());
      }
      
      base.OnClicked(controlId, control, actionType);
    }
    
    void OnPreRecordInterval()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false)) return;
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.ShowQuickNumbers = false;
        dlg.SetHeading(GUILocalizeStrings.Get(1444));//pre-record
        dlg.Add(GUILocalizeStrings.Get(886));//default

        foreach (int interval in RecordingIntervalValues)
        {
          if (interval == 1)
          {
            dlg.Add(String.Format("{0} {1}", interval, GUILocalizeStrings.Get(3003))); // minute
          }
          else
          {
            dlg.Add(String.Format("{0} {1}", interval, GUILocalizeStrings.Get(3004))); // minutes
          }
        }
        if (rec.PreRecordInterval < 0) dlg.SelectedLabel = 0;
        else if (RecordingIntervalValues.IndexOf(rec.PreRecordInterval) == -1) dlg.SelectedLabel = 4; // select 5 minutes if the value is not part of the list
        else dlg.SelectedLabel = RecordingIntervalValues.IndexOf(rec.PreRecordInterval) + 1;
  
        dlg.DoModal(GetID);
   
        if (dlg.SelectedLabel < 0) return;

        rec.PreRecordInterval = RecordingIntervalValues[dlg.SelectedLabel - 1];
        rec.Persist();
        
        TvServer server = new TvServer();
        server.OnNewSchedule();
      }
      Update();
    }
  
    void OnPostRecordInterval()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false)) return;
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.ShowQuickNumbers = false;
        dlg.SetHeading(GUILocalizeStrings.Get(1445));//pre-record
        dlg.Add(GUILocalizeStrings.Get(886));//default

        foreach (int interval in RecordingIntervalValues)
        {
          if (interval == 1)
          {
            dlg.Add(String.Format("{0} {1}", interval, GUILocalizeStrings.Get(3003))); // minute
          }
          else
          {
            dlg.Add(String.Format("{0} {1}", interval, GUILocalizeStrings.Get(3004))); // minutes
          }
        }
        
        if (rec.PostRecordInterval < 0) dlg.SelectedLabel = 0;
        else if (RecordingIntervalValues.IndexOf(rec.PostRecordInterval) == -1) dlg.SelectedLabel = 4; // select 5 minutes if the value is not part of the list
        else dlg.SelectedLabel = RecordingIntervalValues.IndexOf(rec.PostRecordInterval) + 1;

        dlg.DoModal(GetID);
        if (dlg.SelectedLabel < 0) return;

        rec.PostRecordInterval = RecordingIntervalValues[dlg.SelectedLabel - 1];
        rec.Persist();
      }
      Update();
    }

    void OnSetQuality()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false)) return;
      ///@
      ///GUITVPriorities.OnSetQuality(rec);
      Update();
    }

    void OnSetEpisodes()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false)) return;

      TvPriorities.OnSetEpisodesToKeep(rec);
      Update();
    }

    void OnRecordProgram(Program program)
    {
    	Log.Debug("TVProgammInfo.OnRecordProgram - programm = {0}", program.ToString());
    	Schedule recordingSchedule;
    	if (IsRecordingProgram(program, out recordingSchedule, true)) // check if schedule is already existing
    	{
    		CancelProgram(program, recordingSchedule);
    	}
    	else
    	{
    		CreateProgram(program, (int)ScheduleRecordingType.Once);
    	}
			Update();
    }

    private VirtualCard DeleteRecordingPrompt(TvServer server, Schedule schedule)
    {
      VirtualCard card = null;
      if (server.IsRecordingSchedule(schedule.IdSchedule, out card)) //check if we currently recoding this schedule
      {
        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
        if (null == dlgYesNo)
        {
          Log.Error("TVProgramInfo.DeleteRecordingPrompt: ERROR no GUIDialogYesNo found !!!!!!!!!!");
          return card;
        }
        dlgYesNo.SetHeading(GUILocalizeStrings.Get(653)); //Delete this recording?
        dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730)); //This schedule is recording. If you delete
        dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731)); //the schedule then the recording is stopped.
        dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732)); //are you sure
        dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

        if (dlgYesNo.IsConfirmed)
        {
          server.StopRecordingSchedule(schedule.IdSchedule);
        }
        else
        {
          Log.Debug("TVProgramInfo.DeleteRecordingPrompt: not confirmed");
          return card;
        }
      }
      return card;
    }


    void CancelProgram(Program program, Schedule schedule)
    {
      Log.Debug("TVProgammInfo.CancelProgram - programm = {0}", program.ToString());
      Log.Debug("                            - schedule = {0}", schedule.ToString());
      Log.Debug(" ProgramID = {0}            ScheduleID = {1}", program.IdProgram, schedule.IdSchedule);

      TvServer server = new TvServer();
      VirtualCard card = null;

      if (schedule.ScheduleType == (int)ScheduleRecordingType.Once)
      {
        card = DeleteRecordingPrompt(server, schedule);
        schedule.Delete();
      }

      else if ((schedule.ScheduleType == (int)ScheduleRecordingType.Daily)
            || (schedule.ScheduleType == (int)ScheduleRecordingType.Weekends)
            || (schedule.ScheduleType == (int)ScheduleRecordingType.Weekly)
            || (schedule.ScheduleType == (int)ScheduleRecordingType.WorkingDays))
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        if (dlg == null)
        {
          Log.Error("TVProgramInfo.CancelProgram: ERROR no GUIDialogMenu found !!!!!!!!!!");
          return;
        }

        dlg.Reset();
        dlg.SetHeading(program.Title);
        dlg.AddLocalizedString(981); //Cancel this show
        dlg.AddLocalizedString(982); //Delete this entire schedule
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1) return;
        bool deleteEntireSched = false;
        switch (dlg.SelectedId)
        {
          case 981: //delete specific series

            deleteEntireSched = false;
            break;
          case 982: //Delete entire recording
            deleteEntireSched = true;
            break;
        }
        card = DeleteRecordingPrompt(server, schedule);
        if (deleteEntireSched)
          schedule.Delete();
        else
        {
          CanceledSchedule canceledSchedule = new CanceledSchedule(schedule.IdSchedule, program.StartTime);
          canceledSchedule.Persist();
        }
      }

      if ((schedule.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnEveryChannel) || (schedule.ScheduleType == (int)ScheduleRecordingType.EveryTimeOnThisChannel))
      {
        GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
        if (dlg == null)
        {
          Log.Error("TVProgramInfo.CancelProgram: ERROR no GUIDialogMenu found !!!!!!!!!!");
          return;
        }

        dlg.Reset();
        dlg.SetHeading(program.Title);
        dlg.AddLocalizedString(981); //Cancel this show
        dlg.AddLocalizedString(982); //Delete this entire schedule
        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1) return;
        switch (dlg.SelectedId)
        {
          case 981: //delete specific series
            CanceledSchedule canceledSchedule = new CanceledSchedule(schedule.IdSchedule, program.StartTime);
            canceledSchedule.Persist();
            break;
          case 982: //Delete entire recording
            schedule.Delete();
            break;
        }
      }
      //Schedules of type "EveryTimeOnXChannel never record they only create type "Once" schedules as needed.
      //Here we check if that type "Once" associated sched has been created and if it's currently recording.
      Schedule assocSchedule = Schedule.RetrieveOnce(schedule.IdChannel, schedule.ProgramName, schedule.StartTime, schedule.EndTime);
      if (assocSchedule != null)
      {
        card = DeleteRecordingPrompt(server, assocSchedule);        
        assocSchedule.Delete();
      }
      server.OnNewSchedule();
    }
		
		void CreateProgram(Program program, int scheduleType)
		{
			Log.Debug("TVProgramInfo.CreateProgram: program = {0}", program.ToString());
		  Schedule schedule = null;
			Schedule saveSchedule = null;
			TvBusinessLayer layer = new TvBusinessLayer();
			if (IsRecordingProgram(program, out schedule, false)) // check if schedule is already existing
			{
				Log.Debug("TVProgramInfo.CreateProgram - series schedule found ID={0}, Type={1}", schedule.IdSchedule, schedule.ScheduleType);
				Log.Debug("                            - schedule= {0}", schedule.ToString());
				//schedule = Schedule.Retrieve(schedule.IdSchedule); // get the correct informations
				if (schedule.IsSerieIsCanceled(program.StartTime))
				{
					saveSchedule = schedule;
					schedule = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);
					schedule.PreRecordInterval = saveSchedule.PreRecordInterval;
					schedule.PostRecordInterval = saveSchedule.PostRecordInterval;
					schedule.ScheduleType = (int) ScheduleRecordingType.Once; // needed for layer.GetConflictingSchedules(...)
				}
			}
			else 
			{
				Log.Debug("TVProgramInfo.CreateProgram - no series schedule");
				// no series schedule => create it
				schedule = new Schedule(program.IdChannel, program.Title, program.StartTime, program.EndTime);
				schedule.PreRecordInterval = Int32.Parse(layer.GetSetting("preRecordInterval", "5").Value);
				schedule.PostRecordInterval = Int32.Parse(layer.GetSetting("postRecordInterval", "5").Value);
				schedule.ScheduleType = scheduleType;
			}
			
			// check if this program is conflicting with any other already scheduled recording
			IList conflicts = layer.GetConflictingSchedules(schedule);
			Log.Debug("TVProgramInfo.CreateProgram - conflicts.Count = {0}", conflicts.Count);
			TvServer server = new TvServer();
		  bool skipConflictingEpisodes = false;
			if (conflicts.Count > 0)
			{
				GUIDialogTVConflict dlg = (GUIDialogTVConflict) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_TVCONFLICT);
				if (dlg != null)
				{
					dlg.Reset();
					dlg.SetHeading(GUILocalizeStrings.Get(879)); // "recording conflict"
					foreach (Schedule conflict in conflicts)
					{
						Log.Debug("TVProgramInfo.CreateProgram: Conflicts = " + conflict.ToString());

						GUIListItem item = new GUIListItem(conflict.ProgramName);
						item.Label2 = GetRecordingDateTime(conflict);
						item.Label3 = conflict.IdChannel.ToString();
						item.TVTag = conflict;
						dlg.AddConflictRecording(item);
					}
				  dlg.ConflictingEpisodes = (scheduleType != (int)ScheduleRecordingType.Once);
					dlg.DoModal(GetID);
					switch (dlg.SelectedLabel)
					{
						case 0: // Skip new Recording
							{
								Log.Debug("TVProgramInfo.CreateProgram: Skip new recording");
								return;
							}
						case 1: // Don't record the already scheduled one(s)
							{
								Log.Debug("TVProgramInfo.CreateProgram: Skip old recording(s)");
								foreach (Schedule conflict in conflicts)
								{
									Program prog =
										new Program(conflict.IdChannel, conflict.StartTime, conflict.EndTime, conflict.ProgramName, "-", "-", false,
										            DateTime.MinValue, string.Empty, string.Empty, -1, string.Empty, -1);
									CancelProgram(prog, Schedule.Retrieve(conflict.IdSchedule));
								}
								break;
							}
						case 2: // Skip for conflicting episodes
							{
								Log.Debug("TVProgramInfo.CreateProgram: Skip conflicting episode(s)");
							  skipConflictingEpisodes = true;
							  break;
							}
						default: // Skipping new Recording
							{
								Log.Debug("TVProgramInfo.CreateProgram: Default => Skip new recording");
								return;
							}
					}
				}
			}

			if (saveSchedule != null)
			{
				Log.Debug("TVProgramInfo.CreateProgram - UnCancleSerie at {0}", program.StartTime);
				saveSchedule.UnCancelSerie(program.StartTime);
				saveSchedule.Persist();
			}
			else
			{
				Log.Debug("TVProgramInfo.CreateProgram - create schedule = {0}", schedule.ToString());
				schedule.Persist();
			}
      if (skipConflictingEpisodes)
      {
        List<Schedule> episodes = layer.GetRecordingTimes(schedule);
        foreach (Schedule episode in episodes)
        {
          if (DateTime.Now > episode.EndTime) continue;
          if (episode.IsSerieIsCanceled(episode.StartTime)) continue;
          foreach (Schedule conflict in conflicts)
          {
            if (episode.IsOverlapping(conflict))
            {
              Log.Debug("TVProgramInfo.CreateProgram - skip episode = {0}", episode.ToString());
              CanceledSchedule canceledSchedule = new CanceledSchedule(schedule.IdSchedule, episode.StartTime);
              canceledSchedule.Persist();
            }
          }
        }
      }
		  server.OnNewSchedule();
		}

    void OnAdvancedRecord()
    {
      if (currentProgram == null)
        return;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(616));//616=Select Schedule type
        //610=None
        //611=Record once
        //612=Record everytime on this channel
        //613=Record everytime on every channel
        //614=Record every week at this time
        //615=Record every day at this time
        for (int i = 611; i <= 615; ++i)
        {
          dlg.AddLocalizedString(i);
        }
        dlg.AddLocalizedString(672);// 672=Record Mon-Fri
        dlg.AddLocalizedString(1051);// 1051=Record Sat-Sun

        dlg.DoModal(GetID);
        if (dlg.SelectedLabel == -1) return;

				int scheduleType = (int)ScheduleRecordingType.Once;
        switch (dlg.SelectedId)
        {
          case 611://once
            scheduleType = (int)ScheduleRecordingType.Once;
            break;
          case 612://everytime, this channel
            scheduleType = (int)ScheduleRecordingType.EveryTimeOnThisChannel;
            break;
          case 613://everytime, all channels
            scheduleType = (int)ScheduleRecordingType.EveryTimeOnEveryChannel;
            break;
          case 614://weekly
            scheduleType = (int)ScheduleRecordingType.Weekly;
            break;
          case 615://daily
            scheduleType = (int)ScheduleRecordingType.Daily;
            break;
          case 672://Mo-Fi
            scheduleType = (int)ScheduleRecordingType.WorkingDays;
            break;
          case 1051://Record Sat-Sun
            scheduleType = (int)ScheduleRecordingType.Weekends;
            break;
        }
        CreateProgram(currentProgram, scheduleType);

				if (scheduleType == (int)ScheduleRecordingType.Once)
				{
					//check if this program is interrupted (for example by a news bulletin)
					//ifso ask the user if he wants to record the 2nd part also
					IList programs = new ArrayList();
					DateTime dtStart = currentProgram.EndTime.AddMinutes(1);
					DateTime dtEnd = dtStart.AddHours(3);
					TvBusinessLayer layer = new TvBusinessLayer();
					programs = layer.GetPrograms(currentProgram.ReferencedChannel(), dtStart, dtEnd);
					if (programs.Count >= 2)
					{
						Program next = programs[0] as Program;
						Program nextNext = programs[1] as Program;
						if (nextNext.Title == currentProgram.Title)
						{
							TimeSpan ts = next.EndTime - nextNext.StartTime;
							if (ts.TotalMinutes <= 40)
							{
								//
								GUIDialogYesNo dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_YES_NO);
								dlgYesNo.SetHeading(1012); //This program will be interrupted by
								dlgYesNo.SetLine(1, next.Title);
								dlgYesNo.SetLine(2, 1013); //Would you like to record the second part also?
								dlgYesNo.DoModal(GetID);
								if (dlgYesNo.IsConfirmed)
								{
									CreateProgram(nextNext, scheduleType);
									Update();
								}
							}
						}
					}
				}
      }
      Update();
    }
		   
    void OnNotify()
    {
      currentProgram.Notify = !currentProgram.Notify;
      // get the right db instance of current prog before we store it
      // currentProgram is not a ref to the real entity
      Program modifiedProg = Program.RetrieveByTitleAndTimes(currentProgram.Title, currentProgram.StartTime, currentProgram.EndTime);
      modifiedProg.Notify = currentProgram.Notify;
      modifiedProg.Persist();
      Update();
      TvNotifyManager.OnNotifiesChanged();
    }

    void OnKeep()
    {
      Schedule rec;
      if (false == IsRecordingProgram(currentProgram, out  rec, false)) return;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null) return;
      dlg.Reset();
      dlg.SetHeading(1042);
      dlg.AddLocalizedString(1043);//Until watched
      dlg.AddLocalizedString(1044);//Until space needed
      dlg.AddLocalizedString(1045);//Until date
      dlg.AddLocalizedString(1046);//Always
      switch (rec.KeepMethod)
      {
        case (int)KeepMethodType.UntilWatched:
          dlg.SelectedLabel = 0;
          break;
        case (int)KeepMethodType.UntilSpaceNeeded:
          dlg.SelectedLabel = 1;
          break;
        case (int)KeepMethodType.TillDate:
          dlg.SelectedLabel = 2;
          break;
        case (int)KeepMethodType.Always:
          dlg.SelectedLabel = 3;
          break;
      }
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel == -1) return;
      switch (dlg.SelectedId)
      {
        case 1043:
          rec.KeepMethod = (int)KeepMethodType.UntilWatched;
          break;
        case 1044:
          rec.KeepMethod = (int)KeepMethodType.UntilSpaceNeeded;

          break;
        case 1045:
          rec.KeepMethod = (int)KeepMethodType.TillDate;
          dlg.Reset();
          dlg.ShowQuickNumbers = false;
          dlg.SetHeading(1045);
          for (int iDay = 1; iDay <= 100; iDay++)
          {
            DateTime dt = currentProgram.StartTime.AddDays(iDay);
            dlg.Add(dt.ToLongDateString());
          }
          TimeSpan ts = (rec.KeepDate - currentProgram.StartTime);
          int days = (int)ts.TotalDays;
          if (days >= 100) days = 30;
          dlg.SelectedLabel = days - 1;
          dlg.DoModal(GetID);
          if (dlg.SelectedLabel < 0) return;
          rec.KeepDate = currentProgram.StartTime.AddDays(dlg.SelectedLabel + 1);
          break;
        case 1046:
          rec.KeepMethod = (int)KeepMethodType.Always;
          break;
      }
      rec.Persist();
      TvServer server = new TvServer();
      server.OnNewSchedule();

    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      if (item != null)
      {
        Program episode = null;
        if (item.MusicTag != null)
          episode = item.MusicTag as Program;

        if (episode != null)
        {
          Log.Info("TVProgrammInfo.item_OnItemSelected: {0}", episode.Title);
          UpdateProgramDescription(null, episode);
        }
        else
          Log.Warn("TVProgrammInfo.item_OnItemSelected: episode was NULL!");
      }
      else
        Log.Warn("TVProgrammInfo.item_OnItemSelected: params where NULL!");
    }

    private string GetRecordingDateTime(Schedule rec)
    {
      return String.Format("{0} {1} - {2}",
                MediaPortal.Util.Utils.GetShortDayString(rec.StartTime),
                rec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                rec.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
    }
  }
}
