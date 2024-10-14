//using System;
using Homebrew;
using System.Collections;
using UnityEngine;
using UnityEngine.LowLevel;


public class Player : MonoBehaviour
{

    //���״̬
    [Foldout("���״̬", true)]
    [Header("���״̬")]
    
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
    [ReadOnly] public bool isFirstBrokeBlock;  //����ģʽ�������˲�����ٷ���

    [Foldout("Transforms", true)]
    [Header("Transforms")]
    public ManagerHub managerhub;
    public CommandManager commandManager;
    public World world;
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
    public float reach = 8f;
    private float checkIncrement = 0.01f;
    [HideInInspector]
    public float ray_length = 0f;
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
    public float angle = 50f;
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
    public float flyVelocity = 0.1f; //���·����ٶ�
    public int jump_press = 0;


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
    public byte foot_BlockType = VoxelData.Air;
    public byte foot_BlockType_temp = VoxelData.Air;

    //�������isInCave
    public float isInCave_checkInterval = 10f; private float isInCave_nextCheckTime = 10f;// ÿ0.5����һ��


    //--------------------------------- ���ں��� --------------------------------------




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
        world.Start_Position = transform.position;
    }

    
    private void FixedUpdate()
    {

        if (world.game_state == Game_State.Playing)
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


            if (transform.position.y < -20f)
            {
                lifemanager.UpdatePlayerBlood(100, true, true);
            }

            placeCursorBlocks();

        }



    }


    
    private void Update()
    {
        if (world.game_state == Game_State.Playing)
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
            if (!isFlying)
            {
                update_block();
            }

            //��Ϸ����ͣ����ͣ�������
            if (managerhub.canvasManager.isPausing == true || commandManager.isConsoleActive == true)
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

            // ÿ��һ��ʱ����һ��
            if (Time.time >= isInCave_nextCheckTime)
            {
                //print($"{Time.time}");
                CheckisInCave();
                isInCave_nextCheckTime = Time.time + isInCave_checkInterval; // ������һ�μ���ʱ��
            }


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
        if (managerhub.world.game_mode == GameMode.Survival)
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


    //����isInCave״̬
    public void CheckisInCave()
    {
        //print("");
        float playerY = cam.position.y + 10f;
        Vector3 RelaPosition = managerhub.world.GetRelalocation(cam.position);
        Vector3 _ChunkLocation = managerhub.world.GetChunkLocation(cam.position);

        //print($"RelaPosition: {RelaPosition} , ChunkLocation = {_ChunkLocation}");
        float NoiseY = managerhub.world.GetTotalNoiseHigh_Biome((int)RelaPosition.x, (int)RelaPosition.z, new Vector3((int)_ChunkLocation.x * 16f, 0f, (int)_ChunkLocation.z * 16f), managerhub.world.worldSetting.worldtype);

        //print($"PlayerY��{playerY} , NoiseY��{NoiseY} , ��: {NoiseY - playerY}");


        // ����۾�����λ���Ƿ��ڵر����£���Fog��Ϊ�������ɫ����
        if (playerY < NoiseY || playerY < 0)
        {
            if (!isInCave)
            {
                //print("������");
                // ��ʼ������ɵ���Ѩ״̬
                managerhub.timeManager.Buff_CaveFog(true);
                isInCave = true; 
            }
        }
        else
        {
            if (isInCave)
            {
                //print("����ر�");
                // ��ʼ������ɵ�����״̬
                if (!managerhub.timeManager.isNight)
                {
                    managerhub.timeManager.Buff_CaveFog(false);
                }
                
                isInCave = false;
            }
        }
    }


    //---------------------------------------------------------------------------------






    //--------------------------------- ��Ҳ��� --------------------------------------

    //������������ֹ�ŵ״�ģ
    
    //private void AdjustPlayerToGround()
    //{
    //    if (isGrounded && hasExec_AdjustPlayerToGround)
    //    {
    //        print("����һ������");
    //        Vector3 myposition = transform.position;
    //        Vector3 Vec = world.GetRelalocation(foot.position);
    //        transform.position = new Vector3(myposition.x, Vec.y + 1.95f, myposition.z);
    //        hasExec_AdjustPlayerToGround = false;
    //    }
    //}


    //���ݼ���
    private void CalculateVelocity()
    {

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
        if (!isFlying)
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
    private void GetPlayerInputs()
    {

        // ��ȡ��ǰ������ֵ
        float currentHorizontalInput = Input.GetAxis("Horizontal");
        float currentVerticalInput = Input.GetAxis("Vertical");

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


        if (Input.GetButtonDown("Sprint") && !isFlying)
        {

            isSprinting = true;
            musicmanager.footstepInterval = PlayerData.sprintSpeed;

        }

        if (managerhub.lifeManager.SprintLock)
        {
            isSprinting = false;
        }
            
        if (Input.GetButtonUp("Sprint") && !isFlying)
        {

            isSprinting = false;
            musicmanager.footstepInterval = PlayerData.walkSpeed;

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
            

        if (Input.GetKeyDown(KeyCode.Q))
        {

            backpackmanager.ThrowDropBox();
        
        }

        //����ģʽ
        if (world.game_mode == GameMode.Creative)
        {

            if (Input.GetKeyDown(KeyCode.Space))
            {

                jump_press++;

                if (((Time.time - lastJumpTime) < doubleTapInterval) && jump_press == 1)
                {
                    if (isSprinting)
                    {
                        isSprinting = false;
                    }
                   
                    //�ı���Ұ
                    if (!isFlying)
                    {
                        //������Ұ
                        StartCoroutine(expandchangeview(true));
                    }
                    else
                    {
                        StartCoroutine(expandchangeview(false));
                    }

                    isFlying = !isFlying;
                    jump_press = 0;

                }

                else if ((Time.time - lastJumpTime) >= doubleTapInterval)
                {

                    jump_press = 0;

                }

                lastJumpTime = Time.time;

            }

        }
        




        // �����ˮ�а�����Ծ�� && leg����ˮ�棬������Ծ���� 
        if (isSwiming && Input.GetKey(KeyCode.Space) && (leg.position.y - 0.1f < world.terrainLayerProbabilitySystem.sea_level))
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
        if (Input.GetMouseButtonDown(0))
        {
            isFirstBrokeBlock = true;


        }

        //����ɿ���������isChanger��ԭ
        if (Input.GetMouseButtonUp(0))
        {

            isChangeBlock = false;
            hasExec_isChangedBlock = true;
            musicmanager.isbroking = false;
            musicmanager.Audio_player_broke.Stop();
            isFirstBrokeBlock = false;
        }

        //�����������

        if (Input.GetKey(KeyCode.Mouse0))
        {

            //Debug.Log("Player Mouse0");
            //isLeftMouseDown = true;
            //Debug.Log(new Vector3(Mathf.FloorToInt(RayCast_now().x), Mathf.FloorToInt(RayCast_now().y), Mathf.FloorToInt(RayCast_now().z)));
            //Vector3 _raycastNow = RayCast_now();
            RayCastStruct _rayCast = NewRayCast();

            if (_rayCast.isHit && hasExec_isChangedBlock && world.blocktypes[world.GetBlockType(_rayCast.hitPoint)].canBeChoose)
            {
                OldPointLocation = new Vector3(Mathf.FloorToInt(_rayCast.hitPoint.x), Mathf.FloorToInt(_rayCast.hitPoint.y), Mathf.FloorToInt(_rayCast.hitPoint.z));
                hasExec_isChangedBlock = false;
            }


            Vector3 pointvector = new Vector3(Mathf.FloorToInt(_rayCast.hitPoint.x), Mathf.FloorToInt(_rayCast.hitPoint.y), Mathf.FloorToInt(_rayCast.hitPoint.z));

            if (pointvector != OldPointLocation && OldPointLocation != Vector3.zero && pointvector != Vector3.zero)
            {
                //print("CHangedBlock");
                isChangeBlock = true;
                musicmanager.isbroking = false;
                OldPointLocation = pointvector;

            }

            //�����
            if (_rayCast.hitPoint != Vector3.zero)
            {

                //�������������ִ��
                if (!isDestroying)
                {

                    //Debug.Log("ִ������");
                    elapsedTime = 0.0f;
                    StartCoroutine(DestroySoilWithDelay(_rayCast.hitPoint));


                }


                //world.GetChunkObject(RayCast_now()).EditData(world.GetRelalocation(RayCast_now()), 4);


                //print($"��������Ϊ��{RayCast_now()}");
                //print($"�������Ϊ��{world.GetRelalocation(RayCast())}");
                //print($"��������Ϊ��{world.GetBlockType(RayCast())}");
            }


        }

        //�Ҽ���������
        if (Input.GetMouseButtonDown(1))
        {

            isPlacing = true;
            RayCastStruct _rayCast = NewRayCast();

            //Vector3 RayCast = RayCast_last();
            //Vector3 _raycastNow = RayCast_now();
            byte _targettype = world.GetBlockType(_rayCast.hitPoint);
            byte _selecttype = managerhub.backpackManager.slots[selectindex].blockId;

            //�Ҽ��ɻ�������
            if (_targettype < world.blocktypes.Length && world.blocktypes[_targettype].isinteractable)
            {
                //print("isinteractable");

                switch (_targettype)
                {
                    
                    //��Ƭ��
                    case 40:
                        if (_selecttype == VoxelData.Tool_MusicDiscs)
                        {
                            managerhub.musicManager.Audio_envitonment.clip = managerhub.musicManager.audioclips[4];
                            managerhub.musicManager.Audio_envitonment.Play();
                            managerhub.backpackManager.update_slots(1, 50);
                        }
                        break;
                   
                    //DFS���� 
                    case 42:
                        if (_selecttype == VoxelData.Tool_BoneMeal)
                        {
                            //canvasManager.UIManager[VoxelData.ui���].childs[1]._object.SetActive(!canvasManager.UIManager[VoxelData.ui���].childs[1]._object.activeSelf);
                            world.Allchunks[world.GetChunkLocation(_rayCast.hitPoint)].EditData(_rayCast.hitPoint, VoxelData.Air);
                            BlocksFunction.Smoke(managerhub, _rayCast.hitPoint, 2.5f);
                            managerhub.backpackManager.update_slots(1, 56);
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



                }

                return;
            }

            //����ǹ��߷��飬�򲻽��з���ķ���
            if (_selecttype < managerhub.world.blocktypes .Length && !managerhub.world.blocktypes[_selecttype].isTool)
            {
                //����� && �������2f && �Ҳ��ǽŵ���
                if (_rayCast.isHit && (_rayCast.hitPoint_Previous - cam.position).magnitude > max_hand_length && !CanPutBlock(new Vector3(_rayCast.hitPoint_Previous.x, _rayCast.hitPoint_Previous.y - 1f, _rayCast.hitPoint_Previous.z)))
                {

                    //music
                    musicmanager.PlaySoung_Place();

                    if (backpackmanager.istheindexHaveBlock(selectindex))
                    {




                        //Edit
                        world.GetChunkObject(_rayCast.hitPoint_Previous).EditData(world.GetRelalocation(_rayCast.hitPoint_Previous), backpackmanager.slots[selectindex].blockId);


                        //EditNumber
                        world.UpdateEditNumber(_rayCast.hitPoint_Previous, backpackmanager.slots[selectindex].blockId);


                        if (world.game_mode == GameMode.Survival)
                        {

                            backpackmanager.update_slots(1, point_Block_type);
                            //backpackmanager.ChangeBlockInHand();

                        }

                    }

                    //print($"��������Ϊ��{RayCast_last()}");
                    //print($"�������Ϊ��{world.GetRelalocation(RayCast())}");
                    //print($"��������Ϊ��{world.GetBlockType(RayCast())}");



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
                            BlocksFunction.Boom(managerhub, _rayCast.hitPoint, 4);
                            GameObject.Instantiate(particle_explosion, _rayCast.hitPoint_Previous, Quaternion.identity);
                            musicmanager.PlaySound(MusicData.explore);

                            // ��ұ�ը��
                            Vector3 _Direction = cam.transform.position - _rayCast.hitPoint;  //ը�ɷ���
                            float _value = _Direction.magnitude / 3;  //�������ĵ�̶�[0,1]

                            //����ը�ɾ���
                            _Direction.y = Mathf.Lerp(0, 1, _value);
                            float Distance = Mathf.Lerp(3, 0, _value);

                            ForceMoving(_Direction, Distance, 0.1f);

                            if (managerhub.world.game_mode == GameMode.Survival && _Direction.magnitude <= 4)
                            {
                                managerhub.lifeManager.UpdatePlayerBlood((int)Mathf.Lerp(30, 10, _value), true, true);
                            }
                        }
                        break;


                    default:
                        break;
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
    IEnumerator DestroySoilWithDelay(Vector3 position)
    {
        //print("�������ƻ�Э��");
        isDestroying = true;
        byte _selecttype = managerhub.backpackManager.slots[selectindex].blockId;
        //if (point_Block_type == 255)
        //{
        //    print("point_Block_type == 255");
        //    yield break;
        //}
        byte theBlockwhichBeBrokenType = point_Block_type;

        if (theBlockwhichBeBrokenType == 255)
        {
            theBlockwhichBeBrokenType = 2;
        }


        //�ƻ�������ϵͳ
        GameObject BrokingparticleInstance = Instantiate(Particle_Broking);
        BrokingparticleInstance.transform.parent = particel_Broken_transform;
        BrokingparticleInstance.transform.position = position;
        BrokingparticleInstance.GetComponent<ParticleCollision>().Particle_PLay(theBlockwhichBeBrokenType);
        //Broking_Animation.textureSheetAnimation.SetSprite(0, world.blocktypes[theBlockwhichBeBrokenType].buttom_sprit);

        //Broking_Animation.Play();

        // ��¼Э�̿�ʼִ��ʱ��ʱ��
        float startTime = Time.time;
        float destroy_time = world.blocktypes[world.GetBlockType(position)].DestroyTime;

        //�ھ�ʱ���޸�
        //1. ����ģʽ
        //end.����
        if (theBlockwhichBeBrokenType == VoxelData.BedRock)
        {
            //print("ʱ������");
            destroy_time = Mathf.Infinity;

        }
        else
        {

            //ѡ���±�Ϊ0�������
            if (_selecttype != 255 && world.blocktypes[_selecttype].canBreakBlockWithMouse1 == false)
            {
                destroy_time = Mathf.Infinity;
                isFirstBrokeBlock = false;
            }

            else if (isSuperMining || _selecttype == VoxelData.Tool_Pickaxe)
            {
                destroy_time = 0.25f;
            }
        }
        

        // �ȴ�
        while (Time.time - startTime < destroy_time)
        {

            if (isFirstBrokeBlock && managerhub.world.game_mode == GameMode.Creative && theBlockwhichBeBrokenType != VoxelData.BedRock)
            {
                isFirstBrokeBlock = false;
                break;
                
            }

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
                musicmanager.isbroking = false;

                //���ʻ�ԭ
                HighLightMaterial.color = new Color(0, 0, 0, 0);
                HighLightMaterial.mainTexture = DestroyTextures[0];

                //��Ч��ͣ
                musicmanager.Audio_player_broke.Stop();

                BrokingparticleInstance.GetComponent<ParticleSystem>().Stop();
                //Broking_Animation.Stop();

                yield break;

            }

            yield return null;

        }



        //����ɹ���������
        // ִ�������������߼�
        isDestroying = false;

        BrokingparticleInstance.GetComponent<ParticleSystem>().Stop();
        musicmanager.PlaySound_Broken(theBlockwhichBeBrokenType);

        //�ƻ�����Ч��
        GameObject particleInstance = Instantiate(Particle_Broken);
        particleInstance.transform.parent = particel_Broken_transform;
        particleInstance.transform.position = position;
        particleInstance.GetComponent<ParticleCollision>().Particle_PLay(theBlockwhichBeBrokenType);

        elapsedTime = 0.0f;
        musicmanager.isbroking = false;

        //���ʻ�ԭ
        HighLightMaterial.color = new Color(0, 0, 0, 0);
        HighLightMaterial.mainTexture = DestroyTextures[0];

        //����ģʽ
        if (world.game_mode == GameMode.Survival)
        {

            //�������ж�
            if (world.blocktypes[theBlockwhichBeBrokenType].candropBlock)
            {
                //��Ҷ����ƻ��
                if (theBlockwhichBeBrokenType == VoxelData.Leaves)
                {
                    if (managerhub.world.GetProbability(30))
                    {
                        backpackmanager.CreateDropBox(new Vector3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), Mathf.FloorToInt(position.z)), new BlockItem(VoxelData.Apple, 1), false);
                    }
                }

            }



        }




        //�Ž������ɵ�����ִ��
        //canvasManager.Change_text_selectBlockname(point_Block_type);





        //if (world.blocktypes[point_Block_type].Particle_Material == null)
        //    particleInstance.GetComponent<ParticleSystemRenderer>().material = world.blocktypes[VoxelData.Soil].Particle_Material;
        //else
        //    particleInstance.GetComponent<ParticleSystemRenderer>().material = world.blocktypes[point_Block_type].Particle_Material;

        //Destroy(particleInstance, 10.0f);

        //Broking_Animation.Stop();

        //World
        var chunkObject = world.GetChunkObject(position);
        chunkObject.EditData(world.GetRelalocation(position), VoxelData.Air);
        //world.UpdateEditNumber(position, VoxelData.Air);


        
        //EditNumber


        //print($"��������Ϊ��{position}");
        //print($"�������Ϊ��{world.GetRelalocation(position)}");
        //print($"��������Ϊ��{world.GetBlockType(position)}"); 

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
    private void placeCursorBlocks()
    {
        //����ɹ����ã�����Ҫ�ٸ��£�ֱ����Ҹı䷽��isChangeBlock == true����world.GetBlockType(pos)�����ı�

        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {

            Vector3 pos = cam.position + (cam.forward * step);

            //�쳣���
            if (pos.y < 0)
            {
                pos = new Vector3(pos.x,0, pos.z);
            }


            //������Է���
            if (world.RayCheckForVoxel(pos))
            {
                int posX = Mathf.FloorToInt(pos.x);
                int posY = Mathf.FloorToInt(pos.y);
                int posZ = Mathf.FloorToInt(pos.z);
                point_Block_type = world.GetBlockType(pos);
                

                //�����һ�������·���
                if (new Vector3(posX, posY, posZ) != LastHighLightBlockPos)
                {
                    //print("��һ��");
                    updateHightLightBlock(posX, posY, posZ);
                    LastHighLightBlockPos = new Vector3(posX, posY, posZ);
                }

               

                return;

            }


            point_Block_type = PlayerData.notHit;
            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        HighlightBlock.gameObject.SetActive(false);

    }

    void updateHightLightBlock(int posX, int posY,int posZ)
    {
        //print("���¸�������");

        HighlightBlock.position = new Vector3(posX + 0.5f, posY + 0.5f + 0.01f, posZ + 0.5f);
        HighlightBlock.localScale = new Vector3(1.01f, 1.01f, 1.01f);


        //��̬�ı�HighLightBlock��С
        if (managerhub.world.blocktypes[point_Block_type].isDIYCollision)
        {
            CollosionRange _collisionRange = managerhub.world.blocktypes[point_Block_type].CollosionRange;
            float offsetX = _collisionRange.xRange.y - _collisionRange.xRange.x;
            float offsetY = _collisionRange.yRange.y - _collisionRange.yRange.x;
            float offsetZ = _collisionRange.zRange.y - _collisionRange.zRange.x;
            HighlightBlock.position = new Vector3(posX + _collisionRange.xRange.y - offsetX / 2f, posY + _collisionRange.yRange.y - offsetY / 2f, posZ + (_collisionRange.zRange.y - offsetZ / 2f));
            HighlightBlock.localScale = new Vector3(offsetX + 0.01f, offsetY + 0.01f, offsetZ + 0.01f); ;

        }

        HighlightBlock.gameObject.SetActive(true);
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
            Vector3 myposition = transform.position;
            Vector3 Vec = world.GetRelalocation(foot.position); // ��ȡ�Ų�Ӧ�е�Y����
            if (Vec.y + 1.95f > myposition.y) // ֻ�������ϵ��������ֹ��Ծʱ����
            {
                transform.position = new Vector3(myposition.x, Vec.y + 1.95f, myposition.z);
                //print("����һ������");
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
            selectblock.GetComponent<RectTransform>().anchoredPosition = new Vector2(PlayerData.SelectLocation_x[selectindex], 0);
        }

        // �ӽǺ��ƶ�ʵ��
        transform.Rotate(Vector3.up * mouseHorizontal);
        cam.localRotation = Quaternion.Euler(Camera_verticalInput, 0, 0);

        if (isFlying)
        {
            // ����
            if (Input.GetKey(KeyCode.Space))
            {
                velocity.y = flyVelocity * Time.deltaTime;
            }
            // �½�
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                velocity.y = -flyVelocity * Time.deltaTime;
            }
            // �ɿ��������𽥼���
            else
            {
                // ͨ����ֵ�����𽥼�С�ٶ�
                velocity.y = Mathf.Lerp(velocity.y, 0, Time.deltaTime * 0.3f);
            }
        }

        transform.Translate(velocity, Space.World);
    }



    //��ʼ���������
    public void InitPlayerLocation()
    {

        if (managerhub.world.isLoadSaving)
        {
            managerhub.world.Start_Position = managerhub.world.worldSetting.playerposition;
        }


        if (managerhub.world.isLoadSaving && managerhub.world.worldSetting.playerposition.y > 0)
        {
            managerhub.world.Start_Position = managerhub.world.worldSetting.playerposition;
            transform.rotation = managerhub.world.worldSetting.playerrotation;
        }
        else
        {

            managerhub.world.Start_Position = new Vector3(managerhub.world.GetRealChunkLocation(managerhub.world.Start_Position).x, TerrainData.ChunkHeight - 2, managerhub.world.GetRealChunkLocation(managerhub.world.Start_Position).z);
            //print($"start: {managerhub.world.Start_Position}");
            managerhub.world.Start_Position = managerhub.world.AddressingBlock(managerhub.world.Start_Position, 3);

        }
        //print($"end: {managerhub.world.Start_Position}");
        transform.position = world.Start_Position;
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
            world.CollisionCheckForVoxel(down_����) ||
            world.CollisionCheckForVoxel(down_����) ||
            world.CollisionCheckForVoxel(down_����) ||
            world.CollisionCheckForVoxel(down_����)

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
            world.CollisionCheckForVoxel(up_����) ||
            world.CollisionCheckForVoxel(up_����) ||
            world.CollisionCheckForVoxel(up_����) ||
            world.CollisionCheckForVoxel(up_����)
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
            //���world����true������ײ
            if (world.CollisionCheckForVoxel(front_����) || 
                world.CollisionCheckForVoxel(front_����) || 
                world.CollisionCheckForVoxel(front_����) || 
                world.CollisionCheckForVoxel(front_����) ||
                world.CollisionCheckForVoxel(front_Center))
            {
                return true;
            }

            //����¶�
            else if (isSquating)
            {
                //(���¹��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
                if ((world.CollisionCheckForVoxel(down_����) && !world.CollisionCheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z + extend_delta))) || (world.CollisionCheckForVoxel(down_����) && !world.CollisionCheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z + extend_delta))))
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
            //    world.CollisionCheckForVoxel(back_����) ||
            //    world.CollisionCheckForVoxel(back_����) ||
            //    world.CollisionCheckForVoxel(back_����) ||
            //    world.CollisionCheckForVoxel(back_����)
            //    )
            //        return true;
            //    else
            //        return false;
            //}
            //else
            //{
            //    //(���Ϲ��� && �������첻�ǹ���) || (���Ϲ��� && �������첻�ǹ���)
            //    if ((world.CollisionCheckForVoxel(down_����) && !world.CollisionCheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z - extend_delta))) || (world.CollisionCheckForVoxel(down_����) && !world.CollisionCheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z - extend_delta))))
            //    {

            //        return true;
            //    }
            //    else
            //        return false;
            //}

            if (
                world.CollisionCheckForVoxel(back_����) ||
                world.CollisionCheckForVoxel(back_����) ||
                world.CollisionCheckForVoxel(back_����) ||
                world.CollisionCheckForVoxel(back_����) ||
                world.CollisionCheckForVoxel(back_Center)
                )
                return true;

            else if (isSquating)
            {

                //(���Ϲ��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
                if ((world.CollisionCheckForVoxel(down_����) && !world.CollisionCheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z - extend_delta))) || (world.CollisionCheckForVoxel(down_����) && !world.CollisionCheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z - extend_delta))))
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
            //    world.CollisionCheckForVoxel(left_����) ||
            //    world.CollisionCheckForVoxel(left_����) ||
            //    world.CollisionCheckForVoxel(left_����) ||
            //    world.CollisionCheckForVoxel(left_����)
            //    )
            //        return true;
            //    else
            //        return false;
            //}
            //else
            //{
                
            //    //(���Ϲ��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
            //    if ((world.CollisionCheckForVoxel(down_����) && !world.CollisionCheckForVoxel(new Vector3(down_����.x - extend_delta, down_����.y, down_����.z))) || (world.CollisionCheckForVoxel(down_����) && !world.CollisionCheckForVoxel(new Vector3(down_����.x - extend_delta, down_����.y, down_����.z))))
            //    {

            //        return true;
            //    }
            //    else
            //        return false;



            //}


            if (
                world.CollisionCheckForVoxel(left_����) ||
                world.CollisionCheckForVoxel(left_����) ||
                world.CollisionCheckForVoxel(left_����) ||
                world.CollisionCheckForVoxel(left_����) ||
                world.CollisionCheckForVoxel(left_Center)
                )
                return true;

            else if (isSquating)
            {

                //(���Ϲ��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
                if ((world.CollisionCheckForVoxel(down_����) && !world.CollisionCheckForVoxel(new Vector3(down_����.x - extend_delta, down_����.y, down_����.z))) || (world.CollisionCheckForVoxel(down_����) && !world.CollisionCheckForVoxel(new Vector3(down_����.x - extend_delta, down_����.y, down_����.z))))
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
                world.CollisionCheckForVoxel(right_����) ||
                world.CollisionCheckForVoxel(right_����) ||
                world.CollisionCheckForVoxel(right_����) ||
                world.CollisionCheckForVoxel(right_����) ||
                world.CollisionCheckForVoxel(right_Center)
                )
                return true;

            else if (isSquating)
            {

                //(���Ϲ��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
                if ((world.CollisionCheckForVoxel(down_����) && !world.CollisionCheckForVoxel(new Vector3(down_����.x + extend_delta, down_����.y, down_����.z))) || (world.CollisionCheckForVoxel(down_����) && !world.CollisionCheckForVoxel(new Vector3(down_����.x + extend_delta, down_����.y, down_����.z))))
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
            //���world����true������ײ
            if (world.CollisionCheckForVoxel(front_����) ||
                world.CollisionCheckForVoxel(front_����) ||
                world.CollisionCheckForVoxel(back_����)  ||
                world.CollisionCheckForVoxel(back_����)  ||
                world.CollisionCheckForVoxel(left_����)  ||
                world.CollisionCheckForVoxel(left_����)  ||
                world.CollisionCheckForVoxel(right_����) ||
                world.CollisionCheckForVoxel(right_����)
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
            //���world����true������ײ
            if (world.CollisionCheckForVoxel(front_Center) ||
                world.CollisionCheckForVoxel(back_Center) ||
                world.CollisionCheckForVoxel(left_Center) ||
                world.CollisionCheckForVoxel(right_Center)
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

        foot_BlockType_temp = world.GetBlockType(foot.position);

        //��������䶯
        if (foot_BlockType_temp != foot_BlockType)
        {


            //update
            foot_BlockType = foot_BlockType_temp;

        }

    }



    //-------------------------------------------------------------------------------------





    //--------------------------------- ���߼�� ------------------------------------------


    [System.Serializable]
    public struct RayCastStruct
    {
        // �Ƿ�����
        public bool isHit;

        // �������
        public Vector3 rayOrigin;

        // ���е�����
        public Vector3 hitPoint;

        // ����ǰһ������
        public Vector3 hitPoint_Previous;

        // ���з�������
        public byte blockType; // �����Ҫʹ��ö�٣�Ҳ���Ը�Ϊö������

        // ���з��߷���
        public Vector3 hitNormal;

        // ���߾���
        public float rayDistance;

        // ���캯��
        public RayCastStruct(bool isHit, Vector3 rayOrigin, Vector3 hitPoint, Vector3 hitPoint_Previous, byte blockType, Vector3 hitNormal, float rayDistance)
        {
            this.isHit = isHit;
            this.rayOrigin = rayOrigin;
            this.hitPoint = hitPoint;
            this.hitPoint_Previous = hitPoint_Previous;
            this.blockType = blockType;
            this.hitNormal = hitNormal;
            this.rayDistance = rayDistance;
        }

        // ����ToString���������ڴ�ӡ���
        public override string ToString()
        {
            return $"RayCastStruct: \n" +
                   $"  Is Hit: {isHit}\n" +
                   $"  Ray Origin: {rayOrigin}\n" +
                   $"  Hit Point: {hitPoint}\n" +
                   $"  Previous Hit Point: {hitPoint_Previous}\n" +
                   $"  Block Type: {blockType}\n" +
                   $"  Hit Normal: {hitNormal}\n" +
                   $"  Ray Distance: {rayDistance}";
        }
    }



    //���߼��
    // ���ؽṹ���RayCast
    public RayCastStruct NewRayCast()
    {
        // ����������������
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();  // ���ڼ�¼��һ�����λ��
        Vector3 hitPoint = Vector3.zero;  // ���ڼ�¼���еĵ�
        byte blockType = 255;             // ���ڼ�¼���з�������ͣ�Ĭ��Ϊ255��ʾδ����
        Vector3 hitNormal = Vector3.zero; // ���ڼ�¼���߷���
        float rayDistance = 0f;           // ���ڼ�¼���߾���
        bool isHit = false;               // ���ڼ�¼�Ƿ�����

        // ��������㣨�����λ�ã���ʼ�������������ǰ������м��
        while (step < reach)
        {
            // ��ǰ�������ڵĵ�
            Vector3 pos = cam.position + (cam.forward * step);

            // ���y����С��0������Ϊ0�����⴩͸����
            if (pos.y < 0)
            {
                pos = new Vector3(pos.x, 0, pos.z);
            }

            // ��⵱ǰ���Ƿ�������ĳ������
            if (managerhub.world.RayCheckForVoxel(pos))
            {
                // ��¼���е�
                hitPoint = pos;
                isHit = true; // ��¼����

                // ��ȡ���еķ�������
                blockType = managerhub.world.GetBlockType(pos);

                // �������еķ��߷��򣬻������е�����λ���жϷ��ߵ�λ����
                Vector3 blockCenter = new Vector3(Mathf.Floor(hitPoint.x) + 0.5f, Mathf.Floor(hitPoint.y) + 0.5f, Mathf.Floor(hitPoint.z) + 0.5f);
                Vector3 relativePos = hitPoint - blockCenter;

                // �ҳ�Ӱ��������
                if (Mathf.Abs(relativePos.x) > Mathf.Abs(relativePos.y) && Mathf.Abs(relativePos.x) > Mathf.Abs(relativePos.z))
                {
                    // x��ռ�������������Ҳ���
                    hitNormal = new Vector3(Mathf.Sign(relativePos.x), 0, 0);
                }
                else if (Mathf.Abs(relativePos.y) > Mathf.Abs(relativePos.x) && Mathf.Abs(relativePos.y) > Mathf.Abs(relativePos.z))
                {
                    // y��ռ���������ж�����ײ�
                    hitNormal = new Vector3(0, Mathf.Sign(relativePos.y), 0);
                }
                else
                {
                    // z��ռ����������ǰ�����
                    hitNormal = new Vector3(0, 0, Mathf.Sign(relativePos.z));
                }

                // �������߾���
                rayDistance = (pos - cam.position).magnitude;

                // ���к�����ѭ��
                break;
            }

            // ������һ֡��λ��
            lastPos = pos;

            // �������߲���
            step += checkIncrement;
        }

        // ���û�������κη��飬����δ���еĽ��
        if (!isHit)
        {
            return new RayCastStruct
            {
                isHit = false,
                rayOrigin = cam.position,
                hitPoint = Vector3.zero,
                hitPoint_Previous = Vector3.zero,
                blockType = 255,  // δ���з���
                hitNormal = Vector3.zero,
                rayDistance = 0f
            };
        }

        // �������߼�����Ľṹ��
        return new RayCastStruct
        {
            isHit = true,                    // ��������״̬Ϊtrue
            rayOrigin = cam.position,        // ���ߵ����
            hitPoint = hitPoint,             // ���еĵ�
            hitPoint_Previous = lastPos,     // ��һ����
            blockType = blockType,           // ��������
            hitNormal = hitNormal,           // ���еķ��߷���
            rayDistance = rayDistance        // ���߾���
        };
    }



    //���߼�⡪�����ش��еķ�����������
    //û���о���(0,0,0)
    //Vector3 RayCast_now()
    //{

    //    float step = checkIncrement;
    //    //Vector3 lastPos = new Vector3();

    //    while (step < reach)
    //    {

    //        Vector3 pos = cam.position + (cam.forward * step);

    //        //�쳣���
    //        if (pos.y < 0)
    //        {
    //            pos = new Vector3(pos.x, 0, pos.z);
    //        }

    //        // ���������Ա����
    //        //if (debug_ray)
    //        //{
    //        //    Debug.DrawRay(cam.position, cam.forward * step, Color.red, 100f);
    //        //}

    //        //(������ || (�ǹ��� && ���ǻ��� && ����ˮ)�򷵻�
    //        //if (world.GetBlockType(pos) == VoxelData.Bamboo || (world.GetBlockType(pos) != VoxelData.Air && world.GetBlockType(pos) != VoxelData.BedRock && world.GetBlockType(pos) != VoxelData.Water))
    //        if (managerhub.world.RayCheckForVoxel(pos))
    //        {
                

    //            //print($"now���߼�⣺{(pos-cam.position).magnitude}");
    //            ray_length = (pos - cam.position).magnitude;
    //            return pos;

    //        }

    //        //lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

    //        step += checkIncrement;

    //    }

    //    point_Block_type = 255;
    //    return Vector3.zero;

    //}


    ////���߼�⡪�����ش��еķ����ǰһ֡
    //Vector3 RayCast_last()
    //{

    //    float step = checkIncrement;
    //    Vector3 lastPos = new Vector3();

    //    while (step < reach)
    //    {

    //        Vector3 pos = cam.position + (cam.forward * step);

    //        // ���������Ա����
    //        //if (debug_ray)
    //        //{
    //        //    Debug.DrawRay(cam.position, cam.forward * step, Color.red, 100f);
    //        //}

    //        //���
    //        if (world.GetBlockType(pos) != VoxelData.Air && world.GetBlockType(pos) != VoxelData.Water)
    //        {

    //            //print($"last���߼�⣺{(lastPos - cam.position).magnitude}");
    //            ray_length = (lastPos - cam.position).magnitude;
    //            return lastPos;

    //        }

    //        //lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
    //        lastPos = pos;

    //        step += checkIncrement;

    //    }

    //    return new Vector3(0f, 0f, 0f);

    //}


    // ���߼�⡪�����ش���ʼ�㵽����ǰһ֡��ľ���
    //float RayCast_last(Vector3 originPos, Vector3 direction, float distance)
    //{
    //    float step = checkIncrement;
    //    Vector3 lastPos = originPos;

    //    while (step < distance)
    //    {
    //        Vector3 pos = originPos + (direction.normalized * step);

    //        // ���������Ա���ԣ�ʹ����ɫ
    //        //Debug.DrawLine(lastPos, pos, Color.red);

    //        // ���
    //        if (world.GetBlockType(pos) != VoxelData.Air && world.GetBlockType(pos) != VoxelData.Water)
    //        {
    //            // ���ش���ʼ�㵽����ǰһ֡��ľ���
    //            //print((lastPos - originPos).magnitude);
    //            return (lastPos - originPos).magnitude;
                
    //        }

    //        // ���浱ǰ֡��λ����Ϊ������Чλ��
    //        lastPos = pos;

    //        step += checkIncrement;
    //    }

    //    // ���û�м�⵽�κ���Ч�Ŀ飬������
    //    return distance;
    //}


    //-------------------------------------------------------------------------------------





    //---------------------------------- debug ---------------------------------------------




    //������ײ
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



    //��¼���״̬
    void GetPlayerState()
    {

        //�Ƿ���Ӿ
        //��ǰ���� || y+1�����ǲ���ˮ
        if (world.GetBlockType(foot.transform.position) == VoxelData.Water || world.GetBlockType(new Vector3(foot.transform.position.x, foot.transform.position.y + 1f, foot.transform.position.z)) == VoxelData.Water)
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
            if (foot.transform.position.y > new_foot_high || isSwiming)
            {

                new_foot_high = transform.position.y;

            }



            //�ж�����Ƿ�����
            if (world.game_mode == GameMode.Survival)
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
        if (world.GetRelalocation(new Vector3(pos.x, pos.y + 1f, pos.z)) == world.GetRelalocation(cam.position))
        {
        
            return true;
        
        }


        if (world.GetRelalocation(pos) == world.GetRelalocation(down_����))
        {

            return true;

        }


        else if (world.GetRelalocation(pos) == world.GetRelalocation(down_����))
        {

            return true;

        }
        else if (world.GetRelalocation(pos) == world.GetRelalocation(down_����))
        {

            return true;

        }
        else if (world.GetRelalocation(pos) == world.GetRelalocation(down_����))
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


