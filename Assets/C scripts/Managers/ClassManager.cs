
//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using static UnityEngine.GraphicsBuffer;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;
using System;
//using static UnityEditor.PlayerSettings;
//using System.Diagnostics;



//���� �ţ�{0, isSolid false} ����������Solid��������
// 0 1  2   3  4  5
//�� ǰ �� �� �� ��
[System.Serializable]
public class FaceCheckMode
{
    public int FaceDirect;  //���Direct���ڱ��ط��򣬱���0ָ�����Լ�����ĺ󷽣���Ϊ����Ҫ�˼����������ת
    public FaceCheck_Enum checktype;
    public byte appointType;
    public DrawMode appointDrawmode;
    public bool isCreateFace;
}



//�ṹ��BlockType
//�洢��������+���Ӧ��UV
[System.Serializable]
public class BlockType
{
    [Header("�������")]
    public string blockName;
    public float DestroyTime;
    public bool isSolid;        //�Ƿ���赲���
    public bool isTransparent;  //�ܱ߷����Ƿ����޳�
    public bool canBeChoose;    //�Ƿ�ɱ��������鲶׽��
    public bool candropBlock;   //�Ƿ���䷽��
    public bool IsOriented;     //�Ƿ������ҳ���
    public bool isinteractable; //�Ƿ�ɱ��Ҽ�����
    public bool is2d;           //����������ʾ

    [Header("���߲���")]
    public bool isTool;         //���ֹ�����
    public bool isNeedRotation; //true�����һ������ת


    [Header("�Զ�����ײ")]
    public bool isDIYCollision;
    //������˵���Ƿ������ڼ�ѹ����ֵ
    //����Y��˵��(0.5f,0,0f)������Y������������ڼ�ѹ0.5f��Y������������ڼ�ѹ0.0f����̨�׵���ײ����
    public CollosionRange CollosionRange;

    [Header("Sprits")]
    public Sprite icon; //��Ʒ��ͼ��
    public Sprite sprite; //������
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


//������
//[System.Serializable]
//public class ToolType
//{
//    public string name;
//    public Sprite sprite;
//}

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



// һ��������WorldSetting
[Serializable]
public class WorldSetting
{
    public String date = "0000";//�浵��������
    public String name = "�µ�����";
    public int seed = 0;
    public GameMode gameMode = GameMode.Survival;
    public int worldtype = VoxelData.Biome_Default;
    public Vector3 playerposition;   // ������ҵ�����
    public Quaternion playerrotation; // ������ҵ���ת

    public WorldSetting(int _seed)
    {
        this.seed = _seed;
    }
}


//[System.Serializable]
//public class EditStruct
//{
//    public Vector3 ChunkLocation;
//    public Vector3 RelativeLocation;
//    public byte EditType;

//    public EditStruct(Vector3 _ChunkLocation, Vector3 _RelativeLocation, byte _EditType)
//    {
//        ChunkLocation = _ChunkLocation;
//        RelativeLocation = _RelativeLocation;
//        EditType = _EditType;
//    }

//}


//����޸ĵ����ݻ���
[System.Serializable]
public class EditStruct
{
    public Vector3 editPos;
    public byte targetType;


    public EditStruct(Vector3 _editPos, byte _targetType)
    {
        editPos = _editPos;
        targetType = _targetType;
    }

}



//���ձ���Ľṹ��
[System.Serializable]
public class SavingData
{
    public Vector3 ChunkLocation;
    public List<EditStruct> EditDataInChunkList = new List<EditStruct>();

    // Ϊ�˼��ݷ����л���ԭΪDictionary
    [System.NonSerialized]
    public Dictionary<Vector3, byte> EditDataInChunk = new Dictionary<Vector3, byte>();

    public SavingData(Vector3 _vec, Dictionary<Vector3, byte> _D)
    {
        ChunkLocation = _vec;
        EditDataInChunk = _D;

        // ���ֵ�ת��Ϊ�б�
        foreach (var kvp in _D)
        {
            EditDataInChunkList.Add(new EditStruct(kvp.Key, kvp.Value));
        }
    }

    // �ڷ����л���ԭDictionary
    public void RestoreDictionary()
    {
        EditDataInChunk = new Dictionary<Vector3, byte>();
        foreach (var structItem in EditDataInChunkList)
        {
            EditDataInChunk[structItem.editPos] = structItem.targetType;
        }
    }

    // ����Ƿ����ָ���� ChunkLocation
    public bool ContainsChunkLocation(Vector3 location)
    {
        return ChunkLocation == location;
    }
}


// ΪList���󴴽�һ����װ�࣬�Ա��ܹ�����ת��ΪJSON
[System.Serializable]
public class Wrapper<T>
{
    public List<T> Items;

    public Wrapper(List<T> items)
    {
        Items = items;
    }
}

//������ײ��
[System.Serializable]
public class CollosionRange
{
    public Vector2 xRange = new Vector3(0, 1f);
    public Vector2 yRange = new Vector3(0, 1f);
    public Vector2 zRange = new Vector3(0, 1f);
}



//PerlinNoise������
public static class PerlinNoise
{
    static float interpolate(float a0, float a1, float w)
    {
        //���Բ�ֵ
        //return (a1 - a0) * w + a0;

        //hermite��ֵ
        return Mathf.SmoothStep(a0, a1, w);
    }


    static Vector2 randomVector2(Vector2 p)
    {
        float random = Mathf.Sin(666 + p.x * 5678 + p.y * 1234) * 4321;
        return new Vector2(Mathf.Sin(random), Mathf.Cos(random));
    }


    static float dotGridGradient(Vector2 p1, Vector2 p2)
    {
        Vector2 gradient = randomVector2(p1);
        Vector2 offset = p2 - p1;
        return Vector2.Dot(gradient, offset) / 2 + 0.5f;
    }


    public static float GetPerlinNoise(float x, float y)
    {
        //������ά����
        Vector2 pos = new Vector2(x, y);
        //�����õ�������'����'���ĸ���������
        Vector2 rightUp = new Vector2((int)x + 1, (int)y + 1);
        Vector2 rightDown = new Vector2((int)x + 1, (int)y);
        Vector2 leftUp = new Vector2((int)x, (int)y + 1);
        Vector2 leftDown = new Vector2((int)x, (int)y);

        //����x�ϵĲ�ֵ
        float v1 = dotGridGradient(leftDown, pos);
        float v2 = dotGridGradient(rightDown, pos);
        float interpolation1 = interpolate(v1, v2, x - (int)x);

        //����y�ϵĲ�ֵ
        float v3 = dotGridGradient(leftUp, pos);
        float v4 = dotGridGradient(rightUp, pos);
        float interpolation2 = interpolate(v3, v4, x - (int)x);

        float value = interpolate(interpolation1, interpolation2, y - (int)y);
        return value;
    }
}