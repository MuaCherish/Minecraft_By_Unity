using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

//实用函数
public static class UsefulFunction
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


}
