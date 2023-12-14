using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Player Object
    private GameObject Player_Object;
    private Vector3 Player_Direction;
    CharacterController Player_Controller;

    //Player Variable
    public float Move_Speed = 4;
    public float Jump_Speed = 5;
    private Vector3 velocity;
    private bool isGround;

    //Camera
    public float Mouse_Sensitive = 200;
    private float Camera_verticalInput;

    //Switch Camera
    public Camera FirstPersonCamera;
    public Camera ThirdPersonCamera;


    void Start()
    {
        // Init player
        Player_Object = transform.parent.gameObject;
        Player_Controller = Player_Object.GetComponent<CharacterController>();

        //Hide
        HideCursor();
    }


    void Update()
    {
        //是否在地面
        isGround = Player_Controller.isGrounded;

        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        //获取玩家输入
        float horizontalInput = Input.GetAxisRaw("Horizontal") * Move_Speed;
        float verticalInput = Input.GetAxisRaw("Vertical") * Move_Speed;
        float Mouse_X = Input.GetAxis("Mouse X") * Mouse_Sensitive * Time.deltaTime;
        float Mouse_Y = Input.GetAxis("Mouse Y") * Mouse_Sensitive * Time.deltaTime;

        //处理输入数据
        Player_Direction = transform.forward * verticalInput + transform.right * horizontalInput;
        Camera_verticalInput -= Mouse_Y;
        Camera_verticalInput = Mathf.Clamp(Camera_verticalInput, -70f, 70f);

        //实现玩家操作
        Player_Controller.Move(Player_Direction * Time.deltaTime);
        Player_Object.transform.Rotate(Vector3.up * Mouse_X);
        transform.localRotation = Quaternion.Euler(Camera_verticalInput, 0, 0);

        //获取玩家跳跃
        if (Input.GetButtonDown("Jump") && isGround)
        {
            velocity.y = Jump_Speed;
        }

        //处理跳跃数据
        velocity.y -= (9.8f) * Time.deltaTime;

        //实现玩家跳跃
        Player_Controller.Move(velocity * Time.deltaTime);


        //获取玩家摄像机切换
        if (Input.GetKeyDown(KeyCode.V))
        {
            // 切换摄像机
            SwitchCamera();
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
