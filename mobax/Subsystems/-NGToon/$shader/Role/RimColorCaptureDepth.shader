Shader "NGRole/RimColor/CaptureDepth"
{
	Properties
	{
	}
	SubShader
	{
		Pass
		{
			Name "CaptureDepth"
			Tags { "LightMode" = "CaptureDepth" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"


			struct v2f {
				float4 position : SV_POSITION;
				fixed depth : TEXCOORD0;
			};


			v2f vert(appdata_base v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.depth = COMPUTE_DEPTH_01;
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				return float4(1,1,1,1);
			}
			ENDCG
		}
	}
	Fallback Off
}