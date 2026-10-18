Shader "UI/CenterReveal"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("黑色扩散半径", Range(0, 2)) = 0
        _Softness ("Softness", Range(0.02, 1)) = 0.12
        _Aspect ("画面宽高比", Float) = 1
        _MaskColor ("遮罩颜色", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float2 _Center;
            float _Radius;
            float _Softness;
            float _Aspect;
            fixed4 _MaskColor;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;
                float2 circleSpace = float2((uv.x - _Center.x) * _Aspect, uv.y - _Center.y);
                float distanceFromCenter = length(circleSpace);

                // 半径越大，黑色区域越大；半径达到 1 时可覆盖整个屏幕。
                float blackMask = 1.0 - smoothstep(_Radius - _Softness, _Radius, distanceFromCenter);

                fixed4 col = fixed4(_MaskColor.rgb, blackMask * _MaskColor.a);
                col *= i.color;
                return col;
            }
            ENDCG
        }
    }
}
