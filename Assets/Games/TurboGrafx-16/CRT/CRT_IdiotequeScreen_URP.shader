Shader "Custom/CRT_IdiotequeScreen_URP"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "black" {}
        _ScreenTint ("Line Color", Color) = (0.15, 1.0, 0.25, 1)
        _FlickerIntensity ("Flicker Intensity (0-1, set by script)", Range(0,1)) = 0.1
        _NoiseAmount ("Noise Amount (0-1, set by script)", Range(0,1)) = 0.1
        _JitterAmount ("Jitter Amount (0-1, set by script)", Range(0,0.1)) = 0.0
        _Brightness ("Brightness (set by script)", Float) = 1.0

        [Header(Radial Burst Pattern)]
        _LineSegments ("Angular Line Segments", Float) = 64
        _LineThickness ("Line Thickness", Range(0.01, 0.3)) = 0.08
        _MinActiveFraction ("Min Active Lines (idle)", Range(0,1)) = 0.05
        _MaxActiveFraction ("Max Active Lines (loud)", Range(0,1)) = 0.55
        _MinLineLength ("Min Line Length", Range(0,1)) = 0.15
        _MaxLineLength ("Max Line Length (loud)", Range(0,2)) = 1.1
        _PatternSpeedIdle ("Pattern Re-roll Speed, Idle (Hz)", Range(0.5, 30)) = 3
        _PatternSpeedLoud ("Pattern Re-roll Speed, Loud (Hz)", Range(0.5, 30)) = 14
        _AspectCorrection ("Aspect Correction (screen width / height)", Float) = 1.0

        [Header(Scanline and Glow)]
        _ScanlineCount ("Scanline Count", Float) = 240
        _ScanlineStrength ("Scanline Strength", Range(0,1)) = 0.25
        _VignetteStrength ("Vignette Strength", Range(0,1)) = 0.3
        _GlowAmount ("Line Glow", Range(0,1)) = 0.4
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _ScreenTint;
                float _FlickerIntensity;
                float _NoiseAmount;
                float _JitterAmount;
                float _Brightness;
                float _LineSegments;
                float _LineThickness;
                float _MinActiveFraction;
                float _MaxActiveFraction;
                float _MinLineLength;
                float _MaxLineLength;
                float _PatternSpeedIdle;
                float _PatternSpeedLoud;
                float _AspectCorrection;
                float _ScanlineCount;
                float _ScanlineStrength;
                float _VignetteStrength;
                float _GlowAmount;
            CBUFFER_END

            #define PI 3.14159265359
            #define TWO_PI 6.28318530718

            float hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float t = _Time.y;

                float lineY = floor(uv.y * _ScanlineCount);
                float jitterNoise = hash21(float2(lineY, floor(t * 30.0))) - 0.5;
                uv.x += jitterNoise * _JitterAmount;

                float4 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                float2 centered = uv - 0.5;
                centered.x *= _AspectCorrection;
                float r = length(centered) * 2.0; 
                float theta = atan2(centered.y, centered.x); 

                float loudness = saturate(_FlickerIntensity);

                float rerollSpeed = lerp(_PatternSpeedIdle, _PatternSpeedLoud, loudness);
                float frameSeed = floor(t * rerollSpeed);

                float segments = max(4.0, _LineSegments);
                float angle01 = (theta + PI) / TWO_PI; 
                float scaledAngle = angle01 * segments;
                float binIndex = floor(scaledAngle);
                float angleInBin = frac(scaledAngle) - 0.5; 

                float activeFraction = lerp(_MinActiveFraction, _MaxActiveFraction, loudness);
                float existsRoll = hash21(float2(binIndex, frameSeed));
                float exists = step(existsRoll, activeFraction);

                float lengthRoll = hash21(float2(binIndex + 17.0, frameSeed));
                float maxLen = lerp(_MinLineLength, _MaxLineLength, saturate(loudness * 1.3));
                float thisLineLength = lerp(_MinLineLength, maxLen, lengthRoll);

                float angularMask = 1.0 - smoothstep(_LineThickness * 0.4, _LineThickness, abs(angleInBin));
                float radialMask = step(r, thisLineLength) * (1.0 - smoothstep(thisLineLength * 0.75, thisLineLength, r));
                float coreGlow = _GlowAmount * (1.0 - smoothstep(0.0, 0.12, r));

                float burstMask = saturate(angularMask * radialMask * exists + coreGlow);

                float3 col = baseTex.rgb + burstMask * _ScreenTint.rgb;

                float scan = sin(uv.y * _ScanlineCount * PI) * 0.5 + 0.5;
                col *= lerp(1.0, scan, _ScanlineStrength);

                float grain = hash21(uv * t * 60.0 + jitterNoise);
                col += (grain - 0.5) * _NoiseAmount * 0.15;

                float flickerPulse = (1.0 - _FlickerIntensity * 0.3) + _FlickerIntensity * hash21(float2(floor(t * 50.0), 0.0)) * 0.3;
                col *= flickerPulse * _Brightness;

                float2 vigUV = uv - 0.5;
                float vig = 1.0 - dot(vigUV, vigUV) * _VignetteStrength * 2.0;
                col *= saturate(vig);

                return float4(saturate(col), 1.0);
            }
            ENDHLSL
        }
    }
}
