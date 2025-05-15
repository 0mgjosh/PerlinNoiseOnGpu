#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
float spin = 0;
float scale = 1;
float2 offset = float2(0,0);

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float2 randomGradient(float2 p)
{
    p = p + offset;
    float x = dot(p, float2(123.4, 234.5));
    float y = dot(p, float2(234.5, 335.6));
    float2 gradient = float2(x, y);
    gradient = sin(gradient);
    gradient = gradient * 43758.5453;
    gradient = sin(gradient + spin);
    return gradient;
}

float2 quintic(float2 p)
{
    return p * p * p * (10.0 + p * (-15.0 + p * 6.0));
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 white = float3(1,1,1);
    float3 black = float3(0,0,0);
    float3 col = black;

    float2 uv = input.TextureCoordinates;
	
    uv *= scale;
	
    float2 gridID = floor(uv);
    float2 gridUV = frac(uv);
    col = float3(gridID, 0);
    col = float3(gridUV, 0);
	
    float2 bl = gridID;
    float2 br = gridID + float2(1, 0);
    float2 tl = gridID + float2(0, 1);
    float2 tr = gridID + float2(1, 1);
    
    float2 gradBl = randomGradient(bl);
    float2 gradBr = randomGradient(br);
    float2 gradTl = randomGradient(tl);
    float2 gradTr = randomGradient(tr);
    
    float2 distFromPixelToBl = gridUV;
    float2 distFromPixelToBr = gridUV - float2(1, 0);
    float2 distFromPixelToTl = gridUV - float2(0, 1);
    float2 distFromPixelToTr = gridUV - float2(1, 1);
    
    float dotBl = dot(gradBl, distFromPixelToBl);
    float dotBr = dot(gradBr, distFromPixelToBr);
    float dotTl = dot(gradTl, distFromPixelToTl);
    float dotTr = dot(gradTr, distFromPixelToTr);
    
    gridUV = quintic(gridUV);

    float b = lerp(dotBl, dotBr, gridUV.x);
    float t = lerp(dotTl, dotTr, gridUV.x);
    float perlin = lerp(b, t, gridUV.y);
	
    perlin += 0.1;
        
    return float4(perlin,perlin,0,1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};