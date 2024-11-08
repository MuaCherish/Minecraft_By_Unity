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
    //private bool isHurtOnCooldown = false;  // ��ʶ����״̬�Ƿ�����ȴ��
    public float hurtCooldownDuration = 2f;  // ����״̬����ȴʱ��

    [Header("Animators")]
    public Animation _animation;


    // ��������Animator��ָ������
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
        if (GUI.Button(new Rect(0, 0, 400, 400), "�����"))
        {
            Debug.Log("OK");
            PlayAnimation();
        } 
    }


    private void Update()
    {
        // ���ݵ�ǰ״ִ̬�ж�Ӧ����Ϊ
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

    // �ı�״̬�����������״̬ʱ���߼�
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

    // �����ɫ���˵��߼�����ʵ����ȴ����
    //private IEnumerator Entity_Hurted()
    //{
    //    isHurtOnCooldown = true;
    //    Debug.Log("Steve is hurt");

    //    // ��������ʱ���߼������綯�����š�����ֵ���ٵ�

    //    yield return new WaitForSeconds(hurtCooldownDuration);

    //    // ��ȴʱ����������������������״̬
    //    isHurtOnCooldown = false;
    //    Debug.Log("Hurt cooldown ended");
    //}
}
