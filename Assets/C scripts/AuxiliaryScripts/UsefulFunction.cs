using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

//实用函数
public static class UsefulFunction
{

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
        return Vector3.zero;
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


}
