using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace NoiseMapTest
{

    public static class GenMesh
    {
        public static Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();

            // 顶点
            Vector3[] vertices = {
                new Vector3(-0.5f, 0, -0.5f), // 左下
                new Vector3(0.5f, 0, -0.5f),  // 右下
                new Vector3(-0.5f, 0, 0.5f),  // 左上
                new Vector3(0.5f, 0, 0.5f)    // 右上
            };

            // 三角形索引
            int[] triangles = { 0, 2, 1, 1, 2, 3 };

            // UV 坐标
            Vector2[] uv = {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();

            return mesh;
        }
    }


}


