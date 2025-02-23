using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PostRender : MonoBehaviour
{
    // 用于存储雾效材质
    public Material fogMaterial;

    // 在摄像机渲染结束后调用
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (fogMaterial != null)
        {
            // 使用自定义材质处理图像
            Graphics.Blit(source, destination, fogMaterial);
        }
        else
        {
            // 如果没有材质，则直接拷贝源纹理到目标纹理
            Graphics.Blit(source, destination);
        }
    }
}
