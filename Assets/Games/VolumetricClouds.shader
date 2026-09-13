Shader "Hidden/SeeMeAgain/VolumetricClouds"
{
    Properties
    {
        _NoiseTex        ("Cloud Noise 3D", 3D) = "" {}
        _WeatherTex      ("Weather Map (optional)", 2D) = "white" {}
        _CloudColor      ("Cloud Base Color", Color) = (0.5, 0.53, 0.62, 1)
        _SunColor        ("Sun / Backlight Color", Color) = (1.0, 0.82, 0.55, 1)
        _AmbientTop      ("Ambient Top (sky)", Color) = (0.55, 0.62, 0.82, 1)
        _AmbientBottom   ("Ambient Bottom (shadowed underside)", Color) = (0.10, 0.11, 0.16, 1)

        _BoundsMin ("Cloud Box Min (world)", Vector) = (-6000, 900, -6000, 0)
        _BoundsMax ("Cloud Box Max (world)", Vector) = (6000, 2600, 6000, 0)

        _Coverage           ("Coverage", Range(0,1)) = 0.5
        _DensityMultiplier  ("Density Multiplier", Range(0,5)) = 1.3
        _DetailStrength     ("Detail Erosion Strength", Range(0,1)) = 0.35
        _ShapeScale         ("Shape Noise Scale", Float) = 0.00035
        _DetailScale        ("Detail Noise Scale", Float) = 0.0015

        _WindDir   ("Wind Direction", Vector) = (1, 0, 0.3, 0)
        _WindSpeed ("Wind Speed", Float) = 6

        _StepCount      ("Primary Steps", Range(16,128)) = 72
        _LightStepCount ("Light Steps (self-shadow)", Range(1,12)) = 6

        _ForwardG         ("Forward Scatter G (silver lining)", Range(0,0.99)) = 0.78
        _BackG            ("Back Scatter G", Range(-0.99,0)) = -0.2
        _SilverIntensity  ("Silver Lining Intensity", Range(0,4)) = 1.8
        _PowderStrength   ("Powder Effect Strength", Range(0,4)) = 1.1

        _DebugMode        ("Debug Mode", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "VolumetricCloudsPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D_X(_BlitTexture);

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord   : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                float2 uv = float2((input.vertexID << 1) & 2, input.vertexID & 2);
                o.positionCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);
                o.texcoord = uv;
                #if UNITY_UV_STARTS_AT_TOP
                o.texcoord.y = 1.0 - o.texcoord.y;
                #endif
                return o;
            }

            TEXTURE3D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);
            TEXTURE2D(_WeatherTex);
            SAMPLER(sampler_WeatherTex);

            float4 _CloudColor, _SunColor, _AmbientTop, _AmbientBottom;
            float4 _BoundsMin, _BoundsMax, _WindDir;
            float _Coverage, _DensityMultiplier, _DetailStrength;
            float _ShapeScale, _DetailScale, _WindSpeed;
            float _StepCount, _LightStepCount;
            float _ForwardG, _BackG, _SilverIntensity, _PowderStrength;
            float _DebugMode;

            #define PI 3.14159265359

            float Rema(float v, float lo, float hi, float outLo, float outHi)
            {
                return outLo + (v - lo) / max(hi - lo, 1e-5) * (outHi - outLo);
            }

            float2 RayBoxIntersect(float3 ro, float3 rd, float3 bmin, float3 bmax)
            {
                float3 invDir = 1.0 / rd;
                float3 t0 = (bmin - ro) * invDir;
                float3 t1 = (bmax - ro) * invDir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);
                float dstMin = max(max(tmin.x, tmin.y), tmin.z);
                float dstMax = min(min(tmax.x, tmax.y), tmax.z);
                return float2(dstMin, dstMax);
            }

            float SampleDensity(float3 posWS, bool cheap)
            {
                float3 windOffset = _WindDir.xyz * _Time.y * _WindSpeed;

                float3 shapeUVW = (posWS + windOffset) * _ShapeScale;
                float4 shapeNoise = SAMPLE_TEXTURE3D_LOD(_NoiseTex, sampler_NoiseTex, frac(shapeUVW), 0);

                float heightFrac = saturate((posWS.y - _BoundsMin.y) / max(_BoundsMax.y - _BoundsMin.y, 1.0));
                float heightGradient = saturate(Rema(heightFrac, 0.0, 0.12, 0.0, 1.0))
                                      * saturate(Rema(heightFrac, 0.55, 1.0, 1.0, 0.0));

                float coverage = _Coverage;
                float2 weatherUV = posWS.xz * 0.00004 + windOffset.xz * 0.00002;
                float weather = SAMPLE_TEXTURE2D_LOD(_WeatherTex, sampler_WeatherTex, frac(weatherUV), 0).r;
                coverage = saturate(coverage * lerp(0.6, 1.4, weather));

                float baseShape = shapeNoise.r * heightGradient;
                float density = saturate(Rema(baseShape, 1.0 - coverage, 1.0, 0.0, 1.0));

                if (!cheap && density > 0.001)
                {
                    float3 detailUVW = (posWS + windOffset * 1.5) * _DetailScale;
                    float3 detailNoise = SAMPLE_TEXTURE3D_LOD(_NoiseTex, sampler_NoiseTex, frac(detailUVW), 0).gba;
                    float detailFBM = detailNoise.r * 0.625 + detailNoise.g * 0.25 + detailNoise.b * 0.125;
                    float erodeAmount = lerp(detailFBM, 1.0 - detailFBM, saturate(heightFrac * 2.0));
                    density = saturate(Rema(density, erodeAmount * _DetailStrength, 1.0, 0.0, 1.0));
                }

                return density * _DensityMultiplier;
            }

            float HenyeyGreenstein(float cosAngle, float g)
            {
                float g2 = g * g;
                return (1.0 - g2) / (4.0 * PI * pow(max(1.0 + g2 - 2.0 * g * cosAngle, 1e-4), 1.5));
            }

            float PhaseFunction(float cosAngle)
            {
                float forward = HenyeyGreenstein(cosAngle, _ForwardG) * _SilverIntensity;
                float back = HenyeyGreenstein(cosAngle, _BackG);
                return max(forward, back);
            }

            float LightMarch(float3 posWS, float3 lightDir)
            {
                float stepSize = 130.0;
                float totalDensity = 0.0;
                float3 p = posWS;

                for (int i = 0; i < (int)_LightStepCount; i++)
                {
                    p += lightDir * stepSize;
                    totalDensity += SampleDensity(p, true) * stepSize;
                }
                return exp(-totalDensity * 0.008);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                half4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                // --- DEBUG MODE 1: Shader Execution Test ---
                if (_DebugMode == 1.0) return half4(1, 0, 1, 1); // Bright Magenta

                float rawDepth = SampleSceneDepth(uv);
                float3 camPos = _WorldSpaceCameraPos;
                float3 farWS = ComputeWorldSpacePosition(uv, UNITY_RAW_FAR_CLIP_VALUE, UNITY_MATRIX_I_VP);
                float3 rayDir = normalize(farWS - camPos);

                // --- DEBUG MODE 2: Ray Directions ---
                if (_DebugMode == 2.0) return half4(rayDir * 0.5 + 0.5, 1.0);

                float sceneDist = 1e9;
                UNITY_BRANCH
                if (rawDepth > 0.00001) 
                {
                    float3 scenePosWS = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
                    sceneDist = length(scenePosWS - camPos);
                }

                // --- DEBUG MODE 3: Scene Depth Visualization ---
                if (_DebugMode == 3.0) return half4(frac(sceneDist / 1000.0).rrr, 1.0);

                float2 boxHit = RayBoxIntersect(camPos, rayDir, _BoundsMin.xyz, _BoundsMax.xyz);
                float tStart = max(boxHit.x, 0.0);
                float tEnd = min(boxHit.y, sceneDist);

                // --- DEBUG MODE 4: Box Bounds Ray Intersection ---
                if (_DebugMode == 4.0)
                {
                    if (boxHit.y > boxHit.x && boxHit.y > 0) return half4(0, 1, 0, 1); // Green where box is hit
                    return half4(0, 0, 0, 1); // Black where box is missed
                }

                if (boxHit.y < boxHit.x || tEnd <= tStart)
                {
                    return sceneColor;
                }

                // --- DEBUG MODE 5: Direct 3D Noise Texture Inspection ---
                if (_DebugMode == 5.0)
                {
                    float3 debugPos = camPos + rayDir * tStart;
                    float3 noiseSample = SAMPLE_TEXTURE3D_LOD(_NoiseTex, sampler_NoiseTex, frac(debugPos * _ShapeScale), 0).rgb;
                    return half4(noiseSample, 1.0);
                }

                int stepCount = (int)_StepCount;
                float stepSize = (tEnd - tStart) / stepCount;
                float3 lightDir = normalize(_MainLightPosition.xyz);

                float transmittance = 1.0;
                float3 accumColor = float3(0, 0, 0);
                float t = tStart + stepSize * 0.5;

                [loop]
                for (int i = 0; i < stepCount; i++)
                {
                    if (transmittance < 0.01) break;

                    float3 p = camPos + rayDir * t;
                    float density = SampleDensity(p, false);

                    if (density > 0.001)
                    {
                        float lightTransmittance = LightMarch(p, lightDir);
                        float cosAngle = dot(rayDir, lightDir);
                        float phase = PhaseFunction(cosAngle);

                        float heightFrac = saturate((p.y - _BoundsMin.y) / max(_BoundsMax.y - _BoundsMin.y, 1.0));
                        float3 ambient = lerp(_AmbientBottom.rgb, _AmbientTop.rgb, heightFrac);

                        float powder = 1.0 - exp(-density * _PowderStrength * 2.0);
                        float3 litColor = _CloudColor.rgb * ambient
                                         + _SunColor.rgb * lightTransmittance * phase * powder;

                        float absorption = density * stepSize * 0.004;
                        float stepTransmittance = exp(-absorption);

                        accumColor += transmittance * (1.0 - stepTransmittance) * litColor;
                        transmittance *= stepTransmittance;
                    }

                    t += stepSize;
                }

                // --- DEBUG MODE 6: Accumulated Cloud Mask (Alpha Only) ---
                if (_DebugMode == 6.0) return half4((1.0 - transmittance).rrr, 1.0);

                float3 finalColor = sceneColor.rgb * transmittance + accumColor;
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}