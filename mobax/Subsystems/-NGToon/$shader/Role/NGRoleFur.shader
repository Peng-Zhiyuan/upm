Shader "NGRole/Fur"
{
	Properties
    {
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
		[NoScaleOffset]_Albedo ("Albedo", 2D) = "white" { }
        [NoScaleOffset]_FurTex ("Fur Pattern", 2D) = "white" { }
		[NGSeparator]
		[NGHeader(Shadow)]
		_FSColor("Shadow Color", Color) = (0.195,0.195,0.195,1.0)
		_HColor("Highlight Color", Color) = (0.785,0.785,0.785,1.0)
		_FRampThreshold("Ramp Threshold", Range(0.001,1)) = 0.5
		_RampSmooth("Ramp Smoothing", Range(0.001,1)) = 0.1
        [NGHeader(Fur)]
        _FurLength ("Fur Length", Range(0.0, 1)) = 0.5
        _FurDensity ("Fur Density", Range(0, 2)) = 0.11
        _FurThinness ("Fur Thinness", Range(0.01, 10)) = 1
        _FurShading ("Fur Shading", Range(0.0, 1)) = 0.25       
        _ForceGlobal("Force Global",Vector) = (0,0,0,0)
        _ForceLocal("Force Local",Vector) = (0,0,0,0)
		[NGSeparator]
        [HideInInspector]_UseMatcapEffect("Use Matcap Effect",Float) = 0
    }
    
    Category
    {   
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "AlphaTest+50" "RenderPipeline" = "UniversalPipeline" }

            Pass
            {
                Tags{"LightMode" = "RoleForward"}
                Cull Back
                ZWrite On
                HLSLPROGRAM
                
                #pragma vertex vert_surface
                #pragma fragment frag_surface
                #include "../Frame/NGRoleFur.hlsl"
                
                ENDHLSL
                
            }

            Pass
            {
                Tags{"LightMode" = "RoleForwardLow"}
                Cull Back
                ZWrite On
                HLSLPROGRAM

                #pragma vertex vert_surface
                #pragma fragment frag_surface

                #define _LOW_QUALITY

                #include "../Frame/NGRoleFur.hlsl"

                ENDHLSL

            }


            Pass
            {
                Tags { "LightMode" = "FurRendererLayer" }
                Cull Off
                ZWrite Off
                Blend SrcAlpha OneMinusSrcAlpha
                HLSLPROGRAM
                
                #pragma vertex vert_base
                #pragma fragment frag_base
                #include "../Frame/NGRoleFur.hlsl"
                
                ENDHLSL
                
            }
/*
            Pass
            {
                Name "PlaneShadow"
                Tags { "LightMode" = "PlaneShadow" }
                Blend SrcAlpha  OneMinusSrcAlpha
                ZWrite Off
                Cull Back

                Stencil
                {
                    Ref 0
                    Comp Equal
                    WriteMask 255
                    ReadMask 255
                    Pass Invert
                    Fail Keep
                    ZFail Keep
                }

                HLSLPROGRAM

                #pragma vertex vert_plane_shadow
                #pragma fragment frag_plane_shadow

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "../Frame/PlaneShadow.hlsl"

                ENDHLSL
            }
*/
            
        }    
	}
	Fallback Off
    CustomEditor "PunishingFurInspector"
}