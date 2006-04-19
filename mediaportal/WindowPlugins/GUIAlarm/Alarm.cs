/* 
 *	Copyright (C) 2005 Team MediaPortal
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
  public class Alarm : IDisposable
  {
    #region Private Variables
    private static AlarmCollection _Alarms;
    private System.Windows.Forms.Timer _AlarmTimer = new System.Windows.Forms.Timer();
    private System.Windows.Forms.Timer _VolumeFadeTimer = new System.Windows.Forms.Timer();
    private System.Windows.Forms.Timer _DisallowShutdownTimer;
    private static System.Windows.Forms.Timer _StopAlarmTimer = new System.Windows.Forms.Timer();
    private int _Id;
    private bool _Enabled;
    private string _Name;
    private DateTime _Time;
    private DateTime _SnoozeAlarmTime;
    private static DateTime _StopAlarmTime = new DateTime(1901, 1, 1); //ensure that its before the current time
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
    private bool _Wakeup;
    private AlarmType _AlarmType;
    private string _Message;
    private int _RepeatCount;
    private PlayListPlayer playlistPlayer;
    private bool _disallowShutdown;
    private bool _switchedWindow;
    private static bool _initializedStopAlarmTimer;
    private System.ComponentModel.BackgroundWorker _backgroundWorker;

    //constants
    private const int _MaxAlarms = 20;
    #endregion

    #region Public Enumerations
    public enum AlarmType
    {
      Once = 0,
      Recurring = 1
    }
    public enum MediaType
    {
      PlayList = 0,
      Radio = 1,
      File = 2,
      Message = 3
    }
    #endregion

    #region Constructor
    public Alarm(int id, string name, int mediaType, bool enabled, DateTime time, bool mon, bool tue, bool wed, bool thu, bool fri, bool sat, bool sun, string sound, bool volumeFade, bool wakeup, int alarmType, string message)
      :this()
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
      _Wakeup = wakeup;
      _AlarmType = (AlarmType)alarmType;
      _Message = message;
      _SnoozeAlarmTime = new DateTime(1901,1,1); //ensure that its before the actual alarm trigger time
      _disallowShutdown = false;
      _switchedWindow = false;

      InitializeTimer();
      
    }

    private Alarm()
    {
      playlistPlayer = PlayListPlayer.SingletonPlayer;
    }

    public Alarm(int id):this()
    {
      _Id = id;
      _Name = GUILocalizeStrings.Get(869) + _Id.ToString();
      _Time = DateTime.Now;
    }
    #endregion

    #region Private Properties
    public AlarmType AlarmOccurrenceType
    {
      get { return _AlarmType; }
      set { _AlarmType = value; }
    }

    public bool Wakeup
    {
      get { return _Wakeup; }
      set { _Wakeup = value; }
    }

    public string Name
    {
      get { return _Name; }
      set { _Name = value; }
    }
    /// <summary>
    /// Returns a string to display the days the alarm is enabled
    /// </summary>
    public string DaysEnabled
    {
      get
      {
        StringBuilder sb = new StringBuilder("-------");

        if (_Sun)
          sb.Replace("-", "S", 0, 1);
        if (_Mon)
          sb.Replace("-", "M", 1, 1);
        if (_Tue)
          sb.Replace("-", "T", 2, 1);
        if (_Wed)
          sb.Replace("-", "W", 3, 1);
        if (_Thu)
          sb.Replace("-", "T", 4, 1);
        if (_Fri)
          sb.Replace("-", "F", 5, 1);
        if (_Sat)
          sb.Replace("-", "S", 6, 1);

        return sb.ToString();
      }

    }
    public MediaType AlarmMediaType
    {
      get { return _MediaType; }
      set { _MediaType = value; }
    }
    public bool Enabled
    {
      get { return _Enabled; }
      set
      {
        _Enabled = value;
        _AlarmTimer.Enabled = value;
      }
    }
    public DateTime Time
    {
      get { return _Time; }
      set { _Time = value; }
    }
    public DateTime NextAlarmTriggerTime
      {
          get
          {
              if (DateTime.Compare(_Time, _SnoozeAlarmTime) < 0)
              {
                  return _SnoozeAlarmTime;
              }
              else
              {
                  return _Time;
              }
          }
      }
    public string Sound
    {
      get { return _Sound; }
      set { _Sound = value; }
    }
    public int Id
    {
      get { return _Id; }
    }
    public bool Mon
    {
      get { return _Mon; }
      set { _Mon = value; }
    }
    public bool Tue
    {
      get { return _Tue; }
      set { _Tue = value; }
    }
    public bool Wed
    {
      get { return _Wed; }
      set { _Wed = value; }
    }
    public bool Thu
    {
      get { return _Thu; }
      set { _Thu = value; }
    }
    public bool Fri
    {
      get { return _Fri; }
      set { _Fri = value; }
    }
    public bool Sat
    {
      get { return _Sat; }
      set { _Sat = value; }
    }
    public bool Sun
    {
      get { return _Sun; }
      set { _Sun = value; }
    }
    public bool VolumeFade
    {
      get { return _VolumeFade; }
      set { _VolumeFade = value; }
    }
    public GUIListItem SelectedItem
    {
      get { return _SelectedItem; }
      set { _SelectedItem = value; }
    }
    public string Message
    {
      get { return _Message; }
      set { _Message = value; }
    }
    public bool DisallowShutdown
      {
          get
          {
              return _disallowShutdown;
          }

          set
          {
              _disallowShutdown = value;
              
          }
      }
    public bool AlarmTriggerDialogOpen
      {
          get
          {
              if (_backgroundWorker == null)
              {
                  return false;
              }
              else
              {
                  return _backgroundWorker.IsBusy;
              }
          }
      }

    #endregion

    #region Private Methods

    /// <summary>
          /// Initializes the timer objects
          /// </summary>
    private void InitializeTimer()
    {
      _AlarmTimer.Tick += new EventHandler(OnTimer);
      _AlarmTimer.Interval = 1000; //second	
      _VolumeFadeTimer.Tick += new EventHandler(OnTimer);
      _VolumeFadeTimer.Interval = 3000; //3 seconds

      if (_Enabled)
        _AlarmTimer.Enabled = true;
    }

    /// <summary>
    /// Executes on the interval of the timer objects.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTimer(Object sender, EventArgs e)
    {
      if (sender == _AlarmTimer)
      {
          if (DateTime.Now.Hour == NextAlarmTriggerTime.Hour && DateTime.Now.Minute == NextAlarmTriggerTime.Minute)
        {
          if (_AlarmType == AlarmType.Recurring && IsDayEnabled() || _AlarmType == AlarmType.Once)
          {
            Log.Write("Alarm: {0} fired at {1}", _Name, DateTime.Now);

            //reset the disallowshutdown flag after delay for alarm to load
             StartDisallowShutdownTimer();

            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              Play();
              //enable fade timer if selected
              if (_VolumeFade)
              {
                g_Player.Volume = 0;
                _VolumeFadeTimer.Enabled = true;
              }
		
            }

            //disable the timer.
            _AlarmTimer.Enabled = false;

            //set StopAlarmTime if media is playing (radio only feature due to MP limitations)
            if (this.AlarmMediaType == MediaType.Radio)
            {
                Alarm.StopAlarmTime = DateTime.Now.AddMinutes(Alarm.AlarmTimeout);
            }

            //temporary workaround for PowerScheduler issue
            if (MediaPortal.GUI.Library.GUIWindowManager.ActiveWindow == (int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_HOME || MediaPortal.GUI.Library.GUIWindowManager.ActiveWindow == (int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_SECOND_HOME)
            {
                MediaPortal.GUI.Library.GUIWindowManager.ActivateWindow(5000);
                _switchedWindow = true;
            }
              
            //load alarm dialog in different thread - allows PowerScheduler to continue working properly (as it requires the UI thread, and dialogs steal the UI thread when modal)
            _backgroundWorker = new System.ComponentModel.BackgroundWorker();
            _backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(ShowAlarmTriggeredDialog_DoWork);
            _backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(ShowAlarmTriggeredDialog_RunWorkerCompleted);
            
            //build dialog title
            string dialogTitle;
            if (this.Message == string.Empty)
            {
                dialogTitle = GUILocalizeStrings.Get(850) + " - " + this.Name;
            }
            else
            {
                dialogTitle = this.Message;
            }
            
            //run thread
            _backgroundWorker.RunWorkerAsync(dialogTitle);
          }

        }
      }
      else if (sender == _VolumeFadeTimer)
      {
        if (g_Player.Volume < 99)
        {
          g_Player.Volume += 1;

        }
        else
        {
          _VolumeFadeTimer.Enabled = false;
        }
      }
    }

    /// <summary>
      /// Method that shows the alarm triggered dialog - to dismiss or snooze alarm
      /// </summary>
    public void ShowAlarmTriggeredDialog_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
          {
              GUIDialogMenu dlgAlarmOpts = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
              dlgAlarmOpts.Reset();

              dlgAlarmOpts.SetHeading((string)e.Argument);

              dlgAlarmOpts.Add(GUILocalizeStrings.Get(857) + " 5 " + GUILocalizeStrings.Get(3004));
              dlgAlarmOpts.Add(GUILocalizeStrings.Get(857) + " 10 " + GUILocalizeStrings.Get(3004));
              dlgAlarmOpts.Add(GUILocalizeStrings.Get(857) + " 15 " + GUILocalizeStrings.Get(3004));
              dlgAlarmOpts.Add(GUILocalizeStrings.Get(857) + " 30 " + GUILocalizeStrings.Get(3004));
              dlgAlarmOpts.Add(GUILocalizeStrings.Get(857) + " 1 " + GUILocalizeStrings.Get(3001));
              dlgAlarmOpts.Add(GUILocalizeStrings.Get(858));

              dlgAlarmOpts.DoModal(GUI.Library.GUIWindowManager.ActiveWindow);
    
              //process dialog result
              switch (dlgAlarmOpts.SelectedLabel)
              {
                  case 0:
                      e.Result = 5;
                      break;

                  case 1:
                      e.Result = 10;
                      break;

                  case 2:
                      e.Result = 15;
                      break;

                  case 3:
                      e.Result = 30;
                      break;

                  case 4:
                      e.Result = 60;
                      break;

                  default:
                      e.Result = 0;
                      break;
              }
          }

    /// <summary>
      /// Event handler for when the AlarmTriggeredDialog thread is finished
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
    private void ShowAlarmTriggeredDialog_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
      {
          SetSnoozePeriod((int)e.Result);
          _backgroundWorker.Dispose();
      }

    /// <summary>
      /// Sets the snooze period and reenables the alarm timer
      /// </summary>
      /// <param name="minutes"></param>
    private void SetSnoozePeriod(int minutes)
      {
          if (minutes == 0)
          {
              //temporary workaround for PowerScheduler issue
              //returns to home screen if the alarm screen was activated during trigger of alarm
              if (_switchedWindow == true)
              {
                  MediaPortal.GUI.Library.GUIWindowManager.ActivateWindow(0, true);
                  _switchedWindow = false;
              }
          }
          else
          {
              if (_MediaType != MediaType.Message) { g_Player.Stop(); }
              _SnoozeAlarmTime = DateTime.Now.AddMinutes(minutes);
              _AlarmTimer.Enabled = true;
              _AlarmTimer.Start();

              //temporary workaround for PowerScheduler issue
              //returns to home screen if the alarm screen was activated during trigger of alarm
              if ((_switchedWindow == true) && (this.Wakeup == true))
              {
                  MediaPortal.GUI.Library.GUIWindowManager.ActivateWindow(0, true);
                  _switchedWindow = false;
              }
          }
      }    

    /// <summary>
    /// Checks if the current dayofweek for the alarm is enabled
    /// </summary>
    /// <returns>true if current dayofweek is enabled</returns>
    private bool IsDayEnabled()
    {
      switch (DateTime.Now.DayOfWeek)
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
      switch (day)
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
      switch (_MediaType)
      {
        case MediaType.PlayList:
          if (PlayListFactory.IsPlayList(_Sound))
          {
            string soundName = Alarm.PlayListPath + "\\" + _Sound;
            IPlayListIO loader = PlayListFactory.CreateIO(soundName);
            PlayList playlist = new PlayList();

            if (!loader.Load(playlist, soundName))
            {
              ShowErrorDialog();
              return;
            }
            if (playlist.Count == 1)
            {
              g_Player.Play(playlist[0].FileName);
              g_Player.Volume = 99;
              return;
            }
            for (int i = 0; i < playlist.Count; ++i)
            {
              PlayListItem playListItem = playlist[i];
              playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Add(playListItem);
            }
            if (playlistPlayer.GetPlaylist(PlayListType.PLAYLIST_MUSIC).Count > 0)
            {
              playlistPlayer.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
              playlistPlayer.Reset();

              playlistPlayer.Play(0);
              g_Player.Volume = 99;

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
            if (station.Name == _Sound)
            {
              if (station.URL.Length < 5)
              {
                // FM radio
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_TUNE_RADIO, (int)GUIWindow.Window.WINDOW_RADIO, 0, 0, 0, 0, null);
                msg.Label = station.Name;
                GUIGraphicsContext.SendMessage(msg);
              }
              else
              {
                // internet radio stream
                g_Player.PlayAudioStream(station.URL);
              }
              break;
            }
          }
          break;
        case MediaType.File:
          if (Alarm.AlarmSoundPath.Length != 0 && _Sound.Length != 0)
          {
            try
            {
              _RepeatCount = 0;
              g_Player.Play(Alarm.AlarmSoundPath + "\\" + _Sound);
              g_Player.Volume = 99;

              //add playback end handler if file <= repeat seconds in configuration
              if (g_Player.Duration <= Alarm.RepeatSeconds)
                g_Player.PlayBackEnded += new MediaPortal.Player.g_Player.EndedHandler(g_Player_PlayBackEnded);

            }
            catch (System.Runtime.InteropServices.COMException)
            {
              ShowErrorDialog();
            }
          }
          else
          {
            ShowErrorDialog();
          }


          break;
        case MediaType.Message:
          //do not play any media, message only
          break;
      }

    }

    /// <summary>
    /// Shows the Error Dialog
    /// </summary>
    private void ShowErrorDialog()
    {
      GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
      if (dlgOK != null)
      {
        dlgOK.SetHeading(6);
        dlgOK.SetLine(1, 477);
        dlgOK.SetLine(2, "");
        dlgOK.DoModal(GUIAlarm.WindowAlarm);
      }
      return;
    }

    /// <summary>
    /// Handles the playback ended event to loop the sound file if necessary
    /// </summary>
    /// <param name="type"></param>
    /// <param name="filename"></param>
    private void g_Player_PlayBackEnded(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
      //play file again, increment loop counter
      if (_RepeatCount <= RepeatCount)
      {
        g_Player.Play(Alarm.AlarmSoundPath + "\\" + _Sound);
        _RepeatCount += 1;
      }

    }
    #endregion

    #region IDisposable Members
    public void Dispose()
    {
      _AlarmTimer.Enabled = false;
      _AlarmTimer.Dispose();
      _VolumeFadeTimer.Dispose();
    }
    #endregion

    #region Static Methods

    /// <summary>
    /// Loads all of the alarms from the profile xml
    /// </summary>
    /// <returns>ArrayList of Alarm Objects</returns>
    public static void LoadAll()
    {
      AlarmCollection Alarms = new AlarmCollection();

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        for (int i = 0; i < _MaxAlarms; i++)
        {
          string NameTag = String.Format("alarmName{0}", i);
          string MediaTypeTag = String.Format("alarmMediaType{0}", i);
          string TimeTag = String.Format("alarmTime{0}", i);
          string EnabledTag = String.Format("alarmEnabled{0}", i);
          string MonTag = String.Format("alarmMon{0}", i);
          string TueTag = String.Format("alarmTue{0}", i);
          string WedTag = String.Format("alarmWed{0}", i);
          string ThuTag = String.Format("alarmThu{0}", i);
          string FriTag = String.Format("alarmFri{0}", i);
          string SatTag = String.Format("alarmSat{0}", i);
          string SunTag = String.Format("alarmSun{0}", i);
          string SoundTag = String.Format("alarmSound{0}", i);
          string VolumeFadeTag = String.Format("alarmVolumeFade{0}", i);
          string WakeUpPCTag = String.Format("alarmWakeUpPC{0}", i);
          string AlarmTypeTag = String.Format("alarmType{0}", i);
          string MessageTag = String.Format("alarmMessage{0}", i);

          string AlarmName = xmlreader.GetValueAsString("alarm", NameTag, "");

          if (AlarmName.Length > 0)
          {
            bool AlarmEnabled = xmlreader.GetValueAsBool("alarm", EnabledTag, false);
            int AlarmMediaType = xmlreader.GetValueAsInt("alarm", MediaTypeTag, 1);
            DateTime AlarmTime = DateTime.Parse(xmlreader.GetValueAsString("alarm", TimeTag, string.Empty));
            bool AlarmMon = xmlreader.GetValueAsBool("alarm", MonTag, false);
            bool AlarmTue = xmlreader.GetValueAsBool("alarm", TueTag, false);
            bool AlarmWed = xmlreader.GetValueAsBool("alarm", WedTag, false);
            bool AlarmThu = xmlreader.GetValueAsBool("alarm", ThuTag, false);
            bool AlarmFri = xmlreader.GetValueAsBool("alarm", FriTag, false);
            bool AlarmSat = xmlreader.GetValueAsBool("alarm", SatTag, false);
            bool AlarmSun = xmlreader.GetValueAsBool("alarm", SunTag, false);
            string AlarmSound = xmlreader.GetValueAsString("alarm", SoundTag, string.Empty);
            bool AlarmVolumeFade = xmlreader.GetValueAsBool("alarm", VolumeFadeTag, false);
            bool WakeUpPC = xmlreader.GetValueAsBool("alarm", WakeUpPCTag, false);
            int AlarmType = xmlreader.GetValueAsInt("alarm", AlarmTypeTag, 1);
            string Message = xmlreader.GetValueAsString("alarm", MessageTag, string.Empty);


            Alarm objAlarm = new Alarm(i, AlarmName, AlarmMediaType, AlarmEnabled, AlarmTime,
              AlarmMon, AlarmTue, AlarmWed, AlarmThu,
              AlarmFri, AlarmSat, AlarmSun, AlarmSound, AlarmVolumeFade, WakeUpPC, AlarmType, Message);

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

      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {

        xmlwriter.SetValue("alarm", "alarmName" + id, alarmToSave.Name);
        xmlwriter.SetValue("alarm", "alarmMediaType" + id, (int)alarmToSave.AlarmMediaType);
        xmlwriter.SetValueAsBool("alarm", "alarmEnabled" + id, alarmToSave.Enabled);
        xmlwriter.SetValue("alarm", "alarmTime" + id, alarmToSave.Time);
        xmlwriter.SetValueAsBool("alarm", "alarmMon" + id, alarmToSave.Mon);
        xmlwriter.SetValueAsBool("alarm", "alarmTue" + id, alarmToSave.Tue);
        xmlwriter.SetValueAsBool("alarm", "alarmWed" + id, alarmToSave.Wed);
        xmlwriter.SetValueAsBool("alarm", "alarmThu" + id, alarmToSave.Thu);
        xmlwriter.SetValueAsBool("alarm", "alarmFri" + id, alarmToSave.Fri);
        xmlwriter.SetValueAsBool("alarm", "alarmSat" + id, alarmToSave.Sat);
        xmlwriter.SetValueAsBool("alarm", "alarmSun" + id, alarmToSave.Sun);
        xmlwriter.SetValue("alarm", "alarmSound" + id, alarmToSave.Sound);
        xmlwriter.SetValueAsBool("alarm", "alarmVolumeFade" + id, alarmToSave.VolumeFade);
        xmlwriter.SetValueAsBool("alarm", "alarmWakeUpPC" + id, alarmToSave.Wakeup);
        xmlwriter.SetValue("alarm", "alarmType" + id, (int)alarmToSave.AlarmOccurrenceType);
        xmlwriter.SetValue("alarm", "alarmMessage" + id, alarmToSave.Message);
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
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        xmlwriter.RemoveEntry("alarm", "alarmName" + id);
        xmlwriter.RemoveEntry("alarm", "alarmEnabled" + id);
        xmlwriter.RemoveEntry("alarm", "alarmTime" + id);
        xmlwriter.RemoveEntry("alarm", "alarmMon" + id);
        xmlwriter.RemoveEntry("alarm", "alarmTue" + id);
        xmlwriter.RemoveEntry("alarm", "alarmWed" + id);
        xmlwriter.RemoveEntry("alarm", "alarmThu" + id);
        xmlwriter.RemoveEntry("alarm", "alarmFri" + id);
        xmlwriter.RemoveEntry("alarm", "alarmSat" + id);
        xmlwriter.RemoveEntry("alarm", "alarmSun" + id);
        xmlwriter.RemoveEntry("alarm", "alarmSound" + id);
        xmlwriter.RemoveEntry("alarm", "alarmMediaType" + id);
        xmlwriter.RemoveEntry("alarm", "alarmVolumeFade" + id);
        xmlwriter.RemoveEntry("alarm", "alarmWakeUpPC" + id);
        xmlwriter.RemoveEntry("alarm", "alarmType" + id);
        xmlwriter.RemoveEntry("alarm", "alarmMessage" + id);
      }
      return true;
    }

    /// <summary>
    /// Gets the next black Id for a new alarm
    /// </summary>
    /// <returns>Integer Id</returns>
    public static int GetNextId
    {
      get
      {
        string tempText;
        for (int i = 0; i < _MaxAlarms; i++)
        {
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
          {
            tempText = xmlreader.GetValueAsString("alarm", "alarmName" + i, "");
            if (tempText.Length == 0)
            {
              return i;
            }
          }
        }
        return -1;
      }
    }

    /// <summary>
    /// Gets the icon based on the current alarm media type
    /// </summary>
    public string GetIcon
    {
      get
      {
        switch (_MediaType)
        {
          case MediaType.File:
            return "defaultAudio.png";
          case MediaType.PlayList:
            return "DefaultPlaylist.png";
          case MediaType.Radio:
            {
              string thumb = Utils.GetCoverArt(Thumbs.Radio, this.Sound);
              if (thumb.Length != 0) return thumb;
              return "DefaultMyradio.png";
            }
          case MediaType.Message:
            {
              return "dialog_information.png";
            }
        }
        return string.Empty;
      }
    }

    /// <summary>
    /// Refreshes the loaded alarms from the config file
    /// </summary>
    public static void RefreshAlarms()
    {
      if (_Alarms != null)
      {
        foreach (Alarm a in _Alarms)
        {
          a.Dispose();
        }
        _Alarms.Clear();

        //Load all the alarms 
        Alarm.LoadAll();
      }
    }

    /// <summary>
    /// Checks to see if the stopalarmtimer has been initialized; if not, initialize it
    /// </summary>
    private static void InitializeStopAlarmTimer()
    {
        if (_initializedStopAlarmTimer == false)
        {
            _StopAlarmTimer.Interval = 10000; //10 seconds
            _StopAlarmTimer.Tick += new EventHandler(OnStopAlarmTimer);

            g_Player.PlayBackEnded += new g_Player.EndedHandler(Static_g_Player_PlaybackEnded);

            _initializedStopAlarmTimer = true;
        }
    }

    /// <summary>
    /// Reacts to the StopAlarmTimer - if the time has passed, stop the alarm.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void OnStopAlarmTimer(Object sender, EventArgs e)
    {
        if (DateTime.Compare(_StopAlarmTime, DateTime.Now) <= 0)
        {
            bool alarmTriggerDialogOpen = false;

            foreach (Alarm a in _Alarms)
            {
                if (a.AlarmTriggerDialogOpen == true)
                {
                    alarmTriggerDialogOpen = true;
                }
            }

            //if the dialog is open, dismiss it
            if (alarmTriggerDialogOpen == true)
            {
                MediaPortal.GUI.Library.Action act = new Action();
                act.wID = MediaPortal.GUI.Library.Action.ActionType.REMOTE_6; //number for the dismiss option

                MediaPortal.GUI.Library.GUIGraphicsContext.OnAction(act);
            }

            g_Player.Stop();
            _StopAlarmTimer.Enabled = false;
        }
    }

    /// <summary>
    /// Disable the StopAlarmTimer if the alarm has been stopped.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="stoptime"></param>
    /// <param name="filename"></param>
    private static void Static_g_Player_PlaybackEnded(MediaPortal.Player.g_Player.MediaType type, string filename)
    {
        _StopAlarmTime = new DateTime(1901, 1, 1);
    }

    #endregion

    #region Static Properties
    /// <summary>
    /// Gets / Sets the loaded alarms
    /// </summary>
    public static AlarmCollection LoadedAlarms
    {
      get { return _Alarms; }
    }
    /// <summary>
    /// Gets the alarms sound path from the configuration file
    /// </summary>
    public static string AlarmSoundPath
    {
      get
      {
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          return Utils.RemoveTrailingSlash(xmlreader.GetValueAsString("alarm", "alarmSoundsFolder", ""));
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
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          return Utils.RemoveTrailingSlash(xmlreader.GetValueAsString("music", "playlists", ""));
        }
      }
    }

    /// <summary>
    /// Gets the configured timeout period in minutes
    /// </summary>
    public static int AlarmTimeout
    {
      get
      {
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          return xmlreader.GetValueAsInt("alarm", "alarmTimeout", 60);
        }
      }
    }

    /// <summary>
    /// Gets the configured duration to qualify to repeat the playing file
    /// </summary>
    public static int RepeatSeconds
    {
      get
      {
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          return xmlreader.GetValueAsInt("alarm", "alarmRepeatSeconds", 120);
        }
      }
    }

    /// <summary>
    /// Gets the configured count to repeat the file
    /// </summary>
    public static int RepeatCount
    {
      get
      {
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          return xmlreader.GetValueAsInt("alarm", "alarmRepeatCount", 5);
        }
      }
    }
    
    /// <summary>
    /// Checks all alarms for disallowShutdown flag
    /// </summary>
    public static bool DisallowShutdownCheckAllAlarms
    {
        get
        {
            bool disallowShutdown = false;
            foreach (Alarm a in _Alarms)
            {
                if (a.DisallowShutdown == true)
                {
                    disallowShutdown = true;
                }
            }

            return disallowShutdown;
        }
    }
    
    /// <summary>
    /// Gets/sets the stopalarmtime
    /// </summary>
    public static DateTime StopAlarmTime
    {
        get
        {
            return _StopAlarmTime;
        }
        set
        {
            //only take the later datetime value
            if (DateTime.Compare(value, _StopAlarmTime) > 0)
            {
                _StopAlarmTime = value;
                InitializeStopAlarmTimer();
                _StopAlarmTimer.Enabled = true;
            }
            //if value is before current time, assume StopAlarmTimer is to be disabled
            else if (DateTime.Compare(value, DateTime.Now) < 0)
            {
                _StopAlarmTime = value;
                _StopAlarmTimer.Enabled = false;
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
        if (!GUIGraphicsContext.IsFullScreenVideo || !GUIGraphicsContext.IsPlaying)
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
    /// <param name="earliestStartTime">Interface parameter for PowerScheduler - defines when the next wake up can start</param>
    /// <returns>DateTime</returns>
    public static DateTime GetNextAlarmDateTime(DateTime earliestStartTime)
    {
      if (_Alarms == null) return new DateTime(2100, 1, 1, 1, 0, 0, 0);

      DateTime NextStartTime = new DateTime();
      DateTime tmpNextStartTime = new DateTime();

      //make the starting off nextdatetime a year away so earlier (real alarm trigger) times can be compared
      NextStartTime = DateTime.Now.AddYears(1);

      foreach (Alarm a in _Alarms)
      {
        //alarm must be enabled and set to wake up the pc.
        if (a.Enabled && a.Wakeup)
        {
            switch (a.AlarmOccurrenceType)
            {
                case AlarmType.Once:
                    tmpNextStartTime = a.NextAlarmTriggerTime;
                    break;

                case AlarmType.Recurring:
                    //resolve recurring alarm to next trigger date
                    //loop through the next 7 days to 
                    //find the next enabled day for the alarm
                    for (int i = 0; i < 8; i++)
                    {
                        DateTime DateToCheck = DateTime.Now.AddDays(i);

                        //check to see if day is enabled for this alarm
                        if (a.IsDayEnabled(DateToCheck.DayOfWeek))
                        {
                            //found next enabled day - build new date from the new date found, combined with the alarm trigger time
                            tmpNextStartTime = new DateTime(DateToCheck.Year, DateToCheck.Month, DateToCheck.Day, a.NextAlarmTriggerTime.Hour, a.NextAlarmTriggerTime.Minute, a.NextAlarmTriggerTime.Second);

                            //check to see if the alarm for this day has passed or not
                            if (DateTime.Compare(tmpNextStartTime, DateTime.Now) > 0)
                            {
                                //trigger time has not passed yet, therefore this is the next trigger time for this alarm
                                break;
                            }
                        }
                    }
                    break;
            }
            
          if (DateTime.Compare(tmpNextStartTime, earliestStartTime) >= 0)
          {
              if (DateTime.Compare(tmpNextStartTime, NextStartTime) < 0)
              {
                  NextStartTime = new DateTime(tmpNextStartTime.Ticks);
              }

              //reset disallowshutdown flag
              a.DisallowShutdown = false;
          }
          else if (DateTime.Compare(tmpNextStartTime,DateTime.Now)>=0)
          {
           //next alarm is before the earliestStartTime, so disallowshutdown
              a.DisallowShutdown = true;
          }
        }

      }

      //MediaPortal.GUI.Library.Log.Write("Alarm: next alarm trigger time: {0}", NextStartTime.ToString());
      
        return NextStartTime;
    }

      /// <summary>
      /// Provides delay before resetting the disallowshutdown flag.
      /// Allows the alarm to fire properly and for the alarm to load - e.g. music, radio etc.
      /// </summary>
      private void StartDisallowShutdownTimer()
      {
          _DisallowShutdownTimer = new System.Windows.Forms.Timer();

          _DisallowShutdownTimer.Tick += new EventHandler(OnDisallowShutdownTimer);
          _DisallowShutdownTimer.Interval = 60000; //60 seconds
          _DisallowShutdownTimer.Enabled = true;

      }

      /// <summary>
      /// Responds to DisallowShutdownTimer - resets the disallowshutdown flag after alarm has fired and the delay
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnDisallowShutdownTimer(Object sender, EventArgs e)
      {
          _disallowShutdown = false;
          _DisallowShutdownTimer.Enabled = false;

      }


    #endregion


  }
}
