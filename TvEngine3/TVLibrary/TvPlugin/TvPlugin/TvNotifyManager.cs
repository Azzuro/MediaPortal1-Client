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
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Collections.Generic;

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Configuration;

using Gentle.Common;
using Gentle.Framework;

using TvDatabase;
using TvControl;
using TvLibrary.Interfaces;

namespace TvPlugin
{
  public class TvNotifyManager
  {
    private System.Windows.Forms.Timer _timer;
    // flag indicating that notifies have been added/changed/removed
    private static bool _notifiesListChanged;
    private static bool _enableTVNotification;
    private static bool _enableRecNotification;
    //private static bool _enableNotifyOnRecFailed;
    private static bool _busy;
    private int _preNotifyConfig;
    //list of all notifies (alert me n minutes before program starts)
    private IList _notifiesList;
    private IList _notifiedRecordings;
    //private IList _notifiedRecordingsStarted;
    // private IList _notifiedRecordingsFailed;

    private Dictionary<int, Recording> _actualRecordings;
    private Dictionary<int, Schedule> _notifiedRecordingsStarted;

    private User _dummyuser;

    public TvNotifyManager()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _enableTVNotification = xmlreader.GetValueAsBool("mytv", "enableTvNotifier", true);
        _enableRecNotification = xmlreader.GetValueAsBool("mytv", "enableRecNotifier", true);
        _preNotifyConfig = xmlreader.GetValueAsInt("mytv", "notifyTVBefore", 300);
        //_enableNotifyOnRecFailed = xmlreader.GetValueAsBool("mytv", "enableTvOnRecFailed", true);
      }      

      _busy = false;
      _timer = new System.Windows.Forms.Timer();

      // check every 15 seconds for notifies
      _dummyuser = new User();
      _dummyuser.IsAdmin = false;
      _dummyuser.Name = "Free channel checker";
      _timer.Interval = 15000;
      _timer.Enabled = true;
      _timer.Tick += new EventHandler(_timer_Tick);


    }

    public static void OnNotifiesChanged()
    {
      Log.Info("TvNotify:OnNotifiesChanged");
      _notifiesListChanged = true;
    }

    private Recording CheckForStoppedRecordings()
    {
      Recording stoppedRec = null;
      IList recordings = TvDatabase.Recording.ListAllActive();
      bool found = false;
      foreach (KeyValuePair<int, Recording> pair in _actualRecordings)
      {
        Recording actRec = (Recording)pair.Value;

        foreach (Recording rec in recordings)
        {
          if (rec.IdRecording == actRec.IdRecording)
          {
            found = true;
            break;
          }
        }
        if (found)
        {
          break;
        }
        else
        {
          stoppedRec = actRec;
        }
      }

      if (stoppedRec != null)
      {
        UpdateActiveRecordings();
      }
      return stoppedRec;
    }

    private Recording AddActiveRecordings()
    {
      Recording newRecAdded = null;
      IList recordings = TvDatabase.Recording.ListAllActive();
      if (_actualRecordings == null)
      {
        _actualRecordings = new Dictionary<int, Recording>();
      }
      foreach (Recording rec in recordings)
      {
        if (!_actualRecordings.ContainsKey(rec.IdRecording))
        {
          if (TvRecorded.IsRecordingActual(rec))
          {
            _actualRecordings.Add(rec.IdRecording, rec);
            newRecAdded = rec;
          }
        }
      }
      return newRecAdded;
    }

    private void UpdateActiveRecordings()
    {
      _actualRecordings = new Dictionary<int, Recording>();

      IList recordings = TvDatabase.Recording.ListAllActive();
      foreach (Recording rec in recordings)
      {
        if (TvRecorded.IsRecordingActual(rec))
        {
          _actualRecordings.Add(rec.IdRecording, rec);
        }
      }
    }

    void LoadNotifies()
    {
      try
      {
        Log.Info("TvNotify:LoadNotifies");
        SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Program));
        sb.AddConstraint(Operator.Equals, "notify", 1);
        SqlStatement stmt = sb.GetStatement(true);
        _notifiesList = ObjectFactory.GetCollection(typeof(Program), stmt.Execute());
        _notifiedRecordings = new ArrayList();
        _notifiedRecordingsStarted = new Dictionary<int, Schedule>();

        if (_notifiesList != null)
        {
          Log.Info("TvNotify: {0} notifies", _notifiesList.Count);
        }

        UpdateActiveRecordings();
      }
      catch (Exception e)
      {
        Log.Error("TvNotify:LoadNotifies exception : {0}", e.Message);
      }
    }

    private void Notify(string heading, string mainMsg, string channelName)
    {
      GUIDialogNotify pDlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
      if (pDlgNotify != null)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_STOP_TV, 0, 0, 0, 0, 0, null);
        string logo = Utils.GetCoverArt(Thumbs.TVChannel, channelName);
        GUIGraphicsContext.SendMessage(msg); //Send the message so the miniguide 

        pDlgNotify.Reset();
        pDlgNotify.ClearAll();
        pDlgNotify.SetImage(logo);
        pDlgNotify.SetHeading(heading);
        if (mainMsg.Length > 0)
        {
          pDlgNotify.SetText(mainMsg);
        }
        pDlgNotify.TimeOut = 10;
        pDlgNotify.DoModal(GUIWindowManager.ActiveWindow);
      }
    }

    void _timer_Tick(object sender, EventArgs e)
    {

      if (!_enableTVNotification && !_enableRecNotification) { return; };
      if (_busy) { return; };
      _busy = true;

      if (_actualRecordings == null)
      {
        AddActiveRecordings();
      }

      if (!TVHome.Connected) { return; };
      try
      {
        DateTime preNotifySecs = DateTime.Now.AddSeconds(_preNotifyConfig);
        if (_enableTVNotification)
        {
          if (_notifiesListChanged)
          {
            LoadNotifies();
            _notifiesListChanged = false;
          }
          if (_notifiesList != null && _notifiesList.Count > 0)
          {
            foreach (Program program in _notifiesList)
            {
              if (preNotifySecs > program.StartTime)
              {
                Log.Info("Notify {0} on {1} start {2}", program.Title, program.ReferencedChannel().DisplayName, program.StartTime);
                program.Notify = false;
                program.Persist();
                MediaPortal.TV.Database.TVProgram tvProg = new MediaPortal.TV.Database.TVProgram();
                tvProg.Channel = program.ReferencedChannel().DisplayName;
                tvProg.Title = program.Title;
                tvProg.Description = program.Description;
                tvProg.Genre = program.Genre;
                tvProg.Start = Utils.datetolong(program.StartTime);
                tvProg.End = Utils.datetolong(program.EndTime);

                _notifiesList.Remove(program);
                Log.Info("send notify");
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY_TV_PROGRAM, 0, 0, 0, 0, 0, null);
                msg.Object = tvProg;
                GUIGraphicsContext.SendMessage(msg);
                msg = null;
                Log.Info("send notify done");
                return;
              }
            }
          }
        }
        //Log.Debug("TVPlugIn: Notifier checking for recording to start at {0}", preNotifySecs);
        //if (g_Player.IsTV && TVHome.Card.IsTimeShifting && g_Player.Playing)
        IList schedulesList = null;
        if (_enableRecNotification && g_Player.Playing)
        {
          if (TVHome.TvServer.IsTimeToRecord(preNotifySecs))
          {
            try
            {
              schedulesList = Schedule.ListAll();
              foreach (Schedule rec in schedulesList)
              {
                //Check if alerady notified user
                foreach (Schedule notifiedRec in _notifiedRecordings)
                {
                  if (rec == notifiedRec)
                  {
                    return;
                  }
                }
                //Check if timing it's time 
                Log.Debug("TVPlugIn: Notifier checking program {0}", rec.ProgramName);
                if (TVHome.TvServer.IsTimeToRecord(preNotifySecs, rec.IdSchedule))
                {
                  //check if freecard is available. 
                  //Log.Debug("TVPlugIn: Notify verified program {0} about to start recording. {1} / {2}", rec.ProgramName, rec.StartTime, preNotifySecs);
                  if (TVHome.Navigator.Channel.IdChannel != rec.IdChannel && (int)TVHome.TvServer.GetChannelState(rec.IdChannel, _dummyuser) == 0) //not tunnable
                  {
                    Log.Debug("TVPlugIn: No free card available for {0}. Notifying user.", rec.ProgramName);
                    Notify(GUILocalizeStrings.Get(1004), String.Format("{0}. {1}", rec.ProgramName, GUILocalizeStrings.Get(200055)), TVHome.Navigator.CurrentChannel);
                    return;
                  }
                }
              }
            }
            catch (Exception ex)
            {
              Log.Debug("Tv NotifyManager: Exception at recording notification {0}", ex.ToString());
            }
          }

          //check if rec. has started
          if (!_enableRecNotification) return;

          Recording newRecording = AddActiveRecordings();

          if (newRecording != null)
          {                        
            IList prgs = TvDatabase.Program.RetrieveByTitleAndTimesInterval(newRecording.Title, newRecording.StartTime.AddHours(-4), newRecording.EndTime);

            Program prg = null;
            if (prgs != null && prgs.Count > 0)
            {
              prg = (Program)prgs[0];
            }
            string text = "";
            if (prg != null)
            {
              text = String.Format("{0} {1}-{2}",
                prg.Title,
                //prg.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                DateTime.Now.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                prg.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            }
            else
            {
              text = String.Format("{0} {1}-{2}",
                newRecording.Title,
                newRecording.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                DateTime.Now.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
                //text = GUILocalizeStrings.Get(736);//no tvguide data available
            }

            Notify(GUILocalizeStrings.Get(1446), text, newRecording.ReferencedChannel().DisplayName);
            return;
          }         
          //check if rec. has ended.                
          //AddActiveRecordings();
          Recording stoppedRec = CheckForStoppedRecordings();
          if (stoppedRec == null) return;
          if (!TvRecorded.IsRecordingActual(stoppedRec))
          {
            string text = "";
            IList prgs = TvDatabase.Program.RetrieveByTitleAndTimesInterval(stoppedRec.Title, stoppedRec.StartTime, stoppedRec.EndTime);

            Program prg = null;
            if (prgs != null && prgs.Count > 0)
            {
              prg = (Program)prgs[0];
            }
            if (prg != null)
            {
              text = String.Format("{0} {1}-{2}",
                prg.Title,
                prg.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                prg.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
            }
            else
            {
              text = String.Format("{0} {1}-{2}",
                stoppedRec.Title,
                stoppedRec.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                DateTime.Now.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
              //text = GUILocalizeStrings.Get(736);//no tvguide data available
            }
            //Recording stopped:                    
            Notify(GUILocalizeStrings.Get(1447), text, stoppedRec.ReferencedChannel().DisplayName);
            return; //we do not want to show any more notifications.
          }
          
          // todo
          // check if rec. has failed.
          //if (_enableNotifyOnRecFailed)
          //{
          //}

        }
      }
      finally
      {
        _busy = false;
      }
    }
  }
}
