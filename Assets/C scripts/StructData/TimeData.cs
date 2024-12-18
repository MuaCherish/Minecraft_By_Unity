using System;
using UnityEngine;

/// <summary>
/// 总控类
/// </summary>
[Serializable]
public class TimeManagertruct 
{
    [Header("Time参数")] public Time_TimeStruct _time;
    [Header("Skybox参数")] public Time_SkyBoxStruct _skybox;
    //[Header("Cloud参数")] public Time_CloudStruct _cloud;
    [Header("Terrain参数")] public Time_TerrainStruct _Terrain;
    [Header("Water参数")] public Time_WaterStruct _Water;

}

#region 时间参数

[Serializable]
public class Time_TimeStruct
{
    [Header("当前时间"), Range(0, 24)] public float CurrentTime = 12; [HideInInspector] public float previous_CurrentTime = 12; [ReadOnly] public float value = 1;
    [Header("游戏内一小时 = 现实多少秒")] public float second_GameOneHour = 60; // 现实中多少秒过完游戏内一小时

    [Header("白天过渡范围")] public Vector2 天开始变黑;
    [Header("晚上过渡范围")] public Vector2 天开始变亮;
}

[Serializable]
public class Time_SkyBoxStruct
{
    [Header("天空盒引用")] public Material SkyboxMaterial;
    [Header("白天颜色")] public Color[] DayColor = new Color[2];
    [Header("晚霞颜色")] public Color[] SunSetColor = new Color[2];
    [Header("晚上颜色")] public Color[] NightColor = new Color[2];
    //[Header("迷雾白天范围")] public Vector2 FogDayDistance;  // 迷雾距离
    //[Header("迷雾矿洞范围")] public Vector2 FogCaveDistance;  // 迷雾距离
}

//[Serializable]
//public class Time_CloudStruct
//{
//    [Header("白天颜色")] public Color CloudDayColor;
//    [Header("晚上颜色")] public Color CloudNightColor;
//}


[Serializable]
public class Time_TerrainStruct
{
    [Header("地形材质引用")] public Material BlocksMaterial;
    [Header("白天颜色")] public Color BlocksDayColor;
    [Header("晚上颜色")] public Color BlocksNightColor;
}

[Serializable]
public class Time_WaterStruct
{
    [Header("水面材质引用")] public Material WatersMaterial;
    [Header("亮度范围")] public Vector2 LightnessRange = new Vector2(0f, 1f);
}

#endregion


#region 天气参数

[Serializable]
public class WeatherStruct
{
    [Header("天气")] public Enum_Weather weather;
    [Header("白天颜色")] public Color FogDayColor;
    [Header("晚上颜色")] public Color FogNightColor;

}

#endregion