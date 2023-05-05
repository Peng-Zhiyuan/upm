Shader "Hidden/ALINE/Outline" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,0.5)
		_FadeColor ("Fade Color", Color) = (1,1,1,0.3)
		_PixelWidth ("Width (px)", Float) = 4
		_LengthPadding ("Length Padding (px)", Float) = 0
	}
	SubShader {
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Offset -3, -50
		Tags { "IgnoreProjector"="True" "RenderType"="Overlay" }
		// With line joins some triangles can actually end up backwards, so disable culling
		Cull Off
		
		// Render behind objects
		Pass {
			ZTest Greater
			
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature UNITY_HDRP
			#include "aline_common.cginc"

			float4 _Color;
			float4 _FadeColor;
			float _PixelWidth;
			float _LengthPadding;

			static const float FalloffTextureScreenPixels = 2;
			
			line_v2f vert (appdata_color v, out float4 outpos : SV_POSITION) {
				float pixelWidth = _PixelWidth * length(v.normal);
				line_v2f o = line_vert(v, pixelWidth, _LengthPadding, outpos);
				o.col = v.color * _Color * _FadeColor;
				o.col.rgb = ConvertSRGBToDestinationColorSpace(o.col.rgb);
				return o;
			}

			half4 frag (line_v2f i, float4 screenPos : VPOS) : COLOR {
				float a = calculateLineAlpha(i, i.lineWidth, FalloffTextureScreenPixels);
				return i.col * float4(1,1,1,a);
			}
			ENDHLSL
		}

		// First pass writes to the Z buffer
		// where the lines have a pretty high opacity
		Pass {
			ZTest LEqual
			ZWrite On
			ColorMask 0
			
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature UNITY_HDRP
			#include "aline_common.cginc"
			
			float _PixelWidth;
			float _LengthPadding;
			
			// Number of screen pixels that the _Falloff texture corresponds to
			static const float FalloffTextureScreenPixels = 2;
			
			line_v2f vert (appdata_color v, out float4 outpos : SV_POSITION) {
				float pixelWidth = _PixelWidth * length(v.normal);
				line_v2f o = line_vert(v, pixelWidth, _LengthPadding, outpos);
				o.col = half4(1,1,1,1);
				return o;
			}

			half4 frag (line_v2f i, float4 screenPos : VPOS) : COLOR {
				float a = calculateLineAlpha(i, i.lineWidth, FalloffTextureScreenPixels);
				if (a < 0.7) discard;
				return float4(1,1,1,a);
			}
			ENDHLSL
		}
		
		// Render in front of objects
		Pass {
			ZTest LEqual
			
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature UNITY_HDRP
			#include "aline_common.cginc"

			float4 _Color;
			float _PixelWidth;
			float _LengthPadding;
			
			// Number of screen pixels that the _Falloff texture corresponds to
			static const float FalloffTextureScreenPixels = 2;
			
			line_v2f vert (appdata_color v, out float4 outpos : SV_POSITION) {
				float pixelWidth = _PixelWidth * length(v.normal);
				line_v2f o = line_vert(v, pixelWidth, _LengthPadding, outpos);
				o.col = v.color * _Color;
				o.col.rgb = ConvertSRGBToDestinationColorSpace(o.col.rgb);
				return o;
			}

			half4 frag (line_v2f i, float4 screenPos : VPOS) : COLOR {
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				float a = calculateLineAlpha(i, i.lineWidth, FalloffTextureScreenPixels);
				return i.col * float4(1,1,1,a);
			}
			ENDHLSL
		}
	}
	Fallback Off
}
