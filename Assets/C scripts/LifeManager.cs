using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeManager : MonoBehaviour
{
    [Header("Player State")]
    public int blood = 20;
    private int maxblood = 20;

    [Header("Transforms")]
    public CanvasManager canvasManager;
    public Image[] Bloods = new Image[10];
    public Image[] BloodContainer = new Image[10];
    public Sprite heart_full;
    public Sprite heart_half;

    //Container��˸����
    [Header("Blink container")]
    public int blink_numbet = 3;
    public float blink_time = 0.2f;
    public float duretime = 0.3f;

    //��ʼ��Ѫ��
    private void Start()
    {
        UpdatePlayerBlood(0, false);
    }

    //����Ѫ��
    public void UpdatePlayerBlood(int hurt, bool isBlind)
    {
        //��ȥ�˺�
        blood -= hurt;

        //�������
        if (blood <= 0)
        {

            //empty
            for (int i = 0; i < maxblood / 2; i++)
            {
                Bloods[i].color = Color.black;
            }

            canvasManager.PlayerDead();
            return;
        }

        //Ѫ��Ϊż��
        if (blood % 2 == 0)
        {
            //full
            for (int i = 0; i < blood / 2; i++)
            {
                Bloods[i].sprite = heart_full;
                Bloods[i].color = Color.white;
            }

            //empty
            for (int i = blood / 2; i < maxblood / 2; i++)
            {
                Bloods[i].color = Color.black;
            }

        }
        //Ѫ��Ϊ����
        else
        {
            //full
            for (int i = 0; i < blood / 2; i++)
            {
                Bloods[i].sprite = heart_full;
                Bloods[i].color = Color.white;
            }

            //half
            Bloods[blood / 2].sprite = heart_half;
            Bloods[blood / 2].color = Color.white;

            //empty
            for (int i = (blood / 2) + 1; i < maxblood / 2; i++)
            {
                Bloods[i].color = Color.black; 
            }
        }



        //��˸Ѫ��
        if (isBlind)
        {
            StartCoroutine(Blink_container());
        }
        
    }

    //Ѫ����˸
    IEnumerator Blink_container()
    {
        for (int i = 0;i < blink_numbet;i ++)
        {
            foreach (Image item in BloodContainer)
            {
                item.color = Color.white;
                
            }

            yield return new WaitForSeconds(blink_time);

            foreach (Image item in BloodContainer)
            {
                item.color = Color.black;

            }

            yield return new WaitForSeconds(duretime);
        }
    }

}
