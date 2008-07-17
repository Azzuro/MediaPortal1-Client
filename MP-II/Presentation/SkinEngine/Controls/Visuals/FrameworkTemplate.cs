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

using System.Collections.Generic;
using Presentation.SkinEngine.MpfElements.Resources;
using Presentation.SkinEngine.XamlParser;
using Presentation.SkinEngine.XamlParser.Interfaces;
using MediaPortal.Utilities.DeepCopy;
using Presentation.SkinEngine.MpfElements;

namespace Presentation.SkinEngine.Controls.Visuals
{
  /// <summary>
  /// Defines a container for UI elements which are used as template controls
  /// for all types of UI-templates. Special template types
  /// like <see cref="Styles.ControlTemplate"/> or <see cref="DataTemplate"/> are derived
  /// from this class. This class basically has no other job than holding those
  /// UI elements and cloning them when the template should be applied
  /// (method <see cref="LoadContent()"/>).
  /// </summary>
  /// <remarks>
  /// Templated controls such as <see cref="Button">Buttons</see> or
  /// <see cref="ListView">ListViews</see> implement several properties holding
  /// instances of <see cref="FrameworkTemplate"/>, for each templated feature.
  /// </remarks>
  public class FrameworkTemplate: DependencyObject, INameScope, IAddChild<UIElement>, IDeepCopyable
  {
    #region Private fields

    ResourceDictionary _resourceDictionary;
    UIElement _templateElement;
    protected IDictionary<string, object> _names = new Dictionary<string, object>();
    protected INameScope _parent = null;

    #endregion

    #region Ctor

    public FrameworkTemplate()
    {
      Init();
    }

    void Init()
    {
      _resourceDictionary = new ResourceDictionary();
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      FrameworkTemplate ft = source as FrameworkTemplate;
      _templateElement = copyManager.GetCopy(ft._templateElement);
      _resourceDictionary = copyManager.GetCopy(ft._resourceDictionary);
      _parent = copyManager.GetCopy(ft._parent);
      foreach (KeyValuePair<string, object> kvp in ft._names)
        _names.Add(copyManager.GetCopy(kvp.Key), copyManager.GetCopy(kvp.Value));
    }

    #endregion

    #region Public properties

    public ResourceDictionary Resources
    {
      get { return _resourceDictionary; }
    }

    #endregion

    #region Public methods

    public UIElement LoadContent()
    {
      UIElement element = MpfCopyManager.DeepCopyCutLP(_templateElement);
      element.IsTemplateRoot = true;
      return element;
    }

    #endregion

    #region IAddChild implementation

    public void AddChild(UIElement o)
    {
      _templateElement = o;
      _templateElement.Resources.Merge(Resources);
    }

    #endregion

    #region INamescope implementation

    public object FindName(string name)
    {
      if (_names.ContainsKey(name))
        return _names[name];
      else if (_parent != null)
        return _parent.FindName(name);
      else
        return null;
    }

    public void RegisterName(string name, object instance)
    {
      _names.Add(name, instance);
    }

    public void UnregisterName(string name)
    {
      _names.Remove(name);
    }

    public void RegisterParent(INameScope parent)
    {
      _parent = parent;
    }

    #endregion
  }
}
