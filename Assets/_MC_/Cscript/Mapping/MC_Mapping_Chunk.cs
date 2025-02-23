using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region Terrain

public static class TerrainData
{
    //chunk��С
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;

    /*Ⱥϵϵͳ
        *ƽԭ��0
        *��ԭ��1
        *ɳĮ��2
        *����3
        */
    public static readonly int Biome_Plain = 0;
    public static readonly int Biome_Plateau = 1;
    public static readonly int Biome_Dessert = 2;
    public static readonly int Biome_Marsh = 3;
    public static readonly int Biome_Forest = 4;
    public static readonly int Biome_Default = 5;
    public static readonly int Biome_SuperPlain = 6;


}


//��������ṹ��
public class VoxelStruct
{
    public byte voxelType = VoxelData.Air;
    public int blockOriented = 0;

    //�����ɵ���������
    public bool up = true;
}


//Ⱥϵϵͳ
[System.Serializable]
public class BiomeNoiseSystem
{
    public string BiomeName;
    public Color BiomeColor;
    public Vector2 HighDomain;
    public Vector3 Noise_Scale_123;
    public Vector3 Noise_Rank_123;
}


//���ʷֲ������ϵͳ
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
    public int ������ľ��������Forest_treecount;
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


#endregion


#region Voxel

/// <summary>
/// old
/// </summary>
//�ṹ��BlockType
//�洢��������+���Ӧ��UV
[System.Serializable]
public class BlockType
{
    [Header("��������")]
    public string blockName;
    public BlockClassfy BlockClassfy;

    [Header("�������")]
    public float DestroyTime;
    public bool isSolid;        //�Ƿ���赲���
    public bool isTransparent;  //�ܱ߷����Ƿ����޳�
    public bool canBeChoose;    //�Ƿ�ɱ��������鲶׽��
    public bool candropBlock;   //�Ƿ���䷽��
    public bool IsOriented;     //�Ƿ������ҳ���
    public bool isinteractable; //�Ƿ�ɱ��Ҽ�����
    public bool is2d;           //����������ʾ
    public bool CanBeCover;        //�Ƿ�ᱻ����
    public bool NotSuspended;    //�������շ���

    [Header("���߲���")]
    public bool isTool;         //���ֹ�����
    public bool canBreakBlockWithMouse1 = true;  //������ƻ����� 
    public bool isNeedRotation; //true�����һ������ת


    [Header("�Զ�����ײ")]
    public bool isDIYCollision;
    //������˵���Ƿ������ڼ�ѹ����ֵ
    //����Y��˵��(0.5f,0,0f)������Y������������ڼ�ѹ0.5f��Y������������ڼ�ѹ0.0f����̨�׵���ײ����
    public CollosionRange CollosionRange;

    [Header("Sprits")]
    public Sprite icon; //��Ʒ��ͼ��
    public Sprite front_sprite; //������
    public Sprite sprite;  //����
    public Sprite top_sprit; //������
    public Sprite buttom_sprit; //������


    [Header("����")]
    public AudioClip[] walk_clips = new AudioClip[2];
    public AudioClip broking_clip;
    public AudioClip broken_clip;


    [Header("����")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    public DrawMode DrawMode;

    [Header("�������ж�(��ǰ��������)")]
    public bool GenerateTwoFaceWithAir;    //��������������˫�����
    public List<FaceCheckMode> OtherFaceCheck;



    //��ͼ�е��������
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
/// ����Block���ͼ��ϣ�Mesh����
/// </summary>
public static class VoxelData
{

    #region Block �궨��

    //����궨��
    /*
	0������BedRock
	1��ʯͷStone
	2���ݵ�Grass
	3������Soil
	4������Air
    5��ɳ��Sand
    6��ľͷWood
    7����ҶLeaves
    8��ˮWater
    9��ú̿Coal
    10:����ľBirchWood
    11:��ɫ��BlueFlower
    12:��ɫ��1��WhiteFlower_1
    13:��ɫ��2��WhiteFlower_2
    14:��ɫ��YellowFlower
    15:���Candle
    16:Cactus
    17:TNT
    18:����̨WorkTable
    19:Pumpkin
    20:����Glass
    21:�ྦྷʯBlue_Crystal
    22:��ʯDiamond
    23:����Iron
    24:���Gold
    25:����Bamboo
    26:ľ��WoodenPlanks
    27:Fluor
    28:��ľBush
    29:ѩ��Snow
    30:ѩ��ƬSnowPower
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
    public static readonly Byte Cactus = 16; //������
    public static readonly Byte TNT = 17;
    public static readonly Byte WorkTable = 18;
    public static readonly Byte Pumpkin = 19; //�Ϲ�
    public static readonly Byte Glass = 20;
    public static readonly Byte Blue_Crystal = 21;
    public static readonly Byte Diamond = 22;
    public static readonly Byte Iron = 23;
    public static readonly Byte Gold = 24;
    public static readonly Byte Bamboo = 25;
    public static readonly Byte WoodenPlanks = 26;
    public static readonly Byte Fluor = 27; //өʯ
    public static readonly Byte Bush = 28;
    public static readonly Byte Snow = 29;
    public static readonly Byte SnowPower = 30;
    public static readonly Byte Mycelium = 31;   //��˿��
    public static readonly Byte Cobblestone = 32; //Բʯ
    public static readonly Byte Door_Up = 33;
    public static readonly Byte Door_Down = 34;
    public static readonly Byte HalfBrick_Wood = 35;
    public static readonly Byte Mushroom_red = 36;
    public static readonly Byte Mushroom_brown = 37;
    public static readonly Byte Redbrick = 38;
    public static readonly Byte Furnace = 39; // ��¯
    public static readonly Byte JukeBox = 40; //��Ƭ��
    public static readonly Byte Wool_Green = 41;
    public static readonly Byte Wool_White = 42;
    public static readonly Byte Wool_Red = 43;
    public static readonly Byte Wool_Pink = 44;
    public static readonly Byte Chest = 45; //���ӱ���
    public static readonly Byte Gravel = 46; //ɳ��
    public static readonly Byte Glass_Plane = 47;
    public static readonly Byte Tool_Sword = 48;
    public static readonly Byte Tool_Pork = 49;
    public static readonly Byte Tool_MusicDiscs = 50;
    public static readonly Byte Tool_Flint = 51; //���ʯ
    public static readonly Byte Tool_Pickaxe = 52; //����
    public static readonly Byte Tool_Egg = 53;
    public static readonly Byte Tool_ReadMap = 54;
    public static readonly Byte Tool_SnowBall = 55;
    public static readonly Byte Tool_BoneMeal = 56; //�Ƿ�
    public static readonly Byte Sapling = 57; //����
    public static readonly Byte Tool_Book = 58;
    public static readonly Byte Apple = 59;
    public static readonly Byte Tool_Shovel = 60;  //����
    public static readonly Byte Tool_Bow = 61;  //��
    public static readonly Byte Tool_Arrow = 62; //��
    public static readonly Byte Fish = 63;
    public static readonly Byte Rotten_Flesh = 64; //����
    public static readonly Byte Slimeball = 65;  //ճҺ��


    #endregion

    #region Mesh �궨��

    public static readonly int TextureAtlasSizeInBlocks = 16;

    public static float NormalizedBlockTextureSize
    {

        get { return 1f / (float)TextureAtlasSizeInBlocks; }

    }


    //BlockFacing
    public static readonly int[,] BlockOriented = new int[6, 6]
    {
        //ֻ�������ĸ����������õ�
        {0, 1, 2, 3, 4, 5}, //front
        {1, 0, 2, 3, 5, 4}, //back
        {0, 1, 2, 3, 4, 5},
        {0, 1, 2, 3, 4, 5},
        {4, 5, 2, 3, 1, 0}, //right
        {5, 4, 2, 3, 0, 1}, //left
		


	};


    #region ��������

    //�������� - Block
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

    //�������� - Water
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

    //�������� - SnowPower
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


    //�������� - Door
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

    //�������� - HalfBrick
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

    //�������� - Torch
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

    //6��ָ������
    public static readonly Vector3[] faceChecks = new Vector3[6]
    {

        new Vector3(0.0f, 0.0f, -1.0f), //Back
        new Vector3(0.0f, 0.0f, 1.0f),  //Front
        new Vector3(0.0f, 1.0f, 0.0f),  //Up
        new Vector3(0.0f, -1.0f, 0.0f), //Down
        new Vector3(-1.0f, 0.0f, 0.0f), //Left
        new Vector3(1.0f, 0.0f, 0.0f),  //Right

    };

    //4��ָ������
    public static readonly Vector3[] faceChecks_Surround = new Vector3[4]
    {
        //˳ʱ���������Ϊ����XZ�������ж�

        new Vector3(0.0f, 0.0f, 1.0f),  //Front
        new Vector3(1.0f, 0.0f, 0.0f),  //Right

        new Vector3(0.0f, 0.0f, -1.0f), //Back
        new Vector3(-1.0f, 0.0f, 0.0f), //Left
        

    };


    //ˮ����̬��������
    public static readonly Vector3[] faceChecks_WaterFlow = new Vector3[5]
    {

        new Vector3(0.0f, -1.0f, 0.0f), //Down
        new Vector3(0.0f, 0.0f, 1.0f),  //Front
        new Vector3(0.0f, 0.0f, -1.0f), //Back
        new Vector3(-1.0f, 0.0f, 0.0f), //Left
        new Vector3(1.0f, 0.0f, 0.0f),  //Right

    };

    #region ��������

    //�������� - Block
    public static readonly int[,] voxelTris = new int[6, 4]
    {

        {0, 3, 1, 2}, // Back Face
		{5, 6, 4, 7}, // Front Face
		{3, 7, 2, 6}, // Top Face
		{1, 5, 0, 4}, // Bottom Face
		{4, 7, 0, 3}, // Left Face
		{1, 2, 5, 6}  // Right Face

	};

    //�������� - Bush
    public static readonly int[,] voxelTris_Bush = new int[4, 4]
    {

        {0, 3, 6, 5},
        {5, 6, 3, 0},
        {4, 7, 2, 1},
        {1, 2, 7, 4},

    };



    #endregion

    //UV����
    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {

        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)

    };


    #endregion

}


//���� �ţ�{0, isSolid false} ����������Solid��������
// 0 1  2   3  4  5
//�� ǰ �� �� �� ��
[Serializable]
public class FaceCheckMode
{
    public int FaceDirect;  //���Direct���ڱ��ط��򣬱���0ָ�����Լ�����ĺ󷽣���Ϊ����Ҫ�˼����������ת
    public FaceCheck_Enum checktype;
    public byte appointType;
    public DrawMode appointDrawmode;
    public bool isCreateFace;
}


//������ײ��
[Serializable]
public class CollosionRange
{
    public Vector2 xRange = new Vector3(0, 1f);
    public Vector2 yRange = new Vector3(0, 1f);
    public Vector2 zRange = new Vector3(0, 1f);
}

#endregion

