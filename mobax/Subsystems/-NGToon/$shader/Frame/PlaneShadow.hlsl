#ifndef PLANE_SHADOW_FUNC_INCLUDED
#define PLANE_SHADOW_FUNC_INCLUDED

half4 _ShadowPlane;
half4 _ShadowProjDir;
half4 _WorldPos;
half _ShadowInvLen;
half4 _ShadowFadeParams;

struct appdata
{
	half4 vertex : POSITION;
};

struct v2ff
{
	half4 vertex : SV_POSITION;
	half3 xlv_TEXCOORD0 : TEXCOORD0;
	half3 xlv_TEXCOORD1 : TEXCOORD1;
	half distance : TEXCOORD2;
};

v2ff vert_plane_shadow(appdata v)
{
	v2ff o;

	half3 lightdir = normalize(_ShadowProjDir).xyz;
	half3 worldpos = mul(unity_ObjectToWorld, v.vertex).xyz;
	// _ShadowPlane.w = p0 * n  
	half distance = (_ShadowPlane.w - dot(_ShadowPlane.xyz, worldpos)) / dot(_ShadowPlane.xyz, lightdir.xyz);
	worldpos = worldpos + distance * lightdir.xyz;
	o.vertex = mul(unity_MatrixVP, half4(worldpos, 1.0));
	o.xlv_TEXCOORD0 = _WorldPos.xyz;
	o.xlv_TEXCOORD1 = worldpos;
	o.distance = distance;

	return o;
}

half4 frag_plane_shadow(v2ff i) : SV_Target
{
	clip(i.distance);
	half3 posToPlane_2 = (i.xlv_TEXCOORD0 - i.xlv_TEXCOORD1);
	half4 color;
	color.xyz = half3(0.0, 0.0, 0.0);

	color.w = (pow((1.0 - clamp(((sqrt(dot(posToPlane_2, posToPlane_2)) * _ShadowInvLen) - _ShadowFadeParams.x), 0.0, 1.0)), _ShadowFadeParams.y) * _ShadowFadeParams.z);

	return color;
}

#endif