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
	/// </summary>
	public class GUITVSearch:GUIWindow, IComparer
	{
		[SkinControlAttribute(2)]				protected GUIButtonControl btnSortyBy=null;
		[SkinControlAttribute(3)]				protected GUIToggleButtonControl btnSortAsc=null;
		[SkinControlAttribute(4)]				protected GUIToggleButtonControl btnSearchByGenre=null;
		[SkinControlAttribute(5)]				protected GUIToggleButtonControl btnSearchByTitle=null;
		[SkinControlAttribute(6)]				protected GUIToggleButtonControl btnSearchByDescription=null;
		[SkinControlAttribute(7)]				protected GUISelectButtonControl btnLetter=null;
		[SkinControlAttribute(8)]				protected GUISelectButtonControl btnShow=null;
		[SkinControlAttribute(9)]				protected GUISelectButtonControl btnEpisode=null;
		[SkinControlAttribute(10)]				protected GUIListControl listView=null;
		[SkinControlAttribute(11)]				protected GUIListControl titleView=null;
		[SkinControlAttribute(12)]				protected GUILabelControl lblNumberOfItems=null;
		[SkinControlAttribute(13)]				protected GUIFadeLabel lblProgramTitle=null;
		[SkinControlAttribute(14)]				protected GUILabelControl lblProgramTime=null;
		[SkinControlAttribute(15)]				protected GUITextScrollUpControl lblProgramDescription=null;
		[SkinControlAttribute(17)]				protected GUILabelControl lblProgramGenre=null;
		[SkinControlAttribute(18)]				protected GUIImage imgTvLogo=null;

    enum SearchMode
    {
      Genre, 
      Title,
			Description
    }
    enum SortMethod
    {
      Name,
      Date,
      Channel
    }
    SearchMode currentSearchMode=SearchMode.Title;
    SortMethod currentSortMethod=SortMethod.Name;
    bool       sortAscending=true;
    int        currentLevel=0;
    string     currentGenre=String.Empty;
    ArrayList   listRecordings=new ArrayList();
    int        currentSearchKind=-1;
    string     currentSearchCriteria=String.Empty;
		string     filterLetter="a";
		string     filterShow=String.Empty;
		string     filterEpisode=String.Empty;

		public GUITVSearch()
    {
      GetID=(int)GUIWindow.Window.WINDOW_SEARCHTV;
		}

    public override bool Init()
    {
      bool bResult=Load (GUIGraphicsContext.Skin+@"\mytvsearch.xml");
      return bResult;
    }

    public override void OnAction(Action action)
    {
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

		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
			listRecordings.Clear();
					
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

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			listRecordings.Clear();
			currentSearchKind=-1;
			currentSearchCriteria=String.Empty;
			TVDatabase.GetRecordings(ref listRecordings);
					
			
			btnShow.RestoreSelection=false;
			btnEpisode.RestoreSelection=false;
			btnLetter.RestoreSelection=false;
			btnShow.Clear();
			btnEpisode.Clear();
			btnLetter.AddSubItem("#");
			for (char k='A'; k <='Z'; k++)
			{
				btnLetter.AddSubItem(k.ToString());
			}
			Update();
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnSearchByGenre)
			{
				btnShow.Clear();
				btnEpisode.Clear();
				currentSearchMode=SearchMode.Genre;
				currentLevel=0;
				filterEpisode=String.Empty;
				filterLetter="#";
				filterShow=String.Empty;
				currentSearchKind=-1;
				currentSearchCriteria=String.Empty;
				Update();
				GUIControl.FocusControl(GetID,btnSearchByGenre.GetID);
			}
			if (control==btnSearchByTitle)
			{
				btnShow.Clear();
				btnEpisode.Clear();
				filterEpisode=String.Empty;
				filterLetter="a";
				filterShow=String.Empty;
				currentSearchMode=SearchMode.Title;
				currentLevel=0;
				currentSearchKind=-1;
				currentSearchCriteria=String.Empty;
				Update();
				GUIControl.FocusControl(GetID,btnSearchByTitle.GetID);
			}
			if (control==btnSearchByDescription)
			{
				btnShow.Clear();
				btnEpisode.Clear();
				filterEpisode=String.Empty;
				filterLetter="#";
				filterShow=String.Empty;
				currentSearchMode=SearchMode.Description;
				currentLevel=0;
				currentSearchKind=-1;
				currentSearchCriteria=String.Empty;
				Update();
				GUIControl.FocusControl(GetID,btnSearchByDescription.GetID);
			}
			if (control==btnSortAsc)
			{
				sortAscending=!sortAscending;
				Update();
				GUIControl.FocusControl(GetID, btnSortAsc.GetID);
			}
          
			if (control==btnSortyBy)
			{
				switch ( currentSortMethod)
				{
					case SortMethod.Name: 
						currentSortMethod = SortMethod.Channel;
						break;
					case SortMethod.Channel: 
						currentSortMethod = SortMethod.Date;
						break;
					case SortMethod.Date: 
						currentSortMethod = SortMethod.Name;
						break;
				}
				Update();
				GUIControl.FocusControl(GetID, btnSortyBy.GetID);
			}

			if (control==listView || control==titleView)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,control.GetID,0,0,null);
				OnMessage(msg);         
				int iItem=(int)msg.Param1;
				if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
				{
					OnClick(iItem);
				}
			}
			if (control==btnLetter)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,controlId,0,0,null);
				OnMessage(msg);
				filterLetter=msg.Label;
				filterShow=String.Empty;
				filterEpisode=String.Empty;
				Update();
				GUIControl.FocusControl(GetID,btnLetter.GetID);
			}
			if (control==btnShow)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,controlId,0,0,null);
				OnMessage(msg);
				filterShow=msg.Label;
				filterEpisode=String.Empty;
				Update();
				GUIControl.FocusControl(GetID,btnShow.GetID);
			}
			if (control==btnEpisode)
			{
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,controlId,0,0,null);
				OnMessage(msg);
				filterEpisode=msg.Label;
				Update();
				GUIControl.FocusControl(GetID,btnEpisode.GetID);
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

    void Update()
    {
			listView.Clear();
			titleView.Clear();
      if (currentLevel==0 && currentSearchMode==SearchMode.Genre)
      {
				listView.IsVisible=true;
				titleView.IsVisible=false;
        GUIControl.FocusControl(GetID,listView.GetID);
				btnEpisode.Disabled=true;
				btnLetter.Disabled=true;
				btnShow.Disabled=true;
				lblProgramDescription.IsVisible=false;
				if (lblProgramGenre!=null) lblProgramGenre.IsVisible=false;
				lblProgramTime.IsVisible=false;
				lblProgramTitle.IsVisible=false;
				listView.Height = lblProgramDescription.YPosition - listView.YPosition;
				lblNumberOfItems.YPosition = listView.SpinY;
      }
      else
      {
				if ( filterLetter!="#")
				{
					listView.IsVisible=false;
					titleView.IsVisible=true;
					GUIControl.FocusControl(GetID,titleView.GetID);
					
					if (filterShow==String.Empty)
					{
						lblProgramDescription.IsVisible=false;
						if (lblProgramGenre!=null) lblProgramGenre.IsVisible=false;
						lblProgramTime.IsVisible=false;
						lblProgramTitle.IsVisible=false;
						if (imgTvLogo!=null) 
							imgTvLogo.IsVisible=false;
						if (titleView.SubItemCount==2)
						{
							string subItem=(string)titleView.GetSubItem(1);
							int h=Int32.Parse(subItem.Substring(1));
							GUIGraphicsContext.ScaleVertical(ref h);
							titleView.Height = h;
							h=Int32.Parse(subItem.Substring(1));
							h-=55;
							GUIGraphicsContext.ScaleVertical(ref h);
							titleView.SpinY  = titleView.YPosition+h;
							titleView.FreeResources();
							titleView.AllocResources();
						}
					}
					else
					{
						lblProgramDescription.IsVisible=true;
						if (lblProgramGenre!=null) lblProgramGenre.IsVisible=true;
						lblProgramTime.IsVisible=true;
						lblProgramTitle.IsVisible=true;
						if (imgTvLogo!=null) 
							imgTvLogo.IsVisible=true;
						if (titleView.SubItemCount==2)
						{
							string subItem=(string)titleView.GetSubItem(0);
							int h=Int32.Parse(subItem.Substring(1));
							GUIGraphicsContext.ScaleVertical(ref h);
							titleView.Height = h;
							
							h=Int32.Parse(subItem.Substring(1));
							h-=50;
							GUIGraphicsContext.ScaleVertical(ref h);
							titleView.SpinY  = titleView.YPosition+h;
							
							titleView.FreeResources();
							titleView.AllocResources();
						}
						lblNumberOfItems.YPosition = titleView.SpinY;
					}
				}
				else
				{
					listView.IsVisible=true;
					titleView.IsVisible=false;
					GUIControl.FocusControl(GetID,listView.GetID);
					
					lblProgramDescription.IsVisible=false;
					if (lblProgramGenre!=null) lblProgramGenre.IsVisible=false;
					lblProgramTime.IsVisible=false;
					lblProgramTitle.IsVisible=false;
					if (imgTvLogo!=null) 
						imgTvLogo.IsVisible=false;
				}
				btnEpisode.Disabled=false;
				btnLetter.Disabled=false;
				btnShow.Disabled=false;
				lblNumberOfItems.YPosition = listView.SpinY;
				
      }

			ArrayList programs = new ArrayList();
			ArrayList episodes = new ArrayList();
			int itemCount=0;
      switch (currentSearchMode)
      {
        case SearchMode.Genre:
          if (currentLevel==0)
          {
            ArrayList genres = new ArrayList();
            TVDatabase.GetGenres(ref genres);
            foreach(string genre in genres)
            {
              GUIListItem item = new GUIListItem();
              item.IsFolder=true;
              item.Label=genre;
              item.Path=genre;
              Utils.SetDefaultIcons(item);
              listView.Add(item);
              itemCount++;
            }
          }
          else
          {
						listView.Clear();
						titleView.Clear();
						GUIListItem item = new GUIListItem();
            item.IsFolder=true;
            item.Label="..";
            item.Label2=String.Empty;
            item.Path=String.Empty;
            item.IconImage="defaultFolderBackBig.png";
            item.IconImageBig="defaultFolderBackBig.png";
						listView.Add(item);
						titleView.Add(item);

            ArrayList titles = new ArrayList();
            TVDatabase.SearchProgramsPerGenre(currentGenre,titles, currentSearchKind, currentSearchCriteria);
            foreach(TVProgram program in titles)
            {
							//dont show programs which have ended
							if (program.EndTime < DateTime.Now) continue;
							if (filterLetter!="#")
							{
								if (currentSearchMode==SearchMode.Description)
								{
									if (!program.Description.ToLower().StartsWith(filterLetter.ToLower())) continue;
								}
								else if (currentSearchMode==SearchMode.Title)
								{
									if (!program.Title.ToLower().StartsWith(filterLetter.ToLower())) continue;
								}
								bool add=true;
								foreach (TVProgram prog in programs)
								{
									if (prog.Title == program.Title)
									{
										add=false;
									}
								}
								if (!add && filterShow==String.Empty) continue;
								if (add)
								{
									programs.Add(program);
								}

								if (filterShow!=String.Empty)
								{
									if (program.Title==filterShow)
									{
										episodes.Add(program);
									}
								}
							}
							if (filterShow!=String.Empty && program.Title!=filterShow) continue;

							string strTime=String.Format("{0} {1}", 
								Utils.GetShortDayString(program.StartTime) , 
								program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
							if (filterEpisode!=String.Empty && strTime != filterEpisode) continue;
		
							strTime=String.Format("{0} {1} - {2}", 
								Utils.GetShortDayString(program.StartTime) , 
                program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
                program.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));

              item = new GUIListItem();
              item.IsFolder=false;
              item.Label=program.Title;
              item.Label2=strTime;
              item.Path=program.Title;
              item.MusicTag=program;
							bool isSerie;
              if (IsRecording(program, out isSerie))
              {
								if (isSerie)
									item.PinImage=Thumbs.TvRecordingSeriesIcon;
								else
									item.PinImage=Thumbs.TvRecordingIcon;
              }
              Utils.SetDefaultIcons(item);
              SetChannelLogo(program,ref item);
							listView.Add(item);
							titleView.Add(item);
              itemCount++;
            }
          }
          break;
          case SearchMode.Title:
          {
            ArrayList titles = new ArrayList();
            long start=Utils.datetolong( DateTime.Now );
            long end  =Utils.datetolong( DateTime.Now.AddMonths(1) );
            TVDatabase.SearchPrograms(start,end, ref titles, currentSearchKind, currentSearchCriteria);
            foreach(TVProgram program in titles)
						{
							if (filterLetter!="#")
							{
								if (currentSearchMode==SearchMode.Description)
								{
									if (!program.Description.ToLower().StartsWith(filterLetter.ToLower())) continue;
								}
								else if (currentSearchMode==SearchMode.Title)
								{
									if (!program.Title.ToLower().StartsWith(filterLetter.ToLower())) continue;
								}
								bool add=true;
								foreach (TVProgram prog in programs)
								{
									if (prog.Title == program.Title)
									{
										add=false;
									}
								}
								if (!add && filterShow==String.Empty) continue;
								if (add)
								{
									programs.Add(program);
								}

								if (filterShow!=String.Empty)
								{
									if (program.Title==filterShow)
									{
										episodes.Add(program);
									}
								}
							}

							if (filterShow!=String.Empty && program.Title!=filterShow) continue;

							string strTime=String.Format("{0} {1}", 
								Utils.GetShortDayString(program.StartTime) , 
								program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
							if (filterEpisode!=String.Empty && strTime != filterEpisode) continue;
		
							strTime=String.Format("{0} {1} - {2}", 
								Utils.GetShortDayString(program.StartTime) , 
								program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
								program.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));

              GUIListItem item = new GUIListItem();
              item.IsFolder=false;
              item.Label=program.Title;
              item.Label2=strTime;
              item.Path=program.Title;
              item.MusicTag=program;
							bool isSerie;
              if (IsRecording(program, out isSerie))
              {
                if (isSerie)
									item.PinImage=Thumbs.TvRecordingSeriesIcon;
								else
									item.PinImage=Thumbs.TvRecordingIcon;
              }
              Utils.SetDefaultIcons(item);
              SetChannelLogo(program,ref item);
							listView.Add(item);
							titleView.Add(item);
              itemCount++;
            }
          }
          break;


				case SearchMode.Description:
				{
					ArrayList titles = new ArrayList();
					long start=Utils.datetolong( DateTime.Now );
					long end  =Utils.datetolong( DateTime.Now.AddMonths(1) );
					TVDatabase.SearchProgramsByDescription(start,end, ref titles, currentSearchKind, currentSearchCriteria);
					foreach(TVProgram program in titles)
					{
						if (filterLetter!="#")
						{
							if (currentSearchMode==SearchMode.Description)
							{
								if (!program.Description.ToLower().StartsWith(filterLetter.ToLower())) continue;
							}
							else if (currentSearchMode==SearchMode.Title)
							{
								if (!program.Title.ToLower().StartsWith(filterLetter.ToLower())) continue;
							}
							bool add=true;
							foreach (TVProgram prog in programs)
							{
								if (prog.Title == program.Title)
								{
									add=false;
								}
							}
							if (!add && filterShow==String.Empty) continue;
							if (add)
							{
								programs.Add(program);
							}

							if (filterShow!=String.Empty)
							{
								if (program.Title==filterShow)
								{
									episodes.Add(program);
								}
							}
						}

						if (filterShow!=String.Empty && program.Title!=filterShow) continue;

						string strTime=String.Format("{0} {1}", 
							Utils.GetShortDayString(program.StartTime) , 
							program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
						if (filterEpisode!=String.Empty && strTime != filterEpisode) continue;
		
						strTime=String.Format("{0} {1} - {2}", 
							Utils.GetShortDayString(program.StartTime) , 
							program.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
							program.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));

						GUIListItem item = new GUIListItem();
						item.IsFolder=false;
						item.Label=program.Description;
						item.Label2=strTime;
						item.Path=program.Title;
						item.MusicTag=program;
						bool isSerie;
						if (IsRecording(program,out isSerie))
						{
							if (isSerie)
								item.PinImage=Thumbs.TvRecordingSeriesIcon;
							else
								item.PinImage=Thumbs.TvRecordingIcon;
						}
						Utils.SetDefaultIcons(item);
						SetChannelLogo(program,ref item);
						listView.Add(item);
						titleView.Add(item);
						itemCount++;
					}
				}
					break;
      }
      string strObjects=String.Format("{0} {1}", itemCount, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount",strObjects);

			btnShow.Clear();
			programs.Sort();
			int selItem=0;
			int count=0;
			foreach (TVProgram prog in programs)
			{
				btnShow.Add(prog.Title.ToString());			
				if (filterShow==prog.Title)
					selItem=count;
				count++;
			}
			GUIControl.SelectItemControl(GetID,btnShow.GetID,selItem);

			selItem=0;
			count=0;
			btnEpisode.Clear();
			foreach (TVProgram prog in episodes)
			{
				string strTime=String.Format("{0} {1}", 
					Utils.GetShortDayString(prog.StartTime) , 
					prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));
				  btnEpisode.Add(strTime.ToString());			
				if (filterEpisode==strTime)
					selItem=count;
				count++;
			}
			GUIControl.SelectItemControl(GetID,btnEpisode.GetID,selItem);
      OnSort();

      string strLine=String.Empty;
      switch (currentSortMethod)
      {
        case SortMethod.Name: 
          strLine = GUILocalizeStrings.Get(622);
          break;
        case SortMethod.Channel: 
          strLine = GUILocalizeStrings.Get(620);
          break;
        case SortMethod.Date: 
          strLine = GUILocalizeStrings.Get(621);
          break;
      }
			btnSortyBy.Label=strLine;

      if (sortAscending)
        btnSortAsc.Selected=false;
			else
				btnSortAsc.Selected=true;

			UpdateButtonStates();
    }
    #region Sort Members
    void OnSort()
    {
      
      listView.Sort(this);
      titleView.Sort(this);
    }

    public int Compare(object x, object y)
    {
      if (x==y) return 0;
      GUIListItem item1=(GUIListItem)x;
      GUIListItem item2=(GUIListItem)y;
      if (item1==null) return -1;
      if (item2==null) return -1;

      TVProgram prog1=item1.MusicTag as TVProgram;
      TVProgram prog2=item2.MusicTag as TVProgram;

      int iComp=0;
      switch (currentSortMethod)
      {
        case SortMethod.Name:
          if (sortAscending)
          { 
            iComp=String.Compare(item1.Label,item2.Label,true);
          }
          else
          {
              
            iComp=String.Compare(item2.Label,item1.Label,true);
          }
          return iComp;

        case SortMethod.Channel:
          if (prog1!=null && prog2!=null)
          {
           if (sortAscending)
           { 
             iComp=String.Compare(prog1.Channel,prog2.Channel,true);
           }
           else
           {
              
             iComp=String.Compare(prog2.Channel,prog1.Channel,true);
           }
           return iComp;
          }
        return 0;

        case SortMethod.Date:
          if (prog1!=null && prog2!=null)
          {
            if (sortAscending)
            { 
              iComp=(int)(prog1.Start-prog2.Start);
            }
            else
            {
              
              iComp=(int)(prog2.Start-prog1.Start);
            }
            return iComp;
          }
          return 0;
      }
      return iComp;
    }
    #endregion


    GUIListItem GetItem(int iItem)
    {
      if (currentLevel!=0)
        return titleView[iItem];
			return listView[iItem];
    }

    void OnClick(int iItem)
    {
      GUIListItem item = GetItem(iItem);
      if (item==null) return;
      switch (currentSearchMode)
      {
        case SearchMode.Genre:
          if (currentLevel==0)
          {
						filterLetter="#";
						filterShow=String.Empty;
						filterEpisode=String.Empty;
            currentGenre=item.Label;
            currentLevel++;
            Update();
          }
          else
          {
            if (item.IsFolder) 
						{
							filterLetter="#";
							filterShow=String.Empty;
							filterEpisode=String.Empty;
              currentGenre=String.Empty;
              currentLevel=0;
              Update();
            }
            else
            {
							TVProgram program = item.MusicTag as TVProgram;
							if (filterShow==String.Empty)
							{
								filterShow=program.Title;
								Update();
								return;
							}
              OnRecord(program);
            }
          }
        break;
				case SearchMode.Title:
				{
					TVProgram program = item.MusicTag as TVProgram;
					if (filterShow==String.Empty)
					{
						filterShow=program.Title;
						Update();
						return;
					}
					OnRecord(program);
				}
					break;
				case SearchMode.Description:
				{
					TVProgram program = item.MusicTag as TVProgram;
					
					if (filterShow==String.Empty)
					{
						filterShow=program.Title;
						Update();
						return;
					}
					OnRecord(program);
				}
					break;
      }
    }

    void SetChannelLogo(TVProgram prog, ref GUIListItem item)
    {
			string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,prog.Channel);
			if (!System.IO.File.Exists(strLogo))
			{
				strLogo="defaultVideoBig.png";
			}			
			if (filterShow==String.Empty)
			{
				strLogo=Utils.GetCoverArt(Thumbs.TVShows,prog.Title);
				if (!System.IO.File.Exists(strLogo))
				{
					strLogo="defaultVideoBig.png";
				}
			}


      item.ThumbnailImage=strLogo;
      item.IconImageBig=strLogo;
      item.IconImage=strLogo;
    }

    void OnRecord(TVProgram program)
    {
      if (program==null) return;
      GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg!=null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(616));//616=Select Recording type
			
        //610=None
        //611=Record once
        //612=Record everytime on this channel
        //613=Record everytime on every channel
        //614=Record every week at this time
        //615=Record every day at this time
        for (int i=610; i <= 615; ++i)
        {
          dlg.Add( GUILocalizeStrings.Get(i));
        }
				dlg.Add( GUILocalizeStrings.Get(672));// 672=Record Mon-Fri

        dlg.DoModal( GetID);
        if (dlg.SelectedLabel==-1) return;
        TVRecording rec=new TVRecording();
        rec.Title=program.Title;
        rec.Channel=program.Channel;
        rec.Start=program.Start;
        rec.End=program.End;

        switch (dlg.SelectedLabel)
        {
          case 0://none
						foreach (TVRecording rec1 in listRecordings)
						{
							if (rec1.IsRecordingProgram(program,true))
							{
								if (rec1.RecType != TVRecording.RecordingType.Once)
								{
									//delete specific series
									rec1.CanceledSeries.Add(program.Start);
									TVDatabase.AddCanceledSerie(rec1,program.Start);
								}
								else
								{
									//cancel recording
									rec1.Canceled=Utils.datetolong(DateTime.Now);
									TVDatabase.UpdateRecording( rec1);
								}
								Recorder.StopRecording(rec1);
							}
						}
            listRecordings.Clear();
            TVDatabase.GetRecordings(ref listRecordings);
            Update();
            return;
          case 1://once
            rec.RecType=TVRecording.RecordingType.Once;
            break;
          case 2://everytime, this channel
            rec.RecType=TVRecording.RecordingType.EveryTimeOnThisChannel;
            break;
          case 3://everytime, all channels
            rec.RecType=TVRecording.RecordingType.EveryTimeOnEveryChannel;
            break;
          case 4://weekly
            rec.RecType=TVRecording.RecordingType.Weekly;
            break;
          case 5://daily
            rec.RecType=TVRecording.RecordingType.Daily;
            break;
          case 6://Mo-Fi
            rec.RecType=TVRecording.RecordingType.WeekDays;
            break;
        }
        TVDatabase.AddRecording(ref rec);
        listRecordings.Clear();
        TVDatabase.GetRecordings(ref listRecordings);
        Update();
      }
    }

    bool IsRecording(TVProgram program, out bool isSerie)
    {
      bool isRecording=false;
			isSerie=false;
      foreach (TVRecording record in listRecordings)
      {
        if (record.IsRecordingProgram(program,true) ) 
        {
					if (record.RecType !=TVRecording.RecordingType.Once)
						isSerie=true;
          isRecording=true;
          break;
        }
      
      }
      return isRecording;
    }

    

		GUIListItem GetSelectedItem()
		{
			return titleView.SelectedListItem;
		}
		void UpdateDescription()
		{
			if (currentLevel==0 && currentSearchMode==SearchMode.Genre) return;
			GUIListItem item = GetSelectedItem();
			TVProgram prog = null;
			if (item!=null)
				prog = item.MusicTag as TVProgram;
			if (prog==null)
			{
				GUIPropertyManager.SetProperty("#TV.Search.Title",String.Empty);
				GUIPropertyManager.SetProperty("#TV.Search.Genre",String.Empty);
				GUIPropertyManager.SetProperty("#TV.Search.Time",String.Empty);
				GUIPropertyManager.SetProperty("#TV.Search.Description",String.Empty);
				GUIPropertyManager.SetProperty("#TV.Search.thumb",String.Empty);
				return;
			}

			string strTime=String.Format("{0} {1} - {2}", 
				Utils.GetShortDayString(prog.StartTime) , 
				prog.StartTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat),
				prog.EndTime.ToString("t",CultureInfo.CurrentCulture.DateTimeFormat));

			GUIPropertyManager.SetProperty("#TV.Search.Title",prog.Title);
			GUIPropertyManager.SetProperty("#TV.Search.Time",strTime);
			if (prog!=null)
			{
				GUIPropertyManager.SetProperty("#TV.Search.Description",prog.Description);
				GUIPropertyManager.SetProperty("#TV.Search.Genre",prog.Genre);
			}
			else
			{
				GUIPropertyManager.SetProperty("#TV.Search.Description",String.Empty);
				GUIPropertyManager.SetProperty("#TV.Search.Genre",String.Empty);
			}

    
			string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,prog.Channel);
			if (System.IO.File.Exists(strLogo))
			{
				GUIPropertyManager.SetProperty("#TV.Search.thumb",strLogo);
			}
			else
			{
				GUIPropertyManager.SetProperty("#TV.Search.thumb","defaultVideoBig.png");
			}
		}

		void UpdateButtonStates()
		{
			btnSearchByDescription.Selected=false;
			btnSearchByTitle.Selected=false;
			btnSearchByGenre.Selected=false;

			if (currentSearchMode==SearchMode.Title)
				btnSearchByTitle.Selected=true;
			if (currentSearchMode==SearchMode.Description)
				btnSearchByDescription.Selected=true;
			if (currentSearchMode==SearchMode.Genre)
				btnSearchByGenre.Selected=true;
					
		}
	}
}
