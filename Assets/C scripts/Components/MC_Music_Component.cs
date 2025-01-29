using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
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
    AudioSource[] MainAudioSources;
    private int MainAudioSourceCount = 3;

    [Foldout("个性化片段", true)]
    [Header("受伤音效下标")] public int BeHurtIndex = 26; //默认为史莱姆音效
    [Header("死亡音效下标")] public int DeathIndex = 28; //默认为苦力怕音效

    [Foldout("播放器参数", true)]
    [Header("玩家可听到的最远距离")] public float MaxDistanceToHear;

    //其他播放器
    [Foldout("更多播放器设置", true)]
    [Header("更多播放器")] public AudioSource[] OtherAudioSources;


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

            // 其他参数配置（可选）
            //sourceTemp.rolloffMode = AudioRolloffMode.Linear; // 设置音量随距离衰减模式
            //sourceTemp.dopplerLevel = 0; // 禁用多普勒效应

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


    #region [Public]+改变音量+播放音频

    /// <summary>
    /// 给定百分比，将其下所有播放器设置改变
    /// </summary>
    /// <param name="_value">百分比</param>
    public void ChangeSoundSetting(float _value)
    {

    }


    /// <summary>
    /// 播放更多播放器
    /// </summary>
    public void PlayOtherSound(int _index, int _id)
    {
        //记得要缓慢切换
    }



    #endregion


    #region 脚步声实现

    [Foldout("脚步声", true)]
    [Header("脚步声间隔时间")] public float FootStep_InterTime = 0.3f;
    int item = 0;   //用来区分左右脚的
    private float nextFoot = 0f;


    void _ReferUpdate_FootStep()
    {
        // 提前返回 - 玩家静止或在空中
        if (!Velocity_Component.isMoving || !Collider_Component.isGround) return;

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


    #region 水面切换音效和游泳实现


    void _ReferUpdate_Water()
    {
        bool isCurrentlyInWater = Collider_Component.FootBlockType == VoxelData.Water;

        // 切换入水/出水状态
        if (isCurrentlyInWater != isInTheWater)
        {
            // 播放音效
            MainAudioSources[0].PlayOneShot(managerhub.NewmusicManager.audioclips[MusicData.fall_water]);

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
