using UnityEngine;

public class footsounds : MonoBehaviour
{
    public AudioClip footstepClip; // ��·��Ч
    private AudioSource audioSource; // AudioSource���
    public float footstepInterval = 0.5f; // ��·��Ч���ż��
    private float nextFootstepTime; // ��һ����·��Ч����ʱ��

    void Start()
    {
        // ��ȡ��Ϸ�����ϵ�AudioSource��������û�������һ��
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // ����Ƿ�������������߿��Ը��ݾ��������޸�Ϊ����������
        if (Input.GetMouseButton(0))
        {
            if (Time.time >= nextFootstepTime)
            {
                PlayFootstepSound();
                // ������һ����·��Ч�Ĳ���ʱ��
                nextFootstepTime = Time.time + footstepInterval;
            }
        }
    }

    void PlayFootstepSound()
    {
        if (footstepClip != null && audioSource != null)
        {
            // ������·��Ч
            audioSource.PlayOneShot(footstepClip);
        }
    }
}
