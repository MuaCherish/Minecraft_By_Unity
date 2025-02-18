using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// old
/// </summary>
//结构体BlockType
//存储方块种类+面对应的UV
[System.Serializable]
public class BlockType
{
    [Header("基本参数")]
    public string blockName;
    public BlockClassfy BlockClassfy;

    [Header("方块参数")]
    public float DestroyTime;
    public bool isSolid;        //是否会阻挡玩家
    public bool isTransparent;  //周边方块是否面剔除
    public bool canBeChoose;    //是否可被高亮方块捕捉到
    public bool candropBlock;   //是否掉落方块
    public bool IsOriented;     //是否跟随玩家朝向
    public bool isinteractable; //是否可被右键触发
    public bool is2d;           //用来区分显示
    public bool CanBeCover;        //是否会被覆盖
    public bool NotSuspended;    //不可悬空放置

    [Header("工具参数")]
    public bool isTool;         //区分功能性
    public bool canBreakBlockWithMouse1 = true;  //左键可破坏方块 
    public bool isNeedRotation; //true后会做一定的旋转


    [Header("自定义碰撞")]
    public bool isDIYCollision;
    //抽象来说就是方块向内挤压的数值
    //对于Y来说，(0.5f,0,0f)，就是Y正方向的面向内挤压0.5f，Y负方向的面向内挤压0.0f，即台阶的碰撞参数
    public CollosionRange CollosionRange;

    [Header("Sprits")]
    public Sprite icon; //物品栏图标
    public Sprite front_sprite; //掉落物
    public Sprite sprite;  //侧面
    public Sprite top_sprit; //掉落物
    public Sprite buttom_sprit; //掉落物


    [Header("音乐")]
    public AudioClip[] walk_clips = new AudioClip[2];
    public AudioClip broking_clip;
    public AudioClip broken_clip;


    [Header("绘制")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    public DrawMode DrawMode;

    [Header("面生成判断(后前上下左右)")]
    public bool GenerateTwoFaceWithAir;    //如果朝向空气，则双面绘制
    public List<FaceCheckMode> OtherFaceCheck;



    //贴图中的面的坐标
    public int GetTextureID(int faceIndex)
    {

        switch (faceIndex)
        {

            case 0:
                return backFaceTexture;

            case 1:
                return frontFaceTexture;

            case 2:
                return topFaceTexture;

            case 3:
                return bottomFaceTexture;

            case 4:
                return leftFaceTexture;

            case 5:
                return rightFaceTexture;

            default:
                Debug.Log($"Error in GetTextureID; invalid face index {faceIndex}");
                return 0;


        }

    }


}


/// <summary>
/// 包含Block类型集合，Mesh数据
/// </summary>
public static class VoxelData
{

    #region Block 宏定义

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
    public static readonly Byte Furnace = 39; // 熔炉
    public static readonly Byte JukeBox = 40; //唱片机
    public static readonly Byte Wool_Green = 41;
    public static readonly Byte Wool_White = 42;
    public static readonly Byte Wool_Red = 43;
    public static readonly Byte Wool_Pink = 44;
    public static readonly Byte Chest = 45; //箱子宝箱
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
    public static readonly Byte Apple = 59;
    public static readonly Byte Tool_Shovel = 60;  //铲子
    public static readonly Byte Tool_Bow = 61;  //弓
    public static readonly Byte Tool_Arrow = 62; //箭
    public static readonly Byte Fish = 63;
    public static readonly Byte Rotten_Flesh = 64; //腐肉
    public static readonly Byte Slimeball = 65;  //粘液球


    #endregion

    #region Mesh 宏定义

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


    #endregion

}


//比如 门：{0, isSolid false} 代表后方如果是Solid则不生成面
// 0 1  2   3  4  5
//后 前 上 下 左 右
[Serializable]
public class FaceCheckMode
{
    public int FaceDirect;  //这个Direct属于本地方向，比如0指的是自己朝向的后方，因为后面要顾及到物体的旋转
    public FaceCheck_Enum checktype;
    public byte appointType;
    public DrawMode appointDrawmode;
    public bool isCreateFace;
}


//方块碰撞类
[Serializable]
public class CollosionRange
{
    public Vector2 xRange = new Vector3(0, 1f);
    public Vector2 yRange = new Vector3(0, 1f);
    public Vector2 zRange = new Vector3(0, 1f);
}
