using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;

public class World : MonoBehaviour
{

    [Header("方块类型")]
    public Material material;
    public BlockType[] blocktypes;

    [Header("渲染设置")]
    [Tooltip("4就是边长为4*16的正方形")]
    public int renderSize = 4; //渲染区块半径,即renderSize*16f
    [Tooltip("2就是接近2*16的时候开始刷新区块")]
    public float StartToRender = 2f;

    [Header("噪声采样比例(越小拉的越长)")]
    public float noise2d_scale_smooth = 0.01f;
    public float noise2d_scale_steep = 0.04f;
    public float noise3d_scale = 0.085f;


    [Header("分层结构")]
    [Range(0, 60)]
    public float soil_min = 15;
    [Range(0, 60)]
    public float soil_max = 55;
    [Range(0, 60)]
    public float sea_level = 30;
    

    //玩家
    [Header("玩家碰撞盒")]
    [Tooltip("Forward Back \n Left Right \n Up Down")]
    public Transform[] Block_transforms = new Transform[10];
    public byte ERROR_CODE_OUTOFVOXELMAP = 255;
    public Vector3 Start_Position = new Vector3(1600f, 63f, 1600f);
    //public GameObject FirstCamera;
    //private PlayerController playercontroller;

    //isBlock
    Chunk chunktemp;
    [HideInInspector]
    public bool isBlock = false;
    [HideInInspector]
    public bool isnearblock = false;
    public bool[,] BlockDirection = new bool[1,10];

    //全部Chunk位置
    [HideInInspector]
    public Dictionary<Vector3, Chunk> Allchunks = new Dictionary<Vector3, Chunk>();

    //等待添加队列
    private bool CreateCoroutineState = false;
    //private bool hasExecuted1 = false;
    private List<Vector3> WatingToCreate_Chunks = new List<Vector3>();

    //等待删除队列
    private bool RemoveCoroutineState = false;
    //private bool hasExecuted2 = false;
    private List<Vector3> WatingToRemove_Chunks = new List<Vector3>();
    private Chunk obj;

    //协程
    [Header("协程延迟时间")]
    public float CreateCoroutineDelay = 0.2f;
    public float RemoveCoroutineDelay = 0.5f;

    //生成方向
    private Vector3 Center_Now;
    private Vector3 Center_direction; //这个代表了方向

    //private Vector3 v3 = new Vector3(0, 2, 0);


    private void Start()
    {
        Application.targetFrameRate = 120;
        //playercontroller = FirstCamera.GetComponent<PlayerController>();
        InitMap();
        Init_Player_Location();
    }


    private void Update()
    {
        
        //如果大于16f
        if (GetVector3Length(Block_transforms[5].transform.position - Center_Now) > (StartToRender * 16f))
        {
            //更新Center
            Center_direction = VtoNormal(Block_transforms[5].transform.position - Center_Now);
            Center_Now += Center_direction * VoxelData.ChunkWidth;
            //调试
            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere.transform.position = Center_Old;
            //sphere.transform.localScale = new Vector3(2f, 2f, 2f);
            AddtoCreateChunks(Center_direction);
            AddtoRemoveChunks(Center_direction);
        }

        //碰撞判断
        isHitWall();

        //Debug.DrawLine(Center_Now, player.transform.position, Color.red, Time.deltaTime);
    }

   






    //----------------------------------World Options---------------------------------------
    //初始化
    void InitMap()
    {
        Center_Now = new Vector3(Block_transforms[5].transform.position.x,0, Block_transforms[5].transform.position.z);

        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.transform.position = Center_Now;
        //sphere.transform.localScale = new Vector3(2f, 2f, 2f);


        for (int x = -renderSize + (int)(Block_transforms[5].transform.position.x / VoxelData.ChunkWidth); x < renderSize + (int)(Block_transforms[5].transform.position.x / VoxelData.ChunkWidth); x++)
        {
            for (int z = -renderSize + (int)(Block_transforms[5].transform.position.z / VoxelData.ChunkWidth); z < renderSize + (int)(Block_transforms[5].transform.position.x / VoxelData.ChunkWidth); z++)
            {
                CreateChunk(new Vector3(x, 0, z));
            }
        }

        

    }
    //--------------------------------------------------------------------------------------





    //-----------------------------------Create 协程----------------------------------------
    //添加到等待添加队列
    void AddtoCreateChunks(Vector3 add_vec)
    {
        //ForWard
        if (add_vec == new Vector3(0, 0, 1))
        {
            add_vec = (Center_Now / VoxelData.ChunkWidth) + Center_direction * (renderSize - 1);

            for (int i = -renderSize; i < renderSize; i++)
            {
                //CreateChunk(add_vec + new Vector3((float)i, 0, 0));
                WatingToCreate_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));

            }
        }

        //Back
        if (add_vec == new Vector3(0, 0, -1))
        {
            add_vec = (Center_Now / VoxelData.ChunkWidth) + Center_direction * (renderSize);

            for (int i = -renderSize; i < renderSize; i++)
            {
                //CreateChunk(add_vec + new Vector3((float)i, 0, 0));
                WatingToCreate_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));
            }
        }

        //Left
        if (add_vec == new Vector3(-1, 0, 0))
        {
            add_vec = (Center_Now / VoxelData.ChunkWidth) + Center_direction * (renderSize);

            for (int i = -renderSize; i < renderSize; i++)
            {
                //CreateChunk(add_vec + new Vector3(0, 0, (float)i));
                WatingToCreate_Chunks.Add(add_vec + new Vector3(0, 0, (float)i));
            }
        }

        //Right
        if (add_vec == new Vector3(1, 0, 0))
        {
            add_vec = (Center_Now / VoxelData.ChunkWidth) + Center_direction * (renderSize - 1);

            for (int i = -renderSize; i < renderSize; i++)
            {
                //CreateChunk(add_vec + new Vector3(0, 0, (float)i));
                WatingToCreate_Chunks.Add(add_vec + new Vector3(0, 0, (float)i));
            }
        }

        //Debug.Log("已经添加坐标");


        //判断是否启动协程
        //先两次添加再启动协程，后面数据多了一次启动协程
        if (WatingToCreate_Chunks.Count > 0 && CreateCoroutineState == false)
        {
            StartCoroutine(CreateChunksQueue());
            //Debug.Log("Create 协程启动");
            CreateCoroutineState = true;
            //hasExecuted1 = false;
        }


    }
    //协程：创造Chunk
    private IEnumerator CreateChunksQueue()
    {
        while (true)
        {
            yield return new WaitForSeconds(CreateCoroutineDelay);


            //如果队列中有数据，就读取
            //如果队列中没有数据，就关闭协程
            if (WatingToCreate_Chunks.Count > 0)
            {
                CreateChunk(WatingToCreate_Chunks[0]);
                WatingToCreate_Chunks.RemoveAt(0);
            }
            else
            {
                //if (!hasExecuted1)
                //{ // 在此处执行一次性的逻辑 
                //    Debug.Log("Create 协程已关闭");
                //    hasExecuted1 = true;
                //}

                CreateCoroutineState = false;
                StopCoroutine(CreateChunksQueue());
            }



        }
    }
    //生成Chunk
    void CreateChunk(Vector3 pos)
    {

        //先判断一下有没有
        if (Allchunks.ContainsKey(pos))
        {
            return;
        }

        //if (pos.x == 99 && pos.z == 100)
        //{
        //    print("");
        //}

        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //cube.transform.position = new Vector3(x + chunkwidth / 2, 0, z + chunkwidth / 2);
        //cube.transform.localScale = new Vector3(chunkwidth, 1, chunkwidth);

        Chunk chunk_temp = new Chunk(new Vector3(pos.x,0, pos.z), this);

         Allchunks.Add(pos, chunk_temp);
    }
    //--------------------------------------------------------------------------------------





    //-----------------------------------Remove 协程-----------------------------------------
    //添加到等待删除队列
    void AddtoRemoveChunks(Vector3 add_vec)
    {

        //ForWard
        if (add_vec == new Vector3(0, 0, 1))
        {
            add_vec = (Center_Now / VoxelData.ChunkWidth) - Center_direction * (renderSize + 1);

            for (int i = -renderSize; i < renderSize; i++)
            {

                WatingToRemove_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));

            }
        }



        //Back
        if (add_vec == new Vector3(0, 0, -1))
        {
            add_vec = (Center_Now / VoxelData.ChunkWidth) - Center_direction * (renderSize);

            for (int i = -renderSize; i < renderSize; i++)
            {

                WatingToRemove_Chunks.Add(add_vec + new Vector3((float)i, 0, 0));

            }
        }



        //Left
        if (add_vec == new Vector3(-1, 0, 0))
        {
            add_vec = (Center_Now / VoxelData.ChunkWidth) - Center_direction * (renderSize);

            for (int i = -renderSize; i < renderSize; i++)
            {

                WatingToRemove_Chunks.Add(add_vec + new Vector3(0, 0, (float)i));
            }
        }

        //Right
        if (add_vec == new Vector3(1, 0, 0))
        {
            add_vec = (Center_Now / VoxelData.ChunkWidth) - Center_direction * (renderSize + 1);

            for (int i = -renderSize; i < renderSize; i++)
            {

                WatingToRemove_Chunks.Add(add_vec + new Vector3(0, 0, (float)i));

            }
        }

        //判断是否启动协程
        //先两次添加再启动协程，后面数据多了一次启动协程
        if (WatingToRemove_Chunks.Count > 0 && RemoveCoroutineState == false)
        {
            StartCoroutine(RemoveChunksQueue());
            //Debug.Log("Remove 协程启动");
            RemoveCoroutineState = true;
            //hasExecuted2 = false;
        }

    }

    //协程：删除ChunK
    private IEnumerator RemoveChunksQueue()
    {
        while (true)
        {
            yield return new WaitForSeconds(RemoveCoroutineDelay);


            //如果队列中有数据，就读取
            //如果队列中没有数据，就关闭协程
            if (WatingToRemove_Chunks.Count > 0)
            {
                if (Allchunks.TryGetValue(WatingToRemove_Chunks[0], out obj))
                {
                    obj.DestroyChunk();
                    Allchunks.Remove(WatingToRemove_Chunks[0]);
                    WatingToRemove_Chunks.RemoveAt(0);
                }
                else
                {
                    WatingToRemove_Chunks.RemoveAt(0);
                    WatingToRemove_Chunks.Add(WatingToRemove_Chunks[0]);
                }


            }
            else
            {
                //if (!hasExecuted2)
                //{ // 在此处执行一次性的逻辑 
                //    Debug.Log("Remove 协程已关闭");
                //    hasExecuted2 = true;
                //}

                RemoveCoroutineState = false;
                StopCoroutine(RemoveChunksQueue());
            }



        }

    }
    //---------------------------------------------------------------------------------------





    //----------------------------------Player Options---------------------------------------

    //初始化人物位置
    void Init_Player_Location()
    {
        //从<1600,63,1600>向下遍历，直到坐标符合条件
        while (GetBlockType(Start_Position) == 4)
        {
            Start_Position.y -= 1f;
        }

        Start_Position.y += 2f;


    }



    //Vector3 --> 大区块坐标
    public Vector3 GetChunkLocation(Vector3 vec)
    {
        return new Vector3((vec.x - vec.x % VoxelData.ChunkWidth) / VoxelData.ChunkWidth, 0, (vec.z - vec.z % VoxelData.ChunkWidth) / VoxelData.ChunkWidth);
    
    }

    //Vector3 --> 大区块对象
    public Chunk GetChunkObject(Vector3 pos)
    {

        Allchunks.TryGetValue(GetChunkLocation(pos), out Chunk chunktemp);
        return chunktemp;
    }


    //Vector3 --> 区块里的相对坐标
    public Vector3 GetRelalocation(Vector3 vec)
    {
        return new Vector3(Mathf.FloorToInt(vec.x % VoxelData.ChunkWidth), Mathf.FloorToInt(vec.y), Mathf.FloorToInt(vec.z % VoxelData.ChunkWidth));

    }

    //判断是否撞墙
    public void isHitWall()
    {
        isnearblock = false; // 将初始值设为false
        isBlock = true;

        //遍历碰撞盒
        for (int i = 0; i <= 9; i++)
        {

            //if (GetBlockType(Block_transforms[i].position) == ERROR_CODE)
            //{
            //    // 处理跳跃数据
            //    playercontroller.velocity.y -= playercontroller.gravity * Time.deltaTime;  // 在空中时应用重力
            //}


            if (GetBlockType(Block_transforms[i].position) != 4 && GetBlockType(Block_transforms[i].position) != ERROR_CODE_OUTOFVOXELMAP)
            {
                isnearblock = true;
                BlockDirection[0,i] = true;

            //如果5不是空气，则判定为离地
            }else if (GetBlockType(Block_transforms[i].position) == 4 && i == 5)
            {
                isBlock = false;
            }
            else
            {
                BlockDirection[0,i] = false;
            }
        }
        
    }

    //返回方块类型
    public byte GetBlockType(Vector3 pos)
    {
        Allchunks.TryGetValue(GetChunkLocation(pos), out Chunk chunktemp);

        //如果玩家在刷新区外
        //if (chunktemp == null)
        //{
        //    return ERROR_CODE;
        //}

        //如果玩家在区内，但Y值太高

        if ((int)GetRelalocation(pos).y >= VoxelData.ChunkHeight)
        {
            //isBlock = false;
            //isnearblock = false;
            return ERROR_CODE_OUTOFVOXELMAP;
        }

        byte block_type = chunktemp.voxelMap[(int)GetRelalocation(pos).x, (int)GetRelalocation(pos).y, (int)GetRelalocation(pos).z];


        return block_type;
    }



    //---------------------------------------------------------------------------------------












    //------------------------------------工具------------------------------------------------
    //向量单位正交化
    Vector3 VtoNormal(Vector3 v)
    {
        if (v.x >= 0)
        {
            if (v.z >= 0)
            {
                if (Mathf.Abs(v.x) >= Mathf.Abs(v.z))
                {
                    return new Vector3(1, 0, 0);
                }
                else
                {
                    return new Vector3(0, 0, 1);
                }
            }
            else
            {
                if (Mathf.Abs(v.x) >= Mathf.Abs(v.z))
                {
                    return new Vector3(1, 0, 0);
                }
                else
                {
                    return new Vector3(0, 0, -1);
                }
            }

        }
        else
        {
            if (v.z >= 0)
            {
                if (Mathf.Abs(v.x) >= Mathf.Abs(v.z))
                {
                    return new Vector3(-1, 0, 0);
                }
                else
                {
                    return new Vector3(0, 0, 1);
                }
            }
            else
            {
                if (Mathf.Abs(v.x) >= Mathf.Abs(v.z))
                {
                    return new Vector3(-1, 0, 0);
                }
                else
                {
                    return new Vector3(-1, 0, 0);
                }
            }
        }
    }

    //求Vector3的2d长度
    float GetVector3Length(Vector3 vec)
    {
        Vector2 vector2 = new Vector2(vec.x, vec.z);
        return vector2.magnitude;
    }
    //----------------------------------------------------------------------------------------






}

//结构体BlockType
//存储方块种类+面对应的UV
[System.Serializable]
public class BlockType
{

    public string blockName;
    public bool isSolid;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    //贴图中的面的坐标
    public int GetTextureID(int faceIndex)
    {

        switch (faceIndex)
        {

            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;


        }

    }

}