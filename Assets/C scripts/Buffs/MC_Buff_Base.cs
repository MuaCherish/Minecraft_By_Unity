using Homebrew;
using System.Collections;
using UnityEngine;

public abstract class MC_Buff_Base : MonoBehaviour
{

    #region ������Ҫ��д��

    /// <summary>
    /// BuffЧ��,����봫�ݲ����������д�ڶ���
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerator StartBuffEffect();
    public abstract IEnumerator StartBuffEffect(float[] _floatList);

    #endregion


    #region �����Ե��õ�

    /// <summary>
    /// ���ó���ʱ��
    /// </summary>
    public void ResetBuffDuration()
    {
        BuffControllerTimer = 0f;
    }

    /// <summary>
    /// ��BuffComponent���ã�����Buff
    /// </summary>
    public void StartBuff(MC_Buff_Component _Buff_Component, int _BuffType)
    {
        Buff_Component = _Buff_Component;
        BuffType = _BuffType; 
        StartCoroutine(StartBuffController());
    }


    #endregion

        
    #region Buff������

    [Header("Buff����")] [ReadOnly] public int BuffType;
    [Header("����ʱ��")] [ReadOnly] public float BuffDuration = 5f;
    [Header("��ǰʱ��")][ReadOnly] public float BuffControllerTimer = 0;
    private Coroutine _BuffEffectCoroutine;
    MC_Buff_Component Buff_Component;

    IEnumerator StartBuffController()
    {
        _BuffEffectCoroutine = StartCoroutine(StartBuffEffect());

        // Wait until BuffDuration has passed
        while (BuffControllerTimer < BuffDuration)
        {
            BuffControllerTimer += Time.deltaTime;
            yield return null;
        }

        EndBuff();
    }
    private void EndBuff()
    {
        StopCoroutine(_BuffEffectCoroutine);

        Buff_Component.RemoveBuff(BuffType);
    }

    #endregion

}
