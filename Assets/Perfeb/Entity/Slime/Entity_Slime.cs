using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_Slime : MonoBehaviour
{

    #region 状态

    

    #endregion


    #region 周期函数

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


    #region 落地会弹一下

    [Header("弹跳力度")] public float elasticity = 10.0f;
    [Header("高度阈值")] public float disY = 1f;
    private float maxY = -Mathf.Infinity;
    private bool hasExec_GroundJump;
    void _ReferUpdate_BoundWithGround()
    {

        //弹跳检测
        if (Collider_Component.isGround)
        {
            float _dis = maxY - Collider_Component.FootPoint.y;
            if (hasExec_GroundJump && _dis > disY)
            {
                Velocity_Component.AddForce(new Vector3(0f, 1f, 0f), elasticity);
                hasExec_GroundJump = false;
            }

            //最大高度
            maxY = Collider_Component.FootPoint.y;
        }
        else
        {
            //最大高度
            if (Collider_Component.FootPoint.y > maxY)
                maxY = Collider_Component.FootPoint.y;

            hasExec_GroundJump = true;
        }

        //最大高度
        if (Collider_Component.IsInTheWater(Collider_Component.EyesPoint))
        {
            maxY = Collider_Component.FootPoint.y;
        }

    }

    #endregion
}
