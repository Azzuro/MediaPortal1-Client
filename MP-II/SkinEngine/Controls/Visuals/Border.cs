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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using MediaPortal.Control.InputManager;
using MediaPortal.Presentation.DataObjects;
using MediaPortal.SkinEngine.Controls.Brushes;
using MediaPortal.Utilities;
using SlimDX;
using SlimDX.Direct3D9;
using MediaPortal.SkinEngine;
using MediaPortal.SkinEngine.Rendering;
using MediaPortal.SkinEngine.Controls.Visuals;
using MediaPortal.SkinEngine.Controls.Visuals.Shapes;
using MediaPortal.SkinEngine.DirectX;
using RectangleF = System.Drawing.RectangleF;
using PointF = System.Drawing.PointF;
using SizeF = System.Drawing.SizeF;
using MediaPortal.SkinEngine.Xaml.Interfaces;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.SkinEngine.SkinManagement;

// changes possible:
// - opacity
// - vertices
// - effect / effect parameters
// - rendertransform
// - visibility

namespace MediaPortal.SkinEngine.Controls.Visuals
{

  public class Border : Shape, IAddChild<FrameworkElement>, IUpdateEventHandler
  {
    #region Private fields

    Property _backgroundProperty;
    Property _borderProperty;
    Property _borderThicknessProperty;
    Property _cornerRadiusProperty;
    FrameworkElement _content;
    VisualAssetContext _backgroundAsset;
    VisualAssetContext _borderAsset;
    PrimitiveContext _backgroundContext;
    PrimitiveContext _borderContext;
    UIEvent _lastEvent = UIEvent.None;

    #endregion

    #region Ctor

    public Border()
    {
      Init();
      Attach();
    }

    void Init()
    {
      _borderProperty = new Property(typeof(Brush), null);
      _backgroundProperty = new Property(typeof(Brush), null);
      _borderThicknessProperty = new Property(typeof(double), 1.0);
      _cornerRadiusProperty = new Property(typeof(double), 0.0);
    }

    void Attach()
    {
      _borderProperty.Attach(OnBorderBrushChanged);
      _backgroundProperty.Attach(OnBackgroundBrushChanged);
      _borderThicknessProperty.Attach(OnLayoutPropertyChanged);
      _cornerRadiusProperty.Attach(OnLayoutPropertyChanged);
    }

    void Detach()
    {
      _borderProperty.Detach(OnBorderBrushChanged);
      _backgroundProperty.Detach(OnBackgroundBrushChanged);
      _borderThicknessProperty.Detach(OnLayoutPropertyChanged);
      _cornerRadiusProperty.Detach(OnLayoutPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      Border b = source as Border;
      BorderBrush = copyManager.GetCopy(b.BorderBrush);
      Background = copyManager.GetCopy(b.Background);
      BorderThickness = copyManager.GetCopy(b.BorderThickness);
      CornerRadius = copyManager.GetCopy(b.CornerRadius);
      Attach();
    }

    #endregion

    #region Change event handlers

    void OnBackgroundBrushChanged(Property property)
    {
      Brush brush = property.GetValue() as Brush;
      if (brush != null)
      {
        brush.ClearAttachedEvents();
        brush.Attach(OnBackgroundBrushPropertyChanged);
      }
    }

    void OnBorderBrushChanged(Property property)
    {
      Brush brush = property.GetValue() as Brush;
      if (brush != null)
      {
        brush.ClearAttachedEvents();
        brush.Attach(OnBorderBrushPropertyChanged);
      }
    }

    void OnBackgroundBrushPropertyChanged(Property property)
    {
      _lastEvent |= UIEvent.FillChange;
      if (Screen != null) Screen.Invalidate(this);
    }

    void OnBorderBrushPropertyChanged(Property property)
    {
      _lastEvent |= UIEvent.StrokeChange;
      if (Screen != null) Screen.Invalidate(this);
    }

    void OnLayoutPropertyChanged(Property property)
    {
      _performLayout = true;
      if (Screen != null) Screen.Invalidate(this);
    }

    #endregion

    #region Properties

    public Property BackgroundProperty
    {
      get { return _backgroundProperty; }
    }

    public Brush Background
    {
      get { return _backgroundProperty.GetValue() as Brush; }
      set { _backgroundProperty.SetValue(value); }
    }

    public Property BorderBrushProperty
    {
      get { return _borderProperty; }
      set { _borderProperty = value; }
    }

    public Brush BorderBrush
    {
      get { return _borderProperty.GetValue() as Brush; }
      set { _borderProperty.SetValue(value); }
    }

    public Property BorderThicknessProperty
    {
      get { return _borderThicknessProperty; }
    }

    public double BorderThickness
    {
      get { return (double)_borderThicknessProperty.GetValue(); }
      set { _borderThicknessProperty.SetValue(value); }
    }

    public Property CornerRadiusProperty
    {
      get { return _cornerRadiusProperty; }
    }

    public double CornerRadius
    {
      get { return (double)_cornerRadiusProperty.GetValue(); }
      set { _cornerRadiusProperty.SetValue(value); }
    }

    #endregion

    #region Measure&arrange

    public override void Arrange(System.Drawing.RectangleF finalRect)
    {
      System.Drawing.RectangleF layoutRect = new System.Drawing.RectangleF(finalRect.X, finalRect.Y, finalRect.Width, finalRect.Height);

      ComputeInnerRectangle(ref finalRect);

      _finalRect = new RectangleF(finalRect.Location, finalRect.Size);

      ActualPosition = new Vector3(finalRect.Location.X, finalRect.Location.Y, SkinContext.GetZorder());
      ActualWidth = finalRect.Width;
      ActualHeight = finalRect.Height;

      _finalLayoutTransform = SkinContext.FinalLayoutTransform;

      Initialize();
      InitializeTriggers();
      IsInvalidLayout = false;

      if (!finalRect.IsEmpty)
      {
        if (_finalRect.Width != finalRect.Width || _finalRect.Height != _finalRect.Height)
          _performLayout = true;
        _finalRect = new System.Drawing.RectangleF(finalRect.Location, finalRect.Size);
      }
      if (_content != null)
      {
        PointF location = new PointF(layoutRect.Location.X, layoutRect.Location.Y);
        ArrangeContent(_content, ref location, layoutRect.Size);
        _content.Arrange(new RectangleF(location, _content.DesiredSize));
      }
    }

    protected virtual void ArrangeContent(FrameworkElement child, ref System.Drawing.PointF p, SizeF s)
    {
      if (VisualParent == null) return;

      if (child.HorizontalAlignment == HorizontalAlignmentEnum.Center)
      {
        if (s.Width > 0)
          p.X += ((s.Width - child.DesiredSize.Width) / 2);
      }
      else if (child.HorizontalAlignment == HorizontalAlignmentEnum.Right)
      {
        if (s.Width > 0)
          p.X += (s.Width - child.DesiredSize.Width);
      }
      if (child.VerticalAlignment == VerticalAlignmentEnum.Center)
      {
        if (s.Height > 0)
          p.Y += ((s.Height - child.DesiredSize.Height) / 2);
      }
      else if (child.VerticalAlignment == VerticalAlignmentEnum.Bottom)
      {
        if (s.Height > 0)
          p.Y += (s.Height - child.DesiredSize.Height);
      }
    }

    public override void Measure(ref SizeF totalSize)
    {
      SizeF childSize = new SizeF(0, 0);
      if (_content != null)
      {
        _content.Measure(ref childSize);
      }

      _desiredSize = new SizeF((float)Width * SkinContext.Zoom.Width, (float)Height * SkinContext.Zoom.Height);

      if (Double.IsNaN(Width))
        _desiredSize.Width = childSize.Width;

      if (Double.IsNaN(Height))
        _desiredSize.Height = childSize.Height;

      totalSize = _desiredSize;
      AddMargin(ref totalSize);

      //Trace.WriteLine(String.Format("border.measure :{0} returns {1}x{2}", this.Name, (int)totalSize.Width, (int)totalSize.Height));

    }

    #endregion

    #region Rendering

    void SetupBrush(UIEvent uiEvent)
    {
      if ((uiEvent & UIEvent.OpacityChange) != 0 || (uiEvent & UIEvent.FillChange) != 0)
      {
        if (Background != null && _backgroundContext != null)
        {
          RenderPipeline.Instance.Remove(_backgroundContext);
          Background.SetupPrimitive(_backgroundContext);
          RenderPipeline.Instance.Add(_backgroundContext);
        }
      }

      if ((uiEvent & UIEvent.OpacityChange) != 0 || (uiEvent & UIEvent.StrokeChange) != 0)
      {
        if (BorderBrush != null && _borderContext != null)
        {
          RenderPipeline.Instance.Remove(_borderContext);
          BorderBrush.SetupPrimitive(_borderContext);
          RenderPipeline.Instance.Add(_borderContext);
        }
      }
    }

    public new void Update()
    {
      UpdateLayout();
      if (_performLayout)
      {
        PerformLayout();
        _performLayout = false;
        _lastEvent = UIEvent.None;
      }
      else if (_lastEvent != UIEvent.None)
      {
        if ((_lastEvent & UIEvent.Hidden) != 0)
        {
          RenderPipeline.Instance.Remove(_backgroundContext);
          RenderPipeline.Instance.Remove(_borderContext);
          _backgroundContext = null;
          _borderContext = null;
          _performLayout = true;
        }
        else
        {
          SetupBrush(_lastEvent);
        }
        _lastEvent = UIEvent.None;
      }
    }

    public override void DoRender()
    {

      if (!IsVisible) return;
      if (Background != null || (BorderBrush != null && BorderThickness > 0))
      {
        if (Background != null && _backgroundAsset != null && _backgroundAsset.IsAllocated == false)
          _performLayout = true;
        if (BorderBrush != null && _borderAsset != null && _borderAsset.IsAllocated == false)
          _performLayout = true;
        if (_performLayout)
        {
          PerformLayout();
          _performLayout = false;
        }
        SkinContext.AddOpacity(this.Opacity);
        //ExtendedMatrix m = new ExtendedMatrix();
        //m.Matrix = Matrix.Translation(new Vector3((float)ActualPosition.X, (float)ActualPosition.Y, (float)ActualPosition.Z));
        //SkinContext.AddTransform(m);
        if (Background != null)
        {
          //GraphicsDevice.TransformWorld = SkinContext.FinalMatrix.Matrix;
          //GraphicsDevice.Device.VertexFormat = PositionColored2Textured.Format;
          if (Background.BeginRender(_backgroundAsset.VertexBuffer, _verticesCountFill, PrimitiveType.TriangleList))
          {
            GraphicsDevice.Device.SetStreamSource(0, _backgroundAsset.VertexBuffer, 0, PositionColored2Textured.StrideSize);
            GraphicsDevice.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, _verticesCountFill);
            Background.EndRender();
          }
          _backgroundAsset.LastTimeUsed = SkinContext.Now;
        }

        if (BorderBrush != null && BorderThickness > 0)
        {
          //GraphicsDevice.Device.VertexFormat = PositionColored2Textured.Format;
          if (BorderBrush.BeginRender(_borderAsset.VertexBuffer, _verticesCountBorder, PrimitiveType.TriangleList))
          {
            GraphicsDevice.Device.SetStreamSource(0, _borderAsset.VertexBuffer, 0, PositionColored2Textured.StrideSize);
            GraphicsDevice.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, _verticesCountBorder);
            BorderBrush.EndRender();
          }
          _borderAsset.LastTimeUsed = SkinContext.Now;
        }
        //SkinContext.RemoveTransform();
        SkinContext.RemoveOpacity();
      }

      if (_content != null)
      {
        SkinContext.AddOpacity(this.Opacity);
        _content.DoRender();
        SkinContext.RemoveOpacity();
      }
    }

    #endregion

    #region Input handling

    public override void FireUIEvent(UIEvent eventType, UIElement source)
    {
      if (_content != null)
        _content.FireUIEvent(eventType, source);
      if (SkinContext.UseBatching)
      {
        _lastEvent |= eventType;
        if (Screen != null) Screen.Invalidate(this);
      }
    }

    public override void OnKeyPressed(ref MediaPortal.Control.InputManager.Key key)
    {
      base.OnKeyPressed(ref key);
      if (_content != null)
      {
        _content.OnKeyPressed(ref key);
      }
    }

    public override void OnMouseMove(float x, float y)
    {
      base.OnMouseMove(x, y);
      if (_content != null)
      {
        _content.OnMouseMove(x, y);
      }
    }

    #endregion

    #region findXXX methods

    public override void FireEvent(string eventName)
    {
      if (_content != null)
      {
        _content.FireEvent(eventName);
      }
      base.FireEvent(eventName);
    }

    public override void AddChildren(ICollection<UIElement> childrenOut)
    {
      base.AddChildren(childrenOut);
      if (_content != null)
        childrenOut.Add(_content);
    }

    #endregion

    #region Layouting

    protected override void PerformLayout()
    {
      //Trace.WriteLine("Border.PerformLayout() " + this.Name);

      double w = ActualWidth;
      double h = ActualHeight;
      float centerX, centerY;
      SizeF rectSize = new SizeF((float)w, (float)h);

      ExtendedMatrix m = new ExtendedMatrix();
      m.Matrix *= _finalLayoutTransform.Matrix;
      if (LayoutTransform != null)
      {
        ExtendedMatrix em;
        LayoutTransform.GetTransform(out em);
        m.Matrix *= em.Matrix;
      }
      m.InvertSize(ref rectSize);
      System.Drawing.RectangleF rect = new System.Drawing.RectangleF(-0.5f, -0.5f, rectSize.Width + 0.5f, rectSize.Height + 0.5f);
      rect.X += (float)ActualPosition.X;
      rect.Y += (float)ActualPosition.Y;
      PositionColored2Textured[] verts;
      GraphicsPath path;
      if (Background != null || (BorderBrush != null && BorderThickness > 0))
      {
        using (path = GetRoundedRect(rect, (float)CornerRadius))
        {
          CalcCentroid(path, out centerX, out centerY);
          if (Background != null)
          {
            if (SkinContext.UseBatching == false)
            {
              if (_backgroundAsset == null)
              {
                _backgroundAsset = new VisualAssetContext("Border._backgroundAsset:" + this.Name);
                ContentManager.Add(_backgroundAsset);
              }
              _backgroundAsset.VertexBuffer = ConvertPathToTriangleFan(path, centerX, centerY, out verts);
              if (_backgroundAsset.VertexBuffer != null)
              {
                Background.SetupBrush(this, ref verts);


                PositionColored2Textured.Set(_backgroundAsset.VertexBuffer, ref verts);
                _verticesCountFill = (verts.Length / 3);

              }
            }
            else
            {
              Shape.PathToTriangleList(path, centerX, centerY, out verts);
              _verticesCountFill = (verts.Length / 3);
              Background.SetupBrush(this, ref verts);
              if (_backgroundContext == null)
              {
                _backgroundContext = new PrimitiveContext(_verticesCountFill, ref verts);
                Background.SetupPrimitive(_backgroundContext);
                RenderPipeline.Instance.Add(_backgroundContext);
              }
              else
              {
                _backgroundContext.OnVerticesChanged(_verticesCountFill, ref verts);
              }
            }
          }

          if (BorderBrush != null && BorderThickness > 0)
          {
            if (SkinContext.UseBatching == false)
            {
              if (_borderAsset == null)
              {
                _borderAsset = new VisualAssetContext("Border._borderAsset:" + this.Name);
                ContentManager.Add(_borderAsset);
              }
              _borderAsset.VertexBuffer = ConvertPathToTriangleStrip(path, (float)BorderThickness, true, out verts, _finalLayoutTransform, false);
              if (_borderAsset.VertexBuffer != null)
              {
                BorderBrush.SetupBrush(this, ref verts);

                PositionColored2Textured.Set(_borderAsset.VertexBuffer, ref verts);
                _verticesCountBorder = (verts.Length / 3);

              }
            }
            else
            {
              Shape.PathToTriangleStrip(path, (float)BorderThickness, true, out verts, _finalLayoutTransform);
              BorderBrush.SetupBrush(this, ref verts);
              _verticesCountBorder = (verts.Length / 3);
              if (_borderContext == null)
              {
                _borderContext = new PrimitiveContext(_verticesCountBorder, ref verts);
                BorderBrush.SetupPrimitive(_borderContext);
                RenderPipeline.Instance.Add(_borderContext);
              }
              else
              {
                _borderContext.OnVerticesChanged(_verticesCountBorder, ref verts);
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Get the desired Rounded Rectangle path.
    /// </summary>
    private GraphicsPath GetRoundedRect(RectangleF baseRect, float CornerRadius)
    {
      // if corner radius is less than or equal to zero, 

      // return the original rectangle 

      if (CornerRadius <= 0.0f && CornerRadius <= 0.0f)
      {
        GraphicsPath mPath = new GraphicsPath();
        mPath.AddRectangle(baseRect);
        mPath.CloseFigure();
        System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
        m.Translate(-baseRect.X, -baseRect.Y, MatrixOrder.Append);
        m.Multiply(_finalLayoutTransform.Get2dMatrix(), MatrixOrder.Append);
        if (LayoutTransform != null)
        {
          ExtendedMatrix em;
          LayoutTransform.GetTransform(out em);
          m.Multiply(em.Get2dMatrix(), MatrixOrder.Append);
        }
        m.Translate(baseRect.X, baseRect.Y, MatrixOrder.Append);
        mPath.Transform(m);
        mPath.Flatten();
        return mPath;
      }

      // if the corner radius is greater than or equal to 

      // half the width, or height (whichever is shorter) 

      // then return a capsule instead of a lozenge 

      if (CornerRadius >= (Math.Min(baseRect.Width, baseRect.Height)) / 2.0)
        return GetCapsule(baseRect);

      // create the arc for the rectangle sides and declare 

      // a graphics path object for the drawing 

      float diameter = CornerRadius * 2.0F;
      SizeF sizeF = new SizeF(diameter, diameter);
      RectangleF arc = new RectangleF(baseRect.Location, sizeF);
      GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

      // top left arc 


      path.AddArc(arc, 180, 90);

      // top right arc 

      arc.X = baseRect.Right - diameter;
      path.AddArc(arc, 270, 90);

      // bottom right arc 

      arc.Y = baseRect.Bottom - diameter;
      path.AddArc(arc, 0, 90);

      // bottom left arc

      arc.X = baseRect.Left;
      path.AddArc(arc, 90, 90);

      path.CloseFigure();
      System.Drawing.Drawing2D.Matrix mtx = new System.Drawing.Drawing2D.Matrix();
      mtx.Translate(-baseRect.X, -baseRect.Y, MatrixOrder.Append);
      mtx.Multiply(_finalLayoutTransform.Get2dMatrix(), MatrixOrder.Append);
      if (LayoutTransform != null)
      {
        ExtendedMatrix em;
        LayoutTransform.GetTransform(out em);
        mtx.Multiply(em.Get2dMatrix(), MatrixOrder.Append);
      }
      mtx.Translate(baseRect.X, baseRect.Y, MatrixOrder.Append);
      path.Transform(mtx);

      path.Flatten();
      return path;
    }

    /// <summary>
    /// Gets the desired Capsular path.
    /// </summary>
    private GraphicsPath GetCapsule(RectangleF baseRect)
    {
      float diameter;
      RectangleF arc;
      GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
      try
      {
        if (baseRect.Width > baseRect.Height)
        {
          // return horizontal capsule 

          diameter = baseRect.Height;
          SizeF sizeF = new SizeF(diameter, diameter);
          arc = new RectangleF(baseRect.Location, sizeF);
          path.AddArc(arc, 90, 180);
          arc.X = baseRect.Right - diameter;
          path.AddArc(arc, 270, 180);
        }
        else if (baseRect.Width < baseRect.Height)
        {
          // return vertical capsule 

          diameter = baseRect.Width;
          SizeF sizeF = new SizeF(diameter, diameter);
          arc = new RectangleF(baseRect.Location, sizeF);
          path.AddArc(arc, 180, 180);
          arc.Y = baseRect.Bottom - diameter;
          path.AddArc(arc, 0, 180);
        }
        else
        {
          // return circle 

          path.AddEllipse(baseRect);
        }
      }
      catch (Exception)
      {
        path.AddEllipse(baseRect);
      }
      finally
      {
        path.CloseFigure();
      }
      System.Drawing.Drawing2D.Matrix mtx = new System.Drawing.Drawing2D.Matrix();
      mtx.Translate(-baseRect.X, -baseRect.Y, MatrixOrder.Append);
      mtx.Multiply(_finalLayoutTransform.Get2dMatrix(), MatrixOrder.Append);
      if (LayoutTransform != null)
      {
        ExtendedMatrix em;
        LayoutTransform.GetTransform(out em);
        mtx.Multiply(em.Get2dMatrix(), MatrixOrder.Append);
      }
      mtx.Translate(baseRect.X, baseRect.Y, MatrixOrder.Append);
      path.Transform(mtx);
      return path;
    }

    #endregion

    #region IAddChild Members

    public void AddChild(FrameworkElement o)
    {
      _content = o;
      _content.VisualParent = this;
    }

    #endregion

    public override void Deallocate()
    {
      base.Deallocate();
      if (BorderBrush != null)
        this.BorderBrush.Deallocate();
      if (Background != null)
        this.Background.Deallocate();
      if (_content != null)
      {
        _content.Deallocate();
      }
      if (_borderAsset != null)
      {
        _borderAsset.Free(true);
        ContentManager.Remove(_borderAsset);
        _borderAsset = null;
      }
      if (_backgroundAsset != null)
      {
        _backgroundAsset.Free(true);
        ContentManager.Remove(_backgroundAsset);
        _backgroundAsset = null;
      }
      _performLayout = true;
      if (_backgroundContext != null)
      {
        RenderPipeline.Instance.Remove(_backgroundContext);
        _backgroundContext = null;
      }
      if (_borderContext != null)
      {
        RenderPipeline.Instance.Remove(_borderContext);
        _borderContext = null;
      }
    }

    public override void Allocate()
    {
      base.Allocate();
      if (BorderBrush != null)
        this.BorderBrush.Allocate();
      if (Background != null)
        this.Background.Allocate();
      if (_content != null)
      {
        _content.Allocate();
      }
    }

    public override void DoBuildRenderTree()
    {
      if (!IsVisible) return;
      PerformLayout();
      _performLayout = false;
      _lastEvent = UIEvent.None;
      if (_content != null)
      {
        _content.BuildRenderTree();
      }
    }

    public override void DestroyRenderTree()
    {
      if (_backgroundContext != null)
      {
        RenderPipeline.Instance.Remove(_backgroundContext);
        _backgroundContext = null;
      }
      if (_borderContext != null)
      {
        RenderPipeline.Instance.Remove(_borderContext);
        _borderContext = null;
      }
      if (_content != null)
      {
        _content.DestroyRenderTree();
      }
    }
  }
}
