using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class debug : MonoBehaviour
{
    public Material material1;
    public Material material2;

    void Start()
    {
        // �������� GameObject ����������������
        GameObject square1 = CreateSquareMesh(material1);
        GameObject square2 = CreateSquareMesh(material2);

        // ���ڶ��������ε�λ�������ƶ�
        square2.transform.position = new Vector3(2.5f, 0, 0);
    }

    GameObject CreateSquareMesh(Material material)
    {
        // ����һ���µ� GameObject
        GameObject square = new GameObject("Square");

        // ��� MeshFilter �� MeshRenderer ���
        MeshFilter meshFilter = square.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = square.AddComponent<MeshRenderer>();

        // ���� Mesh
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

        // ���� Mesh �� MeshFilter
        meshFilter.mesh = mesh;

        // ���ò��ʵ� MeshRenderer
        meshRenderer.material = material;

        return square;
    }
}