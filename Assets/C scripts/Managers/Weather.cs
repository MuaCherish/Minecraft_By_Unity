using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Enum_Weather
{
    Sunny,
    Rainy
}

public class Weather : MonoBehaviour
{

    #region 状态

    [Foldout("状态", true)]
    [Header("当前天气")] [ReadOnly] public Enum_Weather weather;
    [Header("是否正在天黑")][ReadOnly] public bool isTransitioningToRain = false; // 标记是否正在过渡到下雨
    [Header("是否正在下雨")] [ReadOnly] public bool isRaining = false; // 标记是否正在下雨

    #endregion 


    #region 周期函数

    ManagerHub managerhub;
    Player player;
    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        player = managerhub.player;
    }

    private void Update()
    {
        switch (managerhub.world.game_state)
        {
            case Game_State.Playing:
                CheckWeatherTransition();
                HandleCloudyWeather();
                break;
        }

        
    }





    #endregion


    #region 天气转换概率

    [Foldout("Transform", true)]
    [Header("RainObject")] public GameObject RainObject;

    [Foldout("天气系统", true)]
    [Header("天气检查间隔")] public float weatherCheckInterval = 10f; // 检查天气的时间间隔                             
   

    // 随机改变天气为Cloudy的间隔时间和概率


    float lastWeatherCheckTime;
    float rainDuration = 60f; // 雨持续时间
    //阴天检测
    void CheckWeatherTransition()
    {
        // 如果已经是阴天或者冷却时间未结束，直接返回
        if (weather == Enum_Weather.Rainy)
            return;

        if (Time.time - lastWeatherCheckTime > weatherCheckInterval)
        {
            lastWeatherCheckTime = Time.time;

            if (weather == Enum_Weather.Sunny && Random.value < weather_Cloudy_robability)
            {
                SetWeatherRainy();
            }
        }
    }

   

    #endregion


    #region 阴天

    [Foldout("阴天", true)]
    [Header("阴天概率")][Range(0, 1)] public float weather_Cloudy_robability = 0.3f; // 30%的概率变成Cloudy
    [Header("过渡时长")] public float transitionDuration = 5f; // 时间值过渡的总时长
    [Header("下雨持续时间范围")] public Vector2 Range_rainDuration = new Vector2(10f, 30f); // 雨持续时间
    [Header("头顶射线检测")] public float checkHeadTime = 1f;
    private float HeadTime_elapsedTime = 0f; // 用于计时


    /// <summary>
    /// 将天气转为阴天
    /// </summary>
    public void SetWeatherRainy()
    {
        weather = Enum_Weather.Rainy;
        rainDuration = Random.Range(Range_rainDuration.x, Range_rainDuration.y); // 随机设置雨持续时间
        haExec_StartToRain = true;
    }

    public void SetWeatherRainy(float _transitionDuration, float _RainDutation)
    {
        weather = Enum_Weather.Rainy;
        rainDuration = _RainDutation; // 随机设置雨持续时间
        transitionDuration = _transitionDuration;
        haExec_StartToRain = true;
    }

    /// <summary>
    /// 处理Cloudy天气的逻辑：时间值逐渐归零、下雨，雨结束后天气恢复
    /// </summary>
    float elapsedTime; // 累计的过渡时间
    //float initialTimeValue; // 开始过渡时的时间值
    //float endTimeValue; // 结束过渡时的时间值
    bool haExec_StartToRain = false;

    void HandleCloudyWeather()
    {
        // 不是下雨天，直接返回
        if (weather != Enum_Weather.Rainy)
            return;

        // 处理白天到黑夜的过渡
        if (haExec_StartToRain)
        {
            isTransitioningToRain = true;
            elapsedTime = 0f;
            haExec_StartToRain = false;
            managerhub.timeManager.SwitchWeatherState(TimeData.Rain, transitionDuration, 1);
            return;
        }

        // 处理黑夜到下雨的过渡
        if (isTransitioningToRain)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= transitionDuration)
            {
                isTransitioningToRain = false;
                StartRain();
            }
            return;
        }

        // 处理下雨过程
        if (isRaining)
        {
            CheckPlayerHead();
            rainDuration -= Time.deltaTime;
            if (rainDuration <= 0)
            {
                isRaining = false;
                elapsedTime = 0f;
            }
            return;
        }

        // 处理雨停后的天亮过程
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= transitionDuration)
        {
            weather = Enum_Weather.Sunny;
            RainObject.GetComponent<Rain>().StopRain();
            managerhub.timeManager.FromGameTimeToUpdateAll_Smooth(transitionDuration);
            managerhub.NewmusicManager.BackGroundTime();
        }
    }

    void CheckPlayerHead()
    {
        elapsedTime += Time.deltaTime;

        // 每隔 checkHeadTime 触发一次
        if (elapsedTime >= checkHeadTime)
        {
            elapsedTime = 0f; // 重置计时器

            // 执行射线检测逻辑
            RayCastStruct _rayCast = player.NewRayCast(player.transform.position, Vector3.up, 32f);
            if (_rayCast.isHit == 1)
            {
                // Do something when head is detected
                //print("玩家在屋子里");
                managerhub.NewmusicManager.SetVolumn_Smooth(0.2f, 0.2f);
            }
            else if(_rayCast.isHit == 0)
            {
                managerhub.NewmusicManager.SetVolumn_Smooth(0.5f, 0.2f);
            }
        }
    }

    // 开始下雨
    void StartRain()
    {
        isRaining = true;
        RainObject.SetActive(true);
        RainObject.GetComponent<Rain>().StartRain();
        managerhub.NewmusicManager.SwitchBackgroundMusic(MusicData.Rain, 1f, 0.5f);

    }

    #endregion



}
