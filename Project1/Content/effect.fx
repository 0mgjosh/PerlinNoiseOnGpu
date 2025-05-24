#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
Texture2D palette;
float spin = 0;
float scale = 1;
float2 offset = float2(0,0);
int octave = 1;
float level = 0;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};
sampler2D PalletteSampler = sampler_state
{
    Texture = <palette>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float2 randomGradient(float2 p)
{
    float x = dot(p, float2(123.4, 234.5));
    float y = dot(p, float2(234.5, 335.6));
    float2 gradient = float2(x, y);
    gradient = sin(gradient);
    gradient = gradient * 127365.123;
    gradient = sin(gradient + spin);
    return gradient;
}

float2 quintic(float2 p)
{
    return p * p * p * (10.0 + p * (-15.0 + p * 6.0));
}

float GetPerlin(float x, float y, float frequency)
{
    float2 coord = float2(x, y);
    
    coord *= scale;
    
    coord += offset * frequency;
    
    float2 gridID = floor(coord);
    float2 gridUV = frac(coord);
	
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
    float result = lerp(b, t, gridUV.y);
	    
    return result;
}

float OctavePerlin(float x, float y, float z, int octaves, float persistence)
{
    float total = 0;
    float frequency = 1;
    float amplitude = 1;
    float maxValue = 0; // Used for normalizing result to 0.0 - 1.0
    
    
    for (int i = 0; i < octaves; i++)
    {
        float2 coord = float2(x, y);
        
        coord *= frequency;
        
        total += GetPerlin(coord.x, coord.y, frequency) * amplitude;
        
        maxValue += amplitude;
        
        amplitude *= persistence;
        
        frequency *= 2;
    }
    
    return total / maxValue + .3;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TextureCoordinates;
    uv -= float2(.5, .5);
	    
    float perlin = OctavePerlin(uv.x, uv.y, 0, octave, .5);
    
    float green = (perlin * round(perlin));
    green = smoothstep(0, .5, green) * green;
    float blue = (perlin * ((round(perlin) + 1) % 2));
    blue = smoothstep(-.5, .5, blue) * blue;
    
    return float4(0, green, blue, 1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};