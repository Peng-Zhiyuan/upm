#ifndef ROLE_FACE_INCLUDED
#define ROLE_FACE_INCLUDED

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
#ifdef _USE_DITHER
#include "Dither.hlsl"
#endif
#ifdef _FRESNEL_EFFECT
#include "FresnelEffect.hlsl"
#endif

#ifdef _DISSOLVE_EFFECT
#include "DissolveEffect.hlsl"
#endif

//#include "SSS.hlsl"

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
#ifdef _LOW_QUALITY

	half2 matcap : TEXCOORD3;
	half4 shadowCoord : TEXCOORD4;
	#if defined(_USE_DITHER)
		half4 spos :TEXCOORD5;
		half cameraDistance:TEXCOORD6;
	#endif
	#ifdef _VIEW_CLIP
		half3 viewPostion :TEXCOORD5;
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

half4 _BaseColor;
sampler2D _Albedo;
half4 _Albedo_ST;


sampler2D _OcclusionMap;
half4 _OcclusionMap_TexelSize;
sampler2D _Ramp;

half4 _SColor;
half _RampThreshold;
half _RampThresholdSub;
half _RampSmooth;

half4 _FaceUp;
half4 _FaceFront;
//half _SpecalShadowDisappear;

half _ShadowThreshold;
half _OcclusionAddThreshold;
half _OcclusionAddIntensity;
half _OcclusionSubIntensity;

//half4 _EmissionColor;

half4 _OffsetPosition;
half _OffsetProcess;

half4 _SpecularColor;
half _Glossiness;
half4 _CustomLightDir;
half _CustomLightAtten;
half4 _CustomLightColor;
half4 _RolePosition;
half _CameraDistance;
half _UseMask;
half _Flip;
half _AsymmetricalSpecular;
half _GRoleLightFreeDir;
half _GRoleLightAtten;
half4 _GRoleLightDir;
half4 _GRoleLightColor;
half4 _GRoleShadowColor;

inline half GetNdlByLightDirectionSDF(half3 normalDirection, half3 Left, half3 lightDirection, half2 uv, half3 occlusion)
{
    half4 l_ilmTex = step(_Flip,0.01) * tex2D(_OcclusionMap, uv) + step(0.01, _Flip) * tex2D(_OcclusionMap, half2(1 - uv.x, uv.y));
    half4 r_ilmTex = step(0.01, _Flip) * tex2D(_OcclusionMap, uv) + step(_Flip, 0.01) * tex2D(_OcclusionMap, half2(1 - uv.x, uv.y));
    half2 Front = _FaceFront.xz;
    half2 LeftDir = half2(Front.y, -Front.x);
    half2 LightDir = normalize(-lightDirection.xz);
	bool isLeft = dot(LightDir, LeftDir) > 0;
    half ctrl =  clamp(0, 1, dot(Front, LightDir) * 0.5 + 0.5);
    half ilm = isLeft ? l_ilmTex.g : r_ilmTex.g;
    half ndl = 1- step(ilm, ctrl);
	return ndl;
}

inline half GetNdlByLightDirection(half3 normalDirection, half3 Left, half3 lightDirection, half2 uv, half3 occlusion)
{
	half FoL = dot(normalize(_FaceFront.xz), normalize(lightDirection.xz));
	half LoL = dot(normalize(Left.xz), normalize(lightDirection.xz));
    bool isLeft = LoL > 0;
    half ndl = NDL_XZ(normalDirection, lightDirection, 0);
	ndl = step(_RampThreshold, ndl);
	return ndl;
}

v2f vert(appdata v) {
	v2f o = (v2f)0;
	o.posWorld = half4(TransformObjectToWorld(v.vertex.xyz), 1);
	o.pos = TransformWorldToHClip(o.posWorld.xyz);
	o.uv = v.uv;
	o.normalDir = TransformObjectToWorldNormal(v.normal);

#ifdef _LOW_QUALITY
	//MatCap
#ifdef _USE_MATCAP
	COMPUTE_MATCAP_DATA(v.normal, o.matcap)
#endif

#else
	VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
	o.ndcPos = vertexInput.positionNDC;
	o.vsPos = vertexInput.positionVS;
	//shadow
	//o.shadowCoord = mul(_FaceLightProjection, o.posWorld);
	//o.shadowCoord.z = -(mul(_FaceLightDepthProjection, o.posWorld).z * _FaceFarplaneScale);
#ifdef MAIN_LIGHT_SHADOWS
	o.shadowCoord = TransformWorldToShadowCoord(_RolePosition.xyz);
#endif
	o.spos = ComputeScreenPos(o.pos);
	//o.spos.xy /= o.spos.w;
	float3 vPos = TransformWorldToView(o.posWorld);
	o.spos.z = -vPos.z * _ProjectionParams.w;
	#ifdef _USE_DITHER
		o.cameraDistance = length(_WorldSpaceCameraPos - o.posWorld);
	#endif
#endif
#ifdef _VIEW_CLIP
	COMPUTE_VIEWPORTCLIP_DATA(o.viewPostion, o.posWorld)
#endif
	return o;
}


half4 frag(v2f i, half facing : VFACE) : SV_Target{
	#ifdef _VIEW_CLIP
		VIEWPORT_CLIP(i.viewPostion)
	#endif
	half4 mainTex = tex2D(_Albedo, TRANSFORM_TEX(i.uv, _Albedo));
	half alpha = mainTex.a;
#ifdef _USE_DITHER
	//alpha *= saturate(_CameraDistance / 5);
	alpha *= saturate((_CameraDistance - 1) / 2);
	ditherClip(i.spos.xy / i.spos.w, alpha);
#endif

	//half3 lightColor = _MainLightColor.rgb;
	half3 normalDirection = normalize(i.normalDir);
		
	half3 lightDirection = lerp(_MainLightPosition.xyz, _GRoleLightDir.xyz, _GRoleLightDir.w); //normalize(_MainLightPosition).xyz;
	lightDirection = lerp(lightDirection, _CustomLightDir.xyz, _CustomLightDir.w);

	lightDirection.y = lerp(length(_MainLightPosition.xz), lightDirection.y, _GRoleLightFreeDir);    
	lightDirection = normalize(lightDirection);

	half3 lightColor = lerp(_MainLightColor.rgb, _GRoleLightColor.rgb * _GRoleLightAtten, _GRoleLightColor.a);//_MainLightColor.rgb
	lightColor = lerp(lightColor, _CustomLightColor.rgb * _CustomLightAtten, _CustomLightColor.a);
	
	//half4 SHADOW_COORDS = TransformWorldToShadowCoord(_RolePosition.     xyz);
	//half shadow = MainLightRealtimeShadow(SHADOW_COORDS);
#ifdef MAIN_LIGHT_SHADOWS
	half shadow = MainLightRealtimeShadow(i.shadowCoord);
	lightColor = lightColor * lerp(0.65, 1,saturate(shadow));
#endif
	half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
	_RampThreshold = lerp(_RampThreshold - _RampThresholdSub, _RampThreshold, dot(normalize(viewDirection.xz), normalize(lightDirection.xz)) * 0.5 + 0.5);
	//_RampThreshold = lerp(_RampThreshold - _RampThresholdSub, _RampThreshold, dot(normalize(viewDirection.xz), normalize(lightDirection.xz)) * 0.5 + 0.5);
	//_RampThreshold = lerp(0, _RampThreshold, dot(normalize(viewDirection.xz), normalize(lightDirection.xz)) * 0.5 + 0.5);
#ifndef _LOW_QUALITY
	
	//half3 front = normalize(TransformObjectToWorldDir(half3(0, 0, 1)).xyz);
	//half3 up = normalize(TransformObjectToWorldDir(half3(0, 1, 0)).xyz);
	//half3 front = normalize(TransformObjectToWorldDir(half3(0, 1, 0)).xyz);
	//half3 up = normalize(TransformObjectToWorldDir(half3(-1, 0, 0)).xyz);
    //_FaceFront = half4(front, 0);
	//_FaceUp = half4(up,0);
	half3 Left = cross(_FaceUp.xyz, _FaceFront.xyz);
	half atten = 1;

	//if (_ReciveShadow > 0)
	//{
	//	atten = GetShadowAttenuate(i.shadowCoord.xyz, _FaceShadowDepthMap, _Bias, _FaceShadowDepthMap_TexelSize.x * 0.8, _FaceShadowDepthMap_TexelSize.y * 0.8);
	//}


	half4 occlusion = tex2D(_OcclusionMap, i.uv).rgba;
	half occAdd = step(occlusion.r, _OcclusionAddThreshold);
	half occSub = step(_OcclusionAddThreshold, occlusion.r);
	occAdd = lerp(0, (_OcclusionAddThreshold - occlusion.r) * _OcclusionAddIntensity, occAdd);
	occSub = lerp(0, (occlusion.r - _OcclusionAddThreshold) * _OcclusionSubIntensity, occSub);
	half occOffset = occSub - occAdd;
#ifdef	_USE_SDF
	half ndl = GetNdlByLightDirectionSDF(normalDirection, Left, lightDirection, i.uv, occlusion.rgb);
#else
	half ndl = GetNdlByLightDirection(normalDirection, Left, lightDirection, i.uv, occlusion.rgb);
#endif
	ndl += occOffset;

	ndl -= max(0,_UseMask) * step(occlusion.b, 0.45);//_UseMask > 0 && occlusion.b <= 0.45
	
	ndl = saturate(min(ndl, atten));
#else

    half ndl_xz = NDL_XZ(normalDirection, lightDirection, 0);//NDL(normalDirection, lightDirection, 0);
	half ndl = smoothstep(_RampThreshold - _RampSmooth * 0.5, _RampThreshold + _RampSmooth * 0.5, ndl_xz);
#endif
	
	_SColor.rgb = lerp(_SColor.rgb, _SColor.rgb * _GRoleShadowColor,_GRoleShadowColor.a);
#ifndef _LOW_QUALITY
	#ifdef _USE_RAMP
		half3 rampTexture = tex2D(_Ramp, half2(ndl, ndl)).rgb;
		half3 shadowColor = lerp(lerp(_SColor.rgb, half3(1, 1, 1), ndl), half3(1, 1, 1), rampTexture);
	#else
		half3 shadowColor = lerp(_SColor.rgb, half3(1, 1, 1), ndl);//lerp(_SColor.rgb, half3(1, 1, 1), ndl);
	#endif

#else
	half3 shadowColor =  lerp(_SColor.rgb, half3(1, 1, 1), ndl);//lerp(lerp(_SColor.rgb, half3(1, 1, 1), ndl), half3(1, 1, 1), half3(0.3137, 0.3137, 0.3137));//_SColor.rgb * (1 - ndl);
#endif

    //shadowColor = lerp(_SColor.rgb,1, ndl);


	half3 diffuse = mainTex.rgb * _BaseColor.xyz * shadowColor;//lightColor;
    half3 additionalLights = 0;

#ifndef _NO_ADDITIONALLIGHTS
	uint pixelLightCount = GetAdditionalLightsCount();

	for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
	{
		Light light = GetCustomAdditionalLight(lightIndex, i.posWorld);
		half ndl = NDL(normalDirection, light.direction, 0);
		//ndl = smoothstep(_RampThreshold - _RampSmooth * 0.5, _RampThreshold + _RampSmooth * 0.5, ndl);
		additionalLights += light.color * light.distanceAttenuation * ndl * 0.01;
	}
#endif

#ifndef _LOW_QUALITY

	#ifdef _USE_RIM_NORMAL 
		diffuse = ComputeRimColorByNoV(diffuse, normalDirection, viewDirection, lightDirection, occlusion.a * lightColor.r * 0.5);
		//albedo = ComputeRimColor(albedo, normalDirection, i.posWorld, i.spos);
	#endif

	#ifdef _USE_RIM_DEPTH 
		diffuse = ComputeRimColorByDepth(diffuse, normalDirection, i.posWorld, i.spos, lightDirection, i.vsPos, i.ndcPos, occlusion.a * lightColor.r * 0.5);
	#endif
	//diffuse = ComputeSSSColorByNoV(diffuse, normalDirection, viewDirection, lightDirection);
	
		//half perceptualRoughness = 1 - _Glossiness;
		//if (ndl >= 0 && occlusion.r >= 0.75 && occlusion.g >= 0.01)
		//{
			half3 specularColor = _SpecularColor.rgb;

			//Blinn-Phong Specular (legacy)
			half3 h = normalize(lightDirection + viewDirection);
			half ndh = max(0.01f, dot(normalDirection, h));
		
			half spec = pow(ndh, _Glossiness * 128.0) * 2.0 * occlusion.r;//smoothstep(0.6, 1,occlusion.r);
			half3 specular = lightColor * specularColor * spec;
			diffuse += specular * step(0.75,occlusion.r) * step(0,ndl);// * step(0.01, occlusion.g);

		//}
		
#ifndef	_USE_SDF
	half FoL = dot(normalize(_FaceFront.xz), normalize(lightDirection.xz));
	{
	
		half ndl_xz = NDL_XZ(normalDirection, lightDirection, 0); 
		half LoL = dot(normalize(Left.xz), normalize(lightDirection.xz));
		half lightLeft = step(0, LoL);
		half uvLeft = step(i.uv.x, 0.5);

		half3 specularColor = _SpecularColor.rgb;
		half spec = pow(1, _Glossiness * 128.0) * 2.0 ;//smoothstep(0.6, 1,occlusion.r);
		half3 specular = lightColor * specularColor * spec;
		diffuse += specular * step(0.99, occlusion.b) * step(_AsymmetricalSpecular, FoL) * step(abs(lightLeft - uvLeft), 0.01);

	}
#endif
	//half3 albedo = diffuse * max(half3(0.2, 0.2, 0.2), CustomLightProbe(normalDirection) + 0.5 * lightColor + additionalLights);
	half3 albedo = diffuse * max(half3(0.2, 0.2, 0.2), 0.5 * lightColor + additionalLights);
#else
	//MatCap
	//half3 albedo = diffuse * max(half3(0.2, 0.2, 0.2), CustomLightProbe(normalDirection) + 0.5 * lightColor + additionalLights);
	half3 albedo = diffuse * max(half3(0.2, 0.2, 0.2), 0.5 * lightColor + additionalLights);
#ifdef _USE_MATCAP
	//MatCap
	albedo = ComputeMatcapColor(albedo, i.matcap);
#endif
#endif

#ifdef _FRESNEL_EFFECT
	  albedo = ComputeFresnel(albedo, normalDirection, viewDirection, lightDirection);
#endif

#ifdef _DISSOLVE_EFFECT
	albedo = ComputeDissolve(albedo, i.uv, i.posWorld.xy);
#endif
	return half4(albedo, alpha);
}

#endif