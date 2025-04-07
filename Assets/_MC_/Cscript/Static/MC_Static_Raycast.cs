using MCEntity;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

/// <summary>
/// 射线检测类
/// </summary>
public static class MC_Static_Raycast
{
    /// <summary>
    /// 射线检测
    /// </summary>
    /// <param name="_world">world</param>
    /// <param name="_origin">起始点</param>
    /// <param name="_direct">方向</param>
    /// <param name="_maxDistance">射线最大长度</param>
    /// <param name="castingEntityId">排除自己的Id</param>
    /// <param name="checkIncrement">步进最小长度</param>
    /// <returns></returns>
    public static MC_RayCastStruct RayCast(ManagerHub _managerhub, MC_RayCast_FindType _FindType, Vector3 _origin, Vector3 _direct, float _maxDistance, int castingEntityId, float checkIncrement, bool debug = false)
    {
        // 预处理
        _direct.Normalize();

        // 初始化结构体
        float step = 0f;
        Vector3 lastPos = new Vector3();
        Vector3 hitPoint = Vector3.zero;
        byte blockType = 255;
        Vector3 hitNormal = Vector3.zero;
        float rayDistance = _maxDistance;
        byte isHit = 0;
        EntityInfo targetEntity = new EntityInfo(-1, "Unknown Entity", null);

        // 从射线起点开始，沿目标方向进行检测
        while (step < _maxDistance)
        {
            // 当前射线所在的点
            Vector3 pos = _origin + (_direct * step);

            // 提前返回-如果y坐标小于0
            if (pos.y < 0)
                pos = new Vector3(pos.x, 0, pos.z);

            // 提前返回-如果已经命中
            if (isHit != 0)
                break;

            // 实体命中检测
            if (_FindType != MC_RayCast_FindType.OnlyFindBlock && targetEntity._id == -1)
            {
                // 获取范围内的实体
                if (_managerhub.Service_Entity.GetOverlapSphereEntity(_origin, _maxDistance, out var entitiesInRange))
                {
                    // 检查是否有实体与射线相交，并且该实体与射线碰撞
                    foreach (var entity in entitiesInRange)
                    {
                        // 排除当前实体自身
                        if (entity._id == castingEntityId)
                            continue;

                        // 获取实体的碰撞检测组件
                        var collider = entity._obj.GetComponent<MC_Component_Physics>();
                        if (collider != null && collider.CheckHitBox(pos))
                        {
                            targetEntity._id = entity._id;
                            targetEntity._obj = entity._obj;
                            isHit = 2;
                            break; // 找到第一个符合条件的实体，退出循环
                        }
                    }
                }
            }

            // 方块命中检测
            if (_FindType != MC_RayCast_FindType.OnlyFindEntity && _managerhub.Service_Chunk.RayCheckForVoxel(pos))
            {
                // 记录命中点
                hitPoint = pos;
                isHit = 1; // 记录命中

                // 获取命中的方块类型
                blockType = _managerhub.Service_Chunk.GetBlockType(pos);

                // 计算命中的法线方向，基于命中点的相对位置判断法线单位向量
                Vector3 blockCenter = new Vector3(Mathf.Floor(hitPoint.x) + 0.5f, Mathf.Floor(hitPoint.y) + 0.5f, Mathf.Floor(hitPoint.z) + 0.5f);
                Vector3 relativePos = hitPoint - blockCenter;

                // 计算法线
                if (Mathf.Abs(relativePos.x) > Mathf.Abs(relativePos.y) && Mathf.Abs(relativePos.x) > Mathf.Abs(relativePos.z))
                    hitNormal = new Vector3(Mathf.Sign(relativePos.x), 0, 0);
                else if (Mathf.Abs(relativePos.y) > Mathf.Abs(relativePos.x) && Mathf.Abs(relativePos.y) > Mathf.Abs(relativePos.z))
                    hitNormal = new Vector3(0, Mathf.Sign(relativePos.y), 0);
                else
                    hitNormal = new Vector3(0, 0, Mathf.Sign(relativePos.z));

                // 计算射线距离
                rayDistance = (pos - _origin).magnitude;

                // 命中后跳出循环
                break;
            }

            // 调试模式：绘制射线
            if (debug)
            {
                Debug.DrawRay(_origin, _direct * step, Color.red); // 绘制射线
            }

            // 更新
            lastPos = pos;
            step += checkIncrement;
        }

        // 返回结果
        return new MC_RayCastStruct
        {
            isHit = isHit,
            rayOrigin = _origin,
            hitPoint = hitPoint,
            hitPoint_Previous = lastPos,
            blockType = blockType,
            hitNormal = hitNormal,
            rayDistance = rayDistance,
            targetEntityInfo = targetEntity._id != -1 ? targetEntity : new EntityInfo(-1, "Unknown Entity", null),
        };
    }


}

//返回结构体
[System.Serializable]
public struct MC_RayCastStruct
{
    /// <summary>
    /// 是否命中: 0没有命中, 1命中方块, 2命中实体
    /// </summary>
    public byte isHit;
    public Vector3 rayOrigin; // 射线起点
    public Vector3 hitPoint;// 打中点坐标
    public Vector3 hitPoint_Previous;// 打中前一点坐标
    public byte blockType;  // 打中方块类型
    public Vector3 hitNormal;// 打中法线方向
    public float rayDistance; // 射线距离
    public EntityInfo targetEntityInfo;// 目标实体（可为空）

    // 构造函数
    public MC_RayCastStruct(byte isHit, Vector3 rayOrigin, Vector3 hitPoint, Vector3 hitPoint_Previous, byte blockType, Vector3 hitNormal, float rayDistance, EntityInfo targetEntityInfo)
    {
        this.isHit = isHit;
        this.rayOrigin = rayOrigin;
        this.hitPoint = hitPoint;
        this.hitPoint_Previous = hitPoint_Previous;
        this.blockType = blockType;
        this.hitNormal = hitNormal;
        this.rayDistance = rayDistance;
        this.targetEntityInfo = targetEntityInfo;
    }

    // 覆盖ToString方法，用于打印输出
    public override string ToString()
    {
        return $"RayCastStruct: \n" +
               $"  Is Hit: {isHit}\n" +
               $"  Ray Origin: {rayOrigin}\n" +
               $"  Hit Point: {hitPoint}\n" +
               $"  Previous Hit Point: {hitPoint_Previous}\n" +
               $"  Block Type: {blockType}\n" +
               $"  Hit Normal: {hitNormal}\n" +
               $"  Ray Distance: {rayDistance}\n" +
               $"  Target Entity: {targetEntityInfo._id}, {targetEntityInfo._name}, {targetEntityInfo._obj}";
    }
}

//搜索模式
[System.Serializable]
public enum MC_RayCast_FindType
{
    AllFind,
    OnlyFindBlock,
    OnlyFindEntity,
}
