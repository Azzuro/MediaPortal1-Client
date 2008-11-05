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

using MediaPortal.Presentation.DataObjects;
using SlimDX;
using SlimDX.Direct3D9;
using MediaPortal.SkinEngine.DirectX;
using MediaPortal.SkinEngine.Xaml.Interfaces;
using MediaPortal.Utilities.DeepCopy;

namespace MediaPortal.SkinEngine.Controls.Brushes
{
  public enum BrushMappingMode
  {
    Absolute,
    RelativeToBoundingBox
  };

  public enum ColorInterpolationMode
  {
    ColorInterpolationModeScRgbLinearInterpolation,
    ColorInterpolationModeSRgbLinearInterpolation
  };

  public enum GradientSpreadMethod
  {
    Pad,
    Reflect,
    Repeat
  };

  public class GradientBrush : Brush, IAddChild<GradientStop>
  {
    #region Private fields

    protected PositionColored2Textured[] _verts;
    Property _colorInterpolationModeProperty;
    Property _gradientStopsProperty;
    Property _spreadMethodProperty;
    Property _mappingModeProperty;

    #endregion

    #region Ctor

    public GradientBrush()
    {
      Init();
      Attach();
    }

    void Init()
    {
      _gradientStopsProperty = new Property(typeof(GradientStopCollection), new GradientStopCollection(this));
      _colorInterpolationModeProperty =
        new Property(typeof(ColorInterpolationMode),
                     ColorInterpolationMode.ColorInterpolationModeScRgbLinearInterpolation);
      _spreadMethodProperty = new Property(typeof(GradientSpreadMethod), GradientSpreadMethod.Pad);
      _mappingModeProperty = new Property(typeof(BrushMappingMode), BrushMappingMode.RelativeToBoundingBox);
    }

    void Attach()
    {
      _gradientStopsProperty.Attach(OnPropertyChanged);
      _colorInterpolationModeProperty.Attach(OnPropertyChanged);
      _spreadMethodProperty.Attach(OnPropertyChanged);
      _mappingModeProperty.Attach(OnPropertyChanged);
    }

    void Detach()
    {
      _gradientStopsProperty.Detach(OnPropertyChanged);
      _colorInterpolationModeProperty.Detach(OnPropertyChanged);
      _spreadMethodProperty.Detach(OnPropertyChanged);
      _mappingModeProperty.Detach(OnPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      GradientBrush b = (GradientBrush) source;
      ColorInterpolationMode = copyManager.GetCopy(b.ColorInterpolationMode);
      SpreadMethod = copyManager.GetCopy(b.SpreadMethod);
      MappingMode = copyManager.GetCopy(b.MappingMode);
      foreach (GradientStop stop in b.GradientStops)
        GradientStops.Add(copyManager.GetCopy(stop));
      Attach();
    }

    #endregion

    /// <summary>
    /// Called when one of the gradients changed.
    /// </summary>
    public void OnGradientsChanged()
    {
      OnPropertyChanged(GradientStopsProperty);
    }

    #region Public properties

    public Property ColorInterpolationModeProperty
    {
      get { return _colorInterpolationModeProperty; }
    }

    public ColorInterpolationMode ColorInterpolationMode
    {
      get { return (ColorInterpolationMode)_colorInterpolationModeProperty.GetValue(); }
      set { _colorInterpolationModeProperty.SetValue(value); }
    }

    public Property GradientStopsProperty
    {
      get { return _gradientStopsProperty; }
    }

    public GradientStopCollection GradientStops
    {
      get { return (GradientStopCollection)_gradientStopsProperty.GetValue(); }
    }

    public Property MappingModeProperty
    {
      get { return _mappingModeProperty; }
    }

    public BrushMappingMode MappingMode
    {
      get { return (BrushMappingMode)_mappingModeProperty.GetValue(); }
      set { _mappingModeProperty.SetValue(value); }
    }

    public Property SpreadMethodProperty
    {
      get { return _spreadMethodProperty; }
    }

    public GradientSpreadMethod SpreadMethod
    {
      get { return (GradientSpreadMethod)_spreadMethodProperty.GetValue(); }
      set { _spreadMethodProperty.SetValue(value); }
    }

    #endregion

    #region Protected methods

    protected void SetColor(VertexBuffer vertexbuffer)
    {
      Color4 color = ColorConverter.FromColor(GradientStops[0].Color);
      color.Alpha *= (float)Opacity;
      for (int i = 0; i < _verts.Length; ++i)
      {
        _verts[i].Color = color.ToArgb();
      }

      PositionColored2Textured.Set(vertexbuffer, ref _verts);
    }

    #endregion

    #region IAddChild Members

    public void AddChild(GradientStop o)
    {
      GradientStops.Add(o);
    }

    #endregion
  }
}
