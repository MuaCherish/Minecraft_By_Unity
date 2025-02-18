using Homebrew;
using MCEntity;
using System.Collections;
using UnityEngine;

/// <summary>
/// �����Ҫʵ�ֵĻ��У�
/// ���Ǳ��û����
/// </summary>

[RequireComponent(typeof(MC_Collider_Component))]
[RequireComponent(typeof(MC_Velocity_Component))]
public class MC_Music_Component : MonoBehaviour
{

    #region ״̬

    [Foldout("״̬", true)]
    [Header("�Ƿ���ˮ��")] public bool isInTheWater;

    #endregion


    #region ���ں���

    ManagerHub managerhub;
    MC_Velocity_Component Velocity_Component;
    MC_Collider_Component Collider_Component;
    NewMusicManager musicManager;

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        Velocity_Component = GetComponent<MC_Velocity_Component>();
        Collider_Component = GetComponent<MC_Collider_Component>();
        musicManager = managerhub.NewmusicManager;
    }

    private void Start()
    {
        InitAudioSource();
        _ReferStart_ClipCheck();
        StartCoroutine(Coroutine_Saying());
    }

    public AudioClip _clip;    // Ҫ���ŵ���Ƶ����
    private void Update()
    {
        if (managerhub.world.game_state == Game_State.Playing)
        {
            _ReferUpdate_FootStep();
            _ReferUpdate_Water();

            // ������������
            if (Input.GetKeyDown(KeyCode.H))  // 0 ��������
            {
                // ���� OneShot ��Ƶ
                PlaySound(_clip);
            }
        }
    }

    #endregion


    #region ����������

    [Foldout("��������ʼ������", true)]
    [Header("��Զ����")] public float MaxDistant = 5f;
    [Header("����")] public float AudioVolume = 0.8f;
    private AudioSource MainAudioSource;

    [Foldout("ʵ����Ի�Ƭ��", true)]
    [Header("������Ч")] public AudioClip BehurtClip; //Ĭ��Ϊʷ��ķ��Ч
    [Header("������Ч")] public AudioClip DeathClip; //Ĭ��Ϊ��������Ч
    [Header("�԰���Ч")] public AudioClip[] SayingClips;
    [Header("�԰���Ч�ӳٷ�Χ")] public Vector2 SayingDelayRange = new Vector2(5f, 30f);

    void _ReferStart_ClipCheck()
    {
        if(BehurtClip == null)
        {
            print("BehurtClipδ����");
        }

        if (DeathClip == null)
        {
            print("DeathClipδ����");
        }
    }

    // ��ʼ��������
    void InitAudioSource()
    {
        //GameObject
        GameObject AudioSourceObject = new GameObject("Audio-Object");
        AudioSourceObject.transform.SetParent(this.transform);
        AudioSourceObject.transform.localPosition = Vector3.zero;

        //AudioSource
        MainAudioSource = AudioSourceObject.AddComponent<AudioSource>();
        MainAudioSource.spatialBlend = 1f; // 1.0��ʾ��ȫ3D
        MainAudioSource.maxDistance = MaxDistant;
        MainAudioSource.volume = AudioVolume;
        MainAudioSource.rolloffMode = AudioRolloffMode.Logarithmic; // �������������˥��ģʽ
        MainAudioSource.dopplerLevel = 0; // ���ö�����ЧӦ
        MainAudioSource.playOnAwake = false;
    }

    #endregion


    #region ����

    /// <summary>
    /// ����Ƭ��
    /// </summary>
    public void PlaySound(AudioClip _Clip)
    {
        MainAudioSource.PlayOneShot(_Clip);
    }

    #endregion


    #region �԰���Ч

    IEnumerator Coroutine_Saying()
    {
        //��ǰ����-�յ�
        if (SayingClips.Length == 0)
            yield break;

        while (true)
        {

            TrySaying();

            float delayTime = Random.Range(SayingDelayRange.x, SayingDelayRange.y);
            yield return new WaitForSeconds(delayTime);

        }

    }

    /// <summary>
    /// ���Խ�һ��
    /// </summary>
    public void TrySaying()
    {
        //��ǰ����-�յ�
        if (SayingClips.Length == 0)
            return;

        int index = Random.Range(0, SayingClips.Length);
        MainAudioSource.PlayOneShot(SayingClips[index]);

    }

    #endregion



    #region �Ų���

    [Foldout("�Ų���", true)]
    [Header("ʵ���Ƿ��нŲ���")] public bool hasFootStep = true;
    [Header("�Ų������ʱ��")] public float FootStep_InterTime = 0.3f;
    int item = 0;   //�����������ҽŵ�
    private float nextFoot = 0f;

    void _ReferUpdate_FootStep()
    {
        // ��ǰ���� - ��Ҿ�ֹ���ڿ���
        if (!Velocity_Component.isMoving || !Collider_Component.isGround || hasFootStep == false) 
            return;

        // ����Ƿ񵽴���һ������ʱ��
        if (Time.time >= nextFoot)
        {
            // ��ȡ���ŵ���Ч������ʹ��ר����Ч������Ĭ��ʯͷ��Ч
            AudioClip clipToPlay = GetFootstepClip(Collider_Component.FootBlockType);

            // ������Ч���л�item
            MainAudioSource.PlayOneShot(clipToPlay);
            item = 1 - item;  // �л�item��0��1֮��

            // �����´β��ŵ�ʱ��
            nextFoot = Time.time + FootStep_InterTime;
        }
    }

    // ��ȡ���ʵ�������Ч
    AudioClip GetFootstepClip(byte blockType)
    {
        // ���������Ƿ���ר����Ч
        AudioClip[] clips = managerhub.world.blocktypes[blockType].walk_clips;

        // ��������͵���Ч��������Ч�����ض�Ӧ����Ч
        if (blockType != VoxelData.Air && blockType != 255 && clips[item] != null)
            return clips[item];

        // ���򣬷���Ĭ��ʯͷ��Ч
        return managerhub.world.blocktypes[VoxelData.Stone].walk_clips[item];
    }


    #endregion 


    #region ��Ӿ��Ч

    void _ReferUpdate_Water()
    {
        bool isCurrentlyInWater = Collider_Component.FootBlockType == VoxelData.Water;

        // �л���ˮ/��ˮ״̬
        if (isCurrentlyInWater != isInTheWater)
        {
            // ������Ч
            MainAudioSource.PlayOneShot(musicManager.audioclips[MusicData.fall_water]);

            // ����״̬
            isInTheWater = isCurrentlyInWater;
        }

        //��Ӿ
        //if (isInTheWater && Velocity_Component.isMoving)
        //{
        //    MainAudioSources[MusicData.AudioSource_Swimming].Play();
        //}
        //else
        //{
        //    if (MainAudioSources[MusicData.AudioSource_Swimming].isPlaying)
        //    {
        //        MainAudioSources[MusicData.AudioSource_Swimming].Stop();
        //    }
        //}

    }


    #endregion

}
