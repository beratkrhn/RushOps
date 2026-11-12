// CRT look of the arcade screen (issue #323, F-ARC-05): a slight barrel curvature, scanlines on every game pixel row
// and a soft vignette. A UI shader for the RawImage that shows the game texture, so it touches only the screen,
// not the HUD above it or the control panel. Strengths come from ArcadeConfig.
Shader "RushOps/ArcadeCRT"
{
    Properties
    {
        [PerRendererData] _MainTex ("Game Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Curvature ("Curvature", Range(0, 0.3)) = 0.04
        _Scanlines ("Scanline Strength", Range(0, 1)) = 0.18
        _Lines ("Scanline Count", Float) = 320
        _Vignette ("Vignette", Range(0, 1)) = 0.25

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Curvature;
            float _Scanlines;
            float _Lines;
            float _Vignette;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Barrel curvature: points move outwards the further they are from the centre.
                float2 centred = i.uv * 2.0 - 1.0;
                centred *= 1.0 + _Curvature * dot(centred, centred);
                float2 uv = centred * 0.5 + 0.5;
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                {
                    return fixed4(0.05, 0.043, 0.078, 1.0) * i.color;
                }

                fixed4 color = tex2D(_MainTex, uv);
                // The lower part of every game pixel row is darker.
                float row = frac(uv.y * _Lines);
                color.rgb *= 1.0 - _Scanlines * step(row, 0.35);
                // Darker towards the edges of the tube.
                float edge = uv.x * uv.y * (1.0 - uv.x) * (1.0 - uv.y) * 16.0;
                color.rgb *= lerp(1.0, saturate(pow(edge, 0.25)), _Vignette);
                color.a = 1.0;
                return color * i.color;
            }
            ENDCG
        }
    }
}
