using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BackPackManager : MonoBehaviour
{
    //Transforms
    public Player player;
    public World world;
    public GameObject[] icons = new GameObject[9];
    public TextMeshProUGUI[] numbers = new TextMeshProUGUI[9];

    //���ݲ���
    public Slot[] slots = new Slot[9];


    //��Ʒ���ı仯
    //����ֵ:һ�㶼��true������false���ǲ����ƻ����߷���
    //brokeOrplace:0Ϊ�ƻ���1Ϊ����
    //blocktype:���뷽������
    public void update_slots (int brokeOrplace,byte blocktype)
    {
        //broke
        if (brokeOrplace == 0)
        {
            bool isfind = false;
            int i = 0;

            foreach (Slot item in slots)
            {
                
                //����ҵ�
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
                //���û�ҵ�
                int _index = find_empty_index();
                if (_index != -1)
                {
                    slots[_index].ishave = true;
                    slots[_index].blockId = blocktype;
                    slots[_index].number++;

                    numbers[_index].text = $"{slots[_index].number}"; 

                    //���ͼƬ
                    icons[_index].GetComponent<Image>().sprite = world.blocktypes[blocktype].icon;
                    // ��ͼ���͸���ȵ���Ϊ 255
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


    //����ģʽ��Ʒ��
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


}

//��Ʒ�����ݽṹ
[System.Serializable]
public class Slot  
{
    public bool ishave = false;
    public byte blockId = 255;
    public int number = 0;
}