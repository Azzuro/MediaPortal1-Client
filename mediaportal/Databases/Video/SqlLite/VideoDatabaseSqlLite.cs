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
using System.Collections;
using SQLite.NET;
using MediaPortal.Util;
using MediaPortal.Database;
using MediaPortal.GUI.Library;

namespace MediaPortal.Video.Database
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class VideoDatabaseSqlLite : IVideoDatabase, IDisposable
  {
    public SQLiteClient m_db = null;

    #region ctor
    public VideoDatabaseSqlLite()
    {
      Open();
    }
    #endregion

    #region helper funcs
    void Open()
    {

      Log.WriteFile(Log.LogType.Log, false, "opening video database");
      try
      {
        // Open database
        String strPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.
          GetExecutingAssembly().Location);
        try
        {
          System.IO.Directory.CreateDirectory(strPath + @"\database");
        }
        catch (Exception) { }
        m_db = new SQLiteClient(strPath + @"\database\VideoDatabaseV5.db3");
        DatabaseUtility.SetPragmas(m_db);
        CreateTables();


      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

      Log.WriteFile(Log.LogType.Log, false, "video database opened");
    }
    public void Dispose()
    {
      if (m_db != null)
      {
        m_db.Close();
        m_db.Dispose();
        m_db = null;
      }
    }

    bool CreateTables()
    {
      if (m_db == null) return false;
      DatabaseUtility.AddTable(m_db, "bookmark", "CREATE TABLE bookmark ( idBookmark integer primary key, idFile integer, fPercentage text)");
      DatabaseUtility.AddTable(m_db, "genre", "CREATE TABLE genre ( idGenre integer primary key, strGenre text)");
      DatabaseUtility.AddTable(m_db, "genrelinkmovie", "CREATE TABLE genrelinkmovie ( idGenre integer, idMovie integer)");
      DatabaseUtility.AddTable(m_db, "movie", "CREATE TABLE movie ( idMovie integer primary key, idPath integer, hasSubtitles integer, discid text)");
      DatabaseUtility.AddTable(m_db, "movieinfo", "CREATE TABLE movieinfo ( idMovie integer, idDirector integer, strPlotOutline text, strPlot text, strTagLine text, strVotes text, fRating text,strCast text,strCredits text, iYear integer, strGenre text, strPictureURL text, strTitle text, IMDBID text, mpaa text,runtime integer, iswatched integer)");
      DatabaseUtility.AddTable(m_db, "actorlinkmovie", "CREATE TABLE actorlinkmovie ( idActor integer, idMovie integer )");
      DatabaseUtility.AddTable(m_db, "actors", "CREATE TABLE actors ( idActor integer primary key, strActor text )");
      DatabaseUtility.AddTable(m_db, "path", "CREATE TABLE path ( idPath integer primary key, strPath text, cdlabel text)");
      DatabaseUtility.AddTable(m_db, "files", "CREATE TABLE files ( idFile integer primary key, idPath integer, idMovie integer,strFilename text)");
      DatabaseUtility.AddTable(m_db, "resume", "CREATE TABLE resume ( idResume integer primary key, idFile integer, stoptime integer, resumeData blob)");
      DatabaseUtility.AddTable(m_db, "duration", "CREATE TABLE duration ( idDuration integer primary key, idFile integer, duration integer)");

      DatabaseUtility.AddTable(m_db, "actorinfo", "CREATE TABLE actorinfo ( idActor integer, dateofbirth text, placeofbirth text, minibio text, biography text)");
      DatabaseUtility.AddTable(m_db, "actorinfomovies", "CREATE TABLE actorinfomovies ( idActor integer, idDirector integer, strPlotOutline text, strPlot text, strTagLine text, strVotes text, fRating text,strCast text,strCredits text, iYear integer, strGenre text, strPictureURL text, strTitle text, IMDBID text, mpaa text,runtime integer, iswatched integer, role text)");
      return true;
    }

    #endregion

    #region Movie files and paths
    public int AddFile(int lMovieId, int lPathId, string strFileName)
    {
      if (m_db == null) return -1;
      string strSQL = "";
      try
      {
        int lFileId = -1;
        SQLiteResultSet results;
        strFileName = strFileName.Trim();

        strSQL = String.Format("select * from files where idmovie={0} and idpath={1} and strFileName like '{2}'", lMovieId, lPathId, strFileName);
        results = m_db.Execute(strSQL);
        if (results != null && results.Rows.Count > 0)
        {
          lFileId = System.Int32.Parse(DatabaseUtility.Get(results, 0, "idFile"));
          return lFileId;
        }
        strSQL = String.Format("insert into files (idFile, idMovie,idPath, strFileName) values(null, {0},{1},'{2}')", lMovieId, lPathId, strFileName);
        results = m_db.Execute(strSQL);
        lFileId = m_db.LastInsertID();
        return lFileId;
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public int GetFile(string strFilenameAndPath, out int lPathId, out int lMovieId, bool bExact)
    {
      lPathId = -1;
      lMovieId = -1;
      try
      {
        if (null == m_db) return -1;
        string strPath, strFileName;
        string cdlabel = GetDVDLabel(strFilenameAndPath);
        DatabaseUtility.RemoveInvalidChars(ref cdlabel);
        strFilenameAndPath = strFilenameAndPath.Trim();
        DatabaseUtility.Split(strFilenameAndPath, out strPath, out strFileName);
        DatabaseUtility.RemoveInvalidChars(ref strPath);
        lPathId = GetPath(strPath);
        if (lPathId < 0) return -1;

        string strSQL;
        SQLiteResultSet results;
        strSQL = String.Format("select * from files where idpath={0}", lPathId);
        results = m_db.Execute(strSQL);
        if (results.Rows.Count > 0)
        {
          for (int iRow = 0; iRow < results.Rows.Count; ++iRow)
          {
            string strFname = DatabaseUtility.Get(results, iRow, "strFilename");
            if (bExact)
            {
              if (strFname == strFileName)
              {
                // was just returning 'true' here, but this caused problems with
                // the bookmarks as these are stored by fileid. forza.
                int lFileId = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "idFile"));
                lMovieId = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "idMovie"));
                return lFileId;
              }
            }
            else
            {
              if (Utils.ShouldStack(strFname, strFileName))
              {
                int lFileId = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "idFile"));
                lMovieId = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "idMovie"));
                return lFileId;
              }
              if (strFname == strFileName)
              {
                // was just returning 'true' here, but this caused problems with
                // the bookmarks as these are stored by fileid. forza.
                int lFileId = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "idFile"));
                lMovieId = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "idMovie"));
                return lFileId;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public int AddMovieFile(string strFile)
    {
      bool bHassubtitles = false;
      if (strFile.ToLower().IndexOf(".ifo") >= 0) bHassubtitles = true;
      if (strFile.ToLower().IndexOf(".vob") >= 0) bHassubtitles = true;
      string strCDLabel = "";
      if (Utils.IsDVD(strFile))
      {
        strCDLabel = Utils.GetDriveSerial(strFile);
      }
      string[] sub_exts = { ".utf", ".utf8", ".utf-8", ".sub", ".srt", ".smi", ".rt", ".txt", ".ssa", ".aqt", ".jss", ".ass", ".idx", ".ifo" };
      // check if movie has subtitles
      for (int i = 0; i < sub_exts.Length; i++)
      {
        string strSubTitleFile = strFile;
        strSubTitleFile = System.IO.Path.ChangeExtension(strFile, sub_exts[i]);
        if (System.IO.File.Exists(strSubTitleFile))
        {
          bHassubtitles = true;
          break;
        }
      }
      return VideoDatabase.AddMovie(strFile, bHassubtitles);
    }

    public int AddPath(string strPath)
    {
      try
      {
        if (null == m_db) return -1;
        string strSQL;

        string cdlabel = GetDVDLabel(strPath);
        DatabaseUtility.RemoveInvalidChars(ref cdlabel);

        strPath = strPath.Trim();
        SQLiteResultSet results;
        strSQL = String.Format("select * from path where strPath like '{0}' and cdlabel like '{1}'", strPath, cdlabel);
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into Path (idPath, strPath, cdlabel) values( NULL, '{0}', '{1}')", strPath, cdlabel);
          m_db.Execute(strSQL);
          int lPathId = m_db.LastInsertID();
          return lPathId;
        }
        else
        {
          int lPathId = System.Int32.Parse(DatabaseUtility.Get(results, 0, "idPath"));
          return lPathId;
        }

      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public int GetPath(string strPath)
    {
      try
      {
        if (null == m_db) return -1;
        string strSQL;
        string cdlabel = GetDVDLabel(strPath);
        DatabaseUtility.RemoveInvalidChars(ref cdlabel);

        strPath = strPath.Trim();
        strSQL = String.Format("select * from path where strPath like '{0}' and cdlabel like '{1}'", strPath, cdlabel);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count > 0)
        {
          int lPathId = System.Int32.Parse(DatabaseUtility.Get(results, 0, "idPath"));
          return lPathId;
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public void DeleteFile(int iFileId)
    {
      try
      {
        string strSQL;
        strSQL = String.Format("delete from files where idfile={0}", iFileId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from resume where idfile={0}", iFileId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from duration where idfile={0}", iFileId);
        m_db.Execute(strSQL);
      }
      catch (SQLiteException ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public void RemoveFilesForMovie(int lMovieId)
    {
      try
      {
        if (null == m_db) return;
        string strSQL = String.Format("delete from files where idMovie={0}", lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public int GetFileId(string strFilenameAndPath)
    {
      int lPathId;
      int lMovieId;
      return GetFile(strFilenameAndPath, out lPathId, out  lMovieId, true);
    }

    public void GetFiles(int lMovieId, ref ArrayList movies)
    {
      try
      {
        movies.Clear();
        if (null == m_db) return;
        if (lMovieId < 0) return;

        string strSQL;
        strSQL = String.Format("select * from path,files where path.idPath=files.idPath and files.idmovie={0} order by strFilename", lMovieId);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          string strPath, strFile;
          strFile = DatabaseUtility.Get(results, i, "files.strFilename");
          strPath = DatabaseUtility.Get(results, i, "path.strPath");
          if (strPath != Strings.Unknown)
            strFile = strPath + strFile;
          movies.Add(strFile);
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }


    #endregion

    #region Genres
    public int AddGenre(string strGenre1)
    {
      try
      {
        string strGenre = strGenre1.Trim();
        DatabaseUtility.RemoveInvalidChars(ref strGenre);

        if (null == m_db) return -1;
        SQLiteResultSet results;
        string strSQL = "select * from genre where strGenre like '";
        strSQL += strGenre;
        strSQL += "'";
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = "insert into genre (idGenre, strGenre) values( NULL, '";
          strSQL += strGenre;
          strSQL += "')";
          m_db.Execute(strSQL);
          int lGenreId = m_db.LastInsertID();
          return lGenreId;
        }
        else
        {
          int lGenreId = System.Int32.Parse(DatabaseUtility.Get(results, 0, "idGenre"));
          return lGenreId;
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public void GetGenres(ArrayList genres)
    {
      if (m_db == null) return;
      try
      {
        genres.Clear();
        if (null == m_db) return;
        SQLiteResultSet results;
        results = m_db.Execute("select * from genre order by strGenre");
        if (results.Rows.Count == 0) return;
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          genres.Add(DatabaseUtility.Get(results, iRow, "strGenre"));
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

    }

    public void AddGenreToMovie(int lMovieId, int lGenreId)
    {
      try
      {
        if (null == m_db) return;
        string strSQL;
        strSQL = String.Format("select * from genrelinkmovie where idGenre={0} and idMovie={1}", lGenreId, lMovieId);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into genrelinkmovie (idGenre, idMovie) values( {0},{1})", lGenreId, lMovieId);
          m_db.Execute(strSQL);
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void DeleteGenre(string genre)
    {
      try
      {
        string genreFiltered = genre;
        DatabaseUtility.RemoveInvalidChars(ref genreFiltered);
        string sql = String.Format("select * from genre where strGenre like '{0}'", genreFiltered);
        SQLiteResultSet results;
        results = m_db.Execute(sql);
        if (results.Rows.Count == 0) return;

        int idGenre = Int32.Parse(DatabaseUtility.Get(results, 0, "idGenre"));
        m_db.Execute(sql);

        m_db.Execute(String.Format("delete from genrelinkmovie where idGenre={0}", idGenre));
        m_db.Execute(String.Format("delete from genre where idGenre={0}", idGenre));

      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void RemoveGenresForMovie(int lMovieId)
    {
      try
      {
        if (null == m_db) return;
        string strSQL = String.Format("delete from genrelinkmovie where idMovie={0}", lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }


    #endregion

    #region Actors
    public int AddActor(string strActor1)
    {
      try
      {
        string strActor = strActor1;
        DatabaseUtility.RemoveInvalidChars(ref strActor);
        if (null == m_db) return -1;
        string strSQL = "select * from Actors where strActor like '";
        strSQL += strActor;
        strSQL += "'";
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = "insert into Actors (idActor, strActor) values( NULL, '";
          strSQL += strActor;
          strSQL += "')";
          m_db.Execute(strSQL);
          int lActorId = m_db.LastInsertID();
          return lActorId;
        }
        else
        {
          int lActorId = System.Int32.Parse(DatabaseUtility.Get(results, 0, "idActor"));
          return lActorId;
        }

      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public void GetActors(ArrayList actors)
    {
      if (m_db == null) return;
      try
      {
        actors.Clear();
        if (null == m_db) return;
        SQLiteResultSet results;
        results = m_db.Execute("select * from actors");
        if (results.Rows.Count == 0) return;
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          actors.Add(DatabaseUtility.Get(results, iRow, "strActor"));
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void AddActorToMovie(int lMovieId, int lActorId)
    {
      try
      {
        if (null == m_db) return;
        string strSQL;
        strSQL = String.Format("select * from actorlinkmovie where idActor={0} and idMovie={1}", lActorId, lMovieId);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into actorlinkmovie (idActor, idMovie) values( {0},{1})", lActorId, lMovieId);
          m_db.Execute(strSQL);
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void DeleteActor(string actor)
    {
      try
      {
        string actorFiltered = actor;
        DatabaseUtility.RemoveInvalidChars(ref actorFiltered);
        string sql = String.Format("select * from actors where strActor like '{0}'", actorFiltered);
        SQLiteResultSet results;
        results = m_db.Execute(sql);
        if (results.Rows.Count == 0) return;

        int idactor = Int32.Parse(DatabaseUtility.Get(results, 0, "idActor"));
        m_db.Execute(sql);

        m_db.Execute(String.Format("delete from actorlinkmovie where idActor={0}", idactor));
        m_db.Execute(String.Format("delete from actors where idActor={0}", idactor));

      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void RemoveActorsForMovie(int lMovieId)
    {
      try
      {
        if (null == m_db) return;
        string strSQL = String.Format("delete from actorlinkmovie where idMovie={0}", lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }


    #endregion

    #region bookmarks
    public void ClearBookMarksOfMovie(string strFilenameAndPath)
    {
      try
      {
        if (null == m_db) return;
        int lPathId, lMovieId;
        int lFileId = GetFile(strFilenameAndPath, out lPathId, out lMovieId, true);
        if (lFileId < 0) return;
        string strSQL;
        strSQL = String.Format("delete from bookmark where idFile={0}", lFileId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }
    public void AddBookMarkToMovie(string strFilenameAndPath, float fTime)
    {
      try
      {
        int lPathId, lMovieId;
        int lFileId = GetFile(strFilenameAndPath, out lPathId, out lMovieId, true);
        if (lFileId < 0) return;
        if (null == m_db) return;
        string strSQL;
        strSQL = String.Format("select * from bookmark where idFile={0} and fPercentage='{1}'", lFileId, fTime);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count != 0) return;

        strSQL = String.Format("insert into bookmark (idBookmark, idFile, fPercentage) values(NULL,{0},'{1}')", lFileId, fTime);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetBookMarksForMovie(string strFilenameAndPath, ref ArrayList bookmarks)
    {
      bookmarks.Clear();
      try
      {
        int lPathId, lMovieId;
        int lFileId = GetFile(strFilenameAndPath, out lPathId, out lMovieId, true);
        if (lFileId < 0) return;
        bookmarks.Clear();
        if (null == m_db) return;

        string strSQL;
        strSQL = String.Format("select * from bookmark where idFile={0} order by fPercentage", lFileId);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return;
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          double fTime = Convert.ToDouble(DatabaseUtility.Get(results, iRow, "fPercentage"));
          bookmarks.Add(fTime);
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    #endregion

    #region MovieInfo
    public void SetMovieInfo(string strFilenameAndPath, ref IMDBMovie details)
    {
      if (strFilenameAndPath.Length == 0) return;
      int lMovieId = GetMovie(strFilenameAndPath, true);
      if (lMovieId < 0) return;
      details.ID = lMovieId;
      SetMovieInfoById(lMovieId, ref details);

      string strPath, strFileName;
      DatabaseUtility.Split(strFilenameAndPath, out strPath, out strFileName);
      details.Path = strPath;
      details.File = strFileName;
    }
    public void SetMovieInfoById(int lMovieId, ref IMDBMovie details)
    {
      try
      {
        details.ID = lMovieId;

        IMDBMovie details1 = details;
        string strLine;
        strLine = details1.Cast; DatabaseUtility.RemoveInvalidChars(ref strLine); details1.Cast = strLine;
        strLine = details1.Director; DatabaseUtility.RemoveInvalidChars(ref strLine); details1.Director = strLine;
        strLine = details1.Plot; DatabaseUtility.RemoveInvalidChars(ref strLine); details1.Plot = strLine;
        strLine = details1.PlotOutline; DatabaseUtility.RemoveInvalidChars(ref strLine); details1.PlotOutline = strLine;
        strLine = details1.TagLine; DatabaseUtility.RemoveInvalidChars(ref strLine); details1.TagLine = strLine;
        strLine = details1.ThumbURL; DatabaseUtility.RemoveInvalidChars(ref strLine); details1.ThumbURL = strLine;
        strLine = details1.SearchString; DatabaseUtility.RemoveInvalidChars(ref strLine); details1.SearchString = strLine;
        strLine = details1.Title; DatabaseUtility.RemoveInvalidChars(ref strLine); details1.Title = strLine;
        strLine = details1.Votes; DatabaseUtility.RemoveInvalidChars(ref strLine); details1.Votes = strLine;
        strLine = details1.WritingCredits; DatabaseUtility.RemoveInvalidChars(ref strLine); details1.WritingCredits = strLine;
        strLine = details1.Genre; DatabaseUtility.RemoveInvalidChars(ref strLine); details1.Genre = strLine;
        strLine = details1.IMDBNumber; DatabaseUtility.RemoveInvalidChars(ref strLine); details1.IMDBNumber = strLine;
        strLine = details1.MPARating; DatabaseUtility.RemoveInvalidChars(ref strLine); details1.MPARating = strLine;

        // add director
        int lDirector = -1;
        if (details.Director != Strings.Unknown)
        {
          lDirector = AddActor(details.Director);
          AddActorToMovie(lMovieId, lDirector);
        }
        // add all genres
        string szGenres = details.Genre;
        ArrayList vecGenres = new ArrayList();
        if (szGenres != Strings.Unknown)
        {
          if (szGenres.IndexOf("/") >= 0)
          {
            Tokens f = new Tokens(szGenres, new char[] { '/' });
            foreach (string strGenre in f)
            {
              strGenre.Trim();
              int lGenreId = AddGenre(strGenre);
              vecGenres.Add(lGenreId);
            }
          }
          else
          {
            string strGenre = details.Genre;
            strGenre.Trim();
            int lGenreId = AddGenre(strGenre);
            vecGenres.Add(lGenreId);
          }
        }
        // add cast...
        ArrayList vecActors = new ArrayList();
        if (details.Cast != Strings.Unknown)
        {
          char[] splitter = { '\n', ',' };
          string[] actors = details.Cast.Split(splitter);
          for (int i = 0; i < actors.Length; ++i)
          {
            int pos = actors[i].IndexOf(" as ");
            string actor = actors[i];
            if (pos >= 0)
            {
              actor = actors[i].Substring(0, pos);
            }
            actor = actor.Trim();
            int lActorId = AddActor(actor);
            vecActors.Add(lActorId);
          }
        }
        for (int i = 0; i < vecGenres.Count; ++i)
        {
          AddGenreToMovie(lMovieId, (int)vecGenres[i]);
        }

        for (int i = 0; i < vecActors.Count; i++)
        {
          AddActorToMovie(lMovieId, (int)vecActors[i]);
        }

        string strSQL;
        string strRating;
        strRating = String.Format("{0}", details1.Rating);
        if (strRating == "") strRating = "0.0";
        strSQL = String.Format("select * from movieinfo where idmovie={0}", lMovieId);
        //	Log.WriteFile(Log.LogType.Log,true,"dbs:{0}", strSQL);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("insert into movieinfo ( idMovie,idDirector,strPlotOutline,strPlot,strTagLine,strVotes,fRating,strCast,strCredits , iYear  ,strGenre, strPictureURL, strTitle,IMDBID,mpaa,runtime,iswatched) values({0},{1},'{2}','{3}','{4}','{5}','{6}','{7}','{8}',{9},'{10}','{11}','{12}','{13}','{14}',{15},{16})",
            lMovieId, lDirector, details1.PlotOutline,
            details1.Plot, details1.TagLine,
            details1.Votes, strRating,
            details1.Cast, details1.WritingCredits,

            details1.Year, details1.Genre,
            details1.ThumbURL, details1.Title,
            details1.IMDBNumber, details1.MPARating, details1.RunTime, details1.Watched);

          //			Log.WriteFile(Log.LogType.Log,true,"dbs:{0}", strSQL);
          m_db.Execute(strSQL);

        }
        else
        {
          strSQL = String.Format("update movieinfo set idDirector={0}, strPlotOutline='{1}', strPlot='{2}', strTagLine='{3}', strVotes='{4}', fRating='{5}', strCast='{6}',strCredits='{7}', iYear={8}, strGenre='{9}', strPictureURL='{10}', strTitle='{11}', IMDBID='{12}', mpaa='{13}', runtime={14}, iswatched={15} where idMovie={16}",
            lDirector, details1.PlotOutline,
            details1.Plot, details1.TagLine,
            details1.Votes, strRating,
            details1.Cast, details1.WritingCredits,
            details1.Year, details1.Genre,
            details1.ThumbURL, details1.Title,
            details1.IMDBNumber,
            details1.MPARating, details1.RunTime,
            details1.Watched, lMovieId);

          //		Log.WriteFile(Log.LogType.Log,true,"dbs:{0}", strSQL);
          m_db.Execute(strSQL);
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} src:{2}, stack:{1}", ex.Message, ex.Source, ex.StackTrace);
        Open();
      }
    }

    public void DeleteMovieInfo(string strFileNameAndPath)
    {
      int lMovieId = GetMovie(strFileNameAndPath, false);
      if (lMovieId < 0) return;
      DeleteMovieInfoById(lMovieId);
    }
    public void DeleteMovieInfoById(long lMovieId)
    {
      try
      {
        if (null == m_db) return;
        if (lMovieId == -1) return;
        Log.Write("Removing movie:{0}", lMovieId);
        string strSQL;
        strSQL = String.Format("delete from genrelinkmovie where idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from actorlinkmovie where idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from movieinfo where idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from files where idMovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from movie where idMovie={0}", lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public bool HasMovieInfo(string strFilenameAndPath)
    {
      if (strFilenameAndPath == null || strFilenameAndPath.Length == 0) return false;
      try
      {
        if (null == m_db) return false;
        int lMovieId = GetMovie(strFilenameAndPath, false);
        if (lMovieId < 0) return false;
        string strSQL;
        strSQL = String.Format("select * from movieinfo where movieinfo.idmovie={0}", lMovieId);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }

    public int GetMovieInfo(string strFilenameAndPath, ref IMDBMovie details)
    {
      int lMovieId = GetMovie(strFilenameAndPath, false);
      if (lMovieId < 0) return -1;

      if (!HasMovieInfo(strFilenameAndPath)) return -1;
      GetMovieInfoById(lMovieId, ref details);
      return lMovieId;
    }

    public void GetMovieInfoById(int lMovieId, ref IMDBMovie details)
    {
      try
      {
        string strSQL;
        strSQL = String.Format("select * from movieinfo,actors,movie,path where path.idpath=movie.idpath and movie.idMovie=movieinfo.idMovie and movieinfo.idmovie={0} and idDirector=idActor", lMovieId);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return;
        details.Rating = (float)System.Double.Parse(DatabaseUtility.Get(results, 0, "movieinfo.fRating"));
        if (details.Rating > 10.0f) details.Rating /= 10.0f;
        details.Director = DatabaseUtility.Get(results, 0, "actors.strActor");
        details.WritingCredits = DatabaseUtility.Get(results, 0, "movieinfo.strCredits");
        details.TagLine = DatabaseUtility.Get(results, 0, "movieinfo.strTagLine");
        details.PlotOutline = DatabaseUtility.Get(results, 0, "movieinfo.strPlotOutline");
        details.Plot = DatabaseUtility.Get(results, 0, "movieinfo.strPlot");
        details.Votes = DatabaseUtility.Get(results, 0, "movieinfo.strVotes");
        details.Cast = DatabaseUtility.Get(results, 0, "movieinfo.strCast");
        details.Year = System.Int32.Parse(DatabaseUtility.Get(results, 0, "movieinfo.iYear"));
        details.Genre = DatabaseUtility.Get(results, 0, "movieinfo.strGenre").Trim();
        details.ThumbURL = DatabaseUtility.Get(results, 0, "movieinfo.strPictureURL");
        details.Title = DatabaseUtility.Get(results, 0, "movieinfo.strTitle");
        details.Path = DatabaseUtility.Get(results, 0, "path.strPath");
        details.DVDLabel = DatabaseUtility.Get(results, 0, "movie.discid");
        details.IMDBNumber = DatabaseUtility.Get(results, 0, "movieinfo.IMDBID");
        lMovieId = System.Int32.Parse(DatabaseUtility.Get(results, 0, "movieinfo.idMovie"));
        details.SearchString = String.Format("{0}", details.Title);
        details.CDLabel = DatabaseUtility.Get(results, 0, "path.cdlabel");
        details.MPARating = DatabaseUtility.Get(results, 0, "movieinfo.mpaa");
        details.RunTime = System.Int32.Parse(DatabaseUtility.Get(results, 0, "movieinfo.runtime"));
        details.Watched = System.Int32.Parse(DatabaseUtility.Get(results, 0, "movieinfo.iswatched"));
        details.ID = (int)lMovieId;
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void SetWatched(IMDBMovie details)
    {
      try
      {
        if (null == m_db) return;
        if (details.ID < 0) return;
        string strSQL;
        strSQL = String.Format("update movieinfo set iswatched={0} where idMovie={1}", details.Watched, details.ID);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }


    #endregion

    #region Movie Resume
    public void DeleteMovieStopTime(int iFileId)
    {
      try
      {
        string sql = String.Format("delete from resume where idFile={0}", iFileId);
        m_db.Execute(sql);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public int GetMovieStopTime(int iFileId)
    {
      try
      {
        string sql = string.Format("select * from resume where idFile={0}", iFileId);
        SQLiteResultSet results;
        results = m_db.Execute(sql);
        if (results.Rows.Count == 0) return 0;
        int stoptime = Int32.Parse(DatabaseUtility.Get(results, 0, "stoptime"));
        return stoptime;

      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    public void SetMovieStopTime(int iFileId, int stoptime)
    {
      try
      {
        string sql = String.Format("select * from resume where idFile={0}", iFileId);
        SQLiteResultSet results;
        results = m_db.Execute(sql);
        if (results.Rows.Count == 0)
        {
          sql = String.Format("insert into resume ( idResume,idFile,stoptime) values(NULL,{0},{1})",
            iFileId, stoptime);
        }
        else
        {
          sql = String.Format("update resume set stoptime={0} where idFile={1}",
            stoptime, iFileId);
        }
        m_db.Execute(sql);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    byte[] FromHexString(string s)
    {
      byte[] bytes = new byte[s.Length / 2];
      for (int i = 0; i < bytes.Length; i++)
      {
        bytes[i] = byte.Parse(s.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
      }
      return bytes;
    }

    string ToHexString(byte[] bytes)
    {
      char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
      char[] chars = new char[bytes.Length * 2];
      for (int i = 0; i < bytes.Length; i++)
      {
        int b = bytes[i];
        chars[i * 2] = hexDigits[b >> 4];
        chars[i * 2 + 1] = hexDigits[b & 0xF];
      }
      return new string(chars);
    }

    public int GetMovieStopTimeAndResumeData(int iFileId, out byte[] resumeData)
    {
      resumeData = null;

      try
      {
        string sql = string.Format("select * from resume where idFile={0}", iFileId);
        SQLiteResultSet results;
        results = m_db.Execute(sql);
        if (results.Rows.Count == 0) return 0;
        int stoptime = Int32.Parse(DatabaseUtility.Get(results, 0, "stoptime"));
        string resumeString = DatabaseUtility.Get(results, 0, "resumeData");
        resumeData = new byte[resumeString.Length / 2];
        FromHexString(resumeString).CopyTo(resumeData, 0);
        return stoptime;

      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    public void SetMovieStopTimeAndResumeData(int iFileId, int stoptime, byte[] resumeData)
    {
      try
      {
        string sql = String.Format("select * from resume where idFile={0}", iFileId);
        SQLiteResultSet results;
        results = m_db.Execute(sql);

        string resumeString = "-";
        if (resumeData != null) resumeString = ToHexString(resumeData);
        if (results.Rows.Count == 0)
        {
          sql = String.Format("insert into resume ( idResume,idFile,stoptime,resumeData) values(NULL,{0},{1},'{2}')",
            iFileId, stoptime, resumeString);
        }
        else
        {
          sql = String.Format("update resume set stoptime={0},resumeData='{1}' where idFile={2}",
            stoptime, resumeString, iFileId);
        }
        m_db.Execute(sql);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public int GetMovieDuration(int iFileId)
    {
      try
      {
        string sql = string.Format("select * from duration where idFile={0}", iFileId);
        SQLiteResultSet results;
        results = m_db.Execute(sql);
        if (results.Rows.Count == 0) return 0;
        int duration = Int32.Parse(DatabaseUtility.Get(results, 0, "duration"));
        return duration;

      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    public void SetMovieDuration(int iFileId, int duration)
    {
      try
      {
        string sql = String.Format("select * from duration where idFile={0}", iFileId);
        SQLiteResultSet results;
        results = m_db.Execute(sql);
        if (results.Rows.Count == 0)
        {
          sql = String.Format("insert into duration ( idDuration,idFile,duration) values(NULL,{0},{1})",
            iFileId, duration);
        }
        else
        {
          sql = String.Format("update duration set duration={0} where idFile={1}",
            duration, iFileId);
        }
        m_db.Execute(sql);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    #endregion

    #region Movie
    public void DeleteMovie(string strFilenameAndPath)
    {
      try
      {
        int lPathId;
        int lMovieId;
        if (null == m_db) return;
        if (GetFile(strFilenameAndPath, out lPathId, out lMovieId, false) < 0)
        {
          return;
        }

        ClearBookMarksOfMovie(strFilenameAndPath);

        string strSQL;

        // Delete files attached to the movie
        strSQL = String.Format("select * from path,files where path.idPath=files.idPath and files.idmovie={0} order by strFilename", lMovieId);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          int iFileId = System.Int32.Parse(DatabaseUtility.Get(results, i, "files.idFile"));
          DeleteFile(iFileId);
        }

        strSQL = String.Format("delete from genrelinkmovie where idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from actorlinkmovie where idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from movieinfo where idmovie={0}", lMovieId);
        m_db.Execute(strSQL);

        strSQL = String.Format("delete from movie where idmovie={0}", lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public int AddMovie(string strFilenameAndPath, bool bHassubtitles)
    {
      if (m_db == null) return -1;
      try
      {
        if (null == m_db) return -1;
        string strPath, strFileName;

        DatabaseUtility.Split(strFilenameAndPath, out strPath, out strFileName);
        DatabaseUtility.RemoveInvalidChars(ref strPath);
        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        int lMovieId = GetMovie(strFilenameAndPath, false);
        if (lMovieId < 0)
        {
          string strSQL;

          int lPathId = AddPath(strPath);

          if (lPathId < 0) return -1;
          int iHasSubs = 0;
          if (bHassubtitles) iHasSubs = 1;
          strSQL = String.Format("insert into movie (idMovie, idPath, hasSubtitles, discid) values( NULL, {0}, {1},'')", lPathId, iHasSubs);

          m_db.Execute(strSQL);
          lMovieId = m_db.LastInsertID();
          AddFile(lMovieId, lPathId, strFileName);
        }
        else
        {
          int lPathId = GetPath(strPath);
          if (lPathId < 0) return -1;
          AddFile(lMovieId, lPathId, strFileName);
        }
        return lMovieId;
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    int GetMovie(string strFilenameAndPath, bool bExact)
    {
      int lPathId;
      int lMovieId;
      if (GetFile(strFilenameAndPath, out lPathId, out  lMovieId, bExact) < 0)
      {
        return -1;
      }
      return lMovieId;
    }

    public void GetMovies(ref ArrayList movies)
    {
      try
      {
        movies.Clear();
        if (null == m_db) return;
        string strSQL;
        strSQL = String.Format("select * from movie,movieinfo,actors,path where movieinfo.idmovie=movie.idmovie and movieinfo.iddirector=actors.idActor and movie.idpath=path.idpath");

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return;
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          details.Rating = (float)System.Double.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.fRating"));
          if (details.Rating > 10.0f) details.Rating /= 10.0f;
          details.Director = DatabaseUtility.Get(results, iRow, "actors.strActor");
          details.WritingCredits = DatabaseUtility.Get(results, iRow, "movieinfo.strCredits");
          details.TagLine = DatabaseUtility.Get(results, iRow, "movieinfo.strTagLine");
          details.PlotOutline = DatabaseUtility.Get(results, iRow, "movieinfo.strPlotOutline");
          details.Plot = DatabaseUtility.Get(results, iRow, "movieinfo.strPlot");
          details.Votes = DatabaseUtility.Get(results, iRow, "movieinfo.strVotes");
          details.Cast = DatabaseUtility.Get(results, iRow, "movieinfo.strCast");
          details.Year = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iYear"));
          details.Genre = DatabaseUtility.Get(results, iRow, "movieinfo.strGenre").Trim();
          details.ThumbURL = DatabaseUtility.Get(results, iRow, "movieinfo.strPictureURL");
          details.Title = DatabaseUtility.Get(results, iRow, "movieinfo.strTitle");
          details.Path = DatabaseUtility.Get(results, iRow, "path.strPath");
          details.DVDLabel = DatabaseUtility.Get(results, iRow, "movie.discid");
          details.IMDBNumber = DatabaseUtility.Get(results, iRow, "movieinfo.IMDBID");
          long lMovieId = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.idMovie"));
          details.SearchString = String.Format("{0}", details.Title);
          details.CDLabel = DatabaseUtility.Get(results, 0, "path.cdlabel");
          details.MPARating = DatabaseUtility.Get(results, 0, "movieinfo.mpaa");
          details.RunTime = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.runtime"));
          details.Watched = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iswatched"));
          details.ID = (int)lMovieId;
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public int GetMovieId(string strFilenameAndPath)
    {
      int lMovieId = GetMovie(strFilenameAndPath, true);
      return lMovieId;
    }
    public bool HasSubtitle(string strFilenameAndPath)
    {
      try
      {
        if (null == m_db) return false;
        int lMovieId = GetMovie(strFilenameAndPath, false);
        if (lMovieId < 0) return false;
        string strSQL;
        strSQL = String.Format("select * from movie where movie.idMovie={0}", lMovieId);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          return false;
        }
        int lHasSubs = System.Int32.Parse(DatabaseUtility.Get(results, 0, "hasSubtitles"));
        if (lHasSubs != 0) return true;
        return false;
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }


    public void SetThumbURL(int lMovieId, string thumbURL)
    {
      DatabaseUtility.RemoveInvalidChars(ref thumbURL);
      try
      {
        if (null == m_db) return;
        string strSQL;

        strSQL = String.Format("update movieinfo set strPictureURL='{0}' where idMovie={1}", thumbURL, lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void SetDVDLabel(int lMovieId, string strDVDLabel1)
    {
      string strDVDLabel = strDVDLabel1;
      DatabaseUtility.RemoveInvalidChars(ref strDVDLabel);
      try
      {
        if (null == m_db) return;
        string strSQL;

        strSQL = String.Format("update movie set discid='{0}' where idMovie={1}", strDVDLabel1, lMovieId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }


    #endregion

    #region Movie Queries
    public void GetYears(ArrayList years)
    {
      if (m_db == null) return;
      try
      {
        years.Clear();
        if (null == m_db) return;
        SQLiteResultSet results;
        results = m_db.Execute("select * from movieinfo");
        if (results.Rows.Count == 0) return;
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          string strYear = DatabaseUtility.Get(results, iRow, "iYear");
          bool bAdd = true;
          for (int i = 0; i < years.Count; ++i)
          {
            if (strYear == (string)years[i]) bAdd = false;
          }
          if (bAdd) years.Add(strYear);
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetMoviesByGenre(string strGenre1, ref ArrayList movies)
    {
      try
      {
        string strGenre = strGenre1;
        DatabaseUtility.RemoveInvalidChars(ref strGenre);

        movies.Clear();
        if (null == m_db) return;
        string strSQL;
        strSQL = String.Format("select * from genrelinkmovie,genre,movie,movieinfo,actors,path where path.idpath=movie.idpath and genrelinkmovie.idGenre=genre.idGenre and genrelinkmovie.idmovie=movie.idmovie and movieinfo.idmovie=movie.idmovie and genre.strGenre='{0}' and movieinfo.iddirector=actors.idActor", strGenre);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return;
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          details.Rating = (float)System.Double.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.fRating"));
          if (details.Rating > 10.0f) details.Rating /= 10.0f;
          details.Director = DatabaseUtility.Get(results, iRow, "actors.strActor");
          details.WritingCredits = DatabaseUtility.Get(results, iRow, "movieinfo.strCredits");
          details.TagLine = DatabaseUtility.Get(results, iRow, "movieinfo.strTagLine");
          details.PlotOutline = DatabaseUtility.Get(results, iRow, "movieinfo.strPlotOutline");
          details.Plot = DatabaseUtility.Get(results, iRow, "movieinfo.strPlot");
          details.Votes = DatabaseUtility.Get(results, iRow, "movieinfo.strVotes");
          details.Cast = DatabaseUtility.Get(results, iRow, "movieinfo.strCast");
          details.Year = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iYear"));
          details.Genre = DatabaseUtility.Get(results, iRow, "movieinfo.strGenre").Trim();
          details.ThumbURL = DatabaseUtility.Get(results, iRow, "movieinfo.strPictureURL");
          details.Title = DatabaseUtility.Get(results, iRow, "movieinfo.strTitle");
          details.Path = DatabaseUtility.Get(results, iRow, "path.strPath");
          details.DVDLabel = DatabaseUtility.Get(results, iRow, "movie.discid");
          details.IMDBNumber = DatabaseUtility.Get(results, iRow, "movieinfo.IMDBID");
          long lMovieId = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.idMovie"));
          details.SearchString = String.Format("{0}", details.Title);
          details.CDLabel = DatabaseUtility.Get(results, 0, "path.cdlabel");
          details.MPARating = DatabaseUtility.Get(results, 0, "movieinfo.mpaa");
          details.RunTime = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.runtime"));
          details.Watched = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iswatched"));
          details.ID = (int)lMovieId;
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetMoviesByActor(string strActor1, ref ArrayList movies)
    {
      try
      {
        string strActor = strActor1;
        DatabaseUtility.RemoveInvalidChars(ref strActor);

        movies.Clear();
        if (null == m_db) return;
        string strSQL;
        strSQL = String.Format("select * from actorlinkmovie,actors,movie,movieinfo,path where path.idpath=movie.idpath and actors.idActor=actorlinkmovie.idActor and actorlinkmovie.idmovie=movie.idmovie and movieinfo.idmovie=movie.idmovie and actors.stractor='{0}'", strActor);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return;
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          details.Rating = (float)System.Double.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.fRating"));
          if (details.Rating > 10.0f) details.Rating /= 10.0f;
          details.Director = DatabaseUtility.Get(results, iRow, "actors.strActor");
          details.WritingCredits = DatabaseUtility.Get(results, iRow, "movieinfo.strCredits");
          details.TagLine = DatabaseUtility.Get(results, iRow, "movieinfo.strTagLine");
          details.PlotOutline = DatabaseUtility.Get(results, iRow, "movieinfo.strPlotOutline");
          details.Plot = DatabaseUtility.Get(results, iRow, "movieinfo.strPlot");
          details.Votes = DatabaseUtility.Get(results, iRow, "movieinfo.strVotes");
          details.Cast = DatabaseUtility.Get(results, iRow, "movieinfo.strCast");
          details.Year = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iYear"));
          details.Genre = DatabaseUtility.Get(results, iRow, "movieinfo.strGenre").Trim();
          details.ThumbURL = DatabaseUtility.Get(results, iRow, "movieinfo.strPictureURL");
          details.Title = DatabaseUtility.Get(results, iRow, "movieinfo.strTitle");
          details.Path = DatabaseUtility.Get(results, iRow, "path.strPath");
          details.DVDLabel = DatabaseUtility.Get(results, iRow, "movie.discid");
          details.IMDBNumber = DatabaseUtility.Get(results, iRow, "movieinfo.IMDBID");
          long lMovieId = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.idMovie"));
          details.SearchString = String.Format("{0}", details.Title);
          details.CDLabel = DatabaseUtility.Get(results, 0, "path.cdlabel");
          details.MPARating = DatabaseUtility.Get(results, 0, "movieinfo.mpaa");
          details.RunTime = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.runtime"));
          details.Watched = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iswatched"));
          details.ID = (int)lMovieId;
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetMoviesByYear(string strYear, ref ArrayList movies)
    {
      try
      {
        int iYear = System.Int32.Parse(strYear);

        movies.Clear();
        if (null == m_db) return;
        string strSQL;
        strSQL = String.Format("select * from movie,movieinfo,actors,path where path.idpath=movie.idpath and movieinfo.idmovie=movie.idmovie and movieinfo.iddirector=actors.idActor and movieinfo.iYear={0}", iYear);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return;
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          details.Rating = (float)System.Double.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.fRating"));
          if (details.Rating > 10.0f) details.Rating /= 10.0f;
          details.Director = DatabaseUtility.Get(results, iRow, "actors.strActor");
          details.WritingCredits = DatabaseUtility.Get(results, iRow, "movieinfo.strCredits");
          details.TagLine = DatabaseUtility.Get(results, iRow, "movieinfo.strTagLine");
          details.PlotOutline = DatabaseUtility.Get(results, iRow, "movieinfo.strPlotOutline");
          details.Plot = DatabaseUtility.Get(results, iRow, "movieinfo.strPlot");
          details.Votes = DatabaseUtility.Get(results, iRow, "movieinfo.strVotes");
          details.Cast = DatabaseUtility.Get(results, iRow, "movieinfo.strCast");
          details.Year = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iYear"));
          details.Genre = DatabaseUtility.Get(results, iRow, "movieinfo.strGenre").Trim();
          details.ThumbURL = DatabaseUtility.Get(results, iRow, "movieinfo.strPictureURL");
          details.Title = DatabaseUtility.Get(results, iRow, "movieinfo.strTitle");
          details.Path = DatabaseUtility.Get(results, iRow, "path.strPath");
          details.DVDLabel = DatabaseUtility.Get(results, iRow, "movie.discid");
          details.IMDBNumber = DatabaseUtility.Get(results, iRow, "movieinfo.IMDBID");
          long lMovieId = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.idMovie"));
          details.SearchString = String.Format("{0}", details.Title);
          details.CDLabel = DatabaseUtility.Get(results, 0, "path.cdlabel");
          details.MPARating = DatabaseUtility.Get(results, 0, "movieinfo.mpaa");
          details.RunTime = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.runtime"));
          details.Watched = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "movieinfo.iswatched"));
          details.ID = (int)lMovieId;
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

    }

    public void GetMoviesByPath(string strPath1, ref ArrayList movies)
    {
      try
      {
        string strPath = strPath1;
        if (strPath.Length > 0)
        {
          if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
            strPath = strPath.Substring(0, strPath.Length - 1);
        }

        DatabaseUtility.RemoveInvalidChars(ref strPath);



        movies.Clear();
        if (null == m_db) return;
        int lPathId = GetPath(strPath);
        if (lPathId < 0) return;
        string strSQL;
        strSQL = String.Format("select * from files,movieinfo where files.idpath={0} and files.idMovie=movieinfo.idMovie", lPathId);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return;
        for (int iRow = 0; iRow < results.Rows.Count; iRow++)
        {
          IMDBMovie details = new IMDBMovie();
          long lMovieId = System.Int32.Parse(DatabaseUtility.Get(results, iRow, "files.idMovie"));
          details.SearchString = String.Format("{0}", details.Title);
          details.IMDBNumber = DatabaseUtility.Get(results, iRow, "movieinfo.IMDBID");
          details.Title = DatabaseUtility.Get(results, iRow, "movieinfo.strTitle");
          details.File = DatabaseUtility.Get(results, iRow, "files.strFilename");
          details.ID = (int)lMovieId;
          movies.Add(details);
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetMoviesByFilter(string sql, out ArrayList movies, bool actorTable, bool movieinfoTable, bool genreTable)
    {
      movies = new ArrayList();
      try
      {
        if (null == m_db) return;
        SQLiteResultSet results = GetResults(sql);
        IMDBMovie movie;

        for (int i = 0; i < results.Rows.Count; i++)
        {
          movie = new IMDBMovie();
          SQLiteResultSet.Row fields = results.Rows[i];
          if (actorTable && !movieinfoTable)
          {
            movie.Actor = fields.fields[1];
            movie.actorId = (int)Math.Floor(0.5d + Double.Parse(fields.fields[0]));
          }
          if (genreTable && !movieinfoTable)
          {
            movie.SingleGenre = fields.fields[1];
            movie.genreId = (int)Math.Floor(0.5d + Double.Parse(fields.fields[0]));
          }
          if (movieinfoTable)
          {
            movie.Rating = (float)System.Double.Parse(DatabaseUtility.Get(results, i, "movieinfo.fRating"));
            if (movie.Rating > 10.0f) movie.Rating /= 10.0f;
            movie.Director = DatabaseUtility.Get(results, i, "actors.strActor");
            movie.WritingCredits = DatabaseUtility.Get(results, i, "movieinfo.strCredits");
            movie.TagLine = DatabaseUtility.Get(results, i, "movieinfo.strTagLine");
            movie.PlotOutline = DatabaseUtility.Get(results, i, "movieinfo.strPlotOutline");
            movie.Plot = DatabaseUtility.Get(results, i, "movieinfo.strPlot");
            movie.Votes = DatabaseUtility.Get(results, i, "movieinfo.strVotes");
            movie.Cast = DatabaseUtility.Get(results, i, "movieinfo.strCast");
            movie.Year = System.Int32.Parse(DatabaseUtility.Get(results, i, "movieinfo.iYear"));
            movie.Genre = DatabaseUtility.Get(results, i, "movieinfo.strGenre").Trim();
            movie.ThumbURL = DatabaseUtility.Get(results, i, "movieinfo.strPictureURL");
            movie.Title = DatabaseUtility.Get(results, i, "movieinfo.strTitle");
            movie.Path = DatabaseUtility.Get(results, i, "path.strPath");
            movie.DVDLabel = DatabaseUtility.Get(results, i, "movie.discid");
            movie.IMDBNumber = DatabaseUtility.Get(results, i, "movieinfo.IMDBID");
            long lMovieId = System.Int32.Parse(DatabaseUtility.Get(results, i, "movieinfo.idMovie"));
            movie.SearchString = String.Format("{0}", movie.Title);
            movie.CDLabel = DatabaseUtility.Get(results, 0, "path.cdlabel");
            movie.MPARating = DatabaseUtility.Get(results, 0, "movieinfo.mpaa");
            movie.RunTime = System.Int32.Parse(DatabaseUtility.Get(results, i, "movieinfo.runtime"));
            movie.Watched = System.Int32.Parse(DatabaseUtility.Get(results, i, "movieinfo.iswatched"));
            movie.ID = (int)lMovieId;
          }
          movies.Add(movie);
        }

        return;
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return;
    }

    public void UpdateCDLabel(IMDBMovie movieDetails, string CDlabel)
    {
      if (movieDetails == null) return;
      SQLiteResultSet results;
      try
      {
        string sql = String.Format("select idPath from path where cdlabel = '{0}'", movieDetails.CDLabel);
        results = m_db.Execute(sql);
        int idPath = System.Int32.Parse(DatabaseUtility.Get(results, 0, "idPath"));
        sql = String.Format("update path set cdlabel = '{0}' where idPath = '{1}'", CDlabel, idPath);
        results = m_db.Execute(sql);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public string GetDVDLabel(string strFile)
    {
      string cdlabel = String.Empty;
      if (Utils.IsDVD(strFile))
      {
        cdlabel = Utils.GetDriveSerial(strFile);
      }
      return cdlabel;
    }

    public SQLiteResultSet GetResults(string sql)
    {
      try
      {
        if (null == m_db) return null;
        SQLiteResultSet results;
        results = m_db.Execute(sql);
        return results;
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return null;
    }

    #endregion

    #region ActorInfo
    public void SetActorInfo(int idActor, IMDBActor actor)
    {
      //"CREATE TABLE actorinfo ( idActor integer, dateofbirth text, placeofbirth text, minibio text, biography text
      try
      {
        if (null == m_db) return;
        string strSQL = String.Format("select * from actorinfo where idActor ={0}", idActor);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into actorinfo (idActor , dateofbirth , placeofbirth , minibio , biography ) values( {0},'{1}','{2}','{3}','{4}')",
            idActor, DatabaseUtility.FilterText(actor.DateOfBirth),
            DatabaseUtility.FilterText(actor.PlaceOfBirth),
            DatabaseUtility.FilterText(actor.MiniBiography),
            DatabaseUtility.FilterText(actor.Biography));
          m_db.Execute(strSQL);
        }
        else
        {
          // exists, modify it
          strSQL = String.Format("update actorinfo set dateofbirth='{1}', placeofbirth='{2}' , minibio='{3}' , biography='{4}' where idActor={0}",
            idActor, DatabaseUtility.FilterText(actor.DateOfBirth),
            DatabaseUtility.FilterText(actor.PlaceOfBirth),
            DatabaseUtility.FilterText(actor.MiniBiography),
            DatabaseUtility.FilterText(actor.Biography));
          m_db.Execute(strSQL);
          RemoveActorInfoMovie(idActor);
        }
        for (int i = 0; i < actor.Count; ++i)
        {
          AddActorInfoMovie(idActor, actor[i]);
        }
        return;

      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return;
    }
    public void AddActorInfoMovie(int idActor, IMDBActor.IMDBActorMovie movie)
    {
      //idActor, idDirector , strPlotOutline , strPlot , strTagLine , strVotes , fRating ,strCast ,strCredits , iYear , strGenre , strPictureURL , strTitle , IMDBID , mpaa ,runtime , iswatched , role 
      string movieTitle = DatabaseUtility.FilterText(movie.MovieTitle);
      try
      {
        if (null == m_db) return;
        string strSQL = String.Format("insert into actorinfomovies (idActor, idDirector , strPlotOutline , strPlot , strTagLine , strVotes , fRating ,strCast ,strCredits , iYear , strGenre , strPictureURL , strTitle , IMDBID , mpaa ,runtime , iswatched , role  ) values( {0} ,{1} ,'{2}' , '{3}' , '{4}' , '{5}' , '{6}' ,'{7}' ,'{8}' , {9} , '{10}' , '{11}' , '{12}' , '{13}' ,'{14}',{15} , {16} , '{17}' )",
                                idActor, -1, "-", "-", "-", "-", "-", "-", "-", movie.Year, "-", "-", movieTitle, "-", "-", -1, 0, DatabaseUtility.FilterText(movie.Role));
        m_db.Execute(strSQL);
        return;
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return;
    }
    public void RemoveActorInfoMovie(int actorId)
    {
      try
      {
        if (null == m_db) return;
        string strSQL = String.Format("delete from actorinfomovies where idActor={0}", actorId);
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public IMDBActor GetActorInfo(int idActor)
    {
      //"CREATE TABLE actorinfo ( idActor integer, dateofbirth text, placeofbirth text, minibio text, biography text
      try
      {
        if (null == m_db) return null;
        string strSQL = String.Format("select * from actors,actorinfo where actors.idActor=actorinfo.idActor and actors.idActor ={0}", idActor);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          IMDBActor actor = new IMDBActor();
          actor.Biography = DatabaseUtility.Get(results, 0, "actorinfo.biography");
          actor.DateOfBirth = DatabaseUtility.Get(results, 0, "actorinfo.dateofbirth");
          actor.MiniBiography = DatabaseUtility.Get(results, 0, "actorinfo.minibio");
          actor.Name = DatabaseUtility.Get(results, 0, "actors.strActor");
          actor.PlaceOfBirth = DatabaseUtility.Get(results, 0, "actorinfo.placeofbirth");

          strSQL = String.Format("select * from actorinfomovies where idActor ={0}", idActor);
          results = m_db.Execute(strSQL);
          for (int i = 0; i < results.Rows.Count; ++i)
          {
            IMDBActor.IMDBActorMovie movie = new IMDBActor.IMDBActorMovie();
            movie.MovieTitle = DatabaseUtility.Get(results, i, "strTitle");
            movie.Role = DatabaseUtility.Get(results, i, "role");
            movie.Year = Int32.Parse(DatabaseUtility.Get(results, i, "iYear"));
            actor.Add(movie);
          }
          return actor;
        }
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Log, true, "videodatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return null;
    }
    #endregion
  }
}
