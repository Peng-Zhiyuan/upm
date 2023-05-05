#ifndef WATER_COMMON_INCLUDED
#define WATER_COMMON_INCLUDED

	#include "../Include/NGEnv_WATER_SURFACEDATA.hlsl"
 
	struct Attributes {
		float4 positionOS				:	POSITION;
		float2 uv						:	TEXCOORD0;
		float4 color					:	COLOR;
		float3 normalOS					:	NORMAL;
		float4 tangentOS				:	TANGENT;
	};

	struct Varyings {
		float4 positionCS 				:	SV_POSITION;
		float2 uv						:	TEXCOORD0;
		float4 color					:	COLOR;
		float4 ScreenPosition			:	TEXCOORD1;
		float4 positionWSAndFogFactor	:	TEXCOORD2;
		// float3 T2W01					:	TEXCOORD3;
		// float3 T2W02					:	TEXCOORD4;
		// float3 T2W03					:	TEXCOORD5;
		// float3 normal				:	NORMAL;
		float4 ShadowCooord				:	TEXCOORD5;
		float3 normalWS					:	TEXCOORD3;
		float4 tangentWS				:	TEXCOORD4;
	};

	// void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
	// {
	// 	inputData = (InputData)0;
	// 	
	// 	float sgn = input.tangentWS.w;
	// }

	Varyings vert(Attributes IN) {
		Varyings OUT = (Varyings)0;

		VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
		OUT.positionCS = positionInputs.positionCS;
		OUT.positionWSAndFogFactor.rgb = positionInputs.positionWS;

		OUT.uv = IN.uv;
		OUT.color = IN.color;
		VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);
		OUT.normalWS = normalInputs.normalWS;
		OUT.tangentWS = half4(normalInputs.tangentWS.xyz, IN.tangentOS.w * GetOddNegativeScale());//TransformObjectToWorldDir(IN.tangentOS.xyz);

		// half3 biTangentWS = cross(normalWS.xyz, tangentWS.xyz) * IN.tangentOS.w * unity_WorldTransformParams.w;

		// OUT.T2W01 = half3(tangentWS.x, biTangentWS.x, normalWS.x);
		// OUT.T2W02 = half3(tangentWS.y, biTangentWS.y, normalWS.y);
		// OUT.T2W03 = half3(tangentWS.z, biTangentWS.z, normalWS.z);

		// OUT.normal = float3(0, 1, 0);
		OUT.ScreenPosition = ComputeScreenPos(OUT.positionCS);
		OUT.positionWSAndFogFactor.a = ComputeFogFactor(OUT.positionCS.z);
		OUT.ShadowCooord = TransformWorldToShadowCoord(OUT.positionWSAndFogFactor.xyz);
		return OUT;
	}
	
inline float3 BoxProjectedCubemapDirection (float3 worldRefl, float3 worldPos, float4 cubemapCenter, float4 boxMin, float4 boxMax)
{
    
    //UNITY_BRANCH
    //if (cubemapCenter.w > 0.0)         // 判断反射探头（Reflection Probe组件）是否勾选了BoxProjection
    {
		half3 nrdir = normalize(worldRefl);

        half3 rbmax = (boxMax.xyz - worldPos) / nrdir;
        half3 rbmin = (boxMin.xyz - worldPos) / nrdir;

        half3 rbminmax = (nrdir > 0.0f) ? rbmax : rbmin;

        half fa = min(min(rbminmax.x, rbminmax.y), rbminmax.z);

        worldPos -= cubemapCenter.xyz;
        worldRefl = worldPos + nrdir * fa;

/*
        float3 nrdir = normalize(worldRefl);

        float3 rbmax = (boxMax.xyz - worldPos);
        float3 rbmin = (boxMin.xyz - worldPos);
 
        float3 select = step (float3(0,0,0), nrdir);
        float3 rbminmax = lerp (rbmax, rbmin, select);
        rbminmax /= nrdir;

        float fa = min(min(rbminmax.x, rbminmax.y), rbminmax.z);
 
        worldPos -= cubemapCenter.xyz;
        worldRefl = worldPos + nrdir * fa;
*/

    }
    return worldRefl;
}

	half4 frag(Varyings IN) : SV_Target {
		// float Noise1 = Unity_GradientNoise_half(float2(IN.uv + _SpeedNoiseDepth * _Time.x), _NoiseScaleDepth) - _NoiseStrengthDepth;
		//DepthFade
        float4 screenPos = float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0);
        screenPos.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? screenPos.z : screenPos.z * 0.5 + 0.5;
        float SampleDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(IN.ScreenPosition.xy / IN.ScreenPosition.w)).r;
		// float SampleDepth = LinearEyeDepth(SampleDepth, _ZBufferParams);
        float3 worldPos = ComputeWorldSpacePosition(UnityStereoTransformScreenSpaceTex(screenPos.xy), SampleDepth, UNITY_MATRIX_I_VP);
        float Distance = distance(worldPos, IN.positionWSAndFogFactor.rgb);		
        float depthTest = saturate(exp2(-Distance * _DepthDistance));

		////////////////////////////////////////
		//  DIR
		////////////////////////////////////////
		Light light = GetMainLight(TransformWorldToShadowCoord(IN.positionWSAndFogFactor.xyz));
		float3 vDir = SafeNormalize(GetWorldSpaceViewDir(IN.positionWSAndFogFactor.xyz));;//SafeNormalize(_WorldSpaceCameraPos - IN.positionWSAndFogFactor.xyz);
		float3 hDir = normalize(SafeNormalize(light.direction) + vDir);
		
		////////////////////////////////////////
		//  NORMAL
		////////////////////////////////////////
		float2 normal1UV = float2(IN.uv * _BumpMap_ST.xy + _BumpMap_ST.zw + _NormalSpeed.xy * _Time.x);
		float3 n1Dir = (UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, normal1UV), _NormalInt1));
		float2 normal2UV = float2(IN.uv * _BumpMap_ST.xy * _BumpMapScale + _BumpMap_ST.zw + _NormalSpeed.zw * _Time.x);
		float3 n2Dir = (UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, normal2UV), _NormalInt2));
		float3 nBlend = NormalBlend(n1Dir, n2Dir);
		// float3 nDir;
		// nDir.x = dot(IN.T2W01, nBlend);
		// nDir.y = dot(IN.T2W02, nBlend);
		// nDir.z = dot(IN.T2W03, nBlend);
		// nDir = normalize(nDir.xzy);

		float sgn = IN.tangentWS.w;
		float3 bitangent = sgn * cross(IN.normalWS.xyz, IN.tangentWS.xyz);
		float3 normalWS = TransformTangentToWorld(nBlend,half3x3(IN.tangentWS.xyz, bitangent, IN.normalWS));
		normalWS = NormalizeNormalPerPixel(normalWS);
		//DOT
		// float NoL = max(normalize(dot(nDir, light.direction)), 0.000001);


		////////////////////////////////////////
		//  FRESNEL
		////////////////////////////////////////
		float3 Fresnel = (_FresnelStrength * pow(1.0 - saturate(dot(normalWS, vDir)), _FresnelPower));
		float Fresnel_Alpha = _AlphaFresnelStrength * pow(1.0 - saturate(dot(normalWS, vDir)), _AlphaFresnelPower);

		float depth01 = Linear01Depth(SampleDepth, _ZBufferParams);
		depth01 = saturate(depth01 * 32);
		//return float4(depth01.xxx, 1);
		//return float4(depthTest.xxx * 1, 1);
		//return float4(Fresnel_Alpha.xxx, 1);
		////////////////////////////////////////
		//  COLOR
		////////////////////////////////////////
		float4 Color = lerp(_DeepColor, _ShallowColor, Fresnel_Alpha);
		



		////////////////////////////////////////
		//  REFLECTION
		////////////////////////////////////////
		half3 reflection = 0;
		half3 reflectVector = reflect(-vDir, normalWS);
		float2 screenUV = IN.ScreenPosition.xy / IN.ScreenPosition.w;
		float2 p11_22 = float2(unity_CameraInvProjection._11, unity_CameraInvProjection._22);
		float3 viewDir = -(float3((screenUV * 2 - 1) / p11_22, -1));
		half3 viewNormal = mul(normalWS, (float3x3)GetWorldToViewMatrix()).xyz;
		half2 reflectionUV = screenUV + normalWS.zx * half2(0.02, 0.15);		
#if _ENUM_REFLECTION_PROBES
		//float mip = PerceptualRoughnessToMipmapLevel(s.perceptualRoughness);
		//float4 sample = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, 0);
		reflectVector = BoxProjectedCubemapDirection(reflectVector, worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
		//reflectVector = BoxProjectedCubemapDirection(reflectVector, worldPos, _SpecCube0_ProbePosition, _SpecCube0_BoxMin, _SpecCube0_BoxMax);
		reflection += GlossyEnvironmentReflection(reflectVector, _ReflectionLod, 1) * _ReflectionIntensity;
		//reflection = sample.rgb;
#elif _ENUM_REFLECTION_PLANARREFLECTION
		reflection += SAMPLE_TEXTURE2D_LOD(_PlanarReflectionTexture, sampler_PlanarReflectionTexture, reflectionUV, _ReflectionLod) * _ReflectionIntensity;//tex2D(_ReflectionTex, i.screenPos.xy / i.screenPos.w);
#elif _ENUM_REFLECTION_PRANDRP
		//reflectVector = BoxProjectedCubemapDirection(reflectVector, worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
		half3 RP = GlossyEnvironmentReflection(reflectVector, _ReflectionLod, 1) * _ReflectionIntensity;
		half3 PR = SAMPLE_TEXTURE2D_LOD(_PlanarReflectionTexture, sampler_PlanarReflectionTexture, reflectionUV, _ReflectionLod) * _ReflectionIntensity * 2;//tex2D(_ReflectionTex, i.screenPos.xy / i.screenPos.w);
		reflection += lerp(RP, PR, 0.5);
#endif
		

		////////////////////////////////////////
		//  LIGHTING
		////////////////////////////////////////
#ifndef _SPECULARHIGHLIGHTS_OFF
		half3 atten = light.color * light.distanceAttenuation * light.shadowAttenuation;
		BRDFData brdfData;
		half alpha = 1;
		InitializeBRDFData(Color.rgb, 0, half3(1, 1, 1), _Smoothness, alpha, brdfData);
		half3 spec = DirectBRDF(brdfData, normalWS, light.direction, vDir) * atten;
		// reflectionTex.rgb += spec;
		// spec *= reflection.rgb;
		// return half4(dot(nDir, halfDir).xxx, 1);
		//FOAM
#else 
		half3 spec = 0;
#endif

		half4 foamTex = SAMPLE_TEXTURE2D(_FoamMap, sampler_FoamMap, IN.positionWSAndFogFactor.xz * _FoamMap_ST.xy + _FoamMap_ST.zw + _Time.y * float2(_FoamTextureSpeedX, _FoamTextureSpeedY));
		float FoamFrequency = sin(depthTest * _FoamFrequency + _speed * _Time.x);
		float Noise = Unity_GradientNoise_half(float2(IN.uv + _SpeedNoise * _Time.x), _NoiseScale) - _NoiseStrength;
		float addFN = FoamFrequency + Noise + foamTex.r;
		float FoamWidth = (1 - (_FoamWidth - depthTest));
		float FoamStep = step(FoamWidth, addFN + depthTest);
		float FoamMask = (1 - smoothstep(1 - _IntersectionWaterBlend, 0.5, exp2(-Distance * _IntersectionDepthDistance))) * FoamStep;

//#ifdef _ADDITIONAL_LIGHTS
//		int transPixelLightCount = GetAdditionalLightsCount();
//		for (int i = 0; i < transPixelLightCount; ++i)
//		{
//			Light light = GetAdditionalLight(i, IN.positionWSAndFogFactor.xyz);
//			float3 atten = light.color * light.distanceAttenuation;
//			atten = lerp(atten, atten * light.shadowAttenuation, 0.5);
//
//			half3 transmission = max(0, -dot(IN.normalWS, light.direction)) * atten;
//			Color.rgb += Color.rgb * transmission;
//		}
//#endif
		//COMP
		half3 FinalColor2 = (Color.rgb + lerp(float3(0, 0, 0), reflection.rgb, Fresnel) + spec + FoamMask * _FoamColor.rgb * _FoamIntensity);

		//Fog
		FinalColor2.rgb = MixFog(FinalColor2.rgb, IN.positionWSAndFogFactor.a);
		
		//DEBUG
		//Shadow
		// return half4(spec, 1);
		//color
		// return half4(Color + lerp(_DeepColor, reflectionTex.rgb, Fresnel), Color.a);
		//Fresnel
		// return half4(Fresnel.rgb, Color.a);
		//spec
		// return half4(spec.rgb, 1);
		//FOAM
		// return half4(FoamMask.xxx, Color.a);
		//COMP
		// return half4(lerp(Color.rgb, reflectionTex, Fresnel) + spec + foam * _FoamColor * _FoamIntensity, Color.a * (1.2 - depthTest));
		//COLOR

		//return float4(depthTest.xxx, 1);
		return half4(FinalColor2.rgb, 1 * (1 - depthTest) * 1);
	}
#endif