#region Copyright (C) 2007-2009 Team MediaPortal

/*
    Copyright (C) 2007-2009 Team MediaPortal
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

using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.ContentManagement;
using MediaPortal.UI.SkinEngine.Effects;
using MediaPortal.UI.SkinEngine.DirectX;
using MediaPortal.UI.SkinEngine.Rendering;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.UI.SkinEngine.SkinManagement;

namespace MediaPortal.UI.SkinEngine.Controls.Brushes
{
  public class SolidColorBrush : Brush
  {
    #region Private properties

    AbstractProperty _colorProperty;
    EffectAsset _effect;
    EffectHandleAsset _effectHandleColor;

    #endregion

    #region Ctor

    public SolidColorBrush()
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
      _colorProperty = new SProperty(typeof(Color), Color.White);
    }

    void Attach()
    {
      _colorProperty.Attach(OnPropertyChanged);
    }

    void Detach()
    {
      _colorProperty.Detach(OnPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      SolidColorBrush b = (SolidColorBrush) source;
      Color = copyManager.GetCopy(b.Color);
      Attach();
    }

    #endregion

    protected override void OnPropertyChanged(AbstractProperty prop, object oldValue)
    {
      FireChanged();
    }

    public AbstractProperty ColorProperty
    {
      get { return _colorProperty; }
    }

    public Color Color
    {
      get { return (Color) _colorProperty.GetValue(); }
      set { _colorProperty.SetValue(value); }
    }

    public override void SetupBrush(RectangleF bounds, ExtendedMatrix layoutTransform, float zOrder, ref PositionColored2Textured[] verts)
    {
      UpdateBounds(bounds, layoutTransform, ref verts);
      base.SetupBrush(bounds, layoutTransform, zOrder, ref verts);
      _effect = ContentManager.GetEffect("solidbrush");
      _effectHandleColor = _effect.GetParameterHandle("g_solidColor");
      Color4 color = ColorConverter.FromColor(Color);
      color.Alpha *= (float) Opacity;
      for (int i = 0; i < verts.Length; ++i)
        verts[i].Color = color.ToArgb();
    }

    public override bool BeginRender(VertexBuffer vertexBuffer, int primitiveCount, PrimitiveType primitiveType)
    {
      Color4 v = ColorConverter.FromColor(Color);
      v.Alpha *= (float) SkinContext.Opacity;
      _effectHandleColor.SetParameter(v);
      _effect.StartRender(null);
      return true;
    }

    public override void SetupPrimitive(PrimitiveContext context)
    {
      Color4 v = ColorConverter.FromColor(Color);
      v.Alpha *= (float) SkinContext.Opacity;
      context.Effect = _effect;
      context.Parameters = new EffectParameters();
      context.Parameters.Add(_effectHandleColor, v);
    }

    public override void EndRender()
    {
      if (_effect != null)
        _effect.EndRender();
    }

  }
}
