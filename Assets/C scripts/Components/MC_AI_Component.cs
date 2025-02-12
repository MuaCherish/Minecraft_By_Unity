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


        #region ״̬

        [Foldout("״̬��", true)]
        [Header("��ǰ״̬")][ReadOnly] public AIState currentState = AIState.Idle;
        [Header("Idle")][ReadOnly] public IdleState current_IdleState = IdleState.Wait;
        [Header("Chase")][ReadOnly] public ChaseState current_ChaseState = ChaseState.Wait;
        [Header("Flee")][ReadOnly] public FleeState current_FleeState = FleeState.Wait;
        [Header("�Ƿ������")][ReadOnly] public bool isSeePlayer;

        [Foldout("ʵ������", true)]
        [Header("�ƶ���ʽ")] public AIMovingType currentMovingType = AIMovingType.Walk;
        [Header("�Ƿ���й�����")] public bool isAggressive;

        #endregion


        #region ���ں���

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


        #region AI����״̬��

        /// <summary>
        /// �趨AI״̬
        /// </summary>
        /// <param name="_state"></param>
        public void SetAIState(AIState _state)
        {
            currentState = _state;
        }

        //�ܿ�����
        void _ReferUpdate_AIState_Controller()
        {
            //��ǰ����-���ʵ���Ѿ�����
            if (Life_Component.isEntity_Dead)
                return;

            AutoUpdateAIState();
            ApplyAIState();
        }

        //�Զ�����״̬
        void AutoUpdateAIState()
        {
            //������ʵ��״̬�Զ�����
            if (isAggressive)
            {
                //��ǰ����-�����������ģʽ
                if (world.game_mode != GameMode.Survival)
                    return;

                //������ҽ���׷��״̬
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

        //״̬����
        void ApplyAIState()
        {

            //��ǰ����-��ͣAI�
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

        [Foldout("Idle/Jump����", true)]
        [Header("Idle��ԾYֵ")] public float IdleJumpY = 1f;
        [Header("Wait״̬�ȴ���Χ")] public Vector2 IdlewaitTimeRange = new Vector2(3f, 7f);
        [Header("IdleMoving����")] public float IdleMovingProbability = 0.6f;
        [Header("��Ծ���ȷ�Χ")] public Vector2 IdleJumpForceRange = new Vector2(100f, 200f);
        private Vector3 targetDirection = Vector3.zero;
        private bool isWaiting = false;  //hasExec_Wait
        private float waitTime = 0f;  // �ȴ�ʱ��
        private float waitStartTime = 0f;  // ��¼��ʼʱ��
        private float rotateStartTime = -1f;    // �ȴ���ʼʱ�䣬��ʼ��Ϊ-1����ʾ��δ��ʼ�ȴ�


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
            // �����δ��ʼ�ȴ������õȴ�ʱ�䲢��¼��ʼʱ��
            if (!isWaiting)
            {
                // �������
                Vector3 direction = (Random.onUnitSphere * 0.5f).normalized;
                direction.y = IdleJumpY;  // ����Y��Ϊjumpheight
                targetDirection = direction;

                // ��������ȴ�ʱ��
                waitTime = Random.Range(IdlewaitTimeRange.x, IdlewaitTimeRange.y);
                waitStartTime = Time.time;  // ��¼��ʼ�ȴ���ʱ��
                isWaiting = true;  // ����ѿ�ʼ�ȴ�
            }

            // ����ȴ�ʱ���ѵ�
            if (Time.time - waitStartTime >= waitTime)
            {
                current_IdleState = IdleState.Rotate;  // ��ת״̬
                isWaiting = false;
            }
        }

        void Handle_IdleState_JumpType_Rotate()
        {
            // ���㵱ǰ������Ŀ�귽��ĽǶȲ����XZƽ����ת��
            Vector3 currentForward = Velocity_Component.ModelObject.transform.forward;
            currentForward.y = 0;  // ����y��Ϊ0��ȷ��ֻ��XZƽ�����
            float angle = Vector3.SignedAngle(currentForward, targetDirection, Vector3.up);  // ����ǶȲ�

            // ������ת�ٶȺͽǶȲ������תʱ��
            float rotateWaitTime = Mathf.Abs(angle) / Velocity_Component.rotationSpeed;  // ��ת����ʱ��

            // ���û�п�ʼ�ȴ�����¼��ʼʱ��
            if (rotateStartTime == -1f)
            {
                rotateStartTime = Time.time;
                Velocity_Component.EntitySmoothRotation(targetDirection, rotateWaitTime);  // ��ʼ��ת
            }

            // ����ȴ�ʱ��δ�ﵽ��ת����ʱ��
            if (Time.time - rotateStartTime >= rotateWaitTime)
            {
                // ��ת��ɺ�����л�״̬
                float randomValue = Random.value;  // ����һ��0��1֮��������

                // ���ݸ����ж���һ��״̬
                if (randomValue < IdleMovingProbability)
                    current_IdleState = IdleState.Moving;
                else
                    current_IdleState = IdleState.Wait;

                rotateStartTime = -1f;  // ���ÿ�ʼʱ��
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
            //����ȴ�

            //ȷ����һ�ε���·��
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

        [Foldout("Chase/Jump����", true)]
        [Header("׷��״̬��ԾYֵ")] public float ChaseJumpY = 1f;
        [Header("׷��״̬��Ծ���ȷ�Χ")] public Vector2 ChaseJumpForceRange = new Vector2(180f, 200f);
        [Header("׷��״̬��Ϣʱ�䷶Χ")] public Vector2 ChaseWaitTimeRange = new Vector2(1f, 3f);


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
            //�ȴ�ChaseWaitTimeRange��Χ
            if (!isWaiting)
            {
                waitTime = Random.Range(ChaseWaitTimeRange.x, ChaseWaitTimeRange.y);  // ��������ȴ�ʱ��
                waitStartTime = Time.time;  // ��¼��ʼ�ȴ���ʱ��
                isWaiting = true;  // ����ѿ�ʼ�ȴ�
            }

            // ����ȴ�ʱ���ѵ�
            if (Time.time - waitStartTime >= waitTime)
            {
                current_ChaseState = ChaseState.Moving;
                isWaiting = false;
            }

        }

        void Handle_ChaseState_JumpType_Moving()
        {
            //��ǰ����-û��ؾͲ�����
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
        /// AI���ܣ��������ʱ�����Ƶ�
        /// </summary>
        public void EntityFlee()
        {
            //��ǰ����-����Ѿ�������
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
            if (currentState == AIState.Flee)  //�����Chase״̬���ö���
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

        [Foldout("Flee/Jump����", true)]
        [Header("����ʱ��")] public Vector2 JumpFleeTimeRange = new Vector2(10f, 20f);
        [Header("Flee��ԾYֵ")] public float FleeJumpY = 0.5f;
        [Header("Wait״̬�ȴ���Χ")] public Vector2 FleewaitTimeRange = new Vector2(3f, 7f);
        [Header("FleeMoving����")] public float FleeMovingProbability = 0.6f;
        [Header("��Ծ���ȷ�Χ")] public Vector2 FleeJumpForceRange = new Vector2(100f, 200f);

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
            // �����δ��ʼ�ȴ������õȴ�ʱ�䲢��¼��ʼʱ��
            if (!isWaiting)
            {
                // �������
                Vector3 direction = (Random.onUnitSphere * 0.5f).normalized;
                direction.y = FleeJumpY;  // ����Y��Ϊjumpheight
                targetDirection = direction;

                // ��������ȴ�ʱ��
                waitTime = Random.Range(FleewaitTimeRange.x, FleewaitTimeRange.y);
                waitStartTime = Time.time;  // ��¼��ʼ�ȴ���ʱ��
                isWaiting = true;  // ����ѿ�ʼ�ȴ�
            }

            // ����ȴ�ʱ���ѵ�
            if (Time.time - waitStartTime >= waitTime)
            {
                current_FleeState = FleeState.Rotate;  // ��ת״̬
                isWaiting = false;
            }
        }

        void Handle_FleeState_JumpType_Rotate()
        {
            // ���㵱ǰ������Ŀ�귽��ĽǶȲ����XZƽ����ת��
            Vector3 currentForward = Velocity_Component.ModelObject.transform.forward;
            currentForward.y = 0;  // ����y��Ϊ0��ȷ��ֻ��XZƽ�����
            float angle = Vector3.SignedAngle(currentForward, targetDirection, Vector3.up);  // ����ǶȲ�

            // ������ת�ٶȺͽǶȲ������תʱ��
            float rotateWaitTime = Mathf.Abs(angle) / Velocity_Component.rotationSpeed;  // ��ת����ʱ��

            // ���û�п�ʼ�ȴ�����¼��ʼʱ��
            if (rotateStartTime == -1f)
            {
                rotateStartTime = Time.time;
                Velocity_Component.EntitySmoothRotation(targetDirection, rotateWaitTime);  // ��ʼ��ת
            }

            // ����ȴ�ʱ��δ�ﵽ��ת����ʱ��
            if (Time.time - rotateStartTime >= rotateWaitTime)
            {
                // ��ת��ɺ�����л�״̬
                float randomValue = Random.value;  // ����һ��0��1֮��������

                // ���ݸ����ж���һ��״̬
                if (randomValue < FleeMovingProbability)
                    current_FleeState = FleeState.Moving;
                else
                    current_FleeState = FleeState.Wait;

                rotateStartTime = -1f;  // ���ÿ�ʼʱ��
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

        [Foldout("Flee/Walk����", true)]
        [Header("����ʱ��")] public Vector2 WalkFleeTimeRange = new Vector2(10f, 20f);

        void Handle_FleeState_WalkType() 
        {

        }

        #endregion


        #endregion



        #endregion


        #region AI����


        #region AI�Զ���Ծ

        #endregion


        #region AIѰ·

        [Foldout("AIѰ·", true)]
        [Header("�����м�ڵ�ľ������")] public float nodeAcceptanceDistance = 0.25f;
        [Header("�����ڵ����Ѱ·�ȴ�ʱ��")] public float maxNavigationWaitTime = 3f;

        /// <summary>
        /// ʵ�嵼��
        /// </summary>
        public void EntityNavigation()
        {

        } 


        IEnumerator EntityNavigation_Old(List<Vector3> _Nodes)
        {
            //AI����Ŀ�ĵ�

            //��������ÿһ���ڵ�
            foreach (var _pos in _Nodes)
            {
                //��ʱ
                float _moveTime = 0;

                //��_pos�ƶ�ֱ�����С��nodeAcceptanceDistance
                while (true)
                {
                    //�����ʱ��ֹͣ�ƶ�

                    //���ʵ��ĳ���

                    //�ƶ�

                    //end
                    _moveTime += Time.deltaTime;
                    yield return null;
                }
            }

            
        }


        #endregion


        #region AI�Ӿ�

        [Foldout("AI�Ӿ�", true)]
        [Header("�Ƿ��ע�����")] public bool canWatchPlayer;
        [Header("������Χ")] public float visionDistance = 10f;
        [Header("���תһ��ͷ")] public float WatchrotateTime = 0.2f;
        private float WatchTimer = 0f;

        void _ReferUpdate_VisionSystem()
        {
            // ���ʵ������Ҿ���С�� VisionDistance
            float _dis = (player.cam.transform.position - Collider_Component.EyesPoint).magnitude;

            if (_dis <= visionDistance)
            {
                // ÿ֡�������߼�⣬���� isSeePlayer ״̬
                Vector3 _direct = player.cam.transform.position - Collider_Component.EyesPoint;
                RayCastStruct _rayCast = player.NewRayCast(Collider_Component.EyesPoint, _direct, _dis, Registration_Component.EntityID);

                // ��ǽ�����򿴲���
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
            //��ǰ����-�������ע�����
            if (!canWatchPlayer)
                return;

            // ��ǰ���� - �����Զ����û�������
            float _dis = (player.transform.position - transform.position).magnitude;
            if (_dis > visionDistance || isSeePlayer == false)
                return;

            //��ǰ����-�����ͣAI�
            if (Debug_PauseAI)
                return;

            // ��ǰ���� - �����ʱ��δ�ﵽ��תʱ��
            if (WatchTimer < WatchrotateTime)
            {
                WatchTimer += Time.deltaTime;  // �ۼӼ�ʱ��
                return;  // �ȴ��´���ת
            }

            // ��ת
            Vector3 playerPosition = player.transform.position;
            Vector3 direction = (playerPosition - transform.position).normalized;
            Velocity_Component.EntitySmoothRotation(direction, WatchrotateTime);

            // ���ü�ʱ�����ȴ��´���ת
            WatchTimer = 0f;
        }
        
        #endregion


        #region AI��Ӿ

        [Foldout("AI��Ӿ", true)]
        [Header("��ˮ�л����ϸ�")] public bool AI_CanSwiming;
        [Header("���ʱ��")] public float Floating_CheckTime = 0.1f; private float lastCheckTime = 0f;
        [Header("�ϸ���")] public float FloatingForce = 5f;

        void _ReferUpdate_AISwimming()
        {
            // ���������Ӿ����ʱ�䵽������
            if (AI_CanSwiming && Time.time - lastCheckTime >= Floating_CheckTime)
            {
                // �����ϴμ���ʱ��
                lastCheckTime = Time.time;

                // ���������ˮ��������һ��
                if (Collider_Component.IsInTheWater(transform.position))
                {
                    Velocity_Component.AddForce(Vector3.up, FloatingForce);
                }
            }
        }


        #endregion


        #region AI����

        [Foldout("AI����", true)]
        [Header("������Χ������")] public float AttackRangeMultiplier = 1.2f;
        [Header("�����˺�ֵ")] public int AttackDamage = 3;
        [Header("������ȴ")] public float AttackColdTime = 0.5f;
        private float lastAttackTime = 0f;  // ��¼�ϴι���ʱ��

        void _ReferUpdate_AIAttack()
        {
            // ��ǰ����-�����������ģʽ
            if (world.game_mode != GameMode.Survival)
                return;

            // ��ǰ����-����׷��״̬
            if (currentState != AIState.Chase)
                return;

            // ��ǰ����-ʵ����������̫Զ��
            float _dis = (transform.position - player.transform.position).magnitude;
            float _maxDis = Mathf.Abs(Collider_Component.hitBoxWidth - player.playerWidth);
            if (_dis > _maxDis)
                return;

            // ����Ƿ�����ȴ״̬�������ȴʱ�仹û�����Ͳ�����
            if (Time.time - lastAttackTime < AttackColdTime)
                return;

            // �������һ�ι���ʱ��
            lastAttackTime = Time.time;

            // ���ʵ������ҵ���ײ�У�����������˺�
            Vector3 hitVec = player.CheckHitBox(transform.position, Collider_Component.hitBoxWidth * AttackRangeMultiplier, Collider_Component.hitBoxHeight * AttackRangeMultiplier);
            if (hitVec != Vector3.zero)
            {
                //print("�ɹ������");
                hitVec.y = 1f;
                player.ForceMoving(hitVec, 2.5f, 0.2f);
                Collider_Component.managerhub.lifeManager.UpdatePlayerBlood(AttackDamage, true, true);
            }
        }


        #endregion


        #endregion


        #region Debug


        [Foldout("Debug", true)]
        [Header("��ʾ��������")] public bool Debug_DrawRay;
        [Header("��ͣAi�")] public bool Debug_PauseAI;

        void OnDrawGizmos()
        {
            //��ǰ����-���û�п�debug
            if (!Debug_DrawRay)
                return;

            //��������
            Vector3 _direct = player.cam.transform.position - Collider_Component.EyesPoint;
            float _dis = (player.cam.transform.position - Collider_Component.EyesPoint).magnitude;
            if (_dis <= visionDistance)
            {

                // ��������
                if (isSeePlayer)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.green;
                Gizmos.DrawRay(Collider_Component.EyesPoint, _direct.normalized * _dis);  // ���۾�λ�õ����λ�õ�����

                // ���ƾ�����
                if (isSeePlayer)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, visionDistance);

            }
            else
            {
                // ���ƾ�����
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, visionDistance);
            }

            //������Χ����
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
    #region ״̬

    [Foldout("״̬��", true)]
    [Header("AI״̬��")][ReadOnly] public AIState myState;

    [Foldout("AI״̬", true)]
    [Header("���ڹ���")][ReadOnly] public bool isAttacking;
    [Header("�ɹ������")][ReadOnly] public bool isSucceded_HitPlayer;

    #endregion


    #region ���ں���

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


    #region AI״̬��


    //״̬��������
    Coroutine Coroutine_AIState_Controller;
    IEnumerator Corou_AIState_Controller()
    {

        while (true)
        {
            // ��ǰ���� - ���û�й�����
            // ��ǰ���� - ����Ǵ���ģʽ
            if (isAggressive == false || world.game_mode == GameMode.Creative)
            {
                yield return null; // �ó�����Ȩ��������ѭ��
                continue;
            }

            //��ʵ���۾�������۾�����һ������
            Vector3 _direct = player.cam.transform.position - Collider_Component.EyesPoint;
            RayCastStruct _rayCast = player.NewRayCast(Collider_Component.EyesPoint, _direct, AIseeDistance, Registration_Component.EntityID);

            //print(_rayCast.rayDistance);

            //���֮��û�з������ڿ��ӷ�Χ�� ����׷��
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



            //�ȴ�1s
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


    #region AI����

    [Foldout("AIͨ������", true)]
    [Header("AI�ƶ���ʽ")] public AIMovingType myMovingType = AIMovingType.Walk;

    [Foldout("������AI", true)]
    [Header("�Ƿ���й�����")] public bool isAggressive = false;
    [Header("AI���Ӿ���")] public float AIseeDistance = 20f;
    [Header("Idle״̬����ӳ�")] public float Idle_WaitTime = 1f;
    [Header("Chase״̬����ӳ�")] public float Chase_WaitTime = 5f;

    [Foldout("�ǹ�����AI", true)]
    [Header("�Ƿ������")] public bool isCanFlee = false;
    [Header("����ʱ��")] public float fleeTime = 5f;
    [Header("�����ٶ�")] public float fleeSpeed = 5f;
    [Header("��ת�ٶ�")] public float RotationSpeed = 0.3f;


    #endregion


    #region AI�ƶ�

    Coroutine Coroutine_AIMoving;

    //�Զ���Ծ
    void _ReferUpdate_AutoJump()
    {
        if (myMovingType == AIMovingType.Walk && Collider_Component.Collider_Surround)
        {


            if (Random.Range(0f, 1f) < 0.01f) // ÿ0.05���ʸı����ܷ���
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

            //ʷ��ķ
            if (myMovingType == AIMovingType.Jump)
            {
                if (myState == AIState.Idle)
                {
                    //�ӳ�������
                    float waitTime = Random.Range(3f, 7f);
                    yield return new WaitForSeconds(waitTime);

                    //�������
                    Vector3 direction = Random.onUnitSphere * 0.5f; // ʹ���������
                    direction.y = 0.9f; // ����Y��Ϊjumpheight
                    Vector3 direct = direction.normalized; // ��׼������

                    //�������
                    float force = Velocity_Component.force_jump;

                    //ת����Ծ����
                    if (!Life_Component.isEntity_Dead)
                    {
                        Velocity_Component.EntitySmoothRotation(direct, 0.7f);
                        yield return new WaitForSeconds(0.7f);
                    }


                    //��Ծ
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

                    // �ӳ������루ģ����Ծ�����ȴʱ�䣩
                    float waitTime = Random.Range(2f, 4f);
                    yield return new WaitForSeconds(waitTime);

                    // ��ȡ���λ��
                    Vector3 playerPosition = player.transform.position;

                    // ���㷽�򣺳������
                    Vector3 direction = (playerPosition - transform.position).normalized;
                    direction.y = 0.9f; // ����Y��Ϊjumpheight��ȷ����Ծ�������һ���߶�

                    // �̶�����
                    float force = Random.Range(100, 150);

                    // ��Ծ
                    if (!Life_Component.isEntity_Dead)
                    {
                        StartCoroutine(Corou_WaitForSecond("isAttacking", 1f));
                        Velocity_Component.AddForce(direction, force);
                    }


                }
            }
            //��
            else if (myMovingType == AIMovingType.Walk)
            {
                // �ӳ�������
                float waitTime = Random.Range(3f, 7f);
                yield return new WaitForSeconds(waitTime);

                // �������
                Vector3 direction = Random.onUnitSphere; // ʹ���������
                direction.y = 0f; // ȷ���������ϻ������ƶ�
                Vector3 direct = direction.normalized; // ��׼������

                if (!Life_Component.isEntity_Dead)
                {
                    Velocity_Component.EntitySmoothRotation(direct, 0.7f);
                    yield return new WaitForSeconds(0.7f);
                }

                // ����ƶ�ʱ��
                float walkTime = Random.Range(5f, 15f);
                float elapsedTime = 0f;

                // ��÷����ƶ��涨ʱ��
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
            Velocity_Component.EntitySmoothRotation(direction, 0.25f); // ������ת�����
        }

        Coroutine_AlwaysLookAtPlayer = null;



    }



    #endregion


    #region AI����


    void _ReferUpdate_AIAttack()
    {
        //��ǰ����-�����������ģʽ
        if (world.game_mode != GameMode.Survival)
            return;

        //��ǰ����-��ͣAI�
        if (Debug_PauseAI)
            return;

        //���AI������ҵ���һ����Χ�������ڷ�������ʱ
        //ʱ�̼��ʵ������ҵ���ײ�У�����������˺�
        Player player = Collider_Component.managerhub.player;
        float _dis = (transform.position - player.transform.position).magnitude;
        float _maxDis = Mathf.Abs(Collider_Component.hitBoxWidth - player.playerWidth);
        if (_dis < _maxDis && isAttacking)
        {
            Vector3 hitVec = player.CheckHitBox(transform.position, Collider_Component.hitBoxWidth, Collider_Component.hitBoxHeight);
            if (hitVec != Vector3.zero && !isSucceded_HitPlayer)
            {
                StartCoroutine(Corou_WaitForSecond("isSucceded_HitPlayer", 1f));
                //print("�ɹ������");
                hitVec.y = 1f;
                player.ForceMoving(hitVec, 2.5f, 0.2f);
                Collider_Component.managerhub.lifeManager.UpdatePlayerBlood(6, true, true);
            }
        }
    }

    #endregion


    #region AI����

    //תΪ����ģʽ
    public void SwitchFleeState()
    {
        //��ǰ����-�����ͣAi�
        if (Debug_PauseAI)
        {
            return;
        }
        //print("����");
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

        // ����ʱ���ѡ��һ�����ܷ���
        fleeDirection = Random.insideUnitSphere;
        fleeDirection.y = 0; // ȷ�����ܷ�����ˮƽ����
        fleeDirection.Normalize(); // ��׼������

        // ��ʼʱ��ת�������ܷ���
        Velocity_Component.EntitySmoothRotation(fleeDirection, 0.3f);

        while (_time < fleeTime)
        {
            _time += Time.deltaTime;

            // ÿһ֡�������ܷ������
            Velocity_Component.SetVelocity("x", fleeDirection.x * fleeSpeed * 1.5f);
            Velocity_Component.SetVelocity("z", fleeDirection.z * fleeSpeed * 1.5f);

            // ���Կ��Ǹ�AI��һЩ������أ�ʹ�����ܲ���ô����
            if (Random.Range(0f, 1f) < 0.001f) // ÿ0.05���ʸı����ܷ���
            {
                fleeDirection = Random.insideUnitSphere;
                fleeDirection.y = 0; // ����ˮƽ����
                fleeDirection.Normalize();

                // ÿ�θı����ܷ���ʱ������ת
                Velocity_Component.EntitySmoothRotation(fleeDirection, 0.3f);
            }

            yield return null;
        }

        // ���ܽ������л���Idle״̬
        Animator_Component.isRun = false;
        myState = AIState.Idle;
        Coroutine_AIFlee = null;
    }




    #endregion


    #region AI�ϸ�

    [Foldout("�ϸ�����", true)]
    [Header("��ˮ�л����ϸ�")] public bool AI_CanSwiming;
    [Header("���ʱ��")] public float Floating_CheckTime = 1f; private float lastCheckTime = 0f;
    [Header("�ϸ���")] public float FloatingForce = 1f;


    void _ReferUpdate_AICompetent()
    {
        // ���������Ӿ����ʱ�䵽������
        if (AI_CanSwiming && Time.time - lastCheckTime >= Floating_CheckTime)
        {
            // �����ϴμ���ʱ��
            lastCheckTime = Time.time;

            // ���������ˮ��������һ��
            if (Collider_Component.IsInTheWater(Collider_Component.EyesPoint))
            {
                Velocity_Component.AddForce(Vector3.up, FloatingForce);
            }
        }
    }



    #endregion


    #region Debug

    [Foldout("Debug", true)]
    [Header("�����ߵ���")] public bool Debug_ShowEyesRayCast;
    [Header("��ͣAi�")] public bool Debug_PauseAI;

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