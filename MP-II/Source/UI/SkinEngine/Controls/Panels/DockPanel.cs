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
using System.Collections.Generic;
using System.Drawing;
using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.Controls.Visuals;
using MediaPortal.UI.SkinEngine.MpfElements;
using MediaPortal.Utilities.DeepCopy;

namespace MediaPortal.UI.SkinEngine.Controls.Panels
{
  public enum Dock { Left, Right, Top, Bottom, Center };

  public class DockPanel : Panel
  {
    protected const string DOCK_ATTACHED_PROPERTY = "DockPanel.Dock";

    protected AbstractProperty _lastChildFillProperty;

    #region Ctor

    public DockPanel()
    {
      Init();
    }

    protected void Init()
    {
      _lastChildFillProperty = new SProperty(typeof(bool), true);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      DockPanel p = (DockPanel) source;
      LastChildFill = p.LastChildFill;
    }

    #endregion

    public AbstractProperty LastChildFillProperty
    {
      get { return _lastChildFillProperty; }
    }
    
    public bool LastChildFill
    {
      get { return (bool) _lastChildFillProperty.GetValue(); }
      set { _lastChildFillProperty.SetValue(value); }
    }

    protected override SizeF CalculateDesiredSize(SizeF totalSize)
    {
      return CalculateDesiredSize(Children.GetEnumerator(), totalSize);
    }

    protected override void ArrangeOverride()
    {
      base.ArrangeOverride();
      float offsetTop = 0.0f;
      float offsetLeft = 0.0f;
      float offsetRight = 0.0f;
      float offsetBottom = 0.0f;
      SizeF availableSize = new SizeF(_innerRect.Width, _innerRect.Height);

      int count = 0;
      // Area allocated to child
      SizeF childArea;

      foreach (FrameworkElement child in Children)
      {
        count++;
        //Trace.WriteLine(String.Format("DockPanel:arrange {0} {1}", count, child.Name));

        if (!child.IsVisible)
          continue;

        // Size of the child
        SizeF childSize = child.DesiredSize;

        if (GetDock(child) == Dock.Top)
        {
          PointF location = new PointF(offsetLeft, offsetTop);
          location.X += ActualPosition.X;
          location.Y += ActualPosition.Y;

          // Allocate area to child
          if (count == Children.Count && LastChildFill)
            childArea = new SizeF(availableSize.Width, availableSize.Height);
          else
            childArea = new SizeF(availableSize.Width, childSize.Height);

          // Position the child within the child area
          ArrangeChildHorizontal(child, ref location, ref childArea);
          child.Arrange(new RectangleF(location, childArea));

          offsetTop += childArea.Height;
          availableSize.Height -= childArea.Height;
        }
        else if (GetDock(child) == Dock.Bottom)
        {
          PointF location;
          if (count == Children.Count && LastChildFill)
            location = new PointF(offsetLeft, _innerRect.Height - (offsetBottom + availableSize.Height));
          else
            location = new PointF(offsetLeft, _innerRect.Height - (offsetBottom + childSize.Height));

          location.X += ActualPosition.X;
          location.Y += ActualPosition.Y;

          // Allocate area to child
          if (count == Children.Count && LastChildFill)
            childArea = new SizeF(availableSize.Width, availableSize.Height);
          else
            childArea = new SizeF(availableSize.Width, childSize.Height);

          // Position the child within the child area
          ArrangeChildHorizontal(child, ref location, ref childArea);
          child.Arrange(new RectangleF(location, childArea));

          offsetBottom += childArea.Height;
          availableSize.Height -= childArea.Height;
        }
        else if (GetDock(child) == Dock.Left)
        {
          PointF location = new PointF(offsetLeft, offsetTop);
          location.X += ActualPosition.X;
          location.Y += ActualPosition.Y;

          // Allocate area to child
          if (count == Children.Count && LastChildFill)
            childArea = new SizeF(availableSize.Width, availableSize.Height);
          else
            childArea = new SizeF(childSize.Width, availableSize.Height);

          // Position the child within the child area
          ArrangeChildVertical(child, ref location, ref childArea);
          child.Arrange(new RectangleF(location, childArea));

          offsetLeft += childArea.Width;
          availableSize.Width -= childArea.Width;
        }
        else if (GetDock(child) == Dock.Right)
        {
          PointF location;
          if (count == Children.Count && LastChildFill)
            location = new PointF(_innerRect.Width - (offsetRight + availableSize.Width), offsetTop);
          else
            location = new PointF(_innerRect.Width - (offsetRight + childSize.Width), offsetTop);
          location.X += ActualPosition.X;
          location.Y += ActualPosition.Y;

          // Allocate area to child
          if (count == Children.Count && LastChildFill)
            childArea = new SizeF(availableSize.Width,availableSize.Height);
          else
            childArea = new SizeF(childSize.Width,availableSize.Height);

          // Position the child within the child area
          ArrangeChildVertical(child, ref location, ref childArea);
          child.Arrange(new RectangleF(location, childArea));

          offsetRight += childArea.Width;
          availableSize.Width -= childArea.Width;
        }
        else // Center
        {
          PointF location = new PointF(offsetLeft, offsetTop);
          location.X += ActualPosition.X;
          location.Y += ActualPosition.Y;
          childSize = new SizeF(availableSize.Width, availableSize.Height);
          if (count == Children.Count && LastChildFill)
            child.Arrange(new RectangleF(location, childSize));
          else
          {
            ArrangeChild(child, ref location, ref childSize);
            child.Arrange(new RectangleF(location, childSize));
          }

          // Do not remove child size from a border offset or from size - the child will
          // stay in the "empty space" without taking place from the border layouting variables
        }
      }
    }

    protected static SizeF CalculateDesiredSize(IEnumerator<UIElement> currentChildEnumerator,
        SizeF currentAvailableSize)
    {
      if (!currentChildEnumerator.MoveNext())
        return new SizeF(0, 0);
      // Skip invisible children
      while (!currentChildEnumerator.Current.IsVisible)
        if (!currentChildEnumerator.MoveNext())
          return new SizeF(0, 0);

      UIElement child = currentChildEnumerator.Current;
      SizeF childSize = new SizeF(currentAvailableSize.Width, currentAvailableSize.Height);
      SizeF nextChildrenDesiredSize;

      if (GetDock(child) == Dock.Top || GetDock(child) == Dock.Bottom)
      {
        child.Measure(ref childSize);
        currentAvailableSize.Height -= childSize.Height;
        nextChildrenDesiredSize = CalculateDesiredSize(currentChildEnumerator, currentAvailableSize);
        return new SizeF(Math.Max(childSize.Width, nextChildrenDesiredSize.Width),
            childSize.Height + nextChildrenDesiredSize.Height);
      }
      if (GetDock(child) == Dock.Left || GetDock(child) == Dock.Right)
      {
        child.Measure(ref childSize);
        currentAvailableSize.Width -= childSize.Width;
        nextChildrenDesiredSize = CalculateDesiredSize(currentChildEnumerator, currentAvailableSize);
        return new SizeF(childSize.Width + nextChildrenDesiredSize.Width,
            Math.Max(childSize.Height, nextChildrenDesiredSize.Height));
      }
      // Else assume center
      child.Measure(ref childSize);
      nextChildrenDesiredSize = CalculateDesiredSize(currentChildEnumerator, currentAvailableSize);
      return new SizeF(Math.Max(childSize.Width, nextChildrenDesiredSize.Width),
          Math.Max(childSize.Height, nextChildrenDesiredSize.Height));
    }

    #region Attached properties

    /// <summary>
    /// Getter method for the attached property <c>Dock</c>.
    /// </summary>
    /// <param name="targetObject">The object whose property value will
    /// be returned.</param>
    /// <returns>Value of the <c>Dock</c> property on the
    /// <paramref name="targetObject"/>.</returns>
    public static Dock GetDock(DependencyObject targetObject)
    {
      return targetObject.GetAttachedPropertyValue<Dock>(DOCK_ATTACHED_PROPERTY, Dock.Left);
    }

    /// <summary>
    /// Setter method for the attached property <c>Dock</c>.
    /// </summary>
    /// <param name="targetObject">The object whose property value will
    /// be set.</param>
    /// <param name="value">Value of the <c>Dock</c> property on the
    /// <paramref name="targetObject"/> to be set.</returns>
    public static void SetDock(DependencyObject targetObject, Dock value)
    {
      targetObject.SetAttachedPropertyValue<Dock>(DOCK_ATTACHED_PROPERTY, value);
    }

    /// <summary>
    /// Returns the <c>Dock</c> attached property for the
    /// <paramref name="targetObject"/>. When this method is called,
    /// the property will be created if it is not yet attached to the
    /// <paramref name="targetObject"/>.
    /// </summary>
    /// <param name="targetObject">The object whose attached
    /// property should be returned.</param>
    /// <returns>Attached <c>Dock</c> property.</returns>
    public static AbstractProperty GetDockAttachedProperty(DependencyObject targetObject)
    {
      return targetObject.GetOrCreateAttachedProperty<Dock>(DOCK_ATTACHED_PROPERTY, Dock.Left);
    }

    #endregion
  }
}
