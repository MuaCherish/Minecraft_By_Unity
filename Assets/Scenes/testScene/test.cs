using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public Material material; // ���ڽ��յĲ���

    void Start()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),   // ���½�
            new Vector3(2, 0, 0),   // ���½�
            new Vector3(0, 0, 1),   // ���Ͻ�
            new Vector3(2, 0, 1)    // ���Ͻ�
        };
        mesh.triangles = new int[]
        {
            0, 2, 1, // ��һ��������
            2, 3, 1  // �ڶ���������
        };

        // ���� Mesh �� UV ���꣬������ͼ�ظ�
        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(2, 0),
            new Vector2(0, 1),
            new Vector2(2, 1)
        };

        mesh.RecalculateNormals();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
        meshRenderer.material = material;
    }
}
