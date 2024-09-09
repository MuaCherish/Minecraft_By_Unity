using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VerticalColor : MonoBehaviour
{
    void Start()
    {
        // ����һ���µ�Mesh
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // ���������εĶ���
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-0.5f, -0.5f, 0);
        vertices[1] = new Vector3(0.5f, -0.5f, 0);
        vertices[2] = new Vector3(-0.5f, 0.5f, 0);
        vertices[3] = new Vector3(0.5f, 0.5f, 0);

        // ���������ε�������
        int[] triangles = new int[6];
        // ��һ��������
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        // �ڶ���������
        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 1;

        // ����ÿ���������ɫ
        Color[] colors = new Color[4];
        colors[0] = Color.red;    // ���½�
        colors[1] = Color.green;  // ���½�
        colors[2] = Color.blue;   // ���Ͻ�
        colors[3] = Color.yellow; // ���Ͻ�

        // �����㡢�����κ���ɫ��ֵ��Mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        // ����Mesh
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}

