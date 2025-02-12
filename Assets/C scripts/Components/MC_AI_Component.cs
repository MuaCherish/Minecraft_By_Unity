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

    [RequireComponent(typeof(MC_Velocity_Component))]
    [RequireComponent(typeof(MC_Collider_Component))]
    [RequireComponent(typeof(MC_Life_Component))]
    [RequireComponent(typeof(MC_Animator_Component))]
    public class MC_AI_Component : MonoBehaviour
    {


        #region 状态

        [Foldout("状态机", true)]
        [Header("当前状态")][ReadOnly] public AIState currentState = AIState.Idle;
        [Header("Idle")][ReadOnly] public IdleState current_IdleState = IdleState.Wait;
        [Header("Chase")][ReadOnly] public ChaseState current_ChaseState = ChaseState.Wait;
        [Header("Flee")][ReadOnly] public FleeState current_FleeState = FleeState.Wait;
        [Header("是否发现玩家")][ReadOnly] public bool isSeePlayer;

        [Foldout("实体类型", true)]
        [Header("移动方式")] public AIMovingType currentMovingType = AIMovingType.Walk;
        [Header("是否具有攻击性")] public bool isAggressive;

        #endregion


        #region 周期函数

        MC_Velocity_Component Velocity_Component;
        MC_Collider_Component Collider_Component;
        MC_Registration_Component Registration_Component;
        MC_Life_Component Life_Component;
        MC_Animator_Component Animator_Component;
        World world;
        Player player;

        private void Awake()
        {
            Velocity_Component = GetComponent<MC_Velocity_Component>();
            Collider_Component = GetComponent<MC_Collider_Component>();
            Registration_Component = GetComponent<MC_Registration_Component>();
            Life_Component = GetComponent<MC_Life_Component>();
            Animator_Component = GetComponent<MC_Animator_Component>();
            world = Collider_Component.managerhub.world;
            player = Collider_Component.managerhub.player;
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
            _ReferUpdate_AIState_Controller();
            _ReferUpdate_VisionSystem();
            _ReferUpdate_WatchPlayer();
            _ReferUpdate_AISwimming();
            _ReferUpdate_AIAttack();
        }


        #endregion


        #region AI有限状态机

        /// <summary>
        /// 设定AI状态
        /// </summary>
        /// <param name="_state"></param>
        public void SetAIState(AIState _state)
        {
            currentState = _state;
        }

        //总控制器
        void _ReferUpdate_AIState_Controller()
        {
            //提前返回-如果实体已经死亡
            if (Life_Component.isEntity_Dead)
                return;

            AutoUpdateAIState();
            ApplyAIState();
        }

        //自动更新状态
        void AutoUpdateAIState()
        {
            //攻击型实体状态自动更新
            if (isAggressive)
            {
                //提前返回-如果不是生存模式
                if (world.game_mode != GameMode.Survival)
                    return;

                //看到玩家进入追逐状态
                if (isSeePlayer)
                {
                    currentState = AIState.Chase;
                    canWatchPlayer = true;
                }
                else
                {
                    currentState = AIState.Idle;
                    canWatchPlayer = false;
                }
            }
        }

        //状态调度
        void ApplyAIState()
        {

            //提前返回-暂停AI活动
            if (Debug_PauseAI)
                return;

            switch (currentState)
            {
                case AIState.Idle:
                    OnIdleState();

                    //Chase
                    if (current_ChaseState != ChaseState.Wait)
                        current_ChaseState = ChaseState.Wait;
                    //Flee

                    break;

                case AIState.Chase:
                    OnChaseState();

                    //Idle
                    if (current_IdleState != IdleState.Wait)
                        current_IdleState = IdleState.Wait;
                    //Flee

                    break;

                case AIState.Flee:
                    OnFleeState();

                    //Idle
                    if (current_IdleState != IdleState.Wait)
                        current_IdleState = IdleState.Wait;
                    //Chase
                    if (current_ChaseState != ChaseState.Wait)
                        current_ChaseState = ChaseState.Wait;

                    break;
            }
        }


        #region IdleState

        void OnIdleState()
        {

            switch (currentMovingType)
            {
                case AIMovingType.Jump:
                    Hanle_IdleState_JumpType();
                    break;

                case AIMovingType.Walk:
                    Hanle_IdleState_WalkType();
                    break;

            }
        }


        #region IdleState / JumpType

        [Foldout("Idle/Jump设置", true)]
        [Header("Idle跳跃Y值")] public float IdleJumpY = 1f;
        [Header("Wait状态等待范围")] public Vector2 IdlewaitTimeRange = new Vector2(3f, 7f);
        [Header("IdleMoving概率")] public float IdleMovingProbability = 0.6f;
        [Header("跳跃力度范围")] public Vector2 IdleJumpForceRange = new Vector2(100f, 200f);
        private Vector3 targetDirection = Vector3.zero;
        private bool isWaiting = false;  //hasExec_Wait
        private float waitTime = 0f;  // 等待时间
        private float waitStartTime = 0f;  // 记录开始时间
        private float rotateStartTime = -1f;    // 等待开始时间，初始化为-1，表示尚未开始等待


        void Hanle_IdleState_JumpType()
        {
            switch (current_IdleState)
            {
                case IdleState.Wait:
                    Handle_IdleState_JumpType_Wait();
                    break;

                case IdleState.Rotate:
                    Handle_IdleState_JumpType_Rotate();
                    break;

                case IdleState.Moving:
                    Handle_IdleState_JumpType_Moving();
                    break;
            }
        }

        void Handle_IdleState_JumpType_Wait()
        {
            // 如果尚未开始等待，设置等待时间并记录开始时间
            if (!isWaiting)
            {
                // 随机方向
                Vector3 direction = (Random.onUnitSphere * 0.5f).normalized;
                direction.y = IdleJumpY;  // 设置Y轴为jumpheight
                targetDirection = direction;

                // 设置随机等待时间
                waitTime = Random.Range(IdlewaitTimeRange.x, IdlewaitTimeRange.y);
                waitStartTime = Time.time;  // 记录开始等待的时间
                isWaiting = true;  // 标记已开始等待
            }

            // 如果等待时间已到
            if (Time.time - waitStartTime >= waitTime)
            {
                current_IdleState = IdleState.Rotate;  // 旋转状态
                isWaiting = false;
            }
        }

        void Handle_IdleState_JumpType_Rotate()
        {
            // 计算当前方向与目标方向的角度差（仅在XZ平面旋转）
            Vector3 currentForward = Velocity_Component.ModelObject.transform.forward;
            currentForward.y = 0;  // 保持y轴为0，确保只在XZ平面计算
            float angle = Vector3.SignedAngle(currentForward, targetDirection, Vector3.up);  // 计算角度差

            // 根据旋转速度和角度差计算旋转时间
            float rotateWaitTime = Mathf.Abs(angle) / Velocity_Component.rotationSpeed;  // 旋转所需时间

            // 如果没有开始等待，记录开始时间
            if (rotateStartTime == -1f)
            {
                rotateStartTime = Time.time;
                Velocity_Component.EntitySmoothRotation(targetDirection, rotateWaitTime);  // 开始旋转
            }

            // 如果等待时间未达到旋转所需时间
            if (Time.time - rotateStartTime >= rotateWaitTime)
            {
                // 旋转完成后可以切换状态
                float randomValue = Random.value;  // 生成一个0到1之间的随机数

                // 根据概率判断下一个状态
                if (randomValue < IdleMovingProbability)
                    current_IdleState = IdleState.Moving;
                else
                    current_IdleState = IdleState.Wait;

                rotateStartTime = -1f;  // 重置开始时间
            }


        }

        void Handle_IdleState_JumpType_Moving()
        {

            float _force = Random.Range(IdleJumpForceRange.x, IdleJumpForceRange.y);
            Velocity_Component.AddForce(targetDirection, _force);
            current_IdleState = IdleState.Wait;
        }


        #endregion


        #region IdleState / WalkType


        void Hanle_IdleState_WalkType()
        {
            switch (current_IdleState)
            {
                case IdleState.Wait:
                    Handle_IdleState_WalkType_Wait();
                    break;

                case IdleState.Moving:
                    Handle_IdleState_WalkType_Moving();
                    break;
            }
        }


        void Handle_IdleState_WalkType_Wait()
        {
            //随机等待

            //确定下一次导航路径
        }


        void Handle_IdleState_WalkType_Moving()
        {

        }


        #endregion


        #endregion


        #region ChaseState

        void OnChaseState()
        {
            switch (currentMovingType)
            {
                case AIMovingType.Jump:
                    Handle_ChaseState_JumpType();
                    break;

                case AIMovingType.Walk:
                    Handle_ChaseState_WalkType();
                    break;

            }
        }


        #region ChaseState / JumpType

        [Foldout("Chase/Jump设置", true)]
        [Header("追逐状态跳跃Y值")] public float ChaseJumpY = 1f;
        [Header("追逐状态跳跃力度范围")] public Vector2 ChaseJumpForceRange = new Vector2(180f, 200f);
        [Header("追逐状态休息时间范围")] public Vector2 ChaseWaitTimeRange = new Vector2(1f, 3f);


        void Handle_ChaseState_JumpType()
        {

            switch (current_ChaseState)
            {
                case ChaseState.Moving:
                    Handle_ChaseState_JumpType_Moving();
                    break;

                case ChaseState.Wait:
                    Handle_ChaseState_JumpType_Wait();
                    break;
            }

        }

        void Handle_ChaseState_JumpType_Wait()
        {
            //等待ChaseWaitTimeRange范围
            if (!isWaiting)
            {
                waitTime = Random.Range(ChaseWaitTimeRange.x, ChaseWaitTimeRange.y);  // 设置随机等待时间
                waitStartTime = Time.time;  // 记录开始等待的时间
                isWaiting = true;  // 标记已开始等待
            }

            // 如果等待时间已到
            if (Time.time - waitStartTime >= waitTime)
            {
                current_ChaseState = ChaseState.Moving;
                isWaiting = false;
            }

        }

        void Handle_ChaseState_JumpType_Moving()
        {
            //提前返回-没落地就不起跳
            if (!Collider_Component.isGround)
                return;

            float _force = Random.Range(ChaseJumpForceRange.x, ChaseJumpForceRange.y);
            targetDirection = (player.transform.position - transform.position).normalized;
            targetDirection.y = ChaseJumpY;
            Velocity_Component.AddForce(targetDirection, _force);

            //End
            current_ChaseState = ChaseState.Wait;
        }




        #endregion


        #region ChaseState / WalkType

        void Handle_ChaseState_WalkType()
        {

        }

        #endregion


        #endregion


        #region FleeState

        /// <summary>
        /// AI逃跑，这个是有时间限制的
        /// </summary>
        public void EntityFlee()
        {
            //提前返回-如果已经在逃跑
            if (Corou_FleeTimer != null)
                return;

            float _fleeTime = 10f;
            switch (currentMovingType)
            {
                case AIMovingType.Jump:
                    _fleeTime = Random.Range(JumpFleeTimeRange.x, JumpFleeTimeRange.y);
                    break;
                case AIMovingType.Walk:
                    _fleeTime = Random.Range(WalkFleeTimeRange.x, WalkFleeTimeRange.y);
                    break;
            }

            //Start
            Corou_FleeTimer = StartCoroutine(FleeTimer(_fleeTime));

        }

        Coroutine Corou_FleeTimer;
        IEnumerator FleeTimer(float _fleeTime)
        {
            currentState = AIState.Flee;
            yield return new WaitForSeconds(_fleeTime);

            //End
            if (currentState == AIState.Flee)  //如果是Chase状态则不用动他
            {
                currentState = AIState.Idle;
                Corou_FleeTimer = null;
            }

        }


        void OnFleeState()
        {
            switch (currentMovingType)
            {
                case AIMovingType.Jump:
                    Handle_FleeState_JumpType();
                    break;

                case AIMovingType.Walk:
                    Handle_FleeState_WalkType();
                    break;

            }
        }


        #region FleeState / JumpType

        [Foldout("Flee/Jump设置", true)]
        [Header("逃跑时间")] public Vector2 JumpFleeTimeRange = new Vector2(10f, 20f);
        [Header("Flee跳跃Y值")] public float FleeJumpY = 0.5f;
        [Header("Wait状态等待范围")] public Vector2 FleewaitTimeRange = new Vector2(3f, 7f);
        [Header("FleeMoving概率")] public float FleeMovingProbability = 0.6f;
        [Header("跳跃力度范围")] public Vector2 FleeJumpForceRange = new Vector2(100f, 200f);

        void Handle_FleeState_JumpType()
        {
            switch (current_FleeState)
            {
                case FleeState.Wait:
                    Handle_FleeState_JumpType_Wait();
                    break;

                case FleeState.Rotate:
                    Handle_FleeState_JumpType_Rotate();
                    break;

                case FleeState.Moving:
                    Handle_FleeState_JumpType_Moving();
                    break;

            }
        }

        void Handle_FleeState_JumpType_Wait()
        {
            // 如果尚未开始等待，设置等待时间并记录开始时间
            if (!isWaiting)
            {
                // 随机方向
                Vector3 direction = (Random.onUnitSphere * 0.5f).normalized;
                direction.y = FleeJumpY;  // 设置Y轴为jumpheight
                targetDirection = direction;

                // 设置随机等待时间
                waitTime = Random.Range(FleewaitTimeRange.x, FleewaitTimeRange.y);
                waitStartTime = Time.time;  // 记录开始等待的时间
                isWaiting = true;  // 标记已开始等待
            }

            // 如果等待时间已到
            if (Time.time - waitStartTime >= waitTime)
            {
                current_FleeState = FleeState.Rotate;  // 旋转状态
                isWaiting = false;
            }
        }

        void Handle_FleeState_JumpType_Rotate()
        {
            // 计算当前方向与目标方向的角度差（仅在XZ平面旋转）
            Vector3 currentForward = Velocity_Component.ModelObject.transform.forward;
            currentForward.y = 0;  // 保持y轴为0，确保只在XZ平面计算
            float angle = Vector3.SignedAngle(currentForward, targetDirection, Vector3.up);  // 计算角度差

            // 根据旋转速度和角度差计算旋转时间
            float rotateWaitTime = Mathf.Abs(angle) / Velocity_Component.rotationSpeed;  // 旋转所需时间

            // 如果没有开始等待，记录开始时间
            if (rotateStartTime == -1f)
            {
                rotateStartTime = Time.time;
                Velocity_Component.EntitySmoothRotation(targetDirection, rotateWaitTime);  // 开始旋转
            }

            // 如果等待时间未达到旋转所需时间
            if (Time.time - rotateStartTime >= rotateWaitTime)
            {
                // 旋转完成后可以切换状态
                float randomValue = Random.value;  // 生成一个0到1之间的随机数

                // 根据概率判断下一个状态
                if (randomValue < FleeMovingProbability)
                    current_FleeState = FleeState.Moving;
                else
                    current_FleeState = FleeState.Wait;

                rotateStartTime = -1f;  // 重置开始时间
            }
        }

        void Handle_FleeState_JumpType_Moving() 
        {
            float _force = Random.Range(FleeJumpForceRange.x, FleeJumpForceRange.y);
            Velocity_Component.AddForce(targetDirection, _force);
            current_FleeState = FleeState.Wait;
        }

        #endregion


        #region FleeState / WalkType

        [Foldout("Flee/Walk设置", true)]
        [Header("逃跑时间")] public Vector2 WalkFleeTimeRange = new Vector2(10f, 20f);

        void Handle_FleeState_WalkType() 
        {

        }

        #endregion


        #endregion



        #endregion


        #region AI能力


        #region AI自动跳跃

        #endregion


        #region AI寻路

        [Foldout("AI寻路", true)]
        [Header("到达中间节点的距离误差")] public float nodeAcceptanceDistance = 0.25f;
        [Header("单个节点最大寻路等待时间")] public float maxNavigationWaitTime = 3f;

        /// <summary>
        /// 实体导航
        /// </summary>
        public void EntityNavigation()
        {

        } 


        IEnumerator EntityNavigation_Old(List<Vector3> _Nodes)
        {
            //AI看向目的地

            //挨个经过每一个节点
            foreach (var _pos in _Nodes)
            {
                //计时
                float _moveTime = 0;

                //向_pos移动直到误差小于nodeAcceptanceDistance
                while (true)
                {
                    //如果超时则停止移动

                    //获得实体的朝向

                    //移动

                    //end
                    _moveTime += Time.deltaTime;
                    yield return null;
                }
            }

            
        }


        #endregion


        #region AI视觉

        [Foldout("AI视觉", true)]
        [Header("是否会注视玩家")] public bool canWatchPlayer;
        [Header("视力范围")] public float visionDistance = 10f;
        [Header("多久转一次头")] public float WatchrotateTime = 0.2f;
        private float WatchTimer = 0f;

        void _ReferUpdate_VisionSystem()
        {
            // 如果实体与玩家距离小于 VisionDistance
            float _dis = (player.cam.transform.position - Collider_Component.EyesPoint).magnitude;

            if (_dis <= visionDistance)
            {
                // 每帧进行射线检测，更新 isSeePlayer 状态
                Vector3 _direct = player.cam.transform.position - Collider_Component.EyesPoint;
                RayCastStruct _rayCast = player.NewRayCast(Collider_Component.EyesPoint, _direct, _dis, Registration_Component.EntityID);

                // 被墙挡着则看不见
                if (_rayCast.isHit == 1)
                    isSeePlayer = false;
                else
                    isSeePlayer = true;
            }
            else
            {
                isSeePlayer = false;
            }
        }

        void _ReferUpdate_WatchPlayer()
        {
            //提前返回-如果不能注视玩家
            if (!canWatchPlayer)
                return;

            // 提前返回 - 距离过远或者没看到玩家
            float _dis = (player.transform.position - transform.position).magnitude;
            if (_dis > visionDistance || isSeePlayer == false)
                return;

            //提前返回-如果暂停AI活动
            if (Debug_PauseAI)
                return;

            // 提前返回 - 如果计时器未达到旋转时间
            if (WatchTimer < WatchrotateTime)
            {
                WatchTimer += Time.deltaTime;  // 累加计时器
                return;  // 等待下次旋转
            }

            // 旋转
            Vector3 playerPosition = player.transform.position;
            Vector3 direction = (playerPosition - transform.position).normalized;
            Velocity_Component.EntitySmoothRotation(direction, WatchrotateTime);

            // 重置计时器，等待下次旋转
            WatchTimer = 0f;
        }
        
        #endregion


        #region AI游泳

        [Foldout("AI游泳", true)]
        [Header("在水中会向上浮")] public bool AI_CanSwiming;
        [Header("检测时间")] public float Floating_CheckTime = 0.1f; private float lastCheckTime = 0f;
        [Header("上浮力")] public float FloatingForce = 5f;

        void _ReferUpdate_AISwimming()
        {
            // 如果可以游泳并且时间到达检查间隔
            if (AI_CanSwiming && Time.time - lastCheckTime >= Floating_CheckTime)
            {
                // 更新上次检查的时间
                lastCheckTime = Time.time;

                // 如果中心入水，则尝试跳一下
                if (Collider_Component.IsInTheWater(transform.position))
                {
                    Velocity_Component.AddForce(Vector3.up, FloatingForce);
                }
            }
        }


        #endregion


        #region AI攻击

        [Foldout("AI攻击", true)]
        [Header("攻击范围扩大倍数")] public float AttackRangeMultiplier = 1.2f;
        [Header("攻击伤害值")] public int AttackDamage = 3;
        [Header("攻击冷却")] public float AttackColdTime = 0.5f;
        private float lastAttackTime = 0f;  // 记录上次攻击时间

        void _ReferUpdate_AIAttack()
        {
            // 提前返回-如果不是生存模式
            if (world.game_mode != GameMode.Survival)
                return;

            // 提前返回-不是追逐状态
            if (currentState != AIState.Chase)
                return;

            // 提前返回-实体与玩家离得太远了
            float _dis = (transform.position - player.transform.position).magnitude;
            float _maxDis = Mathf.Abs(Collider_Component.hitBoxWidth - player.playerWidth);
            if (_dis > _maxDis)
                return;

            // 检查是否处于冷却状态，如果冷却时间还没到，就不攻击
            if (Time.time - lastAttackTime < AttackColdTime)
                return;

            // 更新最后一次攻击时间
            lastAttackTime = Time.time;

            // 检测实体与玩家的碰撞盒，并予以玩家伤害
            Vector3 hitVec = player.CheckHitBox(transform.position, Collider_Component.hitBoxWidth * AttackRangeMultiplier, Collider_Component.hitBoxHeight * AttackRangeMultiplier);
            if (hitVec != Vector3.zero)
            {
                //print("成功打到玩家");
                hitVec.y = 1f;
                player.ForceMoving(hitVec, 2.5f, 0.2f);
                Collider_Component.managerhub.lifeManager.UpdatePlayerBlood(AttackDamage, true, true);
            }
        }


        #endregion


        #endregion


        #region Debug


        [Foldout("Debug", true)]
        [Header("显示调试射线")] public bool Debug_DrawRay;
        [Header("暂停Ai活动")] public bool Debug_PauseAI;

        void OnDrawGizmos()
        {
            //提前返回-如果没有开debug
            if (!Debug_DrawRay)
                return;

            //视力调试
            Vector3 _direct = player.cam.transform.position - Collider_Component.EyesPoint;
            float _dis = (player.cam.transform.position - Collider_Component.EyesPoint).magnitude;
            if (_dis <= visionDistance)
            {

                // 绘制射线
                if (isSeePlayer)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.green;
                Gizmos.DrawRay(Collider_Component.EyesPoint, _direct.normalized * _dis);  // 从眼睛位置到玩家位置的射线

                // 绘制警戒线
                if (isSeePlayer)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, visionDistance);

            }
            else
            {
                // 绘制警戒线
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, visionDistance);
            }

            //攻击范围调试
            if (currentState == AIState.Chase)
            {
                float _width = Collider_Component.hitBoxWidth * AttackRangeMultiplier;
                float _height = Collider_Component.hitBoxHeight * AttackRangeMultiplier;
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, new Vector3(_width, _height, _width));
            }


        }

        #endregion


    }






}




public class MC_AI_Component_OldClone: MonoBehaviour
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
    [Header("AI移动方式")] public AIMovingType myMovingType = AIMovingType.Walk;

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
        if (myMovingType == AIMovingType.Walk && Collider_Component.Collider_Surround)
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
            if (myMovingType == AIMovingType.Jump)
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
            else if (myMovingType == AIMovingType.Walk)
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