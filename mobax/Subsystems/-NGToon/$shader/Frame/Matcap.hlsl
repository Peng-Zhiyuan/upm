#ifndef MATCAP_FUNC_INCLUDED
#define MATCAP_FUNC_INCLUDED

#include "Base.hlsl"

half _UseMatcapEffect;
sampler2D _MatCap;
half4 _MatCapColor;
half _MatCapPower;
half _MatCapAlphaPower;
half _MatCapMultiply;
half _MatCapAdd;



#define COMPUTE_MATCAP_DATA(normal,matcap) { if(_UseMatcapEffect>0){float3 worldNorm = normalize(unity_WorldToObject[0].xyz * normal.x + unity_WorldToObject[1].xyz * normal.y + unity_WorldToObject[2].xyz * normal.z);worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);matcap.xy = worldNorm.xy * 0.5 + 0.5;} }

inline half3 ComputeMatcapColor(half3 albedo,half2 uv)
{
	if (_UseMatcapEffect > 0) 
	{
		half4 matcap = tex2D(_MatCap, uv);
		albedo =lerp(albedo, albedo * matcap.rgb * _MatCapColor.rgb * _MatCapPower, 1+_MatCapMultiply * _MatCapAlphaPower);
		albedo += matcap.rgb * matcap.a * _MatCapColor.rgb * _MatCapPower * _MatCapAdd;
	}
	return albedo;
}

#endif