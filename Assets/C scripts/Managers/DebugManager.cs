using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using static UnityEngine.Animations.AimConstraint;

public class DebugManager : MonoBehaviour
{


    #region 状态

    [Header("状态")]
    [ReadOnly] public bool isDebug = false;

    #endregion


    #region 周期函数

    private ManagerHub managerHub;
    private void Start()
    {
        managerHub = VoxelData.GetManagerhub();
    }

    private void FixedUpdate()
    {
        if (isDebug)
        {
            UpdateScreen();
        }
    }

    void Update()
    {

        if (managerHub.world.game_state == Game_State.Start)
        {
            if (DebugScreen.activeSelf)
            {
                DebugScreen.SetActive(false);
            }


        }
        else if (managerHub.world.game_state == Game_State.Playing)
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                isDebug = !isDebug;
                DebugScreen.SetActive(!DebugScreen.activeSelf);
            }


        }



    }


    #endregion


    #region 调试屏幕

    public Camera FirstPersonCamera;
    public GameObject DebugScreen;
    public TextMeshProUGUI LeftText;
    public TextMeshProUGUI RightText;




    void UpdateScreen()
    {
        Vector3 footlocation = managerHub.world.PlayerFoot.position;

        //FPS
        CalculateFPS();


        //update
        //LeftText.text += $"\n";
        LeftText.text = $"帧数: {managerHub.fpsmManaer.fps:F2}\n";
        LeftText.text += $"当前时间: {managerHub.timeManager.GetCurrentTime():F2}时\n";
        LeftText.text += $"\n";
        LeftText.text += $"[Player]\n";
        LeftText.text += $"速度: {managerHub.player.velocity}\n";
        LeftText.text += $"朝向: {CalculateFacing()}\n";
        LeftText.text += $"实际朝向: {managerHub.player.FactFacing}\n";
        LeftText.text += $"实际运动方向: {managerHub.player.ActualMoveDirection}\n";
        //LeftText.text += $"新的运动方向: {managerHub.player.momentum}\n";
        LeftText.text += $"输入: {managerHub.player.keyInput}\n";
        LeftText.text += $"眼睛坐标: {managerHub.player.cam.position}\n";
        LeftText.text += $"实时重力: {managerHub.player.verticalMomentum}\n";
        LeftText.text += $"绝对坐标: {(new Vector3((int)footlocation.x, (int)footlocation.y, (int)footlocation.z))}\n";
        LeftText.text += $"相对坐标: {managerHub.world.GetRelalocation(footlocation)}\n";
        LeftText.text += $"已保存方块数量: {managerHub.world.EditNumber.Count}\n";
        LeftText.text += $"碰撞点检测个数:{managerHub.player.CollisionNumber}\n";
        //LeftText.text += $"生存模式玩家走过的路程: {managerHub.player.accumulatedDistance:F2}m\n";
        LeftText.text += $"\n";
        LeftText.text += $"[Chunk]\n";
        LeftText.text += $"区块坐标: {managerHub.world.GetChunkLocation(footlocation)}\n";
        LeftText.text += $"初始化区块平均渲染时间: {(managerHub.world.OneChunkRenderTime * 1000f):F3}ms\n";
        LeftText.text += $"\n";
        //LeftText.text += $"[Noise]\n";




        //RightText.text += $"\n";
        RightText.text = $"[System]\n";
        RightText.text += $"实时渲染面数: {CameraOnPreRender()}个\n";
        

    }

    

    #endregion


    #region DEBUG-计算FPS

    // FPS计数器
    private int frameCount;
    private float elapsedTime;
    private float fps;

    // 更新并计算FPS
    void CalculateFPS()
    {
        // 每帧累加时间
        elapsedTime += Time.deltaTime;
        frameCount++;

        // 每秒计算一次FPS
        if (elapsedTime >= 1.0f)
        {
            // 计算FPS
            fps = frameCount / elapsedTime;

            // 重置计数器和 elapsedTime
            frameCount = 0;
            elapsedTime = 0f;
        }
    }


    #endregion


    #region DEBUG-计算String朝向

    //facing
    string CalculateFacing()
    {
        Vector3 forward = managerHub.player.transform.forward;
        float angle = Mathf.Atan2(forward.z, forward.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        string facingDirection;

        if (Mathf.Abs(angle - 0) <= 30 || Mathf.Abs(angle - 360) <= 30)
        {
            facingDirection = "East";
        }
        else if (Mathf.Abs(angle - 90) <= 30)
        {
            facingDirection = "North";
        }
        else if (Mathf.Abs(angle - 180) <= 30)
        {
            facingDirection = "West";
        }
        else if (Mathf.Abs(angle - 270) <= 30)
        {
            facingDirection = "South";
        }
        else if (Mathf.Abs(angle - 45) <= 30)
        {
            facingDirection = "NorthEast";
        }
        else if (Mathf.Abs(angle - 135) <= 30)
        {
            facingDirection = "SouthEast";
        }
        else if (Mathf.Abs(angle - 225) <= 30)
        {
            facingDirection = "SouthWest";
        }
        else if (Mathf.Abs(angle - 315) <= 30)
        {
            facingDirection = "NorthWest";
        }
        else
        {
            facingDirection = "Unknown";
        }

        return facingDirection;
    }

    #endregion


    #region DEBUG-Camera渲染面数

    //渲染面数
    private int CameraOnPreRender()
    {
        int triangleCount = 0;

        // 获取场景中的所有 MeshRenderer
        MeshRenderer[] meshRenderers = FindObjectsOfType<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            // 检查物体是否在相机视野内
            if (meshRenderer.isVisible)
            {
                // 获取 MeshFilter 组件
                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    // 累加三角形数
                    triangleCount += meshFilter.sharedMesh.triangles.Length / 3; // 每个三角形由3个顶点组成
                }
            }
        }

        return triangleCount;
    }

    #endregion


}
