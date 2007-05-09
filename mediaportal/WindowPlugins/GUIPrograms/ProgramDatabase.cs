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
using System.Collections;
using System.IO;
using MediaPortal.GUI.Library;
using Programs.Utils;
using SQLite.NET;
using WindowPlugins.GUIPrograms;
using MediaPortal.Database;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for DBPrograms.
  /// </summary>
  public class ProgramDatabase
  {
    public static SQLiteClient sqlDB = null;
    static Applist mAppList = null;
    static ProgramViewHandler viewHandler = null;

    // singleton. Dont allow any instance of this class
    private ProgramDatabase(){}

    static ProgramDatabase()
    {
      Open();

      viewHandler = new ProgramViewHandler();
      ProgramSettings.viewHandler = viewHandler;
      mAppList = new Applist(sqlDB, new AppItem.FilelinkLaunchEventHandler(LaunchFilelink));
    }

    private static void Open()
    {
      Log.Info("Opening ProgramDatabase");
      try
      {
        // Open database
        try
        {
          System.IO.Directory.CreateDirectory(Config.GetFolder(Config.Dir.Database));
        }
        catch (Exception) { }

        sqlDB = new SQLiteClient(Config.GetFile(Config.Dir.Database, "ProgramDatabaseV4.db3"));

        MediaPortal.Database.DatabaseUtility.SetPragmas(sqlDB);

        ProgramSettings.sqlDB = sqlDB;
        // make sure the DB-structure is complete
        CreateObjects();
        // patch old ContentID values...
        PatchContentID();
        // patch old values, which were made empty by old versions...
        PatchEmptyValues();
        // patch genre-values
        PatchGenreValues();
        // remove trigger
        PatchAppTrigger();
        // add PreLaunch / PostLaunch fields
        PatchPrePostLaunch();
        // dirty hack: propagate the sqlDB to the singleton objects...
        ProgramSettings.sqlDB = sqlDB;
      }
      catch (Exception ex)
      {
        Log.Info("ProgramDatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      Log.Info("ProgramDatabase opened");
    }

    static void LaunchFilelink(FilelinkItem curLink, bool MPGUIMode)
    {
      AppItem targetApp = mAppList.GetAppByID(curLink.TargetAppID);
      if (targetApp != null)
      {
        targetApp.LaunchFile(curLink, MPGUIMode);
      }
    }


    static bool ObjectExists(string strName, string strType)
    {
      SQLiteResultSet results;
      bool res = false;
      results = sqlDB.Execute("SELECT name FROM sqlite_master WHERE name='" + strName + "' and type='" + strType + 
        "' UNION ALL SELECT name FROM sqlite_temp_master WHERE type='" + strType + "' ORDER BY name");
      if (results != null && results.Rows.Count > 0)
      {
        if (results.Rows.Count == 1)
        {
          SQLiteResultSet.Row arr = results.Rows[0];
          if (arr.fields.Count == 1)
          {
            if (arr.fields[0] == strName)
            {
              res = true;
            }
          }
        }
      }
      return res;
    }


    static bool AddObject(string strName, string strType, string strSQL)
    // checks if object exists and returns true if it newly added the object
    {
      if (sqlDB == null)
        return false;
      if (ObjectExists(strName, strType))
        return false;
      try
      {
        sqlDB.Execute(strSQL);
      }
      catch (SQLiteException ex)
      {
        Log.Info("programdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        return false;
      }
      return true;
    }


    static bool CreateObjects()
    {
      bool skipPrePostPatch = false;
      if (sqlDB == null)
        return false;
      skipPrePostPatch = DatabaseUtility.AddTable(sqlDB,"application", "CREATE TABLE application (appid integer primary key, fatherID integer, title text, shorttitle text, filename text, arguments text, windowstyle text, startupdir text, useshellexecute text, usequotes text, source_type text, source text, imagefile text, filedirectory text, imagedirectory text, validextensions text, enabled text, importvalidimagesonly text, iposition integer, enableGUIRefresh text, GUIRefreshPossible text, contentID integer, systemdefault text, waitforexit text, pincode integer, preLaunch text, postLaunch text)");
      DatabaseUtility.AddTable(sqlDB, "tblfile",  "CREATE TABLE tblfile (fileid integer primary key, appid integer, title text, filename text, filepath text, imagefile text, genre text, genre2 text, genre3 text, genre4 text, genre5 text, country text, manufacturer text, year integer, rating integer, overview text, system text, import_flag integer, manualfilename text, lastTimeLaunched text, launchcount integer, isfolder text, external_id integer, uppertitle text, tagdata text, categorydata text)");
      DatabaseUtility.AddTable(sqlDB, "filterItem",  "CREATE TABLE filterItem (appid integer, grouperAppID integer, fileID integer, filename text, tag integer)");
      DatabaseUtility.AddTable(sqlDB, "setting",  "CREATE TABLE setting (settingid integer primary key, key text, value text)");
      DatabaseUtility.AddIndex(sqlDB, "idxFile1", "CREATE INDEX idxFile1 ON tblfile(appid)");
      DatabaseUtility.AddIndex(sqlDB, "idxFile2", "CREATE INDEX idxFile2 ON tblfile(filepath, uppertitle)");
      DatabaseUtility.AddIndex(sqlDB, "idxApp1", "CREATE INDEX idxApp1 ON application(fatherID)");
      DatabaseUtility.AddIndex(sqlDB, "idxFilterItem1", "CREATE UNIQUE INDEX idxFilterItem1 ON filterItem(appID, fileID, grouperAppID)");
      if (skipPrePostPatch)
      {
        // don't need to add prelaunch / postlaunch anymore if table was created above
        ProgramSettings.WriteSetting(ProgramUtils.cPREPOST_PATCH, "DONE") ;
      }

      return true;
    }

    static void PatchContentID()
    {
      if (sqlDB == null)
        return;
      if (!ProgramSettings.KeyExists(ProgramUtils.cCONTENT_PATCH))
      {
        Log.Info("myPrograms: applying contentID-patch");
        sqlDB.Execute("update application set contentID = 100 where contentID IS NULL");
        sqlDB.Execute("update application set contentID = 100 where contentID <= 0");
        ProgramSettings.WriteSetting(ProgramUtils.cCONTENT_PATCH, "DONE");
      }
    }

    static void PatchEmptyValues()
    {
      if (sqlDB == null)
        return;

      Log.Info("myPrograms: applying empty-value-patch");
      sqlDB.Execute("update tblfile set launchcount = 0 where launchcount = ''");
      sqlDB.Execute("update tblfile set launchcount = 0 where launchcount <= 0");
      sqlDB.Execute("update tblfile set lastTimeLaunched = '01.01.0001 00:00:00' where lastTimeLaunched = ''");
    }

    static void PatchGenreValues()
    {
      if (sqlDB == null)
        return;
      if (!ProgramSettings.KeyExists(ProgramUtils.cGENRE_PATCH))
      {
        Log.Info("myPrograms: applying genre-patch");
        sqlDB.Execute("update tblfile set genre = '' where genre IS NULL");
        sqlDB.Execute("update tblfile set genre2 = '' where genre2 IS NULL");
        sqlDB.Execute("update tblfile set genre3 = '' where genre3 IS NULL");
        sqlDB.Execute("update tblfile set genre4 = '' where genre4 IS NULL");
        sqlDB.Execute("update tblfile set genre5 = '' where genre5 IS NULL");
        ProgramSettings.WriteSetting(ProgramUtils.cGENRE_PATCH, "DONE") ;
      }
    }

    // trigger prevents sqlitev3 client from deleting apps....
    static void PatchAppTrigger()
    {
      if (sqlDB == null)
        return;
      if (ObjectExists("td_application", "trigger"))
      {
        try
        {
          sqlDB.Execute("drop trigger td_application");
        }
        catch (SQLiteException ex)
        {
          Log.Info("programdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        }
      }
    }

    static void PatchPrePostLaunch()
    {
      if (sqlDB == null)
        return;
      if (!ProgramSettings.KeyExists(ProgramUtils.cPREPOST_PATCH))
      {
        try
        {
        Log.Info("myPrograms: adding preLaunch / postLaunch fields");
        sqlDB.Execute("alter table application add column preLaunch text");
        sqlDB.Execute("alter table application add column postLaunch text");
        ProgramSettings.WriteSetting(ProgramUtils.cPREPOST_PATCH, "DONE") ;
        }
        catch (SQLiteException ex)
        {
          Log.Info("programdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        }
      }
    }





    static public Applist AppList
    {
      get
      {
        return mAppList;
      }
    }

    static public void GetFilesByFilter(string sql, out ArrayList files, bool artistTable, bool albumTable, bool songTable, bool genreTable)
    {
      files=new ArrayList();
      try
      {
        SQLiteResultSet results=GetResults(sql);
        FileItem file;

        for (int i=0; i<results.Rows.Count; i++)
        {
          file = new FileItem(sqlDB);
/*
 *           song = new Song();
          ArrayList fields = (ArrayList)results.Rows[i];
          if (artistTable && !songTable)
          {
            song.Artist = (string)fields[1];
            song.artistId= (int)Math.Floor(0.5d+Double.Parse((string)fields[0]));
          }
          if (albumTable && !songTable)
          {
            song.Album =  (string)fields[2];
            song.albumId = (int)Math.Floor(0.5d+Double.Parse((string)fields[0]));
            song.artistId= (int)Math.Floor(0.5d+Double.Parse((string)fields[1]));
            if (fields.Count>=5)
              song.Artist = (string)fields[4];
          }
          if (genreTable && !songTable)
          {
            song.Genre = (string)fields[1];
            song.genreId = (int)Math.Floor(0.5d+Double.Parse((string)fields[0]));
          }
          if (songTable)
          {
            song.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
            song.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
            song.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
            song.artistId= (int)Math.Floor(0.5d+Double.Parse(DatabaseUtility.Get(results, i, "song.idArtist")));
            song.Track = (int)Math.Floor(0.5d+Double.Parse(DatabaseUtility.Get(results, i, "song.iTrack")));
            song.Duration = (int)Math.Floor(0.5d+Double.Parse(DatabaseUtility.Get(results, i, "song.iDuration")));
            song.Year = (int)Math.Floor(0.5d+Double.Parse(DatabaseUtility.Get(results, i, "song.iYear")));
            song.Title = DatabaseUtility.Get(results, i, "song.strTitle");
            song.TimesPlayed = (int)Math.Floor(0.5d+Double.Parse(DatabaseUtility.Get(results, i, "song.iTimesPlayed")));
            song.Favorite=(int)Math.Floor(0.5d+Double.Parse(DatabaseUtility.Get(results, i, "song.favorite")))!=0;
            song.Rating= (int)Math.Floor(0.5d+Double.Parse(DatabaseUtility.Get(results, i, "song.iRating")));
            song.Favorite=(int)Math.Floor(0.5d+Double.Parse(DatabaseUtility.Get(results, i, "song.favorite")))!=0;
            song.songId= (int)Math.Floor(0.5d+Double.Parse(DatabaseUtility.Get(results, i, "song.idSong")));
            song.albumId= (int)Math.Floor(0.5d+Double.Parse(DatabaseUtility.Get(results, i, "song.idAlbum")));
            song.genreId= (int)Math.Floor(0.5d+Double.Parse(DatabaseUtility.Get(results, i, "song.idGenre")));
            string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
            strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
            song.FileName = strFileName;
          }
*/          
          files.Add(file);
        }	  

        return ;
      }
      catch (Exception ex) 
      {
        Log.Error("programdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

      return ;		
    }


    static public SQLiteResultSet GetResults(string sql)
    {
      try
      {
        if (null == sqlDB) return null;
        SQLiteResultSet results;
        results = sqlDB.Execute(sql);
        return results;
      }
      catch (Exception ex) 
      {
        Log.Error("programdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

      return null;	
    }

  }
}
