using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


//鼠标选择存档的时候按钮发光
public class ButtomColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Sprite Default_Sprite;
    public Sprite Select_Sprite;
    public World world;

    private Image image;

    public string myPathName;

    public bool isPointed = false;

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

        // 查找场景中的 World 脚本实例
        if (world == null)
        {
            world = FindObjectOfType<World>();
            if (world == null) 
            {
                Debug.LogError("World script not found in the scene.");
            }
        }


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
        if (world != null)
        {
            // 假设 World 类有一个 HandleButtonClick 方法
            world.SelectSaving(myPathName);
        }
    }

    private void FixedUpdate()
    {
        if (isPointed && world.PointSaving != myPathName)
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
