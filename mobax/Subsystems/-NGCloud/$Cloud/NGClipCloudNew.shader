Shader "NGEnv/NGClipCloudNew"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BaseColor("BaseColor", Color) = (1,1,1,1)
        _EdgeColor("EdgeColor", Color) = (1,1,1,1)
        _EdgeIntensity("_EdgeIntensity", float) = 1.5
        _ShadowColor("ShadowColor", Color) = (1,1,1,1)
        _ShadowThreshold("ShadowThreshold", Range(0, 1)) = 0
        _X_Num("X_Num", float) = 2
        _Y_Num("Y_Num", float) = 4
        _CloudIndex("CloudIndex", float) = 0
        _Dissolve("Dissolve", Range(0, 1)) = 0
        _DissolveExcess("DissolveExcess", Range(0, 1)) = .5
        _DissolveSoft("DissolveSoft", Range(0.01, 1)) = 0.1
        _TurbulenceTex("TurbulenceTex", 2D) = "white" {}
        _UVScale("UVScale", Range(0, 100)) = 20
        _TurbulenceSpeed("TurbulenceSpeed", Range(0, 10)) = 0
        _TurbulenceFactor("TurbulenceFactor", Range(-1, 1)) = 1

    }
        SubShader
        {
            Tags
            {
                "RenderPipeline" = "UniversalRenderPipeline"
                "Queue" = "Transparent"
                "RenderType" = "Transparent"
            }
            HLSLINCLUDE
            #include"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include"Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes
            {
                float3 positionOS: POSITION;
                half3 normalOS: NORMAL;
                half4 tangentOS: TANGENT;
                float2 texcoord: TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv: TEXCOORD0;
                float2 noise_uv: TEXCOORD1;
                float3 positionWS: TEXCOORD2;
                half3 normalWS: TEXCOORD3;
                half3 tangentWS: TEXCOORD4;
                half3 bitangentWS: TEXCOORD5;
                float4 positionCS: SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _TurbulenceTex_ST;
            float4 _ShadowColor;
            float4 _EdgeColor;
            float4 _BaseColor;
            float _X_Num, _Y_Num, _CloudIndex;
            // float _Contrast;
            float _Dissolve;
            float _DissolveExcess;
            float _DissolveSoft;
            float _ShadowThreshold;
            float _TurbulenceSpeed;
            float _TurbulenceFactor;
            half _EdgeIntensity;
            half _UVScale;
            CBUFFER_END
            sampler2D _MainTex;
            sampler2D _TurbulenceTex;

            float3 CustomHsvToRgb(float3 c) {
                float3 rgb = clamp(abs(fmod(c.x * 6 + float3(0.0,4.0,2.0),6) - 3.0) - 1.0, 0, 1);
                rgb = rgb * rgb * (3.0 - 2.0 * rgb);
                return lerp(float3(1,1,1), rgb, c.y);
            }

            float3 CustomRgbToHsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            ENDHLSL

            Pass
            {
                Tags { "LightMode" = "UniversalForward"}
                ZWrite off
                Blend SrcAlpha OneMinusSrcAlpha
                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                Varyings vert(Attributes input)
                {
                    Varyings output;
                    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS);
                    VertexNormalInputs vertexNormalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                    output.positionCS = vertexInput.positionCS;
                    output.positionWS = vertexInput.positionWS;
                    output.normalWS = vertexNormalInputs.normalWS;
                    output.tangentWS = vertexNormalInputs.tangentWS;
                    output.bitangentWS = vertexNormalInputs.bitangentWS;

                    float2 uv = TRANSFORM_TEX(input.texcoord, _MainTex);

                    const float xIndex = fmod(_CloudIndex, _X_Num);
                    const float yIndex = fmod(floor(_CloudIndex / _X_Num), _Y_Num);
                    //缩小uv到某个范围内
                    output.uv = float2(uv.x / _X_Num, uv.y / _Y_Num);
                    //移动uv框
                    output.uv.x += xIndex / _X_Num;
                    output.uv.y += yIndex / _Y_Num;

                    output.noise_uv = TRANSFORM_TEX(input.texcoord, _TurbulenceTex);
                    return output;
                }

                half4 frag(Varyings input) : SV_Target
                {
                    half4 cloud_tex = tex2D(_MainTex, input.uv);
                    float diff_alpha = saturate((2 * _ShadowThreshold - cloud_tex.r));
                    half3 main_col = lerp(_BaseColor.rgb, _ShadowColor.rgb, diff_alpha);
                    float dissolve_origin_alpha = saturate(cloud_tex.b + _DissolveExcess - _Dissolve * (1 + _DissolveExcess));

                    float2 noise_uv = input.noise_uv * _UVScale - _Time.x * _TurbulenceSpeed;

                    half4 noise_col = tex2D(_TurbulenceTex, noise_uv);
                    noise_uv = noise_col.rg * 2 - 1;
                    float2 uv_disturbance = input.uv + noise_uv * _TurbulenceFactor * 0.01;
                    half4 cloud_tex_disturbance = tex2D(_MainTex, uv_disturbance);

                    half dissolve_origin_alpha_disturbance = saturate(cloud_tex_disturbance.b + _DissolveExcess - _Dissolve * noise_col.r * (1 + _DissolveExcess));

                    dissolve_origin_alpha = dissolve_origin_alpha_disturbance * dissolve_origin_alpha;

                    float dissolve_alpha = smoothstep(0, _DissolveSoft, dissolve_origin_alpha);
                    float edgeIntensity = ((1 - smoothstep(0, _DissolveSoft, dissolve_origin_alpha)) + cloud_tex.g) * _EdgeIntensity;

                    float alpha = saturate(cloud_tex_disturbance.a * dissolve_alpha * _BaseColor.a);
                    return half4(main_col + _EdgeColor.rgb * _EdgeColor.a * edgeIntensity, alpha);
                }
                ENDHLSL
            }
        }
}