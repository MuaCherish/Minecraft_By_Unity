using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MC_Static_Math;


public class MC_Static_Chunk 
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

    /// <summary>
    /// managerhub 
    /// </summary>
    public static ManagerHub managerhub
    {
        get
        {
            if (_managerhub == null)
            {
                _managerhub = SceneData.GetManagerhub(); // 在第一次访问时创建
            }
            return _managerhub;
        }
    }

    private static ManagerHub _managerhub;  // 静态变量存储 World 实例



    /// <summary>
    /// World
    /// </summary>
    public static MC_Service_World Service_world
    {
        get
        {
            if (_Service_world == null)
            {
                _Service_world = SceneData.GetManagerhub().Service_World; // 在第一次访问时创建
            }
            return _Service_world;
        }
    }

    private static MC_Service_World _Service_world;  // 静态变量存储 World 实例


    /// <summary>
    /// 是否是固体
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public static bool CheckSolid(Vector3 _pos)
    {
        byte _BlockType = managerhub.Service_World.GetBlockType(_pos);

        if (_BlockType == 255 || MC_Runtime_StaticData.Instance.ItemData.items[_BlockType].isSolid)
        {
            return true;
        }
        else
        {
            return false;
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


}
