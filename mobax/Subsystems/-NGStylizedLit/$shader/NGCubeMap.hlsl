#ifndef CUBE_MAP_INCLUDED
#define CUBE_MAP_INCLUDED
#ifdef CUBE_MAP
/*
half _UseReflectMap;
samplerCUBE _CubeMap;
sampler2D _MatMap;
half _ReflectAmount;
half3 _MapColor;
half4 _GlobalSunPos;
half3 _GlobalSunColor;

float4 unity_SpecCube0_ProbePosition;
float4 unity_SpecCube0_BoxMin;
float4 unity_SpecCube0_BoxMax;

float4 _SpecCube0_ProbePosition;
float4 _SpecCube0_BoxMin;
float4 _SpecCube0_BoxMax;
*/
inline float3 BoxProjectedCubemapDirection (float3 worldRefl, float3 worldPos, float4 cubemapCenter, float4 boxMin, float4 boxMax)
{
    
    //UNITY_BRANCH
    //if (cubemapCenter.w > 0.0)         // 判断反射探头（Reflection Probe组件）是否勾选了BoxProjection
    {

       	half3 nrdir = normalize(worldRefl);

        half3 rbmax = (boxMax.xyz - worldPos) / nrdir;
        half3 rbmin = (boxMin.xyz - worldPos) / nrdir;

        half3 rbminmax = (nrdir > 0.0f) ? rbmax : rbmin;

        half fa = min(min(rbminmax.x, rbminmax.y), rbminmax.z);

        worldPos -= cubemapCenter.xyz;
        worldRefl = worldPos + nrdir * fa;

    }
    return worldRefl;
}

inline half IsCollision(half3 source, half3 rayDir, half3 center, half radius)
{
    half3 offset = center - source;
    half e = length(offset);
    half a = dot(offset, normalize(rayDir));
    return radius * radius - (e * e - a * a);
}

inline half3 CubeMapReflect(half3 color, half3 refWS, half2 uv, half3 world_pos, half mask)
{
    //refWS = BoxProjectedCubemapDirection(refWS, world_pos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
    //refWS = BoxProjectedCubemapDirection(refWS, world_pos, _SpecCube0_ProbePosition, _SpecCube0_BoxMin, _SpecCube0_BoxMax);
    //half mask = tex2D(_ReflectMask, uv).b;
    half4 cubeMapCol = texCUBElod(_CubeMap, half4(refWS, 0));// SAMPLE_TEXTURECUBE_LOD(_CubeMap, sampler_CubeMap, refWS, _LOD);
   //half light_strength = length(refWS - lightDir);//dot(lightDir, refWS);
    //half3 sunPos = lerp(half3(-3, 3, -68), _GlobalSunPos.xyz, step(0.r,_GlobalSunPos.w);
    half3 specular = _GlobalSunColor  * smoothstep(0,0.35 * _GlobalSunPos.w,IsCollision(world_pos, refWS, _GlobalSunPos.xyz, _GlobalSunPos.w));
    color = lerp(color, color * (1 - _ReflectAmount) + cubeMapCol * _ReflectAmount * _MapColor.rgb + specular * mask, step(0.1,mask));
    //half ndl_origin = dot(normalDirection, templightDir);
    return color;
}

inline half3 MatCapReflect(half3 color, half3 normalDirWS, half2 uv, half mask)
{
    //half mask = tex2D(_ReflectMask, uv).r;
    float3 nDirVS = normalize(mul(UNITY_MATRIX_V, normalDirWS));
    float2 matcapUV = nDirVS.rg * 0.5 + 0.5;
    half4 matCol = tex2D(_MatMap, matcapUV);// SAMPLE_TEXTURE2D(_MatMap, sampler_MatMap, matcapUV);
    color = lerp(color, color * (1 - _ReflectAmount) + matCol * _ReflectAmount * _MapColor.rgb, step(0.5, mask));
    return color;
}

#endif
#endif