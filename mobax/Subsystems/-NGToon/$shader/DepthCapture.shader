// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ShadowMap/DepthCapture" {

    SubShader{
        Tags { "RenderType" = "Opaque" }
        LOD 200

	  Pass {
		  CGPROGRAM
		  #pragma vertex vert
		  #pragma fragment frag
		  #include "UnityCG.cginc"


		  struct v2f {
			   half4 position : SV_POSITION;
			   half depth : TEXCOORD0;
		  };


		 v2f vert(appdata_base v)
		  {
			   v2f o;
			   o.position = UnityObjectToClipPos(v.vertex);
			   o.depth = COMPUTE_DEPTH_01;
			   return o;
		  }

		  half4 frag(v2f i) : COLOR
		  {
			   return (EncodeFloatRGBA(i.depth));
		  }


	  ENDCG
	  }

    }
}
