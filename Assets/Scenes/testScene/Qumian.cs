using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Qumian : MonoBehaviour
{
    public int subdivisions = 1;  // 细分数量
    public float radius = 1f;     // 圆柱面的半径
    public Material material;     // 材质

    private int previousSubdivisions;
    private float previousRadius;

    void Start()
    {
        CreateMesh();
    }

    void OnValidate()
    {
        // 当public变量改变时刷新mesh
        if (subdivisions != previousSubdivisions || radius != previousRadius)
        {
            CreateMesh();
            previousSubdivisions = subdivisions;
            previousRadius = radius;
        }

        if (material != null)
        {
            GetComponent<MeshRenderer>().material = material;
        }
    }

    void CreateMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        int vertCount = (subdivisions + 1) * (subdivisions + 1);
        int triCount = subdivisions * subdivisions * 6;

        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uv = new Vector2[vertCount];
        int[] triangles = new int[triCount];

        float step = 1.0f / subdivisions;
        int vertIndex = 0;
        int triIndex = 0;

        for (int y = 0; y <= subdivisions; y++)
        {
            for (int x = 0; x <= subdivisions; x++)
            {
                // 计算平面上的点
                float u = x * step - 0.5f;
                float v = y * step;

                // 将平面点映射到半圆柱面上，弯曲朝向内部
                float angle = u * Mathf.PI;  // 半圆柱的内部
                float xPos = radius * Mathf.Sin(angle);  // X 轴弯曲
                float zPos = radius * Mathf.Cos(angle);  // Z 轴深度
                float yPos = v;  // Y 轴保持平直

                vertices[vertIndex] = new Vector3(xPos, yPos, zPos);
                uv[vertIndex] = new Vector2((float)x / subdivisions, (float)y / subdivisions);

                if (x < subdivisions && y < subdivisions)
                {
                    triangles[triIndex] = vertIndex;
                    triangles[triIndex + 1] = vertIndex + subdivisions + 1;
                    triangles[triIndex + 2] = vertIndex + 1;

                    triangles[triIndex + 3] = vertIndex + 1;
                    triangles[triIndex + 4] = vertIndex + subdivisions + 1;
                    triangles[triIndex + 5] = vertIndex + subdivisions + 2;

                    triIndex += 6;
                }

                vertIndex++;
            } 
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshRenderer>().material = material;
    }
}
