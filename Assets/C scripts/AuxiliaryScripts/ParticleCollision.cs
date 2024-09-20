using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    public ManagerHub managerhub;
    public Vector3 gravity = new Vector3(0, -50f, 0); // 自定义重力
    public float Y_Offset = 0.2f;


    public void StartPatticle_Broken(ManagerHub _managerhub)
    {
        // 获取粒子系统组件
        _particleSystem = GetComponent<ParticleSystem>();

        managerhub = _managerhub;

        _particleSystem.Play();
    }

    void Update()
    {
        // 获取粒子系统中的所有粒子
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particleSystem.particleCount];
        int particleCount = _particleSystem.GetParticles(particles);

        // 遍历每个粒子
        for (int i = 0; i < particleCount; i++)
        {
            // 获取粒子的位置
            Vector3 particlePosition = particles[i].position;

            // 判断该位置是否有碰撞
            if (managerhub.world.blocktypes[managerhub.world.GetBlockType(new Vector3(particlePosition.x, particlePosition.y + Y_Offset, particlePosition.z))].isSolid)
            {
                // 设置粒子的速度为零
                particles[i].velocity = Vector3.zero;
            }
            else
            {
                // 如果没有碰撞，则应用自定义重力
                particles[i].velocity += gravity * Time.deltaTime;
            }
        }

        // 更新粒子系统
        _particleSystem.SetParticles(particles, particleCount);
    }

    //public bool isSolid(Vector3 pos)
    //{
    //    // 检查给定位置是否为碰撞区域
    //    return pos.y <= DEBUGY;
    //}
}
