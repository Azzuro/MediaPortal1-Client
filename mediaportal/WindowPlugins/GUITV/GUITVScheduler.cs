using System;
using System.Collections;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Player;

namespace MediaPortal.GUI.TV
{
	/// <summary>
	/// 
	/// </summary>
	public class GUITVScheduler :GUIWindow, IComparer
	{
		#region variables, ctor/dtor
    enum SortMethod
    {
      Channel=0,
      Date=1,
      Name=2,
      Type=3,
      Status=4,
    }
		[SkinControlAttribute(2)]				protected GUIButtonControl btnSortBy=null;
		[SkinControlAttribute(4)]				protected GUIToggleButtonControl btnSortAsc=null;
		[SkinControlAttribute(6)]				protected GUIButtonControl btnNew=null;
		[SkinControlAttribute(7)]				protected GUIButtonControl btnCleanup=null;
		[SkinControlAttribute(10)]			protected GUIListControl listSchedules=null;

    SortMethod        currentSortMethod=SortMethod.Date;
		bool              m_bSortAscending=true;
		int								m_iSelectedItem=0;
		
		string						currentShow=String.Empty;

    public  GUITVScheduler()
    {
      GetID=(int)GUIWindow.Window.WINDOW_SCHEDULER;
    }
    ~GUITVScheduler()
    {
    }
    
		#endregion

    #region Serialisation
    void LoadSettings()
    {
      using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        string strTmp=String.Empty;
        strTmp=(string)xmlreader.GetValue("tvscheduler","sort");
        if (strTmp!=null)
        {
          if (strTmp=="channel") currentSortMethod=SortMethod.Channel;
          else if (strTmp=="date") currentSortMethod=SortMethod.Date;
          else if (strTmp=="name") currentSortMethod=SortMethod.Name;
          else if (strTmp=="type") currentSortMethod=SortMethod.Type;
          else if (strTmp=="status") currentSortMethod=SortMethod.Status;
        }
        m_bSortAscending=xmlreader.GetValueAsBool("tvscheduler","sortascending",true);
      }
    }

    void SaveSettings()
    {
      using(MediaPortal.Profile.Xml   xmlwriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            xmlwriter.SetValue("tvscheduler","sort","channel");
            break;
          case SortMethod.Date:
            xmlwriter.SetValue("tvscheduler","sort","date");
            break;
          case SortMethod.Name:
            xmlwriter.SetValue("tvscheduler","sort","name");
            break;
          case SortMethod.Type:
            xmlwriter.SetValue("tvscheduler","sort","type");
            break;
          case SortMethod.Status:
            xmlwriter.SetValue("tvscheduler","sort","status");
            break;
        }
        xmlwriter.SetValueAsBool("tvscheduler","sortascending",m_bSortAscending);
      }
    }
    #endregion

		#region overrides
		public override bool Init()
		{
			bool bResult=Load (GUIGraphicsContext.Skin+@"\mytvscheduler.xml");
			LoadSettings();
			return bResult;
		}


    public override void OnAction(Action action)
    {
			if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				if (listSchedules.Focus)
				{
					GUIListItem item = listSchedules[0];
					if (item != null)
					{
						if (item.IsFolder && item.Label == "..")
						{
							currentShow=String.Empty;
							LoadDirectory();
							return;
						}
					}
				}
			}

      switch (action.wID)
      {
				case Action.ActionType.ACTION_SHOW_GUI:
					if ( !g_Player.Playing && Recorder.IsViewing())
					{
						//if we're watching tv
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
					}
					else if (g_Player.Playing  && g_Player.IsTV && !g_Player.IsTVRecording)
					{
						//if we're watching a tv recording
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
					}
					else if (g_Player.Playing&&g_Player.HasVideo)
					{
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
					}
        break;
      }
      base.OnAction(action);
    }
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
					
			LoadSettings();
			LoadDirectory();
					
			while (m_iSelectedItem>=GetItemCount() && m_iSelectedItem>0) m_iSelectedItem--;
			GUIControl.SelectItemControl(GetID,listSchedules.GetID,m_iSelectedItem);

		}
		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
			m_iSelectedItem=GetSelectedItemNo();
			SaveSettings();
					
			if ( !GUITVHome.IsTVWindow(newWindowId) )
			{
				if (! g_Player.Playing)
				{
					if (GUIGraphicsContext.ShowBackground)
					{
						// stop timeshifting & viewing... 
	              
						Recorder.StopViewing();
					}
				}
			}
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnSortBy) // sort by
			{
				switch (currentSortMethod)
				{
					case SortMethod.Channel:
						currentSortMethod=SortMethod.Date;
						break;
					case SortMethod.Date:
						currentSortMethod=SortMethod.Name;
						break;
					case SortMethod.Name:
						currentSortMethod=SortMethod.Type;
						break;
					case SortMethod.Type:
						currentSortMethod=SortMethod.Status;
						break;
					case SortMethod.Status:
						currentSortMethod=SortMethod.Channel;
						break;
				}
				OnSort();
			}
			if (control == listSchedules)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,control.GetID,0,0,null);
				OnMessage(msg);         
				int iItem=(int)msg.Param1;
				if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
				{
					OnClick(iItem);
				}
			}
			if (control == btnCleanup)
			{
				OnCleanup();
			}
			if (control==btnNew)
			{
				OnNewShedule();
			}

		}

    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {
				case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
					UpdateDescription();
					break;
      }
      return base.OnMessage(message);
    }
		protected override void OnShowContextMenu()
		{
			OnClick(GetSelectedItemNo());
		}


		#endregion

		#region list management 
    GUIListItem GetSelectedItem()
    {
      return listSchedules.SelectedListItem;
    }

    GUIListItem GetItem(int index)
    {
			if (index < 0 || index >=listSchedules.Count) return null;
			return listSchedules[index];
		}

    int GetSelectedItemNo()
    {
      GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,listSchedules.GetID,0,0,null);
      OnMessage(msg);         
      int iItem=(int)msg.Param1;
      return iItem;
    }
    int GetItemCount()
    {
      return listSchedules.Count;
    }
		#endregion

    #region Sort Members
    void OnSort()
    {
      SetLabels();
      listSchedules.Sort(this);
      UpdateButtonStates();
    }

    public int Compare(object x, object y)
    {
      if (x==y) return 0;
      GUIListItem item1=(GUIListItem)x;
      GUIListItem item2=(GUIListItem)y;
      if (item1==null) return -1;
      if (item2==null) return -1;
			if (item1.IsFolder && item1.Label=="..") return -1;
			if (item2.IsFolder && item2.Label=="..") return -1;
			if (item1.IsFolder && !item2.IsFolder) return -1;
			else if (!item1.IsFolder && item2.IsFolder) return 1; 

      int iComp=0;
      TimeSpan ts;
      TVRecording rec1=(TVRecording)item1.TVTag;
      TVRecording rec2=(TVRecording)item2.TVTag;
 
			//0=Recording->1=Finished->2=Waiting->3=Canceled
			int type1=2,type2=2;
			if (item1.Label3==GUILocalizeStrings.Get(682)) type1=0;
			else if (item1.Label3==GUILocalizeStrings.Get(683)) type1=1;
			else if (item1.Label3==GUILocalizeStrings.Get(681)) type1=2;
			else if (item1.Label3==GUILocalizeStrings.Get(684)) type1=3;

					
			if (item2.Label3==GUILocalizeStrings.Get(682)) type2=0;
			else if (item2.Label3==GUILocalizeStrings.Get(683)) type2=1;
			else if (item2.Label3==GUILocalizeStrings.Get(681)) type2=2;
			else if (item2.Label3==GUILocalizeStrings.Get(684)) type2=3;
			if (type1==0 && type2!=0) return -1;
			if (type2==0 && type1!=0) return 1;

      switch (currentSortMethod)
      {
        case SortMethod.Name:
          if (m_bSortAscending)
          {
            iComp=String.Compare(rec1.Title,rec2.Title,true);
            if (iComp==0) goto case SortMethod.Channel;
            else return iComp;
          }
          else
          {
            iComp=String.Compare(rec2.Title ,rec1.Title,true);
            if (iComp==0) goto case SortMethod.Channel;
            else return iComp;
          }
        
        case SortMethod.Status:
					// sort by: 0=Recording->1=Finished->2=Waiting->3=Canceled
          if (m_bSortAscending)
          {
						if (type1 < type2) return -1;
						if (type1 > type2) return 1;
          }
          else
					{
						if (type1 < type2) return 1;
						if (type1 > type2) return -1;
          }
          goto case SortMethod.Channel;

        case SortMethod.Channel:
          if (m_bSortAscending)
          {
            iComp=String.Compare(rec1.Channel,rec2.Channel,true);
            if (iComp==0) goto case SortMethod.Date;
            else return iComp;
          }
          else
          {
            iComp=String.Compare(rec2.Channel,rec1.Channel,true);
            if (iComp==0) goto case SortMethod.Date;
            else return iComp;
          }

        case SortMethod.Date:
          if (m_bSortAscending)
          {
            if (rec1.StartTime==rec2.StartTime) return 0;
            if (rec1.StartTime>rec2.StartTime) return 1;
            return -1;
          }
          else
          {
            if (rec2.StartTime==rec1.StartTime) return 0;
            if (rec2.StartTime>rec1.StartTime) return 1;
            return -1;
          }

        case SortMethod.Type:
          item1.Label2=GetRecType(rec1.RecType);
          item2.Label2=GetRecType(rec2.RecType);
          if (rec1.RecType!=rec2.RecType) 
          {
            if (m_bSortAscending)
              return (int)rec1.RecType-(int)rec2.RecType;
            else
              return (int)rec2.RecType-(int)rec1.RecType;
          }
          if (rec1.StartTime!=rec2.StartTime)
          {
            if (m_bSortAscending)
            {
              ts=rec1.StartTime - rec2.StartTime;
              return (int)(ts.Minutes);
            }
            else
            {
              ts=rec2.StartTime - rec1.StartTime;
              return (int)(ts.Minutes);
            }
          }
          if (rec1.Channel!=rec2.Channel)
            if (m_bSortAscending)
              return String.Compare(rec1.Channel,rec2.Channel);
            else
              return String.Compare(rec2.Channel,rec1.Channel);
          if (rec1.Title!=rec2.Title)
            if (m_bSortAscending)
              return String.Compare(rec1.Title,rec2.Title);
            else
              return String.Compare(rec2.Title,rec1.Title);
          return 0;
      } 
      return 0;
    }
    #endregion

		#region scheduled tv methods
		void LoadDirectory()
		{
			GUIControl.ClearControl(GetID,listSchedules.GetID);

			ArrayList itemlist = new ArrayList();
			TVDatabase.GetRecordings(ref itemlist);
			int total=0;
			if (currentShow==String.Empty)
			{
				foreach (TVRecording rec in itemlist)
				{
					ArrayList recs = ConflictManager.Util.GetRecordingTimes(rec);
					GUIListItem item= new GUIListItem();
					item.Label		  = rec.Title;
					item.TVTag			= rec;
					if (recs.Count> 1)
					{
						item.IsFolder=true;
						Utils.SetDefaultIcons(item);
					}
					else
					{
						string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,rec.Channel);
						if (!System.IO.File.Exists(strLogo))
						{
							strLogo="defaultVideoBig.png";
						}
						item.ThumbnailImage=strLogo;
						item.IconImageBig=strLogo;
						item.IconImage=strLogo;
					}
					int card;
					if (Recorder.IsRecordingSchedule(rec,out card))
					{
						if (rec.RecType !=TVRecording.RecordingType.Once)
							item.PinImage=Thumbs.TvRecordingSeriesIcon;
						else
							item.PinImage=Thumbs.TvRecordingIcon;
					}
					else
					{
						if (ConflictManager.IsConflict(rec))
						{
							item.PinImage=Thumbs.TvConflictRecordingIcon;
						}
					}
					listSchedules.Add(item);
					total++;
				}
			}
			else
			{

				GUIListItem item= new GUIListItem();
				item.Label		  = "..";
				item.IsFolder=true;
				Utils.SetDefaultIcons(item);
				listSchedules.Add(item);
				total++;

				foreach (TVRecording rec in itemlist)
				{
					if (!rec.Title.Equals(currentShow)) continue;
					ArrayList recs = ConflictManager.Util.GetRecordingTimes(rec);
					if (recs.Count>= 1)
					{
						for (int x=0; x < recs.Count;++x)
						{
							TVRecording recSeries=(TVRecording )recs[x];
							if (DateTime.Now > recSeries.EndTime) continue;
							if (recSeries.Canceled!=0) continue;						
						
							item=new GUIListItem();
							item.Label=recSeries.Title;
							item.TVTag=recSeries;
							string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,recSeries.Channel);
							if (!System.IO.File.Exists(strLogo))
							{
								strLogo="defaultVideoBig.png";
							}
							int card;
							if (Recorder.IsRecordingSchedule(recSeries,out card))
							{
								if (rec.StartTime<= DateTime.Now && rec.EndTime >= DateTime.Now)
								{
									if (rec.RecType !=TVRecording.RecordingType.Once)
										item.PinImage=Thumbs.TvRecordingSeriesIcon;
									else
										item.PinImage=Thumbs.TvRecordingIcon;
								}
							}
							else
							{
								if (ConflictManager.IsConflict(rec))
								{
									item.PinImage=Thumbs.TvConflictRecordingIcon;
								}
							}
							item.ThumbnailImage=strLogo;
							item.IconImageBig=strLogo;
							item.IconImage=strLogo;
							listSchedules.Add(item);
							total++;
						}
					}
				}
			}
      
			string strObjects=String.Format("{0} {1}", total, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
			GUIControl cntlLabel = GetControl(12);
			if (cntlLabel!=null)
				cntlLabel.YPosition = listSchedules.SpinY;

			OnSort();
			UpdateButtonStates();
		}

		void UpdateButtonStates()
		{
			string strLine=String.Empty;
			switch (currentSortMethod)
			{
				case SortMethod.Channel:
					strLine=GUILocalizeStrings.Get(620);// Sort by: Channel
					break;
				case SortMethod.Date:
					strLine=GUILocalizeStrings.Get(621);// Sort by: Date
					break;
				case SortMethod.Name:
					strLine=GUILocalizeStrings.Get(268);// Sort by: Title
					break;
				case SortMethod.Type:
					strLine=GUILocalizeStrings.Get(623);// Sort by: Type
					break;
				case SortMethod.Status:
					strLine=GUILocalizeStrings.Get(685);// Sort by: Status
					break;
			}
			GUIControl.SetControlLabel(GetID,btnSortBy.GetID,strLine);

			if (m_bSortAscending)
				btnSortAsc.Selected=false;
			else
				btnSortAsc.Selected=true;
		}

    void SetLabels()
    {
      SortMethod method=currentSortMethod;
      bool bAscending=m_bSortAscending;

      for (int i=0; i < GetItemCount();++i)
      {
        GUIListItem item=GetItem(i);
				if (item.IsFolder && item.Label.Equals("..")) continue;
        TVRecording rec=(TVRecording)item.TVTag;
        
        switch (rec.Status)
        {
          case TVRecording.RecordingStatus.Waiting:
            item.Label3=GUILocalizeStrings.Get(681);//waiting
          break;
          case TVRecording.RecordingStatus.Finished:
            item.Label3=GUILocalizeStrings.Get(683);//Finished
            break;
          case TVRecording.RecordingStatus.Canceled:
            item.Label3=GUILocalizeStrings.Get(684);//Canceled
            break;
        }

        // check with recorder.
        int card;
				if (Recorder.IsRecordingSchedule(rec, out card))
				{
					item.Label3=GUILocalizeStrings.Get(682);//Recording
						if (rec.RecType !=TVRecording.RecordingType.Once)
							item.PinImage=Thumbs.TvRecordingSeriesIcon;
						else
							item.PinImage=Thumbs.TvRecordingIcon;
				}
				else if (ConflictManager.IsConflict(rec))
				{
					item.PinImage=Thumbs.TvConflictRecordingIcon;
				}
				else 
				{
					item.PinImage=String.Empty;
				}
        
        switch (currentSortMethod)
        {
          case SortMethod.Channel:
            goto case SortMethod.Name;
          case SortMethod.Date:
            goto case SortMethod.Name;
          case SortMethod.Name:
            goto case SortMethod.Type;
          case SortMethod.Type:
            string strType=String.Empty;
            item.Label=rec.Title;
            string strTime=String.Format("{0} {1} - {2}", 
                                rec.StartTime.ToShortDateString() , 
                                rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
                                rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
            switch (rec.RecType)
            {
              case TVRecording.RecordingType.Once:
								item.Label2=String.Format("{0} {1} - {2}", 
												Utils.GetShortDayString(rec.StartTime) , 
												rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
												rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));;
                break;
              case TVRecording.RecordingType.Daily:
                strTime=String.Format("{0}-{1}", 
                                        rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
                                        rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
                strType=GUILocalizeStrings.Get(648) ;
                item.Label2=String.Format("{0} {1}",strType,strTime);
                break;

              case TVRecording.RecordingType.WeekDays:
                strTime=String.Format("{0}-{1} {2}-{3}",
                  GUILocalizeStrings.Get(657),//657=Mon
                  GUILocalizeStrings.Get(661),//661=Fri
                  rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
                  rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
                strType=GUILocalizeStrings.Get(648) ;
                item.Label2=String.Format("{0} {1}",strType,strTime);
                break;

              case TVRecording.RecordingType.Weekly:
                string day;
                switch (rec.StartTime.DayOfWeek)
                {
                  case DayOfWeek.Monday :	day = GUILocalizeStrings.Get(11);	break;
                  case DayOfWeek.Tuesday :	day = GUILocalizeStrings.Get(12);	break;
                  case DayOfWeek.Wednesday :	day = GUILocalizeStrings.Get(13);	break;
                  case DayOfWeek.Thursday :	day = GUILocalizeStrings.Get(14);	break;
                  case DayOfWeek.Friday :	day = GUILocalizeStrings.Get(15);	break;
                  case DayOfWeek.Saturday :	day = GUILocalizeStrings.Get(16);	break;
                  default:	day = GUILocalizeStrings.Get(17);	break;
                }

                strTime=String.Format("{0}-{1}", 
                                      rec.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
                                      rec.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
                strType=GUILocalizeStrings.Get(649) ;
                item.Label2=String.Format("{0} {1} {2}",strType,day,strTime);
                break;
              case TVRecording.RecordingType.EveryTimeOnThisChannel:
                item.Label=rec.Title;
                item.Label2=GUILocalizeStrings.Get(650);
                break;
              case TVRecording.RecordingType.EveryTimeOnEveryChannel:
                item.Label=rec.Title;
                item.Label2=GUILocalizeStrings.Get(651);
                break;
            }
            break;
        }
      }
    }

    void OnClick(int iItem)
		{
			m_iSelectedItem=GetSelectedItemNo();
      GUIListItem item = GetItem(iItem);
      if (item==null) return;
      TVRecording rec=item.TVTag as TVRecording;
			if (item.IsFolder)
			{
				if (item.Label.Equals(".."))
				{
					currentShow=String.Empty;
					LoadDirectory();
					return;
				}
				currentShow=rec.Title;
				LoadDirectory();
				return;
			}

      GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg==null) return;
      
      dlg.Reset();
      dlg.SetHeading(rec.Title);
			
			if (rec.Series==false)
			{
				dlg.AddLocalizedString( 618);//delete
			}
			else
			{
				dlg.AddLocalizedString( 981);//Delete this recording
				dlg.AddLocalizedString( 982);//Delete series recording
				dlg.AddLocalizedString( 888);//Episodes management
			}
			int card;
			if (Recorder.IsRecordingSchedule(rec,out card))
			{
				dlg.AddLocalizedString( 979); //Play recording from beginning
				dlg.AddLocalizedString( 980); //Play recording from live point
			}
			else
			{
				dlg.AddLocalizedString( 882);//Quality settings
			}

      dlg.DoModal( GetID);
      if (dlg.SelectedLabel==-1) return;
      switch (dlg.SelectedId)
			{
				case 888:////Episodes management
					GUITVPriorities.OnSetEpisodesToKeep(rec);
					break;
				case 882:////Quality settings
					GUITVPriorities.OnSetQuality(rec);
				break;

				case 981: //Delete this recording only
				{
					if (Recorder.IsRecordingSchedule(rec, out card))
					{
						GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
						if (null != dlgYesNo)
						{
							dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));//Delete this recording?
							dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730));//This schedule is recording. If you delete
							dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731));//the schedule then the recording is stopped.
							dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732));//are you sure
							dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

							if (dlgYesNo.IsConfirmed) 
							{
								Recorder.StopRecording(rec);
								rec.CanceledSeries.Add(Utils.datetolong(rec.StartTime));
								TVDatabase.AddCanceledSerie(rec,Utils.datetolong(rec.StartTime));
							}
						}
					}
					else
					{
						rec.CanceledSeries.Add(Utils.datetolong(rec.StartTime));
						TVDatabase.AddCanceledSerie(rec,Utils.datetolong(rec.StartTime));
					}
					LoadDirectory();
				}
				break;

				case 982: //Delete series recording
					goto case 618;

        case 618: // delete entire recording
        {
					if (Recorder.IsRecordingSchedule(rec, out card))
					{
						GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
						if (null != dlgYesNo)
						{
							dlgYesNo.SetHeading(GUILocalizeStrings.Get(653));//Delete this recording?
							dlgYesNo.SetLine(1, GUILocalizeStrings.Get(730));//This schedule is recording. If you delete
							dlgYesNo.SetLine(2, GUILocalizeStrings.Get(731));//the schedule then the recording is stopped.
							dlgYesNo.SetLine(3, GUILocalizeStrings.Get(732));//are you sure
							dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

							if (dlgYesNo.IsConfirmed) 
							{
								Recorder.StopRecording(rec);
								TVDatabase.RemoveRecording(rec);
							}
						}
					}
					else
					{
						TVDatabase.RemoveRecording(rec);
					}
          LoadDirectory();
        }
        break;

				case 979: // Play recording from beginning
					Recorder.StopViewing();
					string filename=Recorder.GetRecordingFileName(rec);
					if (filename!=String.Empty)
					{
						g_Player.Play(filename);
						if (g_Player.Playing)
						{
							GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
							return;
						}
					}
					break;

				case 980: // Play recording from live point
					GUITVHome.IsTVOn=true;
					GUITVHome.ViewChannel(rec.Channel);
					if (Recorder.IsViewing())
					{
						if (g_Player.Playing)
						{
							g_Player.SeekAsolutePercentage(99);
						}
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
						return;
					}
					break;
			}
			while (m_iSelectedItem>=GetItemCount() && m_iSelectedItem>0) m_iSelectedItem--;
			GUIControl.SelectItemControl(GetID,listSchedules.GetID,m_iSelectedItem);
    }
    
    void ChangeType(TVRecording rec)
    {
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(616));//616=Select Recording type
        //611=Record once
        //612=Record everytime on this channel
        //613=Record everytime on every channel
        //614=Record every week at this time
        //615=Record every day at this time
				for (int i=611; i <= 615; ++i)
				{
					dlg.Add( GUILocalizeStrings.Get(i));
        }
				dlg.Add( GUILocalizeStrings.Get(672));// 672=Record Mon-Fri
        switch (rec.RecType)
        {
          case TVRecording.RecordingType.Once:
            dlg.SelectedLabel=0;
          break;
          case TVRecording.RecordingType.EveryTimeOnThisChannel:
            dlg.SelectedLabel=1;
            break;
          case TVRecording.RecordingType.EveryTimeOnEveryChannel:
            dlg.SelectedLabel=2;
            break;
          case TVRecording.RecordingType.Weekly:
            dlg.SelectedLabel=3;
            break;
          case TVRecording.RecordingType.Daily:
            dlg.SelectedLabel=4;
            break;
          case TVRecording.RecordingType.WeekDays:
            dlg.SelectedLabel=5;
            break;
        }
				dlg.DoModal( GetID);
				if (dlg.SelectedLabel==-1) return;
				switch (dlg.SelectedLabel)
				{
					case 0://once
						rec.RecType=TVRecording.RecordingType.Once;
            rec.Canceled=0;
            break;
					case 1://everytime, this channel
						rec.RecType=TVRecording.RecordingType.EveryTimeOnThisChannel;
            rec.Canceled=0;
            break;
					case 2://everytime, all channels
						rec.RecType=TVRecording.RecordingType.EveryTimeOnEveryChannel;
            rec.Canceled=0;
            break;
					case 3://weekly
						rec.RecType=TVRecording.RecordingType.Weekly;
            rec.Canceled=0;
            break;
					case 4://daily
						rec.RecType=TVRecording.RecordingType.Daily;
            rec.Canceled=0;
            break;
          case 5://Mo-Fi
            rec.RecType=TVRecording.RecordingType.WeekDays;
            rec.Canceled=0;
            break;
				}
				TVDatabase.UpdateRecording(rec);
        LoadDirectory();

			}
		}

    void OnNewShedule()
    {
			//Channel
			//Type (once,daily,....)
			//Date
			//Time
			//Duration
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg==null) return;
      
			dlg.Reset();
			dlg.SetHeading(GUILocalizeStrings.Get(891));//Select TV Channel
			ArrayList channels = new ArrayList();
			TVDatabase.GetChannels(ref channels);
			foreach (TVChannel chan in channels)
			{
				GUIListItem item = new GUIListItem(chan.Name);
				string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,chan.Name);
				if (!System.IO.File.Exists(strLogo))
				{
					strLogo="defaultVideoBig.png";
				}
				item.ThumbnailImage=strLogo;
				item.IconImageBig=strLogo;
				item.IconImage=strLogo;
				dlg.Add(item);
			}
			dlg.DoModal(GetID);
			if (dlg.SelectedLabel<0) return;

			TVChannel selectedChannel=channels[dlg.SelectedLabel] as TVChannel;
			dlg.Reset();
			dlg.SetHeading(616);//select recording type
			for (int i=611; i <= 615; ++i)
			{
				dlg.Add( GUILocalizeStrings.Get(i));
			}
			dlg.Add( GUILocalizeStrings.Get(672));// 672=Record Mon-Fri
				
			dlg.DoModal( GetID);
			if (dlg.SelectedLabel==-1) return;
			TVRecording rec=new TVRecording();
			
			rec.Channel=selectedChannel.Name;
			switch (dlg.SelectedLabel)
			{
				case 0://once
					rec.RecType=TVRecording.RecordingType.Once;
					break;
				case 1://everytime, this channel
					rec.RecType=TVRecording.RecordingType.EveryTimeOnThisChannel;
					break;
				case 2://everytime, all channels
					rec.RecType=TVRecording.RecordingType.EveryTimeOnEveryChannel;
					break;
				case 3://weekly
					rec.RecType=TVRecording.RecordingType.Weekly;
					break;
				case 4://daily
					rec.RecType=TVRecording.RecordingType.Daily;
					break;
				case 5://Mo-Fi
					rec.RecType=TVRecording.RecordingType.WeekDays;
					break;
			}
			//Date
			dlg.Reset();
			dlg.SetHeading(636);//select day
			dlg.ShowQuickNumbers=false;
			DateTime dtNow =DateTime.Now;
			int day;
			for (day=0; day < 30;day++)
			{
				if (day>0)
					dtNow=DateTime.Now.AddDays(day);
					dlg.Add( dtNow.ToLongDateString());
			}
			dlg.DoModal( GetID);
			if (dlg.SelectedLabel==-1) return;
			day=dlg.SelectedLabel;
			
			dlg.Reset();
			dlg.SetHeading(142);//select time
			dlg.ShowQuickNumbers=false;
			//time
			int no=0;
			int hour,minute;
			for (hour=0; hour <= 23; hour++)
			{
				for (minute=0; minute < 60; minute+=15)
				{
					if (hour==0 && minute==0) continue;
					string time="";
					if (hour<10) time="0"+hour.ToString();
					else time=hour.ToString();
					time+=":";
					if (minute<10) time=time+ "0"+minute.ToString();
					else time+= minute.ToString();

					if (hour < 1) time=String.Format("{0} {1}",minute,GUILocalizeStrings.Get(3004));

					dlg.Add(time);

					if (DateTime.Now.Hour <= hour  && DateTime.Now.Minute <= minute)
						dlg.SelectedLabel=no;
					no++;
				}
			}
			dlg.DoModal( GetID);
			if (dlg.SelectedLabel==-1) return;

			int mins=(dlg.SelectedLabel+1)*15;
			hour=(mins)/60;
			minute=((mins)%60);

			
			dlg.Reset();
			dlg.SetHeading(180);//select time
			dlg.ShowQuickNumbers=false;
			//duration
			for (float hours =0.5f ; hours <=3f; hours+=0.5f)
			{
				dlg.Add(String.Format("{0} {1}", hours.ToString("f2"),GUILocalizeStrings.Get(3002) ));
			}
			dlg.DoModal( GetID);
			if (dlg.SelectedLabel==-1) return;
			int duration=(dlg.SelectedLabel+1)*30;

			dtNow =DateTime.Now.AddDays(day);
			rec.Start=Utils.datetolong( new DateTime(dtNow.Year,dtNow.Month,dtNow.Day,hour,minute,0,0) );
			rec.End=Utils.datetolong( rec.StartTime.AddMinutes(duration) );
      rec.Title=GUILocalizeStrings.Get(413);
      TVDatabase.AddRecording(ref rec);
      LoadDirectory();
    }

    void OnEdit(TVRecording rec)
    {
      GUIDialogDateTime dlg = (GUIDialogDateTime)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_DATETIME);
      if (dlg!=null)
      {
        int card;
        ArrayList channels = new ArrayList();
        TVDatabase.GetChannels(ref channels);
        dlg.SetHeading(637);
        dlg.Items.Clear();
        dlg.EnableChannel=true;
        dlg.EnableStartTime=true;
        if (Recorder.IsRecordingSchedule(rec, out card))
        {
          dlg.EnableChannel=false;
          dlg.EnableStartTime=false;
        }
        foreach (TVChannel chan in channels)
        {
          dlg.Items.Add(chan.Name);
        }
        dlg.Channel=rec.Channel;
        dlg.StartDateTime=rec.StartTime;
        dlg.EndDateTime=rec.EndTime;
        dlg.DoModal(GetID);
        if (dlg.IsConfirmed)
        {
          rec.Channel=dlg.Channel;
          rec.Start=Utils.datetolong(dlg.StartDateTime);
          rec.End=Utils.datetolong(dlg.EndDateTime);
          rec.Canceled=0;
          TVDatabase.UpdateRecording( rec);
          LoadDirectory();
        }
      }
    }

    string GetRecType(TVRecording.RecordingType recType)
    {
      string strType=String.Empty;
      switch(recType)
      {
        case TVRecording.RecordingType.Daily:
          strType=GUILocalizeStrings.Get(648);//daily
          break;
        case TVRecording.RecordingType.EveryTimeOnEveryChannel:
          strType=GUILocalizeStrings.Get(651);//Everytime on any channel
          break;
        case TVRecording.RecordingType.EveryTimeOnThisChannel:
          strType=GUILocalizeStrings.Get(650);//Everytime on this channel
          break;
        case TVRecording.RecordingType.Once:
          strType=GUILocalizeStrings.Get(647);//Once
          break;
        case TVRecording.RecordingType.WeekDays:
          strType=GUILocalizeStrings.Get(680);//Mon-Fri
          break;
        case TVRecording.RecordingType.Weekly:
          strType=GUILocalizeStrings.Get(679);//Weekly
          break;
      }
      return strType;
    }

    void OnCleanup()
    {
      int iCleaned=0;
      ArrayList itemlist = new ArrayList();
      TVDatabase.GetRecordings(ref itemlist);
      foreach (TVRecording rec in itemlist)
      {
        if (rec.IsDone() || rec.Canceled!=0)
        {
          iCleaned++;
          TVDatabase.RemoveRecording(rec);
        }
      }
      GUIDialogOK pDlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      LoadDirectory();
      if (pDlgOK!=null)
      {
        pDlgOK.SetHeading(624);
        pDlgOK.SetLine(1,String.Format("{0}{1}", GUILocalizeStrings.Get(625),iCleaned));
        pDlgOK.SetLine(2,String.Empty);
        pDlgOK.DoModal(GetID);
      }
    }

		void UpdateDescription()
		{
			TVRecording rec = new TVRecording();
			rec.SetProperties(null);
			GUIListItem pItem=GetItem( GetSelectedItemNo() );
			if (pItem==null)
			{
				return;
			}
			rec=pItem.TVTag as TVRecording;
			if (rec==null) return;
			TVProgram prog=ConflictManager.Util.GetProgramAt(rec.Channel,rec.StartTime);
			rec.SetProperties(prog);
		}
		#endregion
	}
}
