using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeManager : MonoBehaviour
{
    [Header("状态")]
    [ReadOnly]public int blood = 20;   private int maxblood = 20;  //当前血量和最高血量
    [ReadOnly] public int food = 20;   private int maxfood = 20; 

    [Header("引用")]
    public ManagerHub managerhub;
    
    #region 周期函数

    public void InitLifeManager()
    {
        blood = 20;
        food = 20;

        UpdatePlayerBlood(0, false, false);
        UpdatePlayerFood(0, false);

        

        RecoveryCoroutine = null;
    }


    bool hasExec_Init = true;
    private void FixedUpdate()
    {
        if (managerhub.world.game_state == Game_State.Playing)
        {
            //if (Input.GetKeyDown(KeyCode.R))
            //{
            //    UpdatePlayerFood(1,true);
            //}


            //初始化血条
            if (hasExec_Init)
            {
                UpdatePlayerBlood(0, false, false);
                UpdatePlayerFood(0, false);
                hasExec_Init = false;
            }
            

            //常驻恢复协程
            if (RecoveryCoroutine == null)
            {
                RecoveryCoroutine = StartCoroutine(Recovery());
            }
        }
    }

    //自动恢复
    IEnumerator Recovery()
    {

        while (true)
        {
            if (managerhub.world.game_state == Game_State.Playing)
            {
                //自动恢复血条
                if (blood < maxblood && food >= 14)
                {
                    UpdatePlayerBlood(-recoverBlood, true, false);

                }

                //自动恢复饱食度
                //if (food != maxfood)
                //{
                //    UpdatePlayerFood(-recoverBlood, false);

                //}

                //饥饿
                if (food == 0 && blood >= 2)
                {
                    UpdatePlayerBlood(1, true, false);
                }

            }


            

            yield return new WaitForSeconds(recoverTime);
        }


    }

    #endregion


    #region 生命值系统

    [Header("生命值系统引用")]
    public Image[] Bloods = new Image[10];
    public Image[] BloodContainer = new Image[10];
    public Sprite heart_full;
    public Sprite heart_half;
    public Sprite oxygen_full;
    public Sprite oxygen_brust;


    //Container闪烁参数
    [Header("血条闪烁")]
    public int blink_numbet = 3;
    public float blink_time = 0.2f;
    public float duretime = 0.3f;

    //血量恢复
    [Header("血量恢复")]
    public float recoverTime = 5f;
    public int recoverBlood = 1;
    Coroutine RecoveryCoroutine;

    //更新血条
    public void UpdatePlayerBlood(int hurt, bool isBlind, bool isShakeHead)
    {
        //减去伤害
        blood -= hurt;

        //受伤效果
        if (isShakeHead)   //解决初始化尖叫的问题
        {
            managerhub.musicManager.PlaySound_fallGround();
            StartCoroutine(managerhub.player.Animation_Behurt());
        }

        

        //如果死亡
        if (blood <= 0)
        {
            blood = 0;

            //empty
            for (int i = 0; i < maxblood / 2; i++)
            {
                Bloods[i].color = Color.black;
            }

            managerhub.canvasManager.PlayerDead();
            return;
        }

        if (blood >= maxblood)
        {
            blood = maxblood;
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



    #endregion


    #region 饥饿值系统

    [Header("饥饿值系统引用")]
    public Image[] foods = new Image[10];
    public Image[] FoodsContainer = new Image[10];
    public Sprite food_full;
    public Sprite food_half;

    [Header("饥饿值系统参数")]
    public bool SprintLock = false;

    //更新饥饿条
    public void UpdatePlayerFood(int hurt, bool isBlind)
    {
        //减去
        food -= hurt;


        //出界判断
        if (food <= 0)
        {
            food = 0;
            //empty
            for (int i = 0; i < maxfood / 2; i++)
            {
                foods[i].color = Color.black;
            }

            return;
        }

        if (food > maxfood)
        {
            food = maxfood;
        }
         
        //奔跑锁
        if (food <= 6)
        {
            SprintLock = true;
        }
        else
        {
            SprintLock = false;
        }



        //血条为偶数
        if (food % 2 == 0)
        {
            //full
            for (int i = 0; i < food / 2; i++)
            {
                foods[i].sprite = food_full;
                foods[i].color = Color.white;
            }

            //empty
            for (int i = food / 2; i < maxfood / 2; i++)
            {
                foods[i].color = Color.black;
            }

        }
        //血条为奇数
        else
        {
            //full
            for (int i = 0; i < food / 2; i++)
            {
                foods[i].sprite = food_full;
                foods[i].color = Color.white;
            }

            //half
            foods[food / 2].sprite = food_half;
            foods[food / 2].color = Color.white;

            //empty
            for (int i = (food / 2) + 1; i < maxfood / 2; i++)
            {
                foods[i].color = Color.black;
            }
        }



        //闪烁血条
        if (isBlind)
        {
            StartCoroutine(Blink_foodcontainer());
        }

    }


    //饥饿条闪烁
    IEnumerator Blink_foodcontainer()
    {
        for (int i = 0; i < blink_numbet; i++)
        {
            foreach (Image item in FoodsContainer)
            {
                item.color = Color.white;

            }

            yield return new WaitForSeconds(blink_time);

            foreach (Image item in FoodsContainer)
            {
                item.color = Color.black;

            }

            yield return new WaitForSeconds(duretime);
        }
    }


    #endregion


    #region 氧气系统

    [Header("氧气系统")]
    public int oxygen = 10;
    private int oxygen_max = 10;
    public Image[] oxygen_sprites = new Image[10];

    Coroutine minus_oxy_Coroutine;
    Coroutine add_oxy_Coroutine;

    public float minusTime = 1f;
    public float brustTime = 0.5f;

    public float addTime = 0.2f;

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

            if (managerhub.world.game_state != Game_State.Playing && managerhub.world.game_state != Game_State.Pause)
            {
                break;
            }

            UpdatePlayerBlood(2, true, true);
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

    #endregion


}
