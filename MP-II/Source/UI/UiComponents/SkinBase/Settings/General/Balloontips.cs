#region Copyright (C) 2007-2008 Team MediaPortal

/*
 *  Copyright (C) 2007-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This file is part of MediaPortal II
 *
 *  MediaPortal II is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  MediaPortal II is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

#endregion

using System;
using MediaPortal.Configuration.ConfigurationClasses;
using MediaPortal.Core;
using MediaPortal.Core.Logging;
using MediaPortal.Utilities.SystemAPI;

namespace UiComponents.SkinBase.Settings.System
{
  public class Balloontips : YesNo
  {
    #region Base overrides

    public override Type SettingsObjectType
    {
      get { return null; }
    }

    public override void Load(object settingsObject)
    {
      _yes = WindowsAPI.IsShowBalloonTips;
    }

    public override void Save(object settingsObject)
    {
      try
      {
        WindowsAPI.IsShowBalloonTips = _yes;
      }
      catch (Exception ex)
      {
        ServiceScope.Get<ILogger>().Error("Can't change 'Enable Balloon Tips' value in registry", ex);
      }
    }

    #endregion
  }
}