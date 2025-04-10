using Homebrew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MC_Static_Math;
using static MC_Static_Navigation;

namespace MC_Debug
{
    public class Debug_Navigate : MonoBehaviour
    {


        #region ���ں���

        ManagerHub managerhub;
        MC_Service_World Service_world;

        private void Awake()
        {
            managerhub = SceneData.GetManagerhub();
            Service_world = managerhub.Service_World;
        }

        private void Update()
        {
            if (MC_Runtime_DynamicData.instance.GetGameState() == Game_State.Playing)
            {
                _ReferUpdate_Astar();
                _ReferUpdate_RandomWalk();
            }


        }


        #endregion


        #region ��������㷨

        [Foldout("��������㷨", true)]
        [Header("ִ���㷨")] public bool Toggle_RandomWalk;
        [Header("���ƶ�̬·��")] public bool Toggle_ShowDynamicPath;
        [Header("���������߲���")] public int RandomWalk_WalkSteps = 10;  // �� N ��
        [Header("��ѭ�ϸ��������")] public float RandomWalk_prevDirectionProbability = 0f;
        [Header("��ǰ·��")] public List<Vector3> RandomWalk_ResultPath = new List<Vector3>();

        void _ReferUpdate_RandomWalk()
        {
            //��̬·��
            if (Toggle_RandomWalk)
            {
                Idle_RandomWalk(transform.position, RandomWalk_WalkSteps, RandomWalk_prevDirectionProbability, out var _result);
                RandomWalk_ResultPath = new List<Vector3>(_result);
                Toggle_RandomWalk = false;
            }

            //��̬·��
            if (Toggle_ShowDynamicPath && DynamicPathCoroutine == null)
            {
                DynamicPathCoroutine = StartCoroutine(DrawPathWithDelay());
                Toggle_ShowDynamicPath = false;
            }
            else
            {
                if (DrawOrder.Count != 0)
                    DrawOrder.Clear();
            }
        }

        /// <summary>a
        /// ��������㷨
        /// </summary>
        /// <param name="_StartPos">��ʼ��</param>
        /// <param name="_Steps">��������</param>
        /// <param name="_prevDirectionProbability">��ֱ�ߵĸ���</param>
        /// <param name="_Nodes">��������·����</param>
        void Idle_RandomWalk(Vector3 _StartPos, int _Steps, float _prevDirectionProbability, out List<Vector3> _Nodes)
        {
            // ��ʼ��
            Vector3 _thisPos = GetCenterVector3(_StartPos);
            Vector3 _thisDirect = EntityData.NearNodes[Random.Range(0, EntityData.NearNodes.Length)];
            _Nodes = new List<Vector3> { _thisPos };

            //��ǰ����-��ʼ�㱻��ס || ��ʼ������
            //��ǰ����-��ʼ�㱻��ס || ��ʼ������
            byte _StartPosBlockType = managerhub.Service_World.GetBlockType(_thisPos);
            byte _StartDownPosBlockType = managerhub.Service_World.GetBlockType(_thisPos + Vector3.down);
            if (_StartPosBlockType == 255 || MC_Runtime_StaticData.Instance.ItemData.items[_StartPosBlockType].isSolid || _StartDownPosBlockType == VoxelData.Air)
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
        bool GetOneDestination(List<Vector3> _VisitPositions, Vector3 _LastPos, Vector3 _LastDirect, float _prevDirectionProbability, out Vector3 _nextPos, out Vector3 _nextDirect)
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
        Vector3 GetNextDirect(Vector3 _LastDirect, float _prevDirectionProbability)
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

        //���ƶ�̬·��Э��
        private IEnumerator DrawPathWithDelay()
        {

            DrawOrder.Clear(); // ��ջ���˳��

            foreach (var item in RandomWalk_ResultPath)
            {
                DrawOrder.Add(item);
                yield return new WaitForSeconds(StepDelay);
            }

            DynamicPathCoroutine = null;
        }


        #endregion


        #region AStar�㷨

        [Foldout("Astar", true)]
        [Header("ִ��A*�㷨")] public bool Toggle_StartAstar;
        [Header("���ƶ�̬·��")] public bool Toggle_DynamicAstar;
        [Header("��ʼ�ڵ�")] public GameObject StartPos;
        [Header("Ŀ��ڵ�")] public GameObject EndPos;
        [Header("�����̵���󲽳�")] public int AstartMaxStep = (2 - 1) * 16 * 16;
        [Header("�����б�")] public List<AstarNode> _OpenList = new List<AstarNode>();
        [Header("�ر��б�")] public List<AstarNode> _CloseList = new List<AstarNode>();
        [Header("����·��")] public List<Vector3> _AstarResultPath = new List<Vector3>();
        Coroutine Dynamic_AstarCoroutine;
        private List<Vector3> _Astar_LineResultPath = new List<Vector3>();

        void _ReferUpdate_Astar()
        {

            //��̬·��
            if (Toggle_StartAstar)
            {
                //Chase_Astar(StartPos.transform.position, EndPos.transform.position, 4, out List<Vector3> _result);

                Algo_Astar(StartPos.transform.position, EndPos.transform.position, 4, out List<Vector3> _result);

                _Astar_LineResultPath = new List<Vector3>(_result);
                Toggle_StartAstar = false;
            }

            //��̬·��
            if (Toggle_DynamicAstar && Dynamic_AstarCoroutine == null)
            {
                Dynamic_AstarCoroutine = StartCoroutine(Chase_AstarCoroutine(StartPos.transform.position, EndPos.transform.position));
                Toggle_DynamicAstar = false;
            }
        }


        /// <summary>
        /// A* ׷���㷨
        /// </summary>
        public void Chase_Astar(Vector3 _StartPos, Vector3 _EndPos, int _RenderSize, out List<Vector3> _result)
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

            //Debug

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
                    if (CheckNodeLimit(_P, EntityData.NearNodes[i]) && !isListContainNode(_NearNode, OpenList))
                    {
                        if (!isListContainNode(_NearNode, CloseList))
                        {
                            OpenList.Add(_NearNode); UpdateOpenList(OpenList);
                        }
                    }

                }

                //Wait
                stepCount++;

            }
        }

        /// <summary>
        /// A* ׷���㷨��Э�̰�
        /// </summary>
        private IEnumerator Chase_AstarCoroutine(Vector3 _StartPos, Vector3 _EndPos)
        {
            //Clear
            _OpenList.Clear();
            _CloseList.Clear();
            _AstarResultPath.Clear();

            //��ǰ����-�����ʼ�㿨ס��������
            if (CheckSolid(_StartPos) || !CheckSolid(_StartPos + Vector3.down * 2f))
                yield break;

            //�淶�������յ�
            _StartPos = GetCenterVector3(_StartPos);
            _EndPos = GetCenterVector3(_EndPos);

            //��ʼ����ʼ�ڵ�
            int stepCount = 0;
            Vector3 _startP = GetCenterVector3(_StartPos);
            float _startG = 0f;
            float _startH = EuclideanDistance3D(_StartPos, _EndPos);
            AstarNode _startparentPos = null;
            AstarNode StartNode = new AstarNode(_startP, _startG, _startH, _startparentPos);
            _OpenList.Add(StartNode); UpdateOpenList(_OpenList);


            //һֱѭ����DeQueue�����յ� ���߳�����󲽳�
            while (_OpenList.Count > 0)
            {
                //ѡ��·��
                AstarNode _CurrentNode = _OpenList[0];

                //����CloseList
                _CloseList.Add(_CurrentNode);

                //�˳�����
                if (_CurrentNode.P == _EndPos)
                {
                    _AstarResultPath = Astar_BacktrackingPath(_CurrentNode);
                    break;
                }
                else
                {
                    _OpenList.RemoveAt(0);
                }

                // If we exceed the maximum steps, break the loop
                if (stepCount >= AstartMaxStep)
                {
                    Debug.Log("Exceeded max step count, returning the closest path.");
                    // Find the node in OpenList with the smallest F or closest to the target
                    float _MinH = _startH;
                    AstarNode closestNode = StartNode;
                    foreach (var item in _OpenList)
                    {
                        if (item.H < _MinH)
                        {
                            _MinH = item.H;
                            closestNode = item;
                        }
                    }
                    _AstarResultPath = Astar_BacktrackingPath(closestNode);
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
                    if (CheckNodeLimit(_P, EntityData.NearNodes[i]) && !isListContainNode(_NearNode, _OpenList))
                    {
                        if (!isListContainNode(_NearNode, _CloseList))
                        {
                            _OpenList.Add(_NearNode); UpdateOpenList(_OpenList);
                        }
                    }

                }

                //Wait
                stepCount++;
                yield return new WaitForSeconds(StepDelay);
            }

            Dynamic_AstarCoroutine = null;
        }

        //���ݲ��������·��
        List<Vector3> Astar_BacktrackingPath(AstarNode _EndNode)
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
        bool isListContainNode(AstarNode _node, List<AstarNode> _List)
        {
            foreach (var item in _List)
            {
                if (item.P == _node.P)
                    return true;
            }

            return false;
        }

        //ά��OpenList,F���������H����
        void UpdateOpenList(List<AstarNode> _OpenList)
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

        //ŷ������㷨
        public float EuclideanDistance3D(Vector3 pointA, Vector3 pointB)
        {
            float dx = pointA.x - pointB.x;
            float dy = pointA.y - pointB.y;
            float dz = pointA.z - pointB.z;

            return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
        }



        #endregion


        #region Gizmos

        [Foldout("Gizmos", true)]
        [Header("���ƿ��")] public float TestPathWidth = 0.3f;
        [Header("ÿ�����ӳ�ʱ��")] public float StepDelay = 0.5f; // �� Gizmos ���ӻ�ʱ�м��
        private List<Vector3> DrawOrder = new List<Vector3>(); // �����洢����˳��
        Coroutine DynamicPathCoroutine;


        private void OnDrawGizmos()
        {


            //��������㷨
            if (RandomWalk_ResultPath.Count > 0)
            {
                //��̬·��
                for (int i = 0; i < RandomWalk_ResultPath.Count - 1; i++)
                {
                    Gizmos.color = Color.cyan; // �����ߵ���ɫΪ��ɫ������Ի���������ɫ��
                    Gizmos.DrawLine(RandomWalk_ResultPath[i], RandomWalk_ResultPath[i + 1]); // ����ÿ�������ڵ�֮�������
                }

                //ʼ�ջ��������յ�
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(RandomWalk_ResultPath[0], new Vector3(TestPathWidth, TestPathWidth, TestPathWidth));
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(RandomWalk_ResultPath[RandomWalk_ResultPath.Count - 1], new Vector3(TestPathWidth, TestPathWidth, TestPathWidth));

                //���ƶ�̬·��
                if (DrawOrder.Count != 0)
                {
                    int index = 0;
                    foreach (var item in DrawOrder)
                    {
                        //��ǰ����-����Ҫ������β��ǰ���Ѿ�����
                        if (index == 0 || index == RandomWalk_ResultPath.Count - 1)
                        {
                            index++;
                            continue;
                        }


                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(item, new Vector3(TestPathWidth, TestPathWidth, TestPathWidth));
                        index++;



                    }



                }
            }

            //A*�㷨��̬·��
            if (_Astar_LineResultPath.Count > 0)
            {
                // ���� Gizmos ����ɫ
                Gizmos.color = Color.green;

                // ѭ������·����ÿһ�������ĵ㣬���Ƴ�����
                for (int i = 0; i < _Astar_LineResultPath.Count - 1; i++)
                {
                    // �ӵ�ǰ�㵽��һ�����������
                    Gizmos.DrawLine(_Astar_LineResultPath[i], _Astar_LineResultPath[i + 1]);
                }
            }

            //A*��̬�㷨
            if (_CloseList.Count > 0)
            {
                if (_AstarResultPath.Count == 0)
                {
                    //����OpenList
                    foreach (var item in _OpenList)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(item.P, new Vector3(TestPathWidth, TestPathWidth, TestPathWidth));
                    }

                    //������ѡ��·��
                    foreach (var item in _CloseList)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireCube(item.P, new Vector3(TestPathWidth, TestPathWidth, TestPathWidth));
                    }
                }
                else
                {
                    foreach (var pos in _AstarResultPath)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireCube(pos, new Vector3(TestPathWidth, TestPathWidth, TestPathWidth));
                    }
                }
            }

        }

        #endregion


        #region �㷨����

        /// <summary>
        /// �Ƿ������
        /// true:������
        /// </summary>
        /// <param name="_nextPos"></param>
        /// <param name="_nextDirect"></param>
        /// <returns></returns>
        bool CheckNodeLimit(Vector3 _nextPos, Vector3 _nextDirect)
        {
            byte _thisBlockType = managerhub.Service_World.GetBlockType(_nextPos);
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
        /// �Ƿ������ͨ
        /// </summary>
        public bool PathCanNavigate(List<Vector3> _Nodes)
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


        /// <summary>
        /// ����Vector3���鲢���������飬ͬʱ��ָ�����������λ
        /// </summary>
        /// <param name="array"></param>
        /// <param name="_FirstDirect"></param>
        /// <returns></returns>
        public Vector3[] ShuffleArray(Vector3[] array, Vector3 _FirstDirect)
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

        /// <summary>
        /// �Ƿ��ǹ���
        /// </summary>
        /// <param name="_pos"></param>
        /// <returns></returns>
        bool CheckSolid(Vector3 _pos)
        {
            byte _BlockType = managerhub.Service_World.GetBlockType(_pos);

            if (_BlockType == 255 || MC_Runtime_StaticData.Instance.ItemData.items[_BlockType].isSolid)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion


    }


}


