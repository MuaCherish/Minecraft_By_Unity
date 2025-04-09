using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        GameObject managerhubObject = GameObject.Find("Managerhub");

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
        GameObject _Object = GameObject.Find("Environment/Temps/Clones");

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
        GameObject _obj = GameObject.Find("Environment/Temps/Particles");

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

    //��ȡ�����еĵ������ŵ�
    public static GameObject GetDropBlockParent()
    {
        // �ڳ����в�����Ϊ "ManagerHub" �� GameObject
        GameObject _obj = GameObject.Find("Environment/Temps/DropBlocks");

        if (_obj != null)
        {
            return _obj;
        }
        else
        {
            Debug.LogError("Cant Find DropBlocks");
            return null;
        }
    }

    /// <summary>
    /// ��ȡ�����е�ʵ���ŵ�
    /// </summary>
    /// <returns></returns>
    public static GameObject GetEntityParent()
    {
        // �ڳ����в�����Ϊ "ManagerHub" �� GameObject
        GameObject _obj = GameObject.Find("Environment/Temps/Entity");

        if (_obj != null)
        {
            return _obj;
        }
        else
        {
            Debug.LogError("Cant Find Entity");
            return null;
        }
    }

    /// <summary>
    /// ��ȡ�����е�Chunk��ŵ�
    /// </summary>
    /// <returns></returns>
    public static GameObject GetChunkParent()
    {
        // �ڳ����в�����Ϊ "ManagerHub" �� GameObject
        GameObject _obj = GameObject.Find("Environment/Temps/Chunks");

        if (_obj != null)
        {
            return _obj;
        }
        else
        {
            Debug.LogError("Cant Find Chunks");
            return null;
        }
    }


    /// <summary>
    /// ��ȡServiceWorld
    /// </summary>
    /// <returns></returns>
    public static MC_Service_World GetService_World()
    {
        // �ڳ����в�����Ϊ "ManagerHub" �� GameObject
        GameObject _obj = GameObject.Find("Managerhub/MC_Services/MC_Service_World");

        if (_obj != null)
        {
            return _obj.GetComponent<MC_Service_World>();
        }
        else
        {
            Debug.LogError("SceneData Cant Find");
            return null;
        }
    }

}