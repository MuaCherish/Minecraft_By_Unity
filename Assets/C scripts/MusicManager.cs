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

    //Ƭ��
    [Header("����Ƭ��")]
    public AudioClip[] audioclips;

    //��Դ
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
    [HideInInspector]
    public AudioSource Audio_Player_moving_swiming;

    //Э��
    private Coroutine fadetoStopenvironment;
    private Coroutine environmentCoroutine;

    //envitonment
    private bool isPausing = false;

    //place and broke
    [HideInInspector]
    public bool isbroking = false;

    //moving
    [Header("���/leg/foot״̬")]
    public bool hasExec_isGround = true;
    public bool hasExec_isSwiming = true;
    public byte leg_blocktype;
    [HideInInspector]
    public byte previous_leg_blocktype = VoxelData.Air;
    //public byte foot_blocktype;
    //public byte previous_foot_blocktype = VoxelData.Air;

    //һ���Դ���
    public float footstepInterval; // ��·��Ч���ż��
    private float nextFootstepTime; 


    //---------------------------------- ���ں��� ----------------------------------------

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

        //swimming
        Audio_Player_moving_swiming = gameObject.AddComponent<AudioSource>();
        Audio_Player_moving_swiming.clip = audioclips[VoxelData.moving_water];
        Audio_Player_moving_swiming.loop = true;

        //walking
        footstepInterval = VoxelData.walkSpeed;
    }


    private void FixedUpdate()
    {
        Fun_environment();

        FUN_PlaceandBroke();

        FUN_Moving();


        Fade_FallInto_Water();

        //����
        UpdateMovingClip();

       
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
            if (fadetoStopenvironment == null)
            {
                fadetoStopenvironment = StartCoroutine(Fade_Stop_Environment());
            }


            //if (Input.GetKeyDown(KeyCode.Escape))
            //{
            //    isPausing = !isPausing;

            //    if (isPausing)
            //    {
            //        Audio_player_diving.volume = 0f;
            //        Audio_player_moving.volume = 0f;
            //        Audio_envitonment.Pause();
            //    }
            //    else
            //    {
            //        Audio_player_diving.volume = 0.5f;
            //        Audio_player_moving.volume = 0.7f;
            //        Audio_envitonment.UnPause();
            //    }

            //}






        }
        else if (world.game_state == Game_State.Playing)
        {
            if (environmentCoroutine == null)
            {
                environmentCoroutine = StartCoroutine(envitonment_Playing());
            }
        }

        


    }

    IEnumerator Fade_Stop_Environment()
    {
        float backup_volume = Audio_envitonment.volume;

        for (float i = backup_volume; i >= 0; i -= 0.05f)
        {
            Audio_envitonment.volume = i;
            yield return null;
        }

        Audio_envitonment.Stop();
        Audio_envitonment.volume = backup_volume;

    }

    IEnumerator envitonment_Playing()
    {

        yield return new WaitForSeconds(3f);

        int bgm_sequence = 1;

        while (true)
        {
            // �����ֲ������ && û����ͣ ���л�����
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

            //ֻ��û���ƻ�������²�ִ��
            if (!isbroking)
            {
                //����ƻ�
                if (Input.GetMouseButton(0))
                {
                    //�������
                    if (player.broke_Block_type != VoxelData.notHit)
                    {
                        isbroking = true;
                        update_Clips();
                        Audio_player_broke.Play();
                    }
                }



            }



            //�ɿ����
            if (Input.GetMouseButtonUp(0) || player.isChangeBlock)
            {
                isbroking = false;
                Audio_player_broke.Stop();

            }

        }
    }

    public void PlaySoung_Place()
    {
        Audio_player_place.PlayOneShot(audioclips[VoxelData.place_normal]);
    }

    //typeӳ�䵽clips
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
            //������FixedUpdate()

            // �������ƶ�������Ƶδ���ţ���������ڵ�����,�򲥷���Ƶ
            if (player.isMoving && !Audio_player_moving.isPlaying && player.isGrounded)
            {
                if (Time.time >= nextFootstepTime)
                {
                    Audio_player_moving.PlayOneShot(Audio_player_moving.clip);
                    nextFootstepTime = Time.time + footstepInterval;
                }
               
            }


            // �����Ƶ���ڲ��ţ�������Ҳ��ڵ������Ҳ�����Ӿ�У�����ͣ��Ƶ
            //else if (Audio_player_moving.isPlaying && !player.isGrounded && !player.isSwiming)
            //{
            //    Audio_player_moving.Pause();
            //}
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


            //flop to Water
            ifmovingStateSwitch();

            //flop to ground��player�б�����

            //diving
            playsound_diving();
        }
    }


    //����
    void UpdateMovingClip()
    {

        //������·��鷢���仯������Ĳ����б�
        //�ݵ�
        if (player.foot_BlockType == VoxelData.Grass)
        {
            Audio_player_moving.clip = audioclips[VoxelData.moving_grass];
        }
        //ɳ��
        else if (player.foot_BlockType == VoxelData.Sand)
        {
            Audio_player_moving.clip = audioclips[VoxelData.moving_sand];
        }
        //ʯͷ
        else if (player.foot_BlockType == VoxelData.Stone)
        {
            Audio_player_moving.clip = audioclips[VoxelData.moving_stone];
        }
        //ˮ
        else if (player.foot_BlockType == VoxelData.Water)
        {
            Audio_player_moving.clip = audioclips[VoxelData.moving_sand];
        }
        //����
        else if (player.foot_BlockType == VoxelData.Air)
        {
            Audio_player_moving.clip = null;
        }
        //����
        else
        {
            Audio_player_moving.clip = audioclips[VoxelData.moving_else];
        }

        


    }


    //���վ��ˮ������ˮ����Ի�����ˮ������-20��ˮ
    void Fade_FallInto_Water()
    {
        //���isground��isswimmingͬʱ������ôֻ��ִ��swimming
        if (player.isGrounded && player.isSwiming)
        {
            //swiming
            if (player.isSwiming)
            {
                if (hasExec_isGround)
                {
                    //��������ظ߶�Ϊ0����ô�ͱ�0 
                    if (player.new_foot_high - player.transform.position.y <= 0f)
                    {
                        player.verticalMomentum = 0f;
                    }
                    //Debug.Log("�л���ˮ��");
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
                    //Debug.Log("�л�������");
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
                    //��������ظ߶�Ϊ0����ô�ͱ�0 
                    if (player.new_foot_high - player.transform.position.y <= 0f)
                    {
                        player.verticalMomentum = 0f;
                    }
                    //Debug.Log("�л���ˮ��");
                    hasExec_isSwiming = true;
                    //Audio_player_moving.clip = audioclips[VoxelData.moving_water];
                    hasExec_isGround = false;
                }

            }

        }
    }

    //������type����ΪAir��water
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

    //������״̬�����л���ʱ�򻻳���ˮ��Ч
    void ifmovingStateSwitch()
    {
        leg_blocktype = classifytype();

        if (leg_blocktype != previous_leg_blocktype)
        {
            Audio_player_falling.PlayOneShot(audioclips[VoxelData.fall_water]);
            previous_leg_blocktype = leg_blocktype;
        }

    }

    //����ˤ����Ч
    public void PlaySound_fallGround()
    {
        Audio_player_falling.PlayOneShot(audioclips[VoxelData.fall_high]);
    }

    //����Ǳˮ��Ч
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






    //------------------------------------ ������ -------------------------------------------



    //---------------------------------------------------------------------------------------




}
