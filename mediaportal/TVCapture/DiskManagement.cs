/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Management;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using Toub.MediaCenter.Dvrms.Metadata;

namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Summary description for DiskManagement.
  /// </summary>
  public class DiskManagement
  {
    static bool importing = false;
    static DateTime _diskSpaceCheckTimer = DateTime.Now;
    static DateTime _deleteOldRecordingTimer = DateTime.MinValue;
    static  DiskManagement()
    {
      Recorder.OnTvRecordingEnded += new MediaPortal.TV.Recording.Recorder.OnTvRecordingHandler(DiskManagement.Recorder_OnTvRecordingEnded);
    }
    #region dvr-ms importing
    static public void DeleteRecording(string recordingFilename)
    {
      Utils.FileDelete(recordingFilename);
      int pos = recordingFilename.LastIndexOf(@"\");
      if (pos < 0) return;
      string path = recordingFilename.Substring(0, pos);
      string filename = recordingFilename.Substring(pos + 1);
      pos = filename.LastIndexOf(".");
      if (pos >= 0)
        filename = filename.Substring(0, pos);
      filename = filename.ToLower();
      string[] files;
      try
      {
        files = System.IO.Directory.GetFiles(path);
        foreach (string fileName in files)
        {
          try
          {
            if (fileName.ToLower().IndexOf(filename) >= 0)
            {
              if (fileName.ToLower().IndexOf(".sbe") >= 0)
              {
                System.IO.File.Delete(fileName);
              }
            }
          }
          catch (Exception) { }
        }
      }
      catch (Exception) { }
    }
    static public void ImportDvrMsFiles()
    {
      //dont import during recording...
      if (Recorder.IsAnyCardRecording()) return;
      if (importing) return;
      Thread WorkerThread = new Thread(new ThreadStart(ImportWorkerThreadFunction));
      WorkerThread.SetApartmentState(ApartmentState.STA);
      WorkerThread.IsBackground = true;
      WorkerThread.Start();
    }
    static void ImportWorkerThreadFunction()
    {
      System.Threading.Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
      importing = true;
      try
      {
        //dont import during recording...
        if (Recorder.IsAnyCardRecording()) return;
        List<TVRecorded> recordings = new List<TVRecorded>();
        TVDatabase.GetRecordedTV(ref recordings);
        for (int i = 0; i < Recorder.Count; i++)
        {
          TVCaptureDevice dev = Recorder.Get(i);
          if (dev == null) continue;
          try
          {
            string[] files = System.IO.Directory.GetFiles(dev.RecordingPath, "*.dvr-ms");
            foreach (string file in files)
            {
              System.Threading.Thread.Sleep(100);
              bool add = true;
              foreach (TVRecorded rec in recordings)
              {
                if (Recorder.IsAnyCardRecording()) return;
                if (rec.FileName != null)
                {
                  if (rec.FileName.ToLower() == file.ToLower())
                  {
                    add = false;
                    break;
                  }
                }
              }
              if (add)
              {
                Log.WriteFile(Log.LogType.Recorder, "Recorder: import recording {0}", file);
                try
                {
                  System.Threading.Thread.Sleep(100);
                  using (DvrmsMetadataEditor editor = new DvrmsMetadataEditor(file))
                  {
                    IDictionary dict = editor.GetAttributes();
                    if (dict != null)
                    {
                      TVRecorded newRec = new TVRecorded();
                      newRec.FileName = file;
                      foreach (MetadataItem item in dict.Values)
                      {
                        if (item == null) continue;
                        if (item.Name == null) continue;
                        //Log.WriteFile(Log.LogType.Recorder,"attribute:{0} value:{1}", item.Name,item.Value.ToString());
                        try { if (item.Name.ToLower() == "channel") newRec.Channel = (string)item.Value.ToString(); }
                        catch (Exception) { }
                        try { if (item.Name.ToLower() == "title") newRec.Title = (string)item.Value.ToString(); }
                        catch (Exception) { }
                        try { if (item.Name.ToLower() == "programtitle") newRec.Title = (string)item.Value.ToString(); }
                        catch (Exception) { }
                        try { if (item.Name.ToLower() == "genre") newRec.Genre = (string)item.Value.ToString(); }
                        catch (Exception) { }
                        try { if (item.Name.ToLower() == "details") newRec.Description = (string)item.Value.ToString(); }
                        catch (Exception) { }
                        try { if (item.Name.ToLower() == "start") newRec.Start = (long)UInt64.Parse(item.Value.ToString()); }
                        catch (Exception) { }
                        try { if (item.Name.ToLower() == "end") newRec.End = (long)UInt64.Parse(item.Value.ToString()); }
                        catch (Exception) { }
                      }
                      if (newRec.Channel == null)
                      {
                        string name = Utils.GetFilename(file);
                        string[] parts = name.Split('_');
                        if (parts.Length > 0)
                          newRec.Channel = parts[0];
                      }
                      if (newRec.Channel != null && newRec.Channel.Length > 0)
                      {
                        int id = TVDatabase.AddRecordedTV(newRec);
                        if (id < 0)
                        {
                          Log.WriteFile(Log.LogType.Recorder, "Recorder: import recording {0} failed");
                        }
                        recordings.Add(newRec);
                      }
                      else
                      {
                        Log.WriteFile(Log.LogType.Recorder, "Recorder: import recording {0} failed, unknown tv channel", file);
                      }
                    }
                  }//using (DvrmsMetadataEditor editor = new DvrmsMetadataEditor(file))
                }
                catch (Exception ex)
                {
                  Log.WriteFile(Log.LogType.Log, true, "Recorder:Unable to import {0} reason:{1} {2} {3}", file, ex.Message, ex.Source, ex.StackTrace);
                }
              }//if (add)
            }//foreach (string file in files)
          }
          catch (Exception ex)
          {
            Log.WriteFile(Log.LogType.Log, true, "Recorder:Exception while importing recordings reason:{0} {1}", ex.Message, ex.Source);
          }
        }//for (int i=0; i < Recorder.Count;++i)
      }
      catch (Exception)
      {
      }
      importing = false;
    } //static void ImportDvrMsFiles()
    #endregion


    #region diskmanagement
    /// <summary>
    /// this method deleted any timeshifting files in the specified folder
    /// </summary>
    /// <param name="path">folder name</param>
    static public void DeleteOldTimeShiftFiles(string path)
    {
      if (path == null) return;
      if (path == String.Empty) return;
      // Remove any trailing slashes
      path = Utils.RemoveTrailingSlash(path);


      // clean the TempDVR\ folder
      string directory = String.Empty;
      string[] files;
      try
      {
        directory = String.Format(@"{0}\TempDVR", path);
        files = System.IO.Directory.GetFiles(directory, "*.tmp");
        foreach (string fileName in files)
        {
          try
          {
            System.IO.File.Delete(fileName);
          }
          catch (Exception) { }
        }
      }
      catch (Exception) { }

      // clean the TempSBE\ folder
      try
      {
        directory = String.Format(@"{0}\TempSBE", path);
        files = System.IO.Directory.GetFiles(directory, "*.tmp");
        foreach (string fileName in files)
        {
          try
          {
            System.IO.File.Delete(fileName);
          }
          catch (Exception) { }
        }
      }
      catch (Exception) { }

      // delete *.tv
      try
      {
        directory = String.Format(@"{0}", path);
        files = System.IO.Directory.GetFiles(directory, "*.tv");
        foreach (string fileName in files)
        {
          try
          {
            System.IO.File.Delete(fileName);
          }
          catch (Exception) { }
        }
      }
      catch (Exception) { }
    }//static void DeleteOldTimeShiftFiles(string path)

    static public void Process()
    {
      if (DateTime.Now.Date != _deleteOldRecordingTimer.Date)
      {
        _deleteOldRecordingTimer = DateTime.Now;
        while (true)
        {
          bool deleted = false;
          List<TVRecorded> recordings = new List<TVRecorded>();
          TVDatabase.GetRecordedTV(ref recordings);
          foreach (TVRecorded rec in recordings)
          {
            if (rec.KeepRecordingMethod == TVRecorded.KeepMethod.TillDate)
            {
              if (rec.KeepRecordingTill.Date <= DateTime.Now.Date)
              {
                if (Utils.FileDelete(rec.FileName))
                {
                  Log.WriteFile(Log.LogType.Recorder, "Recorder: delete old recording:{0} date:{1}",
                                    rec.FileName,
                                    rec.StartTime.ToShortTimeString());
                  TVDatabase.RemoveRecordedTV(rec);
                  DeleteRecording(rec.FileName);
                  VideoDatabase.DeleteMovie(rec.FileName);
                  VideoDatabase.DeleteMovieInfo(rec.FileName);
                  deleted = true;
                  break;
                }
              }
            }
          }
          if (!deleted) return;
        }
      }
    }

    static public void CheckRecordingDiskSpace()
    {
      TimeSpan ts = DateTime.Now - _diskSpaceCheckTimer;
      if (ts.TotalMinutes < 1) return;

      _diskSpaceCheckTimer = DateTime.Now;

      //first get all drives..
      List<string> drives = new List<string>();
      for (int i = 0; i < Recorder.Count; ++i)
      {
        TVCaptureDevice dev = Recorder.Get(i);
        if (dev.RecordingPath == null) continue;
        if (dev.RecordingPath.Length < 2) continue;
        string drive = dev.RecordingPath.Substring(0, 2);
        bool newDrive = true;
        foreach (string tmpDrive in drives)
        {
          if (drive.ToLower() == tmpDrive.ToLower())
          {
            newDrive = false;
          }
        }
        if (newDrive) drives.Add(drive);
      }

      // for each drive get all recordings
      List<RecordingFileInfo> recordings = new List<RecordingFileInfo>();
      foreach (string drive in drives)
      {
        recordings.Clear();
        long lMaxRecordingSize = 0;
        long diskSize = 0;
        try
        {
          string cmd = String.Format("win32_logicaldisk.deviceid=\"{0}:\"", drive[0]);
          using (ManagementObject disk = new ManagementObject(cmd))
          {
            disk.Get();
            diskSize = Int64.Parse(disk["Size"].ToString());
          }
        }
        catch (Exception)
        {
          continue;
        }

        for (int i = 0; i < Recorder.Count; ++i)
        {
          TVCaptureDevice dev = Recorder.Get(i);
          dev.GetRecordings(drive, ref recordings);

          int percentage = dev.MaxSizeLimit;
          long lMaxSize = (long)(((float)diskSize) * (((float)percentage) / 100f));
          if (lMaxSize > lMaxRecordingSize)
            lMaxRecordingSize = lMaxSize;
        }//foreach (TVCaptureDevice dev in m_tvcards)

        long totalSize = 0;
        foreach (RecordingFileInfo info in recordings)
        {
          totalSize += info.info.Length;
        }

        if (totalSize >= lMaxRecordingSize && lMaxRecordingSize > 0)
        {
          Log.WriteFile(Log.LogType.Recorder, "Recorder: exceeded diskspace limit for recordings on drive:{0}", drive);
          Log.WriteFile(Log.LogType.Recorder, "Recorder:   {0} recordings contain {1} while limit is {2}",
                                                recordings.Count, Utils.GetSize(totalSize), Utils.GetSize(lMaxRecordingSize));

          // we exceeded the diskspace
          //delete oldest files...
          recordings.Sort();
          while (totalSize > lMaxRecordingSize && recordings.Count > 0)
          {
            RecordingFileInfo fi = (RecordingFileInfo)recordings[0];
            List<TVRecorded> tvrecs = new List<TVRecorded>();
            TVDatabase.GetRecordedTV(ref tvrecs);
            foreach (TVRecorded tvrec in tvrecs)
            {
              if (tvrec.FileName.ToLower() == fi.filename.ToLower())
              {
                if (tvrec.KeepRecordingMethod == TVRecorded.KeepMethod.UntilSpaceNeeded)
                {
                  Log.WriteFile(Log.LogType.Recorder, "Recorder: delete old recording:{0} size:{1} date:{2} {3}",
                                                      fi.filename,
                                                      Utils.GetSize(fi.info.Length),
                                                      fi.info.CreationTime.ToShortDateString(), fi.info.CreationTime.ToShortTimeString());
                  totalSize -= fi.info.Length;
                  if (Utils.FileDelete(fi.filename))
                  {
                    TVDatabase.RemoveRecordedTV(tvrec);
                    DeleteRecording(fi.filename);
                    VideoDatabase.DeleteMovie(fi.filename);
                    VideoDatabase.DeleteMovieInfo(fi.filename);
                  }
                }
                break;
              }//if (tvrec.FileName.ToLower()==fi.filename.ToLower())
            }//foreach (TVRecorded tvrec in tvrecs)
            recordings.RemoveAt(0);
          }//while (totalSize > m_lMaxRecordingSize && files.Count>0)
        }//if (totalSize >= lMaxRecordingSize && lMaxRecordingSize >0) 
      }//foreach (string drive in drives)
    }//static void CheckRecordingDiskSpace()

    #endregion

    #region episode disk management
    static private void Recorder_OnTvRecordingEnded(string recordingFilename, TVRecording recording, TVProgram program)
    {
      Log.WriteFile(Log.LogType.Recorder, "diskmanagement: recording {0} ended. type:{1} max episodes:{2}",
          recording.Title,recording.RecType.ToString(), recording.EpisodesToKeep);

      if (recording.EpisodesToKeep == Int32.MaxValue) return;
      if (recording.RecType == TVRecording.RecordingType.Once) return;

      //check how many episodes we got
      List<TVRecorded> recordings = new List<TVRecorded>();
      TVDatabase.GetRecordedTV(ref recordings);
      while (true)
      {
        Log.WriteFile(Log.LogType.Recorder, "got:{0} recordings", recordings.Count);
        int recordingsFound = 0;
        DateTime oldestRecording = DateTime.MaxValue;
        string oldestFileName = String.Empty;
        TVRecorded oldestRec = null;
        foreach (TVRecorded rec in recordings)
        {
          Log.WriteFile(Log.LogType.Recorder, "check:{0}", rec.Title);
          if (String.Compare(rec.Title,recording.Title,true)==0)
          {
            recordingsFound++;
            if (rec.StartTime < oldestRecording)
            {
              oldestRecording = rec.StartTime;
              oldestFileName = rec.FileName;
              oldestRec = rec;
            }
          }
        }
        Log.WriteFile(Log.LogType.Recorder, "diskmanagement:   total episodes now:{0}", recordingsFound);
        if (oldestRec!=null)
        {
          Log.WriteFile(Log.LogType.Recorder, "diskmanagement:   oldest episode:{0} {1}", oldestRec.StartTime.ToShortDateString(), oldestRec.StartTime.ToLongTimeString() );
        }

        if (oldestRec == null) return;
        if (recordingsFound == 0) return;
        if (recordingsFound <= recording.EpisodesToKeep) return;
        Log.WriteFile(Log.LogType.Recorder, false, "diskmanagement:   Delete episode {0} {1} {2} {3}",
                             oldestRec.Channel,
                             oldestRec.Title,
                             oldestRec.StartTime.ToLongDateString(),
                             oldestRec.StartTime.ToLongTimeString());

        Utils.FileDelete(oldestFileName);
        DeleteRecording(oldestFileName);
        VideoDatabase.DeleteMovie(oldestFileName);
        VideoDatabase.DeleteMovieInfo(oldestFileName);
        recordings.Remove(oldestRec);
        TVDatabase.RemoveRecordedTV(oldestRec);
      }
    }
    #endregion
  }
}
