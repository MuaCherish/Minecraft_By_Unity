using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public static class BlocksFunction
{

    //��ը�㷨
    public static void Boom(ManagerHub managerhub, Vector3 _originPos, float _r)
    {

        // ��ԭʼλ��ת��Ϊ����
        _originPos = new Vector3((int)_originPos.x, (int)_originPos.y, (int)_originPos.z);

        List<EditStruct> _editNumber = new List<EditStruct>();

        // ���㷶Χ�ڵ���ʼ�ͽ�������
        int startX = Mathf.FloorToInt(_originPos.x - _r);
        int endX = Mathf.FloorToInt(_originPos.x + _r);
        int startY = Mathf.FloorToInt(_originPos.y - _r + 1);
        int endY = Mathf.FloorToInt(_originPos.y + _r);
        int startZ = Mathf.FloorToInt(_originPos.z - _r);
        int endZ = Mathf.FloorToInt(_originPos.z + _r);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    Vector3 currentPos = new Vector3(x, y, z);


                    // ���������Ƿ������ΰ뾶��Χ��
                    // �Ҳ�����ˮ
                    if (Vector3.Distance(_originPos, currentPos) <= _r && managerhub.world.GetBlockType(currentPos) != VoxelData.Water)
                    {
                        // ������������ڱ�ը��Եλ��
                        if (Vector3.Distance(_originPos, currentPos) >= (_r - 0.5f))
                        {
                            // ֻ�� 40% ������ӿ���
                            if (Random.value <= 0.4f)
                            {
                                _editNumber.Add(new EditStruct(currentPos, VoxelData.Air));
                            }
                        }
                        else
                        {
                            // �Ǳ�Եλ�ã�ֱ����ӿ���
                            _editNumber.Add(new EditStruct(currentPos, VoxelData.Air));
                        }
                    }
                }
            }
        }

        // ���� EditBlock ����������������Ϊ Air
        managerhub.world.EditBlock(_editNumber);
        //Debug.Log($"Boom:{_editNumber.Count}");
    }


    //�����㷨
    public static void Smoke(ManagerHub managerhub, Vector3 _originPos, float _r)
    {
        _originPos = new Vector3((int)_originPos.x, (int)_originPos.y, (int)_originPos.z);
        List<Vector3> directions = new List<Vector3>
    {
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, -1, 0),
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1)
    };

        Queue<Vector3> queue = new Queue<Vector3>();
        HashSet<Vector3> visited = new HashSet<Vector3>();
        List<EditStruct> _editNumber = new List<EditStruct>();

        queue.Enqueue(_originPos);
        visited.Add(_originPos);

        while (queue.Count > 0)
        {
            Vector3 currentPos = queue.Dequeue();

            // ��鵱ǰ�����Ƿ�Ϊ����
            if (!managerhub.world.blocktypes[managerhub.world.GetBlockType(currentPos)].isSolid)
            {
                _editNumber.Add(new EditStruct(currentPos, VoxelData.Wool_White)); // ��������ʵ������������滻 VoxelData.Smoke

                bool hasExpandableNeighbor = false;
                foreach (Vector3 direction in directions)
                {
                    Vector3 neighborPos = currentPos + direction;
                    // ������ڷ���δ���ʹ������ڰ뾶��Χ��
                    if (!visited.Contains(neighborPos) && Vector3.Distance(_originPos, neighborPos) <= _r)
                    {
                        if (!managerhub.world.blocktypes[managerhub.world.GetBlockType(neighborPos)].isSolid)
                        {
                            hasExpandableNeighbor = true; // �п���չ���ھ�
                            visited.Add(neighborPos);
                            queue.Enqueue(neighborPos);
                        }
                    }
                }

                // �����ǰ����û�п���չ���ھӣ����ټ�����չ
                if (!hasExpandableNeighbor)
                {
                    continue;
                }
            }
        }

        // ���� EditBlock �����������������滻Ϊ������
        //Debug.Log($"{_editNumber.Count}");
        managerhub.world.EditBlock(_editNumber, 0.3f);
    }


    
}
