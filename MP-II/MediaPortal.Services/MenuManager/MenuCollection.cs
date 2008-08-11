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
using System.Text;
using MediaPortal.Core;
using MediaPortal.Presentation.MenuManager;
namespace MediaPortal.Services.MenuManager
{
  public class MenuCollection : IMenuCollection
  {
    private Dictionary<string, IMenu> _menus;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuManager"/> class.
    /// </summary>
    public MenuCollection()
    {
      _menus = new Dictionary<string, IMenu>();
    }

    /// <summary>
    /// register a menu
    /// </summary>
    /// <param name="name">menu alias name</param>
    /// <param name="menu">the menu</param>
    public void Register(string name, IMenu menu)
    {
      _menus[name] = menu;
    }

    /// <summary>
    /// returns if the menu with the name exists
    /// </summary>
    /// <param name="name">alias name of the manu</param>
    /// <returns>true if exists, otherwise false</returns>
    public bool Contains(string name)
    {
      return _menus.ContainsKey(name);
    }

    /// <summary>
    /// returns the menu
    /// </summary>
    /// <param name="name">menu alias name</param>
    /// <returns>Menu or null if menu does not exists</returns>
    public IMenu GetMenu(string name)
    {
      if (!Contains(name))
      {
        IMenuBuilder builder = ServiceScope.Get<IMenuBuilder>();
        Register(name, builder.Build(name));
      }
      return _menus[name];
    }

    /// <summary>
    /// Gets the number of menus
    /// </summary>
    /// <value>The count.</value>
    public int Count
    {
      get { return _menus.Count; }
    }

    /// <summary>
    /// returns all menus.
    /// </summary>
    /// <value>The menus.</value>
    public Dictionary<string, IMenu> Menus
    {
      get { return _menus; }
    }
  }
}
