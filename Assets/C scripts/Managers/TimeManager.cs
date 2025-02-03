using Homebrew;
using System.Collections;
using UnityEngine;
using Cloud;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;

public class TimeManager : MonoBehaviour
{


    #region ���ں���

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

        // ʹ��GetCopy����ȡWeatherState�ĸ���
        CurrentWeatherState = WeatherStates[0].GetCopy();

    }

    private void OnApplicationQuit()
    {
        ApplyWeatherState(WeatherStates[TimeData.Day].GetCopy());
    }



    #endregion


    #region ��Ϸʱ��

    [Foldout("ʱ������", true)]
    [Header("ʱ������")] public GameTime gameTime;
    private bool isPauseTime;


    /// <summary>
    /// �Ƿ�������
    /// </summary>
    /// <returns>�Ƿ�Ϊ����</returns>
    public bool IsNight()
    {
        return IsNight(gameTime.CurrentTime);
    }

    /// <summary>
    /// �Ƿ������ϣ�����ָ��ʱ�䣩
    /// </summary>
    /// <param name="_time">ָ��ʱ��</param>
    /// <returns>�Ƿ�Ϊ����</returns>
    public bool IsNight(float _time)
    {
        return _time <= gameTime.�쿪ʼ����.x || _time >= gameTime.�쿪ʼ���.y;
    }

    /// <summary>
    /// ��ȡ��ǰ��Ϸʱ��
    /// </summary>
    /// <returns>��ǰʱ��</returns>
    public float GetCurrentTime()
    {
        return gameTime.CurrentTime;
    }

    /// <summary>
    /// ���õ�ǰ��Ϸʱ��
    /// </summary>
    /// <param name="_time">Ŀ��ʱ��</param>
    public void SetTime(float _time)
    {
        gameTime.CurrentTime = Mathf.Clamp(_time, 0, 24); // ȷ��ʱ����Ч
    }

    /// <summary>
    /// ��ͣ��ָ�ʱ������
    /// </summary>
    /// <param name="_pause">�Ƿ���ͣ</param>
    public void PauseTime(bool _pause)
    {
        isPauseTime = _pause;
    }


    /// <summary>
    /// ������Ϸʱ��
    /// </summary>
    void UpdateGameTime_Clock()
    {
        // ��ǰ���أ������ͣʱ�������Ϸ��
        if (isPauseTime)
            return;

        // ȷ��ʱ���� 0-24 Сʱ֮��ѭ��
        if (gameTime.CurrentTime >= 24)
            gameTime.CurrentTime -= 24;


        Update_Value();

        // ������Ϸʱ��
        gameTime.CurrentTime += Time.deltaTime / gameTime.second_GameOneHour;
    }


    //���ʱ���Ƿ��������仯
    bool HasSignificantTimeChange()
    {
        const float maxTimeChange = 0.5f; // ����Ϊ�̶�ֵ
        return Mathf.Abs(gameTime.CurrentTime - gameTime.previous_CurrentTime) > maxTimeChange;
    }




    #endregion


    #region ����ģ��



    [Foldout("Transforms",true)]
    [Header("Sky����")] public GameObject SkyParent;
    [Header("SkyBox")] public Material Mat_SkyboxObject;
    [Header("Terrain")] public Material Mat_TerrainObject;
    [Header("Water")] public Material Mat_Water;
    [Header("Sun")] public SpriteRenderer Sprite_Sun;

    [Foldout("����ģ��",true)]
    [Header("Ŀǰ����")] [ReadOnly] public WeatherState CurrentWeatherState; 
    [Header("����ģ����")] public WeatherState[] WeatherStates;

    private Coroutine coroutine_WeatherTransition;  // �洢�������е�Э��


    /// <summary>
    /// ����������Objectsͬ����Ϸʱ��
    /// </summary>
    public void FromGameTimeToUpdateAll_Smooth()
    {
        float midA = (gameTime.�쿪ʼ����.x + gameTime.�쿪ʼ����.y) / 2f;  //6��
        float midB = (gameTime.�쿪ʼ���.x + gameTime.�쿪ʼ���.y) / 2f;  //18��

        float _maxTime = (gameTime.�쿪ʼ����.x - midA) / 2f * gameTime.second_GameOneHour;

        //4
        if (Mathf.Abs(gameTime.CurrentTime - gameTime.�쿪ʼ����.x) <= 0.03f)
        {
            SwitchWeatherState(TimeData.Day, _maxTime, 1f);

        }
        //6
        else if (Mathf.Abs(gameTime.CurrentTime - midA) <= 0.03f)
        {
            SwitchWeatherState(TimeData.Day, _maxTime, 1f);
        }
        //16
        else if (Mathf.Abs(gameTime.CurrentTime - gameTime.�쿪ʼ���.x) <= 0.03f)
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
    /// ����������Objectsͬ����Ϸʱ��
    /// </summary>
    public void FromGameTimeToUpdateAll_Smooth(float _duraTion)
    {
        //��ȡ��ǰʱ��
        float _current = gameTime.CurrentTime;

        //ȷ����һ������������_maxValue
        byte _lastId = TimeData.Day;
        byte _nextId = TimeData.Day;
        float _maxValue = 1f;

        //���ĵ�
        float midA = (gameTime.�쿪ʼ����.x + gameTime.�쿪ʼ����.y) / 2f;  //6��
        float midB = (gameTime.�쿪ʼ���.x + gameTime.�쿪ʼ���.y) / 2f;  //18��

        //����
        if (_current >= gameTime.�쿪ʼ����.y && _current <= gameTime.�쿪ʼ���.x)
        {
            _nextId = TimeData.Day;
            SwitchWeatherState(_nextId, _duraTion, _maxValue);
        }
        //��ҹ
        else if (_current >= gameTime.�쿪ʼ���.y || _current <= gameTime.�쿪ʼ����.x)
        {
            _nextId = TimeData.Night;
            SwitchWeatherState(_nextId, _duraTion, _maxValue);
        }
        //���ɵ�����
        else if (_current >= midA && _current <= gameTime.�쿪ʼ����.y)
        {
            _lastId = TimeData.Sunset;
            _nextId = TimeData.Day;
            _maxValue = (_current - midA) / (gameTime.�쿪ʼ����.y - midA);
            SwitchWeatherState(_nextId, _duraTion, _maxValue);
        }
        //���ɵ���ҹ
        else if (_current >= midB && _current <= gameTime.�쿪ʼ���.y)
        {
            _lastId = TimeData.Sunset;
            _nextId = TimeData.Night;
            _maxValue = (_current - midB) / (gameTime.�쿪ʼ���.y - midB);
            SwitchWeatherState(_nextId, _duraTion, _maxValue);
        }
        //��ҹ���ɵ���ϼ
        else if (_current >= gameTime.�쿪ʼ����.x && _current <= midA)
        {
            _lastId = TimeData.Night;
            _nextId = TimeData.Sunset;
            _maxValue = (_current - gameTime.�쿪ʼ����.x) / (midB - gameTime.�쿪ʼ����.x);
            SwitchWeatherState(_nextId, _duraTion, _maxValue);
        }
        //������ɵ���ϼ
        else if (_current >= gameTime.�쿪ʼ���.x && _current <= midB)
        {
            _lastId = TimeData.Day;
            _nextId = TimeData.Sunset;
            _maxValue = (_current - gameTime.�쿪ʼ���.x) / (midB - gameTime.�쿪ʼ���.x);
            SwitchWeatherState(_nextId, _duraTion, _maxValue);
        }


    }

    /// <summary>
    /// ����������Objectsͬ����Ϸʱ��
    /// </summary>
    public void FromGameTimeToUpdateAll_Imediately()
    {
        //��ȡ��ǰʱ��
        float _current = gameTime.CurrentTime;

        //ȷ����һ������������_maxValue
        byte _lastId = TimeData.Day;
        byte _nextId = TimeData.Day;
        float _maxValue = 0f;

        //���ĵ�
        float midA = (gameTime.�쿪ʼ����.x + gameTime.�쿪ʼ����.y) / 2f;  //6��
        float midB = (gameTime.�쿪ʼ���.x + gameTime.�쿪ʼ���.y) / 2f;  //18��

        //����
        if (_current >= gameTime.�쿪ʼ����.y && _current <= gameTime.�쿪ʼ���.x) 
        {
            _nextId = TimeData.Day;
            ApplyWeatherState(WeatherStates[_nextId].GetCopy());
        }
        //��ҹ
        else if(_current >= gameTime.�쿪ʼ���.y || _current <= gameTime.�쿪ʼ����.x)
        {
            _nextId = TimeData.Night;
            ApplyWeatherState(WeatherStates[_nextId].GetCopy());
        }
        //���ɵ�����
        else if (_current >= midA && _current <= gameTime.�쿪ʼ����.y) 
        {
            _lastId = TimeData.Sunset;
            _nextId = TimeData.Day;
            _maxValue = (_current - midA) / (gameTime.�쿪ʼ����.y - midA);
            ApplyWeatherState(WeatherStates[_lastId], WeatherStates[_nextId].GetCopy(), _maxValue);
        }
        //���ɵ���ҹ
        else if (_current >= midB && _current <= gameTime.�쿪ʼ���.y) 
        {
            _lastId = TimeData.Sunset;
            _nextId = TimeData.Night;
            _maxValue = (_current - midB) / (gameTime.�쿪ʼ���.y - midB);
            ApplyWeatherState(WeatherStates[_lastId], WeatherStates[_nextId].GetCopy(), _maxValue);
        }
        //��ҹ���ɵ���ϼ
        else if (_current >= gameTime.�쿪ʼ����.x && _current <= midA)
        {
            _lastId = TimeData.Night;
            _nextId = TimeData.Sunset;
            _maxValue = (_current - gameTime.�쿪ʼ����.x) / (midB - gameTime.�쿪ʼ����.x);
            ApplyWeatherState(WeatherStates[_lastId], WeatherStates[_nextId].GetCopy(), _maxValue);
        }
        //������ɵ���ϼ
        else if (_current >= gameTime.�쿪ʼ���.x && _current <= midB)
        {
            _lastId = TimeData.Day;
            _nextId = TimeData.Sunset;
            _maxValue = (_current - gameTime.�쿪ʼ���.x) / (midB - gameTime.�쿪ʼ���.x);
            ApplyWeatherState(WeatherStates[_lastId], WeatherStates[_nextId].GetCopy(), _maxValue);
        }

        

    }


    /// <summary>
    /// ��һ��ʱ���ڹ�������ģ��İٷֱ�
    /// _maxValue = 1����100%
    /// _maxTime = 0�Ļ�����ֱ�Ӹ�ֵ
    /// </summary>
    /// <param name="_id">Ŀ������ģ��ID</param>
    /// <param name="_time">����ʱ��</param>
    public void SwitchWeatherState(byte _id, float _maxTime, float _maxValue)
    {
        // �쳣���� - Id ����
        if (_id < 0 || _id >= WeatherStates.Length)
        {
            Debug.LogWarning("Invalid weather ID");
            return;
        }

        // ֱ�Ӹ�ֵ����� (������Э��)
        if (_maxTime == 0)
        {
            ApplyWeatherState(WeatherStates[_id].GetCopy());
            return;
        }

        // �������ɹ���
        StartWeatherTransition(WeatherStates[_id].GetCopy(), _maxTime, _maxValue);
    }

    private void StartWeatherTransition(WeatherState targetWeather, float _maxTime, float _maxValue)
    {
        WeatherState startWeather = CurrentWeatherState.GetCopy();

        float targetTransitionProgress = Mathf.Clamp01(_maxValue);  // ���� _maxValue ����Ŀ�����

        // �� Update �н�����֡����
        StartCoroutine(UpdateWeatherTransition(startWeather, targetWeather, _maxTime, targetTransitionProgress));
    }

    private IEnumerator UpdateWeatherTransition(WeatherState startWeather, WeatherState targetWeather, float _maxTime, float targetTransitionProgress)
    {

        float elapsedTime = 0f;

        // ������ɹ���
        while (elapsedTime < _maxTime)
        {
            elapsedTime += Time.deltaTime;
            float transitionProgress = Mathf.Clamp01(elapsedTime / _maxTime);  // ������ɽ���

            // ���� _maxValue ��������
            transitionProgress = Mathf.Lerp(0f, targetTransitionProgress, transitionProgress);

            // ��������״̬�ĸ�������
            UpdateWeatherParams(startWeather, targetWeather, transitionProgress);

            yield return null;
        }

        // �������ɣ�ֱ������Ŀ������״̬
        ApplyWeatherState(targetWeather);
    }

    private void UpdateWeatherParams(WeatherState startWeather, WeatherState targetWeather, float transitionProgress)
    {
        // Skybox ����
        Mat_SkyboxObject.SetColor("_ColorA", Color.Lerp(startWeather.Skybox_Colour[0], targetWeather.Skybox_Colour[0], transitionProgress));
        Mat_SkyboxObject.SetColor("_ColorB", Color.Lerp(startWeather.Skybox_Colour[1], targetWeather.Skybox_Colour[1], transitionProgress));

        // Terrain ����
        Mat_TerrainObject.SetColor("_Color", Color.Lerp(startWeather.Terrain_Colour, targetWeather.Terrain_Colour, transitionProgress));

        // Water ����
        Mat_Water.SetFloat("__2", Mathf.Lerp(startWeather.Water_Lightness, targetWeather.Water_Lightness, transitionProgress));

        // Cloud ����
        if (managerhub.cloudManager != null)
        {
            managerhub.cloudManager.SetCloudColor(Color.Lerp(startWeather.Cloud_Colour, targetWeather.Cloud_Colour, transitionProgress));
        }

        // Sun ����
        Sprite_Sun.color = Color.Lerp(startWeather.Sun_Colour, targetWeather.Sun_Colour, transitionProgress);

        // ���� CurrentWeatherState
        CurrentWeatherState.Skybox_Colour[0] = Color.Lerp(startWeather.Skybox_Colour[0], targetWeather.Skybox_Colour[0], transitionProgress);
        CurrentWeatherState.Skybox_Colour[1] = Color.Lerp(startWeather.Skybox_Colour[1], targetWeather.Skybox_Colour[1], transitionProgress);
        CurrentWeatherState.Terrain_Colour = Color.Lerp(startWeather.Terrain_Colour, targetWeather.Terrain_Colour, transitionProgress);
        CurrentWeatherState.Cloud_Colour = Color.Lerp(startWeather.Cloud_Colour, targetWeather.Cloud_Colour, transitionProgress);
        CurrentWeatherState.Water_Lightness = Mathf.Lerp(startWeather.Water_Lightness, targetWeather.Water_Lightness, transitionProgress);
    }

    private void ApplyWeatherState(WeatherState targetWeather)
    {
        // ֱ��Ӧ��Ŀ������״̬�ĸ�������
        Mat_SkyboxObject.SetColor("_ColorA", targetWeather.Skybox_Colour[0]);
        Mat_SkyboxObject.SetColor("_ColorB", targetWeather.Skybox_Colour[1]);
        Mat_TerrainObject.SetColor("_Color", targetWeather.Terrain_Colour);
        Mat_Water.SetFloat("__2", targetWeather.Water_Lightness);
        if (managerhub.cloudManager != null)
            managerhub.cloudManager.SetCloudColor(targetWeather.Cloud_Colour);
        Sprite_Sun.color = targetWeather.Sun_Colour;

        // ���µ�ǰ����״̬
        CurrentWeatherState = targetWeather;
    }

    private void ApplyWeatherState(WeatherState Last_Weather, WeatherState Next_Weather, float _value)
    {
        // ʹ�� Lerp ���в�ֵ����
        Color skyboxColorA = Color.Lerp(Last_Weather.Skybox_Colour[0], Next_Weather.Skybox_Colour[0], _value);
        Color skyboxColorB = Color.Lerp(Last_Weather.Skybox_Colour[1], Next_Weather.Skybox_Colour[1], _value);
        Color terrainColor = Color.Lerp(Last_Weather.Terrain_Colour, Next_Weather.Terrain_Colour, _value);
        Color cloudColor = Color.Lerp(Last_Weather.Cloud_Colour, Next_Weather.Cloud_Colour, _value);
        Color sunColor = Color.Lerp(Last_Weather.Sun_Colour, Next_Weather.Sun_Colour, _value);

        float waterLightness = Mathf.Lerp(Last_Weather.Water_Lightness, Next_Weather.Water_Lightness, _value);

        // ���²��ʵ���ɫ����������
        Mat_SkyboxObject.SetColor("_ColorA", skyboxColorA);
        Mat_SkyboxObject.SetColor("_ColorB", skyboxColorB);
        Mat_TerrainObject.SetColor("_Color", terrainColor);
        Mat_Water.SetFloat("__2", waterLightness);

        if (managerhub.cloudManager != null)
            managerhub.cloudManager.SetCloudColor(cloudColor);

        Sprite_Sun.color = sunColor;

        // ���µ�ǰ����״̬
        CurrentWeatherState.Skybox_Colour[0] = skyboxColorA;
        CurrentWeatherState.Skybox_Colour[1] = skyboxColorB;
        CurrentWeatherState.Terrain_Colour = terrainColor;
        CurrentWeatherState.Cloud_Colour = cloudColor;
        CurrentWeatherState.Sun_Colour = sunColor;
        CurrentWeatherState.Water_Lightness = waterLightness;
    }



    #endregion


    #region [������Ҫ��]��ҹѭ��

    /// <summary>
    /// ������Ϸ����
    /// </summary>
    void UpdateGameTime_Object()
    {

        //��ǰ���� - ����������򲻸���
        if (managerhub.weather.weather == Enum_Weather.Rainy)
            return;


        // ���ֵ���������������ı�
        // ����Objects
        if (HasSignificantTimeChange() || ShouldUpdateObjects())
            UpdateAll();

        // ���浱ǰʱ��
        gameTime.previous_CurrentTime = gameTime.CurrentTime;
    }

    /// <summary>
    /// ����Ƿ���Ҫ���¶���
    /// </summary>
    /// <returns>�Ƿ���Ҫ����</returns>
    private bool ShouldUpdateObjects()
    {
        return gameTime._value != 0 && gameTime._value != 1;
    }

    void Update_Value()
    {

        if ((gameTime.CurrentTime >= gameTime.�쿪ʼ����.x && gameTime.CurrentTime <= gameTime.�쿪ʼ����.y) ||
            (gameTime.CurrentTime >= gameTime.�쿪ʼ���.x && gameTime.CurrentTime <= gameTime.�쿪ʼ���.y)
            )
        {
            if (gameTime.CurrentTime < 12)
            {
                gameTime._value = Mathf.InverseLerp(
                    gameTime.�쿪ʼ����.x,
                    gameTime.�쿪ʼ����.y,
                    gameTime.CurrentTime
                );
            }
            else
            {
                gameTime._value = 1 - Mathf.InverseLerp(
                    gameTime.�쿪ʼ���.x,
                    gameTime.�쿪ʼ���.y,
                    gameTime.CurrentTime
                );
            }
        }
        else if (gameTime.CurrentTime >= gameTime.�쿪ʼ����.y && gameTime.CurrentTime <= gameTime.�쿪ʼ���.x)
        {
            gameTime._value = 1;
        }
        else
        {
            gameTime._value = 0;
        }


    }



    /// <summary>
    /// ��������Object
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

        // 0 ~ 0.5���� Night �� SunSet
        if (gameTime._value >= 0f && gameTime._value < 0.5f)
        {
            float t = gameTime._value / 0.5f;  // ��һ������ (0 ~ 1)
            ALerpColor = Color.Lerp(WeatherStates[TimeData.Night].Skybox_Colour[0], WeatherStates[TimeData.Sunset].Skybox_Colour[0], t);
            BLerpColor = Color.Lerp(WeatherStates[TimeData.Night].Skybox_Colour[1], WeatherStates[TimeData.Sunset].Skybox_Colour[1], t);
            CloudColor = Color.Lerp(WeatherStates[TimeData.Night].Skybox_Colour[1], WeatherStates[TimeData.Sunset].Skybox_Colour[1], t - 0.1f);
        }
        // 0.5 ~ 1���� SunSet �� Day
        else if (gameTime._value >= 0.5f && gameTime._value <= 1f)
        {
            float t = (gameTime._value - 0.5f) / 0.5f;  // ��һ������ (0 ~ 1)
            ALerpColor = Color.Lerp(WeatherStates[TimeData.Sunset].Skybox_Colour[0], WeatherStates[TimeData.Day].Skybox_Colour[0], t);
            BLerpColor = Color.Lerp(WeatherStates[TimeData.Sunset].Skybox_Colour[1], WeatherStates[TimeData.Day].Skybox_Colour[1], t);
            CloudColor = Color.Lerp(WeatherStates[TimeData.Sunset].Skybox_Colour[1], WeatherStates[TimeData.Day].Skybox_Colour[1], t - 0.1f);
        }
        else
        {
            return;  // ������Χ����������
        }

        // Ӧ�ü�������ɫ
        Mat_SkyboxObject.SetColor("_ColorA", ALerpColor);
        Mat_SkyboxObject.SetColor("_ColorB", BLerpColor);

        // �����Ƶ���ɫ


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

//����ģ����
[System.Serializable]
public class WeatherState
{
    public string name;
    public Color[] Skybox_Colour = new Color[2];
    public Color Terrain_Colour;
    public Color Cloud_Colour;
    public float Water_Lightness;
    public Color Sun_Colour;

    // ���캯��
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

    // �����������ݽṹ�ĸ���
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

