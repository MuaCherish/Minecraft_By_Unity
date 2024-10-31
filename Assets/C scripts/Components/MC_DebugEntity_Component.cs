using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MC_Velocity_Component))]
public class MC_DebugEntity_Component : MonoBehaviour
{
    [Foldout("实时碰撞", true)]
    public bool Front;
    public bool Back;
    public bool Left;
    public bool Right;
    public bool Up;
    public bool Down;

    [Foldout("实体跳跃", true)]
    [Header("跳跃方向")] public Vector3 Jump_Direct; 
    [Header("跳跃力度")] public float Jump_Value; 
    [Header("跳跃一次")] public bool Toggle_EntityOnceJump;

    [Foldout("实体移动", true)]
    [Header("随机选点半径")] public float Debug_radious = 5f;
    [Header("移动一次")] public bool Toggle_EntityOnceMove;


    #region 周期函数

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
