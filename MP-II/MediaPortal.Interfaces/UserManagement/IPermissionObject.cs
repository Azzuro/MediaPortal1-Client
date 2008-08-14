#region Copyright (C) 2007-2008 Team MediaPortal

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

namespace MediaPortal.Services.UserManagement
{
  /// <summary>
  /// Interface for a permisson object.
  /// </summary>
  public interface IPermissionObject
  {
    /// <summary>
    /// Checks if this permission includes the permission on the specified object, which
    /// will be checked for various critera (f.e. checks if the path name of a share is contained in the
    /// folder represented by this permission object).
    /// </summary>
    /// <param name="obj">Permission object to check.</param>
    /// <returns><c>true</c>, if the permission on the given object is included by this permission object,
    /// else <c>false</c>.</returns>
    bool IncludesObject(IPermissionObject obj);
  }
}
