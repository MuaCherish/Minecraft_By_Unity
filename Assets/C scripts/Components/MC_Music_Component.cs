using Homebrew;
using MCEntity;
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

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        Velocity_Component = GetComponent<MC_Velocity_Component>();
        Collider_Component = GetComponent<MC_Collider_Component>();
    }

    private void Start()
    {
        InitAudioSource();
        _ReferStart_ClipCheck();
    }

    private void Update()
    {
        if (managerhub.world.game_state == Game_State.Playing)
        {
            _ReferUpdate_FootStep();
            _ReferUpdate_Water();
        }
    }

    #endregion


    #region ����������

    //�ر�������
    GameObject AudioSourceObject;
    public AudioSource[] MainAudioSources;
    private int MainAudioSourceCount = 3;

    [Foldout("ʵ����Ի�Ƭ��", true)]
    [Header("������Ч")] public AudioClip BehurtClip; //Ĭ��Ϊʷ��ķ��Ч
    [Header("������Ч")] public AudioClip DeathClip; //Ĭ��Ϊ��������Ч
    [Header("�԰���Ч")] public AudioClip[] SayingClips;

    [Foldout("����������", true)]
    [Header("��ҿ���������Զ����")] public float MaxDistanceToHear = 5f;

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
        // ��������-����Ѿ�����Object�Ļ�
        if (AudioSourceObject != null)
            return;

        // ��ʼ��������
        MainAudioSources = new AudioSource[MainAudioSourceCount];

        // �������ظ��������GameObject������ȫ��������
        AudioSourceObject = new GameObject("AudioSourceObject");
        AudioSourceObject.transform.SetParent(this.transform);

        for (int i = 0; i < MainAudioSourceCount; i++)
        {
            AudioSource sourceTemp = AudioSourceObject.AddComponent<AudioSource>();

            // ���ø��������Ϊ3D��Ч
            sourceTemp.spatialBlend = 1.0f; // 1.0��ʾ��ȫ3D

            // ���������������
            sourceTemp.maxDistance = MaxDistanceToHear;

            sourceTemp.volume = 0.8f;

            // �����������ã���ѡ��
            sourceTemp.rolloffMode = AudioRolloffMode.Linear; // �������������˥��ģʽ
            sourceTemp.dopplerLevel = 0; // ���ö�����ЧӦ

            //��������������

            //����Clip
            if (i == MusicData.AudioSource_Swimming)
            {
                sourceTemp.clip = managerhub.NewmusicManager.audioclips[MusicData.fall_water];
            }

            MainAudioSources[i] = sourceTemp;
        }
    }

    #endregion


    #region ����

    /// <summary>
    /// ����Ƭ��
    /// </summary>
    public void PlaySound(AudioClip _Clip)
    {
        MainAudioSources[MusicData.AudioSource_AnyOneShot].PlayOneShot(_Clip);
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
            MainAudioSources[0].PlayOneShot(clipToPlay);
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
            // �����ƵԴ�Ƿ�Ϊ null
            if (MainAudioSources[0] == null)
            {
                Debug.LogError("AudioSource is null.");
                return;
            }

            // �����Ƶ�����Ƿ���Ч
            if (managerhub.NewmusicManager.audioclips[MusicData.fall_water] == null)
            {
                Debug.LogError("AudioClip for fall_water is null.");
                return;
            }

            // �����ƵԴ����������
            if (MainAudioSources[0].volume == 0)
            {
                Debug.LogWarning("AudioSource volume is set to 0.");
            }

            // ��� AudioListener �Ƿ��ھ���״̬
            if (AudioListener.volume == 0)
            {
                Debug.LogWarning("AudioListener volume is set to 0.");
            }

            // �����ƵԴ�Ƿ����ڲ���
            if (MainAudioSources[0].isPlaying)
            {
                Debug.LogWarning("AudioSource is already playing another clip.");
                return; // ������ڲ��ţ������µ���Ч������
            }

            // ������Ч
            print("����oneshot");
            MainAudioSources[0].PlayOneShot(managerhub.NewmusicManager.audioclips[MusicData.fall_water]);

            // ����״̬
            isInTheWater = isCurrentlyInWater;
        }

        //��Ӿ
        if (isInTheWater && Velocity_Component.isMoving)
        {
            MainAudioSources[MusicData.AudioSource_Swimming].Play();
        }
        else
        {
            if (MainAudioSources[MusicData.AudioSource_Swimming].isPlaying)
            {
                MainAudioSources[MusicData.AudioSource_Swimming].Stop();
            }
        }

    }


    #endregion

}
