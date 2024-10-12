using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Goods_Tool", menuName = "ScriptableObjects/Goods/Tool", order = 4)]
public class GoodBase_Tool : GoodBase
{
    [Foldout("持有参数", true)]
    public Vector3 handPosition;
    public Vector3 handRotation;

    [Foldout("工具独有参数", true)]
    [Header("左键可破坏方块")] public bool canBreakBlockWithMouse1;
    [Header("右键有特定操作")] public bool hasMouse2Action;
    [Header("长按右键有特定操作")] public bool hasMouse2HoldAction;

    // 覆盖虚拟属性
    public override Vector3 HandPosition { get { return handPosition; } }
    public override Vector3 HandRotation { get { return handRotation; } }
    public override bool CanBreakBlockWithMouse1 { get { return canBreakBlockWithMouse1; } }
    public override bool HasMouse2Action { get { return hasMouse2Action; } }
    public override bool HasMouse2HoldAction { get { return hasMouse2HoldAction; } }
}
