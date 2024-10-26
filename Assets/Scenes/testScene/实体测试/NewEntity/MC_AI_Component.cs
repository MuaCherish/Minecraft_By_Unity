using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MCEntity
{
    [SerializeField]
    public enum AIState
    {
        Idle,        //����Ŀ�ĵ��й�
        Chase,
        Attack,
        Flee,        
    }

    [RequireComponent(typeof(MC_Velocity_Component))]
    public class MC_AI_Component : MonoBehaviour
    {

        #region ���ں���

        MC_Velocity_Component Velocity_Component;

        private void Awake()
        {
            Velocity_Component = GetComponent<MC_Velocity_Component>();
        }

        private void Start()
        {
            myState = AIState.Idle; 
        }

        private void Update()
        {
            UpdateStateControler();
        }

        #endregion


        #region AI״̬��

        [Header("AI״̬��")] public AIState myState;


        private void UpdateStateControler()
        {
            switch (myState)
            {
                case AIState.Idle:
                    HandleIdleState();
                    break;
                case AIState.Chase:
                    HandleChaseState();
                    break;
                case AIState.Attack:
                    HandleAttackState();
                    break;
                case AIState.Flee:
                    HandleFleeState();
                    break;
            }
        }


        #endregion


        #region AI״̬����

        //Idle
        private void HandleIdleState()
        {
            // ʵ���й��߼�
            // ����ѡ������ƶ����ߵȴ�
            // ���磬�й�һ��ʱ�������ı�״̬
            //if (Random.Range(0f, 1f) < 0.01f) // 1% ���ʷ���Ŀ��
            //{
            //    myState = AIState.����Ŀ��; // �л�������Ŀ��״̬
            //}
        }

        //Chase
        private void HandleChaseState()
        {
            // ������Ŀ����߼�
            // ���磬��ȡĿ��λ�ò��л�״̬
            // ��������һ�����������Ŀ��
            //GameObject target = DetectTarget();
            //if (target != null)
            //{
            //    MoveToObject(target); // �ƶ���Ŀ��
            //    myState = AIState.�ƶ���Ŀ��; // �л����ƶ���Ŀ��״̬
            //}
        }

        //Flee
        private void HandleAttackState()
        {
            // �������Ŀ��λ�ã��л�״̬
            //if (Vector3.Distance(transform.position, _targetPos_MoveTo) < 0.1f)
            //{
            //    myState = AIState.����Ŀ��; // ����Ŀ����л�������Ŀ��״̬
            //}
        } 

        //Flee
        private void HandleFleeState()
        {
            // �������Ŀ��λ�ã��л�״̬
            //if (Vector3.Distance(transform.position, _targetPos_MoveTo) < 0.1f)
            //{
            //    myState = AIState.����Ŀ��; // ����Ŀ����л�������Ŀ��״̬
            //}
        }


        #endregion




    }
}