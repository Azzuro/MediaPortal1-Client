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

using MediaPortal.FileEventNotification;

namespace MediaPortal.Core.Services.FileEventNotification
{
  struct EventData
  {

    #region Variables

    private readonly FileWatchInfo _fileWatchInfo;
    private readonly IFileWatchEventArgs _fileWatchEventArgs;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the FileWatchInfo to report to.
    /// </summary>
    public FileWatchInfo Info
    {
      get { return _fileWatchInfo; }
    }

    /// <summary>
    /// Gets the extra arguments regarding the event.
    /// </summary>
    public IFileWatchEventArgs Args
    {
      get { return _fileWatchEventArgs; }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of EventData which holds an instance of FileWatchInfo
    /// and an instance of FileWatchEventArgs.
    /// </summary>
    /// <param name="fileWatchInfo"></param>
    /// <param name="fileWatchEventArgs"></param>
    public EventData(FileWatchInfo fileWatchInfo, IFileWatchEventArgs fileWatchEventArgs)
    {
      _fileWatchInfo = fileWatchInfo;
      _fileWatchEventArgs = fileWatchEventArgs;
    }

    #endregion

  }
}
