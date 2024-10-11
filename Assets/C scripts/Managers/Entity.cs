using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public class Entity : MonoBehaviour
{
    
    [Header("״̬")]
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




    #region ���캯�������ں���

    public void Initialize()
    {
        managerhub = GameObject.Find("Manager/ManagerHub").GetComponent<ManagerHub>();
    }

    Coroutine jumptopleyerr;
    IEnumerator alwaysJumpToPlayer()
    {
        while (true)
        {
            // �� 0.05 �� 0.15 �ķ�Χ�����������Ծ����
            float _jump = Random.Range(0.05f, 0.15f);

            // �� 3 �� 7 ��ķ�Χ����������ȴ�ʱ��
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


    #region ��ײϵͳ

    [Header("��ײ����")]
    public float eyesHight = 1.7f;
    public float playerWidth = 0.3f;
    public float playerHeight = 1.9f;
    private float high_delta = 0.7f; // ��ȷ���߶��£���ײ�����Ը߶�
    private float extend_delta = 0.1f;
    private float delta = 0.05f;


    #region ��ײ��
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
    //Center
    Vector3 front_Center = new Vector3();
    Vector3 back_Center = new Vector3();
    Vector3 left_Center = new Vector3();
    Vector3 right_Center = new Vector3();
    #endregion


    #region ��ײ���

    private float checkDownSpeed(float downSpeed)
    {

        if (
            managerhub.world.CollisionCheckForVoxel(down_����) ||
            managerhub.world.CollisionCheckForVoxel(down_����) ||
            managerhub.world.CollisionCheckForVoxel(down_����) ||
            managerhub.world.CollisionCheckForVoxel(down_����)

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
            managerhub.world.CollisionCheckForVoxel(up_����) ||
            managerhub.world.CollisionCheckForVoxel(up_����) ||
            managerhub.world.CollisionCheckForVoxel(up_����) ||
            managerhub.world.CollisionCheckForVoxel(up_����)
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
            //���managerhub.world����true������ײ
            if (managerhub.world.CollisionCheckForVoxel(front_����) ||
                managerhub.world.CollisionCheckForVoxel(front_����) ||
                managerhub.world.CollisionCheckForVoxel(front_����) ||
                managerhub.world.CollisionCheckForVoxel(front_����) ||
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
                managerhub.world.CollisionCheckForVoxel(back_����) ||
                managerhub.world.CollisionCheckForVoxel(back_����) ||
                managerhub.world.CollisionCheckForVoxel(back_����) ||
                managerhub.world.CollisionCheckForVoxel(back_����) ||
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
                managerhub.world.CollisionCheckForVoxel(left_����) ||
                managerhub.world.CollisionCheckForVoxel(left_����) ||
                managerhub.world.CollisionCheckForVoxel(left_����) ||
                managerhub.world.CollisionCheckForVoxel(left_����) ||
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
                managerhub.world.CollisionCheckForVoxel(right_����) ||
                managerhub.world.CollisionCheckForVoxel(right_����) ||
                managerhub.world.CollisionCheckForVoxel(right_����) ||
                managerhub.world.CollisionCheckForVoxel(right_����) ||
                managerhub.world.CollisionCheckForVoxel(right_Center)
                )
                return true;

            else
                return false;


        }

    }

    #endregion

    //������ײ��
    void update_block()
    {
        Vector3 _selfPos = transform.position;

        // ������ĸ���
        if (momentum.y > 0)
        {
            up_���� = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2));
            up_���� = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2));
            up_���� = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2));
            up_���� = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2));
        }

        // ������ĸ���
        down_���� = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2));
        down_���� = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2));
        down_���� = new Vector3(_selfPos.x + (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2));
        down_���� = new Vector3(_selfPos.x - (playerWidth / 2), _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2));

        //front
        if (momentum.z > 0)
        {
            front_Center = new Vector3(_selfPos.x, _selfPos.y, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_���� = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_���� = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_���� = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);
            front_���� = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) + extend_delta);

        }



        //back
        if (momentum.z < 0)
        {
            back_Center = new Vector3(_selfPos.x, _selfPos.y, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_���� = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_���� = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_���� = new Vector3(_selfPos.x - (playerWidth / 2) + delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);
            back_���� = new Vector3(_selfPos.x + (playerWidth / 2) - delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) - extend_delta);

        }


        //left
        if (momentum.x < 0)
        {
            left_Center = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y, _selfPos.z);
            left_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) + delta);
            left_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) - delta);
            left_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) + delta);
            left_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) - delta);


        }

        //right
        if (momentum.x > 0)
        {
            right_Center = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y, _selfPos.z);
            right_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + high_delta, _selfPos.z + (playerWidth / 2) - delta);
            right_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + high_delta, _selfPos.z - (playerWidth / 2) + delta);
            right_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - high_delta, _selfPos.z + (playerWidth / 2) - delta);
            right_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - high_delta, _selfPos.z - (playerWidth / 2) + delta);

        }


    }


    //�����ж���
    void Draw_HitBox()
    {
        Vector3 _selfPos = transform.position;
        Vector3 _eyesPos = new Vector3(_selfPos.x, _selfPos.y + eyesHight, _selfPos.z);

        Vector3 _front_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _front_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _front_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _front_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z + (playerWidth / 2) + extend_delta);

        Vector3 _back_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _back_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y + (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _back_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _back_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _selfPos.y - (playerHeight / 2), _selfPos.z - (playerWidth / 2) - extend_delta);

        Vector3 _eyes_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _eyesPos.y, _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _eyes_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _eyesPos.y, _selfPos.z + (playerWidth / 2) + extend_delta);
        Vector3 _eyes_���� = new Vector3(_selfPos.x - (playerWidth / 2) - extend_delta, _eyesPos.y, _selfPos.z - (playerWidth / 2) - extend_delta);
        Vector3 _eyes_���� = new Vector3(_selfPos.x + (playerWidth / 2) + extend_delta, _eyesPos.y, _selfPos.z - (playerWidth / 2) - extend_delta);

        //ͷ��������
        Debug.DrawLine(_front_����, _front_����, Color.white);
        Debug.DrawLine(_front_����, _back_����, Color.white);
        Debug.DrawLine(_back_����, _back_����, Color.white);
        Debug.DrawLine(_back_����, _front_����, Color.white);

        //�ŵ�������
        Debug.DrawLine(_front_����, _front_����, Color.white);
        Debug.DrawLine(_front_����, _back_����, Color.white);
        Debug.DrawLine(_back_����, _back_����, Color.white);
        Debug.DrawLine(_back_����, _front_����, Color.white);

        //�ĸ�����
        Debug.DrawLine(_front_����, _front_����, Color.white);
        Debug.DrawLine(_front_����, _front_����, Color.white);
        Debug.DrawLine(_back_����, _back_����, Color.white);
        Debug.DrawLine(_back_����, _back_����, Color.white);

        //�۾�
        Debug.DrawLine(_eyes_����, _eyes_����, Color.red);
        Debug.DrawLine(_eyes_����, _eyes_����, Color.red);
        Debug.DrawLine(_eyes_����, _eyes_����, Color.red);
        Debug.DrawLine(_eyes_����, _eyes_����, Color.red);
    }

    void Draw_CollisionBox()
    {


        // �����������
        Debug.DrawLine(up_����, up_����, Color.red); // ���� -- ����
        Debug.DrawLine(up_����, up_����, Color.red); // ���� -- ����
        Debug.DrawLine(up_����, up_����, Color.red); // ���� -- ����
        Debug.DrawLine(up_����, up_����, Color.red); // ���� -- ����

        // �����������
        Debug.DrawLine(down_����, down_����, Color.blue); // ���� -- ����
        Debug.DrawLine(down_����, down_����, Color.blue); // ���� -- ����
        Debug.DrawLine(down_����, down_����, Color.blue); // ���� -- ����
        Debug.DrawLine(down_����, down_����, Color.blue); // ���� -- ����


        //�ϰ�Ȧ
        Debug.DrawLine(front_����, front_����, Color.yellow);
        Debug.DrawLine(front_����, right_����, Color.red);
        Debug.DrawLine(right_����, right_����, Color.yellow);
        Debug.DrawLine(right_����, back_����, Color.red);
        Debug.DrawLine(back_����, back_����, Color.yellow);
        Debug.DrawLine(back_����, left_����, Color.red);
        Debug.DrawLine(left_����, left_����, Color.yellow);
        Debug.DrawLine(left_����, front_����, Color.red);


        //������ײ��
        Debug.DrawLine(front_Center, right_Center, Color.green);
        Debug.DrawLine(right_Center, back_Center, Color.green);
        Debug.DrawLine(back_Center, left_Center, Color.green);
        Debug.DrawLine(left_Center, front_Center, Color.green);


        //�°�Ȧ
        Debug.DrawLine(front_����, front_����, Color.yellow);
        Debug.DrawLine(front_����, right_����, Color.red);
        Debug.DrawLine(right_����, right_����, Color.yellow);
        Debug.DrawLine(right_����, back_����, Color.red);
        Debug.DrawLine(back_����, back_����, Color.yellow);
        Debug.DrawLine(back_����, left_����, Color.red);
        Debug.DrawLine(left_����, left_����, Color.yellow);
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
        Debug.DrawLine(front_����, front_����, Color.yellow);
        Debug.DrawLine(front_����, front_����, Color.yellow);
        //back
        Debug.DrawLine(back_����, back_����, Color.yellow);
        Debug.DrawLine(back_����, back_����, Color.yellow);
        //left
        Debug.DrawLine(left_����, left_����, Color.yellow);
        Debug.DrawLine(left_����, left_����, Color.yellow);
        //right
        Debug.DrawLine(right_����, right_����, Color.yellow);
        Debug.DrawLine(right_����, right_����, Color.yellow);

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



    #endregion


    #region ����ϵͳ

    [Header("��������")]
    public int blood = 20;

    #endregion


    #region ����ϵͳ

    [Header("��������")]
    public Vector3 velocity;
    public Vector3 momentum;
    public float jumpForce = 5f;
    public float gravity = -9.8f;
    public float ground_V = 1f; //��������
    public float gravity_V = 3.8f; //��������


    private void CalculateVelocity()
    {
        // ����
        if (momentum.y > gravity)
            momentum.y += Time.deltaTime * gravity * gravity_V;

        // �ٶ�
        velocity += momentum * Time.deltaTime;

        // ��Ĥ����
        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;

        // ��ʵ�����ʱ����Ӧ�õ�����������Сˮƽ�ٶ�
        if (isGrounded)
        {
            // ʹ�� ground_V ���𲽼�С X �� Z ������ٶ�
            velocity.x = Mathf.MoveTowards(velocity.x, 0, ground_V * Time.deltaTime);
            velocity.z = Mathf.MoveTowards(velocity.z, 0, ground_V * Time.deltaTime);
        }

        // Y������ж�
        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y); // �����ƶ�������Ƿ���������
        else if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);   // �����ƶ�������Ƿ���������
    }

    private void AchieveVelocity()
    {
        if (jumpRequest)
        {
            momentum.y = jumpForce;

            isGrounded = false;
            jumpRequest = false;
        }

        // ���ݼ�������ٶ��ƶ�ʵ��
        transform.Translate(velocity, Space.World);
    }


    #endregion




}
