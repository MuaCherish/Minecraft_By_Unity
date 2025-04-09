using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;
using System.Collections;
using static MC_Static_Math;
using System.IO;
using UnityEditorInternal;
using Homebrew;

public class MC_Service_World : MonoBehaviour
{

    #region ���ں���

    ManagerHub managerhub;
    MC_Service_Saving Service_Saving;
    Player player;

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        Service_Saving = managerhub.Service_Saving;
        player = managerhub.player;
    }


    bool hasExec_Start = true;
    bool hasExec_Loading = true;
    bool hasExec_Playing = true;
    bool hasExec_Pause = true;
    private void Update()
    {
        Handle_AlwaysUpdate();


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

    void Handle_AlwaysUpdate()
    {
        ThreadUpdateController();
    }


    void Handle_GameState_Start()
    {
        if (hasExec_Start)
        {
            InitWorldManager();
            hasExec_Start = false;
            Service_Saving.isFinishUpdateEditNumber = false;
        }


    }

    void Handle_GameState_Loading()
    {
        if (hasExec_Loading)
        {

            InitStartTime = Time.time;

            //��ʼ��ʼ��
            Update_CenterChunks(true);

            hasExec_Loading = false;
        }
    }

    void Handle_GameState_Playing()
    {
        if (hasExec_Playing)
        {
            MC_Static_Unity.LockMouse(true);
            hasExec_Playing = false;
        }

        ChunkUpdateController();

    }

    void Handle_GameState_Pause()
    {
        if (hasExec_Pause)
        {
            hasExec_Pause = false;
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

    #region ����

    //��ֲ
    [Foldout("״̬", true)]
    [Header("��Ϸ״̬")][ReadOnly] public Game_State game_state = Game_State.Start;
    [Header("��Ϸģʽ")][ReadOnly] public GameMode game_mode = GameMode.Survival;

    //������Ա�ΪResources����ȡ
    [Foldout("����", true)]
    [Header("���β���")] public Material material;
    [Header("ˮ�����")] public Material material_Water;
    [Header("Items")] public BlockType[] blocktypes;
    [Header("Items����")] public BlockType[] BackUp_blocktypes;
    [Header("BlockTexture(���ڵ�����)")] public Texture2D atlasTexture;

    //���Է�װΪ��Ϸ����
    //������ýṹ��������������ۺ�����
    [Foldout("������Ⱦ", true)]
    [Header("������Ⱦ���ĵ�")] public Vector3 Center_Now;
    [Header("������Ⱦ����")] public Vector3 Center_direction; //��������˷���
    [Header("Chunk��Ⱦ�뾶(������=r^2*4)")] public int renderSize = 5;
    [Header("��ʼ���µ���С������")] public float StartToRender = 1f;
    [Header("��ʼ���ٵ����뾶")] public float DestroySize = 7f;

    //������Ϸ������
    [Foldout("Ⱥϵ�������ʺ͵�����������", true)]
    public BiomeProperties _biomeProperties;

    //�����߼�
    [Foldout("���", true)]
    public bool hasExec_RandomPlayerLocation = true;
    [Header("��ҳ�����")] public Vector3 Start_Position = new Vector3(1600f, 127f, 1600f);

    //��ֲ
    [Foldout("Debug", true)]
    [Header("�Ƿ�����Chunk����")] public bool isGenChunkSurrendFace = false;

    //Э�̸�ΪUpdaet����
    //Ū����߳�����Щ����
    //��Queue��Ϊ����������
    [Foldout("���ݽṹ", true)]
    public Chunk obj;//?

    //����
    public bool MeshLock = false;
    public bool RenderLock = false;
    public int Delay_RenderMesh = 1000;

    //Э��
    public Coroutine Render_Coroutine;
    public Coroutine Mesh_Coroutine;
    public Coroutine Init_Map_Thread_NoInit_Coroutine;
    public Coroutine Init_MapCoroutine;
    public Coroutine CreateCoroutine;
    public Coroutine RemoveCoroutine;


    [Foldout("Debug", true)]
    float InitStartTime; //���鿪ʼ��Ⱦʱ��
    float InitEndTime; //���������Ⱦʱ��

    //Ӧ��û����
    public Dictionary<Vector3, Chunk> Allchunks = new Dictionary<Vector3, Chunk>();
    public readonly object Allchunks_Lock = new object();
    public Thread myThread_Render;
    public List<Vector3> WatingToCreate_Chunks = new List<Vector3>();
    public List<Vector3> WatingToRemove_Chunks = new List<Vector3>();
    public ConcurrentQueue<Chunk> WaitToCreateMesh = new ConcurrentQueue<Chunk>();
    public ConcurrentQueue<Chunk> WaitToRender = new ConcurrentQueue<Chunk>();
    public ConcurrentQueue<Chunk> WaitToRender_temp = new ConcurrentQueue<Chunk>();
    public ConcurrentQueue<Chunk> WaitToRender_New = new ConcurrentQueue<Chunk>();
    public ConcurrentQueue<Chunk> WaitToFlashChunkQueue = new ConcurrentQueue<Chunk>();

    #endregion

    #region ��Ϸ�����߼�����

    /// <summary>
    /// Init
    /// </summary>
    public void InitWorldManager()
    {
        hasExec_RandomPlayerLocation = true;
        game_mode = GameMode.Survival;

        Service_Saving.isLoadSaving = false;
        isGenChunkSurrendFace = managerhub.�Ƿ�����Chunk����;

        // ʹ�� persistentDataPath ��Ϊ��Ŀ¼
        Service_Saving.savingPATH = Path.Combine(Application.persistentDataPath);

        // ȷ��Ŀ¼����
        if (!Directory.Exists(Service_Saving.savingPATH))
        {
            Directory.CreateDirectory(Service_Saving.savingPATH);
        }

        // ��ӡĿ¼·����ȷ��
        //Debug.Log("�浵Ŀ¼: " + savingPATH);


        //��ʼ��
        Start_Position = new Vector3(1600f, 127f, 1600f);

        game_state = Game_State.Start;
        Service_Saving.TheSaving = new List<SavingData>();
        Service_Saving.EditNumber = new List<EditStruct>();
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
        GameObject ChunkParent = SceneData.GetChunkParent();
        foreach (Transform child in ChunkParent.transform)
        {
            Destroy(child.gameObject);
        }

        //-------˳���ܱ仯------------------
        _biomeProperties.terrainLayerProbabilitySystem.Seed = UnityEngine.Random.Range(0, 100000000);
        Service_Saving.worldSetting = new WorldSetting(_biomeProperties.terrainLayerProbabilitySystem.Seed);
        UnityEngine.Random.InitState(Service_Saving.worldSetting.seed);
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

    /// <summary>
    /// ��������
    /// </summary>
    void ChunkUpdateController()
    {
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

    /// <summary>
    /// ʱ�̼���߳�
    /// </summary>
    void ThreadUpdateController()
    {
        CreateMeshCoroutineManager(); //Mesh�̳߳�פ
        RenderCoroutineManager();//��Ⱦ�̳߳�פ
    }

    #endregion

    #region ��ʼ����ͼ

    public IEnumerator Init_Map_Thread(bool _isInitPlayerLocation)
    {
        //ȷ��Chunk���ĵ�
        GetChunkCenterNow();

        //��ʼ�����鲢��ӽ�����
        float temp = 0f;
        for (int x = -renderSize + (int)(Center_Now.x / TerrainData.ChunkWidth); x < renderSize + (int)(Center_Now.x / TerrainData.ChunkWidth); x++)
        {
            for (int z = -renderSize + (int)(Center_Now.z / TerrainData.ChunkWidth); z < renderSize + (int)(Center_Now.z / TerrainData.ChunkWidth); z++)
            {
                CreateBaseChunk(new Vector3(x, 0, z));
                temp++;
                float max = renderSize * renderSize * 4;
                managerhub.canvasManager.Initprogress = Mathf.Lerp(0f, 0.9f, temp / max);
                yield return new WaitForSeconds(0.01f);
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

        if (Service_Saving.isLoadSaving)
        {
            Center_Now = new Vector3(GetRealChunkLocation(Service_Saving.worldSetting.playerposition).x, 0, GetRealChunkLocation(Service_Saving.worldSetting.playerposition).z);
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

    #region ��̬���ص�ͼ

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

                yield return new WaitForSeconds(0.01f);
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
    public void AddtoCreateChunks(Vector3 add_vec)
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

            yield return new WaitForSeconds(0.001f);


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
    public void CreateBaseChunk(Vector3 pos)
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
        if (Service_Saving.ContainsChunkLocation(_ChunkLocation))
        {
            _chunk_temp = new Chunk(_ChunkLocation, managerhub, false, Service_Saving.TheSaving[Service_Saving.GetIndexOfChunkLocation(_ChunkLocation)].EditDataInChunkList);
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
        if (Service_Saving.ContainsChunkLocation(_ChunkLocation))
        {
            _chunk_temp = new Chunk(_ChunkLocation, managerhub, false, Service_Saving.TheSaving[Service_Saving.GetIndexOfChunkLocation(_ChunkLocation)].EditDataInChunkList);
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
    public void AddtoRemoveChunks(Vector3 add_vec)
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

            yield return new WaitForSeconds(0.001f);


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

    #region ��������(ʲô��ʺ����)

    void Chunk_HideOrRemove(Vector3 chunklocation)
    {
        obj.HideChunk();
    }

    #endregion

    #region ��Ⱦ����

    //��ȾЭ�̳�
    public void RenderCoroutineManager()
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
            yield return new WaitForSeconds(0.001f);
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
    public void CreateMeshCoroutineManager()
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

            //Mesh_0_TaskCount = WaitToCreateMesh.Count;
            //print("WaitToCreateMesh.Count");
            yield return new WaitForSeconds(0.001f);


        }

    }

    #endregion

    #region ����Chunk����

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

    #region ��ȡ����

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
        if (MC_Static_Chunk.isOutOfChunkRange(pos))
            return 0;

        //��ȡ�������
        Vector3 _vec = GetRelaPos(pos);

        //��ȡBlock����
        byte block_type = chunktemp.GetBlock((int)_vec.x, (int)_vec.y, (int)_vec.z).voxelType;

        //Return
        return block_type;

    }

    #endregion

    #region �ṹ����(δӦ��)

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

    #region �޸ķ���

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

    #region ������

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

    #region ָ������Ѱַ

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

    #region ���ؿ��ó�����

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

}
