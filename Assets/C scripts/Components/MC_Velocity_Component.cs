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


        #region ��������

      

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

            //print($"ǿ��λ��{_direct * _value}");

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
        private bool RequestforEntityJump = true;
        public void EntityJump()
        {
            // ���ڵ�����ʱ������Ծ
            if (Collider_Component.isGround && RequestforEntityJump)
            {
                //print($"momentum:{momentum}, velocity: {velocity}");
                AddForce(Vector3.up, force_jump); // �����Ծ�ĳ���
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

        //�ٶȺ�������
        [Foldout("�ٶȺ�������", true)]
        [Header("ʵ���ƶ��ٶ�")] public float speed_move = 1.5f;
        [Header("ʵ���ն��½��ٶ�")] public float speedDown_ultimate = -20f;
        [Header("ʵ��ˮ���ն��ٶ�")] public float WaterspeedDown_ultimate = -2f;
        [Header("ʵ����Ծ��")] public float force_jump = 270f; 
        [Header("ʵ��������")] public float force_hurt = 125f; 
        [Header("ʵ������")] public float force_gravity = -13f;
        [Header("ʵ��ˮ������")] public float force_Watergravity = -2f;
        [Header("ʵ����Ծ��ȴ")] public float JumpColdTime = 0.1f;  private float jumpCooldownTimer = 0f;   // ��ʱ��

        [Foldout("��ת����", true)]
        [Header("��ת�ٶ�")] public float rotationSpeed = 90f; // Ĭ����ת�ٶ�

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
            Vector3 acceleration = momentum; // ���ݶ���������ٶ�

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



        //private Vector3 targetDirection = Vector3.forward; // Ĭ�ϳ���
        //private bool isInstantRotation = false; // Ĭ�Ϸ�˲ʱ��ת

        /// <summary>
        /// ��Entityת��ĳ�����򣬽���תXZƽ�棬����Yֵ���䣬ƽ������
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

        [Foldout("ʵ����ת", true)]
        [Header("�����ͷ����ʱ��ٷֱ�")] public float BodySlowRotationPercent = 1.4f;
        private Coroutine _rotationCoroutine;  // ���ڱ��浱ǰ��ת��Э��

        public void EntitySmoothRotation(Vector3 _direct, float _elapseTime)
        {
            // �����ǰ������תЭ���ڽ��У����ظ�����
            if (_rotationCoroutine == null)
            {
                _rotationCoroutine = StartCoroutine(SmoothRotationCoroutine(_direct, _elapseTime));
            }
        }

        private IEnumerator SmoothRotationCoroutine(Vector3 _direct, float _elapseTime)
        {
            // ��Ŀ�귽���Yֵ����Ϊ0��ȷ��ֻ��XZƽ����ת
            _direct.y = 0;

            // ȷ��Ŀ�귽���г��ȣ������쳣
            if (_direct.sqrMagnitude < 0.0001f)
                yield break;

            // ����Ŀ�귽�����ת��Ԫ��
            Quaternion targetRotation = Quaternion.LookRotation(_direct);

            // ��¼ͷ��������ĳ�ʼ��ת
            Quaternion initialHeadRotation = _Head.transform.rotation;
            Quaternion initialBodyRotation = _Body.transform.rotation;

            // �ۼ�ʱ��
            float elapsedTime = 0f;
            float headRotationTime = _elapseTime;  // ͷ������תʱ��
            float bodyRotationTime = _elapseTime * BodySlowRotationPercent;  // �������תʱ��

            // whileѭ���и����������תʱ����п���
            while (elapsedTime < bodyRotationTime)
            {
                // ��ǰ����-���ʵ��������
                if (life_Component != null && life_Component.isEntity_Dead)
                    yield break;

                elapsedTime += Time.deltaTime;

                // ͷ����ת����ͷ����תʱ��ƽ����ת�� - ֻ����ͷ��û�����ʱ����ת
                if (elapsedTime < headRotationTime)
                {
                    _Head.transform.rotation = Quaternion.Slerp(
                        initialHeadRotation,
                        targetRotation,
                        elapsedTime / headRotationTime
                    );
                }
                // ������ת
                _Body.transform.rotation = Quaternion.Slerp(
                    initialBodyRotation,
                    targetRotation,
                    Mathf.Clamp01(elapsedTime / bodyRotationTime) // ����������ת
                );

                // �ȴ���һ֡
                yield return null;
            }

            // ȷ������ͷ�������嶼��׼Ŀ�귽��
            _Head.transform.rotation = targetRotation;
            _Body.transform.rotation = targetRotation;

            // Э�̽�����������תЭ�̱�־
            _rotationCoroutine = null;
        }



        /// <summary>
        /// ʵ��������ת��ת
        /// ����ֵ��[-1,1]֮��
        /// </summary>
        public void EntityRotation(float _HorizonInput)
        {
            // ���ˮƽ����ת
            _Model.transform.Rotate(Vector3.up, _HorizonInput * Time.fixedDeltaTime);
        }


        /// <summary>
        /// ͷ��ת
        /// ����ֵ��[-1,1]֮��
        /// </summary>
        public void EntityHeadVerticleRotation(float _VerticleInput, Vector2 _CameraLimit, Vector2 _HeadLimit)
        {
            // ���㵱ǰ�Ĵ�ֱ��ת�Ƕ�
            float currentVerticalAngle = _Head.transform.localRotation.eulerAngles.x;

            // ����ת�Ƕ�������0��360֮��
            if (currentVerticalAngle > 180)
                currentVerticalAngle -= 360;

            // �����µ���ת�Ƕ�
            float newVerticalAngle = currentVerticalAngle - _VerticleInput * Time.fixedDeltaTime;

            // �����µ���ת�Ƕ���[-90, 90]��Χ��
            newVerticalAngle = Mathf.Clamp(newVerticalAngle, _HeadLimit.x, _HeadLimit.y);

            // ʹ�� Quaternion.Euler �����µ���ת
            _Head.transform.localRotation = Quaternion.Euler(newVerticalAngle, 0f, 0f);
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