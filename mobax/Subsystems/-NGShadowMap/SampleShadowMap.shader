Shader "NGEnv/SampleShadowMap" {
    Properties {
        _MainColor ("Tint Color", Color) = (1,1,1,1)
//        _MainTexBrightness ("Brightness", Float ) = 1
//        _TurbulenceTex ("Wave Tex", 2D) = "bump" {}

//        [Header(Noise)]
//        [Space(5)]
//        _NoiseScale ("Noise Scale", Float) = 0.5
//		_NoiseStrength ("Noise Strength", Range(0, 2)) = 0.5
//		_SpeedNoise ("Noise Speed", Float) = 0.5
//        _NoiseTex ("Noise Tex", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline" = "UniversalRenderPipeline"
        }

        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="UniversalForward"
            }
			Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
			ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            //#define WAVE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
 
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            //#include "Include/NGEnv_WATER_SURFACEDATA.hlsl"
            #pragma target 2.0           
            
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                //float4 texcoord1 : TEXCOORD1;
                float4 vertexColor : COLOR;
            };

            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float3 wpos:TEXCOORD1;
                float4 vertexColor : COLOR;
            };

            VertexOutput vert (VertexInput IN) {
                VertexOutput OUT = (VertexOutput)0;
                OUT.uv0 = IN.texcoord0;
                //VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                OUT.wpos = TransformObjectToWorld(IN.vertex.xyz);;
                OUT.vertexColor = IN.vertexColor;
                OUT.pos = TransformObjectToHClip( IN.vertex.xyz );
                return OUT;
            }
            half ShadowAtten(float3 worldPosition)
            {
                    return MainLightRealtimeShadow(TransformWorldToShadowCoord(worldPosition));
            }
            float4 frag(VertexOutput IN) : SV_Target {
                // float2 NoiseUV = (IN.uv0 + (float2(_NoisePannerX, _NoisePannerY) * _Time.y));
                // float Noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, TRANSFORM_TEX(NoiseUV, _NoiseTex));
                                //Unity_GradientNoise_half(float2(IN.uv0.x + _SpeedNoise * _Time.x * 0.01, IN.uv0.y + _SpeedNoise * _Time.x), _NoiseScale) - _NoiseStrength;
                //half cascadeIndex = ComputeCascadeIndex(wpos);
                //float4 coords = mul(_MainLightWorldToShadow[cascadeIndex], float4(wpos, 1.0));
                           
               // ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
                //half4 shadowParams = GetMainLightShadowParams();
                //atten = inside ? SampleShadowmap(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), coords, shadowSamplingData, shadowParams, false) : 1.0f;
                half atten = ShadowAtten(IN.wpos);
                return atten * 0.5;
            }
            ENDHLSL
        }
    }
}
