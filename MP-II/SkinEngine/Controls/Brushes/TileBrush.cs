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
using MediaPortal.SkinEngine.DirectX;
using MediaPortal.SkinEngine.Controls.Visuals;
using SlimDX;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.SkinEngine.SkinManagement;

namespace MediaPortal.SkinEngine.Controls.Brushes
{

  public enum TileMode
  {
    //no tiling
    None,
    //content is tiled
    Tile,
    //content is tiled and flipped around x-axis
    FlipX,
    //content is tiled and flipped around y-axis
    FlipY
  };

  public class TileBrush : Brush
  {
    #region Private fields

    Property _alignmentXProperty;
    Property _alignmentYProperty;
    Property _stretchProperty;
    Property _viewPortProperty;
    Property _tileModeProperty;

    #endregion

    #region Ctor

    public TileBrush()
    {
      Init();
      Attach();
    }

    void Init()
    {
      _alignmentXProperty = new Property(typeof(AlignmentX), AlignmentX.Center);
      _alignmentYProperty = new Property(typeof(AlignmentY), AlignmentY.Center);
      _stretchProperty = new Property(typeof(Stretch), Stretch.Fill);
      _tileModeProperty = new Property(typeof(TileMode), TileMode.None);
      _viewPortProperty = new Property(typeof(Vector4), new Vector4(0, 0, 1, 1));
    }

    void Attach()
    {
      _alignmentXProperty.Attach(OnPropertyChanged);
      _alignmentYProperty.Attach(OnPropertyChanged);
      _stretchProperty.Attach(OnPropertyChanged);
      _tileModeProperty.Attach(OnPropertyChanged);
      _viewPortProperty.Attach(OnPropertyChanged);
    }

    void Detach()
    {
      _alignmentXProperty.Detach(OnPropertyChanged);
      _alignmentYProperty.Detach(OnPropertyChanged);
      _stretchProperty.Detach(OnPropertyChanged);
      _tileModeProperty.Detach(OnPropertyChanged);
      _viewPortProperty.Detach(OnPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      TileBrush b = source as TileBrush;
      AlignmentX = copyManager.GetCopy(b.AlignmentX);
      AlignmentY = copyManager.GetCopy(b.AlignmentY);
      Stretch = copyManager.GetCopy(b.Stretch);
      Tile = copyManager.GetCopy(b.Tile);
      ViewPort = copyManager.GetCopy(b.ViewPort);
      Attach();
    }

    #endregion

    #region Public properties

    public Property AlignmentXProperty
    {
      get { return _alignmentXProperty; }
    }

    public AlignmentX AlignmentX
    {
      get { return (AlignmentX)_alignmentXProperty.GetValue(); }
      set { _alignmentXProperty.SetValue(value); }
    }

    public Property AlignmentYProperty
    {
      get { return _alignmentYProperty; }
    }

    public AlignmentY AlignmentY
    {
      get { return (AlignmentY)_alignmentYProperty.GetValue(); }
      set { _alignmentYProperty.SetValue(value); }
    }

    public Property StretchProperty
    {
      get { return _stretchProperty; }
    }

    public Stretch Stretch
    {
      get { return (Stretch)_stretchProperty.GetValue(); }
      set { _stretchProperty.SetValue(value); }
    }

    public Property ViewPortProperty
    {
      get { return _viewPortProperty; }
    }

    public Vector4 ViewPort
    {
      get { return (Vector4)_viewPortProperty.GetValue(); }
      set { _viewPortProperty.SetValue(value); }
    }

    public Property TileProperty
    {
      get { return _tileModeProperty; }
    }

    public TileMode Tile
    {
      get { return (TileMode)_tileModeProperty.GetValue(); }
      set { _tileModeProperty.SetValue(value); }
    }

    #endregion

    public override void SetupBrush(FrameworkElement element, ref PositionColored2Textured[] verts)
    {
      // todo here:
      ///   - stretchmode
      ///   - tilemode  : none,tile,flipx,flipy
      ///   - alignmentx/alignmenty
      ///   - viewport  : dimensions of a single tile
      ///   
      switch (Stretch)
      {
        case Stretch.None:
          //center, original size
          break;

        case Stretch.Uniform:
          //center, keep aspect ratio and show borders
          break;

        case Stretch.UniformToFill:
          //keep aspect ratio, zoom in to avoid borders
          break;

        case Stretch.Fill:
          //stretch to fill
          break;
      }

      float w = (float)element.ActualWidth;
      float h = (float)element.ActualHeight;
      float xoff = _bounds.X;
      float yoff = _bounds.Y;
      if (element.FinalLayoutTransform != null)
      {
        w = _bounds.Width;
        h = _bounds.Height;
        element.FinalLayoutTransform.TransformXY(ref w, ref h);
        element.FinalLayoutTransform.TransformXY(ref xoff, ref yoff);
      }

      for (int i = 0; i < verts.Length; ++i)
      {
        float u, v;
        float x1, y1;
        y1 = (float)verts[i].Y;
        v = (float)(y1 - (element.ActualPosition.Y + yoff));
        v /= (float)(h * ViewPort.W);
        v += ViewPort.Y;


        x1 = (float)verts[i].X;
        u = (float)(x1 - (element.ActualPosition.X + xoff));
        u /= (float)(w * ViewPort.Z);
        u += ViewPort.X;

        Scale(ref u, ref v);

        if (u < 0) u = 0;
        if (u > 1) u = 1;
        if (v < 0) v = 0;
        if (v > 1) v = 1;
        unchecked
        {
          ColorValue color = ColorConverter.FromColor(System.Drawing.Color.White);
          color.Alpha *= (float)Opacity;
          verts[i].Color = color.ToArgb();
        }
        verts[i].Tu1 = u;
        verts[i].Tv1 = v;
        verts[i].Z = element.ActualPosition.Z;

      }
    }

    protected virtual void Scale(ref float u, ref float v)
    { }

    protected virtual Vector2 BrushDimensions
    {
      get { return new Vector2(1, 1); }
    }
  }
}
