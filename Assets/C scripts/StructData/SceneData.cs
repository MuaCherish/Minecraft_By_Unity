using System.Collections;
using System.Collections.Generic;
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
    /// 获取场景里的World
    /// </summary>
    /// <returns></returns>
    public static World GetWorld()
    {
        // 在场景中查找名为 "ManagerHub" 的 GameObject
        GameObject worldObject = GameObject.Find("Manager/World Manager");

        if (worldObject != null)
        {
            World world = worldObject.GetComponent<World>();
            return world;
        }
        else
        {
            Debug.LogError("World Manager找不到");
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
    /// 获取场景里的粒子存放点
    /// </summary>
    /// <returns></returns>
    public static GameObject GetParticleParent()
    {
        // 在场景中查找名为 "ManagerHub" 的 GameObject
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