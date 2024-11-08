using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCEntity;

[RequireComponent(typeof(MC_Collider_Component))]
public abstract class MC_Entity_Base : MonoBehaviour // 将类声明为 abstract
{
    #region 周期函数

    protected ManagerHub managerhub;
    protected MC_Collider_Component Collider_Component;
    protected MC_Velocity_Component Velocity_Component;

    protected virtual void Update()
    {
        //FindComponents();

        if (managerhub.world.game_state == Game_State.Playing)
        {
            DestroyEntity();
        }
    }

    #endregion

    #region 父类功能

    protected void FindComponents()
    {
        if (managerhub == null ||
            Collider_Component == null ||
            Velocity_Component == null
            )
        {
            managerhub = GlobalData.GetManagerhub();
            Collider_Component = GetComponent<MC_Collider_Component>();
            Velocity_Component = GetComponent<MC_Velocity_Component>();
        }
    }

    // 检查间隔时间（单位：秒）
    protected float checkInterval = 5f;
    protected float lastCheckTime = -5f; // 初始化为负值以确保首次检测

    protected void DestroyEntity()
    {
        // 检查Y坐标条件，立即销毁
        if (Collider_Component.FootPoint.y <= -20f)
        {
            Destroy(this.gameObject);
            return;
        }

        // 每隔 checkInterval 秒检查一次 Chunk 的显示状态
        if (Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time; // 更新上次检查的时间

            // 仅在满足条件时销毁
            if (managerhub.world.GetChunkObject(Collider_Component.FootPoint).isShow == false)
            {
                Destroy(this.gameObject);
            }
        }
    }


    #endregion

    #region 子类功能

    // 抽象方法，要求子类实现
    public abstract void OnStartEntity();
    public abstract void OnEndEntity();

    #endregion
}
