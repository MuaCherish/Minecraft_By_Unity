using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
//using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    //Transformers
    [Header("Transforms")]
    public World world;
    public Player player;
    public Transform eyes;
    public Transform leg;
    public Transform foot;

    //片段
    [Header("音乐片段")]
    public AudioClip[] audioclips;

    //音源
    [HideInInspector]
    public AudioSource Audio_envitonment;
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

    //协程
    private Coroutine environmentCoroutine;

    //envitonment
    private bool isPausing = false;

    //place and broke
    [HideInInspector]
    public bool isbroking = false;

    //moving
    [Header("玩家/leg/foot状态")]
    public bool hasExec_isGround = true;
    public bool hasExec_isSwiming = true;
    public byte leg_blocktype;
    [HideInInspector]
    public byte previous_leg_blocktype = VoxelData.Air;
    public byte foot_blocktype;
    //public byte previous_foot_blocktype = VoxelData.Air;

    //一次性代码
    bool hasExec_StopEnvironment = true;



    //---------------------------------- 周期函数 ----------------------------------------

    private void Start()
    {
        //environment
        Audio_envitonment = gameObject.AddComponent<AudioSource>();
        Audio_envitonment.clip = audioclips[VoxelData.bgm_menu];
        Audio_envitonment.loop = false;
        Audio_envitonment.Play();

        //place
        Audio_player_place = gameObject.AddComponent<AudioSource>();
        Audio_player_place.loop = false;

        //broke
        Audio_player_broke = gameObject.AddComponent<AudioSource>();
        Audio_player_broke.loop = false;

        //moving
        Audio_player_moving = gameObject.AddComponent<AudioSource>();
        Audio_player_moving.volume = 0.7f;
        Audio_player_moving.loop = false;

        //falling
        Audio_player_falling = gameObject.AddComponent<AudioSource>();
        Audio_player_falling.volume = 0.5f;
        Audio_player_falling.loop = false;

        //diving
        Audio_player_diving = gameObject.AddComponent<AudioSource>();
        Audio_player_diving.clip = audioclips[VoxelData.dive];
        Audio_player_diving.volume = 0.5f;
        Audio_player_diving.loop = true;

        //Click_Music
        Audio_Click = gameObject.AddComponent<AudioSource>();
        Audio_Click.volume = 0.5f;
        Audio_Click.loop = false;
    }

    private void Update()
    {
        Fun_environment();

        FUN_PlaceandBroke();

        FUN_Moving();
    }


    //---------------------------------------------------------------------------------------


    



    //---------------------------------- envitonment ----------------------------------------
    public void PlaySound_Click()
    {
        Audio_Click.PlayOneShot(audioclips[VoxelData.click]);
    }

    void Fun_environment()
    {
        if (world.game_state == Game_State.Loading)
        {
            if (hasExec_StopEnvironment)
            {
                Audio_envitonment.Stop();
                hasExec_StopEnvironment = false;
            }
            
        }
        else if (world.game_state == Game_State.Playing)
        {
            if (environmentCoroutine == null)
            {
                environmentCoroutine = StartCoroutine(envitonment_Playing());
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPausing = !isPausing;

            if (isPausing)
            {
                Audio_player_diving.volume = 0f;
                Audio_player_moving.volume = 0f;
                Audio_envitonment.Pause();
            }
            else
            {
                Audio_player_diving.volume = 0.5f;
                Audio_player_moving.volume = 0.7f;
                Audio_envitonment.UnPause();
            }

        }


    }

    IEnumerator envitonment_Playing()
    {

        yield return new WaitForSeconds(3f);

        int bgm_sequence = 1;

        while (true)
        {
            // 当音乐播放完毕 && 没有暂停 才切换音乐
            if (!Audio_envitonment.isPlaying && !isPausing)
            {
                if (bgm_sequence == 1)
                {
                    Audio_envitonment.clip = audioclips[VoxelData.bgm_3];
                    Audio_envitonment.volume = 0.5f;
                    Audio_envitonment.Play();
                    bgm_sequence = 2;
                }
                else if (bgm_sequence == 2)
                {
                    Audio_envitonment.clip = audioclips[VoxelData.bgm_2];
                    Audio_envitonment.volume = 0.5f;
                    Audio_envitonment.Play();
                    bgm_sequence = 3;
                }
                else if (bgm_sequence == 3)
                {
                    Audio_envitonment.clip = audioclips[VoxelData.bgm_1];
                    Audio_envitonment.volume = 0.5f;
                    Audio_envitonment.Play();
                    bgm_sequence = 1;
                }
            }

            if (Audio_envitonment.isPlaying)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(3f);
            }
            
        }
    }


    //---------------------------------------------------------------------------------------






    //-------------------------------- place and Broke --------------------------------------
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
                    //如果打中
                    if (player.broke_Block_type != VoxelData.notHit)
                    {
                        isbroking = true;
                        update_Clips();
                        Audio_player_broke.Play();
                    }
                    else
                    {
                        Audio_player_broke.Stop();
                    }
                }
            }



            //松开左键
            if (Input.GetMouseButtonUp(0))
            {
                isbroking = false;
                Audio_player_broke.Stop();

            }

            //右键放置
            if (Input.GetMouseButtonDown(1))
            {
                Audio_player_place.PlayOneShot(audioclips[VoxelData.place_normal]);
            }
        }
    }

    //type映射到clips
    private void update_Clips()
    {
        //Leaves
        if (player.broke_Block_type == VoxelData.Leaves)
        {
            Audio_player_broke.clip = audioclips[VoxelData.broke_leaves];
        }//Sand
        else if (player.broke_Block_type == VoxelData.Sand)
        {
            Audio_player_broke.clip = audioclips[VoxelData.broke_sand];
        }//Grass_Soil
        else if (player.broke_Block_type == VoxelData.Grass)
        {
            Audio_player_broke.clip = audioclips[VoxelData.broke_soil];
        }//Soil
        else if (player.broke_Block_type == VoxelData.Soil)
        {
            Audio_player_broke.clip = audioclips[VoxelData.broke_soil];
        }//Wood
        else if (player.broke_Block_type == VoxelData.Wood)
        {
            Audio_player_broke.clip = audioclips[VoxelData.broke_wood];
        }//else all Stone
        else
        {
            Audio_player_broke.clip = audioclips[VoxelData.broke_stone];
        }

    }
    //---------------------------------------------------------------------------------------






    //------------------------------------ moving -------------------------------------------
    void FUN_Moving()
    {
        if (world.game_state == Game_State.Playing)
        {
            //换碟
            UpdateMovingClip();


            // 如果玩家移动并且音频未播放，并且玩家在地面上或者游泳中，则播放音频
            if (player.isMoving && !Audio_player_moving.isPlaying && (player.isGrounded || player.isSwiming))
            {
                Audio_player_moving.Play();
            }
            // 如果玩家未移动并且正在播放音频，并且玩家在地面上，则暂停音频
            else if (!player.isMoving && Audio_player_moving.isPlaying && player.isGrounded)
            {
                Audio_player_moving.Pause();
            }
            // 如果音频正在播放，并且玩家不在地面上且不在游泳中，则暂停音频
            else if (Audio_player_moving.isPlaying && !player.isGrounded && !player.isSwiming)
            {
                Audio_player_moving.Pause();
            }



            //flop to Water
            ifmovingStateSwitch();

            //flop to ground在player中被调用

            //diving
            playsound_diving();
        }
    }
    
    //换碟
    void UpdateMovingClip()
    {
        //如果isground和isswimming同时亮，那么只能执行swimming
        if (player.isGrounded && player.isSwiming)
        {
            //swiming
            if (player.isSwiming)
            {
                if (hasExec_isGround)
                {
                    //Debug.Log("切换到水中");

                    //如果玩家离地高度为0，那么就变0 
                    if (player.new_foot_high - player.transform.position.y <= 0f)
                    {
                        player.verticalMomentum = 0f;
                    }

                    hasExec_isSwiming = true;
                    Audio_player_moving.clip = audioclips[VoxelData.moving_water];
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
                    Audio_player_moving.clip = audioclips[VoxelData.moving_normal];
                    hasExec_isSwiming = false;
                }
            }

            //swiming
            if (player.isSwiming)
            {
                if (hasExec_isGround)
                {
                    //Debug.Log("切换到水中");

                    //如果玩家离地高度为0，那么就变0 
                    if (player.new_foot_high - player.transform.position.y <= 0f)
                    {
                        player.verticalMomentum = 0f;
                    }

                    hasExec_isSwiming = true;
                    Audio_player_moving.clip = audioclips[VoxelData.moving_water];
                    hasExec_isGround = false;
                }

            }

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

    //如果玩家状态发生切换的时候换成落水音效
    void ifmovingStateSwitch()
    {
        leg_blocktype = classifytype();

        if (leg_blocktype != previous_leg_blocktype)
        {
            Audio_player_falling.PlayOneShot(audioclips[VoxelData.fall_water]);
            previous_leg_blocktype = leg_blocktype;
        }

    }

    //播放摔落音效
    public void PlaySound_fallGround()
    {
        Audio_player_falling.PlayOneShot(audioclips[VoxelData.fall_high]);
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


    //--------------------------------------------------------------------------------------






    //------------------------------------ 工具类 -------------------------------------------



    //---------------------------------------------------------------------------------------




}
