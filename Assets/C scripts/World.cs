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

    [Header("渲染")]
    public int renderSize = 4; //渲染区块半径,即renderSize*16f
    public float StartToRender = 16f;

    [Header("噪声范围")]
    public float noise2d_scale_smooth = 0.1f;
    public float noise2d_scale_steep = 0.08f;
    //public float noise2d_scale_plain = 0.1f;
    public float noise3d_scale = 0.1f;


    [Header("分层结构")]
    [Range(0, 60)]
    public float soil_min = 20;
    [Range(0, 60)]
    public float soil_max = 50;
    [Range(0, 60)]
    public float sea_level = 36;
    

    //玩家
    [Header("玩家碰撞盒")]
    [Tooltip("Forward Back \n Left Right \n Up Down")]
    public Transform[] transforms = new Transform[6];


    //isBlock
    Chunk chunktemp;
    [HideInInspector]
    public bool isBlock = false;
    [HideInInspector]
    public bool isnearblock = false;
    public bool[,] BlockDirection = new bool[1,6];

    //全部Chunk位置
    private Dictionary<Vector3, Chunk> Allchunks = new Dictionary<Vector3, Chunk>();

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
        InitMap();
    }


    private void Update()
    {
        
        //如果大于16f
        if (GetVector3Length(transforms[5].transform.position - Center_Now) > StartToRender)
        {
            //更新Center
            Center_direction = VtoNormal(transforms[5].transform.position - Center_Now);
            Center_Now += Center_direction * VoxelData.ChunkWidth;
            //调试
            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere.transform.position = Center_Old;
            //sphere.transform.localScale = new Vector3(2f, 2f, 2f);
            AddtoCreateChunks(Center_direction);
            AddtoRemoveChunks(Center_direction);
        }

        //碰撞判断
        //IsGrounded();
        isNearBlock();

        //Debug.DrawLine(Center_Now, player.transform.position, Color.red, Time.deltaTime);
    }

   






    //----------------------------------World Options---------------------------------------
    //初始化
    void InitMap()
    {
        Center_Now = new Vector3(transforms[5].transform.position.x,0, transforms[5].transform.position.z);

        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.transform.position = Center_Now;
        //sphere.transform.localScale = new Vector3(2f, 2f, 2f);


        for (int x = -renderSize + (int)(transforms[5].transform.position.x / VoxelData.ChunkWidth); x < renderSize + (int)(transforms[5].transform.position.x / VoxelData.ChunkWidth); x++)
        {
            for (int z = -renderSize + (int)(transforms[5].transform.position.z / VoxelData.ChunkWidth); z < renderSize + (int)(transforms[5].transform.position.x / VoxelData.ChunkWidth); z++)
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

        if (pos.x == 102)
        {

        }

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
            Debug.Log("Remove 协程启动");
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
    //获取所在区块
    public Vector3 GetChunkLocation(Transform transform)
    {
  
        return new Vector3((transform.position.x - transform.position.x % VoxelData.ChunkWidth) / VoxelData.ChunkWidth, 0, (transform.position.z - transform.position.z % VoxelData.ChunkWidth) / VoxelData.ChunkWidth);
    
    }

    //获取所在相对坐标
    public Vector3 GetRelalocation(Transform transform)
    {

        return new Vector3(Mathf.FloorToInt(transform.position.x % VoxelData.ChunkWidth), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z % VoxelData.ChunkWidth));

    }

    //给定坐标，判断是不是Block
    public void isNearBlock()
    {
        isnearblock = false; // 将初始值设为false
        isBlock = true;

        for (int i = 0; i <= 5; i++)
        {

            chunktemp = Allchunks[GetChunkLocation(transforms[i])];
            byte block_type = chunktemp.voxelMap[(int)GetRelalocation(transforms[i]).x, (int)GetRelalocation(transforms[i]).y, (int)GetRelalocation(transforms[i]).z];

            if (block_type != 4)
            {
                isnearblock = true;
                BlockDirection[0,i] = true;
            }else if (block_type == 4 && i == 5)
            {
                isBlock = false;
            }
            else
            {
                BlockDirection[0,i] = false;
            }
        }
        
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