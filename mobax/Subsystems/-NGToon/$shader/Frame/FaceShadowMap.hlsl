#ifndef FACESHADOW_MAP_FUNC_INCLUDED
#define SHADOW_MAP_FUNC_INCLUDED
#include "Base.hlsl"

// For Shadow
//half _UseCustomFaceShadowMap;
half _ReciveShadow;
sampler2D _FaceShadowDepthMap;
half4 _FaceShadowDepthMap_TexelSize;
half4x4  _FaceLightProjection;
half4x4  _FaceLightDepthProjection;
half _Bias;
half _FaceFarplaneScale;


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