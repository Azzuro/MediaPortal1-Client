using System;
using System.Collections;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Player;
using MediaPortal.Video.Database;
using Toub.MediaCenter.Dvrms.Metadata;
namespace MediaPortal.GUI.TV
{
	/// <summary>
	/// 
	/// </summary>
	public class GUITVCompressStatus :GUIWindow, IComparer
	{
		#region variables
		enum Controls
		{ 
			LABEL_PROGRAMTITLE=13,
			LABEL_PROGRAMTIME=14,
			LABEL_PROGRAMDESCRIPTION=15,
			LABEL_PROGRAMGENRE=17,
		};

		enum SortMethod
		{
			Channel=0,
			Date=1,
			Name=2,
			Genre=3,
			Played=4,
		}
		enum ViewAs
		{
			List,
			Album
		}

		ViewAs						currentViewMethod=ViewAs.Album;
		SortMethod        currentSortMethod=SortMethod.Date;
		int								m_iSelectedItem=0;
		bool              m_bSortAscending=true;
		DateTime          updateTimer=DateTime.Now;
		
		[SkinControlAttribute(2)]			  protected GUIButtonControl btnViewAs=null;
		[SkinControlAttribute(3)]				protected GUIButtonControl btnSortBy=null;
		[SkinControlAttribute(4)]				protected GUIToggleButtonControl btnSortAsc=null;
		[SkinControlAttribute(5)]				protected GUIButtonControl btnClear=null;

		[SkinControlAttribute(10)]			protected GUIListControl listAlbums=null;
		[SkinControlAttribute(11)]			protected GUIListControl listViews=null;

		#endregion
		public  GUITVCompressStatus()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TV_COMPRESS_COMPRESS_STATUS ;
		}

		#region Serialisation
		void LoadSettings()
		{
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				string strTmp=String.Empty;
				strTmp=(string)xmlreader.GetValue("tvcompressstatus","sort");
				if (strTmp!=null)
				{
					if (strTmp=="channel") currentSortMethod=SortMethod.Channel;
					else if (strTmp=="date") currentSortMethod=SortMethod.Date;
					else if (strTmp=="name") currentSortMethod=SortMethod.Name;
					else if (strTmp=="type") currentSortMethod=SortMethod.Genre;
					else if (strTmp=="played") currentSortMethod=SortMethod.Played;
				}
				strTmp=(string)xmlreader.GetValue("tvcompressstatus","view");
				if (strTmp!=null)
				{
					if (strTmp=="albu,") currentViewMethod=ViewAs.Album;
					else if (strTmp=="list") currentViewMethod=ViewAs.List;
				}
				
			}
		}

		void SaveSettings()
		{
			using(MediaPortal.Profile.Xml   xmlwriter=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				switch (currentSortMethod)
				{
					case SortMethod.Channel:
						xmlwriter.SetValue("tvcompressstatus","sort","channel");
						break;
					case SortMethod.Date:
						xmlwriter.SetValue("tvcompressstatus","sort","date");
						break;
					case SortMethod.Name:
						xmlwriter.SetValue("tvcompressstatus","sort","name");
						break;
					case SortMethod.Genre:
						xmlwriter.SetValue("tvcompressstatus","sort","type");
						break;
					case SortMethod.Played:
						xmlwriter.SetValue("tvcompressstatus","sort","played");
						break;
				}
				switch (currentViewMethod)
				{
					case ViewAs.Album:
						xmlwriter.SetValue("tvcompressstatus","view","album");
						break;
					case ViewAs.List:
						xmlwriter.SetValue("tvcompressstatus","view","list");
						break;
				}
				xmlwriter.SetValueAsBool("tvcompressstatus","sortascending",m_bSortAscending);
			}
		}
		#endregion

		#region overrides
		public override bool Init()
		{
			bool bResult=Load (GUIGraphicsContext.Skin+@"\mytvcompressstatus.xml");
			LoadSettings();
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
					else if (g_Player.Playing && g_Player.IsTV && !g_Player.IsTVRecording)
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
			base.OnPageDestroy (newWindowId);
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();

					
			LoadSettings();
			LoadDirectory();

			GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_RESUME_TV, (int)GUIWindow.Window.WINDOW_TV,GetID,0,0,0,null);
			msg.SendToTargetWindow=true;
			GUIWindowManager.SendThreadMessage(msg);
					
			while (m_iSelectedItem>=GetItemCount() && m_iSelectedItem>0) m_iSelectedItem--;
			GUIControl.SelectItemControl(GetID,listViews.GetID,m_iSelectedItem);
			GUIControl.SelectItemControl(GetID,listAlbums.GetID,m_iSelectedItem);

		}


		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			if (control==btnSortAsc)
			{
				m_bSortAscending=!m_bSortAscending;
				OnSort();
			}
			if (control==btnClear)
			{
				Transcoder.Clear();
				LoadDirectory();
			}


			if (control==btnViewAs)
			{
				switch (currentViewMethod)
				{
					case ViewAs.Album:
						currentViewMethod=ViewAs.List;
						break;
					case ViewAs.List:
						currentViewMethod=ViewAs.Album;
						break;
				}
				LoadDirectory();
			}

			if (control == btnSortBy) // sort by
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
						currentSortMethod=SortMethod.Genre;
						break;
					case SortMethod.Genre:
						currentSortMethod=SortMethod.Played;
						break;
					case SortMethod.Played:
						currentSortMethod=SortMethod.Channel;
						break;
				}
				OnSort();
			}

		}

		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{
				case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
					UpdateProperties();
					break;
			}
			return base.OnMessage(message);
		}


		void LoadDirectory()
		{
			GUIControl.ClearControl(GetID,listAlbums.GetID);
			GUIControl.ClearControl(GetID,listViews.GetID);

			ArrayList itemlist = new ArrayList();
			ArrayList recordings = Transcoder.Queue;
			foreach (Transcoder.TranscoderInfo info in recordings)
			{
				GUIListItem item=new GUIListItem();
				item.Label=info.recorded.Title;
				item.TVTag=info;
				string strLogo=Utils.GetCoverArt(Thumbs.TVChannel,info.recorded.Channel);
				if (!System.IO.File.Exists(strLogo))
				{
					strLogo="defaultVideoBig.png";
				}
				item.ThumbnailImage=strLogo;
				item.IconImageBig=strLogo;
				item.IconImage=strLogo;
				itemlist.Add(item);
			}
			foreach (GUIListItem item in itemlist)
			{
				listAlbums.Add(item);
				listViews.Add(item);
			}
      
			string strObjects=String.Format("{0} {1}", itemlist.Count, GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
			GUIControl cntlLabel = GetControl(12);
			
			if (currentViewMethod==ViewAs.Album)
				cntlLabel.YPosition = listAlbums.SpinY;
			else
				cntlLabel.YPosition = listViews.SpinY;

			OnSort();
			UpdateButtonStates();
			UpdateProperties();
	
		}

		void UpdateButtonStates()
		{
			string strLine=String.Empty;
			switch (currentSortMethod)
			{
				case SortMethod.Channel:
					strLine=GUILocalizeStrings.Get(620);//Sort by: Channel
					break;
				case SortMethod.Date:
					strLine=GUILocalizeStrings.Get(621);//Sort by: Date
					break;
				case SortMethod.Name:
					strLine=GUILocalizeStrings.Get(268);//Sort by: Title
					break;
				case SortMethod.Genre:
					strLine=GUILocalizeStrings.Get(678);//Sort by: Genre
					break;
				case SortMethod.Played:
					strLine=GUILocalizeStrings.Get(671);//Sort by: Watched
					break;
			}
			GUIControl.SetControlLabel(GetID,btnSortBy.GetID,strLine);
			switch (currentViewMethod)
			{
				case ViewAs.Album:
					strLine=GUILocalizeStrings.Get(100);
					break;
				case ViewAs.List:
					strLine=GUILocalizeStrings.Get(101);
					break;
			}
			GUIControl.SetControlLabel(GetID,btnViewAs.GetID,strLine);


			if (m_bSortAscending)
				btnSortAsc.Selected=false;
			else
				btnSortAsc.Selected=true;

			if (currentViewMethod==ViewAs.List)
			{
				GUIControl.HideControl(GetID,(int)Controls.LABEL_PROGRAMTITLE);
				GUIControl.HideControl(GetID,(int)Controls.LABEL_PROGRAMDESCRIPTION);
				GUIControl.HideControl(GetID,(int)Controls.LABEL_PROGRAMGENRE);
				GUIControl.HideControl(GetID,(int)Controls.LABEL_PROGRAMTIME);
				listAlbums.IsVisible=false;
				listViews.IsVisible=true;
			}
			else
			{
				GUIControl.ShowControl(GetID,(int)Controls.LABEL_PROGRAMTITLE);
				GUIControl.ShowControl(GetID,(int)Controls.LABEL_PROGRAMDESCRIPTION);
				GUIControl.ShowControl(GetID,(int)Controls.LABEL_PROGRAMGENRE);
				GUIControl.ShowControl(GetID,(int)Controls.LABEL_PROGRAMTIME);
				listAlbums.IsVisible=true;
				listViews.IsVisible=false;
			}
		}

		void SetLabels()
		{
			SortMethod method=currentSortMethod;
			bool bAscending=m_bSortAscending;

			for (int i=0; i < listAlbums.Count;++i)
			{
				GUIListItem item1=listAlbums[i];
				GUIListItem item2=listViews[i];
				if (item1.Label=="..") continue;
				Transcoder.TranscoderInfo info=(Transcoder.TranscoderInfo )item1.TVTag;
				TVRecorded rec=info.recorded;
				item1.Label=rec.Title;
				switch (info.status)
				{
					case Transcoder.Status.Waiting:
						item1.Label2=item2.Label2=GUILocalizeStrings.Get(681);
						break;
					case Transcoder.Status.Busy:
						item1.Label2=item2.Label2=String.Format("{0}%", info.percentDone);
						break;
					case Transcoder.Status.Error:
						item1.Label2=item2.Label2=GUILocalizeStrings.Get(257);
						break;
					case Transcoder.Status.Completed:
						item1.Label2=item2.Label2=GUILocalizeStrings.Get(997);
						break;
				}
				
			}
		}

		
		void UpdateProperties()
		{
			TVRecorded rec;
			GUIListItem pItem=GetItem( GetSelectedItemNo() );
			if (pItem==null)
			{
				rec = new TVRecorded();
				rec.SetProperties();
				return;
			}
			rec=(pItem.TVTag as Transcoder.TranscoderInfo).recorded;
			if (rec==null)
			{
				rec = new TVRecorded();
				rec.SetProperties();
				return;
			}
			rec.SetProperties();
		}

		#endregion

		#region album/list view management
		GUIListItem GetSelectedItem()
		{
			int iControl;
			iControl=listAlbums.GetID;
			if (currentViewMethod==ViewAs.List)
				iControl=listViews.GetID;
			GUIListItem item = GUIControl.GetSelectedListItem(GetID,iControl);
			return item;
		}

		GUIListItem GetItem(int iItem)
		{
			if (currentViewMethod==ViewAs.List) 
			{
				if (iItem<0 || iItem>=listViews.Count) return null;
				return listViews[iItem];
			}
			else 
			{
				if (iItem<0 || iItem>=listAlbums.Count) return null;
				return listAlbums[iItem];
			}
		}

		int GetSelectedItemNo()
		{
			int iControl;
			iControl=listAlbums.GetID;
			if (currentViewMethod==ViewAs.List)
				iControl=listViews.GetID;

			GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
			OnMessage(msg);         
			int iItem=(int)msg.Param1;
			return iItem;
		}
		int GetItemCount()
		{
			if (currentViewMethod==ViewAs.List)
				return listViews.Count;
			else 
				return listAlbums.Count;
		}
		#endregion

		#region Sort Members
		void OnSort()
		{
			SetLabels();
			listAlbums.Sort(this);
			listViews.Sort(this);
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
			TVRecorded rec1=(TVRecorded)item1.TVTag;
			TVRecorded rec2=(TVRecorded)item2.TVTag;
			switch (currentSortMethod)
			{
				case SortMethod.Played:
					item1.Label2=String.Format("{0} {1}",rec1.Played, GUILocalizeStrings.Get(677));//times
					item2.Label2=String.Format("{0} {1}",rec2.Played, GUILocalizeStrings.Get(677));//times
					if (rec1.Played==rec2.Played) goto case SortMethod.Name;
					else
					{
						if (m_bSortAscending) return rec1.Played-rec2.Played;
						else return rec2.Played-rec1.Played;
					}

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

				case SortMethod.Genre:
					item1.Label2=rec1.Genre;
					item2.Label2=rec2.Genre;
					if (rec1.Genre!=rec2.Genre) 
					{
						if (m_bSortAscending)
							return String.Compare(rec1.Genre,rec2.Genre,true);
						else
							return String.Compare(rec2.Genre,rec1.Genre,true);
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

		public override void Process()
		{
			TimeSpan ts = DateTime.Now-updateTimer;
			if (ts.TotalSeconds>1)
			{
				m_iSelectedItem=GetSelectedItemNo();
				LoadDirectory();
				updateTimer=DateTime.Now;
			}
			base.Process ();
		}

	}
}
