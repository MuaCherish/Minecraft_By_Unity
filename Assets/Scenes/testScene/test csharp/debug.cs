using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class debug : MonoBehaviour
{
    public Material material1;
    public Material material2;

    void Start()
    {
        // 创建两个 GameObject 来放置两个正方形
        GameObject square1 = CreateSquareMesh(material1);
        GameObject square2 = CreateSquareMesh(material2);

        // 将第二个正方形的位置向右移动
        square2.transform.position = new Vector3(2.5f, 0, 0);
    }

    GameObject CreateSquareMesh(Material material)
    {
        // 创建一个新的 GameObject
        GameObject square = new GameObject("Square");

        // 添加 MeshFilter 和 MeshRenderer 组件
        MeshFilter meshFilter = square.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = square.AddComponent<MeshRenderer>();

        // 创建 Mesh
        Mesh mesh = new Mesh();
        Vector3[] vertices = {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0)
        };

        int[] triangles = {
            0, 1, 2,
            2, 3, 0
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // 设置 Mesh 到 MeshFilter
        meshFilter.mesh = mesh;

        // 设置材质到 MeshRenderer
        meshRenderer.material = material;

        return square;
    }
}