using System.Collections.Generic;
using UnityEngine;

public static class MC_UtilityFunctions
{

    #region ��С�� 

    public static float Delta = 0.01f;
    public static float Delta_Pro = 0.0125f;

    #endregion


    #region Enum

    /// <summary>
    /// ����
    /// </summary>
    public enum BlockDirection
    {
        ǰ,
        ��,
        ��,
        ��,
        ��,
        ��
    }

    #endregion


    #region BlockType

    /// <summary>
    /// �����������꣬���������͡�<para/>
    /// �������Զ�����״��
    /// </summary>
    /// <param name="_pos">��������</param>
    /// <returns></returns>
    public static byte GetBlockType(Vector3 _pos)
    {
        return 0;
    }

    /// <summary>
    /// World
    /// </summary>
    public static World world
    {
        get
        {
            return SceneData.GetWorld();
        }

    }

    /// <summary>
    /// �Ƿ��ǹ���
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public static bool CheckSolid(Vector3 _pos)
    {
        byte _BlockType = world.GetBlockType(_pos);

        if (_BlockType == 255 || world.blocktypes[_BlockType].isSolid)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    #endregion


    #region ���

    /// <summary>
    /// ���Ϊ true�������ز��̶���ꣻ���Ϊ false������ʾ���������
    /// </summary>
    /// <param name="isLocked">���Ϊ true�������ز��̶���ꣻ���Ϊ false������ʾ���������</param>
    public static void LockMouse(bool isLocked)
    {
        if (isLocked)
        {
            //print("�������");
            // ��������겢������������Ļ����
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            //print("��ʾ���");
            // ��ʾ����겢�������
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }


    #endregion


    #region �����ж�

    /// <summary>
    /// ���Ի�����������ж��Ƿ����
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public static bool isOutOfChunkRange(Vector3 _pos)
    {
        //��ȡ�������
        Vector3 _vec = GetRelaPos(_pos);

        //�Ƿ����
        int _x = (int)_vec.x;
        int _y = (int)_vec.y;
        int _z = (int)_vec.z;

        if (_x < 0 || _x > TerrainData.ChunkWidth - 1 || _y < 0 || _y > TerrainData.ChunkHeight - 1 || _z < 0 || _z > TerrainData.ChunkWidth - 1)
            return true;
        else
            return false;
    }

    /// <summary>
    /// ����ֵ�ж�
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_y"></param>
    /// <param name="_z"></param>
    /// <returns></returns>
    public static bool isOutOfChunkRange(int _x, int _y, int _z)
    {
        //�Ƿ����
        if (_x < 0 || _x > TerrainData.ChunkWidth - 1 || _y < 0 || _y > TerrainData.ChunkHeight - 1 || _z < 0 || _z > TerrainData.ChunkWidth - 1)
            return true;
        else
            return false;

    }


    #endregion


    #region ���ȡ��

    /// <summary>
    /// ���������������ˮƽԲ�������ѡ��
    /// </summary>
    /// <param name="center">Բ������</param>
    /// <param name="radius">Բ�İ뾶</param>
    /// <returns>����������ɵ�ˮƽ��(Vector3)</returns>
    public static Vector3 GetRandomPointInCircle(Vector3 center, float radius)
    {
        // �������һ���Ƕȣ������ƣ�
        float angle = Random.Range(0f, Mathf.PI * 2);

        // �������һ�����룬ȷ���ڰ뾶��Χ��
        float randomRadius = Random.Range(0f, radius);

        // �����������ˮƽƽ���ϵ�x��z����
        float xOffset = Mathf.Cos(angle) * randomRadius;
        float zOffset = Mathf.Sin(angle) * randomRadius;

        // ������������������
        return new Vector3(center.x + xOffset, center.y, center.z + zOffset);
    }


    #endregion


    #region Probability

    /// <summary>
    /// ���ظ���ֵ
    /// </summary>
    /// <param name="_Probability"></param>
    /// <returns></returns>
    public static bool GetProbability(float _Probability)
    {
        // ȷ������ֵ�� 0 �� 100 ֮��
        _Probability = Mathf.Clamp(_Probability, 0, 100);

        // ����һ�� 0 �� 100 ֮��������
        float randomValue = UnityEngine.Random.Range(0f, 100f);

        // ��������С�ڵ�������ֵ���򷵻� true
        //Debug.Log(randomValue);
        bool a = randomValue < _Probability;

        return a;
    }

    #endregion


    #region Vector3

    /// <summary>
    /// ����������ɾ�������
    /// </summary>
    /// <param name="_vec"></param>
    /// <returns></returns>
    public static Vector3 GetRealPos(Vector3 _vec, Vector3 _ChunkLocation)
    {
        return Vector3.zero;
    }

    /// <summary>
    /// �����������Ϊ�������
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public static Vector3 GetRelaPos(Vector3 _pos)
    {
        return new Vector3(Mathf.FloorToInt(_pos.x % TerrainData.ChunkWidth), Mathf.FloorToInt(_pos.y) % TerrainData.ChunkHeight, Mathf.FloorToInt(_pos.z % TerrainData.ChunkWidth));
    }

    /// <summary>
    /// ����Int���͵�Vector3
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public static Vector3 GetIntVector3(Vector3 _pos)
    {
        return new Vector3((int)_pos.x, (int)_pos.y, (int)_pos.z);
    }

    /// <summary>
    /// ��������������꣬���������ڷ�����������꣬ע��ֻ��0.5f
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public static Vector3 GetCenterVector3(Vector3 _pos)
    {
        return new Vector3((int)_pos.x + 0.5f, (int)_pos.y + 0.5f, (int)_pos.z + 0.5f);
    }


    /// <summary>
    /// ŷ������㷨
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <returns></returns>
    public static float EuclideanDistance3D(Vector3 pointA, Vector3 pointB)
    {
        float dx = pointA.x - pointB.x;
        float dy = pointA.y - pointB.y;
        float dz = pointA.z - pointB.z;

        return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// ����Vector3���鲢���������飬ͬʱ��ָ�����������λ
    /// </summary>
    /// <param name="array"></param>
    /// <param name="_FirstDirect"></param>
    /// <returns></returns>
    public static Vector3[] ShuffleArray(Vector3[] array, Vector3 _FirstDirect)
    {
        // �������飬�����޸�ԭ����
        Vector3[] shuffledArray = (Vector3[])array.Clone();

        // ����_FirstDirect�������е�����λ��
        int firstDirectIndex = System.Array.IndexOf(shuffledArray, _FirstDirect);

        // ����ҵ���ָ���ķ��򣬾ͽ����Ƶ��������λ
        if (firstDirectIndex >= 0)
        {
            // �� _FirstDirect �ŵ��������λ
            Vector3 temp = shuffledArray[firstDirectIndex];
            shuffledArray[firstDirectIndex] = shuffledArray[0];
            shuffledArray[0] = temp;
        }

        // ϴ��ʣ�ಿ�֣�������1��ʼϴ�ƣ�
        System.Random rng = new System.Random();
        for (int i = shuffledArray.Length - 1; i > 1; i--)  // �ӵڶ���Ԫ�ؿ�ʼϴ��
        {
            int j = rng.Next(1, i + 1);  // ���� 1 �� i ֮����������
                                         // ����Ԫ��
            (shuffledArray[i], shuffledArray[j]) = (shuffledArray[j], shuffledArray[i]);
        }

        return shuffledArray;
    }


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