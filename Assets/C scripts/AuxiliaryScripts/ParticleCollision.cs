using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    public Vector3 gravity = new Vector3(0, -9.81f, 0); // �Զ�������
    public float DEBUGY;

    void Start()
    {
        // ��ȡ����ϵͳ���
        _particleSystem = GetComponent<ParticleSystem>();
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
            if (isSolid(particlePosition))
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

    public bool isSolid(Vector3 pos)
    {
        // ������λ���Ƿ�Ϊ��ײ����
        return pos.y <= DEBUGY;
    }
}
