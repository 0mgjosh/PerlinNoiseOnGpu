#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
Texture2D PaletteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

sampler2D PaletteTextureSampler = sampler_state
{
	Texture = <PaletteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 coords = input.TextureCoordinates;
    float4 perlinSample = tex2D(SpriteTextureSampler, coords);
	
    float isLand = perlinSample.r * round(perlinSample.r);
    float isWater = !isLand;
	
    float temp = (perlinSample.g * 1.5) * isLand;
    float temp2 = (perlinSample.g / 1.5) * isLand;
    float land = perlinSample.r * 1.8 * isLand;
    float water = perlinSample.r * isWater;
	
    float4 color = float4(temp, land, water, 
    1);
    
    color -= (perlinSample.b/1.2) * isLand;
	
    return color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};