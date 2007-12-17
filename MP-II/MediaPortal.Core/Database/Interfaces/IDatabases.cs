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

namespace MediaPortal.Core.Database.Interfaces
{
  /// <summary>
  /// service which returns all databases registered
  /// </summary>
  public interface IDatabases
  {
    /// <summary>
    /// Gets a list of all databases registered.
    /// </summary>
    /// <value>The databases registered.</value>
    List<IDatabase> DatabasesRegistered { get;}

    /// <summary>
    /// Determines whether the specified database has been registered
    /// </summary>
    /// <param name="databaseName">Name of the database.</param>
    /// <returns>
    /// 	<c>true</c> if the specified database is registered; otherwise, <c>false</c>.
    /// </returns>
    bool Contains(string databaseName);
  }
}
