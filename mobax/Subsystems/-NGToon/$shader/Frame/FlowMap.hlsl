#ifndef FLOW_MAP_FUNC_INCLUDED
#define FLOW_MAP_FUNC_INCLUDED
#include "Base.hlsl"

sampler2D _FlowMap;
half4 _FlowMap_ST;

sampler2D _FlowMask;
half4 _FlowMask_ST;
half4 _FlowSpeed;
half4 _FlowColor;
half _FlowStrength;
inline half3 CalculateFlowLight(half3 baseColor, float2 uv,half3 wPos, half3 pos, half ndv) {
    //����
    float2 pos_offset = wPos.xy;      //pos_world - pos_self ������ռ�ƽ��uvʱ��������ͼ������ģ���ƶ�
    float2 uvspeed = pos_offset + _FlowSpeed.zw * _Time.y + _FlowSpeed.xy; //_Time.y
    half4 mask = tex2D(_FlowMask, uv);
    float2 uv_r = uvspeed + mask.rg;    //��uv��������ͼ����uv
    float3 flowMap_rgb = tex2D(_FlowMap, uv_r).xyz;
    float3 RunLight = flowMap_rgb.rgb * _FlowColor;//_SColor* _SColorMu;
    float3 fresnelColor = lerp(baseColor, _FlowColor, smoothstep(0.3,1,1- ndv));
    float3 OutPutColor = lerp(baseColor, fresnelColor.xyz + RunLight * _FlowStrength, mask.b);
    return OutPutColor; //float4
}

#endif