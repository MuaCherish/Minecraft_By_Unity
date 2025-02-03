using Homebrew;
using System.Collections;
using UnityEngine;
using Cloud;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;

public class TimeManager : MonoBehaviour
{


    #region 周期函数

    private ManagerHub managerhub;
    bool hasExec_Start = true;
    bool hasExec_Loading = true;
    bool hasExec_Playing = true;

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
    }
    private void Start()
    {
        
    }

    private void Update()
    {

        switch (managerhub.world.game_state)
        {
            case Game_State.Start:
                Handle_GameState_Start();
                if (!hasExec_Loading)
                    hasExec_Loading = true;

                if (!hasExec_Playing)
                    hasExec_Playing = true;
                break;

            case Game_State.Loading:
                Handle_GameState_Loading();
                if (!hasExec_Start)
                    hasExec_Start = true;
                if (!hasExec_Playing)
                    hasExec_Playing = true;
                break;

            case Game_State.Playing:
                Handle_GameState_Playing();
                if (!hasExec_Start)
                    hasExec_Start = true;

                if (!hasExec_Loading)
                    hasExec_Loading = true;
                break;

        }

    }

   
    void Handle_GameState_Start()
    {
        if (hasExec_Start)
        {
            
            hasExec_Start = false;
        }




    }


    void Handle_GameState_Loading()
    {
        if (hasExec_Loading)
        {
            InitTimeManager();

            if (!SkyParent.activeSelf)
            {
                SkyParent.SetActive(true);
            }
            hasExec_Loading = false;
        }


        

        

    }


    void Handle_GameState_Playing()
    {
        if (hasExec_Playing)
        {
            hasExec_Playing = false;
        }



        UpdateGameTime_Clock();
        UpdateGameTime_Object();




        

    }


    public void InitTimeManager()
    {
        gameTime.CurrentTime = 12f;
        //gameTime.CurrentTime = Random.Range(12f, 20f);
        gameTime.previous_CurrentTime = 0f;

        // 使用GetCopy来获取WeatherState的副本
        CurrentWeatherState = WeatherStates[0].GetCopy();

    }

    private void OnApplicationQuit()
    {
        ApplyWeatherState(WeatherStates[TimeData.Day].GetCopy());
    }



    #endregion


    #region 游戏时钟

    [Foldout("时间流逝", true)]
    [Header("时间设置")] public GameTime gameTime;
    private bool isPauseTime;


    /// <summary>
    /// 是否是晚上
    /// </summary>
    /// <returns>是否为晚上</returns>
    public bool IsNight()
    {
        return IsNight(gameTime.CurrentTime);
    }

    /// <summary>
    /// 是否是晚上（根据指定时间）
    /// </summary>
    /// <param name="_time">指定时间</param>
    /// <returns>是否为晚上</returns>
    public bool IsNight(float _time)
    {
        return _time <= gameTime.天开始变亮.x || _time >= gameTime.天开始变黑.y;
    }

    /// <summary>
    /// 获取当前游戏时间
    /// </summary>
    /// <returns>当前时间</returns>
    public float GetCurrentTime()
    {
        return gameTime.CurrentTime;
    }

    /// <summary>
    /// 设置当前游戏时间
    /// </summary>
    /// <param name="_time">目标时间</param>
    public void SetTime(float _time)
    {
        gameTime.CurrentTime = Mathf.Clamp(_time, 0, 24); // 确保时间有效
    }

    /// <summary>
    /// 暂停或恢复时间流逝
    /// </summary>
    /// <param name="_pause">是否暂停</param>
    public void PauseTime(bool _pause)
    {
        isPauseTime = _pause;
    }


    /// <summary>
    /// 更新游戏时钟
    /// </summary>
    void UpdateGameTime_Clock()
    {
        // 提前返回：如果暂停时间或不在游戏中
        if (isPauseTime)
            return;

        // 确保时间在 0-24 小时之间循环
        if (gameTime.CurrentTime >= 24)
            gameTime.CurrentTime -= 24;


        Update_Value();

        // 增加游戏时间
        gameTime.CurrentTime += Time.deltaTime / gameTime.second_GameOneHour;
    }


    //检查时间是否有显著变化
    bool HasSignificantTimeChange()
    {
        const float maxTimeChange = 0.5f; // 调整为固定值
        return Mathf.Abs(gameTime.CurrentTime - gameTime.previous_CurrentTime) > maxTimeChange;
    }




    #endregion


    #region 天气模板



    [Foldout("Transforms",true)]
    [Header("Sky父类")] public GameObject SkyParent;
    [Header("SkyBox")] public Material Mat_SkyboxObject;
    [Header("Terrain")] public Material Mat_TerrainObject;
    [Header("Water")] public Material Mat_Water;
    [Header("Sun")] public SpriteRenderer Sprite_Sun;

    [Foldout("天气模板",true)]
    [Header("目前天气")] [ReadOnly] public WeatherState CurrentWeatherState; 
    [Header("天气模板类")] public WeatherState[] WeatherStates;

    private Coroutine coroutine_WeatherTransition;  // 存储正在运行的协程


    /// <summary>
    /// 缓慢将所有Objects同步游戏时间
    /// </summary>
    public void FromGameTimeToUpdateAll_Smooth()
    {
        float midA = (gameTime.天开始变亮.x + gameTime.天开始变亮.y) / 2f;  //6点
        float midB = (gameTime.天开始变黑.x + gameTime.天开始变黑.y) / 2f;  //18点

        float _maxTime = (gameTime.天开始变亮.x - midA) / 2f * gameTime.second_GameOneHour;

        //4
        if (Mathf.Abs(gameTime.CurrentTime - gameTime.天开始变亮.x) <= 0.03f)
        {
            SwitchWeatherState(TimeData.Day, _maxTime, 1f);

        }
        //6
        else if (Mathf.Abs(gameTime.CurrentTime - midA) <= 0.03f)
        {
            SwitchWeatherState(TimeData.Day, _maxTime, 1f);
        }
        //16
        else if (Mathf.Abs(gameTime.CurrentTime - gameTime.天开始变黑.x) <= 0.03f)
        {
            SwitchWeatherState(TimeData.Day, _maxTime, 1f);
        }
        //18
        else if (Mathf.Abs(gameTime.CurrentTime - midB) <= 0.03f)
        {
            SwitchWeatherState(TimeData.Day, _maxTime, 1f);
        }


    }

    /// <summary>
    /// 缓慢将所有Objects同步游戏时间
    /// </summary>
    public void FromGameTimeToUpdateAll_Smooth(float _duraTion)
    {
        //获取当前时间
        float _current = gameTime.CurrentTime;

        //确定下一个天气并计算_maxValue
        byte _lastId = TimeData.Day;
        byte _nextId = TimeData.Day;
        float _maxValue = 1f;

        //中心点
        float midA = (gameTime.天开始变亮.x + gameTime.天开始变亮.y) / 2f;  //6点
        float midB = (gameTime.天开始变黑.x + gameTime.天开始变黑.y) / 2f;  //18点

        //白天
        if (_current >= gameTime.天开始变亮.y && _current <= gameTime.天开始变黑.x)
        {
            _nextId = TimeData.Day;
            SwitchWeatherState(_nextId, _duraTion, _maxValue);
        }
        //黑夜
        else if (_current >= gameTime.天开始变黑.y || _current <= gameTime.天开始变亮.x)
        {
            _nextId = TimeData.Night;
            SwitchWeatherState(_nextId, _duraTion, _maxValue);
        }
        //过渡到白天
        else if (_current >= midA && _current <= gameTime.天开始变亮.y)
        {
            _lastId = TimeData.Sunset;
            _nextId = TimeData.Day;
            _maxValue = (_current - midA) / (gameTime.天开始变亮.y - midA);
            SwitchWeatherState(_nextId, _duraTion, _maxValue);
        }
        //过渡到黑夜
        else if (_current >= midB && _current <= gameTime.天开始变黑.y)
        {
            _lastId = TimeData.Sunset;
            _nextId = TimeData.Night;
            _maxValue = (_current - midB) / (gameTime.天开始变黑.y - midB);
            SwitchWeatherState(_nextId, _duraTion, _maxValue);
        }
        //黑夜过渡到晚霞
        else if (_current >= gameTime.天开始变亮.x && _current <= midA)
        {
            _lastId = TimeData.Night;
            _nextId = TimeData.Sunset;
            _maxValue = (_current - gameTime.天开始变亮.x) / (midB - gameTime.天开始变亮.x);
            SwitchWeatherState(_nextId, _duraTion, _maxValue);
        }
        //白天过渡到晚霞
        else if (_current >= gameTime.天开始变黑.x && _current <= midB)
        {
            _lastId = TimeData.Day;
            _nextId = TimeData.Sunset;
            _maxValue = (_current - gameTime.天开始变黑.x) / (midB - gameTime.天开始变黑.x);
            SwitchWeatherState(_nextId, _duraTion, _maxValue);
        }


    }

    /// <summary>
    /// 立即将所有Objects同步游戏时间
    /// </summary>
    public void FromGameTimeToUpdateAll_Imediately()
    {
        //获取当前时间
        float _current = gameTime.CurrentTime;

        //确定下一个天气并计算_maxValue
        byte _lastId = TimeData.Day;
        byte _nextId = TimeData.Day;
        float _maxValue = 0f;

        //中心点
        float midA = (gameTime.天开始变亮.x + gameTime.天开始变亮.y) / 2f;  //6点
        float midB = (gameTime.天开始变黑.x + gameTime.天开始变黑.y) / 2f;  //18点

        //白天
        if (_current >= gameTime.天开始变亮.y && _current <= gameTime.天开始变黑.x) 
        {
            _nextId = TimeData.Day;
            ApplyWeatherState(WeatherStates[_nextId].GetCopy());
        }
        //黑夜
        else if(_current >= gameTime.天开始变黑.y || _current <= gameTime.天开始变亮.x)
        {
            _nextId = TimeData.Night;
            ApplyWeatherState(WeatherStates[_nextId].GetCopy());
        }
        //过渡到白天
        else if (_current >= midA && _current <= gameTime.天开始变亮.y) 
        {
            _lastId = TimeData.Sunset;
            _nextId = TimeData.Day;
            _maxValue = (_current - midA) / (gameTime.天开始变亮.y - midA);
            ApplyWeatherState(WeatherStates[_lastId], WeatherStates[_nextId].GetCopy(), _maxValue);
        }
        //过渡到黑夜
        else if (_current >= midB && _current <= gameTime.天开始变黑.y) 
        {
            _lastId = TimeData.Sunset;
            _nextId = TimeData.Night;
            _maxValue = (_current - midB) / (gameTime.天开始变黑.y - midB);
            ApplyWeatherState(WeatherStates[_lastId], WeatherStates[_nextId].GetCopy(), _maxValue);
        }
        //黑夜过渡到晚霞
        else if (_current >= gameTime.天开始变亮.x && _current <= midA)
        {
            _lastId = TimeData.Night;
            _nextId = TimeData.Sunset;
            _maxValue = (_current - gameTime.天开始变亮.x) / (midB - gameTime.天开始变亮.x);
            ApplyWeatherState(WeatherStates[_lastId], WeatherStates[_nextId].GetCopy(), _maxValue);
        }
        //白天过渡到晚霞
        else if (_current >= gameTime.天开始变黑.x && _current <= midB)
        {
            _lastId = TimeData.Day;
            _nextId = TimeData.Sunset;
            _maxValue = (_current - gameTime.天开始变黑.x) / (midB - gameTime.天开始变黑.x);
            ApplyWeatherState(WeatherStates[_lastId], WeatherStates[_nextId].GetCopy(), _maxValue);
        }

        

    }


    /// <summary>
    /// 在一定时间内过渡天气模板的百分比
    /// _maxValue = 1则是100%
    /// _maxTime = 0的话将会直接赋值
    /// </summary>
    /// <param name="_id">目标天气模板ID</param>
    /// <param name="_time">过渡时间</param>
    public void SwitchWeatherState(byte _id, float _maxTime, float _maxValue)
    {
        // 异常处理 - Id 错误
        if (_id < 0 || _id >= WeatherStates.Length)
        {
            Debug.LogWarning("Invalid weather ID");
            return;
        }

        // 直接赋值的情况 (不启动协程)
        if (_maxTime == 0)
        {
            ApplyWeatherState(WeatherStates[_id].GetCopy());
            return;
        }

        // 启动过渡过程
        StartWeatherTransition(WeatherStates[_id].GetCopy(), _maxTime, _maxValue);
    }

    private void StartWeatherTransition(WeatherState targetWeather, float _maxTime, float _maxValue)
    {
        WeatherState startWeather = CurrentWeatherState.GetCopy();

        float targetTransitionProgress = Mathf.Clamp01(_maxValue);  // 根据 _maxValue 调整目标进度

        // 在 Update 中进行逐帧过渡
        StartCoroutine(UpdateWeatherTransition(startWeather, targetWeather, _maxTime, targetTransitionProgress));
    }

    private IEnumerator UpdateWeatherTransition(WeatherState startWeather, WeatherState targetWeather, float _maxTime, float targetTransitionProgress)
    {

        float elapsedTime = 0f;

        // 渐变过渡过程
        while (elapsedTime < _maxTime)
        {
            elapsedTime += Time.deltaTime;
            float transitionProgress = Mathf.Clamp01(elapsedTime / _maxTime);  // 计算过渡进度

            // 根据 _maxValue 调整进度
            transitionProgress = Mathf.Lerp(0f, targetTransitionProgress, transitionProgress);

            // 更新天气状态的各个参数
            UpdateWeatherParams(startWeather, targetWeather, transitionProgress);

            yield return null;
        }

        // 结束过渡，直接设置目标天气状态
        ApplyWeatherState(targetWeather);
    }

    private void UpdateWeatherParams(WeatherState startWeather, WeatherState targetWeather, float transitionProgress)
    {
        // Skybox 渐变
        Mat_SkyboxObject.SetColor("_ColorA", Color.Lerp(startWeather.Skybox_Colour[0], targetWeather.Skybox_Colour[0], transitionProgress));
        Mat_SkyboxObject.SetColor("_ColorB", Color.Lerp(startWeather.Skybox_Colour[1], targetWeather.Skybox_Colour[1], transitionProgress));

        // Terrain 渐变
        Mat_TerrainObject.SetColor("_Color", Color.Lerp(startWeather.Terrain_Colour, targetWeather.Terrain_Colour, transitionProgress));

        // Water 渐变
        Mat_Water.SetFloat("__2", Mathf.Lerp(startWeather.Water_Lightness, targetWeather.Water_Lightness, transitionProgress));

        // Cloud 渐变
        if (managerhub.cloudManager != null)
        {
            managerhub.cloudManager.SetCloudColor(Color.Lerp(startWeather.Cloud_Colour, targetWeather.Cloud_Colour, transitionProgress));
        }

        // Sun 渐变
        Sprite_Sun.color = Color.Lerp(startWeather.Sun_Colour, targetWeather.Sun_Colour, transitionProgress);

        // 更新 CurrentWeatherState
        CurrentWeatherState.Skybox_Colour[0] = Color.Lerp(startWeather.Skybox_Colour[0], targetWeather.Skybox_Colour[0], transitionProgress);
        CurrentWeatherState.Skybox_Colour[1] = Color.Lerp(startWeather.Skybox_Colour[1], targetWeather.Skybox_Colour[1], transitionProgress);
        CurrentWeatherState.Terrain_Colour = Color.Lerp(startWeather.Terrain_Colour, targetWeather.Terrain_Colour, transitionProgress);
        CurrentWeatherState.Cloud_Colour = Color.Lerp(startWeather.Cloud_Colour, targetWeather.Cloud_Colour, transitionProgress);
        CurrentWeatherState.Water_Lightness = Mathf.Lerp(startWeather.Water_Lightness, targetWeather.Water_Lightness, transitionProgress);
    }

    private void ApplyWeatherState(WeatherState targetWeather)
    {
        // 直接应用目标天气状态的各个参数
        Mat_SkyboxObject.SetColor("_ColorA", targetWeather.Skybox_Colour[0]);
        Mat_SkyboxObject.SetColor("_ColorB", targetWeather.Skybox_Colour[1]);
        Mat_TerrainObject.SetColor("_Color", targetWeather.Terrain_Colour);
        Mat_Water.SetFloat("__2", targetWeather.Water_Lightness);
        if (managerhub.cloudManager != null)
            managerhub.cloudManager.SetCloudColor(targetWeather.Cloud_Colour);
        Sprite_Sun.color = targetWeather.Sun_Colour;

        // 更新当前天气状态
        CurrentWeatherState = targetWeather;
    }

    private void ApplyWeatherState(WeatherState Last_Weather, WeatherState Next_Weather, float _value)
    {
        // 使用 Lerp 进行插值计算
        Color skyboxColorA = Color.Lerp(Last_Weather.Skybox_Colour[0], Next_Weather.Skybox_Colour[0], _value);
        Color skyboxColorB = Color.Lerp(Last_Weather.Skybox_Colour[1], Next_Weather.Skybox_Colour[1], _value);
        Color terrainColor = Color.Lerp(Last_Weather.Terrain_Colour, Next_Weather.Terrain_Colour, _value);
        Color cloudColor = Color.Lerp(Last_Weather.Cloud_Colour, Next_Weather.Cloud_Colour, _value);
        Color sunColor = Color.Lerp(Last_Weather.Sun_Colour, Next_Weather.Sun_Colour, _value);

        float waterLightness = Mathf.Lerp(Last_Weather.Water_Lightness, Next_Weather.Water_Lightness, _value);

        // 更新材质的颜色和其他属性
        Mat_SkyboxObject.SetColor("_ColorA", skyboxColorA);
        Mat_SkyboxObject.SetColor("_ColorB", skyboxColorB);
        Mat_TerrainObject.SetColor("_Color", terrainColor);
        Mat_Water.SetFloat("__2", waterLightness);

        if (managerhub.cloudManager != null)
            managerhub.cloudManager.SetCloudColor(cloudColor);

        Sprite_Sun.color = sunColor;

        // 更新当前天气状态
        CurrentWeatherState.Skybox_Colour[0] = skyboxColorA;
        CurrentWeatherState.Skybox_Colour[1] = skyboxColorB;
        CurrentWeatherState.Terrain_Colour = terrainColor;
        CurrentWeatherState.Cloud_Colour = cloudColor;
        CurrentWeatherState.Sun_Colour = sunColor;
        CurrentWeatherState.Water_Lightness = waterLightness;
    }



    #endregion


    #region [尽量不要碰]昼夜循环

    /// <summary>
    /// 更新游戏物体
    /// </summary>
    void UpdateGameTime_Object()
    {

        //提前返回 - 如果是阴天则不更新
        if (managerhub.weather.weather == Enum_Weather.Rainy)
            return;


        // 如果值跳动过大，则立即改变
        // 更新Objects
        if (HasSignificantTimeChange() || ShouldUpdateObjects())
            UpdateAll();

        // 保存当前时间
        gameTime.previous_CurrentTime = gameTime.CurrentTime;
    }

    /// <summary>
    /// 检查是否需要更新对象
    /// </summary>
    /// <returns>是否需要更新</returns>
    private bool ShouldUpdateObjects()
    {
        return gameTime._value != 0 && gameTime._value != 1;
    }

    void Update_Value()
    {

        if ((gameTime.CurrentTime >= gameTime.天开始变亮.x && gameTime.CurrentTime <= gameTime.天开始变亮.y) ||
            (gameTime.CurrentTime >= gameTime.天开始变黑.x && gameTime.CurrentTime <= gameTime.天开始变黑.y)
            )
        {
            if (gameTime.CurrentTime < 12)
            {
                gameTime._value = Mathf.InverseLerp(
                    gameTime.天开始变亮.x,
                    gameTime.天开始变亮.y,
                    gameTime.CurrentTime
                );
            }
            else
            {
                gameTime._value = 1 - Mathf.InverseLerp(
                    gameTime.天开始变黑.x,
                    gameTime.天开始变黑.y,
                    gameTime.CurrentTime
                );
            }
        }
        else if (gameTime.CurrentTime >= gameTime.天开始变亮.y && gameTime.CurrentTime <= gameTime.天开始变黑.x)
        {
            gameTime._value = 1;
        }
        else
        {
            gameTime._value = 0;
        }


    }



    /// <summary>
    /// 更新所有Object
    /// </summary>
    public void UpdateAll()
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
        if (gameTime._value >= 0f && gameTime._value < 0.5f)
        {
            float t = gameTime._value / 0.5f;  // 归一化比例 (0 ~ 1)
            ALerpColor = Color.Lerp(WeatherStates[TimeData.Night].Skybox_Colour[0], WeatherStates[TimeData.Sunset].Skybox_Colour[0], t);
            BLerpColor = Color.Lerp(WeatherStates[TimeData.Night].Skybox_Colour[1], WeatherStates[TimeData.Sunset].Skybox_Colour[1], t);
            CloudColor = Color.Lerp(WeatherStates[TimeData.Night].Skybox_Colour[1], WeatherStates[TimeData.Sunset].Skybox_Colour[1], t - 0.1f);
        }
        // 0.5 ~ 1：从 SunSet 到 Day
        else if (gameTime._value >= 0.5f && gameTime._value <= 1f)
        {
            float t = (gameTime._value - 0.5f) / 0.5f;  // 归一化比例 (0 ~ 1)
            ALerpColor = Color.Lerp(WeatherStates[TimeData.Sunset].Skybox_Colour[0], WeatherStates[TimeData.Day].Skybox_Colour[0], t);
            BLerpColor = Color.Lerp(WeatherStates[TimeData.Sunset].Skybox_Colour[1], WeatherStates[TimeData.Day].Skybox_Colour[1], t);
            CloudColor = Color.Lerp(WeatherStates[TimeData.Sunset].Skybox_Colour[1], WeatherStates[TimeData.Day].Skybox_Colour[1], t - 0.1f);
        }
        else
        {
            return;  // 超出范围，不做处理
        }

        // 应用计算后的颜色
        Mat_SkyboxObject.SetColor("_ColorA", ALerpColor);
        Mat_SkyboxObject.SetColor("_ColorB", BLerpColor);

        // 更新云的颜色


    }


    Color CloudColor;
    void SetCloudColor()
    {
        //Color LerpCloudColor = Color.Lerp(timeStruct._cloud.CloudNightColor, timeStruct._cloud.CloudDayColor, gameTime._value);
        //managerhub.cloudManager.SetCloudColor(LerpCloudColor);
        CloudColor.a = 1f;
        managerhub.cloudManager.SetCloudColor(CloudColor);
    }


    void SetTerrainColor()
    {
        Mat_TerrainObject.color = Color.Lerp(WeatherStates[TimeData.Night].Terrain_Colour, WeatherStates[TimeData.Day].Terrain_Colour, gameTime._value);
    }


    void SetWaterColor()
    {
        
        float a = Mathf.Lerp(WeatherStates[TimeData.Night].Water_Lightness, WeatherStates[TimeData.Day].Water_Lightness, gameTime._value);
        Mat_Water.SetFloat("_1", a);

    }


    void SetLightCast()
    {
        if (managerhub.sunMoving.isOpenLightCast)
        {
            managerhub.sunMoving.SetLightCastDensity();
        }
    }


    #endregion


}

//天气模板类
[System.Serializable]
public class WeatherState
{
    public string name;
    public Color[] Skybox_Colour = new Color[2];
    public Color Terrain_Colour;
    public Color Cloud_Colour;
    public float Water_Lightness;
    public Color Sun_Colour;

    // 构造函数
    public WeatherState(string name, Color skyboxColor1, Color skyboxColor2, Color terrainColor, Color cloudColor, float waterLightness, Color sunColor)
    {
        this.name = name;
        this.Skybox_Colour[0] = skyboxColor1;
        this.Skybox_Colour[1] = skyboxColor2;
        this.Terrain_Colour = terrainColor;
        this.Cloud_Colour = cloudColor;
        this.Water_Lightness = waterLightness;
        this.Sun_Colour = sunColor;
    }

    // 返回整个数据结构的副本
    public WeatherState GetCopy()
    {
        return new WeatherState(
            this.name,
            new Color(this.Skybox_Colour[0].r, this.Skybox_Colour[0].g, this.Skybox_Colour[0].b, this.Skybox_Colour[0].a),
            new Color(this.Skybox_Colour[1].r, this.Skybox_Colour[1].g, this.Skybox_Colour[1].b, this.Skybox_Colour[1].a),
            new Color(this.Terrain_Colour.r, this.Terrain_Colour.g, this.Terrain_Colour.b, this.Terrain_Colour.a),
            new Color(this.Cloud_Colour.r, this.Cloud_Colour.g, this.Cloud_Colour.b, this.Cloud_Colour.a),
            this.Water_Lightness,
            new Color(this.Sun_Colour.r, this.Sun_Colour.g, this.Sun_Colour.b, this.Sun_Colour.a)
        );
    }

}

