using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class SceneData
{
    /// <summary>
    /// 获取场景里的ManagerHub
    /// </summary>
    /// <returns></returns>
    public static ManagerHub GetManagerhub()
    {
        // 在场景中查找名为 "ManagerHub" 的 GameObject
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
    /// 获取场景里的Clones
    /// </summary>
    /// <returns></returns>
    public static GameObject GetClonesParent()
    {
        // 在场景中查找名为 "ManagerHub" 的 GameObject
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
    /// 获取场景里的粒子存放点
    /// </summary>
    /// <returns></returns>
    public static GameObject GetParticleParent()
    {
        // 在场景中查找名为 "ManagerHub" 的 GameObject
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

    //获取场景中的掉落物存放点
    public static GameObject GetDropBlockParent()
    {
        // 在场景中查找名为 "ManagerHub" 的 GameObject
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
    /// 获取场景中的实体存放点
    /// </summary>
    /// <returns></returns>
    public static GameObject GetEntityParent()
    {
        // 在场景中查找名为 "ManagerHub" 的 GameObject
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
    /// 获取场景中的Chunk存放点
    /// </summary>
    /// <returns></returns>
    public static GameObject GetChunkParent()
    {
        // 在场景中查找名为 "ManagerHub" 的 GameObject
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
    /// 获取ServiceWorld
    /// </summary>
    /// <returns></returns>
    public static MC_Service_World GetService_World()
    {
        // 在场景中查找名为 "ManagerHub" 的 GameObject
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