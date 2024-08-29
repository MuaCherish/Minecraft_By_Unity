using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
//using System.Windows.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//using static UnityEngine.Rendering.DebugUI;

public class CommandManager : MonoBehaviour
{
    [Header("Transforms")]
    public World world;
    public Player player;
    public BackPackManager backpackmanager;
    public GameObject CommandScreen;
    public LifeManager lifemanager;
    //public GameObject 内置消息栏;
    public TMP_InputField inputField; // 用于输入命令的InputField
    

    [Header("状态")]
    public bool isConsoleActive = false; // 标志位，跟踪控制台的激活状态

    [Header("参数")]
    public List<CommandSystem> commands = new List<CommandSystem>();


    //----------------------------- 命令面板的 -----------------------------------------


    private void Update()
    {
        if (world.game_state == Game_State.Playing)
        {
            // 按下T键且控制台未激活时才激活控制台
            if (Input.GetKeyDown(KeyCode.T) && !isConsoleActive)
            {
                ActivateConsole();
            }
            // 按下回车键时关闭控制台
            else if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && isConsoleActive)
            {
                DeactivateConsole();
            }
        }

    }

    private void ActivateConsole()
    {
        inputField.text = "";

        CommandScreen.SetActive(true);
        isConsoleActive = true;

        // 控制台光标闪烁
        inputField.ActivateInputField();
        // 设置光标位置为末尾
        //inputField.caretPosition = inputField.text.Length;
        // 允许键盘输入
        inputField.readOnly = false;
    }

    private void DeactivateConsole()
    {
        CommandScreen.SetActive(false);
        isConsoleActive = false;

        // 关闭控制台时禁止输入
        inputField.readOnly = true;
    }


    //------------------------------ 消息系统 ------------------------------------------------

    public TextMeshProUGUI[] 外置消息栏 = new TextMeshProUGUI[13];
    public FixedList<Amessage> AliveMessages = new FixedList<Amessage>(13);
    public float messageLife = 3f;

    //回调函数
    public void FinishInput()
    {
        //print(inputField.text); // 打印输入内容

        //输入为空不执行
        if (string.IsNullOrEmpty(inputField.text))
        {
            return;
        }


        //填充指令
        AliveMessages.Add(new Amessage(CheckCommand(inputField.text, out Color messagecolor), messageLife, messagecolor));


        //启动协程
        if (CheckMessageLifeCoroutine == null)
        {
            //print("启动协程");
            CheckMessageLifeCoroutine = StartCoroutine(CheckMessageLife());
        }

        // 更新外置消息栏的显示
        UpdateMessageScreen();
    }


    //指令解析-执行函数
    public String CheckCommand(String _input, out Color _color)
    {

        //检查是否是指令

        //提取头指令
        string commandFirst = "";
        if (_input.StartsWith("/"))
        {
            // 查找第一个空格的位置
            int spaceIndex = _input.IndexOf(' ');

            if (spaceIndex != -1)
            {
                // 提取从 '/' 到第一个空格之间的部分
                commandFirst = _input.Substring(0, spaceIndex);
            }
            else
            {
                // 如果没有空格，则提取从 '/' 到结束的部分
                commandFirst = _input;
            }
            _color = Color.green;
        }
        else
        {
            //不是指令
            _color = Color.white;
            return "<MuaCherish> " + _input;
        }


        //根据头指令获取指令集的下标
        int index = 0;
        foreach (var StandardCommand in commands)
        {
            if (commandFirst == StandardCommand.command)
            {
                break;
            }
            index++;
        }

        //执行指令
        switch (index)
        {
            //打印
            case 0:

            // 提取参数
            return "<系统消息> " + _input.Substring(6).Trim(); // 提取 "/print " 后面的内容并去除首尾空格


            case 1:
                //world.ClassifyWorldData();
                _color = Color.red;
                return "<系统消息> " + "该指令已停用"; 


            case 2:
                lifemanager.UpdatePlayerBlood(30, true, true);
                DeactivateConsole();
                return "<系统消息> " + "玩家已死亡";
                 
            case 3:
                world.LoadAllSaves(world.savingPATH + "\\Saves");
                return "<系统消息> " + "正在尝试读取存档";

            case 4:
                string pattern = @"\/give\s+(\d+)";

                // 使用正则表达式匹配数字
                Match match = Regex.Match(_input, pattern);

                if (match.Success)
                {
                    string numberString = match.Groups[1].Value;

                    if (byte.TryParse(numberString, out byte number))
                    {
                        //Debug.Log("提取并转换的数字: " + number);

                        if (number < world.blocktypes.Length)
                        {

                            if (world.game_mode == GameMode.Creative)
                            {

                                backpackmanager.update_slots(0, number);
                            }
                            else
                            {
                                backpackmanager.update_slots(0, number);
                            }
                            
                            return "<系统消息> " + "给与玩家方块";
                        }
                        else
                        {
                            return "<系统消息> " + "方块id不存在";
                        }
                    }
                    else
                    {
                        return "<系统消息> " + "id转换失败";
                    }
                    
                }
                else
                {
                    _color = Color.red;
                    return "<系统消息> " + "id转换失败";
                } 
                

            //在这里添加新指令----------------------------
            //没有找到
            default:
                _color = Color.red;
                return "<系统消息> " + "指令异常"; 
        }


        
    }


    //刷新ui
    private void UpdateMessageScreen()
    {
        // 清空并重新更新外置消息栏
        for (int i = 0; i < 外置消息栏.Length; i++)
        {
            if (i < AliveMessages.Count)
            {
                外置消息栏[i].text = AliveMessages[i].content;
                外置消息栏[i].gameObject.SetActive(true);
                外置消息栏[i].gameObject.GetComponent<TextMeshProUGUI>().color = AliveMessages[i].color;
            }
            else
            {
                外置消息栏[i].gameObject.SetActive(false);
            }
        }
    }




    //消息生命周期协程
    private Coroutine CheckMessageLifeCoroutine;
    IEnumerator CheckMessageLife()
    {
        while (true)
        {
             //为空就结束
            if (AliveMessages.Count == 0)
            {
                CheckMessageLifeCoroutine = null;
                break;
            }
            else
            {
                //print($"例行检查{AliveMessages.Count - 1},队尾生命{AliveMessages.GetTail().life}");
                if (AliveMessages.GetTail().life <= 0f)
                {
                    //print($"移除队尾元素{AliveMessages.Count - 1}");
                    AliveMessages.RemoveTail();
                    UpdateMessageScreen();
                }


                for (int i = 0; i < AliveMessages.Count; i++)
                {
                    if (AliveMessages[i].life > 0f)
                    {
                        AliveMessages[i].life--;
                    }

                }
            }
             

            yield return new WaitForSeconds(1f); 
        }
    }





}


//指令集
[System.Serializable]
public class CommandSystem
{
    public string name;
    public string command;
}

//有界链表
//添加从头部
//删除从尾部
public class FixedList<T>
{
    private List<T> _list;
    public int Capacity { get; private set; }

    public FixedList(int capacity)
    {
        Capacity = capacity;
        _list = new List<T>(capacity);
    }

    // 添加元素到头部
    public void Add(T item)
    {
        // 如果列表已满，移除最旧的元素
        if (_list.Count >= Capacity)
        {
            _list.RemoveAt(_list.Count - 1);
        }

        // 将新元素添加到头部
        _list.Insert(0, item);
    }

    // 访问列表中的元素
    public IEnumerable<T> Items => _list.AsEnumerable();

    // 获取列表的当前元素数量
    public int Count => _list.Count;

    // 清空列表
    public void Clear()
    {
        _list.Clear();
    }

    // 返回链表尾部元素
    public T GetTail()
    {
        if (_list.Count == 0)
        {
            Debug.Log("empty");
            return default(T); // 返回默认值
        }
        return _list[_list.Count - 1];
    }

    // 移除链表尾部元素
    public void RemoveTail()
    {
        // 如果列表为空，抛出异常或处理为空情况
        if (_list.Count == 0)
        {
            throw new InvalidOperationException("List is empty.");
        }

        // 移除列表尾部的元素
        _list.RemoveAt(_list.Count - 1);
    }

    // 索引访问器
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _list.Count)
            {
                throw new ArgumentOutOfRangeException("Index is out of range.");
            }
            return _list[index];
        }
    }
}

//消息结构体
[System.Serializable]
public class Amessage 
{
    public string content;
    public float life;
    public Color color;

    public Amessage(string _content,float _life)
    {
        content = _content;
        life = _life;
    }

    public Amessage(string _content, float _life,Color _color)
    {
        content = _content;
        life = _life;
        color = _color;
    }

}