using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
//using static UnityEditor.Progress;
using System.IO;
using System.Diagnostics;
using UnityEngine.EventSystems;
using System.Reflection;


public class CanvasManager : MonoBehaviour
{
    public ManagerHub managerhub;
    [Header("UIMAnager")]
    public List<CanvasId> UIManager = new List<CanvasId>();

    [Header("Transforms")]
    //��������
    public ParticleSystem partSystem;
    public World world;
    public Transform Camera;
    public MusicManager musicmanager;
    //public GameObject MainCamera;
    //public GameObject PlayerObject;
    public Player player;
    public TextMeshProUGUI selectblockname;
    public BackPackManager BackPackManager;
    public LifeManager LifeManager;
    //public TextMeshProUGUI gamemodeTEXT;
    //public GameObject CreativeButtom;
    //public GameObject SurvivalButtom;
    //public GameObject Survival_Screen;

    //��Ҫ��Ļ
    //public GameObject Start_Screen;
    //public GameObject Init_Screen;
    //public GameObject Loading_Screen;

    //���ɶ���
    //public Slider slider;
    //public TextMeshProUGUI tmp;
    //public GameObject handle;

    //InitScreen
    //public TextMeshProUGUI ErrorMessage;
    //public Toggle SuperPlainToggle;

    //Playing��Ļ����
    public GameObject OpenYourEyes;
    //public GameObject Debug_Screen;
    //public GameObject ToolBar;
    //public GameObject CursorCross_Screen;
    public GameObject Swimming_Screen;
    //public GameObject Pause_Screen;
    //public GameObject HowToPlay_Screen;
    //public GameObject Prompt_Screen;
    public GameObject prompt;
    public TextMeshProUGUI prompt_Text;
    //public GameObject DeadScreen;

    //�޸�ֵ����
    //public Slider slider_bgm;
    //public Slider slider_sound;
    //public Slider slider_MouseSensitivity;
    //public Toggle toggle_SpaceMode;  
    //public Toggle toggle_SuperMing;


    //��Ϸ״̬�ж�
    public bool isPausing = false;


    //�޸�ֵ
    [Header("Options")]
    //bgm
    public float volume_bgm = 0.5f;
    //private float previous_bgm = 0.5f;

    //sound
    public float volume_sound = 0.5f;
    //private float previous_sound = 0.5f;

    //render speed
    public float Mouse_Sensitivity = 1f;
    //private float previous_Mouse_Sensitivity = 1f;

    //isSpaceMode
    public bool SpaceMode_isOn = false;
    //private bool previous_spaceMode_isOn = false;

    //isSpaceMode
    public bool SuperMining_isOn = false;
    //private bool previous_SuperMining_isOn = false;

    //pormpt
    public float promptShowspeed = 400f;

    //eyestime
    public float eyesOpenTime = 0.5f;

    //Jump_MuaCherish
    public GameObject muacherish;
    public float speed = 1.0f; // ���Ƹ����ٶȵĲ���
    public float magnitude = 0.04f; // ���Ƹ������ȵĲ���
    public float colorSpeed = 1f; //������ɫ�ٶ�
    Coroutine muacherishCoroutine;

    //ShowBlockName
    Coroutine showblocknameCoroutine;

    //Score
    //public TextMeshProUGUI scoreText;


    //һ���Դ���
    bool hasExec_Playing = true;
    public bool hasExec_PromptScreen_isShow = false;
    //bool hasExec_PromptScreen_isHide = true;
    bool hasExec_InWater = false;

    //debug


    //----------------------------------- �������� ---------------------------------------

    private void Start()
    {
        InitCanvasManager();
    }

    public void InitCanvasManager()
    {
        if (muacherishCoroutine == null)
        {
            muacherishCoroutine = StartCoroutine(jumpMuaCherish());
        }
        waittoFinishSaveAndBackToMenuCoroutine = null;

        // ��ʼ���޸�ֵ����
        isPausing = false;
        volume_bgm = 0.5f;
        //previous_bgm = 0.5f;
        volume_sound = 0.5f;
        //previous_sound = 0.5f;
        //Mouse_Sensitivity = 1f;
        //previous_Mouse_Sensitivity = 1f;
        SpaceMode_isOn = false;
        //previous_spaceMode_isOn = false;
        SuperMining_isOn = false;
        //previous_SuperMining_isOn = false;

        // ��ʼ����ʾ��Ϣ��ʾ�ٶ�
        promptShowspeed = 400f;

        // ��ʼ������ʱ��
        eyesOpenTime = 0.5f;

        // ��ʼ����������
        speed = 0.15f;
        magnitude = 0.04f;
        colorSpeed = 1f;
        muacherishCoroutine = null;

        // ��ʼ����ʾ��������Э��
        showblocknameCoroutine = null;

        // ��ʼ��һ���Դ����ִ��״̬
        hasExec_Playing = true;
        hasExec_PromptScreen_isShow = false;
        //hasExec_PromptScreen_isHide = true;
        hasExec_InWater = false;

        // ��ʼ��UI����ջ
        UIBuffer = new FixedStack<int>(5, 0);

        // ��ʼ��״̬��ر���
        isInitError = false;
        Initprogress = 0;
        NotNeedBackGround = false;

        // ��ʼ������ʱ��
        startTime = 0f;
        endTime = 0f;

        // ��ʼ����ǰ��������
        currentWorldType = TerrainData.Biome_Default;
        numberofWorldType = 7; // ����

        // ��ʼ����Ҫ�ڲ�����ı���
        RenderSize_Value = previous_RenderSize_Value;

        // ��ʼ����������
        previous_mappedValue = 0;  // ��Ⱦ��Χ
        previous_starttoreder_mappedValue = 0;  // ��ʼ��Ⱦ��Χ

        //�ָ���ť
        isClickSaving = false;
        UIManager[CanvasData.ui��ʼ��_ѡ��浵].childs[0]._object.GetComponent<Image>().color = new Color(58f / 255, 58f / 255, 58f / 255, 1);
        UIManager[CanvasData.ui��ʼ��_ѡ��浵].childs[1]._object.GetComponent<Image>().color = new Color(58f / 255, 58f / 255, 58f / 255, 1);
    }


    private void Update()
    {

        //������
        if (world.game_state == Game_State.Loading)
        {
            OpenYourEyes.SetActive(true);
            LoadingWorld();

        }

        //�������
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

            if (Input.GetKeyDown(KeyCode.E) && isOpenBackpack && !managerhub.commandManager.isConsoleActive && !managerhub.player.isSpectatorMode)
            {
                isPausing = false;
                isOpenBackpack = false;
                SwitchUI_Player(-1);
                managerhub.world.game_state = Game_State.Playing;

                //print("E �رձ���");
                CheckSwapBlockAndDropOut();
            }

            //LayintSwapBlock();

        }


        Prompt_FlashLight();
    }

    //���ؽ�����
    public void LoadingWorld()
    {
        TextMeshProUGUI progressNumber = UIManager[CanvasData.ui��������].childs[0]._object.GetComponent<TextMeshProUGUI>();
        GameObject progressHandle = UIManager[CanvasData.ui��������].childs[1]._object;

        //UpdateText
        progressNumber.text = $"{(Initprogress * 100):F2} %";

        //UpdateScrollBar
        float x = Mathf.Lerp(1f, 6f, Initprogress);
        float y = Mathf.Lerp(1f, 6f, Initprogress);
        progressHandle.transform.localScale = new Vector3(x, y, 1f);

        //�������������
        if (Initprogress == 1)
        {
            world.game_state = Game_State.Playing;

            SwitchToUI(CanvasData.ui���);

        }
    }



    //----------------------------------------------------------------------------------------






    //------------------------------------- ���ð� -----------------------------------------------------------------------------------------------------------------------------------------------

    //����ui����ջ
    [Header("Transforms")]
    //public NighManager nightmanager;
    public FixedStack<int> UIBuffer = new FixedStack<int>(5, 0);

    [Header("״̬")]
    public bool isInitError = false;
    [HideInInspector] public float Initprogress = 0;
    public bool NotNeedBackGround = false; //��Ϸ����ͣ���ر�����
    public bool isClickSaving = false;

    [Header("ѡ��浵")]
    public GameObject NewWorld_item;
    public Transform NewWorld_Transform;

    //Score
    float startTime; float endTime;

    // ��ǰ��������
    private int currentWorldType = TerrainData.Biome_Default;
    private int numberofWorldType = 7; // ����-��������Ҽ���

    //��Ҫ�ڲ�����ı���
    private float RenderSize_Value = 0.4f; private float previous_RenderSize_Value = 0.4f;

    //��������
    private int previous_mappedValue;  //��Ⱦ��Χ 
    private int previous_starttoreder_mappedValue;  //��ʼ��Ⱦ��Χ

    //------------------------------------- ��Ҫ���� ------------------------------------------

    // ��תUI
    public void SwitchToUI(int _TargetID)
    {
        //�쳣���
        if (_TargetID < 0 || _TargetID >= UIManager.Count)
        {
            UnityEngine.Debug.LogError("�Ƿ�ID");
            return;
        }



        //����Ŀ�������������ж�
        if (UpdateCanvasState(_TargetID))
        {
            //�����ϼ�����ʾ�¼�
            if (UIBuffer.Count() > 0)
            {

                UIManager[UIBuffer.Peek()].canvas.SetActive(false);
            }
            UIManager[_TargetID].canvas.SetActive(true);

            //Music���ڸ���Ŀ�������������ж�ǰ��
            managerhub.NewmusicManager.PlayOneShot(MusicData.click);
        }




        UIBuffer.Push(_TargetID);  // ��Ŀ��UI��ID����̶�ջ
        //print($"���� {_TargetID}, count {UIBuffer.Count()}");
    }

    //�����ϼ�UI
    public void BackToPrevious()
    {
        if (UIBuffer.Count() > 0)
        {

            int currentUIID = UIBuffer.Pop(); // ������ǰUI��ID
            int previousUIID = UIBuffer.Peek(); // ��ȡ��һ��UI��ID������������

            // ��ʾ�ϼ�UI
            UIManager[currentUIID].canvas.SetActive(false);
            UIManager[previousUIID].canvas.SetActive(true);


            //�����ϸ����壬�����Ŀ¼����
            if (currentUIID == CanvasData.uiѡ��ϸ��)
            {
                UIManager[Detail_Number].canvas.SetActive(false);
            }


            //Music
            managerhub.NewmusicManager.PlayOneShot(MusicData.click);

            //print($"��һ��UI: {previousUIID}, ��������С: {UIBuffer.Count()}");
        }
        else
        {
            UnityEngine.Debug.Log("û����һ��UI�ɷ���");
        }
    }


    //����Ŀ�������������ж�
    //����ֵ���Ƿ���Ի���
    public bool UpdateCanvasState(int _TargetID)
    {
        //ѡ��浵
        if (_TargetID == CanvasData.ui��ʼ��_ѡ��浵)
        {
            foreach (Transform child in NewWorld_Transform)
            {
                // ����������
                GameObject.Destroy(child.gameObject);
            }

            world.LoadAllSaves(world.savingPATH);
        }


        //�½�����
        else if (_TargetID == CanvasData.ui��ʼ��_�½�����)
        {
            //InitAllManagers();

            //��ʼ������
            UIManager[_TargetID].childs[0]._object.GetComponent<TMP_InputField>().text = "�µ�����"; //Name
            UIManager[_TargetID].childs[1]._object.GetComponent<TMP_InputField>().text = "�����������������"; //Seed
            UIManager[_TargetID].childs[2]._object.GetComponent<TextMeshProUGUI>().text = "��Ϸģʽ������"; //GameMode
            UIManager[_TargetID].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "�������ͣ�Ĭ��"; //WorldType

            //rendersize
            UIManager[_TargetID].childs[5]._object.GetComponent<Slider>().value = RenderSize_Value;
            UIManager[_TargetID].childs[6]._object.GetComponent<TextMeshProUGUI>().text = $"��Ⱦ���룺{Mathf.RoundToInt(Mathf.Lerp(2, 23, RenderSize_Value))} ����";
        }


        //�ж��ǲ���ѡ��
        else if (_TargetID == CanvasData.uiѡ��)
        {
            if (NotNeedBackGround)
            {
                UIManager[_TargetID].canvas.GetComponent<Image>().color = new Color(0, 0, 0, 120f / 255);
            }
            else
            {
                UIManager[_TargetID].canvas.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }
        }
        //�ж��ǲ���ѡ��ϸ��
        else if (_TargetID == CanvasData.uiѡ��ϸ��)
        {

            //ͬ��һЩ����

            //rendersize
            UIManager[CanvasData.uiѡ��ϸ��].childs[5]._object.GetComponent<Slider>().value = RenderSize_Value;
            UIManager[CanvasData.uiѡ��ϸ��].childs[22]._object.GetComponent<TextMeshProUGUI>().text = $"��Ⱦ���룺{Mathf.RoundToInt(Mathf.Lerp(2, 23, RenderSize_Value))} ����";

            if (NotNeedBackGround)
            {
                UIManager[_TargetID].canvas.GetComponent<Image>().color = new Color(0, 0, 0, 120f / 255);
            }
            else
            {
                UIManager[_TargetID].canvas.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }

            UIManager[_TargetID].childs[Detail_Number]._object.SetActive(true);
        }

        //��Ϸģʽ���ж�
        else if (_TargetID == CanvasData.ui��������)
        {


            if (isInitError)
            {
                return false;
            }
            else
            {

                //��Ϸ״̬�л�
                world.game_state = Game_State.Loading;

                //��������л�
                //PlayerObject.SetActive(true);
                //MainCamera.SetActive(false);

                //����
                //HideCursor();
                ToggleMouseVisibilityAndLock(true);
            }


        }

        //���ģʽ
        else if (_TargetID == CanvasData.ui���)
        {
            // �������������Ļ����
            //Cursor.lockState = CursorLockMode.Locked;
            ////��겻����
            //Cursor.visible = false;
            ToggleMouseVisibilityAndLock(true);


            if (world.game_mode == GameMode.Survival)
            {
                UIManager[_TargetID].childs[0]._object.SetActive(true);
            }
            else
            {
                UIManager[_TargetID].childs[0]._object.SetActive(false);
            }

        }

        //����Ϸ����ͣ
        else if (_TargetID == CanvasData.ui��Ϸ����ͣ)
        {
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
            ToggleMouseVisibilityAndLock(false);
        }

        //����
        else if (_TargetID == CanvasData.ui����)
        {
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
            ToggleMouseVisibilityAndLock(false);


            UIManager[_TargetID].childs[0]._object.GetComponent<TextMeshProUGUI>().text = $"������{(int)(endTime - startTime)}";
        }

        //һ�㶼��true
        return true;
    }


    /// <summary>
    /// ���ݴ���Ĳ���ֵ���ز��̶���꣬���߻ָ����Ŀɼ��Ժ������ƶ�
    /// </summary>
    /// <param name="isLocked">���Ϊ true�������ز��̶���ꣻ���Ϊ false������ʾ���������</param>
    public void ToggleMouseVisibilityAndLock(bool isLocked)
    {
        if (isLocked)
        {
            //print("�������");
            // ��������겢������������Ļ����
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            //print("��ʾ���");
            // ��ʾ����겢�������
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }




    //--------------------------------- ����һ�������� ---------------------------------------



    //ѡ��浵���
    public void NewWorldGenerate(String name, String date, GameMode gamemode, int worldtype, int seed)
    {
        //��ʼ��item
        GameObject instance = Instantiate(NewWorld_item);
        instance.transform.SetParent(NewWorld_Transform, false);
        instance.transform.Find("TMP_WorldName").GetComponent<TextMeshProUGUI>().text = name;
        instance.transform.Find("TMP_Time").GetComponent<TextMeshProUGUI>().text = date;
        instance.transform.Find("TMP_GameMode").GetComponent<TextMeshProUGUI>().text = world.GetGameModeString(gamemode) + "   " + world.GetWorldTypeString(worldtype) + "   ���ӣ�" + seed;
    }
    //ɾ���浵��ť
    public void ClickToDeleteSaving()
    {
        if (world.PointSaving == "")
        {
            return;
        }

        // ��������·��
        string fullPath = Path.Combine(world.savingPATH, "Saves", world.PointSaving);

        // ȷ��Ҫɾ����·������
        if (Directory.Exists(fullPath))
        {
            //Debug.Log("�浵����");

            // ɾ���浵
            world.DeleteSave(fullPath);

            // ˢ�½���
            foreach (Transform child in NewWorld_Transform)
            {
                // ����������
                GameObject.Destroy(child.gameObject);
            }

            // ���¼��ش浵
            world.LoadAllSaves(world.savingPATH);
        }
        else
        {
            UnityEngine.Debug.LogWarning($"Ҫɾ���Ĵ浵·�� {fullPath} ������.");
        }
    }


    //����ѡ�������ѡ��
    public void LightButton()
    {
        isClickSaving = true;
        UIManager[CanvasData.ui��ʼ��_ѡ��浵].childs[0]._object.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        UIManager[CanvasData.ui��ʼ��_ѡ��浵].childs[1]._object.GetComponent<Image>().color = new Color(1, 1, 1, 1);
    }

    //������ص�ͼ��ui
    public void EnderSaving()
    {
        if (isClickSaving)
        {
            //���ش浵
            //print(world.savingPATH + "\\Saves\\" + world.PointSaving);
            world.LoadSavingData(world.savingPATH + "\\Saves\\" + world.PointSaving);
            //print(world.TheSaving.Count);

            SwitchToUI(CanvasData.ui��������);
        }
    }


    //�½��������
    public void Compoments_SaveWorldSettings(int _id)
    {
        //print("OnDeselect_SaveWorldSettings");
        switch (_id)
        {
            //��������
            case 0:
                world.worldSetting.name = UIManager[CanvasData.ui��ʼ��_�½�����].childs[0]._object.GetComponent<TMP_InputField>().text;
                break;

            //��������
            case 1:

                String _text = UIManager[CanvasData.ui��ʼ��_�½�����].childs[1]._object.GetComponent<TMP_InputField>().text;
                int _number;


                if (_text != null && string.IsNullOrEmpty(_text))
                {
                    isInitError = true;
                    //print("����Ϊ��!");
                    UIManager[CanvasData.ui��ʼ��_�½�����].childs[4]._object.GetComponent<TextMeshProUGUI>().text = "����Ϊ�գ�";
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
                                UIManager[CanvasData.ui��ʼ��_�½�����].childs[4]._object.GetComponent<TextMeshProUGUI>().text = " ";
                            }


                            world.worldSetting.seed = _number;

                        }
                        else
                        {
                            isInitError = true;
                            //Debug.Log("���ӱ������0��");
                            UIManager[CanvasData.ui��ʼ��_�½�����].childs[4]._object.GetComponent<TextMeshProUGUI>().text = "���ӱ������0��";

                        }

                    }
                    else if (_text == "�����������������")
                    {
                        isInitError = false;
                    }

                    else
                    {
                        isInitError = true;
                        //Debug.Log("����ת��ʧ�ܣ�");
                        UIManager[CanvasData.ui��ʼ��_�½�����].childs[4]._object.GetComponent<TextMeshProUGUI>().text = "����ת��ʧ�ܣ�";
                    }
                }


                break;

            //��Ⱦ����
            case 2:

                float Rendervalue = UIManager[CanvasData.ui��ʼ��_�½�����].childs[5]._object.GetComponent<Slider>().value;

                //����ȫ�ֱ���
                RenderSize_Value = Rendervalue;

                //��ֵ��һ�������int
                //2 7 12 17 23 
                int mappedValue = Mathf.RoundToInt(Mathf.Lerp(2, 23, Rendervalue));

                if (previous_mappedValue != mappedValue)
                {
                    //�ı��ı�
                    UIManager[CanvasData.ui��ʼ��_�½�����].childs[6]._object.GetComponent<TextMeshProUGUI>().text = $"��Ⱦ���룺{mappedValue} ����";

                    //�ı�world
                    world.renderSize = mappedValue;

                    previous_mappedValue = mappedValue;
                }

                break;

            //������Ϸģʽ
            case 3:
                if (world.game_mode == GameMode.Survival)
                {
                    //��Ϊ����ģʽ
                    world.game_mode = GameMode.Creative;
                    UIManager[CanvasData.ui��ʼ��_�½�����].childs[2]._object.GetComponent<TextMeshProUGUI>().text = "��Ϸģʽ������";
                }
                else
                {
                    //��Ϊ����ģʽ
                    world.game_mode = GameMode.Survival;
                    UIManager[CanvasData.ui��ʼ��_�½�����].childs[2]._object.GetComponent<TextMeshProUGUI>().text = "��Ϸģʽ������";
                }
                
                managerhub.NewmusicManager.PlayOneShot(MusicData.click);
                break;

            //������������
            case 4:
                // �л�����һ����������
                currentWorldType = (currentWorldType + 1) % numberofWorldType;

                switch (currentWorldType)
                {
                    case 0:
                        world.worldSetting.worldtype = TerrainData.Biome_Plain;
                        UIManager[CanvasData.ui��ʼ��_�½�����].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "�������ͣ���ԭȺϵ";
                        break;
                    case 1:
                        world.worldSetting.worldtype = TerrainData.Biome_Plateau;
                        UIManager[CanvasData.ui��ʼ��_�½�����].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "�������ͣ���ԭȺϵ";
                        break;
                    case 2:
                        world.worldSetting.worldtype = TerrainData.Biome_Dessert;
                        UIManager[CanvasData.ui��ʼ��_�½�����].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "�������ͣ�ɳĮȺϵ";
                        break;
                    case 3:
                        world.worldSetting.worldtype = TerrainData.Biome_Marsh;
                        UIManager[CanvasData.ui��ʼ��_�½�����].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "�������ͣ�����Ⱥϵ";
                        break;
                    case 4:
                        world.worldSetting.worldtype = TerrainData.Biome_Forest;
                        UIManager[CanvasData.ui��ʼ��_�½�����].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "�������ͣ�����Ⱥϵ";
                        break;
                    case 5:
                        world.worldSetting.worldtype = TerrainData.Biome_Default;
                        UIManager[CanvasData.ui��ʼ��_�½�����].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "�������ͣ�Ĭ��";
                        break;
                    case 6:
                        world.worldSetting.worldtype = TerrainData.Biome_SuperPlain;
                        UIManager[CanvasData.ui��ʼ��_�½�����].childs[3]._object.GetComponent<TextMeshProUGUI>().text = "�������ͣ���ƽ̹";
                        break;
                    default:
                        print("ClickToSwitchWorldType����");
                        break;
                }



                managerhub.NewmusicManager.PlayOneShot(MusicData.click);
                break;

            default:
                break;
        }
    }


    //ѡ�����


    //ѡ��ϸ�� - ��Ƶ����
    public void Compoments_VideoSettings(int _id)
    {
        switch (_id)
        {
            //��Ⱦ��Χ
            case 0:
                float Rendervalue = UIManager[CanvasData.uiѡ��ϸ��].childs[5]._object.GetComponent<Slider>().value;

                //����ȫ�ֱ���
                RenderSize_Value = Rendervalue;

                //��ֵ��һ�������int
                //2 7 12 17 23 
                int mappedValue = Mathf.RoundToInt(Mathf.Lerp(2, 23, Rendervalue));

                if (previous_mappedValue != mappedValue)
                {
                    //�ı��ı�
                    UIManager[CanvasData.uiѡ��ϸ��].childs[22]._object.GetComponent<TextMeshProUGUI>().text = $"��Ⱦ���룺{mappedValue} ����";

                    //�ı�world
                    world.renderSize = mappedValue;

                    previous_mappedValue = mappedValue;
                }

                break;

            //��ʼ��Ⱦ��Χ
            case 1:
                float StartToRendervalue = UIManager[CanvasData.uiѡ��ϸ��].childs[6]._object.GetComponent<Slider>().value;

                //��ֵ��һ�������int
                //2 7 12 17 23 
                int starttoreder_mappedValue = Mathf.RoundToInt(Mathf.Lerp(1, 4, StartToRendervalue));

                if (previous_starttoreder_mappedValue != starttoreder_mappedValue)
                {
                    //�ı��ı�
                    UIManager[CanvasData.uiѡ��ϸ��].childs[23]._object.GetComponent<TextMeshProUGUI>().text = $"��ʼ��Ⱦ�ľ��룺{starttoreder_mappedValue} ����";

                    //�ı�world
                    world.StartToRender = starttoreder_mappedValue;

                    previous_starttoreder_mappedValue = starttoreder_mappedValue;
                }

                break;
            default:
                break;
        }
    }

    //ѡ��ϸ�� - ��������Ч
    public void Compoments_MusicSettings(int _id)
    {
        float value;

        switch (_id)
        {
            //��������
            case 0:

                //GetValue
                value = UIManager[CanvasData.uiѡ��ϸ��].childs[7]._object.GetComponent<Slider>().value;

                //Update
                managerhub.NewmusicManager.SetAudioBackGroundVolumn(Mathf.Lerp(0f, 1f, value));

                break;

            //��Ч
            case 1:

                //GetValue
                value = UIManager[CanvasData.uiѡ��ϸ��].childs[8]._object.GetComponent<Slider>().value;

                //Update
                musicmanager.Audio_player_place.volume = Mathf.Lerp(0f, 0.8f, value);
                musicmanager.Audio_player_broke.volume = Mathf.Lerp(0f, 0.4f, value);
                musicmanager.Audio_player_moving.volume = Mathf.Lerp(0f, 0.6f, value);
                musicmanager.Audio_player_falling.volume = Mathf.Lerp(0f, 0.4f, value);
                musicmanager.Audio_player_diving.volume = Mathf.Lerp(0f, 0.8f, value);
                musicmanager.Audio_Click.volume = Mathf.Lerp(0f, 0.2f, value);
                musicmanager.Audio_Player_moving_swiming.volume = Mathf.Lerp(0f, 0.8f, value);

                break;
            default:
                break;
        }
    }

    //ѡ��ϸ�� - ��ҹ����
    public void Compoments_NightSettings(int _id)
    {
        switch (_id)
        {
            //����ʱ��
            case 0:

                break;

            //ҹ��ʱ��
            case 1:

                break;

            //����ʱ��
            case 2:

                break;
            default:
                break;
        }
    }

    //ѡ��ϸ�� - �������
    public void Compoments_PlayerSettings(int _id)
    {

    }


    //ѡ��ϸ�� - ��������

    //------------------------------------- ���� ------------------------------------------

    //���ڹ���̨����Ϸ�н������ʾ
    //-1Ϊ�ر�
    public void SwitchUI_Player(int _index)
    {
        //������ǹر�
        if (_index != -1)
        {
            if (isPausing == false)
            {
                isPausing = true;
                ToggleMouseVisibilityAndLock(false);
                UIManager[CanvasData.ui���].childs[CanvasData.uiplayer_��������]._object.SetActive(true);
                UIManager[CanvasData.ui���].childs[_index]._object.SetActive(true);
                UIManager[CanvasData.ui���].childs[CanvasData.uiplayer_׼��]._object.SetActive(false);
                managerhub.world.game_state = Game_State.Pause;
            }
        }
        //�ر�UI
        else
        {
            int index = 0;
            foreach (Transform item in UIManager[CanvasData.ui���].childs[CanvasData.uiplayer_��������]._object.transform)
            {
                //���رպ�ɫ����
                if (index == 0)
                {
                    continue;
                }
                item.gameObject.SetActive(false);
                index++;
            }

            // ����������Ϊ���ɼ�
            UIManager[CanvasData.ui���].childs[CanvasData.uiplayer_׼��]._object.SetActive(true);
            UIManager[CanvasData.ui���].childs[CanvasData.uiplayer_��������]._object.SetActive(false);
        }



    }



    //�򿪴浵Ŀ¼
    public void OpenPersistentDataDirectory()
    {
        string savesFolderPath = Path.Combine(managerhub.world.savingPATH, "Saves");

        // ��·���е�������б���滻Ϊ��б��
        string formattedPath = savesFolderPath.Replace("/", "\\");

        //print(formattedPath);

        // �� Windows ��Դ�������������� persistentDataPath
        Process.Start("explorer.exe", formattedPath);
    }



    //���沢�˳�
    public void SaveAndQuitGame()
    {
        //music
        managerhub.NewmusicManager.PlayOneShot(MusicData.click);

        world.ClassifyWorldData();

        StartCoroutine(waittoQuit());
    }

    IEnumerator waittoQuit()
    {
        while (true)
        {
            if (world.isFinishSaving)
            {
                Application.Quit();
            }
            yield return null;
        }



    }


    //ֱ���˳�
    public void JustQuitGame()
    {
        //music
        managerhub.NewmusicManager.PlayOneShot(MusicData.click);

        Application.Quit();
    }


    //���沢�ص�����ҳ��
    public void SaveAndBackToMenu()
    {

        if (waittoFinishSaveAndBackToMenuCoroutine == null)
        {
            waittoFinishSaveAndBackToMenuCoroutine = StartCoroutine(waittoFinishSaveAndBackToMenu());
        }

    }

    public Coroutine waittoFinishSaveAndBackToMenuCoroutine;
    IEnumerator waittoFinishSaveAndBackToMenu()
    {
        world.ClassifyWorldData();

        // �л��� "���ڱ�����" �� UI
        SwitchToUI(CanvasData.ui���ڱ�����);

        // ���������ɺ���1����ӳ٣�����ʵ�����������
        yield return new WaitForSeconds(1f);

        while (true)
        {
            // �ȴ����籣������Լ� updateEditNumberCoroutine Э��ִ�����
            if (world.isFinishSaving && managerhub.world.updateEditNumberCoroutine == null)
            {
                // �л��ز˵�����
                SwitchToUI(CanvasData.ui�˵�);

                // ֹͣ����Э�̲����³�ʼ�����й�����
                StopAllCoroutines();
                InitAllManagers();
                break;
            }

            // ��ͣһ֡���������ռ�� CPU
            yield return null;
        }
    }



    //ȫ����ʼ��
    public void InitAllManagers()
    {
        InitCanvasManager();
        musicmanager.InitMusicManager();
        world.InitWorldManager();
        player.InitPlayerManager();
        BackPackManager.InitBackPackManager();
        LifeManager.InitLifeManager();
        managerhub.debugManager.InitDebugManager();
        InitBackPackSlots();
    }


    //�ޱ���������´�ѡ��ui
    public void SwitchNoBackGround(bool _t)
    {
        NotNeedBackGround = _t;
    }

    //�ر�ϸ��������Ŀ¼
    public void CloseDetail_Child(int _Id)
    {
        UIManager[CanvasData.uiѡ��ϸ��].childs[_Id]._object.SetActive(false);
    }

    //������MuaCherish
    IEnumerator jumpMuaCherish()
    {
        float offset = 0.0f;

        while (true)
        {
            if (managerhub.world.game_state == Game_State.Start)
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
            }


            yield return null;
        }
    }

    //ѡ�����ѡ��ϸ������ʱ�򴫵ݵĲ���
    private int Detail_Number = 0;
    public void UpdateDetail_Number(int _t)
    {
        Detail_Number = _t;
    }

    //����վ
    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    //Fov
    public void ChangeFOV()
    {
        // ��ȡ��������ֵ
        float sliderValue = UIManager[CanvasData.uiѡ��].childs[0]._object.GetComponent<Slider>().value;

        // ����������ֵӳ�䵽FOV�ķ�Χ��
        float newFOV = Mathf.Lerp(50f, 90f, sliderValue);

        // ������ҵ�FOV
        player.CurrentFOV = newFOV;
        player.eyes.fieldOfView = newFOV;
    }


    //-------------------------------------------------------------------------------------------------------------------------------------------------------------







    //------------------------------------- GameMode -----------------------------------------

    //�Ƿ�򿪱���
    public bool isOpenBackpack = false;

    //Survival
    void GameMode_Survival()
    {
        if (hasExec_Playing)
        {
            //ToolBar.SetActive(true);
            //CursorCross_Screen.SetActive(true);
            //Prompt_Screen.SetActive(true);

            openyoureyes();

            //��¼��ʼʱ��
            startTime = Time.time;

            hasExec_Playing = false;
        }


        //EscScreen
        Show_Esc_Screen();

        if (Input.GetKeyDown(KeyCode.E) && !isOpenBackpack && !managerhub.commandManager.isConsoleActive && !managerhub.player.isSpectatorMode)
        {
            isOpenBackpack = true;
            SwitchUI_Player(CanvasData.uiplayer_���汳��);
        }

        //Debug���
        //if (Input.GetKeyDown(KeyCode.F3))
        //{
        //    //Debug_Screen.SetActive(!Debug_Screen.activeSelf);
        //}


        //SwimmingScreen
        if (world.GetBlockType(Camera.transform.position + new Vector3(0f, 0.2f, 0f)) == VoxelData.Water)
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

            //Prompt_Screen.SetActive(true);


            player.isSuperMining = true;


            openyoureyes();


            hasExec_Playing = false;
        }


        //EscScreen
        Show_Esc_Screen();

        if (Input.GetKeyDown(KeyCode.E) && !isOpenBackpack && !managerhub.commandManager.isConsoleActive && !managerhub.player.isSpectatorMode)
        {
            isOpenBackpack = true;
            SwitchUI_Player(CanvasData.uiplayer_���챳��);
            UpdateBackPackButton(0);
            UpdateBackPackSlot(BlockClassfy.ȫ������);
        }


        //SwimmingScreen
        if (world.GetBlockType(Camera.transform.position) == VoxelData.Water)
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


    [Header("���챳��")]
    public GameObject ����ģʽ����;
    public Transform ���챳��Content;
    public GameObject ���챳��blockitem;
    public TextMeshProUGUI ���챳��Text;
    public GameObject ���챳��text_ShowName;
    public Transform ������Ʒ��Content;

    //������Ʒ����ʼ��
    public void InitBackPackSlots()
    {
        foreach (Transform item in ������Ʒ��Content)
        {
            item.gameObject.GetComponent<SlotBlockItem>().InitBlockItem(new BlockItem(255, 0));
            item.gameObject.GetComponent<SlotBlockItem>().UpdateBlockItem(true);
        }
    }

    [System.Serializable]
    public class BlockEntry
    {
        public BlockClassfy _classify; // ���� BlockClassfy ��һ���������л���ö�ٻ���
        public GameObject _button;
    }
    public List<BlockEntry> ����ģʽ����ѡ����;

    //����
    public void UpdateBackPackSlot(BlockClassfy _classfy)
    {
        ���챳��Text.text = _classfy.ToString();

        //clear
        foreach (Transform item in ���챳��Content.transform)
        {
            Destroy(item.gameObject);
        }

        //������
        if (_classfy == BlockClassfy.ȫ������)
        {
            for (byte index = 0; index < managerhub.world.blocktypes.Length; index++)
            {
                CreateBlockItem(new BlockItem(index, 0));
            }
        }

        //����
        else
        {
            for (byte index = 0; index < managerhub.world.blocktypes.Length; index++)
            {
                if (managerhub.world.blocktypes[index].BlockClassfy == _classfy)
                {
                    CreateBlockItem(new BlockItem(index, 0));
                }
            }
        }

    }

    //���ɷ���Item
    public void CreateBlockItem(BlockItem _item)
    {
        //��ʼ��item
        GameObject instance = Instantiate(���챳��blockitem);
        instance.transform.SetParent(���챳��Content, false);
        instance.transform.Find("Image").GetComponent<Image>().color = new Color(1, 1, 1, 200f / 255);

        //�������ʾģʽ
        GameObject icon2D = instance.transform.Find("Icon").gameObject;
        icon2D.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        GameObject icon3D = instance.transform.Find("3Dicon").gameObject;

        //��ʼ���ű�
        instance.GetComponent<SlotBlockItem>().InitBlockItem(new BlockItem(_item._blocktype, _item._number));
        if (_item._number != 0)
        {
            instance.transform.Find("TMP_number").gameObject.GetComponent<TextMeshProUGUI>().text = $"{_item._number}";
        }
        else
        {
            instance.transform.Find("TMP_number").gameObject.GetComponent<TextMeshProUGUI>().text = "";
        }


        //�����±�
        //instance.transform.Find("TMP_index").GetComponent<TextMeshProUGUI>().text = $"{_item._blocktype}";


        //�Ƿ���ʾ3dͼ��
        if (!managerhub.world.blocktypes[_item._blocktype].is2d)
        {
            icon2D.SetActive(false);
            icon3D.SetActive(true);

            instance.transform.Find("3Dicon/up").gameObject.GetComponent<Image>().sprite = managerhub.world.blocktypes[_item._blocktype].top_sprit;
            instance.transform.Find("3Dicon/left").gameObject.GetComponent<Image>().sprite = managerhub.world.blocktypes[_item._blocktype].sprite;
            instance.transform.Find("3Dicon/right").gameObject.GetComponent<Image>().sprite = managerhub.world.blocktypes[_item._blocktype].sprite;
        }
        else
        {
            icon2D.SetActive(true);
            icon3D.SetActive(false);

            instance.transform.Find("Icon").GetComponent<Image>().sprite = managerhub.world.blocktypes[_item._blocktype].icon;
        }
    }


    //���°�ť

    public void UpdateBackPackButton(int _index)
    {
        BlockClassfy _classify = ����ģʽ����ѡ����[_index]._classify;

        UpdateBackPackSlot(_classify);

        foreach (var item in ����ģʽ����ѡ����)
        {
            //����ť����ɫ
            if (item._classify == _classify)
            {
                //color
                item._button.GetComponent<Image>().color = new Color(1, 1, 1, 1);

                //rect
                RectTransform rectTransform = item._button.GetComponent<RectTransform>();
                Vector2 temp = rectTransform.anchoredPosition;
                temp.y = 2; // ���� Y ֵΪ 2
                rectTransform.anchoredPosition = temp; // ���� RectTransform ��λ��
            }
            //������ť�䰵ɫ
            else
            {
                //color
                item._button.GetComponent<Image>().color = new Color(153f / 255, 153f / 255, 153f / 255, 1);

                //rect
                RectTransform rectTransform = item._button.GetComponent<RectTransform>();
                Vector2 temp = rectTransform.anchoredPosition;
                temp.y = -2; // ���� Y ֵΪ 2
                rectTransform.anchoredPosition = temp; // ���� RectTransform ��λ��
            }
        }

    }

    //������ʾBlock����
    public void UpdateText_ShowName(byte _type)
    {
        if (_type == 255)
        {
            ���챳��text_ShowName.GetComponent<TextMeshProUGUI>().text = "";
        }
        else
        {
            ���챳��text_ShowName.GetComponent<TextMeshProUGUI>().text = $"{managerhub.world.blocktypes[_type].blockName} - {_type}";
        }

    }

    

    //���ʽ���ý��
    [Header("���ʽ���ý��")]
    public GameObject SwapBlockPrefeb;
    public SwapBlockStruct SwapBlock = null;

    [SerializeField]
    public class SwapBlockStruct
    {
        public GameObject _object;
        public BlockItem _data = new BlockItem(0, 0);
    }


    //��������Ƿ���swapblock������о��ӳ�ȥ
    void CheckSwapBlockAndDropOut()
    {
        if (SwapBlock != null)
        {
            managerhub.backpackManager.CreateDropBox(managerhub.backpackManager.GetPlayerEyesToThrow(), SwapBlock._data, true);
            DestroySwapBlock();
        }
    }


    //����SwapBlock
    public Transform ����ģʽ����Parent;
    public Transform ����ģʽ����Parent;
    public void CreateSwapBlock(BlockItem _item)
    {
        
        //��ʼ��item
        GameObject instance = Instantiate(SwapBlockPrefeb);
       

        if (managerhub.world.game_mode == GameMode.Survival)
        {
            instance.transform.SetParent(����ģʽ����Parent, false);
            instance.transform.localScale = new Vector3(0.34f, 0.34f, 0.34f);
            instance.transform.SetSiblingIndex(1);
        }
        else
        {
            instance.transform.SetParent(����ģʽ����Parent, false);
            instance.transform.localScale = new Vector3(0.26f, 0.26f, 0.26f);
            instance.transform.SetSiblingIndex(3);
        }

        //instance.transform.Find("Image").GetComponent<Image>().color = new Color(1, 1, 1, 200f / 255);

        //��ʼ������
        SwapBlock = new SwapBlockStruct();
        SwapBlock._object = instance;
        SwapBlock._data = _item;

        //�������ʾģʽ
        GameObject icon2D = instance.transform.Find("Icon").gameObject;
        GameObject icon3D = instance.transform.Find("3Dicon").gameObject;
        icon2D.GetComponent<Image>().raycastTarget = false;

        //��ʼ���ű�
        instance.GetComponent<SwapBlockItem>().InitBlockItem(new BlockItem(_item._blocktype, _item._number));
        if (_item._number != 0 || _item._number != 1)
        {
            instance.transform.Find("TMP_number").gameObject.GetComponent<TextMeshProUGUI>().text = $"{_item._number}";
        }
        else
        {
            instance.transform.Find("TMP_number").gameObject.GetComponent<TextMeshProUGUI>().text = "";
        }


        //�����±�
        //instance.transform.Find("TMP_index").GetComponent<TextMeshProUGUI>().text = $"{_item._blocktype}";


        //�Ƿ���ʾ3dͼ��
        if (!managerhub.world.blocktypes[_item._blocktype].is2d)
        {
            icon2D.SetActive(false);
            icon3D.SetActive(true);

            instance.transform.Find("3Dicon/up").gameObject.GetComponent<Image>().sprite = managerhub.world.blocktypes[_item._blocktype].top_sprit;
            instance.transform.Find("3Dicon/left").gameObject.GetComponent<Image>().sprite = managerhub.world.blocktypes[_item._blocktype].sprite;
            instance.transform.Find("3Dicon/right").gameObject.GetComponent<Image>().sprite = managerhub.world.blocktypes[_item._blocktype].sprite;
        }
        else
        {
            icon2D.SetActive(true);
            icon3D.SetActive(false);

            instance.transform.Find("Icon").GetComponent<Image>().sprite = managerhub.world.blocktypes[_item._blocktype].icon;
        }

    }

    //����SwapBlock
    public void DestroySwapBlock()
    {
        Destroy(SwapBlock._object);
        Cursor.visible = true;
        SwapBlock = null;
    }

    //�������û�㵽slot�����
    [HideInInspector]public bool hasClickedSlot = false;  // ����Ƿ�����slot
    public float maxWaitSeconds = 0.1f;  // �������ȴ�ʱ�䣨��λ���룩


    //�ӳ��ж��Ƿ�����slot
    public void LayintSwapBlock()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && SwapBlock != null)
        {
            // ����Э�̣����߼��ŵ�һ�������ڼ��
            StartCoroutine(ExecuteAfterSeconds());
        }
    }

    private IEnumerator ExecuteAfterSeconds()
    {
        float elapsedTime = 0f;

        // �ȴ�һ��ʱ��
        while (elapsedTime < maxWaitSeconds)
        {
            // �������� slot����ִ���κβ���
            if (hasClickedSlot)
            {
                // print($"���slot�������� {elapsedTime} ��");
                hasClickedSlot = false;  // �����ɺ����ñ��
                yield break;  // ����� slot����ǰ����Э��
            }

            elapsedTime += Time.deltaTime;  // �ۼӾ�����ʱ��
            yield return null;  // �ȴ���һ֡
        }

        // ����Ƿ�����slot
        if (!hasClickedSlot && SwapBlock != null)
        {
            // ������������ʱ����δ��� slot������ swapblock
            //print("δ���slot������swapblock");
            DestroySwapBlock();
        }

        // ���ñ�ǣ��Ա���һ�ε����Ч
        hasClickedSlot = false;
    }




    //----------------------------------------------------------------------------------------






    //----------------------------------- Init_Screen ---------------------------------------


    ////ѡ������ģʽ
    //public void GamemodeToSurvival()
    //{
    //    world.game_mode = GameMode.Survival;
    //    gamemodeTEXT.text = "��ǰ��Ϸģʽ������ģʽ";

    //    //�ı䰴ť��ɫ
    //    SurvivalButtom.GetComponent<Image>().color = new Color(106 / 255f, 115 / 255f, 200 / 255f, 1f);
    //    CreativeButtom.GetComponent<Image>().color = new Color(149 / 255f, 134 / 255f, 119 / 255f, 1f);
    //}

    ////ѡ����ģʽ
    //public void GamemodeToCreative()
    //{
    //    world.game_mode = GameMode.Creative;
    //    gamemodeTEXT.text = "��ǰ��Ϸģʽ������ģʽ";

    //    //�ı䰴ť��ɫ
    //    SurvivalButtom.GetComponent<Image>().color = new Color(149 / 255f, 134 / 255f, 119 / 255f, 1f);
    //    CreativeButtom.GetComponent<Image>().color = new Color(106 / 255f, 115 / 255f, 200 / 255f, 1f);
    //}


    //---------------------------------------------------------------------------------------






    //--------------------------------- Loading_Screen -------------------------------------

    //Loading������ʱ���һ�����۵Ķ���
    void openyoureyes()
    {
        
        StartCoroutine(Animation_Openyoureyes());
    }

    IEnumerator Animation_Openyoureyes()
    {
        OpenYourEyes.SetActive(true);
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
        if (Input.GetKeyDown(KeyCode.Escape) && !managerhub.player.isSpectatorMode)
        {
            // ִ����ͣ
            if (!isPausing)
            {
                isPausing = true;
                SwitchToUI(CanvasData.ui��Ϸ����ͣ);
                world.game_state = Game_State.Pause;

                // ��������Ա�����ͣʱʹ��
                ToggleMouseVisibilityAndLock(false);
            }
            // �����ͣ
            else
            {
                isPausing = false;

                if (isOpenBackpack)
                {
                    isOpenBackpack = false;
                    //print("Esc �رձ���");
                    CheckSwapBlockAndDropOut();
                    //UpdateBackPackButton(0);
                }

                switch (UIBuffer.Peek())
                {
                    // ���������ҽ���
                    case 8:
                        SwitchUI_Player(-1);
                        break;
                    default:
                        SwitchToUI(CanvasData.ui���);
                        break;
                }

                // �����������
                ToggleMouseVisibilityAndLock(true);
                world.game_state = Game_State.Playing;
            }
        }
    }

    //�ص���Ϸ
    public void BackToGame()
    {

        isPausing = !isPausing;


        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        ToggleMouseVisibilityAndLock(true);

        SwitchToUI(CanvasData.ui���);
        world.game_state = Game_State.Playing;
    }





    ////Help����
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


    //�ֵ�Ͳ����ʾ
    void Prompt_FlashLight()
    { 
        if (managerhub.player.isInCave)
        {
            if (hasExec_PromptScreen_isShow == false)
            {
                //First_Prompt_PlayerThe_Flashlight();
                managerhub.commandManager.PrintMessage("<ϵͳ��Ϣ> ���԰�[F]���ֵ�Ͳ", 10f,Color.white);
                hasExec_PromptScreen_isShow = true;
            }
            
        }

        //if (Input.GetKeyDown(KeyCode.F) && hasExec_PromptScreen_isShow)
        //{
        //    if (hasExec_PromptScreen_isHide)
        //    {
        //        StartCoroutine(Hide_Animation_PromptScreen());

        //        hasExec_PromptScreen_isHide = false;
        //    }
        //}

    }

    //public void First_Prompt_PlayerThe_Flashlight()
    //{
    //    //prompt_Text.text = "You can press <F> \r\nto open \"FlashLight\"";
    //    //StartCoroutine(Show_Animation_PromptScreen());
    //    //hasExec_PromptScreen_isShow = true;
    //    //managerhub.commandManager.PrintMessage("You can press <F> \r\nto open \"FlashLight\"",Color.yellow);
    //}

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
    //void UpdatePauseScreenValue()
    //{
    //    //bgm volume
    //    volume_bgm = slider_bgm.value;
    //    if (volume_bgm != previous_bgm)
    //    {
    //        //bgm
    //        musicmanager.Audio_envitonment.volume = Mathf.Lerp(0f, 1f, volume_bgm);

    //        //����previous
    //        previous_bgm = volume_bgm;
    //    }


    //    //sound volume
    //    volume_sound = slider_sound.value;
    //    if (volume_sound != previous_sound)
    //    {
    //        //sound
    //        musicmanager.Audio_player_place.volume = Mathf.Lerp(0f, 1f, volume_sound);
    //        musicmanager.Audio_player_broke.volume = Mathf.Lerp(0f, 1f, volume_sound);
    //        musicmanager.Audio_player_moving.volume = Mathf.Lerp(0f, 1f, volume_sound);
    //        musicmanager.Audio_player_falling.volume = Mathf.Lerp(0f, 1f, volume_sound);
    //        musicmanager.Audio_player_diving.volume = Mathf.Lerp(0f, 1f, volume_sound);
    //        musicmanager.Audio_Click.volume = Mathf.Lerp(0f, 1f, volume_sound);

    //        //����previous
    //        previous_sound = volume_sound;
    //    }

    //    //MouseSensitivity
    //    Mouse_Sensitivity = Mathf.Lerp(1f, 4f, slider_MouseSensitivity.value);

    //    if (Mouse_Sensitivity != previous_Mouse_Sensitivity)
    //    {
    //        //�ı����������

    //        //����previous
    //        previous_Mouse_Sensitivity = Mouse_Sensitivity;
    //    }


    //    //space mode
    //    SpaceMode_isOn = toggle_SpaceMode.isOn;
    //    if (SpaceMode_isOn != previous_spaceMode_isOn)
    //    {
    //        if (SpaceMode_isOn)
    //        {
    //            player.gravity = -3f;
    //            player.isSpaceMode = true;
    //        }
    //        else
    //        {
    //            player.gravity = -20f;
    //            player.isSpaceMode = false;
    //        }

    //        //����previous
    //        previous_spaceMode_isOn = SpaceMode_isOn;
    //    }


    //    //SuperMining
    //    SuperMining_isOn = toggle_SuperMing.isOn;
    //    if (SuperMining_isOn != previous_SuperMining_isOn)
    //    {
    //        if (SuperMining_isOn)
    //        {
    //            player.isSuperMining = true;
    //        }
    //        else
    //        {
    //            player.isSuperMining = false;
    //        }

    //        //����previous
    //        previous_SuperMining_isOn = SuperMining_isOn;
    //    }

    //}


    //---------------------------------------------------------------------------------------





    //---------------------------------- ���������� -----------------------------------------

    /// <summary>
    /// �������
    /// </summary>

    public GameObject Evaporation_Particle;
    public GameObject ParticleParent;
    public void PlayerDead()
    {
        //DeadScreen.SetActive(true);
        SwitchToUI(CanvasData.ui����);

        // ����ʵ������������������Ϊ particleParent
        GameObject deadParticle = GameObject.Instantiate(
            Evaporation_Particle,
            managerhub.player.transform.position,
            Quaternion.LookRotation(Vector3.up),
            ParticleParent.transform  // ���ø�����
        );

        world.game_state = Game_State.Dead;
    }

    public void PlayerClickToRestart()
    {
        //DeadScreen.SetActive(false);
        SwitchToUI(CanvasData.ui���);



        world.game_state = Game_State.Playing;

        LifeManager.blood = 20;
        LifeManager.oxygen = 10;
        LifeManager.UpdatePlayerBlood(0, false, false);
        startTime = Time.time;

        player.InitPlayerLocation();
        player.transform.rotation = Quaternion.identity;

        world.Update_CenterChunks(false);

        world.HideFarChunks();

        //if (managerhub.timeManager.gameObject.activeSelf)
        //{
        //    managerhub.timeManager.Buff_CaveFog(false);
        //}
        



        openyoureyes();
    }
    //--------------------------------------------------------------------------------------






    //------------------------------------ ������ -------------------------------------------

    public GameObject CanvasMainScreen;
    public GameObject SpectatorScreen;

    public void SpectatorMode(bool _open)
    {
        CanvasMainScreen.SetActive(!_open);
        SpectatorScreen.SetActive(_open);
    }


    //void HideCursor()
    //{
    //    // �������������Ļ����
    //    Cursor.lockState = CursorLockMode.Locked;
    //    //��겻����
    //    Cursor.visible = false;

    //    ToggleMouseVisibilityAndLock(true);
    //}

   

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


//UI������
[System.Serializable]
public class CanvasId
{
    public string name;
    public GameObject canvas;
    public List<UIChild> childs;
}

//UI����
[System.Serializable]
public class UIChild
{
    public string name;
    public GameObject _object;
}


//�̶���С��ջ
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

        // ��ʼ��ʱ���һ����ʼԪ��
        stack.Add(initialElement);
    }

    public void Push(T item)
    {
        if (stack.Count >= capacity)
        {
            // �����������Ƴ�����µ�Ԫ��
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

        // ��ȡջ��Ԫ��
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
        // �� List<T> ת��Ϊ����
        return stack.ToArray();
    }
}