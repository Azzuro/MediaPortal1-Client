#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace System.Windows.Input
{
  public class UICommand : RoutedCommand
  {
    #region Constructors

    public UICommand(string name, Type declaringType) : base(name, declaringType)
    {
      _text = string.Empty;
    }

    public UICommand(string name, Type declaringType, InputGestureCollection inputGestures)
      : base(name, declaringType, inputGestures)
    {
      _text = string.Empty;
    }

    public UICommand(string name, Type declaringType, InputGestureCollection inputGestures, string text)
      : base(name, declaringType, inputGestures)
    {
      _text = text;
    }

    #endregion Constructors

    #region Properties

    public string Text
    {
      get { return _text; }
      set { _text = value; }
    }

    #endregion Properties

    #region Fields

    private string _text;

    #endregion Fields
  }
}