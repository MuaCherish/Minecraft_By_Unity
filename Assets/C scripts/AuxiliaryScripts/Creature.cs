using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Creature : MonoBehaviour
{
    [Header("�������")]
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
            //������ҽ�������
            Update_FootBlockType();

            //������ײ��
            update_block();
        }
    }

    #region ��ײ���
    //����16����ײ��
    void update_block()
    {

        // ������ĸ���
        CreatureValue.up_���� = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2), transform.position.y + (CreatureValue.playerHeight / 2), transform.position.z + (CreatureValue.playerWidth / 2));
        CreatureValue.up_���� = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2), transform.position.y + (CreatureValue.playerHeight / 2), transform.position.z + (CreatureValue.playerWidth / 2));
        CreatureValue.up_���� = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2), transform.position.y + (CreatureValue.playerHeight / 2), transform.position.z - (CreatureValue.playerWidth / 2));
        CreatureValue.up_���� = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2), transform.position.y + (CreatureValue.playerHeight / 2), transform.position.z - (CreatureValue.playerWidth / 2));

        // ������ĸ���
        CreatureValue.down_���� = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2), transform.position.y - (CreatureValue.playerHeight / 2), transform.position.z + (CreatureValue.playerWidth / 2));
        CreatureValue.down_���� = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2), transform.position.y - (CreatureValue.playerHeight / 2), transform.position.z + (CreatureValue.playerWidth / 2));
        CreatureValue.down_���� = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2), transform.position.y - (CreatureValue.playerHeight / 2), transform.position.z - (CreatureValue.playerWidth / 2));
        CreatureValue.down_���� = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2), transform.position.y - (CreatureValue.playerHeight / 2), transform.position.z - (CreatureValue.playerWidth / 2));


        //front
        CreatureValue.front_���� = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) + CreatureValue.delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta);
        CreatureValue.front_���� = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) - CreatureValue.delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta);
        CreatureValue.front_���� = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) + CreatureValue.delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta);
        CreatureValue.front_���� = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) - CreatureValue.delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta);


        //back
        CreatureValue.back_���� = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) + CreatureValue.delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta);
        CreatureValue.back_���� = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) - CreatureValue.delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta);
        CreatureValue.back_���� = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) + CreatureValue.delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta);
        CreatureValue.back_���� = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) - CreatureValue.delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta);


        //left
        CreatureValue.left_���� = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) + CreatureValue.delta);
        CreatureValue.left_���� = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) - CreatureValue.delta);
        CreatureValue.left_���� = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) + CreatureValue.delta);
        CreatureValue.left_���� = new Vector3(transform.position.x - (CreatureValue.playerWidth / 2) - CreatureValue.extend_delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) - CreatureValue.delta);


        //right
        CreatureValue.right_���� = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) - CreatureValue.delta);
        CreatureValue.right_���� = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta, transform.position.y + (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) + CreatureValue.delta);
        CreatureValue.right_���� = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z + (CreatureValue.playerWidth / 2) - CreatureValue.delta);
        CreatureValue.right_���� = new Vector3(transform.position.x + (CreatureValue.playerWidth / 2) + CreatureValue.extend_delta, transform.position.y - (CreatureValue.playerHeight / 4), transform.position.z - (CreatureValue.playerWidth / 2) + CreatureValue.delta);


    }

    //��ײ��⣨���£�
    private float checkDownSpeed(float downSpeed)
    {
        if (
        CreatureValue.world.CheckForVoxel(CreatureValue.down_����) ||
        CreatureValue.world.CheckForVoxel(CreatureValue.down_����) ||
            CreatureValue.world.CheckForVoxel(CreatureValue.down_����) ||
            CreatureValue.world.CheckForVoxel(CreatureValue.down_����)

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

    //��ײ��⣨ͷ�ϣ�
    private float checkUpSpeed(float upSpeed)
    {
        if (
            CreatureValue.world.CheckForVoxel(CreatureValue.up_����) ||
            CreatureValue.world.CheckForVoxel(CreatureValue.up_����) ||
            CreatureValue.world.CheckForVoxel(CreatureValue.up_����) ||
            CreatureValue.world.CheckForVoxel(CreatureValue.up_����)
           )
        {
            return 0;

        }
        else
        {
            return upSpeed;

        }



    }

    //��ײ����ļ�⣨-Z����Ϊfront��
    public bool front
    {

        get
        {

            if (
                CreatureValue.world.CheckForVoxel(CreatureValue.front_����) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.front_����) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.front_����) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.front_����)
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
                CreatureValue.world.CheckForVoxel(CreatureValue.back_����) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.back_����) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.back_����) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.back_����)
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
                CreatureValue.world.CheckForVoxel(CreatureValue.left_����) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.left_����) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.left_����) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.left_����)
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
                CreatureValue.world.CheckForVoxel(CreatureValue.right_����) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.right_����) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.right_����) ||
                CreatureValue.world.CheckForVoxel(CreatureValue.right_����)
                )
                return true;
            
            else
                return false;
        }
    }

    //���½��·�������
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

    [Header("����״̬")]
    public byte foot_BlockType = VoxelData.Air;
    public bool isGrounded;
    public bool isSwiming;
    public bool isMoving;
    public bool jumpRequest;

    [Header("�������")]
    [HideInInspector] public Vector3 velocity;
    public float walkSpeed = 4f;
    public float jumpForce = 6f;
    public float gravity = -15f;
    public float MaxHurtHigh = 7f;

    [Header("��ײ����")]
    public float playerWidth = 0.3f;
    public float playerHeight = 1.7f;
    [HideInInspector] public float extend_delta = 0.1f;
    [HideInInspector] public float delta = 0.05f;

    //��ײ��������
    // ������ĸ���
    [HideInInspector] public Vector3 up_���� = new Vector3();
    [HideInInspector] public Vector3 up_���� = new Vector3();
    [HideInInspector] public Vector3 up_���� = new Vector3();
    [HideInInspector] public Vector3 up_���� = new Vector3();

    // ������ĸ���
    [HideInInspector] public Vector3 down_���� = new Vector3();
    [HideInInspector] public Vector3 down_���� = new Vector3();
    [HideInInspector] public Vector3 down_���� = new Vector3();
    [HideInInspector] public Vector3 down_���� = new Vector3();

    //front
    [HideInInspector] public Vector3 front_���� = new Vector3();
    [HideInInspector] public Vector3 front_���� = new Vector3();
    [HideInInspector] public Vector3 front_���� = new Vector3();
    [HideInInspector] public Vector3 front_���� = new Vector3();

    //back
    [HideInInspector] public Vector3 back_���� = new Vector3();
    [HideInInspector] public Vector3 back_���� = new Vector3();
    [HideInInspector] public Vector3 back_���� = new Vector3();
    [HideInInspector] public Vector3 back_���� = new Vector3();

    //left
    [HideInInspector] public Vector3 left_���� = new Vector3();
    [HideInInspector] public Vector3 left_���� = new Vector3();
    [HideInInspector] public Vector3 left_���� = new Vector3();
    [HideInInspector] public Vector3 left_���� = new Vector3();

    //right
    [HideInInspector] public Vector3 right_���� = new Vector3();
    [HideInInspector] public Vector3 right_���� = new Vector3();
    [HideInInspector] public Vector3 right_���� = new Vector3();
    [HideInInspector] public Vector3 right_���� = new Vector3();
}