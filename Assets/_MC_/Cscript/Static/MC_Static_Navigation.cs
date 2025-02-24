using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MC_Static_Math;
using static MC_Static_World;
public static class MC_Static_Navigation
{


    #region World

    /// <summary>
    /// World
    /// </summary>
    public static World world
    {
        get
        {
            if (_world == null)
            {
                _world = SceneData.GetManagerhub().world; // �ڵ�һ�η���ʱ����
            }
            return _world;
        }
    }
    private static World _world;  // ��̬�����洢 World ʵ��


    #endregion


    #region Path


    /// <summary>
    /// �Ƿ������
    /// true:������
    /// </summary>
    /// <param name="_nextPos"></param>
    /// <param name="_nextDirect"></param>
    /// <returns></returns>
    public static bool CheckNodeLimit(Vector3 _nextPos, Vector3 _nextDirect)
    {
        byte _thisBlockType = world.GetBlockType(_nextPos);
        Vector3 _LastPos = _nextPos - _nextDirect;

        //��ǰ����-���û�з�������
        if (_thisBlockType == 255)
            return false;

        //��ǰ����-����Ƿ���
        if (CheckSolid(_nextPos))
            return false;

        //��ǰ����-���nextPos����
        if (!CheckSolid(_nextPos + Vector3.down))
            return false;

        //��ǰ����-���ֱ�Ǳ�ռ������Խ��߷�����Ч
        if (_nextDirect.x != 0 && _nextDirect.z != 0)
        {
            if (CheckSolid(_LastPos + new Vector3(_nextDirect.x, _nextDirect.y, 0f)) &&
                CheckSolid(_LastPos + new Vector3(0f, _nextDirect.y, _nextDirect.z)))
            {
                return false;
            }
        }

        //ͨ�����м��
        return true;
    }


    /// <summary>
    /// �Ƿ������ͨ·��
    /// </summary>
    public static bool PathCanNavigate(List<Vector3> _Nodes)
    {
        foreach (var _node in _Nodes)
        {
            if (CheckSolid(_node))
            {
                //�߲�ͨ
                return false;
            }
        }

        //������ͨ
        return true;
    }





    #region RandomWalk�㷨


    /// <summary>a
    /// ��������㷨
    /// </summary>
    /// <param name="_StartPos">��ʼ��</param>
    /// <param name="_Steps">��������</param>
    /// <param name="_prevDirectionProbability">��ֱ�ߵĸ���</param>
    /// <param name="_Nodes">��������·����</param>
    public static void Algo_RandomWalk(Vector3 _StartPos, int _Steps, float _prevDirectionProbability, out Vector3 _TargetPos)
    {
        // ��ʼ��
        Vector3 _thisPos = GetCenterVector3(_StartPos);
        Vector3 _thisDirect = EntityData.NearNodes[Random.Range(0, EntityData.NearNodes.Length)];
        List<Vector3> _Nodes = new List<Vector3> { _thisPos };
        _TargetPos = Vector3.zero;

        //��ǰ����-��ʼ�㱻��ס || ��ʼ������
        byte _StartPosBlockType = world.GetBlockType(_thisPos);
        byte _StartDownPosBlockType = world.GetBlockType(_thisPos + Vector3.down);
        if (_StartPosBlockType == 255 || world.blocktypes[_StartPosBlockType].isSolid || _StartDownPosBlockType == VoxelData.Air)
            return;

        // ���� N ��
        for (int i = 0; i < _Steps; i++)
        {
            // ������һ��Ŀ��λ��
            //���������ɶҲû�ҵ�����ֱ�ӽ�������
            if (!GetOneDestination(_Nodes, _thisPos, _thisDirect, _prevDirectionProbability, out var _nextPos, out var _nextDirect))
            {
                _TargetPos = _Nodes[_Nodes.Count - 1];
                break;
            }

            // ����λ�úͷ���
            _thisPos = _nextPos;
            _thisDirect = _nextDirect;

            // ������һ��λ��
            _Nodes.Add(_nextPos);
        }
    }


    /// <summary>a
    /// ��������㷨
    /// </summary>
    /// <param name="_StartPos">��ʼ��</param>
    /// <param name="_Steps">��������</param>
    /// <param name="_prevDirectionProbability">��ֱ�ߵĸ���</param>
    /// <param name="_Nodes">��������·����</param>
    public static void Algo_RandomWalk(Vector3 _StartPos, int _Steps, float _prevDirectionProbability, out List<Vector3> _Nodes)
    {
        // ��ʼ��
        Vector3 _thisPos = GetCenterVector3(_StartPos);
        Vector3 _thisDirect = EntityData.NearNodes[Random.Range(0, EntityData.NearNodes.Length)];
        _Nodes = new List<Vector3> { _thisPos };

        //��ǰ����-��ʼ�㱻��ס || ��ʼ������
        byte _StartPosBlockType = world.GetBlockType(_thisPos);
        byte _StartDownPosBlockType = world.GetBlockType(_thisPos + Vector3.down);
        if (_StartPosBlockType == 255 || world.blocktypes[_StartPosBlockType].isSolid || _StartDownPosBlockType == VoxelData.Air)
            return;

        // ���� N ��
        for (int i = 0; i < _Steps; i++)
        {
            // ������һ��Ŀ��λ��
            //���������ɶҲû�ҵ�����ֱ�ӽ�������
            if (!GetOneDestination(_Nodes, _thisPos, _thisDirect, _prevDirectionProbability, out var _nextPos, out var _nextDirect))
                break;

            // ����λ�úͷ���
            _thisPos = _nextPos;
            _thisDirect = _nextDirect;

            // ������һ��λ��
            _Nodes.Add(_nextPos);
        }
    }

    // Ѱ�ҵ����㷶Χ�ڵ���һ����
    // true:�����ҵ���һ����
    private static bool GetOneDestination(List<Vector3> _VisitPositions, Vector3 _LastPos, Vector3 _LastDirect, float _prevDirectionProbability, out Vector3 _nextPos, out Vector3 _nextDirect)
    {

        //��ʼ��
        bool NoWayToNavigate = true;
        _nextPos = Vector3.zero;
        _nextDirect = GetNextDirect(_LastDirect, _prevDirectionProbability);

        //�������з���_nextDirect����λ
        Vector3[] shuffledDirections = ShuffleArray(EntityData.NearNodes, _nextDirect);  //ϴ������
        foreach (var _direct in shuffledDirections)
        {
            //Update
            _nextPos = _LastPos + _direct;
            _nextDirect = _direct;

            //��ǰ����-������
            if (CheckNodeLimit(_nextPos, _nextDirect) && !_VisitPositions.Contains(_nextPos))
            {
                NoWayToNavigate = false;
                break;
            }

        }

        //�ж��Ƿ���Ͷ��· 
        if (NoWayToNavigate)
        {
            //print("�Ѿ���·����");
            return false;
        }
        else
        {
            //��������
            return true;
        }

    }

    //ȷ���������
    private static Vector3 GetNextDirect(Vector3 _LastDirect, float _prevDirectionProbability)
    {
        if (Random.value < _prevDirectionProbability)
        {
            return _LastDirect;
        }
        else
        {
            return EntityData.NearNodes[Random.Range(0, EntityData.NearNodes.Length)];
        }
    }


    #endregion


    #region Astar�㷨

    /// <summary>
    /// A*Ѱ·�㷨������һ����·��
    /// </summary>
    public static void Algo_Astar(Vector3 _StartPos, Vector3 _EndPos, int _RenderSize, out List<Vector3> _result)
    {
        //Clear
        _result = new List<Vector3>();

        //��ǰ����-�����ʼ�㿨ס��������
        if (CheckSolid(_StartPos) || !CheckSolid(_StartPos + Vector3.down * 2f))
            return;

        //�淶�������յ�
        _StartPos = GetCenterVector3(_StartPos);
        _EndPos = GetCenterVector3(_EndPos);
        int maxStep = _RenderSize * (int)(_EndPos - _StartPos).magnitude * TerrainData.ChunkWidth;

        //���������б� / �ر��б�
        List<AstarNode> OpenList = new List<AstarNode>();
        List<AstarNode> CloseList = new List<AstarNode>();

        //��ʼ����ʼ�ڵ�
        int stepCount = 0;
        Vector3 _startP = GetCenterVector3(_StartPos);
        float _startG = 0f;
        float _startH = EuclideanDistance3D(_StartPos, _EndPos);
        AstarNode _startparentPos = null;
        AstarNode StartNode = new AstarNode(_startP, _startG, _startH, _startparentPos);
        OpenList.Add(StartNode); UpdateOpenList(OpenList);

        //һֱѭ����DeQueue�����յ� ���߳�����󲽳�
        while (OpenList.Count > 0)
        {
            //ѡ��·��
            AstarNode _CurrentNode = OpenList[0];

            //����CloseList
            CloseList.Add(_CurrentNode);

            //�˳�����-�ҵ��յ�
            if (_CurrentNode.P == _EndPos)
            {
                _result = Astar_BacktrackingPath(_CurrentNode);
                break;
            }
            else
            {
                OpenList.RemoveAt(0);
            }

            //�˳�����-���������������򷵻ؾ����յ������ֵ
            if (stepCount >= maxStep)
            {
                Debug.Log("Exceeded max step count, returning the closest path.");
                // Find the node in OpenList with the smallest F or closest to the target
                float _MinH = _startH;
                AstarNode closestNode = StartNode;
                foreach (var item in OpenList)
                {
                    if (item.H < _MinH)
                    {
                        _MinH = item.H;
                        closestNode = item;
                    }
                }
                _result = Astar_BacktrackingPath(closestNode);
                break;
            }

            //����OpenList
            for (int i = 0; i < EntityData.NearNodes.Length; i++)
            {
                Vector3 _P = _CurrentNode.P + EntityData.NearNodes[i];
                float _G = _CurrentNode.G + EntityData.NearNodes[i].magnitude;
                float _H = EuclideanDistance3D(_P, _EndPos);
                AstarNode _parentPos = _CurrentNode;

                AstarNode _NearNode = new AstarNode(_P, _G, _H, _parentPos);

                //�����δ��¼�ڵ㣬�����OpenList
                if (CheckNodeLimit(_P, EntityData.NearNodes[i]) && !isListContainNode(_NearNode, OpenList) && !isListContainNode(_NearNode, CloseList))
                    OpenList.Add(_NearNode); UpdateOpenList(OpenList);

            }

            //Wait
            stepCount++;

        }
    }

    //ά��OpenList,F���������H����
    private static void UpdateOpenList(List<AstarNode> _OpenList)
    {
        // ʹ�� List.Sort ������ F �� H ����
        _OpenList.Sort((node1, node2) =>
        {
            // ���ȱȽ� F ֵ
            int fComparison = node1.F.CompareTo(node2.F);

            // ��� F ֵ��ͬ���ٱȽ� H ֵ 
            if (fComparison == 0)
            {
                return node1.H.CompareTo(node2.H);
            }

            return fComparison;
        });
    }

    //���ݲ��������·��
    private static List<Vector3> Astar_BacktrackingPath(AstarNode _EndNode)
    {
        AstarNode _nextNode = _EndNode;
        List<Vector3> result = new List<Vector3>();

        // ֻҪ parentNode ��Ϊ null���ͼ�������
        while (_nextNode.parentNode != null)
        {
            result.Add(_nextNode.P);
            _nextNode = _nextNode.parentNode;
        }

        // ��ת·��˳��
        result.Reverse();

        return result; // ���ط�ת���·��
    }

    //�б��Ƿ����Astar�ڵ�
    private static bool isListContainNode(AstarNode _node, List<AstarNode> _List)
    {
        foreach (var item in _List)
        {
            if (item.P == _node.P)
                return true;
        }

        return false;
    }



    #endregion


    #endregion


}



//Astar���ݽṹ
[System.Serializable]
public class AstarNode
{
    public Vector3 P;
    public float G; // ·������
    public float H; // Ԥ������
    public float F; // ������
    public AstarNode parentNode;

    public AstarNode(Vector3 _currentPos, float _G, float _H, AstarNode _parentNode)
    {
        P = _currentPos;
        G = _G;
        H = _H;
        F = G + H;
        parentNode = _parentNode;
    }


}
