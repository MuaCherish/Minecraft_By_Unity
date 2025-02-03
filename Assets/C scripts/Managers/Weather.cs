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

    #region ״̬

    [Foldout("״̬", true)]
    [Header("��ǰ����")] [ReadOnly] public Enum_Weather weather;
    [Header("�Ƿ��������")][ReadOnly] public bool isTransitioningToRain = false; // ����Ƿ����ڹ��ɵ�����
    [Header("�Ƿ���������")] [ReadOnly] public bool isRaining = false; // ����Ƿ���������

    #endregion 


    #region ���ں���

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


    #region ����ת������

    [Foldout("Transform", true)]
    [Header("RainObject")] public GameObject RainObject;

    [Foldout("����ϵͳ", true)]
    [Header("���������")] public float weatherCheckInterval = 10f; // ���������ʱ����                             
   

    // ����ı�����ΪCloudy�ļ��ʱ��͸���


    float lastWeatherCheckTime;
    float rainDuration = 60f; // �����ʱ��
    //������
    void CheckWeatherTransition()
    {
        // ����Ѿ������������ȴʱ��δ������ֱ�ӷ���
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


    #region ����

    [Foldout("����", true)]
    [Header("�������")][Range(0, 1)] public float weather_Cloudy_robability = 0.3f; // 30%�ĸ��ʱ��Cloudy
    [Header("����ʱ��")] public float transitionDuration = 5f; // ʱ��ֵ���ɵ���ʱ��
    [Header("�������ʱ�䷶Χ")] public Vector2 Range_rainDuration = new Vector2(10f, 30f); // �����ʱ��
    [Header("ͷ�����߼��")] public float checkHeadTime = 1f;
    private float HeadTime_elapsedTime = 0f; // ���ڼ�ʱ


    /// <summary>
    /// ������תΪ����
    /// </summary>
    public void SetWeatherRainy()
    {
        weather = Enum_Weather.Rainy;
        rainDuration = Random.Range(Range_rainDuration.x, Range_rainDuration.y); // ������������ʱ��
        haExec_StartToRain = true;
    }

    public void SetWeatherRainy(float _transitionDuration, float _RainDutation)
    {
        weather = Enum_Weather.Rainy;
        rainDuration = _RainDutation; // ������������ʱ��
        transitionDuration = _transitionDuration;
        haExec_StartToRain = true;
    }

    /// <summary>
    /// ����Cloudy�������߼���ʱ��ֵ�𽥹��㡢���꣬������������ָ�
    /// </summary>
    float elapsedTime; // �ۼƵĹ���ʱ��
    //float initialTimeValue; // ��ʼ����ʱ��ʱ��ֵ
    //float endTimeValue; // ��������ʱ��ʱ��ֵ
    bool haExec_StartToRain = false;

    void HandleCloudyWeather()
    {
        // ���������죬ֱ�ӷ���
        if (weather != Enum_Weather.Rainy)
            return;

        // ������쵽��ҹ�Ĺ���
        if (haExec_StartToRain)
        {
            isTransitioningToRain = true;
            elapsedTime = 0f;
            haExec_StartToRain = false;
            managerhub.timeManager.SwitchWeatherState(TimeData.Rain, transitionDuration, 1);
            return;
        }

        // �����ҹ������Ĺ���
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

        // �����������
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

        // ������ͣ�����������
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

        // ÿ�� checkHeadTime ����һ��
        if (elapsedTime >= checkHeadTime)
        {
            elapsedTime = 0f; // ���ü�ʱ��

            // ִ�����߼���߼�
            RayCastStruct _rayCast = player.NewRayCast(player.transform.position, Vector3.up, 32f);
            if (_rayCast.isHit == 1)
            {
                // Do something when head is detected
                //print("�����������");
                managerhub.NewmusicManager.SetVolumn_Smooth(0.2f, 0.2f);
            }
            else if(_rayCast.isHit == 0)
            {
                managerhub.NewmusicManager.SetVolumn_Smooth(0.5f, 0.2f);
            }
        }
    }

    // ��ʼ����
    void StartRain()
    {
        isRaining = true;
        RainObject.SetActive(true);
        RainObject.GetComponent<Rain>().StartRain();
        managerhub.NewmusicManager.SwitchBackgroundMusic(MusicData.Rain, 1f, 0.5f);

    }

    #endregion



}
