Shader "NGEnv/NGGaussianBlur"
{
    Properties
    {
        blurRangeX("Blur x", float) = 1
        blurRangeY("Blur y", float) = 1
        [HideInInspector]_MainTex ("ScreenTexture", 2D) = "" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass //pass 0
        {
            Name "BlurX"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float2 uv               : TEXCOORD0;
            };


            CBUFFER_START(UnityPerMaterial)
            float4  _MainTex_ST;
            float   blurRangeX = 1;
            float4  _MainTex_TexelSize;
            CBUFFER_END

            TEXTURE2D (_MainTex);
            SAMPLER(sampler_MainTex);
            #define  SampleCount  10

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;

                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv.xy  = TRANSFORM_TEX(v.uv.xy, _MainTex);

                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 col = 0;
                int s = 0;
                for (int j = -SampleCount; j <= SampleCount; j++)
                { 
                    int a = SampleCount - abs(j) + 1;
                    s += a;
                    half4 c = a * SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv + float2(j* blurRangeX * _MainTex_TexelSize.x, 0));
                    col += c;
                }
                col /= s;

                return col;
            }
            ENDHLSL
        }

        Pass //pass 1
        {
            Name "BlurY"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float2 uv               : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float blurRangeY = 1;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            #define  SampleCount  10

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;

                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv.xy  = TRANSFORM_TEX(v.uv.xy, _MainTex);

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 col = 0;
                int s = 0; 
                for (int j = -SampleCount; j <= SampleCount; j++)
                {
                    int a = SampleCount - abs(j) + 1;
                    s += a;
                    half4 c = a*SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv + float2(0, j* blurRangeY * _MainTex_TexelSize.y));
                    col += c;
                }
                col /= s;
                
                return col;
            }
            ENDHLSL
        }
    }
}
