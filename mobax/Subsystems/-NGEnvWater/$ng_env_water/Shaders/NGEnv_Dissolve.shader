Shader "NGEnv/Particle/Dissolve" {
    Properties {
        _MainColor ("Tint Color", Color) = (1,1,1,1)
        _MainTexBrightness ("Brightness", Float ) = 1
        _TurbulenceTex ("Wave Tex", 2D) = "bump" {}

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
            #define WAVE
            #include "Include/NGEnv_WATER_SURFACEDATA.hlsl"
            #pragma target 2.0           
            
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput IN) {
                VertexOutput OUT = (VertexOutput)0;
                OUT.uv0 = IN.texcoord0;
                OUT.uv1 = IN.texcoord1;
                OUT.vertexColor = IN.vertexColor;
                OUT.pos = TransformObjectToHClip( IN.vertex.xyz );
                return OUT;
            }
            float4 frag(VertexOutput IN) : SV_Target {
                // float2 NoiseUV = (IN.uv0 + (float2(_NoisePannerX, _NoisePannerY) * _Time.y));
                // float Noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, TRANSFORM_TEX(NoiseUV, _NoiseTex));
                                //Unity_GradientNoise_half(float2(IN.uv0.x + _SpeedNoise * _Time.x * 0.01, IN.uv0.y + _SpeedNoise * _Time.x), _NoiseScale) - _NoiseStrength;
                
                float2 TurbulenceUV = (IN.uv0 + (float2(IN.uv1.r, IN.uv1.g) * _Time.y));
                float4 _TurbulenceTex_var = SAMPLE_TEXTURE2D(_TurbulenceTex, sampler_TurbulenceTex, TRANSFORM_TEX(TurbulenceUV, _TurbulenceTex));
                clip(_TurbulenceTex_var.r + (1 - IN.uv1.b) - 0.5);

                float3 finalColor = _MainColor.rgb * (_MainTexBrightness * _TurbulenceTex_var.rgb * IN.vertexColor.rgb);
                float alpha = _MainColor.a * (IN.vertexColor.a * _TurbulenceTex_var.a);
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}
