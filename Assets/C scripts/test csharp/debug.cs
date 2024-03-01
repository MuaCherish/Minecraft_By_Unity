using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class debug : MonoBehaviour
{
    public Material material1;
    public Material material2;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Start()
    {
        GameObject obj = new GameObject();
        meshFilter = obj.AddComponent<MeshFilter>();
        meshRenderer = obj.AddComponent<MeshRenderer>();

        // ���㶨��
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0)
        };

        // �������������������
        int[] trianglesFront = new int[] { 0, 1, 2, 2, 1, 3 };
        // ���÷�������������У�ע��˳��
        int[] trianglesBack = new int[] { 2, 1, 0, 3, 1, 2 };

        // ���� UV
        Vector2[] uvs = new Vector2[] 
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 1)
        };

        // ���ò���
        Material[] materials = new Material[] { material1, material2 };

        // ���� Mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.subMeshCount = 2;
        mesh.SetTriangles(trianglesFront, 0);
        mesh.SetTriangles(trianglesBack, 1);
        mesh.uv = uvs; // ���� UV
        mesh.SetUVs(0,uvs);

        // Ӧ�� Mesh �Ͳ���
        meshFilter.mesh = mesh;
        meshRenderer.sharedMaterials = materials;
    }
}