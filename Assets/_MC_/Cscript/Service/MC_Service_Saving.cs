using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using static MC_Static_Math;
using static MC_Static_Chunk;
using Homebrew;


public class MC_Service_Saving : MC_Tick_Base
{

    #region 周期函数

    public override void Handle_GameState_Start_Once()
    {
        base.Handle_GameState_Start_Once();

        InitManager();

    }

    #endregion


    //Save
    #region Saving:存档管理

    [Foldout("当前世界存档", true)]
    public bool isFinishSaving = false;
    public string savingPATH = ""; //存档根目录
    public WorldSetting worldSetting;
    public List<SavingData> TheSaving = new List<SavingData>(); //读取的存档
    public List<EditStruct> EditNumber = new List<EditStruct>(); //玩家数据
    public bool isLoadSaving = false;
    public bool isFinishUpdateEditNumber = false;

    // 推送玩家更新的具体方块
    public List<EditStruct> WaitToAdd_EditList = new List<EditStruct>();

    //Init
    public void InitManager()
    {
        if (!Directory.Exists(savingPATH))
            Directory.CreateDirectory(savingPATH);
        savingPATH = Path.Combine(Application.persistentDataPath);// 使用 persistentDataPath 作为根目录
        TheSaving = new List<SavingData>();
        EditNumber = new List<EditStruct>();
        isLoadSaving = false;

        //-------顺序不能变化------------------
        MC_Runtime_StaticData.Instance.BiomeData.biomeProperties.terrainLayerProbabilitySystem.Seed = UnityEngine.Random.Range(0, 100000000);
        worldSetting = new WorldSetting(MC_Runtime_StaticData.Instance.BiomeData.biomeProperties.terrainLayerProbabilitySystem.Seed);
        UnityEngine.Random.InitState(worldSetting.seed);
        //-------------------------------------
    }


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
        worldSetting.gameMode = MC_Runtime_DynamicData.instance.GetGameMode();
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
                MC_Runtime_DynamicData.instance.SetGameMode(worldSetting.gameMode);



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

}