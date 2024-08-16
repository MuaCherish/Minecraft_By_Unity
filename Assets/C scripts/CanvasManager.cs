using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    [Header("Transforms")]
    //场景对象
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

    //主要屏幕
    public bool isInTheInitScreen;  //按下了开始游戏，进入了Init界面
    public GameObject Start_Screen;
    public GameObject Init_Screen;
    public GameObject Loading_Screen;

    //过渡对象
    public Slider slider;
    public TextMeshProUGUI tmp;
    public GameObject handle;

    //InitScreen
    public TextMeshProUGUI ErrorMessage;
    public Toggle SuperPlainToggle;

    //Playing屏幕内容
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

    //修改值参数
    public Slider slider_bgm;
    public Slider slider_sound;
    public Slider slider_MouseSensitivity;
    public Toggle toggle_SpaceMode;  
    public Toggle toggle_SuperMing;


    //游戏状态判断
    public bool isPausing = false;


    //修改值
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
    public float speed = 1.0f; // 控制浮动速度的参数
    public float magnitude = 0.04f; // 控制浮动幅度的参数
    public float colorSpeed = 1f; //渐变颜色速度
    Coroutine muacherishCoroutine;

    //ShowBlockName
    Coroutine showblocknameCoroutine;

    //Score
    public TextMeshProUGUI scoreText;
    float startTime;
    float endTime;

    //一次性代码
    bool hasExec_Playing = true;
    public bool hasExec_PromptScreen_isShow = false;
    bool hasExec_PromptScreen_isHide = true;
    bool hasExec_InWater = false;

    //debug


    //----------------------------------- 生命周期 ---------------------------------------

    private void Start()
    {
        if (muacherishCoroutine == null)
        {
            muacherishCoroutine = StartCoroutine(jumpMuaCherish());
        }

        
    }

    private void Update() 
    {

        //初始化
        if (world.game_state == Game_State.Start && isInTheInitScreen)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ClickToLoading();
            }
        }



        //加载中
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

            //如果进度条满了
            if (world.initprogress == 1)
            {
                world.game_state = Game_State.Playing;
            }



        }

        //加载完成
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

            //记录开始时间
            startTime = Time.time;

            hasExec_Playing = false;
        }


        //EscScreen
        Show_Esc_Screen();



        //Debug面板
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug_Screen.SetActive(!Debug_Screen.activeSelf);
        }


        //SwimmingScreen
        if (world.GetBlockType(Camera.transform.position + new Vector3(0f, 0.2f, 0f)) == VoxelData.Water && !isPausing)
        {
            //入水
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


            //出水
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



        //Debug面板
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug_Screen.SetActive(!Debug_Screen.activeSelf);
        }


        //SwimmingScreen
        if (world.GetBlockType(Camera.transform.position) == VoxelData.Water && !isPausing)
        {
            //入水
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
        //screen切换
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
            // 控制缩放
            float scaleX = 1.0f + Mathf.PingPong(offset * speed, magnitude) * 0.5f; // 控制x轴的缩放
            float scaleY = 1.0f + Mathf.PingPong(offset * speed, magnitude) * 0.5f; // 控制y轴的缩放
            muacherish.transform.localScale = new Vector3(scaleX, scaleY, muacherish.transform.localScale.z);

            // 控制颜色渐变
            Color color = new Color(
                Mathf.Sin(offset * colorSpeed) * 0.5f + 0.5f, // 红
                Mathf.Sin(offset * colorSpeed + Mathf.PI * 2 / 3) * 0.5f + 0.5f, // 绿
                Mathf.Sin(offset * colorSpeed + Mathf.PI * 4 / 3) * 0.5f + 0.5f, // 蓝
                1f // 不透明度
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

        //检查是否输入种子
        world.CheckSeed();

        //检查是否输入RenderSize
        world.CheckRenderSize();

        if (world.InitError == false)
        {
            //清内存
            MainCamera.SetActive(false);

            //screen切换
            Init_Screen.SetActive(false);
            Loading_Screen.SetActive(true);

            //游戏状态切换
            world.game_state = Game_State.Loading;

            //主摄像机切换
            PlayerObject.SetActive(true);
            MainCamera.SetActive(false);

            //其他
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
        //screen切换
        Init_Screen.SetActive(false);
        Start_Screen.SetActive(true);
        isInTheInitScreen = false;
        //music
        musicmanager.PlaySound_Click();
    }

    //选择生存模式
    public void GamemodeToSurvival()
    {
        world.game_mode = GameMode.Survival;
        gamemodeTEXT.text = "当前游戏模式：生存模式";

        //改变按钮颜色
        SurvivalButtom.GetComponent<Image>().color = new Color(106 / 255f, 115 / 255f, 200 / 255f, 1f);
        CreativeButtom.GetComponent<Image>().color = new Color(149 / 255f, 134 / 255f, 119 / 255f, 1f);
    }

    //选择创造模式
    public void GamemodeToCreative()
    {
        world.game_mode = GameMode.Creative;
        gamemodeTEXT.text = "当前游戏模式：创造模式";

        //改变按钮颜色
        SurvivalButtom.GetComponent<Image>().color = new Color(149 / 255f, 134 / 255f, 119 / 255f, 1f);
        CreativeButtom.GetComponent<Image>().color = new Color(106 / 255f, 115 / 255f, 200 / 255f, 1f);
    }


    //---------------------------------------------------------------------------------------






    //--------------------------------- Loading_Screen -------------------------------------

    //Loading结束的时候加一个睁眼的动画
    void openyoureyes()
    {
        OpenYourEyes.SetActive(true);
        StartCoroutine(Animation_Openyoureyes());
    }

    IEnumerator Animation_Openyoureyes()
    {
        Image image = OpenYourEyes.GetComponent<Image>();

        // 记录渐变开始的时间
        float startTime = Time.time;

        while (Time.time - startTime < eyesOpenTime)
        {
            // 计算当前的透明度
            float alpha = Mathf.Lerp(1f, 0f, (Time.time - startTime) / eyesOpenTime);

            // 更新Image组件的透明度
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);

            // 等待一帧
            yield return null;
        }

        // 渐变结束后，将透明度设置为完全透明
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);

        //直接给隐藏
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
            //可视
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


        // 将鼠标锁定在屏幕中心
        Cursor.lockState = CursorLockMode.Locked;

        //鼠标不可视
        Cursor.visible = false;

        world.game_state = Game_State.Playing;
        ToolBar.SetActive(true);
        CursorCross_Screen.SetActive(true);
    }


    //打开操作指南
    public void Click_HowToPlay()
    {
        //music
        musicmanager.PlaySound_Click();

        StartCoroutine(Help_Animation_Open());

    }

    //Help动画
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


    //手电筒的提示
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
        //保存原来值
        Vector2 oldPosition = prompt.GetComponent<RectTransform>().anchoredPosition;
        float sx = oldPosition.x;

        //-344~344缓慢拉出来
        while (sx <= 344f)
        {
            sx += Time.deltaTime * promptShowspeed;
            prompt.GetComponent<RectTransform>().anchoredPosition = new Vector2(sx, oldPosition.y);
            yield return null;
        }

        //矫正位置
        prompt.GetComponent<RectTransform>().anchoredPosition = new Vector2(344f, oldPosition.y);
        yield return null;
    }

    IEnumerator Hide_Animation_PromptScreen()
    {
        //保存原来值
        Vector2 oldPosition = prompt.GetComponent<RectTransform>().anchoredPosition;
        float sx = oldPosition.x;

        //-344~344缓慢拉出来
        while (sx >= -344f)
        {
            sx -= Time.deltaTime * promptShowspeed;
            prompt.GetComponent<RectTransform>().anchoredPosition = new Vector2(sx, oldPosition.y);
            yield return null;
        }

        //矫正位置
        prompt.GetComponent<RectTransform>().anchoredPosition = new Vector2(-344f, oldPosition.y);
        prompt_Text.text = "";
        yield return null;
    }


    //--------------------------------------------------------------------------------------






    //---------------------------------- 实时修改值 ------------------------------------------

    //更新值
    void UpdatePauseScreenValue()
    {
        //bgm volume
        volume_bgm = slider_bgm.value;
        if (volume_bgm != previous_bgm)
        {
            //bgm
            musicmanager.Audio_envitonment.volume = Mathf.Lerp(0f, 1f, volume_bgm);

            //更新previous
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

            //更新previous
            previous_sound = volume_sound;
        }

        //MouseSensitivity
        Mouse_Sensitivity = Mathf.Lerp(1f, 4f, slider_MouseSensitivity.value);

        if (Mouse_Sensitivity != previous_Mouse_Sensitivity)
        {
            //改变鼠标灵敏度

            //更新previous
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

            //更新previous
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

            //更新previous
            previous_SuperMining_isOn = SuperMining_isOn;
        }

    }


    //---------------------------------------------------------------------------------------





    //---------------------------------- 死亡与重生 -----------------------------------------
    public void PlayerDead()
    {
        DeadScreen.SetActive(true);

        //Score
        endTime = Time.time;
        scoreText.text = $"分数：{(int)(endTime - startTime)}";

        //解除鼠标
        Cursor.lockState = CursorLockMode.None;
        //鼠标可视
        Cursor.visible = true;

        world.game_state = Game_State.Dead;
    }

    public void PlayerClickToRestart()
    {
        DeadScreen.SetActive(false);

        // 将鼠标锁定在屏幕中心
        Cursor.lockState = CursorLockMode.Locked;
        //鼠标不可视
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






    //------------------------------------ 工具类 -------------------------------------------
    void HideCursor()
    {
        // 将鼠标锁定在屏幕中心
        Cursor.lockState = CursorLockMode.Locked;
        //鼠标不可视
        Cursor.visible = false;
    }

    public void QuitGame()
    {
        //music
        musicmanager.PlaySound_Click();

        Application.Quit();
    }

    //显示Block名字
    public void Change_text_selectBlockname(byte prokeblocktype)
    {
        //255代表是切换方块
        //非255代表是破坏方块，直接播放破坏方块的名字就行
        if (prokeblocktype == 255)
        {
            //如果有方块的话
            if (BackPackManager.istheindexHaveBlock(player.selectindex))
            {
                //如果有协程在执行，则立即终止
                if (showblocknameCoroutine != null)
                {
                    StopCoroutine(showblocknameCoroutine);
                }

                showblocknameCoroutine = StartCoroutine(showblockname(prokeblocktype));
            }
        }
        else
        {
            //如果有协程在执行，则立即终止
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
            //显示这个方块的名字2s
            selectblockname.text = world.blocktypes[BackPackManager.slots[player.selectindex].blockId].blockName;

            yield return new WaitForSeconds(2f);

            selectblockname.text = "";

            showblocknameCoroutine = null;
        }
        else
        {
            //显示这个方块的名字2s
            selectblockname.text = world.blocktypes[_blocktype].blockName;

            yield return new WaitForSeconds(2f);

            selectblockname.text = "";

            showblocknameCoroutine = null;
        }
        
    }

    //--------------------------------------------------------------------------------------




}
