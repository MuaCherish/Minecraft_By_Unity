using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class TimeManager : MonoBehaviour
{

    [Header("引用")]
    public ManagerHub managerhub;

    [Header("时间参数")]
    [Range(0, 24)] public float CurrentTime = 12;
    [SerializeField, ReadOnly] private float value;
    public float second_GameOneHour = 60; // 现实中多少秒过完游戏内一小时

    [Header("矿洞迷雾")]
    public float checkInterval = 10f; private float nextCheckTime = 0;// 每0.5秒检查一次
    public float fogTransitionTime = 1f; // 迷雾过渡时间
    public Vector2 FogDayDistance;  //迷雾距离
    public Vector2 FogCaveDistance;  //迷雾距离

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
        // 启动条件
        if (managerhub.world.game_state == Game_State.Playing)
        {
            if (TimeCoroutine == null)
            {
                TimeCoroutine = StartCoroutine(UpdateTime());
            }

            // 每隔一段时间检查一次
            //if (Time.time >= nextCheckTime)
            //{
            //    CheckCaveFog();
            //    nextCheckTime = Time.time + checkInterval; // 设置下一次检查的时间
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


    //封装成buff
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

            // Lerp 颜色和距离
            RenderSettings.fogColor = Color.Lerp(startColor, targetColor, t);
            RenderSettings.fogStartDistance = Mathf.Lerp(startDistance.x, targetDistance.x, t);
            RenderSettings.fogEndDistance = Mathf.Lerp(startDistance.y, targetDistance.y, t);

            yield return null;
        }

        // 确保最终值正确
        RenderSettings.fogColor = targetColor;
        RenderSettings.fogStartDistance = targetDistance.x;
        RenderSettings.fogEndDistance = targetDistance.y;
    }

    // 时间流逝
    private Coroutine TimeCoroutine; //24小时制的时间
    private IEnumerator UpdateTime()
    {
        while (true)
        {
            // 等待一帧
            yield return null;

            // 计算每帧应该增加的游戏时间
            float elapsedTime = Time.deltaTime / second_GameOneHour;

            // 增加游戏时间
            CurrentTime += elapsedTime;

            // 确保时间在 0-24 小时之间循环
            if (CurrentTime >= 24)
            {
                CurrentTime -= 24;
            }

            // 开始过渡
            if ((CurrentTime >= 5 && CurrentTime <= 7) || (CurrentTime >= 17 && CurrentTime <= 19))
            {
                //Value百分值
                if (CurrentTime < 12)
                {
                    value = Mathf.InverseLerp(5, 7, CurrentTime);
                }
                else
                {
                    value = 1 - Mathf.InverseLerp(17, 19, CurrentTime);
                }

                //改变
                SkyboxMaterial.SetFloat("_Exposure", value);

                if (managerhub.player.isInCave == false)
                {
                    RenderSettings.fogColor = Color.Lerp(FogNightColor, FogDayColor, value);
                }
                
                BlocksMaterial.color = Color.Lerp(BlocksNightColor, BlocksDayColor, value);
            }

            // 关闭条件
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


    // 初始化
    public void InitTimeManager()
    {
        CurrentTime = 12;
    }
}
