using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool isFind;
    public GameObject Chunk;
    public Camera eyescamera; // ���������
    public float expandAngle = 10f; // �ӽ���չ�Ƕ�

    // �жϸ��������Ƿ�����չ�����Ұ��Χ��
    public bool IsInCameraView(Vector3 worldPosition)
    {
        // ��ȡ�����������ķ�������
        Vector3 directionToTarget = worldPosition - eyescamera.transform.position;
        // ��������������ǰ���ļн�
        float angleToTarget = Vector3.Angle(eyescamera.transform.forward, directionToTarget);

        // ��ȡ��������ӽ�һ��Ƕȣ�����60����Ұ�����Ϊ30�ȣ�
        float halfFov = eyescamera.fieldOfView / 2f;
        // ��չ��İ��
        float expandedFov = halfFov + expandAngle;

        // ���������Ƿ�����չ�����׶�Ƕ���
        bool isInExpandedFov = angleToTarget < expandedFov;

        // �ж������Ƿ���������Ĳü���Χ�ڣ�����������0�����������ǰ����
        bool isInDistance = Vector3.Dot(eyescamera.transform.forward, directionToTarget) > 0;

        return isInExpandedFov && isInDistance;
    }

    private void Update()
    {
        isFind = IsInCameraView(Chunk.transform.position);
    }
}
