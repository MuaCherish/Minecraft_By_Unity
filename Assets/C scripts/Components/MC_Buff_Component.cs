using Homebrew;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ʩ����.........
/// Ŀǰֻ֧����ǰԤ���buff����֧�������е�buff��ӣ���Ϊ��û�еط����õõ�
/// </summary>
public class MC_Buff_Component : MonoBehaviour
{
    #region ״̬

    [Foldout("Buff����")]
    [Header("��ǰbuff")] public List<BuffBase> Buffs = new List<BuffBase>();

    // ����һ���ֵ䣬�� BuffType ӳ�䵽��Ӧ�� Buff ����
    private static readonly Dictionary<BuffType, Type> BuffTypeToClassMap = new Dictionary<BuffType, Type>
    {
        { BuffType.Vision, typeof(Buff_Vision) },
        { BuffType.Swim, typeof(Buff_Swimming) },
    };

    #endregion

    #region ���ں���

    void Update()
    {
        foreach (BuffBase buff in Buffs)
        {
            buff.Update();
        }
    }

    #endregion

    #region Buff������

    /// <summary>
    /// �������
    /// </summary>
    /// <param name="_buffType"></param>
    public void AddBuff(BuffType _buffType)
    {
        // ����Ƿ��Ѿ����ڸ����͵�Buff
        if (Buffs.Exists(buff => buff.buff == _buffType))
        {
            Debug.LogWarning($"Buff of type {_buffType} already exists.");
            return;
        }

        // ͨ���ֵ��ҵ���Ӧ�� Buff ����
        if (BuffTypeToClassMap.TryGetValue(_buffType, out Type buffClass))
        {
            // ʹ�÷��䴴��ʵ��
            BuffBase newBuff = Activator.CreateInstance(buffClass) as BuffBase;
            if (newBuff != null)
            {
                newBuff.buff = _buffType;
                Buffs.Add(newBuff);
                Debug.Log($"Added Buff: {_buffType}");
            }
            else
            {
                Debug.LogError($"Failed to create Buff instance for type: {_buffType}");
            }
        }
        else
        {
            Debug.LogError($"Unknown BuffType: {_buffType}");
        }
    }

    /// <summary>
    /// ʧȥ����
    /// </summary>
    /// <param name="_buffType"></param>
    public void RemoveBuff(BuffType _buffType)
    {
        // ���Ҳ��Ƴ���Ӧ���͵�Buff
        BuffBase buffToRemove = Buffs.Find(buff => buff.buff == _buffType);
        if (buffToRemove != null)
        {
            Buffs.Remove(buffToRemove);
            Debug.Log($"Removed Buff: {_buffType}");
        }
        else
        {
            Debug.LogWarning($"Buff of type {_buffType} not found.");
        }
    }

    #endregion
}

#region Buff_Vision

public class Buff_Vision : BuffBase
{
    public override void Update()
    {
        Debug.Log("Updating Vision Ability");
    }
}

#endregion


#region Buff_Swimming

public class Buff_Swimming : BuffBase
{
    public override void Update()
    {
        Debug.Log("Updating Swimming Ability");
    }
}

#endregion