﻿#region Copyright (C) 2007 Team MediaPortal

/*
    Copyright (C) 2007 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Services.Importers
{
  public class FileChangeEvent
  {
    #region Enum
    public enum FileChangeType
    {
      Deleted,
      Renamed,
      Created,
      Changed,
      DirectoryDeleted
    }
    #endregion

    #region Variables
    private FileChangeType _type;
    private string _strFilename;
    private string _strOldFilename;
    #endregion

    #region Constructors/Destructors
    public FileChangeEvent()
    {
      _strFilename = null;
      _strOldFilename = null;
    }
    #endregion

    #region Properties
    public FileChangeType Type
    {
      get { return _type; }
      set { _type = value; }
    }

    public string FileName
    {
      get { return _strFilename; }
      set { _strFilename = value; }
    }

    public string OldFileName
    {
      get { return _strOldFilename; }
      set { _strOldFilename = value; }
    }
    #endregion
  }
}