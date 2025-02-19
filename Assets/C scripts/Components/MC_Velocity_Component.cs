using UnityEngine;
using System.Collections;
using MCEntity;
using Homebrew;
using System.IO;

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
        MC_Life_Component life_Component;
        private void Awake()
        {
            Collider_Component = GetComponent<MC_Collider_Component>();
            life_Component = GetComponent<MC_Life_Component>();
        }

        private void Start()
        {

            Initialize();
        }



        //public bool RotationToggle;
        //public float horizon;
        private void FixedUpdate()
        {
            if (Collider_Component.managerhub.world.game_state == Game_State.Playing)
            {
                _ReferFixedUpdate_Caculate();

            }

        }

        private void Update()
        {
            _ReferUpdate_UpdateState();
        }

        #endregion


        #region 公开函数

      

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

            //print($"强制位移{_direct * _value}");

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
        private bool RequestforEntityJump = true;
        public void EntityJump()
        {
            // 仅在地面上时进行跳跃
            if (Collider_Component.isGround && RequestforEntityJump)
            {
                //print($"momentum:{momentum}, velocity: {velocity}");
                AddForce(Vector3.up, force_jump); // 添加跳跃的冲量
                StartCoroutine(ColdTime());
            }
        }

        IEnumerator ColdTime()
        { 
            RequestforEntityJump = false;
            yield return new WaitForSeconds(1f);
            RequestforEntityJump = true;
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

        //速度和力参数
        [Foldout("速度和力参数", true)]
        [Header("实体移动速度")] public float speed_move = 1.5f;
        [Header("实体终端下降速度")] public float speedDown_ultimate = -20f;
        [Header("实体水中终端速度")] public float WaterspeedDown_ultimate = -2f;
        [Header("实体跳跃力")] public float force_jump = 270f; 
        [Header("实体受伤力")] public float force_hurt = 125f; 
        [Header("实体重力")] public float force_gravity = -13f;
        [Header("实体水下重力")] public float force_Watergravity = -2f;
        [Header("实体跳跃冷却")] public float JumpColdTime = 0.1f;  private float jumpCooldownTimer = 0f;   // 计时器

        [Foldout("旋转参数", true)]
        [Header("旋转速度")] public float rotationSpeed = 90f; // 默认旋转速度

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
            Vector3 acceleration = momentum; // 根据动量计算加速度

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



        //private Vector3 targetDirection = Vector3.forward; // 默认朝向
        //private bool isInstantRotation = false; // 默认非瞬时旋转

        /// <summary>
        /// 将Entity转向某个方向，仅旋转XZ平面，保持Y值不变，平滑过渡
        /// </summary>
        GameObject _Model
        {
            get
            {
                return Collider_Component.Model;
            }
        }
        GameObject _Head
        {
            get
            {
                return Collider_Component.Head;
            }
        }
        GameObject _Body
        {
            get
            {
                return Collider_Component.Body;
            }
        }

        [Foldout("实体旋转", true)]
        [Header("身体比头慢的时间百分比")] public float BodySlowRotationPercent = 1.4f;
        private Coroutine _rotationCoroutine;  // 用于保存当前旋转的协程

        public void EntitySmoothRotation(Vector3 _direct, float _elapseTime)
        {
            // 如果当前已有旋转协程在进行，则不重复启动
            if (_rotationCoroutine == null)
            {
                _rotationCoroutine = StartCoroutine(SmoothRotationCoroutine(_direct, _elapseTime));
            }
        }

        private IEnumerator SmoothRotationCoroutine(Vector3 _direct, float _elapseTime)
        {
            // 将目标方向的Y值设置为0，确保只在XZ平面旋转
            _direct.y = 0;

            // 确保目标方向有长度，避免异常
            if (_direct.sqrMagnitude < 0.0001f)
                yield break;

            // 计算目标方向的旋转四元数
            Quaternion targetRotation = Quaternion.LookRotation(_direct);

            // 记录头部和身体的初始旋转
            Quaternion initialHeadRotation = _Head.transform.rotation;
            Quaternion initialBodyRotation = _Body.transform.rotation;

            // 累计时间
            float elapsedTime = 0f;
            float headRotationTime = _elapseTime;  // 头部的旋转时间
            float bodyRotationTime = _elapseTime * BodySlowRotationPercent;  // 身体的旋转时间

            // while循环中根据身体的旋转时间进行控制
            while (elapsedTime < bodyRotationTime)
            {
                // 提前返回-如果实体已死亡
                if (life_Component != null && life_Component.isEntity_Dead)
                    yield break;

                elapsedTime += Time.deltaTime;

                // 头部旋转（按头部旋转时间平滑旋转） - 只有在头部没有完成时才旋转
                if (elapsedTime < headRotationTime)
                {
                    _Head.transform.rotation = Quaternion.Slerp(
                        initialHeadRotation,
                        targetRotation,
                        elapsedTime / headRotationTime
                    );
                }
                // 身体旋转
                _Body.transform.rotation = Quaternion.Slerp(
                    initialBodyRotation,
                    targetRotation,
                    Mathf.Clamp01(elapsedTime / bodyRotationTime) // 控制身体旋转
                );

                // 等待下一帧
                yield return null;
            }

            // 确保最终头部和身体都对准目标方向
            _Head.transform.rotation = targetRotation;
            _Body.transform.rotation = targetRotation;

            // 协程结束后重置旋转协程标志
            _rotationCoroutine = null;
        }



        /// <summary>
        /// 实体整体旋转旋转
        /// 输入值在[-1,1]之间
        /// </summary>
        public void EntityRotation(float _HorizonInput)
        {
            // 完成水平的旋转
            _Model.transform.Rotate(Vector3.up, _HorizonInput * Time.fixedDeltaTime);
        }


        /// <summary>
        /// 头旋转
        /// 输入值在[-1,1]之间
        /// </summary>
        public void EntityHeadVerticleRotation(float _VerticleInput, Vector2 _CameraLimit, Vector2 _HeadLimit)
        {
            // 计算当前的垂直旋转角度
            float currentVerticalAngle = _Head.transform.localRotation.eulerAngles.x;

            // 将旋转角度限制在0到360之间
            if (currentVerticalAngle > 180)
                currentVerticalAngle -= 360;

            // 计算新的旋转角度
            float newVerticalAngle = currentVerticalAngle - _VerticleInput * Time.fixedDeltaTime;

            // 限制新的旋转角度在[-90, 90]范围内
            newVerticalAngle = Mathf.Clamp(newVerticalAngle, _HeadLimit.x, _HeadLimit.y);

            // 使用 Quaternion.Euler 设置新的旋转
            _Head.transform.localRotation = Quaternion.Euler(newVerticalAngle, 0f, 0f);
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