/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;

using TvLibrary.Log;

using TvDatabase;
using TvControl;

namespace TvService
{
  /// <summary>
  /// class which holds all details about a schedule which is current being recorded
  /// </summary>
  public class RecordingDetail
  {
    #region variables
    Schedule _schedule;
    Channel _channel;
    string _fileName;
    DateTime _endTime;
    TvDatabase.Program _program;
    CardDetail _cardInfo;
    DateTime _dateTimeRecordingStarted;
    #endregion

    #region ctor
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="schedule">Schedule of this recording</param>
    /// <param name="channel">Channel on which the recording is done</param>
    /// <param name="endTime">Date/Time the recording should start without pre-record interval</param>
    /// <param name="endTime">Date/Time the recording should stop with post record interval</param>
    public RecordingDetail(Schedule schedule, Channel channel, DateTime startTime, DateTime endTime)
    {
      _schedule = schedule;
      _channel = channel;
      _endTime = endTime;

      //find which program we are recording
      _program = schedule.ReferencedChannel().CurrentProgram;
      if (_program != null)
      {
        if (startTime >= _program.EndTime)
        {
          TvDatabase.Program next = schedule.ReferencedChannel().NextProgram;
          if (next != null)
          {
            //then we are not recording the current program, but the next one
            _program = next;
          }
        }
      }

      //no program? then treat this as a manual recording
      if (_program == null)
      {
        _program = new TvDatabase.Program(0, DateTime.Now, endTime, "manual", "", "", false);
      }
    }
    #endregion

    #region properties
    /// <summary>
    /// get/sets the CardInfo for this recording
    /// </summary>
    public CardDetail CardInfo
    {
      get
      {
        return _cardInfo;
      }
      set
      {
        _cardInfo = value;
      }
    }

    /// <summary>
    /// Gets or sets the recording start date time.
    /// </summary>
    /// <value>The recording start date time.</value>
    public DateTime RecordingStartDateTime
    {
      get
      {
        return _dateTimeRecordingStarted;
      }
      set
      {
        _dateTimeRecordingStarted = value;
      }
    }

    /// <summary>
    /// gets the Schedule belonging to this recording
    /// </summary>
    public Schedule Schedule
    {
      get
      {
        return _schedule;
      }
    }
    /// <summary>
    /// gets the Channel which is being recorded
    /// </summary>
    public Channel Channel
    {
      get
      {
        return _channel;
      }
    }
    /// <summary>
    /// Gets the filename of the recording
    /// </summary>
    public string FileName
    {
      get
      {
        return _fileName;
      }
      set
      {
        _fileName = value;
      }
    }
    /// <summary>
    /// Gets the date/time on which the recording should stop
    /// </summary>
    public DateTime EndTime
    {
      get
      {
        return _endTime;
      }
    }
    /// <summary>
    /// Gets the Program which is being recorded
    /// </summary>
    public TvDatabase.Program Program
    {
      get
      {
        return _program;
      }
    }

    /// <summary>
    /// Property which returns true when recording is busy
    /// and false when recording should be stopped
    /// </summary>
    public bool IsRecording
    {
      get
      {
        if (DateTime.Now >= EndTime.AddMinutes(_schedule.PostRecordInterval)) return false;
        return true;
      }
    }
    #endregion

    #region private members
    /// <summary>
    /// Create the filename for the recording 
    /// </summary>
    /// <param name="recordingPath"></param>
    public void MakeFileName(string recordingPath)
    {
      Setting setting;
      TvBusinessLayer layer = new TvBusinessLayer();
      if ((ScheduleRecordingType)_schedule.ScheduleType == ScheduleRecordingType.Once)
      {
        setting = layer.GetSetting("moviesformat", "%title%");
      }
      else
      {
        setting = layer.GetSetting("seriesformat", "%title%");
      }
      string strInput = "title%";
      if (setting != null)
      {
        if (setting.Value != null)
        {
          strInput = setting.Value;
        }
      }
      string recFileFormat = string.Empty;
      string recDirFormat = string.Empty;
      string subDirectory = string.Empty;
      string fullPath = recordingPath;
      string fileName = string.Empty;
      string recEngineExt = ".mpg";

      strInput = Utils.ReplaceTag(strInput, "%channel%", Utils.MakeFileName(_schedule.ReferencedChannel().Name), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%title%", Utils.MakeFileName(Program.Title), "unknown");
      //strInput = Utils.ReplaceTag(strInput, "%name%", Utils.MakeFileName(Program.Episode), "unknown");
      //strInput = Utils.ReplaceTag(strInput, "%series%", Utils.MakeFileName(Program.SeriesNum), "unknown");
      //strInput = Utils.ReplaceTag(strInput, "%episode%", Utils.MakeFileName(Program.EpisodeNum), "unknown");
      //strInput = Utils.ReplaceTag(strInput, "%part%", Utils.MakeFileName(Program.EpisodePart), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%date%", Utils.MakeFileName(Program.StartTime.ToString("yyyy-MM-dd")), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%start%", Utils.MakeFileName(Program.StartTime.ToShortTimeString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%end%", Utils.MakeFileName(Program.EndTime.ToShortTimeString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%genre%", Utils.MakeFileName(Program.Genre), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%startday%", Utils.MakeFileName(Program.StartTime.Day.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%startmonth%", Utils.MakeFileName(Program.StartTime.Month.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%startyear%", Utils.MakeFileName(Program.StartTime.Year.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%starthh%", Utils.MakeFileName(Program.StartTime.Hour.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%startmm%", Utils.MakeFileName(Program.StartTime.Minute.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%endday%", Utils.MakeFileName(Program.EndTime.Day.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%endmonth%", Utils.MakeFileName(Program.EndTime.Month.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%startyear%", Utils.MakeFileName(Program.EndTime.Year.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%endhh%", Utils.MakeFileName(Program.EndTime.Hour.ToString()), "unknown");
      strInput = Utils.ReplaceTag(strInput, "%endmm%", Utils.MakeFileName(Program.EndTime.Minute.ToString()), "unknown");

      int index = strInput.LastIndexOf('\\');
      if (index != -1)
      {
        subDirectory = strInput.Substring(0, index).Trim();
        fileName = strInput.Substring(index + 1).Trim();
      }
      else
        fileName = strInput.Trim();


      if (subDirectory != string.Empty)
      {
        subDirectory = Utils.RemoveTrailingSlash(subDirectory);
        subDirectory = Utils.MakeDirectoryPath(subDirectory);
        fullPath = recordingPath + "\\" + subDirectory;
        if (!System.IO.Directory.Exists(fullPath))
          System.IO.Directory.CreateDirectory(fullPath);
      }
      if (fileName == string.Empty)
      {
        DateTime dt = Program.StartTime;
        fileName = String.Format("{0}_{1}_{2}{3:00}{4:00}{5:00}{6:00}p{7}{8}",
                                  _schedule.ReferencedChannel().Name, Program.Title,
                                  dt.Year, dt.Month, dt.Day,
                                  dt.Hour,
                                  dt.Minute,
                                  DateTime.Now.Minute, DateTime.Now.Second);
      }
      fileName = Utils.MakeFileName(fileName);
      if (System.IO.File.Exists(fullPath + "\\" + fileName + recEngineExt))
      {
        int i = 1;
        while (System.IO.File.Exists(fullPath + "\\" + fileName + "_" + i.ToString() + recEngineExt))
          ++i;
        fileName += "_" + i.ToString();
      }
      _fileName = fullPath + "\\" + fileName + recEngineExt;
    }
    #endregion
  }

}
