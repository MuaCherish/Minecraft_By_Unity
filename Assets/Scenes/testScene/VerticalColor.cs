using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VerticalColor : MonoBehaviour
{
    void Start()
    {
        // 创建一个新的Mesh
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // 定义正方形的顶点
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-0.5f, -0.5f, 0);
        vertices[1] = new Vector3(0.5f, -0.5f, 0);
        vertices[2] = new Vector3(-0.5f, 0.5f, 0);
        vertices[3] = new Vector3(0.5f, 0.5f, 0);

        // 定义正方形的三角形
        int[] triangles = new int[6];
        // 第一个三角形
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        // 第二个三角形
        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 1;

        // 定义每个顶点的颜色
        Color[] colors = new Color[4];
        colors[0] = Color.red;    // 左下角
        colors[1] = Color.green;  // 右下角
        colors[2] = Color.blue;   // 左上角
        colors[3] = Color.yellow; // 右上角

        // 将顶点、三角形和颜色赋值给Mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        // 更新Mesh
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}

