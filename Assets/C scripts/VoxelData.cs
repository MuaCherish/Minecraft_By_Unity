using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    //方块宏定义
    /*
	0：基岩BedRock
	1：石头Stone
	2：草地Grass
	3：泥土Soil
	4：空气Air
    5：沙子Sand
    6：木头Wood
    7：树叶Leaves
    8：水Water
    9：煤炭Coal
	*/
    public static readonly Byte BedRock = 0;
    public static readonly Byte Stone = 1;
    public static readonly Byte Grass = 2;
    public static readonly Byte Soil = 3;
    public static readonly Byte Air = 4;
    public static readonly Byte Sand = 5;
    public static readonly Byte Wood = 6;
    public static readonly Byte Leaves = 7;
    public static readonly Byte Water = 8;
    public static readonly Byte Coal = 9;


    //chunk大小
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 64;

    public static readonly int TextureAtlasSizeInBlocks = 8;

    public static float NormalizedBlockTextureSize
    {

        get { return 1f / (float)TextureAtlasSizeInBlocks; }

    }


    //顶点数组
    public static readonly Vector3[] voxelVerts = new Vector3[8] {

        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),

    };


    //指定方向
    public static readonly Vector3[] faceChecks = new Vector3[6]
    {

        new Vector3(0.0f, 0.0f, -1.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, -1.0f, 0.0f),
        new Vector3(-1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),

    };


    //绘制序列
    public static readonly int[,] voxelTris = new int[6, 4] {

        {0, 3, 1, 2}, // Back Face
		{5, 6, 4, 7}, // Front Face
		{3, 7, 2, 6}, // Top Face
		{1, 5, 0, 4}, // Bottom Face
		{4, 7, 0, 3}, // Left Face
		{1, 2, 5, 6}  // Right Face

	};

    //UV数组
    public static readonly Vector2[] voxelUvs = new Vector2[4] {

        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)

    };
}
