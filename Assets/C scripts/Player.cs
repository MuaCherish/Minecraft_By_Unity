using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    private bool isGrounded;
    private bool isSprinting;

    [Header("Transforms")]
    public Transform cam;
    public World world;

    [Header("��ɫ����")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float jumpForce = 6f;
    public float gravity = -15f;

    [Header("��ײ����")]
    public float playerWidth = 0.3f;
    public float playerHeight = 1.7f;
    public float extend_delta = 0.1f;
    public float delta = 0.05f;

    //����
    [HideInInspector]
    public float horizontalInput;
    [HideInInspector]
    public float verticalInput;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
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


    //��ײ��������
    // ������ĸ���
    Vector3 up_���� = new Vector3();
    Vector3 up_���� = new Vector3();
    Vector3 up_���� = new Vector3();
    Vector3 up_���� = new Vector3();

    // ������ĸ���
    Vector3 down_���� = new Vector3();
    Vector3 down_���� = new Vector3();
    Vector3 down_���� = new Vector3();
    Vector3 down_���� = new Vector3();

    //front
    Vector3 front_���� = new Vector3();
    Vector3 front_���� = new Vector3();
    Vector3 front_���� = new Vector3();
    Vector3 front_���� = new Vector3();

    //back
    Vector3 back_���� = new Vector3();
    Vector3 back_���� = new Vector3();
    Vector3 back_���� = new Vector3();
    Vector3 back_���� = new Vector3();

    //left
    Vector3 left_���� = new Vector3();
    Vector3 left_���� = new Vector3();
    Vector3 left_���� = new Vector3();
    Vector3 left_���� = new Vector3();

    //right
    Vector3 right_���� = new Vector3();
    Vector3 right_���� = new Vector3();
    Vector3 right_���� = new Vector3();
    Vector3 right_���� = new Vector3();


   


    //��ײ���
    private void FixedUpdate()
    {
        if (world.game_state == Game_State.Playing)
        {
            update_block();
            CalculateVelocity();
            AchieveInput();

        }

    }


    //��ȡ�������
    private void Update()
    {
        if (world.game_state == Game_State.Playing)
        {
            GetPlayerInputs();
            

            if (showBlockMesh)
            {
                drawdebug();
            }
        }
         

        

    }


    //���ݼ���
    private void CalculateVelocity()
    {

        // ��������
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // �����ٶ�
        if (isSprinting)
            velocity = ((transform.forward * verticalInput) + (transform.right * horizontalInput)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * verticalInput) + (transform.right * horizontalInput)) * Time.fixedDeltaTime * walkSpeed;

        // �ϲ�����
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;



        //��Ĥ����
        //ǰ��
        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;

        //����
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;





        if (velocity.y < 0)
        {
            velocity.y = checkDownSpeed(velocity.y);
        }
        else if (velocity.y > 0)
        {
            velocity.y = checkUpSpeed(velocity.y);
        }
       
            



    }

    
    //���ղ���
    private void GetPlayerInputs()
    {

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;

        if (isGrounded && Input.GetButtonDown("Jump"))
            jumpRequest = true;

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
            if (RayCast_last() != Vector3.zero && (RayCast_last() - cam.position).magnitude > max_hand_length && world.GetRelalocation(RayCast_last()) != world.GetRelalocation(new Vector3(world.PlayerFoot.position.x, world.PlayerFoot.position.y + 1f, world.PlayerFoot.position.z)))
            {
                world.GetChunkObject(RayCast_last()).EditData(world.GetRelalocation(RayCast_last()), 3);
                //print($"��������Ϊ��{RayCast_last()}");
                //print($"�������Ϊ��{world.GetRelalocation(RayCast())}");
                //print($"��������Ϊ��{world.GetBlockType(RayCast())}");
            }


        }

    }


    //ʵ�ֲ���
    private void AchieveInput()
    {
        if (jumpRequest)
        {
            verticalMomentum = jumpForce;
            isGrounded = false;
            jumpRequest = false;
            
        }
        transform.Rotate(Vector3.up * mouseHorizontal);
        cam.Rotate(Vector3.right * -mouseVertical);
        transform.Translate(velocity, Space.World);
    }






    //----------------------------------��ײ���---------------------------------------

    //������ײ��
    void update_block()
    {

        // ������ĸ���
        up_���� = new Vector3(transform.position.x - (playerWidth / 2), transform.position.y + (playerHeight / 2), transform.position.z + (playerWidth / 2));
        up_���� = new Vector3(transform.position.x + (playerWidth / 2), transform.position.y + (playerHeight / 2), transform.position.z + (playerWidth / 2));
        up_���� = new Vector3(transform.position.x + (playerWidth / 2), transform.position.y + (playerHeight / 2), transform.position.z - (playerWidth / 2));
        up_���� = new Vector3(transform.position.x - (playerWidth / 2), transform.position.y + (playerHeight / 2), transform.position.z - (playerWidth / 2));

        // ������ĸ���
        down_���� = new Vector3(transform.position.x - (playerWidth / 2), transform.position.y - (playerHeight / 2), transform.position.z + (playerWidth / 2));
        down_���� = new Vector3(transform.position.x + (playerWidth / 2), transform.position.y - (playerHeight / 2), transform.position.z + (playerWidth / 2));
        down_���� = new Vector3(transform.position.x + (playerWidth / 2), transform.position.y - (playerHeight / 2), transform.position.z - (playerWidth / 2));
        down_���� = new Vector3(transform.position.x - (playerWidth / 2), transform.position.y - (playerHeight / 2), transform.position.z - (playerWidth / 2));


        //front
        front_���� = new Vector3(transform.position.x - (playerWidth / 2) + delta, transform.position.y + (playerHeight / 4), transform.position.z + (playerWidth / 2) + extend_delta);
        front_���� = new Vector3(transform.position.x + (playerWidth / 2) - delta, transform.position.y + (playerHeight / 4), transform.position.z + (playerWidth / 2) + extend_delta);
        front_���� = new Vector3(transform.position.x - (playerWidth / 2) + delta, transform.position.y - (playerHeight / 4), transform.position.z + (playerWidth / 2) + extend_delta);
        front_���� = new Vector3(transform.position.x + (playerWidth / 2) - delta, transform.position.y - (playerHeight / 4), transform.position.z + (playerWidth / 2) + extend_delta);


        //back
        back_���� = new Vector3(transform.position.x - (playerWidth / 2) + delta, transform.position.y + (playerHeight / 4), transform.position.z - (playerWidth / 2) - extend_delta);
        back_���� = new Vector3(transform.position.x + (playerWidth / 2) - delta, transform.position.y + (playerHeight / 4), transform.position.z - (playerWidth / 2) - extend_delta);
        back_���� = new Vector3(transform.position.x - (playerWidth / 2) + delta, transform.position.y - (playerHeight / 4), transform.position.z - (playerWidth / 2) - extend_delta);
        back_���� = new Vector3(transform.position.x + (playerWidth / 2) - delta, transform.position.y - (playerHeight / 4), transform.position.z - (playerWidth / 2) - extend_delta);


        //left
        left_���� = new Vector3(transform.position.x - (playerWidth / 2) - extend_delta, transform.position.y + (playerHeight / 4), transform.position.z - (playerWidth / 2) + delta);
        left_���� = new Vector3(transform.position.x - (playerWidth / 2) - extend_delta, transform.position.y + (playerHeight / 4), transform.position.z + (playerWidth / 2) - delta);
        left_���� = new Vector3(transform.position.x - (playerWidth / 2) - extend_delta, transform.position.y - (playerHeight / 4), transform.position.z - (playerWidth / 2) + delta);
        left_���� = new Vector3(transform.position.x - (playerWidth / 2) - extend_delta, transform.position.y - (playerHeight / 4), transform.position.z + (playerWidth / 2) - delta);


        //right
        right_���� = new Vector3(transform.position.x + (playerWidth / 2) + extend_delta, transform.position.y + (playerHeight / 4), transform.position.z + (playerWidth / 2) - delta);
        right_���� = new Vector3(transform.position.x + (playerWidth / 2) + extend_delta, transform.position.y + (playerHeight / 4), transform.position.z - (playerWidth / 2) + delta);
        right_���� = new Vector3(transform.position.x + (playerWidth / 2) + extend_delta, transform.position.y - (playerHeight / 4), transform.position.z + (playerWidth / 2) - delta);
        right_���� = new Vector3(transform.position.x + (playerWidth / 2) + extend_delta, transform.position.y - (playerHeight / 4), transform.position.z - (playerWidth / 2) + delta);


    }

    //��ײ��⣨���£�
    private float checkDownSpeed(float downSpeed)
    {
      

        if (
            world.CheckForVoxel(down_����) ||
            world.CheckForVoxel(down_����) ||
            world.CheckForVoxel(down_����) ||
            world.CheckForVoxel(down_����)

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

    //��ײ��⣨ͷ�ϣ�
    private float checkUpSpeed(float upSpeed)
    {
        


        if (
            world.CheckForVoxel(up_����) ||
            world.CheckForVoxel(up_����) ||
            world.CheckForVoxel(up_����) ||
            world.CheckForVoxel(up_����)
           )
        {
            return 0;

        }
        else
        {
            return upSpeed;

        }

    }

    //��ײ���
    public bool front
    {

        get
        {
            if (
                world.CheckForVoxel(front_����) ||
                world.CheckForVoxel(front_����) ||
                world.CheckForVoxel(front_����) ||
                world.CheckForVoxel(front_����) 
                )
            {
                
                return true;
            }
            else
                return false;
        }

    }
    public bool back
    {

        get
        {
            if (
                world.CheckForVoxel(back_����) ||
                world.CheckForVoxel(back_����) ||
                world.CheckForVoxel(back_����) ||
                world.CheckForVoxel(back_����)
                )
                return true;
            else
                return false;
        }

    }
    public bool left
    {

        get
        {
            if (
                world.CheckForVoxel(left_����) ||
                world.CheckForVoxel(left_����) ||
                world.CheckForVoxel(left_����) ||
                world.CheckForVoxel(left_����)
                )
                return true;
            else
                return false;
        }

    }
    public bool right
    {

        get
        {
            if (
                world.CheckForVoxel(right_����) ||
                world.CheckForVoxel(right_����) ||
                world.CheckForVoxel(right_����) ||
                world.CheckForVoxel(right_����)
                )
                return true;
            else
                return false;
        }

    }

    //-------------------------------------------------------------------------------------





    //--------------------------------- ���߼�� ------------------------------------------

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
            //if (debug_ray)
            //{
            //    Debug.DrawRay(cam.position, cam.forward * step, Color.red, 100f);
            //}

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
            //if (debug_ray)
            //{
            //    Debug.DrawRay(cam.position, cam.forward * step, Color.red, 100f);
            //}

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
    //-------------------------------------------------------------------------------------





    //---------------------------------- debug ---------------------------------------------

    //������ײ
    void drawdebug()
    {
        // �����������
        Debug.DrawLine(up_����, up_����, Color.red); // ���� -- ����
        Debug.DrawLine(up_����, up_����, Color.red); // ���� -- ����
        Debug.DrawLine(up_����, up_����, Color.red); // ���� -- ����
        Debug.DrawLine(up_����, up_����, Color.red); // ���� -- ����

        // �����������
        Debug.DrawLine(down_����, down_����, Color.red); // ���� -- ����
        Debug.DrawLine(down_����, down_����, Color.red); // ���� -- ����
        Debug.DrawLine(down_����, down_����, Color.red); // ���� -- ����
        Debug.DrawLine(down_����, down_����, Color.red); // ���� -- ����


        //�ϰ�Ȧ
        Debug.DrawLine(front_����, front_����, Color.red);
        Debug.DrawLine(front_����, right_����, Color.red);
        Debug.DrawLine(right_����, right_����, Color.red);
        Debug.DrawLine(right_����, back_����, Color.red);
        Debug.DrawLine(back_����, back_����, Color.red);
        Debug.DrawLine(back_����, left_����, Color.red);
        Debug.DrawLine(left_����, left_����, Color.red);
        Debug.DrawLine(left_����, front_����, Color.red);

        //�°�Ȧ
        Debug.DrawLine(front_����, front_����, Color.red);
        Debug.DrawLine(front_����, right_����, Color.red);
        Debug.DrawLine(right_����, right_����, Color.red);
        Debug.DrawLine(right_����, back_����, Color.red);
        Debug.DrawLine(back_����, back_����, Color.red);
        Debug.DrawLine(back_����, left_����, Color.red);
        Debug.DrawLine(left_����, left_����, Color.red);
        Debug.DrawLine(left_����, front_����, Color.red);

        //������
        //up����
        Debug.DrawLine(up_����, front_����, Color.red);
        Debug.DrawLine(up_����, left_����, Color.red);
        //up����
        Debug.DrawLine(up_����, front_����, Color.red);
        Debug.DrawLine(up_����, right_����, Color.red);
        //up����
        Debug.DrawLine(up_����, left_����, Color.red);
        Debug.DrawLine(up_����, back_����, Color.red);
        //up����
        Debug.DrawLine(up_����, back_����, Color.red);
        Debug.DrawLine(up_����, right_����, Color.red);

        //������
        //front
        Debug.DrawLine(front_����, front_����, Color.red);
        Debug.DrawLine(front_����, front_����, Color.red);
        //back
        Debug.DrawLine(back_����, back_����, Color.red);
        Debug.DrawLine(back_����, back_����, Color.red);
        //left
        Debug.DrawLine(left_����, left_����, Color.red);
        Debug.DrawLine(left_����, left_����, Color.red);
        //right
        Debug.DrawLine(right_����, right_����, Color.red);
        Debug.DrawLine(right_����, right_����, Color.red);

        //������
        //down����
        Debug.DrawLine(down_����, front_����, Color.red);
        Debug.DrawLine(down_����, left_����, Color.red);
        //down����
        Debug.DrawLine(down_����, front_����, Color.red);
        Debug.DrawLine(down_����, right_����, Color.red);
        //down����
        Debug.DrawLine(down_����, left_����, Color.red);
        Debug.DrawLine(down_����, back_����, Color.red);
        //down����
        Debug.DrawLine(down_����, back_����, Color.red);
        Debug.DrawLine(down_����, right_����, Color.red);

    }

    //-------------------------------------------------------------------------------------


}