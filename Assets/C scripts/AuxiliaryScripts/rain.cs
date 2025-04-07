using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rain : MonoBehaviour
{
    ManagerHub managerhub;
    World world;
    ParticleSystem ps;

    private ParticleSystem.Particle[] particles; // ���ڴ洢������Ϣ

    // �Զ������
    public float Y_Offset = 0.1f;  // ���ڼ������ƫ����
    public Vector3 gravity = new Vector3(0, -9.8f, 0); // ��������

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        world = managerhub.world;
        ps = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// ��ʼ���꣺��������ϵͳ����������Ч��
    /// </summary>
    public void StartRain()
    {
        // ��������ϵͳ
        ps.Play();
    }

    /// <summary>
    /// ֹͣ���꣺�ȴ����ӷ�����Ϻ��Զ����ض���
    /// </summary>
    public void StopRain()
    {
        StartCoroutine(StopRainCoroutine());
    }

    private IEnumerator StopRainCoroutine()
    {
        // ֹͣ���ӷ���
        ps.Stop();

        // �ȴ�����������ʧ
        while (ps.IsAlive(true))
        {
            yield return null; // �ȴ���һ֡
        }

        // ���ص�ǰ GameObject
        gameObject.SetActive(false);
    }

    void Update()
    {
        // ��ȡ��ǰ���ӵ�����
        int particleCount = ps.particleCount;

        // ��ʼ���������飨������Ҫʱ����һ�Σ�
        if (particles == null || particles.Length < particleCount)
        {
            particles = new ParticleSystem.Particle[particleCount];
        }

        // ��ȡ����ϵͳ�е�����
        int numParticlesAlive = ps.GetParticles(particles);

        for (int i = 0; i < numParticlesAlive; i++)
        {
            // ��ȡ���ӵ�ǰ��λ�ú��ٶ�
            Vector3 particlePosition = particles[i].position;
            Vector3 particleVelocity = particles[i].velocity;

            // ��ײ��⣺������Ӽ�����ײ����
            if (managerhub.Service_Chunk.CollisionCheckForVoxel(new Vector3(particlePosition.x, particlePosition.y + Y_Offset, particlePosition.z)))
            {
                // Y���ٶ�˲����㣬��ʾ���ӡ���ء�
                particleVelocity.y = 0f;
            }
            else
            {
                // δ��ײʱ��ʩ���Զ���������ֻӰ��Y��
                particleVelocity += gravity * Time.deltaTime;
            }

            // �������ӵ��ٶ�
            particles[i].velocity = particleVelocity;
        }

        // ��������ϵͳ
        ps.SetParticles(particles, numParticlesAlive);
    }
}
