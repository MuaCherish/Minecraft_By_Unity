using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using System;
using static MC_Static_Unity;
using static MC_Static_Math;
using static MC_Static_Chunk;
using Homebrew;
using static UnityEditor.PlayerSettings;


public class World : MonoBehaviour
{

    #region ״̬

    [Foldout("״̬", true)]
    [Header("��Ϸ״̬")][ReadOnly] public Game_State game_state = Game_State.Start;
    [Header("��Ϸģʽ")][ReadOnly] public GameMode game_mode = GameMode.Survival;


    #endregion


    #region ���ں���

    ManagerHub managerhub;
    Player player;

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        player = managerhub.player;
        ChunkParent = SceneData.GetChunkParent();
    }

    private void Start()
    {
        
    }

    bool hasExec_Start = true;
    bool hasExec_Loading = true;
    bool hasExec_Playing = true;
    bool hasExec_Pause = true;
    private void Update()
    {
        switch (game_state)
        {
            case Game_State.Start:
                Handle_GameState_Start();
                if (!hasExec_Loading)
                    hasExec_Loading = true;
                if (!hasExec_Playing)
                    hasExec_Playing = true;
                if (!hasExec_Pause)
                    hasExec_Pause = true;
                break;

            case Game_State.Loading:
                Handle_GameState_Loading();
                if (!hasExec_Start)
                    hasExec_Start = true;
                if (!hasExec_Playing)
                    hasExec_Playing = true;
                if (!hasExec_Pause)
                    hasExec_Pause = true;
                break;

            case Game_State.Playing:
                Handle_GameState_Playing();
                if (!hasExec_Start)
                    hasExec_Start = true;
                if (!hasExec_Loading)
                    hasExec_Loading = true;
                if (!hasExec_Pause)
                    hasExec_Pause = true;
                break;

            case Game_State.Pause:
                Handle_GameState_Pause();
                if (!hasExec_Start)
                    hasExec_Start = true;
                if (!hasExec_Loading)
                    hasExec_Loading = true;
                if (!hasExec_Playing)
                    hasExec_Playing = true;
                break;
        }

    }

    void Handle_GameState_Start()
    {
        if(hasExec_Start)
        {
            InitWorldManager();
            hasExec_Start = false;
        }
    }

    void Handle_GameState_Loading()
    {
        if (hasExec_Loading)
        {
            hasExec_Loading = false;
        }
    }

    void Handle_GameState_Playing()
    {
        if(hasExec_Playing)
        {
            //Load_InitSpawnPos();
            hasExec_Playing = false;
        }
    }

    void Handle_GameState_Pause()
    {
        if (hasExec_Pause)
        {
            hasExec_Pause = false;
        }
    }

    #endregion


    #region ��Ϸ��������

    [Foldout("Block/Item", true)]
    [Header("Material-�������� + ��������")]
    public Material material;
    public Material material_Water;
    [Header("Block/Item��Ϣ")] public BlockType[] blocktypes;
    [Header("BlockTexture(���ڵ�����)")] public Texture2D atlasTexture;

    [Foldout("Chunk", true)]
    [Header("World-��Ⱦ����")]
    [Tooltip("4���Ǳ߳�Ϊ4*16��������")] public int renderSize = 5;        //��Ⱦ����뾶,��renderSize*16f
    [Tooltip("2���ǽӽ�2*16��ʱ��ʼˢ������")] public float StartToRender = 1f;
    public float DestroySize = 7f;

    [Foldout("Noise", true)]
    [Header("Ⱥϵ�������ʺ�����(ֵԽ��ΧԽС)")]
    public float ����Ũ��OxygenDensity;
    public float ��ά�ܶ�Density3d;
    public float ����̶�Aridity;
    public float ����ʪ��MoistureLevel;
    public BiomeNoiseSystem[] biomenoisesystems;
    [Header("���ʷֲ������ϵͳ(n%)(����Ϊ���֮n)")] public TerrainLayerProbabilitySystem terrainLayerProbabilitySystem;

    [Foldout("Player", true)]
    [Header("��ҳ�����")] public Vector3 Start_Position = new Vector3(1600f, 127f, 1600f);


    #endregion


    #region (���Ż�)Loading˳����

    [Header("New��ҳ�����")] public Vector3 PlayerSpawnPos;

    IEnumerator Handle_Loading()
    {
        Load_XZ();
        yield return StartCoroutine(Load_InitStartChunks());
        Load_InitSpawnPos();

    }

    //1. ��ȷ��XZ
    void Load_XZ()
    {
        float _X = UnityEngine.Random.Range(800, 3200);
        float _Y = TerrainData.ChunkHeight - 0.5f;
        float _Z = UnityEngine.Random.Range(800, 3200);
        PlayerSpawnPos = new Vector3(_X, _Y, _Z);
    }

    //2. ��������
    IEnumerator Load_InitStartChunks()
    {
        yield return null;
    }

    //3. ȷ��������
    void Load_InitSpawnPos()
    {

        //��ʼ��Y����
        MC_RayCastStruct _RayCast = MC_Static_Raycast.RayCast(managerhub, MC_RayCast_FindType.OnlyFindBlock, PlayerSpawnPos, Vector3.down, TerrainData.ChunkHeight, -1, 1f);
        PlayerSpawnPos.y = _RayCast.hitPoint_Previous.y;

        print(PlayerSpawnPos);

        //��������
        //world.Start_Position = transform.position;
        //managerhub.player.transform.position = new Vector3(UnityEngine.Random.Range(800, 3200), transform.position.y, UnityEngine.Random.Range(800, 3200));
    }

    //Playing

    //��ʼ���������
    public void _Player_InitPlayerLocation()
    {
        if (managerhub.world.isLoadSaving)
        {
            managerhub.world.Start_Position = managerhub.world.worldSetting.playerposition;
        }


        if (managerhub.world.isLoadSaving && managerhub.world.worldSetting.playerposition.y > 0)
        {
            managerhub.world.Start_Position = managerhub.world.worldSetting.playerposition;
            transform.rotation = managerhub.world.worldSetting.playerrotation;
        }
        else
        {

            managerhub.world.Start_Position = new Vector3(GetRealChunkLocation(managerhub.world.Start_Position).x, TerrainData.ChunkHeight - 2, GetRealChunkLocation(managerhub.world.Start_Position).z);
            managerhub.world.Start_Position = managerhub.world.AddressingBlock(managerhub.world.Start_Position, 3);

        }

        transform.position = world.Start_Position;

    }


    #endregion


    #region ���Ż�

    //һ���Դ���
    bool hasExec = true;
    bool hasExec_SetSeed = true;

    

    public void InitWorldManager()
    {
        hasExec_RandomPlayerLocation = true;
        game_mode = GameMode.Survival;


        isLoadSaving = false;
        �Ƿ�����Chunk���� = managerhub.�Ƿ�����Chunk����;

        // ʹ�� persistentDataPath ��Ϊ��Ŀ¼
        savingPATH = Path.Combine(Application.persistentDataPath);

        // ȷ��Ŀ¼����
        if (!Directory.Exists(savingPATH))
        {
            Directory.CreateDirectory(savingPATH);
        }

        // ��ӡĿ¼·����ȷ��
        //Debug.Log("�浵Ŀ¼: " + savingPATH);


        //��ʼ��
        Start_Position = new Vector3(1600f, 127f, 1600f);

        game_state = Game_State.Start;
        TheSaving = new List<SavingData>();
        EditNumber = new List<EditStruct>();
        //savingDatas = new List<SavingData>();
        renderSize = 10;
        StartToRender = 1f;
        DestroySize = 7f;
        
        Allchunks = new Dictionary<Vector3, Chunk>();
        WatingToCreate_Chunks = new List<Vector3>();
        WatingToRemove_Chunks = new List<Vector3>();
        myThread_Render = null;
        WaitToRender_New = new ConcurrentQueue<Chunk>();
        //�Ƿ�����Chunk���� = false;
        Center_Now = Vector3.zero;
        Center_direction = Vector3.zero;
        hasExec = true;
        hasExec_SetSeed = true;
        CreateCoroutine = null;
        RemoveCoroutine = null;
        Render_Coroutine = null;
        Mesh_Coroutine = null;
        MeshLock = false;
        WaitToCreateMesh = new ConcurrentQueue<Chunk>();
        RenderLock = false;
        WaitToRender = new ConcurrentQueue<Chunk>();
        WaitToRender_temp = new ConcurrentQueue<Chunk>();
        WaitToFlashChunkQueue = new ConcurrentQueue<Chunk>();

        // ���� Chunks ���Ŀ¼�µ�����������
        foreach (Transform child in ChunkParent.transform)
        {
            Destroy(child.gameObject);
        }

        //-------˳���ܱ仯------------------
        terrainLayerProbabilitySystem.Seed = UnityEngine.Random.Range(0, 100000000);
        worldSetting = new WorldSetting(terrainLayerProbabilitySystem.Seed);
        UnityEngine.Random.InitState(worldSetting.seed);
        //-------------------------------------

        managerhub.canvasManager.PointSaving = "";

        //��ʼ������ʱ��
        InitStartTime = 0f;
        InitEndTime = 0f;

        if (managerhub.������ģʽ)
        {
            renderSize = 2;
        }
        if (managerhub.�޺�ҹģʽ)
        {
            if (managerhub.timeManager.gameObject.activeSelf)
                managerhub.timeManager.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {

        //Mesh�̳߳�פ
        CreateMeshCoroutineManager();


        //��Ⱦ�̳߳�פ
        RenderCoroutineManager();



        //��ʼ����ͼ
        if (game_state == Game_State.Loading)
        {

            if (hasExec_SetSeed)
            {
                InitStartTime = Time.time;
                //��ȡ��ǰģʽ
                //if (canvasManager.currentWorldType == 6)
                //{
                //    //SuperPlainMode = true;
                //}

                //worldSetting.worldtype = canvasManager.currentWorldType

                //��ʼ��ʼ��
                Update_CenterChunks(true);

                hasExec_SetSeed = false;
            }


        }


        //��Ϸ��ʼ
        if (game_state == Game_State.Playing)
        {

            if (hasExec)
            {

                LockMouse(true);
                hasExec = false;
            }


            //����ƶ�ˢ��
            //�������16f
            if (Get2DLengthforVector3(player.foot.transform.position - Center_Now) > (StartToRender * 16f) && Get2DLengthforVector3(player.foot.transform.position - Center_Now) <= ((StartToRender + 1) * 16f))
            {

                //����Center
                Center_direction = NormalizeToAxis(player.foot.transform.position - Center_Now);
                Center_Now += Center_direction * TerrainData.ChunkWidth;

                //���Chunk
                AddtoCreateChunks(Center_direction);
                AddtoRemoveChunks(Center_direction);

            }
            //����ƶ���Զ����
            else if (Get2DLengthforVector3(player.foot.transform.position - Center_Now) > ((StartToRender + 1) * 16f))
            {



                Update_CenterWithNoInit();




            }


        }

    }

    void OnApplicationQuit()
    {

        //������Ϸ
        game_state = Game_State.Ending;

        //�ȴ�Render�߳�
        if (myThread_Render != null && myThread_Render.IsAlive)
        {

            myThread_Render.Join(); // �ȴ��̰߳�ȫ����ֹ

        }

    }

    #endregion

   
    //ChunkGenerator
    #region [ChunkGenerator]��ʼ����ͼ

    IEnumerator Init_Map_Thread(bool _isInitPlayerLocation)
    {
        //ȷ��Chunk���ĵ�
        GetChunkCenterNow();

        //��ʼ�����鲢��ӽ�����
        float temp = 0f;
        for (int x = -renderSize + (int)(Center_Now.x / TerrainData.ChunkWidth); x < renderSize + (int)(Center_Now.x / TerrainData.ChunkWidth); x++){
            for (int z = -renderSize + (int)(Center_Now.z / TerrainData.ChunkWidth); z < renderSize + (int)(Center_Now.z / TerrainData.ChunkWidth); z++){
                CreateBaseChunk(new Vector3(x, 0, z));
                temp++;
                float max = renderSize * renderSize * 4;
                managerhub.canvasManager.Initprogress = Mathf.Lerp(0f, 0.9f, temp / max);
                yield return new WaitForSeconds(InitCorountineDelay);
            }

        }

        //���³�ʼ�����λ�ã���ֹ��ģ
        if (_isInitPlayerLocation)
        {
            managerhub.player.InitPlayerLocation();
        }


        //��Ϸ��ʼ
        yield return new WaitForSeconds(0.5f);
        managerhub.canvasManager.Initprogress = 1f;

        //�������Ż�Э��
        StartCoroutine(Chunk_Optimization());
        StartCoroutine(FlashChunkCoroutine());

        Init_MapCoroutine = null;
    }

    //ȷ��Chunk���ĵ�
    void GetChunkCenterNow()
    {

        if (isLoadSaving)
        {
            Center_Now = new Vector3(GetRealChunkLocation(worldSetting.playerposition).x, 0, GetRealChunkLocation(worldSetting.playerposition).z);
            return;
        }


        if (hasExec_RandomPlayerLocation)
        {
            managerhub.player.RandomPlayerLocaiton();
            hasExec_RandomPlayerLocation = false;
        }
        //print(PlayerFoot.transform.position);
        Center_Now = new Vector3(GetRealChunkLocation(player.foot.transform.position).x, 0, GetRealChunkLocation(player.foot.transform.position).z);

    }

    #endregion


    #region [ChunkGenerator]��̬���ص�ͼ

    public GameObject ChunkParent;
    [HideInInspector] public bool �Ƿ�����Chunk���� = false;
    [Header("Cave-��Ѩϵͳ")]
    public float noise3d_scale = 0.085f;
    public float cave_width = 0.45f;

    //ȫ��Chunkλ��
    public Dictionary<Vector3, Chunk> Allchunks = new Dictionary<Vector3, Chunk>();
    private readonly object Allchunks_Lock = new object();

    //�ȴ���Ӷ���
    private List<Vector3> WatingToCreate_Chunks = new List<Vector3>();
    private List<Vector3> WatingToRemove_Chunks = new List<Vector3>();
    private Chunk obj;

    //Э��
    [Header("Corountine-Э���ӳ�ʱ��")]
    public float InitCorountineDelay = 1f;
    public float CreateCoroutineDelay = 0.5f;
    public float RemoveCoroutineDelay = 0.5f;
    public float RenderDelay = 0.1f;
    public int Mesh_0_TaskCount = 0;

    //���ɷ���
    private Vector3 Center_Now;
    private Vector3 Center_direction; //��������˷���


    //��ʱ
    public float InitStartTime;
    public float InitEndTime;

    //Flash
    public ConcurrentQueue<Chunk> WaitToFlashChunkQueue = new ConcurrentQueue<Chunk>();


    //Create && Remove Э��
    Coroutine CreateCoroutine;
    Coroutine RemoveCoroutine;

    //��ʼ����ͼ
    public Coroutine Init_MapCoroutine;
    bool hasExec_RandomPlayerLocation = true;
    

    Coroutine Init_Map_Thread_NoInit_Coroutine;

    public void Update_CenterWithNoInit()
    {
        if (Init_Map_Thread_NoInit_Coroutine == null)
        {
            //print("����ƶ�̫�죡Center_Now�Ѹ���");
            Init_Map_Thread_NoInit_Coroutine = StartCoroutine(Init_Map_Thread_NoInit());

            //managerhub.timeManager.UpdateDayFogDistance();
            HideFarChunks();
        }
    }

    IEnumerator Init_Map_Thread_NoInit()
    {

        Center_Now = new Vector3(GetRealChunkLocation(player.foot.transform.position).x, 0, GetRealChunkLocation(player.foot.transform.position).z);

        for (int x = -renderSize + (int)(Center_Now.x / TerrainData.ChunkWidth); x < renderSize + (int)(Center_Now.x / TerrainData.ChunkWidth); x++)
        {

            for (int z = -renderSize + (int)(Center_Now.z / TerrainData.ChunkWidth); z < renderSize + (int)(Center_Now.z / TerrainData.ChunkWidth); z++)
            {

                //Create
                CreateBaseChunk(new Vector3(x, 0, z));

                yield return new WaitForSeconds(InitCorountineDelay);
            }

        }

        Init_Map_Thread_NoInit_Coroutine = null;

    }


    //������������
    public void Update_CenterChunks(bool _isInitPlayerLocation)
    {
        //print("������������");
        //update������������
        if (Init_MapCoroutine == null)
        {
            Init_MapCoroutine = StartCoroutine(Init_Map_Thread(_isInitPlayerLocation));
        }


    }

    //�����Զ����
    public void HideFarChunks()
    {
        foreach (var temp in Allchunks)
        {
            if (Get2DLengthforVector3(player.foot.transform.position - temp.Value.myposition) > (StartToRender * 16f))
            {
                temp.Value.HideChunk();
            }
        }
    }

    //�Ż�Chunk����Э��
    //�������ǰ�BaseChunkȫ����������һ��

    public IEnumerator Chunk_Optimization()
    {

        foreach (var Chunk in Allchunks)
        {

            WaitToCreateMesh.Enqueue(Chunk.Value);

        }



        yield return new WaitForSeconds(1f);

    }


    //����ѽ��յĵ���Chunk�ö��߳�ˢ��
    public IEnumerator FlashChunkCoroutine()
    {

        while (true)
        {

            if (WaitToFlashChunkQueue.TryDequeue(out Chunk chunktemp))
            {
                Thread myThread = new Thread(new ThreadStart(chunktemp.UpdateChunkMesh_WithSurround));
                myThread.Start();
            }


            yield return null;
        }



    }


    //��ӵ��ȴ���Ӷ���
    void AddtoCreateChunks(Vector3 add_vec)
    {

        //ForWard
        if (add_vec == new Vector3(0, 0, 1))
        {

            add_vec = (Center_Now / TerrainData.ChunkWidth) + Center_direction * (renderSize - 1);

            //����Chunk
            for (int i = -renderSize; i < renderSize; i++)
            {
                //CreateChunk(add_vec + new Vector3((float)i, 0, 0));

                WatingToCreate_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));


            }

            //�������Chunk����
            for (int i = -renderSize; i < renderSize; i++)
            {
                //CreateChunk(add_vec + new Vector3((float)i, 0, 0));

                lock (Allchunks_Lock)
                {

                    if (Allchunks.TryGetValue(add_vec + new Vector3((float)i, 0, -1), out Chunk chunktemp))
                        WaitToCreateMesh.Enqueue(chunktemp);

                }





            }

        }

        //Back
        if (add_vec == new Vector3(0, 0, -1))
        {

            add_vec = (Center_Now / TerrainData.ChunkWidth) + Center_direction * (renderSize);

            for (int i = -renderSize; i < renderSize; i++)
            {

                WatingToCreate_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));

            }

            //�������Chunk����
            for (int i = -renderSize; i < renderSize; i++)
            {

                //CreateChunk(add_vec + new Vector3((float)i, 0, 0));
                lock (Allchunks_Lock)
                {

                    if (Allchunks.TryGetValue(add_vec + new Vector3((float)i, 0, 1), out Chunk chunktemp))
                        WaitToCreateMesh.Enqueue(chunktemp);

                }




            }

        }

        //Left
        if (add_vec == new Vector3(-1, 0, 0))
        {

            add_vec = (Center_Now / TerrainData.ChunkWidth) + Center_direction * (renderSize);

            for (int i = -renderSize; i < renderSize; i++)
            {

                //CreateChunk(add_vec + new Vector3(0, 0, (float)i));
                WatingToCreate_Chunks.Add(add_vec + new Vector3(0, 0, (float)i));

            }

            //�������Chunk����
            for (int i = -renderSize; i < renderSize; i++)
            {

                lock (Allchunks_Lock)
                {

                    //CreateChunk(add_vec + new Vector3((float)i, 0, 0));
                    if (Allchunks.TryGetValue(add_vec + new Vector3(1, 0, (float)i), out Chunk chunktemp))
                        WaitToCreateMesh.Enqueue(chunktemp);

                }



            }

        }



        //Right
        if (add_vec == new Vector3(1, 0, 0))
        {

            add_vec = (Center_Now / TerrainData.ChunkWidth) + Center_direction * (renderSize - 1);

            for (int i = -renderSize; i < renderSize; i++)
            {

                //CreateChunk(add_vec + new Vector3(0, 0, (float)i));
                WatingToCreate_Chunks.Add(add_vec + new Vector3(0, 0, (float)i));

            }


            //�������Chunk����
            for (int i = -renderSize; i < renderSize; i++)
            {

                lock (Allchunks_Lock)
                {

                    //CreateChunk(add_vec + new Vector3((float)i, 0, 0));
                    if (Allchunks.TryGetValue(add_vec + new Vector3(-1, 0, (float)i), out Chunk chunktemp))
                        WaitToCreateMesh.Enqueue(chunktemp);

                }



            }

        }

        //Debug.Log("�Ѿ��������");


        //�ж��Ƿ�����Э��
        //���������������Э�̣��������ݶ���һ������Э��
        if (WatingToCreate_Chunks.Count > 0 && CreateCoroutine == null)
        {

            CreateCoroutine = StartCoroutine(CreateChunksQueue());

        }


    }

    //Э�̣�����Chunk
    private IEnumerator CreateChunksQueue()
    {

        while (true)
        {

            yield return new WaitForSeconds(CreateCoroutineDelay);


            //��������������ݣ��Ͷ�ȡ
            //���������û�����ݣ��͹ر�Э��
            if (WatingToCreate_Chunks.Count > 0)
            {

                //����鵽��chunk�Ѿ����ڣ�����
                //������������
                if (Allchunks.TryGetValue(WatingToCreate_Chunks[0], out obj))
                {

                    if (obj.isShow == false)
                    {

                        obj.ShowChunk();

                    }

                }
                else
                {

                    CreateChunk(WatingToCreate_Chunks[0]);

                }

                WatingToCreate_Chunks.RemoveAt(0);

            }
            else
            {

                CreateCoroutine = null;
                break;

            }



        }

    }

    //����Chunk
    //BaseChunk������������޳�
    void CreateBaseChunk(Vector3 pos)
    {

        //���ж�һ����û��
        if (Allchunks.ContainsKey(pos))
        {
            Allchunks[pos].ShowChunk();
            return;
        }

        //����Chunk
        Vector3 _ChunkLocation = new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z));
        Chunk _chunk_temp;


        //if (_ChunkLocation == new Vector3(195f,0,89f))
        //{
        //    print("");
        //}

        //����Chunk
        if (ContainsChunkLocation(_ChunkLocation))
        {
            _chunk_temp = new Chunk(_ChunkLocation, managerhub, false, TheSaving[GetIndexOfChunkLocation(_ChunkLocation)].EditDataInChunkList);
        }
        else
        {
            _chunk_temp = new Chunk(_ChunkLocation, managerhub, false);
        }

        //GameObject chunkGameObject = new GameObject($"{Mathf.FloorToInt(pos.x)}, 0, {Mathf.FloorToInt(pos.z)}");
        //Chunk chunktemp = chunkGameObject.AddComponent<Chunk>();
        //chunktemp.InitChunk(new Vector3(0, 0, 0), this);

        //��ӵ��ֵ�
        Allchunks.Add(_ChunkLocation, _chunk_temp);

    }

    //��BaseChunk�����Chunk���޳�
    void CreateChunk(Vector3 pos)
    {

        //���ж�һ����û��
        if (Allchunks.ContainsKey(pos))
        {

            Allchunks[pos].ShowChunk();
            return;

        }


        Vector3 _ChunkLocation = new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z));
        Chunk _chunk_temp;

        //����Chunk
        if (ContainsChunkLocation(_ChunkLocation))
        {
            _chunk_temp = new Chunk(_ChunkLocation, managerhub, false, TheSaving[GetIndexOfChunkLocation(_ChunkLocation)].EditDataInChunkList);
        }
        else
        {
            _chunk_temp = new Chunk(_ChunkLocation, managerhub, false);
        }


        //GameObject chunkGameObject = new GameObject($"{Mathf.FloorToInt(pos.x)}, 0, {Mathf.FloorToInt(pos.z)}");
        //Chunk chunktemp = chunkGameObject.AddComponent<Chunk>();
        //chunktemp.InitChunk(new Vector3(0, 0, 0), this);

        //��ӵ��ֵ�
        Allchunks.Add(pos, _chunk_temp);

    }

    //��ӵ��ȴ�ɾ������
    void AddtoRemoveChunks(Vector3 add_vec)
    {

        //ForWard
        if (add_vec == new Vector3(0, 0, 1))
        {

            add_vec = (Center_Now / TerrainData.ChunkWidth) - Center_direction * (renderSize + 1);

            for (int i = -renderSize; i < renderSize; i++)
            {

                WatingToRemove_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));

            }

        }



        //Back
        if (add_vec == new Vector3(0, 0, -1))
        {

            add_vec = (Center_Now / TerrainData.ChunkWidth) - Center_direction * (renderSize);

            for (int i = -renderSize; i < renderSize; i++)
            {

                WatingToRemove_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));

            }

        }



        //Left
        if (add_vec == new Vector3(-1, 0, 0))
        {

            add_vec = (Center_Now / TerrainData.ChunkWidth) - Center_direction * (renderSize);

            for (int i = -renderSize; i < renderSize; i++)
            {

                WatingToRemove_Chunks.Add(add_vec + new Vector3(0, 0, (float)i));

            }

        }

        //Right
        if (add_vec == new Vector3(1, 0, 0))
        {

            add_vec = (Center_Now / TerrainData.ChunkWidth) - Center_direction * (renderSize + 1);

            for (int i = -renderSize; i < renderSize; i++)
            {

                WatingToRemove_Chunks.Add(add_vec + new Vector3(0, 0, (float)i));

            }

        }

        //�ж��Ƿ�����Э��
        //���������������Э�̣��������ݶ���һ������Э��
        if (WatingToRemove_Chunks.Count > 0 && RemoveCoroutine == null)
        {

            RemoveCoroutine = StartCoroutine(RemoveChunksQueue());

        }

    }

    //Э�̣�ɾ��ChunK
    private IEnumerator RemoveChunksQueue()
    {

        while (true)
        {

            yield return new WaitForSeconds(RemoveCoroutineDelay);


            //��������������ݣ��Ͷ�ȡ
            //���������û�����ݣ��͹ر�Э��
            if (WatingToRemove_Chunks.Count > 0)
            {

                if (Allchunks.TryGetValue(WatingToRemove_Chunks[0], out obj))
                {

                    Chunk_HideOrRemove(WatingToRemove_Chunks[0]);

                    WatingToRemove_Chunks.RemoveAt(0);

                }
                else
                {

                    WatingToRemove_Chunks.RemoveAt(0);
                    WatingToRemove_Chunks.Add(WatingToRemove_Chunks[0]);

                }


            }
            else
            {

                RemoveCoroutine = null;
                break;

            }



        }

    }


    #endregion
    #region [ChunkGenerator]��������

    void Chunk_HideOrRemove(Vector3 chunklocation)
    {

        obj.HideChunk();
    }

    #endregion
    #region [ChunkGenerator]��Ⱦ����

    public Thread myThread_Render;
    public int Delay_RenderMesh = 1000;
    public ConcurrentQueue<Chunk> WaitToRender_New = new ConcurrentQueue<Chunk>();

    //Render_0 && Render_1 Э��
    [HideInInspector] public bool RenderLock = false;
    public ConcurrentQueue<Chunk> WaitToRender = new ConcurrentQueue<Chunk>();
    public ConcurrentQueue<Chunk> WaitToRender_temp = new ConcurrentQueue<Chunk>();
    Coroutine Render_Coroutine;


    //Threading
    [HideInInspector] public bool MeshLock = false;
    public ConcurrentQueue<Chunk> WaitToCreateMesh = new ConcurrentQueue<Chunk>();
    Coroutine Mesh_Coroutine;

    //��ȾЭ�̳�
    void RenderCoroutineManager()
    {
        // ����ȴ���Ⱦ�Ķ��в�Ϊ�գ�����û���������е���ȾЭ��
        if (WaitToRender.Count != 0 && Render_Coroutine == null)
        {
            //print($"������ȾЭ��");
            Render_Coroutine = StartCoroutine(Render_0());
        }
    }

    // һ����ȾЭ��
    IEnumerator Render_0()
    {
        bool hasError = false;  // ����Ƿ����쳣

        while (true)
        {
            try
            {
                // ���ԴӶ�����ȡ��Ҫ��Ⱦ��Chunk
                if (WaitToRender.TryDequeue(out Chunk chunktemp))
                {
                    //print($"{GetRelaChunkLocation(chunktemp.myposition)}��ʼ��Ⱦ");

                    // ���Chunk�Ѿ�׼������Ⱦ������CreateMesh
                    if (chunktemp.isReadyToRender)
                    {
                        chunktemp.CreateMesh();
                    }
                }

                // �������Ϊ�գ�ֹͣЭ��
                if (WaitToRender.Count == 0)
                {
                    //print($"����Ϊ�գ�ֹͣЭ��");
                    Render_Coroutine = null;
                    RenderLock = false;
                    break;
                }
            }
            catch (Exception ex)
            {
                // �����쳣����ֹЭ�����쳣��ֹ
                Debug.LogError($"��ȾЭ�̳���: {ex.Message}\n{ex.StackTrace}");

                hasError = true;  // ��Ƿ�������
                break;  // �˳���ǰѭ�����ȴ�����������
            }

            // ��������ȴ�һ��ʱ���Կ�����ȾƵ��
            yield return new WaitForSeconds(RenderDelay);
        }

        // ����������쳣���ȴ�������Э��
        if (hasError)
        {
            Render_Coroutine = null;  // ����Э��״̬
            yield return new WaitForSeconds(1f);  // �ȴ�һ��ʱ��
            RenderCoroutineManager();  // ����������ȾЭ��
        }
    }

    //MeshЭ��
    void CreateMeshCoroutineManager()
    {

        if (WaitToCreateMesh.Count != 0 && Mesh_Coroutine == null)
        {

            Mesh_Coroutine = StartCoroutine(Mesh_0());

        }

    }

    bool hasExec_CaculateInitTime = true;
    public float OneChunkRenderTime;
    IEnumerator Mesh_0()
    {

        while (true)
        {

            if (MeshLock == false)
            {

                MeshLock = true;

                WaitToCreateMesh.TryDequeue(out Chunk chunktemp);

                //print($"{GetRelaChunkLocation(chunktemp.myposition)}��ӵ�meshQueue");

                //Mesh�߳�
                Thread myThread = new Thread(new ThreadStart(chunktemp.UpdateChunkMesh_WithSurround));
                myThread.Start();

                if (WaitToCreateMesh.Count == 0)
                {
                    if (hasExec_CaculateInitTime)
                    {
                        //print("��Ⱦ����");
                        InitEndTime = Time.time;

                        //renderSize * renderSize * 4������������2����Ϊ���޳���Ⱦ������
                        OneChunkRenderTime = (InitEndTime - InitStartTime) / (renderSize * renderSize * 4 * 2);

                        hasExec_CaculateInitTime = false;
                    }

                    Mesh_Coroutine = null;
                    break;

                }




            }

            Mesh_0_TaskCount = WaitToCreateMesh.Count;
            //print("WaitToCreateMesh.Count");
            yield return new WaitForSeconds(RenderDelay);


        }

    }

    #endregion
    #region [ChunkGenerator]����Chunk����

    //Vector3 --> ���������
    public Chunk GetChunkObject(Vector3 pos)
    {

        Allchunks.TryGetValue(GetRelaChunkLocation(pos), out Chunk chunktemp);
        return chunktemp;

    }

    //New-��ȡ�������
    public bool TryGetChunkObject(Vector3 pos, out Chunk chunktemp)
    {
        
        if (Allchunks.TryGetValue(GetRelaChunkLocation(pos), out Chunk _chunktemp))
        {
            chunktemp = _chunktemp;
            return true;
        }

        chunktemp = null;
        return false;
    }

    #endregion
    #region [ChunkGenerator]��ȡ����

    /// <summary>
    /// ���ط�������,������Ǿ�������
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public byte GetBlockType(Vector3 pos)
    {
        //��ǰ����-�Ҳ�������
        if (!Allchunks.TryGetValue(GetRelaChunkLocation(pos), out Chunk chunktemp))
            return 0;

        //��ǰ����-������������߽�
        if (isOutOfChunkRange(pos))
            return 0;

        //��ȡ�������
        Vector3 _vec = GetRelaPos(pos);

        //��ȡBlock����
        byte block_type = chunktemp.GetBlock((int)_vec.x, (int)_vec.y, (int)_vec.z).voxelType;

        //Return
        return block_type;

    }

    #endregion
    #region [ChunkGenerator]�ṹ����

    //���ڼ�¼����
    //recordData.pos����Ǿ�������
    private List<EditStruct> recordData = new List<EditStruct>();

    //��¼����
    public void RecordBuilding(Vector3 _Start, Vector3 _End)
    {
        recordData.Clear();


        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {

            }
        }
    }

    //�ͷŽ���
    //����0�±��end�±꣬��������������飬Ȼ������ݶ������鼴��
    public void ReleaseBuilding(Vector3 place, List<EditStruct> _recordData)
    {

        Vector3 start = _recordData[0].editPos;
        Vector3 end = _recordData[_recordData.Count - 1].editPos;

        for (float x = start.x; x < end.x; x += 16f)
        {
            for (float z = start.z; z < end.z; z += 16f)
            {

            }
        }
    }


    #endregion
    #region [ChunkGenrator]�޸ķ���

    //����޸ķ���
    public void EditBlock(Vector3 _pos, byte _target)
    {
        Allchunks[GetRelaChunkLocation(_pos)].EditData(_pos, _target);
    }

    public void EditBlock(List<EditStruct> _editStructs)
    {
        List<Vector3> _ChunkLocations = new List<Vector3>();

        // ����_editStructs���洢ChunkLocations
        foreach (var item in _editStructs)
        {

            // ���allchunks��û��pos.��_ChunkLocations���
            if (Allchunks.ContainsKey(GetRelaChunkLocation(item.editPos)))
            {
                if (!_ChunkLocations.Contains(GetRelaChunkLocation(item.editPos)))
                {
                    _ChunkLocations.Add(GetRelaChunkLocation(item.editPos));
                }


            }
            else
            {
                print($"���鲻����:{GetRelaChunkLocation(item.editPos)}");

            }


        }

        // ����_ChunkLocations����allchunk���_ChunkLocationsִ��EditData
        foreach (var chunkLocation in _ChunkLocations)
        {
            Allchunks[chunkLocation].EditData(_editStructs);
        }

        // ��ӡ�ҵ�����������
        //print($"�ҵ�{_ChunkLocations.Count}��");
    }


    Coroutine editBlockCoroutine;
    public void EditBlock(List<EditStruct> _editStructs, float _time)
    {
        if (editBlockCoroutine == null)
        {
            editBlockCoroutine = StartCoroutine(Coroutine_editBlock(_editStructs, _time));
        }


    }

    IEnumerator Coroutine_editBlock(List<EditStruct> _editStructs, float _time)
    {
        foreach (var item in _editStructs)
        {
            //print("ִ��EditBlocks");
            Allchunks[GetRelaChunkLocation(item.editPos)].EditData(item.editPos, item.targetType);

            yield return new WaitForSeconds(_time);
        }
        editBlockCoroutine = null;
    }

    #endregion
    #region [ChunkGenerator]������

    //�������ײ�еķ����ж�
    //true������ײ
    public bool CollisionCheckForVoxel(Vector3 pos)
    {

        Vector3 realLocation = pos; //��������
        Vector3 relaLocation = GetRelaPos(realLocation);
        byte targetBlock = GetBlockType(realLocation);

        //�����ж�(Chunk)
        if (!Allchunks.ContainsKey(GetRelaChunkLocation(realLocation)))
            return true;

        //�����ж�(Y)
        if (realLocation.y >= TerrainData.ChunkHeight || realLocation.y < 0)
            return false;

        //������Զ�����ײ
        if (blocktypes[targetBlock].isSolid && blocktypes[targetBlock].isDIYCollision)
        {
            realLocation = CollisionOffset(realLocation, targetBlock);
            Vector3 OffsetrelaLocation = GetRelaPos(realLocation);

            if (OffsetrelaLocation != relaLocation)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //���ع��廹�ǿ���
        return blocktypes[Allchunks[GetRelaChunkLocation(realLocation)].voxelMap[(int)relaLocation.x, (int)relaLocation.y, (int)relaLocation.z].voxelType].isSolid;

    }

    //public float XOFFSET;
    //public float YOFFSET;
    //public float ZOFFSET;

    public Vector3 CollisionOffset(Vector3 _realPos, byte _targetType)
    {
        Vector3 _input = new Vector3(managerhub.player.horizontalInput, managerhub.player.Facing.y, managerhub.player.verticalInput);
        float _x = _realPos.x; Vector2 _xRange = blocktypes[_targetType].CollosionRange.xRange; float _xOffset = _x - (int)_x;
        float _y = _realPos.y; Vector2 _yRange = blocktypes[_targetType].CollosionRange.yRange; float _yOffset = _y - (int)_y;
        float _z = _realPos.z; Vector2 _zRange = blocktypes[_targetType].CollosionRange.zRange; float _zOffset = _z - (int)_z;


        //X
        if (_input.x >= 0 || _xOffset < _xRange.x)
            _x -= _xRange.x;
        else if (_input.x < 0 || _xOffset > _xRange.y)
            _x += (1 - _xRange.y);


        //Y
        if (_input.y >= 0 || _yOffset < _yRange.x)
            _y -= _yRange.x;
        else if (_input.y < 0 || _yOffset > _yRange.y)
            _y += (1 - _yRange.y);

        //Z
        if (_input.z >= 0 || _zOffset < _zRange.x)
            _z -= _zRange.x;
        else if (_input.z < 0 || _zOffset > _zRange.y)
            _z += (1 - _zRange.y);

        return new Vector3(_x, _y, _z);
    }

    //���ø��������
    //�����۾����ߵļ��
    public bool RayCheckForVoxel(Vector3 pos)
    {

        Vector3 realLocation = pos; //��������
        Vector3 relaLocation = GetRelaPos(realLocation);
        byte targetBlock = GetBlockType(realLocation);

        if (!Allchunks.ContainsKey(GetRelaChunkLocation(pos))) { return false; }

        //�����������
        Vector3 vec = GetRelaPos(new Vector3(pos.x, pos.y, pos.z));

        //�ж�XOZ����û�г���
        if (!Allchunks.ContainsKey(GetRelaChunkLocation(pos))) { return true; }

        //�ж�Y����û�г���
        if (realLocation.y >= TerrainData.ChunkHeight) { return false; }


        //������Զ�����ײ
        if (blocktypes[targetBlock].isDIYCollision)
        {
            realLocation = CollisionOffset(realLocation, targetBlock);
            Vector3 OffsetrelaLocation = GetRelaPos(realLocation);

            if (OffsetrelaLocation != relaLocation)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //���ع��廹�ǿ���
        return blocktypes[Allchunks[GetRelaChunkLocation(new Vector3(pos.x, pos.y, pos.z))].voxelMap[(int)vec.x, (int)vec.y, (int)vec.z].voxelType].canBeChoose;

    }


    #endregion
    #region [ChunkGenerator]ָ������Ѱַ

    //ָ������Ѱַ
    public Vector3 AddressingBlock(Vector3 _start, int _direct)
    {
        Vector3 _address = _start;
        //print($"start: {_address}");

        for (int i = 0; i < TerrainData.ChunkHeight; i++)
        {
            byte _byte = GetBlockType(_address);
            if (_byte != VoxelData.Air)
            {
                //print($"���꣺{_address} , ����{_byte}");
                //���һ�������ڽ�
                if (_byte == VoxelData.Water)
                {
                    EditBlock(_address, VoxelData.Grass);
                }

                //Offset
                return _address + new Vector3(0.5f, 2f, 0.5f);
            }

            _address += VoxelData.faceChecks[_direct];
        }

        print("Ѱַʧ��");
        return _start;

    }

    //����һ����ʼ����ͳ�ʼ���򣬳�������������ChunkHeight������һ���ǿ�������
    public Vector3 LoopAndFindABestLocation(Vector3 _start, Vector3 _direct)
    {
        _direct.x = _direct.x > 0 ? 1 : 0;
        _direct.y = _direct.y > 0 ? 1 : 0;
        _direct.z = _direct.z > 0 ? 1 : 0;

        Vector3 _next = _start;

        //Loop
        for (int i = 0; i < TerrainData.ChunkHeight; i++)
        {
            // Check�������ǰλ�õķ������Ͳ��ǿ��������ظ�����
            if (GetBlockType(_next) != VoxelData.Air)
            {
                return _next;
            }

            // �ۻ��ƶ�λ��
            _next += _direct; // ʹ�ù�һ���ķ����������ƶ�
        }

        return _start;
    }

    #endregion
    #region [ChunkGenerator]���ؿ��ó�����

    public void GetSpawnPos(Vector3 _pos, out List<Vector3> _Spawns)
    {
        _Spawns = new List<Vector3>();
        Vector3 _ChunkLocation = GetRelaChunkLocation(_pos);

        //��ǰ����-û������
        if (!Allchunks.TryGetValue(_ChunkLocation, out Chunk _chunktemp))
            return;

        _chunktemp.GetSpawnPos(GetRelaPos(_pos), out List<Vector3> __Spawns);
        _Spawns = new List<Vector3>(__Spawns);
    }

    #endregion

    //Noise
    #region [NoiseGenerator]

    //������
    public float GetSimpleNoise(int _x, int _z, Vector3 _myposition)
    {
        float smoothNoise = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((_x + _myposition.x) * 0.01f, (_z + _myposition.z) * 0.01f));
        return smoothNoise;
    }

    //��ƫ�����������������ȷֲ���
    //_offset����ΪVector3(111f,222f)
    //_Scale��������:0.01Ϊ��������,0.1Ϊˮ��ɳ��ֲ�
    public float GetSimpleNoiseWithOffset(int _x, int _z, Vector3 _myposition, Vector2 _Offset, float _Scale)
    {
        float smoothNoise = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((_x + _myposition.x + _Offset.x) * _Scale, (_z + _myposition.z + _Offset.y) * _Scale));
        return smoothNoise;
    }

    //�������Ⱥϵ����
    public byte GetBiomeType(int _x, int _z, Vector3 _myposition)
    {

        float _A = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(0f, 0f, 0f));
        float _B = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(123f, 0f, 456f));
        float _C = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(789f, 0f, 123f));
        float _D = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(456f, 0f, 789f));

        ////ɳĮ
        //if (_C >= ����̶�Aridity)
        //{

        //    return 2;

        //}

        //else
        //{

        //    //��ԭ
        //    if (_B >= ��ά�ܶ�Density3d)
        //    {
        //        return 1;
        //    }

        //    //��ԭ
        //    else if (_A >= ����Ũ��OxygenDensity)
        //    {

        //        if (_D >= ����ʪ��MoistureLevel)
        //        {
        //            return 3;
        //        }
        //        else
        //        {
        //            return 0;
        //        }



        //    }
        //    else
        //    {
        //        //��������
        //        return 4;
        //    }

        //}

        //��ԭ
        if (_B >= ��ά�ܶ�Density3d)
        {
            return 1;
        }

        else
        {

            //ɳĮ
            if (_C >= ����̶�Aridity)
            {

                return 2;

            }

            //��ԭ
            else if (_A >= ����Ũ��OxygenDensity)
            {

                if (_D >= ����ʪ��MoistureLevel)
                {
                    return 3;
                }
                else
                {
                    return 0;
                }



            }
            else
            {
                //��������
                return 4;
            }

        }


    }

    //���ݸ���������Ⱥϵ����
    //��ɸ�����Ⱥϵ����
    public float GetTotalNoiseHigh_Biome(int _x, int _z, Vector3 _myposition, int _WorldType)
    {
        if (_x < 0 || _x > TerrainData.ChunkWidth || _z < 0 || _z > TerrainData.ChunkWidth)
        {
            print($"GetTotalNoiseHigh_Biome����,{_x},{_z}");
            return 128f;
        }

        if (_WorldType == TerrainData.Biome_SuperPlain)
        {
            return 0f;
        }


        //Ĭ��
        if (_WorldType == TerrainData.Biome_Default)
        {
            //Noise
            float noise_1 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[0].Noise_Scale_123.x, (float)(_z + _myposition.z) * biomenoisesystems[0].Noise_Scale_123.x);
            float noise_2 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[0].Noise_Scale_123.y, (float)(_z + _myposition.z) * biomenoisesystems[0].Noise_Scale_123.y);
            float noise_3 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[0].Noise_Scale_123.z, (float)(_z + _myposition.z) * biomenoisesystems[0].Noise_Scale_123.z);
            float noise = Mathf.Lerp(0f, 1f, noise_1 * biomenoisesystems[0].Noise_Rank_123.x + noise_2 * biomenoisesystems[0].Noise_Rank_123.y + noise_3 * biomenoisesystems[0].Noise_Rank_123.z);
            float noise_High = Mathf.Lerp(biomenoisesystems[0].HighDomain.x, biomenoisesystems[0].HighDomain.y, noise);

            //���ݶ���
            int BiomeType = -1;
            float BiomeIntensity = 0f;
            float _A = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(0f, 0f, 0f));
            float _B = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(123f, 0f, 456f));
            float _C = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(789f, 0f, 123f));
            float _D = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(456f, 0f, 789f));

            //��õ�ǰȺϵ
            //���Ⱥϵ���ǿ��
            //��ԭ
            if (_B >= ��ά�ܶ�Density3d)
            {
                BiomeType = TerrainData.Biome_Plateau;
                BiomeIntensity = Mathf.InverseLerp(��ά�ܶ�Density3d, 1f, _B);
            }
            else
            {

                if (_C >= ����̶�Aridity)
                {
                    BiomeType = TerrainData.Biome_Dessert;
                    BiomeIntensity = Mathf.InverseLerp(����̶�Aridity, 1f, _C);
                }
                //��ԭ
                else if (_A >= ����Ũ��OxygenDensity)
                {
                    if (_D >= ����ʪ��MoistureLevel)
                    {
                        BiomeType = TerrainData.Biome_Marsh;
                        BiomeIntensity = Mathf.InverseLerp(����ʪ��MoistureLevel, 1f, _D);
                    }
                    else
                    {
                        BiomeType = TerrainData.Biome_Plain;
                        BiomeIntensity = Mathf.InverseLerp(����Ũ��OxygenDensity, 1f, _A);
                    }
                }
                else
                {
                    BiomeType = TerrainData.Biome_Plain;
                    BiomeIntensity = Mathf.InverseLerp(����Ũ��OxygenDensity, 1f, _A);
                }

            }

            //BiomeType = 1;

            //���Ⱥϵ
            float Mixnoise_1 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[BiomeType].Noise_Scale_123.x, (float)(_z + _myposition.z) * biomenoisesystems[BiomeType].Noise_Scale_123.x);
            float Mixnoise_2 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[BiomeType].Noise_Scale_123.y, (float)(_z + _myposition.z) * biomenoisesystems[BiomeType].Noise_Scale_123.y);
            float Mixnoise_3 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[BiomeType].Noise_Scale_123.z, (float)(_z + _myposition.z) * biomenoisesystems[BiomeType].Noise_Scale_123.z);
            float Mixnoise = Mathf.Lerp(0f, 1f, noise_1 * biomenoisesystems[BiomeType].Noise_Rank_123.x + noise_2 * biomenoisesystems[BiomeType].Noise_Rank_123.y + noise_3 * biomenoisesystems[BiomeType].Noise_Rank_123.z);
            float Mixnoise_High = Mathf.Lerp(biomenoisesystems[BiomeType].HighDomain.x, biomenoisesystems[BiomeType].HighDomain.y, Mixnoise);

            float �������� = Mathf.Lerp(noise_High, Mixnoise_High, BiomeIntensity);

            return ��������;
            //return noise_High + �������� * ���������Ŵ���; 
        }

        else
        {
            //Noise
            float noise_1 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[_WorldType].Noise_Scale_123.x, (float)(_z + _myposition.z) * biomenoisesystems[_WorldType].Noise_Scale_123.x);
            float noise_2 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[_WorldType].Noise_Scale_123.y, (float)(_z + _myposition.z) * biomenoisesystems[_WorldType].Noise_Scale_123.y);
            float noise_3 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[_WorldType].Noise_Scale_123.z, (float)(_z + _myposition.z) * biomenoisesystems[_WorldType].Noise_Scale_123.z);
            float noise = Mathf.Lerp(0f, 1f, noise_1 * biomenoisesystems[_WorldType].Noise_Rank_123.x + noise_2 * biomenoisesystems[_WorldType].Noise_Rank_123.y + noise_3 * biomenoisesystems[_WorldType].Noise_Rank_123.z);
            float noise_High = Mathf.Lerp(biomenoisesystems[_WorldType].HighDomain.x, biomenoisesystems[_WorldType].HighDomain.y, noise);
            return noise_High;
        }


    }


    #endregion


    //Save
    #region [SaveGenerator]�浵����

    [Foldout("�浵ϵͳ", true)]
    [Header("����浵")]
    public bool isFinishSaving = false;
    public String savingPATH = ""; //�浵��Ŀ¼
    public WorldSetting worldSetting;
    public List<SavingData> TheSaving = new List<SavingData>(); //��ȡ�Ĵ浵
    public List<EditStruct> EditNumber = new List<EditStruct>(); //�������
    public bool isLoadSaving = false;


    //ɾ���浵
    public void DeleteSave(string savepath)
    {
        if (Directory.Exists(savepath))
        {
            try
            {
                // ɾ�������ļ�
                foreach (string file in Directory.GetFiles(savepath))
                {
                    File.Delete(file);
                    //Debug.Log($"Deleted file: {file}");
                }

                // �ݹ�ɾ��������Ŀ¼
                foreach (string directory in Directory.GetDirectories(savepath))
                {
                    DeleteSave(directory);
                }

                // ɾ����Ŀ¼
                Directory.Delete(savepath);
                //Debug.Log("���ɾ��");
            }
            catch (Exception ex)
            {
                // �����쳣������־��¼
                Debug.LogError($"ɾ��Ŀ¼ {savepath} ʱ����: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"ָ��·�� {savepath} ������.");
        }
    }

    //��ȡTheSaving������
    public int GetIndexOfChunkLocation(Vector3 location)
    {
        // ���� TheSaving �б�
        for (int i = 0; i < TheSaving.Count; i++)
        {
            var savingData = TheSaving[i];
            // ʹ�� SavingData �� ContainsChunkLocation ������� ChunkLocation
            if (savingData.ContainsChunkLocation(location))
            {
                return i; // ����ƥ���������
            }
        }

        return -1; // ���û���ҵ�ƥ����򷵻� -1
    }


    //����TheSaving�Ƿ����ChunkLocation
    public bool ContainsChunkLocation(Vector3 location)
    {
        // ���� TheSaving �б�
        foreach (var savingData in TheSaving)
        {
            // ʹ�� SavingData �� ContainsChunkLocation ������� ChunkLocation
            if (savingData.ContainsChunkLocation(location))
            {
                return true; // �ҵ�ƥ��� ChunkLocation
            }
        }

        return false; // û���ҵ�ƥ��� ChunkLocation
    }


    // ������Ҹ��µľ��巽��
    public List<EditStruct> WaitToAdd_EditList = new List<EditStruct>();
    public Coroutine updateEditNumberCoroutine;

    /// <summary>
    /// ע�ⷵ�ؾ�������
    /// </summary>
    /// <param name="RealPos"></param>
    /// <param name="targetBlocktype"></param>
    public void UpdateEditNumber(Vector3 RealPos, byte targetBlocktype)
    {
        // ���޸�ϸ��������World��
        // ת��RealPosΪ����Vector3�Ա������ֵ��key
        Vector3 intPos = new Vector3((int)RealPos.x, (int)RealPos.y, (int)RealPos.z);

        // �����Ƿ��Ѿ�������ͬ��editPos
        EditStruct existingEdit = EditNumber.Find(edit => edit.editPos == intPos);

        if (existingEdit != null)
        {
            // ������ڣ�����targetType
            existingEdit.targetType = targetBlocktype;
        }
        else
        {
            // ��������ڣ�����µ�EditStruct
            //print($"Edit����: {intPos} --- {targetBlocktype}");
            if (intPos.y >= 0)
            {
                EditNumber.Add(new EditStruct(intPos, targetBlocktype));
            }

        }
    }

    public void UpdateEditNumber(List<EditStruct> _EditList)
    {
        // ����µı༭�б��ȴ�����Ķ���β��
        WaitToAdd_EditList.AddRange(_EditList);
        // ���Э��δ���У�������Э��
        if (updateEditNumberCoroutine == null)
        {
            updateEditNumberCoroutine = StartCoroutine(_updateEditNumberCoroutine());
        }
    }

    IEnumerator _updateEditNumberCoroutine()
    {
        // ÿ�δ�������������⿨��
        int batchSize = 10;

        while (WaitToAdd_EditList.Count > 0)
        {
            // ÿ��ȡ����� batchSize �� EditStruct ��ͷ�����д���
            int count = Mathf.Min(batchSize, WaitToAdd_EditList.Count);

            for (int i = 0; i < count; i++)
            {
                // ȡ���б��еĵ�һ��Ԫ��
                EditStruct edit = WaitToAdd_EditList[0];

                //��������
                if (edit.targetType != VoxelData.BedRock)
                {
                    // ���༭����ӵ� EditNumber ��
                    UpdateEditNumber(edit.editPos, edit.targetType);
                }
                else
                {
                    print("��������");
                }

                // ��ͷ���Ƴ��Ѵ������
                WaitToAdd_EditList.RemoveAt(0);
            }

            // ��ͣһ֡������һ���Դ���̫����¿���
            yield return null;
        }

        // ������ɺ󣬽�Э�̱�����Ϊ null
        //print("null");
        updateEditNumberCoroutine = null;
    }

    // ��EditNumber����
    public void ClassifyWorldData()
    {
        foreach (var edittemp in EditNumber)
        {
            // ��ȡ��ǰ�޸����ڵ�����λ��
            Vector3 _ChunkLocation = GetRelaChunkLocation(edittemp.editPos);

            // ����Ƿ��� savingDatas ���ҵ���Ӧ�� ChunkLocation
            bool found = false;

            // �����Ƿ�����ͬ�� ChunkLocation
            foreach (var savingtemp in TheSaving)
            {
                if (savingtemp.ChunkLocation == _ChunkLocation)
                {
                    // ����ҵ�����Ӧ�� ChunkLocation����������λ�úͷ������͵� EditDataInChunk
                    savingtemp.EditDataInChunk[GetRelaPos(edittemp.editPos)] = edittemp.targetType;
                    found = true;
                    break;  // �ҵ���ֱ������ѭ��
                }
            }

            // ���û���ҵ���Ӧ�� ChunkLocation�����½�һ�� SavingData ����ӵ� savingDatas
            if (!found)
            {
                // �����µ� EditDataInChunk �ֵ䣬����ӵ�ǰ�����λ�úͷ�������
                Dictionary<Vector3, byte> newEditDataInChunk = new Dictionary<Vector3, byte>();
                newEditDataInChunk[GetRelaPos(edittemp.editPos)] = edittemp.targetType;

                // �����µ� SavingData ����ӵ� savingDatas
                SavingData newSavingData = new SavingData(_ChunkLocation, newEditDataInChunk);
                TheSaving.Add(newSavingData);
            }
        }

        SAVINGDATA(savingPATH);
    }

    //�浵
    public void SAVINGDATA(string savePath)
    {
        // ���´浵�ṹ��
        worldSetting.playerposition = managerhub.player.transform.position;
        worldSetting.playerrotation = managerhub.player.transform.rotation;
        worldSetting.gameMode = game_mode;
        string previouDate = worldSetting.date;
        worldSetting.date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        // ����һ����Ϊ "Saves" ���ļ���
        string savesFolderPath = Path.Combine(savePath, "Saves");
        if (!Directory.Exists(savesFolderPath))
        {
            Directory.CreateDirectory(savesFolderPath);
        }

        // �� "Saves" �ļ����´���һ���Դ浵���������������ļ���
        string folderPath = Path.Combine(savesFolderPath, worldSetting.date);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // ɾ��Saves�ļ���������ΪpreviouDate���ļ��У�������ҵ��Ļ�
        string oldFolderPath = Path.Combine(savesFolderPath, previouDate);
        if (Directory.Exists(oldFolderPath))
        {
            // ɾ�����ļ��м�����������
            Directory.Delete(oldFolderPath, true);
        }

        // �����е� SavingData ���ֵ�ת��Ϊ�б�
        foreach (var data in TheSaving)
        {
            data.EditDataInChunkList = new List<EditStruct>();
            foreach (var kvp in data.EditDataInChunk)
            {
                data.EditDataInChunkList.Add(new EditStruct(kvp.Key, kvp.Value));
            }
        }

        // �� worldSetting �� savingDatas ת��Ϊ JSON �ַ���
        string worldSettingJson = JsonUtility.ToJson(worldSetting, true);
        string savingDatasJson = JsonUtility.ToJson(new Wrapper<SavingData>(TheSaving), true);

        // �� JSON �ַ������浽ָ��·�����ļ�����
        File.WriteAllText(Path.Combine(folderPath, "WorldSetting.json"), worldSettingJson);
        File.WriteAllText(Path.Combine(folderPath, "SavingDatas.json"), savingDatasJson);



        Debug.Log("�����ѱ��浽: " + folderPath);
        isFinishSaving = true;
    }

    //��ȡȫ���浵
    public void LoadAllSaves(string savingPATH)
    {
        // ���� Saves Ŀ¼��·��
        string savesFolderPath = Path.Combine(savingPATH, "Saves");

        // ��� Saves Ŀ¼�Ƿ���ڣ�����������򴴽���
        if (!Directory.Exists(savesFolderPath))
        {
            try
            {
                Directory.CreateDirectory(savesFolderPath);
                //Debug.Log($"Saves �ļ����Ѵ���: {savesFolderPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"���� Saves �ļ���ʱ����: {ex.Message}");
                return; // �����ļ���ʧ��ʱ�˳�����
            }
        }

        // ��ȡ�浵Ŀ¼�µ������ļ���·��
        string[] saveDirectories;
        try
        {
            saveDirectories = Directory.GetDirectories(savesFolderPath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"��ȡ�浵Ŀ¼ʱ����: {ex.Message}");
            return; // �����쳣���˳�����
        }

        // ����ÿ���浵�ļ���
        foreach (string saveDirectory in saveDirectories)
        {
            // �����ǰ�浵�ļ�������
            string folderName = Path.GetFileName(saveDirectory);
            //Debug.Log($"Loading save from folder: {folderName}");

            // ���� LoadData ������ȡ��ǰ�浵
            try
            {
                WorldSetting _worldsetting = LoadWorldSetting(saveDirectory);
                managerhub.canvasManager.NewWorldGenerate(
                    _worldsetting.name,
                    _worldsetting.date,
                    _worldsetting.gameMode,
                    _worldsetting.worldtype,
                    _worldsetting.seed
                );
            }
            catch (Exception ex)
            {
                Debug.LogError($"���ش浵 {folderName} ʱ����: {ex.Message}");
            }
        }

        //Debug.Log($"Total saves loaded: {saveDirectories.Length}");
    }

    //�����浵����
    public WorldSetting LoadWorldSetting(string savePath)
    {
        // ���� WorldSetting.json �ļ�������·��
        string worldSettingPath = Path.Combine(savePath, "WorldSetting.json");

        // ��� WorldSetting.json �ļ��Ƿ����
        if (File.Exists(worldSettingPath))
        {
            // ��ȡ WorldSetting.json �ļ�������
            string worldSettingJson = File.ReadAllText(worldSettingPath);

            // �� JSON �ַ��������л�Ϊ WorldSetting ����
            return JsonUtility.FromJson<WorldSetting>(worldSettingJson);
        }
        else
        {
            // ����ļ������ڣ����������Ϣ������ null
            Debug.LogError("�Ҳ��� WorldSetting.json �ļ�");
            return null;
        }
    }

    //���ش浵
    public void LoadSavingData(string savePath)
    {
        isLoadSaving = true;

        // ���� SavingDatas.json �ļ�������·��
        string savingDatasPath = Path.Combine(savePath, "SavingDatas.json");

        // ��� SavingDatas.json �ļ��Ƿ����
        if (File.Exists(savingDatasPath))
        {
            // ��ȡ SavingDatas.json �ļ�������
            string savingDatasJson = File.ReadAllText(savingDatasPath);

            // �� JSON �ַ��������л�Ϊ Wrapper<SavingData> ����
            Wrapper<SavingData> wrapper = JsonUtility.FromJson<Wrapper<SavingData>>(savingDatasJson);

            // ��� wrapper �Ƿ�Ϊ null
            if (wrapper != null && wrapper.Items != null)
            {
                // �������л��� Items �б�ֵ�� TheSaving
                TheSaving = wrapper.Items;

                // ���� TheSaving �б��ָ�ÿ�� SavingData �����е��ֵ�
                foreach (var data in TheSaving)
                {
                    data.RestoreDictionary();

                    // Debug��ӡ
                    //Debug.Log($"Chunk Location: {data.ChunkLocation}");
                    //foreach (var pair in data.EditDataInChunk)
                    //{
                    //    Debug.Log($"Position: {pair.Key}, Type: {pair.Value}");
                    //}
                }

                worldSetting = LoadWorldSetting(savePath);


                //����һЩ����
                game_mode = worldSetting.gameMode;



            }
            else
            {
                // ��� wrapper �� wrapper.Items Ϊ null�����������Ϣ
                Debug.LogWarning("Wrapper<SavingData> �� Items �б�Ϊ null");
            }
        }
        else
        {
            // ����ļ������ڣ����������Ϣ
            Debug.LogError("�Ҳ��� SavingDatas.json �ļ�");
        }
    }


    #endregion

}