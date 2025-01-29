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

        #region 状态

        [Foldout("状态机", true)]
        [Header("AI状态机")][ReadOnly] public AIState myState;

        [Foldout("AI状态", true)]
        [Header("正在攻击")][ReadOnly] public bool isAttacking;
        [Header("成功打到玩家")][ReadOnly] public bool isSucceded_HitPlayer;

        #endregion


        #region 周期函数

        MC_Velocity_Component Velocity_Component;
        MC_Collider_Component Collider_Component;
        World world;
        private void Awake()
        {
            Velocity_Component = GetComponent<MC_Velocity_Component>();
            Collider_Component = GetComponent<MC_Collider_Component>();
            world = Collider_Component.managerhub.world;
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

        public bool Toggle_Flee;
        private void Update()
        {
            _ReferUpdate_AICompetent();
            _ReferUpdate_AIAttack();

            if (Toggle_Flee)
            {
                SwitchFleeState();
                Toggle_Flee = false;
            }

        }

        #endregion


        #region AI状态机


        //状态机控制器
        Coroutine Coroutine_AIState_Controller;
        IEnumerator Corou_AIState_Controller()
        {

            while(true)
            {
                // 提前返回 - 如果没有攻击性
                // 提前返回 - 如果是创造模式
                if (isAggressive == false || world.game_mode == GameMode.Creative)
                {
                    yield return null; // 让出控制权，避免死循环
                    continue;
                }


                //从实体眼睛向玩家眼睛发送一条射线
                Vector3 _direct = Collider_Component.managerhub.player.cam.transform.position - Collider_Component.EyesPoint;
                RayCastStruct _rayCast = Collider_Component.managerhub.player.NewRayCast(Collider_Component.EyesPoint, _direct, AIseeDistance);

                // 可视化射线用于调试
                Debug.DrawRay(Collider_Component.EyesPoint, _direct.normalized * AIseeDistance, Color.red, 1f);
                print("发射射线");

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


                //等待1s
                if (myState == AIState.Idle)
                {
                    yield return new WaitForSeconds(Idle_WaitTime);
                }
                else if (myState == AIState.Chase)
                {
                    yield return new WaitForSeconds(Chase_WaitTime);
                }


                //print($"mystate = {myState}, isHit = {_rayCast.isHit}, isOutOfRange = {_rayCast.isOutOfRange}");
            }




        }

        

        IEnumerator WateToTurnBackIdleState()
        {
            yield return new WaitForSeconds(fleeTime);
            myState = AIState.Idle;
        }

        #endregion


        #region AI设置

        [Foldout("AI通用设置", true)]
        [Header("AI移动方式")] public AIMovingType myMovingType = AIMovingType.WalkType;

        [Foldout("攻击性AI", true)]
        [Header("是否具有攻击性")] public bool isAggressive = false;
        [Header("AI可视距离")] public float AIseeDistance = 20f;
        [Header("Idle状态侦察延迟")] public float Idle_WaitTime = 1f;
        [Header("Chase状态侦察延迟")] public float Chase_WaitTime = 5f;

        [Foldout("非攻击性AI", true)]
        [Header("是否会逃跑")] public bool isCanFlee = false;
        [Header("逃跑时间")] public float fleeTime = 5f;
        [Header("逃跑速度")] public float fleeSpeed = 2f;
        [Header("旋转速度")] public float RotationSpeed = 0.3f;


        #endregion


        #region AI移动

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
                        float waitTime = Random.Range(3f, 7f);
                        yield return new WaitForSeconds(waitTime);

                        //随机方向
                        Vector3 direction = Random.onUnitSphere * 0.5f; // 使用随机方向
                        direction.y = 0.9f; // 设置Y轴为jumpheight
                        Vector3 direct = direction.normalized; // 标准化向量

                        //随机力度
                        float force = Velocity_Component.force_jump;

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
                else if (myMovingType == AIMovingType.WalkType)
                {
                    yield return null;
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
            while (myState == AIState.Chase)
            {
                yield return new WaitForSeconds(0.2f);
                Vector3 playerPosition = Collider_Component.managerhub.player.transform.position;
                Vector3 direction = (playerPosition - transform.position).normalized;
                Velocity_Component.EntitySmoothRotation(direction, 0.25f); // 半秒内转向玩家
            }

            Coroutine_AlwaysLookAtPlayer = null;



        }



        #endregion


        #region AI攻击


        void _ReferUpdate_AIAttack()
        {
            //提前返回-如果不是生存模式
            if (world.game_mode != GameMode.Survival)
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


        #region AI逃跑

        //转为逃跑模式
        public void SwitchFleeState()
        {
            myState = AIState.Flee;
            StartCoroutine(WateToTurnBackIdleState());
            if (Coroutine_AIFlee == null)
                Coroutine_AIFlee = StartCoroutine(Corou_AIFlee());
        }


        Coroutine Coroutine_AIFlee;
        IEnumerator Corou_AIFlee()
        {
            float _time = 0;
            Vector3 fleeDirection = Vector3.zero;

            // 逃跑时随机选择一个逃跑方向
            fleeDirection = Random.insideUnitSphere;
            fleeDirection.y = 0; // 确保逃跑方向在水平面上
            fleeDirection.Normalize(); // 标准化方向

            // 初始时旋转朝向逃跑方向
            Velocity_Component.EntitySmoothRotation(fleeDirection, 0.7f);

            while (_time < fleeTime)
            {
                _time += Time.deltaTime;

                // 每一帧都向逃跑方向加速
                Velocity_Component.SetVelocity("x", fleeDirection.x * fleeSpeed);
                Velocity_Component.SetVelocity("z", fleeDirection.z * fleeSpeed);

                // 碰撞检测：如果周围有障碍物，尝试跳跃
                if (Collider_Component.Collider_Surround)
                {
                    Velocity_Component.EntityJump();
                }

                // 可以考虑给AI加一些随机因素，使其逃跑不那么线性
                if (Random.Range(0f, 1f) < 0.05f) // 每0.05概率改变逃跑方向
                {
                    fleeDirection = Random.insideUnitSphere;
                    fleeDirection.y = 0; // 保持水平逃跑
                    fleeDirection.Normalize();

                    // 每次改变逃跑方向时重新旋转
                    Velocity_Component.EntitySmoothRotation(fleeDirection, 0.7f);
                }

                yield return null;
            }

            // 逃跑结束后切换到Idle状态
            myState = AIState.Idle;
        }




        #endregion


        #region AI上浮

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