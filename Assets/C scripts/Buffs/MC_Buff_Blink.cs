using MCEntity;
using System.Collections;
using UnityEngine;

public class MC_Buff_Blink : MC_Buff_Base
{
    MC_Life_Component Life_Component;
   
    private void Awake()
    {
        Life_Component = GetComponent<MC_Life_Component>();
    }

    public override IEnumerator StartBuffEffect()
    {
        //��ǰ����-û���������
        if (Life_Component == null)
            yield break;

        while(true)
        {
            //���
            Life_Component.UpdateEntityColor(Life_Component.Color_Blink);
            yield return new WaitForSeconds(0.25f);

            //�ָ�
            Life_Component.ResetEntityColor();
            yield return new WaitForSeconds(0.25f);
        }
    }
        
    public override IEnumerator StartBuffEffect(float[] _floatList)
    {
        print("MC_Buff_Blink");
        yield return null;
    }
}
