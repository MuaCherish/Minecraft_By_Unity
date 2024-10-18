using System;
using UnityEngine;

/// <summary>
/// �ܿ���
/// </summary>
[Serializable]
public class TimeManagertruct 
{
    [Header("Time����")] public Time_TimeStruct _time;
    [Header("Fog-Skybox����")] public Time_FogStruct _fog;
    [Header("Cloud����")] public Time_CloudStruct _cloud;
    [Header("Terrain����")] public Time_TerrainStruct _Terrain;
}

#region ʱ�����

[Serializable]
public class Time_TimeStruct
{
    [Header("��ǰʱ��"), Range(0, 24)] public float CurrentTime = 12; [HideInInspector] public float previous_CurrentTime = 12; [HideInInspector] public float value;
    [Header("��Ϸ��һСʱ = ��ʵ������")] public float second_GameOneHour = 60; // ��ʵ�ж����������Ϸ��һСʱ

    [Header("������ɷ�Χ")] public Vector2 �쿪ʼ���;
    [Header("���Ϲ��ɷ�Χ")] public Vector2 �쿪ʼ����;
}

[Serializable]
public class Time_FogStruct
{
    [Header("��պ�����")] public Material SkyboxMaterial;
    [Header("���������ɫ")] public Color FogDayColor;
    [Header("����������ɫ")] public Color FogNightColor;
    [Header("������췶Χ")] public Vector2 FogDayDistance;  // �������
    [Header("����󶴷�Χ")] public Vector2 FogCaveDistance;  // �������
}

[Serializable]
public class Time_CloudStruct
{
    [Header("������ɫ")] public Color CloudDayColor;
    [Header("������ɫ")] public Color CloudNightColor;
}


[Serializable]
public class Time_TerrainStruct
{
    [Header("���β�������")] public Material BlocksMaterial;
    [Header("������ɫ")] public Color BlocksDayColor;
    [Header("������ɫ")] public Color BlocksNightColor;
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