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
    public bool isCalled = false;  //��Ҫɾ�ң���
    public bool iHaveWater = false;
    // private bool haeExec_iHaveWater = true;
    public bool hasExec_isHadupdateWater = true;  //�ñ�־����һ��Chunkÿ��ֻ�ܸ���һ��ˮ��

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

    //�浵
    public bool isSaving = false;
    public List<EditStruct> EditList = new List<EditStruct>();

    //Ⱥϵ����
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

    //�����෽��
    Queue<Vector3> Coals = new Queue<Vector3>();
    Queue<Vector3> Bamboos = new Queue<Vector3>();


    //��
    float caveWidth;
    public float mean = 16f; // ��ֵ
    public float stdDev = 5f; // ��׼��


    //���̱߳���
    Thread myThread;
    public System.Random rand;
    public Vector3 myposition;


    //debug
    //bool debug_CanLookCave;




    //---------------------------------- ���ں��� ---------------------------------------




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
        Forest_treecount = world.terrainLayerProbabilitySystem.������ľ��������Forest_treecount;
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

        //��ʼ��Voxel����
        InitVoxelStruct();


        //��ȡȺϵ����
        worldType = world.worldSetting.worldtype;
        //print(worldType);

        switch (worldType)
        {
            //��ԭȺϵ
            case 0:
                myThread = new Thread(new ThreadStart(CreateData));
                break;
            //��ԭȺϵ
            case 1:
                myThread = new Thread(new ThreadStart(CreateData));
                break;
            //ɳĮȺϵ
            case 2:
                myThread = new Thread(new ThreadStart(CreateData_Dessert));
                break;
            //����Ⱥϵ
            case 3:
                myThread = new Thread(new ThreadStart(CreateData_Marsh));
                break;
            //����Ⱥϵ
            case 4:
                myThread = new Thread(new ThreadStart(CreateData_Forest));
                break;
            //Ĭ��
            case 5:
                myThread = new Thread(new ThreadStart(CreateData));
                break;
            //��ƽ̹
            case 6:
                myThread = new Thread(new ThreadStart(CreateData_SuperPlain));
                //CreateData_SuperPlain();
                break;
            default:
                print("chunk.worldType����");
                break;
        }

        myThread.Start();

        //print($"----------------------------------------------");
        //print($"{world.GetChunkLocation(myposition)}�Ѿ����ɣ�");
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
        Forest_treecount = world.terrainLayerProbabilitySystem.������ľ��������Forest_treecount;
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

        //��ʼ��Voxel����
        InitVoxelStruct();


        //��ȡȺϵ����
        worldType = world.worldSetting.worldtype;
        //print(worldType);

        switch (worldType)
        {
            //��ԭȺϵ
            case 0:
                myThread = new Thread(new ThreadStart(CreateData));
                break; 
            //��ԭȺϵ
            case 1:
                myThread = new Thread(new ThreadStart(CreateData));
                break;
            //ɳĮȺϵ
            case 2:
                myThread = new Thread(new ThreadStart(CreateData_Dessert));
                break;
            //����Ⱥϵ
            case 3:
                myThread = new Thread(new ThreadStart(CreateData_Marsh));
                break;
            //����Ⱥϵ
            case 4:
                myThread = new Thread(new ThreadStart(CreateData_Forest));
                break;
            //Ĭ��
            case 5:
                myThread = new Thread(new ThreadStart(CreateData));
                break;
            //��ƽ̹
            case 6:
                myThread = new Thread(new ThreadStart(CreateData_SuperPlain));
                //CreateData_SuperPlain();
                break;
            default:
                print("chunk.worldType����");
                break;
        }

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



    //��Ѩ����������
    float GetCaveNoise(int _x, int _y, int _z)
    {
        return Perlin3D(((float)_x + myposition.x) * noise3d_scale, ((float)_y + myposition.y) * noise3d_scale, ((float)_z + myposition.z) * noise3d_scale); // ��100��Ϊ0.1

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
                    float noiseHigh = world.GetTotalNoiseHigh_Biome(x, z, myposition, world.worldSetting.worldtype);


                    //������
                    float noise3d = GetCaveNoise(x, y, z);


                    //���ݻ���
                    byte terrainFace = GetBlock(x, y - 1, z).voxelType;


                    //ɳĮ����
                    //float noise_desery = GetSmoothNoise_Desert(x, z);


                    //�жϻ���
                    //0~3�㲻׼���ɿ�
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
                    //��������
                    else if (y > noiseHigh && y > world.terrainLayerProbabilitySystem.sea_level)
                    {

                        //����һ��
                        if (y - 1 < noiseHigh)
                        {

                            //�ݵز�
                            if (terrainFace != VoxelData.Sand && terrainFace != VoxelData.Air && terrainFace != VoxelData.Snow)
                            {

                                //��ľ��
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
                                //�ݵ�ѩ��Ƭ
                                else if (y > world.terrainLayerProbabilitySystem.Snow_Level - 10)
                                {
                                    UpdateBlock(x, y, z, VoxelData.SnowPower);
                                }
                                else
                                {

                                    UpdateBlock(x, y, z, VoxelData.Air) ;

                                }
                            }

                            //ѩ�ز��������ѩ��Ƭ
                            else if (terrainFace == VoxelData.Snow && GetProbability(50))
                            {
                                UpdateBlock(x, y, z, VoxelData.SnowPower) ;
                            }


                            //ɳ�Ӳ�
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

                    //�ж�ˮ��
                    else if (y > noiseHigh && y - 1 < world.terrainLayerProbabilitySystem.sea_level)
                    {

                        UpdateBlock(x, y, z, VoxelData.Water) ;

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

                                UpdateBlock(x, y, z, VoxelData.Sand) ;

                            }

                            //��ԭ����
                            else
                            {
                                //100ѩ��
                                if (y > world.terrainLayerProbabilitySystem.Snow_Level)
                                {
                                    UpdateBlock(x, y, z, VoxelData.Snow) ;
                                }

                                //90~100��������ѩ��
                                else if ((y > (world.terrainLayerProbabilitySystem.Snow_Level - 10f)) && GetProbability(70))
                                {
                                    UpdateBlock(x, y, z, VoxelData.Snow) ;
                                }



                                //���ں�ƽ��
                                else if (y > world.terrainLayerProbabilitySystem.sea_level)
                                {

                                    //�Ƿ��Ǿ�˿��
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


                        //�������ж�
                        else if (y > noiseHigh - 7)
                        {
                            //ɳĮ�ж�
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
                            //ɳĮ�ж�
                            if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Dessert)
                            {
                                UpdateBlock(x, y, z, VoxelData.Sand) ;
                            }
                            else
                            {
                                UpdateBlock(x, y, z, VoxelData.Soil) ;
                            }


                        }





                        //��
                        else if (noise3d < GetVaveWidth(y))
                        {

                            UpdateBlock(x, y, z, VoxelData.Air) ;

                        }

                        //�����ж�
                        else
                        {

                            //ú̿
                            if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Coal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Stone) ;
                                Coals.Enqueue(new Vector3(x, y, z));

                            }

                            //��
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Iron))
                            {

                                UpdateBlock(x, y, z, VoxelData.Iron) ;

                            }

                            //��
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Gold))
                            {

                                UpdateBlock(x, y, z, VoxelData.Gold) ;

                            }

                            //���ʯ
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Blue_Crystal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Blue_Crystal) ;

                            }

                            //��ʯ
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


        //������
        CreateTree(0, 0);

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

        if (isSaving)
        {
            LoadData(EditList);
        }

        //����world��create
        world.WaitToCreateMesh.Enqueue(this);

    }

    //��ƽ̹����
    void CreateData_SuperPlain()
    {
        //��һ��chunk���б���
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


        //����world��create
        world.WaitToCreateMesh.Enqueue(this);
        //UpdateChunkMesh_WithSurround(false, true);
    }

    //ɳĮȺϵ
    void CreateData_Dessert()
    {
        //��һ��chunk���б���
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {



                    //��������
                    //float noiseHigh = GetTotalNoiseHigh(x, z);
                    float noiseHigh = world.GetTotalNoiseHigh_Biome(x, z, myposition, world.worldSetting.worldtype);
                    //������
                    float noise3d = GetCaveNoise(x, y, z);

                    //���ݻ���
                    byte terrainFace = GetBlock(x, y - 1, z).voxelType;


                    //�жϻ���
                    //0~3�㲻׼���ɿ�
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
                    //��������
                    else if (y > noiseHigh && y > world.terrainLayerProbabilitySystem.sea_level)
                    {

                        //����һ��
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
                    //����֮��
                    else
                    {

                        //�ر�
                        if ((y + 1) > noiseHigh)
                        {
                            UpdateBlock(x, y, z, VoxelData.Sand) ;
                        }


                        //�������ж�
                        else if (y > noiseHigh - 7)
                        {
                            UpdateBlock(x, y, z, VoxelData.Sand) ;


                        }
                        else if (y >= (noiseHigh - 10) && y <= (noiseHigh - 7) && GetProbability(50))
                        {
                            //ɳĮ�ж�
                            if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Dessert)
                            {
                                UpdateBlock(x, y, z, VoxelData.Sand) ;
                            }
                            else
                            {
                                UpdateBlock(x, y, z, VoxelData.Soil) ;
                            }


                        }
                        //��
                        else if (noise3d < GetVaveWidth(y))
                        {

                            UpdateBlock(x, y, z, VoxelData.Air) ;

                        }

                        //�����ж�
                        else
                        {

                            //ú̿
                            if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Coal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Stone) ;
                                Coals.Enqueue(new Vector3(x, y, z));

                            }

                            //��
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Iron))
                            {

                                UpdateBlock(x, y, z, VoxelData.Iron) ;

                            }

                            //��
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Gold))
                            {

                                UpdateBlock(x, y, z, VoxelData.Gold) ;

                            }

                            //���ʯ
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Blue_Crystal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Blue_Crystal) ;

                            }

                            //��ʯ
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


        //������
        //CreateTree();

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

        if (isSaving)
        {
            LoadData(EditList);
        }

        //����world��create
        world.WaitToCreateMesh.Enqueue(this);
    }

    //����Ⱥϵ
    void CreateData_Marsh()
    {


        //��һ��chunk���б���
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    //��������
                    //float noiseHigh = GetTotalNoiseHigh(x, z);
                    float noiseHigh = world.GetTotalNoiseHigh_Biome(x, z, myposition, world.worldSetting.worldtype);


                    //������
                    float noise3d = GetCaveNoise(x, y, z);

                    //���ݻ���
                    byte terrainFace = GetBlock(x, y - 1, z).voxelType;



                    //�жϻ���
                    //0~3�㲻׼���ɿ�
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
                    //��������
                    else if (y > noiseHigh && y > world.terrainLayerProbabilitySystem.sea_level)
                    {

                        //����һ��
                        if (y - 1 < noiseHigh)
                        {

                            //�ݵز�
                            if (terrainFace != VoxelData.Sand && terrainFace != VoxelData.Air && terrainFace != VoxelData.Snow)
                            {

                                //��ľ��
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
                                //�ݵ�ѩ��Ƭ
                                else if (y > world.terrainLayerProbabilitySystem.Snow_Level - 10)
                                {
                                    UpdateBlock(x, y, z, VoxelData.SnowPower);
                                }
                                else
                                {

                                    UpdateBlock(x, y, z, VoxelData.Air) ;

                                }
                            }

                            //ѩ�ز��������ѩ��Ƭ
                            else if (terrainFace == VoxelData.Snow && GetProbability(50))
                            {
                                UpdateBlock(x, y, z, VoxelData.SnowPower) ;
                            }


                            //ɳ�Ӳ�
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

                    //�ж�ˮ��
                    else if (y > noiseHigh && y - 1 < world.terrainLayerProbabilitySystem.sea_level)
                    {

                        UpdateBlock(x, y, z, VoxelData.Water) ;

                    }

                    //����֮��
                    else
                    {

                        //�ر�
                        if ((y + 1) > noiseHigh)
                        {

                            UpdateBlock(x, y, z, VoxelData.Mycelium) ;
                        }


                        //�������ж�
                        else if (y > noiseHigh - 7)
                        {
                            UpdateBlock(x, y, z, VoxelData.Soil) ;


                        }
                        else if (y >= (noiseHigh - 10) && y <= (noiseHigh - 7) && GetProbability(50))
                        {

                            UpdateBlock(x, y, z, VoxelData.Soil) ;


                        }





                        //��
                        else if (noise3d < GetVaveWidth(y))
                        {

                            UpdateBlock(x, y, z, VoxelData.Air) ;

                        }

                        //�����ж�
                        else
                        {

                            //ú̿
                            if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Coal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Stone) ;
                                Coals.Enqueue(new Vector3(x, y, z));

                            }

                            //��
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Iron))
                            {

                                UpdateBlock(x, y, z, VoxelData.Iron) ;

                            }

                            //��
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Gold))
                            {

                                UpdateBlock(x, y, z, VoxelData.Gold) ;

                            }

                            //���ʯ
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Blue_Crystal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Blue_Crystal) ;

                            }

                            //��ʯ
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


        //������
        //[ȷ��XZ]xoz�����ѡ��5����
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

                        UpdateBlock(random_x, random_y + i, random_z, VoxelData.Wood) ;

                    }

                }

                //������Ҷ
                BuildLeaves(random_x, random_y + random_Tree_High, random_z);

            }


            //Debug.Log($"{random_x}, {random_y}, {random_z}");
        }

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

        if (isSaving)
        {
            LoadData(EditList);
        }

        //����world��create
        world.WaitToCreateMesh.Enqueue(this);

    }


    //����Ⱥϵ
    void CreateData_Forest()
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
                    float noiseHigh = world.GetTotalNoiseHigh_Biome(x, z, myposition, world.worldSetting.worldtype);


                    //������
                    float noise3d = GetCaveNoise(x, y, z);

                    float _sealevel = 59;


                    //���ݻ���
                    byte terrainFace = GetBlock(x, y - 1, z).voxelType;

                    //ɳĮ����
                    //float noise_desery = GetSmoothNoise_Desert(x, z);


                    //�жϻ���
                    //0~3�㲻׼���ɿ�
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
                    //��������
                    else if (y > noiseHigh && y > _sealevel)
                    {

                        //����һ��
                        if (y - 1 < noiseHigh)
                        {

                            //�ݵز�
                            if (terrainFace != VoxelData.Sand && terrainFace != VoxelData.Air && terrainFace != VoxelData.Snow)
                            {

                                //��ľ��
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
                                //�ݵ�ѩ��Ƭ
                                else if (y > world.terrainLayerProbabilitySystem.Snow_Level - 10)
                                {
                                    UpdateBlock(x, y, z, VoxelData.SnowPower) ;
                                }
                                else
                                {

                                    UpdateBlock(x, y, z, VoxelData.Air) ;

                                }
                            }

                            //ѩ�ز��������ѩ��Ƭ
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

                    //�ж�ˮ��
                    else if (y > noiseHigh && y - 1 < _sealevel)
                    {

                        UpdateBlock(x, y, z, VoxelData.Water) ;

                    }

                    //����֮��
                    else
                    {

                        //�ر�
                        if ((y + 1) > noiseHigh)
                        {

                            //100ѩ��
                            if (y > world.terrainLayerProbabilitySystem.Snow_Level)
                            {
                                UpdateBlock(x, y, z, VoxelData.Snow) ;
                            }

                            //90~100��������ѩ��
                            else if ((y > (world.terrainLayerProbabilitySystem.Snow_Level - 10f)) && GetProbability(70))
                            {
                                UpdateBlock(x, y, z, VoxelData.Snow) ;
                            }



                            //���ں�ƽ��
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


                        //�������ж�
                        else if (y > noiseHigh - 7)
                        {
                            //ɳĮ�ж�
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
                            //ɳĮ�ж�
                            if (world.GetBiomeType(x, z, myposition) == VoxelData.Biome_Dessert)
                            {
                                UpdateBlock(x, y, z, VoxelData.Sand) ;
                            }
                            else
                            {
                                UpdateBlock(x, y, z, VoxelData.Soil) ;
                            }


                        }





                        //��
                        else if (noise3d < GetVaveWidth(y))
                        {

                            UpdateBlock(x, y, z, VoxelData.Air) ;

                        }

                        //�����ж�
                        else
                        {

                            //ú̿
                            if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Coal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Stone) ;
                                Coals.Enqueue(new Vector3(x, y, z));

                            }

                            //��
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Iron))
                            {

                                UpdateBlock(x, y, z, VoxelData.Iron) ;

                            }

                            //��
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Gold))
                            {

                                UpdateBlock(x, y, z, VoxelData.Gold) ;

                            }

                            //���ʯ
                            else if (GetProbabilityTenThousandth(world.terrainLayerProbabilitySystem.Random_Blue_Crystal))
                            {

                                UpdateBlock(x, y, z, VoxelData.Blue_Crystal) ;

                            }

                            //��ʯ
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


        //������
        //[ȷ��XZ]xoz�����ѡ��5����
        int _Forest_treecount = 25;
        while (_Forest_treecount-- != 0)
        {

            int random_x = rand.Next(2, VoxelData.ChunkWidth - 2);
            int random_z = rand.Next(2, VoxelData.ChunkWidth - 2);
            int random_y = VoxelData.ChunkHeight;
            int random_Tree_High = rand.Next(7, 15);

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

                        UpdateBlock(random_x, random_y + i, random_z, VoxelData.Wood) ;

                    }

                }

                //������Ҷ
                BuildLeaves(random_x, random_y + random_Tree_High, random_z);

            }


            //Debug.Log($"{random_x}, {random_y}, {random_z}");
        }

        //����ú̿
        foreach (var item in Coals)
        {

            CreateCoal((int)item.x, (int)item.y, (int)item.z);

        }

        ////��������
        //foreach (var item in Bamboos)
        //{

        //    CreateBamboo((int)item.x, (int)item.y, (int)item.z);

        //}

        if (isSaving)
        {
            LoadData(EditList);
        }

        //����world��create
        world.WaitToCreateMesh.Enqueue(this);

    }



    //---------------------------------- Tree ----------------------------------------

    //tree
    void CreateTree(int _x, int _z)
    {
        //����Ⱥϵ
        if (world.GetBiomeType(_x, _z, myposition) == VoxelData.Biome_Forest)
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

                            UpdateBlock(random_x, random_y + i, random_z, VoxelData.Wood) ;

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

                            UpdateBlock(random_x, random_y + i, random_z, VoxelData.Wood) ;

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

            //����ѩ���ж�
            if (((_y + 1) >= world.terrainLayerProbabilitySystem.Snow_Level - 10f) && ((_y + 2) < VoxelData.ChunkHeight))
            {
                UpdateBlock(_x, _y + 2, _z + 1, VoxelData.SnowPower);
                UpdateBlock(_x - 1, _y + 2, _z, VoxelData.SnowPower);
                UpdateBlock(_x, _y + 2, _z, VoxelData.SnowPower);
                UpdateBlock(_x + 1, _y + 2, _z, VoxelData.SnowPower);
                UpdateBlock(_x, _y + 2, _z - 1, VoxelData.SnowPower);
            }

        }

        //�ڶ���
        CreateLeaves(_x - 1, _y, _z);
        CreateLeaves(_x + 1, _y, _z);
        CreateLeaves(_x, _y, _z - 1);
        CreateLeaves(_x, _y, _z + 1);

        //����ѩ���ж�
        if (((_y) >= world.terrainLayerProbabilitySystem.Snow_Level - 10f) && ((_y + 1) < VoxelData.ChunkHeight) && GetBlock(_x - 1, _y + 1, _z).voxelType != VoxelData.Leaves)
        {

            UpdateBlock(_x - 1, _y + 1, _z, VoxelData.SnowPower);
            UpdateBlock(_x + 1, _y + 1, _z, VoxelData.SnowPower);
            UpdateBlock(_x, _y + 1, _z - 1, VoxelData.SnowPower);
            UpdateBlock(_x, _y + 1, _z + 1, VoxelData.SnowPower);

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
                if (GetBlock(_x, _y - 1, _z).voxelType != VoxelData.Air)
                {

                    if (GetBlock(_x, _y - 1, _z).voxelType == VoxelData.Mycelium || GetBlock(_x, _y - 1, _z).voxelType == VoxelData.Grass || GetBlock(_x, _y - 1, _z).voxelType == VoxelData.Soil && GetBlock(_x, _y - 2, _z).voxelType != VoxelData.Leaves)
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
        if (GetBlock(_x, _y, _z).voxelType == VoxelData.Stone)
            UpdateBlock(_x, _y, _z, VoxelData.Coal);


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

                GetBlock(x, y + temp, z).voxelType = VoxelData.Bamboo;

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

                if (GetBlock(_x, y - 1, _z).voxelType == VoxelData.Water)
                {

                    return true;

                }

            }

        }

        return false;

    }



    //[ˮ������]
    //����Լ���ˮ
    //ǰ������������ǿ�����������Ǳ��ˮ
    //public void Always_updateWater()
    //{

    //    //���Ż� - ������ж���ΧһȦ����ˮ����ִ�м��ֱ������

    //    ClearMeshData();

    //    //ˢ���Լ�
    //    for (int _y = 0; _y < VoxelData.ChunkHeight; _y++)
    //    {

    //        for (int _x = 0; _x < VoxelData.ChunkWidth; _x++)
    //        {

    //            for (int _z = 0; _z < VoxelData.ChunkWidth; _z++)
    //            {

    //                //�޸�VoxelMap
    //                if (hasExec_isHadupdateWater)
    //                    _updateWater(_x, _y, _z);

    //                // �ǿ��� - ��Ⱦ
    //                // ˮ���� - ��Ⱦ
    //                if (world.blocktypes[GetBlock(_x, _y, _z).voxelType].DrawMode != DrawMode.Air)
    //                    UpdateMeshData(new Vector3(_x, _y, _z));

    //            }

    //        }

    //    }



    //    hasExec_isHadupdateWater = true;

    //    //��ӵ�world����Ⱦ����
    //    isReadyToRender = true;

    //    //������Ⱦ�߳�
    //    if (world.RenderLock)
    //    {

    //        world.WaitToRender_temp.Enqueue(this);
    //        //print($"{world.GetChunkLocation(myposition)}�����������temp");

    //    }
    //    else
    //    {

    //        //print($"{world.GetChunkLocation(myposition)}���");
    //        world.WaitToRender.Enqueue(this);

    //    }

    //}


    //void _updateWater(int _x, int _y,int _z)
    //{

    //    //ֻ���Լ���ˮ�������ִ��
    //    if (GetBlock(_x, _y, _z).voxelType == VoxelData.Water)
    //    {

    //        //����������
    //        for (int _p = 0; _p < 5; _p++)
    //        {

    //            //�������
    //            if (isOutOfRange(new Vector3(_x, _y, _z) + VoxelData.faceChecks_WaterFlow[_p]))
    //            {

    //                //�ܻ�ȡ������Chunk
    //                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks_WaterFlow[_p], out Chunk chunktemp))
    //                {

    //                    Vector3 directlocation = GetDirectChunkVoxelMapLocation(new Vector3(_x, _y, _z) + VoxelData.faceChecks_WaterFlow[_p]);

    //                    if (chunktemp.GetBlock((int)directlocation.x, (int)directlocation.y, (int)directlocation.z).voxelType == VoxelData.Air)
    //                    {

    //                        chunktemp.UpdateBlock((int)directlocation.x, (int)directlocation.y, (int)directlocation.z).voxelType = VoxelData.Water;

    //                        //������ˮ�����ɺ�ˮ����
    //                        if (chunktemp.iHaveWater == false)
    //                        {

    //                            chunktemp.iHaveWater = true;

    //                        }

    //                        hasExec_isHadupdateWater = false;

    //                    }

    //                }


    //            }

    //            //û����
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




    //���ⷽ��仯
    void updateSomeBlocks(int _x, int _y,int _z)
    {

        //�����ж�
        //���ܸ��յķ���(��ľ�� + ���� + ϸѩ) 
        if (GetBlock(_x, _y - 1, _z).voxelType == VoxelData.Air)
        {
            //���Լ��ǲ������յķ���
            if (GetBlock(_x, _y, _z).voxelType == VoxelData.Bush || GetBlock(_x, _y, _z).voxelType == VoxelData.Bamboo || GetBlock(_x, _y, _z).voxelType == VoxelData.SnowPower)
            {

                UpdateBlock(_x, _y, _z, VoxelData.Air);
            }

            //��
            if (GetBlock(_x, _y, _z).voxelType == VoxelData.Door_Down)
            {

                UpdateBlock(_x, _y, _z, VoxelData.Air);
                UpdateBlock(_x, _y + 1, _z, VoxelData.Air);
            }
        }

        //����
        if (GetBlock(_x, _y, _z).voxelType == VoxelData.Door_Up && GetBlock(_x, _y - 1, _z).voxelType != VoxelData.Door_Down)
        {
            UpdateBlock(_x, _y, _z, VoxelData.Air);
        }

        //����
        if (GetBlock(_x, _y, _z).voxelType == VoxelData.Door_Down && GetBlock(_x, _y + 1, _z).voxelType == VoxelData.Air)
        {

            UpdateBlock(_x, _y + 1, _z, VoxelData.Door_Up);
        }




    }

    //���û��ƻ�ʱ��仯�����ⷽ��
    public void EditForSomeBlocks(Vector3 _pos, byte _target)
    {
        //���������ţ�������������ʧ
        if (_target == VoxelData.Air && GetBlock((int)_pos.x, (int)_pos.y - 1, (int)_pos.z).voxelType == VoxelData.Door_Down)
        {
            UpdateBlock((int)_pos.x, (int)_pos.y - 1, (int)_pos.z, VoxelData.Air);
        }

        //������ש�ϳ�һ��ľ��
        //if (_target == VoxelData.HalfBrick && voxelMap[(int)_pos.x, (int)_pos.y - 1, (int)_pos.z].voxelType == VoxelData.HalfBrick)
        //{
        //    voxelMap[(int)_pos.x, (int)_pos.y, (int)_pos.z].voxelType = VoxelData.Air;
        //    voxelMap[(int)_pos.x, (int)_pos.y - 1, (int)_pos.z].voxelType = VoxelData.WoodenPlanks;
        //}
    }

    //------------------------------------------------------------------------------------






    //---------------------------------- Mesh���� ----------------------------------------

    //��ʼ���� 
    public void UpdateChunkMesh_WithSurround() //�������أ������дĬ��Ϊfalse
    {

        UpdateChunkMesh_WithSurround(false, false);

    }


    //����1��true������ܱ�����
    //����2��true����ʹ�ö��߳�
    public void UpdateChunkMesh_WithSurround(object obj, bool NotNeedThreading)
    {

        bool iscaller = (bool)obj;

        ClearMeshData();

        //ˢ���Լ�
        for (int _y = 0; _y < VoxelData.ChunkHeight; _y++)
        {

            for (int _x = 0; _x < VoxelData.ChunkWidth; _x++)
            {

                for (int _z = 0; _z < VoxelData.ChunkWidth; _z++)
                {

                    //��仯�����ⷽ��
                    updateSomeBlocks(_x, _y, _z);

                    //[�ѷ������ƶ����������߳�ִ��]ˮ������
                    //updateWater();

                    //if (isOutOfRange(x,y,z))
                    //{
                    //    print("");
                    //}

                    //����ǿ�������Ⱦ
                    if (world.blocktypes[GetBlock(_x, _y, _z).voxelType].DrawMode != DrawMode.Air)
                    {
                        UpdateMeshData(new Vector3(_x, _y, _z));
                    }
                        



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


        if (NotNeedThreading)
        {
            CreateMesh();
        }
        else
        {
            
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
        


        if (BaseChunk == true)
        {

            BaseChunk = false;

        }






    }


    //Playerʹ�ã�����ʡȥ�����ܱ߷�����µĸ���
    public void UpdateChunkMesh_WithSurround(Vector3 _pos, object obj, bool NotNeedThreading)
    {

        bool iscaller = (bool)obj;

        ClearMeshData();

        //ˢ���Լ�
        for (int _y = 0; _y < VoxelData.ChunkHeight; _y++)
        {

            for (int _x = 0; _x < VoxelData.ChunkWidth; _x++)
            {

                for (int _z = 0; _z < VoxelData.ChunkWidth; _z++)
                {

                    //��仯�����ⷽ��
                    updateSomeBlocks(_x, _y, _z);

                    //[�ѷ������ƶ����������߳�ִ��]ˮ������
                    //updateWater();

                    //if (isOutOfRange(x,y,z))
                    //{
                    //    print("");
                    //}

                    //����ǿ�������Ⱦ
                    if (world.blocktypes[GetBlock(_x, _y, _z).voxelType].DrawMode != DrawMode.Air)
                    {
                        UpdateMeshData(new Vector3(_x, _y, _z));
                    }




                    //UpdateMeshData(new Vector3(x, y, z));
                }

            }

        }


        //֪ͨ��Χ����ˢ��
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
                    //print($"{world.GetChunkLocation(myposition)}�����������temp");

                }
                else
                {

                    //print($"{world.GetChunkLocation(myposition)}���");
                    world.WaitToRender.Enqueue(this);

                }
            }
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


    // �����༭����
    public void LoadData(List<EditStruct> _EditList)
    {
        //print($"EditData:{_EditList.Count}");

        for (int i = 0; i < _EditList.Count; i++)
        {


            int x = Mathf.FloorToInt(_EditList[i].editPos.x);
            int y = Mathf.FloorToInt(_EditList[i].editPos.y);
            int z = Mathf.FloorToInt(_EditList[i].editPos.z);

            // ���������
            //if (isOutOfRange(x, y, z))
            //{
            //    continue;
            //}

            // ���÷�������
            UpdateBlock(x, y, z, _EditList[i].targetType);
        }

        // ������������
        //UpdateChunkMesh_WithSurround(true, false);

        isSaving = false;
    }



    #region �޸ķ���

    //�༭����
    //���վ�������
    public void EditData(Vector3 pos, byte targetBlocktype) 
    {

        //ClearFInd_Direvtion();
        Vector3 _relaVec = world.GetRelalocation(pos);

        int x = (int)_relaVec.x;
        int y = (int)_relaVec.y;
        int z = (int)_relaVec.z;

        //��ֹ����
        if (y >= VoxelData.ChunkHeight - 2)
        {

            return;

        }

        //if (isOutOfRange(x,y,z))
        //{
        //    return;
        //}

        UpdateBlock(x, y, z, targetBlocktype);

        //�жϳ���
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

            // ���������
            //if (isOutOfRange(_x, _y, _z))
            //{
            //    continue;
            //}

            //�Ǳ�����������
            if (world.GetChunkLocation(_EditList[i].editPos) != world.GetChunkLocation(myposition))
            {
                //Debug.Log($"edit:{world.GetChunkLocation(_EditList[i].editPos)}, myposition:{world.GetChunkLocation(myposition)}");
                continue;
            }

            //����Ҳ����
            if (thisType == VoxelData.BedRock)
            {
                continue;
            }


            //������
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
            

            // ���÷������� 
            UpdateBlock(_x, _y, _z, _EditList[i].targetType);
        }

        // ������������
        UpdateChunkMesh_WithSurround(true, false);

    }





    #endregion




    //������Ҹ��µľ��巽��
    // ������Ҹ��µľ��巽��
    public void UpdateEditNumber(Vector3 RealPos, byte targetBlocktype)
    {
        // ���޸�ϸ��������World��
        // ת��RealPosΪ����Vector3�Ա������ֵ��key
        Vector3 intPos = new Vector3((int)RealPos.x, (int)RealPos.y, (int)RealPos.z);

        // �����Ƿ��Ѿ�������ͬ��editPos
        EditStruct existingEdit = world.EditNumber.Find(edit => edit.editPos == intPos);

        if (existingEdit != null)
        {
            // ������ڣ�����targetType
            existingEdit.targetType = targetBlocktype;
        }
        else
        {
            // ��������ڣ�����µ�EditStruct
            world.EditNumber.Add(new EditStruct(intPos, targetBlocktype));
        }
    }



    //�����ɵ��ж�
    //�Ƿ�����----Y������----N����
    //�����߽�----Ҳ����N
    bool CheckVoxel(Vector3 pos, int _p)
    {

        int _Targetx = Mathf.FloorToInt(pos.x);
        int _Targety = Mathf.FloorToInt(pos.y);
        int _Targetz = Mathf.FloorToInt(pos.z);


        
      //print($"{x},{y},{z}");


        //���Ŀ�����
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

                        return !world.�Ƿ�����Chunk����;

                    }

                }


                //Back
                if (_Targetz < 0)
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

                        return !world.�Ƿ�����Chunk����;

                    }


                }


                //Left
                if (_Targetx < 0)
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

                        return !world.�Ƿ�����Chunk����;

                    }

                }


                //Right
                if (_Targetx > VoxelData.ChunkWidth - 1)
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

                        return !world.�Ƿ�����Chunk����;

                    }

                }

            }

            //Up����Ҫ����

            //Down:���²�һ�ɲ�����
            if (_Targety < 0)
            {

                return true;

            }

            //else:�Լ��ǲ��ǿ���
            if (GetBlock(_Targetx - (int)VoxelData.faceChecks[_p].x, _Targety - (int)VoxelData.faceChecks[_p].y, _Targetz - (int)VoxelData.faceChecks[_p].z).voxelType == VoxelData.Air || GetBlock(_Targetx - (int)VoxelData.faceChecks[_p].x, _Targety - (int)VoxelData.faceChecks[_p].y, _Targetz - (int)VoxelData.faceChecks[_p].z).voxelType == VoxelData.Water)
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

    //�����Լ���Ŀ������ͣ��ж��Ƿ�������
    //false��������
    //_targetRelativeY:1Ŀ�����Ϸ���-1Ŀ�����·���0Ϊͬһƽ��
    bool CheckSelfAndTarget(byte _self, byte _target, int _p)
    {

        //�ȼ����Ҫ���Ķ�����
        if (world.blocktypes[_self].OtherFaceCheck.Count != 0)
        {
            int i = 0;
            foreach (FaceCheckMode temp in world.blocktypes[_self].OtherFaceCheck)
            {
                
                //��鷽��
                if (temp.FaceDirect == _p)
                {
                    //print($"���ͣ�{_self} , temp��{i}");
                    //�������
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



        //��שר��
        //if (_self == VoxelData.SnowPower && world.blocktypes[_target].isSolid)
        //{
        //    //����������Լ��Ϸ������ɣ�����������
        //    if (_targetRelativeY == 1)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}
        ////Ŀ���ǰ�ש
        //if (_target == VoxelData.SnowPower)
        //{
        //    //����Լ��ǰ�ש
        //    if (_self == VoxelData.SnowPower)
        //    {
        //        //�������Լ�ʱ�����ɣ���������
        //        if (Mathf.Abs(_targetRelativeY) != 1)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    //����Լ��ǹ���
        //    else if (world.blocktypes[_self].isSolid)
        //    {
        //        //�Ϸ��ǰ�שʱ������, ��������
        //        if (_targetRelativeY == 1)
        //        {
        //            return false;

        //        }
        //        else
        //        {
        //            return true;
        //        }

        //    }
        //    //������������
        //    else
        //    {
        //        return true;
        //    }
        //}

        //ͨ���ж�
        //�ж��ǲ���͸������
        if (world.blocktypes[_target].isTransparent || _target == VoxelData.Air)
        {

            return false;

        }

        //��һ�ε��ڿ������棬ˮ��Ӵ�����ҲҪ������
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


        //print("��������ֵ���");
        return world.blocktypes[_self].isSolid;

        // ���Ŀ���ǿ�������͸�����������������
        //if (world.blocktypes[_target].isTransparent || _target == VoxelData.Air)
        //{

        //    return false;

        //}

        ////����Լ���Ŀ����ȣ�������
        //if (_self == _target)
        //{
        //    return true;
        //}

        ////����Լ���ˮ
        //// Ŀ����Transparent������
        ////Ŀ����ˮ���߹��壬������
        //if (_self == VoxelData.Water)
        //{

        //    if (world.blocktypes[_target].isSolid)
        //    {

        //        return true;

        //    }

        //}

        ////����Լ���ѩ��Ƭ
        //if (_self == VoxelData.SnowPower)
        //{


        //    //Ŀ����ˮ������
        //    //Ŀ����transparent������
        //    if (world.blocktypes[_target].isTransparent || _target == VoxelData.Water || _target == _self)
        //    {
        //        return true;
        //    }

        //    //Ŀ���ǹ��壬������
        //    if (world.blocktypes[_target].isSolid)
        //    {
        //        return false;
        //    }



        //}

        ////����Լ��ǹ���
        ////Ŀ���ǹ��壬������
        ////����������
        //if (world.blocktypes[_self].isSolid)
        //{

        //    return false;

        //}



        //return true;

    }




    //�����У���˳���ж�������ɷ���
    //����mesh��Ĳ���
    //_calledFrom = 0 ����UpdateChunkMesh_WithSurround
    //_calledFrom = 1 ����UpdateWater
    void UpdateMeshData(Vector3 pos)
    {

        //if (isOutOfRange(pos))
        //{
        //    print($"UpdateMeshData���磬pos = {pos}");
        //    return;
        //}

        int _x = (int)pos.x;
        int _y = (int)pos.y;
        int _z = (int)pos.z;

        byte blockID = GetBlock(_x, _y, _z).voxelType;


        //�������ģʽ
        switch (world.blocktypes[blockID].DrawMode)
        {
            // Bush����ģʽ
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


            // SnowPower����ģʽ
            case DrawMode.SnowPower:

                //�ж�������
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


            //ˮ�����ģʽ
            case DrawMode.Water: 



                //�ж�������
                for (int p = 0; p < 6; p++)
                {

                    //���Ի���
                    if (!CheckVoxel(pos + VoxelData.faceChecks[p], p))
                    {

                        //������·���ˮ���򻻳ɷ������Ⱦ��ʽ
                        if ((GetBlock((int)pos.x, (int)pos.y + 1, (int)pos.z).voxelType == VoxelData.Water || GetBlock((int)pos.x, (int)pos.y - 1, (int)pos.z).voxelType == VoxelData.Water) && p != 2  && GetBlock((int)pos.x, (int)pos.y + 1, (int)pos.z).voxelType != VoxelData.Air)
                        {
                            //�����Ҫ˫�����
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

                        //�����ˮ��
                        else if (GetBlock(_x, _y, _z).voxelType == VoxelData.Water && GetBlock(_x, _y, _z, 2).voxelType == VoxelData.Air)
                        {
                            //���ں��㷨
                            if (p == 2 && GetBlock(_x, _y, _z).up == true)
                            {
                                int _zz = 0;
                                int _xx = 0;

                                //if (x == 15 && z == 9)
                                //{
                                //    print("");
                                //}


                                //�ֲ�����
                                int __z = 0;
                                bool Z�������ϰ��� = false;

                                //���к��õ�ǰ�������̤���ķ�Χ
                                for (_xx = 0; _xx < VoxelData.ChunkWidth; _xx++)
                                {
                                    for (__z = 0; __z < VoxelData.ChunkWidth; __z++)
                                    {




                                        //���������ֹͣ
                                        if (isOutOfRange(_x + _xx, _y, _z + __z))
                                        {
                                            break;
                                        }

                                        //if(_xx == 8){
                                        //    print("");
                                        //} 


                                        //Ŀ����ˮ && Ŀ���Ϸ��ǿ��� && Ŀ���up��true
                                        if ((GetBlock(_x + _xx, _y, _z + __z).voxelType != VoxelData.Water || GetBlock(_x + _xx, _y + 1, _z + __z).voxelType != VoxelData.Air) || (GetBlock(_x + _xx, _y, _z + __z).up == false))
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

                                        //���������ֹͣ
                                        if (_x + _xx == VoxelData.ChunkWidth)
                                        {
                                            break;
                                        }

                                    }



                                }

                                //print($"local��{new Vector3(x, y, z)},���Ϳ�<{_xx},{_zz}>");


                                //���Ӧ�ø���һ�����Ϳ� _xx��_zz



                                //��ֹ�������0
                                if (_xx == 0)
                                {

                                    //int _x = Mathf.FloorToInt((pos + VoxelData.faceChecks[p]).x);
                                    //int _y = Mathf.FloorToInt((pos + VoxelData.faceChecks[p]).y);
                                    //int _z = Mathf.FloorToInt((pos + VoxelData.faceChecks[p]).z);

                                    //�����Ҫ˫�����
                                    if (world.blocktypes[GetBlock(pos).voxelType].GenerateTwoFaceWithAir && GetBlock(pos, p).voxelType == VoxelData.Air)
                                    {
                                        print("������");
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

                                        //����p���ɶ�Ӧ���棬��Ӧ��UV
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


                                    //����p���ɶ�Ӧ���棬��Ӧ��UV
                                    //AddTexture(world.blocktypes[blockID].GetTextureID(p));
                                    uvs.Add(new Vector2(0f, 0f));
                                    uvs.Add(world.ComponentwiseMultiply(new Vector2(0f, 1f), new Vector2(1, _zz)));
                                    uvs.Add(new Vector2(1f, 0f));
                                    uvs.Add(world.ComponentwiseMultiply(new Vector2(1f, 1f), new Vector2(1, _zz)));

                                }
                                else
                                {
                                    //�����Ҫ˫�����
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

                                        //����p���ɶ�Ӧ���棬��Ӧ��UV
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


                                    //����p���ɶ�Ӧ���棬��Ӧ��UV
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
                                //�����Ҫ˫�����
                                if (world.blocktypes[GetBlock(pos).voxelType].GenerateTwoFaceWithAir && GetBlock(pos, p).voxelType == VoxelData.Air)
                                {
                                    vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 0]]);
                                    vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 1]]);
                                    vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 2]]);
                                    vertices.Add(pos + VoxelData.voxelVerts_Water[VoxelData.voxelTris[p, 3]]);

                                    //����p���ɶ�Ӧ���棬��Ӧ��UV
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


                        else
                        {

                            //�����Ҫ˫�����
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


                    }

                }





                break;


            //�Ż���
            case DrawMode.Door:

                //�ж�������
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

            //��ש����
            case DrawMode.HalfBrick:

                //�ж�������
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



            //��ѻ���
            case DrawMode.Torch:

                //�ж�������
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



            //Ĭ��Block����ģʽ
            default: 



                //�ж�������
                for (int p = 0; p < 6; p++)
                {


                    if (!CheckVoxel(pos + VoxelData.faceChecks[p], p))
                    {

                        //�����Ҫ˫�����
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


                        //����p���ɶ�Ӧ���棬��Ӧ��UV
                        //ChangeBlockFacing��������ĳ���
                        AddTexture(world.blocktypes[blockID].GetTextureID(ChangeBlockFacing(p, GetBlock(_x, _y, _z).blockOriented)));




                    }

                }

                break;

        }



    }


    public int ChangeBlockFacing(int _p, int _face)
    {
        // ǰ - �� - �� - ��
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

        InitializeVoxelMap_state();

    }

    public void InitializeVoxelMap_state()
    {
        // ������ά�����е�ÿһ��Ԫ��
        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    // ֻ���� VoxelStruct �ķ�voxelType����
                    //voxelMap[x, y, z].up = true;
                    UpdateBlockDirection(new Vector3(x,y,z),2,true);
                    // �������������Ҫ���õķ���������ԣ�Ҳ�����������������
                    // ���磺 voxelMap[x, y, z].left = true;
                    // ���磺 voxelMap[x, y, z].right = true;
                }
            }
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






    //---------------------------------- �������� ----------------------------------------
    //�޸�Voxel��Ϊ�ڲ��÷���EditBlockΪ�ⲿ�÷�
    private void UpdateBlock(Vector3 _pos, byte _UpdateType)
    {
        int _x = Mathf.FloorToInt(_pos.x);
        int _y = Mathf.FloorToInt(_pos.y);
        int _z = Mathf.FloorToInt(_pos.z);


        //���Ŀ�����
        if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
        {

            print("UpdateBlockTypec�������磡");

        }

        else
        {

            voxelMap[_x, _y, _z].voxelType = _UpdateType;
        }
    }

    private void UpdateBlock(int _x, int _y, int _z, byte _UpdateType)
    {

        //���Ŀ�����
        if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
        {

            //print("UpdateBlockTypec�������磡");

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


        //���Ŀ�����
        if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
        {

            print("UpdateBlockTypec�������磡");

        }

        else
        {

            voxelMap[_x, _y, _z].blockOriented = _orient;
        }
    }

    //UpdateBlockDirection(pos, 2, true)���ǰ�up��Ϊtrue
    private void UpdateBlockDirection(Vector3 _pos, int _direct, bool _bool)
    {
        int _x = Mathf.FloorToInt(_pos.x);
        int _y = Mathf.FloorToInt(_pos.y);
        int _z = Mathf.FloorToInt(_pos.z);


        //���Ŀ�����
        if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
        {

            print("UpdateBlockTypec�������磡");

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




    //���Voxel
    //���������GetBlockҪ���ǵ�(-1,0,-1)���ڽӷ��ʣ�����(0,0,-32)��ȼ�Զ�ķ���(��Ȼ������)
    //��ȡĿ��Chunk�����λ��
    void GetRelativeTargetChunk(int _x, int _y, int _z, out Vector3 TargetChunkDirection, out Vector3 TargeChunkPosition)
    {
        TargetChunkDirection = Vector3.zero;
        TargeChunkPosition = new Vector3(_x, _y, _z);

        float width = VoxelData.ChunkWidth;

        //�������Chunk
        TargetChunkDirection += new Vector3((int)(_x / width), 0f, 0f);
        TargetChunkDirection += new Vector3(0f, 0f, (int)(_z / width));

        // �����������
        if (_x < 0)
        {
            TargeChunkPosition.x = (_x % width + width) % width;
        }
        else if (_x > VoxelData.ChunkWidth - 1)
        {
            TargeChunkPosition.x = _x % width;
        }

        TargeChunkPosition.y = _y; // y ����

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

        //���Ŀ�����
        if (isOutOfRange(_x, _y, _z))
        {
            int _p;

            //Front
            if (_z > VoxelData.ChunkWidth - 1)
            {
                _p = 1;
                //����ܲ鵽
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
                    //print("GetTargetBlockType()��������Ŀ��Chunk");
                    return new VoxelStruct();

                }

            }

            //Back
            if (_z < 0)
            {
                _p = 0;
                //����ܲ鵽
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {
                    return chunktemp.voxelMap[_x, _y, VoxelData.ChunkWidth - 1];

                }
                else
                {
                    //print("GetTargetBlockType()��������Ŀ��Chunk");
                    return new VoxelStruct();

                }


            }

            //Left
            if (_x < 0)
            {
                _p = 4;
                //����ܲ鵽
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {

                    return chunktemp.voxelMap[VoxelData.ChunkWidth - 1, _y, _z];

                }
                else
                {
                    //print("GetTargetBlockType()��������Ŀ��Chunk");
                    return new VoxelStruct();

                }

            }

            //Right
            if (_x > VoxelData.ChunkWidth - 1)
            {
                _p = 5;
                //����ܲ鵽
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {

                    return chunktemp.voxelMap[0, _y, _x];

                }
                else
                {
                    //print("GetTargetBlockType()��������Ŀ��Chunk");
                    return new VoxelStruct();

                }

            }

            //Up����Ҫ����
            //if (_y > VoxelData.ChunkHeight - 1)
            //{
            //    return new VoxelStruct();
            //}

            ////Down
            //if (_y < 0)
            //{

            //    return new VoxelStruct();

            //}

            //print("GetTargetBlockType()�����˵���ȫ���Ҳ���");
            return new VoxelStruct();

        }

        else
        {

            return voxelMap[_x, _y, _z];
        }

    }

    //����
    private VoxelStruct GetBlock(int _x, int _y,int _z)
    {
        //���Ŀ�����
        if (isOutOfRange(_x, _y, _z))
        {
            //���³��粻�ù�
            if (_y < 0 || _y > VoxelData.ChunkWidth - 1)
            {
                return new VoxelStruct();
            }

            //��ȡĿ��Chunk�����λ��
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



    //��ö���Voxel
    //�������GetBlockһ��ֻ��һ��
    private VoxelStruct GetBlock(Vector3 _pos, int _p)
    {
        Vector3 _TargetPos = _pos + VoxelData.faceChecks[_p];
        int _x = (int)_TargetPos.x;
        int _y = (int)_TargetPos.y;
        int _z = (int)_TargetPos.z;

        //���Ŀ�����
        if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
        {
            //Front
            if (_z > VoxelData.ChunkWidth - 1)
            {

                //����ܲ鵽
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {
                    return chunktemp.voxelMap[_x, _y, 0];
                }
                else
                {
                    //print("GetTargetBlockType()��������Ŀ��Chunk");
                    return new VoxelStruct();

                }

            }

            //Back
            if (_z < 0)
            {

                //����ܲ鵽
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {
                    return chunktemp.voxelMap[_x, _y, VoxelData.ChunkWidth - 1];

                }
                else
                {
                    //print("GetTargetBlockType()��������Ŀ��Chunk");
                    return new VoxelStruct();

                }


            }

            //Left
            if (_x < 0)
            {

                //����ܲ鵽
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {

                    return chunktemp.voxelMap[VoxelData.ChunkWidth - 1, _y, _z];

                }
                else
                {
                    //print("GetTargetBlockType()��������Ŀ��Chunk");
                    return new VoxelStruct();

                }

            }

            //Right
            if (_x > VoxelData.ChunkWidth - 1)
            {

                //����ܲ鵽
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {

                    return chunktemp.voxelMap[0, _y, _x];

                }
                else
                {
                    //print("GetTargetBlockType()��������Ŀ��Chunk");
                    return new VoxelStruct();

                }

            }

            //Up����Ҫ����
            //if (_y > VoxelData.ChunkHeight - 1)
            //{
            //    return new VoxelStruct();
            //}

            ////Down
            //if (_y < 0)
            //{

            //    return new VoxelStruct();

            //}

            print("GetTargetBlockType()�����˵���ȫ���Ҳ���");
            return new VoxelStruct();

        }

        else
        {
            
            return voxelMap[_x, _y, _z];
        }
    }

    //����
    private VoxelStruct GetBlock(int _x, int _y, int _z, int _p)
    {

        //���Ŀ�����
        if (_x < 0 || _x > VoxelData.ChunkWidth - 1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
        {
            //Front
            if (_z > VoxelData.ChunkWidth - 1)
            {

                //����ܲ鵽
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {
                    return chunktemp.voxelMap[_x, _y, 0];
                }
                else
                {
                    //print("GetTargetBlockType()��������Ŀ��Chunk");
                    return new VoxelStruct();

                }

            }

            //Back
            if (_z < 0)
            {

                //����ܲ鵽
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {
                    return chunktemp.voxelMap[_x, _y, VoxelData.ChunkWidth - 1];

                }
                else
                {
                    //print("GetTargetBlockType()��������Ŀ��Chunk");
                    return new VoxelStruct();

                }


            }

            //Left
            if (_x < 0)
            {

                //����ܲ鵽
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {

                    return chunktemp.voxelMap[VoxelData.ChunkWidth - 1, _y, _z];

                }
                else
                {
                    //print("GetTargetBlockType()��������Ŀ��Chunk");
                    return new VoxelStruct();

                }

            }

            //Right
            if (_x > VoxelData.ChunkWidth - 1)
            {

                //����ܲ鵽
                if (world.Allchunks.TryGetValue(world.GetChunkLocation(myposition) + VoxelData.faceChecks[_p], out Chunk chunktemp))
                {

                    return chunktemp.voxelMap[0, _y, _x];

                }
                else
                {
                    //print("GetTargetBlockType()��������Ŀ��Chunk");
                    return new VoxelStruct();

                }

            }

           

            print("GetTargetBlockType()�����˵���ȫ���Ҳ���, upDown");
            return new VoxelStruct();

        }

        else
        {

            return voxelMap[_x, _y, _z];
        }
    }


    //--------------------------------------------------------------------------------------

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
                    voxelMap[x, y, z] = new VoxelStruct(); // ��ʼ��ÿ��Ԫ��
                }
            }
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




