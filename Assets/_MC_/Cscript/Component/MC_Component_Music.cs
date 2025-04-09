using Homebrew;
using MCEntity;
using System.Collections;
using UnityEngine;

/// <summary>
/// �����Ҫʵ�ֵĻ��У�
/// ���Ǳ��û����
/// </summary>

[RequireComponent(typeof(MC_Component_Physics))]
[RequireComponent(typeof(MC_Component_Velocity))]
public class MC_Component_Music : MonoBehaviour
{

    #region ״̬

    [Foldout("״̬", true)]
    [Header("�Ƿ���ˮ��")] public bool isInTheWater;

    #endregion


    #region ���ں���

    ManagerHub managerhub;
    MC_Component_Velocity Component_Velocity;
    MC_Component_Physics Component_Physics;
    MC_Service_Music Service_Music;

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        Component_Velocity = GetComponent<MC_Component_Velocity>();
        Component_Physics = GetComponent<MC_Component_Physics>();
        Service_Music = managerhub.Service_Music;
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
        if (MC_Runtime_DynamicData.instance.GetGameState() == Game_State.Playing)
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

    /// <summary>
    /// ����Ƭ��
    /// </summary>
    public void PlaySound(int _index)
    {
        AudioClip _clip = managerhub.Service_Music.audioclips[_index];
        MainAudioSource.PlayOneShot(_clip);
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
        if (!Component_Velocity.isMoving || !Component_Physics.isGround || hasFootStep == false) 
            return;

        // ����Ƿ񵽴���һ������ʱ��
        if (Time.time >= nextFoot)
        {
            // ��ȡ���ŵ���Ч������ʹ��ר����Ч������Ĭ��ʯͷ��Ч
            AudioClip clipToPlay = GetFootstepClip(Component_Physics.FootBlockType);

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
        AudioClip[] clips = managerhub.Service_World.blocktypes[blockType].walk_clips;

        // ��������͵���Ч��������Ч�����ض�Ӧ����Ч
        if (blockType != VoxelData.Air && blockType != 255 && clips[item] != null)
            return clips[item];

        // ���򣬷���Ĭ��ʯͷ��Ч
        return managerhub.Service_World.blocktypes[VoxelData.Stone].walk_clips[item];
    }


    #endregion 


    #region ��Ӿ��Ч

    void _ReferUpdate_Water()
    {
        bool isCurrentlyInWater = Component_Physics.FootBlockType == VoxelData.Water;

        // �л���ˮ/��ˮ״̬
        if (isCurrentlyInWater != isInTheWater)
        {
            // ������Ч
            MainAudioSource.PlayOneShot(Service_Music.audioclips[MusicData.fall_water]);

            // ����״̬
            isInTheWater = isCurrentlyInWater;
        }

        //��Ӿ
        //if (isInTheWater && Component_Velocity.isMoving)
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
