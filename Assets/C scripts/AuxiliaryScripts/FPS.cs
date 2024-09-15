using UnityEngine;

public class FPS : MonoBehaviour
{
    /// <summary>
    /// ��һ�θ���֡�ʵ�ʱ��
    /// </summary>
    private float m_lastUpdateShowTime = 0f;
    /// <summary>
    /// ������ʾ֡�ʵ�ʱ����
    /// </summary>
    private readonly float m_updateTime = 0.5f;
    /// <summary>
    /// ֡��
    /// </summary>
    private int m_frames = 0;
    /// <summary>
    /// ֡����
    /// </summary>
    private float m_frameDeltaTime = 0;
    /// <summary>
    /// ������FPS�����������������
    /// </summary>
    public float fps { get; private set; } // ��������Զ�ȡFPSֵ

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
