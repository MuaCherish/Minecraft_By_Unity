using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{

    public GameObject StartScreen;
    public GameObject LoadingScreen;
    public GameObject CursorCross;
    public Slider slider;
    public TextMeshProUGUI tmp;
    public GameObject scrollbar;

    public bool isLoading = false;
    public bool isGamePlaying = false;

    //world
    //public GameObject worldObject;
    public World world;
    [HideInInspector]
    public bool OnclickToInitMap = false;

    private void Start()
    {
        //world = worldObject.GetComponent<World>();
    }


    //当点击开始游戏时
    public void OnClickEvent()
    {
        StartScreen.SetActive(false);
        LoadingScreen.SetActive(true);
        OnclickToInitMap = true;
        isLoading = true;
        //world.StartToUnitMap();
    }




    //当加载完成时
    private void Update()
    {
        if (isLoading)
        {
            //Slider
            slider.value = world.initprogress;

            //Text
            tmp.text = $"{(world.initprogress * 100):F2} %";

            //ScrollBar
            //X:1~3.46
            //Y:1~3.15
            float x = Mathf.Lerp(1f,3.46f,world.initprogress);
            float y = Mathf.Lerp(1f,3.15f, world.initprogress);
            scrollbar.transform.localScale = new Vector3(x,y,1f);

            if (world.initprogress == 1)
            {
                isLoading = false;

                isGamePlaying = true;

            }

        }

        if (isGamePlaying)
        {
            LoadingScreen.SetActive(false);
            CursorCross.SetActive(true);
        }
    }


}
