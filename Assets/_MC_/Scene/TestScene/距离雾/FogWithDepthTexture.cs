using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class FogWithDepthTexture : MonoBehaviour
{
    public Material fogMaterial;  // ֱ�����ò���
    public Color fogColor = Color.gray;  // �����ɫ
    public float fogStart = 0f;          // ��Ŀ�ʼ����
    public float fogEnd = 50f;           // ��Ľ�������

    private Camera myCamera;

    private void Start()
    {
        myCamera = GetComponent<Camera>();
        myCamera.depthTextureMode |= DepthTextureMode.Depth; // �����������
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (fogMaterial != null)
        {
            // ������Ĳ���
            fogMaterial.SetColor("_FogColor", fogColor);
            fogMaterial.SetFloat("_FogStart", fogStart);
            fogMaterial.SetFloat("_FogEnd", fogEnd);

            // ����������Ĳü��ռ����
            float nearClip = myCamera.nearClipPlane;
            float farClip = myCamera.farClipPlane;
            fogMaterial.SetFloat("_Near", nearClip);
            fogMaterial.SetFloat("_Far", farClip);

            Graphics.Blit(src, dest, fogMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);  // ��������ֱ����Ⱦ
        }
    }
}
