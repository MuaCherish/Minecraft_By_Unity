using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DevelopModeVoxelChunk : MonoBehaviour
{
    public Material Mymaterial;
    public DevelopModeWorld world;
    public Vector3 myposition;
    public GameObject chunkObject;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public DevelopModeVoxelChunk(Vector3 thisPosition, DevelopModeWorld _world)
    {
        world = _world;

        //Self
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = world.material_VoxelChunk;
        chunkObject.transform.SetParent(world.ChunkPATH.transform);
        chunkObject.transform.position = new Vector3(thisPosition.x * VoxelData.ChunkWidth, 0f, thisPosition.z * VoxelData.ChunkWidth);
        chunkObject.name = "VoxelChunk--" + thisPosition.x + "," + thisPosition.z;
        myposition = chunkObject.transform.position;

        UpdateTerrain();
    }









    private void UpdateTerrain()
    {
        if (meshFilter == null)
            return;

        ClearMesh();

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(VoxelData.ChunkWidth + 1) * (VoxelData.ChunkWidth + 1)];
        Vector2[] uv = new Vector2[vertices.Length];

        for (int z = 0; z <= VoxelData.ChunkWidth; z++)
        {
            for (int x = 0; x <= VoxelData.ChunkWidth; x++)
            {
                vertices[z * (VoxelData.ChunkWidth + 1) + x] = new Vector3(x, Mathf.FloorToInt(world.GetTotalNoiseHigh_Biome(x, z, myposition)), z);
                uv[z * (VoxelData.ChunkWidth + 1) + x] = new Vector2((float)x / VoxelData.ChunkWidth, (float)z / VoxelData.ChunkWidth);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
         
        int[] triangles = new int[VoxelData.ChunkWidth * VoxelData.ChunkWidth * 6];
        int index = 0;

        for (int z = 0; z < VoxelData.ChunkWidth; z++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                int topLeft = z * (VoxelData.ChunkWidth + 1) + x;
                int topRight = topLeft + 1;
                int bottomLeft = (z + 1) * (VoxelData.ChunkWidth + 1) + x;
                int bottomRight = bottomLeft + 1;

                triangles[index++] = topLeft;
                triangles[index++] = bottomLeft;
                triangles[index++] = topRight;

                triangles[index++] = topRight;
                triangles[index++] = bottomLeft;
                triangles[index++] = bottomRight;
            }
        }

        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    private void ClearMesh()
    {
        if (meshFilter.mesh != null)
        {
            meshFilter.mesh.Clear();
        }
    }



    public void Destroyself()
    {
        Destroy(chunkObject);
    }
}
