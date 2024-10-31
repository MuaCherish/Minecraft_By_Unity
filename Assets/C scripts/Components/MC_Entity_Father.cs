using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCEntity;

[RequireComponent(typeof(MC_Collider_Component))]
public abstract class MC_Entity_Father : MonoBehaviour // ��������Ϊ abstract
{
    #region ���ں���

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

    #region ���๦��

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

    protected void DestroyEntity()
    {
        if (Collider_Component.FootPoint.y <= -20f)
        {
            Destroy(this.gameObject);
        }
    }

    #endregion

    #region ���๦��

    // ���󷽷���Ҫ������ʵ��
    public abstract void OnStartEntity();
    public abstract void OnEndEntity();

    #endregion
}
