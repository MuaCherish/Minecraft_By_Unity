using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MC_Component_Velocity))]
[RequireComponent(typeof(MC_Component_Physics))]
public class MC_Component_PlayerInput : MonoBehaviour
{
    #region ���ں���

    MC_Component_Velocity Component_Velocity;
    MC_Component_Physics Component_Physics;
    MC_Service_World Service_world; 

    private void Awake()
    {
        Component_Velocity = GetComponent<MC_Component_Velocity>();
        Component_Physics = GetComponent<MC_Component_Physics>();
        Service_world = Component_Physics.managerhub.Service_World;
    }

    private void Start()
    {
        _ReferStart_ComponentInit();
    }

    private void Update()
    {
        if (Service_world.game_state == Game_State.Playing)
        {
            GetInput();
            CaculateInput();
        }
    }

    private void FixedUpdate()
    {
        if (Service_world.game_state == Game_State.Playing)
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
        MC_Component_AI AI_Component = GetComponentInParent<MC_Component_AI>();
        if (AI_Component != null)
            AI_Component.enabled = false;

        //��ModelHead��ģ����ת����
        Component_Physics.Model.transform.rotation = Quaternion.Euler(0, 0, 0);
        Component_Physics.Head.transform.rotation = Quaternion.Euler(0, 0, 0);
        Component_Physics.Body.transform.rotation = Quaternion.Euler(0, 0, 0);

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
        GameObject _Model = Component_Physics.Model;

        // ��ˮƽ���뷽��ת��ΪVector3������Model�ĳ�����м���
        moveDirection = (_Model.transform.forward * currentVerticalInput + _Model.transform.right * currentHorizontalInput).normalized;

        // �����������Ĵ�ֱֵ����ֹ�����ӽǳ�����Χ��
        mouseVerticalSpeed = Mathf.Clamp(mouseVerticalSpeed, -90f, 90f);
    }

    void ApplyInput()
    {
        // �趨Velocity
        Component_Velocity.SetVelocity("x", moveDirection.x * Component_Velocity.speed_move);
        Component_Velocity.SetVelocity("z", moveDirection.z * Component_Velocity.speed_move);

        // �趨����ת��
        Component_Velocity.EntityRotation(mouseHorizonSpeed * RotationHorizonSensitivity);

        // �趨����ת��
        Component_Velocity.EntityHeadVerticleRotation(mouseVerticalSpeed * RotationVerticleSensitivity, CameraLimitRange, HeadLimitRange);
    }

    #endregion
}
