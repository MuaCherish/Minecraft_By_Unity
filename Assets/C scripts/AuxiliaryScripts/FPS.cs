using UnityEngine;

public class FPS : MonoBehaviour
{
    /// <summary>
    /// 上一次更新帧率的时间
    /// </summary>
    private float m_lastUpdateShowTime = 0f;
    /// <summary>
    /// 更新显示帧率的时间间隔
    /// </summary>
    private readonly float m_updateTime = 0.5f;
    /// <summary>
    /// 帧数
    /// </summary>
    private int m_frames = 0;
    /// <summary>
    /// 帧间间隔
    /// </summary>
    private float m_frameDeltaTime = 0;
    /// <summary>
    /// 公共的FPS变量，供其他类访问
    /// </summary>
    public float fps { get; private set; } // 其他类可以读取FPS值

    void Awake()
    {
        Application.targetFrameRate = 100;
    }

    void Start()
    {
        m_lastUpdateShowTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
        m_frames++;
        if (Time.realtimeSinceStartup - m_lastUpdateShowTime >= m_updateTime)
        {
            fps = m_frames / (Time.realtimeSinceStartup - m_lastUpdateShowTime);
            m_frameDeltaTime = (Time.realtimeSinceStartup - m_lastUpdateShowTime) / m_frames;
            m_frames = 0;
            m_lastUpdateShowTime = Time.realtimeSinceStartup;
        }
    }
}
