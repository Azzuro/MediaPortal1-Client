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
using MediaPortal.Control.InputManager;
using MediaPortal.SkinEngine.SkinManagement;

namespace MediaPortal.SkinEngine
{
  public class InputManager : IInputManager
  {
    #region Protected fields

    private ICollection<Key> _registeredKeys;
    private bool _needRawKeyboardData;

    #endregion

    #region events

    public event MouseMoveHandler OnMouseMove;
    public event KeyPressedHandler OnKeyPressed;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="InputManager"/> class.
    /// </summary>
    public InputManager()
    {
      _needRawKeyboardData = false;
      _registeredKeys = new List<Key>();
      _registeredKeys.Add(Key.ContextMenu);
      _registeredKeys.Add(Key.Down);
      _registeredKeys.Add(Key.DvdDown);
      _registeredKeys.Add(Key.DvdLeft);
      _registeredKeys.Add(Key.DvdMenu);
      _registeredKeys.Add(Key.DvdRight);
      _registeredKeys.Add(Key.DvdSelect);
      _registeredKeys.Add(Key.DvdUp);
      _registeredKeys.Add(Key.End);
      _registeredKeys.Add(Key.Enter);
      _registeredKeys.Add(Key.Home);
      _registeredKeys.Add(Key.Left);
      _registeredKeys.Add(Key.None);
      _registeredKeys.Add(Key.PageDown);
      _registeredKeys.Add(Key.PageUp);
      _registeredKeys.Add(Key.Right);
      _registeredKeys.Add(Key.Up);
      _registeredKeys.Add(Key.ZoomMode);
      _registeredKeys.Add(Key.Space);
    }

    public void Reset()
    {
      OnMouseMove = null;
      OnKeyPressed = null;
    }

    /// <summary>
    /// returns all registered keys.
    /// </summary>
    /// <value>The keys.</value>
    public ICollection<Key> Keys
    {
      get { return _registeredKeys; }
    }

    /// <summary>
    /// called by window when a mouse move is detected
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    public void MouseMove(float x, float y)
    {
      SkinContext.HandlingInput = true;
      SkinContext.MouseUsed = true;
      if (OnMouseMove != null)
      {
        OnMouseMove(x, y);
      }
      SkinContext.HandlingInput = false;
      SkinContext.ScreenSaverActive = false;
    }

    /// <summary>
    /// called by window when a keypress has been received
    /// </summary>
    /// <param name="key">The key.</param>
    public void KeyPressed(Key key)
    {
      SkinContext.HandlingInput = true;
      if (OnKeyPressed != null)
      {
        OnKeyPressed(ref key);
      }

      SkinContext.HandlingInput = false;
      SkinContext.ScreenSaverActive = false;
    }

    /// <summary>
    /// Called by the skin when it wants to press a key
    /// </summary>
    /// <param name="keyName">string containing the key name.</param>
    public void PressKey(string keyName)
    {
      SkinContext.HandlingInput = true;
      SkinContext.ScreenSaverActive = false;
      foreach (Key key in Keys)
      {
        if (String.Compare(keyName, key.Name, true) == 0)
        {
          Key k = key;
          if (OnKeyPressed != null)
          {
            OnKeyPressed(ref k);
          }
        }
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether skinengine needs raw key data (for a textbox for example)
    /// </summary>
    /// <value><c>true</c> if [need raw key data]; otherwise, <c>false</c>.</value>
    public bool NeedRawKeyData
    {
      get { return _needRawKeyboardData; }
      set { _needRawKeyboardData = value; }
    }
  }
}
