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
    public bool isCalled = false;  //��Ҫɾ�ң���

    //Transform
    TestWorld world;
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
    public VoxelStruct[,,] voxelMap = new VoxelStruct[TerrainData.ChunkWidth, TerrainData.ChunkHeight, TerrainData.ChunkWidth];


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
    public TestChunk(Vector3 thisPosition, TestWorld _world, bool _BaseChunk)
    { 


        //World
        world = _world;
        caveWidth = world.cave_width;
        //debug_CanLookCave = !world.debug_CanLookCave;
        BaseChunk = _BaseChunk;
        noise3d_scale = world.noise3d_scale;
        Normal_treecount = world.terrainLayerProbabilitySystem.Normal_treecount;
        Forest_treecount = world.terrainLayerProbabilitySystem.������ľ��������Forest_treecount;
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
                    voxelMap[x, y, z] = new VoxelStruct(); // ��ʼ��ÿ��Ԫ��
                }
            }
        }

        CreateData();

        //print($"----------------------------------------------");
        //print($"{world.GetChunkLocation(myposition)}�Ѿ����ɣ�");
    }



    //-----------------------------------------------------------------------------------






    //----------------------------------- Noise ----------------------------------------

    //��Ѩ����������
    float GetCaveNoise(int _x, int _y, int _z)
    {
        return Perlin3D(((float)_x + myposition.x) * noise3d_scale, ((float)_y + y) * noise3d_scale, ((float)_z + myposition.z) * noise3d_scale); // ��100��Ϊ0.1

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
        for (int y = 0; y < TerrainData.ChunkHeight; y++)
        {
            for (int x = 0; x < TerrainData.ChunkWidth; x++)
            {
                for (int z = 0; z < TerrainData.ChunkWidth; z++)
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
                    //��������
                    else if (y > noiseHigh && y > world.terrainLayerProbabilitySystem.sea_level)
                    {

                        //����һ��
                        if (y - 1 < noiseHigh)
                        {

                            //���������
                            if (voxelMap[x, y - 1, z].voxelType != VoxelData.Sand && voxelMap[x, y - 1, z].voxelType != VoxelData.Air && voxelMap[x, y - 1, z].voxelType != VoxelData.Snow)
                            {

                                //��ľ��
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

                            //����
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

                    //�ж�ˮ��
                    else if (y > noiseHigh && y - 1 < world.terrainLayerProbabilitySystem.sea_level)
                    {

                        voxelMap[x, y, z].voxelType = VoxelData.Water;

                    }

                    //����֮��
                    else
                    {

                        //�ر�
                        if ((y + 1) > noiseHigh)
                        {

                            //ɳĮ����
                            if (world.GetBiomeType(x, z, myposition) == TerrainData.Biome_Dessert)
                            {

                                voxelMap[x, y, z].voxelType = VoxelData.Sand;

                            }

                            //��ԭ����
                            else
                            {
                                //100ѩ��
                                if (y > world.terrainLayerProbabilitySystem.Snow_Level)
                                {
                                    voxelMap[x, y, z].voxelType = VoxelData.Snow;
                                }

                                //90~100��������ѩ��
                                else if ((y > (world.terrainLayerProbabilitySystem.Snow_Level - 10f)) && GetProbability(70))
                                {
                                    voxelMap[x, y, z].voxelType = VoxelData.Snow;
                                }



                                //���ں�ƽ��
                                else if (y > world.terrainLayerProbabilitySystem.sea_level)
                                {

                                    //�Ƿ��Ǿ�˿��
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


                        //�������ж�
                        else if (y > noiseHigh - 7)
                        {
                            //ɳĮ�ж�
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
                            //ɳĮ�ж�
                            if (world.GetBiomeType(x, z, myposition) == TerrainData.Biome_Dessert)
                            {
                                voxelMap[x, y, z].voxelType = VoxelData.Sand;
                            }
                            else
                            {
                                voxelMap[x, y, z].voxelType = VoxelData.Soil;
                            }


                        }





                        //��
                        else if (noise3d < GetVaveWidth(y))
                        {

                            voxelMap[x, y, z].voxelType = VoxelData.Air;

                        }

                        //�����ж�
                        else
                        {

                            //ú̿
                            if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Coal))
                            {

                                voxelMap[x, y, z].voxelType = VoxelData.Stone;
                                Coals.Enqueue(new Vector3(x, y, z));

                            }

                            //��
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Iron))
                            {

                                voxelMap[x, y, z].voxelType = VoxelData.Iron;

                            }

                            //��
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Gold))
                            {

                                voxelMap[x, y, z].voxelType = VoxelData.Gold;

                            }

                            //���ʯ
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Blue_Crystal))
                            {

                                voxelMap[x, y, z].voxelType = VoxelData.Blue_Crystal;

                            }

                            //��ʯ
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
        //world.WaitToCreateMesh.Enqueue(this);

        UpdateChunkMesh_WithSurround();

    }




    //---------------------------------- Tree ----------------------------------------

    //tree
    void CreateTree()
    {
        //����Ⱥϵ
        if (world.GetBiomeType(x, z, myposition) == TerrainData.Biome_Forest)
        {
            //[ȷ��XZ]xoz�����ѡ��5����
            while (Forest_treecount-- != 0)
            {

                int random_x = rand.Next(2, TerrainData.ChunkWidth - 2);
                int random_z = rand.Next(2, TerrainData.ChunkWidth - 2);
                int random_y = TerrainData.ChunkHeight;
                int random_Tree_High = rand.Next(world.terrainLayerProbabilitySystem.TreeHigh_min, world.terrainLayerProbabilitySystem.TreeHigh_max + 1);

                //�������������׮
                //������������
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

                int random_x = rand.Next(2, TerrainData.ChunkWidth - 2);
                int random_z = rand.Next(2, TerrainData.ChunkWidth - 2);
                int random_y = TerrainData.ChunkHeight;
                int random_Tree_High = rand.Next(world.terrainLayerProbabilitySystem.TreeHigh_min, world.terrainLayerProbabilitySystem.TreeHigh_max + 1);

                //�������������׮
                //������������
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

            //����ѩ���ж�
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

            //����ѩ���ж�
            if (((_y + 1) >= world.terrainLayerProbabilitySystem.Snow_Level - 10f) && ((_y + 2) < TerrainData.ChunkHeight))
            {
                voxelMap[_x, _y + 2, _z + 1].voxelType = VoxelData.Snow;
                voxelMap[_x - 1, _y + 2, _z].voxelType = VoxelData.Snow;
                voxelMap[_x, _y + 2, _z].voxelType = VoxelData.Snow;
                voxelMap[_x + 1, _y + 2, _z].voxelType = VoxelData.Snow;
                voxelMap[_x, _y + 2, _z - 1].voxelType = VoxelData.Snow;
            }

        }

        //�ڶ���
        CreateLeaves(_x - 1, _y, _z);
        CreateLeaves(_x + 1, _y, _z);
        CreateLeaves(_x, _y, _z - 1);
        CreateLeaves(_x, _y, _z + 1);

        //����ѩ���ж�
        if (((_y) >= world.terrainLayerProbabilitySystem.Snow_Level - 10f) && ((_y + 1) < TerrainData.ChunkHeight) && voxelMap[_x - 1, _y + 1, _z].voxelType != VoxelData.Leaves)
        {

            voxelMap[_x - 1, _y + 1, _z].voxelType = VoxelData.Snow;
            voxelMap[_x + 1, _y + 1, _z].voxelType = VoxelData.Snow;
            voxelMap[_x, _y + 1, _z - 1].voxelType = VoxelData.Snow;
            voxelMap[_x, _y + 1, _z + 1].voxelType = VoxelData.Snow;

        }

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

            //ʮ�ּܲ�����ѩ���⼷���ڶ���
            //voxelMap[_x, _y, _z + 1] = VoxelData.Snow;
            //voxelMap[_x - 1, _y, _z] = VoxelData.Snow;
            //voxelMap[_x + 1, _y, _z] = VoxelData.Snow;
            //voxelMap[_x, _y, _z - 1] = VoxelData.Snow;
        }

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
    int CanSetStump(int _x, int _y, int _z, int treehigh)
    {

        if (GetProbability(_y))
        {

            while (_y > 0)
            {

                //��������������߲ݵ�������
                if (voxelMap[_x, _y - 1, _z].voxelType != VoxelData.Air)
                {

                    if (voxelMap[_x, _y - 1, _z].voxelType == VoxelData.Grass || voxelMap[_x, _y - 1, _z].voxelType == VoxelData.Soil && voxelMap[_x, _y - 2, _z].voxelType != VoxelData.Leaves)
                    {

                        //�ж������Ƿ�̫��
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

    //��Ҷ�����ж�
    //leaves�趨ֵ����ֹ������ľ
    void CreateLeaves(int x, int y, int z)
    {

        //����ǹ��壬�Ͳ���������Ҷ��
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

            if (voxelMap[_x, _y, _z].voxelType == VoxelData.Stone)
                voxelMap[_x, _y, _z].voxelType = VoxelData.Coal;

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

                voxelMap[x, y + temp, z].voxelType = VoxelData.Bamboo;

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

                if (voxelMap[_x, y - 1, _z].voxelType == VoxelData.Water)
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
        if (voxelMap[x, y, z].voxelType == VoxelData.Bamboo && voxelMap[x, y - 1, z].voxelType == VoxelData.Air)
        {

            voxelMap[x, y, z].voxelType = VoxelData.Air;

        }
    }

    //��ľ����ʧ
    void updateBush()
    {

        //����Լ���Bush && �Լ������ǿ��� ���Լ���Ϊ����
        if (voxelMap[x, y, z].voxelType == VoxelData.Bush && voxelMap[x, y - 1, z].voxelType == VoxelData.Air)
        {

            voxelMap[x, y, z].voxelType = VoxelData.Air;

        }

    }

    //------------------------------------------------------------------------------------






    //---------------------------------- Mesh���� ----------------------------------------


    public void UpdateChunkMesh_WithSurround()
    {

        ClearMeshData();

        //ˢ���Լ�
        for (y = 0; y < TerrainData.ChunkHeight; y++)
        {

            for (x = 0; x < TerrainData.ChunkWidth; x++)
            {

                for (z = 0; z < TerrainData.ChunkWidth; z++)
                {

                    //���Ӷ���
                    updateBamboo();

                    //Bush��ʧ
                    updateBush();

                    //[�ѷ������ƶ����������߳�ִ��]ˮ������
                    //updateWater();

                    // �ǿ��� - ��Ⱦ
                    // ˮ���� - ��Ⱦ
                    if (world.blocktypes[voxelMap[x, y, z].voxelType].DrawMode != DrawMode.Air)
                        UpdateMeshData(new Vector3(x, y, z));

                    //UpdateMeshData(new Vector3(x, y, z));
                }

            }

        }


        CreateMesh();

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
        if (x < 0 || x > TerrainData.ChunkWidth - 1 || y < 0 || y > TerrainData.ChunkHeight - 1 || z < 0 || z > TerrainData.ChunkWidth - 1)
        {

            //if (ThisChunkLocation == new Vector3(100f,0f,99f))
            //{
            //	print("");
            //}

            

            //Down:���²�һ�ɲ�����
            //if (y < 0)
            //{

            //    return true;

            //}

            ////else:�Լ��ǲ��ǿ���
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
            if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z].voxelType == VoxelData.Air) && voxelMap[x, y, z].voxelType == VoxelData.Air) || ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z].voxelType == VoxelData.Water) && voxelMap[x, y, z].voxelType == VoxelData.Water))
            {

                return true;

            }
            else // ����������
            {
                //����Լ���ˮ��Ŀ���ǿ���
                //������
                if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z].voxelType == VoxelData.Water) && voxelMap[x, y, z].voxelType == VoxelData.Air))
                {

                    return false;

                }

            }

            //�ж��ǲ���͸������
            if (world.blocktypes[voxelMap[x, y, z].voxelType].isTransparent)
            {

                return false;

            }

        }


        return world.blocktypes[voxelMap[x, y, z].voxelType].isSolid;

    }


    //�����У���˳���ж�������ɷ���
    void UpdateMeshData(Vector3 pos)
    {

        byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z].voxelType;

        //�������ģʽ
        switch (world.blocktypes[voxelMap[x, y, z].voxelType].DrawMode)
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



                            //������� && ��ˮ�� && �Ϸ��ǿ��� => ���������ܱ�ˮ���������ں�
                            if (p == 2 && voxelMap[x, y, z].voxelType == VoxelData.Water && voxelMap[x, y + 1, z].voxelType == VoxelData.Air && voxelMap[x, y, z].up == true)
                            {
                                int _zz = 0;
                                int _xx = 0;

                                //�ֲ�����
                                int __z = 0;
                                bool Z�������ϰ��� = false;

                                //���к��õ�ǰ�������̤���ķ�Χ
                                for (_xx = 0; _xx < TerrainData.ChunkWidth; _xx++)
                                {
                                    for (__z = 0; __z < TerrainData.ChunkWidth; __z++)
                                    {

                                        //������������
                                        if (isOutOfRange(x + _xx, y, z + __z))
                                        {
                                            continue;
                                        }

                                        //if(_xx == 8){
                                        //    print("");
                                        //} 


                                        //Ŀ����ˮ && Ŀ���Ϸ��ǿ��� && Ŀ���up��true
                                        if ((voxelMap[x + _xx, y, z + __z].voxelType != VoxelData.Water || voxelMap[x + _xx, y + 1, z + __z].voxelType != VoxelData.Air) || (voxelMap[x + _xx, y, z + __z].up == false))
                                        {
                                            Z�������ϰ��� = true;
                                            break;
                                        }

                                    }


                                    if (__z > _zz)
                                    {
                                        _zz = __z;
                                    }


                                    if (Z�������ϰ���)
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

                                //print($"local��{new Vector3(x, y, z)},���Ϳ�<{_xx},{_zz}>");


                                //���Ӧ�ø���һ�����Ϳ� _xx��_zz



                                //��ֹ�������0
                                if (_xx == 0)
                                {
                                    vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 0]]);
                                    vertices.Add(pos + ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 1]], new Vector3(1, 1, _zz)));
                                    vertices.Add(pos + ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 2]], new Vector3(1, 1, 1)));
                                    vertices.Add(pos + ComponentwiseMultiply(VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 3]], new Vector3(1, 1, _zz)));


                                    //����p���ɶ�Ӧ���棬��Ӧ��UV
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


                                    //����p���ɶ�Ӧ���棬��Ӧ��UV
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







    //---------------------------------- tools ----------------------------------------


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

        if (_x < 0 || _x > TerrainData.ChunkWidth - 1 || _y < 0 || _y > TerrainData.ChunkHeight - 1 || _z < 0 || _z > TerrainData.ChunkWidth - 1)
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

        if (_x < 0 || _x > TerrainData.ChunkWidth - 1 || _y < 0 || _y > TerrainData.ChunkHeight - 1 || _z < 0 || _z > TerrainData.ChunkWidth - 1)
        {

            return true;

        }
        else
        {

            return false;

        }

    }

    // �����˷�
    Vector3 ComponentwiseMultiply(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z); 
    }

    // ���� Vector2 �ķ����˷�
    Vector2 ComponentwiseMultiply(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x * b.x, a.y * b.y);
    }

    //----------------------------------------------------------------------------------

}


