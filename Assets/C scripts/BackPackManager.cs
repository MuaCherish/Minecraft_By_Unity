using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackPackManager : MonoBehaviour
{
    public Player player;

    //��Ʒ��
    public Slot[] slots = new Slot[9];

    int empty_index = 0;

    //��Ʒ���ı仯
    //����ֵ:һ�㶼��true������false���ǲ����ƻ����߷���
    //brokeOrplace:0Ϊ�ƻ���1Ϊ����
    //blocktype:���뷽������
    public void update_slots (int brokeOrplace,byte blocktype)
    {
        //broke
        if (brokeOrplace == 0)
        {
            foreach (Slot item in slots)
            {
                //����ҵ�
                if (item.blockId == blocktype)
                {
                    item.number++;
                }
            }

            //���û�ҵ�
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

    //����UI
    void refresh_slotUI ()
    {
        
    }

    //�����ҵ�һ�����±��
    //�Ҳ�������-1
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


    //������������жϵ�ǰ�±���û�з���
    public bool istheindexHaveBlock(int index)
    {
        return slots[index].ishave;
    }


}

//��Ʒ�����ݽṹ
[System.Serializable]
public class Slot  
{
    public bool ishave = false;
    public byte blockId = 255;
    public int number = 0;
}