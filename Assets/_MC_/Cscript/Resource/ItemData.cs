using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �����������洢 ItemDataBase
/// </summary>
public static class MC_Resource_ItemData
{
    private static Item_Database itemDatabase;
    private static readonly string DataBasePath = "ItemDataBase\\_Item_DataBase"; // ��Դ·�� (Resources �ļ�����)

    /// <summary>
    /// ��ȡ ItemDataBase ʵ��
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
    /// ������Ʒ���ݿ�
    /// </summary>
    private static void LoadDatabase()
    {
        itemDatabase = Resources.Load<Item_Database>(DataBasePath);

        if (itemDatabase == null)
            Debug.LogError("Item_Database not found at path: " + DataBasePath);
    }
}

