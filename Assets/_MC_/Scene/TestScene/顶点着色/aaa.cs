using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class aaa : MonoBehaviour
{
    // 公开变量控制整个面的亮度（0到1之间）
    [Range(0, 1)]
    public float brightness = 1.0f;

    private Mesh mesh;

    void Start()
    {
        CreateMesh();
    }

    // 创建并更新网格
    void CreateMesh()
    {
        // 创建一个新的Mesh对象
        mesh = new Mesh();
        mesh.name = "QuadMesh";

        // 定义顶点
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0f), // 左下角
            new Vector3(0.5f, -0.5f, 0f),  // 右下角
            new Vector3(-0.5f, 0.5f, 0f),  // 左上角
            new Vector3(0.5f, 0.5f, 0f)    // 右上角
        };

        // 定义UV坐标
        Vector2[] uv = new Vector2[]
        {
            new Vector2(0f, 0f), // 左下角
            new Vector2(1f, 0f), // 右下角
            new Vector2(0f, 1f), // 左上角
            new Vector2(1f, 1f)  // 右上角
        };

        // 定义三角形
        int[] triangles = new int[]
        {
            0, 2, 1, // 第一个三角形
            2, 3, 1  // 第二个三角形
        };

        // 计算灰度颜色，根据亮度值来设置颜色
        Color color = new Color(brightness, brightness, brightness, 1f);
        Color[] colors = new Color[]
        {
            color, // 左下角
            color, // 右下角
            color, // 左上角
            color  // 右上角
        };

        // 将顶点、UV、三角形和颜色赋值给mesh
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.colors = colors;

        // 重新计算法线（以确保面朝Z+）
        mesh.RecalculateNormals();

        // 将mesh赋值给MeshFilter
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    // 在编辑器中修改值时自动更新网格
    void OnValidate()
    {
        // 确保网格在编辑器中更改时及时更新
        if (mesh != null)
        {
            // 计算新的颜色值
            Color color = new Color(brightness, brightness, brightness, 1f);
            Color[] colors = new Color[]
            {
                color, // 左下角
                color, // 右下角
                color, // 左上角
                color  // 右上角
            };

            // 重新设置颜色
            mesh.colors = colors;
        }
        else
        {
            // 如果网格不存在，重新创建网格
            CreateMesh();
        }
    }
}
