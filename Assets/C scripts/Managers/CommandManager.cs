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
    [Header("状态")]
    [ReadOnly]public bool isConsoleActive = false; // 标志位，跟踪控制台的激活状态

    [Header("Transforms")]
    public ManagerHub managerhub;
    public GameObject CommandScreen;
    //public GameObject 内置消息栏;
    public TMP_InputField inputField; // 用于输入命令的InputField
    

    [Header("参数")]
    public List<CommandSystem> commands = new List<CommandSystem>();


    //----------------------------- 命令面板的 -----------------------------------------


    private void Update()
    {
        if (managerhub.world.game_state == Game_State.Playing)
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


    public void PrintMessage(String _Info, float _time, Color _color)
    {
        //填充指令
        AliveMessages.Add(new Amessage(_Info, _time, _color));


        //启动协程
        if (CheckMessageLifeCoroutine == null)
        {
            //print("启动协程");
            CheckMessageLifeCoroutine = StartCoroutine(CheckMessageLife());
        }

        // 更新外置消息栏的显示
        UpdateMessageScreen();
    }


    //回调函数
    public void FinishInput()
    {
        //print(inputField.text); // 打印输入内容

        //输入为空不执行
        if (string.IsNullOrEmpty(inputField.text))
        {
            return;
        }

        PrintMessage(CheckCommand(inputField.text,out Color messagecolor), messageLife, messagecolor);


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
            

            //print
            case 0:

            // 提取参数
            return "<系统消息> " + _input.Substring(6).Trim(); // 提取 "/print " 后面的内容并去除首尾空格

            //save
            case 1:
                //world.ClassifyWorldData();
                _color = Color.red;
                return "<系统消息> " + "该指令已停用"; 

            //kill
            case 2:
                managerhub.lifeManager.UpdatePlayerBlood(30, true, true);
                DeactivateConsole();
                return "<系统消息> " + "玩家已死亡";
                 
            //load
            case 3:
                //world.LoadAllSaves(world.savingPATH + "\\Saves");
                _color = Color.red;
                return "<系统消息> " + "该指令已停用";

            //give
            case 4:
                // 修改正则表达式，使第二个参数可选
                string pattern = @"\/give\s+(\d+)(?:\s+(\d+))?";

                // 使用正则表达式匹配
                Match match = Regex.Match(_input, pattern);

                if (match.Success)
                {
                    // 提取 type 参数
                    string typeString = match.Groups[1].Value;

                    // 如果第二个参数不存在，则默认值为 1
                    string numberString = match.Groups[2].Success ? match.Groups[2].Value : "1";

                    // 尝试将 type 转换为 byte 类型
                    if (byte.TryParse(typeString, out byte type) && int.TryParse(numberString, out int number))
                    {
                        //Debug.Log("提取并转换的类型和数量: " + type + ", " + number);

                        // 判断 type 是否在 blocktypes 范围内
                        if (type < managerhub.world.blocktypes.Length)
                        {
                            // 更新背包内容，例如插入 type 数量为 number 的物品
                            managerhub.backpackManager.update_slots(0, type, number);
                            managerhub.backpackManager.ChangeBlockInHand();
                            managerhub.musicManager.PlaySound_Absorb();
                            return "<系统消息> " + "给与玩家方块";
                        }
                        else
                        {
                            return "<系统消息> " + "方块 id 不存在";
                        }
                    }
                    else
                    {
                        return "<系统消息> " + "类型或数量转换失败";
                    }
                }
                else
                {
                    _color = Color.red;
                    return "<系统消息> " + "指令格式错误";
                }


            //time
            case 5:
                string pattern5 = @"\/time\s+(\d+)";

                // 使用正则表达式匹配数字
                Match match5 = Regex.Match(_input, pattern5);

                if (match5.Success)
                {
                    string numberString = match5.Groups[1].Value;

                    if (byte.TryParse(numberString, out byte number))
                    {
                        //Debug.Log("提取并转换的数字: " + number);

                        if (number >= 0 && number <= 24)
                        {

                            managerhub.timeManager.SetTime(number);

                            return "<系统消息> " + $"已将时间更新至{number}时";
                        }
                        else
                        {
                            return "<系统消息> " + "时间必须为24小时制";
                        }
                    }
                    else
                    {
                        return "<系统消息> " + "time转换失败";
                    }

                }
                else
                {
                    _color = Color.red;
                    return "<系统消息> " + "time转换失败";
                }

            //fps
            case 6:
                string pattern6 = @"\/fps\s+(\d+)";

                // 使用正则表达式匹配数字
                Match match6 = Regex.Match(_input, pattern6);

                if (match6.Success)
                {
                    string numberString = match6.Groups[1].Value;

                    if (byte.TryParse(numberString, out byte number))
                    {
                        //Debug.Log("提取并转换的数字: " + number);

                        if (number >= 0)
                        {
                            if (number == 0)
                            {
                                Application.targetFrameRate = -1;
                            }
                            else
                            {
                                Application.targetFrameRate = number;
                            }
                            

                            return "<系统消息> " + $"已将帧数更新至{number}时";
                        }
                        else
                        {
                            return "<系统消息> " + "帧数不可为负数, 0为解除帧数限制";
                        }
                    }
                    else
                    {
                        return "<系统消息> " + "fps转换失败";
                    }

                }
                else
                {
                    _color = Color.red;
                    return "<系统消息> " + "fps转换失败";
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