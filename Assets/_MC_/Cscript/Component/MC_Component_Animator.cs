using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MC_Component_Physics))]
[RequireComponent(typeof(MC_Component_Velocity))]
public class MC_Component_Animator : MonoBehaviour
{


    #region ���ں���

    MC_Component_Physics Component_Physics;
    MC_Component_Velocity Component_Velocity;
    Animator animator;
    MC_Service_World Service_World;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        Component_Physics = GetComponent<MC_Component_Physics>();
        Service_World = Component_Physics.managerhub.Service_World;
        Component_Velocity = GetComponent<MC_Component_Velocity>();
    }

    private void Start()
    {
        previous_speed = animator.speed;
    }

    void Update()
    {
        switch (MC_Runtime_DynamicData.instance.GetGameState())
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
        animator.speed = previous_speed * _currentSpeed / Component_Velocity.speed_move;
    }


    void _ReferUpdate_AutoWalk()
    {

        //��ǰ����-���������·
        if (!CanWalk)
            return;

        if (Component_Velocity.isMoving)
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
