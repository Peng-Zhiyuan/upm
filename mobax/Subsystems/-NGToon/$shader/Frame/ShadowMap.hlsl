#ifndef SHADOW_MAP_FUNC_INCLUDED
#define SHADOW_MAP_FUNC_INCLUDED
#include "Base.hlsl"

// For Shadow
half _UseCustomShadowMap;
half _ReciveShadow;
sampler2D _ShadowDepthMap;
half4 _ShadowDepthMap_TexelSize;
half4x4  _LightProjection;
half4x4  _LightDepthProjection;
half _Bias;
half _farplaneScale;


inline half GetNearDepth(half3 pos, half bias, sampler2D depthMap, half offsetX, half offsetY) {
	return (pos.z - bias > DecodeFloatRGBA(tex2D(depthMap, float2(pos.x + offsetX, pos.y + offsetY)))) ? 1 : 0;
}

inline half GetShadowAttenuate(half3 pos, sampler2D depthMap, half bias, half pixelWidth, half pixelHeight) {
	half atten = 0;
	int i = 0;
	int j = 0;
	for (i = -2; i <= 2; i++)
		for (j = -2; j <= 2; j++)
			atten += GetNearDepth(pos, bias, depthMap, i * pixelWidth, j * pixelHeight);
	atten = atten / 25;
	return 1 - atten;
}

#endif