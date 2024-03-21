using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainTest_Cube : MonoBehaviour
{
    public Material Mymaterial;
    public GameObject Cube;
    public Transform Cubes;

    public int Terrain_Width; private int _Terrain_Width;

    public float soil_min; private float _soil_min;
    public float soil_max; private float _soil_max;

    public float noise2d_scale_smooth; private float _noise2d_scale_smooth;
    public float noise2d_scale_steep; private float _noise2d_scale_steep;


    private void Start()
    {
        ////�жϿ���
        //float noise3d = Perlin3D((float)x * noise3d_scale + myposition.x * noise3d_scale, (float)y * noise3d_scale + y * noise3d_scale, (float)z * noise3d_scale + myposition.z * noise3d_scale); // ��100��Ϊ0.1


        updateValue();
        UpdateTerrain();
    }

    private void Update()
    {
        if (isValueChange())
        {
            updateValue();
            UpdateTerrain();
        }
    }


    //ֵ�ı�ʱ����
    public bool isValueChange()
    {
        if (_Terrain_Width != Terrain_Width ||
            _soil_min != soil_min ||
            _soil_max != soil_max ||
            _noise2d_scale_smooth != noise2d_scale_smooth ||
            _noise2d_scale_steep != noise2d_scale_steep
            )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //����ֵ
    public void updateValue()
    {
        _Terrain_Width = Terrain_Width;
        _soil_min = soil_min;
        _soil_max = soil_max;
        _noise2d_scale_smooth = noise2d_scale_smooth;
        _noise2d_scale_steep = noise2d_scale_steep;
    }

    //���µ���
    public void UpdateTerrain()
    {
        ClearClones();

        for (int z = 0; z < Terrain_Width; z++)
        {
            for (int x = 0; x < Terrain_Width; x++)
            {
                // ���� Cube ��¡
                GameObject cubeClone = GameObject.Instantiate(Cube, new Vector3(x, 0, z), Quaternion.identity);
                cubeClone.transform.parent = Cubes.transform;

                // ����߶�
                float noise2d_1 = Mathf.Lerp((float)soil_min, (float)soil_max, Mathf.PerlinNoise((float)x * noise2d_scale_smooth + x * noise2d_scale_smooth, (float)z * noise2d_scale_smooth + z * noise2d_scale_smooth));
                float noise2d_2 = Mathf.Lerp((float)(soil_min), (float)soil_max, Mathf.PerlinNoise((float)x * noise2d_scale_steep + x * noise2d_scale_steep, (float)z * noise2d_scale_steep + z * noise2d_scale_steep));
                float noise2d_3 = Mathf.Lerp((float)(soil_min), (float)soil_max, Mathf.PerlinNoise((float)x * 0.1f + x * 0.1f, (float)z * 0.15f + z * 0.15f));
                float noiseHigh = noise2d_1 * 0.6f + noise2d_2 * 0.4f + noise2d_3 * 0.05f;

                // ���� Cube ��¡�ĸ߶�
                cubeClone.transform.localPosition = new Vector3(x, Mathf.FloorToInt(noiseHigh), z);

                // ���ݵ�ǰ�߶����ò�����ɫ
                Renderer renderer = cubeClone.GetComponent<Renderer>();
                if (renderer != null)
                {
                    float currentHeight = noiseHigh;
                    Color color = Color.Lerp(Color.blue, Color.red, Mathf.InverseLerp(soil_min, soil_max, currentHeight));

                    // ���������Ĳ���ʵ��
                    Material cubeMaterial = new Material(Mymaterial);
                    cubeMaterial.color = color;

                    // ���� Cube �Ĳ���
                    renderer.material = cubeMaterial;
                }




            }
        }
    }


    public void ClearClones()
    {
        // ����������������Ӷ��󣬲���������
        for (int i = 0; i < Cubes.childCount; i++)
        {
            GameObject.Destroy(Cubes.GetChild(i).gameObject);
        }
    }

    //3d����
    public static float Perlin3D(float x, float y, float z)
    {

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);

        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        float ABC = AB + BC + AC + BA + CB + CA;
        return ABC / 6f;

    }



}
