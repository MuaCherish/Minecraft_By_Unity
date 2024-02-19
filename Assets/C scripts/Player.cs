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

    [Header("角色参数")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float jumpForce = 6f;
    public float gravity = -15f;

    [Header("碰撞参数")]
    public float playerWidth = 0.3f;
    public float playerHeight = 1.7f;
    public float extend_delta = 0.1f;
    public float delta = 0.05f;

    //输入
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


   


    //碰撞检测
    private void FixedUpdate()
    {
        if (world.game_state == Game_State.Playing)
        {
            update_block();
            CalculateVelocity();
            AchieveInput();

        }

    }


    //获取玩家输入
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


    //数据计算
    private void CalculateVelocity()
    {

        // 计算重力
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // 计算速度
        if (isSprinting)
            velocity = ((transform.forward * verticalInput) + (transform.right * horizontalInput)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * verticalInput) + (transform.right * horizontalInput)) * Time.fixedDeltaTime * walkSpeed;

        // 合并数据
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;



        //滑膜数据
        //前后
        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;

        //左右
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

    
    //接收操作
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

            //如果打到 && 距离大于2f && 且不是脚底下
            if (RayCast_last() != Vector3.zero && (RayCast_last() - cam.position).magnitude > max_hand_length && world.GetRelalocation(RayCast_last()) != world.GetRelalocation(new Vector3(world.PlayerFoot.position.x, world.PlayerFoot.position.y + 1f, world.PlayerFoot.position.z)))
            {
                world.GetChunkObject(RayCast_last()).EditData(world.GetRelalocation(RayCast_last()), 3);
                //print($"绝对坐标为：{RayCast_last()}");
                //print($"相对坐标为：{world.GetRelalocation(RayCast())}");
                //print($"方块类型为：{world.GetBlockType(RayCast())}");
            }


        }

    }


    //实现操作
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






    //----------------------------------碰撞检测---------------------------------------

    //更新碰撞盒
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

    //碰撞检测
    public bool front
    {

        get
        {
            if (
                world.CheckForVoxel(front_左上) ||
                world.CheckForVoxel(front_右上) ||
                world.CheckForVoxel(front_左下) ||
                world.CheckForVoxel(front_右下) 
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
                world.CheckForVoxel(back_左上) ||
                world.CheckForVoxel(back_右上) ||
                world.CheckForVoxel(back_左下) ||
                world.CheckForVoxel(back_右下)
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
                world.CheckForVoxel(left_左上) ||
                world.CheckForVoxel(left_右上) ||
                world.CheckForVoxel(left_左下) ||
                world.CheckForVoxel(left_右下)
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
                world.CheckForVoxel(right_左上) ||
                world.CheckForVoxel(right_右上) ||
                world.CheckForVoxel(right_左下) ||
                world.CheckForVoxel(right_右下)
                )
                return true;
            else
                return false;
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

            // 绘制射线以便调试
            //if (debug_ray)
            //{
            //    Debug.DrawRay(cam.position, cam.forward * step, Color.red, 100f);
            //}

            //检测
            if (world.GetBlockType(pos) != 4)
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


}