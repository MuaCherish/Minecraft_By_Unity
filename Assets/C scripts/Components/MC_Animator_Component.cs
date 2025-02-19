using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MC_Collider_Component))]
[RequireComponent(typeof(MC_Velocity_Component))]
public class MC_Animator_Component : MonoBehaviour
{


    #region 周期函数

    MC_Collider_Component Collider_Component;
    MC_Velocity_Component Velocity_Component;
    Animator animator;
    World world;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        Collider_Component = GetComponent<MC_Collider_Component>();
        world = Collider_Component.managerhub.world;
        Velocity_Component = GetComponent<MC_Velocity_Component>();
    }

    private void Start()
    {
        previous_speed = animator.speed;
    }

    void Update()
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

        _ReferUpdate_AutoWalk();

    }


    #endregion


    #region 设置

    [Foldout("动画设置", true)]
    [Header("拥有行走动画")] public bool CanWalk;
    [Header("拥有攻击动画")] public bool CanAttack;
    [Header("拥有互动动画")] public bool canAct;


    #endregion


    #region Auto-Walk

    /// <summary>
    /// 设置动画速度
    /// </summary>
    /// <param name="_currentSpeed"></param>
    private float previous_speed;
    public void SetSpeed(float _currentSpeed)
    {
        animator.speed = previous_speed * _currentSpeed / Velocity_Component.speed_move;
    }


    void _ReferUpdate_AutoWalk()
    {

        //提前返回-如果可以走路
        if (!CanWalk)
            return;

        if (Velocity_Component.isMoving)
        {
            animator.SetBool("isWalk", true);
        }
        else
        {
            animator.SetBool("isWalk", false);
        }
    }

    #endregion


    #region Attack

    public void PlayAttackAnimation()
    {
        //提前返回-如果不能攻击
        if (!CanAttack)
            return;

        animator.SetTrigger("isAttack");
    }


    #endregion


    #region Act

    public void SetAnimation_Act()
    {
        animator.SetTrigger("isAct");
    }

    #endregion


}
