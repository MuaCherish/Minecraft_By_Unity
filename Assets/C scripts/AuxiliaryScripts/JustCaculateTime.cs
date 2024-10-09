using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 用来进行性能测试
/// </summary>
public class CaculateTime : MonoBehaviour
{
    public Vector3 myPosition;
    public int worldType;
    public ManagerHub managerhub;

    void Start()
    {
        MeasurePerformance(1000); // 测试 1000 次调用
    }

    void MeasurePerformance(int iterations)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < iterations; i++)
        {
            // 你可以使用任意的 x 和 z 值进行测试
            float result = managerhub.world.GetTotalNoiseHigh_Biome(i, i, myPosition, worldType);
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"Average time per call: {stopwatch.ElapsedMilliseconds / (float)iterations} ms");
    }

}
