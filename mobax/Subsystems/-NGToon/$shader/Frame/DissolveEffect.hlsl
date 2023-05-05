#ifndef DISSOLVE_EFFECT_INCLUDED
#define DISSOLVE_EFFECT_INCLUDED
#include "Base.hlsl"

CBUFFER_START(UnityPerMaterial)
half _DissolveRatio;
half4 _DissolveColor;
//half _DissolveHeight;
half _DissolveRange;
half _DissolveDir;
CBUFFER_END
sampler2D _DessolveMask;
half4 _DessolveMask_ST;
half3 ComputeDissolve(half3 albedo,half2 uv, half2 uv2)
{
    half4 maskTex = tex2D(_DessolveMask, TRANSFORM_TEX(uv, _DessolveMask));
    float uvY = lerp(maskTex.r, uv2.y, _DissolveDir);

    half dissolve_y = _DissolveRatio;// *_DissolveHeight;
    half dissolve_range = _DissolveRange;// min(dissolve_y, _DissolveRange);
    half em1 = step(0, uvY - dissolve_y + dissolve_range);
    half em2 = step(0, uvY - dissolve_y - dissolve_range);
    half em = em1 - em2;
    half3 emissions = em * _DissolveColor.rgb * _DissolveColor.a * smoothstep(0,1, dissolve_range - (uvY - dissolve_y));

    clip(uvY - dissolve_y);
    return albedo + emissions;
}
#endif