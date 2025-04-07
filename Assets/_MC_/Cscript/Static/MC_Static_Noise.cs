using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//PerlinNoise噪声类
public static class MC_Static_Noise 
{

    /// <summary>
    /// 简单噪声
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
    /// 简单偏移噪声，用来给树等分布用
    /// _offset类似为Vector3(111f,222f)
    /// _Scale噪声缩放:0.01为正常缩放,0.1为水下沙泥分布
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
    /// 获得所在群系类型
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

        //高原
        if (_B >= _biomeProperties.三维密度Density3d)
        {
            return 1;
        }

        else
        {

            //沙漠
            if (_C >= _biomeProperties.干燥程度Aridity)
            {

                return 2;

            }

            //草原
            else if (_A >= _biomeProperties.氧气浓度OxygenDensity)
            {

                if (_D >= _biomeProperties.空气湿度MoistureLevel)
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
                //返回密林
                return 4;
            }

        }


    }

    /// <summary>
    /// 根据给定参数和群系种类
    /// 变成给定的群系噪声
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
            //print($"GetTotalNoiseHigh_Biome出界,{_x},{_z}");
            return 128f;
        }

        if (_WorldType == TerrainData.Biome_SuperPlain)
        {
            return 0f;
        }


        //默认
        if (_WorldType == TerrainData.Biome_Default)
        {
            //Noise
            float noise_1 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[0].Noise_Scale_123.x, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[0].Noise_Scale_123.x);
            float noise_2 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[0].Noise_Scale_123.y, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[0].Noise_Scale_123.y);
            float noise_3 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[0].Noise_Scale_123.z, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[0].Noise_Scale_123.z);
            float noise = Mathf.Lerp(0f, 1f, noise_1 * _biomeProperties.biomenoisesystems[0].Noise_Rank_123.x + noise_2 * _biomeProperties.biomenoisesystems[0].Noise_Rank_123.y + noise_3 * _biomeProperties.biomenoisesystems[0].Noise_Rank_123.z);
            float noise_High = Mathf.Lerp(_biomeProperties.biomenoisesystems[0].HighDomain.x, _biomeProperties.biomenoisesystems[0].HighDomain.y, noise);

            //数据定义
            int BiomeType = -1;
            float BiomeIntensity = 0f;
            float _A = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(0f, 0f, 0f));
            float _B = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(123f, 0f, 456f));
            float _C = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(789f, 0f, 123f));
            float _D = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(456f, 0f, 789f));

            //获得当前群系
            //获得群系混合强度
            //高原
            if (_B >= _biomeProperties.三维密度Density3d)
            {
                BiomeType = TerrainData.Biome_Plateau;
                BiomeIntensity = Mathf.InverseLerp(_biomeProperties.三维密度Density3d, 1f, _B);
            }
            else
            {

                if (_C >= _biomeProperties.干燥程度Aridity)
                {
                    BiomeType = TerrainData.Biome_Dessert;
                    BiomeIntensity = Mathf.InverseLerp(_biomeProperties.干燥程度Aridity, 1f, _C);
                }
                //草原
                else if (_A >= _biomeProperties.氧气浓度OxygenDensity)
                {
                    if (_D >= _biomeProperties.空气湿度MoistureLevel)
                    {
                        BiomeType = TerrainData.Biome_Marsh;
                        BiomeIntensity = Mathf.InverseLerp(_biomeProperties.空气湿度MoistureLevel, 1f, _D);
                    }
                    else
                    {
                        BiomeType = TerrainData.Biome_Plain;
                        BiomeIntensity = Mathf.InverseLerp(_biomeProperties.氧气浓度OxygenDensity, 1f, _A);
                    }
                }
                else
                {
                    BiomeType = TerrainData.Biome_Plain;
                    BiomeIntensity = Mathf.InverseLerp(_biomeProperties.氧气浓度OxygenDensity, 1f, _A);
                }

            }

            //BiomeType = 1;

            //混合群系
            float Mixnoise_1 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[BiomeType].Noise_Scale_123.x, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[BiomeType].Noise_Scale_123.x);
            float Mixnoise_2 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[BiomeType].Noise_Scale_123.y, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[BiomeType].Noise_Scale_123.y);
            float Mixnoise_3 = Mathf.PerlinNoise((float)(_x + _myposition.x) * _biomeProperties.biomenoisesystems[BiomeType].Noise_Scale_123.z, (float)(_z + _myposition.z) * _biomeProperties.biomenoisesystems[BiomeType].Noise_Scale_123.z);
            float Mixnoise = Mathf.Lerp(0f, 1f, noise_1 * _biomeProperties.biomenoisesystems[BiomeType].Noise_Rank_123.x + noise_2 * _biomeProperties.biomenoisesystems[BiomeType].Noise_Rank_123.y + noise_3 * _biomeProperties.biomenoisesystems[BiomeType].Noise_Rank_123.z);
            float Mixnoise_High = Mathf.Lerp(_biomeProperties.biomenoisesystems[BiomeType].HighDomain.x, _biomeProperties.biomenoisesystems[BiomeType].HighDomain.y, Mixnoise);

            float 增量噪声 = Mathf.Lerp(noise_High, Mixnoise_High, BiomeIntensity);

            return 增量噪声;
            //return noise_High + 增量噪声 * 增量噪声放大倍数; 
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
