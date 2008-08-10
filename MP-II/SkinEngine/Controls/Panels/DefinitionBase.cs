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

namespace Presentation.SkinEngine.Controls.Panels
{
  public class DefinitionBase : IDeepCopyable
  {
    Property _nameProperty;

    public DefinitionBase()
    {
      Init();
    }

    void Init()
    {
      _nameProperty = new Property(typeof(string), "");
    }

    public virtual void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      DefinitionBase d = source as DefinitionBase;
      Name = copyManager.GetCopy(d.Name);
    }

    public Property NameProperty
    {
      get { return _nameProperty; }
    }

    public string Name
    {
      get { return _nameProperty.GetValue() as string; }
      set { _nameProperty.SetValue(value); }
    }
  }
}
