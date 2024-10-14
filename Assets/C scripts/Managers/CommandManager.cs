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

    #region ״̬

    [Foldout("״̬", true)]
    [Header("����̨�Ƿ񼤻�")]
    [ReadOnly]public bool isConsoleActive = false; // ��־λ�����ٿ���̨�ļ���״̬

    #endregion
    

    #region ���ں���

    private ManagerHub managerhub;
    private World world;

    private void Start()
    {
        managerhub = GlobalData.GetManagerhub();
        world = managerhub.world;
    }


    private void Update()
    {
        if (world.game_state == Game_State.Playing || world.game_state == Game_State.Pause)
        {
            // ����T���ҿ���̨δ����ʱ�ż������̨
            if (Input.GetKeyDown(KeyCode.T) && !isConsoleActive && !managerhub.player.isSpectatorMode)
            {
                ActivateConsole();
            }
            // ���»س���ʱ�رտ���̨
            else if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && isConsoleActive)
            {
                DeactivateConsole();
            }
        }

    }


    #endregion


    #region �������

    [Foldout("�������", true)]
    public GameObject CommandScreen;
    //public GameObject ������Ϣ��;
    [Header("�����")]
    public TMP_InputField inputField; // �������������InputField


    private void ActivateConsole()
    {
        inputField.text = "";

        CommandScreen.SetActive(true);
        isConsoleActive = true;

        // ����̨�����˸
        inputField.ActivateInputField();
        // ���ù��λ��Ϊĩβ
        //inputField.caretPosition = inputField.text.Length;
        // �����������
        inputField.readOnly = false;
    }

    private void DeactivateConsole()
    {
        CommandScreen.SetActive(false);
        isConsoleActive = false;

        // �رտ���̨ʱ��ֹ����
        inputField.readOnly = true;
    }

    #endregion


    #region ����ϵͳ

    [Foldout("����ϵͳ", true)]
    [Header("������Ϣ��")]
    public TextMeshProUGUI[] ������Ϣ�� = new TextMeshProUGUI[13];
    [Header("13��������Ϣ")]
    public FixedList<Amessage> AliveMessages = new FixedList<Amessage>(13);
    [Header("��Ϣ����")]
    public float messageLife = 3f;
    private Coroutine CheckMessageLifeCoroutine;

    //�н�����������Ϣϵͳ(��Ӵ�ͷ��, ɾ����β��)
    public class FixedList<T>
    {
        private List<T> _list;
        public int Capacity { get; private set; }

        public FixedList(int capacity)
        {
            Capacity = capacity;
            _list = new List<T>(capacity);
        }

        // ���Ԫ�ص�ͷ��
        public void Add(T item)
        {
            // ����б��������Ƴ���ɵ�Ԫ��
            if (_list.Count >= Capacity)
            {
                _list.RemoveAt(_list.Count - 1);
            }

            // ����Ԫ����ӵ�ͷ��
            _list.Insert(0, item);
        }

        // �����б��е�Ԫ��
        public IEnumerable<T> Items => _list.AsEnumerable();

        // ��ȡ�б�ĵ�ǰԪ������
        public int Count => _list.Count;

        // ����б�
        public void Clear()
        {
            _list.Clear();
        }

        // ��������β��Ԫ��
        public T GetTail()
        {
            if (_list.Count == 0)
            {
                Debug.Log("empty");
                return default(T); // ����Ĭ��ֵ
            }
            return _list[_list.Count - 1];
        }

        // �Ƴ�����β��Ԫ��
        public void RemoveTail()
        {
            // ����б�Ϊ�գ��׳��쳣����Ϊ�����
            if (_list.Count == 0)
            {
                throw new InvalidOperationException("List is empty.");
            }

            // �Ƴ��б�β����Ԫ��
            _list.RemoveAt(_list.Count - 1);
        }

        // ����������
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

    //��Ϣ�ṹ��
    [System.Serializable]
    public class Amessage
    {
        public string content;
        public float life;
        public Color color;

        public Amessage(string _content, float _life)
        {
            content = _content;
            life = _life;
        }

        public Amessage(string _content, float _life, Color _color)
        {
            content = _content;
            life = _life;
            color = _color;
        }

    }


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


        //����Э��
        if (CheckMessageLifeCoroutine == null)
        {
            //print("����Э��");
            CheckMessageLifeCoroutine = StartCoroutine(CheckMessageLife());
        }

        // ����������Ϣ������ʾ
        UpdateMessageScreen();
    }


    //�ص�����
    public void FinishInput()
    {
        //print(inputField.text); // ��ӡ��������

        //����Ϊ�ղ�ִ��
        if (string.IsNullOrEmpty(inputField.text))
        {
            return;
        }

        PrintMessage(CheckCommand(inputField.text,out Color messagecolor), messageLife, messagecolor);


    }

    //ˢ����Ϣ��
    private void UpdateMessageScreen()
    {
        // ��ղ����¸���������Ϣ��
        for (int i = 0; i < ������Ϣ��.Length; i++)
        {
            if (i < AliveMessages.Count)
            {
                ������Ϣ��[i].text = AliveMessages[i].content;
                ������Ϣ��[i].gameObject.SetActive(true);
                ������Ϣ��[i].gameObject.GetComponent<TextMeshProUGUI>().color = AliveMessages[i].color;
            }
            else
            {
                ������Ϣ��[i].gameObject.SetActive(false);
            }
        }
    }


    //��Ϣ��������Э��
    IEnumerator CheckMessageLife()
    {
        while (true)
        {
            //Ϊ�վͽ���
            if (AliveMessages.Count == 0)
            {
                CheckMessageLifeCoroutine = null;
                break;
            }
            else
            {
                //print($"���м��{AliveMessages.Count - 1},��β����{AliveMessages.GetTail().life}");
                if (AliveMessages.GetTail().life <= 0f)
                {
                    //print($"�Ƴ���βԪ��{AliveMessages.Count - 1}");
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




    #endregion


    #region ָ��ϵͳ

    //ָ��ṹ��
    [Serializable]
    public class CommandSystem
    {
        public string name;
        public string command;
    }

    [Foldout("ָ��ϵͳ", true)]
    [Header("ָ�")] public List<CommandSystem> commands = new List<CommandSystem>();
    [Header("ʷ��ķ����")] public GameObject Entity_Slim;

    //ָ�����-ִ�к���
    public String CheckCommand(String _input, out Color _color)
    {

        //����Ƿ���ָ��

        //��ȡͷָ��
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
            return "<MuaCherish> " + _input;
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
                DeactivateConsole();
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
                        if (type < world.blocktypes.Length)
                        {
                            // ���±������ݣ�������� type ����Ϊ number ����Ʒ
                            managerhub.backpackManager.update_slots(0, type, number);
                            //managerhub.backpackManager.ChangeBlockInHand();
                            managerhub.musicManager.PlaySound_Absorb();
                            return "<ϵͳ��Ϣ> " + "������ҷ���";
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

                return "<ϵͳ��Ϣ> " + "��鿴�����ĵ�";

            //addSlim
            case 9:
                
                Entity_Slim.transform.position = managerhub.player.transform.position;
                Entity_Slim.SetActive(true);
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
            

            //û���ҵ�
            default:
                _color = Color.red;
                return "<ϵͳ��Ϣ> " + "ָ���쳣"; 
        }


        
    }

    #endregion


}
