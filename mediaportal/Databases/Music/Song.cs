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
using System.Text;
using MediaPortal.TagReader;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// 
  /// </summary>
  public enum SongStatus 
  { Init,
    Loaded,
    Cached,
    Queued,
    Submitted,
    Short
  }

  [Serializable()]
  public class Song
  {
    string m_strFileName = "";
    string m_strTitle = "";
    string m_strArtist = "";
    string m_strAlbum = "";
    string m_strAlbumArtist = "";
    string m_strGenre = "";
    int m_iTrack = 0;
    int m_iDuration = 0;
    int m_iYear = 0;
    int m_iTimedPlayed = 0;
    int m_irating = 0;
    int idGenre = -1;
    int idAlbum = -1;
    int idArtist = -1;
    int idAlbumArtist = -1;
    int Id = -1;
    bool favorite = false;
    string m_strDateModified = "";
    DateTime _DateTimePlayed;
    SongStatus _audioScrobblerStatus;
    bool _audioScrobblerProcessed;
    string _musicBrainzID;
    string _strURL = "";
    string _webImage = "";
    string _lastFMMatch = "";

    public Song()
    {
    }

    public Song Clone()
    {
      Song newsong = new Song();
      newsong.Album = Album;
      newsong.Artist = Artist;
      newsong.AlbumArtist = AlbumArtist;
      newsong.Duration = Duration;
      newsong.FileName = FileName;
      newsong.Genre = Genre;
      newsong.TimesPlayed = TimesPlayed;
      newsong.Title = Title;
      newsong.Track = Track;
      newsong.Year = Year;
      newsong.Rating = Rating;
      newsong.idGenre = idGenre;
      newsong.idAlbum = idAlbum;
      newsong.idArtist = idArtist;
      newsong.idAlbumArtist = idAlbumArtist;
      newsong.Id = Id;
      newsong.favorite = Favorite;
      newsong.DateModified = DateModified;
      newsong.DateTimePlayed = DateTimePlayed;
      newsong.AudioScrobblerStatus = AudioScrobblerStatus;
      newsong.AudioScrobblerProcessed = AudioScrobblerProcessed;
      newsong.MusicBrainzID = MusicBrainzID;
      newsong.URL = URL;
      newsong.WebImage = WebImage;
      newsong.LastFMMatch = LastFMMatch;

      return newsong;
    }

    public void Clear()
    {
      favorite = false;
      idGenre = -1;
      idAlbum = -1;
      idArtist = -1;
      idAlbumArtist = -1;
      Id = -1;
      m_strFileName = "";
      m_strTitle = "";
      m_strArtist = "";
      m_strAlbum = "";
      m_strAlbumArtist = "";
      m_strGenre = "";
      m_iTrack = 0;
      m_iDuration = 0;
      m_iYear = 0;
      m_iTimedPlayed = 0;
      m_irating = 0;
      m_strDateModified = "";
      _DateTimePlayed = DateTime.MinValue;
      _audioScrobblerStatus = SongStatus.Init;
      _audioScrobblerProcessed = false;
      _musicBrainzID = "";
      _strURL = "";
      _webImage = "";
      _lastFMMatch = "";
    }

    public string FileName
    {
      get { return m_strFileName; }
      set { m_strFileName = value; }
    }

    public string Title
    {
      get { return m_strTitle; }
      set { m_strTitle = value; }
    }

    public string Artist
    {
      get { return m_strArtist; }
      set
      {
        m_strArtist = value;
        //remove 01. artist name
        if (m_strArtist.Length > 4)
        {
          if (Char.IsDigit(m_strArtist[0]) &&
              Char.IsDigit(m_strArtist[1]) &&
              m_strArtist[2] == '.' &&
              m_strArtist[3] == ' ')
          {
            m_strArtist = m_strArtist.Substring(4);
          }
        }
        //remove artist name [dddd]
        int pos = m_strArtist.IndexOf("[");
        if (pos > 0)
        {
          m_strArtist = m_strArtist.Substring(pos);
        }
        m_strArtist = m_strArtist.Trim();
      }
    }

    public string Album
    {
      get { return m_strAlbum; }
      set { m_strAlbum = value; }
    }

    public string Genre
    {
      get { return m_strGenre; }
      set { m_strGenre = value; }
    }

    public string AlbumArtist
    {
      get { return m_strAlbumArtist; }
      set { m_strAlbumArtist = value; }
    }

    public int Track
    {
      get { return m_iTrack; }
      set
      {
        m_iTrack = value;
        if (m_iTrack < 0)
          m_iTrack = 0;
      }
    }

    /// <summary>
    /// Length of song in total seconds
    /// </summary>
    public int Duration
    {
      get { return m_iDuration; }
      set
      {
        m_iDuration = value;
        if (m_iDuration < 0)
          m_iDuration = 0;
      }
    }

    public int Year
    {
      get { return m_iYear; }
      set
      {
        m_iYear = value;
        if (m_iYear < 0)
          m_iYear = 0;
        else
        {
          if (m_iYear > 0 && m_iYear < 100)
            m_iYear += 1900;
        }
      }
    }

    public int TimesPlayed
    {
      get { return m_iTimedPlayed; }
      set { m_iTimedPlayed = value; }
    }

    public int Rating
    {
      get { return m_irating; }
      set { m_irating = value; }
    }

    public bool Favorite
    {
      get { return favorite; }
      set { favorite = value; }
    }

    public int albumId
    {
      get { return idAlbum; }
      set { idAlbum = value; }
    }

    public int genreId
    {
      get { return idGenre; }
      set { idGenre = value; }
    }

    public int artistId
    {
      get { return idArtist; }
      set { idArtist = value; }
    }

    public int albumartistId
    {
      get { return idAlbumArtist; }
      set { idAlbumArtist = value; }
    }

    public int songId
    {
      get { return Id; }
      set { Id = value; }
    }

    public string DateModified
    {
      get { return m_strDateModified; }
      set { m_strDateModified = value; }
    }

    /// <summary>
    /// Last UTC time the song was played
    /// </summary>
    public DateTime DateTimePlayed
    {
      get { return _DateTimePlayed; }
      set { _DateTimePlayed = value; }
    }

    public SongStatus AudioScrobblerStatus
    {
      get { return _audioScrobblerStatus; }
      set { _audioScrobblerStatus = value; }
    }

    public bool AudioScrobblerProcessed
    {
      get { return _audioScrobblerProcessed; }
      set { _audioScrobblerProcessed = value; }
    }

    public string MusicBrainzID
    {
      get { return _musicBrainzID; }
      set { _musicBrainzID = value; }
    }

    public string URL
    {
      get { return _strURL; }
      set { _strURL = value; }
    }

    public string WebImage
    {
      get { return _webImage; }
      set { _webImage = value; }
    }

    public string LastFMMatch
    {
      get { return _lastFMMatch; }
      set { _lastFMMatch = value; }
    }

    public MusicTag ToMusicTag()
    {
      MusicTag tmpTag = new MusicTag();

      tmpTag.Title = this.Title;
      tmpTag.Album = this.Album;
      tmpTag.Artist = this.Artist;
      tmpTag.Duration = this.Duration;
      tmpTag.Genre = this.Genre;
      tmpTag.Track = this.Track;
      tmpTag.Year = this.Year;
      tmpTag.Rating = this.Rating;

      return tmpTag;
    }

    public string ToShortString()
    {
      StringBuilder s = new StringBuilder();

      if (m_strTitle != "")
        s.Append(m_strTitle);
      else
        s.Append("(Untitled)");
      if (m_strArtist != "")
        s.Append(" - " + m_strArtist);
      if (m_strAlbum != "")
        s.Append(" (" + m_strAlbum + ")");

      return s.ToString();
    }

    public string ToLastFMString()
    {
      StringBuilder s = new StringBuilder();

      if (m_strTitle != "")
      {
        s.Append(m_strTitle);
        s.Append(" - ");
      }
      if (m_strArtist != "")
        s.Append(m_strArtist);
      if (m_iDuration > 0)
      {
        s.Append(" [");
        s.Append(Util.Utils.SecondsToHMSString(m_iDuration));
        s.Append("]");
      }
      if (m_iTimedPlayed > 0)
        s.Append(" (played: " + Convert.ToString(m_iTimedPlayed) + " times)");

      return s.ToString();
    }

    public string ToLastFMMatchString(bool showURL_)
    {
      StringBuilder s = new StringBuilder();
      if (m_strArtist != "")
      {
        s.Append(m_strArtist);
        if (m_strAlbum != "")
          s.Append(" - " + m_strAlbum);
        else
        {
          if (m_strTitle != "")
            s.Append(" - " + m_strTitle);
          if (m_strGenre != "")
            s.Append(" (tagged: " + m_strGenre + ")");
        }
      }
      else
        if (m_strAlbum != "")
          s.Append(m_strAlbum);
      if (_lastFMMatch != "")
        if (_lastFMMatch.IndexOf(".") == -1)
          s.Append(" (match: " + _lastFMMatch + "%)");
        else
          s.Append(" (match: " + _lastFMMatch.Remove(_lastFMMatch.IndexOf(".") + 2) + "%)");
      if (showURL_)
        if (_strURL != "")
          s.Append(" (link: " + _strURL + ")");

      return s.ToString();
    }

    public string ToURLArtistString()
    {
      return System.Web.HttpUtility.UrlEncode(m_strArtist);
    }

    public override string ToString()
    {
      return m_strArtist + "\t" +
        m_strTitle + "\t" +
        m_strAlbum + "\t" +
        _musicBrainzID + "\t" +
        m_iDuration + "\t" +
        _DateTimePlayed.ToString("s");
    }

    public string getQueueTime(bool asUnixTime)
    {
      string queueTime = string.Empty;

      if (asUnixTime)
        queueTime = Convert.ToString(Util.Utils.GetUnixTime(DateTimePlayed.ToUniversalTime()));
      else
      {
        queueTime = String.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}",
                                  _DateTimePlayed.Year,
                                  _DateTimePlayed.Month,
                                  _DateTimePlayed.Day,
                                  _DateTimePlayed.Hour,
                                  _DateTimePlayed.Minute,
                                  _DateTimePlayed.Second);
      }
      return queueTime;
    }

    //public string GetPostData(int index)
    //{
    //  // Generate POST data for updates:
    //  //	u - username
    //  //	s - md5 response
    //  //	a - artist
    //  //	t - title
    //  //	b - album
    //  //	m - musicbrainz id
    //  //	l - length (secs)
    //  //	i - time (UTC)

    //  return String.Format("a[{0}]={1}&t[{0}]={2}&b[{0}]={3}&m[{0}]={4}&l[{0}]={5}&i[{0}]={6}",
    //                       index,
    //                       System.Web.HttpUtility.UrlEncode(m_strArtist),
    //                       System.Web.HttpUtility.UrlEncode(m_strTitle),
    //                       System.Web.HttpUtility.UrlEncode(m_strAlbum),
    //                       System.Web.HttpUtility.UrlEncode(_musicBrainzID),
    //                       m_iDuration,
    //                       System.Web.HttpUtility.UrlEncode(getQueueTime()));
    //}
  }


  public class SongMap
  {
    public string m_strPath;
    public Song m_song;
  }
}