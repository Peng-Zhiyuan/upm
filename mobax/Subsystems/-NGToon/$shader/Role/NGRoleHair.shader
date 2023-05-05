Shader "NGRole/Hair"
{
	Properties
	{
		[Toggle(_LOW_QUALITY)]_UseLowQuality("Use Low Quality", Float) = 0
		[Toggle(_USE_HSV_SHADOW)] _UseHSVShadowColor("Use HSV Shadow Color", Float) = 0
		[Toggle(_USE_SHADOW_COLOR_TEXTURE)] _UseShadowColorTexture("Use Shadow Texture", Float) = 0
		[Toggle(_USE_RAMP_TEXTURE)] _UseRampTexture("Use Ramp Texture", Float) = 0
		[Toggle(_USE_CARTOON_METAL)] _UseCartoonMetal("Use Cartoon Metal Specular", Float) = 0
		[Toggle(_USE_DITHER)] _UseDither("Use Dither", Float) = 0

		[HDR] _BaseColor("BaseColor Gain", Color) = (1.0,1.0,1.0,1.0)
		_Region1Color("Region 1 Color", Color) = (1.0,1.0,1.0,1.0)
		_Region2Color("Region 2 Color", Color) = (0.0,0.0,0.0,0.0)
		_Region3Color("Region 3 Color", Color) = (0.0,0.0,0.0,0.0)
		_Region4Color("Region 4 Color", Color) = (0.0,0.0,0.0,0.0)
		[NoScaleOffset]_Albedo("Albedo", 2D) = "white" {}
		[NoScaleOffset]_ShadowColorTexture("Shadow", 2D) = "white" {}
		[Toggle(_USE_COLOR_PALETTE)] _UseColorPalette("Use Color Palette", Float) = 0
		[NoScaleOffset]_ColorPalette("Color Palette", 2D) = "white" {}
		[NoScaleOffset]_OcclusionMap("Occlusion", 2D) = "white" {}
		[NoScaleOffset]_Ramp("Ramp", 2D) = "white" {}
		_SColor("First Shadow Color", Color) = (0.195,0.195,0.195,1.0)
		_SSColor("Second Shadow Color", Color) = (0.3,0.3,0.3,1.0)
		_Region2SColor("Region 2 First Shadow Color", Color) = (0.195,0.195,0.195,1.0)
		_Region2SSColor("Region 2 Second Shadow Color", Color) = (0.3,0.3,0.3,1.0)
		_Region3SColor("Region 3 First Shadow Color", Color) = (0.195,0.195,0.195,1.0)
		_Region3SSColor("Region 3 Second Shadow Color", Color) = (0.3,0.3,0.3,1.0)
		_Region4SColor("Region 4 First Shadow Color", Color) = (0.195,0.195,0.195,1.0)
		_Region4SSColor("Region 4 Second Shadow Color", Color) = (0.3,0.3,0.3,1.0)
		
		_ShadowThreshould("Shadow Threshould",Range(0,1)) = 0.5
		_ShadowColorH("ShadowColor H",Range(-360,360)) = 0
		_ShadowColorS("ShadowColor S",Range(-1,1)) = 0
		_ShadowColorV("ShadowColor V",Range(0,1)) = 0.5
		
		_ShadowThreshould2("Shadow Threshould 2",Range(0,1)) = 0.5
		_ShadowColorH2("Second ShadowColor H",Range(-360,360)) = 0
		_ShadowColorS2("Second ShadowColor S",Range(-1,1)) = 0
		_ShadowColorV2("Second ShadowColor V",Range(0,1)) = 0.3
		_RampThreshold("First Ramp Threshold", Range(0.001,1)) = 0.5
		_RampThresholdSub("Ramp Threshold Sub", Range(0,1)) = 0.5
		_SRampThreshold("Second Ramp Threshold", Range(0,1)) = 0.1

		_OcclusionAddThreshold("Occlusion Threshold", Range(0,1)) = 0.5
		_OcclusionAddIntensity("Occlusion Add Intensity", Range(0,5)) = 0
		_OcclusionSubIntensity("Occlusion Sub Intensity", Range(0,5)) = 0
		_OcclusionShadowIntensity("Occlusion Shadow Intensity", Range(0,1)) = 0.1
		_OcclusionShadowThreshold("Occlusion Shadow Threshould", Range(0,1)) = 0.8

		[Toggle(_RECIVE_SHADOW)]_ReciveShadow("Receive Shadow", Float) = 1
		_Bias("Shadow Bias",Range(0,2)) = 0.01
		_SpecularColor("Specular Color",Color) = (1,1,1,1)
		
		_Roughness("Glossiness", Range(0.001, 1)) = 0.5
		_Hardness("Hardness", Range(0, 1)) = 1
		_SpecularIntensity("Specular Intensity", Range(0,10)) = 1
		[HDR]_OutLineColor("OutLineColor", Color) = (0,0,0,1)
		_OutLineZOffset("OutLine Z Offset",Range(0,1)) = 0
		_OutLineWidthOffset("OutLine Width Offset",Range(-1,1)) = 0
		[Toggle(_OUTLINE_USE_VERTEXCOLOR)] _UseVertexColor("Use Vertex Color",Float) = 0
		[Toggle(_OUTLINE_UV2_AS_NORMALS)] _OutlineUV2AsNormals("UV2 As Normals", Float) = 0
		[Toggle(_USE_RIM)] _UseRim("Use Rim", Float) = 1
		[HDR]_RimColor("Rim Color", Color) = (0.8,0.8,0.8,0.6)
		_RimWidth("Rim Width", Range(0,1)) = 0.1
		_RimMin("Rim Min", Range(0,1)) = 0
		_RimMax("Rim Max", Range(0,1)) = 1
		_RimThreshold("Rim Threshold", Range(0,1)) = 0.1	
		/*
		[Toggle(USE_SSS)]_UseSSS("Use SSS", Float) = 0
		[HDR]_SSSColor("SSS Color", Color) = (0.8,0.8,0.8,0.6)
		_SSSAtten("SSS Atten", Range(1,10)) = 2
		_SSSAttenScale("SSS Atten Scale", Range(1,10)) = 2
		*/

	
		[HDR]_EmissionColor("Emission Color",Color) = (0,0,0,0)
		[Toggle(_USE_STOCK)] _UseStock("Use Stock", Float) = 0
		//_StockingDenier("Denier", Range(0,1)) = 0.1
		_StockingDenier("Denier", Range(0,1)) = 0.1
        _StockingDenierTex("Denier Texture", 2D) = "black"{}
        _StockingRimPower("Rim Power", Range(0,30)) = 12
		_StockingFresnelScale("Fresnel Scale",Range(0, 6)) = 1
        _StockingFresnelMin("Fresnel Min Value",Range(0, 1)) = 0
        _StockingTint("Color Tint", Color) = (0,0,0,0)
		_StockingGlossiness("Glossiness", Range(0.001, 1)) = 0.5
		_StockingSpecular("Specular", Color) = (0,0,0,0)

		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendNG("Blending Source", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlendNG("Blending Dest", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2
		/*
		[HideInInspector][Toggle(_USE_MATCAP)]_UseMatcapEffect("Use Matcap Effect",Float) = 1

		[HideInInspector]_MatCapColor("_MatCapColor", Color) = (0.195,0.195,0.195,1.0)
		[HideInInspector]_MatCapPower("_MatCapPower", Range(0,1)) = 0.1

		[HideInInspector]_MatCapAlphaPower("_MatCapAlphaPower", Range(0,1)) = 0.1
		[HideInInspector]_MatCapMultiply("_MatCapMultiply", Range(0,1)) = 0.1
		[HideInInspector]_MatCapAdd("_MatCapAdd", Range(0,1)) = 0.1


		[NoScaleOffset]_MatCap("_MatCap", 2D) = "white" {}
		*/


		//[HideInInspector]_UseTeleportEffect("Use Teleport Effect",Float) = 0
		[HideInInspector]_UseViewPortClip("Use ViewPort Clip",Float) = 0
		[HideInInspector]_VersionMode("VersionMode",Float) = 1
		[HideInInspector]_RoleUp("Role Up",Vector) = (0,1,0,1)
		[HideInInspector]_RoleFront("Role Front",Vector) = (0,0,1,1)
		_CartoonMetal("CartoonMetal", 2D) = "white" {}
		_CartoonMetalUVrotate("CartoonMetal UVrotate", Float) = 0
		//_CustomLightDir("Custom Light Dir",Vector) = (0,0,0,0)
		//_CustomLightAtten("Custom Light Atten",Float) = 1
		//_CustomLightColor("Custom Light Color",Color) =(0,0,0,0)
		[HideInInspector]_OffsetPosition("Offset Position", Vector) = (0, 0, 0, 0)
		[HideInInspector]_OffsetProcess("Offset Process", Range(-3, 3)) = 0
		_CoverAlpha("Cover Alpha", Range(0.0, 1)) = 0.5
		_RolePosition("Role Position", Vector) = (0, 0, 0, 0)
		_CameraDistance("Camera Distance", Float) = 0

		//--------------------------Effects------------------------------//
		[Toggle(_FRESNEL_EFFECT)] _FresnelEffect("Fresnel effect", Float) = 0
		[HDR]_FresnelColor("Fresnel Color",Color) = (0,0,0,0)
		//_FresnelStart("Fresnel Start", Range(0,1)) = 0

		//[Toggle(_DISSOLVE_EFFECT)] _DissolveEffect("Dissolve effect", Float) = 0
		[HDR]_DissolveColor("Dissolve Color",Color) = (1,1,1,1)
		_DissolveRatio("Dissolve Ratio", Range(0,1)) = 0
		_DissolveHeight("Dissolve Height", Range(0,4)) = 4
		_DissolveRange("Dissolve Range", Range(0,0.1)) = 0.02
	
	}

	SubShader{
		Tags {"RenderType" = "Opaque" "Queue" = "AlphaTest+50" "RenderPipeline" = "UniversalPipeline"}
/*
		Pass {
				
				Name "EyeBrowG"
				Tags { "LightMode" = "EyebrowG" }
				Cull[_Cull]
				Blend SrcAlpha OneMinusSrcAlpha
				ZWrite On
				ZTest LEqual
				Stencil {
					Ref 1
					Comp Equal
					Pass Keep
					Fail Keep
				}
				HLSLPROGRAM
				
				#pragma vertex vert
				#pragma fragment frag
				//#pragma multi_compile  _LOW_QUALITY
				#define USE_COVER_ALPHA
				#include "../Frame/Eyebrow.hlsl"
				#pragma fragmentoption ARB_precision_hint_fastest       // 最快速精度
				
				// -------------------------------------
				// Material Keywords
				#pragma multi_compile _ _USE_DITHER
				#pragma shader_feature_local _USE_COLOR_PALETTE
				#pragma shader_feature_local _ _USE_HSV_SHADOW  _USE_NORMAL_SHADOW
				// -------------------------------------
				// Universal Pipeline Keywords
				#pragma multi_compile _MAIN_LIGHT_SHADOWS
				#pragma multi_compile _MAIN_LIGHT_SHADOWS_CASCADE
			
				ENDHLSL
			}
*/

			Pass {
				Name "HairBangs"
				Tags{"LightMode" = "EyebrowG"}
				Blend SrcAlpha OneMinusSrcAlpha
				//Cull[_Cull]
				ZWrite On
				ZTest LEqual
				Stencil {
					Ref 1
					Comp Equal
					Pass Keep
					Fail Keep
				}
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#define USE_COVER_ALPHA
				#include "../Frame/Cloth.hlsl"
				#pragma fragmentoption ARB_precision_hint_fastest
				// -------------------------------------
				// Material Keywords
				#pragma shader_feature_local _USE_METALLIC
				#pragma shader_feature_local _USE_CARTOON_METAL
				#pragma shader_feature_local _USE_RIM_DEPTH _USE_RIM_NORMAL
				#pragma shader_feature_local _USE_COLOR_PALETTE
				//#pragma shader_feature_local _USE_STOCK
				#pragma shader_feature_local _ _USE_HSV_SHADOW  _USE_NORMAL_SHADOW


				// -------------------------------------
				// Universal Pipeline Keywords
				#pragma multi_compile _MAIN_LIGHT_SHADOWS
				#pragma multi_compile _MAIN_LIGHT_SHADOWS_CASCADE

				// -------------------------------------
				// Custom Keywords
				#pragma multi_compile_local _ _LOW_QUALITY
				#pragma multi_compile_local _ _FRESNEL_EFFECT //_DISSOLVE_EFFECT
				#pragma multi_compile_local _ _USE_DITHER

				//#pragma multi_compile _ Anti_Aliasing_ON
				//#define _VIEW_CLIP
				#pragma multi_compile_local _ _USE_MATCAP
				//#pragma multi_compile _ _VIEW_CLIP
				ENDHLSL
			}

		Pass {
			Name "HairBangs"
			Tags{"LightMode" = "RoleHair"}
			Blend[_SrcBlendNG][_DstBlendNG]
			Cull[_Cull]
			ZWrite On
			ZTest LEqual
			Stencil {
					Ref 1
					Comp NotEqual
					Pass Keep
					Fail Keep
				}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "../Frame/Cloth.hlsl"
			#pragma fragmentoption ARB_precision_hint_fastest
			// -------------------------------------
			// Material Keywords
			#pragma shader_feature_local _USE_METALLIC
			#pragma shader_feature_local _USE_CARTOON_METAL
			#pragma shader_feature_local _USE_RIM_DEPTH _USE_RIM_NORMAL
			#pragma shader_feature_local _USE_COLOR_PALETTE
			//#pragma shader_feature_local _USE_STOCK
			#pragma shader_feature_local _ _USE_HSV_SHADOW  _USE_NORMAL_SHADOW


			// -------------------------------------
			// Universal Pipeline Keywords
			#pragma multi_compile _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _MAIN_LIGHT_SHADOWS_CASCADE

			// -------------------------------------
		    // Custom Keywords
			#pragma multi_compile_local _ _LOW_QUALITY
			#pragma multi_compile_local _ _FRESNEL_EFFECT //_DISSOLVE_EFFECT
			#pragma multi_compile_local _ _USE_DITHER

			//#pragma multi_compile _ Anti_Aliasing_ON
		    //#define _VIEW_CLIP
			#pragma multi_compile_local _ _USE_MATCAP
			//#pragma multi_compile _ _VIEW_CLIP
			ENDHLSL
		}

		Pass{
			Name "ClothLow"
			Tags{"LightMode" = "ClothLow"}
			Blend[_SrcBlendNG][_DstBlendNG]
			Cull[_Cull]
			ZWrite On

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#define _LOW_QUALITY
			#include "../Frame/Cloth.hlsl"
			#pragma fragmentoption ARB_precision_hint_fastest       
			#pragma shader_feature_local _USE_METALLIC
			#pragma shader_feature_local _USE_CARTOON_METAL
			
			ENDHLSL
		}

		Pass
		{
			Name "Outline"
			Tags { "LightMode" = "Outline" }
			Cull Front
			ZWrite On
			Stencil {
					Ref 1
					Comp NotEqual
					Pass Keep
					Fail Keep
				}
			HLSLPROGRAM
			#pragma vertex vert_outline
			#pragma fragment frag_outline
			//#define _LOW_QUALITY
			//#define _OUTLINE_UV2_AS_NORMALS
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "../Frame/Outline.hlsl"

			#pragma fragmentoption ARB_precision_hint_fastest      
			#pragma multi_compile _OUTLINE_UV2_AS_NORMALS
			#pragma multi_compile_local _ _USE_DITHER
			#pragma multi_compile_local _LOW_QUALITY
			//#pragma multi_compile _ _OUTLINE_USE_VERTEXCOLOR
			//#pragma multi_compile _ _DISSOLVE_EFFECT
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

            // more explict render state to avoid confusion
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