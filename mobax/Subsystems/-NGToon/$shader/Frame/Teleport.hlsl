#ifndef TELEPORT_FUNC_INCLUDED
#define TELEPORT_FUNC_INCLUDED

half _UseTeleportEffect;
sampler2D _TeleportMask;
half4 _TeleportMask_ST;
half _TeleportCurrentTime;
half _TeleportMaskUVScale;
half _TeleportMoveSpeed;
half _TeleportFadeSpeed;
half4 _TeleportBaseColor;
half4 _TeleportRimColor;
half _TeleportRimMin;
half _TeleportRimMax;
half _TeleportRimTime;
half _TeleportColorTime;

#define COMPUTE_TELEPORT_DATA(o,vertex,spos) { if(_UseTeleportEffect>0){o.spos = ComputeScreenPos(o.vertex); o.spos.xy /= o.spos.w; half ct = saturate(_TeleportCurrentTime - _TeleportRimTime - _TeleportColorTime); o.vertex.y = lerp(o.vertex.y - ct * _TeleportMoveSpeed,o.vertex.y , 1 - o.spos.y - ct);}}

#define TELEPORT_CLIP(uv) {  if(_UseTeleportEffect>0){half4 mask = tex2D(_TeleportMask, uv.xy * half2(_TeleportMaskUVScale,_TeleportMaskUVScale));half m = lerp(0, mask.r, (1 - uv.y));clip(m- saturate(_TeleportCurrentTime- _TeleportRimTime- _TeleportColorTime) * _TeleportFadeSpeed);}}

#define TELEPORT_CLIP_OUTLINE { if(_UseTeleportEffect>0){ clip(-1);}}

inline half4 ComputeTeleportColor(half4 diffuse,half3 viewDirection, half3 normalDirection)
{
	if (_UseTeleportEffect > 0) 
	{
		half rim = 1.0f - saturate(dot(viewDirection, normalDirection));
		rim = smoothstep(_TeleportRimMin, _TeleportRimMax, rim);
		diffuse = lerp(diffuse, _TeleportBaseColor, saturate((_TeleportCurrentTime - _TeleportRimTime) / _TeleportColorTime));
		half4 rimColor = lerp(diffuse, _TeleportRimColor, rim);
		half4 col = lerp(diffuse, rimColor, saturate(_TeleportCurrentTime / _TeleportRimTime));
		return col;
	}
	else 
	{
		return diffuse;
	}
	
}

#endif