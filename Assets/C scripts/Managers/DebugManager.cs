using Homebrew;
using TMPro;
using UnityEngine;
using static MC_Static_Math;

public class DebugManager : MonoBehaviour
{

    #region ״̬

    [Foldout("״̬", true)]
    [ReadOnly] public bool isDebug = false;

    #endregion


    #region ���ں���

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

    [Foldout("Debug����", true)]
    [Header("Debugˢ��Ƶ��")]
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


    #region ������Ļ

    [Foldout("������Ļ", true)]
    [Header("���������")] public Camera FirstPersonCamera;
    [Header("������Ļ��������")] public GameObject DebugScreen;
    [Header("�ı�����")] public TextMeshProUGUI[] TextScreens;



    void UpdateScreen()
    {
        Vector3 footlocation = managerhub.world.PlayerFoot.position;
        

        // ���� FPS ������ɫ
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
        TextScreens[0].text = $"<color={fpsColor}>֡��: {fps:F2}</color>\n";
        TextScreens[0].text += $"��ǰʱ��: {managerhub.timeManager.GetCurrentTime():F2}ʱ\n";
        TextScreens[0].text += $"\n";


        TextScreens[0].text += $"[Player]\n";
        TextScreens[0].text += $"�ٶ�: {managerhub.player.velocity}\n";
        TextScreens[0].text += $"����: {CalculateFacing()}\n";
        TextScreens[0].text += $"ʵ�ʳ���: {managerhub.player.FactFacing}\n";
        TextScreens[0].text += $"ʵ���˶�����: {managerhub.player.ActualMoveDirection}\n";
        //LeftText.text += $"�µ��˶�����: {managerhub.player.momentum}\n";
        TextScreens[0].text += $"����: {managerhub.player.keyInput}\n";
        TextScreens[0].text += $"�۾�����: {managerhub.player.cam.position}\n";
        TextScreens[0].text += $"ʵʱ����: {managerhub.player.verticalMomentum}\n";
        //LeftText.text += $"��������: {(new Vector3((int)footlocation.x, (int)footlocation.y, (int)footlocation.z))}\n";
        //LeftText.text += $"�������: {GetRelaPos(footlocation)}\n";
        TextScreens[0].text += $"�ѱ��淽������: {managerhub.world.EditNumber.Count}\n";
        TextScreens[0].text += $"��ײ�������:{managerhub.player.CollisionNumber}\n";
        //LeftText.text += $"����ģʽ����߹���·��: {managerHub.player.accumulatedDistance:F2}m\n";
        TextScreens[0].text += $"\n";


        TextScreens[0].text += $"[Chunk]\n";
        TextScreens[0].text += $"��������: {GetRelaChunkLocation(footlocation)}\n";
        TextScreens[0].text += $"��ʼ������ƽ����Ⱦʱ��: {CaculateChunkRenderTime()}\n";
        TextScreens[0].text += $"\n";


        TextScreens[0].text += $"[FootPosition]\n";
        TextScreens[0].text += $"foot��������: {(new Vector3(footlocation.x, footlocation.y, footlocation.z))} \n";
        TextScreens[0].text += $"foot�������: {GetRelaPos(footlocation)} \n";
        TextScreens[0].text += $"foot��������: {managerhub.world.GetBlockType(footlocation)} \n";
        TextScreens[0].text += $"\n";

        TextScreens[0].text += $"[System]\n";
        TextScreens[0].text += $"ʵʱ��Ⱦ����; {CameraOnPreRender(FirstPersonCamera)}\n";
        TextScreens[0].text += $"\n";


    }

    

    #endregion


    #region DEBUG-����String����

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


    #region DEBUG-Camera��Ⱦ����

    [Header("��Ⱦ��������")]
    private string chunksPath = "Environment/Chunks"; // ��Ҫ���ҵ�Ŀ¼·��

    // ��Ⱦ����
    private string CameraOnPreRender(Camera camera)
    {
        int triangleCount = 0;

        // ����ָ��·���µ�����������
        Transform[] chunks = GameObject.Find(chunksPath).GetComponentsInChildren<Transform>();

        foreach (Transform chunk in chunks)
        {
            MeshRenderer meshRenderer = chunk.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.isVisible) // ȷ��������Renderer�ҿɼ�
            {
                // ��ȡMeshRenderer�İ�Χ��
                Bounds bounds = meshRenderer.bounds;

                // ʹ�ð�Χ�м���Ƿ�����Ұ��
                if (IsInView(camera, bounds))
                {
                    // ��ȡMeshFilter���ۼ���������
                    MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        triangleCount += meshFilter.sharedMesh.triangles.Length / 3; // ÿ����������3���������
                    }
                }
            }
        }

        // ���������������ز�ͬ��ʽ���ַ���
        if (triangleCount < 10000)
        {
            return triangleCount.ToString(); // ����ʵ����ֵ
        }
        else
        {
            return $"{(triangleCount / 10000f):0.#}w"; // ������ 'w' Ϊ��λ�ĸ�ʽ
        }
    }

    // ��������������Χ���Ƿ����������Ұ��
    private bool IsInView(Camera camera, Bounds bounds)
    {
        // ��ȡ������Ĳü�ƽ��
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

        // ����Χ���Ƿ�����׶���ཻ
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }




    #endregion


    #region DEBUG-������Ⱦʱ��

    string CaculateChunkRenderTime()
    {
        float time = Mathf.Round(managerhub.world.OneChunkRenderTime * 1000f * 100f) / 100f;

        if (time == 0)
        {
            // ����"���ڼ���..."
            return "���ڼ���...";
        }
        else
        {
            // ���ر�����λС����time
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


    #region Chunk�߽���ʾ��

    [Foldout("Chunk�߽���ʾ", true)]
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


