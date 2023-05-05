Shader "NGEnv/EnvBlur"
{
	Properties 
	{
		_MainTex ("ScreenTexture", 2D) = "" {}
	}
	HLSLINCLUDE
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	struct a2v 
	{
		float4 vertex:POSITION;
		float2 texcoord:TEXCOORD0;
	};
	struct v2f 
	{
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
		float4 uv01 : TEXCOORD1;
		float4 uv23 : TEXCOORD2;
		float4 uv45 : TEXCOORD3;
	};

	float2 _offset;
	sampler2D _MainTex;
	//顶点着色器
	//传入的offset的值为（intensity,0,）和（0,intensity）。
	//（intensity,0,）和（0,intensity）分别为横向和纵向。
	v2f vert (a2v v) {
		v2f o;
		o.pos = TransformObjectToHClip(v.vertex);
		o.uv.xy = v.texcoord.xy;
		//存6个向外扩散的坐标
		o.uv01 =  v.texcoord.xyxy + _offset.xyxy * float4(1,1, -1,-1);
		o.uv23 =  v.texcoord.xyxy + _offset.xyxy * float4(1,1, -1,-1) * 2.0;
		o.uv45 =  v.texcoord.xyxy + _offset.xyxy * float4(1,1, -1,-1) * 3.0;
		return o;
	}
	//片元着色器
	half4 frag (v2f i) : COLOR {
		//高斯模糊，也叫正态分布。重点就是权重分配，越靠近像素中心，权重越高。这里的权重，写的大概值。
		half4 color = float4 (0,0,0,0);
		color += 0.40 * tex2D (_MainTex, i.uv);
		color += 0.15 * tex2D (_MainTex, i.uv01.xy);
		color += 0.15 * tex2D (_MainTex, i.uv01.zw);
		color += 0.10 * tex2D (_MainTex, i.uv23.xy);
		color += 0.10 * tex2D (_MainTex, i.uv23.zw);
		color += 0.05 * tex2D (_MainTex, i.uv45.xy);
		color += 0.05 * tex2D (_MainTex, i.uv45.zw);
		//输出屏幕
		return color;
	}
	ENDHLSL
	//执行pass
	Subshader 
	{
		Pass 
 		{
    		HLSLPROGRAM
    		#pragma vertex vert
    		#pragma fragment frag
    		ENDHLSL
		}
	}
}
