#ifndef WATER_SURFACEDATA_OLD_INCLUDED
#define WATER_SURFACEDATA_OLD_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 
    CBUFFER_START(UnityPerMaterial)
    float4 _BumpMap_ST, _FoamMap_ST;
    float4 _ShallowColor, _DeepColor, _FresnelColor, _FoamColor, _WaterSpecular;
    float4 _NormalSpeed;
    float _DepthDistance, _NormalInt1, _NormalInt2, _BumpMapScale, _Smoothness, _ReflectionIntensity, _ReflectionLod,
          _FresnelStrength, _FresnelPower, _FoamTextureSpeedX, _FoamTextureSpeedY, _FoamIntensity, _FoamFrequency,
          _speed, _FoamWidth, _IntersectionDepthDistance, _IntersectionWaterBlend, _NoiseScale, _NoiseStrength, _SpeedNoise,
		  _NoiseScaleDepth, _NoiseStrengthDepth, _SpeedNoiseDepth, _WaterGloss;
#ifdef WAVE
	float4 _MainTex_ST, _TurbulenceTex_ST, _NoiseTex_ST;
	float4 _MainColor;
	float _MainTexBrightness;
	
#endif
	float4 unity_SpecCube0_ProbePosition;
	float4 unity_SpecCube0_BoxMin;
	float4 unity_SpecCube0_BoxMax;
	float4 _SpecCube0_ProbePosition;
	float4 _SpecCube0_BoxMin;
	float4 _SpecCube0_BoxMax;
    CBUFFER_END
	//TEXTURECUBE(unity_SpecCube0);
	//SAMPLER(samplerunity_SpecCube0);

	TEXTURE2D_X_FLOAT(_CameraDepthTexture);
	SAMPLER(sampler_CameraDepthTexture);
	TEXTURE2D(_BumpMap);
	SAMPLER(sampler_BumpMap);
	TEXTURE2D(_FoamMap);
	SAMPLER(sampler_FoamMap);
	TEXTURE2D(_PlanarReflectionTexture);
	SAMPLER(sampler_PlanarReflectionTexture);
#ifdef WAVE
	TEXTURE2D(_MainTex);
	SAMPLER(sampler_MainTex);
	TEXTURE2D(_TurbulenceTex);
	SAMPLER(sampler_TurbulenceTex);
	TEXTURE2D(_NoiseTex);
	SAMPLER(sampler_NoiseTex);
#endif

	float3 NormalBlend(float3 A, float3 B)
	{
		return SafeNormalize(float3(A.rg + B.rg, A.b * B.b));
	}
	float4 Remap(float4 In, float2 InMinMax, float2 OutMinMax)
	{
		return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
	}
	half2 Unity_GradientNoise_Dir_half(half2 p)
	{
		p = p % 289;
		float x = float(34 * p.x + 1) * p.x % 289 + p.y;
		x = (34 * x + 1) * x % 289;
		x = frac(x / 41) * 2 - 1;
		return normalize(half2(x - floor(x + 0.5), abs(x) - 0.5));
	}
	half Unity_GradientNoise_half(half2 UV, half Scale)
	{ 
		half2 p = UV * Scale;
		half2 ip = floor(p);
		half2 fp = frac(p);
		half d00 = dot(Unity_GradientNoise_Dir_half(ip), fp);
		half d01 = dot(Unity_GradientNoise_Dir_half(ip + half2(0, 1)), fp - half2(0, 1));
		half d10 = dot(Unity_GradientNoise_Dir_half(ip + half2(1, 0)), fp - half2(1, 0));
		half d11 = dot(Unity_GradientNoise_Dir_half(ip + half2(1, 1)), fp - half2(1, 1));
		fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
		return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
	}
#endif