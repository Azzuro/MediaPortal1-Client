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

using System.Collections;
using MediaPortal.ExtensionMethods;

namespace System.Windows
{
  public sealed class UIElementCollection : CollectionBase, IDisposable
  {
    #region Methods

    public void Add(UIElement element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      List.Add(element);
    }

    public bool Contains(UIElement element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      return List.Contains(element);
    }

    public void CopyTo(UIElement[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw new ArgumentNullException("array");
      }

      List.CopyTo(array, arrayIndex);
    }

    public int IndexOf(UIElement element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      return List.IndexOf(element);
    }

    public void Insert(int index, UIElement element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      List.Insert(index, element);
    }

    public bool Remove(UIElement element)
    {
      if (element == null)
      {
        throw new ArgumentNullException("element");
      }

      if (List.Contains(element) == false)
      {
        return false;
      }

      List.Remove(element);

      return true;
    }

    #endregion Methods

    #region Properties

    public UIElement this[int index]
    {
      get { return (UIElement)List[index]; }
      set { List[index] = value; }
    }

    #endregion Properties

    #region IDisposable Members

    public void Dispose()
    {      
      List.Dispose(); //only dispose items, do not remove collection.
    }

    #endregion
  }
}