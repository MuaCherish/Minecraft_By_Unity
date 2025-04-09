using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicObject3D : MonoBehaviour
{

    #region 周期函数

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


    #region 音乐控制

    public bool isStart = false;
    public void StartMusic(AudioClip _clip)
    {
        //提前返回-片段为空不执行
        if (_clip == null)
            return;

        isStart = true;
        musicSource.clip = _clip;
        musicSource.Play();
    }

    #endregion

}
