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

    #region ���ں���

    private ManagerHub managerhub;
    //private World world;

    private void Awake()
    {
        managerhub = GlobalData.GetManagerhub();
    }



    #endregion



    #region ָ��ϵͳ

    
    [Foldout("ָ��ϵͳ", true)]
    [Header("ָ�")] public List<CommandSystem> commands = new List<CommandSystem>();
    [Header("ʷ��ķ����")] public GameObject Entity_Slim;

    //ָ�����-ִ�к���
    public String CheckCommand(String _input, out Color _color)
    {

        //����Ƿ���ָ��

        //��ȡͷָ��,�������ָ���򷵻�
        string commandFirst = "";
        if (_input.StartsWith("/"))
        {
            // ���ҵ�һ���ո��λ��
            int spaceIndex = _input.IndexOf(' ');

            if (spaceIndex != -1)
            {
                // ��ȡ�� '/' ����һ���ո�֮��Ĳ���
                commandFirst = _input.Substring(0, spaceIndex);
            }
            else
            {
                // ���û�пո�����ȡ�� '/' �������Ĳ���
                commandFirst = _input;
            }
            _color = Color.green;
        }
        else
        {
            //����ָ��
            _color = Color.white;
            return $"<{managerhub.chatManager.playerName}> " + _input;
        }


        //����ͷָ���ȡָ����±�
        int index = 0;
        foreach (var StandardCommand in commands)
        {
            if (commandFirst == StandardCommand.command)
            {
                break;
            }
            index++;
        }

        //ִ��ָ��
        switch (index)
        {
            

            //print
            case 0:

            // ��ȡ����
            return "<ϵͳ��Ϣ> " + _input.Substring(6).Trim(); // ��ȡ "/print " ��������ݲ�ȥ����β�ո�

            //save
            case 1:
                //world.ClassifyWorldData();
                _color = Color.red;
                return "<ϵͳ��Ϣ> " + "��ָ����ͣ��"; 

            //kill
            case 2:
                managerhub.lifeManager.UpdatePlayerBlood(30, true, true);
                managerhub.chatManager.DeactivateConsole();
                return "<ϵͳ��Ϣ> " + "���������";
                 
            //load
            case 3:
                //world.LoadAllSaves(world.savingPATH + "\\Saves");
                _color = Color.red;
                return "<ϵͳ��Ϣ> " + "��ָ����ͣ��";

            //give
            case 4:
                // �޸�������ʽ��ʹ�ڶ���������ѡ
                string pattern = @"\/give\s+(\d+)(?:\s+(\d+))?";

                // ʹ��������ʽƥ��
                Match match = Regex.Match(_input, pattern);

                if (match.Success)
                {
                    // ��ȡ type ����
                    string typeString = match.Groups[1].Value;

                    // ����ڶ������������ڣ���Ĭ��ֵΪ 1
                    string numberString = match.Groups[2].Success ? match.Groups[2].Value : "1";

                    // ���Խ� type ת��Ϊ byte ����
                    if (byte.TryParse(typeString, out byte type) && int.TryParse(numberString, out int number))
                    {
                        //Debug.Log("��ȡ��ת�������ͺ�����: " + type + ", " + number);

                        // �ж� type �Ƿ��� blocktypes ��Χ��
                        if (type < managerhub.world.blocktypes.Length)
                        {
                            // ���±������ݣ�������� type ����Ϊ number ����Ʒ
                            managerhub.backpackManager.update_slots(0, type, number);
                            //managerhub.backpackManager.ChangeBlockInHand();
                            managerhub.NewmusicManager.PlayOneShot(MusicData.absorb_1);
                            return "<ϵͳ��Ϣ> " + $"������ҷ��� [{typeString}]";
                        }
                        else
                        {
                            return "<ϵͳ��Ϣ> " + "���� id ������";
                        }
                    }
                    else
                    {
                        return "<ϵͳ��Ϣ> " + "���ͻ�����ת��ʧ��";
                    }
                }
                else
                {
                    _color = Color.red;
                    return "<ϵͳ��Ϣ> " + "ָ���ʽ����";
                }

            //time
            case 5:
                string pattern5 = @"\/time\s+(\d+)";

                // ʹ��������ʽƥ������
                Match match5 = Regex.Match(_input, pattern5);

                if (match5.Success)
                {
                    string numberString = match5.Groups[1].Value;

                    if (byte.TryParse(numberString, out byte number))
                    {
                        //Debug.Log("��ȡ��ת��������: " + number);

                        if (number >= 0 && number <= 24)
                        {

                            managerhub.timeManager.SetTime(number);

                            return "<ϵͳ��Ϣ> " + $"�ѽ�ʱ�������{number}ʱ";
                        }
                        else
                        {
                            return "<ϵͳ��Ϣ> " + "ʱ�����Ϊ24Сʱ��";
                        }
                    }
                    else
                    {
                        return "<ϵͳ��Ϣ> " + "timeת��ʧ��";
                    }

                }
                else
                {
                    _color = Color.red;
                    return "<ϵͳ��Ϣ> " + "timeת��ʧ��";
                }

            //fps
            case 6:
                string pattern6 = @"\/fps\s+(\d+)";

                // ʹ��������ʽƥ������
                Match match6 = Regex.Match(_input, pattern6);

                if (match6.Success)
                {
                    string numberString = match6.Groups[1].Value;

                    if (byte.TryParse(numberString, out byte number))
                    {
                        //Debug.Log("��ȡ��ת��������: " + number);

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
                            

                            return "<ϵͳ��Ϣ> " + $"�ѽ�֡��������{number}ʱ";
                        }
                        else
                        {
                            return "<ϵͳ��Ϣ> " + "֡������Ϊ����, 0Ϊ���֡������";
                        }
                    }
                    else
                    {
                        return "<ϵͳ��Ϣ> " + "fpsת��ʧ��";
                    }

                }
                else
                {
                    _color = Color.red;
                    return "<ϵͳ��Ϣ> " + "fpsת��ʧ��";
                }

            // Fog ����
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
                            return "<ϵͳ��Ϣ> " + "��Ч�ѹر�";
                        }
                        else if (fogValue == 1)
                        {
                            RenderSettings.fog = true;
                            return "<ϵͳ��Ϣ> " + "��Ч�ѿ���";
                        }
                        else
                        {
                            return "<ϵͳ��Ϣ> " + "��Ч����Ч����";
                        }
                    }
                    else
                    {
                        return "<ϵͳ��Ϣ> " + "��Чת��ʧ��";
                    }
                }
                else
                {
                    _color = Color.red;
                    return "<ϵͳ��Ϣ> " + "��Чת��ʧ��";
                }

            //help
            case 8:
                managerhub.backpackManager.update_slots(0, VoxelData.Tool_Book);

                //��ʾָ��
                if (Coroutine_ShowCommands == null)
                {
                    Coroutine_ShowCommands = StartCoroutine(ShowAllCommands());
                }
                return "<ϵͳ��Ϣ> " + "��鿴�����ĵ�";

            //addSlim
            case 9:

                managerhub.world.AddEntity(EntityData.TestSlim, managerhub.player.transform.position);
                
                return "<ϵͳ��Ϣ> " + "�����ʷ��ķ";

            // �Թ���ģʽ
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
                            return "<ϵͳ��Ϣ> " + "�Թ���ģʽ�ѹر�";
                        }
                        else if (fogValue == 1)
                        {
                            managerhub.player.SpectatorMode(true);
                            return "<ϵͳ��Ϣ> " + "�Թ���ģʽ�ѿ���";
                        }
                        else
                        {
                            _color = Color.red;
                            return "<ϵͳ��Ϣ> " + "/spectatormode��������";
                        }
                    }
                    else
                    {
                        _color = Color.red;
                        return "<ϵͳ��Ϣ> " + "spectatormodeת��ʧ��";
                    }
                }
                else
                {
                    _color = Color.red;
                    return "<ϵͳ��Ϣ> " + "spectatormodeת��ʧ��";
                }

            //poslightcast
            case 11:
                managerhub.sunMoving.isOpenLightCast = !managerhub.sunMoving.isOpenLightCast;

                return "<ϵͳ��Ϣ> " + "���л����ߺ���";

            //TimeSpeed
            case 12:
                string pattern12 = @"\/timespeed\s+(\d+)";

                // ʹ��������ʽƥ������
                Match match12 = Regex.Match(_input, pattern12);

                if (match12.Success)
                {
                    string numberString = match12.Groups[1].Value;

                    if (float.TryParse(numberString, out float number))
                    {
                        //Debug.Log("��ȡ��ת��������: " + number);

                        if (number > 0)
                        {
                            managerhub.timeManager.timeStruct._time.second_GameOneHour = number;


                            return "<ϵͳ��Ϣ> " + $"�Ѹ���ʱ������";
                        }
                        else
                        {
                            return "<ϵͳ��Ϣ> " + "���ٲ���Ϊ����߸���";
                        }
                    }
                    else
                    {
                        return "<ϵͳ��Ϣ> " + "<number>ת��ʧ��";
                    }

                }
                else
                {
                    _color = Color.red;
                    return "<ϵͳ��Ϣ> " + "<number>ת��ʧ��";
                }

            //editname
            case 13:
                string pattern13 = @"\/editname\s+(.*)";  // ƥ��/editname �����һ������

                // ʹ��������ʽƥ������
                Match match13 = Regex.Match(_input, pattern13);

                if (match13.Success)
                {
                    string str = match13.Groups[1].Value.Trim();  // ��ȡ��������ݲ�ȥ���ո�

                    // ���strΪ�գ��򷵻����ֲ���Ϊ��
                    if (string.IsNullOrEmpty(str))
                    {
                        _color = Color.red;
                        return "<ϵͳ��Ϣ> " + "���ֲ���Ϊ��";
                    }
                    else
                    {
                        managerhub.chatManager.playerName = str;
                        _color = Color.green;
                        return "<ϵͳ��Ϣ> " + "�޸ĳɹ�";
                    }
                }
                else
                {
                    _color = Color.red;
                    return "<ϵͳ��Ϣ> " + "<name>ת��ʧ��";
                }


            //û���ҵ�
            default:
                _color = Color.red;
                return "<ϵͳ��Ϣ> " + "ָ���쳣"; 
        }


        
    }


    //��ӡ����ָ�
    private Coroutine Coroutine_ShowCommands;
    IEnumerator ShowAllCommands()
    {
        yield return new WaitForSeconds(0.5f);

        //ÿ����ʾ��ʾָ������
        foreach (var item in commands)
        {

            managerhub.chatManager.PrintMessage($"{item.grammar}��{item.interpretation}", 20f, Color.yellow);
            yield return new WaitForSeconds(0.5f);
 
        }

        Coroutine_ShowCommands = null;

    }

    #endregion


}


//ָ��ṹ��
[Serializable]
public class CommandSystem
{
    public string name;
    public string command;
    public string grammar;
    public string interpretation;
}
