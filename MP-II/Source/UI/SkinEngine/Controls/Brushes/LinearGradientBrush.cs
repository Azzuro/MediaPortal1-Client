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
using SlimDX;
using SlimDX.Direct3D9;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.UI.SkinEngine.SkinManagement;

namespace MediaPortal.UI.SkinEngine.Controls.Brushes
{
  public class LinearGradientBrush : GradientBrush, IAsset
  {
    #region Private fields

    Texture _cacheTexture;
    double _height;
    double _width;
    Vector3 _position;
    EffectAsset _effect;
    DateTime _lastTimeUsed;

    AbstractProperty _startPointProperty;
    AbstractProperty _endPointProperty;
    bool _refresh = false;
    bool _singleColor = true;
    EffectHandleAsset _handleRelativeTransform;
    EffectHandleAsset _handleOpacity;
    EffectHandleAsset _handleStartPoint;
    EffectHandleAsset _handleEndPoint;
    EffectHandleAsset _handleSolidColor;
    EffectHandleAsset _handleAlphaTexture;
    BrushTexture _brushTexture;

    #endregion

    #region Ctor

    public LinearGradientBrush()
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
      _startPointProperty = new SProperty(typeof(Vector2), new Vector2(0.0f, 0.0f));
      _endPointProperty = new SProperty(typeof(Vector2), new Vector2(1.0f, 1.0f));
    }

    void Attach()
    {
      _startPointProperty.Attach(OnPropertyChanged);
      _endPointProperty.Attach(OnPropertyChanged);
    }

    void Detach()
    {
      _startPointProperty.Detach(OnPropertyChanged);
      _endPointProperty.Detach(OnPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      LinearGradientBrush b = (LinearGradientBrush) source;
      StartPoint = copyManager.GetCopy(b.StartPoint);
      EndPoint = copyManager.GetCopy(b.EndPoint);
      Attach();
    }

    #endregion

    protected override void OnPropertyChanged(AbstractProperty prop, object oldValue)
    {
      _refresh = true;
      FireChanged();
    }

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

    public AbstractProperty StartPointProperty
    {
      get { return _startPointProperty; }
    }

    public Vector2 StartPoint
    {
      get { return (Vector2)_startPointProperty.GetValue(); }
      set { _startPointProperty.SetValue(value); }
    }

    public AbstractProperty EndPointProperty
    {
      get { return _endPointProperty; }
    }

    public Vector2 EndPoint
    {
      get { return (Vector2)_endPointProperty.GetValue(); }
      set { _endPointProperty.SetValue(value); }
    }

    public override void SetupBrush(RectangleF bounds, ExtendedMatrix layoutTransform, float zOrder, ref PositionColored2Textured[] verts)
    {
      _verts = verts;
      // if (_texture == null || element.ActualHeight != _height || element.ActualWidth != _width)
      {
        UpdateBounds(bounds, layoutTransform, ref verts);
        if (!IsOpacityBrush)
          base.SetupBrush(bounds, layoutTransform, zOrder, ref verts);

        _height = bounds.Height;
        _width = bounds.Width;
        _position = new Vector3(bounds.X, bounds.Y, zOrder);
        if (_brushTexture == null)
          _brushTexture = BrushCache.Instance.GetGradientBrush(GradientStops, IsOpacityBrush);
        if (_cacheTexture != null)
          Free(true);
        _refresh = true;
      }
    }

    public override bool BeginRender(VertexBuffer vertexBuffer, int primitiveCount, PrimitiveType primitiveType)
    {
      if (Transform != null)
      {
        ExtendedMatrix mTrans;
        Transform.GetTransform(out mTrans);
        SkinContext.AddRenderTransform(mTrans);
      }
      if (_brushTexture == null)
        return false;
      if (_refresh)
      {
        _refresh = false;
        CheckSingleColor();
        _brushTexture = BrushCache.Instance.GetGradientBrush(GradientStops, IsOpacityBrush);
        if (_singleColor)
        {
          SetColor(vertexBuffer);
          _effect = ContentManager.GetEffect("solidbrush");
          _handleSolidColor = _effect.GetParameterHandle("g_solidColor");
        }
        else
        {
          _effect = ContentManager.GetEffect("lineargradient");
          _handleRelativeTransform = _effect.GetParameterHandle("RelativeTransform");
          _handleOpacity = _effect.GetParameterHandle("g_opacity");
          _handleStartPoint = _effect.GetParameterHandle("g_StartPoint");
          _handleEndPoint = _effect.GetParameterHandle("g_EndPoint");
        }
        if (_cacheTexture != null)
          Free(true);
      }

      float[] g_startpoint = new float[] { StartPoint.X, StartPoint.Y };
      float[] g_endpoint = new float[] { EndPoint.X, EndPoint.Y };
      if (MappingMode == BrushMappingMode.Absolute)
      {
        g_startpoint[0] = ((StartPoint.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _bounds.Width;
        g_startpoint[1] = ((StartPoint.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _bounds.Height;

        g_endpoint[0] = ((EndPoint.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _bounds.Width;
        g_endpoint[1] = ((EndPoint.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _bounds.Height;
      }
      //GraphicsDevice.TransformWorld = SkinContext.FinalMatrix.Matrix;
      if (!_singleColor)
      {
        if (Freezable)
        {
          if (_cacheTexture == null)
          {
            Trace.WriteLine("LinearGradientBrush:Create cached texture");
            _effect = ContentManager.GetEffect("lineargradient");
            _handleRelativeTransform = _effect.GetParameterHandle("RelativeTransform");
            _handleOpacity = _effect.GetParameterHandle("g_opacity");
            _handleStartPoint = _effect.GetParameterHandle("g_StartPoint");
            _handleEndPoint = _effect.GetParameterHandle("g_EndPoint");

            Trace.WriteLine("LinearGradientBrush:Create cached texture");
            float w = (float)_width;
            float h = (float)_height;
            float cx = GraphicsDevice.Width / (float) SkinContext.SkinWidth;
            float cy = GraphicsDevice.Height / (float) SkinContext.SkinHeight;

            bool copy = true;
            if ((int)w == SkinContext.SkinWidth && (int)h == SkinContext.SkinHeight)
            {
              copy = false;
              w /= 2;
              h /= 2;
            }
            ExtendedMatrix m = new ExtendedMatrix();
            m.Matrix *= SkinContext.FinalTransform.Matrix;
            //next put the control at position (0,0,0)
            //and scale it correctly since the backbuffer now has the dimensions of the control
            //instead of the skin width/height dimensions
            m.Matrix *= Matrix.Translation(new Vector3(-(_position.X + 1), -(_position.Y + 1), 0));
            m.Matrix *= Matrix.Scaling(((SkinContext.SkinWidth) * cx) / w, (SkinContext.SkinHeight * cy) / h, 1.0f);

            SkinContext.AddRenderTransform(m);

            GraphicsDevice.Device.EndScene();
            _cacheTexture = new Texture(GraphicsDevice.Device, (int)w, (int)h, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);
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

                //render the control (will be rendered into the opacitytexture)
                GraphicsDevice.Device.BeginScene();
                //GraphicsDevice.Device.VertexFormat = PositionColored2Textured.Format;
                //GraphicsDevice.TransformWorld = SkinContext.FinalMatrix.Matrix;

                Matrix mrel;
                RelativeTransform.GetTransformRel(out mrel);
                mrel = Matrix.Invert(mrel);
                _handleRelativeTransform.SetParameter(mrel);
                _handleOpacity.SetParameter((float)(Opacity * SkinContext.Opacity));
                _handleStartPoint.SetParameter(g_startpoint);
                _handleEndPoint.SetParameter(g_endpoint);
                _effect.StartRender(_brushTexture.Texture);

                GraphicsDevice.Device.SetStreamSource(0, vertexBuffer, 0, PositionColored2Textured.StrideSize);
                GraphicsDevice.Device.DrawPrimitives(primitiveType, 0, primitiveCount);

                _effect.EndRender();

                GraphicsDevice.Device.EndScene();
                SkinContext.RemoveRenderTransform();

                //restore the backbuffer
                GraphicsDevice.Device.SetRenderTarget(0, backBuffer);
                _effect = ContentManager.GetEffect("normal");
              }

              ContentManager.Add(this);
            }
            GraphicsDevice.Device.BeginScene();
          }
          _effect.StartRender(_cacheTexture);
          //GraphicsDevice.Device.SetTexture(0, _cacheTexture);
          _lastTimeUsed = SkinContext.Now;
        }
        else
        {
          Matrix m;
          RelativeTransform.GetTransformRel(out m);
          m = Matrix.Invert(m);

          _handleRelativeTransform.SetParameter(m);
          _handleOpacity.SetParameter((float)(Opacity * SkinContext.Opacity));
          _handleStartPoint.SetParameter(g_startpoint);
          _handleEndPoint.SetParameter(g_endpoint);
          _effect.StartRender(_brushTexture.Texture);
          _lastTimeUsed = SkinContext.Now;
        }
      }
      else
      {
        Color4 v = ColorConverter.FromColor(GradientStops.OrderedGradientStopList[0].Color);
        _handleSolidColor.SetParameter(v);
        _effect.StartRender(null);
        _lastTimeUsed = SkinContext.Now;
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
        _brushTexture = BrushCache.Instance.GetGradientBrush(GradientStops, IsOpacityBrush);
        if (_singleColor)
        {
          //SetColor(vertexBuffer);
        }
        _effect = ContentManager.GetEffect("linearopacitygradient");
        _handleRelativeTransform = _effect.GetParameterHandle("RelativeTransform");
        _handleOpacity = _effect.GetParameterHandle("g_opacity");
        _handleStartPoint = _effect.GetParameterHandle("g_StartPoint");
        _handleEndPoint = _effect.GetParameterHandle("g_EndPoint");
        _handleAlphaTexture = _effect.GetParameterHandle("g_alphatex");
      }

      float[] g_startpoint = new float[] { StartPoint.X, StartPoint.Y };
      float[] g_endpoint = new float[] { EndPoint.X, EndPoint.Y };
      if (MappingMode == BrushMappingMode.Absolute)
      {
        g_startpoint[0] = ((StartPoint.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _bounds.Width;
        g_startpoint[1] = ((StartPoint.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _bounds.Height;

        g_endpoint[0] = ((EndPoint.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _bounds.Width;
        g_endpoint[1] = ((EndPoint.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _bounds.Height;
      }

      //GraphicsDevice.TransformWorld = SkinContext.FinalMatrix.Matrix;
      if (!_singleColor)
      {
        Matrix m;
        RelativeTransform.GetTransformRel(out m);
        m = Matrix.Invert(m);
        _handleRelativeTransform.SetParameter(m);
        _handleOpacity.SetParameter((float)(Opacity * SkinContext.Opacity));
        _handleStartPoint.SetParameter(g_startpoint);
        _handleEndPoint.SetParameter(g_endpoint);
        _handleAlphaTexture.SetParameter(_brushTexture.Texture);

        _effect.StartRender(tex);
        _lastTimeUsed = SkinContext.Now;
      }
      else
      {
        _effect.StartRender(null);
        _lastTimeUsed = SkinContext.Now;
      }
    }

    public override void EndRender()
    {
      if (_effect != null)
        _effect.EndRender();
      if (Transform != null)
        SkinContext.RemoveRenderTransform();
    }

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
        TimeSpan ts = SkinContext.Now - _lastTimeUsed;
        if (ts.TotalSeconds >= 1)
          return true;

        return false;
      }
    }

    public bool Free(bool force)
    {
      if (_cacheTexture != null)
      {
        Trace.WriteLine("LinearGradientBrush: Free cached texture");
        _cacheTexture.Dispose();
        _cacheTexture = null;
        return true;
      }
      return false;
    }

    #endregion

    public override Texture Texture
    {
      get { return _brushTexture.Texture; }
    }

    public override void Deallocate()
    {
      if (_cacheTexture != null)
      {
        _cacheTexture.Dispose();
        _cacheTexture = null;
        ContentManager.Remove(this);
      }
    }

    public override void Allocate()
    {
    }

    public override void SetupPrimitive(Rendering.PrimitiveContext context)
    {
      context.Parameters = new EffectParameters();
      CheckSingleColor();
      _brushTexture = BrushCache.Instance.GetGradientBrush(GradientStops, IsOpacityBrush);
      if (_singleColor)
      {
        Color4 v = ColorConverter.FromColor(GradientStops.OrderedGradientStopList[0].Color);
        v.Alpha *= (float)SkinContext.Opacity;
        context.Effect = ContentManager.GetEffect("solidbrush");
        context.Parameters.Add(context.Effect.GetParameterHandle("g_solidColor"), v);
        return;
      }
      else
      {
        context.Effect = ContentManager.GetEffect("lineargradient");
        _handleRelativeTransform = context.Effect.GetParameterHandle("RelativeTransform");
        _handleOpacity = context.Effect.GetParameterHandle("g_opacity");
        _handleStartPoint = context.Effect.GetParameterHandle("g_StartPoint");
        _handleEndPoint = context.Effect.GetParameterHandle("g_EndPoint");
      }

      float[] g_startpoint = new float[] { StartPoint.X, StartPoint.Y };
      float[] g_endpoint = new float[] { EndPoint.X, EndPoint.Y };
      if (MappingMode == BrushMappingMode.Absolute)
      {
        g_startpoint[0] = ((StartPoint.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _bounds.Width;
        g_startpoint[1] = ((StartPoint.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _bounds.Height;

        g_endpoint[0] = ((EndPoint.X * SkinContext.Zoom.Width) - (_minPosition.X - _orginalPosition.X)) / _bounds.Width;
        g_endpoint[1] = ((EndPoint.Y * SkinContext.Zoom.Height) - (_minPosition.Y - _orginalPosition.Y)) / _bounds.Height;
      }

      Matrix m;
      RelativeTransform.GetTransformRel(out m);
      m = Matrix.Invert(m);

      context.Parameters.Add(_handleRelativeTransform, m);
      context.Parameters.Add(_handleOpacity, (float)(Opacity * SkinContext.Opacity));
      context.Parameters.Add(_handleStartPoint, g_startpoint);
      context.Parameters.Add(_handleEndPoint, g_endpoint);
      context.Texture = _brushTexture;
    }
  }
}
