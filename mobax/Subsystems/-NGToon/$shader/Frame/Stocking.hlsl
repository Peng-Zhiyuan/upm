#ifndef ROLE_STOCKING_INCLUDED
#define ROLE_STOCKING_INCLUDED

half _StockingRimPower;
half _StockingDenier;
half _StockingFresnelScale;
half _StockingFresnelMin;
half4 _StockingTint;
sampler2D _StockingDenierTex;
half _StockingGlossiness;
half4 _StockingSpecular;
half _StockingSpecularThreshould;
half _StockingSpecularSmooth;
half _StockingFresnelThreshould;
half _StockingFresnelSmooth;
half3 CalculateStock(half3 _diffuse, half ndl, half NdotH, half ndv,half2 uv, half3 lightColor)
{
    //half3 shadowColor = lerp(_SColor.rgb, half3(1, 1, 1), ndl);

    //half4 mainTex = tex2D(_Albedo, TRANSFORM_TEX(i.uv, _Albedo));

    //half ndv = dot(viewDirection, normalDirection);
    ndv = smoothstep(_StockingFresnelThreshould - 0.5 * _StockingFresnelSmooth, _StockingFresnelThreshould + 0.5 * _StockingFresnelSmooth, ndv);
    float rim = pow(abs(1 - ndv), _StockingRimPower / 10);   //边缘光
   
    float fresnel = pow(abs(1.0 - max(0, ndv)), _StockingFresnelScale);    //菲涅尔
    float density = max(rim, (_StockingDenier * (1 - tex2D(_StockingDenierTex, uv).r)));  //lerp参数

    half3 diffuse = lerp(_diffuse, _StockingTint, density).rgb;
    //   diffuse = diffuse * (1 - fresnel) * shadowColor * lightColor;
    diffuse = diffuse * (_StockingFresnelMin + (1 - fresnel) * (1 - _StockingFresnelMin))  * lightColor;

    float oneMinusReflectivity;
    half3 specularColor = _StockingSpecular.rgb;
	diffuse = CustomEnergyConservationBetweenDiffuseAndSpecular(diffuse, specularColor, oneMinusReflectivity);
    //diffuse = CustomDiffuseAndSpecularFromMetallic(diffuse, _MetallicIntensity, _OneMinusReflectivityIntensity, specularColor, oneMinusReflectivity);
    //specularColor *= _StockingSpecular.rgb;

    //half3 halfDirection = normalize(viewDirection + lightDirection);
    //half NdotH = saturate(dot(normalDirection, halfDirection));
    //half exponent = exp2(9.0 * _Glossiness);
    half exponent = exp2(9.0 *_StockingGlossiness);
    NdotH = smoothstep(_StockingSpecularThreshould - 0.5 * _StockingSpecularSmooth, _StockingSpecularThreshould + 0.5 * _StockingSpecularSmooth, NdotH);
    half3 specular = pow(NdotH, exponent) * specularColor; // Blinn-Phong
   /* specular = smoothstep(0.5 - 0.5 * _StockingSpecularSoft, 0.5 + 0.5 * _StockingSpecularSoft, NdotH);*/
    half3 albedo = diffuse + specular;
    return albedo;
}

#endif

