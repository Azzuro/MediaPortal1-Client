using System;
using System.Text;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Radio.Database;

namespace MediaPortal.GUI.Alarm
{
	/// <summary>
	/// Alarm Class
	/// </summary>
	public class Alarm
	{
		#region Private Variables
			private static AlarmCollection _Alarms;
			private System.Windows.Forms.Timer _AlarmTimer = new System.Windows.Forms.Timer();
			private System.Windows.Forms.Timer _VolumeFadeTimer = new  System.Windows.Forms.Timer();
			private int _Id;
			private bool _Enabled;
			private string _Name;
			private DateTime _Time;
			//private AlarmType _Type;
			private bool _Mon;
			private bool _Tue;
			private bool _Wed;
			private bool _Thu;
			private bool _Fri;
			private bool _Sat;
			private bool _Sun;
			private string _Sound;
			private MediaType _MediaType;
			private bool _VolumeFade;
			private GUIListItem _SelectedItem;
			private bool _WakeUpPC;
			private AlarmType _AlarmType;

			//constants
			private const int MAX_ALARMS = 20;
		#endregion

		#region Public Enumerations
		public enum AlarmType
		{
			Once = 0,
			Recurring = 1
		}
		public enum MediaType
		{
			PlayList =0,
			Radio = 1,
			File = 2	
		}
		#endregion

		#region Constructor
		public Alarm(int id,string name,int mediaType, bool enabled,DateTime time,bool mon,bool tue,bool wed,bool thu,bool fri,bool sat,bool sun,string sound,bool volumeFade,bool wakeUpPC,int alarmType)
		{
			_Id = id;
			_Name = name;
			_MediaType = (MediaType)mediaType;
			_Enabled = enabled;
			_Time = time;
			_Mon = mon;
			_Tue = tue;
			_Wed = wed;
			_Thu = thu;
			_Fri = fri;
			_Sat = sat;
			_Sun = sun;
			_Sound = sound;
			_VolumeFade = volumeFade;
			_WakeUpPC = wakeUpPC;
			_AlarmType = (AlarmType)alarmType;

			InitializeTimer();

		}

		#endregion

		#region Properties	
			public AlarmType AlarmOccurrenceType
			{
				get{return _AlarmType;}
				set{_AlarmType = value;}
			}
			
			public bool WakeUpPC
			{
				get{return _WakeUpPC;}
				set{_WakeUpPC = value;}
			}
			
			public Alarm(int id)
			{
				_Id = id;
				_Name = GUILocalizeStrings.Get(869) + _Id.ToString();
			}
		#endregion

		#region Properties	
			public string Name
			{
				get{return _Name;}
				set{_Name = value;}
			}
			/// <summary>
			/// Returns a string to display the days the alarm is enabled
			/// </summary>
			public string DaysEnabled
			{
				get
				{
					StringBuilder sb= new StringBuilder("-------");

					if(_Sun)
						sb.Replace("-","S",0,1);
					if(_Mon)
						sb.Replace("-","M",1,1);
					if(_Tue)
						sb.Replace("-","T",2,1);
					if(_Wed)
						sb.Replace("-","W",3,1);
					if(_Thu)
						sb.Replace("-","T",4,1);
					if(_Fri)
						sb.Replace("-","F",5,1);
					if(_Sat)
						sb.Replace("-","S",6,1);

					return sb.ToString();

				}

			}
			public MediaType AlarmMediaType
			{
				get{return _MediaType;}
				set{_MediaType = value;}
			}
			public bool Enabled
			{
				get{return _Enabled;}
				set{
					_Enabled = value;
					_AlarmTimer.Enabled = value;
					}
			}
			public DateTime Time
			{
				get{return _Time;}
				set{_Time = value;}
			}
//			public AlarmType Type
//			{
//				get{return _Type;}
//				set{_Type = value;}
//			}
			public string Sound
			{
				get{return _Sound;}
				set{_Sound = value;}
			}
			public int Id
			{
				get{return _Id;}
			}
			public bool Mon
			{
				get{return _Mon;}
				set{_Mon = value;}
			}
			public bool Tue
			{
				get{return _Tue;}
				set{_Tue = value;}
			}
			public bool Wed
			{
				get{return _Wed;}
				set{_Wed = value;}
			}
			public bool Thu
			{
				get{return _Thu;}
				set{_Thu = value;}
			}
			public bool Fri
			{
				get{return _Fri;}
				set{_Fri = value;}
			}
			public bool Sat
			{
				get{return _Sat;}
				set{_Sat = value;}
			}
			public bool Sun
			{
				get{return _Sun;}
				set{_Sun = value;}
			}
			public bool VolumeFade
			{
				get{return _VolumeFade;}
				set{_VolumeFade = value;}
			}
			public GUIListItem SelectedItem
			{
				get{return _SelectedItem;}
				set{_SelectedItem = value;}
			}
		#endregion

		#region Private Methods
		
		/// <summary>
		/// Initializes the timer object
		/// </summary>
		private void InitializeTimer()
		{
			_AlarmTimer.Tick += new EventHandler(OnTimer);
			_AlarmTimer.Interval = 1000; //second	
			_VolumeFadeTimer.Tick += new EventHandler(OnTimer);
			_VolumeFadeTimer.Interval = 3000; //3 seconds	

			if(_Enabled)
				_AlarmTimer.Enabled = true;
		}

		/// <summary>
		/// Executes on the interval of the timer object.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTimer(Object sender, EventArgs e)
		{
			if(sender == _AlarmTimer)
			{
				if(DateTime.Now.Hour == _Time.Hour && DateTime.Now.Minute == _Time.Minute && IsDayEnabled())
				{
					Log.Write("Alarm {0} fired at {1}",_Name,DateTime.Now);

					if (!GUIGraphicsContext.IsFullScreenVideo)
					{
						Play();
						//enable fade timer if selected
						if(_VolumeFade)
						{
							g_Player.Volume = 0;
							_VolumeFadeTimer.Enabled = true;
						}
							
						GUIWindowManager.ActivateWindow(GUIAlarm.WINDOW_ALARM);
					}

					//disable the timer.
					_AlarmTimer.Enabled = false;
				}
			}
			if(sender == _VolumeFadeTimer)
			{
				if(g_Player.Volume < 99)
				{
					g_Player.Volume +=1;

				}
				else
				{
					_VolumeFadeTimer.Enabled = false;
				}
			}
			
		}

		

		/// <summary>
		/// Checks if the current dayofweek for the alarm is enabled
		/// </summary>
		/// <returns>true if current dayofweek is enabled</returns>
		private bool IsDayEnabled()
		{
			switch(DateTime.Now.DayOfWeek)
			{
				case DayOfWeek.Monday:
					return _Mon;
				case DayOfWeek.Tuesday:
					return _Tue;
				case DayOfWeek.Wednesday:
					return _Wed;
				case DayOfWeek.Thursday:
					return _Thu;
				case DayOfWeek.Friday:
					return _Fri;
				case DayOfWeek.Saturday:
					return _Sat;
				case DayOfWeek.Sunday:
					return _Sun;
			}
			return false;
		}

		/// <summary>
		/// Returns if the day parameter is enabled for this alarm
		/// </summary>
		/// <param name="day">day to check</param>
		/// <returns>True if day passed in is enabled</returns>
		private bool IsDayEnabled(DayOfWeek day)
		{
			switch(day)
			{
				case DayOfWeek.Monday:
					return _Mon;
				case DayOfWeek.Tuesday:
					return _Tue;
				case DayOfWeek.Wednesday:
					return _Wed;
				case DayOfWeek.Thursday:
					return _Thu;
				case DayOfWeek.Friday:
					return _Fri;
				case DayOfWeek.Saturday:
					return _Sat;
				case DayOfWeek.Sunday:
					return _Sun;
			}
			return false;

		}

		/// <summary>
		/// Plays the selected media type
		/// </summary>
		private void Play()
		{
			switch(_MediaType)
			{
				case MediaType.PlayList:
					if(PlayListFactory.IsPlayList(_Sound))
					{
						PlayList playlist = PlayListFactory.Create(Alarm.PlayListPath + "\\" + _Sound);
						if(playlist==null) return;
						if(!playlist.Load(Alarm.PlayListPath + "\\" +  _Sound))
						{
							ShowErrorDialog();
							return;
						}
						if(playlist.Count == 1)
						{
							g_Player.Play(playlist[0].FileName);
							g_Player.Volume=99;
							return;
						}
						for(int i=0; i<playlist.Count; ++i)
						{
							PlayList.PlayListItem playListItem = playlist[i];
							PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Add(playListItem);
						}
						if(PlayListPlayer.GetPlaylist(PlayListPlayer.PlayListType.PLAYLIST_MUSIC).Count>0)
						{
							PlayListPlayer.CurrentPlaylist = PlayListPlayer.PlayListType.PLAYLIST_MUSIC;
							PlayListPlayer.Reset();
						
							PlayListPlayer.Play(0);
							g_Player.Volume=99;
						
						}
					}
					else
					{
						ShowErrorDialog();
					}
					break;
				case MediaType.Radio:
					ArrayList stations = new ArrayList();
					RadioDatabase.GetStations(ref stations);
					foreach (RadioStation station in stations)
					{
						if(station.Name == _Sound)
						{ 	
							g_Player.Play(GetPlayPath(station));
							g_Player.Volume=99;
						}
					}
					break;
				case MediaType.File:
					try
					{
						g_Player.Play(Alarm.AlarmSoundPath + "\\" +  _Sound);
						g_Player.Volume=99;
					}
					catch
					{
						ShowErrorDialog();
					}
				
					break;
			}

		}

		/// <summary>
		/// Gets the playpath for a radio station
		/// </summary>
		/// <param name="station"></param>
		/// <returns></returns>
		string GetPlayPath(RadioStation station)
		{
			if (station.URL.Length>5)
			{
				return station.URL;
			}
			else
			{
				string strFile=String.Format("{0}.radio",station.Frequency);
				return strFile;
			}
		}
		/// <summary>
		/// Shows the Error Dialog
		/// </summary>
		private void ShowErrorDialog()
		{
			GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
			if(dlgOK !=null)
			{
				dlgOK.SetHeading(6);
				dlgOK.SetLine(1,477);
				dlgOK.SetLine(2,"");
				dlgOK.DoModal(GUIAlarm.WINDOW_ALARM);
			}
			return;
		}
		#endregion

		public void Dispose()
		{
			_AlarmTimer.Enabled=false;
			_AlarmTimer.Dispose();
		} 

		#region Static Methods
			/// <summary>
			/// Loads all of the alarms from the profile xml
			/// </summary>
			/// <returns>ArrayList of Alarm Objects</returns>
			public static void LoadAll()
			{
				AlarmCollection Alarms = new AlarmCollection();

				using(AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
				{
					for (int i=0; i < MAX_ALARMS; i++)
					{
						string NameTag=String.Format("alarmName{0}",i);
						string MediaTypeTag=String.Format("alarmMediaType{0}",i);
						string TimeTag=String.Format("alarmTime{0}",i);
						string EnabledTag=String.Format("alarmEnabled{0}",i);
						string MonTag =  String.Format("alarmMon{0}",i);
						string TueTag =  String.Format("alarmTue{0}",i);
						string WedTag =  String.Format("alarmWed{0}",i);
						string ThuTag =  String.Format("alarmThu{0}",i);
						string FriTag =  String.Format("alarmFri{0}",i);
						string SatTag =  String.Format("alarmSat{0}",i);
						string SunTag =  String.Format("alarmSun{0}",i);
						string SoundTag =  String.Format("alarmSound{0}",i);
						string VolumeFadeTag = String.Format("alarmVolumeFade{0}",i);
						string WakeUpPCTag = String.Format("alarmWakeUpPC{0}",i);
						string AlarmTypeTag = String.Format("alarmType{0}",i);

						string AlarmName=xmlreader.GetValueAsString("alarm",NameTag,"");

						if (AlarmName.Length>0)
						{
							bool AlarmEnabled=xmlreader.GetValueAsBool("alarm",EnabledTag,false);
							int AlarmMediaType =xmlreader.GetValueAsInt("alarm",MediaTypeTag,1);
							DateTime AlarmTime = DateTime.Parse(xmlreader.GetValueAsString("alarm",TimeTag,string.Empty));
							bool AlarmMon = xmlreader.GetValueAsBool("alarm",MonTag,false);
							bool AlarmTue = xmlreader.GetValueAsBool("alarm",TueTag,false);
							bool AlarmWed = xmlreader.GetValueAsBool("alarm",WedTag,false);
							bool AlarmThu = xmlreader.GetValueAsBool("alarm",ThuTag,false);
							bool AlarmFri = xmlreader.GetValueAsBool("alarm",FriTag,false);
							bool AlarmSat = xmlreader.GetValueAsBool("alarm",SatTag,false);
							bool AlarmSun = xmlreader.GetValueAsBool("alarm",SunTag,false);
							string AlarmSound = xmlreader.GetValueAsString("alarm",SoundTag,string.Empty);
							bool AlarmVolumeFade = xmlreader.GetValueAsBool("alarm",VolumeFadeTag,false);
							bool WakeUpPC = xmlreader.GetValueAsBool("alarm",WakeUpPCTag,false);
							int AlarmType = xmlreader.GetValueAsInt("alarm",AlarmTypeTag,1);

								
							Alarm objAlarm = new Alarm(i,AlarmName,AlarmMediaType,AlarmEnabled,AlarmTime,
								AlarmMon,AlarmTue,AlarmWed,AlarmThu,
								AlarmFri,AlarmSat,AlarmSun,AlarmSound,AlarmVolumeFade,WakeUpPC,AlarmType);

							Alarms.Add(objAlarm);
						}
					}	
				}
				_Alarms = Alarms;

			}
			/// <summary>
			/// Saves an alarm to the configuration file
			/// </summary>
			/// <param name="alarmToSave">Alarm object to save</param>
			/// <returns></returns>
			public static bool SaveAlarm(Alarm alarmToSave)
			{
				int id = alarmToSave.Id;
				
				using(AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
				{
					
					xmlwriter.SetValue("alarm","alarmName"+id,alarmToSave.Name);
					xmlwriter.SetValue("alarm","alarmMediaType"+id,(int)alarmToSave.AlarmMediaType);
					xmlwriter.SetValueAsBool("alarm","alarmEnabled"+id,alarmToSave.Enabled);
					xmlwriter.SetValue("alarm","alarmTime"+id,alarmToSave.Time);
					xmlwriter.SetValueAsBool("alarm","alarmMon"+id,alarmToSave.Mon);   
					xmlwriter.SetValueAsBool("alarm","alarmTue"+id,alarmToSave.Tue);   
					xmlwriter.SetValueAsBool("alarm","alarmWed"+id,alarmToSave.Wed);   
					xmlwriter.SetValueAsBool("alarm","alarmThu"+id,alarmToSave.Thu);   
					xmlwriter.SetValueAsBool("alarm","alarmFri"+id,alarmToSave.Fri);   
					xmlwriter.SetValueAsBool("alarm","alarmSat"+id,alarmToSave.Sat); 
					xmlwriter.SetValueAsBool("alarm","alarmSun"+id,alarmToSave.Sun); 
					xmlwriter.SetValue("alarm","alarmSound"+id,alarmToSave.Sound);
					xmlwriter.SetValueAsBool("alarm","alarmVolumeFade"+id,alarmToSave.VolumeFade); 
					xmlwriter.SetValueAsBool("alarm","alarmWakeUpPC"+id,alarmToSave.WakeUpPC); 
					xmlwriter.SetValue("alarm","alarmType"+id,(int)alarmToSave.AlarmOccurrenceType);
				}
				return true;
		
				
			
			}

			/// <summary>
			/// Deletes an alarm from the configuration file
			/// </summary>
			/// <param name="id">Id of alarm to be deleted</param>
			/// <returns>true if suceeded</returns>
			public static bool DeleteAlarm(int id)
			{
				using(AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
				{
					xmlwriter.RemoveEntry("alarm","alarmName"+id);
					xmlwriter.RemoveEntry("alarm","alarmEnabled"+id);
					xmlwriter.RemoveEntry("alarm","alarmTime"+id);
					xmlwriter.RemoveEntry("alarm","alarmMon"+id);   
					xmlwriter.RemoveEntry("alarm","alarmTue"+id);   
					xmlwriter.RemoveEntry("alarm","alarmWed"+id);   
					xmlwriter.RemoveEntry("alarm","alarmThu"+id);   
					xmlwriter.RemoveEntry("alarm","alarmFri"+id);   
					xmlwriter.RemoveEntry("alarm","alarmSat"+id); 
					xmlwriter.RemoveEntry("alarm","alarmSun"+id); 
					xmlwriter.RemoveEntry("alarm","alarmSound"+id);
					xmlwriter.RemoveEntry("alarm","alarmMediaType"+id);
					xmlwriter.RemoveEntry("alarm","alarmVolumeFade"+id);
					xmlwriter.RemoveEntry("alarm","alarmWakeUpPC"+id);
					xmlwriter.RemoveEntry("alarm","alarmType"+id);
				}
				return true;
			} 

			/// <summary>
			/// Gets the next black Id for a new alarm
			/// </summary>
			/// <returns>Integer Id</returns>
			public static int GetNextId()
			{
				string tempText;
				for (int i=0; i < MAX_ALARMS; i++)
				{
					using(AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
					{
						tempText = xmlreader.GetValueAsString("alarm","alarmName"+i,"");
						if (tempText.Length == 0)
						{
							return i;
						}
					}	
				}
				return -1;
			}

			

			/// <summary>
			/// Refreshes the loaded alarms from the config file
			/// </summary>
			public static void RefreshAlarms()
			{

				if(_Alarms != null)
				{
					foreach(Alarm a in _Alarms)
					{
						a.Dispose();
					}
					_Alarms.Clear();
			
					//Load all the alarms 
					Alarm.LoadAll();


				}
			}
			
		#endregion

		#region Static Properties
			/// <summary>
			/// Gets / Sets the loaded alarms
			/// </summary>
			public static AlarmCollection LoadedAlarms  
			{
				get{return _Alarms;}
			}
			/// <summary>
			/// Gets the alarms sound path from the configuration file
			/// </summary>
			public static string AlarmSoundPath
			{
				get
				{ 
					using(AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
					{
						return  Utils.RemoveTrailingSlash(xmlreader.GetValueAsString("alarm","alarmSoundsFolder",""));
					}
				}
			}
			/// <summary>
			/// Gets the playlist path from the configuration file
			/// </summary>
			public static string PlayListPath
			{

				get
				{ 
					using(AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
					{
						return  Utils.RemoveTrailingSlash(xmlreader.GetValueAsString("music","playlists",""));
					}
				}
			}
			/// <summary>
			/// Gets the snooze time from the configuration file
			/// </summary>
			public static int SnoozeTime
			{
				get
				{ 
					using(AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
					{
						return xmlreader.GetValueAsInt("alarm","alarmSnoozeTime",5);
					}
				}
			}
		#endregion

		#region PowerScheduler Interface Implementation
			/// <summary>
			/// Powersheduler implimentation, returns true if the plugin can allow hibernation
			/// </summary>
			public bool CanHibernate
			{
				get
				{
					if(!GUIGraphicsContext.IsFullScreenVideo || !GUIGraphicsContext.IsPlaying)
					{
						return true;
					}
					else
					{
						return false;
					}
				}		
			}


			/// <summary>
			/// Gets the DateTime for the next active alarm to wake up the pc.
			/// </summary>
			/// <param name="alarms">ArrayList of loaded alarms</param>
			/// <returns>alarm object</returns>
			public static DateTime GetNextAlarmDateTime(DateTime earliestStartTime)
			{	
				//timespan to search.
				DateTime NextStartTime = new DateTime();//=  DateTime.Now.AddMonths(1);
				DateTime tmpNextStartTime = new DateTime();
								
				foreach(Alarm a in _Alarms)
				{
					//alarm must be enabled and set to wake up the pc.
					if(a.Enabled && a.WakeUpPC)
					{	
						switch(a.AlarmOccurrenceType)
						{
							case AlarmType.Once:
								tmpNextStartTime = a.Time;
								break;
							case AlarmType.Recurring:
								//check if alarm has passed
								if(a.Time.Ticks < DateTime.Now.Ticks)
								{
									//alarm has passed, loop through the next 7 days to 
									//find the next enabled day for the alarm
									for(int i=1; i < 8; i++)
									{
										DateTime DateToCheck = DateTime.Now.AddDays(i);

										if(a.IsDayEnabled(DateToCheck.DayOfWeek))
										{
											//found next enabled day
											tmpNextStartTime = DateToCheck;	
											break;
										}
									}
								}
								else
								{
									//alarm has not passed
									tmpNextStartTime = a.Time;
								}
								break;
						}

						if (tmpNextStartTime.Ticks > earliestStartTime.Ticks)
						{
							NextStartTime = new DateTime(tmpNextStartTime.Ticks);
						}
					}
						
				}
					
				return NextStartTime;
			}

			
		#endregion

	}
}
