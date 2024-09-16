using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;
//using static UnityEditor.Progress;

public class TimeManager : MonoBehaviour
{
    [Header("״̬")]
    [ReadOnly]public bool isNight;

    [Header("����")]
    public ManagerHub managerhub;

    [Header("ʱ�����")]
    [Range(0, 24),SerializeField] private float CurrentTime = 12; private float previous_CurrentTime = 12;
    [SerializeField, ReadOnly] private float value;
    public float second_GameOneHour = 60; // ��ʵ�ж����������Ϸ��һСʱ
    public Vector2 �쿪ʼ���;
    public Vector2 �쿪ʼ����;

    [Header("������")]
    //public float checkInterval = 10f; private float nextCheckTime = 0;// ÿ0.5����һ��
    public float fogTransitionTime = 3f; // �������ʱ��
    public Vector2 FogDayDistance;  //�������
    public Vector2 FogCaveDistance;  //�������

    [Header("Skybox")]
    public Material SkyboxMaterial;
    public Vector2 SkyboxRange = new Vector2(0.2f, 1f);

    [Header("Fog")]
    public Color FogDayColor;
    public Color FogNightColor;
    

    [Header("Blocks")]
    public Material BlocksMaterial;
    public Color BlocksDayColor;
    public Color BlocksNightColor;


    


    //---------------------------------��Ҫ����-------------------------------------

    // ��ʼ��
    public void InitTimeManager()
    {
        CurrentTime = 12;
    }

    //����ϵͳʱ��
    public void SetTime(float _time)
    {
        CurrentTime = _time;
    }

    //�����Ұ���
    public void Buff_CaveFog(bool _Open)
    {
        if (TransitionFogCoroutine == null && isNight)
        {
            TransitionFogCoroutine = StartCoroutine(TransitionFog(_Open));
        }

    }

    //������Ⱦ��Χ�����������
    public void UpdateDayFogDistance()
    {
        float _renderSize = managerhub.canvasManager.world.renderSize;
        float _t = _renderSize * 10 - 10f;


        FogDayDistance = new Vector2(_t / 4, _t);

        RenderSettings.fogStartDistance = FogDayDistance.x;
        RenderSettings.fogEndDistance = FogDayDistance.y;
    }


    public float GetCurrentTime()
    {
        return CurrentTime;
    }


    //------------------------------------------------------------------------------






    //---------------------------------��Ҫ����-------------------------------------

    private bool haeExec_Update = true;
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
                UpdateDayFogDistance();

                haeExec_Update = false;
            }

            if (CurrentTime <= 6 || CurrentTime >= 18)
            {
                isNight = true;
            }
            else
            {
                isNight = false;
            }
            

            //if (managerhub.player.isInCave) 
            //{
            //    Buff_CaveFog(true);
            //}
            //else
            //{
            //    Buff_CaveFog(false);
            //}

        }
    }


    private void OnApplicationQuit()
    {
        BlocksMaterial.color = BlocksDayColor;
        SkyboxMaterial.SetFloat("_Exposure", 1);
    }

    //------------------------------------------------------------------------------






    //---------------------------------Э��-------------------------------------

    //��ɫ����Э��
    private Coroutine TransitionFogCoroutine;
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
        TransitionFogCoroutine = null;
    }

    // ʱ������Э��
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

            //----------------------------------------�̶�˳��


            //���ֵ��;���ִ���ȴ۸ģ�����������ֵ
            if (Mathf.Abs(CurrentTime - previous_CurrentTime) > 1)
            {
                if (!isNight)
                {
                    value = 1;
                }
                else
                {
                    value = 0;
                }

                SkyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(SkyboxRange.x, SkyboxRange.y, value));

                if (managerhub.player.isInCave == false)
                {
                    RenderSettings.fogColor = Color.Lerp(FogNightColor, FogDayColor, value);
                }

                BlocksMaterial.color = Color.Lerp(BlocksNightColor, BlocksDayColor, value);
            }
            previous_CurrentTime = CurrentTime;

            //----------------------------------------

            // ��ʼ����
            if ((CurrentTime >= �쿪ʼ����.x && CurrentTime <= �쿪ʼ����.y) || (CurrentTime >= �쿪ʼ���.x && CurrentTime <= �쿪ʼ���.y))
            {
                //Value�ٷ�ֵ
                if (CurrentTime < 12) 
                {
                    value = Mathf.InverseLerp(�쿪ʼ����.x, �쿪ʼ����.y, CurrentTime);
                }
                else
                {
                    value = 1 - Mathf.InverseLerp(�쿪ʼ���.x, �쿪ʼ���.y, CurrentTime);
                }

                //�ı� 
                SkyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(SkyboxRange.x, SkyboxRange.y, value));

                if (managerhub.player.isInCave == false)
                {
                    RenderSettings.fogColor = Color.Lerp(FogNightColor, FogDayColor, value);
                }
                
                BlocksMaterial.color = Color.Lerp(BlocksNightColor, BlocksDayColor, value);
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

    //------------------------------------------------------------------------------

}
