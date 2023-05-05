#ifndef NGHEIGHT_FOG_INCLUDED
#define NGHEIGHT_FOG_INCLUDED

#ifdef HEIGHT_FOG
//CBUFFER_START(UnityPerMaterial)
/*
half _DepthFogStart;
half _DepthFogRange;
half _DepthFogDensity;
half _HeightFogStart;
half _HeightFogRange;
*/
float4 _FogColor;
float4 _FogEmissionColor;// _MainTex_ST, _Color;
float  _FogMin, _FogMax, _FogEmissionPower, _FogEmissionFalloff, _FogFalloff, _FogRelativeWorldOrLocal;
float _FogWaveSpeedX,_FogWaveSpeedZ,_FogWaveAmplitudeX,_FogWaveAmplitudeZ,_FogWaveFreqX,_FogWaveFreqZ, _ANIMATION, _STANDARD_FOG, _OVERRIDE_FOG_COLOR;
//CBUFFER_END
/*
inline half CalculateFogVS(half3 _WorldPos)
{
	half3 f3Distance = _WorldPos - _WorldSpaceCameraPos;

	half fDepthFog = saturate((length(f3Distance) - _DepthFogStart) * _DepthFogRange);
	fDepthFog *= _DepthFogDensity;

	half fHeightFog = saturate((_HeightFogStart - f3Distance.y) * _HeightFogRange);

	return saturate(fDepthFog + fHeightFog);
}
*/

inline half3 waveCalc(half3 worldPos)
{

    if (_ANIMATION > 0) {
        float timeX = _Time.x * 20 * -_FogWaveSpeedX;
        float timeZ = _Time.x * 20 * -_FogWaveSpeedZ;
        float waveValueX = sin(timeX + worldPos.x * _FogWaveFreqX) * _FogWaveAmplitudeX;
        float waveValueZ = sin(timeZ + worldPos.z * _FogWaveFreqZ) * _FogWaveAmplitudeZ;
        float waveValue = (waveValueX + waveValueZ) / 2;
        return half3(worldPos.x, worldPos.y + waveValue, worldPos.z);
    }
    else {
        return worldPos;
    }

}
half4 DoHeightFogStuff(half4 color, half3 positionOS, half3 positionWS) 
{
    half3 localPos = waveCalc(positionOS);
    half3 wPos = waveCalc(positionWS);
    float lerpValue = clamp((((localPos.y * clamp(1 - _FogRelativeWorldOrLocal, 0, 1)) + (wPos.y * clamp(0 + _FogRelativeWorldOrLocal, 0, 1))) - _FogMin) / (_FogMax - _FogMin), 0, 1);
	lerpValue = 1 - pow(lerpValue, _FogFalloff);
    half4 emission = _FogColor + _FogEmissionColor * _FogEmissionPower;
    half4 fogEmissionColor = lerp(_FogColor, emission, pow(lerpValue, _FogEmissionFalloff));
    color = lerp(color, fogEmissionColor, lerpValue);
    return color;
}

#endif
#endif