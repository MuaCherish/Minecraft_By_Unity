using Homebrew;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Item_Block
/// </summary>
[CreateAssetMenu(fileName = "New Item_Block", menuName = "Items/Item_Block")]
public class Item_Block : Item_Base
{
    [Foldout("BlockState", true)]
    [Header("销毁时间")] public float DestroyTime;
    [Header("是否是固体")] public bool isSolid;        //是否会阻挡玩家
    [Header("是否透明 (即靠近Solid的面会被剔除)")] public bool isTransparent;  //周边方块是否面剔除
    [Header("是否可被选择 (比如水是不可被选择的)")] public bool canBeChoose;    //是否可被高亮方块捕捉到
    [Header("是否掉落方块")] public bool candropBlock;   //是否掉落方块
    [Header("是否具备朝向 (初始朝向为朝向玩家方向)")] public bool IsOriented;     //是否跟随玩家朝向
    [Header("是否可被右键触发")] public bool isinteractable; //是否可被右键触发
    [Header("是否可被覆盖 (比如在花上放置会把花覆盖)")] public bool CanBeCover;        //是否会被覆盖
    [Header("悬空会被销毁")] public bool NotSuspended;    //不可悬空放置

    [Foldout("DIYCollision", true)]
    [Header("是否自定义")] public bool isDIYCollision;  //抽象来说就是方块向内挤压的数值
    [Header("XYZ积分范围")] public CollosionRange CollosionRange;  //对于Y来说，(0.5f,0,0f)，就是Y正方向的面向内挤压0.5f，Y负方向的面向内挤压0.0f，即台阶的碰撞参数

    [Foldout("Sprits", true)]
    [Header("是否是2D (false即会在物品栏生成3d纹理，true会使用icon作为物品栏纹理)")] public bool is2d;           //用来区分显示
    [Header("正面")] public Sprite front_sprite; //掉落物
    [Header("侧面")] public Sprite sprite;  //侧面
    [Header("上面")] public Sprite top_sprit; //掉落物
    [Header("下面")] public Sprite buttom_sprit; //掉落物

    [Foldout("AudioClips", true)]
    [Header("走路音效")] public AudioClip[] walk_clips = new AudioClip[2];
    [Header("破坏中音效")] public AudioClip broking_clip;
    [Header("被破坏音效")] public AudioClip broken_clip;

    [Foldout("Mesh (对于SpriteAtlas中纹理的下标)", true)]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    [Header("绘制模式")] public DrawMode DrawMode;
    [Header("接近空气的面会生成双面")] public bool GenerateTwoFaceWithAir;    //如果朝向空气，则双面绘制
    [Header("额外面生成判断(后前上下左右)")] public List<FaceCheckMode> OtherFaceCheck;

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
