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

            // ����
            Vector3[] vertices = {
                new Vector3(-0.5f, 0, -0.5f), // ����
                new Vector3(0.5f, 0, -0.5f),  // ����
                new Vector3(-0.5f, 0, 0.5f),  // ����
                new Vector3(0.5f, 0, 0.5f)    // ����
            };

            // ����������
            int[] triangles = { 0, 2, 1, 1, 2, 3 };

            // UV ����
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


