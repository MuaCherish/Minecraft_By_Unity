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

    #region ���ں���

    public override void Handle_GameState_Start_Once()
    {
        base.Handle_GameState_Start_Once();

        InitManager();

    }

    #endregion


    //Save
    #region Saving:�浵����

    [Foldout("��ǰ����浵", true)]
    public bool isFinishSaving = false;
    public string savingPATH = ""; //�浵��Ŀ¼
    public WorldSetting worldSetting;
    public List<SavingData> TheSaving = new List<SavingData>(); //��ȡ�Ĵ浵
    public List<EditStruct> EditNumber = new List<EditStruct>(); //�������
    public bool isLoadSaving = false;
    public bool isFinishUpdateEditNumber = false;

    // ������Ҹ��µľ��巽��
    public List<EditStruct> WaitToAdd_EditList = new List<EditStruct>();

    //Init
    public void InitManager()
    {
        if (!Directory.Exists(savingPATH))
            Directory.CreateDirectory(savingPATH);
        savingPATH = Path.Combine(Application.persistentDataPath);// ʹ�� persistentDataPath ��Ϊ��Ŀ¼
        TheSaving = new List<SavingData>();
        EditNumber = new List<EditStruct>();
        isLoadSaving = false;

        //-------˳���ܱ仯------------------
        MC_Runtime_StaticData.Instance.BiomeData.biomeProperties.terrainLayerProbabilitySystem.Seed = UnityEngine.Random.Range(0, 100000000);
        worldSetting = new WorldSetting(MC_Runtime_StaticData.Instance.BiomeData.biomeProperties.terrainLayerProbabilitySystem.Seed);
        UnityEngine.Random.InitState(worldSetting.seed);
        //-------------------------------------
    }


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
        worldSetting.gameMode = MC_Runtime_DynamicData.instance.GetGameMode();
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
                MC_Runtime_DynamicData.instance.SetGameMode(worldSetting.gameMode);



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

}