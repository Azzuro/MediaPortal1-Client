half4x4 worldViewProj : WORLDVIEWPROJ; // Our world view projection matrix
texture g_texture; // Color texture 
 
sampler textureSampler = sampler_state
{
  Texture = <g_texture>;
  MipFilter = LINEAR;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
};
                          
//application to vertex structure
struct a2v
{
  half4 Position  : POSITION0;
  half4 Color     : COLOR0;
  half2 Texcoord  : TEXCOORD0;  // vertex texture coords 
};

// vertex shader to pixelshader structure
struct v2p 
{
  half4 Position   : POSITION;
  half4 Color      : COLOR0;
  half2 Texcoord   : TEXCOORD0;
};

// pixel shader to frame
struct p2f 
{
  half4 Color : COLOR0;
};
  
// the vertex shader
void renderVertexShader( in a2v IN, out v2p OUT ) 
{
  //getting to position to object space
  OUT.Position = mul(IN.Position, worldViewProj);
  OUT.Color = IN.Color;
  OUT.Texcoord = IN.Texcoord;
}

// the pixel shader
void renderPixelShader( in v2p IN, out p2f OUT) 
{ 
  OUT.Color = tex2D(textureSampler, IN.Texcoord) * IN.Color;
}

technique simple
{
  pass p0
  {
    VertexShader = compile vs_2_0 renderVertexShader();
    PixelShader  = compile ps_2_0 renderPixelShader();
  }
}
