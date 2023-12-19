using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    //Switch Camera
    [Header("�����")]
    public Camera FirstPersonCamera;
    public Camera ThirdPersonCamera;

    // Player Object
    private GameObject Player_Object;
    private Vector3 Player_Direction;
    CharacterController Player_Controller;

    //Player Variable
    [Header("�������")]
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

    //�Ƿ���������
    [Header("������")]
    public bool enableMouseAcceleration = false;

    // ƽ����ֵϵ��
    public float smoothSpeed = 5f;


    //World compoment
    private GameObject worldObject;
    World world;


    void Start()
    {
        // Init player
        Player_Object = transform.parent.gameObject;
        Player_Controller = Player_Object.GetComponent<CharacterController>();

        //��ȡWorld�ű�
        worldObject = GameObject.Find("World");
        world = worldObject.GetComponent<World>();

        //Hide
        HideCursor();
    }


    void Update()
    {
        // �Ƿ��ڵ���
        isGround = world.isBlock;
        isnearblock = world.isnearblock;

        // ��������
        horizontalInput = Input.GetAxisRaw("Horizontal") * (Input.GetKey(KeyCode.LeftShift) ? Move_Speed * shift_scale : Move_Speed);
        verticalInput = Input.GetAxisRaw("Vertical") * (Input.GetKey(KeyCode.LeftShift) ? Move_Speed * shift_scale : Move_Speed);

        // �Ƿ�Ctrl
        // isCrouching = Input.GetKey(KeyCode.LeftControl);

        // ����Ƿ���ǽ
        isNearBlock();

        // ����ٶ�
        targetMouseSpeedX = Input.GetAxisRaw("Mouse X") * Mouse_Sensitive * Time.deltaTime;
        targetMouseSpeedY = Input.GetAxisRaw("Mouse Y") * Mouse_Sensitive * Time.deltaTime;

        // ������������ٶȣ���ʹ��ƽ����ֵ��������ٶ�
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

        // ������������
        Player_Direction = transform.forward * verticalInput + transform.right * horizontalInput;
        Camera_verticalInput -= currentMouseSpeedY;
        Camera_verticalInput = Mathf.Clamp(Camera_verticalInput, -70f, 70f);
        Vector3 directionToXZ = new Vector3(Player_Direction.x, 0, Player_Direction.z);

        // ʵ����Ҳ���
        Player_Controller.Move(directionToXZ * Time.deltaTime);
        Player_Object.transform.Rotate(Vector3.up * currentMouseSpeedX);
        transform.localRotation = Quaternion.Euler(Camera_verticalInput, 0, 0);

        // ��ȡ�����Ծ
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
            // ������Ծ����
            velocity.y -= gravity * Time.deltaTime;  // �ڿ���ʱӦ������
        }

        // ʵ�������Ծ
        Player_Controller.Move(velocity * Time.deltaTime);

        // ��ȡ���������л�
        if (Input.GetKeyDown(KeyCode.V))
        {
            // �л������
            SwitchCamera();
        }
    }


    //�Ƿ񿿽�����
    void isNearBlock()
    {
        if (isnearblock)
        {
            // ����������������
            if (world.BlockDirection[0, 0])
            {
                // ����Ӵ�����ֹ��ǰ�ƶ�
                if (verticalInput > 0f)
                {
                    verticalInput = 0f;
                }
            }

            if (world.BlockDirection[0, 1])
            {
                // ����Ӵ�����ֹ����ƶ�
                if (verticalInput < 0f)
                {
                    verticalInput = 0f;
                }
            }

            if (world.BlockDirection[0, 2])
            {
                // ���Ӵ�����ֹ�����ƶ�
                if (horizontalInput < 0f)
                {
                    horizontalInput = 0f;
                }
            }

            if (world.BlockDirection[0, 3])
            {
                // �Ҳ�Ӵ�����ֹ�����ƶ�
                if (horizontalInput > 0f)
                {
                    horizontalInput = 0f;
                }
            }

            if (world.BlockDirection[0, 4])
            {
                // �����Ӵ�����ֹ�����ƶ�
                if (Input.GetKey(KeyCode.Space))  // ������Ծ��ͨ���ո��
                {
                    velocity.y = 0f;
                }
            }
        }
    }

    void SwitchCamera()
    {
        // �л������״̬
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
        // ���������
        cam.enabled = true;
        // ���������������������״̬���߼�
    }

    void DisableCamera(Camera cam)
    {
        // ���������
        cam.enabled = false;
        // ����������������Խ���״̬���߼�
    }

    void HideCursor()
    {
        // �������������Ļ����
        Cursor.lockState = CursorLockMode.Locked;
        //��겻����
        Cursor.visible = false;
    }

}
