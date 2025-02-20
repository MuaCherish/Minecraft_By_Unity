using MCEntity;
using System.Collections;
using UnityEngine;

public class MC_Buff_Blink : MC_Buff_Base
{

    #region ���ں���

    MC_Life_Component Life_Component;
    MC_Music_Component Music_Component;
   
    private void Awake()
    {
        Life_Component = GetComponent<MC_Life_Component>();
        Music_Component = GetComponent<MC_Music_Component>();
    }


    #endregion

    /// <summary>
    /// ������˸
    /// </summary>
    /// <returns></returns>
    public override IEnumerator StartBuffEffect()
    {
        //��ǰ����-û���������
        if (Life_Component == null)
            yield break;

        Music_Component.PlaySound(MusicData.TNT_Fuse);
        Life_Component.DynamicEntityColorLock(true);

        while (true)
        {
            //���
            Life_Component.GetEntityMat().SetFloat("_Metallic", 0f);  
            yield return new WaitForSeconds(0.25f);

            //�ָ�
            Life_Component.GetEntityMat().SetFloat("_Metallic", 0.7f);
            yield return new WaitForSeconds(0.25f);
        }
    }

    /// <summary>
    /// �ָ�һЩ����
    /// </summary>
    public override void EndBuffEffect()
    {
        Life_Component.GetEntityMat().SetFloat("_Metallic", 0.7f);
        Life_Component.DynamicEntityColorLock(false);
    }
}
