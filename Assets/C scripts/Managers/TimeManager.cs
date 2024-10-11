using Homebrew;
using System;
using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{

    #region 状态

    [Foldout("状态", true)]
    [Header("当前时间(24小时制)")][Range(0, 24), SerializeField] private float CurrentTime = 12;
    [Header("是否是晚上")][ReadOnly]public bool isNight;

    #endregion


    #region 周期函数

    private ManagerHub managerhub;

    private void Start()
    {
        managerhub = VoxelData.GetManagerhub();
    }

    #endregion


    [Foldout("时间参数", true)]
    [Header("游戏内一小时对应现实多少秒")] public float second_GameOneHour = 60; // 现实中多少秒过完游戏内一小时

    [Foldout("天空过渡参数", true)]
    [Header("白天过渡范围")] public Vector2 天开始变黑;
    [Header("晚上过渡范围")] public Vector2 天开始变亮;
    private float previous_CurrentTime = 12;
    private float value;

    [Foldout("迷雾过渡参数", true)]
    [Header("迷雾白天颜色")] public Color FogDayColor;
    [Header("迷雾晚上颜色")] public Color FogNightColor;
    [Header("迷雾过渡时间")] public float fogTransitionTime = 3f;  // 迷雾过渡时间
    [Header("迷雾白天范围")] public Vector2 FogDayDistance;  // 迷雾距离
    [Header("迷雾矿洞范围")] public Vector2 FogCaveDistance;  // 迷雾距离


    [Foldout("天空盒过渡参数", true)]
    [Header("天空盒引用")] public Material SkyboxMaterial;
    [Header("天空盒过渡范围")] public Vector2 SkyboxRange = new Vector2(0.2f, 1f);


    [Foldout("地形颜色过渡参数", true)]
    [Header("地形材质引用")] public Material BlocksMaterial;
    [Header("白天颜色")] public Color BlocksDayColor;
    [Header("晚上颜色")] public Color BlocksNightColor;



    // 初始化
    public void InitTimeManager()
    {
        CurrentTime = 12;
    }

    //设置系统时间
    public void SetTime(float _time)
    {
        CurrentTime = _time;
    }

    //玩家视野变黑
    public void Buff_CaveFog(bool _Open)
    {
        if (TransitionFogCoroutine == null)
        {
            TransitionFogCoroutine = StartCoroutine(TransitionFog(_Open));


        }

    }

    //根据渲染范围更新迷雾距离
    public void UpdateDayFogDistance()
    {
        float _renderSize = managerhub.canvasManager.world.renderSize;
        float _t = _renderSize * 10 - 10f;


        FogDayDistance = new Vector2(_t / 4, _t);

        RenderSettings.fogStartDistance = FogDayDistance.x;
        RenderSettings.fogEndDistance = FogDayDistance.y;
    }


    public float GetCurrentTime()
    {
        return CurrentTime;
    }


    //------------------------------------------------------------------------------






    //---------------------------------次要函数-------------------------------------

    private bool haeExec_Update = true;
    private void Update()
    {
        // 启动条件
        if (managerhub.world.game_state == Game_State.Playing)
        {
            if (TimeCoroutine == null)
            {
                TimeCoroutine = StartCoroutine(UpdateTime());
            }

            if (haeExec_Update)
            {
                UpdateDayFogDistance();

                haeExec_Update = false;
            }

            if (CurrentTime <= 6 || CurrentTime >= 18)
            {
                isNight = true;
            }
            else
            {
                isNight = false;
            }


            if (isOpenBlackFog == false && managerhub.player.isInCave)
            {
                Buff_CaveFog(true);
                isOpenBlackFog = true;
            }
            else if(!isNight && isOpenBlackFog == true && !managerhub.player.isInCave)
            {
                Buff_CaveFog(false);
                isOpenBlackFog = false;
            }

        }
    }


    private void OnApplicationQuit()
    {
        BlocksMaterial.color = BlocksDayColor;
        SkyboxMaterial.SetFloat("_Exposure", 1);
    }

    //------------------------------------------------------------------------------






    //---------------------------------协程-------------------------------------

    //黑色迷雾协程
    private bool isOpenBlackFog = false;
    private Coroutine TransitionFogCoroutine;
    private IEnumerator TransitionFog(bool enterCave)
    {
        float elapsedTime = 0f;
        Color startColor = RenderSettings.fogColor;
        Vector2 startDistance = new Vector2(RenderSettings.fogStartDistance, RenderSettings.fogEndDistance);

        Color targetColor = enterCave ? FogNightColor : FogDayColor;
        Vector2 targetDistance = enterCave ? FogCaveDistance : FogDayDistance;

        // 获取 BlocksMaterial 的初始颜色和目标颜色
        Color startBlockColor = enterCave ? BlocksDayColor : BlocksNightColor;
        Color targetBlockColor = enterCave ? BlocksNightColor : BlocksDayColor;

        while (elapsedTime < fogTransitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fogTransitionTime;

            // Lerp 颜色和距离
            RenderSettings.fogColor = Color.Lerp(startColor, targetColor, t);
            RenderSettings.fogStartDistance = Mathf.Lerp(startDistance.x, targetDistance.x, t);
            RenderSettings.fogEndDistance = Mathf.Lerp(startDistance.y, targetDistance.y, t);

            // Lerp BlocksMaterial 的颜色
            BlocksMaterial.color = Color.Lerp(startBlockColor, targetBlockColor, t);

            yield return null;
        }

        // 确保最终值正确
        RenderSettings.fogColor = targetColor;
        RenderSettings.fogStartDistance = targetDistance.x;
        RenderSettings.fogEndDistance = targetDistance.y;
        BlocksMaterial.color = targetBlockColor; // 确保最终颜色正确
        TransitionFogCoroutine = null;
    }

    // 时间流逝协程
    private Coroutine TimeCoroutine; //24小时制的时间
    private IEnumerator UpdateTime()
    {
        while (true)
        {
            // 等待一帧
            yield return null;

            // 计算每帧应该增加的游戏时间
            float elapsedTime = Time.deltaTime / second_GameOneHour;

            // 增加游戏时间
            CurrentTime += elapsedTime;

            // 确保时间在 0-24 小时之间循环
            if (CurrentTime >= 24)
            {
                CurrentTime -= 24;
            }

            //----------------------------------------固定顺序


            //如果值中途出现大幅度篡改，则立即调整值
            if (Mathf.Abs(CurrentTime - previous_CurrentTime) > 1)
            {
                if (!isNight)
                {
                    value = 1;
                }
                else
                {
                    value = 0;
                }

                SkyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(SkyboxRange.x, SkyboxRange.y, value));

                if (managerhub.player.isInCave == false)
                {
                    RenderSettings.fogColor = Color.Lerp(FogNightColor, FogDayColor, value);
                }

                BlocksMaterial.color = Color.Lerp(BlocksNightColor, BlocksDayColor, value);
            }
            previous_CurrentTime = CurrentTime;

            //----------------------------------------

            // 开始过渡
            if ((CurrentTime >= 天开始变亮.x && CurrentTime <= 天开始变亮.y) || (CurrentTime >= 天开始变黑.x && CurrentTime <= 天开始变黑.y))
            {
                //Value百分值
                if (CurrentTime < 12) 
                {
                    value = Mathf.InverseLerp(天开始变亮.x, 天开始变亮.y, CurrentTime);
                }
                else
                {
                    value = 1 - Mathf.InverseLerp(天开始变黑.x, 天开始变黑.y, CurrentTime);
                }

                //改变 
                SkyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(SkyboxRange.x, SkyboxRange.y, value));

                if (managerhub.player.isInCave == false)
                {
                    RenderSettings.fogColor = Color.Lerp(FogNightColor, FogDayColor, value);
                }
                
                BlocksMaterial.color = Color.Lerp(BlocksNightColor, BlocksDayColor, value);
            }

            

            // 关闭条件
            if (managerhub.world.game_state == Game_State.Start)
            {
                TimeCoroutine = null;
                InitTimeManager();
                yield break;
            }
        }
    }

    //------------------------------------------------------------------------------

}
