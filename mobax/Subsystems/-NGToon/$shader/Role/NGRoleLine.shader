Shader "NGRole/Line"
{
	Properties
	{
		[HDR] _BaseColor("BaseColor Gain", Color) = (1.0,1.0,1.0,1.0)
		[NoScaleOffset]_Albedo("Albedo", 2D) = "white" {}
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendNG("Blending Source", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlendNG("Blending Dest", Float) = 0
		_SColor("Shadow Color", Color) = (0.195,0.195,0.195,1.0)	
		_RampThreshold("Ramp Threshold", Range(0.001,1)) = 0.5
		_RampSmooth("Ramp Smoothing", Range(0,1)) = 0.01

		//_OutLineZOffset("OutLine Z Offset",Range(0,1)) = 0
		
		[HideInInspector]_UseViewPortClip("Use ViewPort Clip",Float) = 0
		//_CustomLightDir ("Custom Light Dir",Vector) = (0,0,0,0)
		
		[HideInInspector]_VersionMode("VersionMode",Float) = 1

		[Toggle(_ALPHA_ANIM)] _AlphaAnim("Alpha anim", Float) = 0
		 _AlphaAnimSpeed("Alpha anim speed", Float) = 1
		 _AlphaAnimMin("Alpha anim min", Range(0,1)) = 0
		 _AlphaAnimMax("Alpha anim max", Range(0,1)) = 1
		//--------------------------Effects------------------------------//
		[Toggle(_FRESNEL_EFFECT)] _FresnelEffect("Fresnel effect", Float) = 0
		[HDR]_FresnelColor("Fresnel Color",Color) = (0,0,0,0)
		_FresnelStart("Fresnel Start", Range(0,1)) = 0

		//[Toggle(_DISSOLVE_EFFECT)] _DissolveEffect("Dissolve effect", Float) = 0
		[HDR]_DissolveColor("Dissolve Color",Color) = (1,1,1,1)
		_DissolveRatio("Dissolve Ratio", Range(0,1)) = 0
		_DissolveHeight("Dissolve Height", Range(0,4)) = 4
		_DissolveRange("Dissolve Range", Range(0,0.1)) = 0.02
	}

	SubShader{
			  
		//Tags {"RenderType" = "Opaque" "Queue" = "AlphaTest+50" "RenderPipeline" = "UniversalPipeline"}
		Tags {"RenderType" = "Opaque" "Queue" = "AlphaTest+60" "RenderPipeline" = "UniversalPipeline"}
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
			Tags { "LightMode" = "Cloth" }
			Cull[_Cull]
			ZWrite On
			Blend[_SrcBlendNG][_DstBlendNG]
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "../Frame/Line.hlsl"
			
			#pragma fragmentoption ARB_precision_hint_fastest   
			// Material Keywords
			#pragma shader_feature_local _ _USE_RIM_DEPTH _USE_RIM_NORMAL
			#pragma shader_feature_local _ _USE_RAMP

			// Custom Keywords
			#pragma multi_compile_local _ _FRESNEL_EFFECT //_DISSOLVE_EFFECT
			#pragma multi_compile_local _ _ALPHA_ANIM //_DISSOLVE_EFFECT
			ENDHLSL
		}
	}
	Fallback Off
	CustomEditor "PunishingInspector"
}

