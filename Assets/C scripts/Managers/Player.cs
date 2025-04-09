using Homebrew;
using MCEntity;
using System.Collections;
using UnityEngine;
using static MC_Static_Math;
using static MC_Static_BlocksFunction;

public class Player : MonoBehaviour
{

    //���״̬
    [Foldout("���״̬", true)]    
    [ReadOnly] public bool isGrounded;
    [ReadOnly] public bool isSprinting;
    [ReadOnly] public bool isSwiming;
    [ReadOnly] public bool isMoving;
    [ReadOnly] public bool isSquating;
    [ReadOnly] public bool isFlying;
    [ReadOnly] public bool isCatchBlock;
    [ReadOnly] public bool isInCave; // ����Ƿ��ڿ���
    [ReadOnly] public bool isPause;
    [ReadOnly] public bool isBroking;
    [ReadOnly] public bool isFirstBrokeBlock = true;  //����ģʽ�������˲�����ٷ���
    [ReadOnly] public bool isInputing;  //����ֹͣ
    [ReadOnly] public bool NotCheckPlayerCollision;
    [ReadOnly] public bool isHitEntity = false; //��ʵ����
    [ReadOnly] public bool isSpectatorMode; 
    [ReadOnly] public bool isDead; 
    [ReadOnly] public bool ShowEntityHitbox;


    [Foldout("Transforms", true)]
    [Header("Transforms")]
    ManagerHub managerhub;
    public CommandManager commandManager;
    MC_Service_World Service_World;
    public MusicManager musicmanager;
    public BackPackManager backpackmanager;
    public LifeManager lifemanager;


    public GameObject particle_explosion;

    public Material HighLightMaterial;
    public Texture[] DestroyTextures = new Texture[10];
    public Transform cam;
    //public Animation camaraAnimation;
    public Transform HighlightBlock;
    //public GameObject HighlightBlockObject;

    public GameObject eyesObject;
    public Transform leg;
    public GameObject selectblock;
    public GameObject Eye_Light;

    public GameObject Particle_Broken;
    public Transform particel_Broken_transform;
    //public ParticleSystem Broking_Animation;
    public GameObject Particle_Broking;

    //Hand
    public GameObject hand_Hold;
    public GameObject hand;
    public GameObject handBlock;
    public GameObject handTool;


    [Header("��ɫ����")]
    public Transform foot; 
    //private bool hasExec = true;
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float squatWalkSpeed = 1f;
    public float squatSpeed = 3f;
    public float jumpForce = 6f;
    public float gravity = -15f;
    //public float mass = 1f;
    public float MaxHurtHigh = 7f;
    public float gravity_V = 0.2f; //��������


    [Header("��ײ����")]
    public float playerWidth = 0.3f;
    public float playerHeight = 1.7f;
    public float high_delta = 0.9f; // ��ȷ���߶��£���ײ�����Ը߶�
    private float extend_delta = 0.1f;
    private float delta = 0.05f;


    [Header("���������Ҫ��ʱ��")]
    //public float destroyTime = 2f;
    // ���ڸ�������Ƿ������
    //private bool isLeftMouseDown;
    //�Ѿ���ȥ��ʱ��
    private float elapsedTime = 0.0f;
    private Material material; // ����Ĳ���
    private Color initialColor; // ��ʼ��ɫ
    private bool isDestroying;
    public bool isChangeBlock = false;
    public Vector3 OldPointLocation;


    //����
    [HideInInspector]
    public float horizontalInput;
    [HideInInspector]
    public float verticalInput;
    [HideInInspector]
    public float scrollWheelInput;
    private float mouseHorizontal;
    private float mouseVerticalspeed;
    private float Camera_verticalInput;
    public Vector3 velocity;
    //public float Max_verticalMomentum;
    public float verticalMomentum = 0;
    public Vector3 momentum = Vector3.zero; // ���˲ʱ����
    private bool jumpRequest;


    //raycast
    
    [HideInInspector]
    public bool isPlacing = false;
    private float max_hand_length = 0.7f;


    //debug
    [Header("debug")]
    public bool Show_CollisionBox = false;
    public bool Show_HitBox = false;
    public float water_jumpforce = 3f;
    public float watergravity = -3f;
    public float mult = 2f;


    //ˤ���˺�
    [Header("ˤ�����")]
    private float groundTime; public float minGroundedTime = 0.1f; // ��С�����ʱ��
    Coroutine falldownCoroutine;
    public float hurtCooldownTime = 0.5f;  //ˤ����ȴʱ��
    public float new_foot_high = -100f;
    [Header("������ͷ���Ƕ�")] public float angle = 50f;
    public float cycleLength = 16f; // �������ڳ���
    public float speed = 200f; // ����ʱ��������ٶ� 


    //music
    [Header("���ָ��ķ���")]
    public byte point_Block_type = 255;


    //����ģʽ
    [Header("����mode")]
    public bool isSpaceMode = false;
    public bool isSuperMining = false;


    //select
    [Header("���ѡ���±�")]
    public int selectindex = 0;


    //����ʱ�ı��Ӿ�
    [Header("�Ӿ����")]
    public Camera eyes;
    public float viewduration = 1f; // ����ʱ��
    public float CurrentFOV = 70;
    bool expandview = false;


    [Header("����ģʽ")]
    private float lastJumpTime;
    public float doubleTapInterval = 0.5f; // ����˫��ʱ����
    public float flyVelocity_Verticle = 0.1f; //���·����ٶ�
    public float flyVelocity_Horizon_mult = 1.5f; //ˮƽ�����ٶ�
    public float flyLerpTime = 0.3f;
    public bool jump_press = false;


    //��ײ��������
    // ������ĸ���
    Vector3 up_���� = new Vector3();
    Vector3 up_���� = new Vector3();
    Vector3 up_���� = new Vector3();
    Vector3 up_���� = new Vector3();


    // ������ĸ���
    Vector3 down_���� = new Vector3();
    Vector3 down_���� = new Vector3();
    Vector3 down_���� = new Vector3();
    Vector3 down_���� = new Vector3();


    //front
    Vector3 front_���� = new Vector3();
    Vector3 front_���� = new Vector3();
    Vector3 front_���� = new Vector3();
    Vector3 front_���� = new Vector3();


    //back
    Vector3 back_���� = new Vector3();
    Vector3 back_���� = new Vector3();
    Vector3 back_���� = new Vector3();
    Vector3 back_���� = new Vector3();


    //left
    Vector3 left_���� = new Vector3();
    Vector3 left_���� = new Vector3();
    Vector3 left_���� = new Vector3();
    Vector3 left_���� = new Vector3();


    //right
    Vector3 right_���� = new Vector3();
    Vector3 right_���� = new Vector3();
    Vector3 right_���� = new Vector3();
    Vector3 right_���� = new Vector3();


    //��ҽ�������
    [Header("��ҽ�������")] public byte foot_BlockType = VoxelData.Air; private byte foot_BlockType_temp = VoxelData.Air;


    //�������isInCave
    [Header("��Ѩ״̬�����ʱ��")] public float isInCave_checkInterval = 1f; private float isInCave_nextCheckTime = 1f;// ÿ0.5����һ��

    //TNTʵ��
    public GameObject Entity_TNT;

    //--------------------------------- ���ں��� --------------------------------------

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        Service_World = managerhub.Service_World;
    }


    void Start()
    {

        InitPlayerManager();
    }

    public void InitPlayerManager()
    {
        HighLightMaterial.color = new Color(0, 0, 0, 0);
        HighLightMaterial.mainTexture = DestroyTextures[0];

        //X = [800,1600]
        //Z = [400,800]
        


        //��ʼ������
        isSwiming = false;
        new_foot_high = -100f;

        isSuperMining = false;
        isFlying = false;
        momentum = Vector3.zero;


        footPos = Vector3.zero;
        previous_footPos = Vector3.zero;
        walkingDistance = 0f;
        //accumulatedDistance = 0f;
        //managerhub.backpackManager.ChangeBlockInHand();
    }

    public void RandomPlayerLocaiton()
    {
        transform.position = new Vector3(Random.Range(800, 3200), transform.position.y, Random.Range(800, 3200));
        Service_World.Start_Position = transform.position;
    }

    
    private void FixedUpdate()
    {

        if (Service_World.game_state == Game_State.Playing)
        {

            //���㱥ʳ��
            DynamicFood();

            //������ҽ�������
            Update_FootBlockType();

            



            //�������״̬
            GetPlayerState();



            //������ײ��
            if (Show_CollisionBox)
            {

                Draw_CollisionBox();

            }

            //�����ж���
            if (Show_HitBox)
            {

                Draw_HitBox();

            }


            
            //���Թ���ģʽ
            if (!isSpectatorMode)
            {

                if (transform.position.y < -20f)
                {
                    lifemanager.UpdatePlayerBlood(100, true, true);
                }


                placeCursorBlocks();
            }
            

        }



    }


    
    private void Update()
    {
        if (Service_World.game_state == Game_State.Playing)
        {

            if (isGrounded)
            {
                groundTime += Time.deltaTime; // ����ڵ����ϵ�ʱ��
            }
            else
            {
                groundTime = 0;
            }

            //�ı��Ӿ�(������ܵĻ�)
            change_eyesview();

            //������ײ��
            CollisionNumber = 0;

            if (!NotCheckPlayerCollision)
            {
                update_block();
            }

            //��Ϸ����ͣ����ͣ�������
            if (managerhub.canvasManager.isPausing == true || managerhub.chatManager.isInputing == true)
            {
                horizontalInput = 0f;
                verticalInput = 0f;
                mouseHorizontal = 0f;
                mouseVerticalspeed = 0f;

            }

            //Playing
            else
            {

                GetPlayerInputs();
                

            }


            //������������
            CalculateVelocity();

            //ʵ�ֲ���
            AchieveInput();


            //AdjustPlayerToGround();

            
            //DynamicState_isCave();

        }


        
    }




    //��С��ʳ��
    [Header("��ʳ�Ȳ���")]
    private bool hasExec_FixedUpdate = true;
    private Vector3 footPos;
    private Vector3 previous_footPos;
    public float walkingDistance;
    //public float accumulatedDistance; //�ۼ��ߵ�·��
    public void DynamicFood()
    {
        if (Service_World.game_mode == GameMode.Survival)
        {
            


            //����
            if (isSprinting)
            {
                //��ʼ��
                if (hasExec_FixedUpdate)
                {
                    footPos = foot.position;
                    previous_footPos = foot.position;
                    walkingDistance = 0f;
                    //accumulatedDistance = 0f;
                    hasExec_FixedUpdate = false;
                }


                footPos = foot.position;
                walkingDistance += (footPos - previous_footPos).magnitude;
                previous_footPos = foot.position;
            }
            else
            {
                hasExec_FixedUpdate = true;
            }
            

            //��ʳ��˥��
            if (walkingDistance >= 10f)
            {
                managerhub.lifeManager.UpdatePlayerFood(0.2f, false);
                walkingDistance = 0f;
            }
        }
    }




    #region DynamicState_isCave

    void DynamicState_isCave()
    {
        // ÿ��һ��ʱ����һ��
        if (Time.time >= isInCave_nextCheckTime)
        {
            //print($"{Time.time}");
            CheckisInCave();
            isInCave_nextCheckTime = Time.time + isInCave_checkInterval; // ������һ�μ���ʱ��
        }
    }

    //����isInCave״̬
    public void CheckisInCave()
    {
        //print("");
        float playerY = cam.position.y + 10f;
        Vector3 RelaPosition = GetRelaPos(cam.position);
        Vector3 _ChunkLocation = GetRelaChunkLocation(cam.position);
        //print($"RelaPosition: {RelaPosition} , ChunkLocation = {_ChunkLocation}");
        float NoiseY = MC_Static_Noise.GetTotalNoiseHigh_Biome((int)RelaPosition.x, (int)RelaPosition.z, new Vector3((int)_ChunkLocation.x * 16f, 0f, (int)_ChunkLocation.z * 16f), managerhub.Service_Saving.worldSetting.worldtype, Service_World._biomeProperties);

        //print($"PlayerY��{playerY} , NoiseY��{NoiseY} , ��: {NoiseY - playerY}");


        // ����۾�����λ���Ƿ��ڵر����£���Fog��Ϊ�������ɫ����
        if (playerY < NoiseY || eyes.transform.position.y < 0)
        {
            if (!isInCave)
            {
                //print("������");
                // ��ʼ������ɵ���Ѩ״̬
                //managerhub.timeManager.Buff_CaveFog(true);
                isInCave = true; 
            }
        }
        else
        {
            if (isInCave)
            {
                //print("����ر�");
                // ��ʼ������ɵ�����״̬
                //if (!managerhub.timeManager.isNight)
                //{
                //    managerhub.timeManager.Buff_CaveFog(false);
                //}
                
                isInCave = false;
            }
        }
    }

    #endregion



    //---------------------------------------------------------------------------------






    //--------------------------------- ��Ҳ��� --------------------------------------

    //������������ֹ�ŵ״�ģ

    //private void AdjustPlayerToGround()
    //{
    //    if (isGrounded && hasExec_AdjustPlayerToGround)
    //    {
    //        print("����һ������");
    //        Vector3 myposition = transform.position;
    //        Vector3 Vec = GetRelaPos(foot.position);
    //        transform.position = new Vector3(myposition.x, Vec.y + 1.95f, myposition.z);
    //        hasExec_AdjustPlayerToGround = false;
    //    }
    //}


    //���ݼ���
    private void CalculateVelocity()
    {
        //���������
        if (isGrounded && isFlying && !NotCheckPlayerCollision)
        {
            isFlying = false;
            StartCoroutine(expandchangeview(false));
        }

        //����ӽǴ���
        Camera_verticalInput -= mouseVerticalspeed;
        Camera_verticalInput = Mathf.Clamp(Camera_verticalInput, -90f, 90f);

        // ��������

        if (!isFlying)
        {
            if (!isSwiming)
            {
                //verticalMomentum += Time.fixedDeltaTime * gravity * gravity_V;
                if (verticalMomentum > gravity)
                    verticalMomentum += Time.deltaTime * gravity * gravity_V;

            }
            else
            {

                if (verticalMomentum > watergravity)
                    verticalMomentum += mult * Time.deltaTime * watergravity;

            }
        }
        else
        {
            verticalMomentum = 0f;
        }
        


        // �����ٶ�
        if (isSprinting)
        {
        
            velocity = ((transform.forward * verticalInput) + (transform.right * horizontalInput)) * Time.deltaTime * sprintSpeed;
        
        }
        else if (isSquating)
        {
            
            velocity = ((transform.forward * verticalInput) + (transform.right * horizontalInput)) * Time.deltaTime * squatWalkSpeed;
        
        }
        else
        {
        
            velocity = ((transform.forward * verticalInput) + (transform.right * horizontalInput)) * Time.deltaTime * walkSpeed;
        
        }


        // �ϲ�����
        velocity += momentum * Time.deltaTime;
        velocity += Vector3.up * verticalMomentum * Time.deltaTime;



        //��Ĥ����
        //ǰ��
        if (!NotCheckPlayerCollision)
        {
            if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
                velocity.z = 0;

            //����
            if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
                velocity.x = 0;
        }

        


        //����Ƿ��ƶ�
        if (velocity.x != 0f || velocity.z != 0f)
        {

            isMoving = true;

        }
        else
        {

            isMoving = false;

        }

        //��Ծ��ǽ�ж�

        if (velocity.y < 0)
        {

            velocity.y = checkDownSpeed(velocity.y);

        }

        else if (velocity.y > 0)
        {

            velocity.y = checkUpSpeed(velocity.y);

        }


    }


    //�ı��Ӿ�
    private Coroutine currentViewChangeCoroutine; // ��¼��ǰ�������е�Э��

    void change_eyesview()
    {
        if (isSprinting && isMoving && !expandview)
        {
            // �����Э���������У���ֹͣ��
            if (currentViewChangeCoroutine != null)
            {
                StopCoroutine(currentViewChangeCoroutine);
            }

            // ����Э��������Ұ
            currentViewChangeCoroutine = StartCoroutine(expandchangeview(true));
            expandview = true;
        }
        else if ((!isSprinting || !isMoving) && expandview && !isFlying)
        {
            // �����Э���������У���ֹͣ��
            if (currentViewChangeCoroutine != null)
            {
                StopCoroutine(currentViewChangeCoroutine);
            }

            // ����Э����С��Ұ
            currentViewChangeCoroutine = StartCoroutine(expandchangeview(false));
            expandview = false;
        }
    }

    IEnumerator expandchangeview(bool expand)
    {
        float startTime = Time.time;
        float initialFOV = eyes.fieldOfView; // ��ǰ��FOVֵ
        float targetFOV = expand ? CurrentFOV + 20f : CurrentFOV; // Ŀ��FOVֵ

        while (Time.time - startTime < viewduration)
        {
            float t = (Time.time - startTime) / viewduration;
            eyes.fieldOfView = Mathf.Lerp(initialFOV, targetFOV, t);
            yield return null;
        }

        eyes.fieldOfView = targetFOV; // ȷ��������Ұֵ׼ȷ
        currentViewChangeCoroutine = null; // Э�̽�������ռ�¼
    }


    // ����ϵ����Խ�����Խ����
    [Header("�������")]
    public float inputInertia = 0.1f;
    public float Flying_inputInertia = 0.3f;

    // ���ڴ洢��һ�ε�����ֵ
    private float horizontalInputSmooth;
    private float verticalInputSmooth;


    //���ղ���
    [HideInInspector] public bool hasExec_isChangedBlock = true;
    public Vector3 test_Normal;
    private void GetPlayerInputs()
    {

        //��ǰ����-������������ֹͣ��������
        if (isDead)
            return;

        // ��ȡ��ǰ������ֵ
        float currentHorizontalInput = Input.GetAxis("Horizontal");
        float currentVerticalInput = Input.GetAxis("Vertical");

        // ����Ƿ�������
        if (currentHorizontalInput != 0 || currentVerticalInput != 0)
        {
            isInputing = true; // ������
        }
        else
        {
            isInputing = false; // û������
        }


        //�����ٶ�����
        if (isFlying)
        {
            //flyVelocity_Horizon_mult
            currentHorizontalInput *= flyVelocity_Horizon_mult;
            currentVerticalInput *= flyVelocity_Horizon_mult;

        }


        // ʹ�� Mathf.Lerp �����������ƽ������
        if (!isFlying) 
        {
            horizontalInputSmooth = Mathf.Lerp(horizontalInputSmooth, currentHorizontalInput, Time.deltaTime / inputInertia);
            verticalInputSmooth = Mathf.Lerp(verticalInputSmooth, currentVerticalInput, Time.deltaTime / inputInertia);
        }
        else
        {
            horizontalInputSmooth = Mathf.Lerp(horizontalInputSmooth, currentHorizontalInput, Time.deltaTime / Flying_inputInertia);
            verticalInputSmooth = Mathf.Lerp(verticalInputSmooth, currentVerticalInput, Time.deltaTime / Flying_inputInertia);
        }

        // ������ֵ�жϣ�ȷ��΢С������ֱ�ӹ���
        if (Mathf.Abs(horizontalInputSmooth) < 0.01f)
        {
            horizontalInputSmooth = 0f;
        }

        if (Mathf.Abs(verticalInputSmooth) < 0.01f)
        {
            verticalInputSmooth = 0f;
        }

        // ����ʹ��ƽ������������ֵ
        horizontalInput = horizontalInputSmooth;
        verticalInput = verticalInputSmooth;

        // ��ȡ������루�޹��ԣ�
        mouseHorizontal = Input.GetAxis("Mouse X") * managerhub.canvasManager.Mouse_Sensitivity;
        mouseVerticalspeed = Input.GetAxis("Mouse Y") * managerhub.canvasManager.Mouse_Sensitivity;

        scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");

        //��Ұ�һ��R������л�һ�����е���Ʒ
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    placeBlock_Index = (byte)Random.Range(0,25);
        //}

        //��Ұ�һ��F�����л��ֵ�
        if (Input.GetKeyDown(KeyCode.F))
        {

            Eye_Light.SetActive(!Eye_Light.activeSelf);

        }

        //����ײ��
        if (Input.GetKeyDown(KeyCode.F6))
        {
            ShowEntityHitbox = !ShowEntityHitbox;
        }


        //�Թ���ģʽ
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (isSpectatorMode)
            {
                isSpectatorMode = false ;
                SpectatorMode(false);
            }
            else
            {
                isSpectatorMode = true;
                SpectatorMode(true);
            }
            
        }


        if (Input.GetKeyDown(KeyCode.LeftShift) && !isFlying)
        {

            isSprinting = true;
            managerhub.OldMusicManager.footstepInterval = PlayerData.sprintSpeed;

        }

        if (managerhub.lifeManager.SprintLock)
        {
            isSprinting = false;
        }
            
        if (Input.GetKeyUp(KeyCode.LeftShift) && !isFlying)
        {

            isSprinting = false;
            managerhub.OldMusicManager.footstepInterval = PlayerData.walkSpeed;

        }


        //�����˳�
        //if (Input.GetKeyDown(KeyCode.F5))
        //{
        //    SwitchThridPersonMode();
        //}

        


        if (isGrounded && Input.GetKey(KeyCode.Space) && groundTime >= minGroundedTime)
        {
            //print("������Ծ");
            jumpRequest = true;
            groundTime = 0; // ��Ծ�����ü�ʱ
        }
            

        if (Input.GetKeyDown(KeyCode.Q) && !managerhub.player.isSpectatorMode)
        {

            backpackmanager.ThrowDropBox();
        
        }

        //����ģʽ
        if (Service_World.game_mode == GameMode.Creative && !isSpectatorMode)
        {

            if (Input.GetKeyDown(KeyCode.Space))
            {

                jump_press = true;

                if (((Time.time - lastJumpTime) < doubleTapInterval) && jump_press)
                {
                    FlyingMode(!isFlying);

                }

                else if ((Time.time - lastJumpTime) >= doubleTapInterval)
                {

                    jump_press = false;

                }

                lastJumpTime = Time.time;

            }

        }
        




        // �����ˮ�а�����Ծ�� && leg����ˮ�棬������Ծ���� 
        if (isSwiming && Input.GetKey(KeyCode.Space) && (leg.position.y - 0.1f < Service_World._biomeProperties.terrainLayerProbabilitySystem.sea_level))
        {

            jumpRequest = true;

        }

        else if (isSwiming && Input.GetKey(KeyCode.Space) && (front || back || left || right))
        {

            jumpRequest = true;

        }



        //��סCtrl������������½�һ���߶�
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
        
            isSquating = true;
        
        }


        //�ɿ�Ctrl�����������ԭ
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
        
            isSquating = false;
        
        }


        //������������
        if (Input.GetMouseButtonDown(0) && !isSpectatorMode)
        {
            isFirstBrokeBlock = true;

            MC_RayCastStruct _rayCast = MC_Static_Raycast.RayCast(managerhub, MC_RayCast_FindType.AllFind,cam.position, cam.forward, reach, -1, checkIncrement);

            //print($"���߷��䳤�� = {_rayCast.rayDistance}");

            //����ʵ��
            if (_rayCast.isHit == 2)
            {
                isHitEntity = true;
                //print("��ʵ����");

                //ʵ����ɷ���
                Vector3 direct = managerhub.player.transform.forward;
                direct.y = 1f;

                //������ҹ�����Ч
                managerhub.NewmusicManager.PlayOneShot(MusicData.Player_Attack);

                //ʵ���Ѫ
                if (_rayCast.targetEntityInfo._obj.GetComponent<MC_Component_Life>() != null)
                    _rayCast.targetEntityInfo._obj.GetComponent<MC_Component_Life>().UpdateEntityLife(-1, direct);

                //����ʵ�屻������Ч
                //int _index = MusicData.Slime_Behurt;
                //if (_rayCast.targetEntityInfo._obj.GetComponent<MC_Music_Component>() != null)
                //    _index = _rayCast.targetEntityInfo._obj.GetComponent<MC_Music_Component>().BeHurtIndex;
                //if (_index != 0)
                //    managerhub.NewmusicManager.PlayOneShot(_index);

            }

            //print(OnLoadResource.Instance.Goods[1]);
        }

        //����ɿ���������isChanger��ԭ
        if (Input.GetMouseButtonUp(0))
        {

            isChangeBlock = false;
            hasExec_isChangedBlock = true;
            managerhub.OldMusicManager.isbroking = false;
            managerhub.OldMusicManager.Audio_player_broke.Stop();
            isFirstBrokeBlock = true;
            isHitEntity = false;
        }

        //�����������

        if (Input.GetKey(KeyCode.Mouse0) && !isSpectatorMode)
        {

            //Debug.Log("Player Mouse0");
            //isLeftMouseDown = true;
            //Debug.Log(new Vector3(Mathf.FloorToInt(RayCast_now().x), Mathf.FloorToInt(RayCast_now().y), Mathf.FloorToInt(RayCast_now().z)));
            //Vector3 _raycastNow = RayCast_now();
            MC_RayCastStruct _rayCast = MC_Static_Raycast.RayCast(managerhub,MC_RayCast_FindType.AllFind,cam.position, cam.forward, reach, -1, checkIncrement);
            test_Normal = _rayCast.hitNormal;

            if (_rayCast.isHit == 1 && hasExec_isChangedBlock && Service_World.blocktypes[managerhub.Service_World.GetBlockType(_rayCast.hitPoint)].canBeChoose)
            {
                OldPointLocation = new Vector3(Mathf.FloorToInt(_rayCast.hitPoint.x), Mathf.FloorToInt(_rayCast.hitPoint.y), Mathf.FloorToInt(_rayCast.hitPoint.z));
                hasExec_isChangedBlock = false;
            }


            Vector3 pointvector = new Vector3(Mathf.FloorToInt(_rayCast.hitPoint.x), Mathf.FloorToInt(_rayCast.hitPoint.y), Mathf.FloorToInt(_rayCast.hitPoint.z));

            if (pointvector != OldPointLocation && OldPointLocation != Vector3.zero && pointvector != Vector3.zero)
            {
                //print("CHangedBlock");
                isChangeBlock = true;
                managerhub.OldMusicManager.isbroking = false;
                OldPointLocation = pointvector;

            }

            //�����
            if (_rayCast.hitPoint != Vector3.zero)
            {

                //�������������ִ��
                if (!isDestroying && !isHitEntity)
                {

                    //Debug.Log("ִ������");
                    elapsedTime = 0.0f;
                    StartCoroutine(DestroySoilWithDelay(_rayCast));


                }


                //Service_World.GetChunkObject(RayCast_now()).EditData(GetRelaPos(RayCast_now()), 4);


                //print($"��������Ϊ��{RayCast_now()}");
                //print($"�������Ϊ��{GetRelaPos(RayCast())}");
                //print($"��������Ϊ��{Service_World.GetBlockType(RayCast())}");
            }


        }
        
        //�Ҽ���������
        if (Input.GetMouseButtonDown(1) && !isSpectatorMode)
        {

            isPlacing = true;
            MC_RayCastStruct _rayCast = MC_Static_Raycast.RayCast(managerhub, MC_RayCast_FindType.AllFind, cam.position, cam.forward, reach, -1, checkIncrement);

            if (_rayCast.hitPoint != Vector3.zero)
            {
                //Vector3 RayCast = RayCast_last();
                //Vector3 _raycastNow = RayCast_now();
                byte _targettype = managerhub.Service_World.GetBlockType(_rayCast.hitPoint);
                byte _selecttype = managerhub.backpackManager.slots[selectindex].blockId;

                //�Ҽ��ɻ�������
                if (_targettype < Service_World.blocktypes.Length && Service_World.blocktypes[_targettype].isinteractable && !isSquating)
                {
                    //print("isinteractable");

                    switch (_targettype)
                    {

                        //��Ƭ��
                        case 40:
                            if (_selecttype == VoxelData.Tool_MusicDiscs)
                            {

                                //managerhub.NewmusicManager.SwitchBackgroundMusic(MusicData.MusicBox, 1f, 0.5f);
                                managerhub.NewmusicManager.Create3DSound(GetCenterVector3(_rayCast.hitPoint_Previous) , MusicData.MusicBox);
                                managerhub.backpackManager.update_slots(1, 50);
                            }
                            break;

                        //DFS���� 
                        case 42:
                            if (_selecttype == VoxelData.Tool_BoneMeal)
                            {
                                //canvasManager.UIManager[VoxelData.ui���].childs[1]._object.SetActive(!canvasManager.UIManager[VoxelData.ui���].childs[1]._object.activeSelf);
                                Service_World.Allchunks[GetRelaChunkLocation(_rayCast.hitPoint)].EditData(_rayCast.hitPoint, VoxelData.Air);
                                
                                Smoke(_rayCast.hitPoint);
                                managerhub.backpackManager.update_slots(1, 56);

                                // ��ұ�ը��
                                //Vector3 _Direction = cam.transform.position - _rayCast.hitPoint;  //ը�ɷ���
                                //float _value = _Direction.magnitude / 3;  //�������ĵ�̶�[0,1]

                                ////����ը�ɾ���
                                //_Direction.y = Mathf.Lerp(0, 1, _value);
                                //float Distance = Mathf.Lerp(3, 0, _value);

                                //ForceMoving(_Direction, Distance, 0.1f);
                            }
                            break;

                        //����̨
                        case 18:
                            managerhub.canvasManager.SwitchUI_Player(CanvasData.uiplayer_����̨);
                            break;

                        //��¯
                        case 39:
                            managerhub.canvasManager.SwitchUI_Player(CanvasData.uiplayer_��¯);
                            break;

                        //����
                        case 45:
                            managerhub.canvasManager.SwitchUI_Player(CanvasData.uiplayer_����);
                            break;

                            //����
                            //case 57:

                            //    if (_selecttype == VoxelData.Tool_BoneMeal)
                            //    {
                            //        print("tree");
                            //        Chunk chunktemp = Service_World.GetChunkObject(_rayCast.hitPoint);
                            //        chunktemp.GenerateTree((int)_rayCast.hitPoint.x, (int)_rayCast.hitPoint.y, (int)_rayCast.hitPoint.z);
                            //        chunktemp.EditData(_rayCast.hitPoint, VoxelData.Wood);
                            //    }

                            //break;

                    }

                    return;
                }

                //����ǹ��߷��飬�򲻽��з���ķ���
                if (_selecttype < Service_World.blocktypes.Length && !Service_World.blocktypes[_selecttype].isTool)
                {
                    //����� && �������2f && �Ҳ��ǽŵ���
                    if (_rayCast.isHit == 1 && (_rayCast.hitPoint_Previous - cam.position).magnitude > max_hand_length && !CanPutBlock(new Vector3(_rayCast.hitPoint_Previous.x, _rayCast.hitPoint_Previous.y - 1f, _rayCast.hitPoint_Previous.z)))
                    {

                        //music
                        managerhub.OldMusicManager.PlaySoung_Place();

                        if (backpackmanager.istheindexHaveBlock(selectindex))
                        {




                            //Edit
                            if (Service_World.blocktypes[point_Block_type].CanBeCover)
                            {
                                managerhub.Service_World.GetChunkObject(_rayCast.hitPoint).EditData(_rayCast.hitPoint, backpackmanager.slots[selectindex].blockId);

                            }
                            else
                            {
                                managerhub.Service_World.GetChunkObject(_rayCast.hitPoint_Previous).EditData(_rayCast.hitPoint_Previous, backpackmanager.slots[selectindex].blockId);

                            }


                            //EditNumber
                            //Service_World.UpdateEditNumber(_rayCast.hitPoint_Previous, backpackmanager.slots[selectindex].blockId);


                            if (Service_World.game_mode == GameMode.Survival)
                            {

                                backpackmanager.update_slots(1, point_Block_type);
                                //backpackmanager.ChangeBlockInHand();

                            }

                        }

                        //print($"��������Ϊ��{RayCast_last()}");
                        //print($"�������Ϊ��{GetRelaPos(RayCast())}");
                        //print($"��������Ϊ��{Service_World.GetBlockType(RayCast())}");



                    }

                }

                //ִ�й��߷�����Ҽ�
                else
                {
                    switch (_selecttype)
                    {
                        //ƻ��
                        case 59:
                            managerhub.lifeManager.UpdatePlayerFood(-4, true);
                            managerhub.backpackManager.update_slots(1, VoxelData.Apple);
                            break;

                        //����
                        case 49:
                            managerhub.lifeManager.UpdatePlayerFood(-8, true);
                            managerhub.backpackManager.update_slots(1, VoxelData.Tool_Pork);
                            break;

                        //�鼮
                        case 58:
                            managerhub.canvasManager.SwitchUI_Player(CanvasData.uiplayer_�鼮);
                            break;

                        //���ʯ
                        case 51:
                            if (_targettype == VoxelData.TNT)
                            {
                                //��������
                                var chunkObject = managerhub.Service_World.GetChunkObject(_rayCast.hitPoint);
                                chunkObject.EditData(_rayCast.hitPoint, VoxelData.Air);

                                CreateTNT(_rayCast.hitPoint, false);

                            }
                            break;


                        default:
                            break;
                    }
                }


            }




        }

        //ѡ�񷽿�
        SelectBlock();


        //����ѡ��
        // ������¹���
        if (scrollWheelInput < 0f)
        {

            if (selectindex >= 8)
            {

                selectindex = 0;

            }
            else
            {

                selectindex++;

            }

            managerhub.canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }

        // ������Ϲ���
        else if (scrollWheelInput > 0f)
        {

            if (selectindex <= 0)
            {

                selectindex = 8;

            }
            else
            {

                selectindex--;

            }

            managerhub.canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }



    }





    //public float DEbug_DIstance;

    // �ȴ�2���ִ�����������ķ���
    //public float Broking_Offset = 0.1f;
    //public Vector3 previous_Broking_Normal;
    public GameObject Particle_TNT_Prefeb;
    IEnumerator DestroySoilWithDelay(MC_RayCastStruct _HitSrtuct)
    {

        //print("�������ƻ�Э��");
        isDestroying = true;
        byte _selecttype = managerhub.backpackManager.slots[selectindex].blockId;
        //if (point_Block_type == 255)
        //{
        //    print("point_Block_type == 255");
        //    yield break;
        //}
        byte theBlockwhichBeBrokenType = managerhub.Service_World.GetBlockType(_HitSrtuct.hitPoint);

        if (theBlockwhichBeBrokenType == 255)
        {
            theBlockwhichBeBrokenType = 2;
        }

        //�ƻ�������ϵͳ
        brokingBox.Particle_Play(point_Block_type);
        //Broking_Animation.Play();

        // ��¼Э�̿�ʼִ��ʱ��ʱ��
        float startTime = Time.time;

        //��ȡ�ھ�ʱ��
        float destroy_time = GetDestroyTime(_selecttype, theBlockwhichBeBrokenType);
        //print(destroy_time);

        // �ȴ�
        while (Time.time - startTime < destroy_time)
        {

            //�Ƿ������ȴ�
            //if (CheckToJumpBrokenTime(_selecttype, theBlockwhichBeBrokenType))
            //{
                
            //    break;
            //}





            // ������ʲ�ֵ
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / destroy_time);

            // ���²���
            if (t != 1 && point_Block_type != VoxelData.BedRock && !isSuperMining)
            {

                HighLightMaterial.color = new Color(0, 0, 0, 1);

                //if (Mathf.FloorToInt(t * 10) >= 10)
                //{
                //    print("DestroyTextures�±�Խ��");
                //}


                int index = Mathf.FloorToInt(t * 10);
                if (index < DestroyTextures.Length)
                {
                    HighLightMaterial.mainTexture = DestroyTextures[index];
                }
                else
                {
                    // ����Խ�����������ʹ��Ĭ�ϲ��ʻ������ò���
                    print("DestroyTextures�±�Խ��");
                    HighLightMaterial.mainTexture = DestroyTextures[DestroyTextures.Length - 1];
                }


            }

            // �������ڵȴ��ڼ��ɿ������ || ת����Ŀ�꣬��ȡ�������������߼�
            if (!Input.GetMouseButton(0) || isChangeBlock)
            {

                elapsedTime = 0.0f;
                isDestroying = false;
                isChangeBlock = false;
                managerhub.OldMusicManager.isbroking = false;

                //���ʻ�ԭ
                HighLightMaterial.color = new Color(0, 0, 0, 0);
                HighLightMaterial.mainTexture = DestroyTextures[0];

                //��Ч��ͣ
                managerhub.OldMusicManager.Audio_player_broke.Stop();

                brokingBox.gameObject.GetComponent<ParticleSystem>().Stop();
                //Broking_Animation.Stop();

                yield break;

            }

            yield return null;

        }



        //����ɹ���������
        // ִ�������������߼�
        isDestroying = false;

        brokingBox.gameObject.GetComponent<ParticleSystem>().Stop();
        //musicmanager.PlaySound_Broken(theBlockwhichBeBrokenType);

        ////�ƻ�����Ч��
        //GameObject particleInstance = Instantiate(Particle_Broken);
        //particleInstance.transform.parent = particel_Broken_transform;
        ////particleInstance.transform.position = _PosCenter;
        //particleInstance.GetComponent<ParticleCollision>().Particle_PLay(theBlockwhichBeBrokenType);

        elapsedTime = 0.0f;
        managerhub.OldMusicManager.isbroking = false;

        //���ʻ�ԭ
        HighLightMaterial.color = new Color(0, 0, 0, 0);
        HighLightMaterial.mainTexture = DestroyTextures[0];



        //World
        var chunkObject = managerhub.Service_World.GetChunkObject(_HitSrtuct.hitPoint);
        chunkObject.EditData(_HitSrtuct.hitPoint, VoxelData.Air);

    }

    //��̬����ƻ�ʱ��
    float GetDestroyTime(byte _SelectType, byte _beBrokenType)
    {
        //��ǰ����, ����Ǳ�������Զ�����ƻ�����
        if (_SelectType != 255 && !Service_World.blocktypes[_SelectType].canBreakBlockWithMouse1)
            return Mathf.Infinity;

        //����ģʽ
        if (Service_World.game_mode == GameMode.Creative)
        {
            //�״��ƻ�
            if (isFirstBrokeBlock)
            {
                isFirstBrokeBlock = false;
                return 0f;
            }

            return 0.25f;
        }

        //����ģʽ   
        if (Service_World.game_mode == GameMode.Survival)
        {

            //����
            if (_SelectType == 255)
            {
                //��������ʱ��
                if (_beBrokenType == VoxelData.BedRock)
                    return Mathf.Infinity;
            }
            //�����з���
            else
            {
                //��������ʱ��
                if (_beBrokenType == VoxelData.BedRock)
                    return Mathf.Infinity;
                //���� 
                if (_SelectType == VoxelData.Tool_Pickaxe)
                    return 0.25f;
            }

        }


        return Service_World.blocktypes[_beBrokenType].DestroyTime;
    }







    //���������ֱ�ֹͣ������
    public GameObject HandsHold;
    public Transform HandInit;
    public float InitTime = 0.1f;
    Coroutine InitHandCoroutine;
       
    public void InitHandTransform()
    {
        if (InitHandCoroutine == null)
        {
            print("����Э��");
            InitHandCoroutine = StartCoroutine(_InitHandTransform());
        }
        
    }

    IEnumerator _InitHandTransform()
    {
        //����ʱ���ڽ�_Hand���ɵ�HandInit
        // ��¼��ʼ״̬
        Vector3 startPosition = HandsHold.transform.position;
        Quaternion startRotation = HandsHold.transform.rotation;

        // Ŀ��״̬
        Vector3 targetPosition = HandInit.position;
        Quaternion targetRotation = HandInit.rotation;

        // ʱ�����
        float elapsedTime = 0f;

        while (elapsedTime < InitTime)
        {
            // ��ֵ����λ�ú���ת
            HandsHold.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / InitTime);
            HandsHold.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / InitTime);

            // �����ѹ�ȥ��ʱ��
            elapsedTime += Time.deltaTime;

            // �ȴ���һ֡
            yield return null;
        }

        // ȷ�����һ�ε���Ŀ��λ�ú���ת
        HandsHold.transform.position = targetPosition;
        HandsHold.transform.rotation = targetRotation;
        InitHandCoroutine = null;
    }



    //��ȡ��Ұ�1~9
    private void SelectBlock()
    {

        // ��ⰴ��1��9
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {

            selectindex = 0;
            managerhub.canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {

            selectindex = 1;
            managerhub.canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {

            selectindex = 2;
            managerhub.canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {

            selectindex = 3;
            managerhub.canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {

            selectindex = 4;
            managerhub.canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {

            selectindex = 5;
            managerhub.canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {

            selectindex = 6;
            managerhub.canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {

            selectindex = 7;
            managerhub.canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {

            selectindex = 8;
            managerhub.canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }

        
    }


    //���ø�������
    [ReadOnly]public Vector3 LastHighLightBlockPos;
    [ReadOnly]public Vector3 LastHighLightBlock_Normal;
    private void placeCursorBlocks()
    {
        //����ɹ����ã�����Ҫ�ٸ��£�ֱ����Ҹı䷽��isChangeBlock == true����Service_World.GetBlockType(pos)�����ı�

        MC_RayCastStruct _rayCast = MC_Static_Raycast.RayCast(managerhub, MC_RayCast_FindType.AllFind, cam.position, cam.forward, reach, -1, checkIncrement);

        //������Է���
        if (_rayCast.isHit == 1)
        {
            int posX = Mathf.FloorToInt(_rayCast.hitPoint.x);
            int posY = Mathf.FloorToInt(_rayCast.hitPoint.y);
            int posZ = Mathf.FloorToInt(_rayCast.hitPoint.z);
            Vector3 NowHighLightBlockPos = new Vector3(posX, posY, posZ);
            point_Block_type = managerhub.Service_World.GetBlockType(_rayCast.hitPoint);


            //�����һ�������·���
            if (NowHighLightBlockPos != LastHighLightBlockPos)
            {
                //print("��һ��");
                updateHightLightBlock(NowHighLightBlockPos);
                updateBrokingBox(_rayCast);
                LastHighLightBlockPos = NowHighLightBlockPos;
            }

            //�����һ�������·���
            if (_rayCast.hitNormal != LastHighLightBlock_Normal)
            {
                updateBrokingBox(_rayCast);

                LastHighLightBlock_Normal = _rayCast.hitNormal;
            }

            return;

        }
        else
        {
            HighlightBlock.gameObject.SetActive(false);
        }

        

    }

    void updateHightLightBlock(Vector3 _v)
    {
        //print("���¸�������");

        HighlightBlock.position = new Vector3(_v.x + 0.5f, _v.y + 0.5f, _v.z + 0.5f);
        HighlightBlock.localScale = new Vector3(1f, 1f, 1f);


        //��̬�ı�HighLightBlock��С
        if (Service_World.blocktypes[point_Block_type].isDIYCollision)
        {
            CollosionRange _collisionRange = Service_World.blocktypes[point_Block_type].CollosionRange;
            float offsetX = _collisionRange.xRange.y - _collisionRange.xRange.x;
            float offsetY = _collisionRange.yRange.y - _collisionRange.yRange.x;
            float offsetZ = _collisionRange.zRange.y - _collisionRange.zRange.x;
            HighlightBlock.position = new Vector3(_v.x + _collisionRange.xRange.y - offsetX / 2f, _v.y + _collisionRange.yRange.y - offsetY / 2f, _v.z + (_collisionRange.zRange.y - offsetZ / 2f));
            HighlightBlock.localScale = new Vector3(offsetX, offsetY, offsetZ); ;

        }



        //updateBrokingBox(_rayCast);


        HighlightBlock.gameObject.SetActive(true);
    }

    public ParticleCollision brokingBox;
    public float brokingbox���������Ĳ��� = 0.15f;
    void updateBrokingBox(MC_RayCastStruct _rayCast)
    {
        //brokingBox.Particle_PLay(point_Block_type);

        //��̬�ı��ƻ�����
        Vector3 _PosCenter = new Vector3((int)_rayCast.hitPoint.x + 0.5f, (int)_rayCast.hitPoint.y + 0.5f, (int)_rayCast.hitPoint.z + 0.5f);

        // ������ת��ʹ�� hitNormal ��Ϊ����ġ�ǰ���򡱶���
        // ���ݷ����жϷ��򣬲����ɶ�Ӧ����ת vec
        //Vector3 vec = Vector3.zero;
        //previous_Broking_Normal = _rayCast.hitNormal;
        Vector3 _scale = Vector3.one;
        switch (_rayCast.hitNormal)
        {
            case Vector3 v when v == Vector3.up:       // (0, 1, 0)
                _scale = new Vector3(1f, 1f, 0.1f);
                break;

            case Vector3 v when v == Vector3.down:     // (0, -1, 0)
                _scale = new Vector3(1f, 1f, 0.1f);
                break;

            case Vector3 v when v == Vector3.right:    // (1, 0, 0)
                _scale = new Vector3(0.1f, 1f, 1f);
                break;

            case Vector3 v when v == Vector3.left:     // (-1, 0, 0)
                _scale = new Vector3(0.1f, 1f, 1f);
                break;

            case Vector3 v when v == Vector3.forward:  // (0, 0, 1)
                _scale = new Vector3(1f, 0.1f, 1f);
                break;

            case Vector3 v when v == Vector3.back:     // (0, 0, -1)
                _scale = new Vector3(1f, 0.1f, 1f);
                break;
        }
        ParticleSystem particleSystem = brokingBox.gameObject.GetComponent<ParticleSystem>();

        // ��ȡ shape ģ�鲢�����޸�
        var shapeModule = particleSystem.shape;
        shapeModule.scale = _scale;  // ֱ���޸ĸ�������������

        // ������ת��Ӧ�õ��ƻ�����ʵ����
        //BrokingparticleInstance.transform.rotation = Quaternion.LookRotation(vec);

        //pos
        float _brokingOffset = 0.5f;

        if (Service_World.blocktypes[point_Block_type].isDIYCollision)
        {
            //Service_World.blocktypes[point_Block_type].CollosionRange.xRange

            switch (_rayCast.hitNormal)
            {
                case Vector3 v when v == Vector3.up:       // (0, 1, 0)
                    _brokingOffset = Service_World.blocktypes[point_Block_type].CollosionRange.yRange.y - 0.5f;
                    break;

                case Vector3 v when v == Vector3.down:     // (0, -1, 0)
                    _brokingOffset = Service_World.blocktypes[point_Block_type].CollosionRange.yRange.x - 0.5f;
                    break;

                case Vector3 v when v == Vector3.right:    // (1, 0, 0)
                    _brokingOffset = Service_World.blocktypes[point_Block_type].CollosionRange.xRange.y - 0.5f;
                    break;

                case Vector3 v when v == Vector3.left:     // (-1, 0, 0)
                    _brokingOffset = Service_World.blocktypes[point_Block_type].CollosionRange.xRange.x - 0.5f;
                    break;

                case Vector3 v when v == Vector3.forward:  // (0, 0, 1)
                    _brokingOffset = Service_World.blocktypes[point_Block_type].CollosionRange.zRange.y - 0.5f;
                    break;

                case Vector3 v when v == Vector3.back:     // (0, 0, -1)
                    _brokingOffset = Service_World.blocktypes[point_Block_type].CollosionRange.zRange.x - 0.5f;
                    break;
            }

        }

        brokingBox.gameObject.transform.position = _PosCenter + _rayCast.hitNormal * (_brokingOffset + brokingbox���������Ĳ���);
    }



    //ʵ�ֲ���
    bool hasExec_AdjustPlayerToGround = true;
    private void AchieveInput()
    {
        // ����ʵ��
        if (jumpRequest)
        {
            if (isSwiming)
            {
                verticalMomentum = water_jumpforce;
            }
            else
            {
                verticalMomentum = jumpForce;
            }

            isGrounded = false;
            jumpRequest = false;
            hasExec_AdjustPlayerToGround = true; // ����Ծʱ���õ������
        }

        // λ�õ�������ֹ��ģ
        if (isGrounded && hasExec_AdjustPlayerToGround)
        {
            if (!Service_World.blocktypes[managerhub.Service_World.GetBlockType(foot.position)].isDIYCollision)
            {
                Vector3 myposition = transform.position;
                Vector3 Vec = GetRelaPos(foot.position); // ��ȡ�Ų�Ӧ�е�Y����
                if (Vec.y + 1.95f > myposition.y) // ֻ�������ϵ��������ֹ��Ծʱ����
                {

                    transform.position = new Vector3(myposition.x, Vec.y + 1.95f, myposition.z);
                    //print("����һ������");
                }
            }

            hasExec_AdjustPlayerToGround = false; // ������ϣ���ֹÿִ֡��
        }

        // �¶�ʵ��
        if (isSquating && !isFlying)
        {
            float high = cam.localPosition.y - squatSpeed * Time.deltaTime;
            if (high <= 0.388f)
            {
                high = 0.388f;
            }
            cam.localPosition = new Vector3(cam.localPosition.x, high, cam.localPosition.z);
        }
        else
        {
            float high = cam.localPosition.y + squatSpeed * Time.deltaTime;
            if (high >= 0.81f)
            {
                high = 0.81f;
            }
            cam.localPosition = new Vector3(cam.localPosition.x, high, cam.localPosition.z);
        }

        // ѡ�񷽿�ʵ��
        if (selectindex >= 0 && selectindex <= 9)
        {
            selectblock.GetComponent<RectTransform>().anchoredPosition = new Vector2(CanvasData.SelectLocation_x[selectindex], 0);
        }

        // �ӽǺ��ƶ�ʵ��
        transform.Rotate(Vector3.up * mouseHorizontal);
        cam.localRotation = Quaternion.Euler(Camera_verticalInput, 0, 0);

        if (isFlying)
        {
            // ����
            if (Input.GetKey(KeyCode.Space))
            {
                velocity.y = flyVelocity_Verticle * Time.deltaTime;

                if (!NotCheckPlayerCollision)
                {
                    velocity.y = checkUpSpeed(velocity.y);
                }
                
            }
            // �½�
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                velocity.y = - flyVelocity_Verticle * Time.deltaTime;

                if (!NotCheckPlayerCollision)
                {
                    velocity.y = checkDownSpeed(velocity.y);
                }
            }
            // �ɿ��������𽥼���
            //else
            //{
            //    // ͨ����ֵ�����𽥼�С�ٶ�
            //    velocity.y = Mathf.Lerp(velocity.y, 0, Time.deltaTime * flyLerpTime);
            //}
            
        }

        transform.Translate(velocity, Space.World);
    }


    //����ģʽ
    public void FlyingMode(bool _open)
    {
        if (isSprinting)
        {
            isSprinting = false;
        }

        //�ı���Ұ
        StartCoroutine(expandchangeview(_open));

        isFlying = _open;

        jump_press = false;
    }


    //�Թ���ģʽ
    public void SpectatorMode(bool _open)
    {
        if (_open)
        {
            //�ر���ײ
            NotCheckPlayerCollision = true;

            //��������ģʽ
            FlyingMode(true);

            //�ر�UI
            managerhub.canvasManager.SpectatorMode(true);
        }
        else
        {
            //�ر���ײ
            NotCheckPlayerCollision = false;

            //�رշ���ģʽ
            FlyingMode(false);

            //��UI
            managerhub.canvasManager.SpectatorMode(false);
        }


    }


    //��ʼ���������
    public void InitPlayerLocation()
    {
        if (managerhub.Service_Saving.isLoadSaving)
        {
            Service_World.Start_Position = managerhub.Service_Saving.worldSetting.playerposition;
        }


        if (managerhub.Service_Saving.isLoadSaving && managerhub.Service_Saving.worldSetting.playerposition.y > 0)
        {
            Service_World.Start_Position = managerhub.Service_Saving.worldSetting.playerposition;
            transform.rotation = managerhub.Service_Saving.worldSetting.playerrotation;
        }
        else
        {

            Service_World.Start_Position = new Vector3(GetRealChunkLocation(Service_World.Start_Position).x, TerrainData.ChunkHeight - 2, GetRealChunkLocation(Service_World.Start_Position).z);
            //print($"start: {Service_World.Start_Position}");
            Service_World.Start_Position = managerhub.Service_World.AddressingBlock(Service_World.Start_Position, 3);

        }
        //print($"end: {Service_World.Start_Position}");
        transform.position = Service_World.Start_Position;
        //print(transform.position);
    }

    
    public GameObject MainBody;
    public GameObject FirstPersonCamera;
    public GameObject ThridPersonCamera;

    //ture����ʾ
    public void SwitchThridPersonMode()
    {
        // �л���ʾHitBox��״̬ 
        Show_HitBox = !Show_HitBox;

        // �л����������ʾ״̬�����ڵ����˳�ģʽ��
        MainBody.SetActive(!MainBody.activeSelf);

        // �л���һ�˳ƺ͵����˳������
        if (FirstPersonCamera.activeSelf)
        {
            FirstPersonCamera.SetActive(false); 
            ThridPersonCamera.SetActive(true);
        }
        else
        {
            FirstPersonCamera.SetActive(true);
            ThridPersonCamera.SetActive(false);
        }
    }



    //--------------------------------------------------------------------------------





    //----------------------------------��ײ���---------------------------------------

    public Vector3 front_Center = new Vector3();
    public Vector3 back_Center = new Vector3();
    public Vector3 left_Center = new Vector3();
    public Vector3 right_Center = new Vector3();
    [HideInInspector]public int CollisionNumber = 4;

    //����16����ײ��
    void update_block()
    {
        Vector3 _selfPos = transform.position;

        // ������ĸ���
        if (FactFacing.y > 0)
        {
            up_���� = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2));
            up_���� = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2));
            up_���� = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2));
            up_���� = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2));
            CollisionNumber += 4;
        }

        // ������ĸ���
        down_���� = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2));
        down_���� = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2));
        down_���� = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2));
        down_���� = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2));
        CollisionNumber += 4;

        //front
        if (ActualMoveDirection.z > 0)
        {
            front_Center = new Vector3(_selfPos.x, _selfPos.y, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_���� = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_���� = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_���� = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_���� = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);
            CollisionNumber += 5;

        }



        //back
        if (ActualMoveDirection.z < 0)
        {
            back_Center = new Vector3(_selfPos.x, _selfPos.y, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_���� = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_���� = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_���� = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_���� = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);
            CollisionNumber += 5;

        }


        //left
        if (ActualMoveDirection.x < 0)
        {
            left_Center = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y, _selfPos.z);
            left_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) + delta);
            left_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) - delta);
            left_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) + delta);
            left_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) - delta);
            CollisionNumber += 5;


        }

        //right
        if (ActualMoveDirection.x > 0)
        {
            right_Center = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y, _selfPos.z);
            right_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) - delta);
            right_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) + delta);
            right_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) - delta);
            right_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) + delta);
            CollisionNumber += 5;

        }


    }



    //��ײ��⣨���£�
    private float checkDownSpeed(float downSpeed)
    {

        if (
            managerhub.Service_World.CollisionCheckForVoxel(down_����) ||
            managerhub.Service_World.CollisionCheckForVoxel(down_����) ||
            managerhub.Service_World.CollisionCheckForVoxel(down_����) ||
            managerhub.Service_World.CollisionCheckForVoxel(down_����)

            )
        {

            isGrounded = true;
            return 0;

        }
        else
        {

            isGrounded = false;
            return downSpeed;

        }



    }


    //��ײ��⣨ͷ�ϣ�
    private float checkUpSpeed(float upSpeed)
    {

        if (
            managerhub.Service_World.CollisionCheckForVoxel(up_����) ||
            managerhub.Service_World.CollisionCheckForVoxel(up_����) ||
            managerhub.Service_World.CollisionCheckForVoxel(up_����) ||
            managerhub.Service_World.CollisionCheckForVoxel(up_����)
           )
        {

            return 0;

        }
        else
        {

            return upSpeed;

        }



    }


    //�������
    public Vector2 keyInput
    {
        get
        {
            return new Vector2(horizontalInput,verticalInput);
        }
    }

    //�������objectʵ���˶�����
    public Vector3 ActualMoveDirection
    {
        get
        {
            Vector3 direction = Vector3.zero;

            // ���keyInput�����뷽��
            Vector2 input = keyInput;

            // ����泯�ķ���
            Vector3 facing = FactFacing;

            // ��������з���ֵ�������ʵ���˶�����
            if (input != Vector2.zero)
            {
                if (input.y > 0) // ��ǰ�˶�
                {
                    direction += new Vector3(facing.x, 0, facing.z);
                }
                else if (input.y < 0) // ����˶�
                {
                    direction -= new Vector3(facing.x, 0, facing.z);
                }

                if (input.x < 0) // �����˶�
                {
                    // ��������Է������� (-z, x)
                    direction += new Vector3(-facing.z, 0, facing.x);
                }
                else if (input.x > 0) // �����˶�
                {
                    // �ҷ�������Է�����Ҳ� (z, -x)
                    direction += new Vector3(facing.z, 0, -facing.x);
                }
            }
            else
            {
                // ���û�а��¼��̣�direction ��������
                direction = Vector3.zero;
            }

            // �� momentum ��ӵ�ʵ���˶�����
            direction += momentum;

            // ��һ��������������ȷ���˶�����Ϊ��λ����
            if (direction != Vector3.zero)
            {
                direction = direction.normalized;
            }

            return direction;
        }
    }




    //��������������泯����
    public Vector3 Facing
    {
        get
        {
            float _y = isGrounded ? -1 : 1;
            return new Vector3(Mathf.RoundToInt(transform.forward.x), _y, Mathf.RoundToInt(transform.forward.z));

        }
    }

    //����ʵ���泯����
    public Vector3 FactFacing
    {
        get
        {
            float _y = isGrounded ? -1 : 1;
            return new Vector3(transform.forward.x, _y, transform.forward.z);

        }
    }


    //����0 1 4 5�ĸ�����
    public int IntForFacing
    {
        get
        {
            // ��ȡ��ҵ�ǰ������
            Vector3 forward = transform.forward;

            // ��ȡXZƽ��ķ����������뵽��ӽ�������
            float x = Mathf.Round(forward.x);
            float z = Mathf.Round(forward.z);

            // �жϷ��򲢷��ض�Ӧ������ֵ
            if (z > 0 && Mathf.Abs(forward.x) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 0; // ǰ (Z+)
            }
            else if (z < 0 && Mathf.Abs(forward.x) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 1; // �� (Z-)
            }
            else if (x > 0 && Mathf.Abs(forward.z) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 5; // �� (X+)
            }
            else if (x < 0 && Mathf.Abs(forward.z) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 4; // �� (X-)
            }

            // Ĭ�Ϸ���ֵ����ʾ�޷�ȷ������ͨ������²��ᷢ����
            return -1;
        }
    }

    public int RealBacking
    {
        get
        {
            // ��ȡ��ҵ�ǰ������
            Vector3 forward = transform.forward;

            // ��ȡXZƽ��ķ����������뵽��ӽ�������
            float x = Mathf.Round(forward.x);
            float z = Mathf.Round(forward.z);

            // �жϺ󱳷��򲢷��ض�Ӧ������ֵ
            if (z > 0 && Mathf.Abs(forward.x) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 1; // ���� Z- ���򣨺�
            }
            else if (z < 0 && Mathf.Abs(forward.x) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 0; // ���� Z+ ����ǰ��
            }
            else if (x > 0 && Mathf.Abs(forward.z) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 4; // ���� X- ������
            }
            else if (x < 0 && Mathf.Abs(forward.z) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 5; // ���� X+ �����ң�
            }

            // Ĭ�Ϸ���ֵ����ʾ�޷�ȷ������ͨ������²��ᷢ����
            return -1;
        }
    }



    //��ײ����ļ�⣨-Z����Ϊfront��
    //����ĽǶ�
    public bool front
    {
       
        get
        {
            //���Service_World����true������ײ
            if (managerhub.Service_World.CollisionCheckForVoxel(front_����) || 
                managerhub.Service_World.CollisionCheckForVoxel(front_����) || 
                managerhub.Service_World.CollisionCheckForVoxel(front_����) || 
                managerhub.Service_World.CollisionCheckForVoxel(front_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(front_Center))
            {
                return true;
            }

            //����¶�
            else if (isSquating)
            {
                //(���¹��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
                if ((managerhub.Service_World.CollisionCheckForVoxel(down_����) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z + extend_delta))) || (managerhub.Service_World.CollisionCheckForVoxel(down_����) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z + extend_delta))))
                {

                    return true;
                     
                }
                else
                    return false;
            }
            else
                return false;

            
        }

    }
    public bool back
    {

        get
        {
            //if (!isSquating)
            //{
            //    if (
            //    Service_World.CollisionCheckForVoxel(back_����) ||
            //    Service_World.CollisionCheckForVoxel(back_����) ||
            //    Service_World.CollisionCheckForVoxel(back_����) ||
            //    Service_World.CollisionCheckForVoxel(back_����)
            //    )
            //        return true;
            //    else
            //        return false;
            //}
            //else
            //{
            //    //(���Ϲ��� && �������첻�ǹ���) || (���Ϲ��� && �������첻�ǹ���)
            //    if ((Service_World.CollisionCheckForVoxel(down_����) && !Service_World.CollisionCheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z - extend_delta))) || (Service_World.CollisionCheckForVoxel(down_����) && !Service_World.CollisionCheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z - extend_delta))))
            //    {

            //        return true;
            //    }
            //    else
            //        return false;
            //}

            if (
                managerhub.Service_World.CollisionCheckForVoxel(back_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(back_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(back_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(back_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(back_Center)
                )
                return true;

            else if (isSquating)
            {

                //(���Ϲ��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
                if ((managerhub.Service_World.CollisionCheckForVoxel(down_����) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z - extend_delta))) || (managerhub.Service_World.CollisionCheckForVoxel(down_����) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z - extend_delta))))
                {

                    return true;

                }
                else
                    return false;



            }
            else
                return false;

        }
            

    }

    public bool left
    {

        get
        {
            //if (!isSquating)
            //{
            //    if (
            //    Service_World.CollisionCheckForVoxel(left_����) ||
            //    Service_World.CollisionCheckForVoxel(left_����) ||
            //    Service_World.CollisionCheckForVoxel(left_����) ||
            //    Service_World.CollisionCheckForVoxel(left_����)
            //    )
            //        return true;
            //    else
            //        return false;
            //}
            //else
            //{
                
            //    //(���Ϲ��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
            //    if ((Service_World.CollisionCheckForVoxel(down_����) && !Service_World.CollisionCheckForVoxel(new Vector3(down_����.x - extend_delta, down_����.y, down_����.z))) || (Service_World.CollisionCheckForVoxel(down_����) && !Service_World.CollisionCheckForVoxel(new Vector3(down_����.x - extend_delta, down_����.y, down_����.z))))
            //    {

            //        return true;
            //    }
            //    else
            //        return false;



            //}


            if (
                managerhub.Service_World.CollisionCheckForVoxel(left_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(left_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(left_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(left_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(left_Center)
                )
                return true;

            else if (isSquating)
            {

                //(���Ϲ��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
                if ((managerhub.Service_World.CollisionCheckForVoxel(down_����) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_����.x - extend_delta, down_����.y, down_����.z))) || (managerhub.Service_World.CollisionCheckForVoxel(down_����) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_����.x - extend_delta, down_����.y, down_����.z))))
                {

                    return true;

                }
                else
                    return false;



            }
            else 
                return false;

            


        }

    }
    public bool right
    {

        get
        {

            if (
                managerhub.Service_World.CollisionCheckForVoxel(right_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(right_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(right_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(right_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(right_Center)
                )
                return true;

            else if (isSquating)
            {

                //(���Ϲ��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
                if ((managerhub.Service_World.CollisionCheckForVoxel(down_����) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_����.x + extend_delta, down_����.y, down_����.z))) || (managerhub.Service_World.CollisionCheckForVoxel(down_����) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_����.x + extend_delta, down_����.y, down_����.z))))
                {

                    return true;

                }
                else
                    return false;
            }
            else
                return false;

            
        }

    }

    public bool collision_foot
    {
        get
        {
            //���Service_World����true������ײ
            if (managerhub.Service_World.CollisionCheckForVoxel(front_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(front_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(back_����)  ||
                managerhub.Service_World.CollisionCheckForVoxel(back_����)  ||
                managerhub.Service_World.CollisionCheckForVoxel(left_����)  ||
                managerhub.Service_World.CollisionCheckForVoxel(left_����)  ||
                managerhub.Service_World.CollisionCheckForVoxel(right_����) ||
                managerhub.Service_World.CollisionCheckForVoxel(right_����)
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool collision_waist
    {
        get
        {
            //���Service_World����true������ײ
            if (managerhub.Service_World.CollisionCheckForVoxel(front_Center) ||
                managerhub.Service_World.CollisionCheckForVoxel(back_Center) ||
                managerhub.Service_World.CollisionCheckForVoxel(left_Center) ||
                managerhub.Service_World.CollisionCheckForVoxel(right_Center)
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    //���½��·�������
    void Update_FootBlockType()
    {

        foot_BlockType_temp = managerhub.Service_World.GetBlockType(foot.position);

        //��������䶯
        if (foot_BlockType_temp != foot_BlockType)
        {


            //update
            foot_BlockType = foot_BlockType_temp;

        }

    }



    //-------------------------------------------------------------------------------------





    //--------------------------------- ���߼�� ------------------------------------------





    //���߼��
    [Foldout("���߼��", true)]
    [Header("���߳���")] public float reach = 5.2f;
    [Header("���߼��")] public float checkIncrement = 0.1f;


    //��þ��з���
    public Vector3 GetCenterPoint(Vector3 _pos)
    {
        return new Vector3((int)_pos.x + 0.5f, (int)_pos.y + 0.5f, (int)_pos.z + 0.5f);
    }

    void Draw_CollisionBox()
    {

        // �����������
        Debug.DrawLine(up_����, up_����, Color.red); // ���� -- ����
        Debug.DrawLine(up_����, up_����, Color.red); // ���� -- ����
        Debug.DrawLine(up_����, up_����, Color.red); // ���� -- ����
        Debug.DrawLine(up_����, up_����, Color.red); // ���� -- ����

        // �����������
        Debug.DrawLine(down_����, down_����, Color.red); // ���� -- ����
        Debug.DrawLine(down_����, down_����, Color.red); // ���� -- ����
        Debug.DrawLine(down_����, down_����, Color.red); // ���� -- ����
        Debug.DrawLine(down_����, down_����, Color.red); // ���� -- ����


        //�ϰ�Ȧ
        Debug.DrawLine(front_����, front_����, Color.yellow);
        Debug.DrawLine(front_����, right_����, Color.red);
        Debug.DrawLine(right_����, right_����, Color.yellow);
        Debug.DrawLine(right_����, back_����, Color.red);
        Debug.DrawLine(back_����, back_����, Color.yellow);
        Debug.DrawLine(back_����, left_����, Color.red);
        Debug.DrawLine(left_����, left_����, Color.yellow);
        Debug.DrawLine(left_����, front_����, Color.red);


        //������ײ��
        Debug.DrawLine(front_Center, right_Center, Color.green);
        Debug.DrawLine(right_Center, back_Center, Color.green);
        Debug.DrawLine(back_Center, left_Center, Color.green);
        Debug.DrawLine(left_Center, front_Center, Color.green);


        //�°�Ȧ
        Debug.DrawLine(front_����, front_����, Color.yellow);
        Debug.DrawLine(front_����, right_����, Color.red);
        Debug.DrawLine(right_����, right_����, Color.yellow);
        Debug.DrawLine(right_����, back_����, Color.red);
        Debug.DrawLine(back_����, back_����, Color.yellow);
        Debug.DrawLine(back_����, left_����, Color.red);
        Debug.DrawLine(left_����, left_����, Color.yellow);
        Debug.DrawLine(left_����, front_����, Color.red);


        //������
        //up����
        Debug.DrawLine(up_����, front_����, Color.red);
        Debug.DrawLine(up_����, left_����, Color.red);

        //up����
        Debug.DrawLine(up_����, front_����, Color.red);
        Debug.DrawLine(up_����, right_����, Color.red);

        //up����
        Debug.DrawLine(up_����, left_����, Color.red);
        Debug.DrawLine(up_����, back_����, Color.red);

        //up����
        Debug.DrawLine(up_����, back_����, Color.red);
        Debug.DrawLine(up_����, right_����, Color.red);


        //������
        //front
        Debug.DrawLine(front_����, front_����, Color.yellow);
        Debug.DrawLine(front_����, front_����, Color.yellow);
        //back
        Debug.DrawLine(back_����, back_����, Color.yellow);
        Debug.DrawLine(back_����, back_����, Color.yellow);
        //left
        Debug.DrawLine(left_����, left_����, Color.yellow);
        Debug.DrawLine(left_����, left_����, Color.yellow);
        //right
        Debug.DrawLine(right_����, right_����, Color.yellow);
        Debug.DrawLine(right_����, right_����, Color.yellow);

        //������
        //down����
        Debug.DrawLine(down_����, front_����, Color.red);
        Debug.DrawLine(down_����, left_����, Color.red);

        //down����
        Debug.DrawLine(down_����, front_����, Color.red);
        Debug.DrawLine(down_����, right_����, Color.red);

        //down����
        Debug.DrawLine(down_����, left_����, Color.red);
        Debug.DrawLine(down_����, back_����, Color.red);

        //down����
        Debug.DrawLine(down_����, back_����, Color.red);
        Debug.DrawLine(down_����, right_����, Color.red);

    }

    //bool hasExecHigbox = true;
    void Draw_HitBox()
    {
        Vector3 _selfPos = transform.position;
        Vector3 _eyesPos = eyes.transform.position;

        Vector3 _front_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _front_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _front_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _front_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);

        Vector3 _back_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _back_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _back_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _back_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);

        Vector3 _eyes_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _eyesPos.y, _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _eyes_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _eyesPos.y, _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _eyes_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _eyesPos.y, _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _eyes_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _eyesPos.y, _selfPos.z - (playerWidth / 2) - extend_delta);

        //if (hasExecHigbox)
        //{
        //    Vector3 center = (_back_���� + _front_����) / 2f;
        //    float width = (_front_���� - _front_����).magnitude;
        //    float height = (_front_���� - _front_����).magnitude;

        //     print($"��������: {center}");
        //    print($"��: {width}");
        //    print($"��: {height}");

        //    hasExecHigbox = false;
        //}


        //ͷ��������
        Debug.DrawLine(_front_����, _front_����, Color.white);
        Debug.DrawLine(_front_����, _back_����, Color.white); 
        Debug.DrawLine(_back_����, _back_����, Color.white);
        Debug.DrawLine(_back_����, _front_����, Color.white); 

        //�ŵ�������
        Debug.DrawLine(_front_����, _front_����, Color.white);
        Debug.DrawLine(_front_����, _back_����, Color.white);
        Debug.DrawLine(_back_����, _back_����, Color.white);
        Debug.DrawLine(_back_����, _front_����, Color.white);

        //�ĸ�����
        Debug.DrawLine(_front_����, _front_����, Color.white);
        Debug.DrawLine(_front_����, _front_����, Color.white);
        Debug.DrawLine(_back_����, _back_����, Color.white);
        Debug.DrawLine(_back_����, _back_����, Color.white);

        //�۾�
        Debug.DrawLine(_eyes_����, _eyes_����, Color.red);
        Debug.DrawLine(_eyes_����, _eyes_����, Color.red);
        Debug.DrawLine(_eyes_����, _eyes_����, Color.red);
        Debug.DrawLine(_eyes_����, _eyes_����, Color.red);
    }





    //-------------------------------------------------------------------------------------






    //----------------------------------- ���״̬ -------------------------------------------


    //�ж��㷨
    public Vector3 CheckHitBox(Vector3 _targetCenter, float _targetWidth, float _targetHeight)
    {
        // ����ж��䣬������֪
        Vector3 _selfCenter = transform.position;
        float _selfWidth = playerWidth; // ��ҿ�ȣ��ױ������εı߳���
        float _selfHeight = playerHeight; // ��Ҹ߶�

        // ������ҵı߽�
        float selfMinX = _selfCenter.x - _selfWidth / 2;
        float selfMaxX = _selfCenter.x + _selfWidth / 2;
        float selfMinY = _selfCenter.y - _selfHeight / 2;
        float selfMaxY = _selfCenter.y + _selfHeight / 2;
        float selfMinZ = _selfCenter.z - _selfWidth / 2; // �������������ͬ
        float selfMaxZ = _selfCenter.z + _selfWidth / 2;

        // ����Ŀ��ı߽�
        float targetMinX = _targetCenter.x - _targetWidth / 2;
        float targetMaxX = _targetCenter.x + _targetWidth / 2;
        float targetMinY = _targetCenter.y - _targetHeight / 2;
        float targetMaxY = _targetCenter.y + _targetHeight / 2;
        float targetMinZ = _targetCenter.z - _targetWidth / 2; // ����Ŀ�����������ͬ
        float targetMaxZ = _targetCenter.z + _targetWidth / 2;

        // �ж����㷨
        bool isCollision = selfMaxX >= targetMinX && selfMinX <= targetMaxX &&
                           selfMaxY >= targetMinY && selfMinY <= targetMaxY &&
                           selfMaxZ >= targetMinZ && selfMinZ <= targetMaxZ; // ���� Z �����ײ���

        //print(isCollision);

        // ����
        if (isCollision)
        {
            // �����ص�����
            Vector3 overlapDirection = (_selfCenter - _targetCenter).normalized;

            // ���������ӵ�����Ϣ
            // Debug.Log($"������ײ, Length = {(_selfCenter - _targetCenter).magnitude}");

            return overlapDirection;
        }
        else
        {
            return Vector3.zero;
        }
    }

    //���ǿ���ƶ�
    public void ForceMoving(Vector3 moveDirection, float moveDistance, float moveTime)
    {
        // �����ʼ����
        momentum = moveDirection.normalized * (moveDistance / moveTime);

        //����˲ʱ����
        verticalMomentum = 0f;

        //���߼��ȷ�����movetime
        //Vector3 _selfPos = transform.position;
        //float _MinDistnce = 3f;
        ////��ǰ
        //if (moveDirection.z > 0 && moveDirection.x < 0)
        //{
        //    Vector3 _front_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        //    Vector3 _front_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        //    float _MinDistnce_1 = RayCast_last(_front_����, moveDirection, 3);
        //    float _MinDistnce_2 = RayCast_last(_front_����, moveDirection, 3);

        //    if (_MinDistnce_1 < _MinDistnce)
        //    {
        //        _MinDistnce = _MinDistnce_1;
        //    }

        //    if (_MinDistnce_2 < _MinDistnce)
        //    {
        //        _MinDistnce = _MinDistnce_2;
        //    }

        //}

        ////��ǰ
        //if (moveDirection.z > 0 && moveDirection.x > 0)
        //{
        //    Vector3 _front_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        //    Vector3 _front_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        //    float _MinDistnce_1 = RayCast_last(_front_����, moveDirection, 3);
        //    float _MinDistnce_2 = RayCast_last(_front_����, moveDirection, 3);

        //    if (_MinDistnce_1 < _MinDistnce)
        //    {
        //        _MinDistnce = _MinDistnce_1;
        //    }

        //    if (_MinDistnce_2 < _MinDistnce)
        //    {
        //        _MinDistnce = _MinDistnce_2;
        //    }
        //}

        ////���
        //if (moveDirection.z < 0 && moveDirection.x < 0)
        //{
        //    Vector3 _back_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        //    Vector3 _back_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        //    float _MinDistnce_1 = RayCast_last(_back_����, moveDirection, 3);
        //    float _MinDistnce_2 = RayCast_last(_back_����, moveDirection, 3);

        //    if (_MinDistnce_1 < _MinDistnce)
        //    {
        //        _MinDistnce = _MinDistnce_1;
        //    }

        //    if (_MinDistnce_2 < _MinDistnce)
        //    {
        //        _MinDistnce = _MinDistnce_2;
        //    }
        //}

        ////�Һ�
        //if (moveDirection.z < 0 && moveDirection.x > 0)
        //{
        //    Vector3 _back_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        //    Vector3 _back_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        //    float _MinDistnce_1 = RayCast_last(_back_����, moveDirection, 3);
        //    float _MinDistnce_2 = RayCast_last(_back_����, moveDirection, 3);

        //    if (_MinDistnce_1 < _MinDistnce)
        //    {
        //        _MinDistnce = _MinDistnce_1;
        //    }

        //    if (_MinDistnce_2 < _MinDistnce)
        //    {
        //        _MinDistnce = _MinDistnce_2;
        //    }
        //}

        ////print(_MinDistnce);

        ////��ֹ����0
        //float _v = Mathf.Lerp(3,0, (momentum.magnitude - 20f) / 20f);

        //if (momentum.magnitude > 0)
        //{
        //    moveTime = _MinDistnce / _v;
        //}
        //else
        //{
        //    moveTime = 0.01f; // ����һ����С�ƶ�ʱ�䣬�Է�ֹ NaN ����
        //}

        //print(moveTime);

        // ����һ��Э�̣����ƶ�ʱ���������ֹͣ����
        StartCoroutine(StopForceMovingAfterTime(moveTime));
    }

    // Э�̣���ָ����ʱ�����ֹͣ����
    private IEnumerator StopForceMovingAfterTime(float moveTime)
    {
        // ��� moveTime �ǳ�С������ֹͣ����
        //if (moveTime <= 0.01f)
        //{
        //    momentum = Vector3.zero;
        //    yield break;
        //}
        //print(moveTime);

        // �ȴ�ָ�����ƶ�ʱ��
        yield return new WaitForSeconds(moveTime);

        // ��һ��ʱ�����𽥼��ٶ���
        float elapsed = 0f;
        float decayDuration = 0.5f; // ��������ʧ��ʱ��

        while (elapsed < decayDuration)
        {
            momentum = Vector3.Lerp(momentum, Vector3.zero, elapsed / decayDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ȷ����������
        momentum = Vector3.zero;
    }


    public void CreateTNT(Vector3 _point, bool _acybytnt)
    {
        //����TNTʵ��
        //GameObject tnt = GameObject.Instantiate(Entity_TNT);
        //tnt.GetComponent<Entity_TNT>().OnStartEntity(GetCenterPoint(_point), _acybytnt);

        managerhub.Service_Entity.AddEntity(EntityData.TNT, _point, out EntityInfo _result, true);
        _result._obj.GetComponent<Entity_TNT>().OnStartEntity(GetCenterPoint(_point), _acybytnt);
    }

    //��¼���״̬
    void GetPlayerState()
    {

        //�Ƿ���Ӿ
        //��ǰ���� || y+1�����ǲ���ˮ
        if (managerhub.Service_World.GetBlockType(foot.transform.position) == VoxelData.Water || managerhub.Service_World.GetBlockType(new Vector3(foot.transform.position.x, foot.transform.position.y + 1f, foot.transform.position.z)) == VoxelData.Water)
        {

            isSwiming = true;

        }
        else
        {

            isSwiming = false;

        }


        //��¼���ˤ��
        //�Ƿ���̫��ģʽ
        if (!isSpaceMode)
        {

            //��������ˮ��new_foot_high����
            if (foot.transform.position.y > new_foot_high || isSwiming || isSpectatorMode)
            {

                new_foot_high = transform.position.y;

            }



            //�ж�����Ƿ�����
            if (Service_World.game_mode == GameMode.Survival)
            {
                
                if (isGrounded)
                {

                    if (((new_foot_high - foot.transform.position.y) > MaxHurtHigh) && falldownCoroutine == null)
                    {

                        falldownCoroutine = StartCoroutine(HandleHurt());

                    }

                    new_foot_high = transform.position.y;

                }

            }
            
        }

        

    }


    //ִ������Э��
    IEnumerator HandleHurt()
    {

        lifemanager.UpdatePlayerBlood((int)(new_foot_high - foot.transform.position.y), true, true);

        yield return new WaitForSeconds(hurtCooldownTime); // �ȴ�������ȴʱ�����

        falldownCoroutine = null;

    }


    //������ͷ����
    public IEnumerator Animation_Behurt()
    {

        Vector3 startRotation = transform.localRotation.eulerAngles;
        Vector3 targetRotation = startRotation + new Vector3(0f, 0f, angle);


        float elapsedTime = 0f;
        while (elapsedTime < cycleLength / 2f)
        {

            elapsedTime += Time.deltaTime * speed;
            transform.localRotation = Quaternion.Euler(Vector3.Lerp(startRotation, targetRotation, elapsedTime / (cycleLength / 2f)));
            yield return null;

        }


        elapsedTime = 0f;


        while (elapsedTime < cycleLength / 2f)
        {

            elapsedTime += Time.deltaTime * speed;
            transform.localRotation = Quaternion.Euler(Vector3.Lerp(targetRotation, startRotation, elapsedTime / (cycleLength / 2f)));
            yield return null;

        }

        transform.localRotation = Quaternion.Euler(startRotation); // ��֤��������ʱ����ص���ʼ�Ƕ�

    }





    //-------------------------------------------------------------------------------------







    //---------------------------------- ������ ---------------------------------------------

    public Vector3 GetIntPosition(Vector3 _pos)
    {
        return new Vector3((int)_pos.x, (int)_pos.y, (int)_pos.z);
    }


    public Transform GetEyesPosition()
    {
        return cam;
    }

    
    public GameObject GetHand_Hold()
    {
        return hand_Hold;
    }

    public GameObject GetHand()
    {
        return hand;
    }

    public GameObject GetHandBlock()
    {
        return handBlock;
    }


    //�������������Ƿ��foot���ĸ�������ͬ��ֻҪ��һ����ͬ�򷵻�true
    bool CanPutBlock(Vector3 pos)
    {
        
        //�������eyes��������ǰ����true
        if (GetRelaPos(new Vector3(pos.x, pos.y + 1f, pos.z)) == GetRelaPos(cam.position))
        {
        
            return true;
        
        }


        if (GetRelaPos(pos) == GetRelaPos(down_����))
        {

            return true;

        }


        else if (GetRelaPos(pos) == GetRelaPos(down_����))
        {

            return true;

        }
        else if (GetRelaPos(pos) == GetRelaPos(down_����))
        {

            return true;

        }
        else if (GetRelaPos(pos) == GetRelaPos(down_����))
        {

            return true;

        }
        else
        {

            return false;

        }

    }

    //-------------------------------------------------------------------------------------

}


