using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
//using static UnityEditor.Progress;

public class BackPackManager : MonoBehaviour
{
    [Header("״̬")]
    [ReadOnly]public bool isfull = false;
    [ReadOnly] public bool isChanging = false;  //�л��������ȴ�������Ǻ���û������

    //���ݲ���
    [Header("����")]
    public ManagerHub managerhub;
    public Slot[] slots = new Slot[9];

    [Header("���������")]
    public float dropblock_destroyTime = 100f;
    public float absorb_Distance = 2.3f;
    public float drop_gravity = 4f;
    public float moveToplayer_duation = 0.2f;
    public float throwForce = 3f;
    public float ColdTime_Absorb = 1f;

    //�л�������Ʒ��
    [Header("�ı�������Ʒ")]
    private byte previous_HandBlock = 255;   //255�����ֱ���
    public float ChangeColdTime = 1f;
    

    //------------------------------------------------ ���Ĺ��� ---------------------------------------------------------------

    /// <summary>
    /// ������Ʒ����λ״̬�������ƻ�����÷�����������Ӧ��Ʒ������
    /// </summary>
    /// <param name="brokeOrplace">�������ͣ�0��ʾ�ƻ����飬1��ʾ���÷��顣</param>
    /// <param name="blocktype">Ҫ�����ķ������ͣ���ʾ�����ID��</param>
    public void update_slots (int brokeOrplace,byte blocktype)
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
                    _slot.TMP_number.text = $"{_slot.number}";
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

                    //icon
                    //�ж�����3d����2d
                    if (managerhub.world.blocktypes[blocktype].DrawMode == DrawMode.Block) //3d
                    {
                        slots[_index].Icon3Dobject.SetActive(true);
                        slots[_index].TopFace.sprite = managerhub.world.blocktypes[blocktype].top_sprit;
                        slots[_index].LeftFace.sprite = managerhub.world.blocktypes[blocktype].sprite;
                        slots[_index].RightFace.sprite = managerhub.world.blocktypes[blocktype].sprite;
                    }
                    else
                    {
                        slots[_index].icon.sprite = managerhub.world.blocktypes[blocktype].icon;
                        slots[_index].icon.color = new Color(1f, 1f, 1f, 1f);
                    }

                    //number
                    slots[_index].number++;
                    slots[_index].TMP_number.text = $"{slots[_index].number}";


                }

                if (_index == -1)
                {
                    isfull = true;
                }
            }

        }


        //place������--
        else if (brokeOrplace == 1)
        {
            if (slots[managerhub.player.selectindex].number - 1 <= 0)
            {
                isfull = false;
                slots[managerhub.player.selectindex].ResetSlot();
            }
            else
            {
                slots[managerhub.player.selectindex].number--;
                slots[managerhub.player.selectindex].TMP_number.text = $"{slots[managerhub.player.selectindex].number}";
            }

        }
    }

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
                    _slot.TMP_number.text = $"{_slot.number}";
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

                    //icon
                    //�ж�����3d����2d
                    if (managerhub.world.blocktypes[blocktype].DrawMode == DrawMode.Block) //3d
                    {
                        slots[_index].Icon3Dobject.SetActive(true);
                        slots[_index].TopFace.sprite = managerhub.world.blocktypes[blocktype].top_sprit;
                        slots[_index].LeftFace.sprite = managerhub.world.blocktypes[blocktype].sprite;
                        slots[_index].RightFace.sprite = managerhub.world.blocktypes[blocktype].sprite;
                    }
                    else
                    {
                        slots[_index].icon.sprite = managerhub.world.blocktypes[blocktype].icon;
                        slots[_index].icon.color = new Color(1f, 1f, 1f, 1f);
                    }

                    //number
                    slots[_index].number += _number;
                    slots[_index].TMP_number.text = $"{slots[_index].number}";


                }

                if (_index == -1)
                {
                    isfull = true;
                }
            }

        }


        //place������--
        else if (brokeOrplace == 1)
        {
            if (slots[managerhub.player.selectindex].number - 1 <= 0)
            {
                isfull = false;
                slots[managerhub.player.selectindex].ResetSlot();
            }
            else
            {
                if (slots[managerhub.player.selectindex].number > _number)
                {
                    slots[managerhub.player.selectindex].number -= _number;
                    slots[managerhub.player.selectindex].TMP_number.text = $"{slots[managerhub.player.selectindex].number}";
                }
                else
                {
                    slots[managerhub.player.selectindex].number = 0;
                    slots[managerhub.player.selectindex].TMP_number.text = $"{slots[managerhub.player.selectindex].number}";
                }
                
            }

        }
    }




    /// <summary>
    /// ��Q����Ʒ
    /// </summary>
    public void ThrowDropBox()
    {
        Transform Eyes = managerhub.player.GetEyesPosition();
        Vector3 _ThrowOrigin = Eyes.transform.forward * 0.3f + new Vector3(Eyes.transform.position.x, Eyes.transform.position.y - 0.3f, Eyes.transform.position.z);

        //�ж��Ƿ����ӵ�����
        if (slots[managerhub.player.selectindex].blockId != 255 && slots[managerhub.player.selectindex].number > 0)
        {
            //���������
            CreateDropBox(_ThrowOrigin, slots[managerhub.player.selectindex].blockId, true, ColdTime_Absorb);

            //��Ʒ����һ
            update_slots(1, 0);

            ChangeBlockInHand();
        }


    }



    //---------------------------------------------------------------------------------------------------------------------










    //------------------------------------------------ ʵ�ֹ��� ---------------------------------------------------------------


    //��ʼ��Manager
    public void InitBackPackManager()
    {
        foreach (var _slot in slots)
        {
            _slot.ResetSlot();
        }
    }

    //���������(����,����)
    public void CreateDropBox(Vector3 _pos, byte _blocktype, bool _needThrow, float _ColdTimeTiabsorb)
    {
        World world = managerhub.world;
        Transform Eyes = managerhub.player.GetEyesPosition();

        //ˢ��ƫ��
        float x_offset = UnityEngine.Random.Range(2, 8) / 10f;
        float y_offset = UnityEngine.Random.Range(5, 8) / 10f;
        float z_offset = UnityEngine.Random.Range(2, 8) / 10f;

        //��������
        GameObject DropBlock = new GameObject(managerhub.world.blocktypes[_blocktype].blockName);
        DropBlock.AddComponent<FloatingCube>().InitWorld(managerhub, _blocktype, _ColdTimeTiabsorb);
        DropBlock.transform.SetParent(GameObject.Find("Environment/DropBlocks").transform);

        if (_needThrow)
        {
            DropBlock.transform.position = new Vector3(_pos.x, _pos.y, _pos.z);

        }
        else
        {
            DropBlock.transform.position = new Vector3(_pos.x + x_offset, _pos.y + y_offset, _pos.z + z_offset);

        }

        //����ͼ����ͼ��û��ͼ��icon
        if (managerhub.world.blocktypes[_blocktype].sprite != null)
        {
           

            //Top
            GameObject _Top = new GameObject("Top");
            _Top.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].top_sprit;
            _Top.transform.SetParent(DropBlock.transform);
            _Top.transform.localPosition = new Vector3(0, 0.16f, 0);
            _Top.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

            //Buttom
            GameObject _Buttom = new GameObject("Buttom");
            _Buttom.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].buttom_sprit;
            _Buttom.transform.SetParent(DropBlock.transform);
            _Buttom.transform.localPosition = new Vector3(0, 0, 0);
            _Buttom.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

            //Left
            GameObject _Left = new GameObject("Left");
            _Left.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
            _Left.transform.SetParent(DropBlock.transform);
            _Left.transform.localPosition = new Vector3(-0.08f, 0.08f, 0);
            _Left.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

            //Right
            GameObject _Right = new GameObject("Right");
            _Right.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
            _Right.transform.SetParent(DropBlock.transform);
            _Right.transform.localPosition = new Vector3(0.08f, 0.08f, 0);
            _Right.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

            //Forward
            GameObject _Forward = new GameObject("Forward");
            _Forward.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
            _Forward.transform.SetParent(DropBlock.transform);
            _Forward.transform.localPosition = new Vector3(0, 0.08f, 0.08f);
            _Forward.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

            //Back
            GameObject _Back = new GameObject("Back");
            _Back.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
            _Back.transform.SetParent(DropBlock.transform);
            _Back.transform.localPosition = new Vector3(0, 0.08f, -0.08f);
            _Back.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        else
        {
            //Top
            GameObject _Top = new GameObject("Top");
            _Top.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
            _Top.transform.SetParent(DropBlock.transform);
            _Top.transform.localPosition = new Vector3(0, 0.16f, 0);
            _Top.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

            //Buttom
            GameObject _Buttom = new GameObject("Buttom");
            _Buttom.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
            _Buttom.transform.SetParent(DropBlock.transform);
            _Buttom.transform.localPosition = new Vector3(0, 0, 0);
            _Buttom.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

            //Left
            GameObject _Left = new GameObject("Left");
            _Left.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
            _Left.transform.SetParent(DropBlock.transform);
            _Left.transform.localPosition = new Vector3(-0.08f, 0.08f, 0);
            _Left.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

            //Right
            GameObject _Right = new GameObject("Right");
            _Right.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
            _Right.transform.SetParent(DropBlock.transform);
            _Right.transform.localPosition = new Vector3(0.08f, 0.08f, 0);
            _Right.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

            //Forward
            GameObject _Forward = new GameObject("Forward");
            _Forward.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
            _Forward.transform.SetParent(DropBlock.transform);
            _Forward.transform.localPosition = new Vector3(0, 0.08f, 0.08f);
            _Forward.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

            //Back
            GameObject _Back = new GameObject("Back");
            _Back.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].sprite;
            _Back.transform.SetParent(DropBlock.transform);
            _Back.transform.localPosition = new Vector3(0, 0.08f, -0.08f);
            _Back.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }


        //���Ŵ���
        DropBlock.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);


        //�Ƿ��ӳ�ȥ
        if (_needThrow)
        {
            Rigidbody rd = DropBlock.AddComponent<Rigidbody>();
            //rd.isKinematic = true;
            rd.velocity = Eyes.transform.forward * throwForce;
        }

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
                if (!managerhub.world.blocktypes[now_HandBlock].isTool)
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
                            child.gameObject.GetComponent<SpriteRenderer>().sprite = managerhub.world.blocktypes[slots[managerhub.player.selectindex].blockId].top_sprit;
                        }
                        else
                        {
                            child.gameObject.GetComponent<SpriteRenderer>().sprite = managerhub.world.blocktypes[slots[managerhub.player.selectindex].blockId].sprite;
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

                    //�ı丸����Transform
                    //HanTool.transform.localPosition = new Vector3(0.542f, -0.098f, 0.774f);
                    //HanTool.transform.rotation = Quaternion.Euler(0f, -93.837f, 68.013f);



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

                    //print(managerhub.world.blocktypes[now_HandBlock].blockName);
                    //print($"index: {now_HandBlock} , sprite:{managerhub.world.blocktypes[now_HandBlock].sprite}");
                    managerhub.textureTo3D.ProcessSprite(managerhub.world.blocktypes[now_HandBlock].Toolsprite, HanTool.transform, 4);


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

    //---------------------------------------------------------------------------------------------------------------------










    //------------------------------------------------ ���� ---------------------------------------------------------------

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


    //�л�������ȴʱ��
    IEnumerator ChangeBlockColdTime()
    {
        isChanging = true;

        yield return new WaitForSeconds(ChangeColdTime);

        isChanging = false;
    }

    //---------------------------------------------------------------------------------------------------------------------
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

