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
        Idle,       
        Chase,
        Flee,
    }

    [SerializeField]
    public enum AIMovingType
    {
        JumpType,
        WalkType,
        FlyType,
    }

    [RequireComponent(typeof(MC_Velocity_Component))]
    [RequireComponent(typeof(MC_Collider_Component))]
    public class MC_AI_Component : MonoBehaviour
    {

        #region 周期函数

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

            if (Coroutine_AIState_Controller == null)
            {
                Coroutine_AIState_Controller = StartCoroutine(Corou_AIState_Controller());
            }

            if (Coroutine_AIMoving == null)
            {
                Coroutine_AIMoving = StartCoroutine(Corou_AIMoving());
            }
        }

        private void Update()
        {
            _ReferUpdate_AICompetent();
            _ReferUpdate_AIAttack();
        }

        #endregion


        #region AI状态机

        [Foldout("状态机", true)]
        [Header("AI状态机")][ReadOnly] public AIState myState;

        [Foldout("AI状态", true)]
        [Header("正在攻击")][ReadOnly] public bool isAttacking;
        [Header("成功打到玩家")][ReadOnly] public bool isSucceded_HitPlayer;


        Coroutine Coroutine_AIState_Controller;
        IEnumerator Corou_AIState_Controller()
        {

            while(true)
            {
                //提前返回-如果没有攻击性
                if (!isAggressive)
                    yield return null;

                //等待1s
                if (myState == AIState.Idle)
                {
                    yield return new WaitForSeconds(Idle_WaitTime);
                }
                else if (myState == AIState.Chase)
                {
                    yield return new WaitForSeconds(Chase_WaitTime);
                }


                //从实体眼睛向玩家眼睛发送一条射线
                Vector3 _direct = Collider_Component.managerhub.player.cam.transform.position - Collider_Component.EyesPoint;
                RayCastStruct _rayCast = Collider_Component.managerhub.player.NewRayCast(Collider_Component.EyesPoint, _direct, AIseeDistance);

                //如果之间没有方块
                if (_rayCast.isHit == 1 || _rayCast.isHit == 2)
                {
                    //Idle
                    myState = AIState.Idle;
                }
                else if((Collider_Component.managerhub.player.cam.transform.position - Collider_Component.EyesPoint).magnitude <= AIseeDistance)
                {
                    //Chase
                    myState = AIState.Chase;

                }

                //print($"mystate = {myState}, isHit = {_rayCast.isHit}, isOutOfRange = {_rayCast.isOutOfRange}");
            }




        }


        #endregion


        #region AI设置

        [Foldout("AI设置", true)]
        [Header("AI移动方式")] public AIMovingType myMovingType = AIMovingType.WalkType;
        [Header("是否具有攻击性")] public bool isAggressive = false;
        [Header("AI可视距离")] public float AIseeDistance = 20f;
        [Header("Idle状态侦察延迟")] public float Idle_WaitTime = 1f;
        [Header("Chase状态侦察延迟")] public float Chase_WaitTime = 5f;



        #endregion


        #region AI攻击

        
        void _ReferUpdate_AIAttack()
        {
            //提前返回-如果不是生存模式
            if (Collider_Component.managerhub.world.game_mode != GameMode.Survival)
                return;

            //如果AI距离玩家低于一定范围，且正在发出攻击时
            //时刻检测实体与玩家的碰撞盒，并予以玩家伤害
            Player player = Collider_Component.managerhub.player;
            float _dis = (transform.position - player.transform.position).magnitude;
            float _maxDis = Mathf.Abs(Collider_Component.hitBoxWidth - player.playerWidth);
            if (_dis < _maxDis && isAttacking)
            {
                Vector3 hitVec = player.CheckHitBox(transform.position, Collider_Component.hitBoxWidth, Collider_Component.hitBoxHeight);
                if (hitVec != Vector3.zero && !isSucceded_HitPlayer)
                {
                    StartCoroutine(Corou_WaitForSecond("isSucceded_HitPlayer", 1f));
                    //print("成功打到玩家");
                    hitVec.y = 1f;
                    player.ForceMoving(hitVec, 2.5f, 0.2f);
                    Collider_Component.managerhub.lifeManager.UpdatePlayerBlood(6, true, true);
                }
            }
        }

        #endregion


        #region AI移动方式

        Coroutine Coroutine_AIMoving;

        IEnumerator Corou_AIMoving()
        {


            while (true)
            {
                if (myMovingType == AIMovingType.JumpType)
                {
                    if (myState == AIState.Idle)
                    {
                        //延迟若干秒
                        float waitTime = Random.Range(3f,7f);
                        yield return new WaitForSeconds(waitTime);

                        //随机方向
                        Vector3 direction = Random.onUnitSphere * 0.5f; // 使用随机方向
                        direction.y = 0.9f; // 设置Y轴为jumpheight
                        Vector3 direct = direction.normalized; // 标准化向量

                        //随机力度
                        float force = Random.Range(100, 150);

                        //转向跳跃方向
                        Velocity_Component.EntitySmoothRotation(direct, 0.7f);
                        yield return new WaitForSeconds(0.7f);

                        //跳跃
                        Velocity_Component.AddForce(direct, force);

                        

                    }
                    else if (myState == AIState.Chase)
                    {

                        if (Coroutine_AlwaysLookAtPlayer == null)
                        {
                            Coroutine_AlwaysLookAtPlayer = StartCoroutine(Corou_AlwaysLookAtPlayer());
                        }

                        // 延迟若干秒（模拟跳跃后的冷却时间）
                        float waitTime = Random.Range(2f, 4f);
                        yield return new WaitForSeconds(waitTime);

                        // 获取玩家位置
                        Vector3 playerPosition = Collider_Component.managerhub.player.transform.position;

                        // 计算方向：朝向玩家
                        Vector3 direction = (playerPosition - transform.position).normalized;
                        direction.y = 0.9f; // 设置Y轴为jumpheight，确保跳跃方向具有一定高度

                        // 固定力度
                        float force = Random.Range(100, 150);

                        // 跳跃
                        StartCoroutine(Corou_WaitForSecond("isAttacking", 1f));
                        Velocity_Component.AddForce(direction, force);

                    }
                }

            }



        }


        IEnumerator Corou_WaitForSecond(string state, float _Time)
        {
            switch (state)
            {
                case "isAttacking":
                    isAttacking = true;
                    break;
                case "isSucceded_HitPlayer":
                    isSucceded_HitPlayer = true;
                    break;
            }

            yield return new WaitForSeconds(_Time);


            switch (state)
            {
                case "isAttacking":
                    isAttacking = false;
                    break;
                case "isSucceded_HitPlayer":
                    isSucceded_HitPlayer = false;
                    break;
            }
        }


        Coroutine Coroutine_AlwaysLookAtPlayer;
        IEnumerator Corou_AlwaysLookAtPlayer()
        {
            while(myState == AIState.Chase)
            {
                yield return new WaitForSeconds(0.2f);
                Vector3 playerPosition = Collider_Component.managerhub.player.transform.position;
                Vector3 direction = (playerPosition - transform.position).normalized;
                Velocity_Component.EntitySmoothRotation(direction, 0.25f); // 半秒内转向玩家
            }

            Coroutine_AlwaysLookAtPlayer = null;



        }



        #endregion


        #region 上浮能力

        [Foldout("上浮能力", true)]
        [Header("在水中会向上浮")] public bool AI_CanSwiming;
        [Header("上浮力")] public float FloatingForce = 1f;


        //该函数在Update中
        void _ReferUpdate_AICompetent()
        {
            if (AI_CanSwiming)
            {
                //如果中心入水，则尝试跳一下
                if (Collider_Component.IsInTheWater(transform.position))
                {
                    Velocity_Component.AddForce(Vector3.up, FloatingForce);
                }
            }
        }


        #endregion

    }
}