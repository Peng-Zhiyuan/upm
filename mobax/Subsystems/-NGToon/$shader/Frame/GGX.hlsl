#ifndef GGX_FUNC_INCLUDED
#define GGX_FUNC_INCLUDED

#ifdef UNITY_COLORSPACE_GAMMA
#define unity_ColorSpaceDielectricSpec half4(0.220916301, 0.220916301, 0.220916301, 1.0 - 0.220916301)

#else // Linear values
#define unity_ColorSpaceDielectricSpec half4(0.04, 0.04, 0.04, 1.0 - 0.04) // standard dielectric reflectivity coef at incident angle (= 4%)
#endif

inline half CustomSmithJointGGXVisibilityTerm(half NdotL, half NdotV, half roughness)
{
	// Approximation of the above formulation (simplify the sqrt, not mathematically correct but close enough)
	half lambdaV = NdotL * (NdotV * (1 - roughness) + roughness);
	half lambdaL = NdotV * (NdotL * (1 - roughness) + roughness);

#if defined(SHADER_API_SWITCH)
	return 0.5f / (lambdaV + lambdaL + 1e-4f); // work-around against hlslcc rounding error
#else
	return 0.5f / (lambdaV + lambdaL + 1e-5f);
#endif
}

inline half CustomGGXTerm(half NdotH, half roughness)
{
	half a2 = roughness * roughness;
	half d = (NdotH * a2 - NdotH) * NdotH + 1.0f; // 2 mad
	return INV_PI * a2 / (d * d + 1e-7f); // This function is not intended to be running on Mobile,
												// therefore epsilon is smaller than what can be represented by half
}

inline half CustomPow5(half x)
{
	return x * x * x * x * x;
}
/*

inline half D_Term(half NdotH, half roughness)
{
	half a2 = roughness * roughness;
	half d = (NdotH * a2 - NdotH) * NdotH + 1.0f; // 2 mad
	return INV_PI * a2 / (d * d + 1e-7f); // This function is not intended to be running on Mobile,
												// therefore epsilon is smaller than what can be represented by half
}

//F项 直接光
inline half3 F_Term(half HdotL,half3 F0)
{
    half Fre = exp2((-5.55473*HdotL-6.98316)*HdotL);
    return lerp(Fre,1,F0);
}


//G项子项
half G_section(half dot,half k)
{
	 half nom=dot;
     half denom=lerp(dot,1,k);
     return nom/denom;
}



//G项 直接光
float G_Term(half NdotL,half NdotV,half roughness)
{

   float k = pow(1+roughness,2)/8;

   float Gnl = G_section(NdotL,k);

   float Gnv = G_section(NdotV,k);

   return Gnl*Gnv;

}
*/

inline half3 CustomFresnelTerm(half3 F0, half LdotH)
{
	half t = CustomPow5(1 - LdotH);   // ala Schlick interpoliation
	return F0 + (1 - F0) * t;
}

inline half3 CustomFresnelLerp(half3 F0, half3 F90, half LdotH)
{
	half t = CustomPow5(1 - LdotH);   // ala Schlick interpoliation
	return lerp(F0, F90, t);
}

inline half OneMinusReflectivityFromMetallic(half metallic)
{
	// We'll need oneMinusReflectivity, so
	//   1-reflectivity = 1-lerp(dielectricSpec, 1, metallic) = lerp(1-dielectricSpec, 0, metallic)
	// store (1-dielectricSpec) in unity_ColorSpaceDielectricSpec.a, then
	//   1-reflectivity = lerp(alpha, 0, metallic) = alpha + metallic*(0 - alpha) =
	//                  = alpha - metallic * alpha
	half oneMinusDielectricSpec = unity_ColorSpaceDielectricSpec.a;
	return oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
}

inline half3 CustomDiffuseAndSpecularFromMetallic(half3 albedo, half metallic, half oneMinusReflectivityIntensity,out half3 specColor, out half oneMinusReflectivity)
{
	specColor = lerp(unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
	oneMinusReflectivity = OneMinusReflectivityFromMetallic(metallic);
	return lerp(albedo, albedo * oneMinusReflectivity, oneMinusReflectivityIntensity);
}

half SpecularStrength(half3 specular)
{
#if (SHADER_TARGET < 30)
	// SM2.0: instruction count limitation
	// SM2.0: simplified SpecularStrength
	return specular.r; // Red channel - because most metals are either monocrhome or with redish/yellowish tint
#else
	return max(max(specular.r, specular.g), specular.b);
#endif
}

inline half3 CustomEnergyConservationBetweenDiffuseAndSpecular(half3 albedo, half3 specColor, out half oneMinusReflectivity)
{
	oneMinusReflectivity = 1 - SpecularStrength(specColor);
#if !UNITY_CONSERVE_ENERGY
	return albedo;
#elif UNITY_CONSERVE_ENERGY_MONOCHROME
	return albedo * oneMinusReflectivity;
#else
	return albedo * (half3(1, 1, 1) - specColor);
#endif
}

inline half GGXSpecularPBL(half NdotL,half NdotV,half NdotH,half roughness)
{
	half visTerm = CustomSmithJointGGXVisibilityTerm(NdotL, NdotV, roughness);
	half normTerm = CustomGGXTerm(NdotH, roughness);
	half specularPBL = (visTerm * normTerm) * PI;
	specularPBL = max(0, specularPBL * NdotL);
	return specularPBL;
}
/*
inline half3 GGXSpecular(half NdotL,half NdotV,half NdotH,half LdotH,half roughness, half3 F0)
{
	 half D = D_Term(NdotH,roughness);
     half G = G_Term(NdotL,NdotV,roughness);
     half3 F = F_Term(LdotH,F0);
     half3 BRDFSpeSection = D*G*F/(4*NdotL*NdotV);
     half3 DirectSpeColor = BRDFSpeSection * NdotL * PI;
     return DirectSpeColor;
}
*/
#endif