#ifndef FRESNEL_EFFECT_INCLUDED
#define FRESNEL_EFFECT_INCLUDED
#include "Base.hlsl"

half _FresnelStart;
half4 _FresnelColor;
half _FresnelEffect;
half _DissolveEffect;
half3 ComputeFresnel(half3 albedo,half3 normalDirectionWS,half3 viewDirectionWS, half3 lightDirecton)//, half intensity)
{
    half f =  1.0 - saturate(dot(viewDirectionWS, normalDirectionWS));
    half rim = smoothstep(_FresnelStart, 1, f);
    half intensity = f * rim;//smoothstep(0, _RimSmooth, rim);
	lightDirecton.y = 0;
	normalDirectionWS.y = 0;
	//half isRimLight = max(1 - step(0,  dot(normalize(normalDirectionWS), normalize(lightDirecton))), 0.25);
	return lerp(albedo, albedo + _FresnelColor.rgb *_FresnelColor.a, intensity * max(_FresnelEffect, _DissolveEffect));// * isRimLight);
}
#endif