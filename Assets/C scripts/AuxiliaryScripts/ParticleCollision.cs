using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    public ManagerHub managerhub;
    public Vector3 gravity = new Vector3(0, -50f, 0); // 自定义重力
    public float Y_Offset = 0.2f;

    private void Awake()
    {
        // 获取粒子系统组件
        _particleSystem = GetComponent<ParticleSystem>();
        managerhub = GameObject.Find("Manager/ManagerHub").GetComponent<ManagerHub>();
        
    }

    public void Particle_PLay(byte _targetType)
    {
        _particleSystem.Play();
        _particleSystem.textureSheetAnimation.SetSprite(0, managerhub.world.blocktypes[_targetType].buttom_sprit);
    }

    void Update()
    {
        // 获取粒子系统中的所有粒子
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particleSystem.particleCount];
        int particleCount = _particleSystem.GetParticles(particles);

        // 遍历粒子
        for (int i = 0; i < particleCount; i++)
        {
            // 获取粒子的位置
            Vector3 particlePosition = particles[i].position;

            // 判断该位置是否有碰撞
            if (managerhub.world.CollisionCheckForVoxel(new Vector3(particlePosition.x, particlePosition.y + Y_Offset, particlePosition.z)))
            {
                // 停止粒子运动
                particles[i].velocity = Vector3.zero;
            }
            else
            {
                // 应用自定义重力
                particles[i].velocity += gravity * Time.deltaTime;
            }
        }

        // 更新粒子系统
        _particleSystem.SetParticles(particles, particleCount);
    }
}
