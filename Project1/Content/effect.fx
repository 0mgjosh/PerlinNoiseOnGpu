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

float2 randomGradient(float2 p, float seed)
{
    float x = dot(p, float2(123.4, 234.5));
    float y = dot(p, float2(234.5, 335.6));
    float2 gradient = float2(x, y);
    gradient = sin(gradient);
    gradient = gradient * seed;
    gradient = sin(gradient + spin);
    return gradient;
}

float2 quintic(float2 p)
{
    return p * p * p * (10.0 + p * (-15.0 + p * 6.0));
}

float GetPerlin(float x, float y, float frequency, float seed)
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
    
    float2 gradBl = randomGradient(bl, seed);
    float2 gradBr = randomGradient(br, seed);
    float2 gradTl = randomGradient(tl, seed);
    float2 gradTr = randomGradient(tr, seed);
    
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

float GetPerlinScaled(float x, float y, float scaler, float seed)
{
    float2 coord = float2(x, y);
    
    coord *= scale*scaler;
    
    coord += offset * scaler;
    
    float2 gridID = floor(coord);
    float2 gridUV = frac(coord);
	
    float2 bl = gridID;
    float2 br = gridID + float2(1, 0);
    float2 tl = gridID + float2(0, 1);
    float2 tr = gridID + float2(1, 1);
    
    float2 gradBl = randomGradient(bl, seed);
    float2 gradBr = randomGradient(br, seed);
    float2 gradTl = randomGradient(tl, seed);
    float2 gradTr = randomGradient(tr, seed);
    
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

float OctavePerlin(float x, float y, float z, int octaves, float persistence, float boost, float seed)
{
    float total = 0;
    float frequency = 1;
    float amplitude = 1;
    float maxValue = 0; // Used for normalizing result to 0.0 - 1.0
    
    
    for (int i = 0; i < octaves; i++)
    {
        float2 coord = float2(x, y);
        
        coord *= frequency;
        
        total += GetPerlin(coord.x, coord.y, frequency, seed) * amplitude;
        
        maxValue += amplitude;
        
        amplitude *= persistence;
        
        frequency *= 2;
    }
    
    return total / maxValue + boost;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TextureCoordinates;
    uv -= float2(.5, .5);
	    
    float perlin = OctavePerlin(uv.x, uv.y, 0, octave, .5, 0, 52312.234);
    perlin = (perlin + 1) / 2; // Normalize to 0-1 range
    perlin = clamp(perlin-.1, 0, 1); // Raise sea level
    
    float temperature = GetPerlinScaled(uv.x, uv.y, .1, 52312.234);
    temperature *= 1.5;
    temperature = (temperature + 1) / 2; // Normalize to 0-1 range
        
    //float green_gate = round(perlin);
    //float green = (perlin * green_gate);
    //green = (((green -.5) * 3) + .5) * green_gate;
    
    //float blue_gate = ((round(perlin) + 1) % 2);
    //float blue = (perlin * blue_gate);
    //blue = (((blue + 1) / 2) - .3) * blue_gate;
    
    float4 alpha = tex2D(SpriteTextureSampler, uv);
    
    float paletteX = (floor(perlin * 48) + .5) / 48;
    float paletteY = (floor(temperature * 7) + .5) / 7;
    float4 color = tex2D(PalletteSampler, float2( paletteX, paletteY));
        
    return float4(color.r,color.g,color.b,alpha.a);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};