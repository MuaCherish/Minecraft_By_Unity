using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    //Transformers
    public World world;
    public Player player;

    //片段
    public AudioClip[] audioclips;

    //音源
    public AudioSource Audio_envitonment;
    public AudioSource Audio_player_place;
    public AudioSource Audio_player_broke;
    public AudioSource Audio_player_move;

    //协程
    private Coroutine environmentCoroutine;

    //envitonment
    private bool isPausing = false;

    //place and broke
    public bool isbroking;
    public bool isPlacing;

    private void Start()
    {
        Audio_envitonment = gameObject.AddComponent<AudioSource>();
        Audio_envitonment.clip = audioclips[VoxelData.bgm_menu];
        Audio_envitonment.loop = false;
        Audio_envitonment.Play();

        Audio_player_place = gameObject.AddComponent<AudioSource>();
        Audio_player_place.loop = false;

        Audio_player_broke = gameObject.AddComponent<AudioSource>();
        Audio_player_broke.loop = false;

        Audio_player_move = gameObject.AddComponent<AudioSource>();
        Audio_player_move.loop = false;

    }

    private void Update()
    {
       
        Fun_environment();

        if (world.game_state == Game_State.Playing)
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
                        if (player.broke_Block_type != 255)
                        {
                            isbroking = true;
                            update_Clips();
                            Audio_player_broke.Play();
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

            //moving
        }


    }

    //---------------------------------- envitonment ----------------------------------------
    void Fun_environment()
    {
        if (world.game_state == Game_State.Loading)
        {
            Audio_envitonment.Stop();
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
                Audio_envitonment.Pause();
            }
            else
            {
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
                    Audio_envitonment.clip = audioclips[VoxelData.bgm_1];
                    Audio_envitonment.volume = 0.5f;
                    Audio_envitonment.Play();
                    bgm_sequence = 2;
                }
                else if (bgm_sequence == 2)
                {
                    Audio_envitonment.clip = audioclips[VoxelData.bgm_2];
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






    //------------------------------------ place --------------------------------------------


    //---------------------------------------------------------------------------------------






    //------------------------------------ broke --------------------------------------------
    //type映射到music下标
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


    //---------------------------------------------------------------------------------------

}


