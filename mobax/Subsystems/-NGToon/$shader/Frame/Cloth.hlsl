#ifndef ROLE_CLOTH_INCLUDED
#define ROLE_CLOTH_INCLUDED

#include "CustomLightFunc.hlsl"

#ifdef _LOW_QUALITY
#include "Matcap.hlsl"
#else

#include "ShadowMap.hlsl"
#include "RimColor.hlsl"
#endif

#include "GGX.hlsl"
#include "SPReflection.hlsl"
#include "ColorPalette.hlsl"

#if defined(_USE_FLOW_MAP)|| defined(_DISSOLVE_EFFECT)
#include "FlowMap.hlsl"
#endif

#ifdef _USE_DITHER
#include "Dither.hlsl"
#endif

#ifdef _USE_STOCK
#include "Stocking.hlsl"
#endif

#if defined(_FRESNEL_EFFECT) || defined(_DISSOLVE_EFFECT)
#include "FresnelEffect.hlsl"
#endif

#ifdef _DISSOLVE_EFFECT
#include "DissolveEffect.hlsl"
#endif

//#include "SSS.hlsl"

sampler2D _Albedo;
half4 _Albedo_ST;
#ifdef _USE_SHADOW_COLOR_TEXTURE
sampler2D _ShadowColorTexture;
#endif

//uniform sampler2D _Normal;
sampler2D _OcclusionMap;
sampler2D _ColorPalette;
/*
#ifdef _USE_METALLIC
sampler2D _MetallicMap;
half _MetallicIntensity;
#else
sampler2D _Specular;
#endif
*/
#ifdef _USE_RAMP_TEXTURE
sampler2D _Ramp;
#endif

sampler2D _CartoonMetal;
half4 _CartoonMetal_ST;


half _RampThreshold;
half _RampThresholdSub;
half _SRampThreshold;
half _RampSmooth;
//half _SecondRampThresholdIntensity;
//half _Glossiness;
//half _SecondRampVisableThreshold;

half _OcclusionAddThreshold;
half _OcclusionAddIntensity;
half _OcclusionSubIntensity;

half _OcclusionShadowIntensity;
half _OcclusionShadowThreshold;

half4 _SpecularColor;
half _Roughness;

half _Hardness;
half _SpecularThreshold;

half _SpecularIntensity;
//half _ReflectionIntensity;
half _FresnelThickness;

half4 _EmissionColor;
half _EmissionAnim;
half4 _RoleUp;
half4 _RoleFront;
half4 _BaseColor;
//half _AnimationOffsetX;
//half _AnimationOffsetY;
int nbSamples;
half environment_rotation;
half _OneMinusReflectivityIntensity;
half _UseAnimation;

//half _FlipBackFaceNormal;

half _CartoonMetalUVrotate;
//half _RoleEulerAnglesY;

half4 _OffsetPosition;
half _OffsetProcess;
half4 _CustomLightDir;
half4 _CustomLightColor;
half _CustomLightAtten;

#ifdef USE_COVER_ALPHA
half _CoverAlpha;
#endif


half4 _RolePosition;
half _CameraDistance;
half _GRoleLightFreeDir;
half _GRoleLightAtten;
half4 _GRoleLightDir;
half4 _GRoleLightColor;
half4 _GRoleShadowColor;
half4 _GRoleSecondShadowColor;

half Offset (half uv,half vaule) 
{
	half final = max(0,(uv-1)) * vaule + uv + (min(0,uv))* vaule ;
	return final;
}

struct appdata {
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	half3 normal : NORMAL;
	half4 tangent : TANGENT;
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
		half3 viewPostion :TEXCOORD7;
	#endif
	
#else
	//half3 tangentDir : TEXCOORD3;
	//half3 bitangentDir : TEXCOORD4;
	half4 shadowCoord : TEXCOORD3;
	half4 spos :TEXCOORD4;
	float4 ndcPos:TEXCOORD5;
	float3 vsPos:TEXCOORD6;
	#ifdef _VIEW_CLIP
		half3 viewPostion :TEXCOORD7;
	#endif

	#ifdef _USE_DITHER
		half cameraDistance:TEXCOORD8;
	#endif
#endif
#ifdef _DISSOLVE_EFFECT
		float2 uv1:TEXCOORD9;
#endif

};

v2f vert(appdata v) {
	v2f o = (v2f)0;
	o.uv = v.uv;
#ifdef _DISSOLVE_EFFECT
	o.uv1 = v.uv1;
#endif
	o.posWorld = TransformObjectToWorld(v.vertex.xyz);
	o.pos = TransformWorldToHClip(o.posWorld);

#ifdef _LOW_QUALITY
	o.normalDir = TransformObjectToWorldNormal(v.normal);
	//MatCap
	#ifdef _USE_MATCAP
		COMPUTE_MATCAP_DATA(v.normal, o.matcap)
	#endif

#else
	#ifdef _USE_RIM_DEPTH 
	VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
	o.ndcPos = vertexInput.positionNDC;
	o.vsPos = vertexInput.positionVS;
	#endif
	VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normal, v.tangent);
	o.normalDir = normalInputs.normalWS;
	// o.tangentDir = normalInputs.tangentWS;
	//o.bitangentDir = normalInputs.bitangentWS;
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

half4 frag(v2f i, half facing : VFACE) : SV_Target{

	half4 mainTex = tex2D(_Albedo, TRANSFORM_TEX(i.uv, _Albedo)) * _BaseColor;
	half alpha = mainTex.a;
#ifdef USE_COVER_ALPHA
	alpha *=(1 - _CoverAlpha);
#endif
#ifdef _USE_DITHER
	//alpha *= saturate(i.cameraDistance/5.0);
	alpha *= saturate((_CameraDistance -1)/ 2);
	ditherClip(i.spos.xy / i.spos.w, alpha);
#endif

	half3 normalDirection = normalize(i.normalDir);
	//if (facing < 0)
	//{
		// clip(-1);
		//return half4(1,1,1,0);
		normalDirection *= lerp(-1,1, step(0, facing)) ;
	//}

	//#if defined(_LOW_QUALITY) && defined(_USE_MATCAP)
		//MatCap
	//	return half4(ComputeMatcapColor(mainTex.rgb, i.matcap),1);
	//#endif

	/*
	#ifdef _LOW_QUALITY
		half3 normalDirection = normalize(i.normalDir);
	#else
		i.normalDir = normalize(i.normalDir);
		half3x3 tangentTransform = half3x3(i.tangentDir, i.bitangentDir, i.normalDir);
		half3 _NormalMap_var = UnpackNormal(tex2D(_Normal,i.uv));
		half3 normalLocal = _NormalMap_var.rgb;
		half3 normalDirection = normalize(mul(normalLocal, tangentTransform)); // Perturbed normals
		if (facing < 0 && _FlipBackFaceNormal > 0)
		{
			normalDirection = -normalDirection;
		}
	#endif
	*/

	half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

	half3 lightDirection = lerp(_MainLightPosition.xyz, _GRoleLightDir.xyz, _GRoleLightDir.w); //normalize(_MainLightPosition).xyz;
	lightDirection = lerp(lightDirection, _CustomLightDir.xyz, _CustomLightDir.w);

	lightDirection.y = lerp(length(_MainLightPosition.xz), lightDirection.y, _GRoleLightFreeDir);    
	lightDirection = normalize(lightDirection);

	half3 lightColor = lerp(_MainLightColor.rgb, _GRoleLightColor.rgb * _GRoleLightAtten, _GRoleLightColor.a);//_MainLightColor.rgb
	lightColor = lerp(lightColor, _CustomLightColor.rgb * _CustomLightAtten, _CustomLightColor.a);

	//float4 SHADOW_COORDS = TransformWorldToShadowCoord(_RolePosition.xyz);
	//half shadow = MainLightRealtimeShadow(SHADOW_COORDS);
#ifdef MAIN_LIGHT_SHADOWS
	half shadow = MainLightRealtimeShadow(i.shadowCoord);
	lightColor = lightColor * lerp(0.65, 1,saturate(shadow));
#endif
//#ifndef _LOW_QUALITY
//	half atten = 1;

	//if (_ReciveShadow > 0)
	//{
	//	atten = GetShadowAttenuate(i.shadowCoord.xyz, _ShadowDepthMap, _Bias, _ShadowDepthMap_TexelSize.x * 0.8, _ShadowDepthMap_TexelSize.y * 0.8);
	//}
	
//#endif
	half vdl =  dot(normalize(viewDirection.xz), normalize(lightDirection.xz)) * 0.5 + 0.5;
	_RampThreshold = lerp(_RampThreshold - _RampThresholdSub, _RampThreshold, vdl);
	half3 templightDir = half3(lightDirection.x, lerp(lightDirection.y * 0, lightDirection.y, vdl), lightDirection.z);
	half4 occlusion = tex2D(_OcclusionMap, i.uv);
	half ndl_origin = dot(normalDirection, templightDir);
	half ndl = ndl_origin;
	half occAdd = step(occlusion.g, _OcclusionAddThreshold);
	half occSub = step(_OcclusionAddThreshold, occlusion.g);
	occAdd = lerp(0, (_OcclusionAddThreshold - occlusion.g) * _OcclusionAddIntensity, occAdd);
	occSub = lerp(0, (occlusion.g - _OcclusionAddThreshold) * _OcclusionSubIntensity, occSub);
	half occOffset = occSub - occAdd;
	ndl += occOffset;
//#ifndef _LOW_QUALITY
//	ndl *= atten;
//#endif
#ifndef _LOW_QUALITY//_NO_SPECULAR
/*
	#ifdef _USE_METALLIC
		half4 metallicMap = tex2D(_MetallicMap, i.uv);
		half gloss = 1 - _Roughness * metallicMap.g;
		half metallic = _MetallicIntensity * metallicMap.r;
	#else
		half4 specularMask = tex2D(_Specular, i.uv);
		half gloss = _Roughness * specularMask.a;
	#endif
*/
	//half gloss = _Roughness * step(occlusion.b, 0.1) * step(0.1, occlusion.a);//min(1, step(occlusion.r, 0.1) + step(occlusion.b, 0.1)) * step(0.1, occlusion.a);
	half static_specular = step(0.75, occlusion.r);
	half static_specular_intensity = smoothstep(0.75,1,occlusion.r);

	//half static_specular = step(0.65 + lerp(0.35,0,saturate(ndl)), occlusion.r);
	half flow_specular = step(occlusion.b, 0.25);
	half flow_specular_intensity = smoothstep(0.75, 1, 1 - occlusion.b);

    half cartoon_specular = step(occlusion.r, 0.1);
	half cartoon_specular_intensity = smoothstep(0.75, 1,  1 - occlusion.r);



	half show_specular = step(0.1, occlusion.a);// * min(1, flow_specular + static_specular + cartoon_specular);
	//return half4(show_specular,show_specular,show_specular,1);
	half gloss = _Roughness * min(1, flow_specular + static_specular + cartoon_specular);// * show_specular;
	//_SpecularIntensity *= show_specular * lerp(1, static_specular_intensity, static_specular * step(ndl, 0));
	//_SpecularIntensity *= lerp(1, static_specular_intensity, static_specular * step(ndl, 0));
	half perceptualRoughness = 1.0 - gloss;
	half roughness = perceptualRoughness * perceptualRoughness;
	half3 halfDirection = normalize(viewDirection + lightDirection);
	
#endif

	//_RampThreshold = lerp(0, _RampThreshold, dot(viewDirection,lightDirection) * 0.5 + 0.5);
	half sRampThreshold = _RampThreshold + _SRampThreshold;// * _SecondRampThresholdIntensity;
	//half ramp2 = lerp(1, smoothstep(sRampThreshold - _RampSmooth * 0.5, sRampThreshold + _RampSmooth * 0.5, ndl), step(_SecondRampVisableThreshold, occlusion.r));
	half ramp2 = lerp(1, smoothstep(sRampThreshold - _RampSmooth * 0.5, sRampThreshold + _RampSmooth * 0.5, ndl), step(0.9, occlusion.a));//二阶色开关
	half ramp1 = smoothstep(_RampThreshold - _RampSmooth * 0.5, _RampThreshold + _RampSmooth * 0.5, ndl);
	

#if defined(_USE_HSV_SHADOW) || defined(_USE_NORMAL_SHADOW)
	half3 diffuse;
	half3 sColorCount;
	half3 ssColorCount;
	half4 palette = tex2D(_ColorPalette, i.uv);
	GetColorByColorPalette(mainTex, palette, diffuse, sColorCount, ssColorCount);
#else
	half3 diffuse = mainTex.rgb;
	half3 sColorCount = _SColor.rgb;
	half3 ssColorCount = _SSColor.rgb;
#endif

#ifdef _USE_SHADOW_COLOR_TEXTURE
	half4 shadowTex = tex2D(_ShadowColorTexture, i.uv);
	half3 fsColor = sColorCount.rgb * shadowTex.rgb;
	half3 ssColor = ssColorCount.rgb * shadowTex.rgb;

	fsColor = lerp(fsColor,  fsColor * _GRoleShadowColor, _GRoleShadowColor.a);
	ssColor =  lerp(ssColor,  ssColor * _GRoleSecondShadowColor, _GRoleSecondShadowColor.a);
#else
	half3 fsColor = sColorCount.rgb;
	half3 ssColor = ssColorCount.rgb;
	
	fsColor = lerp(fsColor,  fsColor * _GRoleShadowColor, _GRoleShadowColor.a);
	ssColor =  lerp(ssColor,  ssColor * _GRoleSecondShadowColor, _GRoleSecondShadowColor.a);
#endif
//		secondColor = lerp(secondColor, _GRoleSecondShadowColor,_GRoleSecondShadowColor.a);
//	shadowColor = lerp(shadowColor, _GRoleShadowColor,_GRoleShadowColor.a);
/*
#ifdef _USE_HSV_SHADOW
	#ifdef _USE_RAMP_TEXTURE
	ndl = lerp(ndl + saturate(1 - sRampThreshold), ndl + saturate(1 - (_RampThreshold + _SRampThreshold)), step(ndl, _RampThreshold));
	half3 rampTexture = tex2D(_Ramp, half2(ndl, ndl)).rgb;
	half3 secondColor = lerp(ssColor, diffuse, ramp2);
	diffuse = lerp(lerp(fsColor, secondColor, ramp1), diffuse, rampTexture);
	#else
	half3 secondColor = lerp(ssColor, diffuse, ramp2);
	diffuse = lerp(fsColor, secondColor, ramp1);
	#endif
#else
*/

#ifdef _USE_RAMP_TEXTURE	
	ndl = lerp(ndl + saturate(1 - sRampThreshold), ndl + saturate(1 - (_RampThreshold + _SRampThreshold)), step(ndl, _RampThreshold));
	half3 rampTexture = tex2D(_Ramp, half2(ndl, ndl)).rgb;
	half3 secondColor = lerp(ssColor, half3(1, 1, 1), ramp2);
	half3 shadowColor = lerp(lerp(fsColor, secondColor, ramp1), half3(1, 1, 1), rampTexture);
#else
	half3 secondColor = lerp(ssColor, half3(1, 1, 1), ramp2);
	half3 shadowColor = lerp(fsColor, secondColor, ramp1);
#endif

#if defined(_USE_STOCK) && !defined(_LOW_QUALITY)
	half stock = step(0.25, occlusion.r) * step(occlusion.r, 0.35);
	diffuse *= lerp(shadowColor * (1 - occAdd * _OcclusionShadowIntensity * step(1 - _OcclusionShadowThreshold, 1 - (ndl_origin * 0.5 + 0.5))), 1, stock);
#else
	diffuse *= shadowColor * (1 - occAdd * _OcclusionShadowIntensity * step(1 - _OcclusionShadowThreshold, 1 - (ndl_origin * 0.5 + 0.5)));
#endif
	
	
//#endif

    half3 additionalLights = 0;

#ifndef _NO_ADDITIONALLIGHTS
	uint pixelLightCount = GetAdditionalLightsCount();
	for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
	{
		Light light = GetCustomAdditionalLight(lightIndex, i.posWorld);
		half ndl = NDL( normalDirection, light.direction, occOffset);
		ndl = smoothstep(sRampThreshold - _RampSmooth * 0.5, sRampThreshold + _RampSmooth * 0.5, ndl);
		additionalLights += light.color * light.distanceAttenuation * ndl * 0.1;
	}
	diffuse += additionalLights;
	//diffuse += CustomLightProbe(normalDirection);
#endif


	//emission
	//half3 emission = step(0.99, occlusion.b) * _EmissionColor.rgb * mainTex.rgb * lerp(1,(sin(_EmissionAnim * _Time.y) * 0.5  + 0.5), step(0.01,_EmissionAnim));
	half3 emission = step(0.99, occlusion.b) * _EmissionColor.rgb * mainTex.rgb;
	emission = 0.5 * emission + 0.5 * emission * lightColor;
	//emission = emission +emission * lightColor;

#ifndef _LOW_QUALITY//_NO_SPECULAR
	
	half LdotH = saturate(dot(lightDirection, halfDirection));
	half NdotV = abs(dot(normalDirection, viewDirection));
	half NdotH = saturate(dot(normalDirection, halfDirection));//max(saturate(dot(normalDirection, halfDirection)) * step(occlusion.b, 0.1), step(0.75, occlusion.r) * step(0.01, ramp2));
	//half VdotH = saturate(dot(viewDirection, halfDirection))
	
//#ifdef _USE_STOCK
//	half3 albedo = CalculateStock(diffuse, saturate(ndl), NdotH, NdotV, i.uv, lightColor);
//#else
	//specular
	half oneMinusReflectivity;
	half3 specularColor = (_SpecularColor.rgb * gloss);
	diffuse = CustomEnergyConservationBetweenDiffuseAndSpecular(diffuse, specularColor, oneMinusReflectivity);
	//half glossiness = max(1-roughness ,0.01);
	//half specularPBL = pow(NdotH, glossiness * 256);

	half specularPBL = GGXSpecularPBL(saturate(ndl), NdotV, NdotH, roughness);
	#if !defined(_LOW_QUALITY) && defined(_USE_CARTOON_METAL)// && defined(_USE_METALLIC)
		//卡通金属高光
		half3 front = _RoleFront.xyz;//normalize(TransformObjectToWorldDir(half3(0, 0, -1)).xyz);
		half3 up = _RoleUp.xyz;//normalize(TransformObjectToWorldDir(half3(0, 1, 0)).xyz);
		//half3 front = normalize(TransformObjectToWorldDir(half3(0, 0, -1)).xyz);
		//half3 up = normalize(TransformObjectToWorldDir(half3(0, 1, 0)).xyz);
		half3 lightDir = lightDirection;
		lightDir.y = 0;
		lightDir = normalize(lightDir);
		half FdotL = dot(front, lightDir);
		half FCrossL = dot(cross(front, lightDir),up);
		half roleEulerAnglesY = (1 - FdotL)* 0.25 * step(FCrossL,0)+(0.5 + (FdotL + 1)* 0.25) * step(0, FCrossL);

		half2 CMuv = float2(i.uv.x * cos(-_CartoonMetalUVrotate) -  i.uv.y * sin(-_CartoonMetalUVrotate), i.uv.x * sin(-_CartoonMetalUVrotate) +  i.uv.y*cos(-_CartoonMetalUVrotate));
		//half2 teeee = half2( Offset(_CartoonMetal_ST.z,_RoleEulerAnglesY), Offset(_CartoonMetal_ST.w,_RoleEulerAnglesY));
		//half2 teeee = half2(0, _CartoonMetal_ST.w + roleEulerAnglesY);
		half2 teeee = half2(Offset(_CartoonMetal_ST.z,roleEulerAnglesY), Offset(_CartoonMetal_ST.w,roleEulerAnglesY));
		half3 cartoonMetal = tex2D(_CartoonMetal, CMuv* _CartoonMetal_ST.xy + _CartoonMetal_ST.zw + teeee ).rgb;
	#endif

	half specular_intensity = max(static_specular * static_specular_intensity, cartoon_specular * cartoon_specular_intensity);
	half specular_tag =  max(static_specular, cartoon_specular);
	half inlight = step(0.01, ramp1);

	half flow_specularPBL = step(0.5 , max(specularPBL * flow_specular_intensity, specular_intensity));
	flow_specularPBL = lerp(flow_specularPBL * 0.5, flow_specularPBL, inlight);//暗部高光减弱

	half static_specularPBL = step(0.1 * _Hardness, specularPBL) * specular_intensity;

	specularPBL = lerp(static_specularPBL, flow_specularPBL,flow_specular);

	half3 directSpecular = specularPBL *  _SpecularIntensity * CustomFresnelTerm(specularColor, LdotH) * show_specular;
	

	
#ifndef _LOW_QUALITY//_NO_REFLECTTION`

	//Blinn-Phong Specular (legacy)
	//float perceptualRoughness = 1 - _Glossiness;
	/*
	half mask = step(0.75, occlusion.r) * step(0.1, occlusion.a) * step(0.01, ramp1);

	half3 h = normalize(lightDirection + viewDirection);
	half ndh = max(0, dot(normalDirection, h));
	half glossiness = max(_Roughness - 0.5,0.01);
	half spec = pow(ndh, glossiness * 256 ) * _SpecularIntensity * mask * occlusion.r;
    directSpecular = max(directSpecular, _SpecularColor.rgb * spec);
	*/

    //directSpecular = max(directSpecular, mask * CustomFresnelTerm(mask, LdotH));
	half3  indirectSpecular = half3(0,0,0);
	#ifdef _USE_CARTOON_METAL
	indirectSpecular = half3(1,1,1) * max(0.1, inlight) * cartoon_specular * smoothstep(0, 10, _SpecularIntensity) * show_specular;
	#endif

	half3 specular = lightColor * (directSpecular + indirectSpecular);
#else

	half3 specular = lightColor * directSpecular;
#endif

	#if !defined(_LOW_QUALITY) && defined(_USE_CARTOON_METAL)// && defined(_USE_METALLIC)
		 // half3 albedo = diffuse + lerp(specular,specular * cartoonMetal, 1 - metallicMap.a)+ emission;
		  half3 albedo = diffuse + lerp(specular,specular * cartoonMetal, cartoon_specular);
	#else
		  half3 albedo = diffuse + specular;// + emission;
	#endif
#ifdef _USE_STOCK
	 albedo = lerp(albedo, CalculateStock(diffuse, saturate(ndl), NdotH, NdotV, i.uv, lightColor), stock);
#endif
//#endif
#endif


//half backSSS = saturate(dot(normalDirection, -lightDirection));
//albedo = lerp(albedo, albedo +1, smoothstep(0.9, 1,backSSS));
#ifdef _LOW_QUALITY
	
		//MatCap
		//albedo = ComputeMatcapColor(albedo, i.matcap);
		//albedo = lerp(albedo, albedo * CustomLightProbe(), 0.5);
		//albedo += CustomLightProbe(normalDirection);
	//half3 albedo = diffuse * max(half3(0.2, 0.2, 0.2), CustomLightProbe(normalDirection) +  lightColor * 0.5 + additionalLights) + emission;
	half3 albedo = diffuse * max(half3(0.2, 0.2, 0.2),  lightColor * 0.5 + additionalLights) + emission;
	#ifdef _USE_MATCAP
		//MatCap
		albedo = ComputeMatcapColor(albedo, i.matcap);
		//return half4(albedo,1);
	#endif
#else		
	
	#ifdef _USE_RIM_NORMAL
		albedo = ComputeRimColorByNoV(albedo, normalDirection, viewDirection, lightDirection, smoothstep(0, 0.4,occlusion.a) * lightColor.r * 0.5);
	#endif
	#ifdef _USE_RIM_DEPTH 
		albedo = ComputeRimColorByDepth(albedo, normalDirection, i.posWorld, i.spos, lightDirection, i.vsPos, i.ndcPos, smoothstep(0, 0.4,occlusion.a) * lightColor.r * 0.5);
	#endif
	albedo = albedo * max(half3(0.2, 0.2, 0.2), lightColor * 0.5 + additionalLights) + emission;
	//albedo = albedo * max(half3(0.2, 0.2, 0.2), CustomLightProbe(normalDirection) + lightColor * 0.5 + additionalLights) + emission;
	//half3 albedo = diffuse * max(half3(0.2, 0.2, 0.2), lightColor * 0.5 + additionalLights) + emission;
	//albedo = lerp(albedo, albedo * CustomLightProbe(), 0.5);
#endif
//albedo = ComputeSSSColorByNoV(albedo, normalDirection, viewDirection, lightDirection);

#if defined(_USE_FLOW_MAP)|| defined(_DISSOLVE_EFFECT)
	albedo = CalculateFlowLight(albedo, i.uv, i.posWorld, i.pos, NdotV);
#endif


#if defined(_FRESNEL_EFFECT) || defined(_DISSOLVE_EFFECT)
	albedo = ComputeFresnel(albedo, normalDirection, viewDirection, lightDirection);
#endif

#ifdef _DISSOLVE_EFFECT
	albedo = ComputeDissolve(albedo, i.uv, i.uv1);
#endif

	//half occAddVal2 = step(occlusion.g, _OcclusionAddThreshold);
	//albedo = occAddVal2 * _OcclusionShadowIntensity * step( 1 - _OcclusionShadowThreshold,  1 - (ndl_origin * 0.5 + 0.5));
	return half4(albedo, alpha);
}

#endif