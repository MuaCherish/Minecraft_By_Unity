using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3 (0.0f, 0.0f, 0.0f),
        new Vector3 (1.0f, 0.0f, 0.0f),
        new Vector3 (1.0f, 1.0f, 0.0f),
        new Vector3 (0.0f, 1.0f, 0.0f),
        new Vector3 (0.0f, 0.0f, 1.0f),
        new Vector3 (1.0f, 0.0f, 1.0f),
        new Vector3 (1.0f, 1.0f, 1.0f),
        new Vector3 (0.0f, 1.0f, 1.0f),
    };

    public static readonly int[,] voxelTris = new int[6, 6];
}
