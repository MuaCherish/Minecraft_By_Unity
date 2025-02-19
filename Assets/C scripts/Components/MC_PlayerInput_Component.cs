using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MC_Velocity_Component))]
[RequireComponent(typeof(MC_Collider_Component))]
public class MC_PlayerInput_Component : MonoBehaviour
{
    #region 周期函数

    MC_Velocity_Component Velocity_Component;
    MC_Collider_Component Collider_Component;
    World world;

    private void Awake()
    {
        world = SceneData.GetWorld();
        Velocity_Component = GetComponent<MC_Velocity_Component>();
        Collider_Component = GetComponent<MC_Collider_Component>();
    }

    private void Start()
    {
        _ReferStart_ComponentInit();
    }

    private void Update()
    {
        if (world.game_state == Game_State.Playing)
        {
            GetInput();
            CaculateInput();
        }
    }

    private void FixedUpdate()
    {
        if (world.game_state == Game_State.Playing)
        {
            ApplyInput();
        }
    }

    #endregion


    #region 键盘控制

    [Foldout("键盘控制", true)]
    [Header("实体水平旋转灵敏度")] public float RotationHorizonSensitivity = 200f; // 设置旋转灵敏度
    [Header("实体垂直旋转灵敏度")] public float RotationVerticleSensitivity = 200f; // 设置旋转灵敏度

    [Foldout("视角限制", true)]
    [Header("摄像机限制范围")] public Vector2 CameraLimitRange = new Vector2(-90, 90);
    [Header("实体头限制范围")] public Vector2 HeadLimitRange = new Vector2(-20, 20);

    // 控制输入的相关变量
    private float currentHorizontalInput;
    private float currentVerticalInput;

    // 鼠标输入的相关变量
    private float mouseHorizonSpeed;
    public float mouseVerticalSpeed;

    // 计算出来的移动方向
    private Vector3 moveDirection;

    // 如果有AI组件则禁用
    void _ReferStart_ComponentInit()
    {
        //禁用AI组件
        MC_AI_Component AI_Component = GetComponentInParent<MC_AI_Component>();
        if (AI_Component != null)
            AI_Component.enabled = false;

        //将ModelHead等模型旋转归零
        Collider_Component.Model.transform.rotation = Quaternion.Euler(0, 0, 0);
        Collider_Component.Head.transform.rotation = Quaternion.Euler(0, 0, 0);
        Collider_Component.Body.transform.rotation = Quaternion.Euler(0, 0, 0);

    }

    // 获取键盘和鼠标输入
    void GetInput()
    {
        // 获取键盘输入
        currentHorizontalInput = Input.GetAxis("Horizontal");
        currentVerticalInput = Input.GetAxis("Vertical");

        // 获取鼠标输入（X轴旋转和Y轴视角）
        mouseHorizonSpeed = Input.GetAxis("Mouse X");
        mouseVerticalSpeed = Input.GetAxis("Mouse Y");
    }

    // 计算输入（这里可以根据需要添加额外的处理逻辑）
    void CaculateInput()
    {
        // 获取Model的朝向
        GameObject _Model = Collider_Component.Model;

        // 将水平输入方向转换为Vector3，根据Model的朝向进行计算
        moveDirection = (_Model.transform.forward * currentVerticalInput + _Model.transform.right * currentHorizontalInput).normalized;

        // 限制鼠标输入的垂直值（防止上下视角超出范围）
        mouseVerticalSpeed = Mathf.Clamp(mouseVerticalSpeed, -90f, 90f);
    }

    void ApplyInput()
    {
        // 设定Velocity
        Velocity_Component.SetVelocity("x", moveDirection.x * Velocity_Component.speed_move);
        Velocity_Component.SetVelocity("z", moveDirection.z * Velocity_Component.speed_move);

        // 设定左右转向
        Velocity_Component.EntityRotation(mouseHorizonSpeed * RotationHorizonSensitivity);

        // 设定上下转向
        Velocity_Component.EntityHeadVerticleRotation(mouseVerticalSpeed * RotationVerticleSensitivity, CameraLimitRange, HeadLimitRange);
    }

    #endregion
}
