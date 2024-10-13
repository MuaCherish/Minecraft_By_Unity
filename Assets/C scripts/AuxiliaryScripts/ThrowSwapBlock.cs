using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ThrowSwapBlock : MonoBehaviour, IPointerDownHandler
{
    private ManagerHub managerhub;

    private void Start()
    {
        managerhub = GlobalData.GetManagerhub();
    }

    //�������
    public void OnPointerDown(PointerEventData eventData)
    {
        if (managerhub == null)
        {
            managerhub = GlobalData.GetManagerhub();
        }

        //�ж�swapblock�Ƿ����
        if (managerhub.canvasManager.SwapBlock != null)
        {
            BackPackManager backpackmanager = managerhub.backpackManager;
            //��swapBlock�����������swapBlock
            backpackmanager.CreateDropBox(backpackmanager.GetPlayerEyesToThrow(),managerhub.canvasManager.SwapBlock._data, true);

            managerhub.canvasManager.DestroySwapBlock();

        }
    }
}
