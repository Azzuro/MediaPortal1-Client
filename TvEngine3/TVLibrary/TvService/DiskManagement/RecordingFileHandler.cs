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
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using TvDatabase;
using TvLibrary.Log;
using System.Collections;

namespace TvService
{
  public class RecordingFileHandler
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public RecordingFileHandler()
    {
    }

    /// <summary>
    /// Deletes a recording from the disk where it is saved. 
    /// If the recording is split into multiple parts, they all will be deleted. 
    /// Additional information like the matroska xml files will also be removed.
    /// If the above results in an empty folder, we also clean that one.
    /// </summary>
    /// <param name="rec">The recording we want to delete the files for.</param>
    public bool DeleteRecordingOnDisk(Recording rec)
    {
      if (System.IO.File.Exists(rec.FileName))
      {
        try
        {
          //Delete the matroska tag info xml file 
          if (File.Exists(Path.ChangeExtension(rec.FileName, ".xml")))
            File.Delete(Path.ChangeExtension(rec.FileName, ".xml"));
          // If a recording got interrupted there may be files like <recording name>_1.mpg, etc.
          string SearchFile = System.IO.Path.GetFileNameWithoutExtension(rec.FileName) + @"*";
          // Check only the ending for underscores as a user might have a naming pattern including them between e.g. station and program title
          string SubSearch = SearchFile.Substring((SearchFile.Length - 3));
          int UnderScorePosition = SubSearch.LastIndexOf(@"_");
          if (UnderScorePosition != -1)
            // Length - 3 should be enough since there won't be thousands of files with the same name.
            SearchFile = SearchFile.Substring(0, SearchFile.Length - 3) + @"*";
          string[] allRecordingFiles = System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(rec.FileName), SearchFile);
          Log.Debug("RecordingFileHandler: found {0} file(s) to delete for recording {1}", Convert.ToString(allRecordingFiles.Length), SearchFile);
          foreach (string recPartPath in allRecordingFiles)
          {
            System.IO.File.Delete(recPartPath);
          }
          CleanRecordingFolders(rec.FileName);
        }
        catch (Exception ex)
        {
          Log.Error("RecordingFileHandler: Error while deleting a recording from disk: {0}", ex.Message);
          return false; // file not deleted, return failure
        }
        return true; // file deleted, return success
      }
      return true; // no file to delete, return success
    }

    /// <summary>
    /// When deleting a recording we check if the folder the recording
    /// was deleted from can be deleted.
    /// A folder must not be deleted, if there are still files or subfolders in it.
    /// </summary>
    /// <param name="fileName">The recording file which is deleted.</param>
    void CleanRecordingFolders(string fileName)
    {
      try
      {
        Log.Debug("RecordingFileHandler: Clean orphan recording dirs for {0}", fileName);
        string recfolder = System.IO.Path.GetDirectoryName(fileName);
        List<string> recordingPaths = new List<string>();

        IList cards = Card.ListAll();
        foreach (Card card in cards)
        {
          string currentCardPath = card.RecordingFolder;
          if (!recordingPaths.Contains(currentCardPath))
            recordingPaths.Add(currentCardPath);
        }
        Log.Debug("RecordingFileHandler: Checking {0} path(s) for cleanup", Convert.ToString(recordingPaths.Count));

        foreach (string checkPath in recordingPaths)
        {
          if (checkPath != string.Empty && checkPath != System.IO.Path.GetPathRoot(checkPath))
          {
            // make sure we're only deleting directories which are "recording dirs" from a tv card
            if (fileName.Contains(checkPath))
            {
              Log.Debug("RecordingFileHandler: Origin for recording {0} found: {1}", System.IO.Path.GetFileName(fileName), checkPath);
              string deleteDir = recfolder;
              // do not attempt to step higher than the recording base path
              while (deleteDir != System.IO.Path.GetDirectoryName(checkPath) && deleteDir.Length > checkPath.Length)
              {
                try
                {
                  string[] files = System.IO.Directory.GetFiles(deleteDir);
                  string[] subdirs = System.IO.Directory.GetDirectories(deleteDir);
                  if (files.Length == 0)
                  {
                    if (subdirs.Length == 0)
                    {
                      System.IO.Directory.Delete(deleteDir);
                      Log.Debug("RecordingFileHandler: Deleted empty recording dir - {0}", deleteDir);
                      DirectoryInfo di = System.IO.Directory.GetParent(deleteDir);
                      deleteDir = di.FullName;
                    }
                    else
                    {
                      Log.Debug("RecordingFileHandler: Found {0} sub-directory(s) in recording path - not cleaning {1}", Convert.ToString(subdirs.Length), deleteDir);
                      return;
                    }
                  }
                  else
                  {
                    Log.Debug("RecordingFileHandler: Found {0} file(s) in recording path - not cleaning {1}", Convert.ToString(files.Length), deleteDir);
                    return;
                  }
                }
                catch (Exception ex1)
                {
                  Log.Info("RecordingFileHandler: Could not delete directory {0} - {1}", deleteDir, ex1.Message);
                  // bail out to avoid i-loop
                  return;
                }
              }
            }
          }
          else
            Log.Debug("RecordingFileHandler: Path not valid for removal - {1}", checkPath);
        }
      }
      catch (Exception ex)
      {
        Log.Error("RecordingFileHandler: Error cleaning the recording folders - {0},{1}", ex.Message, ex.StackTrace);
      }
    }

  }
}
