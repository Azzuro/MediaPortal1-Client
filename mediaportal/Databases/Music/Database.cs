using System;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using System.Collections;
using SQLite.NET;

namespace MediaPortal.Music.Database
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
  public class Database
  {
    public class CArtistCache
    {
      public int idArtist = 0;
      public string strArtist = "";
    };

    public class CPathCache
    {
      public int idPath = 0;
      public string strPath = "";
    };

    public class CGenreCache
    {
      public int idGenre = 0;
      public string strGenre = "";
    };

    public class AlbumInfoCache : AlbumInfo
    {
      public int idAlbum = 0;
      public int idArtist = 0;
    };

    public class ArtistInfoCache : ArtistInfo
    {
      public int idArtist = 0;
    }

    
    ArrayList m_artistCache = new ArrayList();
    ArrayList m_genreCache = new ArrayList();
    ArrayList m_pathCache = new ArrayList();
    ArrayList m_albumCache = new ArrayList();

    static SQLiteClient m_db = null;
    public Database()
    {
      try 
      {
        // Open database
        System.IO.Directory.CreateDirectory("database");
        m_db = new SQLiteClient(@"database\musicdatabase2.db");
        CreateTables();

        m_db.Execute("PRAGMA cache_size=8192\n");
        m_db.Execute("PRAGMA synchronous='OFF'\n");
				m_db.Execute("PRAGMA count_changes='OFF'\n");
      } 
      catch (SQLiteException ex) 
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }
    ~Database()
    {
    
    }

		static public SQLiteClient DBHandle
		{
			get { return m_db; }
		}

    public bool		Open()
    {
      return true;
    }

    public void	Close()
    {
      /*if (m_db!=null)
      {
        m_db.Close();
        m_db=null;
      }*/
    }
    void AddTable(string strTable, string strSQL)
    {
      if (m_db == null) return;
      SQLiteResultSet results;
      results = m_db.Execute("SELECT name FROM sqlite_master WHERE name='" + strTable + "' and type='table' UNION ALL SELECT name FROM sqlite_temp_master WHERE type='table' ORDER BY name");
      if (results != null && results.Rows.Count > 0) 
      {
				
        if (results.Rows.Count == 1) 
        {
          ArrayList arr = (ArrayList)results.Rows[0];
          if (arr.Count == 1)
          {
            if ((string)arr[0] == strTable) 
            {
              return;
            }
          }
        }
      }

      try 
      {
        m_db.Execute(strSQL);
      }
      catch (SQLiteException ex) 
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return;
    }

    bool CreateTables()
    {
      if (m_db == null) return false;
      AddTable("artist","CREATE TABLE artist ( idArtist integer primary key, strArtist text)\n");
      AddTable("album","CREATE TABLE album ( idAlbum integer primary key, idArtist integer, strAlbum text)\n");
      AddTable("genre","CREATE TABLE genre ( idGenre integer primary key, strGenre text)\n");
      AddTable("path","CREATE TABLE path ( idPath integer primary key,  strPath text)\n");
      AddTable("albuminfo","CREATE TABLE albuminfo ( idAlbumInfo integer primary key, idAlbum integer, idArtist integer,iYear integer, idGenre integer, strTones text, strStyles text, strReview text, strImage text, strTracks text, iRating integer)\n");
			AddTable("artistinfo","CREATE TABLE artistinfo ( idArtistInfo integer primary key, idArtist integer, strBorn text, strYearsActive text, strGenres text, strTones text, strStyles text, strInstruments text, strImage text, strAMGBio text, strAlbums text, strCompilations text, strSingles text, strMisc text)\n");
      AddTable("song","CREATE TABLE song ( idSong integer primary key, idArtist integer, idAlbum integer, idGenre integer, idPath integer, strTitle text, iTrack integer, iDuration integer, iYear integer, dwFileNameCRC text, strFileName text, iTimesPlayed integer)\n");
			return true;
    }

    public string Get(SQLiteResultSet results, int iRecord, string strColum)
    {
      if (null == results) return "";
      if (results.Rows.Count < iRecord) return "";
      ArrayList arr = (ArrayList)results.Rows[iRecord];
      int iCol = 0;
      foreach (string columnName in results.ColumnNames)
      {
        if (strColum == columnName)
        {
          string strLine = ((string)arr[iCol]).Trim();
          strLine = strLine.Replace("''","'");
          return strLine;
        }
        iCol++;
      }
      return "";
    }

    public void RemoveInvalidChars(ref string strTxt)
    {
      string strReturn = "";
      for (int i = 0; i < (int)strTxt.Length; ++i)
      {
        char k = strTxt[i];
        if (k == '\'') 
        {
          strReturn += "'";
        }
        strReturn += k;
      }
      if (strReturn == "") 
        strReturn = "unknown";
      strTxt = strReturn.Trim();
    }
    public void Split(string strFileNameAndPath, out string strPath, out string strFileName)
    {
      strFileNameAndPath = strFileNameAndPath.Trim();
      strFileName = "";
      strPath = "";
      if (strFileNameAndPath.Length == 0) return;
      int i = strFileNameAndPath.Length - 1;
      while (i > 0)
      {
        char ch = strFileNameAndPath[i];
        if (ch == ':' || ch == '/' || ch == '\\') break;
        else i--;
      }
      strPath = strFileNameAndPath.Substring(0, i).Trim();
      strFileName = strFileNameAndPath.Substring(i, strFileNameAndPath.Length - i).Trim();
    }

    public int AddPath(string strPath1)
    {
      string strSQL;
      try
      {
        if (strPath1 == null) return - 1;
        if (strPath1.Length == 0) return - 1;
        string strPath = strPath1;
        //	musicdatabase always stores directories 
        //	without a slash at the end 
        if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
          strPath = strPath.Substring(0, strPath.Length - 1);
        RemoveInvalidChars(ref strPath);

        if (null == m_db) return - 1;

        foreach (CPathCache path in m_pathCache)
        {
          if (path.strPath == strPath1)
          {
            return path.idPath;
          }
        }

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
          m_pathCache.Add(path);
          return path.idPath;
        }
        else
        {
          CPathCache path = new CPathCache();
          path.idPath = Int32.Parse(Get(results, 0, "idPath"));
          path.strPath = strPath1;
          m_pathCache.Add(path);
          return path.idPath;
        }
      } 
      catch (SQLiteException ex) 
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

      return - 1;
    }

    public int AddArtist(string strArtist1)
    {
      string strSQL;
      try 
      {
        string strArtist = strArtist1;
        RemoveInvalidChars(ref strArtist);

        if (null == m_db) return - 1;
        foreach (CArtistCache artist in m_artistCache)
        {
          if (artist.strArtist == strArtist1)
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
          m_artistCache.Add(artist);
          return artist.idArtist;
        }
        else
        {
          CArtistCache artist = new CArtistCache();
          artist.idArtist = Int32.Parse(Get(results, 0, "idArtist"));
          artist.strArtist = strArtist1;
          m_artistCache.Add(artist);
          return artist.idArtist;
        }
      }
      catch (SQLiteException ex) 
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

      return - 1;
    }

    public int AddGenre(string strGenre1)
    {
      string strSQL;
      try
      {
        string strGenre = strGenre1;
        RemoveInvalidChars(ref strGenre);

        if (null == m_db) return - 1;
        foreach (CGenreCache genre in m_genreCache)
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
          m_genreCache.Add(genre);
          return genre.idGenre;
        }
        else
        {
          CGenreCache genre = new CGenreCache();
          genre.idGenre = Int32.Parse(Get(results, 0, "idGenre"));
          genre.strGenre = strGenre1;
          m_genreCache.Add(genre);
          return genre.idGenre;
        }
      }
      catch (SQLiteException ex) 
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

      return - 1;
    }


    public int AddAlbum(string strAlbum1, int lArtistId)
    {
      string strSQL;
      try
      {
        string strAlbum = strAlbum1;
        RemoveInvalidChars(ref strAlbum);

        if (null == m_db) return - 1;
        foreach (AlbumInfoCache album in m_albumCache)
        {
          if (strAlbum1 == album.Album)
            return album.idAlbum;
        }

        strSQL = String.Format("select * from album where strAlbum like '{0}'", strAlbum);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);

        if (results.Rows.Count == 0) 
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into album (idAlbum, strAlbum,idArtist) values( NULL, '{0}', {1})", strAlbum, lArtistId);
          m_db.Execute(strSQL);

          AlbumInfoCache album = new AlbumInfoCache();
          album.idAlbum = m_db.LastInsertID();
          album.Album = strAlbum1;
          album.idArtist = lArtistId;
          m_albumCache.Add(album);
          return album.idAlbum;
        }
        else
        {
          AlbumInfoCache album = new AlbumInfoCache();
          album.idAlbum = Int32.Parse(Get(results, 0, "idAlbum"));
          album.Album = strAlbum1;
          album.idArtist = Int32.Parse(Get(results, 0, "idArtist"));
          m_albumCache.Add(album);
          return album.idAlbum;
        }
      }
      catch (SQLiteException ex) 
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

      return - 1;
    }

    public void EmptyCache()
    {
      m_artistCache.Clear();
      m_genreCache.Clear();
      m_pathCache.Clear();
      m_albumCache.Clear();
    }

    public bool IsOpen
    {
      get { return m_db != null; }
    }


    public void AddSong(Song song1, bool bCheck)
    {
      //Log.Write("database.AddSong {0} {1} {2}  {3}", song1.FileName,song1.Album, song1.Artist, song1.Title);
      string strSQL;
      try
      {
        Song song = song1.Clone();
        string strTmp;
        strTmp = song.Album; RemoveInvalidChars(ref strTmp); song.Album = strTmp;
        strTmp = song.Genre; RemoveInvalidChars(ref strTmp); song.Genre = strTmp;
        strTmp = song.Artist; RemoveInvalidChars(ref strTmp); song.Artist = strTmp;
        strTmp = song.Title; RemoveInvalidChars(ref strTmp); song.Title = strTmp;

        string strPath, strFileName;
        Split(song1.FileName, out strPath, out strFileName);
        RemoveInvalidChars(ref strFileName);

        if (null == m_db) return;
        int lGenreId = AddGenre(song1.Genre);
        int lArtistId = AddArtist(song1.Artist);
        int lPathId = AddPath(strPath);
        int lAlbumId = AddAlbum(song1.Album, lArtistId);

        ulong dwCRC = 0;
        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
        dwCRC = crc.calc(song1.FileName);
        SQLiteResultSet results;

        if (bCheck)
        {
          strSQL = String.Format("select * from song where idAlbum={0} AND idGenre={1} AND idArtist={2} AND dwFileNameCRC='{3}' AND strTitle='{4}'", 
                                lAlbumId, lGenreId, lArtistId, dwCRC, song.Title);
          results = m_db.Execute(strSQL);

          if (results.Rows.Count != 0)  return;
        }
    		
        strSQL = String.Format("insert into song (idSong,idArtist,idAlbum,idGenre,idPath,strTitle,iTrack,iDuration,iYear,dwFileNameCRC,strFileName,iTimesPlayed) values(NULL,{0},{1},{2},{3},'{4}',{5},{6},{7},'{8}','{9}',{10})",
          lArtistId, lAlbumId, lGenreId, lPathId, 
          song.Title, 
          song.Track, song.Duration, song.Year, 
          dwCRC, 
          strFileName, 0);

        m_db.Execute(strSQL);
      }
      catch (SQLiteException ex) 
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    public bool GetSongByFileName(string strFileName1, ref Song song)
    {
	    try
	    {
		    song.Clear();
		    string strFileName = strFileName1;
		    RemoveInvalidChars(ref strFileName);

		    string strPath, strFName;
		    Split(strFileName, out strPath, out strFName);

		    if (null == m_db) return false;

        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
        ulong dwCRC = crc.calc(strFileName1);

        string strSQL;
		    strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and dwFileNameCRC='{0}' and strPath='{1}'",
										          dwCRC, 
										          strPath);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return false;
		    song.Artist = Get(results, 0, "artist.strArtist");
		    song.Album = Get(results, 0, "album.strAlbum");
		    song.Genre = Get(results, 0, "genre.strGenre");
		    song.Track = Int32.Parse(Get(results, 0, "song.iTrack"));
		    song.Duration = Int32.Parse(Get(results, 0, "song.iDuration"));
		    song.Year = Int32.Parse(Get(results, 0, "song.iYear"));
		    song.Title = Get(results, 0, "song.strTitle");
		    song.TimesPlayed = Int32.Parse(Get(results, 0, "song.iTimesPlayed"));
		    song.FileName = strFileName1;
		    return true;
      }
      catch (SQLiteException ex) 
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

	    return false;
    }

    public bool GetSong(string strTitle1, ref Song song)
    {
	    try
	    {
		    song.Clear();
		    string strTitle = strTitle1;
		    RemoveInvalidChars(ref strTitle);

		    if (null == m_db) return false;

		    string strSQL;
		    strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle='{0}'",strTitle);

        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return false;

		    song.Artist = Get(results, 0, "artist.strArtist");
		    song.Album = Get(results, 0, "album.strAlbum");
		    song.Genre = Get(results, 0, "genre.strGenre");
		    song.Track = Int32.Parse(Get(results, 0, "song.iTrack"));
		    song.Duration = Int32.Parse(Get(results, 0, "song.iDuration"));
		    song.Year = Int32.Parse(Get(results, 0, "song.iYear"));
		    song.Title = Get(results, 0, "song.strTitle");
		    song.TimesPlayed = Int32.Parse(Get(results, 0, "song.iTimesPlayed"));

		    string strFileName = Get(results, 0, "path.strPath");
		    strFileName += Get(results, 0, "song.strFileName");
		    song.FileName = strFileName;
		    return true;
      }
      catch (SQLiteException ex) 
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

	    return false;
    }
//added by Sam
	public bool GetRandomSong(ref Song song)
	  {
		  try
		  {
			  song.Clear();

			  if (null == m_db) return false;

			  string strSQL;
			  int maxIDSong, rndIDSong;
			  strSQL = String.Format("select * from song ORDER BY idSong DESC LIMIT 1");
			  SQLiteResultSet results;
			  results = m_db.Execute(strSQL);
			  maxIDSong = Int32.Parse(Get(results,0,"idSong"));
			  rndIDSong = new System.Random().Next(maxIDSong);

			  strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and idSong={0}",rndIDSong);

			  results = m_db.Execute(strSQL);
			  if (results.Rows.Count > 0) 
			  {
				  song.Artist = Get(results, 0, "artist.strArtist");
				  song.Album = Get(results, 0, "album.strAlbum");
				  song.Genre = Get(results, 0, "genre.strGenre");
				  song.Track = Int32.Parse(Get(results, 0, "song.iTrack"));
				  song.Duration = Int32.Parse(Get(results, 0, "song.iDuration"));
				  song.Year = Int32.Parse(Get(results, 0, "song.iYear"));
				  song.Title = Get(results, 0, "song.strTitle");
				  song.TimesPlayed = Int32.Parse(Get(results, 0, "song.iTimesPlayed"));

				  string strFileName = Get(results, 0, "path.strPath");
				  strFileName += Get(results, 0, "song.strFileName");
				  song.FileName = strFileName;
				  return true;
			  }
			  else
			  {
				  GetRandomSong(ref song);
				  return true;
			  }
			  
		  }
		  catch (SQLiteException ex) 
		  {
			  Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
		  }

		  return false;
	  }

	  //added by Sam
	  public int GetNumOfSongs()
	  {
		try
		  {
			  if (null == m_db) return 0;

			  string strSQL;
			  int NumOfSongs;
			  strSQL = String.Format("select count(*) from song");
			  SQLiteResultSet results;
			  results = m_db.Execute(strSQL);
			  ArrayList row = (ArrayList)results.Rows[0];
			  NumOfSongs = Int32.Parse((string)row[0]);
			  return NumOfSongs;
		  }
		  catch (SQLiteException ex) 
		  {
			  Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
		  }

	  return 0;

	  }
    
	  //added by Sam
	  public bool GetAllSongs(ref ArrayList songs)
	  {
		  try
		  {
			  if (null == m_db) return false;

			  string strSQL;
			  SQLiteResultSet results;
			  strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist");

			  results = m_db.Execute(strSQL);
			  Song song;

			  for (int i=0; i<results.Rows.Count; i++)
			  {
				song = new Song();
		        song.Artist = Get(results, i, "artist.strArtist");
				song.Album = Get(results, i, "album.strAlbum");
				song.Genre = Get(results, i, "genre.strGenre");
				song.Track = Int32.Parse(Get(results, i, "song.iTrack"));
				song.Duration = Int32.Parse(Get(results, i, "song.iDuration"));
				song.Year = Int32.Parse(Get(results, i, "song.iYear"));
				song.Title = Get(results, i, "song.strTitle");
				song.TimesPlayed = Int32.Parse(Get(results, i, "song.iTimesPlayed"));

				string strFileName = Get(results, i, "path.strPath");
				strFileName += Get(results, i, "song.strFileName");
				song.FileName = strFileName;
				songs.Add(song);
			  }	  

			  return true;
		  }
		  catch (SQLiteException ex) 
		  {
			  Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
		  }

		  return false;
	  }
    public bool GetSongsByArtist(string strArtist1, ref ArrayList songs)
    {
	    try
	    {
		    string strArtist = strArtist1;
		    RemoveInvalidChars(ref strArtist);

		    songs.Clear();
		    if (null == m_db) return false;
    		
		    string strSQL;
		    strSQL = String.Format("select song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and artist.strArtist like '{0}'",strArtist);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
		    if (results.Rows.Count == 0) return false;
		    for (int i = 0; i < results.Rows.Count; ++i)
		    {
			    Song song = new Song();
			    song.Artist = Get(results, i, "artist.strArtist");
			    song.Album = Get(results, i, "album.strAlbum");
			    song.Genre = Get(results, i, "genre.strGenre");
			    song.Track = Int32.Parse(Get(results, i, "song.iTrack"));
			    song.Duration = Int32.Parse(Get(results, i, "song.iDuration"));
			    song.Year = Int32.Parse(Get(results, i, "song.iYear"));
			    song.Title = Get(results, i, "song.strTitle");
			    song.TimesPlayed = Int32.Parse(Get(results, i, "song.iTimesPlayed"));
			    string strFileName = Get(results, i, "path.strPath");
			    strFileName += Get(results, i, "song.strFileName");
			    song.FileName = strFileName;
			    songs.Add(song);
		    }

		    return true;
	    }
      catch (SQLiteException ex) 
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

	    return false;
    }

    public bool GetSongsByAlbum(string strAlbum1, ref ArrayList songs)
    {
	    try
	    {
        
		    string strAlbum = strAlbum1;
		    //	musicdatabase always stores directories 
		    //	without a slash at the end 

		    RemoveInvalidChars(ref strAlbum);

		    songs.Clear();
		    if (null == m_db) return false;
    		
		    string strSQL;
		    strSQL = String.Format("select song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and album.strAlbum like '{0}' and path.idPath=song.idPath order by song.iTrack", strAlbum);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return false;
		    for (int i = 0; i < results.Rows.Count; ++i)
		    {
			    Song song = new Song();
			    song.Artist = Get(results, i, "artist.strArtist");
			    song.Album = Get(results, i, "album.strAlbum");
			    song.Genre = Get(results, i, "genre.strGenre");
			    song.Track = Int32.Parse(Get(results, i, "song.iTrack"));
			    song.Duration = Int32.Parse(Get(results, i, "song.iDuration"));
			    song.Year = Int32.Parse(Get(results, i, "song.iYear"));
			    song.Title = Get(results, i, "song.strTitle");
			    song.TimesPlayed = Int32.Parse(Get(results, i, "song.iTimesPlayed"));

			    string strFileName = Get(results, i, "path.strPath");
			    strFileName += Get(results, i, "song.strFileName");
			    song.FileName = strFileName;

			    songs.Add(song);
		    }

		    return true;
      }
      catch (SQLiteException ex) 
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

	  return false;

    }
	  //
	  public bool GetSongs(int searchKind,string strTitle1,ref ArrayList songs)
	  {
		  try
		  {
			  songs.Clear();
			  string strTitle=strTitle1;
			  if (null == m_db) 
				  return false;
    		
			  string strSQL="";
			  switch (searchKind)
			  {
				  case 0:
					  strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle like '{0}%'",strTitle);
					  break;
				  case 1:
					  strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle like '%{0}%'",strTitle);
					  break;
				  case 2:
					  strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle like '%{0}'",strTitle);
					  break;
				  case 3:
					  strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and strTitle like '{0}'",strTitle);
					  break;
			  }
			  
			  SQLiteResultSet results;
			  results = m_db.Execute(strSQL);
			  if (results.Rows.Count == 0) 
				  return false;
			  for (int i = 0; i < results.Rows.Count; ++i)
			  {
				  string strFileName=Get(results, i, "path.strPath");
				  strFileName += Get(results, i, "song.strFileName");
				  GUIListItem item = new GUIListItem();
				  item.IsFolder = false;
				  item.Label = Utils.GetFilename(strFileName);
				  item.Label2 = "";
				  item.Label3="";
				  item.Path = strFileName;
				  item.FileInfo = new System.IO.FileInfo(strFileName);
				  Utils.SetDefaultIcons(item);
				  Utils.SetThumbnails(ref item);
				  songs.Add(item);
			  }
			  return true;
		  }
		  catch (SQLiteException ex) 
		  {
			  Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
		  }
		  return false;
	  }
	  //
	  public bool GetArtists(int searchKind,string strArtist1,ref ArrayList artists)
	  {
		  try
		  {
			  artists.Clear();
			  string strArtist2=strArtist1;
			  if (null == m_db) return false;
    		

			  // Exclude "Various Artists"
			  string strVariousArtists = GUILocalizeStrings.Get(340);
			  long lVariousArtistId = AddArtist(strVariousArtists);
			  string strSQL="";
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
			  }

			  SQLiteResultSet results;
			  results = m_db.Execute(strSQL);
			  if (results.Rows.Count == 0) return false;
			  for (int i = 0; i < results.Rows.Count; ++i)
			  {
				  string strArtist = Get(results, i, "strArtist");
				  artists.Add(strArtist);
			  }

			  return true;
		  }
		  catch (SQLiteException ex) 
		  {
			  Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
		  }

		  return false;
	  }
	  //
    public bool GetArtists(ref ArrayList artists)
    {
	    try
	    {
		    artists.Clear();

		    if (null == m_db) return false;
    		

		    // Exclude "Various Artists"
		    string strVariousArtists = GUILocalizeStrings.Get(340);
		    long lVariousArtistId = AddArtist(strVariousArtists);
		    string strSQL;
		    strSQL = String.Format("select * from artist where idArtist <> {0} ", lVariousArtistId);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
			    string strArtist = Get(results, i, "strArtist");
			    artists.Add(strArtist);
		    }

		    return true;
	    }
      catch (SQLiteException ex) 
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

	    return false;
    }

    public bool GetAlbums(ref ArrayList albums)
    {
      try
      {
        albums.Clear();
        if (null == m_db) return false;
    		
        string strSQL;
        strSQL = String.Format("select * from album,artist where album.idArtist=artist.idArtist");
        //strSQL=String.Format("select distinct album.idAlbum, album.idArtist, album.strAlbum, artist.idArtist, artist.strArtist from album,artist,song where song.idArtist=artist.idArtist and song.idAlbum=album.idAlbum");
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
			    AlbumInfo album = new AlbumInfo();
			    album.Album = Get(results, i, "album.strAlbum");
			    album.Artist = Get(results, i, "artist.strArtist");
			    albums.Add(album);
		    }
		    return true;
	    }
	    catch (SQLiteException ex)
	    {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
	    }

	    return false;
    }
	  public bool GetAlbums(int searchKind,string strAlbum1,ref ArrayList albums)
	  {
		  try
		  {
			  string strAlbum=strAlbum1;
			  albums.Clear();
			  if (null == m_db) return false;
    		
			  string strSQL="";
			  switch (searchKind)
			  {
				  case 0:
					  strSQL = String.Format("select * from album,artist where album.idArtist=artist.idArtist and album.strAlbum like '{0}%'",strAlbum);
					  break;
				  case 1:
					  strSQL = String.Format("select * from album,artist where album.idArtist=artist.idArtist and album.strAlbum like '%{0}%'",strAlbum);
					  break;
				  case 2:
					  strSQL = String.Format("select * from album,artist where album.idArtist=artist.idArtist and album.strAlbum like '%{0}'",strAlbum);
					  break;
				  case 3:
					  strSQL = String.Format("select * from album,artist where album.idArtist=artist.idArtist and album.strAlbum like '{0}'",strAlbum);
					  break;
			  }
					  //strSQL=String.Format("select distinct album.idAlbum, album.idArtist, album.strAlbum, artist.idArtist, artist.strArtist from album,artist,song where song.idArtist=artist.idArtist and song.idAlbum=album.idAlbum");
			  SQLiteResultSet results;
			  results = m_db.Execute(strSQL);
			  if (results.Rows.Count == 0) return false;
			  for (int i = 0; i < results.Rows.Count; ++i)
			  {
				  AlbumInfo album = new AlbumInfo();
				  album.Album = Get(results, i, "album.strAlbum");
				  album.Artist = Get(results, i, "artist.strArtist");
				  albums.Add(album);
			  }
			  return true;
		  }
		  catch (SQLiteException ex)
		  {
			  Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
		  }

		  return false;
	  }
    public bool GetGenres(ref ArrayList genres)
    {
	    try
	    {
		    genres.Clear();
		    if (null == m_db) return false;
		    string strSQL;
		    strSQL = String.Format("select * from genre");
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return false;
        for (int i = 0; i < results.Rows.Count; ++i)
		    {
			    string strGenre = Get(results, i, "strGenre");
			    genres.Add(strGenre);
		    }

		    return true;
	    }
	    catch (SQLiteException ex)
	    {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return false;
    }
	  //
	  public bool GetGenres(int searchKind,string strGenere1,ref ArrayList genres)
	  {
		  try
		  {
			  genres.Clear();
			  string strGenere=strGenere1;
			  if (null == m_db) return false;
			  string strSQL="";
			  switch (searchKind)
			  {
				  case 0:
					  strSQL = String.Format("select * from genre where strGenre like '{0}%'",strGenere);
					  break;
				  case 1:
					  strSQL = String.Format("select * from genre where strGenre like '%{0}%'",strGenere);
					  break;
				  case 2:
					  strSQL = String.Format("select * from genre where strGenre like '%{0}'",strGenere);
					  break;
				  case 3:
					  strSQL = String.Format("select * from genre where strGenre like '{0}'",strGenere);
					  break;
			  }
			  SQLiteResultSet results;
			  results = m_db.Execute(strSQL);
			  if (results.Rows.Count == 0) return false;
			  for (int i = 0; i < results.Rows.Count; ++i)
			  {
				  string strGenre = Get(results, i, "strGenre");
				  genres.Add(strGenre);
			  }

			  return true;
		  }
		  catch (SQLiteException ex)
		  {
			  Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
		  }
		  return false;
	  }
	  //
    public bool GetSongsByGenre(string strGenre, ref ArrayList songs)
    {
	    try
	    {
		    string strSQLGenre = strGenre;
		    RemoveInvalidChars(ref strSQLGenre);

		    songs.Clear();
		    if (null == m_db) return false;
    		
		    string strSQL;
		    strSQL = String.Format("select song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,genre,album,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and genre.strGenre like '{0}'",strSQLGenre);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) return false;
        for (int i = 0; i < results.Rows.Count; ++i)
        {
			    Song song = new Song();
			    song.Artist = Get(results, i, "artist.strArtist");
			    song.Album = Get(results, i, "album.strAlbum");
			    song.Genre = Get(results, i, "genre.strGenre");
			    song.Track = Int32.Parse(Get(results, i, "song.iTrack"));
			    song.Duration = Int32.Parse(Get(results, i, "song.iDuration"));
			    song.Year = Int32.Parse(Get(results, i, "song.iYear"));
			    song.Title = Get(results, i, "song.strTitle");
			    song.TimesPlayed = Int32.Parse(Get(results, i, "song.iTimesPlayed"));
    			
			    string strFileName = Get(results, i, "path.strPath");
			    strFileName += Get(results, i, "song.strFileName");
			    song.FileName = strFileName;

			    songs.Add(song);
		    }

		    return true;
	    }
	    catch (SQLiteException ex)
	    {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

	    return false;

    }
	  public bool GetTop100(int searchKind,string strGenere1,ref ArrayList songs)
	  {
		  try
		  {
			  songs.Clear();
			  string strGenere=strGenere1;

			  if (null == m_db) return false;
				
			  string strSQL="";
			  switch (searchKind)
			  {
				  case 0:
					  strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and song.strTitle like '{0}%' and iTimesPlayed > 0 order by song.iTimesPlayed desc limit 100",strGenere);
					  break;
				  case 1:
					  strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and song.strTitle like '%{0}%' and iTimesPlayed > 0 order by song.iTimesPlayed desc limit 100",strGenere);
					  break;
				  case 2:
					  strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and song.strTitle like '%{0}' and iTimesPlayed > 0 order by song.iTimesPlayed desc limit 100",strGenere);
					  break;
				  case 3:
					  strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and song.strTitle like '{0}' and iTimesPlayed > 0 order by song.iTimesPlayed desc limit 100",strGenere);
					  break;
			  }

			  SQLiteResultSet results;
			  results = m_db.Execute(strSQL);
			  if (results.Rows.Count == 0) return false;
			  for (int i = 0; i < results.Rows.Count; ++i)
			  {
				  Song song = new Song();
				  song.Artist = Get(results, i, "artist.strArtist");
				  song.Album = Get(results, i, "album.strAlbum");
				  song.Genre = Get(results, i, "genre.strGenre");
				  song.Track = Int32.Parse(Get(results, i, "song.iTrack"));
				  song.Duration = Int32.Parse(Get(results, i, "song.iDuration"));
				  song.Year = Int32.Parse(Get(results, i, "song.iYear"));
				  song.Title = Get(results, i, "song.strTitle");
				  song.TimesPlayed = Int32.Parse(Get(results, i, "song.iTimesPlayed"));
				  song.Track = i;
					
				  string strFileName = Get(results, i, "path.strPath");
				  strFileName += Get(results, i, "song.strFileName");
				  song.FileName = strFileName;
				  songs.Add(song);
			  }

			  return true;
		  }
		  catch (SQLiteException ex)
		  {
			  Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
		  }

		  return false;
	  }

		public bool GetTop100(ref ArrayList songs)
		{
			try
			{
				songs.Clear();
				if (null == m_db) return false;
				
				string strSQL;
				strSQL = String.Format("select * from song,album,genre,artist,path where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and iTimesPlayed > 0 order by song.iTimesPlayed desc limit 100");
				SQLiteResultSet results;
				results = m_db.Execute(strSQL);
				if (results.Rows.Count == 0) return false;
				for (int i = 0; i < results.Rows.Count; ++i)
				{
					Song song = new Song();
					song.Artist = Get(results, i, "artist.strArtist");
					song.Album = Get(results, i, "album.strAlbum");
					song.Genre = Get(results, i, "genre.strGenre");
					song.Track = Int32.Parse(Get(results, i, "song.iTrack"));
					song.Duration = Int32.Parse(Get(results, i, "song.iDuration"));
					song.Year = Int32.Parse(Get(results, i, "song.iYear"));
					song.Title = Get(results, i, "song.strTitle");
					song.TimesPlayed = Int32.Parse(Get(results, i, "song.iTimesPlayed"));
					song.Track = i;
					
					string strFileName = Get(results, i, "path.strPath");
					strFileName += Get(results, i, "song.strFileName");
					song.FileName = strFileName;
					songs.Add(song);
				}

				return true;
			}
			catch (SQLiteException ex)
			{
				Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
			}

			return false;
		}

		public bool GetSongsByPath(string strPath1, ref ArrayList songs)
		{
			try
			{
        songs.Clear();
        if (strPath1 == null) return false;
        if (strPath1.Length == 0) return false;
				string strPath = strPath1;
				//	musicdatabase always stores directories 
				//	without a slash at the end 
				if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
					strPath = strPath.Substring(0, strPath.Length - 1);
				RemoveInvalidChars(ref strPath);
				if (null == m_db) return false;
				
				string strSQL;
				strSQL = String.Format("select * from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and path.strPath like '{0}'",strPath);
				SQLiteResultSet results;
				results = m_db.Execute(strSQL);
				if (results.Rows.Count == 0) return false;
				for (int i = 0; i < results.Rows.Count; ++i)
				{
					Song song = new Song();
					song.Artist = Get(results, i, "artist.strArtist");
					song.Album = Get(results, i, "album.strAlbum");
					song.Genre = Get(results, i, "genre.strGenre");
					song.Track = Int32.Parse(Get(results, i, "song.iTrack"));
					song.Duration = Int32.Parse(Get(results, i, "song.iDuration"));
					song.Year = Int32.Parse(Get(results, i, "song.iYear"));
					song.Title = Get(results, i, "song.strTitle");
					song.TimesPlayed = Int32.Parse(Get(results, i, "song.iTimesPlayed"));
					
					string strFileName = Get(results, i, "path.strPath");
					strFileName += Get(results, i, "song.strFileName");
					song.FileName = strFileName;
					
					songs.Add(song);
				}
				return true;
			}
			catch (SQLiteException ex)
			{
				Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
			}
			return false;
		}

		
		public bool GetRecentlyPlayedAlbums(ref ArrayList albums)
    {
			try
			{
				albums.Clear();
				if (null == m_db) return false;
				
				string strSQL;
				strSQL = String.Format("select distinct album.*, artist.*, path.* from album,artist,path,song where album.idAlbum=song.idAlbum and album.idArtist=artist.idArtist and song.idPath=path.idPath and song.iTimesPlayed > 0 order by song.iTimesPlayed limit 20");
				SQLiteResultSet results;
				results = m_db.Execute(strSQL);
				if (results.Rows.Count == 0) return false;
				for (int i = 0; i < results.Rows.Count; ++i)
				{
					AlbumInfo album = new AlbumInfo();
					album.Album = Get(results, 0, "album.strAlbum");
					album.Artist = Get(results, 0, "artist.strArtist");
					albums.Add(album);
				}

				return true;
			}
			catch (SQLiteException ex)
			{
				Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
			}
			return false;
    }

    public void ResetTop100()
    {
			try
			{
        string strSQL = String.Format("update song set iTimesPlayed=0");
        m_db.Execute(strSQL);
      }
      catch (SQLiteException ex)
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

		public bool IncrTop100CounterByFileName(string strFileName1)
		{
			try
			{
				Song song = new Song();
				string strFileName = strFileName1;
				RemoveInvalidChars(ref strFileName);

				string strPath, strFName;
				Split(strFileName, out strPath, out strFName);
        if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
          strPath = strPath.Substring(0, strPath.Length - 1);

				if (null == m_db) return false;
				
				string strSQL;
				ulong dwCRC;
				CRCTool crc = new CRCTool();
				crc.Init(CRCTool.CRCCode.CRC32);
				dwCRC = crc.calc(strFileName1);

				strSQL = String.Format("select * from song,path where song.idPath=path.idPath and dwFileNameCRC='{0}' and strPath='{1}'",
														dwCRC, 
														strPath);
				SQLiteResultSet results;
				results = m_db.Execute(strSQL);
				if (results.Rows.Count == 0) return false;
				int idSong = Int32.Parse(Get(results, 0, "song.idSong"));
				int iTimesPlayed = Int32.Parse(Get(results, 0, "song.iTimesPlayed"));

				strSQL = String.Format("update song set iTimesPlayed={0} where idSong={1}",
															++iTimesPlayed, idSong);
				m_db.Execute(strSQL);
				return true;
			}
			catch (SQLiteException ex)
			{
				Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
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
				strTmp = album.Album; RemoveInvalidChars(ref strTmp); album.Album = strTmp;
				strTmp = album.Genre; RemoveInvalidChars(ref strTmp); album.Genre = strTmp;
				strTmp = album.Artist; RemoveInvalidChars(ref strTmp); album.Artist = strTmp;
				strTmp = album.Tones; RemoveInvalidChars(ref strTmp); album.Tones = strTmp;
				strTmp = album.Styles; RemoveInvalidChars(ref strTmp); album.Styles = strTmp;
				strTmp = album.Review; RemoveInvalidChars(ref strTmp); album.Review = strTmp;
				strTmp = album.Image; RemoveInvalidChars(ref strTmp); album.Image = strTmp;
        strTmp = album.Tracks; RemoveInvalidChars(ref strTmp); album.Tracks = strTmp;
        //strTmp=album.Path  ;RemoveInvalidChars(ref strTmp);album.Path=strTmp;

				if (null == m_db) return - 1;
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
			catch (SQLiteException ex)
			{
				Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
			}

			return - 1;

    }
    public void DeleteAlbumInfo(string strAlbumName1)
    {
      string strAlbum = strAlbumName1;
      RemoveInvalidChars(ref strAlbum);
      string strSQL = String.Format("select * from albuminfo,album where albuminfo.idAlbum=album.idAlbum and album.strAlbum like '{0}'",strAlbum);
      SQLiteResultSet results;
      results = m_db.Execute(strSQL);
      if (results.Rows.Count != 0) 
      {
        int iAlbumId = Int32.Parse(Get(results, 0, "albuminfo.idAlbum"));
        strSQL = String.Format("delete from albuminfo where albuminfo.idAlbum={0}",iAlbumId);
        m_db.Execute(strSQL);
      }
    }

    public bool GetAlbumInfo(string strAlbum1, string strPath1, ref AlbumInfo album)
    {
			try
      {
        if (strPath1 == null) return false;
        if (strPath1.Length == 0) return false;
				string strAlbum = strAlbum1;
				string strPath = strPath1;
				//	musicdatabase always stores directories 
				//	without a slash at the end 
				if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
					strPath = strPath.Substring(0, strPath.Length - 1);
				RemoveInvalidChars(ref strAlbum);
				RemoveInvalidChars(ref strPath);
				string strSQL;
				strSQL = String.Format("select * from albuminfo,album,genre,artist where albuminfo.idAlbum=album.idAlbum and albuminfo.idGenre=genre.idGenre and albuminfo.idArtist=artist.idArtist and album.strAlbum like '{0}'",strAlbum);
				SQLiteResultSet results;
				results = m_db.Execute(strSQL);
				if (results.Rows.Count != 0) 
				{
					album.Rating = Int32.Parse(Get(results, 0, "albuminfo.iRating"));
					album.Year = Int32.Parse(Get(results, 0, "albuminfo.iYear"));
					album.Album = Get(results, 0, "album.strAlbum");
					album.Artist = Get(results, 0, "artist.strArtist");
					album.Genre = Get(results, 0, "genre.strGenre");
					album.Image = Get(results, 0, "albuminfo.strImage");
					album.Review = Get(results, 0, "albuminfo.strReview");
					album.Styles = Get(results, 0, "albuminfo.strStyles");
          album.Tones = Get(results, 0, "albuminfo.strTones");
          album.Tracks = Get(results, 0, "albuminfo.strTracks");
					//album.Path   = Get(results,0,"path.strPath");
					return true;
				}
				return false;
			}
			catch (SQLiteException ex)
			{
				Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
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
        strTmp = artist.Artist; RemoveInvalidChars(ref strTmp); artist.Artist = strTmp;
        strTmp = artist.Born; RemoveInvalidChars(ref strTmp); artist.Born = strTmp;
        strTmp = artist.YearsActive; RemoveInvalidChars(ref strTmp); artist.YearsActive = strTmp;
        strTmp = artist.Genres; RemoveInvalidChars(ref strTmp); artist.Genres = strTmp;
        strTmp = artist.Instruments; RemoveInvalidChars(ref strTmp); artist.Instruments = strTmp;
        strTmp = artist.Tones; RemoveInvalidChars(ref strTmp); artist.Tones = strTmp;
        strTmp = artist.Styles; RemoveInvalidChars(ref strTmp); artist.Styles = strTmp;
        strTmp = artist.AMGBio; RemoveInvalidChars(ref strTmp); artist.AMGBio = strTmp;
        strTmp = artist.Image; RemoveInvalidChars(ref strTmp); artist.Image = strTmp;
        strTmp = artist.Albums; RemoveInvalidChars(ref strTmp); artist.Albums = strTmp;
        strTmp = artist.Compilations; RemoveInvalidChars(ref strTmp); artist.Compilations = strTmp;
        strTmp = artist.Singles; RemoveInvalidChars(ref strTmp); artist.Singles = strTmp;
        strTmp = artist.Misc; RemoveInvalidChars(ref strTmp); artist.Misc = strTmp;

        if (null == m_db) return - 1;
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
      catch (SQLiteException ex)
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

      return - 1;

    }
    public void DeleteArtistInfo(string strArtistName1)
    {
      string strArtist = strArtistName1;
      RemoveInvalidChars(ref strArtist);
      string strSQL = String.Format("select * from artist where artist.strArtist like '{0}'",strArtist);
      SQLiteResultSet results;
      results = m_db.Execute(strSQL);
      if (results.Rows.Count != 0) 
      {
        int iArtistId = Int32.Parse(Get(results, 0, "idArtist"));
        strSQL = String.Format("delete from artistinfo where artistinfo.idArtist={0}",iArtistId);
        m_db.Execute(strSQL);
      }
    }

    public bool GetArtistInfo(string strArtist1, string strPath1, ref ArtistInfo artist)
    {
      try
      {
        if (strPath1 == null) return false;
        if (strPath1.Length == 0) return false;
        string strArtist = strArtist1;
        string strPath = strPath1;
        //	musicdatabase always stores directories 
        //	without a slash at the end 
        if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
          strPath = strPath.Substring(0, strPath.Length - 1);
        RemoveInvalidChars(ref strArtist);
        RemoveInvalidChars(ref strPath);
        string strSQL;
        strSQL = String.Format("select * from artist,artistinfo where artist.idArtist=artistinfo.idArtist and artist.strArtist like '{0}'",strArtist);
        SQLiteResultSet results;
        results = m_db.Execute(strSQL);
        if (results.Rows.Count != 0) 
        {
          artist.Artist = Get(results, 0, "artist.strArtist");
          artist.Born = Get(results, 0, "artistinfo.strBorn");
          artist.YearsActive = Get(results, 0, "artistinfo.strYearsActive");
          artist.Genres = Get(results, 0, "artistinfo.strGenres");
          artist.Styles = Get(results, 0, "artistinfo.strStyles");
          artist.Tones = Get(results, 0, "artistinfo.strTones");
          artist.Instruments = Get(results, 0, "artistinfo.strInstruments");
          artist.Image = Get(results, 0, "artistinfo.strImage");
          artist.AMGBio = Get(results, 0, "artistinfo.strAMGBio");
          artist.Albums = Get(results, 0, "artistinfo.strAlbums");
          artist.Compilations = Get(results, 0, "artistinfo.strCompilations");
          artist.Singles = Get(results, 0, "artistinfo.strSingles");
          artist.Misc = Get(results, 0, "artistinfo.strMisc");
          return true;
        }
        return false;
      }
      catch (SQLiteException ex)
      {
        Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

      return false;
    }

		public bool GetSongsByPath2(string strPath1, ref ArrayList songs)
		{
			try
			{
        songs.Clear();
        if (strPath1 == null) return false;
        if (strPath1.Length == 0) return false;
				string strPath = strPath1;
				//	musicdatabase always stores directories 
				//	without a slash at the end 
				if (strPath[strPath.Length - 1] == '/' || strPath[strPath.Length - 1] == '\\')
					strPath = strPath.Substring(0, strPath.Length - 1);
				RemoveInvalidChars(ref strPath);
				if (null == m_db) return false;
				
				string strSQL;
				strSQL = String.Format("select song.strTitle, song.iYear, song.iDuration, song.iTrack, song.iTimesPlayed, song.strFileName, path.strPath, genre.strGenre, album.strAlbum, artist.strArtist from song,path,album,genre,artist where song.idPath=path.idPath and song.idAlbum=album.idAlbum and song.idGenre=genre.idGenre and song.idArtist=artist.idArtist and path.strPath like '{0}'",strPath);
				SQLiteResultSet results;
				results = m_db.Execute(strSQL);
				if (results.Rows.Count == 0) return false;
				for (int i = 0; i < results.Rows.Count; ++i)
				{
					SongMap songmap = new SongMap();
					Song song = new Song();
					song.Artist = Get(results, i, "artist.strArtist");
					song.Album = Get(results, i, "album.strAlbum");
					song.Genre = Get(results, i, "genre.strGenre");
					song.Track = Int32.Parse(Get(results, i, "song.iTrack"));
					song.Duration = Int32.Parse(Get(results, i, "song.iDuration"));
					song.Year = Int32.Parse(Get(results, i, "song.iYear"));
					song.Title = Get(results, i, "song.strTitle");
					song.TimesPlayed = Int32.Parse(Get(results, i, "song.iTimesPlayed"));
					
					string strFileName = Get(results, i, "path.strPath");
					strFileName += Get(results, i, "song.strFileName");
					song.FileName = strFileName;
					songmap.m_song = song;
					songmap.m_strPath = song.FileName;
					
					songs.Add(songmap);
				}

				return true;
			}
			catch (SQLiteException ex)
			{
				Log.Write("musicdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
			}

			return false;
		}

    
    bool GetSongsByPathes(ArrayList pathes, ref ArrayList songs)
    {
      return false;
    }

    public void CheckVariousArtistsAndCoverArt()
    {
	    if (m_albumCache.Count <= 0)
		    return;

	    foreach (AlbumInfoCache album in m_albumCache)
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

	    m_albumCache.Clear();
    }

    public void BeginTransaction()
    {
      try
      {
        m_db.Execute("begin");
      }
      catch (Exception ex)
      {
        Log.Write("musicdatabase begin transaction failed exception err:{0} ", ex.Message);
      }
    }
    
    public void CommitTransaction()
    {
      
      try
      {
        m_db.Execute("commit");
      }
      catch (Exception ex)
      {
        Log.Write("musicdatabase commit failed exception err:{0} ", ex.Message);
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
        Log.Write("musicdatabase rollback failed exception err:{0} ", ex.Message);
      }
    }
  }
}
