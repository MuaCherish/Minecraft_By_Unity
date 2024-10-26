Shader "Custom/DistanceFogWithSkybox"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)     // �������ɫ
        _FogDistance ("Fog Distance", Float) = 50.0  // ��Ч��ʼ�ľ���
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

            float4 _Color;             // �������ɫ
            float _FogDistance;        // ��Ч�ľ���
            float4x4 _CamToWorld;      // ������������

            // Unity�ṩ����պ�ȫ������
            samplerCUBE _SkyboxTex;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // ת�����ü��ռ�
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;  // ����ռ�����
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // ��ȡ�����λ��
                float3 cameraPos = _WorldSpaceCameraPos;

                // ���������������ľ���
                float dist = distance(i.worldPos, cameraPos);

                // ��Ч��Ȩ��ֵ�����ھ��룬Զ�����������ӽ���պ���ɫ
                float fogFactor = saturate(dist / _FogDistance);

                // ������պ���ɫ
                fixed4 skyboxColor = texCUBE(_SkyboxTex, normalize(i.worldPos - cameraPos));

                // ���ݾ��룬��������ɫ�𽥹���Ϊ��պ���ɫ
                return lerp(_Color, skyboxColor, fogFactor);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
