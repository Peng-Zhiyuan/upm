#ifndef ROLE_BODY_INCLUDED
#define ROLE_BODY_INCLUDED

#include "CustomLightFunc.hlsl"		
#ifdef _LOW_QUALITY
#include "Matcap.hlsl"
#else
#include "ShadowMap.hlsl"
#include "RimColor.hlsl"
#endif
#ifdef _USE_DITHER
#include "Dither.hlsl"
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

//#include "SSS.hlsl"
half4 _BaseColor;
sampler2D _Albedo;
half4 _Albedo_ST;
sampler2D _OcclusionMap;
sampler2D _Ramp;

half4 _SColor;

half _RampThreshold;
half _RampThresholdSub;
half _RampSmooth;

half _OcclusionAddThreshold;
half _OcclusionAddIntensity;
half _OcclusionSubIntensity;

half4 _SpecularColor;
half _Glossiness;


half4 _EmissionColor;

half4 _OffsetPosition;
half _OffsetProcess;
half4 _CustomLightDir;
half _CustomLightAtten;
half4 _CustomLightColor;
half4 _RoleUp;
half4 _RoleFront;
half4 _RolePosition;
half _CameraDistance;
half _GRoleLightFreeDir;
half4 _GRoleLightDir;
half4 _GRoleLightColor;

half _GRoleLightAtten;
half4 _GRoleShadowColor;
struct appdata {
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	half3 normal : NORMAL;
};
struct v2f {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 posWorld : TEXCOORD1;
	half3 normalDir : TEXCOORD2;


#ifdef _LOW_QUALITY
	half2 matcap : TEXCOORD3;
	half4 shadowCoord : TEXCOORD4;
#if defined(_USE_DITHER)
	half4 spos :TEXCOORD5;
	half cameraDistance:TEXCOORD6;
#endif

#ifdef _VIEW_CLIP
	float3 viewPostion :TEXCOORD5;
#endif
#else
	half4 shadowCoord : TEXCOORD3;
	half4 spos :TEXCOORD4;
	float4 ndcPos:TEXCOORD5;
	float3 vsPos:TEXCOORD6;
#ifdef _VIEW_CLIP
	half3 viewPostion :TEXCOORD7;
#endif
#if _USE_DITHER
	half cameraDistance:TEXCOORD7;
#endif
#endif
};

v2f vert(appdata v) {
	v2f o = (v2f)0;
	o.posWorld = TransformObjectToWorld(v.vertex.xyz);
	o.pos = TransformWorldToHClip(o.posWorld);

	o.uv = v.uv;
	o.normalDir = TransformObjectToWorldNormal(v.normal);
#ifdef _LOW_QUALITY
	//MatCap
	//COMPUTE_MATCAP_DATA(v.normal, o.matcap)
#ifdef _USE_MATCAP
	COMPUTE_MATCAP_DATA(v.normal, o.matcap)
#endif
#else
#ifdef _USE_RIM_DEPTH 
	VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
	o.ndcPos = vertexInput.positionNDC;
	o.vsPos = vertexInput.positionVS;
#endif
	//shadow
	//o.shadowCoord = mul(_LightProjection, half4(o.posWorld, 1));
	//o.shadowCoord.z = -(mul(_LightDepthProjection, half4(o.posWorld, 1)).z * _farplaneScale);
#ifdef MAIN_LIGHT_SHADOWS
	o.shadowCoord = TransformWorldToShadowCoord(_RolePosition.xyz);
#endif
	o.spos = ComputeScreenPos(o.pos);
	//o.spos.xy /= o.spos.w;
	float3 vPos = TransformWorldToView(o.posWorld);
	o.spos.z = -vPos.z * _ProjectionParams.w;
	//o.spos.w = length(vPos);
#ifdef _USE_DITHER
	o.cameraDistance = length(_WorldSpaceCameraPos - o.posWorld);
#endif

#endif
#ifdef _VIEW_CLIP
	COMPUTE_VIEWPORTCLIP_DATA(o.viewPostion, o.posWorld)
#endif
		return o;
}


half4 frag(v2f i) : SV_Target{
	//half3 lightColor = _MainLightColor.rgb;
#ifdef _VIEW_CLIP
	VIEWPORT_CLIP(i.viewPostion)
#endif
	half alpha = 1;
#ifdef _USE_DITHER
	alpha *= saturate((_CameraDistance - 1) / 2);
	//alpha *= saturate(_CameraDistance / 5);
	ditherClip(i.spos.xy / i.spos.w, alpha);
#endif

	half3 normalDirection = normalize(i.normalDir);


	half3 lightDirection = lerp(_MainLightPosition.xyz, _GRoleLightDir.xyz, _GRoleLightDir.w); //normalize(_MainLightPosition).xyz;
	lightDirection = lerp(lightDirection, _CustomLightDir.xyz, _CustomLightDir.w);
	lightDirection.y = lerp(length(_MainLightPosition.xz), lightDirection.y, _GRoleLightFreeDir);    
	//lightDirection.y =  length(_MainLightPosition.xz);    
	lightDirection = normalize(lightDirection);

	half3 lightColor = lerp(_MainLightColor.rgb, _GRoleLightColor.rgb * _GRoleLightAtten, _GRoleLightColor.a);//_MainLightColor.rgb
	lightColor = lerp(lightColor, _CustomLightColor.rgb * _CustomLightAtten, _CustomLightColor.a);


	//half4 SHADOW_COORDS = TransformWorldToShadowCoord(_RolePosition.xyz);
	//half shadow = MainLightRealtimeShadow(SHADOW_COORDS);
#ifdef MAIN_LIGHT_SHADOWS
	half shadow = MainLightRealtimeShadow(i.shadowCoord);
	lightColor = lightColor * lerp(0.65, 1,saturate(shadow));
#endif
	half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
//#ifndef _LOW_QUALITY
	
	//half atten = 1;


	//if (_ReciveShadow > 0)
	//{
		//atten = 1 - GetNearDepth(i.shadowCoord.xyz, _Bias, _ShadowDepthMap, 0, 0);
	//	atten = GetShadowAttenuate(i.shadowCoord.xyz, _ShadowDepthMap, _Bias, _ShadowDepthMap_TexelSize.x * 0.8, _ShadowDepthMap_TexelSize.y * 0.8);
	//}
	
//#endif
	half vdl =  dot(normalize(viewDirection.xz), normalize(lightDirection.xz)) * 0.5 + 0.5;
	_RampThreshold = lerp(_RampThreshold - _RampThresholdSub, _RampThreshold, vdl);
	half3 templightDir = half3(lightDirection.x, lerp(lightDirection.y * 0, lightDirection.y, vdl), lightDirection.z);
	half ndl = dot(normalDirection, templightDir);


	half4 occlusion = tex2D(_OcclusionMap, i.uv).rgba;

	half occAdd = step(occlusion.g, _OcclusionAddThreshold);
	half occSub = step(_OcclusionAddThreshold, occlusion.g);
	occAdd = lerp(0, (_OcclusionAddThreshold - occlusion.g) * _OcclusionAddIntensity, occAdd);
	occSub = lerp(0, (occlusion.g - _OcclusionAddThreshold) * _OcclusionSubIntensity, occSub);
	half occOffset = occSub - occAdd;
	ndl += occOffset;
	ndl = min(ndl, 1);
//#ifndef _LOW_QUALITY
	//ndl = min(ndl, atten);
//#endif

    //_RampThreshold = lerp(0, _RampThreshold, dot(normalize(viewDirection.xz), normalize(lightDirection.xz)) * 0.5 + 0.5);
	half ramp = smoothstep(_RampThreshold - _RampSmooth * 0.5, _RampThreshold + _RampSmooth * 0.5, ndl);
	ndl = saturate(ndl + (1 - _RampThreshold));
	_SColor.rgb = lerp(_SColor.rgb,_SColor.rgb * _GRoleShadowColor,_GRoleShadowColor.a);
#ifndef _LOW_QUALITY
	//half3 rampTexture = tex2D(_Ramp, half2(ndl, ndl)).rgb;
	//half3 shadowColor = lerp(lerp(_SColor.rgb, half3(1, 1, 1), ramp), half3(1, 1, 1), rampTexture);
	#ifdef _USE_RAMP
		half3 rampTexture = tex2D(_Ramp, half2(ndl, ndl)).rgb;
		half3 shadowColor = lerp(lerp(_SColor.rgb, half3(1, 1, 1), ndl), half3(1, 1, 1), rampTexture);
	#else
		half3 shadowColor = lerp(_SColor.rgb, half3(1, 1, 1), ramp);//lerp(lerp(_SColor.rgb, half3(1, 1, 1), ramp), half3(1, 1, 1), 0);//lerp(_SColor.rgb, half3(1, 1, 1), ndl);
	#endif
#else
	half3 shadowColor = lerp(_SColor.rgb, half3(1, 1, 1), ramp);//lerp(lerp(_SColor.rgb, half3(1, 1, 1), ramp), half3(1, 1, 1), half3(0.3137, 0.3137, 0.3137));
#endif






	half4 mainTex = tex2D(_Albedo, TRANSFORM_TEX(i.uv, _Albedo));

	half3 diffuse = mainTex.rgb * _BaseColor.rgb * shadowColor;// * lightColor;
    half3 additionalLights = 0;

#ifndef _NO_ADDITIONALLIGHTS
	uint pixelLightCount = GetAdditionalLightsCount();
	for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
	{
		Light light = GetCustomAdditionalLight(lightIndex, i.posWorld);
#ifndef _LOW_QUALITY
		half ndl = NDL(normalDirection, light.direction, occOffset);
#else
		half ndl = NDL(normalDirection, light.direction, 0);
#endif
		ndl = smoothstep(_RampThreshold - _RampSmooth * 0.5, _RampThreshold + _RampSmooth * 0.5, ndl);
		additionalLights += light.color * light.distanceAttenuation * ndl * 0.01;
	}
#endif

	//diffuse *= shadowColor;
	//emission
	half3 emission = step(0.9,occlusion.b) * _EmissionColor.rgb * mainTex.rgb;
	emission = 0.5 * emission + 0.5 * emission * lightColor;

#ifdef _LOW_QUALITY

	//MatCap
	//albedo = ComputeMatcapColor(albedo, i.matcap);
	//albedo = lerp(albedo, albedo * CustomLightProbe(), 0.5);
	//albedo += CustomLightProbe(normalDirection);
	//half3 albedo = diffuse * max(half3(0.2, 0.2, 0.2), CustomLightProbe(normalDirection) + 0.5 * lightColor + additionalLights) + emission;
	half3 albedo = diffuse * max(half3(0.2, 0.2, 0.2), 0.5 * lightColor + additionalLights) + emission;
#ifdef _USE_MATCAP
			//MatCap
	albedo = ComputeMatcapColor(albedo, i.matcap);
			//return half4(albedo,1);
#endif
#else

	//if(ndl > 0 && occlusion.r > 0.75 && occlusion.a > 0.1)
	//{
		half perceptualRoughness = 1 - _Glossiness;
		half3 specularColor = _SpecularColor.rgb;

		//Blinn-Phong Specular (legacy)
		half3 h = normalize(lightDirection + viewDirection);
		half ndh = max(0, dot(normalDirection, h));
		half spec = pow(ndh, _Glossiness * 128.0) * 2.0 * occlusion.r;//smoothstep(0.6, 1,occlusion.r);
		half3 specular = lightColor * specularColor * spec;
		half specualr_scale = step(0.00001, ndl) *  step(0.75, occlusion.r) *  step(0.1, occlusion.a);// (1 - step(ndl, 0))* (1 - step(occlusion.r, 0.75))* (1 - step(occlusion.a, 0.01));
		diffuse += specular * specualr_scale;
	//}

	#ifdef _USE_RIM_NORMAL 
		diffuse = ComputeRimColorByNoV(diffuse, normalDirection, viewDirection, lightDirection, occlusion.a * lightColor.r * 0.5);
		//albedo = ComputeRimColor(albedo, normalDirection, i.posWorld, i.spos);
	#endif

	#ifdef _USE_RIM_DEPTH 
		diffuse = ComputeRimColorByDepth(diffuse, normalDirection, i.posWorld, i.spos, lightDirection, i.vsPos, i.ndcPos, occlusion.a * lightColor.r * 0.5);
	#endif
	half3 albedo = diffuse * max(half3(0.2, 0.2, 0.2),  0.5 * lightColor + additionalLights) + emission;
	//half3 albedo = diffuse * max(half3(0.2, 0.2, 0.2), CustomLightProbe(normalDirection) + 0.5 * lightColor + additionalLights) + emission;
	//albedo += CustomLightProbe(normalDirection);
	//albedo *= CustomLightProbe();// lerp(albedo, albedo * CustomLightProbe(), 0.5);
#endif
	//albedo = ComputeSSSColorByNoV(albedo, normalDirection, viewDirection, lightDirection);
#ifdef _FRESNEL_EFFECT
	albedo = ComputeFresnel(albedo, normalDirection, viewDirection, lightDirection);
#endif

#ifdef _DISSOLVE_EFFECT
	albedo = ComputeDissolve(albedo, i.uv, i.posWorld.xy);
#endif
	return half4(albedo, alpha);
}

#endif