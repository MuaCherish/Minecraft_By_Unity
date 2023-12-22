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

    [Header("��������")]
    public Material material;
    public BlockType[] blocktypes;

    [Header("��Ⱦ")]
    public int renderSize = 4; //��Ⱦ����뾶,��renderSize*16f
    public float StartToRender = 16f;

    [Header("������Χ")]
    public float noise2d_scale_smooth = 0.1f;
    public float noise2d_scale_steep = 0.08f;
    //public float noise2d_scale_plain = 0.1f;
    public float noise3d_scale = 0.1f;


    [Header("�ֲ�ṹ")]
    [Range(0, 60)]
    public float soil_min = 20;
    [Range(0, 60)]
    public float soil_max = 50;
    [Range(0, 60)]
    public float sea_level = 36;
    

    //���
    [Header("�����ײ��")]
    [Tooltip("Forward Back \n Left Right \n Up Down")]
    public Transform[] transforms = new Transform[6];


    //isBlock
    Chunk chunktemp;
    [HideInInspector]
    public bool isBlock = false;
    [HideInInspector]
    public bool isnearblock = false;
    public bool[,] BlockDirection = new bool[1,6];

    //ȫ��Chunkλ��
    private Dictionary<Vector3, Chunk> Allchunks = new Dictionary<Vector3, Chunk>();

    //�ȴ���Ӷ���
    private bool CreateCoroutineState = false;
    //private bool hasExecuted1 = false;
    private List<Vector3> WatingToCreate_Chunks = new List<Vector3>();

    //�ȴ�ɾ������
    private bool RemoveCoroutineState = false;
    //private bool hasExecuted2 = false;
    private List<Vector3> WatingToRemove_Chunks = new List<Vector3>();
    private Chunk obj;

    //Э��
    [Header("Э���ӳ�ʱ��")]
    public float CreateCoroutineDelay = 0.2f;
    public float RemoveCoroutineDelay = 0.5f;

    //���ɷ���
    private Vector3 Center_Now;
    private Vector3 Center_direction; //��������˷���

    //private Vector3 v3 = new Vector3(0, 2, 0);


    private void Start()
    {
        Application.targetFrameRate = 120;
        InitMap();
    }


    private void Update()
    {
        
        //�������16f
        if (GetVector3Length(transforms[5].transform.position - Center_Now) > StartToRender)
        {
            //����Center
            Center_direction = VtoNormal(transforms[5].transform.position - Center_Now);
            Center_Now += Center_direction * VoxelData.ChunkWidth;
            //����
            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere.transform.position = Center_Old;
            //sphere.transform.localScale = new Vector3(2f, 2f, 2f);
            AddtoCreateChunks(Center_direction);
            AddtoRemoveChunks(Center_direction);
        }

        //��ײ�ж�
        //IsGrounded();
        isNearBlock();

        //Debug.DrawLine(Center_Now, player.transform.position, Color.red, Time.deltaTime);
    }

   






    //----------------------------------World Options---------------------------------------
    //��ʼ��
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





    //-----------------------------------Create Э��----------------------------------------
    //��ӵ��ȴ���Ӷ���
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

        //Debug.Log("�Ѿ��������");


        //�ж��Ƿ�����Э��
        //���������������Э�̣��������ݶ���һ������Э��
        if (WatingToCreate_Chunks.Count > 0 && CreateCoroutineState == false)
        {
            StartCoroutine(CreateChunksQueue());
            //Debug.Log("Create Э������");
            CreateCoroutineState = true;
            //hasExecuted1 = false;
        }


    }
    //Э�̣�����Chunk
    private IEnumerator CreateChunksQueue()
    {
        while (true)
        {
            yield return new WaitForSeconds(CreateCoroutineDelay);


            //��������������ݣ��Ͷ�ȡ
            //���������û�����ݣ��͹ر�Э��
            if (WatingToCreate_Chunks.Count > 0)
            {
                CreateChunk(WatingToCreate_Chunks[0]);
                WatingToCreate_Chunks.RemoveAt(0);
            }
            else
            {
                //if (!hasExecuted1)
                //{ // �ڴ˴�ִ��һ���Ե��߼� 
                //    Debug.Log("Create Э���ѹر�");
                //    hasExecuted1 = true;
                //}

                CreateCoroutineState = false;
                StopCoroutine(CreateChunksQueue());
            }



        }
    }
    //����Chunk
    void CreateChunk(Vector3 pos)
    {

        //���ж�һ����û��
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





    //-----------------------------------Remove Э��-----------------------------------------
    //��ӵ��ȴ�ɾ������
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

        //�ж��Ƿ�����Э��
        //���������������Э�̣��������ݶ���һ������Э��
        if (WatingToRemove_Chunks.Count > 0 && RemoveCoroutineState == false)
        {
            StartCoroutine(RemoveChunksQueue());
            Debug.Log("Remove Э������");
            RemoveCoroutineState = true;
            //hasExecuted2 = false;
        }

    }

    //Э�̣�ɾ��ChunK
    private IEnumerator RemoveChunksQueue()
    {
        while (true)
        {
            yield return new WaitForSeconds(RemoveCoroutineDelay);


            //��������������ݣ��Ͷ�ȡ
            //���������û�����ݣ��͹ر�Э��
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
                //{ // �ڴ˴�ִ��һ���Ե��߼� 
                //    Debug.Log("Remove Э���ѹر�");
                //    hasExecuted2 = true;
                //}

                RemoveCoroutineState = false;
                StopCoroutine(RemoveChunksQueue());
            }



        }

    }
    //---------------------------------------------------------------------------------------





    //----------------------------------Player Options---------------------------------------
    //��ȡ��������
    public Vector3 GetChunkLocation(Transform transform)
    {
  
        return new Vector3((transform.position.x - transform.position.x % VoxelData.ChunkWidth) / VoxelData.ChunkWidth, 0, (transform.position.z - transform.position.z % VoxelData.ChunkWidth) / VoxelData.ChunkWidth);
    
    }

    //��ȡ�����������
    public Vector3 GetRelalocation(Transform transform)
    {

        return new Vector3(Mathf.FloorToInt(transform.position.x % VoxelData.ChunkWidth), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z % VoxelData.ChunkWidth));

    }

    //�������꣬�ж��ǲ���Block
    public void isNearBlock()
    {
        isnearblock = false; // ����ʼֵ��Ϊfalse
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












    //------------------------------------����------------------------------------------------
    //������λ������
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

    //��Vector3��2d����
    float GetVector3Length(Vector3 vec)
    {
        Vector2 vector2 = new Vector2(vec.x, vec.z);
        return vector2.magnitude;
    }
    //----------------------------------------------------------------------------------------






}

//�ṹ��BlockType
//�洢��������+���Ӧ��UV
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

    //��ͼ�е��������
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