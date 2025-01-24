using Homebrew;
using System.Collections;
using UnityEngine;


public class MusicManager : MonoBehaviour
{

    #region 周期函数

    ManagerHub managerhub;
    World world;
    Player player;


    //片段
    [Header("音乐片段")]
    public AudioClip[] audioclips;

    //音源
    //[HideInInspector]
    //public AudioSource Audio_envitonment;
    [HideInInspector]
    public AudioSource Audio_player_place;
    [HideInInspector]
    public AudioSource Audio_player_broke;
    [HideInInspector]
    public AudioSource Audio_player_moving;
    [HideInInspector]
    public AudioSource Audio_player_falling;
    [HideInInspector]
    public AudioSource Audio_player_diving;
    [HideInInspector]
    public AudioSource Audio_Click;
    [HideInInspector]
    public AudioSource Audio_Player_moving_swiming;



    private void Awake()
    {
        managerhub = GlobalData.GetManagerhub();
        world = managerhub.world;
        player = managerhub.player;
    }


    private void Start()
    {
        InitMusicManager();
    }

 
    private void Update()
    {
        if (world.game_state == Game_State.Playing)
        {
            //脚步音效
            PlaySound_Foot();
        }
    }

    private void FixedUpdate()
    {
        FUN_PlaceandBroke();

        FUN_Moving();


        Fade_FallInto_Water();

        if (world.game_state == Game_State.Playing)
        {
            UpdateFootBlockType();
        }

        
    }

    public void InitMusicManager()
    {
        //environment
        //if (Audio_envitonment == null)
        //{
        //    Audio_envitonment = gameObject.AddComponent<AudioSource>();
        //}

        //Audio_envitonment.clip = audioclips[MusicData.bgm_menu];
        //Audio_envitonment.volume = 0.4f;
        //Audio_envitonment.loop = false;
        //Audio_envitonment.Play();

        //place
        if (Audio_player_place == null)
        {
            Audio_player_place = gameObject.AddComponent<AudioSource>();
        }
        Audio_player_place.volume = 0.2f;
        Audio_player_place.loop = false;

        //broke
        if (Audio_player_broke == null)
        {
            Audio_player_broke = gameObject.AddComponent<AudioSource>();
        }
        Audio_player_broke.volume = 0.2f;
        Audio_player_broke.loop = true;

        //moving
        if (Audio_player_moving == null)
        {
            Audio_player_moving = gameObject.AddComponent<AudioSource>();
        }
        Audio_player_moving.volume = 0.3f;
        Audio_player_moving.loop = false;

        //falling
        if (Audio_player_falling == null)
        {
            Audio_player_falling = gameObject.AddComponent<AudioSource>();
        }
        Audio_player_falling.volume = 0.2f;
        Audio_player_falling.loop = false;

        //diving
        if (Audio_player_diving == null)
        {
            Audio_player_diving = gameObject.AddComponent<AudioSource>();
        }
        Audio_player_diving.clip = audioclips[MusicData.dive];
        Audio_player_diving.volume = 0.4f;
        Audio_player_diving.loop = true;

        //Click_Music
        if (Audio_Click == null)
        {
            Audio_Click = gameObject.AddComponent<AudioSource>();
        }
        Audio_Click.volume = 0.2f;
        Audio_Click.loop = false;

        //swimming
        if (Audio_Player_moving_swiming == null)
        {
            Audio_Player_moving_swiming = gameObject.AddComponent<AudioSource>();
        }
        Audio_Player_moving_swiming.clip = audioclips[MusicData.moving_water];
        Audio_Player_moving_swiming.volume = 0.4f;
        Audio_Player_moving_swiming.loop = true;

        //walking
        footstepInterval = PlayerData.walkSpeed;
    }


    #endregion


    #region OneShot

    //place and broke
    [ReadOnly] public bool isbroking = false;


    public void PlaySoung_Place()
    {
        if (managerhub.backpackManager.istheindexHaveBlock(player.selectindex))
        {
            managerhub.NewmusicManager.PlayOneShot(world.blocktypes[managerhub.backpackManager.slots[player.selectindex].blockId].broken_clip);

        }


    }


    #endregion


    #region Broking

    void FUN_PlaceandBroke()
    {
        //place and broke
        if (world.game_state == Game_State.Playing)
        {

            //只有没有破坏的情况下才执行
            if (!isbroking)
            {
                //左键破坏
                if (Input.GetMouseButton(0))
                {

                    RayCastStruct _rayCast = player.NewRayCast(player.cam.position, player.cam.transform.forward, player.reach);

                    //如果打中
                    if (player.point_Block_type != PlayerData.notHit && _rayCast.isHit == 1)
                    {
                        isbroking = true;

                        if (world.blocktypes[player.point_Block_type].broking_clip != null)
                        {
                            Audio_player_broke.clip = world.blocktypes[player.point_Block_type].broking_clip;
                        }
                        else
                        {
                            Audio_player_broke.clip = world.blocktypes[VoxelData.Stone].broking_clip;
                        }


                        Audio_player_broke.Play();
                    }
                    else
                    {
                        Audio_player_broke.Stop();
                    }
                }

            }


        }
    }


    #endregion


    #region 游泳，入水，潜水

    //moving
    [Header("玩家/leg/foot状态")]
    [HideInInspector] public bool hasExec_isGround = true;
    [HideInInspector] public bool hasExec_isSwiming = true;
    [HideInInspector] public byte leg_blocktype;
    [HideInInspector] public byte previous_leg_blocktype = VoxelData.Air;

    public Transform eyes;
    public Transform leg;
    public Transform foot;


    //当脚下方块切换的时候触发
    void UpdateFootBlockType()
    {
        footBlocktype = world.GetBlockType(foot.position);

        if (footBlocktype != previous_foot_blocktype)
        {
            previous_foot_blocktype = footBlocktype;
        }
    }

    //播放潜水音效
    void playsound_diving()
    {
        if (world.GetBlockType(eyes.position) == VoxelData.Water && !Audio_player_diving.isPlaying)
        {
            Audio_player_diving.Play();
        }

        if (world.GetBlockType(eyes.position) != VoxelData.Water)
        {
            Audio_player_diving.Stop();
        }
    }


    //将给定type分类为Air和water
    byte classifytype()
    {
        byte a = world.GetBlockType(leg.position);

        if (a == VoxelData.Water)
        {
            return VoxelData.Water;
        }
        else
        {
            return VoxelData.Air;
        }
    }

    void FUN_Moving()
    {
        if (world.game_state == Game_State.Playing)
        {
            //游泳音效
            PlaySound_Swiming();

            //出入水音效
            ifmovingStateSwitch();

            //flop to ground在player中被调用

            //潜水音效
            playsound_diving();
        }
    }

    //游泳音效
    void PlaySound_Swiming()
    {
        if (player.isSwiming && player.isMoving)
        {
            if (!Audio_Player_moving_swiming.isPlaying)
            {
                Audio_Player_moving_swiming.Play();
            }

        }
        else
        {
            if (Audio_Player_moving_swiming.isPlaying)
            {
                Audio_Player_moving_swiming.Pause();
            }

        }
    }

    //玩家站在水边走向水里可以缓慢入水而不是-20入水
    void Fade_FallInto_Water()
    {
        //如果isground和isswimming同时亮，那么只能执行swimming
        if (player.isGrounded && player.isSwiming)
        {
            //swiming
            if (player.isSwiming)
            {
                if (hasExec_isGround)
                {
                    //如果玩家离地高度为0，那么就变0 
                    if (player.new_foot_high - player.transform.position.y <= 0f)
                    {
                        player.verticalMomentum = 0f;
                    }
                    //Debug.Log("切换到水中");
                    hasExec_isSwiming = true;
                    //Audio_player_moving.clip = audioclips[VoxelData.moving_water];
                    hasExec_isGround = false;
                }

            }
        }
        else
        {

            //ground
            if (player.isGrounded)
            {
                if (hasExec_isSwiming)
                {
                    //Debug.Log("切换到地面");
                    hasExec_isGround = true;
                    //Audio_player_moving.clip = audioclips[VoxelData.moving_normal];
                    hasExec_isSwiming = false;
                }
            }

            //swiming
            if (player.isSwiming)
            {
                if (hasExec_isGround)
                {
                    //如果玩家离地高度为0，那么就变0 
                    if (player.new_foot_high - player.transform.position.y <= 0f)
                    {
                        player.verticalMomentum = 0f;
                    }
                    //Debug.Log("切换到水中");
                    hasExec_isSwiming = true;
                    //Audio_player_moving.clip = audioclips[VoxelData.moving_water];
                    hasExec_isGround = false;
                }

            }

        }
    }

    //如果玩家状态发生切换的时候换成落水音效
    void ifmovingStateSwitch()
    {

        leg_blocktype = classifytype();

        if (leg_blocktype != previous_leg_blocktype)
        {
            managerhub.NewmusicManager.PlayOneShot(MusicData.fall_water);
            previous_leg_blocktype = leg_blocktype;
        }

    }



    #endregion


    #region 玩家脚步声实现

    //walk
    int item = 0;   //用来区分左右脚的
    [HideInInspector] public byte footBlocktype = VoxelData.Grass;
    [HideInInspector] public byte previous_foot_blocktype = VoxelData.Grass;
    [HideInInspector] public float footstepInterval; // 走路音效播放间隔
    private float nextFoot;

    //脚步声
    void PlaySound_Foot()
    {
        // 如果玩家移动并且音频未播放，并且玩家在地面上，则播放音频
        if (player.isInputing && player.isGrounded && !player.isSquating)
        {
            if (Time.time >= nextFoot)
            {
                //如果不是空气则播放
                if (footBlocktype != VoxelData.Air)
                {
                    //如果有专属音效
                    if (footBlocktype != 255 && world.blocktypes[footBlocktype].walk_clips[item] != null)
                    {
                        if (item == 0)
                        {
                            managerhub.NewmusicManager.PlayOneShot(world.blocktypes[footBlocktype].walk_clips[item]);
                            item = 1;
                        }
                        else
                        {
                            managerhub.NewmusicManager.PlayOneShot(world.blocktypes[footBlocktype].walk_clips[item]);
                            item = 0;
                        }
                    }
                    //如果没有专属走路音效，则默认播放石头音效
                    else
                    {
                        if (item == 0)
                        {
                            managerhub.NewmusicManager.PlayOneShot(world.blocktypes[VoxelData.Stone].walk_clips[item]);
                            item = 1;
                        }
                        else
                        {
                            managerhub.NewmusicManager.PlayOneShot(world.blocktypes[VoxelData.Stone].walk_clips[item]);
                            item = 0;
                        }
                    }
                }
                
                

                nextFoot = Time.time + footstepInterval;
            }
        }

    }

    #endregion


}


