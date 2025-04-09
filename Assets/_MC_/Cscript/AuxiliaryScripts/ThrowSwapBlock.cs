using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ThrowSwapBlock : MonoBehaviour, IPointerDownHandler
{
    private ManagerHub managerhub;

    private void Start()
    {
        managerhub = SceneData.GetManagerhub();
    }

    //当被点击
    public void OnPointerDown(PointerEventData eventData)
    {
        if (managerhub == null)
        {
            managerhub = SceneData.GetManagerhub();
        }
        //print("黑色背景被点击");
        //判断swapblock是否存在
        if (managerhub.canvasManager.SwapBlock != null)
        {
            BackPackManager backpackmanager = managerhub.backpackManager;
            //将swapBlock丢出，并清除swapBlock
            backpackmanager.CreateDropBox(backpackmanager.GetPlayerEyesToThrow(),managerhub.canvasManager.SwapBlock._data, true);

            managerhub.canvasManager.DestroySwapBlock();

        }
    }
}
