using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class World : MonoBehaviour
{

    [Header("方块类型")]
    public Material material;
    public BlockType[] blocktypes;

    [Header("地形生成")]
    public float noise2d_scale_smooth = 0.05f;
    public float noise2d_scale_steep = 0.02f;
    public float noise3d_scale = 0.05f;


    //固定资产
    private int renderSize = 4; //渲染区块半径,即renderSize*16f
    [Header("玩家")]
    public Transform player;

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
        InitMap();
    }


    private void Update()
    {

        //如果大于16f
        if (GetVector3Length(player.transform.position - Center_Now) > VoxelData.ChunkWidth)
        {
            //更新Center
            Center_direction = VtoNormal(player.transform.position - Center_Now);
            Center_Now += Center_direction * VoxelData.ChunkWidth;
            //Debug.Log(Center_Old);

            //调试
            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere.transform.position = Center_Old;
            //sphere.transform.localScale = new Vector3(2f, 2f, 2f);


            AddtoCreateChunks(Center_direction);

            AddtoRemoveChunks(Center_direction);


        }



        //Debug.Log((player.transform.position - Center).magnitude);
        //Debug.DrawLine(Center_Now + v3, player.transform.position + v3, Color.red, Time.deltaTime);
        //Debug.DrawLine(Center_Now, player.transform.position, Color.red, Time.deltaTime);

    }



    //初始化
    void InitMap()
    {
        Center_Now = player.transform.position;

        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.transform.position = Center_Now;
        //sphere.transform.localScale = new Vector3(2f, 2f, 2f);


        for (int x = -renderSize + (int)(player.transform.position.x / VoxelData.ChunkWidth); x < renderSize + (int)(player.transform.position.x / VoxelData.ChunkWidth); x++)
        {
            for (int z = -renderSize + (int)(player.transform.position.z / VoxelData.ChunkWidth); z < renderSize + (int)(player.transform.position.x / VoxelData.ChunkWidth); z++)
            {
                CreateChunk(new Vector3(x, 0, z));
            }
        }

        

    }


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


        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //cube.transform.position = new Vector3(x + chunkwidth / 2, 0, z + chunkwidth / 2);
        //cube.transform.localScale = new Vector3(chunkwidth, 1, chunkwidth);

         Chunk chunk_temp = new Chunk(new Vector3(pos.x,0, pos.z), this, noise2d_scale_smooth, noise2d_scale_steep, noise3d_scale);

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