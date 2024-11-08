using Homebrew;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class NewMusicManager : MonoBehaviour
{


    #region ״̬


    #endregion


    #region ���ں���

    ManagerHub managerhub;
    public AudioClip[] audioclips;


    private void Start()
    {
        managerhub = GlobalData.GetManagerhub();
        _ReferStart_BackGroundMusic();

        //print("��ʼ��������");
        
    }

    private void Update()
    {
        _ReferUpdate_BackGroundMusic();
    }

    #endregion

    #region ��������

    /// <summary>
    /// �ı�����
    /// </summary>
    /// <param name="_a"></param>
    public void SetAudioBackGroundVolumn(float _a)
    {
        Audio_envitonment.volume = _a;
    }


    /// <summary>
    /// ������Ƶ
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
    /// �������ص㴴��3d��Ƶ
    /// </summary>
    /// <param name="_pos"></param>
    /// <param name="_id"></param>
    public void PlaySound(Vector3 _pos, int _id)
    {

    }


    /// <summary>
    /// �л���������
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



    #region ��������

    // ��������AudioSource
    private AudioSource Audio_envitonment;

    // Э�̹���
    private Coroutine fadeCoroutine;
    private Coroutine playCoroutine;


    #region ��������-���ں���

  
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


    #region ��������-����ѭ��

    // ����ѭ������Э��
    private void StartPlayLoopIfNotRunning()
    {
        if (playCoroutine == null)
        {
            playCoroutine = StartCoroutine(PlayEnvironmentLoop());
        }
    }


    
    // ѭ�����ű�������
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


    #region ��������-����


    /// <summary>
    /// ��������Э��
    /// </summary>
    public void FadeToStopBackGroundMusic()
    {
        if (fadeCoroutine == null)
        {
            fadeCoroutine = StartCoroutine(FadeOutAndStopEnvironment());
        }
    }


    // �����������ֲ�ֹͣ
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


    #region ��������-�л�����

    public float FadeDuration = 1f;

    
    // ������ǰ���ֲ��л���������
    private IEnumerator FadeOutAndSwitchMusic(int newClipId)
    {
        float initialVolume = Audio_envitonment.volume;
        float fadeStep = initialVolume / (FadeDuration / Time.deltaTime);

        for (float volume = initialVolume; volume >= 0; volume -= fadeStep)
        {
            Audio_envitonment.volume = volume;
            yield return null;
        }

        // �л�������
        Audio_envitonment.clip = audioclips[newClipId];
        Audio_envitonment.volume = initialVolume;
        Audio_envitonment.Play();
        fadeCoroutine = null;
    }


    #endregion



    #endregion






}
