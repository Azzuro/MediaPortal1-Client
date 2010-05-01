#region Copyright (C) 2007-2010 Team MediaPortal

/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using MediaPortal.UI.SkinEngine.Controls.Panels;

namespace MediaPortal.UI.SkinEngine.Controls.Visuals
{
  public class UIElementCollection : IEnumerable<UIElement>, IDisposable
  {
    protected UIElement _parent;
    protected IList<UIElement> _elements;
    protected bool _zIndexFixed = false;

    public UIElementCollection(UIElement parent)
    {
      _parent = parent;
      _elements = new List<UIElement>();
    }

    public void Dispose()
    {
      Clear();
    }

    protected void InvalidateParent()
    {
      if (_parent != null)
        _parent.InvalidateLayout();
    }

    protected void DisposeUIElement(UIElement element)
    {
      element.VisualParent = null;
      element.SetScreen(null);
      element.Deallocate();
      element.Dispose();
    }

    public void FixZIndex()
    {
      if (SkinManagement.SkinContext.UseBatching)
        return;
      if (_zIndexFixed) return;
      _zIndexFixed = true;
      double zindex1 = 0;
      foreach (UIElement element in _elements)
      {
        if (Panel.GetZIndex(element) < 0)
        {
          Panel.SetZIndex(element, zindex1);
          zindex1 += 0.0001;
        }
        else
        {
          Panel.SetZIndex(element, Panel.GetZIndex(element)+zindex1);
          zindex1 += 0.0001;
        }
      }
    }

    public void SetParent(UIElement parent)
    {
      _parent = parent;
      foreach (UIElement element in _elements)
      {
        element.VisualParent = _parent;
        element.Screen = _parent == null ? null : _parent.Screen;
      }
      InvalidateParent();
    }

    public void Add(UIElement element)
    {
      // TODO: Allocate if we are already allocated
      element.VisualParent = _parent;
      element.Screen = _parent == null ? null : _parent.Screen;
      _elements.Add(element);
      InvalidateParent();
    }

    public void Remove(UIElement element)
    {
      if (_elements.Remove(element))
        DisposeUIElement(element);
      InvalidateParent();
    }

    public void Clear()
    {
      IList<UIElement> oldElements = _elements;
      _elements = new List<UIElement>();
      foreach (UIElement element in oldElements)
        DisposeUIElement(element);
      InvalidateParent();
    }

    public int Count
    {
      get { return _elements.Count; }
    }

    public UIElement this[int index]
    {
      get { return _elements[index]; }
      set
      {
        if (value != _elements[index])
        {
          UIElement element = _elements[index];
          DisposeUIElement(element);
          _elements[index] = value;
          if (_parent != null)
          {
            _elements[index].VisualParent = _parent;
            _parent.InvalidateLayout();
          }
        }
      }
    }

    #region IEnumerable<UIElement> Members

    public IEnumerator<UIElement> GetEnumerator()
    {
      return _elements.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _elements.GetEnumerator();
    }

    #endregion

    #region Base overrides

    public override string ToString()
    {
      return string.Format("{0}: Count={1}", typeof(UIElementCollection).Name, Count);
    }

    #endregion
  }
}
