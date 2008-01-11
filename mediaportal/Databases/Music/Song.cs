#region Copyright (C) 2005-2008 Team MediaPortal

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
  {
    Init,
    Loaded,
    Cached,
    Queued,
    Submitted,
    Short
  }

  [Serializable()]
  public class Song
  {
    int _iTrackId = -1;
    string _strFileName = "";
    string _strTitle = "";
    string _strArtist = "";
    string _strAlbum = "";
    string _strAlbumArtist = "";
    string _strGenre = "";
    int _iTrack = 0;
    int _iNumTracks = 0;
    int _iDuration = 0;
    int _iYear = 0;
    int _iTimedPlayed = 0;
    int _iRating = 0;
    int _iResumeAt = 0;
    bool _favorite = false;
    DateTime _dateTimeModified = DateTime.MinValue;
    DateTime _dateTimePlayed = DateTime.MinValue;
    SongStatus _audioScrobblerStatus;
    string _musicBrainzID;
    string _strURL = "";
    string _webImage = "";
    string _lastFMMatch = "";
    int _iDisc = 0;
    int _iNumDisc = 0;
    string _strLyrics = "";
    

    public Song()
    {
    }

    public Song Clone()
    {
      Song newsong = new Song();
      newsong.Id = Id;
      newsong.Album = Album;
      newsong.Artist = Artist;
      newsong.AlbumArtist = AlbumArtist;
      newsong.Duration = Duration;
      newsong.FileName = FileName;
      newsong.Genre = Genre;
      newsong.TimesPlayed = TimesPlayed;
      newsong.Title = Title;
      newsong.Track = Track;
      newsong.TrackTotal = TrackTotal;
      newsong.Year = Year;
      newsong.Rating = Rating;
      newsong.Favorite = Favorite;
      newsong.DateTimeModified = DateTimeModified;
      newsong.DateTimePlayed = DateTimePlayed;
      newsong.AudioScrobblerStatus = AudioScrobblerStatus;
      newsong.MusicBrainzID = MusicBrainzID;
      newsong.URL = URL;
      newsong.WebImage = WebImage;
      newsong.LastFMMatch = LastFMMatch;
      newsong.ResumeAt = ResumeAt;
      newsong.DiscId = DiscId;
      newsong.DiscTotal = DiscTotal;
      newsong.Lyrics = Lyrics;

      return newsong;
    }

    public void Clear()
    {
      _iTrackId = -1;
      _favorite = false;
      _strFileName = "";
      _strTitle = "";
      _strArtist = "";
      _strAlbum = "";
      _strAlbumArtist = "";
      _strGenre = "";
      _iTrack = 0;
      _iNumTracks = 0;
      _iDuration = 0;
      _iYear = 0;
      _iTimedPlayed = 0;
      _iRating = 0;
      _dateTimeModified = DateTime.MinValue;
      _dateTimePlayed = DateTime.MinValue;
      _audioScrobblerStatus = SongStatus.Init;
      _musicBrainzID = "";
      _strURL = "";
      _webImage = "";
      _lastFMMatch = "";
      _iResumeAt = 0;
      _iDisc = 0;
      _iNumDisc = 0;
      _strLyrics = "";
    }

    public int Id
    {
      get { return _iTrackId; }
      set { _iTrackId = value; }
    }

    public string FileName
    {
      get { return _strFileName; }
      set { _strFileName = value; }
    }

    public string Artist
    {
      get { return _strArtist; }
      set
      {
        _strArtist = value;
        //remove 01. artist name
        if (_strArtist.Length > 4)
        {
          if (Char.IsDigit(_strArtist[0]) &&
              Char.IsDigit(_strArtist[1]) &&
              _strArtist[2] == '.' &&
              _strArtist[3] == ' ')
          {
            _strArtist = _strArtist.Substring(4);
          }
        }
        //remove artist name [dddd]
        int pos = _strArtist.IndexOf("[");
        if (pos > 0)
        {
          _strArtist = _strArtist.Substring(pos);
        }
        _strArtist = _strArtist.Trim();
      }
    }

    public string AlbumArtist
    {
      get { return _strAlbumArtist; }
      set { _strAlbumArtist = value; }
    }

    public string Album
    {
      get { return _strAlbum; }
      set { _strAlbum = value; }
    }

    public string Genre
    {
      get { return _strGenre; }
      set { _strGenre = value; }
    }

    public string Title
    {
      get { return _strTitle; }
      set { _strTitle = value; }
    }

    public int Track
    {
      get { return _iTrack; }
      set
      {
        _iTrack = value;
        if (_iTrack < 0)
          _iTrack = 0;
      }
    }

    public int TrackTotal
    {
      get { return _iNumTracks; }
      set
      {
        _iNumTracks = value;
        if (_iNumTracks < 0)
          _iNumTracks = 0;
      }
    }

    /// <summary>
    /// Length of song in total seconds
    /// </summary>
    public int Duration
    {
      get { return _iDuration; }
      set
      {
        _iDuration = value;
        if (_iDuration < 0)
          _iDuration = 0;
      }
    }

    public int Year
    {
      get { return _iYear; }
      set
      {
        _iYear = value;
        if (_iYear < 0)
          _iYear = 0;
        else
        {
          if (_iYear > 0 && _iYear < 100)
            _iYear += 1900;
        }
      }
    }

    public int TimesPlayed
    {
      get { return _iTimedPlayed; }
      set { _iTimedPlayed = value; }
    }

    public int Rating
    {
      get { return _iRating; }
      set { _iRating = value; }
    }

    public bool Favorite
    {
      get { return _favorite; }
      set { _favorite = value; }
    }

    public DateTime DateTimeModified
    {
      get { return _dateTimeModified; }
      set { _dateTimeModified = value; }
    }

    /// <summary>
    /// Last UTC time the song was played
    /// </summary>
    public DateTime DateTimePlayed
    {
      get { return _dateTimePlayed; }
      set { _dateTimePlayed = value; }
    }

    public SongStatus AudioScrobblerStatus
    {
      get { return _audioScrobblerStatus; }
      set { _audioScrobblerStatus = value; }
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

    public int ResumeAt
    {
      get { return _iResumeAt; }
      set
      {
        _iResumeAt = value;
        if (_iResumeAt < 0)
          _iResumeAt = 0;
      }
    }

    public int DiscId
    {
      get { return _iDisc; }
      set
      {
        _iDisc = value;
        if (_iDisc < 0)
          _iDisc = 0;
      }
    }

    public int DiscTotal
    {
      get { return _iNumDisc; }
      set
      {
        _iNumDisc = value;
        if (_iNumDisc < 0)
          _iNumDisc = 0;
      }
    }

    public string Lyrics
    {
      get { return _strLyrics; }
      set { _strLyrics = value; }
    }

    public MusicTag ToMusicTag()
    {
      MusicTag tmpTag = new MusicTag();

      tmpTag.Title = this.Title;
      tmpTag.Album = this.Album;
      tmpTag.AlbumArtist = this.AlbumArtist;
      tmpTag.Artist = this.Artist;
      tmpTag.Duration = this.Duration;
      tmpTag.Genre = this.Genre;
      tmpTag.Track = this.Track;
      tmpTag.Year = this.Year;
      tmpTag.Rating = this.Rating;
      tmpTag.TimesPlayed = this.TimesPlayed;
      tmpTag.Lyrics = this.Lyrics;

      return tmpTag;
    }

    public string ToShortString()
    {
      StringBuilder s = new StringBuilder();

      if (_strTitle != "")
        s.Append(_strTitle);
      else
        s.Append("(Untitled)");
      if (_strArtist != "")
        s.Append(" - " + _strArtist);
      if (_strAlbum != "")
        s.Append(" (" + _strAlbum + ")");

      return s.ToString();
    }

    public string ToLastFMString()
    {
      StringBuilder s = new StringBuilder();

      if (_strTitle != "")
      {
        s.Append(_strTitle);
        s.Append(" - ");
      }
      if (_strArtist != "")
        s.Append(_strArtist);
      if (_iDuration > 0)
      {
        s.Append(" [");
        s.Append(Util.Utils.SecondsToHMSString(_iDuration));
        s.Append("]");
      }
      if (_iTimedPlayed > 0)
        s.Append(" (played: " + Convert.ToString(_iTimedPlayed) + " times)");

      return s.ToString();
    }

    public string ToLastFMMatchString(bool showURL_)
    {
      StringBuilder s = new StringBuilder();
      if (_strArtist != "")
      {
        s.Append(_strArtist);
        if (_strAlbum != "")
          s.Append(" - " + _strAlbum);
        else
        {
          if (_strTitle != "")
            s.Append(" - " + _strTitle);
          if (_strGenre != "")
            s.Append(" (tagged: " + _strGenre + ")");
        }
      }
      else
        if (_strAlbum != "")
          s.Append(_strAlbum);
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
      return System.Web.HttpUtility.UrlEncode(_strArtist);
    }

    public override string ToString()
    {
      return _strArtist + "\t" +
        _strTitle + "\t" +
        _strAlbum + "\t" +
        _musicBrainzID + "\t" +
        _iDuration + "\t" +
        _dateTimePlayed.ToString("s");
    }

    public string getQueueTime(bool asUnixTime)
    {
      string queueTime = string.Empty;

      if (asUnixTime)
        queueTime = Convert.ToString(Util.Utils.GetUnixTime(DateTimePlayed.ToUniversalTime()));
      else
      {
        queueTime = String.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}",
                                  _dateTimePlayed.Year,
                                  _dateTimePlayed.Month,
                                  _dateTimePlayed.Day,
                                  _dateTimePlayed.Hour,
                                  _dateTimePlayed.Minute,
                                  _dateTimePlayed.Second);
      }
      return queueTime;
    }
  }


  public class SongMap
  {
    public string m_strPath;
    public Song m_song;
  }
}