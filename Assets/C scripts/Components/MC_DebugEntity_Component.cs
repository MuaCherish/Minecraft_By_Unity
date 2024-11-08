using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        _ReferUpdate_EntityJumpAndMoving();

        _ReferUpdate_RealCollision();

        _ReferUpdate_EntityControler();
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
    [Header("��ת���ٶ�")] public float RotationSpeed = 1f;

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
            Velocity_Component.SetVelocity("z", Velocity_Component.speed_move);
        }
        else if (Input.GetKey(KeyCode.K)) // ���
        {
            Velocity_Component.SetVelocity("z", -Velocity_Component.speed_move);
        }

        if (Input.GetKey(KeyCode.J)) // ����
        {
            Velocity_Component.SetVelocity("x", -Velocity_Component.speed_move);
        }
        else if (Input.GetKey(KeyCode.L)) // ����
        {
            Velocity_Component.SetVelocity("x", Velocity_Component.speed_move);
        }

        // ʹ��RightShift������Ծ
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            Velocity_Component.EntityJump();
        }
    }


    #endregion

}
