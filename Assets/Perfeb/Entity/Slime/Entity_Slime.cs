using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_Slime : MonoBehaviour
{

    #region ״̬

    

    #endregion


    #region ���ں���

    MC_Collider_Component Collider_Component;
    MC_Velocity_Component Velocity_Component;
    World world;
    private void Awake()
    {
        Collider_Component = GetComponent<MC_Collider_Component>();
        Velocity_Component = GetComponent<MC_Velocity_Component>();
        world = Collider_Component.managerhub.world;
    }

    private void Update()
    {
        switch (world.game_state)
        {
            case Game_State.Playing:
                Handle_GameState_Playing();
                break;
        }
    }

    void Handle_GameState_Playing()
    {
        _ReferUpdate_BoundWithGround();
    }

    #endregion


    #region ��ػᵯһ��

    [Header("��������")] public float elasticity = 10.0f;
    [Header("�߶���ֵ")] public float disY = 1f;
    private float maxY = -Mathf.Infinity;
    private bool hasExec_GroundJump;
    void _ReferUpdate_BoundWithGround()
    {

        //�������
        if (Collider_Component.isGround)
        {
            float _dis = maxY - Collider_Component.FootPoint.y;
            if (hasExec_GroundJump && _dis > disY)
            {
                Velocity_Component.AddForce(new Vector3(0f, 1f, 0f), elasticity);
                hasExec_GroundJump = false;
            }

            //���߶�
            maxY = Collider_Component.FootPoint.y;
        }
        else
        {
            //���߶�
            if (Collider_Component.FootPoint.y > maxY)
                maxY = Collider_Component.FootPoint.y;

            hasExec_GroundJump = true;
        }

        //���߶�
        if (Collider_Component.IsInTheWater(Collider_Component.EyesPoint))
        {
            maxY = Collider_Component.FootPoint.y;
        }

    }

    #endregion
}
