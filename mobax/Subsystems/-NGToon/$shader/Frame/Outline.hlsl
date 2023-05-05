#ifndef OUTLINE_FUNC_INCLUDED
#define OUTLINE_FUNC_INCLUDED
#include "CustomLightFunc.hlsl"
#ifdef _USE_DITHER
#include "Dither.hlsl"
#endif
//#ifdef _DISSOLVE_EFFECT
//#include "DissolveEffect.hlsl"
//#endif

struct appdata {
	float4 vertex : POSITION;
	half2 uv : TEXCOORD0;
#ifdef _OUTLINE_UV2_AS_NORMALS
    //half2 uv2 : TEXCOORD1;
	half2 uv3 : TEXCOORD2;
	half4 tangent : TANGENT;
#endif
	half3 normal : NORMAL;
#ifdef _OUTLINE_USE_VERTEXCOLOR
	half4 color :COLOR;
#endif
};

struct v2f {
	float4 pos : SV_POSITION;                           
	half2 uv : TEXCOORD0;
#if defined(_USE_DITHER)
    float4 spos :TEXCOORD1;
	float cameraDistance:TEXCOORD2;
#endif
};

#ifndef _LOW_QUALITY
sampler2D _OcclusionMap;
uniform float4 _OcclusionMap_ST;
#endif


half _OutLineWidth;
half _OutLineWidthOffset;
half4 _OutLineColor;
half _CameraDisStrength;
half _OutLineMin;
half _OutLineMax;
half _CameraDistance;
#ifndef _OUTLINE_USE_VERTEXCOLOR			
half _OutLineZOffset;
#endif
float3 OctahedronToUnitVector(float2 oct)
{
    float3 unitVec = float3(oct, 1 - dot(float2(1, 1), abs(oct)));

    if (unitVec.z < 0)
    {
        unitVec.xy = (1 - abs(unitVec.yx)) * float2(unitVec.x >= 0 ? 1 : -1, unitVec.y >= 0 ? 1 : -1);
    }

    return normalize(unitVec);
}
//half4 _OffsetPosition;
//half _OffsetProcess;
v2f vert_outline(appdata v)
{
    v2f o;
    
//#ifdef _OUTLINE_UV2_AS_NORMALS
//    half3 n = OctahedronToUnitVector(v.uv3);
//    /*
//    //unpack uv2
//    v.uv2.x = v.uv2.x * 255.0 / 16.0;
//    n.x = floor(v.uv2.x) / 15.0;
//    n.y = frac(v.uv2.x) * 16.0 / 15.0;
//    //get z
//    n.z = v.uv2.y;
//    //transform
//    n = n * 2 - 1;
//    */
//    half3 bitangent = normalize(cross(v.normal, v.tangent.xyz) * v.tangent.w);
//    half3x3 tangentTransform = half3x3(v.tangent.xyz, bitangent, v.normal);
//    half3 normal = normalize(mul(n, tangentTransform));
//#else
    half3 normal = v.normal;
//#endif

   // normal = v.normalcom.leo;
    float3 posWorld = TransformObjectToWorld(v.vertex.xyz);
    float4 vPos = half4(TransformWorldToView(posWorld), 1.0f);
    float cameraDis = length(vPos.xyz);
    half3 vNormal = normalize(mul((half3x3)UNITY_MATRIX_IT_MV, normal));
    half2 offset = TransformWViewToHClip(vNormal).xy;
    half outlineWidth = _OutLineWidth + _OutLineWidthOffset;

    outlineWidth = lerp(outlineWidth * _OutLineMin, outlineWidth * _OutLineMax, cameraDis * _CameraDisStrength);
#ifdef _OUTLINE_USE_VERTEXCOLOR
    vPos.xyz += normalize(vPos.xyz) * saturate(v.color.b - 0.9);
    offset = offset * outlineWidth * 0.01 * (1 - v.color.r);
#else
    vPos.xyz += normalize(vPos.xyz) * _OutLineZOffset;
    offset = offset * outlineWidth * 0.01;
#endif

#ifdef _USE_FILTER
    half4 occlusion =  tex2Dlod(_OcclusionMap,float4(TRANSFORM_TEX(v.uv, _OcclusionMap),0.0,0));
    offset *= smoothstep(0, 0.5, (1 - occlusion.g));
#endif
    o.pos = mul(UNITY_MATRIX_P, vPos);
    o.pos.xy += offset;
    o.uv = v.uv;
#ifdef _USE_DITHER
    o.spos = ComputeScreenPos(o.pos);
	o.cameraDistance = length(_WorldSpaceCameraPos - posWorld);
#endif
    return o;
}


half4 frag_outline(v2f i) : SV_Target{
    #ifdef _DISSOLVE_EFFECT
	    clip(-1);
    #else
        clip(_OutLineColor.a - 0.01);
        /*
        if(_OutLineColor.a == 0)
        {
            clip(-1);
        }
        */
        //clip(lerp(0, -1, step(_OutLineColor.a, 0))
    #endif

#ifdef _USE_DITHER

	half alpha = saturate((_CameraDistance - 1) / 2);
    //half alpha = saturate((i.cameraDistance - 3) / 5);
	ditherClip(i.spos.xy / i.spos.w, 0);
#endif
    
    #ifdef _LOW_QUALITY
    return _OutLineColor * _MainLightColor.rgba;// * half4(max(half3(0.5,0.5,0.5), CustomLightProbe(half3(1,1,1))), 1);
    #else 
    return _OutLineColor * _MainLightColor.rgba;// * half4(max(half3(0.5,0.5,0.5), CustomLightProbe(half3(1,1,1))), 1);
    #endif

}

#endif