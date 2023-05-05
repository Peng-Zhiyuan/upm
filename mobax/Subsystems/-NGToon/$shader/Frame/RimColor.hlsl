#ifndef RIM_COLOR_INCLUDED
#define RIM_COLOR_INCLUDED
#include "Base.hlsl"

sampler2D _CustomCameraDepthTexture;

TEXTURE2D_X_FLOAT(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

half _UseRim;
half4 _RimColor;

half _RimMin;
half _RimMax;

half _RimWidth;
half _RimThreshold;



half3 ComputeRimColorByDepth(half3 albedo,half3 normalDirection,float3 posWorld, float4 screenPos, half3 lightDirecton, float3 positionVS, float4 positionNDC, half intensity)
{
	float3 normalVS = TransformWorldToViewDir(normalDirection, true);
	float3 samplePositionVS = float3(positionVS.xy + normalVS.xy * _RimWidth, positionVS.z);
    float4 samplePositionCS = TransformWViewToHClip(samplePositionVS);
	float4 samplePositionVP = TransformHClipToViewPortPos(samplePositionCS);

	float depth = positionNDC.z / positionNDC.w;
	float linearEyeDepth = Linear01Depth(depth, _ZBufferParams);
    float offsetDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, samplePositionVP).r;
	float linearEyeOffsetDepth = Linear01Depth(offsetDepth, _ZBufferParams);
	float depthDiff = abs(linearEyeOffsetDepth - linearEyeDepth);
	half rimIntensity = step(0.5, depthDiff);//smoothstep(0, _RimThreshold, depthDiff);
	lightDirecton.y = 0;
	normalDirection.y = 0;
	half isRimLight = max(1 - step(0, dot(normalize(normalDirection), normalize(lightDirecton))), 0.25);

	return lerp(albedo, albedo + _RimColor.rgb * _RimColor.a, saturate(rimIntensity * isRimLight * intensity));
}
/*
half3 ComputeSSSColorByNoV(half3 albedo,half3 normalDirectionWS,half3 viewDirectionWS, half3 lightDirecton, half intensity)
{
	half3 backDir =  normalDirectionWS * 2 + lightDirecton;
    half f =  1 - saturate(dot(viewDirectionWS, backDir));
    half sssIntensity = saturate(pow(f, 4));
	return lerp(albedo, albedo + _RimColor.rgb *_RimColor.a, sssIntensity);
}
*/

half3 ComputeRimColorByNoV(half3 albedo,half3 normalDirectionWS,half3 viewDirectionWS, half3 lightDirecton, half intensity)
{
    half f =  1.0 - saturate(dot(viewDirectionWS, normalDirectionWS));
    half rim = smoothstep(_RimMin, _RimMax, f);
    half rimIntensity = rim;//smoothstep(0, _RimSmooth, rim);
	lightDirecton.y = 0;
	normalDirectionWS.y = 0;
	half isRimLight = max(1 - step(0,  dot(normalize(normalDirectionWS), normalize(lightDirecton))), 0.25);
	return lerp(albedo, albedo + _RimColor.rgb *_RimColor.a, rimIntensity * isRimLight * intensity);
}
/*
half3 ComputeRimColor(half3 albedo,half3 normalDirection,half3 posWorld,half4 screenPos)
{
	if (_UseRim > 0)
	{
		half3 rimLightDir = _RimPosition.xyz - posWorld;
		//Rim
		half rim = saturate(dot(rimLightDir.xyz, normalDirection));
		if (rim > 0)
		{
			half2 L_View = normalize(mul((half3x3)UNITY_MATRIX_V, rimLightDir.xyz).xy);
			half2 N_View = normalize(mul((half3x3)UNITY_MATRIX_V, normalDirection).xy);
			half lDotN = saturate(dot(N_View, L_View));

			_RimWidth *= _ScreenParams.x / 2000;

			_RimWidth /= screenPos.w * 0.1;
			half2 ssUVAdd = N_View * lDotN / _ScreenParams.xy;
			half depthTexture = tex2D(_CustomCameraDepthTexture, screenPos.xy + ssUVAdd * _RimWidth).x;
			rim *= saturate(1 - depthTexture);
		}		
		return lerp(albedo, albedo * _RimColor.rgb, rim);
		
	}
	return albedo;
}
*/
#endif