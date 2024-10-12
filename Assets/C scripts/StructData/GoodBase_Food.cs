using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "Goods_Food", menuName = "ScriptableObjects/Goods/Food", order = 5)]
public class GoodBase_Food : GoodBase
{
    [Foldout("持有参数", true)]
    public Vector3 handPosition;
    public Vector3 handRotation;

    [Foldout("食物独有参数", true)]
    [Header("恢复的血量")] public int recoveryAmount;

    // 覆盖虚拟属性
    public override Vector3 HandPosition { get { return handPosition; } }
    public override Vector3 HandRotation { get { return handRotation; } }
    public override int HealthRecoveryAmount { get { return recoveryAmount; } }
}
