#ifndef SPINE_SPRITE_SHADOW_URP1229
#define SPINE_SPRITE_SHADOW_URP1229

#include "Include/Spine-Sprite-Common-URP1229.hlsl"
#include "Include/Lighting1229.hlsl"
#include "SpineCoreShaders/SpriteLighting1229.cginc"


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


float3 GetLightDir(float positionWS)
{
    Light mainLight = GetMainLight();
    float3 dir = normalize(mainLight.direction);
    //#ifdef _ADDITIONAL_LIGHTS 
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, positionWS);
        dir += normalize(light.direction);
        return light.direction;
    }
    //#endif
    return normalize(dir);
}

float3 ShadowProjectPos(float4 vertPos)
{
    float3 shadowPos;

    //得到顶点的世界空间坐标
    float3 worldPos = mul(unity_ObjectToWorld, vertPos).xyz;
    float3 lightDir = normalize(_PSLightDir.xyz);


    //阴影的世界空间坐标（低于地面的部分不做改变）
    _PShadowHeight=0.02;
    shadowPos.y = min(worldPos.y, _PShadowHeight);
    shadowPos.xz = worldPos.xz - lightDir.xz * max(0, worldPos.y - _PShadowHeight) / lightDir.y;

    return shadowPos;
}


v2f vert(appdata v)
{
    v2f o;


    //得到阴影的世界空间坐标
    float3 shadowPos = ShadowProjectPos(v.vertex);

    //转换到裁切空间
    // o.vertex = UnityWorldToClipPos(shadowPos);
    o.vertex = TransformWorldToHClip(shadowPos);

    //得到中心点世界坐标
    float3 center = float3(unity_ObjectToWorld[0].w, _PShadowHeight, unity_ObjectToWorld[2].w);

    //计算阴影衰减
    float falloff = 1 - saturate(distance(shadowPos, center) * _PShadowFalloff);

    //阴影颜色

    o.color = _PShadowColor;
    o.color.a *= falloff;

    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

    return o;
}

half4 frag(v2f i) : SV_Target
{
    half4 f = i.color;
    //f.a = tex2D(_MainTex, i.uv).a * f.a;
    clip(tex2D(_MainTex, i.uv).a-0.01);
    return f;
}


#endif
