using Homebrew;
using UnityEngine;

/// <summary>
/// 物品基类，所有物品都继承于此
/// </summary>
public class Item_Base : ScriptableObject
{

    [Foldout("基础参数", true)]
    [Header("Item名字")] public string itemName;
    [Header("Item分类")] public BlockClassfy BlockClassfy;
    [Header("物品栏图标")] public Sprite icon;

}