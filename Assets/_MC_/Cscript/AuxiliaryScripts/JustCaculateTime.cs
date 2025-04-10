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
    //public ManagerHub managerhub;

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
            float result = MC_Static_Noise.GetTotalNoiseHigh_Biome(i, i, myPosition, worldType, MC_Runtime_StaticData.Instance.BiomeData.biomeProperties);
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"Average time per call: {stopwatch.ElapsedMilliseconds / (float)iterations} ms");
    }

}
