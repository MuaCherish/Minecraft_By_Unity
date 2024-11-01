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

    #region ״̬

    [Foldout("״̬", true)]
    //[Header("��ǰʱ��(24Сʱ��)")][Range(0, 24), SerializeField] private float CurrentTime = 12;
    [Header("�Ƿ�������")][ReadOnly] public bool isNight;

    #endregion


    #region ���ں���

    private ManagerHub managerhub;
    private bool haeExec_Update = true;


    private void Start()
    {
        managerhub = GlobalData.GetManagerhub();
        InitTimeManager();


        //WeatherCheck();

        

    }


    [Foldout("����", true)]
    [Header("�߿�����")] public GameObject SkyParent;
    private void Update()
    {
        // ��������
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


    #region ״̬�ж�

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


    #region ������

    
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

    //����ϵͳʱ��
    public void SetTime(float _time)
    {
        timeStruct._time.CurrentTime = _time;
    }

    #endregion


    #region ʱ������

    [Foldout("ʱ������", true)]
    public TimeManagertruct timeStruct;
    //public WeatherStruct[] weatherLists;

    // ʱ������Э��
    private Coroutine TimeCoroutine; //24Сʱ�Ƶ�ʱ��
    private IEnumerator UpdateTime()
    {
        while (true)
        {
            // �ȴ�һ֡
            yield return null;

            // ����ÿ֡Ӧ�����ӵ���Ϸʱ��
            float elapsedTime = Time.deltaTime / timeStruct._time.second_GameOneHour;

            // ������Ϸʱ��
            timeStruct._time.CurrentTime += elapsedTime;

            // ȷ��ʱ���� 0-24 Сʱ֮��ѭ��
            if (timeStruct._time.CurrentTime >= 24)
            {
                timeStruct._time.CurrentTime -= 24;
            }

            //----------------------------------------�̶�˳��


            //���ֵ��;���ִ���ȴ۸ģ�����������ֵ
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

            // ��ʼ����
            if ((timeStruct._time.CurrentTime >= timeStruct._time.�쿪ʼ����.x && timeStruct._time.CurrentTime <= timeStruct._time.�쿪ʼ����.y) || (timeStruct._time.CurrentTime >= timeStruct._time.�쿪ʼ���.x && timeStruct._time.CurrentTime <= timeStruct._time.�쿪ʼ���.y))
            {
                //Value�ٷ�ֵ
                if (timeStruct._time.CurrentTime < 12)
                {
                    timeStruct._time.value = Mathf.InverseLerp(timeStruct._time.�쿪ʼ����.x, timeStruct._time.�쿪ʼ����.y, timeStruct._time.CurrentTime);
                }
                else
                {
                    timeStruct._time.value = 1 - Mathf.InverseLerp(timeStruct._time.�쿪ʼ���.x, timeStruct._time.�쿪ʼ���.y, timeStruct._time.CurrentTime);
                }

                //Color LerpFogColor = Color.Lerp(timeStruct._fog.FogNightColor, timeStruct._fog.FogDayColor, timeStruct._time.value);

                //�ı� 
                //timeStruct._Skybox.SkyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(timeStruct._Skybox.SkyboxRange.x, timeStruct._Skybox.SkyboxRange.y, timeStruct._time.value));
                //timeStruct._fog.SkyboxMaterial.SetColor("_Tint", LerpFogColor);

                //if (managerhub.player.isInCave == false)
                //{
                //    RenderSettings.fogColor = LerpFogColor;
                //}
                UpdateAll();


            }



            // �ر�����
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

