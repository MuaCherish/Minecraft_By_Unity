using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MC_Static_Math;


public class MC_Static_Chunk 
{

    #region BlockType

    /// <summary>
    /// �����������꣬���������͡�<para/>
    /// �������Զ�����״��
    /// </summary>
    /// <param name="_pos">��������</param>
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
            if (_world == null)
            {
                _world = SceneData.GetManagerhub().world; // �ڵ�һ�η���ʱ����
            }
            return _world;
        }
    }

    private static World _world;  // ��̬�����洢 World ʵ��


    /// <summary>
    /// �Ƿ��ǹ���
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


    #region �����ж�

    /// <summary>
    /// ���Ի�����������ж��Ƿ����
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public static bool isOutOfChunkRange(Vector3 _pos)
    {
        //��ȡ�������
        Vector3 _vec = GetRelaPos(_pos);

        //�Ƿ����
        int _x = (int)_vec.x;
        int _y = (int)_vec.y;
        int _z = (int)_vec.z;

        if (_x < 0 || _x > TerrainData.ChunkWidth - 1 || _y < 0 || _y > TerrainData.ChunkHeight - 1 || _z < 0 || _z > TerrainData.ChunkWidth - 1)
            return true;
        else
            return false;
    }

    /// <summary>
    /// ����ֵ�ж�
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_y"></param>
    /// <param name="_z"></param>
    /// <returns></returns>
    public static bool isOutOfChunkRange(int _x, int _y, int _z)
    {
        //�Ƿ����
        if (_x < 0 || _x > TerrainData.ChunkWidth - 1 || _y < 0 || _y > TerrainData.ChunkHeight - 1 || _z < 0 || _z > TerrainData.ChunkWidth - 1)
            return true;
        else
            return false;

    }


    #endregion


}
