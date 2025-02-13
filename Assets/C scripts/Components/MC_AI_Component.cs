using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MCEntity
{

    [RequireComponent(typeof(MC_Velocity_Component))]
    [RequireComponent(typeof(MC_Collider_Component))]
    [RequireComponent(typeof(MC_Life_Component))]
    [RequireComponent(typeof(MC_Animator_Component))]
    [RequireComponent(typeof(MC_Registration_Component))]
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
            _ReferUpdate_AutoJump();
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
            Vector3 currentForward = Velocity_Component.EntityForward;
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


        [Foldout("Idle/Walk设置", true)]
        [Header("IdleWalk等待时间")] public Vector2 IdleWalkWaitTimeRange = new Vector2(3, 20f);
        [Header("随机游走-迭代步数")] public int RandomWalk_Steps = 10;
        [Header("随机游走-遵循上次方向的概率")] public float PrevDirectionProbability = 0.8f;

        private Vector3 TargetPos;
        bool isReachTargetPos = true;
        bool hasExec_Handle_IdleState_WalkType_Wait = true;
        bool hasExec_Handle_IdleState_WalkType_Moving = true;


        void Hanle_IdleState_WalkType()
        {
            switch (current_IdleState)
            {
                case IdleState.Wait:
                    Handle_IdleState_WalkType_Wait();
                    if (!hasExec_Handle_IdleState_WalkType_Moving)
                        hasExec_Handle_IdleState_WalkType_Moving = true;
                    break;

                case IdleState.Moving:
                    Handle_IdleState_WalkType_Moving();
                    if (!hasExec_Handle_IdleState_WalkType_Wait)
                        hasExec_Handle_IdleState_WalkType_Wait = true;
                    break;
            }
        }


        void Handle_IdleState_WalkType_Wait()
        {
            //一次性函数
            if(hasExec_Handle_IdleState_WalkType_Wait)
            {
                // 设置随机等待时间
                waitTime = Random.Range(IdleWalkWaitTimeRange.x, IdleWalkWaitTimeRange.y);
                waitStartTime = Time.time;  // 记录开始等待的时间
                isWaiting = true;  // 标记已开始等待

                hasExec_Handle_IdleState_WalkType_Wait = false;
            }

            // 如果等待时间已到
            if (Time.time - waitStartTime >= waitTime)
            {
                current_IdleState = IdleState.Moving;  // 旋转状态
                isWaiting = false;
            }

        }


        
        void Handle_IdleState_WalkType_Moving()
        {
            //一次性函数
            if (hasExec_Handle_IdleState_WalkType_Moving)
            {

                //获取随机游走函数返回的路径
                Vector3 _StartPos = transform.position;
                MC_UtilityFunctions.Algo_RandomWalk(_StartPos, RandomWalk_Steps, PrevDirectionProbability, out List<Vector3> _Result);
                TargetPos = _Result[_Result.Count - 1];
                EntityMoveTo(TargetPos);
                isReachTargetPos = false;

                hasExec_Handle_IdleState_WalkType_Moving = false;
            }


            //While-等待走到目的地
            if (isReachTargetPos)
            {
                //走到目的地
                current_IdleState = IdleState.Wait;
            }

            

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
            Vector3 currentForward = Velocity_Component.EntityForward;
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

        [Foldout("AI自动跳跃", true)]
        [Header("AI是否会自动跳跃")] public bool isAIcanAutoJump;
        [Header("前进距离检测阈值")] public float AutoJump_CheckMaxDistance = 0.3f;
        [Header("自动跳跃冷却时间")] public float AutoJump_ColdTime = 1f; private float AutoJumpTimer = 0f;

        void _ReferUpdate_AutoJump()
        {
            // 提前返回-如果AI不会跳跃
            if (!isAIcanAutoJump)
                return;

            // 提前返回-如果AI没有移动
            if (!Velocity_Component.isMoving)
                return;

            // 向腿的正前方发射射线
            Vector3 _Origin = Collider_Component.FootPoint + new Vector3(0f, 0.125f, 0f);
            Vector3 _Direct = Velocity_Component.EntityForward;
            RayCastStruct _RayCast = player.NewRayCast(_Origin, _Direct, AutoJump_CheckMaxDistance, Registration_Component.GetEntityId());
            //print(_RayCast);

            // 如果没检测到方块则返回
            if (_RayCast.isHit != 1)
                return;

            // 如果冷却时间未结束则跳过
            if (AutoJumpTimer < AutoJump_ColdTime)
            {
                AutoJumpTimer += Time.deltaTime; // 在冷却时间内增加计时器
                return;
            }

            // 实体触发跳跃，随后进入冷却
            print("实体跳跃");
            Velocity_Component.EntityJump();

            // 跳跃后开始冷却
            AutoJumpTimer = 0f;  // 跳跃后才开始计时
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
                RayCastStruct _rayCast = player.NewRayCast(Collider_Component.EyesPoint, _direct, _dis, Registration_Component.GetEntityId());

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


        #region AI运动

        [Foldout("AI运动", true)]
        [Header("到达中间节点的距离误差")] public float nodeAcceptanceDistance = 0.25f;

        /// <summary>
        /// 实体移动到目标方向
        /// </summary>
        void EntityMoveTo(Vector3 _TargetPos)
        {
            StartCoroutine(EntityMoveToCoroutine(_TargetPos));
        }

        void EntityMoveTo(GameObject _TargetPos)
        {
            StartCoroutine(EntityMoveToCoroutine(_TargetPos));
        }

        IEnumerator EntityMoveToCoroutine(Vector3 _TargetPos)
        {
            //预处理
            //Vector3 _currentForward = Velocity_Component.EntityForward.normalized;
            Vector3 _Direct = (_TargetPos - transform.position).normalized;
            float _Speed = Velocity_Component.speed_move;
            float startTime = 0f;
            float maxNavigationWaitTime = (_TargetPos - transform.position).magnitude / _Speed;

            //标记动画组件Walk
            if (Animator_Component != null)
                Animator_Component.isWalk = true;

            //Rotation
            Velocity_Component.EntitySmoothRotation(_Direct, 0.2f);

            while (true)
            {
                //提前退出-到达目的地
                if ((transform.position - _TargetPos).magnitude < nodeAcceptanceDistance ||
                    startTime > maxNavigationWaitTime)  //超时
                {
                    isReachTargetPos = true;
                    break;
                }

                //向Forward移动
                Velocity_Component.SetVelocity("x", _Direct.x * _Speed);
                Velocity_Component.SetVelocity("z", _Direct.z * _Speed);

                //end
                startTime += Time.deltaTime;
                yield return null;
            }

            //End
            Animator_Component.isWalk = false;

        }

        IEnumerator EntityMoveToCoroutine(GameObject _TargetPos)
        {
            //预处理
            //Vector3 _currentForward = Velocity_Component.EntityForward.normalized;
            float _Speed = Velocity_Component.speed_move;
            float startTime = 0f;
            float maxNavigationWaitTime = (_TargetPos.transform.position - transform.position).magnitude / _Speed;

            //标记动画组件Walk
            if (Animator_Component != null)
                Animator_Component.isWalk = true;

            while (true)
            {
                //提前退出-到达目的地
                if ((transform.position - _TargetPos.transform.position).magnitude < nodeAcceptanceDistance ||
                    startTime > maxNavigationWaitTime)  //超时
                {
                    isReachTargetPos = true;
                    break;
                }


                Vector3 _Direct = (_TargetPos.transform.position - transform.position).normalized;
                Velocity_Component.EntitySmoothRotation(_Direct, 0.2f);

                //向Forward移动
                Velocity_Component.SetVelocity("x", _Direct.x * _Speed);
                Velocity_Component.SetVelocity("z", _Direct.z * _Speed);

                //end
                startTime += Time.deltaTime;
                yield return null;
            }

            //End
            Animator_Component.isWalk = false;

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
        [Header("是否可以攻击")] public bool canAttack = false;
        [Header("攻击范围扩大倍数")] public float AttackRangeMultiplier = 1.2f;
        [Header("攻击伤害值")] public int AttackDamage = 3;
        [Header("攻击冷却")] public float AttackColdTime = 0.5f;
        private float lastAttackTime = 0f;  // 记录上次攻击时间

        void _ReferUpdate_AIAttack()
        {
            //提前返回-如果不能攻击
            if (!canAttack)
                return;

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

            //绘制目的地
            if (!isReachTargetPos)
                Gizmos.DrawWireCube(TargetPos, new Vector3(0.8f, 0.8f, 0.8f));

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
 