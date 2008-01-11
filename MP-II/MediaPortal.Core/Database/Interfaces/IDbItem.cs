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

namespace MediaPortal.Core.Database.Interfaces
{
  public interface IDbItem
  {
    /// <summary>
    /// Gets the attributes for this item
    /// </summary>
    /// <value>The attributes.</value>
    Dictionary<string, IDbAttribute> Attributes { get; }

    /// <summary>
    /// Gets the database.
    /// </summary>
    /// <value>The database.</value>
    IDatabase Database { get; }

    /// <summary>
    /// save item in the database
    /// </summary>
    void Save();

    /// <summary>
    /// delete the item
    /// </summary>
    void Delete();

    /// <summary>
    /// Gets or sets the value for the specified attribute
    /// </summary>
    /// <value></value>
    object this[string key] { get; set; }
  }
}
