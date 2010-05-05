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
using System.Diagnostics;
using System.Drawing;
using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.ContentManagement;
using MediaPortal.UI.SkinEngine.Effects;
using MediaPortal.UI.SkinEngine.DirectX;
using MediaPortal.UI.SkinEngine.Rendering;
using SlimDX;
using SlimDX.Direct3D9;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.UI.SkinEngine.SkinManagement;

namespace MediaPortal.UI.SkinEngine.Controls.Brushes
{
  public class RadialGradientBrush : GradientBrush, IAsset
  {
    #region Private fields

    Texture _cacheTexture;
    double _height;
    double _width;
    Vector3 _position;
    EffectAsset _effect;
    DateTime _lastTimeUsed;

    AbstractProperty _centerProperty;
    AbstractProperty _gradientOriginProperty;
    AbstractProperty _radiusXProperty;
    AbstractProperty _radiusYProperty;
    bool _refresh = false;
    bool _singleColor = true;
    EffectHandleAsset _handleRelativeTransform;
    EffectHandleAsset _handleFocus;
    EffectHandleAsset _handleCenter;
    EffectHandleAsset _handleRadius;
    EffectHandleAsset _handleOpacity;
    EffectHandleAsset _handleColor;
    EffectHandleAsset _handleAlphaTexture;
    GradientBrushTexture _gradientBrushTexture;
    float[] g_focus;
    float[] g_center;
    float[] g_radius;

    #endregion

    #region Ctor

    public RadialGradientBrush()
    {
      Init();
      Attach();
    }

    public override void Dispose()
    {
      base.Dispose();
      Detach();
      Free(false);
    }

    void Init()
    {
      _centerProperty = new SProperty(typeof(Vector2), new Vector2(0.5f, 0.5f));
      _gradientOriginProperty = new SProperty(typeof(Vector2), new Vector2(0.5f, 0.5f));
      _radiusXProperty = new SProperty(typeof(double), 0.5);
      _radiusYProperty = new SProperty(typeof(double), 0.5);
    }

    void Attach()
    {
      _centerProperty.Attach(OnPropertyChanged);
      _gradientOriginProperty.Attach(OnPropertyChanged);
      _radiusXProperty.Attach(OnPropertyChanged);
      _radiusYProperty.Attach(OnPropertyChanged);
    }

    void Detach()
    {
      _centerProperty.Detach(OnPropertyChanged);
      _gradientOriginProperty.Detach(OnPropertyChanged);
      _radiusXProperty.Detach(OnPropertyChanged);
      _radiusYProperty.Detach(OnPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      RadialGradientBrush b = (RadialGradientBrush) source;
      Center = copyManager.GetCopy(b.Center);
      GradientOrigin = copyManager.GetCopy(b.GradientOrigin);
      RadiusX = b.RadiusX;
      RadiusY = b.RadiusY;
      Attach();
    }

    #endregion

    #region Protected methods

    protected override void OnPropertyChanged(AbstractProperty prop, object oldValue)
    {
      _refresh = true;
      FireChanged();
    }

    #endregion

    #region Public properties

    public AbstractProperty CenterProperty
    {
      get { return _centerProperty; }
    }

    public Vector2 Center
    {
      get { return (Vector2)_centerProperty.GetValue(); }
      set { _centerProperty.SetValue(value); }
    }

    public AbstractProperty GradientOriginProperty
    {
      get { return _gradientOriginProperty; }
    }

    public Vector2 GradientOrigin
    {
      get { return (Vector2)_gradientOriginProperty.GetValue(); }
      set { _gradientOriginProperty.SetValue(value); }
    }

    public AbstractProperty RadiusXProperty
    {
      get { return _radiusXProperty; }
    }

    public double RadiusX
    {
      get { return (double)_radiusXProperty.GetValue(); }
      set { _radiusXProperty.SetValue(value); }
    }

    public AbstractProperty RadiusYProperty
    {
      get { return _radiusYProperty; }
    }

    public double RadiusY
    {
      get { return (double)_radiusYProperty.GetValue(); }
      set { _radiusYProperty.SetValue(value); }
    }

    public override Texture Texture
    {
      get { return _gradientBrushTexture.Texture; }
    }

    #endregion

    protected void CheckSingleColor()
    {
      int color = -1;
      _singleColor = true;
      foreach (GradientStop stop in GradientStops)
        if (color == -1)
          color = stop.Color.ToArgb();
        else
          if (color != stop.Color.ToArgb())
          {
            _singleColor = false;
            return;
          }
    }

    #region Public methods

    public override void SetupBrush(RectangleF bounds, ExtendedMatrix layoutTransform, float zOrder, PositionColored2Textured[] verts)
    {
      UpdateBounds(bounds, layoutTransform, verts);
      base.SetupBrush(bounds, layoutTransform, zOrder, verts);

      _height = bounds.Height;
      _width = bounds.Width;
      _position = new Vector3(bounds.X, bounds.Y, zOrder);

      if (_gradientBrushTexture == null)
        _gradientBrushTexture = BrushCache.Instance.GetGradientBrush(GradientStops);
      Free(true);
      _refresh = true;
    }

    public override bool BeginRender(PrimitiveContext primitiveContext)
    {
      if (Transform != null)
      {
        ExtendedMatrix mTrans;
        Transform.GetTransform(out mTrans);
        SkinContext.AddRenderTransform(mTrans);
      }
      if (_gradientBrushTexture == null) return false;
      if (_refresh)
      {
        _refresh = false;
        CheckSingleColor();
        _gradientBrushTexture = BrushCache.Instance.GetGradientBrush(GradientStops);
        if (_singleColor)
        {
          _effect = ContentManager.GetEffect("solidbrush");
          _handleColor = _effect.GetParameterHandle("g_solidColor");
        }
        else
        {
          _effect = ContentManager.GetEffect("radialgradient");
          _handleRelativeTransform = _effect.GetParameterHandle("RelativeTransform");
          _handleFocus = _effect.GetParameterHandle("g_focus");
          _handleCenter = _effect.GetParameterHandle("g_center");
          _handleRadius = _effect.GetParameterHandle("g_radius");
          _handleOpacity = _effect.GetParameterHandle("g_opacity");
        }
        Free(true);
        g_focus = new float[] { GradientOrigin.X, GradientOrigin.Y };
        g_center = new float[] { Center.X, Center.Y };
        g_radius = new float[] { (float) RadiusX, (float) RadiusY };

        if (MappingMode == BrushMappingMode.Absolute)
        {
          g_focus[0] = ((GradientOrigin.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _vertsBounds.Width;
          g_focus[1] = ((GradientOrigin.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _vertsBounds.Height;

          g_center[0] = ((Center.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _vertsBounds.Width;
          g_center[1] = ((Center.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _vertsBounds.Height;

          g_radius[0] = (float)((RadiusX * SkinContext.Zoom.Width) / _vertsBounds.Width);
          g_radius[1] = (float)((RadiusY * SkinContext.Zoom.Height) / _vertsBounds.Height);
        }
      }

      if (_singleColor)
      {
        Color4 v = ColorConverter.FromColor(GradientStops[0].Color);
        _handleColor.SetParameter(v);
        _effect.StartRender(null);
        _lastTimeUsed = SkinContext.FrameRenderingStartTime;
      }
      else
      {
        if (Freezable)
        {
          if (_cacheTexture == null)
          {
            Trace.WriteLine("RadialGradientBrush:Create cached texture");
            _effect = ContentManager.GetEffect("radialgradient");
            _handleRelativeTransform = _effect.GetParameterHandle("RelativeTransform");
            _handleFocus = _effect.GetParameterHandle("g_focus");
            _handleCenter = _effect.GetParameterHandle("g_center");
            _handleRadius = _effect.GetParameterHandle("g_radius");
            _handleOpacity = _effect.GetParameterHandle("g_opacity");

            float w = (float) _width;
            float h = (float) _height;
            float cx = 1.0f;// GraphicsDevice.Width / (float) SkinContext.SkinWidth;
            float cy = 1.0f;// GraphicsDevice.Height / (float) SkinContext.SkinHeight;

            bool copy = true;
            if ((int) w >= SkinContext.SkinResources.SkinWidth && (int) h >= SkinContext.SkinResources.SkinHeight)
            {
              copy = false;
              w /= 2;
              h /= 2;
            }
            ExtendedMatrix m = new ExtendedMatrix();
            m.Matrix *= SkinContext.FinalRenderTransform.Matrix;
            //next put the control at position (0,0,0)
            //and scale it correctly since the backbuffer now has the dimensions of the control
            //instead of the skin width/height dimensions
            m.Matrix *= Matrix.Translation(new Vector3(-(_position.X + 1), -(_position.Y + 1), 0));
            m.Matrix *= Matrix.Scaling((GraphicsDevice.Width * cx) / w, (GraphicsDevice.Height * cy) / h, 1.0f);

            SkinContext.AddRenderTransform(m);

            GraphicsDevice.Device.EndScene();

            _cacheTexture = new Texture(GraphicsDevice.Device, (int) w, (int) h, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);
            //get the current backbuffer
            using (Surface backBuffer = GraphicsDevice.Device.GetRenderTarget(0))
            {
              //get the surface of our opacity texture
              using (Surface cacheSurface = _cacheTexture.GetSurfaceLevel(0))
              {
                if (copy)
                {
                  //copy the correct rectangle from the backbuffer in the opacitytexture
                  GraphicsDevice.Device.StretchRectangle(backBuffer,
                      new Rectangle((int) (_position.X * cx), (int) (_position.Y * cy),
                          (int) (_width * cx), (int) (_height * cy)),
                      cacheSurface, new Rectangle(0, 0, (int) w, (int) h), TextureFilter.None);
                }
                //change the rendertarget to the opacitytexture
                GraphicsDevice.Device.SetRenderTarget(0, cacheSurface);

                // Render the control (will be rendered into the opacitytexture)
                GraphicsDevice.Device.BeginScene();

                Matrix mrel;
                RelativeTransform.GetTransformRel(out mrel);
                mrel = Matrix.Invert(mrel);
                _handleRelativeTransform.SetParameter(mrel);
                _handleFocus.SetParameter(g_focus);
                _handleCenter.SetParameter(g_center);
                _handleRadius.SetParameter(g_radius);
                _handleOpacity.SetParameter((float) (Opacity * SkinContext.Opacity));

                _effect.StartRender(_gradientBrushTexture.Texture);

                GraphicsDevice.Device.VertexFormat = primitiveContext.VertexFormat;
                GraphicsDevice.Device.SetStreamSource(0, primitiveContext.VertexBuffer, 0, primitiveContext.StrideSize);
                GraphicsDevice.Device.DrawPrimitives(primitiveContext.PrimitiveType, 0, primitiveContext.NumVertices);

                _effect.EndRender();

                GraphicsDevice.Device.EndScene();
                SkinContext.RemoveRenderTransform();

                // Restore the backbuffer
                GraphicsDevice.Device.SetRenderTarget(0, backBuffer);
              }
              //TextureLoader.Save(@"C:\1\1.png", ImageFileFormat.Png, _cacheTexture);
              _effect = ContentManager.GetEffect("normal");
              ContentManager.Add(this);
            }
            GraphicsDevice.Device.BeginScene();
          }
          _effect.StartRender(_cacheTexture);
          _lastTimeUsed = SkinContext.FrameRenderingStartTime;
        }
        else
        {
          Matrix m;
          RelativeTransform.GetTransformRel(out m);
          m = Matrix.Invert(m);

          _handleRelativeTransform.SetParameter(m);
          _handleFocus.SetParameter(g_focus);
          _handleCenter.SetParameter(g_center);
          _handleRadius.SetParameter(g_radius);
          _handleOpacity.SetParameter((float) (Opacity * SkinContext.Opacity));

          _effect.StartRender(_gradientBrushTexture.Texture);
          _lastTimeUsed = SkinContext.FrameRenderingStartTime;
        }
      }
      return true;
    }

    public override void BeginRender(Texture tex)
    {
      if (Transform != null)
      {
        ExtendedMatrix mTrans;
        Transform.GetTransform(out mTrans);
        SkinContext.AddRenderTransform(mTrans);
      }
      if (tex == null)
        return;
      if (_refresh)
      {
        _refresh = false;
        CheckSingleColor();
        _gradientBrushTexture = BrushCache.Instance.GetGradientBrush(GradientStops);
        _effect = ContentManager.GetEffect("radialopacitygradient");
        _handleRelativeTransform = _effect.GetParameterHandle("RelativeTransform");
        _handleFocus = _effect.GetParameterHandle("g_focus");
        _handleCenter = _effect.GetParameterHandle("g_center");
        _handleRadius = _effect.GetParameterHandle("g_radius");
        _handleOpacity = _effect.GetParameterHandle("g_opacity");
        _handleAlphaTexture = _effect.GetParameterHandle("g_alphatex");

        g_focus = new float[] { GradientOrigin.X, GradientOrigin.Y };
        g_center = new float[] { Center.X, Center.Y };
        g_radius = new float[] { (float) RadiusX, (float) RadiusY };

        if (MappingMode == BrushMappingMode.Absolute)
        {
          g_focus[0] = ((GradientOrigin.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _vertsBounds.Width;
          g_focus[1] = ((GradientOrigin.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _vertsBounds.Height;

          g_center[0] = ((Center.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _vertsBounds.Width;
          g_center[1] = ((Center.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _vertsBounds.Height;

          g_radius[0] = (float) ((RadiusX * SkinContext.Zoom.Width) / _vertsBounds.Width);
          g_radius[1] = (float) ((RadiusY * SkinContext.Zoom.Height) / _vertsBounds.Height);
        }
      }

      if (_singleColor)
      {
        _effect.StartRender(null);
        _lastTimeUsed = SkinContext.FrameRenderingStartTime;
      }
      else
      {
        Matrix m;
        RelativeTransform.GetTransformRel(out m);
        m = Matrix.Invert(m);

        _handleRelativeTransform.SetParameter(m);
        _handleFocus.SetParameter(g_focus);
        _handleCenter.SetParameter(g_center);
        _handleRadius.SetParameter(g_radius);
        _handleOpacity.SetParameter((float) (Opacity * SkinContext.Opacity));
        _handleAlphaTexture.SetParameter(_gradientBrushTexture.Texture);
        _effect.StartRender(tex);
        _lastTimeUsed = SkinContext.FrameRenderingStartTime;
      }
    }

    public override void EndRender()
    {
      if (_effect != null)
        _effect.EndRender();
      if (Transform != null)
        SkinContext.RemoveRenderTransform();
    }

    #endregion

    #region IAsset Members

    public bool IsAllocated
    {
      get { return (_cacheTexture != null); }
    }

    public bool CanBeDeleted
    {
      get
      {
        if (!IsAllocated)
          return false;
        TimeSpan ts = SkinContext.FrameRenderingStartTime - _lastTimeUsed;
        if (ts.TotalSeconds >= 1)
          return true;

        return false;
      }
    }

    public bool Free(bool force)
    {
      if (_cacheTexture != null)
      {
        Trace.WriteLine("RadialGradientBrush: Free cached texture");
        _cacheTexture.Dispose();
        _cacheTexture = null;
        return true;
      }
      return false;
    }

    #endregion

    public override void Deallocate()
    {
      if (_cacheTexture != null)
      {
        Trace.WriteLine("RadialGradientBrush: Deallocate cached texture");
        _cacheTexture.Dispose();
        _cacheTexture = null;
        ContentManager.Remove(this);
      }
    }

    public override void Allocate()
    { }

    public override void SetupPrimitive(PrimitiveContext context)
    {
      context.Parameters = new EffectParameters();
      CheckSingleColor();
      context.Texture = BrushCache.Instance.GetGradientBrush(GradientStops);
      if (_singleColor)
      {

        Color4 v = ColorConverter.FromColor(GradientStops[0].Color);
        v.Alpha *= (float) SkinContext.Opacity;
        context.Effect = ContentManager.GetEffect("solidbrush");
        context.Parameters.Add(context.Effect.GetParameterHandle("g_solidColor"), v);
        return;
      }
      else
      {
        context.Effect = ContentManager.GetEffect("radialgradient");
        _handleRelativeTransform = context.Effect.GetParameterHandle("RelativeTransform");
        _handleFocus = context.Effect.GetParameterHandle("g_focus");
        _handleCenter = context.Effect.GetParameterHandle("g_center");
        _handleRadius = context.Effect.GetParameterHandle("g_radius");
        _handleOpacity = context.Effect.GetParameterHandle("g_opacity");

        g_focus = new float[] { GradientOrigin.X, GradientOrigin.Y };
        g_center = new float[] { Center.X, Center.Y };
        g_radius = new float[] { (float)RadiusX, (float)RadiusY };

        if (MappingMode == BrushMappingMode.Absolute)
        {
          g_focus[0] = ((GradientOrigin.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _vertsBounds.Width;
          g_focus[1] = ((GradientOrigin.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _vertsBounds.Height;

          g_center[0] = ((Center.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _vertsBounds.Width;
          g_center[1] = ((Center.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _vertsBounds.Height;

          g_radius[0] = (float)((RadiusX * SkinContext.Zoom.Width) / _vertsBounds.Width);
          g_radius[1] = (float)((RadiusY * SkinContext.Zoom.Height) / _vertsBounds.Height);
        }
        Matrix mrel;
        RelativeTransform.GetTransformRel(out mrel);
        mrel = Matrix.Invert(mrel);
        context.Parameters.Add(_handleRelativeTransform, mrel);
        context.Parameters.Add(_handleFocus, g_focus);
        context.Parameters.Add(_handleCenter, g_center);
        context.Parameters.Add(_handleRadius, g_radius);
        context.Parameters.Add(_handleOpacity, (float) (Opacity * SkinContext.Opacity));
      }
    }
  }
}
