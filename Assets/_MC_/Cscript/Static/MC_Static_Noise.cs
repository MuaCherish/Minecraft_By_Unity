using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//PerlinNoise������
public static class MC_Static_Noise 
{

    /// <summary>
    /// ������
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_z"></param>
    /// <param name="_myposition"></param>
    /// <returns></returns>
    public static float GetSimpleNoise(int _x, int _z, Vector3 _myposition)
    {
        float smoothNoise = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((_x + _myposition.x) * 0.01f, (_z + _myposition.z) * 0.01f));
        return smoothNoise;
    }

    /// <summary>
    /// ��ƫ�����������������ȷֲ���
    /// _offset����ΪVector3(111f,222f)
    /// _Scale��������:0.01Ϊ��������,0.1Ϊˮ��ɳ��ֲ�
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_z"></param>
    /// <param name="_myposition"></param>
    /// <param name="_Offset"></param>
    /// <param name="_Scale"></param>
    /// <returns></returns>
    public static float GetSimpleNoiseWithOffset(int _x, int _z, Vector3 _myposition, Vector2 _Offset, float _Scale)
    {
        float smoothNoise = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((_x + _myposition.x + _Offset.x) * _Scale, (_z + _myposition.z + _Offset.y) * _Scale));
        return smoothNoise;
    }

    /// <summary>
    /// �������Ⱥϵ����
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_z"></param>
    /// <param name="_myposition"></param>
    /// <param name="_biomeProperties"></param>
    /// <returns></returns>
    public static byte GetBiomeType(int _x, int _z, Vector3 _myposition, BiomeProperties _biomeProperties)
    {

        float _A = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(0f, 0f, 0f));
        float _B = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(123f, 0f, 456f));
        float _C = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(789f, 0f, 123f));
        float _D = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(456f, 0f, 789f));

        //��ԭ
        if (_B >= _biomeProperties.��ά�ܶ�Density3d)
        {
            return 1;
        }

        else
        {

            //ɳĮ
            if (_C >= _biomeProperties.����̶�Aridity)
            {

                return 2;

            }

            //��ԭ
            else if (_A >= _biomeProperties.����Ũ��OxygenDensity)
            {

                if (_D >= _biomeProperties.����ʪ��MoistureLevel)
                {
                    return 3;
                }
                else
                {
                    return 0;
                }



            }
            else
            {
                //��������
                return 4;
            }

        }


    }

    /// <summary>
    /// ���ݸ���������Ⱥϵ����
    /// ��ɸ�����Ⱥϵ����
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_z"></param>
    /// <param name="_myposition"></param>
    /// <param name="_WorldType"></param>
    /// <param name="_biomeProperties"></param>
    /// <returns></returns>
    public static float GetTotalNoiseHigh_Biome(int _x, int _z, Vector3 _myposition, int _WorldType, BiomeProperties _biomeProperties)
    {
        if (_x < 0 || _x > TerrainData.ChunkWidth || _z < 0 || _z > TerrainData.ChunkWidth)
        {
            //print($"GetTotalNoiseHigh_Biome����,{_x},{_z}");
            return 128f;
        }

        if (_WorldType == TerrainData.Biome_SuperPlain)
        {
            return 0f;
        }


        //Ĭ��
        if (_WorldType == TerrainData.Biome_Default)
        {
            //Noise
            float noise_1 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[0].Noise_Scale_123.x, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[0].Noise_Scale_123.x);
            float noise_2 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[0].Noise_Scale_123.y, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[0].Noise_Scale_123.y);
            float noise_3 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[0].Noise_Scale_123.z, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[0].Noise_Scale_123.z);
            float noise = Mathf.Lerp(0f, 1f, noise_1 * _biomeProperties.biomenoisesystems[0].Noise_Rank_123.x + noise_2 * _biomeProperties.biomenoisesystems[0].Noise_Rank_123.y + noise_3 * _biomeProperties.biomenoisesystems[0].Noise_Rank_123.z);
            float noise_High = Mathf.Lerp(_biomeProperties.biomenoisesystems[0].HighDomain.x, _biomeProperties.biomenoisesystems[0].HighDomain.y, noise);

            //���ݶ���
            int BiomeType = -1;
            float BiomeIntensity = 0f;
            float _A = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(0f, 0f, 0f));
            float _B = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(123f, 0f, 456f));
            float _C = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(789f, 0f, 123f));
            float _D = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(456f, 0f, 789f));

            //��õ�ǰȺϵ
            //���Ⱥϵ���ǿ��
            //��ԭ
            if (_B >= _biomeProperties.��ά�ܶ�Density3d)
            {
                BiomeType = TerrainData.Biome_Plateau;
                BiomeIntensity = Mathf.InverseLerp(_biomeProperties.��ά�ܶ�Density3d, 1f, _B);
            }
            else
            {

                if (_C >= _biomeProperties.����̶�Aridity)
                {
                    BiomeType = TerrainData.Biome_Dessert;
                    BiomeIntensity = Mathf.InverseLerp(_biomeProperties.����̶�Aridity, 1f, _C);
                }
                //��ԭ
                else if (_A >= _biomeProperties.����Ũ��OxygenDensity)
                {
                    if (_D >= _biomeProperties.����ʪ��MoistureLevel)
                    {
                        BiomeType = TerrainData.Biome_Marsh;
                        BiomeIntensity = Mathf.InverseLerp(_biomeProperties.����ʪ��MoistureLevel, 1f, _D);
                    }
                    else
                    {
                        BiomeType = TerrainData.Biome_Plain;
                        BiomeIntensity = Mathf.InverseLerp(_biomeProperties.����Ũ��OxygenDensity, 1f, _A);
                    }
                }
                else
                {
                    BiomeType = TerrainData.Biome_Plain;
                    BiomeIntensity = Mathf.InverseLerp(_biomeProperties.����Ũ��OxygenDensity, 1f, _A);
                }

            }

            //BiomeType = 1;

            //���Ⱥϵ
            float Mixnoise_1 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[BiomeType].Noise_Scale_123.x, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[BiomeType].Noise_Scale_123.x);
            float Mixnoise_2 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[BiomeType].Noise_Scale_123.y, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[BiomeType].Noise_Scale_123.y);
            float Mixnoise_3 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[BiomeType].Noise_Scale_123.z, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[BiomeType].Noise_Scale_123.z);
            float Mixnoise = Mathf.Lerp(0f, 1f, noise_1 * _biomeProperties.biomenoisesystems[BiomeType].Noise_Rank_123.x + noise_2 * _biomeProperties.biomenoisesystems[BiomeType].Noise_Rank_123.y + noise_3 * _biomeProperties.biomenoisesystems[BiomeType].Noise_Rank_123.z);
            float Mixnoise_High = Mathf.Lerp(_biomeProperties.biomenoisesystems[BiomeType].HighDomain.x, _biomeProperties.biomenoisesystems[BiomeType].HighDomain.y, Mixnoise);

            float �������� = Mathf.Lerp(noise_High, Mixnoise_High, BiomeIntensity);

            return ��������;
            //return noise_High + �������� * ���������Ŵ���; 
        }

        else
        {
            //Noise
            float noise_1 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[_WorldType].Noise_Scale_123.x, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[_WorldType].Noise_Scale_123.x);
            float noise_2 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[_WorldType].Noise_Scale_123.y, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[_WorldType].Noise_Scale_123.y);
            float noise_3 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[_WorldType].Noise_Scale_123.z, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[_WorldType].Noise_Scale_123.z);
            float noise = Mathf.Lerp(0f, 1f, noise_1 * _biomeProperties.biomenoisesystems[_WorldType].Noise_Rank_123.x + noise_2 * _biomeProperties.biomenoisesystems[_WorldType].Noise_Rank_123.y + noise_3 * _biomeProperties.biomenoisesystems[_WorldType].Noise_Rank_123.z);
            float noise_High = Mathf.Lerp(_biomeProperties.biomenoisesystems[_WorldType].HighDomain.x, _biomeProperties.biomenoisesystems[_WorldType].HighDomain.y, noise);
            return noise_High;
        }


    }

}
