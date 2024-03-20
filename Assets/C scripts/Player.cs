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
    //玩家状态
    [Header("玩家状态")]
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

    [Header("角色参数")]
    public Transform foot;
    private bool hasExec = true;
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float squatWalkSpeed = 1f;
    public float squatSpeed = 3f;
    public float jumpForce = 6f;
    public float gravity = -15f;
    public float MaxHurtHigh = 7f;

    [Header("碰撞参数")]
    public float playerWidth = 0.3f;
    public float playerHeight = 1.7f;
    public float extend_delta = 0.1f;
    public float delta = 0.05f;

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
    private Vector3 OldPointLocation;

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

    //摔落伤害
    [Header("摔落参数")]
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
    public float minFOV = 70;
    public float maxFOV = 90;
    bool expandview = false;


    [Header("飞行模式")]
    private float lastJumpTime;
    private float doubleTapInterval = 0.5f; // Adjust as needed
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


    //--------------------------------- 周期函数 --------------------------------------

    void Start()
    {
        HighLightMaterial.color = new Color(0, 0, 0, 0);
        HighLightMaterial.mainTexture = DestroyTextures[0];
    }

    private void FixedUpdate()
    {
        if (world.game_state == Game_State.Playing)
        {
            //初始化人物位置
            if (hasExec)
            {
                InitPlayerLocation();
                hasExec = false;
            }
            
            //if (world.GetBlockType(foot.position) == VoxelData.Water)
            //{
            //    print("");
            //}

            //更新玩家脚下坐标
            Update_FootBlockType();

            //改变视距(如果奔跑的话)
            change_eyesview();

            //计算碰撞点
            update_block();

            //计算玩家状态
            GetPlayerState();

            //计算输入数据
            CalculateVelocity();

            //实现操作
            AchieveInput();

        }

    }


    //获取玩家输入
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
            if (verticalMomentum > gravity)
                verticalMomentum += Time.fixedDeltaTime * gravity;
        }
        else
        {
            if (verticalMomentum > watergravity)
                verticalMomentum += mult * Time.fixedDeltaTime * watergravity;
        }


        // 计算速度
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
            

        // 合并数据
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;



        //滑膜数据
        //前后
        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;

        //左右
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;


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
    void change_eyesview()
    {
        if (isSprinting && isMoving && !expandview)
        {
            //启动协程扩大视野
            StartCoroutine(expandchangeview(true));

            //update
            expandview = true;
        }
        else if((!isSprinting || !isMoving) && expandview)
        {
            //缩小视野
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

        eyes.fieldOfView = targetFOV; // 确保最终视野值准确
    }


    //接收操作
    private void GetPlayerInputs()
    {

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVerticalspeed = Input.GetAxis("Mouse Y");
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

        //飞行模式
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
        




        // 如果在水中按下跳跃键 && leg低于水面，触发跳跃请求 
        if (isSwiming && Input.GetKey(KeyCode.Space) && (leg.position.y - 0.1f < world.sea_level))
        {
            jumpRequest = true;
        }else if (isSwiming && Input.GetKey(KeyCode.Space) && (front || back || left || right))
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
        if (Input.GetMouseButtonDown(0))
        {
            OldPointLocation = new Vector3(Mathf.FloorToInt(RayCast_now().x), Mathf.FloorToInt(RayCast_now().y), Mathf.FloorToInt(RayCast_now().z));
        }

        //如果松开鼠标左键，isChanger还原
        if (Input.GetMouseButtonUp(0))
        {
            isChangeBlock = false;
            musicmanager.isbroking = false;
            musicmanager.Audio_player_broke.Stop();
        }

        //左键销毁泥土
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


            //如果打到 && 距离大于2f && 且不是脚底下
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

    // 等待2秒后执行销毁泥土的方法
    IEnumerator DestroySoilWithDelay(Vector3 position)
    {
        isDestroying = true;

        // 记录协程开始执行时的时间
        float startTime = Time.time;
        float destroy_time = world.blocktypes[world.GetBlockType(position)].DestroyTime;

        //是否开启快速挖掘
        if (isSuperMining)
        {
            destroy_time = 0.1f;
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
                HighLightMaterial.mainTexture = DestroyTextures[Mathf.FloorToInt(t * 10)];
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

        //只有生存模式才会掉掉落物
        if (world.game_mode == GameMode.Survival)
        {
            backpackmanager.CreateDropBox(new Vector3(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), Mathf.FloorToInt(position.z)), point_Block_type, false, backpackmanager.ColdTime_Absorb);

        }

        //放进背包由掉落物执行

        canvasManager.Change_text_selectBlockname(point_Block_type);

        

        world.GetChunkObject(position).EditData(world.GetRelalocation(position), VoxelData.Air);

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
                velocity.y = 0.1f;
            }

            //下降
            else if (Input.GetKey(KeyCode.LeftControl) && checkDownSpeed(1) != 0)
            {
                velocity.y = -0.1f;
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



    //--------------------------------------------------------------------------------





    //----------------------------------碰撞检测---------------------------------------

    //更新16个碰撞点
    void update_block()
    {

        // 上面的四个点
        up_左上 = new Vector3(transform.position.x - (playerWidth / 2), transform.position.y + (playerHeight / 2), transform.position.z + (playerWidth / 2));
        up_右上 = new Vector3(transform.position.x + (playerWidth / 2), transform.position.y + (playerHeight / 2), transform.position.z + (playerWidth / 2));
        up_右下 = new Vector3(transform.position.x + (playerWidth / 2), transform.position.y + (playerHeight / 2), transform.position.z - (playerWidth / 2));
        up_左下 = new Vector3(transform.position.x - (playerWidth / 2), transform.position.y + (playerHeight / 2), transform.position.z - (playerWidth / 2));

        // 下面的四个点
        down_左上 = new Vector3(transform.position.x - (playerWidth / 2), transform.position.y - (playerHeight / 2), transform.position.z + (playerWidth / 2));
        down_右上 = new Vector3(transform.position.x + (playerWidth / 2), transform.position.y - (playerHeight / 2), transform.position.z + (playerWidth / 2));
        down_右下 = new Vector3(transform.position.x + (playerWidth / 2), transform.position.y - (playerHeight / 2), transform.position.z - (playerWidth / 2));
        down_左下 = new Vector3(transform.position.x - (playerWidth / 2), transform.position.y - (playerHeight / 2), transform.position.z - (playerWidth / 2));


        //front
        front_左上 = new Vector3(transform.position.x - (playerWidth / 2) + delta, transform.position.y + (playerHeight / 4), transform.position.z + (playerWidth / 2) + extend_delta);
        front_右上 = new Vector3(transform.position.x + (playerWidth / 2) - delta, transform.position.y + (playerHeight / 4), transform.position.z + (playerWidth / 2) + extend_delta);
        front_左下 = new Vector3(transform.position.x - (playerWidth / 2) + delta, transform.position.y - (playerHeight / 4), transform.position.z + (playerWidth / 2) + extend_delta);
        front_右下 = new Vector3(transform.position.x + (playerWidth / 2) - delta, transform.position.y - (playerHeight / 4), transform.position.z + (playerWidth / 2) + extend_delta);


        //back
        back_左上 = new Vector3(transform.position.x - (playerWidth / 2) + delta, transform.position.y + (playerHeight / 4), transform.position.z - (playerWidth / 2) - extend_delta);
        back_右上 = new Vector3(transform.position.x + (playerWidth / 2) - delta, transform.position.y + (playerHeight / 4), transform.position.z - (playerWidth / 2) - extend_delta);
        back_左下 = new Vector3(transform.position.x - (playerWidth / 2) + delta, transform.position.y - (playerHeight / 4), transform.position.z - (playerWidth / 2) - extend_delta);
        back_右下 = new Vector3(transform.position.x + (playerWidth / 2) - delta, transform.position.y - (playerHeight / 4), transform.position.z - (playerWidth / 2) - extend_delta);


        //left
        left_左上 = new Vector3(transform.position.x - (playerWidth / 2) - extend_delta, transform.position.y + (playerHeight / 4), transform.position.z - (playerWidth / 2) + delta);
        left_右上 = new Vector3(transform.position.x - (playerWidth / 2) - extend_delta, transform.position.y + (playerHeight / 4), transform.position.z + (playerWidth / 2) - delta);
        left_左下 = new Vector3(transform.position.x - (playerWidth / 2) - extend_delta, transform.position.y - (playerHeight / 4), transform.position.z - (playerWidth / 2) + delta);
        left_右下 = new Vector3(transform.position.x - (playerWidth / 2) - extend_delta, transform.position.y - (playerHeight / 4), transform.position.z + (playerWidth / 2) - delta);


        //right
        right_左上 = new Vector3(transform.position.x + (playerWidth / 2) + extend_delta, transform.position.y + (playerHeight / 4), transform.position.z + (playerWidth / 2) - delta);
        right_右上 = new Vector3(transform.position.x + (playerWidth / 2) + extend_delta, transform.position.y + (playerHeight / 4), transform.position.z - (playerWidth / 2) + delta);
        right_左下 = new Vector3(transform.position.x + (playerWidth / 2) + extend_delta, transform.position.y - (playerHeight / 4), transform.position.z + (playerWidth / 2) - delta);
        right_右下 = new Vector3(transform.position.x + (playerWidth / 2) + extend_delta, transform.position.y - (playerHeight / 4), transform.position.z - (playerWidth / 2) + delta);


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

    //碰撞方向的检测（-Z方向为front）
    //方块的角度
    public bool front
    {
       
        get
        {
            ////正常情况
            //if (!isSquating)
            //{
            //    if (
            //    world.CheckForVoxel(front_左上) ||
            //    world.CheckForVoxel(front_右上) ||
            //    world.CheckForVoxel(front_左下) ||
            //    world.CheckForVoxel(front_右下)
            //    )
            //    {

            //        return true;
            //    }
            //    else
            //        return false;
            //}
            ////蹲下情况
            //else
            //{
            //    //(左下固体 && 左下延伸不是固体) || (右下固体 && 右下延伸不是固体)
            //    if ((world.CheckForVoxel(down_左下) && !world.CheckForVoxel(new Vector3(down_左下.x, down_左下.y, down_左下.z + extend_delta))) || (world.CheckForVoxel(down_右下) && !world.CheckForVoxel(new Vector3(down_右下.x, down_右下.y, down_右下.z + extend_delta))))
            //    {

            //        return true;
            //    }
            //    else
            //        return false;
            //}


            if (
                world.CheckForVoxel(front_左上) ||
                world.CheckForVoxel(front_右上) ||
                world.CheckForVoxel(front_左下) ||
                world.CheckForVoxel(front_右下)
                )
            {

                return true;
            }else if (isSquating)
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
                world.CheckForVoxel(back_右下)
                )
                return true;
            else if (isSquating)
            {
                if (
                world.CheckForVoxel(back_左上) ||
                world.CheckForVoxel(back_右上) ||
                world.CheckForVoxel(back_左下) ||
                world.CheckForVoxel(back_右下)
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
                world.CheckForVoxel(left_右下)
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

            //if (!isSquating)
            //{
            //    if (
            //    world.CheckForVoxel(right_左上) ||
            //    world.CheckForVoxel(right_右上) ||
            //    world.CheckForVoxel(right_左下) ||
            //    world.CheckForVoxel(right_右下)
            //    )
            //        return true;
            //    else
            //        return false;
            //}
            //else
            //{
            //    //(左上固体 && 左上延伸不是固体) || (左下固体 && 左下延伸不是固体)
            //    if ((world.CheckForVoxel(down_左上) && !world.CheckForVoxel(new Vector3(down_左上.x + extend_delta, down_左上.y, down_左上.z))) || (world.CheckForVoxel(down_左下) && !world.CheckForVoxel(new Vector3(down_左下.x + extend_delta, down_左下.y, down_左下.z))))
            //    {

            //        return true;
            //    }
            //    else
            //        return false;
            //}

            if (
                world.CheckForVoxel(right_左上) ||
                world.CheckForVoxel(right_右上) ||
                world.CheckForVoxel(right_左下) ||
                world.CheckForVoxel(right_右下)
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

            // 绘制射线以便调试
            //if (debug_ray)
            //{
            //    Debug.DrawRay(cam.position, cam.forward * step, Color.red, 100f);
            //}

            //(是竹子 || (是固体 && 不是基岩 && 不是水)则返回
            if (world.GetBlockType(pos) == VoxelData.Bamboo || (world.GetBlockType(pos) != VoxelData.Air && world.GetBlockType(pos) != VoxelData.BedRock && world.GetBlockType(pos) != VoxelData.Water))
            {
                

                //print($"now射线检测：{(pos-cam.position).magnitude}");
                ray_length = (pos - cam.position).magnitude;
                return pos;

            }

            //lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        point_Block_type = 255;
        return new Vector3(0f, 0f, 0f);
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



    //-------------------------------------------------------------------------------------





    //---------------------------------- debug ---------------------------------------------

    //绘制碰撞
    void drawdebug()
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
        Debug.DrawLine(front_左上, front_右上, Color.red);
        Debug.DrawLine(front_右上, right_左上, Color.red);
        Debug.DrawLine(right_左上, right_右上, Color.red);
        Debug.DrawLine(right_右上, back_右上, Color.red);
        Debug.DrawLine(back_右上, back_左上, Color.red);
        Debug.DrawLine(back_左上, left_左上, Color.red);
        Debug.DrawLine(left_左上, left_右上, Color.red);
        Debug.DrawLine(left_右上, front_左上, Color.red);

        //下半圈
        Debug.DrawLine(front_左下, front_右下, Color.red);
        Debug.DrawLine(front_右下, right_左下, Color.red);
        Debug.DrawLine(right_左下, right_右下, Color.red);
        Debug.DrawLine(right_右下, back_右下, Color.red);
        Debug.DrawLine(back_右下, back_左下, Color.red);
        Debug.DrawLine(back_左下, left_左下, Color.red);
        Debug.DrawLine(left_左下, left_右下, Color.red);
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
        Debug.DrawLine(front_左上, front_左下, Color.red);
        Debug.DrawLine(front_右上, front_右下, Color.red);
        //back
        Debug.DrawLine(back_左上, back_左下, Color.red);
        Debug.DrawLine(back_右上, back_右下, Color.red);
        //left
        Debug.DrawLine(left_左上, left_左下, Color.red);
        Debug.DrawLine(left_右上, left_右下, Color.red);
        //right
        Debug.DrawLine(right_左上, right_左下, Color.red);
        Debug.DrawLine(right_右上, right_右下, Color.red);

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

    //-------------------------------------------------------------------------------------






    //----------------------------------- 玩家状态 -------------------------------------------

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
        }else if (world.GetRelalocation(pos) == world.GetRelalocation(down_右上))
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
