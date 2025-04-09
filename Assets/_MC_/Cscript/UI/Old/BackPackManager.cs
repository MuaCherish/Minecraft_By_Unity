using Homebrew;
using MCEntity;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class BackPackManager : MonoBehaviour
{

    #region ״̬

    [Foldout("״̬", true)]
    [Header("�л��������ȴ��")][ReadOnly] public bool isChanging = false; 

    #endregion


    #region ���ں���

    ManagerHub managerhub;

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
    }


    private void Update()
    {
        
    }


    //��ʼ��Manager
    public void InitBackPackManager()
    {
        foreach (var _slot in slots)
        {
            _slot.ResetSlot();
        }
        ClearDropBlocks();

        ChangeBlockInHand();
    }

    #endregion


    #region �漰��UI����

    public Slot[] slots = new Slot[9];
    public Transform ���汳����Ʒ��;
    public Transform ���챳����Ʒ��;


    //����ȥ����ģʽ������Ʒ������Ʒ�Ž�ȥ
    public Transform ����ģʽ������Ʒ��;
    void AddBlockToBackPackSlots(BlockItem _targetItem)
    {
        if (MC_Runtime_DynamicData.instance.GetGameMode() == GameMode.Survival)
        {
            foreach (Transform item in ����ģʽ������Ʒ��)
            {
                SlotBlockItem slotScript = item.GetComponent<SlotBlockItem>();

                if (slotScript.MyItem._blocktype == _targetItem._blocktype || slotScript.MyItem._blocktype == 255)
                {
                    //print("�����һ����Ʒ");
                    slotScript.AddBlock(_targetItem);
                    break;
                }
            }
        }

    }


    /// <summary>
    /// ��Ⱦ��Ʒ����ͬ��������Ʒ��
    /// </summary>
    public void Render_BackPackSlots(bool _NeedSYN)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            byte _type = slots[i].blockId;

            if (_type != 255)
            {
                //״̬
                slots[i].ishave = true;

                //Icon
                if (managerhub.Service_World.blocktypes[_type].is2d == false) //3d
                {
                    slots[i].Icon3Dobject.SetActive(true);
                    slots[i].TopFace.sprite = managerhub.Service_World.blocktypes[_type].top_sprit;
                    slots[i].LeftFace.sprite = managerhub.Service_World.blocktypes[_type].sprite;
                    slots[i].RightFace.sprite = managerhub.Service_World.blocktypes[_type].sprite;

                    slots[i].icon.color = new Color(1f, 1f, 1f, 0f);
                }
                else
                {
                    slots[i].Icon3Dobject.SetActive(false);
                    slots[i].icon.sprite = managerhub.Service_World.blocktypes[_type].icon;
                    slots[i].icon.color = new Color(1f, 1f, 1f, 1f);
                }

                //number
                slots[i].TMP_number.text = $"{slots[i].number}";

            }
            //����Ⱦ
            else
            {
                slots[i].ResetSlot();
            }
        }

        if (_NeedSYN)
        {
            SYN_allSlots(0);
        }

    }


    #endregion


    #region ��Ʒ��


    /// <summary>
    /// ����ͬ����Ʒ���ͱ�����Ʒ��
    /// <para>[0]: ���±�����Ʒ��</para>
    /// <para>[1]: ������Ʒ��</para>
    /// </summary>
    /// <param name="_prior"></param>
    public void SYN_allSlots(int _prior)
    {

        Transform ������Ʒ��;
        if (MC_Runtime_DynamicData.instance.GetGameMode() == GameMode.Survival)
        {
            ������Ʒ�� = ���汳����Ʒ��;
        }
        else
        {
            ������Ʒ�� = ���챳����Ʒ��;
        }


        //ͬ��������Ʒ��
        if (_prior == 0)
        {
            GameObject[] ������Ʒ������ = new GameObject[slots.Length];
            int index_������Ʒ�� = 0;

            //��䱳����Ʒ������
            foreach (Transform item in ������Ʒ��)
            {
                ������Ʒ������[index_������Ʒ��] = item.gameObject;

                index_������Ʒ��++;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                ������Ʒ������[i].GetComponent<SlotBlockItem>().InitBlockItem(new BlockItem(slots[i].blockId, slots[i].number));
                ������Ʒ������[i].GetComponent<SlotBlockItem>().UpdateBlockItem(false);
            }
        }

        //ͬ����Ʒ��
        else if (_prior == 1)
        {
            //print("��ͬ����Ʒ��");
            int i = 0;
            foreach (Transform item in ������Ʒ��)
            {
                BlockItem _targetItem = item.GetComponent<SlotBlockItem>().MyItem;
                slots[i].blockId = _targetItem._blocktype;
                slots[i].number = _targetItem._number;
                i++;
            }
            Render_BackPackSlots(false);
            ChangeBlockInHand();
        }
        else
        {
            print("_prior����������");
        }



    }

    /// <summary>
    /// ������Ʒ������
    /// <para>[0]: ����++ </para>
    /// <para>[1]: ����--</para>
    /// </summary>
    /// <param name="brokeOrplace">�������ͣ����û��ƻ�</param>
    /// <param name="blocktype">��������</param>
    public void update_slots(int brokeOrplace, byte blocktype)
    {
        //broke,����++
        if (brokeOrplace == 0)
        {
            bool isfind = false;

            //��Ѱ���Ƿ��и÷���
            foreach (Slot _slot in slots)
            {

                //����ҵ�
                if (_slot.blockId == blocktype)
                {
                    isfind = true;

                    _slot.number++;
                }
            }

            //���û�и÷��飬���¼�
            if (!isfind)
            {
                int _index = find_empty_index();
                if (_index != -1)
                {
                    //parameter
                    slots[_index].ishave = true;
                    slots[_index].blockId = blocktype;

                    //number
                    slots[_index].number++;


                }

            }

        }


        //place������--
        else if (brokeOrplace == 1)
        {
            if (slots[managerhub.player.selectindex].number - 1 <= 0)
            {
                slots[managerhub.player.selectindex].ResetSlot();
            }
            else
            {
                slots[managerhub.player.selectindex].number--;
            }

        }

        Render_BackPackSlots(true);
        ChangeBlockInHand();
    }

    /// <summary>
    /// ������Ʒ������
    /// <para>[0]: ����++ </para>
    /// <para>[1]: ����--</para>
    /// </summary>
    /// <param name="brokeOrplace">�������ͣ����û��ƻ�</param>
    /// <param name="blocktype">��������</param>
    /// <param name="_number">���µ�����</param>
    public void update_slots(int brokeOrplace, byte blocktype, int _number)
    {
        //broke,����++
        if (brokeOrplace == 0)
        {
            bool isfind = false;

            //��Ѱ���Ƿ��и÷���
            foreach (Slot _slot in slots)
            {

                //����ҵ�
                if (_slot.blockId == blocktype)
                {
                    isfind = true;

                    _slot.number += _number;
                }
            }

            //���û�и÷��飬���¼�
            if (!isfind)
            {
                int _index = find_empty_index();
                if (_index != -1)
                {
                    //parameter
                    slots[_index].ishave = true;
                    slots[_index].blockId = blocktype;

                    //number
                    slots[_index].number += _number;


                }

                //��Ʒ���������ˣ����ñ�����Ʒ��
                else if (_index == -1)
                {
                    //print("��ӵ�������Ʒ��");
                    AddBlockToBackPackSlots(new BlockItem(blocktype, _number));
                }

            }


            Render_BackPackSlots(true);
            ChangeBlockInHand();

        }


        //place������--
        else if (brokeOrplace == 1)
        {
            if (slots[managerhub.player.selectindex].number - 1 <= 0)
            {
                slots[managerhub.player.selectindex].ResetSlot();
            }
            else
            {
                if (slots[managerhub.player.selectindex].number > _number)
                {
                    slots[managerhub.player.selectindex].number -= _number;
                }
                else
                {
                    slots[managerhub.player.selectindex].number = 0;
                }

            }

        }
    }


    #endregion


    #region �����ﲿ��

    [Header("���������")]
    public float dropblock_destroyTime = 100f;
    public float absorb_Distance = 2.3f;
    public float drop_gravity = 4f;
    public float moveToplayer_duation = 0.2f;
    public float throwForce = 3f;


    /// <summary>
    /// ���������(����,����)
    /// </summary>
    /// <param name="_pos">����ΪInt���꣬����ҪCenter����</param>
    /// <param name="_InitItem"></param>
    /// <param name="_needThrow"></param>
    public GameObject NewDropBlock;
    public void CreateDropBox(Vector3 _pos, BlockItem _InitItem, bool _needThrow)
    {
        Transform Eyes = managerhub.player.GetEyesPosition();

        //ˢ��ƫ��
        float x_offset = UnityEngine.Random.Range(2, 8) / 10f;
        float y_offset = UnityEngine.Random.Range(5, 8) / 10f;
        float z_offset = UnityEngine.Random.Range(2, 8) / 10f;

        //��������
        GameObject DropBlock = GameObject.Instantiate(NewDropBlock);
        DropBlock.transform.SetParent(SceneData.GetDropBlockParent().transform);


        //�Ƿ��ӳ�ȥ
        if (_needThrow)
        {
            Vector3 _position = Eyes.position;
            DropBlock.GetComponent<DropBlock>().OnStartEntity(_position, _InitItem, false);

            Vector3 direct = Eyes.forward; direct.y = 0.5f;
            DropBlock.GetComponent<MC_Component_Velocity>().AddForce(direct, throwForce);
        }
        else
        {
            Vector3 _p = new Vector3(_pos.x + x_offset, _pos.y + y_offset, _pos.z + z_offset);
            DropBlock.GetComponent<DropBlock>().OnStartEntity(_p, _InitItem, true);
        }


    }



    /// <summary>
    /// ��Q����Ʒ
    /// </summary>
    /// 

    //��ȡ��׼ȷ������۾�������
    public Vector3 GetPlayerEyesToThrow()
    {
        Transform Eyes = managerhub.player.GetEyesPosition();
        Vector3 _ThrowOrigin = Eyes.transform.forward * 0.3f + new Vector3(Eyes.transform.position.x, Eyes.transform.position.y - 0.3f, Eyes.transform.position.z);
        return _ThrowOrigin;
    }

    public void ThrowDropBox()
    {
        Transform Eyes = managerhub.player.GetEyesPosition();
        Vector3 _ThrowOrigin = Eyes.transform.forward * 0.3f + new Vector3(Eyes.transform.position.x, Eyes.transform.position.y - 0.3f, Eyes.transform.position.z);

        //�ж��Ƿ����ӵ�����
        if (slots[managerhub.player.selectindex].blockId != 255 && slots[managerhub.player.selectindex].number > 0)
        {
            //���������
            CreateDropBox(_ThrowOrigin, new BlockItem(slots[managerhub.player.selectindex].blockId, 1), true);

            //��Ʒ����һ
            update_slots(1, 0);

            ChangeBlockInHand();
        }


    }

    void ClearDropBlocks()
    {
        Transform dropblockParent = GameObject.Find("Environment/DropBlocks").transform;

        foreach (Transform item in dropblockParent)
        {
            Destroy(item.gameObject);
        }
    }



    #endregion


    #region �л����鲿��

    [Header("�ı�������Ʒ")]
    private byte previous_HandBlock = 255;   //255�����ֱ���
    public float ChangeColdTime = 1f;

    //�л�������ȴʱ��
    IEnumerator ChangeBlockColdTime()
    {
        isChanging = true;

        yield return new WaitForSeconds(ChangeColdTime);

        isChanging = false;
    }

    //�л���Ʒ
    bool ToolAlive = false;
    public void ChangeBlockInHand()
    {

        GameObject Hand_Hold = managerhub.player.hand_Hold;
        GameObject Hand = managerhub.player.hand;
        GameObject HandBlock = managerhub.player.handBlock;
        GameObject HanTool = managerhub.player.handTool;

        byte now_HandBlock = slots[managerhub.player.selectindex].blockId;


        //�����һ������Ҫ�л����� ���� ��������з��鵫��isCatchBloch = false ���� ��������޷��鵫��ȴ��isCatchBloch = true
        if (now_HandBlock != previous_HandBlock || (now_HandBlock != 255 && managerhub.player.isCatchBlock == false) || (now_HandBlock == 255 && managerhub.player.isCatchBlock == true))
        {
            //�л���
            if (now_HandBlock == 255)
            {

                //���ټ�ѹ��Ʒ
                if (ToolAlive)
                {
                    foreach (Transform item in HanTool.transform)
                    {
                        Destroy(item.gameObject);
                    }
                    ToolAlive = false;
                }

                //������ȴʱ��
                StartCoroutine(ChangeBlockColdTime());

                //����Hand_Hold
                Hand_Hold.GetComponent<Animation>().Play("ChangeBlock_Down");

                //��ʾ�֣����ط���
                Hand.SetActive(true);
                HandBlock.SetActive(false);

                //�ó�Hand_Hold
                Hand_Hold.GetComponent<Animation>().Play("ChangeBlock_Up");

                //״̬
                managerhub.player.isCatchBlock = false;

            }
            //�л�������߹���
            else
            {
                //����Ƿ���
                if (!managerhub.Service_World.blocktypes[now_HandBlock].is2d)
                {
                    //���ټ�ѹ��Ʒ
                    if (ToolAlive)
                    {
                        foreach (Transform item in HanTool.transform)
                        {
                            Destroy(item.gameObject);
                        }
                        ToolAlive = false;
                    }

                    //������ȴʱ��
                    StartCoroutine(ChangeBlockColdTime());

                    //����Hand_Hold
                    Hand_Hold.GetComponent<Animation>().Play("ChangeBlock_Down");

                    //�Է��飬������
                    HandBlock.SetActive(true);
                    Hand.SetActive(false);

                    //���²���
                    int index = 0;
                    foreach (Transform child in HandBlock.transform)
                    {
                        //����
                        if (index >= 4)
                        {
                            child.gameObject.GetComponent<SpriteRenderer>().sprite = managerhub.Service_World.blocktypes[slots[managerhub.player.selectindex].blockId].top_sprit;
                        }
                        else
                        {
                            child.gameObject.GetComponent<SpriteRenderer>().sprite = managerhub.Service_World.blocktypes[slots[managerhub.player.selectindex].blockId].sprite;
                        }

                        index++;
                    }


                    //�ó�Hand_Hold
                    Hand_Hold.GetComponent<Animation>().Play("ChangeBlock_Up");

                    //״̬
                    managerhub.player.isCatchBlock = true;
                }
                else //����ǹ���
                {
                    ToolAlive = true;

                    //�ñ�����ˮͰ����ת�ǲ�һ����
                    if (managerhub.Service_World.blocktypes[now_HandBlock].isNeedRotation)
                    {
                        HanTool.transform.localPosition = new Vector3(0.542f, -0.098f, 0.774f);
                        HanTool.transform.localEulerAngles = new Vector3(0f, -93.837f, 68.013f);
                    }
                    else
                    {
                        HanTool.transform.localPosition = new Vector3(1f, -0.2f, 0.696f);
                        HanTool.transform.localEulerAngles = new Vector3(0f, -116.5f, 2.2f);
                    }




                    //������ȴʱ��
                    StartCoroutine(ChangeBlockColdTime());

                    //����Hand_Hold
                    Hand_Hold.GetComponent<Animation>().Play("ChangeBlock_Down");

                    //�Է��飬������
                    if (HandBlock.activeSelf)
                    {
                        HandBlock.SetActive(false);
                    }

                    HanTool.SetActive(true);
                    Hand.SetActive(false);

                    //���²���
                    foreach (Transform item in HanTool.transform)
                    {
                        Destroy(item.gameObject);
                    }

                    //print(managerhub.Service_World.blocktypes[now_HandBlock].blockName);
                    //print($"index: {now_HandBlock} , sprite:{managerhub.Service_World.blocktypes[now_HandBlock].sprite}");
                    managerhub.textureTo3D.ProcessSprite(managerhub.Service_World.blocktypes[now_HandBlock].sprite, HanTool.transform, 4, true);


                    //�ó�Hand_Hold
                    Hand_Hold.GetComponent<Animation>().Play("ChangeBlock_Up");

                    //״̬
                    managerhub.player.isCatchBlock = true;
                }

            }

            //update
            previous_HandBlock = now_HandBlock;
        }
    }


    #endregion


    #region ����


    //�������isFull
    //������
    //���ص�ǰ��Ʒ���Ƿ������װ��Ʒ
    public bool CheckSlotsFull(byte _targetType)
    {
        if (MC_Runtime_DynamicData.instance.GetGameMode() == GameMode.Creative)
        {
            bool hasEmptySlot = false;
            for (int i = 0; i < slots.Length; i++)
            {
                byte targetType = _targetType;

                //�ݿ������
                if (_targetType == VoxelData.Grass)
                {
                    targetType = VoxelData.Soil;
                }


                //��⵽��ͬ�Ĳ���
                if (slots[i].blockId == targetType)
                {
                    //���Ժϲ�
                    return false;
                }

                //���յ�slot
                if (slots[i].number == 0)
                {
                    hasEmptySlot = true;
                }
            }

            //���û����ͬ���ʣ������ɿյ�slot
            if (hasEmptySlot)
            {
                //û��
                return false;
            }
            else
            {
                //����
                return true;
            }
        }

        else
        {
            return false;
        }

    }

    //������������жϵ�ǰ�±���û�з���
    public bool istheindexHaveBlock(int index)
    {
        return slots[index].ishave;
    }

    //�����ҵ�һ�����±��
    //�Ҳ�������-1
    int find_empty_index()
    {
        for (int i = 0; i < 9; i++)
        {
            if (slots[i].ishave == false)
            {
                return i;
            }
        }

        return -1;
    }



    #endregion

}



//��Ʒ�����ݽṹ
[System.Serializable]
public class Slot
{
    //parameter
    public bool ishave = false;
    public byte blockId = 255;

    //2d-icon
    public Image icon;

    //3d-icon
    public GameObject Icon3Dobject;
    public Image TopFace;
    public Image LeftFace;
    public Image RightFace;

    //number
    public int number = 0;
    public TextMeshProUGUI TMP_number;



    public void ResetSlot()
    {

        //parameter
        ishave = false;
        blockId = 255;

        //2d-icon
        icon.sprite = null; icon.color = new Color(1f, 1f, 1f, 0f);

        //3d-icon
        Icon3Dobject.SetActive(false);
        TopFace.sprite = null;
        LeftFace.sprite = null;
        RightFace.sprite = null;

        //number
        number = 0;
        TMP_number.text = "";
    }


}

