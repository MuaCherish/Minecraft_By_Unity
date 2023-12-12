using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    //网格过滤器
    public MeshFilter meshFilter;
    //网格渲染器
    public MeshRenderer meshRenderer;
    //
    public int testvaties;

    void Start()
    {
        //顶点的索引
        int vertexIndex = 0;
        //三角形下标
        int triangleIndex = 0;

        //顶点数组
        List<Vector3> vertices = new List<Vector3>();
        //三角形数组
        List<int> triangles = new List<int>();
        //UV数组
        List<Vector2> uvs = new List<Vector2>();

        //遍历绘制序列
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                triangleIndex = VoxelData.voxelTris[i, j];

                vertices.Add(VoxelData.voxelVerts[triangleIndex]);
                triangles.Add(vertexIndex);
                uvs.Add(VoxelData.voxelUvs[j]);

                vertexIndex++;

            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

    }

}
