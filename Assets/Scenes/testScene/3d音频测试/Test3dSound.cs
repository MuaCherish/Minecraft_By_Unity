using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test3dSound : MonoBehaviour
{
    public AudioClip _clip;    // 要播放的音频剪辑
    public float maxDistance = 10f;  // 最远可听距离
    public float minDistance = 2f;  // 最近可听距离

    private AudioSource _audioSource;  // 声音源

    void Start()
    {
        // 创建 AudioSource 组件并配置为 3D 音频
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 1.0f; 
        _audioSource.playOnAwake = false; 

        // 设置 maxDistance 和 minDistance
        _audioSource.maxDistance = maxDistance;
        _audioSource.minDistance = minDistance;
        _audioSource.rolloffMode = AudioRolloffMode.Logarithmic; 
    }

    void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))  // 0 是鼠标左键
        {
            // 播放 OneShot 音频
            _audioSource.PlayOneShot(_clip);
        }
    }
}
