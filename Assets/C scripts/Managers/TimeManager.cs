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


    #region ���ں���

    private ManagerHub managerhub;
    private void Start()
    {
        managerhub = GlobalData.GetManagerhub();
        InitTimeManager();
    }

    private void Update()
    {

        switch (managerhub.world.game_state)
        {
            case Game_State.Start:
                Handle_GameState_Start();
                break;

            case Game_State.Playing:
                Handle_GameState_Playing();
                break;

        }


    }

    void Handle_GameState_Start()
    {
        if (hasExec_Start)
        {
            InitTimeManager();
            

            hasExec_Start = false;
        }
        
    }

    void Handle_GameState_Playing()
    {
        hasExec_Start = true;

        if (TimeCoroutine == null)
            TimeCoroutine = StartCoroutine(UpdateTime());
        //hasExec_TimeCoroutine = true;

    }

    bool hasExec_Start = true;
    public void InitTimeManager()
    {
        //timeStruct._time.CurrentTime = 12f;
        timeStruct._time.CurrentTime = Random.Range(8f, 15f);
        timeStruct._time.previous_CurrentTime = 0f;
        timeStruct._time.value = 1;
        timeStruct._Water.WatersMaterial.SetFloat("__2", timeStruct._Water.LightnessRange.y);
        SkyParent.SetActive(true);

        //if (isRandomWeather)
        //{
        //    weather = (Enum_Weather)Random.Range(0, System.Enum.GetValues(typeof(Enum_Weather)).Length);
        //}

    }


    private void OnApplicationQuit()
    {
        timeStruct._Terrain.BlocksMaterial.color = timeStruct._Terrain.BlocksDayColor;
        //timeStruct._Skybox.SkyboxMaterial.SetFloat("_Exposure", 1);
        //timeStruct._Skybox.SkyboxMaterial.color = timeStruct._fog.FogDayColor;
        InitSkyBox();
    }


    #endregion


    #region ����

    [Foldout("����", true)]
    [Header("Sky����")] public GameObject SkyParent;

    [Foldout("ʱ������", true)]
    public TimeManagertruct timeStruct;


    #endregion



    #region ʱ������

    /// <summary>
    /// �Ƿ�������
    /// </summary>
    /// <returns>�Ƿ�Ϊ����</returns>
    public bool IsNight()
    {
        return IsNight(timeStruct._time.CurrentTime);
    }

    /// <summary>
    /// �Ƿ������ϣ�����ָ��ʱ�䣩
    /// </summary>
    /// <param name="_time">ָ��ʱ��</param>
    /// <returns>�Ƿ�Ϊ����</returns>
    public bool IsNight(float _time)
    {
        return _time <= timeStruct._time.�쿪ʼ����.x || _time >= timeStruct._time.�쿪ʼ���.y;
    }

    /// <summary>
    /// ��ȡ��ǰ��Ϸʱ��
    /// </summary>
    /// <returns>��ǰʱ��</returns>
    public float GetCurrentTime()
    {
        return timeStruct._time.CurrentTime;
    }

    /// <summary>
    /// ���õ�ǰ��Ϸʱ��
    /// </summary>
    /// <param name="_time">Ŀ��ʱ��</param>
    public void SetTime(float _time)
    {
        timeStruct._time.CurrentTime = Mathf.Clamp(_time, 0, 24); // ȷ��ʱ����Ч
    }

    /// <summary>
    /// ��ͣ��ָ�ʱ������
    /// </summary>
    /// <param name="_pause">�Ƿ���ͣ</param>
    public void PauseTime(bool _pause)
    {
        isPauseTime = _pause;
    }

    #endregion

    #region ��Ϸʱ��

    private Coroutine TimeCoroutine; // 24Сʱ�Ƶ�ʱ��Э��
    private bool isPauseTime;

    private IEnumerator UpdateTime()
    {
        while (true)
        {
            // ��ǰ���أ������ͣʱ�������Ϸ��
            if (isPauseTime || !IsGameRunning())
            {
                yield return null;
                continue;
            }

            // ȷ��ʱ���� 0-24 Сʱ֮��ѭ��
            NormalizeTime();

            // ������Ϸʱ��
            UpdateGameTime();

            // ��������ֵ
            UpdateTimeValue();

            // ��� Value �ı���࣬��������
            if (HasSignificantTimeChange())
            {
                UpdateAll();
                Debug.Log($"����һ�Σ�value: {timeStruct._time.value}");
            }

            // ���������Ч������³�������
            if (ShouldUpdateObjects())
            {
                UpdateAll();
            }

            // ���浱ǰʱ��
            timeStruct._time.previous_CurrentTime = timeStruct._time.CurrentTime;

            // �ȴ���һ֡
            yield return null;
        }
    }

    /// <summary>
    /// �����Ϸ�Ƿ���������
    /// </summary>
    /// <returns>�Ƿ�������</returns>
    private bool IsGameRunning()
    {
        if (managerhub.world.game_state != Game_State.Start) return true;

        TimeCoroutine = null; // ֹͣЭ��
        return false;
    }

    /// <summary>
    /// ȷ��ʱ���� 0-24 Сʱ֮��ѭ��
    /// </summary>
    private void NormalizeTime()
    {
        if (timeStruct._time.CurrentTime >= 24)
        {
            timeStruct._time.CurrentTime -= 24;
        }
    }

    /// <summary>
    /// ������Ϸʱ��
    /// </summary>
    private void UpdateGameTime()
    {
        float elapsedTime = Time.deltaTime / timeStruct._time.second_GameOneHour;
        timeStruct._time.CurrentTime += elapsedTime;
    }

    /// <summary>
    /// ��������ֵ
    /// </summary>
    private void UpdateTimeValue()
    {
        if (timeStruct._time.CurrentTime < 12)
        {
            timeStruct._time.value = Mathf.InverseLerp(
                timeStruct._time.�쿪ʼ����.x,
                timeStruct._time.�쿪ʼ����.y,
                timeStruct._time.CurrentTime
            );
        }
        else
        {
            timeStruct._time.value = 1 - Mathf.InverseLerp(
                timeStruct._time.�쿪ʼ���.x,
                timeStruct._time.�쿪ʼ���.y,
                timeStruct._time.CurrentTime
            );
        }
    }

    /// <summary>
    /// ���ʱ���Ƿ��������仯
    /// </summary>
    /// <returns>�Ƿ���Ҫ��������</returns>
    private bool HasSignificantTimeChange()
    {
        const float maxTimeChange = 0.5f; // ����Ϊ�̶�ֵ
        return Mathf.Abs(timeStruct._time.CurrentTime - timeStruct._time.previous_CurrentTime) > maxTimeChange;
    }

    /// <summary>
    /// ����Ƿ���Ҫ���¶���
    /// </summary>
    /// <returns>�Ƿ���Ҫ����</returns>
    private bool ShouldUpdateObjects()
    {
        return timeStruct._time.value != 0 && timeStruct._time.value != 1;
    }






#endregion



    #region UpdateObject


    //��������ʱ��Object
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

        // 0 ~ 0.5���� Night �� SunSet
        if (timeStruct._time.value >= 0f && timeStruct._time.value < 0.5f)
        {
            float t = timeStruct._time.value / 0.5f;  // ��һ������ (0 ~ 1)
            ALerpColor = Color.Lerp(timeStruct._skybox.NightColor[0], timeStruct._skybox.SunSetColor[0], t);
            BLerpColor = Color.Lerp(timeStruct._skybox.NightColor[1], timeStruct._skybox.SunSetColor[1], t);
            CloudColor = Color.Lerp(timeStruct._skybox.NightColor[1], timeStruct._skybox.SunSetColor[1], t - 0.1f);
        }
        // 0.5 ~ 1���� SunSet �� Day
        else if (timeStruct._time.value >= 0.5f && timeStruct._time.value <= 1f)
        {
            float t = (timeStruct._time.value - 0.5f) / 0.5f;  // ��һ������ (0 ~ 1)
            ALerpColor = Color.Lerp(timeStruct._skybox.SunSetColor[0], timeStruct._skybox.DayColor[0], t);
            BLerpColor = Color.Lerp(timeStruct._skybox.SunSetColor[1], timeStruct._skybox.DayColor[1], t);
            CloudColor = Color.Lerp(timeStruct._skybox.SunSetColor[1], timeStruct._skybox.DayColor[1], t - 0.1f);
        }
        else
        {
            return;  // ������Χ����������
        }

        // Ӧ�ü�������ɫ
        timeStruct._skybox.SkyboxMaterial.SetColor("_ColorA", ALerpColor);
        timeStruct._skybox.SkyboxMaterial.SetColor("_ColorB", BLerpColor);

        // �����Ƶ���ɫ


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


    //������Ⱦ��Χ�����������
    //public void UpdateDayFogDistance()
    //{
    //    float _renderSize = managerhub.canvasManager.world.renderSize;
    //    float _t = _renderSize * 10 - 10f;


    //    timeStruct._fog.FogDayDistance = new Vector2(_t * 0.8f, _t);

    //    RenderSettings.fogStartDistance = timeStruct._fog.FogDayDistance.x;
    //    RenderSettings.fogEndDistance = timeStruct._fog.FogDayDistance.y;
    //}



    #endregion


    #region ��պ�

    void InitSkyBox()
    {
        timeStruct._skybox.SkyboxMaterial.SetColor("_ColorA", timeStruct._skybox.DayColor[0]);
        timeStruct._skybox.SkyboxMaterial.SetColor("_ColorB", timeStruct._skybox.DayColor[1]);

    }



    #endregion


    #region ����(�ѽ���)

    //[Foldout("��������", true)]
    //[Header("�������")] public bool isRandomWeather = true;
    //[Header("����")] public Enum_Weather weather = Enum_Weather.Sunshine;
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


    #region Buff_��Ѩģʽ���ѽ��ã�

    //[Foldout("Buff_��Ѩģʽ", true)]
    //[Header("��Ѩģʽ����ʱ��")] public float fogTransitionTime = 3f;  // �������ʱ��

    ////��ɫ����Э��
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
    ///// ��Ѩģʽ���ı�fog��skybox��terrain
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

    //    // ��ȡ BlocksMaterial �ĳ�ʼ��ɫ��Ŀ����ɫ
    //    Color startBlockColor = enterCave ? timeStruct._Terrain.BlocksDayColor : timeStruct._Terrain.BlocksNightColor;
    //    Color targetBlockColor = enterCave ? timeStruct._Terrain.BlocksNightColor : timeStruct._Terrain.BlocksDayColor;

    //    // ��ȡ Skybox �ĳ�ʼ�ع�ֵ��Ŀ���ع�ֵ
    //    //float startExposure = enterCave ? 1f : 0f;
    //    //float targetExposure = enterCave ? 0f : 1f;

    //    while (elapsedTime < fogTransitionTime)
    //    {
    //        elapsedTime += Time.deltaTime;
    //        float t = elapsedTime / fogTransitionTime;

    //        // Lerp ��ɫ�;���
    //        Color FogLerpColor = Color.Lerp(startColor, targetColor, t);
    //        RenderSettings.fogColor = FogLerpColor;
    //        RenderSettings.fogStartDistance = Mathf.Lerp(startDistance.x, targetDistance.x, t);
    //        RenderSettings.fogEndDistance = Mathf.Lerp(startDistance.y, targetDistance.y, t);

    //        // Lerp BlocksMaterial ����ɫ
    //        timeStruct._Terrain.BlocksMaterial.color = Color.Lerp(startBlockColor, targetBlockColor, t);

    //        // Lerp Skybox ���ع�ֵ
    //        //timeStruct._Skybox.SkyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(startExposure, targetExposure, t));
    //        timeStruct._fog.SkyboxMaterial.SetColor("_Tint", FogLerpColor);

    //        yield return null;
    //    }

    //    // ȷ������ֵ��ȷ
    //    RenderSettings.fogColor = targetColor;
    //    RenderSettings.fogStartDistance = targetDistance.x;
    //    RenderSettings.fogEndDistance = targetDistance.y;
    //    timeStruct._Terrain.BlocksMaterial.color = targetBlockColor;
    //    //timeStruct._Skybox.SkyboxMaterial.SetFloat("_Exposure", targetExposure);
    //    timeStruct._fog.SkyboxMaterial.SetColor("_Tint", targetColor);
    //    TransitionFogCoroutine = null;
    //}




    #endregion


    #region Buff_Ǳˮģʽ


    #endregion 

}

