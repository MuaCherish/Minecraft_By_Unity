using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalData
{
    /// <summary>
    /// ��ȡ�������ManagerHub
    /// </summary>
    /// <returns></returns>
    public static ManagerHub GetManagerhub()
    {
        // �ڳ����в�����Ϊ "ManagerHub" �� GameObject
        GameObject managerhubObject = GameObject.Find("Manager/ManagerHub");

        if (managerhubObject != null)
        {
            ManagerHub managerhub = managerhubObject.GetComponent<ManagerHub>();
            return managerhub;
        }
        else
        {
            Debug.LogError("ManagerHub not found in the scene at Manager/ManagerHub");
            return null;
        }
    }

    /// <summary>
    /// ��ȡ�������Clones
    /// </summary>
    /// <returns></returns>
    public static GameObject GetClonesParent()
    {
        // �ڳ����в�����Ϊ "ManagerHub" �� GameObject
        GameObject _Object = GameObject.Find("Environment/Clones");

        if (_Object != null)
        {
            return _Object;
        }
        else
        {
            Debug.LogError("Cant Find Clones");
            return null;
        }
    }

}