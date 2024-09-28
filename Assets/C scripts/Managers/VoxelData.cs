using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
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
    16:Cactus
    17:TNT
    18:工作台WorkTable
    19:Pumpkin
    20:玻璃Glass
    21:青晶石Blue_Crystal
    22:钻石Diamond
    23:铁矿Iron
    24:金矿Gold
    25:竹子Bamboo
    26:木板WoodenPlanks
    27:Fluor
    28:灌木Bush
    29:雪块Snow
    30:雪碎片SnowPower
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
    public static readonly Byte Torch = 15;
    public static readonly Byte Cactus = 16; //仙人掌
    public static readonly Byte TNT = 17;
    public static readonly Byte WorkTable = 18;
    public static readonly Byte Pumpkin = 19; //南瓜
    public static readonly Byte Glass = 20;
    public static readonly Byte Blue_Crystal = 21;
    public static readonly Byte Diamond = 22;
    public static readonly Byte Iron = 23;
    public static readonly Byte Gold = 24;
    public static readonly Byte Bamboo = 25;
    public static readonly Byte WoodenPlanks = 26;
    public static readonly Byte Fluor = 27; //萤石
    public static readonly Byte Bush = 28;
    public static readonly Byte Snow = 29;
    public static readonly Byte SnowPower = 30;
    public static readonly Byte Mycelium = 31;   //菌丝体
    public static readonly Byte Cobblestone = 32; //圆石
    public static readonly Byte Door_Up = 33;
    public static readonly Byte Door_Down = 34; 
    public static readonly Byte HalfBrick_Wood = 35;
    public static readonly Byte Mushroom_red = 36;
    public static readonly Byte Mushroom_brown = 37;
    public static readonly Byte Redbrick = 38;
    public static readonly Byte Furnace = 39;
    public static readonly Byte JukeBox = 40; //唱片机
    public static readonly Byte Wool_Green = 41;
    public static readonly Byte Wool_White = 42;
    public static readonly Byte Wool_Red = 43;
    public static readonly Byte Wool_Pink = 44;
    public static readonly Byte Chest = 45;
    public static readonly Byte Gravel = 46; //沙砾
    public static readonly Byte Glass_Plane = 47;
    public static readonly Byte Tool_Sword = 48;
    public static readonly Byte Tool_Pork = 49;
    public static readonly Byte Tool_MusicDiscs = 50;
    public static readonly Byte Tool_Flint = 51; //打火石
    public static readonly Byte Tool_Pickaxe = 52; //稿子
    public static readonly Byte Tool_Egg = 53;
    public static readonly Byte Tool_ReadMap = 54;
    public static readonly Byte Tool_SnowBall = 55;
    public static readonly Byte Tool_BoneMeal = 56; //骨粉
    public static readonly Byte Sapling = 57; //树苗
    public static readonly Byte Tool_Book = 58;
    //public static readonly Byte Tool_Pork = 59;
    //public static readonly Byte Tool_Pork = 60;
    //public static readonly Byte Tool_Pork = 61;
    //public static readonly Byte Tool_Pork = 62;
    //public static readonly Byte Tool_Pork = 63;
    //public static readonly Byte Tool_Pork = 64;
    //public static readonly Byte Tool_Pork = 65;


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
    public static readonly int explore = 19;



    //canvas宏定义
    public static readonly int ui菜单 = 0;
    public static readonly int ui多人游戏 = 1;
    public static readonly int ui初始化_选择存档 = 2;
    public static readonly int ui初始化_新建世界 = 3;
    public static readonly int ui加载世界 = 4;
    public static readonly int ui选项 = 5;
    public static readonly int ui选项细节 = 6;
    public static readonly int ui调试 = 7;
    public static readonly int ui玩家 = 8;
    public static readonly int ui游戏中暂停 = 9;
    public static readonly int ui死亡 = 10;
    public static readonly int ui正在保存中 = 11;
    public static readonly int ui项目展示内容 = 12;

    //ui选项细节宏定义
    public static readonly int 视频设置 = 0;
    public static readonly int 音乐与声音 = 1;
    public static readonly int 昼夜模式 = 2;
    public static readonly int 玩家设置 = 3;
    public static readonly int 辅助设置 = 4;




    //Select
    public static readonly float[] SelectLocation_x = new float[9]
    {

        46f,126f,206f,286f,366f,445f,526f,606f,686f,

    };



    //chunk大小
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;

    public static readonly int TextureAtlasSizeInBlocks = 16;

    public static float NormalizedBlockTextureSize
    {

        get { return 1f / (float)TextureAtlasSizeInBlocks; }

    }


    //BlockFacing
    public static readonly int[,] BlockOriented = new int[6, 6]
    {
        //只有四周四个矩阵是有用的
        {0, 1, 2, 3, 4, 5}, //front
        {1, 0, 2, 3, 5, 4}, //back
        {0, 1, 2, 3, 4, 5},
        {0, 1, 2, 3, 4, 5},
        {4, 5, 2, 3, 1, 0}, //right
        {5, 4, 2, 3, 0, 1}, //left
		


	};


    #region 顶点数组

    //顶点数组 - Block
    public static readonly Vector3[] voxelVerts = new Vector3[8] 
    {

        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
        //new Vector3(-0.01f, -0.01f, -0.01f),
        //new Vector3(1.01f, -0.01f, -0.01f),
        //new Vector3(1.01f, 1.01f, -0.01f),
        //new Vector3(-0.01f, 1.01f, -0.01f),
        //new Vector3(-0.01f, -0.01f, 1.01f),
        //new Vector3(1.01f, -0.01f, 1.01f),
        //new Vector3(1.01f, 1.01f, 1.01f),
        //new Vector3(-0.01f, 1.01f, 1.01f),

    };

    //顶点数组 - Water
    public static readonly Vector3[] voxelVerts_Water = new Vector3[8] 
    {

        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.8f, 0.0f),
        new Vector3(0.0f, 0.8f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.8f, 1.0f),
        new Vector3(0.0f, 0.8f, 1.0f),

    };

    //顶点数组 - SnowPower
    public static readonly Vector3[] voxelVerts_SnowPower = new Vector3[8]
    {

        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.2f, 0.0f),
        new Vector3(0.0f, 0.2f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.2f, 1.0f),
        new Vector3(0.0f, 0.2f, 1.0f),

    };


    //顶点数组 - Door
    public static readonly Vector3[] voxelVerts_Door = new Vector3[8]
    {

        new Vector3(0.0f, 0.0f, 0.8125f),
        new Vector3(1.0f, 0.0f, 0.8125f),
        new Vector3(1.0f, 1.0f, 0.8125f),
        new Vector3(0.0f, 1.0f, 0.8125f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),

    };

    //顶点数组 - HalfBrick
    public static readonly Vector3[] voxelVerts_HalfBrick = new Vector3[8]
    {

        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.5f, 0.0f),
        new Vector3(0.0f, 0.5f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.5f, 1.0f),
        new Vector3(0.0f, 0.5f, 1.0f),

    };

    //顶点数组 - Torch
    public static readonly Vector3[] voxelVerts_Torch = new Vector3[8]
    {

        new Vector3(0.4375f, 0.0f, 0.4375f),
        new Vector3(0.5625f, 0.0f, 0.4375f),
        new Vector3(0.5625f, 1.0f, 0.4375f),
        new Vector3(0.4375f, 1.0f, 0.4375f),
        new Vector3(0.4375f, 0.0f, 0.5625f),
        new Vector3(0.5625f, 0.0f, 0.5625f),
        new Vector3(0.5625f, 1.0f, 0.5625f),
        new Vector3(0.4375f, 1.0f, 0.5625f),

    };


    #endregion

    //6个指定方向
    public static readonly Vector3[] faceChecks = new Vector3[6]
    {

        new Vector3(0.0f, 0.0f, -1.0f), //Back
        new Vector3(0.0f, 0.0f, 1.0f),  //Front
        new Vector3(0.0f, 1.0f, 0.0f),  //Up
        new Vector3(0.0f, -1.0f, 0.0f), //Down
        new Vector3(-1.0f, 0.0f, 0.0f), //Left
        new Vector3(1.0f, 0.0f, 0.0f),  //Right

    };

    //4个指定方向
    public static readonly Vector3[] faceChecks_Surround = new Vector3[4]
    {
        //顺时针遍历，因为朝着XZ正方向判定

        new Vector3(0.0f, 0.0f, 1.0f),  //Front
        new Vector3(1.0f, 0.0f, 0.0f),  //Right

        new Vector3(0.0f, 0.0f, -1.0f), //Back
        new Vector3(-1.0f, 0.0f, 0.0f), //Left
        

    };


    //水流动态流动方向
    public static readonly Vector3[] faceChecks_WaterFlow = new Vector3[5]
    {

        new Vector3(0.0f, -1.0f, 0.0f), //Down
        new Vector3(0.0f, 0.0f, 1.0f),  //Front
        new Vector3(0.0f, 0.0f, -1.0f), //Back
        new Vector3(-1.0f, 0.0f, 0.0f), //Left
        new Vector3(1.0f, 0.0f, 0.0f),  //Right

    };

    #region 绘制序列

    //绘制序列 - Block
    public static readonly int[,] voxelTris = new int[6, 4] 
    {

        {0, 3, 1, 2}, // Back Face
		{5, 6, 4, 7}, // Front Face
		{3, 7, 2, 6}, // Top Face
		{1, 5, 0, 4}, // Bottom Face
		{4, 7, 0, 3}, // Left Face
		{1, 2, 5, 6}  // Right Face

	};

    //绘制序列 - Bush
    public static readonly int[,] voxelTris_Bush = new int[4, 4] 
    {

        {0, 3, 6, 5},
        {5, 6, 3, 0},
        {4, 7, 2, 1},
        {1, 2, 7, 4},

    };

    

    #endregion

    //UV数组
    public static readonly Vector2[] voxelUvs = new Vector2[4] 
    {

        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)

    };
}
