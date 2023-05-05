Shader "NGEnv/NGClipCloudFinal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("BaseColor", Color) = (1,1,1,1) 
        _ShadowColor("ShadowColor", Color) = (1,1,1,1)
        _ShadowThreshold ("ShadowThreshold", Range(0, 1)) = 0
       
        _Dissolve("Dissolve", Range(0, 1)) = 0
        _DistortionStrength("DistortionStrength", Range(0, 1)) = 0.5

        _NoiseTex ("NoiseTex", 2D) = "white" {}
        _NoiseUV("Noise UV", Range(0,100)) = 1

        _RimColor("RimColor", Color) = (1,1,1,1)
        //_RimIntensity("RimIntensity", float) = 1.5

        _X_Num("X_Num", float) = 2
        _Y_Num("Y_Num", float) = 4
        _CloudIndex("CloudIndex", float) = 0
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
            float4 positionCS: SV_POSITION;
        };
        
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_ST;
        float4 _NoiseTex_ST;

        float4 _ShadowColor;
        float _ShadowThreshold;

        float4 _RimColor;
        float4 _BaseColor;
        //half _RimIntensity;
        float _X_Num, _Y_Num, _CloudIndex;
        float _Dissolve;
        float _DissolveSoft;

        half _NoiseUV;
        float _DistortionStrength;

        CBUFFER_END
        sampler2D _MainTex;
        sampler2D _NoiseTex;
        
        
        ENDHLSL
        
        Pass
        {    
            Tags { "LightMode" = "UniversalForward"}
            ZWrite off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            Varyings vert (Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS);
                output.positionCS = vertexInput.positionCS;

                float2 uv = TRANSFORM_TEX(input.texcoord, _MainTex);

                const float xIndex = fmod(_CloudIndex, _X_Num);
                const float yIndex = fmod(floor(_CloudIndex / _X_Num), _Y_Num);
                //缩小uv到某个范围内
                output.uv = float2(uv.x / _X_Num, uv.y / _Y_Num);
                //移动uv框
                output.uv.x += xIndex / _X_Num;
                output.uv.y += yIndex / _Y_Num;
                
                output.noise_uv = TRANSFORM_TEX(input.texcoord, _NoiseTex);
                return output;
            }
            
            half4 frag (Varyings input) : SV_Target
            {
                half4 cloudColor = tex2D(_MainTex, input.uv);
                half noise = tex2D(_NoiseTex, input.noise_uv * _NoiseUV - _Time.yy * 0.2).r;
                noise = (noise - 0.5) * 2;
                float2 uv = input.uv + noise * _DistortionStrength * 0.005;
                half4 distorbColor = tex2D(_MainTex, uv);

                half distorbGradient = 1 - smoothstep(distorbColor.b, distorbColor.b + 0.1, _Dissolve);
                half gradient = 1 - smoothstep(cloudColor.b, cloudColor.b + 0.1, _Dissolve);

                half4 finalColor = 0;
                half diff_col = saturate((2 * _ShadowThreshold - cloudColor.r));
                finalColor.rgb = lerp(_BaseColor.rgb, _ShadowColor.rgb, diff_col) + cloudColor.g * _RimColor.rgb * _RimColor.a;
                finalColor.a = gradient * distorbColor.a * distorbGradient;
                return finalColor;
            }
            ENDHLSL
        }
    }
}