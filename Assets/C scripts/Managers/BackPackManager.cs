using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class BackPackManager : MonoBehaviour
{
    //Transforms
    [Header("Transforms")]
    public Material MAT_HandinBlock;
    public Player player;
    public GameObject Eyes;
    public World world;
    public MusicManager musicmanager;
    public GameObject[] icons = new GameObject[9];
    public TextMeshProUGUI[] numbers = new TextMeshProUGUI[9];

    //数据部分
    [Header("Slots")]
    public Slot[] slots = new Slot[9];

    [Header("DropBlock")]
    public float dropblock_destroyTime = 10f;
    public float absorb_Distance = 2f;
    public float drop_gravity = 1f;
    public float moveToplayer_duation = 1f;

    //切换手中物品的
    [Header("ChangingBlock")]
    public byte previous_HandBlock = 255;   //255代表手本身
    public GameObject Hand_Hold;
    public GameObject Hand;
    public GameObject HandBlock;
    public bool isChanging = false;
    public float ChangeColdTime = 1f;

    public float throwForce = 3f;
    public float ColdTime_Absorb = 1f;


    //物品栏的变化
    //返回值:一般都是true，返回false则是不能破坏或者放置
    //brokeOrplace:0为破坏，1为放置
    //blocktype:传入方块类型
    public void update_slots (int brokeOrplace,byte blocktype)
    {
        //broke
        if (brokeOrplace == 0)
        {
            bool isfind = false;
            int i = 0;

            foreach (Slot item in slots)
            {
                
                //如果找到
                if (item.blockId == blocktype)
                {
                    item.number++;
                    isfind = true;
                    numbers[i].text = $"{item.number}";
                }
                i ++;
            }

            if (!isfind)
            {
                //如果没找到
                int _index = find_empty_index();
                if (_index != -1)
                {
                    slots[_index].ishave = true;
                    slots[_index].blockId = blocktype;
                    slots[_index].number++;

                    numbers[_index].text = $"{slots[_index].number}"; 

                    //添加图片
                    icons[_index].GetComponent<Image>().sprite = world.blocktypes[blocktype].icon;
                    // 将图像的透明度调整为 255
                    icons[_index].GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                }
            }
            
        }


        //place
        else if (brokeOrplace == 1)
        { 
            if (slots[player.selectindex].number - 1 == 0)
            {
                slots[player.selectindex].ishave = false;
                slots[player.selectindex].blockId = 255;
                slots[player.selectindex].number = 0;

                numbers[player.selectindex].text = "";
                icons[player.selectindex].GetComponent<Image>().sprite = null;
                icons[player.selectindex].GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
            }
            else
            {
                slots[player.selectindex].number--;
                numbers[player.selectindex].text = $"{slots[player.selectindex].number}";
            }

            
        }
    }


    //用来找第一个空下标的
    //找不到返回-1
    int find_empty_index()
    {
        for (int i = 0;i < 9; i++)
        {
            if (slots[i].ishave == false)
            {
                return i;
            }
        }

        return -1;
    }


    //这个函数用来判断当前下标有没有方块
    public bool istheindexHaveBlock(int index)
    {
        return slots[index].ishave;
    }


    //创造模式物品栏
    public void CreativeMode()
    {
        //0. Grass
        slots[0].ishave = true;
        slots[0].blockId = VoxelData.Grass;
        icons[0].GetComponent<Image>().sprite = world.blocktypes[VoxelData.Grass].icon;
        icons[0].GetComponent<Image>().color = Color.white;

        //1. Soil
        slots[1].ishave = true;
        slots[1].blockId = VoxelData.Soil;
        icons[1].GetComponent<Image>().sprite = world.blocktypes[VoxelData.Soil].icon;
        icons[1].GetComponent<Image>().color = Color.white;

        //2. Wood
        slots[2].ishave = true;
        slots[2].blockId = VoxelData.Wood;
        icons[2].GetComponent<Image>().sprite = world.blocktypes[VoxelData.Wood].icon;
        icons[2].GetComponent<Image>().color = Color.white;

        //3. WoodenPlanks
        slots[3].ishave = true;
        slots[3].blockId = VoxelData.WoodenPlanks;
        icons[3].GetComponent<Image>().sprite = world.blocktypes[VoxelData.WoodenPlanks].icon;
        icons[3].GetComponent<Image>().color = Color.white;

        //4. Leaves
        slots[4].ishave = true;
        slots[4].blockId = VoxelData.Leaves;
        icons[4].GetComponent<Image>().sprite = world.blocktypes[VoxelData.Leaves].icon;
        icons[4].GetComponent<Image>().color = Color.white;

        //5. Stone
        slots[5].ishave = true;
        slots[5].blockId = VoxelData.Stone;
        icons[5].GetComponent<Image>().sprite = world.blocktypes[VoxelData.Stone].icon;
        icons[5].GetComponent<Image>().color = Color.white;

        //6. Glass
        slots[6].ishave = true;
        slots[6].blockId = VoxelData.Glass;
        icons[6].GetComponent<Image>().sprite = world.blocktypes[VoxelData.Glass].icon;
        icons[6].GetComponent<Image>().color = Color.white;

        //7. Water
        slots[7].ishave = true;
        slots[7].blockId = VoxelData.Water;
        icons[7].GetComponent<Image>().sprite = world.blocktypes[VoxelData.Water].icon;
        icons[7].GetComponent<Image>().color = new Color(61 / 255f,175 / 255f,1,1f);

        //8. Fluor
        slots[8].ishave = true;
        slots[8].blockId = VoxelData.Fluor;
        icons[8].GetComponent<Image>().sprite = world.blocktypes[VoxelData.Fluor].icon;
        icons[8].GetComponent<Image>().color = Color.white;
    }


    //创造掉落物(坐标,类型)
    public void CreateDropBox(Vector3 _pos, byte _blocktype, bool _needThrow, float _ColdTimeTiabsorb)
    {
        //刷新偏移
        float x_offset = UnityEngine.Random.Range(2, 8) / 10f;
        float y_offset = UnityEngine.Random.Range(5, 8) / 10f;
        float z_offset = UnityEngine.Random.Range(2, 8) / 10f;

        //创建父类
        GameObject DropBlock = new GameObject(world.blocktypes[_blocktype].blockName);
        DropBlock.AddComponent<FloatingCube>().InitWorld(world, dropblock_destroyTime, absorb_Distance, drop_gravity, moveToplayer_duation, _blocktype, this, musicmanager, _ColdTimeTiabsorb);
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
        if (world.blocktypes[_blocktype].dropsprite != null)
        {
            //Top
            GameObject _Top = new GameObject("Top");
            _Top.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].top_dropsprite;
            _Top.transform.SetParent(DropBlock.transform);
            _Top.transform.localPosition = new Vector3(0, 0.16f, 0);
            _Top.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

            //Buttom
            GameObject _Buttom = new GameObject("Buttom");
            _Buttom.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].top_dropsprite;
            _Buttom.transform.SetParent(DropBlock.transform);
            _Buttom.transform.localPosition = new Vector3(0, 0, 0);
            _Buttom.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

            //Left
            GameObject _Left = new GameObject("Left");
            _Left.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].dropsprite;
            _Left.transform.SetParent(DropBlock.transform);
            _Left.transform.localPosition = new Vector3(-0.08f, 0.08f, 0);
            _Left.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

            //Right
            GameObject _Right = new GameObject("Right");
            _Right.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].dropsprite;
            _Right.transform.SetParent(DropBlock.transform);
            _Right.transform.localPosition = new Vector3(0.08f, 0.08f, 0);
            _Right.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

            //Forward
            GameObject _Forward = new GameObject("Forward");
            _Forward.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].dropsprite;
            _Forward.transform.SetParent(DropBlock.transform);
            _Forward.transform.localPosition = new Vector3(0, 0.08f, 0.08f);
            _Forward.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

            //Back
            GameObject _Back = new GameObject("Back");
            _Back.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].dropsprite;
            _Back.transform.SetParent(DropBlock.transform);
            _Back.transform.localPosition = new Vector3(0, 0.08f, -0.08f);
            _Back.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        else
        {
            //Top
            GameObject _Top = new GameObject("Top");
            _Top.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].icon;
            _Top.transform.SetParent(DropBlock.transform);
            _Top.transform.localPosition = new Vector3(0, 0.16f, 0);
            _Top.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

            //Buttom
            GameObject _Buttom = new GameObject("Buttom");
            _Buttom.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].icon;
            _Buttom.transform.SetParent(DropBlock.transform);
            _Buttom.transform.localPosition = new Vector3(0, 0, 0);
            _Buttom.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));

            //Left
            GameObject _Left = new GameObject("Left");
            _Left.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].icon;
            _Left.transform.SetParent(DropBlock.transform);
            _Left.transform.localPosition = new Vector3(-0.08f, 0.08f, 0);
            _Left.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

            //Right
            GameObject _Right = new GameObject("Right");
            _Right.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].icon;
            _Right.transform.SetParent(DropBlock.transform);
            _Right.transform.localPosition = new Vector3(0.08f, 0.08f, 0);
            _Right.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

            //Forward
            GameObject _Forward = new GameObject("Forward");
            _Forward.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].icon;
            _Forward.transform.SetParent(DropBlock.transform);
            _Forward.transform.localPosition = new Vector3(0, 0.08f, 0.08f);
            _Forward.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

            //Back
            GameObject _Back = new GameObject("Back");
            _Back.AddComponent<SpriteRenderer>().sprite = world.blocktypes[_blocktype].icon;
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


    //按Q扔物品
    public void ThrowDropBox()
    {
        //判断是否能扔掉落物
        if (slots[player.selectindex].blockId != 255 && slots[player.selectindex].number > 0)
        {
            //创造掉落物
            CreateDropBox(Eyes.transform.forward * 0.3f +  new Vector3(Eyes.transform.position.x, Eyes.transform.position.y - 0.3f, Eyes.transform.position.z), slots[player.selectindex].blockId, true, ColdTime_Absorb);

            //物品栏减一
            update_slots(1, 0);

            ChangeBlockInHand();
        }


    }


    //切换物品
    public void ChangeBlockInHand()
    {
        byte now_HandBlock = slots[player.selectindex].blockId;

        //如果不一样，就要切换方块 或者 方块槽里有方块但是isCatchBloch = false 或者 方块槽里无方块但是却是isCatchBloch = true
        if (now_HandBlock != previous_HandBlock || (now_HandBlock != 255 && player.isCatchBlock == false) || (now_HandBlock == 255 && player.isCatchBlock == true))
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
                player.isCatchBlock = false;

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
                if (world.blocktypes[player.selectindex].texture != null)
                {
                    MAT_HandinBlock.mainTexture = world.blocktypes[slots[player.selectindex].blockId].texture;
                }
                else
                {
                    MAT_HandinBlock.mainTexture = world.blocktypes[VoxelData.BedRock].texture;
                }

                //拿出Hand_Hold
                Hand_Hold.GetComponent<Animation>().Play("ChangeBlock_Up");

                //状态
                player.isCatchBlock = true;

            }

            //update
            previous_HandBlock = now_HandBlock;
        }
    }


    


    //切换方块冷却时间
    IEnumerator ChangeBlockColdTime()
    {
        isChanging = true;

        yield return new WaitForSeconds(ChangeColdTime);

        isChanging = false;
    }


}

//物品栏数据结构
[System.Serializable]
public class Slot  
{
    public bool ishave = false;
    public byte blockId = 255;
    public int number = 0;
}