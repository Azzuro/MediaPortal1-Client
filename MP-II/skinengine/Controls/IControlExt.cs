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

using MediaPortal.Core.InputManager;
using MediaPortal.Core.Properties;

namespace SkinEngine.Controls
{
  public interface IControlExt : IControl
  {
    void OnKeyPressed(ref Key key);
    void OnMouseMove(float x, float y);
    string Name { get; }
    bool IsAnimating { get; }
    bool HasFocus { get; set; }

    //void UpdateProperties();
    void DoRender(uint time);
    void Reset();
  }
}