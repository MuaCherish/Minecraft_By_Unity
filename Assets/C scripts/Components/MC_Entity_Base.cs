using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCEntity;

[RequireComponent(typeof(MC_Collider_Component))]
public abstract class MC_Entity_Base : MonoBehaviour // ��������Ϊ abstract
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

    // �����ʱ�䣨��λ���룩
    protected float checkInterval = 5f;
    protected float lastCheckTime = -5f; // ��ʼ��Ϊ��ֵ��ȷ���״μ��

    protected void DestroyEntity()
    {
        // ���Y������������������
        if (Collider_Component.FootPoint.y <= -20f)
        {
            Destroy(this.gameObject);
            return;
        }

        // ÿ�� checkInterval ����һ�� Chunk ����ʾ״̬
        if (Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time; // �����ϴμ���ʱ��

            // ������������ʱ����
            if (managerhub.world.GetChunkObject(Collider_Component.FootPoint).isShow == false)
            {
                Destroy(this.gameObject);
            }
        }
    }


    #endregion

    #region ���๦��

    // ���󷽷���Ҫ������ʵ��
    public abstract void OnStartEntity();
    public abstract void OnEndEntity();

    #endregion
}
