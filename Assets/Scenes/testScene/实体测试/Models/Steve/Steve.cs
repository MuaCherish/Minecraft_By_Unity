using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steve : MonoBehaviour
{
    public enum EntityState
    {
        Idle,
        Moving,
        Hurt
    }

    [Header("State")]
    public EntityState currentState;
    //private bool isHurtOnCooldown = false;  // 标识受伤状态是否处于冷却中
    public float hurtCooldownDuration = 2f;  // 受伤状态的冷却时间

    [Header("Animators")]
    public Animation _animation;


    // 播放所有Animator的指定动画
    private void PlayAnimation()
    {
        if (_animation != null)
        {
            if (_animation.Play("LeftHandMoving"))
            {
                print("Playing LeftHandMoving");
            }
            else
            {
                print("Failed to play the animation. Make sure the animation name is correct.");
            }
        } 
        else
        {
            Debug.LogError("Animation component is missing.");
        }
    }


    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 400, 400), "点这里！"))
        {
            Debug.Log("OK");
            PlayAnimation();
        } 
    }


    private void Update()
    {
        // 根据当前状态执行对应的行为
        //switch (currentState)
        //{
        //    case EntityState.Moving:
        //        Entity_Moving();
        //        break;

        //    case EntityState.Idle:
        //        Entity_StopMoving();
        //        break;

        //    case EntityState.Hurt:
        //        if (!isHurtOnCooldown)
        //        {
        //            StartCoroutine(Entity_Hurted());
        //        }
        //        break;
        //}
    }

    // 改变状态并处理进入新状态时的逻辑
    public void ChangeState(EntityState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (currentState)
        {
            case EntityState.Moving:
                OnEnterMoving();
                break;

            case EntityState.Idle:
                OnEnterIdle();
                break;

            case EntityState.Hurt:
                OnEnterHurt();
                break;
        }
    }

    private void OnEnterMoving()
    {
       //Debug.Log("Entering Moving State");
    }

    private void OnEnterIdle()
    {
        //Debug.Log("Entering Idle State");
    }

    private void OnEnterHurt()
    {
        //Debug.Log("Entering Hurt State");
    }

    public void Entity_Moving()
    {
        //Debug.Log("Steve is moving");
    }

    public void Entity_StopMoving()
    {
        //Debug.Log("Steve stopped moving");
    }

    // 处理角色受伤的逻辑，并实现冷却机制
    //private IEnumerator Entity_Hurted()
    //{
    //    isHurtOnCooldown = true;
    //    Debug.Log("Steve is hurt");

    //    // 处理受伤时的逻辑，例如动画播放、生命值减少等

    //    yield return new WaitForSeconds(hurtCooldownDuration);

    //    // 冷却时间结束，重新允许进入受伤状态
    //    isHurtOnCooldown = false;
    //    Debug.Log("Hurt cooldown ended");
    //}
}
