using Homebrew;
using System.Collections;
using UnityEngine;

public class NewMusicManager : MonoBehaviour
{


    #region ״̬

    [Foldout("״̬", true)]
    [Header("���ڴ��ڱ�������״̬")][ReadOnly] public bool isOnBackGroundTime; //Menu���㱳������

    #endregion


    #region ���ں���

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


    #region ����ϵͳ

    [Foldout("Transforms", true)]
    [Header("3d��������")] public GameObject Prefeb_3dMusicObject; 

    [Foldout("����ϵͳ", true)]
    [Header("���ּ�")] public AudioClip[] audioclips;
   
    private AudioSource Audio_envitonment; // ��������AudioSource
    private AudioSource Audio_oneShot; // Ƭ����Դ
    private Coroutine fadeCoroutine;
    private Coroutine backGround_Coroutine;


    /// <summary>
    /// ƽ����������
    /// </summary>
    public void SetVolumn(float _a)
    {
        Audio_envitonment.volume = _a;
    }

    /// <summary>
    /// ƽ����������
    /// </summary>
    public void SetVolumn_Smooth(float _a, float _duration)
    {
        StartCoroutine(SmoothVolumeChange(_a, _duration));
    }

    private IEnumerator SmoothVolumeChange(float targetVolume, float _duration)
    {
        float startVolume = Audio_envitonment.volume;
        float duration = _duration; // ����ʱ�䣨�룩
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            Audio_envitonment.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Audio_envitonment.volume = targetVolume; // ȷ������������Ŀ������
    }



    /// <summary>
    /// �������ص㴴��3d��Ƶ
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
    /// �л��������֣�ע�ⲻ���л���������
    /// </summary>
    /// <param name="_id"></param>
    /// <param name="_fadeDuration">�����͵���ĳ���ʱ��</param>
    /// <param name="_finalVolume">���յ�����</param>
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

    // ������ǰ���ֲ��л���������
    IEnumerator FadeOutAndSwitchMusic(int newClipId, float fadeDuration, float finalVolume)
    {

        float initialVolume = Audio_envitonment.volume;
        float fadeStep = initialVolume / (fadeDuration / Time.deltaTime);

        // ������ǰ����
        for (float volume = initialVolume; volume >= 0; volume -= fadeStep)
        {
            Audio_envitonment.volume = volume;
            yield return null;
        }

        //�رձ�������Э��
        isOnBackGroundTime = false;
        StopCoroutine(backGround_Coroutine);
        backGround_Coroutine = null;

        // �л�������
        Audio_envitonment.clip = audioclips[newClipId];
        Audio_envitonment.Play();


        // ����������
        float targetVolume = finalVolume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            Audio_envitonment.volume = Mathf.Lerp(0, targetVolume, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ȷ������������Ŀ������
        Audio_envitonment.volume = targetVolume;
        fadeCoroutine = null;
    }





    #endregion


    #region ����ѭ��


    /// <summary>
    /// ���»ص���������ģʽ
    /// </summary>
    public void BackGroundTime()
    {
        if (backGround_Coroutine == null)
        {
            isOnBackGroundTime = true;
            backGround_Coroutine = StartCoroutine(PlayEnvironmentLoop());
        }
    }


    // ѭ�����ű�������
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
    /// �趨OneShot����
    /// </summary>
    /// <param name="_volumn"></param>
    public void SetOneShot_Volumn(float _volumn)
    {
        Audio_oneShot.volume = _volumn;
    }


    /// <summary>
    /// ����Ƭ��
    /// </summary>
    /// <param name="_pos"></param>
    /// <param name="_id"></param>
    public void PlayOneShot(int _id)
    {
        Audio_oneShot.PlayOneShot(audioclips[_id]);
    }


    /// <summary>
    /// ����Ƭ��
    /// </summary>
    /// <param name="_clip"></param>
    public void PlayOneShot(AudioClip _clip)
    {
        Audio_oneShot.PlayOneShot(_clip);
    }

    #endregion


}
