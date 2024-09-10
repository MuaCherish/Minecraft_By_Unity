using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public static class BlocksFunction
{


    //爆炸算法
    public static void Boom(ManagerHub managerhub, Vector3 _originPos, float _r)
    {
        _originPos = new Vector3((int)_originPos.x, (int)_originPos.y,(int)_originPos.z);
        // 计算范围内的起始和结束坐标
        int startX = Mathf.FloorToInt(_originPos.x - _r);
        int endX = Mathf.FloorToInt(_originPos.x + _r);
        int startY = Mathf.FloorToInt(_originPos.y - _r);
        int endY = Mathf.FloorToInt(_originPos.y + _r);
        int startZ = Mathf.FloorToInt(_originPos.z - _r);
        int endZ = Mathf.FloorToInt(_originPos.z + _r);

        List<EditStruct> _editNumber = new List<EditStruct>();

        // 遍历范围内的所有坐标
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    Vector3 currentPos = new Vector3(x, y, z);

                    // 检查该坐标是否在球形半径范围内
                    if (Vector3.Distance(_originPos, currentPos) <= _r)
                    {
                        _editNumber.Add(new EditStruct(currentPos,VoxelData.Air));
                    }
                }
            }
        }

        // 调用EditBlock函数，将坐标设置为Air
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

            // 检查当前方块是否为空气
            if (!managerhub.worldManager.blocktypes[managerhub.worldManager.GetBlockType(currentPos)].isSolid)
            {
                _editNumber.Add(new EditStruct(currentPos, VoxelData.Snow)); // 你可以用适当的烟雾体素替换 VoxelData.Smoke

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

        // 调用 EditBlock 函数，将空气方块替换为烟雾方块
        //Debug.Log($"{_editNumber.Count}");
        managerhub.worldManager.EditBlock(_editNumber,0.1f);
    }



}
