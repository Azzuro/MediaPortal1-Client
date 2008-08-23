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
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.Win32;

using MediaPortal.Core;
using MediaPortal.Presentation.Localisation;
using MediaPortal.Core.Logging;
using MediaPortal.Core.PathManager;
using MediaPortal.Core.Settings;
using MediaPortal.Configuration;
using MediaPortal.Configuration.Settings;

namespace Components.Configuration.Settings
{
  public class Autostart : YesNo
  {

    #region Constructor

    public Autostart()
    {
      base.SetSettingsObject(new SystemSettings());
    }

    #endregion

    #region Public Methods

    public override void Load(object settingsObject)
    {
      base._yes = ((SystemSettings)settingsObject).Autostart;
    }

    public override void Save(object settingsObject)
    {
      ((SystemSettings)settingsObject).Autostart = base._yes;
      Commit();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Commits the autostart setting to the registry.
    /// </summary>
    private void Commit()
    {
      try
      {
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"software\microsoft\windows\currentversion\run"))
        {
          if (base._yes)
            key.SetValue("MediaPortal II", ServiceScope.Get<IPathManager>().GetPath("APPLICATION_ROOT") + @"\MediaPortal.exe", RegistryValueKind.ExpandString);
          else
            key.DeleteValue("MediaPortal II", false);
        }
      }
      catch (System.Security.SecurityException ex)
      {
        ServiceScope.Get<ILogger>().Error("SecurityException: {0} -> Can't write autostart-value \"{1}\" to registry.", ex.Message, base._yes);
      }
      catch (UnauthorizedAccessException ex)
      {
        ServiceScope.Get<ILogger>().Error("UnauthorizedAccessException: {0} -> Can't write autostart-value \"{1}\" to registry.", ex.Message, base._yes);
      }
      catch (IOException ex)
      {
        ServiceScope.Get<ILogger>().Error("IOException: {0} -> Can't write autostart-value \"{1}\" to registry.", ex.Message, base._yes);
      }
      catch (Exception ex)
      {
        ServiceScope.Get<ILogger>().Error("Can't write autostart-value \"{0}\" to registry.", ex, base._yes);
      }
    }

    #endregion

  }
}
