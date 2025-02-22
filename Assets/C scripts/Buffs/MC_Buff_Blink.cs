using MCEntity;
using System.Collections;
using UnityEngine;

public class MC_Buff_Blink : MC_Buff_Base
{

    #region 周期函数

    MC_Component_Life Life_Component;
    MC_Component_Music Music_Component;
   
    private void Awake()
    {
        Life_Component = GetComponent<MC_Component_Life>();
        Music_Component = GetComponent<MC_Component_Music>();
    }


    #endregion

    /// <summary>
    /// 持续闪烁
    /// </summary>
    /// <returns></returns>
    public override IEnumerator StartBuffEffect()
    {
        //提前返回-没有生命组件
        if (Life_Component == null)
            yield break;

        Music_Component.PlaySound(MusicData.TNT_Fuse);
        Life_Component.DynamicEntityColorLock(true);

        while (true)
        {
            //变白
            Life_Component.GetEntityMat().SetFloat("_Metallic", 0f);  
            yield return new WaitForSeconds(0.25f);

            //恢复
            Life_Component.GetEntityMat().SetFloat("_Metallic", 0.7f);
            yield return new WaitForSeconds(0.25f);
        }
    }

    /// <summary>
    /// 恢复一些变量
    /// </summary>
    public override void EndBuffEffect()
    {
        Life_Component.GetEntityMat().SetFloat("_Metallic", 0.7f);
        Life_Component.DynamicEntityColorLock(false);
    }
}
