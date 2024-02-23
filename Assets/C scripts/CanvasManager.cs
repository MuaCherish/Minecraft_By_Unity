using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{

    [Header("Transform")]
    public Transform Camera;


    //Start && Init
    [Header("Start && Init")]
    public GameObject Start_Screen;

    //Loading
    [Header("Locading")]
    public GameObject Loading_Screen;
    public Slider slider;
    public TextMeshProUGUI tmp;
    public GameObject handle;

    //Playing
    [Header("Playing")]
    public GameObject Playing_Screen;
    public GameObject Debug_Screen;
    public GameObject CursorCross_Screen;
    public GameObject Swimming_Screen;

    //Pause
    [Header("Pause")]
    public GameObject Pause_Screen;


    //��Ϸ״̬�ж�
    public bool isPausing = false;

    //world 
    public World world;


    //��ʼ���� -> ���ؽ���
    public void OnClickEvent()
    {
        //������ʾ
        Start_Screen.SetActive(false);
        Loading_Screen.SetActive(true);
        HideCursor();

        world.game_state = Game_State.Loading;
    }

    //���ؽ��� -> ��Ϸ����
    private void Update()
    {
        //������
        if (world.game_state == Game_State.Loading)
        {
            //Slider
            slider.value = world.initprogress;

            //Text
            tmp.text = $"{(world.initprogress * 100):F2} %";

            //ScrollBar
            float x = Mathf.Lerp(1f, 3.4f, world.initprogress);
            float y = Mathf.Lerp(1f, 3.26f, world.initprogress);
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
            Loading_Screen.SetActive(false);
            Playing_Screen.SetActive(true);
            CursorCross_Screen.SetActive(true);

            //Debug���
            if (Input.GetKeyDown(KeyCode.F3))
            {
                Debug_Screen.SetActive(!Debug_Screen.activeSelf);
            }

            //EscScreen
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isPausing = !isPausing;
                Playing_Screen.SetActive(!Playing_Screen.activeSelf);
                Pause_Screen.SetActive(!Pause_Screen.activeSelf);
            }

            //QuitGame
            if (Input.GetKeyDown(KeyCode.Q) && isPausing)
            {
                //Debug.Log("isQuiting");
                Application.Quit();
            }

            //SwimmingScreen
            if (world.GetBlockType(Camera.transform.position) == VoxelData.Water)
            {
                Swimming_Screen.SetActive(true);
            }
            else
            {
                Swimming_Screen.SetActive(false);
            }

        }
    }

    void HideCursor()
    {
        // �������������Ļ����
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        //��겻����
        UnityEngine.Cursor.visible = false;
    }


}
