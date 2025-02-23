using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test3dSound : MonoBehaviour
{
    public AudioClip _clip;    // Ҫ���ŵ���Ƶ����
    public float maxDistance = 10f;  // ��Զ��������
    public float minDistance = 2f;  // �����������

    private AudioSource _audioSource;  // ����Դ

    void Start()
    {
        // ���� AudioSource ���������Ϊ 3D ��Ƶ
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 1.0f; 
        _audioSource.playOnAwake = false; 

        // ���� maxDistance �� minDistance
        _audioSource.maxDistance = maxDistance;
        _audioSource.minDistance = minDistance;
        _audioSource.rolloffMode = AudioRolloffMode.Logarithmic; 
    }

    void Update()
    {
        // ������������
        if (Input.GetMouseButtonDown(0))  // 0 ��������
        {
            // ���� OneShot ��Ƶ
            _audioSource.PlayOneShot(_clip);
        }
    }
}
