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
    [Header("����ʱ��")] public float DestroyTime;
    [Header("�Ƿ��ǹ���")] public bool isSolid;        //�Ƿ���赲���
    [Header("�Ƿ�͸�� (������Solid����ᱻ�޳�)")] public bool isTransparent;  //�ܱ߷����Ƿ����޳�
    [Header("�Ƿ�ɱ�ѡ�� (����ˮ�ǲ��ɱ�ѡ���)")] public bool canBeChoose;    //�Ƿ�ɱ��������鲶׽��
    [Header("�Ƿ���䷽��")] public bool candropBlock;   //�Ƿ���䷽��
    [Header("�Ƿ�߱����� (��ʼ����Ϊ������ҷ���)")] public bool IsOriented;     //�Ƿ������ҳ���
    [Header("�Ƿ�ɱ��Ҽ�����")] public bool isinteractable; //�Ƿ�ɱ��Ҽ�����
    [Header("�Ƿ�ɱ����� (�����ڻ��Ϸ��û�ѻ�����)")] public bool CanBeCover;        //�Ƿ�ᱻ����
    [Header("���ջᱻ����")] public bool NotSuspended;    //�������շ���

    [Foldout("DIYCollision", true)]
    [Header("�Ƿ��Զ���")] public bool isDIYCollision;  //������˵���Ƿ������ڼ�ѹ����ֵ
    [Header("XYZ���ַ�Χ")] public CollosionRange CollosionRange;  //����Y��˵��(0.5f,0,0f)������Y������������ڼ�ѹ0.5f��Y������������ڼ�ѹ0.0f����̨�׵���ײ����

    [Foldout("Sprits", true)]
    [Header("�Ƿ���2D (false��������Ʒ������3d����true��ʹ��icon��Ϊ��Ʒ������)")] public bool is2d;           //����������ʾ
    [Header("����")] public Sprite front_sprite; //������
    [Header("����")] public Sprite sprite;  //����
    [Header("����")] public Sprite top_sprit; //������
    [Header("����")] public Sprite buttom_sprit; //������

    [Foldout("AudioClips", true)]
    [Header("��·��Ч")] public AudioClip[] walk_clips = new AudioClip[2];
    [Header("�ƻ�����Ч")] public AudioClip broking_clip;
    [Header("���ƻ���Ч")] public AudioClip broken_clip;

    [Foldout("Mesh (����SpriteAtlas��������±�)", true)]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    [Header("����ģʽ")] public DrawMode DrawMode;
    [Header("�ӽ��������������˫��")] public bool GenerateTwoFaceWithAir;    //��������������˫�����
    [Header("�����������ж�(��ǰ��������)")] public List<FaceCheckMode> OtherFaceCheck;

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
