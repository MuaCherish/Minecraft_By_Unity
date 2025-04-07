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
/// world将改为MC_Service_Saving
/// </summary>
public class World : MonoBehaviour
{

    //Save
    #region Saving:存档管理

    [Foldout("存档系统", true)]
    [Header("世界存档")]
    public bool isFinishSaving = false;
    public string savingPATH = ""; //存档根目录
    public WorldSetting worldSetting;
    public List<SavingData> TheSaving = new List<SavingData>(); //读取的存档
    public List<EditStruct> EditNumber = new List<EditStruct>(); //玩家数据
    public bool isLoadSaving = false;
    public bool isFinishUpdateEditNumber = false;

    // 推送玩家更新的具体方块
    public List<EditStruct> WaitToAdd_EditList = new List<EditStruct>();

    //删除存档
    public void DeleteSave(string savepath)
    {
        if (Directory.Exists(savepath))
        {
            try
            {
                // 删除所有文件
                foreach (string file in Directory.GetFiles(savepath))
                {
                    File.Delete(file);
                    //Debug.Log($"Deleted file: {file}");
                }

                // 递归删除所有子目录
                foreach (string directory in Directory.GetDirectories(savepath))
                {
                    DeleteSave(directory);
                }

                // 删除空目录
                Directory.Delete(savepath);
                //Debug.Log("完成删除");
            }
            catch (Exception ex)
            {
                // 处理异常，如日志记录
                Debug.LogError($"删除目录 {savepath} 时出错: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"指定路径 {savepath} 不存在.");
        }
    }

    //获取TheSaving的索引
    public int GetIndexOfChunkLocation(Vector3 location)
    {
        // 遍历 TheSaving 列表
        for (int i = 0; i < TheSaving.Count; i++)
        {
            var savingData = TheSaving[i];
            // 使用 SavingData 的 ContainsChunkLocation 方法检查 ChunkLocation
            if (savingData.ContainsChunkLocation(location))
            {
                return i; // 返回匹配项的索引
            }
        }

        return -1; // 如果没有找到匹配项，则返回 -1
    }

    //返回TheSaving是否包含ChunkLocation
    public bool ContainsChunkLocation(Vector3 location)
    {
        // 遍历 TheSaving 列表
        foreach (var savingData in TheSaving)
        {
            // 使用 SavingData 的 ContainsChunkLocation 方法检查 ChunkLocation
            if (savingData.ContainsChunkLocation(location))
            {
                return true; // 找到匹配的 ChunkLocation
            }
        }

        return false; // 没有找到匹配的 ChunkLocation
    }

    //更新玩家编辑的方块序列
    public void UpdateEditNumber(Vector3 RealPos, byte targetBlocktype)
    {
        // 将修改细节推送至World里
        // 转换RealPos为整型Vector3以便用作字典的key
        Vector3 intPos = new Vector3((int)RealPos.x, (int)RealPos.y, (int)RealPos.z);

        // 查找是否已经存在相同的editPos
        EditStruct existingEdit = EditNumber.Find(edit => edit.editPos == intPos);

        if (existingEdit != null)
        {
            // 如果存在，更新targetType
            existingEdit.targetType = targetBlocktype;
        }
        else
        {
            // 如果不存在，添加新的EditStruct
            //print($"Edit更新: {intPos} --- {targetBlocktype}");
            if (intPos.y >= 0)
            {
                EditNumber.Add(new EditStruct(intPos, targetBlocktype));
            }

        }
    }
    public void UpdateEditNumber(List<EditStruct> _EditList)
    {
        // 添加新的编辑列表到等待处理的队列尾部
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
                Debug.Log("处理到基岩");
            }

            WaitToAdd_EditList.RemoveAt(0);
        }
        isFinishUpdateEditNumber = true;
    }

    // 将EditNumber归类
    public void ClassifyWorldData()
    {
        foreach (var edittemp in EditNumber)
        {
            // 获取当前修改所在的区块位置
            Vector3 _ChunkLocation = GetRelaChunkLocation(edittemp.editPos);

            // 标记是否在 savingDatas 中找到相应的 ChunkLocation
            bool found = false;

            // 查找是否有相同的 ChunkLocation
            foreach (var savingtemp in TheSaving)
            {
                if (savingtemp.ChunkLocation == _ChunkLocation)
                {
                    // 如果找到了相应的 ChunkLocation，则添加相对位置和方块类型到 EditDataInChunk
                    savingtemp.EditDataInChunk[GetRelaPos(edittemp.editPos)] = edittemp.targetType;
                    found = true;
                    break;  // 找到后直接跳出循环
                }
            }

            // 如果没有找到对应的 ChunkLocation，则新建一个 SavingData 并添加到 savingDatas
            if (!found)
            {
                // 创建新的 EditDataInChunk 字典，并添加当前的相对位置和方块类型
                Dictionary<Vector3, byte> newEditDataInChunk = new Dictionary<Vector3, byte>();
                newEditDataInChunk[GetRelaPos(edittemp.editPos)] = edittemp.targetType;

                // 创建新的 SavingData 并添加到 savingDatas
                SavingData newSavingData = new SavingData(_ChunkLocation, newEditDataInChunk);
                TheSaving.Add(newSavingData);
            }
        }

        SAVINGDATA(savingPATH);
    }

    //存档
    public void SAVINGDATA(string savePath)
    {
        // 更新存档结构体
        worldSetting.playerposition = managerhub.player.transform.position;
        worldSetting.playerrotation = managerhub.player.transform.rotation;
        worldSetting.gameMode = game_mode;
        string previouDate = worldSetting.date;
        worldSetting.date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        // 创建一个名为 "Saves" 的文件夹
        string savesFolderPath = Path.Combine(savePath, "Saves");
        if (!Directory.Exists(savesFolderPath))
        {
            Directory.CreateDirectory(savesFolderPath);
        }

        // 在 "Saves" 文件夹下创建一个以存档创建日期命名的文件夹
        string folderPath = Path.Combine(savesFolderPath, worldSetting.date);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 删除Saves文件夹中名字为previouDate的文件夹，如果能找到的话
        string oldFolderPath = Path.Combine(savesFolderPath, previouDate);
        if (Directory.Exists(oldFolderPath))
        {
            // 删除旧文件夹及其所有内容
            Directory.Delete(oldFolderPath, true);
        }

        // 将所有的 SavingData 的字典转换为列表
        foreach (var data in TheSaving)
        {
            data.EditDataInChunkList = new List<EditStruct>();
            foreach (var kvp in data.EditDataInChunk)
            {
                data.EditDataInChunkList.Add(new EditStruct(kvp.Key, kvp.Value));
            }
        }

        // 将 worldSetting 和 savingDatas 转换为 JSON 字符串
        string worldSettingJson = JsonUtility.ToJson(worldSetting, true);
        string savingDatasJson = JsonUtility.ToJson(new Wrapper<SavingData>(TheSaving), true);

        // 将 JSON 字符串保存到指定路径的文件夹中
        File.WriteAllText(Path.Combine(folderPath, "WorldSetting.json"), worldSettingJson);
        File.WriteAllText(Path.Combine(folderPath, "SavingDatas.json"), savingDatasJson);



        Debug.Log("数据已保存到: " + folderPath);
        isFinishSaving = true;
    }

    //读取全部存档
    public void LoadAllSaves(string savingPATH)
    {
        // 构造 Saves 目录的路径
        string savesFolderPath = Path.Combine(savingPATH, "Saves");

        // 检查 Saves 目录是否存在，如果不存在则创建它
        if (!Directory.Exists(savesFolderPath))
        {
            try
            {
                Directory.CreateDirectory(savesFolderPath);
                //Debug.Log($"Saves 文件夹已创建: {savesFolderPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建 Saves 文件夹时出错: {ex.Message}");
                return; // 创建文件夹失败时退出函数
            }
        }

        // 获取存档目录下的所有文件夹路径
        string[] saveDirectories;
        try
        {
            saveDirectories = Directory.GetDirectories(savesFolderPath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"获取存档目录时出错: {ex.Message}");
            return; // 处理异常并退出函数
        }

        // 遍历每个存档文件夹
        foreach (string saveDirectory in saveDirectories)
        {
            // 输出当前存档文件夹名称
            string folderName = Path.GetFileName(saveDirectory);
            //Debug.Log($"Loading save from folder: {folderName}");

            // 调用 LoadData 函数读取当前存档
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
                Debug.LogError($"加载存档 {folderName} 时出错: {ex.Message}");
            }
        }

        //Debug.Log($"Total saves loaded: {saveDirectories.Length}");
    }

    //分析存档名字
    public WorldSetting LoadWorldSetting(string savePath)
    {
        // 构建 WorldSetting.json 文件的完整路径
        string worldSettingPath = Path.Combine(savePath, "WorldSetting.json");

        // 检查 WorldSetting.json 文件是否存在
        if (File.Exists(worldSettingPath))
        {
            // 读取 WorldSetting.json 文件的内容
            string worldSettingJson = File.ReadAllText(worldSettingPath);

            // 将 JSON 字符串反序列化为 WorldSetting 对象
            return JsonUtility.FromJson<WorldSetting>(worldSettingJson);
        }
        else
        {
            // 如果文件不存在，输出错误信息并返回 null
            Debug.LogError("找不到 WorldSetting.json 文件");
            return null;
        }
    }

    //加载存档
    public void LoadSavingData(string savePath)
    {
        isLoadSaving = true;

        // 构建 SavingDatas.json 文件的完整路径
        string savingDatasPath = Path.Combine(savePath, "SavingDatas.json");

        // 检查 SavingDatas.json 文件是否存在
        if (File.Exists(savingDatasPath))
        {
            // 读取 SavingDatas.json 文件的内容
            string savingDatasJson = File.ReadAllText(savingDatasPath);

            // 将 JSON 字符串反序列化为 Wrapper<SavingData> 对象
            Wrapper<SavingData> wrapper = JsonUtility.FromJson<Wrapper<SavingData>>(savingDatasJson);

            // 检查 wrapper 是否为 null
            if (wrapper != null && wrapper.Items != null)
            {
                // 将反序列化的 Items 列表赋值给 TheSaving
                TheSaving = wrapper.Items;

                // 遍历 TheSaving 列表，恢复每个 SavingData 对象中的字典
                foreach (var data in TheSaving)
                {
                    data.RestoreDictionary();

                    // Debug打印
                    //Debug.Log($"Chunk Location: {data.ChunkLocation}");
                    //foreach (var pair in data.EditDataInChunk)
                    //{
                    //    Debug.Log($"Position: {pair.Key}, Type: {pair.Value}");
                    //}
                }

                worldSetting = LoadWorldSetting(savePath);


                //更新一些参数
                game_mode = worldSetting.gameMode;



            }
            else
            {
                // 如果 wrapper 或 wrapper.Items 为 null，输出警告信息
                Debug.LogWarning("Wrapper<SavingData> 或 Items 列表为 null");
            }
        }
        else
        {
            // 如果文件不存在，输出错误信息
            Debug.LogError("找不到 SavingDatas.json 文件");
        }
    }

    #endregion

    #region 状态

    [Foldout("状态", true)]
    [Header("游戏状态")][ReadOnly] public Game_State game_state = Game_State.Start;
    [Header("游戏模式")][ReadOnly] public GameMode game_mode = GameMode.Survival;


    #endregion

    #region 周期函数

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

        //结束游戏
        game_state = Game_State.Ending;

        //等待Render线程
        if (myThread_Render != null && myThread_Render.IsAlive)
        {

            myThread_Render.Join(); // 等待线程安全地终止

        }

    }


    #endregion

    #region (待优化)变量

    [Foldout("Block/Item", true)]
    [Header("Material-方块类型 + 工具类型")]
    public Material material;
    public Material material_Water;
    [Header("Block/Item信息")] public BlockType[] blocktypes; 
    public BlockType[] BackUp_blocktypes;
    [Header("BlockTexture(用于掉落物)")] public Texture2D atlasTexture;

    [Foldout("Chunk", true)]
    [Header("World-渲染设置")]
    [Tooltip("4就是边长为4*16的正方形")] public int renderSize = 5;        //渲染区块半径,即renderSize*16f
    [Tooltip("2就是接近2*16的时候开始刷新区块")] public float StartToRender = 1f;
    public float DestroySize = 7f;

    [Foldout("Noise", true)]
    [Header("群系特征概率和数据")]
    public BiomeProperties _biomeProperties;

    [Foldout("Player", true)]
    [Header("玩家出生点")] public Vector3 Start_Position = new Vector3(1600f, 127f, 1600f);


    [Foldout("动态加载地图变量", true)]
    public GameObject ChunkParent;
    [HideInInspector] public bool 是否生成Chunk侧面 = false;
    public Dictionary<Vector3, Chunk> Allchunks = new Dictionary<Vector3, Chunk>();
    public readonly object Allchunks_Lock = new object();
    public List<Vector3> WatingToCreate_Chunks = new List<Vector3>();
    public List<Vector3> WatingToRemove_Chunks = new List<Vector3>();
    public Chunk obj;

    [Foldout("协程延迟时间", true)]
    public float InitCorountineDelay = 1f;
    public float CreateCoroutineDelay = 0.5f;
    public float RemoveCoroutineDelay = 0.5f;
    public float RenderDelay = 0.1f;
    public int Mesh_0_TaskCount = 0;

    [Foldout("生成方向", true)]
    public Vector3 Center_Now;
    public Vector3 Center_direction; //这个代表了方向

    [Foldout("计时", true)]
    public float InitStartTime;
    public float InitEndTime;

    [Foldout("Create && Remove 协程", true)]
    public Coroutine CreateCoroutine;
    public Coroutine RemoveCoroutine;
    public ConcurrentQueue<Chunk> WaitToFlashChunkQueue = new ConcurrentQueue<Chunk>();
    public Coroutine Init_Map_Thread_NoInit_Coroutine;

    [Foldout("初始化地图", true)]
    public Coroutine Init_MapCoroutine;
    public bool hasExec_RandomPlayerLocation = true;

    public Thread myThread_Render;
    public int Delay_RenderMesh = 1000;
    public ConcurrentQueue<Chunk> WaitToRender_New = new ConcurrentQueue<Chunk>();

    //Render_0 && Render_1 协程
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
    #region [MC_Service_World] 逻辑处理

    //一次性代码
    bool hasExec = true;
    bool hasExec_SetSeed = true;

    public void InitWorldManager()
    {
        hasExec_RandomPlayerLocation = true;
        game_mode = GameMode.Survival;

        isLoadSaving = false;
        是否生成Chunk侧面 = managerhub.是否生成Chunk侧面;

        // 使用 persistentDataPath 作为根目录
        savingPATH = Path.Combine(Application.persistentDataPath);

        // 确保目录存在
        if (!Directory.Exists(savingPATH))
        {
            Directory.CreateDirectory(savingPATH);
        }

        // 打印目录路径以确认
        //Debug.Log("存档目录: " + savingPATH);


        //初始化
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
        //是否生成Chunk侧面 = false;
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

        // 销毁 Chunks 大纲目录下的所有子物体
        foreach (Transform child in ChunkParent.transform)
        {
            Destroy(child.gameObject);
        }

        //-------顺序不能变化------------------
        _biomeProperties.terrainLayerProbabilitySystem.Seed = UnityEngine.Random.Range(0, 100000000);
        worldSetting = new WorldSetting(_biomeProperties.terrainLayerProbabilitySystem.Seed);
        UnityEngine.Random.InitState(worldSetting.seed);
        //-------------------------------------

        managerhub.canvasManager.PointSaving = "";

        //初始化计数时间
        InitStartTime = 0f;
        InitEndTime = 0f;

        if (managerhub.低区块模式)
        {
            renderSize = 2;
        }
        if (managerhub.无黑夜模式)
        {
            if (managerhub.timeManager.gameObject.activeSelf)
                managerhub.timeManager.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {

        //Mesh线程常驻
        Service_Chunk.CreateMeshCoroutineManager();


        //渲染线程常驻
        Service_Chunk.RenderCoroutineManager();



        //初始化地图
        if (game_state == Game_State.Loading)
        {

            if (hasExec_SetSeed)
            {
                InitStartTime = Time.time;
                //获取当前模式
                //if (canvasManager.currentWorldType == 6)
                //{
                //    //SuperPlainMode = true;
                //}

                //worldSetting.worldtype = canvasManager.currentWorldType

                //开始初始化
                Service_Chunk.Update_CenterChunks(true);

                hasExec_SetSeed = false;
            }


        }


        //游戏开始
        if (game_state == Game_State.Playing)
        {

            if (hasExec)
            {

                LockMouse(true);
                hasExec = false;
            }


            //玩家移动刷新
            //如果大于16f
            if (Get2DLengthforVector3(player.foot.transform.position - Center_Now) > (StartToRender * 16f) && Get2DLengthforVector3(player.foot.transform.position - Center_Now) <= ((StartToRender + 1) * 16f))
            {

                //更新Center
                Center_direction = NormalizeToAxis(player.foot.transform.position - Center_Now);
                Center_Now += Center_direction * TerrainData.ChunkWidth;

                //添加Chunk
                Service_Chunk.AddtoCreateChunks(Center_direction);
                Service_Chunk.AddtoRemoveChunks(Center_direction);

            }
            //玩家移动过远距离
            else if (Get2DLengthforVector3(player.foot.transform.position - Center_Now) > ((StartToRender + 1) * 16f))
            {



                Service_Chunk.Update_CenterWithNoInit();




            }


        }

    }

   

    #endregion

}