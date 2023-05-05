#ifndef SSS_COLOR_INCLUDED
#define SSS_COLOR_INCLUDED
#include "Base.hlsl"

half4 _SSSColor;

half _SSSAttenScale;
half _SSSAtten;
half _UseSSS;

half3 ComputeSSSColorByNoV(half3 albedo,half3 normalDirectionWS,half3 viewDirectionWS, half3 lightDirecton)
{
	half3 backDir =  normalDirectionWS * _SSSAtten + lightDirecton;
    half f =  1 - saturate(dot(viewDirectionWS, backDir));
    half intensity = saturate(pow(f, _SSSAttenScale));
	return lerp(albedo, albedo + _SSSColor.rgb *_SSSColor.a, intensity * _UseSSS);
}

#endif