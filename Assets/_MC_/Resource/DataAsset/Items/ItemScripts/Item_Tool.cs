using Homebrew;
using UnityEngine;

/// <summary>
/// Item_Tool（包括工具、武器、食物等）
/// </summary>
[CreateAssetMenu(fileName = "New Item_Tool", menuName = "Items/Item_Tool")]
public class Item_Tool : Item_Base
{
    [Foldout("ToolState", true)]
    [Header("是否是工具")] public bool isTool;         //区分功能性
    [Header("持有该Tool可破坏方块 (比如剑是不能破坏的)")] public bool canBreakBlockWithMouse1 = true;  //左键可破坏方块 

    [Foldout("CustomRotation", true)]
    [Header("自定义旋转")] public Vector3 CustomRotation;  //做一定的旋转
}
