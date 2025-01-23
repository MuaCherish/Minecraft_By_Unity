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

    #region ״̬

    [Foldout("״̬", true)]
    [Header("��ǰ����")] [ReadOnly] public Enum_Weather weather;
    [Header("�Ƿ��������")][ReadOnly] public bool isTransitioningToRain = false; // ����Ƿ����ڹ��ɵ�����
    [Header("�Ƿ���������")] [ReadOnly] public bool isRaining = false; // ����Ƿ���������

    #endregion 


    #region ���ں���

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


    #region ����ϵͳ


    [Foldout("Transform", true)]
    [Header("RainObject")] public GameObject RainObject;

    [Foldout("����ϵͳ", true)]
    [Header("���������")] public float weatherCheckInterval = 10f; // ���������ʱ����
    [Header("�������")] [Range(0, 1)] public float weather_Cloudy_robability = 0.3f; // 30%�ĸ��ʱ��Cloudy
    [Header("����ʱ��")] public float transitionDuration = 5f; // ʱ��ֵ���ɵ���ʱ��
    [Header("�������ʱ�䷶Χ")] public Vector2 Range_rainDuration = new Vector2(10f, 30f); // �����ʱ��


    // ����ı�����ΪCloudy�ļ��ʱ��͸���

    
    float lastWeatherCheckTime;
    float rainDuration = 60f; // �����ʱ��
    //������
    void CheckWeatherTransition()
    {
        //��ǰ����-����Ѿ������� 
        if (weather == Enum_Weather.Rainy)
            return;


        if (Time.time - lastWeatherCheckTime > weatherCheckInterval)
        {
            lastWeatherCheckTime = Time.time;

            if (weather == Enum_Weather.Sunshine && Random.value < weather_Cloudy_robability)
            {
                weather = Enum_Weather.Rainy;
                rainDuration = Random.Range(Range_rainDuration.x, Range_rainDuration.y); // ������������ʱ��
                haExec_StartToRain = true;
            }
        }
    }

    /// <summary>
    /// ����Cloudy�������߼���ʱ��ֵ�𽥹��㡢���꣬������������ָ�
    /// </summary>
    float elapsedTime; // �ۼƵĹ���ʱ��
    float initialTimeValue; // ��ʼ����ʱ��ʱ��ֵ
    float endTimeValue; // ��������ʱ��ʱ��ֵ
    bool haExec_StartToRain = false;

    //����ǰҡ
    void HandleCloudyWeather()
    {
        //��ǰ����-��������
        if (weather != Enum_Weather.Rainy)
            return;

        //���� -> ���
        if (haExec_StartToRain)
        {
            //print("true");
            isTransitioningToRain = true;
            elapsedTime = 0f;
            initialTimeValue = timemanager.timeStruct._time.value;
            haExec_StartToRain = false;
        }

        //���
        if (isTransitioningToRain) 
        {
            elapsedTime += Time.deltaTime;
            timemanager.timeStruct._time.value = Mathf.Lerp(initialTimeValue, 0, elapsedTime / transitionDuration);
            timemanager.UpdateAll();

            if (elapsedTime >= transitionDuration)
            {
                // ʱ�������ɣ���ʼ����
                isTransitioningToRain = false;
                StartRain();
            }
        }

        //����
        else if (isRaining)
        {
            rainDuration -= Time.deltaTime;
            if (rainDuration <= 0)
            {
                // ��ͣ����ʼ�ָ�ʱ��ֵ
                isRaining = false;
                elapsedTime = 0f;
                endTimeValue = timemanager.Get_Value(timemanager.GetCurrentTime());
            }
        }

        //����
        else
        {
            // ��ͣ��ָ�ʱ��ֵ
            //print(elapsedTime);
            elapsedTime += Time.deltaTime;
            timemanager.timeStruct._time.value = Mathf.Lerp(0, endTimeValue, elapsedTime / transitionDuration);
            timemanager.UpdateAll();

            if (elapsedTime >= transitionDuration)
            {
                // ʱ��ֵ�ָ���ɣ��л�������
                weather = Enum_Weather.Sunshine;
                RainObject.GetComponent<Rain>().StopRain();
            }
        }
    }

    // ��ʼ����
    void StartRain()
    {
        isRaining = true;
        RainObject.SetActive(true);
        RainObject.GetComponent<Rain>().StartRain();
        managerhub.NewmusicManager.SwitchBackgroundMusic(MusicData.Rain);
    }


    #endregion
}
