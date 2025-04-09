using Homebrew;
using MCEntity;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class BackPackManager : MonoBehaviour
{

    #region 状态

    [Foldout("状态", true)]
    [Header("切换方块的冷却锁")][ReadOnly] public bool isChanging = false; 

    #endregion


    #region 周期函数

    ManagerHub managerhub;

    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
    }


    private void Update()
    {
        
    }


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

    #endregion


    #region 涉及到UI部分

    public Slot[] slots = new Slot[9];
    public Transform 生存背包物品栏;
    public Transform 创造背包物品栏;


    //用于去生存模式背包物品栏将物品放进去
    public Transform 生存模式背包物品栏;
    void AddBlockToBackPackSlots(BlockItem _targetItem)
    {
        if (MC_Runtime_DynamicData.instance.GetGameMode() == GameMode.Survival)
        {
            foreach (Transform item in 生存模式背包物品栏)
            {
                SlotBlockItem slotScript = item.GetComponent<SlotBlockItem>();

                if (slotScript.MyItem._blocktype == _targetItem._blocktype || slotScript.MyItem._blocktype == 255)
                {
                    //print("添加了一个物品");
                    slotScript.AddBlock(_targetItem);
                    break;
                }
            }
        }

    }


    /// <summary>
    /// 渲染物品栏，同步背包物品栏
    /// </summary>
    public void Render_BackPackSlots(bool _NeedSYN)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            byte _type = slots[i].blockId;

            if (_type != 255)
            {
                //状态
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
            //不渲染
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


    #region 物品栏


    /// <summary>
    /// 用于同步物品栏和背包物品栏
    /// <para>[0]: 更新背包物品栏</para>
    /// <para>[1]: 更新物品栏</para>
    /// </summary>
    /// <param name="_prior"></param>
    public void SYN_allSlots(int _prior)
    {

        Transform 背包物品栏;
        if (MC_Runtime_DynamicData.instance.GetGameMode() == GameMode.Survival)
        {
            背包物品栏 = 生存背包物品栏;
        }
        else
        {
            背包物品栏 = 创造背包物品栏;
        }


        //同步背包物品栏
        if (_prior == 0)
        {
            GameObject[] 背包物品栏集合 = new GameObject[slots.Length];
            int index_背包物品栏 = 0;

            //填充背包物品栏集合
            foreach (Transform item in 背包物品栏)
            {
                背包物品栏集合[index_背包物品栏] = item.gameObject;

                index_背包物品栏++;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                背包物品栏集合[i].GetComponent<SlotBlockItem>().InitBlockItem(new BlockItem(slots[i].blockId, slots[i].number));
                背包物品栏集合[i].GetComponent<SlotBlockItem>().UpdateBlockItem(false);
            }
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
            Render_BackPackSlots(false);
            ChangeBlockInHand();
        }
        else
        {
            print("_prior不符合输入");
        }



    }

    /// <summary>
    /// 计算物品栏参数
    /// <para>[0]: 方块++ </para>
    /// <para>[1]: 方块--</para>
    /// </summary>
    /// <param name="brokeOrplace">操作类型，放置或破坏</param>
    /// <param name="blocktype">方块类型</param>
    public void update_slots(int brokeOrplace, byte blocktype)
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

                    //number
                    slots[_index].number++;


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
            }

        }

        Render_BackPackSlots(true);
        ChangeBlockInHand();
    }

    /// <summary>
    /// 计算物品栏参数
    /// <para>[0]: 方块++ </para>
    /// <para>[1]: 方块--</para>
    /// </summary>
    /// <param name="brokeOrplace">操作类型，放置或破坏</param>
    /// <param name="blocktype">方块类型</param>
    /// <param name="_number">更新的数量</param>
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

                    //number
                    slots[_index].number += _number;


                }

                //物品栏发不下了，则用背包物品栏
                else if (_index == -1)
                {
                    //print("添加到背包物品栏");
                    AddBlockToBackPackSlots(new BlockItem(blocktype, _number));
                }

            }


            Render_BackPackSlots(true);
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
                }
                else
                {
                    slots[managerhub.player.selectindex].number = 0;
                }

            }

        }
    }


    #endregion


    #region 掉落物部分

    [Header("掉落物参数")]
    public float dropblock_destroyTime = 100f;
    public float absorb_Distance = 2.3f;
    public float drop_gravity = 4f;
    public float moveToplayer_duation = 0.2f;
    public float throwForce = 3f;


    /// <summary>
    /// 创造掉落物(坐标,类型)
    /// </summary>
    /// <param name="_pos">必须为Int坐标，不需要Center坐标</param>
    /// <param name="_InitItem"></param>
    /// <param name="_needThrow"></param>
    public GameObject NewDropBlock;
    public void CreateDropBox(Vector3 _pos, BlockItem _InitItem, bool _needThrow)
    {
        Transform Eyes = managerhub.player.GetEyesPosition();

        //刷新偏移
        float x_offset = UnityEngine.Random.Range(2, 8) / 10f;
        float y_offset = UnityEngine.Random.Range(5, 8) / 10f;
        float z_offset = UnityEngine.Random.Range(2, 8) / 10f;

        //创建父类
        GameObject DropBlock = GameObject.Instantiate(NewDropBlock);
        DropBlock.transform.SetParent(SceneData.GetDropBlockParent().transform);


        //是否扔出去
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
    /// 按Q扔物品
    /// </summary>
    /// 

    //获取到准确的玩家眼睛丢出点
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

        //判断是否能扔掉落物
        if (slots[managerhub.player.selectindex].blockId != 255 && slots[managerhub.player.selectindex].number > 0)
        {
            //创造掉落物
            CreateDropBox(_ThrowOrigin, new BlockItem(slots[managerhub.player.selectindex].blockId, 1), true);

            //物品栏减一
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


    #region 切换方块部分

    [Header("改变手中物品")]
    private byte previous_HandBlock = 255;   //255代表手本身
    public float ChangeColdTime = 1f;

    //切换方块冷却时间
    IEnumerator ChangeBlockColdTime()
    {
        isChanging = true;

        yield return new WaitForSeconds(ChangeColdTime);

        isChanging = false;
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
                if (!managerhub.Service_World.blocktypes[now_HandBlock].is2d)
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
                            child.gameObject.GetComponent<SpriteRenderer>().sprite = managerhub.Service_World.blocktypes[slots[managerhub.player.selectindex].blockId].top_sprit;
                        }
                        else
                        {
                            child.gameObject.GetComponent<SpriteRenderer>().sprite = managerhub.Service_World.blocktypes[slots[managerhub.player.selectindex].blockId].sprite;
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

                    //print(managerhub.Service_World.blocktypes[now_HandBlock].blockName);
                    //print($"index: {now_HandBlock} , sprite:{managerhub.Service_World.blocktypes[now_HandBlock].sprite}");
                    managerhub.textureTo3D.ProcessSprite(managerhub.Service_World.blocktypes[now_HandBlock].sprite, HanTool.transform, 4, true);


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


    #endregion


    #region 工具


    //用于替代isFull
    //外界调用
    //返回当前物品栏是否可以再装物品
    public bool CheckSlotsFull(byte _targetType)
    {
        if (MC_Runtime_DynamicData.instance.GetGameMode() == GameMode.Creative)
        {
            bool hasEmptySlot = false;
            for (int i = 0; i < slots.Length; i++)
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

        else
        {
            return false;
        }

    }

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



    #endregion

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

