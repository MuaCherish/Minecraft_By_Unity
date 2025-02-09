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
        // 找到根节点
        Vector3 footRoot = transform.position - new Vector3(0f, height / 2f, 0f);

        // 获取起始旋转
        Quaternion startRotation = obj.transform.rotation;

        // 计算目标旋转，绕着 Cube 的 forward 轴旋转
        Quaternion endRotation = startRotation * Quaternion.Euler(angle, 0, 0);

        // 计算旋转轴为 Cube.transform.forward
        Vector3 rotationAxis = obj.transform.forward;

        // 旋转开始时间
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;

            // 将旋转角度插值，使用 Slerp 来平滑旋转
            obj.transform.RotateAround(footRoot, rotationAxis, angle * Time.deltaTime / duration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // 确保最终的旋转角度
        obj.transform.RotateAround(footRoot, rotationAxis, angle * Time.deltaTime / duration);
    }


}
