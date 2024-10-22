using System;
using UnityEngine;

/// <summary>
/// �ܿ���
/// </summary>
[Serializable]
public class TimeManagertruct 
{
    [Header("Time����")] public Time_TimeStruct _time;
    [Header("Skybox����")] public Time_SkyBoxStruct _skybox;
    //[Header("Cloud����")] public Time_CloudStruct _cloud;
    [Header("Terrain����")] public Time_TerrainStruct _Terrain;
    [Header("Water����")] public Time_WaterStruct _Water;

}

#region ʱ�����

[Serializable]
public class Time_TimeStruct
{
    [Header("��ǰʱ��"), Range(0, 24)] public float CurrentTime = 12; [HideInInspector] public float previous_CurrentTime = 12; [ReadOnly] public float value;
    [Header("��Ϸ��һСʱ = ��ʵ������")] public float second_GameOneHour = 60; // ��ʵ�ж����������Ϸ��һСʱ

    [Header("������ɷ�Χ")] public Vector2 �쿪ʼ���;
    [Header("���Ϲ��ɷ�Χ")] public Vector2 �쿪ʼ����;
}

[Serializable]
public class Time_SkyBoxStruct
{
    [Header("��պ�����")] public Material SkyboxMaterial;
    [Header("������ɫ")] public Color[] DayColor = new Color[2];
    [Header("��ϼ��ɫ")] public Color[] SunSetColor = new Color[2];
    [Header("������ɫ")] public Color[] NightColor = new Color[2];
    //[Header("������췶Χ")] public Vector2 FogDayDistance;  // �������
    //[Header("����󶴷�Χ")] public Vector2 FogCaveDistance;  // �������
}

//[Serializable]
//public class Time_CloudStruct
//{
//    [Header("������ɫ")] public Color CloudDayColor;
//    [Header("������ɫ")] public Color CloudNightColor;
//}


[Serializable]
public class Time_TerrainStruct
{
    [Header("���β�������")] public Material BlocksMaterial;
    [Header("������ɫ")] public Color BlocksDayColor;
    [Header("������ɫ")] public Color BlocksNightColor;
}

[Serializable]
public class Time_WaterStruct
{
    [Header("ˮ���������")] public Material WatersMaterial;
    [Header("���ȷ�Χ")] public Vector2 LightnessRange = new Vector2(0f, 1f);
}

#endregion


#region ��������

[Serializable]
public class WeatherStruct
{
    [Header("����")] public Enum_Weather weather;
    [Header("������ɫ")] public Color FogDayColor;
    [Header("������ɫ")] public Color FogNightColor;

}

#endregion