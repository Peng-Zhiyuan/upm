Shader "NGShader/Bloom"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Bloom("Bloom Data", Vector) = (0.5,0,0)
	}
	SubShader
{
	Pass { // 0
		ZTest Always Cull Off ZWrite Off
		Fog { Mode Off }

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment fragBeautifyFast
		#pragma target 3.0
		#pragma fragmentoption ARB_precision_hint_fastest 

		#include "UnityCG.cginc"

		UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
		UNITY_DECLARE_SCREENSPACE_TEXTURE(_BloomTex);
		half4     _MainTex_TexelSize;
		half4     _MainTex_ST;
		uniform float3 _Bloom;

		struct appdata {
			float4 vertex : POSITION;
			half2 texcoord : TEXCOORD0;
		};

		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv: TEXCOORD0;
			float2 depthUV : TEXCOORD1;
			float2 uvN: TEXCOORD2;
			float2 uvS: TEXCOORD3;
			float2 uvW: TEXCOORD4;
		};

		inline float getLuma(float3 rgb) {
			const float3 lum = float3(0.299, 0.587, 0.114);
			return dot(rgb, lum);
		}

		v2f vert(appdata v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);

			#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0) {
				// Depth texture is inverted WRT the main texture
				o.uv.y = 1.0 - o.uv.y;
			}
			#endif    	
			return o;
		}


		float4 fragBeautifyFast(v2f i) : SV_Target{
			float4 pixel = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);
#if UNITY_COLORSPACE_GAMMA
			pixel.rgb = GammaToLinearSpace(pixel.rgb);
#endif
			pixel.rgb += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_BloomTex, i.uv).rgb * _Bloom.xxx;
#if UNITY_COLORSPACE_GAMMA
			pixel.rgb = LinearToGammaSpace(pixel.rgb);
#endif
			return pixel;
		}

		ENDCG
	}

	Pass { // 1
		ZTest Always Cull Off ZWrite Off
		Fog { Mode Off }

		CGPROGRAM
		#pragma vertex vertLum
		#pragma fragment fragLum
		#pragma fragmentoption ARB_precision_hint_fastest

		#include "UnityCG.cginc"

		UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
		half4     _MainTex_TexelSize;
		half4     _MainTex_ST;

		half4 	  _Bloom;

		struct appdata {
			float4 vertex : POSITION;
			half2 texcoord : TEXCOORD0;
		};

		struct v2fLum {
			float4 pos : SV_POSITION;
			half2 uv: TEXCOORD0;
		};

		inline half Brightness(half3 c) {
			return max(c.r, max(c.g, c.b));
		}

		v2fLum vertLum(appdata v) {
			v2fLum o;
			o.pos = UnityObjectToClipPos(v.vertex);

			o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);

			#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0) {
					// Depth texture is inverted WRT the main texture
					o.uv.y = 1.0 - o.uv.y;
				}
			#endif
			return o;
		}

		half4 fragLum(v2fLum i) : SV_Target{

			half4 c = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);

#if UNITY_COLORSPACE_GAMMA
			c.rgb = GammaToLinearSpace(c.rgb);
#endif
			//fixed br = Brightness(c.rgb);
			//c.rgb = lerp(c.rgb * clamp(br - _Bloom.w, 0, 1), max(c.rgb - _Bloom.www, 0), saturate(br - 1));
			c.rgb = max((c.rgb - _Bloom.www), 0);

			//c.rgb = half3(alpha, alpha, alpha);

			//	c *= alpha;

				return c;
			}

			ENDCG
		}

		Pass{ // 2
			ZTest Always Cull Off ZWrite Off
			Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vertBlur
			#pragma fragment fragBlur
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
			half4     _MainTex_TexelSize;
			half4     _MainTex_ST;

			half      _BlurScale;

			struct appdata {
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
			};

			struct v2fCross {
				float4 pos : SV_POSITION;
				half2 uv: TEXCOORD0;
			};

			v2fCross vertBlur(appdata v) {
				v2fCross o;
				o.pos = UnityObjectToClipPos(v.vertex);
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0) {
					// Texture is inverted WRT the main texture
					v.texcoord.y = 1.0 - v.texcoord.y;
				}
				#endif   
				o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
				return o;
			}

			half4 fragBlur(v2fCross i) : SV_Target{
				half offsetX2 = _MainTex_TexelSize.x * 3.2307692308 * _BlurScale;
				half offsetX = _MainTex_TexelSize.x * 1.3846153846 * _BlurScale;
				half offsetY2 = _MainTex_TexelSize.y * 3.2307692308 * _BlurScale;
				half offsetY = _MainTex_TexelSize.y * 1.3846153846 * _BlurScale;
				half2 uvOffset[25] = {
					half2(-offsetX2, offsetY2), half2(-offsetX, offsetY2), half2(0, offsetY2), half2(offsetX, offsetY2), half2(offsetX2, offsetY2),
						half2(-offsetX2, offsetY), half2(-offsetX, offsetY), half2(0, offsetY), half2(offsetX, offsetY), half2(offsetX2, offsetY),
						half2(-offsetX2, 0), half2(-offsetX, 0), half2(0, 0), half2(offsetX, 0), half2(offsetX2, 0),
						half2(-offsetX2, -offsetY), half2(-offsetX, -offsetY), half2(0, -offsetY), half2(offsetX, -offsetY), half2(offsetX2, -offsetY),
						half2(-offsetX2, -offsetY2), half2(-offsetX, -offsetY2), half2(0, -offsetY2), half2(offsetX, -offsetY2), half2(offsetX2, -offsetY2),
				};
				half4 s[25];
				for (int k = 0; k < 25; k++)
				{
					s[k] = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv + uvOffset[k]);
				}
				half4 pixel = (
					(1.0 * (s[0] + s[4] + s[20] + s[24])) +
					(4.0 * (s[1] + s[3] + s[5] + s[9] + s[15] + s[19] + s[21] + s[23])) +
					(7.0 * (s[2] + s[10] + s[14] + s[22])) +
					(16.0 * (s[6] + s[8] + s[16] + s[18])) +
					(26.0 * (s[7] + s[11] + s[13] + s[17])) +
					(41.0 * s[12])
					) / 273.0;
				return pixel;
			}

			ENDCG
		}

		Pass{ // 3 Bloom premul compose
			ZTest Always Cull Off ZWrite Off
			Fog { Mode Off }
			Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragCopy
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 3.0

			#include "UnityCG.cginc"

			UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
			half4     _MainTex_TexelSize;
			half4     _MainTex_ST;

			struct appdata {
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				half2 uv: TEXCOORD0;
			};

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);

				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0) {
					// Depth texture is inverted WRT the main texture
					o.uv.y = 1.0 - o.uv.y;
				}
				#endif    	
				return o;
			}

			half4 fragCopy(v2f i) : SV_Target{
				return UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);
			}
			ENDCG
		}
	

	}
	Fallback Off
}