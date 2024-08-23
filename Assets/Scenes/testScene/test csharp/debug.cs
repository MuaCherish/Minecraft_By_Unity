using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class debug : MonoBehaviour
{
    public Texture texture;
    [Range(0, 1)] public float lightness = 1.0f;

    private Mesh mesh;
    private Material material;

    void Start()
    {
        // 创建Mesh
        mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0), // 左下角
            new Vector3(0.5f, -0.5f, 0),  // 右下角
            new Vector3(0.5f, 0.5f, 0),   // 右上角
            new Vector3(-0.5f, 0.5f, 0)   // 左上角
        };

        mesh.triangles = new int[]
        {
            0, 1, 2,
            2, 3, 0
        };

        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0), // 左下角
            new Vector2(1, 0), // 右下角
            new Vector2(1, 1), // 右上角
            new Vector2(0, 1)  // 左上角
        };

        // 初始化颜色
        mesh.colors = new Color[4];

        GetComponent<MeshFilter>().mesh = mesh;

        // 创建Material
        material = new Material(Shader.Find("Standard"));
        material.mainTexture = texture;
        GetComponent<MeshRenderer>().material = material;

        UpdateMeshColors();
    }

    void Update()
    {
        UpdateMeshColors();
    }

    void UpdateMeshColors()
    {
        // 计算颜色，从白色到黑色根据lightness的值变化
        Color vertexColor = Color.Lerp(Color.white, Color.black, lightness);

        // 更新每个顶点的颜色
        Color[] colors = new Color[]
        {
            vertexColor,
            vertexColor,
            vertexColor,
            vertexColor
        };

        mesh.colors = colors;
    }
}