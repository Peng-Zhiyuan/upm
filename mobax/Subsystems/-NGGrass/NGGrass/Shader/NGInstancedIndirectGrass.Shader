

Shader "NGEnv/NGInstancedIndirectGrass"
{
	Properties
	{
		//[MainColor] _BaseColor("BaseColor", Color) = (1,1,1,1)
		[NoScaleOffset]_MainTex("Texture2D", 2D) = "white" {}

		[Toggle(BILLBOARD)] BillBoard("Bill Board", Float) = 0
		//_RandomNormal("RandomNormal", Float) = 1
		// NoiseSpeed("NoiseSpeed", Float) = 0
		[NoScaleOffset]  NoiseTex("Noise", 2D) = "white" { }
		 NoiseMin("NoiseMin", Range(0.1,1)) = 0.3
		 NoiseScale("NoiseScale", Range(0.1,10)) = 1
		 //WinPow("WinPow", Float) = 0
		 //Smoothness("Smoothness", Float) = 0
		 ColorTop("ColorTop", Color) = (0,0,0,0)
		 ColorBase("ColorBase", Color) = (0,0,0,0)
		// Distance("Distance", Float) = 0
		// Impack("Impack", Float) = 0

		 _WindAFrequency("WindAFrequency", Float) = 0
		 _WindAIntensity("WindAIntensity", Float) = 0
		 _WindATiling("WindATiling", Vector) = (0,0,0,0)
		 //_WindAWrap("WindAFrequency", Vector) = (0,0,0,0)

		 //_WindBFrequency("WindBFrequency", Float) = 0
		 //_WindBIntensity("WindBIntensity", Float) = 0
		// _WindBTiling("WindBTiling", Vector) = (0,0,0,0)

		// _WindCFrequency("WindCFrequency", Float) = 0
		// _WindCIntensity("WindCIntensity", Float) = 0
		// _WindCTiling("WindCTiling", Vector) = (0,0,0,0)
			 
	}
		//HLSLINCLUDE

		//ENDHLSL

		  SubShader
		 {
			 Tags{ "RenderPipeline" = "UniversalPipeline" "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" }

			 Pass
			 {
				 Name "Forward"
				 Tags { "LightMode" = "UniversalForward" }

			 //Blend SrcAlpha OneMinusSrcAlpha
			 Cull Off

			 HLSLPROGRAM
			 #pragma target 3.0 
			 #pragma vertex vert
			 #pragma fragment frag
			//#pragma multi_compile_fog
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX  _ADDITIONAL_LIGHTS
			//#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT

			 #pragma multi_compile_instancing
			 #pragma instancing_options procedural:setup
			 #pragma multi_compile _ BILLBOARD
			 #pragma multi_compile _ _GPUCULL_PROCEDURAL_INSTANCINGD
			  #pragma multi_compile _ GRASS_ANIM
			 //#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			 sampler2D _MainTex;
			 sampler2D NoiseTex;
			 half4 NoiseTex_ST;
			 CBUFFER_START(UnityPerMaterial)
			 //half4 _BaseColor;
			 half4 _MainTex_ST;
			 //half _RandomNormal;
			 half4 _MainTex_TexelSize;
			 half3 _GroundColor;
			 half4 ColorTop;
			 half4 ColorBase;
			 half NoiseMin;
			 half NoiseScale;
			 half _WindAFrequency;
			 half _WindAIntensity;
			 half4 _WindATiling;
			
			/* half _WindBFrequency;
			 half _WindBIntensity;
			 half4 _WindBTiling;

			 half _WindCFrequency;
			 half _WindCIntensity;
			 half4 _WindCTiling;*/
			 CBUFFER_END


	 #ifdef _GPUCULL_PROCEDURAL_INSTANCINGD
			 StructuredBuffer<uint> _VisibleInstanceOnlyTransformIDBuffer;
			 StructuredBuffer<float4x4> _PerInstanceMatrixData;
	 #endif

		 void setup() {}


		 struct vetexAtt {
			 half4 positionOS : POSITION;
			 half3 normalOS : NORMAL;
			 half4 texcoord : TEXCOORD0;
			 //uint instanceID : SV_InstanceID;
			 UNITY_VERTEX_INPUT_INSTANCE_ID
		 };

		 struct Varyings
		 {
			 half2 uv                       : TEXCOORD0;
			 half4 positionCS			   : SV_POSITION;
			 half3 color                    : COLOR;
			 UNITY_VERTEX_INPUT_INSTANCE_ID
		 };
		 /*
		 half3 ApplySingleDirectLight(Light light, half3 N, half3 V, half3 albedo, half positionOSY)
		 {
			 half3 H = normalize(light.direction + V);
			 //direct diffuse 
			 half directDiffuse = dot(N, light.direction) * 0.5 + 0.5; //half lambert, to fake grass SSS
			 //direct specular
			 float directSpecular = saturate(dot(N, H));
			 //pow(directSpecular,8)
			 directSpecular *= directSpecular;
			 directSpecular *= directSpecular;
			 directSpecular *= directSpecular;
			 //directSpecular *= directSpecular; //enable this line = change to pow(directSpecular,16)
			 //add direct directSpecular to result
			 directSpecular *= 0.1 * positionOSY;//only apply directSpecular to grass's top area, to simulate grass AO
			 half3 lighting = light.color * (light.shadowAttenuation * light.distanceAttenuation);
			 half3 result = (albedo * directDiffuse + directSpecular) * lighting;
			 return result;
		 }
		 */
		 half3 ApplySingleDirectLight(Light light, half3 N, half3 V, half positionOSY, half3 vertexSH)
		 {
			 half3 H = normalize(light.direction + V);
			 half directDiffuse = dot(N, light.direction) * 0.5 + 0.5; //half lambert, to fake grass SSS
			 half directSpecular = saturate(dot(N, H));

			 directSpecular *= directSpecular;
			 directSpecular *= directSpecular;
			 directSpecular *= directSpecular;
			 directSpecular *= 0.05;

			 half3 lighting = light.color * ((light.shadowAttenuation + 0.5) * light.distanceAttenuation);
			 half3 result = (directDiffuse + directSpecular) * lighting;
			 result += vertexSH * (1.0 - saturate(dot(N, light.direction)));
			 return result;
		 }

		 Varyings vert(vetexAtt input, uint instanceID : SV_InstanceID)
		 {
			 Varyings output = (Varyings)0;
			 //uint instanceID = input.instanceID;
			 UNITY_SETUP_INSTANCE_ID(input);
			 UNITY_TRANSFER_INSTANCE_ID(input, output);
			 VertexPositionInputs vertexInput;
#ifdef _GPUCULL_PROCEDURAL_INSTANCINGD
			float4x4 localToTerrian = _PerInstanceMatrixData[_VisibleInstanceOnlyTransformIDBuffer[instanceID]];
			half3 perGrassPivotPosWS = half3(localToTerrian[0][3], localToTerrian[1][3], localToTerrian[2][3]);
			//vertexInput.positionWS = mul(localToTerrian, half4(input.positionOS.xyz, 1.0)).xyz;
			half3 normalWS = mul((float3x3)localToTerrian, input.normalOS);
#ifdef	BILLBOARD
			//rotation(make grass LookAt() camera just like a billboard)
			float3 cameraTransformRightWS = UNITY_MATRIX_V[0].xyz;
			//UNITY_MATRIX_V[0].xyz == world space camera Right unit vector
			float3 cameraTransformUpWS = UNITY_MATRIX_V[1].xyz;
			//UNITY_MATRIX_V[1].xyz == world space camera Up unit vector
			float3 cameraTransformForwardWS = -UNITY_MATRIX_V[2].xyz;
			//UNITY_MATRIX_V[2].xyz == -1 * world space camera Forward unit vector

			//Expand Billboard (billboard Left+right)
			float3 positionOS = input.positionOS.x * cameraTransformRightWS;
			//random width from posXZ, min 0.1
			//Expand Billboard (billboard Up)
			positionOS += input.positionOS.y * cameraTransformUpWS;
			vertexInput.positionWS = mul(localToTerrian, half4(positionOS.xyz, 1.0)).xyz;
#else
			vertexInput.positionWS = mul(localToTerrian, half4(input.positionOS.xyz, 1.0)).xyz;
#endif
#else 
			vertexInput.positionWS = mul(GetObjectToWorldMatrix(), half4(input.positionOS.xyz, 1.0)).xyz;
			vertexInput.positionCS = TransformWorldToHClip(vertexInput.positionWS);
			
			half3 normalWS = mul((float3x3)GetObjectToWorldMatrix(), input.normalOS);
			half3 perGrassPivotPosWS = vertexInput.positionWS;
#endif


#ifdef	GRASS_ANIM
			//Wind
			//float3 cameraTransformRightWS = UNITY_MATRIX_V[0].xyz;
			float wind = 0;
			wind += (sin(_Time.y * _WindAFrequency + perGrassPivotPosWS.x * _WindATiling.x + perGrassPivotPosWS.z * _WindATiling.y) * _WindATiling.z + _WindATiling.w) * _WindAIntensity; //windA
			//wind += (sin(_Time.y * _WindBFrequency + perGrassPivotPosWS.x * _WindBTiling.x + perGrassPivotPosWS.z *_WindBTiling.y) * _WindBTiling.z + _WindBTiling.w) * _WindBIntensity; //windB
			//wind += (sin(_Time.y * _WindCFrequency + perGrassPivotPosWS.x * _WindCTiling.x + perGrassPivotPosWS.z *_WindCTiling.y) * _WindCTiling.z + _WindCTiling.w) * _WindCIntensity; //windC
			wind *= input.texcoord.y; //wind only affect top region, don't affect root region
			//float3 windOffset = UNITY_MATRIX_V[0].xyz *  wind; //swing using billboard left right direction
			vertexInput.positionWS.xyz += wind;
			
#endif
			vertexInput.positionCS = mul(UNITY_MATRIX_VP, half4(vertexInput.positionWS.xyz, 1.0));
			half3 scrPos = vertexInput.positionWS/(100 * NoiseScale);
				/*
				//get "is grass stepped" data(bending) from RT
				float2 grassBendingUV = ((perGrassPivotPosWS.xz - _PivotPosWS.xz) / _BoundSize) * 0.5 + 0.5;
				//claculate where is this grass inside bound (can optimize to 2 MAD)
				float stepped = tex2Dlod(_GrassBendingRT, float4(grassBendingUV, 0, 0)).x;

				//bending by RT (hard code)
				float3 bendDir = cameraTransformForwardWS;
				bendDir.xz *= 0.5; //make grass shorter when bending, looks better
				bendDir.y = min(-0.5, bendDir.y);
				//prevent grass become too long if camera forward is / near parallel to ground
				positionOS = lerp(positionOS.xyz + bendDir * positionOS.y / -bendDir.y, positionOS.xyz,
					stepped * 0.95 + 0.05); //don't fully bend, will produce ZFighting
				*/
				//wind animation (biilboard Left Right direction only sin wave)      
				//float4 perGrassPivotPosWS = vertexInput.positionWS;
				


				half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
				half3 vertexSH = half3(0.0,0.0,0.0);
				OUTPUT_SH(normalWS.xyz, vertexSH);
				half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

				//half3 lightingResult = ApplySingleDirectLight(mainLight, normalWS, normalize(viewDirWS), vertexInput.positionWS.y, vertexSH);

				Light mainLight;
#if _MAIN_LIGHT_SHADOWS
				half4 shadowMask = half4(1, 1, 1, 1);
				half4 shadowCoord = TransformWorldToShadowCoord(vertexInput.positionWS);
				mainLight = GetMainLight(shadowCoord, vertexInput.positionWS, shadowMask);
#else
				mainLight = GetMainLight();
#endif                               
				//half3 randomAddToN = (_RandomNormal * sin(perGrassPivotPosWS.x * 80.32523 + perGrassPivotPosWS.z) + wind * -0.25)* cameraTransformRightWS; //random normal per grass 
				//default grass's normal is pointing 100% upward in world space, it is an important but simple grass normal trick
				//-apply random to normal else lighting is too uniform
				//-apply cameraTransformForwardWS to normal because grass is billboard
				//float2 screenUV = scrPos.xy / scrPos.w;  //屏幕UV
				//half3 N = normalize(half3(0, 1, 0) + randomAddToN - cameraTransformForwardWS * 0.5);
				//half3 V = viewDirWS / 100;// ViewWSLength;
				//half3 topColor = tex2Dlod(Texture2D_noise, float4(TRANSFORM_TEX(vertexInput.positionWS.xz, Texture2D_noise), 0, 0)) * ColorTop;
				half3 topColor = lerp(NoiseMin, 1,tex2Dlod(NoiseTex, float4(TRANSFORM_TEX(scrPos.xz, NoiseTex), 0, 0)).r) * ColorTop;
				//sample mip 0 only
				//half3 albedo = lerp(ColorBase, topColor, input.texcoord.y);
				//（计算球谐光照，因为场景中的光照探针）
				//half3 lightingResult = SampleSH(0) * albedo;
				//main direct light
				//lightingResult += ApplySingleDirectLight(mainLight, N, V, albedo);
				half3 lightingResult = ApplySingleDirectLight(mainLight, normalWS, normalize(viewDirWS), vertexInput.positionWS.y, vertexSH);
				//mainLight = GetMainLight(TransformWorldToShadowCoord(vertexInput.positionWS));

#if _ADDITIONAL_LIGHTS
				int additionalLightsCount = GetAdditionalLightsCount();
				for (uint lightIndex = 0u; lightIndex < additionalLightsCount; ++lightIndex)
				{
					Light light = GetAdditionalLight(lightIndex, vertexInput.positionWS);
					half ndl = dot(normalWS.xyz, light.direction);
					lightingResult += light.color * light.distanceAttenuation ;
				}
#endif
				output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
				output.positionCS = vertexInput.positionCS;
				output.color = MixFog(lightingResult, fogFactor);
				output.color *= lerp(ColorBase, topColor, input.texcoord.y);
				return output;
			}

			half4 frag(Varyings input) : SV_Target
			{
				//UNITY_SETUP_INSTANCE_ID(input);
				half2 uv = input.uv.xy;
				half4 mainTex = tex2D(_MainTex,uv);
				clip(mainTex.r - 0.1);
				return half4(input.color.xyz, 1);
			}
			ENDHLSL
		}

			 //	Pass
			 //	{
			 //		Name "ShadowCaster"
			 //		Tags{"LightMode" = "ShadowCaster"}

			 //		ZWrite On
			 //		ZTest LEqual
			 //		Cull Back

			 //		HLSLPROGRAM
			 //		#pragma vertex ShadowPassVertex
			 //		#pragma fragment ShadowPassFragment
			 //		#pragma multi_compile_instancing
			 //		#pragma instancing_options procedural:setup
			 //		#pragma multi_compile __ _GPUCULL_PROCEDURAL_INSTANCINGD

			 //		//#ifndef UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED
			 //		//#define UNIVERSAL_SHADOW_CASTER_PASS_INCLUDED
			 //		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"


			 //		struct vetexAtt {
			 //			half4 positionOS : POSITION;
			 //			half4 texcoord : TEXCOORD0;
			 //			//uint instanceID : SV_InstanceID;
			 //			UNITY_VERTEX_INPUT_INSTANCE_ID
			 //		};

			 //		struct Varyings
			 //		{
			 //			half4 positionCS  : SV_POSITION;
			 //			half2 uv          : TEXCOORD0;
			 //			UNITY_VERTEX_INPUT_INSTANCE_ID
			 //		};

			 //		Varyings ShadowPassVertex(vetexAtt input, uint instanceID : SV_InstanceID)
			 //		{
			 //			Varyings output = (Varyings)0;
			 //			UNITY_SETUP_INSTANCE_ID(input);

			 //#ifdef _GPUCULL_PROCEDURAL_INSTANCINGD
			 //			float4x4 localToTerrian = _PerInstanceMatrixData[_VisibleInstanceOnlyTransformIDBuffer[instanceID]];
			 //			half3 perGrassPivotPosWS = half3(localToTerrian[0][3], localToTerrian[1][3], localToTerrian[2][3]);
			 //			half perGrassHeight = lerp(0.8, 1.5, (sin(perGrassPivotPosWS.x * 23.4643 + perGrassPivotPosWS.z) * 0.45 + 0.55));
			 //			input.positionOS.y *= perGrassHeight;
			 //			half3 positionWS = mul(localToTerrian, half4(input.positionOS.xyz, 1.0)).xyz;
			 //			output.positionCS = mul(UNITY_MATRIX_VP, half4(positionWS, 1.0));
			 //#else 
			 //			half3 positionWS = mul(GetObjectToWorldMatrix(), half4(input.positionOS.xyz, 1.0)).xyz;
			 //			output.positionCS = TransformWorldToHClip(positionWS);
			 //#endif
			 //			output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
			 //			return output;
			 //		}

			 //		half4 ShadowPassFragment(Varyings input) : SV_TARGET
			 //		{
			 //			half4 mainTex = tex2D(_MainTex,input.uv);
			 //			clip(mainTex.a - _CutOff);
			 //			return 0;
			 //		}
			 //		ENDHLSL
			 //	}

	 //			Pass
	 //			{
	 //				Name "DepthOnly"
	 //				Tags{"LightMode" = "DepthOnly"}
	 //
	 //				ZWrite On
	 //				ColorMask 0
	 //				Cull Back
	 //
	 //				HLSLPROGRAM
	 //				#pragma vertex DepthOnlyVertex
	 //				#pragma fragment DepthOnlyFragment
	 //
	 //				#pragma multi_compile_instancing
	 //				#pragma instancing_options procedural:setup
	 //				//#pragma multi_compile __ _GPUCULL_PROCEDURAL_INSTANCINGD
	 //				#ifndef UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED
	 //				#define UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED
	 //
	 //				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	 //
	 //				struct vetexAtt {
	 //					half4 positionOS : POSITION;
	 //					half4 texcoord : TEXCOORD0;
	 //					//uint instanceID : SV_InstanceID;
	 //					UNITY_VERTEX_INPUT_INSTANCE_ID
	 //				};
	 //
	 //				struct Varyings
	 //				{
	 //					half4 positionCS  : SV_POSITION;
	 //					half2 uv          : TEXCOORD0;
	 //					UNITY_VERTEX_INPUT_INSTANCE_ID
	 //				};
	 //
	 //				Varyings DepthOnlyVertex(vetexAtt input, uint instanceID : SV_InstanceID)
	 //				{
	 //					Varyings output = (Varyings)0;
	 //					UNITY_SETUP_INSTANCE_ID(input);
	 //
	 //#ifdef _GPUCULL_PROCEDURAL_INSTANCINGD
	 //					float4x4 localToTerrian = _PerInstanceMatrixData[_VisibleInstanceOnlyTransformIDBuffer[instanceID]];
	 //					half3 perGrassPivotPosWS = half3(localToTerrian[0][3], localToTerrian[1][3], localToTerrian[2][3]);
	 //					half perGrassHeight = lerp(0.8, 1.5, (sin(perGrassPivotPosWS.x * 23.4643 + perGrassPivotPosWS.z) * 0.45 + 0.55));
	 //					input.positionOS.y *= perGrassHeight;
	 //					half3 positionWS = mul(localToTerrian, half4(input.positionOS.xyz, 1.0)).xyz;
	 //					output.positionCS = mul(UNITY_MATRIX_VP, half4(positionWS, 1.0));
	 //#else 
	 //					half3 positionWS = mul(GetObjectToWorldMatrix(), half4(input.positionOS.xyz, 1.0)).xyz;
	 //					output.positionCS = TransformWorldToHClip(positionWS);
	 //#endif
	 //					output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
	 //					return output;
	 //				}
	 //
	 //				half4 DepthOnlyFragment(Varyings input) : SV_TARGET
	 //				{
	 //					half4 mainTex = tex2D(_MainTex,input.uv);
	 //					clip(mainTex.a - _CutOff);
	 //					return 0;
	 //				}
	 //				#endif
	 //
	 //				ENDHLSL
	 //			}

		 }
}
