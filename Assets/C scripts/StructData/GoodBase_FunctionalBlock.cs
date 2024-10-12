using Homebrew;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Goods_Block_Functional", menuName = "ScriptableObjects/Goods/FunctionalBlock", order = 3)]
public class GoodBase_FunctionalBlock : GoodBase
{
    [Foldout("��������", true)]
    [Header("����ʱ��")] public float destroyTime;
    [Header("�Ƿ���͸����")] public bool isTransparent;
    [Header("�Ƿ�ɱ���������ѡ��")] public bool canBeChosen;
    [Header("�Ƿ���䷽��")] public bool canDropBlock;

    [Foldout("��ײ����", true)]
    [Header("�Ƿ�����ײ")] public bool isSolid;
    [Header("�Ƿ����Զ�����ײ")] public bool isDIYCollision;
    [Header("�Զ�����ײ����")] public CollosionRange collisionRange;

    [Foldout("�������", true)]
    [Header("��Ⱦ2d����3d")] public bool is2d;
    [Header("��������(Front-Surround-Top-Buttom)")] public Sprite[] blockSprites;

    [Foldout("��Ч����", true)]
    [Header("������Ч")] public AudioClip[] walkSounds = new AudioClip[2];
    [Header("�ƻ���Ч(Broking-Broken)")] public AudioClip[] brokeSounds = new AudioClip[2];

    [Foldout("���Ʋ���", true)]
    [Header("����ģʽ")] public DrawMode drawMode;
    [Header("��������(��ǰ��������)")] public int[] faceTextures = new int[6];
    [Header("�����������ж�")] public List<FaceCheckMode> otherFaceCheck;

    [Foldout("�����Է�����в���", true)]
    [Header("�Ƿ��Ҽ��ɽ���")] public bool isInteractable;
    [Header("�Ƿ���з���")] public bool isOriented;

    // ������������
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
