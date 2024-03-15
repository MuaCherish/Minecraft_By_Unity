using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackPackManager : MonoBehaviour
{
    public Player player;

    //物品栏
    public Slot[] slots = new Slot[9];

    int empty_index = 0;

    //物品栏的变化
    //返回值:一般都是true，返回false则是不能破坏或者放置
    //brokeOrplace:0为破坏，1为放置
    //blocktype:传入方块类型
    public void update_slots (int brokeOrplace,byte blocktype)
    {
        //broke
        if (brokeOrplace == 0)
        {
            foreach (Slot item in slots)
            {
                //如果找到
                if (item.blockId == blocktype)
                {
                    item.number++;
                }
            }

            //如果没找到
            int _index = find_empty_index();
            if (_index != -1)
            {
                slots[_index].ishave = true;
                slots[_index].blockId = blocktype;
                slots[_index].number++;
            }
        }


        //place
        else if (brokeOrplace == 1)
        {
            slots[player.selectindex].number--;
        }
    }

    //更新UI
    void refresh_slotUI ()
    {
        
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


}

//物品栏数据结构
[System.Serializable]
public class Slot  
{
    public bool ishave = false;
    public byte blockId = 255;
    public int number = 0;
}