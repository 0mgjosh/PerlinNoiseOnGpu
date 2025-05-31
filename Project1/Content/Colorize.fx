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
float3 sun;

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
	
    float3 sample_position = float3(coords.x, coords.y, perlinSample.r);
    float3 direction = normalize(sun-sample_position);
    float point_distance = distance(sun,sample_position);
    float shadow = 0;
    float shadow_length = 0;
	
    for (int i = 0; i < 100; i++)
    {
        float a1 = tex2D(SpriteTextureSampler, sample_position.xy + float2(0,.0001)).r;
        float a2 = tex2D(SpriteTextureSampler, sample_position.xy + float2(-.0001,-.0001)).r;
        float a3 = tex2D(SpriteTextureSampler, sample_position.xy + float2(.0001,.0001)).r;
        float a4 = tex2D(SpriteTextureSampler, sample_position.xy).r;
        float perlin_at_step = (a1 + a2 + a3+a4)/4;
        
        sample_position += direction * (i*0.0001);
        
        if (perlin_at_step > sample_position.z)
        {
            shadow_length = i;
            shadow = shadow_length;
        }

    }
    shadow *= isLand;
    return color-shadow;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};