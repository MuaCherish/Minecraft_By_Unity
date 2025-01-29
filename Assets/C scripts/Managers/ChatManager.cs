using Homebrew;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{

    #region 状态

    [Foldout("状态", true)]
    [Header("正在输入")][ReadOnly] public bool isInputing = false; // 标志位，跟踪控制台的激活状态
    [Header("玩家名字")] public string playerName = "MuaCherish";

    #endregion


    #region 周期函数

    ManagerHub managerhub;
    void Awake()
    {
        managerhub = SceneData.GetManagerhub();
    }

    private void Start()
    {
        //自动寻找外置消息栏引用
        Init_GetOutMessage_Gameobject();

        //初始化消息容器
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


    #region 输入窗口

    
    [Foldout("引用", true)]
    [Header("输入窗口")] public GameObject InputScreen;
    [Header("输入框")] public TMP_InputField inputField; // 用于输入命令的InputField


    void SwitchChatScreen()
    {
        // 按下T键且控制台未激活时才激活控制台
        if (Input.GetKeyDown(KeyCode.T) && !isInputing && !managerhub.player.isSpectatorMode)
        {
            UsefulFunction.LockMouse(false);

            ActivateConsole();
            
            InsideMessage_GameObject.SetActive(true);
            OutSideMessage_GameObject.SetActive(false);
        }
        // 按下回车键时关闭控制台
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

        inputField.ActivateInputField();// 控制台光标闪烁
        inputField.readOnly = false;// 允许键盘输入


    }

    public void DeactivateConsole()
    {
        InputScreen.SetActive(false);
        isInputing = false;

        // 关闭控制台时禁止输入
        inputField.readOnly = true;
    }

    #endregion


    #region 外置消息栏

    //外置消息栏设置
    [Foldout("外置消息栏", true)]
    public FixedList<Amessage> AliveMessages;
    [Header("父类")] public GameObject OutSideMessage_GameObject;
    [Header("消息生命")] public float OutSideMessage_FloatingTime = 10f;
    [Header("Content")] public GameObject OutSideMessage_Content;
    GameObject[] OutSideMessage_ReferGameObject;

    //获取外置消息栏引用
    void Init_GetOutMessage_Gameobject()
    {
        if (OutSideMessage_Content == null)
        {
            Debug.LogError("OutMessage_Content 未设置！");
            return;
        }

        // 获取 OutMessage_Content 下的所有子物体
        int childCount = OutSideMessage_Content.transform.childCount;

        // 初始化数组大小为子物体数量
        OutSideMessage_ReferGameObject = new GameObject[childCount];

        // 将所有子物体存入数组
        for (int i = 0; i < childCount; i++)
        {
            OutSideMessage_ReferGameObject[i] = OutSideMessage_Content.transform.GetChild(i).gameObject;
        }

        //Debug.Log($"成功获取 {childCount} 个子物体引用。");
    }

    //消息生命周期协程
    private Coroutine CheckMessageLifeCoroutine;
    IEnumerator CheckMessageLife()
    {
        //数据处理
        while (true)
        {
            //提前返回-生命消息为空就结束
            if (AliveMessages.Count == 0)
            {
                CheckMessageLifeCoroutine = null;
                break;
            }


            //print($"例行检查{AliveMessages.Count - 1},队尾生命{AliveMessages.GetTail().life}");
            if (AliveMessages.GetTail().life <= 0f)
            {
                //print($"移除队尾元素{AliveMessages.Count - 1}");
                AliveMessages.RemoveTail();

                //渲染
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


    //刷新消息栏
    private void UpdateMessageScreen()
    {

        // 清空并重新更新外置消息栏
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


    #region 内置消息栏

    //内置消息栏设置
    [Foldout("内置消息栏", true)]
    [Header("父类")] public GameObject InsideMessage_GameObject;
    [Header("Content")] public GameObject InsideMessage_Content;
    [Header("聊天背景")] public GameObject InsideMessage_ChatBackGround;
    [Header("最多显示多少条消息")] public int MaxMessageNumber = 30;
    [Header("Amessage预制体")] public GameObject Prefeb_Amessage;


    //内置消息栏添加消息
    void InsideMessage_Add(string _Info, Color _color)
    {
        // 在InsideMessage_Content下实例化Amessage预制体
        GameObject messageInstance = Instantiate(Prefeb_Amessage, InsideMessage_Content.transform);

        // 获取该实例中的TextMeshProUGUI组件
        TextMeshProUGUI messageText = messageInstance.transform.Find("TMP_info").GetComponent<TextMeshProUGUI>();

        // 设置文本和颜色
        messageText.text = _Info;
        messageText.color = _color;

        // 如果超出了最大消息数量，销毁最早的消息
        if (InsideMessage_Content.transform.childCount > MaxMessageNumber)
        {
            Destroy(InsideMessage_Content.transform.GetChild(0).gameObject);
        }
    }



    #endregion


    #region 聊天系统


    /// <summary>
    /// 发送消息，并控制生命周期
    /// </summary>
    /// <param name="_Info"></param>
    /// <param name="_time"></param>
    /// <param name="_color"></param>
    public void PrintMessage(String _Info, float _time, Color _color)
    {

        //填充指令
        AliveMessages.Add(new Amessage(_Info, _time, _color));


        //启动浮动消息协程
        if (CheckMessageLifeCoroutine == null)
            CheckMessageLifeCoroutine = StartCoroutine(CheckMessageLife());

        // 更新外置消息栏的显示
        UpdateMessageScreen();


        //添加内部消息栏
        InsideMessage_Add(_Info,_color);


    }


    /// <summary>
    /// 回调函数，无需修改
    /// </summary>
    public void FinishInput()
    {
        OutSideMessage_GameObject.SetActive(true);
        InsideMessage_GameObject.SetActive(false);
        //print(inputField.text); // 打印输入内容

        //输入为空不执行
        if (string.IsNullOrEmpty(inputField.text))
        {
            return;
        }

        PrintMessage(managerhub.commandManager.CheckCommand(inputField.text, out Color messagecolor), OutSideMessage_FloatingTime, messagecolor);

    }


    #endregion

}
