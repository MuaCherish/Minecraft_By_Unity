using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static MC_Tool_Navigation;

namespace MCEntity
{

    [RequireComponent(typeof(MC_Velocity_Component))]
    [RequireComponent(typeof(MC_Collider_Component))]
    [RequireComponent(typeof(MC_Registration_Component))]
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
        [Header("�Ƿ��׷�����")] public bool isAggressive;
        [Header("�Ƿ������")] public bool EntityCanFlee = true;

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
            world = Collider_Component.managerhub.world;
            player = Collider_Component.managerhub.player;
            Animator_Component = GetComponent<MC_Animator_Component>();
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
            if (Life_Component != null && Life_Component.isEntity_Dead)
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

                //��ǰ����-�����������򷵻�Idle״̬
                if (player.isDead)
                {
                    currentState = AIState.Idle;
                    return;
                }

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
            if (Debug_PauseAI || Life_Component.isEntity_Dead)
                return;

            switch (currentState)
            {
                case AIState.Idle:
                    OnIdleState();

                    //Chase
                    if (current_ChaseState != ChaseState.Wait)
                        current_ChaseState = ChaseState.Wait;
                    //Flee
                    if (current_FleeState != FleeState.Wait)
                        current_FleeState = FleeState.Wait;

                    break;

                case AIState.Chase:
                    OnChaseState();

                    //Idle
                    if (current_IdleState != IdleState.Wait)
                        current_IdleState = IdleState.Wait;
                    //Flee
                    if (current_FleeState != FleeState.Wait)
                        current_FleeState = FleeState.Wait;
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
            OnIdleState_Update();
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

        //����һЩIdle�ı���
        void OnIdleState_Update()
        {
            //��ǰ����-�������Idle״̬
            if (currentState != AIState.Idle)
                return;

            _AutoWatchToPlayer();
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
            Vector3 currentForward = Collider_Component.EntityFaceForward;
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


        [Foldout("Idle/Walk����", true)]
        [Header("IdleWalk�ȴ�ʱ��")] public Vector2 IdleWalkWaitTimeRange = new Vector2(3, 20f);
        [Header("�������-��������")] public int IdleWalk_RandomWalk_Steps = 10;
        [Header("�������-��ѭ�ϴη���ĸ���")] public float IdleWalk_PrevDirectionProbability = 0.8f;

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
            //һ���Ժ���
            if(hasExec_Handle_IdleState_WalkType_Wait)
            {
                // ��������ȴ�ʱ��
                waitTime = Random.Range(IdleWalkWaitTimeRange.x, IdleWalkWaitTimeRange.y);
                waitStartTime = Time.time;  // ��¼��ʼ�ȴ���ʱ��
                isWaiting = true;  // ����ѿ�ʼ�ȴ�

                hasExec_Handle_IdleState_WalkType_Wait = false;
            }

            // ����ȴ�ʱ���ѵ�
            if (Time.time - waitStartTime >= waitTime)
            {
                current_IdleState = IdleState.Moving;  // ��ת״̬
                isWaiting = false;
            }

        }

        void Handle_IdleState_WalkType_Moving()
        {
            //һ���Ժ���
            if (hasExec_Handle_IdleState_WalkType_Moving)
            {

                //��ȡ������ߺ������ص�·��
                Vector3 _StartPos = Collider_Component.FootPoint + new Vector3(0f, 0.125f, 0f);
                Algo_RandomWalk(_StartPos, IdleWalk_RandomWalk_Steps, IdleWalk_PrevDirectionProbability, out List<Vector3> _Result);
                TargetPos = _Result[_Result.Count - 1];
                EntityMoveTo(TargetPos, Velocity_Component.speed_move, currentState);
                isReachTargetPos = false;

                hasExec_Handle_IdleState_WalkType_Moving = false;
            }


            //While-�ȴ��ߵ�Ŀ�ĵ�
            if (isReachTargetPos)
            {
                //�ߵ�Ŀ�ĵ�
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


        bool hasExec_Handle_ChaseState_WalkType_Moving = true;
        void Handle_ChaseState_WalkType()
        {
            Handle_ChaseState_WalkType_Moving();
        }


        void Handle_ChaseState_WalkType_Moving()
        {
            if (hasExec_Handle_ChaseState_WalkType_Moving)
            {

                //һֱ׷�����
                EntityMoveTo(player.gameObject, Velocity_Component.speed_move);

                hasExec_Handle_ChaseState_WalkType_Moving = false;
            }
            
        }

        #endregion


        #endregion


        #region FleeState

        /// <summary>
        /// AI���ܣ��������ʱ�����Ƶ�
        /// </summary>
        public void EntityFlee()
        {
            //��ǰ����-�����ͣAI��Ϊ
            if (Debug_PauseAI)
                return;

            // ��ǰ���� - ����Ѿ���������
            if (Corou_FleeTimer != null)
            {
                // �����ӳ�����ʱ��
                StopCoroutine(Corou_FleeTimer);
                Corou_FleeTimer = StartCoroutine(FleeTimer(currentFleeTime));
                return;
            }

            // ��ʼ����ʱ��
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

            // ���õ�ǰ����ʱ��
            currentFleeTime = _fleeTime;

            // �������ܼ�ʱ��
            Corou_FleeTimer = StartCoroutine(FleeTimer(currentFleeTime));
        }

        Coroutine Corou_FleeTimer;
        float currentFleeTime;

        IEnumerator FleeTimer(float _fleeTime)
        {
            currentState = AIState.Flee;
            yield return new WaitForSeconds(_fleeTime);

            // ���ܽ���
            if (currentState == AIState.Flee)
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
            Vector3 currentForward = Collider_Component.EntityFaceForward;
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
        [Header("����ʱ��")] public Vector2 WalkFleeTimeRange = new Vector2(5f, 10f);
        [Header("�����ٶ�")] public float WalkFleeSpeed = 5f;
        [Header("Flee/Walk�ȴ�ʱ��")] public Vector2 FleeWalkWaitTimeRange = new Vector2(0f, 2f);
        [Header("�������-��������")] public int FleeWalk_RandomWalk_Steps = 10;
        [Header("�������-��ѭ�ϴη���ĸ���")] public float FleeWalk_PrevDirectionProbability = 0.8f;

        bool hasExec_Handle_FleeState_WalkType_Wait = true;
        bool hasExec_Handle_FleeState_WalkType_Moving = true;
        void Handle_FleeState_WalkType() 
        {
            switch (current_FleeState)
            {
                case FleeState.Wait:
                    Handle_FleeState_WalkType_Wait();
                    if (!hasExec_Handle_FleeState_WalkType_Moving)
                        hasExec_Handle_FleeState_WalkType_Moving = true;
                    break;

                case FleeState.Moving:
                    Handle_FleeState_WalkType_Moving();
                    if (!hasExec_Handle_FleeState_WalkType_Wait)
                        hasExec_Handle_FleeState_WalkType_Wait = true;
                    break;
            }
        }


        void Handle_FleeState_WalkType_Wait()
        {
            if (hasExec_Handle_FleeState_WalkType_Wait)
            {

                // ��������ȴ�ʱ��
                waitTime = Random.Range(FleeWalkWaitTimeRange.x, FleeWalkWaitTimeRange.y);
                waitStartTime = Time.time;  // ��¼��ʼ�ȴ���ʱ��
                isWaiting = true;  // ����ѿ�ʼ�ȴ�

                hasExec_Handle_FleeState_WalkType_Wait = false;
            }

            // ����ȴ�ʱ���ѵ�
            if (Time.time - waitStartTime >= waitTime)
            {
                current_FleeState = FleeState.Moving;  // ��ת״̬
                isWaiting = false;
            }
        }

        void Handle_FleeState_WalkType_Moving()
        {
            if (hasExec_Handle_FleeState_WalkType_Moving)
            {

                //��ȡ������ߺ������ص�·��
                Vector3 _StartPos = Collider_Component.FootPoint + new Vector3(0f, 0.125f, 0f);
                Algo_RandomWalk(_StartPos, FleeWalk_RandomWalk_Steps, FleeWalk_PrevDirectionProbability, out List<Vector3> _Result);
                TargetPos = _Result[_Result.Count - 1];
                EntityMoveTo(TargetPos, WalkFleeSpeed, currentState);
                isReachTargetPos = false;

                hasExec_Handle_FleeState_WalkType_Moving = false;
            }


            //While-�ȴ��ߵ�Ŀ�ĵ�
            if (isReachTargetPos)
            {
                //�ߵ�Ŀ�ĵ�
                current_FleeState = FleeState.Wait;
            }
        }


        #endregion


        #endregion



        #endregion


        #region AI����


        #region AI�Զ���Ծ

        [Foldout("AI�Զ���Ծ", true)]
        [Header("AI�Ƿ���Զ���Ծ")] public bool isAIcanAutoJump = true;
        [Header("ǰ����������ֵ")] public float AutoJump_CheckMaxDistance = 1;
        [Header("�Զ���Ծ��ȴʱ��")] public float AutoJump_ColdTime = 0.5f; private float AutoJumpTimer = 0f;

        void _ReferUpdate_AutoJump()
        {
            // ��ǰ����-���AI������Ծ
            if (!isAIcanAutoJump)
                return;

            // ��ǰ����-���AIû���ƶ�
            if (!Velocity_Component.isMoving)
                return;

            // ���ȵ���ǰ����������
            Vector3 _Origin = Collider_Component.FootPoint + new Vector3(0f, 0.125f, 0f);
            Vector3 _Direct = Collider_Component.EntityFaceForward;
            MC_RayCastStruct _RayCast = MC_Tool_Raycast.RayCast(world, MC_RayCast_FindType.OnlyFindBlock, _Origin, _Direct, AutoJump_CheckMaxDistance, Registration_Component.GetEntityId()._id, 0.1f);
            //print(_RayCast);

            // ���û��⵽�����򷵻�
            if (_RayCast.isHit != 1)
                return;

            // �����ȴʱ��δ����������
            if (AutoJumpTimer < AutoJump_ColdTime)
            {
                AutoJumpTimer += Time.deltaTime; // ����ȴʱ�������Ӽ�ʱ��
                return;
            }

            // ʵ�崥����Ծ����������ȴ
            //print("ʵ����Ծ");
            Velocity_Component.EntityJump();

            // ��Ծ��ʼ��ȴ
            AutoJumpTimer = 0f;  // ��Ծ��ſ�ʼ��ʱ
        }


        #endregion


        #region AI�Ӿ�

        [Foldout("AI�Ӿ�", true)]
        [Header("�Ƿ��ע�����")] public bool canWatchPlayer;
        [Header("����������Χ")] public float visionDistance = 15f;
        [Header("׷��״̬��ת��ʱ��")] public float ChaseVisionrotateTime = 0.2f;
        [Header("ע��������Χ")] public float WatchvisionDistance = 3f;
        [Header("ע����ҵ�ת��ʱ��")] public float WatchPLayerrotateTime = 0.4f;
        private float WatchTimer = 0f;

        void _ReferUpdate_VisionSystem()
        {
            // ���ʵ������Ҿ���С�� VisionDistance
            float _dis = (player.cam.transform.position - Collider_Component.EyesPoint).magnitude;

            if (_dis <= visionDistance)
            {
                // ÿ֡�������߼�⣬���� isSeePlayer ״̬
                Vector3 _direct = player.cam.transform.position - Collider_Component.EyesPoint;
                MC_RayCastStruct _rayCast = MC_Tool_Raycast.RayCast(world, MC_RayCast_FindType.OnlyFindBlock,Collider_Component.EyesPoint, _direct, _dis, Registration_Component.GetEntityId()._id, 0.1f);

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
            if (WatchTimer < ChaseVisionrotateTime)
            {
                WatchTimer += Time.deltaTime;  // �ۼӼ�ʱ��
                return;  // �ȴ��´���ת
            }

            // ��ת
            Vector3 playerPosition = player.transform.position;
            Vector3 direction = (playerPosition - transform.position).normalized;
            Velocity_Component.EntitySmoothRotation(direction, ChaseVisionrotateTime);

            // ���ü�ʱ�����ȴ��´���ת
            WatchTimer = 0f;
        }


        void _AutoWatchToPlayer()
        {
            //��ǰ����-���������ģʽ�Ҿ߱�������������
            if (world.game_mode == GameMode.Survival && isAggressive)
                return;

            //��ǰ����-�������Idle-Wait�׶�
            if(current_IdleState != IdleState.Wait)
            {
                canWatchPlayer = false;
                visionDistance = WatchvisionDistance;
                ChaseVisionrotateTime = WatchPLayerrotateTime;
            }

            //AI�ڵȴ��ڼ����ע�����
            canWatchPlayer = true;
            visionDistance = WatchvisionDistance;
            ChaseVisionrotateTime = WatchPLayerrotateTime;
        }

        #endregion


        #region AI�˶�

        [Foldout("AI�˶�", true)]
        [Header("�����м�ڵ�ľ������")] public float nodeAcceptanceDistance = 1f;

        /// <summary>
        /// ʵ���ƶ���Ŀ�귽��
        /// </summary>
        private Coroutine _EntityMoveToCoroutine;
        void EntityMoveTo(Vector3 _TargetPos, float _Speed, AIState _LastState)
        {
            if(_EntityMoveToCoroutine == null)
                _EntityMoveToCoroutine = StartCoroutine(EntityMoveToCoroutine(_TargetPos, _Speed, _LastState));
        }
        IEnumerator EntityMoveToCoroutine(Vector3 _TargetPos, float _Speed, AIState _LastState)
        {
            //Ԥ����
            //Vector3 _currentForward = Velocity_Component.EntityForward.normalized;
            Vector3 _Direct = (_TargetPos - transform.position).normalized;
            float startTime = 0f;
            float maxNavigationWaitTime = (_TargetPos - transform.position).magnitude / _Speed;

            //��Ƕ������Walk
            if (Animator_Component != null && Animator_Component.CanWalk)
                Animator_Component.SetSpeed(_Speed);
            

            while (true)
            {
                //��ǰ�˳�-����Ŀ�ĵ�
                if ((transform.position - _TargetPos).magnitude < nodeAcceptanceDistance ||
                    startTime > maxNavigationWaitTime)  //��ʱ
                {
                    isReachTargetPos = true;
                    break;
                }

                //��ǰ����-�������׷��״̬������ֹͣ
                if (currentState == AIState.Chase || Debug_PauseAI)
                {
                    _EntityMoveToCoroutine = null;
                    yield break;
                }

                //��ǰ����-����ϴ���Idle״̬����ǰ״̬��ΪFlee������ֹͣ
                if(_LastState == AIState.Idle && currentState == AIState.Flee)
                {
                    _EntityMoveToCoroutine = null;
                    yield break;
                }

                //��Forward�ƶ�
                Velocity_Component.SetVelocity("x", _Direct.x * _Speed);
                Velocity_Component.SetVelocity("z", _Direct.z * _Speed);

                //Rotation
                _Direct = (_TargetPos - transform.position).normalized;
                Velocity_Component.EntitySmoothRotation(_Direct, 0.2f);

                //end
                startTime += Time.deltaTime;
                yield return null;
            }

            _EntityMoveToCoroutine = null;
        }

        /// <summary>
        /// ʵ��׷�����
        /// </summary>
        void EntityMoveTo(GameObject _TargetPos, float _Speed)
        {
            StartCoroutine(EntityMoveToCoroutine(_TargetPos, _Speed));
        }
        IEnumerator EntityMoveToCoroutine(GameObject _TargetPos, float _Speed)
        {
            //Ԥ����
            //Vector3 _currentForward = Velocity_Component.EntityForward.normalized;
            float startTime = 0f;
            float maxNavigationWaitTime = (_TargetPos.transform.position - transform.position).magnitude / _Speed;

            //��Ƕ������Walk
            if (Animator_Component != null && Animator_Component.CanWalk)
                Animator_Component.SetSpeed(_Speed);


            while (true)
            {
                //��ǰ�˳�-����Ŀ�ĵ�
                //�����׷��ģʽ�򲻽�����Щ�ж�
                if (currentState != AIState.Chase || Debug_PauseAI)
                {
                    if ((transform.position - _TargetPos.transform.position).magnitude < nodeAcceptanceDistance ||
                    startTime > maxNavigationWaitTime)  //��ʱ
                    {
                        isReachTargetPos = true;
                        break;
                    }
                }

                Vector3 _Direct = (_TargetPos.transform.position - transform.position).normalized;
                Velocity_Component.EntitySmoothRotation(_Direct, 0.5f);

                //��Forward�ƶ�
                Velocity_Component.SetVelocity("x", _Direct.x * _Speed);
                Velocity_Component.SetVelocity("z", _Direct.z * _Speed);

                //end
                startTime += Time.deltaTime;
                yield return null;
            }

            //End
            hasExec_Handle_ChaseState_WalkType_Moving = true;  //�����Ҫ��
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
        [Header("�Ƿ��ܴ�����Χ����")] public bool canAttack;
        [Header("������Χ������(xΪ���,yΪ�߶�)")] public Vector2 AttackRangeMultiplier = new Vector2(1.2f, 1.2f);
        [Header("�����˺�ֵ")] public int AttackDamage = 3;
        [Header("������ȴ")] public float AttackColdTime = 1f;
        private float lastAttackTime = 0f;  // ��¼�ϴι���ʱ��

        void _ReferUpdate_AIAttack()
        {
            // ��ǰ����-���û�й�����
            if (!isAggressive)
                return;

            // ��ǰ����-�����������ģʽ
            if (world.game_mode != GameMode.Survival)
                return;

            // ��ǰ����-����׷��״̬
            if (currentState != AIState.Chase)
                return;

            //��ǰ����-������ܴ�������
            if (!canAttack)
                return;

            // ��ǰ����-ʵ����������̫Զ��
            //float _dis = (transform.position - player.transform.position).magnitude;
            //float _maxDis = Mathf.Abs(Collider_Component.hitBoxWidth - player.playerWidth);
            //if (_dis > _maxDis)
            //    return;

            // ÿ֡�������Ƿ��ڹ�����Χ��
            Vector3 hitVec = player.CheckHitBox(transform.position, Collider_Component.hitBoxWidth * AttackRangeMultiplier.x, Collider_Component.hitBoxHeight * AttackRangeMultiplier.y);

            if (hitVec != Vector3.zero && Time.time - lastAttackTime >= AttackColdTime)
            {
                // �����⵽��ײ����ȴʱ���ѹ�����ִ�й���
                lastAttackTime = Time.time; // ���¹���ʱ��

                // ��������ƶ����˺�
                hitVec.y = 1f; // ȷ�����������ʵ�
                player.ForceMoving(hitVec, 2.5f, 0.2f);
                Collider_Component.managerhub.lifeManager.UpdatePlayerBlood(AttackDamage, true, true);

                // ���Ź�������
                if (Animator_Component != null)
                    Animator_Component.PlayAttackAnimation();

                // ��ӡ������Ϣ����ѡ��
                // print("�ɹ������");
            }
        }



        #endregion


        #endregion


        #region Debug


        [Foldout("Debug", true)]
        [Header("��ʾ��������")] public bool Debug_DrawRay;
        [Header("��ͣAi�")] public bool Debug_PauseAI;

        /// <summary>
        /// ��ͣAI��Ϊ
        /// </summary>
        public void PauseAI(bool _bool)
        {
            Debug_PauseAI = _bool;
        }

        void OnDrawGizmos()
        {
            //������
            if (world == null || world.game_state != Game_State.Playing)
                return;

            //��ǰ����-���û�п�debug
            if (!Debug_DrawRay)
                return;

            //����Ŀ�ĵ�
            if (!isReachTargetPos)
                Gizmos.DrawWireCube(TargetPos, new Vector3(0.8f, 0.8f, 0.8f));

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
                float _width = Collider_Component.hitBoxWidth * AttackRangeMultiplier.x;
                float _height = Collider_Component.hitBoxHeight * AttackRangeMultiplier.y;
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, new Vector3(_width, _height, _width));
            }


        }

        #endregion


    }
}
 