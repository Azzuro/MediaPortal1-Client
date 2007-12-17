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

using System.Collections.Generic;

using MediaPortal.Core.Settings;

namespace MediaPortal.MyInput
{

  /// <summary>
  /// Settings class for MyInput.
  /// </summary>
  public class MyInputSettings
  {

    #region Variables

    bool _firstRun;
    string _serverHost;

    List<MappedKeyCode> _remoteMap;

    #endregion Variables

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether this is the first run.
    /// </summary>
    /// <value><c>true</c> if this is the first run; otherwise, <c>false</c>.</value>
    [Setting(SettingScope.Global, true)]
    public bool FirstRun
    {
      get { return _firstRun; }
      set { _firstRun = value; }
    }

    /// <summary>
    /// Gets or sets the server host.
    /// </summary>
    /// <value>The server host.</value>
    [Setting(SettingScope.Global, "localhost")]
    public string ServerHost
    {
      get { return _serverHost; }
      set { _serverHost = value; }
    }

    /// <summary>
    /// Gets or sets the remote map.
    /// </summary>
    /// <value>The remote map.</value>
    [Setting(SettingScope.User, null)]
    public List<MappedKeyCode> RemoteMap
    {
      get { return _remoteMap; }
      set { _remoteMap = value; }
    }


    #endregion Properties

  }

}
