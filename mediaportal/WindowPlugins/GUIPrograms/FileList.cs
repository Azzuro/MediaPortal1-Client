using System;
using System.Collections;
using MediaPortal.GUI.Library;
using Programs.Utils;
using SQLite.NET;
using WindowPlugins.GUIPrograms;

namespace ProgramsDatabase
{

  public class Filelist: ArrayList
  {

    string mFilepath = "";

    static SQLiteClient sqlDB = null;

    public Filelist(SQLiteClient initSqlDB)
    {
      // constructor: save SQLiteDB object 
      sqlDB = initSqlDB;
    }

    public string Filepath
    {
      get
      {
        return mFilepath;
      }
    }


    public FileItem GetFileByID(int nFileID)
    {
      foreach (FileItem curFile in this)
      {
        if (curFile.FileID == nFileID)
        {
          return curFile;
        }
      }
      return null;
    }


    static private FileItem DBGetFileItem(SQLiteResultSet results, int iRecord)
    {
      FileItem newFile = new FileItem(sqlDB);
      newFile.FileID = ProgramUtils.GetIntDef(results, iRecord, "fileid",  - 1);
      newFile.AppID = ProgramUtils.GetIntDef(results, iRecord, "appid",  - 1);
      newFile.Title = ProgramUtils.Get(results, iRecord, "title");
      newFile.Filename = ProgramUtils.Get(results, iRecord, "filename");
      newFile.Filepath = ProgramUtils.Get(results, iRecord, "filepath");
      newFile.Imagefile = ProgramUtils.Get(results, iRecord, "imagefile");
      newFile.Genre = ProgramUtils.Get(results, iRecord, "genre");
      newFile.Genre2 = ProgramUtils.Get(results, iRecord, "genre2");
      newFile.Genre3 = ProgramUtils.Get(results, iRecord, "genre3");
      newFile.Genre4 = ProgramUtils.Get(results, iRecord, "genre4");
      newFile.Genre5 = ProgramUtils.Get(results, iRecord, "genre5");
      newFile.Country = ProgramUtils.Get(results, iRecord, "country");
      newFile.Manufacturer = ProgramUtils.Get(results, iRecord, "manufacturer");
      newFile.Year = ProgramUtils.GetIntDef(results, iRecord, "year",  - 1);
      newFile.Rating = ProgramUtils.GetIntDef(results, iRecord, "rating", 5);
      newFile.Overview = ProgramUtils.Get(results, iRecord, "overview");
      newFile.System_ = ProgramUtils.Get(results, iRecord, "system");
      newFile.ExtFileID = ProgramUtils.GetIntDef(results, iRecord, "external_id",  - 1);
      newFile.LastTimeLaunched = ProgramUtils.GetDateDef(results, iRecord, "lastTimeLaunched", DateTime.MinValue);
      newFile.LaunchCount = ProgramUtils.GetIntDef(results, iRecord, "launchcount", 0);
      newFile.IsFolder = ProgramUtils.GetBool(results, iRecord, "isfolder");
      newFile.TagData = ProgramUtils.Get(results, iRecord, "tagdata");
      newFile.CategoryData = ProgramUtils.Get(results, iRecord, "categorydata");
      string fieldtype2 = ProgramUtils.Get(results, iRecord, "fieldtype2");
      if (fieldtype2 == "INT")
      {
        // so much to avoid round-differences!
        newFile.Title2 = ProgramUtils.GetIntDef(results, iRecord, "title2", -1).ToString();
        if (newFile.Title2 == "-1")
        {
          newFile.Title2 = "";
        }
      }
      else if (fieldtype2 == "STR")
      {
        newFile.Title2 = ProgramUtils.Get(results, iRecord, "title2");
      }
      return newFile;
    }

    static private ProgramFilterItem DBGetFilterItem(SQLiteResultSet results, int iRecord)
    {
      ProgramFilterItem newFilter = new ProgramFilterItem();
      string fieldtype = ProgramUtils.Get(results, iRecord, "fieldtype");
      if (fieldtype == "INT")
      {
        // so much to avoid round-differences!
        newFilter.Title = ProgramUtils.GetIntDef(results, iRecord, "title", -1).ToString();
        if (newFilter.Title == "-1")
        {
          newFilter.Title = "";
        }
      }
      else
      {
        newFilter.Title = ProgramUtils.Get(results, iRecord, "title");
      }
      string fieldtype2 = ProgramUtils.Get(results, iRecord, "fieldtype2");
      if (fieldtype2 == "INT")
      {
        // so much to avoid round-differences!
        newFilter.Title2 = ProgramUtils.GetIntDef(results, iRecord, "title2", -1).ToString();
        if (newFilter.Title2 == "-1")
        {
          newFilter.Title2 = "";
        }
      }
      else if (fieldtype2 == "STR")
      {
        newFilter.Title2 = ProgramUtils.Get(results, iRecord, "title2");
      }
      return newFilter;
    }


    public void Load(int appID, string pathSubfolders)
    {
      if (sqlDB == null)
        return ;
      if (ProgramSettings.viewHandler == null)
        return;
      try
      {
        Clear();
        if (null == sqlDB)
          return ;
        SQLiteResultSet results;
        mFilepath = pathSubfolders;
        string sqlQuery = ProgramSettings.viewHandler.BuildQuery(appID, pathSubfolders);
        // Log.Write("dw \n{0}", sqlQuery);
        results = sqlDB.Execute(sqlQuery);
        if (results.Rows.Count == 0)
          return ;
        for (int curRow = 0; curRow < results.Rows.Count; curRow++)
        {
          if (ProgramSettings.viewHandler.IsFilterQuery)
          {
            ProgramFilterItem curFilter = DBGetFilterItem(results, curRow);
            Add(curFilter);
          }
          else
          {
            FileItem curFile = DBGetFileItem(results, curRow);
            Add(curFile);
          }
        }
      }
      catch (SQLiteException ex)
      {
        Log.Write("Filedatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }
  }

}
