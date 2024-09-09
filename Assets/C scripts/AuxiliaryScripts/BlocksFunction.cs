using System.Collections;
using System.Collections.Generic;
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





}
