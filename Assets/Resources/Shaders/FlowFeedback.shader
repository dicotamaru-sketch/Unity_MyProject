// 残像バッファ: 前フレームを流れ場に沿って移流させ減衰し、今フレームの軌跡を加える。
// 流れ場は FlowSimulation と同じリング曲線から導く（見た目の後付けではなく同じ数理系）。
Shader "Hidden/Flow/Feedback"
{
    Properties
    {
        _MainTex ("Previous", 2D) = "black" {}
        _TrailTex ("Trail", 2D) = "black" {}
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _TrailTex;
            float4 _MainTex_TexelSize;

            float _Decay;
            float _InputWeight;
            float _Dt;
            float _AdvectScale;
            float4 _CamCenter;
            float4 _CamHalfSize;
            float4 _Ring;        // x: radius, y: wobble amp, z: wobble freq, w: angular speed
            float _RingWidth;
            float4 _Drag;        // xy: world pos, zw: world velocity
            float _DragRadius;
            float _DragStrength;
            float _Spread;

            float2 FlowVelocity(float2 world)
            {
                float r = length(world);
                float u = atan2(world.y, world.x);
                float ringR = _Ring.x + _Ring.y * sin(_Ring.z * u);
                float2 tang = float2(-world.y, world.x) / max(r, 1e-4);
                float band = exp(-pow((r - ringR) / _RingWidth, 2.0));
                float2 vel = tang * _Ring.w * r * band;

                float2 dd = world - _Drag.xy;
                float dragFall = exp(-dot(dd, dd) / max(_DragRadius * _DragRadius, 1e-4));
                vel += _Drag.zw * _DragStrength * dragFall;
                return vel;
            }

            float4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float2 world = _CamCenter.xy + (uv - 0.5) * 2.0 * _CamHalfSize.xy;
                float2 vel = FlowVelocity(world);
                float2 prevUV = uv - vel * _Dt * _AdvectScale / (2.0 * _CamHalfSize.xy);

                float2 t = _MainTex_TexelSize.xy * _Spread;
                float4 prev = tex2D(_MainTex, prevUV) * 4.0;
                prev += tex2D(_MainTex, prevUV + float2( t.x, 0));
                prev += tex2D(_MainTex, prevUV + float2(-t.x, 0));
                prev += tex2D(_MainTex, prevUV + float2(0,  t.y));
                prev += tex2D(_MainTex, prevUV + float2(0, -t.y));
                prev /= 8.0;

                float4 trail = tex2D(_TrailTex, uv);
                float4 col = prev * _Decay + trail * _InputWeight;
                return col;
            }
            ENDCG
        }
    }
}
