Shader "NGRole/NGLiquidVolume"
{
    Properties
    {
        [HideInInspector] _Color1("Color 1", Color) = (1,0,0,1)
        [HideInInspector]_FoamColor("Foam Color", Color) = (1,1,1,0.9)
        [HideInInspector]_Color2("Color 2", Color) = (1,0,0,1)
        [HideInInspector]_Muddy("Muddy", Range(0,1)) = 1.0
        [HideInInspector]_Turbulence("Turbulence", Vector) = (1.0,1.0,1.0,0)
        [HideInInspector]_TurbulenceSpeed("Turbulence Speed", Float) = 1
        [HideInInspector]_SparklingIntensity("Sparkling Intensity", Range(0,1)) = 1.0
        [HideInInspector]_SparklingThreshold("Sparkling Threshold", Range(0,1)) = 0.85
        [HideInInspector]_DeepAtten("Deep Atten", Range(0,10)) = 2.0
        [HideInInspector]_LiquidRaySteps("Liquid Ray Steps", Int) = 2
        [HideInInspector]_SmokeColor("Smoke Color", Color) = (0.7,0.7,0.7,0.1)
        [HideInInspector]_SmokeAtten("Smoke Atten", Range(0,10)) = 2.0
        [HideInInspector]_SmokeRaySteps("Smoke Ray Steps", Int) = 10
        [HideInInspector]_SmokeSpeed("Smoke Speed", Range(0,20)) = 5.0
        _NoiseTex2D("Noise Tex 2D", 2D) = "white"
        [HideInInspector]_FoamRaySteps("Foam Ray Steps", Int) = 4
        [HideInInspector]_FoamWeight("Foam Weight", Float) = 10.0
        [HideInInspector]_FoamBottom("Foam Visible From Bottom", Float) = 1.0
        [HideInInspector]_FoamTurbulence("Foam Turbulence", Float) = 1.0
        [HideInInspector]_Scale("Scale", Vector) = (0.25, 0.2, 1, 5.0)

        [HideInInspector]_CullMode("Cull Mode", Int) = 2
        [HideInInspector]_ZTestMode("ZTest Mode", Int) = 4

        [HideInInspector]_FoamDensity("Foam Density", Float) = 1
        [HideInInspector]_FoamMaxPos("Foam Max Pos", Float) = 0
        [HideInInspector]_LevelPos("Level Pos", Float) = 0
        [HideInInspector]_UpperLimit("Upper Limit", Float) = 0
        [HideInInspector]_NoiseTex("Noise Tex", 3D) = "white"
        [HideInInspector]_Center("Center", Vector) = (1,1,1)
        [HideInInspector]_Size("Size", Vector) = (1,1,1,0.5)
        [HideInInspector]_DoubleSidedBias("Double Sided Bias", Float) = 0
        [HideInInspector]_ShowFoam("Show Foam", Float) = 1
        [HideInInspector]_ShowSmoke("Show Smoke", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "AlphaTest+50"  "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "RoleLiquid" }
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha
            Cull[_CullMode]
            ZTest[_ZTestMode]
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../Frame/CustomLightFunc.hlsl"

            struct appdata
            {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
                half3 normal : NORMAL;
            };

            struct v2f
            {
                half4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
                half3 posWorld : TEXCOORD1;
                half4 vertex : TEXCOORD2;
            };

            sampler2D _NoiseTex2D;
            sampler3D _NoiseTex;
            float _SparklingIntensity, _SparklingThreshold;

            half4 _Color1;
            half4 _Color2;
            half4 _FoamColor;
            half _FoamMaxPos;
            int _FoamRaySteps;
            half _FoamDensity;
            half _FoamBottom;
            half _FoamTurbulence;
            half _LevelPos;
          //  half3 _FlaskThickness;
            half4 _Size;
            half3 _Center;
            half _Muddy;
            half4 _Turbulence;
            half _DeepAtten;
            half4 _SmokeColor;
            half _SmokeAtten;
            int _SmokeRaySteps;
            half _SmokeSpeed;
            int _LiquidRaySteps;
           // half3 _GlossinessInt;
            half _FoamWeight;
            half4 _Scale;
            half4x4 _Rot;
            half _UpperLimit;
          //  half3 wsCameraPos;
            half _TurbulenceSpeed;
            half _DoubleSidedBias;
            half _ShowFoam;
            half _ShowSmoke;

            void intCylinder(half3 rd, out half t0, out half t1)
            {
                rd = mul((float3x3)_Rot, rd);
                half3 d = _WorldSpaceCameraPos - _Center;
                d = mul((float3x3)_Rot, d);
                half a = dot(rd.xz, rd.xz);
                half b = dot(rd.xz, d.xz);
                half c = dot(d.xz, d.xz) - _Size.w * _Size.w;
                half t = sqrt(max(b * b - a * c, 0));
                t0 = (-b - t) / a;
                t1 = (-b + t) / a;

                half sy = _Size.y * 0.5;
                half h = abs(d.y) - sy;
                if (h > 0)
                {
                    half rdl = dot(rd.xz / rd.y, rd.xz / rd.y);
                    half tc = h * sqrt(1.0 + rdl);
                    t0 = max(t0, tc);
                }
                h = sign(rd.y) * -d.y + sy;
                if (h > 0)
                {
                    half rdl = dot(rd.xz / rd.y, rd.xz / rd.y);
                    half tc = h * sqrt(1.0 + rdl);
                    t1 = min(t1, tc);
                }
            }

            half4 raymarch(half4 vertex, half3 rd, half t0, half t1)
            {
                half3 wpos = _WorldSpaceCameraPos + rd * t0;

                float turbulence = (tex2D(_NoiseTex2D, vertex.xz).g - 0.5) * _Turbulence.x;
                turbulence += sin(vertex.w) * _Turbulence.y;
                turbulence *= 0.05 * _Size.y * _FoamTurbulence;
                _LevelPos += turbulence;
                _FoamMaxPos += turbulence;

                // we can get rid of 2 length calls simplifying expressions:
                float2 rdy = rd.xz / rd.y;
                float delta = sqrt(1.0 + dot(rdy, rdy));
                float h = abs(wpos.y - _LevelPos);
                float t2 = t0 + h * delta; // length(delta * h.xx);;

                // compute foam level (t3)
                float hf = abs(wpos.y - _FoamMaxPos);
                float t3 = t0 + hf * delta;

                // ray-march smoke
                float tmin, tmax;
                float sy = sign(rd.y);
                half4 sumFoam = half4(0, 0, 0, 0);
                half4 sumSmoke = half4(0, 0, 0, 0);
                if (_ShowFoam > 0)
                {
                    if (_ShowSmoke > 0)
                    {                        
                        if (wpos.y > _LevelPos) {
                            tmin = t0;
                            tmax = rd.y < 0 ? min(t2, t1) : t1;
                            float stepSize = (tmax - tmin) / (float)_SmokeRaySteps;
                            float4 dir = float4(rd * stepSize, 0);
                            float4 rpos = float4(_WorldSpaceCameraPos + rd * tmin, 0);
                            float4 disp = float4(0, _Time.x * _Turbulence.x * _Size.y * _SmokeSpeed, 0, 0);
                            for (int k = _SmokeRaySteps; k > 0; k--, rpos += dir) {
                                half n = tex3Dlod(_NoiseTex, (rpos - disp) * _Scale.x).r;
                                half4 lc = half4(_SmokeColor.rgb, n * _SmokeColor.a);
                                lc.rgb *= lc.aaa;
                                half deep = exp(((_LevelPos - rpos.y) / _Size.y) * _SmokeAtten);
                                lc *= deep;
                                sumSmoke += lc * (1.0 - sumSmoke.a);
                            }
                        }
                    }    

                    // ray-march foam
                    tmax = min(t3, t1), tmin = t0;
                    if (wpos.y > _FoamMaxPos) {
                        tmin = tmax;
                        tmax = min(t2, t1) * -sy;
                    }
                    else if (wpos.y < _LevelPos) {
                        tmin = min(t2, t1);
                        tmax *= _FoamBottom * sy;
                    }
                    else if (rd.y < 0) {
                        tmax = min(t2, t1);
                    }

                    if (tmax > tmin) {
                        float stepSize = (tmax - tmin) / (float)_FoamRaySteps;
                        float rayStepsFoamColorModify = 1 / (float)_FoamRaySteps;
                        float4 dir = float4(rd * stepSize, 0);
                        float4 rpos = float4(_WorldSpaceCameraPos + rd * tmin, 0);
                        rpos.y -= _LevelPos;
                        float foamThickness = _FoamMaxPos - _LevelPos;
                        float4 disp = float4(_Time.x, 0, _Time.x, 0) * _Turbulence.x * _Size.w * _FoamTurbulence;
                        for (int k = _FoamRaySteps; k > 0; k--, rpos += dir) {
                            //					float h = saturate( (rpos.y - _LevelPos) / foamThickness );
                            float h = saturate(rpos.y / foamThickness);
                            float n = saturate(tex3Dlod(_NoiseTex, (rpos - disp) * _Scale.y).r + _FoamDensity);
                            if (n > h) {
                                half4 lc = half4(_FoamColor.rgb, n - h);
                                lc.a *= _FoamColor.a * rayStepsFoamColorModify;
                                lc.rgb *= lc.aaa;
                                //						float deep = saturate((rpos.y-_LevelPos) * _FoamWeight / foamThickness);
                                half deep = saturate(rpos.y * _FoamWeight / foamThickness);
                                lc *= deep;
                                sumFoam += lc * (1.0 - sumFoam.a);
                            }
                        }
                        sumFoam *= 1.0 + _FoamDensity;
                    }
                }

                // ray-march liquid
                if (wpos.y > _LevelPos) {
                    tmin = t2;
                    tmax = t1 * -sy;
                }
                else {
                    tmin = t0;
                    tmax = min(t2, t1);
                }
                half4 sum = half4(0, 0, 0, 0);
                if (tmax > tmin) {
                    float stepSize = (tmax - tmin) / (float)_LiquidRaySteps;
                    float rayStepsColorModify = 1 / (float)_LiquidRaySteps;
                    float4 dir = float4(rd * stepSize, 0);
                    float4 rpos = float4(_WorldSpaceCameraPos + rd * tmin, 0);	// does not matter to move to level pos
                    rpos.y -= _LevelPos;
                    float4 disp = float4(_Time.x * _Turbulence.y, _Time.x * 1.5, _Time.x * _Turbulence.y, 0) * (_Turbulence.y + _Turbulence.x) * _Size.y;
                    float4 disp2 = float4(0, _Time.x * 2.5 * (_Turbulence.y + _Turbulence.x) * _Size.y, 0, 0);
                    for (int k = _LiquidRaySteps; k > 0; k--, rpos += dir) {
                        //					fixed deep = exp(((rpos.y - _LevelPos)/_Size.y) * _DeepAtten);
                        half deep = saturate(exp((rpos.y / _Size.y) * _DeepAtten));
                        half n = tex3Dlod(_NoiseTex, (rpos - disp) * _Scale.z).r;
                        half4 c1 = lerp(_Color2, _Color1, deep);
                       // half4 c2 = lerp(_Color4, _Color2, deep);
                        half4 lc = half4(c1.rgb, (1.0 - _Muddy) + n * _Muddy);
                        lc.a *= c1.a * rayStepsColorModify;
                        lc.rgb *= lc.aaa;
                      //  lc.rgb *= deep;
                        sum += lc * (1.0 - sum.a);

                        n = tex3Dlod(_NoiseTex, (rpos - disp2) * _Scale.w).r;
                        lc = half4(c1.rgb + max(n - _SparklingThreshold, 0) * _SparklingIntensity, (1.0 - _Muddy) + n * _Muddy);
                        lc.a *= c1.a * rayStepsColorModify;
                        lc.rgb *= lc.aaa;
                      //  lc.rgb *= deep;
                        sum += lc * (1.0 - sum.a);
                    }
                }

                // Final blend
                if (wpos.y > _LevelPos) {
                    if (_ShowSmoke > 0 && _ShowFoam > 0)
                    {
                        half4 lfoam = sumFoam * (1.0 - sumSmoke.a);
                        half4 liquid = sum * (1.0 - lfoam.a) * (1.0 - sumSmoke.a);
                        sum = sumSmoke + lfoam + liquid;
                    }
                    else
                    {
                        half4 liquid = sum * (1.0 - sumFoam.a);
                        sum = sumFoam + liquid;
                    }
                }
                else {
                    half4 lfoam = sumFoam * (1.0 - sum.a);
                    sum = sum + lfoam;
                }

                return sum;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                o.vertex.w = dot(o.vertex.xz, _Turbulence.zw) + _TurbulenceSpeed;
                o.vertex.xz *= 0.1.xx * _Turbulence.xx;	// extracted from frag
                o.vertex.xz += _Time.xx;
              //  v.vertex.xyz *= _FlaskThickness;
                o.posWorld = TransformObjectToWorld(v.vertex.xyz);
                o.pos = TransformWorldToHClip(o.posWorld);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                if (i.vertex.y > _UpperLimit)
                {
                    discard;
                }

                half t0, t1;
                half3 rd = i.posWorld - _WorldSpaceCameraPos.xyz;
                half dist = length(rd);
                rd /= dist;
                intCylinder(rd, t0, t1);

                t0 = max(0, t0);

                half4 co = raymarch(i.vertex, rd, t0, t1);

                return co;
            }
            ENDHLSL
        }
    }
}
