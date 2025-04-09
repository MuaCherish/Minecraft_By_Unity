using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


//���ѡ��浵��ʱ��ť����
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
        // ��ȡ Image ���
        image = GetComponent<Image>();

        // ȷ�� Image ����� Default_Sprite ������
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

        // ���ó�ʼ����
        image.sprite = Default_Sprite;

        myPathName = transform.Find("TMP_Time").GetComponent<TextMeshProUGUI>().text;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // �����ͣʱ�ı�����
        if (Select_Sprite != null)
        {
            image.sprite = Select_Sprite;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // ����뿪ʱ�ָ�����
        if (!isPointed && Default_Sprite != null)
        {
            image.sprite = Default_Sprite;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // �������¼�
        //Debug.Log("Button clicked!");
        isPointed = true;

        // �����ʱ�ı����� 
        if (Select_Sprite != null)
        {
            image.sprite = Select_Sprite;
        }

        // ���������������κ�����Ҫ�ڰ�ť���ʱִ�еĴ���
        if (Service_World != null)
        {
            // ���� World ����һ�� HandleButtonClick ����
            managerhub.canvasManager.SelectSaving(myPathName);
        }
    }

    private void FixedUpdate()
    {
        if (isPointed && managerhub.canvasManager.PointSaving != myPathName)
        {
            // ����뿪ʱ�ָ�����
            if (Default_Sprite != null)
            {
                image.sprite = Default_Sprite;
            }
            isPointed = false;
        }
    }
} 
