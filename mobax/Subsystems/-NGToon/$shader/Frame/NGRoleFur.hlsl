#ifndef UNIVERSAL_ROLE_FUR_FUNC_INCLUDED
#define UNIVERSAL_ROLE_FUR_FUNC_INCLUDED

#include "CustomLightFunc.hlsl"
#include "Matcap.hlsl"
#ifdef _VIEW_CLIP
#include "ViewPortClip.hlsl"
#endif

struct appdata {
	 half4 vertex : POSITION;
	 half2 uv : TEXCOORD0;
	 half3 normal : NORMAL;
 };

struct v2f {
	 // 变量名必须为pos，因为光影宏：TRANSFER_VERTEX_TO_FRAGMENT中有些嵌套宏有固化这个变量名称来处理
	half4 pos : SV_POSITION;                           // shadow需要
	half2 uv : TEXCOORD0;
	half4 posWorld : TEXCOORD1;
	half3 normalDir : TEXCOORD2;
	half2 matcap : TEXCOORD3;
#ifdef _VIEW_CLIP
	half3 viewPostion :TEXCOORD4;
#endif
 };

half4 _Color;
uniform sampler2D _Normal;
sampler2D _Albedo;
sampler2D _FurTex;

half4 _HColor;
half4 _FSColor;

half _FRampThreshold;
half _RampSmooth;

half _FurLength;
half _FurDensity;
half _FurThinness;
half _FurShading;

half3 _ForceGlobal;
half3 _ForceLocal;

//half4 _ForceDirection;
//half  _ForceDirectionPower;

//half4 _SpecularColor;
//half _Glossiness;

//half _UseDiffuseLight;
//half _UseVertexColor;

half _FUR_OFFSET;

v2f vert_surface(appdata v)
{
	v2f o = (v2f)0;
	o.posWorld = float4(TransformObjectToWorld(v.vertex.xyz), 1);
	o.pos = TransformWorldToHClip(o.posWorld.xyz);
    o.uv = v.uv;
	o.normalDir = TransformObjectToWorldNormal(v.normal);
#ifdef _LOW_QUALITY
	//MatCap
	COMPUTE_MATCAP_DATA(v.normal, o.matcap)
#endif
#ifdef _VIEW_CLIP
	COMPUTE_VIEWPORTCLIP_DATA(o.viewPostion, o.posWorld.xyz)
#endif
    return o;
}

v2f vert_base(appdata v)
{
	v2f o = (v2f)0;
	half3 P = v.vertex.xyz + v.normal * _FurLength * _FUR_OFFSET;
	P += clamp(mul(unity_WorldToObject, _ForceGlobal).xyz + _ForceLocal.xyz, -1, 1) * pow(_FUR_OFFSET, 3) * _FurLength;
	o.posWorld = float4(TransformObjectToWorld(P), 1);
	o.pos = TransformWorldToHClip(o.posWorld.xyz);
    o.uv = v.uv;
	o.normalDir = TransformObjectToWorldNormal(v.normal);
#ifdef _VIEW_CLIP
	COMPUTE_VIEWPORTCLIP_DATA(o.viewPostion, o.posWorld.xyz)
#endif
    return o;
}

half4 frag_surface(v2f i): SV_Target
{   
#ifdef _VIEW_CLIP
	VIEWPORT_CLIP(i.viewPostion)
#endif
	half3 normalDirection = normalize(i.normalDir);
	half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
	half3 lightColor = _MainLightColor.rgb;
#ifdef _LOW_QUALITY
	half3 albedo = tex2D(_Albedo, i.uv).rgb * lightColor;
	//MatCap
	albedo = ComputeMatcapColor(albedo, i.matcap);
	albedo *= CustomLightProbe(normalDirection);
	return half4(albedo, 1);
#else
	half3 lightDirection = normalize(_MainLightPosition).xyz;
	
	half ndl = dot(normalDirection, lightDirection);
	
	half ramp = smoothstep(_FRampThreshold - _RampSmooth * 0.5, _FRampThreshold + _RampSmooth * 0.5, ndl);
	half3 shadowColor = lerp(_FSColor.rgb, _HColor.rgb, ramp);
	half4 mainTex = tex2D(_Albedo, i.uv) * _Color;
	half3 diffuse = mainTex.rgb * lightColor;

	uint pixelLightCount = GetAdditionalLightsCount();
	//遍历处理
	for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
	{
		//获取灯光
		Light light = GetCustomAdditionalLight(lightIndex, i.posWorld.xyz);
		half ndl = saturate(dot(normalDirection, light.direction));
		ndl = smoothstep(_FRampThreshold - _RampSmooth * 0.5, _FRampThreshold + _RampSmooth * 0.5, ndl);
		diffuse += light.color * light.distanceAttenuation * ndl;
	}
	diffuse *= shadowColor;

	half3 albedo = diffuse;
	
	return half4(albedo, 1);
#endif

	
}

half4 frag_base(v2f i): SV_Target
{
#ifdef _VIEW_CLIP
	VIEWPORT_CLIP(i.viewPostion)
#endif
	half3 normalDirection = normalize(i.normalDir);
	
	half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
	half3 lightDirection = normalize(_MainLightPosition).xyz;
	half3 lightColor = _MainLightColor.rgb;
	
	half ndl = dot(normalDirection, lightDirection);
	
	half ramp = smoothstep(_FRampThreshold - _RampSmooth * 0.5, _FRampThreshold + _RampSmooth * 0.5, ndl);
	half3 shadowColor = lerp(_FSColor.rgb, _HColor.rgb, ramp);
	half4 mainTex = tex2D(_Albedo, i.uv) * _Color;
	mainTex -= (pow(1 - _FUR_OFFSET, 3)) * _FurShading;
	half3 diffuse = mainTex.rgb * lightColor;
	
	uint pixelLightCount = GetAdditionalLightsCount();
	//遍历处理
	for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
	{
		//获取灯光
		Light light = GetCustomAdditionalLight(lightIndex, i.posWorld.xyz);
		half ndl = saturate(dot(normalDirection, light.direction));
		ndl = smoothstep(_FRampThreshold - _RampSmooth * 0.5, _FRampThreshold + _RampSmooth * 0.5, ndl);
		diffuse += light.color * light.distanceAttenuation * ndl;
	}
	diffuse *= shadowColor;
	
	half3 noise = tex2D(_FurTex, i.uv * _FurThinness).rgb;
	half alpha = clamp(noise.x - (_FUR_OFFSET * _FUR_OFFSET) * _FurDensity, 0, 1);
    
    return half4(diffuse, alpha);
}

#endif