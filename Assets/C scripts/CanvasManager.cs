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
    public ParticleSystem partSystem;
    public World world;
    public Transform Camera;
    public MusicManager musicmanager;
    public GameObject MainCamera;
    public GameObject PlayerObject;
    public Player player;
    public TextMeshProUGUI selectblockname;
    public BackPackManager BackPackManager;
    public LifeManager LifeManager;
    public TextMeshProUGUI gamemodeTEXT;
    public GameObject CreativeButtom;
    public GameObject SurvivalButtom;
    public GameObject Survival_Screen;

    //��Ҫ��Ļ
    public bool isInTheInitScreen;  //�����˿�ʼ��Ϸ��������Init����
    public GameObject Start_Screen;
    public GameObject Init_Screen;
    public GameObject Loading_Screen;

    //���ɶ���
    public Slider slider;
    public TextMeshProUGUI tmp;
    public GameObject handle;

    //InitScreen
    public TextMeshProUGUI ErrorMessage;
    public Toggle SuperPlainToggle;

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
    public GameObject DeadScreen;

    //�޸�ֵ����
    public Slider slider_bgm;
    public Slider slider_sound;
    public Slider slider_MouseSensitivity;
    public Toggle toggle_SpaceMode;  
    public Toggle toggle_SuperMing;


    //��Ϸ״̬�ж�
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
    public float Mouse_Sensitivity = 1f;
    private float previous_Mouse_Sensitivity = 1f;

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

    //Jump_MuaCherish
    public GameObject muacherish;
    public float speed = 1.0f; // ���Ƹ����ٶȵĲ���
    public float magnitude = 0.04f; // ���Ƹ������ȵĲ���
    public float colorSpeed = 1f; //������ɫ�ٶ�
    Coroutine muacherishCoroutine;

    //ShowBlockName
    Coroutine showblocknameCoroutine;

    //Score
    public TextMeshProUGUI scoreText;
    float startTime;
    float endTime;

    //һ���Դ���
    bool hasExec_Playing = true;
    public bool hasExec_PromptScreen_isShow = false;
    bool hasExec_PromptScreen_isHide = true;
    bool hasExec_InWater = false;

    //debug


    //----------------------------------- �������� ---------------------------------------

    private void Start()
    {
        if (muacherishCoroutine == null)
        {
            muacherishCoroutine = StartCoroutine(jumpMuaCherish());
        }

        
    }

    private void Update() 
    {

        //��ʼ��
        if (world.game_state == Game_State.Start && isInTheInitScreen)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ClickToLoading();
            }
        }



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
            //Survival
            if (world.game_mode == GameMode.Survival)
            {
                GameMode_Survival();
            }
            //Creative
            else if (world.game_mode == GameMode.Creative)
            {
                GameMode_Creative();
            }

            
            
        }
        if (Pause_Screen.activeSelf)
        {
            UpdatePauseScreenValue();
        }
        

        Prompt_FlashLight();
    }


    //----------------------------------------------------------------------------------------







    //------------------------------------- GameMode -----------------------------------------
    //Survival
    void GameMode_Survival()
    {
        if (hasExec_Playing)
        {
            Loading_Screen.SetActive(false);
            ToolBar.SetActive(true);
            CursorCross_Screen.SetActive(true);
            Prompt_Screen.SetActive(true);

            openyoureyes();

            StopCoroutine(muacherishCoroutine);

            //��¼��ʼʱ��
            startTime = Time.time;

            hasExec_Playing = false;
        }


        //EscScreen
        Show_Esc_Screen();



        //Debug���
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug_Screen.SetActive(!Debug_Screen.activeSelf);
        }


        //SwimmingScreen
        if (world.GetBlockType(Camera.transform.position + new Vector3(0f, 0.2f, 0f)) == VoxelData.Water && !isPausing)
        {
            //��ˮ
            if (hasExec_InWater == false)
            {
                //data
                //Debug.Log("IntoWater");
                
                Swimming_Screen.SetActive(true);

                partSystem.Play();

                LifeManager.Oxy_IntoWater();

                //update
                hasExec_InWater = true;
            }

        }
        else
        {
            Swimming_Screen.SetActive(false);

            partSystem.Stop();


            //��ˮ
            if (hasExec_InWater == true)
            {

                //data
                //Debug.Log("OutWater");
                LifeManager.Oxy_OutWater();


                //update
                hasExec_InWater = false;
            }

        }
    }

    //Creative
    void GameMode_Creative()
    {
        if (hasExec_Playing)
        {
            Loading_Screen.SetActive(false);
            ToolBar.SetActive(true);
            Survival_Screen.SetActive(false);
            CursorCross_Screen.SetActive(true);
            Prompt_Screen.SetActive(true);

            toggle_SuperMing.isOn = true;
            player.isSuperMining = true;
            openyoureyes();

            BackPackManager.CreativeMode();

            StopCoroutine(muacherishCoroutine);

            hasExec_Playing = false;
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
            //��ˮ
            Swimming_Screen.SetActive(true);
            partSystem.Play();
        }
        else
        {
            Swimming_Screen.SetActive(false);
            partSystem.Stop();

        }
    }

    //----------------------------------------------------------------------------------------









    //----------------------------------- Start_Screen ---------------------------------------
    //Start -> Init
    public void ClickToInit()
    {
        //screen�л�
        Start_Screen.SetActive(false);
        Init_Screen.SetActive(true);
        isInTheInitScreen = true;

        //music
        musicmanager.PlaySound_Click();
    }

    IEnumerator jumpMuaCherish()
    {
        float offset = 0.0f;

        while (true)
        {
            // ��������
            float scaleX = 1.0f + Mathf.PingPong(offset * speed, magnitude) * 0.5f; // ����x�������
            float scaleY = 1.0f + Mathf.PingPong(offset * speed, magnitude) * 0.5f; // ����y�������
            muacherish.transform.localScale = new Vector3(scaleX, scaleY, muacherish.transform.localScale.z);

            // ������ɫ����
            Color color = new Color(
                Mathf.Sin(offset * colorSpeed) * 0.5f + 0.5f, // ��
                Mathf.Sin(offset * colorSpeed + Mathf.PI * 2 / 3) * 0.5f + 0.5f, // ��
                Mathf.Sin(offset * colorSpeed + Mathf.PI * 4 / 3) * 0.5f + 0.5f, // ��
                1f // ��͸����
            );
            muacherish.GetComponent<TextMeshProUGUI>().color = color;

            offset += Time.deltaTime;

            yield return null;
        }
    }


    //----------------------------------------------------------------------------------------






    //----------------------------------- Init_Screen ---------------------------------------
    //Init -> Loading
    public void ClickToLoading()
    {

        //����Ƿ���������
        world.CheckSeed();

        //����Ƿ�����RenderSize
        world.CheckRenderSize();

        if (world.InitError == false)
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
        else
        {

            StartCoroutine(ShowErrorMessage());

            world.InitError = false;
        }

        

    }

    IEnumerator ShowErrorMessage()
    {
        ErrorMessage.text = "illegal input !!";

        yield return new WaitForSeconds(2f);

        ErrorMessage.text = "";
    }




    //Init -> Start
    public void BackToStart()
    {
        //screen�л�
        Init_Screen.SetActive(false);
        Start_Screen.SetActive(true);
        isInTheInitScreen = false;
        //music
        musicmanager.PlaySound_Click();
    }

    //ѡ������ģʽ
    public void GamemodeToSurvival()
    {
        world.game_mode = GameMode.Survival;
        gamemodeTEXT.text = "��ǰ��Ϸģʽ������ģʽ";

        //�ı䰴ť��ɫ
        SurvivalButtom.GetComponent<Image>().color = new Color(106 / 255f, 115 / 255f, 200 / 255f, 1f);
        CreativeButtom.GetComponent<Image>().color = new Color(149 / 255f, 134 / 255f, 119 / 255f, 1f);
    }

    //ѡ����ģʽ
    public void GamemodeToCreative()
    {
        world.game_mode = GameMode.Creative;
        gamemodeTEXT.text = "��ǰ��Ϸģʽ������ģʽ";

        //�ı䰴ť��ɫ
        SurvivalButtom.GetComponent<Image>().color = new Color(149 / 255f, 134 / 255f, 119 / 255f, 1f);
        CreativeButtom.GetComponent<Image>().color = new Color(106 / 255f, 115 / 255f, 200 / 255f, 1f);
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

        //ֱ�Ӹ�����
        OpenYourEyes.SetActive(false);
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
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
        if (player.transform.position.y <= 50f + 5f)
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

        //MouseSensitivity
        Mouse_Sensitivity = Mathf.Lerp(1f, 4f, slider_MouseSensitivity.value);

        if (Mouse_Sensitivity != previous_Mouse_Sensitivity)
        {
            //�ı����������

            //����previous
            previous_Mouse_Sensitivity = Mouse_Sensitivity;
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





    //---------------------------------- ���������� -----------------------------------------
    public void PlayerDead()
    {
        DeadScreen.SetActive(true);

        //Score
        endTime = Time.time;
        scoreText.text = $"������{(int)(endTime - startTime)}";

        //������
        Cursor.lockState = CursorLockMode.None;
        //������
        Cursor.visible = true;

        world.game_state = Game_State.Dead;
    }

    public void PlayerClickToRestart()
    {
        DeadScreen.SetActive(false);

        // �������������Ļ����
        Cursor.lockState = CursorLockMode.Locked;
        //��겻����
        Cursor.visible = false;

        world.game_state = Game_State.Playing;

        LifeManager.blood = 20; 
        LifeManager.oxygen = 10;
        LifeManager.UpdatePlayerBlood(0, false, false);
        startTime = Time.time;

        player.InitPlayerLocation();
        player.transform.rotation = Quaternion.identity;

        world.Update_CenterChunks();

        world.HideFarChunks();

        openyoureyes();
    }
    //--------------------------------------------------------------------------------------






    //------------------------------------ ������ -------------------------------------------
    void HideCursor()
    {
        // �������������Ļ����
        Cursor.lockState = CursorLockMode.Locked;
        //��겻����
        Cursor.visible = false;
    }

    public void QuitGame()
    {
        //music
        musicmanager.PlaySound_Click();

        Application.Quit();
    }

    //��ʾBlock����
    public void Change_text_selectBlockname(byte prokeblocktype)
    {
        //255�������л�����
        //��255�������ƻ����飬ֱ�Ӳ����ƻ���������־���
        if (prokeblocktype == 255)
        {
            //����з���Ļ�
            if (BackPackManager.istheindexHaveBlock(player.selectindex))
            {
                //�����Э����ִ�У���������ֹ
                if (showblocknameCoroutine != null)
                {
                    StopCoroutine(showblocknameCoroutine);
                }

                showblocknameCoroutine = StartCoroutine(showblockname(prokeblocktype));
            }
        }
        else
        {
            //�����Э����ִ�У���������ֹ
            if (showblocknameCoroutine != null)
            {
                StopCoroutine(showblocknameCoroutine);
            }

            showblocknameCoroutine = StartCoroutine(showblockname(prokeblocktype));
        }

        
    }

    IEnumerator showblockname(byte _blocktype)
    {

        if (_blocktype == 255)
        {
            //��ʾ������������2s
            selectblockname.text = world.blocktypes[BackPackManager.slots[player.selectindex].blockId].blockName;

            yield return new WaitForSeconds(2f);

            selectblockname.text = "";

            showblocknameCoroutine = null;
        }
        else
        {
            //��ʾ������������2s
            selectblockname.text = world.blocktypes[_blocktype].blockName;

            yield return new WaitForSeconds(2f);

            selectblockname.text = "";

            showblocknameCoroutine = null;
        }
        
    }

    //--------------------------------------------------------------------------------------




}
