using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GoodsData", menuName = "ScriptableObjects/Goods", order = 1)]
public class GoodData : ScriptableObject
{
    [Header("必填参数")]
    public string goodName;
    public Sprite icon; //物品栏图标
    public BlockClassfy classify;

    public Goods_建筑方块 Goods_建筑方块;
    public Goods_功能性方块 Goods_功能性方块;
    public Goods_工具 Goods_工具;
    public Goods_食物 Goods_食物;
    public Goods_禁用 Goods_禁用;
}


[System.Serializable]
public class Goods_建筑方块
{
    public float DestroyTime;
    public bool isSolid;        //是否会阻挡玩家
    public bool isTransparent;  //周边方块是否面剔除
    public bool canBeChoose;    //是否可被高亮方块捕捉到
    public bool candropBlock;   //是否掉落方块
    public bool isDIYCollision;//抽象来说就是方块向内挤压的数值
    public CollosionRange CollosionRange; //对于Y来说，(0.5f,0,0f)，就是Y正方向的面向内挤压0.5f，Y负方向的面向内挤压0.0f，即台阶的碰撞参数


    public bool is2d;           //用来区分显示
    public Sprite front_sprite; //掉落物
    public Sprite surround_sprite; //掉落物
    public Sprite top_sprit; //掉落物
    public Sprite buttom_sprit; //掉落物

    public AudioClip[] walk_clips = new AudioClip[2];
    public AudioClip broking_clip;
    public AudioClip broken_clip;

    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    public DrawMode DrawMode;


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

[System.Serializable]
public class Goods_功能性方块
{
    public float DestroyTime;
    public bool isSolid;        //是否会阻挡玩家
    public bool isTransparent;  //周边方块是否面剔除
    public bool canBeChoose;    //是否可被高亮方块捕捉到
    public bool candropBlock;   //是否掉落方块
    public bool isDIYCollision;//抽象来说就是方块向内挤压的数值
    public CollosionRange CollosionRange; //对于Y来说，(0.5f,0,0f)，就是Y正方向的面向内挤压0.5f，Y负方向的面向内挤压0.0f，即台阶的碰撞参数

    public bool isinteractable; //是否可被右键触发
    public bool IsOriented;     //是否跟随玩家朝向

    public bool is2d;           //用来区分显示
    public Sprite front_sprite; //掉落物
    public Sprite surround_sprite; //掉落物
    public Sprite top_sprit; //掉落物
    public Sprite buttom_sprit; //掉落物

    public AudioClip[] walk_clips = new AudioClip[2];
    public AudioClip broking_clip;
    public AudioClip broken_clip;

    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    public DrawMode DrawMode;

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


[System.Serializable]
public class Goods_工具
{
    public bool hasDiyRotation;//自定义旋转
    public Vector3 DiyRotation;

    public bool canBreakBlockWithMouse1;  // 左键可破坏方块
    public bool hasMouse2Action;          // 右键操作（比如放置方块）
    public bool hasMouse2HoldAction;      // 长按右键的功能（比如连续放置方块）


}


[System.Serializable]
public class Goods_食物
{
    public bool hasDiyRotation;//自定义旋转
    public Vector3 DiyRotation;

    public int healthRecoveryAmount;      // 可恢复的血量值


}


[System.Serializable]
public class Goods_禁用
{

}

