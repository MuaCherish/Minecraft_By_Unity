using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 一个完整的WorldSetting
[Serializable]
public class WorldSetting
{
    public String date = "0000";//存档创建日期
    public String name = "新的世界";
    public int seed = 0;
    public GameMode gameMode = GameMode.Survival;
    public int worldtype = TerrainData.Biome_Default;
    public Vector3 playerposition;   // 保存玩家的坐标
    public Quaternion playerrotation; // 保存玩家的旋转

    public WorldSetting(int _seed)
    {
        seed = _seed;
    }
}



//玩家修改的数据缓存
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



//最终保存的结构体
[Serializable]
public class SavingData
{
    public Vector3 ChunkLocation;
    public List<EditStruct> EditDataInChunkList = new List<EditStruct>();

    // 为了兼容反序列化后还原为Dictionary
    [System.NonSerialized]
    public Dictionary<Vector3, byte> EditDataInChunk = new Dictionary<Vector3, byte>();

    public SavingData(Vector3 _vec, Dictionary<Vector3, byte> _D)
    {
        ChunkLocation = _vec;
        EditDataInChunk = _D;

        // 将字典转换为列表
        foreach (var kvp in _D)
        {
            EditDataInChunkList.Add(new EditStruct(kvp.Key, kvp.Value));
        }
    }

    // 在反序列化后还原Dictionary
    public void RestoreDictionary()
    {
        EditDataInChunk = new Dictionary<Vector3, byte>();
        foreach (var structItem in EditDataInChunkList)
        {
            EditDataInChunk[structItem.editPos] = structItem.targetType;
        }
    }

    // 检查是否包含指定的 ChunkLocation
    public bool ContainsChunkLocation(Vector3 location)
    {
        return ChunkLocation == location;
    }
}


// 为List对象创建一个封装类，以便能够将其转换为JSON
[Serializable]
public class Wrapper<T>
{
    public List<T> Items;

    public Wrapper(List<T> items)
    {
        Items = items;
    }
}
