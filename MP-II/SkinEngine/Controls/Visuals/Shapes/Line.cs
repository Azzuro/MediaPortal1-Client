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
using System.Drawing.Drawing2D;
using MediaPortal.Presentation.DataObjects;
using MediaPortal.SkinEngine;
using MediaPortal.SkinEngine.DirectX;
using MediaPortal.SkinEngine.Rendering;
using RectangleF = System.Drawing.RectangleF;
using PointF = System.Drawing.PointF;
using SizeF = System.Drawing.SizeF;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.SkinEngine.SkinManagement;

namespace MediaPortal.SkinEngine.Controls.Visuals.Shapes
{
  public class Line : Shape
  {
    #region Private fields

    Property _x1Property;
    Property _y1Property;
    Property _x2Property;
    Property _y2Property;

    #endregion

    #region Ctor

    public Line()
    {
      Init();
      Attach();
    }

    void Init()
    {
      _x1Property = new Property(typeof(double), 0.0);
      _y1Property = new Property(typeof(double), 0.0);
      _x2Property = new Property(typeof(double), 0.0);
      _y2Property = new Property(typeof(double), 0.0);
    }

    void Attach()
    {
      _x1Property.Attach(OnCoordinateChanged);
      _y1Property.Attach(OnCoordinateChanged);
      _x2Property.Attach(OnCoordinateChanged);
      _y2Property.Attach(OnCoordinateChanged);
    }

    void Detach()
    {
      _x1Property.Detach(OnCoordinateChanged);
      _y1Property.Detach(OnCoordinateChanged);
      _x2Property.Detach(OnCoordinateChanged);
      _y2Property.Detach(OnCoordinateChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      Line l = source as Line;
      X1 = copyManager.GetCopy(l.X1);
      Y1 = copyManager.GetCopy(l.Y1);
      X2 = copyManager.GetCopy(l.X2);
      Y2 = copyManager.GetCopy(l.Y2);
      Attach();
    }

    #endregion

    void OnCoordinateChanged(Property property)
    {
      Invalidate();
      if (Screen != null) Screen.Invalidate(this);
    }

    public Property X1Property
    {
      get { return _x1Property; }
    }

    public double X1
    {
      get { return (double)_x1Property.GetValue(); }
      set { _x1Property.SetValue(value); }
    }

    public Property Y1Property
    {
      get { return _y1Property; }
    }

    public double Y1
    {
      get { return (double)_y1Property.GetValue(); }
      set { _y1Property.SetValue(value); }
    }

    public Property X2Property
    {
      get { return _x2Property; }
    }

    public double X2
    {
      get { return (double)_x2Property.GetValue(); }
      set { _x2Property.SetValue(value); }
    }

    public Property Y2Property
    {
      get { return _y2Property; }
    }

    public double Y2
    {
      get { return (double)_y2Property.GetValue(); }
      set { _y2Property.SetValue(value); }
    }

    protected override void PerformLayout()
    {
      //Trace.WriteLine("Line.PerformLayout() " + this.Name);

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
      System.Drawing.RectangleF rect = new System.Drawing.RectangleF(0, 0, rectSize.Width, rectSize.Height);
      rect.X += (float)ActualPosition.X;
      rect.Y += (float)ActualPosition.Y;
      //Fill brush
      GraphicsPath path;
      PositionColored2Textured[] verts;

      //border brush

      if (Stroke != null && StrokeThickness > 0)
      {
        using (path = GetLine(rect))
        {
          CalcCentroid(path, out centerX, out centerY);
          if (_borderAsset == null)
          {
            _borderAsset = new VisualAssetContext("Line._borderContext:" + this.Name);
            ContentManager.Add(_borderAsset);
          }
          if (SkinContext.UseBatching == false)
          {
            _borderAsset.VertexBuffer = ConvertPathToTriangleFan(path, centerX, centerY, out verts);
            if (_borderAsset.VertexBuffer != null)
            {
              Stroke.SetupBrush(this, ref verts);
              PositionColored2Textured.Set(_borderAsset.VertexBuffer, ref verts);
              _verticesCountBorder = (verts.Length / 3);
            }
          }
          else
          {
            Shape.PathToTriangleList(path, centerX, centerY, out verts);
            _verticesCountBorder = (verts.Length / 3);
            Stroke.SetupBrush(this, ref verts);
            if (_strokeContext == null)
            {
              _strokeContext = new PrimitiveContext(_verticesCountBorder, ref verts);
              Stroke.SetupPrimitive(_strokeContext);
              RenderPipeline.Instance.Add(_strokeContext);
            }
            else
            {
              _strokeContext.OnVerticesChanged(_verticesCountBorder, ref verts);
            }
          }
        }
      }

    }

    /*
    public override void DoRender()
    {
      if (!IsVisible) return;
      if (Stroke == null) return;

      if ((_borderAsset != null && !_borderAsset.IsAllocated) || _borderAsset == null)
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
      //SkinContext.RemoveTransform();
      SkinContext.RemoveOpacity();
    }
    */

    public override void Measure(SizeF availableSize)
    {
      using (GraphicsPath p = GetLine(new RectangleF(0, 0, 0, 0)))
      {
        RectangleF bounds = p.GetBounds();
        if (Width > 0) bounds.Width = (float)Width;
        if (Height > 0) bounds.Height = (float)Height;
        bounds.Width *= SkinContext.Zoom.Width;
        bounds.Height *= SkinContext.Zoom.Height;

        float marginWidth = (float)((Margin.Left + Margin.Right) * SkinContext.Zoom.Width);
        float marginHeight = (float)((Margin.Top + Margin.Bottom) * SkinContext.Zoom.Height);
        _desiredSize = new System.Drawing.SizeF((float)bounds.Width, (float)bounds.Height);
        if (availableSize.Width > 0 && Width <= 0)
          _desiredSize.Width = (float)(availableSize.Width - marginWidth);
        if (availableSize.Width > 0 && Height <= 0)
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
        //Trace.WriteLine(String.Format("line.measure :{0} {1}x{2} returns {3}x{4}", this.Name, (int)availableSize.Width, (int)availableSize.Height, (int)_desiredSize.Width, (int)_desiredSize.Height));
      }
    }

    /// <summary>
    /// Get the desired Rounded Rectangle path.
    /// </summary>
    private GraphicsPath GetLine(RectangleF baseRect)
    {
      float x1 = (float)(X1);
      float y1 = (float)(Y1);
      float x2 = (float)(X2);
      float y2 = (float)(Y2);

      float w = (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));

      float ang = (float)((y2 - y1) / (x2 - x1));
      ang = (float)Math.Atan(ang);
      ang *= (float)(180.0f / Math.PI);
      GraphicsPath mPath = new GraphicsPath();
      System.Drawing.Rectangle r = new System.Drawing.Rectangle((int)x1, (int)y1, (int)w, (int)StrokeThickness);
      mPath.AddRectangle(r);
      mPath.CloseFigure();

      System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
      matrix.RotateAt(ang, new PointF(x1, y1), MatrixOrder.Append);


      if (_finalLayoutTransform != null)
        matrix.Multiply(_finalLayoutTransform.Get2dMatrix(), MatrixOrder.Append);
      matrix.Scale(SkinContext.Zoom.Width, SkinContext.Zoom.Height, MatrixOrder.Append);

      if (LayoutTransform != null)
      {
        ExtendedMatrix em;
        LayoutTransform.GetTransform(out em);
        matrix.Multiply(em.Get2dMatrix(), MatrixOrder.Append);
      }
      matrix.Translate(baseRect.X, baseRect.Y, MatrixOrder.Append);
      mPath.Transform(matrix);
      mPath.Flatten();

      return mPath;
    }
  }
}
