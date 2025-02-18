using Homebrew;
using MCEntity;
using System.Collections;
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

    public AudioClip _clip;    // 要播放的音频剪辑
    private void Update()
    {
        if (managerhub.world.game_state == Game_State.Playing)
        {
            _ReferUpdate_FootStep();
            _ReferUpdate_Water();

            // 检测鼠标左键点击
            if (Input.GetKeyDown(KeyCode.H))  // 0 是鼠标左键
            {
                // 播放 OneShot 音频
                PlaySound(_clip);
            }
        }
    }

    #endregion


    #region 播放器管理

    [Foldout("播放器初始化设置", true)]
    [Header("最远距离")] public float MaxDistant = 5f;
    [Header("音量")] public float AudioVolume = 0.8f;
    private AudioSource MainAudioSource;

    [Foldout("实体个性化片段", true)]
    [Header("受伤音效")] public AudioClip BehurtClip; //默认为史莱姆音效
    [Header("死亡音效")] public AudioClip DeathClip; //默认为苦力怕音效
    [Header("旁白音效")] public AudioClip[] SayingClips;
    [Header("旁白音效延迟范围")] public Vector2 SayingDelayRange = new Vector2(5f, 30f);

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
        //GameObject
        GameObject AudioSourceObject = new GameObject("Audio-Object");
        AudioSourceObject.transform.SetParent(this.transform);
        AudioSourceObject.transform.localPosition = Vector3.zero;

        //AudioSource
        MainAudioSource = AudioSourceObject.AddComponent<AudioSource>();
        MainAudioSource.spatialBlend = 1f; // 1.0表示完全3D
        MainAudioSource.maxDistance = MaxDistant;
        MainAudioSource.volume = AudioVolume;
        MainAudioSource.rolloffMode = AudioRolloffMode.Logarithmic; // 设置音量随距离衰减模式
        MainAudioSource.dopplerLevel = 0; // 禁用多普勒效应
        MainAudioSource.playOnAwake = false;
    }

    #endregion


    #region 播放

    /// <summary>
    /// 播放片段
    /// </summary>
    public void PlaySound(AudioClip _Clip)
    {
        MainAudioSource.PlayOneShot(_Clip);
    }

    #endregion


    #region 旁白音效

    IEnumerator Coroutine_Saying()
    {
        //提前返回-空的
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
    /// 尝试叫一声
    /// </summary>
    public void TrySaying()
    {
        //提前返回-空的
        if (SayingClips.Length == 0)
            return;

        int index = Random.Range(0, SayingClips.Length);
        MainAudioSource.PlayOneShot(SayingClips[index]);

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
            MainAudioSource.PlayOneShot(clipToPlay);
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
            // 播放音效
            MainAudioSource.PlayOneShot(musicManager.audioclips[MusicData.fall_water]);

            // 更新状态
            isInTheWater = isCurrentlyInWater;
        }

        //游泳
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
