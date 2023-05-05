Shader "NGRole/Cloth/Before Liquid"
{
	Properties
	{
		[Toggle(_USE_METALLIC)] _UseMetallic("Use Metallic Specular", Float) = 0
		[Toggle(_USE_HSV_SHADOW)] _UseHSVShadowColor("Use HSV Shadow Color", Float) = 0
		[Toggle(_USE_SHADOW_COLOR_TEXTURE)] _UseShadowColorTexture("Use Shadow Texture", Float) = 0
		[Toggle(_USE_RAMP_TEXTURE)] _UseRampTexture("Use Ramp Texture", Float) = 0
		[Toggle(_USE_CARTOON_SPECULAR)] _UseCartoonSpecular("Use Cartoon Specular", Float) = 0
		[Toggle(_USE_REFLECTION_MAP)] _UseReflectionMap("Use Reflection Map", Float) = 0		
		[HideInInspector] _UseCustomShadowMap("Use Custom ShadowMap", Float) = 0
		[Toggle(_USE_ANIMATION)]_UseAnimation("Use Animation", Float) = 0
		[HDR] _BaseColor("BaseColor Gain", Color) = (1.0,1.0,1.0,1.0)
		_Region1Color("Region 1 Color", Color) = (1.0,1.0,1.0,1.0)
		_Region2Color("Region 2 Color", Color) = (0.0,0.0,0.0,0.0)
		_Region3Color("Region 3 Color", Color) = (0.0,0.0,0.0,0.0)
		_Region4Color("Region 4 Color", Color) = (0.0,0.0,0.0,0.0)
		[NoScaleOffset]_Albedo("Albedo", 2D) = "white" {}
		[NoScaleOffset]_ColorPalette("Color Palette", 2D) = "black" {}
		[NoScaleOffset]_ShadowColorTexture("Shadow", 2D) = "white" {}
		[Normal][NoScaleOffset]_Normal("Normal", 2D) = "bump"{}
		[NoScaleOffset]_OcclusionMap("Occlusion", 2D) = "white" {}
		[NoScaleOffset]_Specular("Specular", 2D) = "black" {}
		[NoScaleOffset]_MetallicMap("Metallic", 2D) = "black" {}
		[NoScaleOffset]_Ramp("Ramp", 2D) = "white" {}
		_AnimationOffsetX("Animation Offset X",Float) = 0.333
		_AnimationOffsetY("Animation Offset Y",Float) = 0.25
		_ReflectionMap("Reflection Map", 2D) = "gray" {}
		_SColor("Region 1 First Shadow Color", Color) = (0.195,0.195,0.195,1.0)
		_SSColor("Region 1 Second Shadow Color", Color) = (0.3,0.3,0.3,1.0)
		_Region2SColor("Region 2 First Shadow Color", Color) = (0.195,0.195,0.195,1.0)
		_Region2SSColor("Region 2 Second Shadow Color", Color) = (0.3,0.3,0.3,1.0)
		_Region3SColor("Region 3 First Shadow Color", Color) = (0.195,0.195,0.195,1.0)
		_Region3SSColor("Region 3 Second Shadow Color", Color) = (0.3,0.3,0.3,1.0)
		_Region4SColor("Region 4 First Shadow Color", Color) = (0.195,0.195,0.195,1.0)
		_Region4SSColor("Region 4 Second Shadow Color", Color) = (0.3,0.3,0.3,1.0)
		_ShaodwColorH("ShadowColor H",Range(-360,360)) = 0
		_ShaodwColorS("ShadowColor S",Range(-1,1)) = 0
		_ShaodwColorV("ShadowColor V",Range(0,1)) = 0.5
		_ShaodwColorH2("Second ShadowColor H",Range(-360,360)) = 0
		_ShaodwColorS2("Second ShadowColor S",Range(-1,1)) = 0
		_ShaodwColorV2("Second ShadowColor V",Range(0,1)) = 0.3
		_RampThreshold("First Ramp Threshold", Range(0.001,1)) = 0.5
		_SRampThreshold("Second Ramp Threshold", Range(0,1)) = 0.1
		[HideInInspector]_SecondRampThresholdIntensity("Second Ramp Threshold Intensity",Range(0,1)) = 1
		_SecondRampVisableThreshold("Second Ramp Visable Threshold", Range(0,1)) = 0.1
		_RampSmooth("Ramp Smoothing", Range(0.001,1)) = 0.001
		_OcclusionAddThreshold("Occlusion Threshold", Range(0,1)) = 0.5
		_OcclusionAddIntensity("Occlusion Add Intensity", Range(0,5)) = 0
		_OcclusionSubIntensity("Occlusion Sub Intensity", Range(0,5)) = 0
		[Toggle(_RECIVE_SHADOW)]_ReciveShadow("Recive Shadow", Float) = 1
		_Bias("Shadow Bias",Range(0,2)) = 0.01
		_SpecularColor("Specular Color",Color) = (1,1,1,1)
		_MetallicIntensity("Metallic", Range(0, 1)) = 1
		_Roughness("Glossiness", Range(0.001, 1)) = 1
		_Hardness("Hardness", Range(0, 1)) = 0
		_SpecularThreshold("Specular Threshold",Range(0,60)) = 30
		_SpecularIntensity("Specular Intensity", Range(0,10)) = 1
		_OneMinusReflectivityIntensity("One Minus Reflectivity Intensity", Range(0,1)) = 1
		[HideInInspector]nbSamples("Reflection Samples", Range(1,16)) = 16
		_ReflectionIntensity("Reflection Intensity", Range(0,5)) = 1
		_FresnelThickness("Fresnel Thickness", Range(0, 5)) = 1
		environment_rotation("Environment Rotation", Range(0, 2)) = 0
		[HDR]_OutLineColor("OutLineColor", Color) = (0,0,0,1)
		_OutLineZOffset("OutLine Z Offset",Range(0,1)) = 0
		[Toggle(_OUTLINE_USE_VERTEXCOLOR)] _UseVertexColor("Use Vertex Color",Float) = 0
		[Toggle(_OUTLINE_UV2_AS_NORMALS)] _OutlineUV2AsNormals("UV2 As Normals", Float) = 0
		[Toggle(_USE_RIM)] _UseRim("Use Rim", Float) = 0
		[HDR]_RimColor("Rim Color", Color) = (0.8,0.8,0.8,0.6)
		_RimWidth("Rim Width", Range(1,25)) = 1
		_RimPosition("Rim Position",Vector) = (0.0,0.0,0.0,1.0)
		[HDR]_EmissionColor("Emission Color",Color) = (0,0,0,0)
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendNG("Blending Source", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlendNG("Blending Dest", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2
		[Toggle(_FLIP_BACKFACE_NORMAL)] _FlipBackFaceNormal("Flip Back Face Normal", Float) = 0
		[HideInInspector]_UseMatcapEffect("Use Matcap Effect",Float) = 0
		[HideInInspector]_UseTeleportEffect("Use Teleport Effect",Float) = 0
		[HideInInspector]_UseViewPortClip("Use ViewPort Clip",Float) = 0
		[HideInInspector]_VersionMode("VersionMode",Float) = 1
	}
	SubShader{
		Tags {"RenderType" = "Opaque" "Queue" = "AlphaTest+50" "RenderPipeline" = "UniversalPipeline"}
		Pass {
			Name "Cloth"
			Tags{"LightMode" = "ClothBeforeLiquid"}
			Blend[_SrcBlendNG][_DstBlendNG]
			Cull[_Cull]
			ZWrite On
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "../Frame/Cloth.hlsl"

			#pragma fragmentoption ARB_precision_hint_fastest       
			#pragma multi_compile _ _USE_METALLIC
			#pragma multi_compile _ _USE_CARTOON_METAL
			// -------------------------------------
			// Universal Pipeline Keywords
			#pragma multi_compile _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _MAIN_LIGHT_SHADOWS_CASCADE
			ENDHLSL
		}

		Pass{
			Name "ClothLow"
			Tags{"LightMode" = "ClothBeforeLiquidLow"}
			Blend[_SrcBlendNG][_DstBlendNG]
			Cull[_Cull]
			ZWrite On
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#define _LOW_QUALITY
			#include "../Frame/Cloth.hlsl"

			#pragma fragmentoption ARB_precision_hint_fastest      

			#pragma multi_compile _ _USE_METALLIC
			#pragma multi_compile _ _USE_CARTOON_METAL
			
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
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "../Frame/Outline.hlsl"

			#pragma fragmentoption ARB_precision_hint_fastest       
			#pragma multi_compile _ _OUTLINE_UV2_AS_NORMALS
			#pragma multi_compile _ _OUTLINE_USE_VERTEXCOLOR

			ENDHLSL
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