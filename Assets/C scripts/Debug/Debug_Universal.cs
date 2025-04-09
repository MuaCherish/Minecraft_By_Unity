using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;  // 引入 Stopwatch 类
using Homebrew;

public class Debug_Universal : MonoBehaviour
{
    public MC_Service_World Service_World;
    public MC_Service_Entity Service_Entity;
    public GameObject TargetObj;
    public float Radius;

    [Foldout("测试结果", true)]
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
            // 设置 Gizmos 的颜色
            Gizmos.color = Color.green;

            // 绘制一个球体，表示范围
            Gizmos.DrawWireSphere(TargetObj.transform.position, Radius);
        }
    }
}
