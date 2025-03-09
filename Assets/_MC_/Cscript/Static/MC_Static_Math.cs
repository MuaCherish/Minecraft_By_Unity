using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MC_Static_Math
{



    #region Vector3

    /// <summary>
    /// ���ؾ������ĵ�����Ȧ��Χ�ڵ������
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radiusRange"></param>
    /// <returns></returns>
    public static Vector3 GetRandomPointInDonut(Vector3 center, Vector2 radiusRange)
    {
        float angle = Random.Range(0f, Mathf.PI * 2); // ����Ƕ�
        float radius = Mathf.Sqrt(Random.Range(radiusRange.x * radiusRange.x, radiusRange.y * radiusRange.y)); // ȷ�����ȷֲ�
        return new Vector3(center.x + Mathf.Cos(angle) * radius, center.y, center.z + Mathf.Sin(angle) * radius);
    }



    /// <summary>
    /// ��ȡ��������������������
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3 GetRelaChunkLocation(Vector3 vec)
    {

        return new Vector3((vec.x - vec.x % TerrainData.ChunkWidth) / TerrainData.ChunkWidth, 0, (vec.z - vec.z % TerrainData.ChunkWidth) / TerrainData.ChunkWidth);

    }

    /// <summary>
    /// ��ȡ��������������������
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3 GetRealChunkLocation(Vector3 vec)
    {

        return new Vector3(16f * ((vec.x - vec.x % TerrainData.ChunkWidth) / TerrainData.ChunkWidth), 0, 16f * ((vec.z - vec.z % TerrainData.ChunkWidth) / TerrainData.ChunkWidth));

    }


    /// <summary>
    ///  ������������һ����x��z��
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector3 NormalizeToAxis(Vector3 v)
    {
        // ѡ�� x �� z ��������ֵ�ϴ����
        if (Mathf.Abs(v.x) >= Mathf.Abs(v.z))
        {
            return new Vector3(Mathf.Sign(v.x), 0, 0); // ���� x �ᵥλ���������� x �ķ���
        }
        else
        {
            return new Vector3(0, 0, Mathf.Sign(v.z)); // ���� z �ᵥλ���������� z �ķ���
        }
    }

    /// <summary>
    /// ��Vector3��2d����
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static float Get2DLengthforVector3(Vector3 vec)
    {
        Vector2 vector2 = new Vector2(vec.x, vec.z);
        return vector2.magnitude;
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
    /// ����������Ϊ��������
    /// </summary>
    /// <returns></returns>
    public static Vector3 GetRealPos(Vector3 _vec, Vector3 _ChunkRealLocation)
    {
        return _ChunkRealLocation + _vec;
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


    /// <summary>
    /// ŷ������㷨
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
    /// ����Vector3���鲢���������飬ͬʱ��ָ�����������λ
    /// </summary>
    /// <param name="array"></param>
    /// <param name="_FirstDirect"></param>
    /// <returns></returns>
    public static Vector3[] ShuffleArray(Vector3[] array, Vector3 _FirstDirect)
    {
        // �������飬�����޸�ԭ����
        Vector3[] shuffledArray = (Vector3[])array.Clone();

        // ����_FirstDirect�������е�����λ��
        int firstDirectIndex = System.Array.IndexOf(shuffledArray, _FirstDirect);

        // ����ҵ���ָ���ķ��򣬾ͽ����Ƶ��������λ
        if (firstDirectIndex >= 0)
        {
            // �� _FirstDirect �ŵ��������λ
            Vector3 temp = shuffledArray[firstDirectIndex];
            shuffledArray[firstDirectIndex] = shuffledArray[0];
            shuffledArray[0] = temp;
        }

        // ϴ��ʣ�ಿ�֣�������1��ʼϴ�ƣ�
        System.Random rng = new System.Random();
        for (int i = shuffledArray.Length - 1; i > 1; i--)  // �ӵڶ���Ԫ�ؿ�ʼϴ��
        {
            int j = rng.Next(1, i + 1);  // ���� 1 �� i ֮����������
                                         // ����Ԫ��
            (shuffledArray[i], shuffledArray[j]) = (shuffledArray[j], shuffledArray[i]);
        }

        return shuffledArray;
    }

    #endregion


    #region Probability

    /// <summary>
    /// �����ڶ������ڴ���һ�εĸ���
    /// ����update���ĸ��ʣ�����ÿһ֡���ĺ���
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="targetProbability"></param>
    /// <param name="frameRate"></param>
    /// <returns></returns>
    public static float CalculateFrameProbability(float duration, float targetProbability, float frameRate = 60f)
    {
        int totalFrames = Mathf.RoundToInt(duration * frameRate); // ������֡��
        return 1 - Mathf.Pow(1 - targetProbability, 1f / totalFrames); // ����ÿ֡��������
    }


    /// <summary>
    /// ���ݸ������ʷ��� true �� false
    /// </summary>
    /// <param name="_Probability">����ֵ��0~1�������� 0.1 ��ʾ 10% ����</param>
    /// <returns>�Ƿ񴥷�</returns>
    public static bool GetProbability(float _Probability)
    {
        // ȷ������ֵ�� 0 �� 1 ֮��
        _Probability = Mathf.Clamp(_Probability, 0f, 1f);

        return Random.value < _Probability;
    }


    #endregion



}
