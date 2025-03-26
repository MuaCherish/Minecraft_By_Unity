using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物品数据库，存储所有 ItemBase 物品
/// </summary>
[CreateAssetMenu(fileName = "New Item_Database", menuName = "Items/Item_Database")]
public class Item_Database : ScriptableObject
{
    public List<Item_Base> items = new List<Item_Base>();

    /// <summary>
    /// 获取Item元素
    /// </summary>
    /// <returns></returns>
    public Item_Base GetItem(byte _blockType)
    {
        return items[_blockType];
    }

    /// <summary>
    /// 获取容量
    /// </summary>
    /// <returns></returns>
    public int GetItemCount()
    {
        return items.Count;
    }

    /// <summary>
    /// 是否是工具
    /// </summary>
    /// <returns></returns>
    public bool ItemisTool(int index)
    {
        if (items[index].BlockClassfy == BlockClassfy.工具)
            return true;

        return false;
    }

}
