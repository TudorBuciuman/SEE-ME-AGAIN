Shader "UI/CanvasPaintEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Painting Effect)]
        _BrushSize ("Brush Stroke Size", Range(1, 5)) = 2
        _ColorSteps ("Color Posterization Steps", Range(2, 32)) = 12
        
        [Header(Canvas Texture)]
        _CanvasTex ("Canvas Grain Texture (Grayscale)", 2D) = "bump" {}
        _CanvasTiling ("Canvas Tiling", Vector) = (10, 10, 0, 0)
        _CanvasBumpiness ("Canvas Relief Depth", Range(0, 1)) = 0.25
        
        // UI Mask Support
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_TexelSize;

            int _BrushSize;
            float _ColorSteps;

            sampler2D _CanvasTex;
            float4 _CanvasTiling;
            float _CanvasBumpiness;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            // Simplified Kuwahara Filter for Paint Brush Effect
            fixed4 GetPaintColor(float2 uv)
            {
                int radius = _BrushSize;
                float2 srcSize = _MainTex_TexelSize.xy;

                float3 m[4] = { float3(0,0,0), float3(0,0,0), float3(0,0,0), float3(0,0,0) };
                float3 s[4] = { float3(0,0,0), float3(0,0,0), float3(0,0,0), float3(0,0,0) };
                int samples[4] = { 0, 0, 0, 0 };

                for (int j = -radius; j <= radius; ++j)
                {
                    for (int i = -radius; i <= radius; ++i)
                    {
                        float3 c = tex2D(_MainTex, uv + float2(i, j) * srcSize).rgb;
                        
                        int quadrant = 0;
                        if (i >= 0 && j >= 0) quadrant = 0;
                        else if (i <= 0 && j >= 0) quadrant = 1;
                        else if (i <= 0 && j <= 0) quadrant = 2;
                        else quadrant = 3;

                        m[quadrant] += c;
                        s[quadrant] += c * c;
                        samples[quadrant]++;
                    }
                }

                float minVar = 1000.0;
                float3 finalColor = float3(0,0,0);

                for (int k = 0; k < 4; ++k)
                {
                    m[k] /= samples[k];
                    s[k] = abs(s[k] / samples[k] - m[k] * m[k]);
                    float variance = s[k].r + s[k].g + s[k].b;

                    if (variance < minVar)
                    {
                        minVar = variance;
                        finalColor = m[k];
                    }
                }

                // Posterize slightly to emphasize paint thickness
                finalColor = floor(finalColor * _ColorSteps) / _ColorSteps;

                float alpha = tex2D(_MainTex, uv).a;
                return fixed4(finalColor, alpha);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. Paint Filter Effect
                fixed4 color = (GetPaintColor(IN.texcoord) + _TextureSampleAdd) * IN.color;

                // 2. Canvas Grain Overlay
                float2 canvasUV = IN.texcoord * _CanvasTiling.xy;
                fixed4 canvasTex = tex2D(_CanvasTex, canvasUV);
                
                // Convert canvas gray scale into lighting bumps
                float canvasLuminance = dot(canvasTex.rgb, float3(0.299, 0.587, 0.114));
                float canvasFactor = lerp(1.0, canvasLuminance * 1.5, _CanvasBumpiness);
                
                color.rgb *= canvasFactor;

                // 3. Standard UI Mask Clipping
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
            ENDHLSL
        }
    }
}