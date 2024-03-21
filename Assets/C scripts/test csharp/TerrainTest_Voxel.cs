using UnityEngine;
using UnityEngine.UIElements;

public class TerrainTest_Voxel : MonoBehaviour
{
    public Material Mymaterial;

    public int Terrain_Width; private int _Terrain_Width;

    public float soil_min; private float _soil_min;
    public float soil_max; private float _soil_max;

    public float noise2d_scale_smooth; private float _noise2d_scale_smooth;
    public float noise2d_scale_steep; private float _noise2d_scale_steep;

    private Vector3 _position;

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

    //ֵ�ı�ʱ����
    public bool isValueChange()
    {
        if (_Terrain_Width != Terrain_Width ||
            _soil_min != soil_min ||
            _soil_max != soil_max ||
            _noise2d_scale_smooth != noise2d_scale_smooth ||
            _noise2d_scale_steep != noise2d_scale_steep ||
            _position != transform.position
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
        _position = transform.position;
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
                float noise2d_1 = Mathf.Lerp((float)soil_min, (float)soil_max, Mathf.PerlinNoise((float)x * noise2d_scale_smooth + this.transform.position.x * noise2d_scale_smooth, (float)z * noise2d_scale_smooth + this.transform.position.z * noise2d_scale_smooth));
                float noise2d_2 = Mathf.Lerp((float)(soil_min), (float)soil_max, Mathf.PerlinNoise((float)x * noise2d_scale_steep + this.transform.position.x * noise2d_scale_steep, (float)z * noise2d_scale_steep + this.transform.position.z * noise2d_scale_steep));
                float noise2d_3 = Mathf.Lerp((float)(soil_min), (float)soil_max, Mathf.PerlinNoise((float)x * 0.1f + x * 0.1f, (float)z * 0.15f + z * 0.15f));
                float noiseHigh = noise2d_1 * 0.6f + noise2d_2 * 0.4f + noise2d_3 * 0.05f;

                vertices[z * (Terrain_Width + 1) + x] = new Vector3(x, Mathf.FloorToInt(noiseHigh), z);
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