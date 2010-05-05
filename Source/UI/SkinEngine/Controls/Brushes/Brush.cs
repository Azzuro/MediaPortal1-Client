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
using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.Controls.Transforms;
using MediaPortal.UI.SkinEngine.DirectX;
using MediaPortal.UI.SkinEngine.MpfElements;
using MediaPortal.UI.SkinEngine.Rendering;
using SlimDX;
using SlimDX.Direct3D9;
using MediaPortal.Utilities.DeepCopy;

namespace MediaPortal.UI.SkinEngine.Controls.Brushes
{
  /// <summary>
  /// todo:
  ///   - transforms
  ///   - stretchmode
  ///   - tilemode
  ///   - alignmentx/alignmenty
  ///   - viewbox
  ///   - resource cleanup (textures & vertexbuffers)
  /// </summary>
  public class Brush : DependencyObject, IObservable
  {
    #region Private fields

    AbstractProperty _opacityProperty;
    AbstractProperty _relativeTransformProperty;
    Transform _transform;
    AbstractProperty _freezableProperty;
    protected RectangleF _vertsBounds;
    protected PointF _orginalPosition;
    protected PointF _minPosition;

    #endregion

    #region Ctor

    public Brush()
    {
      Init();
      Attach();
    }

    public override void Dispose()
    {
      base.Dispose();
      Detach();
      if (_transform != null)
        _transform.Dispose();
    }

    void Init()
    {
      _opacityProperty = new SProperty(typeof(double), 1.0);
      _relativeTransformProperty = new SProperty(typeof(Transform), new Transform());
      _transform = null;
      _freezableProperty = new SProperty(typeof(bool), false);
      _vertsBounds = new RectangleF(0, 0, 0, 0);
      _orginalPosition = new PointF(0, 0);
    }

    void Attach()
    {
      _opacityProperty.Attach(OnPropertyChanged);
    }

    void Detach()
    {
      _opacityProperty.Detach(OnPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      Brush b = (Brush) source;
      Opacity = b.Opacity;
      RelativeTransform = copyManager.GetCopy(b.RelativeTransform);
      Transform = copyManager.GetCopy(b.Transform);
      Freezable = b.Freezable;
      Attach();
    }

    #endregion

    public event ObjectChangedHandler ObjectChanged;

    #region Protected methods

    protected void FireChanged()
    {
      if (ObjectChanged != null)
        ObjectChanged(this);
    }

    /// <summary>
    /// Called when a property changed.
    /// </summary>
    /// <param name="prop">The property.</param>
    /// <param name="oldValue">The old value of the property.</param>
    protected virtual void OnPropertyChanged(AbstractProperty prop, object oldValue)
    { }

    #endregion

    #region Public properties

    public AbstractProperty FreezableProperty
    {
      get { return _freezableProperty; }
    }

    public bool Freezable
    {
      get { return (bool) _freezableProperty.GetValue(); }
      set { _freezableProperty.SetValue(value); }
    }

    public AbstractProperty OpacityProperty
    {
      get { return _opacityProperty; }
    }

    public double Opacity
    {
      get { return (double) _opacityProperty.GetValue(); }
      set { _opacityProperty.SetValue(value); }
    }

    public AbstractProperty RelativeTransformProperty
    {
      get { return _relativeTransformProperty; }
    }

    public Transform RelativeTransform
    {
      get { return (Transform) _relativeTransformProperty.GetValue(); }
      set { _relativeTransformProperty.SetValue(value); }
    }

    public Transform Transform
    {
      get { return _transform; }
      set { _transform = value; }
    }

    public virtual Texture Texture
    {
      get { return null; }
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Scales the specified u/v coordinates.
    /// </summary>
    public virtual void Scale(ref float u, ref float v, ref Color4 color)
    { }

    public virtual void SetupBrush(RectangleF bounds, ExtendedMatrix layoutTransform, float zOrder, PositionColored2Textured[] verts)
    {
      float w = bounds.Width;
      float h = bounds.Height;
      float xoff = _vertsBounds.X;
      float yoff = _vertsBounds.Y;
      if (layoutTransform != null)
      {
        w = _vertsBounds.Width;
        h = _vertsBounds.Height;
        layoutTransform.TransformXY(ref w, ref h);
        layoutTransform.TransformXY(ref xoff, ref yoff);
      }
      for (int i = 0; i < verts.Length; ++i)
      {
        PositionColored2Textured vert = verts[i];
        float x1 = vert.X;
        float u = x1 - (bounds.X + xoff);
        u /= w;

        float y1 = vert.Y;
        float v = y1 - (bounds.Y + yoff);
        v /= h;

        if (u < 0) u = 0;
        if (u > 1) u = 1;
        if (v < 0) v = 0;
        if (v > 1) v = 1;
        unchecked
        {
          Color4 color = ColorConverter.FromColor(Color.White);
          color.Alpha *= (float) Opacity;
          vert.Color = color.ToArgb();
        }
        vert.Tu1 = u;
        vert.Tv1 = v;
        vert.Z = zOrder;
        verts[i] = vert;
      }
    }

    protected void UpdateBounds(RectangleF bounds, ExtendedMatrix layoutTransform, PositionColored2Textured[] verts)
    {
      float minx = float.MaxValue;
      float miny = float.MaxValue;
      float maxx = 0;
      float maxy = 0;
      for (int i = 0; i < verts.Length; ++i)
      {
        PositionColored2Textured vert = verts[i];
        if (vert.X < minx) minx = vert.X;
        if (vert.Y < miny) miny = vert.Y;

        if (vert.X > maxx) maxx = vert.X;
        if (vert.Y > maxy) maxy = vert.Y;
      }
      // Albert, 2010-04-23: The following can be removed when the layout transform is no longer calculated in the
      // vertex coordinates
      if (layoutTransform != null)
      {
        maxx -= minx;
        maxy -= miny;
        minx -= bounds.X;
        miny -= bounds.Y;
        layoutTransform.InvertXY(ref minx, ref miny);
        layoutTransform.InvertXY(ref maxx, ref maxy);

        _orginalPosition.X = bounds.X;
        _orginalPosition.Y = bounds.Y;
        _minPosition.X = _orginalPosition.X + minx;
        _minPosition.Y = _orginalPosition.Y + miny;
      }
      _vertsBounds = new RectangleF(minx, miny, maxx, maxy);
    }

    public virtual bool BeginRender(PrimitiveContext primitiveContext)
    {
      return false;
    }

    public virtual void BeginRender(Texture tex)
    { }

    public virtual void EndRender()
    { }

    public virtual void Allocate()
    { }

    public virtual void Deallocate()
    { }

    public virtual void SetupPrimitive(PrimitiveContext context)
    { }

    #endregion
  }
}
