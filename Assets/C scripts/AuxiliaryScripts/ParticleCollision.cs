using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    public ManagerHub managerhub;
    public Vector3 gravity = new Vector3(0, -9.81f, 0); // 自定义重力
    private int MaxRandomCutCount = 20;
    public float Y_Offset = 0.2f;

    // 添加阻力变量
    public float friction = 4f; // 可以根据需要调整此值

    private void Awake()
    {
        // 获取粒子系统组件
        _particleSystem = GetComponent<ParticleSystem>();
        managerhub = SceneData.GetManagerhub();
    }

    public void Particle_Play(byte _targetType)
    {
        if (managerhub == null)
        {
            managerhub = SceneData.GetManagerhub();
        }

        // 生成10张随机裁剪后的Sprite
        Sprite originalSprite = managerhub.world.blocktypes[_targetType].icon;

        for (int i = 0; i < MaxRandomCutCount; i++)
        {
            _particleSystem.textureSheetAnimation.SetSprite(i, GetRandomCutCloudSprite(originalSprite, new Vector2Int(4, 4)));
        }

        // 播放粒子系统
        _particleSystem.Play();
    }

    // 随机裁剪并返回新的Sprite
    Sprite GetRandomCutCloudSprite(Sprite _WaitToCutSprite, Vector2Int _CutPixelSize)
    {
        // 获取子纹理的Rect和Texture
        Rect spriteRect = _WaitToCutSprite.rect;
        Texture2D texture = _WaitToCutSprite.texture;

        // 确保裁剪不会超出子纹理的边界
        int maxX = (int)(spriteRect.width - _CutPixelSize.x);
        int maxY = (int)(spriteRect.height - _CutPixelSize.y);

        // 随机起点位置基于子纹理的Rect
        int startX = (int)(Random.Range(0, maxX) + spriteRect.x);
        int startY = (int)(Random.Range(0, maxY) + spriteRect.y);

        // 定义新的裁剪区域, 相对于整个纹理集的位置
        Rect rect = new Rect(startX, startY, _CutPixelSize.x, _CutPixelSize.y);

        // 创建并返回新的Sprite
        return Sprite.Create(
            texture,
            rect,
            new Vector2(0.5f, 0.5f),  // 使Sprite居中
            100f                      // 像素单位比例
        );
    }

    void Update()
    {
        // 获取粒子系统中的所有粒子
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particleSystem.particleCount];
        int particleCount = _particleSystem.GetParticles(particles);

        for (int i = 0; i < particleCount; i++)
        {
            // 获取粒子当前的位置
            Vector3 particlePosition = particles[i].position;
            Vector3 particleVelocity = particles[i].velocity;

            // 碰撞检测：如果粒子即将碰撞地面（通过Y_Offset检测）
            if (managerhub.Service_Chunk.CollisionCheckForVoxel(new Vector3(particlePosition.x, particlePosition.y + Y_Offset, particlePosition.z)))
            {
                // Y轴速度瞬间归零，表示粒子“落地”
                particleVelocity.y = 0f;

                // 使用阻力变量逐渐减缓X和Z轴的速度
                particleVelocity.x = Mathf.Lerp(particleVelocity.x, 0f, Time.deltaTime * friction);
                particleVelocity.z = Mathf.Lerp(particleVelocity.z, 0f, Time.deltaTime * friction);
            }
            else
            {
                // 施加自定义重力，只影响Y轴
                particleVelocity += gravity * Time.deltaTime;
            }

            // 更新粒子的速度
            particles[i].velocity = particleVelocity;
        }

        // 更新粒子系统
        _particleSystem.SetParticles(particles, particleCount);
    }
}
