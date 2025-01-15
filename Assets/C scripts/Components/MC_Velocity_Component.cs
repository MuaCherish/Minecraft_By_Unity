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

        #region ״̬

        [Foldout("״̬", true)]
        [ReadOnly] public bool isMoving;
        [ReadOnly] public bool isJumpRequest;

        void _ReferUpdate_UpdateState()
        {
            // �����ƶ�״̬
            isMoving = (velocity != Vector3.zero);

            if (Collider_Component.isGround)
            {
                // �ڵ���ʱ������ȴ��ʱ��
                if (jumpCooldownTimer <= 0)
                {
                    // ��ȴʱ����ɣ�������Ծ����
                    isJumpRequest = true;
                }
                else
                {
                    // ��ȴ��ʱ������
                    jumpCooldownTimer -= Time.deltaTime;
                }
            }
            else
            {
                // �뿪����ʱ��������Ծ����
                isJumpRequest = false;

                // ���뿪����ʱ����������ȴ��ʱ��
                jumpCooldownTimer = JumpColdTime;
            }
        }

        #endregion


        #region ���ں��� 

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


        #region ��������

        /// <summary>
        /// ʵ����ת
        /// ����ֵ��[-1,1]֮��
        /// </summary>
        public void EntityRotation(float _HorizonInput, float _VerticalInput)
        {
            // ���ˮƽ����ת
            transform.Rotate(Vector3.up, _HorizonInput * RotationSensitivity * Time.fixedDeltaTime);

            // ���ͷ���ڣ�����ɴ�ֱ����ת
            if (HeadObject != null)
            {
                // ��ֱ��ת���㣬������[-90, 90]��֮��
                verticalRotation -= _VerticalInput * RotationSensitivity * Time.fixedDeltaTime;
                verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

                // ����ͷ������ľֲ���ת����Ӱ��X��
                HeadObject.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            }
        }


        /// <summary>
        /// ��ȡ�ٶ�
        /// </summary>
        /// <returns></returns>
        public Vector3 GetVelocity()
        {
            return velocity;
        }


        /// <summary>
        /// ����ٶ�
        /// �޷����̸ı��ٶȣ�����һ���AddForce���ɽ��y����
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
                    print("SetVelocity��������");
                    break;
            }

            CheckSliperVelocity();


        }


        /// <summary>
        /// ����ٶȣ�������������
        /// �޷����̸ı��ٶȣ�����һ���AddForce���ɽ��y����
        /// </summary>
        /// <param name="_v"></param>
        public void SetVelocity(BlockDirection _Direct, float _value)
        {
            // ���ݷ������ö�Ӧ���ٶȷ�����ֻ���õ�һ�����غ����޸��ض���
            switch (_Direct)
            {
                case BlockDirection.ǰ:
                    SetVelocity("x", transform.forward.x * _value);
                    SetVelocity("z", transform.forward.z * _value);
                    break;
                case BlockDirection.��:
                    SetVelocity("x", -transform.forward.x * _value);
                    SetVelocity("z", -transform.forward.z * _value);
                    break;
                case BlockDirection.��:
                    SetVelocity("x", -transform.right.x * _value);
                    SetVelocity("z", -transform.right.z * _value);
                    break;
                case BlockDirection.��:
                    SetVelocity("x", transform.right.x * _value);
                    SetVelocity("z", transform.right.z * _value);
                    break;
                default:
                    print("SetVelocity.BlockDirection��������");
                    break;
            }
        }



        /// <summary>
        /// ʵ��ֹͣ
        /// </summary>
        public void StopVelocity()
        {
            velocity = Vector3.zero;
        }


        /// <summary>
        /// ���ö���
        /// </summary>
        /// <param name="_force">˲ʱ����</param>
        public void AddForce(Vector3 _direct, float _value)
        {
            _direct = Vector3.Normalize(_direct);

            //print($"ʩ����{_force.magnitude}����");
            StartCoroutine(waitFrameToAddOtherForce(_direct, _value));

            // ����˲ʱ���ٶ�
            //Vector3 instantAcceleration = _direct.normalized * (_value / mass);

            //// �����ٶ�ֱ��Ӧ�����ٶ�
            //velocity += instantAcceleration;
        }

        
        /// <summary>
        /// ʵ��������Ծһ��
        /// </summary>
        public void EntityJump()
        {
            // ���ڵ�����ʱ������Ծ
            if (Collider_Component.isGround && isJumpRequest)
            {
                //print($"momentum:{momentum}, velocity: {velocity}");
                AddForce(Vector3.up, force_jump); // �����Ծ�ĳ���
            }
        }


        /// <summary>
        /// ʵ�����������Ծһ��
        /// </summary>
        /// <param name="_direct"></param>
        public void EntityJump(Vector3 _direct)
        {
            _direct = _direct.normalized;

            // ���ڵ�����ʱ������Ծ
            if (Collider_Component.isGround && isJumpRequest)
            {

                AddForce(_direct, force_jump); // �����Ծ�ĳ���
            }
        }

         
        /// <summary>
        /// ʵ�������Ծһ��
        /// </summary>
        /// <param name="_value"></param>
        public void EntityRandomJump(float _value)
        {
            Vector3 direction = Random.onUnitSphere * 0.5f; // ʹ���������
            direction.y = 0.9f; // ����Y��Ϊjumpheight
            Vector3 direct = direction.normalized; // ��׼������
            AddForce(direct, _value); // ʩ����
        }


        #endregion


        #region �ٶ������

        //ʵ��״̬
        [Foldout("״̬", true)]
        [Header("˲ʱ�ٶ�")][SerializeField][ReadOnly] private Vector3 velocity = Vector3.zero;
        [Header("˲ʱ����")][SerializeField][ReadOnly] private Vector3 momentum = new Vector3(0, 0, 0);
        [Header("˲ʱ��綯��")][SerializeField][ReadOnly] private Vector3 Othermomentum = Vector3.zero; //�ܻ�ʹ�����������AddMomentum(Force)

        //ʵ�����
        [Foldout("ʵ�����", true)]
        [Header("ʵ������")] public float mass = 1f;

        //�ٶȺ�������
        [Foldout("�ٶȺ�������", true)]
        [Header("ʵ���ƶ��ٶ�")] public float speed_move = 1.5f;
        [Header("ʵ���ն��½��ٶ�")] public float speedDown_ultimate = -20f;
        [Header("ʵ��ˮ���ն��ٶ�")] public float WaterspeedDown_ultimate = -2f;
        [Header("ʵ����Ծ��")] public float force_jump = 270f; 
        [Header("ʵ������")] public float force_gravity = -13f;
        [Header("ʵ��ˮ������")] public float force_Watergravity = -2f;
        [Header("ʵ����Ծ��ȴ")] public float JumpColdTime = 0.1f;  private float jumpCooldownTimer = 0f;   // ��ʱ��

        [Foldout("��ת����", true)]
        [Header("ʵ��ͷ(ѡ��)")] public GameObject HeadObject; // ͷ���������ڴ�ֱ��ת
        [Header("ʵ����ת������")] public float RotationSensitivity = 50.0f; // ������ת������

        private float verticalRotation = 0f; // ���ڴ洢��ֱ��ת���ۼ�ֵ
        //˥��ϵ��
        [Foldout("˥��ϵ��", true)]
        [Header("ˮƽĦ��ϵ�� (ԽСԽ��)")] public float Damping_Horizontal = 10f;


        //��̬�ն��ٶ�
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
        
        //��̬����
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


        
        
        // ��ʼ������
        private void Initialize()
        {
            velocity = Vector3.zero;
            momentum = new Vector3(0, force_gravity, 0);
            Othermomentum = Vector3.zero;
        }

        //Update����
        private void _ReferFixedUpdate_Caculate()
        {
            Caculate();
            ApplyPosition();
        }

        // �ܵļ���
        private void Caculate()
        {
            Caculate_Momentum();   // ���ȼ��㶯�������������Ϳ���������
            Caculate_Velocity();   // Ȼ������ٶ�
        }



        // ���㶯��
        void Caculate_Momentum()
        {
            // ����Ƿ��ڵ�����
            //if (Collider_Component.colliderDown)
            //{
            //    // �����ڵ���״̬�����ö���
            //    if (momentum.y <= force_gravity)
            //    {
            //        momentum = new Vector3(0, force_gravity, 0);
            //        return; // ����ڵ����ϣ���������������
            //    }
            //}
            momentum = Vector3.zero;

            // ��������
            momentum += Gravity;

            // ����������
            if (Othermomentum != Vector3.zero)
            {
                momentum += Othermomentum;
                Othermomentum = Vector3.zero;
                //print($"ʩ��������Moment��{momentum}");
            }

            // ����Ƿ�ﵽ�ն��ٶ�
            if (momentum.y <= Gravity.y)
            {
                momentum = Gravity;
            }
        }

        // �����ٶ�
        void Caculate_Velocity()
        {
            // ��������-����ڵ���
            if (Collider_Component.isGround && 
                Othermomentum == Vector3.zero
                )
            {
                if (momentum.y <= Gravity.y)
                {
                    // ����ֱ�ٶȹ���
                    //velocity.y = 0f;


                    CheckSliperVelocity();

                    // ˮƽ�ٶȻ�����Ϊ0
                    // ʹ������˥����Damping_Horizontal ������Զ����ˮƽ˥������
                    velocity.x = Mathf.MoveTowards(velocity.x, 0f, Damping_Horizontal * Time.fixedDeltaTime);
                    velocity.z = Mathf.MoveTowards(velocity.z, 0f, Damping_Horizontal * Time.fixedDeltaTime);

                   
                    return;
                }
                
            }

            //---------------------------��������-----------------------

            // ���ݶ���������ٶ�
            Vector3 acceleration = momentum / mass; // ���ݶ���������ٶ�

            // �����ٶ�
            velocity += acceleration * Time.fixedDeltaTime; // ʹ�ü��ٶȸ����ٶ�

            //---------------------------��ֵ����-----------------------

            // ����Ƿ�ﵽ�ն��ٶȲ������ٶ�
            CheckUltVelocity();

            //��Ĥ����
            CheckSliperVelocity();
        }

        //�ն��ٶ�����
        void CheckUltVelocity()
        {
            if (velocity.y <= Ultimate_VerticalVelocity)
            {
                // ǿ���ٶ�Ϊ�ն��ٶ�
                velocity.y = Ultimate_VerticalVelocity;
            }
        }

        //��Ĥ����
        void CheckSliperVelocity()
        {
            //��Ĥ���
            if ((velocity.z > 0 && Collider_Component.collider_Front) || (velocity.z < 0 && Collider_Component.collider_Back))
                velocity.z = 0;
            if ((velocity.x > 0 && Collider_Component.collider_Right) || (velocity.x < 0 && Collider_Component.collider_Left))
                velocity.x = 0;
            if (velocity.y > 0 && Collider_Component.collider_Up || velocity.y < 0 && Collider_Component.isGround)
                velocity.y = 0;
        }



        // �ƶ�ʵ��
        private void ApplyPosition()
        {
            // ����λ�ã�ֻʹ���ٶ�
            transform.position += velocity * Time.fixedDeltaTime;

        }










        #endregion


        #region Entity��ת

        private Vector3 targetDirection = Vector3.forward; // Ĭ�ϳ���
        private float rotationSpeed = 90f; // Ĭ����ת�ٶ�
        private bool isInstantRotation = false; // Ĭ�Ϸ�˲ʱ��ת

        /// <summary>
        /// ������ת����
        /// </summary>
        /// <param name="newTargetDirection">Ŀ����ת���򣨵�λ������</param>
        /// <param name="newRotationSpeed">��ת�ٶȣ��Ƕ�/�룩</param>
        /// <param name="isInstant">�Ƿ����������ת</param>
        public void SetRotationParameters(Vector3 newTargetDirection, float newRotationSpeed, bool isInstant = false)
        {
            targetDirection = newTargetDirection.normalized; // ����Ŀ�귽�򲢹�һ��
            rotationSpeed = newRotationSpeed;
            isInstantRotation = isInstant;
        }


        #endregion


        #region AI-�ƶ���Ŀ��λ��

        private Vector3 _targetPos_MoveTo;
        private Coroutine moveToObjectCoroutine;
        private bool isMovingToTarget;

        /// <summary>
        /// �ƶ���Ŀ������
        /// </summary>
        /// <param name="_targetObject">Ŀ������Object</param>
        public void MoveToObject(GameObject _targetObject)
        {
            Vector3 targetPos = _targetObject.transform.position;
            StartMoveCoroutine(targetPos);
        }

        /// <summary>
        /// �ƶ���Ŀ��λ��
        /// </summary>
        /// <param name="_targetPos">Ŀ��λ��Vector3</param>
        public void MoveToObject(Vector3 _targetPos)
        {
            StartMoveCoroutine(_targetPos);
        }

        // Ŀ���л�
        private void StartMoveCoroutine(Vector3 targetPos)
        {
            _targetPos_MoveTo = targetPos;

            // �ƶ���Ŀ��
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

            float TryJumpCold = 1f; // ��ȴʱ��
            float jumpCooldownTimer = 0f; // ��ȴ��ʱ��

            Vector3 Last_targetPos_MoveTo = Vector3.zero;
            Vector3 _NoY_targetPos_MoveTo = new Vector3(_targetPos_MoveTo.x, 0f, _targetPos_MoveTo.z);  //ȥ��Yֵ��Ŀ��
            transform.LookAt(_targetPos_MoveTo);

            // ��ס������
            Vector3 lastCheckedPosition = new Vector3(selfPos.x, 0f, selfPos.z);
            float stuckCheckInterval = 2f; // ÿ����һ��
            float stuckTimer = 0f;
            float stuckThreshold = 0.05f; // λ�Ʊ仯��ֵ�����С�ڸ�ֵ����Ϊ��ס
            float stuckDuration = 2f; // ��ס����ʱ��

            while (true)
            {
                Vector3 _NoY_selfPos = new Vector3(selfPos.x, 0f, selfPos.z);

                // ��������
                if (!MC_UtilityFunctions.IsTargetVisible(_targetPos_MoveTo) || // ��������
                    (_NoY_selfPos - _NoY_targetPos_MoveTo).magnitude <= 0.5f || // С����С���
                    isMovingToTarget == false) // ����Ϊֹͣ
                {
                    StopVelocity();
                    moveToObjectCoroutine = null;
                    isMovingToTarget = false;
                    yield break;
                }

                // �����������������һ�£���ֻ����ȴʱ�䵽ʱ������
                if (Collider_Component.Collider_Surround && jumpCooldownTimer <= 0f)
                {
                    //print("Jump");
                    EntityJump();
                    jumpCooldownTimer = TryJumpCold; // ������ȴʱ��
                }

                // ��ȴ��ʱ������
                if (jumpCooldownTimer > 0f)
                {
                    jumpCooldownTimer -= Time.deltaTime; // ��֡������ȴʱ��
                }

                // ����Ƿ�ס
                stuckTimer += Time.deltaTime;
                if (stuckTimer >= stuckCheckInterval)
                {
                    if (Vector3.Distance(lastCheckedPosition, _NoY_selfPos) < stuckThreshold)
                    {
                        stuckDuration -= stuckCheckInterval;
                        if (stuckDuration <= 0f)
                        {
                            //print("�ƶ�����ס��ֹͣ����");
                            StopVelocity();
                            moveToObjectCoroutine = null;
                            isMovingToTarget = false;
                            yield break;
                        }
                    }
                    else
                    {
                        stuckDuration = 2f; // ���ÿ�ס��ʱ��
                    }
                    lastCheckedPosition = _NoY_selfPos;
                    stuckTimer = 0f;
                }

                // ��;�ı�Ŀ��
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


        #region ���� 

        /// <summary>
        /// ��������
        /// </summary>
        public Vector3 selfPos
        {
            get { return transform.position; }
        }


        //�ȴ�һ֡���OtherForce
        IEnumerator waitFrameToAddOtherForce(Vector3 _direct, float _value)
        {
            yield return null;
            Othermomentum += _direct * _value;
        }



        #endregion


    }
}