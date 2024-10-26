Shader "Custom/DistanceFogWithSkybox"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)     // 物体的颜色
        _FogDistance ("Fog Distance", Float) = 50.0  // 雾效开始的距离
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

            float4 _Color;             // 物体的颜色
            float _FogDistance;        // 雾效的距离
            float4x4 _CamToWorld;      // 相机的世界矩阵

            // Unity提供的天空盒全局纹理
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
                o.vertex = UnityObjectToClipPos(v.vertex); // 转换到裁剪空间
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;  // 世界空间坐标
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 获取相机的位置
                float3 cameraPos = _WorldSpaceCameraPos;

                // 计算该像素与相机的距离
                float dist = distance(i.worldPos, cameraPos);

                // 雾效的权重值：基于距离，远处的物体会更接近天空盒颜色
                float fogFactor = saturate(dist / _FogDistance);

                // 采样天空盒颜色
                fixed4 skyboxColor = texCUBE(_SkyboxTex, normalize(i.worldPos - cameraPos));

                // 根据距离，将物体颜色逐渐过渡为天空盒颜色
                return lerp(_Color, skyboxColor, fogFactor);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
