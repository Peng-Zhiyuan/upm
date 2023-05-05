#ifndef SP_REFLECTION_FUNC_INCLUDED
#define SP_REFLECTION_FUNC_INCLUDED

TEXTURECUBE(_CustomReflectionMap);
sampler2D _ReflectionMap;



inline half fibonacci1D(int i)
{
	//return 0.618034;
	return frac((half(i)+1.0) * 1.618034);
}

inline half2 fibonacci2D(int i, int nbSamples)
{
	return half2(
		(half(i)+0.5) / half(nbSamples),
		fibonacci1D(i)
		);
}

inline half3 importanceSampleGGX(half2 Xi, half3 T, half3 B, half3 N, half roughness)
{
	half a = roughness * roughness;
	half cosT = sqrt((1.0 - Xi.y) / (1.0 + (a * a - 1.0) * Xi.y));
	half sinT = sqrt(1.0 - cosT * cosT);
	half phi = 2.0 * 3.14159265 * Xi.x;
	return
		T * (sinT * cos(phi)) +
		B * (sinT * sin(phi)) +
		N * cosT;
}

inline half normal_distrib(
	half ndh,
	half Roughness)
{
	// use GGX / Trowbridge-Reitz, same as Disney and Unreal 4
	// cf http://blog.selfshadow.com/publications/s2013-shading-course/karis/s2013_pbs_epic_notes_v2.pdf p3
	half alpha = Roughness * Roughness;
	half tmp = alpha / max(1e-8, (ndh * ndh * (alpha * alpha - 1.0) + 1.0));
	return tmp * tmp * 0.31830988f;
}

inline half probabilityGGX(half ndh, half vdh, half Roughness)
{
	return normal_distrib(ndh, Roughness) * ndh / (4.0 * vdh);
}

inline half distortion(half3 Wn)
{
	// Computes the inverse of the solid angle of the (differential) pixel in
	// the cube map pointed at by Wn
	half sinT = sqrt(1.0 - Wn.y * Wn.y);
	return sinT;
}

inline half computeLOD(half nbSamples, half3 Ln, half p)
{
	return max(0.0, 4.5 - 0.5 * log2(half(nbSamples)*p * distortion(Ln)));
}

inline half3 envSampleLOD(half3 dir, half lod,half environment_rotation)
{
	// WORKAROUND: Intel GLSL compiler for HD5000 is bugged on OSX:
	// https://bugs.chromium.org/p/chromium/issues/detail?id=308366
	// It is necessary to replace atan(y, -x) by atan(y, -1.0 * x) to force
	// the second parameter to be interpreted as a float
	half2 pos = 0.31830988f * half2(atan2(-dir.z, -1.0 * dir.x), 2.0 * asin(dir.y));
	pos = 0.5 * pos + half2(0.5, 0.5);
	pos.x += environment_rotation;
	half4 rgba = tex2Dlod(_ReflectionMap, half4(pos, lod, lod));
	return DecodeRGBM(rgba);
}

#endif