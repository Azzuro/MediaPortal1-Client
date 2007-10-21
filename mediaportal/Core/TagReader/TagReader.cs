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
using System.Reflection;
using System.Drawing;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;
using TagLib;

namespace MediaPortal.TagReader
{
  /// <summary>
  /// This class will manage all tagreader plugins
  /// See the ITag.cs for more information about tagreader plugins
  /// It will load all tagreader plugins and when Mediaportal wants information for a given music file
  /// it will check which tagreader plugin supports it and ask it to read the information
  /// which is then returned to mediaportal
  /// </summary>
  public class TagReader
  {
    /// <summary>
    /// Constructor
    /// This will load all tagreader plugins from plugins/tagreaders
    /// </summary>
    static TagReader()
    {
    }

    /// <summary>
    /// This method is called by mediaportal when it wants information for a music file
    /// The method will check which tagreader supports the file and ask it to extract the information from it
    /// </summary>
    /// <param name="strFile">filename of the music file</param>
    /// <returns>
    /// MusicTag instance when file has been read
    /// null when file type is not supported or if the file does not contain any information
    /// </returns>
    public static MusicTag ReadTag(string strFile)
    {
      if (!IsAudio(strFile))
        return null;

      char[] trimChars = { ' ', '\x00' };

      try
      {
        // Set the flag to use the standard System Encoding set by the user
        // Otherwise Latin1 is used as default, which causes characters in various languages being displayed wrong
        TagLib.ByteVector.UseBrokenLatin1Behavior = true;
        TagLib.File tag = TagLib.File.Create(strFile);
        MusicTag musictag = new MusicTag();
        string[] artists = tag.Tag.Performers;
        if (artists.Length > 0)
          musictag.Artist = String.Join(";", artists).Trim(trimChars);
        musictag.Album = tag.Tag.Album;
        musictag.HasAlbumArtist = false;
        string[] albumartists = tag.Tag.AlbumArtists;
        if (albumartists.Length > 0)
        {
          musictag.AlbumArtist = String.Join(";", albumartists).Trim(trimChars);
          musictag.HasAlbumArtist = true;
        }
        musictag.BitRate = tag.Properties.AudioBitrate;
        musictag.Comment = tag.Tag.Comment;
        string[] composer = tag.Tag.Composers;
        if (composer.Length > 0)
          musictag.Composer = composer[0].Trim(trimChars);
        IPicture[] pics = new IPicture[] { };
        pics = tag.Tag.Pictures;
        if (pics.Length > 0)
          musictag.CoverArtImageBytes = pics[0].Data.Data;
        musictag.Duration = (int)tag.Properties.Duration.TotalSeconds;
        musictag.FileName = strFile;
        musictag.FileType = tag.MimeType;
        string[] genre = tag.Tag.Genres;
        if (genre.Length > 0)
          musictag.Genre = String.Join(";", genre).Trim(trimChars);
        string lyrics = tag.Tag.Lyrics;
        if (lyrics == null)
          musictag.Lyrics = "";
        else
          musictag.Lyrics = lyrics.Trim(trimChars);
        musictag.Title = tag.Tag.Title.Trim(trimChars);
        musictag.Track = (int)tag.Tag.Track;
        musictag.TrackTotal = (int)tag.Tag.TrackCount;
        musictag.DiscID = (int)tag.Tag.Disc;
        musictag.DiscTotal = (int)tag.Tag.DiscCount;

        // To display Multiple Disc Albums correctly, we add the discid mupltplied by 100
        if (musictag.DiscID > 0)
          musictag.Track += (musictag.DiscID * 100);

        musictag.Year = (int)tag.Tag.Year;

        if (tag.MimeType == "taglib/mp3")
        {
          // Handle the Rating, which comes from the POPM frame
          TagLib.Id3v2.Tag id32_tag = tag.GetTag(TagLib.TagTypes.Id3v2) as TagLib.Id3v2.Tag;
          if (id32_tag != null)
          {
            TagLib.Id3v2.PopularimeterFrame popm;
            foreach (TagLib.Id3v2.Frame frame in id32_tag)
            {
              popm = frame as TagLib.Id3v2.PopularimeterFrame;
              if (popm != null)
              {
                int rating = popm.Rating;
                int i = 0;
                if (rating > 205)
                  i = 5;
                else if (rating > 154)
                  i = 4;
                else if (rating > 104)
                  i = 3;
                else if (rating > 53)
                  i = 2;
                else if (rating > 0)
                  i = 1;
                musictag.Rating = i;
              }
            }
          }
        }

        // if we didn't get a title, use the Filename without extension to prevent the file to appear as "unknown"
        if (musictag.Title == "")
          musictag.Title = System.IO.Path.GetFileNameWithoutExtension(strFile);

        return musictag;
      }
      catch (UnsupportedFormatException)
      {
        Log.Warn("Tagreader: Unsupported File Format {0}", strFile);
      }
      catch (Exception ex)
      {
        Log.Error("TagReader: Exception reading file {0}. {1}", strFile, ex.Message);
      }
      return null;
    }

    private static bool IsAudio(string fileName)
    {
      string ext = System.IO.Path.GetExtension(fileName).ToLower();

      switch (ext)
      {
        case ".ape":
        case ".flac":
        case ".mp3":
        case ".ogg":
        case ".wv":
        case ".wav":
        case ".wma":
        case ".mp4":
        case ".m4a":
        case ".m4p":
        case ".mpc":
        case ".mp+":
        case ".mpp":
          return true;
      }

      return false;
    }
  }
}
