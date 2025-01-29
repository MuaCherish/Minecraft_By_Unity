using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
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
    AudioSource[] MainAudioSources;
    private int MainAudioSourceCount = 3;

    [Foldout("���Ի�Ƭ��", true)]
    [Header("������Ч�±�")] public int BeHurtIndex = 26; //Ĭ��Ϊʷ��ķ��Ч
    [Header("������Ч�±�")] public int DeathIndex = 28; //Ĭ��Ϊ��������Ч

    [Foldout("����������", true)]
    [Header("��ҿ���������Զ����")] public float MaxDistanceToHear;

    //����������
    [Foldout("���ಥ��������", true)]
    [Header("���ಥ����")] public AudioSource[] OtherAudioSources;


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

            // �����������ã���ѡ��
            //sourceTemp.rolloffMode = AudioRolloffMode.Linear; // �������������˥��ģʽ
            //sourceTemp.dopplerLevel = 0; // ���ö�����ЧӦ

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


    #region [Public]+�ı�����+������Ƶ

    /// <summary>
    /// �����ٷֱȣ����������в��������øı�
    /// </summary>
    /// <param name="_value">�ٷֱ�</param>
    public void ChangeSoundSetting(float _value)
    {

    }


    /// <summary>
    /// ���Ÿ��ಥ����
    /// </summary>
    public void PlayOtherSound(int _index, int _id)
    {
        //�ǵ�Ҫ�����л�
    }



    #endregion


    #region �Ų���ʵ��

    [Foldout("�Ų���", true)]
    [Header("�Ų������ʱ��")] public float FootStep_InterTime = 0.3f;
    int item = 0;   //�����������ҽŵ�
    private float nextFoot = 0f;


    void _ReferUpdate_FootStep()
    {
        // ��ǰ���� - ��Ҿ�ֹ���ڿ���
        if (!Velocity_Component.isMoving || !Collider_Component.isGround) return;

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


    #region ˮ���л���Ч����Ӿʵ��


    void _ReferUpdate_Water()
    {
        bool isCurrentlyInWater = Collider_Component.FootBlockType == VoxelData.Water;

        // �л���ˮ/��ˮ״̬
        if (isCurrentlyInWater != isInTheWater)
        {
            // ������Ч
            MainAudioSources[0].PlayOneShot(managerhub.NewmusicManager.audioclips[MusicData.fall_water]);

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
