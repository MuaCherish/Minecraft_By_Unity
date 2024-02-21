using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class debug : MonoBehaviour
{
    public float fadeDuration = 2.0f; // 渐变持续时间为2秒
    private Material material; // 物体的材质
    private Color initialColor; // 初始颜色
    private float elapsedTime = 0.0f; // 已经过的时间
    private bool isMouseDown = false; // 鼠标左键是否按下

    void Start()
    {
        // 获取物体上的材质
        Renderer renderer = GetComponent<Renderer>();
        material = renderer.material;

        // 保存初始颜色
        initialColor = material.color;
    }

    void Update()
    {
        // 如果鼠标左键按下
        if (Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
            elapsedTime = 0.0f;
        }

        // 如果鼠标左键松开
        else if (Input.GetMouseButtonUp(0))
        {
            isMouseDown = false;
            elapsedTime = 0.0f;
            // 直接将透明度归零
            material.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
            return;
        }

        // 如果鼠标左键一直按住
        if (isMouseDown)
        {
            // 计算透明度插值
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            float targetAlpha = Mathf.Lerp(initialColor.a, 1f, t);

            // 更新材质的颜色和透明度
            material.color = new Color(initialColor.r, initialColor.g, initialColor.b, targetAlpha);
        }
    }
}
