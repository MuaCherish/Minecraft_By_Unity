using JetBrains.Annotations;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
//using System.Diagnostics;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
//using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;
using System.Linq;




//ȫ����Ϸ״̬
public enum Game_State
{

    Start, Loading, Playing, Pause, Dead, Ending,

}

public enum GameMode
{

    Creative, Survival,

}


public enum DrawMode
{

    Block,Bush,Torch,Air,Water,SnowPower,

}

//public enum WorldType
//{
//    Ĭ��, ��ƽ̹����,��ԭȺϵ,��ԭȺϵ,ɳĮȺϵ,����Ⱥϵ,����Ⱥϵ,
//}




public class World : MonoBehaviour
{

    [Header("Transforms")]
    public CanvasManager canvasManager;
    public Player player;

    [Header("����浵")]
    public WorldSetting worldSetting;
    public Dictionary<Vector3, byte> EditNumber = new Dictionary<Vector3, byte>(); //�������
    public List<SavingData> savingDatas = new List<SavingData>();//���ձ�������

    [Header("��Ϸ״̬")]
    public Game_State game_state = Game_State.Start;
    public GameMode game_mode = GameMode.Survival;
    //public bool SuperPlainMode = false; 


    [Header("Material-��������")]
    public Material material;
    public Material material_Water;
    public BlockType[] blocktypes;
     

    [Header("World-��Ⱦ����")]
    [Tooltip("4���Ǳ߳�Ϊ4*16��������")] public int renderSize = 5;        //��Ⱦ����뾶,��renderSize*16f
    [Tooltip("2���ǽӽ�2*16��ʱ��ʼˢ������")] public float StartToRender = 1f;
    public float DestroySize = 7f;


    [Header("Cave-��Ѩϵͳ")]
    //noise3d_scaleԽ��Ѩ����Խ���أ�Խ��Խ��
    //cave_widthԽС��Ѩƽ����ȱ�С������̫�󣬲�Ȼȫ�Ƕ�
    //public bool debug_CanLookCave = false;
    public float noise3d_scale = 0.085f;
    public float cave_width = 0.45f;


    [Header("Ⱥϵ�������ʺ�����(ֵԽ��ΧԽС)")]
    public float ����Ũ��OxygenDensity;
    public float ��ά�ܶ�Density3d;
    public float ����̶�Aridity;
    public float ����ʪ��MoistureLevel;
    public BiomeNoiseSystem[] biomenoisesystems;


    [Header("���ʷֲ������ϵͳ(n%)(����Ϊ���֮n)")]
    public TerrainLayerProbabilitySystem terrainLayerProbabilitySystem;


    //���
    [Header("Player-��ҽŵ�����")]
    public Transform PlayerFoot;
    [HideInInspector]
    public byte ERROR_CODE_OUTOFVOXELMAP = 255;
    [HideInInspector]
    public Vector3 Start_Position = new Vector3(1600f, 127f, 1600f);


    //ȫ��Chunkλ��
    public Dictionary<Vector3, Chunk> Allchunks = new Dictionary<Vector3, Chunk>();
    private readonly object Allchunks_Lock = new object();


    //�ȴ���Ӷ���
    //private List<chunkWithsequence> WatingToCreateChunks = new List<chunkWithsequence>();
    private List<Vector3> WatingToCreate_Chunks = new List<Vector3>();


    //�ȴ�ɾ������
    private List<Vector3> WatingToRemove_Chunks = new List<Vector3>();
    private Chunk obj;


    //Э��
    [Header("Corountine-Э���ӳ�ʱ��")]
    public float InitCorountineDelay = 1f;
    public float CreateCoroutineDelay = 0.5f;
    public float RemoveCoroutineDelay = 0.5f;
    public float RenderDelay = 0.1f;
    public int Mesh_0_TaskCount = 0;


    [Header("ˮ��������Ⱦ�߳�")]
    private Thread myThread_Water;
    public int Delay_RenderFlowWater = 5;
    public float MaxDistant_RenderFlowWater = 5;


    [Header("[��ʱδ����]ǿ������Ⱦ�߳�(�ӳ�Ϊ����)")]
    public Thread myThread_Render;
    public int Delay_RenderMesh = 1000;
    public ConcurrentQueue<Chunk> WaitToRender_New = new ConcurrentQueue<Chunk>();


    [Header("Debug")]
    public bool �Ƿ�����Chunk���� = false;


    //���ɷ���
    private Vector3 Center_Now;
    private Vector3 Center_direction; //��������˷���


    //UI Manager
    //[HideInInspector]
    //public float initprogress = 0f;


    //Chunks����
    [HideInInspector]
    public GameObject Chunks;


    //һ���Դ���
    bool hasExec = true;
    bool hasExec_SetSeed = true;


    //Create && Remove Э��
    Coroutine CreateCoroutine;
    Coroutine RemoveCoroutine;


    //Render_0 && Render_1 Э��
    [HideInInspector] public bool RenderLock = false;
    public ConcurrentQueue<Chunk> WaitToRender = new ConcurrentQueue<Chunk>();
    public ConcurrentQueue<Chunk> WaitToRender_temp = new ConcurrentQueue<Chunk>();
    Coroutine Render_Coroutine;


    //Threading
    [HideInInspector]public bool MeshLock = false;
    public ConcurrentQueue<Chunk> WaitToCreateMesh = new ConcurrentQueue<Chunk>();
    Coroutine Mesh_Coroutine;

    //Flash
    public ConcurrentQueue<Chunk> WaitToFlashChunkQueue = new ConcurrentQueue<Chunk>();

    //Init
    //[HideInInspector] public bool InitError = false;


    //----------------------------------���ں���---------------------------------------




    private void Start()
    {

        //֡��
        Application.targetFrameRate = 90;

        InitWorld();

    }

    public void InitWorld()
    {
        //��ʼ��
        game_state = Game_State.Start;
        EditNumber = new Dictionary<Vector3, byte>();
        savingDatas = new List<SavingData>();
        renderSize = 5;
        StartToRender = 1f;
        DestroySize = 7f;
        Start_Position = new Vector3(1600f, 127f, 1600f);
        Allchunks = new Dictionary<Vector3, Chunk>();
        WatingToCreate_Chunks = new List<Vector3>();
        WatingToRemove_Chunks = new List<Vector3>();
        myThread_Render = null;
        WaitToRender_New = new ConcurrentQueue<Chunk>();
        �Ƿ�����Chunk���� = false;
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

        //Self
        if (Chunks == null)
        {
            Chunks = new GameObject();
            Chunks.name = "Chunks";
            Chunks.transform.SetParent(GameObject.Find("Environment").transform);
        }

        // ���� Chunks ���Ŀ¼�µ�����������
        foreach (Transform child in Chunks.transform)
        {
            Destroy(child.gameObject);
        }

        //��������
        terrainLayerProbabilitySystem.Seed = UnityEngine.Random.Range(0, 10000);

        //��ʼ������
        worldSetting = new WorldSetting(terrainLayerProbabilitySystem.Seed);
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
                //��ȡ��ǰģʽ
                //if (canvasManager.currentWorldType == 6)
                //{
                //    //SuperPlainMode = true;
                //}

                //worldSetting.worldtype = canvasManager.currentWorldType

                //��ʼ��ʼ��
                Update_CenterChunks();

                hasExec_SetSeed = false;
            }


        }


        //��Ϸ��ʼ
        if (game_state == Game_State.Playing)
        {

            if (hasExec)
            {

                // �������������Ļ����
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;

                //��겻����
                UnityEngine.Cursor.visible = false;

            }


            //����ƶ�ˢ��
            //�������16f
            if (GetVector3Length(PlayerFoot.transform.position - Center_Now) > (StartToRender * 16f) && GetVector3Length(PlayerFoot.transform.position - Center_Now) <= ((StartToRender + 1) * 16f))
            {

                //����Center
                Center_direction = VtoNormal(PlayerFoot.transform.position - Center_Now);
                Center_Now += Center_direction * VoxelData.ChunkWidth;

                //���Chunk
                AddtoCreateChunks(Center_direction);
                AddtoRemoveChunks(Center_direction);

            }
            //����ƶ���Զ����
            else if (GetVector3Length(PlayerFoot.transform.position - Center_Now) > ((StartToRender + 1) * 16f))
            {



                Update_CenterWithNoInit();




            }


        }

    }



    void OnApplicationQuit()
    {

        //������Ϸ
        game_state = Game_State.Ending;

        //print("Quit");
        RenderSettings.skybox.SetFloat("_Exposure", 0.69f);

        //�ȴ�Water�߳�
        if (myThread_Water != null && myThread_Water.IsAlive)
        {

            myThread_Water.Join(); // �ȴ��̰߳�ȫ����ֹ

        }

        //�ȴ�Render�߳�
        if (myThread_Render != null && myThread_Render.IsAlive)
        {

            myThread_Render.Join(); // �ȴ��̰߳�ȫ����ֹ

        }

    }




    //---------------------------------------------------------------------------------------










    //----------------------------------World Options---------------------------------------




    //���˵���ͼ
    //public void Start_Screen_Init()
    //{

    //    Chunk chunk_temp = new Chunk(new Vector3(5, 0, 2), this ,true, false);
    //    //Chunk chunk_temp1 = new Chunk(new Vector3(3, 0, 2), this, true);
    //    //Chunk chunk_temp2 = new Chunk(new Vector3(3, 0, 2), this, true);
        

    //    //for (float x = 0; x < 5; x ++)
    //    //{
    //    //    for (float z = 0; z < 5; z++)
    //    //    {
    //    //        Chunk chunk_temp3 = new Chunk(new Vector3(x, 0, z), this, true);
    //    //    }
    //    //}
    //    //GameObject chunkGameObject = new GameObject("TheMenuChunk");
    //    //Chunk chunk = chunkGameObject.AddComponent<Chunk>();
    //    //chunk.InitChunk(new Vector3(0, 0, 0), this);

    //}

    


    //��ʼ����ͼ
    IEnumerator Init_Map_Thread()
    {
        Center_Now = new Vector3(GetRealChunkLocation(PlayerFoot.transform.position).x, 0, GetRealChunkLocation(PlayerFoot.transform.position).z);

        //дһ��Э�̣�����������ع�Զ������
        
        
        //print($"Center:{Center_Now}");
        //print($"Foot:{PlayerFoot.transform.position}, ChunkFoot:{GetChunkLocation(PlayerFoot.transform.position)}");
        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.transform.position = Center_Now;
        //sphere.transform.localScale = new Vector3(2f, 2f, 2f);
        float temp = 0f;

        for (int x = -renderSize + (int)(Center_Now.x / VoxelData.ChunkWidth); x < renderSize + (int)(Center_Now.x / VoxelData.ChunkWidth); x++)
        {

            for (int z = -renderSize + (int)(Center_Now.z / VoxelData.ChunkWidth); z < renderSize + (int)(Center_Now.z / VoxelData.ChunkWidth); z++)
            {

                //Create
                CreateBaseChunk(new Vector3(x, 0, z));

                //ʣ����ȼ���
                float max = renderSize * renderSize * 4;
                temp++;
                canvasManager.Initprogress = Mathf.Lerp(0f, 0.9f, temp / max);

                yield return new WaitForSeconds(InitCorountineDelay);
            }

        }

        //���³�ʼ�����λ�ã����ô�ģ
        Init_Player_Location();

        //��Ϸ��ʼ
        yield return new WaitForSeconds(0.5f);
        canvasManager.Initprogress = 1f; 

        //�������Ż�Э��
        StartCoroutine(Chunk_Optimization());

        StartCoroutine(FlashChunkCoroutine());

        //����������Ⱦˮ�������߳�
        //myThread_Water = new Thread(new ThreadStart(Thread_AwaysUpdate_Water));
        //myThread_Water.Start();

        //������ȾMesh�߳�
        //myThread_Render = new Thread(new ThreadStart(Thread_RenderMesh));
        //myThread_Render.Start();

    }


    Coroutine Init_Map_Thread_NoInit_Coroutine;

    public void Update_CenterWithNoInit()
    {
        if (Init_Map_Thread_NoInit_Coroutine == null)
        {
            print("����ƶ�̫�죡Center_Now�Ѹ���");
            Init_Map_Thread_NoInit_Coroutine = StartCoroutine(Init_Map_Thread_NoInit());
            HideFarChunks();
        }
    }

    IEnumerator Init_Map_Thread_NoInit()
    {

        Center_Now = new Vector3(GetRealChunkLocation(PlayerFoot.transform.position).x, 0, GetRealChunkLocation(PlayerFoot.transform.position).z);

        for (int x = -renderSize + (int)(Center_Now.x / VoxelData.ChunkWidth); x < renderSize + (int)(Center_Now.x / VoxelData.ChunkWidth); x++)
        {

            for (int z = -renderSize + (int)(Center_Now.z / VoxelData.ChunkWidth); z < renderSize + (int)(Center_Now.z / VoxelData.ChunkWidth); z++)
            {

                //Create
                CreateBaseChunk(new Vector3(x, 0, z));

                yield return new WaitForSeconds(InitCorountineDelay);
            }

        }

        Init_Map_Thread_NoInit_Coroutine = null;

    }


    //������������
    public void Update_CenterChunks()
    {
    
        //update������������
        StartCoroutine(Init_Map_Thread());
    
    }


    public void HideFarChunks()
    {
        foreach (var temp in Allchunks)
        {
            if (GetVector3Length(PlayerFoot.transform.position - temp.Value.myposition) > (StartToRender * 16f))
            {
                temp.Value.HideChunk();
            }
        }
    }



    //�����Զ����
    IEnumerator Update_FarChunks()
    {

        //�����Զ������
        yield return null;

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



    //һֱ����ˮ���߳�
    void Thread_AwaysUpdate_Water()
    {
        int ���� = 0;
        int ���� = 0;

        //һֱѭ��
        while (game_state != Game_State.Ending)
        {

            lock (Allchunks_Lock)
            {

                //��������AllChunks
                foreach (var chunktemp in Allchunks)
                {

                    //����������ˮ������12�����ڣ������
                    if (chunktemp.Value.iHaveWater && GetVector3Length(chunktemp.Value.myposition - Center_Now) > MaxDistant_RenderFlowWater)
                    {

                        chunktemp.Value.Always_updateWater();
                        //print($"ˢ����{chunktemp.Value.name}");

                        ����++;

                    }

                }

            }

            




            //����5����
            ����++;
            print($"��{����}��ˢ�£�һ��ˢ��{����}��");
            ���� = 0;
            Thread.Sleep(Delay_RenderFlowWater * 1000);

        }

        Debug.LogError("Water�߳���ֹ");
        
    }




    //--------------------------------------------------------------------------------------





    //-----------------------------------Create Э��----------------------------------------




    //��ӵ��ȴ���Ӷ���
    void AddtoCreateChunks(Vector3 add_vec)
    {

        //ForWard
        if (add_vec == new Vector3(0, 0, 1))
        {

            add_vec = (Center_Now / VoxelData.ChunkWidth) + Center_direction * (renderSize - 1);

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

            add_vec = (Center_Now / VoxelData.ChunkWidth) + Center_direction * (renderSize);

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

            add_vec = (Center_Now / VoxelData.ChunkWidth) + Center_direction * (renderSize);

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

            add_vec = (Center_Now / VoxelData.ChunkWidth) + Center_direction * (renderSize - 1);

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
        Chunk chunk_temp = new Chunk(new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z)), this, true);

        //GameObject chunkGameObject = new GameObject($"{Mathf.FloorToInt(pos.x)}, 0, {Mathf.FloorToInt(pos.z)}");
        //Chunk chunktemp = chunkGameObject.AddComponent<Chunk>();
        //chunktemp.InitChunk(new Vector3(0, 0, 0), this);

        //��ӵ��ֵ�
        Allchunks.Add(pos, chunk_temp);

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

        //����Chunk
        Chunk chunk_temp = new Chunk(new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z)), this, false);

        //GameObject chunkGameObject = new GameObject($"{Mathf.FloorToInt(pos.x)}, 0, {Mathf.FloorToInt(pos.z)}");
        //Chunk chunktemp = chunkGameObject.AddComponent<Chunk>();
        //chunktemp.InitChunk(new Vector3(0, 0, 0), this);

        //��ӵ��ֵ�
        Allchunks.Add(pos, chunk_temp);

    }




    //--------------------------------------------------------------------------------------







    //-----------------------------------Remove Э��-----------------------------------------




    //��ӵ��ȴ�ɾ������
    void AddtoRemoveChunks(Vector3 add_vec)
    {

        //ForWard
        if (add_vec == new Vector3(0, 0, 1))
        {

            add_vec = (Center_Now / VoxelData.ChunkWidth) - Center_direction * (renderSize + 1);

            for (int i = -renderSize; i < renderSize; i++)
            {

                WatingToRemove_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));

            }

        }



        //Back
        if (add_vec == new Vector3(0, 0, -1))
        {

            add_vec = (Center_Now / VoxelData.ChunkWidth) - Center_direction * (renderSize);

            for (int i = -renderSize; i < renderSize; i++)
            {

                WatingToRemove_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));

            }

        }



        //Left
        if (add_vec == new Vector3(-1, 0, 0))
        {

            add_vec = (Center_Now / VoxelData.ChunkWidth) - Center_direction * (renderSize);

            for (int i = -renderSize; i < renderSize; i++)
            {

                WatingToRemove_Chunks.Add(add_vec + new Vector3(0, 0, (float)i));

            }

        }

        //Right
        if (add_vec == new Vector3(1, 0, 0))
        {

            add_vec = (Center_Now / VoxelData.ChunkWidth) - Center_direction * (renderSize + 1);

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



    void Chunk_HideOrRemove(Vector3 chunklocation)
    {

        //���������Χ��ж��,���������
        //if (GetVector3Length(PlayerFoot.transform.position - chunklocation) > (DestroySize * 16f))
        //{
        //    Allchunks.Remove(chunklocation);
        //    obj.DestroyChunk();


        //}
        //else
        //{
        //    obj.HideChunk();
        //}

        obj.HideChunk();


    }




    //---------------------------------------------------------------------------------------





    //-----------------------------------Render Э��-----------------------------------------




    //��ȾЭ�̳�
    void RenderCoroutineManager()
    {

        if (WaitToRender.Count != 0 && Render_Coroutine == null)
        {

            //print($"������ȾЭ��");
            Render_Coroutine = StartCoroutine(Render_0());

        }


    }

    //һ����ȾЭ��
    IEnumerator Render_0()
    {

        while (true)
        {

            //Queue
            WaitToRender.TryDequeue(out Chunk chunktemp);

            //print($"{GetChunkLocation(chunktemp.myposition)}��ʼ��Ⱦ");

            //CreateMesh
            if (chunktemp.isReadyToRender)
            {

                chunktemp.CreateMesh();

            }

            //Empty
            if (WaitToRender.Count == 0)
            {

                //print($"����Ϊ�գ�ֹͣЭ��");
                Render_Coroutine = null;
                RenderLock = false;
                break;

            }

            yield return new WaitForSeconds(RenderDelay);

        }

    }

    //��λΪms������ÿ����Ⱦ����̬�ı���Ⱦʱ��
    //void dynamicRandertime(float nowtime)
    //{

    //    if (nowtime > RenderDelay)
    //    {

    //        RenderDelay = nowtime;

    //    }

    //}



    //MeshЭ��
    void CreateMeshCoroutineManager()
    {

        if (WaitToCreateMesh.Count != 0 && Mesh_Coroutine == null)
        {

            Mesh_Coroutine = StartCoroutine(Mesh_0());

        }

    }


    IEnumerator Mesh_0()
    {

        while(true)
        {

            if (MeshLock == false)
            {

                MeshLock = true;

                WaitToCreateMesh.TryDequeue(out Chunk chunktemp);

                //print($"{GetChunkLocation(chunktemp.myposition)}��ӵ�meshQueue");

                //Mesh�߳�
                Thread myThread = new Thread(new ThreadStart(chunktemp.UpdateChunkMesh_WithSurround));
                myThread.Start();

                if (WaitToCreateMesh.Count == 0)
                {

                    Mesh_Coroutine = null;
                    break;

                }


                

            }

            Mesh_0_TaskCount = WaitToCreateMesh.Count;
            //print("WaitToCreateMesh.Count");
            yield return new WaitForSeconds(RenderDelay);


        }



       
        

        
    }



    //�µ���Ⱦ����
    void Thread_RenderMesh()
    {

        while (game_state != Game_State.Ending)
        {

            if (WaitToRender_New.TryDequeue(out Chunk chunktemp))
            {

                if (chunktemp.isReadyToRender)
                {

                    chunktemp.CreateMesh();

                }

            }

            Thread.Sleep(Delay_RenderMesh);

        }

        Debug.LogError("Render�߳���ֹ");

    }



    //---------------------------------------------------------------------------------------






    //----------------------------------Player Options---------------------------------------

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
        //Ĭ��
        if (_WorldType == VoxelData.Biome_Default)
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
                BiomeType = VoxelData.Biome_Plateau;
                BiomeIntensity = Mathf.InverseLerp(��ά�ܶ�Density3d, 1f, _B);
            }
            else
            {

                if (_C >= ����̶�Aridity)
                {
                    BiomeType = VoxelData.Biome_Dessert;
                    BiomeIntensity = Mathf.InverseLerp(����̶�Aridity, 1f, _C);
                }
                //��ԭ
                else if (_A >= ����Ũ��OxygenDensity)
                {
                    if (_D >= ����ʪ��MoistureLevel)
                    {
                        BiomeType = VoxelData.Biome_Marsh;
                        BiomeIntensity = Mathf.InverseLerp(����ʪ��MoistureLevel, 1f, _D);
                    }
                    else
                    {
                        BiomeType = VoxelData.Biome_Plain;
                        BiomeIntensity = Mathf.InverseLerp(����Ũ��OxygenDensity, 1f, _A);
                    }
                }
                else
                {
                    BiomeType = VoxelData.Biome_Plain;
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


    //��ʼ������λ��
    void Init_Player_Location()
    {

        Start_Position = new Vector3(GetRealChunkLocation(PlayerFoot.transform.position).x, VoxelData.ChunkHeight - 1f, GetRealChunkLocation(PlayerFoot.transform.position).z);
        

        //��<1600,63,1600>���±�����ֱ�������������
        while (GetBlockType(Start_Position) == VoxelData.Air)
        {

            Start_Position.y -= 1f;
        
        }

        Start_Position.y += 2f;
        
        player.InitPlayerLocation();
        //print(Start_Position);
    }


    //Vector3 --> ����������
    public Vector3 GetChunkLocation(Vector3 vec)
    {

        return new Vector3((vec.x - vec.x % VoxelData.ChunkWidth) / VoxelData.ChunkWidth, 0, (vec.z - vec.z % VoxelData.ChunkWidth) / VoxelData.ChunkWidth);

    }


    public Vector3 GetRealChunkLocation(Vector3 vec)
    {

        return new Vector3(16f * ((vec.x - vec.x % VoxelData.ChunkWidth) / VoxelData.ChunkWidth), 0, 16f * ((vec.z - vec.z % VoxelData.ChunkWidth) / VoxelData.ChunkWidth));

    }

    //Vector3 --> ���������
    public Chunk GetChunkObject(Vector3 pos)
    {

        Allchunks.TryGetValue(GetChunkLocation(pos), out Chunk chunktemp);
        return chunktemp;

    }


    //Vector3 --> ��������������
    public Vector3 GetRelalocation(Vector3 vec)
    {

        return new Vector3(Mathf.FloorToInt(vec.x % VoxelData.ChunkWidth), Mathf.FloorToInt(vec.y) % VoxelData.ChunkHeight, Mathf.FloorToInt(vec.z % VoxelData.ChunkWidth));

    }

     
    //���ط�������
    public byte GetBlockType(Vector3 pos)
    {

        if(Allchunks.TryGetValue(GetChunkLocation(pos), out Chunk chunktemp))
        {
            if ((int)GetRelalocation(pos).y >= VoxelData.ChunkHeight || (int)GetRelalocation(pos).y < 0)
            {

                //isBlock = false;
                //isnearblock = false;
                print("��������쳣��");
                return ERROR_CODE_OUTOFVOXELMAP;

            }

            byte block_type = chunktemp.voxelMap[(int)GetRelalocation(pos).x, (int)GetRelalocation(pos).y, (int)GetRelalocation(pos).z].voxelType;

            return block_type;
        }

        //��������ˢ������
        //if (chunktemp == null)
        //{
        //    return VoxelData.notChunk;
        //}

        //�����������ڣ���Yֵ̫��
        print("���Yֵ̫�ߣ�");
        return ERROR_CODE_OUTOFVOXELMAP;



    }




    //---------------------------------------------------------------------------------------







    //------------------------------------����------------------------------------------------







    // �浵ϵͳ
    public void SaveWorldData()
    {
        foreach (var edittemp in EditNumber)
        {
            // ��ȡ��ǰ�޸����ڵ�����λ��
            Vector3 _ChunkLocation = GetChunkLocation(edittemp.Key);

            // ����Ƿ��� savingDatas ���ҵ���Ӧ�� ChunkLocation
            bool found = false;

            // �����Ƿ�����ͬ�� ChunkLocation
            foreach (var savingtemp in savingDatas)
            {
                if (savingtemp.ChunkLocation == _ChunkLocation)
                {
                    // ����ҵ�����Ӧ�� ChunkLocation����������λ�úͷ������͵� EditDataInChunk
                    savingtemp.EditDataInChunk[GetRelalocation(edittemp.Key)] = edittemp.Value;
                    found = true;
                    break;  // �ҵ���ֱ������ѭ��
                }
            }

            // ���û���ҵ���Ӧ�� ChunkLocation�����½�һ�� SavingData ����ӵ� savingDatas
            if (!found)
            {
                // �����µ� EditDataInChunk �ֵ䣬����ӵ�ǰ�����λ�úͷ�������
                Dictionary<Vector3, byte> newEditDataInChunk = new Dictionary<Vector3, byte>();
                newEditDataInChunk[GetRelalocation(edittemp.Key)] = edittemp.Value;

                // �����µ� SavingData ����ӵ� savingDatas
                SavingData newSavingData = new SavingData(_ChunkLocation, newEditDataInChunk);
                savingDatas.Add(newSavingData);
            }
        }

        // ��ӡ savingDatas����ѡ�����ڵ��ԣ�
        //foreach (var savingtemp in savingDatas)
        //{
        //    Debug.Log($"Chunk: {savingtemp.ChunkLocation}, Edits: {savingtemp.EditDataInChunk.Count}");

        //    //for (int i = 0;i < savingtemp.EditDataInChunk.Count; i ++)
        //    //{
        //    //    Debug.Log($"Chunk: {savingtemp.ChunkLocation}, Edits: {savingtemp.EditDataInChunk.Count}");
        //    //} 


        //}
    }




    //������λ������
    Vector3 VtoNormal(Vector3 v)
    {

        if (v.x >= 0)
        {

            if (v.z >= 0)
            {

                if (Mathf.Abs(v.x) >= Mathf.Abs(v.z))
                {

                    return new Vector3(1, 0, 0);

                }
                else
                {

                    return new Vector3(0, 0, 1);

                }

            }
            else
            {

                if (Mathf.Abs(v.x) >= Mathf.Abs(v.z))
                {
                
                    return new Vector3(1, 0, 0);
                
                }
                else
                {
                
                    return new Vector3(0, 0, -1);
                
                }

            }

        }
        else
        {

            if (v.z >= 0)
            {
                
                if (Mathf.Abs(v.x) >= Mathf.Abs(v.z))
                {
                
                    return new Vector3(-1, 0, 0);
                
                }
                else
                {

                    return new Vector3(0, 0, 1);
                
                }

            }
            else
            {

                if (Mathf.Abs(v.x) >= Mathf.Abs(v.z))
                {

                    return new Vector3(-1, 0, 0);

                }
                else
                {

                    return new Vector3(-1, 0, 0);

                }

            }

        }

    }



    //��Vector3��2d����
    float GetVector3Length(Vector3 vec)
    {
        Vector2 vector2 = new Vector2(vec.x, vec.z);
        return vector2.magnitude;
    }





    //----------------------------------------------------------------------------------------


    public Vector3 testPos;

    //�������ײ�еķ����ж�
    //true������ײ
    public bool CheckForVoxel(Vector3 pos)
    {
        //if (GetBlockType(pos) == VoxelData.Wood)
        //{
        //    print("");
        //}

        //�����������
        Vector3 vec = GetRelalocation(new Vector3(pos.x, pos.y, pos.z));
        
        //�ж�XOZ����û�г���
        if (!Allchunks.ContainsKey(GetChunkLocation(pos))) { return true; }

        //�ж�Y����û�г���
        if (vec.y >= VoxelData.ChunkHeight) { return false; }

        //С��0
        if (vec.y < 0) { return false; }

        //ʶ���ש�㷨
        //if (GetBlockType(pos) == VoxelData.Wood)
        //{
        //    print("Wood");
        //    Vector3 _vec = pos;
            
             
        //    if (pos.y - (int)pos.y > 0.5f)
        //    {
        //        _vec += new Vector3(0f,0.5f,0f);
                
        //        testPos = _vec;
        //    } 

        //    _vec = GetRelalocation(_vec);
        //    return blocktypes[Allchunks[GetChunkLocation(pos)].voxelMap[(int)_vec.x, (int)_vec.y, (int)_vec.z].voxelType].isSolid;
            
        //}

        //���ع��廹�ǿ���
        return blocktypes[Allchunks[GetChunkLocation(new Vector3(pos.x, pos.y, pos.z))].voxelMap[(int)vec.x, (int)vec.y, (int)vec.z].voxelType].isSolid;

    }

    //���ø��������
    public bool eyesCheckForVoxel(Vector3 pos)
    {

        if (!Allchunks.ContainsKey(GetChunkLocation(pos))) { return false; }

        //�����������
        Vector3 vec = GetRelalocation(new Vector3(pos.x, pos.y, pos.z));

        //�ж�XOZ����û�г���
        if (!Allchunks.ContainsKey(GetChunkLocation(pos))) { return true; }

        //�ж�Y����û�г���
        if (vec.y >= VoxelData.ChunkHeight) { return false; }

        //���ӷ���true
        //if (GetBlockType(pos) == VoxelData.Bamboo) { return true; }

        //���ع��廹�ǿ���
        return blocktypes[Allchunks[GetChunkLocation(new Vector3(pos.x, pos.y, pos.z))].voxelMap[(int)vec.x, (int)vec.y, (int)vec.z].voxelType].canBeChoose;

    }

    // �����˷�
    public Vector3 ComponentwiseMultiply(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    // ���� Vector2 �ķ����˷�
    public Vector2 ComponentwiseMultiply(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x * b.x, a.y * b.y);
    }

    ////����SavedataStruct
    //public void UpdateSaveList(Vector3 _C,Dictionary<Vector3,byte> _E)
    //{
    //    foreach (var temp in SaveList)
    //    {
    //        if (temp.ChunkLocation == _C)
    //        {
    //            temp.EditData.Add(_E);
    //            print("���ҵ�");
    //        }
    //    }

    //    //����Ҳ���������
    //    SavaDataStruct dataStruct = new SavaDataStruct();
    //    dataStruct.ChunkLocation = _C;
    //    dataStruct.EditData.Add(_E);
    //    SaveList.Add(dataStruct);
    //    print("δ�ҵ�");
    //}


}


//�ṹ��BlockType
//�洢��������+���Ӧ��UV
[System.Serializable]
public class BlockType
{

    public string blockName;
    public float DestroyTime;
    public bool isSolid;       //�Ƿ���赲���
    public bool isTransparent; //�ܱ߷����Ƿ����޳�
    public bool canBeChoose;   //�Ƿ�ɱ��������鲶׽��
    public bool candropBlock;  //�Ƿ���䷽��


    [Header("Sprite")]
    public Sprite icon;
    public Texture texture;
    public Sprite dropsprite;
    public Sprite top_dropsprite;
    public Material Particle_Material;


    [Header("Clips")]
    public AudioClip[] walk_clips = new AudioClip[2]; 
    public AudioClip broking_clip;
    public AudioClip broken_clip;


    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;


    [Header("DrawMode")]
    public DrawMode DrawMode;


    //��ͼ�е��������
    public int GetTextureID(int faceIndex)
    {

        switch (faceIndex)
        {

            case 0:
                return backFaceTexture;

            case 1:
                return frontFaceTexture;

            case 2:
                return topFaceTexture;

            case 3:
                return bottomFaceTexture;

            case 4:
                return leftFaceTexture;

            case 5:
                return rightFaceTexture;

            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;


        }

    }

}


//��������ṹ��
public class VoxelStruct
{
    public byte voxelType = VoxelData.Air;

    //�����ɵ���������
    public bool up = true;
}


//Ⱥϵϵͳ
[System.Serializable]
public class BiomeNoiseSystem
{
    public string BiomeName;
    public Color BiomeColor;
    public Vector2 HighDomain;
    public Vector3 Noise_Scale_123;
    public Vector3 Noise_Rank_123;
}


//���ʷֲ������ϵͳ
//TerrainLayerProbabilitySystem
[System.Serializable]
public class TerrainLayerProbabilitySystem
{
    //seed
    public bool isRandomSeed = true;
    public int Seed = 0;

    //level
    public float sea_level = 60;
    public float Snow_Level = 100;

    //tree
    public int Normal_treecount;
    public int ������ľ��������Forest_treecount;
    public int TreeHigh_min = 5;
    public int TreeHigh_max = 7;

    //random
    public float Random_Bush;
    public float Random_Bamboo;
    public float Random_BlueFlower;
    public float Random_WhiteFlower1;
    public float Random_WhiteFlower2;
    public float Random_YellowFlower;

    //coal
    public float Random_Coal;
    public float Random_Iron;
    public float Random_Gold;
    public float Random_Blue_Crystal;
    public float Random_Diamond;
}



// һ��������WorldSetting
[Serializable]
public class WorldSetting
{
    public String name = "�µ�����";
    public int seed = 0;
    public GameMode gameMode = GameMode.Survival;
    public int worldtype = VoxelData.Biome_Default;


    public WorldSetting(int _seed)
    {
        this.seed = _seed;
    }
}


//[System.Serializable]
//public class EditStruct
//{
//    public Vector3 ChunkLocation;
//    public Vector3 RelativeLocation;
//    public byte EditType;

//    public EditStruct(Vector3 _ChunkLocation, Vector3 _RelativeLocation, byte _EditType)
//    {
//        ChunkLocation = _ChunkLocation;
//        RelativeLocation = _RelativeLocation;
//        EditType = _EditType;
//    }

//}


[System.Serializable]
public class SavingData
{
    public Vector3 ChunkLocation;
    public Dictionary<Vector3, byte> EditDataInChunk = new Dictionary<Vector3, byte>();

    public SavingData(Vector3 _vec, Dictionary<Vector3, byte> _D)
    {
        ChunkLocation = _vec;
        EditDataInChunk = _D;
    }
}
