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

using System.IO;
using MediaPortal.Thumbnails;

namespace MediaPortal.Services.ThumbnailGenerator
{
  /// <summary>
  /// Holds the data of one work item for the thumbnail generator thread.
  /// </summary>
  public class WorkItem
  {
    public FileSystemInfo _source;
    public FileInfo _destination;
    public int _quality;
    public int _width;
    public int _height;
    public CreatedDelegate _createdDelegate;

    public WorkItem(FileSystemInfo source, FileInfo destination, int width, int height, int quality,
      CreatedDelegate createdDelegate)
    {
      _width = width;
      _height = height;
      _quality = quality;

      _source = source;
      _destination = destination;
      _createdDelegate = createdDelegate;
    }

    public FileSystemInfo Source
    {
      get { return _source; }
    }

    public FileInfo Destination
    {
      get { return _destination; }
    }

    public int Width
    {
      get { return _width; }
    }

    public int Height
    {
      get { return _height; }
    }

    public int Quality
    {
      get { return _quality; }
    }

    public CreatedDelegate CreatedDelegate
    {
      get { return _createdDelegate; }
    }
  }
}
