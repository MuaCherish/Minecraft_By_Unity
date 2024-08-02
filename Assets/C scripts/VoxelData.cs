using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    //地形参数
    /*
     * 平原：soil[10,30],sealevel[17],tree[1]
     * 丘陵：soil[20,50],sealevel[30],tree[5]
    */

    //特殊参数
    /*
     * 253：未找到Chunk
     * 254：固体
     * 255：射线未打中
    */
    public static readonly Byte notChunk = 25;
    public static readonly Byte Solid = 254;
    public static readonly Byte notHit = 255;

    //走路参数
    /*
     * walkSpeed：走路播放延迟
     * sprintSpeed：冲刺播放延迟
    */
    public static readonly float walkSpeed = 0.5f;
    public static readonly float sprintSpeed = 0.3f;

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
    10:白桦木BirchWood
    11:蓝色花BlueFlower
    12:白色花1号WhiteFlower_1
    13:白色花2号WhiteFlower_2
    14:黄色花YellowFlower
    15:火把Candle
    16:仙人掌Cactus
    17:TNT
    18:工作台WorkTable
    19:南瓜Pumpkin
    20:玻璃Glass
    21:青晶石Blue_Crystal
    22:钻石Diamond
    23:铁矿Iron
    24:金矿Gold
    25:竹子Bamboo
    26:木板WoodenPlanks
    27:萤石Fluor
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
    public static readonly Byte BirchWood = 10;
    public static readonly Byte BlueFlower = 11;
    public static readonly Byte WhiteFlower_1 = 12;
    public static readonly Byte WhiteFlower_2 = 13;
    public static readonly Byte YellowFlower = 14;
    public static readonly Byte Candle = 15;
    public static readonly Byte Cactus = 16;
    public static readonly Byte TNT = 17;
    public static readonly Byte WorkTable = 18;
    public static readonly Byte Pumpkin = 19;
    public static readonly Byte Glass = 20;
    public static readonly Byte Blue_Crystal = 21;
    public static readonly Byte Diamond = 22;
    public static readonly Byte Iron = 23;
    public static readonly Byte Gold = 24;
    public static readonly Byte Bamboo = 25;
    public static readonly Byte WoodenPlanks = 26;
    public static readonly Byte Fluor = 27;
    public static readonly Byte Bush = 28;


    //音乐宏定义
    //宏定义
    /*
     * 0.  click
     * 1.  bgm_menu
     * 2.  bgm_1
     * 3.  bgm_2
     * 16. bgm_3
     * 4.  dancegirl
     * 
     * 5.  moving_normal
     * 6.  moving_water
     * 
     * 7.  dive
     * 8.  fall_water
     * 9.  fall_high
     * 
     * 10. place
     * 
     * 11. broke_leaves
     * 12. broke_sand
     * 13. broke_soil
     * 14. broke_wood
     * 15. broke_stone
     * 
     * 17. absorb_1
     * 18. absorb_2
    */
    public static readonly int click = 0;
    public static readonly int bgm_menu = 1;
    public static readonly int bgm_1 = 2;
    public static readonly int bgm_2 = 3;
    public static readonly int dancegirl = 4;
    public static readonly int moving_normal = 5;
    public static readonly int moving_water = 6;
    public static readonly int dive = 7;
    public static readonly int fall_water = 8;
    public static readonly int fall_high = 9;
    public static readonly int place_normal = 10;
    public static readonly int broke_leaves = 11;
    public static readonly int broke_sand = 12;
    public static readonly int broke_soil = 13;
    public static readonly int broke_wood = 14;
    public static readonly int broke_stone = 15;
    public static readonly int bgm_3 = 16;
    public static readonly int absorb_1 = 17;
    public static readonly int absorb_2 = 18;


    //Select
    public static readonly float[] SelectLocation_x = new float[9]
    {
        46f,126f,206f,286f,366f,445f,526f,606f,686f,
    };



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


    //Block绘制序列
    public static readonly int[,] voxelTris = new int[6, 4] {

        {0, 3, 1, 2}, // Back Face
		{5, 6, 4, 7}, // Front Face
		{3, 7, 2, 6}, // Top Face
		{1, 5, 0, 4}, // Bottom Face
		{4, 7, 0, 3}, // Left Face
		{1, 2, 5, 6}  // Right Face

	};

    //绘制序列 - Bush
    public static readonly int[,] voxelTris_Bush = new int[4, 4] {

        {0, 3, 6, 5},
        {5, 6, 3, 0},
        {4, 7, 2, 1},
        {1, 2, 7, 4},

    };

    //UV数组
    public static readonly Vector2[] voxelUvs = new Vector2[4] {

        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)

    };
}
