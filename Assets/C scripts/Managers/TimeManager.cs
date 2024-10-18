using Homebrew;
using System.Collections;
using UnityEngine;
using Cloud;

public enum Enum_Weather
{
    Sunshine = 0,
    Cloudy = 1,
}

public class TimeManager : MonoBehaviour
{

    #region 状态

    [Foldout("状态", true)]
    //[Header("当前时间(24小时制)")][Range(0, 24), SerializeField] private float CurrentTime = 12;
    [Header("是否是晚上")][ReadOnly] public bool isNight;

    #endregion


    #region 周期函数

    private ManagerHub managerhub;
    private bool haeExec_Update = true;


    private void Start()
    {
        managerhub = GlobalData.GetManagerhub();
        InitTimeManager();


        WeatherCheck();

        

    }

    


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


            DynamicState_isNight();


            DynamicSwitchCaveMode();
        }
        else
        {
            if (haeExec_Update == false)
            {
                haeExec_Update = true;
            }
            
        }
    }


    private void OnApplicationQuit()
    {
        timeStruct._Terrain.BlocksMaterial.color = timeStruct._Terrain.BlocksDayColor;
        //timeStruct._Skybox.SkyboxMaterial.SetFloat("_Exposure", 1);
        //timeStruct._Skybox.SkyboxMaterial.color = timeStruct._fog.FogDayColor;
        timeStruct._fog.SkyboxMaterial.SetColor("_Tint", timeStruct._fog.FogDayColor);
    }


    #endregion


    #region 状态判断

    void DynamicState_isNight()
    {
        if (timeStruct._time.CurrentTime <= 6 || timeStruct._time.CurrentTime >= 18)
        {
            isNight = true;
        }
        else
        {
            isNight = false;
        }
    }

    #endregion


    #region 外界调用

    
    public void InitTimeManager()
    {
        timeStruct._time.CurrentTime = Random.Range(0f, 24f);

        if (isRandomWeather)
        {
            weather = (Enum_Weather)Random.Range(0, System.Enum.GetValues(typeof(Enum_Weather)).Length);
        }
        
    }

    public float GetCurrentTime()
    {
        return timeStruct._time.CurrentTime;
    }

    //设置系统时间
    public void SetTime(float _time)
    {
        timeStruct._time.CurrentTime = _time;
    }

    #endregion


    #region 时间流逝

    [Foldout("时间流逝", true)]
    public TimeManagertruct timeStruct;
    public WeatherStruct[] weatherLists;

    // 时间流逝协程
    private Coroutine TimeCoroutine; //24小时制的时间
    private IEnumerator UpdateTime()
    {
        while (true)
        {
            // 等待一帧
            yield return null;

            // 计算每帧应该增加的游戏时间
            float elapsedTime = Time.deltaTime / timeStruct._time.second_GameOneHour;

            // 增加游戏时间
            timeStruct._time.CurrentTime += elapsedTime;

            // 确保时间在 0-24 小时之间循环
            if (timeStruct._time.CurrentTime >= 24)
            {
                timeStruct._time.CurrentTime -= 24;
            }

            //----------------------------------------固定顺序


            //如果值中途出现大幅度篡改，则立即调整值
            if (Mathf.Abs(timeStruct._time.CurrentTime - timeStruct._time.previous_CurrentTime) > 1)
            {
                if (!isNight)
                {
                    timeStruct._time.value = 1;
                }
                else
                {
                    timeStruct._time.value = 0;
                }
                Color LerpFogColor = Color.Lerp(timeStruct._fog.FogNightColor, timeStruct._fog.FogDayColor, timeStruct._time.value);
                //timeStruct._Skybox.SkyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(timeStruct._Skybox.SkyboxRange.x, timeStruct._Skybox.SkyboxRange.y, timeStruct._time.value));
                //timeStruct._Skybox.SkyboxMaterial.color = LerpFogColor;
                timeStruct._fog.SkyboxMaterial.SetColor("_Tint", LerpFogColor);

                if (managerhub.player.isInCave == false)
                {
                    RenderSettings.fogColor = Color.Lerp(timeStruct._fog.FogNightColor, timeStruct._fog.FogDayColor, timeStruct._time.value);
                }

                timeStruct._Terrain.BlocksMaterial.color = Color.Lerp(timeStruct._Terrain.BlocksNightColor, timeStruct._Terrain.BlocksDayColor, timeStruct._time.value);
            }

            Color LerpCloudColor = Color.Lerp(timeStruct._cloud.CloudNightColor, timeStruct._cloud.CloudDayColor, timeStruct._time.value);
            managerhub.cloudManager.SetCloudColor(LerpCloudColor);

            timeStruct._time.previous_CurrentTime = timeStruct._time.CurrentTime;

            //----------------------------------------

            // 开始过渡
            if ((timeStruct._time.CurrentTime >= timeStruct._time.天开始变亮.x && timeStruct._time.CurrentTime <= timeStruct._time.天开始变亮.y) || (timeStruct._time.CurrentTime >= timeStruct._time.天开始变黑.x && timeStruct._time.CurrentTime <= timeStruct._time.天开始变黑.y))
            {
                //Value百分值
                if (timeStruct._time.CurrentTime < 12)
                {
                    timeStruct._time.value = Mathf.InverseLerp(timeStruct._time.天开始变亮.x, timeStruct._time.天开始变亮.y, timeStruct._time.CurrentTime);
                }
                else
                {
                    timeStruct._time.value = 1 - Mathf.InverseLerp(timeStruct._time.天开始变黑.x, timeStruct._time.天开始变黑.y, timeStruct._time.CurrentTime);
                }

                Color LerpFogColor = Color.Lerp(timeStruct._fog.FogNightColor, timeStruct._fog.FogDayColor, timeStruct._time.value);

                //改变 
                //timeStruct._Skybox.SkyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(timeStruct._Skybox.SkyboxRange.x, timeStruct._Skybox.SkyboxRange.y, timeStruct._time.value));
                timeStruct._fog.SkyboxMaterial.SetColor("_Tint", LerpFogColor);

                if (managerhub.player.isInCave == false)
                {
                    RenderSettings.fogColor = LerpFogColor;
                }

                LerpCloudColor = Color.Lerp(timeStruct._cloud.CloudNightColor, timeStruct._cloud.CloudDayColor, timeStruct._time.value);
                managerhub.cloudManager.SetCloudColor(LerpCloudColor);


                timeStruct._Terrain.BlocksMaterial.color = Color.Lerp(timeStruct._Terrain.BlocksNightColor, timeStruct._Terrain.BlocksDayColor, timeStruct._time.value);
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


    //根据渲染范围更新迷雾距离
    public void UpdateDayFogDistance()
    {
        float _renderSize = managerhub.canvasManager.world.renderSize;
        float _t = _renderSize * 10 - 10f;


        timeStruct._fog.FogDayDistance = new Vector2(_t * 0.8f, _t);

        RenderSettings.fogStartDistance = timeStruct._fog.FogDayDistance.x;
        RenderSettings.fogEndDistance = timeStruct._fog.FogDayDistance.y;
    }

    #endregion


    #region 天气

    [Foldout("天气参数", true)]
    [Header("随机天气")] public bool isRandomWeather = true;
    [Header("天气")] public Enum_Weather weather = Enum_Weather.Sunshine;
    void WeatherCheck()
    {
        

        switch (weather)
        {
            case Enum_Weather.Sunshine:
                timeStruct._fog.FogDayColor = weatherLists[0].FogDayColor;
                timeStruct._fog.FogNightColor = weatherLists[0].FogNightColor;
                break;

            case Enum_Weather.Cloudy:
                managerhub.cloudManager.enabled = false;
                timeStruct._fog.FogDayColor = weatherLists[1].FogDayColor;
                timeStruct._fog.FogNightColor = weatherLists[1].FogNightColor;
                break;
        }
    }


    #endregion


    #region Buff_洞穴模式

    [Foldout("Buff_洞穴模式", true)]
    [Header("洞穴模式过渡时间")] public float fogTransitionTime = 3f;  // 迷雾过渡时间

    //黑色迷雾协程
    private bool isOpenBlackFog = false;
    private Coroutine TransitionFogCoroutine;


    public void DynamicSwitchCaveMode()
    {
        if (isOpenBlackFog == false && managerhub.player.isInCave)
        {
            Buff_CaveFog(true);
            isOpenBlackFog = true;
        }
        else if (!isNight && isOpenBlackFog == true && !managerhub.player.isInCave)
        {
            Buff_CaveFog(false);
            isOpenBlackFog = false;
        }
    }


    /// <summary>
    /// 洞穴模式：改变fog，skybox和terrain
    /// </summary>
    /// <param name="_Open"></param>
    public void Buff_CaveFog(bool _Open)
    {
        if (TransitionFogCoroutine == null)
        {
            TransitionFogCoroutine = StartCoroutine(TransitionFog(_Open));
        }
    }

    private IEnumerator TransitionFog(bool enterCave)
    {
        float elapsedTime = 0f;
        Color startColor = RenderSettings.fogColor;
        Vector2 startDistance = new Vector2(RenderSettings.fogStartDistance, RenderSettings.fogEndDistance);

        Color targetColor = enterCave ? timeStruct._fog.FogNightColor : timeStruct._fog.FogDayColor;
        Vector2 targetDistance = enterCave ? timeStruct._fog.FogCaveDistance : timeStruct._fog.FogDayDistance;

        // 获取 BlocksMaterial 的初始颜色和目标颜色
        Color startBlockColor = enterCave ? timeStruct._Terrain.BlocksDayColor : timeStruct._Terrain.BlocksNightColor;
        Color targetBlockColor = enterCave ? timeStruct._Terrain.BlocksNightColor : timeStruct._Terrain.BlocksDayColor;

        // 获取 Skybox 的初始曝光值和目标曝光值
        //float startExposure = enterCave ? 1f : 0f;
        //float targetExposure = enterCave ? 0f : 1f;

        while (elapsedTime < fogTransitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fogTransitionTime;

            // Lerp 颜色和距离
            Color FogLerpColor = Color.Lerp(startColor, targetColor, t);
            RenderSettings.fogColor = FogLerpColor;
            RenderSettings.fogStartDistance = Mathf.Lerp(startDistance.x, targetDistance.x, t);
            RenderSettings.fogEndDistance = Mathf.Lerp(startDistance.y, targetDistance.y, t);

            // Lerp BlocksMaterial 的颜色
            timeStruct._Terrain.BlocksMaterial.color = Color.Lerp(startBlockColor, targetBlockColor, t);

            // Lerp Skybox 的曝光值
            //timeStruct._Skybox.SkyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(startExposure, targetExposure, t));
            timeStruct._fog.SkyboxMaterial.SetColor("_Tint", FogLerpColor);

            yield return null;
        }

        // 确保最终值正确
        RenderSettings.fogColor = targetColor;
        RenderSettings.fogStartDistance = targetDistance.x;
        RenderSettings.fogEndDistance = targetDistance.y;
        timeStruct._Terrain.BlocksMaterial.color = targetBlockColor;
        //timeStruct._Skybox.SkyboxMaterial.SetFloat("_Exposure", targetExposure);
        timeStruct._fog.SkyboxMaterial.SetColor("_Tint", targetColor);
        TransitionFogCoroutine = null;
    }




    #endregion


    #region Buff_潜水模式


    #endregion 

}

