using System;
using System.Collections;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Radio.Database;

namespace MediaPortal.GUI.Alarm
{
	/// <summary>
	/// Alarm Details Window for the myalarm plugin
	/// </summary>
	public class GUIAlarmDetails : GUIWindow
	{
		public const int WINDOW_ALARM_DETAILS = 5001;

		#region Private Variables
		private Alarm _CurrentAlarm;
		private string _AlarmSoundsFolder = string.Empty;
		private string _PlayListFolder;
		private int ID;
		#endregion

		#region Constructor
		public GUIAlarmDetails()
		{
			GetID=GUIAlarmDetails.WINDOW_ALARM_DETAILS;
		}
		#endregion
				
		#region Private Enumerations
		private enum Controls
		{
			EnabledButton = 2,
			SoundList = 3,
			PlayType = 4,
			RenameButton = 5,
			AlarmHour = 7,
			AlarmMinute = 8,
			NameLabel = 9,
			DeleteButton = 18,
			VolumeFadeButton = 20,
			SoundListLabel =21,
			WakeUpButton = 22,
			NoMediaFoundLabel = 23,
			AlarmTypeButton = 24,
			DaysEnabledLabel = 25,
			DateLabel = 26,
			DateDay = 27,
			DateMonth = 28,
			DateYear = 29
		}

		private enum DayOfWeekControls
		{
			Monday = 10,
			Tuesday = 11,
			Wednesday =12,
			Thursday =13,
			Friday = 14,
			Saturday = 15,
			Sunday = 16
		}
		#endregion
		
		#region Overrides
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\myalarmdetails.xml");
		}

		public override void OnAction(Action action)
		{
			if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG ||action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
			{
				//save settings here
				SaveSettings();
				GUIWindowManager.PreviousWindow();
				return;
			}
			base.OnAction(action);
		}
		public override bool OnMessage(GUIMessage message)
		{
			switch ( message.Message )
			{

				case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
				{
					base.OnMessage(message);
					GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(850) );
					GetAlarmId();
					LoadListControl(_CurrentAlarm.AlarmMediaType);
					return true;
				}
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
				{
					//get sender control
					int iControl=message.SenderControlId;
						
					if (iControl == (int)Controls.RenameButton)
					{
						string strName= string.Empty;
						GetStringFromKeyboard(ref strName);
						if (strName.Length != 0)
						{
							GUIControl.SetControlLabel(GetID,(int)Controls.NameLabel,strName);
							GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(850) + @"/" + strName);
							GUIControl.FocusControl(GetID, iControl);
						}
						return true;
					}
					if(iControl ==(int)Controls.VolumeFadeButton)
					{
							
						return true;
					}
					if (iControl==(int)Controls.SoundList)
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);
						int iItem=(int)msg.Param1;
						int iAction=(int)message.Param1;
						if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
						{
							OnClick(iItem);
						}
						GUIControl.FocusControl(GetID, iControl);
						return true;
					}
					if(iControl ==(int)Controls.DeleteButton)
					{
						DeleteAlarm();
						Alarm.RefreshAlarms();
						GUIWindowManager.PreviousWindow();
						return true;
					}
					if(iControl == (int)Controls.PlayType)
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
						OnMessage(msg);
						int nSelected = (int)msg.Param1;

						switch(nSelected)
						{
							case (int)Alarm.MediaType.PlayList:
								LoadListControl(Alarm.MediaType.PlayList);
								break;
							case (int)Alarm.MediaType.Radio:
								LoadListControl(Alarm.MediaType.Radio);
								break;
							case (int)Alarm.MediaType.File:
								LoadListControl(Alarm.MediaType.File);
								break;

						}
						return true;
					}
					if(iControl == (int)Controls.AlarmTypeButton)
					{
						//change alarm type.
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0, iControl, 0, 0, null);
						OnMessage(msg);
						int nSelected = (int)msg.Param1;

						switch(nSelected)
						{
							case (int)Alarm.AlarmType.Once:
								//show date controls.
								SetTypeControls(Alarm.AlarmType.Once);
								break;
							case (int)Alarm.AlarmType.Recurring:
								SetTypeControls(Alarm.AlarmType.Recurring);
								break;
						}

					}
				}
					break;
					
			}
			return base.OnMessage(message);
		}
		#endregion

		#region Base Dialog Members
		public void RenderDlg(long timePassed)
		{
			// render this dialog box
			base.Render(timePassed);
		}
	
		#endregion

		#region Private Methods
		/// <summary>
		/// Loads the list control with the type of sound selected
		/// </summary>
		private void LoadListControl(Alarm.MediaType mediaType)
		{
				
			//clear the list
			GUIControl.ClearControl(GetID,(int)Controls.SoundList);

			VirtualDirectory Directory;
			ArrayList itemlist;

			switch(mediaType)
			{
				case Alarm.MediaType.Radio:
					//set the label
					GUIControl.SetControlLabel(GetID,(int)Controls.SoundListLabel,GUILocalizeStrings.Get(862));
					//load radios
					_CurrentAlarm.AlarmMediaType = Alarm.MediaType.Radio;
					ArrayList stations = new ArrayList();
					RadioDatabase.GetStations(ref stations);
					foreach (RadioStation station in stations)
					{
						GUIListItem pItem = new GUIListItem(station.Name);
						if(pItem.Label == _CurrentAlarm.Sound)
						{
							pItem.IconImage = "check-box.png";
							_CurrentAlarm.SelectedItem = pItem;	
						}

						GUIControl.AddListItemControl(GetID,(int)Controls.SoundList,pItem);
					}
					GUIControl.HideControl(GetID,(int)Controls.NoMediaFoundLabel);

					if(stations.Count == 0)
					{
						GUIControl.ShowControl(GetID,(int)Controls.NoMediaFoundLabel);
					}
					break;
				case Alarm.MediaType.File:
					GUIControl.SetControlLabel(GetID,(int)Controls.SoundListLabel,GUILocalizeStrings.Get(863));
					//load alarm sounds directory
					_CurrentAlarm.AlarmMediaType = Alarm.MediaType.File;
					Directory = new VirtualDirectory();
					Directory.SetExtensions(Util.Utils.AudioExtensions);
					itemlist = Directory.GetDirectory(_AlarmSoundsFolder);
						
					foreach (GUIListItem item in itemlist)
					{
						if(!item.IsFolder)
						{
							GUIListItem pItem = new GUIListItem(item.FileInfo.Name);
							if(pItem.Label == _CurrentAlarm.Sound)
							{
								pItem.IconImage = "check-box.png";
								_CurrentAlarm.SelectedItem = pItem;
							}

							GUIControl.AddListItemControl(GetID,(int)Controls.SoundList,pItem);
						}
					}
					GUIControl.HideControl(GetID,(int)Controls.NoMediaFoundLabel);

					if(itemlist.Count ==0)
					{
						GUIControl.ShowControl(GetID,(int)Controls.NoMediaFoundLabel);
					}
					break;
				case Alarm.MediaType.PlayList:
					GUIControl.SetControlLabel(GetID,(int)Controls.SoundListLabel,GUILocalizeStrings.Get(851));
					//load playlist directory
					_CurrentAlarm.AlarmMediaType = Alarm.MediaType.PlayList;
					Directory = new VirtualDirectory();
					Directory.AddExtension(".m3u");
					itemlist = Directory.GetDirectory(_PlayListFolder);
						
					foreach (GUIListItem item in itemlist)
					{
						if(!item.IsFolder)
						{
							GUIListItem pItem = new GUIListItem(item.FileInfo.Name);
							if(pItem.Label == _CurrentAlarm.Sound)
							{
								pItem.IconImage = "check-box.png";
								_CurrentAlarm.SelectedItem = pItem;
							}

							GUIControl.AddListItemControl(GetID,(int)Controls.SoundList,pItem);
						}
						GUIControl.HideControl(GetID,(int)Controls.NoMediaFoundLabel);
					}
					if(itemlist.Count ==0)
					{
						GUIControl.ShowControl(GetID,(int)Controls.NoMediaFoundLabel);
					}
						break;	
				}
			
			
			//set object count label & selected item
			string strObjects = String.Format("{0} {1}",GUIControl.GetItemCount(GetID,(int)Controls.SoundList).ToString(), GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
			if(_CurrentAlarm.SelectedItem != null)
			GUIPropertyManager.SetProperty("#selecteditem",_CurrentAlarm.SelectedItem.Label);
				
		}
		

			/// <summary>
			/// Gets the input from the virtual keyboard window
			/// </summary>
			/// <param name="strLine"></param>
			private void GetStringFromKeyboard(ref string strLine)
			{
				VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
				if (null == keyboard) return;
				keyboard.Reset();
				keyboard.Text = strLine;
				keyboard.DoModal(GetID);
				if (keyboard.IsConfirmed)
				{
					strLine = keyboard.Text;
				}
			}
			
			private void SetTypeControls(Alarm.AlarmType alarmType)
			{
				switch(alarmType)
				{
					case Alarm.AlarmType.Once:
						//show date controls.
						GUIControl.ShowControl(GetID,(int)Controls.DateLabel);
						GUIControl.ShowControl(GetID,(int)Controls.DateDay);
						GUIControl.ShowControl(GetID,(int)Controls.DateMonth);
						GUIControl.ShowControl(GetID,(int)Controls.DateYear);
						//hide days controls
						GUIControl.HideControl(GetID,(int)DayOfWeekControls.Monday);
						GUIControl.HideControl(GetID,(int)DayOfWeekControls.Tuesday);
						GUIControl.HideControl(GetID,(int)DayOfWeekControls.Wednesday);
						GUIControl.HideControl(GetID,(int)DayOfWeekControls.Thursday);
						GUIControl.HideControl(GetID,(int)DayOfWeekControls.Friday);
						GUIControl.HideControl(GetID,(int)DayOfWeekControls.Saturday);
						GUIControl.HideControl(GetID,(int)DayOfWeekControls.Sunday);
						GUIControl.HideControl(GetID,(int)Controls.DaysEnabledLabel);
						break;
					case Alarm.AlarmType.Recurring:
						//show day controls, hide date controls.
						GUIControl.HideControl(GetID,(int)Controls.DateLabel);
						GUIControl.HideControl(GetID,(int)Controls.DateDay);
						GUIControl.HideControl(GetID,(int)Controls.DateMonth);
						GUIControl.HideControl(GetID,(int)Controls.DateYear);

						GUIControl.ShowControl(GetID,(int)DayOfWeekControls.Monday);
						GUIControl.ShowControl(GetID,(int)DayOfWeekControls.Tuesday);
						GUIControl.ShowControl(GetID,(int)DayOfWeekControls.Wednesday);
						GUIControl.ShowControl(GetID,(int)DayOfWeekControls.Thursday);
						GUIControl.ShowControl(GetID,(int)DayOfWeekControls.Friday);
						GUIControl.ShowControl(GetID,(int)DayOfWeekControls.Saturday);
						GUIControl.ShowControl(GetID,(int)DayOfWeekControls.Sunday);
						GUIControl.ShowControl(GetID,(int)Controls.DaysEnabledLabel);
						break;

				}
			}
			/// <summary>
			/// Loads my alarm settings from the profile xml.
			/// </summary>
			private void LoadSettings()
			{	
					if(this.ID == -1)
					{
						GUIControl.DisableControl(GetID,(int)Controls.DeleteButton);

						//create new alarm here
						_CurrentAlarm = new Alarm(Alarm.GetNextId());
					}
					else
					{
						//load existing alarm
						_CurrentAlarm = (Alarm)Alarm.LoadedAlarms[this.ID];
					}

					_AlarmSoundsFolder = Alarm.AlarmSoundPath;
					_PlayListFolder = Alarm.PlayListPath;

					((GUISpinControl)GetControl((int)Controls.AlarmHour)).SetRange(0,23);
					((GUISpinControl)GetControl((int)Controls.AlarmMinute)).SetRange(0,59);
					((GUISpinControl)GetControl((int)Controls.DateDay)).SetRange(0,31);
					((GUISpinControl)GetControl((int)Controls.DateMonth )).SetRange(0,12);
					((GUISpinControl)GetControl((int)Controls.DateYear)).SetRange(2005,2099);

					GUIControl.EnableControl(GetID,(int)Controls.DeleteButton);
					GUIControl.SetControlLabel(GetID,(int)Controls.NameLabel,_CurrentAlarm.Name);
					GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(850) + @"/" + _CurrentAlarm.Name);
					
					((GUISelectButtonControl)GetControl((int)Controls.AlarmTypeButton)).SelectedItem = (int)_CurrentAlarm.AlarmOccurrenceType;
					((GUISelectButtonControl)GetControl((int)Controls.PlayType)).SelectedItem = (int)_CurrentAlarm.AlarmMediaType;
					((GUISpinControl)GetControl((int)Controls.AlarmHour)).Value = _CurrentAlarm.Time.Hour;
					((GUISpinControl)GetControl((int)Controls.AlarmMinute)).Value = _CurrentAlarm.Time.Minute;
					((GUICheckMarkControl)GetControl((int)DayOfWeekControls.Monday)).Selected = _CurrentAlarm.Mon;
					((GUICheckMarkControl)GetControl((int)DayOfWeekControls.Tuesday)).Selected = _CurrentAlarm.Tue;
					((GUICheckMarkControl)GetControl((int)DayOfWeekControls.Wednesday)).Selected = _CurrentAlarm.Wed;
					((GUICheckMarkControl)GetControl((int)DayOfWeekControls.Thursday)).Selected = _CurrentAlarm.Thu;
					((GUICheckMarkControl)GetControl((int)DayOfWeekControls.Friday)).Selected = _CurrentAlarm.Fri;
					((GUICheckMarkControl)GetControl((int)DayOfWeekControls.Saturday)).Selected = _CurrentAlarm.Sat;
					((GUICheckMarkControl)GetControl((int)DayOfWeekControls.Sunday)).Selected = _CurrentAlarm.Sun;
					((GUICheckMarkControl)GetControl((int)Controls.VolumeFadeButton)).Selected = _CurrentAlarm.VolumeFade;
					((GUICheckMarkControl)GetControl((int)Controls.WakeUpButton)).Selected = _CurrentAlarm.WakeUpPC;
					((GUIToggleButtonControl)GetControl((int)Controls.EnabledButton)).Selected = _CurrentAlarm.Enabled;

					((GUISpinControl)GetControl((int)Controls.DateDay)).Value = _CurrentAlarm.Time.Day;
					((GUISpinControl)GetControl((int)Controls.DateMonth)).Value = _CurrentAlarm.Time.Month;
					((GUISpinControl)GetControl((int)Controls.DateYear)).Value = _CurrentAlarm.Time.Year;

					SetTypeControls((Alarm.AlarmType)_CurrentAlarm.AlarmOccurrenceType);
			}

			/// <summary>
			/// Saves a new alarm to the profile xml
			/// </summary>
			private void SaveSettings()
			{
				GUISpinControl ctlAlarmHour = (GUISpinControl)GetControl((int)Controls.AlarmHour);
				GUISpinControl ctlAlarmMinute = (GUISpinControl)GetControl((int)Controls.AlarmMinute);
				GUISpinControl ctlAlarmDay = (GUISpinControl)GetControl((int)Controls.DateDay);
				GUISpinControl ctlAlarmMonth = (GUISpinControl)GetControl((int)Controls.DateMonth);
				GUISpinControl ctlAlarmYear = (GUISpinControl)GetControl((int)Controls.DateYear);

				DateTime AlarmDate = new DateTime(ctlAlarmYear.Value,ctlAlarmMonth.Value,ctlAlarmDay.Value,ctlAlarmHour.Value,ctlAlarmMinute.Value,0);
				_CurrentAlarm.Time = AlarmDate;
				_CurrentAlarm.Name = ((GUILabelControl)GetControl((int)Controls.NameLabel)).Label;
				_CurrentAlarm.Enabled = ((GUIToggleButtonControl)GetControl((int)Controls.EnabledButton)).Selected;
				_CurrentAlarm.AlarmMediaType = (Alarm.MediaType)((GUISelectButtonControl)GetControl((int)Controls.PlayType)).SelectedItem;
				_CurrentAlarm.Mon = ((GUICheckMarkControl)GetControl((int)DayOfWeekControls.Monday)).Selected;
				_CurrentAlarm.Tue = ((GUICheckMarkControl)GetControl((int)DayOfWeekControls.Tuesday)).Selected;
				_CurrentAlarm.Wed = ((GUICheckMarkControl)GetControl((int)DayOfWeekControls.Wednesday)).Selected;
				_CurrentAlarm.Thu = ((GUICheckMarkControl)GetControl((int)DayOfWeekControls.Thursday)).Selected;
				_CurrentAlarm.Fri = ((GUICheckMarkControl)GetControl((int)DayOfWeekControls.Friday)).Selected;
				_CurrentAlarm.Sat = ((GUICheckMarkControl)GetControl((int)DayOfWeekControls.Saturday)).Selected;
				_CurrentAlarm.Sun = ((GUICheckMarkControl)GetControl((int)DayOfWeekControls.Sunday)).Selected;
				_CurrentAlarm.VolumeFade = ((GUICheckMarkControl)GetControl((int)Controls.VolumeFadeButton)).Selected;
				_CurrentAlarm.WakeUpPC = ((GUICheckMarkControl)GetControl((int)Controls.WakeUpButton)).Selected;
				_CurrentAlarm.AlarmOccurrenceType = (Alarm.AlarmType)((GUISelectButtonControl)GetControl((int)Controls.AlarmTypeButton)).SelectedItem;

				Alarm.SaveAlarm(_CurrentAlarm);

				Alarm.RefreshAlarms();

			}

			/// <summary>
			/// Gets the current alarm id
			/// </summary>
			private void GetAlarmId()
			{
				//get the selected alarm from previous window
				GUIAlarm myAlarm = (GUIAlarm)GUIWindowManager.GetWindow((int)GUIAlarm.WINDOW_ALARM);
				this.ID = GUIAlarm.SelectedItemNo;
				
				LoadSettings();			
			}

			/// <summary>
			/// Gets the selected list item
			/// </summary>
			/// <returns></returns>
			GUIListItem GetSelectedItem()
			{
				int iControl;

				iControl=(int)Controls.SoundList;

				GUIListItem item = GUIControl.GetSelectedListItem(GetID,iControl);
				return item;
			}


			void OnClick(int iItem)
			{
				GUIListItem item = GetSelectedItem();
				if (item==null) return;

				if(_CurrentAlarm.SelectedItem != null)
					_CurrentAlarm.SelectedItem.IconImage = string.Empty;
			
				_CurrentAlarm.Sound = item.Label;
				_CurrentAlarm.SelectedItem = item;
				item.IconImage = "check-box.png";

				GUIControl.RefreshControl(GetID,(int)Controls.SoundList);				
			}

			/// <summary>
			/// Deletes an alarm from the config file
			/// </summary>
			private void DeleteAlarm()
			{
				Alarm.DeleteAlarm(_CurrentAlarm.Id);
			}
		#endregion

	}
}
