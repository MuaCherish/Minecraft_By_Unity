//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using static UnityEngine.GraphicsBuffer;
//using static UnityEditor.PlayerSettings;
//using static UnityEditor.Progress;
//using static UnityEditor.PlayerSettings;
//using System.Diagnostics;




public class Chunk : MonoBehaviour
{

    //state
    public bool isShow = true;
    public bool isReadyToRender = false;
    public bool BaseChunk;
    public bool isCalled = false;  //不要删我！！
    public bool iHaveWater = false;
    // private bool haeExec_iHaveWater = true;
    public bool hasExec_isHadupdateWater = true;  //该标志允许一个Chunk每次只能更新一次水体

    //Biome
    public int worldType = 5;
    //public bool isSuperPlainMode; 

    //Transform
    World world;
    ManagerHub managerhub;
    public GameObject chunkObject;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    //noise
    private float noise3d_scale;

    //存档
    public bool isSaving = false;
    public List<EditStruct> EditList = new List<EditStruct>();

    //群系参数
    int Normal_treecount;
    int Forest_treecount;

    //BlockMap
    //private int x;
    //private int y;
    //private int z;
    public VoxelStruct[,,] voxelMap = new VoxelStruct[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];


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
    Thread myThread;
    public System.Random rand;
    public Vector3 myposition;


    //debug
    //bool debug_CanLookCave;




    //---------------------------------- 周期函数 ---------------------------------------




    //Start()
    public Chunk(Vector3 thisPosition, ManagerHub _managerhub, bool _BaseChunk)
    {


        //World
        world = _managerhub.world;
        managerhub = _managerhub;
        caveWidth = world.cave_width;
        //debug_CanLookCave = !world.debug_CanLookCave;
        BaseChunk = _BaseChunk;
        noise3d_scale = world.noise3d_scale;
        Normal_treecount = world.terrainLayerProbabilitySystem.Normal_treecount;
        Forest_treecount = world.terrainLayerProbabilitySystem.密林树木采样次数Forest_treecount;
        //isSuperPlainMode = _isSuperPlainMode;

        //Self
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = world.material;
        chunkObject.transform.SetParent(world.Chunks.transform);
        chunkObject.transform.position = new Vector3(thisPosition.x * VoxelData.ChunkWidth, 0f, thisPosition.z * VoxelData.ChunkWidth);
        chunkObject.name = thisPosition.x + "," + thisPosition.z;
        myposition = chunkObject.transform.position;
        rand = new System.Random(world.worldSetting.seed);
        //print(world.worldSetting.seed);

        //初始化Voxel数组
        InitVoxelStruct();


        //获取群系类型
        worldType = world.worldSetting.worldtype;
        //print(worldType);

        switch (worldType)
        {
            //草原群系
            case 0:
                myThread = new Thread(new ThreadStart(CreateData));
                break;
            //高原群系
            case 1:
                myThread = new Thread(new ThreadStart(CreateData));
                break;
            //沙漠群系
            case 2:
                myThread = new Thread(new ThreadStart(CreateData_Dessert));
                break;
            //沼泽群系
            case 3:
                myThread = new Thread(new ThreadStart(CreateData_Marsh));
                break;
            //密林群系
            case 4:
                myThread = new Thread(new ThreadStart(CreateData_Forest));
                break;
            //默认
            case 5:
                myThread = new Thread(new ThreadStart(CreateData));
                break;
            //超平坦
            case 6:
                myThread = new Thread(new ThreadStart(CreateData_SuperPlain));
                //CreateData_SuperPlain();
                break;
            default:
                print("chunk.worldType出错");
                break;
        }

        myThread.Start();

        //print($"----------------------------------------------");
        //print($"{world.GetChunkLocation(myposition)}已经生成！");
    }


    //Start()
    public Chunk(Vector3 thisPosition, ManagerHub _managerhub, bool _BaseChunk, List<EditStruct> _editList)
    {


        //World
        world = _managerhub.world;
        managerhub = _managerhub;
        caveWidth = world.cave_width;
        //debug_CanLookCave = !world.debug_CanLookCave;
        BaseChunk = _BaseChunk;
        noise3d_scale = world.noise3d_scale;
        Normal_treecount = world.terrainLayerProbabilitySystem.Normal_treecount;
        Forest_treecount = world.terrainLayerProbabilitySystem.密林树木采样次数Forest_treecount;
        //isSuperPlainMode = _isSuperPlainMode;
        isSaving = true;
        EditList = _editList;

        //Self
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = world.material;
        chunkObject.transform.SetParent(world.Chunks.transform);
        chunkObject.transform.position = new Vector3(thisPosition.x * VoxelData.ChunkWidth, 0f, thisPosition.z * VoxelData.ChunkWidth);
        chunkObject.name = thisPosition.x + "," + thisPosition.z;
        myposition = chunkObject.transform.position;
        rand = new System.Random(world.worldSetting.seed);

        //初始化Voxel数组
        InitVoxelStruct();


        //获取群系类型
        worldType = world.worldSetting.worldtype;
        //print(worldType);

        switch (worldType)
        {
            //草原群系
            case 0:
                myThread = new Thread(new ThreadStart(CreateData));
                break; 
            //高原群系
            case 1:
                myThread = new Thread(new ThreadStart(CreateData));
                break;
            //沙漠群系
            case 2:
                myThread = new Thread(new ThreadStart(CreateData_Dessert));
                break;
            //沼泽群系
            case 3:
                myThread = new Thread(new ThreadStart(CreateData_Marsh));
                break;
            //密林群系
            case 4:
                myThread = new Thread(new ThreadStart(CreateData_Forest));
                break;
            //默认
            case 5:
                myThread = new Thread(new ThreadStart(CreateData));
                break;
            //超平坦
            case 6:
                myThread = new Thread(new ThreadStart(CreateData_SuperPlain));
                //CreateData_SuperPlain();
                break;
            default:
                print("chunk.worldType出错");
                break;
        }

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


        float smoothNoise = Mathf.Lerp((float)0, (float)100, Mathf.PerlinNoise((myposition.x + Offset_x) * 0.005f, (myposition.z + Offset_z) * 0.005f));
        return smoothNoise;

    }



    //Desert简单噪声
    float GetSmoothNoise_Desert(int _x, int _z)
    {

        //float randomoffset = rand.Next(0, 10);
        //float Offset_x = 100f * randomoffset;
        //float Offset_z = 100f * randomoffset;

        return Mathf.PerlinNoise((_x + myposition.x) * 0.003f, (_z + myposition.z) * 0.003f);
    }



    //洞穴噪声生成器
    float GetCaveNoise(int _x, int _y, int _z)
    {
        return Perlin3D(((float)_x + myposition.x) * noise3d_scale, ((float)_y + myposition.y) * noise3d_scale, ((float)_z + myposition.z) * noise3d_scale); // 将100改为0.1

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
                    float noiseHigh = world.GetTotalNoiseHigh_Biome(x, z, myposition, world.worldSetting.worldtype);


                    //矿洞噪声
                    float noise3d = GetCaveNoise(x, y, z);


                    //数据缓冲
                    byte terrainFace = GetBlock(x, y - 1, z).voxelType;


                    //沙漠噪声
                    //float noise_desery = GetSmoothNoise_Desert(x, z);


                    //判断基岩
                    //0~3层不准生成矿洞
                    if (y >= 0 && y <= 3)
                    {

                        if (y == 0)
                        {

                            UpdateBlock(x, y, z, VoxelData.BedRock);

                        }
                        else if (y > 0 && y < 3 && GetProbability(50))
                        {

                            UpdateBlock(x, y, z, VoxelData.BedRock);

                        }
                        else
                        {

                            UpdateBlock(x, y, z, VoxelData.Stone);

                        }
                    }
                    //空气部分
                    else if (y > noiseHigh && y > world.terrainLayerProbabilitySystem.sea_level)
                    {

                        //地上一层
                        if (y - 1 < noiseHigh)
                        {

                            //草地层
                            if (terrainFace != VoxelData.Sand && terrainFace != VoxelData.Air && terrainFace != VoxelData.Snow)
                            {

                                //灌木丛
                                if (GetProbability(world.terrainLayerProbabilitySystem.Random_Bush))
                                {

                                    UpdateBlock(x, y, z, VoxelData.Bush);

                                }
                                //BlueFlower
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_BlueFlower))
                                {

                                    UpdateBlock(x, y, z, VoxelData.BlueFlower);

                                }
                                //WhiteFlower_1
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_WhiteFlower1))
                                {

                                    UpdateBlock(x, y, z, VoxelData.WhiteFlower_1);

                                }
                                //WhiteFlower_2
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_WhiteFlower2))
                                {

                                    UpdateBlock(x, y, z, VoxelData.WhiteFlower_2);

                                }
                                //YellowFlower
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_YellowFlower))
                                {

                                    UpdateBlock(x, y, z, VoxelData.YellowFlower);

                                }
                                //草地雪碎片
                                else if (y > world.terrainLayerProbabilitySystem.Snow_Level - 10)
                                {
                                    UpdateBlock(x, y, z, VoxelData.SnowPower);
                                }
                                else
                                {

                                    UpdateBlock(x, y, z, VoxelData.Air) ;

                                }
                            }

                            //雪地层概率生成雪碎片
                            else if (terrainFace == VoxelData.Snow && GetProbability(50))
                            {
                                UpdateBlock(x, y, z, VoxelData.SnowPower) ;
                            }


                            //沙子层
                            else if (terrainFace == VoxelData.Sand && GetProbability(world.terrainLayerProbabilitySystem.Random_Bamboo))
                            {

                                UpdateBlock(x, y, z, VoxelData.Air) ;
                                Bamboos.Enqueue(new Vector3(x, y, z));

                            }
                            else
                            {

                                UpdateBlock(x, y, z, VoxelData.Air);

                            }
                        }

                        else
                        {

                            UpdateBlock(x, y, z, VoxelData.Air) ;

                        }
                    }

                    //判断水面
                    else if (y > noiseHigh && y - 1 < world.terrainLayerProbabilitySystem.sea_level)
                    {

                        UpdateBlock(x, y, z, VoxelData.Water) ;

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

                                UpdateBlock(x, y, z, VoxelData.Sand) ;

                            }

                            //草原气候
                            else
                            {
                                //100雪地
                                if (y > world.terrainLayerProbabilitySystem.Snow_Level)
                                {
                                    UpdateBlock(x, y, z, VoxelData.Snow) ;
                                }

                                //90~100概率生成雪地
                                else if ((y > (world.terrainLayerProbabilitySystem.Snow_Level - 10f)) && GetProbability(70))
                                {
                                    UpdateBlock(x, y, z, VoxelData.Snow) ;
                                }



                                //高于海平面
                                else if (y > world.terrainLayerProbabilitySystem.sea_level)
                                {

                                    //是否是菌丝体
                                    if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Marsh)
                                    {
                                        UpdateBlock(x, y, z, VoxelData.Mycelium) ;
                                    }
                                    else
                                    {
                                        UpdateBlock(x, y, z, VoxelData.Grass) ;
                                    }

                                }
                                else
                                {

                                    if (world.GetSimpleNoiseWithOffset(x, z, myposition, new Vector2(111f, 222f), 0.1f) > 0.5f)
                                    {

                                        UpdateBlock(x, y, z, VoxelData.Sand) ;

                                    }
                                    else
                                    {

                                        UpdateBlock(x, y, z, VoxelData.Soil) ;

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
                                UpdateBlock(x, y, z, VoxelData.Sand) ;
                            }
                            else
                            {
                                UpdateBlock(x, y, z, VoxelData.Soil) ;
                            }


                        }
                        else if (y >= (noiseHigh - 10) && y <= (noiseHigh - 7) && GetProbability(50))
                        {
                            //沙漠判断
                            if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Dessert)
                            {
                                UpdateBlock(x, y, z, VoxelData.Sand) ;
                            }
                            else
                            {
                                UpdateBlock(x, y, z, VoxelData.Soil) ;
                            }


                        }





                        //矿洞
                        else if (noise3d < GetVaveWidth(y))
                        {

                            UpdateBlock(x, y, z, VoxelData.Air) ;

                        }

                        //地下判断
                        else
                        {

                            //煤炭
                            if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Coal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Stone) ;
                                Coals.Enqueue(new Vector3(x, y, z));

                            }

                            //铁
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Iron))
                            {

                                UpdateBlock(x, y, z, VoxelData.Iron) ;

                            }

                            //金
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Gold))
                            {

                                UpdateBlock(x, y, z, VoxelData.Gold) ;

                            }

                            //青金石
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Blue_Crystal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Blue_Crystal) ;

                            }

                            //钻石
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Diamond))
                            {

                                UpdateBlock(x, y, z, VoxelData.Diamond) ;

                            }

                            else
                            {

                                UpdateBlock(x, y, z, VoxelData.Stone) ;

                            }
                        }
                    }
                }
            }
        }


        //补充树
        CreateTree(0, 0);

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

        if (isSaving)
        {
            LoadData(EditList);
        }

        //交给world来create
        world.WaitToCreateMesh.Enqueue(this);

    }

    //超平坦世界
    void CreateData_SuperPlain()
    {
        //对一个chunk进行遍历
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (y == 0)
                    {
                        UpdateBlock(x, y, z, VoxelData.BedRock) ;
                    }
                    else if (y == 1 || y == 2)
                    {
                        UpdateBlock(x, y, z, VoxelData.Soil) ;
                    }
                    else if (y == 3)
                    {
                        UpdateBlock(x, y, z, VoxelData.Grass) ;
                    }
                    else
                    {
                        UpdateBlock(x, y, z, VoxelData.Air) ;
                    }
                }
            }
        }

        if (isSaving)
        {
            LoadData(EditList);
        }


        //交给world来create
        world.WaitToCreateMesh.Enqueue(this);
        //UpdateChunkMesh_WithSurround(false, true);
    }

    //沙漠群系
    void CreateData_Dessert()
    {
        //对一个chunk进行遍历
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {



                    //地形噪声
                    //float noiseHigh = GetTotalNoiseHigh(x, z);
                    float noiseHigh = world.GetTotalNoiseHigh_Biome(x, z, myposition, world.worldSetting.worldtype);
                    //矿洞噪声
                    float noise3d = GetCaveNoise(x, y, z);

                    //数据缓冲
                    byte terrainFace = GetBlock(x, y - 1, z).voxelType;


                    //判断基岩
                    //0~3层不准生成矿洞
                    if (y >= 0 && y <= 3)
                    {

                        if (y == 0)
                        {

                            UpdateBlock(x, y, z, VoxelData.BedRock) ;

                        }
                        else if (y > 0 && y < 3 && GetProbability(50))
                        {

                            UpdateBlock(x, y, z, VoxelData.BedRock) ;

                        }
                        else
                        {

                            UpdateBlock(x, y, z, VoxelData.Stone) ;

                        }
                    }
                    //空气部分
                    else if (y > noiseHigh && y > world.terrainLayerProbabilitySystem.sea_level)
                    {

                        //地上一层
                        if (y - 1 < noiseHigh)
                        {

                            if (terrainFace == VoxelData.Sand && GetProbability(world.terrainLayerProbabilitySystem.Random_Bamboo))
                            {

                                UpdateBlock(x, y, z, VoxelData.Air) ;
                                Bamboos.Enqueue(new Vector3(x, y, z));

                            }
                            else
                            {
                                UpdateBlock(x, y, z, VoxelData.Air) ;

                            }
                        }

                        else
                        {

                            UpdateBlock(x, y, z, VoxelData.Air) ;

                        }
                    }
                    //空气之下
                    else
                    {

                        //地表
                        if ((y + 1) > noiseHigh)
                        {
                            UpdateBlock(x, y, z, VoxelData.Sand) ;
                        }


                        //泥土的判断
                        else if (y > noiseHigh - 7)
                        {
                            UpdateBlock(x, y, z, VoxelData.Sand) ;


                        }
                        else if (y >= (noiseHigh - 10) && y <= (noiseHigh - 7) && GetProbability(50))
                        {
                            //沙漠判断
                            if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Dessert)
                            {
                                UpdateBlock(x, y, z, VoxelData.Sand) ;
                            }
                            else
                            {
                                UpdateBlock(x, y, z, VoxelData.Soil) ;
                            }


                        }
                        //矿洞
                        else if (noise3d < GetVaveWidth(y))
                        {

                            UpdateBlock(x, y, z, VoxelData.Air) ;

                        }

                        //地下判断
                        else
                        {

                            //煤炭
                            if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Coal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Stone) ;
                                Coals.Enqueue(new Vector3(x, y, z));

                            }

                            //铁
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Iron))
                            {

                                UpdateBlock(x, y, z, VoxelData.Iron) ;

                            }

                            //金
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Gold))
                            {

                                UpdateBlock(x, y, z, VoxelData.Gold) ;

                            }

                            //青金石
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Blue_Crystal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Blue_Crystal) ;

                            }

                            //钻石
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Diamond))
                            {

                                UpdateBlock(x, y, z, VoxelData.Diamond) ;

                            }

                            else
                            {

                                UpdateBlock(x, y, z, VoxelData.Stone) ;

                            }
                        }
                    }
                }
            }
        }


        //补充树
        //CreateTree();

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

        if (isSaving)
        {
            LoadData(EditList);
        }

        //交给world来create
        world.WaitToCreateMesh.Enqueue(this);
    }

    //沼泽群系
    void CreateData_Marsh()
    {


        //对一个chunk进行遍历
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    //地形噪声
                    //float noiseHigh = GetTotalNoiseHigh(x, z);
                    float noiseHigh = world.GetTotalNoiseHigh_Biome(x, z, myposition, world.worldSetting.worldtype);


                    //矿洞噪声
                    float noise3d = GetCaveNoise(x, y, z);

                    //数据缓冲
                    byte terrainFace = GetBlock(x, y - 1, z).voxelType;



                    //判断基岩
                    //0~3层不准生成矿洞
                    if (y >= 0 && y <= 3)
                    {

                        if (y == 0)
                        {

                            UpdateBlock(x, y, z, VoxelData.BedRock) ;

                        }
                        else if (y > 0 && y < 3 && GetProbability(50))
                        {

                            UpdateBlock(x, y, z, VoxelData.BedRock) ;

                        }
                        else
                        {

                            UpdateBlock(x, y, z, VoxelData.Stone) ;

                        }
                    }
                    //空气部分
                    else if (y > noiseHigh && y > world.terrainLayerProbabilitySystem.sea_level)
                    {

                        //地上一层
                        if (y - 1 < noiseHigh)
                        {

                            //草地层
                            if (terrainFace != VoxelData.Sand && terrainFace != VoxelData.Air && terrainFace != VoxelData.Snow)
                            {

                                //灌木丛
                                if (GetProbability(world.terrainLayerProbabilitySystem.Random_Bush))
                                {

                                    UpdateBlock(x, y, z, VoxelData.Bush) ;

                                }
                                //BlueFlower
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_BlueFlower))
                                {

                                    UpdateBlock(x, y, z, VoxelData.BlueFlower) ;

                                }
                                //WhiteFlower_1
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_WhiteFlower1))
                                {

                                    UpdateBlock(x, y, z, VoxelData.WhiteFlower_1) ;

                                }
                                //WhiteFlower_2
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_WhiteFlower2))
                                {

                                    UpdateBlock(x, y, z, VoxelData.WhiteFlower_2) ;

                                }
                                //YellowFlower
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_YellowFlower))
                                {

                                    UpdateBlock(x, y, z, VoxelData.YellowFlower);

                                }
                                //草地雪碎片
                                else if (y > world.terrainLayerProbabilitySystem.Snow_Level - 10)
                                {
                                    UpdateBlock(x, y, z, VoxelData.SnowPower);
                                }
                                else
                                {

                                    UpdateBlock(x, y, z, VoxelData.Air) ;

                                }
                            }

                            //雪地层概率生成雪碎片
                            else if (terrainFace == VoxelData.Snow && GetProbability(50))
                            {
                                UpdateBlock(x, y, z, VoxelData.SnowPower) ;
                            }


                            //沙子层
                            else if (terrainFace == VoxelData.Sand && GetProbability(world.terrainLayerProbabilitySystem.Random_Bamboo))
                            {

                                UpdateBlock(x, y, z, VoxelData.Air) ;
                                Bamboos.Enqueue(new Vector3(x, y, z));

                            }
                            else
                            {

                                UpdateBlock(x, y, z, VoxelData.Air) ;

                            }
                        }

                        else
                        {

                            UpdateBlock(x, y, z, VoxelData.Air) ;

                        }
                    }

                    //判断水面
                    else if (y > noiseHigh && y - 1 < world.terrainLayerProbabilitySystem.sea_level)
                    {

                        UpdateBlock(x, y, z, VoxelData.Water) ;

                    }

                    //空气之下
                    else
                    {

                        //地表
                        if ((y + 1) > noiseHigh)
                        {

                            UpdateBlock(x, y, z, VoxelData.Mycelium) ;
                        }


                        //泥土的判断
                        else if (y > noiseHigh - 7)
                        {
                            UpdateBlock(x, y, z, VoxelData.Soil) ;


                        }
                        else if (y >= (noiseHigh - 10) && y <= (noiseHigh - 7) && GetProbability(50))
                        {

                            UpdateBlock(x, y, z, VoxelData.Soil) ;


                        }





                        //矿洞
                        else if (noise3d < GetVaveWidth(y))
                        {

                            UpdateBlock(x, y, z, VoxelData.Air) ;

                        }

                        //地下判断
                        else
                        {

                            //煤炭
                            if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Coal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Stone) ;
                                Coals.Enqueue(new Vector3(x, y, z));

                            }

                            //铁
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Iron))
                            {

                                UpdateBlock(x, y, z, VoxelData.Iron) ;

                            }

                            //金
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Gold))
                            {

                                UpdateBlock(x, y, z, VoxelData.Gold) ;

                            }

                            //青金石
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Blue_Crystal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Blue_Crystal) ;

                            }

                            //钻石
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Diamond))
                            {

                                UpdateBlock(x, y, z, VoxelData.Diamond) ;

                            }

                            else
                            {

                                UpdateBlock(x, y, z, VoxelData.Stone) ;

                            }
                        }
                    }
                }
            }
        }


        //补充树
        //[确定XZ]xoz上随便选择5个点
        int _Forest_treecount = 3;

        if (GetProbability(70))
        {
            _Forest_treecount = 0;
        }

        while (_Forest_treecount-- != 0)
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

                        UpdateBlock(random_x, random_y + i, random_z, VoxelData.Wood) ;

                    }

                }

                //生成树叶
                BuildLeaves(random_x, random_y + random_Tree_High, random_z);

            }


            //Debug.Log($"{random_x}, {random_y}, {random_z}");
        }

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

        if (isSaving)
        {
            LoadData(EditList);
        }

        //交给world来create
        world.WaitToCreateMesh.Enqueue(this);

    }


    //密林群系
    void CreateData_Forest()
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
                    float noiseHigh = world.GetTotalNoiseHigh_Biome(x, z, myposition, world.worldSetting.worldtype);


                    //矿洞噪声
                    float noise3d = GetCaveNoise(x, y, z);

                    float _sealevel = 59;


                    //数据缓冲
                    byte terrainFace = GetBlock(x, y - 1, z).voxelType;

                    //沙漠噪声
                    //float noise_desery = GetSmoothNoise_Desert(x, z);


                    //判断基岩
                    //0~3层不准生成矿洞
                    if (y >= 0 && y <= 3)
                    {

                        if (y == 0)
                        {

                            UpdateBlock(x, y, z, VoxelData.BedRock) ;

                        }
                        else if (y > 0 && y < 3 && GetProbability(50))
                        {

                            UpdateBlock(x, y, z, VoxelData.BedRock) ;

                        }
                        else
                        {

                            UpdateBlock(x, y, z, VoxelData.Stone) ;

                        }
                    }
                    //空气部分
                    else if (y > noiseHigh && y > _sealevel)
                    {

                        //地上一层
                        if (y - 1 < noiseHigh)
                        {

                            //草地层
                            if (terrainFace != VoxelData.Sand && terrainFace != VoxelData.Air && terrainFace != VoxelData.Snow)
                            {

                                //灌木丛
                                if (GetProbability(world.terrainLayerProbabilitySystem.Random_Bush))
                                {

                                    UpdateBlock(x, y, z, VoxelData.Bush) ;

                                }
                                //BlueFlower
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_BlueFlower))
                                {

                                    UpdateBlock(x, y, z, VoxelData.BlueFlower) ;

                                }
                                //WhiteFlower_1
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_WhiteFlower1))
                                {

                                    UpdateBlock(x, y, z, VoxelData.WhiteFlower_1) ;

                                }
                                //WhiteFlower_2
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_WhiteFlower2))
                                {

                                    UpdateBlock(x, y, z, VoxelData.WhiteFlower_2) ;

                                }
                                //YellowFlower
                                else if (GetProbability(world.terrainLayerProbabilitySystem.Random_YellowFlower))
                                {

                                    UpdateBlock(x, y, z, VoxelData.YellowFlower) ;

                                }
                                //草地雪碎片
                                else if (y > world.terrainLayerProbabilitySystem.Snow_Level - 10)
                                {
                                    UpdateBlock(x, y, z, VoxelData.SnowPower) ;
                                }
                                else
                                {

                                    UpdateBlock(x, y, z, VoxelData.Air) ;

                                }
                            }

                            //雪地层概率生成雪碎片
                            else if (terrainFace == VoxelData.Snow && GetProbability(50))
                            {
                                UpdateBlock(x, y, z, VoxelData.SnowPower) ;
                            }

                            else
                            {

                                UpdateBlock(x, y, z, VoxelData.Air) ;

                            }
                        }

                        else
                        {

                            UpdateBlock(x, y, z, VoxelData.Air) ;

                        }
                    }

                    //判断水面
                    else if (y > noiseHigh && y - 1 < _sealevel)
                    {

                        UpdateBlock(x, y, z, VoxelData.Water) ;

                    }

                    //空气之下
                    else
                    {

                        //地表
                        if ((y + 1) > noiseHigh)
                        {

                            //100雪地
                            if (y > world.terrainLayerProbabilitySystem.Snow_Level)
                            {
                                UpdateBlock(x, y, z, VoxelData.Snow) ;
                            }

                            //90~100概率生成雪地
                            else if ((y > (world.terrainLayerProbabilitySystem.Snow_Level - 10f)) && GetProbability(70))
                            {
                                UpdateBlock(x, y, z, VoxelData.Snow) ;
                            }



                            //高于海平面
                            else if (y > _sealevel)
                            {

                                UpdateBlock(x, y, z, VoxelData.Grass) ;

                            }
                            else
                            {

                                if (world.GetSimpleNoiseWithOffset(x, z, myposition, new Vector2(111f, 222f), 0.1f) > 0.5f)
                                {

                                    UpdateBlock(x, y, z, VoxelData.Sand) ;

                                }
                                else
                                {

                                    UpdateBlock(x, y, z, VoxelData.Soil) ;

                                }

                            }
                        }


                        //泥土的判断
                        else if (y > noiseHigh - 7)
                        {
                            //沙漠判断
                            if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Dessert)
                            {
                                UpdateBlock(x, y, z, VoxelData.Sand) ;
                            }
                            else
                            {
                                UpdateBlock(x, y, z, VoxelData.Soil) ;
                            }


                        }
                        else if (y >= (noiseHigh - 10) && y <= (noiseHigh - 7) && GetProbability(50))
                        {
                            //沙漠判断
                            if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Dessert)
                            {
                                UpdateBlock(x, y, z, VoxelData.Sand) ;
                            }
                            else
                            {
                                UpdateBlock(x, y, z, VoxelData.Soil) ;
                            }


                        }





                        //矿洞
                        else if (noise3d < GetVaveWidth(y))
                        {

                            UpdateBlock(x, y, z, VoxelData.Air) ;

                        }

                        //地下判断
                        else
                        {

                            //煤炭
                            if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Coal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Stone) ;
                                Coals.Enqueue(new Vector3(x, y, z));

                            }

                            //铁
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Iron))
                            {

                                UpdateBlock(x, y, z, VoxelData.Iron) ;

                            }

                            //金
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Gold))
                            {

                                UpdateBlock(x, y, z, VoxelData.Gold) ;

                            }

                            //青金石
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Blue_Crystal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Blue_Crystal) ;

                            }

                            //钻石
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Diamond))
                            {

                                UpdateBlock(x, y, z, VoxelData.Diamond) ;

                            }

                            else
                            {

                                UpdateBlock(x, y, z, VoxelData.Stone) ; 

                            }
                        }
                    }
                }
            }
        }


        //补充树
        //[确定XZ]xoz上随便选择5个点
        int _Forest_treecount = 25;
        while (_Forest_treecount-- != 0)
        {

            int random_x = rand.Next(2, VoxelData.ChunkWidth - 2);
            int random_z = rand.Next(2, VoxelData.ChunkWidth - 2);
            int random_y = VoxelData.ChunkHeight;
            int random_Tree_High = rand.Next(7, 15);

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

                        UpdateBlock(random_x, random_y + i, random_z, VoxelData.Wood) ;

                    }

                }

                //生成树叶
                BuildLeaves(random_x, random_y + random_Tree_High, random_z);

            }


            //Debug.Log($"{random_x}, {random_y}, {random_z}");
        }

        //补充煤炭
        foreach (var item in Coals)
        {

            CreateCoal((int)item.x, (int)item.y, (int)item.z);

        }

        ////补充竹子
        //foreach (var item in Bamboos)
        //{

        //    CreateBamboo((int)item.x, (int)item.y, (int)item.z);

        //}

        if (isSaving)
        {
            LoadData(EditList);
        }

        //交给world来create
        world.WaitToCreateMesh.Enqueue(this);

    }



    //---------------------------------- Tree ----------------------------------------

    //tree
    void CreateTree(int _x, int _z)
    {
        //密林群系
        if (world.GetBiomeType(_x, _z, myposition) == VoxelData.Biome_Forest)
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

                            UpdateBlock(random_x, random_y + i, random_z, VoxelData.Wood) ;

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

                            UpdateBlock(random_x, random_y + i, random_z, VoxelData.Wood) ;

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
            if (((_y + 1) >= world.terrainLayerProbabilitySystem.Snow_Level - 10f) && ((_y + 2) < VoxelData.ChunkHeight))
            {
                UpdateBlock(_x, _y + 2, _z, VoxelData.SnowPower) ;
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
            if (((_y + 1) >= world.terrainLayerProbabilitySystem.Snow_Level - 10f) && ((_y + 2) < VoxelData.ChunkHeight))
            {
                UpdateBlock(_x, _y + 2, _z + 1, VoxelData.SnowPower);
                UpdateBlock(_x - 1, _y + 2, _z, VoxelData.SnowPower);
                UpdateBlock(_x, _y + 2, _z, VoxelData.SnowPower);
                UpdateBlock(_x + 1, _y + 2, _z, VoxelData.SnowPower);
                UpdateBlock(_x, _y + 2, _z - 1, VoxelData.SnowPower);
            }

        }

        //第二层
        CreateLeaves(_x - 1, _y, _z);
        CreateLeaves(_x + 1, _y, _z);
        CreateLeaves(_x, _y, _z - 1);
        CreateLeaves(_x, _y, _z + 1);

        //生成雪的判定
        if (((_y) >= world.terrainLayerProbabilitySystem.Snow_Level - 10f) && ((_y + 1) < VoxelData.ChunkHeight) && GetBlock(_x - 1, _y + 1, _z).voxelType != VoxelData.Leaves)
        {

            UpdateBlock(_x - 1, _y + 1, _z, VoxelData.SnowPower);
            UpdateBlock(_x + 1, _y + 1, _z, VoxelData.SnowPower);
            UpdateBlock(_x, _y + 1, _z - 1, VoxelData.SnowPower);
            UpdateBlock(_x, _y + 1, _z + 1, VoxelData.SnowPower);

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
            UpdateBlock(_x, _y, _z + 2, VoxelData.SnowPower);
            UpdateBlock(_x - 1, _y, _z + 2, VoxelData.SnowPower);
            UpdateBlock(_x + 1, _y, _z + 2, VoxelData.SnowPower);
            UpdateBlock(_x - 2, _y, _z + 1, VoxelData.SnowPower);
            UpdateBlock(_x - 1, _y, _z + 1, VoxelData.SnowPower);
            UpdateBlock(_x + 1, _y, _z + 1, VoxelData.SnowPower);
            UpdateBlock(_x + 2, _y, _z + 1, VoxelData.SnowPower);
            UpdateBlock(_x - 2, _y, _z, VoxelData.SnowPower);
            UpdateBlock(_x + 2, _y, _z, VoxelData.SnowPower);
            UpdateBlock(_x - 2, _y, _z - 1, VoxelData.SnowPower);
            UpdateBlock(_x - 1, _y, _z - 1, VoxelData.SnowPower);
            UpdateBlock(_x + 1, _y, _z - 1, VoxelData.SnowPower);
            UpdateBlock(_x + 2, _y, _z - 1, VoxelData.SnowPower);
            UpdateBlock(_x - 1, _y, _z - 2,VoxelData.SnowPower);
            UpdateBlock(_x, _y, _z - 2,VoxelData.SnowPower);
            UpdateBlock(_x + 1, _y, _z - 2,VoxelData.SnowPower);

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
                if (GetBlock(_x, _y - 1, _z).voxelType != VoxelData.Air)
                {

                    if (GetBlock(_x, _y - 1, _z).voxelType == VoxelData.Mycelium || GetBlock(_x, _y - 1, _z).voxelType == VoxelData.Grass || GetBlock(_x, _y - 1, _z).voxelType == VoxelData.Soil && GetBlock(_x, _y - 2, _z).voxelType != VoxelData.Leaves)
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
        if (GetBlock(x, y, z).voxelType != VoxelData.Air)
        {

            return;

        }
        else
        {

            UpdateBlock(x, y, z, VoxelData.Leaves);

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
        if (GetBlock(_x, _y, _z).voxelType == VoxelData.Stone)
            UpdateBlock(_x, _y, _z, VoxelData.Coal);


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

                GetBlock(x, y + temp, z).voxelType = VoxelData.Bamboo;

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

                if (GetBlock(_x, y - 1, _z).voxelType == VoxelData.Water)
                {

                    return true;

                }

            }

        }

        return false;

    }



    //[水的流动]
    //如果自己是水
    //前后左右下如果是空气，则把他们变成水
    //public void Always_updateWater()
    //{

    //    //待优化 - 如果先判断周围一圈都是水，则不执行检查直接跳过

    //    ClearMeshData();

    //    //刷新自己
    //    for (int _y = 0; _y < VoxelData.ChunkHeight; _y++)
    //    {

    //        for (int _x = 0; _x < VoxelData.ChunkWidth; _x++)
    //        {

    //            for (int _z = 0; _z < VoxelData.ChunkWidth; _z++)
    //            {

    //                //修改VoxelMap
    //                if (hasExec_isHadupdateWater)
    //                    _updateWater(_x, _y, _z);

    //                // 非空气 - 渲染
    //                // 水面上 - 渲染
    //                if (world.blocktypes[GetBlock(_x, _y, _z).voxelType].DrawMode != DrawMode.Air)
    //                    UpdateMeshData(new Vector3(_x, _y, _z));

    //            }

    //        }

    //    }



    //    hasExec_isHadupdateWater = true;

    //    //添加到world的渲染队列
    //    isReadyToRender = true;

    //    //交给渲染线程
    //    if (world.RenderLock)
    //    {

    //        world.WaitToRender_temp.Enqueue(this);
    //        //print($"{world.GetChunkLocation(myposition)}被堵塞，入队temp");

    //    }
    //    else
    //    {

    //        //print($"{world.GetChunkLocation(myposition)}入队");
    //        world.WaitToRender.Enqueue(this);

    //    }

    //}


    //void _updateWater(int _x, int _y,int _z)
    //{

    //    //只在自己是水的情况下执行
    //    if (GetBlock(_x, _y, _z).voxelType == VoxelData.Water)
    //    {

    //        //检查五个方向
    //        for (int _p = 0; _p < 5; _p++)
    //        {

    //            //如果出界
    //            if (isOutOfRange(new Vector3(_x, _y, _z) + VoxelData.faceChecks_WaterFlow[_p]))
    //            {

    //                //能获取到对面Chunk
    //                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks_WaterFlow[_p], out Chunk chunktemp))
    //                {

    //                    Vector3 directlocation = GetDirectChunkVoxelMapLocation(new Vector3(_x, _y, _z) + VoxelData.faceChecks_WaterFlow[_p]);

    //                    if (chunktemp.GetBlock((int)directlocation.x, (int)directlocation.y, (int)directlocation.z).voxelType == VoxelData.Air)
    //                    {

    //                        chunktemp.UpdateBlock((int)directlocation.x, (int)directlocation.y, (int)directlocation.z).voxelType = VoxelData.Water;

    //                        //给不含水区块变成含水区块
    //                        if (chunktemp.iHaveWater == false)
    //                        {

    //                            chunktemp.iHaveWater = true;

    //                        }

    //                        hasExec_isHadupdateWater = false;

    //                    }

    //                }


    //            }

    //            //没出界
    //            else
    //            {

    //                if (GetBlock(_x + (int)VoxelData.faceChecks_WaterFlow[_p].x, _y + (int)VoxelData.faceChecks_WaterFlow[_p].y, _z + (int)VoxelData.faceChecks_WaterFlow[_p].z).voxelType == VoxelData.Air)
    //                {

    //                    UpdateBlock(_x + (int)VoxelData.faceChecks_WaterFlow[_p].x, _y + (int)VoxelData.faceChecks_WaterFlow[_p].y, _z + (int)VoxelData.faceChecks_WaterFlow[_p].z).voxelType = VoxelData.Water;

    //                    hasExec_isHadupdateWater = false;

    //                }

    //            }


    //        }

    //    }

    //}




    //特殊方块变化
    void updateSomeBlocks(int _x, int _y,int _z)
    {

        //出界判断
        //不能浮空的方块(灌木丛 + 竹子 + 细雪) 
        if (GetBlock(_x, _y - 1, _z).voxelType == VoxelData.Air)
        {
            //且自己是不能悬空的方块
            if (GetBlock(_x, _y, _z).voxelType == VoxelData.Bush || GetBlock(_x, _y, _z).voxelType == VoxelData.Bamboo || GetBlock(_x, _y, _z).voxelType == VoxelData.SnowPower)
            {

                UpdateBlock(_x, _y, _z, VoxelData.Air);
            }

            //门
            if (GetBlock(_x, _y, _z).voxelType == VoxelData.Door_Down)
            {

                UpdateBlock(_x, _y, _z, VoxelData.Air);
                UpdateBlock(_x, _y + 1, _z, VoxelData.Air);
            }
        }

        //上门
        if (GetBlock(_x, _y, _z).voxelType == VoxelData.Door_Up && GetBlock(_x, _y - 1, _z).voxelType != VoxelData.Door_Down)
        {
            UpdateBlock(_x, _y, _z, VoxelData.Air);
        }

        //下门
        if (GetBlock(_x, _y, _z).voxelType == VoxelData.Door_Down && GetBlock(_x, _y + 1, _z).voxelType == VoxelData.Air)
        {

            UpdateBlock(_x, _y + 1, _z, VoxelData.Door_Up);
        }




    }

    //放置或破坏时会变化的特殊方块
    public void EditForSomeBlocks(Vector3 _pos, byte _target)
    {
        //如果打掉上门，则两个都会消失
        if (_target == VoxelData.Air && GetBlock((int)_pos.x, (int)_pos.y - 1, (int)_pos.z).voxelType == VoxelData.Door_Down)
        {
            UpdateBlock((int)_pos.x, (int)_pos.y - 1, (int)_pos.z, VoxelData.Air);
        }

        //两个半砖合成一个木板
        //if (_target == VoxelData.HalfBrick && voxelMap[(int)_pos.x, (int)_pos.y - 1, (int)_pos.z].voxelType == VoxelData.HalfBrick)
        //{
        //    voxelMap[(int)_pos.x, (int)_pos.y, (int)_pos.z].voxelType = VoxelData.Air;
        //    voxelMap[(int)_pos.x, (int)_pos.y - 1, (int)_pos.z].voxelType = VoxelData.WoodenPlanks;
        //}
    }

    //------------------------------------------------------------------------------------






    //---------------------------------- Mesh部分 ----------------------------------------

    //开始遍历 
    public void UpdateChunkMesh_WithSurround() //参数重载，如果不写默认为false
    {

        UpdateChunkMesh_WithSurround(false, false);

    }


    //参数1：true会呼叫周边区块
    //参数2：true不会使用多线程
    public void UpdateChunkMesh_WithSurround(object obj, bool NotNeedThreading)
    {

        bool iscaller = (bool)obj;

        ClearMeshData();

        //刷新自己
        for (int _y = 0; _y < VoxelData.ChunkHeight; _y++)
        {

            for (int _x = 0; _x < VoxelData.ChunkWidth; _x++)
            {

                for (int _z = 0; _z < VoxelData.ChunkWidth; _z++)
                {

                    //会变化的特殊方块
                    updateSomeBlocks(_x, _y, _z);

                    //[已废弃，移动至单独的线程执行]水的流动
                    //updateWater();

                    //if (isOutOfRange(x,y,z))
                    //{
                    //    print("");
                    //}

                    //如果是空气则不渲染
                    if (world.blocktypes[GetBlock(_x, _y, _z).voxelType].DrawMode != DrawMode.Air)
                    {
                        UpdateMeshData(new Vector3(_x, _y, _z));
                    }
                        



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


        if (NotNeedThreading)
        {
            CreateMesh();
        }
        else
        {
            
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
        


        if (BaseChunk == true)
        {

            BaseChunk = false;

        }






    }


    //Player使用，可以省去呼叫周边方块更新的个数
    public void UpdateChunkMesh_WithSurround(Vector3 _pos, object obj, bool NotNeedThreading)
    {

        bool iscaller = (bool)obj;

        ClearMeshData();

        //刷新自己
        for (int _y = 0; _y < VoxelData.ChunkHeight; _y++)
        {

            for (int _x = 0; _x < VoxelData.ChunkWidth; _x++)
            {

                for (int _z = 0; _z < VoxelData.ChunkWidth; _z++)
                {

                    //会变化的特殊方块
                    updateSomeBlocks(_x, _y, _z);

                    //[已废弃，移动至单独的线程执行]水的流动
                    //updateWater();

                    //if (isOutOfRange(x,y,z))
                    //{
                    //    print("");
                    //}

                    //如果是空气则不渲染
                    if (world.blocktypes[GetBlock(_x, _y, _z).voxelType].DrawMode != DrawMode.Air)
                    {
                        UpdateMeshData(new Vector3(_x, _y, _z));
                    }




                    //UpdateMeshData(new Vector3(x, y, z));
                }

            }

        }


        //通知周围方块刷新
        if (iscaller)
        {
            Chunk DirectChunk;
            if (isOnEdge((int)_pos.x, (int)_pos.y, (int)_pos.z, out Vector3 Orient))
            {
                //print(Orient);
                //print(_pos);

                if (Orient.x > 0)
                {
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + new Vector3(1.0f, 0.0f, 0.0f), out DirectChunk))
                    {
                        world.WaitToFlashChunkQueue.Enqueue(DirectChunk);
                        //DirectChunk.UpdateChunkMesh_WithSurround();
                    }
                }

                if (Orient.x < 0)
                {
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + new Vector3(-1.0f, 0.0f, 0.0f), out DirectChunk))
                    {
                        world.WaitToFlashChunkQueue.Enqueue(DirectChunk);
                        //DirectChunk.UpdateChunkMesh_WithSurround();
                    }
                }

                if (Orient.z > 0)
                {
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + new Vector3(0.0f, 0.0f, 1.0f), out DirectChunk))
                    {
                        world.WaitToFlashChunkQueue.Enqueue(DirectChunk);
                        //DirectChunk.UpdateChunkMesh_WithSurround();
                    }
                }

                if (Orient.z < 0)
                {
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + new Vector3(0.0f, 0.0f, -1.0f), out DirectChunk))
                    {
                        world.WaitToFlashChunkQueue.Enqueue(DirectChunk);
                        //DirectChunk.UpdateChunkMesh_WithSurround();
                    }
                }
            }

            

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


        if (NotNeedThreading)
        {
            CreateMesh();
        }
        else
        {

            if (iscaller)
            {
                world.WaitToFlashChunkQueue.Enqueue(this);
            }
            else
            {
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


    // 批量编辑方块
    public void LoadData(List<EditStruct> _EditList)
    {
        //print($"EditData:{_EditList.Count}");

        for (int i = 0; i < _EditList.Count; i++)
        {


            int x = Mathf.FloorToInt(_EditList[i].editPos.x);
            int y = Mathf.FloorToInt(_EditList[i].editPos.y);
            int z = Mathf.FloorToInt(_EditList[i].editPos.z);

            // 出界就跳过
            //if (isOutOfRange(x, y, z))
            //{
            //    continue;
            //}

            // 设置方块类型
            UpdateBlock(x, y, z, _EditList[i].targetType);
        }

        // 更新区块网格
        //UpdateChunkMesh_WithSurround(true, false);

        isSaving = false;
    }



    #region 修改方块

    //编辑方块
    //接收绝对坐标
    public void EditData(Vector3 pos, byte targetBlocktype) 
    {

        //ClearFInd_Direvtion();
        Vector3 _relaVec = world.GetRelalocation(pos);

        int x = (int)_relaVec.x;
        int y = (int)_relaVec.y;
        int z = (int)_relaVec.z;

        //防止过高
        if (y >= VoxelData.ChunkHeight - 2)
        {

            return;

        }

        //if (isOutOfRange(x,y,z))
        //{
        //    return;
        //}

        UpdateBlock(x, y, z, targetBlocktype);

        //判断朝向
        if (world.blocktypes[targetBlocktype].IsOriented)
        {
            UpdateBlockOriented(new Vector3(x, y, z), world.player.RealBacking);
        }

        EditForSomeBlocks(new Vector3(x, y, z), targetBlocktype);


        UpdateChunkMesh_WithSurround(pos, true, false);
    }

    
    public void EditData(List<EditStruct> _EditList) 
    {
        //print($"EditData:{_EditList.Count}");

        for (int i = 0; i < _EditList.Count; i++)
        {
            Vector3 relaposition = world.GetRelalocation(_EditList[i].editPos);

            int _x = Mathf.FloorToInt(relaposition.x);
            int _y = Mathf.FloorToInt(relaposition.y);
            int _z = Mathf.FloorToInt(relaposition.z);
            byte thisType = GetBlock(_x, _y, _z).voxelType;

            // 出界就跳过
            //if (isOutOfRange(_x, _y, _z))
            //{
            //    continue;
            //}

            //非本区块则跳过
            if (world.GetChunkLocation(_EditList[i].editPos) != world.GetChunkLocation(myposition))
            {
                //Debug.Log($"edit:{world.GetChunkLocation(_EditList[i].editPos)}, myposition:{world.GetChunkLocation(myposition)}");
                continue;
            }

            //基岩也跳过
            if (thisType == VoxelData.BedRock)
            {
                continue;
            }


            //掉落物
            if (thisType != VoxelData.Air && thisType != VoxelData.TNT && GetProbability(30))
            {
                if (thisType == VoxelData.Grass)
                {
                    managerhub.backpackManager.CreateDropBox(_EditList[i].editPos, VoxelData.Soil, false);
                }
                else
                {
                    managerhub.backpackManager.CreateDropBox(_EditList[i].editPos, thisType, false);
                }
                

            }
            

            // 设置方块类型 
            UpdateBlock(_x, _y, _z, _EditList[i].targetType);
        }

        // 更新区块网格
        UpdateChunkMesh_WithSurround(true, false);

    }





    #endregion




    //推送玩家更新的具体方块
    // 推送玩家更新的具体方块
    public void UpdateEditNumber(Vector3 RealPos, byte targetBlocktype)
    {
        // 将修改细节推送至World里
        // 转换RealPos为整型Vector3以便用作字典的key
        Vector3 intPos = new Vector3((int)RealPos.x, (int)RealPos.y, (int)RealPos.z);

        // 查找是否已经存在相同的editPos
        EditStruct existingEdit = world.EditNumber.Find(edit => edit.editPos == intPos);

        if (existingEdit != null)
        {
            // 如果存在，更新targetType
            existingEdit.targetType = targetBlocktype;
        }
        else
        {
            // 如果不存在，添加新的EditStruct
            world.EditNumber.Add(new EditStruct(intPos, targetBlocktype));
        }
    }



    //面生成的判断
    //是方块吗----Y不绘制----N绘制
    //靠近边界----也返回N
    bool CheckVoxel(Vector3 pos, int _p)
    {

        int _Targetx = Mathf.FloorToInt(pos.x);
        int _Targety = Mathf.FloorToInt(pos.y);
        int _Targetz = Mathf.FloorToInt(pos.z);


        
      //print($"{x},{y},{z}");


        //如果目标出界
        if (_Targetx < 0 || _Targetx > VoxelData.ChunkWidth - 1 || _Targety < 0 || _Targety > VoxelData.ChunkHeight - 1 || _Targetz < 0 || _Targetz > VoxelData.ChunkWidth - 1)
        {

            //if (ThisChunkLocation == new Vector3(100f,0f,99f))
            //{
            //	print("");
            //}

            if (!BaseChunk)
            {

                //Front
                if (_Targetz > VoxelData.ChunkWidth - 1)
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
                        return CheckSelfAndTarget(GetBlock(_Targetx, _Targety, _Targetz - 1).voxelType, chunktemp.GetBlock(_Targetx, _Targety, 0).voxelType, _p);


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

                        return !world.是否生成Chunk侧面;

                    }

                }


                //Back
                if (_Targetz < 0)
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


                        return CheckSelfAndTarget(GetBlock(_Targetx, _Targety, _Targetz + 1).voxelType, chunktemp.GetBlock(_Targetx, _Targety, VoxelData.ChunkWidth - 1).voxelType, _p);


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

                        return !world.是否生成Chunk侧面;

                    }


                }


                //Left
                if (_Targetx < 0)
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

                        return CheckSelfAndTarget(GetBlock(_Targetx + 1, _Targety, _Targetz).voxelType, chunktemp.GetBlock(VoxelData.ChunkWidth - 1, _Targety, _Targetz).voxelType, _p);

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

                        return !world.是否生成Chunk侧面;

                    }

                }


                //Right
                if (_Targetx > VoxelData.ChunkWidth - 1)
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

                        return CheckSelfAndTarget(GetBlock(_Targetx - 1, _Targety, _Targetz).voxelType, chunktemp.GetBlock(0, _Targety, _Targetz).voxelType, _p);

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

                        return !world.是否生成Chunk侧面;

                    }

                }

            }

            //Up不需要考虑

            //Down:最下层一律不绘制
            if (_Targety < 0)
            {

                return true;

            }

            //else:自己是不是空气
            if (GetBlock(_Targetx - (int)VoxelData.faceChecks[_p].x, _Targety - (int)VoxelData.faceChecks[_p].y, _Targetz - (int)VoxelData.faceChecks[_p].z).voxelType == VoxelData.Air || GetBlock(_Targetx - (int)VoxelData.faceChecks[_p].x, _Targety - (int)VoxelData.faceChecks[_p].y, _Targetz - (int)VoxelData.faceChecks[_p].z).voxelType == VoxelData.Water)
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

            byte _target = GetBlock(_Targetx, _Targety, _Targetz).voxelType;
            byte _self = GetBlock(_Targetx - (int)VoxelData.faceChecks[_p].x, _Targety - (int)VoxelData.faceChecks[_p].y, _Targetz - (int)VoxelData.faceChecks[_p].z).voxelType;

            //if (_target == VoxelData.Leaves && _self == VoxelData.Wood)
            //{

            //    print("");

            //}

            //GetBlock(x, y, z).voxelType
            //voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z].voxelType

            return CheckSelfAndTarget(_self, _target, _p);

            

            
        }




    }

    //接收自己和目标的类型，判断是否生成面
    //false：不生成
    //_targetRelativeY:1目标在上方，-1目标在下方，0为同一平面
    bool CheckSelfAndTarget(byte _self, byte _target, int _p)
    {

        //先检查需要检测的额外面
        if (world.blocktypes[_self].OtherFaceCheck.Count != 0)
        {
            int i = 0;
            foreach (FaceCheckMode temp in world.blocktypes[_self].OtherFaceCheck)
            {
                
                //检查方向
                if (temp.FaceDirect == _p)
                {
                    //print($"类型：{_self} , temp：{i}");
                    //检查类型
                    if (temp.checktype == FaceCheck_Enum.isSolid)
                    {
                        //print("a");
                        return !temp.isCreateFace;
                    }
                    else if (temp.checktype == FaceCheck_Enum.appointType)
                    {
                        //print("b");
                        if (_target == temp.appointType)
                        {
                            return !temp.isCreateFace;
                        }
                    }
                    else if (temp.checktype == FaceCheck_Enum.appointDrawmode)
                    {
                        //print("c");
                        if (world.blocktypes[_target].DrawMode == temp.appointDrawmode)
                        {
                            return !temp.isCreateFace;
                        } 
                    }
                    
                }
                i++;
            }
        }



        //半砖专区
        //if (_self == VoxelData.SnowPower && world.blocktypes[_target].isSolid)
        //{
        //    //如果固体在自己上方则生成，其他不生成
        //    if (_targetRelativeY == 1)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}
        ////目标是半砖
        //if (_target == VoxelData.SnowPower)
        //{
        //    //如果自己是半砖
        //    if (_self == VoxelData.SnowPower)
        //    {
        //        //四周是自己时不生成，其他生成
        //        if (Mathf.Abs(_targetRelativeY) != 1)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    //如果自己是固体
        //    else if (world.blocktypes[_self].isSolid)
        //    {
        //        //上方是半砖时不生成, 其他生成
        //        if (_targetRelativeY == 1)
        //        {
        //            return false;

        //        }
        //        else
        //        {
        //            return true;
        //        }

        //    }
        //    //其他都不生成
        //    else
        //    {
        //        return true;
        //    }
        //}

        //通用判断
        //判断是不是透明方块
        if (world.blocktypes[_target].isTransparent || _target == VoxelData.Air)
        {

            return false;

        }

        //这一段得在空气上面，水面接触空气也要生成面
        if (_target == VoxelData.Water && _self == VoxelData.Air)
        {
            return false;
        }

        if (world.blocktypes[_target].DrawMode == world.blocktypes[_self].DrawMode)
        {
            return true;
        }

        

        if (_target == VoxelData.SnowPower && _self != VoxelData.SnowPower)
        {
            return false;
        }

        if (_target == VoxelData.Water && _self != VoxelData.Water)
        {
            return false; 
        }

        if (_self == _target)
        {

            return true;

        }
        if (_self == VoxelData.Water && world.blocktypes[_target].isSolid)
        {
            return true;
        }

        //if (_target == VoxelData.Air || world.blocktypes[_target].isTransparent)
        //{
        //    return false;
        //}

        //if (_target == VoxelData.Water)
        //{
        //    if (_self == VoxelData.Water)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
        //if (world.blocktypes[_target].isSolid)
        //{ 
        //    return true;
        //}


        //print("生成了奇怪的面");
        return world.blocktypes[_self].isSolid;

        // 如果目标是空气或者透明，无论如何生成面
        //if (world.blocktypes[_target].isTransparent || _target == VoxelData.Air)
        //{

        //    return false;

        //}

        ////如果自己和目标相等，不生成
        //if (_self == _target)
        //{
        //    return true;
        //}

        ////如果自己是水
        //// 目标是Transparent，生成
        ////目标是水或者固体，不生成
        //if (_self == VoxelData.Water)
        //{

        //    if (world.blocktypes[_target].isSolid)
        //    {

        //        return true;

        //    }

        //}

        ////如果自己是雪碎片
        //if (_self == VoxelData.SnowPower)
        //{


        //    //目标是水，生成
        //    //目标是transparent，生成
        //    if (world.blocktypes[_target].isTransparent || _target == VoxelData.Water || _target == _self)
        //    {
        //        return true;
        //    }

        //    //目标是固体，不生成
        //    if (world.blocktypes[_target].isSolid)
        //    {
        //        return false;
        //    }



        //}

        ////如果自己是固体
        ////目标是固体，则不生成
        ////其他都生成
        //if (world.blocktypes[_self].isSolid)
        //{

        //    return false;

        //}



        //return true;

    }




    //遍历中：：顺带判断面的生成方向
    //创建mesh里的参数
    //_calledFrom = 0 来自UpdateChunkMesh_WithSurround
    //_calledFrom = 1 来自UpdateWater
    void UpdateMeshData(Vector3 pos)
    {

        //if (isOutOfRange(pos))
        //{
        //    print($"UpdateMeshData出界，pos = {pos}");
        //    return;
        //}

        int _x = (int)pos.x;
        int _y = (int)pos.y;
        int _z = (int)pos.z;

        byte blockID = GetBlock(_x, _y, _z).voxelType;


        //方块绘制模式
        switch (world.blocktypes[blockID].DrawMode)
        {
            // Bush绘制模式
            case DrawMode.Bush:

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


            // SnowPower绘制模式
            case DrawMode.SnowPower:

                //判断六个面
                for (int p = 0; p < 6; p++)
                {


                    if (!CheckVoxel(pos + VoxelData.faceChecks[p], p))
                    {

                        vertices.Add(pos + VoxelData.voxelVerts_SnowPower[VoxelData.voxelTris[p, 0]]);
                        vertices.Add(pos + VoxelData.voxelVerts_SnowPower[VoxelData.voxelTris[p, 1]]);
                        vertices.Add(pos + VoxelData.voxelVerts_SnowPower[VoxelData.voxelTris[p, 2]]);
                        vertices.Add(pos + VoxelData.voxelVerts_SnowPower[VoxelData.voxelTris[p, 3]]);

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


            //水面绘制模式
            case DrawMode.Water: 



                //判断六个面
                for (int p = 0; p < 6; p++)
                {

                    //可以绘制
                    if (!CheckVoxel(pos + VoxelData.faceChecks[p], p))
                    {

                        //如果上下方有水，则换成方块的渲染方式
                        if ((GetBlock((int)pos.x, (int)pos.y + 1, (int)pos.z).voxelType == VoxelData.Water || GetBlock((int)pos.x, (int)pos.y - 1, (int)pos.z).voxelType == VoxelData.Water) && p != 2  && GetBlock((int)pos.x, (int)pos.y + 1, (int)pos.z).voxelType != VoxelData.Air)
                        {
                            //如果需要双面绘制
                            if (world.blocktypes[GetBlock(pos).voxelType].GenerateTwoFaceWithAir && GetBlock(pos, p).voxelType == VoxelData.Air)
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

                                triangles_Water.Add(vertexIndex);
                                triangles_Water.Add(vertexIndex + 2);
                                triangles_Water.Add(vertexIndex + 1);
                                triangles_Water.Add(vertexIndex + 3);
                                triangles_Water.Add(vertexIndex + 1);
                                triangles_Water.Add(vertexIndex + 2);
                                vertexIndex += 4;

                                
                            }

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

                        //如果是水面
                        else if (GetBlock(_x, _y, _z).voxelType == VoxelData.Water && GetBlock(_x, _y, _z, 2).voxelType == VoxelData.Air)
                        {
                            //面融合算法
                            if (p == 2 && GetBlock(_x, _y, _z).up == true)
                            {
                                int _zz = 0;
                                int _xx = 0;

                                //if (x == 15 && z == 9)
                                //{
                                //    print("");
                                //}


                                //局部变量
                                int __z = 0;
                                bool Z排碰到障碍物 = false;

                                //运行后获得当前方块可以踏步的范围
                                for (_xx = 0; _xx < VoxelData.ChunkWidth; _xx++)
                                {
                                    for (__z = 0; __z < VoxelData.ChunkWidth; __z++)
                                    {




                                        //如果出界则停止
                                        if (isOutOfRange(_x + _xx, _y, _z + __z))
                                        {
                                            break;
                                        }

                                        //if(_xx == 8){
                                        //    print("");
                                        //} 


                                        //目标是水 && 目标上方是空气 && 目标的up是true
                                        if ((GetBlock(_x + _xx, _y, _z + __z).voxelType != VoxelData.Water || GetBlock(_x + _xx, _y + 1, _z + __z).voxelType != VoxelData.Air) || (GetBlock(_x + _xx, _y, _z + __z).up == false))
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
                                                UpdateBlockDirection(new Vector3(_x + _xx, _y, _z + i), 2 ,false);
                                            }

                                        }


                                        break;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < __z; i++)
                                        {
                                            UpdateBlockDirection(new Vector3(_x + _xx, _y, _z + i), 2, false);
                                        }

                                        //如果出界则停止
                                        if (_x + _xx == VoxelData.ChunkWidth)
                                        {
                                            break;
                                        }

                                    }



                                }

                                //print($"local：{new Vector3(x, y, z)},长和宽：<{_xx},{_zz}>");


                                //最后应该给定一个长和宽 _xx与_zz



                                //防止顶点乘上0
                                if (_xx == 0)
                                {

                                    //int _x = Mathf.FloorToInt((pos + VoxelData.faceChecks[p]).x);
                                    //int _y = Mathf.FloorToInt((pos + VoxelData.faceChecks[p]).y);
                                    //int _z = Mathf.FloorToInt((pos + VoxelData.faceChecks[p]).z);

                                    //如果需要双面绘制
                                    if (world.blocktypes[GetBlock(pos).voxelType].GenerateTwoFaceWithAir && GetBlock(pos, p).voxelType == VoxelData.Air)
                                    {
                                        print("绘制了");
                                        vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 0]]);
                                        vertices.Add(pos + world.ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 1]], new Vector3(1, 1, _zz)));
                                        vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 2]]);
                                        vertices.Add(pos + world.ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 3]], new Vector3(1, 1, _zz)));

                                        triangles_Water.Add(vertexIndex);
                                        triangles_Water.Add(vertexIndex + 2);
                                        triangles_Water.Add(vertexIndex + 1);
                                        triangles_Water.Add(vertexIndex + 3);
                                        triangles_Water.Add(vertexIndex + 1);
                                        triangles_Water.Add(vertexIndex + 2);
                                        vertexIndex += 4;

                                        //根据p生成对应的面，对应的UV
                                        //AddTexture(world.blocktypes[blockID].GetTextureID(p));
                                        uvs.Add(new Vector2(0f, 0f));
                                        uvs.Add(world.ComponentwiseMultiply(new Vector2(0f, 1f), new Vector2(1, _zz)));
                                        uvs.Add(new Vector2(1f, 0f));
                                        uvs.Add(world.ComponentwiseMultiply(new Vector2(1f, 1f), new Vector2(1, _zz)));
                                    }
                                    else
                                    {
                                        print($"bool: {world.blocktypes[GetBlock(pos).voxelType].GenerateTwoFaceWithAir} , target: {GetBlock(pos, p).voxelType}");
                                    }

                                    vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 0]]);
                                    vertices.Add(pos + world.ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 1]], new Vector3(1, 1, _zz)));
                                    vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 2]]);
                                    vertices.Add(pos + world.ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 3]], new Vector3(1, 1, _zz)));


                                    //根据p生成对应的面，对应的UV
                                    //AddTexture(world.blocktypes[blockID].GetTextureID(p));
                                    uvs.Add(new Vector2(0f, 0f));
                                    uvs.Add(world.ComponentwiseMultiply(new Vector2(0f, 1f), new Vector2(1, _zz)));
                                    uvs.Add(new Vector2(1f, 0f));
                                    uvs.Add(world.ComponentwiseMultiply(new Vector2(1f, 1f), new Vector2(1, _zz)));

                                }
                                else
                                {
                                    //如果需要双面绘制
                                    if (world.blocktypes[GetBlock(pos).voxelType].GenerateTwoFaceWithAir && GetBlock(pos, p).voxelType == VoxelData.Air)
                                    {
                                        vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 0]]);
                                        vertices.Add(pos + world.ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 1]], new Vector3(1, 1, _zz)));
                                        vertices.Add(pos + world.ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 2]], new Vector3(_xx, 1, 1)));
                                        vertices.Add(pos + world.ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 3]], new Vector3(_xx, 1, _zz)));

                                        triangles_Water.Add(vertexIndex);
                                        triangles_Water.Add(vertexIndex + 2);
                                        triangles_Water.Add(vertexIndex + 1);
                                        triangles_Water.Add(vertexIndex + 3);
                                        triangles_Water.Add(vertexIndex + 1);
                                        triangles_Water.Add(vertexIndex + 2);
                                        vertexIndex += 4;

                                        //根据p生成对应的面，对应的UV
                                        //AddTexture(world.blocktypes[blockID].GetTextureID(p));
                                        uvs.Add(new Vector2(0f, 0f));
                                        uvs.Add(world.ComponentwiseMultiply(new Vector2(0f, 1f), new Vector2(1, _zz)));
                                        uvs.Add(world.ComponentwiseMultiply(new Vector2(1f, 0f), new Vector2(_xx, 1)));
                                        uvs.Add(world.ComponentwiseMultiply(new Vector2(1f, 1f), new Vector2(_xx, _zz)));
                                    }
                                    vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 0]]);
                                    vertices.Add(pos + world.ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 1]], new Vector3(1, 1, _zz)));
                                    vertices.Add(pos + world.ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 2]], new Vector3(_xx, 1, 1)));
                                    vertices.Add(pos + world.ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 3]], new Vector3(_xx, 1, _zz)));


                                    //根据p生成对应的面，对应的UV
                                    //AddTexture(world.blocktypes[blockID].GetTextureID(p));
                                    uvs.Add(new Vector2(0f, 0f));
                                    uvs.Add(world.ComponentwiseMultiply(new Vector2(0f, 1f), new Vector2(1, _zz)));
                                    uvs.Add(world.ComponentwiseMultiply(new Vector2(1f, 0f), new Vector2(_xx, 1)));
                                    uvs.Add(world.ComponentwiseMultiply(new Vector2(1f, 1f), new Vector2(_xx, _zz)));

                                }


                                triangles_Water.Add(vertexIndex);
                                triangles_Water.Add(vertexIndex + 1);
                                triangles_Water.Add(vertexIndex + 2);
                                triangles_Water.Add(vertexIndex + 2);
                                triangles_Water.Add(vertexIndex + 1);
                                triangles_Water.Add(vertexIndex + 3);
                                vertexIndex += 4;
                            }
                            else if(p != 2)
                            {
                                //如果需要双面绘制
                                if (world.blocktypes[GetBlock(pos).voxelType].GenerateTwoFaceWithAir && GetBlock(pos, p).voxelType == VoxelData.Air)
                                {
                                    vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 0]]);
                                    vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 1]]);
                                    vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 2]]);
                                    vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 3]]);

                                    //根据p生成对应的面，对应的UV
                                    //AddTexture(world.blocktypes[blockID].GetTextureID(p));
                                    uvs.Add(new Vector2(0f, 0f));
                                    uvs.Add(new Vector2(0f, 1f));
                                    uvs.Add(new Vector2(1f, 0f));
                                    uvs.Add(new Vector2(1f, 1f));

                                    triangles_Water.Add(vertexIndex);
                                    triangles_Water.Add(vertexIndex + 2);
                                    triangles_Water.Add(vertexIndex + 1);
                                    triangles_Water.Add(vertexIndex + 3);
                                    triangles_Water.Add(vertexIndex + 1);
                                    triangles_Water.Add(vertexIndex + 2);
                                    vertexIndex += 4;
                                }

                                vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 0]]);
                                vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 1]]);
                                vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 2]]);
                                vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 3]]);

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


                        else
                        {

                            //如果需要双面绘制
                            if (world.blocktypes[GetBlock(pos).voxelType].GenerateTwoFaceWithAir && GetBlock(pos, p).voxelType == VoxelData.Air)
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

                                triangles_Water.Add(vertexIndex);
                                triangles_Water.Add(vertexIndex + 2);
                                triangles_Water.Add(vertexIndex + 1);
                                triangles_Water.Add(vertexIndex + 3);
                                triangles_Water.Add(vertexIndex + 1);
                                triangles_Water.Add(vertexIndex + 2);
                                vertexIndex += 4;
                            }
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


                    }

                }





                break;


            //门绘制
            case DrawMode.Door:

                //判断六个面
                for (int p = 0; p < 6; p++)
                {


                    if (!CheckVoxel(pos + VoxelData.faceChecks[p], p))
                    {

                        vertices.Add(pos + VoxelData.voxelVerts_Door[VoxelData.voxelTris[p, 0]]);
                        vertices.Add(pos + VoxelData.voxelVerts_Door[VoxelData.voxelTris[p, 1]]);
                        vertices.Add(pos + VoxelData.voxelVerts_Door[VoxelData.voxelTris[p, 2]]);
                        vertices.Add(pos + VoxelData.voxelVerts_Door[VoxelData.voxelTris[p, 3]]);

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

            //半砖绘制
            case DrawMode.HalfBrick:

                //判断六个面
                for (int p = 0; p < 6; p++)
                {


                    if (!CheckVoxel(pos + VoxelData.faceChecks[p], p))
                    {

                        vertices.Add(pos + VoxelData.voxelVerts_HalfBrick[VoxelData.voxelTris[p, 0]]);
                        vertices.Add(pos + VoxelData.voxelVerts_HalfBrick[VoxelData.voxelTris[p, 1]]);
                        vertices.Add(pos + VoxelData.voxelVerts_HalfBrick[VoxelData.voxelTris[p, 2]]);
                        vertices.Add(pos + VoxelData.voxelVerts_HalfBrick[VoxelData.voxelTris[p, 3]]);

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



            //火把绘制
            case DrawMode.Torch:

                //判断六个面
                for (int p = 0; p < 6; p++)
                {


                    if (!CheckVoxel(pos + VoxelData.faceChecks[p], p))
                    {

                        vertices.Add(pos + VoxelData.voxelVerts_Torch[VoxelData.voxelTris[p, 0]]);
                        vertices.Add(pos + VoxelData.voxelVerts_Torch[VoxelData.voxelTris[p, 1]]);
                        vertices.Add(pos + VoxelData.voxelVerts_Torch[VoxelData.voxelTris[p, 2]]);
                        vertices.Add(pos + VoxelData.voxelVerts_Torch[VoxelData.voxelTris[p, 3]]);

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



            //默认Block绘制模式
            default: 



                //判断六个面
                for (int p = 0; p < 6; p++)
                {


                    if (!CheckVoxel(pos + VoxelData.faceChecks[p], p))
                    {

                        //如果需要双面绘制
                        if (world.blocktypes[GetBlock(pos).voxelType].GenerateTwoFaceWithAir && GetBlock(pos, p).voxelType == VoxelData.Air)
                        {
                            vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                            vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                            vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                            vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                            triangles.Add(vertexIndex);
                            triangles.Add(vertexIndex + 2);
                            triangles.Add(vertexIndex + 1);
                            triangles.Add(vertexIndex + 3);
                            triangles.Add(vertexIndex + 1);
                            triangles.Add(vertexIndex + 2);
                            vertexIndex += 4;

                            AddTexture(world.blocktypes[blockID].GetTextureID(ChangeBlockFacing(p, GetBlock(_x, _y, _z).blockOriented)));
                        }


                        vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                        vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                        vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                        vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                        //uvs.Add (VoxelData.voxelUvs [0]);
                        //uvs.Add (VoxelData.voxelUvs [1]);
                        //uvs.Add (VoxelData.voxelUvs [2]);
                        //uvs.Add (VoxelData.voxelUvs [3]);
                        //AddTexture(1);



                        triangles.Add(vertexIndex);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + 2);
                        triangles.Add(vertexIndex + 2);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + 3);
                        vertexIndex += 4;


                        //根据p生成对应的面，对应的UV
                        //ChangeBlockFacing：方块面的朝向
                        AddTexture(world.blocktypes[blockID].GetTextureID(ChangeBlockFacing(p, GetBlock(_x, _y, _z).blockOriented)));




                    }

                }

                break;

        }



    }


    public int ChangeBlockFacing(int _p, int _face)
    {
        // 前 - 后 - 左 - 右
        // 0  - 1  - 4  - 5

        //front
        //_face = 0
        // 0  - 1  - 4  - 5

        //back
        //_face = 1
        // 1  - 0  - 5  - 4

        //left
        //_face = 4
        // 5  - 4  - 0  - 1

        //right
        //_face = 5
        // 4  - 5  - 1  - 0
        return VoxelData.BlockOriented[_face, _p];

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

        InitializeVoxelMap_state();

    }

    public void InitializeVoxelMap_state()
    {
        // 遍历三维数组中的每一个元素
        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    // 只重置 VoxelStruct 的非voxelType部分
                    //voxelMap[x, y, z].up = true;
                    UpdateBlockDirection(new Vector3(x,y,z),2,true);
                    // 如果你有其他需要重置的方向或者属性，也可以在这里进行设置
                    // 例如： voxelMap[x, y, z].left = true;
                    // 例如： voxelMap[x, y, z].right = true;
                }
            }
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


        float x = (float)(textureID % 16) / 16;
        float y = (float)(15 - textureID / 16) / 16;

        //if (textureID == 35)
        //{
        //    print($"Bush: x = {x},y = {y}");
        //}

        float temp = 1f / 16;
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
    //修改Voxel，为内部用法，EditBlock为外部用法
    private void UpdateBlock(Vector3 _pos, byte _UpdateType)
    {
        int _x = Mathf.FloorToInt(_pos.x);
        int _y = Mathf.FloorToInt(_pos.y);
        int _z = Mathf.FloorToInt(_pos.z);


        //如果目标出界
        if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
        {

            print("UpdateBlockTypec参数出界！");

        }

        else
        {

            voxelMap[_x, _y, _z].voxelType = _UpdateType;
        }
    }

    private void UpdateBlock(int _x, int _y, int _z, byte _UpdateType)
    {

        //如果目标出界
        if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
        {

            //print("UpdateBlockTypec参数出界！");

        }

        else
        {

            voxelMap[_x, _y, _z].voxelType = _UpdateType;
        }
    }


    private void UpdateBlockOriented(Vector3 _pos, int _orient)
    {
        int _x = Mathf.FloorToInt(_pos.x);
        int _y = Mathf.FloorToInt(_pos.y);
        int _z = Mathf.FloorToInt(_pos.z);


        //如果目标出界
        if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
        {

            print("UpdateBlockTypec参数出界！");

        }

        else
        {

            voxelMap[_x, _y, _z].blockOriented = _orient;
        }
    }

    //UpdateBlockDirection(pos, 2, true)就是吧up改为true
    private void UpdateBlockDirection(Vector3 _pos, int _direct, bool _bool)
    {
        int _x = Mathf.FloorToInt(_pos.x);
        int _y = Mathf.FloorToInt(_pos.y);
        int _z = Mathf.FloorToInt(_pos.z);


        //如果目标出界
        if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
        {

            print("UpdateBlockTypec参数出界！");

        }

        else
        {
            switch (_direct)
            {
                case 2:
                    voxelMap[_x, _y, _z].up = _bool;
                    break;
            }
            
        }
    }




    //获得Voxel
    //不待方向的GetBlock要考虑到(-1,0,-1)非邻接访问，还有(0,0,-32)跨度极远的访问(虽然不可能)
    //获取目标Chunk的相对位置
    void GetRelativeTargetChunk(int _x, int _y, int _z, out Vector3 TargetChunkDirection, out Vector3 TargeChunkPosition)
    {
        TargetChunkDirection = Vector3.zero;
        TargeChunkPosition = new Vector3(_x, _y, _z);

        float width = VoxelData.ChunkWidth;

        //计算相对Chunk
        TargetChunkDirection += new Vector3((int)(_x / width), 0f, 0f);
        TargetChunkDirection += new Vector3(0f, 0f, (int)(_z / width));

        // 计算相对坐标
        if (_x < 0)
        {
            TargeChunkPosition.x = (_x % width + width) % width;
        }
        else if (_x > VoxelData.ChunkWidth - 1)
        {
            TargeChunkPosition.x = _x % width;
        }

        TargeChunkPosition.y = _y; // y 不变

        if (_z < 0)
        {
            TargeChunkPosition.z = (_z % width + width) % width;
        }
        else if (_z > VoxelData.ChunkWidth - 1)
        {
            TargeChunkPosition.z = _z % width;
        }


    }

    private VoxelStruct GetBlock(Vector3 _pos)
    {
        int _x = (int)_pos.x;
        int _y = (int)_pos.x;
        int _z = (int)_pos.x;

        //如果目标出界
        if (isOutOfRange(_x, _y, _z))
        {
            int _p;

            //Front
            if (_z > VoxelData.ChunkWidth - 1)
            {
                _p = 1;
                //如果能查到
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {
                    //if (isOutOfRange(_x,_y,_z))
                    //{
                    //    print(new Vector3(_x,_y,_z));
                    //}

                    return chunktemp.voxelMap[_x, _y, 0];
                }
                else
                {
                    //print("GetTargetBlockType()搜索不到目标Chunk");
                    return new VoxelStruct();

                }

            }

            //Back
            if (_z < 0)
            {
                _p = 0;
                //如果能查到
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {
                    return chunktemp.voxelMap[_x, _y, VoxelData.ChunkWidth - 1];

                }
                else
                {
                    //print("GetTargetBlockType()搜索不到目标Chunk");
                    return new VoxelStruct();

                }


            }

            //Left
            if (_x < 0)
            {
                _p = 4;
                //如果能查到
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {

                    return chunktemp.voxelMap[VoxelData.ChunkWidth - 1, _y, _z];

                }
                else
                {
                    //print("GetTargetBlockType()搜索不到目标Chunk");
                    return new VoxelStruct();

                }

            }

            //Right
            if (_x > VoxelData.ChunkWidth - 1)
            {
                _p = 5;
                //如果能查到
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {

                    return chunktemp.voxelMap[0, _y, _x];

                }
                else
                {
                    //print("GetTargetBlockType()搜索不到目标Chunk");
                    return new VoxelStruct();

                }

            }

            //Up不需要考虑
            //if (_y > VoxelData.ChunkHeight - 1)
            //{
            //    return new VoxelStruct();
            //}

            ////Down
            //if (_y < 0)
            //{

            //    return new VoxelStruct();

            //}

            //print("GetTargetBlockType()出界了但是全都找不到");
            return new VoxelStruct();

        }

        else
        {

            return voxelMap[_x, _y, _z];
        }

    }

    //重载
    private VoxelStruct GetBlock(int _x, int _y,int _z)
    {
        //如果目标出界
        if (isOutOfRange(_x, _y, _z))
        {
            //上下出界不用管
            if (_y < 0 || _y > VoxelData.ChunkWidth - 1)
            {
                return new VoxelStruct();
            }

            //获取目标Chunk的相对位置
            GetRelativeTargetChunk(_x, _y, _z, out Vector3 TargetChunkDirection, out Vector3 TargeChunkPosition);

            int _relaX = (int)TargeChunkPosition.x;
            int _relaY = (int)TargeChunkPosition.y;
            int _relaZ = (int)TargeChunkPosition.z;



            if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + TargetChunkDirection, out Chunk chunktemp))
            {
                if (isOutOfRange(_relaX, _relaY, _relaZ))
                {
                    print($"ThisPos: {new Vector3(_x, _y, _z)} , TargChuDire:{TargetChunkDirection}, relapos: {new Vector3(_relaX, _relaY, _relaZ)}");
                }
                return chunktemp.voxelMap[_relaX, _relaY, _relaZ];
            }
            else
            {
                return new VoxelStruct();
            }

        }

        else
        {

            return voxelMap[_x, _y, _z];
        }

    }



    //获得对面Voxel
    //待方向的GetBlock一般只有一层
    private VoxelStruct GetBlock(Vector3 _pos, int _p)
    {
        Vector3 _TargetPos = _pos + VoxelData.faceChecks[_p];
        int _x = (int)_TargetPos.x;
        int _y = (int)_TargetPos.y;
        int _z = (int)_TargetPos.z;

        //如果目标出界
        if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
        {
            //Front
            if (_z > VoxelData.ChunkWidth - 1)
            {

                //如果能查到
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {
                    return chunktemp.voxelMap[_x, _y, 0];
                }
                else
                {
                    //print("GetTargetBlockType()搜索不到目标Chunk");
                    return new VoxelStruct();

                }

            }

            //Back
            if (_z < 0)
            {

                //如果能查到
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {
                    return chunktemp.voxelMap[_x, _y, VoxelData.ChunkWidth - 1];

                }
                else
                {
                    //print("GetTargetBlockType()搜索不到目标Chunk");
                    return new VoxelStruct();

                }


            }

            //Left
            if (_x < 0)
            {

                //如果能查到
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {

                    return chunktemp.voxelMap[VoxelData.ChunkWidth - 1, _y, _z];

                }
                else
                {
                    //print("GetTargetBlockType()搜索不到目标Chunk");
                    return new VoxelStruct();

                }

            }

            //Right
            if (_x > VoxelData.ChunkWidth - 1)
            {

                //如果能查到
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {

                    return chunktemp.voxelMap[0, _y, _x];

                }
                else
                {
                    //print("GetTargetBlockType()搜索不到目标Chunk");
                    return new VoxelStruct();

                }

            }

            //Up不需要考虑
            //if (_y > VoxelData.ChunkHeight - 1)
            //{
            //    return new VoxelStruct();
            //}

            ////Down
            //if (_y < 0)
            //{

            //    return new VoxelStruct();

            //}

            print("GetTargetBlockType()出界了但是全都找不到");
            return new VoxelStruct();

        }

        else
        {
            
            return voxelMap[_x, _y, _z];
        }
    }

    //重载
    private VoxelStruct GetBlock(int _x, int _y, int _z, int _p)
    {

        //如果目标出界
        if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
        {
            //Front
            if (_z > VoxelData.ChunkWidth - 1)
            {

                //如果能查到
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {
                    return chunktemp.voxelMap[_x, _y, 0];
                }
                else
                {
                    //print("GetTargetBlockType()搜索不到目标Chunk");
                    return new VoxelStruct();

                }

            }

            //Back
            if (_z < 0)
            {

                //如果能查到
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {
                    return chunktemp.voxelMap[_x, _y, VoxelData.ChunkWidth - 1];

                }
                else
                {
                    //print("GetTargetBlockType()搜索不到目标Chunk");
                    return new VoxelStruct();

                }


            }

            //Left
            if (_x < 0)
            {

                //如果能查到
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {

                    return chunktemp.voxelMap[VoxelData.ChunkWidth - 1, _y, _z];

                }
                else
                {
                    //print("GetTargetBlockType()搜索不到目标Chunk");
                    return new VoxelStruct();

                }

            }

            //Right
            if (_x > VoxelData.ChunkWidth - 1)
            {

                //如果能查到
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {

                    return chunktemp.voxelMap[0, _y, _x];

                }
                else
                {
                    //print("GetTargetBlockType()搜索不到目标Chunk");
                    return new VoxelStruct();

                }

            }

           

            print("GetTargetBlockType()出界了但是全都找不到, upDown");
            return new VoxelStruct();

        }

        else
        {

            return voxelMap[_x, _y, _z];
        }
    }


    //--------------------------------------------------------------------------------------

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
    bool isOnEdge(int x, int y, int z, out Vector3 Orient)
    {
        Orient = Vector3.zero;
        bool _bool = false;

        if (x == 0)
        {
            Orient += new Vector3(-1f, 0f, 0f);
            _bool = true;

        }
        if (x == VoxelData.ChunkWidth - 1)
        {
            Orient += new Vector3(1f, 0f, 0f);
            _bool = true;

        }
        if (y == 0)
        {
            Orient += new Vector3(0f, -1f, 0f);
            _bool = true;

        }
        if (y == VoxelData.ChunkHeight - 1)
        {
            Orient += new Vector3(0f, 1f, 0f);
            _bool = true;

        }
        if (z == 0)
        {
            Orient += new Vector3(0f, 0f, -1f);
            _bool = true;

        }
        if (z == VoxelData.ChunkWidth - 1)
        {
            Orient += new Vector3(0f, 0f, 1f);
            _bool = true;

        }

        return _bool;



    }


    void InitVoxelStruct()
    {
        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = new VoxelStruct(); // 初始化每个元素
                }
            }
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




