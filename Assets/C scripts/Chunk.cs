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
    public bool iHaveWater = false; private bool haeExec_iHaveWater = true;
    public bool hasExec_isHadupdateWater = true;  //该标志允许一个Chunk每次只能更新一次水体


    //Transform
    World world;
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
    public byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];


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
    public Chunk(Vector3 thisPosition, World _world, bool _BaseChunk)
    {


        //World
        world = _world;
        caveWidth = world.cave_width;
        //debug_CanLookCave = !world.debug_CanLookCave;
        BaseChunk = _BaseChunk;
        noise3d_scale = world.noise3d_scale;
        Normal_treecount = world.terrainLayerProbabilitySystem.Normal_treecount;
        Forest_treecount = world.terrainLayerProbabilitySystem.密林树木采样次数Forest_treecount;

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
        rand = new System.Random(world.terrainLayerProbabilitySystem.Seed);
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

                            voxelMap[x, y, z] = VoxelData.BedRock;

                        }
                        else if (y > 0 && y < 3 && GetProbability(50))
                        {

                            voxelMap[x, y, z] = VoxelData.BedRock;

                        }
                        else
                        {

                            voxelMap[x, y, z] = VoxelData.Stone;

                        }
                    }
                    //空气部分
                    else if (y > noiseHigh && y > world.terrainLayerProbabilitySystem.sea_level)
                    {

                        //地上一层
                        if (y - 1 < noiseHigh)
                        {

                            //如果可生成
                            if (voxelMap[x, y - 1, z] != VoxelData.Sand && voxelMap[x, y - 1, z] != VoxelData.Air && voxelMap[x, y - 1, z] != VoxelData.Snow)
                            {

                                //灌木丛
                                if (GetProbability(world.terrainLayerProbabilitySystem.Random_Bush))
                                {

                                    voxelMap[x, y, z] = VoxelData.Bush;

                                }
                                //BlueFlower
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_BlueFlower))
                                {

                                    voxelMap[x, y, z] = VoxelData.BlueFlower;

                                }
                                //WhiteFlower_1
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_WhiteFlower1))
                                {

                                    voxelMap[x, y, z] = VoxelData.WhiteFlower_1;

                                }
                                //WhiteFlower_2
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_WhiteFlower2))
                                {

                                    voxelMap[x, y, z] = VoxelData.WhiteFlower_2;

                                }
                                //YellowFlower
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_YellowFlower))
                                {

                                    voxelMap[x, y, z] = VoxelData.YellowFlower;

                                }
                                else
                                {

                                    voxelMap[x, y, z] = VoxelData.Air;

                                }
                            }

                            //竹子
                            else if (voxelMap[x, y - 1, z] == VoxelData.Sand && GetProbability(world.terrainLayerProbabilitySystem.Random_Bamboo))
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
                    else if (y > noiseHigh && y - 1 < world.terrainLayerProbabilitySystem.sea_level)
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
                            if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Dessert)
                            {

                                voxelMap[x, y, z] = VoxelData.Sand;

                            }

                            //草原气候
                            else
                            {
                                //100雪地
                                if (y > world.terrainLayerProbabilitySystem.Snow_Level)
                                {
                                    voxelMap[x, y, z] = VoxelData.Snow;
                                }

                                //90~100概率生成雪地
                                else if ((y > (world.terrainLayerProbabilitySystem.Snow_Level - 10f)) && GetProbability(50))
                                {
                                    voxelMap[x, y, z] = VoxelData.Snow;
                                }



                                //高于海平面
                                else if (y > world.terrainLayerProbabilitySystem.sea_level)
                                {

                                    //是否是菌丝体
                                    if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Marsh)
                                    {
                                        voxelMap[x, y, z] = VoxelData.Mycelium;
                                    }
                                    else
                                    {
                                        voxelMap[x, y, z] = VoxelData.Grass;
                                    }

                                }
                                else
                                {

                                    if (world.GetSimpleNoiseWithOffset(x, z, myposition, new Vector2(111f, 222f), 0.1f) > 0.5f)
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
                            if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Dessert)
                            {
                                voxelMap[x, y, z] = VoxelData.Sand;
                            }
                            else
                            {
                                voxelMap[x, y, z] = VoxelData.Soil;
                            }


                        }
                        else if (y >= (noiseHigh - 10) && y <= (noiseHigh - 7) && GetProbability(50))
                        {
                            //沙漠判断
                            if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Dessert)
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
                            if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Coal))
                            {
                                
                                voxelMap[x, y, z] = VoxelData.Stone;
                                Coals.Enqueue(new Vector3(x, y, z));

                            }

                            //铁
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Iron))
                            {
                                
                                voxelMap[x, y, z] = VoxelData.Iron;

                            }

                            //金
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Gold))
                            {
                                
                                voxelMap[x, y, z] = VoxelData.Gold;

                            }

                            //青金石
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Blue_Crystal))
                            {
                                
                                voxelMap[x, y, z] = VoxelData.Blue_Crystal;

                            }

                            //钻石
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Diamond))
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
        //密林群系
        if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Forest)
        {
            //[确定XZ]xoz上随便选择5个点
            while (Forest_treecount-- != 0)
            {

                int random_x = rand.Next(2, VoxelData.ChunkWidth - 2);
                int random_z = rand.Next(2, VoxelData.ChunkWidth - 2);
                int random_y = VoxelData.ChunkHeight;
                int random_Tree_High = rand.Next(world.terrainLayerProbabilitySystem.TreeHigh_min, world.terrainLayerProbabilitySystem.TreeHigh_max + 1);

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
        else
        {
            //[确定XZ]xoz上随便选择5个点
            int count = rand.Next(0, Normal_treecount);

            while (count-- != 0)
            {

                int random_x = rand.Next(2, VoxelData.ChunkWidth - 2);
                int random_z = rand.Next(2, VoxelData.ChunkWidth - 2);
                int random_y = VoxelData.ChunkHeight;
                int random_Tree_High = rand.Next(world.terrainLayerProbabilitySystem.TreeHigh_min, world.terrainLayerProbabilitySystem.TreeHigh_max + 1);

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


    //[水的流动]
    //如果自己是水
    //前后左右下如果是空气，则把他们变成水
    public void Always_updateWater()
    {

        //待优化 - 如果先判断周围一圈都是水，则不执行检查直接跳过

        ClearMeshData();

        //刷新自己
        for (y = 0; y < VoxelData.ChunkHeight; y++)
        {

            for (x = 0; x < VoxelData.ChunkWidth; x++)
            {

                for (z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    //修改VoxelMap
                    if(hasExec_isHadupdateWater)
                        _updateWater();

                    // 非空气 - 渲染
                    // 水面上 - 渲染
                    if (world.blocktypes[voxelMap[x, y, z]].DrawMode != DrawMode.Air)
                        UpdateMeshData(new Vector3(x, y, z));

                }

            }

        }



        hasExec_isHadupdateWater = true;

        //添加到world的渲染队列
        isReadyToRender = true;

        //交给渲染线程
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

    }


    void _updateWater()
    {
        //只在自己是水的情况下执行
        if (voxelMap[x, y, z] == VoxelData.Water)
        {

            //检查五个方向
            for (int _p = 0; _p < 5; _p++)
            {

                //如果出界
                if (isOutOfRange(new Vector3(x,y,z) + VoxelData.faceChecks_WaterFlow[_p]))
                {

                    //能获取到对面Chunk
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks_WaterFlow[_p], out Chunk chunktemp))
                    {

                        Vector3 directlocation = GetDirectChunkVoxelMapLocation(new Vector3(x,y,z) + VoxelData.faceChecks_WaterFlow[_p]);

                        if (chunktemp.voxelMap[(int)directlocation.x, (int)directlocation.y, (int)directlocation.z] == VoxelData.Air)
                        {

                            chunktemp.voxelMap[(int)directlocation.x, (int)directlocation.y, (int)directlocation.z] = VoxelData.Water;
                            
                            //给不含水区块变成含水区块
                            if (chunktemp.iHaveWater == false)
                            {

                                chunktemp.iHaveWater = true;

                            }

                            hasExec_isHadupdateWater = false;

                        }

                    }


                }

                //没出界
                else
                {

                    if (voxelMap[x + (int)VoxelData.faceChecks_WaterFlow[_p].x, y + (int)VoxelData.faceChecks_WaterFlow[_p].y, z + (int)VoxelData.faceChecks_WaterFlow[_p].z] == VoxelData.Air)
                    {

                        voxelMap[x + (int)VoxelData.faceChecks_WaterFlow[_p].x, y + (int)VoxelData.faceChecks_WaterFlow[_p].y, z + (int)VoxelData.faceChecks_WaterFlow[_p].z] = VoxelData.Water;
                        
                        hasExec_isHadupdateWater = false;

                    }

                }


            }

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

                    //[已废弃，移动至单独的线程执行]水的流动
                    //updateWater();

                    // 非空气 - 渲染
                    // 水面上 - 渲染
                    if (world.blocktypes[voxelMap[x, y, z]].DrawMode != DrawMode.Air)
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
            {
                world.WaitToFlashChunkQueue.Enqueue(DirectChunk);
                //DirectChunk.UpdateChunkMesh_WithSurround();
            }
            if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + new Vector3(0.0f, 0.0f, -1.0f), out DirectChunk))
            {
                world.WaitToFlashChunkQueue.Enqueue(DirectChunk);
                //DirectChunk.UpdateChunkMesh_WithSurround();
            }
            if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + new Vector3(-1.0f, 0.0f, 0.0f), out DirectChunk))
            {
                world.WaitToFlashChunkQueue.Enqueue(DirectChunk);
                //DirectChunk.UpdateChunkMesh_WithSurround();
            }
            if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + new Vector3(1.0f, 0.0f, 0.0f), out DirectChunk))
            {
                world.WaitToFlashChunkQueue.Enqueue(DirectChunk);
                //DirectChunk.UpdateChunkMesh_WithSurround();
            }

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
        triangles_Water.Clear();
        uvs.Clear();

    }

    //编辑方块
    public void EditData(Vector3 pos, byte targetBlocktype)
    {

        //ClearFInd_Direvtion();

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        //防止过高
        if (y >= VoxelData.ChunkHeight - 2)
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

                        //[已废弃，代码统一到一个函数里了]
                        //水的流动
                        //如果自己是水且对面是空气，则把对面变成水
                        //if (voxelMap[x, y, z - 1] == VoxelData.Water && chunktemp.voxelMap[x, y, 0] == VoxelData.Air)
                        //{
                        //    chunktemp.voxelMap[x, y, 0] = VoxelData.Water;

                        //    world.WaitToCreateMesh.Enqueue(chunktemp);

                        //    return true;
                        //}


                        //target是透明的
                        //target是水
                        //且自己不是透明的
                        //则返回false
                        //(目标是透明的 || 目标是水) && (自己不是透明的 && 自己不是水)
                        //if (((world.blocktypes[chunktemp.voxelMap[x, y, 0]].isTransparent || chunktemp.voxelMap[x, y, 0] == VoxelData.Water)  && (!world.blocktypes[voxelMap[x, y, z - 1]].isTransparent && voxelMap[x, y, z - 1] != VoxelData.Water)))
                        //{

                        //    return false;
                        //}
                        //else
                        //{
                        //    return true;
                        //}
                        return CheckSelfAndTarget(voxelMap[x, y, z - 1], chunktemp.voxelMap[x, y, 0]);


                    }
                    else
                    {
                        //if (debug_CanLookCave)
                        //{
                        //    return false;
                        //}
                        //else
                        //{
                        //    return true;
                        //}

                        //查不到一律生成
                        return false;

                    }

                }


                //Back
                if (z < 0)
                {

                    //如果能查到
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                    {


                        //[已废弃，代码统一到一个函数里了]
                        //水的流动
                        //如果自己是水且对面是空气，则把对面变成水
                        //if (voxelMap[x, y, z + 1] == VoxelData.Water && chunktemp.voxelMap[x, y, VoxelData.ChunkWidth - 1] == VoxelData.Air)
                        //{
                        //    chunktemp.voxelMap[x, y, 0] = VoxelData.Water;

                        //    return true;
                        //}



                        //如果target是空气，则返回false
                        //target是水
                        //if (((world.blocktypes[chunktemp.voxelMap[x, y, VoxelData.ChunkWidth - 1]].isTransparent || chunktemp.voxelMap[x, y, VoxelData.ChunkWidth - 1] == VoxelData.Water) && (!world.blocktypes[voxelMap[x, y, z + 1]].isTransparent && voxelMap[x, y, z + 1] != VoxelData.Water)))
                        //{
                        //    return false;
                        //}
                        //else
                        //{
                        //    return true;
                        //}


                        return CheckSelfAndTarget(voxelMap[x, y, z + 1], chunktemp.voxelMap[x, y, VoxelData.ChunkWidth - 1]);


                    }
                    else
                    {
                        //if (debug_CanLookCave)
                        //{
                        //    return false;
                        //}
                        //else
                        //{
                        //    return true;
                        //}

                        //查不到一律生成
                        return false;

                    }


                }


                //Left
                if (x < 0)
                {

                    //如果能查到
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                    {


                        //[已废弃，代码统一到一个函数里了]
                        //水的流动
                        //如果自己是水且对面是空气，则把对面变成水
                        //if (voxelMap[x + 1, y, z] == VoxelData.Water && chunktemp.voxelMap[VoxelData.ChunkWidth - 1, y, z] == VoxelData.Air)
                        //{
                        //    chunktemp.voxelMap[x, y, 0] = VoxelData.Water;

                        //    return true;
                        //}

                        //如果target是空气，则返回false
                        //target是水
                        //if (((world.blocktypes[chunktemp.voxelMap[VoxelData.ChunkWidth - 1, y, z]].isTransparent || chunktemp.voxelMap[VoxelData.ChunkWidth - 1, y, z] == VoxelData.Water) && (!world.blocktypes[voxelMap[x + 1, y, z]].isTransparent && voxelMap[x + 1, y, z] != VoxelData.Water)))
                        //{
                        //    return false;
                        //}
                        //else
                        //{
                        //    return true;
                        //}

                        return CheckSelfAndTarget(voxelMap[x + 1, y, z], chunktemp.voxelMap[VoxelData.ChunkWidth - 1, y, z]);

                    }
                    else
                    {
                    //    if (debug_CanLookCave)
                    //    {
                    //        return false;
                    //    }
                    //    else
                    //    {
                    //        return true;
                    //    }

                        //查不到一律生成
                        return false;

                    }

                }


                //Right
                if (x > VoxelData.ChunkWidth - 1)
                {

                    //如果能查到
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                    {


                        //[已废弃，代码统一到一个函数里了]
                        //水的流动
                        ////如果自己是水且对面是空气，则把对面变成水
                        //if (voxelMap[x - 1, y, z] == VoxelData.Water && chunktemp.voxelMap[0, y, z] == VoxelData.Air)
                        //{
                        //    chunktemp.voxelMap[x, y, 0] = VoxelData.Water;

                        //    return true;
                        //}


                        //如果target是空气，则返回false
                        //target是水
                        //if (((world.blocktypes[chunktemp.voxelMap[0, y, z]].isTransparent || chunktemp.voxelMap[0, y, z] == VoxelData.Water) && (!world.blocktypes[voxelMap[x - 1, y, z]].isTransparent && voxelMap[x - 1, y, z] != VoxelData.Water)))
                        //{
                        //    return false;
                        //}
                        //else
                        //{
                        //    return true;
                        //}

                        return CheckSelfAndTarget(voxelMap[x - 1, y, z], chunktemp.voxelMap[0, y, z]);

                    }
                    else
                    {
                        //if (debug_CanLookCave)
                        //{
                        //    return false;
                        //}
                        //else
                        //{
                        //    return true;
                        //}

                        //查不到一律生成
                        return false;

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
            if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air) && voxelMap[x, y, z] == VoxelData.Air) || ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Water) && voxelMap[x, y, z] == VoxelData.Water))
            {

                return true;

            }
            else // 生成面的情况
            {
                //如果自己是水，目标是空气
                //生成面
                if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Water) && voxelMap[x, y, z] == VoxelData.Air))
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

    //接收自己和目标的类型，判断是否生成面
    //false：不生成
    bool CheckSelfAndTarget(byte _self,byte _target)
    {

        // 如果自己是空气或灌木，无论如何都不生成面
        if (world.blocktypes[_self].isTransparent)
        {

            return true;

        }

        //如果自己是水
        // 目标是Transparent，生成
        //目标是水或者固体，不生成
        if (_self == VoxelData.Water)
        {

            if (world.blocktypes[_target].isTransparent)
            {

                return false;

            }

            if (_target == VoxelData.Water || world.blocktypes[_target].isSolid)
            {

                return true;

            }

        }

        //如果自己是固体
        //目标是固体，则不生成
        //其他都生成
        if (world.blocktypes[_self].isSolid)
        {

            if (world.blocktypes[_target].isSolid)
            {

                return true;


            }
            else
            {

                return false;

            }

        }



        return true;

    }




    //遍历中：：顺带判断面的生成方向
    //创建mesh里的参数
    //_calledFrom = 0 来自UpdateChunkMesh_WithSurround
    //_calledFrom = 1 来自UpdateWater
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



            case DrawMode.Water:  //水面绘制模式



                //判断六个面
                for (int p = 0; p < 6; p++)
                {

                    //可以绘制
                    if (!CheckVoxel(pos + VoxelData.faceChecks[p], p))
                    {

                        //如果上下方有水，则换成方块的渲染方式
                        if ((voxelMap[(int)pos.x, (int)pos.y + 1, (int)pos.z] == VoxelData.Water || voxelMap[(int)pos.x, (int)pos.y - 1, (int)pos.z] == VoxelData.Water) && p != 2 && p != 3 && voxelMap[(int)pos.x, (int)pos.y + 1, (int)pos.z] != VoxelData.Air)
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

                            vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 0]]);
                            vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 1]]);
                            vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 2]]);
                            vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 3]]);

                            //uvs.Add(VoxelData.voxelUvs[0]);
                            //uvs.Add(VoxelData.voxelUvs[1]);
                            //uvs.Add(VoxelData.voxelUvs[2]);
                            //uvs.Add(new Vector2(0 ,0));
                            //uvs.Add(new Vector2(0, 1));
                            //uvs.Add(new Vector2(1, 1));
                            //uvs.Add(new Vector2(1, 0));
                            //AddTexture(1);

                            //根据p生成对应的面，对应的UV
                            //AddTexture(world.blocktypes[blockID].GetTextureID(p));
                            uvs.Add(new Vector2(0f, 0f));
                            uvs.Add(new Vector2(0f, 1f));
                            uvs.Add(new Vector2(1f, 0f));
                            uvs.Add(new Vector2(1f, 1f));

                            triangles_Water.Add(vertexIndex);
                            triangles_Water.Add(vertexIndex + 1);
                            triangles_Water.Add(vertexIndex + 2);
                            triangles_Water.Add(vertexIndex + 2);
                            triangles_Water.Add(vertexIndex + 1);
                            triangles_Water.Add(vertexIndex + 3);
                            vertexIndex += 4;

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

        world.RenderLock = true;

        //print($"{world.GetChunkLocation(myposition)}CreateMesh 开始");

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

        //print($"triangles:{triangles.Count}, triangles_Water:{triangles_Water.Count}");

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






    //------------------------------ PerlinNoise ------------------------------------


    //public bool isFind_Front = false;
    //public bool isFind_Back = false;
    //public bool isFind_Left = false;
    //public bool isFind_Right = false;

    //public void ClearFInd_Direvtion()
    //{
    //    isFind_Front = false;
    //    isFind_Back = false;
    //    isFind_Left = false;
    //    isFind_Right = false;
    //}


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
    bool isOutOfRange(int _x, int _y, int _z)
    {

        if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
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

        if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
        {

            return true;

        }
        else
        {

            return false;

        }

    }


    //是否在边框上
    bool isOnEdge(int x, int y, int z)
    {

        if (x == 0 || x == VoxelData.ChunkWidth - 1 || y == 0 || y == VoxelData.ChunkHeight - 1 || z == 0 || z == VoxelData.ChunkWidth - 1)
        {

            return true;

        }
        else
        {

            return false;

        }
    }


    //洞穴生成概率
    //float Probability(float y)
    //{

    //    //float possible = 0;
    //    //float mid = world.soil_max / 2;

    //    //// 如果y越接近0，则possible越接近0，反之越接近1
    //    //float ratio = y / mid;
    //    //possible = 1 - ratio;

    //    //return Mathf.Clamp01(possible); // 返回归一化到 [0, 1] 区间的概率值

    //}

    //获得对面Chunk的坐标
    //0 0 -1 会变成 0 0 15
    //0 0 16 会变成 0 0 0
    Vector3 GetDirectChunkVoxelMapLocation(Vector3 _vec)
    {

        //检测到16或者-1
        //Front
        if (_vec.z == 16)
        {

            return new Vector3(_vec.x, _vec.y, 0);

        }

        //Back
        else if (_vec.z == -1)
        {

            return new Vector3(_vec.x, _vec.y, 15);

        }

        //Left
        else if (_vec.x == -1)
        {

            return new Vector3(15, _vec.y, _vec.z);

        }

        //Right
        else if (_vec.x == 16)
        {
            return new Vector3(0, _vec.y, _vec.z);
        }

        //没检测到
        else
        {

            print("Chunk.GetDirectChunkVoxelMapLocation()参数输入异常！");
            return new Vector3(0, 0, 0);

        }

    }


    //----------------------------------------------------------------------------------

}




//PerlinNoise噪声类
public static class PerlinNoise
{
    static float interpolate(float a0, float a1, float w)
    {
        //线性插值
        //return (a1 - a0) * w + a0;

        //hermite插值
        return Mathf.SmoothStep(a0, a1, w);
    }


    static Vector2 randomVector2(Vector2 p)
    {
        float random = Mathf.Sin(666 + p.x * 5678 + p.y * 1234) * 4321;
        return new Vector2(Mathf.Sin(random), Mathf.Cos(random));
    }


    static float dotGridGradient(Vector2 p1, Vector2 p2)
    {
        Vector2 gradient = randomVector2(p1);
        Vector2 offset = p2 - p1;
        return Vector2.Dot(gradient, offset) / 2 + 0.5f;
    }


    public static float perlin(float x, float y)
    {
        //声明二维坐标
        Vector2 pos = new Vector2(x, y);
        //声明该点所处的'格子'的四个顶点坐标
        Vector2 rightUp = new Vector2((int)x + 1, (int)y + 1);
        Vector2 rightDown = new Vector2((int)x + 1, (int)y);
        Vector2 leftUp = new Vector2((int)x, (int)y + 1);
        Vector2 leftDown = new Vector2((int)x, (int)y);

        //计算x上的插值
        float v1 = dotGridGradient(leftDown, pos);
        float v2 = dotGridGradient(rightDown, pos);
        float interpolation1 = interpolate(v1, v2, x - (int)x);

        //计算y上的插值
        float v3 = dotGridGradient(leftUp, pos);
        float v4 = dotGridGradient(rightUp, pos);
        float interpolation2 = interpolate(v3, v4, x - (int)x);

        float value = interpolate(interpolation1, interpolation2, y - (int)y);
        return value;
    }
}