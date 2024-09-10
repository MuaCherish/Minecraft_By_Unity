using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public static class BlocksFunction
{


    //��ը�㷨
    public static void Boom(ManagerHub managerhub, Vector3 _originPos, float _r)
    {
        _originPos = new Vector3((int)_originPos.x, (int)_originPos.y,(int)_originPos.z);
        // ���㷶Χ�ڵ���ʼ�ͽ�������
        int startX = Mathf.FloorToInt(_originPos.x - _r);
        int endX = Mathf.FloorToInt(_originPos.x + _r);
        int startY = Mathf.FloorToInt(_originPos.y - _r);
        int endY = Mathf.FloorToInt(_originPos.y + _r);
        int startZ = Mathf.FloorToInt(_originPos.z - _r);
        int endZ = Mathf.FloorToInt(_originPos.z + _r);

        List<EditStruct> _editNumber = new List<EditStruct>();

        // ������Χ�ڵ���������
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    Vector3 currentPos = new Vector3(x, y, z);

                    // ���������Ƿ������ΰ뾶��Χ��
                    if (Vector3.Distance(_originPos, currentPos) <= _r)
                    {
                        _editNumber.Add(new EditStruct(currentPos,VoxelData.Air));
                    }
                }
            }
        }

        // ����EditBlock����������������ΪAir
        managerhub.worldManager.EditBlock(_editNumber);
        //Debug.Log($"Boom:{_editNumber.Count}");
    }


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
            if (!managerhub.worldManager.blocktypes[managerhub.worldManager.GetBlockType(currentPos)].isSolid)
            {
                _editNumber.Add(new EditStruct(currentPos, VoxelData.Snow)); // ��������ʵ������������滻 VoxelData.Smoke

                foreach (Vector3 direction in directions)
                {
                    Vector3 neighborPos = currentPos + direction;
                    if (!visited.Contains(neighborPos) && Vector3.Distance(_originPos, neighborPos) <= _r)
                    {
                        visited.Add(neighborPos);
                        queue.Enqueue(neighborPos);
                    }
                }
            }
        }

        // ���� EditBlock �����������������滻Ϊ������
        //Debug.Log($"{_editNumber.Count}");
        managerhub.worldManager.EditBlock(_editNumber,0.1f);
    }



}
