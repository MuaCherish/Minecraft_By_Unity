using Homebrew;
using System;
using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{

    #region ״̬

    [Foldout("״̬", true)]
    [Header("��ǰʱ��(24Сʱ��)")][Range(0, 24), SerializeField] private float CurrentTime = 12;
    [Header("�Ƿ�������")][ReadOnly]public bool isNight;

    #endregion


    #region ���ں���

    private ManagerHub managerhub;

    private void Start()
    {
        managerhub = VoxelData.GetManagerhub();
    }

    #endregion


    [Foldout("ʱ�����", true)]
    [Header("��Ϸ��һСʱ��Ӧ��ʵ������")] public float second_GameOneHour = 60; // ��ʵ�ж����������Ϸ��һСʱ

    [Foldout("��չ��ɲ���", true)]
    [Header("������ɷ�Χ")] public Vector2 �쿪ʼ���;
    [Header("���Ϲ��ɷ�Χ")] public Vector2 �쿪ʼ����;
    private float previous_CurrentTime = 12;
    private float value;

    [Foldout("������ɲ���", true)]
    [Header("���������ɫ")] public Color FogDayColor;
    [Header("����������ɫ")] public Color FogNightColor;
    [Header("�������ʱ��")] public float fogTransitionTime = 3f;  // �������ʱ��
    [Header("������췶Χ")] public Vector2 FogDayDistance;  // �������
    [Header("����󶴷�Χ")] public Vector2 FogCaveDistance;  // �������


    [Foldout("��պй��ɲ���", true)]
    [Header("��պ�����")] public Material SkyboxMaterial;
    [Header("��պй��ɷ�Χ")] public Vector2 SkyboxRange = new Vector2(0.2f, 1f);


    [Foldout("������ɫ���ɲ���", true)]
    [Header("���β�������")] public Material BlocksMaterial;
    [Header("������ɫ")] public Color BlocksDayColor;
    [Header("������ɫ")] public Color BlocksNightColor;



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
        if (TransitionFogCoroutine == null)
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


            if (isOpenBlackFog == false && managerhub.player.isInCave)
            {
                Buff_CaveFog(true);
                isOpenBlackFog = true;
            }
            else if(!isNight && isOpenBlackFog == true && !managerhub.player.isInCave)
            {
                Buff_CaveFog(false);
                isOpenBlackFog = false;
            }

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
    private bool isOpenBlackFog = false;
    private Coroutine TransitionFogCoroutine;
    private IEnumerator TransitionFog(bool enterCave)
    {
        float elapsedTime = 0f;
        Color startColor = RenderSettings.fogColor;
        Vector2 startDistance = new Vector2(RenderSettings.fogStartDistance, RenderSettings.fogEndDistance);

        Color targetColor = enterCave ? FogNightColor : FogDayColor;
        Vector2 targetDistance = enterCave ? FogCaveDistance : FogDayDistance;

        // ��ȡ BlocksMaterial �ĳ�ʼ��ɫ��Ŀ����ɫ
        Color startBlockColor = enterCave ? BlocksDayColor : BlocksNightColor;
        Color targetBlockColor = enterCave ? BlocksNightColor : BlocksDayColor;

        while (elapsedTime < fogTransitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fogTransitionTime;

            // Lerp ��ɫ�;���
            RenderSettings.fogColor = Color.Lerp(startColor, targetColor, t);
            RenderSettings.fogStartDistance = Mathf.Lerp(startDistance.x, targetDistance.x, t);
            RenderSettings.fogEndDistance = Mathf.Lerp(startDistance.y, targetDistance.y, t);

            // Lerp BlocksMaterial ����ɫ
            BlocksMaterial.color = Color.Lerp(startBlockColor, targetBlockColor, t);

            yield return null;
        }

        // ȷ������ֵ��ȷ
        RenderSettings.fogColor = targetColor;
        RenderSettings.fogStartDistance = targetDistance.x;
        RenderSettings.fogEndDistance = targetDistance.y;
        BlocksMaterial.color = targetBlockColor; // ȷ��������ɫ��ȷ
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
