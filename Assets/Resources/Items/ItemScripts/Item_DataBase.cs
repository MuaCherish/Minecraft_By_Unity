using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��Ʒ���ݿ⣬�洢���� ItemBase ��Ʒ
/// </summary>
[CreateAssetMenu(fileName = "New Item_Database", menuName = "Items/Item_Database")]
public class Item_Database : ScriptableObject
{
    public List<Item_Base> items = new List<Item_Base>();

    /// <summary>
    /// �Ƿ��ǹ���
    /// </summary>
    /// <returns></returns>
    public bool ItemisTool(int index)
    {
        if (items[index].BlockClassfy == BlockClassfy.����)
            return true;

        return false;
    }

}
