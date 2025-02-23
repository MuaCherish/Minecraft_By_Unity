using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool isFind;
    public GameObject Chunk;
    public Camera eyescamera; // 摄像机引用
    public float expandAngle = 10f; // 视角扩展角度

    // 判断给定坐标是否在扩展后的视野范围内
    public bool IsInCameraView(Vector3 worldPosition)
    {
        // 获取摄像机到物体的方向向量
        Vector3 directionToTarget = worldPosition - eyescamera.transform.position;
        // 计算物体和摄像机前方的夹角
        float angleToTarget = Vector3.Angle(eyescamera.transform.forward, directionToTarget);

        // 获取摄像机的视角一半角度（例如60度视野，半角为30度）
        float halfFov = eyescamera.fieldOfView / 2f;
        // 扩展后的半角
        float expandedFov = halfFov + expandAngle;

        // 计算物体是否在扩展后的视锥角度内
        bool isInExpandedFov = angleToTarget < expandedFov;

        // 判断物体是否在摄像机的裁剪范围内（距离必须大于0，物体必须在前方）
        bool isInDistance = Vector3.Dot(eyescamera.transform.forward, directionToTarget) > 0;

        return isInExpandedFov && isInDistance;
    }

    private void Update()
    {
        isFind = IsInCameraView(Chunk.transform.position);
    }
}
