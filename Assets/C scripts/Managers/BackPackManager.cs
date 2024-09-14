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
    [Header("状态")]
    public bool isfull = false;
    public bool isChanging = false;  //切换方块的冷却锁，但是好像没有用上

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
    public float ColdTime_Absorb = 1f;

    //切换手中物品的
    [Header("改变手中物品")]
    private byte previous_HandBlock = 255;   //255代表手本身
    public float ChangeColdTime = 1f;
    

    //------------------------------------------------ 核心功能 ---------------------------------------------------------------

    /// <summary>
    /// 更新物品栏槽位状态，根据破坏或放置方块来增减相应物品数量。
    /// </summary>
    /// <param name="brokeOrplace">操作类型，0表示破坏方块，1表示放置方块。</param>
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
                    if (managerhub.worldManager.blocktypes[blocktype].DrawMode == DrawMode.Block) //3d
                    {
                        slots[_index].Icon3Dobject.SetActive(true);
                        slots[_index].TopFace.sprite = managerhub.worldManager.blocktypes[blocktype].top_sprit;
                        slots[_index].LeftFace.sprite = managerhub.worldManager.blocktypes[blocktype].sprite;
                        slots[_index].RightFace.sprite = managerhub.worldManager.blocktypes[blocktype].sprite;
                    }
                    else
                    {
                        slots[_index].icon.sprite = managerhub.worldManager.blocktypes[blocktype].icon;
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


        //place，方块--
        else if (brokeOrplace == 1)
        {
            if (slots[managerhub.playerManager.selectindex].number - 1 <= 0)
            {
                isfull = false;
                slots[managerhub.playerManager.selectindex].ResetSlot();
            }
            else
            {
                slots[managerhub.playerManager.selectindex].number--;
                slots[managerhub.playerManager.selectindex].TMP_number.text = $"{slots[managerhub.playerManager.selectindex].number}";
            }

        }
    }




    /// <summary>
    /// 按Q扔物品
    /// </summary>
    public void ThrowDropBox()
    {
        Transform Eyes = managerhub.playerManager.GetEyesPosition();
        Vector3 _ThrowOrigin = Eyes.transform.forward * 0.3f + new Vector3(Eyes.transform.position.x, Eyes.transform.position.y - 0.3f, Eyes.transform.position.z);

        //判断是否能扔掉落物
        if (slots[managerhub.playerManager.selectindex].blockId != 255 && slots[managerhub.playerManager.selectindex].number > 0)
        {
            //创造掉落物
            CreateDropBox(_ThrowOrigin, slots[managerhub.playerManager.selectindex].blockId, true, ColdTime_Absorb);

            //物品栏减一
            update_slots(1, 0);

            ChangeBlockInHand();
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
    }

    //创造掉落物(坐标,类型)
    public void CreateDropBox(Vector3 _pos, byte _blocktype, bool _needThrow, float _ColdTimeTiabsorb)
    {
        World worldManager = managerhub.worldManager;
        Transform Eyes = managerhub.playerManager.GetEyesPosition();

        //刷新偏移
        float x_offset = UnityEngine.Random.Range(2, 8) / 10f;
        float y_offset = UnityEngine.Random.Range(5, 8) / 10f;
        float z_offset = UnityEngine.Random.Range(2, 8) / 10f;

        //创建父类
        GameObject DropBlock = new GameObject(managerhub.worldManager.blocktypes[_blocktype].blockName);
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

        //有贴图用贴图，没贴图用icon
        if (managerhub.worldManager.blocktypes[_blocktype].sprite != null)
        {
           

            //Top
            GameObject _Top = new GameObject("Top");
            _Top.AddComponent<SpriteRenderer>().sprite = worldManager.blocktypes[_blocktype].top_sprit;
            _Top.transform.SetParent(DropBlock.transform);
            _Top.transform.localPosition = new Vector3(0, 0.16f, 0);
            _Top.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

            //Buttom
            GameObject _Buttom = new GameObject("Buttom");
            _Buttom.AddComponent<SpriteRenderer>().sprite = worldManager.blocktypes[_blocktype].buttom_sprit;
            _Buttom.transform.SetParent(DropBlock.transform);
            _Buttom.transform.localPosition = new Vector3(0, 0, 0);
            _Buttom.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

            //Left
            GameObject _Left = new GameObject("Left");
            _Left.AddComponent<SpriteRenderer>().sprite = worldManager.blocktypes[_blocktype].sprite;
            _Left.transform.SetParent(DropBlock.transform);
            _Left.transform.localPosition = new Vector3(-0.08f, 0.08f, 0);
            _Left.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

            //Right
            GameObject _Right = new GameObject("Right");
            _Right.AddComponent<SpriteRenderer>().sprite = worldManager.blocktypes[_blocktype].sprite;
            _Right.transform.SetParent(DropBlock.transform);
            _Right.transform.localPosition = new Vector3(0.08f, 0.08f, 0);
            _Right.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

            //Forward
            GameObject _Forward = new GameObject("Forward");
            _Forward.AddComponent<SpriteRenderer>().sprite = worldManager.blocktypes[_blocktype].sprite;
            _Forward.transform.SetParent(DropBlock.transform);
            _Forward.transform.localPosition = new Vector3(0, 0.08f, 0.08f);
            _Forward.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

            //Back
            GameObject _Back = new GameObject("Back");
            _Back.AddComponent<SpriteRenderer>().sprite = worldManager.blocktypes[_blocktype].sprite;
            _Back.transform.SetParent(DropBlock.transform);
            _Back.transform.localPosition = new Vector3(0, 0.08f, -0.08f);
            _Back.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        else
        {
            //Top
            GameObject _Top = new GameObject("Top");
            _Top.AddComponent<SpriteRenderer>().sprite = worldManager.blocktypes[_blocktype].sprite;
            _Top.transform.SetParent(DropBlock.transform);
            _Top.transform.localPosition = new Vector3(0, 0.16f, 0);
            _Top.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

            //Buttom
            GameObject _Buttom = new GameObject("Buttom");
            _Buttom.AddComponent<SpriteRenderer>().sprite = worldManager.blocktypes[_blocktype].sprite;
            _Buttom.transform.SetParent(DropBlock.transform);
            _Buttom.transform.localPosition = new Vector3(0, 0, 0);
            _Buttom.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

            //Left
            GameObject _Left = new GameObject("Left");
            _Left.AddComponent<SpriteRenderer>().sprite = worldManager.blocktypes[_blocktype].sprite;
            _Left.transform.SetParent(DropBlock.transform);
            _Left.transform.localPosition = new Vector3(-0.08f, 0.08f, 0);
            _Left.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

            //Right
            GameObject _Right = new GameObject("Right");
            _Right.AddComponent<SpriteRenderer>().sprite = worldManager.blocktypes[_blocktype].sprite;
            _Right.transform.SetParent(DropBlock.transform);
            _Right.transform.localPosition = new Vector3(0.08f, 0.08f, 0);
            _Right.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

            //Forward
            GameObject _Forward = new GameObject("Forward");
            _Forward.AddComponent<SpriteRenderer>().sprite = worldManager.blocktypes[_blocktype].sprite;
            _Forward.transform.SetParent(DropBlock.transform);
            _Forward.transform.localPosition = new Vector3(0, 0.08f, 0.08f);
            _Forward.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

            //Back
            GameObject _Back = new GameObject("Back");
            _Back.AddComponent<SpriteRenderer>().sprite = worldManager.blocktypes[_blocktype].sprite;
            _Back.transform.SetParent(DropBlock.transform);
            _Back.transform.localPosition = new Vector3(0, 0.08f, -0.08f);
            _Back.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }


        //最后放大本体
        DropBlock.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);


        //是否扔出去
        if (_needThrow)
        {
            Rigidbody rd = DropBlock.AddComponent<Rigidbody>();
            //rd.isKinematic = true;
            rd.velocity = Eyes.transform.forward * throwForce;
        }

    }

    //切换物品
    public void ChangeBlockInHand()
    {
        GameObject Hand_Hold = managerhub.playerManager.GetHand_Hold();
        GameObject Hand = managerhub.playerManager.GetHand();
        GameObject HandBlock = managerhub.playerManager.GetHandBlock();

        byte now_HandBlock = slots[managerhub.playerManager.selectindex].blockId;

        //如果不一样，就要切换方块 或者 方块槽里有方块但是isCatchBloch = false 或者 方块槽里无方块但是却是isCatchBloch = true
        if (now_HandBlock != previous_HandBlock || (now_HandBlock != 255 && managerhub.playerManager.isCatchBlock == false) || (now_HandBlock == 255 && managerhub.playerManager.isCatchBlock == true))
        {
            //切换手
            if (now_HandBlock == 255)
            {
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
                managerhub.playerManager.isCatchBlock = false;

            }
            //切换方块
            else
            {

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
                        child.gameObject.GetComponent<SpriteRenderer>().sprite = managerhub.worldManager.blocktypes[slots[managerhub.playerManager.selectindex].blockId].top_sprit;
                    }
                    else
                    {
                        child.gameObject.GetComponent<SpriteRenderer>().sprite = managerhub.worldManager.blocktypes[slots[managerhub.playerManager.selectindex].blockId].sprite;
                    }
                    
                    index++;
                }


                //拿出Hand_Hold
                Hand_Hold.GetComponent<Animation>().Play("ChangeBlock_Up");

                //状态
                managerhub.playerManager.isCatchBlock = true;

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

