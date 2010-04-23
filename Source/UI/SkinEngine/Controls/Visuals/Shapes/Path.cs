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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.DirectX;
using MediaPortal.UI.SkinEngine.DirectX.Triangulate;
using MediaPortal.UI.SkinEngine.Rendering;
using MediaPortal.UI.SkinEngine.Xaml;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.UI.SkinEngine.SkinManagement;
using SlimDX.Direct3D9;
using FillMode=System.Drawing.Drawing2D.FillMode;

namespace MediaPortal.UI.SkinEngine.Controls.Visuals.Shapes
{
  public class Path : Shape
  {
    #region Protected fields

    protected AbstractProperty _dataProperty;
    protected bool _fillDisabled;

    #endregion

    #region Ctor

    public Path()
    {
      Init();
    }

    void Init()
    {
      _dataProperty = new SProperty(typeof(string), string.Empty);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      Path p = (Path) source;
      Data = p.Data;
    }

    #endregion

    public AbstractProperty DataProperty
    {
      get { return _dataProperty; }
    }

    public string Data
    {
      get { return (string)_dataProperty.GetValue(); }
      set { _dataProperty.SetValue(value); }
    }

    protected override void DoPerformLayout()
    {
      base.DoPerformLayout();

      ExtendedMatrix m = new ExtendedMatrix();
      if (_finalLayoutTransform != null)
        m.Matrix *= _finalLayoutTransform.Matrix;
      if (LayoutTransform != null)
      {
        ExtendedMatrix em;
        LayoutTransform.GetTransform(out em);
        m.Matrix *= em.Matrix;
      }
      RectangleF rect = new RectangleF(ActualPosition.X, ActualPosition.Y, (float) ActualWidth, (float) ActualHeight);

      //Fill brush
      if (Fill != null || ((Stroke != null && StrokeThickness > 0)))
      {
        using (GraphicsPath path = CalculateTransformedPath(rect, _finalLayoutTransform))
        {
          if (Fill != null && !_fillDisabled)
          {
            GraphicsPathIterator gpi = new GraphicsPathIterator(path);
            PositionColored2Textured[][] subPathVerts = new PositionColored2Textured[gpi.SubpathCount][];
            GraphicsPath subPath = new GraphicsPath();
            for (int i = 0; i < subPathVerts.Length; i++)
            {
              bool isClosed;
              gpi.NextSubpath(subPath, out isClosed);
              TriangulateHelper.Triangulate(subPath, out subPathVerts[i]);
            }
            RemovePrimitiveContext(ref _fillContext);
            PositionColored2Textured[] verts;
            GraphicsPathHelper.Flatten(subPathVerts, out verts);
            if (verts != null)
            {
              int numVertices = verts.Length / 3;
              Fill.SetupBrush(ActualBounds, FinalLayoutTransform, ActualPosition.Z, ref verts);
              _fillContext = new PrimitiveContext(numVertices, ref verts, PrimitiveType.TriangleList);
              AddPrimitiveContext(_fillContext);
              Fill.SetupPrimitive(_fillContext);
            }
          }
          if (Stroke != null && StrokeThickness > 0)
          {
            GraphicsPathIterator gpi = new GraphicsPathIterator(path);
            PositionColored2Textured[][] subPathVerts = new PositionColored2Textured[gpi.SubpathCount][];
            GraphicsPath subPath = new GraphicsPath();
            for (int i = 0; i < subPathVerts.Length; i++)
            {
              bool isClosed;
              gpi.NextSubpath(subPath, out isClosed);
              TriangulateHelper.TriangulateStroke_TriangleList(subPath, (float) StrokeThickness, isClosed,
                  out subPathVerts[i], _finalLayoutTransform);
            }
            RemovePrimitiveContext(ref _strokeContext);
            PositionColored2Textured[] verts;
            GraphicsPathHelper.Flatten(subPathVerts, out verts);
            if (verts != null)
            {
              int numVertices = verts.Length/3;
              Stroke.SetupBrush(ActualBounds, FinalLayoutTransform, ActualPosition.Z, ref verts);
              _strokeContext = new PrimitiveContext(numVertices, ref verts, PrimitiveType.TriangleList);
              AddPrimitiveContext(_strokeContext);
              Stroke.SetupPrimitive(_strokeContext);
            }
          }
        }
      }
    }

    protected override SizeF CalculateDesiredSize(SizeF totalSize)
    {
      using (GraphicsPath p = CalculateTransformedPath(new RectangleF(0, 0, 0, 0), null))
      {
        RectangleF bounds = p.GetBounds();
        return new SizeF(bounds.Width * SkinContext.Zoom.Width, bounds.Height * SkinContext.Zoom.Height);
      }
    }

    protected GraphicsPath ParsePath()
    {
      GraphicsPath result = new GraphicsPath(FillMode.Alternate);
      PointF lastPoint = new PointF();
      Regex regex = new Regex(@"[a-zA-Z][-0-9\.,-0-9\. ]*");
      MatchCollection matches = regex.Matches(Data);

      foreach (Match match in matches)
      {
        char cmd = match.Value[0];
        PointF[] points;
        string pointsStr = match.Value.Substring(1).Trim();
        if (pointsStr.Length > 0)
        {
          string[] txtpoints = pointsStr.Split(new char[] { ',', ' ' });
          if (txtpoints.Length == 1)
          {
            points = new PointF[1];
            points[0].X = (float) TypeConverter.Convert(txtpoints[0], typeof(float));
          }
          else
          {
            int c = txtpoints.Length / 2;
            points = new PointF[c];
            for (int i = 0; i < c; i++)
            {
              points[i].X = (float) TypeConverter.Convert(txtpoints[i * 2], typeof(float));
              if (i + 1 < txtpoints.Length)
                points[i].Y = (float) TypeConverter.Convert(txtpoints[i * 2 + 1], typeof(float));
            }
          }
        }
        else
          points = new PointF[] {};
        switch (cmd)
        {
          case 'm':
            {
              //Relative origin
              PointF point = points[0];
              lastPoint = new PointF(lastPoint.X + point.X, lastPoint.Y + point.Y);
              result.StartFigure();
            }
            break;
          case 'M':
            {
              //Absolute origin
              lastPoint = points[0];
              result.StartFigure();
            }
            break;
          case 'L':
            //Absolute Line
            for (int i = 0; i < points.Length; ++i)
            {
              result.AddLine(lastPoint, points[i]);
              lastPoint = points[i];
            }
            break;
          case 'l':
            //Relative Line
            for (int i = 0; i < points.Length; ++i)
            {
              points[i].X += lastPoint.X;
              points[i].Y += lastPoint.Y;
              result.AddLine(lastPoint, points[i]);
              lastPoint = points[i];
            }
            break;
          case 'H':
            {
              //Horizontal line to absolute X 
              PointF point1 = new PointF(points[0].X, lastPoint.Y);
              result.AddLine(lastPoint, point1);
              lastPoint = new PointF(point1.X, point1.Y);
            }
            break;
          case 'h':
            {
              //Horizontal line to relative X
              PointF point1 = new PointF(lastPoint.X + points[0].X, lastPoint.Y);
              result.AddLine(lastPoint, point1);
              lastPoint = new PointF(point1.X, point1.Y);
            }
            break;
          case 'V':
            {
              //Vertical line to absolute y 
              PointF point1 = new PointF(lastPoint.X, points[0].X);
              result.AddLine(lastPoint, point1);
              lastPoint = new PointF(point1.X, point1.Y);
            }
            break;
          case 'v':
            {
              //Vertical line to relative y
              PointF point1 = new PointF(lastPoint.X, lastPoint.Y + points[0].X);
              result.AddLine(lastPoint, point1);
              lastPoint = new PointF(point1.X, point1.Y);
            }
            break;
          case 'C':
            //Quadratic Bezier curve command C21,17,17,21,13,21
            for (int i = 0; i < points.Length; i += 3)
            {
              result.AddBezier(lastPoint, points[i], points[i + 1], points[i + 2]);
              lastPoint = points[i + 2];
            }
            break;
          case 'c':
            //Quadratic Bezier curve command
            for (int i = 0; i < points.Length; i += 3)
            {
              points[i].X += lastPoint.X;
              points[i].Y += lastPoint.Y;
              result.AddBezier(lastPoint, points[i], points[i + 1], points[i + 2]);
              lastPoint = points[i + 2];
            }
            break;
          case 'F':
            //Set fill mode command
            if (points[0].X == 0.0f)
            {
              //the EvenOdd fill rule
              //Rule that determines whether a point is in the fill region by drawing a ray 
              //from that point to infinity in any direction and counting the number of path 
              //segments within the given shape that the ray crosses. If this number is odd, 
              //the point is inside; if even, the point is outside.
              result.FillMode = FillMode.Alternate;
            }
            else if (points[0].X == 1.0f)
            {
              //the Nonzero fill rule.
              //Rule that determines whether a point is in the fill region of the 
              //path by drawing a ray from that point to infinity in any direction
              //and then examining the places where a segment of the shape crosses
              //the ray. Starting with a count of zero, add one each time a segment 
              //crosses the ray from left to right and subtract one each time a path
              //segment crosses the ray from right to left. After counting the crossings,
              //if the result is zero then the point is outside the path. Otherwise, it is inside.
              result.FillMode = FillMode.Winding;
            }
            break;
          case 'z':
            result.CloseFigure();
            break;
        }
      }
      return result;
    }

    protected GraphicsPath CalculateTransformedPath(RectangleF baseRect, ExtendedMatrix finalTransform)
    {
      GraphicsPath result = ParsePath();
      Matrix m = new Matrix();
      RectangleF bounds = result.GetBounds();
      _fillDisabled = bounds.Width < StrokeThickness || bounds.Height < StrokeThickness;
      if (Width > 0) baseRect.Width = (float) Width * SkinContext.Zoom.Width;
      if (Height > 0) baseRect.Height = (float) Height * SkinContext.Zoom.Height;
      float scaleW;
      float scaleH;
      if (Stretch == Stretch.Fill)
      {
        // baseRect is already zoomed, bounds are not, so scale will contain the zoom factor
        scaleW = baseRect.Width / bounds.Width;
        scaleH = baseRect.Height / bounds.Height;
        m.Translate(-bounds.X, -bounds.Y, MatrixOrder.Append);
      }
      else if (Stretch == Stretch.Uniform)
      {
        // baseRect is already zoomed, bounds are not, so scale will contain the zoom factor
        scaleW = Math.Min(baseRect.Width / bounds.Width, baseRect.Height / bounds.Height);
        scaleH = scaleW;
        m.Translate(-bounds.X, -bounds.Y, MatrixOrder.Append);
      }
      else if (Stretch == Stretch.UniformToFill)
      {
        // baseRect is already zoomed, bounds are not, so scale will contain the zoom factor
        scaleW = Math.Max(baseRect.Width / bounds.Width, baseRect.Height / bounds.Height);
        scaleH = scaleW;
        m.Translate(-bounds.X, -bounds.Y, MatrixOrder.Append);
      }
      else
      { // Stretch == Stretch.None
        // Only in this case we must apply the current zoom. In all other cases, the zoom gets implicitly applied
        // by the alignment factor, which is based on the parent control's size, which is already zoomed.
        scaleW = SkinContext.Zoom.Width;
        scaleH = SkinContext.Zoom.Height;
      }
      // In case bounds.Width or bounds.Height or baseRect.Width or baseRect.Height were 0
      if (scaleW == 0 || float.IsNaN(scaleW) || float.IsInfinity(scaleW)) scaleW = 1;
      if (scaleH == 0 || float.IsNaN(scaleH) || float.IsInfinity(scaleH)) scaleH = 1;
      m.Scale(scaleW, scaleH, MatrixOrder.Append);

      if (finalTransform != null)
        m.Multiply(finalTransform.Get2dMatrix(), MatrixOrder.Append);

      if (LayoutTransform != null)
      {
        ExtendedMatrix em;
        LayoutTransform.GetTransform(out em);
        m.Multiply(em.Get2dMatrix(), MatrixOrder.Append);
      }
      m.Translate(baseRect.X, baseRect.Y, MatrixOrder.Append);
      result.Transform(m);
      result.Flatten();
      return result;
    }
  }
}
