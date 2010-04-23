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

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.DirectX;
using MediaPortal.UI.SkinEngine.DirectX.Triangulate;
using MediaPortal.UI.SkinEngine.Rendering;
using SlimDX.Direct3D9;
using RectangleF = System.Drawing.RectangleF;
using SizeF = System.Drawing.SizeF;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.UI.SkinEngine.SkinManagement;

namespace MediaPortal.UI.SkinEngine.Controls.Visuals.Shapes
{
  public class Polygon : Shape
  {
    #region Protected fields

    protected AbstractProperty _pointsProperty;

    #endregion

    #region Ctor

    public Polygon()
    {
      Init();
    }

    void Init()
    {
      _pointsProperty = new SProperty(typeof(IList<Point>), new List<Point>());
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      Polygon p = (Polygon) source;
      Points = new List<Point>(p.Points);
    }

    #endregion

    #region Public properties

    public AbstractProperty PointsProperty
    {
      get { return _pointsProperty; }
    }

    public IList<Point> Points
    {
      get { return (IList<Point>) _pointsProperty.GetValue(); }
      set { _pointsProperty.SetValue(value); }
    }

    #endregion

    #region Layouting

    protected override void DoPerformLayout()
    {
      base.DoPerformLayout();
      
      double w = ActualWidth;
      double h = ActualHeight;
      SizeF rectSize = new SizeF((float) w, (float) h);

      ExtendedMatrix m = new ExtendedMatrix();
      m.Matrix *= _finalLayoutTransform.Matrix;
      if (LayoutTransform != null)
      {
        ExtendedMatrix em;
        LayoutTransform.GetTransform(out em);
        m.Matrix *= em.Matrix;
      }
      m.InvertSize(ref rectSize);
      RectangleF rect = new RectangleF(ActualPosition.X, ActualPosition.Y, rectSize.Width, rectSize.Height);

      //Fill brush
      PositionColored2Textured[] verts;
      if (Fill != null || (Stroke != null && StrokeThickness > 0))
      {
        using (GraphicsPath path = GetPolygon(rect))
        {
          float centerX;
          float centerY;
          TriangulateHelper.CalcCentroid(path, out centerX, out centerY);
          if (Fill != null)
          {
            RemovePrimitiveContext(ref _fillContext);
            TriangulateHelper.FillPolygon_TriangleList(path, centerX, centerY, out verts);
            int numVertices = verts.Length / 3;
            Fill.SetupBrush(ActualBounds, FinalLayoutTransform, ActualPosition.Z, verts);
            _fillContext = new PrimitiveContext(numVertices, ref verts, PrimitiveType.TriangleList);
            AddPrimitiveContext(_fillContext);
            Fill.SetupPrimitive(_fillContext);
          }

          if (Stroke != null && StrokeThickness > 0)
          {
            RemovePrimitiveContext(ref _strokeContext);
            TriangulateHelper.TriangulateStroke_TriangleList(path, (float) StrokeThickness, true, out verts, _finalLayoutTransform);
            int numVertices = verts.Length / 3;
            Stroke.SetupBrush(ActualBounds, FinalLayoutTransform, ActualPosition.Z, verts);
            _strokeContext = new PrimitiveContext(numVertices, ref verts, PrimitiveType.TriangleList);
            AddPrimitiveContext(_strokeContext);
            Stroke.SetupPrimitive(_strokeContext);
          }
        }
      }
    }

    /// <summary>
    /// Get the desired Rounded Rectangle path.
    /// </summary>
    private GraphicsPath GetPolygon(RectangleF baseRect)
    {
      Point[] points = new Point[Points.Count];
      for (int i = 0; i < Points.Count; ++i)
        points[i] = Points[i];
      GraphicsPath mPath = new GraphicsPath();
      mPath.AddPolygon(points);
      mPath.CloseFigure();

      Matrix m = new Matrix();
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

    #endregion
  }
}
