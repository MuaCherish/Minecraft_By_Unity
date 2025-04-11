using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 并不是静态而是需要Load的数据
/// </summary>
public class MC_Runtime_StaticData
{
    private static MC_Runtime_StaticData _instance;
    public static MC_Runtime_StaticData Instance => _instance ??= new MC_Runtime_StaticData();

    public Item_Database ItemData { get; private set; }
    public BiomeDataSO BiomeData { get; private set; }
    public TerrainVisualAssets TerrainMatData { get; private set; }

    private MC_Runtime_StaticData()
    {
        LoadAll();
    }

    private void LoadAll()
    {
        ItemData = Resources.Load<Item_Database>("Items\\_Item_DataBase");
        BiomeData = Resources.Load<BiomeDataSO>("Chunk\\BiomeData");
        TerrainMatData = Resources.Load<TerrainVisualAssets>("Chunk\\TerrainMatData");
    }
}

