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

    #region 状态

    [Foldout("动画状态", true)]
    [Header("走路")] public bool isWalk;
    [Header("奔跑")] public bool isRun;
    [Header("攻击")] public bool isAttack;
    [Header("死亡")] public bool isDead;


    #endregion


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

        _ReferUpdate_ActiveAnimation();
        _ReferUpdate_PassiveAnimation();

    }


    #endregion


    #region 被动动画

    void _ReferUpdate_PassiveAnimation()
    {
        //isWalk
        if (isWalk && HasAttackParam("isWalk"))
            animator.SetBool("isWalk", true);
        else
            animator.SetBool("isWalk", false);

        //isRun
        if (isRun && HasAttackParam("isRun"))
            animator.SetBool("isRun", true);
        else
            animator.SetBool("isRun", false);

        //isAttack
        if (isAttack && HasAttackParam("isAttack"))
            animator.SetBool("isAttack", true);
        else
            animator.SetBool("isAttack", false);

        //isDead
        if (isDead && HasAttackParam("isDead"))
            animator.SetBool("isDead", true);


    }

    #endregion


    #region 主动动画

    void _ReferUpdate_ActiveAnimation()
    {
        if (Velocity_Component.isMoving)
        {
            isWalk = true;
        }
        else
        {
            isWalk = false;
        }
    }

    #endregion


    #region Tool

    bool HasAttackParam(string _para)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == _para && param.type == AnimatorControllerParameterType.Bool)
            {
                return true; // 找到名为 isAttack 的布尔类型参数
            }
        }
        return false; // 没有找到
    }


    #endregion


}
