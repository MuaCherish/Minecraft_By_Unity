using Homebrew;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class NewMusicManager : MonoBehaviour
{


    #region 状态


    #endregion


    #region 周期函数

    ManagerHub managerhub;
    public AudioClip[] audioclips;


    private void Start()
    {
        managerhub = GlobalData.GetManagerhub();
        _ReferStart_BackGroundMusic();

        //print("开始播放音乐");
        
    }

    private void Update()
    {
        _ReferUpdate_BackGroundMusic();
    }

    #endregion

    #region 公开函数

    /// <summary>
    /// 改变音量
    /// </summary>
    /// <param name="_a"></param>
    public void SetAudioBackGroundVolumn(float _a)
    {
        Audio_envitonment.volume = _a;
    }


    /// <summary>
    /// 播放音频
    /// </summary>
    /// <param name="_pos"></param>
    /// <param name="_id"></param>
    public void PlayOneShot(int _id)
    {

        //OneShot
        Audio_envitonment.PlayOneShot(audioclips[_id]);
    }

    /// <summary>
    /// OneShot
    /// </summary>
    /// <param name="_clip"></param>
    public void PlayOneShot(AudioClip _clip)
    {
        //OneShot
        Audio_envitonment.PlayOneShot(_clip);
    }


    /// <summary>
    /// 在所给地点创建3d音频
    /// </summary>
    /// <param name="_pos"></param>
    /// <param name="_id"></param>
    public void PlaySound(Vector3 _pos, int _id)
    {

    }


    /// <summary>
    /// 切换背景音乐
    /// </summary>
    /// <param name="id"></param>
    public void SwitchBackgroundMusic(int id)
    {
        if (id >= 0 && id < audioclips.Length)
        {
            if (fadeCoroutine == null)
            {
                fadeCoroutine = StartCoroutine(FadeOutAndSwitchMusic(id));
            }
        }
    }



    #endregion



    #region 背景音乐

    // 背景音乐AudioSource
    private AudioSource Audio_envitonment;

    // 协程管理
    private Coroutine fadeCoroutine;
    private Coroutine playCoroutine;


    #region 背景音乐-周期函数

  
    void _ReferStart_BackGroundMusic()
    {
        GameObject musicObject = new GameObject("MusicObject");
        musicObject.transform.SetParent(transform);
        Audio_envitonment = musicObject.AddComponent<AudioSource>();

        //Play
        Audio_envitonment.clip = audioclips[MusicData.bgm_menu];
        Audio_envitonment.volume = 0.5f;
        Audio_envitonment.Play();
    }

    void _ReferUpdate_BackGroundMusic()
    {
        
        if (managerhub.world.game_state == Game_State.Loading)
        {
            FadeToStopBackGroundMusic();
        }
        else if (managerhub.world.game_state == Game_State.Playing)
        {
            StartPlayLoopIfNotRunning();
        }
    }

    #endregion


    #region 背景音乐-音乐循环

    // 启动循环播放协程
    private void StartPlayLoopIfNotRunning()
    {
        if (playCoroutine == null)
        {
            playCoroutine = StartCoroutine(PlayEnvironmentLoop());
        }
    }


    
    // 循环播放背景音乐
    IEnumerator PlayEnvironmentLoop()
    {

       
        yield return new WaitForSeconds(3f);
        int bgmIndex = 0;
        AudioClip[] bgmClips = { audioclips[MusicData.bgm_3], audioclips[MusicData.bgm_2], audioclips[MusicData.bgm_1] };

        while (true)
        {
            if (!Audio_envitonment.isPlaying)
            {
                Audio_envitonment.clip = bgmClips[bgmIndex];
                Audio_envitonment.volume = 0.5f;
                Audio_envitonment.Play();
                bgmIndex = (bgmIndex + 1) % bgmClips.Length;
            }
            yield return new WaitForSeconds(1f);
        }
    }



    #endregion


    #region 背景音乐-淡出


    /// <summary>
    /// 启动淡出协程
    /// </summary>
    public void FadeToStopBackGroundMusic()
    {
        if (fadeCoroutine == null)
        {
            fadeCoroutine = StartCoroutine(FadeOutAndStopEnvironment());
        }
    }


    // 淡出背景音乐并停止
    IEnumerator FadeOutAndStopEnvironment()
    {
        float initialVolume = Audio_envitonment.volume;
        for (float volume = initialVolume; volume >= 0; volume -= 0.01f)
        {
            Audio_envitonment.volume = volume;
            yield return null;
        }
        Audio_envitonment.Stop();
        Audio_envitonment.volume = initialVolume;
        fadeCoroutine = null;
    }


    #endregion


    #region 背景音乐-切换音乐

    public float FadeDuration = 1f;

    
    // 淡出当前音乐并切换到新音乐
    private IEnumerator FadeOutAndSwitchMusic(int newClipId)
    {
        float initialVolume = Audio_envitonment.volume;
        float fadeStep = initialVolume / (FadeDuration / Time.deltaTime);

        for (float volume = initialVolume; volume >= 0; volume -= fadeStep)
        {
            Audio_envitonment.volume = volume;
            yield return null;
        }

        // 切换新音乐
        Audio_envitonment.clip = audioclips[newClipId];
        Audio_envitonment.volume = initialVolume;
        Audio_envitonment.Play();
        fadeCoroutine = null;
    }


    #endregion



    #endregion






}
