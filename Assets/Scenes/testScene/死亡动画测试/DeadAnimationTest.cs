using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadAnimationTest : MonoBehaviour
{
    float width = 1f;
    float height = 1f;
    public GameObject Cube;
    public float rotationTime = 1f;

    public bool Toggle;
    private void Update()
    {
        if (Toggle)
        {
            DeadAnimation();
            Toggle = false;
        }
    }


    void DeadAnimation()
    {
        StartCoroutine(RotateCubeAroundPoint(Cube, 90f, rotationTime));
    }

    IEnumerator RotateCubeAroundPoint(GameObject obj, float angle, float duration)
    {
        // �ҵ����ڵ�
        Vector3 footRoot = transform.position - new Vector3(0f, height / 2f, 0f);

        // ��ȡ��ʼ��ת
        Quaternion startRotation = obj.transform.rotation;

        // ����Ŀ����ת������ Cube �� forward ����ת
        Quaternion endRotation = startRotation * Quaternion.Euler(angle, 0, 0);

        // ������ת��Ϊ Cube.transform.forward
        Vector3 rotationAxis = obj.transform.forward;

        // ��ת��ʼʱ��
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;

            // ����ת�ǶȲ�ֵ��ʹ�� Slerp ��ƽ����ת
            obj.transform.RotateAround(footRoot, rotationAxis, angle * Time.deltaTime / duration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // ȷ�����յ���ת�Ƕ�
        obj.transform.RotateAround(footRoot, rotationAxis, angle * Time.deltaTime / duration);
    }


}
