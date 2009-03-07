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

using System.Drawing;
using MediaPortal.Core;
using MediaPortal.Core.Logging;
using MediaPortal.Presentation.DataObjects;
using MediaPortal.Presentation.Geometries;
using MediaPortal.SkinEngine.ContentManagement;
using MediaPortal.SkinEngine.Controls.Visuals;
using MediaPortal.SkinEngine.Effects;
using MediaPortal.SkinEngine;
using MediaPortal.SkinEngine.DirectX;
using MediaPortal.SkinEngine.Players;
using SlimDX.Direct3D9;
using MediaPortal.Presentation.Players;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.SkinEngine.SkinManagement;

namespace MediaPortal.SkinEngine.Controls.Brushes
{
  public class VideoBrush : Brush
  {
    #region Private fields

    Property _streamProperty;
    EffectAsset _effect;
    Size _videoSize;
    Size _videoAspectRatio;
    IGeometry _previousGeometry;
    PositionColored2Textured[] _verts;

    #endregion

    #region Ctor

    public VideoBrush()
    {
      Init();
    }

    void Init()
    {
      _streamProperty = new Property(typeof(int), 0);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      VideoBrush b = (VideoBrush) source;
      Stream = copyManager.GetCopy(b.Stream);
    }

    #endregion

    #region Public properties

    public Property StreamProperty
    {
      get { return _streamProperty; }
    }

    /// <summary>
    /// Gets or sets the video stream number
    /// </summary>
    public int Stream
    {
      get { return (int)_streamProperty.GetValue(); }
      set { _streamProperty.SetValue(value); }
    }

    #endregion

    public override void SetupBrush(FrameworkElement element, ref PositionColored2Textured[] verts)
    {
      UpdateBounds(element, ref verts);
      base.SetupBrush(element, ref verts);
      _effect = ContentManager.GetEffect("normal");
      _verts = verts;
      _videoSize = new Size(0, 0);
      _videoAspectRatio = new Size(0, 0);
    }

    void UpdateVertexBuffer(IVideoPlayer player, VertexBuffer vertexBuffer)
    {
      Size size = player.VideoSize;
      Size aspectRatio = player.VideoAspectRatio;
      if (size == _videoSize && aspectRatio == _videoAspectRatio)
      {
        if (ServiceScope.Get<IGeometryManager>().CurrentVideoGeometry == _previousGeometry)
          return;
      }

      _videoSize = size;
      _videoAspectRatio = aspectRatio;
      IGeometryManager geometryManager = ServiceScope.Get<IGeometryManager>();
      IGeometry geometry = geometryManager.CurrentVideoGeometry;
      _previousGeometry = geometry;
      Rectangle sourceRect;
      Rectangle destinationRect;
      GeometryData gd = new GeometryData(
          new Size(_videoSize.Width, _videoSize.Height), new Size((int) _bounds.Width, (int) _bounds.Height), 1.0f);
      geometryManager.Transform(gd, out sourceRect, out destinationRect);
      string shaderName = geometry.Shader;
      _effect = !string.IsNullOrEmpty(shaderName) ? ContentManager.GetEffect(shaderName) :
          ContentManager.GetEffect("normal");

      float minU = ((float)(sourceRect.X)) / ((float)_videoSize.Width);
      float minV = ((float)(sourceRect.Y)) / ((float)_videoSize.Height);
      float maxU = ((float)(sourceRect.Width)) / ((float)_videoSize.Width);
      float maxV = ((float)(sourceRect.Height)) / ((float)_videoSize.Height);

      float minX = ((float)(destinationRect.X)) / ((float)_bounds.Width);
      float minY = ((float)(destinationRect.Y)) / ((float)_bounds.Height);

      float maxX = ((float)(destinationRect.Width)) / ((float)_bounds.Width);
      float maxY = ((float)(destinationRect.Height)) / ((float)_bounds.Height);

      float diffU = maxU - minU;
      float diffV = maxV - minV;
      PositionColored2Textured[] verts = new PositionColored2Textured[_verts.Length];
      for (int i = 0; i < _verts.Length; ++i)
      {
        float x = ((_verts[i].X - _minPosition.X) / (_bounds.Width)) * maxX + minX;
        float y = ((_verts[i].Y - _minPosition.Y) / (_bounds.Height)) * maxY + minY;
        verts[i].X = (x * _bounds.Width) + _minPosition.X;
        verts[i].Y = (y * _bounds.Height) + _minPosition.Y;

        float u = _verts[i].Tu1 * diffU + minU;
        float v = _verts[i].Tv1 * diffV + minV;
        verts[i].Tu1 = u;
        verts[i].Tv1 = v;
        verts[i].Color = _verts[i].Color;
        verts[i].Z = SkinContext.GetZorder();
      }
      PositionColored2Textured.Set(vertexBuffer, ref verts);
    }

    public override bool BeginRender(VertexBuffer vertexBuffer, int primitiveCount, PrimitiveType primitiveType)
    {
      IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>(false);
      if (playerManager == null)
      {
        ServiceScope.Get<ILogger>().Debug("VideoBrush.BeginRender: Player collection not found");
        return false;
      }
      ISlimDXVideoPlayer player = playerManager[Stream] as ISlimDXVideoPlayer;
      if (player == null) return false;

      if (Transform != null)
      {
        ExtendedMatrix mTrans;
        Transform.GetTransform(out mTrans);
        SkinContext.AddTransform(mTrans);
      }

      UpdateVertexBuffer(player, vertexBuffer);
      player.BeginRender(_effect);
      return true;
    }

    public override void EndRender()
    {
      IPlayerManager playerManager = ServiceScope.Get<IPlayerManager>(false);
      if (playerManager == null)
      {
        ServiceScope.Get<ILogger>().Debug("VideoBrush.BeginRender: Player collection not found");
        return;
      }
      ISlimDXVideoPlayer player = playerManager[Stream] as ISlimDXVideoPlayer;
      if (player == null) return;
      player.EndRender(_effect);
      if (Transform != null)
      {
        SkinContext.RemoveTransform();
      }
    }
  }
}
