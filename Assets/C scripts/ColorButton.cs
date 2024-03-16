using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ColorButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image background;

    public void OnPointerEnter(PointerEventData eventData)
    {
        background.color = Color.white;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        background.color = Color.black;
    }

}
