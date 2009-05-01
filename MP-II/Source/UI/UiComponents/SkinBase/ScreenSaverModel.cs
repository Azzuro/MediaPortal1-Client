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
using MediaPortal.Control.InputManager;
using MediaPortal.Core;
using MediaPortal.Presentation.DataObjects;
using MediaPortal.Presentation.Models;
using MediaPortal.Presentation.Screens;

namespace UiComponents.SkinBase
{
  /// <summary>
  /// This model provides information about the screen saver and mouse controls state. It provides a copy of the
  /// <see cref="IScreenControl.IsScreenSaverActive"/> and <see cref="IInputManager.IsMouseUsed"/> data, but as
  /// <see cref="Property"/> to enable the screen controls to bind to the data.
  /// </summary>
  public class ScreenSaverModel : BaseTimerControlledUIModel
  {
    public const string SCREENSAVER_MODEL_ID_STR = "D4B7FEDD-243F-4afc-A8BE-28BBBF17D799";

    protected Property _isScreenSaverActiveProperty;
    protected Property _isMouseUsedProperty;

    public ScreenSaverModel() : base(100)
    {
      _isScreenSaverActiveProperty = new Property(typeof(bool), false);
      _isMouseUsedProperty = new Property(typeof(bool), false);

      Update();
      SubscribeToMessages();
    }

    public override Guid ModelId
    {
      get { return new Guid(SCREENSAVER_MODEL_ID_STR); }
    }

    protected override void Update()
    {
      IScreenControl screenControl = ServiceScope.Get<IScreenControl>();
      IInputManager inputManager = ServiceScope.Get<IInputManager>();
      IsScreenSaverActive = screenControl.IsScreenSaverActive;
      IsMouseUsed = inputManager.IsMouseUsed;
    }
  
    public Property IsScreenSaverActiveProperty
    {
      get { return _isScreenSaverActiveProperty; }
    }

    public bool IsScreenSaverActive
    {
      get { return (bool) _isScreenSaverActiveProperty.GetValue(); }
      internal set { _isScreenSaverActiveProperty.SetValue(value); }
    }

    public Property IsMouseUsedProperty
    {
      get { return _isMouseUsedProperty; }
    }

    public bool IsMouseUsed
    {
      get { return (bool) _isMouseUsedProperty.GetValue(); }
      internal set { _isMouseUsedProperty.SetValue(value); }
    }
  }
}
