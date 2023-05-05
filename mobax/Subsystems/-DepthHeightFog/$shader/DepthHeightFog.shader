
Shader "Universal Render Pipeline/CustomSSEffect/DepthHeightFog"
{
    Properties
    {
        //_MainTex("Base (RGB)", 2D) = "white" {}
    
        //[HDR]_FogColor("_FogColor (default = 1,1,1,1)", color) = (1,1,1,1)

        //[KeywordEnum(VIEWSPACE,WORLDSPACE)] _DIST_TYPE ("Distance type", int) = 0
        [KeywordEnum(VIEWSPACE)] _DIST_TYPE("Distance type", int) = 0
        [KeywordEnum(LINEAR,EXP,EXP2)] _FUNC_TYPE ("Calculate Func type", int) = 0
        [KeywordEnum(ADD,MUL)] _MIX_TYPE ("Mix type", int) = 0
        [HideInInspector]_MainTex("Texture",2D) = "white"{}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
     
        _FogColor ("Fog Color", Color) = (1, 1, 1, 1)
        _DepthNear ("Depth Near", Float) = 0
        _DepthFar ("Depth Far", Float) = 10
        _HeightStart ("Height Start", Float) = 0
        _HeightEnd ("Height End", Float) = 50

        _WorldPosScale ("WorldPos Scale", Range(0,0.1)) = 0.1
        _NoiseSpX ("Noise Speed X",Range(0,1)) = 0.1
        _NoiseSpY ("Noise Speed Y",Range(0,1)) = 0.1
        _DepthNoiseScale ("Depth Noise Scale",Range(0,30)) = 2
        _HeightNoiseScale ("Height Noise Scale",Range(0,30)) = 2

        _DepthDensity ("Fog Depth Density", Range(0, 1)) = 0.5
        _HeightDensity ("Fog Height Density", Range(0, 1)) = 0.5
        //[Toggle(_CutFogByDepth)] _CutFogByDepth("CutFogByDepth", Float) = 1
        _DepthHeightRatio ("Depth Height Ratio",Range(0,1)) = 0.5
       // _StencilRef("_StencilRef", Float) = 0
     
       // [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Comparison", Float) = 8 
        
    }



        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)

        //float _FogStartHeight;
        //float _FogHeight;
        //float _FogIntensity;

       
        float4 _CameraDepthTexture_ST;
        float4 _MainTex_ST;
        float4 _MainTex_TexelSize;
        float4 _NoiseTex_ST;
        half4 _FogColor;
        //float4x4 _Ray;
        float _HeightStart;
        float _HeightEnd;
        float _DepthFar;
        float _DepthNear;
        float _WorldPosScale;
        float _NoiseSpX;
        float _NoiseSpY;
        float _DepthNoiseScale;
        float _HeightNoiseScale;
        float _HeightDensity;
        float _DepthDensity;
        float _DepthHeightRatio;
        float _CutFogByDepth;
        CBUFFER_END

        //TEXTURE2D(_MainTex);
        //SAMPLER(sampler_MainTex);
        sampler2D _MainTex;
        sampler2D _NoiseTex;
        TEXTURE2D(_CameraDepthTexture);
        SAMPLER(sampler_CameraDepthTexture);



    struct appdata {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
        float3 ray : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };


    //vertex shader
    v2f vert(appdata v)
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        o.positionCS = TransformObjectToHClip(v.positionOS.xyz);

        float sceneRawDepth = 1;
 #if defined(UNITY_REVERSED_Z)
        sceneRawDepth = 1 - sceneRawDepth;
#endif
        float3 worldPos = ComputeWorldSpacePosition(v.uv, sceneRawDepth, UNITY_MATRIX_I_VP);
        o.ray = worldPos - _WorldSpaceCameraPos.xyz;
        o.uv = v.uv;
        return o;
    }

    //fragment shader
    float4 frag(v2f i) : SV_Target
    {
   
        //float4 finalCol;
        float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv);
        float3 wp = _WorldSpaceCameraPos.xyz + i.ray * Linear01Depth(depth, _ZBufferParams);

        float noise = tex2D(_NoiseTex, wp.xz * _WorldPosScale + _Time.x * half2(_NoiseSpX, _NoiseSpY)).r; 

        float dist = 0;
        #if _DIST_TYPE_VIEWSPACE 
            dist = LinearEyeDepth(depth, _ZBufferParams);
        #else
            dist = length(i.ray * Linear01Depth(depth, _ZBufferParams));
        #endif

        float depthFactor = 0;
        #if _FUNC_TYPE_LINEAR
            depthFactor =  max(0,(dist - _DepthNear)) / (_DepthFar - _DepthNear);
        #elif _FUNC_TYPE_EXP
            depthFactor = exp( _DepthDensity * 0.1 * dist);
        #else
            depthFactor = exp(pow(_DepthDensity * 0.1 * dist, 2));
        #endif
        
        float depthNoise = noise * _DepthNoiseScale;
        depthFactor *= depthNoise;
        depthFactor = saturate(depthFactor);

        float heightNoise = noise * _HeightNoiseScale;
        float heightFactor = max(0,(_HeightEnd - wp.y - heightNoise)) / (_HeightEnd - _HeightStart);
        
        heightFactor = saturate(heightFactor);
        float4 mainCol = tex2D(_MainTex, i.uv);

        #if _MIX_TYPE_ADD
             float4 depthCol = lerp(mainCol, _FogColor, depthFactor * _DepthDensity);
             float4 heightCol = lerp(mainCol, _FogColor, heightFactor * _HeightDensity);
             return lerp(depthCol, heightCol, _DepthHeightRatio);
        #else
             return lerp(mainCol, _FogColor, depthFactor * heightFactor * _DepthHeightRatio);
        #endif
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
             /*
             Name "DepthHeightFog"
             Stencil
             {
                Ref[_StencilRef]
                Comp[_StencilComp]
                Pass Replace
             }
             */

             HLSLPROGRAM
             #pragma multi_compile _ _DIST_TYPE_VIEWSPACE
             #pragma multi_compile _ _FUNC_TYPE_LINEAR _FUNC_TYPE_EXP
             #pragma multi_compile _ _MIX_TYPE_ADD
             #pragma vertex vert
             #pragma fragment frag
             ENDHLSL
        }

    }

}
