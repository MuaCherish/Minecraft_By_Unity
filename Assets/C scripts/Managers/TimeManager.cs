using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class TimeManager : MonoBehaviour
{

    [Header("����")]
    public ManagerHub managerhub;

    [Header("ʱ�����")]
    [Range(0, 24)] public float CurrentTime = 12;
    [SerializeField, ReadOnly] private float value;
    public float second_GameOneHour = 60; // ��ʵ�ж����������Ϸ��һСʱ

    [Header("������")]
    public float checkInterval = 10f; private float nextCheckTime = 0;// ÿ0.5����һ��
    public float fogTransitionTime = 1f; // �������ʱ��
    public Vector2 FogDayDistance;  //�������
    public Vector2 FogCaveDistance;  //�������

    [Header("Skybox")]
    public Material SkyboxMaterial;
    public Vector2 SkyboxRange = new Vector2(0f, 1f);

    [Header("Fog")]
    public Transform eyes;
    public Color FogDayColor;
    public Color FogNightColor;
    

    [Header("Blocks")]
    public Material BlocksMaterial;
    public Color BlocksDayColor;
    public Color BlocksNightColor;


    private void Start()
    {
        eyes = managerhub.player.GetEyesPosition();
    }


    

    private void Update()
    {
        // ��������
        if (managerhub.world.game_state == Game_State.Playing)
        {
            if (TimeCoroutine == null)
            {
                TimeCoroutine = StartCoroutine(UpdateTime());
            }

            // ÿ��һ��ʱ����һ��
            //if (Time.time >= nextCheckTime)
            //{
            //    CheckCaveFog();
            //    nextCheckTime = Time.time + checkInterval; // ������һ�μ���ʱ��
            //}

            if (managerhub.player.isInCave)
            {
                Buff_CaveFog(true);
            }
            else
            {
                Buff_CaveFog(false);
            }

        }
    }


    //��װ��buff
    public void Buff_CaveFog(bool _Open)
    {
        StartCoroutine(TransitionFog(_Open));
    }


    private IEnumerator TransitionFog(bool enterCave)
    {
        float elapsedTime = 0f;
        Color startColor = RenderSettings.fogColor;
        Vector2 startDistance = new Vector2(RenderSettings.fogStartDistance, RenderSettings.fogEndDistance);

        Color targetColor = enterCave ? FogNightColor : FogDayColor;
        Vector2 targetDistance = enterCave ? FogCaveDistance : FogDayDistance;

        while (elapsedTime < fogTransitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fogTransitionTime;

            // Lerp ��ɫ�;���
            RenderSettings.fogColor = Color.Lerp(startColor, targetColor, t);
            RenderSettings.fogStartDistance = Mathf.Lerp(startDistance.x, targetDistance.x, t);
            RenderSettings.fogEndDistance = Mathf.Lerp(startDistance.y, targetDistance.y, t);

            yield return null;
        }

        // ȷ������ֵ��ȷ
        RenderSettings.fogColor = targetColor;
        RenderSettings.fogStartDistance = targetDistance.x;
        RenderSettings.fogEndDistance = targetDistance.y;
    }

    // ʱ������
    private Coroutine TimeCoroutine; //24Сʱ�Ƶ�ʱ��
    private IEnumerator UpdateTime()
    {
        while (true)
        {
            // �ȴ�һ֡
            yield return null;

            // ����ÿ֡Ӧ�����ӵ���Ϸʱ��
            float elapsedTime = Time.deltaTime / second_GameOneHour;

            // ������Ϸʱ��
            CurrentTime += elapsedTime;

            // ȷ��ʱ���� 0-24 Сʱ֮��ѭ��
            if (CurrentTime >= 24)
            {
                CurrentTime -= 24;
            }

            // ��ʼ����
            if ((CurrentTime >= 5 && CurrentTime <= 7) || (CurrentTime >= 17 && CurrentTime <= 19))
            {
                //Value�ٷ�ֵ
                if (CurrentTime < 12)
                {
                    value = Mathf.InverseLerp(5, 7, CurrentTime);
                }
                else
                {
                    value = 1 - Mathf.InverseLerp(17, 19, CurrentTime);
                }

                //�ı�
                SkyboxMaterial.SetFloat("_Exposure", value);

                if (managerhub.player.isInCave == false)
                {
                    RenderSettings.fogColor = Color.Lerp(FogNightColor, FogDayColor, value);
                }
                
                BlocksMaterial.color = Color.Lerp(BlocksNightColor, BlocksDayColor, value);
            }

            // �ر�����
            if (managerhub.world.game_state != Game_State.Playing)
            {
                TimeCoroutine = null;
                InitTimeManager();
                yield break;
            }
        }
    }


    private void OnApplicationQuit()
    {
        BlocksMaterial.color = BlocksDayColor;
        SkyboxMaterial.SetFloat("_Exposure", 1);
    }


    // ��ʼ��
    public void InitTimeManager()
    {
        CurrentTime = 12;
    }
}
