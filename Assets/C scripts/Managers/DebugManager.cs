using Homebrew;
using TMPro;
using UnityEngine;

public class DebugManager : MonoBehaviour
{

    #region 状态

    [Foldout("状态", true)]
    [ReadOnly] public bool isDebug = false;

    #endregion


    #region 周期函数

    private ManagerHub managerHub;

    private void Awake()
    {
        Application.targetFrameRate = 100;
    }


    private void Start()
    {
        managerHub = VoxelData.GetManagerhub();
    }

    public void InitDebugManager()
    {
        isDebug = false;
    }

    [Foldout("Debug参数", true)]
    [Header("Debug刷新频率")]
    public float debugUpdateInterval = 0.5f;
    private float lastDebugUpdateTime;

    private void FixedUpdate()
    {
        if (isDebug && Time.time - lastDebugUpdateTime >= debugUpdateInterval)
        {
            UpdateScreen();
            lastDebugUpdateTime = Time.time;
        }
    }

    void Update()
    {

        if (managerHub.world.game_state == Game_State.Start)
        {
            if (DebugScreen.activeSelf)
            {
                isDebug = false;
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

            if (isDebug)
            {
                CaculateFPS();
            }

        }



    }


    #endregion


    #region 调试屏幕

    [Foldout("调试屏幕", true)]
    [Header("摄像机引用")] public Camera FirstPersonCamera;
    [Header("调试屏幕引用引用")] public GameObject DebugScreen;
    [Header("左文本引用")] public TextMeshProUGUI LeftText;
    [Header("右文本引用")] public TextMeshProUGUI RightText;




    void UpdateScreen()
    {
        Vector3 footlocation = managerHub.world.PlayerFoot.position;

        // 根据 FPS 设置颜色
        string fpsColor;
        if (fps < 60)
        {
            fpsColor = "red";
        }
        else if (fps <= 80)
        {
            fpsColor = "yellow";
        }
        else
        {
            fpsColor = "white";
        }

        //update
        //LeftText.text += $"\n";
        LeftText.text = $"<color={fpsColor}>帧数: {fps:F2}</color>\n";
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
        //LeftText.text += $"绝对坐标: {(new Vector3((int)footlocation.x, (int)footlocation.y, (int)footlocation.z))}\n";
        //LeftText.text += $"相对坐标: {managerHub.world.GetRelalocation(footlocation)}\n";
        LeftText.text += $"已保存方块数量: {managerHub.world.EditNumber.Count}\n";
        LeftText.text += $"碰撞点检测个数:{managerHub.player.CollisionNumber}\n";
        //LeftText.text += $"生存模式玩家走过的路程: {managerHub.player.accumulatedDistance:F2}m\n";
        LeftText.text += $"\n";
        LeftText.text += $"[Chunk]\n";
        LeftText.text += $"区块坐标: {managerHub.world.GetChunkLocation(footlocation)}\n";
        LeftText.text += $"初始化区块平均渲染时间: {CaculateChunkRenderTime()}\n";
        LeftText.text += $"\n";
        //LeftText.text += $"[Noise]\n";




        //RightText.text += $"\n"; 
        RightText.text = $"[System]\n";
        RightText.text += $"实时渲染面数; {CameraOnPreRender(FirstPersonCamera)}\n";
        RightText.text += $"\n";
        RightText.text += $"[Position]\n";
        RightText.text += $"foot绝对坐标: {(new Vector3((int)footlocation.x, (int)footlocation.y, (int)footlocation.z))} \n";
        RightText.text += $"foot相对坐标: {managerHub.world.GetRelalocation(footlocation)} \n";
        RightText.text += $"foot坐标类型: {managerHub.world.GetBlockType(footlocation)} \n";
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

    [Header("渲染面数设置")]
    private string chunksPath = "Environment/Chunks"; // 需要查找的目录路径

    // 渲染面数
    private int CameraOnPreRender(Camera camera)
    {
        int triangleCount = 0;

        // 查找指定路径下的所有子物体
        Transform[] chunks = GameObject.Find(chunksPath).GetComponentsInChildren<Transform>();

        foreach (Transform chunk in chunks)
        {
            MeshRenderer meshRenderer = chunk.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.isVisible)  // 确保物体在摄像机视野内
            {
                // 使用 WorldToViewportPoint 来检查物体是否在视野内
                Vector3 viewportPoint = camera.WorldToViewportPoint(chunk.position);
                if (viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                    viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                    viewportPoint.z > 0) // 确保物体在摄像头前方
                {
                    MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        // 累加三角形数
                        triangleCount += meshFilter.sharedMesh.triangles.Length / 3; // 每个三角形由3个顶点组成
                    }
                }
            }
        }

        return triangleCount;
    }




    #endregion


    #region DEBUG-区块渲染时间

    string CaculateChunkRenderTime()
    {
        float time = Mathf.Round(managerHub.world.OneChunkRenderTime * 1000f * 100f) / 100f;

        if (time == 0)
        {
            // 返回"正在计算..."
            return "正在计算...";
        }
        else
        {
            // 返回保留两位小数的time
            return time.ToString("F3") + "ms";
        }
    }



    #endregion


    #region DEBUG-FPS

    private float m_lastUpdateShowTime = 0f;
    private readonly float m_updateTime = 0.5f;
    private int m_frames = 0;
    private float m_frameDeltaTime = 0;
    public float fps { get; private set; }


    void CaculateFPS()
    {
        m_frames++;
        if (Time.realtimeSinceStartup - m_lastUpdateShowTime >= m_updateTime)
        {
            fps = m_frames / (Time.realtimeSinceStartup - m_lastUpdateShowTime);
            m_frameDeltaTime = (Time.realtimeSinceStartup - m_lastUpdateShowTime) / m_frames;
            m_frames = 0;
            m_lastUpdateShowTime = Time.realtimeSinceStartup;
        }
    }

    #endregion


}


