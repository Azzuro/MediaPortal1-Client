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
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.Util;
using System.Collections.Generic;
using System.Collections;
using SQLite.NET;
using Core.Util;
using MediaPortal.Database;
using MediaPortal.TagReader;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using MediaPortal.Configuration;

namespace MediaPortal.Music.Database
{

  public delegate void MusicDBReorgEventHandler(object sender, DatabaseReorgEventArgs e);

  public class DatabaseReorgEventArgs : System.EventArgs
  {
    public int progress;
    // Provide one or more constructors, as well as fields and
    // accessors for the arguments.
    public string phase;
  }

  public class MusicDatabase
  {
    public class CArtistCache
    {
      public int idArtist = 0;
      public string strArtist = String.Empty;
    };

    public class CPathCache
    {
      public int idPath = 0;
      public string strPath = String.Empty;
    };

    public class CGenreCache
    {
      public int idGenre = 0;
      public string strGenre = String.Empty;
    };

    public class AlbumInfoCache : AlbumInfo
    {
      public int idAlbum = 0;
      public int idArtist = 0;
      public int idPath = -1;
    };


    public class ArtistInfoCache : ArtistInfo
    {
      public int idArtist = 0;
    }

    ArrayList _artistCache = new ArrayList();
    ArrayList _genreCache = new ArrayList();
    ArrayList _pathCache = new ArrayList();
    ArrayList _albumCache = new ArrayList();

    //ArrayList _pathids = new ArrayList();
    ArrayList _shares = new ArrayList();

    static bool _treatFolderAsAlbum = false;
    static bool _scanForVariousArtists = true;
    static bool _extractEmbededCoverArt = true;

    static bool _useFolderThumbs = true;
    static bool _useFolderArtForArtistGenre = false;
    static bool _createMissingFolderThumbs = false;

    static DateTime _lastImport = DateTime.Parse("1900-01-01 00:00:00");
    
    //bool AppendPrefixToSortableNameEnd = true;

    string[] ArtistNamePrefixes = new string[]
            {
              "the",
              "les"
            };


    // An event that clients can use to be notified whenever the
    // elements of the list change.
    public event MusicDBReorgEventHandler DatabaseReorgChanged;

    // Invoke the Changed event; called whenever list changes
    protected virtual void OnDatabaseReorgChanged(DatabaseReorgEventArgs e)
    {
      if (DatabaseReorgChanged != null)
        DatabaseReorgChanged(this, e);
    }

    enum Errors
    {
      ERROR_OK = 317,
      ERROR_CANCEL = 0,
      ERROR_DATABASE = 315,
      ERROR_REORG_SONGS = 319,
      ERROR_REORG_ARTIST = 321,
      ERROR_REORG_GENRE = 323,
      ERROR_REORG_PATH = 325,
      ERROR_REORG_ALBUM = 327,
      ERROR_WRITING_CHANGES = 329,
      ERROR_COMPRESSING = 332
    }

    static public SQLiteClient m_db = null;
    static System.DateTime currentDate;

    static MusicDatabase()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _treatFolderAsAlbum = xmlreader.GetValueAsBool("musicfiles", "treatFolderAsAlbum", false);
        _scanForVariousArtists = xmlreader.GetValueAsBool("musicfiles", "scanForVariousArtists", true);
        _extractEmbededCoverArt = xmlreader.GetValueAsBool("musicfiles", "extractthumbs", true);
        _useFolderThumbs = xmlreader.GetValueAsBool("musicfiles", "useFolderThumbs", true);
        _createMissingFolderThumbs = xmlreader.GetValueAsBool("musicfiles", "createMissingFolderThumbs", false);
        _useFolderArtForArtistGenre = xmlreader.GetValueAsBool("musicfiles", "createartistgenrethumbs", false);
        _lastImport = DateTime.Parse(xmlreader.GetValueAsString("musicfiles", "lastImport", "1900-01-01 00:00:00"));
      }
      Open();
    }

    static void Open()
    {
      Log.Info("Opening music database");
      try
      {
        // Open database
        try
        {
          System.IO.Directory.CreateDirectory(Config.GetFolder(Config.Dir.Database));
        }
        catch (Exception) { }

        // no database V7 - copy and update V6
        if (!File.Exists(Config.GetFile(Config.Dir.Database, "MusicDatabaseV7.db3")))
        {
          if (!File.Exists(Config.GetFile(Config.Dir.Database, "MusicDatabaseV6.db3")))
          {
            if (File.Exists(Config.GetFile(Config.Dir.Database, "musicdatabase5.db3")))
            {
              File.Copy((Config.GetFile(Config.Dir.Database, "musicdatabase5.db3")), (Config.GetFile(Config.Dir.Database, "MusicDatabaseV7.db3")), false);

            }
            else
              Log.Info("**** Please rescan your music shares ****");
          }
          else
          {
            if (UpdateDB_V6_to_V7())
            {
              Log.Info("MusicDatabaseV7: old V6 database successfully updated");
              File.Copy((Config.GetFile(Config.Dir.Database, "MusicDatabaseV6.db3")), (Config.GetFile(Config.Dir.Database, "MusicDatabaseV7.db3")), false);
            }
            else
              Log.Error("MusicDatabaseV6: error while trying to update your database to V7");
          }
        }

        m_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, "MusicDatabaseV7.db3"));

        DatabaseUtility.SetPragmas(m_db);

        DatabaseUtility.AddTable(m_db, "artist", "CREATE TABLE artist ( idArtist integer primary key, strArtist text, strSortName text)");
        DatabaseUtility.AddTable(m_db, "album", "CREATE TABLE album ( idAlbum integer primary key, idArtist integer, strAlbum text, iNumArtists integer)");

        //DatabaseUtility.AddTable(m_db, "album", "CREATE TABLE album ( idAlbum integer primary key, idArtist integer, strAlbum text)");
        DatabaseUtility.AddTable(m_db, "genre", "CREATE TABLE genre ( idGenre integer primary key, strGenre text)");
        DatabaseUtility.AddTable(m_db, "path", "CREATE TABLE path ( idPath integer primary key,  strPath text)");
        DatabaseUtility.AddTable(m_db, "albuminfo", "CREATE TABLE albuminfo ( idAlbumInfo integer primary key, idAlbum integer, idArtist integer,iYear integer, idGenre integer, strTones text, strStyles text, strReview text, strImage text, strTracks text, iRating integer)");
        DatabaseUtility.AddTable(m_db, "artistinfo", "CREATE TABLE artistinfo ( idArtistInfo integer primary key, idArtist integer, strBorn text, strYearsActive text, strGenres text, strTones text, strStyles text, strInstruments text, strImage text, strAMGBio text, strAlbums text, strCompilations text, strSingles text, strMisc text)");
        DatabaseUtility.AddTable(m_db, "song", "CREATE TABLE song ( idSong integer primary key, idArtist integer, idAlbum integer, idGenre integer, idPath integer, strTitle text, iTrack integer, iDuration integer, iYear integer, dwFileNameCRC text, strFileName text, iTimesPlayed integer, iRating integer, favorite integer)");

        DatabaseUtility.AddTable(m_db, "scrobbleusers", "CREATE TABLE scrobbleusers ( idScrobbleUser integer primary key, strUsername text, strPassword text)");
        DatabaseUtility.AddTable(m_db, "scrobblesettings", "CREATE TABLE scrobblesettings ( idScrobbleSettings integer primary key, idScrobbleUser integer, iAddArtists integer, iAddTracks integer, iNeighbourMode integer, iRandomness integer, iScrobbleDefault integer, iSubmitOn integer, iDebugLog integer, iOfflineMode integer, iPlaylistLimit integer, iPreferCount integer, iRememberStartArtist)");
        DatabaseUtility.AddTable(m_db, "scrobblemode", "CREATE TABLE scrobblemode ( idScrobbleMode integer primary key, idScrobbleUser integer, iSortID integer, strModeName text)");
        DatabaseUtility.AddTable(m_db, "scrobbletags", "CREATE TABLE scrobbletags ( idScrobbleTag integer primary key, idScrobbleMode integer, iSortID integer, strTagName text)");

        DatabaseUtility.AddIndex(m_db, "idxartist_strArtist", "CREATE UNIQUE INDEX idxartist_strArtist ON artist(strArtist ASC)");
        DatabaseUtility.AddIndex(m_db, "idxartist_strSortName", "CREATE INDEX idxartist_strSortName ON artist(strSortName ASC)");
        DatabaseUtility.AddIndex(m_db, "idxalbum_idArtist", "CREATE INDEX idxalbum_idArtist ON album(idArtist ASC)");
        DatabaseUtility.AddIndex(m_db, "idxalbum_strAlbum", "CREATE INDEX idxalbum_strAlbum ON album(strAlbum ASC)");
        DatabaseUtility.AddIndex(m_db, "idxgenre_strGenre", "CREATE UNIQUE INDEX idxgenre_strGenre ON genre(strGenre ASC)");
        DatabaseUtility.AddIndex(m_db, "idxpath_strPath", "CREATE UNIQUE INDEX idxpath_strPath ON path(strPath ASC)");
        DatabaseUtility.AddIndex(m_db, "idxalbuminfo_idAlbum", "CREATE INDEX idxalbuminfo_idAlbum ON albuminfo(idAlbum ASC)");
        DatabaseUtility.AddIndex(m_db, "idxalbuminfo_idArtist", "CREATE INDEX idxalbuminfo_idArtist ON albuminfo(idArtist ASC)");
        DatabaseUtility.AddIndex(m_db, "idxalbuminfo_idGenre", "CREATE INDEX idxalbuminfo_idGenre ON albuminfo(idGenre ASC)");
        DatabaseUtility.AddIndex(m_db, "idxartistinfo_idArtist", "CREATE INDEX idxartistinfo_idArtist ON artistinfo(idArtist ASC)");
        DatabaseUtility.AddIndex(m_db, "idxsong_idArtist", "CREATE INDEX idxsong_idArtist ON song(idArtist ASC)");
        DatabaseUtility.AddIndex(m_db, "idxsong_idAlbum", "CREATE INDEX idxsong_idAlbum ON song(idAlbum ASC)");
        DatabaseUtility.AddIndex(m_db, "idxsong_idGenre", "CREATE INDEX idxsong_idGenre ON song(idGenre ASC)");
        DatabaseUtility.AddIndex(m_db, "idxsong_idPath", "CREATE INDEX idxsong_idPath ON song(idPath ASC)");
        DatabaseUtility.AddIndex(m_db, "idxsong_strTitle", "CREATE INDEX idxsong_strTitle ON song(strTitle ASC)");
        DatabaseUtility.AddIndex(m_db, "idxsong_strFileName", "CREATE INDEX idxsong_strFileName ON song(strFileName ASC)");
        DatabaseUtility.AddIndex(m_db, "idxsong_dwFileNameCRC", "CREATE INDEX idxsong_dwFileNameCRC ON song(dwFileNameCRC ASC)");

      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      Log.Info("music database opened");
    }

    ~MusicDatabase()
    {
    }

    void temp()
    {

    }
    static public SQLiteClient DBHandle
    {
      get { return m_db; }
    }

    static bool UpdateDB_V6_to_V7()
    {
      bool success = true;
      m_db = new SQLiteClient(Config.GetFile(Config.Dir.Database, "MusicDatabaseV6.db3"));
      SQLiteResultSet results;

      string strSQL = "ALTER TABLE scrobblesettings ADD COLUMN iOfflineMode integer";
      results = m_db.Execute(strSQL);
      if (!DatabaseUtility.TableColumnExists(m_db, "scrobblesettings", "iOfflineMode"))
        success = false;

      strSQL = "ALTER TABLE scrobblesettings ADD COLUMN iPlaylistLimit integer";
      results = m_db.Execute(strSQL);
      if (!DatabaseUtility.TableColumnExists(m_db, "scrobblesettings", "iPlaylistLimit"))
        success = false;

      strSQL = "ALTER TABLE scrobblesettings ADD COLUMN iPreferCount integer";
      results = m_db.Execute(strSQL);
      if (!DatabaseUtility.TableColumnExists(m_db, "scrobblesettings", "iPreferCount"))
        success = false;

      strSQL = "ALTER TABLE scrobblesettings ADD COLUMN iRememberStartArtist integer";
      results = m_db.Execute(strSQL);
      if (!DatabaseUtility.TableColumnExists(m_db, "scrobblesettings", "iRememberStartArtist"))
        success = false;

      return success;
    }

    public int AddPath(string strPath1)
    {
      string strSQL;
      try
      {
        if (strPath1 == null)
          return -1;
        if (strPath1.Length == 0)
          return -1;
        string strPath = strPath1;
        //	musicdatabase always stores directories 
        //	without a slash at the end 
        if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
          strPath = strPath.Substring(0, strPath.Length - 1);
        DatabaseUtility.RemoveInvalidChars(ref strPath);

        if (null == m_db)
          return -1;

        foreach (CPathCache path in _pathCache)
          if (path.strPath == strPath1)
            return path.idPath;

        SQLiteResultSet results;
        strSQL = String.Format("select * from path where strPath like '{0}'", strPath);
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into path (idPath, strPath) values ( NULL, '{0}' )", strPath);
          m_db.Execute(strSQL);

          CPathCache path = new CPathCache();
          path.idPath = m_db.LastInsertID();
          path.strPath = strPath1;
          _pathCache.Add(path);
          return path.idPath;
        }
        else
        {
          CPathCache path = new CPathCache();
          path.idPath = DatabaseUtility.GetAsInt(results, 0, "idPath");
          path.strPath = strPath1;
          _pathCache.Add(path);
          return path.idPath;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public int AddArtist(string strArtist1)
    {
      string strSQL;
      try
      {
        string strArtist = strArtist1;
        DatabaseUtility.RemoveInvalidChars(ref strArtist);

        if (null == m_db)
          return -1;
        string name2 = strArtist1.ToLower().Trim();
        name2 = Regex.Replace(name2, @"[\W]*", string.Empty);
        foreach (CArtistCache artist in _artistCache)
        {
          string name1 = artist.strArtist.ToLower().Trim();
          name1 = Regex.Replace(name1, @"[\W]*", string.Empty);
          if (name1.Equals(name2))
            return artist.idArtist;
        }
        strSQL = String.Format("select * from artist where strArtist like '{0}'", strArtist);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into artist (idArtist, strArtist) values( NULL, '{0}' )", strArtist);
          m_db.Execute(strSQL);
          CArtistCache artist = new CArtistCache();
          artist.idArtist = m_db.LastInsertID();
          artist.strArtist = strArtist1;
          _artistCache.Add(artist);
          return artist.idArtist;
        }
        else
        {
          CArtistCache artist = new CArtistCache();
          artist.idArtist = DatabaseUtility.GetAsInt(results, 0, "idArtist");
          artist.strArtist = strArtist1;
          _artistCache.Add(artist);
          return artist.idArtist;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public int AddGenre(string strGenre1)
    {
      string strSQL;
      try
      {
        string strGenre = strGenre1;
        DatabaseUtility.RemoveInvalidChars(ref strGenre);
        if (String.Compare(strGenre.Substring(0, 1), "(") == 0)
        {
          /// We have the strange codes!
          /// Lets loop for the strange codes up to a of length X and delete them
          ///
          bool FixedTheCode = false;
          for (int i = 1; (i < 10 && i < strGenre.Length & !FixedTheCode); ++i)
          {
            if (String.Compare(strGenre.Substring(i, 1), ")") == 0)
            {
              ///Third position had the other end
              strGenre = strGenre.Substring(i + 1, (strGenre.Length - i - 1));
              FixedTheCode = true;
            }
          }
          //Log.Debug("Genre {0} changed to {1}", strGenre1, strGenre);
        }

        if (null == m_db)
          return -1;
        foreach (CGenreCache genre in _genreCache)
        {
          if (genre.strGenre == strGenre1)
            return genre.idGenre;
        }
        strSQL = String.Format("select * from genre where strGenre like '{0}'", strGenre);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into genre (idGenre, strGenre) values( NULL, '{0}' )", strGenre);
          m_db.Execute(strSQL);

          CGenreCache genre = new CGenreCache();
          genre.idGenre = m_db.LastInsertID();
          genre.strGenre = strGenre1;
          _genreCache.Add(genre);
          return genre.idGenre;
        }
        else
        {
          CGenreCache genre = new CGenreCache();
          genre.idGenre = DatabaseUtility.GetAsInt(results, 0, "idGenre");
          genre.strGenre = strGenre1;
          _genreCache.Add(genre);
          return genre.idGenre;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public void SetFavorite(Song song)
    {
      try
      {
        if (song.songId < 0)
          return;
        int iFavorite = 0;
        if (song.Favorite)
          iFavorite = 1;
        string strSQL = String.Format("update song set favorite={0} where idSong={1}", iFavorite, song.songId);
        m_db.Execute(strSQL);
        return;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void SetRating(string filename, int rating)
    {
      try
      {
        Song song = new Song();
        string strFileName = filename;
        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        string strPath, strFName;
        DatabaseUtility.Split(strFileName, out strPath, out strFName);
        if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
          strPath = strPath.Substring(0, strPath.Length - 1);

        if (null == m_db)
          return;

        string strSQL;
        ulong dwCRC;
        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
        dwCRC = crc.calc(filename);

        strSQL = String.Format("select * from song,path where song.idPath=path.idPath and dwFileNameCRC like '{0}' and strPath like '{1}'",
          dwCRC,
          strPath);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return;
        int idSong = DatabaseUtility.GetAsInt(results, 0, "song.idSong");

        strSQL = String.Format("update song set iRating={0} where idSong={1}",
          rating, idSong);
        m_db.Execute(strSQL);
        return;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return;
    }

    public SQLiteResultSet GetResults(string sql)
    {
      try
      {
        if (null == m_db)
          return null;
        SQLiteResultSet results;
        results = m_db.Execute(sql);
        return results;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return null;
    }

    public void GetSongsByFilter(string sql, out List<Song> songs, bool artistTable, bool albumTable, bool songTable, bool genreTable)
    {
      songs = new List<Song>();
      try
      {
        if (null == m_db)
          return;
        //Originele regel
        //SQLiteResultSet results=GetResults(sql);
        //Nieuwe regel
        SQLiteResultSet results = m_db.Execute(sql);

        MediaPortal.Music.Database.Song song;
        //Log.Write (sql);
        //Log.Write ("Aantal rijen = {0}",(int)results.Rows.Count);

        for (int i = 0; i < results.Rows.Count; i++)
        {
          song = new Song();
          SQLiteResultSet.Row fields = results.Rows[i];
          if (artistTable && !songTable)
          {
            song.Artist = fields.fields[1];
            song.artistId = DatabaseUtility.GetAsInt(results, i, "album.idArtist");
            //Log.Write ("artisttable and not songtable, artistid={0}",song.artistId);
          }
          if (albumTable && !songTable)
          {
            song.Album = fields.fields[2];
            song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
            song.artistId = DatabaseUtility.GetAsInt(results, i, "album.idArtist");

            //if (fields.fields.Count >= 5)
            //    song.Artist = fields.fields[4];
            if (fields.fields.Count >= 6)
              song.Artist = fields.fields[5];
          }
          if (genreTable && !songTable)
          {
            song.Genre = fields.fields[1];
            song.genreId = DatabaseUtility.GetAsInt(results, i, "song.idGenre");
            //Log.Write ("genretable and not songtable, genreid={0}",song.genreId);
          }
          if (songTable)
          {
            song.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
            song.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
            song.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
            song.artistId = DatabaseUtility.GetAsInt(results, i, "song.idArtist");
            song.Track = DatabaseUtility.GetAsInt(results, i, "song.iTrack");
            song.Duration = DatabaseUtility.GetAsInt(results, i, "song.iDuration");
            song.Year = DatabaseUtility.GetAsInt(results, i, "song.iYear");
            song.Title = DatabaseUtility.Get(results, i, "song.strTitle");
            song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
            song.Favorite = DatabaseUtility.GetAsInt(results, i, "song.favorite") != 0;
            song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");
            song.songId = DatabaseUtility.GetAsInt(results, i, "song.idSong");
            DatabaseUtility.GetAsInt(results, i, "song.idAlbum");
            DatabaseUtility.GetAsInt(results, i, "song.idGenre");
            string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
            strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
            song.FileName = strFileName;
            //Log.Write ("Song table with albumid={0}, artistid={1},songid={2}, strFilename={3}",song.albumId,song.artistId,song.songId,song.FileName);
          }
          songs.Add(song);
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void GetSongsByIndex(string sql, out List<Song> songs, int level, bool artistTable, bool albumTable, bool songTable, bool genreTable)
    {
      songs = new List<Song>();
      try
      {
        if (null == m_db)
          return;

        SQLiteResultSet results = m_db.Execute(sql);

        MediaPortal.Music.Database.Song song;

        int specialCharCount = 0;
        bool appendedSpecialChar = false;

        for (int i = 0; i < results.Rows.Count; i++)
        {
          SQLiteResultSet.Row fields = results.Rows[i];


          // Check for special characters to group them on Level 0 of a list
          if (level == 0)
          {
            char ch = ' ';
            if (fields.fields[0] != "")
            {
              ch = fields.fields[0][0];
            }
            bool founddSpecialChar = false;
            if (ch < 'A')
            {
              specialCharCount += Convert.ToInt16(fields.fields[1]);
              founddSpecialChar = true;
            }

            if (founddSpecialChar && i < results.Rows.Count - 1)
              continue;

            // Now we've looped through all Chars < A let's add the song
            if (!appendedSpecialChar)
            {
              appendedSpecialChar = true;
              if (specialCharCount > 0)
              {
                song = new Song();
                if (!songTable)
                {
                  song.Artist = "#";
                  song.Album = "#";
                  song.Genre = "#";
                }
                song.Title = "#";
                song.Duration = specialCharCount;
                songs.Add(song);
              }
            }
          }
          song = new Song();
          if (artistTable && !songTable)
          {
            song.Artist = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (albumTable && !songTable)
          {
            song.Album = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (genreTable && !songTable)
          {
            song.Genre = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          if (songTable)
          {
            song.Title = fields.fields[0];
            // Count of songs
            song.Duration = Convert.ToInt16(fields.fields[1]);
          }
          songs.Add(song);
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public int AddAlbum(string strAlbum1, int lArtistId)
    {
      return AddAlbum(strAlbum1, lArtistId, -1);
    }

    public int AddAlbum(string strAlbum1, int lArtistId, int lPathId)
    {
      string strSQL;
      try
      {
        string strAlbum = strAlbum1;
        DatabaseUtility.RemoveInvalidChars(ref strAlbum);

        if (null == m_db)
          return -1;
        string name2 = strAlbum.ToLower().Trim();
        name2 = Regex.Replace(name2, @"[\W]*", string.Empty);
        foreach (AlbumInfoCache album in _albumCache)
        {
          string name1 = album.Album.ToLower().Trim();
          name1 = Regex.Replace(name1, @"[\W]*", string.Empty);

          if (lPathId != -1)
          {
            if (name1.Equals(name2) && album.idPath == lPathId)
              return album.idAlbum;
          }
          else
          {
            if (name1.Equals(name2) && album.idArtist == lArtistId)
              return album.idAlbum;
          }
        }

        strSQL = String.Format("select * from album where strAlbum like '{0}'", strAlbum);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);

        if ((lPathId == -1 && lArtistId != DatabaseUtility.GetAsInt64(results, 0, "idArtist")) ||
            (lPathId != -1 && lPathId != DatabaseUtility.GetAsInt64(results, 0, "idPath")))
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into album (idAlbum, strAlbum,idArtist) values( NULL, '{0}', {1})", strAlbum, lArtistId);
          m_db.Execute(strSQL);

          AlbumInfoCache album = new AlbumInfoCache();
          album.idAlbum = m_db.LastInsertID();
          album.Album = strAlbum1;
          album.idArtist = lArtistId;
          album.idPath = lPathId;

          _albumCache.Add(album);
          return album.idAlbum;
        }
        else
        {
          AlbumInfoCache album = new AlbumInfoCache();
          album.idAlbum = DatabaseUtility.GetAsInt(results, 0, "idAlbum");
          album.Album = strAlbum1;
          album.idArtist = DatabaseUtility.GetAsInt(results, 0, "idArtist");
          album.idPath = lPathId;

          _albumCache.Add(album);
          return album.idAlbum;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public void EmptyCache()
    {
      _artistCache.Clear();
      _genreCache.Clear();
      _pathCache.Clear();
      _albumCache.Clear();
    }

    public bool IsOpen
    {
      get { return m_db != null; }
    }

    ///TFRO71 4 june 2005 
    ///This part is not used by the database class itself
    ///But the wizard_selectplugins somehow uses this part to add songs
    ///Weird right?
    public void AddSong(Song song1, bool bCheck)
    {
      //Log.Error("database.AddSong {0} {1} {2}  {3}", song1.FileName,song1.Album, song1.Artist, song1.Title);
      string strSQL;
      try
      {
        Song song = song1.Clone();
        string strTmp;
        //Log.Write ("MusicDatabaseReorg: Going to AddSong {0}",song.FileName);

        //        strTmp = song.Album; DatabaseUtility.RemoveInvalidChars(ref strTmp); song.Album = strTmp;
        //        strTmp = song.Genre; DatabaseUtility.RemoveInvalidChars(ref strTmp); song.Genre = strTmp;
        //        strTmp = song.Artist; DatabaseUtility.RemoveInvalidChars(ref strTmp); song.Artist = strTmp;
        strTmp = song.Title;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        song.Title = strTmp;

        // SourceForge Patch 1442438 (hwahrmann) Part 1 of 4
        //strTmp = song.FileName; DatabaseUtility.RemoveInvalidChars(ref strTmp); song.FileName = strTmp;
        // \1442438

        string strPath, strFileName;

        DatabaseUtility.Split(song.FileName, out strPath, out strFileName);

        // SourceForge Patch 1442438 (hwahrmann) Part 2 of 4
        DatabaseUtility.RemoveInvalidChars(ref strFileName);
        // \1442438

        if (null == m_db)
          return;
        int lGenreId = AddGenre(song.Genre);
        int lArtistId = AddArtist(song.Artist);
        int lPathId = AddPath(strPath);

        // SV
        //int lAlbumId = AddAlbum(song.Album, lArtistId);

        int lAlbumId = -1;

        if (_treatFolderAsAlbum)
          lAlbumId = AddAlbum(song.Album, lArtistId, lPathId);

        else
          lAlbumId = AddAlbum(song.Album, lArtistId);
        // \SV

        //Log.Write ("Getting a CRC for {0}",song.FileName);

        ulong dwCRC = 0;
        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
        dwCRC = crc.calc(song1.FileName);
        SQLiteResultSet results;

        //Log.Write ("MusicDatabaseReorg: CRC for {0} = {1}",song.FileName,dwCRC);
        if (bCheck)
        {
          strSQL = String.Format("select * from song where idAlbum={0} AND idGenre={1} AND idArtist={2} AND dwFileNameCRC like '{3}' AND strTitle='{4}'",
                                lAlbumId, lGenreId, lArtistId, dwCRC, song.Title);
          //Log.Write (strSQL);
          try
          {
            results = m_db.Execute(strSQL);

            song1.albumId = lAlbumId;
            song1.artistId = lArtistId;
            song1.genreId = lGenreId;

            if (results.Rows.Count != 0)
            {
              song1.songId = DatabaseUtility.GetAsInt(results, 0, "idSong");
              return;
            }
          }
          catch (Exception)
          {
            Log.Error("MusicDatabaseReorg: Executing query failed");
          }
        } //End if

        int iFavorite = 0;
        if (song.Favorite)
          iFavorite = 1;

        crc.Init(CRCTool.CRCCode.CRC32);
        dwCRC = crc.calc(song1.FileName);

        Log.Info("Song {0} will be added with CRC {1}", strFileName, dwCRC);

        strSQL = String.Format("insert into song (idSong,idArtist,idAlbum,idGenre,idPath,strTitle,iTrack,iDuration,iYear,dwFileNameCRC,strFileName,iTimesPlayed,iRating,favorite) values(NULL,{0},{1},{2},{3},'{4}',{5},{6},{7},'{8}','{9}',{10},{11},{12})",
                    lArtistId, lAlbumId, lGenreId, lPathId,
                    song.Title,
                    song.Track, song.Duration, song.Year,
                    dwCRC,
                    strFileName, 0, song.Rating, iFavorite);
        song1.songId = m_db.LastInsertID();


        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public void DeleteSong(string strFileName, bool bCheck)
    {
      try
      {
        int lGenreId = -1;
        int lArtistId = -1;
        int lPathId = -1;
        int lAlbumId = -1;
        int lSongId = -1;

        // SourceForge Patch 1442438 (hwahrmann) Part 3 of 4
        //DatabaseUtility.RemoveInvalidChars(ref strFileName);
        //string strPath, strFName;
        //DatabaseUtility.Split(strFileName, out strPath, out strFName);
        // \1442438

        if (null == m_db)
          return;

        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
        ulong dwCRC = crc.calc(strFileName);

        // SourceForge Patch 1442438 (hwahrmann) Part 4 of 4
        DatabaseUtility.RemoveInvalidChars(ref strFileName);
        string strPath, strFName;
        DatabaseUtility.Split(strFileName, out strPath, out strFName);
        // \1442438

        string strSQL;
        strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and dwFileNameCRC like '{0}' and strPath like '{1}'",
          dwCRC,
          strPath);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count > 0)
        {
          lArtistId = DatabaseUtility.GetAsInt(results, 0, "artist.idArtist");
          lAlbumId = DatabaseUtility.GetAsInt(results, 0, "album.idAlbum");
          lGenreId = DatabaseUtility.GetAsInt(results, 0, "genre.idGenre");
          lPathId = DatabaseUtility.GetAsInt(results, 0, "path.idPath");
          lSongId = DatabaseUtility.GetAsInt(results, 0, "song.idSong");

          // Delete
          strSQL = String.Format("delete from song where song.idSong={0}", lSongId);
          m_db.Execute(strSQL);

          if (bCheck)
          {
            // Check albums
            strSQL = String.Format("select * from song where song.idAlbum={0}", lAlbumId);
            results = m_db.Execute(strSQL);
            if (results.Rows.Count == 0)
            {
              // Delete album with no songs
              strSQL = String.Format("delete from album where idAlbum={0}", lAlbumId);
              m_db.Execute(strSQL);

              // Delete album info
              strSQL = String.Format("delete from albuminfo where idAlbum={0}", lAlbumId);
              m_db.Execute(strSQL);
            }

            // Check artists
            strSQL = String.Format("select * from song where song.idArtist={0}", lArtistId);
            results = m_db.Execute(strSQL);
            if (results.Rows.Count == 0)
            {
              // Delete artist with no songs
              strSQL = String.Format("delete from artist where idArtist={0}", lArtistId);
              m_db.Execute(strSQL);

              // Delete artist info
              strSQL = String.Format("delete from artistinfo where idArtist={0}", lArtistId);
              m_db.Execute(strSQL);
            }

            // Check path
            strSQL = String.Format("select * from song where song.idPath={0}", lPathId);
            results = m_db.Execute(strSQL);
            if (results.Rows.Count == 0)
            {
              // Delete path with no songs
              strSQL = String.Format("delete from path where idPath={0}", lPathId);
              m_db.Execute(strSQL);

              // remove from cache
              //foreach (CPathCache path in _pathCache)
              //{
              //    if (path.idPath == lPathId)
              //    {
              //        int iIndex = _pathCache.IndexOf(path);
              //        if (iIndex != -1)
              //        {
              //            _pathCache.RemoveAt(iIndex);
              //        }
              //    }
              //}

              // remove from cache 
              for (int i = 0; i < _pathCache.Count; i++)
              {
                CPathCache path = (CPathCache)_pathCache[i];
                if (path.idPath == lPathId)
                {
                  _pathCache.RemoveAt(i);
                  break;
                }
              }
            }

            // Check genre
            strSQL = String.Format("select * from song where song.idGenre={0}", lGenreId);
            results = m_db.Execute(strSQL);
            if (results.Rows.Count == 0)
            {
              // delete genre with no songs
              strSQL = String.Format("delete from genre where idGenre={0}", lGenreId);
              m_db.Execute(strSQL);
            }
          }
        }
        return;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return;
    }

    public bool GetSongByFileName(string strFileName1, ref Song song)
    {
      string strFileName = strFileName1;
      string strPath = String.Empty;
      string strFName = String.Empty;
      try
      {
        song.Clear();

        DatabaseUtility.RemoveInvalidChars(ref strFileName);
        DatabaseUtility.Split(strFileName, out strPath, out strFName);

        if (null == m_db)
          return false;

        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
        ulong dwCRC = crc.calc(strFileName1);

        string strSQL;
        //strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and dwFileNameCRC like '{0}' and strPath like '{1}'",
        //                      dwCRC,
        //                      strPath);

        strSQL = String.Format("SELECT * FROM song AS s INNER JOIN artist AS a ON s.idArtist = a.idArtist INNER JOIN album AS b ON s.idAlbum = b.idAlbum INNER JOIN genre AS g ON s.idGenre = g.idGenre INNER JOIN path AS p ON s.idPath = p.idPath  where dwFileNameCRC like '{0}' and strPath like '{1}'", dwCRC, strPath);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        song.Artist = DatabaseUtility.Get(results, 0, "artist.strArtist");
        song.Album = DatabaseUtility.Get(results, 0, "album.strAlbum");
        song.Genre = DatabaseUtility.Get(results, 0, "genre.strGenre");
        song.Track = DatabaseUtility.GetAsInt(results, 0, "song.iTrack");
        song.Duration = DatabaseUtility.GetAsInt(results, 0, "song.iDuration");
        song.Year = DatabaseUtility.GetAsInt(results, 0, "song.iYear");
        song.Title = DatabaseUtility.Get(results, 0, "song.strTitle");
        song.TimesPlayed = DatabaseUtility.GetAsInt(results, 0, "song.iTimesPlayed");
        song.Rating = DatabaseUtility.GetAsInt(results, 0, "song.iRating");
        song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, 0, "song.favorite"))) != 0;
        song.songId = DatabaseUtility.GetAsInt(results, 0, "song.idSong");
        song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, 0, "song.favorite"))) != 0;
        song.artistId = DatabaseUtility.GetAsInt(results, 0, "artist.idArtist");
        song.albumId = DatabaseUtility.GetAsInt(results, 0, "album.idAlbum");
        song.genreId = DatabaseUtility.GetAsInt(results, 0, "genre.idGenre");
        song.FileName = strFileName1;
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception for string: {2} / path: {3} - err:{0} stack:{1}", ex.Message, ex.StackTrace, strFileName1, strPath);
        Open();
      }

      return false;
    }

    public bool GetSong(string strTitle1, ref Song song)
    {
      try
      {
        song.Clear();
        string strTitle = strTitle1;
        DatabaseUtility.RemoveInvalidChars(ref strTitle);

        if (null == m_db)
          return false;

        string strSQL;
        strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle='{0}'", strTitle);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;

        song.Artist = DatabaseUtility.Get(results, 0, "artist.strArtist");
        song.Album = DatabaseUtility.Get(results, 0, "album.strAlbum");
        song.Genre = DatabaseUtility.Get(results, 0, "genre.strGenre");
        song.Track = DatabaseUtility.GetAsInt(results, 0, "song.iTrack");
        song.Duration = DatabaseUtility.GetAsInt(results, 0, "song.iDuration");
        song.Year = DatabaseUtility.GetAsInt(results, 0, "song.iYear");
        song.Title = DatabaseUtility.Get(results, 0, "song.strTitle");
        song.TimesPlayed = DatabaseUtility.GetAsInt(results, 0, "song.iTimesPlayed");
        song.Rating = DatabaseUtility.GetAsInt(results, 0, "song.iRating");
        song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, 0, "song.favorite"))) != 0;
        ;
        song.songId = DatabaseUtility.GetAsInt(results, 0, "song.idSong");
        song.artistId = DatabaseUtility.GetAsInt(results, 0, "artist.idArtist");
        song.albumId = DatabaseUtility.GetAsInt(results, 0, "album.idAlbum");
        song.genreId = DatabaseUtility.GetAsInt(results, 0, "genre.idGenre");
        string strFileName = DatabaseUtility.Get(results, 0, "path.strPath");
        strFileName += DatabaseUtility.Get(results, 0, "song.strFileName");
        song.FileName = strFileName;
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    //added by Sam
    public bool GetRandomSong(ref Song song)
    {
      try
      {
        song.Clear();

        if (null == m_db)
          return false;

        PRNG rand = new PRNG();
        string strSQL;
        int maxIDSong, rndIDSong;
        strSQL = String.Format("select * from song ORDER BY idSong DESC LIMIT 1");
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        maxIDSong = DatabaseUtility.GetAsInt(results, 0, "idSong");
        rndIDSong = rand.Next(0, maxIDSong);

        strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and idSong={0}", rndIDSong);

        results = m_db.Execute(strSQL);
        if (results.Rows.Count > 0)
        {
          song.Artist = DatabaseUtility.Get(results, 0, "artist.strArtist");
          song.Album = DatabaseUtility.Get(results, 0, "album.strAlbum");
          song.Genre = DatabaseUtility.Get(results, 0, "genre.strGenre");
          song.Track = DatabaseUtility.GetAsInt(results, 0, "song.iTrack");
          song.Duration = DatabaseUtility.GetAsInt(results, 0, "song.iDuration");
          song.Year = DatabaseUtility.GetAsInt(results, 0, "song.iYear");
          song.Title = DatabaseUtility.Get(results, 0, "song.strTitle");
          song.TimesPlayed = DatabaseUtility.GetAsInt(results, 0, "song.iTimesPlayed");
          song.Rating = DatabaseUtility.GetAsInt(results, 0, "song.iRating");
          song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, 0, "song.favorite"))) != 0;
          song.songId = DatabaseUtility.GetAsInt(results, 0, "song.idSong");
          song.artistId = DatabaseUtility.GetAsInt(results, 0, "artist.idArtist");
          song.albumId = DatabaseUtility.GetAsInt(results, 0, "album.idAlbum");
          song.genreId = DatabaseUtility.GetAsInt(results, 0, "genre.idGenre");
          string strFileName = DatabaseUtility.Get(results, 0, "path.strPath");
          strFileName += DatabaseUtility.Get(results, 0, "song.strFileName");
          song.FileName = strFileName;
          return true;
        }
        else
        {
          GetRandomSong(ref song);
          return true;
        }

      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    //added by Sam
    public int GetNumOfSongs()
    {
      try
      {
        if (null == m_db)
          return 0;

        string strSQL;
        int NumOfSongs;
        strSQL = String.Format("select count(*) from song");
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        SQLiteResultSet.Row row = results.Rows[0];
        NumOfSongs = Int32.Parse(row.fields[0]);
        return NumOfSongs;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    // added by rtv
    public int GetNumOfFavorites()
    {
      try
      {
        if (null == m_db)
          return 0;

        string strSQL;
        int NumOfSongs;
        strSQL = String.Format("select count(*) from song where favorite > 0");
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        SQLiteResultSet.Row row = results.Rows[0];
        NumOfSongs = Int32.Parse(row.fields[0]);
        return NumOfSongs;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    // added by rtv
    public double GetAVGPlayCountForArtist(string artist_)
    {
      try
      {
        if (null == m_db || artist_.Length == 0)
          return 0;

        string strSQL;
        string strArtist = artist_;
        double AVGPlayCount;

        DatabaseUtility.RemoveInvalidChars(ref strArtist);
        strSQL = String.Format("select avg(iTimesPlayed) from song where strfilename like '%{0}%'", strArtist);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        SQLiteResultSet.Row row = results.Rows[0];
        // needed for any other country with different decimal separator
        //AVGPlayCount = Double.Parse( row.fields[0], NumberStyles.Number, new CultureInfo("en-US"));
        Double.TryParse(row.fields[0], NumberStyles.Number, new CultureInfo("en-US"), out AVGPlayCount);
        return AVGPlayCount;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return 0;
    }

    //added by Sam
    public bool GetAllSongs(ref List<Song> songs)
    {
      try
      {
        if (null == m_db)
          return false;

        string strSQL;
        SQLiteResultSet results;
        strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist");

        results = m_db.Execute(strSQL);
        Song song;

        for (int i = 0; i < results.Rows.Count; i++)
        {
          song = new Song();
          song.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
          song.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          song.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
          song.Track = DatabaseUtility.GetAsInt(results, i, "song.iTrack");
          song.Duration = DatabaseUtility.GetAsInt(results, i, "song.iDuration");
          song.Year = DatabaseUtility.GetAsInt(results, i, "song.iYear");
          song.Title = DatabaseUtility.Get(results, i, "song.strTitle");
          song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
          song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");
          song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "song.favorite"))) != 0;
          song.songId = DatabaseUtility.GetAsInt(results, i, "song.idSong");
          song.artistId = DatabaseUtility.GetAsInt(results, i, "artist.idArtist");
          song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
          song.genreId = DatabaseUtility.GetAsInt(results, i, "genre.idGenre");
          string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
          strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
          song.FileName = strFileName;
          songs.Add(song);
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByArtist(string strArtist1, ref ArrayList songs)
    {
      try
      {
        string strArtist = strArtist1;
        DatabaseUtility.RemoveInvalidChars(ref strArtist);

        songs.Clear();
        if (null == m_db)
          return false;

        string strSQL;
        strSQL = String.Format("select song.idSong,artist.idArtist,album.idAlbum,genre.idGenre,song.favorite,song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName, song.iRating, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and artist.strArtist like '{0}'", strArtist);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          Song song = new Song();
          song.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
          song.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          song.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
          song.Track = DatabaseUtility.GetAsInt(results, i, "song.iTrack");
          song.Duration = DatabaseUtility.GetAsInt(results, i, "song.iDuration");
          song.Year = DatabaseUtility.GetAsInt(results, i, "song.iYear");
          song.Title = DatabaseUtility.Get(results, i, "song.strTitle");
          song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
          song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");
          song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "song.favorite"))) != 0;
          string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
          strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
          song.songId = DatabaseUtility.GetAsInt(results, i, "song.idSong");
          song.artistId = DatabaseUtility.GetAsInt(results, i, "artist.idArtist");
          song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
          song.genreId = DatabaseUtility.GetAsInt(results, i, "genre.idGenre");
          song.FileName = strFileName;
          songs.Add(song);
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByPathId(int nPathId, ref List<Song> songs)
    {
      songs.Clear();
      if (null == m_db)
        return false;

      string strSQL;
      strSQL = String.Format("select * from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and path.idPath='{0}'", nPathId);
      SQLiteResultSet results;
      results = m_db.Execute(strSQL);
      if (results.Rows.Count == 0)
        return false;
      for (int i = 0; i < results.Rows.Count; ++i)
      {
        Song song = new Song();
        song.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
        song.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
        song.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
        song.Track = DatabaseUtility.GetAsInt(results, i, "song.iTrack");
        song.Duration = DatabaseUtility.GetAsInt(results, i, "song.iDuration");
        song.Year = DatabaseUtility.GetAsInt(results, i, "song.iYear");
        song.Title = DatabaseUtility.Get(results, i, "song.strTitle");
        song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
        song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");
        song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "song.favorite"))) != 0;
        string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
        strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
        song.songId = DatabaseUtility.GetAsInt(results, i, "song.idSong");
        song.artistId = DatabaseUtility.GetAsInt(results, i, "artist.idArtist");
        song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
        song.genreId = DatabaseUtility.GetAsInt(results, i, "genre.idGenre");
        song.FileName = strFileName;
        songs.Add(song);
      }

      return true;
    }

    public bool GetSongsByArtist(int nArtistId, ref List<Song> songs)
    {
      try
      {
        songs.Clear();

        if (null == m_db)
          return false;

        string temp = "select distinct album.* from song,album,genre,artist,path where song.idPath=path.idPath";
        temp += " and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist";
        temp += " and song.idArtist='{0}'  order by strAlbum asc";

        string sql = string.Format(temp, nArtistId);
        GetSongsByFilter(sql, out songs, false, true, false, false);

        List<AlbumInfo> albums = new List<AlbumInfo>();
        GetAlbums(ref albums);

        foreach (Song song in songs)
        {
          foreach (AlbumInfo album in albums)
          {
            if (song.Album.Equals(album.Album))
            {
              song.Artist = album.Artist;
              break;
            }
          }
        }

        return true;
      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByArtistAlbum(int nArtistId, int nAlbumId, ref List<Song> songs)
    {
      try
      {
        songs.Clear();

        if (null == m_db)
          return false;

        string temp = "select * from song,album,genre,artist,path where song.idPath=path.idPath ";
        temp += "and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist  ";
        temp += "and song.idArtist='{0}' and  album.idAlbum='{1}'  order by iTrack asc";

        string sql = string.Format(temp, nArtistId, nAlbumId);
        GetSongsByFilter(sql, out songs, true, true, true, true);

        return true;
      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByGenre(int nGenreId, ref List<Song> songs)
    {
      try
      {
        songs.Clear();

        if (null == m_db)
          return false;

        string temp = "select * from song,album,genre,artist,path where song.idPath=path.idPath ";
        temp += "and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre ";
        temp += "and song.idArtist=artist.idArtist  and  genre.idGenre='{0}'  order by strTitle asc";
        string sql = string.Format(temp, nGenreId);
        GetSongsByFilter(sql, out songs, true, true, true, true);

        return true;
      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByYear(int nYear, ref List<Song> songs)
    {
      try
      {
        songs.Clear();

        if (null == m_db)
          return false;

        string temp = "select * from song,album,genre,artist,path where song.idPath=path.idPath ";
        temp += "and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and ";
        temp += "song.idArtist=artist.idArtist  and  song.iYear='{0}'  order by strTitle asc";

        string sql = string.Format(temp, nYear);
        GetSongsByFilter(sql, out songs, true, true, true, true);

        return true;
      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongs(int searchKind, string strTitle1, ref List<GUIListItem> songs)
    {
      try
      {
        songs.Clear();
        string strTitle = strTitle1;
        DatabaseUtility.RemoveInvalidChars(ref strTitle);
        if (null == m_db)
          return false;

        string strSQL = String.Empty;
        switch (searchKind)
        {
          case 0:
            strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle like '{0}%'", strTitle);
            break;
          case 1:
            strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle like '%{0}%'", strTitle);
            break;
          case 2:
            strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle like '%{0}'", strTitle);
            break;
          case 3:
            strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle like '{0}'", strTitle);
            break;
          default:
            return false;
        }

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
          strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
          GUIListItem item = new GUIListItem();
          item.IsFolder = false;
          item.Label = MediaPortal.Util.Utils.GetFilename(strFileName);
          item.Label2 = String.Empty;
          item.Label3 = String.Empty;
          item.Path = strFileName;
          item.FileInfo = new FileInformation(strFileName, item.IsFolder);
          MediaPortal.Util.Utils.SetDefaultIcons(item);
          MediaPortal.Util.Utils.SetThumbnails(ref item);
          songs.Add(item);
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: exception on song retrieval - {0}", ex.Message);
        Open();
      }
      return false;
    }

    public bool GetArtists(int searchKind, string strArtist1, ref ArrayList artists)
    {
      try
      {
        artists.Clear();
        string strArtist2 = strArtist1;
        if (null == m_db)
          return false;

        // Exclude "Various Artists"
        string strVariousArtists = GUILocalizeStrings.Get(340);
        long lVariousArtistId = AddArtist(strVariousArtists);
        string strSQL = String.Empty;
        switch (searchKind)
        {
          case 0:
            strSQL = String.Format("select * from artist where strArtist like '{0}%' ", strArtist2);
            break;
          case 1:
            strSQL = String.Format("select * from artist where strArtist like '%{0}%' ", strArtist2);
            break;
          case 2:
            strSQL = String.Format("select * from artist where strArtist like '%{0}' ", strArtist2);
            break;
          case 3:
            strSQL = String.Format("select * from artist where strArtist like '{0}' ", strArtist2);
            break;
          case 4:
            strArtist2.Replace('�', '%');
            strArtist2.Replace('�', '%');
            strArtist2.Replace('�', '%');
            strArtist2.Replace('/', '%');
            strArtist2.Replace('-', '%');
            strSQL = String.Format("select * from artist where strArtist like '%{0}%' ", strArtist2);
            break;
          default:
            return false;
        }

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          string strArtist = DatabaseUtility.Get(results, i, "strArtist");
          artists.Add(strArtist);
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetArtists(ref ArrayList artists)
    {
      try
      {
        artists.Clear();

        if (null == m_db)
          return false;


        // Exclude "Various Artists"
        string strVariousArtists = GUILocalizeStrings.Get(340);
        long lVariousArtistId = AddArtist(strVariousArtists);
        string strSQL;
        strSQL = String.Format("select * from artist where idArtist <> {0} ", lVariousArtistId);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          string strArtist = DatabaseUtility.Get(results, i, "strArtist");
          artists.Add(strArtist);
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetAlbums(ref List<AlbumInfo> albums)
    {
      try
      {
        albums.Clear();
        if (null == m_db)
          return false;

        string strSQL;
        strSQL = String.Format("select * from album,artist where album.idArtist=artist.idArtist");
        //strSQL=String.Format("select distinct album.idAlbum, album.idArtist, album.strAlbum, artist.idArtist, artist.strArtist from album,artist,song where song.idArtist=artist.idArtist and song.idAlbum=album.idAlbum");
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          AlbumInfo album = new AlbumInfo();
          album.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          album.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
          album.IdAlbum = DatabaseUtility.GetAsInt(results, i, "album.idArtist");
          albums.Add(album);
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }
    public bool GetAlbums(int searchKind, string strAlbum1, ref ArrayList albums)
    {
      try
      {
        string strAlbum = strAlbum1;
        albums.Clear();
        if (null == m_db)
          return false;

        string strSQL = String.Empty;
        switch (searchKind)
        {
          case 0:
            strSQL = String.Format("select * from album,artist where album.idArtist=artist.idArtist and album.strAlbum like '{0}%'", strAlbum);
            break;
          case 1:
            strSQL = String.Format("select * from album,artist where album.idArtist=artist.idArtist and album.strAlbum like '%{0}%'", strAlbum);
            break;
          case 2:
            strSQL = String.Format("select * from album,artist where album.idArtist=artist.idArtist and album.strAlbum like '%{0}'", strAlbum);
            break;
          case 3:
            strSQL = String.Format("select * from album,artist where album.idArtist=artist.idArtist and album.strAlbum like '{0}'", strAlbum);
            break;
          default:
            return false;
        }
        //strSQL=String.Format("select distinct album.idAlbum, album.idArtist, album.strAlbum, artist.idArtist, artist.strArtist from album,artist,song where song.idArtist=artist.idArtist and song.idAlbum=album.idAlbum");
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          AlbumInfo album = new AlbumInfo();
          album.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          album.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
          album.IdAlbum = DatabaseUtility.GetAsInt(results, i, "album.idArtist");
          albums.Add(album);
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetVariousArtistsAlbums(ref List<AlbumInfo> albums)
    {
      try
      {
        albums.Clear();
        if (null == m_db)
          return false;

        string variousArtists = GUILocalizeStrings.Get(340);

        if (variousArtists.Length == 0)
          variousArtists = "Various Artists";

        long idVariousArtists = AddArtist(variousArtists);

        string strSQL;
        strSQL = String.Format("select * from album where album.idArtist='{0}'", idVariousArtists);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          AlbumInfo album = new AlbumInfo();
          album.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          album.IdAlbum = DatabaseUtility.GetAsInt(results, i, "album.idArtist");
          album.Artist = variousArtists;
          albums.Add(album);
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetGenres(ref ArrayList genres)
    {
      try
      {
        genres.Clear();
        if (null == m_db)
          return false;
        string strSQL;
        strSQL = String.Format("select * from genre");
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          string strGenre = DatabaseUtility.Get(results, i, "strGenre");
          genres.Add(strGenre);
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }

    public bool GetGenres(int searchKind, string strGenere1, ref ArrayList genres)
    {
      try
      {
        genres.Clear();
        string strGenere = strGenere1;
        if (null == m_db)
          return false;
        string strSQL = String.Empty;
        switch (searchKind)
        {
          case 0:
            strSQL = String.Format("select * from genre where strGenre like '{0}%'", strGenere);
            break;
          case 1:
            strSQL = String.Format("select * from genre where strGenre like '%{0}%'", strGenere);
            break;
          case 2:
            strSQL = String.Format("select * from genre where strGenre like '%{0}'", strGenere);
            break;
          case 3:
            strSQL = String.Format("select * from genre where strGenre like '{0}'", strGenere);
            break;
          default:
            return false;
        }
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          string strGenre = DatabaseUtility.Get(results, i, "strGenre");
          genres.Add(strGenre);
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }

    public bool GetSongsByPath(string strPath1, ref ArrayList songs)
    {
      try
      {
        songs.Clear();
        if (strPath1 == null)
          return false;
        if (strPath1.Length == 0)
          return false;
        string strPath = strPath1;
        //	musicdatabase always stores directories 
        //	without a slash at the end 
        if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
          strPath = strPath.Substring(0, strPath.Length - 1);
        DatabaseUtility.RemoveInvalidChars(ref strPath);
        if (null == m_db)
          return false;

        string strSQL;
        strSQL = String.Format("select * from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and path.strPath like '{0}'", strPath);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          Song song = new Song();
          song.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
          song.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          song.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
          song.Track = DatabaseUtility.GetAsInt(results, i, "song.iTrack");
          song.Duration = DatabaseUtility.GetAsInt(results, i, "song.iDuration");
          song.Year = DatabaseUtility.GetAsInt(results, i, "song.iYear");
          song.Title = DatabaseUtility.Get(results, i, "song.strTitle");
          song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
          song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");
          song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "song.favorite"))) != 0;
          song.songId = DatabaseUtility.GetAsInt(results, i, "song.idSong");
          song.artistId = DatabaseUtility.GetAsInt(results, i, "artist.idArtist");
          song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
          song.genreId = DatabaseUtility.GetAsInt(results, i, "genre.idGenre");
          string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
          strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
          song.FileName = strFileName;

          songs.Add(song);
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }

    public bool GetRecentlyPlayedAlbums(ref ArrayList albums)
    {
      try
      {
        albums.Clear();
        if (null == m_db)
          return false;

        string strSQL;
        strSQL = String.Format("select distinct album.*, artist.*, path.* from album,artist,path,song where album.idAlbum=song.idAlbum and album.idArtist=artist.idArtist and song.idPath=path.idPath and song.iTimesPlayed > 0 order by song.iTimesPlayed limit 20");
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          AlbumInfo album = new AlbumInfo();
          album.Album = DatabaseUtility.Get(results, 0, "album.strAlbum");
          album.Artist = DatabaseUtility.Get(results, 0, "artist.strArtist");
          albums.Add(album);
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }

    public string GetAlbumPath(int nArtistId, int nAlbumId)
    {
      try
      {
        if (null == m_db)
          return string.Empty;

        //string sql = string.Format("select * from song, path where song.idPath=path.idPath and song.idArtist='{0}' and  song.idAlbum='{1}'  limit 1", nArtistId, nAlbumId);
        string sql = string.Format("select path.strPath from song,path,album where song.idPath=path.idPath and album.idAlbum=song.idAlbum and album.idArtist='{0}' and  album.idAlbum='{1}'  limit 1", nArtistId, nAlbumId);
        SQLiteResultSet results = m_db.Execute(sql);

        if (results.Rows.Count > 0)
        {
          string sPath = DatabaseUtility.Get(results, 0, "path.strPath");
          sPath += DatabaseUtility.Get(results, 0, "song.strFileName");

          return sPath;
        }
      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return string.Empty;
    }

    public void ResetTop100()
    {
      try
      {
        string strSQL = String.Format("update song set iTimesPlayed=0");
        m_db.Execute(strSQL);
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
    }

    public bool IncrTop100CounterByFileName(string strFileName1)
    {
      try
      {
        Song song = new Song();
        string strFileName = strFileName1;
        DatabaseUtility.RemoveInvalidChars(ref strFileName);

        string strPath, strFName;
        DatabaseUtility.Split(strFileName, out strPath, out strFName);
        if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
          strPath = strPath.Substring(0, strPath.Length - 1);

        if (null == m_db)
          return false;

        string strSQL;
        ulong dwCRC;
        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
        dwCRC = crc.calc(strFileName1);

        strSQL = String.Format("select * from song,path where song.idPath=path.idPath and dwFileNameCRC like '{0}' and strPath like '{1}'",
                            dwCRC,
                            strPath);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        int idSong = DatabaseUtility.GetAsInt(results, 0, "song.idSong");
        int iTimesPlayed = DatabaseUtility.GetAsInt(results, 0, "song.iTimesPlayed");

        strSQL = String.Format("update song set iTimesPlayed={0} where idSong={1}",
                              ++iTimesPlayed, idSong);
        m_db.Execute(strSQL);
        Log.Debug("MusicDatabase: increased playcount for song {1} to {0}", Convert.ToString(iTimesPlayed), strFileName1);
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public int AddAlbumInfo(AlbumInfo album1)
    {
      string strSQL;
      try
      {
        AlbumInfo album = album1.Clone();
        string strTmp;
        //				strTmp = album.Album; DatabaseUtility.RemoveInvalidChars(ref strTmp); album.Album = strTmp;
        //				strTmp = album.Genre; DatabaseUtility.RemoveInvalidChars(ref strTmp); album.Genre = strTmp;
        //				strTmp = album.Artist; DatabaseUtility.RemoveInvalidChars(ref strTmp); album.Artist = strTmp;
        strTmp = album.Tones;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Tones = strTmp;
        strTmp = album.Styles;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Styles = strTmp;
        strTmp = album.Review;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Review = strTmp;
        strTmp = album.Image;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Image = strTmp;
        strTmp = album.Tracks;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        album.Tracks = strTmp;
        //strTmp=album.Path  ;RemoveInvalidChars(ref strTmp);album.Path=strTmp;

        if (null == m_db)
          return -1;
        int lGenreId = AddGenre(album1.Genre);
        //int lPathId   = AddPath(album1.Path);
        int lArtistId = AddArtist(album1.Artist);
        int lAlbumId = AddAlbum(album1.Album, lArtistId);

        strSQL = String.Format("delete  from albuminfo where idAlbum={0} ", lAlbumId);
        m_db.Execute(strSQL);

        strSQL = String.Format("insert into albuminfo (idAlbumInfo,idAlbum,idArtist,idGenre,strTones,strStyles,strReview,strImage,iRating,iYear,strTracks) values(NULL,{0},{1},{2},'{3}','{4}','{5}','{6}',{7},{8},'{9}' )",
                            lAlbumId, lArtistId, lGenreId,
                            album.Tones,
                            album.Styles,
                            album.Review,
                            album.Image,
                            album.Rating,
                            album.Year,
                            album.Tracks);
        m_db.Execute(strSQL);

        int lAlbumInfoId = m_db.LastInsertID();
        return lAlbumInfoId;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return -1;

    }
    public void DeleteAlbumInfo(string strAlbumName1)
    {
      string strAlbum = strAlbumName1;
      DatabaseUtility.RemoveInvalidChars(ref strAlbum);
      string strSQL = String.Format("select * from albuminfo,album where albuminfo.idAlbum=album.idAlbum and album.strAlbum like '{0}'", strAlbum);
      SQLiteResultSet results;
      results = m_db.Execute(strSQL);
      if (results.Rows.Count != 0)
      {
        int iAlbumId = DatabaseUtility.GetAsInt(results, 0, "albuminfo.idAlbum");
        strSQL = String.Format("delete from albuminfo where albuminfo.idAlbum={0}", iAlbumId);
        m_db.Execute(strSQL);
      }
    }

    public bool GetAlbumInfo(string strAlbum1, string strPath1, ref AlbumInfo album)
    {
      try
      {
        if (strPath1 == null)
          return false;
        if (strPath1.Length == 0)
          return false;
        string strAlbum = strAlbum1;
        string strPath = strPath1;
        //	musicdatabase always stores directories 
        //	without a slash at the end 
        if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
          strPath = strPath.Substring(0, strPath.Length - 1);
        DatabaseUtility.RemoveInvalidChars(ref strAlbum);
        DatabaseUtility.RemoveInvalidChars(ref strPath);
        string strSQL;
        strSQL = String.Format("select * from albuminfo,album,genre,artist where albuminfo.idAlbum=album.idAlbum and albuminfo.idGenre=genre.idGenre and albuminfo.idArtist=artist.idArtist and album.strAlbum like '{0}'", strAlbum);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          album.Rating = DatabaseUtility.GetAsInt(results, 0, "albuminfo.iRating");
          album.Year = DatabaseUtility.GetAsInt(results, 0, "albuminfo.iYear");
          album.Album = DatabaseUtility.Get(results, 0, "album.strAlbum");
          album.Artist = DatabaseUtility.Get(results, 0, "artist.strArtist");
          album.Genre = DatabaseUtility.Get(results, 0, "genre.strGenre");
          album.Image = DatabaseUtility.Get(results, 0, "albuminfo.strImage");
          album.Review = DatabaseUtility.Get(results, 0, "albuminfo.strReview");
          album.Styles = DatabaseUtility.Get(results, 0, "albuminfo.strStyles");
          album.Tones = DatabaseUtility.Get(results, 0, "albuminfo.strTones");
          album.Tracks = DatabaseUtility.Get(results, 0, "albuminfo.strTracks");
          //album.Path   = DatabaseUtility.Get(results,0,"path.strPath");
          return true;
        }
        return false;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }
    public bool GetAlbumInfo(int albumId, ref AlbumInfo album)
    {
      try
      {
        string strSQL;
        strSQL = String.Format("select * from albuminfo,album,genre,artist where albuminfo.idAlbum=album.idAlbum and albuminfo.idGenre=genre.idGenre and albuminfo.idArtist=artist.idArtist and album.idAlbum ={0}", albumId);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          album.Rating = DatabaseUtility.GetAsInt(results, 0, "albuminfo.iRating");
          album.Year = DatabaseUtility.GetAsInt(results, 0, "albuminfo.iYear");
          album.Album = DatabaseUtility.Get(results, 0, "album.strAlbum");
          album.Artist = DatabaseUtility.Get(results, 0, "artist.strArtist");
          album.Genre = DatabaseUtility.Get(results, 0, "genre.strGenre");
          album.Image = DatabaseUtility.Get(results, 0, "albuminfo.strImage");
          album.Review = DatabaseUtility.Get(results, 0, "albuminfo.strReview");
          album.Styles = DatabaseUtility.Get(results, 0, "albuminfo.strStyles");
          album.Tones = DatabaseUtility.Get(results, 0, "albuminfo.strTones");
          album.Tracks = DatabaseUtility.Get(results, 0, "albuminfo.strTracks");
          //album.Path   = DatabaseUtility.Get(results,0,"path.strPath");
          return true;
        }
        return false;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public int AddArtistInfo(ArtistInfo artist1)
    {
      string strSQL;
      try
      {
        ArtistInfo artist = artist1.Clone();
        string strTmp;
        //strTmp = artist.Artist; DatabaseUtility.RemoveInvalidChars(ref strTmp); artist.Artist = strTmp;
        strTmp = artist.Born;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Born = strTmp;
        strTmp = artist.YearsActive;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.YearsActive = strTmp;
        strTmp = artist.Genres;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Genres = strTmp;
        strTmp = artist.Instruments;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Instruments = strTmp;
        strTmp = artist.Tones;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Tones = strTmp;
        strTmp = artist.Styles;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Styles = strTmp;
        strTmp = artist.AMGBio;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.AMGBio = strTmp;
        strTmp = artist.Image;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Image = strTmp;
        strTmp = artist.Albums;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Albums = strTmp;
        strTmp = artist.Compilations;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Compilations = strTmp;
        strTmp = artist.Singles;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Singles = strTmp;
        strTmp = artist.Misc;
        DatabaseUtility.RemoveInvalidChars(ref strTmp);
        artist.Misc = strTmp;

        if (null == m_db)
          return -1;
        int lArtistId = AddArtist(artist.Artist);

        //strSQL=String.Format("delete artistinfo where idArtist={0} ", lArtistId);
        //m_db.Execute(strSQL);
        strSQL = String.Format("select * from artistinfo where idArtist={0}", lArtistId);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          strSQL = String.Format("delete artistinfo where idArtist={0} ", lArtistId);
          m_db.Execute(strSQL);
        }

        strSQL = String.Format("insert into artistinfo (idArtistInfo,idArtist,strBorn,strYearsActive,strGenres,strTones,strStyles,strInstruments,strImage,strAMGBio, strAlbums,strCompilations,strSingles,strMisc) values(NULL,{0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}' )",
          lArtistId,
          artist.Born,
          artist.YearsActive,
          artist.Genres,
          artist.Tones,
          artist.Styles,
          artist.Instruments,
          artist.Image,
          artist.AMGBio,
          artist.Albums,
          artist.Compilations,
          artist.Singles,
          artist.Misc);
        m_db.Execute(strSQL);

        int lArtistInfoId = m_db.LastInsertID();
        return lArtistInfoId;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return -1;

    }
    public void DeleteArtistInfo(string strArtistName1)
    {
      string strArtist = strArtistName1;
      DatabaseUtility.RemoveInvalidChars(ref strArtist);
      string strSQL = String.Format("select * from artist where artist.strArtist like '{0}'", strArtist);
      SQLiteResultSet results;
      results = m_db.Execute(strSQL);
      if (results.Rows.Count != 0)
      {
        int iArtistId = DatabaseUtility.GetAsInt(results, 0, "idArtist");
        strSQL = String.Format("delete from artistinfo where artistinfo.idArtist={0}", iArtistId);
        m_db.Execute(strSQL);
      }
    }

    public bool GetArtistInfo(string strArtist1, ref ArtistInfo artist)
    {
      try
      {
        string strArtist = strArtist1;
        DatabaseUtility.RemoveInvalidChars(ref strArtist);
        string strSQL;
        strSQL = String.Format("select * from artist,artistinfo where artist.idArtist=artistinfo.idArtist and artist.strArtist like '{0}'", strArtist);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count != 0)
        {
          artist.Artist = DatabaseUtility.Get(results, 0, "artist.strArtist");
          artist.Born = DatabaseUtility.Get(results, 0, "artistinfo.strBorn");
          artist.YearsActive = DatabaseUtility.Get(results, 0, "artistinfo.strYearsActive");
          artist.Genres = DatabaseUtility.Get(results, 0, "artistinfo.strGenres");
          artist.Styles = DatabaseUtility.Get(results, 0, "artistinfo.strStyles");
          artist.Tones = DatabaseUtility.Get(results, 0, "artistinfo.strTones");
          artist.Instruments = DatabaseUtility.Get(results, 0, "artistinfo.strInstruments");
          artist.Image = DatabaseUtility.Get(results, 0, "artistinfo.strImage");
          artist.AMGBio = DatabaseUtility.Get(results, 0, "artistinfo.strAMGBio");
          artist.Albums = DatabaseUtility.Get(results, 0, "artistinfo.strAlbums");
          artist.Compilations = DatabaseUtility.Get(results, 0, "artistinfo.strCompilations");
          artist.Singles = DatabaseUtility.Get(results, 0, "artistinfo.strSingles");
          artist.Misc = DatabaseUtility.Get(results, 0, "artistinfo.strMisc");
          return true;
        }
        return false;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public bool GetSongsByPath2(string strPath1, ref List<SongMap> songs)
    {
      //Log.Write ("GetSongsByPath2 {0} ",strPath1);

      string strSQL = String.Empty;
      try
      {
        songs.Clear();
        if (strPath1 == null)
          return false;
        if (strPath1.Length == 0)
          return false;
        string strPath = strPath1;
        //	musicdatabase always stores directories 
        //	without a slash at the end 
        if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
          strPath = strPath.Substring(0, strPath.Length - 1);
        DatabaseUtility.RemoveInvalidChars(ref strPath);
        if (null == m_db)
          return false;

        strSQL = String.Format("select song.idSong,artist.idArtist,album.idAlbum,genre.idGenre,song.favorite,song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName,song.iRating, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and path.strPath like '{0}'", strPath);
        //strSQL = String.Format("select song.idSong,artist.idArtist,album.idAlbum,genre.idGenre,song.favorite,song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName,song.iRating, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and path.strPath like '{0}' order by song.iTrack asc", strPath);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          SongMap songmap = new SongMap();
          Song song = new Song();
          song.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
          song.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          song.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
          song.Track = DatabaseUtility.GetAsInt(results, i, "song.iTrack");
          song.Duration = DatabaseUtility.GetAsInt(results, i, "song.iDuration");
          song.Year = DatabaseUtility.GetAsInt(results, i, "song.iYear");
          song.Title = DatabaseUtility.Get(results, i, "song.strTitle");
          song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
          song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");
          song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "song.favorite"))) != 0;
          song.songId = DatabaseUtility.GetAsInt(results, i, "song.idSong");
          song.artistId = DatabaseUtility.GetAsInt(results, i, "artist.idArtist");
          song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
          song.genreId = DatabaseUtility.GetAsInt(results, i, "genre.idGenre");
          string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
          strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
          song.FileName = strFileName;
          songmap.m_song = song;
          songmap.m_strPath = song.FileName;

          songs.Add(songmap);
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("GetSongsByPath2:musicdatabase  {0} exception err:{1} stack:{2}", strSQL, ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }


    bool GetSongsByPathes(ArrayList pathes, ref ArrayList songs)
    {
      return false;
    }
    public bool GetSongsByAlbum(string strAlbum1, ref ArrayList songs)
    {
      try
      {
        string strAlbum = strAlbum1;
        //	musicdatabase always stores directories 
        //	without a slash at the end 

        DatabaseUtility.RemoveInvalidChars(ref strAlbum);

        songs.Clear();
        if (null == m_db)
          return false;

        string strSQL;
        strSQL = String.Format("select song.idSong,artist.idArtist,album.idAlbum,genre.idGenre,song.favorite,song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName, song.iRating, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and album.strAlbum like '{0}' and path.idPath=song.idPath order by song.iTrack", strAlbum);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          Song song = new Song();
          song.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
          song.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          song.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
          song.Track = DatabaseUtility.GetAsInt(results, i, "song.iTrack");
          song.Duration = DatabaseUtility.GetAsInt(results, i, "song.iDuration");
          song.Year = DatabaseUtility.GetAsInt(results, i, "song.iYear");
          song.Title = DatabaseUtility.Get(results, i, "song.strTitle");
          song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "song.favorite"))) != 0;
          song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
          song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");

          song.artistId = DatabaseUtility.GetAsInt(results, i, "artist.idArtist");
          song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
          song.genreId = DatabaseUtility.GetAsInt(results, i, "genre.idGenre");
          string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
          strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
          song.FileName = strFileName;

          songs.Add(song);
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;

    }

    public bool GetSongsByAlbumID(int albumId, ref List<Song> songs)
    {
      try
      {
        songs.Clear();
        if (null == m_db)
          return false;

        string strSQL;
        strSQL = String.Format("select song.idSong,artist.idArtist,album.idAlbum,genre.idGenre,song.favorite,song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName, song.iRating, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and album.idAlbum={0} and path.idPath=song.idPath order by song.iTrack", albumId);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
          return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
          Song song = new Song();
          song.Artist = DatabaseUtility.Get(results, i, "artist.strArtist");
          song.Album = DatabaseUtility.Get(results, i, "album.strAlbum");
          song.Genre = DatabaseUtility.Get(results, i, "genre.strGenre");
          song.Track = DatabaseUtility.GetAsInt(results, i, "song.iTrack");
          song.Duration = DatabaseUtility.GetAsInt(results, i, "song.iDuration");
          song.Year = DatabaseUtility.GetAsInt(results, i, "song.iYear");
          song.Title = DatabaseUtility.Get(results, i, "song.strTitle");
          song.Favorite = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "song.favorite"))) != 0;
          song.TimesPlayed = DatabaseUtility.GetAsInt(results, i, "song.iTimesPlayed");
          song.Rating = DatabaseUtility.GetAsInt(results, i, "song.iRating");

          song.artistId = DatabaseUtility.GetAsInt(results, i, "artist.idArtist");
          song.albumId = DatabaseUtility.GetAsInt(results, i, "album.idAlbum");
          song.genreId = DatabaseUtility.GetAsInt(results, i, "genre.idGenre");
          string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
          strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
          song.FileName = strFileName;

          songs.Add(song);
        }

        return true;
      }

      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }

      return false;
    }

    public void CheckVariousArtistsAndCoverArt()
    {
      if (_albumCache.Count <= 0)
        return;

      foreach (AlbumInfoCache album in _albumCache)
      {
        int lAlbumId = album.idAlbum;
        int lArtistId = album.idArtist;
        int lNewArtistId = album.idArtist;
        bool bVarious = false;
        ArrayList songs = new ArrayList();
        GetSongsByAlbum(album.Album, ref songs);
        if (songs.Count > 1)
        {
          //	Are the artists of this album all the same
          for (int i = 0; i < (int)songs.Count - 1; i++)
          {
            Song song = (Song)songs[i];
            Song song1 = (Song)songs[i + 1];
            if (song.Artist != song1.Artist)
            {
              string strVariousArtists = GUILocalizeStrings.Get(340);
              lNewArtistId = AddArtist(strVariousArtists);
              bVarious = true;
              break;
            }
          }
        }

        if (bVarious)
        {
          string strSQL;
          strSQL = String.Format("update album set idArtist={0} where idAlbum={1}", lNewArtistId, album.idAlbum);
          m_db.Execute(strSQL);
        }
        /*
                string strTempCoverArt;
                string strCoverArt;
                CUtil::GetAlbumThumb(album.strAlbum+album.strPath, strTempCoverArt, true);
                //	Was the album art of this album read during scan?
                if (CUtil::ThumbCached(strTempCoverArt))
                {
                  //	Yes.
                  //	Copy as permanent directory thumb
                  CUtil::GetAlbumThumb(album.strPath, strCoverArt);
                  ::CopyFile(strTempCoverArt, strCoverArt, false);

                  //	And move as permanent thumb for files and directory, where
                  //	album and path is known
                  CUtil::GetAlbumThumb(album.strAlbum+album.strPath, strCoverArt);
                  ::MoveFileEx(strTempCoverArt, strCoverArt, MOVEFILE_REPLACE_EXISTING);
                }*/
      }

      _albumCache.Clear();
    }

    public void BeginTransaction()
    {
      try
      {
        m_db.Execute("begin");
      }
      catch (Exception ex)
      {
        Log.Error("BeginTransaction: musicdatabase begin transaction failed exception err:{0} ", ex.Message);
        //Open();
      }
    }

    public void CommitTransaction()
    {
      Log.Info("Commit will effect {0} rows", m_db.ChangedRows());
      SQLiteResultSet CommitResults;
      try
      {
        CommitResults = m_db.Execute("commit");
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase commit failed exception err:{0} ", ex.Message);
        Open();
      }
    }

    public void RollbackTransaction()
    {
      try
      {
        m_db.Execute("rollback");
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase rollback failed exception err:{0} ", ex.Message);
        Open();
      }
    }


    #region		DatabaseReBuild
    SQLiteResultSet PathResults;
    SQLiteResultSet PathDeleteResults;
    int NumPaths;
    int PathNum;
    string strSQL;

    public int MusicDatabaseReorg(ArrayList shares)
    {
      return MusicDatabaseReorg(shares, _treatFolderAsAlbum, _scanForVariousArtists, true);
    }

    public int MusicDatabaseReorg(ArrayList shares, bool treatFolderAsAlbum, bool scanForVariousArtists, bool updateSinceLastImport)
    {
      // Make sure we use the selected settings if the user hasn't saved the
      // configuration...
      _treatFolderAsAlbum = treatFolderAsAlbum;
      _scanForVariousArtists = scanForVariousArtists;

      if (!updateSinceLastImport)
        _lastImport = DateTime.Parse("1900-01-01 00:00:00");

      if (shares == null)
      {
        LoadShares();
      }
      else
      {
        _shares = (ArrayList)shares.Clone();
      }
      DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();

      DateTime startTime = DateTime.Now;
      int fileCount = 0;

      try
      {
        Log.Info("Musicdatabasereorg: Beginning music database reorganization...");
        Log.Info("Musicdatabasereorg: Last import done at {0}", _lastImport.ToString());

        BeginTransaction();
        /// Delete song that are in non-existing MusicFolders (for example: you moved everything to another disk)
        MyArgs.progress = 2;
        MyArgs.phase = "Removing songs in old folders";
        OnDatabaseReorgChanged(MyArgs);
        DeleteSongsOldMusicFolders();


        /// Delete files that don't exist anymore (example: you deleted files from the Windows Explorer)
        MyArgs.progress = 4;
        MyArgs.phase = "Removing non existing songs";
        OnDatabaseReorgChanged(MyArgs);
        DeleteNonExistingSongs();

        /// Add missing files (example: You downloaded some new files)
        MyArgs.progress = 6;
        MyArgs.phase = "Adding new files";
        OnDatabaseReorgChanged(MyArgs);

        int AddMissingFilesResult = AddMissingFiles(8, 36, ref fileCount);
        //int AddMissingFilesResult = AddMissingFiles(8, 50);
        Log.Info("Musicdatabasereorg: Addmissingfiles: {0} files added", AddMissingFilesResult);

        /// Update the tags
        MyArgs.progress = 38;
        MyArgs.phase = "Updating tags";
        OnDatabaseReorgChanged(MyArgs);
        UpdateTags(40, 82);	//This one works for all the files in the MusicDatabase

        /// Cleanup foreign keys tables.
        /// We added, deleted new files
        /// We update all the tags
        /// Now lets clean up all the foreign keys
        MyArgs.progress = 84;
        MyArgs.phase = "Checking Artists";
        OnDatabaseReorgChanged(MyArgs);
        ExamineAndDeleteArtistids();

        MyArgs.progress = 86;
        MyArgs.phase = "Checking Genres";
        OnDatabaseReorgChanged(MyArgs);
        ExamineAndDeleteGenreids();

        MyArgs.progress = 88;
        MyArgs.phase = "Checking Paths";
        OnDatabaseReorgChanged(MyArgs);
        ExamineAndDeletePathids();

        MyArgs.progress = 90;
        MyArgs.phase = "Checking Albums";
        OnDatabaseReorgChanged(MyArgs);
        ExamineAndDeleteAlbumids();

        MyArgs.progress = 92;
        MyArgs.phase = "Updating album artists counts";
        OnDatabaseReorgChanged(MyArgs);
        UpdateAlbumArtistsCounts(92, 94);

        MyArgs.progress = 94;
        MyArgs.phase = "Updating sortable artist names";
        OnDatabaseReorgChanged(MyArgs);
        UpdateSortableArtistNames();

        // Check for a database backup and delete it if it exists       
        string backupDbPath = Config.GetFile(Config.Dir.Database, "musicdatabase4.db3.bak");

        if (File.Exists(backupDbPath))
        {
          MyArgs.progress = 95;
          MyArgs.phase = "Deleting backup database";
          OnDatabaseReorgChanged(MyArgs);

          File.Delete(backupDbPath);
        }
      }

      catch (Exception ex)
      {
        Log.Error("music-scan{0} {1} {2}",
                            ex.Message, ex.Source, ex.StackTrace);
      }

      finally
      {
        CommitTransaction();

        MyArgs.progress = 96;
        MyArgs.phase = "Finished";
        OnDatabaseReorgChanged(MyArgs);


        MyArgs.progress = 98;
        MyArgs.phase = "Compressing the database";
        OnDatabaseReorgChanged(MyArgs);
        Compress();

        MyArgs.progress = 100;
        MyArgs.phase = "done";
        OnDatabaseReorgChanged(MyArgs);
        EmptyCache();


        DateTime stopTime = DateTime.Now;
        TimeSpan ts = stopTime - startTime;
        float fSecsPerTrack = ((float)ts.TotalSeconds / (float)fileCount);
        string trackPerSecSummary = "";

        if (fileCount > 0)
          trackPerSecSummary = string.Format(" ({0} seconds per track)", fSecsPerTrack);

        Log.Info("Musicdatabasereorg: Music database reorganization done.  Processed {0} tracks in: {1:d2}:{2:d2}:{3:d2}{4}",
            fileCount, ts.Hours, ts.Minutes, ts.Seconds, trackPerSecSummary);

        // Save the time of the reorg, to be able to skip the files not updated / added the next time
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          xmlreader.SetValue("musicfiles", "lastImport", startTime.ToString());
        }
      }
      return (int)Errors.ERROR_OK;
    }

    int UpdateTags(int StartProgress, int EndProgress)
    {
      string strSQL;
      int NumRecordsUpdated = 0;
      int NumRecordsSkipped = 0;
      Log.Info("Musicdatabasereorg: starting Tag update");

      SQLiteResultSet FileList;
      strSQL = String.Format("select * from song, path where song.idPath=path.idPath");

      try
      {
        FileList = m_db.Execute(strSQL);
        if (FileList == null)
        {
          Log.Info("Musicdatabasereorg: UpdateTags: Select from failed");
          return (int)Errors.ERROR_REORG_SONGS;
        }
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: query for tag update could not be executed.");
        //m_db.Execute("rollback");
        return (int)Errors.ERROR_REORG_SONGS;
      }

      //	songs cleanup

      Log.Info("Going to check tags of {0} files", FileList.Rows.Count);

      DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();
      int ProgressRange = EndProgress - StartProgress;
      int TotalSongs = FileList.Rows.Count;
      int SongCounter = 0;

      double NewProgress;
      currentDate = System.DateTime.Now;

      for (int i = 0; i < FileList.Rows.Count; ++i)
      {
        string strFileName = DatabaseUtility.Get(FileList, i, "path.strPath");
        strFileName += DatabaseUtility.Get(FileList, i, "song.strFileName");
        //Log.Info("Musicdatabasereorg: starting Tag update 1 for {0} ", strFileName);

        // Check the file date, if it was created before the last import, we can ignore it
        if (System.IO.File.Exists(strFileName) && System.IO.File.GetLastWriteTime(strFileName) > _lastImport)
        {
          // The song will be updated, tags from the file will be checked against the tags in the database
          int idSong = Int32.Parse(DatabaseUtility.Get(FileList, i, "song.idSong"));
          if (!UpdateSong(strFileName, idSong))
          {
            Log.Info("Musicdatabasereorg: Song update after tag update failed for: {0}", strFileName);
            //m_db.Execute("rollback"); 
            return (int)Errors.ERROR_REORG_SONGS;
          }
          else
          {
            NumRecordsUpdated++;
          }
        }
        else
          NumRecordsSkipped++;

        if ((i % 10) == 0)
        {
          NewProgress = StartProgress + ((ProgressRange * SongCounter) / TotalSongs);
          MyArgs.progress = Convert.ToInt32(NewProgress);
          MyArgs.phase = String.Format("Updating tags {0}/{1}", i, FileList.Rows.Count);
          OnDatabaseReorgChanged(MyArgs);
        }
        SongCounter++;
      }//for (int i=0; i < results.Rows.Count;++i)
      Log.Info("Musicdatabasereorg: UpdateTags completed for {0} songs", (int)NumRecordsUpdated);
      Log.Info("Musicdatabasereorg: Skipped {0} songs because of no updates after last import", (int)NumRecordsSkipped);
      return (int)Errors.ERROR_OK;
    }

    public bool UpdateSong(string strPathSong, int idSong)
    {
      try
      {
        int idAlbum = 0;
        int idArtist = 0;
        int idPath = 0;
        int idGenre = 0;

        MusicTag tag;
        tag = TagReader.TagReader.ReadTag(strPathSong);
        if (tag != null)
        {
          //Log.Write ("Musicdatabasereorg: We are gonna update the tags for {0}", strPathSong);
          Song song = new Song();
          song.Title = tag.Title;
          song.Genre = tag.Genre;
          song.FileName = strPathSong;
          song.Artist = tag.Artist;
          song.Album = tag.Album;
          song.Year = tag.Year;
          song.Track = tag.Track;
          song.Duration = tag.Duration;

          char[] trimChars = { ' ', '\x00' };
          String tagAlbumName = String.Format("{0}-{1}", tag.Artist.Trim(trimChars), tag.Album.Trim(trimChars));
          string strSmallThumb = MediaPortal.Util.Utils.GetCoverArtName(Thumbs.MusicAlbum, Util.Utils.MakeFileName(tagAlbumName));
          string strLargeThumb = MediaPortal.Util.Utils.GetLargeCoverArtName(Thumbs.MusicAlbum, Util.Utils.MakeFileName(tagAlbumName));


          if (tag.CoverArtImageBytes != null)
          {
            if (_extractEmbededCoverArt)
            {
              try
              {
                bool extractFile = false;
                if (!System.IO.File.Exists(strSmallThumb))
                  extractFile = true;
                else
                {
                  // Prevent creation of the thumbnail multiple times, when all songs of an album contain coverart
                  System.DateTime fileDate = System.IO.File.GetLastWriteTime(strSmallThumb);
                  System.TimeSpan span = currentDate - fileDate;
                  if (span.Hours > 0)
                    extractFile = true;
                }

                if (extractFile)
                {
                  if (!MediaPortal.Util.Picture.CreateThumbnail(tag.CoverArtImage, strSmallThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0))
                    Log.Debug("Could not extract thumbnail from {0}", strPathSong);
                  if (!MediaPortal.Util.Picture.CreateThumbnail(tag.CoverArtImage, strLargeThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0))
                    Log.Debug("Could not extract thumbnail from {0}", strPathSong);
                }
              }
              catch (Exception) { }
            }
          }
          // no mp3 coverart - use folder art if present to get an album thumb
          if (_useFolderThumbs)
          {
            // only create for the first file
            if (!System.IO.File.Exists(strSmallThumb))
            {
              string sharefolderThumb = MediaPortal.Util.Utils.GetFolderThumb(strPathSong);
              if (System.IO.File.Exists(sharefolderThumb))
              {
                if (!MediaPortal.Util.Picture.CreateThumbnail(sharefolderThumb, strSmallThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0))
                  Log.Debug("Could not create album thumb from folder {0}", strPathSong);
                if (!MediaPortal.Util.Picture.CreateThumbnail(sharefolderThumb, strLargeThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0))
                  Log.Debug("Could not create large album thumb from folder {0}", strPathSong);
              }
            }
          }
          

          // create the local folder thumb cache / and folder.jpg itself if not present
          if (_useFolderThumbs || _createMissingFolderThumbs)
            CreateFolderThumbs(strPathSong, strSmallThumb);

          if (_useFolderArtForArtistGenre)
          {
            CreateArtistThumbs(strSmallThumb, song.Artist);
            CreateGenreThumbs(strSmallThumb, song.Genre);
          }

          string strPath, strFileName;
          DatabaseUtility.Split(song.FileName, out strPath, out strFileName);

          string strTmp;
          strTmp = song.Album;
          DatabaseUtility.RemoveInvalidChars(ref strTmp);
          song.Album = strTmp;
          strTmp = song.Genre;
          DatabaseUtility.RemoveInvalidChars(ref strTmp);
          song.Genre = strTmp;
          strTmp = song.Artist;
          DatabaseUtility.RemoveInvalidChars(ref strTmp);
          song.Artist = strTmp;
          strTmp = song.Title;
          DatabaseUtility.RemoveInvalidChars(ref strTmp);
          song.Title = strTmp;

          DatabaseUtility.RemoveInvalidChars(ref strFileName);

          /// PDW 25 may 2005
          /// Adding these items starts a select and insert query for each. 
          /// Maybe we should check if anything has changed in the tags
          /// if not, no need to add and invoke query's.
          /// here we are gonna (try to) add the tags

          idGenre = AddGenre(tag.Genre);
          //Log.Write ("Tag.genre = {0}",tag.Genre);
          idArtist = AddArtist(tag.Artist);
          //Log.Write ("Tag.Artist = {0}",tag.Artist);
          idPath = AddPath(strPath);
          //Log.Write ("strPath= {0}",strPath);

          if (_treatFolderAsAlbum)
            idAlbum = AddAlbum(tag.Album, idArtist, idPath);

          else
            idAlbum = AddAlbum(tag.Album, idArtist);

          ulong dwCRC = 0;
          CRCTool crc = new CRCTool();
          crc.Init(CRCTool.CRCCode.CRC32);
          dwCRC = crc.calc(strPathSong);

          //SQLiteResultSet results;

          //Log.Write ("Song {0} will be updated with CRC={1}",song.FileName,dwCRC);

          string strSQL;
          strSQL = String.Format("update song set idArtist={0},idAlbum={1},idGenre={2},idPath={3},strTitle='{4}',iTrack={5},iDuration={6},iYear={7},dwFileNameCRC='{8}',strFileName='{9}' where idSong={10}",
            idArtist, idAlbum, idGenre, idPath,
            song.Title,
            song.Track, song.Duration, song.Year,
            dwCRC,
            strFileName, idSong);
          //Log.Write (strSQL);
          try
          {
            m_db.Execute(strSQL);
          }
          catch (Exception)
          {
            Log.Error("Musicdatabasereorg: Update tags for {0} failed because of DB exception", strPathSong);
            return false;
          }
        }
        else
        {
          Log.Info("Musicdatabasereorg: cannot get tag for {0}", strPathSong);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Musicdatabasereorg: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
      //Log.Write ("Musicdatabasereorg: Update for {0} success", strPathSong);
      return true;
    }

    void CreateFolderThumbs(string strSongPath, string strSmallThumb)
    {
      if (System.IO.File.Exists(strSmallThumb) && strSongPath != String.Empty)
      {
        string folderThumb = MediaPortal.Util.Utils.GetFolderThumb(strSongPath);
        //string folderLThumb = folderThumb;
        //folderLThumb = Util.Utils.ConvertToLargeCoverArt(folderLThumb);
        string localFolderThumb = MediaPortal.Util.Utils.GetLocalFolderThumb(strSongPath);
        string localFolderLThumb = localFolderThumb;
        localFolderLThumb = Util.Utils.ConvertToLargeCoverArt(localFolderLThumb);

        try
        {
          // we've embedded art but no folder.jpg --> copy the large one for cache and create a small cache thumb
          if (!System.IO.File.Exists(folderThumb))
          {
            if (_createMissingFolderThumbs)
            {
              MediaPortal.Util.Picture.CreateThumbnail(strSmallThumb, folderThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0);

              if (_useFolderThumbs)
              {
                if (!System.IO.File.Exists(localFolderThumb))
                  MediaPortal.Util.Picture.CreateThumbnail(strSmallThumb, localFolderThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0);
                if (!System.IO.File.Exists(localFolderLThumb))
                  System.IO.File.Copy(folderThumb, localFolderLThumb, true);
              }
            }
          }
          else
          {
            if (_useFolderThumbs)
            {
              if (!System.IO.File.Exists(localFolderThumb))
                MediaPortal.Util.Picture.CreateThumbnail(folderThumb, localFolderThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0);
              if (!System.IO.File.Exists(localFolderLThumb))
              {
                // just copy the folder.jpg if it is reasonable in size - otherwise re-create it
                System.IO.FileInfo fiRemoteFolderArt = new System.IO.FileInfo(folderThumb);
                if (fiRemoteFolderArt.Length < 32000)
                  System.IO.File.Copy(folderThumb, localFolderLThumb, true);
                else
                  MediaPortal.Util.Picture.CreateThumbnail(folderThumb, localFolderLThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0);
              }
            }
          }
        }
        catch (Exception ex1)
        {
          Log.Warn("Database: could not create folder thumb for {0} - {1}", strSongPath, ex1.Message);
        }
      }
    }

    void CreateArtistThumbs(string strSmallThumb, string songArtist)
    {
      if (System.IO.File.Exists(strSmallThumb) && songArtist != String.Empty)
      {
        string artistThumb = MediaPortal.Util.Utils.GetCoverArtName(Thumbs.MusicArtists, Util.Utils.MakeFileName(songArtist));
        if (!System.IO.File.Exists(artistThumb))
        {
          try
          {
            MediaPortal.Util.Picture.CreateThumbnail(strSmallThumb, artistThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0);
            MediaPortal.Util.Picture.CreateThumbnail(strSmallThumb, Util.Utils.ConvertToLargeCoverArt(artistThumb), (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0);
            //System.IO.File.Copy(strSmallThumb, artistThumb, true);
            //System.IO.File.SetAttributes(artistThumb, System.IO.File.GetAttributes(artistThumb) | System.IO.FileAttributes.Hidden);
          }
          catch (Exception) { }
        }
      }
    }

    void CreateGenreThumbs(string strSmallThumb, string songGenre)
    {
      if (System.IO.File.Exists(strSmallThumb) && songGenre != String.Empty)
      {
        // using the thumb of the first item of a gerne / artist having a thumb

        // The genre may contains unallowed chars
        string strGenre = MediaPortal.Util.Utils.MakeFileName(songGenre);

        // Sometimes the genre contains a number code in brackets -> remove that
        // (code borrowed from addGenre() method)
        if (String.Compare(strGenre.Substring(0, 1), "(") == 0)
        {
          bool FixedTheCode = false;
          for (int i = 1; (i < 10 && i < strGenre.Length & !FixedTheCode); ++i)
          {
            if (String.Compare(strGenre.Substring(i, 1), ")") == 0)
            {
              strGenre = strGenre.Substring(i + 1, (strGenre.Length - i - 1));
              FixedTheCode = true;
            }
          }
        }
        // Now the genre is clean and sober -> build a filename out of it
        string genreThumb = MediaPortal.Util.Utils.GetCoverArtName(Thumbs.MusicGenre, Util.Utils.MakeFileName(strGenre));

        if (!System.IO.File.Exists(genreThumb))
        {
          // thumb for this genre does not exist yet -> simply use the folderThumb from above
          // and copy it to thumbs\music\gerne\<genre>.jpg 
          try
          {
            MediaPortal.Util.Picture.CreateThumbnail(strSmallThumb, genreThumb, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0);
            MediaPortal.Util.Picture.CreateThumbnail(strSmallThumb, Util.Utils.ConvertToLargeCoverArt(genreThumb), (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0);
            //System.IO.File.Copy(strSmallThumb, genreThumb, true);
            //System.IO.File.SetAttributes(genreThumb, System.IO.File.GetAttributes(genreThumb) | System.IO.FileAttributes.Hidden);
          }
          catch (Exception) { }
        }
      }
    }

    int DeleteNonExistingSongs()
    {
      string strSQL;
      /// Opening the MusicDatabase

      SQLiteResultSet results;
      strSQL = String.Format("select * from song, path where song.idPath=path.idPath");
      try
      {
        results = MusicDatabase.DBHandle.Execute(strSQL);
        if (results == null)
          return (int)Errors.ERROR_REORG_SONGS;
      }
      catch (Exception)
      {
        Log.Error("DeleteNonExistingSongs() to get songs from database");
        return (int)Errors.ERROR_REORG_SONGS;
      }
      int removed = 0;
      Log.Info("Musicdatabasereorg: starting song cleanup for {0} songs", (int)results.Rows.Count);
      for (int i = 0; i < results.Rows.Count; ++i)
      {
        string strFileName = DatabaseUtility.Get(results, i, "path.strPath");
        strFileName += DatabaseUtility.Get(results, i, "song.strFileName");
        ///pDlgProgress.SetLine(2, System.IO.Path.GetFileName(strFileName) );

        if (!System.IO.File.Exists(strFileName))
        {
          /// song doesn't exist anymore, delete it
          /// We don't care about foreign keys at this moment. We'll just change this later.

          removed++;
          //Log.Info("Musicdatabasereorg:Song {0} will to be deleted from MusicDatabase", strFileName);
          DeleteSong(strFileName, false);

        }
        if ((i % 10) == 0)
        {
          DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();
          MyArgs.progress = 4;
          MyArgs.phase = String.Format("Removing non existing songs:{0}/{1} checked, {2} removed", i, results.Rows.Count, removed);
          OnDatabaseReorgChanged(MyArgs);
        }
      }//for (int i=0; i < results.Rows.Count;++i)
      Log.Info("Musicdatabasereorg: DeleteNonExistingSongs completed");
      return (int)Errors.ERROR_OK;
    }

    int ExamineAndDeleteArtistids()
    {
      /// This will delete all artists and artistinfo from the database that don't have a corresponding song anymore
      /// First delete all the albuminfo before we delete albums (foreign keys)

      /// TODO: delete artistinfo first
      string strSql = "delete from artist where artist.idArtist not in (select idArtist from song)";
      try
      {
        m_db.Execute(strSql);
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: ExamineAndDeleteArtistids failed");
        //m_db.Execute("rollback");
        return (int)Errors.ERROR_REORG_ARTIST;
      }

      Log.Info("Musicdatabasereorg: ExamineAndDeleteArtistids completed");
      return (int)Errors.ERROR_OK;
    }

    int ExamineAndDeleteGenreids()
    {
      /// This will delete all genres from the database that don't have a corresponding song anymore
      SQLiteResultSet result;
      string strSql = "delete from genre where idGenre not in (select idGenre from song)";
      try
      {
        m_db.Execute(strSql);
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: ExamineAndDeleteGenreids failed");
        //m_db.Execute("rollback");
        return (int)Errors.ERROR_REORG_GENRE;
      }

      strSql = "select count (*) aantal from genre where idGenre not in (select idGenre from song)";
      try
      {
        result = MusicDatabase.DBHandle.Execute(strSql);
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: ExamineAndDeleteGenreids failed");
        //m_db.Execute("rollback");
        return (int)Errors.ERROR_REORG_GENRE;

      }
      string Aantal = DatabaseUtility.Get(result, 0, "aantal");
      if (Aantal != "0")
        return (int)Errors.ERROR_REORG_GENRE;
      Log.Info("Musicdatabasereorg: ExamineAndDeleteGenreids completed");

      return (int)Errors.ERROR_OK;
    }

    int ExamineAndDeletePathids()
    {
      /// This will delete all paths from the database that don't have a corresponding song anymore
      string strSql = String.Format("delete from path where idPath not in (select idPath from song)");
      try
      {
        m_db.Execute(strSql);
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: ExamineAndDeletePathids failed");
        //m_db.Execute("rollback");
        return (int)Errors.ERROR_REORG_PATH;
      }
      Log.Info("Musicdatabasereorg: ExamineAndDeletePathids completed");
      return (int)Errors.ERROR_OK;
    }

    int ExamineAndDeleteAlbumids()
    {
      /// This will delete all albums from the database that don't have a corresponding song anymore
      /// First delete all the albuminfo before we delete albums (foreign keys)
      string strSql = String.Format("delete from albuminfo where idAlbum not in (select idAlbum from song)");
      try
      {
        m_db.Execute(strSql);
      }
      catch (Exception)
      {
        //m_db.Execute("rollback");
        Log.Error("MusicDatabasereorg: ExamineAndDeleteAlbumids() unable to delete old albums");
        return (int)Errors.ERROR_REORG_ALBUM;
      }
      /// Now all the albums without songs will be deleted.
      ///SQLiteResultSet results;
      strSql = String.Format("delete from album where idAlbum not in (select idAlbum from song)");
      try
      {
        m_db.Execute(strSql);
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: ExamineAndDeleteAlbumids failed");
        //m_db.Execute("rollback");
        return (int)Errors.ERROR_REORG_ALBUM;
      }
      Log.Info("Musicdatabasereorg: ExamineAndDeleteAlbumids completed");
      return (int)Errors.ERROR_OK;
    }
    int Compress()
    {
      //	compress database
      try
      {
        DatabaseUtility.CompactDatabase(MusicDatabase.DBHandle);
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: vacuum failed");
        return (int)Errors.ERROR_COMPRESSING;
      }
      Log.Info("Musicdatabasereorg: Compress completed");
      return (int)Errors.ERROR_OK;
    }

    int LoadShares()
    {
      /// 25-may-2005 TFRO71
      /// Added this function to make scan the Music Shares that are in the configuration file.
      /// Songs that are not in these Shares will be removed from the MusicDatabase
      /// The files will offcourse not be touched
      string currentFolder = String.Empty;
      bool fileMenuEnabled = false;
      string fileMenuPinCode = String.Empty;

      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        fileMenuEnabled = xmlreader.GetValueAsBool("filemenu", "enabled", true);

        string strDefault = xmlreader.GetValueAsString("music", "default", String.Empty);
        for (int i = 0; i < 20; i++)
        {
          string strShareName = String.Format("sharename{0}", i);
          string strSharePath = String.Format("sharepath{0}", i);

          string shareType = String.Format("sharetype{0}", i);
          string shareServer = String.Format("shareserver{0}", i);
          string shareLogin = String.Format("sharelogin{0}", i);
          string sharePwd = String.Format("sharepassword{0}", i);
          string sharePort = String.Format("shareport{0}", i);
          string remoteFolder = String.Format("shareremotepath{0}", i);

          string SharePath = xmlreader.GetValueAsString("music", strSharePath, String.Empty);

          if (SharePath.Length > 0)
            _shares.Add(SharePath);
        }
      }
      return 0;
    }

    int DeleteSongsOldMusicFolders()
    {

      /// PDW 24-05-2005
      /// Here we handle the songs in non-existing MusicFolders (shares).
      /// So we have to check Mediaportal.XML
      /// Loading the current MusicFolders
      Log.Info("Musicdatabasereorg: deleting songs in non-existing shares");

      /// For each path in the MusicDatabase we will check if it's in a share
      /// If not, we will delete all the songs in this path.
      strSQL = String.Format("select * from path");

      try
      {
        PathResults = MusicDatabase.DBHandle.Execute(strSQL);
        if (PathResults == null)
          return (int)Errors.ERROR_REORG_SONGS;
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg:DeleteSongsOldMusicFolders() failed");
        //MusicDatabase.DBHandle.Execute("rollback");
        return (int)Errors.ERROR_REORG_SONGS;
      }
      NumPaths = PathResults.Rows.Count;

      /// We will walk through all the paths (from the songs) and see if they match with a share/MusicFolder (from the config)
      for (PathNum = 0; PathNum < PathResults.Rows.Count; ++PathNum)
      {
        string Path = DatabaseUtility.Get(PathResults, PathNum, "strPath");
        string PathId = DatabaseUtility.Get(PathResults, PathNum, "idPath");
        /// We now have a path, we will check it along all the shares
        bool Path_has_Share = false;
        foreach (string Share in _shares)
        {
          ///Here we can check if the Path has an existing share
          if (Share.Length <= Path.Length)
          {
            string Path_part = Path.Substring(0, Share.Length);
            if (Share.ToUpper() == Path_part.ToUpper())
              Path_has_Share = true;
          }
        }
        if (!Path_has_Share)
        {
          Log.Info("Musicdatabasereorg: Path {0} with id {1} has no corresponding share, songs will be deleted ", Path, PathId);
          strSQL = String.Format("delete from song where idPath = {0}", PathId);
          try
          {
            PathDeleteResults = MusicDatabase.DBHandle.Execute(strSQL);
            if (PathDeleteResults == null)
              return (int)Errors.ERROR_REORG_SONGS;
          }
          catch (Exception)
          {
            //MusicDatabase.DBHandle.Execute("rollback");
            Log.Error("Musicdatabasereorg: DeleteSongsOldMusicFolders failed");
            return (int)Errors.ERROR_REORG_SONGS;
          }

          Log.Info("Trying to commit the deletes from the DB");

        } /// If path has no share
      } /// For each path
      Log.Info("Musicdatabasereorg: DeleteSongsOldMusicFolders completed");
      return (int)Errors.ERROR_OK;
    } // DeleteSongsOldMusicFolders

    #endregion

    ArrayList Extensions = MediaPortal.Util.Utils.AudioExtensions;

    ArrayList availableFiles;

    private int AddMissingFiles(int StartProgress, int EndProgress, ref int fileCount)
    {
      /// This seems to clear the arraylist and make it valid
      availableFiles = new ArrayList();

      DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();
      string strSQL;
      ulong dwCRC;
      CRCTool crc = new CRCTool();
      crc.Init(CRCTool.CRCCode.CRC32);

      int totalFiles = 0;

      int ProgressRange = EndProgress - StartProgress;
      int TotalSongs;
      int SongCounter = 0;
      int AddedCounter = 0;
      string MusicFilePath, MusicFileName;
      double NewProgress;

      foreach (string Share in _shares)
      {
        ///Here we can check if the Path has an existing share
        CountFilesInPath(Share, ref totalFiles);
      }
      TotalSongs = totalFiles;
      Log.Info("Musicdatabasereorg: Found {0} files to check if they are new", (int)totalFiles);
      SQLiteResultSet results;

      foreach (string MusicFile in availableFiles)
      {
        ///Here we can check if the Path has an existing share
        ///
        SongCounter++;

        // Check the file date, if it was created before the last import, we can ignore it
        if (System.IO.File.GetCreationTime(MusicFile) > _lastImport)
        {
          DatabaseUtility.Split(MusicFile, out MusicFilePath, out MusicFileName);

          dwCRC = crc.calc(MusicFile);

          /// Convert.ToChar(34) wil give you a "
          /// This is handy in building strings for SQL
          strSQL = String.Format("select * from song,path where song.idPath=path.idPath and strFileName={1}{0}{1} and strPath like {1}{2}{1}", MusicFileName, Convert.ToChar(34), MusicFilePath);
          //Log.Write (strSQL);
          //Log.Write (MusicFilePath);
          //Log.Write (MusicFile);
          //Log.Write (MusicFileName);

          try
          {
            results = m_db.Execute(strSQL);
            if (results == null)
            {
              Log.Info("Musicdatabasereorg: AddMissingFiles finished with error (results == null)");
              return (int)Errors.ERROR_REORG_SONGS;
            }
          }

          catch (Exception)
          {
            Log.Error("Musicdatabasereorg: AddMissingFiles finished with error (exception for select)");
            //m_db.Execute("rollback");
            return (int)Errors.ERROR_REORG_SONGS;
          }

          if (results.Rows.Count >= 1)
          {
            /// The song exists
            /// Log.Write ("Song {0} exists, dont do a thing",MusicFileName);
            /// string strFileName = DatabaseUtility.Get(results,0,"path.strPath") ;
            /// strFileName += DatabaseUtility.Get(results,0,"song.strFileName") ;
          }
          else
          {
            //The song does not exist, we will add it.
            AddSong(MusicFileName, MusicFilePath);
            AddedCounter++;
          }
        }
        if ((SongCounter % 10) == 0)
        {
          NewProgress = StartProgress + ((ProgressRange * SongCounter) / TotalSongs);
          MyArgs.progress = Convert.ToInt32(NewProgress);
          MyArgs.phase = String.Format("Checking for new files {0}/{1} - new: {2}", SongCounter, availableFiles.Count, AddedCounter);
          OnDatabaseReorgChanged(MyArgs);
        }
      } //end for-each
      Log.Info("Musicdatabasereorg: AddMissingFiles finished with SongCounter = {0}", SongCounter);
      Log.Info("Musicdatabasereorg: AddMissingFiles finished with AddedCounter = {0}", AddedCounter);
      Log.Info("Musicdatabasereorg: {0} skipped because of creation before the last import", SongCounter - AddedCounter);

      fileCount = SongCounter;
      return SongCounter;
    }

    /// <summary>
    /// TFRO 7 june 2005
    /// This is the method that adds songs, you need to check existence of the file before. This method
    /// will just add it.
    /// </summary>
    /// <param name="MusicFileName"></param>
    /// <param name="MusicFilePath"></param>
    /// <returns></returns>

    private int AddSong(string MusicFileName, string MusicFilePath)
    {
      SQLiteResultSet results;

      int idPath = AddPath(MusicFilePath);
      int idArtist = AddArtist(Strings.Unknown);

      int idAlbum = -1;

      if (_treatFolderAsAlbum)
        idAlbum = AddAlbum(Strings.Unknown, idArtist, idPath);

      else
        idAlbum = AddAlbum(Strings.Unknown, idArtist);

      int idGenre = AddGenre(Strings.Unknown);

      /// Here we are gonna make a CRC code to add to the database
      /// This coded is used for searching on the filename
      string fname = MusicFilePath;
      if (!fname.EndsWith(@"\"))
        fname = fname + @"\";
      fname += MusicFileName;

      ulong dwCRC = 0;
      CRCTool crc = new CRCTool();
      crc.Init(CRCTool.CRCCode.CRC32);
      dwCRC = crc.calc(fname);

      //Log.Write ("Song {0} will be added with CRC {1}",MusicFileName,dwCRC);
      /// Here we add song to the database
      /// strSQL = String.Format("insert into song (idPath,strFileName,idAlbum,idArtist,idGenre,dwFileNameCRC,iTimesPlayed,iRating,favorite) values ({0},{2}{1}{2},{3},{4},{5},{6},{7},{8},{9})",idPath, MusicFileName, Convert.ToChar(34),idAlbum,idArtist,idGenre,dwCRC,0,0,0);
      /// TFRO 12-6-2005 New insert because we missed some fields.

      strSQL = String.Format("insert into song (idArtist,idAlbum,idGenre,idPath,iTrack,iDuration,iYear,dwFileNameCRC,strFileName,iTimesPlayed,iRating,favorite) values ({0},{1},{2},{3},{4},{5},{6},{7},{9}{8}{9},{10},{11},{12})", idArtist, idAlbum, idGenre, idPath, 0, 0, 0, dwCRC, MusicFileName, Convert.ToChar(34), 0, 0, 0);
      //Log.Write (strSQL);
      try
      {
        //Log.Write ("Musicdatabasereorg: Insert {0}{1} into the database",MusicFilePath,MusicFileName);
        results = m_db.Execute(strSQL);
        if (results == null)
        {
          Log.Info("Musicdatabasereorg: Insert of song {0}{1} failed", MusicFilePath, MusicFileName);
          return (int)Errors.ERROR_REORG_SONGS;
        }
      }
      catch (Exception)
      {
        Log.Error("Musicdatabasereorg: Insert of song {0}{1} failed", MusicFilePath, MusicFileName);
        //m_db.Execute("rollback");
        return (int)Errors.ERROR_REORG_SONGS;
      }

      //Log.Write ("Musicdatabasereorg: Insert of song {0}{1} success",MusicFilePath,MusicFileName);
      return (int)Errors.ERROR_OK;
    }

    private void CountFilesInPath(string path, ref int totalFiles)
    {
      //
      // Count the files in the current directory
      //
      //Log.Info("Musicdatabasereorg: Counting files in {0}", path );

      try
      {
        foreach (string extension in Extensions)
        {
          string[] files = Directory.GetFiles(path, String.Format("*{0}", extension));
          for (int i = 0; i < files.Length; ++i)
          {
            string ext = System.IO.Path.GetExtension(files[i]).ToLower();
            if (ext == ".m3u")
              continue;
            if (ext == ".pls")
              continue;
            if (ext == ".wpl")
              continue;
            if (ext == ".b4s")
              continue;
            if ((File.GetAttributes(files[i]) & FileAttributes.Hidden) == FileAttributes.Hidden)
              continue;
            availableFiles.Add(files[i]);
            totalFiles++;
          }
        }
      }
      catch
      {
        // Ignore
      }
      if ((totalFiles % 10) == 0)
      {
        DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();
        MyArgs.progress = 4;
        MyArgs.phase = String.Format("Adding new files: {0} files found", totalFiles);
        OnDatabaseReorgChanged(MyArgs);
      }
      //
      // Count files in subdirectories
      //
      try
      {
        string[] directories = Directory.GetDirectories(path);

        foreach (string directory in directories)
        {
          CountFilesInPath(directory, ref totalFiles);
        }
      }
      catch
      {
        // Ignore
      }
    }

    public void UpdateAlbumArtistsCounts(int startProgress, int endProgress)
    {
      if (_albumCache.Count == 0)
        return;

      DatabaseReorgEventArgs MyArgs = new DatabaseReorgEventArgs();
      int progressRange = endProgress - startProgress;
      int totalAlbums = _albumCache.Count;
      int albumCounter = 0;
      double newProgress = 0;

      Hashtable artistCountTable = new Hashtable();
      bool variousArtistsFound = false;

      string strSQL;

      try
      {
        // Process the array from the end, to have no troubles when removing processed items
        for (int j = _albumCache.Count - 1; j > -1; j--)
        {
          AlbumInfoCache album = (AlbumInfoCache)_albumCache[j];
          artistCountTable.Clear();

          if (album.Album == Strings.Unknown)
            continue;

          int lAlbumId = album.idAlbum;
          int lArtistId = album.idArtist;

          List<Song> songs = new List<Song>();

          if (_treatFolderAsAlbum)
            GetSongsByPathId(album.idPath, ref songs);

          else
            this.GetSongsByAlbumID(album.idAlbum, ref songs);

          ++albumCounter;

          if (songs.Count > 1)
          {
            //	Are the artists of this album all the same
            for (int i = 0; i < songs.Count; i++)
            {
              Song song = (Song)songs[i];
              artistCountTable[song.artistId] = song;
            }
          }

          int artistCount = Math.Max(1, artistCountTable.Count);

          if (artistCount > 1)
          {
            variousArtistsFound = true;
            Log.Info("Musicdatabasereorg: multiple album artists album found: {0}.  Updating album artist count ({1}).", album.Album, artistCount);

            foreach (DictionaryEntry entry in artistCountTable)
            {
              Song s = (Song)entry.Value;
              Log.Info("   ArtistID:{0}  Artist Name:{1}  Track Title:{2}", s.artistId, s.Artist, s.Title);
            }
          }

          strSQL = string.Format("update album set iNumArtists={0} where idAlbum={1}", artistCount, album.idAlbum);
          m_db.Execute(strSQL);

          // Remove the processed Album from the cache
          lock (_albumCache)
          {
            _albumCache.RemoveAt(j);
          }

          if ((albumCounter % 10) == 0)
          {
            newProgress = startProgress + ((progressRange * albumCounter) / totalAlbums);
            MyArgs.progress = Convert.ToInt32(newProgress);
            MyArgs.phase = String.Format("Updating album {0}/{1} artist counts", albumCounter, totalAlbums);
            OnDatabaseReorgChanged(MyArgs);
          }
        }

        // Finally, set the artist id to the "Various Artists" id on all albums with more than one artist
        if (_scanForVariousArtists && variousArtistsFound)
        {
          string variousArtists = GUILocalizeStrings.Get(340);

          if (variousArtists.Length == 0)
            variousArtists = "various artists";

          long idVariousArtists = AddArtist(variousArtists);

          if (idVariousArtists != -1)
          {
            Log.Info("Musicdatabasereorg: updating artist id's for 'Various Artists' albums");

            strSQL = string.Format("update album set idArtist={0} where iNumArtists>1", idVariousArtists);
            m_db.Execute(strSQL);
          }
        }
      }

      catch (Exception ex)
      {
        Log.Info("Musicdatabasereorg: {0}", ex);
      }
    }

    private bool StripArtistNamePrefix(ref string artistName, bool appendPrefix)
    {
      string temp = artistName.ToLower();

      foreach (string s in ArtistNamePrefixes)
      {
        if (s.Length == 0)
          continue;

        string prefix = s;
        prefix = prefix.Trim().ToLower();
        int pos = temp.IndexOf(prefix + " ");
        if (pos == 0)
        {
          string tempName = artistName.Substring(prefix.Length).Trim();

          if (appendPrefix)
            artistName = string.Format("{0}, {1}", tempName, artistName.Substring(0, prefix.Length));

          else
            artistName = temp;

          return true;
        }
      }

      return false;
    }

    private void UpdateSortableArtistNames()
    {
      ArrayList artists = new ArrayList();
      this.GetArtists(ref artists);

      for (int i = 0; i < artists.Count; i++)
      {
        string origArtistName = (string)artists[i];
        string sortableArtistName = origArtistName;

        StripArtistNamePrefix(ref sortableArtistName, true);

        try
        {
          DatabaseUtility.RemoveInvalidChars(ref sortableArtistName);
          DatabaseUtility.RemoveInvalidChars(ref origArtistName);
          string strSQL = String.Format("update artist set strSortName='{0}' where strArtist like '{1}'", sortableArtistName, origArtistName);
          m_db.Execute(strSQL);
        }

        catch (Exception ex)
        {
          Log.Info("UpdateSortableArtistNames: {0}", ex);
        }
      }
    }

    // by hwahrmann to support the MusicShareWatcher

    public bool SongExists(string strFileName)
    {
      ulong dwCRC = 0;
      CRCTool crc = new CRCTool();
      crc.Init(CRCTool.CRCCode.CRC32);
      dwCRC = crc.calc(strFileName);

      string strSQL;
      strSQL = String.Format("select idSong from song where dwFileNameCRC like '{0}'",
                     dwCRC);

      SQLiteResultSet results;
      results = m_db.Execute(strSQL);
      if (results.Rows.Count > 0)
        // Found
        return true;
      else
        // Not Found
        return false;
    }

    public bool RenameSong(string strOldFileName, string strNewFileName)
    {
      try
      {
        string strPath, strFName;
        DatabaseUtility.Split(strNewFileName, out strPath, out strFName);

        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);

        // The rename may have been on a directory or a file
        // In case of a directory rename, the Path needs to be corrected
        FileInfo fi = new FileInfo(strNewFileName);
        if (fi.Exists)
        {
          // Must be a file that has been changed
          // Now get the CRC of the original file name and the new file name
          ulong dwOldCRC = crc.calc(strOldFileName);
          ulong dwNewCRC = crc.calc(strNewFileName);

          DatabaseUtility.RemoveInvalidChars(ref strFName);

          string strSQL;
          strSQL = String.Format("update song set dwFileNameCRC = '{0}', strFileName = '{1}' where dwFileNameCRC like '{2}'",
                       dwNewCRC,
                       strFName,
                       dwOldCRC);
          SQLiteResultSet results;
          results = m_db.Execute(strSQL);
          return true;
        }
        else
        {
          // See if it is a directory
          DirectoryInfo di = new DirectoryInfo(strNewFileName);
          if (di.Exists)
          {
            // Must be a directory, so let's change the path entries, containing the old
            // name with the new name
            DatabaseUtility.RemoveInvalidChars(ref strOldFileName);

            string strSQL;
            strSQL = String.Format("select * from path where strPath like '{0}%'",
                    strOldFileName);

            SQLiteResultSet results;
            SQLiteResultSet resultSongs;
            ulong dwCRC = 0;
            results = m_db.Execute(strSQL);
            if (results.Rows.Count > 0)
            {
              try
              {
                BeginTransaction();
                // We might have changed a Top directory, so we get a lot of path entries returned
                for (int rownum = 0; rownum < results.Rows.Count; rownum++)
                {
                  int lPathId = DatabaseUtility.GetAsInt(results, rownum, "path.idPath");
                  string strTmpPath = DatabaseUtility.Get(results, rownum, "path.strPath");
                  strPath = strTmpPath.Replace(strOldFileName, strNewFileName);
                  // Need to keep an unmodified path for the later CRC calculation
                  strTmpPath = strPath;
                  DatabaseUtility.RemoveInvalidChars(ref strTmpPath);
                  strSQL = String.Format("update path set strPath='{0}' where idPath={1}",
                          strTmpPath,
                          lPathId);

                  m_db.Execute(strSQL);
                  // And now we need to update the songs with the new CRC
                  strSQL = String.Format("select * from song where idPath = {0}",
                               lPathId);
                  resultSongs = m_db.Execute(strSQL);
                  if (resultSongs.Rows.Count > 0)
                  {
                    for (int i = 0; i < resultSongs.Rows.Count; i++)
                    {
                      strFName = DatabaseUtility.Get(resultSongs, i, "song.strFileName");
                      int lSongId = DatabaseUtility.GetAsInt(resultSongs, i, "song.idSong");
                      dwCRC = crc.calc(strPath + strFName);
                      strSQL = String.Format("update song set dwFileNameCRC='{0}' where idSong={1}",
                                dwCRC,
                                lSongId);
                      m_db.Execute(strSQL);
                    }
                  }
                  EmptyCache();
                }
                CommitTransaction();
                return true;
              }
              catch (Exception)
              {
                RollbackTransaction();
                Log.Info(LogType.MusicShareWatcher, "RenameSong: Rename for {0} failed because of DB exception", strPath);
                return false;
              }
            }
            return true;
          }
          else
          {
            return false;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(LogType.MusicShareWatcher, "musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        return false;
      }
    }

    /// <summary>
    /// A complete Path has been deleted. Now we need to remove all Songs for that path from the dB.
    /// </summary>
    /// <param name="strPath"></param>
    /// <returns></returns>
    public bool DeleteSongDirectory(string strPath)
    {
      try
      {
        DatabaseUtility.RemoveInvalidChars(ref strPath);

        string strSQL;
        strSQL = String.Format("select * from path where strPath like '{0}%'",
                strPath);

        // Get all songs and Path matching the deleted directory and remove them.
        SQLiteResultSet results;
        SQLiteResultSet resultSongs;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count > 0)
        {
          try
          {
            BeginTransaction();
            // We might have deleted a Top directory, so we get a lot of path entries returned
            for (int rownum = 0; rownum < results.Rows.Count; rownum++)
            {
              int lPathId = DatabaseUtility.GetAsInt(results, rownum, "path.idPath");
              string strSongPath = DatabaseUtility.Get(results, rownum, "path.strPath");
              // And now we need to remove the songs
              strSQL = String.Format("select * from song where idPath = {0}",
                           lPathId);
              resultSongs = m_db.Execute(strSQL);
              if (resultSongs.Rows.Count > 0)
              {
                for (int i = 0; i < resultSongs.Rows.Count; i++)
                {
                  string strFName = DatabaseUtility.Get(resultSongs, i, "song.strFileName");
                  DeleteSong(strSongPath + strFName, true);
                }
              }
              EmptyCache();
            }
            // And finally let's remove all the path information
            strSQL = String.Format("delete from path where strPath like '{0}%'",
                                strPath);
            results = m_db.Execute(strSQL);
            CommitTransaction();
            return true;
          }
          catch (Exception)
          {
            RollbackTransaction();
            Log.Error(LogType.MusicShareWatcher, "Delete Directory for {0} failed because of DB exception", strPath);
            return false;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(LogType.MusicShareWatcher, "musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        return false;
      }
      return true;
    }



    // by rtv

    public int AddScrobbleUser(string userName_)
    {
      string strSQL;
      try
      {
        if (userName_ == null)
          return -1;
        if (userName_.Length == 0)
          return -1;
        string strUserName = userName_;

        DatabaseUtility.RemoveInvalidChars(ref strUserName);
        if (null == m_db)
          return -1;

        SQLiteResultSet results;
        strSQL = String.Format("select * from scrobbleusers where strUsername like '{0}'", strUserName);
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0)
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into scrobbleusers (idScrobbleUser , strUsername) values ( NULL, '{0}' )", strUserName);
          m_db.Execute(strSQL);
          Log.Info("MusicDatabase: added scrobbleuser {0} with ID {1}", strUserName, Convert.ToString(m_db.LastInsertID()));
          return m_db.LastInsertID();
        }
        else
          return DatabaseUtility.GetAsInt(results, 0, "idScrobbleUser");
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public string AddScrobbleUserPassword(string userID_, string userPassword_)
    {
      string strSQL;
      try
      {
        if (userPassword_ == null || userID_ == null)
          return string.Empty;
        if (userID_.Length == 0)
          return string.Empty;
        string strUserPassword = userPassword_;

        DatabaseUtility.RemoveInvalidChars(ref strUserPassword);
        if (null == m_db)
          return string.Empty;

        SQLiteResultSet results;
        strSQL = String.Format("select * from scrobbleusers where idScrobbleUser = '{0}'", userID_);
        results = m_db.Execute(strSQL);
        // user doesn't exist therefore no password to change
        if (results.Rows.Count == 0)
          return string.Empty;

        if (DatabaseUtility.Get(results, 0, "strPassword") == strUserPassword)
          // password didn't change
          return userPassword_;
        // set new password
        else
        {
          // if no password was given = fetch it
          if (userPassword_ == "")
            return DatabaseUtility.Get(results, 0, "strPassword");
          else
          {
            strSQL = String.Format("update scrobbleusers set strPassword='{0}' where idScrobbleUser like '{1}'", strUserPassword, userID_);
            m_db.Execute(strSQL);
            return userPassword_;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return string.Empty;
    }

    public int AddScrobbleUserSettings(string userID_, string fieldName_, int fieldValue_)
    {
      string strSQL;
      string currentSettingID;
      try
      {
        if (fieldName_ == null || userID_ == null || userID_ == "-1")
          return -1;
        if (userID_.Length == 0 || fieldName_.Length == 0)
          return -1;

        SQLiteResultSet results;

        strSQL = String.Format("select idScrobbleSettings, idScrobbleUser from scrobblesettings where idScrobbleUser = '{0}'", userID_);
        results = m_db.Execute(strSQL);
        currentSettingID = DatabaseUtility.Get(results, 0, "idScrobbleSettings");
        //Log.Info("MusicDatabase: updating settings with ID {0}", currentSettingID);

        // setting doesn't exist - add it
        if (results.Rows.Count == 0)
        {
          strSQL = String.Format("insert into scrobblesettings (idScrobbleSettings, idScrobbleUser, " + fieldName_ + ") values ( NULL, '{0}', '{1}')", userID_, fieldValue_);
          m_db.Execute(strSQL);
          Log.Info("MusicDatabase: added scrobblesetting {0} for userid {1}", Convert.ToString(m_db.LastInsertID()), userID_);
          if (fieldValue_ > -1)
            return m_db.LastInsertID();
          else
            return fieldValue_;
        }
        else
        {
          strSQL = String.Format("select " + fieldName_ + " from scrobblesettings where idScrobbleSettings = '{0}'", currentSettingID);
          results = m_db.Execute(strSQL);

          if (DatabaseUtility.GetAsInt(results, 0, fieldName_) == fieldValue_)
            // setting didn't change
            return fieldValue_;
          // set new value
          else
          {
            // if no value was given = fetch it
            if (fieldValue_ == -1)
              return DatabaseUtility.GetAsInt(results, 0, fieldName_);
            else
            {
              strSQL = String.Format("update scrobblesettings set " + fieldName_ + "='{0}' where idScrobbleSettings like '{1}'", fieldValue_, currentSettingID);
              m_db.Execute(strSQL);
              return fieldValue_;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return -1;
    }

    public List<string> GetAllScrobbleUsers()
    {
      SQLiteResultSet results;
      List<string> scrobbleUsers = new List<string>();

      strSQL = "select * from scrobbleusers";
      results = m_db.Execute(strSQL);

      if (results.Rows.Count != 0)
      {
        for (int i = 0; i < results.Rows.Count; i++)
          scrobbleUsers.Add(DatabaseUtility.Get(results, i, "strUsername"));
      }
      // what else?

      return scrobbleUsers;
    }

    public bool DeleteScrobbleUser(string userName_)
    {
      string strSQL;
      int strUserID;
      try
      {
        if (userName_ == null)
          return false;
        if (userName_.Length == 0)
          return false;
        string strUserName = userName_;

        DatabaseUtility.RemoveInvalidChars(ref strUserName);
        if (null == m_db)
          return false;

        strUserID = AddScrobbleUser(strUserName);

        SQLiteResultSet results;
        strSQL = String.Format("delete from scrobblesettings where idScrobbleUser = '{0}'", strUserID);
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 1)
        {
          // setting removed now remove user
          strSQL = String.Format("delete from scrobbleusers where idScrobbleUser = '{0}'", strUserID);
          m_db.Execute(strSQL);
          return true;
        }
        else
        {
          Log.Error("MusicDatabase: could not delete settings for scrobbleuser {0} with ID {1}", strUserName, strUserID);
          return false;
        }
      }
      catch (Exception ex)
      {
        Log.Error("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        Open();
      }
      return false;
    }

  }
}
