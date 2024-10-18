using System;
using UnityEngine;

/// <summary>
/// 总控类
/// </summary>
[Serializable]
public class TimeManagertruct 
{
    [Header("Time参数")] public Time_TimeStruct _time;
    [Header("Fog-Skybox参数")] public Time_FogStruct _fog;
    [Header("Cloud参数")] public Time_CloudStruct _cloud;
    [Header("Terrain参数")] public Time_TerrainStruct _Terrain;
}

#region 时间参数

[Serializable]
public class Time_TimeStruct
{
    [Header("当前时间"), Range(0, 24)] public float CurrentTime = 12; [HideInInspector] public float previous_CurrentTime = 12; [HideInInspector] public float value;
    [Header("游戏内一小时 = 现实多少秒")] public float second_GameOneHour = 60; // 现实中多少秒过完游戏内一小时

    [Header("白天过渡范围")] public Vector2 天开始变黑;
    [Header("晚上过渡范围")] public Vector2 天开始变亮;
}

[Serializable]
public class Time_FogStruct
{
    [Header("天空盒引用")] public Material SkyboxMaterial;
    [Header("迷雾白天颜色")] public Color FogDayColor;
    [Header("迷雾晚上颜色")] public Color FogNightColor;
    [Header("迷雾白天范围")] public Vector2 FogDayDistance;  // 迷雾距离
    [Header("迷雾矿洞范围")] public Vector2 FogCaveDistance;  // 迷雾距离
}

[Serializable]
public class Time_CloudStruct
{
    [Header("白天颜色")] public Color CloudDayColor;
    [Header("晚上颜色")] public Color CloudNightColor;
}


[Serializable]
public class Time_TerrainStruct
{
    [Header("地形材质引用")] public Material BlocksMaterial;
    [Header("白天颜色")] public Color BlocksDayColor;
    [Header("晚上颜色")] public Color BlocksNightColor;
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