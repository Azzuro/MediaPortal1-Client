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

using MediaPortal.Presentation.DataObjects;
using MediaPortal.Control.InputManager;
using Presentation.SkinEngine;
using Presentation.SkinEngine.Commands;
using MediaPortal.Utilities.DeepCopy;

namespace Presentation.SkinEngine.Controls.Visuals
{
  public class Button : ContentControl
  {
    #region Protected fields

    protected Property _isPressedProperty;

    protected Property _commandProperty;

    #endregion

    #region Ctor

    public Button()
    {
      Init();
    }

    void Init()
    {
      _isPressedProperty = new Property(typeof(bool), false);
      _commandProperty = new Property(typeof(IExecutableCommand), null);
      Focusable = true;
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      Button b = source as Button;
      Command = copyManager.GetCopy(b.Command);
    }

    #endregion

    #region Public properties

    public override bool HasFocus
    {
      get { return base.HasFocus; }
      set
      {
        base.HasFocus = value;
        if (value == false)
          IsPressed = false;
        //Trace.WriteLine(String.Format("{0} focus:{1}", Name, value));
      }
    }

    public Property IsPressedProperty
    {
      get { return _isPressedProperty; }
    }

    public bool IsPressed
    {
      get { return (bool)_isPressedProperty.GetValue(); }
      set { _isPressedProperty.SetValue(value); }
    }

    #region Command properties

    public Property CommandProperty
    {
      get { return _commandProperty; }
      set { _commandProperty = value; }
    }

    public IExecutableCommand Command
    {
      get { return (IExecutableCommand) _commandProperty.GetValue(); }
      set { _commandProperty.SetValue(value); }
    }

    #endregion

    #endregion
    
    public override void OnKeyPressed(ref Key key)
    {
      if (!HasFocus) return;
      if (key == MediaPortal.Control.InputManager.Key.None) return;
      if (key == MediaPortal.Control.InputManager.Key.Enter)
      {
        IsPressed = true;
        if (Command != null)
          Command.Execute();
      }

      UIElement cntl = FocusManager.PredictFocus(this, ref key);
      if (cntl != null)
      {
        cntl.HasFocus = true;
        key = MediaPortal.Control.InputManager.Key.None;
      }
    }
  }
}
