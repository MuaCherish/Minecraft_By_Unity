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
    public BackPackManager backPackManager;

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
    [ReadOnly]public bool isbroking = false;

    //moving
    [Header("���/leg/foot״̬")]
    [HideInInspector] public bool hasExec_isGround = true;
    [HideInInspector] public bool hasExec_isSwiming = true;
    [HideInInspector] public byte leg_blocktype;
    [HideInInspector] public byte previous_leg_blocktype = VoxelData.Air;


    //walk
    int item = 0;   //�����������ҽŵ�
    [HideInInspector] public byte footBlocktype = VoxelData.Grass;
    [HideInInspector] public byte previous_foot_blocktype = VoxelData.Grass;
    [HideInInspector] public float footstepInterval; // ��·��Ч���ż��
    private float nextFoot; 


    //---------------------------------- ���ں��� ----------------------------------------

    private void Start()
    {
        InitMusicManager();
    }

    public void InitMusicManager()
    {
        //environment
        if(Audio_envitonment == null)
        {
            Audio_envitonment = gameObject.AddComponent<AudioSource>();
        }
        
        Audio_envitonment.clip = audioclips[MusicData.bgm_menu];
        Audio_envitonment.volume = 0.4f;
        Audio_envitonment.loop = false;
        Audio_envitonment.Play();

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
        fadetoStopenvironment = null;
        environmentCoroutine = null;
    }

    private void Update()
    {
        if (world.game_state == Game_State.Playing)
        {
            //�Ų���Ч
            PlaySound_Foot();
        }
    }

    private void FixedUpdate()
    {
        Fun_environment();

        FUN_PlaceandBroke();

        FUN_Moving();


        Fade_FallInto_Water();

        if (world.game_state == Game_State.Playing)
        {
            UpdateFootBlockType();
        }

        
    }


    //---------------------------------------------------------------------------------------






    //---------------------------------- envitonment ----------------------------------------
    public void PlaySound_Click()
    {
        Audio_Click.PlayOneShot(audioclips[MusicData.click]);
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

        for (float i = backup_volume; i >= 0; i -= 0.01f)
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
                    Audio_envitonment.clip = audioclips[MusicData.bgm_3];
                    Audio_envitonment.volume = 0.5f;
                    Audio_envitonment.Play();
                    bgm_sequence = 2;
                }
                else if (bgm_sequence == 2)
                {
                    Audio_envitonment.clip = audioclips[MusicData.bgm_2];
                    Audio_envitonment.volume = 0.5f;
                    Audio_envitonment.Play();
                    bgm_sequence = 3;
                }
                else if (bgm_sequence == 3)
                {
                    Audio_envitonment.clip = audioclips[MusicData.bgm_1];
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
                yield return new WaitForSeconds(100f);
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
                    if (player.point_Block_type != PlayerData.notHit)
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
                    else{
                        Audio_player_broke.Stop();
                    }
                }

            }


        }
    }

    public void PlaySound_Broken(byte pointblock)
    {
        if (world.blocktypes[pointblock].broken_clip != null)
        {
            
            Audio_player_place.PlayOneShot(world.blocktypes[pointblock].broken_clip);
        }
        else
        {
            Audio_player_place.PlayOneShot(world.blocktypes[VoxelData.Stone].broken_clip);
        }
    }

    public void PlaySoung_Place()
    {
        if (backPackManager.istheindexHaveBlock(player.selectindex))
        {
            Audio_player_place.PlayOneShot(world.blocktypes[backPackManager.slots[player.selectindex].blockId].broken_clip);
        }
        
    }

    //---------------------------------------------------------------------------------------
    





    //------------------------------------ moving -------------------------------------------
    void FUN_Moving()
    {
        if (world.game_state == Game_State.Playing)
        {
            //��Ӿ��Ч
            PlaySound_Swiming();

            //����ˮ��Ч
            ifmovingStateSwitch();

            //flop to ground��player�б�����

            //Ǳˮ��Ч
            playsound_diving();
        }
    }

    

    //�Ų���
    void PlaySound_Foot()
    {
        // �������ƶ�������Ƶδ���ţ���������ڵ����ϣ��򲥷���Ƶ
        if (player.isMoving && player.isGrounded && !player.isSquating)
        {
            if (Time.time >= nextFoot)
            {
                //������ǿ����򲥷�
                if (footBlocktype != VoxelData.Air)
                {
                    //�����ר����Ч
                    if (world.blocktypes[footBlocktype].walk_clips[item] != null)
                    {
                        if (item == 0)
                        {
                            Audio_player_moving.PlayOneShot(world.blocktypes[footBlocktype].walk_clips[item]);
                            item = 1;
                        }
                        else
                        {
                            Audio_player_moving.PlayOneShot(world.blocktypes[footBlocktype].walk_clips[item]);
                            item = 0;
                        }
                    }
                    //���û��ר����·��Ч����Ĭ�ϲ���ʯͷ��Ч
                    else
                    {
                        if (item == 0)
                        {
                            Audio_player_moving.PlayOneShot(world.blocktypes[VoxelData.Stone].walk_clips[item]);
                            item = 1;
                        }
                        else
                        {
                            Audio_player_moving.PlayOneShot(world.blocktypes[VoxelData.Stone].walk_clips[item]);
                            item = 0;
                        }
                    }
                }
                
                

                nextFoot = Time.time + footstepInterval;
            }
        }

    }

    //��Ӿ��Ч
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
            Audio_player_falling.PlayOneShot(audioclips[MusicData.fall_water]);
            previous_leg_blocktype = leg_blocktype;
        }

    }

    //����ˤ����Ч
    public void PlaySound_fallGround()
    {
        Audio_player_falling.PlayOneShot(audioclips[MusicData.fall_high]);
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

    //�����·����л���ʱ�򴥷�
    void UpdateFootBlockType()
    {
        footBlocktype = world.GetBlockType(foot.position); 
         
        if (footBlocktype != previous_foot_blocktype)
        {
            previous_foot_blocktype = footBlocktype;
        }
    }

    //����������Ч
    public void PlaySound_Absorb()
    {
        int i = UnityEngine.Random.Range(0, 2);

        if (i == 0)
        {
            Audio_Click.PlayOneShot(audioclips[MusicData.absorb_1]);
        }
        else
        {
            Audio_Click.PlayOneShot(audioclips[MusicData.absorb_2]);
        }
    }

    public void PlaySound(int _clip)
    {
        Audio_Click.PlayOneShot(audioclips[_clip]);
    }

    //---------------------------------------------------------------------------------------




}
