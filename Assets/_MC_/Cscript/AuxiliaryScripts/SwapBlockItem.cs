using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwapBlockItem : MonoBehaviour
{
    public BlockItem MyItem = new BlockItem(0, 0);
    private RectTransform rectTransform;

    //private ManagerHub managerhub;

    //private void Start()
    //{
    //    managerhub = SceneData.GetManagerhub();
    //}


    //public int debug_type;
    //public int debug_Number;
    private Vector2 offset = new Vector2(50f, -50f); // 偏移量，可在 Inspector 中设置

    private void Awake()
    {
        // 获取 RectTransform 组件
        rectTransform = GetComponent<RectTransform>();
        
    }

    private void Start()
    {
        FollowMouse();
    }

    public void InitBlockItem(BlockItem _item)
    {
        MyItem._blocktype = _item._blocktype;
        MyItem._number = _item._number;
    }

    

    // Update is called once per frame
    void Update() 
    {
        FollowMouse();
        //debug_type = MyItem._blocktype;
        //debug_Number = MyItem._number;
    }

    public void FollowMouse()
    {
        Vector2 mousePosition;
        // 将鼠标位置从屏幕坐标转换为 UI 坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            Input.mousePosition,
            Camera.main,
            out mousePosition
        );
        // 设置 RectTransform 的位置为鼠标位置加上偏移
        rectTransform.anchoredPosition = mousePosition + offset;
    }

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    print("click");
    //    managerhub.canvasManager.isCatchFloatingBlockItem = false;
    //    managerhub.canvasManager.floatingBlockInstance = null;
    //    Destroy(this.gameObject);
    //}
}
