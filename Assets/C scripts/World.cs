using JetBrains.Annotations;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

//using System.Diagnostics;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
    Block,Bush,Torch,Air,Water,
}

public class World : MonoBehaviour
{
    [Header("Transforms")]
    public TMP_InputField input_Seed;
    public TMP_InputField input_RenderSize;

    [Header("��Ϸ״̬")]
    public Game_State game_state = Game_State.Start;
    public GameMode game_mode = GameMode.Survival; 

    [Header("Material-��������")]
    public Material material;
    public BlockType[] blocktypes;

    [Header("World-��Ⱦ����")]
    [Tooltip("4���Ǳ߳�Ϊ4*16��������")]
    public int renderSize = 5; //��Ⱦ����뾶,��renderSize*16f
    [Tooltip("2���ǽӽ�2*16��ʱ��ʼˢ������")]
    public float StartToRender = 1f;
    public float DestroySize = 7f;

    [Header("Biome-ƽԭ����")]
    //noise2dԽС����������Խ�������������֮����ȸ���Ȼ
    //ֵԽС��������С������Խ����
    //����Խ��Խ��
    public float noise2d_scale_smooth = 0.01f;
    public float noise2d_scale_steep = 0.04f;

    [Header("Cave-��Ѩϵͳ")]
    //noise3d_scaleԽ��Ѩ����Խ���أ�Խ��Խ��
    //cave_widthԽС��Ѩƽ����ȱ�С������̫�󣬲�Ȼȫ�Ƕ�
    public bool debug_CanLookCave = false;
    public float noise3d_scale = 0.085f;
    public float cave_width = 0.45f;


    [Header("Chunk-�ֲ�ṹ")]
    public bool isRandomSeed = true;
    public int Seed = 0;
    [Range(0, 60)]
    public float soil_min = 15;
    [Range(0, 60)]
    public float soil_max = 55;
    [Range(0, 60)]
    public float sea_level = 30;
    public int TreeCount = 5;
    public int TreeHigh_min = 5;
    public int TreeHigh_max = 7;



    [Header("���ɸ���(n��֮һ)")]
    public int Random_Coal;
    public int Random_Iron;
    public int Random_Gold;
    public int Random_Blue_Crystal;
    public int Random_Diamond;

    [Header("���ɸ���(n%)")]
    public float Random_Bush;
    public int Random_Bamboo;
    public float Random_BlueFlower;
    public float Random_WhiteFlower1;
    public float Random_WhiteFlower2;
    public float Random_YellowFlower;


    //���
    [Header("Player-��ҽŵ�����")]
    public Transform PlayerFoot;
    [HideInInspector]
    public byte ERROR_CODE_OUTOFVOXELMAP = 255;
    [HideInInspector]
    public Vector3 Start_Position = new Vector3(1600f, 63f, 1600f);


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

    [Header("[��ʱδ����]�µ���Ⱦ����(�ӳ�Ϊ����)")]
    public Thread myThread_Render;
    public int Delay_RenderMesh = 1000;
    public ConcurrentQueue<Chunk> WaitToRender_New = new ConcurrentQueue<Chunk>();

    //���ɷ���
    private Vector3 Center_Now;
    private Vector3 Center_direction; //��������˷���

    //UI Manager
    [HideInInspector]
    public float initprogress = 0f;


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

    //Init
    [HideInInspector] public bool InitError = false;


    //----------------------------------���ں���---------------------------------------
    private void Start()
    {
        //֡��
        Application.targetFrameRate = 90;

        //Self
        Chunks = new GameObject();
        Chunks.name = "Chunks";
        Chunks.transform.SetParent(GameObject.Find("Environment").transform);

        //��������
        Seed = Random.Range(0, 100);
        //sea_level = Random.Range(20, 39);

        //��ʼ��һ��С��
        Start_Screen_Init();
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
            if (GetVector3Length(PlayerFoot.transform.position - Center_Now) > (StartToRender * 16f))
            {
                //����Center
                Center_direction = VtoNormal(PlayerFoot.transform.position - Center_Now);
                Center_Now += Center_direction * VoxelData.ChunkWidth;

                //���Chunk
                AddtoCreateChunks(Center_direction);
                AddtoRemoveChunks(Center_direction);
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
    public void Start_Screen_Init()
    {
        Chunk chunk_temp = new Chunk(new Vector3(0, 0, 0), this ,true);

        //GameObject chunkGameObject = new GameObject("TheMenuChunk");
        //Chunk chunk = chunkGameObject.AddComponent<Chunk>();
        //chunk.InitChunk(new Vector3(0, 0, 0), this);
    }

    //�������
    public void CheckSeed()
    {
        if (input_Seed != null && string.IsNullOrEmpty(input_Seed.text))
        {
            //Debug.Log("����Ϊ�գ�");
        } 
        else
        {
            //Debug.Log("���Ӳ�Ϊ�գ�");

            //��������
            int number;
            if (int.TryParse(input_Seed.text, out number))
            {
                // ת���ɹ���number �д洢�������ֶ��е�����
                //Debug.Log("����Ϊ: " + number);

                if (number > 0)
                {
                    Seed = number;
                }
                else
                {
                    InitError = true;
                    Debug.Log("����ת��ʧ�ܣ�");
                }
                

                //����ˮƽ��
                //sea_level = Random.Range(20, 42); 
            }
            else
            {
                InitError = true;
                // ת��ʧ�ܣ������ֶ��е��ַ���������Ч������
                Debug.Log("����ת��ʧ�ܣ�");
            }


        }
    }

    //�����Ⱦ��Χ
    public void CheckRenderSize()
    {
        //size�����6��������������ֵ

        int number;

        if (int.TryParse(input_RenderSize.text, out number))
        {
            // ת���ɹ���number �д洢�������ֶ��е�����
            //Debug.Log("����Ϊ: " + number);
            if (number > 0)
            {
                renderSize = number;
            }
            else
            {
                InitError = true;
            }


            

        }
        else
        {
            // ת��ʧ�ܣ������ֶ��е��ַ���������Ч������
            InitError = true;
            Debug.Log("RenderSIzeת��ʧ�ܣ�");
        }
    }

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
                initprogress = Mathf.Lerp(0f, 0.9f, temp / max);

                yield return new WaitForSeconds(InitCorountineDelay);
            }
        }

        //���³�ʼ�����λ�ã����ô�ģ
        Init_Player_Location();

        //��Ϸ��ʼ
        yield return new WaitForSeconds(0.5f);
        initprogress = 1f;

        //�������Ż�Э��
        StartCoroutine(Chunk_Optimization());

        //����������Ⱦˮ�������߳�
        //myThread_Water = new Thread(new ThreadStart(Thread_AwaysUpdate_Water));
        //myThread_Water.Start();

        //������ȾMesh�߳�
        //myThread_Render = new Thread(new ThreadStart(Thread_RenderMesh));
        //myThread_Render.Start();

    }


    //������������
    public void Update_CenterChunks()
    {
        //update������������
        StartCoroutine(Init_Map_Thread());
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
    void dynamicRandertime(float nowtime)
    {
        if (nowtime > RenderDelay)
        {
            RenderDelay = nowtime;
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

    //��ʼ������λ��
    void Init_Player_Location()
    {

        Start_Position = new Vector3(GetRealChunkLocation(PlayerFoot.transform.position).x, PlayerFoot.transform.position.y, GetRealChunkLocation(PlayerFoot.transform.position).z);
        

        //��<1600,63,1600>���±�����ֱ�������������
        while (GetBlockType(Start_Position) == VoxelData.Air)
        {
            Start_Position.y -= 1f;
        }

        Start_Position.y += 2f;


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
        return new Vector3(Mathf.FloorToInt(vec.x % VoxelData.ChunkWidth), Mathf.FloorToInt(vec.y), Mathf.FloorToInt(vec.z % VoxelData.ChunkWidth));

    }


    //���ط�������
    public byte GetBlockType(Vector3 pos)
    {

        Allchunks.TryGetValue(GetChunkLocation(pos), out Chunk chunktemp);

        //��������ˢ������
        //if (chunktemp == null)
        //{
        //    return VoxelData.notChunk;
        //}

        //�����������ڣ���Yֵ̫��

        if ((int)GetRelalocation(pos).y >= VoxelData.ChunkHeight)
        {
            //isBlock = false;
            //isnearblock = false;
            return ERROR_CODE_OUTOFVOXELMAP;
        }

        byte block_type = chunktemp.voxelMap[(int)GetRelalocation(pos).x, (int)GetRelalocation(pos).y, (int)GetRelalocation(pos).z];


        return block_type;
    }

    //---------------------------------------------------------------------------------------







    //------------------------------------����------------------------------------------------
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

    //�������ײ�еķ����ж�
    //true������ײ
    public bool CheckForVoxel(Vector3 pos)
    {
        //�����������
        Vector3 vec = GetRelalocation(new Vector3(pos.x, pos.y, pos.z));

        //��һ�¸õ����Ƿ����
        //Allchunks.TryGetValue(GetChunkLocation(pos), out obj);
        //if (obj.myState == false)
        //{
        //    return true;
        //}

        //�ж�XOZ����û�г���
        if (!Allchunks.ContainsKey(GetChunkLocation(pos))) { return true; }

        //�ж�Y����û�г���
        if (vec.y >= VoxelData.ChunkHeight) { return false; }

        //���ӷ���false
        if (GetBlockType(pos) == VoxelData.Bamboo) { return false; }

        //���ع��廹�ǿ���
        return blocktypes[Allchunks[GetChunkLocation(new Vector3(pos.x, pos.y, pos.z))].voxelMap[(int)vec.x, (int)vec.y, (int)vec.z]].isSolid;

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
        return blocktypes[Allchunks[GetChunkLocation(new Vector3(pos.x, pos.y, pos.z))].voxelMap[(int)vec.x, (int)vec.y, (int)vec.z]].canBeChoose;

    }




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