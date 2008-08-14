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

using System;
using System.Collections.Generic;
using MediaPortal.Core.UserManagement;

namespace MediaPortal.Services.UserManagement
{
  /// <summary>
  /// Class which stores user data.
  /// </summary>
  public class User : IUser
  {
    protected string _userName = "";
    protected string _image = "";
    protected string _password = "";
    protected bool _needsPassword = false;
    protected DateTime _lastLogin;
    protected List<IRole> _roles;

    public User(string name, bool needsPassword, DateTime lastLogin, string image)
    {
      UserName = name;
      NeedsPassword = needsPassword;
      LastLogin = lastLogin;
      UserImage = image;
      _roles = new List<IRole>();
    }

    public string UserName
    {
      get { return _userName; }
      set { _userName = value; }
    }

    public string Password
    {
      get { return _password; }
      set { _password = value; }
    }

    public bool NeedsPassword
    {
      get { return _needsPassword; }
      set { _needsPassword = value; }
    }

    public DateTime LastLogin
    {
      get { return _lastLogin; }
      set { _lastLogin = value; }
    }

    public string UserImage
    {
      get { return _image; }
      set { _image = @"media\users\" + value; }
    }

    public bool AddRole(IRole role)
    {
      _roles.Add(role);
      return true;
    }

    public bool RemoveRole(IRole role)
    {
      return _roles.Remove(role);
    }

    public IList<IRole> GetRoles()
    {
      return _roles;
    }

    public bool HasPermissionOn(IPermissionObject obj)
    {
      foreach (IRole role in GetRoles())
        foreach (IPermission permission in role.GetPermissions())
          if (permission.HasPermissionOn(obj))
            return true;
      return false;
    }

    public override int GetHashCode()
    {
      return _userName == null ? 0 : _userName.GetHashCode();
    }

    public override bool Equals(object other)
    {
      if (other is User)
        return string.Compare(_userName, ((User) other)._userName, false);
      else
        return false;
    }
  }
}
