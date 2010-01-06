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

using MediaPortal.Drawing;

namespace System.Windows
{
  public class SizeChangedInfo
  {
    #region Constructors

    public SizeChangedInfo(UIElement element, Size previousSize, bool widthChanged, bool heightChanged)
    {
      _previousSize = previousSize;
      _widthChanged = widthChanged;
      _heightChanged = heightChanged;
    }

    #endregion Constructors

    #region Properties

    public bool HeightChanged
    {
      get { return _heightChanged; }
    }

    public Size NewSize
    {
      get { return _newSize; }
    }

    public Size PreviousSize
    {
      get { return _previousSize; }
    }

    public bool WidthChanged
    {
      get { return _widthChanged; }
    }

    #endregion Properties

    #region Fields

    private bool _heightChanged;
    private Size _newSize = Size.Empty;
    private Size _previousSize;
    private bool _widthChanged;

    #endregion Fields
  }
}