using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MC_Static_Math
{



    #region Vector3

    /// <summary>
    /// 返回距离中心点甜甜圈范围内的随机点
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radiusRange"></param>
    /// <returns></returns>
    public static Vector3 GetRandomPointInDonut(Vector3 center, Vector2 radiusRange)
    {
        float angle = Random.Range(0f, Mathf.PI * 2); // 随机角度
        float radius = Mathf.Sqrt(Random.Range(radiusRange.x * radiusRange.x, radiusRange.y * radiusRange.y)); // 确保均匀分布
        return new Vector3(center.x + Mathf.Cos(angle) * radius, center.y, center.z + Mathf.Sin(angle) * radius);
    }



    /// <summary>
    /// 获取所在区块坐标虚拟坐标
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3 GetRelaChunkLocation(Vector3 vec)
    {

        return new Vector3((vec.x - vec.x % TerrainData.ChunkWidth) / TerrainData.ChunkWidth, 0, (vec.z - vec.z % TerrainData.ChunkWidth) / TerrainData.ChunkWidth);

    }

    /// <summary>
    /// 获取所在区块坐标世界坐标
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3 GetRealChunkLocation(Vector3 vec)
    {

        return new Vector3(16f * ((vec.x - vec.x % TerrainData.ChunkWidth) / TerrainData.ChunkWidth), 0, 16f * ((vec.z - vec.z % TerrainData.ChunkWidth) / TerrainData.ChunkWidth));

    }


    /// <summary>
    ///  将输入向量归一化到x或z轴
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector3 NormalizeToAxis(Vector3 v)
    {
        // 选择 x 和 z 分量绝对值较大的轴
        if (Mathf.Abs(v.x) >= Mathf.Abs(v.z))
        {
            return new Vector3(Mathf.Sign(v.x), 0, 0); // 返回 x 轴单位向量，保持 x 的符号
        }
        else
        {
            return new Vector3(0, 0, Mathf.Sign(v.z)); // 返回 z 轴单位向量，保持 z 的符号
        }
    }

    /// <summary>
    /// 求Vector3的2d长度
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static float Get2DLengthforVector3(Vector3 vec)
    {
        Vector2 vector2 = new Vector2(vec.x, vec.z);
        return vector2.magnitude;
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
    /// 将相对坐标改为绝对坐标
    /// </summary>
    /// <returns></returns>
    public static Vector3 GetRealPos(Vector3 _vec, Vector3 _ChunkRealLocation)
    {
        return _ChunkRealLocation + _vec;
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


    #region Probability

    /// <summary>
    /// 给定在多少秒内触发一次的概率
    /// 返回update检测的概率，用于每一帧检测的函数
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="targetProbability"></param>
    /// <param name="frameRate"></param>
    /// <returns></returns>
    public static float CalculateFrameProbability(float duration, float targetProbability, float frameRate = 60f)
    {
        int totalFrames = Mathf.RoundToInt(duration * frameRate); // 计算总帧数
        return 1 - Mathf.Pow(1 - targetProbability, 1f / totalFrames); // 计算每帧触发概率
    }


    /// <summary>
    /// 根据给定概率返回 true 或 false
    /// </summary>
    /// <param name="_Probability">概率值（0~1），例如 0.1 表示 10% 概率</param>
    /// <returns>是否触发</returns>
    public static bool GetProbability(float _Probability)
    {
        // 确保概率值在 0 到 1 之间
        _Probability = Mathf.Clamp(_Probability, 0f, 1f);

        return Random.value < _Probability;
    }


    #endregion



}
