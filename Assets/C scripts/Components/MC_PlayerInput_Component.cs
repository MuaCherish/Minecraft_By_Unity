using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MC_PlayerInput_Component : MonoBehaviour
{


    #region ���ں���

    MC_Velocity_Component Velocity_Component;
    World world;

    private void Awake()
    {
        world = SceneData.GetWorld();
        Velocity_Component = GetComponent<MC_Velocity_Component>();
    }

    private void Start()
    {

    }

    private void Update()
    {

        if (world.game_state == Game_State.Playing)
        {
            _ReferUpdate_InputController();
        }


    }

    #endregion


    #region ���̿���

    void _ReferUpdate_InputController()
    {
        GetInput();
        CaculateInput();
        ApplyInput();
    }


    void GetInput()
    {
        //��ȡ��������
        //float currentHorizontalInput = Input.GetAxis("Horizontal");
        //float currentVerticalInput = Input.GetAxis("Vertical");

        //��ȡ�������
        //mouseHorizontal = Input.GetAxis("Mouse X");
        //mouseVerticalspeed = Input.GetAxis("Mouse Y");
    }


    void CaculateInput()
    {
        //�����뷽��תΪVector3

        //����������Yֵ��������
    }


    void ApplyInput()
    {
        //�趨Velocity

        //�趨����ת��

        //�趨����ת��
    }


    #endregion

}
