using Homebrew;
using MCEntity;
using UnityEngine;
using static MC_UtilityFunctions;

[RequireComponent(typeof(MC_Velocity_Component))]
public class MC_DebugEntity_Component : MonoBehaviour
{

    #region ״̬

    [Foldout("ʵʱ��ײ", true)]
    [ReadOnly] public bool Front;
    [ReadOnly] public bool Back;
    [ReadOnly] public bool Left;
    [ReadOnly] public bool Right;
    [ReadOnly] public bool Up;
    [ReadOnly] public bool Down;

    #endregion


    #region ���ں���

    MC_Velocity_Component Velocity_Component;
    MC_Collider_Component Collider_Component;

    private void Awake()
    {
        Velocity_Component = GetComponent<MC_Velocity_Component>();
        Collider_Component = GetComponent<MC_Collider_Component>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {

        if (Collider_Component.managerhub.world.game_state == Game_State.Playing)
        {
            _ReferUpdate_EntityJumpAndMoving();

            _ReferUpdate_RealCollision();

            _ReferUpdate_EntityControler();

            _ReferUpdate_LifeComponentTest();
        }

       
    }


    #endregion


    #region ʵʱ��ײ

    void _ReferUpdate_RealCollision()
    {
        Front = Collider_Component.collider_Front;
        Back = Collider_Component.collider_Back;
        Left = Collider_Component.collider_Left;
        Right = Collider_Component.collider_Right;
        Up = Collider_Component.collider_Up;
        Down = Collider_Component.collider_Down;
    }

    #endregion


    #region ʵ����Ծ������ƶ�

    [Foldout("ʵ����Ծ", true)]
    [Header("��Ծ����")] public Vector3 Jump_Direct;
    [Header("��Ծ����")] public float Jump_Value;
    [Header("��Ծһ��")] public bool Toggle_EntityOnceJump;

    [Foldout("ʵ���ƶ�", true)]
    [Header("���ѡ��뾶")] public float Debug_radious = 5f;
    [Header("�ƶ�һ��")] public bool Toggle_EntityOnceMove;

    private void _ReferUpdate_EntityJumpAndMoving()
    {
        if (Toggle_EntityOnceJump)
        {
            Velocity_Component.AddForce(Jump_Direct, Jump_Value);
            Toggle_EntityOnceJump = false;
        }

        if (Toggle_EntityOnceMove)
        {
            Velocity_Component.MoveToObject(MC_UtilityFunctions.GetRandomPointInCircle(transform.position, Debug_radious));
            Toggle_EntityOnceMove = false;
        }
    }

    #endregion


    #region ��ʱ�ӹ�С����

    [Foldout("�ӹܿ���", true)]
    [Header("��ʱ�ӹ�С����--[IJKL]�ƶ���[?]��Ծ")] public bool EntityControlerEnable;
    [Header("��ת���ٶ�")] public float RotationSpeed = 0.3f;

    void _ReferUpdate_EntityControler()
    {
        if (EntityControlerEnable)
        {
            EntityControler();
        }
    }

    void EntityControler()
    {
        // ʹ��С���̿���ʵ�巽��
        
        // ������벢����x��z������ٶ�
        if (Input.GetKey(KeyCode.I)) // ��ǰ
        {
            Velocity_Component.SetVelocity(BlockDirection.ǰ, Velocity_Component.speed_move);
        }
        else if (Input.GetKey(KeyCode.K)) // ���
        {
            Velocity_Component.SetVelocity(BlockDirection.��, Velocity_Component.speed_move);
        }

        if (Input.GetKey(KeyCode.J)) // ����
        {
            Velocity_Component.SetVelocity(BlockDirection.��, Velocity_Component.speed_move);
        }
        else if (Input.GetKey(KeyCode.L)) // ����
        {
            Velocity_Component.SetVelocity(BlockDirection.��, Velocity_Component.speed_move);
        }

        // ʹ��RightShift������Ծ
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            Velocity_Component.EntityJump();
        }

        // ���� < ������ת��> ������ת
        // �������ͬʱ��ס���򲻴�����ת
        if (Input.GetKey(KeyCode.N) && !Input.GetKey(KeyCode.M))
        {
            // ������ת
            Velocity_Component.EntityRotation(-RotationSpeed, 0);
        }
        else if (Input.GetKey(KeyCode.M) && !Input.GetKey(KeyCode.N))
        {
            // ������ת
            Velocity_Component.EntityRotation(RotationSpeed, 0);
        }


    }


    #endregion


    #region Life�������

    [Foldout("Life�������", true)]
    [Header("���µ���ֵ")] public int UpdateValue = -1;
    [Header("���±仯Ѫ��")] public bool Toggle_UpdateBlood;

    [Header("�趨��Ѫ��")] public int SetValue = 20;
    [Header("�����趨Ѫ��")] public bool Toggle_UpdateSetBlood;

    MC_Life_Component life_Component;


    void _ReferUpdate_LifeComponentTest()
    {
        if (Toggle_UpdateBlood)
        {

            if (life_Component == null)
            {
                life_Component = GetComponent<MC_Life_Component>();
            }

            life_Component.UpdateEntityLife(UpdateValue, Vector3.back);

            print($"���º�Ѫ��: {life_Component.EntityBlood}");

            Toggle_UpdateBlood = false;
        }


        if (Toggle_UpdateSetBlood)
        {
            if (life_Component == null)
            {
                life_Component = GetComponent<MC_Life_Component>();
            }

            life_Component.SetEntityBlood(SetValue);

            print($"�趨Ѫ��: {life_Component.EntityBlood}");

            Toggle_UpdateSetBlood = false;
        }


    }

    #endregion

}
