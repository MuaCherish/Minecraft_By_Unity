using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class debug : MonoBehaviour
{
    void Start()
    {
        // 创建一个新的 Mesh
        Mesh mesh = new Mesh();

        // 定义正方形的顶点
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0), // 左下角
            new Vector3(0.5f, -0.5f, 0),  // 右下角
            new Vector3(0.5f, 0.5f, 0),   // 右上角
            new Vector3(-0.5f, 0.5f, 0)   // 左上角
        };

        // 定义正方形的三角形（每个正方形由两个三角形组成）
        int[] triangles = new int[]
        {
            0, 2, 1, // 第一个三角形
            0, 3, 2  // 第二个三角形
        };

        // 设置每个顶点的颜色，亮度各不相同
        Color[] colors = new Color[]
        {
            new Color(1.0f, 0.0f, 0.0f, 1.0f), // 左下角（红色，亮度高）
            new Color(0.0f, 1.0f, 0.0f, 1.0f), // 右下角（绿色，中等亮度）
            new Color(0.0f, 0.0f, 1.0f, 1.0f), // 右上角（蓝色，亮度低）
            new Color(1.0f, 1.0f, 1.0f, 1.0f)  // 左上角（白色，最高亮度）
        };

        // 设置 UV 坐标
        Vector2[] uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        // 将顶点、三角形和颜色数据赋给 Mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.uv = uv;

        // 检查并获取 MeshFilter 组件
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = mesh;

        // 检查并获取 MeshRenderer 组件
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.EnableKeyword("_EMISSION");
    }
}