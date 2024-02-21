using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class debug : MonoBehaviour
{
    public float fadeDuration = 2.0f; // �������ʱ��Ϊ2��
    private Material material; // ����Ĳ���
    private Color initialColor; // ��ʼ��ɫ
    private float elapsedTime = 0.0f; // �Ѿ�����ʱ��
    private bool isMouseDown = false; // �������Ƿ���

    void Start()
    {
        // ��ȡ�����ϵĲ���
        Renderer renderer = GetComponent<Renderer>();
        material = renderer.material;

        // �����ʼ��ɫ
        initialColor = material.color;
    }

    void Update()
    {
        // �������������
        if (Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
            elapsedTime = 0.0f;
        }

        // ����������ɿ�
        else if (Input.GetMouseButtonUp(0))
        {
            isMouseDown = false;
            elapsedTime = 0.0f;
            // ֱ�ӽ�͸���ȹ���
            material.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
            return;
        }

        // ���������һֱ��ס
        if (isMouseDown)
        {
            // ����͸���Ȳ�ֵ
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            float targetAlpha = Mathf.Lerp(initialColor.a, 1f, t);

            // ���²��ʵ���ɫ��͸����
            material.color = new Color(initialColor.r, initialColor.g, initialColor.b, targetAlpha);
        }
    }
}
