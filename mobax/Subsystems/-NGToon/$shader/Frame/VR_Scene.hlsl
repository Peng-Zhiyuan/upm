#ifndef VR_SCENE_INCLUDED
#define VR_SCENE_INCLUDED

TEXTURE2D(_EffectMapA);
SAMPLER(sampler_EffectMapA);

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

// half4 _EffectColor;
// half4 _EffectParams;
// half4 _EffCenter;
// half EffRadius;
// half ItemRadius;
// half4 _EffectMapA_ST;

inline half3 VR_SceneTex(TEXTURE2D_PARAM(effmap ,sampler_effmap) ,half2 uv,half4 ST)
{
    half effMapA = SAMPLE_TEXTURE2D(effmap,sampler_effmap,uv * ST.xy + ST.zw ).x;         
    half effMapB = SAMPLE_TEXTURE2D(effmap,sampler_effmap,uv * ST.xy * 0.25 + ST.zw ).y;
    half effMapC = SAMPLE_TEXTURE2D(effmap,sampler_effmap,uv * ST.xy * 0.01 + half2(ST.z * _Time.x , ST.w )).z;
    return half3 (effMapA,effMapB,effMapC);
}


inline void Radius_Mix(half3 effectColor ,half3 effMap ,half radiu,half EffRadius,half ItemRadius, out half3 ItemGird ,out half opacity ,out half valsmoothB,out half valB , out half grid)
{
    half valsmoothA = saturate ( lerp(0,1,(radiu-EffRadius)*0.03));

    half valsmoothAa = saturate ( lerp(0,1,(radiu-EffRadius-14.75)))*4;

    valsmoothB = saturate( lerp(0,1,(radiu-ItemRadius)*0.061));

    half valsmoothC = saturate(lerp(0,1,(radiu-ItemRadius + 10)*0.03));

    half valsmoothD =  pow(saturate( lerp(0,1,(radiu-ItemRadius -14))),6)*1.5;

    half valA = saturate ( step(radiu, EffRadius + 15));

    valB = saturate ( step(radiu, ItemRadius + 15));

    half valAM = saturate(( valsmoothA  + valsmoothAa) *valA);

    grid =  valA * (1 - effMap.x) * effMap.z  +  valAM  +  pow (valsmoothA * valAM,5)  + (1-valsmoothA) * 0.3 *  valAM  +  valA * (1- valB) * 0.3  + valB * valsmoothD ;

    opacity = saturate(grid +   valB + saturate( pow (valsmoothB * valB,5)));

    ItemGird = lerp(0, grid.xxx *effectColor  ,saturate( pow (valsmoothC * valB * 1.5,5)));

}



#endif