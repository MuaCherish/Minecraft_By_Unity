using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GoodsData", menuName = "ScriptableObjects/Goods", order = 1)]
public class GoodData : ScriptableObject
{
    [Header("�������")]
    public string goodName;
    public Sprite icon; //��Ʒ��ͼ��
    public BlockClassfy classify;

    public Goods_�������� Goods_��������;
    public Goods_�����Է��� Goods_�����Է���;
    public Goods_���� Goods_����;
    public Goods_ʳ�� Goods_ʳ��;
    public Goods_���� Goods_����;
}


[System.Serializable]
public class Goods_��������
{
    public float DestroyTime;
    public bool isSolid;        //�Ƿ���赲���
    public bool isTransparent;  //�ܱ߷����Ƿ����޳�
    public bool canBeChoose;    //�Ƿ�ɱ��������鲶׽��
    public bool candropBlock;   //�Ƿ���䷽��
    public bool isDIYCollision;//������˵���Ƿ������ڼ�ѹ����ֵ
    public CollosionRange CollosionRange; //����Y��˵��(0.5f,0,0f)������Y������������ڼ�ѹ0.5f��Y������������ڼ�ѹ0.0f����̨�׵���ײ����


    public bool is2d;           //����������ʾ
    public Sprite front_sprite; //������
    public Sprite surround_sprite; //������
    public Sprite top_sprit; //������
    public Sprite buttom_sprit; //������

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

[System.Serializable]
public class Goods_�����Է���
{
    public float DestroyTime;
    public bool isSolid;        //�Ƿ���赲���
    public bool isTransparent;  //�ܱ߷����Ƿ����޳�
    public bool canBeChoose;    //�Ƿ�ɱ��������鲶׽��
    public bool candropBlock;   //�Ƿ���䷽��
    public bool isDIYCollision;//������˵���Ƿ������ڼ�ѹ����ֵ
    public CollosionRange CollosionRange; //����Y��˵��(0.5f,0,0f)������Y������������ڼ�ѹ0.5f��Y������������ڼ�ѹ0.0f����̨�׵���ײ����

    public bool isinteractable; //�Ƿ�ɱ��Ҽ�����
    public bool IsOriented;     //�Ƿ������ҳ���

    public bool is2d;           //����������ʾ
    public Sprite front_sprite; //������
    public Sprite surround_sprite; //������
    public Sprite top_sprit; //������
    public Sprite buttom_sprit; //������

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


[System.Serializable]
public class Goods_����
{
    public bool hasDiyRotation;//�Զ�����ת
    public Vector3 DiyRotation;

    public bool canBreakBlockWithMouse1;  // ������ƻ�����
    public bool hasMouse2Action;          // �Ҽ�������������÷��飩
    public bool hasMouse2HoldAction;      // �����Ҽ��Ĺ��ܣ������������÷��飩


}


[System.Serializable]
public class Goods_ʳ��
{
    public bool hasDiyRotation;//�Զ�����ת
    public Vector3 DiyRotation;

    public int healthRecoveryAmount;      // �ɻָ���Ѫ��ֵ


}


[System.Serializable]
public class Goods_����
{

}

