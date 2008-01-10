#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;


using TvDatabase;
using TvLibrary.Log;

namespace TvService
{
  public class EpisodeManagement
  {
    public EpisodeManagement()
    {
    }

    public List<Recording> GetEpisodes(string title, IList recordings)
    {
      List<Recording> episodes = new List<Recording>();
      foreach (Recording recording in recordings)
      {
        if (String.Compare(title, recording.Title, true) == 0)
        {
          episodes.Add(recording);
        }
      }
      return episodes;
    }

    public Recording GetOldestEpisode(List<Recording> episodes)
    {
      Recording oldestEpisode = null;
      DateTime oldestDateTime = DateTime.MaxValue;
      foreach (Recording rec in episodes)
      {
        if (rec.StartTime < oldestDateTime)
        {
          oldestDateTime = rec.StartTime;
          oldestEpisode = rec;
        }
      }
      return oldestEpisode;
    }

    #region episode disk management
    public void OnScheduleEnded(string recordingFilename, Schedule recording, TvDatabase.Program program)
    {
      Log.Write( "diskmanagement: recording {0} ended. type:{1} max episodes:{2}",
          program.Title, (ScheduleRecordingType)recording.ScheduleType, recording.MaxAirings);

      CheckEpsiodesForRecording(recording,program);
    }

    void CheckEpsiodesForRecording(Schedule schedule, TvDatabase.Program program)
    {
      if (!schedule.DoesUseEpisodeManagement) return;

      //check how many episodes we got
      while (true)
      {
        IList recordings = Recording.ListAll();

        List<Recording> episodes = GetEpisodes(program.Title, recordings);
        if (episodes.Count <= schedule.MaxAirings) return;

        Recording oldestEpisode = GetOldestEpisode(episodes);
        if (oldestEpisode == null) return;
        Log.Write(  "diskmanagement:   Delete episode {0} {1} {2} {3}",
                             oldestEpisode.ReferencedChannel(),
                             oldestEpisode.Title,
                             oldestEpisode.StartTime.ToLongDateString(),
                             oldestEpisode.StartTime.ToLongTimeString());

				//-- !!! This will cause a multiple start of Scheduler class and then causing multiple recordings !!!
        //-- Delete the file from disk and the recording entry from the database.
        //-- TVController controller = new TVController();
        //-- controller.DeleteRecording(oldestEpisode.IdRecording);
        //-- controller = null;
				if (System.IO.File.Exists(oldestEpisode.FileName))
				{
					try
					{
						System.IO.File.Delete(oldestEpisode.FileName);
					}
					catch(Exception e)
					{
						Log.Error("EpisodeManagement: Exception at deleting file ({0})", oldestEpisode.FileName);
						Log.Write(e);
					}
				}
      	oldestEpisode.Remove();
      }
    }
    #endregion


  }
}
