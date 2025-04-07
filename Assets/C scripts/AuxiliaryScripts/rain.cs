using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rain : MonoBehaviour
{
    ManagerHub managerhub;
    World world;
    ParticleSystem ps;

    private ParticleSystem.Particle[] particles; // 用于存储粒子信息

    // 自定义参数
    public float Y_Offset = 0.1f;  // 用于检测地面的偏移量
    public Vector3 gravity = new Vector3(0, -9.8f, 0); // 重力向量

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        world = managerhub.world;
        ps = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// 开始下雨：启用粒子系统并播放粒子效果
    /// </summary>
    public void StartRain()
    {
        // 启用粒子系统
        ps.Play();
    }

    /// <summary>
    /// 停止下雨：等待粒子发射完毕后自动隐藏对象
    /// </summary>
    public void StopRain()
    {
        StartCoroutine(StopRainCoroutine());
    }

    private IEnumerator StopRainCoroutine()
    {
        // 停止粒子发射
        ps.Stop();

        // 等待所有粒子消失
        while (ps.IsAlive(true))
        {
            yield return null; // 等待下一帧
        }

        // 隐藏当前 GameObject
        gameObject.SetActive(false);
    }

    void Update()
    {
        // 获取当前粒子的数量
        int particleCount = ps.particleCount;

        // 初始化粒子数组（仅在需要时创建一次）
        if (particles == null || particles.Length < particleCount)
        {
            particles = new ParticleSystem.Particle[particleCount];
        }

        // 获取粒子系统中的粒子
        int numParticlesAlive = ps.GetParticles(particles);

        for (int i = 0; i < numParticlesAlive; i++)
        {
            // 获取粒子当前的位置和速度
            Vector3 particlePosition = particles[i].position;
            Vector3 particleVelocity = particles[i].velocity;

            // 碰撞检测：如果粒子即将碰撞地面
            if (managerhub.Service_Chunk.CollisionCheckForVoxel(new Vector3(particlePosition.x, particlePosition.y + Y_Offset, particlePosition.z)))
            {
                // Y轴速度瞬间归零，表示粒子“落地”
                particleVelocity.y = 0f;
            }
            else
            {
                // 未碰撞时，施加自定义重力，只影响Y轴
                particleVelocity += gravity * Time.deltaTime;
            }

            // 更新粒子的速度
            particles[i].velocity = particleVelocity;
        }

        // 更新粒子系统
        ps.SetParticles(particles, numParticlesAlive);
    }
}
