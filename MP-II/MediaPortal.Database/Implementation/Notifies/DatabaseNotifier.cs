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
using MediaPortal.Core.Database.Interfaces;

namespace MediaPortal.Database.Implementation
{
  public class DatabaseNotifier : IDatabaseNotifier
  {
    private List<IDatabaseNotification> _registrations;

    public DatabaseNotifier()
    {
      _registrations = new List<IDatabaseNotification>();
    }

    public void Register(IDatabaseNotification notification)
    {
      _registrations.Add(notification);
    }

    public void UnRegister(IDatabaseNotification notification)
    {
      _registrations.Remove(notification);
    }

    public void Notify(IDatabase database, DatabaseNotificationType notificationType, IDbItem item)
    {
      foreach (IDatabaseNotification notify in _registrations)
      {
        notify.OnNotify(database, notificationType, item);
      }
    }
  }
}