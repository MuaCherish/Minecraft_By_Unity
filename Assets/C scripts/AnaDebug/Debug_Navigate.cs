using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Debug_Navigate : MonoBehaviour
{


    #region 周期函数

    ManagerHub managerhub;
    World world;
    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        world = managerhub.world;
    }

    private void Update()
    {
        if (managerhub.world.game_state == Game_State.Playing)
        {
            if (Toggle_StartToFindDestination)
            {
                // 初始化
                Toggle_StartToFindDestination = false;
                VisitPositions.Clear();  // 清空记录的路径点
                currentIndex = 0;

                // 计算
                RandomWalkDestination(transform.position, out var result);

                // Gizmos
                VisitPositions = new List<Vector3>(result);
                StartCoroutine(DrawPathWithDelay());

            }
        }

        
    }


    #endregion


    #region 随机游走算法

    [Header("执行随机游走算法")] public bool Toggle_StartToFindDestination;
    [Header("最大随机游走步数")] public int WalkSteps = 10;  // 走 N 轮
    [Header("遵循上个方向概率")] public float prevDirectionProbability = 0f;

    /// <summary>
    /// 随机游走算法
    /// </summary>
    /// <param name="_StartPos"></param>
    /// <param name="VisitPositions"></param>
    void RandomWalkDestination(Vector3 _StartPos, out List<Vector3> VisitPositions)
    {
        // 初始化
        Vector3 _thisPos = UsefulFunction.GetCenterVector3(_StartPos);
        Vector3 _thisDirect = EntityData.RandomWalkFace[Random.Range(0, 8)];
        VisitPositions = new List<Vector3>{ _StartPos };

        //提前返回-起始点被堵住 || 起始点悬空
        byte _StartPosBlockType = world.GetBlockType(_thisPos);
        byte _StartDownPosBlockType = world.GetBlockType(_thisPos + Vector3.down);
        if (_StartPosBlockType == 255 || world.blocktypes[_StartPosBlockType].isSolid || _StartDownPosBlockType == VoxelData.Air)
            return;

        // 迭代 N 步
        for (int i = 0; i < WalkSteps; i++)
        {
            // 计算下一个目标位置
            //如果单个点啥也没找到，则直接结束迭代
            if (!GetOneDestination(VisitPositions, _thisPos, _thisDirect, out var _nextPos, out var _nextDirect))
                break;

            // 更新位置和方向
            _thisPos = _nextPos;
            _thisDirect = _nextDirect;

            // 保存下一个位置
            VisitPositions.Add(_nextPos);
        }
    }

    //寻找单个点范围内的下一个点
    bool GetOneDestination(List<Vector3> _VisitPositions, Vector3 _LastPos, Vector3 _LastDirect, out Vector3 _nextPos, out Vector3 _nextDirect)
    {
        _nextPos = Vector3.zero;
        _nextDirect = Vector3.zero;

        // 如果遵循上一个方向的概率大于随机值，则继续走上一个方向
        if (Random.value < prevDirectionProbability)
        {
            _nextPos = _LastPos + _LastDirect;
            _nextDirect = _LastDirect;

            // 如果该点有效，提前退出
            if (CheckVisit(_LastPos, _LastDirect, _nextPos, _nextDirect, _VisitPositions)) 
                return true; 
        }

        // 打乱方向顺序并遍历
        Vector3[] shuffledDirections = ShuffleArray(EntityData.RandomWalkFace);
        int errorNumber = 0;
        foreach (var _direct in shuffledDirections)
        {
            _nextPos = _LastPos + _direct;
            _nextDirect = _direct;

            // 如果该点不可行，继续下一个方向
            if (CheckVisit(_LastPos, _LastDirect, _nextPos, _nextDirect, _VisitPositions))
            {
                break;
            }
            else
            {
                errorNumber = errorNumber + 1;
                continue;
            }

            
        }

        //判断是否走投无路 
        if (errorNumber >= 0 && errorNumber <= 7)
        {
            return true;
        }
        else
        {
            print("路径走投无路");
            return false;
        }

    }

    //放置合理性检测
    bool CheckVisit(Vector3 _LastPos,Vector3 _LastDirect, Vector3 _nextPos,Vector3 _nextDirect, List<Vector3> _VisitPositions)
    {
        byte _thisBlockType = world.GetBlockType(_nextPos);

        //[World] 提前返回-如果没有方块数据 || 如果目标是固体
        if (_thisBlockType == 255 || world.blocktypes[_thisBlockType].isSolid)
            return false;

        //[Path] 提前返回-已经走过的路
        if (_VisitPositions.Contains(_nextPos))
            return false;

        //提前返回-如果直角边占满，则对角线放置无效
        if (_nextDirect == new Vector3(1f, 0f, 1f))
        {
            byte _BlockTypeA = world.GetBlockType(_LastPos + new Vector3(1f, 0f, 0f));
            byte _BlockTypeB = world.GetBlockType(_LastPos + new Vector3(0f, 0f, 1f));

            if (_BlockTypeA == 255 || world.blocktypes[_BlockTypeA].isSolid ||
                _BlockTypeB == 255 || world.blocktypes[_BlockTypeB].isSolid)
            {
                return false;
            }
        }
        else if (_nextDirect == new Vector3(1f, 0f, -1f))
        {
            byte _BlockTypeA = world.GetBlockType(_LastPos + new Vector3(1f, 0f, 0f));
            byte _BlockTypeB = world.GetBlockType(_LastPos + new Vector3(0f, 0f, -1f));

            if (_BlockTypeA == 255 || world.blocktypes[_BlockTypeA].isSolid ||
                _BlockTypeB == 255 || world.blocktypes[_BlockTypeB].isSolid)
            {
                return false;
            }
        }
        else if (_nextDirect == new Vector3(-1f, 0f, -1))
        {
            byte _BlockTypeA = world.GetBlockType(_LastPos + new Vector3(-1f, 0f, 0f));
            byte _BlockTypeB = world.GetBlockType(_LastPos + new Vector3(0f, 0f, -1f));

            if (_BlockTypeA == 255 || world.blocktypes[_BlockTypeA].isSolid ||
                _BlockTypeB == 255 || world.blocktypes[_BlockTypeB].isSolid)
            {
                return false;
            }
        }
        else if (_nextDirect == new Vector3(-1f, 0f, 1f))
        {
            byte _BlockTypeA = world.GetBlockType(_LastPos + new Vector3(-1f, 0f, 0f));
            byte _BlockTypeB = world.GetBlockType(_LastPos + new Vector3(0f, 0f, 1f));

            if (_BlockTypeA == 255 || world.blocktypes[_BlockTypeA].isSolid ||
                _BlockTypeB == 255 || world.blocktypes[_BlockTypeB].isSolid)
            {
                return false;
            }
        }

        //通过所有检测
        return true;
    }

    #endregion


    #region Gizmos

    [Header("绘制Cube宽度")] public float CubeWidth = 0.8f;
    [Header("每步的延迟时间")] public float StepDelay = 0.5f; // 让 Gizmos 可视化时有间隔
    private List<int> DrawOrder = new List<int>(); // 用来存储绘制顺序
    private int currentIndex = 0; // 当前正在绘制的位置
    public List<Vector3> VisitPositions = new List<Vector3>();

    // 绘制路径
    private IEnumerator DrawPathWithDelay()
    {
        DrawOrder.Clear(); // 清空绘制顺序

        // 将每个点的索引添加到绘制顺序中
        for (int i = 0; i < VisitPositions.Count; i++)
        {
            DrawOrder.Add(i);
        }

        // 延迟绘制每一个方块
        for (int i = 0; i < DrawOrder.Count; i++)
        {
            yield return new WaitForSeconds(StepDelay);
            currentIndex = i; // 更新当前绘制的索引
        }

    }

    private void OnDrawGizmos()
    {
        if (VisitPositions.Count > 0 && currentIndex < DrawOrder.Count)
        {
            // 绘制所有路径点
            for (int i = 0; i <= currentIndex; i++)
            {
                DrawCube(i);
            }
        }
    }

    // 绘制方块
    private void DrawCube(int index)
    {
        // 设置起点为绿色，终点为红色，其他点为白色
        Gizmos.color = (index == 0) ? Color.green : (index == VisitPositions.Count - 1) ? Color.red : Color.white;
        Gizmos.DrawWireCube(VisitPositions[index], new Vector3(CubeWidth, CubeWidth, CubeWidth));
    }

    #endregion


    #region Tool

    // 打乱Vector3数组并返回新数组
    public static Vector3[] ShuffleArray(Vector3[] array)
    {
        Vector3[] shuffledArray = (Vector3[])array.Clone(); // 复制数组，避免修改原数组
        System.Random rng = new System.Random();

        for (int i = shuffledArray.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1); // 生成 0 到 i 之间的随机索引
            (shuffledArray[i], shuffledArray[j]) = (shuffledArray[j], shuffledArray[i]); // 交换元素
        }

        return shuffledArray;
    }

    #endregion

}
