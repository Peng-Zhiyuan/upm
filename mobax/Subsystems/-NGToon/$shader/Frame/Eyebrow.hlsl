#ifndef ROLE_EYEBROW_INCLUDED
#define ROLE_EYEBROW_INCLUDED

#include "CustomLightFunc.hlsl"

#ifdef _LOW_QUALITY
#include "Matcap.hlsl"
#else

#include "ShadowMap.hlsl"
#include "RimColor.hlsl"
#endif

#include "GGX.hlsl"
#include "ColorPalette.hlsl"

#ifdef _USE_DITHER
#include "Dither.hlsl"
#endif


#ifdef _DISSOLVE_EFFECT
#include "DissolveEffect.hlsl"
#endif

//#include "SSS.hlsl"
sampler2D _OcclusionMap;
sampler2D _Albedo;
half4 _Albedo_ST;

//uniform sampler2D _Normal;
sampler2D _ColorPalette;

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


half4 _EmissionColor;
half4 _BaseColor;

#ifdef USE_COVER_ALPHA
half _CoverAlpha;
#endif
half _RampThreshold;
half _RampThresholdSub;
half _SRampThreshold;
half _RampSmooth;

half4 _CustomLightDir;
half4 _CustomLightColor;
half _CustomLightAtten;
half4 _RolePosition;
half _CameraDistance;
half _GRoleLightFreeDir;
half _GRoleLightAtten;
half4 _GRoleLightDir;
half4 _GRoleLightColor;
half4 _GRoleShadowColor;
half4 _GRoleSecondShadowColor;

half Offset(half uv, half vaule)
{
	half final = max(0, (uv - 1)) * vaule + uv + (min(0, uv)) * vaule;
	return final;
}

struct appdata {
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
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
	half3 viewPostion :TEXCOORD5;
#endif

#else
	half3 tangentDir : TEXCOORD3;
	half3 bitangentDir : TEXCOORD4;
	half4 shadowCoord : TEXCOORD5;
	half4 spos :TEXCOORD6;
	float4 ndcPos:TEXCOORD7;
	float3 vsPos:TEXCOORD8;
#ifdef _VIEW_CLIP
	half3 viewPostion :TEXCOORD9;
#endif

#ifdef _USE_DITHER
	half cameraDistance:TEXCOORD9;
#endif
#endif
};

v2f vert(appdata v) {
	v2f o = (v2f)0;
	o.uv = v.uv;
	o.posWorld = TransformObjectToWorld(v.vertex.xyz);
	o.pos = TransformWorldToHClip(o.posWorld);

#ifdef _LOW_QUALITY
	o.normalDir = TransformObjectToWorldNormal(v.normal);
	//MatCap
#ifdef _USE_MATCAP
	COMPUTE_MATCAP_DATA(v.normal, o.matcap)
#endif

#else
	VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normal, v.tangent);
	o.normalDir = normalInputs.normalWS;
	o.tangentDir = normalInputs.tangentWS;
	o.bitangentDir = normalInputs.bitangentWS;
	//shadow
	//o.shadowCoord = mul(_LightProjection, half4(o.posWorld, 1));
	//o.shadowCoord.z = -(mul(_LightDepthProjection, half4(o.posWorld, 1)).z * _farplaneScale);
#ifdef MAIN_LIGHT_SHADOWS
	o.shadowCoord = TransformWorldToShadowCoord(_RolePosition.xyz);

	o.spos = ComputeScreenPos(o.pos);
#endif
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
		alpha *= _CoverAlpha;
#endif
		#ifdef _USE_DITHER
			alpha *= saturate((_CameraDistance - 1) / 2);
			ditherClip(i.spos.xy / i.spos.w, alpha);
		#endif

		half3 normalDirection = normalize(i.normalDir);

		normalDirection *= lerp(-1,1, step(0, facing));




		half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

		half3 lightDirection = lerp(_MainLightPosition.xyz, _GRoleLightDir.xyz, _GRoleLightDir.w); //normalize(_MainLightPosition).xyz;
		lightDirection = lerp(lightDirection, _CustomLightDir.xyz, _CustomLightDir.w);

		lightDirection.y = lerp(length(_MainLightPosition.xz), lightDirection.y, _GRoleLightFreeDir);
		lightDirection = normalize(lightDirection);

		half3 lightColor = lerp(_MainLightColor.rgb, _GRoleLightColor.rgb * _GRoleLightAtten, _GRoleLightColor.a);//_MainLightColor.rgb
		lightColor = lerp(lightColor, _CustomLightColor.rgb * _CustomLightAtten, _CustomLightColor.a);
#ifdef MAIN_LIGHT_SHADOWS
		half shadow = MainLightRealtimeShadow(i.shadowCoord);
		lightColor = lightColor * lerp(0.5, 1,saturate(shadow));
#endif

		half vdl = dot(normalize(viewDirection.xz), normalize(lightDirection.xz)) * 0.5 + 0.5;
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

#ifndef _LOW_QUALITY//_NO_SPECULAR
		half static_specular = step(0.75, occlusion.r);
		half static_specular_intensity = smoothstep(0.75,1,occlusion.r);

		half flow_specular = step(occlusion.b, 0.25);
		half flow_specular_intensity = smoothstep(0.75, 1, 1 - occlusion.b);

		half cartoon_specular = step(occlusion.r, 0.1);
		half cartoon_specular_intensity = smoothstep(0.75, 1,  1 - occlusion.r);

		half show_specular = step(0.1, occlusion.a);
		half gloss = _Roughness * min(1, flow_specular + static_specular);

		half perceptualRoughness = 1.0 - gloss;
		half roughness = perceptualRoughness * perceptualRoughness;
		half3 halfDirection = normalize(viewDirection + lightDirection);
#endif
		half sRampThreshold = _RampThreshold + _SRampThreshold;
	
		half ramp2 = lerp(1, smoothstep(sRampThreshold - _RampSmooth * 0.5, sRampThreshold + _RampSmooth * 0.5, ndl), step(0.9, occlusion.a));//����ɫ����
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

		
			half3 fsColor = sColorCount.rgb;
			half3 ssColor = ssColorCount.rgb;

			fsColor = lerp(fsColor,  fsColor * _GRoleShadowColor, _GRoleShadowColor.a);
			ssColor = lerp(ssColor,  ssColor * _GRoleSecondShadowColor, _GRoleSecondShadowColor.a);
			half3 secondColor = lerp(ssColor, half3(1, 1, 1), ramp2);
			half3 shadowColor = lerp(fsColor, secondColor, ramp1);
			diffuse *= shadowColor * (1 - occAdd * _OcclusionShadowIntensity * step(1 - _OcclusionShadowThreshold,  1 - (ndl_origin * 0.5 + 0.5)));
			half3 additionalLights = 0;

			#ifndef _NO_ADDITIONALLIGHTS
				uint pixelLightCount = GetAdditionalLightsCount();
				for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
				{
					Light light = GetCustomAdditionalLight(lightIndex, i.posWorld);
					half ndl = NDL(normalDirection, light.direction, occOffset);
					ndl = smoothstep(sRampThreshold - _RampSmooth * 0.5, sRampThreshold + _RampSmooth * 0.5, ndl);
					additionalLights += light.color * light.distanceAttenuation * ndl * 0.1;
				}
				diffuse += additionalLights;
			#endif
				//emission
			half3 emission = step(0.99,occlusion.b) * _EmissionColor.rgb * mainTex.rgb;
			emission = 0.5 * emission + 0.5 * emission * lightColor;

	#ifndef _LOW_QUALITY

				half LdotH = saturate(dot(lightDirection, halfDirection));
				half NdotV = abs(dot(normalDirection, viewDirection));
				half NdotH = saturate(dot(normalDirection, halfDirection));
				//specular
				half oneMinusReflectivity;
				half3 specularColor = (_SpecularColor.rgb * gloss);
				diffuse = CustomEnergyConservationBetweenDiffuseAndSpecular(diffuse, specularColor, oneMinusReflectivity);
				half specularPBL = GGXSpecularPBL(saturate(ndl), NdotV, NdotH, roughness);

				half specular_intensity = static_specular * static_specular_intensity;
				half inlight = step(0.01, ramp1);

				half flow_specularPBL = step(0.5 , max(specularPBL * flow_specular_intensity, specular_intensity));
				flow_specularPBL = lerp(flow_specularPBL * 0.5, flow_specularPBL, inlight);//�����߹����

				half static_specularPBL = step(0.1 * _Hardness, specularPBL) * specular_intensity;

				specularPBL = lerp(static_specularPBL, flow_specularPBL,flow_specular);
				half3 directSpecular = specularPBL * _SpecularIntensity * CustomFresnelTerm(specularColor, LdotH) * show_specular;
				//half3 specular = lightColor * directSpecular;
			
				half3 specular = lightColor * directSpecular;
				half3 albedo = diffuse + specular;
		
	  #endif



	  #ifdef _LOW_QUALITY
		   half3 albedo = diffuse * max(half3(0.2, 0.2, 0.2),  lightColor * 0.5 + additionalLights) + emission;
		   #ifdef _USE_MATCAP
			   //MatCap
			   albedo = ComputeMatcapColor(albedo, i.matcap);
		   #endif
	   #else		
		   albedo = albedo * max(half3(0.2, 0.2, 0.2), lightColor * 0.5 + additionalLights) + emission;
	   #endif


	   #ifdef _DISSOLVE_EFFECT
		   albedo = ComputeDissolve(albedo, i.posWorld);
	   #endif
	   return half4(albedo,  alpha);
}

#endif