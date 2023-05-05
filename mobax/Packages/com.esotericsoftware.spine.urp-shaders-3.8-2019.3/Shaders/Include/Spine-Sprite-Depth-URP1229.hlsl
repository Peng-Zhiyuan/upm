#ifndef SPINE_SPRITE_DEPTH_URP1229
#define SPINE_SPRITE_DEPTH_URP1229
#include "Include/Spine-Sprite-Common-URP1229.hlsl"
//#include "Include/Spine-Sprite-Common-URP.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
};

       
v2f vert (appdata v)
{
    v2f o;
    o.vertex = TransformObjectToHClip(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}

half4 frag(v2f i) : SV_Target
{
    clip(tex2D(_MainTex,i.uv).a-0.5);
    return 1;
}

#endif
