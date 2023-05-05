#ifndef VERTEX_LIT_FORWARD_PASS_URP_INCLUDED
#define VERTEX_LIT_FORWARD_PASS_URP_INCLUDED

#include "Include/Spine-Sprite-Common-URP1229.hlsl"
#include "Include/Lighting1229.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "SpineCoreShaders/SpriteLighting1229.cginc"

#if defined(_ADDITIONAL_LIGHTS) || defined(MAIN_LIGHT_CALCULATE_SHADOWS)
	#define NEEDS_POSITION_WS
#endif

////////////////////////////////////////
// Vertex output struct
//
struct VertexOutputLWRP
{
    float4 pos : SV_POSITION;
    fixed4 vertexColor : COLOR;
    float3 texcoord : TEXCOORD0;

    half3 viewDirectionWS : TEXCOORD1;

    half holyOffset:FLOAT;

    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 3);

    #if defined(_NORMALMAP)
    half4 normalWorld : TEXCOORD2;
    half4 tangentWorld : TEXCOORD3;
    half4 binormalWorld : TEXCOORD4;
    #else
	half3 normalWorld : TEXCOORD2;
    #endif
    #if (defined(_MAIN_LIGHT_SHADOWS) || defined(MAIN_LIGHT_CALCULATE_SHADOWS)) && !defined(_RECEIVE_SHADOWS_OFF)
	float4 shadowCoord : TEXCOORD5;
    #endif
    #if (defined(NEEDS_POSITION_WS)|| defined(_HOLY_LIGHT))
    float4 positionWS : TEXCOORD6;
    #endif
    UNITY_VERTEX_OUTPUT_STEREO
};

inline float getLightDirOffset(half4 rgba,half dir1,half dir2)
{
	float val=rgba.r * saturate(dir1) + rgba.b * saturate(dir2) +max(0, (1 - max(dir1, dir2))) * rgba.g;
	return pow(val,_SpePow);
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////



half4 MoonLightCal(InputData inputData, half4 texDiffuseAlpha, half4 vertexColor, half4 st,
                                                half2 uv, half holyOffset)
{
    half4 diffuse = texDiffuseAlpha * vertexColor;
    Light mainLight = GetMainLight();
    half3 diffuseLighting = inputData.bakedGI;
    half3 attenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
    half3 attenuatedLightColor = mainLight.color * attenuation;
    diffuseLighting += attenuatedLightColor;
    half3 addLight = half3(0, 0, 0);


	
	
    #ifdef _METAL_TEX
    half3 _left = -_LightDirAdjust.xyz;
    half3 _right = -_left;
    half _d1 = dot(_left, mainLight.direction);
    half _d2 = dot(_right, mainLight.direction);
	float offsetDir=getLightDirOffset(st, _d1, _d2);
    half3 lightMapOffset =offsetDir  * attenuatedLightColor*_SpeOffset2 ;
	lightMapOffset+=offsetDir*inputData.bakedGI*_SpeOffset2;
	
    #endif






    #ifdef _ADDITIONAL_LIGHTS
	int pixelLightCount = GetAdditionalLightsCount();
	for (int i = 0; i < pixelLightCount; ++i)
	{
		Light light = GetAdditionalLight(i, inputData.positionWS);
		half3 attenuation = (light.distanceAttenuation * light.shadowAttenuation);
		half3 attenuatedLightColor = light.color * attenuation;
		addLight +=attenuatedLightColor;
		half lightAngle=max(0.2,dot(inputData.viewDirectionWS,normalize(light.direction)));
		half3 tempLightOffset=lightAngle*attenuatedLightColor;
	    #ifdef _METAL_TEX
	    half _d1= dot(_left, light.direction);
	    half _d2 = dot(_right, light.direction);
	    lightMapOffset += getLightDirOffset(st, _d1, _d2)*attenuatedLightColor* _SpeOffset2;
	    #endif
	}
	//return prepareLitPixelForOutput(half4(addLight, diffuse.a), vertexColor);
	diffuseLighting+=addLight;


	#ifdef _SPE_DEBUG
	return half4(lightMapOffset,1);
	#endif
	
    #endif

    #ifdef _HOLY_LIGHT
    diffuseLighting *= holyOffset;
    #endif

	
    half3 hightLight = half3(0, 0, 0);
    #ifdef _METAL_TEX
    hightLight = lightMapOffset;
    #endif
    half _max = max(diffuseLighting.r, diffuseLighting.g);
    _max = max(diffuseLighting.b, _max);
    half _adjust = 1;
    half maxOffset = _otherLightMax;
    _max = max(1, _max);
    if (_max > maxOffset)
    {
        _adjust = maxOffset / _max;
    }
    diffuseLighting *= _adjust;
    half3 emmisionLight = half3(0, 0, 0);
    #ifdef _METAL_TEX
    emmisionLight = st.a * diffuse.rgb * _EmitOffset*_EmmisionColor;
	//diffuseLighting*=step(st.b,0.05);
    #endif

    half3 finalColor = (diffuseLighting) * (half3(1, 1, 1) + hightLight) * diffuse.rgb + emmisionLight;
    finalColor = _TintColor.rgb * _TintOffset + finalColor * (1 - _TintOffset);
	//BlackWhite
	half avg=(finalColor.x+finalColor.y+finalColor.z);
	half3 grayColor=avg*_GrayColorOffset;
	finalColor=lerp(finalColor,grayColor,_GrayOffset);
    return prepareLitPixelForOutput(half4(finalColor, diffuse.a), vertexColor);
}


float4 GetRawPos(float4 wpos)
{
    const float r1 = 20;
    const float rad1 = radians(r1);
    const float h = max(_GuideH, (wpos.y / sin(rad1)));
    wpos.y = h;
    wpos.xyz = TransformWorldToObject(wpos);
    wpos = calculateLocalPos(wpos);
    return wpos;
}


VertexOutputLWRP ForwardPassVertexSprite(VertexInput input)
{
    VertexOutputLWRP output = (VertexOutputLWRP)0;


    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);


    //input.vertex.y = max(input.vertex.y, 0.01);


    output.pos = calculateLocalPos(input.vertex);
    output.vertexColor = calculateVertexColor(input.color);
    output.texcoord = float3(calculateTextureCoord(input.texcoord), 0);

    float3 positionWS = TransformObjectToWorld(input.vertex.xyz);


	half _RoleHeight=1;
	half _RoleRoot=0;
	half yv = clamp(input.vertex.y - _RoleRoot, 0, _RoleHeight);
	output.holyOffset = (yv / _RoleHeight) * (_RootDarkLimit) + 1-_RootDarkLimit;


    float backFaceSign = 1;
    #if defined(FIXED_NORMALS_BACKFACE_RENDERING)
	backFaceSign = calculateBackfacingSign(positionWS.xyz);
    #endif
    output.viewDirectionWS = GetCameraPositionWS() - positionWS;
    #if defined(NEEDS_POSITION_WS)
	output.positionWS = float4(positionWS, 1);
    #endif

    #if defined(PER_PIXEL_LIGHTING)

    half3 normalWS = calculateSpriteWorldNormal(input, -backFaceSign);
    output.normalWorld.xyz = normalWS;

    #if defined(_NORMALMAP)
    output.tangentWorld.xyz = calculateWorldTangent(input.tangent);
    output.binormalWorld.xyz = calculateSpriteWorldBinormal(input, output.normalWorld.xyz, output.tangentWorld.xyz,
                                                        backFaceSign);
    #endif

    #else // !PER_PIXEL_LIGHTING
	half3 fixedNormal = half3(0, 0, -1);
	half3 normalWS = normalize(mul((float3x3)unity_ObjectToWorld, fixedNormal));

    #endif // !PER_PIXEL_LIGHTING

    #if defined(_HOLY_LIGHT)
    output.positionWS = float4(positionWS, 1);
    #endif

    OUTPUT_SH(normalWS.xyz, output.vertexSH);
    float4 tpos = GetRawPos(float4(positionWS, 1));
    output.pos.z = tpos.z / tpos.w * output.pos.w;
    //output.pos=tpos;
    return output;
}


half4 ForwardPassFragmentSprite(VertexOutputLWRP input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    fixed4 texureColor = calculateTexturePixel(input.texcoord.xy);
    ALPHA_CLIP(texureColor, input.vertexColor)

    // fill out InputData struct
    InputData inputData;
    inputData.shadowCoord = float4(0, 0, 0, 0);
    inputData.viewDirectionWS = normalize(input.viewDirectionWS);

    #if defined(PER_PIXEL_LIGHTING)
	half3 normalWS = input.normalWorld.xyz;
    #else // !PER_PIXEL_LIGHTING
	half3 fixedNormal = half3(0, 0, -1);
	half3 normalWS = normalize(mul((float3x3)unity_ObjectToWorld, fixedNormal));
    #endif // !PER_PIXEL_LIGHTING

    inputData.normalWS = normalWS;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
    #if defined(_ADDITIONAL_LIGHTS)
	inputData.positionWS = input.positionWS.rgb;
    #endif

    #if defined(_HOLY_LIGHT)
    inputData.positionWS = input.positionWS.rgb;
    #endif
    half4 st = getSpecTex(input.texcoord.xy);
    half4 pixel = MoonLightCal(inputData, texureColor, input.vertexColor, st,
                                                            input.texcoord.xy, input.holyOffset);

    //half4 emission = half4(0, 0, 0, 1);
    //half4 pixel = LightweightFragmentBlinnPhongSimplified(inputData, texureColor, emission, input.vertexColor);


    COLORISE(pixel)


    return pixel;
}

#endif
