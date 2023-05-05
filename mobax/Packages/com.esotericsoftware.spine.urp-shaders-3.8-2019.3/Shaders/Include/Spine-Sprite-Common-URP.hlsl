#ifndef URP_SPRITE_COMMON_INCLUDED
#define URP_SPRITE_COMMON_INCLUDED

#undef LIGHTMAP_ON

#if defined(_SPECULAR) || defined(_SPECULAR_GLOSSMAP)
#define SPECULAR
#endif

//Have to process lighting per pixel if using normal maps or a diffuse ramp or rim lighting or specular
#if defined(_NORMALMAP) || defined(_DIFFUSE_RAMP) || defined(_RIM_LIGHTING) || defined(SPECULAR)
#define PER_PIXEL_LIGHTING
#endif

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "SpineCoreShaders/ShaderShared.cginc"



sampler2D _MetallicGlossMap;




inline half4 getSpecTex(float2 uv)
{
	return tex2D(_MetallicGlossMap, uv).rgba; 
}



inline half getSpecTex1(float2 uv,half dir1,half dir2)
{
	//if (dir1 > 0.5) {
	//	return tex2D(_MetallicGlossMap, uv).r;
	//}
	//else if(dir2>0.5) {
	//	return tex2D(_MetallicGlossMap, uv).b;
	//}
	//else {


	//	
	//	return tex2D(_MetallicGlossMap, uv).g;
	//}

	half3 _rgb = tex2D(_MetallicGlossMap, uv).rgb;
	return _rgb.r * saturate(dir1) + _rgb.b * saturate(dir2) +max(0, (1 - max(dir1, dir2))) * _rgb.g;

	
}





#endif // URP_SPRITE_COMMON_INCLUDED
