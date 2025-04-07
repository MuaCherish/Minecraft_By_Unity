using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    public ManagerHub managerhub;
    public Vector3 gravity = new Vector3(0, -9.81f, 0); // �Զ�������
    private int MaxRandomCutCount = 20;
    public float Y_Offset = 0.2f;

    // �����������
    public float friction = 4f; // ���Ը�����Ҫ������ֵ

    private void Awake()
    {
        // ��ȡ����ϵͳ���
        _particleSystem = GetComponent<ParticleSystem>();
        managerhub = SceneData.GetManagerhub();
    }

    public void Particle_Play(byte _targetType)
    {
        if (managerhub == null)
        {
            managerhub = SceneData.GetManagerhub();
        }

        // ����10������ü����Sprite
        Sprite originalSprite = managerhub.world.blocktypes[_targetType].icon;

        for (int i = 0; i < MaxRandomCutCount; i++)
        {
            _particleSystem.textureSheetAnimation.SetSprite(i, GetRandomCutCloudSprite(originalSprite, new Vector2Int(4, 4)));
        }

        // ��������ϵͳ
        _particleSystem.Play();
    }

    // ����ü��������µ�Sprite
    Sprite GetRandomCutCloudSprite(Sprite _WaitToCutSprite, Vector2Int _CutPixelSize)
    {
        // ��ȡ�������Rect��Texture
        Rect spriteRect = _WaitToCutSprite.rect;
        Texture2D texture = _WaitToCutSprite.texture;

        // ȷ���ü����ᳬ��������ı߽�
        int maxX = (int)(spriteRect.width - _CutPixelSize.x);
        int maxY = (int)(spriteRect.height - _CutPixelSize.y);

        // ������λ�û����������Rect
        int startX = (int)(Random.Range(0, maxX) + spriteRect.x);
        int startY = (int)(Random.Range(0, maxY) + spriteRect.y);

        // �����µĲü�����, ���������������λ��
        Rect rect = new Rect(startX, startY, _CutPixelSize.x, _CutPixelSize.y);

        // �����������µ�Sprite
        return Sprite.Create(
            texture,
            rect,
            new Vector2(0.5f, 0.5f),  // ʹSprite����
            100f                      // ���ص�λ����
        );
    }

    void Update()
    {
        // ��ȡ����ϵͳ�е���������
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particleSystem.particleCount];
        int particleCount = _particleSystem.GetParticles(particles);

        for (int i = 0; i < particleCount; i++)
        {
            // ��ȡ���ӵ�ǰ��λ��
            Vector3 particlePosition = particles[i].position;
            Vector3 particleVelocity = particles[i].velocity;

            // ��ײ��⣺������Ӽ�����ײ���棨ͨ��Y_Offset��⣩
            if (managerhub.Service_Chunk.CollisionCheckForVoxel(new Vector3(particlePosition.x, particlePosition.y + Y_Offset, particlePosition.z)))
            {
                // Y���ٶ�˲����㣬��ʾ���ӡ���ء�
                particleVelocity.y = 0f;

                // ʹ�����������𽥼���X��Z����ٶ�
                particleVelocity.x = Mathf.Lerp(particleVelocity.x, 0f, Time.deltaTime * friction);
                particleVelocity.z = Mathf.Lerp(particleVelocity.z, 0f, Time.deltaTime * friction);
            }
            else
            {
                // ʩ���Զ���������ֻӰ��Y��
                particleVelocity += gravity * Time.deltaTime;
            }

            // �������ӵ��ٶ�
            particles[i].velocity = particleVelocity;
        }

        // ��������ϵͳ
        _particleSystem.SetParticles(particles, particleCount);
    }
}
