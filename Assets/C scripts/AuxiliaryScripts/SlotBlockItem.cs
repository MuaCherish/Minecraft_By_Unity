using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotBlockItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public bool isBackPackSlot = false;  //是物品栏
    public BlockItem MyItem = new BlockItem(255, 0);
    private ManagerHub managerhub;

    private void Start()
    {
        managerhub = GlobalData.GetManagerhub();
    }


    //public int debug_type;
    //public int debug_Number;
    //private void Update()
    //{
    //    debug_type = MyItem._blocktype;
    //    debug_Number = MyItem._number;
    //}

    // 初始化
    public void InitBlockItem(BlockItem _item)
    {
        MyItem._blocktype = _item._blocktype;
        MyItem._number = _item._number;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (MyItem._number != 255)
        {
            managerhub.canvasManager.UpdateText_ShowName(MyItem._blocktype);
        }
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (MyItem._number != 255)
        {
            managerhub.canvasManager.UpdateText_ShowName(255);
        }
        
    }

    //如果被点击，尝试触发SwapBlock
    public void OnPointerDown(PointerEventData eventData)
    {
        //print("slot被点击");

        //拿出物品
        if (managerhub.canvasManager.SwapBlock == null)
        {
            //print("拿出物品");
            if (isBackPackSlot)
            {
                if (MyItem._number != 0)
                {
                    
                    managerhub.canvasManager.CreateSwapBlock(new BlockItem(MyItem._blocktype, MyItem._number));
                    Cursor.visible = false;
                    InitMyItem();
                }
                UpdateBlockItem(true);
                
            }
            else
            {
                
                managerhub.canvasManager.CreateSwapBlock(new BlockItem(MyItem._blocktype, 1));
                Cursor.visible = false;
            }
            

        } 
        
        //放入物品
        else
        {
            //print("放入物品");
            if (isBackPackSlot)
            {
                //print("slot被点击");
                managerhub.canvasManager.hasClickedSlot = true;
                

                //执行交换SwapBlock的逻辑              
                //相同累加数字
                if (MyItem._blocktype == managerhub.canvasManager.SwapBlock._data._blocktype)
                {
                    MyItem._number += managerhub.canvasManager.SwapBlock._data._number;
                }
                //不同则改变类型
                else
                {
                    MyItem._blocktype = managerhub.canvasManager.SwapBlock._data._blocktype;
                    MyItem._number = managerhub.canvasManager.SwapBlock._data._number;
                }

                UpdateBlockItem(true);

                //Destroy
                managerhub.canvasManager.DestroySwapBlock();
            }

            Cursor.visible = true;
        }


        
    }

    /// <summary>
    /// 刷新背包物品栏
    /// </summary>
    /// <param name="_needSYN">是否需要同步物品栏</param>
    public void UpdateBlockItem(bool _needSYN)
    {
        //transform.Find("Image").GetComponent<Image>().color = new Color(1, 1, 1, 200f / 255);

        //方块的显示模式
        GameObject icon2D = transform.Find("Icon").gameObject;
        GameObject icon3D = transform.Find("3Dicon").gameObject;
        //print($"正在渲染Slot，number: {MyItem._number}");

        if (managerhub == null)
        {
            managerhub = GlobalData.GetManagerhub();
        }
        //print(MyItem._blocktype + "-" + MyItem._number);

        //如果类型是空的则不渲染
        if (MyItem._blocktype == 255)
        {
            //icon
            icon2D.SetActive(false);
            icon3D.SetActive(false);

            //number
            transform.Find("TMP_number").gameObject.GetComponent<TextMeshProUGUI>().text = "";
        }

        //可以渲染
        else
        {
            //数量不为0
            if (MyItem._number != 0)
            {
                transform.Find("TMP_number").gameObject.GetComponent<TextMeshProUGUI>().text = $"{MyItem._number}";

                //print(MyItem._blocktype);
                //是否显示3d图形
                if (!managerhub.world.blocktypes[MyItem._blocktype].is2d)
                {
                    icon2D.SetActive(false);
                    icon3D.SetActive(true);

                    transform.Find("3Dicon/up").gameObject.GetComponent<Image>().sprite = managerhub.world.blocktypes[MyItem._blocktype].top_sprit;
                    transform.Find("3Dicon/left").gameObject.GetComponent<Image>().sprite = managerhub.world.blocktypes[MyItem._blocktype].sprite;
                    transform.Find("3Dicon/right").gameObject.GetComponent<Image>().sprite = managerhub.world.blocktypes[MyItem._blocktype].sprite;
                }
                else
                {
                    icon2D.SetActive(true);
                    icon2D.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    icon3D.SetActive(false);

                    transform.Find("Icon").GetComponent<Image>().sprite = managerhub.world.blocktypes[MyItem._blocktype].icon;
                }


            }
            else
            {
                //icon
                icon2D.SetActive(true);
                icon2D.GetComponent<Image>().color = new Color(139f / 255, 139f / 255, 139f / 255, 1);
                icon3D.SetActive(false);

                //number
                transform.Find("TMP_number").gameObject.GetComponent<TextMeshProUGUI>().text = "";
            }
        }

        if (_needSYN)
        {
            managerhub.backpackManager.SYN_allSlots(1);
        }
        
    }

    void InitMyItem()
    {
        MyItem._blocktype = 255;
        MyItem._number = 0;
    }


}

[SerializeField]
public class BlockItem
{
    public byte _blocktype = 0;
    public int _number = 0;

    public BlockItem(byte _a, int _b)
    {
        _blocktype = _a;
        _number = _b;
    }
}
