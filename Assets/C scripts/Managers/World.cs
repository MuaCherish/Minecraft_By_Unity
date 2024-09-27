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
using UnityEngine.UIElements.Experimental;
using static UnityEngine.GraphicsBuffer;



//全局游戏状态
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

    Block,Bush,Torch,Air,Water,SnowPower,HalfBrick,Door,Tool,

}


public enum Facing2d
{
    None, front,back, left, right,
}

//public enum WorldType
//{
//    默认, 超平坦世界, 草原群系, 高原群系, 沙漠群系, 沼泽群系, 密林群系,
//}

public enum FaceCheck_Enum
{
    isSolid, appointType, appointDrawmode
}

//比如 门：{0, isSolid false} 代表后方如果是Solid则不生成面

// 0 1  2   3  4  5
//后 前 上 下 左 右

[System.Serializable]
public class FaceCheckMode
{
    public int FaceDirect;  //这个Direct属于本地方向，比如0指的是自己朝向的后方，因为后面要顾及到物体的旋转
    public FaceCheck_Enum checktype;
    public byte appointType;
    public DrawMode appointDrawmode;
    public bool isCreateFace;
}



public class World : MonoBehaviour
{
    [Header("Debug")]
    public bool 低区块模式; private bool hasExec_低区块模式 = true;
    public bool 无黑夜模式; private bool hasExec_无黑夜模式 = true;
    public bool 是否生成Chunk侧面 = false;

    [Header("引用")]
    public ManagerHub managerhub;
    public CanvasManager canvasManager;
    public Player player;

    [Header("世界存档")]
    [HideInInspector] public bool isFinishSaving = false;
    [HideInInspector] public String savingPATH = ""; //存档根目录
    [HideInInspector] public WorldSetting worldSetting;
    [HideInInspector] public List<SavingData> TheSaving = new List<SavingData>(); //读取的存档
    [HideInInspector] public List<EditStruct> EditNumber = new List<EditStruct>(); //玩家数据
    //public List<SavingData> savingDatas = new List<SavingData>();//最终保存数据

    [Header("游戏状态")]
    [ReadOnly]public Game_State game_state = Game_State.Start;
    [ReadOnly] public GameMode game_mode = GameMode.Survival;
    [HideInInspector] public bool isLoadSaving = false;
    //public bool SuperPlainMode = false; 


    [Header("Material-方块类型 + 工具类型")]
    public Material material;
    public Material material_Water;
    public BlockType[] blocktypes;

    [Header("World-渲染设置")]
    [Tooltip("4就是边长为4*16的正方形")] public int renderSize = 5;        //渲染区块半径,即renderSize*16f
    [Tooltip("2就是接近2*16的时候开始刷新区块")] public float StartToRender = 1f;
    public float DestroySize = 7f;


    [Header("Cave-洞穴系统")]
    //noise3d_scale越大洞穴破损越严重，越大越好
    //cave_width越小洞穴平均宽度变小，不能太大，不然全是洞
    //public bool debug_CanLookCave = false;
    public float noise3d_scale = 0.085f;
    public float cave_width = 0.45f;


    [Header("群系特征概率和数据(值越大范围越小)")]
    public float 氧气浓度OxygenDensity;
    public float 三维密度Density3d;
    public float 干燥程度Aridity;
    public float 空气湿度MoistureLevel;
    public BiomeNoiseSystem[] biomenoisesystems;


    [Header("地质分层与概率系统(n%)(矿物为万分之n)")]
    public TerrainLayerProbabilitySystem terrainLayerProbabilitySystem;


    //玩家
    [Header("Player-玩家脚底坐标")]
    public Transform PlayerFoot;
    [HideInInspector]
    public byte ERROR_CODE_OUTOFVOXELMAP = 255;
    public Vector3 Start_Position = new Vector3(1600f, 127f, 1600f);


    //全部Chunk位置
    public Dictionary<Vector3, Chunk> Allchunks = new Dictionary<Vector3, Chunk>();
    private readonly object Allchunks_Lock = new object();


    //等待添加队列
    //private List<chunkWithsequence> WatingToCreateChunks = new List<chunkWithsequence>();
    private List<Vector3> WatingToCreate_Chunks = new List<Vector3>();


    //等待删除队列
    private List<Vector3> WatingToRemove_Chunks = new List<Vector3>();
    private Chunk obj;


    //协程
    [Header("Corountine-协程延迟时间")]
    public float InitCorountineDelay = 1f;
    public float CreateCoroutineDelay = 0.5f;
    public float RemoveCoroutineDelay = 0.5f;
    public float RenderDelay = 0.1f;
    public int Mesh_0_TaskCount = 0;


    [Header("水面流动渲染线程")]
    private Thread myThread_Water;
    public int Delay_RenderFlowWater = 5;
    public float MaxDistant_RenderFlowWater = 5;


    [Header("[暂时未启用]强加载渲染线程(延迟为毫秒)")]
    public Thread myThread_Render;
    public int Delay_RenderMesh = 1000;
    public ConcurrentQueue<Chunk> WaitToRender_New = new ConcurrentQueue<Chunk>();



    //生成方向
    private Vector3 Center_Now;
    private Vector3 Center_direction; //这个代表了方向


    //计时
    public float InitStartTime;
    public float InitEndTime;

    //UI Manager
    //[HideInInspector]
    //public float initprogress = 0f;


    //Chunks父级
    [HideInInspector] public GameObject Chunks;


    //一次性代码
    bool hasExec = true;
    bool hasExec_SetSeed = true;


    //Create && Remove 协程
    Coroutine CreateCoroutine;
    Coroutine RemoveCoroutine;


    //Render_0 && Render_1 协程
    [HideInInspector] public bool RenderLock = false;
    public ConcurrentQueue<Chunk> WaitToRender = new ConcurrentQueue<Chunk>();
    public ConcurrentQueue<Chunk> WaitToRender_temp = new ConcurrentQueue<Chunk>();
    Coroutine Render_Coroutine;


    //Threading
    [HideInInspector] public bool MeshLock = false;
    public ConcurrentQueue<Chunk> WaitToCreateMesh = new ConcurrentQueue<Chunk>();
    Coroutine Mesh_Coroutine;

    //Flash
    public ConcurrentQueue<Chunk> WaitToFlashChunkQueue = new ConcurrentQueue<Chunk>();

    //Init
    //[HideInInspector] public bool InitError = false;


    //----------------------------------周期函数---------------------------------------




    private void Start()
    {

        InitWorldManager();

    }

    public void InitWorldManager()
    {

        game_mode = GameMode.Survival;


        isLoadSaving = false;


        // 使用 persistentDataPath 作为根目录
        savingPATH = Path.Combine(Application.persistentDataPath);

        // 确保目录存在
        if (!Directory.Exists(savingPATH))
        {
            Directory.CreateDirectory(savingPATH);
        }

        // 打印目录路径以确认
        //Debug.Log("存档目录: " + savingPATH);


        //初始化



        game_state = Game_State.Start;
        TheSaving = new List<SavingData>();
        EditNumber = new List<EditStruct>();
        //savingDatas = new List<SavingData>();
        renderSize = 10;
        StartToRender = 1f;
        DestroySize = 7f;
        Start_Position = new Vector3(1600f, 127f, 1600f);
        Allchunks = new Dictionary<Vector3, Chunk>();
        WatingToCreate_Chunks = new List<Vector3>();
        WatingToRemove_Chunks = new List<Vector3>();
        myThread_Render = null;
        WaitToRender_New = new ConcurrentQueue<Chunk>();
        是否生成Chunk侧面 = false;
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

        // 销毁 Chunks 大纲目录下的所有子物体
        foreach (Transform child in Chunks.transform)
        {
            Destroy(child.gameObject);
        }

        //-------顺序不能变化------------------
        terrainLayerProbabilitySystem.Seed = UnityEngine.Random.Range(0, 100000000);
        worldSetting = new WorldSetting(terrainLayerProbabilitySystem.Seed);
        UnityEngine.Random.InitState(worldSetting.seed);
        //-------------------------------------

        PointSaving = "";
    }

    private void FixedUpdate()
    {

        //Mesh线程常驻
        CreateMeshCoroutineManager();


        //渲染线程常驻
        RenderCoroutineManager();



        //初始化地图
        if (game_state == Game_State.Loading)
        {

            if (hasExec_SetSeed)
            {
                InitStartTime = Time.time;
                //获取当前模式
                //if (canvasManager.currentWorldType == 6)
                //{
                //    //SuperPlainMode = true;
                //}

                //worldSetting.worldtype = canvasManager.currentWorldType

                //开始初始化
                Update_CenterChunks();

                hasExec_SetSeed = false;
            }


        }


        //游戏开始
        if (game_state == Game_State.Playing)
        {

            if (hasExec)
            {

                // 将鼠标锁定在屏幕中心
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;

                //鼠标不可视
                UnityEngine.Cursor.visible = false;

            }


            //玩家移动刷新
            //如果大于16f
            if (GetVector3Length(PlayerFoot.transform.position - Center_Now) > (StartToRender * 16f) && GetVector3Length(PlayerFoot.transform.position - Center_Now) <= ((StartToRender + 1) * 16f))
            {

                //更新Center
                Center_direction = VtoNormal(PlayerFoot.transform.position - Center_Now);
                Center_Now += Center_direction * VoxelData.ChunkWidth;

                //添加Chunk
                AddtoCreateChunks(Center_direction);
                AddtoRemoveChunks(Center_direction);

            }
            //玩家移动过远距离
            else if (GetVector3Length(PlayerFoot.transform.position - Center_Now) > ((StartToRender + 1) * 16f))
            {



                Update_CenterWithNoInit();




            }


        }

    }



    void OnApplicationQuit()
    {

        //结束游戏
        game_state = Game_State.Ending;

        //print("Quit");
        //RenderSettings.skybox.SetFloat("_Exposure", 1f);

        //等待Water线程
        if (myThread_Water != null && myThread_Water.IsAlive)
        {

            myThread_Water.Join(); // 等待线程安全地终止

        }

        //等待Render线程
        if (myThread_Render != null && myThread_Render.IsAlive)
        {

            myThread_Render.Join(); // 等待线程安全地终止

        }

    }

    private void Update()
    {
        if (低区块模式 && hasExec_低区块模式)
        {
            renderSize = 2;

            hasExec_低区块模式 = false;
        }

        if (无黑夜模式 && hasExec_无黑夜模式)
        {

            managerhub.timeManager.gameObject.SetActive(false);

            hasExec_无黑夜模式 = false;
        }

    }


    //---------------------------------------------------------------------------------------










    //----------------------------------World Options---------------------------------------




    //主菜单地图
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




    //初始化地图
    public Coroutine Init_MapCoroutine;
    IEnumerator Init_Map_Thread()
    {


        //确定玩家圈养中心点
        if (isLoadSaving)
        {

            Center_Now = new Vector3(GetRealChunkLocation(worldSetting.playerposition).x, 0, GetRealChunkLocation(worldSetting.playerposition).z);

        }
        else
        {

            Center_Now = new Vector3(GetRealChunkLocation(PlayerFoot.transform.position).x, 0, GetRealChunkLocation(PlayerFoot.transform.position).z);

        }

        //写一个协程，清除或者隐藏过远的区块


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

                //剩余进度计算
                float max = renderSize * renderSize * 4;
                temp++;
                canvasManager.Initprogress = Mathf.Lerp(0f, 0.9f, temp / max);

                yield return new WaitForSeconds(InitCorountineDelay);
            }

        }

        //重新初始化玩家位置，放置穿模
        Init_Player_Location();

        //游戏开始
        yield return new WaitForSeconds(0.5f);
        canvasManager.Initprogress = 1f;

        //开启面优化协程
        //StartCoroutine(Chunk_Optimization());

        StartCoroutine(FlashChunkCoroutine());

        //开启持续渲染水体流动线程
        //myThread_Water = new Thread(new ThreadStart(Thread_AwaysUpdate_Water));
        //myThread_Water.Start();

        //开启渲染Mesh线程
        //myThread_Render = new Thread(new ThreadStart(Thread_RenderMesh));
        //myThread_Render.Start();

        Init_MapCoroutine = null;
    }


    Coroutine Init_Map_Thread_NoInit_Coroutine;

    public void Update_CenterWithNoInit()
    {
        if (Init_Map_Thread_NoInit_Coroutine == null)
        {
            //print("玩家移动太快！Center_Now已更新");
            Init_Map_Thread_NoInit_Coroutine = StartCoroutine(Init_Map_Thread_NoInit());

            managerhub.timeManager.UpdateDayFogDistance();
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


    //更新中心区块
    public void Update_CenterChunks()
    {
        //print("更新中心区块");
        //update加载中心区块
        if (Init_MapCoroutine == null)
        {
            Init_MapCoroutine = StartCoroutine(Init_Map_Thread());
        }


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



    //清除过远区块
    IEnumerator Update_FarChunks()
    {

        //如果过远就隐藏
        yield return null;

    }



    //优化Chunk面数协程
    //本质上是把BaseChunk全部重新生成一遍

    public IEnumerator Chunk_Optimization()
    {

        foreach (var Chunk in Allchunks)
        {

            WaitToCreateMesh.Enqueue(Chunk.Value);

        }



        yield return new WaitForSeconds(1f);

    }


    //负责把接收的到的Chunk用多线程刷新
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



    //一直更新水的线程
    //void Thread_AwaysUpdate_Water()
    //{
    //    int 次数 = 0;
    //    int 个数 = 0;

    //    //一直循环
    //    while (game_state != Game_State.Ending)
    //    {

    //        lock (Allchunks_Lock)
    //        {

    //            //遍历所有AllChunks
    //            foreach (var chunktemp in Allchunks)
    //            {

    //                //如果区块包含水，且在12区块内，则更新
    //                if (chunktemp.Value.iHaveWater && GetVector3Length(chunktemp.Value.myposition - Center_Now) > MaxDistant_RenderFlowWater)
    //                {

    //                    chunktemp.Value.Always_updateWater();
    //                    //print($"刷新了{chunktemp.Value.name}");

    //                    个数++;

    //                }

    //            }

    //        }






    //        //休眠5秒钟
    //        次数++;
    //        print($"第{次数}次刷新，一共刷新{个数}个");
    //        个数 = 0;
    //        Thread.Sleep(Delay_RenderFlowWater * 1000);

    //    }

    //    Debug.LogError("Water线程中止");

    //}




    //--------------------------------------------------------------------------------------





    //-----------------------------------Create 协程----------------------------------------




    //添加到等待添加队列
    void AddtoCreateChunks(Vector3 add_vec)
    {

        //ForWard
        if (add_vec == new Vector3(0, 0, 1))
        {

            add_vec = (Center_Now / VoxelData.ChunkWidth) + Center_direction * (renderSize - 1);

            //新增Chunk
            for (int i = -renderSize; i < renderSize; i++)
            {
                //CreateChunk(add_vec + new Vector3((float)i, 0, 0));

                WatingToCreate_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));


            }

            //呼叫里侧Chunk更新
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

            //呼叫里侧Chunk更新
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

            //呼叫里侧Chunk更新
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


            //呼叫里侧Chunk更新
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

        //Debug.Log("已经添加坐标");


        //判断是否启动协程
        //先两次添加再启动协程，后面数据多了一次启动协程
        if (WatingToCreate_Chunks.Count > 0 && CreateCoroutine == null)
        {

            CreateCoroutine = StartCoroutine(CreateChunksQueue());

        }


    }



    //协程：创造Chunk
    private IEnumerator CreateChunksQueue()
    {

        while (true)
        {

            yield return new WaitForSeconds(CreateCoroutineDelay);


            //如果队列中有数据，就读取
            //如果队列中没有数据，就关闭协程
            if (WatingToCreate_Chunks.Count > 0)
            {

                //如果查到的chunk已经存在，则唤醒
                //不存在则生成
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



    //生成Chunk
    //BaseChunk不会进行自身剔除
    void CreateBaseChunk(Vector3 pos)
    {

        //先判断一下有没有
        if (Allchunks.ContainsKey(pos))
        {
            Allchunks[pos].ShowChunk();
            return;
        }

        //调用Chunk
        Vector3 _ChunkLocation = new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z));
        Chunk _chunk_temp;


        //if (_ChunkLocation == new Vector3(195f,0,89f))
        //{
        //    print("");
        //}

        //调用Chunk
        if (ContainsChunkLocation(_ChunkLocation))
        {
            _chunk_temp = new Chunk(_ChunkLocation, this, false, TheSaving[GetIndexOfChunkLocation(_ChunkLocation)].EditDataInChunkList);
        }
        else
        {
            _chunk_temp = new Chunk(_ChunkLocation, this, false);
        }

        //GameObject chunkGameObject = new GameObject($"{Mathf.FloorToInt(pos.x)}, 0, {Mathf.FloorToInt(pos.z)}");
        //Chunk chunktemp = chunkGameObject.AddComponent<Chunk>();
        //chunktemp.InitChunk(new Vector3(0, 0, 0), this);

        //添加到字典
        Allchunks.Add(pos, _chunk_temp);

    }



    //非BaseChunk会进行Chunk面剔除
    void CreateChunk(Vector3 pos)
    {

        //先判断一下有没有
        if (Allchunks.ContainsKey(pos))
        {

            Allchunks[pos].ShowChunk();
            return;

        }


        Vector3 _ChunkLocation = new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z));
        Chunk _chunk_temp;

        //调用Chunk
        if (ContainsChunkLocation(_ChunkLocation))
        {
            _chunk_temp = new Chunk(_ChunkLocation, this, false, TheSaving[GetIndexOfChunkLocation(_ChunkLocation)].EditDataInChunkList);
        }
        else
        {
            _chunk_temp = new Chunk(_ChunkLocation, this, false);
        }


        //GameObject chunkGameObject = new GameObject($"{Mathf.FloorToInt(pos.x)}, 0, {Mathf.FloorToInt(pos.z)}");
        //Chunk chunktemp = chunkGameObject.AddComponent<Chunk>();
        //chunktemp.InitChunk(new Vector3(0, 0, 0), this);

        //添加到字典
        Allchunks.Add(pos, _chunk_temp);

    }




    //--------------------------------------------------------------------------------------







    //-----------------------------------Remove 协程-----------------------------------------




    //添加到等待删除队列
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

        //判断是否启动协程
        //先两次添加再启动协程，后面数据多了一次启动协程
        if (WatingToRemove_Chunks.Count > 0 && RemoveCoroutine == null)
        {

            RemoveCoroutine = StartCoroutine(RemoveChunksQueue());

        }

    }


    //协程：删除ChunK
    private IEnumerator RemoveChunksQueue()
    {

        while (true)
        {

            yield return new WaitForSeconds(RemoveCoroutineDelay);


            //如果队列中有数据，就读取
            //如果队列中没有数据，就关闭协程
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

        //如果超出范围就卸载,否则就隐藏
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





    //-----------------------------------Render 协程-----------------------------------------




    //渲染协程池
    void RenderCoroutineManager()
    {

        if (WaitToRender.Count != 0 && Render_Coroutine == null)
        {

            //print($"启动渲染协程");
            Render_Coroutine = StartCoroutine(Render_0());

        }


    }

    //一条渲染协程
    IEnumerator Render_0()
    {

        while (true)
        {

            //Queue
            WaitToRender.TryDequeue(out Chunk chunktemp);

            //print($"{GetChunkLocation(chunktemp.myposition)}开始渲染");

            //CreateMesh
            if (chunktemp.isReadyToRender)
            {

                chunktemp.CreateMesh();

            }

            //Empty
            if (WaitToRender.Count == 0)
            {

                //print($"队列为空，停止协程");
                Render_Coroutine = null;
                RenderLock = false;
                break;

            }

            yield return new WaitForSeconds(RenderDelay);

        }

    }

    //单位为ms，根据每次渲染，动态改变渲染时间
    //void dynamicRandertime(float nowtime)
    //{

    //    if (nowtime > RenderDelay)
    //    {

    //        RenderDelay = nowtime;

    //    }

    //}



    //Mesh协程
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

                //print($"{GetChunkLocation(chunktemp.myposition)}添加到meshQueue");

                //Mesh线程
                Thread myThread = new Thread(new ThreadStart(chunktemp.UpdateChunkMesh_WithSurround));
                myThread.Start();

                if (WaitToCreateMesh.Count == 0)
                {
                    if (hasExec_CaculateInitTime)
                    {
                        print("渲染完了");
                        InitEndTime = Time.time;

                        //renderSize * renderSize * 4是总区块数，2是因为面剔除渲染了两次
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



    //新的渲染管线
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

        Debug.LogError("Render线程中止");

    }



    //---------------------------------------------------------------------------------------






    //----------------------------------Player Options---------------------------------------

    //简单噪声
    public float GetSimpleNoise(int _x, int _z, Vector3 _myposition)
    {
        float smoothNoise = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((_x + _myposition.x) * 0.01f, (_z + _myposition.z) * 0.01f));
        return smoothNoise;
    }

    //简单偏移噪声，用来给树等分布用
    //_offset类似为Vector3(111f,222f)
    //_Scale噪声缩放:0.01为正常缩放,0.1为水下沙泥分布
    public float GetSimpleNoiseWithOffset(int _x, int _z, Vector3 _myposition, Vector2 _Offset, float _Scale)
    {
        float smoothNoise = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((_x + _myposition.x + _Offset.x) * _Scale, (_z + _myposition.z + _Offset.y) * _Scale));
        return smoothNoise;
    }

    //获得所在群系类型
    public byte GetBiomeType(int _x, int _z, Vector3 _myposition)
    {

        float _A = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(0f, 0f, 0f));
        float _B = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(123f, 0f, 456f));
        float _C = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(789f, 0f, 123f));
        float _D = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(456f, 0f, 789f));

        ////沙漠
        //if (_C >= 干燥程度Aridity)
        //{

        //    return 2;

        //}

        //else
        //{

        //    //高原
        //    if (_B >= 三维密度Density3d)
        //    {
        //        return 1;
        //    }

        //    //草原
        //    else if (_A >= 氧气浓度OxygenDensity)
        //    {

        //        if (_D >= 空气湿度MoistureLevel)
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
        //        //返回密林
        //        return 4;
        //    }

        //}

        //高原
        if (_B >= 三维密度Density3d)
        {
            return 1;
        }

        else
        {

            //沙漠
            if (_C >= 干燥程度Aridity)
            {

                return 2;

            }

            //草原
            else if (_A >= 氧气浓度OxygenDensity)
            {

                if (_D >= 空气湿度MoistureLevel)
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
                //返回密林
                return 4;
            }

        }


    }


    //根据给定参数和群系种类
    //变成给定的群系噪声
    public float GetTotalNoiseHigh_Biome(int _x, int _z, Vector3 _myposition, int _WorldType)
    {
        if (_x < 0 || _x > VoxelData.ChunkWidth || _z < 0 || _z > VoxelData.ChunkWidth)
        {
            print($"GetTotalNoiseHigh_Biome出界,{_x},{_z}");
            return 128f;
        }

        if (_WorldType == VoxelData.Biome_SuperPlain)
        {
            return 0f;
        }


        //默认
        if (_WorldType == VoxelData.Biome_Default)
        {
            //Noise
            float noise_1 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[0].Noise_Scale_123.x, (float)(_z + _myposition.z) * biomenoisesystems[0].Noise_Scale_123.x);
            float noise_2 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[0].Noise_Scale_123.y, (float)(_z + _myposition.z) * biomenoisesystems[0].Noise_Scale_123.y);
            float noise_3 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[0].Noise_Scale_123.z, (float)(_z + _myposition.z) * biomenoisesystems[0].Noise_Scale_123.z);
            float noise = Mathf.Lerp(0f, 1f, noise_1 * biomenoisesystems[0].Noise_Rank_123.x + noise_2 * biomenoisesystems[0].Noise_Rank_123.y + noise_3 * biomenoisesystems[0].Noise_Rank_123.z);
            float noise_High = Mathf.Lerp(biomenoisesystems[0].HighDomain.x, biomenoisesystems[0].HighDomain.y, noise);

            //数据定义
            int BiomeType = -1;
            float BiomeIntensity = 0f;
            float _A = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(0f, 0f, 0f));
            float _B = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(123f, 0f, 456f));
            float _C = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(789f, 0f, 123f));
            float _D = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(456f, 0f, 789f));

            //获得当前群系
            //获得群系混合强度
            //高原
            if (_B >= 三维密度Density3d)
            {
                BiomeType = VoxelData.Biome_Plateau;
                BiomeIntensity = Mathf.InverseLerp(三维密度Density3d, 1f, _B);
            }
            else
            {

                if (_C >= 干燥程度Aridity)
                {
                    BiomeType = VoxelData.Biome_Dessert;
                    BiomeIntensity = Mathf.InverseLerp(干燥程度Aridity, 1f, _C);
                }
                //草原
                else if (_A >= 氧气浓度OxygenDensity)
                {
                    if (_D >= 空气湿度MoistureLevel)
                    {
                        BiomeType = VoxelData.Biome_Marsh;
                        BiomeIntensity = Mathf.InverseLerp(空气湿度MoistureLevel, 1f, _D);
                    }
                    else
                    {
                        BiomeType = VoxelData.Biome_Plain;
                        BiomeIntensity = Mathf.InverseLerp(氧气浓度OxygenDensity, 1f, _A);
                    }
                }
                else
                {
                    BiomeType = VoxelData.Biome_Plain;
                    BiomeIntensity = Mathf.InverseLerp(氧气浓度OxygenDensity, 1f, _A);
                }

            }

            //BiomeType = 1;

            //混合群系
            float Mixnoise_1 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[BiomeType].Noise_Scale_123.x, (float)(_z + _myposition.z) * biomenoisesystems[BiomeType].Noise_Scale_123.x);
            float Mixnoise_2 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[BiomeType].Noise_Scale_123.y, (float)(_z + _myposition.z) * biomenoisesystems[BiomeType].Noise_Scale_123.y);
            float Mixnoise_3 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[BiomeType].Noise_Scale_123.z, (float)(_z + _myposition.z) * biomenoisesystems[BiomeType].Noise_Scale_123.z);
            float Mixnoise = Mathf.Lerp(0f, 1f, noise_1 * biomenoisesystems[BiomeType].Noise_Rank_123.x + noise_2 * biomenoisesystems[BiomeType].Noise_Rank_123.y + noise_3 * biomenoisesystems[BiomeType].Noise_Rank_123.z);
            float Mixnoise_High = Mathf.Lerp(biomenoisesystems[BiomeType].HighDomain.x, biomenoisesystems[BiomeType].HighDomain.y, Mixnoise);

            float 增量噪声 = Mathf.Lerp(noise_High, Mixnoise_High, BiomeIntensity);

            return 增量噪声;
            //return noise_High + 增量噪声 * 增量噪声放大倍数; 
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


    //初始化人物位置
    void Init_Player_Location()
    {

        if (isLoadSaving)
        {
            Start_Position = worldSetting.playerposition;
            player.gameObject.transform.rotation = worldSetting.playerrotation;
        }
        else
        {
            Start_Position = new Vector3(GetRealChunkLocation(PlayerFoot.transform.position).x, VoxelData.ChunkHeight - 1f, GetRealChunkLocation(PlayerFoot.transform.position).z);

            //从<1600,63,1600>向下遍历，直到坐标符合条件
            while (GetBlockType(Start_Position) == VoxelData.Air)
            {

                Start_Position.y -= 1f;

            }

            Start_Position.y += 2f;
        }


        
        
        player.InitPlayerLocation();
        //print(Start_Position);
    }


    //给定一个初始坐标和初始方向，朝着这个方向遍历ChunkHeight，返回一个非空气坐标
    public Vector3 LoopAndFindABestLocation(Vector3 _start, Vector3 _direct)
    {
        _direct.x = _direct.x > 0 ? 1 : 0;
        _direct.y = _direct.y > 0 ? 1 : 0;
        _direct.z = _direct.z > 0 ? 1 : 0;

        Vector3 _next = _start;

        //Loop
        for (int i = 0; i < VoxelData.ChunkHeight; i++)
        {
            // Check，如果当前位置的方块类型不是空气，返回该坐标
            if (GetBlockType(_next) != VoxelData.Air)
            {
                return _next;
            }

            // 累积移动位置
            _next += _direct; // 使用归一化的方向向量逐步移动
        }

        return _start; 
    }


    //Vector3 --> 大区块坐标
    public Vector3 GetChunkLocation(Vector3 vec)
    {

        return new Vector3((vec.x - vec.x % VoxelData.ChunkWidth) / VoxelData.ChunkWidth, 0, (vec.z - vec.z % VoxelData.ChunkWidth) / VoxelData.ChunkWidth);

    }


    public Vector3 GetRealChunkLocation(Vector3 vec)
    {

        return new Vector3(16f * ((vec.x - vec.x % VoxelData.ChunkWidth) / VoxelData.ChunkWidth), 0, 16f * ((vec.z - vec.z % VoxelData.ChunkWidth) / VoxelData.ChunkWidth));

    }

    //Vector3 --> 大区块对象
    public Chunk GetChunkObject(Vector3 pos)
    {

        Allchunks.TryGetValue(GetChunkLocation(pos), out Chunk chunktemp);
        return chunktemp;

    }


    //Vector3 --> 区块里的相对坐标
    public Vector3 GetRelalocation(Vector3 vec)
    {

        return new Vector3(Mathf.FloorToInt(vec.x % VoxelData.ChunkWidth), Mathf.FloorToInt(vec.y) % VoxelData.ChunkHeight, Mathf.FloorToInt(vec.z % VoxelData.ChunkWidth));

    }

     
    //返回方块类型:绝对坐标
    public byte GetBlockType(Vector3 pos)
    {

        if(Allchunks.TryGetValue(GetChunkLocation(pos), out Chunk chunktemp))
        {
            if ((int)GetRelalocation(pos).y < 0)
            {
                //if (!managerhub.player.isInCave)
                //{
                //    managerhub.player.CheckisInCave();
                //}
               

                return ERROR_CODE_OUTOFVOXELMAP;
            }


            if ((int)GetRelalocation(pos).y >= VoxelData.ChunkHeight)
            {

                //isBlock = false;
                //isnearblock = false;
                //print("玩家坐标异常！");
                return ERROR_CODE_OUTOFVOXELMAP;

            }

            byte block_type = chunktemp.voxelMap[(int)GetRelalocation(pos).x, (int)GetRelalocation(pos).y, (int)GetRelalocation(pos).z].voxelType;

            return block_type;
        }

        //如果玩家在刷新区外
        //if (chunktemp == null)
        //{
        //    return VoxelData.notChunk;
        //}

        //如果玩家在区内，但Y值太高
        //print($"找不到玩家脚下的Chunk {pos}");
        return ERROR_CODE_OUTOFVOXELMAP;



    }




    //---------------------------------------------------------------------------------------







    //------------------------------------工具------------------------------------------------

    //给定日期，将pointsaving修改为给定参数
    [HideInInspector] public String PointSaving = "";
    public void SelectSaving(String _PointSaving)
    {
        PointSaving = _PointSaving;

        //将进入选中的世界按钮点亮
        if (canvasManager.isClickSaving == false)
        {
            canvasManager.LightButton();
        }
        
    }

    // 将EditNumber归类
    public void ClassifyWorldData()
    {
        foreach (var edittemp in EditNumber)
        {
            // 获取当前修改所在的区块位置
            Vector3 _ChunkLocation = GetChunkLocation(edittemp.editPos);

            // 标记是否在 savingDatas 中找到相应的 ChunkLocation
            bool found = false;

            // 查找是否有相同的 ChunkLocation
            foreach (var savingtemp in TheSaving)
            {
                if (savingtemp.ChunkLocation == _ChunkLocation)
                {
                    // 如果找到了相应的 ChunkLocation，则添加相对位置和方块类型到 EditDataInChunk
                    savingtemp.EditDataInChunk[GetRelalocation(edittemp.editPos)] = edittemp.targetType;
                    found = true;
                    break;  // 找到后直接跳出循环
                }
            }

            // 如果没有找到对应的 ChunkLocation，则新建一个 SavingData 并添加到 savingDatas
            if (!found)
            {
                // 创建新的 EditDataInChunk 字典，并添加当前的相对位置和方块类型
                Dictionary<Vector3, byte> newEditDataInChunk = new Dictionary<Vector3, byte>();
                newEditDataInChunk[GetRelalocation(edittemp.editPos)] = edittemp.targetType;

                // 创建新的 SavingData 并添加到 savingDatas
                SavingData newSavingData = new SavingData(_ChunkLocation, newEditDataInChunk);
                TheSaving.Add(newSavingData);
            }
        }

        // 打印 savingDatas（可选，用于调试）
        //foreach (var savingtemp in savingDatas)
        //{
        //    Debug.Log($"Chunk: {savingtemp.ChunkLocation}, Edits: {savingtemp.EditDataInChunk.Count}");

        //    //for (int i = 0;i < savingtemp.EditDataInChunk.Count; i ++)
        //    //{
        //    //    Debug.Log($"Chunk: {savingtemp.ChunkLocation}, Edits: {savingtemp.EditDataInChunk.Count}");
        //    //} 
        //}

        //合并上次存档内容
        //MergeSavingDataLists();
        //savingDatas = TheSaving;
        SAVINGDATA(savingPATH);
    }


    //存档
    public void SAVINGDATA(string savePath)
    {
        // 更新存档结构体
        worldSetting.playerposition = player.transform.position;
        worldSetting.playerrotation = player.transform.rotation;
        worldSetting.gameMode = game_mode;
        string previouDate = worldSetting.date;
        worldSetting.date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        // 创建一个名为 "Saves" 的文件夹
        string savesFolderPath = Path.Combine(savePath, "Saves");
        if (!Directory.Exists(savesFolderPath))
        {
            Directory.CreateDirectory(savesFolderPath);
        }

        // 在 "Saves" 文件夹下创建一个以存档创建日期命名的文件夹
        string folderPath = Path.Combine(savesFolderPath, worldSetting.date);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 删除Saves文件夹中名字为previouDate的文件夹，如果能找到的话
        string oldFolderPath = Path.Combine(savesFolderPath, previouDate);
        if (Directory.Exists(oldFolderPath))
        {
            // 删除旧文件夹及其所有内容
            Directory.Delete(oldFolderPath, true);
        }

        // 将所有的 SavingData 的字典转换为列表
        foreach (var data in TheSaving)
        {
            data.EditDataInChunkList = new List<EditStruct>();
            foreach (var kvp in data.EditDataInChunk)
            {
                data.EditDataInChunkList.Add(new EditStruct(kvp.Key, kvp.Value));
            }
        }

        // 将 worldSetting 和 savingDatas 转换为 JSON 字符串
        string worldSettingJson = JsonUtility.ToJson(worldSetting, true);
        string savingDatasJson = JsonUtility.ToJson(new Wrapper<SavingData>(TheSaving), true);

        // 将 JSON 字符串保存到指定路径的文件夹中
        File.WriteAllText(Path.Combine(folderPath, "WorldSetting.json"), worldSettingJson);
        File.WriteAllText(Path.Combine(folderPath, "SavingDatas.json"), savingDatasJson);



         Debug.Log("数据已保存到: " + folderPath);
        isFinishSaving = true;


        // Debug
        //foreach (var data in TheSaving)
        //{
        //    Debug.Log("Chunk Location: " + data.ChunkLocation);
        //    foreach (var editData in data.EditDataInChunkList)
        //    {
        //        Debug.Log("Edit Position: " + editData.editPos + ", Target Type: " + editData.targetType);
        //    }
        //}
    }





    //读取全部存档
    public void LoadAllSaves(string savingPATH)
    {
        // 构造 Saves 目录的路径
        string savesFolderPath = Path.Combine(savingPATH, "Saves");

        // 检查 Saves 目录是否存在，如果不存在则创建它
        if (!Directory.Exists(savesFolderPath))
        {
            try
            {
                Directory.CreateDirectory(savesFolderPath);
                //Debug.Log($"Saves 文件夹已创建: {savesFolderPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建 Saves 文件夹时出错: {ex.Message}");
                return; // 创建文件夹失败时退出函数
            }
        }

        // 获取存档目录下的所有文件夹路径
        string[] saveDirectories;
        try
        {
            saveDirectories = Directory.GetDirectories(savesFolderPath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"获取存档目录时出错: {ex.Message}");
            return; // 处理异常并退出函数
        }

        // 遍历每个存档文件夹
        foreach (string saveDirectory in saveDirectories)
        {
            // 输出当前存档文件夹名称
            string folderName = Path.GetFileName(saveDirectory);
            //Debug.Log($"Loading save from folder: {folderName}");

            // 调用 LoadData 函数读取当前存档
            try
            {
                WorldSetting _worldsetting = LoadWorldSetting(saveDirectory);
                canvasManager.NewWorldGenerate(
                    _worldsetting.name,
                    _worldsetting.date,
                    _worldsetting.gameMode,
                    _worldsetting.worldtype,
                    _worldsetting.seed
                );
            }
            catch (Exception ex)
            {
                Debug.LogError($"加载存档 {folderName} 时出错: {ex.Message}");
            }
        }

        //Debug.Log($"Total saves loaded: {saveDirectories.Length}");
    }


    //分析存档名字
    public WorldSetting LoadWorldSetting(string savePath)
    {
        // 构建 WorldSetting.json 文件的完整路径
        string worldSettingPath = Path.Combine(savePath, "WorldSetting.json");

        // 检查 WorldSetting.json 文件是否存在
        if (File.Exists(worldSettingPath))
        {
            // 读取 WorldSetting.json 文件的内容
            string worldSettingJson = File.ReadAllText(worldSettingPath);

            // 将 JSON 字符串反序列化为 WorldSetting 对象
            return JsonUtility.FromJson<WorldSetting>(worldSettingJson);
        }
        else
        {
            // 如果文件不存在，输出错误信息并返回 null
            Debug.LogError("找不到 WorldSetting.json 文件");
            return null;
        }
    }

    //加载存档
    public void LoadSavingData(string savePath)
    {
        isLoadSaving = true;

        // 构建 SavingDatas.json 文件的完整路径
        string savingDatasPath = Path.Combine(savePath, "SavingDatas.json");

        // 检查 SavingDatas.json 文件是否存在
        if (File.Exists(savingDatasPath))
        {
            // 读取 SavingDatas.json 文件的内容
            string savingDatasJson = File.ReadAllText(savingDatasPath);

            // 将 JSON 字符串反序列化为 Wrapper<SavingData> 对象
            Wrapper<SavingData> wrapper = JsonUtility.FromJson<Wrapper<SavingData>>(savingDatasJson);

            // 检查 wrapper 是否为 null
            if (wrapper != null && wrapper.Items != null)
            {
                // 将反序列化的 Items 列表赋值给 TheSaving
                TheSaving = wrapper.Items;

                // 遍历 TheSaving 列表，恢复每个 SavingData 对象中的字典
                foreach (var data in TheSaving)
                {
                    data.RestoreDictionary();

                    // Debug打印
                    //Debug.Log($"Chunk Location: {data.ChunkLocation}");
                    //foreach (var pair in data.EditDataInChunk)
                    //{
                    //    Debug.Log($"Position: {pair.Key}, Type: {pair.Value}");
                    //}
                }

                worldSetting = LoadWorldSetting(savePath);


                //更新一些参数
                game_mode = worldSetting.gameMode;



            }
            else
            {
                // 如果 wrapper 或 wrapper.Items 为 null，输出警告信息
                Debug.LogWarning("Wrapper<SavingData> 或 Items 列表为 null");
            }
        }
        else
        {
            // 如果文件不存在，输出错误信息
            Debug.LogError("找不到 SavingDatas.json 文件");
        }
    }




    //向量单位正交化
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



    //求Vector3的2d长度
    float GetVector3Length(Vector3 vec)
    {
        Vector2 vector2 = new Vector2(vec.x, vec.z);
        return vector2.magnitude;
    }





    //------------------------------------ 保存建筑 --------------------------------------------------------------

    //目前有问题

    //用于记录建筑
    //recordData.pos存的是绝对坐标
    private List<EditStruct> recordData = new List<EditStruct>();

    //记录建筑
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

    //释放建筑
    //根据0下标和end下标，计算其包含的区块，然后把数据丢给区块即可
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

    //外界修改方块
    public void EditBlock(Vector3 _pos, byte _target)
    {
        Allchunks[GetChunkLocation(_pos)].EditData(_pos,_target);
    }

    public void EditBlock(List<EditStruct> _editStructs)
    {
        List<Vector3> _ChunkLocations = new List<Vector3>();

        // 遍历_editStructs并存储ChunkLocations
        foreach (var item in _editStructs)
        {

            // 如果allchunks里没有pos.则_ChunkLocations添加
            if (Allchunks.ContainsKey(GetChunkLocation(item.editPos)))
            {
                if (!_ChunkLocations.Contains(GetChunkLocation(item.editPos)))
                {
                    _ChunkLocations.Add(GetChunkLocation(item.editPos));
                }


            }
            else
            {
                print($"区块不存在:{GetChunkLocation(item.editPos)}");

            }


        }

        // 遍历_ChunkLocations，将allchunk里的_ChunkLocations执行EditData
        foreach (var chunkLocation in _ChunkLocations)
        {
            Allchunks[chunkLocation].EditBlocks(_editStructs);
        }

        // 打印找到的区块数量
        //print($"找到{_ChunkLocations.Count}个");
    }


    Coroutine editBlockCoroutine;
    public void EditBlock(List<EditStruct> _editStructs, float _time)
    {
        if (editBlockCoroutine == null)
        {
            editBlockCoroutine = StartCoroutine(Coroutine_editBlock(_editStructs,_time));
        }
        

    }

    IEnumerator Coroutine_editBlock(List<EditStruct> _editStructs, float _time)
    {
        foreach (var item in _editStructs)
        {
            //print("执行EditBlocks");
            Allchunks[GetChunkLocation(item.editPos)].EditBlocks(item.editPos, item.targetType);

            yield return new WaitForSeconds(_time);
        }
        editBlockCoroutine = null;
    }

    //--------------------------------------------------------------------------------------------------------------

    //删除存档
    public void DeleteSave(string savepath)
    {
        if (Directory.Exists(savepath))
        {
            try
            {
                // 删除所有文件
                foreach (string file in Directory.GetFiles(savepath))
                {
                    File.Delete(file);
                    //Debug.Log($"Deleted file: {file}");
                }

                // 递归删除所有子目录
                foreach (string directory in Directory.GetDirectories(savepath))
                {
                    DeleteSave(directory);
                }

                // 删除空目录
                Directory.Delete(savepath);
                //Debug.Log("完成删除");
            }
            catch (Exception ex)
            {
                // 处理异常，如日志记录
                Debug.LogError($"删除目录 {savepath} 时出错: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"指定路径 {savepath} 不存在.");
        }
    }


    //合并存档
    //public void MergeSavingDataLists()
    //{
    //    // 创建一个新的列表用于存储合并后的数据
    //    List<SavingData> mergedSavingData = new List<SavingData>();

    //    // 将 TheSaving 中的数据加入到合并列表中
    //    foreach (var theSavingData in TheSaving)
    //    {
    //        // 在 mergedSavingData 中查找是否已经存在相同的 ChunkLocation
    //        var existingData = mergedSavingData.FirstOrDefault(sd => sd.ContainsChunkLocation(theSavingData.ChunkLocation));
    //        if (existingData == null)
    //        {
    //            // 如果没有找到相同 ChunkLocation 的数据，则直接添加
    //            mergedSavingData.Add(new SavingData(theSavingData.ChunkLocation, theSavingData.EditDataInChunk));
    //        }
    //        else
    //        {
    //            // 如果找到了，则更新已有的数据
    //            existingData.EditDataInChunk = new Dictionary<Vector3, byte>(theSavingData.EditDataInChunk);
    //        }
    //    }

    //    // 遍历 savingDatas，合并数据
    //    foreach (var savingData in savingDatas)
    //    {
    //        // 在 mergedSavingData 中查找是否已经存在相同的 ChunkLocation
    //        var existingData = mergedSavingData.FirstOrDefault(sd => sd.ContainsChunkLocation(savingData.ChunkLocation));
    //        if (existingData == null)
    //        {
    //            // 如果没有找到相同 ChunkLocation 的数据，则直接添加
    //            mergedSavingData.Add(new SavingData(savingData.ChunkLocation, savingData.EditDataInChunk));
    //        }
    //        else
    //        {
    //            // 如果找到了，则合并 EditDataInChunkList
    //            foreach (var editStruct in savingData.EditDataInChunkList)
    //            {
    //                existingData.EditDataInChunk[editStruct.editPos] = editStruct.targetType;
    //            }
    //        }
    //    }

    //    // 将合并后的数据赋值回 savingDatas
    //    savingDatas = mergedSavingData;
    //}



    //获取TheSaving的索引
    public int GetIndexOfChunkLocation(Vector3 location)
    {
        // 遍历 TheSaving 列表
        for (int i = 0; i < TheSaving.Count; i++)
        {
            var savingData = TheSaving[i];
            // 使用 SavingData 的 ContainsChunkLocation 方法检查 ChunkLocation
            if (savingData.ContainsChunkLocation(location))
            {
                return i; // 返回匹配项的索引
            }
        }

        return -1; // 如果没有找到匹配项，则返回 -1
    }


    //返回TheSaving是否包含ChunkLocation
    public bool ContainsChunkLocation(Vector3 location)
    {
        // 遍历 TheSaving 列表
        foreach (var savingData in TheSaving)
        {
            // 使用 SavingData 的 ContainsChunkLocation 方法检查 ChunkLocation
            if (savingData.ContainsChunkLocation(location))
            {
                return true; // 找到匹配的 ChunkLocation
            }
        }

        return false; // 没有找到匹配的 ChunkLocation
    }



    //给定int，返回世界类型的中文
    public String GetWorldTypeString(int WorldType)
    {
        switch (WorldType)
        {
            case 0:
                return "草原群系";
            case 1:
                return "高原群系";
            case 2:
                return "沙漠群系";
            case 3:
                return "沼泽群系";
            case 4:
                return "密林群系";
            case 5:
                return "默认群系";
            case 6:
                return "超平坦世界";
            default:
                return "给定世界类型有误GetWorldTypeChinese";
        }
    }

    //给定游戏模式的中文
    public String GetGameModeString(GameMode gamemode)
    {
        if (gamemode == GameMode.Survival)
        {
            return "生存模式";
        }
        else
        {
            return "创造模式";
        }
    }


    //对玩家碰撞盒的方块判断
    //true：有碰撞
    public bool CheckForVoxel(Vector3 pos)
    {
        //if (GetBlockType(pos) == VoxelData.Wood)
        //{
        //    print("");
        //}

       
        Vector3 realLocation = pos; //绝对坐标
        Vector3 relaLocation = GetRelalocation(realLocation);
        byte targetBlock = GetBlockType(realLocation);

        //出界判断(Chunk)
        if (!Allchunks.ContainsKey(GetChunkLocation(realLocation))) 
        {
            return true; 
        }
        
        //出界判断(Y)
        if (realLocation.y >= VoxelData.ChunkHeight || realLocation.y < 0) 
        {
            return false; 
        }


        //方块碰撞偏移

        //X
        //if (player.Facing.x != 0)
        //{
        //    if (player.Facing.x > 0)
        //    {
        //        realLocation.x -= player.Facing.x * blocktypes[targetBlock].CollisionOffset.Xoffset.y;
        //    }
        //    else
        //    {
        //        realLocation.x -= player.Facing.x * blocktypes[targetBlock].CollisionOffset.Xoffset.x;
        //    }

        //}

        ////Y
        //if (player.Facing.y > 0)
        //{
        //    realLocation.y -= player.Facing.y * blocktypes[targetBlock].CollisionOffset.Yoffset.y;
        //}
        //else if (player.Facing.y < 0)
        //{
        //    realLocation.y -= player.Facing.y * blocktypes[targetBlock].CollisionOffset.Yoffset.x;
        //}

        ////Z
        //if (player.Facing.z != 0)
        //{
        //    if (player.Facing.z > 0)
        //    {
        //        realLocation.z -= player.Facing.z * blocktypes[targetBlock].CollisionOffset.Zoffset.y;
        //    }
        //    else
        //    {
        //        realLocation.z -= player.Facing.z * blocktypes[targetBlock].CollisionOffset.Zoffset.x;
        //    }

        //}

        //如果是自定义碰撞
        if (blocktypes[targetBlock].isDIYCollision)
        {
            realLocation = CollisionOffset(realLocation, targetBlock);
            Vector3 OffsetrelaLocation = GetRelalocation(realLocation);

            if (OffsetrelaLocation != relaLocation)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        


        //返回固体还是空气
        return blocktypes[Allchunks[GetChunkLocation(realLocation)].voxelMap[(int)relaLocation.x, (int)relaLocation.y, (int)relaLocation.z].voxelType].isSolid;

    }

    //public float XOFFSET;
    //public float YOFFSET;
    //public float ZOFFSET;

    public Vector3 CollisionOffset(Vector3 _realPos, byte _targetType)
    {
        Vector3 _input = new Vector3(player.horizontalInput, player.Facing.y, player.verticalInput);
        float _x = _realPos.x; Vector2 _xRange = blocktypes[_targetType].CollosionRange.xRange; float _xOffset = _x - (int)_x; 
        float _y = _realPos.y; Vector2 _yRange = blocktypes[_targetType].CollosionRange.yRange; float _yOffset = _y - (int)_y; 
        float _z = _realPos.z; Vector2 _zRange = blocktypes[_targetType].CollosionRange.zRange; float _zOffset = _z - (int)_z;


        //X
        if (_input.x >= 0 || _xOffset < _xRange.x)
            _x -= _xRange.x;
        else if(_input.x < 0 || _xOffset > _xRange.y)
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
    


    //放置高亮方块的
    public bool eyesCheckForVoxel(Vector3 pos)
    {

        if (!Allchunks.ContainsKey(GetChunkLocation(pos))) { return false; }

        //计算相对坐标
        Vector3 vec = GetRelalocation(new Vector3(pos.x, pos.y, pos.z));

        //判断XOZ上有没有出界
        if (!Allchunks.ContainsKey(GetChunkLocation(pos))) { return true; }

        //判断Y上有没有出界
        if (vec.y >= VoxelData.ChunkHeight) { return false; }

        //竹子返回true
        //if (GetBlockType(pos) == VoxelData.Bamboo) { return true; }

        //返回固体还是空气
        return blocktypes[Allchunks[GetChunkLocation(new Vector3(pos.x, pos.y, pos.z))].voxelMap[(int)vec.x, (int)vec.y, (int)vec.z].voxelType].canBeChoose;

    }

    // 分量乘法
    public Vector3 ComponentwiseMultiply(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    // 处理 Vector2 的分量乘法
    public Vector2 ComponentwiseMultiply(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x * b.x, a.y * b.y);
    }


}


//结构体BlockType
//存储方块种类+面对应的UV
[System.Serializable]
public class BlockType
{
    [Header("方块参数")]
    public string blockName;
    public float DestroyTime;
    public bool isSolid;        //是否会阻挡玩家
    public bool isTransparent;  //周边方块是否面剔除
    public bool canBeChoose;    //是否可被高亮方块捕捉到
    public bool candropBlock;   //是否掉落方块
    public bool IsOriented;     //是否跟随玩家朝向
    public bool isinteractable; //是否可被右键触发
    public bool is2d;           //用来区分显示

    [Header("工具参数")]
    public bool isTool;         //区分功能性
    public bool isNeedRotation; //true后会做一定的旋转


    [Header("自定义碰撞")]
    public bool isDIYCollision;
    //抽象来说就是方块向内挤压的数值
    //对于Y来说，(0.5f,0,0f)，就是Y正方向的面向内挤压0.5f，Y负方向的面向内挤压0.0f，即台阶的碰撞参数
    public CollosionRange CollosionRange;

    [Header("Sprits")]
    public Sprite icon; //物品栏图标
    public Sprite sprite; //掉落物
    public Sprite top_sprit; //掉落物
    public Sprite buttom_sprit; //掉落物


    [Header("音乐")]
    public AudioClip[] walk_clips = new AudioClip[2]; 
    public AudioClip broking_clip;
    public AudioClip broken_clip;


    [Header("绘制")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    public DrawMode DrawMode;

    [Header("面生成判断(后前上下左右)")]
    public bool GenerateTwoFaceWithAir;    //如果朝向空气，则双面绘制
    public List<FaceCheckMode> OtherFaceCheck; 

  

    //贴图中的面的坐标
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
                Debug.Log($"Error in GetTextureID; invalid face index {faceIndex}");
                return 0;


        }

    }


}


//工具类
//[System.Serializable]
//public class ToolType
//{
//    public string name;
//    public Sprite sprite;
//}

//方块种类结构体
public class VoxelStruct
{
    public byte voxelType = VoxelData.Air;
    public int blockOriented = 0;

    //面生成的六个方向
    public bool up = true;
}


//群系系统
[System.Serializable]
public class BiomeNoiseSystem
{
    public string BiomeName;
    public Color BiomeColor;
    public Vector2 HighDomain;
    public Vector3 Noise_Scale_123;
    public Vector3 Noise_Rank_123;
}


//地质分层与概率系统
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
    public int 密林树木采样次数Forest_treecount;
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



// 一个完整的WorldSetting
[Serializable]
public class WorldSetting
{
    public String date = "0000";//存档创建日期
    public String name = "新的世界";
    public int seed = 0;
    public GameMode gameMode = GameMode.Survival;
    public int worldtype = VoxelData.Biome_Default;
    public Vector3 playerposition;   // 保存玩家的坐标
    public Quaternion playerrotation; // 保存玩家的旋转

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


//玩家修改的数据缓存
[System.Serializable]
public class EditStruct
{
    public Vector3 editPos;
    public byte targetType;
    

    public EditStruct(Vector3 _editPos, byte _targetType)
    {
        editPos = _editPos;
        targetType = _targetType;
    }

}



//最终保存的结构体
[System.Serializable]
public class SavingData
{
    public Vector3 ChunkLocation;
    public List<EditStruct> EditDataInChunkList = new List<EditStruct>();

    // 为了兼容反序列化后还原为Dictionary
    [System.NonSerialized]
    public Dictionary<Vector3, byte> EditDataInChunk = new Dictionary<Vector3, byte>();

    public SavingData(Vector3 _vec, Dictionary<Vector3, byte> _D)
    {
        ChunkLocation = _vec;
        EditDataInChunk = _D;

        // 将字典转换为列表
        foreach (var kvp in _D)
        {
            EditDataInChunkList.Add(new EditStruct(kvp.Key, kvp.Value));
        }
    }

    // 在反序列化后还原Dictionary
    public void RestoreDictionary()
    {
        EditDataInChunk = new Dictionary<Vector3, byte>();
        foreach (var structItem in EditDataInChunkList)
        {
            EditDataInChunk[structItem.editPos] = structItem.targetType;
        }
    }

    // 检查是否包含指定的 ChunkLocation
    public bool ContainsChunkLocation(Vector3 location)
    {
        return ChunkLocation == location;
    }
}


// 为List对象创建一个封装类，以便能够将其转换为JSON
[System.Serializable]
public class Wrapper<T>
{
    public List<T> Items;

    public Wrapper(List<T> items)
    {
        Items = items;
    }
}

//方块碰撞类
[System.Serializable]
public class CollosionRange
{
    public Vector2 xRange = new Vector3(0 ,1f);
    public Vector2 yRange = new Vector3(0, 1f);
    public Vector2 zRange = new Vector3(0, 1f);
}

