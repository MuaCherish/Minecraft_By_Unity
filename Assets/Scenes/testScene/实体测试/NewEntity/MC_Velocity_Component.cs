using UnityEngine;
using System.Collections;
using MCEntity;
using Homebrew;

namespace MCEntity
{

    [RequireComponent(typeof(MC_Collider_Component))]
    public class MC_Velocity_Component : MonoBehaviour
    {

        #region ״̬

        [Foldout("״̬", true)]
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

        #region ���ں��� 

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
        [Header("ʵ���ƶ��ٶ�")] public float speed_move = 1f;
        [Header("ʵ���ն��½��ٶ�")] public float speedDown_ultimate = -10f;
        [Header("ʵ����Ծ��")] public float force_jump = 77f; 
        [Header("ʵ������")] public float force_gravity = -9.8f;


        //˥��ϵ��
        [Foldout("˥��ϵ��", true)]
        [Header("ˮƽĦ��ϵ�� (ԽСԽ��)")] public float Damping_Horizontal = 20f;

        
        // ��ʼ������
        private void Initialize()
        {
            velocity = Vector3.zero;
            momentum = new Vector3(0, force_gravity, 0);
            Othermomentum = Vector3.zero;
        }

        //Update����
        private void FixedUpdateCaculate()
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

            // ��������
            Vector3 Force_gravity = new Vector3(0, force_gravity, 0); // ����
            momentum += Force_gravity;

            // ����������
            if (Othermomentum != Vector3.zero)
            {
                momentum += Othermomentum;
                Othermomentum = Vector3.zero;
            }

            // ����Ƿ�ﵽ�ն��ٶ�
            if (momentum.y <= force_gravity)
            {
                momentum = new Vector3(0, force_gravity, 0);
            }
        }

        // �����ٶ�
        void Caculate_Velocity()
        {
            // ��������
            if (Collider_Component.isGround)
            {
                if (momentum.y <= force_gravity)
                {
                    // ����ֱ�ٶȹ���
                    velocity.y = 0f;

                    // ˮƽ�ٶȻ�����Ϊ0
                    // ʹ������˥����Damping_Horizontal ������Զ����ˮƽ˥������
                    velocity.x = Mathf.MoveTowards(velocity.x, 0f, Damping_Horizontal * Time.deltaTime);
                    velocity.z = Mathf.MoveTowards(velocity.z, 0f, Damping_Horizontal * Time.deltaTime);
                    
                    return;
                }
                
            }

            //---------------------------��������-----------------------

            // ���ݶ���������ٶ�
            Vector3 acceleration = momentum / mass; // ���ݶ���������ٶ�

            // �����ٶ�
            velocity += acceleration * Time.deltaTime ; // ʹ�ü��ٶȸ����ٶ�

            //---------------------------��ֵ����-----------------------

            // ����Ƿ�ﵽ�ն��ٶȲ������ٶ�
            if (velocity.y <= speedDown_ultimate)
            {
                // ǿ���ٶ�Ϊ�ն��ٶ�
                velocity.y = speedDown_ultimate;
            }

            CheckSliperVelocity();
        }


        void CheckSliperVelocity()
        {
            //��Ĥ���
            if ((velocity.z > 0 && Collider_Component.collider_Front) || (velocity.z < 0 && Collider_Component.collider_Back))
                velocity.z = 0;
            if ((velocity.x > 0 && Collider_Component.collider_Right) || (velocity.x < 0 && Collider_Component.collider_Left))
                velocity.x = 0;
        }



        //�ƶ�ʵ��
        private void ApplyPosition()
        {
            // ����λ�ã�ֻʹ���ٶ�
            transform.position += velocity * Time.deltaTime;
        }




        /// <summary>
        /// ʵ��ֹͣ
        /// </summary>
        public void StopVelocity()
        {
            velocity = Vector3.zero;
        }


        /// <summary>
        /// ����ٶ�
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
        /// ���ö���
        /// </summary>
        /// <param name="_force">˲ʱ����</param>
        public void AddForce(Vector3 _force)
        {
            //print($"ʩ����{_force.magnitude}����");
            Othermomentum += _force;
        }


        /// <summary>
        /// ʵ����Ծ
        /// </summary>
        /// <param name="_force">˲ʱ����</param>
        public void EntityJump()
        {
            // ���ڵ�����ʱ������Ծ
            if (Collider_Component.isGround)
            {

                AddForce(force_jump * Vector3.up); // �����Ծ�ĳ���
            }
        }
        public void EntityJump(Vector3 _direct)
        {
            _direct = _direct.normalized;

            // ���ڵ�����ʱ������Ծ
            if (Collider_Component.isGround)
            {

                AddForce(force_jump * _direct); // �����Ծ�ĳ���
            }
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


        


        #endregion


    }
}