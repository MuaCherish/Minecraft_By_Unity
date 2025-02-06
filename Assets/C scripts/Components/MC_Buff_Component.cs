using Homebrew;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 施工中.........
/// 目前只支持提前预设的buff，不支持运行中的buff添加，因为还没有地方能用得到
/// </summary>
public class MC_Buff_Component : MonoBehaviour
{
    #region 状态

    [Foldout("Buff设置")]
    [Header("当前buff")] public List<BuffBase> Buffs = new List<BuffBase>();

    // 定义一个字典，将 BuffType 映射到对应的 Buff 类型
    private static readonly Dictionary<BuffType, Type> BuffTypeToClassMap = new Dictionary<BuffType, Type>
    {
        { BuffType.Vision, typeof(Buff_Vision) },
        { BuffType.Swim, typeof(Buff_Swimming) },
    };

    #endregion

    #region 周期函数

    void Update()
    {
        foreach (BuffBase buff in Buffs)
        {
            buff.Update();
        }
    }

    #endregion

    #region Buff管理器

    /// <summary>
    /// 添加能力
    /// </summary>
    /// <param name="_buffType"></param>
    public void AddBuff(BuffType _buffType)
    {
        // 检查是否已经存在该类型的Buff
        if (Buffs.Exists(buff => buff.buff == _buffType))
        {
            Debug.LogWarning($"Buff of type {_buffType} already exists.");
            return;
        }

        // 通过字典找到对应的 Buff 类型
        if (BuffTypeToClassMap.TryGetValue(_buffType, out Type buffClass))
        {
            // 使用反射创建实例
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
    /// 失去能力
    /// </summary>
    /// <param name="_buffType"></param>
    public void RemoveBuff(BuffType _buffType)
    {
        // 查找并移除对应类型的Buff
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