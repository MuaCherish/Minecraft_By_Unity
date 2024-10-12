using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class TerrainData
{
    //chunk大小
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;

    /*群系系统
        *平原：0
        *高原：1
        *沙漠：2
        *沼泽：3
        */
    public static readonly int Biome_Plain = 0;
    public static readonly int Biome_Plateau = 1;
    public static readonly int Biome_Dessert = 2;
    public static readonly int Biome_Marsh = 3;
    public static readonly int Biome_Forest = 4;
    public static readonly int Biome_Default = 5;
    public static readonly int Biome_SuperPlain = 6;


}


//方块种类结构体
public class VoxelStruct
{
    public byte voxelType = VoxelData.Air;
    public int blockOriented = 0;

    //面生成的六个方向
    public bool up = true;
}


//群系系统
[System.Serializable]
public class BiomeNoiseSystem
{
    public string BiomeName;
    public Color BiomeColor;
    public Vector2 HighDomain;
    public Vector3 Noise_Scale_123;
    public Vector3 Noise_Rank_123;
}


//地质分层与概率系统
//TerrainLayerProbabilitySystem
[System.Serializable]
public class TerrainLayerProbabilitySystem
{
    //seed
    public bool isRandomSeed = true;
    public int Seed = 0;

    //level
    public float sea_level = 60;
    public float Snow_Level = 100;

    //tree
    public int Normal_treecount;
    public int 密林树木采样次数Forest_treecount;
    public int TreeHigh_min = 5;
    public int TreeHigh_max = 7;

    //random
    public float Random_Bush;
    public float Random_Bamboo;
    public float Random_BlueFlower;
    public float Random_WhiteFlower1;
    public float Random_WhiteFlower2;
    public float Random_YellowFlower;

    //coal
    public float Random_Coal;
    public float Random_Iron;
    public float Random_Gold;
    public float Random_Blue_Crystal;
    public float Random_Diamond;
}
