
Shader "NGSSEffect/SSBlackWhiteFlash"
{
    Properties
    {
        [HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
        _RempTex("Ramp (RGB)", 2D) = "white" {}
        //_Color("_Color (default = 1,1,1,1)", color) = (1,1,1,1)
        [Toggle(_Flash)]_Flash("_Flash", Float) = 1
        _Step("_Step", Range(0, 1)) = 0.5
        _CenterX("_CenterX", Range(0, 1)) = 0.5
        _CenterY("_CenterY", Range(0, 1)) = 0.5
        _SmoothRange("_SmoothRange", Range(0, 1)) = 0
        //[Toggle(ASE_ON)]_AseOn("_AseOn", Float) = 1
        _RaysThreshold("_RaysThreshold", Range(0, 1)) = 0.5
        _RaysDensity("_RaysDensity", Range(0, 10)) = 3
        _RaysFade("_RaysFade", Range(0, 10)) = 3
        _RaysFadeRange("_RaysFadeRange", Range(0, 10)) = 3
        _RaysStrength("_RaysStrength", Range(0, 10)) = 1
        _RaysContrast("_RaysContrast", Range(0.01, 1)) = 0.1
        [HideInInspector]_StencilRef("_StencilRef", Float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Comparison", Float) = 8
        
    }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_ST;
        float4 _RempTex_ST;
        //float4 _MainTex_TexelSize;
        float _Rotate;
        float _Flash;
        float _Step;
        float _SmoothRange;
        //half4 _Color;
        half _CenterX;
        half _CenterY;
        half _RaysThreshold;
        half _RaysDensity;
        half _RaysStrength;
        half _RaysFade;
        half _RaysFadeRange;
        half _RaysContrast;
        CBUFFER_END
        sampler2D _MainTex;
        sampler2D _RempTex;

	

    struct appdata {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
        //float3 ray : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };


    //vertex shader
    v2f vert(appdata v)
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
        o.uv = TRANSFORM_TEX(v.uv, _RempTex);//v.uv;

        return o;
    }

//直角坐标系 转 极坐标
	float2 Polar(float2 UV, float2 Center)
	{
		//0~1的1象限转-0.5~0.5的四象限
		float2 uv = UV-Center;

		//d为各个象限坐标到0点距离,数值为0~0.5
		float distance=length(uv);

		//0~0.5放大到0~1
		distance *=2;

		//4象限坐标求弧度范围是 [-pi,+pi]
		float angle=atan2(uv.x,uv.y);
				
		//把 [-pi,+pi]转换为0~1
		float angle01=angle/3.14159/2+0.5;

		//输出角度与距离
		return float2(angle01,distance);
	}

    float2 twirl(float2 UV , float2 Center, float Strength,float2 set)
    {
        float2 delta = UV - Center;
        float angle = Strength * length(delta);
        float x = cos(angle) * delta.x - sin(angle) * delta.y;
        float y = sin(angle) * delta.x + cos(angle) * delta.y;
        return float2(x + Center.x + set.x, y + Center.y + set.y);
    }

    float2 rotation(float2 UV,float2 Center,float Rotation){
        UV -= Center;
        float s = sin(Rotation);
        float c = cos(Rotation);
        float2x2 rMatrix = float2x2(c, -s, s, c);
        rMatrix *= 0.5;
        rMatrix += 0.5;
        rMatrix = rMatrix * 2 - 1;
        UV.xy = mul(UV.xy, rMatrix);

        UV += Center;
        return UV;
    }

    float3 palette( in float t, in float3 a, in float3 b, in float3 c, in float3 d )
    {
        return a + b*cos( 6.28318*(c*t+d) );
    }
    //fragment shader
    float4 frag(v2f i) : SV_Target
    {
        
        half2 uv = i.positionCS.xy/_ScreenParams.xy;
        half2 center= half2(_CenterX,_CenterY);
        half2 polar_uv = Polar(i.uv,center) * _RaysDensity;
        half4 rampCol = tex2D(_RempTex, polar_uv);
        half strength  = smoothstep(_RaysFade, _RaysFade + _RaysFadeRange, polar_uv.y);
        uv = uv - normalize(center - uv) * rampCol.b * (_RaysStrength * 0.01 + strength ) ;

        half3 mainCol = tex2D(_MainTex, uv); //SampleSceneColor(uv);

        //float2 polar_uv = Polar(i.uv) * _RaysDensity;
        //float4 rampCol = tex2D(_RempTex, polar_uv);
        half rampColVal= strength * step(_RaysThreshold,rampCol.r);
        half luminance = (mainCol.r * 0.29 + mainCol.g * 0.59 + mainCol.b * 0.12);
        //luminance = step(_Step,luminance);
        half halfSmoothRange = _SmoothRange * 0.5;
        luminance = smoothstep(_Step - halfSmoothRange, _Step + halfSmoothRange, luminance);
        //luminance += luminance * step(0,rempCol.r);
        luminance += 100* _RaysContrast * rampColVal;
        luminance = lerp(1 - luminance,luminance, step(_Flash, 0));
        return luminance;
    }



    ENDHLSL

    SubShader
    {

        //Tags {"RenderType" = "Opaque"  "RenderPipeline" = "UniversalPipeline"}
        Tags{ "RenderPipeline" = "UniversalPipeline"  "RenderType" = "Overlay" "Queue" = "Transparent-499" "DisableBatching" = "True" }
            LOD 100
            ZTest Always 
            Cull Off 
            ZWrite Off
            Blend one zero
            Pass
            {
             Name "SSBlackWhiteFlash"
/*
             Stencil
             {
                Ref[_StencilRef]
                Comp[_StencilComp]
                Pass Replace
             }
*/
            
             HLSLPROGRAM
             //#pragma multi_compile _ _DIST_TYPE_VIEWSPACE
             //#pragma multi_compile _ _FUNC_TYPE_LINEAR _FUNC_TYPE_EXP
             //#pragma multi_compile _ ASE_ON
             #pragma vertex vert
             #pragma fragment frag
             ENDHLSL
        }

    }

}