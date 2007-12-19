#region Copyright (C) 2007 Team MediaPortal

/*
    Copyright (C) 2007 Team MediaPortal
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
using System.Text;
using System.Drawing.Drawing2D;
using MediaPortal.Core.Properties;
using SkinEngine.Controls.Brushes;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using SkinEngine;
using SkinEngine.DirectX;
using RectangleF = System.Drawing.RectangleF;
using PointF = System.Drawing.PointF;
using SizeF = System.Drawing.SizeF;

namespace SkinEngine.Controls.Visuals
{
  public class Border : FrameworkElement
  {
    Property _backgroundProperty;
    Property _borderProperty;
    Property _borderThicknessProperty;
    Property _cornerRadiusProperty;
    VertexBuffer _vertexBufferBackground;
    int _verticesCountBackground;
    VertexBuffer _vertexBufferBorder;
    int _verticesCountBorder;


    public Border()
    {
      _borderProperty = new Property(null);
      _backgroundProperty = new Property(null);
      _borderThicknessProperty = new Property((double)1.0);
      _cornerRadiusProperty = new Property((double)10.0);
    }

    #region properties
    /// <summary>
    /// Gets or sets the background property.
    /// </summary>
    /// <value>The background property.</value>
    public Property BackgroundProperty
    {
      get
      {
        return _backgroundProperty;
      }
      set
      {
        _backgroundProperty = value;
      }
    }

    /// <summary>
    /// Gets or sets the background brush
    /// </summary>
    /// <value>The background.</value>
    public Brush Background
    {
      get
      {
        return _backgroundProperty.GetValue() as Brush;
      }
      set
      {
        _backgroundProperty.SetValue(value);
        OnPropertyChanged();
      }
    }

    /// <summary>
    /// Gets or sets the Border property.
    /// </summary>
    /// <value>The Border property.</value>
    public Property BorderBrushProperty
    {
      get
      {
        return _borderProperty;
      }
      set
      {
        _borderProperty = value;
      }
    }

    /// <summary>
    /// Gets or sets the Border brush
    /// </summary>
    /// <value>The Border.</value>
    public Brush BorderBrush
    {
      get
      {
        return _borderProperty.GetValue() as Brush;
      }
      set
      {
        _borderProperty.SetValue(value);
        OnPropertyChanged();
      }
    }

    /// <summary>
    /// Gets or sets the background property.
    /// </summary>
    /// <value>The background property.</value>
    public Property BorderThicknessProperty
    {
      get
      {
        return _borderThicknessProperty;
      }
      set
      {
        _borderThicknessProperty = value;
      }
    }

    /// <summary>
    /// Gets or sets the background brush
    /// </summary>
    /// <value>The background.</value>
    public double BorderThickness
    {
      get
      {
        return (double)_borderThicknessProperty.GetValue();
      }
      set
      {
        _borderThicknessProperty.SetValue(value);
        OnPropertyChanged();
      }
    }

    /// <summary>
    /// Gets or sets the background property.
    /// </summary>
    /// <value>The background property.</value>
    public Property CornerRadiusProperty
    {
      get
      {
        return _cornerRadiusProperty;
      }
      set
      {
        _cornerRadiusProperty = value;
      }
    }

    /// <summary>
    /// Gets or sets the background brush
    /// </summary>
    /// <value>The background.</value>
    public double CornerRadius
    {
      get
      {
        return (double)_cornerRadiusProperty.GetValue();
      }
      set
      {
        _cornerRadiusProperty.SetValue(value);
      }
    }
    #endregion

    public override void DoRender()
    {
      if (!IsVisible) return;
      if (_vertexBufferBackground == null)
      {
        PerformLayout();
      }

      if (BorderBrush != null)
      {
        BorderBrush.SetupBrush(this);
        BorderBrush.BeginRender();
        GraphicsDevice.Device.SetStreamSource(0, _vertexBufferBorder, 0);
        GraphicsDevice.Device.DrawPrimitives(PrimitiveType.TriangleFan, 0, _verticesCountBorder);
        BorderBrush.EndRender();
      }

      if (Background != null)
      {
        Background.SetupBrush(this);
        Background.BeginRender();
        GraphicsDevice.Device.SetStreamSource(0, _vertexBufferBackground, 0);
        GraphicsDevice.Device.DrawPrimitives(PrimitiveType.TriangleFan, 0, _verticesCountBackground);
        Background.EndRender();
      }
      base.DoRender();
    }


    public void PerformLayout()
    {
      Width = 200;
      Height = 46;
      Free();
      int xoff;
      int yoff;
      if (BorderBrush == null)
      {
        xoff = 100;
        yoff = 100;
        ActualWidth = Width;
        ActualHeight = Height;
      }
      else
      {
        xoff = 100 + (int)BorderThickness;
        yoff = 100 + (int)BorderThickness;
        ActualWidth = Width - 2 * +BorderThickness;
        ActualHeight = Height - 2 * +BorderThickness;
      }
      GraphicsPath path = GetRoundedRect(new RectangleF(xoff, yoff, (float)ActualWidth, (float)ActualHeight), (float)CornerRadius);
      PointF[] vertices = ConvertPathToVertices(path, (int)+(xoff + ActualWidth / 2), (int)(yoff + ActualHeight / 2));

      _vertexBufferBackground = new VertexBuffer(typeof(PositionColored2Textured), vertices.Length, GraphicsDevice.Device, Usage.WriteOnly, PositionColored2Textured.Format, Pool.Default);
      PositionColored2Textured[] verts = (PositionColored2Textured[])_vertexBufferBackground.Lock(0, 0);
      unchecked
      {
        for (int i = 0; i < vertices.Length; ++i)
        {
          verts[i].Color = 0xffffff;
          verts[i].X = vertices[i].X;
          verts[i].Y = vertices[i].Y;
          verts[i].Z = 0.0f;
          verts[i].Tu1 = 0.0f;
          verts[i].Tu2 = 1.0f;
          verts[i].Tv1 = 0.0f;
          verts[i].Tv2 = 1.0f;
        }
      }
      _vertexBufferBackground.Unlock();
      _verticesCountBackground = (verts.Length - 2);

      if (BorderBrush != null)
      {
        xoff = 100;
        yoff = 100;
        ActualWidth = Width;
        ActualHeight = Height;
        path = GetRoundedRect(new RectangleF(xoff, yoff, (float)ActualWidth, (float)ActualHeight), (float)CornerRadius);
        vertices = ConvertPathToVertices(path, (int)+(xoff + ActualWidth / 2), (int)(yoff + ActualHeight / 2));

        _vertexBufferBorder = new VertexBuffer(typeof(PositionColored2Textured), vertices.Length, GraphicsDevice.Device, Usage.WriteOnly, PositionColored2Textured.Format, Pool.Default);
        verts = (PositionColored2Textured[])_vertexBufferBorder.Lock(0, 0);
        unchecked
        {
          for (int i = 0; i < vertices.Length; ++i)
          {
            verts[i].Color = 0xffffff;
            verts[i].X = vertices[i].X;
            verts[i].Y = vertices[i].Y;
            verts[i].Z = 0.0f;
            verts[i].Tu1 = 0.0f;
            verts[i].Tu2 = 1.0f;
            verts[i].Tv1 = 0.0f;
            verts[i].Tv2 = 1.0f;
          }
        }
        _vertexBufferBorder.Unlock();
        _verticesCountBorder = (verts.Length - 2);
      }

    }


    public void Free()
    {
      if (_vertexBufferBackground != null)
      {
        _vertexBufferBackground.Dispose();
        _vertexBufferBackground = null;
      }
    }

    #region Get the desired Rounded Rectangle path.
    private GraphicsPath GetRoundedRect(RectangleF baseRect, float radius)
    {
      // if corner radius is less than or equal to zero, 

      // return the original rectangle 

      if (radius <= 0.0F)
      {
        GraphicsPath mPath = new GraphicsPath();
        mPath.AddRectangle(baseRect);
        mPath.CloseFigure();
        return mPath;
      }

      // if the corner radius is greater than or equal to 

      // half the width, or height (whichever is shorter) 

      // then return a capsule instead of a lozenge 

      if (radius >= (Math.Min(baseRect.Width, baseRect.Height)) / 2.0)
        return GetCapsule(baseRect);

      // create the arc for the rectangle sides and declare 

      // a graphics path object for the drawing 

      float diameter = radius * 2.0F;
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
      path.Flatten();
      return path;
    }
    #endregion

    #region Gets the desired Capsular path.
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
      catch (Exception ex)
      {
        path.AddEllipse(baseRect);
      }
      finally
      {
        path.CloseFigure();
      }
      return path;
    }
    #endregion

    PointF[] ConvertPathToVertices(GraphicsPath path, int cx, int cy)
    {
      PointF[] points = path.PathPoints;
      int verticeCount = points.Length + 2;
      PointF[] vertices = new PointF[verticeCount];
      vertices[0] = new PointF(cx, cy);
      vertices[1] = points[0];
      vertices[2] = points[1];
      for (int i = 2; i < points.Length; ++i)
      {
        vertices[i + 1] = points[i];
      }
      vertices[verticeCount - 1] = points[0];
      return vertices;
    }

  }
}
