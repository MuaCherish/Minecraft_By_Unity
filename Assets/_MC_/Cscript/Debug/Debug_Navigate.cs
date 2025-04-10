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


        #region 周期函数

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


        #region 随机游走算法

        [Foldout("随机游走算法", true)]
        [Header("执行算法")] public bool Toggle_RandomWalk;
        [Header("绘制动态路线")] public bool Toggle_ShowDynamicPath;
        [Header("最大随机游走步数")] public int RandomWalk_WalkSteps = 10;  // 走 N 轮
        [Header("遵循上个方向概率")] public float RandomWalk_prevDirectionProbability = 0f;
        [Header("当前路径")] public List<Vector3> RandomWalk_ResultPath = new List<Vector3>();

        void _ReferUpdate_RandomWalk()
        {
            //静态路径
            if (Toggle_RandomWalk)
            {
                Idle_RandomWalk(transform.position, RandomWalk_WalkSteps, RandomWalk_prevDirectionProbability, out var _result);
                RandomWalk_ResultPath = new List<Vector3>(_result);
                Toggle_RandomWalk = false;
            }

            //动态路径
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
        /// 随机游走算法
        /// </summary>
        /// <param name="_StartPos">初始点</param>
        /// <param name="_Steps">迭代次数</param>
        /// <param name="_prevDirectionProbability">走直线的概率</param>
        /// <param name="_Nodes">返回所有路径点</param>
        void Idle_RandomWalk(Vector3 _StartPos, int _Steps, float _prevDirectionProbability, out List<Vector3> _Nodes)
        {
            // 初始化
            Vector3 _thisPos = GetCenterVector3(_StartPos);
            Vector3 _thisDirect = EntityData.NearNodes[Random.Range(0, EntityData.NearNodes.Length)];
            _Nodes = new List<Vector3> { _thisPos };

            //提前返回-起始点被堵住 || 起始点悬空
            //提前返回-起始点被堵住 || 起始点悬空
            byte _StartPosBlockType = managerhub.Service_World.GetBlockType(_thisPos);
            byte _StartDownPosBlockType = managerhub.Service_World.GetBlockType(_thisPos + Vector3.down);
            if (_StartPosBlockType == 255 || MC_Runtime_StaticData.Instance.ItemData.items[_StartPosBlockType].isSolid || _StartDownPosBlockType == VoxelData.Air)
                return;

            // 迭代 N 步
            for (int i = 0; i < _Steps; i++)
            {
                // 计算下一个目标位置
                //如果单个点啥也没找到，则直接结束迭代
                if (!GetOneDestination(_Nodes, _thisPos, _thisDirect, _prevDirectionProbability, out var _nextPos, out var _nextDirect))
                    break;

                // 更新位置和方向
                _thisPos = _nextPos;
                _thisDirect = _nextDirect;

                // 保存下一个位置
                _Nodes.Add(_nextPos);
            }
        }

        // 寻找单个点范围内的下一个点
        // true:可以找到下一个点
        bool GetOneDestination(List<Vector3> _VisitPositions, Vector3 _LastPos, Vector3 _LastDirect, float _prevDirectionProbability, out Vector3 _nextPos, out Vector3 _nextDirect)
        {

            //初始化
            bool NoWayToNavigate = true;
            _nextPos = Vector3.zero;
            _nextDirect = GetNextDirect(_LastDirect, _prevDirectionProbability);

            //遍历所有方向，_nextDirect在首位
            Vector3[] shuffledDirections = ShuffleArray(EntityData.NearNodes, _nextDirect);  //洗牌数组
            foreach (var _direct in shuffledDirections)
            {
                //Update
                _nextPos = _LastPos + _direct;
                _nextDirect = _direct;

                //提前返回-可以走
                if (CheckNodeLimit(_nextPos, _nextDirect) && !_VisitPositions.Contains(_nextPos))
                {
                    NoWayToNavigate = false;
                    break;
                }

            }

            //判断是否走投无路 
            if (NoWayToNavigate)
            {
                //print("已经无路可走");
                return false;
            }
            else
            {
                //还可以走
                return true;
            }

        }

        //确定方向概率
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

        //绘制动态路径协程
        private IEnumerator DrawPathWithDelay()
        {

            DrawOrder.Clear(); // 清空绘制顺序

            foreach (var item in RandomWalk_ResultPath)
            {
                DrawOrder.Add(item);
                yield return new WaitForSeconds(StepDelay);
            }

            DynamicPathCoroutine = null;
        }


        #endregion


        #region AStar算法

        [Foldout("Astar", true)]
        [Header("执行A*算法")] public bool Toggle_StartAstar;
        [Header("绘制动态路线")] public bool Toggle_DynamicAstar;
        [Header("起始节点")] public GameObject StartPos;
        [Header("目标节点")] public GameObject EndPos;
        [Header("可容忍的最大步长")] public int AstartMaxStep = (2 - 1) * 16 * 16;
        [Header("开放列表")] public List<AstarNode> _OpenList = new List<AstarNode>();
        [Header("关闭列表")] public List<AstarNode> _CloseList = new List<AstarNode>();
        [Header("完整路径")] public List<Vector3> _AstarResultPath = new List<Vector3>();
        Coroutine Dynamic_AstarCoroutine;
        private List<Vector3> _Astar_LineResultPath = new List<Vector3>();

        void _ReferUpdate_Astar()
        {

            //静态路线
            if (Toggle_StartAstar)
            {
                //Chase_Astar(StartPos.transform.position, EndPos.transform.position, 4, out List<Vector3> _result);

                Algo_Astar(StartPos.transform.position, EndPos.transform.position, 4, out List<Vector3> _result);

                _Astar_LineResultPath = new List<Vector3>(_result);
                Toggle_StartAstar = false;
            }

            //动态路线
            if (Toggle_DynamicAstar && Dynamic_AstarCoroutine == null)
            {
                Dynamic_AstarCoroutine = StartCoroutine(Chase_AstarCoroutine(StartPos.transform.position, EndPos.transform.position));
                Toggle_DynamicAstar = false;
            }
        }


        /// <summary>
        /// A* 追逐算法
        /// </summary>
        public void Chase_Astar(Vector3 _StartPos, Vector3 _EndPos, int _RenderSize, out List<Vector3> _result)
        {
            //Clear
            _result = new List<Vector3>();

            //提前返回-如果初始点卡住或者悬空
            if (CheckSolid(_StartPos) || !CheckSolid(_StartPos + Vector3.down * 2f))
                return;

            //规范化起点和终点
            _StartPos = GetCenterVector3(_StartPos);
            _EndPos = GetCenterVector3(_EndPos);
            int maxStep = _RenderSize * (int)(_EndPos - _StartPos).magnitude * TerrainData.ChunkWidth;

            //创建开放列表 / 关闭列表
            List<AstarNode> OpenList = new List<AstarNode>();
            List<AstarNode> CloseList = new List<AstarNode>();

            //初始化开始节点
            int stepCount = 0;
            Vector3 _startP = GetCenterVector3(_StartPos);
            float _startG = 0f;
            float _startH = EuclideanDistance3D(_StartPos, _EndPos);
            AstarNode _startparentPos = null;
            AstarNode StartNode = new AstarNode(_startP, _startG, _startH, _startparentPos);
            OpenList.Add(StartNode); UpdateOpenList(OpenList);

            //Debug

            //一直循环到DeQueue包含终点 或者超出最大步长
            while (OpenList.Count > 0)
            {
                //选择路径
                AstarNode _CurrentNode = OpenList[0];

                //更新CloseList
                CloseList.Add(_CurrentNode);

                //退出条件-找到终点
                if (_CurrentNode.P == _EndPos)
                {
                    _result = Astar_BacktrackingPath(_CurrentNode);
                    break;
                }
                else
                {
                    OpenList.RemoveAt(0);
                }

                //退出条件-如果超出最大步数，则返回距离终点最近的值
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

                //扩充OpenList
                for (int i = 0; i < EntityData.NearNodes.Length; i++)
                {
                    Vector3 _P = _CurrentNode.P + EntityData.NearNodes[i];
                    float _G = _CurrentNode.G + EntityData.NearNodes[i].magnitude;
                    float _H = EuclideanDistance3D(_P, _EndPos);
                    AstarNode _parentPos = _CurrentNode;

                    AstarNode _NearNode = new AstarNode(_P, _G, _H, _parentPos);

                    //如果是未记录节点，则存入OpenList
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
        /// A* 追逐算法的协程版
        /// </summary>
        private IEnumerator Chase_AstarCoroutine(Vector3 _StartPos, Vector3 _EndPos)
        {
            //Clear
            _OpenList.Clear();
            _CloseList.Clear();
            _AstarResultPath.Clear();

            //提前返回-如果初始点卡住或者悬空
            if (CheckSolid(_StartPos) || !CheckSolid(_StartPos + Vector3.down * 2f))
                yield break;

            //规范化起点和终点
            _StartPos = GetCenterVector3(_StartPos);
            _EndPos = GetCenterVector3(_EndPos);

            //初始化开始节点
            int stepCount = 0;
            Vector3 _startP = GetCenterVector3(_StartPos);
            float _startG = 0f;
            float _startH = EuclideanDistance3D(_StartPos, _EndPos);
            AstarNode _startparentPos = null;
            AstarNode StartNode = new AstarNode(_startP, _startG, _startH, _startparentPos);
            _OpenList.Add(StartNode); UpdateOpenList(_OpenList);


            //一直循环到DeQueue包含终点 或者超出最大步长
            while (_OpenList.Count > 0)
            {
                //选择路径
                AstarNode _CurrentNode = _OpenList[0];

                //更新CloseList
                _CloseList.Add(_CurrentNode);

                //退出条件
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

                //扩充OpenList
                for (int i = 0; i < EntityData.NearNodes.Length; i++)
                {
                    Vector3 _P = _CurrentNode.P + EntityData.NearNodes[i];
                    float _G = _CurrentNode.G + EntityData.NearNodes[i].magnitude;
                    float _H = EuclideanDistance3D(_P, _EndPos);
                    AstarNode _parentPos = _CurrentNode;

                    AstarNode _NearNode = new AstarNode(_P, _G, _H, _parentPos);

                    //如果是未记录节点，则存入OpenList
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

        //回溯并返回最短路径
        List<Vector3> Astar_BacktrackingPath(AstarNode _EndNode)
        {
            AstarNode _nextNode = _EndNode;
            List<Vector3> result = new List<Vector3>();

            // 只要 parentNode 不为 null，就继续回溯
            while (_nextNode.parentNode != null)
            {
                result.Add(_nextNode.P);
                _nextNode = _nextNode.parentNode;
            }

            // 反转路径顺序
            result.Reverse();

            return result; // 返回反转后的路径
        }

        //列表是否包含Astar节点
        bool isListContainNode(AstarNode _node, List<AstarNode> _List)
        {
            foreach (var item in _List)
            {
                if (item.P == _node.P)
                    return true;
            }

            return false;
        }

        //维护OpenList,F排序，相等则H排序
        void UpdateOpenList(List<AstarNode> _OpenList)
        {
            // 使用 List.Sort 来根据 F 和 H 排序
            _OpenList.Sort((node1, node2) =>
            {
                // 首先比较 F 值
                int fComparison = node1.F.CompareTo(node2.F);

                // 如果 F 值相同，再比较 H 值 
                if (fComparison == 0)
                {
                    return node1.H.CompareTo(node2.H);
                }

                return fComparison;
            });
        }

        //欧几里得算法
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
        [Header("绘制宽度")] public float TestPathWidth = 0.3f;
        [Header("每步的延迟时间")] public float StepDelay = 0.5f; // 让 Gizmos 可视化时有间隔
        private List<Vector3> DrawOrder = new List<Vector3>(); // 用来存储绘制顺序
        Coroutine DynamicPathCoroutine;


        private void OnDrawGizmos()
        {


            //随机游走算法
            if (RandomWalk_ResultPath.Count > 0)
            {
                //静态路线
                for (int i = 0; i < RandomWalk_ResultPath.Count - 1; i++)
                {
                    Gizmos.color = Color.cyan; // 设置线的颜色为青色（你可以换成其他颜色）
                    Gizmos.DrawLine(RandomWalk_ResultPath[i], RandomWalk_ResultPath[i + 1]); // 绘制每两个相邻点之间的连线
                }

                //始终绘制起点和终点
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(RandomWalk_ResultPath[0], new Vector3(TestPathWidth, TestPathWidth, TestPathWidth));
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(RandomWalk_ResultPath[RandomWalk_ResultPath.Count - 1], new Vector3(TestPathWidth, TestPathWidth, TestPathWidth));

                //绘制动态路线
                if (DrawOrder.Count != 0)
                {
                    int index = 0;
                    foreach (var item in DrawOrder)
                    {
                        //提前返回-不需要绘制首尾，前面已经绘制
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

            //A*算法静态路线
            if (_Astar_LineResultPath.Count > 0)
            {
                // 设置 Gizmos 的颜色
                Gizmos.color = Color.green;

                // 循环遍历路径的每一对连续的点，绘制出线条
                for (int i = 0; i < _Astar_LineResultPath.Count - 1; i++)
                {
                    // 从当前点到下一个点绘制线条
                    Gizmos.DrawLine(_Astar_LineResultPath[i], _Astar_LineResultPath[i + 1]);
                }
            }

            //A*动态算法
            if (_CloseList.Count > 0)
            {
                if (_AstarResultPath.Count == 0)
                {
                    //绘制OpenList
                    foreach (var item in _OpenList)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(item.P, new Vector3(TestPathWidth, TestPathWidth, TestPathWidth));
                    }

                    //绘制已选择路径
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


        #region 算法工具

        /// <summary>
        /// 是否可以走
        /// true:可以走
        /// </summary>
        /// <param name="_nextPos"></param>
        /// <param name="_nextDirect"></param>
        /// <returns></returns>
        bool CheckNodeLimit(Vector3 _nextPos, Vector3 _nextDirect)
        {
            byte _thisBlockType = managerhub.Service_World.GetBlockType(_nextPos);
            Vector3 _LastPos = _nextPos - _nextDirect;

            //提前返回-如果没有方块数据
            if (_thisBlockType == 255)
                return false;

            //提前返回-如果是方块
            if (CheckSolid(_nextPos))
                return false;

            //提前返回-如果nextPos悬空
            if (!CheckSolid(_nextPos + Vector3.down))
                return false;

            //提前返回-如果直角边占满，则对角线放置无效
            if (_nextDirect.x != 0 && _nextDirect.z != 0)
            {
                if (CheckSolid(_LastPos + new Vector3(_nextDirect.x, _nextDirect.y, 0f)) &&
                    CheckSolid(_LastPos + new Vector3(0f, _nextDirect.y, _nextDirect.z)))
                {
                    return false;
                }
            }

            //通过所有检测
            return true;
        }

        /// <summary>
        /// 是否可以走通
        /// </summary>
        public bool PathCanNavigate(List<Vector3> _Nodes)
        {
            foreach (var _node in _Nodes)
            {
                if (CheckSolid(_node))
                {
                    //走不通
                    return false;
                }
            }

            //可以走通
            return true;
        }


        /// <summary>
        /// 打乱Vector3数组并返回新数组，同时将指定方向放在首位
        /// </summary>
        /// <param name="array"></param>
        /// <param name="_FirstDirect"></param>
        /// <returns></returns>
        public Vector3[] ShuffleArray(Vector3[] array, Vector3 _FirstDirect)
        {
            // 复制数组，避免修改原数组
            Vector3[] shuffledArray = (Vector3[])array.Clone();

            // 查找_FirstDirect在数组中的索引位置
            int firstDirectIndex = System.Array.IndexOf(shuffledArray, _FirstDirect);

            // 如果找到了指定的方向，就将其移到数组的首位
            if (firstDirectIndex >= 0)
            {
                // 将 _FirstDirect 放到数组的首位
                Vector3 temp = shuffledArray[firstDirectIndex];
                shuffledArray[firstDirectIndex] = shuffledArray[0];
                shuffledArray[0] = temp;
            }

            // 洗牌剩余部分（从索引1开始洗牌）
            System.Random rng = new System.Random();
            for (int i = shuffledArray.Length - 1; i > 1; i--)  // 从第二个元素开始洗牌
            {
                int j = rng.Next(1, i + 1);  // 生成 1 到 i 之间的随机索引
                                             // 交换元素
                (shuffledArray[i], shuffledArray[j]) = (shuffledArray[j], shuffledArray[i]);
            }

            return shuffledArray;
        }

        /// <summary>
        /// 是否是固体
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


