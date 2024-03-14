using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    [Header("Transforms")]
    //��������
    public World world;
    public Transform Camera;
    public MusicManager musicmanager;
    public GameObject MainCamera;
    public GameObject PlayerObject;
    public Player player;

    //��Ҫ��Ļ
    public GameObject Start_Screen;
    public GameObject Init_Screen;
    public GameObject Loading_Screen;

    //���ɶ���
    public Slider slider;
    public TextMeshProUGUI tmp;
    public GameObject handle;

    //Playing��Ļ����
    public GameObject OpenYourEyes;
    public GameObject Debug_Screen;
    public GameObject ToolBar;
    public GameObject CursorCross_Screen;
    public GameObject Swimming_Screen;
    public GameObject Pause_Screen;
    public GameObject HowToPlay_Screen;
    public GameObject Prompt_Screen;
    public GameObject prompt;
    public TextMeshProUGUI prompt_Text;

    //�޸�ֵ����
    public Slider slider_bgm;
    public Slider slider_sound;
    public Slider slider_render_speed;
    public Toggle toggle_SpaceMode;  
    public Toggle toggle_SuperMing;


    //��Ϸ״̬�ж�
    [HideInInspector]
    public bool isPausing = false;


    //�޸�ֵ
    [Header("Options")]
    //bgm
    public float volume_bgm = 0.5f;
    private float previous_bgm = 0.5f;

    //sound
    public float volume_sound = 0.5f;
    private float previous_sound = 0.5f;
    
    //render speed
    public float render_speed = 0.5f;
    private float previous_render_speed = 0.5f;

    //isSpaceMode
    public bool SpaceMode_isOn = false;
    private bool previous_spaceMode_isOn = false;

    //isSpaceMode
    public bool SuperMining_isOn = false;
    private bool previous_SuperMining_isOn = false;

    //pormpt
    public float promptShowspeed = 400f;

    //eyestime
    public float eyesOpenTime = 5f;

    //һ���Դ���
    bool hasExec_Playing = true;
    public bool hasExec_PromptScreen_isShow = false;
    bool hasExec_PromptScreen_isHide = true;


    //----------------------------------- �������� ---------------------------------------

    private void FixedUpdate()
    {
        
        //������
        if (world.game_state == Game_State.Loading)
        {
            //Slider
            slider.value = world.initprogress;

            //Text
            tmp.text = $"{(world.initprogress * 100):F2} %";

            //ScrollBar
            float x = Mathf.Lerp(1f, 5f, world.initprogress);
            float y = Mathf.Lerp(1f, 5f, world.initprogress);
            handle.transform.localScale = new Vector3(x, y, 1f);

            //�������������
            if (world.initprogress == 1)
            {
                world.game_state = Game_State.Playing;
            }



        }

        //�������
        if (world.game_state == Game_State.Playing)
        {

            if (hasExec_Playing)
            {
                Loading_Screen.SetActive(false);
                ToolBar.SetActive(true);
                CursorCross_Screen.SetActive(true);
                Prompt_Screen.SetActive(true);

                hasExec_Playing = false;

                openyoureyes();
            }


            //EscScreen
            Show_Esc_Screen();



            //Debug���
            if (Input.GetKeyDown(KeyCode.F3))
            {
                Debug_Screen.SetActive(!Debug_Screen.activeSelf);
            }


            //SwimmingScreen
            if (world.GetBlockType(Camera.transform.position) == VoxelData.Water && !isPausing)
            {
                Swimming_Screen.SetActive(true);
            }
            else
            {
                Swimming_Screen.SetActive(false);
            }
            
        }

        UpdatePauseScreenValue();

        Prompt_FlashLight();
    }


    //----------------------------------------------------------------------------------------








    //----------------------------------- Start_Screen ---------------------------------------
    //Start -> Init
    public void ClickToInit()
    {
        //screen�л�
        Start_Screen.SetActive(false);
        Init_Screen.SetActive(true);

        //music
        musicmanager.PlaySound_Click();
    }

    //----------------------------------------------------------------------------------------






    //----------------------------------- Init_Screen ---------------------------------------
    //Init -> Loading
    public void ClickToLoading()
    {
        //���ڴ�
        MainCamera.SetActive(false);

        //screen�л�
        Init_Screen.SetActive(false);
        Loading_Screen.SetActive(true);

        //��Ϸ״̬�л�
        world.game_state = Game_State.Loading;

        //��������л�
        PlayerObject.SetActive(true);
        MainCamera.SetActive(false);

        //����
        HideCursor();

        //music
        musicmanager.PlaySound_Click();

    }

    //Init -> Start
    public void BackToStart()
    {
        //screen�л�
        Init_Screen.SetActive(false);
        Start_Screen.SetActive(true);

        //music
        musicmanager.PlaySound_Click();
    }
    //---------------------------------------------------------------------------------------






    //--------------------------------- Loading_Screen -------------------------------------

    //Loading������ʱ���һ�����۵Ķ���
    void openyoureyes()
    {
        OpenYourEyes.SetActive(true);
        StartCoroutine(Animation_Openyoureyes());
    }

    IEnumerator Animation_Openyoureyes()
    {
        Image image = OpenYourEyes.GetComponent<Image>();

        // ��¼���俪ʼ��ʱ��
        float startTime = Time.time;

        while (Time.time - startTime < eyesOpenTime)
        {
            // ���㵱ǰ��͸����
            float alpha = Mathf.Lerp(1f, 0f, (Time.time - startTime) / eyesOpenTime);

            // ����Image�����͸����
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);

            // �ȴ�һ֡
            yield return null;
        }

        // ��������󣬽�͸��������Ϊ��ȫ͸��
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
    }

    //--------------------------------------------------------------------------------------






    //--------------------------------- Playing_Screen -------------------------------------
    //Ese_Screen
    void Show_Esc_Screen()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isPausing)
        {
            isPausing = !isPausing;
            Pause_Screen.SetActive(!Pause_Screen.activeSelf);

            //music
            musicmanager.PlaySound_Click();

            Cursor.lockState = CursorLockMode.None;
            //����
            Cursor.visible = true;

            world.game_state = Game_State.Pause;
            ToolBar.SetActive(false);
            CursorCross_Screen.SetActive(false);
        }
    }

    //back to menu
    public void Click_EscMenu()
    {
        isPausing = !isPausing;
        Pause_Screen.SetActive(!Pause_Screen.activeSelf);

        //music
        musicmanager.PlaySound_Click();


        // �������������Ļ����
        Cursor.lockState = CursorLockMode.Locked;

        //��겻����
        Cursor.visible = false;

        world.game_state = Game_State.Playing;
        ToolBar.SetActive(true);
        CursorCross_Screen.SetActive(true);
    }


    //�򿪲���ָ��
    public void Click_HowToPlay()
    {
        //music
        musicmanager.PlaySound_Click();

        StartCoroutine(Help_Animation_Open());

    }

    //Help����
    IEnumerator Help_Animation_Open()
    {
        HowToPlay_Screen.SetActive(true);
        yield return null;

        for (float y = 0; y <= 1; y += 0.1f)
        {
            HowToPlay_Screen.transform.localScale = new Vector3(1f, y, 1f);
            yield return null;
        }

        HowToPlay_Screen.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    IEnumerator Help_Animation_Close()
    {
        for (float y = 1; y >= 0; y -= 0.1f)
        {
            HowToPlay_Screen.transform.localScale = new Vector3(1f, y, 1f);
            yield return null;
        }

        HowToPlay_Screen.SetActive(false);

    }



    //Help_Close
    public void Back_Pause_Screen()
    {
        StartCoroutine(Help_Animation_Close());

        //music
        musicmanager.PlaySound_Click();
    }


    //�ֵ�Ͳ����ʾ
    void Prompt_FlashLight()
    { 
        if (player.transform.position.y <= world.soil_min + 5f)
        {
            if (hasExec_PromptScreen_isShow == false)
            {
                First_Prompt_PlayerThe_Flashlight();
            }
            
        }

        if (Input.GetKeyDown(KeyCode.F) && hasExec_PromptScreen_isShow)
        {
            if (hasExec_PromptScreen_isHide)
            {
                StartCoroutine(Hide_Animation_PromptScreen());

                hasExec_PromptScreen_isHide = false;
            }
        }

    }

    public void First_Prompt_PlayerThe_Flashlight()
    {
        prompt_Text.text = "You can press <F> \r\nto open \"FlashLight\"";
        StartCoroutine(Show_Animation_PromptScreen());
        hasExec_PromptScreen_isShow = true;
    }

    IEnumerator Show_Animation_PromptScreen()
    {
        //����ԭ��ֵ
        Vector2 oldPosition = prompt.GetComponent<RectTransform>().anchoredPosition;
        float sx = oldPosition.x;

        //-344~344����������
        while (sx <= 344f)
        {
            sx += Time.deltaTime * promptShowspeed;
            prompt.GetComponent<RectTransform>().anchoredPosition = new Vector2(sx, oldPosition.y);
            yield return null;
        }

        //����λ��
        prompt.GetComponent<RectTransform>().anchoredPosition = new Vector2(344f, oldPosition.y);
        yield return null;
    }

    IEnumerator Hide_Animation_PromptScreen()
    {
        //����ԭ��ֵ
        Vector2 oldPosition = prompt.GetComponent<RectTransform>().anchoredPosition;
        float sx = oldPosition.x;

        //-344~344����������
        while (sx >= -344f)
        {
            sx -= Time.deltaTime * promptShowspeed;
            prompt.GetComponent<RectTransform>().anchoredPosition = new Vector2(sx, oldPosition.y);
            yield return null;
        }

        //����λ��
        prompt.GetComponent<RectTransform>().anchoredPosition = new Vector2(-344f, oldPosition.y);
        prompt_Text.text = "";
        yield return null;
    }


    //--------------------------------------------------------------------------------------






    //---------------------------------- ʵʱ�޸�ֵ ------------------------------------------

    //����ֵ
    void UpdatePauseScreenValue()
    {
        //bgm volume
        volume_bgm = slider_bgm.value;
        if (volume_bgm != previous_bgm)
        {
            //bgm
            musicmanager.Audio_envitonment.volume = Mathf.Lerp(0f, 1f, volume_bgm);

            //����previous
            previous_bgm = volume_bgm;
        }


        //sound volume
        volume_sound = slider_sound.value;
        if (volume_sound != previous_sound)
        {
            //sound
            musicmanager.Audio_player_place.volume = Mathf.Lerp(0f, 1f, volume_sound);
            musicmanager.Audio_player_broke.volume = Mathf.Lerp(0f, 1f, volume_sound);
            musicmanager.Audio_player_moving.volume = Mathf.Lerp(0f, 1f, volume_sound);
            musicmanager.Audio_player_falling.volume = Mathf.Lerp(0f, 1f, volume_sound);
            musicmanager.Audio_player_diving.volume = Mathf.Lerp(0f, 1f, volume_sound);
            musicmanager.Audio_Click.volume = Mathf.Lerp(0f, 1f, volume_sound);

            //����previous
            previous_sound = volume_sound;
        }

        //render speed
        render_speed = slider_render_speed.value;

        if (render_speed > 0.9f)
        {
            render_speed = 0.9f;
        }

        if (render_speed != previous_render_speed)
        {
            //delay = 1 - speed
            world.CreateCoroutineDelay = 1 - render_speed; 
            world.RemoveCoroutineDelay = 1 - render_speed;

            //����previous
            previous_render_speed = render_speed;
        }


        //space mode
        SpaceMode_isOn = toggle_SpaceMode.isOn;
        if (SpaceMode_isOn != previous_spaceMode_isOn)
        {
            if (SpaceMode_isOn)
            {
                player.gravity = -3f;
                player.isSpaceMode = true;
            }
            else
            {
                player.gravity = -20f;
                player.isSpaceMode = false;
            }

            //����previous
            previous_spaceMode_isOn = SpaceMode_isOn;
        }


        //SuperMining
        SuperMining_isOn = toggle_SuperMing.isOn;
        if (SuperMining_isOn != previous_SuperMining_isOn)
        {
            if (SuperMining_isOn)
            {
                player.isSuperMining = true;
            }
            else
            {
                player.isSuperMining = false;
            }

            //����previous
            previous_SuperMining_isOn = SuperMining_isOn;
        }

    }


    //---------------------------------------------------------------------------------------






    //------------------------------------ ������ -------------------------------------------
    void HideCursor()
    {
        // �������������Ļ����
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        //��겻����
        UnityEngine.Cursor.visible = false;
    }

    public void QuitGame()
    {
        //music
        musicmanager.PlaySound_Click();

        Application.Quit();
    }

    //--------------------------------------------------------------------------------------




}
