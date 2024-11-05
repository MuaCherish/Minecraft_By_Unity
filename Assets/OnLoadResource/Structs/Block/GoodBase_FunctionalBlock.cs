using Homebrew;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Goods_Block_Functional", menuName = "ScriptableObjects/Goods/FunctionalBlock", order = 3)]
public class GoodBase_FunctionalBlock : GoodBase
{
    [Foldout("基本参数", true)]
    [Header("销毁时间")] public float destroyTime;
    [Header("是否是透明的")] public bool isTransparent;
    [Header("是否可被高亮方块选择")] public bool canBeChosen;
    [Header("是否掉落方块")] public bool canDropBlock;

    [Foldout("碰撞参数", true)]
    [Header("是否有碰撞")] public bool isSolid;
    [Header("是否有自定义碰撞")] public bool isDIYCollision;
    [Header("自定义碰撞参数")] public CollosionRange collisionRange;

    [Foldout("纹理参数", true)]
    [Header("渲染2d还是3d")] public bool is2d;
    [Header("纹理数组(Front-Surround-Top-Buttom)")] public Sprite[] blockSprites;

    [Foldout("音效参数", true)]
    [Header("行走音效")] public AudioClip[] walkSounds = new AudioClip[2];
    [Header("破坏音效(Broking-Broken)")] public AudioClip[] brokeSounds = new AudioClip[2];

    [Foldout("绘制参数", true)]
    [Header("绘制模式")] public DrawMode drawMode;
    [Header("绘制数组(后前上下左右)")] public int[] faceTextures = new int[6];
    [Header("额外面生成判断")] public List<FaceCheckMode> otherFaceCheck;

    [Foldout("功能性方块独有参数", true)]
    [Header("是否右键可交互")] public bool isInteractable;
    [Header("是否具有方向")] public bool isOriented;

    // 覆盖虚拟属性
    public override bool Is2d { get { return is2d; } }
    public override float DestroyTime { get { return destroyTime; } }
    public override bool IsTransparent { get { return isTransparent; } }
    public override bool CanBeChosen { get { return canBeChosen; } }
    public override bool CanDropBlock { get { return canDropBlock; } }
    public override bool IsSolid { get { return isSolid; } }
    public override bool IsDIYCollision { get { return isDIYCollision; } }
    public override CollosionRange CollisionRange { get { return collisionRange; } }
    public override Sprite[] Sprites { get { return blockSprites; } }
    public override AudioClip[] WalkClips { get { return walkSounds; } }
    public override AudioClip[] BrokeClips { get { return brokeSounds; } }
    public override DrawMode DrawMode { get { return drawMode; } }
    public override int[] FaceTextures { get { return faceTextures; } }
    public override List<FaceCheckMode> OtherFaceCheck { get { return otherFaceCheck; } }
    public override bool IsInteractable { get { return isInteractable; } }
    public override bool IsOriented { get { return isOriented; } }

}
