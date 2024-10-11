using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public class Entity : MonoBehaviour
{
    
    [Header("状态")]
    public ManagerHub managerhub;
    public bool debug_ShowHitBox;
    public bool debug_ShowCollisionBox;
    public bool jumpRequest;
    public bool _JumpToPlayer;
    [ReadOnly]public bool isGrounded;


    public void JumpToPlayer(float _Force)
    {
        // Calculate direction from this entity to the player
        Vector3 direction = managerhub.player.transform.position - transform.position;

        // Normalize the direction vector to keep the jump force consistent
        direction.Normalize();

        // Apply the jump force in the direction towards the player
        // Assuming you want to maintain the current Y momentum for the jump
        momentum = new Vector3(direction.x * _Force, jumpForce, direction.z * _Force);

        // Set jumpRequest to true to trigger the jump in AchieveVelocity()
        jumpRequest = true;
        _JumpToPlayer = false;
    }




    #region 构造函数和周期函数

    public void Initialize()
    {
        managerhub = GameObject.Find("Manager/ManagerHub").GetComponent<ManagerHub>();
    }

    Coroutine jumptopleyerr;
    IEnumerator alwaysJumpToPlayer()
    {
        while (true)
        {
            // 在 0.05 到 0.15 的范围内生成随机跳跃力度
            float _jump = Random.Range(0.05f, 0.15f);

            // 在 3 到 7 秒的范围内生成随机等待时间
            float _time = Random.Range(3f, 7f);

            JumpToPlayer(_jump);

            yield return new WaitForSeconds(_time);
        }
    }

    public void Update()
    {
        if (managerhub.world.game_state == Game_State.Playing)
        {
            if (jumptopleyerr == null)
            {
                jumptopleyerr = StartCoroutine(alwaysJumpToPlayer());
            }

            update_block();

            CalculateVelocity();
            AchieveVelocity();

            if (debug_ShowHitBox)
            {
                Draw_HitBox();
            }

            if (debug_ShowCollisionBox)
            {
                Draw_CollisionBox();
            }

        }
    }

    #endregion


    #region 碰撞系统

    [Header("碰撞参数")]
    public float eyesHight = 1.7f;
    public float playerWidth = 0.3f;
    public float playerHeight = 1.9f;
    private float high_delta = 0.7f; // 在确定高度下，碰撞点的相对高度
    private float extend_delta = 0.1f;
    private float delta = 0.05f;


    #region 碰撞点
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
    //Center
    Vector3 front_Center = new Vector3();
    Vector3 back_Center = new Vector3();
    Vector3 left_Center = new Vector3();
    Vector3 right_Center = new Vector3();
    #endregion


    #region 碰撞检测

    private float checkDownSpeed(float downSpeed)
    {

        if (
            managerhub.world.CollisionCheckForVoxel(down_左上) ||
            managerhub.world.CollisionCheckForVoxel(down_右上) ||
            managerhub.world.CollisionCheckForVoxel(down_左下) ||
            managerhub.world.CollisionCheckForVoxel(down_右下)

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
    private float checkUpSpeed(float upSpeed)
    {

        if (
            managerhub.world.CollisionCheckForVoxel(up_左上) ||
            managerhub.world.CollisionCheckForVoxel(up_右上) ||
            managerhub.world.CollisionCheckForVoxel(up_左下) ||
            managerhub.world.CollisionCheckForVoxel(up_右下)
           )
        {

            return 0;

        }
        else
        {

            return upSpeed;

        }



    }
    public bool front
    {

        get
        {
            //如果managerhub.world返回true，则碰撞
            if (managerhub.world.CollisionCheckForVoxel(front_左上) ||
                managerhub.world.CollisionCheckForVoxel(front_右上) ||
                managerhub.world.CollisionCheckForVoxel(front_左下) ||
                managerhub.world.CollisionCheckForVoxel(front_右下) ||
                managerhub.world.CollisionCheckForVoxel(front_Center))
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
                managerhub.world.CollisionCheckForVoxel(back_左上) ||
                managerhub.world.CollisionCheckForVoxel(back_右上) ||
                managerhub.world.CollisionCheckForVoxel(back_左下) ||
                managerhub.world.CollisionCheckForVoxel(back_右下) ||
                managerhub.world.CollisionCheckForVoxel(back_Center)
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
                managerhub.world.CollisionCheckForVoxel(left_左上) ||
                managerhub.world.CollisionCheckForVoxel(left_右上) ||
                managerhub.world.CollisionCheckForVoxel(left_左下) ||
                managerhub.world.CollisionCheckForVoxel(left_右下) ||
                managerhub.world.CollisionCheckForVoxel(left_Center)
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
                managerhub.world.CollisionCheckForVoxel(right_左上) ||
                managerhub.world.CollisionCheckForVoxel(right_右上) ||
                managerhub.world.CollisionCheckForVoxel(right_左下) ||
                managerhub.world.CollisionCheckForVoxel(right_右下) ||
                managerhub.world.CollisionCheckForVoxel(right_Center)
                )
                return true;

            else
                return false;


        }

    }

    #endregion

    //更新碰撞点
    void update_block()
    {
        Vector3 _selfPos = transform.position;

        // 上面的四个点
        if (momentum.y > 0)
        {
            up_左上 = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2));
            up_右上 = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2));
            up_右下 = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2));
            up_左下 = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2));
        }

        // 下面的四个点
        down_左上 = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2));
        down_右上 = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2));
        down_右下 = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2));
        down_左下 = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2));

        //front
        if (momentum.z > 0)
        {
            front_Center = new Vector3(_selfPos.x, _selfPos.y, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_左上 = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_右上 = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_左下 = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_右下 = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);

        }



        //back
        if (momentum.z < 0)
        {
            back_Center = new Vector3(_selfPos.x, _selfPos.y, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_左上 = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_右上 = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_左下 = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_右下 = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);

        }


        //left
        if (momentum.x < 0)
        {
            left_Center = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y, _selfPos.z);
            left_左上 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) + delta);
            left_右上 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) - delta);
            left_左下 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) + delta);
            left_右下 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) - delta);


        }

        //right
        if (momentum.x > 0)
        {
            right_Center = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y, _selfPos.z);
            right_左上 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) - delta);
            right_右上 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) + delta);
            right_左下 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) - delta);
            right_右下 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) + delta);

        }


    }


    //绘制判定箱
    void Draw_HitBox()
    {
        Vector3 _selfPos = transform.position;
        Vector3 _eyesPos = new Vector3(_selfPos.x, _selfPos.y + eyesHight, _selfPos.z);

        Vector3 _front_左上 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _front_右上 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _front_左下 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _front_右下 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);

        Vector3 _back_左上 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _back_右上 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _back_左下 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _back_右下 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);

        Vector3 _eyes_左上 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _eyesPos.y, _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _eyes_右上 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _eyesPos.y, _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _eyes_左下 = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _eyesPos.y, _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _eyes_右下 = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _eyesPos.y, _selfPos.z - (playerWidth / 2) - extend_delta);

        //头顶正方形
        Debug.DrawLine(_front_左上, _front_右上, Color.white);
        Debug.DrawLine(_front_右上, _back_右上, Color.white);
        Debug.DrawLine(_back_右上, _back_左上, Color.white);
        Debug.DrawLine(_back_左上, _front_左上, Color.white);

        //脚底正方形
        Debug.DrawLine(_front_左下, _front_右下, Color.white);
        Debug.DrawLine(_front_右下, _back_右下, Color.white);
        Debug.DrawLine(_back_右下, _back_左下, Color.white);
        Debug.DrawLine(_back_左下, _front_左下, Color.white);

        //四个竖线
        Debug.DrawLine(_front_左上, _front_左下, Color.white);
        Debug.DrawLine(_front_右上, _front_右下, Color.white);
        Debug.DrawLine(_back_左上, _back_左下, Color.white);
        Debug.DrawLine(_back_右上, _back_右下, Color.white);

        //眼睛
        Debug.DrawLine(_eyes_左上, _eyes_右上, Color.red);
        Debug.DrawLine(_eyes_右上, _eyes_右下, Color.red);
        Debug.DrawLine(_eyes_右下, _eyes_左下, Color.red);
        Debug.DrawLine(_eyes_左下, _eyes_左上, Color.red);
    }

    void Draw_CollisionBox()
    {


        // 上面的四条线
        Debug.DrawLine(up_左上, up_右上, Color.red); // 左上 -- 右上
        Debug.DrawLine(up_右上, up_右下, Color.red); // 右上 -- 右下
        Debug.DrawLine(up_右下, up_左下, Color.red); // 右下 -- 左下
        Debug.DrawLine(up_左下, up_左上, Color.red); // 左下 -- 左上

        // 下面的四条线
        Debug.DrawLine(down_左上, down_右上, Color.blue); // 左上 -- 右上
        Debug.DrawLine(down_右上, down_右下, Color.blue); // 右上 -- 右下
        Debug.DrawLine(down_右下, down_左下, Color.blue); // 右下 -- 左下
        Debug.DrawLine(down_左下, down_左上, Color.blue); // 左下 -- 左上


        //上半圈
        Debug.DrawLine(front_左上, front_右上, Color.yellow);
        Debug.DrawLine(front_右上, right_左上, Color.red);
        Debug.DrawLine(right_左上, right_右上, Color.yellow);
        Debug.DrawLine(right_右上, back_右上, Color.red);
        Debug.DrawLine(back_右上, back_左上, Color.yellow);
        Debug.DrawLine(back_左上, left_左上, Color.red);
        Debug.DrawLine(left_左上, left_右上, Color.yellow);
        Debug.DrawLine(left_右上, front_左上, Color.red);


        //中心碰撞点
        Debug.DrawLine(front_Center, right_Center, Color.green);
        Debug.DrawLine(right_Center, back_Center, Color.green);
        Debug.DrawLine(back_Center, left_Center, Color.green);
        Debug.DrawLine(left_Center, front_Center, Color.green);


        //下半圈
        Debug.DrawLine(front_左下, front_右下, Color.yellow);
        Debug.DrawLine(front_右下, right_左下, Color.red);
        Debug.DrawLine(right_左下, right_右下, Color.yellow);
        Debug.DrawLine(right_右下, back_右下, Color.red);
        Debug.DrawLine(back_右下, back_左下, Color.yellow);
        Debug.DrawLine(back_左下, left_左下, Color.red);
        Debug.DrawLine(left_左下, left_右下, Color.yellow);
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
        Debug.DrawLine(front_左上, front_左下, Color.yellow);
        Debug.DrawLine(front_右上, front_右下, Color.yellow);
        //back
        Debug.DrawLine(back_左上, back_左下, Color.yellow);
        Debug.DrawLine(back_右上, back_右下, Color.yellow);
        //left
        Debug.DrawLine(left_左上, left_左下, Color.yellow);
        Debug.DrawLine(left_右上, left_右下, Color.yellow);
        //right
        Debug.DrawLine(right_左上, right_左下, Color.yellow);
        Debug.DrawLine(right_右上, right_右下, Color.yellow);

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



    #endregion


    #region 生命系统

    [Header("生命参数")]
    public int blood = 20;

    #endregion


    #region 动量系统

    [Header("动量参数")]
    public Vector3 velocity;
    public Vector3 momentum;
    public float jumpForce = 5f;
    public float gravity = -9.8f;
    public float ground_V = 1f; //地面阻力
    public float gravity_V = 3.8f; //空气阻力


    private void CalculateVelocity()
    {
        // 重力
        if (momentum.y > gravity)
            momentum.y += Time.deltaTime * gravity * gravity_V;

        // 速度
        velocity += momentum * Time.deltaTime;

        // 滑膜计算
        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;

        // 当实体落地时，逐步应用地面阻力，减小水平速度
        if (isGrounded)
        {
            // 使用 ground_V 来逐步减小 X 和 Z 方向的速度
            velocity.x = Mathf.MoveTowards(velocity.x, 0, ground_V * Time.deltaTime);
            velocity.z = Mathf.MoveTowards(velocity.z, 0, ground_V * Time.deltaTime);
        }

        // Y方向的判定
        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y); // 向下移动，检查是否碰到地面
        else if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);   // 向上移动，检查是否碰到顶部
    }

    private void AchieveVelocity()
    {
        if (jumpRequest)
        {
            momentum.y = jumpForce;

            isGrounded = false;
            jumpRequest = false;
        }

        // 根据计算出的速度移动实体
        transform.Translate(velocity, Space.World);
    }


    #endregion




}
