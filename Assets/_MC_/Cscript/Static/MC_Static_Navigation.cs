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
                _world = SceneData.GetManagerhub().world; // 在第一次访问时创建
            }
            return _world;
        }
    }
    private static World _world;  // 静态变量存储 World 实例


    #endregion


    #region Path


    /// <summary>
    /// 是否可以走
    /// true:可以走
    /// </summary>
    /// <param name="_nextPos"></param>
    /// <param name="_nextDirect"></param>
    /// <returns></returns>
    public static bool CheckNodeLimit(Vector3 _nextPos, Vector3 _nextDirect)
    {
        byte _thisBlockType = world.GetBlockType(_nextPos);
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
    /// 是否可以走通路径
    /// </summary>
    public static bool PathCanNavigate(List<Vector3> _Nodes)
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





    #region RandomWalk算法


    /// <summary>a
    /// 随机游走算法
    /// </summary>
    /// <param name="_StartPos">初始点</param>
    /// <param name="_Steps">迭代次数</param>
    /// <param name="_prevDirectionProbability">走直线的概率</param>
    /// <param name="_Nodes">返回所有路径点</param>
    public static void Algo_RandomWalk(Vector3 _StartPos, int _Steps, float _prevDirectionProbability, out Vector3 _TargetPos)
    {
        // 初始化
        Vector3 _thisPos = GetCenterVector3(_StartPos);
        Vector3 _thisDirect = EntityData.NearNodes[Random.Range(0, EntityData.NearNodes.Length)];
        List<Vector3> _Nodes = new List<Vector3> { _thisPos };
        _TargetPos = Vector3.zero;

        //提前返回-起始点被堵住 || 起始点悬空
        byte _StartPosBlockType = world.GetBlockType(_thisPos);
        byte _StartDownPosBlockType = world.GetBlockType(_thisPos + Vector3.down);
        if (_StartPosBlockType == 255 || world.blocktypes[_StartPosBlockType].isSolid || _StartDownPosBlockType == VoxelData.Air)
            return;

        // 迭代 N 步
        for (int i = 0; i < _Steps; i++)
        {
            // 计算下一个目标位置
            //如果单个点啥也没找到，则直接结束迭代
            if (!GetOneDestination(_Nodes, _thisPos, _thisDirect, _prevDirectionProbability, out var _nextPos, out var _nextDirect))
            {
                _TargetPos = _Nodes[_Nodes.Count - 1];
                break;
            }

            // 更新位置和方向
            _thisPos = _nextPos;
            _thisDirect = _nextDirect;

            // 保存下一个位置
            _Nodes.Add(_nextPos);
        }
    }


    /// <summary>a
    /// 随机游走算法
    /// </summary>
    /// <param name="_StartPos">初始点</param>
    /// <param name="_Steps">迭代次数</param>
    /// <param name="_prevDirectionProbability">走直线的概率</param>
    /// <param name="_Nodes">返回所有路径点</param>
    public static void Algo_RandomWalk(Vector3 _StartPos, int _Steps, float _prevDirectionProbability, out List<Vector3> _Nodes)
    {
        // 初始化
        Vector3 _thisPos = GetCenterVector3(_StartPos);
        Vector3 _thisDirect = EntityData.NearNodes[Random.Range(0, EntityData.NearNodes.Length)];
        _Nodes = new List<Vector3> { _thisPos };

        //提前返回-起始点被堵住 || 起始点悬空
        byte _StartPosBlockType = world.GetBlockType(_thisPos);
        byte _StartDownPosBlockType = world.GetBlockType(_thisPos + Vector3.down);
        if (_StartPosBlockType == 255 || world.blocktypes[_StartPosBlockType].isSolid || _StartDownPosBlockType == VoxelData.Air)
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
    private static bool GetOneDestination(List<Vector3> _VisitPositions, Vector3 _LastPos, Vector3 _LastDirect, float _prevDirectionProbability, out Vector3 _nextPos, out Vector3 _nextDirect)
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


    #region Astar算法

    /// <summary>
    /// A*寻路算法，返回一整个路径
    /// </summary>
    public static void Algo_Astar(Vector3 _StartPos, Vector3 _EndPos, int _RenderSize, out List<Vector3> _result)
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
                if (CheckNodeLimit(_P, EntityData.NearNodes[i]) && !isListContainNode(_NearNode, OpenList) && !isListContainNode(_NearNode, CloseList))
                    OpenList.Add(_NearNode); UpdateOpenList(OpenList);

            }

            //Wait
            stepCount++;

        }
    }

    //维护OpenList,F排序，相等则H排序
    private static void UpdateOpenList(List<AstarNode> _OpenList)
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

    //回溯并返回最短路径
    private static List<Vector3> Astar_BacktrackingPath(AstarNode _EndNode)
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



//Astar数据结构
[System.Serializable]
public class AstarNode
{
    public Vector3 P;
    public float G; // 路径消耗
    public float H; // 预期消耗
    public float F; // 总消耗
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
