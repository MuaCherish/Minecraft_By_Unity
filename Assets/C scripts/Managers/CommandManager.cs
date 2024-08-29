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
    //public GameObject ������Ϣ��;
    public TMP_InputField inputField; // �������������InputField
    

    [Header("״̬")]
    public bool isConsoleActive = false; // ��־λ�����ٿ���̨�ļ���״̬

    [Header("����")]
    public List<CommandSystem> commands = new List<CommandSystem>();


    //----------------------------- �������� -----------------------------------------


    private void Update()
    {
        if (world.game_state == Game_State.Playing)
        {
            // ����T���ҿ���̨δ����ʱ�ż������̨
            if (Input.GetKeyDown(KeyCode.T) && !isConsoleActive)
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


    //------------------------------ ��Ϣϵͳ ------------------------------------------------

    public TextMeshProUGUI[] ������Ϣ�� = new TextMeshProUGUI[13];
    public FixedList<Amessage> AliveMessages = new FixedList<Amessage>(13);
    public float messageLife = 3f;

    //�ص�����
    public void FinishInput()
    {
        //print(inputField.text); // ��ӡ��������

        //����Ϊ�ղ�ִ��
        if (string.IsNullOrEmpty(inputField.text))
        {
            return;
        }


        //���ָ��
        AliveMessages.Add(new Amessage(CheckCommand(inputField.text, out Color messagecolor), messageLife, messagecolor));


        //����Э��
        if (CheckMessageLifeCoroutine == null)
        {
            //print("����Э��");
            CheckMessageLifeCoroutine = StartCoroutine(CheckMessageLife());
        }

        // ����������Ϣ������ʾ
        UpdateMessageScreen();
    }


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
            //��ӡ
            case 0:

            // ��ȡ����
            return "<ϵͳ��Ϣ> " + _input.Substring(6).Trim(); // ��ȡ "/print " ��������ݲ�ȥ����β�ո�


            case 1:
                //world.ClassifyWorldData();
                _color = Color.red;
                return "<ϵͳ��Ϣ> " + "��ָ����ͣ��"; 


            case 2:
                lifemanager.UpdatePlayerBlood(30, true, true);
                DeactivateConsole();
                return "<ϵͳ��Ϣ> " + "���������";
                 
            case 3:
                world.LoadAllSaves(world.savingPATH + "\\Saves");
                return "<ϵͳ��Ϣ> " + "���ڳ��Զ�ȡ�浵";

            case 4:
                string pattern = @"\/give\s+(\d+)";

                // ʹ��������ʽƥ������
                Match match = Regex.Match(_input, pattern);

                if (match.Success)
                {
                    string numberString = match.Groups[1].Value;

                    if (byte.TryParse(numberString, out byte number))
                    {
                        //Debug.Log("��ȡ��ת��������: " + number);

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
                            
                            return "<ϵͳ��Ϣ> " + "������ҷ���";
                        }
                        else
                        {
                            return "<ϵͳ��Ϣ> " + "����id������";
                        }
                    }
                    else
                    {
                        return "<ϵͳ��Ϣ> " + "idת��ʧ��";
                    }
                    
                }
                else
                {
                    _color = Color.red;
                    return "<ϵͳ��Ϣ> " + "idת��ʧ��";
                } 
                

            //�����������ָ��----------------------------
            //û���ҵ�
            default:
                _color = Color.red;
                return "<ϵͳ��Ϣ> " + "ָ���쳣"; 
        }


        
    }


    //ˢ��ui
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
    private Coroutine CheckMessageLifeCoroutine;
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





}


//ָ�
[System.Serializable]
public class CommandSystem
{
    public string name;
    public string command;
}

//�н�����
//��Ӵ�ͷ��
//ɾ����β��
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