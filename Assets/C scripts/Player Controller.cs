using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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
    float horizontalInput;
    float verticalInput;


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


    //World compoment
    private GameObject worldObject;
    World world;


    //手臂晃动
    public bool HandShake = false;
    Vector3 v_temp = new Vector3(0, 0, 0);

    //放置与破坏
    [Header("手的长度/最短采样距离")]
    public Transform cam;
    public float reach = 8f;
    private float checkIncrement = 0.1f;
    


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
    }


    void Update()
    {
        // 是否在地面
        isGround = world.isBlock;
        isnearblock = world.isnearblock;

        // 键盘输入
        horizontalInput = Input.GetAxisRaw("Horizontal") * (Input.GetKey(KeyCode.LeftShift) ? Move_Speed * shift_scale : Move_Speed);
        verticalInput = Input.GetAxisRaw("Vertical") * (Input.GetKey(KeyCode.LeftShift) ? Move_Speed * shift_scale : Move_Speed);

        // 是否按Ctrl
        // isCrouching = Input.GetKey(KeyCode.LeftControl);

        // 检查是否贴墙
        isNearBlock();

        // 鼠标速度
        targetMouseSpeedX = Input.GetAxisRaw("Mouse X") * Mouse_Sensitive * Time.deltaTime;
        targetMouseSpeedY = Input.GetAxisRaw("Mouse Y") * Mouse_Sensitive * Time.deltaTime;

        // 如果启用鼠标加速度，则使用平滑插值计算鼠标速度
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

        // 处理输入数据
        Player_Direction = transform.forward * verticalInput + transform.right * horizontalInput;
        Camera_verticalInput -= currentMouseSpeedY;
        Camera_verticalInput = Mathf.Clamp(Camera_verticalInput, -70f, 70f);
        Vector3 directionToXZ = new Vector3(Player_Direction.x, 0, Player_Direction.z);

        // 实现玩家操作
        Player_Controller.Move(directionToXZ * Time.deltaTime);
        Player_Object.transform.Rotate(Vector3.up * currentMouseSpeedX);
        transform.localRotation = Quaternion.Euler(Camera_verticalInput, 0, 0);

        // 获取玩家跳跃
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

        //检测手臂晃动




        // 实现玩家跳跃
        Player_Controller.Move(velocity * Time.deltaTime);

        // 获取玩家摄像机切换
        if (Input.GetKeyDown(KeyCode.V))
        {
            // 切换摄像机
            SwitchCamera();
        }


        //实现放置与破坏
        CreateAndDestroyBlock();


    }




    //放置与销毁
    void CreateAndDestroyBlock()
    {
        //左键销毁泥土
        if(Input.GetMouseButtonDown(0))
        {
            if (RayCast_now() != Vector3.zero)
            {
                world.GetChunkObject(RayCast_now()).EditData(world.GetRelalocation(RayCast_now()),4);
                print($"绝对坐标为：{RayCast_now()}");
                //print($"相对坐标为：{world.GetRelalocation(RayCast())}");
                //print($"方块类型为：{world.GetBlockType(RayCast())}");
            }


        }

        //右键放置泥土
        if (Input.GetMouseButtonDown(1))
        {

            if (RayCast_last() != Vector3.zero)
            {
                world.GetChunkObject(RayCast_last()).EditData(world.GetRelalocation(RayCast_last()),3);
                print($"绝对坐标为：{RayCast_last()}");
                //print($"相对坐标为：{world.GetRelalocation(RayCast())}");
                //print($"方块类型为：{world.GetBlockType(RayCast())}");
            }

        }

        
    }


    //射线检测——返回打中的方块的相对坐标——没打中就是(0,0,0)
    Vector3 RayCast_now()
    {
        float step = checkIncrement;
        //Vector3 lastPos = new Vector3();

        while (step < reach)
        {

            Vector3 pos = cam.position + (cam.forward * step);

            //检测
            if (world.GetBlockType(pos) != 4)
            {


                return pos;

            }

            //lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        return new Vector3(0f,0f,0f);
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


                return lastPos;

            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        return new Vector3(0f, 0f, 0f);
    }


    //是否靠近方块
    void isNearBlock()
    {
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
    }

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

    void HideCursor()
    {
        // 将鼠标锁定在屏幕中心
        Cursor.lockState = CursorLockMode.Locked;
        //鼠标不可视
        Cursor.visible = false;
    }

}