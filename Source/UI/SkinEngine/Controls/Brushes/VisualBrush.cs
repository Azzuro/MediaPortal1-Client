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

using System.Collections.Generic;
using System.Drawing;
using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.ContentManagement;
using MediaPortal.UI.SkinEngine.Controls.Visuals;
using MediaPortal.UI.SkinEngine.DirectX;
using MediaPortal.UI.SkinEngine.Effects;
using MediaPortal.UI.SkinEngine.Rendering;
using SlimDX;
using SlimDX.Direct3D9;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.UI.SkinEngine.SkinManagement;

namespace MediaPortal.UI.SkinEngine.Controls.Brushes
{
  public class VisualBrush : TileBrush
  {
    #region Private fields

    AbstractProperty _visualProperty;
    EffectAsset _effect;
    Texture _textureVisual;

    #endregion

    #region Ctor

    public VisualBrush()
    {
      Init();
    }

    void Init()
    {
      _visualProperty = new SProperty(typeof(FrameworkElement), null);
      _effect = ContentManager.GetEffect("normal");
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      VisualBrush b = (VisualBrush) source;
      Visual = b.Visual; // Use the original Visual. Why should we use a copy?
    }

    #endregion

    #region Public properties

    public AbstractProperty VisualProperty
    {
      get { return _visualProperty; }
    }

    public FrameworkElement Visual
    {
      get { return (FrameworkElement)_visualProperty.GetValue(); }
      set { _visualProperty.SetValue(value); }
    }

    #endregion

    public override void SetupBrush(RectangleF bounds, ExtendedMatrix layoutTransform, float zOrder, ref PositionColored2Textured[] verts)
    {
      UpdateBounds(bounds, layoutTransform, ref verts);
      base.SetupBrush(bounds, layoutTransform, zOrder, ref verts);
      _textureVisual = new Texture(GraphicsDevice.Device, (int)_bounds.Width, (int)_bounds.Height, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);
    }

    public override bool BeginRender(PrimitiveContext primitiveContext)
    {
      if (Visual == null) return false;
      List<ExtendedMatrix> originalTransforms = SkinContext.CombinedRenderTransforms;
      SkinContext.CombinedRenderTransforms = new List<ExtendedMatrix>();

      GraphicsDevice.Device.EndScene();

      // Get the current backbuffer
      using (Surface backBuffer = GraphicsDevice.Device.GetRenderTarget(0))
      {
        // Get the surface of our texture
        using (Surface textureVisualSurface = _textureVisual.GetSurfaceLevel(0))
        {
          SurfaceDescription desc = backBuffer.Description;

          ExtendedMatrix matrix = new ExtendedMatrix();
          Vector3 pos = new Vector3(Visual.ActualPosition.X, Visual.ActualPosition.Y, Visual.ActualPosition.Z);
          float width = (float) Visual.ActualWidth;
          float height = (float) Visual.ActualHeight;
          float w = (float)(_bounds.Width / Visual.Width);
          float h = (float)(_bounds.Height / Visual.Height);

          //m.Matrix *= SkinContext.FinalMatrix.Matrix;
          //matrix.Matrix *= Matrix.Scaling(w, h, 1);

          if (desc.Width == GraphicsDevice.Width && desc.Height == GraphicsDevice.Height)
          {
            float cx = 1.0f;// ((float)desc.Width) / ((float)GraphicsDevice.Width);
            float cy = 1.0f;//((float)desc.Height) / ((float)GraphicsDevice.Height);

            //copy the correct rectangle from the backbuffer in the opacitytexture
            GraphicsDevice.Device.StretchRectangle(backBuffer, new Rectangle(
                (int) (_orginalPosition.X * cx), (int) (_orginalPosition.Y * cy),
                (int) (_bounds.Width * cx), (int) (_bounds.Height * cy)), textureVisualSurface,
                new Rectangle(0, 0, (int) (_bounds.Width), (int) (_bounds.Height)), TextureFilter.None);
            matrix.Matrix *= Matrix.Translation(new Vector3(-pos.X, -pos.Y, 0));
            matrix.Matrix *= Matrix.Scaling(GraphicsDevice.Width / width, GraphicsDevice.Height / height, 1);
          }
          else
          {
            GraphicsDevice.Device.StretchRectangle(backBuffer, new Rectangle(0, 0, desc.Width, desc.Height),
                textureVisualSurface, new Rectangle(0, 0, (int) _bounds.Width, (int) _bounds.Height), TextureFilter.None);
            
            matrix.Matrix *= Matrix.Translation(new Vector3(-pos.X, -pos.Y, 0));
            matrix.Matrix *= Matrix.Scaling(GraphicsDevice.Width / width, GraphicsDevice.Height / height, 1);
          }

          SkinContext.AddRenderTransform(matrix);

          // Change the rendertarget to our texture
          GraphicsDevice.Device.SetRenderTarget(0, textureVisualSurface);

          // Render the control (will be rendered into our texture)
          GraphicsDevice.Device.BeginScene();
          //GraphicsDevice.Device.VertexFormat = PositionColored2Textured.Format;
          Visual.DoRender();
          GraphicsDevice.Device.EndScene();
          SkinContext.RemoveRenderTransform();

          // Restore the backbuffer
          GraphicsDevice.Device.SetRenderTarget(0, backBuffer);
        }
        //Texture.ToFile(_textureVisual, @"c:\1\test.png", ImageFileFormat.Png);
        //TextureLoader.Save(@"C:\erwin\trunk\MP 2\MediaPortal\bin\x86\Debug\text.png", ImageFileFormat.Png, _textureVisual);
      }
      SkinContext.CombinedRenderTransforms = originalTransforms;
      if (Transform != null)
      {
        ExtendedMatrix mTrans;
        Transform.GetTransform(out mTrans);
        SkinContext.AddRenderTransform(mTrans);
      }
      // Now render our texture
      GraphicsDevice.Device.BeginScene();
      //GraphicsDevice.Device.VertexFormat = PositionColored2Textured.Format;
      _effect.StartRender(_textureVisual);

      return true;
    }

    public override void EndRender()
    {
      if (Visual != null)
      {
        _effect.EndRender();
        if (Transform != null)
          SkinContext.RemoveRenderTransform();
      }
    }
  }
}
