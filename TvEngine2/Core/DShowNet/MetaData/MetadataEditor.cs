#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

// Stephen Toub
// stoub@microsoft.com

using System;
using System.Collections;
using System.Text;

namespace Toub.MediaCenter.Dvrms.Metadata
{
  /// <summary>The type of a metadata attribute value.</summary>
  public enum MetadataItemType
  {
    /// <summary>DWORD</summary>
    Dword = 0,
    /// <summary>String</summary>
    String = 1,
    /// <summary>Binary</summary>
    Binary = 2,
    /// <summary>Boolean</summary>
    Boolean = 3,
    /// <summary>QWORD</summary>
    Qword = 4,
    /// <summary>WORD</summary>
    Word = 5,
    /// <summary>Guid</summary>
    Guid = 6,
  }

  /// <summary>Represents a metadata attribute.</summary>
  public class MetadataItem : ICloneable
  {
    /// <summary>The name of the attribute.</summary>
    private string _name;

    /// <summary>The value of the attribute.</summary>
    private object _value;

    /// <summary>The type of the attribute value.</summary>
    private MetadataItemType _type;

    /// <summary>Initializes the metadata item.</summary>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="value">The value of the attribute.</param>
    /// <param name="type">The type of the attribute value.</param>
    public MetadataItem(string name, object value, MetadataItemType type)
    {
      Name = name;
      Value = value;
      Type = type;
    }

    /// <summary>Gets or sets the name of the attribute.</summary>
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    /// <summary>Gets or sets the value of the attribute.</summary>
    public object Value
    {
      get { return _value; }
      set { _value = value; }
    }

    /// <summary>Gets or sets the type of the attribute value.</summary>
    public MetadataItemType Type
    {
      get { return _type; }
      set { _type = value; }
    }

    /// <summary>Clones the attribute item.</summary>
    /// <returns>A shallow copy of the attribute.</returns>
    public MetadataItem Clone()
    {
      return (MetadataItem) MemberwiseClone();
    }

    /// <summary>Clones the attribute item.</summary>
    /// <returns>A shallow copy of the attribute.</returns>
    object ICloneable.Clone()
    {
      return Clone();
    }
  }

  /// <summary>Metadata editor for ASF files, including WMA, WMV, and DVR-MS files.</summary>
  public abstract class MetadataEditor : IDisposable
  {
    /// <summary>The Title attribute contains the title of the content in the file.</summary>
    public const string Title = "Title";

    /// <summary>The WM/SubTitle attribute contains the subtitle of the content.</summary>
    public const string Subtitle = "WM/SubTitle";

    /// <summary>The Description attribute contains a description of the content of the file.</summary>
    public const string Description = "Description";

    /// <summary>The WM/SubTitleDescription attribute contains a description of the content of the file.</summary>
    public const string SubtitleDescription = "WM/SubTitleDescription";

    /// <summary>The WM/MediaCredits attribute contains a list of those involved in the production of the content of the file.</summary>
    public const string Credits = "WM/MediaCredits";

    /// <summary>The Author attribute contains the name of a media artist or actor associated with the content.</summary>
    public const string Author = "Author";

    /// <summary>The WM/AlbumArtist attribute contains the name of the primary artist for the album.</summary>
    public const string AlbumArtist = "WM/AlbumArtist";

    /// <summary>The WM/AlbumTitle attribute contains the title of the album on which the content was originally released.</summary>
    public const string AlbumTitle = "WM/AlbumTitle";

    /// <summary>The WM/MediaStationName attribute contains the title of the station that aired the content was originally released.</summary>
    public const string StationName = "WM/MediaStationName";

    /// <summary>The WM/Composer attribute contains the name of the music composer.</summary>
    public const string Composer = "WM/Composer";

    /// <summary>The WM/ParentalRating attribute contains the parental rating of the content.</summary>
    public const string ParentalRating = "WM/ParentalRating";

    /// <summary>The WM/ParentalRating attribute contains the reason for the parental rating of the content.</summary>
    public const string ParentalRatingReason = "WM/ParentalRatingReason";

    /// <summary>The WM/MediaOriginalBroadcastDateTime attribute contains the original broadcast date and time of the content.</summary>
    public const string MediaOriginalBroadcastDateTime = "WM/MediaOriginalBroadcastDateTime";

    /// <summary>The WM/Mood attribute contains a category name for the mood of the content.</summary>
    public const string Mood = "WM/Mood";

    /// <summary>The WM/Genre attribute contains the genre of the content.</summary>
    public const string Genre = "WM/Genre";

    /// <summary>The WM/Language attribute contains the language of the stream.</summary>
    public const string Language = "WM/Language";

    /// <summary>The WM/Lyrics attribute contains the lyrics as a simple string.</summary>
    public const string Lyrics = "WM/Lyrics";

    /// <summary>The WM/Lyrics_Synchronised attribute contains lyrics synchronized to times in the file.</summary>
    public const string SynchronizedLyrics = "WM/Lyrics_Synchronised";

    /// <summary>The Duration attribute contains the length of the file in hundreds of nanoseconds.</summary>
    public const string Duration = "Duration";

    /// <summary>The WM/ContentGroupDescription attribute contains a content group description.</summary>
    public const string ContentGroupDescription = "WM/ContentGroupDescription";

    /// <summary>The WM/PartOfSet attribute contains the set grouping for this content.</summary>
    public const string PartOfSet = "WM/PartOfSet";

    /// <summary>Initialize the editor.</summary>
    protected MetadataEditor()
    {
    }

    /// <summary>Releases all of the resources for the editor.</summary>
    void IDisposable.Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>Releases all of the resources for the editor.</summary>
    /// <param name="disposing">Whether the object is currently being disposed (rather than finalized).</param>
    protected virtual void Dispose(bool disposing)
    {
    }

    /// <summary>Releases all of the resources for the editor.</summary>
    ~MetadataEditor()
    {
      Dispose(false);
    }

    /// <summary>Retrieves the string value of a metadata item.</summary>
    /// <param name="items">The collection of metadata items containing the item to be retrieved.</param>
    /// <param name="name">The name of the attribute value to be retrieved.</param>
    /// <returns>The attribute value as a string.</returns>
    public static string GetMetadataItemAsString(IDictionary items, string name)
    {
      MetadataItem item = (MetadataItem) items[name];
      if (item == null || item.Value == null)
      {
        return string.Empty;
      }
      return item.Value.ToString().Trim();
    }

    /// <summary>Sets the value of a string attribute.</summary>
    /// <param name="items">The metadata items collection.</param>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="value">The new value of the attribute.</param>
    public static void SetMetadataItemAsString(IDictionary items, string name, string value)
    {
      items[name] = new MetadataItem(name, value, MetadataItemType.String);
    }

    /// <summary>Copies a metadata item from one collection under one name to another collection under another name.</summary>
    /// <param name="source">The source collection.</param>
    /// <param name="sourceName">The source name.</param>
    /// <param name="destination">The destination collection.</param>
    /// <param name="destinationName">The destination name.</param>
    private static void CopyMetadataItem(IDictionary source, string sourceName, IDictionary destination,
                                         string destinationName)
    {
      // Gets the source item
      MetadataItem item = (MetadataItem) source[sourceName];

      // Clone the item and copy it to the destination
      if (item != null)
      {
        item = item.Clone();
        item.Name = destinationName;
        destination[destinationName] = item;
      }
    }

    /// <summary>Migrate the metadata from one file to another.</summary>
    /// <param name="source">The source editor.></param>
    /// <param name="destination">The destination editor.</param>
    /// <returns>The migrated collection.</returns>
    public static IDictionary MigrateMetadata(MetadataEditor source, MetadataEditor destination)
    {
      return MigrateMetadata(source, destination, false);
    }

    /// <summary>Migrate the metadata from one file to another.</summary>
    /// <param name="source">The source editor.></param>
    /// <param name="destination">The destination editor.</param>
    /// <param name="augmentMetadata">Whether to augment the metadata for WMP and MCE.</param>
    /// <returns>The migrated collection.</returns>
    public static IDictionary MigrateMetadata(MetadataEditor source, MetadataEditor destination, bool augmentMetadata)
    {
      IDictionary metadata = source.GetAttributes();

      // Augment the metadata to provide a better experience in both WMP and MCE
      if (augmentMetadata)
      {
        string title = GetMetadataItemAsString(metadata, Title);
        string subTitle = GetMetadataItemAsString(metadata, Subtitle);
        if (!title.EndsWith(subTitle))
        {
          title += (title.Length > 0 && subTitle.Length > 0 ? " - " : string.Empty) + subTitle;
        }
        SetMetadataItemAsString(metadata, Title, title);
        CopyMetadataItem(metadata, SubtitleDescription, metadata, Description);
        CopyMetadataItem(metadata, Credits, metadata, Author);
        CopyMetadataItem(metadata, Title, metadata, AlbumTitle);
        CopyMetadataItem(metadata, StationName, metadata, Composer);
        CopyMetadataItem(metadata, ParentalRating, metadata, ContentGroupDescription);
        CopyMetadataItem(metadata, MediaOriginalBroadcastDateTime, metadata, PartOfSet);
        CopyMetadataItem(metadata, ParentalRating, metadata, Mood);
      }

      // Set the metadata onto the destination file
      destination.SetAttributes(metadata);

      return metadata;
    }

    /// <summary>Converts a value to the target type and gets its byte representation.</summary>
    /// <param name="item">The item whose value is to be translated.</param>
    /// <param name="valueData">The output byte array.</param>
    protected static bool TranslateAttributeToByteArray(MetadataItem item, out byte[] valueData)
    {
      int valueLength;
      switch (item.Type)
      {
        case MetadataItemType.Dword:
          valueData = BitConverter.GetBytes((int) item.Value);
          return true;

        case MetadataItemType.Word:
          valueData = BitConverter.GetBytes((short) item.Value);
          return true;

        case MetadataItemType.Qword:
          valueData = BitConverter.GetBytes((long) item.Value);
          return true;

        case MetadataItemType.Boolean:
          valueData = BitConverter.GetBytes(((bool) item.Value) ? 1 : 0);
          return true;

        case MetadataItemType.String:
          string strValue = item.Value.ToString();
          valueLength = (strValue.Length + 1)*2; // plus 1 for null-term, times 2 for unicode
          valueData = new byte[valueLength];
          Buffer.BlockCopy(strValue.ToCharArray(), 0, valueData, 0, strValue.Length*2);
          valueData[valueLength - 2] = 0;
          valueData[valueLength - 1] = 0;
          return true;

        default:
          valueData = null;
          return false;
      }
    }

    /// <summary>Sets the collection of string attributes onto the specified file and stream.</summary>
    /// <param name="propsToSet">The properties to set on the file.</param>
    public abstract void SetAttributes(IDictionary propsToSet);

    /// <summary>Gets the value of the specified attribute.</summary>
    /// <param name="itemType">The type of the attribute.</param>
    /// <param name="valueData">The byte array to be parsed.</param>
    protected static object ParseAttributeValue(MetadataItemType itemType, byte[] valueData)
    {
      if (!Enum.IsDefined(typeof (MetadataItemType), itemType))
      {
        throw new ArgumentOutOfRangeException("itemType");
      }
      if (valueData == null)
      {
        throw new ArgumentNullException("valueData");
      }

      // Convert the attribute value to a byte array based on the item type.
      switch (itemType)
      {
        case MetadataItemType.String:
          StringBuilder sb = new StringBuilder(valueData.Length);
          for (int i = 0; i < valueData.Length - 2; i += 2)
          {
            sb.Append(Convert.ToString(BitConverter.ToChar(valueData, i)));
          }
          string result = sb.ToString();
          if (result.EndsWith("\\0"))
          {
            result = result.Substring(0, result.Length - 2);
          }
          return result;
        case MetadataItemType.Boolean:
          return BitConverter.ToBoolean(valueData, 0);
        case MetadataItemType.Dword:
          return BitConverter.ToInt32(valueData, 0);
        case MetadataItemType.Qword:
          return BitConverter.ToInt64(valueData, 0);
        case MetadataItemType.Word:
          return BitConverter.ToInt16(valueData, 0);
        case MetadataItemType.Guid:
          return new Guid(valueData);
        case MetadataItemType.Binary:
          return valueData;
        default:
          throw new ArgumentOutOfRangeException("itemType");
      }
    }

    /// <summary>Gets all of the attributes on a file.</summary>
    /// <returns>A collection of the attributes from the file.</returns>
    public abstract IDictionary GetAttributes();
  }
}