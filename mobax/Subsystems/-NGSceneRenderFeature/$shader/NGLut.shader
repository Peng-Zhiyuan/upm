// Amplify Color - Advanced Color Grading for Unity Pro
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "NGSSEffect/NGLUT" {
	Properties {
		[HideInInspector]_MainTex ("Base (RGB)", 2D) = "" {}
		_LUT ("LUT (RGB)", 2D) = "" {}
		_Strength("Strength", Range(0, 1)) = 1
	}

	CGINCLUDE
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _LUT;
		half _Strength;

		float3 apply_lut( float3 color )
		{
			const float4 coord_scale = float4( 0.0302734375, 0.96875, 31.0, 0.0 );
			const float4 coord_offset = float4( 0.00048828125, 0.015625, 0.0, 0.0 );
			const float2 texel_height_X0 = float2( 0.03125, 0.0 );

			float3 coord = color * coord_scale + coord_offset;

			float3 coord_frac = frac( coord );
			float3 coord_floor = coord - coord_frac;

			float2 coord_bot = coord.xy + coord_floor.zz * texel_height_X0;
			float2 coord_top = coord_bot + texel_height_X0;

			float3 lutcol_bot = tex2D( _LUT, coord_bot ).rgb;
			float3 lutcol_top = tex2D( _LUT, coord_top ).rgb;

			return lerp(color, lerp( lutcol_bot, lutcol_top, coord_frac.z) , _Strength);
		}
	ENDCG
	Subshader {
		ZTest Always Cull Off ZWrite Off Blend Off Fog { Mode off }
	
		Pass {
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag

				fixed4 frag( v2f_img i ) : SV_Target
				{
					//float3 color = LinearToGammaSpace( tex2D( _MainTex, i.uv ).rgb );
					float3 color = tex2D( _MainTex, i.uv ).rgb;
					return fixed4( apply_lut( color ), 1 );
				}
			ENDCG
		}
	}
	Fallback Off
}
