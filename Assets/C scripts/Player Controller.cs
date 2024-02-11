using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class PlayerController : MonoBehaviour
{

    //Switch Camera
    [Header("�����")]
    public Camera FirstPersonCamera;
    public Camera ThirdPersonCamera;
    public Transform cam;

    // Player Object
    private GameObject Player_Object;
    private Vector3 Player_Direction;
    CharacterController Player_Controller;

    //Player Variable
    [Header("�������")]
    //private Vector3 Start_Position = new Vector3(1600f,63f,1600f);
    public float Move_Speed = 4;
    public float Jump_Speed = 6;
    public float gravity = 25f;
    public float shift_scale = 2;
    [HideInInspector]
    public Vector3 velocity;
    private bool isGround = false;
    private bool isnearblock = false;
    

    //Camera
    public float Mouse_Sensitive = 100;
    private float Camera_verticalInput;

    //input
    [HideInInspector]
    public float horizontalInput;
    [HideInInspector]
    public float verticalInput;
    Vector3 playerForward;
    [HideInInspector]
    public int Face_flag = -1;
    Vector3 directionToXZ;
    private float max_hand_length = 0.7f;

    //mouse
    float targetMouseSpeedX;
    float targetMouseSpeedY;
    float currentMouseSpeedX = 0f;
    float currentMouseSpeedY = 0f;

    //�Ƿ���������
    [Header("������")]
    public bool enableMouseAcceleration = false;

    // ƽ����ֵϵ��
    public float smoothSpeed = 10f;

    //��ǽ�ڲ���
    //public float theta;

    //World compoment
    private GameObject worldObject;
    public World world;

    //debug
    public GameObject debugscreen;
    public bool debug_ray = false;

    //�ֱۻζ�
    [HideInInspector]
    public bool HandShake = false;
    [HideInInspector]
    public bool isPlacing = false;

    //�������ƻ�
    [Header("�ֵĳ���")]
    public float reach = 8f;
    private float checkIncrement = 0.01f;
    [HideInInspector]
    public float ray_length = 0f;

    //UI Manager
    //public GameObject UIManager;
    public CanvasManager CanvasManager;

    bool hasExec = true;



    void Start()
    {
        // Init player
        Player_Object = transform.parent.gameObject;
        Player_Controller = Player_Object.GetComponent<CharacterController>();
        //CanvasManager = UIManager.GetComponent<CanvasManager>();
        
        //��ȡWorld�ű�
        //worldObject = GameObject.Find("World");
        //world = worldObject.GetComponent<World>();
       
        //��ʼ������λ��
        //Player_Object.transform.position = new Vector3(world.Start_Position.x, world.Start_Position.y, world.Start_Position.z);
         
        //other 
        debugscreen.SetActive(false);
    }


    void Update()
    {

        //Debug.Log($"{Player_Object.transform.position}");

        if (CanvasManager.isGamePlaying)
        {

            

            //��ȡworld����
            Get_World_Data();

            //��ȡ����
            GetInput();

            //���ݴ���
            InputDataProcess();

            //����ʵ��
            OprateAchievement();

            //�������ݼ���
            Set_Connect_Data();

            if (hasExec)
            {
                //Hide
                HideCursor();

                //��ʼ������λ��
                Player_Object.transform.position = new Vector3(world.Start_Position.x, world.Start_Position.y, world.Start_Position.z);

                hasExec = false;
            }
        }
        
    }





    //----------------------------------- player ----------------------------------------

    

    //��ȡ�������
    void GetInput()
    {
        //Debug���
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugscreen.SetActive(!debugscreen.activeSelf);
        }

        // �ƶ�
        horizontalInput = Input.GetAxisRaw("Horizontal") * (Input.GetKey(KeyCode.LeftShift) ? Move_Speed * shift_scale : Move_Speed);
        verticalInput = Input.GetAxisRaw("Vertical") * (Input.GetKey(KeyCode.LeftShift) ? Move_Speed * shift_scale : Move_Speed);

        // ����
        // isCrouching = Input.GetKey(KeyCode.LeftControl);


        // ��Ծ
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


        // ��ȡ���������л�
        if (Input.GetKeyDown(KeyCode.V))
        {
            // �л������
            SwitchCamera();
        }


        //�����������
        if (Input.GetMouseButtonDown(0))
        {
            //����� && ���ǻ���
            if (RayCast_now() != Vector3.zero)
            {
                world.GetChunkObject(RayCast_now()).EditData(world.GetRelalocation(RayCast_now()), 4);
                //print($"��������Ϊ��{RayCast_now()}");
                //print($"�������Ϊ��{world.GetRelalocation(RayCast())}");
                //print($"��������Ϊ��{world.GetBlockType(RayCast())}");
            }


        }

        //�Ҽ���������
        if (Input.GetMouseButtonDown(1))
        {
            isPlacing = true;
            //print("�Ҽ�");

            //����� && �������2f && �Ҳ��ǽŵ���
            if (RayCast_last() != Vector3.zero && (RayCast_last() - cam.position).magnitude > max_hand_length && world.GetRelalocation(RayCast_last()) != world.GetRelalocation(new Vector3(world.Block_transforms[5].position.x, world.Block_transforms[5].position.y + 1f, world.Block_transforms[5].position.z)))
            {
                world.GetChunkObject(RayCast_last()).EditData(world.GetRelalocation(RayCast_last()), 3);
                //print($"��������Ϊ��{RayCast_last()}");
                //print($"�������Ϊ��{world.GetRelalocation(RayCast())}");
                //print($"��������Ϊ��{world.GetBlockType(RayCast())}");
            }


        }


        // ���
        targetMouseSpeedX = Input.GetAxisRaw("Mouse X") * Mouse_Sensitive * Time.deltaTime;
        targetMouseSpeedY = Input.GetAxisRaw("Mouse Y") * Mouse_Sensitive * Time.deltaTime;

    }

    //���ݴ���
    void InputDataProcess()
    {

        // ����Ƿ���ǽ
        if (isnearblock)
        {
            // ����������������
            if (world.BlockDirection[0, 0])
            {
                Face_flag = 0;
                // ����Ӵ�����ֹ��ǰ�ƶ�
                if (verticalInput > 0f)
                {
                    verticalInput = 0f;
                }
            }

            if (world.BlockDirection[0, 1])
            {
                Face_flag = 1;
                // ����Ӵ�����ֹ����ƶ�
                if (verticalInput < 0f)
                {
                    verticalInput = 0f;
                }
            }

            if (world.BlockDirection[0, 2])
            {
                Face_flag = 2;
                // ���Ӵ�����ֹ�����ƶ�
                if (horizontalInput < 0f)
                {
                    horizontalInput = 0f;
                }
            }

            if (world.BlockDirection[0, 3])
            {
                Face_flag = 3;
                // �Ҳ�Ӵ�����ֹ�����ƶ�
                if (horizontalInput > 0f)
                {
                    horizontalInput = 0f;
                }
            }

            if (world.BlockDirection[0, 4])
            {
                // �����Ӵ�����ֹ�����ƶ�
                //if (Input.GetKey(KeyCode.Space))  // ������Ծ��ͨ���ո��
                //{
                //    velocity.y = 0f;
                //}
                Face_flag = 4;
                velocity.y = 0f;
            }

            if (world.BlockDirection[0, 6])
            {
                Face_flag = 6;
                // ǰ���Ӵ�����ֹ��ǰ���ƶ�
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
                Face_flag = 7;
                // ǰ�Ҳ�Ӵ�����ֹ��ǰ���ƶ�
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
                Face_flag = 8;
                // �����Ӵ�����ֹ������ƶ�
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
                Face_flag = 9;
                // ���Ҳ�Ӵ�����ֹ������ƶ�
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
        else
        {
            Face_flag = -1;
        }

        //�ƶ����ݴ���
        playerForward = transform.forward;
        playerForward.y = 0f; // �� Y ������Ϊ 0��ʹ��ֻ��ˮƽƽ������Ч
        Player_Direction = playerForward.normalized * verticalInput + transform.right * horizontalInput;


        //if (transform.position.y >= (VoxelData.ChunkHeight - 1))
        //{
        //    // ������Ծ����
        //     velocity.y -= gravity * Time.deltaTime;  // �ڿ���ʱӦ������
        //}



        //����ӽ����ݴ���
        Camera_verticalInput -= currentMouseSpeedY;
        Camera_verticalInput = Mathf.Clamp(Camera_verticalInput, -90f, 90f);


        //���ƽ�����ٶȴ���
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

    //����ʵ��
    void OprateAchievement()
    {
        //����ƶ�
        directionToXZ = new Vector3(Player_Direction.x, 0, Player_Direction.z);
        Player_Controller.Move(directionToXZ * Time.deltaTime);

        //����ӽ���ת
        Player_Object.transform.Rotate(Vector3.up * currentMouseSpeedX);
        transform.localRotation = Quaternion.Euler(Camera_verticalInput, 0, 0);

        // �����Ծ
        Player_Controller.Move(velocity * Time.deltaTime);
    }

    //------------------------------------------------------------------------------------





    //----------------------------------- ��̨���� ----------------------------------------

    //��ȡworld����
    void Get_World_Data()
    {
        // �Ƿ��ڵ���
        isGround = world.isBlock;

        //�Ƿ�ײǽ
        isnearblock = world.isnearblock;
    }

    //�������ݼ���
    void Set_Connect_Data()
    {

        //�ֱۻζ�����
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





    //----------------------------------- ������ ------------------------------------------
    void HideCursor()
    {
        // �������������Ļ����
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        //��겻����
        UnityEngine.Cursor.visible = false;
    }

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
            if (debug_ray)
            {
                Debug.DrawRay(cam.position, cam.forward * step, Color.red, 100f);
            }

            //�ǹ��� && ���ǻ����򷵻�
            if (world.GetBlockType(pos) != 4 && world.GetBlockType(pos) != 0)
            {

                //print($"now���߼�⣺{(pos-cam.position).magnitude}");
                ray_length = (pos - cam.position).magnitude;
                return pos;

            }

            //lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

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
            if (debug_ray)
            {
                Debug.DrawRay(cam.position, cam.forward * step, Color.red, 100f);
            }

            //���
            if (world.GetBlockType(pos) != 4)
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

    //camera
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

    //��������Ƕ�
    //void CountTheta()
    //{
    //    //��������Ƕ�
    //    //theta = Mathf.FloorToInt(Mathf.Acos(Vector3.Dot(playerForward.normalized, Vector3.forward.normalized)) * Mathf.Rad2Deg);
    //    // ��ȡ������XOZƽ���ϵĽǶ�
    //    float angleInRadians = Mathf.Atan2(playerForward.x, playerForward.z);
    //    // ������ת��Ϊ�Ƕ�
    //    float angleInDegrees = angleInRadians * Mathf.Rad2Deg;

    //    // �ԽǶȽ���������ʹ�ýǶ���ָ���ķ�Χ��
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