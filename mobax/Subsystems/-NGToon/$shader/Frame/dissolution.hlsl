#ifndef BASE_FUNC_INCLUDED
#define BASE_FUNC_INCLUDED

inline float DecodeFloatRGBA(float4 enc)
{
	float4 kDecodeDot = float4(1.0, 1 / 255.0, 1 / 65025.0, 1 / 16581375.0);
	return dot(enc, kDecodeDot);
}

inline float4 TransformHClipToViewPortPos(float4 positionCS)
 {
     float4 o = positionCS * 0.5f;
     o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
     o.zw = positionCS.zw;
     return o / o.w;
 }
#endif