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

using SQLite.NET;

using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.ServiceImplementations;
using MediaPortal.Util;


namespace MediaPortal.Music.Database
{
  /// <summary>
  /// Singleton class which establishes a database connection and provides lookup methods
  /// </summary>
  public partial class MusicDatabase
  {
    #region Variables
    public static readonly MusicDatabase Instance = new MusicDatabase();

    private SQLiteClient MusicDbClient = null;

    private static bool _treatFolderAsAlbum = false;
    private static bool _extractEmbededCoverArt = true;
    private static bool _useFolderThumbs = true;
    private static bool _useFolderArtForArtistGenre = false;
    private static bool _createMissingFolderThumbs = false;
    private static string _supportedExtensions = ".mp3,.wma,.ogg,.flac,.wav,.cda,.m3u,.pls,.b4s,.m4a,.m4p,.mp4,.wpl,.wv,.ape,.mpc";
    private static DateTime _lastImport;
    private static System.DateTime _currentDate = DateTime.Now;

    string[] ArtistNamePrefixes = new string[]
            {
              "the",
              "les",
              "die"
            };

    #endregion

    #region Constructors/Destructors
    /// <summary>
    /// static constructor. Opens or creates the music database
    /// </summary>
    static MusicDatabase()
    {
      // Log.Debug("MusicDatabase: static database constructor");
    }
    /// <summary>
    /// private constructor to prevent any instance of this class
    /// </summary>
    private MusicDatabase()
    {
      LoadDBSettings();
      Open();
    }

    ~MusicDatabase()
    {
    }
    #endregion

    #region Getters & Setters
    private SQLiteClient DbConnection
    {
      get
      {
        if (MusicDbClient == null)
          MusicDbClient = new SQLiteClient(Config.GetFile(Config.Dir.Database, "MusicDatabaseV10.db3"));

        return MusicDbClient;
      }
    }
    #endregion

    #region Functions
    public static SQLiteResultSet DirectExecute(string aSQL)
    {
      return Instance.DbConnection.Execute(aSQL);
    }

    private void LoadDBSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _treatFolderAsAlbum = xmlreader.GetValueAsBool("musicfiles", "treatFolderAsAlbum", false);
        _extractEmbededCoverArt = xmlreader.GetValueAsBool("musicfiles", "extractthumbs", true);
        _useFolderThumbs = xmlreader.GetValueAsBool("musicfiles", "useFolderThumbs", true);
        _createMissingFolderThumbs = xmlreader.GetValueAsBool("musicfiles", "createMissingFolderThumbs", false);
        _useFolderArtForArtistGenre = xmlreader.GetValueAsBool("musicfiles", "createartistgenrethumbs", false);
        _supportedExtensions = xmlreader.GetValueAsString("music", "extensions", ".mp3,.wma,.ogg,.flac,.wav,.cda,.m3u,.pls,.b4s,.m4a,.m4p,.mp4,.wpl,.wv,.ape,.mpc");

        try
        {
          string lastImport = xmlreader.GetValueAsString("musicfiles", "lastImport", "1900-01-01 00:00:00");
          _lastImport = DateTime.ParseExact(lastImport, "yyyy-M-d H:m:s", System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
          _lastImport = DateTime.ParseExact("1900-01-01 00:00:00", "yyyy-M-d H:m:s", System.Globalization.CultureInfo.InvariantCulture); ;
        }
      }
    }

    private void Open()
    {
      Log.Info("MusicDatabase: Opening database");

      try
      {
        // Open database
        try
        {
          System.IO.Directory.CreateDirectory(Config.GetFolder(Config.Dir.Database));
        }
        catch (Exception) { }

        // Get the DB handle or create it if necessary
        MusicDbClient = DbConnection;

        if (!System.IO.File.Exists(Config.GetFile(Config.Dir.Database, "MusicDatabaseV10.db3")))
        {
          // When we have deleted the database, we need to scan from the beginning, regardsless of the last import setting
          _lastImport = DateTime.ParseExact("1900-01-01 00:00:00", "yyyy-M-d H:m:s", System.Globalization.CultureInfo.InvariantCulture); ;

          if (!CreateDatabase())
            return;
        }
      }

      catch (Exception ex)
      {
        Log.Error("MusicDatabase: exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      Log.Info("MusicDatabase: Database opened");
    }

    private bool CreateDatabase()
    {
      try
      {
        DatabaseUtility.SetPragmas(MusicDbClient);

        // Tracks table containing information for songs 
        DatabaseUtility.AddTable(
              MusicDbClient, "tracks",
              @"CREATE TABLE tracks ( " +
          // Unique id Autoincremented
              "idTrack integer primary key autoincrement, " +
          // Full Path of the file. 
              "strPath text, " +
          // Artist
              "strArtist text, strArtistSortName text, strAlbumArtist text, " +
          // Album (How to handle Various Artist Albums)
              "strAlbum text, " +
          // Genre (multiple genres)
              "strGenre text, " +
          // Song
              "strTitle text, iTrack integer, iNumTracks integer, iDuration integer, iYear integer, " +
              "iTimesPlayed integer, iRating integer, iFavorite integer, iResumeAt integer, iDisc integer, iNumDisc integer, " +
              "iGainTrack double,  iPeakTrack double, " +
              "strLyrics text, musicBrainzID text, " +
              "dateLastPlayed timestamp, dateAdded timestamp" +
              ")"
              );

        // Add a Trigger for inserting the Date into the song table, whenever we do an update
        string strSQL = "CREATE TRIGGER IF NOT EXISTS insert_song_timeStamp AFTER INSERT ON tracks " +
                        "BEGIN " +
                        " UPDATE tracks SET dateAdded = DATETIME('NOW') " +
                        " WHERE rowid = new.rowid; " +
                        "END;";
        MusicDbClient.Execute(strSQL);

        // Indices for Tracks table
        DatabaseUtility.AddIndex(MusicDbClient, "idxpath_strPath", "CREATE INDEX idxpath_strPath ON tracks(strPath ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxartist_strArtist", "CREATE INDEX idxartist_strArtist ON tracks(strArtist ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxartist_strArtistSortName", "CREATE INDEX idxartist_strArtistSortName ON tracks(strArtistSortName ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbum_strAlbumArtist", "CREATE INDEX idxalbum_strAlbumArtist ON tracks(strAlbumArtist ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbum_strAlbum", "CREATE INDEX idxalbum_strAlbum ON tracks(strAlbum ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxgenre_strGenre", "CREATE INDEX idxgenre_strGenre ON tracks(strGenre ASC)");

        // Artist 
        DatabaseUtility.AddTable(MusicDbClient, "artist", "CREATE TABLE artist ( idArtist integer primary key autoincrement, strArtist text)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxartisttable_strArtist", "CREATE INDEX idxartisttable_strArtist ON artist(strArtist ASC)");

        // AlbumArtist 
        DatabaseUtility.AddTable(MusicDbClient, "albumartist", "CREATE TABLE albumartist ( idAlbumArtist integer primary key autoincrement, strAlbumArtist text)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbumartisttable_strAlbumArtist", "CREATE INDEX idxalbumartisttable_strAlbumArtist ON albumartist(strAlbumArtist ASC)");

        // Genre
        DatabaseUtility.AddTable(MusicDbClient, "genre", "CREATE TABLE genre ( idGenre integer primary key autoincrement, strGenre text)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxgenretable_strGenre", "CREATE INDEX idxgenretable_strGenre ON genre(strGenre ASC)");

        // Artist Info and Album Info
        DatabaseUtility.AddTable(MusicDbClient, "albuminfo", "CREATE TABLE albuminfo ( idAlbumInfo integer primary key autoincrement, strAlbum text, strArtist text, strAlbumArtist text,iYear integer, idGenre integer, strTones text, strStyles text, strReview text, strImage text, strTracks text, iRating integer)");
        DatabaseUtility.AddTable(MusicDbClient, "artistinfo", "CREATE TABLE artistinfo ( idArtistInfo integer primary key autoincrement, strArtist text, strBorn text, strYearsActive text, strGenres text, strTones text, strStyles text, strInstruments text, strImage text, strAMGBio text, strAlbums text, strCompilations text, strSingles text, strMisc text)");

        // Indices for Album and Artist Info
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbuminfo_strAlbum", "CREATE INDEX idxalbuminfo_strAlbum ON albuminfo(strAlbum ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbuminfo_strArtist", "CREATE INDEX idxalbuminfo_strArtist ON albuminfo(strArtist ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxalbuminfo_idGenre", "CREATE INDEX idxalbuminfo_idGenre ON albuminfo(idGenre ASC)");
        DatabaseUtility.AddIndex(MusicDbClient, "idxartistinfo_strArtist", "CREATE INDEX idxartistinfo_strArtist ON artistinfo(strArtist ASC)");

        // Scrobble table
        DatabaseUtility.AddTable(MusicDbClient, "scrobbleusers", "CREATE TABLE scrobbleusers ( idScrobbleUser integer primary key, strUsername text, strPassword text)");
        DatabaseUtility.AddTable(MusicDbClient, "scrobblesettings", "CREATE TABLE scrobblesettings ( idScrobbleSettings integer primary key, idScrobbleUser integer, iAddArtists integer, iAddTracks integer, iNeighbourMode integer, iRandomness integer, iScrobbleDefault integer, iSubmitOn integer, iDebugLog integer, iOfflineMode integer, iPlaylistLimit integer, iPreferCount integer, iRememberStartArtist integer, iAnnounce integer)");
        DatabaseUtility.AddTable(MusicDbClient, "scrobblemode", "CREATE TABLE scrobblemode ( idScrobbleMode integer primary key, idScrobbleUser integer, iSortID integer, strModeName text)");
        DatabaseUtility.AddTable(MusicDbClient, "scrobbletags", "CREATE TABLE scrobbletags ( idScrobbleTag integer primary key, idScrobbleMode integer, iSortID integer, strTagName text)");

        Log.Info("MusicDatabase: New Database created successfully");
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: Create of database failed. Err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return false;
    }
    #endregion

    #region Transactions
    public void BeginTransaction()
    {
      try
      {
        MusicDatabase.DirectExecute("begin");
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: BeginTransaction: musicdatabase begin transaction failed exception err:{0} ", ex.Message);
        //Open();
      }
    }

    public void CommitTransaction()
    {
      Log.Debug("MusicDatabase: Commit will effect {0} rows", MusicDatabase.Instance.DbConnection.ChangedRows());
      SQLiteResultSet CommitResults;
      try
      {
        CommitResults = MusicDatabase.DirectExecute("commit");
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: Commit failed exception err:{0} ", ex.Message);
        Open();
      }
    }

    public void RollbackTransaction()
    {
      try
      {
        MusicDatabase.DirectExecute("rollback");
      }
      catch (Exception ex)
      {
        Log.Error("MusicDatabase: Rollback failed exception err:{0} ", ex.Message);
        Open();
      }
    }
    #endregion
  }
}
