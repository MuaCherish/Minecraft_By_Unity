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


    #region ���ں���

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


    #region ����

    [Foldout("��������", true)]
    [Header("ӵ�����߶���")] public bool CanWalk;
    [Header("ӵ�й�������")] public bool CanAttack;
    [Header("ӵ�л�������")] public bool canAct;


    #endregion


    #region Auto-Walk

    /// <summary>
    /// ���ö����ٶ�
    /// </summary>
    /// <param name="_currentSpeed"></param>
    private float previous_speed;
    public void SetSpeed(float _currentSpeed)
    {
        animator.speed = previous_speed * _currentSpeed / Velocity_Component.speed_move;
    }


    void _ReferUpdate_AutoWalk()
    {

        //��ǰ����-���������·
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
        //��ǰ����-������ܹ���
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
