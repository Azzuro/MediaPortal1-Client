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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Core.Properties;
using MediaPortal.Core.InputManager;
using SkinEngine;
using SkinEngine.DirectX;
using SkinEngine.Controls.Brushes;
using SkinEngine.Rendering;

using RectangleF = System.Drawing.RectangleF;
using PointF = System.Drawing.PointF;
using SizeF = System.Drawing.SizeF;
using Matrix = SlimDX.Matrix;
using Brush = SkinEngine.Controls.Brushes.Brush;

using SlimDX;
using SlimDX.Direct3D;
using SlimDX.Direct3D9;
using GeometryUtility;
namespace SkinEngine.Controls.Visuals
{
  /// <summary>
  /// Describes to a LineStrip how it should place the line's width relative to its points
  /// </summary>
  /// <remarks>
  /// The behavior of the LeftHanded and RightHanded modes depends on the order the points are
  /// listed in. LeftHanded will draw the line on the outside of a clockwise curve and on the
  /// inside of a counterclockwise curve; RightHanded is the opposite.
  /// </remarks>
  public enum WidthMode
  {
    /// <summary>
    /// Centers the width on the line
    /// </summary>
    Centered,
    /// <summary>
    /// Places the width on the left-hand side of the line
    /// </summary>
    LeftHanded,
    /// <summary>
    /// Places the width on the right-hand side of the line
    /// </summary>
    RightHanded
  }
  public class Shape : FrameworkElement, IUpdateEventHandler
  {
    Property _stretchProperty;
    Brush _fillProperty;
    Brush _strokeProperty;
    Property _strokeThicknessProperty;
    protected VisualAssetContext _fillAsset;
    protected int _verticesCountFill;
    protected VisualAssetContext _borderAsset;
    protected int _verticesCountBorder;
    protected bool _performLayout;
    protected PrimitiveContext _fillContext;
    protected PrimitiveContext _strokeContext;
    protected UIEvent _lastEvent;

    public Shape()
    {
      Init();
    }

    public Shape(Shape s)
      : base(s)
    {
      Init();
      if (s.Fill != null)
        Fill = (Brush)s.Fill.Clone();
      if (s.Stroke != null)
        Stroke = (Brush)s.Stroke.Clone();
      StrokeThickness = s.StrokeThickness;
      Stretch = s.Stretch;
    }

    public override object Clone()
    {
      return new Shape(this);
    }

    void Init()
    {
      _fillProperty = null;
      _strokeProperty = null;
      _strokeThicknessProperty = new Property(1.0);
      _stretchProperty = new Property(Stretch.None);
      _strokeThicknessProperty.Attach(new PropertyChangedHandler(OnStrokeThicknessChanged));
      // _strokeProperty.Attach(new PropertyChangedHandler(OnStrokeThicknessChanged));
    }

    void OnStrokeThicknessChanged(Property property)
    {
      _performLayout = true;
      if (Window!=null) Window.Invalidate(this);
    }
    void OnFillBrushPropertyChanged(Property property)
    {
      _lastEvent |= UIEvent.FillChange;
      if (Window!=null) Window.Invalidate(this);
    }
    void OnStrokeBrushPropertyChanged(Property property)
    {
      _lastEvent |= UIEvent.StrokeChange;
      if (Window!=null) Window.Invalidate(this);
    }


    /// <summary>
    /// Gets or sets the stretch property.
    /// </summary>
    /// <value>The stretch property.</value>
    public Property StretchProperty
    {
      get
      {
        return _stretchProperty;
      }
      set
      {
        _stretchProperty = value;
      }
    }

    /// <summary>
    /// Gets or sets the stretch.
    /// </summary>
    /// <value>The stretch.</value>
    public Stretch Stretch
    {
      get
      {
        return (Stretch)_stretchProperty.GetValue();
      }
      set
      {
        _stretchProperty.SetValue(value);
      }
    }


    /// <summary>
    /// Gets or sets the fill.
    /// </summary>
    /// <value>The fill.</value>
    public Brush Fill
    {
      get
      {
        return _fillProperty;
      }
      set
      {
        _fillProperty = value;
        if (value != null)
        {
          _fillProperty.Attach(new PropertyChangedHandler(OnFillBrushPropertyChanged));
        }
      }
    }


    /// <summary>
    /// Gets or sets the stroke.
    /// </summary>
    /// <value>The stroke.</value>
    public Brush Stroke
    {
      get
      {
        return _strokeProperty;
      }
      set
      {
        _strokeProperty = value;
        if (value != null)
        {
          _strokeProperty.Attach(new PropertyChangedHandler(OnStrokeBrushPropertyChanged));
        }
      }
    }


    /// <summary>
    /// Gets or sets the stroke thickness property.
    /// </summary>
    /// <value>The stroke thickness property.</value>
    public Property StrokeThicknessProperty
    {
      get
      {
        return _strokeThicknessProperty;
      }
      set
      {
        _strokeThicknessProperty = value;
      }
    }

    /// <summary>
    /// Gets or sets the stroke thickness.
    /// </summary>
    /// <value>The stroke thickness.</value>
    public double StrokeThickness
    {
      get
      {
        return (double)_strokeThicknessProperty.GetValue();
      }
      set
      {
        _strokeThicknessProperty.SetValue(value);
      }
    }

    protected virtual void PerformLayout()
    {
    }
    public override void DoBuildRenderTree()
    {
      if (!IsVisible) return;
      PerformLayout();
      _performLayout = false;
      _lastEvent = UIEvent.None;
    }
    public override void DestroyRenderTree()
    {
      if (_fillContext != null)
      {
        RenderPipeline.Instance.Remove(_fillContext);
        _fillContext = null;
      }
      if (_strokeContext != null)
      {
        RenderPipeline.Instance.Remove(_strokeContext);
        _strokeContext = null;
      }
    }

    void SetupBrush(UIEvent uiEvent)
    {
      if ((uiEvent & UIEvent.FillChange) != 0 || (uiEvent & UIEvent.OpacityChange) != 0)
      {
        if (Fill != null && _fillContext != null)
        {
          RenderPipeline.Instance.Remove(_fillContext);
          Fill.SetupPrimitive(_fillContext);
          RenderPipeline.Instance.Add(_fillContext);
        }
      }
      if ((uiEvent & UIEvent.StrokeChange) != 0 || (uiEvent & UIEvent.OpacityChange) != 0)
      {
        if (Stroke != null && _strokeContext != null)
        {
          RenderPipeline.Instance.Remove(_strokeContext);
          Stroke.SetupPrimitive(_strokeContext);
          RenderPipeline.Instance.Add(_strokeContext);
        }
      }
    }

    public void Update()
    {
      UpdateLayout();
      if (_performLayout)
      {
        PerformLayout();
        _performLayout = false;
        _lastEvent = UIEvent.None;
      }
      else if (_lastEvent == UIEvent.Hidden)
      {
        if (_fillContext != null)
          RenderPipeline.Instance.Remove(_fillContext);
        if (_strokeContext != null)
          RenderPipeline.Instance.Remove(_strokeContext);
        _fillContext = null;
        _strokeContext = null;
        _performLayout = true;
      }
      else if (_lastEvent != UIEvent.None)
      {
        SetupBrush(_lastEvent);
      }
      _lastEvent = UIEvent.None;
    }

    /// <summary>
    /// Renders the visual
    /// </summary>
    public override void DoRender()
    {
      if (!IsVisible) return;
      if (Fill == null && Stroke == null) return;
      if (Fill != null)
      {
        if ((_fillAsset != null && !_fillAsset.IsAllocated) || _fillAsset == null)
          _performLayout = true;
      }
      if (Stroke != null)
      {
        if ((_borderAsset != null && !_borderAsset.IsAllocated) || _borderAsset == null)
          _performLayout = true;
      }
      if (_performLayout)
      {
        PerformLayout();
        _performLayout = false;
      }

      SkinContext.AddOpacity(this.Opacity);
      if (_fillAsset != null)
      {
        //GraphicsDevice.TransformWorld = SkinContext.FinalMatrix.Matrix;
        //GraphicsDevice.Device.VertexFormat = PositionColored2Textured.Format;
        if (Fill.BeginRender(_fillAsset.VertexBuffer, _verticesCountFill, PrimitiveType.TriangleList))
        {
          GraphicsDevice.Device.SetStreamSource(0, _fillAsset.VertexBuffer, 0, PositionColored2Textured.StrideSize);
          GraphicsDevice.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, _verticesCountFill);
          Fill.EndRender();
        }
        _fillAsset.LastTimeUsed = SkinContext.Now;
      }
      if (_borderAsset != null)
      {
        //GraphicsDevice.Device.VertexFormat = PositionColored2Textured.Format;
        if (Stroke.BeginRender(_borderAsset.VertexBuffer, _verticesCountBorder, PrimitiveType.TriangleList))
        {
          GraphicsDevice.Device.SetStreamSource(0, _borderAsset.VertexBuffer, 0, PositionColored2Textured.StrideSize);
          GraphicsDevice.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, _verticesCountBorder);
          Stroke.EndRender();
        }
        _borderAsset.LastTimeUsed = SkinContext.Now;
      }
      SkinContext.RemoveOpacity();

    }

    public override void FireUIEvent(UIEvent eventType, UIElement source)
    {

      if (SkinContext.UseBatching)
      {
        _lastEvent |= eventType;
        if (Window!=null) Window.Invalidate(this);
      }
    }
    /// <summary>
    /// Converts the graphicspath to an array of vertices using trianglist.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="cx">The cx.</param>
    /// <param name="cy">The cy.</param>
    /// <returns></returns>
    static public void PathToTriangleList(GraphicsPath path, float cx, float cy, out PositionColored2Textured[] verts)
    {
      verts = null;
      int pointCount = path.PointCount;
      if (pointCount <= 0) return;
      PointF[] pathPoints = path.PathPoints;
      if (pointCount == 3)
      {
        verts = new PositionColored2Textured[3];

        verts[0].Position = new Vector3(pathPoints[0].X, pathPoints[0].Y, 1);
        verts[1].Position = new Vector3(pathPoints[1].X, pathPoints[1].Y, 1);
        verts[2].Position = new Vector3(pathPoints[2].X, pathPoints[2].Y, 1);
        return;
      }
      else
      {
        int verticeCount = (pointCount) * 3;
        verts = new PositionColored2Textured[verticeCount];
        for (int i = 0; i < pointCount; ++i)
        {
          verts[i * 3 + 0].Position = new Vector3(cx, cy, 1);
          verts[i * 3 + 1].Position = new Vector3(pathPoints[i].X, pathPoints[i].Y, 1);
          if (i + 1 < pointCount)
            verts[i * 3 + 2].Position = new Vector3(pathPoints[i + 1].X, pathPoints[i + 1].Y, 1);
          else
            verts[i * 3 + 2].Position = new Vector3(pathPoints[0].X, pathPoints[0].Y, 1);
        }

        return;
      }
    }




    /// <summary>
    /// Converts the graphicspath to an array of vertices using trianglefan.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="cx">The cx.</param>
    /// <param name="cy">The cy.</param>
    /// <returns></returns>
    static public VertexBuffer ConvertPathToTriangleFan(GraphicsPath path, float cx, float cy, out PositionColored2Textured[] verts)
    {
      verts = null;
      int pointCount = path.PointCount;
      if (pointCount <= 0) return null;
      PointF[] pathPoints = path.PathPoints;
      if (pointCount == 3)
      {
        VertexBuffer vertexBuffer = PositionColored2Textured.Create(3);
        verts = new PositionColored2Textured[3];

        verts[0].Position = new Vector3(pathPoints[0].X, pathPoints[0].Y, 1);
        verts[1].Position = new Vector3(pathPoints[1].X, pathPoints[1].Y, 1);
        verts[2].Position = new Vector3(pathPoints[2].X, pathPoints[2].Y, 1);
        return vertexBuffer;
      }
      else
      {
        int verticeCount = (pointCount) * 3;
        VertexBuffer vertexBuffer = PositionColored2Textured.Create(verticeCount);
        verts = new PositionColored2Textured[verticeCount];
        for (int i = 0; i < pointCount; ++i)
        {
          verts[i * 3 + 0].Position = new Vector3(cx, cy, 1);
          verts[i * 3 + 1].Position = new Vector3(pathPoints[i].X, pathPoints[i].Y, 1);
          if (i + 1 < pointCount)
            verts[i * 3 + 2].Position = new Vector3(pathPoints[i + 1].X, pathPoints[i + 1].Y, 1);
          else
            verts[i * 3 + 2].Position = new Vector3(pathPoints[0].X, pathPoints[0].Y, 1);
        }

        /*
         * convert to trianglefan
        int verticeCount = pointCount + 2;

        VertexBuffer vertexBuffer = PositionColored2Textured.Create(verticeCount);
        verts = new PositionColored2Textured[verticeCount];

        verts[0].Position = new Vector3(cx, cy, 1);
        verts[1].Position = new Vector3(pathPoints[0].X, pathPoints[0].Y, 1);
        verts[2].Position = new Vector3(pathPoints[1].X, pathPoints[1].Y, 1);
        for (int i = 2; i < pointCount; ++i)
        {
          verts[i + 1].Position = new Vector3(pathPoints[i].X, pathPoints[i].Y, 1);
        }
        verts[verticeCount - 1].Position = new Vector3(pathPoints[0].X, pathPoints[0].Y, 1);*/
        return vertexBuffer;
      }
    }

    /// <summary>
    /// Gets the inset.
    /// </summary>
    /// <param name="nextpoint">The nextpoint.</param>
    /// <param name="point">The point.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="thickNess">The thick ness.</param>
    static void GetInset(PointF nextpoint, PointF point, out float x, out float y, double thickNessW, double thickNessH, PolygonDirection direction, ExtendedMatrix finalTransLayoutform)
    {
      double ang = (float)Math.Atan2((nextpoint.Y - point.Y), (nextpoint.X - point.X));  //returns in radians
      double pi2 = Math.PI / 2.0; //90gr
      if (direction == PolygonDirection.Clockwise)
        ang += pi2;
      else
        ang -= pi2;
      x = (float)(Math.Cos(ang) * thickNessW); //radians
      y = (float)(Math.Sin(ang) * thickNessH);
      if (finalTransLayoutform != null)
        finalTransLayoutform.TransformXY(ref x, ref y);
      x += point.X;
      y += point.Y;
    }

    /// <summary>
    /// Gets the next point.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    static PointF GetNextPoint(PointF[] points, int i, int max)
    {
      i++;
      while (i >= max) i -= max;
      return points[i];
    }
    static public void PathToTriangleStrip(GraphicsPath path, float thickNess, bool isClosed, out PositionColored2Textured[] verts, ExtendedMatrix finalTransLayoutform)
    {
      PolygonDirection direction = PointsDirection(path);
      PathToTriangleStrip(path, thickNess, isClosed, direction, out verts, finalTransLayoutform);
    }
    static public void PathToTriangleStrip(GraphicsPath path, float thickNess, bool isClosed, PolygonDirection direction, out PositionColored2Textured[] verts, ExtendedMatrix finalLayoutTransform)
    {
      verts = null;
      if (path.PointCount <= 0) return;
      // thickNess /= 2.0f;
      float thicknessW = thickNess * SkinContext.Zoom.Width;
      float thicknessH = thickNess * SkinContext.Zoom.Height;
      PointF[] points = path.PathPoints;
      int pointCount = points.Length;
      int verticeCount = (pointCount) * 2 * 3;

      verts = new PositionColored2Textured[verticeCount];

      float x, y;
      for (int i = 0; i < (pointCount); ++i)
      {
        int offset = i * 6;
        PointF nextpoint = GetNextPoint(points, i, pointCount);
        GetInset(nextpoint, points[i], out x, out y, (double)thicknessW, (double)thicknessH, direction, finalLayoutTransform);
        verts[offset].Position = new Vector3(points[i].X, points[i].Y, 1);
        verts[offset + 1].Position = new Vector3(nextpoint.X, nextpoint.Y, 1);
        verts[offset + 2].Position = new Vector3(x, y, 1);

        verts[offset + 3].Position = new Vector3(nextpoint.X, nextpoint.Y, 1);
        verts[offset + 4].Position = new Vector3(x, y, 1);

        verts[offset + 5].Position = new Vector3(nextpoint.X + (x - points[i].X), nextpoint.Y + (y - points[i].Y), 1);


      }
    }
    static public void StrokePathToTriangleStrip(GraphicsPath path, float thickNess, bool isClosed, out PositionColored2Textured[] verts, ExtendedMatrix finalTransLayoutform)
    {
      PolygonDirection direction = PointsDirection(path);
      StrokePathToTriangleStrip(path, thickNess, isClosed, direction, out verts, finalTransLayoutform);
    }
    static public void StrokePathToTriangleStrip(GraphicsPath path, float thickNess, bool isClosed, PolygonDirection direction, out PositionColored2Textured[] verts, ExtendedMatrix finalLayoutTransform)
    {
      verts = null;
      if (path.PointCount <= 0) return;
      // thickNess /= 2.0f;
      float thicknessW = thickNess * SkinContext.Zoom.Width;
      float thicknessH = thickNess * SkinContext.Zoom.Height;
      PointF[] points = path.PathPoints;
      int pointCount = points.Length;
      int verticeCount = (pointCount) * 2 * 3;

      verts = new PositionColored2Textured[verticeCount];

      float x, y;
      for (int i = 0; i < (pointCount); ++i)
      {
        int offset = i * 6;
        PointF nextpoint = GetNextPoint(points, i, pointCount);
        GetInset(nextpoint, points[i], out x, out y, (double)thicknessW, (double)thicknessH, direction, finalLayoutTransform);
        verts[offset].Position = new Vector3(points[i].X, points[i].Y, 1);
        verts[offset + 1].Position = new Vector3(nextpoint.X, nextpoint.Y, 1);
        verts[offset + 2].Position = new Vector3(x, y, 1);

        verts[offset + 3].Position = new Vector3(nextpoint.X, nextpoint.Y, 1);
        verts[offset + 4].Position = new Vector3(x, y, 1);

        verts[offset + 5].Position = new Vector3(nextpoint.X + (x - points[i].X), nextpoint.Y + (y - points[i].Y), 1);


      }
    }

    /// <summary>
    /// Converts the graphics path to an array of vertices using trianglestrip.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="cx">The cx.</param>
    /// <param name="cy">The cy.</param>
    /// <param name="thickNess">The thick ness.</param>
    /// <returns></returns>
    static public VertexBuffer ConvertPathToTriangleStrip(GraphicsPath path, float thickNess, bool isClosed, out PositionColored2Textured[] verts, ExtendedMatrix finalTransLayoutform)
    {
      PolygonDirection direction = PointsDirection(path);
      return ConvertPathToTriangleStrip(path, thickNess, isClosed, direction, out verts, finalTransLayoutform);
    }
    static public VertexBuffer ConvertPathToTriangleStrip(GraphicsPath path, float thickNess, bool isClosed, PolygonDirection direction, out PositionColored2Textured[] verts, ExtendedMatrix finalLayoutTransform)
    {
      verts = null;
      if (path.PointCount <= 0) return null;
      // thickNess /= 2.0f;
      float thicknessW = thickNess * SkinContext.Zoom.Width;
      float thicknessH = thickNess * SkinContext.Zoom.Height;
      PointF[] points = path.PathPoints;
      int pointCount = points.Length;
      int verticeCount = (pointCount) * 2 * 3;

      VertexBuffer vertexBuffer = PositionColored2Textured.Create(verticeCount);
      verts = new PositionColored2Textured[verticeCount];

      float x, y;
      for (int i = 0; i < (pointCount); ++i)
      {
        int offset = i * 6;
        PointF nextpoint = GetNextPoint(points, i, pointCount);
        GetInset(nextpoint, points[i], out x, out y, (double)thicknessW, (double)thicknessH, direction, finalLayoutTransform);
        verts[offset].Position = new Vector3(points[i].X, points[i].Y, 1);
        verts[offset + 1].Position = new Vector3(nextpoint.X, nextpoint.Y, 1);
        verts[offset + 2].Position = new Vector3(x, y, 1);

        verts[offset + 3].Position = new Vector3(nextpoint.X, nextpoint.Y, 1);
        verts[offset + 4].Position = new Vector3(x, y, 1);

        verts[offset + 5].Position = new Vector3(nextpoint.X + (x - points[i].X), nextpoint.Y + (y - points[i].Y), 1);


      }
      return vertexBuffer;
    }


    /// <summary>
    /// Splits the path into triangles.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    protected VertexBuffer Triangulate(GraphicsPath path, float cx, float cy, bool isClosed, out PositionColored2Textured[] verts, out PrimitiveType primitive)
    {
      verts = null;
      if (path.PointCount <= 3)
      {
        primitive = PrimitiveType.TriangleList;
        return ConvertPathToTriangleFan(path, cx, cy, out verts);
      }
      if (Name == "path134")
      {
      }
      primitive = PrimitiveType.TriangleList;
      CPolygonShape cutPolygon = new CPolygonShape(path, isClosed);
      cutPolygon.CutEar();

      int count = cutPolygon.NumberOfPolygons;
      VertexBuffer vertexBuffer = PositionColored2Textured.Create(count * 3);
      verts = new PositionColored2Textured[count * 3];
      for (int i = 0; i < count; i++)
      {
        CPoint2D[] triangle = cutPolygon[i];
        int offset = i * 3;
        verts[offset].Position = new Vector3(triangle[0].X, triangle[0].Y, 1);
        verts[offset + 1].Position = new Vector3(triangle[1].X, triangle[1].Y, 1);
        verts[offset + 2].Position = new Vector3(triangle[2].X, triangle[2].Y, 1);
      }
      return vertexBuffer;
    }
    /// <summary>
    /// Arranges the UI element
    /// and positions it in the finalrect
    /// </summary>
    /// <param name="finalRect">The final size that the parent computes for the child element</param>
    public override void Arrange(System.Drawing.RectangleF finalRect)
    {
      //Trace.WriteLine(String.Format("shape.arrange :{0} {1},{2} {3}x{4}", this.Name, (int)finalRect.X, (int)finalRect.Y, (int)finalRect.Width, (int)finalRect.Height));
      _finalRect = new System.Drawing.RectangleF(finalRect.Location, finalRect.Size);
      System.Drawing.RectangleF layoutRect = new System.Drawing.RectangleF(finalRect.X, finalRect.Y, finalRect.Width, finalRect.Height);

      layoutRect.X += (float)(Margin.X * SkinContext.Zoom.Width);
      layoutRect.Y += (float)(Margin.Y * SkinContext.Zoom.Height);
      layoutRect.Width -= (float)((Margin.X + Margin.W) * SkinContext.Zoom.Width);
      layoutRect.Height -= (float)((Margin.Y + Margin.Z) * SkinContext.Zoom.Height);
      ActualPosition = new Vector3(layoutRect.Location.X, layoutRect.Location.Y, 1.0f); ;
      ActualWidth = layoutRect.Width;
      ActualHeight = layoutRect.Height;
      _performLayout = true;
      _finalLayoutTransform = SkinContext.FinalLayoutTransform;
      base.Arrange(layoutRect);
      if (Window!=null) Window.Invalidate(this);
    }

    /// <summary>
    /// measures the size in layout required for child elements and determines a size for the FrameworkElement-derived class.
    /// </summary>
    /// <param name="availableSize">The available size that this element can give to child elements.</param>
    public override void Measure(System.Drawing.SizeF availableSize)
    {
      float marginWidth = (float)((Margin.X + Margin.W) * SkinContext.Zoom.Width);
      float marginHeight = (float)((Margin.Y + Margin.Z) * SkinContext.Zoom.Height);
      _desiredSize = new System.Drawing.SizeF((float)Width * SkinContext.Zoom.Width, (float)Height * SkinContext.Zoom.Height);
      if (Width <= 0)
        _desiredSize.Width = (float)(availableSize.Width - marginWidth);
      if (Height <= 0)
        _desiredSize.Height = (float)(availableSize.Height - marginHeight);

      if (LayoutTransform != null)
      {
        ExtendedMatrix m = new ExtendedMatrix();
        LayoutTransform.GetTransform(out m);
        SkinContext.AddLayoutTransform(m);
      }
      SkinContext.FinalLayoutTransform.TransformSize(ref _desiredSize);

      if (LayoutTransform != null)
      {
        SkinContext.RemoveLayoutTransform();
      }
      _desiredSize.Width += marginWidth;
      _desiredSize.Height += marginHeight;
      _availableSize = new SizeF(availableSize.Width, availableSize.Height);
      base.Measure(availableSize);
      //Trace.WriteLine(String.Format("shape.measure :{0} {1}x{2} returns {3}x{4}", this.Name, (int)availableSize.Width, (int)availableSize.Height, (int)_desiredSize.Width, (int)_desiredSize.Height));
    }


    static protected void ZCross(ref PointF left, ref PointF right, out double result)
    {
      result = left.X * right.Y - left.Y * right.X;
    }
    static public void CalcCentroid(GraphicsPath path, out float cx, out float cy)
    {
      int pointCount = path.PointCount;
      if (pointCount == 0)
      {
        cx = 0;
        cy = 0;
        return;
      }
      PointF[] pathPoints = path.PathPoints;
      Vector2 centroid = new Vector2();
      double temp;
      double area = 0;
      PointF v1 = (PointF)pathPoints[pointCount - 1];
      PointF v2;
      for (int index = 0; index < pointCount; ++index, v1 = v2)
      {
        v2 = pathPoints[index];
        ZCross(ref v1, ref v2, out temp);
        area += temp;
        centroid.X += (float)((v1.X + v2.X) * temp);
        centroid.Y += (float)((v1.Y + v2.Y) * temp);
      }
      temp = 1 / (Math.Abs(area) * 3);
      centroid.X *= (float)temp;
      centroid.Y *= (float)temp;

      cx = (float)(Math.Abs(centroid.X));
      cy = (float)(Math.Abs(centroid.Y));
    }
    static public PolygonDirection PointsDirection(GraphicsPath points)
    {
      int nCount = 0, j = 0, k = 0;
      int nPoints = points.PointCount;

      if (nPoints < 3)
        return PolygonDirection.Unknown;
      PointF[] pathPoints = points.PathPoints;
      for (int i = 0; i < nPoints - 2; i++)
      {
        j = (i + 1) % nPoints; //j:=i+1;
        k = (i + 2) % nPoints; //k:=i+2;

        double crossProduct = (pathPoints[j].X - pathPoints[i].X) * (pathPoints[k].Y - pathPoints[j].Y);
        crossProduct = crossProduct - ((pathPoints[j].Y - pathPoints[i].Y) * (pathPoints[k].X - pathPoints[j].X));

        if (crossProduct > 0)
          nCount++;
        else
          nCount--;
      }

      if (nCount < 0)
        return PolygonDirection.Count_Clockwise;
      else if (nCount > 0)
        return PolygonDirection.Clockwise;
      else
        return PolygonDirection.Unknown;
    }


    #region math helpers
    /// <summary>the slope of v, or NaN if it is nearly vertical</summary>
    /// <param name="v">Vector to take slope from</param>
    protected float vectorSlope(Vector2 v)
    {
      return Math.Abs(v.X) < 0.001f ? float.NaN : (v.Y / v.X);
    }

    /// <summary>Finds the intercept of a line</summary>
    /// <param name="point">A point on the line</param>
    /// <param name="slope">The slope of the line</param>
    protected float lineIntercept(Vector2 point, float slope)
    {
      return point.Y - slope * point.X;
    }

    /// <summary>The unit length right-hand normal of v</summary>
    /// <param name="v">Vector to find the normal of</param>
    protected Vector2 normal(Vector2 v)
    {
      //Avoid division by zero/returning a zero vector
      if (Math.Abs(v.Y) < 0.0001) return new Vector2(0, sgn(v.X));
      if (Math.Abs(v.X) < 0.0001) return new Vector2(-sgn(v.Y), 0);

      float r = 1 / v.Length();
      return new Vector2(-v.Y * r, v.X * r);
    }

    /// <summary>Finds the sign of a number</summary>
    /// <param name="x">Number to take the sign of</param>
    private float sgn(float x)
    {
      return (x > 0f ? 1f : (x < 0f ? -1f : 0f));
    }
    #endregion

    #region point calculation
    /// <overloads>Computes points needed to connect thick lines properly</overloads>
    /// <summary>Finds the inside vertex at a point in a line strip</summary>
    /// <param name="distance">Distance from the center of the line that the point should be</param>
    /// <param name="lastPoint">Point on the strip before point</param>
    /// <param name="point">Point whose inside vertex we are finding</param>
    /// <param name="nextPoint">Point on the strip after point</param>
    /// <param name="slope">Assigned the slope of the line from lastPoint to point</param>
    /// <param name="intercept">Assigned the intercept of the line with the computed slope through the inner point</param>
    /// <remarks>
    /// This overload is less efficient for calculating a sequence of inner vertices because
    /// it does not reuse results from previously calculated points
    /// </remarks>
    protected Vector2 InnerPoint(ref Matrix matrix, Vector2 distance, Vector2 lastPoint, Vector2 point, Vector2 nextPoint, out float slope, out float intercept)
    {
      Vector2 lastDifference = point - lastPoint;
      slope = vectorSlope(lastDifference);
      intercept = lineIntercept(lastPoint + Vector2.Modulate(distance, normal(lastDifference)), slope);
      return InnerPoint(ref matrix, distance, ref slope, ref intercept, lastPoint + Vector2.Modulate(distance, normal(lastDifference)), point, nextPoint);
    }

    /// <summary>Finds the inside vertex at a point in a line strip</summary>
    /// <param name="distance">Distance from the center of the line that the point should be</param>
    /// <param name="lastSlope">Slope of the previous line in, slope from point to nextPoint out</param>
    /// <param name="lastIntercept">Intercept of the previous line in, intercept of the line through point and nextPoint out</param>
    /// <param name="lastInnerPoint">Last computed inner point</param>
    /// <param name="point">Point whose inside vertex we are finding</param>
    /// <param name="nextPoint">Point on the strip after point</param>
    /// <remarks>
    /// This overload can reuse information calculated about the previous point, so it is more
    /// efficient for computing the inside of a string of contiguous points on a strip
    /// </remarks>
    protected Vector2 InnerPoint(ref Matrix matrix, Vector2 distance, ref float lastSlope, ref float lastIntercept, Vector2 lastInnerPoint, Vector2 point, Vector2 nextPoint)
    {
      Vector2 edgeVector = nextPoint - point;
      //Vector2 innerPoint = nextPoint + distance * normal(edgeVector);
      Vector2 innerPoint = Vector2.Modulate(distance, normal(edgeVector));

      TransformXY(ref innerPoint, ref matrix);
      innerPoint = nextPoint + innerPoint;

      float slope = vectorSlope(edgeVector);
      float intercept = lineIntercept(innerPoint, slope);

      float safeSlope, safeIntercept;	//Slope and intercept on one of the lines guaranteed not to be vertical
      float x;						//X-coordinate of intersection

      if (float.IsNaN(slope))
      {
        safeSlope = lastSlope;
        safeIntercept = lastIntercept;
        x = innerPoint.X;
      }
      else if (float.IsNaN(lastSlope))
      {
        safeSlope = slope;
        safeIntercept = intercept;
        x = lastInnerPoint.X;
      }
      else if (Math.Abs(slope - lastSlope) < 0.001)
      {
        safeSlope = slope;
        safeIntercept = intercept;
        x = lastInnerPoint.X;
      }
      else
      {
        safeSlope = slope;
        safeIntercept = intercept;
        x = (lastIntercept - intercept) / (slope - lastSlope);
      }

      if (!float.IsNaN(slope))
        lastSlope = slope;
      if (!float.IsNaN(intercept))
        lastIntercept = intercept;

      return new Vector2(x, safeSlope * x + safeIntercept);
    }
    public void TransformXY(ref Vector2 vector, ref Matrix m)
    {
      float w1 = vector.X * m.M11 + vector.Y * m.M21;
      float h1 = vector.X * m.M12 + vector.Y * m.M22;
      vector.X = w1;
      vector.Y = h1;
    }

    /// <summary>
    /// Generates the vertices of a thickened line strip
    /// </summary>
    /// <param name="points">Sequence of points on the line strip</param>
    /// <param name="thickness">Thickness of the line</param>
    /// <param name="close">Whether to connect the last point back to the first</param>
    /// <param name="widthMode">How to place the weight of the line relative to it</param>
    /// <returns>Points ready to pass to the Transform constructor</returns>
    protected VertexBuffer CalculateLinePoints(GraphicsPath path, float thickness, bool close, WidthMode widthMode, out PositionColored2Textured[] verts)
    {
      verts = null;
      if (path.PointCount < 3)
      {
        if (close) return null;
        else if (path.PointCount < 2)
          return null;
      }

      Matrix matrix = new Matrix();
      if (_finalLayoutTransform != null)
      {
        matrix = _finalLayoutTransform.Matrix;
      }
      if (LayoutTransform != null)
      {
        ExtendedMatrix em;
        LayoutTransform.GetTransform(out em);
        matrix *= em.Matrix;
      }
      int count = path.PointCount;
      PointF[] pathPoints = path.PathPoints;
      if (pathPoints[count - 2] == pathPoints[count - 1])
        count--;
      Vector2[] points = new Vector2[count];
      for (int i = 0; i < count; ++i)
      {
        points[i] = new Vector2(pathPoints[i].X, pathPoints[i].Y);
      }

      Vector2 innerDistance = new Vector2(0, 0);
      switch (widthMode)
      {
        case WidthMode.Centered:
          //innerDistance =thickness / 2;
          innerDistance = new Vector2((thickness / 2) * SkinContext.Zoom.Width, (thickness / 2) * SkinContext.Zoom.Height);
          break;
        case WidthMode.LeftHanded:
          //innerDistance = -thickness;
          innerDistance = new Vector2((-thickness) * SkinContext.Zoom.Width, (-thickness) * SkinContext.Zoom.Height);
          break;
        case WidthMode.RightHanded:
          //innerDistance = thickness;
          innerDistance = new Vector2((thickness) * SkinContext.Zoom.Width, (thickness) * SkinContext.Zoom.Height);
          break;
      }


      Vector2[] outPoints = new Vector2[(points.Length + (close ? 1 : 0)) * 2];

      float slope, intercept;
      //Get the endpoints
      if (close)
      {
        //Get the overlap points
        int lastIndex = outPoints.Length - 4;
        outPoints[lastIndex] = InnerPoint(ref matrix, innerDistance, points[points.Length - 2], points[points.Length - 1], points[0], out slope, out intercept);
        outPoints[0] = InnerPoint(ref matrix, innerDistance, ref slope, ref intercept, outPoints[lastIndex], points[0], points[1]);
      }
      else
      {
        //Take endpoints based on the end segments' normals alone
        outPoints[0] = Vector2.Modulate(innerDistance, normal(points[1] - points[0]));
        TransformXY(ref outPoints[0], ref matrix);
        outPoints[0] = points[0] + outPoints[0];

        //outPoints[0] = points[0] + innerDistance * normal(points[1] - points[0]);
        Vector2 norm = Vector2.Modulate(innerDistance, normal(points[points.Length - 1] - points[points.Length - 2])); //DEBUG

        TransformXY(ref norm, ref matrix);
        outPoints[outPoints.Length - 2] = points[points.Length - 1] + norm;

        //Get the slope and intercept of the first segment to feed into the middle loop
        slope = vectorSlope(points[1] - points[0]);
        intercept = lineIntercept(outPoints[0], slope);
      }

      //Get the middle points
      for (int i = 1; i < points.Length - 1; i++)
      {
        outPoints[2 * i] = InnerPoint(ref matrix, innerDistance, ref slope, ref intercept, outPoints[2 * (i - 1)], points[i], points[i + 1]);
      }

      //Derive the outer points from the inner points
      if (widthMode == WidthMode.Centered)
      {
        for (int i = 0; i < points.Length; i++)
        {
          outPoints[2 * i + 1] = 2 * points[i] - outPoints[2 * i];
        }
      }
      else
      {
        for (int i = 0; i < points.Length; i++)
        {
          outPoints[2 * i + 1] = points[i];
        }
      }

      //Closed strips must repeat the first two points
      if (close)
      {
        outPoints[outPoints.Length - 2] = outPoints[0];
        outPoints[outPoints.Length - 1] = outPoints[1];
      }
      int verticeCount = outPoints.Length;
      VertexBuffer vertexBuffer = PositionColored2Textured.Create(verticeCount);
      verts = new PositionColored2Textured[verticeCount];

      for (int i = 0; i < verticeCount; ++i)
      {
        verts[i].Position = new Vector3(outPoints[i].X, outPoints[i].Y, 1);
      }
      return vertexBuffer;
    }
    #endregion

    public override void Deallocate()
    {
      base.Deallocate();
      if (Fill != null)
        Fill.Deallocate();
      if (Stroke != null)
        Stroke.Deallocate();
      if (_fillAsset != null)
      {
        _fillAsset.Free(true);
        ContentManager.Remove(_fillAsset);
        _fillAsset = null;
      }
      if (_borderAsset != null)
      {
        _borderAsset.Free(true);
        ContentManager.Remove(_borderAsset);
        _borderAsset = null;
      }
      if (_fillContext != null)
      {
        RenderPipeline.Instance.Remove(_fillContext);
        _fillContext = null;
      }
      if (_strokeContext != null)
      {
        RenderPipeline.Instance.Remove(_strokeContext);
        _strokeContext = null;
      }

    }

    public override void Allocate()
    {
      base.Allocate();
      if (Fill != null)
        Fill.Allocate();
      if (Stroke != null)
        Stroke.Allocate();
      _performLayout = true;
    }
  }
}
