﻿#region Copyright (C) 2007-2008 Team MediaPortal

/*
    Copyright (C) 2007-2008 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System.Collections.Generic;
using MediaPortal.Presentation.DataObjects;

namespace Models.Extensions
{
  public enum SortOption
  {
    Name,
    Downloads,
    Size,
    Date
  } ;

  internal class ExtensionComparer : IComparer<ListItem>
  {
    private readonly SortOption _sortOption;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicComparer"/> class.
    /// </summary>
    /// <param name="option">The option.</param>
    public ExtensionComparer(SortOption option)
    {
      _sortOption = option;
    }

    #region IComparer<ListItem> Members

    public int Compare(ListItem x, ListItem y)
    {
      FolderItem item1 = x as FolderItem;
      FolderItem item2 = y as FolderItem;

      if (item1 != null && item2 == null)
      {
        return -1;
      }
      if (item2 != null && item1 == null)
      {
        return 1;
      }
      if (item1 != null)
      {
        //both are folders
        string label1 = item1.Label("Name", "").Evaluate();
        string label2 = item2.Label("Name", "").Evaluate();
        if (label1 == label2)
        {
          return 0;
        }
        if (label1 == "..")
        {
          return -1;
        }
        if (label2 == "..")
        {
          return 1;
        }
        return string.Compare(label1, label2);
      }
      //both are files...
      switch (_sortOption)
      {
        case SortOption.Name:
          {
            string label1 = x.Label("Name", "").Evaluate();
            string label2 = y.Label("Name", "").Evaluate();
            return string.Compare(label1, label2);
          }
        case SortOption.Downloads:
          {
            ExtensionItem m1 = (ExtensionItem) x;
            ExtensionItem m2 = (ExtensionItem) y;
            return m1.Item.Downloads.CompareTo(m2.Item.Downloads);
          }
        case SortOption.Size:
          {
            ExtensionItem m1 = (ExtensionItem) x;
            ExtensionItem m2 = (ExtensionItem) y;
            return m1.Item.Size.CompareTo(m2.Item.Size);
          }
        case SortOption.Date:
          {
            ExtensionItem m1 = (ExtensionItem)x;
            ExtensionItem m2 = (ExtensionItem)y;
            return m1.Item.Date.CompareTo(m2.Item.Date);
          }
      }
      return 0;
    }

    #endregion
  }
}
