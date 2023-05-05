
Shader "NGSSEffect/NGRadialBlur"
{
    Properties
    {
        [HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
        [HideInInspector]_StencilRef("_StencilRef", Float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Comparison", Float) = 8
        _Level("强度",Range(1,100))=10
		_CenterX("中心X坐标",Range(0,1))=0.5
		_CenterY("中心Y坐标",Range(0,1))=0.5
		_BufferRadius("缓冲半径",Range(0,1))=0
    }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_ST;
        float _Level;
		float _CenterX;
		float _CenterY;
		float _BufferRadius;
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


    //vertex shader
    v2f vert(appdata v)
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
        o.uv= TRANSFORM_TEX(v.uv, _MainTex);
        return o;
    }

    //fragment shader
    float4 frag(v2f i) : SV_Target
    {
      
		half4 finalColor;
		half2 center=half2(_CenterX,_CenterY);
		half2 uv=i.uv-center;
		half3 tempColor=half3(0,0,0);
		half blurParams=distance(i.uv,center);
		for(half j=0;j<_Level;j++){
			tempColor+=tex2D(_MainTex,uv*(1-0.01*j*saturate( blurParams/_BufferRadius))+center).rgb;
		}

		finalColor.rgb=tempColor/_Level;
		finalColor.a=1;
		return finalColor;
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
            Pass
            {
             Name "SSRadialBlur"
/*
             Stencil
             {
                Ref[_StencilRef]
                Comp[_StencilComp]
                Pass Replace
             }
*/
            
             HLSLPROGRAM
             //#pragma multi_compile _ _DIST_TYPE_VIEWSPACE
             //#pragma multi_compile _ _FUNC_TYPE_LINEAR _FUNC_TYPE_EXP

             #pragma vertex vert
             #pragma fragment frag
             ENDHLSL
        }

    }

}
