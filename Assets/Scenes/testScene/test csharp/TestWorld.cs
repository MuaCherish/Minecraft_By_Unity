using JetBrains.Annotations;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
//using System.Diagnostics;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
//using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class TestWorld : MonoBehaviour
{

    [Header("Material-��������")]
    public Material material;
    public Material material_Water;
    public BlockType[] blocktypes;


    [Header("World-��Ⱦ����")]
    public int renderSize = 5;    


    [Header("Cave-��Ѩϵͳ")]
    public float noise3d_scale = 0.085f;
    public float cave_width = 0.45f;


    [Header("Ⱥϵ�������ʺ�����(ֵԽ��ΧԽС)")]
    public float ����Ũ��OxygenDensity;
    public float ��ά�ܶ�Density3d;
    public float ����̶�Aridity;
    public float ����ʪ��MoistureLevel;
    public BiomeNoiseSystem[] biomenoisesystems;


    [Header("���ʷֲ������ϵͳ(n%)(����Ϊ���֮n)")]
    public TerrainLayerProbabilitySystem terrainLayerProbabilitySystem;
    public System.Random rand;


    //Chunks����
    [HideInInspector]
    public GameObject Chunks;




    private void Start()
    {

        //֡��
        //Application.targetFrameRate = 90;

        //Self
        Chunks = new GameObject();
        Chunks.name = "Chunks";
        Chunks.transform.SetParent(GameObject.Find("Environment").transform);

        //��������
        if (terrainLayerProbabilitySystem.isRandomSeed)
            terrainLayerProbabilitySystem.Seed = Random.Range(0, 100);
       
        rand = new System.Random(terrainLayerProbabilitySystem.Seed);

        //sea_level = Random.Range(20, 39);

        //��ʼ��һ��С��
        TestChunk chunk_temp = new TestChunk(new Vector3(0, 0, 0), this, true);

    }


    //----------------------------------Noise Options---------------------------------------

    //������
    public float GetSimpleNoise(int _x, int _z, Vector3 _myposition)
    {
        float smoothNoise = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((_x + _myposition.x) * 0.01f, (_z + _myposition.z) * 0.01f));
        return smoothNoise;
    }

    //��ƫ�����������������ȷֲ���
    //_offset����ΪVector3(111f,222f)
    //_Scale��������:0.01Ϊ��������,0.1Ϊˮ��ɳ��ֲ�
    public float GetSimpleNoiseWithOffset(int _x, int _z, Vector3 _myposition, Vector2 _Offset, float _Scale)
    {
        float smoothNoise = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((_x + _myposition.x + _Offset.x) * _Scale, (_z + _myposition.z + _Offset.y) * _Scale));
        return smoothNoise;
    }

    //�������Ⱥϵ����
    public byte GetBiomeType(int _x, int _z, Vector3 _myposition)
    {

        float _A = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(0f, 0f, 0f));
        float _B = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(123f, 0f, 456f));
        float _C = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(789f, 0f, 123f));
        float _D = GetSimpleNoise((int)(_x + _myposition.x), (int)(_z + _myposition.z), new Vector3(456f, 0f, 789f));

        ////ɳĮ
        //if (_C >= ����̶�Aridity)
        //{

        //    return 2;

        //}

        //else
        //{

        //    //��ԭ
        //    if (_B >= ��ά�ܶ�Density3d)
        //    {
        //        return 1;
        //    }

        //    //��ԭ
        //    else if (_A >= ����Ũ��OxygenDensity)
        //    {

        //        if (_D >= ����ʪ��MoistureLevel)
        //        {
        //            return 3;
        //        }
        //        else
        //        {
        //            return 0;
        //        }



        //    }
        //    else
        //    {
        //        //��������
        //        return 4;
        //    }

        //}

        //��ԭ
        if (_B >= ��ά�ܶ�Density3d)
        {
            return 1;
        }

        else
        {

            //ɳĮ
            if (_C >= ����̶�Aridity)
            {

                return 2;

            }

            //��ԭ
            else if (_A >= ����Ũ��OxygenDensity)
            {

                if (_D >= ����ʪ��MoistureLevel)
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


    //���ݸ���������Ⱥϵ����
    //��ɸ�����Ⱥϵ����
    public float GetTotalNoiseHigh_Biome(int _x, int _z, Vector3 _myposition)
    {
        //Noise
        float noise_1 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[0].Noise_Scale_123.x, (float)(_z + _myposition.z) * biomenoisesystems[0].Noise_Scale_123.x);
        float noise_2 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[0].Noise_Scale_123.y, (float)(_z + _myposition.z) * biomenoisesystems[0].Noise_Scale_123.y);
        float noise_3 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[0].Noise_Scale_123.z, (float)(_z + _myposition.z) * biomenoisesystems[0].Noise_Scale_123.z);
        float noise = Mathf.Lerp(0f, 1f, noise_1 * biomenoisesystems[0].Noise_Rank_123.x + noise_2 * biomenoisesystems[0].Noise_Rank_123.y + noise_3 * biomenoisesystems[0].Noise_Rank_123.z);
        float noise_High = Mathf.Lerp(biomenoisesystems[0].HighDomain.x, biomenoisesystems[0].HighDomain.y, noise);

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
        if (_B >= ��ά�ܶ�Density3d)
        {
            BiomeType = TerrainData.Biome_Plateau;
            BiomeIntensity = Mathf.InverseLerp(��ά�ܶ�Density3d, 1f, _B);
        }
        else
        {

            if (_C >= ����̶�Aridity)
            {
                BiomeType = TerrainData.Biome_Dessert;
                BiomeIntensity = Mathf.InverseLerp(����̶�Aridity, 1f, _C);
            }
            //��ԭ
            else if (_A >= ����Ũ��OxygenDensity)
            {
                if (_D >= ����ʪ��MoistureLevel)
                {
                    BiomeType = TerrainData.Biome_Marsh;
                    BiomeIntensity = Mathf.InverseLerp(����ʪ��MoistureLevel, 1f, _D);
                }
                else
                {
                    BiomeType = TerrainData.Biome_Plain;
                    BiomeIntensity = Mathf.InverseLerp(����Ũ��OxygenDensity, 1f, _A);
                }
            }
            else
            {
                BiomeType = TerrainData.Biome_Plain;
                BiomeIntensity = Mathf.InverseLerp(����Ũ��OxygenDensity, 1f, _A);
            }

        }

        //BiomeType = 1;

        //���Ⱥϵ
        float Mixnoise_1 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[BiomeType].Noise_Scale_123.x, (float)(_z + _myposition.z) * biomenoisesystems[BiomeType].Noise_Scale_123.x);
        float Mixnoise_2 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[BiomeType].Noise_Scale_123.y, (float)(_z + _myposition.z) * biomenoisesystems[BiomeType].Noise_Scale_123.y);
        float Mixnoise_3 = Mathf.PerlinNoise((float)(_x + _myposition.x) * biomenoisesystems[BiomeType].Noise_Scale_123.z, (float)(_z + _myposition.z) * biomenoisesystems[BiomeType].Noise_Scale_123.z);
        float Mixnoise = Mathf.Lerp(0f, 1f, noise_1 * biomenoisesystems[BiomeType].Noise_Rank_123.x + noise_2 * biomenoisesystems[BiomeType].Noise_Rank_123.y + noise_3 * biomenoisesystems[BiomeType].Noise_Rank_123.z);
        float Mixnoise_High = Mathf.Lerp(biomenoisesystems[BiomeType].HighDomain.x, biomenoisesystems[BiomeType].HighDomain.y, Mixnoise);

        float �������� = Mathf.Lerp(noise_High, Mixnoise_High, BiomeIntensity);

        return ��������;
        //return noise_High + �������� * ���������Ŵ���; 

    }

}