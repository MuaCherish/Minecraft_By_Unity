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
        // ����Mesh
        mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0), // ���½�
            new Vector3(0.5f, -0.5f, 0),  // ���½�
            new Vector3(0.5f, 0.5f, 0),   // ���Ͻ�
            new Vector3(-0.5f, 0.5f, 0)   // ���Ͻ�
        };

        mesh.triangles = new int[]
        {
            0, 1, 2,
            2, 3, 0
        };

        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0), // ���½�
            new Vector2(1, 0), // ���½�
            new Vector2(1, 1), // ���Ͻ�
            new Vector2(0, 1)  // ���Ͻ�
        };

        // ��ʼ����ɫ
        mesh.colors = new Color[4];

        GetComponent<MeshFilter>().mesh = mesh;

        // ����Material
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
        // ������ɫ���Ӱ�ɫ����ɫ����lightness��ֵ�仯
        Color vertexColor = Color.Lerp(Color.white, Color.black, lightness);

        // ����ÿ���������ɫ
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