using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class debug : MonoBehaviour
{
    void Start()
    {
        // ����һ���µ� Mesh
        Mesh mesh = new Mesh();

        // ���������εĶ���
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0), // ���½�
            new Vector3(0.5f, -0.5f, 0),  // ���½�
            new Vector3(0.5f, 0.5f, 0),   // ���Ͻ�
            new Vector3(-0.5f, 0.5f, 0)   // ���Ͻ�
        };

        // ���������ε������Σ�ÿ����������������������ɣ�
        int[] triangles = new int[]
        {
            0, 2, 1, // ��һ��������
            0, 3, 2  // �ڶ���������
        };

        // ����ÿ���������ɫ�����ȸ�����ͬ
        Color[] colors = new Color[]
        {
            new Color(1.0f, 0.0f, 0.0f, 1.0f), // ���½ǣ���ɫ�����ȸߣ�
            new Color(0.0f, 1.0f, 0.0f, 1.0f), // ���½ǣ���ɫ���е����ȣ�
            new Color(0.0f, 0.0f, 1.0f, 1.0f), // ���Ͻǣ���ɫ�����ȵͣ�
            new Color(1.0f, 1.0f, 1.0f, 1.0f)  // ���Ͻǣ���ɫ��������ȣ�
        };

        // ���� UV ����
        Vector2[] uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        // �����㡢�����κ���ɫ���ݸ��� Mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.uv = uv;

        // ��鲢��ȡ MeshFilter ���
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = mesh;

        // ��鲢��ȡ MeshRenderer ���
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.EnableKeyword("_EMISSION");
    }
}