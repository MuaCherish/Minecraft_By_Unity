using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{

    [Header("Screen")]
    public GameObject StartScreen;
    public GameObject LoadingScreen;
    public GameObject debugscreen;
    public GameObject EscScreen;
    public GameObject CursorCross;

    [Header("UI")]
    public Slider slider;
    public TextMeshProUGUI tmp;
    public GameObject scrollbar;

    //游戏状态判断
    bool isEscing = false;

    //world 
    public World world;


    //开始界面 -> 加载界面
    public void OnClickEvent()
    {
        //界面显示
        StartScreen.SetActive(false);
        LoadingScreen.SetActive(true);


        world.game_state = Game_State.Loading;
    }

    //加载界面 -> 游戏界面
    private void Update()
    {
        //加载中
        if (world.game_state == Game_State.Loading)
        {
            //Slider
            slider.value = world.initprogress;

            //Text
            tmp.text = $"{(world.initprogress * 100):F2} %";

            //ScrollBar
            float x = Mathf.Lerp(1f, 3.46f, world.initprogress);
            float y = Mathf.Lerp(1f, 3.15f, world.initprogress);
            scrollbar.transform.localScale = new Vector3(x, y, 1f);

            //如果进度条满了
            if (world.initprogress == 1)
            {
                world.game_state = Game_State.Playing;
            }

        }

        //加载完成
        if (world.game_state == Game_State.Playing)
        {
            LoadingScreen.SetActive(false);
            CursorCross.SetActive(true);

            //Debug面板
            if (Input.GetKeyDown(KeyCode.F3))
            {
                debugscreen.SetActive(!debugscreen.activeSelf);
            }

            //EscScreen
            //if (Input.GetKeyDown(KeyCode.Escape))
            //{
            //    isEscing = !isEscing;
            //    //Debug.Log($"{isEscing}");
            //    EscScreen.SetActive(!EscScreen.activeSelf);
            //}

            //QuitGame
            if (Input.GetKeyDown(KeyCode.Q) && isEscing)
            {
                //Debug.Log("isQuiting");
                Application.Quit();
            }

        }
    }


}
