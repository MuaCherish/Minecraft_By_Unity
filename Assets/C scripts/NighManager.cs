using System.Collections;
using System.Collections.Generic;
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

    //协程
    Coroutine skybox;
    Coroutine lights;

    //一次性代码
    bool hasExec_calltoPrompt = true;

    private void Start()
    {
        RenderSettings.skybox.SetFloat("_Exposure", skyboxMax);
    }

    private void Update()
    {
        if (world.game_state == Game_State.Playing && skybox == null)
        {
            skybox = StartCoroutine(SetSkybox());
        }

        if (world.game_state == Game_State.Playing && lights == null)
        {
            lights = StartCoroutine(SetLights());
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
            if (hasExec_calltoPrompt)
            {
                canvasManager.First_Prompt_PlayerThe_Flashlight();
                hasExec_calltoPrompt = false;
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
                float t = elapsedTime / switchTime;

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
                float t = elapsedTime / switchTime;

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
}
