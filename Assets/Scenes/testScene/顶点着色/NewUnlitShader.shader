Shader "Custom/VertexColorTextureShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // ���ʵĻ�������
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
                float4 color : COLOR; // ��ȡ������ɫ
                float2 uv : TEXCOORD0; // UV ����
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR; // ���ݶ�����ɫ��Ƭ��
                float2 uv : TEXCOORD0; // ����UV����
            };

            sampler2D _MainTex; // �������������

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color; // ������ɫ���ݵ�Ƭ��
                o.uv = v.uv; // ����UV����
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // �������в�����ɫ
                fixed4 texColor = tex2D(_MainTex, i.uv);
                // ��������ɫ�붥����ɫ��������
                fixed4 col = texColor * i.color;
                return col; // ����������ɫ
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
