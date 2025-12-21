
struct VertexToPixel
{
    float4 Position   	: POSITION;    
    float4 Color		: COLOR0;
    float LightingFactor: TEXCOORD0;
    float2 TextureCoords: TEXCOORD1;
};

struct PixelToFrame
{
    float4 Color : COLOR0;
};

//------- Constants --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float3 xLightDirection;
float xAmbient;
bool xEnableLighting;
bool xShowNormals;
float3 xCamPos;
float3 xCamUp;
float xPointSpriteSize;
float xTime;

//------- Texture Samplers --------

Texture xTexture;
sampler TextureSampler = sampler_state { texture = <xTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

//------- Technique: Pretransformed --------

VertexToPixel PretransformedVS( float4 inPos : POSITION, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;
	
	Output.Position = inPos;
	Output.Color = inColor;
    
	return Output;    
}

PixelToFrame PretransformedPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = PSIn.Color;

	return Output;
}

technique Pretransformed
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 PretransformedVS();
		PixelShader  = compile ps_4_0_level_9_3 PretransformedPS();
#else
		VertexShader = compile vs_1_1 PretransformedVS();
		PixelShader  = compile ps_2_0 PretransformedPS();
#endif				
	}
}

//------- Technique: Colored --------

VertexToPixel ColoredVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Color = inColor;
	
	float3 Normal = normalize(mul(normalize(inNormal), (float3x3)xWorld));	
	Output.LightingFactor = 1;
	if (xEnableLighting)
		Output.LightingFactor = dot(Normal, -xLightDirection);
    
	return Output;    
}

PixelToFrame ColoredPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
    
	Output.Color = PSIn.Color;
	Output.Color.rgb *= saturate(PSIn.LightingFactor) + xAmbient;

	return Output;
}

technique Colored
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 ColoredVS();
		PixelShader  = compile ps_4_0_level_9_3 ColoredPS();
#else
		VertexShader = compile vs_1_1 ColoredVS();
		PixelShader  = compile ps_2_0 ColoredPS();
#endif				
	}
}

//------- Technique: ColoredInstanced --------

VertexToPixel ColoredInstancedVS( 
	float4 inPos : POSITION, 
	float3 inNormal: NORMAL, 
	float4 inColor: COLOR,
	float4 instanceWorld0 : TEXCOORD1,
	float4 instanceWorld1 : TEXCOORD2,
	float4 instanceWorld2 : TEXCOORD3,
	float4 instanceWorld3 : TEXCOORD4)
{	
	VertexToPixel Output = (VertexToPixel)0;
	
	// Reconstruct world matrix from instance data
	float4x4 instanceWorld = float4x4(
		instanceWorld0,
		instanceWorld1,
		instanceWorld2,
		instanceWorld3
	);
	
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (instanceWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Color = inColor;
	
	float3 Normal = normalize(mul(normalize(inNormal), (float3x3)instanceWorld));	
	Output.LightingFactor = 1;
	if (xEnableLighting)
		Output.LightingFactor = dot(Normal, -xLightDirection);
    
	return Output;    
}

technique ColoredInstanced
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 ColoredInstancedVS();
		PixelShader  = compile ps_4_0_level_9_3 ColoredPS();
#else
		// Instancing requires SM4, fall back to regular rendering
		VertexShader = compile vs_1_1 ColoredVS();
		PixelShader  = compile ps_2_0 ColoredPS();
#endif				
	}
}

//------- Technique: ColoredNoShading --------

VertexToPixel ColoredNoShadingVS( float4 inPos : POSITION, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.Color = inColor;
    
	return Output;    
}

PixelToFrame ColoredNoShadingPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
    
	Output.Color = PSIn.Color;

	return Output;
}

technique ColoredNoShading
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 ColoredNoShadingVS();
		PixelShader  = compile ps_4_0_level_9_3 ColoredNoShadingPS();
#else
		VertexShader = compile vs_1_1 ColoredNoShadingVS();
		PixelShader  = compile ps_2_0 ColoredNoShadingPS();
#endif				
	}
}


//------- Technique: Textured --------

VertexToPixel TexturedVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);	
	Output.TextureCoords = inTexCoords;
	
	float3 Normal = normalize(mul(normalize(inNormal), (float3x3)xWorld));	
	Output.LightingFactor = 1;
	if (xEnableLighting)
		Output.LightingFactor = dot(Normal, -xLightDirection);
    
	return Output;    
}

PixelToFrame TexturedPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = tex2D(TextureSampler, PSIn.TextureCoords);
	Output.Color.rgb *= saturate(PSIn.LightingFactor) + xAmbient;

	return Output;
}

technique Textured
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 TexturedVS();
		PixelShader  = compile ps_4_0_level_9_3 TexturedPS();
#else
		VertexShader = compile vs_1_1 TexturedVS();
		PixelShader  = compile ps_2_0 TexturedPS();
#endif				
	}
}

//------- Technique: TexturedNoShading --------

VertexToPixel TexturedNoShadingVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);	
	Output.TextureCoords = inTexCoords;
    
	return Output;    
}

PixelToFrame TexturedNoShadingPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
	
	Output.Color = tex2D(TextureSampler, PSIn.TextureCoords);

	return Output;
}

technique TexturedNoShading
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 TexturedNoShadingVS();
		PixelShader  = compile ps_4_0_level_9_3 TexturedNoShadingPS();
#else
		VertexShader = compile vs_1_1 TexturedNoShadingVS();
		PixelShader  = compile ps_2_0 TexturedNoShadingPS();
#endif		
	}
}

//------- Technique: PointSprites --------

VertexToPixel PointSpriteVS(float3 inPos: POSITION0, float2 inTexCoord: TEXCOORD0)
{
    VertexToPixel Output = (VertexToPixel)0;

    float3 center = mul(inPos, (float3x3)xWorld);
    float3 eyeVector = center - xCamPos;

    float3 sideVector = cross(eyeVector,xCamUp);
    sideVector = normalize(sideVector);
    float3 upVector = cross(sideVector,eyeVector);
    upVector = normalize(upVector);

    float3 finalPosition = center;
    finalPosition += (inTexCoord.x-0.5f)*sideVector*0.5f*xPointSpriteSize;
    finalPosition += (0.5f-inTexCoord.y)*upVector*0.5f*xPointSpriteSize;

    float4 finalPosition4 = float4(finalPosition, 1);

    float4x4 preViewProjection = mul (xView, xProjection);
    Output.Position = mul(finalPosition4, preViewProjection);

    Output.TextureCoords = inTexCoord;

    return Output;
}

PixelToFrame PointSpritePS(VertexToPixel PSIn)
{
    PixelToFrame Output = (PixelToFrame)0;
    Output.Color = tex2D(TextureSampler, PSIn.TextureCoords);
    return Output;
}

technique PointSprites
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 PointSpriteVS();
		PixelShader  = compile ps_4_0_level_9_3 PointSpritePS();
#else
		VertexShader = compile vs_1_1 PointSpriteVS();
		PixelShader  = compile ps_2_0 PointSpritePS();
#endif
	}
}

//------- Technique: Water --------

VertexToPixel WaterVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float4 inColor: COLOR)
{	
	VertexToPixel Output = (VertexToPixel)0;
	
	// Create animated wave distortion for cartoony water effect
	float3 pos = inPos.xyz;
	
	// Multiple wave layers for more interesting movement
	float wave1 = sin(pos.x * 0.02 + xTime * 0.5) * cos(pos.z * 0.02 + xTime * 0.3) * 2.0;
	float wave2 = sin(pos.x * 0.05 + pos.z * 0.05 + xTime * 0.7) * 1.5;
	float wave3 = cos(pos.x * 0.01 - pos.z * 0.01 + xTime * 0.4) * 1.0;
	
	// Combine waves for smooth, cartoony animation
	pos.y += wave1 + wave2 + wave3;
	
	// Transform position
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(float4(pos, 1.0), preWorldViewProjection);
	
	// Pass through color with slight variation based on wave height
	float waveIntensity = (wave1 + wave2 + wave3) * 0.1 + 0.5;
	Output.Color = inColor;
	Output.Color.rgb *= waveIntensity;
	
	// Calculate normal for lighting (slightly perturbed by waves)
	float3 Normal = normalize(mul(normalize(inNormal), (float3x3)xWorld));
	Output.LightingFactor = 1;
	if (xEnableLighting)
		Output.LightingFactor = dot(Normal, -xLightDirection);
    
	return Output;    
}

PixelToFrame WaterPS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
    
	// Cartoony water effect: bright, saturated blue with enhanced lighting
	float4 baseColor = PSIn.Color;
	
	// Enhance the color saturation for cartoony look
	float3 color = baseColor.rgb;
	color = lerp(color, color * 1.3, 0.3); // Boost saturation
	
	// Add fresnel-like effect for stylized water edge
	float fresnel = saturate(PSIn.LightingFactor * 0.5 + 0.5);
	fresnel = pow(fresnel, 1.5); // Sharper transition for cartoony look
	
	// Mix between base color and lighter color based on fresnel
	float3 finalColor = lerp(color, color * 1.5, fresnel);
	
	Output.Color = float4(finalColor, baseColor.a);
	
	// Apply lighting with enhanced contrast for cartoony style
	Output.Color.rgb *= saturate(PSIn.LightingFactor * 1.2) + xAmbient * 0.8;
	
	return Output;
}

technique Water
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 WaterVS();
		PixelShader  = compile ps_4_0_level_9_3 WaterPS();
#else
		VertexShader = compile vs_1_1 WaterVS();
		PixelShader  = compile ps_2_0 WaterPS();
#endif				
	}
}

//------- Technique: Water2 (Shader-based waves on simple rectangle) --------

VertexToPixel Water2VS( float4 inPos : POSITION, float3 inNormal: NORMAL, float4 inColor: COLOR, float2 inTexCoords: TEXCOORD0)
{	
	VertexToPixel Output = (VertexToPixel)0;
	
	// Transform position normally (no vertex displacement - all done in pixel shader)
	float4x4 preViewProjection = mul (xView, xProjection);
	float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
	Output.Position = mul(inPos, preWorldViewProjection);
	
	// Pass through color
	Output.Color = inColor;
	
	// Pass through texture coordinates (we'll use these to calculate world position in pixel shader)
	Output.TextureCoords = inTexCoords;
	
	// Calculate normal for lighting
	float3 Normal = normalize(mul(normalize(inNormal), (float3x3)xWorld));	
	Output.LightingFactor = 1;
	if (xEnableLighting)
		Output.LightingFactor = dot(Normal, -xLightDirection);
    
	return Output;    
}

PixelToFrame Water2PS(VertexToPixel PSIn) 
{
	PixelToFrame Output = (PixelToFrame)0;		
    
	// Reconstruct world position from texture coordinates
	float waterSize = 20000.0;
	float2 worldPos = (PSIn.TextureCoords - 0.5) * waterSize;
	float worldX = worldPos.x;
	float worldZ = worldPos.y;
	
	// ===== ANIMATED SURFACE NORMALS =====
	// Create detailed normal patterns for realistic surface variation
	// Multiple octaves of noise for detailed surface
	float2 uv1 = float2(worldX, worldZ) * 0.01 + float2(xTime * 0.3, xTime * 0.2);
	float2 uv2 = float2(worldX, worldZ) * 0.025 + float2(xTime * 0.5, -xTime * 0.3);
	float2 uv3 = float2(worldX, worldZ) * 0.05 + float2(-xTime * 0.2, xTime * 0.4);
	
	// Sample wave patterns for normal calculation (large-scale waves)
	float waveN1 = sin(uv1.x + uv1.y) * cos(uv1.x - uv1.y);
	float waveN2 = sin(uv2.x * 2.0 + uv2.y) * 0.5;
	float waveN3 = cos(uv3.x + uv3.y * 2.0) * 0.25;
	
	// ===== SMALL-SCALE WAVES (visible when camera is close) =====
	// Calculate distance from camera to water surface point
	// Water height is 20.0 (from WaterRendererType2)
	float waterHeight = 20.0;
	float3 waterWorldPos = float3(worldX, waterHeight, worldZ);
	float3 camToWater = waterWorldPos - xCamPos;
	float cameraDistance = length(camToWater);
	
	// Scale factor: more detail when camera is close (inverse distance with better curve)
	// More aggressive scaling - detail is strong when within 500 units, fades out beyond
	float detailScale = 1.0 - saturate((cameraDistance - 100.0) / 500.0);
	detailScale = pow(detailScale, 0.5); // Less aggressive falloff for more visible effect
	detailScale = max(detailScale, 0.4); // Always show significant small detail
	
	// High-frequency wave patterns for small-scale detail - HIGHER FREQUENCIES (less stretched)
	// Increased frequencies to make waves tighter and less stretched
	float2 uv4 = float2(worldX, worldZ) * 0.4 + float2(xTime * 1.2, xTime * 0.9);
	float2 uv5 = float2(worldX, worldZ) * 0.8 + float2(xTime * 1.8, -xTime * 1.1);
	float2 uv6 = float2(worldX, worldZ) * 1.5 + float2(-xTime * 0.7, xTime * 1.5);
	float2 uv7 = float2(worldX, worldZ) * 2.5 + float2(xTime * 2.0, xTime * 1.3); // Even higher frequency
	
	// Small-scale wave patterns with varying frequencies - MUCH MORE PRONOUNCED
	float waveN4 = sin(uv4.x * 2.0 + uv4.y * 1.8) * cos(uv4.x * 1.5 - uv4.y * 1.3) * 0.8;
	float waveN5 = sin(uv5.x * 3.0 + uv5.y * 2.2) * 0.6;
	float waveN6 = cos(uv6.x * 4.0 + uv6.y * 3.0 + xTime * 0.5) * 0.4;
	float waveN7 = sin(uv7.x * 5.0 + uv7.y * 4.0 + xTime * 0.8) * 0.3; // Additional high-frequency detail
	
	// Scale small waves by camera distance - MUCH STRONGER
	float smallWaveX = (waveN4 + waveN5 + waveN6 + waveN7) * detailScale * 1.5; // 1.5x multiplier
	float smallWaveZ = (cos(uv4.x * 2.0 + uv4.y * 1.8) * sin(uv4.x * 1.5 - uv4.y * 1.3) * 0.8 +
	                    cos(uv5.x * 3.0 + uv5.y * 2.2) * 0.6 +
	                    sin(uv6.x * 4.0 + uv6.y * 3.0 + xTime * 0.5) * 0.4 +
	                    cos(uv7.x * 5.0 + uv7.y * 4.0 + xTime * 0.8) * 0.3) * detailScale * 1.5;
	
	// Combine large-scale and small-scale waves
	float ddx = waveN1 + waveN2 + waveN3 + smallWaveX;
	float ddz = cos(uv1.x + uv1.y) * sin(uv1.x - uv1.y) + 
	            cos(uv2.x * 2.0 + uv2.y) * 0.5 + 
	            sin(uv3.x + uv3.y * 2.0) * 0.25 +
	            smallWaveZ;
	
	// ===== BUMP MAPPING FOR HIGHLIGHTS =====
	// Create additional high-frequency normal detail specifically for specular highlights
	float2 bumpUV = float2(worldX, worldZ) * 1.2 + float2(xTime * 1.5, xTime * 1.2);
	float bumpX = sin(bumpUV.x * 6.0 + bumpUV.y * 4.0) * cos(bumpUV.x * 5.0 - bumpUV.y * 3.0);
	float bumpZ = cos(bumpUV.x * 6.0 + bumpUV.y * 4.0) * sin(bumpUV.x * 5.0 - bumpUV.y * 3.0);
	
	// Bump mapping detail (stronger when close)
	float bumpStrength = detailScale * 0.4;
	float3 bumpNormal = normalize(float3(-bumpX * bumpStrength, 1.0, -bumpZ * bumpStrength));
	
	// Create perturbed normal (for refraction effect)
	// Small waves contribute more when camera is close - INCREASED STRENGTH
	float normalStrength = 0.15 + detailScale * 0.5; // Much stronger normal perturbation when close
	float3 surfaceNormal = normalize(float3(-ddx * normalStrength, 1.0, -ddz * normalStrength));
	
	// Combine surface normal with bump mapping for more pronounced detail
	surfaceNormal = normalize(surfaceNormal + bumpNormal * 0.3);
	
	// ===== WAVE HEIGHT CALCULATION =====
	// Calculate wave height for depth simulation
	float wave1 = sin(worldX * 0.02 + xTime * 0.5) * cos(worldZ * 0.02 + xTime * 0.3) * 2.0;
	float wave2 = sin(worldX * 0.05 + worldZ * 0.05 + xTime * 0.7) * 1.5;
	float wave3 = cos(worldX * 0.01 - worldZ * 0.01 + xTime * 0.4) * 1.0;
	
	// Add small-scale wave height variation (visible when close) - HIGHER FREQ, MORE PRONOUNCED
	float wave4 = sin(worldX * 0.4 + xTime * 1.2) * cos(worldZ * 0.4 + xTime * 0.9) * 1.2;
	float wave5 = sin(worldX * 0.8 + worldZ * 0.8 + xTime * 1.8) * 0.8;
	float wave6 = cos(worldX * 1.5 - worldZ * 1.5 + xTime * 1.0) * 0.5;
	float smallWaveHeight = (wave4 + wave5 + wave6) * detailScale * 1.2; // Additional multiplier
	
	float totalWave = wave1 + wave2 + wave3 + smallWaveHeight;
	
	// ===== DEPTH-BASED COLOR ABSORPTION =====
	// Simulate depth using wave variation and distance from center
	float simulatedDepth = abs(totalWave) * 0.5 + 0.5; // Normalize to 0-1
	float distanceFromCenter = length(worldPos) / (waterSize * 0.5); // 0 at center, 1 at edge
	simulatedDepth = lerp(simulatedDepth, 0.3, saturate(distanceFromCenter * 0.5)); // Deeper at edges
	
	// Absorption color (inverse of water color) - from article technique
	float3 waterColor = float3(0.39, 0.59, 1.0); // Base water blue (100, 150, 255 normalized)
	float3 absorptionColor = float3(1.0, 1.0, 1.0) - waterColor; // Orange/red absorption
	
	// Calculate absorption value using exponential falloff (as in article)
	float absorptionStrength = 0.8;
	float absorptionVal = 1.0 - exp2(-absorptionStrength * simulatedDepth * 5.0);
	float3 subtractiveColor = absorptionColor * absorptionVal;
	
	// Base underwater color (would normally sample from scene, but we'll use a tinted version)
	float3 baseColor = PSIn.Color.rgb;
	float3 underwaterColor = baseColor - subtractiveColor;
	underwaterColor = max(underwaterColor, float3(0.0, 0.0, 0.0)); // Clamp to avoid negatives
	
	// ===== REFRACTION EFFECT =====
	// Use surface normal to offset UVs for refraction-like effect
	float2 refractedUV = PSIn.TextureCoords + surfaceNormal.xz * 0.02;
	
	// ===== CAUSTICS =====
	// Light patterns projected onto underwater geometry (from article)
	float2 causticUV = float2(worldX, worldZ) * 0.005 + float2(xTime * 0.4, xTime * 0.3);
	float caustic1 = sin(causticUV.x * 3.0) * cos(causticUV.y * 3.0);
	float caustic2 = sin(causticUV.x * 5.0 + causticUV.y * 2.0 + xTime * 0.5) * 0.5;
	
	// Add small-scale caustic detail when camera is close
	float2 causticUVSmall = float2(worldX, worldZ) * 0.02 + float2(xTime * 1.0, xTime * 0.8);
	float causticSmall = sin(causticUVSmall.x * 8.0) * cos(causticUVSmall.y * 8.0) * 0.3;
	causticSmall *= detailScale; // Only visible when close
	
	float causticPattern = saturate(caustic1 + caustic2 + causticSmall);
	causticPattern = pow(causticPattern, 2.0); // Sharper caustics
	
	// Apply caustics to underwater color (multiply by light color)
	float3 lightColor = float3(1.0, 0.95, 0.9); // Warm sunlight
	underwaterColor += lightColor * causticPattern * 0.3 * (1.0 - simulatedDepth);
	
	// ===== DEPTH FOAM =====
	// Foam at shallow areas (from article technique)
	float foamDepth = 1.0 - simulatedDepth;
	float foamNoise = sin(worldX * 0.1 + xTime * 0.8) * cos(worldZ * 0.1 - xTime * 0.6);
	foamNoise = foamNoise * 0.5 + 0.5; // Normalize to 0-1
	float foamThreshold = 0.3;
	float foamMask = step(foamThreshold, foamDepth + foamNoise * 0.2);
	foamMask = max(foamMask, step(0.25, foamDepth)); // Additional threshold
	
	// Foam color (white/light blue)
	float3 foamColor = float3(0.9, 0.95, 1.0);
	
	// ===== FRESNEL EFFECT =====
	// More realistic fresnel based on viewing angle
	float3 viewDir = normalize(xCamPos - float3(worldX, waterHeight, worldZ));
	float fresnel = 1.0 - saturate(dot(surfaceNormal, viewDir));
	fresnel = pow(fresnel, 2.0);
	
	// ===== COMBINE ALL EFFECTS =====
	// Mix surface color (reflective) with underwater color (refractive)
	float3 surfaceColor = lerp(waterColor, waterColor * 1.5, fresnel);
	
	// Apply specular highlights based on surface normals with bump mapping
	float3 lightDir = normalize(-xLightDirection);
	
	// Main specular highlight using combined normal (surface + bump)
	float specular = pow(saturate(dot(reflect(-lightDir, surfaceNormal), viewDir)), 32.0);
	
	// Bump-mapped specular highlights - multiple layers for more pronounced effect
	float specularBump1 = pow(saturate(dot(reflect(-lightDir, bumpNormal), viewDir)), 64.0);
	float specularBump2 = pow(saturate(dot(reflect(-lightDir, bumpNormal), viewDir)), 128.0);
	float specularBump3 = pow(saturate(dot(reflect(-lightDir, bumpNormal), viewDir)), 256.0);
	
	// Combine specular highlights - bump mapping adds much more pronounced detail
	float totalSpecular = specular * 0.6 + 
	                      specularBump1 * detailScale * 0.4 +
	                      specularBump2 * detailScale * 0.3 +
	                      specularBump3 * detailScale * 0.2;
	
	surfaceColor += float3(1.0, 1.0, 1.0) * totalSpecular;
	
	// Combine surface and underwater colors based on transparency
	float transparency = 0.7;
	float3 finalColor = lerp(underwaterColor, surfaceColor, transparency + fresnel * 0.3);
	
	// Apply foam
	finalColor = lerp(finalColor, foamColor, foamMask * 0.8);
	
	// Apply lighting
	finalColor *= saturate(PSIn.LightingFactor) + xAmbient;
	
	Output.Color = float4(finalColor, PSIn.Color.a);
	
	return Output;
}

technique Water2
{
	pass Pass0
	{   
#if SM4
		VertexShader = compile vs_4_0_level_9_3 Water2VS();
		PixelShader  = compile ps_4_0_level_9_3 Water2PS();
#else
		VertexShader = compile vs_1_1 Water2VS();
		PixelShader  = compile ps_2_0 Water2PS();
#endif				
	}
}