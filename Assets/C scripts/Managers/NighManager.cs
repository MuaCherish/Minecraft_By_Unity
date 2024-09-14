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
    //以秒为单位
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


    [Header("云朵移动")]
    public GameObject clouds;
    public Transform playerLocation;
    private SpriteRenderer spriteRenderer;
    public float speed = 5f;
    public float maxDistance = 1500f;
    public Color DaylightColor;
    public Color NightColor;

    //协程
    Coroutine skybox;
    Coroutine lights;

    //一次性代码
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

    

    //设置天空盒颜色
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
                elapsedTime += DelayTime; // 每秒增加1秒
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
                elapsedTime += DelayTime; // 每秒增加1秒
            }
            RenderSettings.skybox.SetFloat("_Exposure", skyboxMax);
        }
       
    }


    //设置光源渐变
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
                elapsedTime += DelayTime; // 每秒增加1秒
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
                elapsedTime += DelayTime; // 每秒增加1秒
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
                elapsedTime += DelayTime; // 每秒增加1秒
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
                elapsedTime += DelayTime; // 每秒增加1秒
            }
            directionalLight_2.intensity = light2Max;
            
        }
    }


    void InitClouds()
    {

        // 记录初始位置
        Vector3 startPosition = new Vector3(playerLocation.position.x, clouds.transform.position.y, playerLocation.position.z);
        clouds.transform.position = startPosition;


        // 随机生成XOZ平面上的一个方向
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

        // 计算目标位置
        Vector3 targetPosition = startPosition + randomDirection * maxDistance;

        //显示
        clouds.SetActive(true);

        // 开始协程，传递必要的参数
        StartCoroutine(MoveBackAndForth(startPosition, targetPosition));
    }

    IEnumerator MoveBackAndForth(Vector3 startPosition, Vector3 targetPosition)
    {
        bool movingToTarget = true;

        while (true)
        {

            // 计算当前的目标位置
            Vector3 target = movingToTarget ? targetPosition : startPosition;

            // 移动物体
            clouds.transform.position = Vector3.MoveTowards(clouds.transform.position, target, speed * Time.deltaTime);

            // 如果到达目标位置，则切换方向
            if (Vector3.Distance(clouds.transform.position, target) < 0.01f)
            {
                movingToTarget = !movingToTarget;
            }

            // 等待下一帧
            yield return null;
        }
    }


    IEnumerator ColorSwitchRoutine()
    {
        while (true)
        {
            // 等待10秒
            yield return new WaitForSeconds(DayTime);

            // 插值颜色从DaylightColor到NightColor
            yield return StartCoroutine(LerpColor(DaylightColor, NightColor, switchTime));

            // 再次等待10秒
            yield return new WaitForSeconds(NightTime);

            // 插值颜色从NightColor到DaylightColor
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
                yield break; // 如果 spriteRenderer 不存在，则退出协程
            }

            // 计算插值颜色
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / duration);

            // 增加已用时间
            elapsedTime += Time.deltaTime;

            // 等待下一帧
            yield return null;
        }

        // 确保颜色最终设置为目标颜色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = endColor;
        }
    }
}

