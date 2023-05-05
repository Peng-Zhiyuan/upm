#ifndef UNIVERSAL_CUSTOM_LIGHT_FUNC_INCLUDED
#define UNIVERSAL_CUSTOM_LIGHT_FUNC_INCLUDED

//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// Fills a light struct given a perObjectLightIndex
Light GetCustomAdditionalPerObjectLight(int perObjectLightIndex, float3 positionWS)
{
	// Abstraction over Light input constants
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
	float4 lightPositionWS = _AdditionalLightsBuffer[perObjectLightIndex].position;
	half3 color = _AdditionalLightsBuffer[perObjectLightIndex].color.rgb;
	half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[perObjectLightIndex].attenuation;
	half4 spotDirection = _AdditionalLightsBuffer[perObjectLightIndex].spotDirection;
//	half4 lightOcclusionProbeInfo = _AdditionalLightsBuffer[perObjectLightIndex].occlusionProbeChannels;
#else
	float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
	half3 color = _AdditionalLightsColor[perObjectLightIndex].rgb;
	half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
	half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
//	half4 lightOcclusionProbeInfo = _AdditionalLightsOcclusionProbes[perObjectLightIndex];
#endif

	// Directional lights store direction in lightPosition.xyz and have .w set to 0.0.
	// This way the following code will work for both directional and punctual lights.
	float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
	float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

	half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
	half attenuation = DistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.xy) * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

	Light light;
	light.direction = lightDirection;
	light.distanceAttenuation = attenuation;
	//	light.shadowAttenuation = AdditionalLightRealtimeShadow(perObjectLightIndex, positionWS);
	light.color = color;

	return light;
}

Light GetCustomAdditionalLight(uint i, float3 positionWS)
{
	int perObjectLightIndex = GetPerObjectLightIndex(i);
	return GetCustomAdditionalPerObjectLight(perObjectLightIndex, positionWS);
}

half3 CustomGlossyEnvironmentReflection(half3 reflectVector, half perceptualRoughness)
{
#if !defined(_ENVIRONMENTREFLECTIONS_OFF)
	half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
	half4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip);

#if !defined(UNITY_USE_NATIVE_HDR)
	half3 irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
#else
	half3 irradiance = encodedIrradiance.rbg;
#endif
	return irradiance;
#endif // GLOSSY_REFLECTIONS
	return _GlossyEnvironmentColor.rgb;
}

inline half NDL(half3 NormalDir,half3 LightDir,half occlusionOffset)
{
	return saturate(dot(NormalDir, LightDir) + occlusionOffset);
}

inline half NDL_XZ(half3 NormalDir,half3 LightDir,half occlusionOffset)
{
	 return saturate(dot(normalize(NormalDir.xz),normalize(LightDir.xz)) + occlusionOffset);
}

// Custom LightProbe
inline half3  CustomLightProbe(half3 normal)
{
	half4 n = half4(normal, 1);
	return half3(dot(unity_SHAr, n),  dot(unity_SHAg, n),  dot(unity_SHAb, n)) * 0.1;
	//half4 n = half4(1,1,1,0);
	//return max(half3(0.1,0.1,0.1), half3(dot(unity_SHAr, n),  dot(unity_SHAg, n),  dot(unity_SHAb, n)));

	//half3 averageSH = max(half3(0.1,0.1,0.1),half3(dot(unity_SHAr,half4(1,1,1,1)),  dot(unity_SHAg,half4(1,1,1,1)) ,  dot(unity_SHAb,half4(1,1,1,1))));
	//return averageSH;
    // can prevent result becomes completely black if lightprobe was not baked 
    //averageSH = max(_IndirectLightMinColor,averageSH);
	

	//  return half3(unity_SHAr, unity_SHAg, unity_SHAb);
     // return half3( dot(unity_SHAr,half4(1,1,1,1)),  dot(unity_SHAg,half4(1,1,1,1)) ,  dot(unity_SHAb,half4(1,1,1,1)) );
} 



#endif