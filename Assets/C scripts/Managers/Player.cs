//using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.XR;


public class Player : MonoBehaviour
{

    //���״̬
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


    [Header("Transforms")]
    public ManagerHub managerhub;
    public CommandManager commandManager;
    public World world;
    public MusicManager musicmanager;
    public CanvasManager canvasManager;
    public BackPackManager backpackmanager;
    public LifeManager lifemanager;
    public GameObject particle_explosion;

    public Material HighLightMaterial;
    public Texture[] DestroyTextures = new Texture[10];
    public Transform cam;
    //public Animation camaraAnimation;
    public Transform HighlightBlock;
    //public GameObject HighlightBlockObject;
    
    public Transform leg;
    public GameObject selectblock;
    public GameObject Eye_Light;
    
    public GameObject Particle_Broken;
    public Transform particel_Broken_transform;
    public ParticleSystem Broking_Animation;

    //Hand
    public GameObject hand_Hold;
    public GameObject hand;
    public GameObject handBlock;


    [Header("��ɫ����")]
    public Transform foot;
    private bool hasExec = true;
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
    private Vector3 velocity;
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
        transform.position = new Vector3(Random.Range(800, 3200), transform.position.y, Random.Range(800, 3200));


        //��ʼ������
        isSwiming = false;
        new_foot_high = -100f;

        isSuperMining = false;
        isFlying = false;
        momentum = Vector3.zero;
        managerhub.backpackManager.ChangeBlockInHand();
    }


    private void FixedUpdate()  
    {
         
        if (world.game_state == Game_State.Playing)
        {

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
            if (canvasManager.isPausing == true || commandManager.isConsoleActive == true)
            {
                horizontalInput = 0f;
                verticalInput = 0f;
                mouseHorizontal = 0f;
                mouseVerticalspeed = 0f;

            }
            else
            {

                GetPlayerInputs();
                placeCursorBlocks();

            }


            //������������
            CalculateVelocity();

            //ʵ�ֲ���
            AchieveInput();


            // ÿ��һ��ʱ����һ��
            if (Time.time >= isInCave_nextCheckTime)
            {
                //print($"{Time.time}");
                CheckisInCave();
                isInCave_nextCheckTime = Time.time + isInCave_checkInterval; // ������һ�μ���ʱ��
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
        if (playerY < NoiseY)
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




    //���ݼ���
    private void CalculateVelocity()
    {

        //����ӽǴ���
        Camera_verticalInput -= mouseVerticalspeed;
        Camera_verticalInput = Mathf.Clamp(Camera_verticalInput, -90f, 90f);

        // ��������
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





    //���ղ���
    public bool hasExec_isChangedBlock = true;
    private void GetPlayerInputs()
    {

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X") * canvasManager.Mouse_Sensitivity;
        mouseVerticalspeed = Input.GetAxis("Mouse Y") * canvasManager.Mouse_Sensitivity;
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
            musicmanager.footstepInterval = VoxelData.sprintSpeed;

        }
            
        if (Input.GetButtonUp("Sprint") && !isFlying)
        {

            isSprinting = false;
            musicmanager.footstepInterval = VoxelData.walkSpeed;

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


        //������������,��¼OldPointLocation
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Vector3 _raycastNow = RayCast_now();
            
        
        //}

        //����ɿ���������isChanger��ԭ
        if (Input.GetMouseButtonUp(0))
        {

            isChangeBlock = false;
            hasExec_isChangedBlock = true;
            musicmanager.isbroking = false;
            musicmanager.Audio_player_broke.Stop();

        }

        //�����������

        if (Input.GetKey(KeyCode.Mouse0))
        {
            //Debug.Log("Player Mouse0");
            //isLeftMouseDown = true;
            //Debug.Log(new Vector3(Mathf.FloorToInt(RayCast_now().x), Mathf.FloorToInt(RayCast_now().y), Mathf.FloorToInt(RayCast_now().z)));
            //Vector3 _raycastNow = RayCast_now();

            if(RayCast_now()!= Vector3.zero && hasExec_isChangedBlock && world.blocktypes[world.GetBlockType(RayCast_now())].canBeChoose)
            {
                OldPointLocation = new Vector3(Mathf.FloorToInt(RayCast_now().x), Mathf.FloorToInt(RayCast_now().y), Mathf.FloorToInt(RayCast_now().z));
                hasExec_isChangedBlock = false;
            }


            Vector3 pointvector = new Vector3(Mathf.FloorToInt(RayCast_now().x), Mathf.FloorToInt(RayCast_now().y), Mathf.FloorToInt(RayCast_now().z));

            if (pointvector != OldPointLocation && OldPointLocation != Vector3.zero && pointvector != Vector3.zero)
            {

                isChangeBlock = true;
                musicmanager.isbroking = false;
                OldPointLocation = pointvector;

            }

            //�����
            if (RayCast_now() != Vector3.zero)
            {

                //�������������ִ��
                if (!isDestroying)
                {

                    //Debug.Log("ִ������");
                    elapsedTime = 0.0f;
                    StartCoroutine(DestroySoilWithDelay(RayCast_now()));


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
            Vector3 RayCast = RayCast_last();
            Vector3 _raycastNow = RayCast_now();
            byte _targettype = world.GetBlockType(_raycastNow);

            

            //����ǿɻ�������
            if (_targettype < world.blocktypes.Length && world.blocktypes[_targettype].isinteractable)
            {
                //print("isinteractable");

                switch (_targettype)
                {
                    //��¯
                    case 39:
                        //canvasManager.UIManager[VoxelData.ui���].childs[1]._object.SetActive(!canvasManager.UIManager[VoxelData.ui���].childs[1]._object.activeSelf);
                        world.Allchunks[world.GetChunkLocation(_raycastNow)].EditData(world.GetRelalocation(_raycastNow), VoxelData.Air);
                        BlocksFunction.Smoke(managerhub, _raycastNow, 4);

                        break;
                    //TNT
                    case 17:
                        BlocksFunction.Boom(managerhub, _raycastNow, 3);
                        GameObject.Instantiate(particle_explosion, RayCast, Quaternion.identity);

                        // ��ұ�ը��
                        Vector3 _Direction = cam.transform.position - _raycastNow;  //ը�ɷ���
                        float _value = _Direction.magnitude / 3;  //�������ĵ�̶�[0,1]

                        //����ը�ɾ���
                        _Direction.y = Mathf.Lerp(0, 1, _value);
                        float Distance = Mathf.Lerp(3, 0, _value);

                        ForceMoving(_Direction, Distance, 0.1f);

                        if (managerhub.world.game_mode == GameMode.Survival && _Direction.magnitude <= 3)
                        {
                            managerhub.lifeManager.UpdatePlayerBlood((int)Mathf.Lerp(30, 10, _value), true, true);
                        }

                        //print($"_Direction:{_Direction}, _distance: {_distance}");

                        break;
                }

                return;
            }

            //����� && �������2f && �Ҳ��ǽŵ���
            if (RayCast != Vector3.zero && (RayCast - cam.position).magnitude > max_hand_length && !CanPutBlock(new Vector3(RayCast.x, RayCast.y - 1f, RayCast.z)))
            {

                //music
                musicmanager.PlaySoung_Place();

                if (backpackmanager.istheindexHaveBlock(selectindex))
                {

                    


                    //Edit
                    world.GetChunkObject(RayCast).EditData(world.GetRelalocation(RayCast), backpackmanager.slots[selectindex].blockId);


                    //EditNumber
                    world.GetChunkObject(RayCast).UpdateEditNumber(RayCast, backpackmanager.slots[selectindex].blockId);


                    if (world.game_mode == GameMode.Survival)
                    {

                        backpackmanager.update_slots(1, point_Block_type);
                        backpackmanager.ChangeBlockInHand();

                    }
                    
                }

                //print($"��������Ϊ��{RayCast_last()}");
                //print($"�������Ϊ��{world.GetRelalocation(RayCast())}");
                //print($"��������Ϊ��{world.GetBlockType(RayCast())}");

                
                
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

            canvasManager.Change_text_selectBlockname(255);
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

            canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }



    }

    public float DEbug_DIstance;

    // �ȴ�2���ִ�����������ķ���
    IEnumerator DestroySoilWithDelay(Vector3 position)
    {
        //print("�������ƻ�Э��");
        isDestroying = true;
        if (point_Block_type != 255)
        {
            Broking_Animation.textureSheetAnimation.SetSprite(0, world.blocktypes[point_Block_type].buttom_sprit);
        }
        
        Broking_Animation.Play();

        // ��¼Э�̿�ʼִ��ʱ��ʱ��
        float startTime = Time.time;
        float destroy_time = world.blocktypes[world.GetBlockType(position)].DestroyTime;

        //�Ƿ��������ھ�
        if (isSuperMining)
        {

            destroy_time = 0.25f;

        }

        // �ȴ�
        while (Time.time - startTime < destroy_time)
        {

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

                Broking_Animation.Stop();

                yield break;

            }

            yield return null;

        }

        //����ɹ���������
        // ִ�������������߼�
        isDestroying = false;
        musicmanager.PlaySound_Broken(point_Block_type);
        elapsedTime = 0.0f;
        musicmanager.isbroking = false;

        //���ʻ�ԭ
        HighLightMaterial.color = new Color(0, 0, 0, 0);
        HighLightMaterial.mainTexture = DestroyTextures[0];

        //ֻ������ģʽ�Ż��������
        if (world.game_mode == GameMode.Survival && world.blocktypes[point_Block_type].candropBlock)
        {

            backpackmanager.CreateDropBox(new Vector3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), Mathf.FloorToInt(position.z)), point_Block_type, false, backpackmanager.ColdTime_Absorb);

        }

        //�Ž������ɵ�����ִ��
        //canvasManager.Change_text_selectBlockname(point_Block_type);

        //�ƻ�����Ч��
        GameObject particleInstance = Instantiate(Particle_Broken);
        particleInstance.GetComponent<ParticleSystem>().textureSheetAnimation.SetSprite(0, world.blocktypes[point_Block_type].buttom_sprit);
        particleInstance.transform.parent = particel_Broken_transform;
        particleInstance.transform.position = position;



        //if (world.blocktypes[point_Block_type].Particle_Material == null)
        //    particleInstance.GetComponent<ParticleSystemRenderer>().material = world.blocktypes[VoxelData.Soil].Particle_Material;
        //else
        //    particleInstance.GetComponent<ParticleSystemRenderer>().material = world.blocktypes[point_Block_type].Particle_Material;

        Destroy(particleInstance, 10.0f);

        Broking_Animation.Stop();

        //World
        var chunkObject = world.GetChunkObject(position);
        chunkObject.EditData(world.GetRelalocation(position), VoxelData.Air);
        chunkObject.UpdateEditNumber(position, VoxelData.Air);


        
        //EditNumber


        //print($"��������Ϊ��{position}");
        //print($"�������Ϊ��{world.GetRelalocation(position)}");
        //print($"��������Ϊ��{world.GetBlockType(position)}"); 

    }



    //��ȡ��Ұ�1~9
    private void SelectBlock()
    {

        // ��ⰴ��1��9
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {

            selectindex = 0;
            canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {

            selectindex = 1;
            canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {

            selectindex = 2;
            canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {

            selectindex = 3;
            canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {

            selectindex = 4;
            canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {

            selectindex = 5;
            canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {

            selectindex = 6;
            canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {

            selectindex = 7;
            canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {

            selectindex = 8;
            canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }

        
    }



    private void placeCursorBlocks()
    {

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


            if (world.eyesCheckForVoxel(pos))
            {
                point_Block_type = world.GetBlockType(pos);

                HighlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x) + 0.5f, Mathf.FloorToInt(pos.y) + 0.5f, Mathf.FloorToInt(pos.z) + 0.5f);
                HighlightBlock.gameObject.SetActive(true);


                return;

            }


            point_Block_type = VoxelData.notHit;
            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        HighlightBlock.gameObject.SetActive(false);

    }


    //ʵ�ֲ���
    private void AchieveInput()
    {

        //����ʵ��
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

        }


        //�¶�ʵ��
        //0.81~0.388
        if (isSquating)
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


        //ѡ�񷽿�ʵ��
        if (selectindex >= 0 && selectindex <= 9)
        {

            selectblock.GetComponent<RectTransform>().anchoredPosition = new Vector2(VoxelData.SelectLocation_x[selectindex], 0);

        }




        //�ӽǺ��ƶ�ʵ��
        transform.Rotate(Vector3.up * mouseHorizontal);
        //cam.Rotate(Vector3.right * -mouseVertical);
        cam.localRotation = Quaternion.Euler(Camera_verticalInput, 0, 0);


        if (isFlying)
        {

            //����
            if (Input.GetKey(KeyCode.Space) && checkUpSpeed(1) != 0)
            {

                velocity.y = flyVelocity;

            }

            //�½�
            else if (Input.GetKey(KeyCode.LeftControl) && checkDownSpeed(1) != 0)
            {

                velocity.y = -flyVelocity;

            }



            //�ɿ�
            else
            {

                velocity.y = 0;

            }

            
        }

        transform.Translate(velocity, Space.World);

    }


    public void InitPlayerLocation()
    {

        transform.position = world.Start_Position;

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
            world.CheckForVoxel(down_����) ||
            world.CheckForVoxel(down_����) ||
            world.CheckForVoxel(down_����) ||
            world.CheckForVoxel(down_����)

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
            world.CheckForVoxel(up_����) ||
            world.CheckForVoxel(up_����) ||
            world.CheckForVoxel(up_����) ||
            world.CheckForVoxel(up_����)
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

            // ����Facing�������������ʵ���˶�����
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

            // �� momentum ��ӵ�ʵ���˶�����
            direction += momentum;

            // ��һ��������������ȷ���˶�����Ϊ��λ����
            return direction.normalized;
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
            if (world.CheckForVoxel(front_����) || 
                world.CheckForVoxel(front_����) || 
                world.CheckForVoxel(front_����) || 
                world.CheckForVoxel(front_����) ||
                world.CheckForVoxel(front_Center))
            {
                return true;
            }

            //����¶�
            else if (isSquating)
            {
                //(���¹��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
                if ((world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z + extend_delta))) || (world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z + extend_delta))))
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
            //    world.CheckForVoxel(back_����) ||
            //    world.CheckForVoxel(back_����) ||
            //    world.CheckForVoxel(back_����) ||
            //    world.CheckForVoxel(back_����)
            //    )
            //        return true;
            //    else
            //        return false;
            //}
            //else
            //{
            //    //(���Ϲ��� && �������첻�ǹ���) || (���Ϲ��� && �������첻�ǹ���)
            //    if ((world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z - extend_delta))) || (world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z - extend_delta))))
            //    {

            //        return true;
            //    }
            //    else
            //        return false;
            //}

            if (
                world.CheckForVoxel(back_����) ||
                world.CheckForVoxel(back_����) ||
                world.CheckForVoxel(back_����) ||
                world.CheckForVoxel(back_����) ||
                world.CheckForVoxel(back_Center)
                )
                return true;

            else if (isSquating)
            {

                //(���Ϲ��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
                if ((world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z - extend_delta))) || (world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z - extend_delta))))
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
            //    world.CheckForVoxel(left_����) ||
            //    world.CheckForVoxel(left_����) ||
            //    world.CheckForVoxel(left_����) ||
            //    world.CheckForVoxel(left_����)
            //    )
            //        return true;
            //    else
            //        return false;
            //}
            //else
            //{
                
            //    //(���Ϲ��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
            //    if ((world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x - extend_delta, down_����.y, down_����.z))) || (world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x - extend_delta, down_����.y, down_����.z))))
            //    {

            //        return true;
            //    }
            //    else
            //        return false;



            //}


            if (
                world.CheckForVoxel(left_����) ||
                world.CheckForVoxel(left_����) ||
                world.CheckForVoxel(left_����) ||
                world.CheckForVoxel(left_����) ||
                world.CheckForVoxel(left_Center)
                )
                return true;

            else if (isSquating)
            {

                //(���Ϲ��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
                if ((world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x - extend_delta, down_����.y, down_����.z))) || (world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x - extend_delta, down_����.y, down_����.z))))
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
                world.CheckForVoxel(right_����) ||
                world.CheckForVoxel(right_����) ||
                world.CheckForVoxel(right_����) ||
                world.CheckForVoxel(right_����) ||
                world.CheckForVoxel(right_Center)
                )
                return true;

            else if (isSquating)
            {

                //(���Ϲ��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
                if ((world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x + extend_delta, down_����.y, down_����.z))) || (world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x + extend_delta, down_����.y, down_����.z))))
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
            if (world.CheckForVoxel(front_����) ||
                world.CheckForVoxel(front_����) ||
                world.CheckForVoxel(back_����)  ||
                world.CheckForVoxel(back_����)  ||
                world.CheckForVoxel(left_����)  ||
                world.CheckForVoxel(left_����)  ||
                world.CheckForVoxel(right_����) ||
                world.CheckForVoxel(right_����)
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
            if (world.CheckForVoxel(front_Center) ||
                world.CheckForVoxel(back_Center) ||
                world.CheckForVoxel(left_Center) ||
                world.CheckForVoxel(right_Center)
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



    //���߼�⡪�����ش��еķ�����������
    //û���о���(0,0,0)
    Vector3 RayCast_now()
    {

        float step = checkIncrement;
        //Vector3 lastPos = new Vector3();

        while (step < reach)
        {

            Vector3 pos = cam.position + (cam.forward * step);

            //�쳣���
            if (pos.y < 0)
            {
                pos = new Vector3(pos.x, 0, pos.z);
            }

            // ���������Ա����
            //if (debug_ray)
            //{
            //    Debug.DrawRay(cam.position, cam.forward * step, Color.red, 100f);
            //}

            //(������ || (�ǹ��� && ���ǻ��� && ����ˮ)�򷵻�
            //if (world.GetBlockType(pos) == VoxelData.Bamboo || (world.GetBlockType(pos) != VoxelData.Air && world.GetBlockType(pos) != VoxelData.BedRock && world.GetBlockType(pos) != VoxelData.Water))
            if (world.blocktypes[world.GetBlockType(pos)].canBeChoose)
            {
                

                //print($"now���߼�⣺{(pos-cam.position).magnitude}");
                ray_length = (pos - cam.position).magnitude;
                return pos;

            }

            //lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        point_Block_type = 255;
        return Vector3.zero;

    }


    //���߼�⡪�����ش��еķ����ǰһ֡
    Vector3 RayCast_last()
    {

        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {

            Vector3 pos = cam.position + (cam.forward * step);

            // ���������Ա����
            //if (debug_ray)
            //{
            //    Debug.DrawRay(cam.position, cam.forward * step, Color.red, 100f);
            //}

            //���
            if (world.GetBlockType(pos) != VoxelData.Air && world.GetBlockType(pos) != VoxelData.Water)
            {

                //print($"last���߼�⣺{(lastPos - cam.position).magnitude}");
                ray_length = (lastPos - cam.position).magnitude;
                return lastPos;

            }

            //lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            lastPos = pos;

            step += checkIncrement;

        }

        return new Vector3(0f, 0f, 0f);

    }


    // ���߼�⡪�����ش���ʼ�㵽����ǰһ֡��ľ���
    float RayCast_last(Vector3 originPos, Vector3 direction, float distance)
    {
        float step = checkIncrement;
        Vector3 lastPos = originPos;

        while (step < distance)
        {
            Vector3 pos = originPos + (direction.normalized * step);

            // ���������Ա���ԣ�ʹ����ɫ
            //Debug.DrawLine(lastPos, pos, Color.red);

            // ���
            if (world.GetBlockType(pos) != VoxelData.Air && world.GetBlockType(pos) != VoxelData.Water)
            {
                // ���ش���ʼ�㵽����ǰһ֡��ľ���
                //print((lastPos - originPos).magnitude);
                return (lastPos - originPos).magnitude;
                
            }

            // ���浱ǰ֡��λ����Ϊ������Чλ��
            lastPos = pos;

            step += checkIncrement;
        }

        // ���û�м�⵽�κ���Ч�Ŀ飬������
        return distance;
    }


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
