Shader "COSMA/UI/Glassmorphism"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _TintColor ("Tint Color", Color) = (0.118, 0.118, 0.118, 0.75)
        _BorderColor ("Border Color", Color) = (0.5, 0.5, 0.5, 0.4)
        _BorderWidth ("Border Width", Float) = 1.5
        _CornerRadius ("Corner Radius", Float) = 16
        _RectSize ("Rect Size", Vector) = (300, 200, 0, 0)

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
            Name "GlassmorphismPass"
        CGPROGRAM
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
                float4 vertex       : SV_POSITION;
                fixed4 color        : COLOR;
                float2 texcoord     : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 screenPos    : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _GlobalUniversalBlurTexture;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            fixed4 _TintColor;
            fixed4 _BorderColor;
            float _BorderWidth;
            float _CornerRadius;
            float4 _RectSize; // x = width, y = height in pixels

            // Signed Distance Function for a rounded rectangle
            // p = point relative to rect center, b = half-extents, r = corner radius
            float roundedRectSDF(float2 p, float2 b, float r)
            {
                float2 d = abs(p) - b + float2(r, r);
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0) - r;
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color;
                OUT.screenPos = ComputeScreenPos(OUT.vertex);
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                // Sample the blur background texture
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                half4 blurColor = tex2D(_GlobalUniversalBlurTexture, screenUV);

                // Apply tint overlay to blurred background
                half4 tinted = blurColor * _TintColor;

                // Sample the main sprite texture (used as mask/shape)
                half4 mainTexColor = tex2D(_MainTex, uv) + _TextureSampleAdd;

                // --- Rounded Rectangle SDF ---
                float2 rectHalfSize = _RectSize.xy * 0.5;
                // Clamp corner radius to half of the shortest dimension
                float effectiveRadius = min(_CornerRadius, min(rectHalfSize.x, rectHalfSize.y));

                // Convert UV (0..1) to pixel-space coordinates centered at origin
                float2 pixelPos = (uv - 0.5) * _RectSize.xy;

                float dist = roundedRectSDF(pixelPos, rectHalfSize, effectiveRadius);

                // Anti-aliased edge mask (1 inside, 0 outside)
                float edgeSoftness = 1.0; // pixels of AA
                float shapeMask = 1.0 - smoothstep(-edgeSoftness, edgeSoftness, dist);

                // Border mask: 1.0 on the border region, 0.0 elsewhere
                float borderOuter = smoothstep(-edgeSoftness, edgeSoftness, dist + _BorderWidth);
                float borderInner = smoothstep(-edgeSoftness, edgeSoftness, dist);
                float borderMask = borderOuter - borderInner;

                // Subtle top-edge highlight for frosted glass effect
                float highlightMask = (1.0 - uv.y) * 0.08 * shapeMask;

                // Compose final color
                half4 panelColor = tinted * IN.color;
                panelColor.rgb += highlightMask; // subtle top highlight

                // Blend border on top
                half4 finalColor = lerp(panelColor, _BorderColor, borderMask * _BorderColor.a);
                finalColor.a = panelColor.a * shapeMask * mainTexColor.a;

                // UI clipping support
                #ifdef UNITY_UI_CLIP_RECT
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(finalColor.a - 0.001);
                #endif

                return finalColor;
            }
        ENDCG
        }
    }
}
