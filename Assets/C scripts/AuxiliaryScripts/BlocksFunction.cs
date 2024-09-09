using System.Collections;
using System.Collections.Generic;
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





}
