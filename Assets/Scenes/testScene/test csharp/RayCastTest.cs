using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics; // �������ܼ�ʱ

public class RayCastTest : MonoBehaviour
{
    ManagerHub managerhub;
    Player player;
    private bool hasTested = false; // �Ƿ��Ѳ������

    [Header("���߳���")] public float reach = 5.2f;       // ������󳤶�

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        player = managerhub.player;
    }

    private void Update()
    {
        if (!hasTested && managerhub.world.game_state == Game_State.Playing)
        {
            TestMaxCallsPerFrame();
            hasTested = true; // ���Ϊ�Ѳ��ԣ�ֹͣ����ִ��
        }
    }

    void TestMaxCallsPerFrame()
    {
        // ��ʱ����ʼ
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        int callCount = 0; // ��¼���ô���

        // ���߼�����ѭ��
        while (stopwatch.ElapsedMilliseconds < 16.67f) // Ŀ��ʱ��Ϊһ֡ (16.67ms)
        {
            player.NewRayCast(origin, direction, reach);
            callCount++;
        }

        // ֹͣ��ʱ
        stopwatch.Stop();

        // ������ܱ���
        UnityEngine.Debug.Log($"���ܲ�����ɣ���һ֡��������߼����ô���Ϊ {callCount}");
        UnityEngine.Debug.Log($"���߳�������Ϊ {reach}���������Ϊ {player.checkIncrement}");

        // ���������Ϣ
        AnalyzeRaycastSettings();
    }

    void AnalyzeRaycastSettings()
    {
        // ����������󳤶�
        //float recommendedLength = 20f; // Ĭ���Ƽ� 20
        if (reach > 50)
        {
            UnityEngine.Debug.LogWarning("���߳��ȹ��������ܵ����������⡣���鱣���� 20-50 ��Χ�ڡ�");
        }
        else
        {
            UnityEngine.Debug.Log("���߳����ں���Χ�ڡ�");
        }

        // �������߼���߼�
        //UnityEngine.Debug.Log($"�������ԽС������Խ�ߣ������ܿ��������Ŀǰ����Ϊ {checkIncrement}������������������");

        //// Unity �������߼����Զ������߼�������
        //UnityEngine.Debug.Log("�� Unity �������߼����ȣ�");
        //UnityEngine.Debug.Log("1. Unity �������߼�����֧�����޳������������Ż��������������в�׼ȷ��");
        //UnityEngine.Debug.Log("2. ��ǰ�Զ������߼���������ģ������·����֧�ֶ��⹦�ܣ���ʵ���⣩��");
        //UnityEngine.Debug.Log("3. �Զ������߼�����ܿ����ϴ���������Ƴ���������");
    }
}
