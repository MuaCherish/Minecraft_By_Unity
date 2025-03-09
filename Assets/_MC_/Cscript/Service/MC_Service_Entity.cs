using Homebrew;
using MCEntity;
using System.Collections.Generic;
using UnityEngine;

public class MC_Service_Entity : MonoBehaviour
{

    #region 周期函数

    ManagerHub managerhub;
    Player player;
    World world;
    TimeManager timeManager;
    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        timeManager = managerhub.timeManager;
        player = managerhub.player;
        Entity_Parent = SceneData.GetEntityParent();
        world = managerhub.world;

        isNaturalSpawnEnabled = managerhub.生物自然生成;

        _ReferAwake_InitService();
    }



    private void Update()
    {
        switch (world.game_state)
        {
            case Game_State.Loading:
                Handle_GameState_Loading();
                break;
            case Game_State.Playing:
                Handle_GameState_Playing();
                break;
        }
    }

    private void FixedUpdate()
    {
        switch (world.game_state)
        {
            case Game_State.Playing:
                FixedHandle_GameState_Playing();
                break;
        }
    }

    void FixedHandle_GameState_Playing()
    {
        timeSinceLastExecution += Time.fixedDeltaTime;

        if (timeSinceLastExecution >= interval)
        {
            _ReferFixedUpdate_Service_DynamicAddEntity(); // 执行你的方法
            timeSinceLastExecution -= interval; // 确保时间从零开始计时，防止溢出
        }
    }


    void Handle_GameState_Loading()
    {
        
    }


    void Handle_GameState_Playing()
    {
        _ReferUpdate_CheckShowEntityHitbox();
    }


    #endregion


    #region 常驻服务_动态生成实体

    [Foldout("常驻服务_动态生成实体(20tick)", true)]
    [Header("自然生成")] public bool isNaturalSpawnEnabled;
    [Header("实体生成延迟范围")] public Vector2 AddEntityDelayRange = new Vector2(60f, 120f);
    [Header("实体生成半径范围")] public Vector2 spawnRadiusRange = new Vector2(10f, 16f); //实体生成半径范围(甜甜圈)
    [Header("实体生成diffY合适距离")] public float maxSpawnHeightDifference;  //实体Y - 玩家Y < 实体生成diffY合适距离
    [Header("每种类型生物可生成的最大数量")] public List<int> MutexEntity_MaxGenNumber = new List<int>();
    private float timeSinceLastExecution = 0f;
    private const float interval = 1f / 20f; // 每秒执行20次，即每次间隔0.05秒

    void _ReferAwake_InitService()
    {
        //print($"{MC_Static_Math.CalculateFrameProbability(30, 0.4f, 20)}");

        MutexEntity_MaxGenNumber.Clear();
        foreach (var item in Entity_Prefebs)
        {
            MutexEntity_MaxGenNumber.Add(item.maxGenNumer);
        }

    }

    


    void _ReferFixedUpdate_Service_DynamicAddEntity()
    {
        //提前返回-如果没有开启自然生成
        if (!isNaturalSpawnEnabled)
            return;

        //提前返回-如果是Start则退出
        if (world.game_state == Game_State.Start)
            return;

        //提前返回-如果超出最大实体数量
        if (isFullOfAllEntity())
            return;

        //检查生成
        for (int EntityIndex = 0; EntityIndex < Entity_Prefebs.Length; EntityIndex++)
        {
            //提前跳过-概率判定
            if (!MC_Static_Math.GetProbability(Entity_Prefebs[EntityIndex].GenerateProbability))
                continue;

            //提前跳过-该生物投放数量已满
            if (isFullOfMutexEntity(EntityIndex))
                continue;

            //提前跳过-夜间生物不在白天生成
            if (Entity_Prefebs[EntityIndex].OnlyGenerateInNight && !timeManager.IsNight())
                continue;

            //AddEntity
            Handle_DynamicAddEntity(EntityIndex);
        }



    }

    void Handle_DynamicAddEntity(int _EntityIndex)
    {
        Vector3 NextEntityPos = DynamicAddEntity_FindPos();

        //如果可以多生成几只
        if (MC_Static_Math.GetProbability(Entity_Prefebs[_EntityIndex].MoreGenProbability))
        {
            int MoreNum = Random.Range(1, 4);

            for (int i = 0; i < MoreNum; i++)
            {
                Vector3 SurroundPos = MC_Static_Math.GetRandomPointInDonut(NextEntityPos, new Vector2(3f, 10f));
                AddEntity(_EntityIndex, SurroundPos, out var _entity);
            }

        }

        AddEntity(_EntityIndex, NextEntityPos, out var entity);
        //print($"投放实体, index:{EntityIndex}, pos:{NextEntityPos}");
    }


    //实体最大生成数量是否满了
    bool isFullOfMutexEntity(int _EntityIndex)
    {
        if (MutexEntity_MaxGenNumber[_EntityIndex] <= 0)
            return true;

        return false;
    }


    //寻找合适的坐标 
    Vector3 RandomSpawnPos;
    Vector3 DynamicAddEntity_FindPos()
    {
        // 获取玩家位置
        Vector3 playerPos = managerhub.player.transform.position;

        // 生成一个在甜甜圈范围内的随机点
        RandomSpawnPos = MC_Static_Math.GetRandomPointInDonut(playerPos, spawnRadiusRange);

        // 调用World的获取可用出生点函数
        world.GetSpawnPos(RandomSpawnPos, out List<Vector3> _Result);

        // ForeachList: 判断坐标是否可生成 [不在玩家视锥体内] [距离玩家的Y在合适范围]
        foreach (var _pos in _Result)
        {
            if (CheckCanAddEntity(_pos))
                return _pos;
        }

        return Vector3.zero;
    }

    //检查是否可以生成实体
    bool CheckCanAddEntity(Vector3 _pos)
    {
        Vector3 _playerPos = managerhub.player.transform.position;
        Camera _camera = managerhub.player.eyes;
        bool _Pass = true;

        // 距离玩家Y值超出合适范围
        if (Mathf.Abs(_pos.y - _playerPos.y) > maxSpawnHeightDifference)
            _Pass = false;

        // 不在玩家视锥体内
        Vector3 viewportPoint = _camera.WorldToViewportPoint(_pos);
        if (viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1)
            _Pass = false;

        return _Pass;
    }

    #endregion


    #region 实体管理

    [Foldout("实体管理", true)]
    [Header("蒸汽粒子")] public GameObject Evaporation_Particle;
    [Header("实体预制体")] public EntitySpawnConfig[] Entity_Prefebs;
    [Header("活着的所有实体")] public List<EntityInfo> AllEntity = new List<EntityInfo>();
    [Header("最大实体数量")][SerializeField] public int maxSize = 100; // 默认值为100，可在Inspector中调整
    private int Unique_Id = 0; // 用于生成新的唯一ID
    GameObject Entity_Parent;

    /// <summary>
    /// AllEntity是否已满
    /// </summary>
    /// <returns></returns>
    public bool isFullOfAllEntity()
    {
        if (GetEntityCount() >= maxSize)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 根据id寻找实体
    /// </summary>
    public GameObject FindEntity(int _id)
    {
        foreach (var entity in AllEntity)
        {
            if (entity._id == _id)
            {
                return entity._obj;
            }
        }
        Debug.LogWarning($"实体ID {_id} 不存在！");
        return null;
    }

    /// <summary>
    /// 设置最大容量
    /// </summary>
    /// <param name="newMaxSize">新的最大容量</param>
    public void SetMaxSize(int newMaxSize)
    {
        if (AllEntity.Count >= newMaxSize)
        {
            Debug.LogWarning("新设置的最大容量小于当前实体数量，调整无效！");
            return;
        }

        maxSize = newMaxSize;
        Debug.Log($"最大容量已调整为 {maxSize}。");
    }

    /// <summary>
    /// 添加实体到管理中
    /// </summary>
    /// <param name="_index">需要添加的预制体的下标</param>
    /// <param name="_Startpos">实体的起始位置</param>
    /// <returns>是否添加成功</returns>
    public bool AddEntity(int _PrefebIndex, Vector3 _Startpos, out EntityInfo _Result, bool ignoreMaxMutex = false)
    {
        // 提起返回-检查实体数量是否达到最大值
        if (AllEntity.Count >= maxSize)
        {
            Debug.LogWarning("实体数量已达到最大值，无法添加新实体！");
            _Result = null;
            return false;
        }

        // 提起返回-检查下标是否有效
        if (_PrefebIndex < 0 || _PrefebIndex >= Entity_Prefebs.Length)
        {
            Debug.LogError("索引超出范围，请提供有效的预制体索引！");
            _Result = null;
            return false;
        }

        //提前返回-当前坐标没有区块或者
        if (!world.TryGetChunkObject(_Startpos, out Chunk chunktemp))
        {
            _Result = null;
            return false;
        }

        //提前返回-区块被隐藏
        if (chunktemp != null && chunktemp.isShow == false)
        {
            _Result = null;
            return false;
        }

        //提前返回-如果不是指令模式且无更多可生成实体数量
        if(!ignoreMaxMutex && isFullOfMutexEntity(_PrefebIndex))
        {
            _Result = null;
            return false;
        }

        // 实例化预制体
        GameObject newEntity = Instantiate(Entity_Prefebs[_PrefebIndex].prefeb);

        // 提起返回-如果没有注册组件
        if (newEntity.GetComponent<MC_Component_Registration>() == null)
        {
            print($"{_PrefebIndex}实体未添加注册组件，无法添加到游戏中");
            _Result = null;
            Destroy(newEntity);
            return false;
        }


        // 生成一个唯一的ID
        int entityId = Unique_Id++;

        //生成名字
        string entityName = EntityData.GetEntityName(_PrefebIndex);

        if (entityName == "Unknown Entity")
            print("该实体没有名字！！");

        //生成Struct
        EntityInfo _entityInfo = new EntityInfo(entityId, entityName, newEntity);

        //对实体进行注册
        newEntity.transform.SetParent(Entity_Parent.transform);
        newEntity.transform.position = _Startpos;
        newEntity.name = $"[{entityId}] {entityName}";
        newEntity.GetComponent<MC_Component_Registration>().RegistEntity(_entityInfo);

        // 将新实例加入数据结构
        _Result = _entityInfo;
        AllEntity.Add(_Result);

        //指令型的调用不会触发信号量变化
        if (!ignoreMaxMutex)
            MutexEntity_MaxGenNumber[EntityData.GetEntityPrefebIndex(entityName)]--;

        //Debug.Log($"实体 {newEntity.name} 已添加成功，ID为{entityId}！");

        return true;
    }

    /// <summary>
    /// 从管理中移除实体
    /// </summary>
    /// <param name="entity">需要移除的实体</param>
    /// <returns>是否移除成功</returns>
    public bool RemoveEntity(EntityInfo _EntityID)
    {
        EntityInfo entityToRemove = null;

        // 获取该实体对应的EntityStruct
        foreach (var entityStruct in AllEntity)
        {
            if (entityStruct._obj == _EntityID._obj)
            {
                entityToRemove = entityStruct;
                break;
            }
        }

        if (entityToRemove != null)
        {
            //Debug.Log($"实体 {_EntityID._name} 已移除成功！");
            MutexEntity_MaxGenNumber[EntityData.GetEntityPrefebIndex(entityToRemove._name)] ++;
            //End
            AllEntity.Remove(entityToRemove);
            return true;
        }

        Debug.LogWarning("该实体不在管理中！");
        return false;
    }

    /// <summary>
    /// 检测当前实体数量
    /// </summary>
    /// <returns>实体数量</returns>
    public int GetEntityCount()
    {
        return AllEntity.Count;
    }

    /// <summary>
    /// 清空所有实体
    /// </summary>
    public void ClearEntities()
    {
        AllEntity.Clear();
        Debug.Log("所有实体已清空！");
    }

    /// <summary>
    /// 获取一定范围内的实体（基于位置距离计算）
    /// </summary>
    /// <param name="center">范围检测的球心</param>
    /// <param name="_r">检测范围的半径</param>
    /// <returns>范围内的实体列表</returns>
    public bool GetOverlapSphereEntity(Vector3 center, float _r, out List<EntityInfo> result)
    {
        result = new List<EntityInfo>();  // 初始化输出列表
        float sqrRadius = _r * _r; // 使用平方半径以避免重复开平方运算
        bool hasEntitiesInRange = false;  // 记录是否有实体在范围内

        foreach (var entityStruct in AllEntity)
        {
            if (entityStruct._obj == null)
                continue; // 防止空引用错误

            // 计算实体与球心的距离
            Vector3 offset = entityStruct._obj.transform.position - center;

            // 如果实体的平方距离小于等于检测范围的平方半径，添加到结果列表
            if (offset.sqrMagnitude <= sqrRadius)
            {
                result.Add(entityStruct);
                hasEntitiesInRange = true;  // 有实体在范围内
            }
        }

        return hasEntitiesInRange;
    }

    /// <summary>
    /// 重载，可以忽略自己
    /// </summary>
    /// <param name="center"></param>
    /// <param name="_r"></param>
    /// <param name="_myID"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool GetOverlapSphereEntity(Vector3 center, float _r, int _myID, out List<EntityInfo> result)
    {
        result = new List<EntityInfo>();  // 初始化输出列表
        float sqrRadius = _r * _r; // 使用平方半径以避免重复开平方运算
        bool hasEntitiesInRange = false;  // 记录是否有实体在范围内

        foreach (var entityStruct in AllEntity)
        {
            if (entityStruct._obj == null || entityStruct._id == _myID)
                continue; // 防止空引用错误，同时跳过自身

            // 计算实体与球心的距离
            Vector3 offset = entityStruct._obj.transform.position - center;

            // 如果实体的平方距离小于等于检测范围的平方半径，添加到结果列表
            if (offset.sqrMagnitude <= sqrRadius)
            {
                result.Add(entityStruct);
                hasEntitiesInRange = true;  // 有实体在范围内
            }
        }

        return hasEntitiesInRange;
    }



    #endregion


    #region 打开碰撞盒


    bool hasExec_ShowEntityHitbox = true;
    void _ReferUpdate_CheckShowEntityHitbox()
    {

        if (player.ShowEntityHitbox)
        {
            if (hasExec_ShowEntityHitbox)
            {

                SwitchAllEntityHitbox(true);

                hasExec_ShowEntityHitbox = false;
            }
        }
        else
        {
            if (hasExec_ShowEntityHitbox == false)
            {
                SwitchAllEntityHitbox(false);
                hasExec_ShowEntityHitbox = true;
            }
        }


    }

    /// <summary>
    /// 检查所有碰撞盒，并打开他们的hitbox选项
    /// </summary>
    /// <param name="_bool"></param>
    void SwitchAllEntityHitbox(bool _bool)
    {
        foreach (var item in AllEntity)
        {
            item._obj.GetComponent<MC_Component_Physics>().isDrawHitBox = _bool;
        }
    }

    #endregion


    #region Debug

    [Foldout("Debug", true)]
    [Header("绘制可生成实体的范围")] public bool Debug_DrawAddEntityRange;
    [Header("实体生成位置预测")] public bool Debug_PredictEntityPos;

    private void OnDrawGizmos()
    {
        if (Debug_DrawAddEntityRange && player != null)
        {
            Vector3 playerPos = player.transform.position;
            Camera cam = player.eyes; // 获取玩家相机

            if (cam != null)
            {
                DrawWireCircle(playerPos, spawnRadiusRange.x, cam);
                DrawWireCircle(playerPos, spawnRadiusRange.y, cam);
                DrawFOVLines(playerPos, cam, spawnRadiusRange.y); // 画出FOV扇形
            }
        }

        if (Debug_PredictEntityPos)
        {
            Vector3 StartPos = RandomSpawnPos; StartPos.y = 128f;
            Vector3 EndPos = RandomSpawnPos; EndPos.y = 0f;
            Debug.DrawLine(StartPos, EndPos, Color.red);
        }

    }

    // 在XOZ平面上绘制一个圆，根据视野范围更改颜色
    private void DrawWireCircle(Vector3 center, float radius, Camera cam, int segments = 32)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0); // 初始点

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

            // 计算中点，并检测是否在视野范围内
            Vector3 midPoint = (prevPoint + newPoint) * 0.5f;
            bool inView = IsPointInCameraView(cam, center, midPoint);

            // 设置颜色
            Gizmos.color = inView ? Color.red : Color.green;
            Gizmos.DrawLine(prevPoint, newPoint);

            prevPoint = newPoint;
        }
    }

    // 判断一个点是否在玩家的FOV范围内
    private bool IsPointInCameraView(Camera cam, Vector3 playerPos, Vector3 point)
    {
        Vector3 toPoint = (point - playerPos).normalized; // 玩家到点的方向
        Vector3 forward = cam.transform.forward.normalized; // 玩家视角方向

        float angleToPoint = Vector3.Angle(forward, toPoint); // 计算夹角
        float halfFOV = cam.fieldOfView / 2f; // 视野角的一半

        return angleToPoint < halfFOV;
    }

    // 画出FOV扇形（只在XOZ平面）
    private void DrawFOVLines(Vector3 playerPos, Camera cam, float maxRadius)
    {
        Vector3 forward = cam.transform.forward;
        forward.y = 0; // 只保留XOZ平面方向
        forward.Normalize();

        // 计算 FOV 左右边界方向（绕Y轴旋转）
        Quaternion leftRotation = Quaternion.AngleAxis(-cam.fieldOfView / 2f, Vector3.up);
        Quaternion rightRotation = Quaternion.AngleAxis(cam.fieldOfView / 2f, Vector3.up);

        Vector3 leftDir = leftRotation * forward;
        Vector3 rightDir = rightRotation * forward;

        // 限制方向在XOZ平面并归一化
        leftDir.y = 0;
        rightDir.y = 0;
        leftDir = leftDir.normalized;
        rightDir = rightDir.normalized;

        // 计算视野边界点
        Vector3 leftPoint = playerPos + leftDir * maxRadius;
        Vector3 rightPoint = playerPos + rightDir * maxRadius;

        // 画两条线（只在XOZ平面）
        Gizmos.color = Color.red;
        Gizmos.DrawLine(playerPos, leftPoint);
        Gizmos.DrawLine(playerPos, rightPoint);
    }


    #endregion

}

[System.Serializable]
public class EntitySpawnConfig
{
    public string name;
    [Header("预制体")] public GameObject prefeb; //预制体
    [Header("存活最大个数")] public int maxGenNumer; //存活最大个数
    [Header("只能夜晚生成")] public bool OnlyGenerateInNight; //只能夜晚生成
    [Header("刷新概率")] public float GenerateProbability; //生成1次的概率
    [Header("一次生成2,3个的概率")] public float MoreGenProbability; //一次生成2,3个的概率
}