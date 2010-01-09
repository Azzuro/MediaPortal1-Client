#region Copyright (C) 2007-2010 Team MediaPortal

/*
    Copyright (C) 2007-2010 Team MediaPortal
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

using MediaPortal.Core.Settings;

namespace UiComponents.SkinBase.Settings
{
  public class SkinBaseSettings
  {
    #region Protected fields

    protected string _dateFormat;
    protected string _timeFormat;
    protected bool _disableServerListener;

    #endregion

    [Setting(SettingScope.User, "D")]
    public string DateFormat
    {
      get { return _dateFormat; }
      set { _dateFormat = value; }
    }

    [Setting(SettingScope.User, "t")]
    public string TimeFormat
    {
      get { return _timeFormat; }
      set { _timeFormat = value; }
    }

    [Setting(SettingScope.User, false)]
    public bool DisableServerListener
    {
      get { return _disableServerListener; }
      set { _disableServerListener = value; }
    }
  }
}
