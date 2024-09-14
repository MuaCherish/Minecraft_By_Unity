using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.Experimental.GlobalIllumination;

public class NighManager : MonoBehaviour
{
    [Header("Transform")]
    public World world;
    public CanvasManager canvasManager;
    public Light directionalLight_1;
    public Light directionalLight_2;

    [Header("Time")]
    //����Ϊ��λ
    public float DayTime;
    public float NightTime;

    public float switchTime;
    public float DelayTime;

    //skyboxRange
    float skyboxMax = 0.69f;
    float skyboxMin = 0.08f;

    //Light1Range
    float light1Max = 0.7f;
    float light1Min = 0.1f;

    //Light2Range
    float light2Max = 0.4f;
    float light2Min = 0f;


    [Header("�ƶ��ƶ�")]
    public GameObject clouds;
    public Transform playerLocation;
    private SpriteRenderer spriteRenderer;
    public float speed = 5f;
    public float maxDistance = 1500f;
    public Color DaylightColor;
    public Color NightColor;

    //Э��
    Coroutine skybox;
    Coroutine lights;

    //һ���Դ���
    //private bool hasExec_calltoPrompt = true;

    private void Start()
    {
        RenderSettings.skybox.SetFloat("_Exposure", skyboxMax);
        spriteRenderer = clouds.GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (world.game_state == Game_State.Playing && skybox == null)
        {
            skybox = StartCoroutine(SetSkybox());

            InitClouds();
        }

        if (world.game_state == Game_State.Playing && lights == null)
        {
            lights = StartCoroutine(SetLights());

            StartCoroutine(ColorSwitchRoutine());
        }
    }

    

    //������պ���ɫ
    IEnumerator SetSkybox()
    {
        while(true)
        {
            //Daytime
            yield return new WaitForSeconds(DayTime);

            //Switch
            float elapsedTime = 0;
            while (elapsedTime < switchTime)
            {
                float t = elapsedTime / switchTime;
                float currentExposure = Mathf.Lerp(skyboxMax, skyboxMin, t);
                RenderSettings.skybox.SetFloat("_Exposure", currentExposure); 
                yield return new WaitForSeconds(DelayTime + 0.001f);
                elapsedTime += DelayTime; // ÿ������1��
            }
            RenderSettings.skybox.SetFloat("_Exposure", skyboxMin);

            //call
            if (!canvasManager.hasExec_PromptScreen_isShow)
            {
                canvasManager.First_Prompt_PlayerThe_Flashlight();
                //hasExec_calltoPrompt = false;
            }
            
            //Nighttime
            yield return new WaitForSeconds(NightTime);

            //Switch
            elapsedTime = 0;
            while (elapsedTime < switchTime)
            {
                float t = elapsedTime / switchTime;
                float currentExposure = Mathf.Lerp(skyboxMin, skyboxMax, t);
                RenderSettings.skybox.SetFloat("_Exposure", currentExposure);
                yield return new WaitForSeconds(DelayTime + 0.001f);
                elapsedTime += DelayTime; // ÿ������1��
            }
            RenderSettings.skybox.SetFloat("_Exposure", skyboxMax);
        }
       
    }


    //���ù�Դ����
    IEnumerator SetLights()
    {
        while (true)
        {
            //Daytime
            yield return new WaitForSeconds(DayTime);

            //Switch to nighttime
            float elapsedTime = 0;
            //light2
            while (elapsedTime < switchTime * 0.4f)
            {
                float t = elapsedTime / (switchTime * 0.4f);

                float currentExposure = Mathf.Lerp(light2Max, light2Min, t);
                directionalLight_2.intensity = currentExposure;

                yield return new WaitForSeconds(DelayTime);
                elapsedTime += DelayTime; // ÿ������1��
            }
            directionalLight_2.intensity = light2Min;
            //light1
            elapsedTime = 0;
            while (elapsedTime < switchTime * 0.6f)
            {
                float t = elapsedTime / (switchTime * 0.6f);

                float currentExposure = Mathf.Lerp(light1Max, light1Min, t);
                directionalLight_1.intensity = currentExposure;
                 
                yield return new WaitForSeconds(DelayTime);
                elapsedTime += DelayTime; // ÿ������1��
            }
            directionalLight_1.intensity = light1Min;

            //Nighttime
            yield return new WaitForSeconds(NightTime);

            //Switch to daytime
            elapsedTime = 0;
            //light1
            while (elapsedTime < switchTime * 0.6f)
            {
                float t = elapsedTime / switchTime;

                float currentExposure = Mathf.Lerp(light1Min, light1Max, t);
                directionalLight_1.intensity = currentExposure;

                yield return new WaitForSeconds(DelayTime);
                elapsedTime += DelayTime; // ÿ������1��
            }
            directionalLight_1.intensity = light1Max;
            //light2
            elapsedTime = 0;
            while (elapsedTime < switchTime * 0.4f)
            {
                float t = elapsedTime / switchTime;

                float currentExposure = Mathf.Lerp(light2Min, light2Max, t);
                directionalLight_2.intensity = currentExposure;

                yield return new WaitForSeconds(DelayTime);
                elapsedTime += DelayTime; // ÿ������1��
            }
            directionalLight_2.intensity = light2Max;
            
        }
    }


    void InitClouds()
    {

        // ��¼��ʼλ��
        Vector3 startPosition = new Vector3(playerLocation.position.x, clouds.transform.position.y, playerLocation.position.z);
        clouds.transform.position = startPosition;


        // �������XOZƽ���ϵ�һ������
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

        // ����Ŀ��λ��
        Vector3 targetPosition = startPosition + randomDirection * maxDistance;

        //��ʾ
        clouds.SetActive(true);

        // ��ʼЭ�̣����ݱ�Ҫ�Ĳ���
        StartCoroutine(MoveBackAndForth(startPosition, targetPosition));
    }

    IEnumerator MoveBackAndForth(Vector3 startPosition, Vector3 targetPosition)
    {
        bool movingToTarget = true;

        while (true)
        {

            // ���㵱ǰ��Ŀ��λ��
            Vector3 target = movingToTarget ? targetPosition : startPosition;

            // �ƶ�����
            clouds.transform.position = Vector3.MoveTowards(clouds.transform.position, target, speed * Time.deltaTime);

            // �������Ŀ��λ�ã����л�����
            if (Vector3.Distance(clouds.transform.position, target) < 0.01f)
            {
                movingToTarget = !movingToTarget;
            }

            // �ȴ���һ֡
            yield return null;
        }
    }


    IEnumerator ColorSwitchRoutine()
    {
        while (true)
        {
            // �ȴ�10��
            yield return new WaitForSeconds(DayTime);

            // ��ֵ��ɫ��DaylightColor��NightColor
            yield return StartCoroutine(LerpColor(DaylightColor, NightColor, switchTime));

            // �ٴεȴ�10��
            yield return new WaitForSeconds(NightTime);

            // ��ֵ��ɫ��NightColor��DaylightColor
            yield return StartCoroutine(LerpColor(NightColor, DaylightColor, switchTime));

        }
    }

    IEnumerator LerpColor(Color startColor, Color endColor, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (spriteRenderer == null)
            {
                yield break; // ��� spriteRenderer �����ڣ����˳�Э��
            }

            // �����ֵ��ɫ
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / duration);

            // ��������ʱ��
            elapsedTime += Time.deltaTime;

            // �ȴ���һ֡
            yield return null;
        }

        // ȷ����ɫ��������ΪĿ����ɫ
        if (spriteRenderer != null)
        {
            spriteRenderer.color = endColor;
        }
    }
}

