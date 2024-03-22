//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Chunk : MonoBehaviour
{
    //state
    public bool isShow = true;
    public bool isReadyToRender = false;

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
    System.Random rand;
    Vector3 myposition;

    //---------------------------------- 周期函数 ---------------------------------------

    //Start()
    public Chunk(Vector3 thisPosition, World _world)
    {
        //World
        world = _world;
        noise2d_scale_smooth = world.noise2d_scale_smooth;
        noise2d_scale_steep = world.noise2d_scale_steep;
        noise3d_scale = world.noise3d_scale;
        treecount = world.TreeCount;
        caveWidth = world.cave_width;

        //Self
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = world.material;
        chunkObject.transform.SetParent(world.Chunks.transform);
        chunkObject.transform.position = new Vector3(thisPosition.x * VoxelData.ChunkWidth, 0f, thisPosition.z * VoxelData.ChunkWidth);
        chunkObject.name = thisPosition.x + "," + thisPosition.z;

        //Data线程
        rand = new System.Random(world.Seed);
        myposition = chunkObject.transform.position;
        Thread myThread = new Thread(new ThreadStart(CreateData));
        myThread.Start();
    }


    //-----------------------------------------------------------------------------------







    //---------------------------------- Data线程 ---------------------------------------
    
    //噪声生成器
    float GetTotalNoiseHigh(int _x, int _z)
    {
        //(平原-山脉)过度噪声
        float biome_moutainAndPlane = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((float)_x * 5e-05f + myposition.x * 5e-05f, (float)_z * 5e-05f + myposition.z * 5e-05f));
        
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

    //获取简单噪声
    float GetSmoothNoise_Tree()
    {
        float randomoffset = rand.Next(0, 10);
        float Offset_x = 100f * randomoffset;
        float Offset_z = 100f * randomoffset;

        float smoothNoise = Mathf.Lerp((float)0, (float)100, Mathf.PerlinNoise(  (myposition.x + Offset_x) * 0.005f,     (myposition.z + Offset_z) * 0.005f)   );
        
        return smoothNoise;
    }


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
                    else
                    {

                        //三个2d噪声
                        //float noise2d_1 = Mathf.Lerp((float)world.soil_min, (float)world.soil_max, Mathf.PerlinNoise((float)x * noise2d_scale_smooth + myposition.x * noise2d_scale_smooth, (float)z * noise2d_scale_smooth + myposition.z * noise2d_scale_smooth));
                        //float noise2d_2 = Mathf.Lerp((float)(world.soil_min), (float)world.soil_max, Mathf.PerlinNoise((float)x * noise2d_scale_steep + myposition.x * noise2d_scale_steep, (float)z * noise2d_scale_steep + myposition.z * noise2d_scale_steep));
                        //float noise2d_3 = Mathf.Lerp((float)(world.soil_min), (float)world.soil_max, Mathf.PerlinNoise((float)x * 0.1f + myposition.x * 0.1f, (float)z * 0.15f + myposition.z * 0.15f));

                        //噪声叠加
                        //float noiseHigh = noise2d_1 * 0.6f + noise2d_2 * 0.4f + noise2d_3 * 0.05f;
                        float noiseHigh = GetTotalNoiseHigh(x,z);

                        //空气部分
                        if (y > noiseHigh)
                        {

                            //如果y小于海平面则为水，否则为空气
                            if (y - 1 < world.sea_level)
                            {
                                voxelMap[x, y, z] = VoxelData.Water;
                            }
                            else
                            {

                                ////地表2层
                                //if (y - 1 < noiseHigh)
                                //{
                                //    //竹子
                                //    //if (rand.Next(0, world.Random_Bamboo) < 1)
                                //    //{
                                //    //    voxelMap[x, y, z] = VoxelData.Air;
                                //    //    Bamboos.Enqueue(new Vector3(x, y, z));
                                //    //}
                                //    //else
                                //    //{
                                //    //    voxelMap[x, y, z] = VoxelData.Air;
                                //    //}

                                //    voxelMap[x, y, z] = VoxelData.Air;

                                //}
                                //else
                                //{
                                //    voxelMap[x, y, z] = VoxelData.Air;
                                //}

                                voxelMap[x, y, z] = VoxelData.Air;

                            }



                        }//地表1层
                        else if ((y + 1) > noiseHigh)
                        {
                            if (y > world.sea_level)
                            {
                                voxelMap[x, y, z] = VoxelData.Grass;
                            }
                            else
                            {
                                voxelMap[x, y, z] = VoxelData.Sand;
                            }

                        }


                        //泥土的判断
                        else if (y > noiseHigh - 7)
                        {
                            voxelMap[x, y, z] = VoxelData.Soil;
                        }
                        else if (y >= (noiseHigh - 10) && y <= (noiseHigh - 7) && randomInt == 1)
                        {
                            voxelMap[x, y, z] = VoxelData.Soil;
                        }

                        //地下判断
                        else
                        {

                            //判断空气
                            float noise3d = Perlin3D((float)x * noise3d_scale + myposition.x * noise3d_scale, (float)y * noise3d_scale + y * noise3d_scale, (float)z * noise3d_scale + myposition.z * noise3d_scale); // 将100改为0.1

                            //洞穴深度概率
                            //float randomCave = Probability(y);

                            //空气
                            if (noise3d < caveWidth)
                            {
                                voxelMap[x, y, z] = VoxelData.Air;

                                if (y >= 4 && y <= 6)
                                {
                                    if (randomInt == 1)
                                    {
                                        voxelMap[x, y, z] = VoxelData.Stone;
                                    }
                                }
                            }
                            else
                            {
                                //煤炭
                                if (rand.Next(0, world.Random_Coal) < 1)
                                {
                                    voxelMap[x, y, z] = VoxelData.Stone;
                                    Coals.Enqueue(new Vector3(x, y, z));
                                }//铁
                                else if (rand.Next(0, world.Random_Iron) < 1)
                                {
                                    voxelMap[x, y, z] = VoxelData.Iron;
                                }//金
                                else if (rand.Next(0, world.Random_Gold) < 1)
                                {
                                    voxelMap[x, y, z] = VoxelData.Gold;
                                }//青金石
                                else if (rand.Next(0, world.Random_Blue_Crystal) < 1)
                                {
                                    voxelMap[x, y, z] = VoxelData.Blue_Crystal;
                                }//钻石
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
        }

        //补充树
        CreateTree();

        //补充煤炭
        foreach (var item in Coals)
        {
            CreateCoal((int)item.x, (int)item.y, (int)item.z);
        }

        //补充竹子
        //foreach (var item in Bamboos)
        //{
        //    CreateBamboo((int)item.x, (int)item.y, (int)item.z);
        //}


        //Mesh线程
        Thread myThread = new Thread(new ThreadStart(UpdateChunkMesh));
        myThread.Start();

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

    //------------------------------------------------------------------------------------






    //---------------------------------- Mesh部分 ----------------------------------------

    //开始遍历
    public void UpdateChunkMesh()
    {

        ClearMeshData();

        for (y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    //竹子断裂
                    updateBamboo();

                    //(是固体 || 是水 || 是水面上一层 || 是竹子)才生成
                    if (world.blocktypes[voxelMap[x, y, z]].isSolid || voxelMap[x, y, z] == VoxelData.Water || voxelMap[x, y - 1, z] == VoxelData.Water || voxelMap[x, y, z] == VoxelData.Bamboo)
                        UpdateMeshData(new Vector3(x, y, z));

                }
            }
        }

        //添加到world的渲染队列
        isReadyToRender = true;
        world.WaitToRender.Enqueue(this);
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

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (y >= 62)
        {
            return;
        }

        voxelMap[x, y, z] = targetBlocktype;

        UpdateChunkMesh();
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




            //自己是不是空气
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
            //自己是空气 && 目标是竹子 则绘制
            if (voxelMap[x, y, z] == VoxelData.Bamboo && voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air)
            {
                return true;
            }


            //判断是不是透明方块
            if (world.blocktypes[voxelMap[x, y, z]].isTransparent)
            {
                return false;
            }

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


        }


        return world.blocktypes[voxelMap[x, y, z]].isSolid;

    }

    //遍历中：：顺带判断面的生成方向
    //创建mesh里的参数
    void UpdateMeshData(Vector3 pos)
    {

        //判断六个面
        for (int p = 0; p < 6; p++)
        {

            //if (x == 10 && y == 37 && z == 14)
            //{
            //    print("");
            //}

            //voxelMap[pos + VoxelData.faceChecks[p]]
            //Debug.Log(pos);

            byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

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

    }

    //最后生成网格体
    public void CreateMesh()
    {

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
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

    //生成小花小草
    void updateMeshFlower()
    {

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
