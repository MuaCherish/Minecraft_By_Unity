using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using System;
using Homebrew;

public class MC_Buff_Component : MonoBehaviour
{

    #region 状态

    [Foldout("状态", true)]
    [Header("活跃的buff")] public Dictionary<int, MC_Buff_Base> activeBuffs = new Dictionary<int, MC_Buff_Base>();


    #endregion


    #region 周期函数


    World world;

    private void Awake()
    {
        world = SceneData.GetWorld();
    }

    private void Update()
    {
        if (world.game_state == Game_State.Playing)
        {
            Handle_GameState_Playing();
        }
    }

    void Handle_GameState_Playing()
    {

    }


    #endregion

                                    
    #region Buff管理器

    /// <summary>
    /// 添加 Buff
    /// </summary>
    public void AddBuff(int buffType)
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


    #endregion

}
