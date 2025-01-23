using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Enum_Weather
{
    Sunshine,
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
    TimeManager timemanager;
    private void Awake()
    {
        managerhub = GlobalData.GetManagerhub();
        timemanager = managerhub.timeManager;
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


    #region 天气系统


    [Foldout("Transform", true)]
    [Header("RainObject")] public GameObject RainObject;

    [Foldout("天气系统", true)]
    [Header("天气检查间隔")] public float weatherCheckInterval = 10f; // 检查天气的时间间隔
    [Header("阴天概率")] [Range(0, 1)] public float weather_Cloudy_robability = 0.3f; // 30%的概率变成Cloudy
    [Header("过渡时长")] public float transitionDuration = 5f; // 时间值过渡的总时长
    [Header("下雨持续时间范围")] public Vector2 Range_rainDuration = new Vector2(10f, 30f); // 雨持续时间


    // 随机改变天气为Cloudy的间隔时间和概率

    
    float lastWeatherCheckTime;
    float rainDuration = 60f; // 雨持续时间
    //阴天检测
    void CheckWeatherTransition()
    {
        //提前返回-如果已经是阴天 
        if (weather == Enum_Weather.Rainy)
            return;


        if (Time.time - lastWeatherCheckTime > weatherCheckInterval)
        {
            lastWeatherCheckTime = Time.time;

            if (weather == Enum_Weather.Sunshine && Random.value < weather_Cloudy_robability)
            {
                weather = Enum_Weather.Rainy;
                rainDuration = Random.Range(Range_rainDuration.x, Range_rainDuration.y); // 随机设置雨持续时间
                haExec_StartToRain = true;
            }
        }
    }

    /// <summary>
    /// 处理Cloudy天气的逻辑：时间值逐渐归零、下雨，雨结束后天气恢复
    /// </summary>
    float elapsedTime; // 累计的过渡时间
    float initialTimeValue; // 开始过渡时的时间值
    float endTimeValue; // 结束过渡时的时间值
    bool haExec_StartToRain = false;

    //阴天前摇
    void HandleCloudyWeather()
    {
        //提前返回-不是阴天
        if (weather != Enum_Weather.Rainy)
            return;

        //白天 -> 天黑
        if (haExec_StartToRain)
        {
            //print("true");
            isTransitioningToRain = true;
            elapsedTime = 0f;
            initialTimeValue = timemanager.timeStruct._time.value;
            haExec_StartToRain = false;
        }

        //天黑
        if (isTransitioningToRain) 
        {
            elapsedTime += Time.deltaTime;
            timemanager.timeStruct._time.value = Mathf.Lerp(initialTimeValue, 0, elapsedTime / transitionDuration);
            timemanager.UpdateAll();

            if (elapsedTime >= transitionDuration)
            {
                // 时间过渡完成，开始下雨
                isTransitioningToRain = false;
                StartRain();
            }
        }

        //下雨
        else if (isRaining)
        {
            rainDuration -= Time.deltaTime;
            if (rainDuration <= 0)
            {
                // 雨停，开始恢复时间值
                isRaining = false;
                elapsedTime = 0f;
                endTimeValue = timemanager.Get_Value(timemanager.GetCurrentTime());
            }
        }

        //天亮
        else
        {
            // 雨停后恢复时间值
            //print(elapsedTime);
            elapsedTime += Time.deltaTime;
            timemanager.timeStruct._time.value = Mathf.Lerp(0, endTimeValue, elapsedTime / transitionDuration);
            timemanager.UpdateAll();

            if (elapsedTime >= transitionDuration)
            {
                // 时间值恢复完成，切换回晴天
                weather = Enum_Weather.Sunshine;
                RainObject.GetComponent<Rain>().StopRain();
            }
        }
    }

    // 开始下雨
    void StartRain()
    {
        isRaining = true;
        RainObject.SetActive(true);
        RainObject.GetComponent<Rain>().StartRain();
        managerhub.NewmusicManager.SwitchBackgroundMusic(MusicData.Rain);
    }


    #endregion
}
