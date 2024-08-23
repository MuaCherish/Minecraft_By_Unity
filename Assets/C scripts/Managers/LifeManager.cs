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
    public World world;
    public MusicManager musicmanager;
    public Player player;
    public CanvasManager canvasManager;
    public Image[] Bloods = new Image[10];
    public Image[] BloodContainer = new Image[10];
    public Sprite heart_full;
    public Sprite heart_half;
    public Sprite oxygen_full;
    public Sprite oxygen_brust;

    //Container��˸����
    [Header("Blink container")]
    public int blink_numbet = 3;
    public float blink_time = 0.2f;
    public float duretime = 0.3f;

    //Ѫ���ָ�
    [Header("Recover")]
    public float recoverTime = 5f;
    public int recoverBlood = 1;
    Coroutine RecoveryCoroutine;

    //һ���Դ���
    //bool hasExec_PlaySound = true;
    

    //------------------------------------ Ѫ�� ------------------------------------------

    //��ʼ��Ѫ��
    private void Start()
    {
        //��ʼ��Ѫ��
        UpdatePlayerBlood(0, false, false);

        //��פ�ָ�Э��
        if (RecoveryCoroutine == null)
        {
            RecoveryCoroutine = StartCoroutine(RecoveryBlood());
        }
    }

    //����Ѫ��
    public void UpdatePlayerBlood(int hurt, bool isBlind, bool isShakeHead)
    {
        //��ȥ�˺�
        blood -= hurt;

        //����Ч��
        if (isShakeHead)   //�����ʼ����е�����
        {
            musicmanager.PlaySound_fallGround();
            StartCoroutine(player.Animation_Behurt());
        }

        

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
    
    //�Զ��ָ�Ѫ��
    IEnumerator RecoveryBlood() 
    {

        while (true)
        {
            if (blood != maxblood && world.game_state == Game_State.Playing)
            {
                UpdatePlayerBlood(-recoverBlood, true, false);

            }
            yield return new WaitForSeconds(recoverTime);
        }
        

    }

    //-------------------------------------------------------------------------------------




    public int oxygen = 10;
    private int oxygen_max = 10;
    public Image[] oxygen_sprites = new Image[10];

    Coroutine minus_oxy_Coroutine;
    Coroutine add_oxy_Coroutine;

    public float minusTime = 1f;
    public float brustTime = 0.5f;

    public float addTime = 0.2f;

    //------------------------------------ ���� -------------------------------------------
    //��ˮ
    public void Oxy_IntoWater()
    {
        
        if (minus_oxy_Coroutine == null)
        {
            //��������ͼƬ,��ʾ����
            for (int i = 0; i < oxygen; i++)
            {
                oxygen_sprites[i].sprite = oxygen_full;
                oxygen_sprites[i].color = new Color(1, 1, 1, 1);
            }

            //��ʼ��������
            if (add_oxy_Coroutine != null)
            {
                StopCoroutine(add_oxy_Coroutine);
                add_oxy_Coroutine = null;
            }
            
            minus_oxy_Coroutine = StartCoroutine(minus_oxy());
        }


    }

    IEnumerator minus_oxy()
    {
        //��ʾ���۳�����ֵ
        for (int i = oxygen - 1; i >= 0; i--)
        {
            yield return new WaitForSeconds(minusTime);

            oxygen_sprites[i].sprite = oxygen_brust;

            yield return new WaitForSeconds(brustTime);

            oxygen_sprites[i].color = new Color(1, 1, 1, 0);

            oxygen--;
        }

        //����ֵ���꿪ʼ��Ѫ
        while (true)
        {

            if (world.game_state != Game_State.Playing && world.game_state != Game_State.Pause)
            {
                break;
            }

            UpdatePlayerBlood(2, true, true);
            yield return new WaitForSeconds(minusTime);
        }

    }

    //��ˮ
    public void Oxy_OutWater()
    {
        //��ͣ��������Э��,��ʼ��������
       if (add_oxy_Coroutine == null)
        {
            StopCoroutine(minus_oxy_Coroutine);
            minus_oxy_Coroutine = null;
            add_oxy_Coroutine = StartCoroutine(add_oxy());
        }


        
    }

    IEnumerator add_oxy()
    {
        //��������ֵ
        for (int i = oxygen; i < oxygen_max; i ++)
        {
            //����
            oxygen++;
            oxygen_sprites[i].sprite = oxygen_full;
            oxygen_sprites[i].color = new Color(1,1,1,1f);

            //��һ��
            yield return new WaitForSeconds(addTime);
        }

        //��������ֵ
        for (int i = 0;i < oxygen_max; i ++)
        {
            oxygen_sprites[i].color = new Color(1,1,1,0f);
        }
    }

    //-------------------------------------------------------------------------------------



}
