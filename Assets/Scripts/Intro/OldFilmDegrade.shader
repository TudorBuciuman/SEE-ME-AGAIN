Shader "SeeMeAgain/OldFilmDegrade"
{
    // Full-rect UI overlay for the apology beat: sepia/desaturated tint, grain,
    // drifting scratch lines, projector flicker, vignette, and slight frame
    // jitter — an old newsreel or damaged 16mm title card, not a digital glitch.
    Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)
        _SepiaAmount ("Sepia Amount", Range(0,1)) = 0.6
        _GrainAmount ("Grain Amount", Range(0,1)) = 0.35
        _ScratchAmount ("Scratch Amount", Range(0,1)) = 0.4
        _FlickerAmount ("Projector Flicker Amount", Range(0,1)) = 0.25
        _VignetteStrength ("Vignette Strength", Range(0,2)) = 1
        _JitterAmount ("Frame Jitter", Range(0, 0.05)) = 0.01
        _Time01 ("Seed / Time", Float) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };

            fixed4 _Color;
            float _SepiaAmount, _GrainAmount, _ScratchAmount, _FlickerAmount, _VignetteStrength, _JitterAmount, _Time01;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float2 jitter = float2(
                    frac(sin(_Time01 * 13.1) * 43758.5) - 0.5,
                    frac(sin(_Time01 * 7.7) * 24634.6) - 0.5
                ) * _JitterAmount;
                o.uv = v.uv + jitter;
                o.color = v.color * _Color;
                return o;
            }

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                fixed3 col = fixed3(0.05, 0.045, 0.04);

                // sepia gradient, like an old title card
                fixed3 sepia = lerp(fixed3(0.05, 0.045, 0.04), fixed3(0.28, 0.2, 0.12), 1.0 - uv.y * 0.6);
                col = lerp(col, sepia, _SepiaAmount);

                // grain
                float grain = hash(uv * 900.0 + _Time01 * 60.0);
                col += (grain - 0.5) * _GrainAmount * 0.5;

                // a few drifting vertical scratch lines
                float scratchLine = 0.0;
                for (int s = 0; s < 3; s++)
                {
                    float seedX = hash(float2(s, floor(_Time01 * 4.0 + s)));
                    float lineX = frac(seedX + _Time01 * 0.05 * (s + 1));
                    float d = abs(uv.x - lineX);
                    scratchLine += smoothstep(0.0015, 0.0, d);
                }
                col += scratchLine * _ScratchAmount;

                // projector flicker
                float flicker = 1.0 + (hash(float2(floor(_Time01 * 24.0), 0.0)) - 0.5) * _FlickerAmount;
                col *= flicker;

                // vignette
                float2 centered = uv - 0.5;
                float vig = 1.0 - dot(centered, centered) * _VignetteStrength;
                col *= saturate(vig);

                float alpha = saturate(0.85 * i.color.a);
                return fixed4(col * i.color.rgb, alpha);
            }
            ENDCG
        }
    }
}
