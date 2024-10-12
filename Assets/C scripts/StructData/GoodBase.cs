using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GoodBase : ScriptableObject
{
    [Foldout("必填参数", true)]
    [Header("名字")] public string goodName;
    [Header("物品栏图标")] public Sprite icon; //物品栏图标
    [Header("作用分类")] public BlockClassfy classify;

    // 虚拟属性（Block类和其他类的属性）
    public virtual float DestroyTime { get { return 0f; } }
    public virtual bool IsTransparent { get { return false; } }
    public virtual bool CanBeChosen { get { return false; } }
    public virtual bool CanDropBlock { get { return false; } }
    public virtual bool IsSolid { get { return false; } }
    public virtual bool IsDIYCollision { get { return false; } }
    public virtual CollosionRange CollisionRange { get { return new CollosionRange(); } } // 默认实例
    public virtual bool Is2d { get { return false; } }
    public virtual Sprite[] Sprites { get { return new Sprite[0]; } } // 返回空数组，避免null
    public virtual AudioClip[] WalkClips { get { return new AudioClip[0]; } }
    public virtual AudioClip[] BrokeClips { get { return new AudioClip[0]; } }
    public virtual DrawMode DrawMode { get { return DrawMode.Null; } }
    public virtual int[] FaceTextures { get { return new int[6]; } }
    public virtual List<FaceCheckMode> OtherFaceCheck { get { return new List<FaceCheckMode>(); } }
    public virtual bool IsInteractable { get { return false; } }
    public virtual bool IsOriented { get { return false; } }

    // 虚拟属性（Tool类和Food类的属性）
    public virtual Vector3 HandPosition { get { return Vector3.zero; } }
    public virtual Vector3 HandRotation { get { return Vector3.zero; } }
    public virtual bool CanBreakBlockWithMouse1 { get { return false; } }
    public virtual bool HasMouse2Action { get { return false; } }
    public virtual bool HasMouse2HoldAction { get { return false; } }
    public virtual int HealthRecoveryAmount { get { return 0; } }
}