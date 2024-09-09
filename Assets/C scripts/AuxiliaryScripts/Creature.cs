using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Creature : MonoBehaviour
{
    [Header("生物基类")]
    public CreatureValue CreatureValue;

    public void InitCreature(World _world, float _walkSpeed, float _jumpForce,float _playerWidth,float _playerHeight)
    {
        CreatureValue.world = _world;
        CreatureValue.walkSpeed = _walkSpeed;
        CreatureValue.jumpForce = _jumpForce;
        CreatureValue.playerWidth = _playerWidth;
        CreatureValue.playerHeight = _playerHeight;
    }

    private void FixedUpdate()
    {
        if (CreatureValue.world.game_state == Game_State.Playing)
        {
            //更新玩家脚下坐标
            Update_FootBlockType();

            //计算碰撞点
            update_block();
        }
    }

    #region 碰撞检测
    //更新16个碰撞点
    void update_block()
    {

        // 上面的四个点
        CreatureValue.up_左上 = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2), transform.position.y + (CreatureValue.playerHeight / 2), transform.position.z + (CreatureValue.playerWidth / 2));
        CreatureValue.up_右上 = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2), transform.position.y + (CreatureValue.playerHeight / 2), transform.position.z + (CreatureValue.playerWidth / 2));
        CreatureValue.up_右下 = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2), transform.position.y + (CreatureValue.playerHeight / 2), transform.position.z - (CreatureValue.playerWidth / 2));
        CreatureValue.up_左下 = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2), transform.position.y + (CreatureValue.playerHeight / 2), transform.position.z - (CreatureValue.playerWidth / 2));

        // 下面的四个点
        CreatureValue.down_左上 = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2), transform.position.y - (CreatureValue.playerHeight / 2), transform.position.z + (CreatureValue.playerWidth / 2));
        CreatureValue.down_右上 = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2), transform.position.y - (CreatureValue.playerHeight / 2), transform.position.z + (CreatureValue.playerWidth / 2));
        CreatureValue.down_右下 = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2), transform.position.y - (CreatureValue.playerHeight / 2), transform.position.z - (CreatureValue.playerWidth / 2));
        CreatureValue.down_左下 = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2), transform.position.y - (CreatureValue.playerHeight / 2), transform.position.z - (CreatureValue.playerWidth / 2));


        //front
        CreatureValue.front_左上 = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) + CreatureValue.delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta);
        CreatureValue.front_右上 = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) - CreatureValue.delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta);
        CreatureValue.front_左下 = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) + CreatureValue.delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta);
        CreatureValue.front_右下 = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) - CreatureValue.delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta);


        //back
        CreatureValue.back_左上 = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) + CreatureValue.delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta);
        CreatureValue.back_右上 = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) - CreatureValue.delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta);
        CreatureValue.back_左下 = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) + CreatureValue.delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta);
        CreatureValue.back_右下 = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) - CreatureValue.delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta);


        //left
        CreatureValue.left_左上 = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) + CreatureValue.delta);
        CreatureValue.left_右上 = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) - CreatureValue.delta);
        CreatureValue.left_左下 = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) + CreatureValue.delta);
        CreatureValue.left_右下 = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) - CreatureValue.delta);


        //right
        CreatureValue.right_左上 = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) - CreatureValue.delta);
        CreatureValue.right_右上 = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) + CreatureValue.delta);
        CreatureValue.right_左下 = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) - CreatureValue.delta);
        CreatureValue.right_右下 = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) + CreatureValue.delta);


    }

    //碰撞检测（脚下）
    private float checkDownSpeed(float downSpeed)
    {
        if (
        CreatureValue.world.CheckForVoxel(CreatureValue.down_左上) ||
        CreatureValue.world.CheckForVoxel(CreatureValue.down_右上) ||
            CreatureValue.world.CheckForVoxel(CreatureValue.down_左下) ||
            CreatureValue.world.CheckForVoxel(CreatureValue.down_右下)

            )
        {
            CreatureValue.isGrounded = true;
            return 0;

        }
        else
        {
            CreatureValue.isGrounded = false;
            return downSpeed;

        }



    }

    //碰撞检测（头上）
    private float checkUpSpeed(float upSpeed)
    {
        if (
            CreatureValue.world.CheckForVoxel(CreatureValue.up_左上) ||
            CreatureValue.world.CheckForVoxel(CreatureValue.up_右上) ||
            CreatureValue.world.CheckForVoxel(CreatureValue.up_左下) ||
            CreatureValue.world.CheckForVoxel(CreatureValue.up_右下)
           )
        {
            return 0;

        }
        else
        {
            return upSpeed;

        }



    }

    //碰撞方向的检测（-Z方向为front）
    public bool front
    {

        get
        {

            if (
                CreatureValue.world.CheckForVoxel(CreatureValue.front_左上) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.front_右上) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.front_左下) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.front_右下)
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
                CreatureValue.world.CheckForVoxel(CreatureValue.back_左上) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.back_右上) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.back_左下) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.back_右下)
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
                CreatureValue.world.CheckForVoxel(CreatureValue.left_左上) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.left_右上) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.left_左下) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.left_右下)
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
                CreatureValue.world.CheckForVoxel(CreatureValue.right_左上) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.right_右上) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.right_左下) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.right_右下)
                )
                return true;
            
            else
                return false;
        }
    }

    //更新脚下方块类型
    void Update_FootBlockType()
    {
        
    }
    #endregion
}


[System.Serializable]
public class CreatureValue
{
    [Header("Transforms")]
    public World world;

    [Header("生物状态")]
    public byte foot_BlockType = VoxelData.Air;
    public bool isGrounded;
    public bool isSwiming;
    public bool isMoving;
    public bool jumpRequest;

    [Header("生物参数")]
    [HideInInspector] public Vector3 velocity;
    public float walkSpeed = 4f;
    public float jumpForce = 6f;
    public float gravity = -15f;
    public float MaxHurtHigh = 7f;

    [Header("碰撞参数")]
    public float playerWidth = 0.3f;
    public float playerHeight = 1.7f;
    [HideInInspector] public float extend_delta = 0.1f;
    [HideInInspector] public float delta = 0.05f;

    //碰撞检测的坐标
    // 上面的四个点
    [HideInInspector] public Vector3 up_左上 = new Vector3();
    [HideInInspector] public Vector3 up_右上 = new Vector3();
    [HideInInspector] public Vector3 up_右下 = new Vector3();
    [HideInInspector] public Vector3 up_左下 = new Vector3();

    // 下面的四个点
    [HideInInspector] public Vector3 down_左上 = new Vector3();
    [HideInInspector] public Vector3 down_右上 = new Vector3();
    [HideInInspector] public Vector3 down_右下 = new Vector3();
    [HideInInspector] public Vector3 down_左下 = new Vector3();

    //front
    [HideInInspector] public Vector3 front_左上 = new Vector3();
    [HideInInspector] public Vector3 front_右上 = new Vector3();
    [HideInInspector] public Vector3 front_左下 = new Vector3();
    [HideInInspector] public Vector3 front_右下 = new Vector3();

    //back
    [HideInInspector] public Vector3 back_左上 = new Vector3();
    [HideInInspector] public Vector3 back_右上 = new Vector3();
    [HideInInspector] public Vector3 back_左下 = new Vector3();
    [HideInInspector] public Vector3 back_右下 = new Vector3();

    //left
    [HideInInspector] public Vector3 left_左上 = new Vector3();
    [HideInInspector] public Vector3 left_右上 = new Vector3();
    [HideInInspector] public Vector3 left_左下 = new Vector3();
    [HideInInspector] public Vector3 left_右下 = new Vector3();

    //right
    [HideInInspector] public Vector3 right_左上 = new Vector3();
    [HideInInspector] public Vector3 right_右上 = new Vector3();
    [HideInInspector] public Vector3 right_左下 = new Vector3();
    [HideInInspector] public Vector3 right_右下 = new Vector3();
}