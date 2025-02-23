Shader "Custom/VertexColorTextureShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // 材质的基础纹理
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR; // 获取顶点颜色
                float2 uv : TEXCOORD0; // UV 坐标
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR; // 传递顶点颜色到片段
                float2 uv : TEXCOORD0; // 传递UV坐标
            };

            sampler2D _MainTex; // 定义纹理采样器

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color; // 顶点颜色传递到片段
                o.uv = v.uv; // 传递UV坐标
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 从纹理中采样颜色
                fixed4 texColor = tex2D(_MainTex, i.uv);
                // 将纹理颜色与顶点颜色相乘来混合
                fixed4 col = texColor * i.color;
                return col; // 返回最终颜色
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
