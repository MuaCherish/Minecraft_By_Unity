using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MCEntity
{
    [SerializeField]
    public enum AIState
    {
        Idle,        //漫无目的的闲逛
        Chase,
        Attack,
        Flee,        
    }

    [RequireComponent(typeof(MC_Velocity_Component))]
    public class MC_AI_Component : MonoBehaviour
    {

        #region 周期函数

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


        #region AI状态机

        [Header("AI状态机")] public AIState myState;


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


        #region AI状态处理

        //Idle
        private void HandleIdleState()
        {
            // 实现闲逛逻辑
            // 可以选择随机移动或者等待
            // 例如，闲逛一定时间后随机改变状态
            //if (Random.Range(0f, 1f) < 0.01f) // 1% 概率发现目标
            //{
            //    myState = AIState.发现目标; // 切换到发现目标状态
            //}
        }

        //Chase
        private void HandleChaseState()
        {
            // 处理发现目标的逻辑
            // 例如，获取目标位置并切换状态
            // 假设你有一个方法来检测目标
            //GameObject target = DetectTarget();
            //if (target != null)
            //{
            //    MoveToObject(target); // 移动到目标
            //    myState = AIState.移动到目标; // 切换到移动到目标状态
            //}
        }

        //Flee
        private void HandleAttackState()
        {
            // 如果到达目标位置，切换状态
            //if (Vector3.Distance(transform.position, _targetPos_MoveTo) < 0.1f)
            //{
            //    myState = AIState.攻击目标; // 到达目标后切换到攻击目标状态
            //}
        } 

        //Flee
        private void HandleFleeState()
        {
            // 如果到达目标位置，切换状态
            //if (Vector3.Distance(transform.position, _targetPos_MoveTo) < 0.1f)
            //{
            //    myState = AIState.攻击目标; // 到达目标后切换到攻击目标状态
            //}
        }


        #endregion




    }
}