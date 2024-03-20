//using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class Player : MonoBehaviour
{
    //���״̬
    [Header("���״̬")]
    public bool isGrounded;
    public bool isSprinting;
    public bool isSwiming;
    public bool isMoving;
    public bool isSquating;
    public bool isFlying;

    [Header("Transforms")]
    public Material HighLightMaterial;
    public Texture[] DestroyTextures = new Texture[10];
    public Transform cam;
    public Animation camaraAnimation;
    public Transform HighlightBlock;
    //public GameObject HighlightBlockObject;
    public World world;
    public CanvasManager canvasManager;
    public MusicManager musicmanager;
    public Transform leg;
    public GameObject selectblock;
    public GameObject Eye_Light;
    public BackPackManager backpackmanager;
    public LifeManager lifemanager;

    [Header("��ɫ����")]
    public Transform foot;
    private bool hasExec = true;
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float squatWalkSpeed = 1f;
    public float squatSpeed = 3f;
    public float jumpForce = 6f;
    public float gravity = -15f;
    public float MaxHurtHigh = 7f;

    [Header("��ײ����")]
    public float playerWidth = 0.3f;
    public float playerHeight = 1.7f;
    public float extend_delta = 0.1f;
    public float delta = 0.05f;

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
    private Vector3 OldPointLocation;

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
    public float verticalMomentum = 0;
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
    public bool showBlockMesh = false;
    public float water_jumpforce = 3f;
    public float watergravity = -3f;
    public float mult = 2f;

    //ˤ���˺�
    [Header("ˤ�����")]
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
    public float minFOV = 70;
    public float maxFOV = 90;
    bool expandview = false;


    [Header("����ģʽ")]
    private float lastJumpTime;
    private float doubleTapInterval = 0.5f; // Adjust as needed
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


    //--------------------------------- ���ں��� --------------------------------------

    void Start()
    {
        HighLightMaterial.color = new Color(0, 0, 0, 0);
        HighLightMaterial.mainTexture = DestroyTextures[0];
    }

    private void FixedUpdate()
    {
        if (world.game_state == Game_State.Playing)
        {
            //��ʼ������λ��
            if (hasExec)
            {
                InitPlayerLocation();
                hasExec = false;
            }
            
            //if (world.GetBlockType(foot.position) == VoxelData.Water)
            //{
            //    print("");
            //}

            //������ҽ�������
            Update_FootBlockType();

            //�ı��Ӿ�(������ܵĻ�)
            change_eyesview();

            //������ײ��
            update_block();

            //�������״̬
            GetPlayerState();

            //������������
            CalculateVelocity();

            //ʵ�ֲ���
            AchieveInput();

        }

    }


    //��ȡ�������
    private void Update()
    {
        if (world.game_state == Game_State.Playing)
        {
            if (!canvasManager.isPausing)
            {
                GetPlayerInputs();
                placeCursorBlocks();
            }
            else
            {
                horizontalInput = 0f;
                verticalInput = 0f;
                mouseHorizontal = 0f;
                mouseVerticalspeed = 0f;
            }


            if (showBlockMesh)
            {
                drawdebug();
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
            if (verticalMomentum > gravity)
                verticalMomentum += Time.fixedDeltaTime * gravity;
        }
        else
        {
            if (verticalMomentum > watergravity)
                verticalMomentum += mult * Time.fixedDeltaTime * watergravity;
        }


        // �����ٶ�
        if (isSprinting)
        {
            velocity = ((transform.forward * verticalInput) + (transform.right * horizontalInput)) * Time.fixedDeltaTime * sprintSpeed;
        }
        else if (isSquating)
        {
            velocity = ((transform.forward * verticalInput) + (transform.right * horizontalInput)) * Time.fixedDeltaTime * squatWalkSpeed;
        }
        else
        {
            velocity = ((transform.forward * verticalInput) + (transform.right * horizontalInput)) * Time.fixedDeltaTime * walkSpeed;
        }
            

        // �ϲ�����
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;



        //��Ĥ����
        //ǰ��
        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;

        //����
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;


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
    void change_eyesview()
    {
        if (isSprinting && isMoving && !expandview)
        {
            //����Э��������Ұ
            StartCoroutine(expandchangeview(true));

            //update
            expandview = true;
        }
        else if((!isSprinting || !isMoving) && expandview)
        {
            //��С��Ұ
            StartCoroutine(expandchangeview(false));

            //update
            expandview = false;
        }
    }

    IEnumerator expandchangeview(bool needexpand)
    {
        float startFOV = eyes.fieldOfView;
        float targetFOV = needexpand ? maxFOV : minFOV;
        float startTime = Time.time;

        while (Time.time - startTime < viewduration)
        {
            float t = (Time.time - startTime) / viewduration;
            eyes.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t); 
            yield return null;
        }

        eyes.fieldOfView = targetFOV; // ȷ��������Ұֵ׼ȷ
    }


    //���ղ���
    private void GetPlayerInputs()
    {

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVerticalspeed = Input.GetAxis("Mouse Y");
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


        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
            musicmanager.footstepInterval = VoxelData.sprintSpeed;
        }
            
        if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
            musicmanager.footstepInterval = VoxelData.walkSpeed;
        }
            

        if (isGrounded && Input.GetKey(KeyCode.Space))
        {
            jumpRequest = true;
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
        if (isSwiming && Input.GetKey(KeyCode.Space) && (leg.position.y - 0.1f < world.sea_level))
        {
            jumpRequest = true;
        }else if (isSwiming && Input.GetKey(KeyCode.Space) && (front || back || left || right))
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
        if (Input.GetMouseButtonDown(0))
        {
            OldPointLocation = new Vector3(Mathf.FloorToInt(RayCast_now().x), Mathf.FloorToInt(RayCast_now().y), Mathf.FloorToInt(RayCast_now().z));
        }

        //����ɿ���������isChanger��ԭ
        if (Input.GetMouseButtonUp(0))
        {
            isChangeBlock = false;
            musicmanager.isbroking = false;
            musicmanager.Audio_player_broke.Stop();
        }

        //�����������
        if (Input.GetKey(KeyCode.Mouse0))
        {
            //Debug.Log("Player Mouse0");
            //isLeftMouseDown = true;
            //Debug.Log(new Vector3(Mathf.FloorToInt(RayCast_now().x), Mathf.FloorToInt(RayCast_now().y), Mathf.FloorToInt(RayCast_now().z)));
            Vector3 pointvector = new Vector3(Mathf.FloorToInt(RayCast_now().x), Mathf.FloorToInt(RayCast_now().y), Mathf.FloorToInt(RayCast_now().z));

            if (pointvector != OldPointLocation || pointvector == Vector3.zero)
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


            //����� && �������2f && �Ҳ��ǽŵ���
            if (RayCast_last() != Vector3.zero && (RayCast_last() - cam.position).magnitude > max_hand_length && !CanPutBlock(new Vector3(RayCast_last().x, RayCast_last().y - 1f, RayCast_last().z)))
            {
                //music
                musicmanager.PlaySoung_Place();

                if (backpackmanager.istheindexHaveBlock(selectindex))
                {
                    
                    world.GetChunkObject(RayCast_last()).EditData(world.GetRelalocation(RayCast_last()), backpackmanager.slots[selectindex].blockId);

                    if (world.game_mode == GameMode.Survival)
                    {
                        backpackmanager.update_slots(1, point_Block_type);
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

    // �ȴ�2���ִ�����������ķ���
    IEnumerator DestroySoilWithDelay(Vector3 position)
    {
        isDestroying = true;

        // ��¼Э�̿�ʼִ��ʱ��ʱ��
        float startTime = Time.time;
        float destroy_time = world.blocktypes[world.GetBlockType(position)].DestroyTime;

        //�Ƿ��������ھ�
        if (isSuperMining)
        {
            destroy_time = 0.1f;
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
                HighLightMaterial.mainTexture = DestroyTextures[Mathf.FloorToInt(t * 10)];
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

        //ֻ������ģʽ�Ż��������
        if (world.game_mode == GameMode.Survival)
        {
            backpackmanager.CreateDropBox(new Vector3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), Mathf.FloorToInt(position.z)), point_Block_type, false, backpackmanager.ColdTime_Absorb);

        }

        //�Ž������ɵ�����ִ��

        canvasManager.Change_text_selectBlockname(point_Block_type);

        

        world.GetChunkObject(position).EditData(world.GetRelalocation(position), VoxelData.Air);

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
                velocity.y = 0.1f;
            }

            //�½�
            else if (Input.GetKey(KeyCode.LeftControl) && checkDownSpeed(1) != 0)
            {
                velocity.y = -0.1f;
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



    //--------------------------------------------------------------------------------





    //----------------------------------��ײ���---------------------------------------

    //����16����ײ��
    void update_block()
    {

        // ������ĸ���
        up_���� = new Vector3(transform.position.x - (playerWidth / 2), transform.position.y + (playerHeight / 2), transform.position.z + (playerWidth / 2));
        up_���� = new Vector3(transform.position.x + (playerWidth / 2), transform.position.y + (playerHeight / 2), transform.position.z + (playerWidth / 2));
        up_���� = new Vector3(transform.position.x + (playerWidth / 2), transform.position.y + (playerHeight / 2), transform.position.z - (playerWidth / 2));
        up_���� = new Vector3(transform.position.x - (playerWidth / 2), transform.position.y + (playerHeight / 2), transform.position.z - (playerWidth / 2));

        // ������ĸ���
        down_���� = new Vector3(transform.position.x - (playerWidth / 2), transform.position.y - (playerHeight / 2), transform.position.z + (playerWidth / 2));
        down_���� = new Vector3(transform.position.x + (playerWidth / 2), transform.position.y - (playerHeight / 2), transform.position.z + (playerWidth / 2));
        down_���� = new Vector3(transform.position.x + (playerWidth / 2), transform.position.y - (playerHeight / 2), transform.position.z - (playerWidth / 2));
        down_���� = new Vector3(transform.position.x - (playerWidth / 2), transform.position.y - (playerHeight / 2), transform.position.z - (playerWidth / 2));


        //front
        front_���� = new Vector3(transform.position.x - (playerWidth / 2) + delta, transform.position.y + (playerHeight / 4), transform.position.z + (playerWidth / 2) + extend_delta);
        front_���� = new Vector3(transform.position.x + (playerWidth / 2) - delta, transform.position.y + (playerHeight / 4), transform.position.z + (playerWidth / 2) + extend_delta);
        front_���� = new Vector3(transform.position.x - (playerWidth / 2) + delta, transform.position.y - (playerHeight / 4), transform.position.z + (playerWidth / 2) + extend_delta);
        front_���� = new Vector3(transform.position.x + (playerWidth / 2) - delta, transform.position.y - (playerHeight / 4), transform.position.z + (playerWidth / 2) + extend_delta);


        //back
        back_���� = new Vector3(transform.position.x - (playerWidth / 2) + delta, transform.position.y + (playerHeight / 4), transform.position.z - (playerWidth / 2) - extend_delta);
        back_���� = new Vector3(transform.position.x + (playerWidth / 2) - delta, transform.position.y + (playerHeight / 4), transform.position.z - (playerWidth / 2) - extend_delta);
        back_���� = new Vector3(transform.position.x - (playerWidth / 2) + delta, transform.position.y - (playerHeight / 4), transform.position.z - (playerWidth / 2) - extend_delta);
        back_���� = new Vector3(transform.position.x + (playerWidth / 2) - delta, transform.position.y - (playerHeight / 4), transform.position.z - (playerWidth / 2) - extend_delta);


        //left
        left_���� = new Vector3(transform.position.x - (playerWidth / 2) - extend_delta, transform.position.y + (playerHeight / 4), transform.position.z - (playerWidth / 2) + delta);
        left_���� = new Vector3(transform.position.x - (playerWidth / 2) - extend_delta, transform.position.y + (playerHeight / 4), transform.position.z + (playerWidth / 2) - delta);
        left_���� = new Vector3(transform.position.x - (playerWidth / 2) - extend_delta, transform.position.y - (playerHeight / 4), transform.position.z - (playerWidth / 2) + delta);
        left_���� = new Vector3(transform.position.x - (playerWidth / 2) - extend_delta, transform.position.y - (playerHeight / 4), transform.position.z + (playerWidth / 2) - delta);


        //right
        right_���� = new Vector3(transform.position.x + (playerWidth / 2) + extend_delta, transform.position.y + (playerHeight / 4), transform.position.z + (playerWidth / 2) - delta);
        right_���� = new Vector3(transform.position.x + (playerWidth / 2) + extend_delta, transform.position.y + (playerHeight / 4), transform.position.z - (playerWidth / 2) + delta);
        right_���� = new Vector3(transform.position.x + (playerWidth / 2) + extend_delta, transform.position.y - (playerHeight / 4), transform.position.z + (playerWidth / 2) - delta);
        right_���� = new Vector3(transform.position.x + (playerWidth / 2) + extend_delta, transform.position.y - (playerHeight / 4), transform.position.z - (playerWidth / 2) + delta);


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

    //��ײ����ļ�⣨-Z����Ϊfront��
    //����ĽǶ�
    public bool front
    {
       
        get
        {
            ////�������
            //if (!isSquating)
            //{
            //    if (
            //    world.CheckForVoxel(front_����) ||
            //    world.CheckForVoxel(front_����) ||
            //    world.CheckForVoxel(front_����) ||
            //    world.CheckForVoxel(front_����)
            //    )
            //    {

            //        return true;
            //    }
            //    else
            //        return false;
            //}
            ////�������
            //else
            //{
            //    //(���¹��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
            //    if ((world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z + extend_delta))) || (world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x, down_����.y, down_����.z + extend_delta))))
            //    {

            //        return true;
            //    }
            //    else
            //        return false;
            //}


            if (
                world.CheckForVoxel(front_����) ||
                world.CheckForVoxel(front_����) ||
                world.CheckForVoxel(front_����) ||
                world.CheckForVoxel(front_����)
                )
            {

                return true;
            }else if (isSquating)
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
                world.CheckForVoxel(back_����)
                )
                return true;
            else if (isSquating)
            {
                if (
                world.CheckForVoxel(back_����) ||
                world.CheckForVoxel(back_����) ||
                world.CheckForVoxel(back_����) ||
                world.CheckForVoxel(back_����)
                )
                    return true;
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
                world.CheckForVoxel(left_����)
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

            //if (!isSquating)
            //{
            //    if (
            //    world.CheckForVoxel(right_����) ||
            //    world.CheckForVoxel(right_����) ||
            //    world.CheckForVoxel(right_����) ||
            //    world.CheckForVoxel(right_����)
            //    )
            //        return true;
            //    else
            //        return false;
            //}
            //else
            //{
            //    //(���Ϲ��� && �������첻�ǹ���) || (���¹��� && �������첻�ǹ���)
            //    if ((world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x + extend_delta, down_����.y, down_����.z))) || (world.CheckForVoxel(down_����) && !world.CheckForVoxel(new Vector3(down_����.x + extend_delta, down_����.y, down_����.z))))
            //    {

            //        return true;
            //    }
            //    else
            //        return false;
            //}

            if (
                world.CheckForVoxel(right_����) ||
                world.CheckForVoxel(right_����) ||
                world.CheckForVoxel(right_����) ||
                world.CheckForVoxel(right_����)
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

            // ���������Ա����
            //if (debug_ray)
            //{
            //    Debug.DrawRay(cam.position, cam.forward * step, Color.red, 100f);
            //}

            //(������ || (�ǹ��� && ���ǻ��� && ����ˮ)�򷵻�
            if (world.GetBlockType(pos) == VoxelData.Bamboo || (world.GetBlockType(pos) != VoxelData.Air && world.GetBlockType(pos) != VoxelData.BedRock && world.GetBlockType(pos) != VoxelData.Water))
            {
                

                //print($"now���߼�⣺{(pos-cam.position).magnitude}");
                ray_length = (pos - cam.position).magnitude;
                return pos;

            }

            //lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        point_Block_type = 255;
        return new Vector3(0f, 0f, 0f);
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



    //-------------------------------------------------------------------------------------





    //---------------------------------- debug ---------------------------------------------

    //������ײ
    void drawdebug()
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
        Debug.DrawLine(front_����, front_����, Color.red);
        Debug.DrawLine(front_����, right_����, Color.red);
        Debug.DrawLine(right_����, right_����, Color.red);
        Debug.DrawLine(right_����, back_����, Color.red);
        Debug.DrawLine(back_����, back_����, Color.red);
        Debug.DrawLine(back_����, left_����, Color.red);
        Debug.DrawLine(left_����, left_����, Color.red);
        Debug.DrawLine(left_����, front_����, Color.red);

        //�°�Ȧ
        Debug.DrawLine(front_����, front_����, Color.red);
        Debug.DrawLine(front_����, right_����, Color.red);
        Debug.DrawLine(right_����, right_����, Color.red);
        Debug.DrawLine(right_����, back_����, Color.red);
        Debug.DrawLine(back_����, back_����, Color.red);
        Debug.DrawLine(back_����, left_����, Color.red);
        Debug.DrawLine(left_����, left_����, Color.red);
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
        Debug.DrawLine(front_����, front_����, Color.red);
        Debug.DrawLine(front_����, front_����, Color.red);
        //back
        Debug.DrawLine(back_����, back_����, Color.red);
        Debug.DrawLine(back_����, back_����, Color.red);
        //left
        Debug.DrawLine(left_����, left_����, Color.red);
        Debug.DrawLine(left_����, left_����, Color.red);
        //right
        Debug.DrawLine(right_����, right_����, Color.red);
        Debug.DrawLine(right_����, right_����, Color.red);

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

    //-------------------------------------------------------------------------------------






    //----------------------------------- ���״̬ -------------------------------------------

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
        }else if (world.GetRelalocation(pos) == world.GetRelalocation(down_����))
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
