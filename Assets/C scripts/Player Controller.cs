using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class PlayerController : MonoBehaviour
{

    //Switch Camera
    [Header("摄像机")]
    public Camera FirstPersonCamera;
    public Camera ThirdPersonCamera;

    // Player Object
    private GameObject Player_Object;
    private Vector3 Player_Direction;
    CharacterController Player_Controller;

    //Player Variable
    [Header("玩家设置")]
    public float Move_Speed = 4;
    public float Jump_Speed = 6;
    public float gravity = 25f;
    public float shift_scale = 2;
    private Vector3 velocity;
    private bool isGround = false;
    private bool isnearblock = false;

    //Camera
    public float Mouse_Sensitive = 100;
    private float Camera_verticalInput;

    //input
    public float horizontalInput;
    public float verticalInput;
    Vector3 playerForward;
    public int Face_flag = 0;
    Vector3 directionToXZ;

    //mouse
    float targetMouseSpeedX;
    float targetMouseSpeedY;
    float currentMouseSpeedX = 0f;
    float currentMouseSpeedY = 0f;

    //是否开启鼠标加速
    [Header("鼠标加速")]
    public bool enableMouseAcceleration = false;

    // 平滑插值系数
    public float smoothSpeed = 10f;

    //蹭墙壁参数
    //public float theta;

    //World compoment
    private GameObject worldObject;
    World world;

    //debug
    public GameObject debugscreen;

    //手臂晃动
    public bool HandShake = false;
    public bool isPlacing = false;

    //放置与破坏
    [Header("手的长度/最短采样距离")]
    public Transform cam;
    public float reach = 8f;
    private float checkIncrement = 0.1f;
    public float ray_length = 0f;


    void Start()
    {
        // Init player
        Player_Object = transform.parent.gameObject;
        Player_Controller = Player_Object.GetComponent<CharacterController>();
        

        //获取World脚本
        worldObject = GameObject.Find("World");
        world = worldObject.GetComponent<World>();

        //Hide
        HideCursor();
         
        //other
        debugscreen.SetActive(false);
    }


    void Update()
    {
        //获取world数据
        Get_World_Data();

        //获取输入
        GetInput();

        //数据处理
        InputDataProcess();

        //操作实现
        OprateAchievement();

        //交互数据计算
        Set_Connect_Data();
    }





    //----------------------------------- player ----------------------------------------

    //获取玩家输入
    void GetInput()
    {
        //Debug面板
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugscreen.SetActive(!debugscreen.activeSelf);
        }

        // 移动
        horizontalInput = Input.GetAxisRaw("Horizontal") * (Input.GetKey(KeyCode.LeftShift) ? Move_Speed * shift_scale : Move_Speed);
        verticalInput = Input.GetAxisRaw("Vertical") * (Input.GetKey(KeyCode.LeftShift) ? Move_Speed * shift_scale : Move_Speed);

        // 蹲下
        // isCrouching = Input.GetKey(KeyCode.LeftControl);


        // 跳跃
        if (isGround)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                velocity.y = Jump_Speed;
            }
            else
            {
                velocity.y = 0f;
            }
        }
        else
        {
            // 处理跳跃数据
            velocity.y -= gravity * Time.deltaTime;  // 在空中时应用重力
        }


        // 获取玩家摄像机切换
        if (Input.GetKeyDown(KeyCode.V))
        {
            // 切换摄像机
            SwitchCamera();
        }


        //左键销毁泥土
        if (Input.GetMouseButtonDown(0))
        {
            //如果打到 && 不是基岩
            if (RayCast_now() != Vector3.zero)
            {
                world.GetChunkObject(RayCast_now()).EditData(world.GetRelalocation(RayCast_now()), 4);
                //print($"绝对坐标为：{RayCast_now()}");
                //print($"相对坐标为：{world.GetRelalocation(RayCast())}");
                //print($"方块类型为：{world.GetBlockType(RayCast())}");
            }


        }

        //右键放置泥土
        if (Input.GetMouseButtonDown(1))
        {
            isPlacing = true;
            //print("右键");

            //如果打到 && 距离大于2f
            if (RayCast_last() != Vector3.zero && (RayCast_last() - cam.position).magnitude > 2f)
            {
                world.GetChunkObject(RayCast_last()).EditData(world.GetRelalocation(RayCast_last()), 3);
                //print($"绝对坐标为：{RayCast_last()}");
                //print($"相对坐标为：{world.GetRelalocation(RayCast())}");
                //print($"方块类型为：{world.GetBlockType(RayCast())}");
            }


        }


        // 鼠标
        targetMouseSpeedX = Input.GetAxisRaw("Mouse X") * Mouse_Sensitive * Time.deltaTime;
        targetMouseSpeedY = Input.GetAxisRaw("Mouse Y") * Mouse_Sensitive * Time.deltaTime;

    }

    //数据处理
    void InputDataProcess()
    {

        // 检查是否贴墙
        if (isnearblock)
        {
            // 处理各个方向的限制
            if (world.BlockDirection[0, 0])
            {
                // 正面接触，禁止向前移动
                if (verticalInput > 0f)
                {
                    verticalInput = 0f;
                }
            }

            if (world.BlockDirection[0, 1])
            {
                // 后面接触，禁止向后移动
                if (verticalInput < 0f)
                {
                    verticalInput = 0f;
                }
            }

            if (world.BlockDirection[0, 2])
            {
                // 左侧接触，禁止向左移动
                if (horizontalInput < 0f)
                {
                    horizontalInput = 0f;
                }
            }

            if (world.BlockDirection[0, 3])
            {
                // 右侧接触，禁止向右移动
                if (horizontalInput > 0f)
                {
                    horizontalInput = 0f;
                }
            }

            if (world.BlockDirection[0, 4])
            {
                // 顶部接触，禁止向上移动
                //if (Input.GetKey(KeyCode.Space))  // 假设跳跃是通过空格键
                //{
                //    velocity.y = 0f;
                //}
                velocity.y = 0f;
            }

            if (world.BlockDirection[0, 6])
            {
                // 前左侧接触，禁止向前左移动
                if (horizontalInput < 0f)
                {
                    horizontalInput = 0f;
                }
                if (verticalInput > 0f)
                {
                    verticalInput = 0f;
                }

            }

            if (world.BlockDirection[0, 7])
            {
                // 前右侧接触，禁止向前右移动
                if (horizontalInput >= 0f)
                {
                    horizontalInput = 0f;
                }
                if (verticalInput > 0f)
                {
                    verticalInput = 0f;
                }
            }

            if (world.BlockDirection[0, 8])
            {
                // 后左侧接触，禁止向后左移动
                if (horizontalInput < 0f)
                {
                    horizontalInput = 0f;
                }
                if (verticalInput < 0f)
                {
                    verticalInput = 0f;
                }
            }

            if (world.BlockDirection[0, 9])
            {
                // 后右侧接触，禁止向后右移动
                if (horizontalInput >= 0f)
                {
                    horizontalInput = 0f;
                }
                if (verticalInput < 0f)
                {
                    verticalInput = 0f;
                }
            }
        }

        //移动数据处理
        playerForward = transform.forward;
        playerForward.y = 0f; // 将 Y 分量置为 0，使其只在水平平面上生效
        Player_Direction = playerForward.normalized * verticalInput + transform.right * horizontalInput;


        //鼠标视角数据处理
        Camera_verticalInput -= currentMouseSpeedY;
        Camera_verticalInput = Mathf.Clamp(Camera_verticalInput, -90f, 90f);

        //鼠标平滑加速度处理
        if (enableMouseAcceleration)
        {
            currentMouseSpeedX = Mathf.Lerp(currentMouseSpeedX, targetMouseSpeedX, smoothSpeed * Time.deltaTime);
            currentMouseSpeedY = Mathf.Lerp(currentMouseSpeedY, targetMouseSpeedY, smoothSpeed * Time.deltaTime);
        }
        else
        {
            currentMouseSpeedX = targetMouseSpeedX;
            currentMouseSpeedY = targetMouseSpeedY;
        }
    }

    //操作实现
    void OprateAchievement()
    {
        //玩家移动
        directionToXZ = new Vector3(Player_Direction.x, 0, Player_Direction.z);
        Player_Controller.Move(directionToXZ * Time.deltaTime);

        //玩家视角旋转
        Player_Object.transform.Rotate(Vector3.up * currentMouseSpeedX);
        transform.localRotation = Quaternion.Euler(Camera_verticalInput, 0, 0);

        // 玩家跳跃
        Player_Controller.Move(velocity * Time.deltaTime);
    }

    //------------------------------------------------------------------------------------





    //----------------------------------- 后台部分 ----------------------------------------

    //获取world数据
    void Get_World_Data()
    {
        // 是否在地面
        isGround = world.isBlock;

        //是否撞墙
        isnearblock = world.isnearblock;
    }

    //交互数据计算
    void Set_Connect_Data()
    {

        //手臂晃动数据
        if (horizontalInput != 0 || verticalInput != 0)
        {
            HandShake = true;
        }
        else
        {
            HandShake = false;
        }



    }


    //------------------------------------------------------------------------------------





    //----------------------------------- 工具类 ------------------------------------------
    void HideCursor()
    {
        // 将鼠标锁定在屏幕中心
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        //鼠标不可视
        UnityEngine.Cursor.visible = false;
    }

    //射线检测——返回打中的方块的相对坐标
    //没打中就是(0,0,0)
    Vector3 RayCast_now()
    {
        float step = checkIncrement;
        //Vector3 lastPos = new Vector3();

        while (step < reach)
        {

            Vector3 pos = cam.position + (cam.forward * step);

            //是固体 && 不是基岩则返回
            if (world.GetBlockType(pos) != 4 && world.GetBlockType(pos) != 0)
            {

                //print($"now射线检测：{(pos-cam.position).magnitude}");
                ray_length = (pos - cam.position).magnitude;
                return pos;

            }

            //lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

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

            //检测
            if (world.GetBlockType(pos) != 4)
            {

                //print($"last射线检测：{(lastPos - cam.position).magnitude}");
                ray_length = (lastPos - cam.position).magnitude;
                return lastPos;

            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        return new Vector3(0f, 0f, 0f);
    }

    //camera
    void SwitchCamera()
    {
        // 切换摄像机状态
        if (FirstPersonCamera.enabled)
        {
            EnableCamera(ThirdPersonCamera);
            DisableCamera(FirstPersonCamera);
        }
        else
        {
            EnableCamera(FirstPersonCamera);
            DisableCamera(ThirdPersonCamera);
        }
    }
    void EnableCamera(Camera cam)
    {
        // 启用摄像机
        cam.enabled = true;
        // 这里可以添加其他针对启用状态的逻辑
    }
    void DisableCamera(Camera cam)
    {
        // 禁用摄像机
        cam.enabled = false;
        // 这里可以添加其他针对禁用状态的逻辑
    }

    //计算特殊角度
    //void CountTheta()
    //{
    //    //计算入射角度
    //    //theta = Mathf.FloorToInt(Mathf.Acos(Vector3.Dot(playerForward.normalized, Vector3.forward.normalized)) * Mathf.Rad2Deg);
    //    // 获取向量在XOZ平面上的角度
    //    float angleInRadians = Mathf.Atan2(playerForward.x, playerForward.z);
    //    // 将弧度转换为角度
    //    float angleInDegrees = angleInRadians * Mathf.Rad2Deg;

    //    // 对角度进行修正，使得角度在指定的范围内
    //    if (angleInDegrees > 90f)
    //    {
    //        angleInDegrees = angleInDegrees - 360f;
    //    }
    //    else if (angleInDegrees < -90f)
    //    {
    //        angleInDegrees = angleInDegrees + 360f;
    //    }
    //    theta = Mathf.FloorToInt(angleInDegrees);

    //}
    //------------------------------------------------------------------------------------



}