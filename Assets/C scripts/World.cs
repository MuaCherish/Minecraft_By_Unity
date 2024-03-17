using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//ȫ����Ϸ״̬
public enum Game_State
{
    Start, Loading, Playing, Pause,
}


public class World : MonoBehaviour
{
    [Header("Transforms")]
    public TMP_InputField input_Seed;
    public TMP_InputField input_RenderSize;

    [Header("��Ϸ״̬")]
    public Game_State game_state = Game_State.Start;

    [Header("Material-��������")]
    public Material material;
    public BlockType[] blocktypes;

    [Header("World-��Ⱦ����")]
    [Tooltip("4���Ǳ߳�Ϊ4*16��������")]
    public int renderSize = 5; //��Ⱦ����뾶,��renderSize*16f
    [Tooltip("2���ǽӽ�2*16��ʱ��ʼˢ������")]
    public float StartToRender = 1f;
    public float DestroySize = 7f;

    [Header("Biome-ƽԭ����")]
    //noise2dԽС����������Խ�������������֮����ȸ���Ȼ
    //ֵԽС��������С������Խ����
    //����Խ��Խ��
    public float noise2d_scale_smooth = 0.01f;
    public float noise2d_scale_steep = 0.04f;

    [Header("Cave-��Ѩϵͳ")]
    //noise3d_scaleԽ��Ѩ����Խ���أ�Խ��Խ��
    //cave_widthԽС��Ѩƽ����ȱ�С������̫�󣬲�Ȼȫ�Ƕ�
    public float noise3d_scale = 0.085f;
    public float cave_width = 0.45f;


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



    [Header("���ɸ���(n��֮һ)")]
    public int Random_Bamboo;
    public int Random_Coal;
    public int Random_Iron;
    public int Random_Gold;
    public int Random_Blue_Crystal;
    public int Random_Diamond;



    //���
    [Header("Player-��ҽŵ�����")]
    public Transform PlayerFoot;
    [HideInInspector]
    public byte ERROR_CODE_OUTOFVOXELMAP = 255;
    [HideInInspector]
    public Vector3 Start_Position = new Vector3(1600f, 63f, 1600f);


    //ȫ��Chunkλ��
    [HideInInspector]
    public Dictionary<Vector3, Chunk> Allchunks = new Dictionary<Vector3, Chunk>();

    //�ȴ���Ӷ���
    //private List<chunkWithsequence> WatingToCreateChunks = new List<chunkWithsequence>();
    private List<Vector3> WatingToCreate_Chunks = new List<Vector3>();

    //�ȴ�ɾ������
    private List<Vector3> WatingToRemove_Chunks = new List<Vector3>();
    private Chunk obj;

    //Э��
    [Header("Corountine-Э���ӳ�ʱ��")]
    public float InitCorountineDelay = 1f;
    public float CreateCoroutineDelay = 0.5f;
    public float RemoveCoroutineDelay = 0.5f;
    public float RenderDelay = 0.1f;

    //���ɷ���
    private Vector3 Center_Now;
    private Vector3 Center_direction; //��������˷���

    //UI Manager
    [HideInInspector]
    public float initprogress = 0f;


    //Chunks����
    [HideInInspector]
    public GameObject Chunks;


    //һ���Դ���
    bool hasExec = true;
    bool hasExec_SetSeed = true;

    //Create && Remove Э��
    Coroutine CreateCoroutine;
    Coroutine RemoveCoroutine;

    //Render_0 && Render_1 Э��
    public Queue<Chunk> WaitToRender = new Queue<Chunk>();
    Coroutine Render_Coroutine;

    //----------------------------------���ں���---------------------------------------
    private void Start()
    {
        //֡��
        Application.targetFrameRate = 90;

        //Self
        Chunks = new GameObject();
        Chunks.name = "Chunks";
        Chunks.transform.SetParent(GameObject.Find("Environment").transform);

        //��������
        Seed = Random.Range(0, 100);
        //sea_level = Random.Range(20, 39);

        //��ʼ��һ��С��
        Start_Screen_Init();
    }

    private void FixedUpdate()
    {

        //��Ⱦ�̳߳�פ
        RenderCoroutineManager();

        //��ʼ����ͼ
        if (game_state == Game_State.Loading)
        {
            if (hasExec_SetSeed)
            {
                //����Ƿ���������
                CheckSeed();

                //����Ƿ�����RenderSize
                CheckRenderSize();

                //��ʼ��ʼ��
                StartCoroutine(Init_Map_Thread());

                hasExec_SetSeed = false;
            }


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
            }


            //����ƶ�ˢ��
            //�������16f
            if (GetVector3Length(PlayerFoot.transform.position - Center_Now) > (StartToRender * 16f))
            {
                //����Center
                Center_direction = VtoNormal(PlayerFoot.transform.position - Center_Now);
                Center_Now += Center_direction * VoxelData.ChunkWidth;

                //���Chunk
                AddtoCreateChunks(Center_direction);
                AddtoRemoveChunks(Center_direction);
            }


        }

    }

    void OnApplicationQuit()
    {
        //print("Quit");
        RenderSettings.skybox.SetFloat("_Exposure", 0.69f);
    }

    //---------------------------------------------------------------------------------------










    //----------------------------------World Options---------------------------------------
    //���˵���ͼ
    public void Start_Screen_Init()
    {
        Chunk chunk_temp = new Chunk(new Vector3(0, 0, 0), this);
    }

    //�������
    void CheckSeed()
    {
        if (input_Seed != null && string.IsNullOrEmpty(input_Seed.text))
        {
            //Debug.Log("����Ϊ�գ�");
        }
        else
        {
            //Debug.Log("���Ӳ�Ϊ�գ�");

            //��������
            int number;
            if (int.TryParse(input_Seed.text, out number))
            {
                // ת���ɹ���number �д洢�������ֶ��е�����
                //Debug.Log("����Ϊ: " + number);
                Seed = number;

                //����ˮƽ��
                //sea_level = Random.Range(20, 42); 
            }
            else
            {
                // ת��ʧ�ܣ������ֶ��е��ַ���������Ч������
                Debug.Log("����ת��ʧ�ܣ�");
            }


        }
    }

    //�����Ⱦ��Χ
    void CheckRenderSize()
    {
        //size�����6��������������ֵ

        int number;

        if (int.TryParse(input_RenderSize.text, out number))
        {
            // ת���ɹ���number �д洢�������ֶ��е�����
            //Debug.Log("����Ϊ: " + number);
            if (number == 6)
            {
                return;
            }


            renderSize = number;

        }
        else
        {
            // ת��ʧ�ܣ������ֶ��е��ַ���������Ч������
            Debug.Log("RenderSIzeת��ʧ�ܣ�");
        }
    }

    //��ʼ����ͼ
    IEnumerator Init_Map_Thread()
    {
        Center_Now = new Vector3(PlayerFoot.transform.position.x, 0, PlayerFoot.transform.position.z);

       //дһ��Э�̣�����������ع�Զ������


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


    //������������
    public void Update_CenterChunks()
    {
        //update������������
        StartCoroutine(Init_Map_Thread());
    }

    //�����Զ����
    IEnumerator Update_FarChunks()
    {
        //�����Զ������

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
        if (WatingToCreate_Chunks.Count > 0 && CreateCoroutine == null)
        {
            CreateCoroutine = StartCoroutine(CreateChunksQueue());
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
                //����鵽��chunk�Ѿ����ڣ�����
                //������������
                if (Allchunks.TryGetValue(WatingToCreate_Chunks[0], out obj)) 
                {
                    if (obj != null)
                    {
                        obj.ShowChunk();
                    }
                    
                }
                else
                {
                    CreateChunk(WatingToCreate_Chunks[0]);
                }

                WatingToCreate_Chunks.RemoveAt(0);

            }
            else
            {

                CreateCoroutine = null;
                break;
            }



        }
    }
    //����Chunk
    void CreateChunk(Vector3 pos)
    {

        //���ж�һ����û��
        if (Allchunks.ContainsKey(pos))
        {
            Allchunks[pos].ShowChunk();
            return;
        }

        //����Chunk
        Chunk chunk_temp = new Chunk(new Vector3(Mathf.FloorToInt(pos.x), 0, Mathf.FloorToInt(pos.z)), this);

        //��ӵ��ֵ�
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
        if (WatingToRemove_Chunks.Count > 0 && RemoveCoroutine == null)
        {
            RemoveCoroutine = StartCoroutine(RemoveChunksQueue());
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

                    Chunk_HideOrRemove(WatingToRemove_Chunks[0]);
                    
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

                RemoveCoroutine = null;
                break;
            }



        }

    }


    void Chunk_HideOrRemove(Vector3 chunklocation)
    {
        //���������Χ��ж��,���������
        //if (GetVector3Length(PlayerFoot.transform.position - chunklocation) > (DestroySize * 16f))
        //{
        //    Allchunks.Remove(chunklocation);
        //    obj.DestroyChunk();


        //}
        //else
        //{
        //    obj.HideChunk();
        //}

        obj.HideChunk();


    }
    //---------------------------------------------------------------------------------------





    //-----------------------------------Render Э��-----------------------------------------
    //��ȾЭ�̳�
    void RenderCoroutineManager()
    {
        if (WaitToRender.Count != 0 && Render_Coroutine == null)
        {
            Render_Coroutine = StartCoroutine(Render_0());
        }


    }

    //һ����ȾЭ��
    IEnumerator Render_0()
    {
        while (true)
        {
            WaitToRender.TryDequeue(out Chunk chunktemp);

            //if (chunktemp.isReadyToRender)
            //{
            //    float startTime = Time.realtimeSinceStartup;

            //    chunktemp.CreateMesh();

            //    float executionTime = Time.realtimeSinceStartup - startTime;

            //    // ���ִ��ʱ�䣨���룩
            //    //Debug.Log($"{chunktemp.chunkObject.name}:Execution time: " + executionTime * 1000 + " ms");
            //    dynamicRandertime(executionTime);

            //}

            if (chunktemp.isReadyToRender)
            {
                chunktemp.CreateMesh();
            }

            if (WaitToRender.Count == 0)
            {
                Render_Coroutine = null;
                break;
            }

            //yield return new WaitForSeconds(RenderDelay);
            yield return new WaitForSeconds(RenderDelay);
        }

    }

    //��λΪms������ÿ����Ⱦ����̬�ı���Ⱦʱ��
    void dynamicRandertime(float nowtime)
    {
        if (nowtime > RenderDelay)
        {
            RenderDelay = nowtime;
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


    //���ط�������
    public byte GetBlockType(Vector3 pos)
    {

        Allchunks.TryGetValue(GetChunkLocation(pos), out Chunk chunktemp);

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
    //true������ײ
    public bool CheckForVoxel(Vector3 pos)
    {
        //�����������
        Vector3 vec = GetRelalocation(new Vector3(pos.x, pos.y, pos.z));

        //��һ�¸õ����Ƿ����
        //Allchunks.TryGetValue(GetChunkLocation(pos), out obj);
        //if (obj.myState == false)
        //{
        //    return true;
        //}

        //�ж�XOZ����û�г���
        if (!Allchunks.ContainsKey(GetChunkLocation(pos))) { return true; }

        //�ж�Y����û�г���
        if (vec.y >= VoxelData.ChunkHeight) { return false; }

        //���ӷ���false
        if (GetBlockType(pos) == VoxelData.Bamboo) { return false; }

        //���ع��廹�ǿ���
        return blocktypes[Allchunks[GetChunkLocation(new Vector3(pos.x, pos.y, pos.z))].voxelMap[(int)vec.x, (int)vec.y, (int)vec.z]].isSolid;

    }

    //���ø��������
    public bool eyesCheckForVoxel(Vector3 pos)
    {
        if (!Allchunks.ContainsKey(GetChunkLocation(pos))) { return false; }

        //�����������
        Vector3 vec = GetRelalocation(new Vector3(pos.x, pos.y, pos.z));

        //�ж�XOZ����û�г���
        if (!Allchunks.ContainsKey(GetChunkLocation(pos))) { return true; }

        //�ж�Y����û�г���
        if (vec.y >= VoxelData.ChunkHeight) { return false; }

        //���ӷ���true
        if (GetBlockType(pos) == VoxelData.Bamboo) { return true; }

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
    public float DestroyTime;
    public bool isSolid;
    public bool isTransparent;
    public Sprite icon;

    [Header("Clips")]
    public AudioClip[] walk_clips = new AudioClip[2]; 
    public AudioClip broking_clip;
    public AudioClip broken_clip;

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