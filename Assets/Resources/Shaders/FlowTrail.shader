// 粒子軌跡。uv.x = 共鳴値(0..1)、uv.y = 減衰(1=先頭, 0=尾)。
// 頂点カラーは8bitに量子化されHDRを運べないため、uvで強度を渡す。
Shader "Flow/Trail"
{
    Properties
    {
        _CoolColor ("Cool", Color) = (0.35, 0.55, 0.95, 1)
        _WarmColor ("Warm", Color) = (1.0, 0.75, 0.35, 1)
        _BaseIntensity ("Base Intensity", Float) = 0.9
        _GlowBoost ("Glow Boost", Float) = 4.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend One One
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _CoolColor;
            float4 _WarmColor;
            float _BaseIntensity;
            float _GlowBoost;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float glow = saturate(i.uv.x);
                float fade = saturate(i.uv.y);
                float3 c = lerp(_CoolColor.rgb, _WarmColor.rgb, glow);
                c *= (_BaseIntensity + _GlowBoost * glow * glow) * fade * fade;
                return float4(c, 1.0);
            }
            ENDCG
        }
    }
}
