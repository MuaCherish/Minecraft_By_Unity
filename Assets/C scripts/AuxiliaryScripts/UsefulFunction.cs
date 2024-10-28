using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

//ʵ�ú���
public static class UsefulFunction
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
        return Vector3.zero;
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


}
