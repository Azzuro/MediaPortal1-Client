using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;

namespace MediaPortal.GUI.Alarm 
{

	/// <summary>
	/// An Alarm Plugin for Media Portal
	/// </summary>
	public class GUIAlarm : GUIWindow, IWakeable
	{
		public const int WINDOW_ALARM = 5000;

		#region Private Variables	
			
			private static int _SelectedItemNumber;
			private System.Windows.Forms.Timer _SnoozeTimer = new System.Windows.Forms.Timer();
			private int	_SnoozeCount;
			private int _SnoozeTime;
		#endregion
		
		#region Private Enumerations
		enum Controls
		{
			NewButton = 3,
			SleepTimerButton = 4,
			AlarmList = 5	
		}
		#endregion

		#region Constructor
		public GUIAlarm()
		{
			GetID=(int)GUIAlarm.WINDOW_ALARM;
		}
		#endregion
		
		#region Overrides
		
		public override bool Init()
		{
			LoadSettings();
			return Load (GUIGraphicsContext.Skin+@"\myalarm.xml");
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
				case Action.ActionType.ACTION_KEY_PRESSED:
				{
					//space bar
					if(action.m_key.KeyChar == 32)
					{	
						//enable snooze timer
						_SnoozeCount = 0;
						_SnoozeTimer.Enabled = true;
					}
					break;
				}
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
          GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(850));
					AddAlarmsToList();
					return true;
				}
				case GUIMessage.MessageType.GUI_MSG_CLICKED:
				{
					//get sender control
					base.OnMessage(message);
					int iControl=message.SenderControlId;
							
					if (iControl==(int)Controls.AlarmList)
					{
						GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED,GetID,0,iControl,0,0,null);
						OnMessage(msg);
						int iItem=(int)msg.Param1;
						int iAction=(int)message.Param1;
						if (iAction == (int)Action.ActionType.ACTION_SELECT_ITEM)
						{
							OnClick(iItem);
						}
					}
					//new alarm
					if(iControl ==(int)Controls.NewButton)
					{
						//activate the new alarm window
						_SelectedItemNumber = -1;
						GUIWindowManager.ActivateWindow(GUIAlarmDetails.WINDOW_ALARM_DETAILS);
						return true;
					}
					if(iControl ==(int)Controls.SleepTimerButton)
					{
						GUIWindowManager.ActivateWindow(GUISleepTimer.WINDOW_SLEEP_TIMER);
						return true;
					}
						
					
						
					return true;
				}
				case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
				{
					
				}
					break;

			}
			return base.OnMessage(message);

		} 
		#endregion

		#region Private Methods
		/// <summary>
		/// Initializes the snooze timer object
		/// </summary>
		private void InitializeSnoozeTimer()
		{
			_SnoozeTimer.Tick += new EventHandler(OnTimer);
			_SnoozeTimer.Interval = 60000; //minute	
		}
		/// <summary>
		/// Executes on the interval of the timer object.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTimer(Object sender, EventArgs e)
		{
			Console.WriteLine(_SnoozeCount);
			if(_SnoozeCount == _SnoozeTime)
			{
				_SnoozeTimer.Enabled= false;
				if (g_Player.Paused) g_Player.Pause();
			}
			else
			{
				_SnoozeCount++;
			}
		}
		/// <summary>
		/// Loads my alarm settings from the profile xml.
		/// </summary>
		private void LoadSettings()
		{ 
			InitializeSnoozeTimer();
			_SnoozeTime = Alarm.SnoozeTime;
			//Load all the alarms 
			Alarm.LoadAll();

			//DateTime earliestStarttime = DateTime.Now.AddMinutes(3);	
			//Alarm.GetNextAlarmDateTime(earliestStarttime);
		}

		private void AddAlarmsToList()
		{
			foreach (Alarm objAlarm in Alarm.LoadedAlarms)
			{
					
				GUIListItem item = new GUIListItem();
				item.Label = objAlarm.Name;
				
				//set proper label
				if(objAlarm.AlarmOccurrenceType == Alarm.AlarmType.Recurring)
				{
					item.Label2 = objAlarm.DaysEnabled + "   " + objAlarm.Time.ToString("HH:mm");
				}
				else
				{
					item.Label2 = objAlarm.Time.ToShortDateString() +  "   " + objAlarm.Time.ToString("HH:mm");
				}
			
				item.IsFolder=false;
				item.MusicTag = true;

				//shade inactive alarms
				if (!objAlarm.Enabled)
					item.Shaded = true;

				item.IconImage = GetIcon((Alarm.MediaType)objAlarm.AlarmMediaType);
				
				GUIControl.AddListItemControl(GetID,(int)Controls.AlarmList,item);
			}
			string strObjects =String.Format("{0} {1}",GUIControl.GetItemCount(GetID,(int)Controls.AlarmList).ToString(), GUILocalizeStrings.Get(632));
			GUIPropertyManager.SetProperty("#itemcount",strObjects);
		}

		void OnClick(int iItem)
		{
			_SelectedItemNumber = iItem;

			GUIWindowManager.ActivateWindow(GUIAlarmDetails.WINDOW_ALARM_DETAILS);				
		}
		private string GetIcon(Alarm.MediaType mediaType)
		{
			switch(mediaType)
			{
				case Alarm.MediaType.File:
					return "defaultAudio.png";
				case Alarm.MediaType.PlayList:
					return "DefaultPlaylist.png";
				case Alarm.MediaType.Radio:
					return "DefaultMyradio.png";	
			}
			return string.Empty;
		}

		#endregion
	
		#region Public Properties
			/// <summary>
			/// Gets the selected Item number from the alarm list
			/// </summary>
			public static int SelectedItemNo
			{
				get{return _SelectedItemNumber;}
			}
		#endregion

		#region IWakeable Members

		public bool DisallowShutdown()
		{
			return false;// !PreShutdownCheck();
		}

		public DateTime GetNextEvent(DateTime earliestWakeuptime)
		{	
			return Alarm.GetNextAlarmDateTime(DateTime.Now);
		}
		public string PluginName()
		{
			return GUILocalizeStrings.Get(850);
		}

		#endregion
		
	}
}
