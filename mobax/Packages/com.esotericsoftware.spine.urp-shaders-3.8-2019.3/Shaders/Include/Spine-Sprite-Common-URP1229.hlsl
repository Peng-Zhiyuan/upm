#ifndef URP_SPRITE_COMMON_URP1229_INCLUDED
#define URP_SPRITE_COMMON_URP1229_INCLUDED

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


sampler2D _MetalMap;

inline half4 getSpecTex(float2 uv)
{
	return tex2D(_MetalMap, uv).rgba;
}


#endif
