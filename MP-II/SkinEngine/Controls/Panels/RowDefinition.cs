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
using MediaPortal.Utilities.DeepCopy;

namespace MediaPortal.SkinEngine.Controls.Panels
{
  public class RowDefinition : DefinitionBase
  {
    Property _heightProperty;

    public RowDefinition()
    {
      Init();
    }

    void Init()
    {
      _heightProperty = new Property(typeof(GridLength), new GridLength());
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      RowDefinition rd = source as RowDefinition;
      Height = copyManager.GetCopy(rd.Height);
    }

    public Property HeightProperty
    {
      get { return _heightProperty; }
    }

    public GridLength Height
    {
      get { return _heightProperty.GetValue() as GridLength; }
      set { _heightProperty.SetValue(value); }
    }
  }
}
