using Homebrew;
using System.Collections;
using UnityEngine;


//////////////////////////////////////
//������֪��
//1. ������ʱЧ�Ե�buff��һ��ʱ���ڱ����������������ʱ��
//2. �������ʱ��뵥һ����Ϊû�в���
//////////////////////////////////////
public abstract class MC_Buff_Base : MonoBehaviour
{

    #region ������Ҫ��д��

    /// <summary>
    /// BuffЧ��,����봫�ݲ����������д�ڶ���
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerator StartBuffEffect();
    public abstract void EndBuffEffect();

    #endregion


    #region �����Ե��õ�

    /// <summary>
    /// ��BuffComponent���ã�����Buff
    /// </summary>
    public void StartBuff(MC_Buff_Component _Buff_Component, int _BuffType)
    {
        Buff_Component = _Buff_Component;
        BuffType = _BuffType;
        StartCoroutine(StartBuffController());
    }

    /// <summary>
    /// ����buff����ʱ��
    /// </summary>
    public void SetBuffDuration(float _NewDuration)
    {
        BuffDuration = _NewDuration;
    }

    /// <summary>
    /// ���ó���ʱ��
    /// �������ͨ����д���������ã����籬ը����һ���Թ��̲������ó���ʱ��
    /// </summary>
    public virtual void ResetBuffDuration()
    {
        BuffControllerTimer = 0f;
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
        EndBuffEffect();
        Buff_Component.RemoveBuff(BuffType);
    }

    #endregion

}
