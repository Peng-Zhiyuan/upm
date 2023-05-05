#ifndef UNIVERSAL_LIT_INPUT_INCLUDED
#define UNIVERSAL_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
half4 _SpecColor;
half _Emission;
half4 _EmissionColor;
half _Cutoff;
half _Smoothness;
half _Metallic;
half _BumpScale;
half _OcclusionStrength;
half4 _GlobalShadowColor;
half _Surface;
half _YSpeed;
half _XSpeed;
half _UseHeightMap;
//StylizedLit
float4 _MedColor, _ShadowColor, _ReflectColor;
float4 _SpecularLightOffset;
float _MedThreshold, _MedSmooth, _ShadowThreshold, _ShadowSmooth, _ReflectThreshold, _ReflectSmooth;
float _SpecularThreshold, _SpecularSmooth, _SpecularIntensity ,_FresnelIntensity, _FresnelThreshold, _FresnelSmooth;
float _ReflProbeIntensity, _ReflProbeRotation, _MetalReflProbeIntensity;
//float _ShadowBrushStrength, _ReflBrushStrength;
float _ReceiveShadows;
float _GIIntensity, _GGXSpecular, _DirectionalFresnel;




//#ifdef CUBE_MAP
half _UseReflectMap;
samplerCUBE _CubeMap;
sampler2D _MatMap;
half _ReflectAmount;
half3 _MapColor;
half4 _GlobalSunPos;
half3 _GlobalSunColor;

float4 unity_SpecCube0_ProbePosition;
float4 unity_SpecCube0_BoxMin;
float4 unity_SpecCube0_BoxMax;

float4 _SpecCube0_ProbePosition;
float4 _SpecCube0_BoxMin;
float4 _SpecCube0_BoxMax;
//#endif

//#ifdef HEIGHT_MAP
sampler2D _HeightMap;
half _HeightScale;
//#endif
CBUFFER_END

TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);
TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);
#define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)

half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
{
    half4 specGloss = 0;
#ifdef _METALLICSPECGLOSSMAP
    specGloss = SAMPLE_METALLICSPECULAR(uv);
    specGloss.a *= _Smoothness;
#else 
    specGloss.rgb = _Metallic.rrr;
    specGloss.a = _Smoothness;
#endif

    return specGloss;
}
half SampleOcclusion(float2 uv)
{
#ifdef _OCCLUSIONMAP
// TODO: Controls things like these by exposing SHADER_QUALITY levels (low, medium, high)
#if defined(SHADER_API_GLES)
    return SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
#else
    half occ = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
    return LerpWhiteTo(occ, _OcclusionStrength);
#endif
#else
    return 1.0;
#endif
}

inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    outSurfaceData = (SurfaceData)0;

    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

    half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = specGloss.rgb;// half3(0.0h, 0.0h, 0.0h);

    outSurfaceData.smoothness = specGloss.a; 
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outSurfaceData.occlusion = SampleOcclusion(uv);
    outSurfaceData.emission = specGloss.g * _EmissionColor.rgb;
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
