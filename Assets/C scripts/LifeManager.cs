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

    //Container闪烁参数
    [Header("Blink container")]
    public int blink_numbet = 3;
    public float blink_time = 0.2f;
    public float duretime = 0.3f;



    //------------------------------------ 血条 ------------------------------------------

    //初始化血条
    private void Start()
    {
        UpdatePlayerBlood(0, false);
    }

    //更新血条
    public void UpdatePlayerBlood(int hurt, bool isBlind)
    {
        //减去伤害
        blood -= hurt;

        //受伤效果
        musicmanager.PlaySound_fallGround();
        StartCoroutine(player.Animation_Behurt());

        //如果死亡
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

        //血条为偶数
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
        //血条为奇数
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



        //闪烁血条
        if (isBlind)
        {
            StartCoroutine(Blink_container());
        }
        
    }

    //血条闪烁
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

    //-------------------------------------------------------------------------------------




    public int oxygen = 10;
    private int oxygen_max = 10;
    public Image[] oxygen_sprites = new Image[10];

    Coroutine minus_oxy_Coroutine;
    Coroutine add_oxy_Coroutine;

    public float minusTime = 1f;
    public float brustTime = 0.5f;

    public float addTime = 0.2f;

    //------------------------------------ 氧气 -------------------------------------------
    //入水
    public void Oxy_IntoWater()
    {
        
        if (minus_oxy_Coroutine == null)
        {
            //更换氧气图片,显示氧气
            for (int i = 0; i < oxygen; i++)
            {
                oxygen_sprites[i].sprite = oxygen_full;
                oxygen_sprites[i].color = new Color(1, 1, 1, 1);
            }

            //开始消耗氧气
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
        //显示并扣除氧气值
        for (int i = oxygen - 1; i >= 0; i--)
        {
            yield return new WaitForSeconds(minusTime);

            oxygen_sprites[i].sprite = oxygen_brust;

            yield return new WaitForSeconds(brustTime);

            oxygen_sprites[i].color = new Color(1, 1, 1, 0);

            oxygen--;
        }

        //氧气值掉完开始扣血
        while (true)
        {

            if (world.game_state == Game_State.Dead)
            {
                break;
            }

            UpdatePlayerBlood(2, true);
            yield return new WaitForSeconds(minusTime);
        }

    }

    //出水
    public void Oxy_OutWater()
    {
        //暂停消耗氧气协程,开始补充氧气
       if (add_oxy_Coroutine == null)
        {
            StopCoroutine(minus_oxy_Coroutine);
            minus_oxy_Coroutine = null;
            add_oxy_Coroutine = StartCoroutine(add_oxy());
        }


        
    }

    IEnumerator add_oxy()
    {
        //补充氧气值
        for (int i = oxygen; i < oxygen_max; i ++)
        {
            //补充
            oxygen++;
            oxygen_sprites[i].sprite = oxygen_full;
            oxygen_sprites[i].color = new Color(1,1,1,1f);

            //等一会
            yield return new WaitForSeconds(addTime);
        }

        //隐藏氧气值
        for (int i = 0;i < oxygen_max; i ++)
        {
            oxygen_sprites[i].color = new Color(1,1,1,0f);
        }
    }

    //-------------------------------------------------------------------------------------



}
