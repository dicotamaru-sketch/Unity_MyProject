Shader "Flow/Additive"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _Intensity ("Intensity", Float) = 1
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

            sampler2D _MainTex;
            float _Intensity;

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
                float4 t = tex2D(_MainTex, i.uv);
                return float4(t.rgb * _Intensity, 1.0);
            }
            ENDCG
        }
    }
}
