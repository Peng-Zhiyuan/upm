Shader "NGVertex/VertexAnimation"
{
    Properties
    {
        _Color("Color",Color) = (1,1,1,1)
        _Fresnel("Fade(X) Intancity(Y)",vector) = (0.81,5.2,1,1)
        _Animation("Repeat(XZ) Intancity(YW)",vector) = (1.7,0.42,3.59,0.38)
        _MaskScale("Maskscale",Range(0,1)) = 0.473
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent"
        }
        Pass
        {
            Tags
            {
                "LightModel" = "ForwardBase"
            }
            Cull Back ZWrite on ZTest LEqual Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float4 _Fresnel;
            float _MaskScale;
            float4 _Animation;
            CBUFFER_END

            struct Attributes
            {
                float4 posOS : POSITION;
                half3 normalOS :NORMAL;
            };

            struct Varyings
            {
                float4 posCS : SV_POSITION;
                half3 posOS :TEXCOORD0;
                half3 normalWS :TEXCOORD1;
                float3 posWS :TEXCOORD2;
            };

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                o.posOS = v.posOS;
                v.posOS.x += sin((v.posOS.y + _Time.y) * _Animation.x) * _Animation.y;
                v.posOS.z += sin((v.posOS.y + _Time.y) * _Animation.z) * _Animation.w;
                o.posCS = TransformObjectToHClip(v.posOS);
                o.posWS = TransformObjectToWorld(v.posOS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half mask = saturate(i.posOS.y * _MaskScale);

                // return mask;
                half3 N = normalize(i.normalWS);
                float3 V = normalize(_WorldSpaceCameraPos - i.posWS);
                half fresnel = pow(1 - saturate(dot(N, V)), _Fresnel.x) * _Fresnel.y;
                half3 final = fresnel * _Color.rgb;
                return half4(final, mask);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}