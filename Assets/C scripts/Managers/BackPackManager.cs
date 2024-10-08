using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
//using static UnityEditor.Progress;

public class BackPackManager : MonoBehaviour
{
    [Header("状态")]
    [ReadOnly] public bool isChanging = false;  //切换方块的冷却锁，但是好像没有用上

    //数据部分
    [Header("引用")]
    public ManagerHub managerhub;
    public Slot[] slots = new Slot[9];

    [Header("掉落物参数")]
    public float dropblock_destroyTime = 100f;
    public float absorb_Distance = 2.3f;
    public float drop_gravity = 4f;
    public float moveToplayer_duation = 0.2f;
    public float throwForce = 3f;

    //切换手中物品的
    [Header("改变手中物品")]
    private byte previous_HandBlock = 255;   //255代表手本身
    public float ChangeColdTime = 1f;


    //------------------------------------------------ 核心功能 ---------------------------------------------------------------

    /// <summary>
    /// 同步物品栏和背包物品栏
    /// _prior = 0：物品栏更新了，同步背包物品栏
    /// _prior = 1：背包物品栏更新了，同步物品栏
    /// </summary>
    public Transform 背包物品栏;
    public void SYN_allSlots(int _prior)
    {
        //同步背包物品栏
        if (_prior == 0)
        {
            print("已同步背包物品栏");
        }

        //同步物品栏
        else if (_prior == 1)
        {
            //print("已同步物品栏");
            int i = 0;
            foreach (Transform item in 背包物品栏)
            {
                BlockItem _targetItem = item.GetComponent<SlotBlockItem>().MyItem;
                slots[i].blockId = _targetItem._blocktype;
                slots[i].number = _targetItem._number;
                i++;
            }
            FlashSlot();
            ChangeBlockInHand();
        }
        else
        {
            print("_prior不符合输入");
        }
        

        
    }

    //单纯刷新Slot
    public void FlashSlot()
    {
        for (int i = 0;i < slots.Length;i ++)
        {
            byte _type = slots[i].blockId;

            if (_type != 255)
            {
                //状态
                slots[i].ishave = true;

                //Icon
                if (managerhub.world.blocktypes[_type].is2d == false) //3d
                {
                    slots[i].Icon3Dobject.SetActive(true);
                    slots[i].TopFace.sprite = managerhub.world.blocktypes[_type].top_sprit;
                    slots[i].LeftFace.sprite = managerhub.world.blocktypes[_type].sprite;
                    slots[i].RightFace.sprite = managerhub.world.blocktypes[_type].sprite;
                }
                else
                {
                    slots[i].icon.sprite = managerhub.world.blocktypes[_type].icon;
                    slots[i].icon.color = new Color(1f, 1f, 1f, 1f);
                }

                //number
                slots[i].TMP_number.text = $"{slots[i].number}";

            }
            //不渲染
            else
            {
                slots[i].ResetSlot();
            }
        }
    }

    /// <summary>
    /// 更新物品栏
    /// (0:方块++)
    /// (1:方块--)
    /// </summary>
    /// <param name="brokeOrplace">0: 方块++</param>
    /// <param name="blocktype">要操作的方块类型，表示方块的ID。</param>
    public void update_slots (int brokeOrplace,byte blocktype)
    {
        //broke,方块++
        if (brokeOrplace == 0)
        {
            bool isfind = false;

            //先寻找是否有该方块
            foreach (Slot _slot in slots)
            {

                //如果找到
                if (_slot.blockId == blocktype)
                {
                    isfind = true;

                    _slot.number++;
                    _slot.TMP_number.text = $"{_slot.number}";
                }
            }

            //如果没有该方块，则新加
            if (!isfind)
            {
                int _index = find_empty_index();
                if (_index != -1)
                {
                    //parameter
                    slots[_index].ishave = true;
                    slots[_index].blockId = blocktype;

                    //icon
                    //判断是用3d还是2d
                    if (managerhub.world.blocktypes[blocktype].is2d == false) //3d
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

            }

        }


        //place，方块--
        else if (brokeOrplace == 1)
        {
            if (slots[managerhub.player.selectindex].number - 1 <= 0)
            {
                slots[managerhub.player.selectindex].ResetSlot();
            }
            else
            {
                slots[managerhub.player.selectindex].number--;
                slots[managerhub.player.selectindex].TMP_number.text = $"{slots[managerhub.player.selectindex].number}";
            }

        }


        ChangeBlockInHand();
    }

    /// <summary>
    /// 更新物品栏(0:方块++)
    /// </summary>
    /// <param name="brokeOrplace"></param>
    /// <param name="blocktype"></param>
    /// <param name="_number"></param>
    public void update_slots(int brokeOrplace, byte blocktype, int _number)
    {
        //broke,方块++
        if (brokeOrplace == 0)
        {
            bool isfind = false;

            //先寻找是否有该方块
            foreach (Slot _slot in slots)
            {

                //如果找到
                if (_slot.blockId == blocktype)
                {
                    isfind = true;

                    _slot.number += _number;
                    _slot.TMP_number.text = $"{_slot.number}";
                }
            }

            //如果没有该方块，则新加
            if (!isfind)
            {
                int _index = find_empty_index();
                if (_index != -1)
                {
                    //parameter
                    slots[_index].ishave = true;
                    slots[_index].blockId = blocktype;

                    //icon
                    //判断是用3d还是2d
                    if (managerhub.world.blocktypes[blocktype].is2d == false) //3d
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

            }

            ChangeBlockInHand();

        }


        //place，方块--
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
    /// 按Q扔物品
    /// </summary>
    public void ThrowDropBox()
    {
        Transform Eyes = managerhub.player.GetEyesPosition();
        Vector3 _ThrowOrigin = Eyes.transform.forward * 0.3f + new Vector3(Eyes.transform.position.x, Eyes.transform.position.y - 0.3f, Eyes.transform.position.z);

        //判断是否能扔掉落物
        if (slots[managerhub.player.selectindex].blockId != 255 && slots[managerhub.player.selectindex].number > 0)
        {
            //创造掉落物
            CreateDropBox(_ThrowOrigin, slots[managerhub.player.selectindex].blockId, true);

            //物品栏减一
            update_slots(1, 0);

            ChangeBlockInHand();
        }


    }

    //用于替代isFull
    //外界调用
    //返回当前物品栏是否可以再装物品
    public bool CheckSlotsFull(byte _targetType)
    {
        bool hasEmptySlot = false;
        for (int i = 0;i < slots.Length; i++)
        {
            byte targetType = _targetType;

            //草块变泥土
            if (_targetType == VoxelData.Grass)
            {
                targetType = VoxelData.Soil;
            }


            //检测到相同的材质
            if (slots[i].blockId == targetType)
            {
                //可以合并
                return false;
            }

            //检测空的slot
            if (slots[i].number == 0)
            {
                hasEmptySlot = true;
            }
        }

        //如果没有相同材质，但是由空的slot
        if (hasEmptySlot)
        {
            //没满
            return false;
        }
        else
        {
            //满了
            return true;
        }

    }

    //---------------------------------------------------------------------------------------------------------------------










    //------------------------------------------------ 实现功能 ---------------------------------------------------------------


    //初始化Manager
    public void InitBackPackManager()
    {
        foreach (var _slot in slots)
        {
            _slot.ResetSlot();
        }
        ClearDropBlocks();

        ChangeBlockInHand();
    }

    void ClearDropBlocks()
    {
        Transform dropblockParent = GameObject.Find("Environment/DropBlocks").transform;

        foreach (Transform item in dropblockParent)
        {
            Destroy(item.gameObject);
        }
    }


    //创造掉落物(坐标,类型)
    public void CreateDropBox(Vector3 _pos, byte _blocktype, bool _needThrow)
    {
        World world = managerhub.world;
        Transform Eyes = managerhub.player.GetEyesPosition();

        //刷新偏移
        float x_offset = UnityEngine.Random.Range(2, 8) / 10f;
        float y_offset = UnityEngine.Random.Range(5, 8) / 10f;
        float z_offset = UnityEngine.Random.Range(2, 8) / 10f;

        //创建父类
        GameObject DropBlock = new GameObject(managerhub.world.blocktypes[_blocktype].blockName);
        DropBlock.AddComponent<FloatingCube>().InitWorld(managerhub, _blocktype);
        DropBlock.transform.SetParent(GameObject.Find("Environment/DropBlocks").transform);


        // 创造物体

        //具有多面的立方体

        //只有单面的立方体

        //2d挤压物体

        //如果不是2d挤压物体
        if (managerhub.world.blocktypes[_blocktype].is2d )
        {
            managerhub.textureTo3D.ProcessSprite(managerhub.world.blocktypes[_blocktype].sprite, DropBlock.transform, 2, false);


        }
        else
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
        


        //是否扔出去
        if (_needThrow)
        {
            DropBlock.transform.position = new Vector3(_pos.x, _pos.y, _pos.z);
            Rigidbody rd = DropBlock.AddComponent<Rigidbody>();
            //rd.isKinematic = true;
            rd.velocity = Eyes.transform.forward * throwForce;
        }
        else
        {
            DropBlock.transform.position = new Vector3(_pos.x + x_offset, _pos.y + y_offset, _pos.z + z_offset);
        }

        //最后放大本体
        DropBlock.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

    }

    //切换物品
    bool ToolAlive = false;
    public void ChangeBlockInHand()
    {
        
        GameObject Hand_Hold = managerhub.player.hand_Hold;
        GameObject Hand = managerhub.player.hand;
        GameObject HandBlock = managerhub.player.handBlock;
        GameObject HanTool = managerhub.player.handTool;

        byte now_HandBlock = slots[managerhub.player.selectindex].blockId;


        //如果不一样，就要切换方块 或者 方块槽里有方块但是isCatchBloch = false 或者 方块槽里无方块但是却是isCatchBloch = true
        if (now_HandBlock != previous_HandBlock || (now_HandBlock != 255 && managerhub.player.isCatchBlock == false) || (now_HandBlock == 255 && managerhub.player.isCatchBlock == true))
        {
            //切换手
            if (now_HandBlock == 255)
            {

                //销毁挤压物品
                if (ToolAlive)
                {
                    foreach (Transform item in HanTool.transform)
                    {
                        Destroy(item.gameObject);
                    }
                    ToolAlive = false;
                }

                //开启冷却时间
                StartCoroutine(ChangeBlockColdTime());

                //放下Hand_Hold
                Hand_Hold.GetComponent<Animation>().Play("ChangeBlock_Down");

                //显示手，隐藏方块
                Hand.SetActive(true);
                HandBlock.SetActive(false);

                //拿出Hand_Hold
                Hand_Hold.GetComponent<Animation>().Play("ChangeBlock_Up");

                //状态
                managerhub.player.isCatchBlock = false;

            }
            //切换方块或者工具
            else
            {
                //如果是方块
                if (!managerhub.world.blocktypes[now_HandBlock].is2d)
                {
                    //销毁挤压物品
                    if (ToolAlive)
                    {
                        foreach (Transform item in HanTool.transform)
                        {
                            Destroy(item.gameObject);
                        }
                        ToolAlive = false;
                    }

                    //开启冷却时间
                    StartCoroutine(ChangeBlockColdTime());

                    //放下Hand_Hold
                    Hand_Hold.GetComponent<Animation>().Play("ChangeBlock_Down");

                    //显方块，隐藏手
                    HandBlock.SetActive(true);
                    Hand.SetActive(false);

                    //更新材质
                    int index = 0;
                    foreach (Transform child in HandBlock.transform)
                    {
                        //顶面
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


                    //拿出Hand_Hold
                    Hand_Hold.GetComponent<Animation>().Play("ChangeBlock_Up");

                    //状态
                    managerhub.player.isCatchBlock = true;
                }
                else //如果是工具
                {
                    ToolAlive = true;

                    //拿宝剑和水桶的旋转是不一样的
                    if (managerhub.world.blocktypes[now_HandBlock].isNeedRotation)
                    {
                        HanTool.transform.localPosition = new Vector3(0.542f, -0.098f, 0.774f);
                        HanTool.transform.localEulerAngles = new Vector3(0f, -93.837f, 68.013f);
                    }
                    else
                    {
                        HanTool.transform.localPosition = new Vector3(1f, -0.2f, 0.696f);
                        HanTool.transform.localEulerAngles = new Vector3(0f, -116.5f, 2.2f);
                    }
                    



                    //开启冷却时间
                    StartCoroutine(ChangeBlockColdTime());

                    //放下Hand_Hold
                    Hand_Hold.GetComponent<Animation>().Play("ChangeBlock_Down");

                    //显方块，隐藏手
                    if (HandBlock.activeSelf)
                    {
                        HandBlock.SetActive(false);
                    }

                    HanTool.SetActive(true);
                    Hand.SetActive(false);

                    //更新材质
                    foreach (Transform item in HanTool.transform)
                    {
                        Destroy(item.gameObject);
                    }

                    //print(managerhub.world.blocktypes[now_HandBlock].blockName);
                    //print($"index: {now_HandBlock} , sprite:{managerhub.world.blocktypes[now_HandBlock].sprite}");
                    managerhub.textureTo3D.ProcessSprite(managerhub.world.blocktypes[now_HandBlock].sprite, HanTool.transform, 4, true);


                    //拿出Hand_Hold
                    Hand_Hold.GetComponent<Animation>().Play("ChangeBlock_Up");

                    //状态
                    managerhub.player.isCatchBlock = true;
                }

            }

            //update
            previous_HandBlock = now_HandBlock;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------










    //------------------------------------------------ 工具 ---------------------------------------------------------------

    //这个函数用来判断当前下标有没有方块
    public bool istheindexHaveBlock(int index)
    {
        return slots[index].ishave;
    }

    //用来找第一个空下标的
    //找不到返回-1
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


    //切换方块冷却时间
    IEnumerator ChangeBlockColdTime()
    {
        isChanging = true;

        yield return new WaitForSeconds(ChangeColdTime);

        isChanging = false;
    }

    //---------------------------------------------------------------------------------------------------------------------
}



//物品栏数据结构
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

