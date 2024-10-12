using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GoodBase : ScriptableObject
{
    [Foldout("�������", true)]
    [Header("����")] public string goodName;
    [Header("��Ʒ��ͼ��")] public Sprite icon; //��Ʒ��ͼ��
    [Header("���÷���")] public BlockClassfy classify;

    // �������ԣ�Block�������������ԣ�
    public virtual float DestroyTime { get { return 0f; } }
    public virtual bool IsTransparent { get { return false; } }
    public virtual bool CanBeChosen { get { return false; } }
    public virtual bool CanDropBlock { get { return false; } }
    public virtual bool IsSolid { get { return false; } }
    public virtual bool IsDIYCollision { get { return false; } }
    public virtual CollosionRange CollisionRange { get { return new CollosionRange(); } } // Ĭ��ʵ��
    public virtual bool Is2d { get { return false; } }
    public virtual Sprite[] Sprites { get { return new Sprite[0]; } } // ���ؿ����飬����null
    public virtual AudioClip[] WalkClips { get { return new AudioClip[0]; } }
    public virtual AudioClip[] BrokeClips { get { return new AudioClip[0]; } }
    public virtual DrawMode DrawMode { get { return DrawMode.Null; } }
    public virtual int[] FaceTextures { get { return new int[6]; } }
    public virtual List<FaceCheckMode> OtherFaceCheck { get { return new List<FaceCheckMode>(); } }
    public virtual bool IsInteractable { get { return false; } }
    public virtual bool IsOriented { get { return false; } }

    // �������ԣ�Tool���Food������ԣ�
    public virtual Vector3 HandPosition { get { return Vector3.zero; } }
    public virtual Vector3 HandRotation { get { return Vector3.zero; } }
    public virtual bool CanBreakBlockWithMouse1 { get { return false; } }
    public virtual bool HasMouse2Action { get { return false; } }
    public virtual bool HasMouse2HoldAction { get { return false; } }
    public virtual int HealthRecoveryAmount { get { return 0; } }
}