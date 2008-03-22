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

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MediaPortal.Core;
using MediaPortal.Presentation.Collections;
using MediaPortal.Presentation.Properties;
using MediaPortal.Core.UserManagement;
using MediaPortal.Core.PluginManager;
using MediaPortal.Services.UserManagement;

namespace Models.Login
{
  /// <summary>
  /// viewmodel for handling logins
  /// </summary>
  public class LoginModel : IPlugin
  {
    private ItemsCollection _usersExposed = new ItemsCollection();
    private Property _currentUser;

    #region IPlugin Members
    public void Initialize(string id)
    {
    }

    public void Dispose()
    {
    }

    /// <summary>
    /// constructor
    /// </summary>
    public LoginModel()
    {
      _currentUser = new Property(null);
      LoadUsers();
    }
    #endregion

    /// <summary>
    /// will load the users from somewhere
    /// </summary>
    private void LoadUsers()
    {
      // add a few dummy users, later this should be more flexible and handled by a login manager / user account control
      User u1 =
        new User(SystemInformation.ComputerName.ToLower(), true, new DateTime(2007, 10, 25, 12, 20, 30),
                 SystemInformation.ComputerName.ToLower() + ".jpg");
      User u2 =
        new User(SystemInformation.UserName.ToLower(), false, new DateTime(2007, 10, 26, 10, 30, 13),
                 SystemInformation.UserName.ToLower() + ".jpg");
      ServiceScope.Get<IUserService>().AddUser(u1);
      ServiceScope.Get<IUserService>().AddUser(u2);
      RefreshUserList();
    }

    /// <summary>
    /// this will turn the _users list into the _usersExposed list
    /// </summary>
    private void RefreshUserList()
    {
      List<IUser> users = ServiceScope.Get<IUserService>().GetUsers();
      // clear the exposed users list
      Users.Clear();
      // add users to expose them
      ListItem buff = null;
      foreach (IUser user in users)
      {
        if (user == null)
        {
          continue;
        }
        buff = new ListItem();
        buff.Add("UserName", user.UserName);
        buff.Add("UserImage", user.UserImage);
        if (user.NeedsPassword)
        {
          buff.Add("NeedsPassword", "true");
        }
        else
        {
          buff.Add("NeedsPassword", "false");
        }
        buff.Add("LastLogin", user.LastLogin.ToString("G"));
        Users.Add(buff);
      }
      // tell the skin that something might have changed
      Users.FireChange();
    }

    /// <summary>
    /// selects a user
    /// </summary>
    /// <param name="item"></param>
    public void SelectUser(ListItem item)
    {
      List<IUser> users = ServiceScope.Get<IUserService>().GetUsers();

      foreach (IUser user in users)
      {
        if (user.UserName == item.Labels["UserName"].Evaluate(null, null))
        {
          CurrentUser = user;
          return;
        }
      }
    }

    /// <summary>
    /// exposes the current user to the skin
    /// </summary>
    public Property CurrentUserProperty
    {
      get { return _currentUser; }
      set { _currentUser = value; }
    }

    /// <summary>
    /// exposes the current user to the skin
    /// </summary>
    public IUser CurrentUser
    {
      get { return (IUser) _currentUser.GetValue(); }
      set { _currentUser.SetValue(value); }
    }

    /// <summary>
    /// exposes the users to the skin
    /// </summary>
    public ItemsCollection Users
    {
      get { return _usersExposed; }
    }
  }
}
