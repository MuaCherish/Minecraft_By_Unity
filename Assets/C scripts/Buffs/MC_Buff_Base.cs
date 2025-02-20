using Homebrew;
using System.Collections;
using UnityEngine;


//////////////////////////////////////
//子类须知：
//1. 必须是时效性的buff，一定时间内必须结束，除非重置时间
//2. 触发性质必须单一，因为没有参数
//////////////////////////////////////
public abstract class MC_Buff_Base : MonoBehaviour
{

    #region 子类需要重写的

    /// <summary>
    /// Buff效果,如果想传递参数则可以重写第二个
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerator StartBuffEffect();
    public abstract void EndBuffEffect();

    #endregion


    #region 外界可以调用的

    /// <summary>
    /// 由BuffComponent调用，启动Buff
    /// </summary>
    public void StartBuff(MC_Buff_Component _Buff_Component, int _BuffType)
    {
        Buff_Component = _Buff_Component;
        BuffType = _BuffType;
        StartCoroutine(StartBuffController());
    }

    /// <summary>
    /// 设置buff持续时间
    /// </summary>
    public void SetBuffDuration(float _NewDuration)
    {
        BuffDuration = _NewDuration;
    }

    /// <summary>
    /// 重置持续时间
    /// 子类可以通过重写来禁用重置，比如爆炸这种一次性过程不能重置持续时间
    /// </summary>
    public virtual void ResetBuffDuration()
    {
        BuffControllerTimer = 0f;
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
        EndBuffEffect();
        Buff_Component.RemoveBuff(BuffType);
    }

    #endregion

}
