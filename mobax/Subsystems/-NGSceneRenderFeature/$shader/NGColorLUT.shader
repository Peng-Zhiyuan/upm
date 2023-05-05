
Shader "NGSSEffect/NGColorLUT"
{
    Properties
    {
        [HideInInspector]_MainTex("Albedo (RGB)", 2D) = "white" {}
        _LUT("Base (RGB)", 2D) = "" {}
        _Strength("Strength", Range(0, 1)) = 1
    }

    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
    CBUFFER_START(UnityPerMaterial)
	float4 _MainTex_ST;
    float4 _LUT_ST;
    float _ColorGradingLUTInLogC;
    float4 _LUTParameters;
    half _Strength;
    CBUFFER_END
    sampler2D _MainTex;
   // sampler2D _LUT;
   
    TEXTURE2D(_LUT);
    SAMPLER(sampler_linear_clamp);//work

    struct appdata {
        half4 positionOS : POSITION;
        half2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f {
        half4 positionCS : SV_POSITION;
        half2 uv:TEXCOORD;
        UNITY_VERTEX_OUTPUT_STEREO
    };
    
    v2f vert(appdata v)
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
        o.uv= TRANSFORM_TEX(v.uv, _MainTex);
        return o;
    }

   
    half3 ApplyColorGradingLUT(half3 color)
    {
        float3 colorLutSpace = saturate(_ColorGradingLUTInLogC ? LinearToLogC(color) : color);
        return ApplyLut2D(TEXTURE2D_ARGS(_LUT, sampler_LinearClamp),colorLutSpace,_LUTParameters.xyz);
    }

    half4 frag(v2f i) : SV_Target
    {
		half4 color = tex2D(_MainTex, i.uv);
        color.rgb = LinearToSRGB(color.rgb);
        //color.rgb = LinearToSRGB(color.rgb);
        color.rgb = lerp(color.rgb, ApplyColorGradingLUT(color), _Strength);
        color.rgb = SRGBToLinear(color.rgb);
	    return half4(color.rgb, color.a);
    }

    ENDHLSL
    SubShader
    {
        Tags{ "RenderPipeline" = "UniversalPipeline"  "RenderType" = "Overlay" "Queue" = "Transparent-499" "DisableBatching" = "True" }
            LOD 100
            ZTest Always 
            Cull Off 
            ZWrite Off
            Blend one zero

            Pass
            {
             Name "NGColorLUT"
             HLSLPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             ENDHLSL
            }

        }

}
