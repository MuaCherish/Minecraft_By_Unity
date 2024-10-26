using UnityEngine;
using System.Collections;
using MCEntity;
using Homebrew;

namespace MCEntity
{

    [RequireComponent(typeof(MC_Collider_Component))]
    public class MC_Velocity_Component : MonoBehaviour
    {

        #region 状态

        [Foldout("状态", true)]
        public bool isMoving;

        void ReferUpdateState()
        {
            if(velocity != Vector3.zero)
            {
                isMoving = true;
            }
            else
            {
                isMoving= false;
            }
        }

        #endregion

        #region 周期函数 

        MC_Collider_Component Collider_Component;
        //public ManagerHub managerhub;

        private void Awake()
        {
            Collider_Component = GetComponent<MC_Collider_Component>();
            
            //if (managerhub == null)
            //{
            //    //managerhub = Global.FindManagerhub();
            //}
            //else
            //{
            //    Debug.LogError("Velocity_Component.Managerhub == null");
            //}

        }

        private void Start()
        {

            Initialize();
        }




        private void FixedUpdate()
        {
            FixedUpdateCaculate();
            //UpdateDebug();
        }

        private void Update()
        {
            ReferUpdateState();
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
        [Header("实体移动速度")] public float speed_move = 1f;
        [Header("实体终端下降速度")] public float speedDown_ultimate = -10f;
        [Header("实体跳跃力")] public float force_jump = 77f; 
        [Header("实体重力")] public float force_gravity = -9.8f;


        //衰减系数
        [Foldout("衰减系数", true)]
        [Header("水平摩擦系数 (越小越滑)")] public float Damping_Horizontal = 20f;

        
        // 初始化函数
        private void Initialize()
        {
            velocity = Vector3.zero;
            momentum = new Vector3(0, force_gravity, 0);
            Othermomentum = Vector3.zero;
        }

        //Update引用
        private void FixedUpdateCaculate()
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

            // 计算重力
            Vector3 Force_gravity = new Vector3(0, force_gravity, 0); // 重力
            momentum += Force_gravity;

            // 计算其他力
            if (Othermomentum != Vector3.zero)
            {
                momentum += Othermomentum;
                Othermomentum = Vector3.zero;
            }

            // 检查是否达到终端速度
            if (momentum.y <= force_gravity)
            {
                momentum = new Vector3(0, force_gravity, 0);
            }
        }

        // 计算速度
        void Caculate_Velocity()
        {
            // 条件返回
            if (Collider_Component.isGround)
            {
                if (momentum.y <= force_gravity)
                {
                    // 将垂直速度归零
                    velocity.y = 0f;

                    // 水平速度缓慢减为0
                    // 使用线性衰减，Damping_Horizontal 是你可以定义的水平衰减因子
                    velocity.x = Mathf.MoveTowards(velocity.x, 0f, Damping_Horizontal * Time.deltaTime);
                    velocity.z = Mathf.MoveTowards(velocity.z, 0f, Damping_Horizontal * Time.deltaTime);
                    
                    return;
                }
                
            }

            //---------------------------计算区域-----------------------

            // 根据动量计算加速度
            Vector3 acceleration = momentum / mass; // 根据动量计算加速度

            // 更新速度
            velocity += acceleration * Time.deltaTime ; // 使用加速度更新速度

            //---------------------------限值区域-----------------------

            // 检查是否达到终端速度并限制速度
            if (velocity.y <= speedDown_ultimate)
            {
                // 强制速度为终端速度
                velocity.y = speedDown_ultimate;
            }

            CheckSliperVelocity();
        }


        void CheckSliperVelocity()
        {
            //滑膜检测
            if ((velocity.z > 0 && Collider_Component.collider_Front) || (velocity.z < 0 && Collider_Component.collider_Back))
                velocity.z = 0;
            if ((velocity.x > 0 && Collider_Component.collider_Right) || (velocity.x < 0 && Collider_Component.collider_Left))
                velocity.x = 0;
        }



        //移动实体
        private void ApplyPosition()
        {
            // 更新位置，只使用速度
            transform.position += velocity * Time.deltaTime;
        }




        /// <summary>
        /// 实体停止
        /// </summary>
        public void StopVelocity()
        {
            velocity = Vector3.zero;
        }


        /// <summary>
        /// 添加速度
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
        /// 设置动量
        /// </summary>
        /// <param name="_force">瞬时冲量</param>
        public void AddForce(Vector3 _force)
        {
            //print($"施加了{_force.magnitude}的力");
            Othermomentum += _force;
        }


        /// <summary>
        /// 实体跳跃
        /// </summary>
        /// <param name="_force">瞬时冲量</param>
        public void EntityJump()
        {
            // 仅在地面上时进行跳跃
            if (Collider_Component.isGround)
            {

                AddForce(force_jump * Vector3.up); // 添加跳跃的冲量
            }
        }
        public void EntityJump(Vector3 _direct)
        {
            _direct = _direct.normalized;

            // 仅在地面上时进行跳跃
            if (Collider_Component.isGround)
            {

                AddForce(force_jump * _direct); // 添加跳跃的冲量
            }
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


        


        #endregion


    }
}