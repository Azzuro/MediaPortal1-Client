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
using MediaPortal.Control.InputManager;
using MediaPortal.Core;
using MediaPortal.Presentation.Actions;
using MediaPortal.Presentation.Workflow;

namespace MediaPortal.SkinEngine.InputManagement
{
  /// <summary>
  /// Delegate for a mouse handler.
  /// </summary>
  /// <param name="x">X coordinate of the new mouse position.</param>
  /// <param name="y">Y coordinate of the new mouse position.</param>
  public delegate void MouseMoveHandler(float x, float y);

  /// <summary>
  /// Delegate for a key handler.
  /// </summary>
  /// <param name="key">The key which was pressed. This parmeter should be set to <see cref="Key.None"/> when the
  /// key was consumed.</param>
  public delegate void KeyPressedHandler(ref Key key);

  /// <summary>
  /// Input manager class which provides the public <see cref="IInputManager"/> interface and some other
  /// skin engine internal functionality.
  /// </summary>
  public class InputManager : IInputManager
  {
    #region Protected fields

    protected DateTime _lastMouseUsageTime = DateTime.MinValue;
    protected DateTime _lastInputTime = DateTime.MinValue;

    protected static InputManager _instance = null;

    #endregion

    public InputManager() { }

    public static InputManager Instance
    {
      get
      {
        if (_instance == null)
          _instance = new InputManager();
        return _instance;
      }
    }

    /// <summary>
    /// Can be registered by classes of the skin engine to be informed about mouse movements.
    /// </summary>
    public event MouseMoveHandler MouseMoved;

    /// <summary>
    /// Can be registered by classes of the skin engine to be informed about key events.
    /// </summary>
    public event KeyPressedHandler KeyPressed;

    /// <summary>
    /// Can be registered by classes of the skin engine to preview key events before shortcuts are triggered.
    /// </summary>
    public event KeyPressedHandler KeyPreview;

    #region IInputManager implementation

    public DateTime LastMouseUsageTime
    {
      get { return _lastMouseUsageTime; }
      internal set { _lastMouseUsageTime = value; }
    }

    public DateTime LastInputTime
    {
      get { return _lastInputTime; }
      internal set { _lastInputTime = value; }
    }

    public void MouseMove(float x, float y)
    {
      DateTime now = DateTime.Now;
      _lastInputTime = now;
      _lastMouseUsageTime = now;
      if (MouseMoved != null)
        MouseMoved(x, y);
    }

    public void KeyPress(Key key)
    {
      _lastInputTime = DateTime.Now;
        if (KeyPreview != null)
          KeyPreview(ref key);
      if (key == Key.None)
        return;
      // Try shortcuts...
      NavigationContext navigationContext = ServiceScope.Get<IWorkflowManager>().CurrentNavigationContext;
      CommandShortcut shortcut = navigationContext.GetShortcut(key);
      if (shortcut == null)
      {
        if (KeyPressed != null)
          KeyPressed(ref key);
      }
      else
        shortcut.Action();
    }

    #endregion
  }
}