using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Toub.MediaCenter.Dvrms.Metadata;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Recording;
using MediaPortal.TV.Database;

namespace MediaPortal.TV.DiskSpace
{
  class RecordingImporterWorker
  {
    static bool importing = false;
    static List<TVChannel> channels = new List<TVChannel>();
    static List<TVRecorded> recordings = new List<TVRecorded>();
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
      try
      {
        importing = true;
        //dont import during recording...
        if (Recorder.IsAnyCardRecording()) return;
        TVDatabase.GetRecordedTV(ref recordings);
        TVDatabase.GetChannels(ref channels);
        for (int i = 0; i < Recorder.Count; i++)
        {
          TVCaptureDevice dev = Recorder.Get(i);
          if (dev == null) continue;
          ProcessDirectory(dev.RecordingPath);
          string[] directories = System.IO.Directory.GetDirectories(dev.RecordingPath, "*", SearchOption.AllDirectories);
          foreach (string directory in directories)
          {
            ProcessDirectory(directory);
          }//foreach (string directory in directories)
        }//for (int i=0; i < Recorder.Count;++i)

      }
      catch (Exception)
      {
      }
      finally
      {
        importing = false;
      }
    } //static void ImportDvrMsFiles()


    static void ProcessDirectory(string directory)
    {
      try
      {
        int index = directory.IndexOf("card");
        if ((index == -1) || ((index != -1) && ((directory.Substring(index - 1, 1) != "\\"))))
        {
          string[] files = System.IO.Directory.GetFiles(directory, "*.dvr-ms");
          foreach (string file in files)
          {
            System.Threading.Thread.Sleep(100);
            bool add = true;
            foreach (TVRecorded rec in recordings)
            {
              if (Recorder.IsAnyCardRecording()) return;
              if (rec.FileName != null)
              {
                if (String.Compare(rec.FileName, file, true) == 0)
                {
                  add = false;
                  break;
                }
              }
            }
            if (add)
            {
              TVRecorded rec = AddFileToTvDatabase(file);
              if (rec != null)
              {
                recordings.Add(rec);
              }
              System.Threading.Thread.Sleep(100);
            }//if (add)
          }//foreach (string file in files)
        }//if ((index == -1)
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }



    static TVRecorded AddFileToTvDatabase(string fileName)
    {
      //Log.Info("Recorder: import recording {0}", file);
      try
      {
        using (DvrmsMetadataEditor editor = new DvrmsMetadataEditor(fileName))
        {
          TVRecorded newRec = new TVRecorded();
          newRec.FileName = fileName;

          IDictionary dict = editor.GetAttributes();
          if (dict == null) return null;
          foreach (MetadataItem item in dict.Values)
          {
            if (item == null) continue;
            if (item.Name == null) continue;
            //Log.WriteFile(LogType.Recorder,"attribute:{0} value:{1}", item.Name,item.Value.ToString());
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
            string name = MediaPortal.Util.Utils.GetFilename(fileName);
            foreach (TVChannel channel in channels)
            {
              if (name.Contains(channel.Name))
              {
                if ((newRec.Channel!= null) && (newRec.Channel.Length > 0))
                {
                  if (newRec.Channel.Length > channel.Name.Length) continue;
                }
                newRec.Channel = channel.Name;
              }
            }
            if ((newRec.Channel == null) && (channels.Count > 0))  // still no channel found
            {
              newRec.Channel = channels[0].Name;                   // assign the first one to have it in the DB
            }
          }

          if (newRec.Title == null || newRec.Title.Length == 0)
          {
            newRec.Title = MediaPortal.Util.Utils.GetFilename(fileName);  // to have at least one info in the data base about what it is
          }

          if (newRec.Channel != null && newRec.Channel.Length > 0)
          {
            int id = TVDatabase.AddRecordedTV(newRec);
            if (id >= 0)
            {
              return newRec;
            }
            //Log.Info("Recorder: import recording {0} failed");
          }
          else
          {
            //Log.Info("Recorder: import recording {0} failed, unknown tv channel", file);
          }
        }//using (DvrmsMetadataEditor editor = new DvrmsMetadataEditor(file))
      }
      catch (Exception)
      {
        //Log.Error("Recorder:Unable to import {0} reason:{1} {2} {3}", file, ex.Message, ex.Source, ex.StackTrace);
      }
      return null;
    }
  }
}
