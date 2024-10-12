using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Goods_Tool", menuName = "ScriptableObjects/Goods/Tool", order = 4)]
public class GoodBase_Tool : GoodBase
{
    [Foldout("���в���", true)]
    public Vector3 handPosition;
    public Vector3 handRotation;

    [Foldout("���߶��в���", true)]
    [Header("������ƻ�����")] public bool canBreakBlockWithMouse1;
    [Header("�Ҽ����ض�����")] public bool hasMouse2Action;
    [Header("�����Ҽ����ض�����")] public bool hasMouse2HoldAction;

    // ������������
    public override Vector3 HandPosition { get { return handPosition; } }
    public override Vector3 HandRotation { get { return handRotation; } }
    public override bool CanBreakBlockWithMouse1 { get { return canBreakBlockWithMouse1; } }
    public override bool HasMouse2Action { get { return hasMouse2Action; } }
    public override bool HasMouse2HoldAction { get { return hasMouse2HoldAction; } }
}
