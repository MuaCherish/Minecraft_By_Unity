using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

//ʵ�ú���
public static class UsefulFunction
{

    #region ��С�� 

    public static float Delta = 0.01f;
    public static float Delta_Pro = 0.0125f;

    #endregion


    #region Enum

    /// <summary>
    /// ����
    /// </summary>
    public enum BlockDirection
    {
        ǰ,
        ��,
        ��,
        ��,
        ��,
        ��
    }

    #endregion


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

    #endregion


    #region Vector3

    /// <summary>
    /// ����������ɾ�������
    /// </summary>
    /// <param name="_vec"></param>
    /// <returns></returns>
    public static Vector3 GetRealPos(Vector3 _vec, Vector3 _ChunkLocation)
    {
        return Vector3.zero;
    }

    /// <summary>
    /// �����������Ϊ�������
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public static Vector3 GetRelaPos(Vector3 _pos)
    {
        return new Vector3(Mathf.FloorToInt(_pos.x % TerrainData.ChunkWidth), Mathf.FloorToInt(_pos.y) % TerrainData.ChunkHeight, Mathf.FloorToInt(_pos.z % TerrainData.ChunkWidth));
    }

    /// <summary>
    /// ����Int���͵�Vector3
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public static Vector3 GetIntVector3(Vector3 _pos)
    {
        return new Vector3((int)_pos.x, (int)_pos.y, (int)_pos.z);
    }

    /// <summary>
    /// ��������������꣬���������ڷ�����������꣬ע��ֻ��0.5f
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
    /// ���ظ���ֵ
    /// </summary>
    /// <param name="_Probability"></param>
    /// <returns></returns>
    public static bool GetProbability(float _Probability)
    {
        // ȷ������ֵ�� 0 �� 100 ֮��
        _Probability = Mathf.Clamp(_Probability, 0, 100);

        // ����һ�� 0 �� 100 ֮��������
        float randomValue = UnityEngine.Random.Range(0f, 100f);

        // ��������С�ڵ�������ֵ���򷵻� true
        //Debug.Log(randomValue);
        bool a = randomValue < _Probability;

        return a;
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


    #region ���

    /// <summary>
    /// ���Ϊ true�������ز��̶���ꣻ���Ϊ false������ʾ���������
    /// </summary>
    /// <param name="isLocked">���Ϊ true�������ز��̶���ꣻ���Ϊ false������ʾ���������</param>
    public static void LockMouse(bool isLocked)
    {
        if (isLocked)
        {
            //print("�������");
            // ��������겢������������Ļ����
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            //print("��ʾ���");
            // ��ʾ����겢�������
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }


    #endregion


}
