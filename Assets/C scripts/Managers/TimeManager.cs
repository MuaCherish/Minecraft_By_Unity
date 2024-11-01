using Homebrew;
using System.Collections;
using UnityEngine;
using Cloud;
using static UnityEngine.Rendering.DebugUI;

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


        //WeatherCheck();

        

    }


    [Foldout("引用", true)]
    [Header("高空引用")] public GameObject SkyParent;
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
                //UpdateDayFogDistance();
                SkyParent.SetActive(true);
                haeExec_Update = false;
            }


            DynamicState_isNight();


            //DynamicSwitchCaveMode();
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
        InitSkyBox();
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
        //timeStruct._time.CurrentTime = 12f;
        timeStruct._time.CurrentTime = Random.Range(8f, 15f);
        timeStruct._time.value = 1;
        timeStruct._Water.WatersMaterial.SetFloat("__2", timeStruct._Water.LightnessRange.y);



        //if (isRandomWeather)
        //{
        //    weather = (Enum_Weather)Random.Range(0, System.Enum.GetValues(typeof(Enum_Weather)).Length);
        //}

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
    //public WeatherStruct[] weatherLists;

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

                UpdateAll();

            }

            

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

                //Color LerpFogColor = Color.Lerp(timeStruct._fog.FogNightColor, timeStruct._fog.FogDayColor, timeStruct._time.value);

                //改变 
                //timeStruct._Skybox.SkyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(timeStruct._Skybox.SkyboxRange.x, timeStruct._Skybox.SkyboxRange.y, timeStruct._time.value));
                //timeStruct._fog.SkyboxMaterial.SetColor("_Tint", LerpFogColor);

                //if (managerhub.player.isInCave == false)
                //{
                //    RenderSettings.fogColor = LerpFogColor;
                //}
                UpdateAll();


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

    void UpdateAll()
    {
        SetLight();
        SetSkyBoxColor();
        SetCloudColor();
        SetTerrainColor();
        SetWaterColor();
        SetLightCast();
    }


    void SetLight()
    {
        managerhub.sunMoving.SetLightInten();
    }

    void SetSkyBoxColor()
    {
        Color ALerpColor;
        Color BLerpColor;

        // 0 ~ 0.5：从 Night 到 SunSet
        if (timeStruct._time.value >= 0f && timeStruct._time.value < 0.5f)
        {
            float t = timeStruct._time.value / 0.5f;  // 归一化比例 (0 ~ 1)
            ALerpColor = Color.Lerp(timeStruct._skybox.NightColor[0], timeStruct._skybox.SunSetColor[0], t);
            BLerpColor = Color.Lerp(timeStruct._skybox.NightColor[1], timeStruct._skybox.SunSetColor[1], t);
            CloudColor = Color.Lerp(timeStruct._skybox.NightColor[1], timeStruct._skybox.SunSetColor[1], t - 0.1f);
        }
        // 0.5 ~ 1：从 SunSet 到 Day
        else if (timeStruct._time.value >= 0.5f && timeStruct._time.value <= 1f)
        {
            float t = (timeStruct._time.value - 0.5f) / 0.5f;  // 归一化比例 (0 ~ 1)
            ALerpColor = Color.Lerp(timeStruct._skybox.SunSetColor[0], timeStruct._skybox.DayColor[0], t);
            BLerpColor = Color.Lerp(timeStruct._skybox.SunSetColor[1], timeStruct._skybox.DayColor[1], t);
            CloudColor = Color.Lerp(timeStruct._skybox.SunSetColor[1], timeStruct._skybox.DayColor[1], t - 0.1f);
        }
        else
        {
            return;  // 超出范围，不做处理
        }

        // 应用计算后的颜色
        timeStruct._skybox.SkyboxMaterial.SetColor("_ColorA", ALerpColor);
        timeStruct._skybox.SkyboxMaterial.SetColor("_ColorB", BLerpColor);

        // 更新云的颜色
        
        
    }



    Color CloudColor;
    void SetCloudColor()
    {
        //Color LerpCloudColor = Color.Lerp(timeStruct._cloud.CloudNightColor, timeStruct._cloud.CloudDayColor, timeStruct._time.value);
        //managerhub.cloudManager.SetCloudColor(LerpCloudColor);
        CloudColor.a = 1f;
        managerhub.cloudManager.SetCloudColor(CloudColor);
    }


    void SetTerrainColor()
    {
        timeStruct._Terrain.BlocksMaterial.color = Color.Lerp(timeStruct._Terrain.BlocksNightColor, timeStruct._Terrain.BlocksDayColor, timeStruct._time.value);
    }


    void SetWaterColor()
    {

        timeStruct._Water.WatersMaterial.SetFloat("__2", Mathf.Lerp(timeStruct._Water.LightnessRange.x, timeStruct._Water.LightnessRange.y, timeStruct._time.value));
    }


    void SetLightCast()
    {
        if (managerhub.sunMoving.isOpenLightCast)
        {
            managerhub.sunMoving.SetLightCastDensity();
        }
    }


    //根据渲染范围更新迷雾距离
    //public void UpdateDayFogDistance()
    //{
    //    float _renderSize = managerhub.canvasManager.world.renderSize;
    //    float _t = _renderSize * 10 - 10f;


    //    timeStruct._fog.FogDayDistance = new Vector2(_t * 0.8f, _t);

    //    RenderSettings.fogStartDistance = timeStruct._fog.FogDayDistance.x;
    //    RenderSettings.fogEndDistance = timeStruct._fog.FogDayDistance.y;
    //}

    #endregion


    #region 天空盒

    void InitSkyBox()
    {
        timeStruct._skybox.SkyboxMaterial.SetColor("_ColorA", timeStruct._skybox.DayColor[0]);
        timeStruct._skybox.SkyboxMaterial.SetColor("_ColorB", timeStruct._skybox.DayColor[1]);

    }



    #endregion


    #region 天气(已禁用)

    //[Foldout("天气参数", true)]
    //[Header("随机天气")] public bool isRandomWeather = true;
    //[Header("天气")] public Enum_Weather weather = Enum_Weather.Sunshine;
    //void WeatherCheck()
    //{


    //    switch (weather)
    //    {
    //        case Enum_Weather.Sunshine:
    //            timeStruct._fog.FogDayColor = weatherLists[0].FogDayColor;
    //            timeStruct._fog.FogNightColor = weatherLists[0].FogNightColor;
    //            break;

    //        case Enum_Weather.Cloudy:
    //            managerhub.cloudManager.enabled = false;
    //            timeStruct._fog.FogDayColor = weatherLists[1].FogDayColor;
    //            timeStruct._fog.FogNightColor = weatherLists[1].FogNightColor;
    //            break;
    //    }
    //}


    #endregion


    #region Buff_洞穴模式（已禁用）

    //[Foldout("Buff_洞穴模式", true)]
    //[Header("洞穴模式过渡时间")] public float fogTransitionTime = 3f;  // 迷雾过渡时间

    ////黑色迷雾协程
    //private bool isOpenBlackFog = false;
    //private Coroutine TransitionFogCoroutine;


    //public void DynamicSwitchCaveMode()
    //{
    //    if (isOpenBlackFog == false && managerhub.player.isInCave)
    //    {
    //        Buff_CaveFog(true);
    //        isOpenBlackFog = true;
    //    }
    //    else if (!isNight && isOpenBlackFog == true && !managerhub.player.isInCave)
    //    {
    //        Buff_CaveFog(false);
    //        isOpenBlackFog = false;
    //    }
    //}


    ///// <summary>
    ///// 洞穴模式：改变fog，skybox和terrain
    ///// </summary>
    ///// <param name="_Open"></param>
    //public void Buff_CaveFog(bool _Open)
    //{
    //    if (TransitionFogCoroutine == null)
    //    {
    //        TransitionFogCoroutine = StartCoroutine(TransitionFog(_Open));
    //    }
    //}

    //private IEnumerator TransitionFog(bool enterCave)
    //{
    //    float elapsedTime = 0f;
    //    Color startColor = RenderSettings.fogColor;
    //    Vector2 startDistance = new Vector2(RenderSettings.fogStartDistance, RenderSettings.fogEndDistance);

    //    Color targetColor = enterCave ? timeStruct._fog.FogNightColor : timeStruct._fog.FogDayColor;
    //    Vector2 targetDistance = enterCave ? timeStruct._fog.FogCaveDistance : timeStruct._fog.FogDayDistance;

    //    // 获取 BlocksMaterial 的初始颜色和目标颜色
    //    Color startBlockColor = enterCave ? timeStruct._Terrain.BlocksDayColor : timeStruct._Terrain.BlocksNightColor;
    //    Color targetBlockColor = enterCave ? timeStruct._Terrain.BlocksNightColor : timeStruct._Terrain.BlocksDayColor;

    //    // 获取 Skybox 的初始曝光值和目标曝光值
    //    //float startExposure = enterCave ? 1f : 0f;
    //    //float targetExposure = enterCave ? 0f : 1f;

    //    while (elapsedTime < fogTransitionTime)
    //    {
    //        elapsedTime += Time.deltaTime;
    //        float t = elapsedTime / fogTransitionTime;

    //        // Lerp 颜色和距离
    //        Color FogLerpColor = Color.Lerp(startColor, targetColor, t);
    //        RenderSettings.fogColor = FogLerpColor;
    //        RenderSettings.fogStartDistance = Mathf.Lerp(startDistance.x, targetDistance.x, t);
    //        RenderSettings.fogEndDistance = Mathf.Lerp(startDistance.y, targetDistance.y, t);

    //        // Lerp BlocksMaterial 的颜色
    //        timeStruct._Terrain.BlocksMaterial.color = Color.Lerp(startBlockColor, targetBlockColor, t);

    //        // Lerp Skybox 的曝光值
    //        //timeStruct._Skybox.SkyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(startExposure, targetExposure, t));
    //        timeStruct._fog.SkyboxMaterial.SetColor("_Tint", FogLerpColor);

    //        yield return null;
    //    }

    //    // 确保最终值正确
    //    RenderSettings.fogColor = targetColor;
    //    RenderSettings.fogStartDistance = targetDistance.x;
    //    RenderSettings.fogEndDistance = targetDistance.y;
    //    timeStruct._Terrain.BlocksMaterial.color = targetBlockColor;
    //    //timeStruct._Skybox.SkyboxMaterial.SetFloat("_Exposure", targetExposure);
    //    timeStruct._fog.SkyboxMaterial.SetColor("_Tint", targetColor);
    //    TransitionFogCoroutine = null;
    //}




    #endregion


    #region Buff_潜水模式


    #endregion 

}

