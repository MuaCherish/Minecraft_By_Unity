using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Unity.VisualScripting;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
//using static UnityEditor.PlayerSettings;
//using static UnityEditor.Progress;

public enum Game_State
{
    Start, Loading, Playing,
}



public class World : MonoBehaviour
{
    [Header("游戏状态")]
    public Game_State game_state = Game_State.Start;

    [Header("Material-方块类型")]
    public Material material;
    public BlockType[] blocktypes;

    [Header("World-渲染设置")]
    [Tooltip("4就是边长为4*16的正方形")]
    public int renderSize = 5; //渲染区块半径,即renderSize*16f
    [Tooltip("2就是接近2*16的时候开始刷新区块")]
    public float StartToRender = 2f;

    [Header("噪声采样比例(越小拉的越长)")]
    public float noise2d_scale_smooth = 0.01f;
    public float noise2d_scale_steep = 0.04f;
    public float noise3d_scale = 0.085f;


    [Header("Chunk-分层结构")]
    public bool isRandomSeed = true;
    public int Seed = 0;
    [Range(0, 60)]
    public float soil_min = 15;
    [Range(0, 60)]
    public float soil_max = 55;
    [Range(0, 60)]
    public float sea_level = 30;
    public int TreeCount = 5;
    public int TreeHigh_min = 5;
    public int TreeHigh_max = 7;


    //玩家
    [Header("Player-玩家脚底坐标")]
    public Transform PlayerFoot;
    [HideInInspector]
    public byte ERROR_CODE_OUTOFVOXELMAP = 255;
    [HideInInspector]
    public Vector3 Start_Position = new Vector3(1600f, 63f, 1600f);
    //[HideInInspector]
    //public string foot_BlockType = "None";



    //isBlock
    //Chunk chunktemp;
    //[HideInInspector]
    //public bool isBlock = false;
    //[HideInInspector]
    //public bool isSwiming = false;
    //[HideInInspector]
    //public bool isnearblock = false;
    //public bool[,] BlockDirection = new bool[1, 10];


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
    [Header("Corountine-协程延迟时间")]
    public float InitCorountineDelay = 1f;
    public float CreateCoroutineDelay = 0.2f;
    public float RemoveCoroutineDelay = 0.5f;

    //生成方向
    private Vector3 Center_Now;
    private Vector3 Center_direction; //这个代表了方向

    //UI Manager
    [HideInInspector]
    public float initprogress = 0f;


    //Chunks父级
    [HideInInspector]
    public GameObject Chunks;


    //其他变量
    bool hasExec = true;


    //----------------------------------周期函数---------------------------------------

    private void Start()
    { 
        //帧数
        Application.targetFrameRate = 120;

        //设置chunks
        Chunks = new GameObject();
        Chunks.name = "Chunks";
        Chunks.transform.SetParent(GameObject.Find("Environment").transform);

        // 允许设置随机值
        if(isRandomSeed)
        {
            //设置种子
            Seed = Random.Range(0, 100);

            //设置水平面
            sea_level = Random.Range(20, 38);
        }
           

        
    }

    //private void FixedUpdate()
    //{
    //    if (game_state == Game_State.Playing)
    //        getFoodBlockType();
    //}

    private void Update()
    {
        //初始化地图
        if (game_state == Game_State.Loading)
        {
            StartCoroutine(Init_Map_Thread());
        }


        //游戏开始
        if (game_state == Game_State.Playing)
        {

            if (hasExec)
            {
                // 将鼠标锁定在屏幕中心
                Cursor.lockState = CursorLockMode.Locked;
                //鼠标不可视
                Cursor.visible = false;

                hasExec = false;
            }



            //如果大于16f
            if (GetVector3Length(PlayerFoot.transform.position - Center_Now) > (StartToRender * 16f))
            {
                //更新Center
                Center_direction = VtoNormal(PlayerFoot.transform.position - Center_Now);
                Center_Now += Center_direction * VoxelData.ChunkWidth;

                //调试
                //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //sphere.transform.position = Center_Old;
                //sphere.transform.localScale = new Vector3(2f, 2f, 2f);

                //添加Chunk
                AddtoCreateChunks(Center_direction);
                AddtoRemoveChunks(Center_direction);
            }

            //碰撞判断
            //isHitWall();

            //更新脚下方块
            //getFoodBlockType();

            //Debug.DrawLine(Center_Now, player.transform.position, Color.red, Time.deltaTime);
        }

    }


    //---------------------------------------------------------------------------------------










    //----------------------------------World Options---------------------------------------
    //初始化地图
    IEnumerator Init_Map_Thread()
    {
        Center_Now = new Vector3(PlayerFoot.transform.position.x, 0, PlayerFoot.transform.position.z);

        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.transform.position = Center_Now;
        //sphere.transform.localScale = new Vector3(2f, 2f, 2f);
        float temp = 0f;

        for (int x = -renderSize + (int)(PlayerFoot.transform.position.x / VoxelData.ChunkWidth); x < renderSize + (int)(PlayerFoot.transform.position.x / VoxelData.ChunkWidth); x++)
        {
            for (int z = -renderSize + (int)(PlayerFoot.transform.position.z / VoxelData.ChunkWidth); z < renderSize + (int)(PlayerFoot.transform.position.x / VoxelData.ChunkWidth); z++)
            {


                //Create
                CreateChunk(new Vector3(x, 0, z));


                float max = renderSize * renderSize * 4;
                temp++;
                initprogress = Mathf.Lerp(0f, 0.9f, temp / max);

                yield return new WaitForSeconds(InitCorountineDelay);
            }
        }

        Init_Player_Location();
        yield return new WaitForSeconds(1f);
        initprogress = 1f;


        yield return null;
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

        //if (pos.x >= 98 && pos.z >= 102)
        //{
        //    print("");
        //}
        //Debug.Log($"{Allchunks.Count}");
        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //cube.transform.position = new Vector3(x + chunkwidth / 2, 0, z + chunkwidth / 2);
        //cube.transform.localScale = new Vector3(chunkwidth, 1, chunkwidth);

        Chunk chunk_temp = new Chunk(new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z)), this);


        //判断一下是否在可视范围内，设置它的可见性
        //if ((GetChunkLocation(Center_Now) - pos).magnitude > LookToRender)
        //{
        //    chunk_temp.HideChunk();
        //}


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
        while (GetBlockType(Start_Position) == VoxelData.Air)
        {
            Start_Position.y -= 1f;
        }

        Start_Position.y += 2f;


    }

    //获取脚下方块
    //void getFoodBlockType()
    //{
    //    switch (GetBlockType(PlayerFoot.transform.position))
    //    {
    //        case 0: foot_BlockType = "BedRock"; break;
    //        case 1: foot_BlockType = "Stone"; break;
    //        case 2: foot_BlockType = "Grass"; break;
    //        case 3: foot_BlockType = "Soil"; break;
    //        case 4: foot_BlockType = "Air"; break;
    //        case 5: foot_BlockType = "Sand"; break;
    //        case 6: foot_BlockType = "Wood"; break;
    //        case 7: foot_BlockType = "Leaves"; break;
    //        case 8: foot_BlockType = "Water"; break;
    //        case 9: foot_BlockType = "Coal"; break;
    //        default: foot_BlockType = "None"; break;
    //    }
    //}

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
    //public void isHitWall()
    //{
    //    isnearblock = false; // 将初始值设为false
    //    isBlock = true;

    //    //遍历碰撞盒
    //    for (int i = 0; i <= 9; i++)
    //    {

    //        //if (GetBlockType(Block_transforms[i].position) == ERROR_CODE)
    //        //{
    //        //    // 处理跳跃数据
    //        //    playercontroller.velocity.y -= playercontroller.gravity * Time.deltaTime;  // 在空中时应用重力
    //        //}


    //        if (GetBlockType(PlayerFoot.position) != 4 && GetBlockType(PlayerFoot.position) != ERROR_CODE_OUTOFVOXELMAP)
    //        {
    //            isnearblock = true;
    //            BlockDirection[0, i] = true;
    //        }
    //        //else if(GetBlockType(Block_transforms[i].position) == 8 && i == 5）
    //        //{

    //        //    isSwiming = true;



    //        //}
    //        //如果5是空气，则判定为离地
    //        else if (GetBlockType(Block_transforms[i].position) == 4 && i == 5)
    //        {
    //            isBlock = false;
    //            isSwiming = false;


    //        }
    //        else
    //        {
    //            BlockDirection[0, i] = false;

    //        }

    //        //swiming
    //        if (GetBlockType(Block_transforms[i].position) == 8 && i == 5)
    //        {
    //            isSwiming = true;
    //        }




    //    }

    //}

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

    //对玩家碰撞盒的方块判断
    public bool CheckForVoxel(Vector3 pos)
    {
        //计算相对坐标
        Vector3 vec = GetRelalocation(new Vector3(pos.x, pos.y, pos.z));

        //判断XOZ上有没有出界
        if (!Allchunks.ContainsKey(GetChunkLocation(pos))) { return true; }

        //判断Y上有没有出界
        if (vec.y >= VoxelData.ChunkHeight) { return false; }

        //返回固体还是空气
        return blocktypes[Allchunks[GetChunkLocation(new Vector3(pos.x, pos.y, pos.z))].voxelMap[(int)vec.x, (int)vec.y, (int)vec.z]].isSolid;

    }




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