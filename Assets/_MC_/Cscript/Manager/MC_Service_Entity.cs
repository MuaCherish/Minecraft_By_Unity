using Homebrew;
using MCEntity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MC_Service_Entity : MonoBehaviour
{

    #region 周期函数

    ManagerHub managerhub;
    Player player;
    World world;
    private void Awake()
    {
        managerhub = SceneData.GetManagerhub();
        player = managerhub.player;
        Entity_Parent = GameObject.Find("Entity");
        world = managerhub.world;
    }

    private void Update()
    {
        switch (world.game_state)
        {
            case Game_State.Playing:
                Handle_GameState_Playing();
                break;
        }
    }

    void Handle_GameState_Playing()
    {
        _ReferUpdate_CheckShowEntityHitbox();
    }


    #endregion


    #region 实体管理

    [Foldout("实体管理", true)]
    [Header("蒸汽粒子")] public GameObject Evaporation_Particle;
    [Header("实体预制体")] public GameObject[] Entity_Prefeb;
    [Header("活着的所有实体")] public List<EntityInfo> AllEntity = new List<EntityInfo>();
    [Header("最大实体数量")][SerializeField] private int maxSize = 100; // 默认值为100，可在Inspector中调整
    private int Unique_Id = 0; // 用于生成新的唯一ID
    GameObject Entity_Parent;

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
    public bool AddEntity(int _index, Vector3 _Startpos, out EntityInfo _Result)
    {
        // 提起返回-检查实体数量是否达到最大值
        if (AllEntity.Count >= maxSize)
        {
            Debug.LogWarning("实体数量已达到最大值，无法添加新实体！");
            _Result = null;
            return false;
        }

        // 提起返回-检查下标是否有效
        if (_index < 0 || _index >= Entity_Prefeb.Length)
        {
            Debug.LogError("索引超出范围，请提供有效的预制体索引！");
            _Result = null;
            return false;
        }

        // 实例化预制体
        GameObject newEntity = Instantiate(Entity_Prefeb[_index]);

        // 提起返回-如果没有注册组件
        if (newEntity.GetComponent<MC_Component_Registration>() == null)
        {
            print($"{_index}实体未添加注册组件，无法添加到游戏中");
            _Result = null;
            Destroy(newEntity);
            return false;
        }


        // 生成一个唯一的ID
        int entityId = Unique_Id++;

        //生成名字
        string entityName = EntityData.GetEntityName(_index);

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
            AllEntity.Remove(entityToRemove);
            //Debug.Log($"实体 {_EntityID._name} 已移除成功！");
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

}
