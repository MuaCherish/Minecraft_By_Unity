using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class DevelopModeWorld : MonoBehaviour
{
    [Header("Material-��������")]
    public Material material;
    public Material material_Water;
    public BlockType[] blocktypes;


    [Header("World-��Ⱦ����")]
    public int renderSize = 5; //��Ⱦ����뾶,��renderSize*16f



    [Header("Biome-ƽԭ����")]
    //noise2dԽС����������Խ�������������֮����ȸ���Ȼ
    //ֵԽС��������С������Խ����
    //����Խ��Խ��
    public float noise2d_scale_smooth = 0.01f;
    public float noise2d_scale_steep = 0.04f;


    [Header("Chunk-�ֲ�ṹ")]
    public bool isRandomSeed = true;
    public int Seed = 0;
    [Range(0, 60)] public float soil_min = 15;
    [Range(0, 60)] public float soil_max = 55;
    [Range(0, 60)] public float sea_level = 30;
    public int TreeCount = 5;
    public int TreeHigh_min = 5;
    public int TreeHigh_max = 7;


    [Header("���ɸ���(n%)")]
    public float Random_Bush;
    public int Random_Bamboo;
    public float Random_BlueFlower;
    public float Random_WhiteFlower1;
    public float Random_WhiteFlower2;
    public float Random_YellowFlower;


    //ȫ��Chunkλ��
    public Dictionary<Vector3, DevelopModeChunk> Allchunks = new Dictionary<Vector3, DevelopModeChunk>();
    private readonly object Allchunks_Lock = new object();


    //�ȴ���Ӷ���
    //private List<chunkWithsequence> WatingToCreateChunks = new List<chunkWithsequence>();
    private List<Vector3> WatingToCreate_Chunks = new List<Vector3>();



    //Chunks����
    [HideInInspector]
    public GameObject ChunkPATH;


    //Create && Remove Э��
    Coroutine CreateCoroutine;

    //�����
    public ConcurrentQueue<DevelopModeChunk> WaitToRender = new ConcurrentQueue<DevelopModeChunk>();

    Coroutine Render_Coroutine;



    //��Ҫ��
    public ConcurrentQueue<DevelopModeChunk> WaitToCreateMesh = new ConcurrentQueue<DevelopModeChunk>();

    Coroutine Mesh_Coroutine;


    public Rect rect;



    private void Start()
    {
        //֡��
        Application.targetFrameRate = 90;

        //Self
        ChunkPATH = new GameObject();
        ChunkPATH.name = "ChunkPATH";
        ChunkPATH.transform.SetParent(GameObject.Find("Environment").transform);

        //��������
        Seed = Random.Range(0, 100);


        CreateXYChunks();

    }


    private void OnGUI()
    {
        if (GUI.Button(rect, "ˢ��Chunks"))
        {
            FlashAllChunks();
        }
    }



    void FlashAllChunks()
    {
        clearChunks();

        CreateXYChunks();

    }











    void CreateXYChunks()
    {
        for (float x = 0; x < renderSize; x++)
        {
            for (float z = 0; z < renderSize; z++)
            {
                DevelopModeChunk Chunktemp = new DevelopModeChunk(new Vector3(x, 0, z), this);
                Allchunks.Add(new Vector3(x, 0, z), Chunktemp);
            }
        }
    }


    void clearChunks()
    {
        // ���� parentObject �µ������Ӷ���
        foreach (Transform child in ChunkPATH.transform)
        {
            // ����ÿһ���Ӷ���
            GameObject.Destroy(child.gameObject);
        }

        Allchunks.Clear();
    }



    //Vector3 --> ����������
    public Vector3 GetChunkLocation(Vector3 vec)
    {

        return new Vector3((vec.x - vec.x % VoxelData.ChunkWidth) / VoxelData.ChunkWidth, 0, (vec.z - vec.z % VoxelData.ChunkWidth) / VoxelData.ChunkWidth);

    }


    public Vector3 GetRealChunkLocation(Vector3 vec)
    {

        return new Vector3(16f * ((vec.x - vec.x % VoxelData.ChunkWidth) / VoxelData.ChunkWidth), 0, 16f * ((vec.z - vec.z % VoxelData.ChunkWidth) / VoxelData.ChunkWidth));

    }

    //Vector3 --> ���������
    public DevelopModeChunk GetChunkObject(Vector3 pos)
    {

        Allchunks.TryGetValue(GetChunkLocation(pos), out DevelopModeChunk chunktemp);
        return chunktemp;

    }


    //Vector3 --> ��������������
    public Vector3 GetRelalocation(Vector3 vec)
    {

        return new Vector3(Mathf.FloorToInt(vec.x % VoxelData.ChunkWidth), Mathf.FloorToInt(vec.y), Mathf.FloorToInt(vec.z % VoxelData.ChunkWidth));

    }


    //���ط�������
    public byte GetBlockType(Vector3 pos)
    {

        Allchunks.TryGetValue(GetChunkLocation(pos), out DevelopModeChunk chunktemp);

        //��������ˢ������
        //if (chunktemp == null)
        //{
        //    return VoxelData.notChunk;
        //}

        //�����������ڣ���Yֵ̫��

        if ((int)GetRelalocation(pos).y >= VoxelData.ChunkHeight)
        {

            //isBlock = false;
            //isnearblock = false;
            return 255;

        }

        byte block_type = chunktemp.voxelMap[(int)GetRelalocation(pos).x, (int)GetRelalocation(pos).y, (int)GetRelalocation(pos).z];


        return block_type;
    }

}

