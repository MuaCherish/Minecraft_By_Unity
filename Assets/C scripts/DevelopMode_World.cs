using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class DevelopModeWorld : MonoBehaviour
{
    [Header("Material-方块类型")]
    public Material material;
    public Material material_Water;
    public BlockType[] blocktypes;


    [Header("World-渲染设置")]
    public int renderSize = 5; //渲染区块半径,即renderSize*16f



    [Header("Biome-平原参数")]
    //noise2d越小，噪声拉的越长，地形与地形之间过度更自然
    //值越小，噪声缩小，地形越紧凑
    //所以越大越好
    public float noise2d_scale_smooth = 0.01f;
    public float noise2d_scale_steep = 0.04f;


    [Header("Chunk-分层结构")]
    public bool isRandomSeed = true;
    public int Seed = 0;
    [Range(0, 60)] public float soil_min = 15;
    [Range(0, 60)] public float soil_max = 55;
    [Range(0, 60)] public float sea_level = 30;
    public int TreeCount = 5;
    public int TreeHigh_min = 5;
    public int TreeHigh_max = 7;


    [Header("生成概率(n%)")]
    public float Random_Bush;
    public int Random_Bamboo;
    public float Random_BlueFlower;
    public float Random_WhiteFlower1;
    public float Random_WhiteFlower2;
    public float Random_YellowFlower;


    //全部Chunk位置
    public Dictionary<Vector3, DevelopModeChunk> Allchunks = new Dictionary<Vector3, DevelopModeChunk>();
    private readonly object Allchunks_Lock = new object();


    //等待添加队列
    //private List<chunkWithsequence> WatingToCreateChunks = new List<chunkWithsequence>();
    private List<Vector3> WatingToCreate_Chunks = new List<Vector3>();



    //Chunks父级
    [HideInInspector]
    public GameObject ChunkPATH;


    //Create && Remove 协程
    Coroutine CreateCoroutine;

    //必须的
    public ConcurrentQueue<DevelopModeChunk> WaitToRender = new ConcurrentQueue<DevelopModeChunk>();

    Coroutine Render_Coroutine;



    //必要的
    public ConcurrentQueue<DevelopModeChunk> WaitToCreateMesh = new ConcurrentQueue<DevelopModeChunk>();

    Coroutine Mesh_Coroutine;


    public Rect rect;



    private void Start()
    {
        //帧数
        Application.targetFrameRate = 90;

        //Self
        ChunkPATH = new GameObject();
        ChunkPATH.name = "ChunkPATH";
        ChunkPATH.transform.SetParent(GameObject.Find("Environment").transform);

        //设置种子
        Seed = Random.Range(0, 100);


        CreateXYChunks();

    }


    private void OnGUI()
    {
        if (GUI.Button(rect, "刷新Chunks"))
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
        // 遍历 parentObject 下的所有子对象
        foreach (Transform child in ChunkPATH.transform)
        {
            // 销毁每一个子对象
            GameObject.Destroy(child.gameObject);
        }

        Allchunks.Clear();
    }



    //Vector3 --> 大区块坐标
    public Vector3 GetChunkLocation(Vector3 vec)
    {

        return new Vector3((vec.x - vec.x % VoxelData.ChunkWidth) / VoxelData.ChunkWidth, 0, (vec.z - vec.z % VoxelData.ChunkWidth) / VoxelData.ChunkWidth);

    }


    public Vector3 GetRealChunkLocation(Vector3 vec)
    {

        return new Vector3(16f * ((vec.x - vec.x % VoxelData.ChunkWidth) / VoxelData.ChunkWidth), 0, 16f * ((vec.z - vec.z % VoxelData.ChunkWidth) / VoxelData.ChunkWidth));

    }

    //Vector3 --> 大区块对象
    public DevelopModeChunk GetChunkObject(Vector3 pos)
    {

        Allchunks.TryGetValue(GetChunkLocation(pos), out DevelopModeChunk chunktemp);
        return chunktemp;

    }


    //Vector3 --> 区块里的相对坐标
    public Vector3 GetRelalocation(Vector3 vec)
    {

        return new Vector3(Mathf.FloorToInt(vec.x % VoxelData.ChunkWidth), Mathf.FloorToInt(vec.y), Mathf.FloorToInt(vec.z % VoxelData.ChunkWidth));

    }


    //返回方块类型
    public byte GetBlockType(Vector3 pos)
    {

        Allchunks.TryGetValue(GetChunkLocation(pos), out DevelopModeChunk chunktemp);

        //如果玩家在刷新区外
        //if (chunktemp == null)
        //{
        //    return VoxelData.notChunk;
        //}

        //如果玩家在区内，但Y值太高

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

