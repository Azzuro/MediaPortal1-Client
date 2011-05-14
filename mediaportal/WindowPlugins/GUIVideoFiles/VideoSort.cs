#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Video.Database;
using System.Collections;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// Summary description for VideoSort.
  /// </summary>
  public class VideoSort : IComparer<GUIListItem>
  {
    public enum SortMethod
    {
      Name = 0,
      Modified = 1,
      Created = 2,
      Size = 3,
      Year = 4,
      Rating = 5,
      Label = 6,
      Unwatched = 7
    }

    protected SortMethod currentSortMethod;
    protected bool sortAscending;

    public VideoSort(SortMethod sortMethod, bool ascending)
    {
      currentSortMethod = sortMethod;
      sortAscending = ascending;
    }

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      if (item1 == item2)
      {
        return 0;
      }
      if (item1 == null)
      {
        return -1;
      }
      if (item2 == null)
      {
        return -1;
      }
      if (item1.IsFolder && item1.Label == "..")
      {
        return -1;
      }
      if (item2.IsFolder && item2.Label == "..")
      {
        return -1;
      }
      if (item1.IsFolder && !item2.IsFolder)
      {
        return -1;
      }
      else if (!item1.IsFolder && item2.IsFolder)
      {
        return 1;
      }


      switch (currentSortMethod)
      {
        case SortMethod.Year:
          {
            if (sortAscending)
            {
              if (item1.Year > item2.Year)
              {
                return 1;
              }
              if (item1.Year < item2.Year)
              {
                return -1;
              }
            }
            else
            {
              if (item1.Year > item2.Year)
              {
                return -1;
              }
              if (item1.Year < item2.Year)
              {
                return 1;
              }
            }
            return 0;
          }
        case SortMethod.Rating:
          {
            if (sortAscending)
            {
              if (item1.Rating > item2.Rating)
              {
                return 1;
              }
              if (item1.Rating < item2.Rating)
              {
                return -1;
              }
            }
            else
            {
              if (item1.Rating > item2.Rating)
              {
                return -1;
              }
              if (item1.Rating < item2.Rating)
              {
                return 1;
              }
            }
            return 0;
          }

        case SortMethod.Name:

          if (sortAscending)
          {
            return String.Compare(item1.Label, item2.Label, true);
          }
          else
          {
            return String.Compare(item2.Label, item1.Label, true);
          }

        case SortMethod.Label:
          if (sortAscending)
          {
            return String.Compare(item1.DVDLabel, item2.DVDLabel, true);
          }
          else
          {
            return String.Compare(item2.DVDLabel, item1.DVDLabel, true);
          }
        case SortMethod.Size:
          if (item1.FileInfo == null || item2.FileInfo == null)
          {
            if (sortAscending)
            {
              return (int)(item1.Duration - item2.Duration);
            }
            else
            {
              return (int)(item2.Duration - item1.Duration);
            }
          }
          else
          {
            if (sortAscending)
            {
               long compare = (item1.FileInfo.Length - item2.FileInfo.Length);
               return compare == 0 ? 0 : compare < 0 ? -1 : 1;
            }
            else
            {
              long compare = (item2.FileInfo.Length - item1.FileInfo.Length);
              return compare == 0 ? 0 : compare < 0 ? -1 : 1;
            }
          }

        case SortMethod.Modified:
        case SortMethod.Created:

          if (item1.FileInfo == null)
          {
            if (!this.TryGetFileInfo(ref item1))
            {
              return -1;
            }
          }

          if (item2.FileInfo == null)
          {
            if (!this.TryGetFileInfo(ref item2))
            {
              return -1;
            }
          }

          if (currentSortMethod == SortMethod.Modified)
          {
            item1.Label2 = item1.FileInfo.ModificationTime.ToShortDateString() + " " +
                           item1.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
            item2.Label2 = item2.FileInfo.ModificationTime.ToShortDateString() + " " +
                           item2.FileInfo.ModificationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          }
          else
          {
            item1.Label2 = item1.FileInfo.CreationTime.ToShortDateString() + " " +
                           item1.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
            item2.Label2 = item2.FileInfo.CreationTime.ToShortDateString() + " " +
                           item2.FileInfo.CreationTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
          }

          if (sortAscending)
          {
            if (currentSortMethod == SortMethod.Modified)
              return DateTime.Compare(item1.FileInfo.ModificationTime, item2.FileInfo.ModificationTime);
            else
              return DateTime.Compare(item1.FileInfo.CreationTime, item2.FileInfo.CreationTime);
          }
          else
          {
            if (currentSortMethod == SortMethod.Modified)
              return DateTime.Compare(item2.FileInfo.ModificationTime, item1.FileInfo.ModificationTime);
            else
              return DateTime.Compare(item2.FileInfo.CreationTime, item1.FileInfo.CreationTime);
          }
        case SortMethod.Unwatched:
          {
            int ret = 0;
            if (item1.IsPlayed && !item2.IsPlayed)
            {
              ret = 1;
              if (!sortAscending) ret = -1;
            }
            if (!item1.IsPlayed && item2.IsPlayed)
            {
              ret = -1;
              if (!sortAscending) ret = 1;
            }
            return ret;
          }
      }
      return 0;
    }

    /// <summary>
    /// In database view the file info isn't set. 
    /// This function trys to get the files from database and then creates the file info for it.
    /// </summary>
    /// <param name="item">Item to store the file info</param>
    /// <returns>True if FileInformation was created otherwise false</returns>
    private bool TryGetFileInfo(ref GUIListItem item)
    {
      if (item == null)
        return false;

      try
      {
        IMDBMovie movie1 = item.AlbumInfoTag as IMDBMovie;
        if (movie1 != null && movie1.ID > 0)
        {
          ArrayList movies1 = new ArrayList();

          VideoDatabase.GetFiles(movie1.ID, ref movies1);

          if (movies1.Count > 0)
          {
            item.FileInfo = new Util.FileInformation(movies1[0] as string, false);
          }
        }
      }
      catch (Exception exp)
      {
        Log.Error("VideoSort::TryGetFileInfo -> Exception: {0}", exp.Message);
      }

      return item.FileInfo != null;
    }
  }
}