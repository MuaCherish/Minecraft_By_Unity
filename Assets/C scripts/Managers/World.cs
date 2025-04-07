using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using System;
using static MC_Static_Unity;
using static MC_Static_Math;
using static MC_Static_Chunk;
using Homebrew;

/// <summary>
/// world����ΪMC_Service_Saving
/// </summary>
public class World : MonoBehaviour
{

    //Save
    #region Saving:�浵����

    [Foldout("�浵ϵͳ", true)]
    [Header("����浵")]
    public bool isFinishSaving = false;
    public string savingPATH = ""; //�浵��Ŀ¼
    public WorldSetting worldSetting;
    public List<SavingData> TheSaving = new List<SavingData>(); //��ȡ�Ĵ浵
    public List<EditStruct> EditNumber = new List<EditStruct>(); //�������
    public bool isLoadSaving = false;
    public bool isFinishUpdateEditNumber = false;

    // ������Ҹ��µľ��巽��
    public List<EditStruct> WaitToAdd_EditList = new List<EditStruct>();

    //ɾ���浵
    public void DeleteSave(string savepath)
    {
        if (Directory.Exists(savepath))
        {
            try
            {
                // ɾ�������ļ�
                foreach (string file in Directory.GetFiles(savepath))
                {
                    File.Delete(file);
                    //Debug.Log($"Deleted file: {file}");
                }

                // �ݹ�ɾ��������Ŀ¼
                foreach (string directory in Directory.GetDirectories(savepath))
                {
                    DeleteSave(directory);
                }

                // ɾ����Ŀ¼
                Directory.Delete(savepath);
                //Debug.Log("���ɾ��");
            }
            catch (Exception ex)
            {
                // �����쳣������־��¼
                Debug.LogError($"ɾ��Ŀ¼ {savepath} ʱ����: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"ָ��·�� {savepath} ������.");
        }
    }

    //��ȡTheSaving������
    public int GetIndexOfChunkLocation(Vector3 location)
    {
        // ���� TheSaving �б�
        for (int i = 0; i < TheSaving.Count; i++)
        {
            var savingData = TheSaving[i];
            // ʹ�� SavingData �� ContainsChunkLocation ������� ChunkLocation
            if (savingData.ContainsChunkLocation(location))
            {
                return i; // ����ƥ���������
            }
        }

        return -1; // ���û���ҵ�ƥ����򷵻� -1
    }

    //����TheSaving�Ƿ����ChunkLocation
    public bool ContainsChunkLocation(Vector3 location)
    {
        // ���� TheSaving �б�
        foreach (var savingData in TheSaving)
        {
            // ʹ�� SavingData �� ContainsChunkLocation ������� ChunkLocation
            if (savingData.ContainsChunkLocation(location))
            {
                return true; // �ҵ�ƥ��� ChunkLocation
            }
        }

        return false; // û���ҵ�ƥ��� ChunkLocation
    }

    //������ұ༭�ķ�������
    public void UpdateEditNumber(Vector3 RealPos, byte targetBlocktype)
    {
        // ���޸�ϸ��������World��
        // ת��RealPosΪ����Vector3�Ա������ֵ��key
        Vector3 intPos = new Vector3((int)RealPos.x, (int)RealPos.y, (int)RealPos.z);

        // �����Ƿ��Ѿ�������ͬ��editPos
        EditStruct existingEdit = EditNumber.Find(edit => edit.editPos == intPos);

        if (existingEdit != null)
        {
            // ������ڣ�����targetType
            existingEdit.targetType = targetBlocktype;
        }
        else
        {
            // ��������ڣ�����µ�EditStruct
            //print($"Edit����: {intPos} --- {targetBlocktype}");
            if (intPos.y >= 0)
            {
                EditNumber.Add(new EditStruct(intPos, targetBlocktype));
            }

        }
    }
    public void UpdateEditNumber(List<EditStruct> _EditList)
    {
        // ����µı༭�б��ȴ�����Ķ���β��
        WaitToAdd_EditList.AddRange(_EditList);
        UpdateEditNumberImmediate();

    }
    public void UpdateEditNumberImmediate()
    {
        while (WaitToAdd_EditList.Count > 0)
        {
            EditStruct edit = WaitToAdd_EditList[0];

            if (edit.targetType != VoxelData.BedRock)
            {
                UpdateEditNumber(edit.editPos, edit.targetType);
            }
            else
            {
                Debug.Log("��������");
            }

            WaitToAdd_EditList.RemoveAt(0);
        }
        isFinishUpdateEditNumber = true;
    }

    // ��EditNumber����
    public void ClassifyWorldData()
    {
        foreach (var edittemp in EditNumber)
        {
            // ��ȡ��ǰ�޸����ڵ�����λ��
            Vector3 _ChunkLocation = GetRelaChunkLocation(edittemp.editPos);

            // ����Ƿ��� savingDatas ���ҵ���Ӧ�� ChunkLocation
            bool found = false;

            // �����Ƿ�����ͬ�� ChunkLocation
            foreach (var savingtemp in TheSaving)
            {
                if (savingtemp.ChunkLocation == _ChunkLocation)
                {
                    // ����ҵ�����Ӧ�� ChunkLocation����������λ�úͷ������͵� EditDataInChunk
                    savingtemp.EditDataInChunk[GetRelaPos(edittemp.editPos)] = edittemp.targetType;
                    found = true;
                    break;  // �ҵ���ֱ������ѭ��
                }
            }

            // ���û���ҵ���Ӧ�� ChunkLocation�����½�һ�� SavingData ����ӵ� savingDatas
            if (!found)
            {
                // �����µ� EditDataInChunk �ֵ䣬����ӵ�ǰ�����λ�úͷ�������
                Dictionary<Vector3, byte> newEditDataInChunk = new Dictionary<Vector3, byte>();
                newEditDataInChunk[GetRelaPos(edittemp.editPos)] = edittemp.targetType;

                // �����µ� SavingData ����ӵ� savingDatas
                SavingData newSavingData = new SavingData(_ChunkLocation, newEditDataInChunk);
                TheSaving.Add(newSavingData);
            }
        }

        SAVINGDATA(savingPATH);
    }

    //�浵
    public void SAVINGDATA(string savePath)
    {
        // ���´浵�ṹ��
        worldSetting.playerposition = managerhub.player.transform.position;
        worldSetting.playerrotation = managerhub.player.transform.rotation;
        worldSetting.gameMode = game_mode;
        string previouDate = worldSetting.date;
        worldSetting.date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        // ����һ����Ϊ "Saves" ���ļ���
        string savesFolderPath = Path.Combine(savePath, "Saves");
        if (!Directory.Exists(savesFolderPath))
        {
            Directory.CreateDirectory(savesFolderPath);
        }

        // �� "Saves" �ļ����´���һ���Դ浵���������������ļ���
        string folderPath = Path.Combine(savesFolderPath, worldSetting.date);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // ɾ��Saves�ļ���������ΪpreviouDate���ļ��У�������ҵ��Ļ�
        string oldFolderPath = Path.Combine(savesFolderPath, previouDate);
        if (Directory.Exists(oldFolderPath))
        {
            // ɾ�����ļ��м�����������
            Directory.Delete(oldFolderPath, true);
        }

        // �����е� SavingData ���ֵ�ת��Ϊ�б�
        foreach (var data in TheSaving)
        {
            data.EditDataInChunkList = new List<EditStruct>();
            foreach (var kvp in data.EditDataInChunk)
            {
                data.EditDataInChunkList.Add(new EditStruct(kvp.Key, kvp.Value));
            }
        }

        // �� worldSetting �� savingDatas ת��Ϊ JSON �ַ���
        string worldSettingJson = JsonUtility.ToJson(worldSetting, true);
        string savingDatasJson = JsonUtility.ToJson(new Wrapper<SavingData>(TheSaving), true);

        // �� JSON �ַ������浽ָ��·�����ļ�����
        File.WriteAllText(Path.Combine(folderPath, "WorldSetting.json"), worldSettingJson);
        File.WriteAllText(Path.Combine(folderPath, "SavingDatas.json"), savingDatasJson);



        Debug.Log("�����ѱ��浽: " + folderPath);
        isFinishSaving = true;
    }

    //��ȡȫ���浵
    public void LoadAllSaves(string savingPATH)
    {
        // ���� Saves Ŀ¼��·��
        string savesFolderPath = Path.Combine(savingPATH, "Saves");

        // ��� Saves Ŀ¼�Ƿ���ڣ�����������򴴽���
        if (!Directory.Exists(savesFolderPath))
        {
            try
            {
                Directory.CreateDirectory(savesFolderPath);
                //Debug.Log($"Saves �ļ����Ѵ���: {savesFolderPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"���� Saves �ļ���ʱ����: {ex.Message}");
                return; // �����ļ���ʧ��ʱ�˳�����
            }
        }

        // ��ȡ�浵Ŀ¼�µ������ļ���·��
        string[] saveDirectories;
        try
        {
            saveDirectories = Directory.GetDirectories(savesFolderPath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"��ȡ�浵Ŀ¼ʱ����: {ex.Message}");
            return; // �����쳣���˳�����
        }

        // ����ÿ���浵�ļ���
        foreach (string saveDirectory in saveDirectories)
        {
            // �����ǰ�浵�ļ�������
            string folderName = Path.GetFileName(saveDirectory);
            //Debug.Log($"Loading save from folder: {folderName}");

            // ���� LoadData ������ȡ��ǰ�浵
            try
            {
                WorldSetting _worldsetting = LoadWorldSetting(saveDirectory);
                managerhub.canvasManager.NewWorldGenerate(
                    _worldsetting.name,
                    _worldsetting.date,
                    _worldsetting.gameMode,
                    _worldsetting.worldtype,
                    _worldsetting.seed
                );
            }
            catch (Exception ex)
            {
                Debug.LogError($"���ش浵 {folderName} ʱ����: {ex.Message}");
            }
        }

        //Debug.Log($"Total saves loaded: {saveDirectories.Length}");
    }

    //�����浵����
    public WorldSetting LoadWorldSetting(string savePath)
    {
        // ���� WorldSetting.json �ļ�������·��
        string worldSettingPath = Path.Combine(savePath, "WorldSetting.json");

        // ��� WorldSetting.json �ļ��Ƿ����
        if (File.Exists(worldSettingPath))
        {
            // ��ȡ WorldSetting.json �ļ�������
            string worldSettingJson = File.ReadAllText(worldSettingPath);

            // �� JSON �ַ��������л�Ϊ WorldSetting ����
            return JsonUtility.FromJson<WorldSetting>(worldSettingJson);
        }
        else
        {
            // ����ļ������ڣ����������Ϣ������ null
            Debug.LogError("�Ҳ��� WorldSetting.json �ļ�");
            return null;
        }
    }

    //���ش浵
    public void LoadSavingData(string savePath)
    {
        isLoadSaving = true;

        // ���� SavingDatas.json �ļ�������·��
        string savingDatasPath = Path.Combine(savePath, "SavingDatas.json");

        // ��� SavingDatas.json �ļ��Ƿ����
        if (File.Exists(savingDatasPath))
        {
            // ��ȡ SavingDatas.json �ļ�������
            string savingDatasJson = File.ReadAllText(savingDatasPath);

            // �� JSON �ַ��������л�Ϊ Wrapper<SavingData> ����
            Wrapper<SavingData> wrapper = JsonUtility.FromJson<Wrapper<SavingData>>(savingDatasJson);

            // ��� wrapper �Ƿ�Ϊ null
            if (wrapper != null && wrapper.Items != null)
            {
                // �������л��� Items �б�ֵ�� TheSaving
                TheSaving = wrapper.Items;

                // ���� TheSaving �б��ָ�ÿ�� SavingData �����е��ֵ�
                foreach (var data in TheSaving)
                {
                    data.RestoreDictionary();

                    // Debug��ӡ
                    //Debug.Log($"Chunk Location: {data.ChunkLocation}");
                    //foreach (var pair in data.EditDataInChunk)
                    //{
                    //    Debug.Log($"Position: {pair.Key}, Type: {pair.Value}");
                    //}
                }

                worldSetting = LoadWorldSetting(savePath);


                //����һЩ����
                game_mode = worldSetting.gameMode;



            }
            else
            {
                // ��� wrapper �� wrapper.Items Ϊ null�����������Ϣ
                Debug.LogWarning("Wrapper<SavingData> �� Items �б�Ϊ null");
            }
        }
        else
        {
            // ����ļ������ڣ����������Ϣ
            Debug.LogError("�Ҳ��� SavingDatas.json �ļ�");
        }
    }

    #endregion

    #region ״̬

    [Foldout("״̬", true)]
    [Header("��Ϸ״̬")][ReadOnly] public Game_State game_state = Game_State.Start;
    [Header("��Ϸģʽ")][ReadOnly] public GameMode game_mode = GameMode.Survival;


    #endregion

    #region ���ں���

    ManagerHub managerhub;
    Player player;
    MC_Service_Chunk Service_Chunk;

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        player = managerhub.player;
        ChunkParent = SceneData.GetChunkParent();
        Service_Chunk = SceneData.GetService_Chunk();
    }

    bool hasExec_Start = true;
    bool hasExec_Loading = true;
    bool hasExec_Playing = true;
    bool hasExec_Pause = true;
    private void Update()
    {
        switch (game_state)
        {
            case Game_State.Start:
                Handle_GameState_Start();
                if (!hasExec_Loading)
                    hasExec_Loading = true;
                if (!hasExec_Playing)
                    hasExec_Playing = true;
                if (!hasExec_Pause)
                    hasExec_Pause = true;
                break;

            case Game_State.Loading:
                Handle_GameState_Loading();
                if (!hasExec_Start)
                    hasExec_Start = true;
                if (!hasExec_Playing)
                    hasExec_Playing = true;
                if (!hasExec_Pause)
                    hasExec_Pause = true;
                break;

            case Game_State.Playing:
                Handle_GameState_Playing();
                if (!hasExec_Start)
                    hasExec_Start = true;
                if (!hasExec_Loading)
                    hasExec_Loading = true;
                if (!hasExec_Pause)
                    hasExec_Pause = true;
                break;

            case Game_State.Pause:
                Handle_GameState_Pause();
                if (!hasExec_Start)
                    hasExec_Start = true;
                if (!hasExec_Loading)
                    hasExec_Loading = true;
                if (!hasExec_Playing)
                    hasExec_Playing = true;
                break;
        }

    }

    void Handle_GameState_Start()
    {
        if(hasExec_Start)
        {
            InitWorldManager();
            hasExec_Start = false;
            isFinishUpdateEditNumber = false;
        }
    }

    void Handle_GameState_Loading()
    {
        if (hasExec_Loading)
        {
            hasExec_Loading = false;
        }
    }

    void Handle_GameState_Playing()
    {
        if(hasExec_Playing)
        {
            hasExec_Playing = false;
        }
    }

    void Handle_GameState_Pause()
    {
        if (hasExec_Pause)
        {
            hasExec_Pause = false;
        }
    }

    void OnApplicationQuit()
    {

        //������Ϸ
        game_state = Game_State.Ending;

        //�ȴ�Render�߳�
        if (myThread_Render != null && myThread_Render.IsAlive)
        {

            myThread_Render.Join(); // �ȴ��̰߳�ȫ����ֹ

        }

    }


    #endregion

    #region (���Ż�)����

    [Foldout("Block/Item", true)]
    [Header("Material-�������� + ��������")]
    public Material material;
    public Material material_Water;
    [Header("Block/Item��Ϣ")] public BlockType[] blocktypes; 
    public BlockType[] BackUp_blocktypes;
    [Header("BlockTexture(���ڵ�����)")] public Texture2D atlasTexture;

    [Foldout("Chunk", true)]
    [Header("World-��Ⱦ����")]
    [Tooltip("4���Ǳ߳�Ϊ4*16��������")] public int renderSize = 5;        //��Ⱦ����뾶,��renderSize*16f
    [Tooltip("2���ǽӽ�2*16��ʱ��ʼˢ������")] public float StartToRender = 1f;
    public float DestroySize = 7f;

    [Foldout("Noise", true)]
    [Header("Ⱥϵ�������ʺ�����")]
    public BiomeProperties _biomeProperties;

    [Foldout("Player", true)]
    [Header("��ҳ�����")] public Vector3 Start_Position = new Vector3(1600f, 127f, 1600f);


    [Foldout("��̬���ص�ͼ����", true)]
    public GameObject ChunkParent;
    [HideInInspector] public bool �Ƿ�����Chunk���� = false;
    public Dictionary<Vector3, Chunk> Allchunks = new Dictionary<Vector3, Chunk>();
    public readonly object Allchunks_Lock = new object();
    public List<Vector3> WatingToCreate_Chunks = new List<Vector3>();
    public List<Vector3> WatingToRemove_Chunks = new List<Vector3>();
    public Chunk obj;

    [Foldout("Э���ӳ�ʱ��", true)]
    public float InitCorountineDelay = 1f;
    public float CreateCoroutineDelay = 0.5f;
    public float RemoveCoroutineDelay = 0.5f;
    public float RenderDelay = 0.1f;
    public int Mesh_0_TaskCount = 0;

    [Foldout("���ɷ���", true)]
    public Vector3 Center_Now;
    public Vector3 Center_direction; //��������˷���

    [Foldout("��ʱ", true)]
    public float InitStartTime;
    public float InitEndTime;

    [Foldout("Create && Remove Э��", true)]
    public Coroutine CreateCoroutine;
    public Coroutine RemoveCoroutine;
    public ConcurrentQueue<Chunk> WaitToFlashChunkQueue = new ConcurrentQueue<Chunk>();
    public Coroutine Init_Map_Thread_NoInit_Coroutine;

    [Foldout("��ʼ����ͼ", true)]
    public Coroutine Init_MapCoroutine;
    public bool hasExec_RandomPlayerLocation = true;

    public Thread myThread_Render;
    public int Delay_RenderMesh = 1000;
    public ConcurrentQueue<Chunk> WaitToRender_New = new ConcurrentQueue<Chunk>();

    //Render_0 && Render_1 Э��
    [HideInInspector] public bool RenderLock = false;
    public ConcurrentQueue<Chunk> WaitToRender = new ConcurrentQueue<Chunk>();
    public ConcurrentQueue<Chunk> WaitToRender_temp = new ConcurrentQueue<Chunk>();
    public Coroutine Render_Coroutine;

    //Threading
    [HideInInspector] public bool MeshLock = false;
    public ConcurrentQueue<Chunk> WaitToCreateMesh = new ConcurrentQueue<Chunk>();
    public Coroutine Mesh_Coroutine;


    #endregion

    //World
    #region [MC_Service_World] �߼�����

    //һ���Դ���
    bool hasExec = true;
    bool hasExec_SetSeed = true;

    public void InitWorldManager()
    {
        hasExec_RandomPlayerLocation = true;
        game_mode = GameMode.Survival;

        isLoadSaving = false;
        �Ƿ�����Chunk���� = managerhub.�Ƿ�����Chunk����;

        // ʹ�� persistentDataPath ��Ϊ��Ŀ¼
        savingPATH = Path.Combine(Application.persistentDataPath);

        // ȷ��Ŀ¼����
        if (!Directory.Exists(savingPATH))
        {
            Directory.CreateDirectory(savingPATH);
        }

        // ��ӡĿ¼·����ȷ��
        //Debug.Log("�浵Ŀ¼: " + savingPATH);


        //��ʼ��
        Start_Position = new Vector3(1600f, 127f, 1600f);

        game_state = Game_State.Start;
        TheSaving = new List<SavingData>();
        EditNumber = new List<EditStruct>();
        //savingDatas = new List<SavingData>();
        renderSize = 10;
        StartToRender = 1f;
        DestroySize = 7f;

        Allchunks = new Dictionary<Vector3, Chunk>();
        WatingToCreate_Chunks = new List<Vector3>();
        WatingToRemove_Chunks = new List<Vector3>();
        myThread_Render = null;
        WaitToRender_New = new ConcurrentQueue<Chunk>();
        //�Ƿ�����Chunk���� = false;
        Center_Now = Vector3.zero;
        Center_direction = Vector3.zero;
        hasExec = true;
        hasExec_SetSeed = true;
        CreateCoroutine = null;
        RemoveCoroutine = null;
        Render_Coroutine = null;
        Mesh_Coroutine = null;
        MeshLock = false;
        WaitToCreateMesh = new ConcurrentQueue<Chunk>();
        RenderLock = false;
        WaitToRender = new ConcurrentQueue<Chunk>();
        WaitToRender_temp = new ConcurrentQueue<Chunk>();
        WaitToFlashChunkQueue = new ConcurrentQueue<Chunk>();

        // ���� Chunks ���Ŀ¼�µ�����������
        foreach (Transform child in ChunkParent.transform)
        {
            Destroy(child.gameObject);
        }

        //-------˳���ܱ仯------------------
        _biomeProperties.terrainLayerProbabilitySystem.Seed = UnityEngine.Random.Range(0, 100000000);
        worldSetting = new WorldSetting(_biomeProperties.terrainLayerProbabilitySystem.Seed);
        UnityEngine.Random.InitState(worldSetting.seed);
        //-------------------------------------

        managerhub.canvasManager.PointSaving = "";

        //��ʼ������ʱ��
        InitStartTime = 0f;
        InitEndTime = 0f;

        if (managerhub.������ģʽ)
        {
            renderSize = 2;
        }
        if (managerhub.�޺�ҹģʽ)
        {
            if (managerhub.timeManager.gameObject.activeSelf)
                managerhub.timeManager.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {

        //Mesh�̳߳�פ
        Service_Chunk.CreateMeshCoroutineManager();


        //��Ⱦ�̳߳�פ
        Service_Chunk.RenderCoroutineManager();



        //��ʼ����ͼ
        if (game_state == Game_State.Loading)
        {

            if (hasExec_SetSeed)
            {
                InitStartTime = Time.time;
                //��ȡ��ǰģʽ
                //if (canvasManager.currentWorldType == 6)
                //{
                //    //SuperPlainMode = true;
                //}

                //worldSetting.worldtype = canvasManager.currentWorldType

                //��ʼ��ʼ��
                Service_Chunk.Update_CenterChunks(true);

                hasExec_SetSeed = false;
            }


        }


        //��Ϸ��ʼ
        if (game_state == Game_State.Playing)
        {

            if (hasExec)
            {

                LockMouse(true);
                hasExec = false;
            }


            //����ƶ�ˢ��
            //�������16f
            if (Get2DLengthforVector3(player.foot.transform.position - Center_Now) > (StartToRender * 16f) && Get2DLengthforVector3(player.foot.transform.position - Center_Now) <= ((StartToRender + 1) * 16f))
            {

                //����Center
                Center_direction = NormalizeToAxis(player.foot.transform.position - Center_Now);
                Center_Now += Center_direction * TerrainData.ChunkWidth;

                //���Chunk
                Service_Chunk.AddtoCreateChunks(Center_direction);
                Service_Chunk.AddtoRemoveChunks(Center_direction);

            }
            //����ƶ���Զ����
            else if (Get2DLengthforVector3(player.foot.transform.position - Center_Now) > ((StartToRender + 1) * 16f))
            {



                Service_Chunk.Update_CenterWithNoInit();




            }


        }

    }

   

    #endregion

}