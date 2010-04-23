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

using System.Collections.Generic;
using System.Drawing;
using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.Controls.Visuals.Templates;
using MediaPortal.UI.SkinEngine.Xaml;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.UI.SkinEngine.SkinManagement;

namespace MediaPortal.UI.SkinEngine.Controls.Visuals
{
  public class ContentPresenter : FrameworkElement
  {
    #region Protected fields

    protected AbstractProperty _contentProperty;
    protected AbstractProperty _contentTemplateProperty;
    protected FrameworkElement _templateControl = null;

    #endregion

    #region Ctor

    public ContentPresenter()
    {
      Init();
      Attach();
    }

    void Init()
    {
      _contentProperty = new SProperty(typeof(object), null);
      _contentTemplateProperty = new SProperty(typeof(DataTemplate), null);
    }

    void Attach()
    {
      _contentProperty.Attach(OnContentChanged);
      _contentTemplateProperty.Attach(OnContentTemplateChanged);
    }

    void Detach()
    {
      _contentProperty.Detach(OnContentChanged);
      _contentTemplateProperty.Detach(OnContentTemplateChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      ContentPresenter p = (ContentPresenter) source;
      Content = copyManager.GetCopy(p.Content);
      ContentTemplate = copyManager.GetCopy(p.ContentTemplate);
      Attach();
      OnContentTemplateChanged(_contentTemplateProperty, false);
    }

    #endregion

    void OnContentChanged(AbstractProperty property, object oldValue)
    {
      if (_templateControl == null)
        // No ContentTemplate set
        FindAutomaticContentDataTemplate();
      if (_templateControl != null)
        // The controls in the DataTemplate access their "data" via their data context, so we must assign it
        _templateControl.Context = Content;
    }

    /// <summary>
    /// Does an automatic search for an approppriate data template for our content, i.e. looks
    /// in our resources for a resource with the Content's type as key.
    /// </summary>
    void FindAutomaticContentDataTemplate()
    {
      if (Content == null)
        return;
      DataTemplate dt = FindResource(Content.GetType()) as DataTemplate;
      if (dt != null)
      {
        FinishBindingsDlgt finishDlgt;
        SetTemplateControl(dt.LoadContent(out finishDlgt) as FrameworkElement);
        finishDlgt.Invoke();
        return;
      }
      object templateControl;
      if (TypeConverter.Convert(Content, typeof(FrameworkElement), out templateControl))
      {
        SetTemplateControl((FrameworkElement) templateControl);
        return;
      }
    }

    void SetTemplateControl(FrameworkElement templateControl)
    {
      if (templateControl == null)
        return;
      _templateControl = templateControl;
      _templateControl.Context = Content;
      _templateControl.VisualParent = this;
      _templateControl.SetScreen(Screen);
    }

    void OnContentTemplateChanged(AbstractProperty property, object oldValue)
    {
      if (ContentTemplate == null)
      {
        FindAutomaticContentDataTemplate();
        return;
      }
      FinishBindingsDlgt finishDlgt;
      SetTemplateControl(ContentTemplate.LoadContent(out finishDlgt) as FrameworkElement);
      finishDlgt.Invoke();
    }

    public FrameworkElement TemplateControl
    {
      get { return _templateControl; }
    }

    public AbstractProperty ContentProperty
    {
      get { return _contentProperty; }
    }

    public object Content
    {
      get { return _contentProperty.GetValue(); }
      set { _contentProperty.SetValue(value); }
    }

    public AbstractProperty ContentTemplateProperty
    {
      get { return _contentTemplateProperty; }
    }

    public DataTemplate ContentTemplate
    {
      get { return _contentTemplateProperty.GetValue() as DataTemplate; }
      set { _contentTemplateProperty.SetValue(value); }
    }

    protected override SizeF CalculateDesiredSize(SizeF totalSize)
    {
      if (_templateControl == null)
        return new SizeF();
      // Measure the child
      _templateControl.Measure(ref totalSize);
      return totalSize;
    }

    protected override void ArrangeOverride(RectangleF finalRect)
    {
      base.ArrangeOverride(finalRect);
      if (_templateControl == null)
        return;
      PointF position = new PointF(finalRect.X, finalRect.Y);
      SizeF availableSize = new SizeF(finalRect.Width, finalRect.Height);
      ArrangeChild(_templateControl, ref position, ref availableSize);
      RectangleF childRect = new RectangleF(position, availableSize);
      _templateControl.Arrange(childRect);
    }

    public override void DoRender()
    {
      base.DoRender();
      if (_templateControl != null)
      {
        SkinContext.AddOpacity(Opacity);
        _templateControl.Render();
        SkinContext.RemoveOpacity();
      }
    }

    public override void DoBuildRenderTree()
    {
      if (!IsVisible) return;
      if (_templateControl != null)
        _templateControl.BuildRenderTree();
    }

    public override void DestroyRenderTree()
    {
      if (_templateControl != null)
        _templateControl.DestroyRenderTree();
    }

    public override void AddChildren(ICollection<UIElement> childrenOut)
    {
      base.AddChildren(childrenOut);
      if (_templateControl != null)
        childrenOut.Add(_templateControl);
    }
  }
}
