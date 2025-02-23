using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class aaa : MonoBehaviour
{
    // ����������������������ȣ�0��1֮�䣩
    [Range(0, 1)]
    public float brightness = 1.0f;

    private Mesh mesh;

    void Start()
    {
        CreateMesh();
    }

    // ��������������
    void CreateMesh()
    {
        // ����һ���µ�Mesh����
        mesh = new Mesh();
        mesh.name = "QuadMesh";

        // ���嶥��
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0f), // ���½�
            new Vector3(0.5f, -0.5f, 0f),  // ���½�
            new Vector3(-0.5f, 0.5f, 0f),  // ���Ͻ�
            new Vector3(0.5f, 0.5f, 0f)    // ���Ͻ�
        };

        // ����UV����
        Vector2[] uv = new Vector2[]
        {
            new Vector2(0f, 0f), // ���½�
            new Vector2(1f, 0f), // ���½�
            new Vector2(0f, 1f), // ���Ͻ�
            new Vector2(1f, 1f)  // ���Ͻ�
        };

        // ����������
        int[] triangles = new int[]
        {
            0, 2, 1, // ��һ��������
            2, 3, 1  // �ڶ���������
        };

        // ����Ҷ���ɫ����������ֵ��������ɫ
        Color color = new Color(brightness, brightness, brightness, 1f);
        Color[] colors = new Color[]
        {
            color, // ���½�
            color, // ���½�
            color, // ���Ͻ�
            color  // ���Ͻ�
        };

        // �����㡢UV�������κ���ɫ��ֵ��mesh
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.colors = colors;

        // ���¼��㷨�ߣ���ȷ���泯Z+��
        mesh.RecalculateNormals();

        // ��mesh��ֵ��MeshFilter
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    // �ڱ༭�����޸�ֵʱ�Զ���������
    void OnValidate()
    {
        // ȷ�������ڱ༭���и���ʱ��ʱ����
        if (mesh != null)
        {
            // �����µ���ɫֵ
            Color color = new Color(brightness, brightness, brightness, 1f);
            Color[] colors = new Color[]
            {
                color, // ���½�
                color, // ���½�
                color, // ���Ͻ�
                color  // ���Ͻ�
            };

            // ����������ɫ
            mesh.colors = colors;
        }
        else
        {
            // ������񲻴��ڣ����´�������
            CreateMesh();
        }
    }
}
