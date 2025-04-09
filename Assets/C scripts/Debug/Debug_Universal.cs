using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;  // ���� Stopwatch ��
using Homebrew;

public class Debug_Universal : MonoBehaviour
{
    public MC_Service_World Service_World;
    public MC_Service_Entity Service_Entity;
    public GameObject TargetObj;
    public float Radius;

    [Foldout("���Խ��", true)]
    public bool isFindEntity;
    public int EntityNumber;

    void TestFunction()
    {
        if (TargetObj == null)
            return;

        if (Service_Entity.GetOverlapSphereEntity(TargetObj.transform.position, Radius, out List<EntityInfo> entities))
        {
            isFindEntity = true;
            EntityNumber = entities.Count;
        }
        else
        {
            isFindEntity = false;
            EntityNumber = 0;
        }
    }

    private void Update()
    {
        TestFunction();
    }

    void OnDrawGizmos()
    {
        if (TargetObj != null)
        {
            // ���� Gizmos ����ɫ
            Gizmos.color = Color.green;

            // ����һ�����壬��ʾ��Χ
            Gizmos.DrawWireSphere(TargetObj.transform.position, Radius);
        }
    }
}
