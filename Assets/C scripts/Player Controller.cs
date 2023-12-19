using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float Mouse_Sensitive = 150;
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
    public float smoothSpeed = 5f;


    //World compoment
    private GameObject worldObject;
    World world;


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

        // 实现玩家跳跃
        Player_Controller.Move(velocity * Time.deltaTime);

        // 获取玩家摄像机切换
        if (Input.GetKeyDown(KeyCode.V))
        {
            // 切换摄像机
            SwitchCamera();
        }
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
                if (Input.GetKey(KeyCode.Space))  // 假设跳跃是通过空格键
                {
                    velocity.y = 0f;
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
