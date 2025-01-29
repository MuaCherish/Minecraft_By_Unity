using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics; // 用于性能计时

public class RayCastTest : MonoBehaviour
{
    ManagerHub managerhub;
    Player player;
    private bool hasTested = false; // 是否已测试完成

    [Header("射线长度")] public float reach = 5.2f;       // 射线最大长度

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
            hasTested = true; // 标记为已测试，停止后续执行
        }
    }

    void TestMaxCallsPerFrame()
    {
        // 计时器开始
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        int callCount = 0; // 记录调用次数

        // 射线检测调用循环
        while (stopwatch.ElapsedMilliseconds < 16.67f) // 目标时间为一帧 (16.67ms)
        {
            player.NewRayCast(origin, direction, reach);
            callCount++;
        }

        // 停止计时
        stopwatch.Stop();

        // 输出性能报告
        UnityEngine.Debug.Log($"性能测试完成：在一帧内最大射线检测调用次数为 {callCount}");
        UnityEngine.Debug.Log($"射线长度设置为 {reach}，检测增量为 {player.checkIncrement}");

        // 输出附加信息
        AnalyzeRaycastSettings();
    }

    void AnalyzeRaycastSettings()
    {
        // 建议射线最大长度
        //float recommendedLength = 20f; // 默认推荐 20
        if (reach > 50)
        {
            UnityEngine.Debug.LogWarning("射线长度过长，可能导致性能问题。建议保持在 20-50 范围内。");
        }
        else
        {
            UnityEngine.Debug.Log("射线长度在合理范围内。");
        }

        // 分析射线检测逻辑
        //UnityEngine.Debug.Log($"检测增量越小，精度越高，但性能开销会更大。目前增量为 {checkIncrement}，建议根据需求调整。");

        //// Unity 内置射线检测和自定义射线检测的区别
        //UnityEngine.Debug.Log("与 Unity 内置射线检测相比：");
        //UnityEngine.Debug.Log("1. Unity 内置射线检测可以支持无限长（物理引擎优化），但可能命中不准确。");
        //UnityEngine.Debug.Log("2. 当前自定义射线检测更灵活，可逐步模拟射线路径，支持额外功能（如实体检测）。");
        //UnityEngine.Debug.Log("3. 自定义射线检测性能开销较大，需合理限制长度与间隔。");
    }
}
