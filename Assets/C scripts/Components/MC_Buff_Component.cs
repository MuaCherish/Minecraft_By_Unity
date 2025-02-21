using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using System;
using Homebrew;

public class MC_Buff_Component : MonoBehaviour
{

    //��Ծ��buff
    public Dictionary<int, MC_Buff_Base> activeBuffs = new Dictionary<int, MC_Buff_Base>();

    /// <summary>
    /// ��� Buff
    /// </summary>
    public void AddBuff(int buffType, float _duration)
    {
        // ����Ƿ��Ѿ��и� Buff������������ó���ʱ��
        if (activeBuffs.ContainsKey(buffType))
        {
            activeBuffs[buffType].ResetBuffDuration();
            return;
        }

        // ��ȡ Buff ��
        Type buffClass = BuffData.GetBuffType(buffType);
        if (buffClass == null)
        {
            Debug.LogWarning($"δ֪ Buff ����: {buffType}");
            return;
        }

        // ��̬��� Buff ���
        MC_Buff_Base newBuff = gameObject.AddComponent(buffClass) as MC_Buff_Base;
        if (newBuff != null)
        {
            activeBuffs[buffType] = newBuff;
            newBuff.SetBuffDuration(_duration);
            newBuff.StartBuff(this, buffType);
        }
    }

    /// <summary>
    /// �Ƴ� Buff
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
