using UnityEngine;

public class footsounds : MonoBehaviour
{
    public AudioClip footstepClip; // 走路音效
    private AudioSource audioSource; // AudioSource组件
    public float footstepInterval = 0.5f; // 走路音效播放间隔
    private float nextFootstepTime; // 下一个走路音效播放时间

    void Start()
    {
        // 获取游戏对象上的AudioSource组件，如果没有则添加一个
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // 检查是否按下了左键（或者可以根据具体需求修改为其他按键）
        if (Input.GetMouseButton(0))
        {
            if (Time.time >= nextFootstepTime)
            {
                PlayFootstepSound();
                // 设置下一个走路音效的播放时间
                nextFootstepTime = Time.time + footstepInterval;
            }
        }
    }

    void PlayFootstepSound()
    {
        if (footstepClip != null && audioSource != null)
        {
            // 播放走路音效
            audioSource.PlayOneShot(footstepClip);
        }
    }
}
