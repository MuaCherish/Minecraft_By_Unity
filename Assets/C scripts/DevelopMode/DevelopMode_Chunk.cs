//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;


public class DevelopModeChunk : MonoBehaviour
{

    //state
    public bool isReadyToRender = false;
    public bool hasExec_isHadupdateWater = true;  //�ñ�־����һ��Chunkÿ��ֻ�ܸ���һ��ˮ��


    //Transform
    DevelopModeWorld world;
    public GameObject chunkObject;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;


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


    //Ⱥϵ����
    int Plain_treecount;
    int Forest_treecount;


    //�����෽��
    Queue<Vector3> Coals = new Queue<Vector3>();
    Queue<Vector3> Bamboos = new Queue<Vector3>();


    //���̱߳���
    public System.Random rand;
    public Vector3 myposition;





    //---------------------------------- ���ں��� ---------------------------------------




    //Start()
    public DevelopModeChunk(Vector3 thisPosition, DevelopModeWorld _world)
    {
        //print("��ʼִ�г�ʼ��");
        //World
        world = _world;
        Plain_treecount = world.һ����ľ��������Plain_treecount;
        Forest_treecount = world.������ľ��������Forest_treecount;

        //Self
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = world.material;
        chunkObject.transform.SetParent(world.ChunkPATH.transform);
        chunkObject.transform.position = new Vector3(thisPosition.x * VoxelData.ChunkWidth, 0f, thisPosition.z * VoxelData.ChunkWidth);
        chunkObject.name = "BlockChunk--" + thisPosition.x + "," + thisPosition.z;
        myposition = chunkObject.transform.position;
        rand = new System.Random(world.Seed);

        ////�л�״̬
        if (myposition == new Vector3((world.RenderWidth - 1) * 16f, 0f, (world.RenderWidth - 1) * 16f))
        {
            world.isLoading = false;
        }
        //Data�߳�
        Thread myThread = new Thread(new ThreadStart(CreateData));
        myThread.Start();

        //CreateData();


        //print($"----------------------------------------------");
        //print($"{world.GetChunkLocation(myposition)}�Ѿ����ɣ�");
    }


    //-----------------------------------------------------------------------------------






    //----------------------------------- Noise ----------------------------------------



    //Tree������
    float GetSmoothNoise_Tree()
    {

        float randomoffset = rand.Next(0, 10);
        float Offset_x = 100f * randomoffset;
        float Offset_z = 100f * randomoffset;


        float smoothNoise = Mathf.Lerp((float)0, (float)100, Mathf.PerlinNoise((myposition.x + Offset_x) * 0.005f, (myposition.z + Offset_z) * 0.005f));
        return smoothNoise;

    }


    //Desert������
    float GetSmoothNoise_Desert(int _x, int _z)
    {

        //float randomoffset = rand.Next(0, 10);
        //float Offset_x = 100f * randomoffset;
        //float Offset_z = 100f * randomoffset;

        return Mathf.PerlinNoise((_x + myposition.x) * 0.003f, (_z + myposition.z) * 0.003f);
    }







    //-----------------------------------------------------------------------------------






    //---------------------------------- Data�߳� ---------------------------------------



    //Data
    public void CreateData()
    {
        //print("��ʼִ��CreateData");

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
                    float noiseHigh = world.GetTotalNoiseHigh_Biome(x, z, myposition);


                    //������
                    //float noise3d = GetCaveNoise(x, y, z);


                    //ɳĮ����
                    float noise_desery = GetSmoothNoise_Desert(x, z);

                    //��������
                    if (y > noiseHigh && y > world.sea_level)
                    {

                        //����һ��
                        if (y - 1 < noiseHigh)
                        {

                            //���������
                            if (voxelMap[x, y - 1, z] != VoxelData.Sand && voxelMap[x, y - 1, z] != VoxelData.Air)
                            {

                                //��ľ��
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

                            //����
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

                    //�ж�ˮ��
                    else if (y > noiseHigh && y - 1 < world.sea_level)
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
                            if (noise_desery > 0.6f)
                            {

                                voxelMap[x, y, z] = VoxelData.Sand;

                            }

                            //��ԭ����
                            else
                            {
                                //100ѩ��
                                if (y > world.Snow_Level)
                                {
                                    voxelMap[x, y, z] = VoxelData.Snow;
                                }

                                //90~100��������ѩ��
                                else if ((y > (world.Snow_Level - 10f)) && GetProbability_New(50))
                                {
                                    voxelMap[x, y, z] = VoxelData.Snow;
                                }



                                //���ں�ƽ��
                                else if (y > world.sea_level)
                                {

                                    voxelMap[x, y, z] = VoxelData.Grass;
                                     
                                }
                                else
                                {

                                    if (GetProbability_New(90))
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
                        //�����ж�
                        else
                        {
                            voxelMap[x, y, z] = VoxelData.Air;
                           
                           
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

        UpdateChunkMesh();
        //Thread myThread = new Thread(new ThreadStart(UpdateChunkMesh));
        //myThread.Start();
    }




    //---------------------------------- Tree ----------------------------------------

    //tree
    void CreateTree()
    {
        //����Ⱥϵ
        if (world.GetBiomeType(x,z,myposition) == VoxelData.Biome_Forest)
        {
            //[ȷ��XZ]xoz�����ѡ��5����
            while (Forest_treecount-- != 0)
            {

                int random_x = rand.Next(2, VoxelData.ChunkWidth - 2);
                int random_z = rand.Next(2, VoxelData.ChunkWidth - 2);
                int random_y = VoxelData.ChunkHeight;
                int random_Tree_High = rand.Next(world.TreeHigh_min, world.TreeHigh_max + 1);

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
            while (world.һ����ľ��������Plain_treecount-- != 0)
            {

                int random_x = rand.Next(2, VoxelData.ChunkWidth - 2);
                int random_z = rand.Next(2, VoxelData.ChunkWidth - 2);
                int random_y = VoxelData.ChunkHeight;
                int random_Tree_High = rand.Next(world.TreeHigh_min, world.TreeHigh_max + 1);

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
    bool GetProbability_New(float input)
    {
        // ȷ������ֵ�� 0 �� 100 ֮��
        input = Mathf.Clamp(input, 0, 100);

        // ����һ�� 0 �� 100 ֮��������
        float randomValue = rand.Next(0, 100);

        // ��������С�ڵ�������ֵ���򷵻� true
        //Debug.Log(randomValue);
        return randomValue <= input;
    }


    bool GetProbability(float input)
    {

        if (input > 50f)
        {

            // �������ֵ����45�����Ƿ���true
            return true;

        }
        else
        {

            // �������ֵ��0~45֮�䣬���ݸ��ʷ���true����false
            float probability = rand.Next(0, 200);
            return probability <= input;

        }
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
    int CanSetStump(int _x, int _y, int _z, int treehigh)
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

    public void UpdateChunkMesh()
    {
        //print("��ʼִ��UpdateChunkMesh");
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


        //��ӵ�world����Ⱦ����
        isReadyToRender = true;



        world.WaitToRender.Enqueue(this);

        //CreateMesh();






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
        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
        {

            //if (ThisChunkLocation == new Vector3(100f,0f,99f))
            //{
            //	print("");
            //}

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
    bool CheckSelfAndTarget(byte _self, byte _target)
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
        //print("��ʼִ��CreateMesh");
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







    //------------------------------------------------------------------------------------






    //---------------------------------- �������� ----------------------------------------




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

    public void Destroyself()
    {
        Destroy(chunkObject);
    }


    //----------------------------------------------------------------------------------

}
