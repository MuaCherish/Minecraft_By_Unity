using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PostRender : MonoBehaviour
{
    // ���ڴ洢��Ч����
    public Material fogMaterial;

    // ���������Ⱦ���������
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (fogMaterial != null)
        {
            // ʹ���Զ�����ʴ���ͼ��
            Graphics.Blit(source, destination, fogMaterial);
        }
        else
        {
            // ���û�в��ʣ���ֱ�ӿ���Դ����Ŀ������
            Graphics.Blit(source, destination);
        }
    }
}
