using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public Material material; // 用于接收的材质

    void Start()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),   // 左下角
            new Vector3(2, 0, 0),   // 右下角
            new Vector3(0, 0, 1),   // 左上角
            new Vector3(2, 0, 1)    // 右上角
        };
        mesh.triangles = new int[]
        {
            0, 2, 1, // 第一个三角形
            2, 3, 1  // 第二个三角形
        };

        // 设置 Mesh 的 UV 坐标，控制贴图重复
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
