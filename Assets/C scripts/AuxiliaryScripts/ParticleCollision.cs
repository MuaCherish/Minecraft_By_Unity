using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    public ManagerHub managerhub;
    public Vector3 gravity = new Vector3(0, -50f, 0); // �Զ�������
    public float Y_Offset = 0.2f;

    private void Awake()
    {
        // ��ȡ����ϵͳ���
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
        // ��ȡ����ϵͳ�е���������
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particleSystem.particleCount];
        int particleCount = _particleSystem.GetParticles(particles);

        // ����ÿ������
        for (int i = 0; i < particleCount; i++)
        {
            // ��ȡ���ӵ�λ��
            Vector3 particlePosition = particles[i].position;

            // �жϸ�λ���Ƿ�����ײ
            if (managerhub.world.CheckForVoxel(new Vector3(particlePosition.x, particlePosition.y + Y_Offset, particlePosition.z)))
            {
                // �������ӵ��ٶ�Ϊ��
                particles[i].velocity = Vector3.zero;
            }
            else
            {
                // ���û����ײ����Ӧ���Զ�������
                particles[i].velocity += gravity * Time.deltaTime;
            }
        }

        // ��������ϵͳ
        _particleSystem.SetParticles(particles, particleCount);
    }

    //public bool isSolid(Vector3 pos)
    //{
    //    // ������λ���Ƿ�Ϊ��ײ����
    //    return pos.y <= DEBUGY;
    //}
}
