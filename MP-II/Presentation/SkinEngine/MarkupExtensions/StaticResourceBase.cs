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

using Presentation.SkinEngine.Xaml.Exceptions;
using Presentation.SkinEngine.SkinManagement;
using Presentation.SkinEngine.Xaml;
using Presentation.SkinEngine.Xaml.Interfaces;
using Presentation.SkinEngine.Controls.Visuals;
using Presentation.SkinEngine.MpfElements.Resources;

namespace Presentation.SkinEngine.MarkupExtensions
{
  /// <summary>
  /// Base class for MPF static resource lookup markup extensions
  /// </summary>
  public class StaticResourceBase
  {
    protected object FindResourceInParserContext(string resourceKey, IParserContext context)
    {
      // Step up the parser's context stack to find the resource.
      // The logical tree is not yet defined at the load time of the
      // XAML file. This is the reason we have to step up the parser's context
      // stack. We will have to simulate the process of finding a resource
      // which is normally done by <see cref="FindResource(string)"/>.
      // The parser's context stack maintains a dictionary of current keyed
      // elements for each stack level because the according resource
      // dictionaries are not built yet.
      foreach (ElementContextInfo current in context.ContextStack)
      {
        if (current.ContainsKey(resourceKey))
          return current.GetKeyedElement(resourceKey);
        else if (current.Instance is UIElement &&
            ((UIElement) current.Instance).Resources.ContainsKey(resourceKey))
        {
          // Don't call UIElement.FindResource here, because the logical tree
          // may be not set up yet.
          return ((UIElement) current.Instance).Resources[resourceKey];
        }
        else if (current.Instance is ResourceDictionary)
        {
          ResourceDictionary rd = (ResourceDictionary) current.Instance;
          if (rd.ContainsKey(resourceKey))
            return rd[resourceKey];
        }
      }
      return null;
    }

    protected object FindResourceInTheme(string resourceKey)
    {
      return SkinContext.SkinResources.FindStyleResource(resourceKey);
    }
  }
}
