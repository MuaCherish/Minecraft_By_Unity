using Homebrew;
using System.Collections;
using UnityEngine;

public class NewMusicManager : MonoBehaviour
{


    #region 状态

    [Foldout("状态", true)]
    [Header("正在处于背景音乐状态")][ReadOnly] public bool isOnBackGroundTime; //Menu不算背景音乐

    #endregion


    #region 周期函数

    ManagerHub managerhub;
    bool hasExec_Start = true;
    bool hasExec_Loading = true;
    bool hasExec_Playing = true;

    private void Awake()
    {
        managerhub = GlobalData.GetManagerhub();
    }


    private void Update()
    {

        switch (managerhub.world.game_state)
        {
            case Game_State.Start:
                Handle_GameState_Start();
                if (!hasExec_Loading)
                    hasExec_Loading = true;
                if (!hasExec_Playing)
                    hasExec_Playing = true;
                break;
            case Game_State.Loading:
                Handle_GameState_Loading();
                if (!hasExec_Start)
                    hasExec_Start = true;
                if (!hasExec_Playing)
                    hasExec_Playing = true;
                break;
            case Game_State.Playing:
                Handle_GameState_Playing();
                if (!hasExec_Start)
                    hasExec_Start = true;
                if (!hasExec_Loading)
                    hasExec_Loading = true;
                break;
        }

    }


    void Handle_GameState_Start()
    {
        if (hasExec_Start)
        {
            Init_MusicManager();
            hasExec_Start = false;
        }
    }

    void Handle_GameState_Loading()
    {
        if (hasExec_Loading)
        {
            
            hasExec_Loading = false;
        }

        
    }

    void Handle_GameState_Playing()
    {
        if (hasExec_Playing)
        {
            
            BackGroundTime();
            hasExec_Playing = false;
        }

        
    }


    void Init_MusicManager()
    {
        GameObject musicObject = new GameObject("MusicObject");
        musicObject.transform.SetParent(transform);
        Audio_envitonment = musicObject.AddComponent<AudioSource>();
        Audio_oneShot = musicObject.AddComponent<AudioSource>();

        //BackGround
        Audio_envitonment.clip = audioclips[MusicData.bgm_menu];
        Audio_envitonment.volume = 0.5f;
        Audio_envitonment.Play();

        //OneShot
        Audio_oneShot.volume = 0.15f;
    }

    #endregion


    #region 音乐系统

    [Foldout("Transforms", true)]
    [Header("3d音乐引用")] public GameObject Prefeb_3dMusicObject; 

    [Foldout("音乐系统", true)]
    [Header("音乐集")] public AudioClip[] audioclips;
   
    private AudioSource Audio_envitonment; // 背景音乐AudioSource
    private AudioSource Audio_oneShot; // 片段音源
    private Coroutine fadeCoroutine;
    private Coroutine backGround_Coroutine;


    /// <summary>
    /// 平滑设置音量
    /// </summary>
    public void SetVolumn(float _a)
    {
        Audio_envitonment.volume = _a;
    }

    /// <summary>
    /// 平滑设置音量
    /// </summary>
    public void SetVolumn_Smooth(float _a, float _duration)
    {
        StartCoroutine(SmoothVolumeChange(_a, _duration));
    }

    private IEnumerator SmoothVolumeChange(float targetVolume, float _duration)
    {
        float startVolume = Audio_envitonment.volume;
        float duration = _duration; // 调节时间（秒）
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            Audio_envitonment.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Audio_envitonment.volume = targetVolume; // 确保最终音量是目标音量
    }



    /// <summary>
    /// 在所给地点创建3d音频
    /// </summary>
    /// <param name="_pos"></param>
    /// <param name="_id"></param>
    public void Create3DSound(Vector3 _pos, int _id)
    {
        GameObject _obj = Instantiate(Prefeb_3dMusicObject);

        _obj.transform.SetParent(GlobalData.GetClonesParent().transform);
        _obj.transform.position = _pos;
        _obj.GetComponent<MusicObject3D>().StartMusic(audioclips[_id]);

    }


    /// <summary>
    /// 切换背景音乐，注意不能切换背景音乐
    /// </summary>
    /// <param name="_id"></param>
    /// <param name="_fadeDuration">淡出和淡入的持续时间</param>
    /// <param name="_finalVolume">最终的音量</param>
    public void SwitchBackgroundMusic(int _id, float _fadeDuration, float _finalVolume)
    {
        if (_id >= 0 && _id < audioclips.Length)
        {
            if (fadeCoroutine == null)
            {  
                fadeCoroutine = StartCoroutine(FadeOutAndSwitchMusic(_id, _fadeDuration, _finalVolume));
            }
        }
    }

    // 淡出当前音乐并切换到新音乐
    IEnumerator FadeOutAndSwitchMusic(int newClipId, float fadeDuration, float finalVolume)
    {

        float initialVolume = Audio_envitonment.volume;
        float fadeStep = initialVolume / (fadeDuration / Time.deltaTime);

        // 淡出当前音乐
        for (float volume = initialVolume; volume >= 0; volume -= fadeStep)
        {
            Audio_envitonment.volume = volume;
            yield return null;
        }

        //关闭背景音乐协程
        isOnBackGroundTime = false;
        StopCoroutine(backGround_Coroutine);
        backGround_Coroutine = null;

        // 切换新音乐
        Audio_envitonment.clip = audioclips[newClipId];
        Audio_envitonment.Play();


        // 淡入新音乐
        float targetVolume = finalVolume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            Audio_envitonment.volume = Mathf.Lerp(0, targetVolume, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保音量是最终目标音量
        Audio_envitonment.volume = targetVolume;
        fadeCoroutine = null;
    }





    #endregion


    #region 音乐循环


    /// <summary>
    /// 重新回到背景音乐模式
    /// </summary>
    public void BackGroundTime()
    {
        if (backGround_Coroutine == null)
        {
            isOnBackGroundTime = true;
            backGround_Coroutine = StartCoroutine(PlayEnvironmentLoop());
        }
    }


    // 循环播放背景音乐
    IEnumerator PlayEnvironmentLoop()
    {
        SetVolumn_Smooth(0f, 1f);
        yield return new WaitForSeconds(1f);

        //Data
        AudioClip[] bgmClips = { audioclips[MusicData.bgm_3], audioclips[MusicData.bgm_2], audioclips[MusicData.bgm_1] };
        int bgmIndex = 0;
        bgmIndex = (bgmIndex + 1) % bgmClips.Length;
        Audio_envitonment.clip = bgmClips[bgmIndex];

        SetVolumn_Smooth(0.5f, 1f);
        yield return new WaitForSeconds(1f);

        
        

        while (true)
        {
            if (!Audio_envitonment.isPlaying)
            {
                Audio_envitonment.clip = bgmClips[bgmIndex];
                Audio_envitonment.volume = 0.5f;
                Audio_envitonment.Play();
                bgmIndex = (bgmIndex + 1) % bgmClips.Length;
            }
            yield return new WaitForSeconds(5f);
        }


    }


    #endregion


    #region OneShot


    /// <summary>
    /// 设定OneShot音量
    /// </summary>
    /// <param name="_volumn"></param>
    public void SetOneShot_Volumn(float _volumn)
    {
        Audio_oneShot.volume = _volumn;
    }


    /// <summary>
    /// 播放片段
    /// </summary>
    /// <param name="_pos"></param>
    /// <param name="_id"></param>
    public void PlayOneShot(int _id)
    {
        Audio_oneShot.PlayOneShot(audioclips[_id]);
    }


    /// <summary>
    /// 播放片段
    /// </summary>
    /// <param name="_clip"></param>
    public void PlayOneShot(AudioClip _clip)
    {
        Audio_oneShot.PlayOneShot(_clip);
    }

    #endregion


}
