using System;
using UnityEngine;


[Serializable]
public class GameTime
{
    [Header("当前时间"), Range(0, 24)] public float CurrentTime = 12;
    [ReadOnly] public float _value = 0; 
    [HideInInspector] public float previous_CurrentTime = 12; 
    [Header("游戏内一小时 = 现实多少秒")] public float second_GameOneHour = 60; // 现实中多少秒过完游戏内一小时

    [Space]
    [Space]
    [Header("//注意白天和完善过度范围的大小必须要相等，不然昼夜循环可能会出问题")]
    [Header("白天过渡范围")] public Vector2 天开始变亮;
    [Header("晚上过渡范围")] public Vector2 天开始变黑;
}


[Serializable]
public class TimeData
{
    public static readonly Byte Day = 0;
    public static readonly Byte Night = 1;
    public static readonly Byte Sunset = 2;
    public static readonly Byte Rain = 3;

}
