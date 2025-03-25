using Homebrew;
using UnityEngine;

/// <summary>
/// Item_Tool���������ߡ�������ʳ��ȣ�
/// </summary>
[CreateAssetMenu(fileName = "New Item_Tool", menuName = "Items/Item_Tool")]
public class Item_Tool : Item_Base
{
    [Foldout("ToolState", true)]
    [Header("�Ƿ��ǹ���")] public bool isTool;         //���ֹ�����
    [Header("���и�Tool���ƻ����� (���罣�ǲ����ƻ���)")] public bool canBreakBlockWithMouse1 = true;  //������ƻ����� 

    [Foldout("CustomRotation", true)]
    [Header("�Զ�����ת")] public Vector3 CustomRotation;  //��һ������ת
}
