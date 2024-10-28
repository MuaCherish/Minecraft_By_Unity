using UnityEngine;

namespace MCEntity
{
    //实体接口
    public interface IEntity
    {
        // 初始化
        public void OnStartEntity();

        // 销毁
        public void OnEndEntity();
    }

    //一些Entity需要的公用函数
    public static class MC_UtilityFunctions
    {
        /// <summary>
        /// 射线检测判断是否看到某物体
        /// </summary>
        public static bool IsTargetVisible(Vector3 _targetPos)
        {
            // 实现射线检测的逻辑
            return true;
        }

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

    }

}
