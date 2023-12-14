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
        //�Ƿ��ڵ���
        isGround = Player_Controller.isGrounded;

        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        //��ȡ�������
        float horizontalInput = Input.GetAxisRaw("Horizontal") * Move_Speed;
        float verticalInput = Input.GetAxisRaw("Vertical") * Move_Speed;
        float Mouse_X = Input.GetAxis("Mouse X") * Mouse_Sensitive * Time.deltaTime;
        float Mouse_Y = Input.GetAxis("Mouse Y") * Mouse_Sensitive * Time.deltaTime;

        //������������
        Player_Direction = transform.forward * verticalInput + transform.right * horizontalInput;
        Camera_verticalInput -= Mouse_Y;
        Camera_verticalInput = Mathf.Clamp(Camera_verticalInput, -70f, 70f);

        //ʵ����Ҳ���
        Player_Controller.Move(Player_Direction * Time.deltaTime);
        Player_Object.transform.Rotate(Vector3.up * Mouse_X);
        transform.localRotation = Quaternion.Euler(Camera_verticalInput, 0, 0);

        //��ȡ�����Ծ
        if (Input.GetButtonDown("Jump") && isGround)
        {
            velocity.y = Jump_Speed;
        }

        //������Ծ����
        velocity.y -= (9.8f) * Time.deltaTime;

        //ʵ�������Ծ
        Player_Controller.Move(velocity * Time.deltaTime);


        //��ȡ���������л�
        if (Input.GetKeyDown(KeyCode.V))
        {
            // �л������
            SwitchCamera();
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
