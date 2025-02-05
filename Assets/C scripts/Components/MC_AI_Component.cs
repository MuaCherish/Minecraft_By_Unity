using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;


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
    [RequireComponent(typeof(MC_Life_Component))]
    [RequireComponent(typeof(MC_Animator_Component))]
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
        MC_Registration_Component Registration_Component;
        MC_Life_Component Life_Component;
        World world;
        Player player;
        MC_Animator_Component Animator_Component;

        private void Awake()
        {
            Velocity_Component = GetComponent<MC_Velocity_Component>();
            Collider_Component = GetComponent<MC_Collider_Component>();
            world = Collider_Component.managerhub.world;
            player = Collider_Component.managerhub.player;
            Registration_Component = GetComponent<MC_Registration_Component>();
            Life_Component = GetComponent<MC_Life_Component>();
            Animator_Component = GetComponent<MC_Animator_Component>();
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

            switch (world.game_state)
            {
                case Game_State.Playing:
                    Handle_GameState_Playing();
                    break;
            }


        }

        void Handle_GameState_Playing()
        {
            _ReferUpdate_AICompetent();
            _ReferUpdate_AIAttack();
            _ReferUpdate_Debug_ShowEyesRayCast();
            _ReferUpdate_AutoJump();
        }


        #endregion


        #region AI状态机


        //状态机控制器
        Coroutine Coroutine_AIState_Controller;
        IEnumerator Corou_AIState_Controller()
        {

            while (true)
            {
                // 提前返回 - 如果没有攻击性
                // 提前返回 - 如果是创造模式
                if (isAggressive == false || world.game_mode == GameMode.Creative)
                {
                    yield return null; // 让出控制权，避免死循环
                    continue;
                }

                //从实体眼睛向玩家眼睛发送一条射线
                Vector3 _direct = player.cam.transform.position - Collider_Component.EyesPoint;
                RayCastStruct _rayCast = player.NewRayCast(Collider_Component.EyesPoint, _direct, AIseeDistance, Registration_Component.EntityID);

                //print(_rayCast.rayDistance);

                //如果之间没有方块且在可视范围内 则起追逐
                if (_rayCast.isHit == 0 && 
                    (player.cam.transform.position - Collider_Component.EyesPoint).magnitude <= AIseeDistance &&
                    player.isDead == false
                    )
                {
                    //Chase
                    myState = AIState.Chase;
                }
                else 
                {
                    //Idle
                    myState = AIState.Idle;
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
        [Header("逃跑速度")] public float fleeSpeed = 5f;
        [Header("旋转速度")] public float RotationSpeed = 0.3f;


        #endregion


        #region AI移动

        Coroutine Coroutine_AIMoving;

        //自动跳跃
        void _ReferUpdate_AutoJump()
        {
            if (myMovingType == AIMovingType.WalkType && Collider_Component.Collider_Surround)
            {
               

                if (Random.Range(0f, 1f) < 0.01f) // 每0.05概率改变逃跑方向
                {
                    Velocity_Component.EntityJump();

                }
            }

           
        }

        IEnumerator Corou_AIMoving()
        {


            while (true)
            {
                if (Debug_PauseAI)
                {
                    yield return null;
                    continue;
                }

                //史莱姆
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
                        if (!Life_Component.isEntity_Dead)
                        {
                            Velocity_Component.EntitySmoothRotation(direct, 0.7f);
                            yield return new WaitForSeconds(0.7f);
                        }


                        //跳跃
                        if (!Life_Component.isEntity_Dead)
                        {
                            Velocity_Component.AddForce(direct, force);
                        }
                            



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
                        Vector3 playerPosition = player.transform.position;

                        // 计算方向：朝向玩家
                        Vector3 direction = (playerPosition - transform.position).normalized;
                        direction.y = 0.9f; // 设置Y轴为jumpheight，确保跳跃方向具有一定高度

                        // 固定力度
                        float force = Random.Range(100, 150);

                        // 跳跃
                        if (!Life_Component.isEntity_Dead)
                        {
                            StartCoroutine(Corou_WaitForSecond("isAttacking", 1f));
                            Velocity_Component.AddForce(direction, force);
                        }
                            

                    }
                }
                //猪
                else if (myMovingType == AIMovingType.WalkType)
                {
                    // 延迟若干秒
                    float waitTime = Random.Range(3f, 7f);
                    yield return new WaitForSeconds(waitTime);

                    // 随机方向
                    Vector3 direction = Random.onUnitSphere; // 使用随机方向
                    direction.y = 0f; // 确保不会向上或向下移动
                    Vector3 direct = direction.normalized; // 标准化向量

                    if (!Life_Component.isEntity_Dead)
                    {
                        Velocity_Component.EntitySmoothRotation(direct, 0.7f);
                        yield return new WaitForSeconds(0.7f);
                    }

                    // 随机移动时间
                    float walkTime = Random.Range(5f, 15f);
                    float elapsedTime = 0f;

                    // 向该方向移动规定时间
                    if (!Life_Component.isEntity_Dead)
                    {
                        while (elapsedTime < walkTime)
                        {
                            Velocity_Component.SetVelocity("x", direct.x * Velocity_Component.speed_move);
                            Velocity_Component.SetVelocity("z", direct.z * Velocity_Component.speed_move);
                            elapsedTime += Time.deltaTime;
                            yield return null;
                        }
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
            while (myState == AIState.Chase)
            {
                yield return new WaitForSeconds(0.2f);
                Vector3 playerPosition = player.transform.position;
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

            //提前返回-暂停AI活动
            if (Debug_PauseAI)
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
            //提前返回-如果暂停Ai活动
            if (Debug_PauseAI)
            {
                return;
            }
            //print("逃跑");
            myState = AIState.Flee;
            
            if (Coroutine_AIFlee == null)
                Coroutine_AIFlee = StartCoroutine(Corou_AIFlee());
        }


        Coroutine Coroutine_AIFlee;
        IEnumerator Corou_AIFlee()
        {
            StartCoroutine(WateToTurnBackIdleState());
            Animator_Component.isRun = true;
            float _time = 0;
            Vector3 fleeDirection = Vector3.zero;

            // 逃跑时随机选择一个逃跑方向
            fleeDirection = Random.insideUnitSphere;
            fleeDirection.y = 0; // 确保逃跑方向在水平面上
            fleeDirection.Normalize(); // 标准化方向

            // 初始时旋转朝向逃跑方向
            Velocity_Component.EntitySmoothRotation(fleeDirection, 0.3f);

            while (_time < fleeTime)
            {
                _time += Time.deltaTime;

                // 每一帧都向逃跑方向加速
                Velocity_Component.SetVelocity("x", fleeDirection.x * fleeSpeed * 1.5f);
                Velocity_Component.SetVelocity("z", fleeDirection.z * fleeSpeed * 1.5f);

                // 可以考虑给AI加一些随机因素，使其逃跑不那么线性
                if (Random.Range(0f, 1f) < 0.001f) // 每0.05概率改变逃跑方向
                {
                    fleeDirection = Random.insideUnitSphere;
                    fleeDirection.y = 0; // 保持水平逃跑
                    fleeDirection.Normalize();

                    // 每次改变逃跑方向时重新旋转
                    Velocity_Component.EntitySmoothRotation(fleeDirection, 0.3f);
                }

                yield return null;
            }

            // 逃跑结束后切换到Idle状态
            Animator_Component.isRun = false;
            myState = AIState.Idle;
            Coroutine_AIFlee = null;
        }




        #endregion


        #region AI上浮

        [Foldout("上浮能力", true)]
        [Header("在水中会向上浮")] public bool AI_CanSwiming;
        [Header("检测时间")] public float Floating_CheckTime = 1f; private float lastCheckTime = 0f;
        [Header("上浮力")] public float FloatingForce = 1f;
        

        void _ReferUpdate_AICompetent()
        {
            // 如果可以游泳并且时间到达检查间隔
            if (AI_CanSwiming && Time.time - lastCheckTime >= Floating_CheckTime)
            {
                // 更新上次检查的时间
                lastCheckTime = Time.time;

                // 如果中心入水，则尝试跳一下
                if (Collider_Component.IsInTheWater(Collider_Component.EyesPoint))
                {
                    Velocity_Component.AddForce(Vector3.up, FloatingForce);
                }
            }
        }



        #endregion


        #region Debug

        [Foldout("Debug", true)]
        [Header("打开射线调试")] public bool Debug_ShowEyesRayCast;
        [Header("暂停Ai活动")] public bool Debug_PauseAI;

        void _ReferUpdate_Debug_ShowEyesRayCast()
        {
            if (!Debug_ShowEyesRayCast)
                return;

            Vector3 _direct = player.cam.transform.position - Collider_Component.EyesPoint;
            float _dis = (player.cam.transform.position - Collider_Component.EyesPoint).magnitude;

            if (_dis < AIseeDistance)
            {
                Debug.DrawRay(Collider_Component.EyesPoint, _direct.normalized * _dis, Color.red);
            }
            else
            {
                Debug.DrawRay(Collider_Component.EyesPoint, _direct.normalized * _dis, Color.green);
            }
            
        }

        #endregion




    }
}


public class MC_AI_Slime_Component : MonoBehaviour
{

    #region 变量

    [Foldout("状态")]
    [Header("当前状态")] public AIState currentState = AIState.Idle;
    [Header("是否发现玩家")] [ReadOnly] public bool isSeePlayer;


    #endregion


    #region 周期函数

    MC_Collider_Component Collider_Component;
    World world;

    private void Awake()
    {
        Collider_Component = GetComponent<MC_Collider_Component>();
        world = Collider_Component.managerhub.world;
    }

    private void Update()
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
        StateController();
        AIVision();
    }


    #endregion


    #region 状态机控制器

    
    void StateController()
    {
        switch (currentState)
        {
            case AIState.Idle:
                OnIdle();
                break;

            case AIState.Chase:
                OnChase();
                break;

            case AIState.Flee:
                OnFlee();
                break;
        }
    }

    
    #endregion


    #region Idle

    void OnIdle()
    {

    }

    #endregion


    #region Chase

    void OnChase()
    {

    }



    #endregion


    #region Flee

    void OnFlee()
    {

    }

    #endregion


    #region 视觉系统

    [Foldout("AI视觉")]
    [Header("视力范围")] public float VisionDistance = 10f;


    void AIVision()
    {
        //如果实体与玩家距离小于VisionDistance

        //每帧进行射线检测,更新isSeePlayer状态
    }

    #endregion


    #region Debug

    #endregion


}