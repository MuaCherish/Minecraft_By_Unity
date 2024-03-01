using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    //����״̬
    public bool myState = false;

    //���
    //public ChunkCoord coord;
    GameObject chunkObject;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    
    //Mesh�Ļ���
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    //Block����������
    public byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    //World�ű�
    World world;
    int treecount;

    //����
    private float noise2d_scale_smooth;
    private float noise2d_scale_steep;
    private float noise3d_scale;

    //ȫ��chunks
    private int x;
    private int y;
    private int z;

    //�������б�
    Queue<Vector3> Coals = new Queue<Vector3>();
    Queue<Vector3> Bamboos = new Queue<Vector3>();

    //---------------------------------- ���ں��� ---------------------------------------

    //Start()
    public Chunk(Vector3 thisPosition, World _world)
    {
        //����
        myState = true;

        //��world��ȡ����
        world = _world;
        
        UnityEngine.Random.InitState(world.Seed);
        noise2d_scale_smooth = world.noise2d_scale_smooth;
        noise2d_scale_steep = world.noise2d_scale_steep;
        noise3d_scale = world.noise3d_scale;
        treecount = world.TreeCount;


        //����
        //x = 0;
        //y = 0;
        //z = 0;

        //ChunkObject
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = world.material;
        
        chunkObject.transform.SetParent(world.Chunks.transform);
        chunkObject.transform.position = new Vector3(thisPosition.x * VoxelData.ChunkWidth, 0f, thisPosition.z * VoxelData.ChunkWidth);
        chunkObject.name = thisPosition.x + "," + thisPosition.z;

        //print(world.sea_level + $"+ {chunkObject.name}");

        //�ȴ�������
        CreateData();

        //��ʼ��������������
        DataToChunk();

    }


    //-----------------------------------------------------------------------------------







    //---------------------------------- Data���� ---------------------------------------

    //�������͵ĳ�ʼ��
    void CreateData()
    {



        //��һ��chunk���б���
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {


                    /*
					 0������
					 1��ʯͷ
					 2���ݵ�
					 3������
					 4������
                     5��ɳ��
                     6��ľͷ
                     7����Ҷ
                     8��ˮ
                     9��ú̿
					*/


                    int randomInt = UnityEngine.Random.Range(0, 2);

                    //�жϻ���
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



                        //����2d����
                        float noise2d_1 = Mathf.Lerp((float)world.soil_min, (float)world.soil_max, Mathf.PerlinNoise((float)x * noise2d_scale_smooth + chunkObject.transform.position.x * noise2d_scale_smooth, (float)z * noise2d_scale_smooth + chunkObject.transform.position.z * noise2d_scale_smooth));
                        float noise2d_2 = Mathf.Lerp((float)(world.soil_min), (float)world.soil_max, Mathf.PerlinNoise((float)x * noise2d_scale_steep + chunkObject.transform.position.x * noise2d_scale_steep, (float)z * noise2d_scale_steep + chunkObject.transform.position.z * noise2d_scale_steep));
                        float noise2d_3 = Mathf.Lerp((float)(world.soil_min), (float)world.soil_max, Mathf.PerlinNoise((float)x * 0.1f + chunkObject.transform.position.x * 0.1f, (float)z * 0.15f + chunkObject.transform.position.z * 0.15f));

                        //��������
                        float noiseHigh = noise2d_1 * 0.6f + noise2d_2 * 0.4f + noise2d_3 * 0.05f;


                        //��������
                        if (y > noiseHigh)
                        {

                            //if (y - 1 < noiseHigh)
                            //{
                            //    if (UnityEngine.Random.Range(0, 100) > 90)
                            //    {
                            //        voxelMap[x, y, z] = 6;
                            //    }
                            //    else
                            //    {
                            //        voxelMap[x, y, z] = VoxelData.Air;
                            //    }
                            //}
                            //else
                            //{
                            //    voxelMap[x, y, z] = 4;
                            //}


                            //���yС�ں�ƽ����Ϊˮ������Ϊ����
                            if (y - 1 < world.sea_level)
                            {
                                voxelMap[x, y, z] = VoxelData.Water;
                            }
                            else
                            {

                                //�ر�2��
                                if (y - 1 < noiseHigh)
                                {
                                    //����
                                    if (UnityEngine.Random.Range(0, world.Random_Bamboo) < 1)
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
                            float noise3d = Perlin3D((float)x * noise3d_scale + chunkObject.transform.position.x * noise3d_scale, (float)y * noise3d_scale + y * noise3d_scale, (float)z * noise3d_scale + chunkObject.transform.position.z * noise3d_scale); // ��100��Ϊ0.1

                            //����
                            if (noise3d < 0.4f)
                            {
                                voxelMap[x, y, z] = VoxelData.Air;
                            }
                            else
                            {
                                //ú̿
                                if (UnityEngine.Random.Range(0, world.Random_Coal) < 1)
                                {
                                    voxelMap[x, y, z] = VoxelData.Stone;
                                    Coals.Enqueue(new Vector3(x,y,z));
                                }//��
                                else if (UnityEngine.Random.Range(0, world.Random_Iron) < 1)
                                {
                                    voxelMap[x, y, z] = VoxelData.Iron;
                                }//��
                                else if (UnityEngine.Random.Range(0, world.Random_Gold) < 1)
                                {
                                    voxelMap[x, y, z] = VoxelData.Gold;
                                }//���ʯ
                                else if (UnityEngine.Random.Range(0, world.Random_Blue_Crystal) < 1)
                                {
                                    voxelMap[x, y, z] = VoxelData.Blue_Crystal;
                                }//��ʯ
                                else if (UnityEngine.Random.Range(0, world.Random_Diamond) < 1)
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
        foreach (var item in Bamboos)
        {
            CreateBamboo((int)item.x, (int)item.y, (int)item.z);
        }

    }

    //---------------------------------- Tree ----------------------------------------

    //tree
    void CreateTree()
    {
        //[ȷ��XZ]xoz�����ѡ��5����
        while (treecount-- != 0)
        {
            int random_x = UnityEngine.Random.Range(2, VoxelData.ChunkWidth - 2);
            int random_z = UnityEngine.Random.Range(2, VoxelData.ChunkWidth - 2);
            int random_y = VoxelData.ChunkHeight;
            bool needTree = true;
            int random_Tree_High = UnityEngine.Random.Range(world.TreeHigh_min, world.TreeHigh_max + 1);

            //[ȷ��Y]���±���ֱ���ر���һ��
            while (random_y-- != 0)
            {
                //�����ɳ�ӻ�����Ҷ����ˮ��������ӣ���������
                if (voxelMap[random_x, random_y - 1, random_z] == VoxelData.Sand || voxelMap[random_x, random_y - 1, random_z] == VoxelData.Leaves || voxelMap[random_x, random_y - 1, random_z] == VoxelData.Water || voxelMap[random_x, random_y - 1, random_z] == VoxelData.Bamboo)
                {
                    needTree = false;
                    break;
                }


                //��������������߶ȣ�������
                //else if (random_y + random_Tree_High >= VoxelData.ChunkHeight)
                //{
                //    needTree = false;
                //    break;
                //}

                //�����������������
                else if (voxelMap[random_x, random_y - 1, random_z] != VoxelData.Air)
                {
                    needTree = true;
                    break;
                }
            }


            //������׮����������5~7������
            if (needTree)
            {

                for (int i = 0; i <= random_Tree_High; i++)
                {
                    voxelMap[random_x, random_y + i, random_z] = VoxelData.Wood;
                }

                //������Ҷ
                CreateLeaves(random_x, random_y + random_Tree_High, random_z);

            }


            //Debug.Log($"{random_x}, {random_y}, {random_z}");
        }
    }

    //leaves
    void CreateLeaves(int _x, int _y, int _z)
    {
        int randomInt = UnityEngine.Random.Range(0, 2);


        //��һ��
        if (randomInt == 0)
        {
            SetLeaves(_x, _y + 1, _z);
        }
        else if (randomInt == 1)
        {
            SetLeaves(_x, _y + 1, _z + 1);
            SetLeaves(_x - 1, _y + 1, _z);
            SetLeaves(_x, _y + 1, _z);
            SetLeaves(_x + 1, _y + 1, _z);
            SetLeaves(_x, _y + 1, _z - 1);
        }

        //�ڶ���
        SetLeaves(_x - 1, _y, _z);
        SetLeaves(_x + 1, _y, _z);
        SetLeaves(_x, _y, _z - 1);
        SetLeaves(_x, _y, _z + 1);

        //������
        SetLeaves(_x - 1, _y - 1, _z + 2);
        SetLeaves(_x, _y - 1, _z + 2);
        SetLeaves(_x + 1, _y - 1, _z + 2);

        SetLeaves(_x - 2, _y - 1, _z + 1);
        SetLeaves(_x - 1, _y - 1, _z + 1);
        SetLeaves(_x, _y - 1, _z + 1);
        SetLeaves(_x + 1, _y - 1, _z + 1);
        SetLeaves(_x + 2, _y - 1, _z + 1);

        SetLeaves(_x - 2, _y - 1, _z);
        SetLeaves(_x - 1, _y - 1, _z);
        SetLeaves(_x + 1, _y - 1, _z);
        SetLeaves(_x + 2, _y - 1, _z);

        SetLeaves(_x - 2, _y - 1, _z - 1);
        SetLeaves(_x - 1, _y - 1, _z - 1);
        SetLeaves(_x, _y - 1, _z - 1);
        SetLeaves(_x + 1, _y - 1, _z - 1);
        SetLeaves(_x + 2, _y - 1, _z - 1);

        SetLeaves(_x - 1, _y - 1, _z - 2);
        SetLeaves(_x, _y - 1, _z - 2);
        SetLeaves(_x + 1, _y - 1, _z - 2);

        //���Ĳ�
        SetLeaves(_x - 1, _y - 2, _z + 2);
        SetLeaves(_x, _y - 2, _z + 2);
        SetLeaves(_x + 1, _y - 2, _z + 2);

        SetLeaves(_x - 2, _y - 2, _z + 1);
        SetLeaves(_x - 1, _y - 2, _z + 1);
        SetLeaves(_x, _y - 2, _z + 1);
        SetLeaves(_x + 1, _y - 2, _z + 1);
        SetLeaves(_x + 2, _y - 2, _z + 1);

        SetLeaves(_x - 2, _y - 2, _z);
        SetLeaves(_x - 1, _y - 2, _z);
        SetLeaves(_x + 1, _y - 2, _z);
        SetLeaves(_x + 2, _y - 2, _z);

        SetLeaves(_x - 2, _y - 2, _z - 1);
        SetLeaves(_x - 1, _y - 2, _z - 1);
        SetLeaves(_x, _y - 2, _z - 1);
        SetLeaves(_x + 1, _y - 2, _z - 1);
        SetLeaves(_x + 2, _y - 2, _z - 1);

        SetLeaves(_x - 1, _y - 2, _z - 2);
        SetLeaves(_x, _y - 2, _z - 2);
        SetLeaves(_x + 1, _y - 2, _z - 2);

    }

    //leaves�趨ֵ����ֹ������ľ
    void SetLeaves(int x, int y, int z)
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
        int random_Coal_up = UnityEngine.Random.Range(0, 100);
        int random_Coal_down = UnityEngine.Random.Range(0, 100);

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

    //����ǿ��� || ����������
    void SetCoal(int _x, int _y, int _z)
    {
        //�������
        if (isOutOfRange(_x,_y,_z))
        {
            return;
        }
        else
        {
            if(voxelMap[_x, _y, _z] == VoxelData.Stone)
                voxelMap[_x, _y, _z] = VoxelData.Coal;
        }

        
    }

    //---------------------------------- Bamboo ----------------------------------------

    //��������
    void CreateBamboo(int x,int y,int z)
    {
        //��ȷ����
        if (BambooJudge(x,y,z))
        {
            //��������1~2��
            //����ǿ����򸲸�Ϊ����
            for (int temp = 0; temp < UnityEngine.Random.Range(1,4); temp ++)
            {
                voxelMap[x,y + temp,z] = VoxelData.Bamboo;
            }

        }



    }

    //�����ж�
    //���������һ��Ϊˮ����Ϊtrue
    bool BambooJudge(int x,int y,int z)
    {
        for (int _x = 0; _x < 1; _x ++)
        {
            for (int _z = 0; _z < 1; _z++)
            {
                if (voxelMap[_x,y - 1, _z] == VoxelData.Water)
                {
                    return true;
                }
            }
        }

        return false;
    }

    //------------------------------------------------------------------------------------






    //---------------------------------- Mesh���� ----------------------------------------

    //��ʼ����
    public void DataToChunk()
    {

        ClearMeshData();

        for (y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    //�����С����������һ��updatemesh�㷨
                    //if(voxelMap[x, y - 1, z] == 9)
                    //{
                    //    updateMeshFlower(new Vector3(x, y, z));
                    //    continue;
                    //}

                    //if (x == 10 && y == 42 && z == 1)
                    //{
                    //    print("");
                    //}



                    //����Լ������� && �Լ������ǿ��� ���Լ���Ϊ����
                    if (voxelMap[x,y,z] == VoxelData.Bamboo && voxelMap[x, y - 1, z] == VoxelData.Air)
                    {
                        voxelMap[x, y, z] = VoxelData.Air;
                    }



                    //(�ǹ��� || ��ˮ || ��ˮ����һ�� || ������)������
                    if (world.blocktypes[voxelMap[x, y, z]].isSolid || voxelMap[x, y, z] == VoxelData.Water || voxelMap[x, y - 1, z] == VoxelData.Water || voxelMap[x, y, z] == VoxelData.Bamboo)
                        UpdateMeshData(new Vector3(x, y, z));
                    


                }
            }
        }

        //�һ���Թ���������
        CreateMesh();

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

        DataToChunk();
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



            //Ŀ�괦�Ƿ����chunk
            //if (Copy_All_Chunks.TryGetValue(VoxelData.faceChecks[_p] + ThisChunkLocation, out Chunk cc))
            //{

            //    if (hasExec)
            //    {
            //        //print($"��ǰChunk��{ThisChunkLocation}��Ŀ��Chunk��{ThisChunkLocation + VoxelData.faceChecks[_p]}");
            //        hasExec = false;
            //    }

            //    //�ж�һ�·���
            //    //Z����
            //    if (VoxelData.faceChecks[_p] == new Vector3(0, 0, -1))
            //    {
            //        //�Լ��ǿ�����Ŀ����Block���򷵻�false
            //        if ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4) && cc.voxelMap[x, y, VoxelData.ChunkWidth - 1] != 4)
            //        {
            //            return false;
            //        }
            //        else
            //        {
            //            return true;
            //        }
            //    }
            //    else if (VoxelData.faceChecks[_p] == new Vector3(0, 0, 1))
            //    {
            //        //�Լ��ǿ�����Ŀ����Block���򷵻�false
            //        if ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4) && cc.voxelMap[x, y, 0] != 4)
            //        {
            //            return false;
            //        }
            //        else
            //        {
            //            return true;
            //        }
            //    }
            //    //x����
            //    else if (VoxelData.faceChecks[_p] == new Vector3(-1, 0, 0))
            //    {
            //        //�Լ��ǿ�����Ŀ����Block���򷵻�false
            //        if ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4) && cc.voxelMap[VoxelData.ChunkWidth - 1, y, z] != 4)
            //        {
            //            return false;
            //        }
            //        else
            //        {
            //            return true;
            //        }
            //    }
            //    else if (VoxelData.faceChecks[_p] == new Vector3(1, 0, 0))
            //    {
            //        //�Լ��ǿ�����Ŀ����Block���򷵻�false
            //        if ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4) && cc.voxelMap[0, y, z] != 4)
            //        {
            //            return false;
            //        }
            //        else
            //        {
            //            return true;
            //        }
            //    }


            //    //�Լ��ǿ�����Ŀ����Block���򷵻�false
            //    //if ((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4) && cc.voxelMap[x & 16, y & 16, z & 16] != 4)
            //    //{
            //    //	return false;
            //    //}
            //    //else
            //    //{
            //    //	return true;
            //    //}

            //}
            //else
            //{
            //    //�Լ��ǲ��ǿ���
            //    if (voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == 4)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}




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
                }else if (((voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Air) && voxelMap[x, y, z] == VoxelData.Water))
                {
                    return false;
                }
            }


            //����Լ���ˮ
            //if (voxelMap[x - (int)VoxelData.faceChecks[_p].x, y - (int)VoxelData.faceChecks[_p].y, z - (int)VoxelData.faceChecks[_p].z] == VoxelData.Water)
            //{
            //    //�Ϸ�ʱ
            //    if (_p == 2 || voxelMap[x, y, z] == 4)
            //    {
            //        return false;
            //    }
            //    else // ǰ������һ�ɲ�����
            //    {
            //        return true;
            //    }
            //}



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
    //meshʵ�廯
    void CreateMesh()
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
        myState = false;
        chunkObject.SetActive(false);
    }

    //��ʾ�Լ�
    public void ShowChunk()
    {
        myState = true;
        chunkObject.SetActive(true);
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
    bool isOutOfRange(int x,int y,int z)
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

    //----------------------------------------------------------------------------------

}
