#ifndef PARALLAX_MAP_INCLUDED
#define PARALLAX_MAP_INCLUDED

#ifdef HEIGHT_MAP
/*
sampler2D _HeightMap;
half _HeightScale;
*/
inline half2 ParallaxOffset(half2 uv, half3 viewDir)
{
    //h为视差贴图采样的结果*高度后-高度/2
    //这是Unity官方提供的一种性能高的视差贴图算法，对于视差偏移度小的效果还不错
    //由于是在平面上移动UV点，对于视差偏移度过大的效果并不好
    half h = tex2D(_HeightMap, uv).r;
    h = h * _HeightScale - _HeightScale/2.0;
    half3 v = normalize(viewDir);
    v.z += 0.42;
    return h * (v.xy/v.z);
}



half2 ParallaxMap(half2 texcoord, half3 viewDir, half3 normalDir)
{
    /*
    half height =  tex2D(_HeightMap, texcoord).r;

    //视点方向越贴近法线，UV 偏移越小
    half2 p = viewDir.xy / viewDir.z * (height * _HeightScale);

    //返回偏移后的 UV 值
    return texcoord + p;    
    */

    const half minLayers = 1;
    const half maxLayers = 1;

    //number of depth layers
    half numLayers = lerp(maxLayers, minLayers, abs(dot(half3(0.0, 0.0, 1.0), viewDir)));
    //half numLayers = lerp(maxLayers, minLayers, abs(dot(normalDir, viewDir)));
    //calculate the size of each layer
    half layerDepth = 1.0f / numLayers;
    //depth or current layer
    half currentLayerDepth = 0.0f;
    //the amount to shift the texture coordinates per layer(from vector p)
    half2 P = viewDir.xy * _HeightScale;
    half2 deltaTexCoords = P / numLayers;
 
    //get initial values
    half2 currentTexCoords = texcoord;
    half currentDepthMapValue = tex2D(_HeightMap, currentTexCoords).r;
    [unroll(10)]
    while(currentLayerDepth < currentDepthMapValue)
    {
        //shift texture coordinates along direction of P
        currentTexCoords -= deltaTexCoords;
        //get depthmap value at current texture coordinates
        currentDepthMapValue = tex2D(_HeightMap, currentTexCoords).r;
        //get depth of next layer
        currentLayerDepth += layerDepth;
    }

    //get texture coordinates before collision(reverse operation)
    half2 prevTexCoords = currentTexCoords + deltaTexCoords;

    //get depth after and before collision for linear interpolation
    half afterDeltaDepth = currentDepthMapValue - currentLayerDepth;
    half beforDeltaDepth = tex2D(_HeightMap, prevTexCoords).r - (currentLayerDepth - layerDepth);

    //interpolation of texture coordinates
    half weight = afterDeltaDepth / (afterDeltaDepth - beforDeltaDepth);
    half2 finalTexCoords = prevTexCoords * weight + currentTexCoords * (1 - weight);

    return finalTexCoords;
    
}

half2 ReliefMapping(half2 uv, half3 viewDir)
{
        half h = tex2D(_HeightMap, uv).r;
        half3 v = normalize(viewDir);
        v.z = abs(v.z);
        half3 startPoint = half3(uv,0);
        v.xy *= _HeightScale;
        int linearStep = 40;
        int binarySearch = 8;
        //half3 offset = (v/v.z)/linearStep;
        half3 offset = (v/v.z)/linearStep;
        for (int index=0;index<linearStep;index++){
            half depth = 1 - h;
            if (startPoint.z < depth){
                startPoint += offset; 
            }
        }
        half3 biOffset = offset;
        for (int index=0;index<binarySearch;index++){
            biOffset = biOffset / 2;
            half depth = 1 - h;
            if (startPoint.z < depth){
                startPoint += biOffset;
            }else{
                startPoint -= biOffset;
            }
        }
        half2 res = startPoint.xy;
        return res;
    }
#endif
#endif