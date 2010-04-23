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

using SlimDX;
using MediaPortal.UI.SkinEngine.DirectX;
using MediaPortal.UI.SkinEngine.Effects;
using MediaPortal.UI.SkinEngine.Rendering;
using MediaPortal.UI.SkinEngine.SkinManagement;
using SlimDX.Direct3D9;

namespace MediaPortal.UI.SkinEngine.ContentManagement
{
  public class TextureRender
  {
    #region Variables

    private float _previousX;
    private float _previousY;
    private float _previousZ;
    private float _previousWidth;
    private float _previousHeight;
    private float _previousColorUpperLeft;
    private float _previousColorBottomLeft;
    private float _previousColorBottomRight;
    private float _previousColorUpperRight;
    //private bool _previousGradientInUse = false;
    private float _previousUoff;
    private float _previousVoff;
    private float _previousUmax;
    private float _previousVMax;
    readonly PrimitiveContext _context;
    readonly PositionColored2Textured[] _vertices;
    readonly TextureAsset _texture;
    bool _added = false;

    #endregion

    public TextureRender(TextureAsset texture)
    {
      _texture = texture;
      _context = new PrimitiveContext
        {
            Texture = texture,
            Effect = ContentManager.GetEffect("normal"),
            Parameters = new EffectParameters(),
            PrimitiveType = PrimitiveType.TriangleList
        };
      _vertices = new PositionColored2Textured[6];
      Set(0, 0, 0, 0, 0, 0, 0, 1, 1, 0xff, 0xff, 0xff, 0xff);
    }


    /// <summary>
    /// Switches to a new texture.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public void SwitchTexture(string fileName, bool thumbnail)
    {
      if (_texture.Name != fileName)
      {
        _previousX = _previousY = _previousZ = _previousWidth = _previousHeight = 0;
        _previousColorUpperLeft = 0;
        _previousColorBottomLeft = 0;
        _previousColorBottomRight = 0;
        _previousColorUpperRight = 0;
        //        _previousGradientInUse = false;
        _context.Texture = ContentManager.GetTexture(fileName, thumbnail);
        Set(0, 0, 0, 0, 0, 0, 0, 1, 1, 0xff, 0xff, 0xff, 0xff);
      }
    }

    public TextureAsset Texture
    {
      get { return _texture; }
    }

    /// <summary>
    /// Fills the texture buffer with the rendering attribtues
    /// </summary>
    private void Set(float x, float y, float z, float w, float h,
        float uoff, float voff, float umax, float vmax,
        int colorUpperLeft, int colorBottomLeft,
        int colorBottomRight, int colorUpperRight)
    {
      if (x == _previousX && y == _previousY && z == _previousZ && w == _previousWidth && h == _previousHeight)
      {
        if (colorUpperLeft == _previousColorUpperLeft && colorBottomLeft == _previousColorBottomLeft &&
            colorBottomRight == _previousColorBottomRight && colorUpperRight == _previousColorUpperRight
            /*&& _previousGradientInUse == SkinContext.GradientInUse*/)
        {
          if (uoff == _previousUoff && voff == _previousVoff &&
              umax == _previousUmax && vmax == _previousVMax)
          {
            return;
          }
        }
      }
      _previousUoff = uoff;
      _previousVoff = voff;
      _previousUmax = umax;
      _previousVMax = vmax;

      uoff *= _texture.MaxU;
      voff *= _texture.MaxV;
      umax *= _texture.MaxU;
      vmax *= _texture.MaxV;
      UpdateVertexBuffer(x, y, z, w, h,
          uoff, voff, umax, vmax,
          colorUpperLeft, colorBottomLeft,
          colorBottomRight, colorUpperRight);
      _previousX = x;
      _previousY = y;
      _previousZ = z;
      _previousWidth = w;
      _previousHeight = h;
      _previousColorUpperLeft = colorUpperLeft;
      _previousColorBottomLeft = colorBottomLeft;
      _previousColorBottomRight = colorBottomRight;
      _previousColorUpperRight = colorUpperRight;
      //      _previousGradientInUse = SkinContext.GradientInUse;
    }

    private void UpdateVertexBuffer(float left, float top, float z, float width, float height,
        float uoff, float voff, float umax, float vmax,
        int alphaUpperLeft, int alphaBottomLeft,
        int alphaBottomRight, int alphaUpperRight)
    {
      float right = left + width;
      float bottom = top + height;

      float u1 = uoff;
      float u2 = umax;
      float v1 = voff;
      float v2 = vmax;

      Vector3 upperLeft = new Vector3(left, top, z);
      Vector3 bottomLeft = new Vector3(left, bottom, z);
      Vector3 bottomRight = new Vector3(right, bottom, z);
      Vector3 upperRight = new Vector3(right, top, z);


      unchecked
      {
        long colorUpperLeft = alphaUpperLeft;
        colorUpperLeft <<= 24;
        colorUpperLeft += 0xffffff;
        long colorBottomLeft = alphaBottomLeft;
        colorBottomLeft <<= 24;
        colorBottomLeft += 0xffffff;
        long colorBottomRight = alphaBottomRight;
        colorBottomRight <<= 24;
        colorBottomRight += 0xffffff;
        long colorUpperRight = alphaUpperRight;
        colorUpperRight <<= 24;
        colorUpperRight += 0xffffff;

        //upper left
        _vertices[0].Tu1 = u1;
        _vertices[0].Tv1 = v1;
        _vertices[0].Position = upperLeft;
        _vertices[0].Color = (int) colorUpperLeft;
        //SkinContext.GetAlphaGradientUV(upperLeft, out tu2, out tv2);

        //bottom left
        _vertices[1].Tu1 = u1;
        _vertices[1].Tv1 = v2;
        _vertices[1].Position = bottomLeft;
        _vertices[1].Color = (int) colorBottomLeft;
        //SkinContext.GetAlphaGradientUV(bottomLeft, out tu2, out tv2);

        //bottom right
        _vertices[2].Tu1 = u2;
        _vertices[2].Tv1 = v2;
        _vertices[2].Position = bottomRight;
        _vertices[2].Color = (int) colorBottomRight;
        //SkinContext.GetAlphaGradientUV(bottomRight, out tu2, out tv2);

        //upper left
        _vertices[3].Tu1 = u1;
        _vertices[3].Tv1 = v1;
        _vertices[3].Position = upperLeft;
        _vertices[3].Color = (int) colorUpperLeft;
        //SkinContext.GetAlphaGradientUV(upperLeft, out tu2, out tv2);

        //upper right
        _vertices[4].Tu1 = u2;
        _vertices[4].Tv1 = v1;
        _vertices[4].Position = upperRight;
        _vertices[4].Color = (int) colorUpperRight;
        //SkinContext.GetAlphaGradientUV(upperRight, out tu2, out tv2);

        //bottom right
        _vertices[5].Tu1 = u2;
        _vertices[5].Tv1 = v2;
        _vertices[5].Position = bottomRight;
        _vertices[5].Color = (int) colorBottomRight;
        //SkinContext.GetAlphaGradientUV(bottomRight, out tu2, out tv2);
        _context.OnVerticesChanged(2, _vertices);
      }
    }

    /// <summary>
    /// Draws the vertex buffer and associated texture
    /// </summary>
    public void Draw(float x, float y, float z, float width, float height,
        float uoff, float voff, float umax, float vmax,
        float alphaUpperLeft, float alphaBottomLeft,
        float alphaBottomRight, float alphaUpperRight)
    {
      if (!_texture.IsAllocated)
        _texture.Allocate();
      if (!_texture.IsAllocated)
        return;

      alphaUpperLeft *= 255;
      if (alphaUpperLeft < 0)
        alphaUpperLeft = 0;
      if (alphaUpperLeft > 255)
        alphaUpperLeft = 255;

      alphaBottomLeft *= 255;
      if (alphaBottomLeft < 0)
        alphaBottomLeft = 0;
      if (alphaBottomLeft > 255)
        alphaBottomLeft = 255;

      alphaBottomRight *= 255;
      if (alphaBottomRight < 0)
        alphaBottomRight = 0;
      if (alphaBottomRight > 255)
        alphaBottomRight = 255;

      alphaUpperRight *= 255;
      if (alphaUpperRight < 0)
        alphaUpperRight = 0;
      if (alphaUpperRight > 255)
        alphaUpperRight = 255;

      Set(x, y, z, width, height,
          uoff, voff, umax, vmax,
          (int) alphaUpperLeft,
          (int) alphaBottomLeft,
          (int) alphaBottomRight,
          (int) alphaUpperRight);
      //GraphicsDevice.Device.VertexFormat = PositionColored2Textured.Format;
      //GraphicsDevice.Device.SetStreamSource(0, _vertexBuffer, 0, PositionColored2Textured.StrideSize);

      //_effect.Render(_texture, 0);
      if (!_added)
      {
        RenderPipeline.Instance.Add(_context);
        _added = true;
      }
    }

    public void Free()
    {
      if (_added && _context != null)
      {
        if (SkinContext.UseBatching)
          RenderPipeline.Instance.Remove(_context);
        _context.Dispose();
      }
      _added = false;
    }
    public void Alloc()
    {
      if (!_added)
        if (SkinContext.UseBatching)
          RenderPipeline.Instance.Add(_context);
      _added = true;
    }
  }
}