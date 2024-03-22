using UnityEngine;
using UnityEngine.UIElements;

public class TerrainTest_Voxel : MonoBehaviour
{
    public Material Mymaterial;

    public float clarrfy_biome; private float _clarrfy_biome;

    public int Terrain_Width; private int _Terrain_Width;

    public float soil_min; private float _soil_min;
    public float soil_max; private float _soil_max;

    public float noise2d_scale_smooth; private float _noise2d_scale_smooth;
    public float noise2d_scale_steep; private float _noise2d_scale_steep;

    public float scale_Smooth; private float _scale_Smooth;
    public float scale_Steep; private float _scale_Steep;

    [Header("平原")]
    public float 平原最高; private float _平原最高;
    public float 平原Smooth; private float _平原Smooth;
    public float 平原Steep; private float _平原Steep;

    [Header("山脉")]
    public float 山脉最高; private float _山脉最高;
    public float 山脉Smooth; private float _山脉Smooth;
    public float 山脉Steep; private float _山脉Steep;

    private Vector3 myposition;

    private MeshFilter meshFilter;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();

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

    //值改变时触发
    public bool isValueChange()
    {
        if (_Terrain_Width != Terrain_Width ||
            _soil_min != soil_min ||
            _soil_max != soil_max ||
            _noise2d_scale_smooth != noise2d_scale_smooth ||
            _noise2d_scale_steep != noise2d_scale_steep ||
            myposition != transform.position ||
            _clarrfy_biome != clarrfy_biome ||
            _scale_Smooth != scale_Smooth ||
            _scale_Steep != scale_Steep ||
            _平原最高 != 平原最高 ||
            _平原Smooth != 平原Smooth ||
            _平原Steep != 平原Steep ||
            _山脉最高 != 山脉最高 ||
            _山脉Smooth != 山脉Smooth ||
            _山脉Steep != 山脉Steep
            )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //更新值
    public void updateValue()
    {
        _Terrain_Width = Terrain_Width;
        _soil_min = soil_min;
        _soil_max = soil_max;
        _noise2d_scale_smooth = noise2d_scale_smooth;
        _noise2d_scale_steep = noise2d_scale_steep;
        myposition = transform.position;
        _clarrfy_biome = clarrfy_biome;
        _scale_Smooth = scale_Smooth;
        _scale_Steep = scale_Steep;
        _平原最高 = 平原最高;
        _平原Smooth = 平原Smooth;
        _平原Steep = 平原Steep;
        _山脉最高 = 山脉最高;
        _山脉Smooth = 山脉Smooth;
        _山脉Steep = 山脉Steep;

    }

    //返回2d噪声最终高度
    float GetTotalNoiseHigh(int _x, int _z)
    {
        

        float biome_moutainAndPlane = Mathf.Lerp((float)(0), (float)10, Mathf.PerlinNoise((float)_x * noise2d_scale_smooth + myposition.x * noise2d_scale_smooth, (float)_z * noise2d_scale_smooth + myposition.z * noise2d_scale_smooth));

        if (biome_moutainAndPlane > clarrfy_biome)
        {
            //float soilMin = Mathf.Clamp(20, 50, Mathf.Clamp(0,1,(biome_moutainAndPlane- clarrfy_biome) / biome_moutainAndPlane));
            //float soilMax = Mathf.Clamp(20, 50, Mathf.Clamp(0, 1, (biome_moutainAndPlane - clarrfy_biome) / biome_moutainAndPlane));


            float noise2d_1 = Mathf.Lerp((float)20, (float)50, Mathf.PerlinNoise((float)_x * 0.02f + myposition.x * 0.02f, (float)_z * 0.02f + myposition.z * 0.02f));
            float noise2d_2 = Mathf.Lerp((float)20, (float)50, Mathf.PerlinNoise((float)_x * 0.004f + myposition.x * 0.004f, (float)_z * 0.004f + myposition.z * 0.004f));;
            //float noise2d_3 = Mathf.Lerp((float)(20), (float)50, Mathf.PerlinNoise((float)_x * 0.1f + _x * 0.1f, (float)_z * 0.15f + _z * 0.15f));
            float noiseHigh = noise2d_1 * 0.6f + noise2d_2 * 0.4f;

            return noiseHigh;


        }
        else
        {
            //float soilMin = Mathf.Clamp(10, 64, Mathf.Clamp(0, 1, (clarrfy_biome) / biome_moutainAndPlane));
            //float soilMax = Mathf.Clamp(10, 64, Mathf.Clamp(0, 1, (clarrfy_biome) / biome_moutainAndPlane));
            //float soilMax = Mathf.Clamp(soil_min, soil_max, Mathf.Clamp(0, 1, clarrfy_biome / biome_moutainAndPlane));

            //float smooth = Mathf.Clamp(0.02f, 0.05f , Mathf.Clamp(0, 1, clarrfy_biome / biome_moutainAndPlane));
            //float steep = Mathf.Clamp(0.004f, 0.05f, Mathf.Clamp(0, 1, clarrfy_biome / biome_moutainAndPlane));

            float noise2d_1 = Mathf.Lerp((float)20, (float)64, Mathf.PerlinNoise((float)_x * 0.05f + myposition.x * 0.05f, (float)_z * 0.05f + myposition.z * 0.05f));
            float noise2d_2 = Mathf.Lerp((float)20, (float)64, Mathf.PerlinNoise((float)_x * 0.05f + myposition.x * 0.05f, (float)_z * 0.05f + myposition.z * 0.05f));
            //float noise2d_3 = Mathf.Lerp((float)(soil_min), (float)soil_max, Mathf.PerlinNoise((float)_x * 0.1f + _x * 0.1f, (float)_z * 0.15f + _z * 0.15f));
            float noiseHigh = noise2d_1 * 0.6f + noise2d_2 * 0.4f;

            return noiseHigh;


        }

    }

    float GetTotalNoiseHigh_3(int _x, int _z)
    {
        //平原:20   50   0.02   0.004
        //山脉:20   64   0.05   0.05
        

        float biome_moutainAndPlane = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((float)_x * noise2d_scale_smooth + myposition.x * noise2d_scale_smooth, (float)_z * noise2d_scale_smooth + myposition.z * noise2d_scale_smooth));

        float soilmax = Mathf.Lerp(平原最高, 山脉最高, biome_moutainAndPlane);
        float smooth = Mathf.Lerp(平原Smooth, 山脉Smooth, biome_moutainAndPlane);
        float steep = Mathf.Lerp(平原Steep, 山脉Steep, biome_moutainAndPlane);

        float noise2d_1 = Mathf.Lerp((float)20, (float)soilmax, Mathf.PerlinNoise((float)_x * smooth + myposition.x * smooth, (float)_z * smooth + myposition.z * smooth));
        float noise2d_2 = Mathf.Lerp((float)20, (float)soilmax, Mathf.PerlinNoise((float)_x * steep + myposition.x * steep, (float)_z * steep + myposition.z * steep));
        float noiseHigh = noise2d_1 * scale_Smooth + noise2d_2 * scale_Steep;



        return noiseHigh;
    }


    float GetTotalNoiseHigh_4(int _x, int _z)
    {
        float biome_moutainAndPlane = Mathf.Lerp((float)0, (float)1, Mathf.PerlinNoise((float)_x * 0.006f + myposition.x * 0.006f, (float)_z * 0.006f + myposition.z * 0.006f));

        float soilmax = Mathf.Lerp(50, 64, biome_moutainAndPlane);
        float smooth = Mathf.Lerp(0.002f, 0.04f, biome_moutainAndPlane);
        float steep = Mathf.Lerp(0.004f, 0.05f, biome_moutainAndPlane);

        float noise2d_1 = Mathf.Lerp((float)20, (float)soilmax, Mathf.PerlinNoise((float)_x * smooth + myposition.x * smooth, (float)_z * smooth + myposition.z * smooth));
        float noise2d_2 = Mathf.Lerp((float)20, (float)soilmax, Mathf.PerlinNoise((float)_x * steep + myposition.x * steep, (float)_z * steep + myposition.z * steep));
        float noiseHigh = noise2d_1 * 0.7f + noise2d_2 * 0.3f;



        return noiseHigh;
    }


    float GetTotalNoiseHigh_2(int _x, int _z)
    {
        float noise2d_1 = Mathf.Lerp((float)soil_min, (float)soil_max, Mathf.PerlinNoise((float)_x * noise2d_scale_smooth + myposition.x * noise2d_scale_smooth, (float)_z * noise2d_scale_smooth + myposition.z * noise2d_scale_smooth));
        float noise2d_2 = Mathf.Lerp((float)soil_min, (float)soil_max, Mathf.PerlinNoise((float)_x * noise2d_scale_steep + myposition.x * noise2d_scale_steep, (float)_z * noise2d_scale_steep + myposition.z * noise2d_scale_steep));
        //float noise2d_3 = Mathf.Lerp((float)(soil_min), (float)soil_max, Mathf.PerlinNoise((float)_x * 0.1f + _x * 0.1f, (float)_z * 0.15f + _z * 0.15f));
        float noiseHigh = noise2d_1 * scale_Smooth + noise2d_2 * scale_Steep;

        return noiseHigh;
    }

    float GetSmoothNoise_Tree(int _x, int _z)
    {
        float Offset_x = 100f * 5;
        float Offset_z = 100f * 5;

        float smoothNoise = Mathf.Lerp((float)soil_min, (float)soil_max, Mathf.PerlinNoise(((float)_x + myposition.x + Offset_x) * noise2d_scale_smooth, ((float)_z + myposition.z + Offset_z) * noise2d_scale_smooth));

        return smoothNoise;
    }


    private void UpdateTerrain()
    {
        if (meshFilter == null)
            return;

        ClearMesh();

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(Terrain_Width + 1) * (Terrain_Width + 1)];
        Vector2[] uv = new Vector2[vertices.Length];

        for (int z = 0; z <= Terrain_Width; z++)
        {
            for (int x = 0; x <= Terrain_Width; x++)
            {
                vertices[z * (Terrain_Width + 1) + x] = new Vector3(x, Mathf.FloorToInt(GetSmoothNoise_Tree(x,z)), z);
                uv[z * (Terrain_Width + 1) + x] = new Vector2((float)x / Terrain_Width, (float)z / Terrain_Width);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;

        int[] triangles = new int[Terrain_Width * Terrain_Width * 6];
        int index = 0;

        for (int z = 0; z < Terrain_Width; z++)
        {
            for (int x = 0; x < Terrain_Width; x++)
            {
                int topLeft = z * (Terrain_Width + 1) + x;
                int topRight = topLeft + 1;
                int bottomLeft = (z + 1) * (Terrain_Width + 1) + x;
                int bottomRight = bottomLeft + 1;

                triangles[index++] = topLeft;
                triangles[index++] = bottomLeft;
                triangles[index++] = topRight;

                triangles[index++] = topRight;
                triangles[index++] = bottomLeft;
                triangles[index++] = bottomRight;
            }
        }
            
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    private void ClearMesh()
    {
        if (meshFilter.mesh != null)
        {
            meshFilter.mesh.Clear();
        }
    }
}
