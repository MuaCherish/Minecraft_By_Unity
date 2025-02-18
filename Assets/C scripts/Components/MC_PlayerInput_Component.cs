using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MC_PlayerInput_Component : MonoBehaviour
{


    #region 周期函数

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


    #region 键盘控制

    void _ReferUpdate_InputController()
    {
        GetInput();
        CaculateInput();
        ApplyInput();
    }


    void GetInput()
    {
        //获取键盘输入
        //float currentHorizontalInput = Input.GetAxis("Horizontal");
        //float currentVerticalInput = Input.GetAxis("Vertical");

        //获取鼠标输入
        //mouseHorizontal = Input.GetAxis("Mouse X");
        //mouseVerticalspeed = Input.GetAxis("Mouse Y");
    }


    void CaculateInput()
    {
        //将输入方向转为Vector3

        //将鼠标输入的Y值进行限制
    }


    void ApplyInput()
    {
        //设定Velocity

        //设定左右转向

        //设定上下转向
    }


    #endregion

}
