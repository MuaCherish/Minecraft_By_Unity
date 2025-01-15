using Homebrew;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class CommandManager : MonoBehaviour
{

    #region 周期函数

    private ManagerHub managerhub;
    //private World world;

    private void Awake()
    {
        managerhub = GlobalData.GetManagerhub();
    }



    #endregion



    #region 指令系统

    
    [Foldout("指令系统", true)]
    [Header("指令集")] public List<CommandSystem> commands = new List<CommandSystem>();
    [Header("史莱姆引用")] public GameObject Entity_Slim;

    //指令解析-执行函数
    public String CheckCommand(String _input, out Color _color)
    {

        //检查是否是指令

        //提取头指令,如果不是指令则返回
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
            return $"<{managerhub.chatManager.playerName}> " + _input;
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
                managerhub.chatManager.DeactivateConsole();
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
                            //managerhub.backpackManager.ChangeBlockInHand();
                            managerhub.NewmusicManager.PlayOneShot(MusicData.absorb_1);
                            return "<系统消息> " + $"给与玩家方块 [{typeString}]";
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

            // Fog 控制
            case 7:
                string pattern7 = @"\/fog\s+(\d)";

                Match match7 = Regex.Match(_input, pattern7);

                if (match7.Success)
                {
                    string fogSetting = match7.Groups[1].Value;

                    if (int.TryParse(fogSetting, out int fogValue))
                    {
                        if (fogValue == 0)
                        {
                            RenderSettings.fog = false;
                            return "<系统消息> " + "雾效已关闭";
                        }
                        else if (fogValue == 1)
                        {
                            RenderSettings.fog = true;
                            return "<系统消息> " + "雾效已开启";
                        }
                        else
                        {
                            return "<系统消息> " + "无效的雾效设置";
                        }
                    }
                    else
                    {
                        return "<系统消息> " + "雾效转换失败";
                    }
                }
                else
                {
                    _color = Color.red;
                    return "<系统消息> " + "雾效转换失败";
                }

            //help
            case 8:
                managerhub.backpackManager.update_slots(0, VoxelData.Tool_Book);

                //显示指令
                if (Coroutine_ShowCommands == null)
                {
                    Coroutine_ShowCommands = StartCoroutine(ShowAllCommands());
                }
                return "<系统消息> " + "请查看帮助文档";

            //addSlim
            case 9:

                managerhub.world.AddEntity(EntityData.TestSlim, managerhub.player.transform.position);
                
                return "<系统消息> " + "已添加史莱姆";

            // 旁观者模式
            case 10:
                string pattern10 = @"\/spectatormode\s+(\d)";

                Match match10 = Regex.Match(_input, pattern10);

                if (match10.Success)
                {
                    string fogSetting = match10.Groups[1].Value;

                    if (int.TryParse(fogSetting, out int fogValue))
                    {
                        if (fogValue == 0)
                        {
                            managerhub.player.SpectatorMode(false);
                            return "<系统消息> " + "旁观者模式已关闭";
                        }
                        else if (fogValue == 1)
                        {
                            managerhub.player.SpectatorMode(true);
                            return "<系统消息> " + "旁观者模式已开启";
                        }
                        else
                        {
                            _color = Color.red;
                            return "<系统消息> " + "/spectatormode参数错误";
                        }
                    }
                    else
                    {
                        _color = Color.red;
                        return "<系统消息> " + "spectatormode转换失败";
                    }
                }
                else
                {
                    _color = Color.red;
                    return "<系统消息> " + "spectatormode转换失败";
                }

            //poslightcast
            case 11:
                managerhub.sunMoving.isOpenLightCast = !managerhub.sunMoving.isOpenLightCast;

                return "<系统消息> " + "已切换光线后处理";

            //TimeSpeed
            case 12:
                string pattern12 = @"\/timespeed\s+(\d+)";

                // 使用正则表达式匹配数字
                Match match12 = Regex.Match(_input, pattern12);

                if (match12.Success)
                {
                    string numberString = match12.Groups[1].Value;

                    if (float.TryParse(numberString, out float number))
                    {
                        //Debug.Log("提取并转换的数字: " + number);

                        if (number > 0)
                        {
                            managerhub.timeManager.timeStruct._time.second_GameOneHour = number;


                            return "<系统消息> " + $"已更新时间流速";
                        }
                        else
                        {
                            return "<系统消息> " + "流速不可为零或者负数";
                        }
                    }
                    else
                    {
                        return "<系统消息> " + "<number>转换失败";
                    }

                }
                else
                {
                    _color = Color.red;
                    return "<系统消息> " + "<number>转换失败";
                }

            //editname
            case 13:
                string pattern13 = @"\/editname\s+(.*)";  // 匹配/editname 后面的一切内容

                // 使用正则表达式匹配内容
                Match match13 = Regex.Match(_input, pattern13);

                if (match13.Success)
                {
                    string str = match13.Groups[1].Value.Trim();  // 获取后面的内容并去除空格

                    // 如果str为空，则返回名字不可为空
                    if (string.IsNullOrEmpty(str))
                    {
                        _color = Color.red;
                        return "<系统消息> " + "名字不可为空";
                    }
                    else
                    {
                        managerhub.chatManager.playerName = str;
                        _color = Color.green;
                        return "<系统消息> " + "修改成功";
                    }
                }
                else
                {
                    _color = Color.red;
                    return "<系统消息> " + "<name>转换失败";
                }


            //没有找到
            default:
                _color = Color.red;
                return "<系统消息> " + "指令异常"; 
        }


        
    }


    //打印所有指令集
    private Coroutine Coroutine_ShowCommands;
    IEnumerator ShowAllCommands()
    {
        yield return new WaitForSeconds(0.5f);

        //每秒显示显示指令内容
        foreach (var item in commands)
        {

            managerhub.chatManager.PrintMessage($"{item.grammar}：{item.interpretation}", 20f, Color.yellow);
            yield return new WaitForSeconds(0.5f);
 
        }

        Coroutine_ShowCommands = null;

    }

    #endregion


}


//指令结构体
[Serializable]
public class CommandSystem
{
    public string name;
    public string command;
    public string grammar;
    public string interpretation;
}
