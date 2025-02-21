using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using System;
using Homebrew;

public class MC_Buff_Component : MonoBehaviour
{

    //活跃的buff
    public Dictionary<int, MC_Buff_Base> activeBuffs = new Dictionary<int, MC_Buff_Base>();

    /// <summary>
    /// 添加 Buff
    /// </summary>
    public void AddBuff(int buffType, float _duration)
    {
        // 检查是否已经有该 Buff，如果有则重置持续时间
        if (activeBuffs.ContainsKey(buffType))
        {
            activeBuffs[buffType].ResetBuffDuration();
            return;
        }

        // 获取 Buff 类
        Type buffClass = BuffData.GetBuffType(buffType);
        if (buffClass == null)
        {
            Debug.LogWarning($"未知 Buff 类型: {buffType}");
            return;
        }

        // 动态添加 Buff 组件
        MC_Buff_Base newBuff = gameObject.AddComponent(buffClass) as MC_Buff_Base;
        if (newBuff != null)
        {
            activeBuffs[buffType] = newBuff;
            newBuff.SetBuffDuration(_duration);
            newBuff.StartBuff(this, buffType);
        }
    }

    /// <summary>
    /// 移除 Buff
    /// </summary>
    public void RemoveBuff(int buffType)
    {
        if (activeBuffs.TryGetValue(buffType, out MC_Buff_Base buff))
        { 
            Destroy(buff);
            activeBuffs.Remove(buffType);
        }
    }


}
