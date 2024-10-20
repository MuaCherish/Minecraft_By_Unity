Shader "Custom/TwoColorSkybox"
{
    Properties
    {
        _ColorA ("Top Color", Color) = (0.2, 0.5, 1, 1)   // 上方的颜色
        _ColorB ("Bottom Color", Color) = (0.8, 0.8, 0.8, 1) // 下方的颜色
        _BlendRange ("Blend Range", Range(0.0, 1.0)) = 0.2 // 过渡范围
        _Center ("Center", Range(0.0, 1.0)) = 0.5 // 分界线位置
    }
    SubShader
    {
        Tags { "Queue" = "Background" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldDir : TEXCOORD0; // 用于计算方向向量
            };

            float4 _ColorA;
            float4 _ColorB;
            float _BlendRange;
            float _Center;

            // 顶点着色器
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); 
                o.worldDir = normalize(v.vertex.xyz); // 计算顶点的世界方向
                return o;
            }

            // 片段着色器
            float4 frag (v2f i) : SV_Target
            {
                // 将Y轴映射到[0,1]，0表示最低点，1表示最高点
                float heightFactor = saturate((i.worldDir.y + 1.0) / 2.0);

                // 使用 _Center 和 _BlendRange 控制过渡区域的平滑程度
                if (_BlendRange > 0.0)
                {
                    float minRange = _Center - _BlendRange / 2.0;
                    float maxRange = _Center + _BlendRange / 2.0;
                    float blendFactor = smoothstep(minRange, maxRange, heightFactor);
                    return lerp(_ColorB, _ColorA, blendFactor); // 混合颜色
                }
                else
                {
                    // 当 BlendRange 为 0 时，直接在 Center 处进行硬切换
                    return heightFactor > _Center ? _ColorA : _ColorB;
                }
            }
            ENDCG
        }
    }
    FallBack "RenderType"
}
