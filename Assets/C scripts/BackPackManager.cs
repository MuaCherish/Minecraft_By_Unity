using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BackPackManager : MonoBehaviour
{
    //Transforms
    [Header("Transforms")]
    public Material MAT_HandinBlock;
    public Player player;
    public World world;
    public MusicManager musicmanager;
    public GameObject[] icons = new GameObject[9];
    public TextMeshProUGUI[] numbers = new TextMeshProUGUI[9];

    //���ݲ���
    [Header("Slots")]
    public Slot[] slots = new Slot[9];

    [Header("DropBlock")]
    public float dropblock_destroyTime = 10f;
    public float absorb_Distance = 2f;
    public float drop_gravity = 1f;
    public float moveToplayer_duation = 1f;

    [Header("BlockInHand")]
    public GameObject HandInHand;
    public GameObject BlockInHand;

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


    //���������(����,����)
    public void CreateDropBox(Vector3 _pos, byte _blocktype)
    {
        //ˢ��ƫ��
        float x_offset = UnityEngine.Random.Range(2, 8) / 10f;
        float y_offset = UnityEngine.Random.Range(5, 8) / 10f;
        float z_offset = UnityEngine.Random.Range(2, 8) / 10f;

        //��������
        GameObject DropBlock = new GameObject(world.blocktypes[_blocktype].blockName);
        DropBlock.AddComponent<FloatingCube>().InitWorld(world, dropblock_destroyTime, absorb_Distance, drop_gravity, moveToplayer_duation, _blocktype, this, musicmanager);
        DropBlock.transform.SetParent(GameObject.Find("Environment/DropBlocks").transform);
        DropBlock.transform.position = new Vector3(_pos.x + x_offset, _pos.y + y_offset, _pos.z + z_offset);

        //����ͼ����ͼ��û��ͼ��icon
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


        //���Ŵ���
        DropBlock.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }


    //�л�������Ʒ��
    byte previous_HandBlock = 255;   //255�����ֱ���
    public void _ChangeBlockInHand()
    {
        byte now_HandBlock = slots[player.selectindex].blockId;
        
        //�����һ������Ҫ�л�����
        if (now_HandBlock != previous_HandBlock)
        {
            //�л���
            if (now_HandBlock == 255)
            {
                //���·���(-0.247 ~ -0.421)
                //�����(-1.2 ~ -0.7)
                StartCoroutine(Animation_HandBlock(false));

            }
            //�л�����
            else
            {
                //���·������
                if (world.blocktypes[player.selectindex].texture != null)
                {
                    MAT_HandinBlock.mainTexture = world.blocktypes[player.selectindex].texture;
                }
                else
                {
                    MAT_HandinBlock.mainTexture = world.blocktypes[VoxelData.BedRock].texture;
                }
                

                //������(-0.7 ~ -1.2)
                //�������(-0.421 ~ -0.247)
                StartCoroutine(Animation_HandBlock(true));

            }

            //update
            previous_HandBlock = now_HandBlock;
        }
    }

    public void ChangeBlockInHand()
    {

    }

    //�л�����
    IEnumerator Animation_HandBlock(bool isHandToBlock)
    {
        //�ó�����
        if (isHandToBlock)
        {
            //������(y-0.5)
            Vector3 handTargetPosition = HandInHand.transform.position + Vector3.down * 0.5f;
            yield return StartCoroutine(MoveObject(HandInHand.transform, handTargetPosition, 1.0f));

            //�ӳ�
            yield return new WaitForSeconds(1.0f);

            //�ó�block(y+0.174)
            Vector3 blockTargetPosition = BlockInHand.transform.position + Vector3.up * 0.174f;
            yield return StartCoroutine(MoveObject(BlockInHand.transform, blockTargetPosition, 1.0f));
        }
        //�ó���
        else
        {
            //����block(y-0.174)
            Vector3 blockTargetPosition = BlockInHand.transform.position + Vector3.down * 0.174f;
            yield return StartCoroutine(MoveObject(BlockInHand.transform, blockTargetPosition, 1.0f));

            //�ӳ�
            yield return new WaitForSeconds(1.0f);

            //�ó���(y+0.5)
            Vector3 handTargetPosition = HandInHand.transform.position + Vector3.up * 0.5f;
            yield return StartCoroutine(MoveObject(HandInHand.transform, handTargetPosition, 1.0f));
        }

        yield return null;
    }

    // �ƶ����嵽Ŀ��λ��
    IEnumerator MoveObject(Transform objTransform, Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0.0f;
        Vector3 startingPosition = objTransform.position;

        while (elapsedTime < duration)
        {
            objTransform.position = Vector3.Lerp(startingPosition, targetPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        objTransform.position = targetPosition; // ȷ���������յ���Ŀ��λ��
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