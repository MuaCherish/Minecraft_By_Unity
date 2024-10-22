using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public static class BlocksFunction
{

    //爆炸算法
    public static void Boom(ManagerHub managerhub, Vector3 _originPos, float _r)
    {

        // 将原始位置转换为整数
        _originPos = new Vector3((int)_originPos.x, (int)_originPos.y, (int)_originPos.z);

        List<EditStruct> _editNumber = new List<EditStruct>();

        // 计算范围内的起始和结束坐标
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


                    // 检查该坐标是否在球形半径范围内
                    // 且不能是水
                    if (Vector3.Distance(_originPos, currentPos) <= _r && managerhub.world.GetBlockType(currentPos) != VoxelData.Water)
                    {
                        // 如果该坐标属于爆炸边缘位置
                        if (Vector3.Distance(_originPos, currentPos) >= (_r - 0.5f))
                        {
                            // 只有 40% 几率添加空气
                            if (Random.value <= 0.4f)
                            {
                                _editNumber.Add(new EditStruct(currentPos, VoxelData.Air));
                            }
                        }
                        else
                        {
                            // 非边缘位置，直接添加空气
                            _editNumber.Add(new EditStruct(currentPos, VoxelData.Air));
                        }
                    }
                }
            }
        }

        // 调用 EditBlock 函数，将坐标设置为 Air
        managerhub.world.EditBlock(_editNumber);
        //Debug.Log($"Boom:{_editNumber.Count}");
    }


    //烟雾算法
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
            if (!managerhub.world.blocktypes[managerhub.world.GetBlockType(currentPos)].isSolid)
            {
                _editNumber.Add(new EditStruct(currentPos, VoxelData.Wool_White)); // 你可以用适当的烟雾体素替换 VoxelData.Smoke

                bool hasExpandableNeighbor = false;
                foreach (Vector3 direction in directions)
                {
                    Vector3 neighborPos = currentPos + direction;
                    // 如果相邻方块未访问过并且在半径范围内
                    if (!visited.Contains(neighborPos) && Vector3.Distance(_originPos, neighborPos) <= _r)
                    {
                        if (!managerhub.world.blocktypes[managerhub.world.GetBlockType(neighborPos)].isSolid)
                        {
                            hasExpandableNeighbor = true; // 有可扩展的邻居
                            visited.Add(neighborPos);
                            queue.Enqueue(neighborPos);
                        }
                    }
                }

                // 如果当前方块没有可扩展的邻居，不再继续扩展
                if (!hasExpandableNeighbor)
                {
                    continue;
                }
            }
        }

        // 调用 EditBlock 函数，将空气方块替换为烟雾方块
        //Debug.Log($"{_editNumber.Count}");
        managerhub.world.EditBlock(_editNumber, 0.3f);
    }


    
}
