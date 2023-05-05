#ifndef NG_COLOR_PALETTE_INCLUDED
#define NG_COLOR_PALETTE_INCLUDED

half4 _Region1Color;
half4 _Region2Color;
half4 _Region3Color;
half4 _Region4Color;


#ifdef _USE_HSV_SHADOW
half _ShadowThreshould;
half _ShadowColorH;
half _ShadowColorS;
half _ShadowColorV;
half _ShadowThreshould2;
half _ShadowColorH2;
half _ShadowColorS2;
half _ShadowColorV2;
#else
half4 _SColor;
half4 _SSColor;
half4 _Region2SColor;
half4 _Region2SSColor;
half4 _Region3SColor;
half4 _Region3SSColor;
half4 _Region4SColor;
half4 _Region4SSColor;
#endif
/*
//HSV to RGB
float3 HSVConvertToRGB(float3 hsv)
{
	float R,G,B;
	//float3 rgb;
	if( hsv.y == 0 )
	{

		R = hsv.z * 255;
		G = hsv.z * 255;
		B = hsv.z * 255;
	}
	else
	{
		float var_r, var_g, var_b;
		float var_h = hsv.x * 6;
		if (var_h == 6)var_h = 0;
		int var_i = (int)var_h;//把var_h转化为整数var_i；
		float var_1 = hsv.z*(1 - hsv.y);
		float var_2 = hsv.z*(1 - hsv.y*(var_h - var_i));
		float var_3 = hsv.z*(1 - hsv.y*(1 - (var_h - var_i)));
		if (var_i == 0) { var_r = hsv.z; var_g = var_3; var_b = var_1; }
		else if (var_i == 1) { var_r = var_2; var_g = hsv.z; var_b = var_1; }
		else if (var_i == 2) { var_r = var_1; var_g = hsv.z; var_b = var_3; }
		else if (var_i == 3) { var_r = var_1; var_g = var_2; var_b = hsv.z; }
		else if (var_i == 4) { var_r = var_3; var_g = var_1; var_b = hsv.z; }
		else { var_r = hsv.z; var_g = var_1; var_b = var_2; }

		R = var_r * 255;
		G = var_g * 255;
		B = var_b * 255;
	}
	return float3(R,G,B);
}     
*/
/*
//RGB to HSV
float3 RGBConvertToHSV(float3 rgb)
{
	float R = rgb.x/255,G = rgb.y/255,B = rgb.z/255;
	float3 hsv;
	float max1=max(R,max(G,B));
	float min1=min(R,min(G,B));
	float del_max = max1 - min1;
	hsv.z = max1;
	if (del_max == 0)
	{
		hsv.x = 0;
		hsv.y = 0;
	}
	else
	{
		hsv.y = del_max / max1;
		float del_R = (((max1 - R) / 6) + (del_max / 2)) / del_max;
		float del_G = (((max1 - G) / 6) + (del_max / 2)) / del_max;
		float del_B = (((max1 - B) / 6) + (del_max / 2)) / del_max;
		if (R == max1)hsv.x = del_B - del_G;
		else if (G == max1)hsv.x = (1 / 3) + del_R - del_B;
		else if (B == max1)hsv.x = (2 / 3) + del_G - del_R;
		if (hsv.x < 0)hsv.x += 1;
		if (hsv.x > 1)hsv.x -= 1;
	}
	return hsv;
}
*/

half3 CustomHsvToRgb( half3 c , half threshould){
    half3 rgb = clamp( abs(fmod(c.x*6.0+half3(0.0,4.0,2.0),6)-3.0)-1.0, 0, 1);
    rgb = rgb*rgb*(3.0-2.0*rgb);
    return lerp(half3(1,1,1), rgb, c.y) * max(threshould, c.z);
}


half3 CustomRgbToHsv(half3 c)
{
    half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
    half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));

    half d = q.x - min(q.w, q.y);
    half e = 1.0e-10;
    return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}


inline void GetColorByColorPalette(half4 mainTex, half4 colorMask, out half3 color, out half3 sColor, out half3 ssColor)
{
	//int index = int(ceil(colorMask.r * 4));
	
	int r = step(0.5, colorMask.r);
	int g = step(0.5, colorMask.g);
	int b = step(0.5, colorMask.b);
	int a = step(0.5, colorMask.a);
	int index = max(max(r, g * 2), max(b * 3 , a * 4));
	
	half4 RegionColors[6] =
	{
		half4(1,1,1,1),_Region1Color,_Region2Color,_Region3Color,_Region4Color,_Region4Color
	};
	
	color = mainTex.rgb * RegionColors[index].rgb;
#ifdef _USE_HSV_SHADOW
	half3 diffuseHSV = CustomRgbToHsv(color.xyz); 
	diffuseHSV += half3(_ShadowColorH / 360, _ShadowColorS, 0);
	diffuseHSV.z *= _ShadowColorV;
	sColor = CustomHsvToRgb(diffuseHSV, _ShadowThreshould);

	half3 s_diffuseHSV = diffuseHSV + half3(_ShadowColorH2 / 360, _ShadowColorS2, 0);
	s_diffuseHSV.z *= _ShadowColorV2;
	ssColor = CustomHsvToRgb(s_diffuseHSV, _ShadowThreshould2);
#else
	half4 RegionSColors[6] =
	{
		_SColor,_SColor,_Region2SColor,_Region3SColor,_Region4SColor,_Region4SColor
	};

	half4 RegionSSColors[6] =
	{
		_SSColor,_SSColor,_Region2SSColor,_Region3SSColor,_Region4SSColor,_Region4SSColor
	};
	sColor = RegionSColors[index];
	ssColor = RegionSSColors[index];
#endif	
}
#endif