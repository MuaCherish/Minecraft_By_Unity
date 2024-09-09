using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterTest : MonoBehaviour
{
    public Material material1;
    public Material material2;

    void Start()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // 定义顶点
        Vector3[] vertices = new Vector3[]
        {
            // 前面顶点（Z-方向）
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
            // 背面顶点（Z+方向）
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0)
        };

        // 定义前面的三角形（Z-方向）
        int[] triangles = new int[]
        {
            0, 2, 1,
            2, 3, 1
        };

        // 定义背面的三角形（Z+方向）
        int[] triangles_2 = new int[]
        {
            4, 5, 6,
            5, 7, 6
        };

        // 定义UV坐标
        Vector2[] uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        mesh.vertices = vertices;
        mesh.uv = uv;

        // 使用两个材质
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.materials = new Material[] { material1, material2 };

        // 为每个面分配材质
        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles, 0); // 第一个子网格使用triangles数组
        mesh.SetTriangles(triangles_2, 1); // 第二个子网格使用triangles_2数组
    }
}
