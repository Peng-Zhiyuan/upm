#ifndef ROLE_LINE_INCLUDED
#define ROLE_LINE_INCLUDED

#include "CustomLightFunc.hlsl"
#ifdef _LOW_QUALITY
#include "Matcap.hlsl"
#else
#include "FaceShadowMap.hlsl"
#include "RimColor.hlsl"
#endif
#ifdef _VIEW_CLIP
#include "ViewPortClip.hlsl"
#endif

#ifdef _FRESNEL_EFFECT
#include "FresnelEffect.hlsl"
#endif

#ifdef _DISSOLVE_EFFECT
#include "DissolveEffect.hlsl"
#endif


struct appdata {
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	half3 normal : NORMAL;
	half4 tangent : TANGENT;
};
struct v2f {
	float4 pos : SV_POSITION;                           
	float2 uv : TEXCOORD0;
	float4 posWorld : TEXCOORD1;
	half3 normalDir : TEXCOORD2;
	half2 matcap : TEXCOORD3;
	#ifdef _VIEW_CLIP
		half3 viewPostion :TEXCOORD4;
	#endif

};

half4 _BaseColor;
sampler2D _Albedo;
float4 _Albedo_ST;
half _AlphaAnim;
half _AlphaAnimMin;
half _AlphaAnimMax;
half _AlphaAnimSpeed;
half4 _SColor;
half _RampThreshold;
half _RampSmooth;
half4 _CustomLightDir;
half _CustomLightAtten;
half4 _CustomLightColor;
half _GRoleLightAtten;
half4 _GRoleLightDir;
half4 _GRoleLightColor;
half4 _GRoleShadowColor;
v2f vert(appdata v) {
	v2f o = (v2f)0;
	o.posWorld = float4(TransformObjectToWorld(v.vertex.xyz), 1);
	o.pos = TransformWorldToHClip(o.posWorld.xyz);
	o.uv = v.uv;
	o.normalDir = TransformObjectToWorldNormal(v.normal);
#ifdef _VIEW_CLIP
	COMPUTE_VIEWPORTCLIP_DATA(o.viewPostion, o.posWorld)
#endif
	return o;
}


half4 frag(v2f i, half facing : VFACE) : SV_Target{
#ifdef _VIEW_CLIP
	VIEWPORT_CLIP(i.viewPostion)
#endif

	//float3 lightColor = _MainLightColor.rgb;
	half3 normalDirection = normalize(i.normalDir);
	
	half3 lightDirection = lerp(_MainLightPosition.xyz, _GRoleLightDir.xyz, _GRoleLightDir.w); //normalize(_MainLightPosition).xyz;
	lightDirection = lerp(lightDirection, _CustomLightDir.xyz, _CustomLightDir.w);

	lightDirection.y =  length(_MainLightPosition.xz);    
	lightDirection = normalize(lightDirection);

	half3 lightColor = lerp(_MainLightColor.rgb, _GRoleLightColor.rgb * _GRoleLightAtten, _GRoleLightColor.a);//_MainLightColor.rgb
	lightColor = lerp(lightColor, _CustomLightColor.rgb * _CustomLightAtten, _CustomLightColor.a);
	
	
	half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

    half ndl_xz = NDL_XZ(normalDirection, lightDirection, 0); 
	half ndl = smoothstep(_RampThreshold - _RampSmooth * 0.5, _RampThreshold + _RampSmooth * 0.5, ndl_xz);
	_SColor.rgb = lerp(_SColor.rgb, _SColor.rgb * _GRoleShadowColor,_GRoleShadowColor.a);
	half3 shadowColor =  lerp(_SColor.rgb, half3(1, 1, 1), ndl);
	half4 mainTex = tex2D(_Albedo, TRANSFORM_TEX(i.uv, _Albedo));
	half alpha = mainTex.a;

#ifdef _ALPHA_ANIM
	alpha *= lerp(_AlphaAnimMin, _AlphaAnimMax, (sin(_AlphaAnimSpeed * _Time.y) * 0.5 + 0.5));
#endif
	
	half3 diffuse = mainTex.rgb * _BaseColor.xyz * shadowColor;//lightColor;
	half3 additionalLights = 0;
#ifndef _NO_ADDITIONALLIGHTS
	uint pixelLightCount = GetAdditionalLightsCount();

	for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
	{
		Light light = GetCustomAdditionalLight(lightIndex, i.posWorld);
		half ndl = NDL(normalDirection, light.direction, 0);
		additionalLights += light.color * light.distanceAttenuation * ndl * 0.01;
	}
#endif
	half3 albedo = diffuse * max(half3(0.2, 0.2, 0.2), 0.5 * lightColor + additionalLights);
	//half3 albedo = diffuse * max(half3(0.2, 0.2, 0.2), CustomLightProbe(normalDirection) + 0.5 * lightColor + additionalLights);
#ifdef _FRESNEL_EFFECT
	albedo = ComputeFresnel(albedo, normalDirection, viewDirection, lightDirection);
#endif

#ifdef _DISSOLVE_EFFECT
	albedo = ComputeDissolve(albedo, i.posWorld);
#endif
	return half4(albedo, alpha);

}

#endif