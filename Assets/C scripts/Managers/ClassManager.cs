
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



//比如 门：{0, isSolid false} 代表后方如果是Solid则不生成面
// 0 1  2   3  4  5
//后 前 上 下 左 右
[System.Serializable]
public class FaceCheckMode
{
    public int FaceDirect;  //这个Direct属于本地方向，比如0指的是自己朝向的后方，因为后面要顾及到物体的旋转
    public FaceCheck_Enum checktype;
    public byte appointType;
    public DrawMode appointDrawmode;
    public bool isCreateFace;
}



//结构体BlockType
//存储方块种类+面对应的UV
[System.Serializable]
public class BlockType
{
    [Header("方块参数")]
    public string blockName;
    public float DestroyTime;
    public bool isSolid;        //是否会阻挡玩家
    public bool isTransparent;  //周边方块是否面剔除
    public bool canBeChoose;    //是否可被高亮方块捕捉到
    public bool candropBlock;   //是否掉落方块
    public bool IsOriented;     //是否跟随玩家朝向
    public bool isinteractable; //是否可被右键触发
    public bool is2d;           //用来区分显示

    [Header("工具参数")]
    public bool isTool;         //区分功能性
    public bool isNeedRotation; //true后会做一定的旋转


    [Header("自定义碰撞")]
    public bool isDIYCollision;
    //抽象来说就是方块向内挤压的数值
    //对于Y来说，(0.5f,0,0f)，就是Y正方向的面向内挤压0.5f，Y负方向的面向内挤压0.0f，即台阶的碰撞参数
    public CollosionRange CollosionRange;

    [Header("Sprits")]
    public Sprite icon; //物品栏图标
    public Sprite sprite; //掉落物
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


//工具类
//[System.Serializable]
//public class ToolType
//{
//    public string name;
//    public Sprite sprite;
//}

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



// 一个完整的WorldSetting
[Serializable]
public class WorldSetting
{
    public String date = "0000";//存档创建日期
    public String name = "新的世界";
    public int seed = 0;
    public GameMode gameMode = GameMode.Survival;
    public int worldtype = VoxelData.Biome_Default;
    public Vector3 playerposition;   // 保存玩家的坐标
    public Quaternion playerrotation; // 保存玩家的旋转

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


//玩家修改的数据缓存
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



//最终保存的结构体
[System.Serializable]
public class SavingData
{
    public Vector3 ChunkLocation;
    public List<EditStruct> EditDataInChunkList = new List<EditStruct>();

    // 为了兼容反序列化后还原为Dictionary
    [System.NonSerialized]
    public Dictionary<Vector3, byte> EditDataInChunk = new Dictionary<Vector3, byte>();

    public SavingData(Vector3 _vec, Dictionary<Vector3, byte> _D)
    {
        ChunkLocation = _vec;
        EditDataInChunk = _D;

        // 将字典转换为列表
        foreach (var kvp in _D)
        {
            EditDataInChunkList.Add(new EditStruct(kvp.Key, kvp.Value));
        }
    }

    // 在反序列化后还原Dictionary
    public void RestoreDictionary()
    {
        EditDataInChunk = new Dictionary<Vector3, byte>();
        foreach (var structItem in EditDataInChunkList)
        {
            EditDataInChunk[structItem.editPos] = structItem.targetType;
        }
    }

    // 检查是否包含指定的 ChunkLocation
    public bool ContainsChunkLocation(Vector3 location)
    {
        return ChunkLocation == location;
    }
}


// 为List对象创建一个封装类，以便能够将其转换为JSON
[System.Serializable]
public class Wrapper<T>
{
    public List<T> Items;

    public Wrapper(List<T> items)
    {
        Items = items;
    }
}

//方块碰撞类
[System.Serializable]
public class CollosionRange
{
    public Vector2 xRange = new Vector3(0, 1f);
    public Vector2 yRange = new Vector3(0, 1f);
    public Vector2 zRange = new Vector3(0, 1f);
}



//PerlinNoise噪声类
public static class PerlinNoise
{
    static float interpolate(float a0, float a1, float w)
    {
        //线性插值
        //return (a1 - a0) * w + a0;

        //hermite插值
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
        //声明二维坐标
        Vector2 pos = new Vector2(x, y);
        //声明该点所处的'格子'的四个顶点坐标
        Vector2 rightUp = new Vector2((int)x + 1, (int)y + 1);
        Vector2 rightDown = new Vector2((int)x + 1, (int)y);
        Vector2 leftUp = new Vector2((int)x, (int)y + 1);
        Vector2 leftDown = new Vector2((int)x, (int)y);

        //计算x上的插值
        float v1 = dotGridGradient(leftDown, pos);
        float v2 = dotGridGradient(rightDown, pos);
        float interpolation1 = interpolate(v1, v2, x - (int)x);

        //计算y上的插值
        float v3 = dotGridGradient(leftUp, pos);
        float v4 = dotGridGradient(rightUp, pos);
        float interpolation2 = interpolate(v3, v4, x - (int)x);

        float value = interpolate(interpolation1, interpolation2, y - (int)y);
        return value;
    }
}