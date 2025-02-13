using System.Collections.Generic;
using UnityEngine;

public static class MC_UtilityFunctions
{

    #region 很小量 

    public static float Delta = 0.01f;
    public static float Delta_Pro = 0.0125f;

    #endregion


    #region Enum

    /// <summary>
    /// 方向
    /// </summary>
    public enum BlockDirection
    {
        前,
        后,
        左,
        右,
        上,
        下
    }

    #endregion


    #region BlockType

    /// <summary>
    /// 给定绝对坐标，返回其类型。<para/>
    /// 不包含自定义形状。
    /// </summary>
    /// <param name="_pos">绝对坐标</param>
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
    /// 是否是固体
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


    #region 光标

    /// <summary>
    /// 如果为 true，则隐藏并固定鼠标；如果为 false，则显示并解锁鼠标
    /// </summary>
    /// <param name="isLocked">如果为 true，则隐藏并固定鼠标；如果为 false，则显示并解锁鼠标</param>
    public static void LockMouse(bool isLocked)
    {
        if (isLocked)
        {
            //print("隐藏鼠标");
            // 隐藏鼠标光标并将其锁定在屏幕中心
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            //print("显示鼠标");
            // 显示鼠标光标并解除锁定
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }


    #endregion


    #region 出界判断

    /// <summary>
    /// 绝对或者相对坐标判断是否出界
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public static bool isOutOfChunkRange(Vector3 _pos)
    {
        //获取相对坐标
        Vector3 _vec = GetRelaPos(_pos);

        //是否出界
        int _x = (int)_vec.x;
        int _y = (int)_vec.y;
        int _z = (int)_vec.z;

        if (_x < 0 || _x > TerrainData.ChunkWidth - 1 || _y < 0 || _y > TerrainData.ChunkHeight - 1 || _z < 0 || _z > TerrainData.ChunkWidth - 1)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 单个值判断
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_y"></param>
    /// <param name="_z"></param>
    /// <returns></returns>
    public static bool isOutOfChunkRange(int _x, int _y, int _z)
    {
        //是否出界
        if (_x < 0 || _x > TerrainData.ChunkWidth - 1 || _y < 0 || _y > TerrainData.ChunkHeight - 1 || _z < 0 || _z > TerrainData.ChunkWidth - 1)
            return true;
        else
            return false;

    }


    #endregion


    #region 随机取点

    /// <summary>
    /// 在物体自身坐标的水平圆心内随机选点
    /// </summary>
    /// <param name="center">圆心坐标</param>
    /// <param name="radius">圆的半径</param>
    /// <returns>返回随机生成的水平点(Vector3)</returns>
    public static Vector3 GetRandomPointInCircle(Vector3 center, float radius)
    {
        // 随机生成一个角度（弧度制）
        float angle = Random.Range(0f, Mathf.PI * 2);

        // 随机生成一个距离，确保在半径范围内
        float randomRadius = Random.Range(0f, radius);

        // 计算随机点在水平平面上的x和z坐标
        float xOffset = Mathf.Cos(angle) * randomRadius;
        float zOffset = Mathf.Sin(angle) * randomRadius;

        // 返回随机点的世界坐标
        return new Vector3(center.x + xOffset, center.y, center.z + zOffset);
    }


    #endregion


    #region Probability

    /// <summary>
    /// 返回概率值
    /// </summary>
    /// <param name="_Probability"></param>
    /// <returns></returns>
    public static bool GetProbability(float _Probability)
    {
        // 确保输入值在 0 到 100 之间
        _Probability = Mathf.Clamp(_Probability, 0, 100);

        // 生成一个 0 到 100 之间的随机数
        float randomValue = UnityEngine.Random.Range(0f, 100f);

        // 如果随机数小于等于输入值，则返回 true
        //Debug.Log(randomValue);
        bool a = randomValue < _Probability;

        return a;
    }

    #endregion


    #region Vector3

    /// <summary>
    /// 将相对坐标变成绝对坐标
    /// </summary>
    /// <param name="_vec"></param>
    /// <returns></returns>
    public static Vector3 GetRealPos(Vector3 _vec, Vector3 _ChunkLocation)
    {
        return Vector3.zero;
    }

    /// <summary>
    /// 将绝对坐标改为相对坐标
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public static Vector3 GetRelaPos(Vector3 _pos)
    {
        return new Vector3(Mathf.FloorToInt(_pos.x % TerrainData.ChunkWidth), Mathf.FloorToInt(_pos.y) % TerrainData.ChunkHeight, Mathf.FloorToInt(_pos.z % TerrainData.ChunkWidth));
    }

    /// <summary>
    /// 返回Int类型的Vector3
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public static Vector3 GetIntVector3(Vector3 _pos)
    {
        return new Vector3((int)_pos.x, (int)_pos.y, (int)_pos.z);
    }

    /// <summary>
    /// 给定任意绝对坐标，返回其所在方块的中心坐标，注意只有0.5f
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public static Vector3 GetCenterVector3(Vector3 _pos)
    {
        return new Vector3((int)_pos.x + 0.5f, (int)_pos.y + 0.5f, (int)_pos.z + 0.5f);
    }


    /// <summary>
    /// 欧几里得算法
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
    /// 打乱Vector3数组并返回新数组，同时将指定方向放在首位
    /// </summary>
    /// <param name="array"></param>
    /// <param name="_FirstDirect"></param>
    /// <returns></returns>
    public static Vector3[] ShuffleArray(Vector3[] array, Vector3 _FirstDirect)
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