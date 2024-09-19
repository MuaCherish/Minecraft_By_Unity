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

    //玩家状态
    [Header("玩家状态")]
    [ReadOnly] public bool isGrounded;
    [ReadOnly] public bool isSprinting;
    [ReadOnly] public bool isSwiming;
    [ReadOnly] public bool isMoving;
    [ReadOnly] public bool isSquating;
    [ReadOnly] public bool isFlying;
    [ReadOnly] public bool isCatchBlock;
    [ReadOnly] public bool isInCave; // 玩家是否在矿洞内
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


    [Header("角色参数")]
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
    public float gravity_V = 0.2f; //空气阻力


    [Header("碰撞参数")]
    public float playerWidth = 0.3f;
    public float playerHeight = 1.7f;
    public float high_delta = 0.9f; // 在确定高度下，碰撞点的相对高度
    private float extend_delta = 0.1f;
    private float delta = 0.05f;


    [Header("打掉方块需要的时间")]
    //public float destroyTime = 2f;
    // 用于跟踪玩家是否按下左键
    //private bool isLeftMouseDown;
    //已经过去的时间
    private float elapsedTime = 0.0f;
    private Material material; // 物体的材质
    private Color initialColor; // 初始颜色
    private bool isDestroying;
    public bool isChangeBlock = false;
    public Vector3 OldPointLocation;


    //输入
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
    public Vector3 momentum = Vector3.zero; // 玩家瞬时动量
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


    //摔落伤害
    [Header("摔落参数")]
    private float groundTime; public float minGroundedTime = 0.1f; // 最小的落地时间
    Coroutine falldownCoroutine;
    public float hurtCooldownTime = 0.5f;  //摔落冷却时间
    public float new_foot_high = -100f;
    public float angle = 50f;
    public float cycleLength = 16f; // 动画周期长度
    public float speed = 200f; // 控制时间的增长速度 


    //music
    [Header("玩家指向的方块")]
    public byte point_Block_type = 255;


    //特殊模式
    [Header("特殊mode")]
    public bool isSpaceMode = false;
    public bool isSuperMining = false;


    //select
    [Header("玩家选择下标")]
    public int selectindex = 0;


    //奔跑时改变视距
    [Header("视距参数")]
    public Camera eyes;
    public float viewduration = 1f; // 过渡时间
    public float CurrentFOV = 70;
    bool expandview = false;


    [Header("飞行模式")]
    private float lastJumpTime;
    public float doubleTapInterval = 0.5f; // 飞行双击时间间隔
    public float flyVelocity = 0.1f; //上下飞行速度
    public int jump_press = 0;


    //碰撞检测的坐标
    // 上面的四个点
    Vector3 up_左上 = new Vector3();
    Vector3 up_右上 = new Vector3();
    Vector3 up_右下 = new Vector3();
    Vector3 up_左下 = new Vector3();


    // 下面的四个点
    Vector3 down_左上 = new Vector3();
    Vector3 down_右上 = new Vector3();
    Vector3 down_右下 = new Vector3();
    Vector3 down_左下 = new Vector3();


    //front
    Vector3 front_左上 = new Vector3();
    Vector3 front_右上 = new Vector3();
    Vector3 front_左下 = new Vector3();
    Vector3 front_右下 = new Vector3();


    //back
    Vector3 back_左上 = new Vector3();
    Vector3 back_右上 = new Vector3();
    Vector3 back_左下 = new Vector3();
    Vector3 back_右下 = new Vector3();


    //left
    Vector3 left_左上 = new Vector3();
    Vector3 left_右上 = new Vector3();
    Vector3 left_左下 = new Vector3();
    Vector3 left_右下 = new Vector3();


    //right
    Vector3 right_左上 = new Vector3();
    Vector3 right_右上 = new Vector3();
    Vector3 right_左下 = new Vector3();
    Vector3 right_右下 = new Vector3();


    //玩家脚下坐标
    public byte foot_BlockType = VoxelData.Air;
    public byte foot_BlockType_temp = VoxelData.Air;

    //用来检查isInCave
    public float isInCave_checkInterval = 10f; private float isInCave_nextCheckTime = 10f;// 每0.5秒检查一次


    //--------------------------------- 周期函数 --------------------------------------




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


        //初始化数据
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

            //更新玩家脚下坐标
            Update_FootBlockType();

            



            //计算玩家状态
            GetPlayerState();

            

            //绘制碰撞盒
            if (Show_CollisionBox)
            {

                Draw_CollisionBox();

            }

            //绘制判定箱
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
                groundTime += Time.deltaTime; // 玩家在地面上的时间
            }
            else
            {
                groundTime = 0;
            }

            //改变视距(如果奔跑的话)
            change_eyesview();

            //计算碰撞点
            CollisionNumber = 0;
            if (!isFlying)
            {
                update_block();
            }

            //游戏中暂停，暂停玩家输入
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


            //计算输入数据
            CalculateVelocity();

            //实现操作
            AchieveInput();


            // 每隔一段时间检查一次
            if (Time.time >= isInCave_nextCheckTime)
            {
                //print($"{Time.time}");
                CheckisInCave();
                isInCave_nextCheckTime = Time.time + isInCave_checkInterval; // 设置下一次检查的时间
            }


        }

    }

    //更新isInCave状态
    public void CheckisInCave()
    {
        //print("");
        float playerY = cam.position.y + 10f;
        Vector3 RelaPosition = managerhub.world.GetRelalocation(cam.position);
        Vector3 _ChunkLocation = managerhub.world.GetChunkLocation(cam.position);

        //print($"RelaPosition: {RelaPosition} , ChunkLocation = {_ChunkLocation}");
        float NoiseY = managerhub.world.GetTotalNoiseHigh_Biome((int)RelaPosition.x, (int)RelaPosition.z, new Vector3((int)_ChunkLocation.x * 16f, 0f, (int)_ChunkLocation.z * 16f), managerhub.world.worldSetting.worldtype);

        //print($"PlayerY：{playerY} , NoiseY：{NoiseY} , 差: {NoiseY - playerY}");



        // 检查眼睛所在位置是否处于地表以下，将Fog改为近距离黑色迷雾
        if (playerY < NoiseY)
        {
            if (!isInCave)
            {
                //print("迷雾开启");
                // 开始迷雾过渡到洞穴状态
                managerhub.timeManager.Buff_CaveFog(true);
                isInCave = true;
            }
        }
        else
        {
            if (isInCave)
            {
                //print("迷雾关闭");
                // 开始迷雾过渡到白天状态
                if (!managerhub.timeManager.isNight)
                {
                    managerhub.timeManager.Buff_CaveFog(false);
                }
                
                isInCave = false;
            }
        }
    }


    //---------------------------------------------------------------------------------






    //--------------------------------- 玩家操作 --------------------------------------




    //数据计算
    private void CalculateVelocity()
    {

        //玩家视角处理
        Camera_verticalInput -= mouseVerticalspeed;
        Camera_verticalInput = Mathf.Clamp(Camera_verticalInput, -90f, 90f);

        // 计算重力
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


        // 计算速度
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


        // 合并数据
        velocity += momentum * Time.deltaTime;
        velocity += Vector3.up * verticalMomentum * Time.deltaTime;



        //滑膜数据
        //前后
        if (!isFlying)
        {
            if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
                velocity.z = 0;

            //左右
            if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
                velocity.x = 0;
        }

        


        //检查是否移动
        if (velocity.x != 0f || velocity.z != 0f)
        {

            isMoving = true;

        }
        else
        {

            isMoving = false;

        }

        //跳跃顶墙判断
        if (velocity.y < 0)
        {

            velocity.y = checkDownSpeed(velocity.y);

        }

        else if (velocity.y > 0)
        {

            velocity.y = checkUpSpeed(velocity.y);

        }

    }


    //改变视距
    private Coroutine currentViewChangeCoroutine; // 记录当前正在运行的协程

    void change_eyesview()
    {
        if (isSprinting && isMoving && !expandview)
        {
            // 如果有协程正在运行，先停止它
            if (currentViewChangeCoroutine != null)
            {
                StopCoroutine(currentViewChangeCoroutine);
            }

            // 启动协程扩大视野
            currentViewChangeCoroutine = StartCoroutine(expandchangeview(true));
            expandview = true;
        }
        else if ((!isSprinting || !isMoving) && expandview && !isFlying)
        {
            // 如果有协程正在运行，先停止它
            if (currentViewChangeCoroutine != null)
            {
                StopCoroutine(currentViewChangeCoroutine);
            }

            // 启动协程缩小视野
            currentViewChangeCoroutine = StartCoroutine(expandchangeview(false));
            expandview = false;
        }
    }

    IEnumerator expandchangeview(bool expand)
    {
        float startTime = Time.time;
        float initialFOV = eyes.fieldOfView; // 当前的FOV值
        float targetFOV = expand ? CurrentFOV + 20f : CurrentFOV; // 目标FOV值

        while (Time.time - startTime < viewduration)
        {
            float t = (Time.time - startTime) / viewduration;
            eyes.fieldOfView = Mathf.Lerp(initialFOV, targetFOV, t);
            yield return null;
        }

        eyes.fieldOfView = targetFOV; // 确保最终视野值准确
        currentViewChangeCoroutine = null; // 协程结束后清空记录
    }





    //接收操作
    public bool hasExec_isChangedBlock = true;
    private void GetPlayerInputs()
    {

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X") * canvasManager.Mouse_Sensitivity;
        mouseVerticalspeed = Input.GetAxis("Mouse Y") * canvasManager.Mouse_Sensitivity;
        scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");


        //玩家按一下R，随机切换一下手中的物品
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    placeBlock_Index = (byte)Random.Range(0,25);
        //}

        //玩家按一下F可以切换手电
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


        //第三人称
        //if (Input.GetKeyDown(KeyCode.F5))
        //{
        //    SwitchThridPersonMode();
        //}


        if (isGrounded && Input.GetKey(KeyCode.Space) && groundTime >= minGroundedTime)
        {
            //print("正常跳跃");
            jumpRequest = true;
            groundTime = 0; // 跳跃后重置计时
        }
            

        if (Input.GetKeyDown(KeyCode.Q))
        {

            backpackmanager.ThrowDropBox();
        
        }

        //飞行模式
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
                   
                    //改变视野
                    if (!isFlying)
                    {
                        //扩大视野
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
        




        // 如果在水中按下跳跃键 && leg低于水面，触发跳跃请求 
        if (isSwiming && Input.GetKey(KeyCode.Space) && (leg.position.y - 0.1f < world.terrainLayerProbabilitySystem.sea_level))
        {

            jumpRequest = true;

        }

        else if (isSwiming && Input.GetKey(KeyCode.Space) && (front || back || left || right))
        {

            jumpRequest = true;

        }



        //按住Ctrl键，摄像机将下降一定高度
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
        
            isSquating = true;
        
        }


        //松开Ctrl键，摄像机还原
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
        
            isSquating = false;
        
        }


        //如果点击鼠标左键,记录OldPointLocation
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Vector3 _raycastNow = RayCast_now();
            
        
        //}

        //如果松开鼠标左键，isChanger还原
        if (Input.GetMouseButtonUp(0))
        {

            isChangeBlock = false;
            hasExec_isChangedBlock = true;
            musicmanager.isbroking = false;
            musicmanager.Audio_player_broke.Stop();

        }

        //左键销毁泥土

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

            //如果打到
            if (RayCast_now() != Vector3.zero)
            {

                //如果正在销毁则不执行
                if (!isDestroying)
                {

                    //Debug.Log("执行销毁");
                    elapsedTime = 0.0f;
                    StartCoroutine(DestroySoilWithDelay(RayCast_now()));


                }


                //world.GetChunkObject(RayCast_now()).EditData(world.GetRelalocation(RayCast_now()), 4);


                //print($"绝对坐标为：{RayCast_now()}");
                //print($"相对坐标为：{world.GetRelalocation(RayCast())}");
                //print($"方块类型为：{world.GetBlockType(RayCast())}");
            }


        }

        //右键放置泥土
        if (Input.GetMouseButtonDown(1))
        {

            isPlacing = true;
            Vector3 RayCast = RayCast_last();
            Vector3 _raycastNow = RayCast_now();
            byte _targettype = world.GetBlockType(_raycastNow);

            

            //如果是可互动方块
            if (_targettype < world.blocktypes.Length && world.blocktypes[_targettype].isinteractable)
            {
                //print("isinteractable");

                switch (_targettype)
                {
                    //熔炉
                    case 39:
                        //canvasManager.UIManager[VoxelData.ui玩家].childs[1]._object.SetActive(!canvasManager.UIManager[VoxelData.ui玩家].childs[1]._object.activeSelf);
                        world.Allchunks[world.GetChunkLocation(_raycastNow)].EditData(world.GetRelalocation(_raycastNow), VoxelData.Air);
                        BlocksFunction.Smoke(managerhub, _raycastNow, 4);

                        break;
                    //TNT
                    case 17:
                        BlocksFunction.Boom(managerhub, _raycastNow, 3);
                        GameObject.Instantiate(particle_explosion, RayCast, Quaternion.identity);

                        // 玩家被炸飞
                        Vector3 _Direction = cam.transform.position - _raycastNow;  //炸飞方向
                        float _value = _Direction.magnitude / 3;  //距离中心点程度[0,1]

                        //计算炸飞距离
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

            //如果打到 && 距离大于2f && 且不是脚底下
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

                //print($"绝对坐标为：{RayCast_last()}");
                //print($"相对坐标为：{world.GetRelalocation(RayCast())}");
                //print($"方块类型为：{world.GetBlockType(RayCast())}");

                
                
            }

        


        }

        //选择方块
        SelectBlock();


        //滚轮选择
        // 如果往下滚动
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

        // 如果往上滚动
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

    // 等待2秒后执行销毁泥土的方法
    IEnumerator DestroySoilWithDelay(Vector3 position)
    {
        //print("开启了破坏协程");
        isDestroying = true;
        if (point_Block_type != 255)
        {
            Broking_Animation.textureSheetAnimation.SetSprite(0, world.blocktypes[point_Block_type].buttom_sprit);
        }
        
        Broking_Animation.Play();

        // 记录协程开始执行时的时间
        float startTime = Time.time;
        float destroy_time = world.blocktypes[world.GetBlockType(position)].DestroyTime;

        //是否开启快速挖掘
        if (isSuperMining)
        {

            destroy_time = 0.25f;

        }

        // 等待
        while (Time.time - startTime < destroy_time)
        {

            // 计算材质插值
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / destroy_time);

            // 更新材质
            if (t != 1 && point_Block_type != VoxelData.BedRock && !isSuperMining)
            {

                HighLightMaterial.color = new Color(0, 0, 0, 1);

                //if (Mathf.FloorToInt(t * 10) >= 10)
                //{
                //    print("DestroyTextures下标越界");
                //}


                int index = Mathf.FloorToInt(t * 10);
                if (index < DestroyTextures.Length)
                {
                    HighLightMaterial.mainTexture = DestroyTextures[index];
                }
                else
                {
                    // 处理越界情况，例如使用默认材质或跳过该操作
                    print("DestroyTextures下标越界");
                    HighLightMaterial.mainTexture = DestroyTextures[DestroyTextures.Length - 1];
                }


            }

            // 如果玩家在等待期间松开了左键 || 转移了目标，则取消销毁泥土的逻辑
            if (!Input.GetMouseButton(0) || isChangeBlock)
            {

                elapsedTime = 0.0f;
                isDestroying = false;
                isChangeBlock = false;
                musicmanager.isbroking = false;

                //材质还原
                HighLightMaterial.color = new Color(0, 0, 0, 0);
                HighLightMaterial.mainTexture = DestroyTextures[0];

                //音效暂停
                musicmanager.Audio_player_broke.Stop();

                Broking_Animation.Stop();

                yield break;

            }

            yield return null;

        }

        //如果成功过了两秒
        // 执行销毁泥土的逻辑
        isDestroying = false;
        musicmanager.PlaySound_Broken(point_Block_type);
        elapsedTime = 0.0f;
        musicmanager.isbroking = false;

        //材质还原
        HighLightMaterial.color = new Color(0, 0, 0, 0);
        HighLightMaterial.mainTexture = DestroyTextures[0];

        //只有生存模式才会掉掉落物
        if (world.game_mode == GameMode.Survival && world.blocktypes[point_Block_type].candropBlock)
        {

            backpackmanager.CreateDropBox(new Vector3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), Mathf.FloorToInt(position.z)), point_Block_type, false, backpackmanager.ColdTime_Absorb);

        }

        //放进背包由掉落物执行
        //canvasManager.Change_text_selectBlockname(point_Block_type);

        //破坏粒子效果
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


        //print($"绝对坐标为：{position}");
        //print($"相对坐标为：{world.GetRelalocation(position)}");
        //print($"方块类型为：{world.GetBlockType(position)}"); 

    }



    //获取玩家按1~9
    private void SelectBlock()
    {

        // 检测按键1到9
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

            //异常检测
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


    //实现操作
    private void AchieveInput()
    {

        //重力实现
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


        //下蹲实现
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


        //选择方块实现
        if (selectindex >= 0 && selectindex <= 9)
        {

            selectblock.GetComponent<RectTransform>().anchoredPosition = new Vector2(VoxelData.SelectLocation_x[selectindex], 0);

        }




        //视角和移动实现
        transform.Rotate(Vector3.up * mouseHorizontal);
        //cam.Rotate(Vector3.right * -mouseVertical);
        cam.localRotation = Quaternion.Euler(Camera_verticalInput, 0, 0);


        if (isFlying)
        {

            //上升
            if (Input.GetKey(KeyCode.Space) && checkUpSpeed(1) != 0)
            {

                velocity.y = flyVelocity;

            }

            //下降
            else if (Input.GetKey(KeyCode.LeftControl) && checkDownSpeed(1) != 0)
            {

                velocity.y = -flyVelocity;

            }



            //松开
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

    //ture是显示
    public void SwitchThridPersonMode()
    {
        // 切换显示HitBox的状态 
        Show_HitBox = !Show_HitBox;

        // 切换玩家身体显示状态（用于第三人称模式）
        MainBody.SetActive(!MainBody.activeSelf);

        // 切换第一人称和第三人称摄像机
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





    //----------------------------------碰撞检测---------------------------------------

    public Vector3 front_Center = new Vector3();
    public Vector3 back_Center = new Vector3();
    public Vector3 left_Center = new Vector3();
    public Vector3 right_Center = new Vector3();
    [HideInInspector]public int CollisionNumber = 4;

    //更新16个碰撞点
    void update_block()
    {
        Vector3 _selfPos = transform.position;

        // 上面的四个点
        if (FactFacing.y > 0)
        {
            up_左上 = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2));
            up_右上 = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2));
            up_右下 = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2));
            up_左下 = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2));
            CollisionNumber += 4;
        }

        // 下面的四个点
        down_左上 = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2));
        down_右上 = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2));
        down_右下 = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2));
        down_左下 = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2));
        CollisionNumber += 4;

        //front
        if (ActualMoveDirection.z > 0)
        {
            front_Center = new Vector3(_selfPos.x, _selfPos.y, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_左上 = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_右上 = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_左下 = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_右下 = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);
            CollisionNumber += 5;

        }



        //back
        if (ActualMoveDirection.z < 0)
        {
            back_Center = new Vector3(_selfPos.x, _selfPos.y, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_左上 = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_右上 = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_左下 = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_右下 = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);
            CollisionNumber += 5;

        }


        //left
        if (ActualMoveDirection.x < 0)
        {
            left_Center = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y, _selfPos.z);
            left_左上 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) + delta);
            left_右上 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) - delta);
            left_左下 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) + delta);
            left_右下 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) - delta);
            CollisionNumber += 5;


        }

        //right
        if (ActualMoveDirection.x > 0)
        {
            right_Center = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y, _selfPos.z);
            right_左上 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) - delta);
            right_右上 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) + delta);
            right_左下 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) - delta);
            right_右下 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) + delta);
            CollisionNumber += 5;

        }


    }



    //碰撞检测（脚下）
    private float checkDownSpeed(float downSpeed)
    {

        if (
            world.CheckForVoxel(down_左上) ||
            world.CheckForVoxel(down_右上) ||
            world.CheckForVoxel(down_左下) ||
            world.CheckForVoxel(down_右下)

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


    //碰撞检测（头上）
    private float checkUpSpeed(float upSpeed)
    {

        if (
            world.CheckForVoxel(up_左上) ||
            world.CheckForVoxel(up_右上) ||
            world.CheckForVoxel(up_左下) ||
            world.CheckForVoxel(up_右下)
           )
        {

            return 0;

        }
        else
        {

            return upSpeed;

        }



    }


    //玩家输入
    public Vector2 keyInput
    {
        get
        {
            return new Vector2(horizontalInput,verticalInput);
        }
    }


    //返回玩家object实际运动方向
    public Vector3 ActualMoveDirection
    {
        get
        {
            Vector3 direction = Vector3.zero;

            // 获得keyInput的输入方向
            Vector2 input = keyInput;

            // 玩家面朝的方向
            Vector3 facing = FactFacing;

            // 根据Facing方向来计算玩家实际运动方向
            if (input.y > 0) // 向前运动
            {
                direction += new Vector3(facing.x, 0, facing.z);
            }
            else if (input.y < 0) // 向后运动
            {
                direction -= new Vector3(facing.x, 0, facing.z);
            }

            if (input.x < 0) // 向左运动
            {
                // 左方向是面对方向的左侧 (-z, x)
                direction += new Vector3(-facing.z, 0, facing.x);
            }
            else if (input.x > 0) // 向右运动
            {
                // 右方向是面对方向的右侧 (z, -x)
                direction += new Vector3(facing.z, 0, -facing.x);
            }

            // 将 momentum 添加到实际运动方向
            direction += momentum;

            // 归一化方向向量，以确保运动方向为单位向量
            return direction.normalized;
        }
    }

    //返回四舍五入的面朝向量
    public Vector3 Facing
    {
        get
        {
            float _y = isGrounded ? -1 : 1;
            return new Vector3(Mathf.RoundToInt(transform.forward.x), _y, Mathf.RoundToInt(transform.forward.z));

        }
    }

    //返回实际面朝向量
    public Vector3 FactFacing
    {
        get
        {
            float _y = isGrounded ? -1 : 1;
            return new Vector3(transform.forward.x, _y, transform.forward.z);

        }
    }


    //返回0 1 4 5四个方向
    public int IntForFacing
    {
        get
        {
            // 获取玩家的前方向量
            Vector3 forward = transform.forward;

            // 获取XZ平面的方向并四舍五入到最接近的整数
            float x = Mathf.Round(forward.x);
            float z = Mathf.Round(forward.z);

            // 判断方向并返回对应的整数值
            if (z > 0 && Mathf.Abs(forward.x) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 0; // 前 (Z+)
            }
            else if (z < 0 && Mathf.Abs(forward.x) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 1; // 后 (Z-)
            }
            else if (x > 0 && Mathf.Abs(forward.z) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 5; // 右 (X+)
            }
            else if (x < 0 && Mathf.Abs(forward.z) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 4; // 左 (X-)
            }

            // 默认返回值，表示无法确定方向（通常情况下不会发生）
            return -1;
        }
    }

    public int RealBacking
    {
        get
        {
            // 获取玩家的前方向量
            Vector3 forward = transform.forward;

            // 获取XZ平面的方向并四舍五入到最接近的整数
            float x = Mathf.Round(forward.x);
            float z = Mathf.Round(forward.z);

            // 判断后背方向并返回对应的整数值
            if (z > 0 && Mathf.Abs(forward.x) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 1; // 背对 Z- 方向（后）
            }
            else if (z < 0 && Mathf.Abs(forward.x) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 0; // 背对 Z+ 方向（前）
            }
            else if (x > 0 && Mathf.Abs(forward.z) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 4; // 背对 X- 方向（左）
            }
            else if (x < 0 && Mathf.Abs(forward.z) <= Mathf.Sin(45 * Mathf.Deg2Rad))
            {
                return 5; // 背对 X+ 方向（右）
            }

            // 默认返回值，表示无法确定方向（通常情况下不会发生）
            return -1;
        }
    }



    //碰撞方向的检测（-Z方向为front）
    //方块的角度
    public bool front
    {
       
        get
        {
            //如果world返回true，则碰撞
            if (world.CheckForVoxel(front_左上) || 
                world.CheckForVoxel(front_右上) || 
                world.CheckForVoxel(front_左下) || 
                world.CheckForVoxel(front_右下) ||
                world.CheckForVoxel(front_Center))
            {
                return true;
            }

            //如果下蹲
            else if (isSquating)
            {
                //(左下固体 && 左下延伸不是固体) || (右下固体 && 右下延伸不是固体)
                if ((world.CheckForVoxel(down_左下) && !world.CheckForVoxel(new Vector3(down_左下.x, down_左下.y, down_左下.z + extend_delta))) || (world.CheckForVoxel(down_右下) && !world.CheckForVoxel(new Vector3(down_右下.x, down_右下.y, down_右下.z + extend_delta))))
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
            //    world.CheckForVoxel(back_左上) ||
            //    world.CheckForVoxel(back_右上) ||
            //    world.CheckForVoxel(back_左下) ||
            //    world.CheckForVoxel(back_右下)
            //    )
            //        return true;
            //    else
            //        return false;
            //}
            //else
            //{
            //    //(左上固体 && 左上延伸不是固体) || (右上固体 && 右上延伸不是固体)
            //    if ((world.CheckForVoxel(down_左上) && !world.CheckForVoxel(new Vector3(down_左上.x, down_左上.y, down_左上.z - extend_delta))) || (world.CheckForVoxel(down_右上) && !world.CheckForVoxel(new Vector3(down_右上.x, down_右上.y, down_右上.z - extend_delta))))
            //    {

            //        return true;
            //    }
            //    else
            //        return false;
            //}

            if (
                world.CheckForVoxel(back_左上) ||
                world.CheckForVoxel(back_右上) ||
                world.CheckForVoxel(back_左下) ||
                world.CheckForVoxel(back_右下) ||
                world.CheckForVoxel(back_Center)
                )
                return true;

            else if (isSquating)
            {

                //(右上固体 && 右上延伸不是固体) || (右下固体 && 右下延伸不是固体)
                if ((world.CheckForVoxel(down_左上) && !world.CheckForVoxel(new Vector3(down_左上.x, down_左上.y, down_左上.z - extend_delta))) || (world.CheckForVoxel(down_右上) && !world.CheckForVoxel(new Vector3(down_右上.x, down_右上.y, down_右上.z - extend_delta))))
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
            //    world.CheckForVoxel(left_左上) ||
            //    world.CheckForVoxel(left_右上) ||
            //    world.CheckForVoxel(left_左下) ||
            //    world.CheckForVoxel(left_右下)
            //    )
            //        return true;
            //    else
            //        return false;
            //}
            //else
            //{
                
            //    //(右上固体 && 右上延伸不是固体) || (右下固体 && 右下延伸不是固体)
            //    if ((world.CheckForVoxel(down_右上) && !world.CheckForVoxel(new Vector3(down_右上.x - extend_delta, down_右上.y, down_右上.z))) || (world.CheckForVoxel(down_右下) && !world.CheckForVoxel(new Vector3(down_右下.x - extend_delta, down_右下.y, down_右下.z))))
            //    {

            //        return true;
            //    }
            //    else
            //        return false;



            //}


            if (
                world.CheckForVoxel(left_左上) ||
                world.CheckForVoxel(left_右上) ||
                world.CheckForVoxel(left_左下) ||
                world.CheckForVoxel(left_右下) ||
                world.CheckForVoxel(left_Center)
                )
                return true;

            else if (isSquating)
            {

                //(右上固体 && 右上延伸不是固体) || (右下固体 && 右下延伸不是固体)
                if ((world.CheckForVoxel(down_右上) && !world.CheckForVoxel(new Vector3(down_右上.x - extend_delta, down_右上.y, down_右上.z))) || (world.CheckForVoxel(down_右下) && !world.CheckForVoxel(new Vector3(down_右下.x - extend_delta, down_右下.y, down_右下.z))))
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
                world.CheckForVoxel(right_左上) ||
                world.CheckForVoxel(right_右上) ||
                world.CheckForVoxel(right_左下) ||
                world.CheckForVoxel(right_右下) ||
                world.CheckForVoxel(right_Center)
                )
                return true;

            else if (isSquating)
            {

                //(左上固体 && 左上延伸不是固体) || (左下固体 && 左下延伸不是固体)
                if ((world.CheckForVoxel(down_左上) && !world.CheckForVoxel(new Vector3(down_左上.x + extend_delta, down_左上.y, down_左上.z))) || (world.CheckForVoxel(down_左下) && !world.CheckForVoxel(new Vector3(down_左下.x + extend_delta, down_左下.y, down_左下.z))))
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
            //如果world返回true，则碰撞
            if (world.CheckForVoxel(front_左下) ||
                world.CheckForVoxel(front_右下) ||
                world.CheckForVoxel(back_左下)  ||
                world.CheckForVoxel(back_右下)  ||
                world.CheckForVoxel(left_右下)  ||
                world.CheckForVoxel(left_右下)  ||
                world.CheckForVoxel(right_右下) ||
                world.CheckForVoxel(right_右下)
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
            //如果world返回true，则碰撞
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

    //更新脚下方块类型
    void Update_FootBlockType()
    {

        foot_BlockType_temp = world.GetBlockType(foot.position);

        //如果发生变动
        if (foot_BlockType_temp != foot_BlockType)
        {


            //update
            foot_BlockType = foot_BlockType_temp;

        }

    }



    //-------------------------------------------------------------------------------------





    //--------------------------------- 射线检测 ------------------------------------------



    //射线检测——返回打中的方块的相对坐标
    //没打中就是(0,0,0)
    Vector3 RayCast_now()
    {

        float step = checkIncrement;
        //Vector3 lastPos = new Vector3();

        while (step < reach)
        {

            Vector3 pos = cam.position + (cam.forward * step);

            //异常检测
            if (pos.y < 0)
            {
                pos = new Vector3(pos.x, 0, pos.z);
            }

            // 绘制射线以便调试
            //if (debug_ray)
            //{
            //    Debug.DrawRay(cam.position, cam.forward * step, Color.red, 100f);
            //}

            //(是竹子 || (是固体 && 不是基岩 && 不是水)则返回
            //if (world.GetBlockType(pos) == VoxelData.Bamboo || (world.GetBlockType(pos) != VoxelData.Air && world.GetBlockType(pos) != VoxelData.BedRock && world.GetBlockType(pos) != VoxelData.Water))
            if (world.blocktypes[world.GetBlockType(pos)].canBeChoose)
            {
                

                //print($"now射线检测：{(pos-cam.position).magnitude}");
                ray_length = (pos - cam.position).magnitude;
                return pos;

            }

            //lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        point_Block_type = 255;
        return Vector3.zero;

    }


    //射线检测——返回打中的方块的前一帧
    Vector3 RayCast_last()
    {

        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {

            Vector3 pos = cam.position + (cam.forward * step);

            // 绘制射线以便调试
            //if (debug_ray)
            //{
            //    Debug.DrawRay(cam.position, cam.forward * step, Color.red, 100f);
            //}

            //检测
            if (world.GetBlockType(pos) != VoxelData.Air && world.GetBlockType(pos) != VoxelData.Water)
            {

                //print($"last射线检测：{(lastPos - cam.position).magnitude}");
                ray_length = (lastPos - cam.position).magnitude;
                return lastPos;

            }

            //lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            lastPos = pos;

            step += checkIncrement;

        }

        return new Vector3(0f, 0f, 0f);

    }


    // 射线检测——返回从起始点到打中前一帧点的距离
    float RayCast_last(Vector3 originPos, Vector3 direction, float distance)
    {
        float step = checkIncrement;
        Vector3 lastPos = originPos;

        while (step < distance)
        {
            Vector3 pos = originPos + (direction.normalized * step);

            // 绘制射线以便调试，使用绿色
            //Debug.DrawLine(lastPos, pos, Color.red);

            // 检测
            if (world.GetBlockType(pos) != VoxelData.Air && world.GetBlockType(pos) != VoxelData.Water)
            {
                // 返回从起始点到打中前一帧点的距离
                //print((lastPos - originPos).magnitude);
                return (lastPos - originPos).magnitude;
                
            }

            // 保存当前帧的位置作为最后的有效位置
            lastPos = pos;

            step += checkIncrement;
        }

        // 如果没有检测到任何有效的块，返回零
        return distance;
    }


    //-------------------------------------------------------------------------------------





    //---------------------------------- debug ---------------------------------------------




    //绘制碰撞
    void Draw_CollisionBox()
    {


        // 上面的四条线
        Debug.DrawLine(up_左上, up_右上, Color.red); // 左上 -- 右上
        Debug.DrawLine(up_右上, up_右下, Color.red); // 右上 -- 右下
        Debug.DrawLine(up_右下, up_左下, Color.red); // 右下 -- 左下
        Debug.DrawLine(up_左下, up_左上, Color.red); // 左下 -- 左上

        // 下面的四条线
        Debug.DrawLine(down_左上, down_右上, Color.red); // 左上 -- 右上
        Debug.DrawLine(down_右上, down_右下, Color.red); // 右上 -- 右下
        Debug.DrawLine(down_右下, down_左下, Color.red); // 右下 -- 左下
        Debug.DrawLine(down_左下, down_左上, Color.red); // 左下 -- 左上


        //上半圈
        Debug.DrawLine(front_左上, front_右上, Color.yellow);
        Debug.DrawLine(front_右上, right_左上, Color.red);
        Debug.DrawLine(right_左上, right_右上, Color.yellow);
        Debug.DrawLine(right_右上, back_右上, Color.red);
        Debug.DrawLine(back_右上, back_左上, Color.yellow);
        Debug.DrawLine(back_左上, left_左上, Color.red);
        Debug.DrawLine(left_左上, left_右上, Color.yellow);
        Debug.DrawLine(left_右上, front_左上, Color.red);


        //中心碰撞点
        Debug.DrawLine(front_Center, right_Center, Color.green);
        Debug.DrawLine(right_Center, back_Center, Color.green);
        Debug.DrawLine(back_Center, left_Center, Color.green);
        Debug.DrawLine(left_Center, front_Center, Color.green);


        //下半圈
        Debug.DrawLine(front_左下, front_右下, Color.yellow);
        Debug.DrawLine(front_右下, right_左下, Color.red);
        Debug.DrawLine(right_左下, right_右下, Color.yellow);
        Debug.DrawLine(right_右下, back_右下, Color.red);
        Debug.DrawLine(back_右下, back_左下, Color.yellow);
        Debug.DrawLine(back_左下, left_左下, Color.red);
        Debug.DrawLine(left_左下, left_右下, Color.yellow);
        Debug.DrawLine(left_右下, front_左下, Color.red);


        //上腰线
        //up左上
        Debug.DrawLine(up_左上, front_左上, Color.red);
        Debug.DrawLine(up_左上, left_右上, Color.red);

        //up右上
        Debug.DrawLine(up_右上, front_右上, Color.red);
        Debug.DrawLine(up_右上, right_左上, Color.red);

        //up左下
        Debug.DrawLine(up_左下, left_左上, Color.red);
        Debug.DrawLine(up_左下, back_左上, Color.red);

        //up右下
        Debug.DrawLine(up_右下, back_右上, Color.red);
        Debug.DrawLine(up_右下, right_右上, Color.red);


        //中腰线
        //front
        Debug.DrawLine(front_左上, front_左下, Color.yellow);
        Debug.DrawLine(front_右上, front_右下, Color.yellow);
        //back
        Debug.DrawLine(back_左上, back_左下, Color.yellow);
        Debug.DrawLine(back_右上, back_右下, Color.yellow);
        //left
        Debug.DrawLine(left_左上, left_左下, Color.yellow);
        Debug.DrawLine(left_右上, left_右下, Color.yellow);
        //right
        Debug.DrawLine(right_左上, right_左下, Color.yellow);
        Debug.DrawLine(right_右上, right_右下, Color.yellow);

        //下腰线
        //down左上
        Debug.DrawLine(down_左上, front_左下, Color.red);
        Debug.DrawLine(down_左上, left_右下, Color.red);

        //down右上
        Debug.DrawLine(down_右上, front_右下, Color.red);
        Debug.DrawLine(down_右上, right_左下, Color.red);

        //down左下
        Debug.DrawLine(down_左下, left_左下, Color.red);
        Debug.DrawLine(down_左下, back_左下, Color.red);

        //down右下
        Debug.DrawLine(down_右下, back_右下, Color.red);
        Debug.DrawLine(down_右下, right_右下, Color.red);

    }


    void Draw_HitBox()
    {
        Vector3 _selfPos = transform.position;
        Vector3 _eyesPos = eyes.transform.position;

        Vector3 _front_左上 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _front_右上 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _front_左下 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _front_右下 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);

        Vector3 _back_左上 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _back_右上 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _back_左下 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _back_右下 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);

        Vector3 _eyes_左上 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _eyesPos.y, _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _eyes_右上 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _eyesPos.y, _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _eyes_左下 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _eyesPos.y, _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _eyes_右下 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _eyesPos.y, _selfPos.z - (playerWidth / 2) - extend_delta);

        //头顶正方形
        Debug.DrawLine(_front_左上, _front_右上, Color.white);
        Debug.DrawLine(_front_右上, _back_右上, Color.white); 
        Debug.DrawLine(_back_右上, _back_左上, Color.white);
        Debug.DrawLine(_back_左上, _front_左上, Color.white); 

        //脚底正方形
        Debug.DrawLine(_front_左下, _front_右下, Color.white);
        Debug.DrawLine(_front_右下, _back_右下, Color.white);
        Debug.DrawLine(_back_右下, _back_左下, Color.white);
        Debug.DrawLine(_back_左下, _front_左下, Color.white);

        //四个竖线
        Debug.DrawLine(_front_左上, _front_左下, Color.white);
        Debug.DrawLine(_front_右上, _front_右下, Color.white);
        Debug.DrawLine(_back_左上, _back_左下, Color.white);
        Debug.DrawLine(_back_右上, _back_右下, Color.white);

        //眼睛
        Debug.DrawLine(_eyes_左上, _eyes_右上, Color.red);
        Debug.DrawLine(_eyes_右上, _eyes_右下, Color.red);
        Debug.DrawLine(_eyes_右下, _eyes_左下, Color.red);
        Debug.DrawLine(_eyes_左下, _eyes_左上, Color.red);
    }


    //-------------------------------------------------------------------------------------






    //----------------------------------- 玩家状态 -------------------------------------------

    //玩家强制移动
    public void ForceMoving(Vector3 moveDirection, float moveDistance, float moveTime)
    {
        // 计算初始动量
        momentum = moveDirection.normalized * (moveDistance / moveTime);

        //降低瞬时重力
        verticalMomentum = 0f;

        //射线检测确定最短movetime
        //Vector3 _selfPos = transform.position;
        //float _MinDistnce = 3f;
        ////左前
        //if (moveDirection.z > 0 && moveDirection.x < 0)
        //{
        //    Vector3 _front_左上 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        //    Vector3 _front_左下 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        //    float _MinDistnce_1 = RayCast_last(_front_左上, moveDirection, 3);
        //    float _MinDistnce_2 = RayCast_last(_front_左下, moveDirection, 3);

        //    if (_MinDistnce_1 < _MinDistnce)
        //    {
        //        _MinDistnce = _MinDistnce_1;
        //    }

        //    if (_MinDistnce_2 < _MinDistnce)
        //    {
        //        _MinDistnce = _MinDistnce_2;
        //    }

        //}

        ////右前
        //if (moveDirection.z > 0 && moveDirection.x > 0)
        //{
        //    Vector3 _front_右上 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        //    Vector3 _front_右下 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        //    float _MinDistnce_1 = RayCast_last(_front_右上, moveDirection, 3);
        //    float _MinDistnce_2 = RayCast_last(_front_右下, moveDirection, 3);

        //    if (_MinDistnce_1 < _MinDistnce)
        //    {
        //        _MinDistnce = _MinDistnce_1;
        //    }

        //    if (_MinDistnce_2 < _MinDistnce)
        //    {
        //        _MinDistnce = _MinDistnce_2;
        //    }
        //}

        ////左后
        //if (moveDirection.z < 0 && moveDirection.x < 0)
        //{
        //    Vector3 _back_左上 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        //    Vector3 _back_左下 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        //    float _MinDistnce_1 = RayCast_last(_back_左上, moveDirection, 3);
        //    float _MinDistnce_2 = RayCast_last(_back_左下, moveDirection, 3);

        //    if (_MinDistnce_1 < _MinDistnce)
        //    {
        //        _MinDistnce = _MinDistnce_1;
        //    }

        //    if (_MinDistnce_2 < _MinDistnce)
        //    {
        //        _MinDistnce = _MinDistnce_2;
        //    }
        //}

        ////右后
        //if (moveDirection.z < 0 && moveDirection.x > 0)
        //{
        //    Vector3 _back_右上 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        //    Vector3 _back_右下 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        //    float _MinDistnce_1 = RayCast_last(_back_右上, moveDirection, 3);
        //    float _MinDistnce_2 = RayCast_last(_back_右下, moveDirection, 3);

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

        ////防止除以0
        //float _v = Mathf.Lerp(3,0, (momentum.magnitude - 20f) / 20f);

        //if (momentum.magnitude > 0)
        //{
        //    moveTime = _MinDistnce / _v;
        //}
        //else
        //{
        //    moveTime = 0.01f; // 设置一个最小移动时间，以防止 NaN 传递
        //}

        //print(moveTime);

        // 启动一个协程，在移动时间结束后逐渐停止动量
        StartCoroutine(StopForceMovingAfterTime(moveTime));
    }

    // 协程：在指定的时间后逐渐停止动量
    private IEnumerator StopForceMovingAfterTime(float moveTime)
    {
        // 如果 moveTime 非常小，立即停止动量
        //if (moveTime <= 0.01f)
        //{
        //    momentum = Vector3.zero;
        //    yield break;
        //}
        //print(moveTime);

        // 等待指定的移动时间
        yield return new WaitForSeconds(moveTime);

        // 在一段时间内逐渐减少动量
        float elapsed = 0f;
        float decayDuration = 0.5f; // 动量逐渐消失的时间

        while (elapsed < decayDuration)
        {
            momentum = Vector3.Lerp(momentum, Vector3.zero, elapsed / decayDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 确保动量归零
        momentum = Vector3.zero;
    }



    //记录玩家状态
    void GetPlayerState()
    {

        //是否游泳
        //当前方块 || y+1方块是不是水
        if (world.GetBlockType(foot.transform.position) == VoxelData.Water || world.GetBlockType(new Vector3(foot.transform.position.x, foot.transform.position.y + 1f, foot.transform.position.z)) == VoxelData.Water)
        {

            isSwiming = true;

        }
        else
        {

            isSwiming = false;

        }


        //记录玩家摔落
        //是否是太空模式
        if (!isSpaceMode)
        {

            //玩家如果落水则new_foot_high重置
            if (foot.transform.position.y > new_foot_high || isSwiming)
            {

                new_foot_high = transform.position.y;

            }



            //判断玩家是否受伤
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


    //执行受伤协程
    IEnumerator HandleHurt()
    {

        lifemanager.UpdatePlayerBlood((int)(new_foot_high - foot.transform.position.y), true, true);

        yield return new WaitForSeconds(hurtCooldownTime); // 等待受伤冷却时间结束

        falldownCoroutine = null;

    }


    //受伤歪头动画
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

        transform.localRotation = Quaternion.Euler(startRotation); // 保证动画结束时物体回到初始角度

    }





    //-------------------------------------------------------------------------------------







    //---------------------------------- 工具类 ---------------------------------------------

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


    //检查给定的坐标是否和foot的四个点有相同，只要有一个相同则返回true
    bool CanPutBlock(Vector3 pos)
    {
        
        //如果等于eyes的坐标提前返回true
        if (world.GetRelalocation(new Vector3(pos.x, pos.y + 1f, pos.z)) == world.GetRelalocation(cam.position))
        {
        
            return true;
        
        }


        if (world.GetRelalocation(pos) == world.GetRelalocation(down_左上))
        {

            return true;

        }


        else if (world.GetRelalocation(pos) == world.GetRelalocation(down_右上))
        {

            return true;

        }
        else if (world.GetRelalocation(pos) == world.GetRelalocation(down_左下))
        {

            return true;

        }
        else if (world.GetRelalocation(pos) == world.GetRelalocation(down_右下))
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
