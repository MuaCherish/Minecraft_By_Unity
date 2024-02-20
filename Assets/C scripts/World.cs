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
    [Header("��Ϸ״̬")]
    public Game_State game_state = Game_State.Start;

    [Header("Material-��������")]
    public Material material;
    public BlockType[] blocktypes;

    [Header("World-��Ⱦ����")]
    [Tooltip("4���Ǳ߳�Ϊ4*16��������")]
    public int renderSize = 5; //��Ⱦ����뾶,��renderSize*16f
    [Tooltip("2���ǽӽ�2*16��ʱ��ʼˢ������")]
    public float StartToRender = 2f;

    [Header("������������(ԽС����Խ��)")]
    public float noise2d_scale_smooth = 0.01f;
    public float noise2d_scale_steep = 0.04f;
    public float noise3d_scale = 0.085f;


    [Header("Chunk-�ֲ�ṹ")]
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


    //���
    [Header("Player-��ҽŵ�����")]
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


    //ȫ��Chunkλ��
    [HideInInspector]
    public Dictionary<Vector3, Chunk> Allchunks = new Dictionary<Vector3, Chunk>();

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
    [Header("Corountine-Э���ӳ�ʱ��")]
    public float InitCorountineDelay = 1f;
    public float CreateCoroutineDelay = 0.2f;
    public float RemoveCoroutineDelay = 0.5f;

    //���ɷ���
    private Vector3 Center_Now;
    private Vector3 Center_direction; //��������˷���

    //UI Manager
    [HideInInspector]
    public float initprogress = 0f;


    //Chunks����
    [HideInInspector]
    public GameObject Chunks;


    //��������
    bool hasExec = true;


    //----------------------------------���ں���---------------------------------------

    private void Start()
    { 
        //֡��
        Application.targetFrameRate = 120;

        //����chunks
        Chunks = new GameObject();
        Chunks.name = "Chunks";
        Chunks.transform.SetParent(GameObject.Find("Environment").transform);

        // �����������ֵ
        if(isRandomSeed)
        {
            //��������
            Seed = Random.Range(0, 100);

            //����ˮƽ��
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
        //��ʼ����ͼ
        if (game_state == Game_State.Loading)
        {
            StartCoroutine(Init_Map_Thread());
        }


        //��Ϸ��ʼ
        if (game_state == Game_State.Playing)
        {

            if (hasExec)
            {
                // �������������Ļ����
                Cursor.lockState = CursorLockMode.Locked;
                //��겻����
                Cursor.visible = false;

                hasExec = false;
            }



            //�������16f
            if (GetVector3Length(PlayerFoot.transform.position - Center_Now) > (StartToRender * 16f))
            {
                //����Center
                Center_direction = VtoNormal(PlayerFoot.transform.position - Center_Now);
                Center_Now += Center_direction * VoxelData.ChunkWidth;

                //����
                //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //sphere.transform.position = Center_Old;
                //sphere.transform.localScale = new Vector3(2f, 2f, 2f);

                //���Chunk
                AddtoCreateChunks(Center_direction);
                AddtoRemoveChunks(Center_direction);
            }

            //��ײ�ж�
            //isHitWall();

            //���½��·���
            //getFoodBlockType();

            //Debug.DrawLine(Center_Now, player.transform.position, Color.red, Time.deltaTime);
        }

    }


    //---------------------------------------------------------------------------------------










    //----------------------------------World Options---------------------------------------
    //��ʼ����ͼ
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

        //if (pos.x >= 98 && pos.z >= 102)
        //{
        //    print("");
        //}
        //Debug.Log($"{Allchunks.Count}");
        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //cube.transform.position = new Vector3(x + chunkwidth / 2, 0, z + chunkwidth / 2);
        //cube.transform.localScale = new Vector3(chunkwidth, 1, chunkwidth);

        Chunk chunk_temp = new Chunk(new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z)), this);


        //�ж�һ���Ƿ��ڿ��ӷ�Χ�ڣ��������Ŀɼ���
        //if ((GetChunkLocation(Center_Now) - pos).magnitude > LookToRender)
        //{
        //    chunk_temp.HideChunk();
        //}


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
            //Debug.Log("Remove Э������");
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

    //��ʼ������λ��
    void Init_Player_Location()
    {
        //��<1600,63,1600>���±�����ֱ�������������
        while (GetBlockType(Start_Position) == VoxelData.Air)
        {
            Start_Position.y -= 1f;
        }

        Start_Position.y += 2f;


    }

    //��ȡ���·���
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

    //Vector3 --> ����������
    public Vector3 GetChunkLocation(Vector3 vec)
    {
        return new Vector3((vec.x - vec.x % VoxelData.ChunkWidth) / VoxelData.ChunkWidth, 0, (vec.z - vec.z % VoxelData.ChunkWidth) / VoxelData.ChunkWidth);

    }

    //Vector3 --> ���������
    public Chunk GetChunkObject(Vector3 pos)
    {

        Allchunks.TryGetValue(GetChunkLocation(pos), out Chunk chunktemp);
        return chunktemp;
    }


    //Vector3 --> ��������������
    public Vector3 GetRelalocation(Vector3 vec)
    {
        return new Vector3(Mathf.FloorToInt(vec.x % VoxelData.ChunkWidth), Mathf.FloorToInt(vec.y), Mathf.FloorToInt(vec.z % VoxelData.ChunkWidth));

    }

    //�ж��Ƿ�ײǽ
    //public void isHitWall()
    //{
    //    isnearblock = false; // ����ʼֵ��Ϊfalse
    //    isBlock = true;

    //    //������ײ��
    //    for (int i = 0; i <= 9; i++)
    //    {

    //        //if (GetBlockType(Block_transforms[i].position) == ERROR_CODE)
    //        //{
    //        //    // ������Ծ����
    //        //    playercontroller.velocity.y -= playercontroller.gravity * Time.deltaTime;  // �ڿ���ʱӦ������
    //        //}


    //        if (GetBlockType(PlayerFoot.position) != 4 && GetBlockType(PlayerFoot.position) != ERROR_CODE_OUTOFVOXELMAP)
    //        {
    //            isnearblock = true;
    //            BlockDirection[0, i] = true;
    //        }
    //        //else if(GetBlockType(Block_transforms[i].position) == 8 && i == 5��
    //        //{

    //        //    isSwiming = true;



    //        //}
    //        //���5�ǿ��������ж�Ϊ���
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

    //���ط�������
    public byte GetBlockType(Vector3 pos)
    {




        Allchunks.TryGetValue(GetChunkLocation(pos), out Chunk chunktemp);

        //��������ˢ������
        //if (chunktemp == null)
        //{
        //    return ERROR_CODE;
        //}

        //�����������ڣ���Yֵ̫��

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

    //�������ײ�еķ����ж�
    public bool CheckForVoxel(Vector3 pos)
    {
        //�����������
        Vector3 vec = GetRelalocation(new Vector3(pos.x, pos.y, pos.z));

        //�ж�XOZ����û�г���
        if (!Allchunks.ContainsKey(GetChunkLocation(pos))) { return true; }

        //�ж�Y����û�г���
        if (vec.y >= VoxelData.ChunkHeight) { return false; }

        //���ع��廹�ǿ���
        return blocktypes[Allchunks[GetChunkLocation(new Vector3(pos.x, pos.y, pos.z))].voxelMap[(int)vec.x, (int)vec.y, (int)vec.z]].isSolid;

    }




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