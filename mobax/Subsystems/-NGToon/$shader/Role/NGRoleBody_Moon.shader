
Shader "NGRole/Body_Moon"
{
	Properties {

		[HDR] _BaseColor("BaseColor Gain", Color) = (1.0,1.0,1.0,1.0)

		[HDR]_Emission_Color("Emission Color", Color) = (1,1,1,1)

		_Albedo("Albedo", 2D) = "white" {}
		_AlbedoRotate("Albedo rotate", Float) = 1

		_Albedo2("Albedo2", 2D) = "white" {}
		_Albedo2Rotate("Albedo2 rotate", Float) = 1
		_NdotV_Range("NdotV_Range", Float) = 1
		[HDR]_OutLineColor("OutLineColor", Color) = (0,0,0,1)
		_OutLineZOffset("OutLine Z Offset",Range(0,1)) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendNG("Blending Source", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlendNG("Blending Dest", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2

	}
	SubShader {

		Pass {
			Tags { "LightMode" = "UniversalForward" }
			ZWrite on
			Blend[_SrcBlendNG][_DstBlendNG]
			LOD 100
			Cull[_Cull]
			HLSLPROGRAM
			// #pragma multi_compile _ _RECIVESHADOW_ON
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"
			#include "../Frame/CustomLightFunc.hlsl"

			CBUFFER_START(UnityPerMaterial)
			
			half4 _BaseColor;
			half4 _Albedo_ST;
			half4 _Albedo2_ST;
			half4 _Albedo3_ST;
			half _RampThreshold;
			half _RampSmooth;
			
			half _NdotV_Range;

			half _AlbedoRotate;
			half _Albedo2Rotate;

			half4 _Emission_Color;

			CBUFFER_END
			
			TEXTURE2D(_Albedo);
			SAMPLER(sampler_Albedo);
			TEXTURE2D(_Albedo2);
			SAMPLER(sampler_Albedo2);
			TEXTURE2D(_Albedo3);
			SAMPLER(sampler_Albedo3);
			
			struct Attributes
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float3 normal: NORMAL;
				// uint id : SV_VertexID;
			};

			struct Varyings 
			{
				float4 pos : SV_POSITION;
				float2 uv: TEXCOORD2;
				float2 uv1 : TEXCOORD3;
				float2 uv2 : TEXCOORD1;
				half3 posWorld : TEXCOORD4;
				float3 normalDir : TEXCOORD5;
				half4 screenPosition : TEXCOORD7;
				// float3 ray : TEXCOORD1;
			};
			
			Varyings vert(Attributes i) 
			{
				Varyings output = (Varyings)0;
				VertexPositionInputs vertexInput = GetVertexPositionInputs(i.vertex.xyz);
				output.pos = vertexInput.positionCS;
				output.uv = TRANSFORM_TEX(i.uv, _Albedo);
				output.normalDir=TransformObjectToWorldNormal(i.normal);
				output.posWorld = vertexInput.positionWS;
				output.uv1 = i.uv1;
				output.uv2 = i.uv2;
				output.screenPosition = ComputeScreenPos(output.pos);

				return output;
			}

			half4 frag(Varyings i) : SV_Target
			{

				float2 screenPos = i.screenPosition.xy/i.screenPosition.w;

				half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				half3 lightDirection =  normalize(_MainLightPosition).xyz;
				// half3 lightColor = _MainLightColor.rgb;

				i.normalDir = normalize(i.normalDir);

				// half3 lightDirection = normalize(_MainLightPosition).xyz;

				half NdotL = saturate(dot(i.normalDir,lightDirection)*10) ;

				half NdotV = 1 -pow (saturate(dot(i.normalDir,viewDirection)),_NdotV_Range);

				NdotV *= NdotL;

				// half3 normalDirection = normalize(i.normalDir); // Perturbe

				_Albedo_ST.zw *= _Time.y;

				_Albedo2_ST.zw *= _Time.y;

				_Albedo3_ST.zw *= _Time.y;

				half2 rUV = float2( screenPos.x *cos(-_AlbedoRotate) -  screenPos.y * sin(-_AlbedoRotate), screenPos.x *sin(-_AlbedoRotate) +  screenPos.y*cos(-_AlbedoRotate));

				half2 rUV2 = float2( screenPos.x *cos(-_Albedo2Rotate) -  screenPos.y * sin(-_Albedo2Rotate), screenPos.x *sin(-_Albedo2Rotate) +  screenPos.y*cos(-_Albedo2Rotate));


				// half4 starTex3 = SAMPLE_TEXTURE2D(_Albedo3,sampler_Albedo3, rUV * _Albedo3_ST.xy + _Albedo3_ST.zw);

				// return saturate(sin(_Time.z)*0.5+0.5).xxxx;

				half4 starTex2 = SAMPLE_TEXTURE2D(_Albedo2,sampler_Albedo2, rUV2 * _Albedo2_ST.xy + _Albedo2_ST.zw );

				// starTex3 = lerp( starTex3, starTex2,  saturate(sin(_Time.y+0.5)*0.5 + 0.65));  


				half4 mainTex = SAMPLE_TEXTURE2D(_Albedo,sampler_Albedo, rUV * _Albedo_ST.xy + _Albedo_ST.zw  );

				// return starTex2;

				half temp = lerp(starTex2.b,mainTex.b,   sin(_Time.y)*0.5+0.75 );

				// mainTex = lerp (mainTex,mainTex *20  ,   pow(starTex3.r * 10 , 3));

				// mainTex = lerp(starTex3, mainTex ,  saturate(  sin(_Time.x)*0.5+0.65) );



				mainTex.rgb = lerp(mainTex.rgb,starTex2.rgb,temp );

				mainTex.rgb *= _BaseColor.rgb;

				// half3 diffuse = mainTex.rgb * _BaseColor.rgb  * shadowColor;

				// return  NdotV.xxxx;
				return  half4(mainTex.rgb,0.97);
			}  
			ENDHLSL
		}

		Pass
		{
			Name "Outline"
			Tags { "LightMode" = "Outline" }
			Blend[_SrcBlendNG][_DstBlendNG]
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

	}
	Fallback Off
	CustomEditor "PunishingInspector"
}