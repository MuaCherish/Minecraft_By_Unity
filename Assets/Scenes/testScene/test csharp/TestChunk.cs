//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine.XR;

public class TestChunk : MonoBehaviour
{

    //state
    public bool isShow = true;
    public bool isReadyToRender = false;
    public bool BaseChunk;
    public bool isCalled = false;  //不要删我！！

    //Transform
    TestWorld world;
    public GameObject chunkObject;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    //noise
    private float noise3d_scale;


    //群系参数
    int Normal_treecount;
    int Forest_treecount;

    //BlockMap
    private int x;
    private int y;
    private int z; 
    public VoxelStruct[,,] voxelMap = new VoxelStruct[TerrainData.ChunkWidth, TerrainData.ChunkHeight, TerrainData.ChunkWidth];


    //Mesh
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<int> triangles_Water = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

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
    //bool debug_CanLookCave;




    //---------------------------------- 周期函数 ---------------------------------------




    //Start()
    public TestChunk(Vector3 thisPosition, TestWorld _world, bool _BaseChunk)
    { 


        //World
        world = _world;
        caveWidth = world.cave_width;
        //debug_CanLookCave = !world.debug_CanLookCave;
        BaseChunk = _BaseChunk;
        noise3d_scale = world.noise3d_scale;
        Normal_treecount = world.terrainLayerProbabilitySystem.Normal_treecount;
        Forest_treecount = world.terrainLayerProbabilitySystem.密林树木采样次数Forest_treecount;
        rand = new System.Random(world.terrainLayerProbabilitySystem.Seed);

        //Self
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = world.material;
        chunkObject.transform.SetParent(world.Chunks.transform);
        chunkObject.transform.position = new Vector3(thisPosition.x * TerrainData.ChunkWidth, 0f, thisPosition.z * TerrainData.ChunkWidth);
        chunkObject.name = thisPosition.x + "," + thisPosition.z;
        myposition = chunkObject.transform.position;


        for (int x = 0; x < TerrainData.ChunkWidth; x++)
        {
            for (int y = 0; y < TerrainData.ChunkHeight; y++)
            {
                for (int z = 0; z < TerrainData.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = new VoxelStruct(); // 初始化每个元素
                }
            }
        }

        CreateData();

        //print($"----------------------------------------------");
        //print($"{world.GetChunkLocation(myposition)}已经生成！");
    }



    //-----------------------------------------------------------------------------------






    //----------------------------------- Noise ----------------------------------------

    //洞穴噪声生成器
    float GetCaveNoise(int _x, int _y, int _z)
    {
        return Perlin3D(((float)_x + myposition.x) * noise3d_scale, ((float)_y + y) * noise3d_scale, ((float)_z + myposition.z) * noise3d_scale); // 将100改为0.1

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
        for (int y = 0; y < TerrainData.ChunkHeight; y++)
        {
            for (int x = 0; x < TerrainData.ChunkWidth; x++)
            {
                for (int z = 0; z < TerrainData.ChunkWidth; z++)
                {


                    // 生成0或1的随机数
                    //int randomInt = rand.Next(0, 2);
                    //int randomFrom_0_10 = rand.Next(0, 10);


                    //地形噪声
                    //float noiseHigh = GetTotalNoiseHigh(x, z);
                    float noiseHigh = world.GetTotalNoiseHigh_Biome(x, z, myposition);


                    //矿洞噪声
                    float noise3d = GetCaveNoise(x, y, z);


                    //沙漠噪声
                    //float noise_desery = GetSmoothNoise_Desert(x, z);


                    //判断基岩
                    //0~3层不准生成矿洞
                    if (y >= 0 && y <= 3)
                    {

                        if (y == 0)
                        {

                            voxelMap[x, y, z].voxelType = VoxelData.BedRock;

                        }
                        else if (y > 0 && y < 3 && GetProbability(50))
                        {

                            voxelMap[x, y, z].voxelType = VoxelData.BedRock;

                        }
                        else
                        {

                            voxelMap[x, y, z].voxelType = VoxelData.Stone;

                        }
                    }
                    //空气部分
                    else if (y > noiseHigh && y > world.terrainLayerProbabilitySystem.sea_level)
                    {

                        //地上一层
                        if (y - 1 < noiseHigh)
                        {

                            //如果可生成
                            if (voxelMap[x, y - 1, z].voxelType != VoxelData.Sand && voxelMap[x, y - 1, z].voxelType != VoxelData.Air && voxelMap[x, y - 1, z].voxelType != VoxelData.Snow)
                            {

                                //灌木丛
                                if (GetProbability(world.terrainLayerProbabilitySystem.Random_Bush))
                                {

                                    voxelMap[x, y, z].voxelType = VoxelData.Bush;

                                }
                                //BlueFlower
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_BlueFlower))
                                {

                                    voxelMap[x, y, z].voxelType = VoxelData.BlueFlower;

                                }
                                //WhiteFlower_1
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_WhiteFlower1))
                                {

                                    voxelMap[x, y, z].voxelType = VoxelData.WhiteFlower_1;

                                }
                                //WhiteFlower_2
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_WhiteFlower2))
                                {

                                    voxelMap[x, y, z].voxelType = VoxelData.WhiteFlower_2;

                                }
                                //YellowFlower
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_YellowFlower))
                                {

                                    voxelMap[x, y, z].voxelType = VoxelData.YellowFlower;

                                }
                                else
                                {

                                    voxelMap[x, y, z].voxelType = VoxelData.Air;

                                }
                            }

                            //竹子
                            else if (voxelMap[x, y - 1, z].voxelType == VoxelData.Sand && GetProbability(world.terrainLayerProbabilitySystem.Random_Bamboo))
                            {

                                voxelMap[x, y, z].voxelType = VoxelData.Air;
                                Bamboos.Enqueue(new Vector3(x, y, z));

                            }
                            else
                            {

                                voxelMap[x, y, z].voxelType = VoxelData.Air;

                            }
                        }

                        else
                        {

                            voxelMap[x, y, z].voxelType = VoxelData.Air;

                        }
                    }

                    //判断水面
                    else if (y > noiseHigh && y - 1 < world.terrainLayerProbabilitySystem.sea_level)
                    {

                        voxelMap[x, y, z].voxelType = VoxelData.Water;

                    }

                    //空气之下
                    else
                    {

                        //地表
                        if ((y + 1) > noiseHigh)
                        {

                            //沙漠气候
                            if (world.GetBiomeType(x, z, myposition) == TerrainData.Biome_Dessert)
                            {

                                voxelMap[x, y, z].voxelType = VoxelData.Sand;

                            }

                            //草原气候
                            else
                            {
                                //100雪地
                                if (y > world.terrainLayerProbabilitySystem.Snow_Level)
                                {
                                    voxelMap[x, y, z].voxelType = VoxelData.Snow;
                                }

                                //90~100概率生成雪地
                                else if ((y > (world.terrainLayerProbabilitySystem.Snow_Level - 10f)) && GetProbability(70))
                                {
                                    voxelMap[x, y, z].voxelType = VoxelData.Snow;
                                }



                                //高于海平面
                                else if (y > world.terrainLayerProbabilitySystem.sea_level)
                                {

                                    //是否是菌丝体
                                    if (world.GetBiomeType(x, z, myposition) == TerrainData.Biome_Marsh)
                                    {
                                        voxelMap[x, y, z].voxelType = VoxelData.Mycelium;
                                    }
                                    else
                                    {
                                        voxelMap[x, y, z].voxelType = VoxelData.Grass;
                                    }

                                }
                                else
                                {

                                    if (world.GetSimpleNoiseWithOffset(x, z, myposition, new Vector2(111f, 222f), 0.1f) > 0.5f)
                                    {

                                        voxelMap[x, y, z].voxelType = VoxelData.Sand;

                                    }
                                    else
                                    {

                                        voxelMap[x, y, z].voxelType = VoxelData.Soil;

                                    }

                                }
                            }
                        }


                        //泥土的判断
                        else if (y > noiseHigh - 7)
                        {
                            //沙漠判断
                            if (world.GetBiomeType(x, z, myposition) == TerrainData.Biome_Dessert)
                            {
                                voxelMap[x, y, z].voxelType = VoxelData.Sand;
                            }
                            else
                            {
                                voxelMap[x, y, z].voxelType = VoxelData.Soil;
                            }


                        }
                        else if (y >= (noiseHigh - 10) && y <= (noiseHigh - 7) && GetProbability(50))
                        {
                            //沙漠判断
                            if (world.GetBiomeType(x, z, myposition) == TerrainData.Biome_Dessert)
                            {
                                voxelMap[x, y, z].voxelType = VoxelData.Sand;
                            }
                            else
                            {
                                voxelMap[x, y, z].voxelType = VoxelData.Soil;
                            }


                        }





                        //矿洞
                        else if (noise3d < GetVaveWidth(y))
                        {

                            voxelMap[x, y, z].voxelType = VoxelData.Air;

                        }

                        //地下判断
                        else
                        {

                            //煤炭
                            if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Coal))
                            {

                                voxelMap[x, y, z].voxelType = VoxelData.Stone;
                                Coals.Enqueue(new Vector3(x, y, z));

                            }

                            //铁
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Iron))
                            {

                                voxelMap[x, y, z].voxelType = VoxelData.Iron;

                            }

                            //金
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Gold))
                            {

                                voxelMap[x, y, z].voxelType = VoxelData.Gold;

                            }

                            //青金石
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Blue_Crystal))
                            {

                                voxelMap[x, y, z].voxelType = VoxelData.Blue_Crystal;

                            }

                            //钻石
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Diamond))
                            {

                                voxelMap[x, y, z].voxelType = VoxelData.Diamond;

                            }

                            else
                            {

                                voxelMap[x, y, z].voxelType = VoxelData.Stone;

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
        //world.WaitToCreateMesh.Enqueue(this);

        UpdateChunkMesh_WithSurround();

    }




    //---------------------------------- Tree ----------------------------------------

    //tree
    void CreateTree()
    {
        //密林群系
        if (world.GetBiomeType(x, z, myposition) == TerrainData.Biome_Forest)
        {
            //[确定XZ]xoz上随便选择5个点
            while (Forest_treecount-- != 0)
            {

                int random_x = rand.Next(2, TerrainData.ChunkWidth - 2);
                int random_z = rand.Next(2, TerrainData.ChunkWidth - 2);
                int random_y = TerrainData.ChunkHeight;
                int random_Tree_High = rand.Next(world.terrainLayerProbabilitySystem.TreeHigh_min, world.terrainLayerProbabilitySystem.TreeHigh_max + 1);

                //如果可以生成树桩
                //向上延伸树干
                random_y = CanSetStump(random_x, random_y, random_z, random_Tree_High);
                if (random_y != -1f)
                {

                    for (int i = 0; i <= random_Tree_High; i++)
                    {

                        if (random_y + i >= TerrainData.ChunkHeight - 1)
                        {

                            Debug.Log($"random_y:{random_y},i={i}");

                        }

                        else
                        {

                            voxelMap[random_x, random_y + i, random_z].voxelType = VoxelData.Wood;

                        }

                    }

                    //生成树叶
                    BuildLeaves(random_x, random_y + random_Tree_High, random_z);

                }


                //Debug.Log($"{random_x}, {random_y}, {random_z}");
            }
        }
        else
        {
            //[确定XZ]xoz上随便选择5个点
            int count = rand.Next(0, Normal_treecount);

            while (count-- != 0)
            {

                int random_x = rand.Next(2, TerrainData.ChunkWidth - 2);
                int random_z = rand.Next(2, TerrainData.ChunkWidth - 2);
                int random_y = TerrainData.ChunkHeight;
                int random_Tree_High = rand.Next(world.terrainLayerProbabilitySystem.TreeHigh_min, world.terrainLayerProbabilitySystem.TreeHigh_max + 1);

                //如果可以生成树桩
                //向上延伸树干
                random_y = CanSetStump(random_x, random_y, random_z, random_Tree_High);
                if (random_y != -1f)
                {

                    for (int i = 0; i <= random_Tree_High; i++)
                    {

                        if (random_y + i >= TerrainData.ChunkHeight - 1)
                        {

                            Debug.Log($"random_y:{random_y},i={i}");

                        }

                        else
                        {

                            voxelMap[random_x, random_y + i, random_z].voxelType = VoxelData.Wood;

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
        // 确保输入值在 0 到 100 之间
        input = Mathf.Clamp(input, 0, 100);

        // 生成一个 0 到 100 之间的随机数
        float randomValue = rand.Next(0, 100);

        // 如果随机数小于等于输入值，则返回 true
        //Debug.Log(randomValue);
        bool a = randomValue < input;


        return a;
    }

    // 返回一个概率值，范围在0~100，根据输入值越接近100，概率接近100%，越接近0，概率接近0%
    bool GetProbabilityTenThousandth(float input)
    {
        // 确保输入值在 0 到 100 之间
        input = Mathf.Clamp(input, 0, 100);

        // 生成一个 0 到 100 之间的随机数
        float randomValue = rand.Next(0, 10000);

        // 如果随机数小于等于输入值，则返回 true
        //Debug.Log(randomValue);
        bool a = randomValue < input;


        return a;
    }

    //构造树叶
    void BuildLeaves(int _x, int _y, int _z)
    {

        int randomInt = rand.Next(0, 2);


        //第一层
        if (randomInt == 0)
        {

            CreateLeaves(_x, _y + 1, _z);

            //生成雪的判定
            if (((_y + 1) >= world.terrainLayerProbabilitySystem.Snow_Level - 10f) && ((_y + 2) < TerrainData.ChunkHeight))
            {
                voxelMap[_x, _y + 2, _z].voxelType = VoxelData.Snow;
            }



        }
        else if (randomInt == 1)
        {

            CreateLeaves(_x, _y + 1, _z + 1);
            CreateLeaves(_x - 1, _y + 1, _z);
            CreateLeaves(_x, _y + 1, _z);
            CreateLeaves(_x + 1, _y + 1, _z);
            CreateLeaves(_x, _y + 1, _z - 1);

            //生成雪的判定
            if (((_y + 1) >= world.terrainLayerProbabilitySystem.Snow_Level - 10f) && ((_y + 2) < TerrainData.ChunkHeight))
            {
                voxelMap[_x, _y + 2, _z + 1].voxelType = VoxelData.Snow;
                voxelMap[_x - 1, _y + 2, _z].voxelType = VoxelData.Snow;
                voxelMap[_x, _y + 2, _z].voxelType = VoxelData.Snow;
                voxelMap[_x + 1, _y + 2, _z].voxelType = VoxelData.Snow;
                voxelMap[_x, _y + 2, _z - 1].voxelType = VoxelData.Snow;
            }

        }

        //第二层
        CreateLeaves(_x - 1, _y, _z);
        CreateLeaves(_x + 1, _y, _z);
        CreateLeaves(_x, _y, _z - 1);
        CreateLeaves(_x, _y, _z + 1);

        //生成雪的判定
        if (((_y) >= world.terrainLayerProbabilitySystem.Snow_Level - 10f) && ((_y + 1) < TerrainData.ChunkHeight) && voxelMap[_x - 1, _y + 1, _z].voxelType != VoxelData.Leaves)
        {

            voxelMap[_x - 1, _y + 1, _z].voxelType = VoxelData.Snow;
            voxelMap[_x + 1, _y + 1, _z].voxelType = VoxelData.Snow;
            voxelMap[_x, _y + 1, _z - 1].voxelType = VoxelData.Snow;
            voxelMap[_x, _y + 1, _z + 1].voxelType = VoxelData.Snow;

        }

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

        //Snow
        if ((_y - 1) >= world.terrainLayerProbabilitySystem.Snow_Level - 10f)
        {
            voxelMap[_x, _y, _z + 2].voxelType = VoxelData.Snow;
            voxelMap[_x - 1, _y, _z + 2].voxelType = VoxelData.Snow;
            voxelMap[_x + 1, _y, _z + 2].voxelType = VoxelData.Snow;
            voxelMap[_x - 2, _y, _z + 1].voxelType = VoxelData.Snow;
            voxelMap[_x - 1, _y, _z + 1].voxelType = VoxelData.Snow;
            voxelMap[_x + 1, _y, _z + 1].voxelType = VoxelData.Snow;
            voxelMap[_x + 2, _y, _z + 1].voxelType = VoxelData.Snow;
            voxelMap[_x - 2, _y, _z].voxelType = VoxelData.Snow;
            voxelMap[_x + 2, _y, _z].voxelType = VoxelData.Snow;
            voxelMap[_x - 2, _y, _z - 1].voxelType = VoxelData.Snow;
            voxelMap[_x - 1, _y, _z - 1].voxelType = VoxelData.Snow;
            voxelMap[_x + 1, _y, _z - 1].voxelType = VoxelData.Snow;
            voxelMap[_x + 2, _y, _z - 1].voxelType = VoxelData.Snow;
            voxelMap[_x - 1, _y, _z - 2].voxelType = VoxelData.Snow;
            voxelMap[_x, _y, _z - 2].voxelType = VoxelData.Snow;
            voxelMap[_x + 1, _y, _z - 2].voxelType = VoxelData.Snow;

            //十字架不生成雪避免挤掉第二层
            //voxelMap[_x, _y, _z + 1] = VoxelData.Snow;
            //voxelMap[_x - 1, _y, _z] = VoxelData.Snow;
            //voxelMap[_x + 1, _y, _z] = VoxelData.Snow;
            //voxelMap[_x, _y, _z - 1] = VoxelData.Snow;
        }

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
    int CanSetStump(int _x, int _y, int _z, int treehigh)
    {

        if (GetProbability(_y))
        {

            while (_y > 0)
            {

                //如果不是泥土或者草地则不生成
                if (voxelMap[_x, _y - 1, _z].voxelType != VoxelData.Air)
                {

                    if (voxelMap[_x, _y - 1, _z].voxelType == VoxelData.Grass || voxelMap[_x, _y - 1, _z].voxelType == VoxelData.Soil && voxelMap[_x, _y - 2, _z].voxelType != VoxelData.Leaves)
                    {

                        //判断树干是否太高
                        if (_y + treehigh + 3 > TerrainData.ChunkHeight - 1)
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

            }
        }



        return -1;

    }

    //树叶生成判断
    //leaves设定值，防止碰到树木
    void CreateLeaves(int x, int y, int z)
    {

        //如果是固体，就不用生成树叶了
        if (voxelMap[x, y, z].voxelType != VoxelData.Air)
        {

            return;

        }
        else
        {

            voxelMap[x, y, z].voxelType = VoxelData.Leaves;

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

            if (voxelMap[_x, _y, _z].voxelType == VoxelData.Stone)
                voxelMap[_x, _y, _z].voxelType = VoxelData.Coal;

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

                voxelMap[x, y + temp, z].voxelType = VoxelData.Bamboo;

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

                if (voxelMap[_x, y - 1, _z].voxelType == VoxelData.Water)
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
        if (voxelMap[x, y, z].voxelType == VoxelData.Bamboo && voxelMap[x, y - 1, z].voxelType == VoxelData.Air)
        {

            voxelMap[x, y, z].voxelType = VoxelData.Air;

        }
    }

    //灌木丛消失
    void updateBush()
    {

        //如果自己是Bush && 自己下面是空气 则自己变为空气
        if (voxelMap[x, y, z].voxelType == VoxelData.Bush && voxelMap[x, y - 1, z].voxelType == VoxelData.Air)
        {

            voxelMap[x, y, z].voxelType = VoxelData.Air;

        }

    }

    //------------------------------------------------------------------------------------






    //---------------------------------- Mesh部分 ----------------------------------------


    public void UpdateChunkMesh_WithSurround()
    {

        ClearMeshData();

        //刷新自己
        for (y = 0; y < TerrainData.ChunkHeight; y++)
        {

            for (x = 0; x < TerrainData.ChunkWidth; x++)
            {

                for (z = 0; z < TerrainData.ChunkWidth; z++)
                {

                    //竹子断裂
                    updateBamboo();

                    //Bush消失
                    updateBush();

                    //[已废弃，移动至单独的线程执行]水的流动
                    //updateWater();

                    // 非空气 - 渲染
                    // 水面上 - 渲染
                    if (world.blocktypes[voxelMap[x, y, z].voxelType].DrawMode != DrawMode.Air)
                        UpdateMeshData(new Vector3(x, y, z));

                    //UpdateMeshData(new Vector3(x, y, z));
                }

            }

        }


        CreateMesh();

    }

    //清除网格
    void ClearMeshData()
    {

        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        triangles_Water.Clear();
        uvs.Clear();

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
        if (x < 0 || x > TerrainData.ChunkWidth - 1 || y < 0 || y > TerrainData.ChunkHeight - 1 || z < 0 || z > TerrainData.ChunkWidth - 1)
        {

            //if (ThisChunkLocation == new Vector3(100f,0f,99f))
            //{
            //	print("");
            //}

            

            //Down:最下层一律不绘制
            //if (y < 0)
            //{

            //    return true;

            //}

            ////else:自己是不是空气
            //if (voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z].voxelType == VoxelData.Air || voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z].voxelType == VoxelData.Water)
            //{

            //    return true;

            //}
            //else
            //{

            //    return false;

            //}


            return true;

        }


        //未出界的情况
        else
        {
            //Debug.Log("未出界");

            //自己是空气 && 目标是竹子 则绘制
            //if (voxelMap[x, y, z] == VoxelData.Bamboo && voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air)
            //{
            //    return true;
            //}




            //自己与目标都是空气
            //或者自己与目标都是水
            //不生成面
            if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z].voxelType == VoxelData.Air) && voxelMap[x, y, z].voxelType == VoxelData.Air) || ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z].voxelType == VoxelData.Water) && voxelMap[x, y, z].voxelType == VoxelData.Water))
            {

                return true;

            }
            else // 生成面的情况
            {
                //如果自己是水，目标是空气
                //生成面
                if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z].voxelType == VoxelData.Water) && voxelMap[x, y, z].voxelType == VoxelData.Air))
                {

                    return false;

                }

            }

            //判断是不是透明方块
            if (world.blocktypes[voxelMap[x, y, z].voxelType].isTransparent)
            {

                return false;

            }

        }


        return world.blocktypes[voxelMap[x, y, z].voxelType].isSolid;

    }


    //遍历中：：顺带判断面的生成方向
    void UpdateMeshData(Vector3 pos)
    {

        byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z].voxelType;

        //方块绘制模式
        switch (world.blocktypes[voxelMap[x, y, z].voxelType].DrawMode)
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



            case DrawMode.Water:  //水面绘制模式



                //判断六个面
                for (int p = 0; p < 6; p++)
                {

                    //可以绘制
                    if (!CheckVoxel(pos + VoxelData.faceChecks[p], p))
                    {

                        //如果上下方有水，则换成方块的渲染方式
                        if ((voxelMap[(int)pos.x, (int)pos.y + 1, (int)pos.z].voxelType == VoxelData.Water || voxelMap[(int)pos.x, (int)pos.y - 1, (int)pos.z].voxelType == VoxelData.Water) && p != 2 && p != 3 && voxelMap[(int)pos.x, (int)pos.y + 1, (int)pos.z].voxelType != VoxelData.Air)
                        {

                            vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                            vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                            vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                            vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                            uvs.Add(new Vector2(0f, 0f));
                            uvs.Add(new Vector2(0f, 1f));
                            uvs.Add(new Vector2(1f, 0f));
                            uvs.Add(new Vector2(1f, 1f));
                            //根据p生成对应的面，对应的UV
                            //AddTexture(world.blocktypes[blockID].GetTextureID(p));

                            triangles_Water.Add(vertexIndex);
                            triangles_Water.Add(vertexIndex + 1);
                            triangles_Water.Add(vertexIndex + 2);
                            triangles_Water.Add(vertexIndex + 2);
                            triangles_Water.Add(vertexIndex + 1);
                            triangles_Water.Add(vertexIndex + 3);
                            vertexIndex += 4;

                        }
                        else
                        {



                            //如果朝上 && 是水面 && 上方是空气 => 尝试搜索周边水面进行面的融合
                            if (p == 2 && voxelMap[x, y, z].voxelType == VoxelData.Water && voxelMap[x, y + 1, z].voxelType == VoxelData.Air && voxelMap[x, y, z].up == true)
                            {
                                int _zz = 0;
                                int _xx = 0;

                                //局部变量
                                int __z = 0;
                                bool Z排碰到障碍物 = false;

                                //运行后获得当前方块可以踏步的范围
                                for (_xx = 0; _xx < TerrainData.ChunkWidth; _xx++)
                                {
                                    for (__z = 0; __z < TerrainData.ChunkWidth; __z++)
                                    {

                                        //如果出界则继续
                                        if (isOutOfRange(x + _xx, y, z + __z))
                                        {
                                            continue;
                                        }

                                        //if(_xx == 8){
                                        //    print("");
                                        //} 


                                        //目标是水 && 目标上方是空气 && 目标的up是true
                                        if ((voxelMap[x + _xx, y, z + __z].voxelType != VoxelData.Water || voxelMap[x + _xx, y + 1, z + __z].voxelType != VoxelData.Air) || (voxelMap[x + _xx, y, z + __z].up == false))
                                        {
                                            Z排碰到障碍物 = true;
                                            break;
                                        }

                                    }


                                    if (__z > _zz)
                                    {
                                        _zz = __z;
                                    }


                                    if (Z排碰到障碍物)
                                    {

                                        if (_xx == 0)
                                        {
                                            for (int i = 0; i < __z; i++)
                                            {
                                                voxelMap[x + _xx, y, z + i].up = false;
                                            }

                                        }


                                        break;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < __z; i++)
                                        {
                                            voxelMap[x + _xx, y, z + i].up = false;
                                        }

                                    }



                                }

                                //print($"local：{new Vector3(x, y, z)},长和宽：<{_xx},{_zz}>");


                                //最后应该给定一个长和宽 _xx与_zz



                                //防止顶点乘上0
                                if (_xx == 0)
                                {
                                    vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 0]]);
                                    vertices.Add(pos + ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 1]], new Vector3(1, 1, _zz)));
                                    vertices.Add(pos + ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 2]], new Vector3(1, 1, 1)));
                                    vertices.Add(pos + ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 3]], new Vector3(1, 1, _zz)));


                                    //根据p生成对应的面，对应的UV
                                    //AddTexture(world.blocktypes[blockID].GetTextureID(p));
                                    uvs.Add(new Vector2(0f, 0f));
                                    uvs.Add(ComponentwiseMultiply(new Vector2(0f, 1f), new Vector2(1, _zz)));
                                    uvs.Add(new Vector2(1f, 0f));
                                    uvs.Add(ComponentwiseMultiply(new Vector2(1f, 1f), new Vector2(1, _zz)));

                                }
                                else
                                {
                                    vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 0]]);
                                    vertices.Add(pos + ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 1]], new Vector3(1, 1, _zz)));
                                    vertices.Add(pos + ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 2]], new Vector3(_xx, 1, 1)));
                                    vertices.Add(pos + ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 3]], new Vector3(_xx, 1, _zz)));


                                    //根据p生成对应的面，对应的UV
                                    //AddTexture(world.blocktypes[blockID].GetTextureID(p));
                                    uvs.Add(new Vector2(0f, 0f));
                                    uvs.Add(ComponentwiseMultiply(new Vector2(0f, 1f), new Vector2(1, _zz)));
                                    uvs.Add(ComponentwiseMultiply(new Vector2(1f, 0f), new Vector2(_xx, 1)));
                                    uvs.Add(ComponentwiseMultiply(new Vector2(1f, 1f), new Vector2(_xx, _zz)));

                                }





                                triangles_Water.Add(vertexIndex);
                                triangles_Water.Add(vertexIndex + 1);
                                triangles_Water.Add(vertexIndex + 2);
                                triangles_Water.Add(vertexIndex + 2);
                                triangles_Water.Add(vertexIndex + 1);
                                triangles_Water.Add(vertexIndex + 3);
                                vertexIndex += 4;

                            }




                            //vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 0]]);
                            //vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 1]]);
                            //vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 2]]);
                            //vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 3]]);
                            //uvs.Add(new Vector2(0f, 0f));
                            //uvs.Add(new Vector2(0f, 1f));
                            //uvs.Add(new Vector2(1f, 0f));
                            //uvs.Add(new Vector2(1f, 1f));
                            //triangles_Water.Add(vertexIndex);
                            //triangles_Water.Add(vertexIndex + 1);
                            //triangles_Water.Add(vertexIndex + 2);
                            //triangles_Water.Add(vertexIndex + 2);
                            //triangles_Water.Add(vertexIndex + 1);
                            //triangles_Water.Add(vertexIndex + 3);
                            //vertexIndex += 4;


                        }




                    }

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

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        //mesh.triangles = triangles.ToArray();

        //使用双材质
        meshRenderer.materials = new Material[] { world.material, world.material_Water };
        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0); // 第一个子网格使用triangles数组
        mesh.SetTriangles(triangles_Water.ToArray(), 1); // 第二个子网格使用triangles_2数组

        //优化
        mesh.Optimize();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

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







    //---------------------------------- tools ----------------------------------------


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
    bool isOutOfRange(int _x, int _y, int _z)
    {

        if (_x < 0 || _x > TerrainData.ChunkWidth - 1 || _y < 0 || _y > TerrainData.ChunkHeight - 1 || _z < 0 || _z > TerrainData.ChunkWidth - 1)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    //出界重载
    bool isOutOfRange(Vector3 pos)
    {

        int _x = (int)pos.x;
        int _y = (int)pos.y;
        int _z = (int)pos.z;

        if (_x < 0 || _x > TerrainData.ChunkWidth - 1 || _y < 0 || _y > TerrainData.ChunkHeight - 1 || _z < 0 || _z > TerrainData.ChunkWidth - 1)
        {

            return true;

        }
        else
        {

            return false;

        }

    }

    // 分量乘法
    Vector3 ComponentwiseMultiply(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z); 
    }

    // 处理 Vector2 的分量乘法
    Vector2 ComponentwiseMultiply(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x * b.x, a.y * b.y);
    }

    //----------------------------------------------------------------------------------

}


