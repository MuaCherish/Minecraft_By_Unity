using Homebrew;
using UnityEngine;

/// <summary>
/// ��Ʒ���࣬������Ʒ���̳��ڴ�
/// </summary>
public class Item_Base : ScriptableObject
{

    [Foldout("��������", true)]
    [Header("Item����")] public string itemName;
    [Header("Item����")] public BlockClassfy BlockClassfy;
    [Header("��Ʒ��ͼ��")] public Sprite icon;

}