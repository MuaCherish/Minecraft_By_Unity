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
    [SerializeField]
    public enum AIState
    {
        Idle,       
        Chase,
        Flee,
    }

    [SerializeField]
    public enum AIMovingType
    {
        JumpType,
        WalkType,
        FlyType,
    }

    [RequireComponent(typeof(MC_Velocity_Component))]
    [RequireComponent(typeof(MC_Collider_Component))]
    [RequireComponent(typeof(MC_Life_Component))]
    [RequireComponent(typeof(MC_Animator_Component))]
    public class MC_AI_Component : MonoBehaviour
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
        [Header("AI�ƶ���ʽ")] public AIMovingType myMovingType = AIMovingType.WalkType;

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
            if (myMovingType == AIMovingType.WalkType && Collider_Component.Collider_Surround)
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
                if (myMovingType == AIMovingType.JumpType)
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
                else if (myMovingType == AIMovingType.WalkType)
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
}


public class MC_AI_Slime_Component : MonoBehaviour
{

    #region ����

    [Foldout("״̬")]
    [Header("��ǰ״̬")] public AIState currentState = AIState.Idle;
    [Header("�Ƿ������")] [ReadOnly] public bool isSeePlayer;


    #endregion


    #region ���ں���

    MC_Collider_Component Collider_Component;
    World world;

    private void Awake()
    {
        Collider_Component = GetComponent<MC_Collider_Component>();
        world = Collider_Component.managerhub.world;
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
        StateController();
        AIVision();
    }


    #endregion


    #region ״̬��������

    
    void StateController()
    {
        switch (currentState)
        {
            case AIState.Idle:
                OnIdle();
                break;

            case AIState.Chase:
                OnChase();
                break;

            case AIState.Flee:
                OnFlee();
                break;
        }
    }

    
    #endregion


    #region Idle

    void OnIdle()
    {

    }

    #endregion


    #region Chase

    void OnChase()
    {

    }



    #endregion


    #region Flee

    void OnFlee()
    {

    }

    #endregion


    #region �Ӿ�ϵͳ

    [Foldout("AI�Ӿ�")]
    [Header("������Χ")] public float VisionDistance = 10f;


    void AIVision()
    {
        //���ʵ������Ҿ���С��VisionDistance

        //ÿ֡�������߼��,����isSeePlayer״̬
    }

    #endregion


    #region Debug

    #endregion


}