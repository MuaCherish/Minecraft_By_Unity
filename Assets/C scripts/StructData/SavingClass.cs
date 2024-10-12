using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// һ��������WorldSetting
[Serializable]
public class WorldSetting
{
    public String date = "0000";//�浵��������
    public String name = "�µ�����";
    public int seed = 0;
    public GameMode gameMode = GameMode.Survival;
    public int worldtype = TerrainData.Biome_Default;
    public Vector3 playerposition;   // ������ҵ�����
    public Quaternion playerrotation; // ������ҵ���ת

    public WorldSetting(int _seed)
    {
        seed = _seed;
    }
}



//����޸ĵ����ݻ���
[Serializable]
public class EditStruct
{
    public Vector3 editPos;
    public byte targetType;


    public EditStruct(Vector3 _editPos, byte _targetType)
    {
        editPos = _editPos;
        targetType = _targetType;
    }

}



//���ձ���Ľṹ��
[Serializable]
public class SavingData
{
    public Vector3 ChunkLocation;
    public List<EditStruct> EditDataInChunkList = new List<EditStruct>();

    // Ϊ�˼��ݷ����л���ԭΪDictionary
    [System.NonSerialized]
    public Dictionary<Vector3, byte> EditDataInChunk = new Dictionary<Vector3, byte>();

    public SavingData(Vector3 _vec, Dictionary<Vector3, byte> _D)
    {
        ChunkLocation = _vec;
        EditDataInChunk = _D;

        // ���ֵ�ת��Ϊ�б�
        foreach (var kvp in _D)
        {
            EditDataInChunkList.Add(new EditStruct(kvp.Key, kvp.Value));
        }
    }

    // �ڷ����л���ԭDictionary
    public void RestoreDictionary()
    {
        EditDataInChunk = new Dictionary<Vector3, byte>();
        foreach (var structItem in EditDataInChunkList)
        {
            EditDataInChunk[structItem.editPos] = structItem.targetType;
        }
    }

    // ����Ƿ����ָ���� ChunkLocation
    public bool ContainsChunkLocation(Vector3 location)
    {
        return ChunkLocation == location;
    }
}


// ΪList���󴴽�һ����װ�࣬�Ա��ܹ�����ת��ΪJSON
[Serializable]
public class Wrapper<T>
{
    public List<T> Items;

    public Wrapper(List<T> items)
    {
        Items = items;
    }
}
