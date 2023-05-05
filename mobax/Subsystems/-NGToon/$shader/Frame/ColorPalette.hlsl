#ifndef COLOR_PALETTE_INCLUDED
#define COLOR_PALETTE_INCLUDED

float4 _Region1Color;
float4 _Region2Color;
float4 _Region3Color;
float4 _Region4Color;


#ifdef _USE_HSV_SHADOW
float _ShadowThreshould;
float _ShadowColorH;
float _ShadowColorS;
float _ShadowColorV;
float _ShadowThreshould2;
float _ShadowColorH2;
float _ShadowColorS2;
float _ShadowColorV2;
#else
float4 _SColor;
float4 _SSColor;
float4 _Region2SColor;
float4 _Region2SSColor;
float4 _Region3SColor;
float4 _Region3SSColor;
float4 _Region4SColor;
float4 _Region4SSColor;
#endif


float3 CustomRgbToHsv(float3 c)
{
	float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
	float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 CustomHsvToRgb( float3 c , float threshould){
    float3 rgb = clamp( abs(fmod(c.x * 6  + float3(0.0,4.0,2.0),6)-3.0)-1.0, 0, 1);
    rgb = rgb*rgb*(3.0-2.0*rgb);
    return lerp(float3(1,1,1), rgb, c.y) * max(threshould, c.z);
}

inline void GetColorByColorPalette(float4 mainTex, float4 colorMask, out float3 color, out float3 sColor, out float3 ssColor)
{
	int index = 0;
#ifdef _USE_COLOR_PALETTE 
	int r = step(0.5, colorMask.r);
	int g = step(0.5, colorMask.g) * 2;
	int b = step(0.5, colorMask.b) * 3;
	int a = step(0.5, colorMask.a) * 4;
    index =  max(max(r, g), max(b, a));
	float4 RegionColors[6] =
	{
		float4(1,1,1,1),_Region1Color,_Region2Color,_Region3Color,_Region4Color,_Region4Color
	};
	color = mainTex.rgb * RegionColors[index].rgb;
#else
	color = mainTex.rgb;
#endif	

#ifdef _USE_HSV_SHADOW
	float3 diffuseHSV = CustomRgbToHsv(color.xyz);
	diffuseHSV += float3(_ShadowColorH / 360, _ShadowColorS, 0);
	diffuseHSV.z *= _ShadowColorV;
	sColor =  CustomHsvToRgb(diffuseHSV, _ShadowThreshould);

	float3 s_diffuseHSV = diffuseHSV+float3(_ShadowColorH2 / 360, _ShadowColorS2, 0);
	s_diffuseHSV.z *= _ShadowColorV2;
	ssColor = CustomHsvToRgb(s_diffuseHSV, _ShadowThreshould2);
#else
	float4 RegionSColors[6] =
	{
		_SColor,_SColor,_Region2SColor,_Region3SColor,_Region4SColor,_Region4SColor
	};

	float4 RegionSSColors[6] =
	{
		_SSColor,_SSColor,_Region2SSColor,_Region3SSColor,_Region4SSColor,_Region4SSColor
	};
	sColor = RegionSColors[index];
	ssColor = RegionSSColors[index];
#endif	
}
#endif