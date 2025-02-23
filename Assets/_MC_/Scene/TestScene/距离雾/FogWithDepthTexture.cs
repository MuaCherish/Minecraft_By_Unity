using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class FogWithDepthTexture : MonoBehaviour
{
    public Material fogMaterial;  // 直接引用材质
    public Color fogColor = Color.gray;  // 雾的颜色
    public float fogStart = 0f;          // 雾的开始距离
    public float fogEnd = 50f;           // 雾的结束距离

    private Camera myCamera;

    private void Start()
    {
        myCamera = GetComponent<Camera>();
        myCamera.depthTextureMode |= DepthTextureMode.Depth; // 开启深度纹理
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (fogMaterial != null)
        {
            // 传递雾的参数
            fogMaterial.SetColor("_FogColor", fogColor);
            fogMaterial.SetFloat("_FogStart", fogStart);
            fogMaterial.SetFloat("_FogEnd", fogEnd);

            // 计算摄像机的裁剪空间距离
            float nearClip = myCamera.nearClipPlane;
            float farClip = myCamera.farClipPlane;
            fogMaterial.SetFloat("_Near", nearClip);
            fogMaterial.SetFloat("_Far", farClip);

            Graphics.Blit(src, dest, fogMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);  // 不做处理，直接渲染
        }
    }
}
