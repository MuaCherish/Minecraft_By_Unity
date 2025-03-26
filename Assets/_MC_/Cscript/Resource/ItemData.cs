using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单例类用来存储 ItemDataBase
/// </summary>
public static class MC_Resource_ItemData
{
    private static Item_Database itemDatabase;
    private static readonly string DataBasePath = "ItemDataBase\\_Item_DataBase"; // 资源路径 (Resources 文件夹内)

    /// <summary>
    /// 获取 ItemDataBase 实例
    /// </summary>
    public static Item_Database Instance
    {
        get
        {
            if (itemDatabase == null)
            {
                LoadDatabase();
            }
            return itemDatabase;
        }
    }

    /// <summary>
    /// 加载物品数据库
    /// </summary>
    private static void LoadDatabase()
    {
        itemDatabase = Resources.Load<Item_Database>(DataBasePath);

        if (itemDatabase == null)
            Debug.LogError("Item_Database not found at path: " + DataBasePath);
    }
}

