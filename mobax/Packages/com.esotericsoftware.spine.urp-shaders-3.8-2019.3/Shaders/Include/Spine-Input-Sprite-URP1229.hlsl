#ifndef URP_INPUT_SPRITE_INCLUDED
#define URP_INPUT_SPRITE_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerMaterial)
//sampler2D _MainTex;
float4 _MainTex_ST;
half4 _Color;
half _Cutoff;
//half _ShadowAlphaCutoff;




half4 _TintColor;
half _TintOffset;

half _GrayOffset;
half _GrayDir;
half _GrayColorOffset;


half _SpeOffset1;
half _SpeOffset2;
half _SpePow;
half _EmitOffset;

half _otherLightMax;

half4 _LightDirAdjust;


half _RoleHeight;
half _RoleRoot;
half _RootDarkLimit;
half _GuideH;

half4 _EmmisionColor;



float _PShadowHeight;
float4 _PShadowColor;
float _PShadowFalloff;
float4 _PSLightDir;



CBUFFER_END

#endif // URP_INPUT_SPRITE_INCLUDED
