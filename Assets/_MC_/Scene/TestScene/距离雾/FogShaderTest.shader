Shader "Custom/FogWithDepth"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0.5, 0.5, 0.5, 1)
        _FogStart ("Fog Start", Float) = 10.0
        _FogEnd ("Fog End", Float) = 50.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float depth : TEXCOORD0;
            };

            float _Near;
            float _Far;
            float _FogStart;
            float _FogEnd;
            float4 _FogColor;

            v2f vert (appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.depth = o.pos.z / o.pos.w; // 计算裁剪空间深度
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // 将深度值转换为 0-1 范围的线性深度
                float depth = Linear01Depth(i.depth);

                // 计算雾的插值比例
                float fogFactor = saturate((depth - _FogStart) / (_FogEnd - _FogStart));

                // 混合原始颜色与雾的颜色
                float4 color = lerp(float4(1, 1, 1, 1), _FogColor, fogFactor);
                return color;
            }
            ENDCG
        }
    }
}
