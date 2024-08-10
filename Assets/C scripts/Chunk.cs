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
    public bool isCalled = false;  //��Ҫɾ�ң���
    public bool iHaveWater = false; private bool haeExec_iHaveWater = true;
    public bool hasExec_isHadupdateWater = true;  //�ñ�־����һ��Chunkÿ��ֻ�ܸ���һ��ˮ��


    //Transform
    World world;
    public GameObject chunkObject;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    //noise
    private float noise3d_scale;


    //Ⱥϵ����
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

    //�����෽��
    Queue<Vector3> Coals = new Queue<Vector3>();
    Queue<Vector3> Bamboos = new Queue<Vector3>();


    //��
    float caveWidth;
    public float mean = 16f; // ��ֵ
    public float stdDev = 5f; // ��׼��


    //���̱߳���
    public System.Random rand;
    public Vector3 myposition;


    //debug
    //bool debug_CanLookCave;




    //---------------------------------- ���ں��� ---------------------------------------




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
        Forest_treecount = world.terrainLayerProbabilitySystem.������ľ��������Forest_treecount;

        //Self
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = world.material;
        chunkObject.transform.SetParent(world.Chunks.transform);
        chunkObject.transform.position = new Vector3(thisPosition.x * VoxelData.ChunkWidth, 0f, thisPosition.z * VoxelData.ChunkWidth);
        chunkObject.name = thisPosition.x + "," + thisPosition.z;
        myposition = chunkObject.transform.position;


        //Data�߳�
        rand = new System.Random(world.terrainLayerProbabilitySystem.Seed);
        Thread myThread = new Thread(new ThreadStart(CreateData));
        myThread.Start();




        //print($"----------------------------------------------");
        //print($"{world.GetChunkLocation(myposition)}�Ѿ����ɣ�");
    }



    //-----------------------------------------------------------------------------------






    //----------------------------------- Noise ----------------------------------------



    //��������������
    float GetTotalNoiseHigh(int _x, int _z)
    {


        //(ƽԭ-ɽ��)��������
        float biome_moutainAndPlane = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((float)_x * 1e-05f + myposition.x * 1e-05f, (float)_z * 1e-05f + myposition.z * 1e-05f));
        

        //С��ƽԭ����
        //��ɽ������
        float soilmax = Mathf.Lerp(50, 64, biome_moutainAndPlane);
        float smooth = Mathf.Lerp(0.002f, 0.04f, biome_moutainAndPlane);
        float steep = Mathf.Lerp(0.004f, 0.05f, biome_moutainAndPlane);


        //��������
        float noise2d_1 = Mathf.Lerp((float)20, (float)soilmax, Mathf.PerlinNoise((float)_x * smooth + myposition.x * smooth, (float)_z * smooth + myposition.z * smooth));
        float noise2d_2 = Mathf.Lerp((float)20, (float)soilmax, Mathf.PerlinNoise((float)_x * steep + myposition.x * steep, (float)_z * steep + myposition.z * steep));
        float noiseHigh = noise2d_1 * 0.7f + noise2d_2 * 0.3f;


        return noiseHigh;


    }



    //Tree������
    float GetSmoothNoise_Tree()
    {

        float randomoffset = rand.Next(0, 10);
        float Offset_x = 100f * randomoffset;
        float Offset_z = 100f * randomoffset;


        float smoothNoise = Mathf.Lerp((float)0, (float)100, Mathf.PerlinNoise(  (myposition.x + Offset_x) * 0.005f,     (myposition.z + Offset_z) * 0.005f)   );
        return smoothNoise;

    }



    //Desert������
    float GetSmoothNoise_Desert(int _x,int _z)
    {

        //float randomoffset = rand.Next(0, 10);
        //float Offset_x = 100f * randomoffset;
        //float Offset_z = 100f * randomoffset;

        return Mathf.PerlinNoise((_x + myposition.x) * 0.003f, (_z + myposition.z) * 0.003f);
    }



    //��Ѩ����������
    float GetCaveNoise(int _x, int _y, int _z)
    {
        return Perlin3D(( (float)_x + myposition.x) * noise3d_scale,     ((float)_y + y) * noise3d_scale,      ((float)_z + myposition.z )* noise3d_scale); // ��100��Ϊ0.1

    }



    //��Ѩ���ʵݼ���
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






    //---------------------------------- Data�߳� ---------------------------------------



    //Data
    void CreateData()
    {


        //��һ��chunk���б���
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {


                    // ����0��1�������
                    //int randomInt = rand.Next(0, 2);
                    //int randomFrom_0_10 = rand.Next(0, 10);


                    //��������
                    //float noiseHigh = GetTotalNoiseHigh(x, z);
                    float noiseHigh = world.GetTotalNoiseHigh_Biome(x, z, myposition);


                    //������
                    float noise3d = GetCaveNoise(x, y, z);


                    //ɳĮ����
                    //float noise_desery = GetSmoothNoise_Desert(x, z);


                    //�жϻ���
                    //0~3�㲻׼���ɿ�
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
                    //��������
                    else if (y > noiseHigh && y > world.terrainLayerProbabilitySystem.sea_level)
                    {

                        //����һ��
                        if (y - 1 < noiseHigh)
                        {

                            //���������
                            if (voxelMap[x, y - 1, z] != VoxelData.Sand && voxelMap[x, y - 1, z] != VoxelData.Air && voxelMap[x, y - 1, z] != VoxelData.Snow)
                            {

                                //��ľ��
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

                            //����
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

                    //�ж�ˮ��
                    else if (y > noiseHigh && y - 1 < world.terrainLayerProbabilitySystem.sea_level)
                    {

                        voxelMap[x, y, z] = VoxelData.Water;

                    }

                    //����֮��
                    else
                    {

                        //�ر�
                        if ((y + 1) > noiseHigh)
                        {

                            //ɳĮ����
                            if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Dessert)
                            {

                                voxelMap[x, y, z] = VoxelData.Sand;

                            }

                            //��ԭ����
                            else
                            {
                                //100ѩ��
                                if (y > world.terrainLayerProbabilitySystem.Snow_Level)
                                {
                                    voxelMap[x, y, z] = VoxelData.Snow;
                                }

                                //90~100��������ѩ��
                                else if ((y > (world.terrainLayerProbabilitySystem.Snow_Level - 10f)) && GetProbability(50))
                                {
                                    voxelMap[x, y, z] = VoxelData.Snow;
                                }



                                //���ں�ƽ��
                                else if (y > world.terrainLayerProbabilitySystem.sea_level)
                                {

                                    //�Ƿ��Ǿ�˿��
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


                        //�������ж�
                        else if (y > noiseHigh - 7)
                        {
                            //ɳĮ�ж�
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
                            //ɳĮ�ж�
                            if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Dessert)
                            {
                                voxelMap[x, y, z] = VoxelData.Sand;
                            }
                            else
                            {
                                voxelMap[x, y, z] = VoxelData.Soil;
                            }


                        }





                        //��
                        else if (noise3d < GetVaveWidth(y))
                        {

                            voxelMap[x, y, z] = VoxelData.Air;

                        }

                        //�����ж�
                        else
                        {

                            //ú̿
                            if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Coal))
                            {
                                
                                voxelMap[x, y, z] = VoxelData.Stone;
                                Coals.Enqueue(new Vector3(x, y, z));

                            }

                            //��
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Iron))
                            {
                                
                                voxelMap[x, y, z] = VoxelData.Iron;

                            }

                            //��
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Gold))
                            {
                                
                                voxelMap[x, y, z] = VoxelData.Gold;

                            }

                            //���ʯ
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Blue_Crystal))
                            {
                                
                                voxelMap[x, y, z] = VoxelData.Blue_Crystal;

                            }

                            //��ʯ
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


        //������
        CreateTree();

        //����ú̿
        foreach (var item in Coals)
        {

            CreateCoal((int)item.x, (int)item.y, (int)item.z);

        }

        //��������
        foreach (var item in Bamboos)
        {

            CreateBamboo((int)item.x, (int)item.y, (int)item.z);

        }

        //����world��create
        world.WaitToCreateMesh.Enqueue(this);

    }




    //---------------------------------- Tree ----------------------------------------

    //tree
    void CreateTree()
    {
        //����Ⱥϵ
        if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Forest)
        {
            //[ȷ��XZ]xoz�����ѡ��5����
            while (Forest_treecount-- != 0)
            {

                int random_x = rand.Next(2, VoxelData.ChunkWidth - 2);
                int random_z = rand.Next(2, VoxelData.ChunkWidth - 2);
                int random_y = VoxelData.ChunkHeight;
                int random_Tree_High = rand.Next(world.terrainLayerProbabilitySystem.TreeHigh_min, world.terrainLayerProbabilitySystem.TreeHigh_max + 1);

                //�������������׮
                //������������
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

                    //������Ҷ
                    BuildLeaves(random_x, random_y + random_Tree_High, random_z);

                }


                //Debug.Log($"{random_x}, {random_y}, {random_z}");
            }
        }
        else
        {
            //[ȷ��XZ]xoz�����ѡ��5����
            int count = rand.Next(0, Normal_treecount);

            while (count-- != 0)
            {

                int random_x = rand.Next(2, VoxelData.ChunkWidth - 2);
                int random_z = rand.Next(2, VoxelData.ChunkWidth - 2);
                int random_y = VoxelData.ChunkHeight;
                int random_Tree_High = rand.Next(world.terrainLayerProbabilitySystem.TreeHigh_min, world.terrainLayerProbabilitySystem.TreeHigh_max + 1);

                //�������������׮
                //������������
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

                    //������Ҷ
                    BuildLeaves(random_x, random_y + random_Tree_High, random_z);

                }


                //Debug.Log($"{random_x}, {random_y}, {random_z}");
            }
        }
    }

    // ����һ������ֵ����Χ��0~100����������ֵԽ�ӽ�100�����ʽӽ�100%��Խ�ӽ�0�����ʽӽ�0%
    bool GetProbability(float input)
    {
        // ȷ������ֵ�� 0 �� 100 ֮��
        input = Mathf.Clamp(input, 0, 100);

        // ����һ�� 0 �� 100 ֮��������
        float randomValue = rand.Next(0, 100);

        // ��������С�ڵ�������ֵ���򷵻� true
        //Debug.Log(randomValue);
        bool a = randomValue < input;


        return a;
    }

    // ����һ������ֵ����Χ��0~100����������ֵԽ�ӽ�100�����ʽӽ�100%��Խ�ӽ�0�����ʽӽ�0%
    bool GetProbabilityTenThousandth(float input)  
    {
        // ȷ������ֵ�� 0 �� 100 ֮��
        input = Mathf.Clamp(input, 0, 100);

        // ����һ�� 0 �� 100 ֮��������
        float randomValue = rand.Next(0, 10000);

        // ��������С�ڵ�������ֵ���򷵻� true
        //Debug.Log(randomValue);
        bool a = randomValue < input;


        return a;
    }




    //������Ҷ
    void BuildLeaves(int _x, int _y, int _z)
    {

        int randomInt = rand.Next(0, 2);


        //��һ��
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

        //�ڶ���
        CreateLeaves(_x - 1, _y, _z);
        CreateLeaves(_x + 1, _y, _z);
        CreateLeaves(_x, _y, _z - 1);
        CreateLeaves(_x, _y, _z + 1);

        //������
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

        //���Ĳ�
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

    //��׮�����ж�
    int CanSetStump(int _x,int _y,int _z, int treehigh)
    {

        if (GetProbability(_y))
        {

            while (_y > 0)
            {

                //��������������߲ݵ�������
                if (voxelMap[_x, _y - 1, _z] != VoxelData.Air)
                {

                    if (voxelMap[_x, _y - 1, _z] == VoxelData.Grass || voxelMap[_x, _y - 1, _z] == VoxelData.Soil)
                    {

                        //�ж������Ƿ�̫��
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

                //��������������߶ȣ�������
                //else if (random_y + random_Tree_High >= VoxelData.ChunkHeight)
                //{
                //    needTree = false;
                //    break;
                //}
            }
        }

        

        return -1;

    }

    //��Ҷ�����ж�
    //leaves�趨ֵ����ֹ������ľ
    void CreateLeaves(int x, int y, int z)
    {

        //����ǹ��壬�Ͳ���������Ҷ��
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

    //ú̿
    void CreateCoal(int xtemp, int ytemp, int ztemp)
    {

        int random_Coal_up = rand.Next(0, 100);
        int random_Coal_down = rand.Next(0, 100);

        //��һ��
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


        //��һ��
        SetCoal(xtemp - 1, ytemp, ztemp - 1);
        SetCoal(xtemp, ytemp, ztemp - 1);
        SetCoal(xtemp + 1, ytemp, ztemp - 1);

        SetCoal(xtemp - 1, ytemp, ztemp);
        SetCoal(xtemp, ytemp, ztemp);
        SetCoal(xtemp + 1, ytemp, ztemp);

        SetCoal(xtemp - 1, ytemp, ztemp + 1);
        SetCoal(xtemp, ytemp, ztemp + 1);
        SetCoal(xtemp + 1, ytemp, ztemp + 1);

        //��һ��
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

    //ú̿�����ж�
    //����ǿ��� || ����������
    void SetCoal(int _x, int _y, int _z)
    {

        //�������
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

    //��������
    void CreateBamboo(int x, int y, int z)
    {

        //��ȷ����
        if (BambooJudge(x, y, z))
        {

            //��������1~2��
            //����ǿ����򸲸�Ϊ����
            for (int temp = 0; temp < rand.Next(1, 4); temp++)
            {

                voxelMap[x, y + temp, z] = VoxelData.Bamboo;

            }

        }



    }

    //�����ж�
    //���������һ��Ϊˮ����Ϊtrue
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

    //���Ӷ����ж�
    void updateBamboo()
    {

        //����Լ������� && �Լ������ǿ��� ���Լ���Ϊ����
        if (voxelMap[x, y, z] == VoxelData.Bamboo && voxelMap[x, y - 1, z] == VoxelData.Air)
        {

            voxelMap[x, y, z] = VoxelData.Air;

        }
    }


    //[ˮ������]
    //����Լ���ˮ
    //ǰ������������ǿ�����������Ǳ��ˮ
    public void Always_updateWater()
    {

        //���Ż� - ������ж���ΧһȦ����ˮ����ִ�м��ֱ������

        ClearMeshData();

        //ˢ���Լ�
        for (y = 0; y < VoxelData.ChunkHeight; y++)
        {

            for (x = 0; x < VoxelData.ChunkWidth; x++)
            {

                for (z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    //�޸�VoxelMap
                    if(hasExec_isHadupdateWater)
                        _updateWater();

                    // �ǿ��� - ��Ⱦ
                    // ˮ���� - ��Ⱦ
                    if (world.blocktypes[voxelMap[x, y, z]].DrawMode != DrawMode.Air)
                        UpdateMeshData(new Vector3(x, y, z));

                }

            }

        }



        hasExec_isHadupdateWater = true;

        //��ӵ�world����Ⱦ����
        isReadyToRender = true;

        //������Ⱦ�߳�
        if (world.RenderLock)
        {

            world.WaitToRender_temp.Enqueue(this);
            //print($"{world.GetChunkLocation(myposition)}�����������temp");

        }
        else
        {

            //print($"{world.GetChunkLocation(myposition)}���");
            world.WaitToRender.Enqueue(this);

        }

    }


    void _updateWater()
    {
        //ֻ���Լ���ˮ�������ִ��
        if (voxelMap[x, y, z] == VoxelData.Water)
        {

            //����������
            for (int _p = 0; _p < 5; _p++)
            {

                //�������
                if (isOutOfRange(new Vector3(x,y,z) + VoxelData.faceChecks_WaterFlow[_p]))
                {

                    //�ܻ�ȡ������Chunk
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks_WaterFlow[_p], out Chunk chunktemp))
                    {

                        Vector3 directlocation = GetDirectChunkVoxelMapLocation(new Vector3(x,y,z) + VoxelData.faceChecks_WaterFlow[_p]);

                        if (chunktemp.voxelMap[(int)directlocation.x, (int)directlocation.y, (int)directlocation.z] == VoxelData.Air)
                        {

                            chunktemp.voxelMap[(int)directlocation.x, (int)directlocation.y, (int)directlocation.z] = VoxelData.Water;
                            
                            //������ˮ�����ɺ�ˮ����
                            if (chunktemp.iHaveWater == false)
                            {

                                chunktemp.iHaveWater = true;

                            }

                            hasExec_isHadupdateWater = false;

                        }

                    }


                }

                //û����
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



    //��ľ����ʧ
    void updateBush()
    {

        //����Լ���Bush && �Լ������ǿ��� ���Լ���Ϊ����
        if (voxelMap[x, y, z] == VoxelData.Bush && voxelMap[x, y - 1, z] == VoxelData.Air)
        {

            voxelMap[x, y, z] = VoxelData.Air;

        }

    }

    //------------------------------------------------------------------------------------






    //---------------------------------- Mesh���� ----------------------------------------

    //��ʼ����
    public void UpdateChunkMesh_WithSurround() //�������أ������дĬ��Ϊfalse
    {

        UpdateChunkMesh_WithSurround(false);

    }

    public void UpdateChunkMesh_WithSurround(object obj)
    {

        bool iscaller = (bool)obj;

        ClearMeshData();

        //ˢ���Լ�
        for (y = 0; y < VoxelData.ChunkHeight; y++)
        {

            for (x = 0; x < VoxelData.ChunkWidth; x++)
            {

                for (z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    //���Ӷ���
                    updateBamboo();

                    //Bush��ʧ
                    updateBush();

                    //[�ѷ������ƶ����������߳�ִ��]ˮ������
                    //updateWater();

                    // �ǿ��� - ��Ⱦ
                    // ˮ���� - ��Ⱦ
                    if (world.blocktypes[voxelMap[x, y, z]].DrawMode != DrawMode.Air)
                        UpdateMeshData(new Vector3(x, y, z));

                    //UpdateMeshData(new Vector3(x, y, z));
                }

            }

        }


        //֪ͨ��Χ����ˢ��
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




        //��ӵ�world����Ⱦ����
        isReadyToRender = true;

        if (isCalled)
        {

            isCalled = false;

        }
        else
        {

            //print($"{world.GetChunkLocation(myposition)}Mesh���");
            world.MeshLock = false;

        }

        if (world.RenderLock)
        {

            world.WaitToRender_temp.Enqueue(this);
            //print($"{world.GetChunkLocation(myposition)}�����������temp");

        }
        else
        {

            //print($"{world.GetChunkLocation(myposition)}���");
            world.WaitToRender.Enqueue(this);

        }


        if (BaseChunk == true)
        {

            BaseChunk = false;

        }
        


        


    }

    //�������
    void ClearMeshData()
    {

        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        triangles_Water.Clear();
        uvs.Clear();

    }

    //�༭����
    public void EditData(Vector3 pos, byte targetBlocktype)
    {

        //ClearFInd_Direvtion();

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        //��ֹ����
        if (y >= VoxelData.ChunkHeight - 2)
        {

            return;

        }

        voxelMap[x, y, z] = targetBlocktype;

        UpdateChunkMesh_WithSurround(true);

    }

    //�����ɵ��ж�
    //�Ƿ�����----Y������----N����
    //�����߽�----Ҳ����N
    bool CheckVoxel(Vector3 pos, int _p)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        //print($"{x},{y},{z}");


        //���Ŀ�����
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

                    //����ܲ鵽
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                    {

                        //[�ѷ���������ͳһ��һ����������]
                        //ˮ������
                        //����Լ���ˮ�Ҷ����ǿ�������Ѷ�����ˮ
                        //if (voxelMap[x, y, z - 1] == VoxelData.Water && chunktemp.voxelMap[x, y, 0] == VoxelData.Air)
                        //{
                        //    chunktemp.voxelMap[x, y, 0] = VoxelData.Water;

                        //    world.WaitToCreateMesh.Enqueue(chunktemp);

                        //    return true;
                        //}


                        //target��͸����
                        //target��ˮ
                        //���Լ�����͸����
                        //�򷵻�false
                        //(Ŀ����͸���� || Ŀ����ˮ) && (�Լ�����͸���� && �Լ�����ˮ)
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

                        //�鲻��һ������
                        return false;

                    }

                }


                //Back
                if (z < 0)
                {

                    //����ܲ鵽
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                    {


                        //[�ѷ���������ͳһ��һ����������]
                        //ˮ������
                        //����Լ���ˮ�Ҷ����ǿ�������Ѷ�����ˮ
                        //if (voxelMap[x, y, z + 1] == VoxelData.Water && chunktemp.voxelMap[x, y, VoxelData.ChunkWidth - 1] == VoxelData.Air)
                        //{
                        //    chunktemp.voxelMap[x, y, 0] = VoxelData.Water;

                        //    return true;
                        //}



                        //���target�ǿ������򷵻�false
                        //target��ˮ
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

                        //�鲻��һ������
                        return false;

                    }


                }


                //Left
                if (x < 0)
                {

                    //����ܲ鵽
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                    {


                        //[�ѷ���������ͳһ��һ����������]
                        //ˮ������
                        //����Լ���ˮ�Ҷ����ǿ�������Ѷ�����ˮ
                        //if (voxelMap[x + 1, y, z] == VoxelData.Water && chunktemp.voxelMap[VoxelData.ChunkWidth - 1, y, z] == VoxelData.Air)
                        //{
                        //    chunktemp.voxelMap[x, y, 0] = VoxelData.Water;

                        //    return true;
                        //}

                        //���target�ǿ������򷵻�false
                        //target��ˮ
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

                        //�鲻��һ������
                        return false;

                    }

                }


                //Right
                if (x > VoxelData.ChunkWidth - 1)
                {

                    //����ܲ鵽
                    if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                    {


                        //[�ѷ���������ͳһ��һ����������]
                        //ˮ������
                        ////����Լ���ˮ�Ҷ����ǿ�������Ѷ�����ˮ
                        //if (voxelMap[x - 1, y, z] == VoxelData.Water && chunktemp.voxelMap[0, y, z] == VoxelData.Air)
                        //{
                        //    chunktemp.voxelMap[x, y, 0] = VoxelData.Water;

                        //    return true;
                        //}


                        //���target�ǿ������򷵻�false
                        //target��ˮ
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

                        //�鲻��һ������
                        return false;

                    }

                }

            }

            //Up����Ҫ����

            //Down:���²�һ�ɲ�����
            if (y < 0)
            {

                return true;

            }

            //else:�Լ��ǲ��ǿ���
            if (voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air || voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Water)
            {

                return true;

            }
            else
            {

                return false;

            }




        }


        //δ��������
        else
        {
            //Debug.Log("δ����");

            //�Լ��ǿ��� && Ŀ�������� �����
            //if (voxelMap[x, y, z] == VoxelData.Bamboo && voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air)
            //{
            //    return true;
            //}


            

            //�Լ���Ŀ�궼�ǿ���
            //�����Լ���Ŀ�궼��ˮ
            //��������
            if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air) && voxelMap[x, y, z] == VoxelData.Air) || ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Water) && voxelMap[x, y, z] == VoxelData.Water))
            {

                return true;

            }
            else // ����������
            {
                //����Լ���ˮ��Ŀ���ǿ���
                //������
                if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Water) && voxelMap[x, y, z] == VoxelData.Air))
                {

                    return false;

                }

            }

            //�ж��ǲ���͸������
            if (world.blocktypes[voxelMap[x, y, z]].isTransparent)
            {

                return false;

            }

        }


        return world.blocktypes[voxelMap[x, y, z]].isSolid;

    }

    //�����Լ���Ŀ������ͣ��ж��Ƿ�������
    //false��������
    bool CheckSelfAndTarget(byte _self,byte _target)
    {

        // ����Լ��ǿ������ľ��������ζ���������
        if (world.blocktypes[_self].isTransparent)
        {

            return true;

        }

        //����Լ���ˮ
        // Ŀ����Transparent������
        //Ŀ����ˮ���߹��壬������
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

        //����Լ��ǹ���
        //Ŀ���ǹ��壬������
        //����������
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




    //�����У���˳���ж�������ɷ���
    //����mesh��Ĳ���
    //_calledFrom = 0 ����UpdateChunkMesh_WithSurround
    //_calledFrom = 1 ����UpdateWater
    void UpdateMeshData(Vector3 pos)
    {

        byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

        //�������ģʽ
        switch (world.blocktypes[voxelMap[x, y, z]].DrawMode)
        {

            case DrawMode.Bush:// Bush����ģʽ

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



            case DrawMode.Water:  //ˮ�����ģʽ



                //�ж�������
                for (int p = 0; p < 6; p++)
                {

                    //���Ի���
                    if (!CheckVoxel(pos + VoxelData.faceChecks[p], p))
                    {

                        //������·���ˮ���򻻳ɷ������Ⱦ��ʽ
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
                            //����p���ɶ�Ӧ���棬��Ӧ��UV
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

                            //����p���ɶ�Ӧ���棬��Ӧ��UV
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



            default: //Ĭ��Block����ģʽ

                

                //�ж�������
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

                        //����p���ɶ�Ӧ���棬��Ӧ��UV
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

    // �������������
    public void CreateMesh()
    {

        world.RenderLock = true;

        //print($"{world.GetChunkLocation(myposition)}CreateMesh ��ʼ");

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        //mesh.triangles = triangles.ToArray();

        //ʹ��˫����
        meshRenderer.materials = new Material[] { world.material, world.material_Water };
        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0); // ��һ��������ʹ��triangles����
        mesh.SetTriangles(triangles_Water.ToArray(), 1); // �ڶ���������ʹ��triangles_2����

        //�Ż�
        mesh.Optimize();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        //print($"triangles:{triangles.Count}, triangles_Water:{triangles_Water.Count}");

        //print($"{world.GetChunkLocation(myposition)}CreateMesh ����");

        world.RenderLock = false;

        while (world.WaitToRender_temp.Count > 0)
        {
            world.WaitToRender_temp.TryDequeue(out Chunk chunktemp);
            world.WaitToRender.Enqueue(chunktemp);
        }


    }

    //�����桪��UV�Ĵ���
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
        //        print($"���˵ķ�����x = {x},y = {y}");
        //        hasExec_2 = false;
        //    }
        //}

    }



    //�����桪��Bush
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






    //---------------------------------- �������� ----------------------------------------



    //�����Լ�
    public void DestroyChunk()
    {

        Destroy(chunkObject);

    }

    //�����Լ�
    public void HideChunk()
    {

        chunkObject.SetActive(false);
        isShow = false;

    }

    //��ʾ�Լ�
    public void ShowChunk()
    {

        chunkObject.SetActive(true);
        isShow = true;

    }

    //3d����
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

    //�Ƿ����
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

    //��������
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


    //�Ƿ��ڱ߿���
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


    //��Ѩ���ɸ���
    //float Probability(float y)
    //{

    //    //float possible = 0;
    //    //float mid = world.soil_max / 2;

    //    //// ���yԽ�ӽ�0����possibleԽ�ӽ�0����֮Խ�ӽ�1
    //    //float ratio = y / mid;
    //    //possible = 1 - ratio;

    //    //return Mathf.Clamp01(possible); // ���ع�һ���� [0, 1] ����ĸ���ֵ

    //}

    //��ö���Chunk������
    //0 0 -1 ���� 0 0 15
    //0 0 16 ���� 0 0 0
    Vector3 GetDirectChunkVoxelMapLocation(Vector3 _vec)
    {

        //��⵽16����-1
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

        //û��⵽
        else
        {

            print("Chunk.GetDirectChunkVoxelMapLocation()���������쳣��");
            return new Vector3(0, 0, 0);

        }

    }


    //----------------------------------------------------------------------------------

}




//PerlinNoise������
public static class PerlinNoise
{
    static float interpolate(float a0, float a1, float w)
    {
        //���Բ�ֵ
        //return (a1 - a0) * w + a0;

        //hermite��ֵ
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
        //������ά����
        Vector2 pos = new Vector2(x, y);
        //�����õ�������'����'���ĸ���������
        Vector2 rightUp = new Vector2((int)x + 1, (int)y + 1);
        Vector2 rightDown = new Vector2((int)x + 1, (int)y);
        Vector2 leftUp = new Vector2((int)x, (int)y + 1);
        Vector2 leftDown = new Vector2((int)x, (int)y);

        //����x�ϵĲ�ֵ
        float v1 = dotGridGradient(leftDown, pos);
        float v2 = dotGridGradient(rightDown, pos);
        float interpolation1 = interpolate(v1, v2, x - (int)x);

        //����y�ϵĲ�ֵ
        float v3 = dotGridGradient(leftUp, pos);
        float v4 = dotGridGradient(rightUp, pos);
        float interpolation2 = interpolate(v3, v4, x - (int)x);

        float value = interpolate(interpolation1, interpolation2, y - (int)y);
        return value;
    }
}