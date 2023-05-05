Shader "NGRole/DissolutionPlus"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Albedo ("Albedo (RGB)", 2D) = "white" {}
        _CellWidth("CellWidth",Range(-1,1))=0.5
        _CellColor("CellColor",Color) = (0,0,0,0)
        _HighLightWidth("HighLightWidth",Range(-1,1))=0.5
        [HDR]_HighLightColor("HighLightColor",Color) = (1,1,1,1)
        _DissolutionTex("DissolutionTex",2D) = "white"{}
        _AlphaCutoff("AlphaCutoff",Range(0,1)) = 0
        _DissolutionWidth("DissolutionWidth",float) = 0.2
        _EmissionsIntensity("EmissionsIntensity",float) = 1.0
        _BlackFadeIn("BlackFadeIn",Range(0.01,1)) = 0
        [HDR]_EmissionsColor("EmissionsColor",Color) = (1,1,1,1)
    }
    SubShader
    {
        Pass
        {
            Name "BASE"
            Tags 
            {
                "RenderType" = "Opaque" 
                "Queue" = "AlphaTest+50"
                //"RenderType" = "TransparentCutout"
                //"Queue" = "Geometry"
                "LightMode" = "Cloth"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
           
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            //传入顶点着色器的数据
            struct a2v
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                half3 normal       : NORMAL;
            };
            //传入片元着色器的数据
            struct v2f
            {
                float4 worldPos : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float3 viewDir      : TEXCOORD1;
                half3 worldNormal  : TEXCOORD2;
                float2 uv2      :TEXCOORD3;
            };
            TEXTURE2D(_Albedo);
            SAMPLER(sampler_Albedo);
            TEXTURE2D(_DissolutionTex);
            SAMPLER(sampler_DissolutionTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _Albedo_ST;
                half4 _DissolutionTex_ST;
                half _HighLightWidth;
                half4 _HighLightColor;
                half _CellWidth;
                half4 _CellColor;
                half _AlphaCutoff;
                half _DissolutionWidth;
                half _EmissionsIntensity;
                half4 _EmissionsColor;
                half _BlackFadeIn;
            CBUFFER_END
            //顶点着色器
            v2f vert(a2v v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _Albedo);
                o.uv2 = TRANSFORM_TEX(v.uv,_DissolutionTex);
                o.worldPos = TransformObjectToHClip(v.vertex);
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos.xyz-TransformObjectToWorld(v.vertex.xyz));
                return o;
            }
            //片元着色器
            half4 frag(v2f i) : SV_Target
            {
                Light mainLight = GetMainLight();
                half LdotN = dot(mainLight.direction,i.worldNormal);
                LdotN = saturate(step(0,LdotN-_CellWidth));
                ///
                half3 halfAngle = normalize(normalize(mainLight.direction)+normalize(i.viewDir));
                half HdotN = saturate(dot(halfAngle,i.worldNormal));
                HdotN = saturate(ceil(HdotN-_HighLightWidth));
                ///
                half4 Albedo = SAMPLE_TEXTURE2D(_Albedo, sampler_Albedo, i.uv);
                half4 Dissolution = SAMPLE_TEXTURE2D(_DissolutionTex,sampler_DissolutionTex,i.uv2);
                ///
                half4 BaseColor = lerp(_CellColor,_Color,LdotN)*Albedo;
                half4 FinalColor= lerp(BaseColor,_HighLightColor+Albedo,HdotN);
               
                half em1 = step(0,Dissolution.r - _AlphaCutoff+_DissolutionWidth);
                half em2 = step(0,Dissolution.r - _AlphaCutoff-_DissolutionWidth);
               // half em1 = step(_AlphaCutoff +_DissolutionWidth,Dissolution.r);
               // half em2 = step(_AlphaCutoff -_DissolutionWidth,Dissolution.r);
                half em = em1 - em2;
                half4 emissions = em * _EmissionsIntensity *_EmissionsColor;
                FinalColor = lerp(FinalColor,0, _BlackFadeIn);
                clip(Dissolution.r - _AlphaCutoff);
                return FinalColor + emissions;
            }
            ENDHLSL
        }
    }
}