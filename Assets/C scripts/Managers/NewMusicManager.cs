using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewMusicManager : MonoBehaviour
{
    #region 状态


    #endregion


    #region 周期函数

    ManagerHub managerhub;
    public AudioClip[] audioclips;


    private void Start()
    {
        managerhub = GlobalData.GetManagerhub();
        _ReferStartBackGroundMusic();
    }

    private void Update()
    {
        _ReferUpdateBackGroundMusic();
    }

    #endregion


    #region 背景音乐

    //背景音乐
    AudioSource Audio_envitonment;

    //协程
    private Coroutine fadetoStopenvironment;
    private Coroutine environmentCoroutine;

    //创建一个AudioSource用于BackGround
    void _ReferStartBackGroundMusic()
    {
        GameObject MusicObject = new GameObject("MusicObject");
        MusicObject.transform.SetParent(transform);
        Audio_envitonment = MusicObject.AddComponent<AudioSource>();
    }

    //背景音乐一直占用0号位置
    void _ReferUpdateBackGroundMusic()
    {
        if (managerhub.world.game_state == Game_State.Loading)
        {
            if (fadetoStopenvironment == null)
            {
                fadetoStopenvironment = StartCoroutine(Fade_Stop_Environment());
            }


        }
        else if (managerhub.world.game_state == Game_State.Playing)
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
            // 当音乐播放完毕 切换音乐
            if (!Audio_envitonment.isPlaying)
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


    #endregion


    #region [public] = OneShot + 3d音频

    /// <summary>
    /// 播放音频
    /// </summary>
    /// <param name="_pos"></param>
    /// <param name="_id"></param>
    public void PlaySound(int _id)
    {
        //OneShot
    }


    /// <summary>
    /// 在所给地点创建3d音频
    /// </summary>
    /// <param name="_pos"></param>
    /// <param name="_id"></param>
    public void PlaySound(Vector3 _pos, int _id)
    {

    }

    #endregion


}
