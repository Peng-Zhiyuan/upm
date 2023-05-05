
Shader "NGSSEffect/NGColorAdjustment"
{
    Properties
    {
        [HideInInspector]_MainTex("Albedo (RGB)", 2D) = "white" {}
        _ColorAdjustments("_ColorAdjustments", Color) = (1,1,1,1)	//调整颜色
        _ColorFilter("_ColorFilter", Color) = (0.0,0.0,0.0,0.0)
    }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Fullscreen.hlsl"
        CBUFFER_START(UnityPerMaterial)
        half4 _ColorAdjustments;
        half4 _ColorFilter;
		float4 _MainTex_ST;
        CBUFFER_END
        sampler2D _MainTex;
	


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

	inline half3 ColorGradePostExposure(half3 color)
	{
		return  color * _ColorAdjustments.x;
	}

	inline half3 ColorGradingContrst(half3 color)
	{
		color = LinearToLogC(color);
		color = (color - ACEScc_MIDGRAY) * _ColorAdjustments.y + ACEScc_MIDGRAY;
		return LogCToLinear(color);
	}
	/*
	half3 HSVToRGB(half3 c)
	{
		float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
		half3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
		return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
	}

	half3 RGBToHSV(half3 c)
	{
		float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
		float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
		float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
		float d = q.x - min(q.w, q.y);
		float e = 1.0e-10;
		return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
	}
	*/

	inline half3 ColorGradeColorFilter(half3 color)
	{
		return color * _ColorFilter.rgb * _ColorFilter.a;
	}

	inline half3 ColorGradingHueShift(half3 color)
	{
		color = RgbToHsv(color);
		half hue = color.x + _ColorAdjustments.z;
		color.x = RotateHue(hue, 0.0, 1.0);
		return HsvToRgb(color);
	}

    inline half3 ColorGradingSaturation(float3 color)
    {
        float luminance = Luminance(color);
        return (color - luminance)* _ColorAdjustments.w + luminance;
    }

	inline half3 ColorGrade(half3 color)
	{
		color = min(color, 60.0);
		color = ColorGradePostExposure(color);
		color = ColorGradingContrst(color);
		color = ColorGradeColorFilter(color);
		color = max(color, 0.0);
		color = ColorGradingHueShift(color);
        color = ColorGradingSaturation(color);
		return max(color, 0.0);
	}


    //fragment shader
    /*
    float4 frag(v2f i) : SV_Target
    {
		half4 color = tex2D(_MainTex, i.uv);
		return half4(ColorGrade(color.rgb), color.a);
    }*/

        float4 ToneMappingNonePassFragment(v2f i) : SV_TARGET
    {
        float4 color = tex2D(_MainTex, i.uv);
        color.rgb = ColorGrade(color.rgb);
        return color;
    }

        float4 ToneMappingACESPassFragment(v2f i) : SV_TARGET
    {
        float4 color = tex2D(_MainTex, i.uv);
        color.rgb = ColorGrade(color.rgb);
        color.rgb = AcesTonemap(unity_to_ACES(color.rgb));
        return color;
    }

        float4 ToneMappingNeutralPassFragment(v2f i) : SV_TARGET
    {
        float4 color = tex2D(_MainTex, i.uv);
        color.rgb = ColorGrade(color.rgb);
        color.rgb = NeutralTonemap(color.rgb);
        return color;
    }

        float4 ToneMappingReinhardPassFragment(v2f i) : SV_TARGET
    {
        float4 color = tex2D(_MainTex, i.uv);
        color.rgb = ColorGrade(color.rgb);
        color.rgb /= color.rgb + 1.0;
        return color;
    }

    ENDHLSL

    SubShader
    {

        //Tags {"RenderType" = "Opaque"  "RenderPipeline" = "UniversalPipeline"}
        Tags{ "RenderPipeline" = "UniversalPipeline"  "RenderType" = "Overlay" "Queue" = "Transparent-499" "DisableBatching" = "True" }
            LOD 100
            ZTest Always 
            Cull Off 
            ZWrite Off
            Blend one zero
            /*
            Pass
            {
             Name "NGColorAdjustment"
             HLSLPROGRAM

             #pragma vertex vert
             #pragma fragment frag
             ENDHLSL
            }
            */
            Pass
            {
             Name "ToneMappingNone"
             HLSLPROGRAM

             #pragma vertex vert
             #pragma fragment ToneMappingNonePassFragment
             ENDHLSL
            }
            /*
            Pass
            {
             Name "ToneMappingACES"
             HLSLPROGRAM

             #pragma vertex vert
             #pragma fragment ToneMappingACESPassFragment
             ENDHLSL
            }
            Pass
            {
             Name "ToneMappingNeutral"
             HLSLPROGRAM

             #pragma vertex vert
             #pragma fragment ToneMappingNeutralPassFragment
             ENDHLSL
            }
                Pass
            {
             Name "ToneMappingReinhard"
             HLSLPROGRAM

             #pragma vertex vert
             #pragma fragment ToneMappingReinhardPassFragment
             ENDHLSL
            }
            */

        }

}
