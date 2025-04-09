using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicObject3D : MonoBehaviour
{

    #region ���ں���

    AudioSource musicSource;
    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
    }


    private void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        if (isStart && !musicSource.isPlaying)
        {
            Destroy(this.gameObject, 1f);
        }
    }


    #endregion


    #region ���ֿ���

    public bool isStart = false;
    public void StartMusic(AudioClip _clip)
    {
        //��ǰ����-Ƭ��Ϊ�ղ�ִ��
        if (_clip == null)
            return;

        isStart = true;
        musicSource.clip = _clip;
        musicSource.Play();
    }

    #endregion

}
