using UnityEngine;
using System.Collections;
using MCEntity;
using Homebrew;
using static UsefulFunction;

namespace MCEntity
{

    [RequireComponent(typeof(MC_Collider_Component))]
    public class MC_Velocity_Component : MonoBehaviour
    {

        #region 状态

        [Foldout("状态", true)]
        [ReadOnly] public bool isMoving;
        [ReadOnly] public bool isJumpRequest;

        void _ReferUpdate_UpdateState()
        {
            // 更新移动状态
            isMoving = (velocity != Vector3.zero);

            if (Collider_Component.isGround)
            {
                // 在地面时更新冷却计时器
                if (jumpCooldownTimer <= 0)
                {
                    // 冷却时间完成，允许跳跃请求
                    isJumpRequest = true;
                }
                else
                {
                    // 冷却计时器减少
                    jumpCooldownTimer -= Time.deltaTime;
                }
            }
            else
            {
                // 离开地面时，重置跳跃请求
                isJumpRequest = false;

                // 当离开地面时重新设置冷却计时器
                jumpCooldownTimer = JumpColdTime;
            }
        }

        #endregion


        #region 周期函数 

        MC_Collider_Component Collider_Component;

        private void Awake()
        {
            Collider_Component = GetComponent<MC_Collider_Component>();
            
        }

        private void Start()
        {

            Initialize();
        }



        public bool RotationToggle;
        public float horizon;
        private void FixedUpdate()
        {
            if (Collider_Component.managerhub.world.game_state == Game_State.Playing)
            {
                _ReferFixedUpdate_Caculate();

                if (RotationToggle)
                {
                    EntityRotation(horizon, 0);
                }
            }
           
        }

        private void Update()
        {
            _ReferUpdate_UpdateState();
        }

        #endregion


        #region 公开函数

        /// <summary>
        /// 实体旋转
        /// 输入值在[-1,1]之间
        /// </summary>
        public void EntityRotation(float _HorizonInput, float _VerticalInput)
        {
            // 完成水平的旋转
            transform.Rotate(Vector3.up, _HorizonInput * RotationSensitivity * Time.fixedDeltaTime);

            // 如果头存在，则完成垂直的旋转
            if (HeadObject != null)
            {
                // 垂直旋转计算，限制在[-90, 90]度之间
                verticalRotation -= _VerticalInput * RotationSensitivity * Time.fixedDeltaTime;
                verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

                // 更新头部对象的局部旋转，仅影响X轴
                HeadObject.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            }
        }


        /// <summary>
        /// 获取速度
        /// </summary>
        /// <returns></returns>
        public Vector3 GetVelocity()
        {
            return velocity;
        }


        /// <summary>
        /// 添加速度
        /// 无法立刻改变速度？调用一点点AddForce即可解除y的锁
        /// </summary>
        /// <param name="_v"></param>
        public void SetVelocity(string _para, float _value)
        {
            switch (_para)
            {
                case "x":
                    velocity.x = _value;
                    break;
                case "y":
                    velocity.y = _value;
                    break;
                case "z":
                    velocity.z = _value;
                    break;
                default:
                    print("SetVelocity参数错误");
                    break;
            }

            CheckSliperVelocity();


        }


        /// <summary>
        /// 添加速度，按对象自身方向
        /// 无法立刻改变速度？调用一点点AddForce即可解除y的锁
        /// </summary>
        /// <param name="_v"></param>
        public void SetVelocity(BlockDirection _Direct, float _value)
        {
            // 根据方向设置对应的速度分量，只调用第一种重载函数修改特定轴
            switch (_Direct)
            {
                case BlockDirection.前:
                    SetVelocity("x", transform.forward.x * _value);
                    SetVelocity("z", transform.forward.z * _value);
                    break;
                case BlockDirection.后:
                    SetVelocity("x", -transform.forward.x * _value);
                    SetVelocity("z", -transform.forward.z * _value);
                    break;
                case BlockDirection.左:
                    SetVelocity("x", -transform.right.x * _value);
                    SetVelocity("z", -transform.right.z * _value);
                    break;
                case BlockDirection.右:
                    SetVelocity("x", transform.right.x * _value);
                    SetVelocity("z", transform.right.z * _value);
                    break;
                default:
                    print("SetVelocity.BlockDirection参数错误");
                    break;
            }
        }



        /// <summary>
        /// 实体停止
        /// </summary>
        public void StopVelocity()
        {
            velocity = Vector3.zero;
        }


        /// <summary>
        /// 设置动量
        /// </summary>
        /// <param name="_force">瞬时冲量</param>
        public void AddForce(Vector3 _direct, float _value)
        {
            _direct = Vector3.Normalize(_direct);

            //print($"施加了{_force.magnitude}的力");
            StartCoroutine(waitFrameToAddOtherForce(_direct, _value));

            // 计算瞬时加速度
            //Vector3 instantAcceleration = _direct.normalized * (_value / mass);

            //// 将加速度直接应用于速度
            //velocity += instantAcceleration;
        }

        
        /// <summary>
        /// 实体向上跳跃一次
        /// </summary>
        public void EntityJump()
        {
            // 仅在地面上时进行跳跃
            if (Collider_Component.isGround && isJumpRequest)
            {
                //print($"momentum:{momentum}, velocity: {velocity}");
                AddForce(Vector3.up, force_jump); // 添加跳跃的冲量
            }
        }


        /// <summary>
        /// 实体给定方向跳跃一次
        /// </summary>
        /// <param name="_direct"></param>
        public void EntityJump(Vector3 _direct)
        {
            _direct = _direct.normalized;

            // 仅在地面上时进行跳跃
            if (Collider_Component.isGround && isJumpRequest)
            {

                AddForce(_direct, force_jump); // 添加跳跃的冲量
            }
        }

         
        /// <summary>
        /// 实体随机跳跃一次
        /// </summary>
        /// <param name="_value"></param>
        public void EntityRandomJump(float _value)
        {
            Vector3 direction = Random.onUnitSphere * 0.5f; // 使用随机方向
            direction.y = 0.9f; // 设置Y轴为jumpheight
            Vector3 direct = direction.normalized; // 标准化向量
            AddForce(direct, _value); // 施加力
        }


        #endregion


        #region 速度与冲量

        //实体状态
        [Foldout("状态", true)]
        [Header("瞬时速度")][SerializeField][ReadOnly] private Vector3 velocity = Vector3.zero;
        [Header("瞬时动量")][SerializeField][ReadOnly] private Vector3 momentum = new Vector3(0, 0, 0);
        [Header("瞬时外界动量")][SerializeField][ReadOnly] private Vector3 Othermomentum = Vector3.zero; //受击使用这个向量，AddMomentum(Force)

        //实体参数
        [Foldout("实体参数", true)]
        [Header("实体重量")] public float mass = 1f;

        //速度和力参数
        [Foldout("速度和力参数", true)]
        [Header("实体移动速度")] public float speed_move = 1.5f;
        [Header("实体终端下降速度")] public float speedDown_ultimate = -20f;
        [Header("实体水中终端速度")] public float WaterspeedDown_ultimate = -2f;
        [Header("实体跳跃力")] public float force_jump = 270f; 
        [Header("实体重力")] public float force_gravity = -13f;
        [Header("实体水下重力")] public float force_Watergravity = -2f;
        [Header("实体跳跃冷却")] public float JumpColdTime = 0.1f;  private float jumpCooldownTimer = 0f;   // 计时器

        [Foldout("旋转参数", true)]
        [Header("实体头(选填)")] public GameObject HeadObject; // 头部对象，用于垂直旋转
        [Header("实体旋转灵敏度")] public float RotationSensitivity = 50.0f; // 设置旋转灵敏度

        private float verticalRotation = 0f; // 用于存储垂直旋转的累计值
        //衰减系数
        [Foldout("衰减系数", true)]
        [Header("水平摩擦系数 (越小越滑)")] public float Damping_Horizontal = 10f;


        //动态终端速度
        public float Ultimate_VerticalVelocity
        {
            get
            {
                if (Collider_Component.IsInTheWater(Collider_Component.FootPoint))
                {
                    return WaterspeedDown_ultimate;
                }
                else
                {
                    return speedDown_ultimate;
                }
            }
        }
        
        //动态重力
        public Vector3 Gravity
        {
            get
            {
                if (Collider_Component.IsInTheWater(Collider_Component.FootPoint))
                {
                    return new Vector3(0, force_Watergravity, 0);
                }
                else
                {
                    return new Vector3(0, force_gravity, 0);
                }
            }
        }


        
        
        // 初始化函数
        private void Initialize()
        {
            velocity = Vector3.zero;
            momentum = new Vector3(0, force_gravity, 0);
            Othermomentum = Vector3.zero;
        }

        //Update引用
        private void _ReferFixedUpdate_Caculate()
        {
            Caculate();
            ApplyPosition();
        }

        // 总的计算
        private void Caculate()
        {
            Caculate_Momentum();   // 首先计算动量（包括重力和空气阻力）
            Caculate_Velocity();   // 然后计算速度
        }



        // 计算动能
        void Caculate_Momentum()
        {
            // 检查是否在地面上
            //if (Collider_Component.colliderDown)
            //{
            //    // 允许在地面状态下重置动量
            //    if (momentum.y <= force_gravity)
            //    {
            //        momentum = new Vector3(0, force_gravity, 0);
            //        return; // 如果在地面上，不进行其他计算
            //    }
            //}
            momentum = Vector3.zero;

            // 计算重力
            momentum += Gravity;

            // 计算其他力
            if (Othermomentum != Vector3.zero)
            {
                momentum += Othermomentum;
                Othermomentum = Vector3.zero;
                //print($"施加了力，Moment：{momentum}");
            }

            // 检查是否达到终端速度
            if (momentum.y <= Gravity.y)
            {
                momentum = Gravity;
            }
        }

        // 计算速度
        void Caculate_Velocity()
        {
            // 条件返回-如果在地面
            if (Collider_Component.isGround && 
                Othermomentum == Vector3.zero
                )
            {
                if (momentum.y <= Gravity.y)
                {
                    // 将垂直速度归零
                    //velocity.y = 0f;


                    CheckSliperVelocity();

                    // 水平速度缓慢减为0
                    // 使用线性衰减，Damping_Horizontal 是你可以定义的水平衰减因子
                    velocity.x = Mathf.MoveTowards(velocity.x, 0f, Damping_Horizontal * Time.fixedDeltaTime);
                    velocity.z = Mathf.MoveTowards(velocity.z, 0f, Damping_Horizontal * Time.fixedDeltaTime);

                   
                    return;
                }
                
            }

            //---------------------------计算区域-----------------------

            // 根据动量计算加速度
            Vector3 acceleration = momentum / mass; // 根据动量计算加速度

            // 更新速度
            velocity += acceleration * Time.fixedDeltaTime; // 使用加速度更新速度

            //---------------------------限值区域-----------------------

            // 检查是否达到终端速度并限制速度
            CheckUltVelocity();

            //滑膜限制
            CheckSliperVelocity();
        }

        //终端速度限制
        void CheckUltVelocity()
        {
            if (velocity.y <= Ultimate_VerticalVelocity)
            {
                // 强制速度为终端速度
                velocity.y = Ultimate_VerticalVelocity;
            }
        }

        //滑膜限制
        void CheckSliperVelocity()
        {
            //滑膜检测
            if ((velocity.z > 0 && Collider_Component.collider_Front) || (velocity.z < 0 && Collider_Component.collider_Back))
                velocity.z = 0;
            if ((velocity.x > 0 && Collider_Component.collider_Right) || (velocity.x < 0 && Collider_Component.collider_Left))
                velocity.x = 0;
            if (velocity.y > 0 && Collider_Component.collider_Up || velocity.y < 0 && Collider_Component.isGround)
                velocity.y = 0;
        }



        // 移动实体
        private void ApplyPosition()
        {
            // 更新位置，只使用速度
            transform.position += velocity * Time.fixedDeltaTime;

        }










        #endregion


        #region Entity旋转

        private Vector3 targetDirection = Vector3.forward; // 默认朝向
        private float rotationSpeed = 90f; // 默认旋转速度
        private bool isInstantRotation = false; // 默认非瞬时旋转

        /// <summary>
        /// 设置旋转参数
        /// </summary>
        /// <param name="newTargetDirection">目标旋转方向（单位向量）</param>
        /// <param name="newRotationSpeed">旋转速度（角度/秒）</param>
        /// <param name="isInstant">是否立即完成旋转</param>
        public void SetRotationParameters(Vector3 newTargetDirection, float newRotationSpeed, bool isInstant = false)
        {
            targetDirection = newTargetDirection.normalized; // 保存目标方向并归一化
            rotationSpeed = newRotationSpeed;
            isInstantRotation = isInstant;
        }


        #endregion


        #region AI-移动到目标位置

        private Vector3 _targetPos_MoveTo;
        private Coroutine moveToObjectCoroutine;
        private bool isMovingToTarget;

        /// <summary>
        /// 移动到目标物体
        /// </summary>
        /// <param name="_targetObject">目标物体Object</param>
        public void MoveToObject(GameObject _targetObject)
        {
            Vector3 targetPos = _targetObject.transform.position;
            StartMoveCoroutine(targetPos);
        }

        /// <summary>
        /// 移动到目标位置
        /// </summary>
        /// <param name="_targetPos">目标位置Vector3</param>
        public void MoveToObject(Vector3 _targetPos)
        {
            StartMoveCoroutine(_targetPos);
        }

        // 目标切换
        private void StartMoveCoroutine(Vector3 targetPos)
        {
            _targetPos_MoveTo = targetPos;

            // 移动到目标
            if (moveToObjectCoroutine == null)
            {
                moveToObjectCoroutine = StartCoroutine(MoveToObjectCoroutine());
            }
            else
            {
                _targetPos_MoveTo = targetPos;
            }
        }

        private IEnumerator MoveToObjectCoroutine()
        {
            Vector3 _Velocity = Vector3.zero;
            isMovingToTarget = true;

            float TryJumpCold = 1f; // 冷却时间
            float jumpCooldownTimer = 0f; // 冷却计时器

            Vector3 Last_targetPos_MoveTo = Vector3.zero;
            Vector3 _NoY_targetPos_MoveTo = new Vector3(_targetPos_MoveTo.x, 0f, _targetPos_MoveTo.z);  //去掉Y值的目标
            transform.LookAt(_targetPos_MoveTo);

            // 卡住检测相关
            Vector3 lastCheckedPosition = new Vector3(selfPos.x, 0f, selfPos.z);
            float stuckCheckInterval = 2f; // 每秒检测一次
            float stuckTimer = 0f;
            float stuckThreshold = 0.05f; // 位移变化阈值，如果小于该值则视为卡住
            float stuckDuration = 2f; // 卡住持续时间

            while (true)
            {
                Vector3 _NoY_selfPos = new Vector3(selfPos.x, 0f, selfPos.z);

                // 条件返回
                if (!MC_UtilityFunctions.IsTargetVisible(_targetPos_MoveTo) || // 看不见了
                    (_NoY_selfPos - _NoY_targetPos_MoveTo).magnitude <= 0.5f || // 小于最小误差
                    isMovingToTarget == false) // 被人为停止
                {
                    StopVelocity();
                    moveToObjectCoroutine = null;
                    isMovingToTarget = false;
                    yield break;
                }

                // 如果碰到东西则尝试跳一下，且只有冷却时间到时才能跳
                if (Collider_Component.Collider_Surround && jumpCooldownTimer <= 0f)
                {
                    //print("Jump");
                    EntityJump();
                    jumpCooldownTimer = TryJumpCold; // 重置冷却时间
                }

                // 冷却计时器更新
                if (jumpCooldownTimer > 0f)
                {
                    jumpCooldownTimer -= Time.deltaTime; // 逐帧减少冷却时间
                }

                // 检查是否卡住
                stuckTimer += Time.deltaTime;
                if (stuckTimer >= stuckCheckInterval)
                {
                    if (Vector3.Distance(lastCheckedPosition, _NoY_selfPos) < stuckThreshold)
                    {
                        stuckDuration -= stuckCheckInterval;
                        if (stuckDuration <= 0f)
                        {
                            //print("移动被卡住，停止索敌");
                            StopVelocity();
                            moveToObjectCoroutine = null;
                            isMovingToTarget = false;
                            yield break;
                        }
                    }
                    else
                    {
                        stuckDuration = 2f; // 重置卡住计时器
                    }
                    lastCheckedPosition = _NoY_selfPos;
                    stuckTimer = 0f;
                }

                // 中途改变目标
                if (Last_targetPos_MoveTo != _targetPos_MoveTo)
                {
                    _Velocity = transform.forward * speed_move;
                    Last_targetPos_MoveTo = _targetPos_MoveTo;
                }
                else
                {
                    SetVelocity("x", _Velocity.x);
                    SetVelocity("z", _Velocity.z);
                }

                yield return null;
            }
        }





        #endregion


        #region 工具 

        /// <summary>
        /// 自身坐标
        /// </summary>
        public Vector3 selfPos
        {
            get { return transform.position; }
        }


        //等待一帧添加OtherForce
        IEnumerator waitFrameToAddOtherForce(Vector3 _direct, float _value)
        {
            yield return null;
            Othermomentum += _direct * _value;
        }



        #endregion


    }
}