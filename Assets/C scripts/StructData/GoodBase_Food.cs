using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "Goods_Food", menuName = "ScriptableObjects/Goods/Food", order = 5)]
public class GoodBase_Food : GoodBase
{
    [Foldout("���в���", true)]
    public Vector3 handPosition;
    public Vector3 handRotation;

    [Foldout("ʳ����в���", true)]
    [Header("�ָ���Ѫ��")] public int recoveryAmount;

    // ������������
    public override Vector3 HandPosition { get { return handPosition; } }
    public override Vector3 HandRotation { get { return handRotation; } }
    public override int HealthRecoveryAmount { get { return recoveryAmount; } }
}
