using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SceneData
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
    /// ��ȡ�������World
    /// </summary>
    /// <returns></returns>
    public static World GetWorld()
    {
        // �ڳ����в�����Ϊ "ManagerHub" �� GameObject
        GameObject worldObject = GameObject.Find("Manager/World Manager");

        if (worldObject != null)
        {
            World world = worldObject.GetComponent<World>();
            return world;
        }
        else
        {
            Debug.LogError("World Manager�Ҳ���");
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

    /// <summary>
    /// ��ȡ����������Ӵ�ŵ�
    /// </summary>
    /// <returns></returns>
    public static GameObject GetParticleParent()
    {
        // �ڳ����в�����Ϊ "ManagerHub" �� GameObject
        GameObject _obj = GameObject.Find("Environment/Particles");

        if (_obj != null)
        {
            return _obj;
        }
        else
        {
            Debug.LogError("Cant Find Particles");
            return null;
        }
    }

}