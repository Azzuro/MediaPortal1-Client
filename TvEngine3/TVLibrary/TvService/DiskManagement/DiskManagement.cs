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
using System.IO;
using System.Collections.Generic;
using TvDatabase;
using TvLibrary.Log;

namespace TvService
{
  /// <summary>
  /// Summary description for DiskManagement.
  /// </summary>
  public class DiskManagement
  {
    readonly System.Timers.Timer _timer;
    public DiskManagement()
    {
      _timer = new System.Timers.Timer();
      _timer.Interval = 15 * 60 * 1000;
      _timer.Enabled = true;
      _timer.Elapsed += _timer_Elapsed;
      Log.Write("DiskManagement: started");
    }


    static List<string> GetDisks()
    {
      List<string> drives = new List<string>();

      IList<Card> cards = Card.ListAll();
      foreach (Card card in cards)
      {
        if (card.RecordingFolder.Length > 0)
        {
          string driveLetter = String.Format("{0}:", card.RecordingFolder[0]);
          if (Utils.getDriveType(driveLetter) == 3)
          {
            if (!drives.Contains(driveLetter))
            {
              drives.Add(driveLetter);
            }
          }
        }
      }
      return drives;
    }

    static void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      CheckFreeDiskSpace();
    }

    /// <summary>
    /// This method checks the diskspace on each harddisk
    /// if the diskspace used by recordings exceeds the disk quota set on the drive
    /// then this method will delete recordings until the disk quota is not exceeded anymore
    /// </summary>
    /// <remarks>Note, this method will run once every 15 minutes
    /// </remarks>
    static void CheckFreeDiskSpace()
    {
      //check diskspace every 15 minutes...
      TvBusinessLayer layer = new TvBusinessLayer();
      if (!(layer.GetSetting("diskQuotaEnabled", "False").Value == "True"))
      {
        //Disk Quota Management disabled: quitting
        return;
      }

      Log.Write("DiskManagement: checking free disk space");

      //first get all drives..
      List<string> drives = GetDisks();

      // next check diskspace on each drive.
      foreach (string drive in drives)
      {
        CheckDriveFreeDiskSpace(drive);
      }
    }

    static bool OutOfDiskSpace(string drive)
    {
      TvBusinessLayer layer = new TvBusinessLayer();

      ulong minimiumFreeDiskSpace;

      string quotaText = layer.GetSetting("freediskspace" + drive[0], "51200").Value;
      try
      {
        minimiumFreeDiskSpace = (ulong)Int32.Parse(quotaText);
      } catch (Exception e)
      {
        Log.Error("DiskManagement: Exception at parsing freediskspace ({0}) to drive {1}", quotaText, drive);
        Log.Error(e.ToString());
        //no setting for this drive: quitting
        return false;
      }

      if (minimiumFreeDiskSpace <= 51200) // 50MB
      {
        minimiumFreeDiskSpace = 51200;
      }

      // Kilobytes to Bytes
      minimiumFreeDiskSpace *= 1024;

      if (minimiumFreeDiskSpace <= 0)
        return false;

      ulong freeDiskSpace = Utils.GetFreeDiskSpace(drive);
      if (freeDiskSpace > minimiumFreeDiskSpace)
        return false;

      Log.Info("DiskManagement: Drive {0} is out of free space!", drive);
      Log.Info("DiskManagement: Has: {0} Minimum Set: {1}", freeDiskSpace.ToString(), minimiumFreeDiskSpace.ToString());
      return true;
    }

    static List<RecordingFileInfo> GetRecordingsOnDrive(string drive)
    {
      List<RecordingFileInfo> recordings = new List<RecordingFileInfo>();
      IList<Recording> recordedTvShows = Recording.ListAll();

      foreach (Recording recorded in recordedTvShows)
      {
        if (recorded.FileName.ToUpperInvariant()[0] != drive.ToUpperInvariant()[0])
          continue;

        bool add = true;
        foreach (RecordingFileInfo fi in recordings)
        {
          if (String.Compare(fi.filename, recorded.FileName, true) == 0)
          {
            add = false;
          }
        }
        if (add)
        {
          try
          {
            FileInfo info = new FileInfo(recorded.FileName);
            RecordingFileInfo fi = new RecordingFileInfo();
            fi.info = info;
            fi.filename = recorded.FileName;
            fi.record = recorded;
            recordings.Add(fi);
          } catch (Exception e)
          {
            Log.Error("DiskManagement: Exception at building FileInfo ({0})", recorded.FileName);
            Log.Write(e);
          }
        }
      }
      return recordings;
    }

    static void CheckDriveFreeDiskSpace(string drive)
    {
      //get disk quota to use
      if (!OutOfDiskSpace(drive))
        return;

      Log.Write("DiskManagement: not enough free space on drive: {0}", drive);

      List<RecordingFileInfo> recordings = GetRecordingsOnDrive(drive);
      if (recordings.Count == 0)
      {
        Log.Write("DiskManagement: no recordings to delete");
        return;
      }

      Log.Write("DiskManagement: found {0} recordings", recordings.Count);

      // Not enough free diskspace
      // start deleting recordings (oldest ones first)
      // until we have enough free disk space again
      recordings.Sort();
      while (OutOfDiskSpace(drive) && recordings.Count > 0)
      {
        RecordingFileInfo fi = recordings[0];
        if (fi.record.KeepUntil == (int)KeepMethodType.UntilSpaceNeeded)
        {
          // Delete the file from disk and the recording entry from the database.
          RecordingFileHandler handler = new RecordingFileHandler();
          bool result = handler.DeleteRecordingOnDisk(fi.record);
          if (result)
          {
            fi.record.Delete();
          }
        }
        recordings.RemoveAt(0);
      }
    }
  }
}
