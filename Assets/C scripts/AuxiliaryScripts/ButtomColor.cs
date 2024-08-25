using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


//���ѡ��浵��ʱ��ť����
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

        // ���ҳ����е� World �ű�ʵ��
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
        if (world != null)
        {
            // ���� World ����һ�� HandleButtonClick ����
            world.SelectSaving(myPathName);
        }
    }

    private void FixedUpdate()
    {
        if (isPointed && world.PointSaving != myPathName)
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
