using System.Collections.Generic;
using UnityEngine;

public struct AlgoBlockStruct
{
    public List<EditStruct> _branch;
}


public static class BlocksFunction
{
    public static float TNT_explore_Radius = 4f;
    public static float Smoke_Radius = 2.5f;
    public static ManagerHub managerhub;


    //��ը�㷨
    public static void Boom(Vector3 _originPos)
    {
        managerhub = SceneData.GetManagerhub();

        // ��ԭʼλ��ת��Ϊ����
        _originPos = new Vector3((int)_originPos.x, (int)_originPos.y, (int)_originPos.z);

        List<EditStruct> _editNumber = new List<EditStruct>();

        // ���㷶Χ�ڵ���ʼ�ͽ�������
        int startX = Mathf.FloorToInt(_originPos.x - TNT_explore_Radius);
        int endX = Mathf.FloorToInt(_originPos.x + TNT_explore_Radius);
        int startY = Mathf.FloorToInt(_originPos.y - TNT_explore_Radius + 1);
        int endY = Mathf.FloorToInt(_originPos.y + TNT_explore_Radius);
        int startZ = Mathf.FloorToInt(_originPos.z - TNT_explore_Radius);
        int endZ = Mathf.FloorToInt(_originPos.z + TNT_explore_Radius);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    Vector3 currentPos = new Vector3(x, y, z);


                    // ���������Ƿ������ΰ뾶��Χ��
                    // �Ҳ�����ˮ
                    if (Vector3.Distance(_originPos, currentPos) <= TNT_explore_Radius && managerhub.world.GetBlockType(currentPos) != VoxelData.Water)
                    {
                        // ������������ڱ�ը��Եλ��
                        if (Vector3.Distance(_originPos, currentPos) >= (TNT_explore_Radius - 0.5f))
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
    public static void Smoke(Vector3 _originPos)
    {
        managerhub = SceneData.GetManagerhub();

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
                    if (!visited.Contains(neighborPos) && Vector3.Distance(_originPos, neighborPos) <= Smoke_Radius)
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


    //��ð뾶������TNT������
    public static void GetAllTNTPositions(Vector3 _originPos, out List<Vector3> positions)
    {

        managerhub = SceneData.GetManagerhub();

        positions = new List<Vector3>();

        // ��ԭʼλ��ת��Ϊ����
        _originPos = new Vector3((int)_originPos.x, (int)_originPos.y, (int)_originPos.z);

        // ���㷶Χ�ڵ���ʼ�ͽ�������
        int startX = Mathf.FloorToInt(_originPos.x - TNT_explore_Radius);
        int endX = Mathf.FloorToInt(_originPos.x + TNT_explore_Radius);
        int startY = Mathf.FloorToInt(_originPos.y - TNT_explore_Radius);
        int endY = Mathf.FloorToInt(_originPos.y + TNT_explore_Radius);
        int startZ = Mathf.FloorToInt(_originPos.z - TNT_explore_Radius);
        int endZ = Mathf.FloorToInt(_originPos.z + TNT_explore_Radius);

        // ����ָ����Χ�ڵ�����
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    Vector3 currentPos = new Vector3(x, y, z);

                    // ��������Ƿ���ָ���뾶��
                    if (Vector3.Distance(_originPos, currentPos) <= TNT_explore_Radius)
                    {
                        // ����λ�õķ��������Ƿ�Ϊ TNT
                        if (managerhub.world.GetBlockType(currentPos) == VoxelData.TNT)
                        {
                            positions.Add(currentPos);
                        }
                    }
                }
            }
        }
    }


    //6��ָ������
    //public static readonly Vector3[] faceChecks = new Vector3[6]
    //{

    //    new Vector3(0.0f, 0.0f, -1.0f), //Back
    //    new Vector3(0.0f, 0.0f, 1.0f),  //Front
    //    new Vector3(0.0f, 1.0f, 0.0f),  //Up
    //    new Vector3(0.0f, -1.0f, 0.0f), //Down
    //    new Vector3(-1.0f, 0.0f, 0.0f), //Left
    //    new Vector3(1.0f, 0.0f, 0.0f),  //Right

    //};

    /// <summary>
    /// ������ʼ�㣬����һ��Ŀ���
    /// </summary>
    /// <param name="_StartPos"></param>
    /// <returns></returns>
    //public static Vector3 GetNavigationDestination(Vector3 _StartPos)
    //{
    //    Vector3 _EndPos = Vector3.zero;
    //    World world = SceneData.GetManagerhub().world;

    //    //��_StartPos���ڷ����������
    //    GetCenterVector3(_StartPos);

    //    //���4������ 
    //    for (int i = 0; i < 6; i ++)
    //    {
    //        Vector3 _TargetPos = _StartPos + VoxelData.faceChecks[i];

    //        //��ǰ����-û�з�������
    //        if (world.GetBlockType(_TargetPos) == 255)
    //            continue;

    //        bool _TargetPos_isSolid = world.blocktypes[world.GetBlockType(_TargetPos)].isSolid;

            
    //    }





    //    return _EndPos;
    //}

}


