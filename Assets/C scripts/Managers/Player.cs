using Homebrew;
using MCEntity;
using System.Collections;
using UnityEngine;
using static MC_Static_Math;
using static MC_Static_BlocksFunction;

public class Player : MonoBehaviour
{

    //玩家状态
    [Foldout("玩家状态", true)]    
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
    [ReadOnly] public bool isFirstBrokeBlock = true;  //创造模式点击可以瞬间销毁方块
    [ReadOnly] public bool isInputing;  //输入停止
    [ReadOnly] public bool NotCheckPlayerCollision;
    [ReadOnly] public bool isHitEntity = false; //打到实体了
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


    [Header("角色参数")]
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
    public Vector3 velocity;
    //public float Max_verticalMomentum;
    public float verticalMomentum = 0;
    public Vector3 momentum = Vector3.zero; // 玩家瞬时动量
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


    //摔落伤害
    [Header("摔落参数")]
    private float groundTime; public float minGroundedTime = 0.1f; // 最小的落地时间
    Coroutine falldownCoroutine;
    public float hurtCooldownTime = 0.5f;  //摔落冷却时间
    public float new_foot_high = -100f;
    [Header("受伤歪头最大角度")] public float angle = 50f;
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
    public float flyVelocity_Verticle = 0.1f; //上下飞行速度
    public float flyVelocity_Horizon_mult = 1.5f; //水平飞行速度
    public float flyLerpTime = 0.3f;
    public bool jump_press = false;


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
    [Header("玩家脚下坐标")] public byte foot_BlockType = VoxelData.Air; private byte foot_BlockType_temp = VoxelData.Air;


    //用来检查isInCave
    [Header("洞穴状态检查间隔时间")] public float isInCave_checkInterval = 1f; private float isInCave_nextCheckTime = 1f;// 每0.5秒检查一次

    //TNT实体
    public GameObject Entity_TNT;

    //--------------------------------- 周期函数 --------------------------------------

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
        


        //初始化数据
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

            //计算饱食度
            DynamicFood();

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


            
            //非旁观者模式
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

            if (!NotCheckPlayerCollision)
            {
                update_block();
            }

            //游戏中暂停，暂停玩家输入
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


            //计算输入数据
            CalculateVelocity();

            //实现操作
            AchieveInput();


            //AdjustPlayerToGround();

            
            //DynamicState_isCave();

        }


        
    }




    //减小饱食度
    [Header("饱食度参数")]
    private bool hasExec_FixedUpdate = true;
    private Vector3 footPos;
    private Vector3 previous_footPos;
    public float walkingDistance;
    //public float accumulatedDistance; //累计走的路程
    public void DynamicFood()
    {
        if (Service_World.game_mode == GameMode.Survival)
        {
            


            //计算
            if (isSprinting)
            {
                //初始化
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
            

            //饱食度衰减
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
        // 每隔一段时间检查一次
        if (Time.time >= isInCave_nextCheckTime)
        {
            //print($"{Time.time}");
            CheckisInCave();
            isInCave_nextCheckTime = Time.time + isInCave_checkInterval; // 设置下一次检查的时间
        }
    }

    //更新isInCave状态
    public void CheckisInCave()
    {
        //print("");
        float playerY = cam.position.y + 10f;
        Vector3 RelaPosition = GetRelaPos(cam.position);
        Vector3 _ChunkLocation = GetRelaChunkLocation(cam.position);
        //print($"RelaPosition: {RelaPosition} , ChunkLocation = {_ChunkLocation}");
        float NoiseY = MC_Static_Noise.GetTotalNoiseHigh_Biome((int)RelaPosition.x, (int)RelaPosition.z, new Vector3((int)_ChunkLocation.x * 16f, 0f, (int)_ChunkLocation.z * 16f), managerhub.Service_Saving.worldSetting.worldtype, Service_World._biomeProperties);

        //print($"PlayerY：{playerY} , NoiseY：{NoiseY} , 差: {NoiseY - playerY}");


        // 检查眼睛所在位置是否处于地表以下，将Fog改为近距离黑色迷雾
        if (playerY < NoiseY || eyes.transform.position.y < 0)
        {
            if (!isInCave)
            {
                //print("迷雾开启");
                // 开始迷雾过渡到洞穴状态
                //managerhub.timeManager.Buff_CaveFog(true);
                isInCave = true; 
            }
        }
        else
        {
            if (isInCave)
            {
                //print("迷雾关闭");
                // 开始迷雾过渡到白天状态
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






    //--------------------------------- 玩家操作 --------------------------------------

    //调整玩家坐标防止脚底穿模

    //private void AdjustPlayerToGround()
    //{
    //    if (isGrounded && hasExec_AdjustPlayerToGround)
    //    {
    //        print("调整一次坐标");
    //        Vector3 myposition = transform.position;
    //        Vector3 Vec = GetRelaPos(foot.position);
    //        transform.position = new Vector3(myposition.x, Vec.y + 1.95f, myposition.z);
    //        hasExec_AdjustPlayerToGround = false;
    //    }
    //}


    //数据计算
    private void CalculateVelocity()
    {
        //飞行中落地
        if (isGrounded && isFlying && !NotCheckPlayerCollision)
        {
            isFlying = false;
            StartCoroutine(expandchangeview(false));
        }

        //玩家视角处理
        Camera_verticalInput -= mouseVerticalspeed;
        Camera_verticalInput = Mathf.Clamp(Camera_verticalInput, -90f, 90f);

        // 计算重力

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
        if (!NotCheckPlayerCollision)
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


    // 惯性系数，越大惯性越明显
    [Header("输入惯性")]
    public float inputInertia = 0.1f;
    public float Flying_inputInertia = 0.3f;

    // 用于存储上一次的输入值
    private float horizontalInputSmooth;
    private float verticalInputSmooth;


    //接收操作
    [HideInInspector] public bool hasExec_isChangedBlock = true;
    public Vector3 test_Normal;
    private void GetPlayerInputs()
    {

        //提前返回-如果玩家死亡，停止键盘输入
        if (isDead)
            return;

        // 获取当前的输入值
        float currentHorizontalInput = Input.GetAxis("Horizontal");
        float currentVerticalInput = Input.GetAxis("Vertical");

        // 检查是否有输入
        if (currentHorizontalInput != 0 || currentVerticalInput != 0)
        {
            isInputing = true; // 有输入
        }
        else
        {
            isInputing = false; // 没有输入
        }


        //飞行速度增加
        if (isFlying)
        {
            //flyVelocity_Horizon_mult
            currentHorizontalInput *= flyVelocity_Horizon_mult;
            currentVerticalInput *= flyVelocity_Horizon_mult;

        }


        // 使用 Mathf.Lerp 进行输入惯性平滑处理
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

        // 增加阈值判断，确保微小的输入直接归零
        if (Mathf.Abs(horizontalInputSmooth) < 0.01f)
        {
            horizontalInputSmooth = 0f;
        }

        if (Mathf.Abs(verticalInputSmooth) < 0.01f)
        {
            verticalInputSmooth = 0f;
        }

        // 这里使用平滑处理后的输入值
        horizontalInput = horizontalInputSmooth;
        verticalInput = verticalInputSmooth;

        // 获取鼠标输入（无惯性）
        mouseHorizontal = Input.GetAxis("Mouse X") * managerhub.canvasManager.Mouse_Sensitivity;
        mouseVerticalspeed = Input.GetAxis("Mouse Y") * managerhub.canvasManager.Mouse_Sensitivity;

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

        //打开碰撞盒
        if (Input.GetKeyDown(KeyCode.F6))
        {
            ShowEntityHitbox = !ShowEntityHitbox;
        }


        //旁观者模式
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
            

        if (Input.GetKeyDown(KeyCode.Q) && !managerhub.player.isSpectatorMode)
        {

            backpackmanager.ThrowDropBox();
        
        }

        //飞行模式
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
        




        // 如果在水中按下跳跃键 && leg低于水面，触发跳跃请求 
        if (isSwiming && Input.GetKey(KeyCode.Space) && (leg.position.y - 0.1f < Service_World._biomeProperties.terrainLayerProbabilitySystem.sea_level))
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


        //如果点击鼠标左键
        if (Input.GetMouseButtonDown(0) && !isSpectatorMode)
        {
            isFirstBrokeBlock = true;

            MC_RayCastStruct _rayCast = MC_Static_Raycast.RayCast(managerhub, MC_RayCast_FindType.AllFind,cam.position, cam.forward, reach, -1, checkIncrement);

            //print($"射线发射长度 = {_rayCast.rayDistance}");

            //攻击实体
            if (_rayCast.isHit == 2)
            {
                isHitEntity = true;
                //print("打到实体了");

                //实体击飞方向
                Vector3 direct = managerhub.player.transform.forward;
                direct.y = 1f;

                //播放玩家攻击音效
                managerhub.NewmusicManager.PlayOneShot(MusicData.Player_Attack);

                //实体扣血
                if (_rayCast.targetEntityInfo._obj.GetComponent<MC_Component_Life>() != null)
                    _rayCast.targetEntityInfo._obj.GetComponent<MC_Component_Life>().UpdateEntityLife(-1, direct);

                //播放实体被攻击音效
                //int _index = MusicData.Slime_Behurt;
                //if (_rayCast.targetEntityInfo._obj.GetComponent<MC_Music_Component>() != null)
                //    _index = _rayCast.targetEntityInfo._obj.GetComponent<MC_Music_Component>().BeHurtIndex;
                //if (_index != 0)
                //    managerhub.NewmusicManager.PlayOneShot(_index);

            }

            //print(OnLoadResource.Instance.Goods[1]);
        }

        //如果松开鼠标左键，isChanger还原
        if (Input.GetMouseButtonUp(0))
        {

            isChangeBlock = false;
            hasExec_isChangedBlock = true;
            managerhub.OldMusicManager.isbroking = false;
            managerhub.OldMusicManager.Audio_player_broke.Stop();
            isFirstBrokeBlock = true;
            isHitEntity = false;
        }

        //左键销毁泥土

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

            //如果打到
            if (_rayCast.hitPoint != Vector3.zero)
            {

                //如果正在销毁则不执行
                if (!isDestroying && !isHitEntity)
                {

                    //Debug.Log("执行销毁");
                    elapsedTime = 0.0f;
                    StartCoroutine(DestroySoilWithDelay(_rayCast));


                }


                //Service_World.GetChunkObject(RayCast_now()).EditData(GetRelaPos(RayCast_now()), 4);


                //print($"绝对坐标为：{RayCast_now()}");
                //print($"相对坐标为：{GetRelaPos(RayCast())}");
                //print($"方块类型为：{Service_World.GetBlockType(RayCast())}");
            }


        }
        
        //右键放置泥土
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

                //右键可互动方块
                if (_targettype < Service_World.blocktypes.Length && Service_World.blocktypes[_targettype].isinteractable && !isSquating)
                {
                    //print("isinteractable");

                    switch (_targettype)
                    {

                        //唱片机
                        case 40:
                            if (_selecttype == VoxelData.Tool_MusicDiscs)
                            {

                                //managerhub.NewmusicManager.SwitchBackgroundMusic(MusicData.MusicBox, 1f, 0.5f);
                                managerhub.NewmusicManager.Create3DSound(GetCenterVector3(_rayCast.hitPoint_Previous) , MusicData.MusicBox);
                                managerhub.backpackManager.update_slots(1, 50);
                            }
                            break;

                        //DFS烟雾 
                        case 42:
                            if (_selecttype == VoxelData.Tool_BoneMeal)
                            {
                                //canvasManager.UIManager[VoxelData.ui玩家].childs[1]._object.SetActive(!canvasManager.UIManager[VoxelData.ui玩家].childs[1]._object.activeSelf);
                                Service_World.Allchunks[GetRelaChunkLocation(_rayCast.hitPoint)].EditData(_rayCast.hitPoint, VoxelData.Air);
                                
                                Smoke(_rayCast.hitPoint);
                                managerhub.backpackManager.update_slots(1, 56);

                                // 玩家被炸飞
                                //Vector3 _Direction = cam.transform.position - _rayCast.hitPoint;  //炸飞方向
                                //float _value = _Direction.magnitude / 3;  //距离中心点程度[0,1]

                                ////计算炸飞距离
                                //_Direction.y = Mathf.Lerp(0, 1, _value);
                                //float Distance = Mathf.Lerp(3, 0, _value);

                                //ForceMoving(_Direction, Distance, 0.1f);
                            }
                            break;

                        //工作台
                        case 18:
                            managerhub.canvasManager.SwitchUI_Player(CanvasData.uiplayer_工作台);
                            break;

                        //熔炉
                        case 39:
                            managerhub.canvasManager.SwitchUI_Player(CanvasData.uiplayer_熔炉);
                            break;

                        //箱子
                        case 45:
                            managerhub.canvasManager.SwitchUI_Player(CanvasData.uiplayer_箱子);
                            break;

                            //树苗
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

                //如果是工具方块，则不进行方块的放置
                if (_selecttype < Service_World.blocktypes.Length && !Service_World.blocktypes[_selecttype].isTool)
                {
                    //如果打到 && 距离大于2f && 且不是脚底下
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

                        //print($"绝对坐标为：{RayCast_last()}");
                        //print($"相对坐标为：{GetRelaPos(RayCast())}");
                        //print($"方块类型为：{Service_World.GetBlockType(RayCast())}");



                    }

                }

                //执行工具方块的右键
                else
                {
                    switch (_selecttype)
                    {
                        //苹果
                        case 59:
                            managerhub.lifeManager.UpdatePlayerFood(-4, true);
                            managerhub.backpackManager.update_slots(1, VoxelData.Apple);
                            break;

                        //猪肉
                        case 49:
                            managerhub.lifeManager.UpdatePlayerFood(-8, true);
                            managerhub.backpackManager.update_slots(1, VoxelData.Tool_Pork);
                            break;

                        //书籍
                        case 58:
                            managerhub.canvasManager.SwitchUI_Player(CanvasData.uiplayer_书籍);
                            break;

                        //打火石
                        case 51:
                            if (_targettype == VoxelData.TNT)
                            {
                                //消除方块
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

            managerhub.canvasManager.Change_text_selectBlockname(255);
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

            managerhub.canvasManager.Change_text_selectBlockname(255);
            backpackmanager.ChangeBlockInHand();

        }



    }





    //public float DEbug_DIstance;

    // 等待2秒后执行销毁泥土的方法
    //public float Broking_Offset = 0.1f;
    //public Vector3 previous_Broking_Normal;
    public GameObject Particle_TNT_Prefeb;
    IEnumerator DestroySoilWithDelay(MC_RayCastStruct _HitSrtuct)
    {

        //print("开启了破坏协程");
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

        //破坏中粒子系统
        brokingBox.Particle_Play(point_Block_type);
        //Broking_Animation.Play();

        // 记录协程开始执行时的时间
        float startTime = Time.time;

        //获取挖掘时间
        float destroy_time = GetDestroyTime(_selecttype, theBlockwhichBeBrokenType);
        //print(destroy_time);

        // 等待
        while (Time.time - startTime < destroy_time)
        {

            //是否跳过等待
            //if (CheckToJumpBrokenTime(_selecttype, theBlockwhichBeBrokenType))
            //{
                
            //    break;
            //}





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
                managerhub.OldMusicManager.isbroking = false;

                //材质还原
                HighLightMaterial.color = new Color(0, 0, 0, 0);
                HighLightMaterial.mainTexture = DestroyTextures[0];

                //音效暂停
                managerhub.OldMusicManager.Audio_player_broke.Stop();

                brokingBox.gameObject.GetComponent<ParticleSystem>().Stop();
                //Broking_Animation.Stop();

                yield break;

            }

            yield return null;

        }



        //如果成功过了两秒
        // 执行销毁泥土的逻辑
        isDestroying = false;

        brokingBox.gameObject.GetComponent<ParticleSystem>().Stop();
        //musicmanager.PlaySound_Broken(theBlockwhichBeBrokenType);

        ////破坏粒子效果
        //GameObject particleInstance = Instantiate(Particle_Broken);
        //particleInstance.transform.parent = particel_Broken_transform;
        ////particleInstance.transform.position = _PosCenter;
        //particleInstance.GetComponent<ParticleCollision>().Particle_PLay(theBlockwhichBeBrokenType);

        elapsedTime = 0.0f;
        managerhub.OldMusicManager.isbroking = false;

        //材质还原
        HighLightMaterial.color = new Color(0, 0, 0, 0);
        HighLightMaterial.mainTexture = DestroyTextures[0];



        //World
        var chunkObject = managerhub.Service_World.GetChunkObject(_HitSrtuct.hitPoint);
        chunkObject.EditData(_HitSrtuct.hitPoint, VoxelData.Air);

    }

    //动态获得破坏时间
    float GetDestroyTime(byte _SelectType, byte _beBrokenType)
    {
        //提前返回, 如果是宝剑则永远不会破坏方块
        if (_SelectType != 255 && !Service_World.blocktypes[_SelectType].canBreakBlockWithMouse1)
            return Mathf.Infinity;

        //创造模式
        if (Service_World.game_mode == GameMode.Creative)
        {
            //首次破坏
            if (isFirstBrokeBlock)
            {
                isFirstBrokeBlock = false;
                return 0f;
            }

            return 0.25f;
        }

        //生存模式   
        if (Service_World.game_mode == GameMode.Survival)
        {

            //空手
            if (_SelectType == 255)
            {
                //基岩无限时间
                if (_beBrokenType == VoxelData.BedRock)
                    return Mathf.Infinity;
            }
            //手中有方块
            else
            {
                //基岩无限时间
                if (_beBrokenType == VoxelData.BedRock)
                    return Mathf.Infinity;
                //稿子 
                if (_SelectType == VoxelData.Tool_Pickaxe)
                    return 0.25f;
            }

        }


        return Service_World.blocktypes[_beBrokenType].DestroyTime;
    }







    //用来控制手臂停止动画的
    public GameObject HandsHold;
    public Transform HandInit;
    public float InitTime = 0.1f;
    Coroutine InitHandCoroutine;
       
    public void InitHandTransform()
    {
        if (InitHandCoroutine == null)
        {
            print("启动协程");
            InitHandCoroutine = StartCoroutine(_InitHandTransform());
        }
        
    }

    IEnumerator _InitHandTransform()
    {
        //给定时间内将_Hand过渡到HandInit
        // 记录初始状态
        Vector3 startPosition = HandsHold.transform.position;
        Quaternion startRotation = HandsHold.transform.rotation;

        // 目标状态
        Vector3 targetPosition = HandInit.position;
        Quaternion targetRotation = HandInit.rotation;

        // 时间变量
        float elapsedTime = 0f;

        while (elapsedTime < InitTime)
        {
            // 插值计算位置和旋转
            HandsHold.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / InitTime);
            HandsHold.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / InitTime);

            // 增加已过去的时间
            elapsedTime += Time.deltaTime;

            // 等待下一帧
            yield return null;
        }

        // 确保最后一次到达目标位置和旋转
        HandsHold.transform.position = targetPosition;
        HandsHold.transform.rotation = targetRotation;
        InitHandCoroutine = null;
    }



    //获取玩家按1~9
    private void SelectBlock()
    {

        // 检测按键1到9
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


    //放置高亮方块
    [ReadOnly]public Vector3 LastHighLightBlockPos;
    [ReadOnly]public Vector3 LastHighLightBlock_Normal;
    private void placeCursorBlocks()
    {
        //如果成功放置，则不需要再更新，直到玩家改变方块isChangeBlock == true或者Service_World.GetBlockType(pos)发生改变

        MC_RayCastStruct _rayCast = MC_Static_Raycast.RayCast(managerhub, MC_RayCast_FindType.AllFind, cam.position, cam.forward, reach, -1, checkIncrement);

        //如果可以放置
        if (_rayCast.isHit == 1)
        {
            int posX = Mathf.FloorToInt(_rayCast.hitPoint.x);
            int posY = Mathf.FloorToInt(_rayCast.hitPoint.y);
            int posZ = Mathf.FloorToInt(_rayCast.hitPoint.z);
            Vector3 NowHighLightBlockPos = new Vector3(posX, posY, posZ);
            point_Block_type = managerhub.Service_World.GetBlockType(_rayCast.hitPoint);


            //如果不一样则重新放置
            if (NowHighLightBlockPos != LastHighLightBlockPos)
            {
                //print("不一样");
                updateHightLightBlock(NowHighLightBlockPos);
                updateBrokingBox(_rayCast);
                LastHighLightBlockPos = NowHighLightBlockPos;
            }

            //如果不一样则重新放置
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
        //print("更新高亮方块");

        HighlightBlock.position = new Vector3(_v.x + 0.5f, _v.y + 0.5f, _v.z + 0.5f);
        HighlightBlock.localScale = new Vector3(1f, 1f, 1f);


        //动态改变HighLightBlock大小
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
    public float brokingbox额外多出来的部分 = 0.15f;
    void updateBrokingBox(MC_RayCastStruct _rayCast)
    {
        //brokingBox.Particle_PLay(point_Block_type);

        //动态改变破坏方向
        Vector3 _PosCenter = new Vector3((int)_rayCast.hitPoint.x + 0.5f, (int)_rayCast.hitPoint.y + 0.5f, (int)_rayCast.hitPoint.z + 0.5f);

        // 计算旋转，使用 hitNormal 作为物体的“前方向”对齐
        // 根据法线判断方向，并生成对应的旋转 vec
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

        // 获取 shape 模块并进行修改
        var shapeModule = particleSystem.shape;
        shapeModule.scale = _scale;  // 直接修改副本变量的属性

        // 计算旋转并应用到破坏粒子实例上
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

        brokingBox.gameObject.transform.position = _PosCenter + _rayCast.hitNormal * (_brokingOffset + brokingbox额外多出来的部分);
    }



    //实现操作
    bool hasExec_AdjustPlayerToGround = true;
    private void AchieveInput()
    {
        // 重力实现
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
            hasExec_AdjustPlayerToGround = true; // 在跳跃时重置调整标记
        }

        // 位置调整，防止穿模
        if (isGrounded && hasExec_AdjustPlayerToGround)
        {
            if (!Service_World.blocktypes[managerhub.Service_World.GetBlockType(foot.position)].isDIYCollision)
            {
                Vector3 myposition = transform.position;
                Vector3 Vec = GetRelaPos(foot.position); // 获取脚部应有的Y坐标
                if (Vec.y + 1.95f > myposition.y) // 只调整向上的情况，防止跳跃时干扰
                {

                    transform.position = new Vector3(myposition.x, Vec.y + 1.95f, myposition.z);
                    //print("调整一次坐标");
                }
            }

            hasExec_AdjustPlayerToGround = false; // 调整完毕，防止每帧执行
        }

        // 下蹲实现
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

        // 选择方块实现
        if (selectindex >= 0 && selectindex <= 9)
        {
            selectblock.GetComponent<RectTransform>().anchoredPosition = new Vector2(CanvasData.SelectLocation_x[selectindex], 0);
        }

        // 视角和移动实现
        transform.Rotate(Vector3.up * mouseHorizontal);
        cam.localRotation = Quaternion.Euler(Camera_verticalInput, 0, 0);

        if (isFlying)
        {
            // 上升
            if (Input.GetKey(KeyCode.Space))
            {
                velocity.y = flyVelocity_Verticle * Time.deltaTime;

                if (!NotCheckPlayerCollision)
                {
                    velocity.y = checkUpSpeed(velocity.y);
                }
                
            }
            // 下降
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                velocity.y = - flyVelocity_Verticle * Time.deltaTime;

                if (!NotCheckPlayerCollision)
                {
                    velocity.y = checkDownSpeed(velocity.y);
                }
            }
            // 松开按键，逐渐减速
            //else
            //{
            //    // 通过插值函数逐渐减小速度
            //    velocity.y = Mathf.Lerp(velocity.y, 0, Time.deltaTime * flyLerpTime);
            //}
            
        }

        transform.Translate(velocity, Space.World);
    }


    //飞行模式
    public void FlyingMode(bool _open)
    {
        if (isSprinting)
        {
            isSprinting = false;
        }

        //改变视野
        StartCoroutine(expandchangeview(_open));

        isFlying = _open;

        jump_press = false;
    }


    //旁观者模式
    public void SpectatorMode(bool _open)
    {
        if (_open)
        {
            //关闭碰撞
            NotCheckPlayerCollision = true;

            //开启飞行模式
            FlyingMode(true);

            //关闭UI
            managerhub.canvasManager.SpectatorMode(true);
        }
        else
        {
            //关闭碰撞
            NotCheckPlayerCollision = false;

            //关闭飞行模式
            FlyingMode(false);

            //打开UI
            managerhub.canvasManager.SpectatorMode(false);
        }


    }


    //初始化玩家坐标
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
            managerhub.Service_World.CollisionCheckForVoxel(down_左上) ||
            managerhub.Service_World.CollisionCheckForVoxel(down_右上) ||
            managerhub.Service_World.CollisionCheckForVoxel(down_左下) ||
            managerhub.Service_World.CollisionCheckForVoxel(down_右下)

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
            managerhub.Service_World.CollisionCheckForVoxel(up_左上) ||
            managerhub.Service_World.CollisionCheckForVoxel(up_右上) ||
            managerhub.Service_World.CollisionCheckForVoxel(up_左下) ||
            managerhub.Service_World.CollisionCheckForVoxel(up_右下)
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

            // 如果输入有非零值，则计算实际运动方向
            if (input != Vector2.zero)
            {
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
            }
            else
            {
                // 如果没有按下键盘，direction 立即归零
                direction = Vector3.zero;
            }

            // 将 momentum 添加到实际运动方向
            direction += momentum;

            // 归一化方向向量，以确保运动方向为单位向量
            if (direction != Vector3.zero)
            {
                direction = direction.normalized;
            }

            return direction;
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
            //如果Service_World返回true，则碰撞
            if (managerhub.Service_World.CollisionCheckForVoxel(front_左上) || 
                managerhub.Service_World.CollisionCheckForVoxel(front_右上) || 
                managerhub.Service_World.CollisionCheckForVoxel(front_左下) || 
                managerhub.Service_World.CollisionCheckForVoxel(front_右下) ||
                managerhub.Service_World.CollisionCheckForVoxel(front_Center))
            {
                return true;
            }

            //如果下蹲
            else if (isSquating)
            {
                //(左下固体 && 左下延伸不是固体) || (右下固体 && 右下延伸不是固体)
                if ((managerhub.Service_World.CollisionCheckForVoxel(down_左下) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_左下.x, down_左下.y, down_左下.z + extend_delta))) || (managerhub.Service_World.CollisionCheckForVoxel(down_右下) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_右下.x, down_右下.y, down_右下.z + extend_delta))))
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
            //    Service_World.CollisionCheckForVoxel(back_左上) ||
            //    Service_World.CollisionCheckForVoxel(back_右上) ||
            //    Service_World.CollisionCheckForVoxel(back_左下) ||
            //    Service_World.CollisionCheckForVoxel(back_右下)
            //    )
            //        return true;
            //    else
            //        return false;
            //}
            //else
            //{
            //    //(左上固体 && 左上延伸不是固体) || (右上固体 && 右上延伸不是固体)
            //    if ((Service_World.CollisionCheckForVoxel(down_左上) && !Service_World.CollisionCheckForVoxel(new Vector3(down_左上.x, down_左上.y, down_左上.z - extend_delta))) || (Service_World.CollisionCheckForVoxel(down_右上) && !Service_World.CollisionCheckForVoxel(new Vector3(down_右上.x, down_右上.y, down_右上.z - extend_delta))))
            //    {

            //        return true;
            //    }
            //    else
            //        return false;
            //}

            if (
                managerhub.Service_World.CollisionCheckForVoxel(back_左上) ||
                managerhub.Service_World.CollisionCheckForVoxel(back_右上) ||
                managerhub.Service_World.CollisionCheckForVoxel(back_左下) ||
                managerhub.Service_World.CollisionCheckForVoxel(back_右下) ||
                managerhub.Service_World.CollisionCheckForVoxel(back_Center)
                )
                return true;

            else if (isSquating)
            {

                //(右上固体 && 右上延伸不是固体) || (右下固体 && 右下延伸不是固体)
                if ((managerhub.Service_World.CollisionCheckForVoxel(down_左上) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_左上.x, down_左上.y, down_左上.z - extend_delta))) || (managerhub.Service_World.CollisionCheckForVoxel(down_右上) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_右上.x, down_右上.y, down_右上.z - extend_delta))))
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
            //    Service_World.CollisionCheckForVoxel(left_左上) ||
            //    Service_World.CollisionCheckForVoxel(left_右上) ||
            //    Service_World.CollisionCheckForVoxel(left_左下) ||
            //    Service_World.CollisionCheckForVoxel(left_右下)
            //    )
            //        return true;
            //    else
            //        return false;
            //}
            //else
            //{
                
            //    //(右上固体 && 右上延伸不是固体) || (右下固体 && 右下延伸不是固体)
            //    if ((Service_World.CollisionCheckForVoxel(down_右上) && !Service_World.CollisionCheckForVoxel(new Vector3(down_右上.x - extend_delta, down_右上.y, down_右上.z))) || (Service_World.CollisionCheckForVoxel(down_右下) && !Service_World.CollisionCheckForVoxel(new Vector3(down_右下.x - extend_delta, down_右下.y, down_右下.z))))
            //    {

            //        return true;
            //    }
            //    else
            //        return false;



            //}


            if (
                managerhub.Service_World.CollisionCheckForVoxel(left_左上) ||
                managerhub.Service_World.CollisionCheckForVoxel(left_右上) ||
                managerhub.Service_World.CollisionCheckForVoxel(left_左下) ||
                managerhub.Service_World.CollisionCheckForVoxel(left_右下) ||
                managerhub.Service_World.CollisionCheckForVoxel(left_Center)
                )
                return true;

            else if (isSquating)
            {

                //(右上固体 && 右上延伸不是固体) || (右下固体 && 右下延伸不是固体)
                if ((managerhub.Service_World.CollisionCheckForVoxel(down_右上) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_右上.x - extend_delta, down_右上.y, down_右上.z))) || (managerhub.Service_World.CollisionCheckForVoxel(down_右下) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_右下.x - extend_delta, down_右下.y, down_右下.z))))
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
                managerhub.Service_World.CollisionCheckForVoxel(right_左上) ||
                managerhub.Service_World.CollisionCheckForVoxel(right_右上) ||
                managerhub.Service_World.CollisionCheckForVoxel(right_左下) ||
                managerhub.Service_World.CollisionCheckForVoxel(right_右下) ||
                managerhub.Service_World.CollisionCheckForVoxel(right_Center)
                )
                return true;

            else if (isSquating)
            {

                //(左上固体 && 左上延伸不是固体) || (左下固体 && 左下延伸不是固体)
                if ((managerhub.Service_World.CollisionCheckForVoxel(down_左上) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_左上.x + extend_delta, down_左上.y, down_左上.z))) || (managerhub.Service_World.CollisionCheckForVoxel(down_左下) && !managerhub.Service_World.CollisionCheckForVoxel(new Vector3(down_左下.x + extend_delta, down_左下.y, down_左下.z))))
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
            //如果Service_World返回true，则碰撞
            if (managerhub.Service_World.CollisionCheckForVoxel(front_左下) ||
                managerhub.Service_World.CollisionCheckForVoxel(front_右下) ||
                managerhub.Service_World.CollisionCheckForVoxel(back_左下)  ||
                managerhub.Service_World.CollisionCheckForVoxel(back_右下)  ||
                managerhub.Service_World.CollisionCheckForVoxel(left_右下)  ||
                managerhub.Service_World.CollisionCheckForVoxel(left_右下)  ||
                managerhub.Service_World.CollisionCheckForVoxel(right_右下) ||
                managerhub.Service_World.CollisionCheckForVoxel(right_右下)
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
            //如果Service_World返回true，则碰撞
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

    //更新脚下方块类型
    void Update_FootBlockType()
    {

        foot_BlockType_temp = managerhub.Service_World.GetBlockType(foot.position);

        //如果发生变动
        if (foot_BlockType_temp != foot_BlockType)
        {


            //update
            foot_BlockType = foot_BlockType_temp;

        }

    }



    //-------------------------------------------------------------------------------------





    //--------------------------------- 射线检测 ------------------------------------------





    //射线检测
    [Foldout("射线检测", true)]
    [Header("射线长度")] public float reach = 5.2f;
    [Header("射线间隔")] public float checkIncrement = 0.1f;


    //获得居中方块
    public Vector3 GetCenterPoint(Vector3 _pos)
    {
        return new Vector3((int)_pos.x + 0.5f, (int)_pos.y + 0.5f, (int)_pos.z + 0.5f);
    }

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

    //bool hasExecHigbox = true;
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

        //if (hasExecHigbox)
        //{
        //    Vector3 center = (_back_左下 + _front_右上) / 2f;
        //    float width = (_front_右下 - _front_左下).magnitude;
        //    float height = (_front_右上 - _front_右下).magnitude;

        //     print($"中心坐标: {center}");
        //    print($"宽: {width}");
        //    print($"高: {height}");

        //    hasExecHigbox = false;
        //}


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


    //判定算法
    public Vector3 CheckHitBox(Vector3 _targetCenter, float _targetWidth, float _targetHeight)
    {
        // 玩家判定箱，假设已知
        Vector3 _selfCenter = transform.position;
        float _selfWidth = playerWidth; // 玩家宽度（底边正方形的边长）
        float _selfHeight = playerHeight; // 玩家高度

        // 计算玩家的边界
        float selfMinX = _selfCenter.x - _selfWidth / 2;
        float selfMaxX = _selfCenter.x + _selfWidth / 2;
        float selfMinY = _selfCenter.y - _selfHeight / 2;
        float selfMaxY = _selfCenter.y + _selfHeight / 2;
        float selfMinZ = _selfCenter.z - _selfWidth / 2; // 假设深度与宽度相同
        float selfMaxZ = _selfCenter.z + _selfWidth / 2;

        // 计算目标的边界
        float targetMinX = _targetCenter.x - _targetWidth / 2;
        float targetMaxX = _targetCenter.x + _targetWidth / 2;
        float targetMinY = _targetCenter.y - _targetHeight / 2;
        float targetMaxY = _targetCenter.y + _targetHeight / 2;
        float targetMinZ = _targetCenter.z - _targetWidth / 2; // 假设目标深度与宽度相同
        float targetMaxZ = _targetCenter.z + _targetWidth / 2;

        // 判定箱算法
        bool isCollision = selfMaxX >= targetMinX && selfMinX <= targetMaxX &&
                           selfMaxY >= targetMinY && selfMinY <= targetMaxY &&
                           selfMaxZ >= targetMinZ && selfMinZ <= targetMaxZ; // 增加 Z 轴的碰撞检测

        //print(isCollision);

        // 返回
        if (isCollision)
        {
            // 计算重叠方向
            Vector3 overlapDirection = (_selfCenter - _targetCenter).normalized;

            // 这里可以添加调试信息
            // Debug.Log($"发生碰撞, Length = {(_selfCenter - _targetCenter).magnitude}");

            return overlapDirection;
        }
        else
        {
            return Vector3.zero;
        }
    }

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


    public void CreateTNT(Vector3 _point, bool _acybytnt)
    {
        //创建TNT实体
        //GameObject tnt = GameObject.Instantiate(Entity_TNT);
        //tnt.GetComponent<Entity_TNT>().OnStartEntity(GetCenterPoint(_point), _acybytnt);

        managerhub.Service_Entity.AddEntity(EntityData.TNT, _point, out EntityInfo _result, true);
        _result._obj.GetComponent<Entity_TNT>().OnStartEntity(GetCenterPoint(_point), _acybytnt);
    }

    //记录玩家状态
    void GetPlayerState()
    {

        //是否游泳
        //当前方块 || y+1方块是不是水
        if (managerhub.Service_World.GetBlockType(foot.transform.position) == VoxelData.Water || managerhub.Service_World.GetBlockType(new Vector3(foot.transform.position.x, foot.transform.position.y + 1f, foot.transform.position.z)) == VoxelData.Water)
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
            if (foot.transform.position.y > new_foot_high || isSwiming || isSpectatorMode)
            {

                new_foot_high = transform.position.y;

            }



            //判断玩家是否受伤
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


    //检查给定的坐标是否和foot的四个点有相同，只要有一个相同则返回true
    bool CanPutBlock(Vector3 pos)
    {
        
        //如果等于eyes的坐标提前返回true
        if (GetRelaPos(new Vector3(pos.x, pos.y + 1f, pos.z)) == GetRelaPos(cam.position))
        {
        
            return true;
        
        }


        if (GetRelaPos(pos) == GetRelaPos(down_左上))
        {

            return true;

        }


        else if (GetRelaPos(pos) == GetRelaPos(down_右上))
        {

            return true;

        }
        else if (GetRelaPos(pos) == GetRelaPos(down_左下))
        {

            return true;

        }
        else if (GetRelaPos(pos) == GetRelaPos(down_右下))
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


