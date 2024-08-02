//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;

public class Chunk : MonoBehaviour
{
    //state
    public bool isShow = true;
    public bool isReadyToRender = false;
    public bool BaseChunk;
    public bool isCalled = false;  //不要删我！！

    //Transform
    World world;
    public GameObject chunkObject;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    //噪声
    private float noise2d_scale_smooth;
    private float noise2d_scale_steep;
    private float noise3d_scale;

    //BlockMap
    private int x;
    private int y;
    private int z;
    public byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    //Mesh
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    //群系参数
    int treecount;

    //生长类方块
    Queue<Vector3> Coals = new Queue<Vector3>();
    Queue<Vector3> Bamboos = new Queue<Vector3>();

    //矿洞
    float caveWidth;
    public float mean = 16f; // 均值
    public float stdDev = 5f; // 标准差

    //多线程变量
    public System.Random rand;
    public Vector3 myposition;

    //debug
    bool debug_CanLookCave;


    //---------------------------------- 周期函数 ---------------------------------------




    //Start()
    public Chunk(Vector3 thisPosition, World _world, bool _BaseChunk)
    {
        //World
        world = _world;
        noise2d_scale_smooth = world.noise2d_scale_smooth;
        noise2d_scale_steep = world.noise2d_scale_steep;
        noise3d_scale = world.noise3d_scale;
        treecount = world.TreeCount;
        caveWidth = world.cave_width;
        debug_CanLookCave = !world.debug_CanLookCave;
        BaseChunk = _BaseChunk;

        //Self
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = world.material;
        chunkObject.transform.SetParent(world.Chunks.transform);
        chunkObject.transform.position = new Vector3(thisPosition.x * VoxelData.ChunkWidth, 0f, thisPosition.z * VoxelData.ChunkWidth);
        chunkObject.name = thisPosition.x + "," + thisPosition.z;
        myposition = chunkObject.transform.position;

        //Data线程
        rand = new System.Random(world.Seed);
        Thread myThread = new Thread(new ThreadStart(CreateData));
        myThread.Start();


        //print($"----------------------------------------------");
        //print($"{world.GetChunkLocation(myposition)}已经生成！");
    }


    //-----------------------------------------------------------------------------------






    //----------------------------------- Noise ----------------------------------------

    //地形噪声生成器
    float GetTotalNoiseHigh(int _x, int _z)
    {
        //(平原-山脉)过度噪声
        float biome_moutainAndPlane = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((float)_x * 1e-05f + myposition.x * 1e-05f, (float)_z * 1e-05f + myposition.z * 1e-05f));
        
        //小：平原噪声
        //大：山脉噪声
        float soilmax = Mathf.Lerp(50, 64, biome_moutainAndPlane);
        float smooth = Mathf.Lerp(0.002f, 0.04f, biome_moutainAndPlane);
        float steep = Mathf.Lerp(0.004f, 0.05f, biome_moutainAndPlane);

        //最终噪声
        float noise2d_1 = Mathf.Lerp((float)20, (float)soilmax, Mathf.PerlinNoise((float)_x * smooth + myposition.x * smooth, (float)_z * smooth + myposition.z * smooth));
        float noise2d_2 = Mathf.Lerp((float)20, (float)soilmax, Mathf.PerlinNoise((float)_x * steep + myposition.x * steep, (float)_z * steep + myposition.z * steep));
        float noiseHigh = noise2d_1 * 0.7f + noise2d_2 * 0.3f;

        return noiseHigh;
    }

    //Tree简单噪声
    float GetSmoothNoise_Tree()
    {
        float randomoffset = rand.Next(0, 10);
        float Offset_x = 100f * randomoffset;
        float Offset_z = 100f * randomoffset;

        float smoothNoise = Mathf.Lerp((float)0, (float)100, Mathf.PerlinNoise(  (myposition.x + Offset_x) * 0.005f,     (myposition.z + Offset_z) * 0.005f)   );
        
        return smoothNoise;
    }

    //Desert简单噪声
    float GetSmoothNoise_Desert(int _x,int _z)
    {
        //float randomoffset = rand.Next(0, 10);
        //float Offset_x = 100f * randomoffset;
        //float Offset_z = 100f * randomoffset;

        return Mathf.PerlinNoise((_x + myposition.x) * 0.003f, (_z + myposition.z) * 0.003f);
    }

    //洞穴噪声生成器
    float GetCaveNoise(int _x, int _y, int _z)
    {
        return Perlin3D(( (float)_x + myposition.x) * noise3d_scale,     ((float)_y + y) * noise3d_scale,      ((float)_z + myposition.z )* noise3d_scale); // 将100改为0.1

    }

    //洞穴概率递减器
    float GetVaveWidth(int _y)
    {
        if (_y >= 3 && _y <= 6)
        {
            return Mathf.Lerp(0, caveWidth, (_y - 3) / 3);
        }
        else
        {
            return caveWidth;
        }
    }


    //-----------------------------------------------------------------------------------






    //---------------------------------- Data线程 ---------------------------------------


    //Data
    void CreateData()
    {
        //对一个chunk进行遍历
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    // 生成0或1的随机数
                    int randomInt = rand.Next(0, 2);
                    int randomFrom_0_10 = rand.Next(0, 10);

                    //地形噪声
                    float noiseHigh = GetTotalNoiseHigh(x, z);

                    //矿洞噪声
                    float noise3d = GetCaveNoise(x, y, z);

                    //沙漠噪声
                    float noise_desery = GetSmoothNoise_Desert(x, z);

                    //判断基岩
                    //0~3层不准生成矿洞
                    if (y >= 0 && y <= 3)
                    {
                        if (y == 0)
                        {
                            voxelMap[x, y, z] = VoxelData.BedRock;
                        }
                        else if (y > 0 && y < 3 && randomInt == 1)
                        {
                            voxelMap[x, y, z] = VoxelData.BedRock;
                        }
                        else
                        {
                            voxelMap[x, y, z] = VoxelData.Stone;
                        }
                    }
                    //空气部分
                    else if (y > noiseHigh && y > world.sea_level)
                    {
                        //地上一层
                        if (y - 1 < noiseHigh)
                        {
                            //如果可生成
                            if (voxelMap[x, y - 1, z] != VoxelData.Sand && voxelMap[x, y - 1, z] != VoxelData.Air)
                            {
                                //灌木丛
                                if (GetProbability(world.Random_Bush))
                                {
                                    voxelMap[x, y, z] = VoxelData.Bush;
                                }
                                //BlueFlower
                                else if (GetProbability(world.Random_BlueFlower))
                                {
                                    voxelMap[x, y, z] = VoxelData.BlueFlower;
                                }
                                //WhiteFlower_1
                                else if (GetProbability(world.Random_WhiteFlower1))
                                {
                                    voxelMap[x, y, z] = VoxelData.WhiteFlower_1;
                                }
                                //WhiteFlower_2
                                else if (GetProbability(world.Random_WhiteFlower2))
                                {
                                    voxelMap[x, y, z] = VoxelData.WhiteFlower_2;
                                }
                                //YellowFlower
                                else if (GetProbability(world.Random_YellowFlower))
                                {
                                    voxelMap[x, y, z] = VoxelData.YellowFlower;
                                }
                                else
                                {
                                    voxelMap[x, y, z] = VoxelData.Air;
                                }
                            }
                            //竹子
                            else if (voxelMap[x, y - 1, z] == VoxelData.Sand && GetProbability(world.Random_Bamboo))
                            {
                                voxelMap[x, y, z] = VoxelData.Air;
                                Bamboos.Enqueue(new Vector3(x, y, z));
                            }
                            else
                            {
                                voxelMap[x, y, z] = VoxelData.Air;
                            }
                        }
                        else
                        {
                            voxelMap[x, y, z] = VoxelData.Air;
                        }
                    }
                    //判断水面
                    else if (y > noiseHigh && y - 1 < world.sea_level)
                    {
                        voxelMap[x, y, z] = VoxelData.Water;
                    }
                    //空气之下
                    else
                    {
                        //地表
                        if ((y + 1) > noiseHigh)
                        {
                            //沙漠气候
                            if (noise_desery > 0.5f)
                            {
                                voxelMap[x, y, z] = VoxelData.Sand;
                            }
                            //草原气候
                            else
                            {
                                //高于海平面
                                if (y > world.sea_level)
                                {
                                    voxelMap[x, y, z] = VoxelData.Grass;
                                }
                                else
                                {
                                    if (randomFrom_0_10 > 3)
                                    {
                                        voxelMap[x, y, z] = VoxelData.Sand;
                                    }
                                    else
                                    {
                                        voxelMap[x, y, z] = VoxelData.Soil;
                                    }
                                }
                            }
                        }
                        //泥土的判断
                        else if (y > noiseHigh - 7)
                        {
                            //沙漠判断
                            if (noise_desery > 0.5f)
                            {
                                voxelMap[x, y, z] = VoxelData.Sand;
                            }
                            else
                            {
                                voxelMap[x, y, z] = VoxelData.Soil;
                            }
                        }
                        else if (y >= (noiseHigh - 10) && y <= (noiseHigh - 7) && randomInt == 1)
                        {
                            //沙漠判断
                            if (noise_desery > 0.5f)
                            {
                                voxelMap[x, y, z] = VoxelData.Sand;
                            }
                            else
                            {
                                voxelMap[x, y, z] = VoxelData.Soil;
                            }
                        }
                        //矿洞
                        else if (noise3d < GetVaveWidth(y))
                        {
                            voxelMap[x, y, z] = VoxelData.Air;
                        }
                        //地下判断
                        else
                        {
                            //煤炭
                            if (rand.Next(0, world.Random_Coal) < 1)
                            {
                                voxelMap[x, y, z] = VoxelData.Stone;
                                Coals.Enqueue(new Vector3(x, y, z));
                            }
                            //铁
                            else if (rand.Next(0, world.Random_Iron) < 1)
                            {
                                voxelMap[x, y, z] = VoxelData.Iron;
                            }
                            //金
                            else if (rand.Next(0, world.Random_Gold) < 1)
                            {
                                voxelMap[x, y, z] = VoxelData.Gold;
                            }
                            //青金石
                            else if (rand.Next(0, world.Random_Blue_Crystal) < 1)
                            {
                                voxelMap[x, y, z] = VoxelData.Blue_Crystal;
                            }
                            //钻石
                            else if (rand.Next(0, world.Random_Diamond) < 1)
                            {
                                voxelMap[x, y, z] = VoxelData.Diamond;
                            }
                            else
                            {
                                voxelMap[x, y, z] = VoxelData.Stone;
                            }
                        }
                    }
                }
            }
        }

        //补充树
        CreateTree();

        //补充煤炭
        foreach (var item in Coals)
        {
            CreateCoal((int)item.x, (int)item.y, (int)item.z);
        }

        //补充竹子
        foreach (var item in Bamboos)
        {
            CreateBamboo((int)item.x, (int)item.y, (int)item.z);
        }

        //交给world来create
        world.WaitToCreateMesh.Enqueue(this);
    }



    //---------------------------------- Tree ----------------------------------------

    //tree
    void CreateTree()
    {

        if (GetSmoothNoise_Tree() > 35f && GetSmoothNoise_Tree() < 60)
        {
            //[确定XZ]xoz上随便选择5个点
            while (treecount-- != 0)
            {

                int random_x = rand.Next(2, VoxelData.ChunkWidth - 2);
                int random_z = rand.Next(2, VoxelData.ChunkWidth - 2);
                int random_y = VoxelData.ChunkHeight;
                int random_Tree_High = rand.Next(world.TreeHigh_min, world.TreeHigh_max + 1);

                //如果可以生成树桩
                //向上延伸树干
                random_y = CanSetStump(random_x, random_y, random_z, random_Tree_High);
                if (random_y != -1f)
                {
                    for (int i = 0; i <= random_Tree_High; i++)
                    {
                        if (random_y + i >= VoxelData.ChunkHeight - 1)
                        {
                            Debug.Log($"random_y:{random_y},i={i}");
                        }
                        else
                        {
                            voxelMap[random_x, random_y + i, random_z] = VoxelData.Wood;
                        }

                    }

                    //生成树叶
                    BuildLeaves(random_x, random_y + random_Tree_High, random_z);

                }


                //Debug.Log($"{random_x}, {random_y}, {random_z}");
            }
        }

        
    }

    // 返回一个概率值，范围在0~100，根据输入值越接近100，概率接近100%，越接近0，概率接近0%
    bool GetProbability(float input)
    {
        if (input > 50f)
        {
            // 如果输入值大于45，总是返回true
            return true;
        }
        else
        {
            // 如果输入值在0~45之间，根据概率返回true或者false
            float probability = rand.Next(0, 200);
            return probability <= input;
        }
    }

    //构造树叶
    void BuildLeaves(int _x, int _y, int _z)
    {

        int randomInt = rand.Next(0, 2);


        //第一层
        if (randomInt == 0)
        {
            CreateLeaves(_x, _y + 1, _z);
        }
        else if (randomInt == 1)
        {
            CreateLeaves(_x, _y + 1, _z + 1);
            CreateLeaves(_x - 1, _y + 1, _z);
            CreateLeaves(_x, _y + 1, _z);
            CreateLeaves(_x + 1, _y + 1, _z);
            CreateLeaves(_x, _y + 1, _z - 1);
        }

        //第二层
        CreateLeaves(_x - 1, _y, _z);
        CreateLeaves(_x + 1, _y, _z);
        CreateLeaves(_x, _y, _z - 1);
        CreateLeaves(_x, _y, _z + 1);

        //第三层
        CreateLeaves(_x - 1, _y - 1, _z + 2);
        CreateLeaves(_x, _y - 1, _z + 2);
        CreateLeaves(_x + 1, _y - 1, _z + 2);

        CreateLeaves(_x - 2, _y - 1, _z + 1);
        CreateLeaves(_x - 1, _y - 1, _z + 1);
        CreateLeaves(_x, _y - 1, _z + 1);
        CreateLeaves(_x + 1, _y - 1, _z + 1);
        CreateLeaves(_x + 2, _y - 1, _z + 1);

        CreateLeaves(_x - 2, _y - 1, _z);
        CreateLeaves(_x - 1, _y - 1, _z);
        CreateLeaves(_x + 1, _y - 1, _z);
        CreateLeaves(_x + 2, _y - 1, _z);

        CreateLeaves(_x - 2, _y - 1, _z - 1);
        CreateLeaves(_x - 1, _y - 1, _z - 1);
        CreateLeaves(_x, _y - 1, _z - 1);
        CreateLeaves(_x + 1, _y - 1, _z - 1);
        CreateLeaves(_x + 2, _y - 1, _z - 1);

        CreateLeaves(_x - 1, _y - 1, _z - 2);
        CreateLeaves(_x, _y - 1, _z - 2);
        CreateLeaves(_x + 1, _y - 1, _z - 2);

        //第四层
        CreateLeaves(_x - 1, _y - 2, _z + 2);
        CreateLeaves(_x, _y - 2, _z + 2);
        CreateLeaves(_x + 1, _y - 2, _z + 2);

        CreateLeaves(_x - 2, _y - 2, _z + 1);
        CreateLeaves(_x - 1, _y - 2, _z + 1);
        CreateLeaves(_x, _y - 2, _z + 1);
        CreateLeaves(_x + 1, _y - 2, _z + 1);
        CreateLeaves(_x + 2, _y - 2, _z + 1);

        CreateLeaves(_x - 2, _y - 2, _z);
        CreateLeaves(_x - 1, _y - 2, _z);
        CreateLeaves(_x + 1, _y - 2, _z);
        CreateLeaves(_x + 2, _y - 2, _z);

        CreateLeaves(_x - 2, _y - 2, _z - 1);
        CreateLeaves(_x - 1, _y - 2, _z - 1);
        CreateLeaves(_x, _y - 2, _z - 1);
        CreateLeaves(_x + 1, _y - 2, _z - 1);
        CreateLeaves(_x + 2, _y - 2, _z - 1);

        CreateLeaves(_x - 1, _y - 2, _z - 2);
        CreateLeaves(_x, _y - 2, _z - 2);
        CreateLeaves(_x + 1, _y - 2, _z - 2);

    }

    //树桩生成判断
    int CanSetStump(int _x,int _y,int _z, int treehigh)
    {

        if (GetProbability(_y))
        {
            while (_y > 0)
            {
                //如果不是泥土或者草地则不生成
                if (voxelMap[_x, _y - 1, _z] != VoxelData.Air)
                {
                    if (voxelMap[_x, _y - 1, _z] == VoxelData.Grass || voxelMap[_x, _y - 1, _z] == VoxelData.Soil)
                    {

                        //判断树干是否太高
                        if (_y + treehigh + 3 > VoxelData.ChunkHeight - 1)
                        {
                            return -1;
                        }
                        else
                        {
                            return _y;
                        }


                    }
                    else
                    {
                        return -1;
                    }
                }
                
                _y--;

                //如果树顶超过最大高度，不生成
                //else if (random_y + random_Tree_High >= VoxelData.ChunkHeight)
                //{
                //    needTree = false;
                //    break;
                //}
            }
        }

        

        return -1;

    }

    //树叶生成判断
    //leaves设定值，防止碰到树木
    void CreateLeaves(int x, int y, int z)
    {
        //如果是固体，就不用生成树叶了
        if (voxelMap[x, y, z] != VoxelData.Air)
        {
            return;
        }
        else
        {
            voxelMap[x, y, z] = VoxelData.Leaves;
        }
    }

    //---------------------------------- Coal ----------------------------------------

    //煤炭
    void CreateCoal(int xtemp, int ytemp, int ztemp)
    {
        int random_Coal_up = rand.Next(0, 100);
        int random_Coal_down = rand.Next(0, 100);

        //上一层
        if (random_Coal_up < 50)
        {
            SetCoal(xtemp - 1, ytemp + 1, ztemp - 1);
            SetCoal(xtemp, ytemp + 1, ztemp - 1);
            SetCoal(xtemp + 1, ytemp + 1, ztemp - 1);

            SetCoal(xtemp - 1, ytemp + 1, ztemp);
            SetCoal(xtemp, ytemp + 1, ztemp - 1);
            SetCoal(xtemp + 1, ytemp + 1, ztemp);

            SetCoal(xtemp - 1, ytemp + 1, ztemp + 1);
            SetCoal(xtemp, ytemp + 1, ztemp + 1);
            SetCoal(xtemp + 1, ytemp + 1, ztemp + 1);
        }


        //中一层
        SetCoal(xtemp - 1, ytemp, ztemp - 1);
        SetCoal(xtemp, ytemp, ztemp - 1);
        SetCoal(xtemp + 1, ytemp, ztemp - 1);

        SetCoal(xtemp - 1, ytemp, ztemp);
        SetCoal(xtemp, ytemp, ztemp);
        SetCoal(xtemp + 1, ytemp, ztemp);

        SetCoal(xtemp - 1, ytemp, ztemp + 1);
        SetCoal(xtemp, ytemp, ztemp + 1);
        SetCoal(xtemp + 1, ytemp, ztemp + 1);

        //下一层
        if (random_Coal_down < 50)
        {
            SetCoal(xtemp - 1, ytemp - 1, ztemp - 1);
            SetCoal(xtemp, ytemp - 1, ztemp - 1);
            SetCoal(xtemp + 1, ytemp - 1, ztemp - 1);

            SetCoal(xtemp - 1, ytemp - 1, ztemp);
            SetCoal(xtemp, ytemp - 1, ztemp - 1);
            SetCoal(xtemp + 1, ytemp - 1, ztemp);

            SetCoal(xtemp - 1, ytemp - 1, ztemp + 1);
            SetCoal(xtemp, ytemp - 1, ztemp + 1);
            SetCoal(xtemp + 1, ytemp - 1, ztemp + 1);
        }
    }

    //煤炭生成判断
    //如果是空气 || 出界则不生成
    void SetCoal(int _x, int _y, int _z)
    {
        //如果出界
        if (isOutOfRange(_x, _y, _z))
        {
            return;
        }
        else
        {
            if (voxelMap[_x, _y, _z] == VoxelData.Stone)
                voxelMap[_x, _y, _z] = VoxelData.Coal;
        }


    }

    //---------------------------------- Bamboo ----------------------------------------

    //生成竹子
    void CreateBamboo(int x, int y, int z)
    {
        //先确定根
        if (BambooJudge(x, y, z))
        {
            //向上延申1~2根
            //如果是空气则覆盖为竹子
            for (int temp = 0; temp < rand.Next(1, 4); temp++)
            {
                voxelMap[x, y + temp, z] = VoxelData.Bamboo;
            }

        }



    }

    //生成判断
    //如果脚下有一个为水，则为true
    bool BambooJudge(int x, int y, int z)
    {
        for (int _x = 0; _x < 1; _x++)
        {
            for (int _z = 0; _z < 1; _z++)
            {
                if (voxelMap[_x, y - 1, _z] == VoxelData.Water)
                {
                    return true;
                }
            }
        }

        return false;
    }

    //竹子断裂判断
    void updateBamboo()
    {
        //如果自己是竹子 && 自己下面是空气 则自己变为空气
        if (voxelMap[x, y, z] == VoxelData.Bamboo && voxelMap[x, y - 1, z] == VoxelData.Air)
        {
            voxelMap[x, y, z] = VoxelData.Air;
        }
    }

    //灌木丛消失
    void updateBush()
    {
        //如果自己是Bush && 自己下面是空气 则自己变为空气
        if (voxelMap[x, y, z] == VoxelData.Bush && voxelMap[x, y - 1, z] == VoxelData.Air)
        {
            voxelMap[x, y, z] = VoxelData.Air;
        }
    }

    //------------------------------------------------------------------------------------






    //---------------------------------- Mesh部分 ----------------------------------------

    //开始遍历
    public void UpdateChunkMesh_WithSurround() //参数重载，如果不写默认为false
    {

        UpdateChunkMesh_WithSurround(false);

    }

    public void UpdateChunkMesh_WithSurround(object obj)
    {
        bool iscaller = (bool)obj;

        ClearMeshData();

        //刷新自己
        for (y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    //竹子断裂
                    updateBamboo();

                    //Bush消失
                    updateBush();

                    //(绘制模式不是空气 || 是水 || 是水面上一层 || 是竹子)才生成
                    if (world.blocktypes[voxelMap[x, y, z]].DrawMode != DrawMode.Air || voxelMap[x, y, z] == VoxelData.Water || voxelMap[x, y - 1, z] == VoxelData.Water)
                        UpdateMeshData(new Vector3(x, y, z));

                    //UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }

        //通知周围方块刷新
        if (iscaller)
        {
            Chunk DirectChunk;
            if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + new Vector3(0.0f, 0.0f, 1.0f), out DirectChunk))
                DirectChunk.UpdateChunkMesh_WithSurround();
            if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + new Vector3(0.0f, 0.0f, -1.0f), out DirectChunk))
                DirectChunk.UpdateChunkMesh_WithSurround();
            if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + new Vector3(-1.0f, 0.0f, 0.0f), out DirectChunk))
                DirectChunk.UpdateChunkMesh_WithSurround();
            if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + new Vector3(1.0f, 0.0f, 0.0f), out DirectChunk))
                DirectChunk.UpdateChunkMesh_WithSurround();
            iscaller = false;
        }




        //添加到world的渲染队列
        isReadyToRender = true;

        if (isCalled)
        {
            isCalled = false;
        }
        else
        {
            //print($"{world.GetChunkLocation(myposition)}Mesh完成");
            world.MeshLock = false;
        }

        if (world.RenderLock)
        {
            world.WaitToRender_temp.Enqueue(this);
            //print($"{world.GetChunkLocation(myposition)}被堵塞，入队temp");
        }
        else
        {
            //print($"{world.GetChunkLocation(myposition)}入队");
            world.WaitToRender.Enqueue(this);
        }


        if (BaseChunk == true)
        {
            BaseChunk = false;
        }
        


        


    }

    //清除网格
    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    //编辑方块
    public void EditData(Vector3 pos, byte targetBlocktype)
    {

        ClearFInd_Direvtion();

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (y >= 62)
        {
            return;
        }

        voxelMap[x, y, z] = targetBlocktype;

        UpdateChunkMesh_WithSurround(true);
    }

    //面生成的判断
    //是方块吗----Y不绘制----N绘制
    //靠近边界----也返回N
    bool CheckVoxel(Vector3 pos, int _p)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        //print($"{x},{y},{z}");


        //如果目标出界
        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
        {

            //if (ThisChunkLocation == new Vector3(100f,0f,99f))
            //{
            //	print("");
            //}

            if (!BaseChunk)
            {
                //Front
                if (z > VoxelData.ChunkWidth - 1)
                {
                    //如果能查到
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                    {


                        //如果target是空气，则返回false
                        if (world.blocktypes[chunktemp.voxelMap[x, y, 0]].isTransparent && !world.blocktypes[voxelMap[x, y, z - 1]].isTransparent)
                        {

                            return false;
                        }
                        else
                        {
                            return true;
                        }



                    }
                    else
                    {
                        if (debug_CanLookCave)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }


                //Back
                if (z < 0)
                {
                    //如果能查到
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                    {


                        //如果target是空气，则返回false
                        if (world.blocktypes[chunktemp.voxelMap[x, y, VoxelData.ChunkWidth - 1]].isTransparent && !world.blocktypes[voxelMap[x, y, z + 1]].isTransparent)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }



                    }
                    else
                    {
                        if (debug_CanLookCave)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }


                }


                //Left
                if (x < 0)
                {
                    //如果能查到
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                    {


                        //如果target是空气，则返回false
                        if (world.blocktypes[chunktemp.voxelMap[VoxelData.ChunkWidth - 1, y, z]].isTransparent && !world.blocktypes[voxelMap[x + 1, y, z]].isTransparent)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }



                    }
                    else
                    {
                        if (debug_CanLookCave)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }


                //Right
                if (x > VoxelData.ChunkWidth - 1)
                {
                    //如果能查到
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                    {
                        //如果target是空气，则返回false
                        if (world.blocktypes[chunktemp.voxelMap[0, y, z]].isTransparent && !world.blocktypes[voxelMap[x - 1, y, z]].isTransparent)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }



                    }
                    else
                    {
                        if (debug_CanLookCave)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            //Up不需要考虑

            //Down:最下层一律不绘制
            if (y < 0)
            {
                return true;
            }

            //else:自己是不是空气
            if (voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air || voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Water)
            {
                return true;
            }
            else
            {
                return false;
            }




        }
        else
        {
            //Debug.Log("未出界");

            //自己是空气 && 目标是竹子 则绘制
            //if (voxelMap[x, y, z] == VoxelData.Bamboo && voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air)
            //{
            //    return true;
            //}


            

            //判断自己和目标是不是都是空气 || 自己和目标是不是都是水
            //不生成面的情况
            if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air) && voxelMap[x, y, z] == VoxelData.Air) || ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Water) && voxelMap[x, y, z] == VoxelData.Water))
            {
                return true;
            }
            else // 生成面的情况
            {
                //(如果自己是水，目标是空气) || (自己是空气，目标是水)
                if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Water) && voxelMap[x, y, z] == VoxelData.Air))
                {
                    return false;
                }
                else if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air) && voxelMap[x, y, z] == VoxelData.Water))
                {
                    return false;
                }
            }

            //判断是不是透明方块
            if (world.blocktypes[voxelMap[x, y, z]].isTransparent)
            {
                return false;
            }
        }


        return world.blocktypes[voxelMap[x, y, z]].isSolid;

    }

    //遍历中：：顺带判断面的生成方向
    //创建mesh里的参数
    void UpdateMeshData(Vector3 pos)
    {
        byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

        //方块绘制模式
        switch (world.blocktypes[voxelMap[x, y, z]].DrawMode)
        {
            case DrawMode.Bush:// Bush绘制模式

                for (int faceIndex = 0; faceIndex < 4; faceIndex++)
                {
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris_Bush[faceIndex, 0]]);
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris_Bush[faceIndex, 1]]);
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris_Bush[faceIndex, 2]]);
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris_Bush[faceIndex, 3]]);



                    AddTexture_Bush(world.blocktypes[blockID].GetTextureID(0));

                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1); 
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2); 
                    triangles.Add(vertexIndex + 3); 
                    triangles.Add(vertexIndex);
                    vertexIndex += 4;


                }

                break;

            default: //默认Block绘制模式

                

                //判断六个面
                for (int p = 0; p < 6; p++)
                {


                    if (!CheckVoxel(pos + VoxelData.faceChecks[p], p))
                    {

                        vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                        vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                        vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                        vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                        //uvs.Add (VoxelData.voxelUvs [0]);
                        //uvs.Add (VoxelData.voxelUvs [1]);
                        //uvs.Add (VoxelData.voxelUvs [2]);
                        //uvs.Add (VoxelData.voxelUvs [3]);
                        //AddTexture(1);

                        //根据p生成对应的面，对应的UV
                        AddTexture(world.blocktypes[blockID].GetTextureID(p));

                        triangles.Add(vertexIndex);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + 2);
                        triangles.Add(vertexIndex + 2);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + 3);
                        vertexIndex += 4;

                    }
                }

                break;
        }

        

    }

    // 最后生成网格体
    public void CreateMesh()
    {
        world.RenderLock = true;
        //print($"{world.GetChunkLocation(myposition)}CreateMesh 开始");

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;


        //print($"{world.GetChunkLocation(myposition)}CreateMesh 结束");

        world.RenderLock = false;

        while (world.WaitToRender_temp.Count > 0)
        {
            world.WaitToRender_temp.TryDequeue(out Chunk chunktemp);
            world.WaitToRender.Enqueue(chunktemp);
        }


    }

    //生成面——UV的处理
    void AddTexture(int textureID)
    {

       


        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));

        //if (textureID == 35)
        //{
        //    if (hasExec_2)

        //    {
        //        print($"别人的方法：x = {x},y = {y}");
        //        hasExec_2 = false;
        //    }
        //}

    }

    //生成面——Bush
    void AddTexture_Bush(int textureID)
    {

        float x = (float)(textureID % 8) / 8;
        float y = (float)(7 - textureID / 8) / 8;

        //if (textureID == 35)
        //{
        //    print($"Bush: x = {x},y = {y}");
        //}

        float temp = 1f / 8;
        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + temp));
        uvs.Add(new Vector2(x + temp, y + temp));
        uvs.Add(new Vector2(x + temp, y));
    }



    //------------------------------------------------------------------------------------






    //------------------------------ CallSurround功能 ------------------------------------
    public bool isFind_Front = false;
    public bool isFind_Back = false;
    public bool isFind_Left = false;
    public bool isFind_Right = false;

    public void ClearFInd_Direvtion()
    {
        isFind_Front = false;
        isFind_Back = false;
        isFind_Left = false;
        isFind_Right = false;
}

    //------------------------------------------------------------------------------------






    //---------------------------------- 辅助部分 ----------------------------------------

    //销毁自己
    public void DestroyChunk()
    {
        Destroy(chunkObject);
    }

    //隐藏自己
    public void HideChunk()
    {
        chunkObject.SetActive(false);
        isShow = false;
    }

    //显示自己
    public void ShowChunk()
    {
        chunkObject.SetActive(true);
        isShow = true;
    }

    //3d噪声
    public static float Perlin3D(float x, float y, float z)
    {

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);

        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        float ABC = AB + BC + AC + BA + CB + CA;
        return ABC / 6f;

    }

    //是否出界
    bool isOutOfRange(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //洞穴生成概率
    float Probability(float y)
    {
        float possible = 0;
        float mid = world.soil_max / 2;

        // 如果y越接近0，则possible越接近0，反之越接近1
        float ratio = y / mid;
        possible = 1 - ratio;

        return Mathf.Clamp01(possible); // 返回归一化到 [0, 1] 区间的概率值
    }


    //----------------------------------------------------------------------------------

}
