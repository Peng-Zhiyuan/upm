#ifndef UNIVERSAL_OUTLINEFUNC_INCLUDED
#define UNIVERSAL_OUTLINEFUNC_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// Fills a light struct given a perObjectLightIndex
CBUFFER_START(UnityPerMaterial)

float4 _OutLineColor;
half _OutlineFactor;

CBUFFER_END

	    struct a2v
		{
			float4 positionOS : POSITION;
			float3 normalOS : NORMAL;
			// float4 vertexColorOS : COLOR;
			// float4 texcoord0 : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
			// DECLARE_LIGHTMAP_OR_SH(half2(0,0), vertexSH, 1);
			// float4 color : COLOR;
			// float4 vertexColor : COLOR1;
			// float2 uv0 : TEXCOORD3;

		};

	    v2f vert(a2v v)
		{
			
			v2f o = (v2f)0;
			v.positionOS.xyz +=     v.normalOS.xyz * _OutlineFactor * 0.001;
			VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
			o.vertex = vertexInput.positionCS;
			// o.vertexColor =  v.vertexColorOS;
			// OUTPUT_SH(half3(1,1,1), o.vertexSH);
			return o;
		}

		half4 frag(v2f i) : SV_Target
		{
				// half4 maskTex = SAMPLE_TEXTURE2D(_MaskTex,sampler_MaskTex,i.uv0); 
				// half3 finalcolor = (maskTex.x*_Color1+maskTex.y*_Color2+maskTex.z*_Color3+maskTex.a*_Color4);
				// //变暗
                // // half3 mixOutColor = half3 (max(finalcolor.r,_OutLineColor.r),max(finalcolor.g,_OutLineColor.g),max(finalcolor.b,_OutLineColor.b));
				// //正片叠底
				// half3 mixOutColor = (finalcolor + (1-maskTex.r-maskTex.g-maskTex.b-maskTex.a) ) * _OutLineColor.rgb;

				// finalcolor = lerp (_OutLineColor.rgb,mixOutColor,_OutLineColor.a);
				
				// half3 ambient = lerp(1,saturate(i.vertexSH.rgb), _Ambient);
				// return  half4(finalcolor.rgb,1);
				return  half4(_OutLineColor.rgb,1);
		}

#endif