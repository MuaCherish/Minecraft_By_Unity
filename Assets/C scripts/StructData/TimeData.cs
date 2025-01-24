using System;
using UnityEngine;


[Serializable]
public class GameTime
{
    [Header("��ǰʱ��"), Range(0, 24)] public float CurrentTime = 12;
    [ReadOnly] public float _value = 0; 
    [HideInInspector] public float previous_CurrentTime = 12; 
    [Header("��Ϸ��һСʱ = ��ʵ������")] public float second_GameOneHour = 60; // ��ʵ�ж����������Ϸ��һСʱ

    [Space]
    [Space]
    [Header("//ע���������ƹ��ȷ�Χ�Ĵ�С����Ҫ��ȣ���Ȼ��ҹѭ�����ܻ������")]
    [Header("������ɷ�Χ")] public Vector2 �쿪ʼ����;
    [Header("���Ϲ��ɷ�Χ")] public Vector2 �쿪ʼ���;
}


[Serializable]
public class TimeData
{
    public static readonly Byte Day = 0;
    public static readonly Byte Night = 1;
    public static readonly Byte Sunset = 2;
    public static readonly Byte Rain = 3;

}
