using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MC_Velocity_Component))]
[RequireComponent(typeof(MC_Collider_Component))]
public class MC_PlayerInput_Component : MonoBehaviour
{
    #region ���ں���

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


    #region ���̿���

    [Foldout("���̿���", true)]
    [Header("ʵ��ˮƽ��ת������")] public float RotationHorizonSensitivity = 200f; // ������ת������
    [Header("ʵ�崹ֱ��ת������")] public float RotationVerticleSensitivity = 200f; // ������ת������

    [Foldout("�ӽ�����", true)]
    [Header("��������Ʒ�Χ")] public Vector2 CameraLimitRange = new Vector2(-90, 90);
    [Header("ʵ��ͷ���Ʒ�Χ")] public Vector2 HeadLimitRange = new Vector2(-20, 20);

    // �����������ر���
    private float currentHorizontalInput;
    private float currentVerticalInput;

    // ����������ر���
    private float mouseHorizonSpeed;
    public float mouseVerticalSpeed;

    // ����������ƶ�����
    private Vector3 moveDirection;

    // �����AI��������
    void _ReferStart_ComponentInit()
    {
        //����AI���
        MC_AI_Component AI_Component = GetComponentInParent<MC_AI_Component>();
        if (AI_Component != null)
            AI_Component.enabled = false;

        //��ModelHead��ģ����ת����
        Collider_Component.Model.transform.rotation = Quaternion.Euler(0, 0, 0);
        Collider_Component.Head.transform.rotation = Quaternion.Euler(0, 0, 0);
        Collider_Component.Body.transform.rotation = Quaternion.Euler(0, 0, 0);

    }

    // ��ȡ���̺��������
    void GetInput()
    {
        // ��ȡ��������
        currentHorizontalInput = Input.GetAxis("Horizontal");
        currentVerticalInput = Input.GetAxis("Vertical");

        // ��ȡ������루X����ת��Y���ӽǣ�
        mouseHorizonSpeed = Input.GetAxis("Mouse X");
        mouseVerticalSpeed = Input.GetAxis("Mouse Y");
    }

    // �������루������Ը�����Ҫ��Ӷ���Ĵ����߼���
    void CaculateInput()
    {
        // ��ȡModel�ĳ���
        GameObject _Model = Collider_Component.Model;

        // ��ˮƽ���뷽��ת��ΪVector3������Model�ĳ�����м���
        moveDirection = (_Model.transform.forward * currentVerticalInput + _Model.transform.right * currentHorizontalInput).normalized;

        // �����������Ĵ�ֱֵ����ֹ�����ӽǳ�����Χ��
        mouseVerticalSpeed = Mathf.Clamp(mouseVerticalSpeed, -90f, 90f);
    }

    void ApplyInput()
    {
        // �趨Velocity
        Velocity_Component.SetVelocity("x", moveDirection.x * Velocity_Component.speed_move);
        Velocity_Component.SetVelocity("z", moveDirection.z * Velocity_Component.speed_move);

        // �趨����ת��
        Velocity_Component.EntityRotation(mouseHorizonSpeed * RotationHorizonSensitivity);

        // �趨����ת��
        Velocity_Component.EntityHeadVerticleRotation(mouseVerticalSpeed * RotationVerticleSensitivity, CameraLimitRange, HeadLimitRange);
    }

    #endregion
}
