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

    //����
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

    //Ⱥϵ����
    int treecount;

    //�����෽��
    Queue<Vector3> Coals = new Queue<Vector3>();
    Queue<Vector3> Bamboos = new Queue<Vector3>();

    //��
    float caveWidth;
    public float mean = 16f; // ��ֵ
    public float stdDev = 5f; // ��׼��

    //���̱߳���
    System.Random rand;
    Vector3 myposition;

    //---------------------------------- ���ں��� ---------------------------------------

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

        //Data�߳�
        rand = new System.Random(world.Seed);
        myposition = chunkObject.transform.position;
        Thread myThread = new Thread(new ThreadStart(CreateData));
        myThread.Start();
    }


    //-----------------------------------------------------------------------------------







    //---------------------------------- Data�߳� ---------------------------------------
    
    //����������
    float GetTotalNoiseHigh(int _x, int _z)
    {
        //(ƽԭ-ɽ��)��������
        float biome_moutainAndPlane = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((float)_x * 5e-05f + myposition.x * 5e-05f, (float)_z * 5e-05f + myposition.z * 5e-05f));
        
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

    //��ȡ������
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
        //��һ��chunk���б���
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    // ����0��1�������
                    int randomInt = rand.Next(0, 2);

                    //�жϻ���
                    //0~3�㲻׼���ɿ�
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

                        //����2d����
                        //float noise2d_1 = Mathf.Lerp((float)world.soil_min, (float)world.soil_max, Mathf.PerlinNoise((float)x * noise2d_scale_smooth + myposition.x * noise2d_scale_smooth, (float)z * noise2d_scale_smooth + myposition.z * noise2d_scale_smooth));
                        //float noise2d_2 = Mathf.Lerp((float)(world.soil_min), (float)world.soil_max, Mathf.PerlinNoise((float)x * noise2d_scale_steep + myposition.x * noise2d_scale_steep, (float)z * noise2d_scale_steep + myposition.z * noise2d_scale_steep));
                        //float noise2d_3 = Mathf.Lerp((float)(world.soil_min), (float)world.soil_max, Mathf.PerlinNoise((float)x * 0.1f + myposition.x * 0.1f, (float)z * 0.15f + myposition.z * 0.15f));

                        //��������
                        //float noiseHigh = noise2d_1 * 0.6f + noise2d_2 * 0.4f + noise2d_3 * 0.05f;
                        float noiseHigh = GetTotalNoiseHigh(x,z);

                        //��������
                        if (y > noiseHigh)
                        {

                            //���yС�ں�ƽ����Ϊˮ������Ϊ����
                            if (y - 1 < world.sea_level)
                            {
                                voxelMap[x, y, z] = VoxelData.Water;
                            }
                            else
                            {

                                ////�ر�2��
                                //if (y - 1 < noiseHigh)
                                //{
                                //    //����
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



                        }//�ر�1��
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


                        //�������ж�
                        else if (y > noiseHigh - 7)
                        {
                            voxelMap[x, y, z] = VoxelData.Soil;
                        }
                        else if (y >= (noiseHigh - 10) && y <= (noiseHigh - 7) && randomInt == 1)
                        {
                            voxelMap[x, y, z] = VoxelData.Soil;
                        }

                        //�����ж�
                        else
                        {

                            //�жϿ���
                            float noise3d = Perlin3D((float)x * noise3d_scale + myposition.x * noise3d_scale, (float)y * noise3d_scale + y * noise3d_scale, (float)z * noise3d_scale + myposition.z * noise3d_scale); // ��100��Ϊ0.1

                            //��Ѩ��ȸ���
                            //float randomCave = Probability(y);

                            //����
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
                                //ú̿
                                if (rand.Next(0, world.Random_Coal) < 1)
                                {
                                    voxelMap[x, y, z] = VoxelData.Stone;
                                    Coals.Enqueue(new Vector3(x, y, z));
                                }//��
                                else if (rand.Next(0, world.Random_Iron) < 1)
                                {
                                    voxelMap[x, y, z] = VoxelData.Iron;
                                }//��
                                else if (rand.Next(0, world.Random_Gold) < 1)
                                {
                                    voxelMap[x, y, z] = VoxelData.Gold;
                                }//���ʯ
                                else if (rand.Next(0, world.Random_Blue_Crystal) < 1)
                                {
                                    voxelMap[x, y, z] = VoxelData.Blue_Crystal;
                                }//��ʯ
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

        //������
        CreateTree();

        //����ú̿
        foreach (var item in Coals)
        {
            CreateCoal((int)item.x, (int)item.y, (int)item.z);
        }

        //��������
        //foreach (var item in Bamboos)
        //{
        //    CreateBamboo((int)item.x, (int)item.y, (int)item.z);
        //}


        //Mesh�߳�
        Thread myThread = new Thread(new ThreadStart(UpdateChunkMesh));
        myThread.Start();

    }


    //---------------------------------- Tree ----------------------------------------

    //tree
    void CreateTree()
    {

        if (GetSmoothNoise_Tree() > 35f && GetSmoothNoise_Tree() < 60)
        {
            //[ȷ��XZ]xoz�����ѡ��5����
            while (treecount-- != 0)
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

    //------------------------------------------------------------------------------------






    //---------------------------------- Mesh���� ----------------------------------------

    //��ʼ����
    public void UpdateChunkMesh()
    {

        ClearMeshData();

        for (y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    //���Ӷ���
                    updateBamboo();

                    //(�ǹ��� || ��ˮ || ��ˮ����һ�� || ������)������
                    if (world.blocktypes[voxelMap[x, y, z]].isSolid || voxelMap[x, y, z] == VoxelData.Water || voxelMap[x, y - 1, z] == VoxelData.Water || voxelMap[x, y, z] == VoxelData.Bamboo)
                        UpdateMeshData(new Vector3(x, y, z));

                }
            }
        }

        //��ӵ�world����Ⱦ����
        isReadyToRender = true;
        world.WaitToRender.Enqueue(this);
    }

    //�������
    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    //�༭����
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




            //�Լ��ǲ��ǿ���
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
            //�Լ��ǿ��� && Ŀ�������� �����
            if (voxelMap[x, y, z] == VoxelData.Bamboo && voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air)
            {
                return true;
            }


            //�ж��ǲ���͸������
            if (world.blocktypes[voxelMap[x, y, z]].isTransparent)
            {
                return false;
            }

            //�ж��Լ���Ŀ���ǲ��Ƕ��ǿ��� || �Լ���Ŀ���ǲ��Ƕ���ˮ
            //������������
            if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air) && voxelMap[x, y, z] == VoxelData.Air) || ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Water) && voxelMap[x, y, z] == VoxelData.Water))
            {
                return true;
            }
            else // ����������
            {
                //(����Լ���ˮ��Ŀ���ǿ���) || (�Լ��ǿ�����Ŀ����ˮ)
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

    //�����У���˳���ж�������ɷ���
    //����mesh��Ĳ���
    void UpdateMeshData(Vector3 pos)
    {

        //�ж�������
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

    }

    //�������������
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

    //����С��С��
    void updateMeshFlower()
    {

    }



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

    //��Ѩ���ɸ���
    float Probability(float y)
    {
        float possible = 0;
        float mid = world.soil_max / 2;

        // ���yԽ�ӽ�0����possibleԽ�ӽ�0����֮Խ�ӽ�1
        float ratio = y / mid;
        possible = 1 - ratio;

        return Mathf.Clamp01(possible); // ���ع�һ���� [0, 1] ����ĸ���ֵ
    }


    //----------------------------------------------------------------------------------

}
