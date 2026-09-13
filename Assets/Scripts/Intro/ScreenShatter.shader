Shader "SeeMeAgain/ScreenShatter"
{
    // Sudden, deliberate destruction — not a digital glitch. Voronoi cells act
    // as glass shards: cracks flash bright first, then each shard independently
    // falls away to reveal black behind it, at a random offset per shard so the
    // break doesn't read as a clean wipe. Drive _ShatterProgress from 0 to 1.
    Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)
        _ShatterProgress ("Shatter Progress", Range(0,1)) = 0
        _CellCount ("Crack Cell Count", Float) = 14
        _CrackWidth ("Crack Line Width", Range(0.0005, 0.02)) = 0.004
        _Seed ("Seed", Float) = 0

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
            float _ShatterProgress;
            float _CellCount;
            float _CrackWidth;
            float _Seed;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            float2 hash2(float2 p)
            {
                float2 k = float2(127.1, 311.7);
                float n = sin(dot(p, k) + _Seed) * 43758.5453;
                return frac(float2(n, n * 1.6180339887));
            }

            void voronoi(float2 uv, out float dist, out float2 cellHash)
            {
                float2 p = uv * _CellCount;
                float2 ip = floor(p);
                float2 fp = frac(p);
                dist = 8.0;
                cellHash = float2(0, 0);

                for (int y = -1; y <= 1; y++)
                for (int x = -1; x <= 1; x++)
                {
                    float2 neighbor = float2(x, y);
                    float2 point_ = hash2(ip + neighbor);
                    float2 diff = neighbor + point_ - fp;
                    float d = length(diff);
                    if (d < dist)
                    {
                        dist = d;
                        cellHash = hash2(ip + neighbor);
                    }
                }
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float dist;
                float2 cellHash;
                voronoi(uv, dist, cellHash);

                // crack lines flash bright as the shatter begins
                float crackLine = smoothstep(_CrackWidth * 3.0, 0.0, dist) * saturate(_ShatterProgress * 3.0);

                // each shard falls away at its own random point in the timeline
                float shardThreshold = cellHash.x;
                float shardProgress = saturate((_ShatterProgress - shardThreshold * 0.5) / 0.5);
                float shardAlpha = 1.0 - shardProgress;

                fixed3 crackColor = fixed3(1, 1, 1);
                fixed3 col = lerp(fixed3(0, 0, 0), crackColor, crackLine);

                float alpha = saturate(shardAlpha + crackLine * 0.5) * i.color.a;
                return fixed4(col * i.color.rgb, alpha);
            }
            ENDCG
        }
    }
}
