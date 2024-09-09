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

        // ���嶥��
        Vector3[] vertices = new Vector3[]
        {
            // ǰ�涥�㣨Z-����
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
            // ���涥�㣨Z+����
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0)
        };

        // ����ǰ��������Σ�Z-����
        int[] triangles = new int[]
        {
            0, 2, 1,
            2, 3, 1
        };

        // ���屳��������Σ�Z+����
        int[] triangles_2 = new int[]
        {
            4, 5, 6,
            5, 7, 6
        };

        // ����UV����
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

        // ʹ����������
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.materials = new Material[] { material1, material2 };

        // Ϊÿ����������
        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles, 0); // ��һ��������ʹ��triangles����
        mesh.SetTriangles(triangles_2, 1); // �ڶ���������ʹ��triangles_2����
    }
}
