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
using MediaPortal.Core;
using MediaPortal.Core.InputManager;
using MediaPortal.Core.Players;
using MediaPortal.Core.Properties;
using MediaPortal.Core.WindowManager;

namespace SkinEngine.Commands
{
  internal class ObjectFactory
  {
    /// <summary>
    /// returns an object for the specified window & name
    /// </summary>
    /// <param name="window">The window.</param>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public static object GetObject(IWindow window, string name)
    {
      return GetObject(null, window, name);
    }

    public static object GetObject(IControl control, IWindow window, string name)
    {
      if (name == "this")
      {
        return control;
      }

      if (name == "container")
      {
        if (control == null)
        {
          return null;
        }
        return control.Container;
      }

      if (name == "WindowManager")
      {
        WindowManager manager = (WindowManager)ServiceScope.Get<IWindowManager>();
        return manager;
      }

      if (name == "InputManager")
      {
        IInputManager manager = ServiceScope.Get<IInputManager>();
        return manager;
      }

      if (name == "Players")
      {
        return ServiceScope.Get<PlayerCollection>();
      }
      /*object controlFound = ((Window)window).GetControlByName(name);
      if (controlFound != null)
      {
        return controlFound;
      }

      Model model = ((Window)window).GetModelByName(name);
      if (model != null)
      {
        return model.Instance;
      }*/
      Model model = ModelManager.Instance.GetModelByName(name);
      if (model != null)
      {
        return model.Instance;
      }
      WindowManager windowManager = (WindowManager)ServiceScope.Get<IWindowManager>();
      if (windowManager.Contains(name))
      {
        return windowManager.GetWindow(name);
      }

      return null;
    }
  }
}
