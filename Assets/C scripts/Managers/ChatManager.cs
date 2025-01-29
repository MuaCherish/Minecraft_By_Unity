using Homebrew;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{

    #region ״̬

    [Foldout("״̬", true)]
    [Header("��������")][ReadOnly] public bool isInputing = false; // ��־λ�����ٿ���̨�ļ���״̬
    [Header("�������")] public string playerName = "MuaCherish";

    #endregion


    #region ���ں���

    ManagerHub managerhub;
    void Awake()
    {
        managerhub = SceneData.GetManagerhub();
    }

    private void Start()
    {
        //�Զ�Ѱ��������Ϣ������
        Init_GetOutMessage_Gameobject();

        //��ʼ����Ϣ����
        AliveMessages = new FixedList<Amessage>(MaxMessageNumber);
    }


    void Update()
    {
        switch (managerhub.world.game_state)
        {
            case Game_State.Playing:
                SwitchChatScreen();
                break;

            case Game_State.Pause:
                SwitchChatScreen();
                break;
        }
    }




    #endregion


    #region ���봰��

    
    [Foldout("����", true)]
    [Header("���봰��")] public GameObject InputScreen;
    [Header("�����")] public TMP_InputField inputField; // �������������InputField


    void SwitchChatScreen()
    {
        // ����T���ҿ���̨δ����ʱ�ż������̨
        if (Input.GetKeyDown(KeyCode.T) && !isInputing && !managerhub.player.isSpectatorMode)
        {
            UsefulFunction.LockMouse(false);

            ActivateConsole();
            
            InsideMessage_GameObject.SetActive(true);
            OutSideMessage_GameObject.SetActive(false);
        }
        // ���»س���ʱ�رտ���̨
        else if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && isInputing)
        {
            UsefulFunction.LockMouse(true);

            FinishInput();
            DeactivateConsole();
        }
    }


    private void ActivateConsole()
    {
        inputField.text = "";

        InputScreen.SetActive(true);
        isInputing = true;

        inputField.ActivateInputField();// ����̨�����˸
        inputField.readOnly = false;// �����������


    }

    public void DeactivateConsole()
    {
        InputScreen.SetActive(false);
        isInputing = false;

        // �رտ���̨ʱ��ֹ����
        inputField.readOnly = true;
    }

    #endregion


    #region ������Ϣ��

    //������Ϣ������
    [Foldout("������Ϣ��", true)]
    public FixedList<Amessage> AliveMessages;
    [Header("����")] public GameObject OutSideMessage_GameObject;
    [Header("��Ϣ����")] public float OutSideMessage_FloatingTime = 10f;
    [Header("Content")] public GameObject OutSideMessage_Content;
    GameObject[] OutSideMessage_ReferGameObject;

    //��ȡ������Ϣ������
    void Init_GetOutMessage_Gameobject()
    {
        if (OutSideMessage_Content == null)
        {
            Debug.LogError("OutMessage_Content δ���ã�");
            return;
        }

        // ��ȡ OutMessage_Content �µ�����������
        int childCount = OutSideMessage_Content.transform.childCount;

        // ��ʼ�������СΪ����������
        OutSideMessage_ReferGameObject = new GameObject[childCount];

        // �������������������
        for (int i = 0; i < childCount; i++)
        {
            OutSideMessage_ReferGameObject[i] = OutSideMessage_Content.transform.GetChild(i).gameObject;
        }

        //Debug.Log($"�ɹ���ȡ {childCount} �����������á�");
    }

    //��Ϣ��������Э��
    private Coroutine CheckMessageLifeCoroutine;
    IEnumerator CheckMessageLife()
    {
        //���ݴ���
        while (true)
        {
            //��ǰ����-������ϢΪ�վͽ���
            if (AliveMessages.Count == 0)
            {
                CheckMessageLifeCoroutine = null;
                break;
            }


            //print($"���м��{AliveMessages.Count - 1},��β����{AliveMessages.GetTail().life}");
            if (AliveMessages.GetTail().life <= 0f)
            {
                //print($"�Ƴ���βԪ��{AliveMessages.Count - 1}");
                AliveMessages.RemoveTail();

                //��Ⱦ
                UpdateMessageScreen();
            }


            for (int i = 0; i < AliveMessages.Count; i++)
            {
                if (AliveMessages[i].life > 0f)
                {
                    AliveMessages[i].life--;
                }

            }


            yield return new WaitForSeconds(1f);
        }




    }


    //ˢ����Ϣ��
    private void UpdateMessageScreen()
    {

        // ��ղ����¸���������Ϣ��
        for (int i = 0; i < OutSideMessage_ReferGameObject.Length; i++)
        {
            if (i < AliveMessages.Count)
            {
                OutSideMessage_ReferGameObject[i].gameObject.SetActive(true);

                TextMeshProUGUI TempText = OutSideMessage_ReferGameObject[i].transform.Find("TMP_info").GetComponent<TextMeshProUGUI>();
                TempText.text = AliveMessages[i].content;
                TempText.color = AliveMessages[i].color;
            }
            else
            {
                OutSideMessage_ReferGameObject[i].gameObject.SetActive(false);
            }
        }
    }


    #endregion


    #region ������Ϣ��

    //������Ϣ������
    [Foldout("������Ϣ��", true)]
    [Header("����")] public GameObject InsideMessage_GameObject;
    [Header("Content")] public GameObject InsideMessage_Content;
    [Header("���챳��")] public GameObject InsideMessage_ChatBackGround;
    [Header("�����ʾ��������Ϣ")] public int MaxMessageNumber = 30;
    [Header("AmessageԤ����")] public GameObject Prefeb_Amessage;


    //������Ϣ�������Ϣ
    void InsideMessage_Add(string _Info, Color _color)
    {
        // ��InsideMessage_Content��ʵ����AmessageԤ����
        GameObject messageInstance = Instantiate(Prefeb_Amessage, InsideMessage_Content.transform);

        // ��ȡ��ʵ���е�TextMeshProUGUI���
        TextMeshProUGUI messageText = messageInstance.transform.Find("TMP_info").GetComponent<TextMeshProUGUI>();

        // �����ı�����ɫ
        messageText.text = _Info;
        messageText.color = _color;

        // ��������������Ϣ�����������������Ϣ
        if (InsideMessage_Content.transform.childCount > MaxMessageNumber)
        {
            Destroy(InsideMessage_Content.transform.GetChild(0).gameObject);
        }
    }



    #endregion


    #region ����ϵͳ


    /// <summary>
    /// ������Ϣ����������������
    /// </summary>
    /// <param name="_Info"></param>
    /// <param name="_time"></param>
    /// <param name="_color"></param>
    public void PrintMessage(String _Info, float _time, Color _color)
    {

        //���ָ��
        AliveMessages.Add(new Amessage(_Info, _time, _color));


        //����������ϢЭ��
        if (CheckMessageLifeCoroutine == null)
            CheckMessageLifeCoroutine = StartCoroutine(CheckMessageLife());

        // ����������Ϣ������ʾ
        UpdateMessageScreen();


        //����ڲ���Ϣ��
        InsideMessage_Add(_Info,_color);


    }


    /// <summary>
    /// �ص������������޸�
    /// </summary>
    public void FinishInput()
    {
        OutSideMessage_GameObject.SetActive(true);
        InsideMessage_GameObject.SetActive(false);
        //print(inputField.text); // ��ӡ��������

        //����Ϊ�ղ�ִ��
        if (string.IsNullOrEmpty(inputField.text))
        {
            return;
        }

        PrintMessage(managerhub.commandManager.CheckCommand(inputField.text, out Color messagecolor), OutSideMessage_FloatingTime, messagecolor);

    }


    #endregion

}
