using Homebrew;
using TMPro;
using UnityEngine;
using static MC_Static_Math;

public class DebugManager : MonoBehaviour
{

    #region 状态

    [Foldout("状态", true)]
    [ReadOnly] public bool isDebug = false;

    #endregion


    #region 周期函数

    private ManagerHub managerhub;

    private void Awake()
    {
        Application.targetFrameRate = 100;
    }


    private void Start()
    {
        managerhub = SceneData.GetManagerhub();
    }

    public void InitDebugManager()
    {
        isDebug = false;
        isShowChunkBorder = false;
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

    void Update(){

        switch (managerhub.world.game_state)
        {
            case Game_State.Start:
                Handle_GameState_Start();
                break;

            case Game_State.Playing:
                Handle_GameState_Playing();
                break;
        }


    }


    void Handle_GameState_Start()
    {
        if (DebugScreen.activeSelf)
        {
            isDebug = false;
            DebugScreen.SetActive(false);
        }
    }

    void Handle_GameState_Playing()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            //RenderSettings.fog = isDebug;
            isDebug = !isDebug;
            DebugScreen.SetActive(!DebugScreen.activeSelf);
        }

        //Border
        if (Input.GetKeyDown(KeyCode.F4))
        {
            isShowChunkBorder = !isShowChunkBorder;

        }

        if (isShowChunkBorder)
        {
            ShowChunkBorder();
        }
        else
        {
            if (hasExec_ChunkBorder == false)
            {
                hasExec_ChunkBorder = true;
                ChunkBorderObject.SetActive(false);
            }
        }


        if (isDebug)
        {
            CaculateFPS();
        }
    }


    #endregion


    #region 调试屏幕

    [Foldout("调试屏幕", true)]
    [Header("摄像机引用")] public Camera FirstPersonCamera;
    [Header("调试屏幕引用引用")] public GameObject DebugScreen;
    [Header("文本引用")] public TextMeshProUGUI[] TextScreens;



    void UpdateScreen()
    {
        Vector3 footlocation = managerhub.world.PlayerFoot.position;
        

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
        //TextScreens[0].text += $"\n";
        TextScreens[0].text = $"<color={fpsColor}>帧数: {fps:F2}</color>\n";
        TextScreens[0].text += $"当前时间: {managerhub.timeManager.GetCurrentTime():F2}时\n";
        TextScreens[0].text += $"\n";


        TextScreens[0].text += $"[Player]\n";
        TextScreens[0].text += $"速度: {managerhub.player.velocity}\n";
        TextScreens[0].text += $"朝向: {CalculateFacing()}\n";
        TextScreens[0].text += $"实际朝向: {managerhub.player.FactFacing}\n";
        TextScreens[0].text += $"实际运动方向: {managerhub.player.ActualMoveDirection}\n";
        //LeftText.text += $"新的运动方向: {managerhub.player.momentum}\n";
        TextScreens[0].text += $"输入: {managerhub.player.keyInput}\n";
        TextScreens[0].text += $"眼睛坐标: {managerhub.player.cam.position}\n";
        TextScreens[0].text += $"实时重力: {managerhub.player.verticalMomentum}\n";
        //LeftText.text += $"绝对坐标: {(new Vector3((int)footlocation.x, (int)footlocation.y, (int)footlocation.z))}\n";
        //LeftText.text += $"相对坐标: {GetRelaPos(footlocation)}\n";
        TextScreens[0].text += $"已保存方块数量: {managerhub.world.EditNumber.Count}\n";
        TextScreens[0].text += $"碰撞点检测个数:{managerhub.player.CollisionNumber}\n";
        //LeftText.text += $"生存模式玩家走过的路程: {managerHub.player.accumulatedDistance:F2}m\n";
        TextScreens[0].text += $"\n";


        TextScreens[0].text += $"[Chunk]\n";
        TextScreens[0].text += $"区块坐标: {GetRelaChunkLocation(footlocation)}\n";
        TextScreens[0].text += $"初始化区块平均渲染时间: {CaculateChunkRenderTime()}\n";
        TextScreens[0].text += $"\n";


        TextScreens[0].text += $"[FootPosition]\n";
        TextScreens[0].text += $"foot绝对坐标: {(new Vector3(footlocation.x, footlocation.y, footlocation.z))} \n";
        TextScreens[0].text += $"foot相对坐标: {GetRelaPos(footlocation)} \n";
        TextScreens[0].text += $"foot坐标类型: {managerhub.world.GetBlockType(footlocation)} \n";
        TextScreens[0].text += $"\n";

        TextScreens[0].text += $"[System]\n";
        TextScreens[0].text += $"实时渲染面数; {CameraOnPreRender(FirstPersonCamera)}\n";
        TextScreens[0].text += $"\n";


    }

    

    #endregion


    #region DEBUG-计算String朝向

    //facing
    string CalculateFacing()
    {
        Vector3 forward = managerhub.player.transform.forward;
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
    private string CameraOnPreRender(Camera camera)
    {
        int triangleCount = 0;

        // 查找指定路径下的所有子物体
        Transform[] chunks = GameObject.Find(chunksPath).GetComponentsInChildren<Transform>();

        foreach (Transform chunk in chunks)
        {
            MeshRenderer meshRenderer = chunk.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.isVisible) // 确保物体有Renderer且可见
            {
                // 获取MeshRenderer的包围盒
                Bounds bounds = meshRenderer.bounds;

                // 使用包围盒检测是否在视野内
                if (IsInView(camera, bounds))
                {
                    // 获取MeshFilter并累加三角形数
                    MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        triangleCount += meshFilter.sharedMesh.triangles.Length / 3; // 每个三角形由3个顶点组成
                    }
                }
            }
        }

        // 根据三角形数返回不同格式的字符串
        if (triangleCount < 10000)
        {
            return triangleCount.ToString(); // 返回实际数值
        }
        else
        {
            return $"{(triangleCount / 10000f):0.#}w"; // 返回以 'w' 为单位的格式
        }
    }

    // 辅助函数：检测包围盒是否在摄像机视野内
    private bool IsInView(Camera camera, Bounds bounds)
    {
        // 获取摄像机的裁剪平面
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

        // 检测包围盒是否与视锥体相交
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }




    #endregion


    #region DEBUG-区块渲染时间

    string CaculateChunkRenderTime()
    {
        float time = Mathf.Round(managerhub.world.OneChunkRenderTime * 1000f * 100f) / 100f;

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


    #region Chunk边界显示器

    [Foldout("Chunk边界显示", true)]
    bool hasExec_ChunkBorder = true;
    public GameObject ChunkBorderObject;
    public Vector3 _newChunkLocation; Vector3 _Previous_hunkLocation = Vector3.zero;
    public bool isShowChunkBorder;

    public void ShowChunkBorder()
    {
        if (hasExec_ChunkBorder)
        {
            ChunkBorderObject.SetActive(true);
            hasExec_ChunkBorder = false;
        }


        _newChunkLocation = GetRelaChunkLocation(managerhub.player.transform.position) * 16f;

        if (_newChunkLocation != _Previous_hunkLocation)
        {
            ChunkBorderObject.transform.position = _newChunkLocation;
            _Previous_hunkLocation = _newChunkLocation;
        }
    }

    #endregion

}


