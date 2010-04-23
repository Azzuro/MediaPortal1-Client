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
using MediaPortal.UI.SkinEngine.ContentManagement;
using MediaPortal.UI.SkinEngine.DirectX;
using SlimDX.Direct3D9;
using MediaPortal.UI.SkinEngine.SkinManagement;

namespace MediaPortal.UI.SkinEngine.Controls.Visuals
{
  public class VisualAssetContext : IAsset
  {
    protected VertexBuffer _vertexBuffer;
    protected PrimitiveType _primitiveType;
    protected Texture _texture;
    readonly string _name;
    static int _assetId = 0;

    public DateTime LastTimeUsed;

    public VisualAssetContext(string controlName, string screenName,
        PositionColored2Textured[] verts, PrimitiveType primitiveType, Texture texture)
    {
      _name = String.Format("visual#{0} {1} {2}", _assetId, screenName, controlName);
      _assetId++;
      _vertexBuffer = PositionColored2Textured.Create(verts.Length);
      _primitiveType = primitiveType;
      PositionColored2Textured.Set(_vertexBuffer, verts);
      _texture = texture;
      LastTimeUsed = SkinContext.Now;
    }

    #region IAsset Members

    public bool IsAllocated
    {
      get { return (_vertexBuffer != null || _texture != null); }
    }

    public bool CanBeDeleted
    {
      get
      {
        if (!IsAllocated)
        {
          return false;
        }
        TimeSpan ts = SkinContext.Now - LastTimeUsed;
        if (ts.TotalSeconds >= 1)
          return true;

        return false;
      }
    }

    public VertexBuffer VertexBuffer
    {
      get { return _vertexBuffer; }
    }

    public PrimitiveType PrimitiveType
    {
      get { return _primitiveType; }
    }

    public Texture Texture
    {
      get { return _texture; }
    }

    public bool Free(bool force)
    {
      if (_vertexBuffer != null)
      {
        _vertexBuffer.Dispose();
        _vertexBuffer = null;
      }

      if (_texture != null)
      {
        _texture.Dispose();
        _texture = null;
      }
      return false;
    }

    public override string ToString()
    {
      return _name;
    }

    #endregion
  }
}
