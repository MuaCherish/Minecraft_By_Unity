using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


//鼠标选择存档的时候按钮发光
public class ButtomColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Sprite Default_Sprite;
    public Sprite Select_Sprite;
    MC_Service_World Service_World;
    ManagerHub managerhub;

    private Image image;

    public string myPathName;

    public bool isPointed = false;

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        Service_World = managerhub.Service_World;
    }

    void Start()
    {
        // 获取 Image 组件
        image = GetComponent<Image>();

        // 确保 Image 组件和 Default_Sprite 被设置
        if (image == null)
        {
            Debug.LogError("Image component not found on this GameObject.");
            return;
        }
        if (Default_Sprite == null)
        {
            Debug.LogError("Default_Sprite is not assigned.");
            return;
        }

        // 设置初始纹理
        image.sprite = Default_Sprite;

        myPathName = transform.Find("TMP_Time").GetComponent<TextMeshProUGUI>().text;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 鼠标悬停时改变纹理
        if (Select_Sprite != null)
        {
            image.sprite = Select_Sprite;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 鼠标离开时恢复纹理
        if (!isPointed && Default_Sprite != null)
        {
            image.sprite = Default_Sprite;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 处理点击事件
        //Debug.Log("Button clicked!");
        isPointed = true;

        // 鼠标点击时改变纹理 
        if (Select_Sprite != null)
        {
            image.sprite = Select_Sprite;
        }

        // 你可以在这里添加任何你想要在按钮点击时执行的代码
        if (Service_World != null)
        {
            // 假设 World 类有一个 HandleButtonClick 方法
            managerhub.canvasManager.SelectSaving(myPathName);
        }
    }

    private void FixedUpdate()
    {
        if (isPointed && managerhub.canvasManager.PointSaving != myPathName)
        {
            // 鼠标离开时恢复纹理
            if (Default_Sprite != null)
            {
                image.sprite = Default_Sprite;
            }
            isPointed = false;
        }
    }
} 
