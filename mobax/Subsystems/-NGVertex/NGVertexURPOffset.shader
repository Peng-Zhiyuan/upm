Shader "NGVertex/URPOffset"
{
    Properties
    {
        //        _TopColor("TopColor",Color) = (1,1,1,1)
        //        _BottomColor("BottomColor",Color) = (1,1,1,1)

        _MainTex("MainTex", 2D) = "white" {}
        _NoiseTex("NoiseTex", 2D) = "white" {}
        _MaskTex("MaskTex", 2D) = "white" {}
        _OffsetX("水平偏移",float) = 1
        _OffsetY("纵向偏移",float) = 1
        _OffsetZ("前后偏移",float) = 1

        _Speed("Speed",float) = 1
        _DepthBiasFactor("DepthBiasFactor",float) =1
        _DcurvatureRadius("DcurvatureRadius",float) = 30
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "Queue" = "AlphaTest+50" "IgnoreProjector" = "True"
        }
        LOD 100
        //Cull On
        Lighting Off
        ZWrite On
        ZTest On
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            // #pragma surface surf Standard keepalpha //modified

            // #include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                // float4 vertexColor : COLOR;
                float2 texcoord0 : TEXCOORD0;
            };

            struct v2f
            {
                // float4 vertexColor : COLOR;

                float4 vertex : SV_POSITION;
                float2 CloudUV01 : TEXCOORD0;
                float2 CloudUV02 : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float2 noisePos:TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                float fogCoord : TEXCOORD5;
                float2 uv0 : TEXCOORD6;
            };

            // float4 _TopColor;
            // float4 _BottomColor;
            sampler2D _NoiseTex;
            sampler2D _MainTex;
            sampler2D _MaskTex;
            uniform float4 _MaskTex_ST;
            uniform float4 _MainTex_ST;
            //float4 _NoiseTex_ST;
            float _OffsetX;
            float _OffsetY;
            float _OffsetZ;

            float _Speed;
            float _DepthBiasFactor;
            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            float _DcurvatureRadius;

            v2f vert(appdata v)
            {
                v2f o;
                //用两个方向获取噪波图的运动
                o.CloudUV01 = v.uv2 + _Time.x * _Speed;
                o.CloudUV02 = v.uv2 - _Time.x * _Speed;
                // o.CloudUV02 = v.uv - _Time.x * _Speed;
                o.noisePos.x = tex2Dlod(_NoiseTex, float4(o.CloudUV01, 0, 0)).r;
                o.noisePos.y = tex2Dlod(_NoiseTex, float4(o.CloudUV02, 0, 0)).r;

                o.uv0 = TRANSFORM_TEX(v.texcoord0, _MaskTex);
                float4 maskCol = tex2Dlod(_MaskTex, float4(o.uv0, 0, 0));

                // 让云的X轴扰动起来
                v.vertex.x += o.noisePos.x * o.noisePos.y * _OffsetX * step(.5, maskCol.r);
                // 让云的Y轴扰动起来
                v.vertex.y -= o.noisePos.x * o.noisePos.y * _OffsetY * step(.5, maskCol.r);
                v.vertex.z += o.noisePos.x * o.noisePos.y * _OffsetZ * step(.5, maskCol.r);
                // 以中心开始 将圆变成一个碗的形状，因为边缘太低，可能会露出一些不必要的场景。
                // o.vertex.y -= pow(distance(float2(0, 0), o.worldPos.xz) / _DcurvatureRadius, 0.01);
                o.worldPos = TransformObjectToWorld(v.vertex);
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.screenPos = ComputeScreenPos(o.vertex);

                o.fogCoord = ComputeFogFactor(o.vertex.z);
                // UNITY_TRANSFER_FOG(o, o.vertex);

                // o.vertexColor = v.vertexColor;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                //采样摄像机深度图
                float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.screenPos);
                float depth = LinearEyeDepth(depthSample, _ZBufferParams);
                float borderLine = saturate((depth - i.screenPos.w) / 2 - _DepthBiasFactor);
                //采样主图
                float4 mainTex = tex2D(_MainTex, i.uv0);
                //获取噪波
                // float noise = saturate(i.noisePos.x * i.noisePos.y);
                //利用噪波图来融合顶底两个颜色
                // float4 col = (mainTex * noise) + (mainTex * (1 - noise));
                //设置与物体交接的地方透明
                // col.a = borderLine;
                // apply fog
                half3 col3 = MixFog(mainTex.rgb, i.fogCoord);

                // UNITY_APPLY_FOG(i.fogCoord, col);
                return half4(mainTex.rgb, mainTex.a);
            }
            ENDHLSL
        }
    }
}