using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// �����������ܲ���
/// </summary>
public class CaculateTime : MonoBehaviour
{
    public Vector3 myPosition;
    public int worldType;
    public ManagerHub managerhub;

    void Start()
    {
        MeasurePerformance(1000); // ���� 1000 �ε���
    }

    void MeasurePerformance(int iterations)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < iterations; i++)
        {
            // �����ʹ������� x �� z ֵ���в���
            float result = managerhub.world.GetTotalNoiseHigh_Biome(i, i, myPosition, worldType);
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"Average time per call: {stopwatch.ElapsedMilliseconds / (float)iterations} ms");
    }

}
