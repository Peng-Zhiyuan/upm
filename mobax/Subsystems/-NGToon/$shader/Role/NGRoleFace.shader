Shader "NGRole/Face"
{
	Properties
	{
		[Toggle(_USE_RAMP)]_UseRampTexture("Use Ramp Texture", Float) = 0
		[Toggle(_LOW_QUALITY)]_UseLowQuality("Use Low Quality", Float) = 0
		[Toggle(_USE_SDF)] _Use_SDF("Use SDF", Float) = 0
		[Toggle(_Flip)] _Flip("Flip", Float) = 0
		[HDR] _BaseColor("BaseColor Gain", Color) = (1.0,1.0,1.0,1.0)
		[NoScaleOffset]_Albedo("Albedo", 2D) = "white" {}
		[NoScaleOffset]_OcclusionMap("Occlusion", 2D) = "white" {}
		[Toggle(_USE_DITHER)] _UseDither("Use Dither", Float) = 0
		[NoScaleOffset]_Ramp("Ramp", 2D) = "white" {}	
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2
		[Toggle(_RECIVE_SHADOW)]_ReciveShadow("Recive Shadow", Float) = 1

		_SColor("Shadow Color", Color) = (0.195,0.195,0.195,1.0)	
		_RampThreshold("Ramp Threshold", Range(0.001,1)) = 0.5
		_RampThresholdSub("Ramp Threshold Sub", Range(0,1)) = 0
		_RampSmooth("Ramp Smoothing", Range(0,1)) = 0.01
		_ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.183
		
		_Bias("Shadow Bias",Range(0,2)) = 0.01
		_OcclusionAddThreshold("Occlusion Threshold", Range(0,1)) = 0.5
		_OcclusionAddIntensity("Occlusion Add Intensity", Range(0,5)) = 0
		_OcclusionSubIntensity("Occlusion Sub Intensity", Range(0,5)) = 0
		
		[HDR]_OutLineColor("OutLineColor", Color) = (0,0,0,1)

		_OutLineZOffset("OutLine Z Offset",Range(0,1)) = 0
		_OutLineWidthOffset("OutLine Width Offset",Range(-1,1)) = 0
		[Toggle(_OUTLINE_USE_VERTEXCOLOR)] _UseVertexColor("Use Vertex Color",Float) = 0
		[Toggle(_OUTLINE_UV2_AS_NORMALS)] _OutlineUV2AsNormals("UV2 As Normals", Float) = 0
		[Toggle(_USE_RIM)] _UseRim("Use Rim", Float) = 1
		[Toggle(_EYEBROW)] _Eyebrow("Eyebrow", Float) = 0

		[HDR]_RimColor("Rim Color", Color) = (0.8,0.8,0.8,0.6)
		_RimWidth("Rim Width", Range(0.001,1)) = 0.005
		_RimMin("Rim Min", Range(0,1)) = 0
		_RimMax("Rim Max", Range(0,1)) = 1
		_RimThreshold("Rim Threshold", Range(0,1)) = 0.1
		[HDR]_EmissionColor("Emission Color",Color) = (0,0,0,0)

/*
		[Toggle(USE_SSS)]_UseSSS("Use SSS", Float) = 0
		[HDR]_SSSColor("SSS Color", Color) = (0.8,0.8,0.8,0.6)
		_SSSAtten("SSS Atten", Range(1,10)) = 2
		_SSSAttenScale("SSS Atten Scale", Range(1,10)) = 2
*/

		[HideInInspector]_FaceUp("Face Up",Vector) = (0,1,0,1)
		[HideInInspector]_FaceFront("Face Front",Vector) = (0,0,1,1)
		[HideInInspector]_UseMatcapEffect("Use Matcap Effect",Float) = 0
		//[HideInInspector]_UseTeleportEffect("Use Teleport Effect",Float) = 0
		[HideInInspector]_UseViewPortClip("Use ViewPort Clip",Float) = 0

		_OffsetPosition ("Offset Position",Vector) = (0,0,0,0)
	
		_OffsetProcess ("Offset Process",Range(-3,3)) = 0
		//_CustomLightDir ("Custom Light Dir",Vector) = (0,0,0,0)
		//_CustomLightAtten("Custom Light Atten",Float) = 0
		_SpecularColor("Specular Color",Color) = (1,1,1,1)
		_Glossiness("Glossiness", Range(0.001, 1)) = 0.5
		_AsymmetricalSpecular("Specular Threshold",Range(0,1)) = 0.967
		[HideInInspector]_VersionMode("VersionMode",Float) = 1
		 _UseMask("Use Mask", Float) = 0
		[HideInInspector]_RolePosition("Role Position",Vector) = (0,0,0,0)

		//--------------------------Effects------------------------------//
		[Toggle(_FRESNEL_EFFECT)] _FresnelEffect("Fresnel effect", Float) = 0
		[HDR]_FresnelColor("Fresnel Color",Color) = (0,0,0,0)
		_FresnelStart("Fresnel Start", Range(0,1)) = 0

		[Toggle(_DISSOLVE_EFFECT)] _DissolveEffect("Dissolve effect", Float) = 0
		[HDR]_DissolveColor("Dissolve Color",Color) = (1,1,1,1)
		_DissolveRatio("Dissolve Ratio", Range(0,1)) = 0
		_DissolveHeight("Dissolve Height", Range(0,4)) = 4
		_DissolveRange("Dissolve Range", Range(0,0.1)) = 0.02

	}

	SubShader{
		Tags {"RenderType" = "Opaque" "Queue" = "AlphaTest+50" "RenderPipeline" = "UniversalPipeline"}
/*
		Stencil
		{
			Ref 1
			Pass Replace
			Fail Replace
			ZFail Replace
		}
*/
		Pass {
			Name "Forward"
			Tags { "LightMode" = "RoleForward" }
			Cull[_Cull]
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "../Frame/Face.hlsl"
	
			#pragma fragmentoption ARB_precision_hint_fastest       // 最快速精度
		    // Material Keywords
			#pragma shader_feature_local _ _USE_RIM_DEPTH _USE_RIM_NORMAL
			#pragma shader_feature_local _USE_RAMP
			#pragma shader_feature_local _USE_SDF

			// Universal Pipeline Keywords
			#pragma _MAIN_LIGHT_SHADOWS _ _MAIN_LIGHT_SHADOWS
			#pragma _MAIN_LIGHT_SHADOWS _ _MAIN_LIGHT_SHADOWS_CASCADE

		   //Custom Keywords
			#pragma multi_compile_local _ _USE_DITHER
			#pragma multi_compile_local _ _USE_MASK
			#pragma multi_compile_local _ _LOW_QUALITY
			#pragma multi_compile_local _ _FRESNEL_EFFECT _DISSOLVE_EFFECT 
			//#pragma multi_compile _ _USE_MATCAP

			ENDHLSL
		}

		Pass{
			Name "ForwardLow"
			Tags { "LightMode" = "RoleForwardLow" }
			Cull[_Cull]
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#define _LOW_QUALITY
			#include "../Frame/Face.hlsl"

			#pragma fragmentoption ARB_precision_hint_fastest     
			
			ENDHLSL
		}

		Pass
		{
			Name "Outline"
			Tags { "LightMode" = "Outline" }
			Cull Front
			ZWrite On
			HLSLPROGRAM
			#pragma vertex vert_outline
			#pragma fragment frag_outline
			//#define _OUTLINE_UV2_AS_NORMALS
			#define _USE_FILTER
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "../Frame/Outline.hlsl"

			#pragma fragmentoption ARB_precision_hint_fastest     
			#pragma multi_compile  _OUTLINE_UV2_AS_NORMALS
			#pragma multi_compile_local _ _DISSOLVE_EFFECT
			//#pragma multi_compile _ _OUTLINE_USE_VERTEXCOLOR
			#pragma multi_compile_local _ _USE_DITHER
			ENDHLSL
		}

		Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
           	Cull Back
        }
		
		Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On // the only goal of this pass is to write depth!
            ZTest LEqual // early exit at Early-Z stage if possible            
            ColorMask 0 // we don't care about color, we just want to write depth, ColorMask 0 will save some write bandwidth
            Cull Back // support Cull[_Cull] requires "flip vertex normal" using VFACE in fragment shader, which is maybe beyond the scope of a simple tutorial shader
        }
		
		/*
		Pass
		{
			Name "PlaneShadow"
			Tags { "LightMode" = "PlaneShadow" }
			Blend SrcAlpha  OneMinusSrcAlpha
			ZWrite Off
			Cull Back

			Stencil
			{
				Ref 0
				Comp Equal
				WriteMask 255
				ReadMask 255
				Pass Invert
				Fail Keep
				ZFail Keep
			}

			HLSLPROGRAM

			#pragma vertex vert_plane_shadow
			#pragma fragment frag_plane_shadow

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "../Frame/PlaneShadow.hlsl"

			ENDHLSL
		}
		*/
	}
	Fallback Off
	CustomEditor "PunishingInspector"
}