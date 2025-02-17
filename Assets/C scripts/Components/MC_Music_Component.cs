using Homebrew;
using MCEntity;
using UnityEngine;

/// <summary>
/// 玩家需要实现的还有：
/// 玩家潜行没声音
/// </summary>

[RequireComponent(typeof(MC_Collider_Component))]
[RequireComponent(typeof(MC_Velocity_Component))]
public class MC_Music_Component : MonoBehaviour
{

    #region 状态

    [Foldout("状态", true)]
    [Header("是否在水中")] public bool isInTheWater;

    #endregion


    #region 周期函数

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


    #region 播放器管理

    //必备播放器
    GameObject AudioSourceObject;
    public AudioSource[] MainAudioSources;
    private int MainAudioSourceCount = 3;

    [Foldout("实体个性化片段", true)]
    [Header("受伤音效")] public AudioClip BehurtClip; //默认为史莱姆音效
    [Header("死亡音效")] public AudioClip DeathClip; //默认为苦力怕音效
    [Header("旁白音效")] public AudioClip[] SayingClips;

    [Foldout("播放器参数", true)]
    [Header("玩家可听到的最远距离")] public float MaxDistanceToHear = 5f;

    void _ReferStart_ClipCheck()
    {
        if(BehurtClip == null)
        {
            print("BehurtClip未设置");
        }

        if (DeathClip == null)
        {
            print("DeathClip未设置");
        }
    }

    // 初始化播放器
    void InitAudioSource()
    {
        // 条件返回-如果已经存在Object的话
        if (AudioSourceObject != null)
            return;

        // 初始化管理器
        MainAudioSources = new AudioSource[MainAudioSourceCount];

        // 创建挂载父类的子类GameObject，挂载全部播放器
        AudioSourceObject = new GameObject("AudioSourceObject");
        AudioSourceObject.transform.SetParent(this.transform);

        for (int i = 0; i < MainAudioSourceCount; i++)
        {
            AudioSource sourceTemp = AudioSourceObject.AddComponent<AudioSource>();

            // 设置该声音组件为3D音效
            sourceTemp.spatialBlend = 1.0f; // 1.0表示完全3D

            // 设置最大听觉距离
            sourceTemp.maxDistance = MaxDistanceToHear;

            sourceTemp.volume = 0.8f;

            // 其他参数配置（可选）
            sourceTemp.rolloffMode = AudioRolloffMode.Linear; // 设置音量随距离衰减模式
            sourceTemp.dopplerLevel = 0; // 禁用多普勒效应

            //配置音量等设置

            //配置Clip
            if (i == MusicData.AudioSource_Swimming)
            {
                sourceTemp.clip = managerhub.NewmusicManager.audioclips[MusicData.fall_water];
            }

            MainAudioSources[i] = sourceTemp;
        }
    }

    #endregion


    #region 播放

    /// <summary>
    /// 播放片段
    /// </summary>
    public void PlaySound(AudioClip _Clip)
    {
        MainAudioSources[MusicData.AudioSource_AnyOneShot].PlayOneShot(_Clip);
    }

    #endregion


    #region 脚步声

    [Foldout("脚步声", true)]
    [Header("实体是否有脚步声")] public bool hasFootStep = true;
    [Header("脚步声间隔时间")] public float FootStep_InterTime = 0.3f;
    int item = 0;   //用来区分左右脚的
    private float nextFoot = 0f;

    void _ReferUpdate_FootStep()
    {
        // 提前返回 - 玩家静止或在空中
        if (!Velocity_Component.isMoving || !Collider_Component.isGround || hasFootStep == false) 
            return;

        // 检查是否到达下一个播放时间
        if (Time.time >= nextFoot)
        {
            // 获取播放的音效，优先使用专属音效，否则默认石头音效
            AudioClip clipToPlay = GetFootstepClip(Collider_Component.FootBlockType);

            // 播放音效并切换item
            MainAudioSources[0].PlayOneShot(clipToPlay);
            item = 1 - item;  // 切换item在0和1之间

            // 更新下次播放的时间
            nextFoot = Time.time + FootStep_InterTime;
        }
    }

    // 获取合适的足音音效
    AudioClip GetFootstepClip(byte blockType)
    {
        // 检查该类型是否有专属音效
        AudioClip[] clips = managerhub.world.blocktypes[blockType].walk_clips;

        // 如果该类型的音效存在且有效，返回对应的音效
        if (blockType != VoxelData.Air && blockType != 255 && clips[item] != null)
            return clips[item];

        // 否则，返回默认石头音效
        return managerhub.world.blocktypes[VoxelData.Stone].walk_clips[item];
    }


    #endregion 


    #region 游泳音效

    void _ReferUpdate_Water()
    {
        bool isCurrentlyInWater = Collider_Component.FootBlockType == VoxelData.Water;

        // 切换入水/出水状态
        if (isCurrentlyInWater != isInTheWater)
        {
            // 检查音频源是否为 null
            if (MainAudioSources[0] == null)
            {
                Debug.LogError("AudioSource is null.");
                return;
            }

            // 检查音频剪辑是否有效
            if (managerhub.NewmusicManager.audioclips[MusicData.fall_water] == null)
            {
                Debug.LogError("AudioClip for fall_water is null.");
                return;
            }

            // 检查音频源的音量设置
            if (MainAudioSources[0].volume == 0)
            {
                Debug.LogWarning("AudioSource volume is set to 0.");
            }

            // 检查 AudioListener 是否处于静音状态
            if (AudioListener.volume == 0)
            {
                Debug.LogWarning("AudioListener volume is set to 0.");
            }

            // 检查音频源是否正在播放
            if (MainAudioSources[0].isPlaying)
            {
                Debug.LogWarning("AudioSource is already playing another clip.");
                return; // 如果正在播放，避免新的音效被覆盖
            }

            // 播放音效
            print("播放oneshot");
            MainAudioSources[0].PlayOneShot(managerhub.NewmusicManager.audioclips[MusicData.fall_water]);

            // 更新状态
            isInTheWater = isCurrentlyInWater;
        }

        //游泳
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
