Shader "SoulCraft/Spine/Sprite1229"
{  
	Properties
	{ 
		_MainTex("Main Texture", 2D) = "white" {}
		[HDR]_Color("Color", Color) = (1,1,1,1)

		_TintColor("TintColor",Color) = (1,1,1,1)
		_TintOffset("TintOffset",Range(0,1)) = 0
		_GrayOffset("GrayOffset",Range(0,1)) = 0
		_GrayDir("GrayDir",Range(0,1)) = 0
		_GrayColorOffset("GrayOffset",Range(0,2)) = 0.33


			//_AoOffset2("Ao Offset1", Range(0,1)) = 0.5
		_SpeOffset1("Spe Offset1", Range(0,50)) = 0.5
		_SpeOffset2("Spe Offset2", Range(0,10)) = 0.5
		_SpePow("Spe Pow", Range(1,100)) = 1
			//_SpeOffset2("Spe Offset2", Range(0,10)) = 0.5
		_EmitOffset("Emit Offset", Range(0,5)) = 3
		_EmitScanOffset("Emit Scan Offset", Range(0,1)) = 0
		_EmitScanPower("Emit Scan Power", Range(0,100)) = 10

		


		_otherLightMax("other Light Max", Range(1,5)) = 2


		_LightDirAdjust("_LightDirAdjust", Vector) = (0,0,1,0)


			//????
		_HolyLightColor("HolyLightColor",Color) = (1,1,1,1)
		_HolyLightY("HolyLightY",Range(-1,1)) =0
		_HolyLightOffset("HolyLightOffset",Range(0,5)) = 0

//		_RoleHeight("_RoleHeight",Range(0,20)) = 1
//		_RoleRoot("_RoleRoot",Range(0,10))=0
		_RootDarkLimit("_RootDarkLimit",Range(0,1))=0.2


		[HDR]_EmmisionColor("_EmmisionColor",Color) = (1,1,1,1)



		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0





		_MetalMap("Metal Map", 2D) = "black" {}



		_FixedNormal("Fixed Normal", Vector) = (0,0,1,1)
		_ZWrite("Depth Write", Float) = 0.0
		_Cutoff("Depth alpha cutoff", Range(0,1)) = 0.0

		_CustomRenderQueue("Custom Render Queue", Float) = 0.0

	 




		
		
		_GuideH("GuideHeight",Range(0,20))=0.9


		
		_PShadowHeight("_PShadowHeight", Float) = 0
        _PShadowColor("_PShadowColor", Color) = (0,0,0,1)
	    _PShadowFalloff("_PShadowFalloff", Range(0,1)) = 0.05
        _PSLightDir("_PSLightDir", Vector) = (0,0,0,1)




		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _RenderQueue("__queue", Float) = 0.0
		[HideInInspector] _Cull("__cull", Float) = 0.0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Compare", Float) = 0.0 // Disabled stencil test by default
	}

	SubShader
	{
		// Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
		// this Subshader will fail.
		Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Sprite" "AlphaDepth"="False" "CanUseSpriteAtlas"="True" "IgnoreProjector"="True" }
		LOD 500

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}
		
		
		
//		Stencil {
//                Ref 6
//                Comp always
//                Pass replace
//		}
		
		// ----------------------------------------------------------------
		//  Forward pass.
		Pass
		{
			// Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
			// no LightMode tag are also rendered by Universal Render Pipeline
			Name "ForwardLit"
			Tags{"LightMode" = "UniversalForward"}
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			Cull[_Cull]

			
			

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard SRP library
			// All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature _ _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ADDITIVEBLEND _ADDITIVEBLEND_SOFT _MULTIPLYBLEND _MULTIPLYBLEND_X2
			#pragma shader_feature _ _FIXED_NORMALS_VIEWSPACE _FIXED_NORMALS_VIEWSPACE_BACKFACE _FIXED_NORMALS_MODELSPACE _FIXED_NORMALS_MODELSPACE_BACKFACE _FIXED_NORMALS_WORLDSPACE
			#pragma shader_feature _ _SPECULAR _SPECULAR_GLOSSMAP
			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _ALPHA_CLIP
			#pragma shader_feature _METAL_TEX
			#pragma shader_feature _SUB_TEX
			#pragma shader_feature _DIFFUSE_RAMP
			#pragma shader_feature _ _FULLRANGE_HARD_RAMP _FULLRANGE_SOFT_RAMP _OLD_HARD_RAMP _OLD_SOFT_RAMP
			#pragma shader_feature _COLOR_ADJUST
			#pragma shader_feature _TEXTURE_BLEND
			#pragma shader_feature _RECEIVE_SHADOWS_OFF
			#pragma shader_feature_local _HOLY_LIGHT
			#pragma shader_feature_local _MAGIC_ON
			#pragma shader_feature_local _SPE_DEBUG


			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA

			// -------------------------------------
			// Universal Pipeline keywords
			#pragma multi_compile _ _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			//--------------------------------------
			// Spine related keywords
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma vertex ForwardPassVertexSprite
			#pragma fragment ForwardPassFragmentSprite

			#define USE_URP
			#define fixed4 half4
			#define fixed3 half3
			#define fixed half
			#include "Include/Spine-Input-Sprite-URP1229.hlsl"
			#include "Include/Spine-Sprite-ForwardPass-URP1229.hlsl"
			ENDHLSL
		}



		Pass
		{
			Name "DepthOnly"
			Tags{"LightMode" = "DepthOnly"}

			ZWrite On
			ColorMask 0
			Cull Off

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex DepthOnlyVertexSprite
			#pragma fragment DepthOnlyFragmentSprite

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			#define USE_URP
			#define fixed4 half4
			#define fixed3 half3
			#define fixed half
			#include "Include/Spine-Input-Sprite-URP1229.hlsl"
			#include "Include/Spine-Sprite-DepthOnlyPass-URP.hlsl"
			ENDHLSL
		}
		Pass
        {
        	
            Name "SpineDepth"
	        Tags 
        	{
	            "LightMode" = "SpineDepth"
            }
            Cull Off 
            ZWrite on
            Blend Zero One
            HLSLPROGRAM
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma multi_compile_instancing
			#define USE_URP
            #define fixed4 half4
			#define fixed3 half3
			#define fixed half
         	#include "Include/Spine-Input-Sprite-URP1229.hlsl"
			#include "Include/Spine-Sprite-Depth-URP1229.hlsl"
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
		// Planar Shadows平面阴影
        Pass
        {
            Name "SpineShadow"
	        Tags 
        	{
	            "LightMode" = "SpineShadow"
            }
            //用使用模板测试以保证alpha显示正确
            Stencil
            {
                Ref 0
                Comp equal
                Pass incrWrap
                Fail keep
                ZFail keep
            }
            Cull Off
            //透明混合模式
            Blend One OneMinusSrcAlpha
            //关闭深度写入
            ZWrite off
            //深度稍微偏移防止阴影与地面穿插 
            Offset -1 , 0
            HLSLPROGRAM
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            #define USE_URP
            #define fixed4 half4
			#define fixed3 half3
			#define fixed half
         	#include "Include/Spine-Input-Sprite-URP1229.hlsl"
			#include "Include/Spine-Sprite-Shadow-URP1229.hlsl"
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

	}

	FallBack "Hidden/InternalErrorShader"
	CustomEditor "SpineSpriteShaderGUI1229"
}
 