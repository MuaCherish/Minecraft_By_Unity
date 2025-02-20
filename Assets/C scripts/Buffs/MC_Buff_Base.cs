using Homebrew;
using System.Collections;
using UnityEngine;

public abstract class MC_Buff_Base : MonoBehaviour
{

    #region 子类需要重写的

    /// <summary>
    /// Buff效果,如果想传递参数则可以重写第二个
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerator StartBuffEffect();
    public abstract IEnumerator StartBuffEffect(float[] _floatList);

    #endregion


    #region 外界可以调用的

    /// <summary>
    /// 重置持续时间
    /// </summary>
    public void ResetBuffDuration()
    {
        BuffControllerTimer = 0f;
    }

    /// <summary>
    /// 由BuffComponent调用，启动Buff
    /// </summary>
    public void StartBuff(MC_Buff_Component _Buff_Component, int _BuffType)
    {
        Buff_Component = _Buff_Component;
        BuffType = _BuffType; 
        StartCoroutine(StartBuffController());
    }


    #endregion

        
    #region Buff控制器

    [Header("Buff类型")] [ReadOnly] public int BuffType;
    [Header("持续时间")] [ReadOnly] public float BuffDuration = 5f;
    [Header("当前时间")][ReadOnly] public float BuffControllerTimer = 0;
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
