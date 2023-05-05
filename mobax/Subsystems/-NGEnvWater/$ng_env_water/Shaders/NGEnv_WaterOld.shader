
Shader "NGEnv/WaterOld"
{
	Properties{
		[Header(Depth)]
		[Space(5)]
		[HDR] _ShallowColor("Shallow Color", Color) = (1, 1, 1, 1)
		[HDR]_DeepColor("Deep Color", Color) = (1, 1, 1, 1)
		_DepthDistance("Depth Distance", Float) = 1

		[Header(Normal)]
		[Space(5)]
		_BumpMap("Normal Map", 2D) = "bump"{}
		_NormalInt1("NormalInt1", Range(0.0, 1.0)) = 1
		_NormalInt2("NormalInt2", Range(0.0, 1.0)) = 1
		_BumpMapScale("NormalMap Scale", Float) = 1
		_NormalSpeed("Normal Speed", Vector) = (1, 1, 1, 1)

		[Header(Reflection)]
		[Space(5)]
		[KeywordEnum(REFLECTION_PROBES, REFLECTION_PLANARREFLECTION, REFLECTION_PRANDRP)]  _ENUM("ReflectionType", Float) = 1
		_ReflectionIntensity("ReflectionIntensity", Range(0.0, 1.0)) = 1
		_ReflectionLod("ReflectionLod", Range(0.0, 1.0)) = 1
		_Smoothness("Smoothness", Range(0.0, 1.0)) = 1

		[Header(Fresnel)]
		[Space(5)]
		_FresnelColor("Fresnel Color", Color) = (1, 1, 1, 1)
		_FresnelStrength("Fresnel Strength", Range(0.0, 1.0)) = 1
		_FresnelPower("Fresnel Power", Range(0.0, 30.0)) = 1

		[Header(Foam)]
		[Space(5)]
		_FoamColor("Foam Color", Color) = (1, 1, 1, 1)
		_FoamIntensity("Foam Intensity", Float) = 1
		_FoamFrequency("Foam Frequency", Float) = 0.5
		_speed("Foam Speed", Float) = 0.5
		_FoamWidth("Foam Width", Float) = 0.5
		_IntersectionDepthDistance("Intersection Depth Distance", Range(0, 3)) = 0.5
		_IntersectionWaterBlend("Intersection Water Blend", Range(0, 0.49)) = 0.5
		[Space(10)]
		_NoiseScale("Noise Scale", Float) = 0.5
		_NoiseStrength("Noise Strength", Range(0, 2)) = 0.5
		_SpeedNoise("Noise Speed", Float) = 0.5
		[Space(10)]
		_FoamMap("Foam Map", 2D) = "white"{}
		_FoamTextureSpeedX("Foam Texture SpeedX", Float) = 1
		_FoamTextureSpeedY("Foam Texture SpeedY", Float) = 1
		[Space(10)]
		_NoiseScaleDepth("Noise Scale Depth", Float) = 0.5
		_NoiseStrengthDepth("Noise Strength Depth", Range(0, 2)) = 0.5
		_SpeedNoiseDepth("Noise Speed Depth", Float) = 0.5
		[Space(10)]
		[ToggleOff(_RECEIVE_SHADOWS_OFF)]_ReceiveShadows("Receive Shadows", Float) = 1.0

	}
		SubShader{
			Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
			Pass {
				Name "Forward"
				Tags { "LightMode"="UniversalForward" }
				//Tags { "LightMode" = "GroundDecal" }
				Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
				ZWrite OFF
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
				#pragma multi_compile _ _MAIN_LIGHT_SHADOWS		
				#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE			
				#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS			
				#pragma multi_compile _ _SHADOWS_SOFT
				#pragma shader_feature _RECEIVE_SHADOWS_OFF
				#pragma shader_feature _ENUM_REFLECTION_PROBES _ENUM_REFLECTION_PLANARREFLECTION _ENUM_REFLECTION_PRANDRP

				#include "Include/NGEnv_WATER_COMMON_OLD.hlsl"

				ENDHLSL
			}
		}
}
