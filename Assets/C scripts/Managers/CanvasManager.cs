using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class CanvasManager : MonoBehaviour
{
    [Header("UIMAnager")]
    public List<CanvasId> UIManager = new List<CanvasId>();
    












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
    //public GameObject Survival_Screen;

    //主要屏幕
    //public GameObject Start_Screen;
    //public GameObject Init_Screen;
    //public GameObject Loading_Screen;

    //过渡对象
    //public Slider slider;
    //public TextMeshProUGUI tmp;
    //public GameObject handle;

    //InitScreen
    //public TextMeshProUGUI ErrorMessage;
    //public Toggle SuperPlainToggle;

    //Playing屏幕内容
    public GameObject OpenYourEyes;
    //public GameObject Debug_Screen;
    //public GameObject ToolBar;
    //public GameObject CursorCross_Screen;
    public GameObject Swimming_Screen;
    //public GameObject Pause_Screen;
    //public GameObject HowToPlay_Screen;
    public GameObject Prompt_Screen;
    public GameObject prompt;
    public TextMeshProUGUI prompt_Text;
    //public GameObject DeadScreen;

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
    //public TextMeshProUGUI scoreText;
    

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


        //加载中
        if (world.game_state == Game_State.Loading)
        {
            LoadingWorld();
        }

        //加载完成
        else if (world.game_state == Game_State.Playing)
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

        //Pause
        else if (world.game_state == Game_State.Pause)
        {
            Show_Esc_Screen();
        }

        Prompt_FlashLight();
    }


    //----------------------------------------------------------------------------------------






    //------------------------------------- 重置版 -----------------------------------------------------------------------------------------------------------------------------------------------

    //建立ui缓冲栈
    [Header("重置区域")]
    public FixedStack<int> UIBuffer = new FixedStack<int>(5, 0);

    [Header("状态")]
    public bool isInitError = false;
    [HideInInspector]public float Initprogress = 0;
    private int previous_mappedValue;
    public bool NotNeedBackGround = false; //游戏中暂停隐藏背景的

    //Score
    float startTime; float endTime;

    // 当前世界类型
    public int currentWorldType = VoxelData.Biome_Default; //5代表默认
    private int numberofWorldType = 7; // 总量-这个不能乱加了


    //------------------------------------- 主要函数 ------------------------------------------

    // 跳转UI
    public void SwitchToUI(int _TargetID)
    {
        //异常检查
        if (_TargetID < 0 || _TargetID >= UIManager.Count)
        {
            Debug.LogError("非法ID");
            return;
        }
        
        

        //根据目标面板进行特殊判断
        if (UpdateCanvasState(_TargetID))
        {
            //隐藏上级，显示下级
            if (UIBuffer.Count() > 0)
            {

                UIManager[UIBuffer.Peek()].canvas.SetActive(false);
            }
            UIManager[_TargetID].canvas.SetActive(true);

            //Music得在根据目标面板进行特殊判断前面
            musicmanager.PlaySound_Click();
        }


        

        UIBuffer.Push(_TargetID);  // 将目标UI的ID加入固定栈
        //print($"加入 {_TargetID}, count {UIBuffer.Count()}");
    }

    //返回上级UI
    public void BackToPrevious()
    {
        if (UIBuffer.Count() > 0)
        {

            int currentUIID = UIBuffer.Pop(); // 弹出当前UI的ID
            int previousUIID = UIBuffer.Peek(); // 获取上一级UI的ID（但不弹出）

            // 显示上级UI
            UIManager[currentUIID].canvas.SetActive(false);
            UIManager[previousUIID].canvas.SetActive(true);


            //如果是细节面板，则把子目录隐藏
            if (currentUIID == VoxelData.ui选项细节)
            {
                UIManager[Detail_Number].canvas.SetActive(false);
            }


            //Music
            musicmanager.PlaySound_Click();

            //print($"上一级UI: {previousUIID}, 缓冲区大小: {UIBuffer.Count()}");
        }
        else
        {
            Debug.Log("没有上一级UI可返回");
        }
    }


    //根据目标面板进行特殊判断
    //返回值，是否可以回退
    public bool UpdateCanvasState(int _TargetID)
    {

        //判断是不是选项
        if (_TargetID == VoxelData.ui选项)
        {
            if (NotNeedBackGround)
            {
                UIManager[_TargetID].canvas.GetComponent<Image>().color = new Color(1, 1, 1, 120f / 255);
            }
            else
            {
                UIManager[_TargetID].canvas.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }
        }
        //判断是不是选项细节
        else if (_TargetID == VoxelData.ui选项细节)
        {

            if (NotNeedBackGround)
            {
                UIManager[_TargetID].canvas.GetComponent<Image>().color = new Color(1, 1, 1, 120f / 255);
            }
            else
            {
                UIManager[_TargetID].canvas.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }

            UIManager[_TargetID].childs[Detail_Number]._object.SetActive(true);
        }

        //游戏模式的判断
        else if (_TargetID == VoxelData.ui加载世界)
        {

            if (isInitError)
            {
                return false;
            }
            else
            {
                //游戏状态切换
                world.game_state = Game_State.Loading;

                //主摄像机切换
                PlayerObject.SetActive(true);
                MainCamera.SetActive(false);

                //其他
                HideCursor();
            }
            

        }

        //玩家模式
        else if (_TargetID == VoxelData.ui玩家)
        {
            // 将鼠标锁定在屏幕中心
            Cursor.lockState = CursorLockMode.Locked;
            //鼠标不可视
            Cursor.visible = false;

            if (world.game_mode == GameMode.Survival)
            {
                UIManager[_TargetID].childs[0]._object.SetActive(true);
            }
            else
            {
                UIManager[_TargetID].childs[0]._object.SetActive(false);
            }

        }

        //从游戏中暂停
        else if (_TargetID == VoxelData.ui游戏中暂停)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        //死亡
        else if (_TargetID == VoxelData.ui死亡)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UIManager[_TargetID].childs[0]._object.GetComponent<TextMeshProUGUI>().text = $"分数：{(int)(endTime - startTime)}";
        }

        //一般都是true
        return true;
    }


    //------------------------------------- 新建世界 ------------------------------------------
    public void OnDeselect_SaveWorldSettings(int _id)
    {
        //print("OnDeselect_SaveWorldSettings");
        switch (_id)
        {
            //保存名字
            case 0:
                world.worldSetting.name = UIManager[VoxelData.ui初始化_新建世界].childs[0]._object.GetComponent<TMP_InputField>().text;
                break;

            //保存种子
            case 1:
                
                String _text = UIManager[VoxelData.ui初始化_新建世界].childs[1]._object.GetComponent<TMP_InputField>().text;
                int _number;


                if (_text != null && string.IsNullOrEmpty(_text))
                {
                    isInitError = true;
                    //print("种子为空!");
                    UIManager[VoxelData.ui初始化_新建世界].childs[4]._object.GetComponent<TextMeshProUGUI>().text = "种子为空！";
                }
                else
                {
                    if (int.TryParse(_text, out _number))
                    {
                        
                        if (_number > 0)
                        {
                            if (isInitError)
                            {
                                isInitError = false;
                                UIManager[VoxelData.ui初始化_新建世界].childs[4]._object.GetComponent<TextMeshProUGUI>().text = " ";
                            }

                            
                            world.worldSetting.seed = _number;
                            
                        } 
                        else
                        {
                            isInitError = true;
                            //Debug.Log("种子必须大于0！");
                            UIManager[VoxelData.ui初始化_新建世界].childs[4]._object.GetComponent<TextMeshProUGUI>().text = "种子必须大于0！";

                        }

                    }
                    else if (_text == "留空以生成随机种子")
                    {
                        isInitError = false;
                    }

                    else
                    {
                        isInitError = true;
                        //Debug.Log("种子转换失败！");
                        UIManager[VoxelData.ui初始化_新建世界].childs[4]._object.GetComponent<TextMeshProUGUI>().text = "种子转换失败！";
                    }
                }
                    
                
                break;

            //渲染区块
            case 2:

                float Rendervalue = UIManager[VoxelData.ui初始化_新建世界].childs[5]._object.GetComponent<Slider>().value;

                //将值归一到具体的int
                //2 7 12 17 23 
                int mappedValue = Mathf.RoundToInt(Mathf.Lerp(2, 23, Rendervalue));

                if (previous_mappedValue != mappedValue)
                {
                    //改变文本
                    UIManager[VoxelData.ui初始化_新建世界].childs[6]._object.GetComponent<TextMeshProUGUI>().text = $"渲染距离：{mappedValue} 区块";

                    //改变world
                    world.renderSize = mappedValue;

                    previous_mappedValue = mappedValue;
                }

                break;

            //保存游戏模式
            case 3:
                break;

            //保存世界类型
            case 4:
                break;

            default:
                break;
        }
    }


    //加载进度条
    public void LoadingWorld()
    {
        TextMeshProUGUI progressNumber = UIManager[VoxelData.ui加载世界].childs[0]._object.GetComponent<TextMeshProUGUI>();
        GameObject progressHandle = UIManager[VoxelData.ui加载世界].childs[1]._object;

        //UpdateText
        progressNumber.text = $"{(Initprogress * 100):F2} %";

        //UpdateScrollBar
        float x = Mathf.Lerp(1f, 6f, Initprogress);
        float y = Mathf.Lerp(1f, 6f, Initprogress);
        progressHandle.transform.localScale = new Vector3(x, y, 1f);

        //如果进度条满了
        if (Initprogress == 1)
        {
            world.game_state = Game_State.Playing;

            SwitchToUI(VoxelData.ui玩家);

        }
    }


    //------------------------------------- 工具 ------------------------------------------

    //切换游戏模式
    public void ClickToSwitchGameMode()
    {
        if (world.game_mode == GameMode.Survival)
        {
            //改为创造模式
            world.game_mode = GameMode.Creative;
            UIManager[VoxelData.ui初始化_新建世界].childs[2]._object.GetComponent<TextMeshProUGUI>().text = "游戏模式：创造";
        }
        else
        {
            //改为生成模式
            world.game_mode = GameMode.Survival;
            UIManager[VoxelData.ui初始化_新建世界].childs[2]._object.GetComponent<TextMeshProUGUI>().text = "游戏模式：生存";
        }

        musicmanager.PlaySound_Click();
    }

    //切换世界类型
    public void ClickToSwitchWorldType()
    {

        // 切换到下一个世界类型
        currentWorldType = (currentWorldType + 1) % numberofWorldType;
       
        switch (currentWorldType)
        {
            case 0:
                UIManager[VoxelData.ui初始化_新建世界].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "世界类型：草原群系";
                break;
            case 1:
                UIManager[VoxelData.ui初始化_新建世界].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "世界类型：高原群系";
                break;
            case 2:
                UIManager[VoxelData.ui初始化_新建世界].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "世界类型：沙漠群系";
                break;
            case 3:
                UIManager[VoxelData.ui初始化_新建世界].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "世界类型：沼泽群系";
                break;
            case 4:
                UIManager[VoxelData.ui初始化_新建世界].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "世界类型：密林群系";
                break;
            case 5:
                UIManager[VoxelData.ui初始化_新建世界].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "世界类型：默认";
                break;
            case 6:
                UIManager[VoxelData.ui初始化_新建世界].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "世界类型：超平坦";
                break;
            default:
                print("ClickToSwitchWorldType出错");
                break;
        }

        

        musicmanager.PlaySound_Click();
    }


    //无背景的情况下打开选项ui
    public void SwitchNoBackGround(bool _t) 
    {
        NotNeedBackGround = _t;
    }

    //关闭细节面板的子目录
    public void CloseDetail_Child(int _Id)
    {
        UIManager[VoxelData.ui选项细节].childs[_Id]._object.SetActive(false);
    }

    //跳动的MuaCherish
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

    //选项调用选项细节面板的时候传递的参数
    private int Detail_Number = 0;
    public void UpdateDetail_Number(int _t)
    {
        Detail_Number = _t;
    }

    //打开网站
    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    //Fov
    public void ChangeFOV()
    {
        // 获取滑动条的值
        float sliderValue = UIManager[VoxelData.ui选项].childs[0]._object.GetComponent<Slider>().value;

        // 将滑动条的值映射到FOV的范围内
        float newFOV = Mathf.Lerp(50f, 90f, sliderValue);

        // 设置玩家的FOV
        player.CurrentFOV = newFOV;
        player.eyes.fieldOfView = newFOV;
    }


    //-------------------------------------------------------------------------------------------------------------------------------------------------------------







    //------------------------------------- GameMode -----------------------------------------
    //Survival
    void GameMode_Survival()
    {
        if (hasExec_Playing)
        {
            //ToolBar.SetActive(true);
            //CursorCross_Screen.SetActive(true);
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
            //Debug_Screen.SetActive(!Debug_Screen.activeSelf);
        }


        //SwimmingScreen
        if (world.GetBlockType(Camera.transform.position + new Vector3(0f, 0.2f, 0f)) == VoxelData.Water)
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
            //Loading_Screen.SetActive(false);
            //ToolBar.SetActive(true);
            //Survival_Screen.SetActive(false);
            //CursorCross_Screen.SetActive(true);
            Prompt_Screen.SetActive(true);

            //toggle_SuperMing.isOn = true;
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
            //Debug_Screen.SetActive(!Debug_Screen.activeSelf);
        }


        //SwimmingScreen
        if (world.GetBlockType(Camera.transform.position) == VoxelData.Water)
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






    //----------------------------------- Init_Screen ---------------------------------------
  

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
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            if (!isPausing)
            {
                //未暂停
                isPausing = !isPausing;
                
                SwitchToUI(VoxelData.ui游戏中暂停);
                world.game_state = Game_State.Pause;
            }
            else
            {
                //正在暂停
                isPausing = !isPausing;

                SwitchToUI(VoxelData.ui玩家);
                world.game_state = Game_State.Playing;
            }

        }
    }

    //回到游戏
    public void BackToGame()
    {

        isPausing = !isPausing;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SwitchToUI(VoxelData.ui玩家);
        world.game_state = Game_State.Playing;
    }





    ////Help动画
    //IEnumerator Help_Animation_Open()
    //{
    //    HowToPlay_Screen.SetActive(true);
    //    yield return null;

    //    for (float y = 0; y <= 1; y += 0.1f)
    //    {
    //        HowToPlay_Screen.transform.localScale = new Vector3(1f, y, 1f);
    //        yield return null;
    //    }

    //    HowToPlay_Screen.transform.localScale = new Vector3(1f, 1f, 1f);
    //}

    //IEnumerator Help_Animation_Close()
    //{
    //    for (float y = 1; y >= 0; y -= 0.1f)
    //    {
    //        HowToPlay_Screen.transform.localScale = new Vector3(1f, y, 1f);
    //        yield return null;
    //    }

    //    HowToPlay_Screen.SetActive(false);

    //}



    //Help_Close
    //public void Back_Pause_Screen()
    //{
    //    StartCoroutine(Help_Animation_Close());

    //    //music
    //    musicmanager.PlaySound_Click();
    //}


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
        //DeadScreen.SetActive(true);
        SwitchToUI(VoxelData.ui死亡);

       

        world.game_state = Game_State.Dead;
    }

    public void PlayerClickToRestart()
    {
        //DeadScreen.SetActive(false);
        SwitchToUI(VoxelData.ui玩家);

        

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


//UI管理器
[System.Serializable]
public class CanvasId
{
    public string name;
    public GameObject canvas;
    public List<UIChild> childs;
}

//UI子类
[System.Serializable]
public class UIChild
{
    public string name;
    public GameObject _object;
}


//固定大小的栈
public class FixedStack<T>
{ 
    private List<T> stack;
    private int capacity;

    public FixedStack(int capacity, T initialElement)
    {
        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than 0");
        }

        this.capacity = capacity;
        stack = new List<T>(capacity);

        // 初始化时添加一个初始元素
        stack.Add(initialElement);
    }

    public void Push(T item)
    {
        if (stack.Count >= capacity)
        {
            // 超出容量，移除最底下的元素
            stack.RemoveAt(0);
        }

        stack.Add(item);
    }

    public T Pop()
    {
        if (IsEmpty())
        {
            throw new InvalidOperationException("Stack is empty.");
        }

        // 获取栈顶元素
        T item = stack[stack.Count - 1];
        stack.RemoveAt(stack.Count - 1);
        return item;
    }

    public T Peek()
    {
        if (IsEmpty())
        {
            throw new InvalidOperationException("Stack is empty.");
        }

        return stack[stack.Count - 1];
    }

    public bool IsEmpty()
    {
        return stack.Count == 0;
    }

    public int Count()
    {
        return stack.Count;
    }

    public int Capacity()
    {
        return capacity;
    }
    public T[] ToArray()
    {
        // 将 List<T> 转换为数组
        return stack.ToArray();
    }
}