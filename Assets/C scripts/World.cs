using JetBrains.Annotations;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
//using System.Diagnostics;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
//using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;



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

    Block,Bush,Torch,Air,Water,

}

public class World : MonoBehaviour
{

    [Header("Transforms")]
    public TMP_InputField input_Seed;
    public TMP_InputField input_RenderSize;


    [Header("游戏状态")]
    public Game_State game_state = Game_State.Start;
    public GameMode game_mode = GameMode.Survival; 


    [Header("Material-方块类型")]
    public Material material;
    public Material material_Water;
    public BlockType[] blocktypes;


    [Header("World-渲染设置")]
    [Tooltip("4就是边长为4*16的正方形")]
    public int renderSize = 5; //渲染区块半径,即renderSize*16f
    [Tooltip("2就是接近2*16的时候开始刷新区块")]
    public float StartToRender = 1f;
    public float DestroySize = 7f;


    [Header("Biome-平原参数")]
    //noise2d越小，噪声拉的越长，地形与地形之间过度更自然
    //值越小，噪声缩小，地形越紧凑
    //所以越大越好
    public float noise2d_scale_smooth = 0.01f;
    public float noise2d_scale_steep = 0.04f;


    [Header("Cave-洞穴系统")]
    //noise3d_scale越大洞穴破损越严重，越大越好
    //cave_width越小洞穴平均宽度变小，不能太大，不然全是洞
    public bool debug_CanLookCave = false;
    public float noise3d_scale = 0.085f;
    public float cave_width = 0.45f;


    [Header("Chunk-分层结构")]
    public bool isRandomSeed = true;
    public int Seed = 0;
    [Range(0, 60)] public float soil_min = 15;
    [Range(0, 60)] public float soil_max = 55;
    [Range(0, 60)] public float sea_level = 30;
    public int TreeCount = 5;
    public int TreeHigh_min = 5;
    public int TreeHigh_max = 7;


    [Header("生成概率(n分之一)")]
    public int Random_Coal;
    public int Random_Iron;
    public int Random_Gold;
    public int Random_Blue_Crystal;
    public int Random_Diamond;


    [Header("生成概率(n%)")]
    public float Random_Bush;
    public int Random_Bamboo;
    public float Random_BlueFlower;
    public float Random_WhiteFlower1;
    public float Random_WhiteFlower2;
    public float Random_YellowFlower;


    //玩家
    [Header("Player-玩家脚底坐标")]
    public Transform PlayerFoot;
    [HideInInspector]
    public byte ERROR_CODE_OUTOFVOXELMAP = 255;
    [HideInInspector]
    public Vector3 Start_Position = new Vector3(1600f, 63f, 1600f);


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


    //UI Manager
    [HideInInspector]
    public float initprogress = 0f;


    //Chunks父级
    [HideInInspector]
    public GameObject Chunks;


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
    [HideInInspector]public bool MeshLock = false;
    public ConcurrentQueue<Chunk> WaitToCreateMesh = new ConcurrentQueue<Chunk>();
    Coroutine Mesh_Coroutine;


    //Init
    [HideInInspector] public bool InitError = false;


    //----------------------------------周期函数---------------------------------------




    private void Start()
    {

        //帧数
        Application.targetFrameRate = 90;

        //Self
        Chunks = new GameObject();
        Chunks.name = "Chunks";
        Chunks.transform.SetParent(GameObject.Find("Environment").transform);

        //设置种子
        Seed = Random.Range(0, 100);

        //sea_level = Random.Range(20, 39);

        //初始化一个小岛
        Start_Screen_Init();

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
            if (GetVector3Length(PlayerFoot.transform.position - Center_Now) > (StartToRender * 16f))
            {

                //更新Center
                Center_direction = VtoNormal(PlayerFoot.transform.position - Center_Now);
                Center_Now += Center_direction * VoxelData.ChunkWidth;

                //添加Chunk
                AddtoCreateChunks(Center_direction);
                AddtoRemoveChunks(Center_direction);

            }


        }

    }



    void OnApplicationQuit()
    {

        //结束游戏
        game_state = Game_State.Ending;

        //print("Quit");
        RenderSettings.skybox.SetFloat("_Exposure", 0.69f);

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




    //---------------------------------------------------------------------------------------










    //----------------------------------World Options---------------------------------------




    //主菜单地图
    public void Start_Screen_Init()
    {

        Chunk chunk_temp = new Chunk(new Vector3(0, 0, 0), this ,true);

        //GameObject chunkGameObject = new GameObject("TheMenuChunk");
        //Chunk chunk = chunkGameObject.AddComponent<Chunk>();
        //chunk.InitChunk(new Vector3(0, 0, 0), this);

    }

    //检查种子
    public void CheckSeed()
    {

        if (input_Seed != null && string.IsNullOrEmpty(input_Seed.text))
        {

            //Debug.Log("种子为空！");

        } 
        else
        {
            //Debug.Log("种子不为空！");

            //设置种子
            int number;

            if (int.TryParse(input_Seed.text, out number))
            {

                // 转换成功，number 中存储了输入字段中的数字
                //Debug.Log("种子为: " + number);

                if (number > 0)
                {

                    Seed = number;

                }
                else
                {

                    InitError = true;
                    Debug.Log("种子转换失败！");

                }
                

                //设置水平面
                //sea_level = Random.Range(20, 42); 

            }
            else
            {

                InitError = true;
                // 转换失败，输入字段中的字符串不是有效的整数
                Debug.Log("种子转换失败！");

            }


        }

    }



    //检查渲染范围
    public void CheckRenderSize()
    {

        //size如果是6则跳过，否则则赋值
        int number;

        if (int.TryParse(input_RenderSize.text, out number))
        {

            // 转换成功，number 中存储了输入字段中的数字
            //Debug.Log("种子为: " + number);
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

            // 转换失败，输入字段中的字符串不是有效的整数
            InitError = true;
            Debug.Log("RenderSIze转换失败！");

        }

    }

    //初始化地图
    IEnumerator Init_Map_Thread()
    {
        
        Center_Now = new Vector3(GetRealChunkLocation(PlayerFoot.transform.position).x, 0, GetRealChunkLocation(PlayerFoot.transform.position).z);

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
                initprogress = Mathf.Lerp(0f, 0.9f, temp / max);

                yield return new WaitForSeconds(InitCorountineDelay);
            }

        }

        //重新初始化玩家位置，放置穿模
        Init_Player_Location();

        //游戏开始
        yield return new WaitForSeconds(0.5f);
        initprogress = 1f;

        //开启面优化协程
        StartCoroutine(Chunk_Optimization());

        //开启持续渲染水体流动线程
        //myThread_Water = new Thread(new ThreadStart(Thread_AwaysUpdate_Water));
        //myThread_Water.Start();

        //开启渲染Mesh线程
        //myThread_Render = new Thread(new ThreadStart(Thread_RenderMesh));
        //myThread_Render.Start();

    }


    //更新中心区块
    public void Update_CenterChunks()
    {
    
        //update加载中心区块
        StartCoroutine(Init_Map_Thread());
    
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

    //一直更新水的线程
    void Thread_AwaysUpdate_Water()
    {
        int 次数 = 0;
        int 个数 = 0;

        //一直循环
        while (game_state != Game_State.Ending)
        {

            lock (Allchunks_Lock)
            {

                //遍历所有AllChunks
                foreach (var chunktemp in Allchunks)
                {

                    //如果区块包含水，且在12区块内，则更新
                    if (chunktemp.Value.iHaveWater && GetVector3Length(chunktemp.Value.myposition - Center_Now) > MaxDistant_RenderFlowWater)
                    {

                        chunktemp.Value.Always_updateWater();
                        //print($"刷新了{chunktemp.Value.name}");

                        个数++;

                    }

                }

            }

            




            //休眠5秒钟
            次数++;
            print($"第{次数}次刷新，一共刷新{个数}个");
            个数 = 0;
            Thread.Sleep(Delay_RenderFlowWater * 1000);

        }

        Debug.LogError("Water线程中止");
        
    }




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
        Chunk chunk_temp = new Chunk(new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z)), this, true);

        //GameObject chunkGameObject = new GameObject($"{Mathf.FloorToInt(pos.x)}, 0, {Mathf.FloorToInt(pos.z)}");
        //Chunk chunktemp = chunkGameObject.AddComponent<Chunk>();
        //chunktemp.InitChunk(new Vector3(0, 0, 0), this);

        //添加到字典
        Allchunks.Add(pos, chunk_temp);

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

        //调用Chunk
        Chunk chunk_temp = new Chunk(new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z)), this, false);

        //GameObject chunkGameObject = new GameObject($"{Mathf.FloorToInt(pos.x)}, 0, {Mathf.FloorToInt(pos.z)}");
        //Chunk chunktemp = chunkGameObject.AddComponent<Chunk>();
        //chunktemp.InitChunk(new Vector3(0, 0, 0), this);

        //添加到字典
        Allchunks.Add(pos, chunk_temp);

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
    void dynamicRandertime(float nowtime)
    {

        if (nowtime > RenderDelay)
        {

            RenderDelay = nowtime;

        }

    }



    //Mesh协程
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

                //print($"{GetChunkLocation(chunktemp.myposition)}添加到meshQueue");

                //Mesh线程
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



    //初始化人物位置
    void Init_Player_Location()
    {

        Start_Position = new Vector3(GetRealChunkLocation(PlayerFoot.transform.position).x, PlayerFoot.transform.position.y, GetRealChunkLocation(PlayerFoot.transform.position).z);
        

        //从<1600,63,1600>向下遍历，直到坐标符合条件
        while (GetBlockType(Start_Position) == VoxelData.Air)
        {

            Start_Position.y -= 1f;
        
        }

        Start_Position.y += 2f;


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

        return new Vector3(Mathf.FloorToInt(vec.x % VoxelData.ChunkWidth), Mathf.FloorToInt(vec.y), Mathf.FloorToInt(vec.z % VoxelData.ChunkWidth));

    }


    //返回方块类型
    public byte GetBlockType(Vector3 pos)
    {

        Allchunks.TryGetValue(GetChunkLocation(pos), out Chunk chunktemp);

        //如果玩家在刷新区外
        //if (chunktemp == null)
        //{
        //    return VoxelData.notChunk;
        //}

        //如果玩家在区内，但Y值太高

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







    //------------------------------------工具------------------------------------------------



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





    //----------------------------------------------------------------------------------------




    //对玩家碰撞盒的方块判断
    //true：有碰撞
    public bool CheckForVoxel(Vector3 pos)
    {

        //计算相对坐标
        Vector3 vec = GetRelalocation(new Vector3(pos.x, pos.y, pos.z));

        //查一下该地形是否存在
        //Allchunks.TryGetValue(GetChunkLocation(pos), out obj);
        //if (obj.myState == false)
        //{
        //    return true;
        //}

        //判断XOZ上有没有出界
        if (!Allchunks.ContainsKey(GetChunkLocation(pos))) { return true; }

        //判断Y上有没有出界
        if (vec.y >= VoxelData.ChunkHeight) { return false; }

        //竹子返回false
        if (GetBlockType(pos) == VoxelData.Bamboo) { return false; }

        //返回固体还是空气
        return blocktypes[Allchunks[GetChunkLocation(new Vector3(pos.x, pos.y, pos.z))].voxelMap[(int)vec.x, (int)vec.y, (int)vec.z]].isSolid;

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
        return blocktypes[Allchunks[GetChunkLocation(new Vector3(pos.x, pos.y, pos.z))].voxelMap[(int)vec.x, (int)vec.y, (int)vec.z]].canBeChoose;

    }




}


//结构体BlockType
//存储方块种类+面对应的UV
[System.Serializable]
public class BlockType
{

    public string blockName;
    public float DestroyTime;
    public bool isSolid;       //是否会阻挡玩家
    public bool isTransparent; //周边方块是否面剔除
    public bool canBeChoose;   //是否可被高亮方块捕捉到
    public bool candropBlock;  //是否掉落方块


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
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;


        }

    }

}