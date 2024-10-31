using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MC_Velocity_Component))]
public class MC_DebugEntity_Component : MonoBehaviour
{
    [Foldout("ʵʱ��ײ", true)]
    public bool Front;
    public bool Back;
    public bool Left;
    public bool Right;
    public bool Up;
    public bool Down;

    [Foldout("ʵ����Ծ", true)]
    [Header("��Ծ����")] public Vector3 Jump_Direct; 
    [Header("��Ծ����")] public float Jump_Value; 
    [Header("��Ծһ��")] public bool Toggle_EntityOnceJump;

    [Foldout("ʵ���ƶ�", true)]
    [Header("���ѡ��뾶")] public float Debug_radious = 5f;
    [Header("�ƶ�һ��")] public bool Toggle_EntityOnceMove;


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
        UpdateDebug();

        Front = Collider_Component.collider_Front;
        Back = Collider_Component.collider_Back;
        Left = Collider_Component.collider_Left;
        Right = Collider_Component.collider_Right;
        Up = Collider_Component.collider_Up;
        Down = Collider_Component.collider_Down;
    }


    #endregion




    private void UpdateDebug()
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
}
