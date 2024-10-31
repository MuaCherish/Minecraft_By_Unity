using Homebrew;
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
    [RequireComponent(typeof(MC_Collider_Component))]
    public class MC_AI_Component : MonoBehaviour
    {
        [Foldout("����", true)]
        [Header("��ˮ�л����ϸ�")] public bool AI_CanSwiming;


        #region ���ں���

        MC_Velocity_Component Velocity_Component;
        MC_Collider_Component Collider_Component;

        private void Awake()
        {
            Velocity_Component = GetComponent<MC_Velocity_Component>();
            Collider_Component = GetComponent<MC_Collider_Component>();
        }

        private void Start()
        {
            myState = AIState.Idle;
        }

        private void Update()
        {
            UpdateStateControler();
            _ReferUpdateAICompetent();
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


        #region AI����

        public float FloatingForce = 1f;

        //�ú�����Update��
        void _ReferUpdateAICompetent()
        {
            if (AI_CanSwiming)
            {
                //���������ˮ��������һ��
                if (Collider_Component.IsInTheWater(transform.position))
                {
                    Velocity_Component.AddForce(new Vector3(0f, 1f, 0f), FloatingForce);
                }
            }
        }


        #endregion

    }
}