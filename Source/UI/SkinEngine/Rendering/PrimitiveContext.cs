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

using MediaPortal.UI.SkinEngine.ContentManagement;
using MediaPortal.UI.SkinEngine.Effects;
using MediaPortal.UI.SkinEngine.DirectX;
using SlimDX.Direct3D9;

namespace MediaPortal.UI.SkinEngine.Rendering
{
  public class PrimitiveContext
  {
    #region Protected fields

    protected EffectAsset _effect;
    protected EffectParameters _parameters;
    protected ITextureAsset _texture;
    protected VertexBuffer _vertexBuffer;
    protected PositionColored2Textured[] _vertices;
    protected PrimitiveType _primitiveType;
    protected int _numVertices;
    protected VertexFormat _vertexFormat;
    protected RenderGroup _renderGroup;

    #endregion

    public PrimitiveContext()
    {
    }

    public PrimitiveContext(int verticesCount, ref PositionColored2Textured[] vertices, PrimitiveType primitiveType)
    {
      InitializeVertexBuffer(verticesCount, vertices);
      _primitiveType = primitiveType;
      _vertexFormat = PositionColored2Textured.Format; // TODO: Make configurable
    }

    public void Dispose()
    {
      _vertexBuffer.Dispose();
    }

    public void InitializeVertexBuffer(int verticesCount, PositionColored2Textured[] vertices)
    {
      _numVertices = verticesCount;
      _vertexBuffer = PositionColored2Textured.Create(vertices.Length);
      PositionColored2Textured.Set(_vertexBuffer, vertices);
      _vertices = vertices;
    }

    public void OnVerticesChanged(int numVertices, PositionColored2Textured[] vertices)
    {
      InitializeVertexBuffer(numVertices, vertices);
      if (_renderGroup != null)
        _renderGroup.UpdateVertices = true;
    }

    #region Properties

    public RenderGroup RenderGroup
    {
      get { return _renderGroup; }
      set { _renderGroup = value; }
    }

    public EffectAsset Effect
    {
      get { return _effect; }
      set { _effect = value; }
    }

    public EffectParameters Parameters
    {
      get { return _parameters; }
      set { _parameters = value; }
    }

    public ITextureAsset Texture
    {
      get { return _texture; }
      set { _texture = value; }
    }

    public PositionColored2Textured[] Vertices
    {
      get { return _vertices; }
    }

    public VertexBuffer VertexBuffer
    {
      get { return _vertexBuffer; }
    }

    public PrimitiveType PrimitiveType
    {
      get { return _primitiveType; }
      set { _primitiveType = value; }
    }

    public int NumVertices
    {
      get { return _numVertices; }
      set { _numVertices = value; }
    }

    public VertexFormat VertexFormat
    {
      get { return _vertexFormat; }
    }

    #endregion
  }
}
