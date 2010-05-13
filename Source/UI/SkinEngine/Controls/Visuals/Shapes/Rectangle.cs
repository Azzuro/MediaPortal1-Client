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
using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.DirectX;
using MediaPortal.UI.SkinEngine.DirectX.Triangulate;
using MediaPortal.UI.SkinEngine.Rendering;
using MediaPortal.Utilities.DeepCopy;
using SlimDX.Direct3D9;

namespace MediaPortal.UI.SkinEngine.Controls.Visuals.Shapes
{
  public class Rectangle : Shape
  {
    #region Protected fields

    protected AbstractProperty _radiusXProperty;
    protected AbstractProperty _radiusYProperty;

    #endregion

    #region Ctor

    public Rectangle()
    {
      Init();
      Attach();
    }

    public override void Dispose()
    {
      base.Dispose();
      Detach();
    }

    void Init()
    {
      _radiusXProperty = new SProperty(typeof(double), 0.0);
      _radiusYProperty = new SProperty(typeof(double), 0.0);
    }

    void Attach()
    {
      _radiusXProperty.Attach(OnRadiusChanged);
      _radiusYProperty.Attach(OnRadiusChanged);
    }

    void Detach()
    {
      _radiusXProperty.Detach(OnRadiusChanged);
      _radiusYProperty.Detach(OnRadiusChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      Rectangle r = (Rectangle) source;
      RadiusX = r.RadiusX;
      RadiusY = r.RadiusY;
      Attach();
    }

    #endregion

    void OnRadiusChanged(AbstractProperty property, object oldValue)
    {
      InvalidateLayout();
      InvalidateParentLayout();
    }

    public AbstractProperty RadiusXProperty
    {
      get { return _radiusXProperty; }
    }

    public double RadiusX
    {
      get { return (double) _radiusYProperty.GetValue(); }
      set { _radiusYProperty.SetValue(value); }
    }

    public AbstractProperty RadiusYProperty
    {
      get { return _radiusYProperty; }
    }

    public double RadiusY
    {
      get { return (double) _radiusYProperty.GetValue(); }
      set { _radiusYProperty.SetValue(value); }
    }

    protected override void ArrangeOverride()
    {
      base.ArrangeOverride();
      _performLayout = true;
    }

    protected override void DoPerformLayout(RenderContext context)
    {
      base.DoPerformLayout(context);

      // Setup brushes
      DisposePrimitiveContext(ref _fillContext);
      DisposePrimitiveContext(ref _strokeContext);
      PositionColored2Textured[] verts;
      if (Fill != null || (Stroke != null && StrokeThickness > 0))
      {
        GraphicsPath path;
        using (path = CreateRectanglePath(_innerRect))
        {
          if (path.PointCount == 0)
            return;
          float centerX = _innerRect.Width / 2 + _innerRect.Left;
          float centerY = _innerRect.Height / 2 + _innerRect.Top;
          if (Fill != null)
          {
            TriangulateHelper.FillPolygon_TriangleList(path, centerX, centerY, out verts);
            int numVertices = verts.Length / 3;
            Fill.SetupBrush(this, ref verts, context.ZOrder);
            _fillContext = new PrimitiveContext(numVertices, ref verts, PrimitiveType.TriangleList);
          }

          if (Stroke != null && StrokeThickness > 0)
          {
            TriangulateHelper.TriangulateStroke_TriangleList(path, (float) StrokeThickness, true, out verts, null);
            int numVertices = verts.Length / 3;
            Stroke.SetupBrush(this, ref verts, context.ZOrder);
            _strokeContext = new PrimitiveContext(numVertices, ref verts, PrimitiveType.TriangleList);
          }
        }
      }
    }

    protected GraphicsPath CreateRectanglePath(RectangleF rect)
    {
      return GraphicsPathHelper.CreateRoundedRectPath(rect, (float) RadiusX, (float) RadiusY, null);
    }
  }
}
