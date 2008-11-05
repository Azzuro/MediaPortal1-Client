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

using System.Drawing.Drawing2D;
using MediaPortal.SkinEngine;
using MediaPortal.SkinEngine.ContentManagement;
using MediaPortal.SkinEngine.DirectX;
using MediaPortal.SkinEngine.Rendering;
using RectangleF = System.Drawing.RectangleF;
using SizeF = System.Drawing.SizeF;
using SlimDX;
using MediaPortal.SkinEngine.SkinManagement;

namespace MediaPortal.SkinEngine.Controls.Visuals.Shapes
{
  public class Ellipse : Shape
  {
    public Ellipse()
    { }

    protected override void PerformLayout()
    {
      //Trace.WriteLine("Ellipse.PerformLayout() " + this.Name);

      double w = ActualWidth;
      double h = ActualHeight;
      Vector3 orgPos = new Vector3(ActualPosition.X, ActualPosition.Y, ActualPosition.Z);
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
      System.Drawing.RectangleF rect = new System.Drawing.RectangleF((float)ActualPosition.X - 0.5f, (float)ActualPosition.Y - 0.5f, rectSize.Width + 0.5f, rectSize.Height + 0.5f);


      //Fill brush
      PositionColored2Textured[] verts;
      GraphicsPath path;
      if (Fill != null || (Stroke != null && StrokeThickness > 0))
      {
        using (path = GetEllipse(rect))
        {
          CalcCentroid(path, out centerX, out centerY);
          if (Fill != null)
          {
            if (SkinContext.UseBatching == false)
            {
              if (_fillAsset == null)
              {
                _fillAsset = new VisualAssetContext("Ellipse._fillContext:" + this.Name);
                ContentManager.Add(_fillAsset);
              }
              _fillAsset.VertexBuffer = ConvertPathToTriangleFan(path, centerX, centerY, out verts);
              if (_fillAsset.VertexBuffer != null)
              {
                Fill.SetupBrush(this, ref verts);

                PositionColored2Textured.Set(_fillAsset.VertexBuffer, ref verts);
                _verticesCountFill = (verts.Length / 3);
              }
            }
            else
            {
              Shape.PathToTriangleList(path, centerX, centerY, out verts);
              _verticesCountFill = (verts.Length / 3);
              Fill.SetupBrush(this, ref verts);
              if (_fillContext == null)
              {
                _fillContext = new PrimitiveContext(_verticesCountFill, ref verts);
                Fill.SetupPrimitive(_fillContext);
                RenderPipeline.Instance.Add(_fillContext);
              }
              else
              {
                _fillContext.OnVerticesChanged(_verticesCountFill, ref verts);
              }
            }
          }

          if (Stroke != null && StrokeThickness > 0)
          {
            if (SkinContext.UseBatching == false)
            {
              if (_borderAsset == null)
              {
                _borderAsset = new VisualAssetContext("Ellipse._borderContext:" + this.Name);
                ContentManager.Add(_borderAsset);
              }
              _borderAsset.VertexBuffer = ConvertPathToTriangleStrip(path, (float)StrokeThickness, true, out verts, _finalLayoutTransform, false);
              if (_borderAsset.VertexBuffer != null)
              {
                Stroke.SetupBrush(this, ref verts);
                PositionColored2Textured.Set(_borderAsset.VertexBuffer, ref verts);
                _verticesCountBorder = (verts.Length / 3);
              }
            }
            else
            {
              Shape.StrokePathToTriangleStrip(path, (float)StrokeThickness, true, out verts, _finalLayoutTransform);
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
      //border brush

      ActualPosition = new Vector3(orgPos.X, orgPos.Y, orgPos.Z);
      ActualWidth = w;
      ActualHeight = h;
    }

    /// <summary>
    /// Get the desired Rounded Rectangle path.
    /// </summary>
    /// <param name="baseRect"></param>
    /// <returns></returns>
    private GraphicsPath GetEllipse(RectangleF baseRect)
    {

      GraphicsPath mPath = new GraphicsPath();
      mPath.AddEllipse(baseRect);
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
  }
}
